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
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Entities.Containers.Components;
namespace UWGame.SimSide.AI.Goals
{
    class GoalFindPrey : CompositeGoal, ITopLevelGoal
    {
        private Vector3 location;
        
        private FindPreyJob job;
        JobID? snapshotJob;

        public double TimeSpentInTopLevelGoal { get; set; }

       // private EntityType typeToHunt;

        private EntityAndRoot? weapon;
        //Dictionary<EntityID, List<ReplenishItemsForAction>> replenishItemsForTools;
        Dictionary<EntityAndRoot, List<ReplenishItemsForAction>> replenishItemsForTools;
        
        bool hasFoundPrey = false;

        
        // private Vector3 location;

        /*
          EvaluateJob
         * -> returns area to search (job), and weapons to use
         * 
         * GoalFindPrey:
            (pick up weapons, set AssignedToJob = FindPreyJob)
         *  add GoalMoveToPosition(): walk to area normally
         *  add GoalSearchArea(bool searchForPrey): 
         *    Activate() -> if searchForPrey then IsStealthy = true
         *    Process(): GetDistance to find nearest accessible search point - remove inaccessible points
         *      Add GoalWait to wait for distance response
         *      HandleMessage() to receive response
         *    Terminate() - IsStealthy = false
         *    
         */


        public GoalFindPrey(Entity owner, FindPreyJob job, List<EntityGroupID> ownersVehicles, EntityAndRoot? weaponToUse,
           // Dictionary<EntityID, List<ReplenishItemsForAction>> replenishItemsForTools, 
             Dictionary<EntityAndRoot, List<ReplenishItemsForAction>> replenishItemsForTools, 
            Vector3 locationInSearchArea)
            : base(owner)
        {
           
            this.job = job;
            this.ownersOfVehicles = ownersVehicles;
           // this.typeToHunt = job.TypeToHunt;
            location = locationInSearchArea;
            this.replenishItemsForTools = replenishItemsForTools;
            weapon = weaponToUse;

        }


     //   static List<ItemType.TaskType> gearTasks = new ItemType.TaskType[] { ItemType.TaskType.UnspecifiedHunting }; 
        


        public GoalFindPrey()
        {
        }

        protected override void Activate()
        {
            Status = Status.Active;

            RemoveAllSubgoals();

            // wait a bit, to see if we are asked to cancel by other agents.
            AddSubgoal(new GoalWait(entity, GameData.Instance.AIConstants.TimeToWaitBeforeStartingGoal));

            // LOCKS
            job.TakeJob(entity);

            SetLocksOnReplenishItems(job, replenishItemsForTools);

            ////

            if (!GatherToolsOrWeapons(weapon, ownersOfVehicles, job, StorageCompartment.Haul))
            {
                Status = Goals.Status.Failed;
                return;
            }

            ReplenishToolsOrWeapons(replenishItemsForTools, ownersOfVehicles, job); //  get ammo  


            List<ItemType.TaskType> gearTasks = new List<ItemType.TaskType> { ItemType.TaskType.UnspecifiedHunting };
            AddNightActivityGear(ref gearTasks);

            FindOptionalEquipmentIfNeeded(location, job, false, true, taskTypesForGear: gearTasks); // get extra food and gear if needed


            AddSubgoal(new GoalMoveToPosition(entity, location, ownersOfVehicles));
            AddSubgoal(new GoalSearchArea(entity, job.Zone, ownersOfVehicles, true, false));

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
            return "Locating prey";
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

        /// <summary>
        /// when carrying the weapon, we are no longer negotiating, so we will never give it up.       
        ///       
        /// perhaps not strictly needed but may avoid some hunting AI problems...
        /// 
        /// also set on GoalHunt
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override bool CanDropRequestedItem(Entity item)
        {
            if (this.job != null && item.AssignedToJob == job.ID)
            {
                return false;
            }

            return true;
        }

        protected override void ProcessWhileActive(GameTime elapsed)
        {           
            if (!preconditionsRegulator.IsReady() ||
                ArePreconditionsOK())
            {
                Status = ProcessSubgoals(elapsed);

            }
            else
            {
                Status = Status.Failed;
            }

            if (Status == Goals.Status.Completed)
            {
                if (job != null) 
                {
                    // NEW: register unsuccesful hunt:
                    if (!hasFoundPrey)
                    {
                        EntityGroup owner;
                        if (job.ResolveOwner(out owner))
                        {
                            job.Zone.ZoneHunt.RegisterUnsuccessfulHunt(owner);
                        }
                    }

                    // Crash here..? because job was null? http://steamcommunity.com/app/284100/discussions/2/351660338730404425/     
                    DestroyJobAndRemoveLocks(ref job, null, replenishItemsForTools, null, weapon);      
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

              //  The.Client.AddLogEvent(The.Client.Log.DebugEvent, owner, "Handling message.");


                switch (message.MessageType)
                {
                    // someone wants us to stop doing this job:
                    case Message.MessageTypes.CancelJobOrItemInUse:
                    case Message.MessageTypes.CancelJobForAIReset:

                        Status = Status.Failed;

                        job.Abandon(entity);

                        return true; //msg handled

                    case Message.MessageTypes.PreyIsNear:
                        {
                            // this goal is completed and the job is done:
                            if (hasFoundPrey == false)
                            {
                                Entity targetEntity = message.Sender;

                                EntityGroup jobOwner;
                                if (job.ResolveOwner(out jobOwner)
                                    && targetEntity.CanBeHunted(entityIntelligence.Allegiance)
                                    && job.Zone.ZoneHunt.HasUnfulfilledHuntOrders(targetEntity.EntityType, jobOwner)// NEW: ignore any prey we are aren't after
                                    && !jobOwner.OtherJobs.Any(j => j is HuntingJob && ((HuntingJob)j).Target == targetEntity.ID))  // NEW: if we aren't hunting it already
                                {

                                    ThreatStance threatStanceToUse = ThreatStance.Bold;
                                    RegionMap footRegionMap =
                                        entityIntelligence.Allegiance.SharedKnowledge.GetMovementMap(entityIntelligence.ProtectionLevel,
                                        entity.EntityType, // entityIntelligence.Allegiance.RepresentativeEntityType.ThreatCategory, 
                                        threatStanceToUse).Layers[SurfaceType.TransportType.Foot].RegionMap;

                                    
                                    double travelTimeScore = 0.0f;

                                    RegionMap.Result result = GoalEvaluator.ScoreTravelTime(footRegionMap, threatStanceToUse, entity.AccessPoint.Value, // #ACCESS .PlaySiteLocation, 
                                        targetEntity.Location.Value, entity, ref travelTimeScore, job);

                                    if (result == RegionMap.Result.NoAccess)
                                    {
                                        return false;//Ensures that we dont try to hunt unreachable animals//Finn // can still hunt if Wait is returned... but perhaps that doesn't matter?
                                    }
                                    if (message.OtherInfo != null)
                                    {
                                        Trigger sourceTrigger = (Trigger)message.OtherInfo;
                                        entityIntelligence.SetTriggerCooldown(sourceTrigger, sourceTrigger.TriggerType.CooldownInTicks);
                                    }

                                    entityIntelligence.Memory.SetRecentlyFoundPrey(targetEntity.EntityID);

                                    Status = Status.Completed;

                                    RemoveAllSubgoals();

                                    Zone zone = job.Zone;
                                    job.Zone.ZoneHunt.NotifyHasFoundPrey(targetEntity.EntityType); // don't recreate the find prey job right away.
                                  
                                    // don't destroy the zone either
                                    DestroyJobAndRemoveLocks(ref job, null, replenishItemsForTools, null, weapon);
                                 
                                    hasFoundPrey = true;
                                      
                                    Expedition expedition = entityIntelligence.CurrentExpedition;

                                  
                                    // create a hunting job for the prey we just found. then cancel this goal and hope we get assigned the hunting job...                         
                                    // NEW: set a reference to the zone
                                    HuntingJob huntingJob = new HuntingJob(targetEntity.EntityID, targetEntity.EntityType, expedition.OwnedEntities, zone.ID);

                                }
                             
                                
                            }

                            return true;
                        }


                    default: return false;
                }
            }
            else
            {
                return true;
            }
        }

        public override void Deactivate()
        {
            if (job != null)
            {               
                //// de-assign replenish items:
                //AssignReplenishItems(job, replenishItemsForTools, false);
                RemoveLocksFromJob(job, null, replenishItemsForTools, null, weapon);
            }

            if (hasFoundPrey == false)
            {
                entity.SetSneaking(false);
            }
        }

        public double ScoreGoal()
        {
            return ScoreJobGoal(job);
        }

        private bool EntityCanBeMarkedAsHuntTarget(Entity entity)
        {
            if (entity != null
                && entity.EntityType.Person == null
                && entity.Intelligence != null
                && entity.Intelligence.Allegiance != null
                && entity.Intelligence.Allegiance.AllegianceType != Allegiances.AllegianceType.Player)
            {
                return true;
            }

            return false;

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
            this.snapshotJob = sn.SnapshotID<Job, JobID>(job);
            this.hasFoundPrey = sn.DoBool(hasFoundPrey);        
            this.weapon = sn.DoEntityAndRootNullable(weapon);
            this.replenishItemsForTools = sn.DoMultiMap(replenishItemsForTools);
            this.TimeSpentInTopLevelGoal = sn.DoDouble(TimeSpentInTopLevelGoal);

            sn.Ignore(job);

            return this;
        }

        public override void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            base.LoadPostProcess(sn);

            //job = (FindPreyJob)LookUp<Job, JobID>.FindByID(snapshotJob);
            if (snapshotJob != null)
            {
                job = (FindPreyJob)LookUp<Job, JobID>.FindByID(snapshotJob);
            }

        }


        #endregion
    }
}
