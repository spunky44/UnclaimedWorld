using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Items;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Systems.Triggers;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Entities.Locomotors.Stances;
using UWGame.SimSide.Entities.Containers.Components;
namespace UWGame.SimSide.AI.Goals
{

   
    /// <summary>
    /// Patrol + Attack!
    /// when in Patrol mode, the job is never removed.
    /// 
    /// In Attack mode, the job is removed after an agent has been in the zone for a while when there are no threats nearby
    /// </summary>
    class GoalPatrol : CompositeGoal, ITopLevelGoal
    {
        public enum CombatAreaMode { Patrol, Attack }

        private Vector3 location;
        private CombatAreaJob /* PatrolJob*/ job;
        JobID snapshotJob;


        private EntityAndRoot? weapon;
        Dictionary<EntityAndRoot, List<ReplenishItemsForAction>> replenishItemsForTools;
       // Dictionary<EntityID, List<ReplenishItemsForAction>> replenishItemsForTools;

        public double TimeSpentInTopLevelGoal { get; set; }

        /// <summary>
        /// If the area is small this will be set to true and the agent will not do goalsearch area but instead goal wait or something
        /// like that.
        /// </summary>
        private bool hasSmallAreaToPatrol = false;


        public GoalPatrol(Entity owner, CombatAreaJob job, List<EntityGroupID> ownersVehicles, EntityAndRoot? weaponToUse,
            Dictionary<EntityAndRoot, List<ReplenishItemsForAction>> replenishItemsForTools, Vector3 locationInSearchArea)
            : base(owner)
        {
            this.ownersOfVehicles = ownersVehicles;
            this.job = job;
            location = locationInSearchArea;
            this.replenishItemsForTools = replenishItemsForTools;
            weapon = weaponToUse;
        }



        public GoalPatrol()
        {
        }


        protected override void Activate()
        {
            sampleDistance = 100f;
            Status = Status.Active;

            RemoveAllSubgoals();

            // LOCKS
            job.TakeJob(entity);

            SetLocksOnReplenishItems(job, replenishItemsForTools);

            ////

            if (job.RequiresBoldStance) // always true
            {
                entity.Intelligence.SetBoldStance();
            }

            if (!GatherToolsOrWeapons(weapon, ownersOfVehicles, job, StorageCompartment.Haul))
            {
                Status = Goals.Status.Failed;
                return;
            }

            List<ItemType.TaskType> gearTasks = null;
            AddNightActivityGear(ref gearTasks);

            FindOptionalEquipmentIfNeeded(location, job, false, true, taskTypesForGear: gearTasks); // get extra food if needed


            /*
             for tiny zones: don't move, stand in one place and change the staus text to "Guarding"
             */

            ReplenishToolsOrWeapons(replenishItemsForTools, ownersOfVehicles, job); // TODO - get ammo  
            AddSubgoal(new GoalMoveToPosition(entity, location, ownersOfVehicles));

            //Depending on area size we want this job to search an area or stand still
            //Larger area we should search it more offten else we could move a little, wait, move a little
            //Perhaps increase the duration the agent stands still the longer the job is going on so he is
            //more active at the start of the job
            //?

          //  AddSubgoal(new GoalSearchArea(entity, job.Zone, ownersOfVehicles, false, false));
            if (job.Zone.MapArea.GetTileLocations().Count <= 4)
            {
                hasSmallAreaToPatrol = true;
            }
         
        }

        public override bool RequiresBoldStance()
        {
            return true;
        }



        protected void ReplenishWeapon()
        {
            // now replenish if needed (the evaluator has already determined that it is possible with this weapon):
            /*if (replenishActions != null)
            {
                foreach (var replenishAction in replenishActions)
                {
                    AddSubgoal(new GoalReplenish(entity, weapon.Value, replenishAction.Item2,
                        replenishAction.Item1, ownersOfVehicles, job));
                }
            }*/
        }
        protected override bool ArePreconditionsOK()
        {
            IKnownEntityData weaponData;
            if (!IsToolOrWeaponOK(weapon, out weaponData))
                return false;

            return true;
        }




        public override string GetStatus()
        {
            if (hasSmallAreaToPatrol)
            {
                return "Guarding area";
            }
            return "Patrolling area"; // "Guarding??"
        }

        public override Goal.DetectionFactor GetDetectAgentsFactor(EntityType typeOfAgent, bool requiresExamineAction)
        {
            if (!requiresExamineAction)
            {
                return DetectionFactor.DetectVeryGood;
            }
            else return DetectionFactor.CannotDetect;
        }


        public override Goal.DetectionFactor GetDetectResourcesFactor(ResourceType resourceType, bool requiresExamineAction)
        {
            if (!requiresExamineAction)
            {
                return DetectionFactor.DetectSome;
            }
            else return DetectionFactor.CannotDetect;
        }

        public override bool IsSame(Jobs.Job job)
        {
            return job == this.job;
        }

      /*  bool IsPatrollingForVermin()
        {
            return job.AttackVermin;
        }*/


        private bool CanHoldSlotWhileVacating()
        {
            return hasReachedArea 
                || TimeSpentInTopLevelGoal > 6; // for attacking further away. If we are fighting en-route, or close to the area, keep the splot so no more people will take it.
        }


        private bool hasReachedArea = false;

        protected override void ProcessWhileActive(GameTime elapsed)
        {

            if (!preconditionsRegulator.IsReady() ||
                ArePreconditionsOK())
            {              

                Status = ProcessSubgoals(elapsed);

                if (CanHoldSlotWhileVacating()) // hasReachedArea)
                {
                    // once we reach the area,
                    // set the timepoint every update (TraverseWaypoint may arbitrate and pick an attackgoal that needs it)
                    entityIntelligence.SetLastAgentOnPost(job);
                }               
            }
            else
            {
                entityIntelligence.ResetLastAgentOnPost(job);
                Status = Status.Failed;
            }

            if (Status == Goals.Status.Completed) // subgoals should never Complete! the agent only leaves the goal when sleeping/eating...
            // but the job should never be destroyed - other agents should then take over
            {
                // we come here after reaching the zone for the first time, and after every GoalSearchArea round.
                // start searching again.
                AttackAreaJob attackAreaJob = job as AttackAreaJob;
                if (!hasReachedArea)
                {
                    // for Attacks, start the clock ticking for when the job will be destroyed:
                    // only the first agent should do this.
                    
                    if (attackAreaJob != null)
                    {
                        attackAreaJob.SetAreaReached();
                    }

                    hasReachedArea = true; // Here we have already searched the area once - GoalSearchArea has completed!
                }
                else
                {
                    // the agent(s) can determine if its time to end:
                    if (attackAreaJob != null)
                    {
                        if (attackAreaJob.IsDurationReached())
                        {
                            if (!attackAreaJob.AreaContainsThreats())
                            {
                                attackAreaJob.Destroy(true, entity);
                                Status = Goals.Status.Completed;
                                return;
                            }
                        }
                    }
                }


                //a Patrol job is never destroyed, except by the player/ AI planner
                //Set the status to active again:
                Status = Goals.Status.Active;

                ITopLevelGoal newGoal;
                // arbitrate should not see this goal as completed; it will result in a 0 current score.
                if (entityIntelligence.Brain.ArbitrateWhileBusy(out newGoal, true))
                {
                    if (newGoal is GoalAttack)
                    {
                        entityIntelligence.SetLastAgentOnPost(job);
                    }
                    else
                    {
                        entityIntelligence.ResetLastAgentOnPost(job);
                    }

                    // we have a new, better goal. The goal has substituted all on the stack.
                    HandleSubstitutedGoalByArbitrator();
                }
                else
                {
                    if (hasSmallAreaToPatrol)
                    {
                        //Add goal turn to face random location?

                        //This is similar code to what exists in  goalsearcharea
                        CheckToStopAndLookAround();
                    }
                    else
                    {
                        //Increase the duration when the agent idles 
                        sampleDistance *= 1.5f;
                        AddSubgoal(new GoalSearchArea(entity, job.Zone, ownersOfVehicles, false, false));
                    }

                }
            }
        }

        

       /* private void SetLastAgentOnPost()
        {
            entityIntelligence.Memory.SetRecentlyOnPatrol(job.ID); // make sure we can pursue vermin targets for a while...
            job.SetLastAgentOnPost(entity.ID);  // hopefully, we can pick up this job again...?
        }

        private void ResetLastAgentOnPost()
        {
            entityIntelligence.Memory.SetRecentlyOnPatrol(null); // reset
            job.RemoveLastAgentOnPost(entity.ID);
        }*/


        private float sampleDistance = 100f;
        //This was a copy of CheckToStopAndLookAround in searcharea

        private void CheckToStopAndLookAround()
        {
            // have a chance to stop and look around in this spot (the chance is greater the longer we have to turn to move to the next waypoint)
            //  Vector2 nextWaypointVector = closestLocation - entity.Location.ToVector2();
            //nextWaypointVector.Normalize();

            float timeToWaitWhenStoppedAndSearchingMean, timeToWaitWhenStoppedAndSearchingStdDev;

            timeToWaitWhenStoppedAndSearchingMean = GameData.Instance.AIConstants.TimeToWaitWhenStoppedAndSearchingMean;
            timeToWaitWhenStoppedAndSearchingStdDev = GameData.Instance.AIConstants.TimeToWaitWhenStoppedAndSearchingStdDev;


            //double timeToWait = 1.0 + 3.0 * Globals.Instance.RandomPredictable.NextDouble();
            double timeToWait = The.Sim.GameplayRandomGenerator.RandomNormalDistribution(timeToWaitWhenStoppedAndSearchingMean, timeToWaitWhenStoppedAndSearchingStdDev);
            // timeToWait = Common.Max(timeToWait, 0.5d);
            if (timeToWait > 0.5d) // wait for a minimum of 0.5d second
            {
                if (entity.HasStance())
                {
                    StanceType stanceToTake;
                    if (timeToWait > 2.0 && The.Sim.GameplayRandomGenerator.NextDouble("GoalPatrol") > 0.4f)
                    {
                        // don't kneel when very brief wait
                        stanceToTake = entity.Locomotor.Stance.PickRandomStance(entity.EntityType.LocomotorType.StancesType.PatrolStancesLongerWait, entity.EntityType.LocomotorType.StancesType.DefaultStanceType);
                        /* ChangeStance(Entities.Locomotors.LeggedLocomotor.Stance.Kneeling);
                         AddSubgoal(new GoalWait(entity, timeToWait, AnimAction.Scouting, AnimModifier.Kneeling));*/
                    }
                    else
                    {
                        stanceToTake = entity.Locomotor.Stance.PickRandomStance(entity.EntityType.LocomotorType.StancesType.PatrolStancesBriefWait, entity.EntityType.LocomotorType.StancesType.DefaultStanceType);

                        /* ChangeStance(Entities.Locomotors.LeggedLocomotor.Stance.Standing);
                         AddSubgoal(new GoalWait(entity, timeToWait, AnimAction.Scouting));*/
                    }

                    ChangeStance(stanceToTake);
                    if (stanceToTake.AnimModifier.HasValue)
                    {
                        AddSubgoal(new GoalWait(entity, timeToWait, AnimAction.Scouting, stanceToTake.AnimModifier.Value));
                    }
                    else
                    {
                        AddSubgoal(new GoalWait(entity, timeToWait, AnimAction.Scouting));
                    }
                }
                else
                {
                    AddSubgoal(new GoalWait(entity, timeToWait, AnimAction.Scouting));
                }

            }
        }


        public override bool HandleMessage(Message message)
        {
            //first, pass the message down the goal hierarchy
            bool handled = ForwardMessageToFrontMostSubgoal(message);

            //if the msg was not handled, test to see if this goal can handle it
            if (handled == false)
            {
                switch (message.MessageType)
                {
                    // someone wants us to stop doing this job:
                    case Message.MessageTypes.CancelJobOrItemInUse:
                    case Message.MessageTypes.CancelJobForAIReset:

                        if (entity.ToString().Contains("August")) //entity.EntityID == (EntityID)4337)
                        {

                        }

                        Status = Status.Failed;

                        entityIntelligence.Memory.SetRecentlyOnPatrol(null); // reset

                        job.Abandon(entity);

                        return true; //msg handled

                    default: return false;
                }
            }
            else
            {
                return true;
            }
        }


        /// <summary>
        /// Prevents the first agent to evaluate a new threat from taking our weapon (the threat may be in front of us)
        /// 
        /// when carrying the weapon, we are no longer negotiating, so we will never give it up.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override bool CanDropRequestedItem(Entity item)
        {
            if (item.AssignedToJob == job.ID)
            {
                return false;
            }

            return true;
        }


        public override void Deactivate()
        {
            job.Abandon(entity);


            if (weapon.HasValue)
            {
                RemoveLockOnToolOrWeapon(job.ID, weapon.Value);
            }

            RemoveLocksOnReplenishItems(job.ID, replenishItemsForTools);

            ResetThreatStance(job);


         //   entityIntelligence.AttacksVermin = false;

        }

        public double ScoreGoal()
        {
            ToolParams? toolParams = null;
            if (weapon.HasValue)
            {
                 toolParams = new ToolParams() { Tools = new List<EntityID>() { weapon.Value.Entity }}; 
            }

            return ScoreJobGoal(job, toolParams: toolParams);
        }


        #region ISnapshot

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public override Snapshotter.Version DoVersion(Snapshotter sn)
        {
            base.DoVersion(sn); // each class in the class hierarchy snapshots and maintains their own version.

            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }


        public override ISnapshot DoSnapshot(Snapshotter sn)
        {
            base.DoSnapshot(sn);

            this.location = sn.DoVector3(location);

            if (sn.mode == Snapshotter.Mode.Save )
            { 
                if (job.ID == JobID.Invalid)
                {
                    throw new Exception("Invalid jobID");
                }
            }

            this.snapshotJob = (JobID)sn.SnapshotID<Job, JobID>(job);
            this.weapon = sn.DoEntityAndRootNullable(weapon);
            this.replenishItemsForTools = sn.DoMultiMap(replenishItemsForTools);
            this.TimeSpentInTopLevelGoal = sn.DoDouble(TimeSpentInTopLevelGoal);
            this.hasSmallAreaToPatrol = sn.DoBool(hasSmallAreaToPatrol);
            this.hasReachedArea = sn.DoBool(hasReachedArea);

            sn.Ignore(job);

            return this;
        }

        public override void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            base.LoadPostProcess(sn);

            job = (CombatAreaJob)LookUp<Job, JobID>.FindByID(snapshotJob);
        }

        #endregion
    }


}
