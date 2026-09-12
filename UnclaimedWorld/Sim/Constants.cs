using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AllGameData.Constants;
using UWGame.SimSide.Entities;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide
{
    public class Constants : IGameDataObject
    {
        public int StartingYear = 2238;


        public float VehicleSpeedCausingMaximumDust = 60f;
        public float SkimmerDustScale = 0.3f;
        public float VehicleDustScale = 0.1f;


        /// <summary>
        /// In Kelvin
        /// </summary>
        public float MeanAmbientTemperature = 288; 
       

        /// <summary>
        /// the limit for a path to take effect as traversal cost
        /// </summary>
        public float PathActivation = 0.6f;
        public float AmountToAddToPathOnTraversal = 0.1f; 

        public float TilesBetweenAddedWaypoints = 1.5f;

        /// <summary>
        /// bulk limits for which a tree blocks movement
        /// </summary>        
        public float BulkOfTreeBlockingCarTransport = 4f;        
        public float BulkOfTreeBlockingATVTransport = 6f;
        public float BulkOfTreeBlockingFootTransport = 8f;

        //Quick pulsing variables
        public float FlashDuration = 2000f;
        public Color FlashingColorWhenClicked = Color.Aquamarine;
        public Color FlashingColorWhenDetected = Color.White;


        public float AverageAgentWatingTimeToTriggerAlert = 4f;
        public float NumberOfAgentsWatingToTriggerAlert = 2;
        

        //
        public float StoredPercentageMeansHauling = 0.4f;
        public float StoredPercentageMeansHeavyHaul = 0.6f;
        public float DistanceForLongHaul = 400f;


        public float MinimumSpeedForTerrainToHaveEffect = 16f;

        /// <summary>
        /// the squared distance from the agent to the item/waypoint considered "close enough"
        /// </summary>
        public float DistanceSquaredLimitForWaypoints = 4f;

        /// <summary>
        /// the distance at which an agent interacts with items on the ground
        /// </summary>
        public float InteractionDistanceForAgents = 12f; 

        public float BulkCapacityForSingleTile = 35f;
        public float BulkCapacityForSubTile = 1.1f;


        public float RottingSpeedOfOrganicFibers = 0.01f;
        public float DryingSpeedOfOrganicFibers = 0.01f;
        public float RottingSpeedOfOrganicMaterial = 0.02f;
        public float DryingSpeedOfOrganicMaterial = 0.01f;

        public float PhosphorusAmountInOrganicMaterial = 0.05f;
        public float NitrogenAmountInOrganicMaterial = 0.05f;


        //**** combat
        public float MeleeToHitBonusOnDistractedTarget = 0.2f;
        public float MeleeToHitBonusOnProneTarget = 0.25f;
        public float FractionOfHitpointsCausingCollapse = 0.20f;

        public float DamageAmountFractionCausingHitReaction = 0.005f;

        public int TimeIntervalForDPSMoraleFactor = 5;

        public float MoraleDamageForZeroDamageAttacks = 0.001f;
        public float vitalBodyPartDamageEvaluationBoost = 15f;   //MP: I put this up to 3000f and didn't notice any change. 15f higher number means higher tendency to flee when vital bodypart (trunk, head) is being hit.

        public float ChanceToDropWeaponWhenFleeing = 0.5f;

        public float CloseDistanceForRangedAttack = 96f;
        public float CloseDistanceRangedAttackToHitFactor = 1.4f;

        #region Hitpoints
        
        public float FractionOfMaxHitpointsLostPerSecondWhenDying = 0.05f;

        /// <summary>
        /// just temporary, until the healing feature is up
        /// </summary>
       /* public float FractionOfMaxHitpointsGainedPerSecond = 0.005f;

        /// <summary>
        /// 1: full regen is possible
        /// 0.5: max 50% of total wounds can be regained
        /// 0: no regen
        /// </summary>
        public float MaxRegainLimit = 0.5f;*/

        #endregion


        public NormalDistribution ExpertSkillDistribution = new NormalDistribution() { Mean = 1d, StandardDeviation = 0d };
        public NormalDistribution HighSkillDistribution = new NormalDistribution() { Max = 0.99f, Min = 0.6f };
        public NormalDistribution MediumSkillDistribution = new NormalDistribution() { Max = 0.7f, Min = 0.3f };
        public NormalDistribution LowSkillDistribution = new NormalDistribution() { Max = 0.4f, Min = 0.1f };
        public NormalDistribution ZeroSkillDistribution = new NormalDistribution() { Max = 0.099f, Min = 0f }; //mp jan 2016: 0.1f is the minimum requirement for doing a task. so zeroskill means he cannot do it.


        public float ExpertSkillBonus = 0.25f;
        public float ExpertSkillBonusThreshold = 0.95f;




        #region trade profiles
        public NormalDistribution HighTradeAmountDistribution = new NormalDistribution() { Max = 3f, Min = 2f };
        public NormalDistribution MediumTradeAmountDistribution = new NormalDistribution() { Max = 2f, Min = 0.6f };
        public NormalDistribution LowTradeAmountDistribution = new NormalDistribution() { Max = 0.5f, Min = 0.2f };
        //public NormalDistribution OccasionalTradeAmountDistribution = new NormalDistribution() { Max = 0.4f, Min = 0.1f };

        /// <summary>
        /// relative factors
        /// 
        /// can  be overridden in TradeProfile
        /// </summary>
      /*  public float DefaultMaxAmountStdDev = 0.1f;
        public float DefaultPriceStdDev = 0.1f;
        public float DefaultProduceAmountStdDev = 0.1f;
        public float DefaultConsumeAmountStdDev = 0.1f;
        */

        #endregion

        public float ZeroEnergyProductionFactor = 0.4f; 
        public float ZeroEnergyToHitFactor = 0.7f;
        public float ZeroEnergyMeleeDamageFactor = 0.7f;
        public float ZeroEnergyIdleWalkFactor = 0.2f;

        /// <summary>
        /// clamps the bottom of the skill and energy factors. 
        /// The job has to be able to finish on max 1 BLK of fuel, because more than one fuel haul isn't implemented
        /// This is done partly to avoid having to refuel tools like the smithy... which isn't implemented.
        /// </summary>
        public float LowestCombinedSkillAndEnergyProductionFactors = 0.32f;


        public float OxygenEnergyRequiredToStartRunning = 0.2f;
        
        // production:
        public float ToolUseDestroyChanceInterval = 1f;

       // public float PowerCoefficientForRandomChanceOfBreakdown = 80f;  //high number means low randomness

        public int ExpeditionStorageRadius = 3;

        // use container classes to make names shorter
        // needs
        public SleepNeed SleepNeed = new SleepNeed();

      
        
        public PhysicalWork PhysicalWork = new PhysicalWork();

        // detection
        public float DefaultDistanceToAlwaysDetectHiddenEntities = 48f;
        public float DefaultDistanceToAlwaysDetectResources = 28f;
        public float DetectionUpdatesPerSecond = 0.5f;


        public float ConditionDamageMeanToContentsOfDestroyedContainers = 0.2f;
        public float ConditionDamageSpreadToContentsOfDestroyedContainers = 0.2f;

        public float DefaultPassiveStealthFactor = 0.2f;

        public float BestDetectionFactorOfActivityWhenNotLooking = 0.5f;
        public float BestDetectionFactorOfActivityWhenSearching = 3f;
               
        //

        public float ChanceToExultAfterWinning = 0.4f;

        // head turns etc.
        public float InterestLevelForConversation = 15f;

        public float InterestLevelForCollidedEntityMean = 10f;
        public float InterestLevelForCollidedEntityStdDeviation = 2f;
        
        public float InterestLevelForSpottedEntityMean = 40f; // 20f can dwell longer at entities
        public float InterestLevelForSpottedEntityStdDeviation = 3f;

        public float InterestLevelForSpottedResourceMean = 4f; // only look briefly at resources. more natural.
        public float InterestLevelForSpottedResourceStdDeviation = 1.2f;

        //public float InterestLevelDropOffPerFrame = 0.003f;
        public float InterestLevelDropOffPerSecond = 2f;

        /// <summary>
        /// an inertia margin to avoid dithering
        /// </summary>
        public float InterestInertiaFactor = 1.28f;

        public float InterestLevelForTurnToFace = 60f;

        public float MaxHeadTurnAngleForZeroInterest = MathHelper.PiOver4;
        public float MaxHeadTurnAngleForMaxInterest = MathHelper.PiOver2; //  + MathHelper.PiOver4;

        // oxygen/muscle energy
        public float DecreaseInOxygenMuscleEnergyWhenRunningPerSecond = 0.1f;


        public float CollisionResistanceFromNonMovers = 0.005f; //0.02f //0.08f// 0.2f    // ...used to be defined by the word Push Strength
        public float CollisionResistanceFromMovers = 0.3f;     //0.7f  //1.3f                  ...used to be defined by the word Push Strength


        public float MinimumSkillValueToUse = 0.1f;


        public float MinimumAgentBulk = 0.1f;  // the minimum bulk required for animal agents in the game
        public float InterestLevelToCauseBodyTurn = 30f;
        
        public int TalkSpeedInCharactersPerSecond = 17; // 20;
        public float MinimumTalkDurationInSeconds = 3f;
        public float MaximumTalkDurationInSeconds = 8f;

     //   public float MinimumInterestLevelForCollidedEntity = 0f;
     //   public float MaximumInterestLevelForCollidedEntity = 70f;


        /// <summary>
        /// the ratio between two numbers affects the pattern of the threat maps
        /// </summary>
        public SerializableDictionary<StrengthRating, float> StrengthRatings = new SerializableDictionary<StrengthRating, float>()
       {//MP may 6 2014, problem is that thunders didnt flee from humans.
           { StrengthRating.None, 0f }, // added this for binal rats
           { StrengthRating.VeryWeak, 0.5f }, 
           { StrengthRating.WeakerThanHumans, 1f },
           { StrengthRating.LikeHumans, 2f },
           { StrengthRating.StrongerThanHumans, 3f },
           { StrengthRating.VeryStrong, 4f },

       };


        public int PopulationCap = 25;
       


        /// <summary>
        /// in kms
        /// 6371d Makes 1 degreee = 50 km
        /// </summary>
        public double DefaultWorldRadius = 6371d;
       
        /// <summary>
        /// km/day
        /// </summary>
        public float AverageOverlandSpeedOnFoot = 40;
        public float ExpertSkillLevel = 0.9f;


        /// <summary>
        /// in seconds, controls morale etc.
        /// </summary>
        public float UpdateIntervalForEntityComponents = 5f;

        /// <summary>
        /// controls needs, stomach etc
        /// </summary>
        public float UpdateIntervalForBioEntity = 3f;

       // public float SlowUpdateIntervalForBioEntity = 10f;

        /// <summary>
        /// degradation
        /// </summary>
        public float UpdateIntervalForNonLivingTypes = 10f;

        public float VisualCommunicationRangeInKms = 13; ///mp: used to have VisualCommunicationRangeInKms = 10  (yelling distance)but was too small to make it work on the Fields map, so increased it


        public float DefaultSpawnRadius = 240f;
        
        
        public float SellPriceModifier = 1.5f;

      //  public float PeriodBeforeFirstEmigrateRollInSeconds = 120f; 

        public void Initialize()
        {

        }

        public void PostDataCompleteInitialize()
        {

        }
    }
}
