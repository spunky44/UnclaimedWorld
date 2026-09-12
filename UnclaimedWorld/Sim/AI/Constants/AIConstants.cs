using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using UWGame.SimSide.AI.Constants.Rating;
using UWGame.SimSide.Entities;
using UWGame.SimSide.InGameEvents.Conditions;

namespace UWGame.SimSide.AI.Constants
{
    public class AIConstants: IGameDataObject
    {
        public Combat Combat = new Combat();
        public EvaluatorWeights EvaluatorWeights = new EvaluatorWeights();
        public Ratings Ratings = new Ratings();
        public Migration Migration = new Migration();

        public AgentCondition AgentCanVote = new AgentCondition()
        { 
            AllowEmigrating = false, AllowFighting = true, AllowSleeping = true, AllowThreatened = true, AllowTravelling = false, AllowUnconscious = false
        };

        public AgentCondition PolicyCanBeAdopted = new AgentCondition()
        {
             AllowEmigrating = false, AllowFighting = false, AllowSleeping = false, AllowThreatened = true, AllowTravelling = false, AllowUnconscious = false
        };


        public double TimeToWaitBeforeStartingGoal = 0.6; // in seconds

        // Vehicle movement
        public int ShortestRouteThatAcceptsPassengers = 6;// value in tiles: //(6 * 48)^2;
               
        /// <summary>
        /// in pixels
        /// </summary>
        public int ShortestDistanceToConsiderAVehicle = 400;
        
      
        /// <summary>
        /// Tolerance is a bit higher when working.
        /// </summary>
        public byte HighestDiscomfortLevelForWorkToContinue = 16;
        
        /// <summary>
        /// A discomfort level higher than this will disrupt leisure activities or prevent them in that spot.
        /// </summary>
        public byte HighestDiscomfortLevelForLeisureActivityToContinue = 10;

        public byte HighestDiscomfortLevelForLeisureActivityToStart = 4;

        public byte ComfortBonusFromBeingInsideBuildingsOrVehicles = 4;


        public float ResidenceScoreMustBeBetterToMove = 0.05f;

        public double AtomicGoalPeriodInSeconds = 0.2;

        // Arbitration
        // DEBUG VALUES! CHANGE THEM BACK?
        public float NoOfTimesPerSecondToArbitrateWhileBusy = 0.3f; // 1f; //0.3f; // shouldn't run too often in production.

       
    /*    public float AmountNewGoalMustBeBetterToSwitch = 0.15f; //0.1f; //causes dithering btw. construct and haul job!?: 0.1f; //0.015f; 
        public float AmountNewGoalMustBeBetterThanOtherEntityToCancel = 0.05f; // 0.03f; // 0.1f;
        */

        /// <summary>
        /// this value rises with the time we have already spent in the goal (inertia)
        /// </summary>
        public float CurrentGoalInertia = 0.1f;


        /// <summary>
        /// for a short interval, a lower score is required for other agents to take over a job.
        /// in seconds
        /// </summary>
        public float TimeToReachFullGoalSwitchInertia = 3f;

        #region Hunting

        public double CooldownTimeAfterUnsuccessfulHunt = 30d;
        public double IncreaseCooldownTimeFactorAfterUnsuccessfulHunt = 1.5d;       
        public double MaxCooldownTimeAfterUnsuccessfulHunt = 180d;
        public double CooldownTimeAfterSpottingPrey = 45d;

        #endregion

        // MOVEMENT
        public double MaxTimeForSliding = 1.0;
        //public float DistanceToReverseSliding = 4f; 

        public float DistanceSquaredToConsiderOnRoad = 16f;
        public float DistanceToConsiderOnRoad = 4f; 

        // Group movement
        public double DistancePromptingHaltRequest = 200; // 200 pixels
        public double DistancePromptingSlowdownRequest = 200; // 

        // Evaluators
        public float JobWithZeroMaterialsDesirability = 0.0f;
        public double IdleGoalDesirability = 0.01;
        public double EvaluatorTimePenaltyForUsingVehicles = 4.0;

        /// <summary>
        /// when estimating if a tool has enough energy to complete a job, multipy the work time required by this factor
        /// 
        /// Why is this needed? i changed the value...
        /// </summary>
        public float WorkTimeFactorToEvaluateToolEnergyUse = 1.1f; // 4f;

        public float MaxDistanceForReplenishItems = 960f;//480f;

        /// <summary>
        /// If there are materials to last this long with current production speed, we are fully satisfied.
        /// </summary>
        public float LimitInSecondsToMaterialRunoutToMatter = 10f;

        /// <summary>
        /// how large an area relative to the vehicle radius do we consider when looking for landing/parking spots
        /// 3 for a vehicle radius of 1.5 tiles means that an area of 3 * 2 * 1.5 * 2 = 18 by 18 tiles will be searched. 
        /// </summary>
        public int AreaSizeRadiusForFindingParkingSpot = 3;

        public double EmigrateDeciderUpdateIntervalInSeconds = 10d;
       // public int MigrationChecksPerDay = 20;
        public float PeriodAfterJoiningBeforeEmigrateIsPossibleInSeconds = 120f; 


        public int DeprecateMemoryFactsWithinTileRadius = 2;
        public double SecondsToKeepDeprecatedMemoryFacts = 10d;


        public float DistanceFromExpeditionToReturnHome = 400f;
        public float TimeSpentIdlingToConsiderReturningHome = 5f;
        public float TimeSpentIdlingToConsiderReturningHomeWithBoldStance = 25f;
        public float TimeSpentIdlingToConsiderTeleporting = 60f;
        public float MaximumRadiusOfTrapAreaToTeleportFrom = 180f; // 100f;

        
        
        public float MaximumDistanceFromExpeditionToChasePrey = 1200f;

        public double MaxTimeForLeavingPatrolPostUntilFreed = 16;
        public float MaxDistanceOutsidePatrolZoneToChaseTargets = 120f;

        // evaluator weighting
        public float PriorityOfThreatJobs = 100f;
        public float PriorityOfAssetThreatJobs = 4.9f; // vermin jobs. must not be higher than eat prio, else the dog will starve... Must be higher than high prio patrol, otherwise a patroller will never attack vermin.
        public float PriorityOfNeeds = 5f; // eating and sleeping - should be highr than high prio patrol to prevent starving! // OLD: 1f;
        public float PriorityOfEmigrating = 5f;  // should be higher than the highest prio job except threatjobs...     
        public float PriorityOfFindingANewHome = 2.5f; // higher than working and hauling on high prio     
        public float PriorityOfHauling = 1f;
        
        public float PriorityOfWorkJobs = 0.9f;

        public float LowJobModifier = 0.5f;
        public float HighJobModifier = 2f;
      //  public float UrgentJobModifier = 3f;
        //
        public double MinimumThreatRatingToBeAThreatToAgents = 0.45;
        public double AggressionScoreFraction = 0.1;
        public double NearnessScoreFraction = 0.9f;
        public float RecentlyKilledCarcassScoreFraction = 0.5f;

        public float DetectionBonusForRememberedEntitiesInSameSpot = 0.5f;

        /// <summary>
        /// in seconds!!
        /// scouting/hunting:
        /// </summary>         
        public float MinimumChanceToStopAndLookWhenSearching = 0.1f;
        public float ChanceToStopAndLookWhenSearchingFactor = 1f;

        public float TimeToWaitWhenStoppedAndSearchingMean = 3f;
        public float TimeToWaitWhenStoppedAndSearchingStdDev = 1f;


        // foraging:          // MP: works best in zones bigger than 9X9 tiles...smaller than that, and they finish too quickly for it to look realistic. I'm unable to increase the time they spend in a zone!

        public float MinimumChanceToStopAndLookWhenSearchingResources = 0.3f; //0.1f                ok: 0.3f      0.7f   50f
        public float ChanceToStopAndLookWhenSearchingResourcesFactor = 20f;    //1f  3f  6f  10f    ok: 20f             40f

        public float TimeToWaitWhenStoppedAndSearchingResourcesMean = 4f;      //3f                 ok: 4f               9f
        public float TimeToWaitWhenStoppedAndSearchingResourcesStdDev = 2f;    //1f                 ok: 2f               2f


        // statistics:

      //  public double TimeInDaysForMovingAverageFoodStatus = 0.5; // 1.0; // 2;
       
       // public double TimeInDaysForMovingAverageSecurityStatus = 0.5; // 2;
     //   public double TimeInDaysForMovingAverageComfortStatus = 0.4;

        /// <summary>
        /// how often the migrate ratings fluctuate on their own
        /// </summary>
        public float MigrateStabilityFrequency = 0.001f;


        // optional equipment:
        public float JobDistanceFromExpeditionToBringOptionalWeapons = 300; 
        public float JobDistanceFromExpeditionToBringOptionalFood = 300;
        public float JobDistanceFromExpeditionToBringOptionalEquipment = 300; 
        public float MaxRadiusFromExpeditionToGatherOptionalEquipment = 390; // 8 * MapManager.tileSize


        //Shift Sleeping used in EvaluateSleep and GoalSleep  // see also SleepNeed.cs
        /// <summary>
        ///  //Will always go to sleep at this sleep level regardless of how many other agents are sleeping       
        /// </summary>
        public float SleepNeedToIgnoreSleepPolicy = 0.05f;

        /// <summary>
        /// the lower the sleep need -> the more tired.
        /// 
        /// What sleep need an agent needs before actually letting someone else take his place in sleeping. Added to prevent agents constantly swapping sleeping 
        /// OLD: SleepNeedLimitToWantToSleepIfRoomToSleep
        /// </summary>
        public float SleepNeedLimitToSwapWithASleeper = 0.25f;

        /// <summary>
        ///  Will go to sleep if AmountOfSleepingDuringNight is less than sleeping agents or if a sleeping agent has a sleep need above 
        ///  SleepNeedLimitToLetSomeoneElseSleepInstead. Was added to let tired agents sleep if someone had slept for a long while already.
        /// </summary>
        public float SleepNeedLimitToLetSomeoneElseSleepInstead = 0.4f;

        /// <summary>
        /// nutrients score 0: full, 1: starving
        /// </summary>
        public float NutrientsScoreForEating = 0.45f; // public float NutrientsScoreForEatingBeforeDinnerTime = 0.45f;
        public float NutrientsScoreToTriggerStarvedEating = 0.9f;


        public float UpperRangeOfEffectiveDamageInStandardDeviations = 2f;


        /// <summary>
        /// Higher numbers will likely cause the framerate to drop, but will prevent agents/systems waiting forever.
        ///   1 frame, or update, takes 0.016 s
        /// </summary>
        public double TimeAllocatedInSecondsForTimeSlicedSystems = 0.009; //0.0045; //0.0005; to debug pathfinder


        public float PlayerMovementMapUpdateInterval = 5f;
        public float OtherMovementMapUpdateInterval = 7.5f;


        public float ResidenceConditionToStartRepair = 0.9f;
        public float OtherStructureConditionToStartRepair = 0.4f;


        /// <summary>
        /// we need to allow more gather jobs to enable batching by agents
        /// </summary>
        public int JobsPerWorkerCap /* ToTriggerWarning*/ = 5;

        public int MinimumJobsPerStandingOrder = 2;


        public double MinimumSearchTimeInAttackZone = 10;


        public float MaxDistanceBetweenAgentsToAlwaysAllowForceDrop = 160f;
        public byte HighestThreatLevelToAllowForceDrop = 0;
        public float MinimumDistanceToThreatsToAllowForceDrop = 120f;

        /// <summary>
        /// should probably be the minimum sensor range (night)
        /// </summary>
        public float CheckingJobRange = 150f;
        
        /// <summary>
        /// used in food production importance calcs, for consume rate
        /// </summary>
        public string GenericFoodItemKey = "item:smokedCarbonTail";


        public float IntervalInDaysForComputingProductImportance = 0.5f;

        public float MaxDistanceToLookForAdditionalFood = 360f;
        public int MaxAdditionalFoodItems = 5;


        public float MaxAdditionalTimeForStarvedHaulingJobScore = 120f;
        public double TimePassedForHaulingJobsToScoreHigher = 300f;


        [XmlIgnore]
        public EntityType GenericFoodItem;

        public void Initialize()
        {

        }

        public void PostDataCompleteInitialize()
        {
            GenericFoodItem = GameData.Instance.AllEntityTypes[GenericFoodItemKey];
        }
    }
}
