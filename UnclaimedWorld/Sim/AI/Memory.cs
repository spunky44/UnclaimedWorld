using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI
{
    /// <summary>
    /// here we store various things the AI might need to keep track of which can't be stored in a Goal
    /// also to create incentives/inertia to select certain goals, without strongly coupling them as subgoals
    /// </summary>
    public class Memory: ISnapshot
    {
        public double TimePointOfFailedLeisureWalkAttempt = 0;

        public TimeSpan TimeSpentIdling;


        /// <summary>
        /// use this to encourage more attacks on the same target... in GoalHunt for instance (when goal hunt is extended this may no longer be necessary).
        /// </summary>
        private EntityID? lastTarget;
        private double? timePointOfLastAttackAgainstTarget = null; // the above entity should also be obsolete at some point...
       
        private EntityID? lastFoundPrey;
        private double? timePointThatPreyWasFound = null; // the above entity should also be obsolete at some point...

        private EntityID? lastHuntedAnimalCarcass;
        private double? timePointThatCarcassWasCreated = null;


        private Dictionary<EntityID, double> lastHauledItems = new Dictionary<EntityID,double>();
        //private double? timePointForHauling = null; 


        const double recentlyHitByEntityMemoryDuration = 5.0;
        EntityID? attackerID;
        private double? timePointThatWeWereLastHit;

        const double timePointForLastCombatAlertMemoryDuration = 10.0;
        private double? timePointForLastCombatAlert;

        /// <summary>
        /// used to prevent agents from leaving/emigrating before some time has passed
        /// </summary>
        private double? timePointForJoiningExpedition;


        /// <summary>
        /// used to allow attacks against vermin when the Patrol job gets abandoned...
        /// </summary>
        private double? timePointForLastPatrolling = null;
        private JobID? lastPatrolJobID = null;


        public AllegianceID? EmigrateTarget
        {
            get;
            private set;
        }


        public Memory()
        {
            if (!Snapshotter.IsSnapshotting)
            {
                CreateRegulators();
            }
        }

        public void RememberAttacker(EntityID entity)
        {
            attackerID = entity;
            timePointThatWeWereLastHit = The.Sim.TotalUnPausedGameTime.TotalSeconds;
        }

        private static bool RetireTimestamp(ref double? timepoint, double duration)
        {
            if (timepoint != null)
            {
                double memoryLifetime = The.Sim.TotalUnPausedGameTime.TotalSeconds - timepoint.Value; // timePointThatWeWereLastHit.Value;

                if (memoryLifetime > duration) // recentlyHitByEntityMemoryDuration)
                {
                    // reset:                  
                    timepoint = null;

                    return true;
                }
            }

            return false;
        }

        List<EntityID> neededItemsForNextGoal = new List<EntityID>();

        public void SetNeededItemForSwitchedGoal(EntityID entityID)
        {
            neededItemsForNextGoal.Add(entityID);
        }

        public void ClearNeededItemsForNextGoal()
        {
            neededItemsForNextGoal.Clear();
        }


        /// <summary>
        /// call this to hold onto items during arbitration switches
        /// </summary>
        /// <returns></returns>
        public bool NeedsItemForSwitchedGoal(EntityID entityID)
        {
            return neededItemsForNextGoal.Contains(entityID);
        }

        public bool WasRecentlyHitBy(EntityID potentialAttackerID)
        {
            EntityID? lastAttackerID = GetLastAttacker();

            return potentialAttackerID == lastAttackerID;

            /*
            if (timePointThatWeWereLastHit != null)
            {
                if (RetireTimestamp(ref timePointThatWeWereLastHit, recentlyHitByEntityMemoryDuration))
                {
                    // reset:
                    attackerID = null;

                    return false;
                }
                else
                {
                    if (attackerID == potentialAttackerID)
                    {
                        return true;
                    }
                }
            }

            return false;   */         
        }

        public EntityID? GetLastAttacker()
        {
            if (timePointThatWeWereLastHit != null)
            {
                if (RetireTimestamp(ref timePointThatWeWereLastHit, recentlyHitByEntityMemoryDuration))
                {
                    // reset:
                    attackerID = null;

                    return null;
                }
                else
                {
                    return attackerID;
                }
            }

            return null;
        }


        public bool RecentlyGotCombatAlert()
        {
            if (timePointForLastCombatAlert != null)
            {
                if (RetireTimestamp(ref timePointForLastCombatAlert, timePointForLastCombatAlertMemoryDuration))
                {                  
                    return false;
                }
                else
                {
                    return true;
                }              
            }

            return false;   
        }

        public void SetTimepointForCombatAlert()
        {
            timePointForLastCombatAlert = The.Sim.TotalUnPausedGameTimeInSeconds;
        }

        public void SetTimepointForJoiningExpedition()
        {
            timePointForJoiningExpedition = The.Sim.TotalUnPausedGameTimeInSeconds;
        }

        /// <summary>
        /// for testing..
        /// </summary>
        public void ResetTimepointForJoiningExpedition()
        {
            timePointForJoiningExpedition = null;
        }

        public bool RecentlyJoinedExpedition()
        {
            if (timePointForJoiningExpedition != null)
            {
                if (RetireTimestamp(ref timePointForJoiningExpedition, GameData.Instance.AIConstants.PeriodAfterJoiningBeforeEmigrateIsPossibleInSeconds))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }

            return false;
        }

        public void SetRecentlyHuntedCarcass(EntityID entity)
        {
            lastHuntedAnimalCarcass = entity;
            timePointThatCarcassWasCreated = The.Sim.TotalUnPausedGameTimeInSeconds;
        }

        public double GetRecentlyHuntedCarcassScore(EntityID carcassToScore)
        {
            return GetTemporarilyIncreasedScore(carcassToScore, ref lastHuntedAnimalCarcass, ref timePointThatCarcassWasCreated, 5.0);
        }

        public void SetRecentlyFoundPrey(EntityID entity)
        {
            lastFoundPrey = entity;
            timePointThatPreyWasFound = The.Sim.TotalUnPausedGameTimeInSeconds;
        }

        /// <summary>
        /// we need to store this in memory since the agent will have to briefly leave his post to fight threats...
        /// </summary>
        /// <returns></returns>
        public CombatAreaJob GetLastCombatAreaJob()
        {
            if (timePointForLastPatrolling.HasValue)
            {              
                if (Common.TimepointIsOutDated(timePointForLastPatrolling.Value, GameData.Instance.AIConstants.MaxTimeForLeavingPatrolPostUntilFreed))
                {
                    // reset
                    timePointForLastPatrolling = null;
                    lastPatrolJobID = null;
                    return null;
                }
                else
                {
                    Job job = LookUp<Job, JobID>.FindByID(lastPatrolJobID);

                    if (job != null)
                    {
                        return (CombatAreaJob)job;
                    }                   
                }
            }

            return null;           
        }

        /// <summary>
        /// reset when we are at the target.
        /// 
        /// Fires a dialog action!
        /// </summary>
        /// <param name="toAllegiance"></param>
        public void SetEmigrateDecision(AllegianceID? toAllegiance, Entity parent)
        {
            EmigrateTarget = toAllegiance;

            if (EmigrateTarget.HasValue)
            {
                List<ActionSets> defaultActionSets;

                parent.EntityType.IntelligenceType.EventActions.TryGetValue(AgentActionHooks.DecidedToLeaveAllegiance, out defaultActionSets); // player only..?

                Goal.FireEventActions(parent, null, defaultActionSets, null);

            }
        }

     
        public void SetRecentlyOnPatrol(JobID? jobID)
        {
            if (jobID.HasValue)
            {
                lastPatrolJobID = jobID;
                timePointForLastPatrolling = The.Sim.TotalUnPausedGameTimeInSeconds;
            }
            else
            {
                lastPatrolJobID = null;
                timePointForLastPatrolling = null;
            }
        }

        public void SetLastAttackTarget(EntityID entity)
        {
            lastTarget = entity;
            timePointOfLastAttackAgainstTarget = The.Sim.TotalUnPausedGameTime.TotalSeconds;

        }

        public double GetRecentlyFoundPreyScore(EntityID target)
        {
            return GetTemporarilyIncreasedScore(target, ref lastFoundPrey, ref timePointThatPreyWasFound, 5.0);

        }

        public double GetLastAttackScore(EntityID target)
        {
            return GetTemporarilyIncreasedScore(target, ref lastTarget, ref timePointOfLastAttackAgainstTarget, 10);

        }

        /// <summary>
        /// used for haulers to encourage hauling the same item when the haul job manager destroys their job and re-assigns the item they are hauling (or going for), 
        /// instead of someone else taking the job (it looks like a job switch, but it is actually a new job).
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public double GetRecentlyHauledItemScore(EntityID target)
        {
            if (lastHauledItems.Count > 0)
            {
                if (cleanupHauledItemsRegulator.IsReady())
                {
                    CleanupLastHauledItems();
                }

                double timepoint;
                if (lastHauledItems.TryGetValue(target, out timepoint))
                {

                    return 1d;// GetTemporarilyIncreasedScore(target, ref lastFoundPrey, ref timePointThatPreyWasFound, 5.0);
                }                
            }

            return 0d;
        }
                    
        

        public void SetLastHauledItem(EntityID entity)
        {
            lastHauledItems[entity] = The.Sim.TotalUnPausedGameTime.TotalSeconds;

        }

        public void ResetHauledItem(EntityID entityID)
        {
            if (lastHauledItems.ContainsKey(entityID))
            {
                lastHauledItems.Remove(entityID);
            }
        }


        private Regulator cleanupHauledItemsRegulator;

        void CreateRegulators()
        {
            cleanupHauledItemsRegulator = new Regulator(The.Sim.GameplayRandomGenerator, 1, "Memory");
        }

        private void CleanupLastHauledItems()
        {
            List<EntityID> itemsToClean = null;
            foreach (var item in lastHauledItems)
            {
                if (The.Sim.TimepointReached(item.Value + 5.0))
                {
                    Common.AddToList(ref itemsToClean, item.Key);
                }
            }

            if (itemsToClean != null)
            {
                foreach (var item in itemsToClean)
                {
                    lastHauledItems.Remove(item);
                }
            }
        }

        private static double GetTemporarilyIncreasedScore(EntityID target, ref EntityID? lastTarget, ref double? timePoint, double duration)
        {
            bool lastTargetIsOutdated = false;

            if (timePoint.HasValue)
            {
                lastTargetIsOutdated = The.Sim.TotalUnPausedGameTimeInSeconds - timePoint.Value > duration;


                if (lastTarget == target)
                {
                    if (!lastTargetIsOutdated)
                    {
                        return 1.0; // attack this again...
                    }
                }

                if (lastTargetIsOutdated)
                {
                    // reset...
                    lastTarget = null;
                    timePoint = null;
                }
            }

            return 0.0;
        }


        #region ISnapshot

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public Snapshotter.Version DoVersion(Snapshotter sn)
        {
            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }

        public bool IsSnapshotted { get; set; }

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            this.attackerID = sn.DoEnumNullable(attackerID);
            this.lastFoundPrey = sn.DoEnumNullable(lastFoundPrey);

            this.lastHuntedAnimalCarcass = sn.DoEnumNullable(lastHuntedAnimalCarcass);
            this.lastTarget = sn.DoEnumNullable(lastTarget);

            this.TimePointOfFailedLeisureWalkAttempt = sn.DoDouble(TimePointOfFailedLeisureWalkAttempt);
            this.timePointOfLastAttackAgainstTarget = sn.DoDoubleNullable(timePointOfLastAttackAgainstTarget);
            this.timePointThatCarcassWasCreated = sn.DoDoubleNullable(timePointThatCarcassWasCreated);
            this.timePointThatPreyWasFound = sn.DoDoubleNullable(timePointThatPreyWasFound);
            this.timePointThatWeWereLastHit = sn.DoDoubleNullable(timePointThatWeWereLastHit);
            this.TimeSpentIdling = sn.DoTimeSpan(TimeSpentIdling);
            this.timePointForLastCombatAlert = sn.DoDoubleNullable(timePointForLastCombatAlert); 
            this.lastPatrolJobID = sn.DoEnumNullable(lastPatrolJobID);
            this.timePointForLastPatrolling = sn.DoDoubleNullable(timePointForLastPatrolling);
            this.timePointForJoiningExpedition = sn.DoDoubleNullable(timePointForJoiningExpedition);

            this.EmigrateTarget = sn.DoEnumNullable(EmigrateTarget);

            this.lastHauledItems = sn.DoDictionary(lastHauledItems); // #HAULAI

            sn.Ignore(neededItemsForNextGoal);

            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            CreateRegulators();
        }

        #endregion

    }
}
