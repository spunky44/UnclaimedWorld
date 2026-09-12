using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Items;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Entities;
using Xclna.Xna.Animation;
using GameStateManagement;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Entities.Locomotors;
using UWGame.SimSide.Collisions;//TODO DECOUPLE
using System.Linq;
using UWGame.SimSide.Systems.Triggers;
using UWGame.Control;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Entities.Locomotors.Stances;
namespace UWGame.SimSide.AI.Goals
{
    // this goal is for briefly idling in the same spot when there is nothing else to do.
    class GoalDoTakeFive : CompositeGoal
    {
        double? duration;
        double timeAlreadyRested = 0;

        public bool TestForDanger = true;

        StanceType stanceToTake = null;

        List<AnimModifier> statesToTake = new List<AnimModifier>();
        List<AnimModifier> statesToClear = new List<AnimModifier>();

        public bool HasTriedToStartConversation = false;
        public bool IsListening = false;


        private const float idleDuration = 5f;


        /// <summary>
        /// either/both arguments can be null.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="duration"></param>
        /// <param name="animationToRun"></param>
        public GoalDoTakeFive(Entity owner, double? duration = null) 
            : base(owner)
        {
            
            this.duration = duration;       
        }



        /*private void SetDuration(float duration)
        {
            if (!this.duration.HasValue)
                this.duration = duration;
        }*/
            

        public GoalDoTakeFive()
        {
        }


        protected override void Activate()
        {           
            entityIntelligence.IsAwakeAndActive = true; // set this as a safety precaution.

            Entities.Locomotors.Locomotor locomotor;
            if (entity.Find(out locomotor))
            {
                // attempt to stop lerping:
                locomotor.CurrentMoveTarget = entity.PlaySiteLocation;
                
                if (entity.HasStance())
                {
                    StanceType currentStance = locomotor.Stance.CurrentStance;  
                  
                    SelectIdleStance(currentStance);                   
                }
            }

            if (duration == null)
                duration = idleDuration; // NEW, use the same duration regardless of stances, states etc.  2f;


            Status = Status.Active;
           
        }

        private void SelectIdleStance(StanceType currentStance)
        {
            StancesType stancesType = entity.EntityType.LocomotorType.StancesType;

            if (entityIntelligence.ThreatStance == ThreatStance.Bold)
            {
                statesToTake.Add(AnimModifier.Bold);

                stanceToTake = stancesType.DefaultStanceType;
               
                return;
            }


           
            bool isAwayFromCamp = The.Map.GetTile(entity.MapPosition.Value).OperatingAreaOf == null
                       || Common.DistanceOctile(entity.PlaySiteLocation, entityIntelligence.CurrentExpedition.Center.Value) > 400f;

            //LeggedLocomotor.Stance[] stancesToSelectFrom;
            ChanceToTakeStance[] stancesToSelectFrom;

            if (isAwayFromCamp)
            {
                statesToTake.Add(AnimModifier.Trouble);
                stancesToSelectFrom = stancesType.IdleStancesAwayFromHome;
            }
            else
            {
                stancesToSelectFrom = stancesType.IdleStancesNearHome;
            }

            StanceType bestStance = entity.Locomotor.Stance.PickRandomStance(stancesToSelectFrom, stancesType.DefaultStanceType);

            
            GetStatesToClear(bestStance, stancesType);
            stanceToTake = bestStance;
            if (bestStance.AnimModifier.HasValue)
            {
                statesToTake.Add(bestStance.AnimModifier.Value);
            }


            if (stanceToTake.CanStartIdleConversation == true && entity.EntityType.IntelligenceType.CanSpeak == true && !HasTriedToStartConversation)
            {
                TryToStartConversation(entity);
            }
          
            ChangeStance(stanceToTake);
        }

        

               
    /*    private void SelectIdleStance(LeggedLocomotor.Stance currentStance)
        {
            if (entityIntelligence.ThreatStance == ThreatStance.Bold)
            {
                statesToTake.Add(AnimModifier.Bold);

               // SetDuration(combatIdleDuration);

                return;

            }

            double sittingScore = 0, kneelingScore = 0, standingScore = 0, lyingScore = 0;
           // LeggedLocomotorType legType = entity.EntityType.LocomotorType.LeggedLocomotorType;
            StancesType stancesType = entity.EntityType.LocomotorType.StancesType;

            bool isAwayFromCamp = The.Map.TileMap[entity.MapPosition.X][entity.MapPosition.Y].OperatingAreaOf == null
                       || Common.DistanceOctile(entity.Location, entityIntelligence.CurrentExpedition.Center) > 400f;

            //LeggedLocomotor.Stance[] stancesToSelectFrom;
            ChanceToTakeStance[] stancesToSelectFrom;

            if (isAwayFromCamp)
            {
                statesToTake.Add(AnimModifier.Trouble);
                stancesToSelectFrom = stancesType.IdleStancesAwayFromHome;
            }
            else
            {
                stancesToSelectFrom = stancesType.IdleStancesNearHome;
            }

            if (stancesToSelectFrom != null) // legType.Stances != null)
            {
                // we want to remain in current stance more often than switching:
                
                if (stancesToSelectFrom.Contains(LeggedLocomotor.Stance.Sitting))
                {
                    sittingScore = ScoreStance(LeggedLocomotor.Stance.Sitting, currentStance, stancesType.IdleChanceToSitFactor, stancesType.IdleRemainInCurrentSitOrStandStanceAddend);
                }                

                if (stancesToSelectFrom.Contains(LeggedLocomotor.Stance.Kneeling))
                {
                    kneelingScore = ScoreStance(LeggedLocomotor.Stance.Kneeling, currentStance, stancesType.IdleChanceToKneelFactor, stancesType.IdleRemainInCurrentSitOrStandStanceAddend);
                }

                if (stancesToSelectFrom.Contains(LeggedLocomotor.Stance.Lying))
                {
                    lyingScore = ScoreStance(LeggedLocomotor.Stance.Lying, currentStance, stancesType.IdleChanceToLayDownFactor, stancesType.IdleRemainInCurrentSitOrStandStanceAddend);
                }
            }

            // standing is default:
            if (stancesToSelectFrom == null || stancesToSelectFrom.Contains(LeggedLocomotor.Stance.Standing))
            {
                standingScore = ScoreStance(LeggedLocomotor.Stance.Standing, currentStance, stancesType.IdleChanceToStandFactor, stancesType.IdleRemainInCurrentSitOrStandStanceAddend);
            }

           

            double maxScore = Math.Max(kneelingScore, Math.Max(lyingScore, Math.Max(standingScore, sittingScore)));

            if (Common.IsEqual(maxScore, standingScore))
            {
                SelectStandingIdleAnim(entity);
            }
            else if (Common.IsEqual(maxScore, kneelingScore))
            {
                // kneel
                stanceToTake = LeggedLocomotor.Stance.Kneeling;
                statesToTake.Add(AnimModifier.Kneeling);
            }
            else if (Common.IsEqual(maxScore, sittingScore))
            {
                stanceToTake = LeggedLocomotor.Stance.Sitting;
                statesToTake.Add(AnimModifier.Sitting);                   
            }
            else if (Common.IsEqual(maxScore, lyingScore))
            {
                stanceToTake = LeggedLocomotor.Stance.Lying;
                statesToTake.Add(AnimModifier.Lying);
            }

            ChangeStance(stanceToTake.Value);

          //  SetDuration(idleDuration); // NEW: use the same duration always.

        }*/


        // OLD:
        /// <summary>
        /// stand up or sit down
        /// </summary>
        /// <param name="currentStance"></param>
        /// <param name="stanceToTake"></param>
     /*   private void InsertBreakDownAnim(LeggedLocomotor.Stance currentStance, LeggedLocomotor.Stance stanceToTake)
        {
            if (currentStance == stanceToTake)
                return;

            if (currentStance == LeggedLocomotor.Stance.Lying || currentStance == LeggedLocomotor.Stance.Sitting)
            {
                if (stanceToTake == LeggedLocomotor.Stance.Standing)
                {
                    AddSubgoal(new GoalWait(entity, 0.6, AnimAction.Idle, AnimModifier.Pre, true));

                    return;
                }
            }


            if (currentStance == LeggedLocomotor.Stance.Standing)
            {
                if (stanceToTake == LeggedLocomotor.Stance.Lying || stanceToTake == LeggedLocomotor.Stance.Sitting)
                {
                    AddSubgoal(new GoalWait(entity, 0.6, AnimAction.Idle, AnimModifier.Low, AnimModifier.Pre, true));

                    return;
                }
            }
        }
        */

        private void GetStatesToClear(StanceType stanceToTake, StancesType stancesType)
        {
            foreach (var item in stancesType.StanceTypes)
            {
                if (item != stanceToTake && item.AnimModifier.HasValue)
                {
                    statesToClear.Add(item.AnimModifier.Value);
                }
            }
        }

       
      /*  private void SelectStandingIdleAnim(Entity entity)
        {
            statesToClear.Add(AnimModifier.Lying);
            statesToClear.Add(AnimModifier.Kneeling);
            statesToClear.Add(AnimModifier.Sitting);

            
            if (entity.EntityType.IntelligenceType.CanSpeak && !HasTriedToStartConversation)
            {
                TryToStartConversation(entity);
            }
            
           
            stanceToTake = LeggedLocomotor.Stance.Standing;
        }*/

        private void TryToStartConversation(Entity entity)
        {
            if (The.Sim.GameplayRandomGenerator.NextDouble("GoalDoTakeFive") < entity.EntityType.IntelligenceType.IdleChanceToTalk)
            {
                List<Collidable<Entity>> idlePersonsNearMe = null;

                The.CollisionManager.GetEntitiesInRange(entity.PlaySiteLocation.ToVector2(), 80f,
                    e => e != entity // not self
                         && e.EntityType.Person != null // must be person
                         && e.Intelligence.IsIdle() // must be idle
                         && !e.Locomotor.IsMovingOrRotating, // standing still
                         ref idlePersonsNearMe);

                if (idlePersonsNearMe != null && idlePersonsNearMe.Count > 0)
                {
                    // talk to closest:
                    //Array.min
                    Entity closestEntity;

                    if (idlePersonsNearMe.Count > 1)
                    {
                        Collidable<Entity> closest = Common.GetMinimum(idlePersonsNearMe, e => Common.DistanceOctile(e.Parent.PlaySiteLocation, entity.PlaySiteLocation));

                        closestEntity = closest.Parent;
                    }
                    else
                    {
                        closestEntity = idlePersonsNearMe[0].Parent;
                    } 
                    
                    HasTriedToStartConversation = true;                   

                    closestEntity.Intelligence.Brain.SendMessage(new Message(entity, Message.MessageTypes.StartConversation, null));

                   
                    // entity.Renderable.SetAnimationStateFlag(AnimState.Slow);
                    // statesToTake.Add(Modifier.Slow);

                    //  SetDuration(idleStandingLongDuration);
                }
            }
        }

        /*
        public static float IdleChanceToTalk = 0.2f;
        public static float IdleChanceToSitFactor = 0.74f; //MP: was 0.42, they never sat at all. 0.8f
        public static float IdleChanceToKneelFactor = 0.8f;
        public static float IdleChanceToStandFactor = 1f;      
        public static float IdleRemainInCurrentSitOrStandStanceAddend = 0.5f;
        */

       // public static float IdleChanceToPlayLongIdle = 0.3f;

        /*
        private double ScoreIdleStance(LeggedLocomotor.Stance evaluatedStance, LeggedLocomotor.Stance currentStance, float chanceToUseStanceFactor, float remainInCurrentStanceAddend)
        {
            double random = chanceToUseStanceFactor * The.Sim.GameplayRandomGenerator.NextDouble("GoalDoTakeFive");
            double stanceAddend = 0;
            if (currentStance == evaluatedStance)
            {
                stanceAddend = remainInCurrentStanceAddend;
            }

            return random + stanceAddend;
        }*/

        /// <summary>
        /// Always called AFTER the OnExit of a previous subgoal of the same parent
        /// </summary>
        public override void OnEnter()
        {
            base.OnEnter();
          //  entity.Renderable.SetAnimationActionStateFlag(AnimAction.Idle);
        }

        /// <summary>
        /// Always called BEFORE the OnEnter of a next subgoal of the same parent
        /// </summary>
        public override void OnExit()
        {
            base.OnExit();

            // clearing the idle flag will usually cause a default filler anim to be picked. However, in RenderAsModel we will filter this anim change away and keep the current anim playing.
            entity.Renderable.ClearAnimationActionStateFlag(AnimAction.Idle);
            entity.Renderable.ClearAnimationStateFlag(AnimModifier.Bold);
            entity.Renderable.ClearAnimationStateFlag(AnimModifier.Kneeling);
            entity.Renderable.ClearAnimationStateFlag(AnimModifier.Sitting);
            entity.Renderable.ClearAnimationStateFlag(AnimModifier.Lying);
            entity.Renderable.ClearAnimationStateFlag(AnimModifier.Talk);
            entity.Renderable.ClearAnimationStateFlag(AnimModifier.Trouble);
            entity.Renderable.ClearAnimationActionStateFlag(AnimAction.Smoking);
         
        }

      
        public override void Deactivate()
        {
            RemoveAllSubgoals(); // why is this needed..?

          //  base.Terminate();

        }



        protected override void ProcessWhileActive(GameTime elapsed)
        {

            //process the subgoals
            Status subgoalStatus = ProcessSubgoals(elapsed);

            if (subgoalStatus == Status.Completed) // we come here after the breakdown anim has played
            {
                ITopLevelGoal newGoal;
                if (entityIntelligence.Brain.ArbitrateWhileBusy(out newGoal, true)) // this is an Idle goal, so we are not really busy!
                {
                    // we have a new, better goal. The goal has substituted all on the stack.
                    HandleSubstitutedGoalByArbitrator();
                }
                else
                {
                    if (entity.Name != null && entity.Name.Contains("Pezal"))
                    {

                    }


                    if (TestForDanger)
                    {
                        if (!ValidateSafetyAndTakeAction(GameData.Instance.AIConstants.HighestDiscomfortLevelForLeisureActivityToContinue))
                        {
                            return; // Status;
                        }
                    }

                    if (Subgoals.Count > 0 && Subgoals.Peek() is GoalDoTakeFiveAtomic)
                    {
                        GoalDoTakeFiveAtomic lastFinishedGoal = (GoalDoTakeFiveAtomic)Subgoals.Peek();
                        // get the elapsed time:
                        timeAlreadyRested = lastFinishedGoal.TimeAlreadyRested;
                    }

                                     
                    if (timeAlreadyRested < duration)
                    {
                        if (ContainerIsTooCrowded()) // test to exit container
                        {
                            AddSubgoal(new GoalExit(entity)); // move outside - keep the outer goal?
                        }
                       /* else if (SpotIsTooCrowded()) // TODO: respect private boundaries
                        {
                            // add GoalMove...
                        }*/
                        else
                        {
                            // keep idling here.
                            // this code will run after each atomic goal finishes...

                            if (entity.HasStance())
                            {
                                entity.Locomotor.Stance.CurrentStance = stanceToTake;  //.LeggedLocomotor.CurrentStance = stanceToTake.Value;
                            }

                            foreach (var item in statesToTake)
                            {
                                entity.Renderable.SetAnimationStateFlag(item);
                            }
                            foreach (var item in statesToClear)
                            {
                                entity.Renderable.ClearAnimationStateFlag(item);
                            }

                            entity.Renderable.SetAnimationActionStateFlag(AnimAction.Idle);

                            AddSubgoal(GoalDoTakeFiveAtomic.GetGoal(entity, timeAlreadyRested, duration.Value));
                        }
                       

                        Status = Status.Active;
                    }
                    else
                    {
                        Status = Status.Completed;
                    }
                }

            }
            else
            {
                Status = subgoalStatus;
            }

        }


        

        /// <summary>
        /// if idling inside a container that is over capacity, move outside
        /// </summary>
        private bool ContainerIsTooCrowded()
        {
            if (entity.ContainedBy.HasValue)
            {
                // make sure that all accupants don't move out in sync
                if (The.Sim.GameplayRandomGenerator.NextDouble("") < 0.08)
                {
                    Entity container = Entity.FindByID(entity.ContainedBy.Value);
                    if (container != null)
                    {
                        int noOfPeopleInside = 0;
                        IGarrison garrison = container.Contains as IGarrison;

                        if (garrison != null)
                        {
                            noOfPeopleInside = garrison.GetNoOfAgentsInside();
                        }

                        int peopleCapacity = container.EntityType.ContainerType.GetCapacityForIdlingPeople();

                        if (noOfPeopleInside > peopleCapacity)
                        {
                            
                            return true;
                        }

                    }
                }
            }

            return false;
        }


      //  public static float TimeToWaitBeforeTurningHeadToListen = 0.5f;
        public static float TimeToWaitBeforeTurningBodyToListen = 0.85f; //1.6f

        public override bool HandleMessage(Message message)
        {
            switch (message.MessageType)
            {
               /* case Message.MessageTypes.DinnerIsReady: // NOT USED!!
                    {
                        // interrupt:
                        Status = Status.Completed;

                        Entity meal = ((Entity)message.OtherInfo);
                        meal.InUseBy = entity.ID;

                        // go for the meal:
                        //clear any existing goals
                        RemoveAllSubgoals();

                        //this is not a CompositeGoal.
                        // see if we can add the goal as next in line:
                        List<Owner> householdAndPrivateOwnersOfVehicles = new List<Owner>();
                        householdAndPrivateOwnersOfVehicles.Add(personEntity.PrivateOwnership);
                        householdAndPrivateOwnersOfVehicles.Add(personEntity.Household.Ownership);

                        entityIntelligence.Brain.AddSubgoal(new GoalEat(entity, meal.ID, null, householdAndPrivateOwnersOfVehicles));

                        return true; //msg handled

                    }*/
                case Message.MessageTypes.StartConversation: // someone wants to talk to us
                    if (!HasTriedToStartConversation && !IsListening)
                    {
                        IsListening = true;

                        // wait a bit before turning body + head, it just looks more natural:
                        AddSubgoal(new GoalWait(entity, TimeToWaitBeforeTurningBodyToListen));


                        // don't turn all the way - in this way, they won't stare at each other forever.
                        AddSubgoal(new GoalTurnToFace(entity, null, message.Sender.EntityID, false, true, GameData.Instance.Constants.InterestLevelForConversation)); // turn and listen
                      
                        // send back a message so the conversation can start:
                        message.Sender.Intelligence.Brain.SendMessage(new Message(entity, Message.MessageTypes.ListenToConversation, null));

                        return true;
                    }
                    break;          
                case Message.MessageTypes.ListenToConversation: // the other part is listening
                    if (HasTriedToStartConversation)
                    {
                       
                        RemoveAllSubgoals();

                        // don't turn all the way - in this way, they won't stare at each other forever.
                        AddSubgoal(new GoalTurnToFace(entity, null, message.Sender.EntityID, false, true, GameData.Instance.Constants.InterestLevelForConversation)); // turn and begin to speak

                        entity.Renderable.SetAnimationStateFlag(AnimModifier.Talk);

                        return true;
                    }
                    break;
                case Message.MessageTypes.Interest:
                    HandleInterestMessage(message);
                    return true;
                case Message.MessageTypes.SpeakLine:
                    HandleSpeakLineMessage();
                    return true;
                case Message.MessageTypes.CancelJobOrItemInUse:
                    return true; // no job is currently performed
                case Message.MessageTypes.CancelJobForAIReset:
                    return true;
            }

            return base.HandleMessage(message);
        }

        private void HandleSpeakLineMessage()
        {
            List<Pair<Entity,Vector2>> nearbyEntities = null;
            The.AgentQuadTree.GetEntitiesInRange(entity.PlaySiteLocation.ToVector2(), 100f,
                e => e != entity 
                    && e.Intelligence.Allegiance == entityIntelligence.Allegiance,
                ref nearbyEntities);

            if (nearbyEntities != null && nearbyEntities.Count > 0)
            {
                // TODO: if there is a group nearby, perhaps face the center (=Centroid)
                AddSubgoal(new GoalTurnToFace(entity, null, nearbyEntities[0].First.EntityID, false, true, GameData.Instance.Constants.InterestLevelForConversation)); // turn and begin to speak

                entity.Renderable.SetAnimationStateFlag(AnimModifier.Talk);
            }
        }

        private void HandleInterestMessage(Message message)
        {
            // for now, only people will show interest...
            if (entity.EntityType.Person == null)
                return;

            if (!HasTriedToStartConversation && !IsListening)
            {
                // if not talking, turn head to something more interesting

                EntityID? interestingEntity;
                Vector3? interestingLocation;
                float? interest = GetInterestAndHandleTrigger(message, out interestingEntity, out interestingLocation);

                if (interest.HasValue)
                {

                    if (entityIntelligence.InterestIsHighEnough(interestingEntity, interestingLocation, interest.Value))
                    {
                        // if very interesting, also turn body:
                        // only when standing...
                       /* bool canTurnBody = true;
                        bool canTurnHead = true;*/

                        bool? canTurnBody = null;
                        bool? canTurnHead = null;
                        
                        if (entity.HasStance()) // locomotor != null && locomotor.LeggedLocomotor != null)
                        {
                            Locomotor locomotor = entity.Locomotor;
                            canTurnBody = locomotor.Stance.CurrentStance.CanTurnBody;
                            canTurnHead = locomotor.Stance.CurrentStance.CanTurnHead;

                         /*   switch (LeggedLocomotor.CurrentStance)
                            {
                                case LeggedLocomotor.Stance.Standing:
                                    canTurnBody = true;
                                    canTurnHead = true;
                                    break;
                                case LeggedLocomotor.Stance.Sitting:
                                case LeggedLocomotor.Stance.Kneeling:
                                    canTurnBody = false;
                                    canTurnHead = true;
                                    break;
                                case LeggedLocomotor.Stance.Lying:
                                    canTurnHead = canTurnBody = false;
                                    break;
                            }*/
                        }

                        if (canTurnBody == true && interest.Value >= GameData.Instance.Constants.InterestLevelToCauseBodyTurn)
                        {
                            bool turnCompletely = The.Sim.GameplayRandomGenerator.NextDouble(null) > 0.3;

                            // add wait goal before turning, to avoid syncing:
                            AddSubgoal(new GoalWait(entity, The.Sim.GameplayRandomGenerator.RandomBetween(0f, 0.9f)));

                            if (entity.Name != null && entity.Name.Contains("Ward"))
                            {
                                int i = 0;
                            }
                            RemoveAllSubgoals();
                            AddSubgoal(new GoalTurnToFace(entity, 
                                interestingLocation != null? interestingLocation.Value.ToVector2() : (Vector2?)null, 
                                interestingEntity, 
                                turnCompletely, true, interest));


                        }
                        else if (canTurnHead == true)
                        {
                            // only turn head:
                            entityIntelligence.SetNewCenterOfAttention(interestingEntity, interestingLocation, interest.Value);
                        }
                    }
                }
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

            this.duration = sn.DoDoubleNullable(duration);
            this.timeAlreadyRested = sn.DoDouble(timeAlreadyRested);
            this.TestForDanger = sn.DoBool(TestForDanger);
            this.stanceToTake = sn.DoGameData(stanceToTake); // sn.DoEnumNullable(stanceToTake);
            statesToTake = sn.DoList(statesToTake);
            statesToClear = sn.DoList(statesToClear);
            this.HasTriedToStartConversation = sn.DoBool(HasTriedToStartConversation);
            this.IsListening = sn.DoBool(IsListening);


           // sn.Ignore(combatIdleDuration);
            sn.Ignore(idleDuration);

           

            return this;
        }

        #endregion

        
    }
}
