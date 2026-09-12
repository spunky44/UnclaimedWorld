using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Buildings;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Items;
using UWGame.SimSide.Entities.Body;
using GameStateManagement;
using UWGame.ClientSide.Log;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Resources;
using UWGame.Control;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Combat;
using UWGame.SimSide.Processes;
namespace UWGame.SimSide.AI.Goals
{
    /// <summary>
    ///  use this goal to manage multiple attacks against the same hunt target without interruption, sneaking, abandoning the target etc.
    /// </summary>
    class GoalHunt : CompositeGoal, ITopLevelGoal
    {
        private HuntingJob job;
        JobID snapshotJob;

        private OwnerID? ownerOfCarcass;

        private AttackType attackType;
        BodyPartID bodyPartToAttackID;
        private EntityAndRoot? weapon;
        public List<ReplenishItemsForAction> ReplenishActions;

        public double TimeSpentInTopLevelGoal { get; set; }

       
        public GoalHunt(Entity entity, HuntingJob job, List<EntityGroupID> ownersOfVehicles, OwnerID? ownerOfCarcass, AttackType attackType,
            BodyPartID bodyPartToAttackID, EntityAndRoot? weapon, 
            List<ReplenishItemsForAction> replenish)
            : base(entity)
        {
            this.job = job;
            this.ownersOfVehicles = ownersOfVehicles;
            this.ownerOfCarcass = ownerOfCarcass;
            this.attackType = attackType;
            //this.bodyPartToAttack = bodyPartToAttack;
            this.bodyPartToAttackID = bodyPartToAttackID;
            this.weapon = weapon;
            this.ReplenishActions = replenish;
        }


       

        public GoalHunt()
        {
        }

        protected override void Activate()
        {
            Status = Status.Active;

            RemoveAllSubgoals();

            // set the locks in this goal so it is done immediately - it will also be done in GoalAttack once that is activated... and the locks will be removed again after the first attack...?
            // *** set locks
            // NEW:
            job.TakeJob(entity);

            // assign the weapon:
            if (!AssignWeapon(job, weapon/*, true*/))
                return;

            // assign replenish items:
            if (!SetLocksOnReplenishItems(job, ReplenishActions))
                return;


            AddSubgoal(new GoalAttack(entity, job, ownersOfVehicles, ownerOfCarcass, attackType, bodyPartToAttackID, weapon, ReplenishActions, false, this));                       
           
        }

     
        
        protected override bool ArePreconditionsOK()
        {
            IKnownEntityData targetData;
            if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(job.Target.Value, out targetData)))
            {
                return false;
            }

            if (IsBeyondHuntingLimits(entity, targetData))
            {
                if (The.Sim.GameplayRandomGenerator.NextDouble("GoalHunt") < 0.2)
                {
                    return false;
                }
            }

            return true;
        }

        public override bool RequiresBoldStance()
        {
            return true;
        }


        public static bool IsBeyondHuntingLimits(Entity entity, IKnownEntityData target)
        {
            if (The.Map.IsHuntingZone(entity.Intelligence.Allegiance, entity.MapPosition.Value))
            {
                return false;
            }

            if (target != null)
            {
                return TargetIsTooFarFromExpedition(entity, target.PlaySiteLocation);
            }
            else return false;
        }

        public static bool TargetIsTooFarFromExpedition(Entity entity, Vector3 targetLocation, float addToDistance = 0f)
        {
            Vector3 homeLocation;
            homeLocation = entity.Intelligence.CurrentExpedition.Center.Value;

            /*
            if (entity.EntityType.Person != null)
            {
                homeLocation = entity.Intelligence.CurrentExpedition.Center;
            }
            else return false; // critters too???
            */

          //  float distance = Common.DistanceOctile(entity.PlaySiteLocation, homeLocation); // why hunter's distance?
            float distance = Common.DistanceOctile(targetLocation, homeLocation);

            if (distance + addToDistance > GameData.Instance.AIConstants.MaximumDistanceFromExpeditionToChasePrey)
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// when carrying the weapon, we are no longer negotiating, so we will never give it up.       
        ///       
        /// perhaps not strictly needed but may avoid some hunting AI problems...
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


        public override string GetStatus()
        {
            return "Hunting";
        }

        public double ScoreGoal()
        {
            AttackParams attackParams = new AttackParams()
            {
                AttackType = attackType,
                BodyPartToAttackID = bodyPartToAttackID,
                Weapon = weapon
            };
            return ScoreJobGoal(job, null, attackParams);
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

        Regulator setSneakingRegulator;
        protected override void ProcessWhileActive(GameTime elapsed)
        {
           /* ActivateIfInactive();

            if (Status == Goals.Status.Active)
            {*/

                if (!preconditionsRegulator.IsReady() ||
                        ArePreconditionsOK())
                {
                    // sneak when within a certain distance, stop sneaking when further away:
                    if (setSneakingRegulator.IsReady())
                    {
                        if (!SetSneaking())
                        {
                            //ExitIfFailedOrCompleted();
                            return; // Status;
                        }
                    }

                    Status = ProcessSubgoals(elapsed);
                }
                else
                {

                    Status = Status.Failed;
                }

                if (Status == Goals.Status.Completed)
                {
                    // is the prey dead? otherwise try to attack it again:
                    if (!AttackTargetAgain())
                    {
                        MarkSuccesfulZoneHunt();

                        Status = Goals.Status.Completed;
                    }
                    else
                    {
                        Status = Goals.Status.Active;
                    }
                }
                else if (Status == Goals.Status.Failed)
                {
                    // if the target moved out of range, GoalAttack will fail. perhaps try to pursue the memory fact location...
                }
          /*  }

            ExitIfFailedOrCompleted();

            return Status;*/
        }

        private void MarkSuccesfulZoneHunt()
        {
            if (job.HuntZone.HasValue)
            {
                Zone zone = LookUp<Zone, ZoneID>.FindByID(job.HuntZone.Value);
                if (zone != null)
                {
                    zone.ZoneHunt.NotifySuccessfulHunt(job.TargetCreatureType, zone);
                }
            }
        }

        protected override void CreateRegulators()
        {
            base.CreateRegulators();

            setSneakingRegulator = new Regulator(The.Sim.GameplayRandomGenerator, 1, "GoalHuntSneaking");
        }

        /// <summary>
        /// get a set of combos against the same target, using only carried weapons and ammo.
        /// </summary>
        /// <returns></returns>
        private bool AttackTargetAgain()
        {
            IKnownEntityData targetData;
            if(EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(job.Target.Value, out targetData)))
            {
                return false;
            }
           
            if (targetData.Body.IsDead())
            {
                return false;
            }
            List<WeaponInstanceCombo> allCombos = new List<WeaponInstanceCombo>();
            
            // get all combos with carried weapons and ammo:
            EvaluateAttackJobs.GetAllWeaponInstanceCombosWithCarriedWeapons(entity, job, new WeaponInstanceComboJobData(){ Job = job }, targetData, allCombos);

            // now score the attacks (using simplified scoring):
            double damageAndChanceToHitScore;
            WeaponInstanceCombo combo;
            for (int i = 0; i < allCombos.Count; i++)
			{
                combo = allCombos[i];

                combo.AttackData.JobData.AttackDirection = GoalDoAttack.GetAttackDirection(entity,
                   targetData.PlaySiteLocation,
                   targetData.Rotation);
                BodyPart bodyPartBeingScored = targetData.Body.FindBodyPart(combo.AttackData.bodyPartID);
                combo.AttackData.EstimatedDamageScore = GameData.Instance.AttackScoresAgainstBodyParts[combo.AttackData.AttackType][bodyPartBeingScored.BodyPartType];

			    damageAndChanceToHitScore = AttackJob.ScoreAttackDamageAndChanceToHit(entity, combo, targetData, 1f /* energyLevelFactor.Value*/, combo.AttackData.EstimatedDamageScore);
                combo.Score = (float)damageAndChanceToHitScore;

                if (combo.Weapon != null && weapon != null && combo.Weapon.EntityID == weapon.Value.Entity)
                {
                    // weigh attacks with the same weapon higher
                    combo.Score *= 2f;
                }                
			}


            allCombos.RemoveAll(c => c.Score == 0d);
            
            if (allCombos.Count > 0)
            {
                allCombos.Sort((a, b) => b.Score.CompareTo(a.Score));

                // remove low scores:
                double topScore = allCombos[0].Score;
                allCombos.RemoveAll(c => c.Score < 0.6 * topScore);            


                float totalScore;
                // pick attack type and body part weighted randomly:
                Common.BuildEdgesFromBucketSizes(allCombos, true, out totalScore);
                if (totalScore > 0f)
                {

                    int index;
                    Common.GetStairStepIndex(allCombos, out index,The.Sim.GameplayRandomGenerator);

                    WeaponInstanceCombo selectedCombo = allCombos[index];

                    List<ReplenishItemsForAction> allFoundReplenishItems = null;



                    if (selectedCombo.Weapon != null)
                    {
                        bool hasEnoughAmmo = true;
                        if (selectedCombo.AttackData.AttackType.RoundsToSpend.HasValue)
                        {
                            // Ammunition ammo;
                            //  entityToReplenish.Find(out ammo);
                            hasEnoughAmmo = selectedCombo.Weapon.HasEnoughAmmo(selectedCombo.AttackData.AttackType.UsesAmmoType, selectedCombo.AttackData.AttackType.RoundsToSpend.Value);

                        }

                        if (!hasEnoughAmmo)
                        {
                            // a reload is necessary (carried ammo only):

                            List<Entity> ammoItems = entity.Contains.GetContainedItemsList(i => i.EntityType.ItemType != null && i.EntityType.ItemType.AmmunitionType != null
                                && i.EntityType == selectedCombo.AttackData.AttackType.UsesAmmoType);

                            if (ammoItems != null && ammoItems.Count > 0)
                            {
                                List<EntityID> ammoItemsToReplenishWith = new List<EntityID>();
                                int ammoCount = 0;
                                foreach (var item in ammoItems)
                                {
                                    ammoCount += item.Item.Ammunition.NoOfRounds;
                                    ammoItemsToReplenishWith.Add(item.EntityID);

                                    if (ammoCount >= selectedCombo.AttackData.AttackType.RoundsToSpend.Value)
                                    {
                                        break;
                                    }
                                }

                                if (ammoCount < selectedCombo.AttackData.AttackType.RoundsToSpend.Value)
                                {
                                    // could not reload...
                                    return false;
                                }

                                // replenish actions! reload with carried ammo it necessary:     
                                ProcessType process = selectedCombo.Weapon.EntityType.ContainerType.GetReplenishProcesses()[selectedCombo.AttackData.AttackType.UsesAmmoType];
                                allFoundReplenishItems = new List<ReplenishItemsForAction>();
                                allFoundReplenishItems.Add(new ReplenishItemsForAction(process, ammoItemsToReplenishWith));
                            }
                        }
                    }

                    ReplenishActions = allFoundReplenishItems; // update this property since it will be accessed from the subgoal...

                    // make sure outer goal (this) manages locks:
                    AddSubgoal(new GoalAttack(entity, job, ownersOfVehicles, ownerOfCarcass, selectedCombo.AttackData.AttackType, selectedCombo.AttackData.bodyPartID,
                        (selectedCombo.Weapon != null ? (EntityAndRoot?)selectedCombo.Weapon.GetAsEntityAndRoot() : null), allFoundReplenishItems, false, this));


                    return true;
                }
            }

            return false;
        }

        private bool SetSneaking()
        {
            IKnownEntityData targetData;
            if(EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(job.Target.Value, out targetData)))
            {
                return false;
            }

            bool weAreSpotted = false;
            Entity targetEntity = targetData as Entity;
            if (targetEntity != null)
            {
                if (targetEntity.CanSeeEntity(entity))
                {
                    weAreSpotted = true;
                }
            }

            if (weAreSpotted)
            {
                if (entityIntelligence.IsStealthy)
                {
                    // we have been spotted. don't sneak anymore, perhaps also fail the goal.
                    entity.SetSneaking(false);
                                        
                  //  The.Client.AddLogEvent(entityIntelligence.Allegiance, The.Client.Log.DebugEvent, entity, string.Format("was spotted by {0} and has stopped sneaking.", targetEntity));
                        
                   
                }

            }
            else if (OtherActiveAllegianceMembersAreStandingNearby(entity, entityIntelligence, 90f))
            {
                entity.SetSneaking(false);
            }
            else
            {
                float distance = Common.DistanceOctile(entity.PlaySiteLocation, targetData.PlaySiteLocation);
                float targetSensorRange = targetData.EntityType.SensorType.Range;

                if (distance < targetSensorRange + 100f) // + 80f * Globals.Instance.RandomPredictable.NextDouble())
                {
                    entity.SetSneaking(true);
                }
                else if (distance > targetSensorRange + 190f) // + 80f * Globals.Instance.RandomPredictable.NextDouble()) // hysteresis - in the middle, don't change state
                {
                    entity.SetSneaking(false);
                }
            }


            return true;
        }

        /// <summary>
        /// it looks daft to sneak around when others are standing next to the agent. 
        /// They would have caused detection by the target.
        /// </summary>
        /// <returns></returns>
        public static bool OtherActiveAllegianceMembersAreStandingNearby(Entity entity, Intelligence entityIntelligence, float radius)
        {
            List<Pair<Entity,Vector2>> nearbyEntities = null;
            The.AgentQuadTree.GetEntitiesInRange(entity.PlaySiteLocation.ToVector2(), 100f,
                e => e != entity 
                    && e.Intelligence.Allegiance == entityIntelligence.Allegiance
                    && !e.Intelligence.IsStealthy
                    && e.Intelligence.IsAwakeAndActive
                , ref nearbyEntities);

            if (nearbyEntities != null && nearbyEntities.Count > 0)
                return true;

            return false;
        }

        public override bool IsSame(Jobs.Job job)
        {
            return job == this.job;
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

                        Status = Status.Failed;

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
        
        public override void Deactivate()
        {
           
            entity.SetSneaking(false);


            // LOCKS
            job.Abandon(entity);

            RemoveLockOnToolOrWeapon(job.ID, weapon);
           // DeassignWeapon(job.ID, weapon);
           
            RemoveLocksOnReplenishItems(job.ID, ReplenishActions);

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

            this.snapshotJob = (JobID)sn.SnapshotID<Job, JobID>(job);
            this.ownerOfCarcass = sn.DoEnumNullable(ownerOfCarcass);
            this.attackType = sn.DoGameData(attackType);          
            this.bodyPartToAttackID = sn.DoEnum(bodyPartToAttackID);
            this.weapon = sn.DoEntityAndRootNullable(weapon); // DoEntityIDNullable(weapon);
            this.ReplenishActions = sn.DoList(ReplenishActions);
            this.TimeSpentInTopLevelGoal = sn.DoDouble(TimeSpentInTopLevelGoal);


            sn.Ignore(job);


            return this;
        }

        public override void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            base.LoadPostProcess(sn);

            job = (HuntingJob)LookUp<Job, JobID>.FindByID(snapshotJob);


        }


        #endregion
    }
}
