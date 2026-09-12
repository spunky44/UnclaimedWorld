using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.AI.Needs;
using UWGame.SimSide.Entities.Body;
using UWGame.SimSide.Entities.Containers;
using UWGame.ClientSide;
using UWGame.ClientSide.Particles;
using UWGame.Client.Particles;
using Xclna.Xna.Animation;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Entities.Locomotors;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Combat;
using UWGame.SimSide.XmlCollections;
using UWGame.SimSide.Entities.Locomotors.Stances;
using UWGame.SimSide.Systems.Triggers;
using UWGame.SimSide.Communication;
using UWGame.SimSide.Entities.Containers.Components;
namespace UWGame.SimSide.AllGameData
{
    public class CreatureLoader
    {

        public const int sensorRangeHuman = 350; // 7 tiles
        public const int sensorRangeHumanNight = 200; // 4;


        public const float humanMaxRegainLimit = 0.5f;
        public const float humanFractionOfMaxHitpointsGainedPerDay = 0.3f;

        public static void Init(List<EntityType> listOfEntityTypes)
        {

            #region Human Needs

            NeedType babySleepNeed;
            NeedType childSleepNeed;
            NeedType youngAdultSleepNeed;
            NeedType adultSleepNeed;
            NeedType oldSleepNeed;
            NeedType humanFoodNeed;
            NeedType humanProteinNeed;
            NeedType humanMicronutrientsNeed;
            NeedType humanStimulantsNeed;
            CreateHumanNeeds(out babySleepNeed, out childSleepNeed, out youngAdultSleepNeed, out adultSleepNeed, out oldSleepNeed, out humanFoodNeed, out humanProteinNeed, out humanMicronutrientsNeed, out humanStimulantsNeed);

            #endregion


                #region Test Types
                EntityType meshtest = new EntityType("meshtest")
                {
                    Name = "Mesh test",


                    RenderableType = new RenderableType()
               {
                   RenderAsModelType = new RenderAsModelType()
                   {
                       AssetName = "meshtest",
                       ModelScale = 3.5f,
                   }
               }
                };
                listOfEntityTypes.Add(meshtest);





                /*
                                EntityType skinnedtest = new EntityType("entity:skinnedtest")
                                {
                                    Name = "Skinned test",

                                    ThreatCategory = ThreatCategory.ManMade,
                                    RenderableType = new RenderableType()
                               {
                                   RenderAsModelType = new RenderAsModelType()
                                   {
                                       AssetName = "skinnedtest",
                                       ModelScale = 1.5f,
                                   }
                               }
                                };
                                listOfEntityTypes.Add(skinnedtest);
               
                */

                EntityType skinnedtestType = new EntityType("entity:skinnedtest")
                {
                    Name = "Skinned test",
                    ThumbnailSmall = "HUD_thumbnail_diamondBird",                   
                    IsFlyer = true,
                    RenderableType = new RenderableType()
                    {
                        RenderAsModelType = new RenderAsModelType()
                        {

                            ModelScale = 4f,
                            AssetName = "skinnedtest",
                            AnimConditions = new AnimConditionInfo[] 
                        {

 /*                        new AnimConditionInfo()
                            {
                                AnimationSet = new RandomAnimationSet(){ BaseAnimations = new string[]{  "walk"}},//, "gaitJog", "gaitRun" },
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Moving }
                            },
 */                                          
                            new AnimConditionInfo() 
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "idle" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle }
                            }


                        }
                        }
                    },
                    LocomotorType = new LocomotorType()
                    {
                        MaxAngularSpeed = .25f * MathHelper.Pi, // MathHelper.Pi,
                        FourSidedSymmetry = true,
                        MeleeRadius = 20f,

                        LeggedLocomotorType = new LeggedLocomotorType()
                        {
                            TerrainNegateFactor = 1f,
                            WalkSlowSpeed = 10f, // 8f, 
                            WalkNormalSpeed = 25f /*debug only*/, //18f /*proper speed*/, patrician:26f, turnip:11f
                            WalkFastSpeed = 24f, // 18f,//30f, // 63f,
                        },
                        CollisionResponderType = new CollisionResponderType()
                        {
                            AgentCollisionResponderType = new AgentCollisionResponderType()
                        }
                    },
                    SensorType = new SensorType()
                    {
                        Range = 200,// 11,
                        RangeAtNight = 150, // 7
                        DetectionTypeKey = "defaultDetection"
                    },

                    BodyType = GameData.Instance.AllBodyTypes["bird"]
                };

                //patricianType.IntelligenceType = new IntelligenceType()
                //{
                //    IsMobile = true,
                //    AttackTypes = new AttackType[]
                // { 
                //     new AttackType(){ Damage = AttackType.DamageTypes.Piercing }
                // }
                //};

                skinnedtestType.IntelligenceType = new IntelligenceType()
                {
                    IsMobile = true,
                    StrengthRating = Entities.StrengthRating.WeakerThanHumans,
                    AggroRange = 0.0f,
                    MembersScoutingFraction = 0.0f,
                };



                skinnedtestType.BiologicalType = new BiologicalType()
                {
                    OxygenAndMuscleEnergyIncreaseRatePerDay = 10f,
                    TimeToConsumeFullMealInDays = 0.0125f,
                    StomachSizeFractionOfEntityBulk = 0.1f,
                    StomachContentsDecreaseRatePerDay = 2,
                    ActiveStealthRating = 0.3f,
                    MaxRegainLimit = humanMaxRegainLimit,
                    FractionOfMaxHitpointsGainedPerDay = humanFractionOfMaxHitpointsGainedPerDay,
                    Carcass = "item:birdCarcass",
                    RaceTypes = new[]
                                {     
                                    
                                    new RaceType()                              // Beige
                                    { KeyName = "pale", Name="Pale race", PortraitSkinType="Pale", PrimaryColor = "F7F4C5".ToColorVector3(), ModelBasicTextureName = "SkinnedTestTexture", Edge = 0.3f
                                    },
/*
                                    new RaceType()                              // Dark
                                    { KeyName = "dark", Name="Dark race", SkinType="Dark", PrimaryColor = "564920".ToColorVector3(), ModelBasicTextureName = "BirdDarkTexture", Edge = 0.6f
                                    },
                                    new RaceType()                              // 
                                    { KeyName = "yellow", Name="Yellow race", SkinType="Yellow", PrimaryColor = "564920".ToColorVector3(), ModelBasicTextureName = "BirdYellowTexture", Edge = 0.6f
                                    },
                                    new RaceType()                              // 
                                    { KeyName = "red", Name="Red race", SkinType="Red", PrimaryColor = "564920".ToColorVector3(), ModelBasicTextureName = "BirdRedTexture", Edge = 0.6f
                                    },
                                    new RaceType()                              // Black
                                    { KeyName = "black", Name="Black race", SkinType = "Black", PrimaryColor = "3F3411".ToColorVector3(), ModelBasicTextureName = "BirdBlackTexture", Edge = 0.9f
                                    }
*/
                                },
                    Castes = new List<CasteType>() 
                { 
                    new CasteType() {
                        KeyName = "male",
                        Reproduction = Reproduction.Male, Edge = 0.51f,
                        HeightMean = 0.5f, HeightStandardDeviation = 0.02f, 
                        WeightMean = 12f, WeightStandardDeviation = 0.15f,

                        AgeGroupTypes = new List<AgeGroupType>() 
                        { 
                            new AgeGroupType() 
                            { 
                                Name = "Baby", AIAgeGroup = AIAgeGroup.Baby, CanReproduce = false, Edge = 1.5f, HeightTargetModifier = 0.05f, WeightTargetModifier = 0.03f/*, 
                                NeedTypes = new NeedType[]{ babySleepNeed }*/
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Child", AIAgeGroup = AIAgeGroup.Child, CanReproduce = false, Edge = 11f, HeightTargetModifier = 0.6f, WeightTargetModifier = 0.5f  
                              //  , NeedTypes = new NeedType[]{ childSleepNeed }
                            },
                            new AgeGroupType() 
                            { 
                                Name = "Young adult", AIAgeGroup = AIAgeGroup.YoungAdult, CanReproduce = false, Edge = 16f, HeightTargetModifier = 0.97f, WeightTargetModifier = 0.92f  
                             //  , NeedTypes = new NeedType[]{ youngAdultSleepNeed }
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Adult", AIAgeGroup = AIAgeGroup.Adult, CanReproduce = true, Edge = 72f, HeightTargetModifier = 1f, WeightTargetModifier = 1f  
                               //, NeedTypes = new NeedType[]{ adultSleepNeed }
                            }
                            ,
                            new AgeGroupType() 
                            { 
                                Name = "Old", AIAgeGroup = AIAgeGroup.Old, CanReproduce = true, Edge = 200f, HeightTargetModifier = 0.9f, WeightTargetModifier = 1f  
                              // , NeedTypes = new NeedType[]{ oldSleepNeed }
                            }
                        } 
                    },
                    new CasteType() { 
                        KeyName = "female",
                        Reproduction = Reproduction.Female, Edge = 1f, //????
                        HeightMean = 0.5f, HeightStandardDeviation = 0.02f, 
                        WeightMean = 12f, WeightStandardDeviation = 0.15f,

                        AgeGroupTypes = new List<AgeGroupType>() 
                        { 
                            new AgeGroupType() 
                            { 
                                Name = "Baby", AIAgeGroup = AIAgeGroup.Baby, CanReproduce = false, Edge = 1.5f, HeightTargetModifier = 0.05f, WeightTargetModifier = 0.03f/*, 
                                NeedTypes = new NeedType[]{ babySleepNeed }*/
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Child", AIAgeGroup = AIAgeGroup.Child, CanReproduce = false, Edge = 11f, HeightTargetModifier = 0.6f, WeightTargetModifier = 0.5f  
                              //, NeedTypes = new NeedType[]{ childSleepNeed }
                            },
                            new AgeGroupType() 
                            { 
                                Name = "Young adult", AIAgeGroup = AIAgeGroup.YoungAdult, CanReproduce = false, Edge = 16f, HeightTargetModifier = 0.97f, WeightTargetModifier = 0.92f  
                             //  , NeedTypes = new NeedType[]{ youngAdultSleepNeed }
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Adult", AIAgeGroup = AIAgeGroup.Adult, CanReproduce = true, Edge = 72f, HeightTargetModifier = 1f, WeightTargetModifier = 1f  
                             // , NeedTypes = new NeedType[]{ adultSleepNeed }
                            }
                            ,
                            new AgeGroupType() 
                            { 
                                Name = "Old", AIAgeGroup = AIAgeGroup.Old, CanReproduce = false, Edge = 200f, HeightTargetModifier = 0.9f, WeightTargetModifier = 1f  
                            //  , NeedTypes = new NeedType[]{ oldSleepNeed }
                            }
                        } 
                    } 
                }

                };

                listOfEntityTypes.Add(skinnedtestType);


                #endregion
   
                #region patrician needs
                NeedType patricianNeed = new NeedType()
                {
                    KeyName = "foodEnergy",
                    FoodNeedType = new FoodNeedType()
                    {
                        FoodNutrient = "foodEnergy",
                        RequiredNutrientsAsFractionOfEntityBulk = 0.03f
                    },
                    DecreasePerDay = new NormalDistribution() { Mean = 2f, StandardDeviation = 0.04f },                 

                    LimitForDecreasedEnergy = 0.05f,
                    DecreasedEnergyWeight = 0.08f, // 0.6f,
                    PhysicalEffects = new PhysicalEffects()
                    {
                        DaysAtZeroCausingCollapse = 3f,
                        DaysAtZeroCausingDeath = 3.5f,
                        DaysAtZeroDecreaseFactor = 1f, //0.5f,
                        LimitForReducedGrowth = 0.1f,
                        LimitForIncreasedSickness = 0.05f,
                        UseExertionFactorToDecrease = false                        
                    }
                };
                #endregion

                #region patriciantype
                EntityType patricianType = new EntityType("entity:patrician")
                {
                    
                    Name = "Patrician",
                    ThumbnailSmall = "HUD_thumbnail_patrician",
                    SummaryDescription = "Predator/scavenger",  //solitary http://en.wikipedia.org/wiki/Territorial_animal  http://en.wikipedia.org/wiki/Spider#Feeding.2C_digestion_and_excretion
                    Description = "\n FEEDING CLASSIFICATION: Carnivore. Eats fish, slugs, smaller animals\n \n HEIGHT: Up to 3 m\n \n ANATOMY\n Vertical, octahedron-shaped body protected by exoskeleton. Its four legs are arranged symmetrically as are the sensory organs on the top.\n Its regal posture, 'crown' and territorial behaviour made researchers name the animal for the ancient Roman land holders.\n \n BEHAVIOR\n Highly territorial animal which jealously protects its hunting grounds. Kills by stabbing with a venomous spear that extends from its legs. With the spear, digestive enzymes are then pumped into the prey and the liquified tissues are sucked out.\n \n SURVIVAL GUIDE NOTES\n The animal will attack us if we enter its territory, but it moves rather slowly.",
                    
                    RenderableType = new RenderableType()
                    {
                        RenderAsModelType = new RenderAsModelType()
                        {
                            //ModelScale = 3f,// no need, priority goes, race ModelScale / cast ModelScale / default (renderAdModelType ModelScale)
                            AssetName = "patrician",
                            GaitAnimations = new XmlDictionary<string, GaitAnimationBracket[]>
                        {
                        
                        { "normal", new[]{                                           
                                              new GaitAnimationBracket(){ MinimumSpeed = 0f, MaximumSpeed = 65f, StrideLength = 6f /*measured: 24f*/, StrideDuration = 0.44f, AnimationKey = "gaitWalk"},                                            
                                              } }
                    },
                       DefaultInfo = new AnimConditionInfo()
                       {
                           // use this neutral anim when all flags are cleared:
                           SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "idle" } }
                       },
                       DefaultStances = new[] // "filler" anims used instead of idle when a stance has been set, but no action yet (happens between goals). This prevents unwanted switching to standing from kneeling, for instance
                        {
                            new  AnimConditionInfo()
                            {
                                 SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "idle" } }   
                            },
                            new  AnimConditionInfo()
                            {
                                 ConditionSet = new AnimConditions(){ Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Bold )},                              
                                 SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "combatIdle" } }   
                            }
                        },
                       AnimConditions = new AnimConditionInfo[] 
                        {
                            new AnimConditionInfo()
                            {                              
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Moving},                            
                                GaitSetKey = "normal"
                            },
                                           
                            new AnimConditionInfo() 
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "idle" }},                            
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle}                              
                            },
                             new AnimConditionInfo() 
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "eat" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Eating }                             
                            },
                             new AnimConditionInfo() 
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "combatIdle" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Bold)}                              
                            },
                              new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "attackLowRight" }}, 
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Attacking,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Low, (int)AnimModifier.Right)},
                               
  
                            },
                              new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "attackHighDouble" }}, 
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Attacking,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.High, (int)AnimModifier.Extreme)},
                               
                            },
                           new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ 
                                    BaseAnimations = new string[]{  "hit" }, 
                                    Sounds = new string[] { "aliens/alienCombat/flutter_Mat43_v2" }  },//patrician hit sound. I vary the pitch in basedataloader
                                Playback = Playback.Manual, 
                               // Sound = "aliens/alienCombat/flutter_Mat43_v2", 
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Recoiling }
                            },
                         


                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{ "dying" }},                      
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying }
                            },
 
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ 
                                    BaseAnimations = new string[]{  "collapse" }, 
                                    Sounds = new[]{ "aliens/alienCombat/strumming_Mat25_v1" }},                              
                                Looping = Looping.No,                                
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Pre)}  
                            },
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{ "dead"  }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Post)}  
                            }
                        }
                   }
               },
                    LocomotorType = new LocomotorType()
                    {
                        MaxAngularSpeed = .25f * MathHelper.Pi, // MathHelper.Pi,
                        FourSidedSymmetry = true,
                        MeleeRadius = 22f,

                        LeggedLocomotorType = new LeggedLocomotorType()
                        {
                            TerrainNegateFactor = 0.5f,
                            WalkSlowSpeed = 10f, // 8f, 
                            WalkNormalSpeed = 18f /*debug only*/,/*proper speed*///, ModelScale = 3f : (NORMAL patrician walk speed=1.0, basespeed=18f ) (SLOW patrician walk speed=0.7, basespeed=11f) (FAST patrician walk speed=2, basespeed=35f) (SUPERSLOW Don't use ... patrician walk speed=0.3, basespeed=11f)
                            WalkFastSpeed = 24f, // 18f,//30f, // 63f,
                            //, ModelScale = 4f : (NORMAL patrician walk speed=1.0, basespeed=20f ) (SLOW patrician walk speed=0.7, basespeed=15f) (FAST patrician walk speed=2, basespeed=50f)

                            
                        },
                        CollisionResponderType = new CollisionResponderType()
                        {
                            AgentCollisionResponderType = new AgentCollisionResponderType()
                        }
                    },
                    SensorType = new SensorType()
                    {
                        Range = sensorRangeHuman + 100,
                        RangeAtNight = sensorRangeHumanNight + 100,
                        DetectionTypeKey = "defaultDetection"
                    },

                    BodyType = GameData.Instance.AllBodyTypes["patrician"],
                   
                    ContainerType = new AgentStorageType() //0.5f)
                    {
                        ItemStorageType = new ItemStorageType(0.8f), // can Haul... later.     MP: if you remove this line you get an error june 24 2014
                        StomachStorageType = new ItemStorageType(0.5f) // stomach size is dynamic - is defined in BioType
                    }
                };
                    
                patricianType.IntelligenceType = new IntelligenceType()
                {
                    IsMobile = true,
                    CanAttack = true,
                    CanUseWeapons = false,
                    CanHunt = true,
                    CanScout = true,
                    CanExamine = true, 
                    CanPatrol = true,
                    CanHaul = false,
                    IsPredator = true,
                    WillAttackNonThreatsNearby = true,
                    StrengthRating = StrengthRating.LikeHumans,
                    InterestInTriggerTypes = new[] { "mineTrigger", "spikeTrapTrigger" },
                    Courage = 0.5f,
                    MemoryInDays = 3f,
                    Boldness = 0.8f, 
                    AggroRange = 300f,
                    ContainerTransactTag = "patricianTransact", //was "patrician"
                    ChanceToRestAfterMeleeAttack = 0.3, 
                    MinRestTimeAfterAttackingInSeconds = 0.5f,
                    MaxRestTimeAfterAttackingInSeconds = 1.2f,

                    Prey = new string[] { "entity:mudWorm", "entity:whiteThunderChicken", "entity:binalRat", "entity:bajingan", "entity:pygmyThunderChicken" },
                    
                    Attacks = new[] 
                     { 
                         "patricianHighDouble",
                         "patricianLowRight"          
                     },
                     Skills = new SerializableDictionary<string, float> 
                    { 
                        {
                           "unarmedFighting" , 0.4f
                        }
                    },
                };



                patricianType.BiologicalType = new BiologicalType()
                {
                    OrderKey = "patricianOrder",
                    OxygenAndMuscleEnergyIncreaseRatePerDay = 10f,
                    TimeToConsumeFullMealInDays = 0.005f,
                    StomachSizeFractionOfEntityBulk = 0.15f,
                    StomachContentsDecreaseRatePerDay = 2,
                    FoodItemTagsThatCanBeConsumed = new[] { "cookedMeat", "rawMeat", "smallRawMeat", "spoiledMeal", "inedibleMeat", "rottenMeat" },
                    ExtractionProcessTypes = new[] { "extractMudWormMeat", "extractLeafCutterMeat", "extractThunderChickenMeat", "extractBinalRatMeat", "extractTwinklerMeat", "extractBushDragonMeat", "extractSwampDemonTreeMeat", "extractDemonTreeMeat", "extractMegapodMeat", "extractWhipjawMeat", "extractSpikePlantMeat", "extractForestGuardianMeat", }, //"extractThinThunderChickenMeat",
                   
                    IsTerritorial = true,
                    MaxRegainLimit = humanMaxRegainLimit,
                    FractionOfMaxHitpointsGainedPerDay = humanFractionOfMaxHitpointsGainedPerDay,
                    ActiveStealthRating = 0.1f,
                    Carcass = "item:patricianCarcass",
                    //new ResourceType("patriciancarcass") { ResourceItem = CreatePlaceholder("item:patriciancarcass") },
                    
                    RaceTypes = new[]
                                {     
                                    new RaceType()                              // Beige
                                    { Name="Steppe patrician", PortraitSkinType="Pale", PrimaryColor = "F7F4C5".ToColorVector3(), ModelBasicTextureName = "PatricianPaleTexture", Edge = 0.3f, ModelScale = 3f
                                    },
                                    new RaceType()                              // Dark
                                    { Name="Undocumented dark patrician", PortraitSkinType="Dark", PrimaryColor = "564920".ToColorVector3(), ModelBasicTextureName = "PatricianBrownTexture", Edge = 0.6f, ModelScale = 3f
                                    },
                                    new RaceType()                              // Dark
                                    { Name="Zebra patrician", PortraitSkinType="Yellow", PrimaryColor = "564920".ToColorVector3(), ModelBasicTextureName = "PatricianZebraTexture", Edge = 0.7f, ModelScale = 3f
                                    },
                                    new RaceType()                              // Dark
                                    { Name="Wasp patrician", PortraitSkinType="Red", PrimaryColor = "564920".ToColorVector3(), ModelBasicTextureName = "PatricianWaspTexture", Edge = 0.8f, ModelScale = 3f
                                    },
                                    new RaceType()                              // Dark
                                    { Name="White patrician", PortraitSkinType="White", PrimaryColor = "564920".ToColorVector3(), ModelBasicTextureName = "PatricianWhiteTexture", Edge = 0.85f, ModelScale = 3f
                                    },
                                    new RaceType()                              // Dark
                                    { Name="Undocumented purple patrician", PortraitSkinType="Purple", PrimaryColor = "564920".ToColorVector3(), ModelBasicTextureName = "PatricianPurpleTexture", Edge = 0.87f, ModelScale = 3f
                                    },
                                    new RaceType()                              // Black
                                    { Name="Black patrician", PortraitSkinType = "Black", PrimaryColor = "3F3411".ToColorVector3(), ModelBasicTextureName = "PatricianBlackTexture", Edge = 0.9f, ModelScale = 3f     
                                    }
                                },
                    Castes = new List<CasteType>() 
                { 
                    new CasteType() { KeyName = "male",
                        Reproduction = Reproduction.Male, Edge = 0.51f,
                        HeightMean = 2f, HeightStandardDeviation = 0.08f, 
                        WeightMean = 110f, WeightStandardDeviation = 0.15f,
                         
                        AgeGroupTypes = new List<AgeGroupType>() 
                        { 
                            new AgeGroupType() 
                            { 
                                Name = "Baby", AIAgeGroup = AIAgeGroup.Baby, CanReproduce = false, Edge = 1f, HeightTargetModifier = 0.05f, WeightTargetModifier = 0.03f, ModelScaleFraction = 0.3f,
                                NeedTypes = new []{ patricianNeed  }
                                /*, 
                                NeedTypes = new NeedType[]{ babySleepNeed }*/
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Child", AIAgeGroup = AIAgeGroup.Child, CanReproduce = false, Edge = 2f, HeightTargetModifier = 0.6f, WeightTargetModifier = 0.5f, ModelScaleFraction = 0.6f,
                                NeedTypes = new []{ patricianNeed  }
                               // , NeedTypes = new NeedType[]{ childSleepNeed }
                            },
                            new AgeGroupType() 
                            { 
                                Name = "Young adult", AIAgeGroup = AIAgeGroup.YoungAdult, CanReproduce = false, Edge = 8f, HeightTargetModifier = 0.97f, WeightTargetModifier = 0.92f, ModelScaleFraction = 0.85f,
                                NeedTypes = new []{ patricianNeed  }
                               //, NeedTypes = new NeedType[]{ youngAdultSleepNeed }
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Adult", AIAgeGroup = AIAgeGroup.Adult, CanReproduce = true, Edge = 20f, HeightTargetModifier = 1f, WeightTargetModifier = 1f, ModelScaleFraction = 1f,
                                NeedTypes = new []{ patricianNeed  }
                              // , NeedTypes = new NeedType[]{ adultSleepNeed }
                            }
                            ,
                            new AgeGroupType() 
                            { 
                                Name = "Old", AIAgeGroup = AIAgeGroup.Old, CanReproduce = true, Edge = 22f, HeightTargetModifier = 0.9f, WeightTargetModifier = 1f, ModelScaleFraction = 1f,
                                NeedTypes = new []{ patricianNeed  }
                               //, NeedTypes = new NeedType[]{ oldSleepNeed }
                            }
                        } 
                    },
                    new CasteType() { KeyName = "female",
                        Reproduction = Reproduction.Female, Edge = 1f, //????
                        HeightMean = 1.90f, HeightStandardDeviation = 0.05f, 
                        WeightMean = 100f, WeightStandardDeviation = 0.10f,

                        AgeGroupTypes = new List<AgeGroupType>() 
                        { 
                            new AgeGroupType() 
                            { 
                                Name = "Baby", AIAgeGroup = AIAgeGroup.Baby, CanReproduce = false, Edge = 1f, HeightTargetModifier = 0.05f, WeightTargetModifier = 0.03f, ModelScaleFraction = 0.3f,
                                NeedTypes = new []{ patricianNeed  }
                                /*,                                NeedTypes = new NeedType[]{ babySleepNeed }*/
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Child", AIAgeGroup = AIAgeGroup.Child, CanReproduce = false, Edge = 2f, HeightTargetModifier = 0.6f, WeightTargetModifier = 0.5f, ModelScaleFraction = 0.6f,
                                NeedTypes = new []{ patricianNeed  }
                             // , NeedTypes = new NeedType[]{ childSleepNeed }
                            },
                            new AgeGroupType() 
                            { 
                                Name = "Young adult", AIAgeGroup = AIAgeGroup.YoungAdult, CanReproduce = false, Edge = 8f, HeightTargetModifier = 0.97f, WeightTargetModifier = 0.92f, ModelScaleFraction = 0.85f,
                                NeedTypes = new []{ patricianNeed  }
                              // , NeedTypes = new NeedType[]{ youngAdultSleepNeed }
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Adult", AIAgeGroup = AIAgeGroup.Adult, CanReproduce = true, Edge = 20f, HeightTargetModifier = 1f, WeightTargetModifier = 1f, ModelScaleFraction = 1f,
                                NeedTypes = new []{ patricianNeed  }
                                // , NeedTypes = new NeedType[]{ adultSleepNeed }
                            }
                            ,
                            new AgeGroupType() 
                            { 
                                Name = "Old", AIAgeGroup = AIAgeGroup.Old, CanReproduce = false, Edge = 22f, HeightTargetModifier = 0.9f, WeightTargetModifier = 1f, ModelScaleFraction = 1f,  
                                NeedTypes = new []{ patricianNeed  }
                                //, NeedTypes = new NeedType[]{ oldSleepNeed }
                            }
                        } 
                    } 
                }

                };

                listOfEntityTypes.Add(patricianType);

                #endregion

                #region snatcher needs
                NeedType snatcherNeed = new NeedType()
                {
                    KeyName = "foodEnergy",
                    FoodNeedType = new FoodNeedType()
                    {
                        FoodNutrient = "foodEnergy",
                        RequiredNutrientsAsFractionOfEntityBulk = 0.03f
                    },
                    DecreasePerDay = new NormalDistribution() { Mean = 2f, StandardDeviation = 0.04f },                  
                    LimitForDecreasedEnergy = 0.05f,
                    DecreasedEnergyWeight = 0.08f, // 0.6f,
                    PhysicalEffects = new PhysicalEffects()
                    {
                        DaysAtZeroCausingCollapse = 3f,
                        DaysAtZeroCausingDeath = 3.5f,
                        DaysAtZeroDecreaseFactor = 1f, //0.5f,
                        LimitForReducedGrowth = 0.1f,
                        LimitForIncreasedSickness = 0.05f,
                        UseExertionFactorToDecrease = false                        
                    }
                };
                #endregion

                #region Snatchertype
                #region whipjaw
                EntityType snatcherType = new EntityType("entity:whipjaw")
                {

                    Name = "Great whipjaw",
                    ThumbnailSmall = "HUD_thumbnail_snatcher",
                    SummaryDescription = "Dangerous predator/scavenger",  // http://en.wikipedia.org/wiki/Armour_(anatomy)
                    Description = "\n FEEDING CLASSIFICATION: Carnivore: Eats carrion and small animals.\n \n LENGTH: Up to 2.5 m\n \n ANATOMY\n We named the whipjaw for the strong arms mounted on its head: During feeding or combat, the animal uncoils these limbs with a powerful motion. The two outermost arms are lined with razor like teeth (for cutting) while the inner arm is studded with small hooks for grabbing and tearing. In combination, they function more or less like a knife and fork cutting off a piece of steak. The middle arm will bring pieces of flesh to the mouth which is situated at the top the head.\n The female is further equipped with two sharp horns and a strong keratin armor making the whipjaw cow almost unassailable.\n \n BEHAVIOR\n The animal makes up for its slow speed with highly aggressive behavior and will most often win when competing with other scavengers for a carcass.\n \n SURVIVAL GUIDE NOTES\n The animal is not afraid of humans and we should keep a distance.",
                   
                    RenderableType = new RenderableType()
                    {
                        RenderAsModelType = new RenderAsModelType()
                        {

                            ModelScale = 2.1f,// priority goes: race ModelScale / cast ModelScale / default (renderAdModelType ModelScale)
                            AssetName = "snatcher",
                            GaitAnimations = new XmlDictionary<string, GaitAnimationBracket[]>
                       {
                        
                        { "normal", new[]{                                           
                                              new GaitAnimationBracket(){ MinimumSpeed = 0f, MaximumSpeed = 65f, /* */ StrideLength = 6f /*measured: 24f*/, StrideDuration = 0.44f, AnimationKey = "walk"},                                            
                                              } }
                    },
                       //new
                            DefaultInfo = new AnimConditionInfo()
                            {
                                // use this neutral anim when all flags are cleared:
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "idle" } }
                            },
                            DefaultStances = new[] // "filler" anims used instead of idle when a stance has been set, but no action yet (happens between goals). This prevents unwanted switching to standing from kneeling, for instance
                        {
                            new  AnimConditionInfo()
                            {
                                 SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "idle" } }   
                            },
                            new  AnimConditionInfo()
                            {
                                 ConditionSet = new AnimConditions(){ Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Bold )},                              
                                 SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "combatIdle" } }   
                            }
                        },
                    //new end

                            AnimConditions = new AnimConditionInfo[] 
                        {
                            new AnimConditionInfo()
                            {                              
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Moving},                            
                                GaitSetKey = "normal"
                            },
                                           
                            new AnimConditionInfo() 
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "idle" }},                            
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle}                              
                            },

                            new AnimConditionInfo() 
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "combatIdle" }},                            
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle, Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Bold )}                              
                            },

                             new AnimConditionInfo() 
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "eat" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Eating }                             
                            },

                              new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "attack1" }}, 
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Attacking,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Low, (int)AnimModifier.Right)},
                            },
                              new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "attack2" }}, 
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Attacking,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.High, (int)AnimModifier.Extreme)},
                            },

                           new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "hit" },
                                Sounds = new string [] { "aliens/alienCombat/snatcherHit" }}, //snatcher hit sound
                                Playback = Playback.Manual, 
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Recoiling }
                            },

                           new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "idle" }}, //todo
                                Playback = Playback.Manual, 
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Recoiling }
                            },
                         
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "collapse" },
                                Sounds = new string [] { "aliens/alienCombat/snatcherDeath"}},
                                Looping = Looping.No,
                                
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Pre)}  
                            },
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{ "dead"  }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Post)}  
                            }
                        }
                        }
                    },
                    LocomotorType = new LocomotorType()
                    {
                        MaxAngularSpeed = .25f * MathHelper.Pi, // MathHelper.Pi,
                        FourSidedSymmetry = true,
                        MeleeRadius = 22f,

                        LeggedLocomotorType = new LeggedLocomotorType()
                        {
                            TerrainNegateFactor = 0.5f,
                            WalkSlowSpeed = 14f, // 8f, 
                            WalkNormalSpeed = 22f /*debug only*/,/*proper speed*///, ModelScale = 3f : (NORMAL patrician walk speed=1.0, basespeed=18f ) (SLOW patrician walk speed=0.7, basespeed=11f) (FAST patrician walk speed=2, basespeed=35f) (SUPERSLOW Don't use ... patrician walk speed=0.3, basespeed=11f)
                            //, ModelScale = 4f :

                            WalkFastSpeed = 28f, // 18f,//30f, // 63f,
                        },
                        CollisionResponderType = new CollisionResponderType()
                        {
                            AgentCollisionResponderType = new AgentCollisionResponderType()
                        }
                    },
                    SensorType = new SensorType()
                    {
                        Range = sensorRangeHuman, // NA was + 100
                        RangeAtNight = sensorRangeHumanNight, // NA was + 100
                        DetectionTypeKey = "defaultDetection"
                    },

                    BodyType = GameData.Instance.AllBodyTypes["snatcher"],

                    ContainerType = new AgentStorageType() //0.5f)
                    {
                        ItemStorageType = new ItemStorageType(0.8f), // 
                        StomachStorageType = new ItemStorageType(0.5f)                         
                    }
                };

                snatcherType.IntelligenceType = new IntelligenceType()
                {
                    IsMobile = true,
                    CanAttack = true,
                    CanUseWeapons = false,
                    CanHunt = true,
                    CanScout = true,
                    CanExamine = true, 
                    CanPatrol = true,
                    CanHaul = false,
                    IsPredator = true,
                    WillAttackNonThreatsNearby = true,
                    StrengthRating = StrengthRating.LikeHumans,
                    InterestInTriggerTypes = new[] { "mineTrigger", "spikeTrapTrigger" },
                    Courage = 0.25f,
                    MemoryInDays = 3f,
                    Boldness = 0.4f,
                    AggroRange = 200f,
                    ContainerTransactTag = "snatcherTransact", //was "snatcher"
                    ChanceToRestAfterMeleeAttack = 0.9,
                    MinRestTimeAfterAttackingInSeconds = 0.4f,
                    MaxRestTimeAfterAttackingInSeconds = 1.2f,
                    Prey = new string[] { "entity:mudWorm", "entity:binalRat", "entity:whiteThunderChicken", "entity:pygmyThunderChicken", "entity:bajingan", "entity:human" },
                
                    Attacks = new[] //necessary to define attacks here again
                     { 
                         "snatcherGrabAttack", //attack1
                         "snatcherFastAttack"  //attack2        
                     },
                    Skills = new SerializableDictionary<string, float> 
                    { 
                        {
                           "unarmedFighting" , 0.3f
                        }
                    },
                };



                snatcherType.BiologicalType = new BiologicalType()
                {
                    OrderKey = "carnufexOrder",
                    OxygenAndMuscleEnergyIncreaseRatePerDay = 10f,
                    TimeToConsumeFullMealInDays = 0.005f,
                    StomachSizeFractionOfEntityBulk = 0.10f,
                    StomachContentsDecreaseRatePerDay = 2,
                    FoodItemTagsThatCanBeConsumed = new[] { "cookedMeat", "rawMeat", "smallRawMeat", "spoiledMeal", "inedibleMeat", "rottenMeat" },
                    ExtractionProcessTypes = new[] { "extractMudWormMeat","extractPatricianMeat", "extractLeafCutterMeat", "extractThunderChickenMeat", "extractBinalRatMeat", "extractTwinklerMeat", "extractBushDragonMeat", "extractSwampDemonTreeMeat", "extractDemonTreeMeat", "extractMegapodMeat", "extractSpikePlantMeat", "extractForestGuardianMeat", }, //"extractThinThunderChickenMeat",
                    IsTerritorial = true,
                    MaxRegainLimit = humanMaxRegainLimit,
                    FractionOfMaxHitpointsGainedPerDay = humanFractionOfMaxHitpointsGainedPerDay,
                    ActiveStealthRating = 0.1f,
                    Carcass = "item:whipjawCarcass",
                    Castes = new List<CasteType>() 
                { 
                    new CasteType() { KeyName = "male",
                        Reproduction = Reproduction.Male, Edge = 0.51f,
                        HeightMean = 2f, HeightStandardDeviation = 0.08f, 
                        WeightMean = 110f, WeightStandardDeviation = 0.15f,
                        ModelBasicTextureName = "SnatcherArmoredTexture4", ModelName = "snatcherArmored",
                        AgeGroupTypes = new List<AgeGroupType>() 
                        { 
                            new AgeGroupType() 
                            { 
                                Name = "Baby", AIAgeGroup = AIAgeGroup.Baby, CanReproduce = false, Edge = 1f, HeightTargetModifier = 0.05f, WeightTargetModifier = 0.03f, ModelScaleFraction = 0.3f,
                                NeedTypes = new [] { snatcherNeed }
                                /*, 
                                NeedTypes = new NeedType[]{ babySleepNeed }*/
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Child", AIAgeGroup = AIAgeGroup.Child, CanReproduce = false, Edge = 3f, HeightTargetModifier = 0.6f, WeightTargetModifier = 0.5f, ModelScaleFraction = 0.6f,
                                NeedTypes = new [] { snatcherNeed }
                               // , NeedTypes = new NeedType[]{ childSleepNeed }
                            },
                            new AgeGroupType() 
                            { 
                                Name = "Young adult", AIAgeGroup = AIAgeGroup.YoungAdult, CanReproduce = false, Edge = 6f, HeightTargetModifier = 0.97f, WeightTargetModifier = 0.92f, ModelScaleFraction = 0.85f,
                                NeedTypes = new [] { snatcherNeed }
                               //, NeedTypes = new NeedType[]{ youngAdultSleepNeed }
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Adult", AIAgeGroup = AIAgeGroup.Adult, CanReproduce = true, Edge = 36f, HeightTargetModifier = 1f, WeightTargetModifier = 1f, ModelScaleFraction = 1f,
                                NeedTypes = new [] { snatcherNeed }
                              // , NeedTypes = new NeedType[]{ adultSleepNeed }
                            }
                            ,
                            new AgeGroupType() 
                            { 
                                Name = "Old", AIAgeGroup = AIAgeGroup.Old, CanReproduce = true, Edge = 40f, HeightTargetModifier = 0.9f, WeightTargetModifier = 1f,
                                ModelScaleFraction = 1f,   //mp april 2015 me testing out new meshes.
                                NeedTypes = new [] { snatcherNeed }
                               //, NeedTypes = new NeedType[]{ oldSleepNeed }
                            }
                        } 
                    },
                    new CasteType() { KeyName = "female",
                        Reproduction = Reproduction.Female, Edge = 1f, //????
                        HeightMean = 1.90f, HeightStandardDeviation = 0.05f, 
                        WeightMean = 100f, WeightStandardDeviation = 0.10f,
                        ModelBasicTextureName = "SnatcherFemaleTexture",
                        AgeGroupTypes = new List<AgeGroupType>() 
                        { 
                            new AgeGroupType() 
                            { 
                                Name = "Baby", AIAgeGroup = AIAgeGroup.Baby, CanReproduce = false, Edge = 1f, HeightTargetModifier = 0.05f, WeightTargetModifier = 0.03f, ModelScaleFraction = 0.3f,
                                NeedTypes = new [] { snatcherNeed }                                
                                /*, NeedTypes = new NeedType[]{ babySleepNeed }*/
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Child", AIAgeGroup = AIAgeGroup.Child, CanReproduce = false, Edge = 4f, HeightTargetModifier = 0.6f, WeightTargetModifier = 0.5f, ModelScaleFraction = 0.6f,
                                NeedTypes = new [] { snatcherNeed }
                             // , NeedTypes = new NeedType[]{ childSleepNeed }
                            },
                            new AgeGroupType() 
                            { 
                                Name = "Young adult", AIAgeGroup = AIAgeGroup.YoungAdult, CanReproduce = false, Edge = 6f, HeightTargetModifier = 0.97f, WeightTargetModifier = 0.92f, ModelScaleFraction = 0.85f,
                                NeedTypes = new [] { snatcherNeed }
                              // , NeedTypes = new NeedType[]{ youngAdultSleepNeed }
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Adult", AIAgeGroup = AIAgeGroup.Adult, CanReproduce = true, Edge = 42f, HeightTargetModifier = 1f, WeightTargetModifier = 1f, ModelScaleFraction = 1f,
                                NeedTypes = new [] { snatcherNeed }
                             // , NeedTypes = new NeedType[]{ adultSleepNeed }
                            }
                            ,
                            new AgeGroupType() 
                            { 
                                Name = "Old", AIAgeGroup = AIAgeGroup.Old, CanReproduce = false, Edge = 48f, HeightTargetModifier = 0.9f, WeightTargetModifier = 1f, ModelScaleFraction = 1f,
                                NeedTypes = new [] { snatcherNeed }
                             // , NeedTypes = new NeedType[]{ oldSleepNeed }
                            }
                        } 
                    } 
                }

                };

                listOfEntityTypes.Add(snatcherType);
                #endregion
                #region lesser whipjaw
                snatcherType = new EntityType("entity:lesserWhipjaw")
                {

                    Name = "Lesser whipjaw",
                    ThumbnailSmall = "HUD_thumbnail_snatcher",
                    SummaryDescription = "Small scavenger. Not dangerous but can be overwhelming in large numbers.",  //
                    Description = "\n FEEDING CLASSIFICATION: Carnivore: Eats carrion and small animals.\n \n LENGTH: About 30 cm\n \n ANATOMY\n We named the whipjaw for the strong arms mounted on its head: During feeding or combat, the animal uncoils these limbs with a powerful motion. The two outermost arms are lined with razor like teeth (for cutting) while the inner arm is studded with small hooks for grabbing and tearing. In combination, they function more or less like a knife and fork cutting off a piece of steak. The middle arm will bring pieces of flesh to the mouth which is situated at the top the head.\n \n BEHAVIOR Will periodically migrate in large numbers along rivers and streams in search for food. \n \n SURVIVAL GUIDE NOTES\n The animal is not afraid of humans but only poses a threat to our food stockpiles.", //

                    RenderableType = new RenderableType()
                    {
                        RenderAsModelType = new RenderAsModelType()
                        {

                            ModelScale = 0.8f,// priority goes: race ModelScale / cast ModelScale / default (renderAdModelType ModelScale)
                            AssetName = "snatcher",
                            ModelBasicTextureName = "SnatcherVariantTexture5",
                            GaitAnimations = new XmlDictionary<string, GaitAnimationBracket[]>
                       {
                        
                        { "normal", new[]{                                           
                                              new GaitAnimationBracket(){ MinimumSpeed = 0f, MaximumSpeed = 77f,  StrideLength = 5f , StrideDuration = 0.44f, AnimationKey = "walk"}, ////////////was: MinimumSpeed = 0f, MaximumSpeed = 65f,  StrideLength = 6f , StrideDuration = 0.44f,                                          
                                              } }
                    },
                            //new
                            DefaultInfo = new AnimConditionInfo()
                            {
                                // use this neutral anim when all flags are cleared:
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "idle" } }
                            },
                            DefaultStances = new[] // "filler" anims used instead of idle when a stance has been set, but no action yet (happens between goals). This prevents unwanted switching to standing from kneeling, for instance
                        {
                            new  AnimConditionInfo()
                            {
                                 SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "idle" } }   
                            },
                            new  AnimConditionInfo()
                            {
                                 ConditionSet = new AnimConditions(){ Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Bold )},                              
                                 SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "combatIdle" } }   
                            }
                        },
                            //new end

                            AnimConditions = new AnimConditionInfo[] 
                        {
                            new AnimConditionInfo()
                            {                              
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Moving},                            
                                GaitSetKey = "normal"
                            },
                                           
                            new AnimConditionInfo() 
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "idle" }},                            
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle}                              
                            },

                            new AnimConditionInfo() 
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "combatIdle" }},                            
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle, Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Bold )}                              
                            },

                             new AnimConditionInfo() 
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "eat" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Eating }                             
                            },

                              new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "attack1" }}, 
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Attacking,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Low, (int)AnimModifier.Right)},
                            },
                              new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "attack2" }}, 
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Attacking,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.High, (int)AnimModifier.Extreme)},
                            },

                           new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "hit" },
                                Sounds = new string [] { "aliens/alienCombat/snatcherHit" }}, //snatcher hit sound
                                Playback = Playback.Manual, 
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Recoiling }
                            },

                           new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "idle" }}, //todo
                                Playback = Playback.Manual, 
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Recoiling }
                            },
                         
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "collapse" },
                                Sounds = new string [] { "aliens/alienCombat/snatcherDeath"}},
                                Looping = Looping.No,
                                
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Pre)}  
                            },
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{ "dead"  }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Post)}  
                            }
                        }
                        }
                    },
                    LocomotorType = new LocomotorType()
                    {
                        MaxAngularSpeed = .25f * MathHelper.Pi, // MathHelper.Pi,
                        FourSidedSymmetry = true,
                        MeleeRadius = 22f,

                        LeggedLocomotorType = new LeggedLocomotorType()
                        { //some copied from bajingan
                            TerrainNegateFactor = 0.1f,
                            WalkSlowSpeed = 15f,  //
                            WalkNormalSpeed = 43f, //
                            WalkFastSpeed = 64f,  //
                        },
                        CollisionResponderType = new CollisionResponderType()
                        {
                            AgentCollisionResponderType = new AgentCollisionResponderType()
                        }
                    },
                    SensorType = new SensorType()
                    {
                        Range = sensorRangeHuman + 100,
                        RangeAtNight = sensorRangeHumanNight + 100,
                        DetectionTypeKey = "defaultDetection"
                    },

                    BodyType = GameData.Instance.AllBodyTypes["snatcher"],

                    ContainerType = new AgentStorageType() //0.5f)
                    {
                        ItemStorageType = new ItemStorageType(0.8f), // 
                        StomachStorageType = new ItemStorageType(0.5f)
                    }
                };

                snatcherType.IntelligenceType = new IntelligenceType() //mp the below copied from bajingan dec 2015
                {
                    IsMobile = true,
                    CanAttack = false,
                    CanUseWeapons = false,
                    CanHunt = false,
                    CanScout = true,
                    CanExamine = true, 
                    CanPatrol = false,
                    CanHaul = false,
                    IsPredator = false,
                //    WillAttackNonThreatsNearby = true,
                    StrengthRating = StrengthRating.None,
                    InterestInTriggerTypes = new[] { "mineTrigger", "smallImprovisedTrapTrigger", "spikeTrapTrigger", "animalMigrateTrigger" },
                    Courage = 0f,
                    MemoryInDays = 0.2f,
                    Boldness = 1.49f,
                    AggroRange = 0f,
                    ContainerTransactTag = "ratTransact", // small animal, kinda similar to rat.

                };



                snatcherType.BiologicalType = new BiologicalType()
                {
                    OrderKey = "carnufexOrder",
                    OxygenAndMuscleEnergyIncreaseRatePerDay = 10f,
                    FoodItemTagsThatCanBeConsumed = new[] { "cookedMeat", "rawMeat", "smallRawMeat", "spoiledMeal", "inedibleMeat", "rottenMeat" },
                    ExtractionProcessTypes = new[] { "extractMudWormMeat", "extractPatricianMeat", "extractLeafCutterMeat", "extractThunderChickenMeat", "extractBinalRatMeat", "extractTwinklerMeat", "extractBushDragonMeat", "extractSwampDemonTreeMeat", "extractDemonTreeMeat", "extractMegapodMeat", "extractSpikePlantMeat", "extractForestGuardianMeat", }, //"extractThinThunderChickenMeat",
                    TimeToConsumeFullMealInDays = 0.0125f, //
                    StomachSizeFractionOfEntityBulk = 0.2f,
                    StomachContentsDecreaseRatePerDay = 3,
                    ActiveStealthRating = 0.1f,
                    IsVermin = true,//scavenger vermin
                    MaxRegainLimit = humanMaxRegainLimit,
                    FractionOfMaxHitpointsGainedPerDay = humanFractionOfMaxHitpointsGainedPerDay,
                    Carcass = "item:whipjawCarcass",
                    Castes = new List<CasteType>() 
                {   ///////// MALE //MP the below copied from bajingan
                    new CasteType() { 
                        KeyName = "male",
                        Reproduction = Reproduction.Male, Edge = 0.51f,  
                        HeightMean = 0.5f, HeightStandardDeviation = 0.02f, 
                        WeightMean = 30f, WeightStandardDeviation = 3f,  // multiply WeightStandardDeviation by 3 and subtract that number from WeightMean, this gives the minimum Weight in kilos. divide by 100 to get the bulk...the minimum possible bulk.  

                        AgeGroupTypes = new List<AgeGroupType>() 
                        { 
                            new AgeGroupType() 
                            { 
                                Name = "Chick", AIAgeGroup = AIAgeGroup.Baby, CanReproduce = false, Edge = 0.5f, HeightTargetModifier = 0.05f, WeightTargetModifier = 0.03f, ModelScaleFraction = 0.5f,   // these modifiers are not used as of now, we think . MP
                                /*,  NeedTypes = new NeedType[]{ babySleepNeed }*/
                                NeedTypes = new []{ snatcherNeed } 
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Chick", AIAgeGroup = AIAgeGroup.Child, CanReproduce = false, Edge = 2f, HeightTargetModifier = 0.6f, WeightTargetModifier = 0.5f, ModelScaleFraction = 0.6f,
                                NeedTypes = new []{ snatcherNeed }
                                //, NeedTypes = new NeedType[]{ childSleepNeed }
                            },
                            new AgeGroupType() 
                            { 
                                Name = "Grown", AIAgeGroup = AIAgeGroup.YoungAdult, CanReproduce = false, Edge = 4f, HeightTargetModifier = 0.97f, WeightTargetModifier = 0.92f, ModelScaleFraction = 0.85f,
                                NeedTypes = new []{ snatcherNeed }
                                //  , NeedTypes = new NeedType[]{ youngAdultSleepNeed }
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Grown", AIAgeGroup = AIAgeGroup.Adult, CanReproduce = true, Edge = 20f, HeightTargetModifier = 1f, WeightTargetModifier = 1f, ModelScaleFraction = 1f,
                                NeedTypes = new []{ snatcherNeed }
                                //   , NeedTypes = new NeedType[]{ adultSleepNeed }
                            }
                            ,
                            new AgeGroupType() 
                            { 
                                Name = "Grown", AIAgeGroup = AIAgeGroup.Old, CanReproduce = true, Edge = 22f, HeightTargetModifier = 0.9f, WeightTargetModifier = 1f, ModelScaleFraction = 1f,
                                NeedTypes = new []{ snatcherNeed }
                                //  , NeedTypes = new NeedType[]{ oldSleepNeed }
                            }
                        } 
                    },
                   /////////FEMALE
                    new CasteType() { 
                        KeyName = "female",
                        Reproduction = Reproduction.Female, Edge = 1f, 
                        HeightMean = 0.45f, HeightStandardDeviation = 0.02f, 
                        WeightMean = 28f, WeightStandardDeviation = 2f,  // multiply WeightStandardDeviation by 3 and subtract that number from WeightMean, this gives the minimum Weight in kilos. divide by 100 to get the bulk...the minimum possible bulk.  

                        AgeGroupTypes = new List<AgeGroupType>() 
                        { 
                            new AgeGroupType() 
                            { 
                                Name = "Chick", AIAgeGroup = AIAgeGroup.Baby, CanReproduce = false, Edge = 0.5f, HeightTargetModifier = 0.05f, WeightTargetModifier = 0.03f, ModelScaleFraction = 0.3f,  // these modifiers are not used as of now, we think . MP
                                NeedTypes = new []{ snatcherNeed }
                                /*,  NeedTypes = new NeedType[]{ babySleepNeed }*/
                                
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Chick", AIAgeGroup = AIAgeGroup.Child, CanReproduce = false, Edge = 2f, HeightTargetModifier = 0.6f, WeightTargetModifier = 0.5f, ModelScaleFraction = 0.6f,
                                NeedTypes = new []{ snatcherNeed }
                                //  , NeedTypes = new NeedType[]{ childSleepNeed }
                            },
                            new AgeGroupType() 
                            { 
                                Name = "Grown", AIAgeGroup = AIAgeGroup.YoungAdult, CanReproduce = false, Edge = 4f, HeightTargetModifier = 0.97f, WeightTargetModifier = 0.92f, ModelScaleFraction = 0.85f,
                                NeedTypes = new []{ snatcherNeed }
                                //     , NeedTypes = new NeedType[]{ youngAdultSleepNeed }
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Grown", AIAgeGroup = AIAgeGroup.Adult, CanReproduce = true, Edge = 20f, HeightTargetModifier = 1f, WeightTargetModifier = 1f, ModelScaleFraction = 1f,
                                NeedTypes = new []{ snatcherNeed }
                                //    , NeedTypes = new NeedType[]{ adultSleepNeed }
                            }
                            ,
                            new AgeGroupType() 
                            { 
                                Name = "Grown", AIAgeGroup = AIAgeGroup.Old, CanReproduce = false, Edge = 22f, HeightTargetModifier = 0.9f, WeightTargetModifier = 1f, ModelScaleFraction = 1f,
                                NeedTypes = new []{ snatcherNeed }
                                //   , NeedTypes = new NeedType[]{ oldSleepNeed }
                            }
                        } 
                    } 
                }

                };

                listOfEntityTypes.Add(snatcherType);
                #endregion
                #endregion

                #region spikePlanttype
                EntityType spikePlantType = new EntityType("entity:spikePlant")
                {

                    Name = "Ursinix",
                    ThumbnailSmall = "HUD_thumbnail_ursinix",
                    SummaryDescription = "Ambush predator",  //
                    Description = "",                   
                    //DetectionTag = "wellHiddenAnimal", // must only be detected after using examine! but sensor will detect it. also set  ActiveStealthRating = 1f further down.
                    #region RenderableType
                    RenderableType = new RenderableType()
                    {
                        RenderAsModelType = new RenderAsModelType()
                        {

                            ModelScale = 1.8f, // priority goes: race ModelScale / cast ModelScale / default (renderAdModelType ModelScale)
                            AssetName = "spikePlant",
                            ModelBasicTextureName = "SpikePlantTexture",
                            GaitAnimations = new XmlDictionary<string, GaitAnimationBracket[]>
                       {
                        
                        { "normal", new[]{            //mp feb 2015: how to make it stationary and play the idle anim: I've iterated the stridelength number so that it plays its idle anim at the correct speed , while "scouting" on the spot.                               
                                              new GaitAnimationBracket(){ MinimumSpeed = 0f, MaximumSpeed = 0f, StrideLength = 0.005f , StrideDuration = 0.44f, AnimationKey = "idle"},                                            
                                              } }
                       },

                            AnimConditions = new AnimConditionInfo[] 
                        {
                            new AnimConditionInfo()
                            {                              
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Moving},                            
                                GaitSetKey = "normal"
                            },
                                           
                            new AnimConditionInfo() 
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "idle" }},                            
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle}                              
                            },

                             new AnimConditionInfo() 
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "combatIdle" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Bold)}                              
                            },
                              new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "attack" }}, 
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Attacking,  
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Low)}
  
                            },


                              new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet()
                                { 
                                    BaseAnimations = new string[]{  "hit" },
                                    Sounds = new string[]{ "aliens/rattleWoodenShort" }//spikeplant hit sound
                                },
                                Playback = Playback.Manual,                                
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Recoiling }
                            },

                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{ "dead" }}, //no twitching anim made yet, so just using dead anim.
                      //          Sound = "aliens/rattleWooden",  bug - never stops ..21 oct 2013
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying }
                            },

                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ 
                                    BaseAnimations = new string[]{  "collapse" },//necessary.
                                    Sounds = new string[]{"aliens/rattleWooden"}}, 
                             
                                Looping = Looping.No,
                                
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Pre)}  
                            },
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{ "dead"  }}, //necessary. put in name of dead anim when made
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Post)}  
                            }

                        }
                        }
                    },
                    #endregion
                    LocomotorType = new LocomotorType()
                    {
                        MaxAngularSpeed = .25f * MathHelper.Pi, // MathHelper.Pi,
                        FourSidedSymmetry = false,
                        MeleeRadius = 1f,

                        LeggedLocomotorType = new LeggedLocomotorType()
                        {
                            TerrainNegateFactor = 0.5f,
                            WalkSlowSpeed = 0,//110.01f, //this freezes the idle anim.. mp could not set IsMobile = false, got a crash.
                            WalkNormalSpeed = 0,//110.01f,

                            WalkFastSpeed = 0//110.01f, // 18f,//30f, // 63f,
                        },
                        CollisionResponderType = new CollisionResponderType()
                        {
                            AgentCollisionResponderType = new AgentCollisionResponderType()
                        }
                    },
                    SensorType = new SensorType()
                    {
                        Range = sensorRangeHuman + 100,
                        RangeAtNight = sensorRangeHumanNight + 100,
                        DetectionTypeKey = "defaultDetection"
                    },

                    BodyType = GameData.Instance.AllBodyTypes["spikePlant"],

                    ContainerType = new AgentStorageType()
                    {
                        CanTransactWithTags = new[] { "ratTransact", "humanTransact", "leafcutterTransact", "chickenTransact" },
                        ItemStorageType = new ItemStorageType(0.5f),
                        StomachStorageType = new ItemStorageType(0.07f)                       
                    },
                    //can always just use trigger if attack doesn't work
                    /*Triggers = new TriggerType[] 
                    { 
                        GameData.Instance.AllTriggerTypes["smallImprovisedTrapTrigger"]
                    },*/
                };

                spikePlantType.IntelligenceType = new IntelligenceType() // remember to replace demonTreeType.! when copy pasting
                {

                    IsMobile = true, // true, //  jan 2015: i got a crash when i set this to false
                    CanAttack = true,
                    CanUseWeapons = false,
                    CanHunt = true,
                    CanScout = false,
                    CanExamine = false, 
                    CanPatrol = false,
                    CanHaul = false,
                    IsPredator = true,
                    WillAttackNonThreatsNearby = false,
                    StrengthRating = StrengthRating.None, //changed from WeakerThanHumans; we don't want the creatures to be scared of it
                    InterestInTriggerTypes = new[] { "mineTrigger" },
                    Courage = 1,//0.5f,
                    MemoryInDays = 3f,
                    Boldness = 1,//0.8f,
                    AggroRange = 300f,
                    ChanceToRestAfterMeleeAttack = 0.3,
                    MinRestTimeAfterAttackingInSeconds = 0.4f,
                    MaxRestTimeAfterAttackingInSeconds = 1.2f,

                    Prey = new string[] { "entity:mudWorm", "entity:binalRat", "entity:whiteThunderChicken", "entity:pygmyThunderChicken", "entity:human" },
                   
                    Attacks = new[]
                     { 
                         "spikePlantAttack"         
                     },
                    Skills = new SerializableDictionary<string, float> 
                    { 
                        {
                           "unarmedFighting" , 1f//0.2f
                        }
                    },
                };



                spikePlantType.BiologicalType = new BiologicalType() // remember to replace demonTreeType.! when copy pasting
                {
                    OrderKey = "spikePlantOrder",
                    OxygenAndMuscleEnergyIncreaseRatePerDay = 10f,
                    TimeToConsumeFullMealInDays = 0.0125f,
                    StomachSizeFractionOfEntityBulk = 0.15f,
                    StomachContentsDecreaseRatePerDay = 2,
                  
                    IsTerritorial = true,
                    MaxRegainLimit = humanMaxRegainLimit,
                    FractionOfMaxHitpointsGainedPerDay = humanFractionOfMaxHitpointsGainedPerDay,
                    ActiveStealthRating = 1f, //0.1f
                    Carcass = "item:spikePlantCarcass", //
                    // new ResourceType("patriciancarcass") { ResourceItem = CreatePlaceholder("item:patriciancarcass") },
                    Castes = new List<CasteType>() 
                { 
                    new CasteType() { KeyName = "male",
                        Reproduction = Reproduction.Male, Edge = 0.51f,
                        HeightMean = 2f, HeightStandardDeviation = 0.08f, 
                        WeightMean = 110f, WeightStandardDeviation = 0.15f,
                         
                        AgeGroupTypes = new List<AgeGroupType>() 
                        { 
                            new AgeGroupType() 
                            { 
                                Name = "Baby", AIAgeGroup = AIAgeGroup.Baby, CanReproduce = false, Edge = 1.5f, HeightTargetModifier = 0.05f, WeightTargetModifier = 0.03f, ModelScaleFraction = 0.3f
                             //, NeedTypes = new NeedType[]{ babySleepNeed }
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Child", AIAgeGroup = AIAgeGroup.Child, CanReproduce = false, Edge = 11f, HeightTargetModifier = 0.6f, WeightTargetModifier = 0.5f, ModelScaleFraction = 0.6f  
                               // , NeedTypes = new NeedType[]{ childSleepNeed }
                            },
                            new AgeGroupType() 
                            { 
                                Name = "Young adult", AIAgeGroup = AIAgeGroup.YoungAdult, CanReproduce = false, Edge = 16f, HeightTargetModifier = 0.97f, WeightTargetModifier = 0.92f, ModelScaleFraction = 0.85f  
                               //, NeedTypes = new NeedType[]{ youngAdultSleepNeed }
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Adult", AIAgeGroup = AIAgeGroup.Adult, CanReproduce = true, Edge = 52f, HeightTargetModifier = 1f, WeightTargetModifier = 1f, ModelScaleFraction = 1f  
                              // , NeedTypes = new NeedType[]{ adultSleepNeed }
                            }
                            ,
                            new AgeGroupType() 
                            { 
                                Name = "Old", AIAgeGroup = AIAgeGroup.Old, CanReproduce = true, Edge = 60f, HeightTargetModifier = 0.9f, WeightTargetModifier = 1f, ModelScaleFraction = 1f  
                               //, NeedTypes = new NeedType[]{ oldSleepNeed }
                            }
                        } 
                    },
                    new CasteType() { KeyName = "female",
                        Reproduction = Reproduction.Female, Edge = 1f, //????
                        HeightMean = 1.90f, HeightStandardDeviation = 0.05f, 
                        WeightMean = 100f, WeightStandardDeviation = 0.10f,

                        AgeGroupTypes = new List<AgeGroupType>() 
                        { 
                            new AgeGroupType() 
                            { 
                                Name = "Baby", AIAgeGroup = AIAgeGroup.Baby, CanReproduce = false, Edge = 1.5f, HeightTargetModifier = 0.05f, WeightTargetModifier = 0.03f, ModelScaleFraction = 0.3f 
                             //   , NeedTypes = new NeedType[]{ babySleepNeed }
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Child", AIAgeGroup = AIAgeGroup.Child, CanReproduce = false, Edge = 11f, HeightTargetModifier = 0.6f, WeightTargetModifier = 0.5f, ModelScaleFraction = 0.6f  
                             // , NeedTypes = new NeedType[]{ childSleepNeed }
                            },
                            new AgeGroupType() 
                            { 
                                Name = "Young adult", AIAgeGroup = AIAgeGroup.YoungAdult, CanReproduce = false, Edge = 16f, HeightTargetModifier = 0.97f, WeightTargetModifier = 0.92f, ModelScaleFraction = 0.85f  
                              // , NeedTypes = new NeedType[]{ youngAdultSleepNeed }
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Adult", AIAgeGroup = AIAgeGroup.Adult, CanReproduce = true, Edge = 52f, HeightTargetModifier = 1f, WeightTargetModifier = 1f, ModelScaleFraction = 1f  
                             // , NeedTypes = new NeedType[]{ adultSleepNeed }
                            }
                            ,
                            new AgeGroupType() 
                            { 
                                Name = "Old", AIAgeGroup = AIAgeGroup.Old, CanReproduce = false, Edge = 60f, HeightTargetModifier = 0.9f, WeightTargetModifier = 1f, ModelScaleFraction = 1f  
                             // , NeedTypes = new NeedType[]{ oldSleepNeed }
                            }
                        } 
                    } 
                }

                };

                listOfEntityTypes.Add(spikePlantType); // remember to replace!! when copy pasting. else you will get an error that says 'duplicate key', for example

                #endregion

                #region Megapod needs
                NeedType wormNeed = new NeedType()
                {
                    KeyName = "foodEnergy",
                    FoodNeedType = new FoodNeedType()
                    { 
                        FoodNutrient = "foodEnergy",
                        RequiredNutrientsAsFractionOfEntityBulk = 0.04f
                    },
                    DecreasePerDay = new NormalDistribution() { Mean = 2f, StandardDeviation = 0.04f },                 
                    
                    LimitForDecreasedEnergy = 0.05f,
                    DecreasedEnergyWeight = 0.08f,
                    PhysicalEffects = new PhysicalEffects()
                    {
                        DaysAtZeroCausingCollapse = 4f,
                        DaysAtZeroCausingDeath = 4f,
                        DaysAtZeroDecreaseFactor = 1f, //0.5f,
                        LimitForReducedGrowth = 0.1f,
                        LimitForIncreasedSickness = 0.05f,
                        UseExertionFactorToDecrease = false                       
                    }
                };
                #endregion

                #region Megapod
                EntityType megapodType = new EntityType("entity:megapod")
                {

                    Name = "Megapod", //Swamp megapod
                    ThumbnailSmall = "HUD_thumbnail_worm",
                    SummaryDescription = "Large slug-like animal",  //
                    Description = "\n FEEDING CLASSIFICATION: Herbivore.\n \n SIZE: up to 600 cm\n \n ANATOMY\n Specialized for an amphibious lifestyle: On land, moves by rhythmic contractions of its 'foot' (for which we named the animal), while in water, it is aided by a powerful tail for swimming. Formidable tusks extend from the jawbones.\n \n BEHAVIOR\n The animal lives in swamps where it divides its time between family life above water and foraging below the waterline. Diet consists of underwater plants that it uproots with its strong forward-pointing tusks which are also used in fights for dominance with its own species and in defending against predators.\n \n THREAT LEVEL\n The animal will defend its territory but its land speed is slow enough for intruders to be able to escape without getting harmed.",
                   
                    RenderableType = new RenderableType()
                    {
                        RenderAsModelType = new RenderAsModelType()
                        {

                            ModelScale = 2.5f, // priority goes: race ModelScale / cast ModelScale / default (renderAdModelType ModelScale)
                            AssetName = "worm", //"worm"
                            ModelBasicTextureName = "WormTexture",
                            GaitAnimations = new XmlDictionary<string, GaitAnimationBracket[]>
                            {
                        
                                { "normal", new[]{                                           
                                              new GaitAnimationBracket(){ MinimumSpeed = 20f, MaximumSpeed = 35f, StrideLength = 22f, StrideDuration = 1.4f, AnimationKey = "walk"},     
                                              //mp mar 2015: this up here is mostly copied from bush dragon.  has no legs so I set stride duration to duration of walk cycle = 1.4 sec. 
                                              // the creature's  speed seems to be controlled further down, in LeggedLocomotorType. I set it low so that it moves slowly.
                                              } }
                            },
                            DefaultInfo = new AnimConditionInfo()
                            {
                                // use this neutral anim when all flags are cleared:
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "idle" } }
                            },
                            DefaultStances = new[] // "filler" anims used instead of idle when a stance has been set, but no action yet (happens between goals). This prevents unwanted switching to standing from kneeling, for instance
                            {
                                new  AnimConditionInfo()
                                {
                                    SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "idle" } }   
                                },
                                new  AnimConditionInfo()
                                {
                                    ConditionSet = new AnimConditions(){ Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Bold )},                              
                                    SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "combatIdle" } }   
                                }
                            },

                        AnimConditions = new AnimConditionInfo[] 
                        {
                            new AnimConditionInfo()
                            {                              
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Moving},                            
                                GaitSetKey = "normal"
                            },
                                           
                            new AnimConditionInfo() 
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "idle" }},                            
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle}                              
                            },

                            new AnimConditionInfo() 
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "eat" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Eating }                             
                            },


                             new AnimConditionInfo() 
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "combatIdle" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Bold)}                              
                            },
                              new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "attack" }}, 
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Attacking,}, 
                  //             Looping = Looping.No //mp 2015 why is this not used for animals but for a lot of human anims??  when i used it, the animal died after very short fight? without this, the animal fights for much longer????                       
  
                            },


                           new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ 
                                    BaseAnimations = new string[]{  "hit" },
                                    Sounds = new string[]{ "aliens/alienCombat/wormHitShort" }}, //MS: Had to cut this sound very short because the full animation never played
                                Playback = Playback.Manual,                             
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Recoiling }
                            },


                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{ "dead" }}, //no twitching anim made yet, so just using dead anim.
                      //          Sound = "aliens/rattleWooden",  bug - never stops ..21 oct 2013
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying }
                            },

                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "collapse" },//necessary.
                                Sounds = new string[]{ "aliens/alienCombat/wormDeathShort" }}, //MS: Had to cut this sound very short because the full animation never played                              
                                Looping = Looping.No,                                
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Pre)}  
                            },
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{ "dead"  }}, //necessary. put in name of dead anim when made
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Post)}  
                            }

                        }
                        }
                    },
                    LocomotorType = new LocomotorType()
                    {
                        MaxAngularSpeed = .25f * MathHelper.Pi, // MathHelper.Pi,
                        FourSidedSymmetry = false,
                        MeleeRadius = 32f,

                        LeggedLocomotorType = new LeggedLocomotorType()
                        {
                            TerrainNegateFactor = 0.1f,
                            WalkSlowSpeed = 5f, //  
                            WalkNormalSpeed = 6f, //debug only. *proper speed*
                            WalkFastSpeed = 8f, // 
                        },
                        CollisionResponderType = new CollisionResponderType()
                        {
                            AgentCollisionResponderType = new AgentCollisionResponderType()
                        }
                    },
                    SensorType = new SensorType()
                    {
                        Range = sensorRangeHuman + 100,
                        RangeAtNight = sensorRangeHumanNight + 100,
                        DetectionTypeKey = "defaultDetection"
                    },

                    BodyType = GameData.Instance.AllBodyTypes["worm"],

                    ContainerType = new AgentStorageType() //0.5f)
                    {
                        ItemStorageType = new ItemStorageType(0.8f), // can Haul... later.     MP: if you remove this line you get an error june 24 2014
                        StomachStorageType = new ItemStorageType(0.5f) // stomach size is dynamic - is defined in BioType
                        
                    },
                };

                megapodType.IntelligenceType = new IntelligenceType()
                {
                    IsMobile = true,
                    CanAttack = true,
                    CanUseWeapons = false,
                    CanHunt = true,
                    CanScout = true,
                    CanExamine = true, 
                    CanPatrol = true,
                    CanHaul = false,

                    IsPredator = true,
                    WillAttackNonThreatsNearby = true,
                    StrengthRating = StrengthRating.LikeHumans,
                    InterestInTriggerTypes = new[] { "mineTrigger", "spikeTrapTrigger" },
                    Courage = 0.3f,
                    MemoryInDays = 3f,
                    Boldness = 0.8f,
                    AggroRange = 250f,
                    ContainerTransactTag = "wormTransact", //was "worm"
                    ChanceToRestAfterMeleeAttack = 0.4,
                    MinRestTimeAfterAttackingInSeconds = 0.5f,
                    MaxRestTimeAfterAttackingInSeconds = 1.0f,

                    Prey = new string[] { "entity:mudWorm", "entity:binalRat", "entity:whiteThunderChicken", "entity:bajingan", "entity:pygmyThunderChicken", "entity:human" },
                   
                    Attacks = new[]
                    { 
                        "wormAttack", //mp the grown males should be stronger and more dangerous. how do we manage that?
                    },
                    Skills = new SerializableDictionary<string, float> 
                    { 
                        {
                           "unarmedFighting" , 0.3f
                        }
                    },
                };

                megapodType.BiologicalType = new BiologicalType() // remember to replace xxxxxType.! when copy pasting
                {
                    OrderKey = "wormOrder",
                    OxygenAndMuscleEnergyIncreaseRatePerDay = 7.5f,
                    TimeToConsumeFullMealInDays = 0.005f,
                    StomachSizeFractionOfEntityBulk = 0.25f,
                    StomachContentsDecreaseRatePerDay = 2,
                    // LPE: not predator - etas fish? whale?
                    FoodItemTagsThatCanBeConsumed = new[] { "cookedMeat", "rawMeat", "smallRawMeat", "spoiledMeal", "inedibleMeat", "rottenMeat" },
                    ExtractionProcessTypes = new[] { "extractMudWormMeat", "extractPatricianMeat", "extractLeafCutterMeat", "extractThunderChickenMeat", "extractBinalRatMeat", "extractTwinklerMeat", "extractBushDragonMeat", "extractSwampDemonTreeMeat", "extractDemonTreeMeat", "extractWhipjawMeat", "extractSpikePlantMeat", "extractForestGuardianMeat", }, //"extractThinThunderChickenMeat",
                    IsTerritorial = true,
                    MaxRegainLimit = humanMaxRegainLimit,
                    FractionOfMaxHitpointsGainedPerDay = humanFractionOfMaxHitpointsGainedPerDay,
                    ActiveStealthRating = 1f, //0.1f
                    Carcass = "item:megapodCarcass", //
                    Castes = new List<CasteType>() 
                { 
                    new CasteType() { KeyName = "male",
                        Reproduction = Reproduction.Male, Edge = 0.51f,
                        HeightMean = 2f, HeightStandardDeviation = 0.08f, 
                        WeightMean = 110f, WeightStandardDeviation = 0.15f,
                        AgeGroupTypes = new List<AgeGroupType>() 
                        { 
                            new AgeGroupType() 
                            { 
                                Name = "Baby", AIAgeGroup = AIAgeGroup.Baby, CanReproduce = false, Edge = 0.5f, HeightTargetModifier = 0.05f, WeightTargetModifier = 0.25f, ModelName = "wormThin", ModelBasicTextureName = "WormThinTexture", ModelScaleFraction = 0.4f,
                                NeedTypes = new []{ wormNeed}
                                /*,NeedTypes = new NeedType[]{ babySleepNeed }*/
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Child", AIAgeGroup = AIAgeGroup.Child, CanReproduce = false, Edge = 2f, HeightTargetModifier = 0.6f, WeightTargetModifier = 0.5f, ModelName = "wormThin", ModelBasicTextureName = "WormThinTexture", ModelScaleFraction = 0.75f,
                                NeedTypes = new []{ wormNeed}
                                // , NeedTypes = new NeedType[]{ childSleepNeed }
                            },
                            new AgeGroupType() 
                            { 
                                Name = "Young adult", AIAgeGroup = AIAgeGroup.YoungAdult, CanReproduce = false, Edge = 4f, HeightTargetModifier = 0.97f, WeightTargetModifier = 0.75f, ModelScaleFraction = 0.85f,  //0.85f
                                NeedTypes = new []{ wormNeed}
                                //, NeedTypes = new NeedType[]{ youngAdultSleepNeed }
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Adult", AIAgeGroup = AIAgeGroup.Adult, CanReproduce = true, Edge = 19f, HeightTargetModifier = 1f, WeightTargetModifier = 1f, ModelScaleFraction = 1f,
                                NeedTypes = new []{ wormNeed}
                                // , NeedTypes = new NeedType[]{ adultSleepNeed }
                            }
                            ,
                            new AgeGroupType() 
                            { 
                                Name = "Old", AIAgeGroup = AIAgeGroup.Old, CanReproduce = true, Edge = 20f, HeightTargetModifier = 0.9f, WeightTargetModifier = 1f, ModelScaleFraction = 1f,
                                NeedTypes = new []{ wormNeed}
                                //, NeedTypes = new NeedType[]{ oldSleepNeed }
                            }
                        } 
                    },
                    new CasteType() { KeyName = "female",
                        Reproduction = Reproduction.Female, Edge = 1f, //????
                        HeightMean = 1.90f, HeightStandardDeviation = 0.05f, 
                        WeightMean = 100f, WeightStandardDeviation = 0.10f,
                        AgeGroupTypes = new List<AgeGroupType>() 
                        { 
                            new AgeGroupType() 
                            { 
                                Name = "Baby", AIAgeGroup = AIAgeGroup.Baby, CanReproduce = false, Edge = 0.5f, HeightTargetModifier = 0.05f, WeightTargetModifier = 0.03f, ModelName = "wormThin", ModelBasicTextureName = "WormThinTexture", ModelScaleFraction = 0.4f,
                                NeedTypes = new []{ wormNeed}
                                /*,NeedTypes = new NeedType[]{ babySleepNeed }*/
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Child", AIAgeGroup = AIAgeGroup.Child, CanReproduce = false, Edge = 2f, HeightTargetModifier = 0.6f, WeightTargetModifier = 0.5f, ModelName = "wormThin", ModelBasicTextureName = "WormThinTexture", ModelScaleFraction = 0.75f,
                                NeedTypes = new []{ wormNeed}
                                // , NeedTypes = new NeedType[]{ childSleepNeed }
                            },
                            new AgeGroupType() 
                            { 
                                Name = "Young adult", AIAgeGroup = AIAgeGroup.YoungAdult, CanReproduce = false, Edge = 4f, HeightTargetModifier = 0.97f, WeightTargetModifier = 0.92f, ModelName = "wormThin", ModelBasicTextureName = "WormThinTexture", ModelScaleFraction = 0.85f,  
                                NeedTypes = new []{ wormNeed}
                                // , NeedTypes = new NeedType[]{ youngAdultSleepNeed }
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Adult", AIAgeGroup = AIAgeGroup.Adult, CanReproduce = true, Edge = 19f, HeightTargetModifier = 1f, WeightTargetModifier = 1f, ModelName = "wormThin", ModelBasicTextureName = "WormThinTexture", ModelScaleFraction = 0.9f,
                                NeedTypes = new []{ wormNeed}
                                // , NeedTypes = new NeedType[]{ adultSleepNeed }
                            }
                            ,
                            new AgeGroupType() 
                            { 
                                Name = "Old", AIAgeGroup = AIAgeGroup.Old, CanReproduce = false, Edge = 20f, HeightTargetModifier = 0.9f, WeightTargetModifier = 1f, ModelName = "wormThin", ModelBasicTextureName = "WormThinTexture", ModelScaleFraction = 0.9f, 
                                NeedTypes = new []{ wormNeed}
                                // , NeedTypes = new NeedType[]{ oldSleepNeed }
                            }
                        } 
                    } 
                }

                };

                listOfEntityTypes.Add(megapodType); // remember to replace!! when copy pasting. else you will get an error that says 'duplicate key', for example

                #endregion

                #region turniptype
                EntityType turnipType = new EntityType("entity:turnip")
                {
                    
                    Name = "Turnip",
                    ThumbnailSmall = "HUD_thumbnail_turnip",
                    SummaryDescription = "Plated grass eater",
                    Description = "\n FEEDING CLASSIFICATION: Herbivore\n \n HEIGHT: up to 3m\n \n ANATOMY\n This bulky animal belongs to the widespread class of four-part symmetric animals. From the basic blueprint it has developed a pyramidal shape topped by a cluster of sensory organs.\n The combination of heavy plating and a sensitive warning system seems to protect the creature from most predators. It also appears to have a defensive mechanism against brushfires.\n \n BEHAVIOR\n The grazer processes vast amounts of plant material as it roams the grasslands.\n When the order was first discovered, only the egg stage of the creature's life cycle was known and the creature was thus named for the egg's appearance: The egg resembles a turnip, as it slowly emerges from the ground.\n \n SURVIVAL GUIDE NOTES\n The creature appears docile and will attempt to stay out of trouble",
                    //mp took out: \n It was later realized that the adult Turnip procreates by every so often burying an egg. The egg lies dormant underground for several years until a second Turnip finds it and fertilizes it. The second Turnip also provides the egg with a packet of highly concentrated nutrients and a colony of small symbiotes, which, once it emerges from the ground, will help the young turnip through its vulnerable first years.
                   
                    RenderableType = new RenderableType()
               {
                   RenderAsModelType = new RenderAsModelType()
                   {

                       //ModelScale = 2.5f,// no need, priority goes: race ModelScale / cast ModelScale / default (renderAdModelType ModelScale)
                       AssetName = "turnip",
                       GaitAnimations = new XmlDictionary<string, GaitAnimationBracket[]>
                       {
                        
                        { "normal", new[]{                                           
                                              new GaitAnimationBracket(){ MinimumSpeed = 10f, MaximumSpeed = 22f, StrideLength = 12f, StrideDuration = 0.80f, AnimationKey = "gaitWalk"},     //MP: iterated strideduration 17th dec 2013.   ... was:       MinimumSpeed = 10f, MaximumSpeed = 22f, StrideLength = 12f, StrideDuration = 0.44f, AnimationKey = "gaitWalk"                                                                  
                                              } }
                       },

                       AnimConditions = new AnimConditionInfo[] 
                        {
                            new AnimConditionInfo()
                            {                              
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Moving},                            
                                GaitSetKey = "normal"
                            },
                                           
                            new AnimConditionInfo() 
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "tentaclesLooking" }}, //mp dont use idle fbx because it doesnt have any anim in it and is boring.
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle }     
                            }
                        }
                   }
               },
                    LocomotorType = new LocomotorType()
                    {

                        MaxAngularSpeed = .25f * MathHelper.Pi, // MathHelper.Pi,
                        FourSidedSymmetry = true,
                        MeleeRadius = 20f,

                        LeggedLocomotorType = new LeggedLocomotorType()
                        {
                            TerrainNegateFactor = 0.8f,
                            WalkSlowSpeed = 5f, // 8f, 
                            WalkNormalSpeed = 7f /*debug only*/, //18f /*proper speed*/, patrician:26f, turnip:11f
                            WalkFastSpeed = 12f, // 18f,//30f, // 63f,
                        },
                        CollisionResponderType = new CollisionResponderType()
                        {
                            AgentCollisionResponderType = new AgentCollisionResponderType()
                        }
                    },
                    SensorType = new SensorType()
                    {
                        Range = sensorRangeHuman,
                        RangeAtNight = sensorRangeHumanNight,
                        DetectionTypeKey = "defaultDetection"
                    },

                    BodyType = GameData.Instance.AllBodyTypes["turnip"]
                };



                turnipType.IntelligenceType = new IntelligenceType()
                { 
                    IsMobile = true,
                    CanAttack = false,
                    CanUseWeapons = false,
                    CanHunt = false,
                    CanScout = true,
                    CanExamine = true, 
                    CanPatrol = true,
                    CanHaul = false,
                    IsPredator = false,
                    StrengthRating = Entities.StrengthRating.VeryWeak, //was LikeHumans, Changed because that strenghtRating is to big for a creature without any attacks, it just creates frustration when an area of the map is blocked off because the agents are to scared to walk near peacefull animals
                    InterestInTriggerTypes = new[] { "mineTrigger", "spikeTrapTrigger" },
                    MemoryInDays = 0.002f, //mp new dec 2015. a short memory makes these timid animals more active around humans because the gigantic yellow threat zones dissappear quick.
                    /*  AttackTypes = new AttackType[]
               { 
                   new AttackType(){ Damage = AttackType.DamageTypes.Piercing }
               }*/
                    AggroRange = 0.0f
                };


                turnipType.BiologicalType = new BiologicalType()
                {
                    OxygenAndMuscleEnergyIncreaseRatePerDay = 5f,
                    TimeToConsumeFullMealInDays = 0.0125f,
                    StomachSizeFractionOfEntityBulk = 0.3f,
                    StomachContentsDecreaseRatePerDay = 2,
                    ActiveStealthRating = 0f,
                  
                    IsTerritorial = false,
                    ResilienceMean = 1f,
                    ResilienceStandardDeviation = 0.08f,
                    MaxRegainLimit = humanMaxRegainLimit,
                    FractionOfMaxHitpointsGainedPerDay = humanFractionOfMaxHitpointsGainedPerDay,
                    Carcass = "item:turnipCarcass",
                    RaceTypes = new[]
                                {     
                                    
                                    new RaceType()                              // Beige
                                    { Name="Pale Turnip", PortraitSkinType="Pale", PrimaryColor = "F7F4C5".ToColorVector3(), ModelBasicTextureName = "TurnipPaleTexture", Edge = 0.3f, ModelScale = 2.5f
                                    },

                                    new RaceType()                              // Beige
                                    { Name="Copper Turnip", PortraitSkinType="Dark", PrimaryColor = "F7F4C5".ToColorVector3(), ModelBasicTextureName = "TurnipDarkTexture", Edge = 0.4f, ModelScale = 2.5f
                                    },

                                    //new RaceType()                              // Black
                                    //{ Name="Lesser turnip", SkinType = "Black", PrimaryColor = "3F3411".ToColorVector3(), ModelBasicTextureName = "TurnipDarkTexture",  Edge = 0.9f, ModelScale=0.65f,  //normal size 2.5f
                                    //  BioProperties = new SerializableDictionary<string,BioProperty>{ { "SensorRange", new BioProperty(){ NumberValue = 300f} },  { "SensorRangeAtNight", new BioProperty(){ NumberValue = 300f} } },  //Small turnip
                                    //  Description = "\n FEEDING CLASSIFICATION: Omnivore, feeds on molluscs and seaweed.\n \n HEIGHT: up to 0.6m\n \n ANATOMY\n The smallest of the animal order we call turnips. Belongs to the widespread class of four-part symmetric animals. From the basic blueprint it has developed a pyramidal shape topped by a cluster of sensory organs.\n The combination of heavy plating and a sensitive warning system seems to protect the creature from most predators. It also appears to have a defensive mechanism against brushfires.\n \n BEHAVIOR\n Lives in coastal areas.\n When the order was first discovered, only the egg stage of the creature's life cycle was known and the creature was thus named for the egg's appearance: The egg resembles a turnip, as it slowly emerges from the ground.\n It was later realized that the adult Turnip procreates by every so often burying an egg. The egg lies dormant underground for several years until a second Turnip finds it and fertilizes it. The second Turnip also provides the egg with a packet of highly concentrated nutrients and a colony of small symbiotes, which, once it emerges from the ground, will help the young turnip through its vulnerable first years.\n \n THREAT FACTOR\n None. The creature appears docile and will attempt to stay out of trouble"
                                    //}
                                },
                    Castes = new List<CasteType>() 
                { 
                    new CasteType() { KeyName = "male", 
                        Reproduction = Reproduction.Male, Edge = 0.51f,
                        HeightMean = 3.2f, HeightStandardDeviation = 0.1f, 
                        WeightMean = 490f, WeightStandardDeviation = 15f,

                        AgeGroupTypes = new List<AgeGroupType>() 
                        { 
                            new AgeGroupType() 
                            { 
                                Name = "Baby", AIAgeGroup = AIAgeGroup.Baby, CanReproduce = false, Edge = 1.5f, HeightTargetModifier = 0.05f, WeightTargetModifier = 0.03f, ModelScaleFraction = 0.3f
                                /*, NeedTypes = new NeedType[]{ babySleepNeed }*/
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Child", AIAgeGroup = AIAgeGroup.Child, CanReproduce = false, Edge = 4f, HeightTargetModifier = 0.6f, WeightTargetModifier = 0.5f, ModelScaleFraction = 0.6f  
                               // , NeedTypes = new NeedType[]{ childSleepNeed }
                            },
                            new AgeGroupType() 
                            { 
                                Name = "Young adult", AIAgeGroup = AIAgeGroup.YoungAdult, CanReproduce = false, Edge = 16f, HeightTargetModifier = 0.97f, WeightTargetModifier = 0.92f, ModelScaleFraction = 0.85f  
                             //  , NeedTypes = new NeedType[]{ youngAdultSleepNeed }
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Adult", AIAgeGroup = AIAgeGroup.Adult, CanReproduce = true, Edge = 72f, HeightTargetModifier = 1f, WeightTargetModifier = 1f, ModelScaleFraction = 1f  
                              // , NeedTypes = new NeedType[]{ adultSleepNeed }
                            }
                            ,
                            new AgeGroupType() 
                            { 
                                Name = "Old", AIAgeGroup = AIAgeGroup.Old, CanReproduce = true, Edge = 80f, HeightTargetModifier = 0.9f, WeightTargetModifier = 1f, ModelScaleFraction = 1f  
                              // , NeedTypes = new NeedType[]{ oldSleepNeed }
                            }
                        } 
                    },
                    new CasteType() { KeyName = "female",
                        Reproduction = Reproduction.Female, Edge = 1f, //????
                        HeightMean = 2.90f, HeightStandardDeviation = 0.1f, 
                        WeightMean = 390f, WeightStandardDeviation = 10f,

                        AgeGroupTypes = new List<AgeGroupType>() 
                        { 
                            new AgeGroupType() 
                            { 
                                Name = "Baby", AIAgeGroup = AIAgeGroup.Baby, CanReproduce = false, Edge = 1.5f, HeightTargetModifier = 0.05f, WeightTargetModifier = 0.03f,  ModelScaleFraction = 0.3f
                                /*, NeedTypes = new NeedType[]{ babySleepNeed }*/
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Child", AIAgeGroup = AIAgeGroup.Child, CanReproduce = false, Edge = 4f, HeightTargetModifier = 0.6f, WeightTargetModifier = 0.5f, ModelScaleFraction = 0.6f  
                              //, NeedTypes = new NeedType[]{ childSleepNeed }
                            },
                            new AgeGroupType() 
                            { 
                                Name = "Young adult", AIAgeGroup = AIAgeGroup.YoungAdult, CanReproduce = false, Edge = 16f, HeightTargetModifier = 0.97f, WeightTargetModifier = 0.92f, ModelScaleFraction = 0.85f  
                              // , NeedTypes = new NeedType[]{ youngAdultSleepNeed }
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Adult", AIAgeGroup = AIAgeGroup.Adult, CanReproduce = true, Edge = 72f, HeightTargetModifier = 1f, WeightTargetModifier = 1f, ModelScaleFraction = 1f  
                             // , NeedTypes = new NeedType[]{ adultSleepNeed }
                            }
                            ,
                            new AgeGroupType() 
                            { 
                                Name = "Old", AIAgeGroup = AIAgeGroup.Old, CanReproduce = false, Edge = 80f, HeightTargetModifier = 0.9f, WeightTargetModifier = 1f, ModelScaleFraction = 1f  
                             // , NeedTypes = new NeedType[]{ oldSleepNeed }
                            }
                        } 
                    } 
                }

                };

                listOfEntityTypes.Add(turnipType);

                #endregion

           


                #region birdtype
                EntityType birdType = new EntityType("entity:bird")
                {
                    Name = "Diamond bird",
                    ThumbnailSmall = "HUD_thumbnail_diamondBird",
                    IsFlyer = true,
                    RenderableType = new RenderableType()
               {
                   RenderAsModelType = new RenderAsModelType()
                   {

                       //ModelScale = 2.1f, // no need, priority goes: race ModelScale / cast ModelScale / default (renderAdModelType ModelScale)
                       AssetName = "bird",
                       AnimConditions = new AnimConditionInfo[] 
                        {

                          new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "walk"}},//, "gaitJog", "gaitRun" },
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Moving }
                            },
                                           
                            new AnimConditionInfo() 
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "idle" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle }
                            }
                        }
                   }
               },
                    LocomotorType = new LocomotorType()
                    {
                        MaxAngularSpeed = .25f * MathHelper.Pi, // MathHelper.Pi,
                        FourSidedSymmetry = true,
                        MeleeRadius = 20f,

                        LeggedLocomotorType = new LeggedLocomotorType()
                        {
                            TerrainNegateFactor = 1f,
                            WalkSlowSpeed = 10f, // 8f, 
                            WalkNormalSpeed = 25f /*debug only*/, //18f /*proper speed*/, patrician:26f, turnip:11f
                            WalkFastSpeed = 24f, // 18f,//30f, // 63f,
                        },
                        CollisionResponderType = new CollisionResponderType()
                        {
                            AgentCollisionResponderType = new AgentCollisionResponderType()
                        }
                    },
                    SensorType = new SensorType()
                    {
                        Range = 100,// 11,
                        RangeAtNight = 50, // 7
                        DetectionTypeKey = "defaultDetection"
                    },

                    BodyType = GameData.Instance.AllBodyTypes["bird"]
                };


                birdType.IntelligenceType = new IntelligenceType()
                {
                    IsMobile = false,
                    StrengthRating = Entities.StrengthRating.VeryWeak, // WeakerThanHumans,
                    AggroRange = 0.0f,
                    MembersScoutingFraction = 0.0f,
                    ForageAndHuntingRadius = 0
                };



                birdType.BiologicalType = new BiologicalType()
                {
                    OxygenAndMuscleEnergyIncreaseRatePerDay = 10f,
                    TimeToConsumeFullMealInDays = 0.0125f,
                    StomachSizeFractionOfEntityBulk = 0.1f,
                    StomachContentsDecreaseRatePerDay = 2,
                    ActiveStealthRating = 0.3f,
                    MaxRegainLimit = humanMaxRegainLimit,
                    FractionOfMaxHitpointsGainedPerDay = humanFractionOfMaxHitpointsGainedPerDay,
                    Carcass = "item:birdCarcass",
                    RaceTypes = new[]
                                {     
                                    //MP june 2015: todo: make different colours. they are all white/yellow/red...
                                    new RaceType()                              // 
                                    { KeyName = "pale", Name="Pale race", PortraitSkinType="Pale", PrimaryColor = "F7F4C5".ToColorVector3(), ModelBasicTextureName = "BirdPaleTexture", Edge = 0.3f, ModelScale = 2.1f
                                    },

                                    new RaceType()                              // 
                                    { KeyName = "dark", Name="Dark race", PortraitSkinType="Dark", PrimaryColor = "564920".ToColorVector3(), ModelBasicTextureName = "BirdDarkTexture", Edge = 0.6f, ModelScale = 2.1f
                                    },
                                    new RaceType()                              // 
                                    { KeyName = "yellow", Name="Yellow race", PortraitSkinType="Yellow", PrimaryColor = "564920".ToColorVector3(), ModelBasicTextureName = "BirdYellowTexture", Edge = 0.6f, ModelScale = 2.1f
                                    },
                                    new RaceType()                              // 
                                    { KeyName = "red", Name="Red race", PortraitSkinType="Red", PrimaryColor = "564920".ToColorVector3(), ModelBasicTextureName = "BirdRedTexture", Edge = 0.6f, ModelScale = 2.1f
                                    },
                                    new RaceType()                              // 
                                    { KeyName = "black", Name="Black race", PortraitSkinType = "Black", PrimaryColor = "3F3411".ToColorVector3(), ModelBasicTextureName = "BirdBlackTexture", Edge = 0.9f, ModelScale = 2.1f
                                    },
                                    new RaceType()                              // this one is actually different in color. red/purple:
                                    { KeyName = "purple", Name="Purple race", PortraitSkinType = "Purple", PrimaryColor = "3F3411".ToColorVector3(), ModelBasicTextureName = "BirdPurpleTexture", Edge = 0.9f, ModelScale = 2.1f
                                    },
                                    new RaceType()                              // this one is very dark turqoise and smaller:
                                    { KeyName = "veryDarkTurqoise", Name="Cave race", PortraitSkinType = "Very Dark Turqoise", PrimaryColor = "3F3411".ToColorVector3(), ModelBasicTextureName = "BirdVeryDarkTurqoiseTexture", Edge = 0.9f, ModelScale = 0.8f
                                    },
                                    new RaceType()                              // this one is very dark green and smaller:
                                    { KeyName = "veryDarkGreen", Name="Cave race", PortraitSkinType = "Very Dark Green", PrimaryColor = "3F3411".ToColorVector3(), ModelBasicTextureName = "BirdVeryDarkGreenTexture", Edge = 0.9f, ModelScale = 1.1f
                                    }

                                },//
                    Castes = new List<CasteType>() 
                { 
                    new CasteType() {
                        KeyName = "male",
                        Reproduction = Reproduction.Male, Edge = 0.51f,
                        HeightMean = 0.5f, HeightStandardDeviation = 0.02f, 
                        WeightMean = 12f, WeightStandardDeviation = 0.15f,

                        AgeGroupTypes = new List<AgeGroupType>() 
                        { 
                            new AgeGroupType() 
                            { 
                                Name = "Baby", AIAgeGroup = AIAgeGroup.Baby, CanReproduce = false, Edge = 0.5f, HeightTargetModifier = 0.05f, WeightTargetModifier = 0.03f, ModelScaleFraction = 0.75f
                                /*, NeedTypes = new NeedType[]{ babySleepNeed }*/
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Child", AIAgeGroup = AIAgeGroup.Child, CanReproduce = false, Edge = 1f, HeightTargetModifier = 0.6f, WeightTargetModifier = 0.5f, ModelScaleFraction = 0.82f  
                              //  , NeedTypes = new NeedType[]{ childSleepNeed }
                            },
                            new AgeGroupType() 
                            { 
                                Name = "Young adult", AIAgeGroup = AIAgeGroup.YoungAdult, CanReproduce = false, Edge = 3f, HeightTargetModifier = 0.97f, WeightTargetModifier = 0.92f, ModelScaleFraction = 0.9f   
                             //  , NeedTypes = new NeedType[]{ youngAdultSleepNeed }
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Adult", AIAgeGroup = AIAgeGroup.Adult, CanReproduce = true, Edge = 18f, HeightTargetModifier = 1f, WeightTargetModifier = 1f, ModelScaleFraction = 1f  
                               //, NeedTypes = new NeedType[]{ adultSleepNeed }
                            }
                            ,
                            new AgeGroupType() 
                            { 
                                Name = "Old", AIAgeGroup = AIAgeGroup.Old, CanReproduce = true, Edge = 20f, HeightTargetModifier = 0.9f, WeightTargetModifier = 1f, ModelScaleFraction = 1f  
                              // , NeedTypes = new NeedType[]{ oldSleepNeed }
                            }
                        } 
                    },
                    new CasteType() { 
                        KeyName = "female",
                        Reproduction = Reproduction.Female, Edge = 1f, //????
                        HeightMean = 0.5f, HeightStandardDeviation = 0.02f, 
                        WeightMean = 12f, WeightStandardDeviation = 0.15f,

                        AgeGroupTypes = new List<AgeGroupType>() 
                        { 
                            new AgeGroupType() 
                            { 
                                Name = "Baby", AIAgeGroup = AIAgeGroup.Baby, CanReproduce = false, Edge = 0.5f, HeightTargetModifier = 0.05f, WeightTargetModifier = 0.03f, ModelScaleFraction = 0.75f
                                /*, NeedTypes = new NeedType[]{ babySleepNeed }*/
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Child", AIAgeGroup = AIAgeGroup.Child, CanReproduce = false, Edge = 1f, HeightTargetModifier = 0.6f, WeightTargetModifier = 0.5f, ModelScaleFraction = 0.82f  
                              //, NeedTypes = new NeedType[]{ childSleepNeed }
                            },
                            new AgeGroupType() 
                            { 
                                Name = "Young adult", AIAgeGroup = AIAgeGroup.YoungAdult, CanReproduce = false, Edge = 3f, HeightTargetModifier = 0.97f, WeightTargetModifier = 0.92f, ModelScaleFraction = 0.9f  
                             //  , NeedTypes = new NeedType[]{ youngAdultSleepNeed }
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Adult", AIAgeGroup = AIAgeGroup.Adult, CanReproduce = true, Edge = 19f, HeightTargetModifier = 1f, WeightTargetModifier = 1f, ModelScaleFraction = 1f
                             // , NeedTypes = new NeedType[]{ adultSleepNeed }
                            }
                            ,
                            new AgeGroupType() 
                            { 
                                Name = "Old", AIAgeGroup = AIAgeGroup.Old, CanReproduce = false, Edge = 22f, HeightTargetModifier = 0.9f, WeightTargetModifier = 1f, ModelScaleFraction = 1f  
                            //  , NeedTypes = new NeedType[]{ oldSleepNeed }
                            }
                        } 
                    } 
                }

                };

                listOfEntityTypes.Add(birdType);

                #endregion


                #region Twinkler needs
               /* NeedType twinklerFoodNeed = new NeedType()
                {
                    KeyName = "foodEnergy",
                    NeedClass = NeedClass.Food,
                    DecreasePerDayMean = 0.7f, // 1.4f, 
                    DecreasePerDayStandardDeviation = 0.02f,
                    LimitForDecreasedEnergy = 0.15f,
                    DecreasedEnergyWeight = 0.6f,
                    PhysicalNeedType = new PhysicalNeedType()
                    {
                        DaysAtZeroCausingCollapse = 2f,
                        DaysAtZeroCausingDeath = 2.05f,
                        DaysAtZeroDecreaseFactor = 0.5f,
                        UseExertionFactorToDecrease = true,
                        FoodNutrient = GameData.Instance.AllFoodNutrientTypes["foodEnergy"],
                        RequiredNutrientsAsFractionOfEntityBulk = 0.6f * dailyMealBulk / 1f  //  = 0,15....Lars suggests 0,25f bulk food pr game day (dailyMealBulk defined 12 lines up). with 60% energy from an ideal meal, for a man weighing 1f bulk, the RequiredNutrientsAsFractionOfEntityBulk is 0,6*0,25/1f
                    },
                };*/

                // only need protein:
                NeedType twinklerProteinNeed = new NeedType()
                {
                    KeyName = "protein",
                    FoodNeedType = new FoodNeedType() 
                    { 
                        FoodNutrient = "protein", 
                        RequiredNutrientsAsFractionOfEntityBulk = 0.03f   //0.012f   answering the hover-tooltip: we assume a days' worth of recommended protein. 0,012 f bulk protein/gameday / 1f = 0,012f   ...... calculated here (under protein) https://docs.google.com/a/unclaimedworld-game.com/document/d/106BIllzcCkma24NlcWCZLwyEpHaG6AWed0GpSUUMTug/edit
               
                    },
                    DecreasePerDay = new NormalDistribution() { Mean = 3f, StandardDeviation = 0.02f },   

                    LimitForDecreasedEnergy = 0.05f,
                    DecreasedEnergyWeight = 0.08f, // 0.6f,
                    PhysicalEffects = new PhysicalEffects()
                    {
                        DaysAtZeroCausingCollapse = 2f,
                        DaysAtZeroCausingDeath = 2.1f,
                        DaysAtZeroDecreaseFactor = 1f, //0.5f,
                        LimitForReducedGrowth = 0.1f,
                        LimitForIncreasedSickness = 0.05f,
                        UseExertionFactorToDecrease = false                             
                    }
                };

                #endregion
                #region alternative twinkler skins / model
                /*
                 * ModelName = "twinklerThinSpiky", ModelBasicTextureName = "QuaditeThinSpikyTexture"
                 * ModelName = "twinklerThin",      ModelBasicTextureName = "QuaditeThinTexture",
                 */
                #endregion
                #region twinklertype
                BodyType bodyType = GameData.Instance.AllBodyTypes["twinkler"];  //needs species entity type set up MP mar 2014
                EntityType twinklerType = new EntityType("entity:twinkler")
                {
                    Name = "Twinkler", //"Quadite",
                    ThumbnailSmall = "HUD_thumbnail_twinkler",
                    SummaryDescription = "Dangerous carnivore",
                    Description = "\n FEEDING CLASSIFICATION: Carnivorous predator\n \n HEIGHT: Up to 1 m\n \n ANATOMY\n Species of quadites named for their nightly display of blinking light signals, believed to be used for communication with other members of the hive.\n Mouth opening is situated on the bottom of the body, the digestive canals extending radially from the center. On the top are bioluminescent communication organs as well as the sensory receptors.\n Covered in a strong exoskeleton. All four legs are capable of stabbing and injecting venom.\n \n BEHAVIOR\n As is the case with the other Quadite species, the Twinkler quadite lives in colonies. We have reports of colonies that hold up to a hundred individuals.\n They emerge from their burrows to hunt in packs, behaving much like wolves. Primary food source is the thunder chicken, but their diet appears to be varied.\n \n SENSES\n We believe they locate prey through vision and smell. Senses are highly developed, especially the eyes, which are able to perceive multispectral images as well as polarized light, an ability which probably aids the animal with hitting its prey.\n Infrared vision  allows them to hunt very efficiently at night while maintaining constant communication.\n \n SURVIVAL GUIDE NOTES\n Threat factor ranges from Medium to High. Humans are sometimes seen as prey and the speed and numbers of twinklers make them able to overrun us. Their hard shell and sharp talons present a demanding challenge in close combat and it is therefore recommended that they be dispatched from a distance with ranged weapons.\n \n COMPETITORS\n Twinklers are apex predators (meaning at the top of their food chain) and in some territories compete with the Black Patrician, another apex predator.",

                    //  Description = "\n FEEDING CLASSIFICATION: Carnivore. Predator or scavenger.\n \n HEIGHT: Up to 1 m\n \n ANATOMY\n Order of animals widespread on this continent. Name hints at their 4-corner shape.\n Mouth opening is situated on the bottom of the body, the digestive canals extending radially from the center. On the top are bioluminescent communication organs as well as the sensory receptors.\n Covered in a strong exoskeleton. Usually, all four legs are capable of stabbing and injecting venom. Venom strength varies but is often lethal. \n \n BEHAVIOR\n Most quadites are social animals, living in smaller packs or large colonies. The most dangerous quadite we have encountered, the Swarm quadite, had several thousand individuals forming a highly ferocious swarm.\n Inter-species collaboration is sometimes seen when different quadites are hunting.\n Quadites emerge from burrows, caves and crevices to hunt the plains. They prefer dry, firm soil and are rarely seen in swamps or marshes. Food sources are a wide range of animals primarily the thunder chicken, but their diet appears to be varied.\n \n SENSES\n We believe they locate prey through vision and smell. Senses are highly developed, especially the eyes, which are able to perceive multispectral images as well as polarized light, an ability which probably aids the animal with hitting its prey.\n Infrared vision  allows them to hunt very efficiently at night while maintaining constant communication.\n \n THREAT FACTOR\n Medium to High. Humans are sometimes seen as prey and the speed and numbers of quadites make them able to overrun us. Their hard shell and sharp talons present a demanding challenge in close combat and it is therefore recommended that they be dispatched from a distance with ranged weapons.\n \n COMPETITORS\n Compete with other species of quadites and often patricians.",
                   
                    RenderableType = new RenderableType()
                    {
                        RenderAsModelType = new RenderAsModelType()
                        {

                            //ModelScale = 2.5f,// no need, priority goes: race ModelScale / cast ModelScale / default (renderAdModelType ModelScale)//normal size:2.5f, small size:1.5f   //per Dec 2012, this is overwritten by any data put in age, race or caste, in that order
                            AssetName = "twinkler",
                            GaitAnimations = new XmlDictionary<string, GaitAnimationBracket[]>
                       {
                        
                        { "normal", new[]{                                           
                                              new GaitAnimationBracket(){ MinimumSpeed = 35f, MaximumSpeed = 77f, StrideLength = 27f, StrideDuration = 0.9f, AnimationKey = "gaitWalk"},  //MP: made a quick iteration on strideduration 17th dec 2013.   values were: :MinimumSpeed = 35f, MaximumSpeed = 77f, StrideLength = 27f, StrideDuration = 0.46f,
                                            
                                              } }

                       },

                            AnimConditions = new AnimConditionInfo[] 
                        {

                            new AnimConditionInfo()
                            {
                              
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Moving},
                                GaitSetKey = "normal"
                            },
                                           
                            new AnimConditionInfo() 
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "idle" }},
                           //     Sound = "aliens/croaker",   error...gets played a lot when they are dead....18th oct 2013
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle }           
                            },
                             new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "attackLowRight" }}, 
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Attacking,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Low, (int)AnimModifier.Right)},
                                //Forbiddens = new BitMask64(typeof(AnimState), (int)AnimState.Moving)
  
                            },
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "attackHighRight" }}, 
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Attacking,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.High, (int)AnimModifier.Right)},
                                //Forbiddens = new BitMask64(typeof(AnimState), (int)AnimState.Moving)  
                            },
                             new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "hit","hit" }, //mp example of series of anims and sounds. in case you want to use different sound effects for the same animation.
                                Sounds = new string[]{ "aliens/hummingClickClacking","aliens/hummingClickClacking" }},
                              //  Sound = "aliens/hummingClickClacking",
                                Playback = Playback.Manual, 
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Recoiling }
                            },

                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{ "dying" }},
                           //     Sound = "aliens/toothCrickets",  bug -never stops ..21 oct
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying }
                            },

                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "dead" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Post)}  
                            },
                             
                        }
                        }
                    },
                    LocomotorType = new LocomotorType()
                    {

                        MaxAngularSpeed = .25f * MathHelper.Pi, // MathHelper.Pi,
                        FourSidedSymmetry = true,
                        MeleeRadius = 12f,

                        LeggedLocomotorType = new LeggedLocomotorType()
                        {
                            TerrainNegateFactor = 0.3f,
                            WalkSlowSpeed = 20f, // 8f, 
                            WalkNormalSpeed = 22f /*debug only*/,  /*proper speed*///, ModelScale = 2.5f : (NORMAL twinkler walk speed=1.0, basespeed=25f ) (SLOW twinkler walk speed=0.7, basespeed=21f) (FAST twinkler walk speed=2, basespeed=75f) 
                            //, ModelScale = 1.5f : (NORMAL twinkler walk speed=1.5, basespeed=21f )
                            WalkFastSpeed = 24f, // 18f,//30f, // 63f,
                        },
                        CollisionResponderType = new CollisionResponderType()
                        {
                            AgentCollisionResponderType = new AgentCollisionResponderType()
                        }
                    },
                    SensorType = new SensorType()
                    {
                        Range = 1000, // 25,//102, //6 // DEMO hack to make twinklers see humans - does this huge sensor range kill FPS? I'm not sure...
                        RangeAtNight = 1000, // 25//102
                        DetectionTypeKey = "defaultDetection"
                    },

                    BodyType = bodyType,
                    ContainerType = new AgentStorageType() //0.5f)
                    {
                        ItemStorageType = new ItemStorageType(0.5f), // can Haul... later.
                        StomachStorageType = new ItemStorageType(0.07f) // stomach size is dynamic - is defined in BioType
                        
                    }
                };


                twinklerType.IntelligenceType = new IntelligenceType()
                {
                    MembersScoutingFraction = 1f, //dec 3, was 1f
                    IsMobile = true,
                    CanAttack = true,
                    CanUseWeapons = false,
                    CanHunt = true,
                    CanScout = true,
                    CanExamine = true, 
                    CanPatrol = true,
                    CanHaul = false,
                    IsPredator = true,
                    WillAttackNonThreatsNearby = true,
                    ContainerTransactTag = "twinklerTransact", //was "twinkler"
                    InterestInTriggerTypes = new[] { "mineTrigger", "spikeTrapTrigger", "smallImprovisedTrapTrigger" },
                    StrengthRating = Entities.StrengthRating.WeakerThanHumans,
                    Boldness = 0.25f, //Boldness is low at the moment to allow baby and child twinklersd to flee, the bigger ones should still be able to attack threats //1f
                    Courage = 0.05f,   //0.1f 0.2f   0.9f  // 0.05f . wanted them to flee more. 0.05f didn't make them flee either...tweak also vitalBodyPartDamageEvaluationBoost in constants.cs
                    MemoryInDays = 2f,

                    Prey = new string[] { "entity:mudWorm", "entity:binalRat", "entity:whiteThunderChicken", "entity:pygmyThunderChicken", "entity:human" },
                  
                    Attacks = new[]
                     { 
                         "twinklerLowRight",
                         "twinklerHighRight"
                         //new AttackType(){ Damage = AttackType.DamageTypes.Piercing, DamageMean=3, DamageStandardDeviation=1.5f, AnimationKey = "attackLowRight"/*, DependsOn = new BodyPartType[]{ bodyType.FindBodyPart("Right arm") }*/ },
                         //new AttackType(){ Damage = AttackType.DamageTypes.Piercing, DamageMean=4, DamageStandardDeviation=1.5f, AnimationKey = "attackHighRight"/*, DependsOn = new BodyPartType[]{ bodyType.FindBodyPart("Right arm") }*/ }
                         
                     },
                    Skills = new SerializableDictionary<string, float> 
                    { 
                        {
                           "unarmedFighting" , 0.2f
                        }
                    },
                    AggroRange = sensorRangeHuman + 100,
                    AssistanceRange = 600f // NEW!!!
                };



                twinklerType.BiologicalType = new BiologicalType()
                {
                    OrderKey = "quaditeOrder",
                    OxygenAndMuscleEnergyIncreaseRatePerDay = 12f,
                    FoodItemTagsThatCanBeConsumed = new[] { "cookedMeat", "rawMeat", "smallRawMeat", "spoiledMeal", "inedibleMeat", "rottenMeat" },
                    ExtractionProcessTypes = new[] { "extractMudWormMeat", "extractPatricianMeat", "extractLeafCutterMeat", "extractThunderChickenMeat", "extractBinalRatMeat", "extractTwinklerMeat", "extractBushDragonMeat", "extractSwampDemonTreeMeat", "extractDemonTreeMeat", "extractMegapodMeat", "extractWhipjawMeat", "extractSpikePlantMeat", "extractForestGuardianMeat", },//"extractThinThunderChickenMeat",
                    TimeToConsumeFullMealInDays = 0.005f, //was 0.009f dec 2014.   was 0.015f MP
                    StomachSizeFractionOfEntityBulk = 0.3f, // big stomach so can eat quickly // 0.2f,
                    StomachContentsDecreaseRatePerDay = 3,
                    ActiveStealthRating = 0.2f,
                   
                    MaxRegainLimit = humanMaxRegainLimit,
                    FractionOfMaxHitpointsGainedPerDay = humanFractionOfMaxHitpointsGainedPerDay,
                    Carcass = "item:quaditeCarcass",

                    ResilienceMean = 7f,
                    ResilienceStandardDeviation = 0.08f,

                    BioPropertyTypes = new[]{                       
                         new BioPropertyType(){
                          KeyName = "SensorRange", //measured in pixels  (White)    //aggrorange and assistancerange can be seen in debug panel>ranges, however if very big, will not be seen. see: public void DrawRanges() for colors.
                          InterpolateSetting = BioPropertyType.Interpolate.DefaultPriority                     
                     },
                      new BioPropertyType(){
                          KeyName = "SensorRangeAtNight", //measured in pixels
                          InterpolateSetting = BioPropertyType.Interpolate.DefaultPriority                     
                     },
                     new BioPropertyType(){ 
                          KeyName = "AggroRange", //measured in pixels  (red)
                          InterpolateSetting = BioPropertyType.Interpolate.DefaultPriority                     
                     },
                     new BioPropertyType(){
                          KeyName = "AssistanceRange", //measured in pixels  (green)
                          InterpolateSetting = BioPropertyType.Interpolate.DefaultPriority                     
                     }},
                    /*   RaceTypes = new List<RaceType>()
                                   {     
                                    
                                       new RaceType()                              // Beige
                                       { KeyName = "Hunter",  Name="Twinkler Quadite", SkinType="Pale", PrimaryColor = "F7F4C5".ToColorVector3(), ModelBasicTextureName = "QuaditeYellowTexture", Edge = 0.3f, ResilienceMean = 7f, ResilienceStandardDeviation = 0.08f, ModelScale=2.5f,  //normal size 2.5f
                                         BioProperties = new SerializableDictionary<string,BioProperty>{ { "SensorRange", new BioProperty(){ NumberValue = 1000f} },  { "SensorRangeAtNight", new BioProperty(){ NumberValue = 1000f} }, { "AggroRange", new BioProperty(){ NumberValue = 950f} }, { "AssistanceRange", new BioProperty(){ NumberValue = 1200f} }},  //Hunter quadite
                                         Description = "\n FEEDING CLASSIFICATION: Carnivorous predator\n \n HEIGHT: Up to 1 m\n \n ANATOMY\n Species of quadites named for their nightly display of blinking light signals, believed to be used for communication with other members of the hive.\n Mouth opening is situated on the bottom of the body, the digestive canals extending radially from the center. On the top are bioluminescent communication organs as well as the sensory receptors.\n Covered in a strong exoskeleton. All four legs are capable of stabbing and injecting venom.\n \n BEHAVIOR\n As is the case with the other Quadite species, the Twinkler quadite lives in colonies. We have reports of colonies that hold up to a hundred individuals.\n They emerge from their burrows to hunt in packs, behaving much like wolves. Primary food source is the thunder chicken, but their diet appears to be varied.\n \n SENSES\n We believe they locate prey through vision and smell. Senses are highly developed, especially the eyes, which are able to perceive multispectral images as well as polarized light, an ability which probably aids the animal with hitting its prey.\n Infrared vision  allows them to hunt very efficiently at night while maintaining constant communication.\n \n THREAT FACTOR\n Medium to High. Humans are sometimes seen as prey and the speed and numbers of twinklers make them able to overrun us. Their hard shell and sharp talons present a demanding challenge in close combat and it is therefore recommended that they be dispatched from  a distance with ranged weapons.\n \n COMPETITORS\n Twinklers are apex predators (meaning at the top of their food chain) and in some territories compete with the Black Patrician, another apex predator."
                                       },    
                                  
                                       new RaceType()                              // Dark
                                       { KeyName = "Guard", Name="Twinkler Quadite", SkinType="Dark", PrimaryColor = "564920".ToColorVector3(), ModelBasicTextureName = "QuaditeRedTexture", Edge = 0.6f, ResilienceMean = 7f, ResilienceStandardDeviation = 0.08f, ModelScale=2.1f,
                                           BioProperties = new SerializableDictionary<string,BioProperty>{ { "SensorRange", new BioProperty(){ NumberValue = 576f} },  { "SensorRangeAtNight", new BioProperty(){ NumberValue = 576f} }, { "AggroRange", new BioProperty(){ NumberValue = 210f} }, { "AssistanceRange", new BioProperty(){ NumberValue = 300f} }  },  //Guard quadite
                                           Description = "\n FEEDING CLASSIFICATION: Carnivorous predator\n \n HEIGHT: Up to 75 cm\n \n ANATOMY\n Small and nimble quadite. Displays blinking light signals, believed to be used for communication with other members of the hive.\n Mouth opening is situated on the bottom of the body, the digestive canals extending radially from the center. On the top are bioluminescent communication organs as well as the sensory receptors.\n Covered in a strong exoskeleton. All four legs are capable of stabbing and injecting venom.\n \n BEHAVIOR\n As is the case with the other Quadite species, the Twinkler Quadite lives in colonies. We have reports of colonies that hold up to a hundred individuals.\n They emerge from their burrows to hunt in packs, behaving much like wolves. Primary food source is the thunder chicken, but their diet appears to be varied.\n \n SENSES\n We believe they locate prey through vision and smell. Senses are highly developed, especially the eyes, which are able to perceive multispectral images as well as polarized light, an ability which probably aids the animal with hitting its prey.\n Infrared vision  allows them to hunt very efficiently at night while maintaining constant communication.\n \n THREAT FACTOR\n Medium to High. Humans are sometimes seen as prey and the speed and numbers of twinklers make them able to overrun us. Their hard shell and sharp talons present a demanding challenge in close combat and it is therefore recommended that they be dispatched from  a distance with ranged weapons.\n \n COMPETITORS\n Twinklers are apex predators (meaning at the top of their food chain) and in some territories compete with the Black Patrician, another apex predator."
                                       },

                                       // TODO: make into SPECIES instead
                                      new RaceType()                              
                                       { Name="Tiger Quadite", SkinType="Red", PrimaryColor = "564920".ToColorVector3(), ModelBasicTextureName = "QuaditeStripedTexture", Edge = 0.65f, ResilienceMean = 7f, ResilienceStandardDeviation = 0.08f, ModelScale=2.7f,
                                           Description = "\n FEEDING CLASSIFICATION: Carnivore. Scavenges and hunts.\n \n HEIGHT: 115 cm\n \n ANATOMY\n Large, lumbering species of quadite.\n Mouth opening is situated on the bottom of the body, the digestive canals extending radially from the center. On the top are bioluminescent communication organs as well as the sensory receptors.\n Covered in a strong exoskeleton. All four legs are capable of stabbing and injecting venom.\n \n BEHAVIOUR\n As is the case with the other Quadite species, the Twinkler Quadite lives in colonies. We have reports of colonies that hold up to a hundred individuals.\n They emerge from their burrows to hunt in packs, behaving much like wolves. Primary food source is the thunder chicken, but their diet appears to be varied.\n \n SENSES\n We believe they locate prey through vision and smell. Senses are highly developed, especially the eyes, which are able to perceive multispectral images as well as polarized light, an ability which probably aids the animal with hitting its prey.\n Infrared vision  allows them to hunt very efficiently at night while maintaining constant communication.\n \n THREAT FACTOR\n Medium to High. Humans are sometimes seen as prey and the speed and numbers of twinklers make them able to overrun us. Their hard shell and sharp talons present a demanding challenge in close combat and it is therefore recommended that they be dispatched from  a distance with ranged weapons.\n \n COMPETITORS\n Twinklers are apex predators (meaning at the top of their food chain) and in some territories compete with the Black Patrician, another apex predator."
                                       },

                                       new RaceType()                              // Black
                                       { Name="Highland Quadite", SkinType = "Black", PrimaryColor = "3F3411".ToColorVector3(), ModelBasicTextureName = "QuaditeTurquoiseTexture", Edge = 0.7f, ResilienceMean = 7f, ResilienceStandardDeviation = 0.08f, ModelScale=2.3f,
                                           Description = "\n FEEDING CLASSIFICATION: Probably carnivore\n \n HEIGHT: Up to 1 m\n \n ANATOMY\n Newly discovered quadite. At a glance, appears to share many characteristics with the Twinkler quadite. More research is needed."
                                       }
                                   },*/
                    Castes = new List<CasteType>() 
                { 
                    new CasteType() { 
                        KeyName = "hunter",
                       // SkinType="Pale", 
                        PrimaryColor = "F7F4C5".ToColorVector3(), ModelBasicTextureName = "QuaditeYellowTexture", 
                        Edge = 0.3f, 
                        ModelScale=2.5f,  //normal size 2.5f
                        ModelName = "twinkler", 
                        BioProperties = new SerializableDictionary<string,BioProperty>{ { "SensorRange", new BioProperty(){ NumberValue = 1000f} },  
                         { "SensorRangeAtNight", new BioProperty(){ NumberValue = 1000f} }, 
                         { "AggroRange", new BioProperty(){ NumberValue = 950f} }, //AF: was 950, reduced to 200f for twinkler eating test    MP: and never increased back up?? dec 2014
                         { "AssistanceRange", new BioProperty(){ NumberValue = 1200f} }},  //Hunter quadite
                                    
                        Reproduction = Reproduction.None, //Edge = 0.9f,
                        HeightMean = 0.6f, HeightStandardDeviation = 0.02f, 
                        WeightMean = 30f, WeightStandardDeviation = 3f,
                        
                        AgeGroupTypes = new List<AgeGroupType>()   // http://www.enchantedlearning.com/subjects/animals/Animalbabies.shtml
                        { 
                            new AgeGroupType() 
                            { 
                                Name = "Hatchling", AIAgeGroup = AIAgeGroup.Baby, CanReproduce = false, Edge = 1.5f, HeightTargetModifier = 0.05f, WeightTargetModifier = 0.03f,  // "Baby"
                                ModelScaleFraction = 0.2f/*, 
                                NeedTypes = new NeedType[]{ babySleepNeed }*/
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Hatchling", AIAgeGroup = AIAgeGroup.Child, CanReproduce = false, Edge = 11f, HeightTargetModifier = 0.6f, WeightTargetModifier = 0.5f,  // "Child"
                                ModelScaleFraction = 0.6f,
                                //, NeedTypes = new NeedType[]{ childSleepNeed }
                            },
                            new AgeGroupType() 
                            { 
                                Name = "Grown", AIAgeGroup = AIAgeGroup.YoungAdult, CanReproduce = false, Edge = 16f, HeightTargetModifier = 0.97f, WeightTargetModifier = 0.92f,  // "Young adult"
                                ModelScaleFraction = 0.9f, 
                                NeedTypes = new []{ twinklerProteinNeed } //not in 0.11
                                //  , NeedTypes = new NeedType[]{ youngAdultSleepNeed }
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Grown", AIAgeGroup = AIAgeGroup.Adult, CanReproduce = true, Edge = 72f, HeightTargetModifier = 1f, WeightTargetModifier = 1f,  // "Adult"
                               ModelScaleFraction = 1.0f,
                               NeedTypes = new []{ twinklerProteinNeed } //not in 0.11
                                //, NeedTypes = new NeedType[]{ adultSleepNeed }
                            }
                            ,
                            new AgeGroupType() 
                            { 
                                Name = "Grown", AIAgeGroup = AIAgeGroup.Old, CanReproduce = true, Edge = 200f, HeightTargetModifier = 0.9f, WeightTargetModifier = 1f,  //"Old"
                                ModelScaleFraction = 0.95f,
                                NeedTypes = new []{ twinklerProteinNeed } //not in 0.11
                              // , NeedTypes = new NeedType[]{ oldSleepNeed }
                            }
                        } 
                    },
                    // TODO: remove the Guards caste - replace with a jobs generator that sets Patrol jobs around the nests
                    new CasteType() {
                        KeyName = "guard",
                        Reproduction = Reproduction.None, //Edge = 1f,
                        HeightMean = .70f, HeightStandardDeviation = 0.02f, 
                        WeightMean = 25f, WeightStandardDeviation = 2f,
                        PrimaryColor = "564920".ToColorVector3(), ModelBasicTextureName = "QuaditeRedTexture", 
                        Edge = 0.6f, 
                        ModelScale=2.1f,
                                        
                        BioProperties = new SerializableDictionary<string,BioProperty>{ 
                        { "SensorRange", new BioProperty(){ NumberValue = 576f} },  
                        { "SensorRangeAtNight", new BioProperty(){ NumberValue = 576f} }, 
                        { "AggroRange", new BioProperty(){ NumberValue = 210f} }, 
                        { "AssistanceRange", new BioProperty(){ NumberValue = 300f} }  },  //Guard quadite
                                    
                       

                        AgeGroupTypes = new List<AgeGroupType>() 
                        { 
                            new AgeGroupType() 
                            { 
                                Name = "Hatchling", AIAgeGroup = AIAgeGroup.Baby, CanReproduce = false, Edge = 1.5f, HeightTargetModifier = 0.05f, WeightTargetModifier = 0.03f/*, 
                                NeedTypes = new NeedType[]{ babySleepNeed }*/
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Hatchling", AIAgeGroup = AIAgeGroup.Child, CanReproduce = false, Edge = 11f, HeightTargetModifier = 0.6f, WeightTargetModifier = 0.5f  
                             // , NeedTypes = new NeedType[]{ childSleepNeed }
                            },
                            new AgeGroupType() 
                            { 
                                Name = "Grown", AIAgeGroup = AIAgeGroup.YoungAdult, CanReproduce = false, Edge = 16f, HeightTargetModifier = 0.97f, WeightTargetModifier = 0.92f,
                                NeedTypes = new []{ twinklerProteinNeed }
                              
                             //  , NeedTypes = new NeedType[]{ youngAdultSleepNeed }
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Grown", AIAgeGroup = AIAgeGroup.Adult, CanReproduce = true, Edge = 72f, HeightTargetModifier = 1f, WeightTargetModifier = 1f,
                                NeedTypes = new []{ twinklerProteinNeed }
                             // , NeedTypes = new NeedType[]{ adultSleepNeed }
                            }
                            ,
                            new AgeGroupType() 
                            { 
                                Name = "Grown", AIAgeGroup = AIAgeGroup.Old, CanReproduce = false, Edge = 200f, HeightTargetModifier = 0.9f, WeightTargetModifier = 1f,
                                NeedTypes = new []{ twinklerProteinNeed }
                             // , NeedTypes = new NeedType[]{ oldSleepNeed }
                            }
                        } 
                    } 
                }

                };

                listOfEntityTypes.Add(twinklerType);

                #endregion
//NEW swarmer quadite:
                #region swarmertype
                bodyType = GameData.Instance.AllBodyTypes["twinkler"];  //
                EntityType swarmerType = new EntityType("entity:swarmer")
                {
                    Name = "Swarmer", //
                    ThumbnailSmall = "HUD_thumbnail_twinkler",
                    SummaryDescription = "One of the most dangerous quadite species, the swarmer is extremely fast and aggressive.",
                    Description = "\n FEEDING CLASSIFICATION: Carnivore.\n \n SIZE: 60-100 cm\n \n ANATOMY\n A fast and agile quadite protected by a strong, spiked exoskeleton. Armed with sharp front limbs which have high penetration power. \n \n BEHAVIOR\n This predator lives in underground colonies near the muckroot biome and anyone that enters its territory will soon find themselves attacked by a group of swarmers.\n \n THREAT LEVEL\n Very high.\n \n NOTES\n In its habitat, extreme caution is advised - sentry turrets and sensors should be set up to secure any kind of camp.", //todo
               
                    RenderableType = new RenderableType()
                    {
                        RenderAsModelType = new RenderAsModelType()
                        {

                            //ModelScale = f,// no need, priority goes: race ModelScale / cast ModelScale / default (renderAdModelType ModelScale)//normal size:2.5f, small size:1.5f   //per Dec 2012, this is overwritten by any data put in age, race or caste, in that order
                            AssetName = "twinklerThinSpiky",
                            GaitAnimations = new XmlDictionary<string, GaitAnimationBracket[]>
                       {
                        
                        { "normal", new[]{                                           
                                              new GaitAnimationBracket(){ MinimumSpeed = 65f, MaximumSpeed = 150f, StrideLength = 21f, StrideDuration = 0.9f, AnimationKey = "gaitWalk"},  //
                                            
                                              } }

                       },
                            #region AnimConditions
                            AnimConditions = new AnimConditionInfo[] 
                        {

                            new AnimConditionInfo()
                            {
                              
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Moving},
                                GaitSetKey = "normal"
                            },
                                           
                            new AnimConditionInfo() 
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "idle" }},
                           //     Sound = "aliens/croaker",   error...gets played a lot when they are dead....18th oct 2013
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle }           
                            },
                             new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "attackLowRight" }}, 
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Attacking,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Low, (int)AnimModifier.Right)},
                                //Forbiddens = new BitMask64(typeof(AnimState), (int)AnimState.Moving)
  
                            },
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "attackHighRight" }}, 
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Attacking,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.High, (int)AnimModifier.Right)},
                                //Forbiddens = new BitMask64(typeof(AnimState), (int)AnimState.Moving)  
                            },
                             new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "hit" }, Sounds = new string[]{ "aliens/hummingClickClacking" }},                              
                                Playback = Playback.Manual, 
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Recoiling }
                            },

                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{ "dying" }},
                           //     Sound = "aliens/toothCrickets",  bug -never stops ..21 oct
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying }
                            },

                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "dead" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Post)}  
                            },
                             
                        }
                            #endregion
                        }
                    },
                    LocomotorType = new LocomotorType()
                    {

                        MaxAngularSpeed = .50f * MathHelper.Pi, // .25f * MathHelper.Pi,
                        FourSidedSymmetry = true,
                        MeleeRadius = 12f,

                        LeggedLocomotorType = new LeggedLocomotorType()
                        {
                            TerrainNegateFactor = 0.3f,
                            WalkSlowSpeed = 50f,  
                            WalkNormalSpeed = 70f ,                            
                            WalkFastSpeed = 100f, // 
                        },
                        CollisionResponderType = new CollisionResponderType()
                        {
                            AgentCollisionResponderType = new AgentCollisionResponderType()
                        }
                    },
                    SensorType = new SensorType()
                    {
                        
                        Range = 1000, // - is overriden by the biologicaltype
                        RangeAtNight = 1000, // 25//102 - is overriden by the biologicaltype
                        DetectionTypeKey = "defaultDetection"
                    },

                    BodyType = bodyType,
                    ContainerType = new AgentStorageType() //0.5f)
                    {
                        ItemStorageType = new ItemStorageType(0.5f), // can Haul... later.
                        StomachStorageType = new ItemStorageType(0.07f) // stomach size is dynamic - is defined in BioType

                    }
                };


                swarmerType.IntelligenceType = new IntelligenceType()
                {
                    MembersScoutingFraction = 1f, //dec 3, was 1f
                    IsMobile = true,
                    CanAttack = true,
                    CanUseWeapons = false,
                    CanHunt = true,
                    CanScout = true,
                    CanExamine = true, 
                    CanPatrol = true,
                    CanHaul = false,
                    IsPredator = true,
                    WillAttackNonThreatsNearby = true, // was true, testing
                    ContainerTransactTag = "twinklerTransact", //was "twinkler"
                    InterestInTriggerTypes = new[] { "mineTrigger", "spikeTrapTrigger", "smallImprovisedTrapTrigger" },
                    StrengthRating = Entities.StrengthRating.LikeHumans, // was WeakerThanHumans
                    Boldness = 0.25f, //Boldness is low at the moment to allow baby and child twinklersd to flee, the bigger ones should still be able to attack threats //1f
                    Courage = 0.05f,   //0.1f 0.2f   0.9f  // 0.05f . wanted them to flee more. 0.05f didn't make them flee either...tweak also vitalBodyPartDamageEvaluationBoost in constants.cs
                    MemoryInDays = 2f,

                    Prey = new string[] { "entity:mudWorm", "entity:binalRat", "entity:whiteThunderChicken", "entity:pygmyThunderChicken", "entity:bajingan", "entity:human" },
                
                    Attacks = new[]
                     { 
                         "twinklerLowRight",
                         "twinklerHighRight"
                         //new AttackType(){ Damage = AttackType.DamageTypes.Piercing, DamageMean=3, DamageStandardDeviation=1.5f, AnimationKey = "attackLowRight"/*, DependsOn = new BodyPartType[]{ bodyType.FindBodyPart("Right arm") }*/ },
                         //new AttackType(){ Damage = AttackType.DamageTypes.Piercing, DamageMean=4, DamageStandardDeviation=1.5f, AnimationKey = "attackHighRight"/*, DependsOn = new BodyPartType[]{ bodyType.FindBodyPart("Right arm") }*/ }
                         
                     },
                    Skills = new SerializableDictionary<string, float> 
                    { 
                        {
                           "unarmedFighting" , 0.2f
                        }
                    },
                    AggroRange = sensorRangeHuman + 100, // red - is overriden by the biologicaltype
                    AssistanceRange = 600f // NEW!!! // 600f did not change anything // green - is overriden by the biologicaltype
                };



                swarmerType.BiologicalType = new BiologicalType()
                {
                    OrderKey = "quaditeOrder",
                    OxygenAndMuscleEnergyIncreaseRatePerDay = 12f,
                    FoodItemTagsThatCanBeConsumed = new[] { "cookedMeat", "rawMeat", "smallRawMeat", "spoiledMeal", "inedibleMeat", "rottenMeat" },
                    ExtractionProcessTypes = new[] { "extractMudWormMeat", "extractPatricianMeat", "extractThunderChickenMeat", "extractBinalRatMeat", "extractTwinklerMeat", "extractBushDragonMeat", "extractSwampDemonTreeMeat", "extractDemonTreeMeat", "extractMegapodMeat", "extractWhipjawMeat", "extractSpikePlantMeat", "extractForestGuardianMeat", }, //"extractThinThunderChickenMeat",
                    TimeToConsumeFullMealInDays = 0.005f, //was 0.009f dec 2014.   was 0.015f MP
                    StomachSizeFractionOfEntityBulk = 0.3f, // big stomach so can eat quickly // 0.2f,
                    StomachContentsDecreaseRatePerDay = 3,
                    ActiveStealthRating = 0.2f,

                    MaxRegainLimit = humanMaxRegainLimit,
                    FractionOfMaxHitpointsGainedPerDay = humanFractionOfMaxHitpointsGainedPerDay,
                    Carcass = "item:swarmerCarcass",

                    ResilienceMean = 7f,
                    ResilienceStandardDeviation = 0.08f,

                    BioPropertyTypes = new[]{                       
                         new BioPropertyType(){
                          KeyName = "SensorRange", //measured in pixels  (White)    //aggrorange and assistancerange can be seen in debug panel>ranges, however if very big, will not be seen. see: public void DrawRanges() for colors.
                          InterpolateSetting = BioPropertyType.Interpolate.DefaultPriority                     
                     },
                      new BioPropertyType(){
                          KeyName = "SensorRangeAtNight", //measured in pixels
                          InterpolateSetting = BioPropertyType.Interpolate.DefaultPriority                     
                     },
                     new BioPropertyType(){ 
                          KeyName = "AggroRange", //measured in pixels  (red)
                          InterpolateSetting = BioPropertyType.Interpolate.DefaultPriority                     
                     },
                     new BioPropertyType(){
                          KeyName = "AssistanceRange", //measured in pixels  (green)
                          InterpolateSetting = BioPropertyType.Interpolate.DefaultPriority                     
                     }},
           
                    Castes = new List<CasteType>() 
                { 
                    // CasteType changed to guard(taken from Twinkler) from hunter. the idea is that the swarmer will be used to guard the
                    // rare metal in Scenario 7. for this it needs a lower range and the defence mind of the guard fits perfectly.
                    new CasteType() {  
                        KeyName = "guard",
                        Reproduction = Reproduction.None, //Edge = 1f,
                        HeightMean = .70f, HeightStandardDeviation = 0.02f, 
                        WeightMean = 25f, WeightStandardDeviation = 2f,
                        PrimaryColor = "564920".ToColorVector3(), 
                        ModelBasicTextureName = "QuaditeThinSpikyTexture", 
                        Edge = 0.6f, 
                        ModelScale=2.1f,
                                        
                        BioProperties = new SerializableDictionary<string,BioProperty>{ 
                        { "SensorRange", new BioProperty(){ NumberValue = 576f} },  
                        { "SensorRangeAtNight", new BioProperty(){ NumberValue = 576f} }, 
                        { "AggroRange", new BioProperty(){ NumberValue = 210f} }, 
                        { "AssistanceRange", new BioProperty(){ NumberValue = 300f} }  },  //Guard quadite
                                    
                       

                        AgeGroupTypes = new List<AgeGroupType>() 
                        { 
                            new AgeGroupType() 
                            { 
                                Name = "Hatchling", AIAgeGroup = AIAgeGroup.Baby, CanReproduce = false, Edge = 1.5f, HeightTargetModifier = 0.05f, WeightTargetModifier = 0.03f/*, 
                                NeedTypes = new NeedType[]{ babySleepNeed }*/
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Hatchling", AIAgeGroup = AIAgeGroup.Child, CanReproduce = false, Edge = 11f, HeightTargetModifier = 0.6f, WeightTargetModifier = 0.5f  
                             // , NeedTypes = new NeedType[]{ childSleepNeed }
                            },
                            new AgeGroupType() 
                            { 
                                Name = "Grown", AIAgeGroup = AIAgeGroup.YoungAdult, CanReproduce = false, Edge = 16f, HeightTargetModifier = 0.97f, WeightTargetModifier = 0.92f,
                                NeedTypes = new []{ twinklerProteinNeed }
                              
                             //  , NeedTypes = new NeedType[]{ youngAdultSleepNeed }
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Grown", AIAgeGroup = AIAgeGroup.Adult, CanReproduce = true, Edge = 72f, HeightTargetModifier = 1f, WeightTargetModifier = 1f,
                                NeedTypes = new []{ twinklerProteinNeed }
                             // , NeedTypes = new NeedType[]{ adultSleepNeed }
                            }
                            ,
                            new AgeGroupType() 
                            { 
                                Name = "Grown", AIAgeGroup = AIAgeGroup.Old, CanReproduce = false, Edge = 200f, HeightTargetModifier = 0.9f, WeightTargetModifier = 1f,
                                NeedTypes = new []{ twinklerProteinNeed }
                             // , NeedTypes = new NeedType[]{ oldSleepNeed }
                            }
                        } 
                    } 
                }

                };

                listOfEntityTypes.Add(swarmerType);

                #endregion


                #region Field quadite (leafcutter) needs

                NeedType quaditeFoodNeed = new NeedType()
                {
                    KeyName = "foodEnergy",
                    FoodNeedType = new FoodNeedType()
                    {
                        FoodNutrient = "foodEnergy",
                        RequiredNutrientsAsFractionOfEntityBulk = 0.1f
                    },
                    DecreasePerDay = new NormalDistribution() { Mean = 1f, StandardDeviation = 0.02f },     
                    LimitForDecreasedEnergy = 0.10f,
                    DecreasedEnergyWeight = 0.6f,
                    PhysicalEffects = new PhysicalEffects()
                    {
                        DaysAtZeroCausingCollapse = 4f, // never die??
                        DaysAtZeroCausingDeath = 4f,
                        DaysAtZeroDecreaseFactor = 1f, //0.5f,
                        UseExertionFactorToDecrease = true                        
                    },
                };

                #endregion

                #region Field quadite (leafcutter)
                bodyType = GameData.Instance.AllBodyTypes["twinkler"];  
                EntityType QuaditeType = new EntityType("entity:fieldQuadite")
                //mp why is this animal reusing QuaditeType instead of having its own?
                {
                    Name = "Field quadite", //"Rustic"
                    ThumbnailSmall = "HUD_thumbnail_twinklerWhiteStar",
                    SummaryDescription = "Ravenous plant eater. Vermin.",
                    Description = "\n FEEDING CLASSIFICATION: Herbivore.\n \n SIZE: 10-40 cm\n \n ANATOMY\n A smaller quadite species specialized in collecting plant food for its colony. Equipped with sharp scissor-like appendages that are able to cut through tough plant material such as spoak leaves.\n \n BEHAVIOR\n Lives in underground colonies built underneath the firegrass plains. The tunneling is done by a builder caste which is rarely seen. The worker caste, however, is easy to notice as it scours the landscape in a constant search for nutrient-rich plant parts.\n \n THREAT LEVEL\n None. Will rather flee than defend itself if threatened.\n \n NOTES\n In its habitat, vegetable food stores have to be protected thoroughly. The field quadite is able to gain entry to any building made from spoak leaves or other soft material.", 
                  
                    RenderableType = new RenderableType()
                    {
                        RenderAsModelType = new RenderAsModelType()
                        {
                            ModelScale = 1.25f, // priority goes: race ModelScale / cast ModelScale / default (renderAdModelType ModelScale)
                            AssetName = "twinklerThin", 
                            GaitAnimations = new XmlDictionary<string, GaitAnimationBracket[]>
                            {
                                { "normal", new[]{new GaitAnimationBracket(){ MinimumSpeed = 35f, MaximumSpeed = 77f, StrideLength = 14f, StrideDuration = 0.9f, AnimationKey = "gaitWalk"},} } //MP: was: StrideLength = 27f, I set it low, to 14f so they skittle instead of skate.
                                
                            },
                            #region AnimConditions
                            AnimConditions = new AnimConditionInfo[]
                            {
                                new AnimConditionInfo()
                                {
                                    ConditionSet = new AnimConditions(){ Action = AnimAction.Moving},
                                    GaitSetKey = "normal"
                                },
                                new AnimConditionInfo() 
                                {
                                    SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "idle" }},
                               //     Sound = "aliens/croaker",   error...gets played a lot when they are dead....18th oct 2013
                                    ConditionSet = new AnimConditions(){ Action = AnimAction.Idle }           
                                },
                                new AnimConditionInfo()
                                {
                                    SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "attackLowRight" }}, 
                                    ConditionSet = new AnimConditions()
                                    {
                                        Action = AnimAction.Attacking,
                                        Modifiers = new BitMask64(typeof(AnimModifier),(int)AnimModifier.Low, (int)AnimModifier.Right)
                                    },
                                    //Forbiddens = new BitMask64(typeof(AnimState), (int)AnimState.Moving)
                                },
                                new AnimConditionInfo()
                                {
                                    SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "attackHighRight" }}, 
                                    ConditionSet = new AnimConditions()
                                    {
                                        Action = AnimAction.Attacking,
                                        Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.High, (int)AnimModifier.Right)
                                    },
                                    //Forbiddens = new BitMask64(typeof(AnimState), (int)AnimState.Moving)  
                                },
                                 new AnimConditionInfo()
                                {
                                    SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "hit" }, Sounds = new string[]{ "aliens/hummingClickClacking" } },                                  
                                    Playback = Playback.Manual, 
                                    ConditionSet = new AnimConditions(){ Action = AnimAction.Recoiling }
                                },

                                new AnimConditionInfo()
                                {
                                    SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{ "dying" }},
                               //     Sound = "aliens/toothCrickets",  bug -never stops ..21 oct
                                    ConditionSet = new AnimConditions(){ Action = AnimAction.Dying }
                                },

                                new AnimConditionInfo()
                                {
                                    SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "dead" }},
                                    ConditionSet = new AnimConditions()
                                    {
                                        Action = AnimAction.Dying, Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Post)
                                    }
                                },
                            }
                            #endregion
                        }
                    },
                    LocomotorType = new LocomotorType()
                    {
                        MaxAngularSpeed = .25f * MathHelper.Pi,
                        FourSidedSymmetry = true,
                        MeleeRadius = 12f,
                        LeggedLocomotorType = new LeggedLocomotorType()
                        {
                            TerrainNegateFactor = 0.3f,//
                            WalkSlowSpeed = 25f, // 
                            WalkNormalSpeed = 35f,//
                            WalkFastSpeed = 50f, // 
                        },
                        CollisionResponderType = new CollisionResponderType()
                        {
                            AgentCollisionResponderType = new AgentCollisionResponderType()
                        }
                    },
                    SensorType = new SensorType()
                    {
                        Range = sensorRangeHuman + 10,
                        RangeAtNight = sensorRangeHuman + 10,
                        DetectionTypeKey = "defaultDetection"
                    },
                    BodyType = bodyType,
                    ContainerType = new AgentStorageType()
                    {
                        ItemStorageType = new ItemStorageType(0.5f),
                        StomachStorageType = new ItemStorageType(0.07f) // stomach size is dynamic - is defined in BioType
                        
                    },
                };

                QuaditeType.IntelligenceType = new IntelligenceType()
                {
                    MembersScoutingFraction = 1f,
                    IsMobile = true,
                    CanAttack = false,
                    CanUseWeapons = false,
                    CanHunt = false,
                    CanScout = true,
                    CanExamine = true, 
                    CanPatrol = true,
                    CanHaul = false,
                    IsPredator = false,
                    ContainerTransactTag = "leafcutterTransact", //was "scavengerQuadite"
                    InterestInTriggerTypes = new[] { "mineTrigger", "spikeTrapTrigger", "smallImprovisedTrapTrigger" },
                    StrengthRating = Entities.StrengthRating.None,
                    Courage = 0f,
                    MemoryInDays = 3f,
                    Boldness = 1.49f,
                    AggroRange = 0.0f,
                    AssistanceRange = 600f,
                   
                };

                QuaditeType.BiologicalType = new BiologicalType()
                {
                    OrderKey = "quaditeOrder",
                    OxygenAndMuscleEnergyIncreaseRatePerDay = 12f,
                    FoodItemTagsThatCanBeConsumed = new[] { "edibleVegi", "inedibleVegi" },
                    TimeToConsumeFullMealInDays = 0.005f,
                    StomachSizeFractionOfEntityBulk = 0.45f,
                    StomachContentsDecreaseRatePerDay = 3,
                    ActiveStealthRating = 0.2f,

                    MaxRegainLimit = humanMaxRegainLimit,
                    FractionOfMaxHitpointsGainedPerDay = humanFractionOfMaxHitpointsGainedPerDay,
                    Carcass = "item:leafcutterCarcass", //placeholder

                    ResilienceMean = 7f,
                    ResilienceStandardDeviation = 0.08f,
                    IsVermin = true,

                    BioPropertyTypes = new[]
                    {
                         new BioPropertyType()
                         {
                          KeyName = "SensorRange", //measured in pixels  (White)    //aggrorange and assistancerange can be seen in debug panel>ranges, however if very big, will not be seen. see: public void DrawRanges() for colors.
                          InterpolateSetting = BioPropertyType.Interpolate.DefaultPriority
                         },
                        new BioPropertyType()
                        {
                          KeyName = "SensorRangeAtNight", //measured in pixels
                          InterpolateSetting = BioPropertyType.Interpolate.DefaultPriority
                        },
                        new BioPropertyType()
                        { 
                             KeyName = "AggroRange", //measured in pixels  (red)
                             InterpolateSetting = BioPropertyType.Interpolate.DefaultPriority
                        },
                        new BioPropertyType()
                        {
                             KeyName = "AssistanceRange", //measured in pixels  (green)
                             InterpolateSetting = BioPropertyType.Interpolate.DefaultPriority                     
                        },
                        new BioPropertyType()
                        {
                             KeyName = "IsVermin", 
                             InterpolateSetting = BioPropertyType.Interpolate.DefaultPriority                     
                        }
                    },
                    Castes = new List<CasteType>()
                    {
                        new CasteType()
                        {
                            KeyName = "Scavenger", //MP this caste is obviously not needed if we only have one. so either remove it or make a second "builder" caste (do we have a mesh for that?)
                            PrimaryColor = "F7F4C5".ToColorVector3(),
                            ModelBasicTextureName = "QuaditeThinTexture",
                            Edge = 0.3f,
                            ModelName = "twinklerThin",
                            BioProperties = new SerializableDictionary<string,BioProperty>
                            { 
                                { "SensorRange", new BioProperty(){ NumberValue = sensorRangeHuman + 10} },  
                                { "SensorRangeAtNight", new BioProperty(){ NumberValue = sensorRangeHuman + 10} }, 
                                { "AggroRange", new BioProperty(){ NumberValue = 0f} },
                                { "AssistanceRange", new BioProperty(){ NumberValue = 1200f} },
                                { "IsVermin", new BioProperty(){ BoolValue = true} }
                            },
                            Reproduction = Reproduction.None, //Edge = 0.9f,
                            HeightMean = 0.6f, HeightStandardDeviation = 0.02f, 
                            WeightMean = 20f, WeightStandardDeviation = 3f,
                            AgeGroupTypes = new List<AgeGroupType>()   // http://www.enchantedlearning.com/subjects/animals/Animalbabies.shtml
                            { 
                                new AgeGroupType() 
                                { 
                                    Name = "Hatchling", AIAgeGroup = AIAgeGroup.Baby, CanReproduce = false, Edge = 0.4f, HeightTargetModifier = 0.05f, WeightTargetModifier = 0.03f,  // "Baby"
                                    ModelScaleFraction = 0.2f
                                    //NeedTypes = new NeedType[]{ babySleepNeed }
                                } ,
                                new AgeGroupType() 
                                { 
                                    Name = "Hatchling", AIAgeGroup = AIAgeGroup.Child, CanReproduce = false, Edge = 0.7f, HeightTargetModifier = 0.6f, WeightTargetModifier = 0.5f,  // "Child"
                                    ModelScaleFraction = 0.6f,
                                    //, NeedTypes = new NeedType[]{ childSleepNeed }
                                },
                                new AgeGroupType() 
                                { 
                                    Name = "Grown", AIAgeGroup = AIAgeGroup.YoungAdult, CanReproduce = false, Edge = 0.9f, HeightTargetModifier = 0.97f, WeightTargetModifier = 0.92f,  // "Young adult"
                                    ModelScaleFraction = 0.9f, 
                                    NeedTypes = new []{ quaditeFoodNeed } 
                                    //  , NeedTypes = new NeedType[]{ youngAdultSleepNeed }
                                } ,
                                new AgeGroupType() 
                                { 
                                    Name = "Grown", AIAgeGroup = AIAgeGroup.Adult, CanReproduce = true, Edge = 3.8f, HeightTargetModifier = 1f, WeightTargetModifier = 1f,  // "Adult"
                                   ModelScaleFraction = 1.0f, 
                                   NeedTypes = new []{ quaditeFoodNeed } 
                                    //, NeedTypes = new NeedType[]{ adultSleepNeed }
                                }
                                ,
                                new AgeGroupType() 
                                { 
                                    Name = "Grown", AIAgeGroup = AIAgeGroup.Old, CanReproduce = true, Edge = 4f, HeightTargetModifier = 0.9f, WeightTargetModifier = 1f,  //"Old"
                                    ModelScaleFraction = 0.95f,
                                    NeedTypes = new []{ quaditeFoodNeed } 
                                  // , NeedTypes = new NeedType[]{ oldSleepNeed }
                                }
                            }
                        },
                    }
                };
                listOfEntityTypes.Add(QuaditeType);
                #endregion

                #region Binal rat needs

                NeedType binalRatFoodNeed = new NeedType()
                {
                    KeyName = "foodEnergy",
                    FoodNeedType = new FoodNeedType() { FoodNutrient = "foodEnergy", RequiredNutrientsAsFractionOfEntityBulk = 0.1f  },
                    DecreasePerDay = new NormalDistribution() { Mean = 2f, StandardDeviation = 0.02f }, //MP...july 24.. was 0.6f   ..buffed it to make rats hungrier quicker (because they spawn with full nutrition, 1f, which I can't seem to change anywhere?)
                 
                    LimitForDecreasedEnergy = 0.10f,
                    DecreasedEnergyWeight = 0.6f,
                    PhysicalEffects = new PhysicalEffects()
                    {
                        DaysAtZeroCausingCollapse = 4f, // never die??
                        DaysAtZeroCausingDeath = 4f,
                        DaysAtZeroDecreaseFactor = 1f, //0.5f,
                        UseExertionFactorToDecrease = true                         
                    }
                };

                #endregion

                #region binalrattype
                EntityType binalratType = new EntityType("entity:binalRat")
                {
                    Name = "Binal rat",
                    ThumbnailSmall = "HUD_thumbnail_binalRat",
                    SummaryDescription = "Small, two-legged omnivore/scavenger. Vermin.",  // 
                    Description = "\n FEEDING CLASSIFICATION: Its flexible diet means it can survive almost anywhere.\n \n HEIGHT: 10 to 25 cm\n \n ANATOMY\n This order shares some characteristics with the thunder chickens. A common feature is retractable tentacles around the mouth used for feeding. They also have in common an antenna on top of the head for detecting dangers.\n \n BEHAVIOR\n Pervasive, very adaptive order of animals.\n \n ENEMIES\n Preyed on by quadites.\n \n SURVIVAL GUIDE NOTES\n They are not very fearful of humans and will be attracted by any food or waste lying exposed. Should be controlled as a pest and not hunted as a source of food (their habit of eating plants that are poisonous to humans makes their flesh inedible to us.)",
                   
                    RenderableType = new RenderableType()
                    {
                        RenderAsModelType = new RenderAsModelType()
                        {

                            ModelScale = 0.5f, // priority goes: race ModelScale / cast ModelScale / default (renderAdModelType ModelScale)
                            AssetName = "thunderchicken",
                            ModelBasicTextureName = "BinalRatBrownTexture",
                            GaitAnimations = new XmlDictionary<string, GaitAnimationBracket[]>
                       {
                        
                        { "normal", new[]{                                           
                                              new GaitAnimationBracket(){ MinimumSpeed = 0f, MaximumSpeed = 77f, StrideLength = 11f /* measured?: 18.1f*/, StrideDuration = 0.4f, AnimationKey = "gaitWalk"},       //MP: iterated strideduration 17th dec 2013.   ... was: MinimumSpeed = 0f, MaximumSpeed = 77f, StrideLength = 11f , StrideDuration = 0.2f, AnimationKey = "gaitWalk"                                     
                                              } }
                       },

                            AnimConditions = new AnimConditionInfo[] 
                        {
                            new AnimConditionInfo()
                            {
                               SoundAndAnimationSet = new RandomSoundAndAnimationSet()
                               {
                                    AdditionalAnimations1 = new string[]{ "hidden" }
                                },
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Moving },
                                GaitSetKey = "normal"
                            },
 
               /*           new AnimConditionInfo()                 //commented out by MP when putting in new gait data. 6 sep 2013
                            {
                                AnimationSet = new RandomAnimationSet(){ 
                                    BaseAnimations = new string[]{  "walk"},
                                    AdditionalAnimations1 = new string[]{ "hidden" }},

                                Conditions = new ConditionSet(){ Modifiers = new BitMask64(typeof(AnimState), (int)AnimState.Moving )                               
                            },*/
                                           
                            new AnimConditionInfo() 
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ 
                                    BaseAnimations = new string[]{  "idle" },
                                    AdditionalAnimations1 = new string[]{ "look" },
                                    AdditionalAnimations2 = new string[]{ "eat" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle }
                            },
                            new AnimConditionInfo() 
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ 
                                    BaseAnimations = new string[]{  "idle" },
                                    AdditionalAnimations2 = new string[]{ "eat" }
                                },
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Eating }
                            },
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{ "dying" },
                                 Sounds = new string[]{ "aliens/binalRatDie" }},
                                //Sound = "aliens/binalRatDie", 
                                Looping = Looping.No,   //MS: I put this in so the sound doesn't trigger twice.
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying }
                            },

                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "dead" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Post)}  
                            },
                        }
                        }
                    },
                    LocomotorType = new LocomotorType()
                    {

                        MaxAngularSpeed = 2.5f * MathHelper.Pi, // .6f * MathHelper.Pi                     
                        MeleeRadius = 10f,

                        LeggedLocomotorType = new LeggedLocomotorType()
                        {
                            TerrainNegateFactor = 0.01f, //0.7
                            WalkSlowSpeed = 8f, // 20f, 
                            WalkNormalSpeed = 50f, //MP increased it now that they are bolder they need to be able to outrun twinklers to survive better. was: 33f
                            WalkFastSpeed = 74f, //64


                        },
                        CollisionResponderType = new CollisionResponderType()
                        {
                            AgentCollisionResponderType = new AgentCollisionResponderType()
                        }
                    },
                    SensorType = new SensorType()
                    {
                        Range = sensorRangeHuman + 10, //MP was:sensorRangeHuman - 20
                        RangeAtNight = sensorRangeHuman + 10,  //MP was:sensorRangeHuman - 20
                        DetectionTypeKey = "defaultDetection"
                    },

                    BodyType = GameData.Instance.AllBodyTypes["thunderchicken"],

                    ContainerType = new AgentStorageType() //0.5f)
                    {
                        ItemStorageType = new ItemStorageType(0.5f), // can Haul... later.     MP: if you remove this line you get an error june 24 2014
                        StomachStorageType = new ItemStorageType(0.07f) // stomach size is dynamic - is defined in BioType                        
                    }
                };


                binalratType.IntelligenceType = new IntelligenceType()
                {
                    IsMobile = true,
                    CanAttack = false,
                    CanUseWeapons = false,
                    CanHunt = false,
                    CanScout = true,
                    CanExamine = true, 
                    CanPatrol = false,
                    CanHaul = false,
                    IsPredator = false,
                    StrengthRating = Entities.StrengthRating.None,
                    InterestInTriggerTypes = new[] { "smallImprovisedTrapTrigger", "spikeTrapTrigger" },//cannot set off a mine
                    ContainerTransactTag = "ratTransact", //was "rat"
                    Courage = 0f,
                    MemoryInDays = 3f,
                    Boldness = 1.49f, // 1.4f, //1f, // (1f-1.4f has one (big) size of threatmap. 1.49f gives a small threat radius. 1.5f and above removes all threat areas)     MP: it has no attacks (it is unable to attack), this means that it will always be cautious, hence the threat maps it sees are massive. boldness is a factor that affects the size ofthreat maps. When boldness is set to 1f, it reduces the threat map size. 
                    AggroRange = 0.0f
                };


                binalratType.BiologicalType = new BiologicalType()
                {
                    OrderKey = "binalRatOrder",
                    OxygenAndMuscleEnergyIncreaseRatePerDay = 10f,
                    FoodItemTagsThatCanBeConsumed = new[] { "cookedMeat", "rawMeat", "smallRawMeat", "rottenMeat", "inedibleMeat", "edibleVegi", "inedibleVegi", },
                    ExtractionProcessTypes = new[] { "extractMudWormMeat", "extractPatricianMeat", "extractLeafCutterMeat", "extractThunderChickenMeat", "extractTwinklerMeat", "extractBushDragonMeat", "extractTurnipMeat", "extractSwampDemonTreeMeat", "extractDemonTreeMeat", "extractMegapodMeat", "extractWhipjawMeat", "extractSpikePlantMeat", "extractForestGuardianMeat", }, //"extractThinThunderChickenMeat",
                    TimeToConsumeFullMealInDays = 0.0125f,//bso increased from 0.005 to 0.0125(20sec) so the player have a chance to fent off the rats if seen quickly enough //mp we want them to eat the player's food quicker. jan 2015 was 0.008f ... before that was 0.015f MP
                    StomachSizeFractionOfEntityBulk = 0.2f,
                    StomachContentsDecreaseRatePerDay = 3,
                    ActiveStealthRating = 0.1f,                 
                    MaxRegainLimit = humanMaxRegainLimit,
                    FractionOfMaxHitpointsGainedPerDay = humanFractionOfMaxHitpointsGainedPerDay,
                    Carcass = "item:binalRatCarcass",
                    IsVermin = true,
                    Castes = new List<CasteType>() 
                { 
                    ///////// MALE
                    new CasteType() {
                        KeyName = "male",
                        Reproduction = Reproduction.Male, Edge = 0.51f,  
                        HeightMean = 0.25f, HeightStandardDeviation = 0.02f, 
                        WeightMean = 15f, WeightStandardDeviation = 1f,  // multiply WeightStandardDeviation by 3 and subtract that number from WeightMean, this gives the minimum Weight in kilos. divide by 100 to get the bulk...the minimum possible bulk.  

                        AgeGroupTypes = new List<AgeGroupType>() 
                        { 
                            new AgeGroupType() 
                            { 
                                Name = "Chick", AIAgeGroup = AIAgeGroup.Baby, CanReproduce = false, Edge = 0.3f, HeightTargetModifier = 0.05f, WeightTargetModifier = 0.03f, ModelScaleFraction = 0.5f,   // these modifiers are not used as of now, we think . MP
                                NeedTypes = new []{ binalRatFoodNeed }
                                /*,  NeedTypes = new NeedType[]{ babySleepNeed }*/
 
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Chick", AIAgeGroup = AIAgeGroup.Child, CanReproduce = false, Edge = 0.6f, HeightTargetModifier = 0.6f, WeightTargetModifier = 0.5f, ModelScaleFraction = 0.75f,  
                                NeedTypes = new []{ binalRatFoodNeed }
                                //, NeedTypes = new NeedType[]{ childSleepNeed }
                            },
                            new AgeGroupType() 
                            { 
                                Name = "Grown", AIAgeGroup = AIAgeGroup.YoungAdult, CanReproduce = false, Edge = 1f, HeightTargetModifier = 0.97f, WeightTargetModifier = 0.92f, ModelScaleFraction = 0.9f,   
                                NeedTypes = new []{ binalRatFoodNeed }
                                //  , NeedTypes = new NeedType[]{ youngAdultSleepNeed }
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Grown", AIAgeGroup = AIAgeGroup.Adult, CanReproduce = true, Edge = 5f, HeightTargetModifier = 1f, WeightTargetModifier = 1f, ModelScaleFraction = 1f,    
                                NeedTypes = new []{ binalRatFoodNeed }
                                //   , NeedTypes = new NeedType[]{ adultSleepNeed }
                            }
                            ,
                            new AgeGroupType() 
                            { 
                                Name = "Grown", AIAgeGroup = AIAgeGroup.Old, CanReproduce = true, Edge = 6f, HeightTargetModifier = 0.9f, WeightTargetModifier = 1f, ModelScaleFraction = 1f,   
                                NeedTypes = new []{ binalRatFoodNeed }
                                //  , NeedTypes = new NeedType[]{ oldSleepNeed }
                            }
                        } 
                    },
                   /////////FEMALE
                    new CasteType() { 
                        KeyName = "female",
                        Reproduction = Reproduction.Female, Edge = 1f, 
                        HeightMean = 0.25f, HeightStandardDeviation = 0.02f, 
                        WeightMean = 14f, WeightStandardDeviation = 1f,  // multiply WeightStandardDeviation by 3 and subtract that number from WeightMean, this gives the minimum Weight in kilos. divide by 100 to get the bulk...the minimum possible bulk.  

                        AgeGroupTypes = new List<AgeGroupType>() 
                        { 
                            new AgeGroupType() 
                            { 
                                Name = "Chick", AIAgeGroup = AIAgeGroup.Baby, CanReproduce = false, Edge = 0.3f, HeightTargetModifier = 0.05f, WeightTargetModifier = 0.03f, ModelScaleFraction = 0.5f, // these modifiers are not used as of now, we think . MP
                                NeedTypes = new []{ binalRatFoodNeed }
                                /*,  NeedTypes = new NeedType[]{ babySleepNeed }*/
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Chick", AIAgeGroup = AIAgeGroup.Child, CanReproduce = false, Edge = 0.6f, HeightTargetModifier = 0.6f, WeightTargetModifier = 0.5f, ModelScaleFraction = 0.75f, 
                                NeedTypes = new []{ binalRatFoodNeed }
                                //  , NeedTypes = new NeedType[]{ childSleepNeed }
                            },
                            new AgeGroupType() 
                            { 
                                Name = "Grown", AIAgeGroup = AIAgeGroup.YoungAdult, CanReproduce = false, Edge = 1f, HeightTargetModifier = 0.97f, WeightTargetModifier = 0.92f, ModelScaleFraction = 0.9f,
                                NeedTypes = new []{ binalRatFoodNeed }
                                //     , NeedTypes = new NeedType[]{ youngAdultSleepNeed }
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Grown", AIAgeGroup = AIAgeGroup.Adult, CanReproduce = true, Edge = 5f, HeightTargetModifier = 1f, WeightTargetModifier = 1f, ModelScaleFraction = 1f, 
                                NeedTypes = new []{ binalRatFoodNeed }
                                //    , NeedTypes = new NeedType[]{ adultSleepNeed }
                            }
                            ,
                            new AgeGroupType() 
                            { 
                                Name = "Grown", AIAgeGroup = AIAgeGroup.Old, CanReproduce = false, Edge = 6f, HeightTargetModifier = 0.9f, WeightTargetModifier = 1f, ModelScaleFraction = 1f, 
                                NeedTypes = new []{ binalRatFoodNeed }
                                //   , NeedTypes = new NeedType[]{ oldSleepNeed }
                            }
                        } 
                    } 
                }

                };

                listOfEntityTypes.Add(binalratType);

                #endregion

                #region Mud worm needs

                NeedType mudWormFoodNeed = new NeedType()
                {//copied from field quadite.  the worms move slow so cannot get to food as easily and quickly though..
                    KeyName = "foodEnergy",
                    FoodNeedType = new FoodNeedType() { FoodNutrient = "foodEnergy", RequiredNutrientsAsFractionOfEntityBulk = 0.1f },
                    DecreasePerDay = new NormalDistribution() { Mean = 1f, StandardDeviation = 0.02f }, //

                    LimitForDecreasedEnergy = 0.10f,
                    DecreasedEnergyWeight = 0.6f,
                    PhysicalEffects = new PhysicalEffects()
                    {
                        DaysAtZeroCausingCollapse = 4f, // never die??
                        DaysAtZeroCausingDeath = 4f,
                        DaysAtZeroDecreaseFactor = 1f, //
                        UseExertionFactorToDecrease = true
                    }
                };

                #endregion
                #region Mud worm
                EntityType mudWormType = new EntityType("entity:mudWorm")
                {

                    Name = "Mud worm", //
                    ThumbnailSmall = "HUD_thumbnail_worm", //todo
                    SummaryDescription = "Small worm-like scavenger. Not dangerous.",  //
                    Description = "It makes its home in soft, moist soil such as river banks and wetlands. Has a scavenger life style and emerges from its tunnels to feed on whatever decomposing flesh or plant matter it can find.",//todo

                    RenderableType = new RenderableType()
                    {
                        RenderAsModelType = new RenderAsModelType()
                        {

                            ModelScale = 0.4f, // priority goes: race ModelScale / cast ModelScale / default (renderAdModelType ModelScale).. should match with the carcass modelscale
                            AssetName = "worm", //"worm"
                            ModelBasicTextureName = "WormSimpleTexture",
                            GaitAnimations = new XmlDictionary<string, GaitAnimationBracket[]>
                            {
                        
                                { "normal", new[]{                                           
                                              new GaitAnimationBracket(){ MinimumSpeed = 20f, MaximumSpeed = 35f, StrideLength = 22f, StrideDuration = 1.4f, AnimationKey = "walk"},     
                                              //mp mar 2015: this up here is mostly copied from bush dragon.  has no legs so I set stride duration to duration of walk cycle = 1.4 sec. 
                                              // the creature's  speed seems to be controlled further down, in LeggedLocomotorType. I set it low so that it moves slowly.
                                              } }
                            },
                            DefaultInfo = new AnimConditionInfo()
                            {
                                // use this neutral anim when all flags are cleared:
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "idle" } }
                            },
                            DefaultStances = new[] // "filler" anims used instead of idle when a stance has been set, but no action yet (happens between goals). This prevents unwanted switching to standing from kneeling, for instance
                            {
                                new  AnimConditionInfo()
                                {
                                    SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "idle" } }   
                                },
                                new  AnimConditionInfo()
                                {
                                    ConditionSet = new AnimConditions(){ Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Bold )},                              
                                    SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "combatIdle" } }   
                                }
                            },
                            #region AnimConditions
                            AnimConditions = new AnimConditionInfo[] 
                        {
                            new AnimConditionInfo()
                            {                              
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Moving},                            
                                GaitSetKey = "normal"
                            },
                                           
                            new AnimConditionInfo() 
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "idle" }},                            
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle}                              
                            },

                            new AnimConditionInfo() 
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "eat" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Eating }                             
                            },


                             new AnimConditionInfo() 
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "combatIdle" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Bold)}                              
                            },
                              new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "attack" }}, 
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Attacking,}, 
                  //             Looping = Looping.No //mp 2015 why is this not used for animals but for a lot of human anims??  when i used it, the animal died after very short fight? without this, the animal fights for much longer????                       
  
                            },


                           new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ 
                                    BaseAnimations = new string[]{  "hit" },
                                    Sounds = new string[]{ "aliens/alienCombat/wormHitShort" }},//MS: Had to cut this sound very short because the full animation never played
                                Playback = Playback.Manual,                               
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Recoiling }
                            },


                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{ "dead" }}, //no twitching anim made yet, so just using dead anim.
                      //          Sound = "aliens/rattleWooden",  bug - never stops ..21 oct 2013
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying }
                            },

                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "collapse" }, 
                                    Sounds = new string[]{ "aliens/alienCombat/wormDeathShort" } }, //MS: Had to cut this sound very short because the full animation never played
                                
                                Looping = Looping.No,
                                
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Pre)}  
                            },
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{ "dead"  }}, //necessary. put in name of dead anim when made
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Post)}  
                            }

                        }
                            #endregion
                        }
                    },
                    LocomotorType = new LocomotorType()
                    {
                        MaxAngularSpeed = .25f * MathHelper.Pi, // MathHelper.Pi,
                        FourSidedSymmetry = false,
                        MeleeRadius = 32f,

                        LeggedLocomotorType = new LeggedLocomotorType()
                        {
                            TerrainNegateFactor = 0.1f,
                            WalkSlowSpeed = 3f, //  
                            WalkNormalSpeed = 4f, //debug only. *proper speed*
                            WalkFastSpeed = 6f, // 
                        },
                        CollisionResponderType = new CollisionResponderType()
                        {
                            AgentCollisionResponderType = new AgentCollisionResponderType()
                        }
                    },
                    SensorType = new SensorType()
                    {
                        Range = sensorRangeHuman + 20, //they move slowly so need to see food from far away
                        RangeAtNight = sensorRangeHumanNight + 20,
                        DetectionTypeKey = "defaultDetection"
                    },

                    BodyType = bodyType, //copied from field quadite
                    ContainerType = new AgentStorageType() 
                    {
                        ItemStorageType = new ItemStorageType(0.5f), // 
                        StomachStorageType = new ItemStorageType(0.07f) // stomach size is dynamic - is defined in BioType

                    },
                };

                mudWormType.IntelligenceType = new IntelligenceType()
                {//mp copied from field quadite mostly
                    IsMobile = true,
                    CanAttack = false,
                    CanUseWeapons = false,
                    CanHunt = false,
                    CanScout = true,
                    CanExamine = true, 
                    CanPatrol = true,
                    CanHaul = false,
                    IsPredator = false,
                                   
                    ContainerTransactTag = "ratTransact", //can get into same places as rats
                    InterestInTriggerTypes = new[] { "mineTrigger", "spikeTrapTrigger", "smallImprovisedTrapTrigger" },
                    StrengthRating = StrengthRating.None,
                    Courage = 0f,
                    MemoryInDays = 3f,
                    Boldness = 1.49f, //kinda fearless and dumb. should have large numbers and move slowly towards food.
              //    WillAttackNonThreatsNearby = true, 
              //    AggroRange = 0f,
                  


                };

                mudWormType.BiologicalType = new BiologicalType() // remember to replace xxxxxType.! when copy pasting
                {//mostly copied from field quadite
                    OrderKey = "wormOrder", //maybe a different order?
                    OxygenAndMuscleEnergyIncreaseRatePerDay = 12f,
                    TimeToConsumeFullMealInDays = 0.005f,
                    StomachSizeFractionOfEntityBulk = 0.45f,
                    StomachContentsDecreaseRatePerDay = 3,
                    // 
                    FoodItemTagsThatCanBeConsumed = new[] { "edibleVegi", "inedibleVegi","cookedMeat", "rawMeat", "smallRawMeat", "spoiledMeal", "inedibleMeat", "rottenMeat" },
                    //do not let them eat each other else they'd not really move anywhere.. //"extractMudWormMeat"
                    ExtractionProcessTypes = new[] {  "extractPatricianMeat", "extractLeafCutterMeat", "extractThunderChickenMeat", "extractBinalRatMeat", "extractTwinklerMeat", "extractBushDragonMeat", "extractSwampDemonTreeMeat", "extractDemonTreeMeat", "extractWhipjawMeat", "extractSpikePlantMeat", "extractForestGuardianMeat", }, //"extractThinThunderChickenMeat",
                    IsTerritorial = false,
                    MaxRegainLimit = humanMaxRegainLimit,
                    FractionOfMaxHitpointsGainedPerDay = humanFractionOfMaxHitpointsGainedPerDay,
                    ActiveStealthRating = 0.2f, //
                    Carcass = "item:mudWormCarcass", //

                    ResilienceMean = 1f, //resilience is a factor that is multiplied with bulk to give hitpoints.
                    ResilienceStandardDeviation = 0f,
                    IsVermin = true,
                    Castes = new List<CasteType>() 
                { 
                    new CasteType() { KeyName = "Worker", //mostly copied from field quadite
                        Reproduction = Reproduction.Male, Edge = 1f, //only one sex
                        HeightMean = 0.3f, HeightStandardDeviation = 0.08f, 
                        WeightMean = 20f, WeightStandardDeviation = 3f,
                       AgeGroupTypes = new List<AgeGroupType>()   // http://www.enchantedlearning.com/subjects/animals/Animalbabies.shtml
                            { 
                                new AgeGroupType() 
                                { 
                                    Name = "Hatchling", AIAgeGroup = AIAgeGroup.Baby, CanReproduce = false, Edge = 0.5f, HeightTargetModifier = 0.7f, WeightTargetModifier = 0.7f,  // "Baby"
                                    ModelScaleFraction = 0.7f
                                    //NeedTypes = new NeedType[]{ babySleepNeed }
                                } ,
                                new AgeGroupType() 
                                { 
                                    Name = "Hatchling", AIAgeGroup = AIAgeGroup.Child, CanReproduce = false, Edge = 0.8f, HeightTargetModifier = 0.8f, WeightTargetModifier = 0.8f,  // "Child"
                                    ModelScaleFraction = 0.8f,
                                    //, NeedTypes = new NeedType[]{ childSleepNeed }
                                },
                                new AgeGroupType() 
                                { 
                                    Name = "Grown", AIAgeGroup = AIAgeGroup.YoungAdult, CanReproduce = false, Edge = 1f, HeightTargetModifier = 0.9f, WeightTargetModifier = 0.9f,  // "Young adult"
                                    ModelScaleFraction = 0.9f, 
                                    NeedTypes = new []{ quaditeFoodNeed } 
                                    //  , NeedTypes = new NeedType[]{ youngAdultSleepNeed }
                                } ,
                                new AgeGroupType() 
                                { 
                                    Name = "Grown", AIAgeGroup = AIAgeGroup.Adult, CanReproduce = true, Edge = 3.5f, HeightTargetModifier = 1f, WeightTargetModifier = 1f,  // "Adult"
                                   ModelScaleFraction = 1.0f, 
                                   
                                    //, NeedTypes = new NeedType[]{ adultSleepNeed }
                                }   ,
                                new AgeGroupType() 
                                { 
                                    Name = "Grown", AIAgeGroup = AIAgeGroup.Old, CanReproduce = true, Edge = 4f, HeightTargetModifier = 1f, WeightTargetModifier = 1f,  //"Old"
                                    ModelScaleFraction = 1f,                                    
                                  // , NeedTypes = new NeedType[]{ oldSleepNeed }
                                }

                        } 
                    },

                }

                };

                listOfEntityTypes.Add(mudWormType); // remember to replace!! when copy pasting. else you will get an error that says 'duplicate key', for example

                #endregion

                #region BushDragon Food needs
                NeedType bushDragonFoodNeed = new NeedType()
                {
                    KeyName = "foodEnergy",                   
                    FoodNeedType = new FoodNeedType()
                    { 
                        FoodNutrient = "foodEnergy",
                        RequiredNutrientsAsFractionOfEntityBulk = 0.1f
                    },
                    DecreasePerDay = new NormalDistribution() { Mean = 2f, StandardDeviation = 0.02f },    //MP...july 24.. was 0.6f   ..buffed it to make rats hungrier quicker (because they spawn with full nutrition, 1f, which I can't seem to change anywhere?)
                    LimitForDecreasedEnergy = 0.10f,
                    DecreasedEnergyWeight = 0.6f,
                    PhysicalEffects = new PhysicalEffects()
                    {
                        DaysAtZeroCausingCollapse = 4f, // never die??
                        DaysAtZeroCausingDeath = 4f,
                        DaysAtZeroDecreaseFactor = 1f, //0.5f,
                        UseExertionFactorToDecrease = true                       
                    },
                };

                #endregion

                #region bushdragontype
                EntityType bushdragonType = new EntityType("entity:bushDragon")
                {
                    Name = "Northern bush dragon",
                    ThumbnailSmall = "HUD_thumbnail_bushDragon",
                    SummaryDescription = "Omnivorous herd animal",
                    Description = "\n FEEDING CLASSIFICATION: Omnivore. Eats low vegetation, small animals, carrion.\n \n HEIGHT: Up to 1.5 m (wings excluded)\n \n ANATOMY\n Waddling, 3-legged animal which has developed wings, not for flight but for display purposes. Likely used to dissuade predators and possibly in mating behaviour. The creature has a defensive weapon in the form of a chemical spray.\n \n BEHAVIOR\n If approached, bush dragons will defend themselves much in the manner of the terran skunk. Against the quadites the spray seems to be particularly effective, causing incapacitation and even death.\n \n SURVIVAL GUIDE NOTES\n The slow-moving creatures protect their herd, but if distance is observed they do not attack. Their defensive chemical is quite toxic to humans and caution is advised.",
                 
                    DetectionTag = "huge", // it's very easy to see the bushdragon. (altho it has yellow colouring to blend in when idling. hmmmm. TODO.)
                    RenderableType = new RenderableType()
                    {

                        RenderAsModelType = new RenderAsModelType()
                        {
                            //ModelScale = 0.8f, // no need, priority goes: race ModelScale / cast ModelScale / default (renderAdModelType ModelScale)
                            AssetName = "bushdragon",
                            GaitAnimations = new XmlDictionary<string, GaitAnimationBracket[]>
                       {
                        
                        { "normal", new[]{                                           
                                              new GaitAnimationBracket(){ MinimumSpeed = 20f, MaximumSpeed = 35f, StrideLength = 11f /* measured? 21.8f*/, StrideDuration = 1.1f, AnimationKey = "gaitWalk"},        //MP: iterated strideduration 17th dec 2013.   ... was:    MinimumSpeed = 20f, MaximumSpeed = 35f, StrideLength = 11f /* measured? 21.8f*/, StrideDuration = 0.6f, AnimationKey = "gaitWalk"                                 
                                              } }
                       },
                            //new
                            DefaultInfo = new AnimConditionInfo()
                            {
                                // use this neutral anim when all flags are cleared:
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "idle" } }
                            },
                            DefaultStances = new[] // "filler" anims used instead of idle when a stance has been set, but no action yet (happens between goals). This prevents unwanted switching to standing from kneeling, for instance
                        {
                            new  AnimConditionInfo()
                            {
                                 SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "idle" } }   
                            },
                            new  AnimConditionInfo()
                            {
                                 ConditionSet = new AnimConditions(){ Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Bold )},                              
                                 SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "combatIdle" } }   //bush dragon
                            }
                        },
                            //new end



                            AnimConditions = new AnimConditionInfo[] 
                        {
                            new AnimConditionInfo()
                            {
                              
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Moving },
                                GaitSetKey = "normal"
                            },
                                           
                            new AnimConditionInfo() 
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "idle" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle }
                            },
                    /*         new AnimConditionInfo() //mp jan 2015. how come it works when this one is not in use. this anim is the unfolding wings anim. that we see in game.
                            {
                                AnimationSet = new RandomAnimationSet(){ BaseAnimations = new string[]{  "scare" }}, 
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Attacking,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Trouble) }, //??
                                //Forbiddens = new BitMask64(typeof(AnimState), (int)AnimState.Moving)  
                            },
                     */


                             new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "attackNormal" }}, 
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Attacking,
                                Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Near)}
                              
  
                            },

                              new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "hit" }, Sounds = new string[]{ "aliens/bushdragonHit"} },
                                Playback = Playback.Manual,                               
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Recoiling }
                            },
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{ "dying" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying }
                            },

                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "dead" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying, 
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Post)}  
                            }
                        }
                        }
                    },
                    LocomotorType = new LocomotorType()
                    {
                        MaxAngularSpeed = .25f * MathHelper.Pi, // MathHelper.Pi,
                        FourSidedSymmetry = false,
                        MeleeRadius = 16, //16f (pixels)

                        LeggedLocomotorType = new LeggedLocomotorType()
                        {
                            TerrainNegateFactor = 0.6f,
                            WalkSlowSpeed = 8f, // 8f, 
                            WalkNormalSpeed = 9f /*debug only*/, //18f /*proper speed*/, patrician:26f, turnip:11f
                            WalkFastSpeed = 11f, // 18f,//30f, // 63f,
                        },
                        CollisionResponderType = new CollisionResponderType()
                        {
                            AgentCollisionResponderType = new AgentCollisionResponderType()
                        }
                    },
                    SensorType = new SensorType()
                    {
                        Range = sensorRangeHuman,
                        RangeAtNight = sensorRangeHumanNight,
                        DetectionTypeKey = "defaultDetection" // this is how the bush dragon detects. see BaseDataLoader under listOfDetectionTypes for definition
                    },

                    BodyType = GameData.Instance.AllBodyTypes["bushdragon"],
                    
                    ContainerType = new AgentStorageType() //0.5f)
                    {
                        ItemStorageType = new ItemStorageType(0.8f), // can Haul... later.     MP: if you remove this line you get an error june 24 2014
                        StomachStorageType = new ItemStorageType(0.07f) // stomach size is dynamic - is defined in BioType
                        
                    }
                };


                bushdragonType.IntelligenceType = new IntelligenceType()
                {
                    MembersScoutingFraction = 1f,
                    IsMobile = true,
                    CanAttack = true,
                    CanUseWeapons = false,
                    CanHunt = true,
                    CanScout = true,
                    CanExamine = true, 
                    CanPatrol = true,
                    CanHaul = false,

                    StrengthRating = StrengthRating.LikeHumans, //MP StrongerThanHumans  was massively big, ruining gameplay because it blocks agents.
                    InterestInTriggerTypes = new[] { "mineTrigger", "spikeTrapTrigger" },
                    Courage = 0.5f,
                    MemoryInDays = 0.2f,
                    Boldness = 0.25f,//0.5f,
                    AggroRange = 110.0f, // MP: may15 2014, was 75f.     300f  set low, so that they don't chase quadites across the map.

                    //new

                    ChanceToRestAfterMeleeAttack = 0.3,
                    MinRestTimeAfterAttackingInSeconds = 0.5f,
                    MaxRestTimeAfterAttackingInSeconds = 1.2f,
                    //new end

                    Prey = new string[] { "entity:mudWorm", "entity:binalRat", "entity:whiteThunderChicken", "entity:pygmyThunderChicken", "entity:bajingan" },
                
                    Skills = new SerializableDictionary<string, float> 
                    { 
                        {
                           "unarmedFighting" , 0.6f
                        }
                    },
                    Attacks = new[]
                    { 
                        "bushDragonSpray"            
                    }
                };


                bushdragonType.BiologicalType = new BiologicalType()
                {
                    OxygenAndMuscleEnergyIncreaseRatePerDay = 8f,
                    TimeToConsumeFullMealInDays = 0.0125f,
                    FoodItemTagsThatCanBeConsumed = new[] { "inedibleVegi", "edibleVegi", "spoiledMeal" },
                    StomachSizeFractionOfEntityBulk = 0.2f,
                    StomachContentsDecreaseRatePerDay = 3,
                    ActiveStealthRating = 0.1f,
                    OrderKey = "bushDragonOrder",
                    IsTerritorial = true,
                    MaxRegainLimit = humanMaxRegainLimit,
                    FractionOfMaxHitpointsGainedPerDay = humanFractionOfMaxHitpointsGainedPerDay,
                    Carcass = "item:bushDragonCarcass",
                    RaceTypes = new[] // MP: I use a grey race on tutorial island. but it's defined in the creatureloader doc in the tut scenario folder.
                    {            
                        new RaceType()                                  // Beige
                        { Name="Northern Bush Dragon", PortraitSkinType="Pale", PrimaryColor = "F7F4C5".ToColorVector3(), ModelBasicTextureName = "BushdragonPaleTexture", Edge = 0.3f, ModelScale = 0.8f
                        },
                        //new RaceType()                                // Dark
                        //{ Name="Dark Bush Dragon", SkinType="Dark", PrimaryColor = "564920".ToColorVector3(), ModelBasicTextureName = "BushdragonDarkTexture", Edge = 0.6f, ModelScale = 0.8f
                        //},
                        //new RaceType()                                // Small
                        //{ Name="Small Bush Dragon", SkinType = "Dark", PrimaryColor = "3F3411".ToColorVector3(),  ModelBasicTextureName = "BushdragonPaleTexture", Edge = 0.3f, ResilienceMean = 0.9f, ResilienceStandardDeviation = 0.08f, ModelScale=0.5f
                        //}      
                    },
                Castes = new List<CasteType>() 
                { 
                    new CasteType() {
                        KeyName = "male",
                        Reproduction = Reproduction.Male, Edge = 0.51f,
                        HeightMean = 2f, HeightStandardDeviation = 0.08f, 
                        WeightMean = 100f, WeightStandardDeviation = 0.15f,

                        AgeGroupTypes = new List<AgeGroupType>() 
                        { 
                            new AgeGroupType() 
                            { 
                                Name = "Baby", AIAgeGroup = AIAgeGroup.Baby, CanReproduce = false, Edge = 0.5f, HeightTargetModifier = 0.5f, WeightTargetModifier = 0.03f, ModelScaleFraction = 0.3f
                                /*, NeedTypes = new NeedType[]{ babySleepNeed }*/
                                //, NeedTypes = new NeedType[]{ bushDragonFoodNeed } //since they cant eat any naturel resources and are too scared to come close to humans, we turned it off until such a time it is implemented
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Child", AIAgeGroup = AIAgeGroup.Child, CanReproduce = false, Edge = 1f, HeightTargetModifier = 0.8f, WeightTargetModifier = 0.5f, ModelScaleFraction = 0.6f  
                             //   , NeedTypes = new NeedType[]{ childSleepNeed }
                               //, NeedTypes = new NeedType[]{ bushDragonFoodNeed } //since they cant eat any naturel resources and are too scared to come close to humans, we turned it off until such a time it is implemented
                            },
                            new AgeGroupType() 
                            { 
                                Name = "Young adult", AIAgeGroup = AIAgeGroup.YoungAdult, CanReproduce = false, Edge = 4f, HeightTargetModifier = 0.97f, WeightTargetModifier = 0.92f, ModelScaleFraction = 0.85f  
                            //   , NeedTypes = new NeedType[]{ youngAdultSleepNeed }
                               //, NeedTypes = new NeedType[]{ bushDragonFoodNeed } //since they cant eat any naturel resources and are too scared to come close to humans, we turned it off until such a time it is implemented
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Adult", AIAgeGroup = AIAgeGroup.Adult, CanReproduce = true, Edge = 28f, HeightTargetModifier = 1f, WeightTargetModifier = 1f, ModelScaleFraction = 1f  
                            //   , NeedTypes = new NeedType[]{ adultSleepNeed }
                                //,NeedTypes = new NeedType[]{ bushDragonFoodNeed } //since they cant eat any naturel resources and are too scared to come close to humans, we turned it off until such a time it is implemented
                            }
                            ,
                            new AgeGroupType() 
                            { 
                                Name = "Old", AIAgeGroup = AIAgeGroup.Old, CanReproduce = true, Edge = 30f, HeightTargetModifier = 0.9f, WeightTargetModifier = 1f, ModelScaleFraction = 1f  
                            //   , NeedTypes = new NeedType[]{ oldSleepNeed }
                                //,NeedTypes = new NeedType[]{ bushDragonFoodNeed } //since they cant eat any naturel resources and are too scared to come close to humans, we turned it off until such a time it is implemented
                            }
                        } 
                    },
                    new CasteType() { 
                        KeyName = "female",
                        Reproduction = Reproduction.Female, Edge = 1f, //????
                        HeightMean = 1.90f, HeightStandardDeviation = 0.05f, 
                        WeightMean = 90f, WeightStandardDeviation = 0.10f,

                        AgeGroupTypes = new List<AgeGroupType>() 
                        { 
                            new AgeGroupType() 
                            { 
                                Name = "Baby", AIAgeGroup = AIAgeGroup.Baby, CanReproduce = false, Edge = 0.5f, HeightTargetModifier = 0.05f, WeightTargetModifier = 0.03f, ModelScaleFraction = 0.3f/*, 
                                NeedTypes = new NeedType[]{ babySleepNeed }*/
                                    //,NeedTypes = new NeedType[]{ bushDragonFoodNeed } //since they cant eat any naturel resources and are too scared to come close to humans, we turned it off until such a time it is implemented
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Child", AIAgeGroup = AIAgeGroup.Child, CanReproduce = false, Edge = 1f, HeightTargetModifier = 0.6f, WeightTargetModifier = 0.5f, ModelScaleFraction = 0.6f  
                             // , NeedTypes = new NeedType[]{ childSleepNeed }
                                 //,NeedTypes = new NeedType[]{ bushDragonFoodNeed } //since they cant eat any naturel resources and are too scared to come close to humans, we turned it off until such a time it is implemented
                            },
                            new AgeGroupType() 
                            { 
                                Name = "Young adult", AIAgeGroup = AIAgeGroup.YoungAdult, CanReproduce = false, Edge = 4f, HeightTargetModifier = 0.97f, WeightTargetModifier = 0.92f, ModelScaleFraction = 0.85f  
                            //   , NeedTypes = new NeedType[]{ youngAdultSleepNeed }
                                //,NeedTypes = new NeedType[]{ bushDragonFoodNeed } //since they cant eat any naturel resources and are too scared to come close to humans, we turned it off until such a time it is implemented
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Adult", AIAgeGroup = AIAgeGroup.Adult, CanReproduce = true, Edge = 28f, HeightTargetModifier = 1f, WeightTargetModifier = 1f, ModelScaleFraction = 1f  
                          //    , NeedTypes = new NeedType[]{ adultSleepNeed }
                                //,NeedTypes = new NeedType[]{ bushDragonFoodNeed } //since they cant eat any naturel resources and are too scared to come close to humans, we turned it off until such a time it is implemented
                            }
                            ,
                            new AgeGroupType() 
                            { 
                                Name = "Old", AIAgeGroup = AIAgeGroup.Old, CanReproduce = false, Edge = 30f, HeightTargetModifier = 0.9f, WeightTargetModifier = 1f, ModelScaleFraction = 1f  
                           //   , NeedTypes = new NeedType[]{ oldSleepNeed }
                                //,NeedTypes = new NeedType[]{ bushDragonFoodNeed } //since they cant eat any naturel resources and are too scared to come close to humans, we turned it off until such a time it is implemented
                            }
                        } 
                    } 
                }

                };

                listOfEntityTypes.Add(bushdragonType);

                #endregion


                #region Spoak Demon Tree needs
                NeedType demonTreeFoodNeed = new NeedType()
                {
                    KeyName = "foodEnergy",
                    FoodNeedType = new FoodNeedType()
                    { 
                        FoodNutrient = "foodEnergy",
                        RequiredNutrientsAsFractionOfEntityBulk = 0.03f   //0.012f   answering the hover-tooltip: we assume a days' worth of recommended protein. 0,012 f bulk protein/gameday / 1f = 0,012f   ...... calculated here (under protein) https://docs.google.com/a/unclaimedworld-game.com/document/d/106BIllzcCkma24NlcWCZLwyEpHaG6AWed0GpSUUMTug/edit
                 
                    },
                    DecreasePerDay = new NormalDistribution() { Mean = 3f, StandardDeviation = 0.02f }, 
                    LimitForDecreasedEnergy = 0.05f,
                    DecreasedEnergyWeight = 0.08f, // 0.6f,
                    PhysicalEffects = new PhysicalEffects()
                    {
                        DaysAtZeroCausingCollapse = 2f,
                        DaysAtZeroCausingDeath = 2.1f,
                        DaysAtZeroDecreaseFactor = 1f, //0.5f,
                        LimitForReducedGrowth = 0.1f,
                        LimitForIncreasedSickness = 0.05f,
                        UseExertionFactorToDecrease = false                          
                    }
                };

                #endregion

                #region  Spoak demonTreetype

                EntityType demonTreeType = new EntityType("entity:spoakDendront")
                {

                    Name = "Spoak dendront",
                    ThumbnailSmall = "HUD_thumbnail_demonTree",
                    SummaryDescription = "Ambush predator",  //
                    Description = "\n FEEDING CLASSIFICATION: Ambush predator.\n \n HEIGHT: up to 250 cm\n \n ANATOMY\n Like other dendronts, this species has specialized in camouflage and mimicry of the surrounding vegetation, in this case spoak trees. The order shares some similarities with the bush dragons, notably, its 3-legged anatomy and the large wing-like appendages.\n \n BEHAVIOR\n The animal is able to blend in with spoak trees, even matching the way the wind moves the leaves. When prey (typically thunder chicken) is near, the dendront seizes it with a quick thrust of its sharp limbs. If the spoak dendront is revealed by competitors (or humans), it will try to scare them away by spreading out its 'leaves'. This menacing pose made some colonists name the animal 'Tree demon' though the name has not been widely adopted.\n \n THREAT LEVEL\n High. The animal is very hard to notice until too late, so caution is recommended in the animal's habitat.",
                  
                    DetectionTag = "wellHiddenAnimal", //bso: make sure to check that the tag is part of the defaultDetection settings that is used for other creatures // should only be detected after using examine! but sensor will detect it. 
                    RenderableType = new RenderableType()
                    {
                        RenderAsModelType = new RenderAsModelType()
                        {

                            ModelScale = 3.8f, // priority goes: race ModelScale / cast ModelScale / default (renderAdModelType ModelScale)
                            AssetName = "demonTree",
                            ModelBasicTextureName = "DemonTreeTexture",
                            GaitAnimations = new XmlDictionary<string, GaitAnimationBracket[]>
                       {
                        
                        { "normal", new[]{                                           
                                              new GaitAnimationBracket(){ MinimumSpeed = 0f, MaximumSpeed = 5f, StrideLength = 1f /*measured: 24f*/, StrideDuration = 0.44f, AnimationKey = "walk"},                                            
                                              } }
                       },

                            AnimConditions = new AnimConditionInfo[] 
                        {
                            new AnimConditionInfo()
                            {                              
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Moving},                            
                                GaitSetKey = "normal"
                            },
                                           
                            new AnimConditionInfo() 
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "idle" }},                            
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle}                              
                            },

                             new AnimConditionInfo() 
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "eat" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Eating }                             
                            },


                             new AnimConditionInfo() 
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "combatIdle" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Bold)}                              
                            },
                              new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "attack" }}, 
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Attacking,}, 
                  //              Looping = Looping.No //mp 2015 why is this not used for animals but for a lot of human anims??  when i used it, the animal died after very short fight? without this, the animal fights for much longer????                       
  
                            },


                              new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "hit" }},
                                //Demontree hit sound here
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Recoiling }
                            },

                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{ "dead" }}, //no twitching anim made yet, so just using dead anim.
                      //          Sound = "aliens/rattleWooden",  bug - never stops ..21 oct 2013
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying }
                            },

                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "collapse" }, Sounds = new string[]{ "aliens/rattleWooden" } }, //necessary.                              
                                Looping = Looping.No,
                                
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Pre)}  
                            },
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{ "dead"  }}, //necessary. put in name of dead anim when made
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Post)}  
                            }

                        }
                        }
                    },
                    LocomotorType = new LocomotorType()
                    {
                        MaxAngularSpeed = 4f * MathHelper.Pi, // MathHelper.Pi,
                        FourSidedSymmetry = false,
                        MeleeRadius = 22f,

                        LeggedLocomotorType = new LeggedLocomotorType()
                        {
                            TerrainNegateFactor = 0.5f,
                            WalkSlowSpeed = 4f, // 8f, 
                            WalkNormalSpeed = 8f /*debug only*/,/*proper speed*///, ModelScale = 3f : (NORMAL patrician walk speed=1.0, basespeed=18f ) (SLOW patrician walk speed=0.7, basespeed=11f) (FAST patrician walk speed=2, basespeed=35f) (SUPERSLOW Don't use ... patrician walk speed=0.3, basespeed=11f)
                            WalkFastSpeed = 12f, // 18f,//30f, // 63f,
                        },
                        CollisionResponderType = new CollisionResponderType()
                        {
                            AgentCollisionResponderType = new AgentCollisionResponderType()
                        }
                    },
                    SensorType = new SensorType()
                    {
                        Range = 225,
                        RangeAtNight = 150,
                        DetectionTypeKey = "defaultDetection"
                    },

                    BodyType = GameData.Instance.AllBodyTypes["demonTree"],
                    ContainerType = new AgentStorageType() //0.5f)
                    {
                        ItemStorageType = new ItemStorageType(0.8f), // can Haul... later.
                        StomachStorageType = new ItemStorageType(0.07f) // stomach size is dynamic - is defined in BioType                        
                    }
                };
                demonTreeType.IntelligenceType = new IntelligenceType() // remember to replace demonTreeType.! when copy pasting
                {
                    IsMobile = true,
                    CanAttack = true,
                    CanUseWeapons = false,
                    CanHunt = true,
                    CanScout = true,
                    CanExamine = true, 
                    CanPatrol = true,
                    CanHaul = false,
                    IsPredator = true,
                    WillAttackNonThreatsNearby = true,
                    StrengthRating = StrengthRating.LikeHumans,
                    InterestInTriggerTypes = new[] { "mineTrigger", "spikeTrapTrigger" },
                    Courage = 0.5f,
                    MemoryInDays = 3f,
                    Boldness = 0.8f,
                    AggroRange = 125f,
                    ContainerTransactTag = "demonTreeTransact", //was "demonTree"
                    ChanceToRestAfterMeleeAttack = 0.5,
                    MinRestTimeAfterAttackingInSeconds = 0.5f,
                    MaxRestTimeAfterAttackingInSeconds = 1f,
                    Prey = new string[] { "entity:mudWorm", "entity:binalRat", "entity:whiteThunderChicken", "entity:pygmyThunderChicken", "entity:bajingan" },
                
                    Attacks = new[]
                    { 
                         // TB: has a special anti-thunderchicken attack type to help it kill its prey before it runs away from it - JAN 2015
                         "demonTreeAttack"         
                     },
                    Skills = new SerializableDictionary<string, float> 
                    { 
                        {
                           "unarmedFighting" , 0.9f
                        }
                    },
                };



                demonTreeType.BiologicalType = new BiologicalType() // remember to replace demonTreeType.! when copy pasting
                {
                    OrderKey = "demonTreeOrder",
                    OxygenAndMuscleEnergyIncreaseRatePerDay = 10f,
                    TimeToConsumeFullMealInDays = 0.0025f,
                    FoodItemTagsThatCanBeConsumed = new[] { "cookedMeat", "rawMeat", "smallRawMeat", "spoiledMeal", "inedibleMeat", "rottenMeat" },
                    ExtractionProcessTypes = new[] { "extractThunderChickenMeat"  }, // , "twinklerExtractThunderChickenGuts" 
                    StomachSizeFractionOfEntityBulk = 0.15f,
                    StomachContentsDecreaseRatePerDay = 0.25f,
                  
                    IsTerritorial = true,
                    MaxRegainLimit = humanMaxRegainLimit,
                    FractionOfMaxHitpointsGainedPerDay = humanFractionOfMaxHitpointsGainedPerDay,
                    ActiveStealthRating = 1f, //0.1f
                    Carcass = "item:demontreeCarcass", //
                    //new ResourceType("patriciancarcass") { ResourceItem = CreatePlaceholder("item:patriciancarcass") },
                    Castes = new List<CasteType>() 
                { 
                    new CasteType() { KeyName = "male",
                        Reproduction = Reproduction.Male, Edge = 0.51f,
                        HeightMean = 2f, HeightStandardDeviation = 0.08f, 
                        WeightMean = 110f, WeightStandardDeviation = 0.15f,
                         
                        AgeGroupTypes = new List<AgeGroupType>() 
                        { 
                            new AgeGroupType() 
                            { 
                                Name = "Baby", AIAgeGroup = AIAgeGroup.Baby, CanReproduce = false, Edge = .5f, HeightTargetModifier = 0.05f, WeightTargetModifier = 0.03f, ModelScaleFraction = 0.3f,
                                NeedTypes = new NeedType[]{ demonTreeFoodNeed } 
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Child", AIAgeGroup = AIAgeGroup.Child, CanReproduce = false, Edge = 1f, HeightTargetModifier = 0.6f, WeightTargetModifier = 0.5f, ModelScaleFraction = 0.6f,  
                                NeedTypes = new NeedType[]{ demonTreeFoodNeed }
                            },
                            new AgeGroupType() 
                            { 
                                Name = "Young adult", AIAgeGroup = AIAgeGroup.YoungAdult, CanReproduce = false, Edge = 4f, HeightTargetModifier = 0.97f, WeightTargetModifier = 0.92f, ModelScaleFraction = 0.85f,
                                NeedTypes = new NeedType[]{ demonTreeFoodNeed }
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Adult", AIAgeGroup = AIAgeGroup.Adult, CanReproduce = true, Edge = 24f, HeightTargetModifier = 1f, WeightTargetModifier = 1f, ModelScaleFraction = 1f,
                                NeedTypes = new NeedType[]{ demonTreeFoodNeed }
                            }
                            ,
                            new AgeGroupType() 
                            { 
                                Name = "Old", AIAgeGroup = AIAgeGroup.Old, CanReproduce = true, Edge = 25f, HeightTargetModifier = 0.9f, WeightTargetModifier = 1f,
                                ModelScaleFraction = 1f,
                                NeedTypes = new NeedType[]{ demonTreeFoodNeed }
                            }
                        } 
                    },
                    new CasteType() { KeyName = "female",
                        Reproduction = Reproduction.Female, Edge = 1f,
                        HeightMean = 1.90f, HeightStandardDeviation = 0.05f, 
                        WeightMean = 100f, WeightStandardDeviation = 0.10f,

                        AgeGroupTypes = new List<AgeGroupType>() 
                        { 
                            new AgeGroupType() 
                            { 
                                Name = "Baby", AIAgeGroup = AIAgeGroup.Baby, CanReproduce = false, Edge = .5f, HeightTargetModifier = 0.05f, WeightTargetModifier = 0.03f, ModelScaleFraction = 0.3f,
                                NeedTypes = new NeedType[]{ demonTreeFoodNeed }
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Child", AIAgeGroup = AIAgeGroup.Child, CanReproduce = false, Edge = 1f, HeightTargetModifier = 0.6f, WeightTargetModifier = 0.5f, ModelScaleFraction = 0.6f,  
                                NeedTypes = new NeedType[]{ demonTreeFoodNeed }
                            },
                            new AgeGroupType() 
                            { 
                                Name = "Young adult", AIAgeGroup = AIAgeGroup.YoungAdult, CanReproduce = false, Edge = 4f, HeightTargetModifier = 0.97f, WeightTargetModifier = 0.92f, ModelScaleFraction = 0.85f,
                                NeedTypes = new NeedType[]{ demonTreeFoodNeed }
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Adult", AIAgeGroup = AIAgeGroup.Adult, CanReproduce = true, Edge = 24f, HeightTargetModifier = 1f, WeightTargetModifier = 1f, ModelScaleFraction = 1f,
                                NeedTypes = new NeedType[]{ demonTreeFoodNeed }
                            }
                            ,
                            new AgeGroupType() 
                            { 
                                Name = "Old", AIAgeGroup = AIAgeGroup.Old, CanReproduce = false, Edge = 25f, HeightTargetModifier = 0.9f, WeightTargetModifier = 1f, ModelScaleFraction = 1f,
                                NeedTypes = new NeedType[]{ demonTreeFoodNeed }
                            }
                        } 
                    } 
                }

                };

                listOfEntityTypes.Add(demonTreeType); // remember to replace!! when copy pasting. else you will get an error that says 'duplicate key', for example

                #region Swamp demon tree

                #region Swamp demon Tree needs
                demonTreeFoodNeed = new NeedType()
                {
                    KeyName = "foodEnergy",
                    FoodNeedType = new FoodNeedType()
                    { 
                        FoodNutrient = "foodEnergy",
                        RequiredNutrientsAsFractionOfEntityBulk = 0.03f   //0.012f   answering the hover-tooltip: we assume a days' worth of recommended protein. 0,012 f bulk protein/gameday / 1f = 0,012f   ...... calculated here (under protein) https://docs.google.com/a/unclaimedworld-game.com/document/d/106BIllzcCkma24NlcWCZLwyEpHaG6AWed0GpSUUMTug/edit
                  
                    },
                    DecreasePerDay = new NormalDistribution() { Mean = 3f, StandardDeviation = 0.02f },
                    LimitForDecreasedEnergy = 0.05f,
                    DecreasedEnergyWeight = 0.08f, // 0.6f,
                    PhysicalEffects = new PhysicalEffects()
                    {
                        DaysAtZeroCausingCollapse = 2f,
                        DaysAtZeroCausingDeath = 3f, //old 2.1 // bso increased for balancing with no food source, because currently there are no thunder chicken-like food sources in swamps. change this when swamp demon gets a food source.
                        DaysAtZeroDecreaseFactor = 1f, //0.5f,
                        LimitForReducedGrowth = 0.1f,
                        LimitForIncreasedSickness = 0.05f,
                        UseExertionFactorToDecrease = false
                    }
                };
                #endregion

                demonTreeType = new EntityType("entity:swampDendront") //placeholder txt 
                {

                    Name = "Swamp dendront", //
                    ThumbnailSmall = "HUD_thumbnail_demonTreeSwamp",
                    SummaryDescription = "Ambush predator",  //
                    Description = "\n FEEDING CLASSIFICATION: Ambush predator.\n \n HEIGHT: up to 250 cm\n \n ANATOMY\n Amphibious species of dendronts which is adapted for the swamp. The dendront order shares some similarities with the bush dragons, notably, its 3-legged anatomy and the flexible appendages which in this species look like vines and branches.\n \n BEHAVIOR\n Will slither around the swamp until prey is within striking distance, at which point the dendront is usually able to kill it with a single strike.\n \n THREAT LEVEL\n High. The animal is very hard to notice until too late, so caution is recommended in the animal's habitat.",
                  
                    DetectionTag = "wellHiddenAnimal", // should only be detected after using examine! but sensor will detect it.  ...OLD txt, not sure if relevant?: also set  ActiveStealthRating = 1f further down.
                    RenderableType = new RenderableType()
                    {
                        RenderAsModelType = new RenderAsModelType()
                        {
                            ModelScale = 3.8f, // priority goes: race ModelScale / cast ModelScale / default (renderAdModelType ModelScale) 
                            AssetName = "demonTreeMoss",
                            ModelBasicTextureName = "DemonTreeMossTexture1",
                            GaitAnimations = new XmlDictionary<string, GaitAnimationBracket[]>
                       {
                        
                        { "normal", new[]{                                           
                                              new GaitAnimationBracket(){ MinimumSpeed = 0f, MaximumSpeed = 5f, StrideLength = 1f /*measured: 24f*/, StrideDuration = 0.44f, AnimationKey = "walk"},                                            
                                              } }
                       },

                            AnimConditions = new AnimConditionInfo[] 
                        {
                            new AnimConditionInfo()
                            {                              
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Moving},                            
                                GaitSetKey = "normal"
                            },
                                           
                            new AnimConditionInfo() 
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "idle" }},                            
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle}                              
                            },

                             new AnimConditionInfo() 
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "eat" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Eating }                             
                            },


                             new AnimConditionInfo() 
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "combatIdle" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Bold)}                              
                            },
                              new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "attack" }}, 
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Attacking,}, 
                  //              Looping = Looping.No //mp 2015 why is this not used for animals but for a lot of human anims??  when i used it, the animal died after very short fight? without this, the animal fights for much longer????                       
  
                            },


                              new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "hit" }},
                                //Demontree hit sound here
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Recoiling }
                            },

                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{ "dead" }}, //no twitching anim made yet, so just using dead anim.
                      //          Sound = "aliens/rattleWooden",  bug - never stops ..21 oct 2013
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying }
                            },

                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "collapse" }, Sounds = new string[]{ "aliens/rattleWooden" }}, //necessary.                             
                                Looping = Looping.No,
                                
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Pre)}  
                            },
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{ "dead"  }}, //necessary. put in name of dead anim when made
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Post)}  
                            }

                        }
                        }
                    },
                    LocomotorType = new LocomotorType()
                    {
                        MaxAngularSpeed = 4f * MathHelper.Pi, // MathHelper.Pi,
                        FourSidedSymmetry = false,
                        MeleeRadius = 22f,

                        LeggedLocomotorType = new LeggedLocomotorType()
                        {
                            TerrainNegateFactor = 0.5f,
                            WalkSlowSpeed = 4f, // 8f, 
                            WalkNormalSpeed = 8f /*debug only*/,/*proper speed*///, ModelScale = 3f : (NORMAL patrician walk speed=1.0, basespeed=18f ) (SLOW patrician walk speed=0.7, basespeed=11f) (FAST patrician walk speed=2, basespeed=35f) (SUPERSLOW Don't use ... patrician walk speed=0.3, basespeed=11f)
                            WalkFastSpeed = 12f, // 18f,//30f, // 63f,
                        },
                        CollisionResponderType = new CollisionResponderType()
                        {
                            AgentCollisionResponderType = new AgentCollisionResponderType()
                        }
                    },
                    SensorType = new SensorType()
                    {
                        Range = 225,
                        RangeAtNight = 150,
                        DetectionTypeKey = "defaultDetection"
                    },

                    BodyType = GameData.Instance.AllBodyTypes["demonTree"],
                    ContainerType = new AgentStorageType() //0.5f)
                    {
                        ItemStorageType = new ItemStorageType(0.8f), // can Haul... later.
                        StomachStorageType = new ItemStorageType(0.07f) // stomach size is dynamic - is defined in BioType
                        
                    }
                };
                demonTreeType.IntelligenceType = new IntelligenceType() // remember to replace demonTreeType.! when copy pasting
                {
                    IsMobile = true,
                    CanAttack = true,
                    CanUseWeapons = false,
                    CanHunt = true,
                    CanScout = true,
                    CanExamine = true, 
                    CanPatrol = true,
                    CanHaul = false,
                    IsPredator = true,
                    WillAttackNonThreatsNearby = true,
                    StrengthRating = StrengthRating.LikeHumans,
                    InterestInTriggerTypes = new[] { "mineTrigger", "spikeTrapTrigger" },
                    Courage = 0.5f,
                    MemoryInDays = 3f,
                    Boldness = 0.8f,
                    AggroRange = 125f,
                    ContainerTransactTag = "demonTreeTransact", //was "demonTree"
                    ChanceToRestAfterMeleeAttack = 0.5,
                    MinRestTimeAfterAttackingInSeconds = 0.5f,
                    MaxRestTimeAfterAttackingInSeconds = 1f,
                    Prey = new string[] { "entity:mudWorm", "entity:binalRat", "entity:whiteThunderChicken", "entity:pygmyThunderChicken", "entity:bajingan" },
                
                    Attacks = new[]
                    { 
                         // TB: has a special anti-thunderchicken attack type to help it kill its prey before it runs away from it - JAN 2015
                         "demonTreeAttack"         
                     },
                    Skills = new SerializableDictionary<string, float> 
                    { 
                        {
                           "unarmedFighting" , 0.9f
                        }
                    },
                };



                demonTreeType.BiologicalType = new BiologicalType() // remember to replace demonTreeType.! when copy pasting
                {
                    OrderKey = "demonTreeOrder",
                    OxygenAndMuscleEnergyIncreaseRatePerDay = 10f,
                    TimeToConsumeFullMealInDays = 0.0025f,
                    FoodItemTagsThatCanBeConsumed = new[] { "cookedMeat", "rawMeat", "smallRawMeat", "spoiledMeal", "inedibleMeat", "rottenMeat" },
                    ExtractionProcessTypes = new[] { "extractThunderChickenMeat" }, // , "twinklerExtractThunderChickenGuts" 
                    StomachSizeFractionOfEntityBulk = 0.15f,
                    StomachContentsDecreaseRatePerDay = 0.25f,

                    IsTerritorial = true,
                    MaxRegainLimit = humanMaxRegainLimit,
                    FractionOfMaxHitpointsGainedPerDay = humanFractionOfMaxHitpointsGainedPerDay,
                    ActiveStealthRating = 1f, //0.1f
                    Carcass = "item:swampDemonTreeCarcass", //
                    //new ResourceType("patriciancarcass") { ResourceItem = CreatePlaceholder("item:patriciancarcass") },
                    Castes = new List<CasteType>() 
                { 
                    new CasteType() { KeyName = "male",
                        Reproduction = Reproduction.Male, Edge = 0.51f,
                        HeightMean = 2f, HeightStandardDeviation = 0.08f, 
                        WeightMean = 110f, WeightStandardDeviation = 0.15f,
                         
                        AgeGroupTypes = new List<AgeGroupType>() 
                        { 
                            new AgeGroupType() 
                            { 
                                Name = "Baby", AIAgeGroup = AIAgeGroup.Baby, CanReproduce = false, Edge = .5f, HeightTargetModifier = 0.05f, WeightTargetModifier = 0.03f, ModelScaleFraction = 0.3f,
                                NeedTypes = new NeedType[]{ demonTreeFoodNeed } 
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Child", AIAgeGroup = AIAgeGroup.Child, CanReproduce = false, Edge = 1f, HeightTargetModifier = 0.6f, WeightTargetModifier = 0.5f, ModelScaleFraction = 0.6f,  
                                NeedTypes = new NeedType[]{ demonTreeFoodNeed }
                            },
                            new AgeGroupType() 
                            { 
                                Name = "Young adult", AIAgeGroup = AIAgeGroup.YoungAdult, CanReproduce = false, Edge = 5f, HeightTargetModifier = 0.97f, WeightTargetModifier = 0.92f, ModelScaleFraction = 0.85f,
                                NeedTypes = new NeedType[]{ demonTreeFoodNeed }
                            },
                            new AgeGroupType() 
                            { 
                                Name = "Adult", AIAgeGroup = AIAgeGroup.Adult, CanReproduce = true, Edge = 28f, HeightTargetModifier = 1f, WeightTargetModifier = 1f, ModelScaleFraction = 1f,
                                NeedTypes = new NeedType[]{ demonTreeFoodNeed }
                            },
                            new AgeGroupType() 
                            { 
                                Name = "Old", AIAgeGroup = AIAgeGroup.Old, CanReproduce = true, Edge = 30f, HeightTargetModifier = 0.9f, WeightTargetModifier = 1f,
                                ModelScaleFraction = 1f,
                                NeedTypes = new NeedType[]{ demonTreeFoodNeed }
                            }
                        } 
                    },
                    new CasteType() { KeyName = "female",
                        Reproduction = Reproduction.Female, Edge = 1f,
                        HeightMean = 1.90f, HeightStandardDeviation = 0.05f, 
                        WeightMean = 100f, WeightStandardDeviation = 0.10f,

                        AgeGroupTypes = new List<AgeGroupType>() 
                        { 
                            new AgeGroupType() 
                            { 
                                Name = "Baby", AIAgeGroup = AIAgeGroup.Baby, CanReproduce = false, Edge = .5f, HeightTargetModifier = 0.05f, WeightTargetModifier = 0.03f, ModelScaleFraction = 0.3f,
                                NeedTypes = new NeedType[]{ demonTreeFoodNeed }
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Child", AIAgeGroup = AIAgeGroup.Child, CanReproduce = false, Edge = 1f, HeightTargetModifier = 0.6f, WeightTargetModifier = 0.5f, ModelScaleFraction = 0.6f,  
                                NeedTypes = new NeedType[]{ demonTreeFoodNeed }
                            },
                            new AgeGroupType() 
                            { 
                                Name = "Young adult", AIAgeGroup = AIAgeGroup.YoungAdult, CanReproduce = false, Edge = 5f, HeightTargetModifier = 0.97f, WeightTargetModifier = 0.92f, ModelScaleFraction = 0.85f,
                                NeedTypes = new NeedType[]{ demonTreeFoodNeed }
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Adult", AIAgeGroup = AIAgeGroup.Adult, CanReproduce = true, Edge = 28f, HeightTargetModifier = 1f, WeightTargetModifier = 1f, ModelScaleFraction = 1f,
                                NeedTypes = new NeedType[]{ demonTreeFoodNeed }
                            }
                            ,
                            new AgeGroupType() 
                            { 
                                Name = "Old", AIAgeGroup = AIAgeGroup.Old, CanReproduce = false, Edge = 30f, HeightTargetModifier = 0.9f, WeightTargetModifier = 1f, ModelScaleFraction = 1f,
                                NeedTypes = new NeedType[]{ demonTreeFoodNeed }
                            }
                        } 
                    } 
                }

                };

                listOfEntityTypes.Add(demonTreeType); // remember to replace!! when copy pasting. else you will get an error that says 'duplicate key', for example
                #endregion

                #endregion


                #region forestguardiantype
                EntityType forestguardianType = new EntityType("entity:forestGuardian")
                {
                    Name = "Forest guardian",
                    RenderableType = new RenderableType()
               {
                   RenderAsModelType = new RenderAsModelType()
                   {
                       //ModelScale = 2.3f, //2.5f // no need, priority goes: race ModelScale / cast ModelScale / default (renderAdModelType ModelScale)
                       AssetName = "forestguardian",
                       GaitAnimations = new XmlDictionary<string, GaitAnimationBracket[]>
                       {
                        
                        { "normal", new[]{                                           
                                              new GaitAnimationBracket(){ MinimumSpeed = 5f, MaximumSpeed = 20f, StrideLength = 22.2f, StrideDuration = 1f, AnimationKey = "gaitWalk"},
                                            
                                              } }

                       },

                       AnimConditions = new AnimConditionInfo[] 
                        {
                            new AnimConditionInfo()
                            {                              
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Moving },
                                GaitSetKey = "normal"
                            },                                          
                            new AnimConditionInfo() 
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "idle" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle }
                            },
                             new AnimConditionInfo() 
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "eat" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Eating }
                            },

                             new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{ "collapse" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Pre)},  
                                Looping = Looping.No
                            },
                             new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "dead" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying }
                            },
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "dead" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Post) } 
                            }
                        }
                   }
               },
                    LocomotorType = new LocomotorType()
                    {
                        MaxAngularSpeed = .08f * MathHelper.Pi, // MathHelper.Pi,
                        FourSidedSymmetry = true,
                        MeleeRadius = 22f,

                        LeggedLocomotorType = new LeggedLocomotorType()
                        {
                            TerrainNegateFactor = 0.9f,
                            WalkSlowSpeed = 1.71f, // 8f, 
                            WalkNormalSpeed = 1.71f /*debug only*/, // for walk 0.2, and modelscale 1.8, basespeed must be 1.5.............for walk 0.2, and modelscale 2.3, basespeed must be 1.71
                            WalkFastSpeed = 1.71f, // 18f,//30f, // 63f,
                        },
                        CollisionResponderType = new CollisionResponderType()
                        {
                            AgentCollisionResponderType = new AgentCollisionResponderType()
                        }
                    },
                    SensorType = new SensorType()
                    {
                        Range = sensorRangeHuman,
                        RangeAtNight = sensorRangeHumanNight,
                        DetectionTypeKey = "defaultDetection"
                    },

                    BodyType = GameData.Instance.AllBodyTypes["forestguardian"]
                };


                forestguardianType.IntelligenceType = new IntelligenceType()
                {
                    IsMobile = true,
                    CanAttack = false,
                    CanUseWeapons = false,
                    CanHunt = false,
                    CanScout = true,
                    CanExamine = true, 
                    CanPatrol = false,
                    CanHaul = false,

                    Boldness = 0.4f,
                    MemoryInDays = 1f,
                    StrengthRating = Entities.StrengthRating.WeakerThanHumans,
                    InterestInTriggerTypes = new[] { "mineTrigger", "spikeTrapTrigger" },
                    AggroRange = 0.0f
                };



                forestguardianType.BiologicalType = new BiologicalType()
                {
                    OrderKey = "forestGuardianOrder",
                    OxygenAndMuscleEnergyIncreaseRatePerDay = 4f,
                    TimeToConsumeFullMealInDays = 0.0125f,
                    StomachSizeFractionOfEntityBulk = 0.3f,
                    StomachContentsDecreaseRatePerDay = 3,
                    ActiveStealthRating = 0.1f,
                    MaxRegainLimit = humanMaxRegainLimit,
                    FractionOfMaxHitpointsGainedPerDay = humanFractionOfMaxHitpointsGainedPerDay,
                    Carcass = "item:forestGuardianCarcass",
                    RaceTypes = new []
                                {     
                                    new RaceType()                              // Beige
                                    { KeyName = "pale", Name="Pale race", PortraitSkinType="Pale", PrimaryColor = "F7F4C5".ToColorVector3(), ModelBasicTextureName = "BushbackPaleTexture", Edge = 0.3f, ModelScale = 2.3f
                                    },
                                    new RaceType()                              // Dark
                                    { KeyName = "dark", Name="Dark race", PortraitSkinType="Dark", PrimaryColor = "564920".ToColorVector3(), ModelBasicTextureName = "BushbackDarkTexture", Edge = 0.6f, ModelScale = 2.3f
                                    },
                                    new RaceType()                              // Black
                                    { KeyName = "black", Name="", PortraitSkinType = "Black", PrimaryColor = "3F3411".ToColorVector3(),  Edge = 0.9f, ModelScale = 2.3f
                                    }
                                },
                    Castes = new List<CasteType>() 
                { 
                    new CasteType() { KeyName = "male",
                        Reproduction = Reproduction.Male, Edge = 0.51f,
                        HeightMean = 2f, HeightStandardDeviation = 0.08f, 
                        WeightMean = 100f, WeightStandardDeviation = 0.15f,

                        AgeGroupTypes = new List<AgeGroupType>() 
                        { 
                            new AgeGroupType() 
                            { 
                                Name = "Baby", AIAgeGroup = AIAgeGroup.Baby, CanReproduce = false, Edge = 1.5f, HeightTargetModifier = 0.05f, WeightTargetModifier = 0.03f, ModelScaleFraction = 0.3f/*, 
                                NeedTypes = new NeedType[]{ babySleepNeed }*/
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Child", AIAgeGroup = AIAgeGroup.Child, CanReproduce = false, Edge = 11f, HeightTargetModifier = 0.6f, WeightTargetModifier = 0.5f, ModelScaleFraction = 0.6f 
                             //   , NeedTypes = new NeedType[]{ childSleepNeed }
                            },
                            new AgeGroupType() 
                            { 
                                Name = "Young adult", AIAgeGroup = AIAgeGroup.YoungAdult, CanReproduce = false, Edge = 16f, HeightTargetModifier = 0.97f, WeightTargetModifier = 0.92f, ModelScaleFraction = 0.85f  
                                //, NeedTypes = new NeedType[]{ youngAdultSleepNeed }
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Adult", AIAgeGroup = AIAgeGroup.Adult, CanReproduce = true, Edge = 72f, HeightTargetModifier = 1f, WeightTargetModifier = 1f, ModelScaleFraction = 1f  
                             //  , NeedTypes = new NeedType[]{ adultSleepNeed }
                            }
                            ,
                            new AgeGroupType() 
                            { 
                                Name = "Old", AIAgeGroup = AIAgeGroup.Old, CanReproduce = true, Edge = 200f, HeightTargetModifier = 0.9f, WeightTargetModifier = 1f, ModelScaleFraction = 1f  
                             //  , NeedTypes = new NeedType[]{ oldSleepNeed }
                            }
                        } 
                    },
                    new CasteType() { KeyName = "female", Reproduction = Reproduction.Female, Edge = 1f, //????
                        HeightMean = 1.90f, HeightStandardDeviation = 0.05f, 
                        WeightMean = 90f, WeightStandardDeviation = 0.10f,

                        AgeGroupTypes = new List<AgeGroupType>() 
                        { 
                            new AgeGroupType() 
                            { 
                                Name = "Baby", AIAgeGroup = AIAgeGroup.Baby, CanReproduce = false, Edge = 1.5f, HeightTargetModifier = 0.05f, WeightTargetModifier = 0.03f, ModelScaleFraction = 0.3f/*, 
                                NeedTypes = new NeedType[]{ babySleepNeed }*/
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Child", AIAgeGroup = AIAgeGroup.Child, CanReproduce = false, Edge = 11f, HeightTargetModifier = 0.6f, WeightTargetModifier = 0.5f, ModelScaleFraction = 0.6f  
                             // , NeedTypes = new NeedType[]{ childSleepNeed }
                            },
                            new AgeGroupType() 
                            { 
                                Name = "Young adult", AIAgeGroup = AIAgeGroup.YoungAdult, CanReproduce = false, Edge = 16f, HeightTargetModifier = 0.97f, WeightTargetModifier = 0.92f, ModelScaleFraction = 0.85f  
                             //  , NeedTypes = new NeedType[]{ youngAdultSleepNeed }
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Adult", AIAgeGroup = AIAgeGroup.Adult, CanReproduce = true, Edge = 72f, HeightTargetModifier = 1f, WeightTargetModifier = 1f, ModelScaleFraction = 1f  
                             // , NeedTypes = new NeedType[]{ adultSleepNeed }
                            }
                            ,
                            new AgeGroupType() 
                            { 
                                Name = "Old", AIAgeGroup = AIAgeGroup.Old, CanReproduce = false, Edge = 200f, HeightTargetModifier = 0.9f, WeightTargetModifier = 1f, ModelScaleFraction = 1f  
                             // , NeedTypes = new NeedType[]{ oldSleepNeed }
                            }
                        } 
                    } 
                }

                };

                listOfEntityTypes.Add(forestguardianType);

                #endregion


                #region Thunder Chicken needs
                NeedType thunderChickenFoodNeed = new NeedType()
                {
                    KeyName = "foodEnergy",
                    FoodNeedType = new FoodNeedType()
                    {
                        FoodNutrient = "foodEnergy",
                        RequiredNutrientsAsFractionOfEntityBulk = 0.1f
                    },
                    DecreasePerDay = new NormalDistribution() { Mean = 2f, StandardDeviation = 0.02f },//MP...july 24.. was 0.6f   ..buffed it to make rats hungrier quicker (because they spawn with full nutrition, 1f, which I can't seem to change anywhere?)
                   
                    LimitForDecreasedEnergy = 0.10f,
                    DecreasedEnergyWeight = 0.6f,
                    PhysicalEffects = new PhysicalEffects()
                    {
                        DaysAtZeroCausingCollapse = 10000f, // set really high, for giving them the ability to eat player food but not die off all the time because theres no food in the wild
                        DaysAtZeroCausingDeath = 10000f,
                        DaysAtZeroDecreaseFactor = 1f, //0.5f,
                        UseExertionFactorToDecrease = true                        
                    },
                };

                #endregion
                #region alternative thunderchicken skins / models
                /*
                 * ModelName = "thunderChickenBulky",   ModelBasicTextureName = "ThunderchickenBulkyTexture1", //mp april 2015 you can put in this mesh here but it has anim problems with the run anim not playing. Maybe samme problem as thin thunder chicken has, with the added anims that need to be commented out. (see the thin species)
                 * ModelName = "thunderchicken",   ModelBasicTextureName = "ThunderchickenDarkTexture",
                 * ModelName = "thunderChickenThin",   ModelBasicTextureName = "ThunderchickenThinTexture1",
                 */
                #endregion
                #region Thunderchickentype

                // mar 2014: I have set up two thunderchicken species: white and pygmy. 2015: also thin (bajingan), which does not have tentacles.
                #region whiteThunderChicken
                EntityType thunderchickenType = new EntityType("entity:whiteThunderChicken")
                {
                    Name = "White thunder chicken",
                    ThumbnailSmall = "HUD_thumbnail_whiteThunderChicken",
                    SummaryDescription = "Quick-running animal",  // http://en.wikipedia.org/wiki/Dodo
                    Description = "\n FEEDING CLASSIFICATION: Omnivore. Prefers to eat smaller bugs.\n \nHEIGHT: 60 cm\n \nANATOMY\n Like all thunder chickens, the white feeds using retractable tentacles around its mouth while an antenna on top of its head detects dangers. \n \nBEHAVIOR\n  When threatened it will quickly run away, sometimes emitting a loud sound. Occasionally solitary, mostly it forms small herds with other species of thunder chicken.\n \nENEMIES\n Hunted by twinkler quadites.\n \nSURVIVAL GUIDE NOTES\n The thunder chickens pose no threat to us and the flesh is edible: Tasty and a good source of protein.",
                  
                    RenderableType = new RenderableType()
               {
                   RenderAsModelType = new RenderAsModelType()
                   {

                       ModelScale = 1.5f, // priority goes: race ModelScale / cast ModelScale / default (renderAdModelType ModelScale)
                       AssetName = "thunderchicken",
                       ModelBasicTextureName = "ThunderchickenPaleTexture",
                       GaitAnimations = new XmlDictionary<string, GaitAnimationBracket[]>
                       {
                        
                        { "normal", new[]{                                           
                                              new GaitAnimationBracket(){ MinimumSpeed = 0f, MaximumSpeed = 77f, StrideLength = 11f /* measured?: 18.1f*/, StrideDuration = 0.4f, AnimationKey = "gaitWalk"},       //MP: iterated strideduration 17th dec 2013.   ... was: MinimumSpeed = 0f, MaximumSpeed = 77f, StrideLength = 11f , StrideDuration = 0.2f, AnimationKey = "gaitWalk"                                     
                                              } }
                       },

                       AnimConditions = new AnimConditionInfo[] 
                        {
                            new AnimConditionInfo()
                            {
                               SoundAndAnimationSet = new RandomSoundAndAnimationSet()
                               {
                                    AdditionalAnimations1 = new string[]{ "hidden" }
                                },
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Moving },
                                GaitSetKey = "normal"
                            },
 
               /*           new AnimConditionInfo()                 //commented out by MP when putting in new gait data. 6 sep 2013
                            {
                                AnimationSet = new RandomAnimationSet(){ 
                                    BaseAnimations = new string[]{  "walk"},
                                    AdditionalAnimations1 = new string[]{ "hidden" }},

                                Conditions = new ConditionSet(){ Modifiers = new BitMask64(typeof(AnimState), (int)AnimState.Moving )                               
                            },*/
                                           
                            new AnimConditionInfo() 
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ 
                                    BaseAnimations = new string[]{  "idle" },
                                    AdditionalAnimations1 = new string[]{ "look" },
                                    AdditionalAnimations2 = new string[]{ "eat" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle }
                            },
                            new AnimConditionInfo() 
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ 
                                    BaseAnimations = new string[]{  "idle" },
                                    AdditionalAnimations2 = new string[]{ "eat" }
                                },
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Eating }
                            },
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{ "dying" }},
                          //      Sound = "aliens/spacechicken",  bug -never stops
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying }
                            },

                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "dead" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Post)}  
                            },
                        }
                   }
               },
                    LocomotorType = new LocomotorType()
                    {

                        MaxAngularSpeed = 1.5f * MathHelper.Pi, // .6f * MathHelper.Pi,                    
                        MeleeRadius = 10f,

                        LeggedLocomotorType = new LeggedLocomotorType()
                        {
                            TerrainNegateFactor = 0.5f,
                            WalkSlowSpeed = 20f, // 8f, 
                            WalkNormalSpeed = 43f /*debug only*/, //25f   18f /*proper speed*/, patrician:26f, turnip:11f
                            WalkFastSpeed = 64f, //24f 18f,//30f, // 63f,


                        },
                        CollisionResponderType = new CollisionResponderType()
                        {
                            AgentCollisionResponderType = new AgentCollisionResponderType()
                        }
                    },
                    SensorType = new SensorType()
                    {
                        Range = sensorRangeHuman + 4, //sensorRangeHuman + 50
                        RangeAtNight = sensorRangeHumanNight,
                        DetectionTypeKey = "defaultDetection"
                    },
                    //TB: the thunderchickens body uses a special SmallAnimalHideLayer that is used in conjunction with the smallAnimalGrabble attack to make it possible for the tree to kill it before it runs away b- JAN 2015
                    BodyType = GameData.Instance.AllBodyTypes["thunderchicken"],

                    ContainerType = new AgentStorageType() //0.5f)
                    {
                        ItemStorageType = new ItemStorageType(0.5f), // can Haul... later.     MP: if you remove this line you get an error june 24 2014
                        StomachStorageType = new ItemStorageType(0.07f) // stomach size is dynamic - is defined in BioType                        
                    }
                };


                thunderchickenType.IntelligenceType = new IntelligenceType()
                {
                    ForageAndHuntingRadius = 250,
                    IsMobile = true,
                    CanAttack = false,
                    CanUseWeapons = false,
                    CanHunt = false,
                    CanScout = true,
                    CanExamine = true, 
                    CanPatrol = false,
                    CanHaul = false,
                    IsPredator = false,  ///
                    ContainerTransactTag = "chickenTransact", //was "chicken"
                    StrengthRating = Entities.StrengthRating.WeakerThanHumans,   //
                    InterestInTriggerTypes = new[] { "mineTrigger", "smallImprovisedTrapTrigger", "spikeTrapTrigger" }, //remember to change for each race of thunderchicken
                    Courage = 0f,
                    MemoryInDays = 0.05f, //mp 0.1f april 2015 was 3f. wanted them to be more mobile but cannot really see a difference.
                    Boldness = 1f, // MP may 6 2014 was 0.2f. I increased it to 1f to make sure they are never in cautious stance, because cautious right now colors their whole world yellow, causing them to freeze when a human apporaches , and all the time run from twinklers. Can be lowered once the strengthratings are adjusted - the goal should be not to have MASSIVE big threatradiusses which cover the whole map, because this causes freezing.
                    AggroRange = 0.0f
                };


                thunderchickenType.BiologicalType = new BiologicalType()
                {
                    OrderKey = "thunderChickenOrder",
                    OxygenAndMuscleEnergyIncreaseRatePerDay = 10f,
                    FoodItemTagsThatCanBeConsumed = new[] { "inedibleVegi", "edibleVegi", "spoiledMeal" }, //MP: crash
                    TimeToConsumeFullMealInDays = 0.0125f,
                    StomachSizeFractionOfEntityBulk = 0.2f,
                    StomachContentsDecreaseRatePerDay = 3,
                    ActiveStealthRating = 0.1f,
                  
                    MaxRegainLimit = humanMaxRegainLimit,
                    FractionOfMaxHitpointsGainedPerDay = humanFractionOfMaxHitpointsGainedPerDay,
                    Carcass = "item:thunderChickenCarcass",
                    Castes = new List<CasteType>() 
                { 
                    ///////// MALE
                    new CasteType() { KeyName = "male",
                        Reproduction = Reproduction.Male, Edge = 0.51f,  
                        HeightMean = 0.5f, HeightStandardDeviation = 0.02f, 
                        WeightMean = 30f, WeightStandardDeviation = 3f,  // multiply WeightStandardDeviation by 3 and subtract that number from WeightMean, this gives the minimum Weight in kilos. divide by 100 to get the bulk...the minimum possible bulk.
                        AgeGroupTypes = new List<AgeGroupType>() 
                        { 
                            new AgeGroupType() 
                            { 
                                Name = "Chick", AIAgeGroup = AIAgeGroup.Baby, CanReproduce = false, Edge = .5f, HeightTargetModifier = 0.05f, WeightTargetModifier = 0.03f, ModelScaleFraction = 0.3f,   // these modifiers are not used as of now, we think . MP
                                NeedTypes = new []{ thunderChickenFoodNeed }
                                /*, NeedTypes = new NeedType[]{ babySleepNeed }*/
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Chick", AIAgeGroup = AIAgeGroup.Child, CanReproduce = false, Edge = 1f, HeightTargetModifier = 0.6f, WeightTargetModifier = 0.5f, ModelScaleFraction = 0.6f,
                                NeedTypes = new []{ thunderChickenFoodNeed }
                                //, NeedTypes = new NeedType[]{ childSleepNeed }
                            },
                            new AgeGroupType() 
                            { 
                                Name = "Grown", AIAgeGroup = AIAgeGroup.YoungAdult, CanReproduce = false, Edge = 2f, HeightTargetModifier = 0.97f, WeightTargetModifier = 0.92f, ModelScaleFraction = 0.85f,
                                NeedTypes = new []{ thunderChickenFoodNeed }
                                //  , NeedTypes = new NeedType[]{ youngAdultSleepNeed }
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Grown", AIAgeGroup = AIAgeGroup.Adult, CanReproduce = true, Edge = 9f, HeightTargetModifier = 1f, WeightTargetModifier = 1f,ModelScaleFraction = 1f,
                                NeedTypes = new []{ thunderChickenFoodNeed }
                                //   , NeedTypes = new NeedType[]{ adultSleepNeed }
                            }
                            ,
                            new AgeGroupType() 
                            { 
                                Name = "Grown", AIAgeGroup = AIAgeGroup.Old, CanReproduce = true, Edge = 10f, HeightTargetModifier = 0.9f, WeightTargetModifier = 1f,
                                ModelScaleFraction =1f,  
                                NeedTypes = new []{ thunderChickenFoodNeed }
                                //  , NeedTypes = new NeedType[]{ oldSleepNeed }
                            }
                        } 
                    },
                   /////////FEMALE
                    new CasteType() { KeyName = "female",
                        Reproduction = Reproduction.Female, Edge = 1f, 
                        HeightMean = 0.45f, HeightStandardDeviation = 0.02f, 
                        WeightMean = 28f, WeightStandardDeviation = 2f,  // multiply WeightStandardDeviation by 3 and subtract that number from WeightMean, this gives the minimum Weight in kilos. divide by 100 to get the bulk...the minimum possible bulk.  
                        

                        AgeGroupTypes = new List<AgeGroupType>() 
                        { 
                            new AgeGroupType() 
                            { 
                                Name = "Chick", AIAgeGroup = AIAgeGroup.Baby, CanReproduce = false, Edge = .5f, HeightTargetModifier = 0.05f, WeightTargetModifier = 0.03f, ModelScaleFraction = 0.3f, // these modifiers are not used as of now, we think . MP
                                /*,  NeedTypes = new NeedType[]{ babySleepNeed }*/
                                NeedTypes = new []{ thunderChickenFoodNeed }
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Chick", AIAgeGroup = AIAgeGroup.Child, CanReproduce = false, Edge = 1f, HeightTargetModifier = 0.6f, WeightTargetModifier = 0.5f, ModelScaleFraction = 0.6f,
                                NeedTypes = new []{ thunderChickenFoodNeed }
                                //  , NeedTypes = new NeedType[]{ childSleepNeed }
                            },
                            new AgeGroupType() 
                            { 
                                Name = "Grown", AIAgeGroup = AIAgeGroup.YoungAdult, CanReproduce = false, Edge = 2f, HeightTargetModifier = 0.97f, WeightTargetModifier = 0.92f, ModelScaleFraction = 0.85f,
                                NeedTypes = new []{ thunderChickenFoodNeed }
                                //     , NeedTypes = new NeedType[]{ youngAdultSleepNeed }
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Grown", AIAgeGroup = AIAgeGroup.Adult, CanReproduce = true, Edge = 10f, HeightTargetModifier = 1f, WeightTargetModifier = 1f, ModelScaleFraction = 1f,
                                NeedTypes = new []{ thunderChickenFoodNeed }
                                //, NeedTypes = new NeedType[]{ adultSleepNeed }
                            }
                            ,
                            new AgeGroupType() 
                            { 
                                Name = "Grown", AIAgeGroup = AIAgeGroup.Old, CanReproduce = false, Edge = 11f, HeightTargetModifier = 0.9f, WeightTargetModifier = 1f, ModelScaleFraction = 1f,
                                NeedTypes = new []{ thunderChickenFoodNeed }
                                //   , NeedTypes = new NeedType[]{ oldSleepNeed }
                            }
                        } 
                    } 
                }

                };

                listOfEntityTypes.Add(thunderchickenType);
                #endregion
                // second species:
                #region pygmyThunderChicken
                thunderchickenType = new EntityType("entity:pygmyThunderChicken")
                {
                    Name = "Pygmy Thunder chicken",
                    ThumbnailSmall = "HUD_thumbnail_thunderChicken",
                    SummaryDescription = "Quick-running animal",  // http://en.wikipedia.org/wiki/Dodo
                    Description = "\n FEEDING CLASSIFICATION: Herbivore.\n \n HEIGHT: up to 40 cm\n \n ANATOMY\n Smaller species of thunder chicken. Like all thunder chickens, feeds using retractable tentacles around its mouth while an antenna on top of its head detects dangers. \n \n BEHAVIOR\n  Quick to flee. Forms small herds with other species of thunder chicken.\n \n THREAT LEVEL\n None.\n \n ENEMIES\n Numerous.\n \n NOTES\n Has been determined edible.",
                   
                    RenderableType = new RenderableType()
                    {
                        RenderAsModelType = new RenderAsModelType()
                        {

                            ModelScale = 1.25f,// priority goes: race ModelScale / cast ModelScale / default (renderAdModelType ModelScale)
                            AssetName = "thunderchicken",
                            ModelBasicTextureName = "ThunderchickenDarkTexture",
                            GaitAnimations = new XmlDictionary<string, GaitAnimationBracket[]>
                       {
                        
                        { "normal", new[]{                                           
                                              new GaitAnimationBracket(){ MinimumSpeed = 0f, MaximumSpeed = 77f, StrideLength = 11f /* measured?: 18.1f*/, StrideDuration = 0.4f, AnimationKey = "gaitWalk"},       //MP: iterated strideduration 17th dec 2013.   ... was: MinimumSpeed = 0f, MaximumSpeed = 77f, StrideLength = 11f , StrideDuration = 0.2f, AnimationKey = "gaitWalk"                                     
                                              } }
                       },

                            AnimConditions = new AnimConditionInfo[] 
                        {
                            new AnimConditionInfo()
                            {
                               SoundAndAnimationSet = new RandomSoundAndAnimationSet()
                               {
                                    AdditionalAnimations1 = new string[]{ "hidden" }
                                },
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Moving },
                                GaitSetKey = "normal"
                            },
 
               /*           new AnimConditionInfo()                 //commented out by MP when putting in new gait data. 6 sep 2013
                            {
                                AnimationSet = new RandomAnimationSet(){ 
                                    BaseAnimations = new string[]{  "walk"},
                                    AdditionalAnimations1 = new string[]{ "hidden" }},

                                Conditions = new ConditionSet(){ Modifiers = new BitMask64(typeof(AnimState), (int)AnimState.Moving )                               
                            },*/
                                           
                            new AnimConditionInfo() 
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ 
                                    BaseAnimations = new string[]{  "idle" },
                                    AdditionalAnimations1 = new string[]{ "look" },
                                    AdditionalAnimations2 = new string[]{ "eat" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle }
                            },
                            new AnimConditionInfo() 
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ 
                                    BaseAnimations = new string[]{  "idle" },
                                    AdditionalAnimations2 = new string[]{ "eat" }
                                },
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Eating }
                            },
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{ "dying" }},
                          //      Sound = "aliens/spacechicken",  bug -never stops
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying }
                            },

                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "dead" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Post)}  
                            },
                        }
                        }
                    },
                    LocomotorType = new LocomotorType()
                    {

                        MaxAngularSpeed = 1.5f * MathHelper.Pi, // .6f * MathHelper.Pi,                    
                        MeleeRadius = 10f,

                        LeggedLocomotorType = new LeggedLocomotorType()
                        {
                            TerrainNegateFactor = 0.7f,
                            WalkSlowSpeed = 30f, // 20f, 
                            WalkNormalSpeed = 43f /*debug only*/, //25f   18f /*proper speed*/, patrician:26f, turnip:11f
                            WalkFastSpeed = 64f 
                        },
                        CollisionResponderType = new CollisionResponderType()
                        {
                            AgentCollisionResponderType = new AgentCollisionResponderType()
                        }
                    },
                    SensorType = new SensorType()
                    {
                        Range = sensorRangeHuman + 4,  // + 50
                        RangeAtNight = sensorRangeHumanNight,
                        DetectionTypeKey = "defaultDetection"
                    },

                    BodyType = GameData.Instance.AllBodyTypes["thunderchicken"],
                    ContainerType = new AgentStorageType() //0.5f)
                    {
                        ItemStorageType = new ItemStorageType(0.5f), // can Haul... later.     MP: if you remove this line you get an error june 24 2014
                        StomachStorageType = new ItemStorageType(0.07f) // stomach size is dynamic - is defined in BioType
                        
                    }
                };


                thunderchickenType.IntelligenceType = new IntelligenceType()
                {
                    IsMobile = true,
                    CanAttack = false,
                    CanUseWeapons = false,
                    CanHunt = false,
                    CanScout = true,
                    CanExamine = true, 
                    CanPatrol = false,
                    CanHaul = false,
                    IsPredator = false,  ///
                    ContainerTransactTag = "chickenTransact", //was "chicken"
                    StrengthRating = Entities.StrengthRating.WeakerThanHumans,   //
                    InterestInTriggerTypes = new[] { "mineTrigger", "smallImprovisedTrapTrigger", "spikeTrapTrigger" },
                    Courage = 0f,
                    MemoryInDays = 0.05f, //mp 0.2f april 2015 was 3f. wanted them to be more mobile but cannot really see a difference.
                    Boldness = 1f, // MP may 6 2014 was 0.2f. I increased it to 1f to make sure they are never in cautious stance, because cautious right now colors their whole world yellow, causing them to freeze when a human apporaches , and all the time run from twinklers. Can be lowered once the strengthratings are adjusted - the goal should be not to have MASSIVE big threatradiusses which cover the whole map, because this causes freezing.
                    AggroRange = 0.0f,

                };


                thunderchickenType.BiologicalType = new BiologicalType()
                {
                    OxygenAndMuscleEnergyIncreaseRatePerDay = 10f,
        //          FoodItemTagsThatCanBeConsumed = new[] { "inedibleVegi", "edibleVegi" }, // Crash
                    TimeToConsumeFullMealInDays = 0.0125f,
                    StomachSizeFractionOfEntityBulk = 0.2f,
                    StomachContentsDecreaseRatePerDay = 3,
                    ActiveStealthRating = 0.1f,                   
                    MaxRegainLimit = humanMaxRegainLimit,
                    FractionOfMaxHitpointsGainedPerDay = humanFractionOfMaxHitpointsGainedPerDay,
                    Carcass = "item:thunderChickenCarcass",
                    Castes = new List<CasteType>() 
                { 
                    ///////// MALE
                    new CasteType() { 
                        KeyName = "male",
                        Reproduction = Reproduction.Male, Edge = 0.51f,  
                        HeightMean = 0.5f, HeightStandardDeviation = 0.02f, 
                        WeightMean = 30f, WeightStandardDeviation = 3f,  // multiply WeightStandardDeviation by 3 and subtract that number from WeightMean, this gives the minimum Weight in kilos. divide by 100 to get the bulk...the minimum possible bulk.  

                        AgeGroupTypes = new List<AgeGroupType>() 
                        { 
                            new AgeGroupType() 
                            { 
                                Name = "Chick", AIAgeGroup = AIAgeGroup.Baby, CanReproduce = false, Edge = .5f, HeightTargetModifier = 0.05f, WeightTargetModifier = 0.03f, ModelScaleFraction = 0.5f,   // these modifiers are not used as of now, we think . MP
                                /*,  NeedTypes = new NeedType[]{ babySleepNeed }*/
                                NeedTypes = new []{ thunderChickenFoodNeed } 
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Chick", AIAgeGroup = AIAgeGroup.Child, CanReproduce = false, Edge = 0.8f, HeightTargetModifier = 0.6f, WeightTargetModifier = 0.5f, ModelScaleFraction = 0.6f,
                                NeedTypes = new []{ thunderChickenFoodNeed }
                                //, NeedTypes = new NeedType[]{ childSleepNeed }
                            },
                            new AgeGroupType() 
                            { 
                                Name = "Grown", AIAgeGroup = AIAgeGroup.YoungAdult, CanReproduce = false, Edge = 2f, HeightTargetModifier = 0.97f, WeightTargetModifier = 0.92f, ModelScaleFraction = 0.85f,
                                NeedTypes = new []{ thunderChickenFoodNeed }
                                //  , NeedTypes = new NeedType[]{ youngAdultSleepNeed }
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Grown", AIAgeGroup = AIAgeGroup.Adult, CanReproduce = true, Edge = 8f, HeightTargetModifier = 1f, WeightTargetModifier = 1f, ModelScaleFraction = 1f,
                                NeedTypes = new []{ thunderChickenFoodNeed }
                                //   , NeedTypes = new NeedType[]{ adultSleepNeed }
                            }
                            ,
                            new AgeGroupType() 
                            { 
                                Name = "Grown", AIAgeGroup = AIAgeGroup.Old, CanReproduce = true, Edge = 9f, HeightTargetModifier = 0.9f, WeightTargetModifier = 1f, ModelScaleFraction = 1f,
                                NeedTypes = new []{ thunderChickenFoodNeed }
                                //  , NeedTypes = new NeedType[]{ oldSleepNeed }
                            }
                        } 
                    },
                   /////////FEMALE
                    new CasteType() { 
                        KeyName = "female",
                        Reproduction = Reproduction.Female, Edge = 1f, 
                        HeightMean = 0.45f, HeightStandardDeviation = 0.02f, 
                        WeightMean = 28f, WeightStandardDeviation = 2f,  // multiply WeightStandardDeviation by 3 and subtract that number from WeightMean, this gives the minimum Weight in kilos. divide by 100 to get the bulk...the minimum possible bulk.  

                        AgeGroupTypes = new List<AgeGroupType>() 
                        { 
                            new AgeGroupType() 
                            { 
                                Name = "Chick", AIAgeGroup = AIAgeGroup.Baby, CanReproduce = false, Edge = .5f, HeightTargetModifier = 0.05f, WeightTargetModifier = 0.03f, ModelScaleFraction = 0.3f,  // these modifiers are not used as of now, we think . MP
                                NeedTypes = new []{ thunderChickenFoodNeed }
                                /*,  NeedTypes = new NeedType[]{ babySleepNeed }*/
                                
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Chick", AIAgeGroup = AIAgeGroup.Child, CanReproduce = false, Edge = 0.8f, HeightTargetModifier = 0.6f, WeightTargetModifier = 0.5f, ModelScaleFraction = 0.6f,
                                NeedTypes = new []{ thunderChickenFoodNeed }
                                //  , NeedTypes = new NeedType[]{ childSleepNeed }
                            },
                            new AgeGroupType() 
                            { 
                                Name = "Grown", AIAgeGroup = AIAgeGroup.YoungAdult, CanReproduce = false, Edge = 2f, HeightTargetModifier = 0.97f, WeightTargetModifier = 0.92f, ModelScaleFraction = 0.85f,
                                NeedTypes = new []{ thunderChickenFoodNeed }
                                //     , NeedTypes = new NeedType[]{ youngAdultSleepNeed }
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Grown", AIAgeGroup = AIAgeGroup.Adult, CanReproduce = true, Edge = 8f, HeightTargetModifier = 1f, WeightTargetModifier = 1f, ModelScaleFraction = 1f,
                                NeedTypes = new []{ thunderChickenFoodNeed }
                                //    , NeedTypes = new NeedType[]{ adultSleepNeed }
                            }
                            ,
                            new AgeGroupType() 
                            { 
                                Name = "Grown", AIAgeGroup = AIAgeGroup.Old, CanReproduce = false, Edge = 9f, HeightTargetModifier = 0.9f, WeightTargetModifier = 1f, ModelScaleFraction = 1f,
                                NeedTypes = new []{ thunderChickenFoodNeed }
                                //   , NeedTypes = new NeedType[]{ oldSleepNeed }
                            }
                        } 
                    } 
                }

                };

                listOfEntityTypes.Add(thunderchickenType);
                #endregion
                ////////thin thunderchicken species. cannot be a caste variation because it has different eating anim:
                #region bajingan (scavenger/varmint. thin ThunderChicken.)
                thunderchickenType = new EntityType("entity:bajingan")
                {
                    Name = "Bajingan", //indonesian: rogue, son of a bitch, crook, scoundrel, rascal, blackguard
                    ThumbnailSmall = "HUD_thumbnail_thunderChickenThin",
                    SummaryDescription = "Opportunistic scavenger (vermin). Is edible.",  // 
                    Description = "\n FEEDING CLASSIFICATION: Omnivore.\n \n HEIGHT: up to 40 cm\n \n ANATOMY\n Lithe species of thunder chicken. Feeds using its mouth (instead of tentacles), which makes it able to feed at a much quicker rate than other thunder chickens. \n \n BEHAVIOR\n  Despite its size, this scavenger has a voracious appetite no doubt owing to its very high metabolism. \n \n THREAT LEVEL\n None.\n \n SURVIVAL GUIDE NOTES\n Keep food supplies hidden if camping in this animal's habitat.",
                
                    RenderableType = new RenderableType()
                    {
                        RenderAsModelType = new RenderAsModelType()
                        {

                            ModelScale = 1.25f,// priority goes: race ModelScale / cast ModelScale / default (renderAdModelType ModelScale)
                            AssetName = "thunderChickenThin",
                            ModelBasicTextureName = "ThunderchickenThinTexture1",
                            GaitAnimations = new XmlDictionary<string, GaitAnimationBracket[]>
                       {
                        
                        { "normal", new[]{                                           
                                              new GaitAnimationBracket(){ MinimumSpeed = 0f, MaximumSpeed = 77f, StrideLength = 11f /* measured?: 18.1f*/, StrideDuration = 0.4f, AnimationKey = "gaitWalk"},       //MP: iterated strideduration 17th dec 2013.   ... was: MinimumSpeed = 0f, MaximumSpeed = 77f, StrideLength = 11f , StrideDuration = 0.2f, AnimationKey = "gaitWalk"                                     
                                              } }
                       },

                            AnimConditions = new AnimConditionInfo[] 
                        {
                            new AnimConditionInfo()
                            {
                               SoundAndAnimationSet = new RandomSoundAndAnimationSet()
                               {
                                    AdditionalAnimations1 = new string[]{ "hidden" }
                                },
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Moving },
                                GaitSetKey = "normal"
                            }, 
                            new AnimConditionInfo() 
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ 
                                    BaseAnimations = new string[]{  "idle" },
                                    AdditionalAnimations1 = new string[]{ "look" }, 
                                                                                    }, 
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle }
                            },
                            new AnimConditionInfo() 
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ 
                                    BaseAnimations = new string[]{  "eatNoTentacle" }   //difference from regular thunderchick
                                },
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Eating }
                            },
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{ "dying" }},
                          //      Sound = "aliens/spacechicken",  bug -never stops
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying }
                            },

                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "dead" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Post)}  
                            },
                        }
                        }
                    },
                    LocomotorType = new LocomotorType()
                    {

                        MaxAngularSpeed = 1.5f * MathHelper.Pi, // .6f * MathHelper.Pi,                    
                        MeleeRadius = 10f,

                        LeggedLocomotorType = new LeggedLocomotorType()
                        {
                            TerrainNegateFactor = 0.7f,
                            WalkSlowSpeed = 30f, // 20f, 
                            WalkNormalSpeed = 43f /*debug only*/, //25f   18f /*proper speed*/, patrician:26f, turnip:11f
                            WalkFastSpeed = 64f, //24f 18f,//30f, // 63f,


                        },
                        CollisionResponderType = new CollisionResponderType()
                        {
                            AgentCollisionResponderType = new AgentCollisionResponderType()
                        }
                    },
                    SensorType = new SensorType()
                    {
                        Range = sensorRangeHuman + 4,  // + 50
                        RangeAtNight = sensorRangeHumanNight,
                        DetectionTypeKey = "defaultDetection"
                    },

                    BodyType = GameData.Instance.AllBodyTypes["thunderchicken"],
                    ContainerType = new AgentStorageType() //0.5f)
                    {
                        ItemStorageType = new ItemStorageType(0.5f), // can Haul... later.     MP: if you remove this line you get an error june 24 2014
                        StomachStorageType = new ItemStorageType(0.07f) // stomach size is dynamic - is defined in BioType
                        
                    }
                };


                thunderchickenType.IntelligenceType = new IntelligenceType()
                {
                    IsMobile = true,
                    CanAttack = false,
                    CanUseWeapons = false,
                    CanHunt = false,
                    CanScout = true,
                    CanExamine = true, 
                    CanPatrol = false,
                    CanHaul = false,
                    ContainerTransactTag = "chickenTransact", //
                    StrengthRating = Entities.StrengthRating.None,   //
                    InterestInTriggerTypes = new[] { "mineTrigger", "smallImprovisedTrapTrigger", "spikeTrapTrigger", "animalMigrateTrigger" }, //"animalMigrateTrigger" used for migration
                    Courage = 0f,
                    IsPredator = false,  ///
                    MemoryInDays = 0.3f, //mp this animal is not timid, so it doesnt get paralyzed from big threat areas. therefore its memory can be longer 
                    Boldness = 1.49f, // 
                    AggroRange = 0.0f

                };


                thunderchickenType.BiologicalType = new BiologicalType()
                {
                    OxygenAndMuscleEnergyIncreaseRatePerDay = 10f,
                    FoodItemTagsThatCanBeConsumed = new[] { "cookedMeat", "rawMeat", "smallRawMeat", "rottenMeat", "inedibleMeat", "edibleVegi", "inedibleVegi", },
                    ExtractionProcessTypes = new[] { "extractMudWormMeat", "extractPatricianMeat", "extractLeafCutterMeat", "extractThunderChickenMeat", "extractBinalRatMeat", "extractTwinklerMeat", "extractBushDragonMeat", "extractSwampDemonTreeMeat", "extractDemonTreeMeat", "extractMegapodMeat", "extractWhipjawMeat", "extractSpikePlantMeat", "extractForestGuardianMeat", },
                    TimeToConsumeFullMealInDays = 0.0125f, // bso changed from 0.05 to 0.0125(20sec)
                    StomachSizeFractionOfEntityBulk = 0.2f,
                    StomachContentsDecreaseRatePerDay = 3,
                    ActiveStealthRating = 0.1f,
                    IsVermin = true,//scavenger vermin
                    MaxRegainLimit = humanMaxRegainLimit,
                    FractionOfMaxHitpointsGainedPerDay = humanFractionOfMaxHitpointsGainedPerDay,
                    Carcass = "item:thunderChickenCarcass",
                    Castes = new List<CasteType>() 
                { 
                    ///////// MALE
                    new CasteType() { 
                        KeyName = "male",
                        Reproduction = Reproduction.Male, Edge = 0.51f,  
                        HeightMean = 0.5f, HeightStandardDeviation = 0.02f, 
                        WeightMean = 30f, WeightStandardDeviation = 3f,  // multiply WeightStandardDeviation by 3 and subtract that number from WeightMean, this gives the minimum Weight in kilos. divide by 100 to get the bulk...the minimum possible bulk.  

                        AgeGroupTypes = new List<AgeGroupType>() 
                        { 
                            new AgeGroupType() 
                            { 
                                Name = "Chick", AIAgeGroup = AIAgeGroup.Baby, CanReproduce = false, Edge = .5f, HeightTargetModifier = 0.05f, WeightTargetModifier = 0.03f, ModelScaleFraction = 0.5f,   // these modifiers are not used as of now, we think . MP
                                /*,  NeedTypes = new NeedType[]{ babySleepNeed }*/
                                NeedTypes = new []{ thunderChickenFoodNeed } 
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Chick", AIAgeGroup = AIAgeGroup.Child, CanReproduce = false, Edge = 1f, HeightTargetModifier = 0.6f, WeightTargetModifier = 0.5f, ModelScaleFraction = 0.6f,
                                NeedTypes = new []{ thunderChickenFoodNeed }
                                //, NeedTypes = new NeedType[]{ childSleepNeed }
                            },
                            new AgeGroupType() 
                            { 
                                Name = "Grown", AIAgeGroup = AIAgeGroup.YoungAdult, CanReproduce = false, Edge = 4f, HeightTargetModifier = 0.97f, WeightTargetModifier = 0.92f, ModelScaleFraction = 0.85f,
                                NeedTypes = new []{ thunderChickenFoodNeed }
                                //  , NeedTypes = new NeedType[]{ youngAdultSleepNeed }
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Grown", AIAgeGroup = AIAgeGroup.Adult, CanReproduce = true, Edge = 12f, HeightTargetModifier = 1f, WeightTargetModifier = 1f, ModelScaleFraction = 1f,
                                NeedTypes = new []{ thunderChickenFoodNeed }
                                //   , NeedTypes = new NeedType[]{ adultSleepNeed }
                            }
                            ,
                            new AgeGroupType() 
                            { 
                                Name = "Grown", AIAgeGroup = AIAgeGroup.Old, CanReproduce = true, Edge = 14f, HeightTargetModifier = 0.9f, WeightTargetModifier = 1f, ModelScaleFraction = 1f,
                                NeedTypes = new []{ thunderChickenFoodNeed }
                                //  , NeedTypes = new NeedType[]{ oldSleepNeed }
                            }
                        } 
                    },
                   /////////FEMALE
                    new CasteType() { 
                        KeyName = "female",
                        Reproduction = Reproduction.Female, Edge = 1f, 
                        HeightMean = 0.45f, HeightStandardDeviation = 0.02f, 
                        WeightMean = 28f, WeightStandardDeviation = 2f,  // multiply WeightStandardDeviation by 3 and subtract that number from WeightMean, this gives the minimum Weight in kilos. divide by 100 to get the bulk...the minimum possible bulk.  

                        AgeGroupTypes = new List<AgeGroupType>() 
                        { 
                            new AgeGroupType() 
                            { 
                                Name = "Chick", AIAgeGroup = AIAgeGroup.Baby, CanReproduce = false, Edge = .5f, HeightTargetModifier = 0.05f, WeightTargetModifier = 0.03f, ModelScaleFraction = 0.3f,  // these modifiers are not used as of now, we think . MP
                                NeedTypes = new []{ thunderChickenFoodNeed }
                                /*,  NeedTypes = new NeedType[]{ babySleepNeed }*/
                                
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Chick", AIAgeGroup = AIAgeGroup.Child, CanReproduce = false, Edge = 1f, HeightTargetModifier = 0.6f, WeightTargetModifier = 0.5f, ModelScaleFraction = 0.6f,
                                NeedTypes = new []{ thunderChickenFoodNeed }
                                //  , NeedTypes = new NeedType[]{ childSleepNeed }
                            },
                            new AgeGroupType() 
                            { 
                                Name = "Grown", AIAgeGroup = AIAgeGroup.YoungAdult, CanReproduce = false, Edge = 4f, HeightTargetModifier = 0.97f, WeightTargetModifier = 0.92f, ModelScaleFraction = 0.85f,
                                NeedTypes = new []{ thunderChickenFoodNeed }
                                //     , NeedTypes = new NeedType[]{ youngAdultSleepNeed }
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Grown", AIAgeGroup = AIAgeGroup.Adult, CanReproduce = true, Edge = 12f, HeightTargetModifier = 1f, WeightTargetModifier = 1f, ModelScaleFraction = 1f,
                                NeedTypes = new []{ thunderChickenFoodNeed }
                                //    , NeedTypes = new NeedType[]{ adultSleepNeed }
                            }
                            ,
                            new AgeGroupType() 
                            { 
                                Name = "Grown", AIAgeGroup = AIAgeGroup.Old, CanReproduce = false, Edge = 14f, HeightTargetModifier = 0.9f, WeightTargetModifier = 1f, ModelScaleFraction = 1f,
                                NeedTypes = new []{ thunderChickenFoodNeed }
                                //   , NeedTypes = new NeedType[]{ oldSleepNeed }
                            }
                        } 
                    } 
                }

                };

                listOfEntityTypes.Add(thunderchickenType);
                #endregion

                #region bulkyThunderchicken
            /*
             * !!WARNING!! this is a copy of pygmyThunderChicken
             * Only model and texture have been changed
             */
                thunderchickenType = new EntityType("entity:studdedThunderChicken")
                {
                    Name = "Studded thunder chicken",
                    ThumbnailSmall = "HUD_thumbnail_thunderChickenBulky",
                    SummaryDescription = "Lightly armored herbivore",  //
                    Description = "\n FEEDING CLASSIFICATION: Herbivore.\n \n HEIGHT: up to 70 cm\n \n ANATOMY\n Species of thunder chicken with a sturdier build and some armor. Feeds using retractable tentacles around its mouth.\n \nBEHAVIOR\n  Despite its protective armor, it is no less timid than other thunder chickens. Lives a mostly solitary life.\n \n THREAT LEVEL\n None.\n \n ENEMIES\n Preyed on by spoak dendronts and several other species.\n \n NOTES\n Edible by humans.", 
                   
                    RenderableType = new RenderableType()
                    {
                        RenderAsModelType = new RenderAsModelType()
                        {
                            ModelScale = 1.25f,// priority goes: race ModelScale / cast ModelScale / default (renderAdModelType ModelScale)
                            AssetName = "thunderChickenBulky",
                            ModelBasicTextureName = "ThunderchickenBulkyTexture1",
                            GaitAnimations = new XmlDictionary<string, GaitAnimationBracket[]>
                            {
                                { "normal", new[]{new GaitAnimationBracket(){ MinimumSpeed = 0f, MaximumSpeed = 77f, StrideLength = 11f /* measured?: 18.1f*/, StrideDuration = 0.4f, AnimationKey = "gaitWalk"},       //MP: iterated strideduration 17th dec 2013.   ... was: MinimumSpeed = 0f, MaximumSpeed = 77f, StrideLength = 11f , StrideDuration = 0.2f, AnimationKey = "gaitWalk"                                     
                                                      } }
                            },

                            AnimConditions = new AnimConditionInfo[] 
                        {
                            new AnimConditionInfo()
                            {
                               SoundAndAnimationSet = new RandomSoundAndAnimationSet()
                               {
                                    AdditionalAnimations1 = new string[]{ "hidden" }
                                },
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Moving },
                                GaitSetKey = "normal"
                            },
 
               /*           new AnimConditionInfo()                 //commented out by MP when putting in new gait data. 6 sep 2013
                            {
                                AnimationSet = new RandomAnimationSet(){ 
                                    BaseAnimations = new string[]{  "walk"},
                                    AdditionalAnimations1 = new string[]{ "hidden" }},

                                Conditions = new ConditionSet(){ Modifiers = new BitMask64(typeof(AnimState), (int)AnimState.Moving )                               
                            },*/
                            new AnimConditionInfo() 
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet()
                                { 
                                    BaseAnimations = new string[]{  "idle" },
                                  //  AdditionalAnimations1 = new string[]{ "look" }, //additionalAnimtions here overwrites walk - this is a workaround 
                                  //  AdditionalAnimations2 = new string[]{ "eat" } //TODO:find bug
                                },
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle }
                            },
                            new AnimConditionInfo() 
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ 
                                    BaseAnimations = new string[]{  "idle" },
                                    AdditionalAnimations2 = new string[]{ "eat" }
                                },
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Eating }
                            },
                             new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{ "dying" }, Sounds = new[]{ "aliens/spacechicken" }},                             
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying, Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Pre) }, 
                                Looping = Looping.No
                            },
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{ "dying" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying }
                            },
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "dead" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Post)}  
                            },
                        }
                        }
                    },
                    LocomotorType = new LocomotorType()
                    {
                        MaxAngularSpeed = 1.5f * MathHelper.Pi, // .6f * MathHelper.Pi,                    
                        MeleeRadius = 10f,
                        LeggedLocomotorType = new LeggedLocomotorType()
                        {
                            TerrainNegateFactor = 0.7f,
                            WalkSlowSpeed = 30f, // 20f, 
                            WalkNormalSpeed = 43f /*debug only*/, //25f   18f /*proper speed*/, patrician:26f, turnip:11f
                            WalkFastSpeed = 64f, //24f 18f,//30f, // 63f,
                        },
                        CollisionResponderType = new CollisionResponderType()
                        {
                            AgentCollisionResponderType = new AgentCollisionResponderType()
                        }
                    },
                    SensorType = new SensorType()
                    {
                        Range = sensorRangeHuman + 4,  // + 50
                        RangeAtNight = sensorRangeHumanNight,
                        DetectionTypeKey = "defaultDetection"
                    },
                    BodyType = GameData.Instance.AllBodyTypes["thunderchicken"],
                    ContainerType = new AgentStorageType() //0.5f)
                    {
                        ItemStorageType = new ItemStorageType(0.5f), // can Haul... later.     MP: if you remove this line you get an error june 24 2014
                        StomachStorageType = new ItemStorageType(0.07f) // stomach size is dynamic - is defined in BioType                        
                    }
                };

                thunderchickenType.IntelligenceType = new IntelligenceType()
                {
                    IsMobile = true,
                    CanAttack = false,
                    CanUseWeapons = false,
                    CanHunt = false,
                    CanScout = true,
                    CanExamine = true, 
                    CanPatrol = false,
                    CanHaul = false,
                    IsPredator = false,  ///
                    ContainerTransactTag = "chickenTransact", //was "chicken"
                    StrengthRating = Entities.StrengthRating.WeakerThanHumans,   //
                    InterestInTriggerTypes = new[] { "mineTrigger", "smallImprovisedTrapTrigger", "spikeTrapTrigger" },
                    Courage = 0f,
                    MemoryInDays = 0.2f, //mp april 2015 was 3f. wanted them to be more mobile but cannot really see a difference.
                    Boldness = 1f, // MP may 6 2014 was 0.2f. I increased it to 1f to make sure they are never in cautious stance, because cautious right now colors their whole world yellow, causing them to freeze when a human apporaches , and all the time run from twinklers. Can be lowered once the strengthratings are adjusted - the goal should be not to have MASSIVE big threatradiusses which cover the whole map, because this causes freezing.
                    AggroRange = 0.0f,
                };

                thunderchickenType.BiologicalType = new BiologicalType()
                {
                    OxygenAndMuscleEnergyIncreaseRatePerDay = 10f,
                    //          FoodItemTagsThatCanBeConsumed = new[] { "inedibleVegi", "edibleVegi" }, // Crash
                    TimeToConsumeFullMealInDays = 0.0125f,
                    StomachSizeFractionOfEntityBulk = 0.2f,
                    StomachContentsDecreaseRatePerDay = 3,
                    ActiveStealthRating = 0.1f,
                    MaxRegainLimit = humanMaxRegainLimit,
                    FractionOfMaxHitpointsGainedPerDay = humanFractionOfMaxHitpointsGainedPerDay,
                    Carcass = "item:thunderChickenCarcass",
                    Castes = new List<CasteType>() 
                { 
                    ///////// MALE
                    new CasteType() { 
                        KeyName = "male",
                        Reproduction = Reproduction.Male, Edge = 0.51f,  
                        HeightMean = 0.5f, HeightStandardDeviation = 0.02f, 
                        WeightMean = 30f, WeightStandardDeviation = 3f,  // multiply WeightStandardDeviation by 3 and subtract that number from WeightMean, this gives the minimum Weight in kilos. divide by 100 to get the bulk...the minimum possible bulk.  

                        AgeGroupTypes = new List<AgeGroupType>() 
                        { 
                            new AgeGroupType() 
                            { 
                                Name = "Chick", AIAgeGroup = AIAgeGroup.Baby, CanReproduce = false, Edge = .5f, HeightTargetModifier = 0.05f, WeightTargetModifier = 0.03f, ModelScaleFraction = 0.5f,   // these modifiers are not used as of now, we think . MP
                                /*,  NeedTypes = new NeedType[]{ babySleepNeed }*/
                                NeedTypes = new []{ thunderChickenFoodNeed } 
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Chick", AIAgeGroup = AIAgeGroup.Child, CanReproduce = false, Edge = 1f, HeightTargetModifier = 0.6f, WeightTargetModifier = 0.5f, ModelScaleFraction = 0.6f,
                                NeedTypes = new []{ thunderChickenFoodNeed }
                                //, NeedTypes = new NeedType[]{ childSleepNeed }
                            },
                            new AgeGroupType() 
                            { 
                                Name = "Grown", AIAgeGroup = AIAgeGroup.YoungAdult, CanReproduce = false, Edge = 4f, HeightTargetModifier = 0.97f, WeightTargetModifier = 0.92f, ModelScaleFraction = 0.85f,
                                NeedTypes = new []{ thunderChickenFoodNeed }
                                //  , NeedTypes = new NeedType[]{ youngAdultSleepNeed }
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Grown", AIAgeGroup = AIAgeGroup.Adult, CanReproduce = true, Edge = 12f, HeightTargetModifier = 1f, WeightTargetModifier = 1f, ModelScaleFraction = 1f,
                                NeedTypes = new []{ thunderChickenFoodNeed }
                                //   , NeedTypes = new NeedType[]{ adultSleepNeed }
                            }
                            ,
                            new AgeGroupType() 
                            { 
                                Name = "Grown", AIAgeGroup = AIAgeGroup.Old, CanReproduce = true, Edge = 14f, HeightTargetModifier = 0.9f, WeightTargetModifier = 1f, ModelScaleFraction = 1f,
                                NeedTypes = new []{ thunderChickenFoodNeed }
                                //  , NeedTypes = new NeedType[]{ oldSleepNeed }
                            }
                        } 
                    },
                   /////////FEMALE
                    new CasteType() { 
                        KeyName = "female",
                        Reproduction = Reproduction.Female, Edge = 1f, 
                        HeightMean = 0.45f, HeightStandardDeviation = 0.02f, 
                        WeightMean = 28f, WeightStandardDeviation = 2f,  // multiply WeightStandardDeviation by 3 and subtract that number from WeightMean, this gives the minimum Weight in kilos. divide by 100 to get the bulk...the minimum possible bulk.  

                        AgeGroupTypes = new List<AgeGroupType>() 
                        { 
                            new AgeGroupType() 
                            { 
                                Name = "Chick", AIAgeGroup = AIAgeGroup.Baby, CanReproduce = false, Edge = .5f, HeightTargetModifier = 0.05f, WeightTargetModifier = 0.03f, ModelScaleFraction = 0.3f,  // these modifiers are not used as of now, we think . MP
                                NeedTypes = new []{ thunderChickenFoodNeed }
                                /*,  NeedTypes = new NeedType[]{ babySleepNeed }*/
                                
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Chick", AIAgeGroup = AIAgeGroup.Child, CanReproduce = false, Edge = 1f, HeightTargetModifier = 0.6f, WeightTargetModifier = 0.5f, ModelScaleFraction = 0.6f,
                                NeedTypes = new []{ thunderChickenFoodNeed }
                                //  , NeedTypes = new NeedType[]{ childSleepNeed }
                            },
                            new AgeGroupType() 
                            { 
                                Name = "Grown", AIAgeGroup = AIAgeGroup.YoungAdult, CanReproduce = false, Edge = 4f, HeightTargetModifier = 0.97f, WeightTargetModifier = 0.92f, ModelScaleFraction = 0.85f,
                                NeedTypes = new []{ thunderChickenFoodNeed }
                                //     , NeedTypes = new NeedType[]{ youngAdultSleepNeed }
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Grown", AIAgeGroup = AIAgeGroup.Adult, CanReproduce = true, Edge = 12f, HeightTargetModifier = 1f, WeightTargetModifier = 1f, ModelScaleFraction = 1f,
                                NeedTypes = new []{ thunderChickenFoodNeed }
                                //    , NeedTypes = new NeedType[]{ adultSleepNeed }
                            }
                            ,
                            new AgeGroupType() 
                            { 
                                Name = "Grown", AIAgeGroup = AIAgeGroup.Old, CanReproduce = false, Edge = 14f, HeightTargetModifier = 0.9f, WeightTargetModifier = 1f, ModelScaleFraction = 1f,
                                NeedTypes = new []{ thunderChickenFoodNeed }
                                //   , NeedTypes = new NeedType[]{ oldSleepNeed }
                            }
                        } 
                    } 
                }

                };

                listOfEntityTypes.Add(thunderchickenType);
                #endregion


                #endregion

                #region Domesticated Twinkler
                bodyType = GameData.Instance.AllBodyTypes["twinkler"];  //needs species entity type set up MP mar 2014
                listOfEntityTypes.Add(new EntityType("entity:domesticatedTwinkler") //"entity:dog"
                {
                    Name = "Domesticated Twinkler", 
                    ThumbnailSmall = "HUD_thumbnail_twinkler",
                    SummaryDescription = "Domesticated animal",
                    Description = "",

                    RenderableType = new RenderableType()
                    {
                        RenderAsModelType = new RenderAsModelType()
                        {
                            ModelScale = 2.5f, //normal size:2.5f, small size:1.5f   //per Dec 2012, this is overwritten by any data put in age, race or caste, in that order
                            AssetName = "twinkler",
                            GaitAnimations = new XmlDictionary<string, GaitAnimationBracket[]>
                            {                        
                                { "normal", new[]
                                    {                                           
                                        new GaitAnimationBracket(){ MinimumSpeed = 35f, MaximumSpeed = 77f, StrideLength = 27f, StrideDuration = 0.9f, AnimationKey = "gaitWalk"},  //MP: made a quick iteration on strideduration 17th dec 2013.   values were: :MinimumSpeed = 35f, MaximumSpeed = 77f, StrideLength = 27f, StrideDuration = 0.46f,
                                    } 
                                }
                           },

                            AnimConditions = new AnimConditionInfo[] 
                        {
                            new AnimConditionInfo()
                            {
                              
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Moving},
                                GaitSetKey = "normal"
                            },
                                           
                            new AnimConditionInfo() 
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "idle" }},
                           //     Sound = "aliens/croaker",   error...gets played a lot when they are dead....18th oct 2013
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle }           
                            },
                             new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "attackLowRight" }}, 
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Attacking,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Low, (int)AnimModifier.Right)},
                                //Forbiddens = new BitMask64(typeof(AnimState), (int)AnimState.Moving)
  
                            },
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "attackHighRight" }}, 
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Attacking,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.High, (int)AnimModifier.Right)},
                                //Forbiddens = new BitMask64(typeof(AnimState), (int)AnimState.Moving)  
                            },
                             new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "hit" }, Sounds = new string[]{ "aliens/hummingClickClacking" }},                              
                                Playback = Playback.Manual, 
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Recoiling }
                            },

                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{ "dying" }},
                           //     Sound = "aliens/toothCrickets",  bug -never stops ..21 oct
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying }
                            },

                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "dead" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Post)}  
                            },
                             
                        }
                        }
                    },
                    LocomotorType = new LocomotorType()
                    {

                        MaxAngularSpeed = .25f * MathHelper.Pi, // MathHelper.Pi,
                        FourSidedSymmetry = true,
                        MeleeRadius = 12f,

                        LeggedLocomotorType = new LeggedLocomotorType()
                        {
                            TerrainNegateFactor = 0.3f,
                            WalkSlowSpeed = 20f, // 8f, 
                            WalkNormalSpeed = 22f /*debug only*/,  /*proper speed*///, ModelScale = 2.5f : (NORMAL twinkler walk speed=1.0, basespeed=25f ) (SLOW twinkler walk speed=0.7, basespeed=21f) (FAST twinkler walk speed=2, basespeed=75f) 
                            //, ModelScale = 1.5f : (NORMAL twinkler walk speed=1.5, basespeed=21f )
                            WalkFastSpeed = 24f, // 18f,//30f, // 63f,
                        },
                        CollisionResponderType = new CollisionResponderType()
                        {
                            AgentCollisionResponderType = new AgentCollisionResponderType()
                        }
                    },
                    SensorType = new SensorType()
                    {
                        Range = sensorRangeHuman, 
                        RangeAtNight = sensorRangeHuman, 
                        DetectionTypeKey = "defaultDetection"
                    },

                    BodyType = bodyType,
                    ContainerType = new AgentStorageType() //0.5f)
                    {
                        ItemStorageType = new ItemStorageType(0.5f), // can Haul... later.
                        StomachStorageType = new ItemStorageType(0.07f) // stomach size is dynamic - is defined in BioType                        
                    },
                IntelligenceType = new IntelligenceType()
                {
                    MembersScoutingFraction = 1f, //dec 3, was 1f
                    IsMobile = true,
                    CanAttack = true,
                    CanUseWeapons = false,
                    CanHunt = false,
                    CanScout = true,
                    CanExamine = true, 
                    CanPatrol = true,
                    CanHaul = false,
                    IsPredator = true,
                    ContainerTransactTag = "twinklerTransact", //was "twinkler"
                    StrengthRating = Entities.StrengthRating.WeakerThanHumans,
                    Boldness = 0.25f, //Boldness is low at the moment to allow baby and child twinklersd to flee, the bigger ones should still be able to attack threats //1f
                    Courage = 0.05f,   //0.1f 0.2f   0.9f  // 0.05f . wanted them to flee more. 0.05f didn't make them flee either...tweak also vitalBodyPartDamageEvaluationBoost in constants.cs
                    MemoryInDays = 2f,
                    Attacks = new[]
                     { 
                         "twinklerLowRight",
                         "twinklerHighRight"
                         //new AttackType(){ Damage = AttackType.DamageTypes.Piercing, DamageMean=3, DamageStandardDeviation=1.5f, AnimationKey = "attackLowRight"/*, DependsOn = new BodyPartType[]{ bodyType.FindBodyPart("Right arm") }*/ },
                         //new AttackType(){ Damage = AttackType.DamageTypes.Piercing, DamageMean=4, DamageStandardDeviation=1.5f, AnimationKey = "attackHighRight"/*, DependsOn = new BodyPartType[]{ bodyType.FindBodyPart("Right arm") }*/ }
                         
                     },
                    Skills = new SerializableDictionary<string, float> 
                    { 
                        {
                           "unarmedFighting" , 0.2f
                        }
                    },
                    AggroRange = sensorRangeHuman + 100,
                    AssistanceRange = 600f // NEW!!!
                },
                BiologicalType = new BiologicalType()
                {
                    OrderKey = "quaditeOrder",
                    OxygenAndMuscleEnergyIncreaseRatePerDay = 12f,
                    FoodItemTagsThatCanBeConsumed = new[] { "cookedMeat", "rawMeat", "smallRawMeat", "spoiledMeal", "inedibleMeat", "rottenMeat" },
                    ExtractionProcessTypes = new[] { "extractMudWormMeat", "extractPatricianMeat", "extractLeafCutterMeat", "extractThunderChickenMeat", "extractBinalRatMeat", "extractBushDragonMeat", "extractSwampDemonTreeMeat", "extractDemonTreeMeat", "extractMegapodMeat", "extractWhipjawMeat", "extractSpikePlantMeat", "extractForestGuardianMeat", }, //"extractThinThunderChickenMeat",
                    TimeToConsumeFullMealInDays = 0.005f, //was 0.009f dec 2014.   was 0.015f MP
                    StomachSizeFractionOfEntityBulk = 0.3f, // big stomach so can eat quickly // 0.2f,
                    StomachContentsDecreaseRatePerDay = 3,
                    ActiveStealthRating = 0.2f,
                 
                    MaxRegainLimit = humanMaxRegainLimit,
                    FractionOfMaxHitpointsGainedPerDay = humanFractionOfMaxHitpointsGainedPerDay,
                    Carcass = "item:quaditeCarcass",

                    ResilienceMean = 7f,
                    ResilienceStandardDeviation = 0.08f,

                    BioPropertyTypes = new[]{                       
                         new BioPropertyType(){
                          KeyName = "SensorRange", //measured in pixels  (White)    //aggrorange and assistancerange can be seen in debug panel>ranges, however if very big, will not be seen. see: public void DrawRanges() for colors.
                          InterpolateSetting = BioPropertyType.Interpolate.DefaultPriority                     
                     },
                      new BioPropertyType(){
                          KeyName = "SensorRangeAtNight", //measured in pixels
                          InterpolateSetting = BioPropertyType.Interpolate.DefaultPriority                     
                     },
                     new BioPropertyType(){ 
                          KeyName = "AggroRange", //measured in pixels  (red)
                          InterpolateSetting = BioPropertyType.Interpolate.DefaultPriority                     
                     },
                     new BioPropertyType(){
                          KeyName = "AssistanceRange", //measured in pixels  (green)
                          InterpolateSetting = BioPropertyType.Interpolate.DefaultPriority                     
                     }},
                    
                    Castes = new List<CasteType>() 
                {                     
                    
                    new CasteType() {
                        KeyName = "male",
                        Reproduction = Reproduction.Male, //Edge = 1f,
                        HeightMean = .70f, HeightStandardDeviation = 0.02f, 
                        WeightMean = 25f, WeightStandardDeviation = 2f,
                        PrimaryColor = "564920".ToColorVector3(), ModelBasicTextureName = "QuaditeRedTexture", 
                        Edge = 0.6f, 
                        ModelScale=2.1f,
                                                   

                        AgeGroupTypes = new List<AgeGroupType>() 
                        { 
                            new AgeGroupType() 
                            { 
                                Name = "Puppy", AIAgeGroup = AIAgeGroup.Baby, CanReproduce = false, Edge = 0.5f, HeightTargetModifier = 0.05f, WeightTargetModifier = 0.03f/*, 
                                NeedTypes = new NeedType[]{ babySleepNeed }*/
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Puppy", AIAgeGroup = AIAgeGroup.Child, CanReproduce = false, Edge = 1f, HeightTargetModifier = 0.1f, WeightTargetModifier = 0.1f  
                             // , NeedTypes = new NeedType[]{ childSleepNeed }
                            },     
                            new AgeGroupType() 
                            { 
                                Name = "Young", AIAgeGroup = AIAgeGroup.YoungAdult, CanReproduce = true, Edge = 4f, HeightTargetModifier = 0.8f, WeightTargetModifier = 0.8f  
                             // , NeedTypes = new NeedType[]{ childSleepNeed }
                            },  
                            new AgeGroupType() 
                            { 
                                Name = "Grown", AIAgeGroup = AIAgeGroup.Adult, CanReproduce = true, Edge = 14f, HeightTargetModifier = 1f, WeightTargetModifier = 1f,
                                NeedTypes = new []{ twinklerProteinNeed }
                             // , NeedTypes = new NeedType[]{ adultSleepNeed }
                            }
                            ,
                            new AgeGroupType() 
                            { 
                                Name = "Old", AIAgeGroup = AIAgeGroup.Old, CanReproduce = false, Edge = 20f, HeightTargetModifier = 0.9f, WeightTargetModifier = 1f,
                                NeedTypes = new []{ twinklerProteinNeed }
                             // , NeedTypes = new NeedType[]{ oldSleepNeed }
                            }
                        } 
                    },
                    new CasteType() {
                        KeyName = "female",
                        Reproduction = Reproduction.Female, //Edge = 1f,
                        HeightMean = .70f, HeightStandardDeviation = 0.02f, 
                        WeightMean = 25f, WeightStandardDeviation = 2f,
                        PrimaryColor = "564920".ToColorVector3(), ModelBasicTextureName = "QuaditeRedTexture", 
                        Edge = 0.6f, 
                        ModelScale=2.1f,
                                                   

                        AgeGroupTypes = new List<AgeGroupType>() 
                        { 
                            new AgeGroupType() 
                            { 
                                Name = "Puppy", AIAgeGroup = AIAgeGroup.Baby, CanReproduce = false, Edge = 0.5f, HeightTargetModifier = 0.05f, WeightTargetModifier = 0.03f/*, 
                                NeedTypes = new NeedType[]{ babySleepNeed }*/
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Puppy", AIAgeGroup = AIAgeGroup.Child, CanReproduce = false, Edge = 1f, HeightTargetModifier = 0.1f, WeightTargetModifier = 0.1f  
                             // , NeedTypes = new NeedType[]{ childSleepNeed }
                            },     
                            new AgeGroupType() 
                            { 
                                Name = "Young", AIAgeGroup = AIAgeGroup.YoungAdult, CanReproduce = true, Edge = 4f, HeightTargetModifier = 0.8f, WeightTargetModifier = 0.8f  
                             // , NeedTypes = new NeedType[]{ childSleepNeed }
                            },  
                            new AgeGroupType() 
                            { 
                                Name = "Grown", AIAgeGroup = AIAgeGroup.Adult, CanReproduce = true, Edge = 14f, HeightTargetModifier = 1f, WeightTargetModifier = 1f,
                                NeedTypes = new []{ twinklerProteinNeed }
                             // , NeedTypes = new NeedType[]{ adultSleepNeed }
                            }
                            ,
                            new AgeGroupType() 
                            { 
                                Name = "Old", AIAgeGroup = AIAgeGroup.Old, CanReproduce = false, Edge = 20f, HeightTargetModifier = 0.9f, WeightTargetModifier = 1f,
                                NeedTypes = new []{ twinklerProteinNeed }
                             // , NeedTypes = new NeedType[]{ oldSleepNeed }
                            }
                        } 
                    } 
                }

                }});

              

                #endregion


                #region Dog needs
                
                float dailyMealBulk = 0.1f;

                NeedType dogFoodEnergyNeed = new NeedType()
                {
                    KeyName = "foodEnergy", // MP this is all copied from human needs                 
                    FoodNeedType = new FoodNeedType()
                    {
                        FoodNutrient = "foodEnergy",
                        RequiredNutrientsAsFractionOfEntityBulk = 0.6f * dailyMealBulk / 1f
                    },
                    DecreasePerDay = new NormalDistribution() { Mean = 1f, StandardDeviation = 0.02f },
                    LimitForDecreasedEnergy = 0.15f,
                    DecreasedEnergyWeight = 0.6f,
                    PhysicalEffects = new PhysicalEffects()
                    {
                        DaysAtZeroCausingCollapse = 2f, // 
                        DaysAtZeroCausingDeath = 2.05f,
                        DaysAtZeroDecreaseFactor = 1f, // 0.5f,
                        UseExertionFactorToDecrease = true                        
                    },
                };

                NeedType dogAdultSleepNeed = new NeedType() // copy of human need, should dogs sleep more or less?
                {
                    KeyName = "sleep",
                    SleepNeedType = new SleepNeedType() { },
                    DecreasePerDay = new NormalDistribution() { Mean = 1f, StandardDeviation = 0.015f },
                    LimitForDecreasedEnergy = 0.4f,  
                    DecreasedEnergyWeight = 0.4f,
                    PhysicalEffects = new PhysicalEffects()
                    {
                        DaysAtZeroCausingDeath = 2f,
                        DaysAtZeroDecreaseFactor = 2f // decrease 'starvation' quickly
                    }
                };

/* MP add this under age group as well, if you know how to do it?? //mp, or, rather not. if the player has no feedback, then it's just a bug.
                NeedType dogProteinNeed = new NeedType()
                {
                    KeyName = "protein", // MP this is all copied from human needs
                    NeedClass = NeedClass.Food,
                    DecreasePerDayMean = 1f,  //0.4f
                    DecreasePerDayStandardDeviation = 0.02f,

                    LimitForDecreasedEnergy = 0.05f,
                    DecreasedEnergyWeight = 0.08f, // 0.6f,
                    PhysicalNeedType = new PhysicalNeedType()
                    {
                        LimitForReducedGrowth = 0.1f,
                        LimitForIncreasedSickness = 0.05f,
                        UseExertionFactorToDecrease = false,
                        FoodNutrient = GameData.Instance.AllFoodNutrientTypes["protein"],
                        RequiredNutrientsAsFractionOfEntityBulk = 0.012f   // answering the hover-tooltip: we assume a days' worth of recommended protein. 0,012 f bulk protein/gameday / 1f = 0,012f   ...... calculated here (under protein) https://docs.google.com/a/unclaimedworld-game.com/document/d/106BIllzcCkma24NlcWCZLwyEpHaG6AWed0GpSUUMTug/edit
                    }
                };
*/
// MP I don't add micronutrients, to keep it simple...but we could...the need would have to be low tho, because it's a carnivore.



                #endregion


                #region Dog
                bodyType = GameData.Instance.AllBodyTypes["dog"];  //
                listOfEntityTypes.Add(new EntityType("entity:dog") //"entity:dog"
                {
                    Name = "Dog",
                    ThumbnailSmall = "HUD_thumbnail_dog",
                    SummaryDescription = "Tau Shepherd dog breed.",
                    Description = "\n The German Shepherd was a popular choice as the basis for a dog breed adapted to the conditions on planet Antheia. Created as a result of some genetic engineering and some breeding. The Tau shepherd is trained to keep pest animals away and will also attack animals that are a threat to its owners.",
                    UseTypeNameForDisplay = false,
                    CategoryKey = "animals",
                    RenderableType = new RenderableType()
                    {
                        RenderAsModelType = new RenderAsModelType()
                        {
                            ModelScale = 1.78f,// priority goes: race ModelScale / cast ModelScale / default (renderAdModelType ModelScale) // this is overwritten by any data put in age, race or caste, in that order
                            AssetName = "dog",
                            GaitAnimations = new XmlDictionary<string, GaitAnimationBracket[]>
                            {                        
                         // MP: The gaitbrackets here must correspond to the base speeds defined in  LeggedLocomotorType  waaay further down !! base speeds define the speed they use for different purposes (?)

                           { "normal", new[]{ 
                                              new GaitAnimationBracket(){ MinimumSpeed = 1f, MaximumSpeed = 57f, StrideLength = 18f, StrideDuration = 0.96f, AnimationKey = "gaitWalk"}, //was MinimumSpeed = 20f, MaximumSpeed = 67f,
                                              new GaitAnimationBracket(){ MinimumSpeed = 54f, MaximumSpeed = 96f, StrideLength = 32f, StrideDuration = 0.96f, AnimationKey = "gaitTrot"}, //was MinimumSpeed = 64f, MaximumSpeed = 106f,
                                              new GaitAnimationBracket(){ MinimumSpeed = 92f, MaximumSpeed = 180f, StrideLength = 46f, StrideDuration = 0.96f, AnimationKey = "gaitGallop"}} }, //was MinimumSpeed = 102f, MaximumSpeed = 180f,
                           },                          
                            DefaultInfo = new AnimConditionInfo()
                            {
                                // use this neutral anim when all flags are cleared:
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "idle" } }
                            },


                          DefaultStances = new[] // "filler" anims used instead of idle when a stance has been set, but no action yet (happens between goals). This prevents unwanted switching to standing from kneeling, for instance
                        {
                            new  AnimConditionInfo()
                            {
                                 SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "idle" } }   
                            },
                            new  AnimConditionInfo()
                            {
                                 ConditionSet = new AnimConditions(){ Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Lying )},   
                                 SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "lying" } }           
                            }
                        },
                        #region AnumConditions
                            AnimConditions = new AnimConditionInfo[] 
                        {
                            new AnimConditionInfo()
                            {
                              
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Moving},
                                GaitSetKey = "normal"
                            },
          
                            
#region Stance changes / breakdowns

                            // we cannot use flag order to specify direction. Instead we use the Reverse flag and define the
                             //default direction as "get up". so, lying down needs "reverse flag".
                              new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "lyingToStand" }}, // stand up from lying
                                ConditionSet = new AnimConditions(){ Action = AnimAction.ChangingStance, 
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Lying)}, //
                                Looping = Looping.No
                            },  


                              new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "standToLying" }}, // lying from standing
                                ConditionSet = new AnimConditions(){ Action = AnimAction.ChangingStance,  
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Lying, (int)AnimModifier.Reverse)}, 
                                Looping = Looping.No
                            },   

#endregion     


                            new AnimConditionInfo() 
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "idle", "idleHowl"}}, 
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle }           
                            },

                                 new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "lying", "sleep" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Lying)}, //
                                Looping = Looping.Yes
                            },  
//mp: maybe use the AnimModifier.Trouble to avoid lying down in dangerous area like we do with humans?



                            new  AnimConditionInfo()
                            {
                                 ConditionSet = new AnimConditions(){ Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Bold )},                              
                                 SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "combatIdle" } }   
                            },

                             new AnimConditionInfo()
                            {
                              //  SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "attack" }}, 
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{ "attack", "attack", "attack", "attack", "attack", "attack" }, 
                                Sounds = new string[]{ "domesticated/dog/dogAttackSnarl1","domesticated/dog/dogAttackSnarl2", "", "", "", "" }}, // skip the sound sometimes, certain people find it too repetitive.
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Attacking,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier))},

  
                            },

                             new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "hit" },
                                Sounds = new string[]{ "domesticated/dog/dogHitBark1" }},
                                Playback = Playback.Manual, 
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Recoiling }
                            },

                            new AnimConditionInfo()
                            {                                 
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{ "collapse" },                                   
                                Sounds = new string[]{ "domesticated/dog/dogHitWhimper2" }},
                                Looping = Looping.No,
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying,
                                                                     Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Pre)}
                               
                            },

                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "dead" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Post)}  
                            },

                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{ "eat" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Eating},
                                Looping = Looping.Yes  
                            }, 

                             new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "sleep" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Sleeping,
                                     Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Lying)},
                                Looping = Looping.Yes  
                            },
                             
                        }
                        #endregion
                        },

       /*                 //MP this is not used because it bugged the model, causing the head to bend upwards when it attacked.  if you put it in, remember InterestInTriggerTypes
                  AnimatedHeadType = new AnimatedHeadType()
                    {
                        SpineBones = new[] { "dogNeck_1_jnt", "dogNeck_2_jnt", "dogNeck_3_jnt", "dogNeck_4_jnt", "dogHead_1_jnt", "dogHead_2_jnt" }, //only up to head joint. did not put in: "dogEar_R_1_jnt", "dogEar_R_1_jnt", "dogEar_L_1_jnt", "dogEar_L_2_jnt", "dogJaw_1_jnt", "dogJaw_2_jnt"

                        TurnToLookLerpFactor = 0.06f,
                        PitchForward = 0.62f
                    }*/
                    },
                    LocomotorType = new LocomotorType()
                    {
                        
                        MaxAngularSpeed = 3f * MathHelper.Pi, // MathHelper.Pi, there's also TurnSpeedWhenTurningInPlace which controls rotation speed (turn speed) when turning in place, but that is for all creatures i guess, since it's only defined one place. MP) 
                        FourSidedSymmetry = false,
                        MeleeRadius = 12f,
                        CanRun = true,
                        Stances = "dog",
                        LeggedLocomotorType = new LeggedLocomotorType()
                        {
                           
                            TerrainNegateFactor = 0.3f,
                            WalkSlowSpeed = 35f, // 
                            WalkNormalSpeed = 76f, //
                            WalkFastSpeed = 80f, // 
                            RunSpeed = 95f,
                        },
                        CollisionResponderType = new CollisionResponderType()
                        {
                            AgentCollisionResponderType = new AgentCollisionResponderType()
                        }
                    },
                    SensorType = new SensorType()
                    {
                        Range = sensorRangeHuman,
                        RangeAtNight = sensorRangeHuman,
                        DetectionTypeKey = "human" //don't use default detection, it doesn't contain all the tags which entities in a human allegiance needs; it will detect everything instantly
                    },

                    BodyType = bodyType,
                    ContainerType = new AgentStorageType() //0.5f)
                    {
                        ItemStorageType = new ItemStorageType(0.5f), // can Haul... later.
                        StomachStorageType = new ItemStorageType(0.07f) // stomach size is dynamic - is defined in BioType                        
                    },
                    IntelligenceType = new IntelligenceType()
                    {
                        MembersScoutingFraction = 1f, //dec 3, was 1f
                        IsMobile = true,
                        AllowEscapeFromTinyAreas = true,
                        CanAttack = true,
                        CanUseWeapons = false,
                        CanHunt = false,
                        CanScout = false,
                        CanExamine = false, 
                        CanPatrol = false,
                        CanHaul = false,
                        HuntsVermin = true,
                        IsPredator = true,
                        ContainerTransactTag = "dogTransact", //was "dog"
                        ServantForEntityTypeTag = "servesHumans",
                        StrengthRating = Entities.StrengthRating.WeakerThanHumans,
                        Boldness = 0.4f, //0.25f
                        Courage = 0.05f,   //fleeing.  ...tweak also vitalBodyPartDamageEvaluationBoost in constants.cs
                        MemoryInDays = 2f, 
                        ChanceToIdleWalkShortDistanceAway = 0.3f,
                        ShortIdleWalkMinDistance = 40f,
                        ShortIdleWalkMaxDistance = 160f,
                        Attacks = new[]
                         { 
                             "dogBiting"
                         
                         },
                     //       InterestInTriggerTypes = new[] { "entityDied", "creature" }, //mp not used because headturn doesnt work
                        Skills = new SerializableDictionary<string, float> 
                        { 
                            {
                               "unarmedFighting" , 0.3f
                            }
                        },
                        AggroRange = sensorRangeHuman + 100,
                        AssistanceRange = 600f // NEW!!!
                    },
                    BiologicalType = new BiologicalType()
                    {
                        OrderKey = "carnivoraOrder",
                        OxygenAndMuscleEnergyIncreaseRatePerDay = 12f,
                        FoodItemTagsThatCanBeConsumed = new[] { "cookedMeat", "rawMeat", "smallRawMeat", "spoiledMeal", "inedibleMeat", "rottenMeat" },
                        ExtractionProcessTypes = new[] { "extractMudWormMeat", "extractPatricianMeat", "extractLeafCutterMeat", "extractThunderChickenMeat", "extractBinalRatMeat", "extractTwinklerMeat", "extractBushDragonMeat", "extractSwampDemonTreeMeat", "extractDemonTreeMeat", "extractMegapodMeat", "extractWhipjawMeat", "extractSpikePlantMeat", "extractForestGuardianMeat", }, //"extractThinThunderChickenMeat",
                        TimeToConsumeFullMealInDays = 0.005f, //was 0.009f dec 2014. 
                        StomachSizeFractionOfEntityBulk = 0.1f, // big stomach so can eat quickly // 0.2f,
                        StomachContentsDecreaseRatePerDay = 3,
                        ActiveStealthRating = 0.2f,
                        EatingStances = new[] { new ChanceToTakeStance() { Stance = "standing" } },

                        MaxRegainLimit = humanMaxRegainLimit,
                        FractionOfMaxHitpointsGainedPerDay = humanFractionOfMaxHitpointsGainedPerDay,
                        Carcass = "item:dogCarcass",

                        ResilienceMean = 5.5f, // ### SSSUPER HACKKK ###
                        ResilienceStandardDeviation = 0.08f,

                        BioPropertyTypes = new[]{                       
                         new BioPropertyType(){
                          KeyName = "SensorRange", //measured in pixels  (White)    //aggrorange and assistancerange can be seen in debug panel>ranges, however if very big, will not be seen. see: public void DrawRanges() for colors.
                          InterpolateSetting = BioPropertyType.Interpolate.DefaultPriority                     
                     },
                      new BioPropertyType(){
                          KeyName = "SensorRangeAtNight", //measured in pixels
                          InterpolateSetting = BioPropertyType.Interpolate.DefaultPriority                     
                     },
                     new BioPropertyType(){ 
                          KeyName = "AggroRange", //measured in pixels  (red)
                          InterpolateSetting = BioPropertyType.Interpolate.DefaultPriority                     
                     },
                     new BioPropertyType(){
                          KeyName = "AssistanceRange", //measured in pixels  (green)
                          InterpolateSetting = BioPropertyType.Interpolate.DefaultPriority                     
                     }},
                        Castes = new List<CasteType>() 
                {                     
                    
                    new CasteType() {
                        KeyName = "male",
                        Reproduction = Reproduction.Male, //Edge = 1f,
                        HeightMean = .70f, HeightStandardDeviation = 0.02f, 
                        WeightMean = 40f, WeightStandardDeviation = 2f,
                        PrimaryColor = "564920".ToColorVector3(), ModelBasicTextureName = "DogGermanShepherdTexture", 
                        Edge = 0.6f,                                                    

                        AgeGroupTypes = new List<AgeGroupType>() 
                        { 
                            new AgeGroupType() 
                            { 
                                Name = "Puppy", AIAgeGroup = AIAgeGroup.Baby, CanReproduce = false, Edge = 0.5f, HeightTargetModifier = 0.05f, WeightTargetModifier = 0.03f, ModelScaleFraction = 0.7f, 
                                //NeedTypes = new NeedType[]{ babySleepNeed }
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Puppy", AIAgeGroup = AIAgeGroup.Child, CanReproduce = false, Edge = 1f, HeightTargetModifier = 0.1f, WeightTargetModifier = 0.1f, ModelScaleFraction = 0.9f,  
                                //NeedTypes = new NeedType[]{ childSleepNeed }
                            },     
                            new AgeGroupType() 
                            {
                                Name = "Young", AIAgeGroup = AIAgeGroup.YoungAdult, CanReproduce = true, Edge = 4f, HeightTargetModifier = 0.8f, WeightTargetModifier = 0.8f,
                                NeedTypes = new []{ dogFoodEnergyNeed, dogAdultSleepNeed },//childSleepNeed
                            },  
                            new AgeGroupType() 
                            { 
                                Name = "Grown", AIAgeGroup = AIAgeGroup.Adult, CanReproduce = true, Edge = 14f, HeightTargetModifier = 1f, WeightTargetModifier = 1f,
                                NeedTypes = new []{ dogFoodEnergyNeed, dogAdultSleepNeed }//adultSleepNeed
                            }
                            ,
                            new AgeGroupType() 
                            { 
                                Name = "Old", AIAgeGroup = AIAgeGroup.Old, CanReproduce = false, Edge = 20f, HeightTargetModifier = 0.9f, WeightTargetModifier = 1f,
                                NeedTypes = new []{ dogFoodEnergyNeed, dogAdultSleepNeed }//oldSleepNeed
                            }
                        } 
                    },
                    new CasteType() {
                        KeyName = "female",
                        Reproduction = Reproduction.Female, //Edge = 1f,
                        HeightMean = .70f, HeightStandardDeviation = 0.02f, 
                        WeightMean = 38f, WeightStandardDeviation = 2f,
                        PrimaryColor = "564920".ToColorVector3(), ModelBasicTextureName = "DogGermanShepherdTexture", 
                        Edge = 0.6f, 

                                                   

                        AgeGroupTypes = new List<AgeGroupType>() 
                        { 
                            new AgeGroupType() 
                            { 
                                Name = "Puppy", AIAgeGroup = AIAgeGroup.Baby, CanReproduce = false, Edge = 0.5f, HeightTargetModifier = 0.05f, WeightTargetModifier = 0.03f, ModelScaleFraction = 0.7f,  
                                //NeedTypes = new NeedType[]{ babySleepNeed }
                            } ,
                            new AgeGroupType() 
                            {
                                Name = "Puppy", AIAgeGroup = AIAgeGroup.Child, CanReproduce = false, Edge = 1f, HeightTargetModifier = 0.1f, WeightTargetModifier = 0.1f, ModelScaleFraction = 0.9f,   
                                //NeedTypes = new NeedType[]{ childSleepNeed }
                            },     
                            new AgeGroupType() 
                            { 
                                Name = "Young", AIAgeGroup = AIAgeGroup.YoungAdult, CanReproduce = true, Edge = 4f, HeightTargetModifier = 0.8f, WeightTargetModifier = 0.8f ,
                                NeedTypes = new []{ dogFoodEnergyNeed, dogAdultSleepNeed }//childSleepNeed
                            },  
                            new AgeGroupType() 
                            { 
                                Name = "Grown", AIAgeGroup = AIAgeGroup.Adult, CanReproduce = true, Edge = 14f, HeightTargetModifier = 1f, WeightTargetModifier = 1f,
                                NeedTypes = new []{ dogFoodEnergyNeed, dogAdultSleepNeed }//adultSleepNeed
                            },
                            new AgeGroupType() 
                            { 
                                Name = "Old", AIAgeGroup = AIAgeGroup.Old, CanReproduce = false, Edge = 20f, HeightTargetModifier = 0.9f, WeightTargetModifier = 1f,
                                NeedTypes = new []{ dogFoodEnergyNeed, dogAdultSleepNeed }//oldSleepNeed
                            }
                        } 
                    } 
                }

                    }
                });



                #endregion




                #region FLEA: Small scout/weeding robot

                #region Shared anims, shared rigging
                AnimConditionInfo robotInactivate = new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet()
                                {
                                    BaseAnimations = new string[] { "fold" },
                                    Sounds = new string[] { "robotServoArms2" }
                                }, // folding its arms back to idle from work idle
                                ConditionSet = new AnimConditions()
                                {
                                    Action = AnimAction.ChangingStance,
                                    Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Inactive)
                                }, //
                                Looping = Looping.No
                            };

                AnimConditionInfo robotActivate = new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet()
                                {
                                    BaseAnimations = new string[] { "unfold" },
                                    Sounds = new string[] { "robotServoArms2" }
                                }, // unfolding its arms from idle to work
                                ConditionSet = new AnimConditions()
                                {
                                    Action = AnimAction.ChangingStance,
                                    Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Inactive, (int)AnimModifier.Reverse)
                                },
                                Looping = Looping.No
                            };   

                AnimConditionInfo robotWeeding = new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet()
                                {
                                    BaseAnimations = new string[] { "farming" }
                                },
                                ConditionSet = new AnimConditions() { Action = AnimAction.Tilling },
                                Looping = Looping.Yes
                            };
                #endregion

                listOfEntityTypes.Add(new EntityType("entity:robotSmall")
                {
                    Name = "FLEA", //"TICK"
                    ThumbnailSmall = "HUD_thumbnail_smallRobot",
                    SummaryDescription = "Small robot equipped for weeding",
                    Description = "In this configuration, the FLEA is able to neutralize most forms of weeds. The arm mounted tool contains a cutting device, a pesticide dispenser and a laser. It is able to power itself by using vegetation as a fuel source for its engine and for recharging its batteries. Its parts are simple and easily replaceable which ensures a very long service life.",
                    CategoryKey = "robots",
                    RenderableType = new RenderableType()
                    {
                        RenderAsModelType = new RenderAsModelType()
                        {

                            ModelScale = 1.5f, //1.5f
                            AssetName = "robotLight",
                            ModelBasicTextureName = "RobotTexture1",
                            GaitAnimations = new XmlDictionary<string, GaitAnimationBracket[]>
                       {
                        
                        { "normal", new[]{                                           
                                              new GaitAnimationBracket(){ MinimumSpeed = 0f, MaximumSpeed = 77f, StrideLength = 11f , StrideDuration = 0.4f, AnimationKey = "idle"},       //MP: iterated strideduration 17th dec 2013.   ... was: MinimumSpeed = 0f, MaximumSpeed = 77f, StrideLength = 11f , StrideDuration = 0.2f, AnimationKey = "gaitWalk"                                     
                                              } }
                       },

                            AnimConditions = new AnimConditionInfo[] 
                        {
                            new AnimConditionInfo()
                            {
                               SoundAndAnimationSet = new RandomSoundAndAnimationSet(),   
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Moving },
                                GaitSetKey = "normal"
                            }, 

                            robotInactivate, // shared with GOPHER
                            robotActivate, // shared with GOPHER
                                           
                            new AnimConditionInfo() 
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ 
                                    BaseAnimations = new string[]{  "idle" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle }
                            },


                             new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "scan" }}, 
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Scouting }
                            },    

                            robotWeeding // shared with GOPHER
                        }
                        }
                    },
                    NonLivingType = new NonLivingType()
                    {
                        PartKeys = new SerializableDictionary<string, int>()
                        {
                            { "item:weedingRobotTool", 1 }
                        }
                    },
                    LocomotorType = new LocomotorType()
                    {

                        MaxAngularSpeed = 1.5f * MathHelper.Pi, // .6f * MathHelper.Pi,                    
                        MeleeRadius = 10f,
                        Stances = "robot",
                        LeggedLocomotorType = new LeggedLocomotorType()
                        {
                            TerrainNegateFactor = 0.5f,
                            WalkSlowSpeed = 20f, // 8f, 
                            WalkNormalSpeed = 33f, //debug only, //25f    patrician:26f, turnip:11f
                            WalkFastSpeed = 64f, //24f 18f,//30f, // 63f,


                        },
                        CollisionResponderType = new CollisionResponderType()
                        {
                            AgentCollisionResponderType = new AgentCollisionResponderType()
                        }
                    },
                    SensorType = new SensorType()
                    {
                        Range = sensorRangeHuman, // 
                        RangeAtNight = sensorRangeHuman,
                        DetectionTypeKey = "human" //don't use default detection, it doesn't contain all the tags which entities in a human allegiance needs; it will detect everything instantly
                    },
                    BodyType = GameData.Instance.AllBodyTypes["robotBody"],
                    
                    IntelligenceType = new IntelligenceType()
                    {
                        RespectsOwnership = true,
                        CanAttack = false,
                        CanUseWeapons = false,
                        CanProduce = true,
                        CanHaul = false,
                        CanHunt = false,
                        CanPatrol = false, //false because has no weapon
                        CanScout = false,
                        CanExamine = false, 
                        CanPanic = false,                      
                        ServantForEntityTypeTag = "servesHumans",
                        IsMobile = true,
                        StrengthRating = Entities.StrengthRating.WeakerThanHumans,   //
                        Courage = 1f,
                        MemoryInDays = 0.1f, //mp april 2015 was 3f. wanted them to be more mobile but cannot really see a difference.
                        Boldness = 1f, // MP may 6 2014 was 0.2f. I increased it to 1f to make sure they are never in cautious stance, because cautious right now colors their whole world yellow, causing them to freeze when a human apporaches , and all the time run from twinklers. Can be lowered once the strengthratings are adjusted - the goal should be not to have MASSIVE big threatradiusses which cover the whole map, because this causes freezing.

                        ChanceToIdleWalkShortDistanceAway = 0.1f, // used to avoid clumping with other robots
                        ShortIdleWalkMaxDistance = 120f,
                        ShortIdleWalkMinDistance = 48f,

                        IntrinsicTools = new string[]
                        {
                            "item:weedingRobotTool"
                        },
                        Skills = new SerializableDictionary<string, float> 
                        { 
                            {
                               "weeding" , 0.8f
                            }
                        },
                    }
                });

                #endregion

                #region HOUND: Small scout/weeding robot

                listOfEntityTypes.Add(new EntityType("entity:guardRobot")
                {
                    Name = "HOUND", //copied from FLEA
                    ThumbnailSmall = "HUD_thumbnail_smallRobot",
                    SummaryDescription = "Small robot equipped for guarding",
                    Description = "In this configuration, the HOUND is able to neutralize most forms of animals. The arm mounted tool contains a laser.",
                    CategoryKey = "robots",
                    RenderableType = new RenderableType()
                    {
                        RenderAsModelType = new RenderAsModelType()
                        {

                            ModelScale = 1.2f, //1.5f
                            AssetName = "robotLight",
                            ModelBasicTextureName = "RobotTexture1",
                            GaitAnimations = new XmlDictionary<string, GaitAnimationBracket[]>
                       {
                        
                        { "normal", new[]{                                           
                                              new GaitAnimationBracket(){ MinimumSpeed = 0f, MaximumSpeed = 77f, StrideLength = 11f , StrideDuration = 0.4f, AnimationKey = "idle"},       //MP: iterated strideduration 17th dec 2013.   ... was: MinimumSpeed = 0f, MaximumSpeed = 77f, StrideLength = 11f , StrideDuration = 0.2f, AnimationKey = "gaitWalk"                                     
                                              } }
                       },

                            AnimConditions = new AnimConditionInfo[] 
                        {
                            new AnimConditionInfo()
                            {
                               SoundAndAnimationSet = new RandomSoundAndAnimationSet(),   
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Moving },
                                GaitSetKey = "normal"
                            }, 

                            robotInactivate, // shared with GOPHER
                            robotActivate, // shared with GOPHER
                                           
                            new AnimConditionInfo() 
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ 
                                    BaseAnimations = new string[]{  "idle" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle }
                            },


                             new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "scan" }}, 
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Scouting }
                            },    

                            robotWeeding // shared with GOPHER
                        }
                        }
                    },                    
                    NonLivingType = new NonLivingType()
                    {
                        PartKeys = new SerializableDictionary<string, int>()
                        {
                            { "item:sentryLaserGun", 1 } // was item:sentryGun
                        }
                    },
                    LocomotorType = new LocomotorType()
                    {

                        MaxAngularSpeed = 1.5f * MathHelper.Pi, // .6f * MathHelper.Pi,                    
                        MeleeRadius = 10f,
                        Stances = "robot",
                        LeggedLocomotorType = new LeggedLocomotorType()
                        {
                            TerrainNegateFactor = 0.5f,
                            WalkSlowSpeed = 20f, // 8f, 
                            WalkNormalSpeed = 33f, //debug only, //25f    patrician:26f, turnip:11f
                            WalkFastSpeed = 64f, //24f 18f,//30f, // 63f,


                        },
                        CollisionResponderType = new CollisionResponderType()
                        {
                            AgentCollisionResponderType = new AgentCollisionResponderType()
                        }
                    },
                    SensorType = new SensorType()
                    {
                        Range = sensorRangeHuman, // 
                        RangeAtNight = sensorRangeHuman,
                        DetectionTypeKey = "human" //don't use default detection, it doesn't contain all the tags which entities in a human allegiance needs; it will detect everything instantly
                    },
                    BodyType = GameData.Instance.AllBodyTypes["robotBody"],

                    IntelligenceType = new IntelligenceType()
                    {
                        RespectsOwnership = true,
                        CanAttack = true,
                        CanUseWeapons = false,
                        CanProduce = true,
                        CanHaul = false,
                        CanHunt = false,
                        CanPatrol = true, //false because has no weapon
                        CanScout = true, 
                        CanExamine = false, // no examine!
                        CanPanic = false,
                        ServantForEntityTypeTag = "servesHumans",
                        IsMobile = true,
                        StrengthRating = Entities.StrengthRating.WeakerThanHumans,   //
                        Courage = 1f,
                        MemoryInDays = 0.1f, //mp april 2015 was 3f. wanted them to be more mobile but cannot really see a difference.
                        Boldness = 1f, // MP may 6 2014 was 0.2f. I increased it to 1f to make sure they are never in cautious stance, because cautious right now colors their whole world yellow, causing them to freeze when a human apporaches , and all the time run from twinklers. Can be lowered once the strengthratings are adjusted - the goal should be not to have MASSIVE big threatradiusses which cover the whole map, because this causes freezing.

                        ChanceToIdleWalkShortDistanceAway = 0.1f, // used to avoid clumping with other robots
                        ShortIdleWalkMaxDistance = 120f,
                        ShortIdleWalkMinDistance = 48f,
                        
                        IntrinsicWeapons = new[] { "item:sentryLaserGun" }, // was item:sentryGun
                        Skills = new SerializableDictionary<string, float> 
                        { 
                            {
                               "shooting" , 0.8f
                            }
                        },
                        Attacks = new[]
                        {
                             "sentryLaserGunShot"
                        }, 
                        AggroRange = sensorRangeHuman -100,
                        AssistanceRange = 400f
                    }
                });

                #endregion
                #region GOPHER: Harvesting/weeding/hauling robot
               // const float pickupHeavyActionPointDuration = 0.32f;
                const float pickupActionPointDuration = 0.4f;
                const float pickupDuration = 1.4f; 

                const float dropActionPointDuration = 1.4f; 
                const float dropDuration = 1.68f; 

                listOfEntityTypes.Add(new EntityType("entity:haulingRobot")
                {
                    Name = "GOPHER",
                    //ThumbnailSmall = "HUD_thumbnail_whiteThunderChicken",
                    SummaryDescription = "Sturdy farm robot able to harvest crops and transport items",
                    Description = "This robot vehicle was designed by the Tau Ceti planners with survivability in mind. It is able to power itself by using vegetation as a fuel source for its engine and for recharging its batteries. Its parts are simple and easily replaceable which ensures a very long service life.",
                    ThumbnailSmall = "HUD_thumbnail_haulingRobot",  
                    CategoryKey = "robots",
                    RenderableType = new RenderableType()
                    {
                       /*  BoxHandlingWhenHauling = new BoxHandlingWhenHauling()
                        {
                            BoxHandling = BoxHandlingWhenHauling.BoxHandlingType.OnlyOnBackWhenHeavyAndHaulingFar,
                            UseHeavyBackpack = true
                        }*/
                        BoxHandlingWhenHauling = new BoxHandlingWhenHauling()
                       {
                            BoxHandling = BoxHandlingWhenHauling.BoxHandlingType.AlwaysOnBack,
                            UseHeavyBackpack = false,
                            ShowBoxInHand = AttacheePoint.LeftHand,
                            AttachorWhenBoxIsInHand = "leftHand"
                       },
                        RenderAsModelType = new RenderAsModelType()
                        {

                            ModelScale = 1.45f, // 1.66f, //1.5f
                            AssetName = "robotHeavy",
                            ModelBasicTextureName = "RobotTexture1",
                            GaitAnimations = new XmlDictionary<string, GaitAnimationBracket[]>
                           {                        
                                { "normal", new[]
                                    {                                           
                                            new GaitAnimationBracket(){ MinimumSpeed = 0f, MaximumSpeed = 77f, StrideLength = 11f /* measured?: 18.1f*/, StrideDuration = 0.4f, AnimationKey = "idle"},       //MP: iterated strideduration 17th dec 2013.   ... was: MinimumSpeed = 0f, MaximumSpeed = 77f, StrideLength = 11f , StrideDuration = 0.2f, AnimationKey = "gaitWalk"                                     
                                     } 
                                }
                           },
                        DefaultInfo = new AnimConditionInfo()
                        {
                            // use this neutral anim when all flags are cleared:
                            SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "idleUnfolded" } }
                        },
                        DefaultStances = new[] // "filler" anims used instead of idle when a stance has been set, but no action yet (happens between goals). This prevents unwanted switching to standing from kneeling, for instance
                        {
                            new  AnimConditionInfo()
                            {
                                 SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "idle" } },
                                 ConditionSet = new AnimConditions(){ 
                                     Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Inactive )
                                 }
                            },
                            new  AnimConditionInfo()
                            {
                                 SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "idleUnfolded" } }   //idleUnfolded is buggy
      
                            }
                        },
                        AnimConditions = new AnimConditionInfo[] 
                        {
                            new AnimConditionInfo()
                            {
                               /*SoundAndAnimationSet = new RandomSoundAndAnimationSet() { Sounds = new[] { "robotDriveEngineMedium" } // move sound disabled because it only works some of the time },   */ 
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Moving },
                                GaitSetKey = "normal"
                            }, 
                            
#region Hauling (moving) - always the same
                            new AnimConditionInfo() // haul light 
                            {                               
                                GaitSetKey = "normal",
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Hauling},
                                Forbiddens = new BitMask64(typeof(AnimModifier), (int)AnimModifier.HaulHeavy)
                            },     
                            new AnimConditionInfo() // haul light long distance - same as above, added to prevent match with HaulHeavy
                            {                               
                                GaitSetKey = "normal",
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Hauling, Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Far )},
                                Forbiddens = new BitMask64(typeof(AnimModifier), (int)AnimModifier.HaulHeavy)
                            },                  
                            new AnimConditionInfo() // haul heavy 
                            {                                
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Hauling, 
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.HaulHeavy )},
                                GaitSetKey = "normal" 
                             
                            },
                            new AnimConditionInfo() // haul heavy long distance
                            {                                
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Hauling, 
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.HaulHeavy, (int)AnimModifier.Far )},
                                GaitSetKey = "normal" //
                            },

#endregion



                            //anims from here   
 #region Stance changes / breakdowns

                            // we cannot use flag order to specify direction. Instead we use the Reverse flag and define the
                             //default direction as "get up". so, Sitting down needs "reverse flag".
                             robotInactivate,
                             robotActivate,

#endregion             


                            new AnimConditionInfo() 
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ 
                                    BaseAnimations = new string[]{  "idle" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle }
                            },
                                 new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "idleUnfolded" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle,
                                                              //   Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Activated)
                                }, 
                                Looping = Looping.Yes
                            },  

                          new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "pickup" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.PickingUp},
                                Looping = Looping.No,
                                StartingPoint = StartingPoint.FromBeginning
                            },    

                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "pickup" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.PickingUp,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Heavy )},
                                Looping = Looping.No,
                                StartingPoint = StartingPoint.FromBeginning
                            },  
                              // 2nd half of pickup:
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "pickup" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.PickingUp, 
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Heavy, (int)AnimModifier.Post )},
                                Looping = Looping.No,
                                StartingPoint = StartingPoint.Specified,
                                StartingPointInSeconds = pickupActionPointDuration
                            },   
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "pickup" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.PickingUp,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Post )},
                                Forbiddens = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Heavy),
                                Looping = Looping.No,
                                StartingPoint = StartingPoint.Specified,
                                StartingPointInSeconds = pickupActionPointDuration
                            },   

                            //***** drop anims:
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{ "drop"  }}, //dropLight
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dropping},
                                                                 
                                Looping = Looping.No,
                                StartingPoint = StartingPoint.FromBeginning                              
                            },   
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{ "drop"  }}, //dropLightSame
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dropping,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Same )},
                                Looping = Looping.No,
                                StartingPoint = StartingPoint.FromBeginning                              
                            },   
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "drop"  }}, //"dropHeavy"
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dropping,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Heavy )},                                
                                Looping = Looping.No,
                                StartingPoint = StartingPoint.FromBeginning                              
                            },              
                            

                            //MP no stance modifier Active needs to be set for work anims because it is set as default.

                            robotWeeding,   

                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "farming" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Harvesting},
                                Looping = Looping.Yes                                
                            }  
                        }
                        }
                    },
                    NonLivingType = new NonLivingType()
                    {
                        PartKeys = new SerializableDictionary<string, int>()
                        {
                            { "item:weedingRobotTool", 1 }                           
                        }
                    },
                    LocomotorType = new LocomotorType()
                    {

                        MaxAngularSpeed = 1.5f * MathHelper.Pi, // .6f * MathHelper.Pi,                    
                        MeleeRadius = 10f,
                        Stances = "robot",
                        LeggedLocomotorType = new LeggedLocomotorType()
                        {
                            TerrainNegateFactor = 0.5f,
                            WalkSlowSpeed = 14f, //14f, 20f
                            WalkNormalSpeed = 30f, //20f, 23f
                            WalkFastSpeed = 42f, //32f, 44f
                            HaulSpeed = 26f //18f
                        },
                        CollisionResponderType = new CollisionResponderType()
                        {
                            AgentCollisionResponderType = new AgentCollisionResponderType()
                        }
                    },
                    SensorType = new SensorType()
                    {
                        Range = sensorRangeHuman - 3, // short range
                        RangeAtNight = sensorRangeHuman - 3,
                        DetectionTypeKey = "human" //don't use default detection, it doesn't contain all the tags which entities in a human allegiance needs; it will detect everything instantly
                    },
                    BodyType = GameData.Instance.AllBodyTypes["robotBody"],

                    ContainerType = new AgentStorageType() //0.5f)
                    {
                        ItemStorageType = new ItemStorageType(1f), //same as person's   //was 0.7      can Haul                        
                        
                    },

                    IntelligenceType = new IntelligenceType()
                    {
                        RespectsOwnership = true,
                        CanAttack = false,
                        CanUseWeapons = false,
                        CanHaul = true, // can haul items
                        CanProduce = true,
                        CanHunt = false,
                        CanPatrol = false, //false because no weapon
                        CanScout = false,
                        CanExamine = false, 
                        CanPanic = false,
                        ContainerTransactTag = "robotTransact",
                        ServantForEntityTypeTag = "servesHumans",
                        IsMobile = true,
                        StrengthRating = Entities.StrengthRating.WeakerThanHumans,   //
                        Courage = 1f,
                        MemoryInDays = 0.1f,
                        Boldness = 1f, 
                        #region Pickup Anim durations                       
                       
                        PickupLightDuration = pickupDuration,
                        PickupLightActionPointDuration = pickupActionPointDuration,

                        PickupHeavyDuration = pickupDuration,
                        PickupHeavyActionPointDuration = pickupActionPointDuration,

                        SwitchLightToLightDuration = pickupDuration, 
                        SwitchLightToLightActionPointDuration = pickupActionPointDuration, 

                        #endregion

                        #region Drop Anim durations                        
                        DropLightDuration = dropDuration,
                        DropLightActionPointDuration = dropActionPointDuration,

                        DropHeavyDuration = dropDuration,
                        DropHeavyActionPointDuration = dropActionPointDuration,

                        DropLightToLightActionPointDuration = dropActionPointDuration, 
                        DropLightToLightDuration = dropDuration, 

                        #endregion

                        ChanceToIdleWalkShortDistanceAway = 0.1f, // used to avoid clumping with other robots
                        ShortIdleWalkMaxDistance = 120f,
                        ShortIdleWalkMinDistance = 48f,

                        IntrinsicTools = new string[]
                        {
                            "item:weedingRobotTool"
                        },
                       
//no gun because it requires replenish which humans have to do... but they can only act on static entities for now.

                        Skills = new SerializableDictionary<string, float> 
                        { 
                            { "grasping" , 0.6f },
                            { "fruitPicking" , 0.6f },
                            { "weeding" , 0.8f }

                        },

                    }
                });

                #endregion

                #region MOLE Digging Robot used in The Mining Camp

                listOfEntityTypes.Add(new EntityType("entity:diggingRobot")
                {
                    Name = "MOLE",
                    //ThumbnailSmall = "HUD_thumbnail_whiteThunderChicken",
                    SummaryDescription = "Sturdy mining robot able to mine ores and transport items",
                    Description = "This robot vehicle was designed by the Tau Ceti planners with survivability in mind. It is able to power itself by using vegetation as a fuel source for its engine and for recharging its batteries. Its parts are simple and easily replaceable which ensures a very long service life.",
                    ThumbnailSmall = "HUD_thumbnail_haulingRobot",
                    CategoryKey = "robots",
                    RenderableType = new RenderableType()
                    {
                        /*  BoxHandlingWhenHauling = new BoxHandlingWhenHauling()
                         {
                             BoxHandling = BoxHandlingWhenHauling.BoxHandlingType.OnlyOnBackWhenHeavyAndHaulingFar,
                             UseHeavyBackpack = true
                         }*/
                        BoxHandlingWhenHauling = new BoxHandlingWhenHauling()
                        {
                            BoxHandling = BoxHandlingWhenHauling.BoxHandlingType.AlwaysOnBack,
                            UseHeavyBackpack = false,
                            ShowBoxInHand = AttacheePoint.LeftHand,
                            AttachorWhenBoxIsInHand = "leftHand"
                        },
                        RenderAsModelType = new RenderAsModelType()
                        {

                            ModelScale = 1.50f, // NA was 1.45
                            AssetName = "robotHeavy",
                            ModelBasicTextureName = "RobotTexture2", //replace with yellow texture, make this work: "RobotTexture2"
                            GaitAnimations = new XmlDictionary<string, GaitAnimationBracket[]>
                           {                        
                                { "normal", new[]
                                    {                                           
                                            new GaitAnimationBracket(){ MinimumSpeed = 0f, MaximumSpeed = 77f, StrideLength = 11f /* measured?: 18.1f*/, StrideDuration = 0.4f, AnimationKey = "idle"},       //MP: iterated strideduration 17th dec 2013.   ... was: MinimumSpeed = 0f, MaximumSpeed = 77f, StrideLength = 11f , StrideDuration = 0.2f, AnimationKey = "gaitWalk"                                     
                                     } 
                                }
                           },
                            DefaultInfo = new AnimConditionInfo()
                            {
                                // use this neutral anim when all flags are cleared:
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "idleUnfolded" } }
                            },
                            DefaultStances = new[] // "filler" anims used instead of idle when a stance has been set, but no action yet (happens between goals). This prevents unwanted switching to standing from kneeling, for instance
                        {
                            new  AnimConditionInfo()
                            {
                                 SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "idle" } },
                                 ConditionSet = new AnimConditions(){ 
                                     Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Inactive )
                                 }
                            },
                            new  AnimConditionInfo()
                            {
                                 SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "idleUnfolded" } }   //idleUnfolded is buggy
      
                            }
                        },
                            AnimConditions = new AnimConditionInfo[] 
                        {
                            new AnimConditionInfo()
                            {
                               /*SoundAndAnimationSet = new RandomSoundAndAnimationSet() { Sounds = new[] { "robotDriveEngineMedium" } // move sound disabled because it only works some of the time },   */ 
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Moving },
                                GaitSetKey = "normal"
                            }, 
                            
#region Hauling (moving) - always the same
                            new AnimConditionInfo() // haul light 
                            {                               
                                GaitSetKey = "normal",
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Hauling},
                                Forbiddens = new BitMask64(typeof(AnimModifier), (int)AnimModifier.HaulHeavy)
                            },     
                            new AnimConditionInfo() // haul light long distance - same as above, added to prevent match with HaulHeavy
                            {                               
                                GaitSetKey = "normal",
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Hauling, Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Far )},
                                Forbiddens = new BitMask64(typeof(AnimModifier), (int)AnimModifier.HaulHeavy)
                            },                  
                            new AnimConditionInfo() // haul heavy 
                            {                                
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Hauling, 
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.HaulHeavy )},
                                GaitSetKey = "normal" 
                             
                            },
                            new AnimConditionInfo() // haul heavy long distance
                            {                                
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Hauling, 
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.HaulHeavy, (int)AnimModifier.Far )},
                                GaitSetKey = "normal" //
                            },

#endregion



                            //anims from here   
 #region Stance changes / breakdowns

                            // we cannot use flag order to specify direction. Instead we use the Reverse flag and define the
                             //default direction as "get up". so, Sitting down needs "reverse flag".
                             robotInactivate,
                             robotActivate,

#endregion             


                            new AnimConditionInfo() 
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ 
                                    BaseAnimations = new string[]{  "idle" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle }
                            },
                                 new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "idleUnfolded" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle,
                                                              //   Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Activated)
                                }, 
                                Looping = Looping.Yes
                            },  

                          new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "pickup" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.PickingUp},
                                Looping = Looping.No,
                                StartingPoint = StartingPoint.FromBeginning
                            },    

                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "pickup" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.PickingUp,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Heavy )},
                                Looping = Looping.No,
                                StartingPoint = StartingPoint.FromBeginning
                            },  
                              // 2nd half of pickup:
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "pickup" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.PickingUp, 
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Heavy, (int)AnimModifier.Post )},
                                Looping = Looping.No,
                                StartingPoint = StartingPoint.Specified,
                                StartingPointInSeconds = pickupActionPointDuration
                            },   
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "pickup" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.PickingUp,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Post )},
                                Forbiddens = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Heavy),
                                Looping = Looping.No,
                                StartingPoint = StartingPoint.Specified,
                                StartingPointInSeconds = pickupActionPointDuration
                            },   

                            //***** drop anims:
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{ "drop"  }}, //dropLight
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dropping},
                                                                 
                                Looping = Looping.No,
                                StartingPoint = StartingPoint.FromBeginning                              
                            },   
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{ "drop"  }}, //dropLightSame
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dropping,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Same )},
                                Looping = Looping.No,
                                StartingPoint = StartingPoint.FromBeginning                              
                            },   
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "drop"  }}, //"dropHeavy"
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dropping,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Heavy )},                                
                                Looping = Looping.No,
                                StartingPoint = StartingPoint.FromBeginning                              
                            },       

                            //MP no stance modifier Active needs to be set for work anims because it is set as default.
                            /////////////////////     
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "farming" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Digging},
                                Looping = Looping.Yes                                
                            }  
                        }
                        }
                    },
                    NonLivingType = new NonLivingType()
                    {
                        PartKeys = new SerializableDictionary<string, int>()
                        {
                            { "item:diggingRobotTool", 1 }                           
                        }
                    },
                    LocomotorType = new LocomotorType()
                    {

                        MaxAngularSpeed = 1.5f * MathHelper.Pi, // .6f * MathHelper.Pi,                    
                        MeleeRadius = 10f,
                        Stances = "robot",
                        LeggedLocomotorType = new LeggedLocomotorType()
                        {
                            TerrainNegateFactor = 0.5f,
                            WalkSlowSpeed = 14f, //14f, 20f
                            WalkNormalSpeed = 30f, //20f, 23f
                            WalkFastSpeed = 42f, //32f, 44f
                            HaulSpeed = 26f //18f
                        },
                        CollisionResponderType = new CollisionResponderType()
                        {
                            AgentCollisionResponderType = new AgentCollisionResponderType()
                        }
                    },
                    SensorType = new SensorType()
                    {
                        Range = sensorRangeHuman - 3, // short range
                        RangeAtNight = sensorRangeHuman - 3,
                        DetectionTypeKey = "human" //don't use default detection, it doesn't contain all the tags which entities in a human allegiance needs; it will detect everything instantly
                    },
                    BodyType = GameData.Instance.AllBodyTypes["robotBody"],

                    ContainerType = new AgentStorageType() //0.5f)
                    {
                        ItemStorageType = new ItemStorageType(1.3f), // can Haul // more than person's. maybe make it bigger?                        

                    },

                    IntelligenceType = new IntelligenceType()
                    {
                        RespectsOwnership = true,
                        CanAttack = false,
                        CanUseWeapons = false,
                        CanHaul = true, // can haul items
                        CanProduce = true,
                        CanHunt = false,
                        CanPatrol = false, //false because no weapon
                        CanScout = false,
                        CanExamine = false, 
                        CanPanic = false,
                        ContainerTransactTag = "robotTransact",
                        ServantForEntityTypeTag = "servesHumans",
                        IsMobile = true,
                        StrengthRating = Entities.StrengthRating.WeakerThanHumans,   //
                        Courage = 1f,
                        MemoryInDays = 0.1f,
                        Boldness = 1f,
                        #region Pickup Anim durations

                        PickupLightDuration = pickupDuration,
                        PickupLightActionPointDuration = pickupActionPointDuration,

                        PickupHeavyDuration = pickupDuration,
                        PickupHeavyActionPointDuration = pickupActionPointDuration,

                        SwitchLightToLightDuration = pickupDuration,
                        SwitchLightToLightActionPointDuration = pickupActionPointDuration,

                        #endregion

                        #region Drop Anim durations
                        DropLightDuration = dropDuration,
                        DropLightActionPointDuration = dropActionPointDuration,

                        DropHeavyDuration = dropDuration,
                        DropHeavyActionPointDuration = dropActionPointDuration,

                        DropLightToLightActionPointDuration = dropActionPointDuration,
                        DropLightToLightDuration = dropDuration,

                        #endregion


                        ChanceToIdleWalkShortDistanceAway = 0.1f, // used to avoid clumping with other robots
                        ShortIdleWalkMaxDistance = 120f, 
                        ShortIdleWalkMinDistance = 48f,

                        IntrinsicTools = new string[]
                        {
                            "item:diggingRobotTool"
                        },

                        //no gun because it requires replenish which humans have to do... but they can only act on static entities for now.

                        Skills = new SerializableDictionary<string, float> 
                        { 
                            { "grasping" , 0.6f },
                            { "fruitPicking" , 0.6f },
                            { "weeding" , 0.8f },
                            //{ "menial" , 0.8f }

                        },

                    }
                });

                #endregion


                #region  //robot test experiments, commented out - NOT USED
                #region test robot with thunderchicken model
                /*             
                listOfEntityTypes.Add(new EntityType("entity:weedingRobot")
                {
                    Name = "TEST Weeding ROBOT",
                    //ThumbnailSmall = "HUD_thumbnail_whiteThunderChicken",
                    SummaryDescription = "Weeding robot",  
                    Description = "",                  
                    RenderableType = new RenderableType()
                    {
                        RenderAsModelType = new RenderAsModelType()
                        {

                            ModelScale = 1.5f, //1.5f
                            AssetName = "thunderchicken",
                            ModelBasicTextureName = "ThunderchickenPaleTexture",
                            GaitAnimations = new XmlDictionary<string, GaitAnimationBracket[]>
                       {
                        
                        { "normal", new[]{                                           
                                              new GaitAnimationBracket(){ MinimumSpeed = 0f, MaximumSpeed = 77f, StrideLength = 11f /* measured?: 18.1f*/
                //, StrideDuration = 0.4f, AnimationKey = "gaitWalk"},       //MP: iterated strideduration 17th dec 2013.   ... was: MinimumSpeed = 0f, MaximumSpeed = 77f, StrideLength = 11f , StrideDuration = 0.2f, AnimationKey = "gaitWalk"                                     
                /*                             } }
                      },

                           AnimConditions = new AnimConditionInfo[] 
                       {
                           new AnimConditionInfo()
                           {
                              SoundAndAnimationSet = new RandomSoundAndAnimationSet()
                              {
                                   AdditionalAnimations1 = new string[]{ "hidden" }
                               },
                               ConditionSet = new AnimConditions(){ Action = AnimAction.Moving },
                               GaitSetKey = "normal"
                           },
 
              /*           new AnimConditionInfo()                 //commented out by MP when putting in new gait data. 6 sep 2013
                           {
                               AnimationSet = new RandomAnimationSet(){ 
                                   BaseAnimations = new string[]{  "walk"},
                                   AdditionalAnimations1 = new string[]{ "hidden" }},

                               Conditions = new ConditionSet(){ Modifiers = new BitMask64(typeof(AnimState), (int)AnimState.Moving )                               
                           },*/

                /*          new AnimConditionInfo() 
                          {
                              SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ 
                                  BaseAnimations = new string[]{  "idle" },
                                  AdditionalAnimations1 = new string[]{ "look" },
                                  AdditionalAnimations2 = new string[]{ "eat" }},
                              ConditionSet = new AnimConditions(){ Action = AnimAction.Idle }
                          },
                          new AnimConditionInfo() 
                          {
                              SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ 
                                  BaseAnimations = new string[]{  "idle" },
                                  AdditionalAnimations2 = new string[]{ "eat" }
                              },
                              ConditionSet = new AnimConditions(){ Action = AnimAction.Eating }
                          },
                          new AnimConditionInfo()
                          {
                              SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{ "dying" }},                          
                              ConditionSet = new AnimConditions(){ Action = AnimAction.Dying }
                          },
                          new AnimConditionInfo()
                          {
                              SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "dead" }},
                              ConditionSet = new AnimConditions(){ Action = AnimAction.Dying,
                                                               Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Post)}  
                          },
                      }
                      }
                  },
                  NonLivingType = new NonLivingType()
                  {
                      PartKeys = new SerializableDictionary<string,int>()
                      {
                          { "item:weedingRobotTool", 1 }
                      }
                  },
                  LocomotorType = new LocomotorType()
                  {

                      MaxAngularSpeed = 1.5f * MathHelper.Pi, // .6f * MathHelper.Pi,                    
                      MeleeRadius = 10f,

                      LeggedLocomotorType = new LeggedLocomotorType()
                      {
                          TerrainNegateFactor = 0.5f,
                          WalkSlowSpeed = 20f, // 8f, 
                          WalkNormalSpeed = 23f /*debug only*/
                //, //25f   18f /*proper speed*/, patrician:26f, turnip:11f
                /*            WalkFastSpeed = 44f, //24f 18f,//30f, // 63f,


                        },
                        CollisionResponderType = new CollisionResponderType()
                        {
                            AgentCollisionResponderType = new AgentCollisionResponderType()
                        }
                    },
                    SensorType = new SensorType()
                    {
                        Range = sensorRangeHuman - 3, // short range
                        RangeAtNight = sensorRangeHuman - 3,
                        DetectionTypeKey = "defaultDetection"
                    },
                    BodyType = GameData.Instance.AllBodyTypes["weedingRobot"],

                    ContainerType = new AgentStorageType() //0.5f)
                    {
                        ItemStorageType = new ItemStorageType(0.5f)                 
                        
                    },

                    IntelligenceType = new IntelligenceType()
                    {
                        RespectsOwnership = true,
                        CanAttack = false,
                        CanUseWeapons = false,
                        CanProduce = true,
                        CanHaul = false,
                        CanHunt = false, 
                        CanPatrol = false, 
                        CanScout = false,
                        CanPanic = false,                       
                        ServantForEntityTypeTag = "servesHumans",
                        IsMobile = true,
                        StrengthRating = Entities.StrengthRating.WeakerThanHumans,   //
                        Courage = 1f,
                        MemoryInDays = 0.1f, //mp april 2015 was 3f. wanted them to be more mobile but cannot really see a difference.
                        Boldness = 1f, // MP may 6 2014 was 0.2f. I increased it to 1f to make sure they are never in cautious stance, because cautious right now colors their whole world yellow, causing them to freeze when a human apporaches , and all the time run from twinklers. Can be lowered once the strengthratings are adjusted - the goal should be not to have MASSIVE big threatradiusses which cover the whole map, because this causes freezing.
                       
                        IntrinsicTools = new string[]
                        {
                            "item:weedingRobotTool"
                        },
                        Skills = new SerializableDictionary<string, float> 
                        { 
                            {
                               "weeding" , 0.8f
                            }
                        },
                    }
                });*/

                #endregion
                #region Patrol robot test chicken
/*
                listOfEntityTypes.Add(new EntityType("entity:patrolRobot")
                {
                    Name = "HOUND2",
                    //ThumbnailSmall = "HUD_thumbnail_whiteThunderChicken",
                    SummaryDescription = "Patrol robot",
                    Description = "",                   
                    RenderableType = new RenderableType()
                    {
                        RenderAsModelType = new RenderAsModelType()
                        {

                            ModelScale = 1.5f, //1.5f
                            AssetName = "thunderchicken",
                            ModelBasicTextureName = "ThunderchickenBulkyTexture2",
                            GaitAnimations = new XmlDictionary<string, GaitAnimationBracket[]>
                       {
                        
                        { "normal", new[]{                                           
                                              new GaitAnimationBracket(){ MinimumSpeed = 0f, MaximumSpeed = 77f, StrideLength = 11f /* measured?: 18.1f*///, StrideDuration = 0.4f, AnimationKey = "gaitWalk"},       //MP: iterated strideduration 17th dec 2013.   ... was: MinimumSpeed = 0f, MaximumSpeed = 77f, StrideLength = 11f , StrideDuration = 0.2f, AnimationKey = "gaitWalk"                                     
                     /*                         } }
                       },

                            AnimConditions = new AnimConditionInfo[] 
                        {
                            new AnimConditionInfo()
                            {
                               SoundAndAnimationSet = new RandomSoundAndAnimationSet()
                               {
                                    AdditionalAnimations1 = new string[]{ "hidden" }
                                },
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Moving },
                                GaitSetKey = "normal"
                            },
 
               /*           new AnimConditionInfo()                 //commented out by MP when putting in new gait data. 6 sep 2013
                            {
                                AnimationSet = new RandomAnimationSet(){ 
                                    BaseAnimations = new string[]{  "walk"},
                                    AdditionalAnimations1 = new string[]{ "hidden" }},

                                Conditions = new ConditionSet(){ Modifiers = new BitMask64(typeof(AnimState), (int)AnimState.Moving )                               
                            },*/
                                           
                    /*        new AnimConditionInfo() 
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ 
                                    BaseAnimations = new string[]{  "idle" },
                                    AdditionalAnimations1 = new string[]{ "look" },
                                    AdditionalAnimations2 = new string[]{ "eat" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle }
                            },
                            new AnimConditionInfo() 
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ 
                                    BaseAnimations = new string[]{  "idle" },
                                    AdditionalAnimations2 = new string[]{ "eat" }
                                },
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Eating }
                            },
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{ "dying" }},
                          //      Sound = "aliens/spacechicken",  bug -never stops
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying }
                            },

                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "dead" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Post)}  
                            },
                        }
                        }
                    },
                    LocomotorType = new LocomotorType()
                    {

                        MaxAngularSpeed = 1.5f * MathHelper.Pi, // .6f * MathHelper.Pi,                    
                        MeleeRadius = 10f,

                        LeggedLocomotorType = new LeggedLocomotorType()
                        {
                            TerrainNegateFactor = 0.5f,
                            WalkSlowSpeed = 20f, // 8f, 
                            WalkNormalSpeed = 23f /*debug only*///, //25f   18f /*proper speed*/, patrician:26f, turnip:11f
                      /*      WalkFastSpeed = 44f, //24f 18f,//30f, // 63f,


                        },
                        CollisionResponderType = new CollisionResponderType()
                        {
                            AgentCollisionResponderType = new AgentCollisionResponderType()
                        }
                    },
                    SensorType = new SensorType()
                    {
                        Range = sensorRangeHuman, 
                        RangeAtNight = sensorRangeHuman,
                        DetectionTypeKey = "defaultDetection"
                    },
                    BodyType = GameData.Instance.AllBodyTypes["weedingRobot"],

                    ContainerType = new AgentStorageType() //0.5f)
                    {
                        ItemStorageType = new ItemStorageType(0.5f), // can Haul... later.                         
                        
                    },

                    IntelligenceType = new IntelligenceType()
                    {
                        RespectsOwnership = true,
                        CanAttack = true,
                        CanUseWeapons = false,
                        CanHaul = false,
                        CanHunt = false,
                        CanPatrol = true, //mp code requires weapon currently...
                        CanScout = true,
                        CanPanic = false,                    
                        ServantForEntityTypeTag = "servesHumans",
                        IsMobile = true,
                        StrengthRating = Entities.StrengthRating.WeakerThanHumans,   //
                        Courage = 1f,
                        MemoryInDays = 0.1f, //mp april 2015 was 3f. wanted them to be more mobile but cannot really see a difference.
                        Boldness = 1f, // MP may 6 2014 was 0.2f. I increased it to 1f to make sure they are never in cautious stance, because cautious right now colors their whole world yellow, causing them to freeze when a human apporaches , and all the time run from twinklers. Can be lowered once the strengthratings are adjusted - the goal should be not to have MASSIVE big threatradiusses which cover the whole map, because this causes freezing.

                        Skills = new SerializableDictionary<string, float> 
                    { 
                        {
                           "shooting" , 0.7f
                        }
                    },
                        Attacks = new[]
                     {
                         "sentryShootGun"
                     }
                    }
                });*/

                #endregion
                #region Test: Gun Dog
                /*  bodyType = GameData.Instance.AllBodyTypes["twinkler"];  //needs species entity type set up MP mar 2014
                listOfEntityTypes.Add(new EntityType("entity:gunDog")
                {
                    Name = "Gun Dog",
                    ThumbnailSmall = "HUD_thumbnail_twinkler",
                    SummaryDescription = "Domesticated animal w/ gun",
                    Description = "",
                    RenderableType = new RenderableType()
                    {
                        RenderAsModelType = new RenderAsModelType()
                        {
                            ModelScale = 2.5f, //normal size:2.5f, small size:1.5f   //per Dec 2012, this is overwritten by any data put in age, race or caste, in that order
                            AssetName = "twinkler",
                            GaitAnimations = new XmlDictionary<string, GaitAnimationBracket[]>
                            {                        
                                { "normal", new[]
                                    {                                           
                                        new GaitAnimationBracket(){ MinimumSpeed = 35f, MaximumSpeed = 77f, StrideLength = 27f, StrideDuration = 0.9f, AnimationKey = "gaitWalk"},  //MP: made a quick iteration on strideduration 17th dec 2013.   values were: :MinimumSpeed = 35f, MaximumSpeed = 77f, StrideLength = 27f, StrideDuration = 0.46f,
                                    } 
                                }
                           },

                            AnimConditions = new AnimConditionInfo[] 
                        {
                            new AnimConditionInfo()
                            {
                              
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Moving},
                                GaitSetKey = "normal"
                            },
                                           
                            new AnimConditionInfo() 
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "idle" }},
                           //     Sound = "aliens/croaker",   error...gets played a lot when they are dead....18th oct 2013
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle }           
                            },
   

                             new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "hit" }, Sounds = new string[]{ "aliens/hummingClickClacking" }},                              
                                Playback = Playback.Manual, 
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Recoiling }
                            },

                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{ "dying" }},
                           //     Sound = "aliens/toothCrickets",  bug -never stops ..21 oct
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying }
                            },

                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "dead" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Post)}  
                            },
                             
                        }
                        }
                    },
                    LocomotorType = new LocomotorType()
                    {

                        MaxAngularSpeed = .25f * MathHelper.Pi, // MathHelper.Pi,
                        FourSidedSymmetry = true,
                        MeleeRadius = 12f,

                        LeggedLocomotorType = new LeggedLocomotorType()
                        {
                            TerrainNegateFactor = 0.3f,
                            WalkSlowSpeed = 20f, // 8f, 
                            WalkNormalSpeed = 22f /*debug only*/
                //,  /*proper speed*///, ModelScale = 2.5f : (NORMAL twinkler walk speed=1.0, basespeed=25f ) (SLOW twinkler walk speed=0.7, basespeed=21f) (FAST twinkler walk speed=2, basespeed=75f) 
                /*    //, ModelScale = 1.5f : (NORMAL twinkler walk speed=1.5, basespeed=21f )
                    WalkFastSpeed = 24f, // 18f,//30f, // 63f,
                },
                CollisionResponderType = new CollisionResponderType()
                {
                    AgentCollisionResponderType = new AgentCollisionResponderType()
                }
            },
            SensorType = new SensorType()
            {
                Range = sensorRangeHuman,
                RangeAtNight = sensorRangeHuman,
                DetectionTypeKey = "defaultDetection"
            },

            BodyType = bodyType,
            ContainerType = new AgentStorageType() //0.5f)
            {
                ItemStorageType = new ItemStorageType(0.5f), // can Haul... later.
                StomachStorageType = new ItemStorageType(0.07f) // stomach size is dynamic - is defined in BioType                        
            },
            IntelligenceType = new IntelligenceType()
            {
                MembersScoutingFraction = 1f, //dec 3, was 1f
                IsMobile = true,
                CanAttack = true,
                CanUseWeapons = false,
                CanHunt = false,
                CanScout = true,
                CanPatrol = true,
                CanHaul = false,
                IsPredator = true,
                ContainerTransactTag = "twinklerTransact", //was "twinkler"
                StrengthRating = Entities.StrengthRating.WeakerThanHumans,
                Boldness = 0.25f, //Boldness is low at the moment to allow baby and child twinklersd to flee, the bigger ones should still be able to attack threats //1f
                Courage = 0.05f,   //0.1f 0.2f   0.9f  // 0.05f . wanted them to flee more. 0.05f didn't make them flee either...tweak also vitalBodyPartDamageEvaluationBoost in constants.cs
                MemoryInDays = 2f,
    
             Skills = new SerializableDictionary<string, float> 
            { 
                {
                   "shooting" , 0.7f
                }
            },
             Attacks = new[]
             {
                 "sentryShootGun"
             },

                AggroRange = sensorRangeHuman,
                AssistanceRange = 1000f //
            },
            BiologicalType = new BiologicalType()
            {
                OrderKey = "quaditeOrder",
                OxygenAndMuscleEnergyIncreaseRatePerDay = 12f,
                FoodItemTagsThatCanBeConsumed = new[] { "cookedMeat", "rawMeat", "smallRawMeat", "spoiledMeal", "inedibleMeat", "rottenMeat" },
                ExtractionProcessTypes = new[] { "extractMudWormMeat", "extractPatricianMeat", "extractLeafCutterMeat", "extractThunderChickenMeat", "extractBinalRatMeat", "extractBushDragonMeat", "extractSwampDemonTreeMeat", "extractDemonTreeMeat", "extractMegapodMeat", "extractWhipjawMeat", "extractSpikePlantMeat", "extractForestGuardianMeat", }, //"extractThinThunderChickenMeat",
                TimeToConsumeFullMealInDays = 0.005f, //was 0.009f dec 2014.   was 0.015f MP
                StomachSizeFractionOfEntityBulk = 0.3f, // big stomach so can eat quickly // 0.2f,
                StomachContentsDecreaseRatePerDay = 3,
                ActiveStealthRating = 0.2f,
                      
                MaxRegainLimit = humanMaxRegainLimit,
                FractionOfMaxHitpointsGainedPerDay = humanFractionOfMaxHitpointsGainedPerDay,
                Carcass = "item:quaditeCarcass",

                ResilienceMean = 7f,
                ResilienceStandardDeviation = 0.08f,

                BioPropertyTypes = new[]{                       
                 new BioPropertyType(){
                  KeyName = "SensorRange", //measured in pixels  (White)    //aggrorange and assistancerange can be seen in debug panel>ranges, however if very big, will not be seen. see: public void DrawRanges() for colors.
                  InterpolateSetting = BioPropertyType.Interpolate.DefaultPriority                     
             },
              new BioPropertyType(){
                  KeyName = "SensorRangeAtNight", //measured in pixels
                  InterpolateSetting = BioPropertyType.Interpolate.DefaultPriority                     
             },
             new BioPropertyType(){ 
                  KeyName = "AggroRange", //measured in pixels  (red)
                  InterpolateSetting = BioPropertyType.Interpolate.DefaultPriority                     
             },
             new BioPropertyType(){
                  KeyName = "AssistanceRange", //measured in pixels  (green)
                  InterpolateSetting = BioPropertyType.Interpolate.DefaultPriority                     
             }},

                Castes = new List<CasteType>() 
        {                     
                    
            new CasteType() {
                KeyName = "male",
                Reproduction = Reproduction.Male, //Edge = 1f,
                HeightMean = .70f, HeightStandardDeviation = 0.02f, 
                WeightMean = 25f, WeightStandardDeviation = 2f,
                PrimaryColor = "564920".ToColorVector3(), ModelBasicTextureName = "QuaditeStripedTexture", 
                Edge = 0.6f, 
                ModelScale=2.3f,
                                                   

                AgeGroupTypes = new List<AgeGroupType>() 
                { 
                    new AgeGroupType() 
                    { 
                        Name = "Puppy", AIAgeGroup = AIAgeGroup.Baby, CanReproduce = false, Edge = 0.5f, HeightTargetModifier = 0.05f, WeightTargetModifier = 0.03f/*, 
                        NeedTypes = new NeedType[]{ babySleepNeed }*/
                /*    } ,
                    new AgeGroupType() 
                    { 
                        Name = "Puppy", AIAgeGroup = AIAgeGroup.Child, CanReproduce = false, Edge = 1f, HeightTargetModifier = 0.1f, WeightTargetModifier = 0.1f  
                     // , NeedTypes = new NeedType[]{ childSleepNeed }
                    },     
                    new AgeGroupType() 
                    { 
                        Name = "Young", AIAgeGroup = AIAgeGroup.YoungAdult, CanReproduce = true, Edge = 4f, HeightTargetModifier = 0.8f, WeightTargetModifier = 0.8f  
                     // , NeedTypes = new NeedType[]{ childSleepNeed }
                    },  
                    new AgeGroupType() 
                    { 
                        Name = "Grown", AIAgeGroup = AIAgeGroup.Adult, CanReproduce = true, Edge = 14f, HeightTargetModifier = 1f, WeightTargetModifier = 1f,
                        NeedTypes = new []{ twinklerProteinNeed }
                     // , NeedTypes = new NeedType[]{ adultSleepNeed }
                    }
                    ,
                    new AgeGroupType() 
                    { 
                        Name = "Old", AIAgeGroup = AIAgeGroup.Old, CanReproduce = false, Edge = 20f, HeightTargetModifier = 0.9f, WeightTargetModifier = 1f,
                        NeedTypes = new []{ twinklerProteinNeed }
                     // , NeedTypes = new NeedType[]{ oldSleepNeed }
                    }
                } 
            },
            new CasteType() {
                KeyName = "female",
                Reproduction = Reproduction.Female, //Edge = 1f,
                HeightMean = .70f, HeightStandardDeviation = 0.02f, 
                WeightMean = 25f, WeightStandardDeviation = 2f,
                PrimaryColor = "564920".ToColorVector3(), ModelBasicTextureName = "QuaditeStripedTexture", 
                Edge = 0.6f, 
                ModelScale=2.1f,
                                                   

                AgeGroupTypes = new List<AgeGroupType>() 
                { 
                    new AgeGroupType() 
                    { 
                        Name = "Puppy", AIAgeGroup = AIAgeGroup.Baby, CanReproduce = false, Edge = 0.5f, HeightTargetModifier = 0.05f, WeightTargetModifier = 0.03f/*, 
                        NeedTypes = new NeedType[]{ babySleepNeed }*/
                /*        } ,
                        new AgeGroupType() 
                        { 
                            Name = "Puppy", AIAgeGroup = AIAgeGroup.Child, CanReproduce = false, Edge = 1f, HeightTargetModifier = 0.1f, WeightTargetModifier = 0.1f  
                         // , NeedTypes = new NeedType[]{ childSleepNeed }
                        },     
                        new AgeGroupType() 
                        { 
                            Name = "Young", AIAgeGroup = AIAgeGroup.YoungAdult, CanReproduce = true, Edge = 4f, HeightTargetModifier = 0.8f, WeightTargetModifier = 0.8f  
                         // , NeedTypes = new NeedType[]{ childSleepNeed }
                        },  
                        new AgeGroupType() 
                        { 
                            Name = "Grown", AIAgeGroup = AIAgeGroup.Adult, CanReproduce = true, Edge = 14f, HeightTargetModifier = 1f, WeightTargetModifier = 1f,
                            NeedTypes = new []{ twinklerProteinNeed }
                         // , NeedTypes = new NeedType[]{ adultSleepNeed }
                        }
                        ,
                        new AgeGroupType() 
                        { 
                            Name = "Old", AIAgeGroup = AIAgeGroup.Old, CanReproduce = false, Edge = 20f, HeightTargetModifier = 0.9f, WeightTargetModifier = 1f,
                            NeedTypes = new []{ twinklerProteinNeed }
                         // , NeedTypes = new NeedType[]{ oldSleepNeed }
                        }
                    } 
                } 
            }

                }
            });


        */
                #endregion
                #endregion

                #region Human

                //****** PERSON. HUMAN.

              //  AnimConditionInfo.TemporaryAttachable[] rightHandBoxAttach = new AnimConditionInfo.TemporaryAttachable[] { new AnimConditionInfo.TemporaryAttachable() { RenderableTypeKey = "box", AttacheePoint = AttacheePoint.RightHand, AttachorTag = "rightHand" } };
                             
                const float pickupHeavyActionPointDuration = 0.32f;
                const float pickupLightActionPointDuration = 0.56f;

                bodyType = GameData.Instance.AllBodyTypes["humanoid"];
                EntityType humanType = new EntityType("entity:human")
                {
                    ShowMarkerWindowSetting = EntityType.ShowMarkerWindowMode.Always,
                    //AlwaysShowStatus = true,
                    Name = "Human",  // TODO: show allegiance/expedition/scenario info some other way. //"PRECOL explorer",
                    SummaryDescription = "", // "Participant in the PRECOL (Precolony) mission",
                    Description = "", 
                    UseTypeNameForDisplay = false,
                    RenderableType = new RenderableType()             
               {
                   //if we want to set them on fire:   
                   //   ParticleEmitterTypes = new ParticleEmitterType[] { new ParticleEmitterType(){ ParticleSystemKey = "smallSmoke" }   },
                   RenderAsModelType = new RenderAsModelType()
                   {
                       AssetName = "man", // will be overridden, only used in validation
                       GaitAnimations = new XmlDictionary<string, GaitAnimationBracket[]>
                       {
                           /* WalkSlowSpeed = 25f, //10f, // 8f, 
                            HaulSpeed = 32f,
                            WalkNormalSpeed = 45f, // , (NORMAL man walk speed=1.0, basespeed=45f) ;  (SLOW man walk speed=0.7, basespeed=25f) ;  (FAST man walk speed=1.3, basespeed=60f)                
                            //   (NORMAL man run speed=1.0, basespeed=140f) ;   (SLOW man run speed=0.9, basespeed=120) ; (FAST man run speed=1.2, basespeed=160f)
                            WalkFastSpeed = 60f, // 24f, // 18f,//30f, // 63f,
                            RunSpeed = 120f, // 60f // ???
                            */

                         // MP: The gaitbrackets here must correspond to the base speeds defined in  LeggedLocomotorType  waaay further down (search for word: briskly)!! base speeds define the speed they use for different purposes (?)

                           { "normal", new[]{ new GaitAnimationBracket(){ MinimumSpeed = 0f, MaximumSpeed = 16f, StrideLength = 9f, StrideDuration = 0.96f, AnimationKey = "gaitSlowWalk"},
                                              new GaitAnimationBracket(){ MinimumSpeed = 14f, MaximumSpeed = 37f, StrideLength = 18f, StrideDuration = 0.96f, AnimationKey = "gaitStroll"}, //stride length here is just estimated, not very precise (MP 3 sep 2013)
                                              new GaitAnimationBracket(){ MinimumSpeed = 35f, MaximumSpeed = 67f, StrideLength = 26f, StrideDuration = 0.96f, AnimationKey = "gaitWalk"},
                                              new GaitAnimationBracket(){ MinimumSpeed = 64f, MaximumSpeed = 106f, StrideLength = 32f, StrideDuration = 0.96f, AnimationKey = "gaitJog"}, 
                                              new GaitAnimationBracket(){ MinimumSpeed = 102f, MaximumSpeed = 180f, StrideLength = 46f, StrideDuration = 0.96f, AnimationKey = "gaitRun"}} },

                           { "haulHeavy", new[]{ new GaitAnimationBracket(){ MinimumSpeed = 0f, MaximumSpeed = 40f, StrideLength = 16f, StrideDuration = 0.96f, AnimationKey = "haulHeavy"},  // 
                                              new GaitAnimationBracket(){ MinimumSpeed = 38f, MaximumSpeed = 106f, StrideLength = 32f, StrideDuration = 0.96f, AnimationKey = "gaitJog"},
                                              new GaitAnimationBracket(){ MinimumSpeed = 102f, MaximumSpeed = 180f, StrideLength = 46f, StrideDuration = 0.96f, AnimationKey = "gaitRun"}} },    
                           
                            { "haulHeavyFar", new[]{ new GaitAnimationBracket(){ MinimumSpeed = 0f, MaximumSpeed = 40f, StrideLength = 22f, StrideDuration = 0.96f, AnimationKey = "haulHeavyBack"},  //mp feb 20, 2015: since it uses same base speed as haulHeavy (HaulSpeed = 40f) I just adjust the stridelength to make it fit with the anim.
                                              new GaitAnimationBracket(){ MinimumSpeed = 38f, MaximumSpeed = 106f, StrideLength = 32f, StrideDuration = 0.96f, AnimationKey = "gaitJog"},
                                              new GaitAnimationBracket(){ MinimumSpeed = 102f, MaximumSpeed = 180f, StrideLength = 46f, StrideDuration = 0.96f, AnimationKey = "gaitRun"}} },      
                        
                           { "sneak",  new[]{ new GaitAnimationBracket(){ MinimumSpeed = 0f, MaximumSpeed = 16f, StrideLength = 9f, StrideDuration = 0.96f, AnimationKey = "gaitSlowWalk"},
                                              new GaitAnimationBracket(){ MinimumSpeed = 14f, MaximumSpeed = 67f, StrideLength = 24f, StrideDuration = 0.96f, AnimationKey = "gaitSneak"},  // TODO: stridelength
                                              new GaitAnimationBracket(){ MinimumSpeed = 64f, MaximumSpeed = 106f, StrideLength = 32f, StrideDuration = 0.96f, AnimationKey = "gaitJog"}, 
                                              new GaitAnimationBracket(){ MinimumSpeed = 102f, MaximumSpeed = 180f, StrideLength = 46f, StrideDuration = 0.96f, AnimationKey = "gaitRun"}} },

                           { "wounded",  new[]{ new GaitAnimationBracket(){ MinimumSpeed = 0f, MaximumSpeed = 16f, StrideLength = 9f, StrideDuration = 0.96f, AnimationKey = "gaitSlowWalk"},
                                              new GaitAnimationBracket(){ MinimumSpeed = 14f, MaximumSpeed = 67f, StrideLength = 18f, StrideDuration = 0.96f, AnimationKey = "gaitLimpWalk"},  // TODO: stridelength
                                              new GaitAnimationBracket(){ MinimumSpeed = 64f, MaximumSpeed = 106f, StrideLength = 32f, StrideDuration = 0.96f, AnimationKey = "gaitJog"}, 
                                              new GaitAnimationBracket(){ MinimumSpeed = 102f, MaximumSpeed = 180f, StrideLength = 46f, StrideDuration = 0.96f, AnimationKey = "gaitRun"}} },

                             { "fatigued",  new[]{ new GaitAnimationBracket(){ MinimumSpeed = 0f, MaximumSpeed = 16f, StrideLength = 9f, StrideDuration = 0.96f, AnimationKey = "gaitSlowWalk"},
                                              new GaitAnimationBracket(){ MinimumSpeed = 14f, MaximumSpeed = 67f, StrideLength = 20f, StrideDuration = 0.96f, AnimationKey = "gaitFatiguedWalk"},  // TODO: stridelength
                                              new GaitAnimationBracket(){ MinimumSpeed = 64f, MaximumSpeed = 106f, StrideLength = 32f, StrideDuration = 0.96f, AnimationKey = "gaitJog"}, 
                                              new GaitAnimationBracket(){ MinimumSpeed = 102f, MaximumSpeed = 180f, StrideLength = 46f, StrideDuration = 0.96f, AnimationKey = "gaitRun"}} },

                       },
                       DefaultInfo = new AnimConditionInfo()
                        {
                            // use this neutral anim when all flags are cleared:
                            SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "idle" } }
                        },
                       DefaultStances = new[] // "filler" anims used instead of idle when a stance has been set, but no action yet (happens between goals). This prevents unwanted switching to standing from kneeling, for instance
                        {
                            new  AnimConditionInfo()
                            {
                                 SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "idle" } }   
                            },
                            new  AnimConditionInfo()
                            {
                                 ConditionSet = new AnimConditions(){ Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Kneeling )},                              
                                 SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "kneel" } }   
                            },
                            new  AnimConditionInfo()
                            {
                                 ConditionSet = new AnimConditions(){ Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Sitting )},                              
                                 SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "sittingIdle" } }           
                            },
                            new  AnimConditionInfo() // new - make one for lying also.
                            {
                                 ConditionSet = new AnimConditions(){ Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Lying )},                              
                                 SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "sleep" } }           
                            }
                        },
                       #region AnimConditions
                       AnimConditions = new AnimConditionInfo[] 
                        {

                          new AnimConditionInfo()
                            {                               
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Moving },                             
                                GaitSetKey = "normal"
                            },
                            new AnimConditionInfo() // haul light - - clutching arm
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ AdditionalAnimations1 = new string[]{"haulLight"}},
                                GaitSetKey = "normal",
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Hauling},
                                Forbiddens = new BitMask64(typeof(AnimModifier), (int)AnimModifier.HaulHeavy)
                            },     
                            new AnimConditionInfo() // haul light long distance - same as above, added to prevent match with HaulHeavy
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ AdditionalAnimations1 = new string[]{"haulLight"}},
                                GaitSetKey = "normal",
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Hauling, Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Far )},
                                Forbiddens = new BitMask64(typeof(AnimModifier), (int)AnimModifier.HaulHeavy)
                            },                  
                            new AnimConditionInfo() // haul heavy - box anim
                            {                                
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Hauling, 
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.HaulHeavy )},
                                GaitSetKey = "haulHeavy", 
                              //  TemporaryRenderablesToAttach = rightHandBoxAttach 
                             
                            },
                            new AnimConditionInfo() // haul heavy long distance
                            {                                
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Hauling, 
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.HaulHeavy, (int)AnimModifier.Far )},
                                GaitSetKey = "haulHeavyFar" //
                            },
                            new AnimConditionInfo() // fatigued walk
                            {
                                GaitSetKey = "fatigued",
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Moving,
                                                                Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Fatigued )}                               
                            },
                             new AnimConditionInfo() // walking wounded
                            {
                                GaitSetKey = "wounded",
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Moving,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Damaged )}                              
                            },
                            new AnimConditionInfo() // 
                            {
                                GaitSetKey = "sneak",
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Moving,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Stealthy )}
                            },


                            // whole pickup anims:
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "pickupLight" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.PickingUp},
                                Looping = Looping.No,
                                StartingPoint = StartingPoint.FromBeginning                              
                            },                          
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "pickupHeavy" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.PickingUp,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Heavy )},
                                Looping = Looping.No,
                                StartingPoint = StartingPoint.FromBeginning
                            },                         
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "pickupMounted" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.PickingUp, 
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Mount )},
                                Looping = Looping.No,
                                StartingPoint = StartingPoint.FromBeginning 
                            },
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "pickupEquipped" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.PickingUp,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Equip )},
                                Looping = Looping.No,
                                StartingPoint = StartingPoint.FromBeginning 
                            },
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "pickupMountedEquipped" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.PickingUp, 
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Mount, (int)AnimModifier.Equip )},
                                Looping = Looping.No,
                                StartingPoint = StartingPoint.FromBeginning 
                            },
                             new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{ "pickupHeavy"/*  "pickupLightSame"*/ }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.PickingUp,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Same )},
                             //   TemporaryRenderablesToAttach = rightHandBoxAttach,       
                                Looping = Looping.No,
                                StartingPoint = StartingPoint.FromBeginning 
                            },
                            // 2nd half of pickup:
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "pickupHeavy" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.PickingUp, 
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Heavy, (int)AnimModifier.Post )},
                               // TemporaryRenderablesToAttach = rightHandBoxAttach,                             
                                Looping = Looping.No,
                                StartingPoint = StartingPoint.Specified,
                                StartingPointInSeconds = pickupHeavyActionPointDuration
                            },   
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "pickupLight" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.PickingUp,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Post )},
                                Forbiddens = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Heavy),
                                Looping = Looping.No,
                                StartingPoint = StartingPoint.Specified,
                                StartingPointInSeconds = pickupLightActionPointDuration
                            },   
                            //***** drop anims:
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "dropLight" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dropping},
                                                                 
                                Looping = Looping.No,
                                StartingPoint = StartingPoint.FromBeginning                              
                            },   
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "dropLightSame" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dropping,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Same )},
                                Looping = Looping.No,
                                StartingPoint = StartingPoint.FromBeginning                              
                            },   
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "dropHeavy" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dropping,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Heavy )},
                              //  TemporaryRenderablesToAttach = rightHandBoxAttach,       
                                Looping = Looping.No,
                                StartingPoint = StartingPoint.FromBeginning                              
                            },              
                             new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "dropMounted" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dropping, 
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Mount )},
                                Looping = Looping.No,
                                StartingPoint = StartingPoint.FromBeginning                              
                            },  
                             new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "dropMountedSame" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dropping,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Mount, (int)AnimModifier.Same )},
                                Looping = Looping.No,
                                StartingPoint = StartingPoint.FromBeginning                              
                            },  
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "dropEquipped" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dropping,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Equip )},
                                Looping = Looping.No,
                                StartingPoint = StartingPoint.FromBeginning                              
                            },  
                           
#region Stance changes / breakdowns

                            // we cannot use flag order to specify direction. Instead we use the Reverse flag and define the
                             //default direction as "get up". so, sitting down needs "reverse flag".
                              new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "kneelToIdle" }}, // stand up from kneeling
                                ConditionSet = new AnimConditions(){ Action = AnimAction.ChangingStance, 
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Kneeling)}, 
                                                                 Forbiddens = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Sitting),
                                Looping = Looping.No
                            },  

                             new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "sittingToIdle" }}, // stand up from sitting
                                ConditionSet = new AnimConditions(){ Action = AnimAction.ChangingStance,   
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Sitting)},
                                                                 Forbiddens = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Kneeling),
                                Looping = Looping.No
                            },  

                             new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "sleepToIdle" }}, // stand up from sleeping
                                ConditionSet = new AnimConditions(){ Action = AnimAction.ChangingStance,   
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Lying)},
                                Looping = Looping.No
                            },  

                              new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "idleToKneel" }}, // kneeling from standing
                                ConditionSet = new AnimConditions(){ Action = AnimAction.ChangingStance,  
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Kneeling, (int)AnimModifier.Reverse)}, 
                                                                 Forbiddens = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Sitting),
                                Looping = Looping.No
                            },   
                              new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "idleToSleep" }}, // standing to lying (sleeping)
                                ConditionSet = new AnimConditions(){ Action = AnimAction.ChangingStance,  
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Lying, (int)AnimModifier.Reverse)
                                },
                                Looping = Looping.No
                            },   
                                new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "idleToSitting" }}, // sitting from standing
                                ConditionSet = new AnimConditions(){ Action = AnimAction.ChangingStance, 
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Sitting, (int)AnimModifier.Reverse)},  //default direction is "get up" so, sitting down needs "reverse flag"
                                                                 Forbiddens = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Kneeling),
                                Looping = Looping.No
                            },   

                                new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "kneelToSitting" }}, // sitting from kneeling
                                ConditionSet = new AnimConditions(){ Action = AnimAction.ChangingStance,  
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Sitting, (int)AnimModifier.Kneeling, (int)AnimModifier.Reverse)}, //default direction is "get up" so, sitting down needs "reverse flag"
                                                            

                                Looping = Looping.No
                            },   


#endregion

#region IDLE 
                          

                                 new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "kneel", "kneelCollectSoil", "kneelExamineSoil", "kneelTablet", "kneelWaterDevice" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Kneeling)},
                                Looping = Looping.Yes
                            },  


                            //MP: something wrong with position of tablet, it floats in the air
             /*                new AnimConditionInfo()
                            {
                                AnimationSet = new RandomAnimationSet(){ BaseAnimations = new string[]{  "sittingTablet" }},  //  use of a renderable tablet (right hand) which is not an item (similar to the haul heavy box)
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Sitting)},                                
                                Looping = Looping.Yes,
                                TemporaryRenderablesToAttach = new[]{ new AnimConditionInfo.TemporaryAttachable(){ RenderableTypeKey = "tablet", AttachorTag = "rightHand" }},  //attachorTag necessary to type in because the tablet is not an item but just a renderable. (in items this is defined)
                                AttachPoints = new[]{ 
                                    new AnimConditionInfo.AttachPointData() { AttacheePoint = AttacheePoint.Hand,
                                         RenderableTypeKey = "tablet", Translation = new Vector3(-29.764f, 15.591f, 8.031f), Rotation = new Vector3(1.601f, 0.709f, 0.026f) }},  //MP: something wrong with position of tablet, it floats in the air
                            },  
             */

                                 new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "kneelTablet" }},  //  use of a renderable tablet  (left hand) which is not an item (similar to the haul heavy box)
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Kneeling)},                                
                                Looping = Looping.Yes,
                                TemporaryRenderablesToAttach = new[]{ new AnimConditionInfo.TemporaryAttachable(){ RenderableTypeKey = "tablet", AttachorTag = "leftHand" }},  //attachorTag necessary to type in because the tablet is not an item but just a renderable. (in items this is defined)
                                AttachPoints = new[]{ 
                                    new AnimConditionInfo.AttachPointData() { AttacheePoint = AttacheePoint.RightHand,
                                         RenderableTypeKey = "tablet", Translation = new Vector3(1.601f, 0.026f, 0.184f), Rotation = new Vector3(-67.559f, 167.717f, -33.543f) }},  
                            },  

                            new AnimConditionInfo()  
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{ "idle", "idleLong", "idleHandsOnHips", "idleStretchesNeck",  "idleWipesNose" }},  // MP: "idleStretchesBody" should only be played very rarely
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle},                                 
                                Forbiddens = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Low) //forbidden low animations while standing
                               
                            },
                            
                            new AnimConditionInfo() 
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{ "idleTalkShort", "idleTalkArgue" }}, // take pauses?
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle,                                 
                                                                     Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Talk)},
                               
                            },               
                      
                             new AnimConditionInfo() 
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{ "exult" }}, 
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle,                                 
                                                                     Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Happy)},// mp may 2015- never seen this used. probably best.
                                Looping = Looping.No
                               
                            },   
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{ "sittingIdle", "sittingAttentive", "sittingMend" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Sitting)},
                                Looping = Looping.Yes
                            },
                             new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "kneel" }}, // for idling in dangerous areas
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Kneeling, (int)AnimModifier.Trouble)},
                                Looping = Looping.Yes
                            },

#endregion // END IDLE

                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "kneel", "kneelShieldEyes", "kneelExamineSoil", "kneelCollectSoil", "kneelTablet" }}, 
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Scouting,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Kneeling)}
                            },     
                             new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "scout" }}, 
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Scouting }
                            },    
                           
                            //new AnimConditionInfo()
                            //{
                            //    animControllerKeys = new string[] { "smoking" },
                            //    Conditions = new ConditionSet(){ Modifiers = new BitMask64(typeof(AnimState), (int)AnimState.Idle, (int)AnimState.Smoking)
                            //},
                             new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "sleep" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Sleeping,
                                     Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Lying)},
                                Looping = Looping.Yes  
                            },
                         
                             new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "mend" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Reloading,
                                    Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Kneeling)},

                               // Looping = Looping.Yes//,
                              //  Playback = Playback.Manual
                            },
                             new AnimConditionInfo() // lighting fires, cooking...
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "mend" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Mending,
                                    Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Kneeling)},
                                Looping = Looping.Yes,
                                //Forbiddens = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Machete, (int)AnimModifier.Bow, (int)AnimModifier.Rifle, (int)AnimModifier.Spear, (int)AnimModifier.Watergun),    //mp don't use this anymore, it's a patchy hacky thing                 
                            },
                            new AnimConditionInfo() // Crafting
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "mend", "mend" },
                                Sounds = new string [] { "activities/crafting/craftingGeneral1", "activities/crafting/craftingGeneral2" }}, //MS: I set animmodifier to Improvised on all crafting jobs in the ProcessLoader. That should make this animation and sound play when crafting.
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Mending,
                                    Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Kneeling, (int)AnimModifier.Improvised)},
                                Looping = Looping.Yes,
                                //Forbiddens = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Machete, (int)AnimModifier.Bow, (int)AnimModifier.Rifle, (int)AnimModifier.Spear, (int)AnimModifier.Watergun),    //mp don't use this anymore, it's a patchy hacky thing                 
                            },

#region Building


#region Build while standing
                               new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "constructPull", "idleSweat", "idleStretchesNeck", "chopLow", "idleWipesNose" },
                                },
                                //Sound = "activities/building/buildingSteel",
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Building},
                                Looping = Looping.Yes
                            },
                            new AnimConditionInfo()
                            {
                                //BUILD STANDING WITH METAL COMPONENTS
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "constructPull", "idleSweat", "idleStretchesNeck", "chopLow", "idleWipesNose" },
                                    Sounds = new string[]{ "activities/salvage/salvagePullMetal", "", "", "activities/gather/gatherChopLow", "" }}, //PLACEHOLDER
                                //Sound = "activities/building/buildingSteel",
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Building,
                                                            Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Metal )},
                              
                                Looping = Looping.Yes
                            },
                            new AnimConditionInfo()
                            {
                                //BUILD STANDING WITH ELECTRONIC COMPONENTS
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "constructPull", "idleSweat", "idleStretchesNeck", "chopLow", "idleWipesNose" },
                                    Sounds = new string[]{ "activities/building/buildingElectronic1", "", "", "activities/building/buildingElectronic2", "" }}, //PLACEHOLDERS
                                //Sound = "activities/building/buildingSteel",
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Building,
                                                            Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Electronic )},
                              
                                Looping = Looping.Yes
                            },

                            new AnimConditionInfo()
                            {
                                //BUILD STANDING WITH NATURAL COMPONENTS
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "constructPull", "idleSweat", "idleStretchesNeck", "chopLow", "idleWipesNose" },
                                    Sounds = new string[]{ "activities/building/buildingImprovisedKneelWaterDevice", "", "", "activities/gather/gatherChopLow", "" }}, //GATHERCHOPLOW IS PLACEHOLDER
                                //Sound = "activities/building/buildingSteel",
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Building,
                                                            Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Improvised )},
                              
                                Looping = Looping.Yes
                            },
                            new AnimConditionInfo()
                            {
                                //BUILD STANDING WITH TARP
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "constructPull", "idleSweat", "idleStretchesNeck", "chopLow", "idleWipesNose" },
                                    Sounds = new string[]{ "activities/building/buildingTarpKneelWaterDevice", "", "", "activities/building/buildingTarpKneelDig", "" }}, //PLACEHOLDERS
                                //Sound = "activities/building/buildingSteel",
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Building,
                                                            Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Tarp )},
                              
                                Looping = Looping.Yes
                            },

                            new AnimConditionInfo()
                            {
                                //BUILD STANDING WITH TARP AND IMPROVISED
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "constructPull", "idleSweat", "idleStretchesNeck", "chopLow", "idleWipesNose" },
                                    Sounds = new string[]{ "activities/building/buildingTarpImprovisedKneelWaterDevice", "", "", "activities/building/buildingTarpImprovisedMend", "" }}, //PLACEHOLDERS
                                //Sound = "activities/building/buildingSteel",
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Building,
                                                            Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Tarp, (int)AnimModifier.Improvised )},
                              
                                Looping = Looping.Yes
                            },
    #endregion


#region Build in kneeling stance   
                            new AnimConditionInfo()
                            {
                                // BUILDING KNEELED WITH NATURAL COMPONENTS
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "mend", "kneelDig", "kneelWaterDevice" },
                                    Sounds = new string[]{ "activities/building/buildingImprovisedMend", "activities/building/buildingImprovisedKneelDig", "activities/building/buildingImprovisedKneelWaterDevice" }}, //FINAL
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Building,
                                                            Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Improvised, (int)AnimModifier.Kneeling )}, //Kneeling
                                Looping = Looping.Yes
                            },

                            new AnimConditionInfo()
                            {
                                //BUILDING KNEELED WITH TARP AND NATURAL COMPONENTS
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "mend", "kneelDig", "kneelWaterDevice" },
                                    Sounds = new string[]{ "activities/building/buildingTarpImprovisedMend", "activities/building/buildingTarpImprovisedKneelDig", "activities/building/buildingTarpImprovisedKneelWaterDevice" }}, //PLACEHOLDERS
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Building,
                                                            Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Improvised, (int)AnimModifier.Tarp, (int)AnimModifier.Kneeling )},
                                Looping = Looping.Yes
                            },

                            new AnimConditionInfo()
                            {
                                //BUILDING KNEELED WITH TARP
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "mend", "kneelDig", "kneelWaterDevice" },
                                    Sounds = new string[]{ "activities/building/buildingTarpMend", "activities/building/buildingTarpKneelDig", "activities/building/buildingTarpKneelWaterDevice" }}, //PLACEHOLDERS
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Building,
                                                            Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Tarp, (int)AnimModifier.Kneeling )},
                                Looping = Looping.Yes
                            },

                            new AnimConditionInfo()
                            {
                                // BUILDING KNEELED WITH ELECTRONIC COMPONENTS
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "mend", "kneelDig", "kneelWaterDevice" }, 
                                    Sounds = new string[]{ "activities/building/buildingElectronic1", "activities/building/buildingElectronic2", "activities/building/buildingElectronic1" }}, //PLACEHOLDERS
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Building,
                                                            Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Electronic, (int)AnimModifier.Kneeling )},
                                Looping = Looping.Yes
                            },
                            new AnimConditionInfo()
                            {
                                // BUILDING KNEELED WITH METAL COMPONENTS
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "mend", "kneelDig", "kneelWaterDevice" }, 
                                    Sounds = new string[]{ "activities/building/buildingMetalMend", "activities/building/buildingMetalKneelDig", "activities/building/buildingMetalKneelWaterDevice" }}, //PLACEHOLDERS
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Building,
                                                            Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Metal, (int)AnimModifier.Kneeling )},
                                Looping = Looping.Yes
                            },
#endregion


#region Build with tools
                            //MS: I have not had the animationsets below occur yet. I will create the build sounds above here, and after that maybe look at creating tool building sounds.
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ 
                                    BaseAnimations = new string[]{ "constructPull", "cutLow", "idleSweat", "idleStretchesNeck", "idleWipesNose" },
                                    Sounds = new string[]{ "", "activities/building/buildingSteel", "", "", "" } },                              
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Building,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Knife )},
                                Looping = Looping.Yes
                            },

                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ 
                                    BaseAnimations = new string[]{ "mend", "kneelDig" },
                                    Sounds = new string[]{ "activities/building/buildingSteel", "" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Building,
                                                            Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Knife, (int)AnimModifier.Kneeling )},
                                Looping = Looping.Yes
                            },

                            // NEW: build/repair clay hut with shovel - copied from digging with shovel
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ 
                                    BaseAnimations = new string[]{  "shovel",  "shovel", "gather" }, //necessary to have both a standing and a kneeling anim in the mix because processes is defined as kneelingOrStandingProduction. Else he will just kneel superquickly all the time, looks weird
                                    Sounds = new string[] { "activities/farming/farmingHoe1A", "activities/farming/farmingHoe1B" , "activities/gather/gatherHandGather", }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Building,
                                                            Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Shovel, /*(int)AnimModifier.Kneeling,*/ (int)AnimModifier.Improvised )},
                                Looping = Looping.Yes
                            },

                           

                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "constructPull", "chopLow", "idleSweat", "idleStretchesNeck" }, //
                                    Sounds = new string[]{ "activities/building/buildingImprovisedKneelWaterDevice","activities/gather/gatherChopLow", "", "" }}, //mp todo
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Building,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Hammer, (int)AnimModifier.Low)},
                                AttachPoints = new[]{                                                             
 
                                         new AnimConditionInfo.AttachPointData() { AttacheePoint = AttacheePoint.RightHand, //mp coords for chopLow for hammer
                                         RenderableTypeKey = "hammer", Translation = new Vector3(-1.942f, 0.157f, 0.052f), Rotation = new Vector3(112.913f, -180f, -110.079f) }  
                                },  
                                Looping = Looping.Yes
                            },

                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "chopLow" },
                                    Sounds = new string[]{ "activities/building/buildingHammer" }}, //mp todo
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Building,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Hammer, (int)AnimModifier.Low)}, //
                                AttachPoints = new[]{                                                             
 
                                         new AnimConditionInfo.AttachPointData() { AttacheePoint = AttacheePoint.RightHand,
                                         RenderableTypeKey = "hammer", Translation = new Vector3(-1.942f, 0.157f, 0.052f), Rotation = new Vector3(112.913f, -180f, -110.079f) }  
                                },              
                                Looping = Looping.Yes
                            },

                            new AnimConditionInfo() //hammering standing up, at the anvil  (..NOTE, "hammering" anim could be used whith knife for chopping vegetables also.)
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "hammering", "standingMend" }, //mp "standingMend" is optional... 
                                    Sounds = new string[]{ "activities/building/buildingHammer", "" }}, //mp todo. should have metal hammer sound.
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Mending, //Note
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Hammer, (int)AnimModifier.Metal)}, 
                                AttachPoints = new[]{                                                             
 
                                         new AnimConditionInfo.AttachPointData() { AttacheePoint = AttacheePoint.RightHand,
                                         RenderableTypeKey = "hammer", Translation = new Vector3(-1.942f, 0.157f, 0.052f), Rotation = new Vector3(112.913f, -180f, -110.079f) }  
                                },  
            
                                Looping = Looping.Yes
                            },



#endregion

#endregion
                               new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "constructPull", "chopLow", "idleWipesNose", "gather" },
                                 Sounds = new string[]{ "activities/butcher/butcherConstructPull", "activities/butcher/butcherChopLow", "activities/butcher/butcherSharpen", "activities/butcher/butcherGather" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Butchering,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Machete )},
                                Looping = Looping.Yes
                            },

                                 new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "constructPull", "cutLow", "idleWipesNose", "gather" },
                                 Sounds = new string[]{ "activities/butcher/butcherConstructPull", "activities/butcher/butcherCutLow", "activities/butcher/butcherSharpen", "activities/butcher/butcherGather" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Butchering,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Knife )},
                                Looping = Looping.Yes
                            },

                                new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "constructPull", "cutLow", "idleSweat", "attackStompRight", "constructPull", "cutLow"},
                                    Sounds = new string [] { "activities/salvage/salvagePullMetal", "activities/salvage/salvageMetalCutLow", "", "activities/salvage/salvageBreakMetal2", "activities/salvage/salvagePullMetal", "activities/salvage/salvageMetalCutLow"}},
                                //Sound = "activities/salvage/salvageAll",
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Salvaging},
                                                               
                                Looping = Looping.Yes
                            },
#region tilling
                            new AnimConditionInfo() //tilling with hoe.   MP how does it know not to attach the hoe when he plays the "gather" anim??
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ 
                                    BaseAnimations = new string[]{  "useHoe", "idleSweat", "gather", "useHoe" }, //mp hmm it never plays 2 in a row..so the hoeing cycle is kinda short.
                                    Sounds = new string[] { "activities/farming/farmingHoe1A", "", "activities/gather/gatherHandGather", "activities/farming/farmingHoe1B"  }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Tilling,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Hoe )},
                AttachPoints = new[]{ 
                                    new AnimConditionInfo.AttachPointData() { AttacheePoint = AttacheePoint.RightHand,
                                         RenderableTypeKey = "farmingHoe",  Translation = new Vector3(1.916f, 4.331f, -0.026f), Rotation = new Vector3(92.12601f, -168.661f, 167.717f) }},
                                Looping = Looping.Yes
                            },

                            new AnimConditionInfo() //tilling with shovel
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ 
                                    BaseAnimations = new string[]{  "shovel", "idleWipesNose", "gather", "shovel" }, //mp:  it never plays 2 in a row
                                    Sounds = new string[] { "activities/farming/farmingHoe1A", "", "activities/gather/gatherHandGather", "activities/farming/farmingHoe1B"  }},//pasted from hoe
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Tilling,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Shovel )},
                                AttachPoints = new[]{ 
                                    new AnimConditionInfo.AttachPointData() { AttacheePoint = AttacheePoint.RightHand,
                                         RenderableTypeKey = "shovel",  Translation = new Vector3(2.467f, 2.362f, -0.367f), Rotation = new Vector3(92.12601f, -168.661f, 162.992f) }}, 
                                Looping = Looping.Yes
                            },
#endregion


                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{ "eat" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Eating},
                                Looping = Looping.Yes  
                            },                        
                           /* new AnimConditionInfo()
                            {
                                AnimationSet = new RandomAnimationSet(){ BaseAnimations = new string[]{  "eat" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Eating,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Sitting)},
                                Looping = Looping.Yes  
                            },*/
                         
                                new AnimConditionInfo() //note: there's also one for digging with hands/trowel
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "gather", "kneelDig", "kneelExamineSoil" },
                                    Sounds = new string[] { "activities/gather/gatherHandGather", "activities/gather/gatherHandKneelDig", "" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Harvesting,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Kneeling)},
                                Looping = Looping.Yes,
                                Forbiddens = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Machete, (int)AnimModifier.Bow, (int)AnimModifier.Rifle, (int)AnimModifier.Spear, (int)AnimModifier.Watergun), 
                            },

                               new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "chopLow" },
                                    Sounds = new string[]{ "activities/gather/gatherChopLow" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Harvesting,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Machete, (int)AnimModifier.Axe, (int)AnimModifier.Low)}, 
                                AttachPoints = new[]{ 
                                    new AnimConditionInfo.AttachPointData() { AttacheePoint = AttacheePoint.RightHand,
                                         RenderableTypeKey = "machete", Translation = new Vector3(0.236f, -0.236f, -0.026f), Rotation = new Vector3(139.37f, -177.165f, -102.52f) },

                                    new AnimConditionInfo.AttachPointData() { AttacheePoint = AttacheePoint.RightHand,
                                         RenderableTypeKey = "axe", Translation = new Vector3(-1.942f, 0.157f, 0.052f), Rotation = new Vector3(139.37f, -145.039f, -83.622f) } ,     //mp is 2 tool options allowed?                         

                                },  
            
                                Looping = Looping.Yes
                            },




                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "fishingSpearIdle", "fishingSpearThrustMiss" }},

                                ConditionSet = new AnimConditions(){ Action = AnimAction.Fishing,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Spear) /*, (int)AnimModifier.Low)*/}, // fishing without a spear is "fishingHook"

                                AttachPoints = new[]{ 
                                    new AnimConditionInfo.AttachPointData() { AttacheePoint = AttacheePoint.RightHand,
                                         RenderableTypeKey = "spear",  Translation = new Vector3(0.499f, -1.234f, 0.079f), Rotation = new Vector3(86.457f, 1.417f, -4.252f) }},  
                                Looping = Looping.Yes
                            },

                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "fishingHook" },},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Fishing},
                                Looping = Looping.Yes
                            }, 


#region digging
                             new AnimConditionInfo() //digging with shovel
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ 
                                    BaseAnimations = new string[]{  "shovel",  "shovel", "gather" }, //necessary to have both a standing and a kneeling anim in the mix because processes is defined as kneelingOrStandingProduction. Else he will just kneel superquickly all the time, looks weird
                                    Sounds = new string[] { "activities/farming/farmingHoe1A", "activities/farming/farmingHoe1B" , "activities/gather/gatherHandGather", }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Digging,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Shovel )},
                                AttachPoints = new[]{ 
                                    new AnimConditionInfo.AttachPointData() { AttacheePoint = AttacheePoint.RightHand,
                                         RenderableTypeKey = "shovel",  Translation = new Vector3(2.467f, 2.362f, -0.367f), Rotation = new Vector3(92.12601f, -168.661f, 162.992f) }}, 
                                Looping = Looping.Yes
                            },                          
                       
                                new AnimConditionInfo() //digging with pickaxe
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "pickaxe","pickaxe", "gather"  }, //necessary to have both a standing and a kneeling anim in the mix because processes is defined as kneelingOrStandingProduction. Else he will just kneel superquickly all the time, looks weird
                                    Sounds = new string[]{ "activities/farming/farmingHoe1A", "activities/farming/farmingHoe1B","activities/gather/gatherHandGather", }}, //mp todo
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Digging,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Pickaxe)}, 
                                AttachPoints = new[]{ 
                                    new AnimConditionInfo.AttachPointData() { AttacheePoint = AttacheePoint.RightHand,
                                         RenderableTypeKey = "pickaxe", Translation = new Vector3(-1.627f, -2.677f, -0.367f), Rotation = new Vector3(139.37f, -177.165f, -102.52f)},                           

                                },  
            
                                Looping = Looping.Yes
                            },
    
                                new AnimConditionInfo() //digging with hands or trowel
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "gather", "kneelDig", "kneelExamineSoil", "idleSweat" },//this is for barehanded digging. BUT The agent will do the shovel dig anim with nothing in her hands -  will once in a while stand up and do the shovel anim without shovel - looks weird. I thought this could be avoided by having both a standing and a kneeling anim in the mix because processes is defined as kneelingOrStandingProduction.
                                    Sounds = new string[] { "activities/gather/gatherHandGather", "activities/gather/gatherHandKneelDig", "", "" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Digging,
                                                              Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Kneeling )},
                                
                                Looping = Looping.Yes,
                                Forbiddens = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Machete, (int)AnimModifier.Bow, (int)AnimModifier.Rifle, (int)AnimModifier.Spear, (int)AnimModifier.Watergun), 
                            },

#endregion

                             new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "cutLow" },
                                    Sounds = new string[]{ "activities/gather/gatherCutLow" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Harvesting,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Knife, (int)AnimModifier.Low)},
                                Looping = Looping.Yes
                                //same knife coordinates as idle (defined in GameData.cs)
                            },
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "gatherStand" },
                                    Sounds = new string[]{ "activities/gather/gatherHandGatherStand" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Harvesting},
                                Looping = Looping.Yes
                            },
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "gatherStand" },
                                    Sounds = new string[]{ "activities/gather/gatherHandGatherStand" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Harvesting, 
                                                                     Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Low)}, // to fall back on when no tool is used
                                Looping = Looping.Yes
                            },



#region combat


                            ////// Unarmed

                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "combatIdleUnarmed" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Bold)}
                            },

                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "combatHit" }},
                                Playback = Playback.Manual,
                                //human hit sound
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Recoiling,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Bold)},
                                Looping = Looping.No
                            },

                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "combatHit" }},
                                Playback = Playback.Manual,
                                //human hit sound
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Recoiling},
                                Looping = Looping.No
                            },
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{ "combatCollapse" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Pre)},
                                Looping = Looping.No
                            },
                              new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{ "combatCollapse" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Pre, (int)AnimModifier.Damaged)},
                                Looping = Looping.No
                            },
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{ "combatCollapse" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Pre, (int)AnimModifier.Bold)}  ,
                                Looping = Looping.No
                            },
                             new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{ "combatCollapse" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Pre, (int)AnimModifier.Bold, (int)AnimModifier.Damaged)},
                                Looping = Looping.No
                            },
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{ "dying" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying/*, ** lets wait with stances for death... uses special collapse anim anyway...
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Lying)*/},
                                Looping = Looping.No
                            },
                             new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{ "dying" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying,
                                                                     Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Damaged)
                                    /*, ** lets wait with stances for death... uses special collapse anim anyway...
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Lying)*/},
                                Looping = Looping.No
                            },

                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "dead" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Post)}  
                            },
                             new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "dead" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Dying,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Post, (int)AnimModifier.Damaged)}  
                            },
                              
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "attackPunchRightMiss" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Attacking,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Fail)},
                                Forbiddens = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Rifle, (int)AnimModifier.Bow, (int)AnimModifier.Watergun), // don't play a miss anim when shooting
                                Looping = Looping.No
                            },
                           
                             new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "attackStompRight" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Attacking,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Low,  (int)AnimModifier.Right, (int)AnimModifier.Near, (int)AnimModifier.Extreme)},
                                //Forbiddens = new BitMask64(typeof(AnimState), (int)AnimState.Moving),
                                Looping = Looping.No
                            },
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "attackSnapkickRight" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Attacking,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Low, (int)AnimModifier.Right, (int)AnimModifier.Near)},   
                                //Forbiddens = new BitMask64(typeof(AnimState), (int)AnimState.Moving),
                                Looping = Looping.No

                            },
                           new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "attackPunchLowRight" }},
                                // sound on  KeyName = "personPunchLowRight" in GameDataLoader
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Attacking,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Right, (int)AnimModifier.Near)},  
                                //Forbiddens = new BitMask64(typeof(AnimState), (int)AnimState.Moving),
                                Looping = Looping.No

                            },
                             new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "attackPunchHighRight" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Attacking,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.High, (int)AnimModifier.Right, (int)AnimModifier.Near)},   
                                //Forbiddens = new BitMask64(typeof(AnimState), (int)AnimState.Moving),
                                Looping = Looping.No
                            },
                                          
                
                            ////Knife
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "attackClubHigh" }}, 
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Attacking,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Knife, (int)AnimModifier.Right, (int)AnimModifier.Near)},   
                                //Forbiddens = new BitMask64(typeof(AnimState), (int)AnimState.Moving),
                                //Sound = "melee/STAB2_24 - 4 Stabs With Blood",
                                Looping = Looping.No,
                             /*   AttachPoints = new[]{ 
                                    new AnimConditionInfo.AttachPointData() { AttacheePoint = AttacheePoint.Hand,
                                         RenderableTypeKey = "knife", Translation = new Vector3(0.236f, -0.236f, -0.026f), Rotation = new Vector3(139.37f, -177.165f, -102.52f) }}*/
                            },

                            ////hammer
                                new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "attackClubHigh" }}, 
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Attacking,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Hammer, (int)AnimModifier.Right, (int)AnimModifier.Near)},   
                                //Forbiddens = new BitMask64(typeof(AnimState), (int)AnimState.Moving),
                                Looping = Looping.No,
                                AttachPoints = new[]{ 
                                    new AnimConditionInfo.AttachPointData() { AttacheePoint = AttacheePoint.RightHand,
                                         RenderableTypeKey = "hammer", Translation = new Vector3(-1.942f, 0.157f, -0.367f), Rotation = new Vector3(108.189f, -161.102f, -95.906f) }}//same as axe attack coords
                            },

                            ////axe
                                new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "attackClubHigh" }}, 
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Attacking,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Axe, (int)AnimModifier.Right, (int)AnimModifier.Near)},   
                                //Forbiddens = new BitMask64(typeof(AnimState), (int)AnimState.Moving),
                                Looping = Looping.No,
                                AttachPoints = new[]{ 
                                    new AnimConditionInfo.AttachPointData() { AttacheePoint = AttacheePoint.RightHand,
                                         RenderableTypeKey = "axe", Translation = new Vector3(-1.942f, 0.157f, -0.367f), Rotation = new Vector3(108.189f, -161.102f, -95.906f) }}//same as hammer attack coords
                            },


                            ////Machete
                                    new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "attackClubHigh" }}, 
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Attacking,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Machete, (int)AnimModifier.Right, (int)AnimModifier.Near)},   
                                //Forbiddens = new BitMask64(typeof(AnimState), (int)AnimState.Moving),
                                //Sound = "melee/STAB2_24 - 4 Stabs With Blood",
                                Looping = Looping.No,
                                AttachPoints = new[]{ 
                                    new AnimConditionInfo.AttachPointData() { AttacheePoint = AttacheePoint.RightHand,
                                         RenderableTypeKey = "machete", Translation = new Vector3(0.236f, -0.236f, -0.026f), Rotation = new Vector3(139.37f, -177.165f, -102.52f) }}
                            },


                            ////Pickaxe 
                                    new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "attackPickaxe" }}, 
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Attacking,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Pickaxe, (int)AnimModifier.Right, (int)AnimModifier.Near)},  

                                Looping = Looping.No,
                                AttachPoints = new[]{ 
                                    new AnimConditionInfo.AttachPointData() { AttacheePoint = AttacheePoint.RightHand,
                                         RenderableTypeKey = "pickaxe", Translation = new Vector3(-2.467f, -4.462f, -0.026f), Rotation = new Vector3(86.457f, -177.165f, -103.465f) }}
                            },



                            ///////////Rifle

                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "combatIdleRifle" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle, 
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Rifle, (int)AnimModifier.Bold)},
                                AttachPoints = new[]{ 
                                    new AnimConditionInfo.AttachPointData() { AttacheePoint = AttacheePoint.RightHand,
                                        RenderableTypeKey = "rifle", Translation = new Vector3(1.129f, 0.814f, 0.709f), Rotation = new Vector3(-7.087f, 37.323f, 9.921f) }},
                            },

                             new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "idle" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Rifle)}
                            },

                        //hitting with rifle butt:
                                new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "attackHoeMid" }}, 
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Attacking,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Rifle, (int)AnimModifier.Near)},   
                                //Forbiddens = new BitMask64(typeof(AnimState), (int)AnimState.Moving),
                                Looping = Looping.No,
                                AttachPoints = new[]{ 
                                    new AnimConditionInfo.AttachPointData() { AttacheePoint = AttacheePoint.RightHand,
                                         RenderableTypeKey = "rifle",  Translation = new Vector3(0.9710001f, 5.066f, 1.601f), Rotation = new Vector3(-109.134f, 61.89f, 157.323f) }} 
                            }, 


                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "combatRifleAim" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Attacking,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Rifle, (int)AnimModifier.Far)},   
                                //Forbiddens = new BitMask64(typeof(AnimState), (int)AnimState.Moving),
                                Looping = Looping.No,
                                AttachPoints = new[]{ 
                                    new AnimConditionInfo.AttachPointData() { AttacheePoint = AttacheePoint.RightHand,
                                        RenderableTypeKey = "rifle", Translation = new Vector3(1.129f, 0.814f, 0.709f), Rotation = new Vector3(-7.087f, 37.323f, 9.921f) }},

                         //       ParticleEmitters = new[]{ new ParticleEmitterEffect(){ ParticleSystemKey = "firePlume", EmitParticlesInParentDirection = true }  }
                                
                            },      
                             new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "combatRifleAim" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Attacking,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Rifle, (int)AnimModifier.Far, (int)AnimModifier.Fail)}, 
                                Looping = Looping.No,
                                AttachPoints = new[]{ 
                                    new AnimConditionInfo.AttachPointData() { AttacheePoint = AttacheePoint.RightHand,
                                        RenderableTypeKey = "rifle", Translation = new Vector3(1.129f, 0.814f, 0.709f), Rotation = new Vector3(-7.087f, 37.323f, 9.921f) }},

                         //       ParticleEmitters = new[]{ new ParticleEmitterEffect(){ ParticleSystemKey = "firePlume", EmitParticlesInParentDirection = true }  }
                                
                            },   
                                new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "combatRifleReload" }, // keycount 48
                                Sounds = new string [] { "activities/weapons/coilrifleReload" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Reloading,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Rifle )},                             
                               // Playback = Playback.Manual, // Lars: cannot be manual because the other matching anim (mend) has a very different run time!
                                AttachPoints = new[]{ 
                                    new AnimConditionInfo.AttachPointData() { AttacheePoint = AttacheePoint.RightHand,
                                        RenderableTypeKey = "rifle", Translation = new Vector3(1.129f, 0.814f, 0.709f), Rotation = new Vector3(-7.087f, 37.323f, 9.921f) }},
                                Looping = Looping.No
                            },


                            ///////Watergun         //MP: I want them to wear the watergunTank on their back as well. is that TODO?

                              new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "combatIdleRifle" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Watergun, (int)AnimModifier.Bold)},
                                AttachPoints = new[]{ 
                                    new AnimConditionInfo.AttachPointData() { AttacheePoint = AttacheePoint.RightHand,
                                        RenderableTypeKey = "watergun", Translation = new Vector3(1.129f, 0.814f, 0.709f), Rotation = new Vector3(-7.087f, 37.323f, 9.921f) }},
                            },

                             new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "idle" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Watergun)}
                            },

                                new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "combatRifleAimHip" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Attacking, 
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Watergun, (int)AnimModifier.Far, (int)AnimModifier.Low)},   
                                //Forbiddens = new BitMask64(typeof(AnimState), (int)AnimState.Moving),
                                Looping = Looping.No,
                                AttachPoints = new[]{ 
                                    new AnimConditionInfo.AttachPointData() { AttacheePoint = AttacheePoint.RightHand,
                                        RenderableTypeKey = "watergun", Translation = new Vector3(1.129f, 0.814f, 0.709f), Rotation = new Vector3(-7.087f, 37.323f, 9.921f) }},
                            },

                                new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "combatWatergunReload" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Reloading,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Watergun )},
                                //Forbiddens = new BitMask64(typeof(AnimState), (int)AnimState.Moving ),
                                Playback = Playback.Manual,
                                Looping = Looping.No,
                                AttachPoints = new[]{ 
                                    new AnimConditionInfo.AttachPointData() { AttacheePoint = AttacheePoint.RightHand,
                                        RenderableTypeKey = "watergun", Translation = new Vector3(1.129f, 0.814f, 0.709f), Rotation = new Vector3(-7.087f, 37.323f, 9.921f) }}
                            },


                            ////////Bow

                                new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "combatIdleBow" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Bow, (int)AnimModifier.Bold)},
                                AttachPoints = new[]{ 
                                    new AnimConditionInfo.AttachPointData() { AttacheePoint = AttacheePoint.RightHand,   //the anim is made for Left Hand -MP  
                                        RenderableTypeKey = "bow", Translation = new Vector3(0.341f, 0.184f, -0.026f), Rotation = new Vector3(-55.276f, 180f, 75.118f) }},
                            },

                             new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "idle" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Bow)}
                            },

                                new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "combatBowAim" },
                                Sounds = new string [] { "activities/weapons/bow/bowAim2A" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Attacking,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Bow, (int)AnimModifier.Far)},   
                                //Forbiddens = new BitMask64(typeof(AnimState), (int)AnimState.Moving),
                                Looping = Looping.No,
                                AttachPoints = new[]{ 
                                    new AnimConditionInfo.AttachPointData() { AttacheePoint = AttacheePoint.RightHand,   //the anim is made for Left Hand -MP  
                                        RenderableTypeKey = "bow", Translation = new Vector3(0.341f, 0.184f, -0.026f), Rotation = new Vector3(-55.276f, 180f, 75.118f) }},
                            },

                            
                                new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "combatBowReload" },
                                Sounds = new string [] { "activities/weapons/bow/bowReload2A" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Reloading,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Bow )},
                                //Forbiddens = new BitMask64(typeof(AnimState), (int)AnimState.Moving ),
                                Playback = Playback.Manual,
                                Looping = Looping.No,
                                AttachPoints = new[]{ 
                                    new AnimConditionInfo.AttachPointData() { AttacheePoint = AttacheePoint.RightHand,   //the anim is made for Left Hand -MP  
                                        RenderableTypeKey = "bow", Translation = new Vector3(0.341f, 0.184f, -0.026f), Rotation = new Vector3(-55.276f, 180f, 75.118f) }},
                            },


                            /////Spear

                                new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "combatIdleSpear" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Bold, (int)AnimModifier.Spear)}
                            },
      
                            new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "idle" }},
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Idle,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Spear)}
                            },
                            
                                new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "attackSpearMid" }}, 
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Attacking,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Spear, (int)AnimModifier.Near)},   
                                //Forbiddens = new BitMask64(typeof(AnimState), (int)AnimState.Moving),
                                Looping = Looping.No,
                                AttachPoints = new[]{ 
                                    new AnimConditionInfo.AttachPointData() { AttacheePoint = AttacheePoint.RightHand,
                                         RenderableTypeKey = "spear",  Translation = new Vector3(0.814f, 6.85f, 1.391f), Rotation = new Vector3(-77.008f, 36.378f, 4.252f) }}
                            }, 
                            
                            //Hoe used as spear:  
                                new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "attackHoeMid" }}, 
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Attacking,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Hoe, (int)AnimModifier.Near)},   
                                //Forbiddens = new BitMask64(typeof(AnimState), (int)AnimState.Moving),
                                Looping = Looping.No,
                                AttachPoints = new[]{ 
                                    new AnimConditionInfo.AttachPointData() { AttacheePoint = AttacheePoint.RightHand,
                                         RenderableTypeKey = "farmingHoe",  Translation = new Vector3(0.9710001f, 5.066f, 1.601f), Rotation = new Vector3(66.61401f, 61.89f, 157.323f) }}
                            }, 
                            
                             //Shovel used as spear:  
                                new AnimConditionInfo()
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "attackHoeMid" }}, 
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Attacking,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Shovel, (int)AnimModifier.Near)},   
                                //Forbiddens = new BitMask64(typeof(AnimState), (int)AnimState.Moving),
                                Looping = Looping.No,
                                AttachPoints = new[]{ 
                                    new AnimConditionInfo.AttachPointData() { AttacheePoint = AttacheePoint.RightHand,
                                         RenderableTypeKey = "shovel",  Translation = new Vector3(0.9710001f, 5.066f, 1.601f), Rotation = new Vector3(66.61401f, 61.89f, 157.323f) }}//copied from attackHoeMid Hoe.
                            },                            




                            new AnimConditionInfo() //not used
                            {
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "combatSpearThrow" }}, 
                                ConditionSet = new AnimConditions(){ Action = AnimAction.Attacking,
                                                                 Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Spear, (int)AnimModifier.Far)},   
                                //Forbiddens = new BitMask64(typeof(AnimState), (int)AnimState.Moving),
                                Looping = Looping.No,
                                AttachPoints = new[]{ 
                                    new AnimConditionInfo.AttachPointData() { AttacheePoint = AttacheePoint.RightHand,
                                         RenderableTypeKey = "spear",  Translation = new Vector3(0.499f, -1.234f, 0.079f), Rotation = new Vector3(86.457f, 1.417f, -4.252f) }}
                            },



                         
#endregion


                            /*

                            new AnimConditionInfo()//FOR AIRCRAFT, OTHER VEHICLES?
                            {
                                AnimControllerKeys = new string[] { "open_for_loading" },
                                playback = Playback.Manual,
                                blendMode = BlendMode.Additive, //additive!
                                Conditions = new ConditionSet(){ Modifiers = new BitMask64(typeof(AnimState), (int)AnimState.Containing, (int)AnimState.Reverse) //uncontaining!  
                            },


                

                            new AnimConditionInfo()//FOR AIRCRAFT
                            {
                                AnimControllerKeys = new string[] { "propeller_right_start" },
                                playback = Playback.Manual, //forwards
                                blendMode = BlendMode.Additive, //additive!
                                Conditions = new ConditionSet(){ Modifiers = new BitMask64(typeof(AnimState), (int)AnimState.Pre, (int)AnimState.Right, (int)AnimState.Slow)   
                            },

                           new AnimConditionInfo()//FOR AIRCRAFT
                            {
                                AnimControllerKeys = new string[] { "propeller_left_start" },
                                playback = Playback.Manual, //forwards
                                blendMode = BlendMode.Additive, //additive!
                                Conditions = new ConditionSet(){ Modifiers = new BitMask64(typeof(AnimState), (int)AnimState.Pre, (int)AnimState.Left, (int)AnimState.Slow)   
                            },

                           new AnimConditionInfo()//FOR AIRCRAFT
                            {
                                AnimControllerKeys = new string[] { "driver_entry" },
                                playback = Playback.Manual, //backwards
                                blendMode = BlendMode.Additive, //additive!
                                Conditions = new ConditionSet(){ Modifiers = new BitMask64(typeof(AnimState), (int)AnimState.Containing, (int)AnimState.Crew)   
                            },

                           new AnimConditionInfo()//FOR AIRCRAFT
                            {
                                AnimControllerKeys = new string[] { "passenger_entry" },
                                playback = Playback.Manual, //backwards
                                blendMode = BlendMode.Additive, //additive!
                                Conditions = new ConditionSet(){ Modifiers = new BitMask64(typeof(AnimState), (int)AnimState.Containing, (int)AnimState.Passenger)   
                            },

                           new AnimConditionInfo()//FOR AIRCRAFT
                            {
                                AnimControllerKeys = new string[] { "open_for_loading" },
                                playback = Playback.Manual, //backwards
                                blendMode = BlendMode.Additive, //additive!
                                Conditions = new ConditionSet(){ Modifiers = new BitMask64(typeof(AnimState), (int)AnimState.Containing, (int)AnimState.Open)   
                            },
             */
                        }
                       #endregion
                   },
                   AnimatedHeadType = new AnimatedHeadType()
                    {
                        SpineBones = new[] { "SpineB", "SpineC", "SpineD", "Neck", "Head" },
                        TurnToLookLerpFactor = 0.06f,
                        PitchForward = 0.12f
                    },
                   BoxHandlingWhenHauling = new BoxHandlingWhenHauling()
                   {
                       BoxHandling = BoxHandlingWhenHauling.BoxHandlingType.OnlyOnBackWhenHeavyAndHaulingFar,
                       UseHeavyBackpack = true,
                       ShowBoxInHand = AttacheePoint.RightHand,
                       AttachorWhenBoxIsInHand = "rightHand"
                   }
               }
                  ,
                    LocomotorType = new LocomotorType()
                    {
                        MaxAngularSpeed = 2.5f * MathHelper.Pi, //MP was: 1.5f * MathHelper.Pi   ...increased it, to 2.5f to make them turn  a bit quicker. (there's also TurnSpeedWhenTurningInPlace which controls rotation speed (turn speed) when turning in place)                
                        MeleeRadius = 12f,
                        CanRun = true,
                        Stances = "humanoid",
                        LeggedLocomotorType = new LeggedLocomotorType()
                        {
                            TerrainNegateFactor = 0.1f,
                            WalkSlowSpeed = 25f, //10f, // 8f, 
                            HaulSpeed = 40f,     //MP nov21 code change requires overhaul. was 37f                    ......    MP oct 22: was 32f.   I increased this to make them move more briskly 
                            WalkNormalSpeed = 66f, //MP nov21 code change requires overhaul. was 60f                     ...... MP oct 22: was 45f.   I increased this to make them move more briskly              /*proper speed*/,                        (NORMAL man walk speed=1.0, basespeed=45f) ;  (SLOW man walk speed=0.7, basespeed=25f) ;  (FAST man walk speed=1.3, basespeed=60f)                
                            //   (NORMAL man run speed=1.0, basespeed=140f) ;   (SLOW man run speed=0.9, basespeed=120) ; (FAST man run speed=1.2, basespeed=160f)
                            WalkFastSpeed = 70f, //MP nov21 code change requires overhaul. was  64f   ........        MP oct 22: was 60f      I set this to fit with MinimumSpeed (64f) for jog gaitbracket, so they dont break into a 1 second jog after walking for a while.          // 24f, // 18f,//30f, // 63f,
                            RunSpeed = 88f // 100f // 110f, //95f //  120f, // 60f // ???                         
                        },
                        CollisionResponderType = new CollisionResponderType()
                        {
                            AgentCollisionResponderType = new AgentCollisionResponderType()
                        }
                    },
                    SensorType = new SensorType()
                    {
                        Range = sensorRangeHuman, // 9, // 8, // TEST ONLY //  12, //6
                        RangeAtNight = sensorRangeHumanNight, // 5, // 24 // 6
                        DetectionTypeKey = "human"
                        //DetectionType = GameData.Instance.AllDetectionTypes["human"]
                    },
                    ContainerType = new AgentStorageType()
                    {
                        ItemStorageType = new ItemStorageType(1f), // fixed capacity - never changes
                        EquipmentStorageType = new ItemStorageType(0.5f), // fixed capacity - never changes
                        StomachStorageType = new ItemStorageType(0.07f) // stomach capacity is defined in BiologicalType - it changes with size of entity

                    },
                   /* CommunicatorType = new CommunicatorType()
                    {
                        Method = CommunicationMethod.Visual, // for communicating with Wilderness sites with exiled people
                        Range = 10
                    },*/                  
                    BodyType = bodyType                   
                };

                humanType.IntelligenceType = new IntelligenceType()
                {
                    IsMobile = true,
                    RespectsOwnership = true,
                    AllowEscapeFromTinyAreas = true,
                    CanAttack = true,
                    CanTradeAndCommunicate = true,
                    CanProduce = true,
                    CanDoJobs = true,                   
                    CanSpeak = true,
                    CanUseWeapons = true,
                    CanCheckProgress = true,
                    CanUseGadgets = true,
                    CanEmigrate = true,
                    CanMountTools = true,
                    CanReplenish = true,
                    CanHaul = true,
                    CanPatrol = true,
                    CanHunt = true,
                    CanScout = true,
                    CanExamine = true,           
                    IsPredator = true,
                    OtherAgentsNearExpeditionCenterAreConsideredThreats = true,
                    IdleChanceToTalk = 0.2f,
                    ContainerTransactTag = "humanTransact", //was "human"
                    HasServantsTags = new[]{ "servesHumans" },
                    Prey = new string[] { "entity:mudWorm", "entity:binalRat", "entity:whiteThunderChicken", "entity:pygmyThunderChicken", "entity:studdedThunderChicken", "entity:bajingan", "entity:twinkler", "entity:fieldQuadite", "entity:turnip", "entity:patrician", "entity:megapod", "entity:whipjaw", "entity:lesserWhipjaw", "entity:spoakDendront", "entity:swampDendront", "entity:bushDragon" },
                    Attacks = new[]
                     { 
                         "personPunchHighRight",
                         "personPunchLowRight",
                         "personSnapkickRight",
                         "personStompRight"                        
                     },
                   /*  IntrinsicCommunicators = new[]
                     {
                         new CommunicatorType()
                         {
                              Method = CommunicationMethod.Visual, // for communicating with Wilderness sites with exiled people
                              Range = 10
                         }
                     }, */                   
                    InterestInTriggerTypes = new[] { /*"drivenVehicle",*/ "entityDied", "creature" },
                    AggroRange = 160.0f,
                    ChanceToRestAfterMeleeAttack = 0.6,
                    ChanceToRestAfterRangedAttack = 0.6,
                    MinRestTimeAfterAttackingInSeconds = 0.5f,
                    MaxRestTimeAfterAttackingInSeconds = 1.2f,
                    
                     ChanceToIdleWalkShortDistanceAway = 0.2f, // can be overridden as BioProperty, if needed
                     ShortIdleWalkMaxDistance = 120f, // can be overridden as BioProperty, if needed
                     ShortIdleWalkMinDistance = 48f,

         

                     Skills = new SerializableDictionary<string,float>()
                     {
                         { "fruitPicking", 1f },
                         { "grasping", 1f }
                     },
                      #region Pickup Anim durations
                    // from file AnimDurations.txt

                    /*
                    man_pickupHeavy 18 (17*0,04)   =0,68    ......actionpoint(with bindpose=0): 9 (8 = 0,32)
                    man_pickupLight 31 = 1,2		......actionpoint(with bindpose=0): 15 (14 = 0,56)
                    man_pickupLightSame 32 = 1,24		......actionpoint(with bindpose=0): 15 (0,56)
                    man_pickupEquipped 44 1,72		......actionpoint(with bindpose=0): 13 (12 = 0,48)
                    man_pickupMounted 25 = 0,96		...actionpoint(with bindpose=0): 13 (12 = 0,48)


                    man_dropHeavy 24 (23*0,04)= 0,92     	...actionpoint(with bindpose=0): 12 (11 = 0,44)
                    man_dropEquipped 34 = 1,32		...actionpoint(with bindpose=0): 21 (20 = 0,8)
                    man_dropLight 31 = 1,2			...actionpoint(with bindpose=0): 17 (16 = 0,64)
                    man_dropMounted 25  (24*0,04) = 0,96	...actionpoint(with bindpose=0): 10 (9  = 0,36)
                    */

                    DropLightDuration = 1.2f,
                    DropLightActionPointDuration = 0.64f,

                    DropHeavyDuration = 0.92f,
                    DropHeavyActionPointDuration = 0.44f,

                    pickupMountedEquippedDuration = 1.72f,
                    pickupMountedEquippedActionPointDuration = 0.48f,

                    pickupEquipDuration = 1.72f,
                    pickupEquipActionPointDuration = 0.48f,


                    PickupLightDuration = 1.2f,
                    PickupLightActionPointDuration = pickupLightActionPointDuration, 

                    PickupHeavyDuration = 0.68f,
                    PickupHeavyActionPointDuration = pickupHeavyActionPointDuration,

                    pickupMountDuration = 0.96f,
                    pickupMountActionPointDuration = 0.48f,

                    SwitchLightToLightDuration = 2.4f, // 1.24f;
                    SwitchLightToLightActionPointDuration = 1.2f, // 0.56f;

                    #endregion

                    #region Drop Anim durations

                    dropEquippedDuration = 1.32f,
                    dropEquippedActionPointDuration = 0.8f,

                    dropMountedDuration = 0.96f,
                    dropMountedActionPointDuration = 0.36f,

                    DropLightToLightActionPointDuration = 0.6f,
                    DropLightToLightDuration = 1.24f, // 

                    dropMountToMountActionPointDuration = 0.5f, // TODO
                    dropMountToMountDuration = 1f, // TODO

                    #endregion
              

                };


                humanType.BiologicalType = new BiologicalType()
                {
                    SpeciesPlural = "humans",
                    HoldsFoodWhenEating = true,
                    EatingStances = new[] { new ChanceToTakeStance() { Stance = "sitting" } },
                    OxygenAndMuscleEnergyIncreaseRatePerDay = 10f,  //ONLY influences the maximum time an agent can run between breaks. (no effect on tasks such as hauling as of yet (july 2013)) We aim for top speed running across 1,5 screen before rest is needed.
                    FoodItemTagsThatCanBeConsumed = new[] { "cookedMeat", "edibleVegi", "coffee", "alcoholicBeverage" }, // TODO: add more tags
                    TimeToConsumeFullMealInDays = 0.0065f, //bso changed to from 0.013 to 0.0065 to free up more time for other activities //mpfeb 2015 reduced from 0.02f....mp january 2015 reduced from 0.035f
                    StomachSizeFractionOfEntityBulk = 0.225f,  //mp changed jan 2016...........mp feb 2015 0.45f . tried with 0.25f but they ate all the friggin time.         was 0.45. it had this comment: corresponds to recommended daily food intake of 0.25f
                    StomachContentsDecreaseRatePerDay = 2,
                    ActiveStealthRating = 0f, // use a skill instead
                  
                    MaxRegainLimit = humanMaxRegainLimit,
                    FractionOfMaxHitpointsGainedPerDay = humanFractionOfMaxHitpointsGainedPerDay,
                    TimeOfDayToGoToSleep = 0.95,

                    ModelBasicTextureName = "ManGrey2Texture", // if we don't specify a default, the texture swap bug may appear.

                    Carcass = "item:body",
                    #region Racetype
                    RaceTypes = new[] //primary color=skin color, secondary color= hair color
                                {  
#region Old race stuff not used
                                    new RaceType()
                                    { KeyName = "white1", Name="White", PortraitSkinType="Celtic",                                         
                                        PrimaryColor = new Vector3(0.9803922f, 0.9764706f, 0.9686275f),  Edge = 0.2f, // Celtic                                      
                                        SecondaryColorProbabilityEdges = new List<ColorProbability>(){ 
                                            new ColorProbability(0.1f, new Vector3(0.9333333f, 0.9019608f, 0.8588235f)),
                                            new ColorProbability(0.5f, new Vector3(0.8941177f, 0.8313726f, 0.7019608f)),
                                            new ColorProbability(0.9f, new Vector3(0.772549f, 0.5254902f, 0.2627451f)),
                                            new ColorProbability(1f, new Vector3(0.7843137f, 0.6666667f, 0.5333334f))} 
                                    },
                                    new RaceType()                                      // LightEuropean
                                    { KeyName = "white2", Name="White", PortraitSkinType="Light European", PrimaryColor = new Vector3(0.9529412f, 0.9176471f, 0.8980392f), Edge = 0.5f, // 0.1f, 0.2f, 0.3f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1.0f 
                                    SecondaryColorProbabilityEdges = new List<ColorProbability>(){ 
                                            new ColorProbability(0.1f, new Vector3(0.9333333f, 0.9019608f, 0.8588235f)),
                                            new ColorProbability(0.2f, new Vector3(0.8941177f, 0.8313726f, 0.7019608f)),
                                            new ColorProbability(0.3f, new Vector3(0.772549f, 0.5254902f, 0.2627451f)),
                                            new ColorProbability(0.5f, new Vector3(0.7843137f, 0.6666667f, 0.5333334f)),
                                            new ColorProbability(0.6f, new Vector3(0.654902f, 0.5019608f, 0.3803922f)),
                                            new ColorProbability(0.7f, new Vector3(0.5137255f, 0.4078431f, 0.3254902f)),
                                            new ColorProbability(0.8f, new Vector3(0.3882353f, 0.2352941f, 0.2078431f)),
                                            new ColorProbability(0.9f, new Vector3(0.2509804f, 0.1882353f, 0.1882353f)),
                                            new ColorProbability(1f, new Vector3(0.09019608f, 0.08235294f, 0.09411765f))} 
                                    },
                                    new RaceType()                              // AverageCaucasian
                                    { KeyName = "white3",  Name="White", PortraitSkinType="Average Caucasian", PrimaryColor = new Vector3(0.9960784f, 0.9647059f, 0.8823529f),  Edge = 0.7f, // 0f, 0f, 0f, 0.1f, 0.2f, 0.7f, 0.8f, 0.9f, 1.0f 
                                        SecondaryColorProbabilityEdges = new List<ColorProbability>(){ 
                                            new ColorProbability(0.1f, new Vector3(0.7843137f, 0.6666667f, 0.5333334f)),
                                            new ColorProbability(0.2f, new Vector3(0.654902f, 0.5019608f, 0.3803922f)),
                                            new ColorProbability(0.7f, new Vector3(0.5137255f, 0.4078431f, 0.3254902f)),
                                            new ColorProbability(0.8f, new Vector3(0.3882353f, 0.2352941f, 0.2078431f)),
                                            new ColorProbability(0.9f, new Vector3(0.2509804f, 0.1882353f, 0.1882353f)),
                                            new ColorProbability(1.0f, new Vector3(0.09019608f, 0.08235294f, 0.09411765f))} 
                                    },
                                    new RaceType()                              // OliveSkin
                                    { KeyName = "hispanic", Name="Hispanic", PortraitSkinType="Olive skin", PrimaryColor = new Vector3(0.9215686f, 0.8392157f, 0.6235294f),  Edge = 0.8f, //  0f, 0f, 0f, 0f, 0f, 0f, 0.1f, 0.7f, 0.9f, 1.0f
                                        SecondaryColorProbabilityEdges = new List<ColorProbability>(){ 
                                            new ColorProbability(0.1f, new Vector3(0.3882353f, 0.2352941f, 0.2078431f)),
                                            new ColorProbability(0.7f, new Vector3(0.2509804f, 0.1882353f, 0.1882353f)),
                                            new ColorProbability(1.0f, new Vector3(0.09019608f, 0.08235294f, 0.09411765f))} 
                                    },
                                    new RaceType()                              // Dark
                                    { KeyName = "black1", Name="Black", PortraitSkinType="Dark", PrimaryColor = new Vector3(0.6117647f, 0.4196078f, 0.2627451f),  Edge = 0.85f, // 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0.2f, 0.7f, 1.0f 
                                        SecondaryColorProbabilityEdges = new List<ColorProbability>(){ 
                                            new ColorProbability(0.2f, new Vector3(0.3882353f, 0.2352941f, 0.2078431f)),
                                            new ColorProbability(0.7f, new Vector3(0.2509804f, 0.1882353f, 0.1882353f)),
                                            new ColorProbability(1f, new Vector3(0.09019608f, 0.08235294f, 0.09411765f))} 
                                    },
                                    new RaceType()                              // Black
                                    { KeyName = "black2", Name="Black", PortraitSkinType = "Black", PrimaryColor = new Vector3(0.3411765f, 0.1960784f, 0.1607843f),  Edge = 0.9f, //  0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0.4f, 1.0f 
                                        SecondaryColorProbabilityEdges = new List<ColorProbability>(){ 
                                            new ColorProbability(0.4f, new Vector3(0.2509804f, 0.1882353f, 0.1882353f)),
                                            new ColorProbability(1f, new Vector3(0.09019608f, 0.08235294f, 0.09411765f))} 
                                    },
                                    new RaceType()                              // 
                                    { KeyName = "asian", Name="Asian", PortraitSkinType="East Asian", PrimaryColor = new Vector3(0.9529412f, 0.9176471f, 0.8980392f),  Edge = 1.0f, //  0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0.4f, 1.0f 
                                        SecondaryColorProbabilityEdges = new List<ColorProbability>(){ 
                                            new ColorProbability(0.1f, new Vector3(0.2509804f, 0.1882353f, 0.1882353f)),
                                            new ColorProbability(1f, new Vector3(0.09019608f, 0.08235294f, 0.09411765f))} 
                                    },
#endregion


                                    ////////////////////////////////////////////////////////////////NEW!////////////////////////////////////////////////////////////////////
 ////////////////////////////////  NEW the  4 races for primitive future. the descendants. With primitive future clothing (earthy colors). TODO: clean up unused textures above!!!!!!!!!!!!!!!!!!!!
                                    new RaceType()                             
                                    { KeyName = "whiteHumanDescendant", Name="White Human Descendant", PortraitSkinType="whitePortrait",                                        
                                         ModelBasicTextureNames = new[] {"ManBlueBrownClothesBrownHairTexture","ManOrangeGreyClothesYellowHairTexture", "ManCurryClothesRedHairTexture", "ManTurquoiseDarkClothesBlondHairTexture", "ManBurgundyClothesWhiteHairTexture", "ManDarkBlueBeigeClothesBrownHairTexture", "ManOrangeDarkGreyClothesYellowHairTexture", "ManDarkBrownClothesRedHairTexture", "ManBlueGreyClothesYellowHairTexture","ManOrangeGreyClothesRedHairTexture", "ManBrownGreyClothesBlondHairTexture", "ManSandyDarkClothesWhiteHairTexture",},
                                        PrimaryColor = new Vector3(0.6117647f, 0.4196078f, 0.2627451f),  Edge = 1.0f,
                                        SecondaryColorProbabilityEdges = new List<ColorProbability>(){ 
                                            new ColorProbability(1f, new Vector3(0.2509804f, 0.1882353f, 0.1882353f))}
                                    } ,


                                    new RaceType()                             
                                    { KeyName = "asianHumanDescendant", Name="Asian Human Descendant", PortraitSkinType="asianPortrait",                                        
                                         ModelBasicTextureNames = new[] {"ManSandyClothesBlackHairTexture","ManCurryGreyClothesBrownSkinTexture", "ManOchreClothesBlackHairTexture", "ManTurquoiseDarkClothesBlackHairTexture",},
                                        PrimaryColor = new Vector3(0.6117647f, 0.4196078f, 0.2627451f),  Edge = 1.0f,
                                        SecondaryColorProbabilityEdges = new List<ColorProbability>(){ 
                                            new ColorProbability(1f, new Vector3(0.2509804f, 0.1882353f, 0.1882353f))}
                                    } ,

                                    new RaceType()                             
                                    { KeyName = "hispanicHumanDescendant", Name="Hispanic Human Descendant", PortraitSkinType="hispanicPortrait",                                        
                                         ModelBasicTextureNames = new[] {"ManBlueBrownClothesBrownHairTexture", "ManDarkRedClothesDarkSkinBrownHairTexture", "ManSandyClothesBlackHairTexture", "ManDarkBlueBeigeClothesBrownHairTexture", "ManCurryGreyClothesBrownSkinTexture", "ManOchreClothesBlackHairTexture", "ManTurquoiseDarkClothesBlackHairTexture",},
                                        PrimaryColor = new Vector3(0.6117647f, 0.4196078f, 0.2627451f),  Edge = 1.0f,
                                        SecondaryColorProbabilityEdges = new List<ColorProbability>(){ 
                                            new ColorProbability(1f, new Vector3(0.2509804f, 0.1882353f, 0.1882353f))}
                                    } ,

                                    new RaceType()                             
                                    { KeyName = "blackHumanDescendant", Name="Black Human Descendant", PortraitSkinType="blackPortrait",                                        
                                         ModelBasicTextureNames = new[] {"ManDarkRedClothesDarkSkinBrownHairTexture", "ManBrownGreyClothesDarkSkinTexture", "ManBrownBeigeClothesDarkSkinTexture", "ManSandyClothesDarkSkinTexture", "ManCurryGreyClothesBrownSkinTexture", },
                                        PrimaryColor = new Vector3(0.6117647f, 0.4196078f, 0.2627451f),  Edge = 1.0f,
                                        SecondaryColorProbabilityEdges = new List<ColorProbability>(){ 
                                            new ColorProbability(1f, new Vector3(0.2509804f, 0.1882353f, 0.1882353f))}
                                    } ,

///////////////////////////////////////The races for Planetfall. ancestors. characterized by white/light grey color patches on clothes. Hi-tech look.////////////////////////////////

                                    new RaceType()                             
                                    { KeyName = "whiteHumanAncestor", Name="White Human Ancestor", PortraitSkinType="whitePortrait",                                        
                                         ModelBasicTextureNames = new[] {"ManBlue1Texture", "ManRed1Texture", "ManRed2Texture", "ManGreen1Texture", "ManGreen2Texture", "ManGrey1Texture", "ManGrey2Texture", "ManGrey3Texture", //the white PRECOL suits
                                             "ManBlueSolid1Texture", "ManGreenSolid1Texture", "ManGreySolid1Texture","ManGreySolid2Texture", //full color fatigues
                                              "ManGreyClothes1Texture", "ManWhitePantsClothes1Texture", "ManWhiteBlueClothes1Texture", "ManBlueGreyClothesYellowHairTexture",  //clothes with patches of white or grey
                                              "ManGreenBlueClothes1Texture"},  //other clothes
                                        PrimaryColor = new Vector3(0.6117647f, 0.4196078f, 0.2627451f),  Edge = 1.0f,
                                        SecondaryColorProbabilityEdges = new List<ColorProbability>(){ 
                                            new ColorProbability(1f, new Vector3(0.2509804f, 0.1882353f, 0.1882353f))}
                                    } ,

                                    new RaceType()                             
                                    { KeyName = "asianHumanAncestor", Name="Asian Human Ancestor", PortraitSkinType="asianPortrait",                                        
                                         ModelBasicTextureNames = new[] {"ManRed1Texture",  "ManGreen1Texture",   //the white PRECOL suits with black hair
                                             "ManGreySolid2Texture", //full color fatigues with black hair
                                               "ManWhitePantsClothes1Texture",   //clothes with patches of white or grey. black hair
                                                "ManTurquoiseDarkClothesBlackHairTexture"}, //other clothes with black hair
                                        PrimaryColor = new Vector3(0.6117647f, 0.4196078f, 0.2627451f),  Edge = 1.0f,
                                        SecondaryColorProbabilityEdges = new List<ColorProbability>(){ 
                                            new ColorProbability(1f, new Vector3(0.2509804f, 0.1882353f, 0.1882353f))}
                                    } ,

                                    new RaceType()                             
                                    { KeyName = "hispanicHumanAncestor", Name="Hispanic Human Ancestor", PortraitSkinType="hispanicPortrait",                                        
                                         ModelBasicTextureNames = new[] {"ManGreen2Texture", "ManGrey2Texture", "ManGrey3Texture", "ManRed1Texture",  "ManGreen1Texture",   //the white PRECOL suits with black and brown hair
                                             "ManGreenSolid1Texture", "ManGreySolid2Texture", //full color fatigues with black and brown hair
                                               "ManWhitePantsClothes1Texture",   //clothes with patches of white or grey. black and brown hair
                                                "ManDarkBlueBeigeClothesBrownHairTexture","ManTurquoiseDarkClothesBlackHairTexture"}, //other clothes with black  and brown hair
                                        PrimaryColor = new Vector3(0.6117647f, 0.4196078f, 0.2627451f),  Edge = 1.0f,
                                        SecondaryColorProbabilityEdges = new List<ColorProbability>(){ 
                                            new ColorProbability(1f, new Vector3(0.2509804f, 0.1882353f, 0.1882353f))}
                                    } ,

                                    new RaceType()                             
                                    { KeyName = "blackHumanAncestor", Name="Black Human Ancestor", PortraitSkinType="blackPortrait",                                        
                                         ModelBasicTextureNames = new[] { "ManBlue2Texture",  //the white PRECOL suits with dark and black skin
                                              //TODO full color fatigues with dark and black skin
                                                  //TODO clothes with patches of white or grey. with dark and black skin
                                               "ManGreenGreyClothes1Texture" }, //other clothes with dark and black skin
                                        PrimaryColor = new Vector3(0.6117647f, 0.4196078f, 0.2627451f),  Edge = 1.0f,
                                        SecondaryColorProbabilityEdges = new List<ColorProbability>(){ 
                                            new ColorProbability(1f, new Vector3(0.2509804f, 0.1882353f, 0.1882353f))}
                                    } ,
/////////////////////////////////////////////////

                                     new RaceType()                             
                                    { KeyName = "sandyClothesBlackHair", Name="Asian", PortraitSkinType="East Asian", ModelBasicTextureName="ManSandyClothesBlackHairTexture", PrimaryColor = new Vector3(0.6117647f, 0.4196078f, 0.2627451f),  Edge = 1.0f,
                                        SecondaryColorProbabilityEdges = new List<ColorProbability>(){ 
                                            new ColorProbability(1f, new Vector3(0.2509804f, 0.1882353f, 0.1882353f))}
                                    } ,

                                     new RaceType()                             
                                    { KeyName = "burgundyClothesWhiteHair", Name="White", PortraitSkinType="White", ModelBasicTextureName="ManBurgundyClothesWhiteHairTexture", PrimaryColor = new Vector3(0.6117647f, 0.4196078f, 0.2627451f),  Edge = 1.0f,
                                        SecondaryColorProbabilityEdges = new List<ColorProbability>(){ 
                                            new ColorProbability(1f, new Vector3(0.2509804f, 0.1882353f, 0.1882353f))}
                                    } ,

                                     new RaceType()                             
                                    { KeyName = "darkBlueBeigeClothesBrownHair", Name="Hispanic", PortraitSkinType="White", ModelBasicTextureName="ManDarkBlueBeigeClothesBrownHairTexture", PrimaryColor = new Vector3(0.6117647f, 0.4196078f, 0.2627451f),  Edge = 1.0f,
                                        SecondaryColorProbabilityEdges = new List<ColorProbability>(){ 
                                            new ColorProbability(1f, new Vector3(0.2509804f, 0.1882353f, 0.1882353f))}
                                    } ,

                                     new RaceType()                             
                                    { KeyName = "orangeDarkGreyClothesYellowHair", Name="White", PortraitSkinType="White", ModelBasicTextureName="ManOrangeDarkGreyClothesYellowHairTexture", PrimaryColor = new Vector3(0.6117647f, 0.4196078f, 0.2627451f),  Edge = 1.0f,
                                        SecondaryColorProbabilityEdges = new List<ColorProbability>(){ 
                                            new ColorProbability(1f, new Vector3(0.2509804f, 0.1882353f, 0.1882353f))}
                                    } ,

                                     new RaceType()                             
                                    { KeyName = "darkBrownClothesRedHair", Name="White", PortraitSkinType="White", ModelBasicTextureName="ManDarkBrownClothesRedHairTexture", PrimaryColor = new Vector3(0.6117647f, 0.4196078f, 0.2627451f),  Edge = 1.0f,
                                        SecondaryColorProbabilityEdges = new List<ColorProbability>(){ 
                                            new ColorProbability(1f, new Vector3(0.2509804f, 0.1882353f, 0.1882353f))}
                                    } ,

                                     new RaceType()                             
                                    { KeyName = "brownBeigeClothesDarkSkin", Name="Black", PortraitSkinType="Black", ModelBasicTextureName="ManBrownBeigeClothesDarkSkinTexture", PrimaryColor = new Vector3(0.6117647f, 0.4196078f, 0.2627451f),  Edge = 1.0f,
                                        SecondaryColorProbabilityEdges = new List<ColorProbability>(){ 
                                            new ColorProbability(1f, new Vector3(0.2509804f, 0.1882353f, 0.1882353f))}
                                    } ,

                                     new RaceType()                             
                                    { KeyName = "sandyClothesDarkSkin", Name="Black", PortraitSkinType="Black", ModelBasicTextureName="ManSandyClothesDarkSkinTexture", PrimaryColor = new Vector3(0.6117647f, 0.4196078f, 0.2627451f),  Edge = 1.0f,
                                        SecondaryColorProbabilityEdges = new List<ColorProbability>(){ 
                                            new ColorProbability(1f, new Vector3(0.2509804f, 0.1882353f, 0.1882353f))}
                                    } ,

                                     new RaceType()                             
                                    { KeyName = "blueGreyClothesYellowHair", Name="White", PortraitSkinType="White", ModelBasicTextureName="ManBlueGreyClothesYellowHairTexture", PrimaryColor = new Vector3(0.6117647f, 0.4196078f, 0.2627451f),  Edge = 1.0f,
                                        SecondaryColorProbabilityEdges = new List<ColorProbability>(){ 
                                            new ColorProbability(1f, new Vector3(0.2509804f, 0.1882353f, 0.1882353f))}
                                    } ,

                                     new RaceType()                             
                                    { KeyName = "orangeGreyClothesRedHair", Name="White", PortraitSkinType="White", ModelBasicTextureName="ManOrangeGreyClothesRedHairTexture", PrimaryColor = new Vector3(0.6117647f, 0.4196078f, 0.2627451f),  Edge = 1.0f,
                                        SecondaryColorProbabilityEdges = new List<ColorProbability>(){ 
                                            new ColorProbability(1f, new Vector3(0.2509804f, 0.1882353f, 0.1882353f))}
                                    } ,

                                     new RaceType()                             
                                    { KeyName = "curryGreyClothesBrownSkin", Name="Hispanic", PortraitSkinType="Dark", ModelBasicTextureName="ManCurryGreyClothesBrownSkinTexture", PrimaryColor = new Vector3(0.6117647f, 0.4196078f, 0.2627451f),  Edge = 1.0f,
                                        SecondaryColorProbabilityEdges = new List<ColorProbability>(){ 
                                            new ColorProbability(1f, new Vector3(0.2509804f, 0.1882353f, 0.1882353f))}
                                    } ,

                                     new RaceType()                             
                                    { KeyName = "ochreClothesBlackHair", Name="Asian", PortraitSkinType="Dark", ModelBasicTextureName="ManOchreClothesBlackHairTexture", PrimaryColor = new Vector3(0.6117647f, 0.4196078f, 0.2627451f),  Edge = 1.0f,
                                        SecondaryColorProbabilityEdges = new List<ColorProbability>(){ 
                                            new ColorProbability(1f, new Vector3(0.2509804f, 0.1882353f, 0.1882353f))}
                                    } ,

                                     new RaceType()                             
                                    { KeyName = "brownGreyClothesBlondHair", Name="White", PortraitSkinType="White", ModelBasicTextureName="ManBrownGreyClothesBlondHairTexture", PrimaryColor = new Vector3(0.6117647f, 0.4196078f, 0.2627451f),  Edge = 1.0f,
                                        SecondaryColorProbabilityEdges = new List<ColorProbability>(){ 
                                            new ColorProbability(1f, new Vector3(0.2509804f, 0.1882353f, 0.1882353f))}
                                    } ,

                                     new RaceType()                             
                                    { KeyName = "turquoiseDarkClothesBlackHair", Name="Asian", PortraitSkinType="East Asian", ModelBasicTextureName="ManTurquoiseDarkClothesBlackHairTexture", PrimaryColor = new Vector3(0.6117647f, 0.4196078f, 0.2627451f),  Edge = 1.0f,
                                        SecondaryColorProbabilityEdges = new List<ColorProbability>(){ 
                                            new ColorProbability(1f, new Vector3(0.2509804f, 0.1882353f, 0.1882353f))}
                                    } ,

                                     new RaceType()                             
                                    { KeyName = "sandyDarkClothesWhiteHair", Name="White", PortraitSkinType="White", ModelBasicTextureName="ManSandyDarkClothesWhiteHairTexture", PrimaryColor = new Vector3(0.6117647f, 0.4196078f, 0.2627451f),  Edge = 1.0f,
                                        SecondaryColorProbabilityEdges = new List<ColorProbability>(){ 
                                            new ColorProbability(1f, new Vector3(0.2509804f, 0.1882353f, 0.1882353f))}
                                    } ,




/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////MP Doesnt work.:
                                     new RaceType()                             
                                    { KeyName = "colorReplaceClothes", Name="ManColorReplaceClothes", PortraitSkinType="White", ModelBasicTextureName="ManColorReplaceTexture", PrimaryColor = new Vector3(0.6117647f, 0.4196078f, 0.2627451f),  Edge = 1.0f,
                                        SecondaryColorProbabilityEdges = new List<ColorProbability>(){ 
                                            new ColorProbability(1f, new Vector3(0.2509804f, 0.1882353f, 0.1882353f))} //0.2509804f multiply by 255: 64, -> 40 (HEX) select Hex on calculator)   0.1882353f X 255= 47,9999 -> 30 (HEX) ...so it gives an RGB of (40,30,30) which is a blackish red. but when running the game, his hair comes out bright yellow... wtf.
                                    }                                     
                                   
                                },
                                #endregion
                    Castes = new List<CasteType>() 
                    { 
                    new CasteType() {
                        KeyName = "male",
                        Reproduction = Reproduction.Male, Edge = 0.51f,
                        HeightMean = 1.8f, HeightStandardDeviation = 0.08f, 
                        WeightMean = 80f, WeightStandardDeviation = 0.15f,
                         
                        ModelName = "man", // "man",
                        ModelScale = 2f, 
                  
                        AgeGroupTypes = new List<AgeGroupType>() 
                        { 
                            new AgeGroupType() 
                            { 
                                Name = "Baby", AIAgeGroup = AIAgeGroup.Baby, CanReproduce = false, Edge = 1.5f, HeightTargetModifier = 0.05f, WeightTargetModifier = 0.03f, 
                                NeedTypes = new NeedType[]{ babySleepNeed, humanFoodNeed, humanProteinNeed, humanMicronutrientsNeed }
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Child", AIAgeGroup = AIAgeGroup.Child, CanReproduce = false, Edge = 11f, HeightTargetModifier = 0.6f, WeightTargetModifier = 0.5f  
                                , NeedTypes = new NeedType[]{ childSleepNeed, humanFoodNeed, humanProteinNeed, humanMicronutrientsNeed }
                            },
                            new AgeGroupType() 
                            { 
                                Name = "Young adult", AIAgeGroup = AIAgeGroup.YoungAdult, CanReproduce = false, Edge = 16f, HeightTargetModifier = 0.97f, WeightTargetModifier = 0.92f  
                               , NeedTypes = new NeedType[]{ youngAdultSleepNeed, humanFoodNeed, humanProteinNeed, humanMicronutrientsNeed }
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Adult", AIAgeGroup = AIAgeGroup.Adult, CanReproduce = true, Edge = 72f, HeightTargetModifier = 1f, WeightTargetModifier = 1f  
                               , NeedTypes = new NeedType[]{ adultSleepNeed, humanFoodNeed, humanProteinNeed, humanMicronutrientsNeed, humanStimulantsNeed }
                            }
                            ,
                            new AgeGroupType() 
                            { 
                                Name = "Old", AIAgeGroup = AIAgeGroup.Old, CanReproduce = true, Edge = 200f, HeightTargetModifier = 0.9f, WeightTargetModifier = 1f  
                               , NeedTypes = new NeedType[]{ oldSleepNeed, humanFoodNeed, humanProteinNeed, humanMicronutrientsNeed, humanStimulantsNeed }
                            }
                        } 
                    },
                    new CasteType() { 
                        KeyName = "female",
                        Reproduction = Reproduction.Female, Edge = 1f, 
                        HeightMean = 1.68f, HeightStandardDeviation = 0.05f, 
                        WeightMean = 68f, WeightStandardDeviation = 0.10f,

                        ModelName = "woman",                       
                        ModelScale = 2f, //1.95f, 
                  
                        AgeGroupTypes = new List<AgeGroupType>() 
                        { 
                            new AgeGroupType() 
                            { 
                                Name = "Baby", AIAgeGroup = AIAgeGroup.Baby, CanReproduce = false, Edge = 1.5f, HeightTargetModifier = 0.05f, WeightTargetModifier = 0.03f, 
                                NeedTypes = new NeedType[]{ babySleepNeed, humanFoodNeed, humanProteinNeed, humanMicronutrientsNeed }
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Child", AIAgeGroup = AIAgeGroup.Child, CanReproduce = false, Edge = 11f, HeightTargetModifier = 0.6f, WeightTargetModifier = 0.5f  
                              , NeedTypes = new NeedType[]{ childSleepNeed, humanFoodNeed, humanProteinNeed, humanMicronutrientsNeed }
                            },
                            new AgeGroupType() 
                            { 
                                Name = "Young adult", AIAgeGroup = AIAgeGroup.YoungAdult, CanReproduce = false, Edge = 16f, HeightTargetModifier = 0.97f, WeightTargetModifier = 0.92f  
                               , NeedTypes = new NeedType[]{ youngAdultSleepNeed, humanFoodNeed, humanProteinNeed, humanMicronutrientsNeed }
                            } ,
                            new AgeGroupType() 
                            { 
                                Name = "Adult", AIAgeGroup = AIAgeGroup.Adult, CanReproduce = true, Edge = 72f, HeightTargetModifier = 1f, WeightTargetModifier = 1f  
                              , NeedTypes = new NeedType[]{ adultSleepNeed, humanFoodNeed, humanProteinNeed, humanMicronutrientsNeed, humanStimulantsNeed }
                            }
                            ,
                            new AgeGroupType() 
                            { 
                                Name = "Old", AIAgeGroup = AIAgeGroup.Old, CanReproduce = false, Edge = 200f, HeightTargetModifier = 0.9f, WeightTargetModifier = 1f  
                              , NeedTypes = new NeedType[]{ oldSleepNeed, humanFoodNeed, humanProteinNeed, humanMicronutrientsNeed, humanStimulantsNeed }
                            }
                        } 
                    } 
                }

                };

                humanType.Person = new PersonType()
                {
                  //  AlwaysEatAtDinnerTime = false,

                    ShirtColors = new List<Vector3>() { new Vector3(0.4980392f, 0.4352941f, 0.3411765f), new Vector3(0.6196079f, 0.4705882f, 0.2509804f), 
                    new Vector3(0.3568628f, 0.6196079f, 0.3843137f), new Vector3(0.509804f, 0.6784314f, 0.4941176f), new Vector3(0.2862745f, 0.6196079f, 1f), 
                    new Vector3(0.3176471f, 0.5529412f, 0.6784314f), new Vector3(0.7372549f, 0.2588235f, 0.2509804f), new Vector3(0.7764706f, 0.7764706f, 0.7764706f), 
                    new Vector3(0.4784314f, 0.4784314f, 0.4784314f), new Vector3(0.3372549f, 0.3372549f, 0.3372549f), new Vector3(0.3882353f, 0.3882353f, 0.3882353f)},

                    PantsColors = new List<Vector3>(){ new Vector3(0.4392157f, 0.3647059f, 0.2705882f), new Vector3(0.627451f, 0.5372549f, 0.427451f), new Vector3(0.3803922f, 0.509804f, 0.3843137f), 
                        new Vector3(0.2588235f, 0.4352941f, 0.6470588f), new Vector3(0.4901961f, 0.7215686f, 0.8784314f), new Vector3(0.4980392f, 0.4980392f, 0.4980392f), new Vector3(0.627451f, 0.627451f, 0.627451f)}


                };

                listOfEntityTypes.Add(humanType);

                #endregion

        }

        /// <summary>
        /// deprecate this - find a better way to show entity type descriptions
        /// </summary>
        /// <param name="babySleepNeed"></param>
        /// <param name="childSleepNeed"></param>
        /// <param name="youngAdultSleepNeed"></param>
        /// <param name="adultSleepNeed"></param>
        /// <param name="oldSleepNeed"></param>
        /// <param name="humanFoodNeed"></param>
        /// <param name="humanProteinNeed"></param>
        /// <param name="humanMicronutrientsNeed"></param>
        public static void CreateHumanNeeds(out NeedType babySleepNeed, out NeedType childSleepNeed, out NeedType youngAdultSleepNeed, out NeedType adultSleepNeed, out NeedType oldSleepNeed, out NeedType humanFoodNeed, out NeedType humanProteinNeed, out NeedType humanMicronutrientsNeed, out NeedType humanStimulantsNeed)
        {
            babySleepNeed = new NeedType() { KeyName = "sleep", SleepNeedType = new SleepNeedType(){ }, DecreasePerDay = new NormalDistribution() { Mean = 0.7f, StandardDeviation = 0.03f }, LimitForDecreasedEnergy = 0.2f, DecreasedEnergyWeight = 0.4f };
            childSleepNeed = new NeedType() { KeyName = "sleep", SleepNeedType = new SleepNeedType(){ }, DecreasePerDay = new NormalDistribution() { Mean = 0.4f, StandardDeviation = 0.02f }, LimitForDecreasedEnergy = 0.2f, DecreasedEnergyWeight = 0.4f };
            youngAdultSleepNeed = new NeedType() { KeyName = "sleep", SleepNeedType = new SleepNeedType(){ }, DecreasePerDay = new NormalDistribution() { Mean = 0.38f, StandardDeviation = 0.015f }, LimitForDecreasedEnergy = 0.2f, DecreasedEnergyWeight = 0.4f };
            adultSleepNeed = new NeedType()
            {
                KeyName = "sleep",
                SleepNeedType = new SleepNeedType(){ },
                DecreasePerDay = new NormalDistribution() { Mean = 1f, StandardDeviation = 0.015f },
                LimitForDecreasedEnergy = 0.4f,   //MP: used to be 0.2f
                DecreasedEnergyWeight = 0.4f,
                PhysicalEffects = new PhysicalEffects()
                {
                    DaysAtZeroCausingDeath = 2f,
                    DaysAtZeroDecreaseFactor = 2f // decrease 'starvation' quickly
                }
            };
            oldSleepNeed = new NeedType() { KeyName = "sleep", SleepNeedType = new SleepNeedType() { }, DecreasePerDay = new NormalDistribution() { Mean = 0.4f, StandardDeviation = 0.02f }, LimitForDecreasedEnergy = 0.2f, DecreasedEnergyWeight = 0.4f };

            float dailyMealBulk = 0.125f;   //.......mp changed to 0.125f jan 2016....Before that: 0,25f bulk food pr game day

            humanFoodNeed = new NeedType()
            {
                KeyName = "foodEnergy",
                FoodNeedType = new FoodNeedType()
                {
                    FoodNutrient = "foodEnergy",
                    RequiredNutrientsAsFractionOfEntityBulk = 0.6f * dailyMealBulk / 1f  //mp changed again on march 31 2016.........mp dailyMealBulk changed jan 2016................  = 0,15....Lars suggests 0,25f bulk food pr game day (dailyMealBulk defined 12 lines up). with 60% energy from an ideal meal, for a man weighing 1f bulk, the RequiredNutrientsAsFractionOfEntityBulk is 0,6*0,25/1f
               
                },
                DecreasePerDay = new NormalDistribution() { Mean = 1f, StandardDeviation = 0.02f },// Bso Changed from 1.4 to 1 to minimice the amount of time between meals, this is create more game time for other aspects of the game //0.7f
               
                LimitForDecreasedEnergy = 0.15f,
                DecreasedEnergyWeight = 0.6f,
                PhysicalEffects = new PhysicalEffects()
                {
                    DaysAtZeroCausingCollapse = 2f,
                    DaysAtZeroCausingDeath = 2.05f,
                    DaysAtZeroDecreaseFactor = 1f, // 0.5f,
                    UseExertionFactorToDecrease = true                     
                }
            };
            humanProteinNeed = new NeedType()
            {
                KeyName = "protein",
                FoodNeedType = new FoodNeedType()
                {
                    FoodNutrient = "protein",
                    RequiredNutrientsAsFractionOfEntityBulk = 0.006f   //.....mp changed to 0.006f jan 2016............. answering the hover-tooltip: we assume a days' worth of recommended protein. 0,012 f bulk protein/gameday / 1f = 0,012f   ...... calculated here (under protein) https://docs.google.com/a/unclaimedworld-game.com/document/d/106BIllzcCkma24NlcWCZLwyEpHaG6AWed0GpSUUMTug/edit
             
                },
                DecreasePerDay = new NormalDistribution() { Mean = 1f, StandardDeviation = 0.02f },
                LimitForDecreasedEnergy = 0.05f,
                DecreasedEnergyWeight = 0.08f, // 0.6f,
                PhysicalEffects = new PhysicalEffects()
                {
                    /*DaysAtZeroCausingCollapse = 2f,
                    DaysAtZeroCausingDeath = 2.1f,
                    DaysAtZeroDecreaseFactor = 0.5f,*/
                    LimitForReducedGrowth = 0.1f,
                    LimitForIncreasedSickness = 0.05f,
                    UseExertionFactorToDecrease = false
                }
            };
            humanMicronutrientsNeed = new NeedType()
            {
                KeyName = "micronutrients",
                FoodNeedType = new FoodNeedType()
                {
                    FoodNutrient = "micronutrients",
                    RequiredNutrientsAsFractionOfEntityBulk = 0.0003f   //......mp changed to 0.0003f jan 2016............recommended daily intake of min+vit is 6g. 0,006kg*8/80kg/bulk = 0,0006f bulk vitamins/gameday... (calculated here: https://docs.google.com/a/unclaimedworld-game.com/spreadsheet/ccc?key=0Asy7trIq4ukddFZ1V0Zhd1ptOVJId0JfQS1TQ2FoZ2c#gid=0 and also look in the drive doc where needs are calculated )
              
                },
                DecreasePerDay = new NormalDistribution() { Mean = 1f, StandardDeviation = 0.02f },             
                LimitForDecreasedEnergy = 0.05f,
                DecreasedEnergyWeight = 0.08f, // 0.6f,
                PhysicalEffects = new PhysicalEffects()
                {
                    /*DaysAtZeroCausingCollapse = 2f,
                    DaysAtZeroCausingDeath = 2.1f,
                    DaysAtZeroDecreaseFactor = 0.5f,*/
                    LimitForReducedGrowth = 0.1f,
                    LimitForIncreasedSickness = 0.05f,
                    UseExertionFactorToDecrease = false                      
                }
            };

            humanStimulantsNeed = new NeedType() 
            {
                KeyName = "stimulants", // we use plural as with micronutrients...
                FoodNeedType = new FoodNeedType()
                {
                    FoodNutrient = "stimulants", // the idea is that coffee, drugs, cigarettes etc. can substitute for eachother
                    RequiredNutrientsAsFractionOfEntityBulk = 0.0003f, //......mp changed to 0.0003f jan 2016..... same amount as micronutrients.. 
                    IsEssential = false,
                },                
                DecreasePerDay = new NormalDistribution() { Mean = 1f, StandardDeviation = 0.02f },
                LimitForDecreasedEnergy = 0f,
                DecreasedEnergyWeight = 0f, // has no adverse effects, the need is only intended to limit the consumption
              /*  ComfortEffects = new ComfortEffects()
                {
                     ComfortWeight = 0.05f                    
                }*/
            };
        }
    }
}
