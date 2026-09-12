using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Buildings;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Items;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Entities.Containers.Components;
namespace UWGame.SimSide.AI.Goals
{
    public class GoalSleep : CompositeGoal, ITopLevelGoal
    {

        //private bool isGoingHomeToSleep;
        private EntityID? containerToSleepIn;
        private WorldLocation? groundLocationToSleepOn;
        private EvaluateSleep.GroundSleepArea? groundSleepArea;
        private ExpeditionID? expeditionToSleepAt;
        public double TimeSpentInTopLevelGoal { get; set; }

        public GoalSleep(Entity owner, WorldLocation? groundLocationToSleepOn, ExpeditionID? expeditionToSleepAt, EntityID? containerToSleepIn, EvaluateSleep.GroundSleepArea? groundSleepArea, List<EntityGroupID> ownersOfVehicles)
            : base(owner)
        {
            this.expeditionToSleepAt = expeditionToSleepAt;
            this.groundLocationToSleepOn = groundLocationToSleepOn;
            this.containerToSleepIn = containerToSleepIn;
            this.groundSleepArea = groundSleepArea;

            this.ownersOfVehicles = ownersOfVehicles;
        }

       

        public GoalSleep()
        {
        }


        protected override void Activate()
        {
            Status = Status.Active;
          
            //make sure the subgoal list is clear.
            RemoveAllSubgoals();
            // TODO: add expedition parameter - if present get its gathering site and our sleep location
            // TODO: Add Expedition ID and Factory (following the usual pattern) to check that expedition exists
            // TODO: add a location parameter to verify that container/expedition hasn't moved (in that case fail the goal)
            if (expeditionToSleepAt.HasValue || containerToSleepIn.HasValue)
            {
                GoToGatheringSite();
            }
            else if (groundLocationToSleepOn.HasValue)
            {
                // sleep on the ground
                AddSubgoal(new GoalMoveToPosition(entity, groundLocationToSleepOn.Value.ToVector3(), ownersOfVehicles, GoalMoveToPosition.VehicleUse.FreeUpAfterUse) { IsFinalDestination = true });
                
              //  SetAnimationStateGoal();

                AddSubgoal(new GoalDoSleep(entity, (EvaluateSleep)GoalEvaluator));
            }
            else 
            {
                   // just sleep where we are...
             //   SetAnimationStateGoal();
                AddSubgoal(new GoalDoSleep(entity, (EvaluateSleep)GoalEvaluator));
            }
                      
        }
        private void GoToGatheringSite()
        {

            if (expeditionToSleepAt.HasValue)
            {


                if (Expedition.FindByID(expeditionToSleepAt.Value).GatheringSite.CanAddVisitor(ref entity))
                {
                    AddSubgoal(new GoalArriveAsVisitor(entity, expeditionToSleepAt, ownersOfVehicles)
                    {
                        // IsFinalDestination = true
                    });
                }

                AddSubgoal(new GoalDoSleep(entity, (EvaluateSleep)GoalEvaluator));
            }
            else
            {
                IKnownEntityData containerData;
                if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(containerToSleepIn.Value, out containerData)))
                {
                    Status = Goals.Status.Failed;
                    return;
                }


                if (containerData.GatheringSite != null)
                {
                    if (containerData.GatheringSite.CanAddVisitor(ref entity))
                    {
                        AddSubgoal(new GoalArriveAsVisitor(entity, containerToSleepIn.Value, ownersOfVehicles)
                        {
                            // IsFinalDestination = true
                        });
                    }
                }
                else
                {
                    if (containerData.ContainsEntity(entity.ID)) // #CONTAINSFIX
                    {
                        //cleanup containment bug:
                        if (entity.ContainedBy == null)
                        {
#if !RELEASE
                            throw new Exception("Containment error!");
#endif

                            Entity containerEntity = containerData as Entity;
                            if (containerEntity != null)
                            {
                                HomeContainer home = containerEntity.Contains as HomeContainer;
                                if (home != null)
                                {
                                    home.FixContainmentBug(entity.ID);
                                }
                            }
                        }
                    }


                    if (!containerData.ContainsEntity(entity.ID)) // #CONTAINSFIX
                    {
                        // go home and sleep
                        AddSubgoal(new GoalMoveToPosition(entity, containerData.AccessPoint.Value, ownersOfVehicles, GoalMoveToPosition.VehicleUse.FreeUpAfterUse) { IsFinalDestination = true });

                        AddSubgoal(new GoalEnter(entity, containerData.EntityID));
                    }
                }

                //  SetAnimationStateGoal();
                AddSubgoal(new GoalDoSleep(entity, (EvaluateSleep)GoalEvaluator));
            }

        }
        protected override bool ArePreconditionsOK()
        {
            if (containerToSleepIn != null)
            {
                IKnownEntityData containerData;
                if (EntityResultCausesFailedGoal(entityIntelligence.Allegiance.SharedKnowledge.GetKnownData(containerToSleepIn.Value, out containerData)))
                {
                    return false;
                }
            }

            return true;
        }

     /*   private void SetAnimationStateGoal()
        {
            AddSubgoal(new GoalWait(entity, 2d, AnimAction.Sleeping, AnimModifier.Pre, true));

        }*/

        public double ScoreGoal()
        {
            double score = 0d;
            double? sleepNeed = null;

            if (((EvaluateSleep)GoalEvaluator).ScoreSleep(sleepNeed, null, null, expeditionToSleepAt , containerToSleepIn, groundLocationToSleepOn, groundSleepArea, null, ref score) 
                == Goals.GoalEvaluator.CalculateResult.Done)
            {
                return score;
            }
            else
            {
                // we don't have time to wait for the score... return the cached score.
                // also, we want to disregard the answer when it comes back...
                return GetCurrentGoalScore();
            }
        }

        protected override void ProcessWhileActive(GameTime elapsed)
        {
            if (entityIntelligence.IsIndependent())
            {
                Expedition e = entityIntelligence.CurrentExpedition;
                int numberOfAgentsSleeping = e.GetNumberOfSleepingIndependents(); // EvaluateSleep.GetNumberOfSleepingAgents();

                if (numberOfAgentsSleeping > e.NoOfIndependentMembersAllowedToSleep()) // GameData.Instance.Constants.NoOfMembersAllowedToSleep)
                {
                    if (entity.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel > GameData.Instance.AIConstants.SleepNeedLimitToLetSomeoneElseSleepInstead)
                    {
                        Message wakeUp = new Message(Message.MessageTypes.StopGoalSleep);
                        entity.SendMessage(wakeUp);
                    }
                }
            }

            if (Status == Goals.Status.Active)
            {
                //process the subgoals
                if (!preconditionsRegulator.IsReady() || ArePreconditionsOK())
                {
                    Status = ProcessSubgoals(elapsed);
                }
                else
                {
                    Status = Goals.Status.Failed;
                }
            }

        }

        public override void Deactivate()
        {
            
           // entity.PersonEntity.CurrentExpedition.ExpeditionGatheringSite.RemoveVisitor(entity.EntityID);
            
            if(expeditionToSleepAt.HasValue)
            {
                Expedition expedition = Expedition.FindByID(expeditionToSleepAt.Value);

                if (expedition != null)
                {
                    expedition.GatheringSite.RemoveVisitor(entity.EntityID);
                }
            }

            if (containerToSleepIn.HasValue)
            {
                IKnownEntityData containerData;
                entityIntelligence.GetKnownData(containerToSleepIn.Value, out containerData);
                
                if (containerData != null && containerData.GatheringSite != null)
                {
                    containerData.GatheringSite.RemoveVisitor(entity.EntityID);
                }
            }

        }

        public override bool IsSame(Job job)
        {
            return false;
        }

        public override bool HandleMessage(Message message)
        {
            switch (message.MessageType)
            {
                case Message.MessageTypes.StopGoalSleep: // we can be awakened by guards etc...
                    Status = Status.Completed;
                    return true;
            }

            base.HandleMessage(message);
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

            this.containerToSleepIn = sn.DoEntityIDNullable(containerToSleepIn);
            this.expeditionToSleepAt = sn.DoEnumNullable(expeditionToSleepAt);
            this.groundLocationToSleepOn = sn.DoWorldLocationNullable(this.groundLocationToSleepOn);
            this.groundSleepArea = sn.DoEnumNullable(this.groundSleepArea);
            this.TimeSpentInTopLevelGoal = sn.DoDouble(TimeSpentInTopLevelGoal);

            
            return this;
        }

        #endregion
    }
}
