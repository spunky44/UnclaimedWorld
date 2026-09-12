using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Items;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Entities;
using UWGame.SimSide.AI.Needs;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Buildings;
using UWGame.ClientSide.Renderables;
using UWGame.ClientSide.Log;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Collisions;
using UWGame.SimSide.InGameEvents.Actions;
using Microsoft.Xna.Framework;
using System.Linq;
using UWGame.SimSide.Snapshots;
namespace UWGame.SimSide.AI.Goals
{
    class GoalDoSleep: CompositeGoal 
    {
        //private bool isSleepingUnnaturally;

        private Regulator testTimeToWakeupRegulator;

        /// <summary>
        /// the maximum amount of sleep we can gain when sleeping under these conditions.
        /// </summary>
        float maximumSleepGainFromThisLocation = 1f;

        /// <summary>
        /// how fast do we regain sleep under these conditions?
        /// </summary>
        
        float increaseAmountPerDay;



        bool isSleeping = false;

        /// <summary>
        /// here we temproarily save the entity's collidable object until he wakes up. 
        /// TODO: Delete this code after agent footprints are implemented...
        /// </summary>
       // Collidable<Entity> collidable;

        public GoalDoSleep(Entity owner, EvaluateSleep evaluator)
            : base(owner)
        {
            GoalEvaluator = evaluator;
        }

        protected override void CreateRegulators()
        {
            conditionRegulator = new Regulator(The.Sim.GameplayRandomGenerator, 0.5, "GoalDoSleepCondition");
            testTimeToWakeupRegulator = new Regulator(The.Sim.GameplayRandomGenerator, 1, "GoalDoSleep");
        }


     

        public GoalDoSleep()
        {
        }


        protected override void Activate()
        {
            if (AreThereNonMovingEntitiesInThisSpot())
            {
                Status = Goals.Status.Failed;
            }
            else
            {
                Entity insideBuilding = null;
                if (!entity.GetContainedBy(out insideBuilding))
                {
                    Status = Goals.Status.Failed;

                    return;
                }

                ComputeSleepConditions(entity, insideBuilding, out maximumSleepGainFromThisLocation, out increaseAmountPerDay);

                Status = Status.Active;
               

                if (entity.EntityType.Person != null) // && entityIntelligence.Allegiance.AllegianceType == Allegiances.AllegianceType.Player)
                {
                    The.Client.AddLogEvent(entityIntelligence.Allegiance, The.Client.Log.GeneralEvent, entity, "goes to sleep");                   

                }

                entityIntelligence.IsAwakeAndActive = false;

                // just drop everything where we sleep...
                DropAllCarriedItems();

                EnableCollisions(false);

                // lay down:
                if (entity.HasStance())
                {
                    ChangeStance(entity.Locomotor.Stance.PickRandomStance(entity.EntityType.LocomotorType.StancesType.SleepStances, entity.EntityType.LocomotorType.StancesType.DefaultStanceType)); // Entities.Locomotors.LeggedLocomotor.Stance.Lying);
                }
                //SetAnimationStateGoal();
                /*
                entity.Renderable.SetAnimationActionStateFlag(AnimAction.Sleeping); 

                // HACK:
                // ignore collisions with sleepers...
                // this is a temporary fix because the pathfinder cannot see and avoid them. When we get footprints from entities implemented, delete this.
                if (entity.CollisionProxy != null)
                {
                    collidable = entity.CollisionProxy;
                    The.CollisionManager.RemoveCollidable(entity.CollisionProxy);

                    entity.CollisionProxy = null;
                }

              */

                // fire triggers, events etc:            
                List<ActionSets> defaultActionSets;
                entity.EntityType.IntelligenceType.EventActions.TryGetValue(AgentActionHooks.GoingToSleep, out defaultActionSets);

                Goal.FireEventActions(entity, null, defaultActionSets, null);    
            }
        }

        public override bool CanReactToInterest()
        {
            return false;
        }

        /// <summary>
        /// compute two values:
        /// how fast we regain sleep
        /// the best sleep needs value that we can achieve
        /// </summary>
        public static void ComputeSleepConditions(Entity entity, IKnownEntityData containerData, out float maximumSleepGainFromThisLocation, out float increaseAmountPerDay)
        {
            float sleepNeedGainFactor = 1f;

            // can recharge fully per default
            maximumSleepGainFromThisLocation = 1f;


            if (entity.EntityType.Person != null)
            {

                if (containerData == null)
                {
                    maximumSleepGainFromThisLocation = GameData.Instance.Constants.SleepNeed.MaxLimitForPeopleSleepingInOpen;

                    sleepNeedGainFactor = GameData.Instance.Constants.SleepNeed.GainFactorForPeopleSleepingInOpen;
                }
                else if (containerData.EntityType.ContainerType != null && containerData.EntityType.ContainerType.ResidenceType != null) 
                { // sleeping indoors

                    sleepNeedGainFactor = Residence.GetSleepNeedGainFactor(containerData.Condition.Value, 
                            containerData.EntityType.ContainerType.ResidenceType.ComfortLevel); // residence.Residence.GetSleepNeedGainFactor();
                    
                }
                else if (containerData.EntityType.GatheringSiteType != null)
                {
                    if (containerData.IsPrepared == true)
                    {
                        // campfire
                        maximumSleepGainFromThisLocation = GameData.Instance.Constants.SleepNeed.MaxLimitForPeopleSleepingNearCampfire;
                        sleepNeedGainFactor = GameData.Instance.Constants.SleepNeed.GainFactorForPeopleSleepingNearCampfire;
                    }
                    else
                    {
                        maximumSleepGainFromThisLocation = GameData.Instance.Constants.SleepNeed.MaxLimitForPeopleSleepingInOpen;
                        sleepNeedGainFactor = GameData.Instance.Constants.SleepNeed.GainFactorForPeopleSleepingInOpen;
                    }
                }

                // the sleepNeedGainFactor should be between 0.8 and 1.1 I think.
                increaseAmountPerDay = sleepNeedGainFactor * GameData.Instance.Constants.SleepNeed.GainPerDayWhenSleeping;

            }
            else
            {
                increaseAmountPerDay = GameData.Instance.Constants.SleepNeed.GainPerDayWhenSleeping;
            }
        }


        public override float GetExertionLevel()
        {
            return GameData.Instance.Constants.PhysicalWork.Sleeping;
        }

        public override Goal.DetectionFactor GetDetectAgentsFactor(EntityType typeOfAgent, bool requiresExamineAction)
        {
            return DetectionFactor.CannotDetect;
        }


        public override Goal.DetectionFactor GetDetectResourcesFactor(ResourceType resourceType, bool requiresExamineAction)
        {
            return DetectionFactor.CannotDetect;
        }
        

        public override string GetStatus()
        {
            return "Sleeping";
        }


     

        Regulator conditionRegulator;
        protected override void ProcessWhileActive(GameTime elapsed)
        {
           
           /* ActivateIfInactive();           

            // we don't run evaluators in our sleep - so no atomic goals are needed.

            if (Status == Status.Active)
            {*/
                // added this to play the lay down breakdown anim:
                Status = ProcessSubgoals(elapsed);

                if (Status == Goals.Status.Completed 
                    && !isSleeping)
                {


                    // only run this once:

                    // NOW we are sleeping:
                    isSleeping = true;
                    entity.Renderable.SetAnimationActionStateFlag(AnimAction.Sleeping);

                    // HACK:
                    // ignore collisions with sleepers...
                    // this is a temporary fix because the pathfinder cannot see and avoid them. When we get footprints from entities implemented, delete this.
                   /* if (entity.Collidable != null)
                    {*/
                        //collidable = entity.Collidable;

                        entity.DisableCollisions();                                            
                    /*
                        entity.Collidable = null;
                    }*/

                    // sleep...
                    Status = Goals.Status.Active;
                                       
                }

                if (Status != Status.Failed)
                {
                    Status = Goals.Status.Active;

                    if (isSleeping)
                    {
                        Need need = entity.BiologicalEntity.Needs.NeedsList.Values.FirstOrDefault(n => n.NeedType.SleepNeedType != null); // entity.BiologicalEntity.Needs.NeedsList["sleep"];

                        //TODO: if we implement night shortening, we need to recharge sleep faster then...

                        if (conditionRegulator.IsReady())
                        {
                            Entity insideBuilding;
                            if (!entity.GetContainedBy(out insideBuilding))
                            {
                                Status = Goals.Status.Failed;

                                //ExitIfFailedOrCompleted();
                                return; // Status;
                            }

                            ComputeSleepConditions(entity, insideBuilding, out maximumSleepGainFromThisLocation, out increaseAmountPerDay);
                        }

                        need.CurrentLevel = Common.IncreaseValueBetweenZeroAndTopLimit(need.CurrentLevel, increaseAmountPerDay, elapsed.ElapsedGameTime.TotalSeconds, maximumSleepGainFromThisLocation);

                        if (testTimeToWakeupRegulator.IsReady())
                        {
                            if (IsItTimeToWakeup())
                            {
                                // rise and shine...
                                Status = Goals.Status.Completed;

                                if (entity.PersonEntity != null) // && entity.Intelligence.Allegiance.AllegianceType == Allegiances.AllegianceType.Player)
                                {
                                    The.Client.AddLogEvent(entityIntelligence.Allegiance, The.Client.Log.GeneralEvent, entity, "wakes up");

                                }

                                // add 'wake up' anim - does this work???
                                //entityIntelligence.Brain.AddSubgoal(new GoalWait(entity, 2, AnimAction.Sleeping, AnimModifier.Post, true));

                            }
                        }
                    }
                }    
         /*   }

            ExitIfFailedOrCompleted();

            return Status;*/
        }




        private bool IsItTimeToWakeup()
        {
           // return true; // #sleepToStand test - uncomment to test anim of sleeper standing up

            // weigh our need to sleep against the time of day to determine if we should wake up
            double sleepScore = ((EvaluateSleep)GoalEvaluator).ScoreSleepNeed();

            
            // guidelines: don't stay asleep during the wrong time of day, even though we could still use some sleep. 
            // during the right time on the other hand, keep sleeping...
            if (sleepScore < 0.5f)
            {
                // we have slept enough to consider waking up...
                double timeOfDayScore = GoalEvaluator.ScoreTimeOfDay();
               
                if (timeOfDayScore < 0.05f)
                {
                    // it is the wrong time of day to sleep. Wake up!
                    return true;
                }

            }

            return false;
        }

        public override void OnExit()
        {
            base.OnExit();

            entity.Renderable.ClearAnimationActionStateFlag(AnimAction.Sleeping); 

            entityIntelligence.IsAwakeAndActive = true;

            // HACK: restore collisions...          
            EnableCollisions(true);

        }


        public override bool HandleMessage(Message message)
        {
            switch(message.MessageType)
            {
                case Message.MessageTypes.WakeUpCombatAlert: // we can be awakened by guards etc...
                    {
                        // check if we already got this message. then ignore it:
                        if (entityIntelligence.Memory.RecentlyGotCombatAlert())
                        {
                            return false; // keep sleeping, but allow upper goals to see the message also.
                        }
                        else
                        {
                            entityIntelligence.Memory.SetTimepointForCombatAlert();
                            Status = Status.Completed;

                            return true;
                        }
                    }
                case Message.MessageTypes.AlertToPresence: // wake up by loud attack noises etc.
                    {
                        Status = Goals.Status.Completed;

                        return false; // continue processing
                    }
                case Message.MessageTypes.Hit:
                    {
                        Status = Status.Completed;       // wake up now if someone hits us...         
                        return false; // continue processing
                    }
                case Message.MessageTypes.CancelJobForAIReset:
                    {
                        // Status NOT failed - we continue as if nothing has happened.

                        return true;
                    }
            }

            // return false:
            return base.HandleMessage(message);
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
           
            this.maximumSleepGainFromThisLocation = sn.DoFloat(maximumSleepGainFromThisLocation);
            this.increaseAmountPerDay = sn.DoFloat(increaseAmountPerDay);
            this.isSleeping = sn.DoBool(isSleeping);

            sn.Ignore(testTimeToWakeupRegulator);

            return this;
        }


        public override void LoadPostProcess(Snapshotter sn)
        {
            base.LoadPostProcess(sn);

        }

        #endregion
    }
}
