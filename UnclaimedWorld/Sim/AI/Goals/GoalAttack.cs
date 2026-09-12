using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Buildings;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Items;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Entities.Body;
using UWGame.SimSide.Entities.Containers;
using GameStateManagement;
using UWGame.ClientSide.Log;
using UWGame.SimSide.Resources;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Systems.Triggers;
using UWGame.Control;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Combat;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Processes;
namespace UWGame.SimSide.AI.Goals
{

    class GoalAttack : CompositeGoal, IIDEventSubscriber, ITopLevelGoal
    {
        public JobID jobID;

        /// <summary>
        /// This variable needs to be saved outside of attackJob aswell. 
        /// The reason why is if AttackJob gets destroyed we still need to be able to remove attackers for Goals that still has not done OnExit
        /// but has an invalidGoalID
        /// </summary>
        public EntityID target;

        private OwnerID? ownerOfCarcass;

        private AttackType attackType;
        BodyPartID bodyPartToAttackID;

        private EntityAndRoot? weapon;

        public double TimeSpentInTopLevelGoal { get; set; }

        /// <summary>
        /// 2014-11-20 Made Public as child Goal GoalDoAttack needs to use this list when destroying job. 
        /// GoalDoAttack now has reference to parent goal.
        /// </summary>
        private List<ReplenishItemsForAction> replenishActions;

        MethodID? goalMove_RepathDoneEventMethodID;

        GoalID? snapshotParentGoal;
        GoalHunt parentGoal;
        //private bool isChasingTarget;

        /// <summary>
        /// set false if a parent goal (GoalHunt) should manage the locks on job and items 
        /// </summary>
        bool manageLocks;

        public GoalAttack(Entity owner, AttackJob job, List<EntityGroupID> ownersOfVehicles, OwnerID? ownerOfCarcass, AttackType attackType,
            BodyPartID bodyPartToAttackID, EntityAndRoot? weapon, List<ReplenishItemsForAction> replenish, bool manageLocks = true, GoalHunt parentGoal = null)
            : base(owner)
        {
            this.jobID = job.ID;
            if (job.Target.HasValue)
            {
                this.target = job.Target.Value;
            }

            if (parentGoal == null)
            {
                this.parentGoal = null;
                this.replenishActions = replenish;
            }
            else
            {
                this.parentGoal = parentGoal;
                this.replenishActions = null;
            }

            
            this.ownersOfVehicles = ownersOfVehicles;
            this.ownerOfCarcass = ownerOfCarcass;
            this.attackType = attackType;
            //this.bodyPartToAttack = bodyPartToAttack;
            this.bodyPartToAttackID = bodyPartToAttackID;
            this.weapon = weapon;
          //  this.replenishActions = replenish;
            this.manageLocks = manageLocks;
        }

              

        public GoalAttack()
        {
        }

        public List<ReplenishItemsForAction> GetReplenishActions()
        {

            if (parentGoal != null)
            {
                return parentGoal.ReplenishActions;
            }
            else
            {
                return replenishActions;
            }
        }

        protected override void Activate()
        {
            Status = Status.Active;

            AttackJob attackJob = (AttackJob)LookUp<Job, JobID>.FindByID(jobID);
            if (attackJob != null)
            {
                // *** set locks
                if (manageLocks)
                {
                    // NEW:

                    attackJob.TakeJob(entity);

                    // assign the weapon:
                    if (!AssignWeapon(attackJob, weapon/*, true*/))
                        return;

                    // assign replenish items:
                    if (!SetLocksOnReplenishItems(attackJob, GetReplenishActions()))
                        return;

                }
                //***

                entityIntelligence.CombatInfo.Target = attackJob.Target; // store the target for easy lookup by others.


                IKnownEntityData targetData;
                if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(attackJob.Target.Value, out targetData)))
                {
                    return;
                }


                //make sure the subgoal list is clear.
                RemoveAllSubgoals();

                if (ArePreconditionsOK())
                {
                   
                    if (!GatherToolsOrWeapons(weapon, ownersOfVehicles, attackJob, StorageCompartment.Haul))
                    {
                        Status = Status.Failed;
                        return;
                    }

                    ReplenishWeapon();


                    if (attackType.RangeType != AttackType.RangeTypes.Melee)
                    {
                        ActivateRangedAttack(targetData);
                    }
                    else
                    {
                        ActivateMeleeAttack(targetData);
                    }

                    SetSpeedBasedOnUrgency();


                    // battles can last a long time.
                    // reset the slot timer for our patrol/area attack each time we attack.
                    SetLastAgentOnPost();
                }
                else
                {
                    Status = Goals.Status.Completed;
                }
            }
            else
            {
                Status = Status.Failed; //Job does not exist in activate??
            }
        }
       
        public override void OnExit()
        {
            Dictionary<EntityID, Vector3> meleeAttackers;
            if (The.Sim.MeleeAttackers.TryGetValue(target, out meleeAttackers))
            {
                meleeAttackers.Remove(entity.EntityID);
                if (meleeAttackers.Count == 0)
                {
                    The.Sim.MeleeAttackers.Remove(target);
                }
            }

            if (entity.EntityType.IntelligenceType.IsMobile
              && entity.Locomotor.LeggedLocomotor != null) // wheels also? probably..
            {
                if (entity.Locomotor.LeggedLocomotor.TargetSpeed == MovementSpeeds.Run)
                {
                    entity.Locomotor.LeggedLocomotor.TargetSpeed = MovementSpeeds.Normal;
                }
            }

            base.OnExit();
        }


        public override void Deactivate()
        {
            if (entity.Name != null && entity.Name.Contains("Lehner"))
            {

            }

            entityIntelligence.CombatInfo.Target = null;
            AttackJob attackJob = (AttackJob)LookUp<Job, JobID>.FindByID(jobID);

            // TODO: if job is null, still release other locks!!!
            // LOCKS            
            if (manageLocks)
            {
                if (attackJob != null)
                {
                    attackJob.Abandon(entity);

                }

                RemoveLockOnToolOrWeapon(jobID, weapon);
                //DeassignWeapon(jobID, weapon);

                // de-assign replenish items:
                RemoveLocksOnReplenishItems(jobID, GetReplenishActions());               

            }
        }
       

        private void SetLastAgentOnPost()
        {
            CombatAreaJob combatAreaJob = entityIntelligence.Memory.GetLastCombatAreaJob();
            if (combatAreaJob != null)
            {
                entityIntelligence.SetLastAgentOnPost(combatAreaJob);
            }
        }

        private void SetSpeedBasedOnUrgency()
        {
            if (entity.EntityType.IntelligenceType.IsMobile 
                && entity.Locomotor.LeggedLocomotor != null) // wheels also? probably..
            {
                // remember to undo this in Terminate or OnExit
                AttackJob attackJob = (AttackJob)LookUp<Job, JobID>.FindByID(jobID);
                if (attackJob != null)
                {
                    ThreatJob threatJob = attackJob as ThreatJob;
                    if (threatJob != null)
                    {
                        float urgency = threatJob.GetUrgency(entity);

                        if (urgency < 0.5f)
                        {
                            entity.Locomotor.LeggedLocomotor.TargetSpeed = MovementSpeeds.Normal; // MovementSpeeds.WalkSlowly; // slow down..?
                            return;
                        }
                        else if (urgency > 0.5f)
                        {
                            entity.Locomotor.LeggedLocomotor.TargetSpeed = MovementSpeeds.Run; //MP changed feb 2014  from: WalkFast
                            return;
                        }
                    }
                }

                entity.Locomotor.LeggedLocomotor.TargetSpeed = MovementSpeeds.Normal;
            }
        }

        public override Goal.DetectionFactor GetDetectAgentsFactor(EntityType typeOfAgent, bool requiresExamineAction)
        {
            // become very alert to agents when attacking!
            if (!requiresExamineAction)
            {
                return DetectionFactor.DetectVeryGood;
            }
            else
            {
                return DetectionFactor.CannotDetect;
            }
        }

        public override Goal.DetectionFactor GetDetectResourcesFactor(ResourceType resourceType, bool requiresExamineAction)
        {
            return DetectionFactor.CannotDetect;
        }


        public override bool RequiresBoldStance()
        {
            return true;
        }

        protected void ReplenishWeapon()
        {
            // now replenish if needed (the evaluator has already determined that it is possible with this weapon):
            List<ReplenishItemsForAction> items = GetReplenishActions();

            if (items != null)
            {
                AttackJob attackJob = (AttackJob)LookUp<Job, JobID>.FindByID(jobID);
                if (attackJob != null)
                {
                    foreach (var replenishAction in items)
                    {

                        AddSubgoal(new GoalReplenish(entity, weapon.Value,
                            replenishAction.Items,
                            replenishAction.Action,
                            ownersOfVehicles, attackJob, StorageCompartment.Haul));
                    }
                }
            }
        }


        protected void ActivateRangedAttack(IKnownEntityData targetData)
        {
             AttackJob attackJob = (AttackJob)LookUp<Job, JobID>.FindByID(jobID);
             if (attackJob != null)
             {
                 Vector3 rangedLocation;

                 bool inRangeYet;
                 if (targetData is Entity)
                 {
                     inRangeYet = CombatInfo.GetRangedLocation(entity, targetData, attackType, out rangedLocation); //targetData.Intelligence.CombatInfo.GetRangedLocation(entity, job.Target, attackType, out rangedLocation);
                 }
                 else
                 {
                     // we can't attack into the fog of war. even homing missiles need to see the target...
                     inRangeYet = false;
                     rangedLocation = targetData.PlaySiteLocation;
                 }

                 if (!inRangeYet)
                 {
                     //Since attacker out of range, he can't just turn and shoot
                     //Advance to within range, first                

                     if (entity.EntityType.IntelligenceType.IsMobile)
                     {
                         AddSubgoal(new GoalMoveToPosition(entity, rangedLocation, ownersOfVehicles, GoalMoveToPosition.VehicleUse.FreeUpAfterUse, attackJob.Target, false, attackType));
                     }
                     else
                     {
                         Status = Goals.Status.Failed;
                         return;
                     }

                 }

                 //turn
                 AddSubgoal(new GoalTurnToFace(entity, null, targetData.EntityID));

                 AddSubgoal(new GoalWait(entity, 1)); // 0.6)); // wait a bit to allow the renderable to catch up the entity so anims will play better...

                 //shoot
                 AddSubgoal(new GoalDoAttack(entity, attackJob, ownerOfCarcass, attackType, bodyPartToAttackID, weapon, this));


                 entity.Intelligence.SetBoldStance();
             }
             else
             {
                 Status = Goals.Status.Failed;
             }
            // MLo TODO May need to employ this failsafe later, so not deleting yet
            // failed to find a location from which to use ranged attack.
            //Status = Status.Failed;
        }

      
        protected void ActivateMeleeAttack(IKnownEntityData targetData)
        {
             AttackJob attackJob = (AttackJob)LookUp<Job, JobID>.FindByID(jobID);
             if (attackJob != null)
             {
                 Vector3? meleeLocation;
                 bool chaseTargetCenterLocation;

                 if (CombatInfo.GetMeleeLocation(entity, targetData, out meleeLocation, out chaseTargetCenterLocation))
                 //targetData.Intelligence.CombatInfo.GetMeleeLocation(entity, job.Target, out meleeLocation, out chaseTargetCenterLocation)) 
                 {
                     if (Common.DistanceOctile(entity.PlaySiteLocation, targetData.PlaySiteLocation) > 48f)
                     {
                         // wait a bit, to see if we are asked to cancel by other agents. 
                         //Not if we are very close, though. It creates delays between attacks.
                         AddSubgoal(new GoalWait(entity, GameData.Instance.AIConstants.TimeToWaitBeforeStartingGoal));
                     }

                     //must pass this struct to GetMeleeLocation, below
                     //bool GetDestinationEntityLocation(Entity mover, out DestinationDescription? destination);


                     // move to our assigned attack location
                     //GoalMoveToPosition.GetDestinationEntityLocation del = CombatInfo.GetMeleeLocation;

                     GoalMoveToPosition goalMove = new GoalMoveToPosition(entity, meleeLocation.Value, ownersOfVehicles, GoalMoveToPosition.VehicleUse.FreeUpAfterUse, attackJob.Target, true, attackType) { IsFinalDestination = true, IsChasingTargetCenterLocation = chaseTargetCenterLocation };

                     goalMove.RegisterPathDoneSubscriber(goalMove_RepathDoneEvent,
                         this, out goalMove_RepathDoneEventMethodID);

                     AddSubgoal(goalMove);

                     AddSubgoal(new GoalTurnToFace(entity, null, targetData.EntityID));

                     // start attacking
                     AddSubgoal(new GoalDoAttack(entity, attackJob, ownerOfCarcass, attackType, bodyPartToAttackID, weapon, this));

                     // mark us as an attacker even if we are still moving in
                     Dictionary<EntityID, Vector3> meleeAttackers;
                     if (!The.Sim.MeleeAttackers.TryGetValue(targetData.EntityID, out meleeAttackers))
                     {
                         meleeAttackers = new Dictionary<EntityID, Vector3>();
                         The.Sim.MeleeAttackers.Add(targetData.EntityID, meleeAttackers);
                     }
                     meleeAttackers.Add(entity.EntityID, meleeLocation.Value);

                     /*    if (entity.EntityID == (EntityID)4541 // twinkler
                             && targetData.EntityID == (EntityID)4564) // pygmy
                         {
                             The.Sim.DebugAttackLog.Add("Added attacker, entity goal status: " + entityIntelligence.Brain.ComposeIndentedString(""));
                         }*/

                     // set our stance to 'Bold' when attacking to prevent blocking/fleeing.                    
                     entity.Intelligence.SetBoldStance();
                 }
                 else
                 {
                     // failed to find a location to melee.
                     Status = Status.Failed;
                 }
             }
             else
             {
                    // Failed job has been removed. Job is done.
                Status = Status.Failed;
             }
        }

        

     /*   private AttackType SelectAttackType()
        {

        }*/

      

        protected override bool ArePreconditionsOK()
        {
            if (isRejoicing)
                return true;

            IKnownEntityData weaponData;
            if (!IsToolOrWeaponOK(weapon, out weaponData))
                return false;

            AttackJob attackJob = (AttackJob)LookUp<Job, JobID>.FindByID(jobID);
            IKnownEntityData targetData;
            if (attackJob != null)
            {
                if (weaponData != null && Common.IsZero(attackJob.GetWeaponPolicyScore(entity, weaponData, attackType)))
                {
                    return false;
                }
                
                if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(attackJob.Target.Value, out targetData)))
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            Entity targetAsEntity = targetData as Entity;

            if (targetAsEntity != null)
            {
                if (!chaseProgressWasMade && !GoalDoAttack.IsInRange(entity, targetData, attackType))
                {
                    if (The.Sim.GameplayRandomGenerator.NextDouble("GoalAttack") < 0.35)
                    {
                        // give up the chase...
                        if (entityIntelligence.Allegiance.AllegianceType == Allegiances.AllegianceType.Player)
                        {
                            
                            The.Client.Log.AddLogEvent(The.Client.Log.CombatEvent, entity, string.Format("has given up chasing {0}.", targetAsEntity));
                            
                        }

                        if (attackType.RangeType == AttackType.RangeTypes.Melee && entityIntelligence.Allegiance.HumanActivities != null)
                        {
                            // record the failed chase attempt:
                            entityIntelligence.Allegiance.HumanActivities.HasChasedHuntedCritter[targetData.EntityType] = true;
                        }

                        return false;
                    }
                }
            }           

            // also give up if we are far from home / outside hunting zones etc... - done in GoalHunt. ThreatManager should remove threat jobs that are too far away from own assets

            return true;
        }

        private bool isRejoicing = false;


        protected override void ProcessWhileActive(GameTime elapsed)
        {
          
            if (!preconditionsRegulator.IsReady() ||
                ArePreconditionsOK())
            {
                //process the subgoals
                Status = ProcessSubgoals(elapsed);
            }
            else
            {
                Status = Goals.Status.Failed;
                // Status = Status.Completed; // Failed??? only makes a difference with MoveToDestination...
            }


            if (Status == Status.Failed)
            {   // we don't want it anymore...
                AttackJob attackJob = (AttackJob)LookUp<Job, JobID>.FindByID(jobID);
                if (attackJob != null)
                {
                    attackJob.Abandon(entity);
                }
            }          
        }

        public override string GetStatus()
        {
            return "Attacking";
        }

        public double ScoreGoal()
        {
            AttackJob attackJob = (AttackJob)LookUp<Job, JobID>.FindByID(jobID);
            if (attackJob == null)
            {
                return 0d;
            }
            AttackParams attackParams = new AttackParams()
            {
                AttackType = attackType,
                BodyPartToAttackID = bodyPartToAttackID,
                Weapon = weapon
            };
            return ScoreJobGoal(attackJob, null, attackParams);            
        }

        bool chaseProgressWasMade = true;
        float? currentPathLength;

        void goalMove_RepathDoneEvent(float newPathLength)
        {
            if (currentPathLength == null || newPathLength < 140f)
            {   // don't monitor progress when we get close
                chaseProgressWasMade = true;
            }
            else if (newPathLength > currentPathLength)
            {
                chaseProgressWasMade = false;
            }
            

            currentPathLength = newPathLength;
        }


        private const double timeBetweenChaseProgressEvaluation = 3;
        private Regulator chaseProgressRegulator;

        private float? previousDistanceToTarget = null;

        /// <summary>
        /// are we catching up with our target?
        /// </summary>
        /// <returns></returns>
        private bool ChaseProgressWasMade()
        {
            return chaseProgressWasMade;
        }

        public override bool IsSame(Jobs.Job job)
        {
            AttackJob attackJob = (AttackJob)LookUp<Job, JobID>.FindByID(this.jobID);
            if (attackJob != null)
            {
                return job == attackJob;
            }
            return false;

        }

        /// <summary>
        /// when carrying the weapon, we are no longer negotiating, so we will never give it up.
        /// I think this is reasonable and needed to avoid chaotic battles.
        ///       
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override bool CanDropRequestedItem(Entity item)
        {
            if (item.AssignedToJob == this.jobID)
            {
                return false;
            }

            return true;
        }


        protected override void CreateRegulators()
        {
            base.CreateRegulators();

            chaseProgressRegulator = new Regulator(The.Sim.GameplayRandomGenerator, 1d / timeBetweenChaseProgressEvaluation, "GoalAttackProgress");
        }
      

        public override bool HandleMessage(Message message)
        {
            if (entity.Name != null && entity.Name.Contains("Lehner"))
            {

            }

            AttackJob attackJob;
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

                      Status = Status.Failed;
                      //job.TakenBy.TryRemove(entity);
                       attackJob = (AttackJob)LookUp<Job, JobID>.FindByID(jobID);
                      if (attackJob != null)
                      {
                          attackJob.Abandon(entity); // not needed - done in Deactivate.
                      }

                      return true; //msg handled
                                     
                  case Message.MessageTypes.EntityDied:
                      if (personEntity != null 
                        //  && !entity.Intelligence.Allegiance.IsUnderThreat() // job still exists...
                          && The.Sim.GameplayRandomGenerator.NextDouble("GoalAttack") < GameData.Instance.Constants.ChanceToExultAfterWinning)
                      {
                          // rejoice...
                        //  isRejoicing = true; 
                          
                          Trigger sourceTrigger = (Trigger)message.OtherInfo;
                          entityIntelligence.SetTriggerCooldown(sourceTrigger, sourceTrigger.TriggerType.CooldownInTicks);
                          
                          Tuple<EntityID, EntityType> info = (Tuple<EntityID, EntityType>)sourceTrigger.messageInfo;
                          EntityID entityID = info.Item1;
                          EntityType entityType = info.Item2;
                           attackJob = (AttackJob)LookUp<Job, JobID>.FindByID(jobID);

                           if (attackJob != null && entityID == attackJob.Target)
                          { 
                              // rejoice if our target died
                              isRejoicing = true; // disable preconditions check

                              Interest interest = sourceTrigger.TriggerType.Interest;
                              if (interest != null)
                              {
                                  entityIntelligence.SetNewCenterOfAttention(entityID, null,
                                      (float)NormalDistribution.GetRandomValue(The.Sim.GameplayRandomGenerator, interest.InterestLevelMean, interest.InterestLevelStdDeviation));
                              }

                              // delay to avoid clones
                              AddSubgoal(new GoalWait(entity, 0.6f + 0.8f * The.Sim.GameplayRandomGenerator.NextDouble("GoalAttack"), true));

                              AddSubgoal(new GoalWait(entity, 2f, AnimAction.Idle, AnimModifier.Happy, true));

                              // add a delay after exult...
                              AddSubgoal(new GoalWait(entity, 1f, true));

                              Status = Goals.Status.Active;
                          }
                          else
                          {
                              return false;
                          }
                      }

                      return true;
                  default: return false;
              }
          }
          else
          {
              return true;
          }
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

            //System.Diagnostics.Debug.Assert(job == null || job.ID != JobID.Invalid, "Invalid ID!!");
            this.target = sn.DoEnum<EntityID>(target);
            this.jobID = sn.DoEnum<JobID>(jobID);
            this.ownerOfCarcass = sn.DoEnumNullable(ownerOfCarcass);
            this.previousDistanceToTarget = sn.DoFloatNullable(previousDistanceToTarget);
            this.attackType = sn.DoGameData(attackType);
            this.bodyPartToAttackID = sn.DoEnum(bodyPartToAttackID);
            this.weapon = sn.DoEntityAndRootNullable(weapon);
            this.chaseProgressWasMade = sn.DoBool(chaseProgressWasMade);
            this.currentPathLength = sn.DoFloatNullable(currentPathLength);
            this.isRejoicing = sn.DoBool(isRejoicing);
            this.manageLocks = sn.DoBool(manageLocks);
            this.TimeSpentInTopLevelGoal = sn.DoDouble(TimeSpentInTopLevelGoal);

            this.goalMove_RepathDoneEventMethodID = sn.DoEnumNullable(goalMove_RepathDoneEventMethodID);


            this.snapshotParentGoal = sn.SnapshotID<Goal, GoalID>(parentGoal);

        //    this.parentGoal = (GoalHunt)LookUpGoals.FindByID(snapshotParentGoal);
            this.replenishActions = sn.DoList(replenishActions);
            if (parentGoal != null || replenishActions != null)
            {
                int i = 0;
            }
            
            sn.Ignore(jobID);
            sn.Ignore(parentGoal);
           

            return this;
        }


        public override void LoadPostProcess(Snapshotter sn)
        {

            sn.RegisterLoadPostProcessCall(this);

            base.LoadPostProcess(sn);

           // job = (AttackJob)LookUp<Job, JobID>.FindByID(snapshotJob);


          //  parentGoal = (GoalHunt)LookUpGoals.FindByID(snapshotParentGoal);

            if (snapshotParentGoal != null)
            {
                parentGoal = (GoalHunt)LookUpGoals.FindByID(snapshotParentGoal);
                //parentGoal = (GoalHunt)LookUp<Goal, GoalID>.FindByID(snapshotParentGoal);
            }

            LoadPostProcessRegisterMethodIDs();

           
        }

        public void LoadPostProcessRegisterMethodIDs()
        {
            if (goalMove_RepathDoneEventMethodID.HasValue)
            {
                ActionLookup<float>.Add(goalMove_RepathDoneEventMethodID.Value, goalMove_RepathDoneEvent);
            }
        }

        #endregion
    }
}
