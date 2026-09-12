using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Items;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Entities.Locomotors;
using UWGame.Control;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Systems.Triggers;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Entities.Locomotors.Stances;
namespace UWGame.SimSide.AI.Goals
{
    public enum Status {Inactive, Active, Completed, Failed }

    public enum GoalID : ulong // needed for references to a parent goal.
    {
        Invalid = uint.MaxValue,
        Max = Invalid,
        First = 1
    }

    public abstract class Goal : ISnapshot, ILookUp<Goal, GoalID> 
    {

        public Entity entity;
        EntityID snapshotEntity;

        protected Intelligence entityIntelligence;
        
        /// <summary>
        /// can be null!
        /// </summary>
        protected Person personEntity;

        private Status status = Status.Inactive;
        public Status Status
        {
            get { return status; }
            set
            {
                if (value != status)
                {
                    if (value == Goals.Status.Completed)
                    {

                    }

                    if ((value == Goals.Status.Failed /*|| value == Goals.Status.Inactive*/
                          && !(this is GoalDoTakeFive)
                          && !(this is GoalTakeFive)
                          && !(this is GoalDoProduceAtomic)
                          && entity != null && entity.Name != null && (entity.Name.Contains("Bob") || entity.Name.Contains("nez")))) ///&& entity.ID == (EntityID)5142/* entity.PersonEntity != null
                        /*  && The.Sim.TotalUnPausedGameTimeInSeconds > 889*/
                    {
                        int i = 0;
                    }

                    status = value;
                    //  if (status )
                }
            }
        }

        protected double? delayPeriodInSeconds;
        protected double delayProgress = 0d;

        /// <summary>
        /// Only for top level goals.
        /// this value affects inertia (the tendency not to switch goals)
        /// </summary>
    //    public double TimeSpentInTopLevelGoal = 0;

        /// <summary>
        /// Decremented by elapsed time in base.Process(), for Goals with any time-based behavior
        /// </summary>
        public double TimeLeftInSeconds
        {
            get;
            set;
        }

        /// <summary>
        /// we need this to recalculate the goal score.
        /// </summary>
        public GoalEvaluator GoalEvaluator;
        int? snapshotGoalEvaluatorIndex;
      
        public Goal(Entity entity)
        {           
            Init(entity);

        }


        protected Goal() { }

        /// <summary>
        /// called on goals from the pool when they are used!
        /// </summary>
        /// <param name="entity"></param>
        protected void Init(Entity entity)
        {
            AddToLookup(); // we don't want to give IDs to goals still in the pool

            hasTerminated = false;
            hasEntered = false;
            hasActivated = false;

            Status = Status.Inactive;
            delayProgress = 0d;
            delayPeriodInSeconds = null;
            TimeLeftInSeconds = 0;
            //TimeSpentInTopLevelGoal = 0;
            this.entity = entity;
            this.entityIntelligence = this.entity.Intelligence;
            this.personEntity = this.entity.PersonEntity; // can be null!

            CreateRegulators();
        }

        public enum MovementSpeeds { WalkSlowly, Normal, WalkFast, Run, Haul };
      /*  public virtual MovementSpeeds GetMovementSpeedType()
        {
            return MovementSpeeds.Normal;
        }*/

        protected bool AreThereNonMovingEntitiesInThisSpot()
        {
            // fail if there are other entities in this spot and we are in the open!
            if (entity.ContainedBy == null) // entity.InsideBuilding == null && entity.InsideVehicle == null)
            {
                if (MapManager.IsStandingOnNonMovingEntity(entity))
                {
                    // check that there are actual available spots inside the tile that we can move to. else do nothing.
                    if (The.Map.FindUnoccupiedSubtileInsideTile(entity, entity.MapPosition.Value) == null)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }

            return false;
        }

       
      /*  public virtual double GetTimeSpentInGoal()
        {
            return TimeSpentInGoal;
        }*/


        protected bool UsedEntityResultShouldFailGoal(EntityResult result)
        {
            if (result == EntityResult.Destroyed || result == EntityResult.EntityStatusIsNowUnknown)
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// sets the Status to Failed and returns true if the result is not usable.
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        protected bool EntityResultCausesFailedGoal(EntityResult result)
        {
            if (UsedEntityResultShouldFailGoal(result))
            {
                Status = Goals.Status.Failed;
                return true;
            }
            else return false;
        }

        protected bool AreToolsOK(Entity entity, List<EntityID> Tools)
        {
            bool toolsAreOk = true;
            IKnownEntityData toolData;

            foreach (var tool in Tools)
            {
                if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(tool, out toolData)))
                {
                    return false;
                }

                if (!ToolDistanceIsValid(entity.PlaySiteLocation, toolData))
                {
                    toolsAreOk = false;
                    break;
                }

              /*  if (Common.DistanceOctile(entity.PlaySiteLocation, toolData.AccessPoint.Value) > 80f
                    && Common.DistanceOctile(entity.PlaySiteLocation, toolData.PlaySiteLocation) > 80f) // campfire can be far away...
                {
                    toolsAreOk = false;
                    break;
                }*/

                if (toolData.EntityType.ToolType.PrepareProcess != null)
                {
                    // is the fire still going?
                    if (toolData.IsPrepared != true)
                    {
                        toolsAreOk = false;
                        break;
                    }
                }
            }

            return toolsAreOk;
        }


        public static bool ToolDistanceIsValid(Vector3 workerLocation, IKnownEntityData toolData)
        {
            if (Common.DistanceOctile(workerLocation, toolData.AccessPoint.Value) > 80f
                    && Common.DistanceOctile(workerLocation, toolData.PlaySiteLocation) > 80f) // campfire can be far away...
            {
                return false;
            }

            return true;

        }

        /// <summary>
        /// sets the Status to Failed and returns true if the entity is not visible. Also runs OnExit if necessary (OnExit can never run twice.)
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        protected bool EntityIsNotSeenDirectly(EntityID entityID, out Entity entity)
        {
            entity = null;
            IKnownEntityData entityData;
            EntityResult result = entityIntelligence.GetKnownData(entityID, out entityData);
            if (result != EntityResult.SeenDirectly)
            {
                Status = Goals.Status.Failed;
                
                ExitIfFailedOrCompleted();

                return true;
            }

            entity = (Entity)entityData;
            return false;
        }

        protected void SetProcessAnimStates(ProcessType processType)
        {

            entity.Renderable.SetAnimationActionStateFlag(processType.AgentActionState);
            if (processType.AgentAnimationStates != null)
            {
                foreach (var flag in processType.AgentAnimationStates)
                {
                    entity.Renderable.SetAnimationStateFlag(flag);
                }
            }
        }
        protected void ClearProcessAnimStates(ProcessType processType)
        {
            entity.Renderable.ClearAnimationActionStateFlag(processType.AgentActionState);
            if (processType.AgentAnimationStates != null)
            {
                foreach (var flag in processType.AgentAnimationStates)
                {
                    entity.Renderable.ClearAnimationStateFlag(flag);
                }
            }
        }

        public override string ToString()
        {
            return GetType().ToString().Remove(0, GetType().ToString().LastIndexOf(".") + 1) + " (" + Status.ToString() + ")";          
        }

        public virtual string GetStatus()
        {
            return "";
        }

        public virtual string ComposeIndentedString(string indent)
        {
            return indent + ToString();
        }

        public void ActivateIfInactive()
        {           
            if (isInactive())
            {
              /*  if (job.RequiresBoldStance) // TODO: factor this code out by adding a job field to Goal - also set lock on job
                {
                    entity.Intelligence.SetBoldStance();
                }	*/
                
                Activate();

                hasActivated = true;
            }
        }

        //if m_iStatus is failed this method sets it to inactive so that the goal
        //will be reactivated (replanned) on the next update-step.       
        protected void ReactivateIfFailed()
        {
          if (HasFailed())
          {
             Status = Status.Inactive;
          }
        }

        protected bool IsDelayed(GameTime elapsed)
        {
            if (delayPeriodInSeconds != null)
            {
                delayProgress += elapsed.ElapsedGameTime.TotalSeconds;

                if (delayProgress > delayPeriodInSeconds)
                {
                    // signifies that delay is over:
                    delayPeriodInSeconds = null;
                    delayProgress = 0;
                    return false;
                }
                return true;
            }
            return false;
        }

        protected Regulator preconditionsRegulator;

        protected virtual void CreateRegulators()
        {
            preconditionsRegulator = new Regulator(The.Sim.GameplayRandomGenerator, 2, "GoalPreconditions");
        }

        protected virtual bool ArePreconditionsOK()
        {
            return true;
        }

        protected void StartDelay(double seconds)
        {
            delayPeriodInSeconds = seconds;
        }

        protected double GetCurrentGoalScore()
        {
            double? score = entityIntelligence.GetCurrentGoalUtility();
            if (score != null && score.HasValue)
            {
                return score.Value;
            }

            return 0;           
        }

        /*
        public double ScoreTopLevelGoal()
        {
            // added this condition because a failed goal may no longer be in state to score itself
            //- for instance a hauling job will have been cancelled and the item unassigned
            //- this will cause a crash if try to score it.
            if (Status == Goals.Status.Failed
                 || Status == Goals.Status.Completed)
            {
                return 0.0;
            }
            else
            {
                double score = ScoreThisTopLevelGoal();


                double inertia = ScoreInertia(); // only add to the top level goal score!!

                score += inertia;

                return score;
            }
           
        }
        */
       


              
       
        protected void ResetThreatStance(Job job)
        {
            if (job.RequiresBoldStance)
            {
                entity.Intelligence.ResetThreatStance(); // needed?
            }
        }

        protected double ScoreJobGoal(Job job, ToolParams? toolParams = null, AttackParams? attackParams = null, HaulingParams? haulingParams = null) //  List<Entity> tools = null, float? productivityOfTools = null)
        {
          /*  if (entity.PersonEntity != null && entity.Name.Contains("Augustine Yeboah"))
            {

            }*/

            double result = 0;
           
            ThreatStance threatStanceToUse;
            RegionMap regionMapToUse = GoalEvaluator.GetRegionMapAndStanceForEvaluator(entity, job, out threatStanceToUse);
          
         
            GoalEvaluator.CalculateResult calcResult =((IScoreJob)GoalEvaluator).ScoreThisJob(regionMapToUse, threatStanceToUse, entity, job, job.TakenBy.Count, null, null, out result, toolParams, attackParams, haulingParams); // tools, productivityOfTools);

            if (calcResult == GoalEvaluator.CalculateResult.Done)
            {
                // cache it:
               // entityIntelligence.CurrentGoalUtility = result;

                return result;
            }
            else
            {
                // we don't have time to wait for the score... return the cached score.
                // also, we want to disregard the answer when it comes back...
                return GetCurrentGoalScore();
            }
        }

        public abstract bool IsSame(Jobs.Job job);

        /// <summary>
        /// top level goals are Activated immediately on construction, to set all locks. Subgoals only when they start processing.
        /// </summary>
        protected abstract void Activate();

        // OLD:
      /*  public virtual Status Process(GameTime elapsed)
        {
            return Status.Active;
        }*/

        public Status Process(GameTime elapsed)
        {
            ActivateIfInactive();
            if (Status == Goals.Status.Active)
            {
                ProcessWhileActive(elapsed);
            }

            ExitIfFailedOrCompleted();

            return Status;
         
        }

        protected abstract void ProcessWhileActive(GameTime elapsed);
       

        protected Status ProcessCountdown(GameTime elapsedTime, double maxTimeToWait, out double scalar)
        {
            Status ret = Status.Active;

            TimeLeftInSeconds -= elapsedTime.ElapsedGameTime.TotalSeconds; //TotalMilliseconds;

            if (maxTimeToWait <= 0)
            {
                scalar = 0d;
                return ret;//can't divide by zero
            }

            if (TimeLeftInSeconds <= 0d)
            {
                TimeLeftInSeconds = 0d;
                ret = Status.Completed;
            }
            else if (TimeLeftInSeconds > maxTimeToWait)
            {
                TimeLeftInSeconds = maxTimeToWait; //clamp (but this should never happen)
            }

            //scalar is assigned the proportion of countdown time to the start time
            scalar = 1d - (TimeLeftInSeconds / maxTimeToWait);

            return ret;
        }


        public virtual void Deactivate() { }

        /// <summary>
        /// Remember to call base when overriding!!!
        /// 
        /// only CompositeGoal should override this method!
        /// </summary>
        public virtual void Terminate()
        {
            //TimeSpentInTopLevelGoal = 0;

            if (!hasTerminated) // only do this once
            {
                if (hasActivated)
                {
                    Deactivate(); // only Deactivate if Activate has been called
                }


                if (hasEntered)
                {
                    OnExit(); // only Exit if OnEnter has been called
                }

                hasTerminated = true;


                //only do these when removed from subgoal list:
             /*   RemoveIDEntry(); // goals in the pool should not have an ID...

                RetireGoal(); // do this regardless of Enter or Activate
              * */
            }
        }

        /// <summary>
        /// only inherit if the goal supports pooling
        /// </summary>
        public virtual void RetireGoal()
        {

        }

       

        protected bool hasEntered = false;
        protected bool hasActivated = false;

        /// <summary>
        /// Remember to call base when overriding!!!
        /// 
        /// Primarily intended to be used for setting anim flags. should be paired with OnExit to de-set the flags.
        /// </summary>
        public virtual void OnEnter()
        {
           // hasEntered = true;

            if (!CanReactToInterest())
            {
                entityIntelligence.ClearCenterOfAttention();
            }
        }

        /// <summary>
        /// NEW: added safeguard to prevent entering twice
        /// </summary>
        public void EnterIfNew()
        {
            if (hasEntered == false)
            {
                OnEnter();

                hasEntered = true;
            }
        }


        protected bool hasTerminated = false;

        /// <summary>
        /// Remember to call base when overriding!!!
        /// Always called BEFORE the OnEnter of a next subgoal of the same parent
        /// 
        /// it can be called if the goal runs to completion or fails
        /// it can also be called when all subgoals are removed (from Terminate). 
        /// no matter what, this method is not called unless the goal has already been entered (activated).
        /// </summary>
        public virtual void OnExit()
        {
          
           // hasTerminated = true;
        }

        protected void ExitIfFailedOrCompleted()
        {
            if (/*hasEntered 
                && !hasExited // safeguard so we don't exit twice
                &&*/
                Status == Goals.Status.Failed || Status == Goals.Status.Completed)
            {  
                Terminate();
            }
        }


        public virtual bool RequiresBoldStance()
        {
            return false;
        }

        //public enum PhysicalWork { None, Light, Medium, Hard }
        public virtual float GetExertionLevel()
        {
            return GameData.Instance.Constants.PhysicalWork.IdleExertionDefault; // .Sitting;
        }

        /// <summary>
        /// Process will call this on all workers
        /// </summary>
        /// <param name="workedTimeInSeconds"></param>
        /// <param name="toolTypeCombination"></param>
        public virtual void PerformWork(SimProcess process, float workedTimeInSeconds, float progressDelta)
        {
        }

        public virtual string GetToolInUseName()
        {
            return null;
        }

        public virtual string GetSkillInUseName()
        {
            return null;
        }

        public virtual float? GetSkillProductivity()
        {
            return null;
        }

        public virtual float? GetToolProductivity()
        {
            return null;
        }

        public virtual float? GetCurrentTotalProductivity()
        {
            return null;
        }

        public enum StealthFactor { None, NotGood, Bad, ExtremelyBad }
        /// <summary>
        /// how much easier are we for other entities to detect because of the activity we are doing?
        /// </summary>
        /// <returns></returns>
        public virtual StealthFactor GetStealthFactor()
        {
            return StealthFactor.None;
        }

        public enum DetectionFactor { CannotDetect, DetectSome, DetectGood, DetectVeryGood }
        public virtual DetectionFactor GetDetectAgentsFactor(EntityType typeOfAgent, bool requiresExamineAction)
        {
            return DetectionFactor.CannotDetect;
        }

        public virtual DetectionFactor GetDetectResourcesFactor(ResourceType resourceType, bool requiresExamineAction) //IResourceItemContainer typeOfResource)
        {
            return DetectionFactor.CannotDetect;
        }

        /// <summary>
        /// some goals do not permit turning the head to track points of interest. this value is used both to set the agent to neutral in when the goal is enteres, and to filter new interest
        /// </summary>
        /// <returns></returns>
        public virtual bool CanReactToInterest()
        {
            return true;
        }

        /// <summary>
        /// true by default
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public virtual bool CanDropRequestedItem(Entity item)
        {            
            return true;
        }


        public virtual Goal GetFrontMostGoal()
        {
            return this;
        }

        
        /// <summary>
        /// though public, not intended to be called from anywhere else but CompositeGoal
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public virtual bool HandleMessage(Message message)
        {
            //goals can handle messages. Many don't though, so this defines a default
            //behavior
            return false; 
        }


        public virtual void AddSubgoal(Goal goal) { throw new Exception("Atomic goals cannot have subgoals."); }
       

        public bool IsCompleted(){return Status == Status.Completed;} 
        public bool isActive(){return Status == Status.Active;}
        public bool isInactive(){return Status == Status.Inactive;}
        public bool HasFailed(){return Status == Status.Failed;}


        public static void FireEventActions(Entity entity, EntityID? targetEntity, AgentActionHooks defaultActionHook, Dictionary<AgentActionHooks, List<ActionSets>> defaultEventActions, 
            AgentActionHooks overridingActionHook, Dictionary<AgentActionHooks, List<ActionSets>> overridingEventActions, bool isSpawning = false)
        {
            // not if consuming...

            // we need to filter talk actions when spawning at start!

            List<ActionSets> defaultActions = null;
            if (defaultEventActions != null)
            {
                defaultEventActions.TryGetValue(defaultActionHook, out defaultActions);
            }

            List<ActionSets> overridingActions = null;
            if (overridingEventActions != null)
            {
                overridingEventActions.TryGetValue(overridingActionHook, out overridingActions);
            }

            // fire triggers, events etc:
            Goal.FireEventActions(entity, targetEntity, 
                defaultActions,
                overridingActions, isSpawning);
        }

        public static void FireEventActions(Entity triggeringEntity, EntityID? targetEntity, List<ActionSets> defaultActionSets, List<ActionSets> overridingActionSets = null, bool isSpawning = false)
        {
            List<ActionSets> actionSetsToUse = null;

            if (overridingActionSets != null && overridingActionSets.Count > 0)
            {
                actionSetsToUse = overridingActionSets;
            }
            else
            {
                actionSetsToUse = defaultActionSets;
            }

            if (actionSetsToUse != null)
            {
                bool isExpired;
                actionSetsToUse.ForEach(a => a.Fire(triggeringEntity, targetEntity, null, out isExpired, isSpawning)); 
            }
        }

        #region Stances

        


       /* protected double ScoreStance(LeggedLocomotor.Stance evaluatedStance, LeggedLocomotor.Stance currentStance, float chanceToUseStanceFactor, float remainInCurrentStanceAddend)
        {
            double random = chanceToUseStanceFactor * The.Sim.GameplayRandomGenerator.NextDouble("Goal");
            double stanceAddend = 0;
            if (currentStance == evaluatedStance)
            {
                stanceAddend = remainInCurrentStanceAddend;
            }

            return random + stanceAddend;
        }*/

      /*  protected void SelectStance(StanceType[] stances, out StanceType stanceToTake) 
        {
          //  LeggedLocomotor.Stance stanceToTake;

            StanceType currentStance = entity.EntityType.LocomotorType.StancesType.DefaultStanceTypeWhenWorking; // LeggedLocomotor.Stance.Standing;
            if (entity.Locomotor.Stance != null)
            {
                currentStance = entity.Locomotor.Stance.CurrentStance;
            }

            // see if the process specifies stances other than default (standing)
            if (stances != null)
            {
                StanceType bestStance = currentStance;
                double bestStanceScore = 0;


                foreach (var stance in stances)
                {
                    double score = ScoreStance(stance, currentStance,
                        1f, 0.5f);

                    if (Common.IsLessThanOrEqual(bestStanceScore, score))
                    {
                        bestStanceScore = score;
                        bestStance = stance;
                    }
                }

                stanceToTake = bestStance;
            }
            else
            {
                // default...
                stanceToTake = LeggedLocomotor.Stance.Standing;
            }

           /*   if (stanceToTake != currentStance)
            {
                // get the flags to set/clear:
              AnimModifier? animFlag = LeggedLocomotor.GetCorrespondingAnimFlag(stanceToTake);
                if (animFlag.HasValue)
                {
                    statesToTake.Add(animFlag.Value);
                }
                Renderable.GetExcludedStanceFlags(animFlag, statesToClear);
                
           
                SetStanceAndAnimFlags(stanceToTake, statesToTake, statesToClear);
            }*/
        /*
        }*/

        /// <summary>
        /// handles interest messages sent from triggers and elsewhere - retrieves the interest value.
        /// The goal should decide whether and how to affect the agent.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        protected float? GetInterestAndHandleTrigger(Message message, out EntityID? entity, out Vector3? location)
        {
            float? interest = null;
            entity = null;
            location = null;

           // Trigger sourceTrigger = message.OtherInfo as Trigger;
            Tuple<Trigger, EntityID?, Vector3?, float?> info = message.OtherInfo as Tuple<Trigger, EntityID?, Vector3?, float?>;
            if (info != null)
            {
                Trigger sourceTrigger = info.Item1;
                if (sourceTrigger != null)
                {                    
                    // sent via trigger:
                   /* if (sourceTrigger.TriggerType.Interest != null
                        && sourceTrigger.TriggerType.Interest.InterestLevel.HasValue)
                    {
                        interest = sourceTrigger.TriggerType.Interest.InterestLevel.Value;
                    }*/
                    Interest interestType =  sourceTrigger.TriggerType.Interest;

                    if (interestType != null)
                    {
                        interest = (float)NormalDistribution.GetRandomValue(The.Sim.GameplayRandomGenerator, interestType.InterestLevelMean, interestType.InterestLevelStdDeviation);
                    }

                    entityIntelligence.SetTriggerCooldown(sourceTrigger, sourceTrigger.TriggerType.CooldownInTicks);
                }
                else
                {
                    // sent from entity (Sensor detected):
                    interest = info.Item4;
                }                

                entity = info.Item2;
                location = info.Item3;

            }

            return interest;           

        }

        #endregion


        #region ISnapshot


        Snapshotter.Version version;
        public virtual Snapshotter.Version DoVersion(Snapshotter sn)
        {
            version = sn.DoVersion(Snapshotter.Version.Original);
            return version;
        }

        public bool IsSnapshotted { get; set; }

        public virtual ISnapshot DoSnapshot(Snapshotter sn)
        {
            this.ID = SnapshotID(sn, ID);
            IDCounter = sn.DoEnum(IDCounter);

            this.snapshotEntity = (EntityID)sn.SnapshotID<Entity, EntityID>(entity);
            if (this.snapshotEntity == EntityID.Invalid)
            {
#if DEBUG || PROFILE

              //  System.Diagnostics.Debug.Assert(false, "Goal.Entity is invalid..."); // not active in Profile
                throw new Exception("Goal.Entity is invalid...");
#endif
                int i = 0;
            }

            this.delayPeriodInSeconds = sn.DoDoubleNullable(delayPeriodInSeconds);
            this.delayProgress = sn.DoDouble(delayProgress);
          //  this.TimeSpentInTopLevelGoal = sn.DoDouble(TimeSpentInTopLevelGoal);
            this.hasEntered = sn.DoBool(hasEntered);
            this.hasActivated = sn.DoBool(hasActivated);
            this.hasTerminated = sn.DoBool(hasTerminated);
            this.status = sn.DoEnum(status);
            this.TimeLeftInSeconds = sn.DoDouble(TimeLeftInSeconds);

            if (sn.mode != Snapshotter.Mode.Load)
            {
                if (GoalEvaluator != null)
                {
                    this.snapshotGoalEvaluatorIndex = entityIntelligence.Brain.GetIndexOfEvaluator(GoalEvaluator);
                }
                else
                {
                    this.snapshotGoalEvaluatorIndex = null;
                }
            }
            this.snapshotGoalEvaluatorIndex = sn.DoInt32Nullable(snapshotGoalEvaluatorIndex);                
            
            sn.Ignore(personEntity);
            sn.Ignore(entityIntelligence);
            sn.Ignore(GoalEvaluator);
            sn.Ignore(preconditionsRegulator);
            
            return this;
        }


        public virtual void LoadPostProcess(Snapshotter sn)
        {

            sn.RegisterLoadPostProcessCall(this);

            entity = Entity.FindByID(snapshotEntity);
            this.entityIntelligence = entity.Intelligence;
            this.personEntity = entity.PersonEntity;
            
            if (snapshotGoalEvaluatorIndex.HasValue)
            {
                GoalEvaluator = entityIntelligence.Brain.GetEvaluatorFromIndex(snapshotGoalEvaluatorIndex.Value);
            }

            CreateRegulators();
        }

        #endregion


        #region ILookup

        private GoalID id = GoalID.Invalid;
        static GoalID IDCounter = GoalID.First;

        public GoalID ID
        {
            get
            {
                return id;
            }

            private set
            {
                if (id != value)
                {
                    if (value == GoalID.Invalid)
                    {

                    }

                    id = value;
                }
            }
        }


        public GoalID GetUniqueID()
        {
            IDCounter++;
            if (IDCounter >= GoalID.Max)
            {
                throw new Exception("Astounding, GoalID just exceeded 64 bits. Something seriously wrong has happened.");
            }

            return IDCounter;
        }

        public GoalID SnapshotID(Snapshotter sn, GoalID id)
        {
            return (GoalID)sn.DoEnum(id);
        }

        public int LoadPostProcessOrder
        {
            get
            {
                return 0;
            }
        }

        public void AddToLookup()
        {
            ID = GetUniqueID();
            if (ID != GoalID.Invalid)
            {
                LookUpGoals.Add(ID, this);             
            }
        }

        public void RemoveIDEntry()
        {
            LookUpGoals.Remove(this);
        }

        void ILookUp<Goal, GoalID>.ResetIDCounter()
        {
        }

        public static void ResetIDCounter() // called by invoke, do not remove
        {
            IDCounter = GoalID.First;
        }


        void ILookUp<Goal, GoalID>.CreateLookupCollection() // interface method - does nothing...
        {
        }

        public static void CreateLookupCollection()
        {
            LookUpGoals.Create();
        }
        
       /* private static void InternalDestroyJob(Job job, bool cancelTakers)
        {
            job.Destroy(cancelTakers);
        }*/

        /// <summary>
        /// Generic because C# does not support polymorphism with the ref keyword!
        ///  
        /// When choosing to cancelTakers make sure you do no longer use the goal after this call as it will get invalidated. 
        /// Do not add any subgoals or any other thing that can cause the snappshotting to crash and this is due to all references to this goal
        /// should now have been removed.
        /// </summary>       
    /*    public static void DestroyJob<T>(ref T job, bool cancelTakers) where T: Job
        {
            //InternalDestroyJob(job, false);

            job.Destroy(cancelTakers);

            job = null;
        }*/
        

        public void SetInvalid()
        {
            id = GoalID.Invalid;
        }

        #endregion
    }
}
