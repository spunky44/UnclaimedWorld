using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Processes;
using UWGame.ClientSide.Renderables;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities.Locomotors;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.ClientSide.GameEvents;
using UWGame.Client.Particles;
using UWGame.SimSide.XmlCollections;
using UWGame.SimSide.Entities.Locomotors.Stances;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Entities.RepairTypes;

namespace UWGame.SimSide.AllGameData
{
    public class ProcessLoader
    {
        public const float sittingLightWork = 2f; // cooking, assembling
        public const float walkingLightWork = 3f; // gather firewood
        public const float moderateWork = 4f; // cutting leaves
        public const float moderateHardWork = 6f; // digging, cutting branches
        public const float hardWork = 8f; // tree felling with axe


        public const float assembleTool = 1.1f * sittingLightWork;

        
        //MP these process names will not be displayed in the Task Manager, instead it uses the word from the JobType defined in BaseDataLoader. (MP maybe in the task manager it overrides them with the jobtype name, if it exists?)
        //I believe it uses theses process names everywhere else. TODO: make them the same?
        public const string synthesizingName = "Synthesizing"; //making enzymes with field lab
        public const string cookingName = "Cooking";
        public const string reloadSentryName = "Reload sentry";
        public const string toolmakingName = "Producing";
        public const string producingName = "Producing";
        public const string upgradingName = "Upgrading";
        public const string salvagingName = "Salvage"; //not using -ing form on purpose because it's shown on the action menu mostly.
        public const string removingName = "Removing"; //for salvaging upgrades
        public const string clearAwayName = "Clear away"; //for farm plots and mines. do not use for woodpile because it can contain items, confusing. //not using -ing form on purpose because it's shown on the action menu mostly.
        public const string settingUpName = "Setting up"; //for tents, sentry etc
        public const string constructingStructureName = "Constructing"; //for building structures
        public const string packingDownName = "Pack down"; //for structures that are merely packed down, such as tents, labs, turrets etc //not using -ing form on purpose because it's shown on the action menu mostly.
        public const string disassembleName = "Disassemble"; //such as a toolbox or other easily seperated things. Taking apart             //not using -ing form on purpose because it's shown on the action menu mostly.
        public const string harvestingName = "Harvesting";
        public const string growingName = "Growing"; 
        public const string gatheringName = "Gathering"; 
        public const string catchingName = "Catching";
        public const string fishingName = "Fishing";
        public const string huntingName = "Hunting";
        public const string miningName = "Extracting"; //like extracting Gold ore / Guano ..a very generic word. //not to be confused with extracting meat.
        public const string extractingName = "Eating";
        public const string primaryExtraction = "Extracting deposit";
        public const string cultivatingName = "Cultivating"; //for tending to the moss/shroom favorbread


        public const string salvageWithLossSummary = "When breaking this object apart, some parts will be retrieved and some may be lost.";
        public const string disassembleWithoutLossSummary = "This object can be disassembled without losing any parts.";
        public const string packingDownWithoutLossSummary = "This object can be packed down and set up repeatedly without losing any parts.";
        public const string clearAwaySummary = "Remove all traces of this structure.";


        private const float baseDaysOfWorkNeeded = 1.0f / 3.0f;//One work day //0.0f;//1.0f / 3.0f;//One work day        //0.0333333f;//- Old number represents 24 hours
            
        public const float timeToPrepareTool = 0.006f * baseDaysOfWorkNeeded;
  
        // reloading, preparing - these seem like they should fit an anim length
        public const float timeToReloadInSeconds = 1.92f; 
        public const float timeToReloadFireExtinguisherInSeconds = 1.36f; 
        public const float timeToReloadImprovisedBowInSeconds = 1.42f;
        
      
        //producing, crafting
        public const float timeForInstantCraftingTask = 0.001f * baseDaysOfWorkNeeded;  //instantaneous
        public const float timeForTinyCraftingTask = 0.01f * baseDaysOfWorkNeeded;  // make blackzpacho
        public const float timeForSmallCraftingTask = 0.02f * baseDaysOfWorkNeeded;
        public const float timeForMediumCraftingTask = 0.04f * baseDaysOfWorkNeeded;  //dissect twinkler
        public const float timeForSmithingBigTool = 0.06f * baseDaysOfWorkNeeded; 
        public const float timeForBigCraftingTask = 0.1f * baseDaysOfWorkNeeded;  //Butcher turnip
        public const float timeForExtraBigCraftingTask = 0.15f * baseDaysOfWorkNeeded;  // Loom components

        public const float timeToCreatePit = 0.04f * baseDaysOfWorkNeeded;

        //repairing
        public const float timeReconditioningSurvivalPart = 0.03f * baseDaysOfWorkNeeded;

        public const float timeForShortStandaloneTask = 0.1f * baseDaysOfWorkNeeded; //mp Atm same as big crafting task
        public const float timeForLongStandaloneTask = 0.3f * baseDaysOfWorkNeeded;
        public const float timeForVeryLongStandaloneTask = 0.9f * baseDaysOfWorkNeeded;

        public const float timeForMolecularAssembly = 0.04f * baseDaysOfWorkNeeded;
        public const float timeForMolecularPlateAssembly = 0.1f * baseDaysOfWorkNeeded;

        //gather, harvest
        public const float timeForTinyHarvestingTask = 0.01f * baseDaysOfWorkNeeded;
        public const float timeForSmallHarvestingTask = 0.02f * baseDaysOfWorkNeeded; // DaysOfWorkNeeded = 0.005f
        public const float timeForMediumHarvestingTask = 0.04f * baseDaysOfWorkNeeded;

        public const float fishing = 1.2f * sittingLightWork;

        
        // construction
        public const float constructionExertion = moderateWork;

        public const float timeForSmallPrimitiveShelter = 0.025f * baseDaysOfWorkNeeded; // a frame/tipi with one person capacity  //0.04f * baseDaysOfWorkNeeded
        public const float timeForMediumPrimitiveShelter = 0.035f * baseDaysOfWorkNeeded; // dome 2 persons                         //0.07f * baseDaysOfWorkNeeded
        public const float timeForLargePrimitiveShelter = 0.07f * baseDaysOfWorkNeeded;// wig wam 3 persons                      // 0.10f * baseDaysOfWorkNeeded
        public const float timeToDigHole = 0.02f * baseDaysOfWorkNeeded;                                                          //0.04f * baseDaysOfWorkNeeded
        public const float timeToBuildAbatis = 0.01f * baseDaysOfWorkNeeded;                                                          //0.04f * baseDaysOfWorkNeeded
        public const float timeToBuildSmallPlot = 0.07f * baseDaysOfWorkNeeded;
        public const float timeToBuildLargePlot = 0.15f * baseDaysOfWorkNeeded;


        public const float salvageStructure = moderateWork;

        public const float timeForReplenishing = 0.007f * baseDaysOfWorkNeeded;

        // shorthands for stances:
     //   public static LeggedLocomotor.Stance[] standing = new[] { LeggedLocomotor.Stance.Standing };
        public static SerializableDictionary<string, ChanceToTakeStance[]> standingProduction = new SerializableDictionary<string, ChanceToTakeStance[]>()
        {
            { "humanoid", new [] { new ChanceToTakeStance() { Stance = "standing" } } },
            { "robot", new [] { new ChanceToTakeStance() { Stance = "active" } } }
        };
       // public static LeggedLocomotor.Stance[] sitting = new[] { LeggedLocomotor.Stance.Sitting };
        public static SerializableDictionary<string, ChanceToTakeStance[]> sittingProduction = new SerializableDictionary<string, ChanceToTakeStance[]>()
        {
            { "humanoid", new [] { new ChanceToTakeStance() { Stance = "sitting" } } },
            { "robot", new [] { new ChanceToTakeStance() { Stance = "active" } } }
        };

       // public static LeggedLocomotor.Stance[] kneelingOrStanding = new[] { LeggedLocomotor.Stance.Standing, LeggedLocomotor.Stance.Kneeling };
        public static SerializableDictionary<string, ChanceToTakeStance[]> kneelingOrStandingProduction = new SerializableDictionary<string, ChanceToTakeStance[]>()
        {
            { "humanoid", new [] { new ChanceToTakeStance() { Stance = "kneeling", AddedChanceToRemainInStance = 0.1f }, new ChanceToTakeStance() { Stance = "standing", AddedChanceToRemainInStance = 0.1f } } },
            { "robot", new [] { new ChanceToTakeStance() { Stance = "active" } } }
        };

        public static SerializableDictionary<string, ChanceToTakeStance[]> kneelingProduction = new SerializableDictionary<string, ChanceToTakeStance[]>()
        {
            { "humanoid", new [] { new ChanceToTakeStance() { Stance = "kneeling" } } },
            { "robot", new [] { new ChanceToTakeStance() { Stance = "active" } } }
        };
        //public static LeggedLocomotor.Stance[] kneeling = new[] { LeggedLocomotor.Stance.Kneeling };

        public static SerializableDictionary<string, ChanceToTakeStance[]> GetSmithingStance()
        {
            return standingProduction;

        }

        public static SerializableDictionary<string, ChanceToTakeStance[]> GetToolMakingStances()
        {
            return kneelingProduction;           

        }

        public static SerializableDictionary<string, ChanceToTakeStance[]> GetSalvageStructureStances()
        {
            return kneelingOrStandingProduction;
        }
        

      /*  private static float TimeInSecondsToDays(float seconds)
        {
            return (float)(seconds / DateAndTime.secondsPerDay);
        }*/

        public static List<ProcessType> InitProcessTypes()
        {
           
            List<ProcessType> listOfProcessTypes = new List<ProcessType>();

                // physical energy expenditure
                // http://www.fao.org/docrep/003/aa040e/AA040E15.htm

            // 0.01f * baseDaysOfWorkNeeded




            #region ///Tool prepare actions///
            listOfProcessTypes.Add(new ProcessType()
            {
                KeyName = "lightFireFuelBurning",
                Name = "Lighting fire",
                WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeToPrepareTool /*0.002f*/ },
                PreparedToolModifier = StateModifier.BurningFuel, // to enable different smoke effects on the general purpose workshop
                AgentActionState = AnimAction.Mending,
                Stances = AllGameData.ProcessLoader.kneelingProduction
            });

            listOfProcessTypes.Add(new ProcessType()
                {
                    KeyName = "lightFire",
                    Name = "Lighting fire",                    
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeToPrepareTool /*0.002f*/ },

                    AgentActionState = AnimAction.Mending, //make one for field kitchen where agent is standing up.
                    Stances = AllGameData.ProcessLoader.kneelingProduction   
                });

            listOfProcessTypes.Add(new ProcessType() 
               {
                   KeyName = "kitchenImprovisedLightFire", //mp march 2016: no longer relevant:  mp todo: make one specially for improvised kitchen where the particlesystem is displaced to the right, so that we do not need to use the default position center of ground sprite. that way we can make the fire position asymmetrical and avoid a huge ground sprite with dead space to the right.
                   Name = "Lighting fire",
                   WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeToPrepareTool },

                   AgentActionState = AnimAction.Mending,
                   Stances = AllGameData.ProcessLoader.kneelingProduction,

               });

            listOfProcessTypes.Add(new ProcessType()
            {
                KeyName = "lightFireWithoutFlames",
                Name = "Lighting fire",
                WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeToPrepareTool },

                AgentActionState = AnimAction.Mending,
                Stances = AllGameData.ProcessLoader.kneelingProduction, //new[] { LeggedLocomotor.Stance.Kneeling },

            });

            listOfProcessTypes.Add(new ProcessType() 
            {
                KeyName = "forgeSmoke",//mp ordinary grey smoke because it's charcoal 
                Name = "Lighting fire",
                WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeToPrepareTool },
                
                AgentActionState = AnimAction.Mending,
                Stances = AllGameData.ProcessLoader.kneelingProduction,                
            });

            listOfProcessTypes.Add(new ProcessType() //mp ordinary grey smoke because it's charcoal 
            {
                KeyName = "kilnSmoke",
                Name = "Lighting fire",
                WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeToPrepareTool },
              

                AgentActionState = AnimAction.Mending,
                Stances = AllGameData.ProcessLoader.kneelingProduction              

            });

            listOfProcessTypes.Add(new ProcessType() //for portable field kitchen. no big smoke or flames, just steam.
            {
                KeyName = "cookAtStove",
                Name = "Cooking at stove",
                JobTypeKey = "cookingJobType",
                WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeToPrepareTool },
              
                AgentActionState = AnimAction.Mending, //mp feb 2015 we need a standing mend anim
                Stances = AllGameData.ProcessLoader.kneelingProduction, //new[] { LeggedLocomotor.Stance.Kneeling },

            });


     

            #endregion

            #region replenish actions - without job

             listOfProcessTypes.Add(new ProcessType() 
            {
                KeyName = "reloadBoltActionRifle",
                Name = "Reloading",
                WorkOrTimeNeeded = new WorkOrTime() { TimeInSecondsNeeded = timeToReloadInSeconds },

                Stances = standingProduction,
                AgentActionState = AnimAction.Reloading,
                ReplenishAction = GoalReplenish.ReplenishAction.Reload                
            });

            listOfProcessTypes.Add(new ProcessType() 
            {
                KeyName = "reloadGunpowderRifle", //mp oct 2015 todo: change reload times so they correpsond with muzzleloader/breechloader
                Name = "Reloading",
                WorkOrTimeNeeded = new WorkOrTime() { TimeInSecondsNeeded = 1.5f * timeToReloadInSeconds },

                Stances = standingProduction,
                AgentActionState = AnimAction.Reloading,
                ReplenishAction = GoalReplenish.ReplenishAction.Reload               
            });
                 
            listOfProcessTypes.Add(new ProcessType() 
            {
                KeyName = "reloadCoilRifleAmmo",
                Name = "Reloading",
                WorkOrTimeNeeded = new WorkOrTime() { TimeInSecondsNeeded = timeToReloadInSeconds }, // this should match the reload anim duration.

                Stances = standingProduction,
                AgentActionState = AnimAction.Reloading,
                ReplenishAction = GoalReplenish.ReplenishAction.Reload        
            });
            
            listOfProcessTypes.Add(new ProcessType() 
            {
                KeyName = "reloadBlunderbuss",
                Name = "Reloading",
                WorkOrTimeNeeded = new WorkOrTime() {  TimeInSecondsNeeded = 1.5f * timeToReloadInSeconds }, // won't match the reload anim length now.

                Stances = standingProduction,
                AgentActionState = AnimAction.Reloading,
                ReplenishAction = GoalReplenish.ReplenishAction.Reload        
            });

            listOfProcessTypes.Add(new ProcessType() 
            {
                KeyName = "reloadShotgun",
                Name = "Reloading",
                WorkOrTimeNeeded = new WorkOrTime() { TimeInSecondsNeeded = timeToReloadInSeconds },

                Stances = standingProduction,
                AgentActionState = AnimAction.Reloading,
                ReplenishAction = GoalReplenish.ReplenishAction.Reload        
            });
            
            listOfProcessTypes.Add(new ProcessType() 
            {
                KeyName = "reloadFireExtinguisher",
                Name = "Reloading",
                WorkOrTimeNeeded = new WorkOrTime() { TimeInSecondsNeeded = timeToReloadFireExtinguisherInSeconds },

                Stances = standingProduction,
                AgentActionState = AnimAction.Reloading,
                ReplenishAction = GoalReplenish.ReplenishAction.Reload        
            });
           
            listOfProcessTypes.Add(new ProcessType() 
            {
                KeyName = "reloadImprovisedBow",
                Name = "Reloading",
                WorkOrTimeNeeded = new WorkOrTime() { TimeInSecondsNeeded = 1.42f  },
              
                Stances = standingProduction,
                AgentActionState = AnimAction.Reloading,
                ReplenishAction = GoalReplenish.ReplenishAction.Reload        
            });
           

            listOfProcessTypes.Add(new ProcessType() 
            {
                KeyName = "refuelCampfire",
                Name = "Rekindling",
                WorkOrTimeNeeded = new WorkOrTime() { TimeInSecondsNeeded = 3f  },
              
                Stances = AllGameData.ProcessLoader.kneelingProduction,
                AgentActionState = AnimAction.Mending,
                ReplenishAction = GoalReplenish.ReplenishAction.Refuel        
            });
            
            listOfProcessTypes.Add(new ProcessType() 
            {
                KeyName = "refuelSmokeOven",
                Name = "Rekindling",
                WorkOrTimeNeeded = new WorkOrTime() { TimeInSecondsNeeded = 5f  },
              
                Stances = AllGameData.ProcessLoader.kneelingProduction,
                AgentActionState = AnimAction.Mending,
                ReplenishAction = GoalReplenish.ReplenishAction.Refuel        
            });
           
            listOfProcessTypes.Add(new ProcessType() 
            {
                KeyName = "refuelSmithy",
                Name = "Refuelling",
                WorkOrTimeNeeded = new WorkOrTime() { TimeInSecondsNeeded = 3f  },
              
                Stances = AllGameData.ProcessLoader.kneelingProduction,
                AgentActionState = AnimAction.Mending,
                ReplenishAction = GoalReplenish.ReplenishAction.Refuel        
            });
             
            listOfProcessTypes.Add(new ProcessType() 
            {
                KeyName = "refuelKiln",
                Name = "Refuelling",
                WorkOrTimeNeeded = new WorkOrTime() { TimeInSecondsNeeded = 3f  },
              
                Stances = AllGameData.ProcessLoader.kneelingProduction,
                AgentActionState = AnimAction.Mending,
                ReplenishAction = GoalReplenish.ReplenishAction.Refuel        
            });           
           
            listOfProcessTypes.Add(new ProcessType() 
            {
                KeyName = "refuelKitchen",
                Name = "Refuelling",
                WorkOrTimeNeeded = new WorkOrTime() { TimeInSecondsNeeded = 3f  },
              
                Stances = AllGameData.ProcessLoader.kneelingProduction,
                AgentActionState = AnimAction.Mending,
                ReplenishAction = GoalReplenish.ReplenishAction.Refuel        
            }); 

         
            #endregion

            #region Replenishing (MP: - with job???) as opposed to the ones above? LP: Yes, these process jobs appear in the task list.

            listOfProcessTypes.Add(new ProcessType()
            {
                Name = reloadSentryName,
                KeyName = "reloadSentryGun",
                JobTypeKey = "sentryReloadJobType",
                RequiredSkill = "menial",
                PhysicalWorkFactor = moderateWork,
                Stances = kneelingProduction,
                /*  Inputs = new[] { new Input() { Entity = "item:sentryGunAmmo", 
                      IsConsumed = true, // not really... but does not become a part either... 
                      Amount = new InputAmount() { NoOfItems = 1 } }},
                  */
                WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForReplenishing },

            });


            #endregion

            #region ///Butchering///

            float butcher = moderateHardWork;


            #region butcherMegapod
            listOfProcessTypes.Add(new ProcessType()
            {
                Name = "Butchering",
                KeyName = "butcherMegapod",
                JobTypeKey = "butcheringJobType",
                RequiredSkill = "butchering",
                PhysicalWorkFactor = butcher,
                  
                Inputs = new[] { new Input() { Entity = "item:megapodCarcass", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1, Substances = new[] { "meat", "guts", "hide", "bones" } } } },
                Outputs = new[]
                     {
                        new Output(){ EntityTypeToCreate = "item:megapodGreenHide", Amount = new OutputAmount(){  Bulk = new Bulk(){InputSubstance = "hide"}}},                   
                        new Output(){ EntityTypeToCreate = "item:megapodBrain", Amount = new OutputAmount(){ NoOfItems = 1 }}, 
                    },
                ProcessToolSetKey = "toolSetButcherFlesh",
                WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask, MultiplyByBulk = true },
                AgentActionState = AnimAction.Butchering,
                AgentAnimationStates = null,
            });
            #endregion

            #region butcherWhipjaw
            listOfProcessTypes.Add(new ProcessType()
            {
                Name = "Butchering",
                KeyName = "butcherWhipjaw",
                JobTypeKey = "butcheringJobType",
                RequiredSkill = "butchering",
                PhysicalWorkFactor = butcher,
                                                                    //testing: IsConsumed = false  ..impossible, only works for processes with parts.   
                Inputs = new[] { new Input() { Entity = "item:whipjawCarcass", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1, Substances = new[] { "meat", "guts", "hide", "bones" } } } },
                Outputs = new[]
                     {
                        new Output(){ EntityTypeToCreate = "item:whipjawGreenHide", Amount = new OutputAmount(){  Bulk = new Bulk(){InputSubstance = "hide"}}},
                        new Output(){ EntityTypeToCreate = "item:whipjawBrain", Amount = new OutputAmount(){ NoOfItems = 1 }},   
                          //
                    },
                ProcessToolSetKey = "toolSetButcherFlesh",
                WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask, MultiplyByBulk = true },
                AgentActionState = AnimAction.Butchering,
                AgentAnimationStates = null,
            });
            #endregion

                #region butcherTurnip
                listOfProcessTypes.Add(new ProcessType()
                 {
                     Name = "Butchering",
                     KeyName = "butcherTurnip",
                     JobTypeKey = "butcheringJobType",
                     RequiredSkill = "butchering",
                     PhysicalWorkFactor = butcher,
                     //Stances = standing,
                     Inputs = new[] { new Input() { Entity = "item:turnipCarcass", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1, Substances = new[] { "meat", "guts", "shell", "hide" } } } },
                     Outputs = new[]
                     {
                        new Output(){ EntityTypeToCreate = "item:turnipMeat", Amount = new OutputAmount(){  Bulk = new Bulk(){InputSubstance = "meat"}}},                   
                        new Output(){ EntityTypeToCreate = "item:turnipShell", Amount = new OutputAmount(){  Bulk = new Bulk(){InputSubstance = "shell"}}},
                        new Output(){ EntityTypeToCreate = "item:turnipGuts", Amount = new OutputAmount(){  Bulk = new Bulk(){InputSubstance = "guts"}}},
                        new Output(){ EntityTypeToCreate = "item:turnipBrain", Amount = new OutputAmount(){ NoOfItems = 1 }}      
                    },
                     ProcessToolSetKey = "toolSetButcherTurnip",
                     WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForBigCraftingTask, MultiplyByBulk = true },
                     AgentActionState = AnimAction.Butchering,
                     AgentAnimationStates = null,
                 });
                #endregion

                #region butcherThunderChicken
                listOfProcessTypes.Add(new ProcessType()
                 {
                     Name = "Butchering",   // http://en.wikipedia.org/wiki/Slaughterhouse  Typically 45–50% of the animal can be turned into edible products (meat). About 5-15% is waste, and the remaining 40–45% of the animal is turned into byproducts such as leather, soaps, candles (tallow), and adhesives
                     KeyName = "butcherThunderChicken",
                     JobTypeKey = "butcheringJobType",
                     RequiredSkill = "butchering",
                     PhysicalWorkFactor = butcher,
                     Stances = standingProduction,
                     Inputs = new[] { new Input() { Entity = "item:thunderChickenCarcass", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1, Substances = new[] { "meat", "guts","hide","bones"} } } }, //bso remember to include all substances if u want the item to be removed after action
                    
                     Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:thunderChickenMeat", Amount = new OutputAmount(){ Bulk = new Bulk(){ InputSubstance = "meat"}}}, //Bso the bulk object is either a fraction of total bulk or an entire inputSubstance
                        new Output(){ EntityTypeToCreate = "item:thunderChickenGreenHide", Amount = new OutputAmount(){  Bulk = new Bulk(){ InputSubstance = "hide"}}},  
                        new Output(){ EntityTypeToCreate = "item:thunderChickenGuts", Amount = new OutputAmount(){  Bulk = new Bulk(){ InputSubstance = "guts"}}},
                        new Output(){ EntityTypeToCreate = "item:thunderChickenBrain", Amount = new OutputAmount(){  NoOfItems = 1 }},
                 //       new Output(){ EntityType = "item:thunderChickenBones", Amount = new OutputAmount(){  Bulk = new Bulk(){ InputSubstance = "bones", FractionOfInputBulk = 0.2f }}},  // MP: have no use for now
                    },

                     ProcessToolSetKey = "toolSetButcherFlesh",

                     WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },

                     AgentActionState = AnimAction.Butchering,
                     AgentAnimationStates = null,

                 });
                #endregion
   


                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = "Butchering",
                    KeyName = "butcherTwinkler",
                    JobTypeKey = "butcheringJobType",
                    RequiredSkill = "butchering",
                    PhysicalWorkFactor = butcher,
                    Stances = standingProduction,
                    Inputs = new[] { new Input() { Entity = "item:quaditeCarcass", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1, Substances = new[]{"meat", "plating", "guts"} } } },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:twinklerMeat",  Amount = new OutputAmount(){  Bulk = new Bulk(){ InputSubstance = "meat" /*FractionOfInputBulk = 0.5f*/} }}, 
                        new Output(){ EntityTypeToCreate = "item:twinklerPlating", Amount = new OutputAmount(){  Bulk = new Bulk(){ InputSubstance = "plating" /*FractionOfInputBulk = 0.5f*/} }},
                   //     new Output(){ EntityType = "item:twinklerGuts", Amount = new Amount(){ FractionOfInputBulk = 0.2f }},  // MP: not used
                    },

                    ProcessToolSetKey = "toolSetButcherFlesh",

                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },

                    AgentActionState = AnimAction.Butchering,
                    AgentAnimationStates = null,

                });

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = "Butchering",
                    KeyName = "butcherBushDragon",
                    JobTypeKey = "butcheringJobType",
                    RequiredSkill = "butchering",
                    PhysicalWorkFactor = butcher,
                    Stances = standingProduction,
                
                    Inputs = new[] { new Input() { Entity = "item:bushDragonCarcass", IsConsumed = true,  Amount = new InputAmount() { NoOfItems = 1} } },

                    Outputs = new[] {
                          new Output(){ EntityTypeToCreate = "item:bushDragonPoisonGlands", Amount = new OutputAmount(){ NoOfItems = 1,  Bulk = new Bulk(){ FractionOfInputBulk = 0.05f} }},
                          new Output(){ EntityTypeToCreate = "item:bushDragonHarvestedCarcass",  IsWasteProduct = true, Amount = new OutputAmount(){ NoOfItems = 1,  Bulk = new Bulk(){ FractionOfInputBulk = 0.95f }}},
                  //      new Output(){ EntityType = "item:bushDragonMeat", Amount = new Amount(){ FractionOfInputBulk = 0.3f }},
                      
                   //     new Output(){ EntityType = "item:bushDragonWings", Amount = new Amount(){ NoOfItems = 3, FractionOfInputBulk = 0.1f }},  //
                   //     new Output(){ EntityType = "item:bushDragonBones", Amount = new Amount(){ FractionOfInputBulk = 0.3f }},
                    },

                    ProcessToolSetKey = "toolSetButcherFlesh",

                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },

                    AgentActionState = AnimAction.Butchering,
                    AgentAnimationStates = null,

                });
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = "Butchering",
                    KeyName = "butcherBinalRat",
                    JobTypeKey = "butcheringJobType",
                    RequiredSkill = "menial", //mp just cutting it up all rough.
                    PhysicalWorkFactor = sittingLightWork,
                    Stances = standingProduction,

                    Inputs = new[] { new Input() { Entity = "item:binalRatCarcass", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1 } } },

                    Outputs = new[] { new Output() { EntityTypeToCreate = "item:binalRatChunk", Amount = new OutputAmount() { NoOfItems = 2, Bulk = new Bulk() { FractionOfInputBulk = 0.9f } } } },

                    ProcessToolSetKey = "toolSetButcherFlesh",
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForTinyCraftingTask },
                    AgentActionState = AnimAction.Butchering,
                    AgentAnimationStates = null,
                });



                #endregion


                #region ///Cooking///



                #region makeBlackzpacho
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = cookingName,
                    KeyName = "makeBlackzpacho",
                    JobTypeKey = "cookingJobType",
                    RequiredSkill = "cooking",
                    PhysicalWorkFactor = sittingLightWork,
                    Stances = kneelingProduction,
                    Inputs = new[] 
                    { 
                        new Input() { Entity = "item:blackpulp", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 2 }},
                        new Input() { Entity = "item:blackpulpEnzyme", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1 }}
                    },

                    Outputs = new Output[] {
                        new Output(){ EntityTypeToCreate = "item:blackzpacho", Amount = new OutputAmount(){ NoOfItems = 1 },  ToolContainerTagsToPlaceIn = new[]{"cookingPot"}} 
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForTinyCraftingTask },
                    ProcessToolSetKey = "toolSetMakeBlendedFood",

                });
                #endregion
                #region makeTurnipRoast
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = cookingName,
                    KeyName = "makeTurnipRoast",
                    JobTypeKey = "cookingJobType",
                    RequiredSkill = "cooking",
                    PhysicalWorkFactor = sittingLightWork,
                    //DesignerItemTag = "ediblewhencooked, ediblewhenraw",
                    Stances = kneelingProduction,
                    Inputs = new[] { new Input() { Entity = "item:turnipMeat", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1 } } },
                    Outputs = new[] { new Output(){ EntityTypeToCreate = "item:turnipRoast", Amount = new OutputAmount(){ NoOfItems = 1 }} },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    ProcessToolSetKey = "toolSetFireplace",
                });
                #endregion
                #region makeTurnipRawSausage
                listOfProcessTypes.Add(new ProcessType() //
                {
                    Name = cookingName,
                    KeyName = "makeTurnipRawSausage",
                    JobTypeKey = "cookingJobType",
                    RequiredSkill = "cooking",
                    PhysicalWorkFactor = sittingLightWork,
                    Stances = standingProduction,
                    WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart, //ferments on its own
                    Inputs = new[] { new Input() { Entity = "item:turnipMeat", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 3 } } ,
                                     new Input() { Entity = "item:turnipGuts", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1 }},
                                     new Input() { Entity = "item:crystalBerries", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1 }} //or salt?
                    },
                    Outputs = new Output[] {
                        new Output(){ EntityTypeToCreate = "item:turnipRawSausage", Amount = new OutputAmount(){ NoOfItems = 5 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForShortStandaloneTask }, //should not take too long, because a drying step follows. IRL, this means 2 days of fermenting, one month of drying.
                    ProcessToolSetKey = "toolSetKitchen",
                });
                #endregion
                #region makeTurnipSalami
                listOfProcessTypes.Add(new ProcessType() //
                {
                    Name = cookingName,
                    KeyName = "makeTurnipSalami",
                    JobTypeKey = "cookingJobType",
                    RequiredSkill = "cooking",
                    PhysicalWorkFactor = sittingLightWork,
                    Stances = standingProduction,
                    WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart, //dries on its own
                    Inputs = new[] { new Input() { Entity = "item:turnipRawSausage", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 5 } } ,

                    },

                    Outputs = new Output[] {
                        new Output(){ EntityTypeToCreate = "item:turnipSalami", Amount = new OutputAmount(){ NoOfItems = 5 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForLongStandaloneTask }, //should  take long
                    ProcessToolSetKey = "toolSetSalamiDrying",
                });

                #endregion
                #region makeTurnipFriedSausage
                listOfProcessTypes.Add(new ProcessType() // when you dont wanna wait to make salami..
                {
                    Name = cookingName,
                    KeyName = "makeTurnipFriedSausage",
                    JobTypeKey = "cookingJobType",
                    RequiredSkill = "cooking",
                    PhysicalWorkFactor = sittingLightWork,
                    Stances = kneelingProduction,                    
                    Inputs = new[] { new Input() { Entity = "item:turnipRawSausage", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1 } } ,

                    },
                    Outputs = new Output[] {
                        new Output(){ EntityTypeToCreate = "item:turnipFriedSausage", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask }, //
                    ProcessToolSetKey = "toolSetFireplace",
                });

                #endregion
                #region makeRoastedStreakFin
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = cookingName,
                    KeyName = "makeRoastedStreakFin",
                    JobTypeKey = "cookingJobType",
                    RequiredSkill = "cooking",
                    PhysicalWorkFactor = sittingLightWork,
                    Stances = kneelingProduction,
                    Inputs = new[] { new Input() { Entity = "item:streakFin", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1 } } },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:roastedStreakFin", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForTinyCraftingTask }, //was timeForSmallCraftingTask  when it was 1 in 2 out
                    ProcessToolSetKey = "toolSetFireplace",

                });
                #endregion
                #region makeRoastedCarbonTail
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = cookingName,
                    KeyName = "makeRoastedCarbonTail",
                    JobTypeKey = "cookingJobType",
                    RequiredSkill = "cooking",
                    PhysicalWorkFactor = sittingLightWork,
                    Stances = kneelingProduction,
                    Inputs = new[] { new Input() { Entity = "item:carbonTail", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1 } } },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:roastedCarbonTail", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForTinyCraftingTask }, //was timeForSmallCraftingTask  when it was 1 in 2 out
                    ProcessToolSetKey = "toolSetFireplace",
                });
                #endregion
                #region makeRoastedAlabasterRay
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = cookingName,
                    KeyName = "makeRoastedAlabasterRay",
                    JobTypeKey = "cookingJobType",
                    RequiredSkill = "cooking",
                    PhysicalWorkFactor = sittingLightWork,
                    Stances = kneelingProduction,
                    Inputs = new[] { new Input() { Entity = "item:alabasterRay", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1 } } },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:roastedAlabasterRay", Amount = new OutputAmount(){ NoOfItems = 2 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    ProcessToolSetKey = "toolSetFireplace",
                });

                #endregion
                #region makeAlabasterStew
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = cookingName,
                    KeyName = "makeAlabasterStew",
                    JobTypeKey = "cookingJobType",
                    RequiredSkill = "cooking",
                    PhysicalWorkFactor = sittingLightWork,
                    Stances = kneelingProduction,
                    Inputs = new[] { new Input() { Entity = "item:alabasterRay", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1 } } },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:alabasterStew", Amount = new OutputAmount(){ NoOfItems = 2 },  ToolContainerTagsToPlaceIn = new[]{"cookingPot"}}  
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    ProcessToolSetKey = "toolSetMakeStew",
                });
                #endregion
                #region makeGrilledUrsinix



                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = cookingName,
                    KeyName = "makeGrilledUrsinix",
                    JobTypeKey = "cookingJobType",
                    RequiredSkill = "cooking",
                    PhysicalWorkFactor = sittingLightWork,
                    Stances = kneelingProduction,
                    Inputs = new[] { new Input() { Entity = "item:driedUrsinix", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1 } } },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:grilledUrsinix", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    ProcessToolSetKey = "toolSetFireplace",
                });
                #endregion
                #region makeThunderChickenStew

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = cookingName,
                    KeyName = "makeThunderChickenStew",
                    JobTypeKey = "cookingJobType",
                    RequiredSkill = "cooking",
                    PhysicalWorkFactor = sittingLightWork,
                    Stances = kneelingProduction,
                    Inputs = new[] { new Input() { Entity = "item:thunderChickenGuts", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1 } } },

                    Outputs = new Output[] {
                        new Output(){ EntityTypeToCreate = "item:thunderChickenStew", Amount = new OutputAmount(){ NoOfItems = 1 }, ToolContainerTypesToPlaceIn = new[]{ "item:advancedCookingPot"}}  // 

                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    ProcessToolSetKey = "toolSetMakeStew", 

                });
                #endregion
                #region makeThunderChickenSkewers

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = cookingName,
                    KeyName = "makeThunderChickenSkewers",
                    JobTypeKey = "cookingJobType",
                    RequiredSkill = "cooking",
                    PhysicalWorkFactor = sittingLightWork,
                    Stances = kneelingProduction,
                    Inputs = new[] { new Input() { Entity = "item:thunderChickenMeat", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1 } } },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:thunderChickenSkewers", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    ProcessToolSetKey = "toolSetFireplace",
                });
                #endregion
                
                //mp july 2015.  this is commented out because it didn't work. carcass er immovable material og immovable Tool (fireplace) i samme proces. there's a validation for it.
                //    KeyName = "makeThunderChickenRoast",


                #region makeRoastedPhantomWeaver
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = cookingName,
                    KeyName = "makeRoastedPhantomWeaver",
                    JobTypeKey = "cookingJobType",
                    RequiredSkill = "cooking",
                    PhysicalWorkFactor = sittingLightWork,
                    Stances = kneelingProduction,
                    Inputs = new[] { new Input() { Entity = "item:phantomWeaver", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1 } } },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:roastedPhantomWeaver", Amount = new OutputAmount(){ NoOfItems = 2 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    ProcessToolSetKey = "toolSetFireplace",
                });
                #endregion
                #region makeRoastedWebWing

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = cookingName,
                    KeyName = "makeRoastedWebWing",
                    JobTypeKey = "cookingJobType",
                    RequiredSkill = "cooking",
                    PhysicalWorkFactor = sittingLightWork,
                    Stances = kneelingProduction,
                    Inputs = new[] { new Input() { Entity = "item:webWing", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1 } } },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:roastedWebWing", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    ProcessToolSetKey = "toolSetFireplace",
                });
                #endregion
                #region makeRoastedCrestedFoiler

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = cookingName,
                    KeyName = "makeRoastedCrestedFoiler",
                    JobTypeKey = "cookingJobType",
                    RequiredSkill = "cooking",
                    PhysicalWorkFactor = sittingLightWork,
                    Stances = kneelingProduction,
                    Inputs = new[] { new Input() { Entity = "item:crestedFoiler", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1 } } },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:roastedCrestedFoiler", Amount = new OutputAmount(){ NoOfItems = 2 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    ProcessToolSetKey = "toolSetFireplace",
                });
                #endregion
                #region makeRoastedGoldenCenobite

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = cookingName,
                    KeyName = "makeRoastedGoldenCenobite",
                    JobTypeKey = "cookingJobType",
                    RequiredSkill = "cooking",
                    PhysicalWorkFactor = sittingLightWork,
                    Stances = kneelingProduction,
                    Inputs = new[] { new Input() { Entity = "item:goldenCenobite", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1 } } },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:roastedGoldenCenobite", Amount = new OutputAmount(){ NoOfItems = 2 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    ProcessToolSetKey = "toolSetFireplace",
                });

                #endregion
                #region makeRoastedMuckGrinder
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = cookingName,
                    KeyName = "makeRoastedMuckGrinder",
                    JobTypeKey = "cookingJobType",
                    RequiredSkill = "cooking",
                    PhysicalWorkFactor = sittingLightWork,
                    Stances = kneelingProduction,
                    Inputs = new[] { new Input() { Entity = "item:muckGrinder", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1 } } },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:roastedMuckGrinder", Amount = new OutputAmount(){ NoOfItems = 2 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    ProcessToolSetKey = "toolSetFireplace",
                });
                #endregion
                #region makeRoastedSpriteSlug
                /* not used. eaten raw
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = cookingName,
                    KeyName = "makeRoastedSpriteSlug",
                    JobTypeKey = "cookingJobType",
                    RequiredSkill = "cooking",
                    PhysicalWorkFactor = sittingLightWork,
                    Stances = kneelingProduction,
                    Inputs = new[] { new Input() { Entity = "item:spriteSlug", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1 } } },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:roastedSpriteSlug", Amount = new OutputAmount(){ NoOfItems = 2 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    ProcessToolSetKey = "toolSetFireplace",
                });
*/
                #endregion
                #region makeRoastedCrazyDweller
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = cookingName,
                    KeyName = "makeRoastedCrazyDweller",
                    JobTypeKey = "cookingJobType",
                    RequiredSkill = "cooking",
                    PhysicalWorkFactor = sittingLightWork,
                    Stances = kneelingProduction,
                    Inputs = new[] { new Input() { Entity = "item:crazyDweller", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1 } } },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:roastedCrazyDweller", Amount = new OutputAmount(){ NoOfItems = 2 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    ProcessToolSetKey = "toolSetFireplace",
                });
                #endregion
                #region makeRoastedDaggermouth

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = cookingName,
                    KeyName = "makeRoastedDaggermouth",
                    JobTypeKey = "cookingJobType",
                    RequiredSkill = "cooking",
                    PhysicalWorkFactor = sittingLightWork,
                    Stances = kneelingProduction,
                    Inputs = new[] { new Input() { Entity = "item:daggermouth", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1 } } },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:roastedDaggermouth", Amount = new OutputAmount(){ NoOfItems = 3 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    ProcessToolSetKey = "toolSetFireplace",
                });

                #endregion
                #region makeRoastedImpEel
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = cookingName,
                    KeyName = "makeRoastedImpEel",
                    JobTypeKey = "cookingJobType",
                    RequiredSkill = "cooking",
                    PhysicalWorkFactor = sittingLightWork,
                    Stances = kneelingProduction,
                    Inputs = new[] { new Input() { Entity = "item:impEel", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1 } } },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:roastedImpEel", Amount = new OutputAmount(){ NoOfItems = 3 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    ProcessToolSetKey = "toolSetFireplace",
                });
                #endregion
                #region makeMinnowSoup

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = cookingName,  //making soup from live minnows should give same result as dead minnows
                    KeyName = "makeMinnowSoup",
                    JobTypeKey = "cookingJobType",
                    RequiredSkill = "cooking",
                    PhysicalWorkFactor = sittingLightWork,
                    Stances = kneelingProduction,
                    Inputs = new[] { new Input() { Entity = "item:minnowsLive", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1 } } },

                    Outputs = new Output[] {
                        new Output(){ EntityTypeToCreate = "item:minnowSoup", Amount = new OutputAmount(){ NoOfItems = 2 },  ToolContainerTagsToPlaceIn = new[]{"cookingPot"}} 
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    ProcessToolSetKey = "toolSetMakeStew",

                });
                #endregion
                #region makeDeadMinnowSoup
                /* listOfProcessTypes.Add(new ProcessType()  //not in use right now 26-5-14
                {
                    Name = cookingName,
                    KeyName = "makeDeadMinnowSoup",
                    RequiredSkill = "cooking",
                    PhysicalWorkFactor = sittingLightWork,
                    Stances = kneeling,
                    Inputs = new[] { new Input() { Entity = "item:minnowsDead", IsConsumed = true, Amount = new Amount() { NoOfItems = 1 } } },

                    Outputs = new Output[] {
                        new Output(){ EntityType = "item:minnowSoup", Amount = new Amount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysOfWorkNeeded = timeForSmallCraftingTask },
                    Tools = "makeStew",

                });*/
                #endregion
                #region makeWaterCanePorridge

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = cookingName,
                    KeyName = "makeWaterCanePorridge",
                    JobTypeKey = "cookingJobType",
                    RequiredSkill = "cooking",
                    PhysicalWorkFactor = sittingLightWork,
                    Stances = kneelingProduction,
                    Inputs = new[] { new Input() { Entity = "item:waterCaneSeeds", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 2 } } },

                    Outputs = new Output[] {
                        new Output(){ EntityTypeToCreate = "item:waterCanePorridge", Amount = new OutputAmount(){ NoOfItems = 2 },  ToolContainerTagsToPlaceIn = new[]{"cookingPot"}} 
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    ProcessToolSetKey = "toolSetMakeStew",

                });
                #endregion
                #region smokeThunderChicken

                listOfProcessTypes.Add(new ProcessType()
                 {
                     Name = cookingName,
                     KeyName = "smokeThunderChicken",
                     JobTypeKey = "cookingJobType",
                     RequiredSkill = "cooking",
                     PhysicalWorkFactor = sittingLightWork,
                     Stances = kneelingProduction,
                     WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart,
                     Inputs = new[] { new Input() { Entity = "item:thunderChickenMeat", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 6 } } },

                     Outputs = new Output[] {
                        new Output(){ EntityTypeToCreate = "item:smokedThunderChicken", Amount = new OutputAmount(){ NoOfItems = 6 },  ToolContainerTagsToPlaceIn = new[]{"smokeOven"}}
                    },
                     WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForShortStandaloneTask },
                     ProcessToolSetKey = "toolSetSmokeOven",

                 });
                #endregion
                #region smokeAlabasterRay

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = cookingName,
                    KeyName = "smokeAlabasterRay",
                    JobTypeKey = "cookingJobType",
                    RequiredSkill = "cooking",
                    PhysicalWorkFactor = sittingLightWork,
                    Stances = kneelingProduction,
                    WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart,
                    Inputs = new[] { new Input() { Entity = "item:alabasterRay", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 6 } } },

                    Outputs = new Output[] {
                        new Output(){ EntityTypeToCreate = "item:smokedAlabasterRay", Amount = new OutputAmount(){ NoOfItems = 6 },  ToolContainerTagsToPlaceIn = new[]{"smokeOven"}}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForShortStandaloneTask },
                    ProcessToolSetKey = "toolSetSmokeOven",

                });
                #endregion
                #region smokeStreakFin

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = cookingName,
                    KeyName = "smokeStreakFin",
                    JobTypeKey = "cookingJobType",
                    RequiredSkill = "cooking",
                    PhysicalWorkFactor = sittingLightWork,
                    Stances = kneelingProduction,
                    WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart,
                    Inputs = new[] { new Input() { Entity = "item:streakFin", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 6 } } },

                    Outputs = new Output[] {
                        new Output(){ EntityTypeToCreate = "item:smokedStreakFin", Amount = new OutputAmount(){ NoOfItems = 6 },  ToolContainerTagsToPlaceIn = new[]{"smokeOven"}}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForShortStandaloneTask },
                    ProcessToolSetKey = "toolSetSmokeOven",

                });
                #endregion
                #region smokeCarbonTail
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = cookingName,
                    KeyName = "smokeCarbonTail",
                    JobTypeKey = "cookingJobType",
                    RequiredSkill = "cooking",
                    PhysicalWorkFactor = sittingLightWork,
                    Stances = kneelingProduction,
                    WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart,
                    Inputs = new[] { new Input() { Entity = "item:carbonTail", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 6 } } },

                    Outputs = new Output[] {
                        new Output(){ EntityTypeToCreate = "item:smokedCarbonTail", Amount = new OutputAmount(){ NoOfItems = 6 },  ToolContainerTagsToPlaceIn = new[]{"smokeOven"}} 
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForShortStandaloneTask },
                    ProcessToolSetKey = "toolSetSmokeOven",

                });

                #endregion
                #region smokeTurnip

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = cookingName,
                    KeyName = "smokeTurnip",
                    JobTypeKey = "cookingJobType",
                    RequiredSkill = "cooking",
                    PhysicalWorkFactor = sittingLightWork,
                    Stances = kneelingProduction,
                    WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart,
                    Inputs = new[] { new Input() { Entity = "item:turnipMeat", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 6 } } }, //mp july 2015. this is a fix so that it's more practical to smoke turnip. Rebalance evertyhing later. 

                    Outputs = new Output[] {
                        new Output(){ EntityTypeToCreate = "item:smokedTurnip", Amount = new OutputAmount(){ NoOfItems = 6 },  ToolContainerTagsToPlaceIn = new[]{"smokeOven"}}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForShortStandaloneTask }, //
                    ProcessToolSetKey = "toolSetSmokeOven",

                });
                #endregion
                
                #region makeHardTack
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = cookingName,
                    KeyName = "makeHardtack",
                    JobTypeKey = "cookingJobType",
                    RequiredSkill = "cooking",
                    PhysicalWorkFactor = moderateWork,
                    WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart, //made in kiln
                    Stances = kneelingProduction,
                    Inputs = new[]
                    {
                         new Input(){ Entity = "item:glassyCreeperPods",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 6 } } //todo update excel sheet.   mp I'm skipping the "Flour" stage for now. it can later be included...
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:hardtack", Amount = new OutputAmount(){ NoOfItems = 6 },  ToolContainerTagsToPlaceIn = new[]{"kiln"} }
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask }, //mp hardtack requires 1 hour of baking so it's not that long, shorter than smoking
                    ProcessToolSetKey = "toolSetOven",
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion
                

                #region makeCrispbread //not used now
          /*      listOfProcessTypes.Add(new ProcessType()
                {
                    Name = cookingName,
                    KeyName = "makeCrispbread",
                    RequiredSkill = "cooking",
                    PhysicalWorkFactor = moderateWork,
                    WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart, //baked really short a few minutes actually, but maybe the dough needs to prepare itself with yeast..?
                    Stances = kneelingProduction,
                    Inputs = new[]
                    {
                         new Input(){ Entity = "item:glassyCreeperPods",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 20 } } //todo update excel sheet.   mp I'm skipping the "Flour" stage for now. it can later be included...
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:crispbread", Amount = new OutputAmount(){ NoOfItems = 20 },   } //todo: ToolContainerTagsToPlaceIn = new[]{ }
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask }, //
                    ProcessToolSetKey = "toolSetKitchen", //todo. communal kitchen
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });*/
                #endregion

                #region dryThunderChicken
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = cookingName,
                    KeyName = "makeDriedThunderChicken",
                    JobTypeKey = "cookingJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = sittingLightWork,
                    Stances = standingProduction,
                    WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart, //mp 
                    Inputs = new[] 
                    { 
                        new Input() { Entity = "item:thunderChickenMeat", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 6, } }, //was 3
                        new Input() { Entity = "item:salt", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1, } }
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:driedThunderChicken", Amount = new OutputAmount(){  NoOfItems = 6}, ToolContainerTagsToPlaceIn = new[]{"meatDryingRack"}}, 
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForLongStandaloneTask },                     
                    ProcessToolSetKey = "toolSetMeatDrying",
                    AgentAnimationStates = new[] { AnimModifier.Improvised } //mp todo make it so that he is standing up close to the rack, hanging up meat.
               //     AgentActionState = AnimAction.Butchering, //mp copied from butchering? todo
                    //     AgentAnimationStates = null, //mp copied from butchering?
                });
                #endregion

                #region dryStreakFin //makes it an ingredient, not directly edible.
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = cookingName,
                    KeyName = "makeDriedStreakFin",
                    JobTypeKey = "cookingJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = sittingLightWork,
                    Stances = standingProduction,
                    WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart, //mp 
                    Inputs = new[] 
                    { 
                        new Input() { Entity = "item:streakFin", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 10, } }, //
                        new Input() { Entity = "item:salt", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1, } } //maybe 2??
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:driedSaltedStreakFin", Amount = new OutputAmount(){  NoOfItems = 10}, ToolContainerTagsToPlaceIn = new[]{"meatDryingRack"}}, 
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForLongStandaloneTask },
                    ProcessToolSetKey = "toolSetMeatDrying",
                    AgentAnimationStates = new[] { AnimModifier.Improvised } //mp todo make it so that he is standing up close to the rack, hanging up meat.

                });
                #endregion
                //

                #region makeDesalinatedStreakFin
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = cookingName, //
                    KeyName = "makeDesalinatedStreakFin",
                    JobTypeKey = "cookingJobType",
                    RequiredSkill = "cooking", //
                    PhysicalWorkFactor = sittingLightWork,
                    WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart, //Needs To dissolve on its own
                    Inputs = new[]
                    {

                         new Input(){ Entity = "item:driedSaltedStreakFin",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 5 } }, //to limit amount of tasks...it is made with 10 output, so player should have enough.

                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:desalinatedStreakFin", Amount = new OutputAmount(){ NoOfItems = 5 },  ToolContainerTagsToPlaceIn = new[]{"liquidContainerNoHeat"}} //
                        
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForShortStandaloneTask }, //
                    ProcessToolSetKey = "toolSetJarNoHeating",
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region makeHexapineSalad
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = cookingName,
                    KeyName = "makeHexapineSalad",
                    JobTypeKey = "cookingJobType",
                    RequiredSkill = "cooking",
                    PhysicalWorkFactor = sittingLightWork,
                    Stances = kneelingProduction,
                    Inputs = new[] 
                    { 
                        new Input() { Entity = "item:hexapineLeaves", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 2 }},
                        new Input() {  Entity = "item:hexapineLeavesEnzyme", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1 }}
                    },
                    Outputs = new Output[] {
                        new Output(){ EntityTypeToCreate = "item:hexapineSalad", Amount = new OutputAmount(){ NoOfItems = 1 },  ToolContainerTagsToPlaceIn = new[]{"cookingPot"}} 
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForTinyCraftingTask },
                    ProcessToolSetKey = "toolSetMakeBlendedFood",

                });
                #endregion
                #region makeFingerPot
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = cookingName,
                    KeyName = "makeFingerPot", //finger fruit is a seed used  in farming, so it's improtant that it cannot be eaten raw
                    JobTypeKey = "cookingJobType",
                    RequiredSkill = "cooking",
                    PhysicalWorkFactor = sittingLightWork,
                    Stances = kneelingProduction,
                    Inputs = new[] 
                    { 
                        new Input() { Entity = "item:fingerFruit", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 2 }}

                    },
                    Outputs = new Output[] {
                        new Output(){ EntityTypeToCreate = "item:fingerPot", Amount = new OutputAmount(){ NoOfItems = 1 },  ToolContainerTagsToPlaceIn = new[]{"cookingPot"}} 
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForTinyCraftingTask },
                    ProcessToolSetKey = "toolSetMakeStew",

                });
                #endregion
                #region makeFermentedFingerFruit
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = cookingName, //
                    KeyName = "makeFermentedFingerFruit",
                    JobTypeKey = "cookingJobType",
                    RequiredSkill = "cooking", //
                    PhysicalWorkFactor = sittingLightWork,
                    WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart, //Needs To ferment on its own
                    Inputs = new[]
                    {

                         new Input(){ Entity = "item:fingerFruit",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 2 } },
                         new Input(){ Entity = "item:salt",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } },
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:fermentedFingerFruit", Amount = new OutputAmount(){ NoOfItems = 2 },  ToolContainerTagsToPlaceIn = new[]{"liquidContainerNoHeat"}} //
                        
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForLongStandaloneTask }, //
                    ProcessToolSetKey = "toolSetFermentWithKitchenAndJar",                     
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region makePickledCarbonTail
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = cookingName, //
                    KeyName = "makePickledCarbonTail",
                    JobTypeKey = "cookingJobType",
                    RequiredSkill = "cooking", //
                    PhysicalWorkFactor = sittingLightWork,
                
                    Inputs = new[]
                    {

                         new Input(){ Entity = "item:carbonTail",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 6 } },
                         new Input(){ Entity = "item:vinegar",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } },
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:pickledCarbonTail", Amount = new OutputAmount(){ NoOfItems = 6 },  ToolContainerTagsToPlaceIn = new[]{"liquidContainerNoHeat"}} //make sure there's room
                        
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask }, //mp fries the fish and dumps it in vinegar.  quick
                    ProcessToolSetKey = "toolSetFermentWithKitchenAndJar",
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region makePickledAlabasterRay
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = cookingName, //
                    KeyName = "makePickledAlabasterRay",
                    JobTypeKey = "cookingJobType",
                    RequiredSkill = "cooking", //
                    PhysicalWorkFactor = sittingLightWork,
                  
                    Inputs = new[]
                    {
                         new Input(){ Entity = "item:alabasterRay",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 2 } },
                         new Input(){ Entity = "item:vinegar",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } },
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:pickledAlabasterRay", Amount = new OutputAmount(){ NoOfItems = 3 },  ToolContainerTagsToPlaceIn = new[]{"liquidContainerNoHeat"}} //
                        
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask }, //mp pickling with vinegar is very quick
                    ProcessToolSetKey = "toolSetFermentWithKitchenAndJar",
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region makeVinegar
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = cookingName, //
                    KeyName = "makeVinegar",
                    JobTypeKey = "cookingJobType",
                    RequiredSkill = "cooking", //
                    PhysicalWorkFactor = sittingLightWork,
                    WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart, //on its own
                    Inputs = new[]
                    {

                    //     new Input(){ Entity = "item:fingerFruit",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } }, //mp too difficult
                         new Input(){ Entity = "item:crystalBerries",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 2 } },
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:vinegar", Amount = new OutputAmount(){ NoOfItems = 2 },  ToolContainerTagsToPlaceIn = new[]{"liquidContainerNoHeat"}} //
                        
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForLongStandaloneTask }, //mp  note that the vinegar tool speeds up this process a LOT.
                    ProcessToolSetKey = "toolSetMakeVinegar",
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region makeCrystalWine
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = "Producing", //
                    KeyName = "makeCrystalWine",
                    JobTypeKey = "cookingJobType",
                   // Tag = "cooking", // not food...
                    RequiredSkill = "cooking", //
                    PhysicalWorkFactor = sittingLightWork,
                    WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart, //ferments on its own
                    Inputs = new[]
                    {
                        
                         new Input(){ Entity = "item:crystalBerries",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 2 } },
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:crystalWine", Amount = new OutputAmount(){ NoOfItems = 2 },  ToolContainerTagsToPlaceIn = new[]{"liquidContainerNoHeat"}} //
                        
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForLongStandaloneTask }, //
                    ProcessToolSetKey = "toolSetFermentWithKitchenAndJar",
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region makeCrystalBrandy
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = "Producing", //
                    KeyName = "makeCrystalBrandy",                   
                    RequiredSkill = "cooking", //
                    JobTypeKey = "cookingJobType",
                    PhysicalWorkFactor = sittingLightWork,
                    WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart, //finishes distilling on its own. could also make it quicker, with worker present the whole time?
                    Inputs = new[]
                    {
                                            
                         new Input(){ Entity = "item:crystalWine",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 4 } }, //http://www.wikihow.com/Make-Rum  20 L mash yields 3 l alcohol
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:crystalBrandy", Amount = new OutputAmount(){ NoOfItems = 2 },  ToolContainerTagsToPlaceIn = new[]{"liquidContainerNoHeat"}} //
                        
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForShortStandaloneTask }, //
                    ProcessToolSetKey = "toolSetMakeBrandy",
                  //  AgentAnimationStates = new[] { AnimModifier.Improvised } ??
                });
                #endregion


                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = cookingName,
                    KeyName = "makeClamwichSoup",
                    JobTypeKey = "cookingJobType",
                    RequiredSkill = "cooking",
                    PhysicalWorkFactor = sittingLightWork,
                    Stances = kneelingProduction,
                    Inputs = new[] { new Input() { Entity = "item:clamwich", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1 } } },

                    Outputs = new Output[] {
                        new Output(){ EntityTypeToCreate = "item:clamwichSoup", Amount = new OutputAmount(){ NoOfItems = 2 },  ToolContainerTagsToPlaceIn = new[]{"cookingPot"}} 
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    ProcessToolSetKey = "toolSetMakeStew",

                });

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = cookingName,
                    KeyName = "makeBakedTorux",
                    JobTypeKey = "cookingJobType",
                    RequiredSkill = "cooking",
                    PhysicalWorkFactor = sittingLightWork,
                    Stances = kneelingProduction,
                    Inputs = new[] { new Input() { Entity = "item:torux", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1 } } },

                    Outputs = new Output[] {
                        new Output(){ EntityTypeToCreate = "item:bakedTorux", Amount = new OutputAmount(){ NoOfItems = 2 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    ProcessToolSetKey = "toolSetFireplace",

                });

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = cookingName,
                    KeyName = "makeGrubGrub",
                    JobTypeKey = "cookingJobType",
                    RequiredSkill = "cooking",
                    PhysicalWorkFactor = sittingLightWork,
                    Stances = kneelingProduction,
                    Inputs = new[] { new Input() { Entity = "item:scampGrub", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1 } } },

                    Outputs = new Output[] {
                        new Output(){ EntityTypeToCreate = "item:grubGrub", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    ProcessToolSetKey = "toolSetFireplace",

                });

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = cookingName,
                    KeyName = "makeBakedCommonOilTubers",
                    JobTypeKey = "cookingJobType",
                    RequiredSkill = "cooking",
                    PhysicalWorkFactor = sittingLightWork,
                    Stances = kneelingProduction,
                    Inputs = new[] { new Input() { Entity = "item:commonOilTubers", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1 } } },

                    Outputs = new Output[] {
                        new Output(){ EntityTypeToCreate = "item:bakedCommonOilTubers", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    ProcessToolSetKey = "toolSetFireplace",

                });


                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = cookingName,
                    KeyName = "makeMashedCommonOilTubers",
                    JobTypeKey = "cookingJobType",
                    RequiredSkill = "cooking",
                    PhysicalWorkFactor = sittingLightWork,
                    Stances = kneelingProduction,
                    Inputs = new[] { new Input() { Entity = "item:commonOilTubers", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1 } } },

                    Outputs = new Output[] {
                        new Output(){ EntityTypeToCreate = "item:mashedCommonOilTubers", Amount = new OutputAmount(){ NoOfItems = 1 },  ToolContainerTagsToPlaceIn = new[]{"cookingPot"}} 
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForTinyCraftingTask },
                    ProcessToolSetKey = "toolSetMakeStew",

                });


                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = cookingName,
                    KeyName = "makeGlassyPorridge",
                    JobTypeKey = "cookingJobType",
                    RequiredSkill = "cooking",
                    PhysicalWorkFactor = sittingLightWork,
                    Stances = kneelingProduction,
                    Inputs = new[] { new Input() { Entity = "item:glassyCreeperPods", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1 } } },

                    Outputs = new Output[] {
                        new Output(){ EntityTypeToCreate = "item:glassyPorridge", Amount = new OutputAmount(){ NoOfItems = 2 },  ToolContainerTagsToPlaceIn = new[]{"cookingPot"}} 
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    ProcessToolSetKey = "toolSetMakeStew",

                });


                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = cookingName,
                    KeyName = "makePowderedCrystalBerries",
                    JobTypeKey = "cookingJobType",
                    RequiredSkill = "cooking",
                    PhysicalWorkFactor = sittingLightWork,
                    Stances = kneelingProduction,
                    Inputs = new[] { new Input() { Entity = "item:crystalBerries", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1 } } },

                    Outputs = new Output[] {
                        new Output(){ EntityTypeToCreate = "item:powderedCrystalBerries", Amount = new OutputAmount(){ NoOfItems = 1 },  ToolContainerTagsToPlaceIn = new[]{"cookingPot"}} 
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForTinyCraftingTask },
                    ProcessToolSetKey = "toolSetMakeBlendedFood",

                });

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = cookingName,
                    KeyName = "makeSimCoffee",
                   // Tag = "cooking", not food
                    JobTypeKey = "cookingJobType",
                    RequiredSkill = "cooking",
                    PhysicalWorkFactor = sittingLightWork,
                    Stances = kneelingProduction,
                    Inputs = new[] { new Input() { Entity = "item:simCoffeeBeans", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1 } } }, 

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:simCoffee", Amount = new OutputAmount(){ NoOfItems = 4 },  ToolContainerTagsToPlaceIn = new[]{"cookingPot"}} 
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    ProcessToolSetKey = "toolSetMakeStew", // MP feb 2015: this is actually quite difficult.  I made a tooltag called makeCoffee, I also need a kettle.. 
                });

                /*
                    listOfProcessTypes.Add(new ProcessType()
                    {
                        Name = "Drying",
                        KeyName = "dryThunderChickenMeat",
                        RequiredSkill = "bushcraft",
                        PhysicalWorkFactor = sittingLightWork,
                        Inputs = new[] { new Input() { Entity = "item:thunderChickenMeat", IsConsumed = true, Amount = new Amount() { NoOfItems = 1 } } },

                        Outputs = new Output[] {
                            new Output(){ EntityType = "item:driedThunderChickenMeat", Amount = new Amount(){ NoOfItems = 1 }}
                        },
                        WorkOrTimeNeeded = new WorkOrTime() { DaysOfWorkNeeded = 0.009f },

                    });

    */


                #region Field Lab make enzymes and other chemicals

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = synthesizingName,
                    KeyName = "makeHexapineLeavesEnzyme",
                    JobTypeKey = "cookingJobType",
                    RequiredSkill = "biology",
                    PhysicalWorkFactor = sittingLightWork,
                    Stances = sittingProduction,
                    Inputs = new[] { new Input() { Entity = "item:hexapineLeaves", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1 } } },
                    Outputs = new[] { new Output() { EntityTypeToCreate = "item:hexapineLeavesEnzyme", Amount = new OutputAmount() { NoOfItems = 4 } },
                                  new Output(){ EntityTypeToCreate = "item:hexapineLeaves", IsWasteProduct = true, Amount = new OutputAmount(){ NoOfItems = 1 }}                     
                    }, //they only need a small sample, so I give the item back to the player after use.

                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForTinyCraftingTask },
                    ProcessToolSetKey = "toolSetMakeEnzyme",

                });

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = synthesizingName,
                    KeyName = "makeBlackpulpEnzyme",
                    JobTypeKey = "cookingJobType",
                    RequiredSkill = "biology",
                    PhysicalWorkFactor = sittingLightWork,
                    Stances = sittingProduction,
                    Inputs = new[] { new Input() { Entity = "item:blackpulp", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1 } } },
                    Outputs = new[] { new Output() { EntityTypeToCreate = "item:blackpulpEnzyme", Amount = new OutputAmount() { NoOfItems = 4 } },
                                        new Output(){ EntityTypeToCreate = "item:blackpulp", IsWasteProduct = true, Amount = new OutputAmount(){ NoOfItems = 1 }} 
                    }, //they only need a small sample, so I give the item back to the player after use.

                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForTinyCraftingTask },
                    ProcessToolSetKey = "toolSetMakeEnzyme",

                });

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = synthesizingName,
                    KeyName = "makeSpottedOilTuberEnzyme",
                    JobTypeKey = "cookingJobType",
                    RequiredSkill = "biology",
                    PhysicalWorkFactor = sittingLightWork,
                    Stances = sittingProduction,
                    Inputs = new[] { new Input() { Entity = "item:spottedOilTubers", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1 } } },

                    Outputs = new[] { new Output() { EntityTypeToCreate = "item:spottedOilTuberEnzyme", Amount = new OutputAmount() { NoOfItems = 4 } },
                                     new Output(){ EntityTypeToCreate = "item:spottedOilTubers", IsWasteProduct = true, Amount = new OutputAmount(){ NoOfItems = 1 }}                     
                   }, //they only need a small sample, so I want to give the item back to the player after use just like when making campfire

                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForTinyCraftingTask },
                    ProcessToolSetKey = "toolSetMakeEnzyme",

                });


                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = cookingName,
                    KeyName = "makeMashedSpottedOilTubers",
                    JobTypeKey = "cookingJobType",
                    RequiredSkill = "cooking",
                    PhysicalWorkFactor = sittingLightWork,
                    Stances = kneelingProduction,
                    Inputs = new[] { 
                                     new Input() { Entity = "item:spottedOilTubers", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 2 } }, 
                                     new Input() { Entity = "item:spottedOilTuberEnzyme", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1 } }
                                   },
                    Outputs = new[] { //mp feb 2015, I have started using FractionOfInputBulk on this cooking process. but the whole food crafting would need an overhaul too at some point.
                                        new Output(){ EntityTypeToCreate = "item:mashedSpottedOilTubers",  Amount = new OutputAmount(){ NoOfItems = 1 }}, 
                                    },

                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForTinyCraftingTask },
                    ProcessToolSetKey = "toolSetMakeStew",

                });

/* mp feb 2015: was:
                   Outputs = new Output[] {
                        new Output(){ EntityType = "item:mashedSpottedOilTubers", Amount = new OutputAmount(){ NoOfItems = 2 }}
                    }, 
 
*/

                /*
                                   Outputs = new[] {
                                        new Output(){ EntityType = "item:mashedSpottedOilTubers",  Amount = new OutputAmount(){  Bulk = new Bulk(){ FractionOfInputBulk = 0.9f} }}, 
      
                                    },
                 */



                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = "Make twinkler pheromone",
                    KeyName = "makeTwinklerPheromone",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "biology",
                    PhysicalWorkFactor = sittingLightWork,
                    Stances = sittingProduction,
                    Inputs = new[] { new Input() { Entity = "item:twinklerMeat", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1 } } },

                    Outputs = new[] { new Output() { EntityTypeToCreate = "item:twinklerPheromone", Amount = new OutputAmount() { NoOfItems = 2 } },
                     
                   }, 

                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForTinyCraftingTask },
                    ProcessToolSetKey = "toolSetMakeEnzyme",

                });



                #endregion
            



                #endregion


                #region ///Salvage Structures///



                float salvageHole = moderateHardWork;

                SerializableDictionary<string, ChanceToTakeStance[]> constructionStances = kneelingOrStandingProduction;

                //  SerializableDictionary<string, ChanceToTakeStance[]> salvageStructureStances = kneelingOrStandingProduction; // new[] { LeggedLocomotor.Stance.Standing, LeggedLocomotor.Stance.Kneeling };

                #region salvagePeatStack
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = salvagingName, //do not use 'clear away' because it may contain items, thereby causing confusing.
                    SummaryDescription = salvageWithLossSummary,
                    KeyName = "salvagePeatStack",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = salvageStructure,
                    IsSalvageProcess = true,
                    Stances = GetSalvageStructureStances(),
                    Inputs = new[] 
                    {
                            new Input(){ Entity = "structure:peatStack", Amount = new InputAmount(){  NoOfItems = 1 } }
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForInstantCraftingTask },
                    AgentActionState = AnimAction.Salvaging,
                    AgentAnimationStates = null,
                });
                #endregion

                #region salvageFirewoodStack
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = salvagingName, //
                    SummaryDescription = salvageWithLossSummary,
                    KeyName = "salvageFirewoodStack",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = salvageStructure,
                    IsSalvageProcess = true,
                    Stances = GetSalvageStructureStances(),
                    Inputs = new[] 
                    {
                            new Input(){ Entity = "structure:firewoodStack", Amount = new InputAmount(){  NoOfItems = 1 } }
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForInstantCraftingTask },
                    AgentActionState = AnimAction.Salvaging,
                    AgentAnimationStates = null,
                });
                #endregion

                #region salvageCompostPit
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = salvagingName, 
                    SummaryDescription = salvageWithLossSummary,
                    KeyName = "salvageCompostPit",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = salvageStructure,
                    Stances = kneelingOrStandingProduction,
                    IsSalvageProcess = true,
                    Inputs = new[] 
                    {
                            new Input(){ Entity = "structure:compostPit", Amount = new InputAmount(){  NoOfItems = 1 } }
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:stones", Amount = new OutputAmount(){ NoOfItems = 1 }},
                    },
  
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    AgentActionState = AnimAction.Digging,
                    //      ProcessToolSetKey = "toolSetDiggingSoil", //mp removed tool demands until we get production tooltip feedback  for the player
                });
                #endregion

                #region salvageCompostBin
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = salvagingName,
                    SummaryDescription = salvageWithLossSummary,
                    KeyName = "salvageCompostBin",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = salvageStructure,
                    IsSalvageProcess = true,
                    Stances = GetSalvageStructureStances(),
                    Inputs = new[] 
                    {
                            new Input(){ Entity = "structure:compostBin", Amount = new InputAmount(){  NoOfItems = 1 } }
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:sticks", Amount = new OutputAmount(){ NoOfItems = 1 }},
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForTinyCraftingTask },
                    AgentActionState = AnimAction.Salvaging,
                    AgentAnimationStates = null,
                });
                #endregion

                #region salvageMeatDryingRack
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = salvagingName,
                    SummaryDescription = salvageWithLossSummary,
                    KeyName = "salvageMeatDryingRack",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = salvageStructure,
                    IsSalvageProcess = true,
                    Stances = GetSalvageStructureStances(),
                    Inputs = new[] 
                    {
                        new Input(){ Entity = "structure:meatDryingRack", Amount = new InputAmount(){  NoOfItems = 1 } }
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:sticks", Amount = new OutputAmount(){ NoOfItems = 1 }},
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    AgentActionState = AnimAction.Salvaging,
                    AgentAnimationStates = null,
                });
                #endregion

                #region salvageHideRack
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = salvagingName,
                    SummaryDescription = salvageWithLossSummary,
                    KeyName = "salvageHideRack",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = salvageStructure,
                    IsSalvageProcess = true,
                    Stances = GetSalvageStructureStances(),
                    Inputs = new[] 
                    {
                        new Input(){ Entity = "structure:hideRack", Amount = new InputAmount(){  NoOfItems = 1 } }
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:sticks", Amount = new OutputAmount(){ NoOfItems = 1 }},
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    AgentActionState = AnimAction.Salvaging,
                    AgentAnimationStates = null,
                });
                #endregion
                #region salvageRareMetalRefinery
                //NA MINING CAMP MATERIAL
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = packingDownName,
                    SummaryDescription = packingDownWithoutLossSummary,
                    KeyName = "salvageRareMetalRefinery",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = salvageStructure,
                    IsSalvageProcess = true,
                    Stances = GetSalvageStructureStances(),
                    Inputs = new[] 
                    {
                        new Input(){ Entity = "structure:rareMetalRefinery", Amount = new InputAmount(){  NoOfItems = 1 } }
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:metalRefineryEquipment", Amount = new OutputAmount(){ NoOfItems = 1 }},
                        new Output(){ EntityTypeToCreate = "item:metalRefineryPart1", Amount = new OutputAmount(){ NoOfItems = 1 }},
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    AgentActionState = AnimAction.Salvaging,
                    AgentAnimationStates = null,
                });
                #endregion
                #region salvageImprovisedGreenhouse
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = salvagingName,
                    SummaryDescription = salvageWithLossSummary,
                    KeyName = "salvageImprovisedGreenhouse",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = salvageStructure,
                    IsSalvageProcess = true,
                    Stances = GetSalvageStructureStances(),
                    Inputs = new[] 
                    {
                            new Input(){ Entity = "structure:improvisedGreenhouse", Amount = new InputAmount(){  NoOfItems = 1 } }
                    },
                    Outputs = new[]
                    {
                        new Output() { EntityTypeToCreate = "item:shadeleafCanes", Amount = new OutputAmount(){  NoOfItems = 2 } },
                        new Output(){ EntityTypeToCreate = "item:improvisedGreenHouseCover", Amount = new OutputAmount(){ NoOfItems = 1 }}

                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    AgentActionState = AnimAction.Salvaging,
                    AgentAnimationStates = null,
                });
                #endregion

                #region salvageSimpleSmithy
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = salvagingName,
                    SummaryDescription = salvageWithLossSummary,
                    KeyName = "salvageSimpleSmithy",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = salvageStructure,
                    IsSalvageProcess = true,
                    Stances = GetSalvageStructureStances(),
                    Inputs = new[] 
                    {
                            new Input(){ Entity = "structure:simpleSmithy", Amount = new InputAmount(){  NoOfItems = 1 } }
                    },
                    Outputs = new[]
                    {
                        new Output() { EntityTypeToCreate = "item:solidMudBrick", Amount = new OutputAmount(){  NoOfItems = 1 } },
                        new Output(){ EntityTypeToCreate = "item:anvil", Amount = new OutputAmount(){ NoOfItems = 1 }},
                        new Output(){ EntityTypeToCreate = "item:barClamps", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    AgentActionState = AnimAction.Salvaging,
                    AgentAnimationStates = null,
                });
                #endregion
                #region salvageSpikeTrap
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = salvagingName,
                    SummaryDescription = salvageWithLossSummary,
                    KeyName = "salvageSpikeTrap",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = salvageStructure,
                    IsSalvageProcess = true,
                    Stances = GetSalvageStructureStances(),
                    Inputs = new[] 
                    {
                            new Input(){ Entity = "structure:spikeTrap", Amount = new InputAmount(){  NoOfItems = 1 } }
                    },
                    Outputs = new[] 
                    {
                            new Output() { EntityTypeToCreate = "item:spikeTrap", Amount = new OutputAmount(){  NoOfItems = 1 } },
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForTinyCraftingTask },
                    AgentActionState = AnimAction.Salvaging,
                    AgentAnimationStates = null,
                });
                #endregion
                #region salvageDryingShed
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = salvagingName,
                    SummaryDescription = salvageWithLossSummary,
                    KeyName = "salvageDryingShed",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = salvageStructure,
                    IsSalvageProcess = true,
                    Stances = GetSalvageStructureStances(),
                    Inputs = new[] {
                            new Input(){ Entity = "structure:dryingShed", Amount = new InputAmount(){  NoOfItems = 1 } }
                        },

                    Outputs = new[] 
                    {
                            new Output() { EntityTypeToCreate = "item:solidMudBrick", Amount = new OutputAmount(){  NoOfItems = 1 } },
                            new Output() { EntityTypeToCreate = "item:shadeleafCanes", Amount = new OutputAmount(){  NoOfItems = 1 } },
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    AgentActionState = AnimAction.Salvaging,
                    AgentAnimationStates = null,
                });
                #endregion
                #region salvageToolshed
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = salvagingName,
                    SummaryDescription = salvageWithLossSummary,
                    KeyName = "salvageToolshed",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = salvageStructure,
                    IsSalvageProcess = true,
                    Stances = GetSalvageStructureStances(),
                    Inputs = new[] {
                            new Input(){ Entity = "structure:toolshed", Amount = new InputAmount(){  NoOfItems = 1 } }
                        },

                    Outputs = new[] 
                    {
                            new Output() { EntityTypeToCreate = "item:solidMudBrick", Amount = new OutputAmount(){  NoOfItems = 1 } },
                            new Output() { EntityTypeToCreate = "item:waterCaneStem", Amount = new OutputAmount(){  NoOfItems = 1 } },
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    AgentActionState = AnimAction.Salvaging,
                    AgentAnimationStates = null,
                });
                #endregion
                #region salvageClayGranary
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = salvagingName,
                    SummaryDescription = salvageWithLossSummary,
                    KeyName = "salvageClayGranary",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = salvageStructure,
                    IsSalvageProcess = true,
                    Stances = GetSalvageStructureStances(),
                    Inputs = new[] {
                            new Input(){ Entity = "structure:clayGranary", Amount = new InputAmount(){  NoOfItems = 1 } }
                        },

                    Outputs = new[] 
                    {
                            new Output() { EntityTypeToCreate = "item:solidMudBrick", Amount = new OutputAmount(){  NoOfItems = 2 } },
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    AgentActionState = AnimAction.Salvaging,
                    AgentAnimationStates = null,
                });
                #endregion
                #region salvageCaneHut
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = salvagingName,
                    SummaryDescription = salvageWithLossSummary,
                    KeyName = "salvageCaneHut",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = salvageStructure,
                    IsSalvageProcess = true,
                    Stances = GetSalvageStructureStances(),
                    Inputs = new[] {
                            new Input(){ Entity = "structure:caneHut", Amount = new InputAmount(){  NoOfItems = 1 } }
                        },

                    Outputs = new[] 
                    {
                            new Output() { EntityTypeToCreate = "item:waterCaneStem", Amount = new OutputAmount(){  NoOfItems = 3 } },
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    AgentActionState = AnimAction.Salvaging,
                    AgentAnimationStates = null,
                });
                #endregion
                #region salvageClayHut
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = salvagingName,
                    SummaryDescription = salvageWithLossSummary,
                    KeyName = "salvageClayHut",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = salvageStructure,
                    IsSalvageProcess = true,
                    Stances = GetSalvageStructureStances(),
                    Inputs = new[] {
                            new Input(){ Entity = "structure:clayHut", Amount = new InputAmount(){  NoOfItems = 1 } }
                        },

                    Outputs = new[] 
                    {
                            new Output() { EntityTypeToCreate = "item:solidMudBrick", Amount = new OutputAmount(){  NoOfItems = 2 } },
                            new Output() { EntityTypeToCreate = "item:spoakBranchesTrimmed", Amount = new OutputAmount(){  NoOfItems = 1 } },
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    AgentActionState = AnimAction.Salvaging,
                    AgentAnimationStates = null,
                });
                #endregion
                #region salvageTurnipHut
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = salvagingName,
                    SummaryDescription = salvageWithLossSummary,
                    KeyName = "salvageTurnipHut",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = salvageStructure,
                    IsSalvageProcess = true,
                    Stances = GetSalvageStructureStances(),
                    Inputs = new[] {
                            new Input(){ Entity = "structure:turnipHut", Amount = new InputAmount(){  NoOfItems = 1 } }
                        },

                    Outputs = new[] 
                    {
                            new Output() { EntityTypeToCreate = "item:turnipShell", Amount = new OutputAmount(){  NoOfItems = 1 } },
                            new Output() { EntityTypeToCreate = "item:sticks", Amount = new OutputAmount(){  NoOfItems = 1 } }
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    AgentActionState = AnimAction.Salvaging,
                    AgentAnimationStates = null,
                });
                #endregion
                #region salvageImprovisedSmithy
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = salvagingName,
                    SummaryDescription = salvageWithLossSummary,
                    KeyName = "salvageImprovisedSmithy",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = salvageStructure,
                    IsSalvageProcess = true,
                    Stances = GetSalvageStructureStances(),
                    Inputs = new[] 
                    {
                            new Input(){ Entity = "structure:improvisedSmithy", Amount = new InputAmount(){  NoOfItems = 1 } }
                    },

                    Outputs = new[] 
                    {
                            new Output() { EntityTypeToCreate = "item:solidMudBrick", Amount = new OutputAmount(){  NoOfItems = 1 } },
                            new Output() { EntityTypeToCreate = "item:stones", Amount = new OutputAmount(){  NoOfItems = 1 } }
                    },

                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    AgentActionState = AnimAction.Salvaging,
                    AgentAnimationStates = null,
                });
                #endregion
                #region salvageKiln
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = salvagingName,
                    SummaryDescription = salvageWithLossSummary,
                    KeyName = "salvageKiln",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = salvageStructure,
                    IsSalvageProcess = true,
                    Stances = GetSalvageStructureStances(),
                    Inputs = new[] {
                            new Input(){ Entity = "structure:kiln", Amount = new InputAmount(){  NoOfItems = 1 } }
                        },
                    Outputs = new[] 
                    {
                            new Output() { EntityTypeToCreate = "item:stones", Amount = new OutputAmount(){  NoOfItems = 1 } }
                    },


                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    AgentActionState = AnimAction.Salvaging,
                    AgentAnimationStates = null,
                });
                #endregion
                #region salvageKilnImprovisedSmall
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = salvagingName,
                    SummaryDescription = salvageWithLossSummary,
                    KeyName = "salvageKilnImprovisedSmall",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = salvageStructure,
                    IsSalvageProcess = true,
                    Stances = GetSalvageStructureStances(),
                    Inputs = new[] {
                            new Input(){ Entity = "structure:kilnImprovisedSmall", Amount = new InputAmount(){  NoOfItems = 1 } }
                        },
                    Outputs = new[] {new Output() { EntityTypeToCreate = "item:stones",  Amount = new OutputAmount() { NoOfItems = 1 } } 
                                
                    },


                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    AgentActionState = AnimAction.Salvaging,
                    AgentAnimationStates = null,
                });
                #endregion
                #region salvageGoldFurnace
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = salvagingName,
                    SummaryDescription = salvageWithLossSummary,
                    KeyName = "salvageGoldFurnace",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = salvageStructure,
                    IsSalvageProcess = true,
                    Stances = GetSalvageStructureStances(),
                    Inputs = new[] {
                            new Input(){ Entity = "structure:goldFurnace", Amount = new InputAmount(){  NoOfItems = 1 } }
                        },
                    Outputs = new[] {new Output() { EntityTypeToCreate = "item:firebricks",  Amount = new OutputAmount() { NoOfItems = 2 } } 
                                
                    },


                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    AgentActionState = AnimAction.Salvaging,
                    AgentAnimationStates = null,
                });
                #endregion

                #region //Salvage UPGRADES
                #region salvagePolymerWorkshopUpgrade
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = removingName,
                    //     SummaryDescription =  //this is not shown anywhere for a salvage upgrade process.
                    KeyName = "salvagePolymerWorkshopUpgrade",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = salvageStructure,
                    IsSalvageProcess = true,
                    Stances = GetSalvageStructureStances(),
                   
                    Inputs = new[] {
                            new Input(){ Entity = "item:polymerWorkshopUpgrade", Amount = new InputAmount(){  NoOfItems = 1 } }
                        },
                    Outputs = new[] {                      
                        new Output() { EntityTypeToCreate = "item:extrusionMachineComponents", Amount = new OutputAmount(){  NoOfItems = 1 } },
                        new Output() { EntityTypeToCreate = "item:solidMudBrick", Amount = new OutputAmount(){  NoOfItems = 3 } },
                       // new Output(){ EntityTypeToCreate = "item:wroughtIron", Amount = new OutputAmount(){ NoOfItems = 3 }}                                
                    },

                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    AgentActionState = AnimAction.Salvaging,
                    AgentAnimationStates = null,
                });
                #endregion

                #region salvageCarpenterWorkshopUpgrade
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = removingName,
                    KeyName = "salvageCarpenterWorkshopUpgrade",
                    //     SummaryDescription =  //this is not shown anywhere for a salvage upgrade process.
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = salvageStructure,
                    IsSalvageProcess = true,
                    Stances = GetSalvageStructureStances(),

                    Inputs = new[] {
                            new Input(){ Entity = "item:carpenterWorkshopUpgrade", Amount = new InputAmount(){  NoOfItems = 1 } }
                        },
                    Outputs = new[] {    
                        new Output() { EntityTypeToCreate = "item:solidMudBrick", Amount = new OutputAmount(){  NoOfItems = 1 } }, 
                        new Output() { EntityTypeToCreate = "item:barClamps", Amount = new OutputAmount(){  NoOfItems = 1 } },  
                        new Output() { EntityTypeToCreate = "item:sticks", Amount = new OutputAmount(){  NoOfItems = 1 } },                            
                    },

                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    AgentActionState = AnimAction.Salvaging,
                    AgentAnimationStates = null,
                });
                #endregion

                #region salvageTextileWorkshopUpgrade
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = removingName,
                    //     SummaryDescription =  //this is not shown anywhere for a salvage upgrade process.
                    KeyName = "salvageTextileWorkshopUpgrade",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = salvageStructure,
                    IsSalvageProcess = true,
                    Stances = GetSalvageStructureStances(),

                    Inputs = new[] {
                            new Input(){ Entity = "item:textileWorkshopUpgrade", Amount = new InputAmount(){  NoOfItems = 1 } }
                        },
                    Outputs = new[] {                      
                        new Output() { EntityTypeToCreate = "item:loomComponents", Amount = new OutputAmount(){  NoOfItems = 1 } },
                        new Output() { EntityTypeToCreate = "item:waterCaneStem", Amount = new OutputAmount(){  NoOfItems = 4 } }                     
                                              
                    },

                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    AgentActionState = AnimAction.Salvaging,
                    AgentAnimationStates = null,
                });
                #endregion

                #region salvageMetalLatheShopHumanPoweredUpgrade
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = removingName,
                    //     SummaryDescription =  //this is not shown anywhere for a salvage upgrade process.
                    KeyName = "salvageMetalLatheShopHumanPoweredUpgrade",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = salvageStructure,
                    IsSalvageProcess = true,
                    Stances = GetSalvageStructureStances(),

                    Inputs = new[] {
                            new Input(){ Entity = "item:metalLatheShopHumanPoweredUpgrade", Amount = new InputAmount(){  NoOfItems = 1 } }
                        },
                    Outputs = new[] {                      
                        new Output() { EntityTypeToCreate = "item:metalLatheComponents", Amount = new OutputAmount(){  NoOfItems = 1 } },
                        new Output() { EntityTypeToCreate = "item:humanPowerUnit", Amount = new OutputAmount(){  NoOfItems = 1 } }                     
                                              
                    },

                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    AgentActionState = AnimAction.Salvaging,
                    AgentAnimationStates = null,
                });
                #endregion
               

                #region salvageWorkshopBuilding
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = removingName,
                    //     SummaryDescription =  //this is not shown anywhere for a salvage upgrade process.
                    KeyName = "salvageWorkshopBuilding",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = salvageStructure,
                    IsSalvageProcess = true,
                    Stances = GetSalvageStructureStances(),
                    Inputs = new[] {
                            new Input(){ Entity = "structure:workshopBuilding", Amount = new InputAmount(){  NoOfItems = 1 } }
                        },
                    Outputs = new[] {                      
                        new Output() { EntityTypeToCreate = "item:solidMudBrick", Amount = new OutputAmount(){  NoOfItems = 2 } },
                        new Output(){ EntityTypeToCreate = "item:waterCaneStem", Amount = new OutputAmount(){ NoOfItems = 2 }},
                                
                    },

                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    AgentActionState = AnimAction.Salvaging,
                    AgentAnimationStates = null,
                });

                #endregion

                #region salvageCookhouse
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = removingName,
                    //     SummaryDescription =  //this is not shown anywhere for a salvage upgrade process.
                    KeyName = "salvageCookhouse",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = salvageStructure,
                    IsSalvageProcess = true,
                    Stances = GetSalvageStructureStances(),
                    Inputs = new[] {
                            new Input(){ Entity = "structure:cookhouse", Amount = new InputAmount(){  NoOfItems = 1 } }
                        },
                    Outputs = new[] {                      
                        new Output() { EntityTypeToCreate = "item:solidMudBrick", Amount = new OutputAmount(){  NoOfItems = 2 } },
                        new Output(){ EntityTypeToCreate = "item:waterCaneStem", Amount = new OutputAmount(){ NoOfItems = 2 }},
                        new Output(){ EntityTypeToCreate = "item:textile", Amount = new OutputAmount(){ NoOfItems = 2 }},
                        new Output(){ EntityTypeToCreate = "item:spoakShingles", Amount = new OutputAmount(){ NoOfItems = 1 }},
                                
                    },

                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    AgentActionState = AnimAction.Salvaging,
                    AgentAnimationStates = null,
                });

                #endregion

                #endregion

                #region salvageStill
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = salvagingName,
                    SummaryDescription = salvageWithLossSummary,
                    KeyName = "salvageStill",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = salvageStructure,
                    IsSalvageProcess = true,
                    Stances = GetSalvageStructureStances(),
                    Inputs = new[] {
                            new Input(){ Entity = "structure:still", Amount = new InputAmount(){  NoOfItems = 1 } }
                        },
                    Outputs = new[] {
                        new Output() { EntityTypeToCreate = "item:stillComponents",  Amount = new OutputAmount() { NoOfItems = 1 } } ,
                        new Output() { EntityTypeToCreate = "item:stones", Amount = new OutputAmount(){  NoOfItems = 1 } },
                        new Output(){ EntityTypeToCreate = "item:clayJar", Amount = new OutputAmount(){ NoOfItems = 1 }},
                                
                    },

                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    AgentActionState = AnimAction.Salvaging,
                    AgentAnimationStates = null,
                });
                #endregion

                #region salvageRadioHutImprovised
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = salvagingName,
                    SummaryDescription = salvageWithLossSummary,
                    KeyName = "salvageRadioHutImprovised",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = salvageStructure,
                    IsSalvageProcess = true,
                    Stances = GetSalvageStructureStances(),
                    Inputs = new[] {
                            new Input(){ Entity = "structure:radioHutImprovised", Amount = new InputAmount(){  NoOfItems = 1 } }
                        },
                    Outputs = new[] {
                        new Output() { EntityTypeToCreate = "item:radio",  Amount = new OutputAmount() { NoOfItems = 1 } } ,
                        new Output() { EntityTypeToCreate = "item:radioAntenna", Amount = new OutputAmount(){  NoOfItems = 1 } },
                        new Output(){ EntityTypeToCreate = "item:shadeleafCanes", Amount = new OutputAmount(){ NoOfItems = 1 }},
                        new Output(){ EntityTypeToCreate = "item:spoakLeaves", Amount = new OutputAmount(){ NoOfItems = 1 }}
                                
                    },


                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    AgentActionState = AnimAction.Salvaging,
                    AgentAnimationStates = null,
                });
                #endregion

                #region salvageRadioHut
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = salvagingName,
                    SummaryDescription = salvageWithLossSummary, //?
                    KeyName = "salvageRadioHut",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = salvageStructure,
                    IsSalvageProcess = true,
                    Stances = GetSalvageStructureStances(),
                    Inputs = new[] {
                            new Input(){ Entity = "structure:radioHut", Amount = new InputAmount(){  NoOfItems = 1 } }
                        },
                    Outputs = new[] {
                        new Output() { EntityTypeToCreate = "item:radio",  Amount = new OutputAmount() { NoOfItems = 1 } } ,
                        new Output() { EntityTypeToCreate = "item:radioAntenna", Amount = new OutputAmount(){  NoOfItems = 1 } },
                        new Output(){ EntityTypeToCreate = "item:shadeleafCanes", Amount = new OutputAmount(){ NoOfItems = 1 }},
                        new Output(){ EntityTypeToCreate = "item:spoakShingles", Amount = new OutputAmount(){ NoOfItems = 1 }}
                                
                    },


                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    AgentActionState = AnimAction.Salvaging,
                    AgentAnimationStates = null,
                });
                #endregion

 
                #region salvageDeadfallTrap
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = disassembleName,
                    SummaryDescription = disassembleName,
                    KeyName = "salvageDeadfallTrap",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = salvageStructure,
                    IsSalvageProcess = true,
                    Stances = GetSalvageStructureStances(),
                    Inputs = new[] {
                            new Input(){ Entity = "structure:deadfallTrap", Amount = new InputAmount(){  NoOfItems = 1 } }
                        },

                    Outputs = new[] {
                            new Output() { EntityTypeToCreate = "item:stones", Amount = new OutputAmount(){  NoOfItems = 1 } } 
                        },

                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    AgentActionState = AnimAction.Salvaging,
                    AgentAnimationStates = null,
                });
                #endregion
                #region salvageSpringSnare
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = disassembleName,
                    SummaryDescription = disassembleName,
                    KeyName = "salvageSpringSnare",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = salvageStructure,
                    IsSalvageProcess = true,
                    Stances = GetSalvageStructureStances(),
                    Inputs = new[] {
                            new Input(){ Entity = "structure:springSnare", Amount = new InputAmount(){  NoOfItems = 1 } }
                        },

                    Outputs = new[] {
                            new Output() { EntityTypeToCreate = "item:shadeleafCanes", Amount = new OutputAmount(){  NoOfItems = 1 } } 
                        },

                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    AgentActionState = AnimAction.Salvaging,
                    AgentAnimationStates = null,
                });
                #endregion
                #region salvageLandMine
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = disassembleName,
                    SummaryDescription = disassembleName,
                    KeyName = "salvageLandMine",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = salvageStructure,
                    IsSalvageProcess = true,
                    Stances = GetSalvageStructureStances(),
                    Inputs = new[] {
                            new Input(){ Entity = "structure:landMine", Amount = new InputAmount(){  NoOfItems = 1 } }
                        },

                    Outputs = new[] {
                            new Output() { EntityTypeToCreate = "item:landMine", Amount = new OutputAmount(){  NoOfItems = 1 } } 
                        },

                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    //mp I want anim to look like mend:
                    //        AgentActionState = AnimAction.Salvaging,
                    //       AgentAnimationStates = null,

                });
                #endregion
                #region salvageSensor
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = packingDownName,
                    SummaryDescription = packingDownWithoutLossSummary,
                    KeyName = "salvageSensor",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = sittingLightWork,
                    IsSalvageProcess = true,
                    Stances = kneelingProduction,
                    Inputs = new[] {
                        new Input(){ Entity = "structure:sensor", Amount = new InputAmount(){  NoOfItems = 1 } }},


                    Outputs = new[] {new Output() { EntityTypeToCreate = "item:sensor",  Amount = new OutputAmount() { NoOfItems = 1 } } //MP: see the sensor structure for the way to set up a salvage process of this kind.
                                
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForInstantCraftingTask },
                    //mp I want anim to look like mend:
            //        AgentActionState = AnimAction.Salvaging,
             //       AgentAnimationStates = null,

                });
                #endregion
                #region salvageSentry
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = packingDownName,
                    SummaryDescription = packingDownWithoutLossSummary,
                    KeyName = "salvageSentry", //taking down the structure
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = sittingLightWork,
                    IsSalvageProcess = true,
                    Stances = kneelingProduction,
                    Inputs = new[] {
                        new Input(){ Entity = "structure:sentry", Amount = new InputAmount(){  NoOfItems = 1 } }},


                    Outputs = new[] {new Output() { EntityTypeToCreate = "item:sentry",  Amount = new OutputAmount() { NoOfItems = 1 } } 
                                
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForInstantCraftingTask },                  

                });
                #endregion
                #region SprayGunSentry
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = packingDownName,
                    SummaryDescription = packingDownWithoutLossSummary,
                    KeyName = "salvageSprayGunSentry", //taking down the structure
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = sittingLightWork,
                    IsSalvageProcess = true,
                    Stances = kneelingProduction,
                    Inputs = new[] {
                        new Input(){ Entity = "structure:sprayGunSentry", Amount = new InputAmount(){  NoOfItems = 1 } }},


                    Outputs = new[] {new Output() { EntityTypeToCreate = "item:spraySentry",  Amount = new OutputAmount() { NoOfItems = 1 } } 
                                
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForInstantCraftingTask },

                });
                #endregion



                #region salvageScarecrow
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = salvagingName,
                    SummaryDescription = salvagingName,
                    KeyName = "salvageScarecrow",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = sittingLightWork,
                    IsSalvageProcess = true,
                    Stances = kneelingProduction,
                    Inputs = new[] {
                        new Input(){ Entity = "structure:scarecrow", Amount = new InputAmount(){  NoOfItems = 1 } }},


                    Outputs = new[] {new Output() { EntityTypeToCreate = "item:twinklerPlating",  Amount = new OutputAmount() { NoOfItems = 1 } } 
                                
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForInstantCraftingTask },
                    //mp I want anim to look like mend:
                    //        AgentActionState = AnimAction.Salvaging,
                    //       AgentAnimationStates = null,

                });
                #endregion
                #region salvageSimplePort
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = salvagingName,
                    SummaryDescription = salvagingName,
                    KeyName = "salvageSimplePort",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = salvageStructure,
                    IsSalvageProcess = true,
                    Stances = GetSalvageStructureStances(),
                    Inputs = new[] {
                            new Input(){ Entity = "structure:simplePort", Amount = new InputAmount(){  NoOfItems = 1 } }
                        },

                    Outputs = new[] 
                    {
                            new Output(){ EntityTypeToCreate = "item:waterCaneStem", Amount = new OutputAmount(){ NoOfItems = 1 }},
                            new Output(){ EntityTypeToCreate = "item:spoakBranchesTrimmed", Amount = new OutputAmount(){ NoOfItems = 2 }},
                            new Output(){ EntityTypeToCreate = "item:spoakShingles", Amount = new OutputAmount(){ NoOfItems = 1 }}, //
                            new Output(){ EntityTypeToCreate = "item:solidMudBrick", Amount = new OutputAmount(){ NoOfItems = 3 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    AgentActionState = AnimAction.Salvaging,
                    AgentAnimationStates = null,
                });
                #endregion
                #region salvageCanopyPort
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = salvagingName,
                    SummaryDescription = salvagingName,
                    KeyName = "salvageCanopyPort",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = salvageStructure,
                    IsSalvageProcess = true,
                    Stances = GetSalvageStructureStances(),
                    Inputs = new[] {
                            new Input(){ Entity = "structure:canopyPort", Amount = new InputAmount(){  NoOfItems = 1 } }
                        },

                    Outputs = new[] 
                    {
                            new Output(){ EntityTypeToCreate = "item:daysheenLeaves", Amount = new OutputAmount(){ NoOfItems = 1 }},
                            new Output(){ EntityTypeToCreate = "item:waterCaneStem", Amount = new OutputAmount(){ NoOfItems = 1 }},
                            new Output(){ EntityTypeToCreate = "item:spoakBranchesTrimmed", Amount = new OutputAmount(){ NoOfItems = 2 }},

                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    AgentActionState = AnimAction.Salvaging,
                    AgentAnimationStates = null,
                });
                #endregion
                #region salvageLandingImprovised
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = salvagingName,
                    SummaryDescription = salvagingName,
                    KeyName = "salvageLandingImprovised",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = salvageStructure,
                    IsSalvageProcess = true,
                    Stances = GetSalvageStructureStances(),
                    Inputs = new[] {
                            new Input(){ Entity = "structure:landingImprovised", Amount = new InputAmount(){  NoOfItems = 1 } }
                        },

                    Outputs = new[] 
                    {
                            new Output(){ EntityTypeToCreate = "item:waterCaneStem", Amount = new OutputAmount(){ NoOfItems = 1 }} 

                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForTinyCraftingTask },
                    AgentActionState = AnimAction.Salvaging,
                    AgentAnimationStates = null,
                });
                #endregion
                #region salvageHelipadBig
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = disassembleName,
                    SummaryDescription = disassembleWithoutLossSummary,
                    KeyName = "salvageHelipadBig",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = constructionExertion,
                    IsSalvageProcess = true,
                    Stances = constructionStances,
                    Inputs = new[] {
                        new Input(){ Entity = "structure:helipadBig", Amount = new InputAmount(){  NoOfItems = 1 } }},
                    Outputs = new[] {new Output() { EntityTypeToCreate = "item:structurePanels",  Amount = new OutputAmount() { NoOfItems = 2 } } 
                                
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    AgentActionState = AnimAction.Salvaging

                });
                #endregion
                #region salvageHelipad
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = disassembleName,
                    SummaryDescription = disassembleWithoutLossSummary,
                    KeyName = "salvageHelipad",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = salvageStructure,
                    IsSalvageProcess = true,
                    Stances = GetSalvageStructureStances(),
                    Inputs = new[] {
                            new Input(){ Entity = "structure:helipad", Amount = new InputAmount(){  NoOfItems = 1 } }
                        },

                    Outputs = new[] 
                    {
                            new Output(){ EntityTypeToCreate = "item:stones", Amount = new OutputAmount(){ NoOfItems = 1 }} 

                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForTinyCraftingTask },
                    AgentActionState = AnimAction.Salvaging,
                    AgentAnimationStates = null,
                });
                #endregion
                #region salvageSatelliteGroundStation
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = packingDownName,
                    SummaryDescription = packingDownWithoutLossSummary,
                    KeyName = "salvageSatelliteGroundStation",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = sittingLightWork,
                    IsSalvageProcess = true,
                    Stances = kneelingProduction,
                    Inputs = new[] {
                        new Input(){ Entity = "structure:satelliteGroundStation", Amount = new InputAmount(){  NoOfItems = 1 } }},
                    Outputs = new[] {new Output() { EntityTypeToCreate = "item:satelliteGroundStation",  Amount = new OutputAmount() { NoOfItems = 1 } } 
                                
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForTinyCraftingTask },
                    AgentActionState = AnimAction.Salvaging

                });
                #endregion
                #region salvageWeatherStation
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = packingDownName,
                    SummaryDescription = packingDownWithoutLossSummary,
                    KeyName = "salvageWeatherStation",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = moderateWork,
                    IsSalvageProcess = true,
                    Stances = constructionStances,
                    Inputs = new[] {
                        new Input(){ Entity = "structure:weatherStation", Amount = new InputAmount(){  NoOfItems = 1 } }},


                    Outputs = new[] {new Output() { EntityTypeToCreate = "item:weatherStationMast",  Amount = new OutputAmount() { NoOfItems = 1 } },
                                     new Output() { EntityTypeToCreate = "item:weatherStationSensors",  Amount = new OutputAmount() { NoOfItems = 1 } }                       
                                
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    AgentActionState = AnimAction.Salvaging, //has kick anim..
                    AgentAnimationStates = null,

                });
                #endregion
                #region salvageMolecularAssembler
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = packingDownName,
                    SummaryDescription = packingDownWithoutLossSummary,
                    KeyName = "salvageMolecularAssembler",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = moderateWork,
                    IsSalvageProcess = true,
                    Stances = constructionStances,
                    Inputs = new[] {
                        new Input(){ Entity = "structure:molecularAssembler", Amount = new InputAmount(){  NoOfItems = 1 } }},


                    Outputs = new[] {new Output() { EntityTypeToCreate = "item:vacuumChamber",  Amount = new OutputAmount() { NoOfItems = 1 } },
                                     new Output() { EntityTypeToCreate = "item:assemblerCabinet",  Amount = new OutputAmount() { NoOfItems = 1 }},
                                     new Output() { EntityTypeToCreate = "item:assemblerCooling",  Amount = new OutputAmount() { NoOfItems = 1 }}                       
                                
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    AgentActionState = AnimAction.Salvaging,
                    AgentAnimationStates = null,

                });

                #endregion
                #region salvageSmallTent
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = packingDownName,
                    SummaryDescription = packingDownWithoutLossSummary,
                    KeyName = "salvageSmallTent",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = salvageStructure,
                    IsSalvageProcess = true,
                    Stances = constructionStances,
                    Inputs = new[] {
                        new Input(){ Entity = "structure:smallTent", Amount = new InputAmount(){  NoOfItems = 1 } }},


                    Outputs = new[] {new Output() { EntityTypeToCreate = "item:smallTent",  Amount = new OutputAmount() { NoOfItems = 1 } } 
                                
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForTinyCraftingTask }, //quicker than salvaging imrpovised structure
                    AgentActionState = AnimAction.Salvaging,
                    AgentAnimationStates = null,

                });
                #endregion
                #region salvageOctagonalTent
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = packingDownName,
                    SummaryDescription = packingDownWithoutLossSummary,
                    KeyName = "salvageOctagonalTent",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = salvageStructure,
                    IsSalvageProcess = true,
                    Stances = constructionStances,
                    Inputs = new[] {
                        new Input(){ Entity = "structure:octagonalTent", Amount = new InputAmount(){  NoOfItems = 1 } }},


                    Outputs = new[] {new Output() { EntityTypeToCreate = "item:octagonalTent",  Amount = new OutputAmount() { NoOfItems = 1 } } 
                                
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForTinyCraftingTask }, //quicker than salvaging imrpovised structure
                    AgentActionState = AnimAction.Salvaging,
                    AgentAnimationStates = null,

                });

                #endregion
                #region salvageDomeTent

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = packingDownName,
                    SummaryDescription = packingDownWithoutLossSummary,
                    KeyName = "salvageDomeTent",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = salvageStructure,
                    IsSalvageProcess = true,
                    Stances = constructionStances,
                    Inputs = new[] {
                        new Input(){ Entity = "structure:domeTent", Amount = new InputAmount(){  NoOfItems = 1 } }},


                    Outputs = new[] {new Output() { EntityTypeToCreate = "item:domeTent",  Amount = new OutputAmount() { NoOfItems = 1 } } 
                                
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForTinyCraftingTask }, //quicker than salvaging imrpovised structure
                    AgentActionState = AnimAction.Salvaging,
                    AgentAnimationStates = null,

                });

                #endregion
                #region salvageStorageHole

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = disassembleName,
                    SummaryDescription = disassembleWithoutLossSummary,
                    KeyName = "salvageStorageHole",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = salvageHole,
                    IsSalvageProcess = true,
                    Stances = GetSalvageStructureStances(),
                    Inputs = new[] {
                        new Input(){ Entity = "structure:storageHole", Amount = new InputAmount(){  NoOfItems = 1 } }},


                    Outputs = new[] {new Output() { EntityTypeToCreate = "item:spoakLeaves", Amount = new OutputAmount() { NoOfItems = 1 } },
                                new Output() { EntityTypeToCreate = "item:stones", Amount = new OutputAmount() { NoOfItems = 1 }
                        }
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    AgentActionState = AnimAction.Salvaging,
                    AgentAnimationStates = null,

                });

                #endregion
                #region salvageCooledFoodCache

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = disassembleName,
                    SummaryDescription = disassembleWithoutLossSummary,
                    KeyName = "salvageCooledFoodCache",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = salvageHole,
                    IsSalvageProcess = true,
                    Stances = GetSalvageStructureStances(),
                    Inputs = new[] {
                        new Input(){ Entity = "structure:cooledFoodCache", Amount = new InputAmount(){  NoOfItems = 1 } }},


                    Outputs = new[] {new Output() { EntityTypeToCreate = "item:spoakLeaves", Amount = new OutputAmount() { NoOfItems = 1 } },
                                new Output() { EntityTypeToCreate = "item:stones", Amount = new OutputAmount() { NoOfItems = 1 } },
                                    new Output() { EntityTypeToCreate = "item:inactivatedFoodCoolerUnit", IsWasteProduct = true, Amount = new OutputAmount() { NoOfItems = 1 }
                        }
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    AgentActionState = AnimAction.Salvaging,
                    AgentAnimationStates = null,

                });

                #endregion
                #region salvageSmokeOven

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = disassembleName,
                    SummaryDescription = disassembleWithoutLossSummary,
                    KeyName = "salvageSmokeOven",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = salvageStructure,
                    IsSalvageProcess = true,
                    Stances = GetSalvageStructureStances(),
                    Inputs = new[] {
                        new Input(){ Entity = "structure:smokeOven", Amount = new InputAmount(){  NoOfItems = 1 } }},

                    Outputs = new[] 
                    {
                                new Output() { EntityTypeToCreate = "item:sticks", Amount = new OutputAmount() { NoOfItems = 1 } },
                                new Output() { EntityTypeToCreate = "item:stones", Amount = new OutputAmount() { NoOfItems = 1 } },
                                new Output() { EntityTypeToCreate = "item:firegrassSod", Amount = new OutputAmount() { NoOfItems = 1 }//Finn adaptation to new salvaging validator
                    }
                     },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    AgentActionState = AnimAction.Salvaging,
                    AgentAnimationStates = null,

                });
                #endregion
                #region salvageCampfire

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = disassembleName,
                    SummaryDescription = disassembleWithoutLossSummary,
                    KeyName = "salvageCampfire",
                    PhysicalWorkFactor = salvageStructure,
                    RequiredSkill = "menial",
                    IsSalvageProcess = true,
                    Stances = GetSalvageStructureStances(),
                    Inputs = new[] {
                        new Input(){ Entity = "structure:campfire", Amount = new InputAmount(){  NoOfItems = 1 } }},


                    Outputs = new[] {new Output() { EntityTypeToCreate = "item:stones", Amount = new OutputAmount() { NoOfItems = 1 }
                        }
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    AgentActionState = AnimAction.Salvaging,
                    AgentAnimationStates = null,

                });
                #endregion
                #region salvageFieldKitchen
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = disassembleName,
                    SummaryDescription = disassembleWithoutLossSummary,
                    KeyName = "salvageFieldKitchen",
                    PhysicalWorkFactor = salvageStructure,
                    RequiredSkill = "menial",
                    IsSalvageProcess = true,
                    Stances = GetSalvageStructureStances(),
                    Inputs = new[] {
                        new Input(){ Entity = "structure:fieldKitchen", Amount = new InputAmount(){  NoOfItems = 1 } }},


                    Outputs = new[] { new Output() { EntityTypeToCreate = "item:fieldKitchenStove", Amount = new OutputAmount() { NoOfItems = 1 }},
                                      new Output() { EntityTypeToCreate = "item:fieldKitchenEquipment", Amount = new OutputAmount() { NoOfItems = 1 }
                        }
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    AgentActionState = AnimAction.Salvaging,
                    AgentAnimationStates = null,

                });
                #endregion
                #region salvageFieldLab
                //mp i always get errors when setting up salvage process -answer: do it like motion sensor, have a part in the structure which is same as output!! 
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = packingDownName,
                    SummaryDescription = packingDownWithoutLossSummary,
                    KeyName = "salvageFieldLab", //packing down the field lab. (not cannibalizing it. you do that to the packed item)
                    PhysicalWorkFactor = sittingLightWork,
                    RequiredSkill = "menial",
                    IsSalvageProcess = true,
                    Stances = kneelingProduction,
                    Inputs = new[] {
                        new Input(){ Entity = "structure:fieldLab", Amount = new InputAmount(){  NoOfItems = 1 } }},

                    Outputs = new[] { new Output() { EntityTypeToCreate = "item:fieldLabPacked", Amount = new OutputAmount() { NoOfItems = 1 }} //mp if I set this to true, then I get a crash: IsWasteProduct = true ,        IsWasteProduct = false

                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    //mp I want anim to look like mend:
       //            AgentActionState = AnimAction.Salvaging,
        //            AgentAnimationStates = null, 

                });


                #endregion
                #region salvageA-frameTarp 




                listOfProcessTypes.Add(new ProcessType()
                    {
                        Name = salvagingName,
                        SummaryDescription = salvageWithLossSummary,
                        KeyName = "salvageA-frameTarp",
                        RequiredSkill = "menial",
                        PhysicalWorkFactor = salvageStructure,
                        IsSalvageProcess = true,
                        Stances = GetSalvageStructureStances(),
                        Inputs = new[] {
                            new Input(){ Entity = "structure:A-frameTarp", Amount = new InputAmount(){  NoOfItems = 1 } }},


                        Outputs = new[] {new Output() { EntityTypeToCreate = "item:sticks", Amount = new OutputAmount() { NoOfItems = 1 } }, //AF 30/5-14 - NoOfItems from 2
                                    new Output() { EntityTypeToCreate = "item:wingweedLeaves", Amount = new OutputAmount() { NoOfItems = 1 } },
                                        new Output() { EntityTypeToCreate = "item:thermalTarp", Amount = new OutputAmount() { NoOfItems = 1 }
                            }
                        },
                        WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                        AgentActionState = AnimAction.Salvaging,
                        AgentAnimationStates = null,

                    });

                #endregion
                #region salvageA-frameSpoakLeaves

                listOfProcessTypes.Add(new ProcessType()
                    {
                        Name = salvagingName,
                        SummaryDescription = salvageWithLossSummary,
                        KeyName = "salvageA-frameSpoakLeaves",
                        RequiredSkill = "menial",
                        PhysicalWorkFactor = salvageStructure,
                        IsSalvageProcess = true,
                        Stances = GetSalvageStructureStances(),
                        Inputs = new[] {
                            new Input(){ Entity = "structure:A-frameSpoakLeaves", Amount = new InputAmount(){  NoOfItems = 1 } }},


                        Outputs = new[] {new Output() { EntityTypeToCreate = "item:spoakBranchesTrimmed", Amount = new OutputAmount() { NoOfItems = 1 } }, 

                        },
                        WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                        AgentActionState = AnimAction.Salvaging,
                        AgentAnimationStates = null,

                    });

                #endregion
                #region salvageA-frameScraps
                listOfProcessTypes.Add(new ProcessType()
                    {
                        Name = salvagingName,
                        SummaryDescription = salvageWithLossSummary,
                        KeyName = "salvageA-frameScraps",
                        RequiredSkill = "menial",
                        PhysicalWorkFactor = salvageStructure,
                        IsSalvageProcess = true,
                        Stances = GetSalvageStructureStances(),
                        Inputs = new[] {
                            new Input(){ Entity = "structure:A-frameScraps", Amount = new InputAmount(){  NoOfItems = 1 } }},


                        Outputs = new[] {new Output() { EntityTypeToCreate = "item:sticks", Amount = new OutputAmount() { NoOfItems = 1 } }, //AF 30/5-14 - NoOfItems from 2
                                    new Output() { EntityTypeToCreate = "item:seatCushions", Amount = new OutputAmount() { NoOfItems = 1 } },
                                        new Output() { EntityTypeToCreate = "item:panelScraps", Amount = new OutputAmount() { NoOfItems = 1 }
                            }
                        },
                        WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                        AgentActionState = AnimAction.Salvaging,
                        AgentAnimationStates = null,

                    });
                #endregion
                #region salvageLean-toTarp

                listOfProcessTypes.Add(new ProcessType()
                    {
                        Name = salvagingName,
                        SummaryDescription = salvageWithLossSummary,
                        KeyName = "salvageLean-toTarp",
                        RequiredSkill = "menial",
                        PhysicalWorkFactor = salvageStructure,
                        IsSalvageProcess = true,
                        Stances = GetSalvageStructureStances(),
                        Inputs = new[] {
                            new Input(){ Entity = "structure:lean-toTarp", Amount = new InputAmount(){  NoOfItems = 1 } }},


                        Outputs = new[] {new Output() { EntityTypeToCreate = "item:sticks", Amount = new OutputAmount() { NoOfItems = 1 } }, 
                                    new Output() { EntityTypeToCreate = "item:thermalTarp", Amount = new OutputAmount() { NoOfItems = 1 } 
                                   
                            }
                        },
                        WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                        AgentActionState = AnimAction.Salvaging,
                        AgentAnimationStates = null,

                    });

                #endregion
                #region salvageLean-toSpoakLeaves
                listOfProcessTypes.Add(new ProcessType()
                    {
                        Name = salvagingName,
                        SummaryDescription = salvageWithLossSummary,
                        KeyName = "salvageLean-toSpoakLeaves",
                        RequiredSkill = "menial",
                        PhysicalWorkFactor = salvageStructure,
                        IsSalvageProcess = true,
                        Stances = GetSalvageStructureStances(),
                        Inputs = new[] {
                            new Input(){ Entity = "structure:lean-toSpoakLeaves", Amount = new InputAmount(){  NoOfItems = 1 } }},


                        Outputs = new[] {new Output() { EntityTypeToCreate = "item:sticks", Amount = new OutputAmount() { NoOfItems = 2 } }, 
                                    new Output() { EntityTypeToCreate = "item:spoakLeaves", Amount = new OutputAmount() { NoOfItems = 1 } 
                                   
                            }
                        },
                        WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                        AgentActionState = AnimAction.Salvaging,
                        AgentAnimationStates = null,

                    });
                #endregion
                #region salvageLean-toScraps

                listOfProcessTypes.Add(new ProcessType()
                    {
                        Name = salvagingName,
                        SummaryDescription = salvageWithLossSummary,
                        KeyName = "salvageLean-toScraps",
                        RequiredSkill = "menial",
                        PhysicalWorkFactor = salvageStructure,
                        IsSalvageProcess = true,
                        Stances = GetSalvageStructureStances(),
                        Inputs = new[] {
                            new Input(){ Entity = "structure:lean-toScraps", Amount = new InputAmount(){  NoOfItems = 1 } }},


                        Outputs = new[] {new Output() { EntityTypeToCreate = "item:sticks", Amount = new OutputAmount() { NoOfItems = 1 } }, 
                                    new Output() { EntityTypeToCreate = "item:panelScraps", Amount = new OutputAmount() { NoOfItems = 1 } 
                                   
                            }
                        },
                        WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                        AgentActionState = AnimAction.Salvaging,
                        AgentAnimationStates = null,

                    });

                #endregion
                #region salvageDaysheenTipi

                listOfProcessTypes.Add(new ProcessType()
                    {
                        Name = salvagingName,
                        SummaryDescription = salvageWithLossSummary,
                        KeyName = "salvageDaysheenTipi",
                        RequiredSkill = "menial",
                        PhysicalWorkFactor = salvageStructure,
                        IsSalvageProcess = true,
                        Stances = GetSalvageStructureStances(),
                        Inputs = new[] {
                            new Input(){ Entity = "structure:daysheenTipi", Amount = new InputAmount(){  NoOfItems = 1 } }},


                        Outputs = new[] {new Output() { EntityTypeToCreate = "item:daysheenLeaves", Amount = new OutputAmount() { NoOfItems = 1 } 
                              
                            }
                        },
                        WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                        AgentActionState = AnimAction.Salvaging,
                        AgentAnimationStates = null,

                    });

                #endregion
                #region salvageDomeShelterTarp

                listOfProcessTypes.Add(new ProcessType()
                    {
                        Name = salvagingName,
                        SummaryDescription = salvageWithLossSummary,
                        KeyName = "salvageDomeShelterTarp",
                        RequiredSkill = "menial",
                        PhysicalWorkFactor = salvageStructure,
                        IsSalvageProcess = true,
                        Stances = GetSalvageStructureStances(),
                        Inputs = new[] {
                            new Input(){ Entity = "structure:domeShelterTarp", Amount = new InputAmount(){  NoOfItems = 1 } }},


                        Outputs = new[] {
                                    new Output() { EntityTypeToCreate = "item:thermalTarp", Amount = new OutputAmount() { NoOfItems = 1 } },
                                    new Output() { EntityTypeToCreate = "item:shadeleafCanes", Amount = new OutputAmount() { NoOfItems = 1 } 
                                   
                            }
                        },
                        WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                        AgentActionState = AnimAction.Salvaging,
                        AgentAnimationStates = null,

                    });
                #endregion
                #region salvageDomeShelterSpoakShingles

                listOfProcessTypes.Add(new ProcessType()
                    {
                        Name = salvagingName,
                        SummaryDescription = salvageWithLossSummary,
                        KeyName = "salvageDomeShelterSpoakShingles",
                        RequiredSkill = "menial",
                        PhysicalWorkFactor = salvageStructure,
                        IsSalvageProcess = true,
                        Stances = GetSalvageStructureStances(),
                        Inputs = new[] {
                            new Input(){ Entity = "structure:domeShelterSpoakShingles", Amount = new InputAmount(){  NoOfItems = 1 } }},


                        Outputs = new[] {
                                    new Output() { EntityTypeToCreate = "item:spoakShingles", Amount = new OutputAmount() { NoOfItems = 1 } },
                                    new Output() { EntityTypeToCreate = "item:shadeleafCanes", Amount = new OutputAmount() { NoOfItems = 1 } 
                                   
                            }
                        },
                        WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                        AgentActionState = AnimAction.Salvaging,
                        AgentAnimationStates = null,

                    });

                #endregion
                #region salvageWigwamSpoakShingles

                listOfProcessTypes.Add(new ProcessType()
                    {
                        Name = salvagingName,
                        SummaryDescription = salvageWithLossSummary,
                        KeyName = "salvageWigwamSpoakShingles",
                        RequiredSkill = "menial",
                        PhysicalWorkFactor = salvageStructure,
                        IsSalvageProcess = true,
                        Stances = GetSalvageStructureStances(),
                        Inputs = new[] {
                        new Input(){ Entity = "structure:wigwamSpoakShingles", Amount = new InputAmount(){  NoOfItems = 1 } }},


                        Outputs = new[] {
                                    new Output() { EntityTypeToCreate = "item:spoakShingles", Amount = new OutputAmount() { NoOfItems = 3 } },
                                        new Output() { EntityTypeToCreate = "item:spoakBranchesTrimmed", Amount = new OutputAmount() { NoOfItems = 1 }
                            }
                        },
                        WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                        AgentActionState = AnimAction.Salvaging,
                        AgentAnimationStates = null,

                    });
                #endregion
                #region salvageImprovisedKitchen

                listOfProcessTypes.Add(new ProcessType()
                    {
                        Name = salvagingName,
                        SummaryDescription = salvageWithLossSummary,
                        KeyName = "salvageImprovisedKitchen",
                        PhysicalWorkFactor = salvageStructure,
                        RequiredSkill = "menial",
                        IsSalvageProcess = true,
                        Stances = GetSalvageStructureStances(),
                        Inputs = new[] {
                        new Input(){ Entity = "structure:improvisedKitchen", Amount = new InputAmount(){  NoOfItems = 1 } }},
                        Outputs = new[] { new Output() { EntityTypeToCreate = "item:panelScraps", Amount = new OutputAmount() { NoOfItems = 1 }},
                                      new Output() { EntityTypeToCreate = "item:sticks", Amount = new OutputAmount() { NoOfItems = 1 }, },
                                      new Output() { EntityTypeToCreate = "item:spoakShingles", Amount = new OutputAmount() { NoOfItems = 1 }, }
                    },
                        WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                        AgentActionState = AnimAction.Salvaging,
                        AgentAnimationStates = null,

                    });
                #endregion
                #region salvageMudBrickKitchen
                listOfProcessTypes.Add(new ProcessType()
                    {
                        Name = salvagingName,
                        SummaryDescription = salvageWithLossSummary,
                        KeyName = "salvageMudBrickKitchen",
                        PhysicalWorkFactor = salvageStructure,
                        RequiredSkill = "menial",
                        IsSalvageProcess = true,
                        Stances = GetSalvageStructureStances(),
                        Inputs = new[] {
                        new Input(){ Entity = "structure:mudBrickKitchen", Amount = new InputAmount(){  NoOfItems = 1 } }},
                        Outputs = new[] { 
                                      new Output() { EntityTypeToCreate = "item:sticks", Amount = new OutputAmount() { NoOfItems = 2 }  },
                                      new Output() { EntityTypeToCreate = "item:spoakShingles", Amount = new OutputAmount() { NoOfItems = 1 }, }
                    },
                        WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                        AgentActionState = AnimAction.Salvaging,
                        AgentAnimationStates = null,

                    });
                #endregion
                #region salvageImprovisedWorkbench

                listOfProcessTypes.Add(new ProcessType()
                    {
                        Name = salvagingName,
                        SummaryDescription = salvageWithLossSummary,
                        KeyName = "salvageImprovisedWorkbench",
                        RequiredSkill = "menial",
                        PhysicalWorkFactor = salvageStructure,
                        IsSalvageProcess = true,
                        Stances = GetSalvageStructureStances(),
                        Inputs = new[] {
                        new Input(){ Entity = "structure:improvisedWorkbench", Amount = new InputAmount(){  NoOfItems = 1 } }},


                        Outputs = new[] {
                                    new Output() { EntityTypeToCreate = "item:panelScraps", Amount = new OutputAmount() { NoOfItems = 1 } },
                                     new Output() { EntityTypeToCreate = "item:sticks", Amount = new OutputAmount() { NoOfItems = 2 } }

                        },
                        WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                        AgentActionState = AnimAction.Salvaging,
                        AgentAnimationStates = null,

                    });
                #endregion
                #region salvageMudBrickWorkbench
                listOfProcessTypes.Add(new ProcessType()
                    {
                        Name = salvagingName,
                        SummaryDescription = salvageWithLossSummary,
                        KeyName = "salvageMudBrickWorkbench",
                        RequiredSkill = "menial",
                        PhysicalWorkFactor = salvageStructure,
                        IsSalvageProcess = true,
                        Stances = GetSalvageStructureStances(),
                        Inputs = new[] {
                        new Input(){ Entity = "structure:mudBrickWorkbench", Amount = new InputAmount(){  NoOfItems = 1 } }},


                        Outputs = new[] {                                    
                                     new Output() { EntityTypeToCreate = "item:sticks", Amount = new OutputAmount() { NoOfItems = 2 } },

                        },
                        WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                        AgentActionState = AnimAction.Salvaging,
                        AgentAnimationStates = null,

                    });


                #endregion
                #region salvageAbatis1



                listOfProcessTypes.Add(new ProcessType()
                    {
                        Name = salvagingName,
                        SummaryDescription = salvageWithLossSummary,
                        KeyName = "salvageAbatis1",
                        RequiredSkill = "menial",
                        PhysicalWorkFactor = salvageStructure,
                        IsSalvageProcess = true,
                        Stances = GetSalvageStructureStances(),
                        Inputs = new[] {
                            new Input(){ Entity = "structure:abatis", Amount = new InputAmount(){  NoOfItems = 1 } }
                        },

                        Outputs = new[] {
                            new Output() { EntityTypeToCreate = "item:spoakBranches", Amount = new OutputAmount(){  NoOfItems = 1 } } 
                        },

                        WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                        AgentActionState = AnimAction.Salvaging,
                        AgentAnimationStates = null,

                    });
                #endregion
  
   //salvage, clear away mines and pits:         
  
                #region salvageFavorbreadFarm
                listOfProcessTypes.Add(new ProcessType()
                    {
                        Name = clearAwayName,
                        SummaryDescription = clearAwaySummary,
                        KeyName = "salvageFavorbreadFarm",
                        RequiredSkill = "menial",
                        PhysicalWorkFactor = salvageStructure,
                        IsSalvageProcess = true,
                        Stances = GetSalvageStructureStances(),
                        Inputs = new[] {
                        new Input(){ Entity = "structure:favorbreadFarm", Amount = new InputAmount(){  NoOfItems = 1 } }},
                        WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                        AgentActionState = AnimAction.Salvaging,
                        AgentAnimationStates = null,

                    });
                    #endregion

                    #region salvageClayPit
                    listOfProcessTypes.Add(new ProcessType()
                    {
                        Name = clearAwayName,
                        SummaryDescription = clearAwaySummary,
                        KeyName = "salvageClayPit",
                        RequiredSkill = "menial",
                        PhysicalWorkFactor = salvageStructure,
                        IsSalvageProcess = true,
                        Stances = GetSalvageStructureStances(),
                        Inputs = new[] {
                        new Input(){ Entity = "structure:clayPit", Amount = new InputAmount(){  NoOfItems = 1 } }},
                        WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                        AgentActionState = AnimAction.Salvaging,
                        AgentAnimationStates = null,

                    });
                    #endregion

                    #region salvageSaltMine
                    listOfProcessTypes.Add(new ProcessType()
                    {
                        Name = clearAwayName,
                        SummaryDescription = clearAwaySummary,
                        KeyName = "salvageSaltMine",
                        RequiredSkill = "menial",
                        PhysicalWorkFactor = salvageStructure,
                        IsSalvageProcess = true,
                        Stances = GetSalvageStructureStances(),
                        Inputs = new[] {
                        new Input(){ Entity = "structure:saltMine", Amount = new InputAmount(){  NoOfItems = 1 } }},
                        WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                        AgentActionState = AnimAction.Salvaging,
                        AgentAnimationStates = null,

                    });
                    #endregion

                    #region salvageBogOrePit
                    listOfProcessTypes.Add(new ProcessType()
                    {
                        Name = clearAwayName,
                        SummaryDescription = clearAwaySummary,
                        KeyName = "salvageBogOrePit",
                        RequiredSkill = "menial",
                        PhysicalWorkFactor = salvageStructure,
                        IsSalvageProcess = true,
                        Stances = GetSalvageStructureStances(),
                        Inputs = new[] {
                        new Input(){ Entity = "structure:bogOrePit", Amount = new InputAmount(){  NoOfItems = 1 } }},
                        WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                        AgentActionState = AnimAction.Salvaging,
                        AgentAnimationStates = null,

                    });
                    #endregion

                    #region salvageRareMetalOrePit
                    //NA MINING CAMP MATERIAL
                    listOfProcessTypes.Add(new ProcessType()
                    {
                        Name = clearAwayName,
                        SummaryDescription = clearAwaySummary,
                        KeyName = "salvageRareMetalorePit1",
                        RequiredSkill = "menial",
                        PhysicalWorkFactor = salvageStructure,
                        IsSalvageProcess = true,
                        Stances = GetSalvageStructureStances(),
                        Inputs = new[] {
                        new Input(){ Entity = "structure:rareMetalOrePit1", Amount = new InputAmount(){  NoOfItems = 1 } }},
                        WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                        AgentActionState = AnimAction.Salvaging,
                        AgentAnimationStates = null,

                    });
                    #endregion
                    #region salvageRareMetalOrePit2
                    //NA MINING CAMP MATERIAL
                    listOfProcessTypes.Add(new ProcessType()
                    {
                        Name = clearAwayName,
                        SummaryDescription = clearAwaySummary,
                        KeyName = "salvageRareMetalorePit2",
                        RequiredSkill = "menial",
                        PhysicalWorkFactor = salvageStructure,
                        IsSalvageProcess = true,
                        Stances = GetSalvageStructureStances(),
                        Inputs = new[] {
                        new Input(){ Entity = "structure:rareMetalOrePit2", Amount = new InputAmount(){  NoOfItems = 1 } }},
                        WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                        AgentActionState = AnimAction.Salvaging,
                        AgentAnimationStates = null,

                    });
                    #endregion
                   

                    #region salvagePeatBank
                    listOfProcessTypes.Add(new ProcessType()
                    {
                        Name = clearAwayName,
                        SummaryDescription = clearAwaySummary,
                        KeyName = "salvagePeatBank",
                        RequiredSkill = "menial",
                        PhysicalWorkFactor = salvageStructure,
                        IsSalvageProcess = true,
                        Stances = GetSalvageStructureStances(),
                        Inputs = new[] {
                        new Input(){ Entity = "structure:peatBank", Amount = new InputAmount(){  NoOfItems = 1 } }},
                        WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                        AgentActionState = AnimAction.Salvaging,
                        AgentAnimationStates = null,

                    });
                    #endregion

                    
                    #region salvage FishTraps
                        #region salvageFishTrapCreek   weir sticks
                    listOfProcessTypes.Add(new ProcessType()
                    {
                        Name = salvagingName,
                        SummaryDescription = salvageWithLossSummary,
                        KeyName = "salvageFishTrapCreekSticks",
                        PhysicalWorkFactor = salvageStructure,
                        RequiredSkill = "menial",
                        IsSalvageProcess = true,
                        Stances = GetSalvageStructureStances(),
                        Inputs = new[] {
                        new Input(){ Entity = "structure:fishTrapCreekSticks", Amount = new InputAmount(){  NoOfItems = 1 } }
                        },
                        Outputs = new[] {
                        new Output() { EntityTypeToCreate = "item:sticks", Amount = new OutputAmount() { NoOfItems = 2 } }
                        },
                        WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                        AgentActionState = AnimAction.Salvaging,
                        AgentAnimationStates = null
                    });
                        #endregion
                    #region salvageFishTrapCreekNet   weir net
                    listOfProcessTypes.Add(new ProcessType()
                    {
                        Name = salvagingName,
                        SummaryDescription = salvageWithLossSummary,
                        KeyName = "salvageFishTrapCreekNet",
                        PhysicalWorkFactor = salvageStructure,
                        RequiredSkill = "menial",
                        IsSalvageProcess = true,
                        Stances = GetSalvageStructureStances(),
                        Inputs = new[] {
                        new Input(){ Entity = "structure:fishTrapCreekNet", Amount = new InputAmount(){  NoOfItems = 1 } }
                        },
                        Outputs = new[] {
                        new Output() { EntityTypeToCreate = "item:fishingNet", Amount = new OutputAmount() { NoOfItems = 2 } },
                        new Output() { EntityTypeToCreate = "item:sticks", Amount = new OutputAmount() { NoOfItems = 1 } }
                        },
                        WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                        AgentActionState = AnimAction.Salvaging,
                        AgentAnimationStates = null
                    });
                    #endregion
                        #region salvageFishTrapCoast - fyke
                    listOfProcessTypes.Add(new ProcessType()
                    {
                        Name = packingDownName,
                        SummaryDescription = packingDownWithoutLossSummary,
                        KeyName = "salvageFishTrapCoast",
                        PhysicalWorkFactor = salvageStructure,
                        RequiredSkill = "menial",
                        IsSalvageProcess = true,
                        Stances = GetSalvageStructureStances(),
                        Inputs = new[] {
                        new Input(){ Entity = "structure:fishTrapCoast", Amount = new InputAmount(){  NoOfItems = 1 }}
                    },
                        Outputs = new[] {
                        new Output() { EntityTypeToCreate = "item:fishTrapHoopNet", Amount = new OutputAmount() { NoOfItems = 1 }},
                        new Output() { EntityTypeToCreate = "item:fishingNet", Amount = new OutputAmount() { NoOfItems = 1 }}
                    },
                        WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                        AgentActionState = AnimAction.Salvaging,
                        AgentAnimationStates = null
                    });
                        #endregion
                        #region salvageFishTrapShore - basket
                    listOfProcessTypes.Add(new ProcessType()
                    {
                        Name = packingDownName,
                        SummaryDescription = packingDownWithoutLossSummary,
                        KeyName = "salvageFishTrapShoreBasket",
                        PhysicalWorkFactor = salvageStructure,
                        RequiredSkill = "menial",
                        IsSalvageProcess = true,
                        Stances = GetSalvageStructureStances(),
                        Inputs = new[] {
                        new Input(){ Entity = "structure:fishTrapShoreBasket", Amount = new InputAmount(){  NoOfItems = 1 } }
                    },
                        Outputs = new[] {
                        new Output() { EntityTypeToCreate = "item:fishTrapBasket", Amount = new OutputAmount() { NoOfItems = 1 } }
                    },
                        WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForTinyCraftingTask },
                        AgentActionState = AnimAction.Salvaging,
                        AgentAnimationStates = null
                    });
                        #endregion
                    #region salvageFishTrapShoreHoopNet
                    listOfProcessTypes.Add(new ProcessType()
                    {
                        Name = packingDownName,
                        SummaryDescription = packingDownWithoutLossSummary,
                        KeyName = "salvageFishTrapShoreHoopNet",
                        PhysicalWorkFactor = salvageStructure,
                        RequiredSkill = "menial",
                        IsSalvageProcess = true,
                        Stances = GetSalvageStructureStances(),
                        Inputs = new[] {
                        new Input(){ Entity = "structure:fishTrapShoreHoopNet", Amount = new InputAmount(){  NoOfItems = 1 } }
                    },
                        Outputs = new[] {
                        new Output() { EntityTypeToCreate = "item:fishTrapHoopNet", Amount = new OutputAmount() { NoOfItems = 1 } }
                    },
                        WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForTinyCraftingTask },
                        AgentActionState = AnimAction.Salvaging,
                        AgentAnimationStates = null
                    });
                    #endregion
                    #endregion

                    // TB Farming 19.03.2015
                    #region salvageSmallPlot
                    listOfProcessTypes.Add(new ProcessType()
                    {
                        Name = clearAwayName,
                        SummaryDescription = clearAwaySummary,
                        KeyName = "salvageSmallPlot",
                        PhysicalWorkFactor = salvageStructure,
                        RequiredSkill = "menial",
                        IsSalvageProcess = true,
                        Stances = GetSalvageStructureStances(),
                        Inputs = new[] {
                            new Input(){ Entity = "structure:smallPlot", Amount = new InputAmount(){  NoOfItems = 1 } }
                        },
                        WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                        AgentActionState = AnimAction.Salvaging,
                        AgentAnimationStates = null,
                        ProcessToolSetKey = "toolSetUnPlowingTools"
                    });
                    #endregion
                    #region salvageLargePlot
                    listOfProcessTypes.Add(new ProcessType()
                    {
                        Name = clearAwayName,
                        SummaryDescription = clearAwaySummary,
                        KeyName = "salvageLargePlot",
                        PhysicalWorkFactor = salvageStructure,
                        RequiredSkill = "menial",
                        IsSalvageProcess = true,
                        Stances = GetSalvageStructureStances(),
                        Inputs = new[] {
                            new Input(){ Entity = "structure:largePlot", Amount = new InputAmount(){  NoOfItems = 1 } }
                        },
                        WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                        AgentActionState = AnimAction.Salvaging,
                        AgentAnimationStates = null,
                        ProcessToolSetKey = "toolSetUnPlowingTools"
                    });
                    #endregion


                #endregion


                #region ///Special actions///


                /*    #region Set Sentry Attack Vermin

                    listOfProcessTypes.Add(new ProcessType()
                    {
                        Name = "Attack vermin", 
                        KeyName = "enableAttackVermin",
                        SummaryDescription = "Switches the robot to attack vermin as well as normal threats",
                        Description = "When in the attack vermin mode, the robot will engage any animals designated as vermin in addition to normal threats.",
                        PhysicalWorkFactor = 0, //shouldn't use any energy for changing a property
                        AttacksVerminValueToSet = true,
                        Stances = kneelingProduction,
                        EnablesSharedActionProcesses = new[] { "disableAttackVermin" },
                        DisablesSharedActionProcesses = new[] { "enableAttackVermin" },   
                        WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForInstantCraftingTask },
                        AgentAnimationStates = new[] { AnimModifier.Electronic }
                    });

                    listOfProcessTypes.Add(new ProcessType()
                    {
                        Name = "Ignore vermin",
                        KeyName = "disableAttackVermin",
                        SummaryDescription = "Switches the robot to ignore vermin",
                        Description = "When in the attack vermin mode, the robot will engage any animals designated as vermin in addition to normal threats.",
                        PhysicalWorkFactor = 0, //shouldn't use any energy for changing a property
                        AttacksVerminValueToSet = false,
                        Stances = kneelingProduction,
                        EnablesSharedActionProcesses = new[] { "enableAttackVermin" },
                        DisablesSharedActionProcesses = new[] { "disableAttackVermin" },   
                        WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForInstantCraftingTask },
                        AgentAnimationStates = new[] { AnimModifier.Electronic }
                    });
                    #endregion*/

                    #region bait special actions
                    listOfProcessTypes.Add(new ProcessType()
                    {
                        Name = "Use blackpulp bait", ////mp this is a special action shown on the special action tooltip (next to BEGIN) also shown in task manager (without an added type name) and tv screen. Should be written in imperative (NOT ing- form)
                        KeyName = "changeBaitToBlackpulp",
                        JobTypeKey = "checkTrapsJobType",
                        RequiredSkill = "menial",
                        SummaryDescription = "This type of food will be brought to the trap when inspected",
                        Description = "Even with no bait option chosen, we will make sure to inspect the trap regularly, and, in case it has been sprung, reset it.",
                        PhysicalWorkFactor = AllGameData.ProcessLoader.moderateWork,
                        Stances = kneelingProduction,
                        Inputs = new[] 
                        {
                            new Input(){ Entity = "item:blackpulp", IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 1 } }
                        },
                        Outputs = new[] 
                        {
                            new Output(){ EntityTypeToCreate = "item:blackpulp", Amount = new OutputAmount(){ NoOfItems = 1 }, IsWasteProduct = true}
                        },
                        WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = AllGameData.ProcessLoader.timeForInstantCraftingTask },
                    });
                    listOfProcessTypes.Add(new ProcessType()
                    {
                        Name = "Use glassy creeper bait",  ////mp this is a special action shown on the special action tooltip (next to BEGIN) also shown in task manager (without an added type name) and tv screen. Should be written in imperative (NOT ing- form)
                        KeyName = "changeBaitToGlassyCreeper",
                        JobTypeKey = "checkTrapsJobType",
                        RequiredSkill = "menial",
                        SummaryDescription = "This type of food will be brought to the trap when inspected",
                        Description = "Even with no bait option chosen, we will make sure to inspect the trap regularly, and, in case it has been sprung, reset it.",
                        PhysicalWorkFactor = AllGameData.ProcessLoader.moderateWork,
                        Stances = kneelingProduction,
                        Inputs = new[] 
                        {
                            new Input(){ Entity = "item:glassyCreeperPods", IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 1 } }
                        },
                        Outputs = new[] 
                        {
                            new Output(){ EntityTypeToCreate = "item:glassyCreeperPods", Amount = new OutputAmount(){ NoOfItems = 1 }, IsWasteProduct = true}
                        },
                        WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = AllGameData.ProcessLoader.timeForInstantCraftingTask },
                    });
                    listOfProcessTypes.Add(new ProcessType()
                    {
                        Name = "Use rat meat bait",  ///mp this is a special action shown on the special action tooltip (next to BEGIN) also shown in task manager (without an added type name) and tv screen. Should be written in imperative (NOT ing- form)
                        KeyName = "changeBaitToRatMeat",
                        JobTypeKey = "checkTrapsJobType",
                        RequiredSkill = "menial",
                        SummaryDescription = "This type of food will be brought to the trap when inspected",
                        Description = "Even with no bait option chosen, we will make sure to inspect the trap regularly, and, in case it has been sprung, reset it.",
                        PhysicalWorkFactor = AllGameData.ProcessLoader.moderateWork,
                        Stances = kneelingProduction,
                        Inputs = new[] 
                        {
                            new Input(){ Entity = "item:binalRatChunk", IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 1 } }
                        },
                        Outputs = new[] 
                        {
                            new Output(){ EntityTypeToCreate = "item:binalRatChunk", Amount = new OutputAmount(){ NoOfItems = 1 }, IsWasteProduct = true } // the input is only there to show prerequisites...
                        },
                        WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = AllGameData.ProcessLoader.timeForInstantCraftingTask },
                    });
                    listOfProcessTypes.Add(new ProcessType()
                    {
                        Name = "Don't use bait",
                        KeyName = "changeBaitToNoBait",
                        JobTypeKey = "checkTrapsJobType",
                        RequiredSkill = "menial",
                        SummaryDescription = "No food will be brought to the trap when inspected",
                        Description = "Even with no bait option chosen, we will make sure to inspect the trap regularly, and, in case it has been sprung, reset it.",
                        PhysicalWorkFactor = AllGameData.ProcessLoader.moderateWork,
                        Stances = kneelingProduction,
                        WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = AllGameData.ProcessLoader.timeForInstantCraftingTask },
                    });
                    #endregion

                    listOfProcessTypes.Add(new ProcessType()
                    {
                        Name = "Reactivating trap", //shown in task manager suceded by type name
                        KeyName = "activateTrapWithBlackpulp",
                        JobTypeKey = "checkTrapsJobType",
                        RequiredSkill = "menial",
                        SummaryDescription = "", //not seen by player
                        Description = "",
                        PhysicalWorkFactor = AllGameData.ProcessLoader.moderateWork,
                        Stances = kneelingProduction,
                        Inputs = new[] 
                        {
                            new Input(){ Entity = "item:blackpulp", IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 1 } }
                        },
                        Outputs = new[] 
                        {
                            new Output(){ EntityTypeToCreate = "item:blackpulp", Amount = new OutputAmount(){ NoOfItems = 1 }, IsWasteProduct = true}
                        },
                        WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = AllGameData.ProcessLoader.timeForTinyCraftingTask },
                    });

                    listOfProcessTypes.Add(new ProcessType() //new bait glassy creeper
                    {
                        Name = "Reactivating trap",//shown in task manager suceded by type name
                        KeyName = "activateTrapWithGlassyCreeper",
                        JobTypeKey = "checkTrapsJobType",
                        RequiredSkill = "menial",
                        SummaryDescription = "", //not seen by player
                        Description = "",
                        PhysicalWorkFactor = AllGameData.ProcessLoader.moderateWork,
                        Stances = kneelingProduction,
                        Inputs = new[] 
                        {
                            new Input(){ Entity = "item:glassyCreeperPods", IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 1 } }
                        },
                        Outputs = new[] 
                        {
                            new Output(){ EntityTypeToCreate = "item:glassyCreeperPods", Amount = new OutputAmount(){ NoOfItems = 1 },IsWasteProduct=true}
                        },
                        WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = AllGameData.ProcessLoader.timeForTinyCraftingTask },
                    });
                    listOfProcessTypes.Add(new ProcessType()
                    {
                        Name = "Reactivating trap",//shown in task manager suceded by type name
                        KeyName = "activateTrapWithRatMeat",
                        JobTypeKey = "checkTrapsJobType",
                        RequiredSkill = "menial",
                        SummaryDescription = "", //not seen by player
                        Description = "",
                        PhysicalWorkFactor = AllGameData.ProcessLoader.moderateWork,
                        Stances = kneelingProduction,
                        Inputs = new[] 
                        {
                            new Input(){ Entity = "item:binalRatChunk", IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 1 } }
                        },
                        Outputs = new[] 
                        {
                            new Output(){ EntityTypeToCreate = "item:binalRatChunk", Amount = new OutputAmount(){ NoOfItems = 1 },IsWasteProduct=true}
                        },
                        WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = AllGameData.ProcessLoader.timeForTinyCraftingTask },
                    });

                    listOfProcessTypes.Add(new ProcessType()
                    {
                        Name = "Kill animal",
                        KeyName = "killTrappedAnimal",
                        RequiredSkill = "menial",
                        SummaryDescription = "", //not seen by player
                        Description = "",
                        PhysicalWorkFactor = AllGameData.ProcessLoader.moderateWork,
                        Stances = kneelingProduction,
                        WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = AllGameData.ProcessLoader.timeForInstantCraftingTask },
                    });

                    listOfProcessTypes.Add(new ProcessType()
                    {
                        Name = "Reactivating trap",
                        KeyName = "activateSnare",
                        JobTypeKey = "checkTrapsJobType",
                        RequiredSkill = "menial",
                        SummaryDescription = "", //not seen by player
                        Description = "",
                        PhysicalWorkFactor = AllGameData.ProcessLoader.moderateWork,
                        Stances = kneelingProduction,
                        WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = AllGameData.ProcessLoader.timeForTinyCraftingTask },
                    });
//mp these cannot be  moved to the scenarios where they are used because detectiontag requires the terrainfeature to be in the general terrainfeatureloader.....sep 27

               
/*  mp not used
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = "Build vine rope bridge",
                    KeyName = "buildRopeBridgeVine", //twinkler island
                    RequiredSkill = "bushcraft",
                    SummaryDescription = "We can build a simple bridge if we have enough rope",
                    Description = "", //
                    PhysicalWorkFactor = AllGameData.ProcessLoader.moderateWork,
                    Stances = kneelingProduction,
                    RequiresBoldStance = true,

                    Inputs = new[] {
                        new Input(){ Entity = "item:vine", IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 1 } }
                    },

                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = AllGameData.ProcessLoader.timeForSmallCraftingTask },

                });
*/


                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = "Fumigating nest",  //mp not sure if the object is shown in task manager? then delete it here
                    KeyName = "useSulfurSmokeBomb",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = AllGameData.ProcessLoader.moderateWork,
                    Stances = AllGameData.ProcessLoader.kneelingProduction,
                    SummaryDescription = "It's being suggested that we fumigate the twinkler nest using some sort of smoke bomb.",
                    Description = "If we carry out this plan, we need to take extreme caution because any disturbance of the nest is likely to trigger a defense response from the twinklers.",
                    RequiresBoldStance = true,
                    //EventActionsOnStart = GameData.Instance.AllActionSets["startUseSulfurSmokeBombRemark"],                    
                    Inputs = new[] {
                        new Input(){ Entity = "item:sulfurSmokeBomb", IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 1 } }
                    },

                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = AllGameData.ProcessLoader.timeForSmallCraftingTask }, //mp make sure the task lasts long enough for both of the dialogue hooks to fire. if too short, the "fire in the hole" remark will not execute because the preceding one is still active.

                });

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = "Blowing up nest",
                    KeyName = "useVarmintBomb", //change keyName
                    SummaryDescription = "Place and detonate explosives in the nest entrance", //placeholder
                    Description = "With a strong enough bomb, it is possible to collapse the dirt walls at the entrance of this quadite nest. However, the colony will likely rebuild the entrance eventually.",//
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = AllGameData.ProcessLoader.moderateWork,
                    Stances = AllGameData.ProcessLoader.kneelingProduction,

                    RequiresBoldStance = true,
                    Inputs = new[]
                    {
                        new Input(){ Entity = "item:varmintBomb", IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 1 } },
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = AllGameData.ProcessLoader.timeForTinyCraftingTask }, //was timeForSmallCraftingTask CHANGED FOR VIDEO, CHANGE BACK.  mp make sure the task lasts long enough for both of the dialogue hooks to fire. if too short, the "fire in the hole" remark will not execute because the preceding one is still active.

                });

                listOfProcessTypes.Add(new ProcessType()//bso not in use, should be deleted
                {
                    Name = "Digging up rat nest",
                    KeyName = "digUpRatNest",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = AllGameData.ProcessLoader.moderateWork,
                    Stances = AllGameData.ProcessLoader.kneelingProduction,
                    SummaryDescription = "summary wip",
                    Description = "Description wip",
                    RequiresBoldStance = true,
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = AllGameData.ProcessLoader.timeForSmallCraftingTask },

                });


                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = "Retrieving supplies",
                    KeyName = "retrieveCrates",
                    RequiredSkill = "bushcraft",
                    SummaryDescription = "We should be able to climb down and get the supplies with the use of some climbing gear",
                    Description = "The supplies are lying at the bottom of a slot canyon where water has carved a deep crevice in the sandstone.", //
                    PhysicalWorkFactor = AllGameData.ProcessLoader.moderateWork,
                    Stances = AllGameData.ProcessLoader.kneelingProduction,
                    RequiresBoldStance = true,

                    Inputs = new[] {
                        new Input(){ Entity = "item:vine", IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 1 } }
                    },

                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = AllGameData.ProcessLoader.timeForTinyCraftingTask },
                    //                Tools = "climbingRope"  ..MP: caused a crash, so i'm just using vine as item input for now. right now, it always needs an input.
                });



                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = "Rescuing team member",
                    KeyName = "rescueColleague",
                    RequiredSkill = "bushcraft",
                    SummaryDescription = "Our lost mission member is alive but motionless at the bottom of this crevice",
                    Description = "He seems to be unconscious. We need to climb down and perform first aid as soon as possible. Some kind of harness is needed to get him up.", //
                    PhysicalWorkFactor = AllGameData.ProcessLoader.moderateWork,
                    Stances = AllGameData.ProcessLoader.kneelingProduction,
                    RequiresBoldStance = true,

                    Inputs = new[] {
                        new Input(){ Entity = "item:vine", IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 1 } }
                    },

                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = AllGameData.ProcessLoader.timeForSmallCraftingTask },
  //                Tools = "climbingRope"  ..MP: caused a crash, so i'm just using vine as item input for now. right now, it always needs an input.
                });

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = "Cleaning shell",
                    KeyName = "cleanTurnipShell",
                    SummaryDescription = "N/A",//placeholder
                    Description = "N/A", //placeholder
                    PhysicalWorkFactor = moderateWork,
                    Stances = kneelingProduction,
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallHarvestingTask },
                    AgentActionState = AnimAction.Harvesting
                });
                
                #region placeFishTrap
                #region placeFishTrapCreekSticks -fish weir sticks
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = "Build fish weir (sticks)", //mp not sure if the object is shown in task manager? then delete it here
                    KeyName = "placeFishTrapCreekSticks",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "bushcraft",
                  //  UserCanCancel = false,
                    SummaryDescription = "Build a fish weir from sticks",
                    Description = "A fish weir made of wooden fences could catch a large number of fish when they migrate through this body of water.",
                    PhysicalWorkFactor = AllGameData.ProcessLoader.moderateWork,
                    Stances = constructionStances,
                    Inputs = new[] {
                        new Input(){ Entity = "item:sticks", IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 5 } }
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "structure:fishTrapCreekSticks", Amount = new OutputAmount(){ NoOfItems = 1 } }
                    },
                   // DisablesSharedActionProcesses = new[] { "placeFishTrapCreekNet" },          
                    AgentActionState = AnimAction.Building,
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = AllGameData.ProcessLoader.timeForBigCraftingTask }
                   // Tools = "cordage", maybe use cordage?
                });
                    #endregion
                #region placeFishTrapCreekNet -fish weir net
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = "Build fish weir (netting)", //mp not sure if the object is shown in task manager? then delete it here
                    KeyName = "placeFishTrapCreekNet",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "fishing",
                  //  UserCanCancel = false,
                    SummaryDescription = "Build a fish weir from netting",
                    Description = "A two-way fish weir made of netting could catch a large number of fish when they migrate both ways through this body of water.",
                    PhysicalWorkFactor = AllGameData.ProcessLoader.moderateWork,
                    Stances = constructionStances,
                    Inputs = new[] {
                        new Input(){ Entity = "item:fishingNet", IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 2 } },
                        new Input(){ Entity = "item:sticks", IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 3 } }
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "structure:fishTrapCreekNet", Amount = new OutputAmount(){ NoOfItems = 1 } }
                    },
                    DisablesSharedActionProcesses = new[] { "placeFishTrapCreekSticks" },                   
                    AgentActionState = AnimAction.Building,
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = AllGameData.ProcessLoader.timeForBigCraftingTask }
                    // Tools = "cordage", maybe use cordage?
                });
                #endregion
                #region placeFishTrapCoast (fyke)
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = "Build fish trap (fyke)",
                    KeyName = "placeFishTrapCoast",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "fishing",
                    SummaryDescription = "Build a 'fyke' fish trap",
                    Description = "This body of water is a typical habitat of the 'streak fin' fish and it would be a suitable place for setting up a fish trap specially designed to catch this species.",
                    PhysicalWorkFactor = AllGameData.ProcessLoader.moderateWork,
                    Stances = kneelingProduction,
                    Inputs = new[] 
                    {
                        new Input(){ Entity = "item:fishTrapHoopNet", IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } }, //mp IsConsumed = true,   because of tech reasons i guess??
                        new Input(){ Entity = "item:fishingNet", IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } }
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "structure:fishTrapCoast", Amount = new OutputAmount(){ NoOfItems = 1 } }
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = AllGameData.ProcessLoader.timeForSmallCraftingTask }
                });
                    #endregion
                #region placeFishTrapShore  (basket)
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = "Place fish trap (basket)",
                    KeyName = "placeFishTrapShoreBasket",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "bushcraft",
                    SummaryDescription = "Place 'basket' fish trap",
                    Description = "This body of water is a typical habitat of the 'carbon tail' fish and it would be a good place for setting up a fish trap specially designed to catch this species.",
                    PhysicalWorkFactor = AllGameData.ProcessLoader.moderateWork,
                    Stances = kneelingProduction,
                    Inputs = new[] {
                        new Input(){ Entity = "item:fishTrapBasket", IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } }
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "structure:fishTrapShoreBasket", Amount = new OutputAmount(){ NoOfItems = 1 } }
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = AllGameData.ProcessLoader.timeForSmallCraftingTask }
                });
                    #endregion
                #region placeFishTrapShoreHoopNet  
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = "Place fish trap (hoop net)",
                    KeyName = "placeFishTrapShoreHoopNet",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "fishing",
                    SummaryDescription = "Place 'hoop net' fish trap",
                    Description = "This body of water is a typical habitat of the 'carbon tail' fish and it would be a good place for setting up a fish trap specially designed to catch this species.",
                    PhysicalWorkFactor = AllGameData.ProcessLoader.moderateWork,
                    Stances = kneelingProduction,
                    Inputs = new[] {
                        new Input(){ Entity = "item:fishTrapHoopNet", IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } }
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "structure:fishTrapShoreHoopNet", Amount = new OutputAmount(){ NoOfItems = 1 } }
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = AllGameData.ProcessLoader.timeForSmallCraftingTask }
                });
                #endregion
                #endregion
                #region checkFishTrap
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = "Checking fish trap",
                    KeyName = "checkFishTrap",
                    JobTypeKey = "checkTrapsJobType",
                    UserCanCancel = false,
                    UserCannotCancelReason = "Checking cannot be cancelled. Abandon or salvage the fish trap to stop using it",
                    SummaryDescription = "Check trap for fish.",
                    Description = "We should regularly check if the trap has caught any fish",
                    PhysicalWorkFactor = AllGameData.ProcessLoader.moderateWork,
                    Stances = AllGameData.ProcessLoader.kneelingProduction,
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForInstantCraftingTask },
                    RequiredSkill = "menial"
                });
                #endregion

                #region build piers

                #region buildSimplePort
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = "Build simple port", //shown on action button but also shown in task mnagner with an added type name
                    KeyName = "buildSimplePort",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "construction",
                    SummaryDescription = "Build a simple port which allows us to receive boats and sell goods to other settlements. Small capacity. Suited for selling food.", //mostly copied from structureloader !!
                    Description = "Has a pier where small boats and barges can moor and a storehouse where goods intended for sale can be placed. The clay storehouse is raised on pillars to keep a small amount of goods safe from vermin. \nThe storehouse also has a space for storage of items that are not intended for sale.", // \nThe pier is made from heavy spoak branches whose open shapes allow water to pass through. 
                    //mp would be good if it could show the structure thumbnail
                    PhysicalWorkFactor = constructionExertion,
                    Stances = constructionStances,                    
                    Inputs = new[] { new Input() { Entity = "item:spoakBranchesTrimmed", Amount = new InputAmount() { NoOfItems = 3 }, IsConsumed = true },  //mp why is it that it needs to be set to  IsConsumed = true ?? - compare with building fishTrapCoast ???                               
                                     new Input() { Entity = "item:spoakShingles", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = true },
                                     new Input() { Entity = "item:solidMudBrick", Amount = new InputAmount() { NoOfItems = 5 }, IsConsumed = true },
                                     new Input() { Entity = "item:waterCaneStem", Amount = new InputAmount() { NoOfItems = 3 }, IsConsumed = true },
                    },
                    Outputs = new[]  //output - not like fishTrapCoast 
                    {
                            new Output() { EntityTypeToCreate = "structure:simplePort", Amount = new OutputAmount(){  NoOfItems = 1 } },

                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForLargePrimitiveShelter },
                    ProcessToolSetKey = "toolSetCordage",
                    AgentActionState = AnimAction.Building,
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region buildCanopyPort
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = "Build canopy port", //shown on action button but also shown in task mnagner with an added type name
                    KeyName = "buildCanopyPort",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "construction",
                    SummaryDescription = "Build a port which allows us to receive boats and sell goods to other settlements. Large capacity. No vermin protection.", //mostly copied from structureloader!! 
                    Description = "Has a pier where small boats and barges can moor and a large storage canopy where goods intended for sale can be placed. The goods are not protected from vermin, so this structure is NOT suited for trading food. \nThe canopy also has a space for storage of items that are not intended for sale.", // \nThe pier, where the boats will dock, is made of heavy spoak branches whose open shapes allow water to pass through.
                    //mp would be good if it could show the structure thumbnail
                    PhysicalWorkFactor = constructionExertion,
                    Stances = constructionStances,
                    Inputs = new[] { new Input() { Entity = "item:daysheenLeaves", Amount = new InputAmount() { NoOfItems = 2 }, IsConsumed = true },
                                     new Input() { Entity = "item:spoakBranchesTrimmed", Amount = new InputAmount() { NoOfItems = 3 }, IsConsumed = true },  //mp why is it that it needs to be set to  IsConsumed = true ?? - compare with building fishTrapCoast ???                               

                                     new Input() { Entity = "item:waterCaneStem", Amount = new InputAmount() { NoOfItems = 3 }, IsConsumed = true },
                    },
                    Outputs = new[]  //output - not like fishTrapCoast 
                    {
                            new Output() { EntityTypeToCreate = "structure:canopyPort", Amount = new OutputAmount(){  NoOfItems = 1 } },

                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumPrimitiveShelter },
                    ProcessToolSetKey = "toolSetCordage",
                    AgentActionState = AnimAction.Building,
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region build improvised landing
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = "Build landing", //shown on action button but also shown in task mnagner with an added type name
                    KeyName = "buildImprovisedLanding",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "construction",
                    SummaryDescription = "Build an improvised landing for boats to moor. Can transfer passengers. Goods can ONLY be received, not sold.", //mostly copied from structureloader
                    Description = "This simple structure does not allow us to sell goods because it lacks a storehouse. It is only suited for receiving goods and embarking and disembarking passengers.",
                    PhysicalWorkFactor = constructionExertion,
                    Stances = constructionStances,
                    Inputs = new[] { new Input() { Entity = "item:waterCaneStem", Amount = new InputAmount() { NoOfItems = 2 }, IsConsumed = true },  //mp IsConsumed = true - compare with building fishTrapCoast??                                
                    },
                    Outputs = new[]  //output - not like fishTrapCoast 
                    {
                            new Output() { EntityTypeToCreate = "structure:landingImprovised", Amount = new OutputAmount(){  NoOfItems = 1 } }
                    },
                    //output - compare with fishTrapCoast 
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallPrimitiveShelter },
                    ProcessToolSetKey = "toolSetCordage",
                    AgentActionState = AnimAction.Building,
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion
                #endregion

                #region Build 'favorbread farm'
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = "Build favorbread farm", //shown on action button but also shown in task mnagner with an added type name
                    KeyName = "buildFavorbreadFarm",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "bushcraft",
                    SummaryDescription = "Dig a pit where we can cultivate the favorbread vegetable", //mostly copied from structureloader
                    Description = "By excavating a small garden directly underneath the dead sanctuary tree we can revive the favorbread if we provide it with the carbohydrates that the tree no longer supplies it with. We have found that the blackpulp is well suited as a nutrient that will make the favorbread grow vigorously.\n Note: We have found that this method of cultivation is not possible next to a LIVING sanctuary tree because disturbing the connection between the two organisms elicits a dangerous, defensive response from them.",
                    PhysicalWorkFactor = constructionExertion,
                    Stances = constructionStances,
                    Inputs = new[] { new Input() { Entity = "item:sticks", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = true },  //mp IsConsumed = true - compare with building fishTrapCoast??                                
                    },
                    Outputs = new[]  //
                    {
                            new Output() { EntityTypeToCreate = "structure:favorbreadFarm", Amount = new OutputAmount(){  NoOfItems = 1 } }
                    },
                    //output - compare with fishTrapCoast 
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeToCreatePit },
                    ProcessToolSetKey = "toolSetDiggingConstruction",
                    AgentActionState = AnimAction.Digging,
                 //   AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region buildClayPit
             listOfProcessTypes.Add(new ProcessType()
                {
                    Name = "Build clay pit", 
                    KeyName = "establishClayPit",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "construction",
                    SummaryDescription = "Establish a clay pit from which we can extract large amounts of clay",
                    Description = "By digging an extraction site here, we will have access to the rich deposits of clay beneath the surface which are otherwise hard to reach. (When the clay pit is established, clay can then be ordered from the Production Manager)",
                    PhysicalWorkFactor = constructionExertion,
                    Stances = constructionStances,
                    Inputs = new[]
                    {
                        new Input(){ Entity = "item:sticks", IsConsumed = false, Amount = new InputAmount(){ NoOfItems = 1 }},
                    },                 
                    Outputs = new[] 
                    {
                         new Output() { EntityTypeToCreate = "structure:clayPit", Amount = new OutputAmount(){  NoOfItems = 1 } }
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeToCreatePit },
                    ProcessToolSetKey = "toolSetDiggingConstruction",
                    AgentActionState = AnimAction.Digging,
                   // AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
            

                #endregion

             #region buildSaltMine
             listOfProcessTypes.Add(new ProcessType()
             {
                 Name = "Build salt mine",
                 KeyName = "establishSaltMine",
                 JobTypeKey = "constructionJobType",
                 RequiredSkill = "construction",
                 SummaryDescription = "Establish a salt mine from which we can extract large amounts of salt",
                 Description = "By digging an extraction site here, we will have access to the rich deposits of salt beneath the surface which are otherwise hard to reach. (When the salt mine is established, salt can then be ordered from the Production Manager)",
                 PhysicalWorkFactor = constructionExertion,
                 Stances = constructionStances,
                 Inputs = new[]
                    {
                        new Input(){ Entity = "item:sticks", IsConsumed = false, Amount = new InputAmount(){ NoOfItems = 1 }},
                    },    
                 Outputs = new[] 
                    {
                         new Output() { EntityTypeToCreate = "structure:saltMine", Amount = new OutputAmount(){  NoOfItems = 1 } }
                    },
                 WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeToCreatePit },
                 ProcessToolSetKey = "toolSetDiggingConstruction",
                 AgentActionState = AnimAction.Digging,
                // AgentAnimationStates = new[] { AnimModifier.Improvised }
             });

             #endregion

             #region buildBogOrePit
             listOfProcessTypes.Add(new ProcessType()
             {
                 Name = "Build bog ore pit",
                 KeyName = "establishBogOrePit",
                 JobTypeKey = "constructionJobType",
                 RequiredSkill = "construction",
                 SummaryDescription = "Establish a pit from which we can extract large amounts of bog ore",
                 Description = "By digging an extraction site here, we will have access to the rich deposits of bog ore beneath the surface which are otherwise hard to reach. (When the bog ore pit is established, bog ore can then be ordered from the Production Manager)",
                 PhysicalWorkFactor = constructionExertion,
                 Stances = constructionStances,
                 Inputs = new[]
                    {
                        new Input(){ Entity = "item:sticks", IsConsumed = false, Amount = new InputAmount(){ NoOfItems = 1 }},
                    },    
                 Outputs = new[] 
                    {
                         new Output() { EntityTypeToCreate = "structure:bogOrePit", Amount = new OutputAmount(){  NoOfItems = 1 } }
                    },
                 WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeToCreatePit },
                 ProcessToolSetKey = "toolSetDiggingConstruction",
                 AgentActionState = AnimAction.Digging,
               //  AgentAnimationStates = new[] { AnimModifier.Improvised }
             });

             #endregion

             #region Build Scandium mine   buildRareMetalOrePit1
             //NA MINING CAMP MATERIAL
             listOfProcessTypes.Add(new ProcessType()
             {
                 Name = "Build scandium mine",
                 KeyName = "establishRareMetalOrePit",
                 JobTypeKey = "constructionJobType",
                 RequiredSkill = "construction",
                 SummaryDescription = "Establish a pit from which we can extract scandium ore",
                 Description = "By digging an extraction site here, we will have access to the rich deposits of scandium ore beneath the surface. (When the scandium mine is established, scandium ore can then be ordered from the Production Manager)",
                 PhysicalWorkFactor = constructionExertion,
                 Stances = constructionStances,
                 Inputs = new[]
                    {
                        new Input(){ Entity = "item:sticks", IsConsumed = false, Amount = new InputAmount(){ NoOfItems = 1 }},
                    },
                 Outputs = new[] 
                    {
                         new Output() { EntityTypeToCreate = "structure:rareMetalOrePit1", Amount = new OutputAmount(){  NoOfItems = 1 } }
                    },
                 WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeToCreatePit },
                 ProcessToolSetKey = "toolSetDiggingConstruction",
                 AgentActionState = AnimAction.Digging,
                 //  AgentAnimationStates = new[] { AnimModifier.Improvised }
             }); 
             #endregion
             #region Build terbium mine   buildRareMetalOrePit2
             //NA MINING CAMP MATERIAL
             listOfProcessTypes.Add(new ProcessType()
             {
                 Name = "Build terbium mine",
                 KeyName = "establishRareMetalOrePit2",
                 JobTypeKey = "constructionJobType",
                 RequiredSkill = "construction",
                 SummaryDescription = "Establish a pit from which we can extract terbium ore",
                 Description = "By digging an extraction site here, we will have access to the rich deposits of terbium ore beneath the surface. (When the terbium mine is established, terbium can then be ordered from the Production Manager)",
                 PhysicalWorkFactor = constructionExertion,
                 Stances = constructionStances,
                 Inputs = new[]
                    {
                        new Input(){ Entity = "item:sticks", IsConsumed = false, Amount = new InputAmount(){ NoOfItems = 1 }},
                    },
                 Outputs = new[] 
                    {
                         new Output() { EntityTypeToCreate = "structure:rareMetalOrePit2", Amount = new OutputAmount(){  NoOfItems = 1 } }
                    },
                 WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeToCreatePit },
                 ProcessToolSetKey = "toolSetDiggingConstruction",
                 AgentActionState = AnimAction.Digging,
                 //  AgentAnimationStates = new[] { AnimModifier.Improvised }
             }); 
             #endregion
             

             #region buildPeatBank
             listOfProcessTypes.Add(new ProcessType()
             {
                 Name = "Build peat bank",
                 KeyName = "establishPeatBank",
                 JobTypeKey = "constructionJobType",
                 RequiredSkill = "menial",
                 SummaryDescription = "Establish a digging site where we can cut peat",
                 Description = "This area is well suited for making a trench where we can cut peat. (When the peat bank is established, peat can then be ordered from the Production Manager)",
                 PhysicalWorkFactor = constructionExertion,
                 Stances = standingProduction,//copied from establish plot since this is also firegrass turf
                 Inputs = new[]
                    {
                        new Input(){ Entity = "item:sticks", IsConsumed = false, Amount = new InputAmount(){ NoOfItems = 1 }},
                    },
                 Outputs = new[] 
                    {
                         new Output() { EntityTypeToCreate = "structure:peatBank", Amount = new OutputAmount(){  NoOfItems = 1 } }
                    },
                 WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeToCreatePit },
                 ProcessToolSetKey = "toolSetPlowingTools",//copied from establish plot since this is also firegrass turf
                 AgentActionState = AnimAction.Tilling,//copied from establish plot since this is also firegrass turf
                 
             });

             #endregion


             string cannotCancelFertilizeReason = "Fertilizing cannot be cancelled. To stop using fertilizer, use the 'Stop fertilizing' action on the farm plot or greenhouse";
            string cannotCancelWeedingReason = "Weeding cannot be cancelled. To stop growing crops, use the 'Stop growing' action on the farm plot or greenhouse";
            string cannotCancelPlantingReason = "Planting cannot be cancelled. To stop growing crops, use the 'Stop growing' action on the farm plot or greenhouse";

                // TB Farming 19.03.2015
                #region establishPlot
                #region establishSmallPlot
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = "Establish farm plot", //not room for long text //mp this is a special action shown on the special action tooltip (next to BEGIN) also shown in task manager (without an added type name) and tv screen. Should be written in imperative (NOT ing- form)
                    KeyName = "establishSmallPlot",
                    JobTypeKey = "constructionJobType",
                    SummaryDescription = "Establish a small farm plot",
                    Description = "At this location we could establish a small farm plot and grow our own crops.",
                    PhysicalWorkFactor = AllGameData.ProcessLoader.moderateWork,
                    Stances = standingProduction,
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = AllGameData.ProcessLoader.timeToBuildSmallPlot },//.timeForInstantCraftingTask   only for debugging
                    ProcessToolSetKey = "toolSetPlowingTools",
                    AgentActionState = AnimAction.Tilling,
                    RequiredSkill = "farming",
                    TierOrArea = new Policies.TierOrArea() { Tier = "basic", Area = RatingTypes.Food } // "foodBasic"
                });
                    #endregion
                    #region establishLargePlot
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = "Establish farm plot", //mp this is a special action shown on the special action tooltip (next to BEGIN) also shown in task manager (without an added type name) and tv screen. Should be written in imperative (NOT ing- form)
                    KeyName = "establishLargePlot",
                    JobTypeKey = "constructionJobType",
                    SummaryDescription = "Establish a large farm plot",
                    Description = "At this location we could establish a large farm plot and grow our own crops.",
                    PhysicalWorkFactor = AllGameData.ProcessLoader.moderateWork,
                    Stances = standingProduction,
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = AllGameData.ProcessLoader.timeToBuildLargePlot },//.timeToBuildLargePlot only for debugging
                    ProcessToolSetKey = "toolSetPlowingTools",
                    AgentActionState = AnimAction.Tilling,
                    RequiredSkill = "farming",
                    TierOrArea = new Policies.TierOrArea() { Tier = "basic", Area = RatingTypes.Food } //TierOrArea = "foodBasic"
                });
                    #endregion
                #endregion
                #region plant seeds
                string cottonName = "Plant cotton";
                string cottonSummary = "Till the soil and plant cotton";
                string cottonDescription = "This plant should give a decent yield provided the farm plot is weeded regularly during the growth period. Once the cotton plant is fully grown the cotton should be picked without delay, as the plant and crops will wither in freezing temperatures.";
                    #region plant plantGlassyCreeperPodsInSmallPlot
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = "Sow Glassy creeper pods", //shown in task manager (without an added type name) and tv screen. Should be written in imperative (NOT ing- form)
                    KeyName = "plantGlassyCreeperPodsInSmallPlot",
                    JobTypeKey = "sowingAndHarvestingJobType",
                    UserCannotCancelReason = cannotCancelPlantingReason,
                    UserCanCancel = false,
                    SummaryDescription = "Till the soil and sow Glassy creeper",
                    Description = "This plant should give a decent yield provided the farm plot is weeded regularly during the growth period. Once the Glassy creeper plant is fully grown the pods should be harvested without delay, as the plant and crops will wither quickly. A new batch of glassy creeper plants can be started immediately after harvesting the previous one, by tilling and sowing again.",
                    PhysicalWorkFactor = moderateWork,
                    Stances = kneelingProduction,
                    Inputs = new[] { new Input() { Entity = "item:glassyCreeperPods", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1 } } },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallHarvestingTask },
                    ProcessToolSetKey = "toolSetSowingWeedingFertilizingFarmPlot",//mp was "plowingTools" but they should prefer the hoe for this. because the soil is not so hard here.
                    AgentActionState = AnimAction.Tilling, //mp i'm using this anim , it includes some tilling and some picking around in the dirt anim
                    RequiredSkill = "farming"
                });
                    #endregion
                    #region plant plantCrystalBerriesInSmallPlot
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = "Sow Crystal berries", ////shown in task manager (without an added type name) and tv screen. Should be written in imperative (NOT ing- form)
                    KeyName = "plantCrystalBerriesInSmallPlot",
                    JobTypeKey = "sowingAndHarvestingJobType",
                    UserCannotCancelReason = cannotCancelPlantingReason,
                    UserCanCancel = false,
                    SummaryDescription = "Till the soil and sow Crystal berries",
                    Description = "This plant should give a decent yield provided the farm plot is weeded regularly during the growth period. Once the Crystal berry shrub is fully grown the berries should be harvested without delay, as the plant and crops will wither quickly. A new batch of crystal berries can be started immediately after harvesting the previous one, by tilling and sowing again.",
                    PhysicalWorkFactor = moderateWork,
                    Stances = kneelingProduction,
                    Inputs = new[] { new Input() { Entity = "item:crystalBerries", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1 } } },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallHarvestingTask },
                    ProcessToolSetKey = "toolSetSowingWeedingFertilizingFarmPlot",//mp was "plowingTools" but they should prefer the hoe for this. because the soil is not so hard here.
                    AgentActionState = AnimAction.Tilling,
                    RequiredSkill = "farming"
                });
                    #endregion
                #region plant plantCottonInSmallPlot
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = cottonName,
                    KeyName = "plantCottonInSmallPlot",
                    JobTypeKey = "sowingAndHarvestingJobType",
                    UserCannotCancelReason = cannotCancelPlantingReason,
                    UserCanCancel = false,
                    SummaryDescription = cottonSummary,
                    Description = cottonDescription,
                    PhysicalWorkFactor = moderateWork,
                    Stances = kneelingProduction,
                    Inputs = new[] { new Input() { Entity = "item:cotton", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1 } } },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallHarvestingTask },
                    ProcessToolSetKey = "toolSetSowingWeedingFertilizingFarmPlot",
                    AgentActionState = AnimAction.Tilling,
                    RequiredSkill = "farming"
                });
                #endregion
                    #region plant plantGlassyCreeperPodsInLargePlot
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = "Sow Glassy creeper pods", //shown in task manager (without an added type name) and tv screen. Should be written in imperative (NOT ing- form)
                    KeyName = "plantGlassyCreeperPodsInLargePlot",
                    JobTypeKey = "sowingAndHarvestingJobType",
                    UserCannotCancelReason = cannotCancelPlantingReason,
                    UserCanCancel = false,
                    SummaryDescription = "Till the soil and sow glassy creeper",
                    Description = "This plant should give a decent yield provided the farm plot is weeded regularly during the growth period. Once the Glassy creeper plant is fully grown the pods should be harvested without delay, as the plant and crops will wither quickly. A new batch of glassy creeper plants can be started immediately after harvesting the previous one, by tilling and sowing again.",
                    PhysicalWorkFactor = moderateWork,
                    Stances = kneelingProduction,
                    Inputs = new[] { new Input() { Entity = "item:glassyCreeperPods", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 2 } } },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumHarvestingTask },
                    ProcessToolSetKey = "toolSetSowingWeedingFertilizingFarmPlot",//mp was "plowingTools" but they should prefer the hoe for this. because the soil is not so hard here.
                    AgentActionState = AnimAction.Tilling,
                    RequiredSkill = "farming"
                });
                    #endregion
                    #region plant plantCrystalBerriesInLargePlot
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = "Sow Crystal berries", ////shown in task manager (without an added type name) and tv screen. Should be written in imperative (NOT ing- form)
                    KeyName = "plantCrystalBerriesInLargePlot",
                    JobTypeKey = "sowingAndHarvestingJobType",
                    UserCannotCancelReason = cannotCancelPlantingReason,
                    UserCanCancel = false,
                    SummaryDescription = "Till the soil and sow crystal berries",
                    Description = "This plant should give a decent yield provided the farm plot is weeded regularly during the growth period. Once the Crystal berry shrub is fully grown the berries should be harvested without delay, as the plant and crops will wither quickly. A new batch of crystal berries can be started immediately after harvesting the previous one, by tilling and sowing again.",
                    PhysicalWorkFactor = moderateWork,
                    Stances = kneelingProduction,
                    Inputs = new[] { new Input() { Entity = "item:crystalBerries", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 2 } } },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumHarvestingTask },
                    ProcessToolSetKey = "toolSetSowingWeedingFertilizingFarmPlot",//mp was "plowingTools" but they should prefer the hoe for this. because the soil is not so hard here.
                    AgentActionState = AnimAction.Tilling,
                    RequiredSkill = "farming"
                });
                    #endregion
                #region plant plantCottonInLargePlot
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = cottonName, 
                    KeyName = "plantCottonInLargePlot",
                    JobTypeKey = "sowingAndHarvestingJobType",
                    UserCannotCancelReason = cannotCancelPlantingReason,
                    UserCanCancel = false,
                    SummaryDescription = cottonSummary,
                    Description = cottonDescription,
                    PhysicalWorkFactor = moderateWork,
                    Stances = kneelingProduction,
                    Inputs = new[] { new Input() { Entity = "item:cotton", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 2 } } },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumHarvestingTask },
                    ProcessToolSetKey = "toolSetSowingWeedingFertilizingFarmPlot",
                    AgentActionState = AnimAction.Tilling,
                    RequiredSkill = "farming"
                });
                #endregion
                #region plant plantFingerFruitsInGreenhouse
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = "Sow Finger fruits", ////shown in task manager (without an added type name) and tv screen. Should be written in imperative (NOT ing- form)
                    KeyName = "plantFingerFruitInGreenhouse",
                    JobTypeKey = "sowingAndHarvestingJobType",
                    UserCannotCancelReason = cannotCancelPlantingReason,
                    UserCanCancel = false,
                    SummaryDescription = "Sow finger fruit",//
                    Description = "The finger fruit plant requires a greenhouse and should give a decent yield provided the plot is weeded regularly during the growth period. Once the finger fruits are grown they should be harvested without delay, as the plant and crops will wither quickly. A new batch of finger fruit can be started immediately after harvesting the previous one, by tilling and sowing again.",
                    PhysicalWorkFactor = moderateWork,
                    Stances = kneelingProduction,
                    Inputs = new[] { new Input() { Entity = "item:fingerFruit", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1 } } },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallHarvestingTask },
                    RequiredSkill = "farming",
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                    #endregion
                #region plantGlassyCreeperPodsInGreenhouse
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = "Sow Glassy creeper pods", //shown in task manager (without an added type name) and tv screen. Should be written in imperative (NOT ing- form)
                    KeyName = "plantGlassyCreeperPodsInGreenhouse",
                    JobTypeKey = "sowingAndHarvestingJobType",
                    UserCannotCancelReason = cannotCancelPlantingReason,
                    UserCanCancel = false,
                    SummaryDescription = "Sow Glassy creeper",
                    Description = "This plant should give a decent yield provided the plot is weeded regularly during the growth period. Once the Glassy creeper plant is fully grown the pods should be harvested without delay, as the plant and crops will wither quickly. A new batch of glassy creeper plants can be started immediately after harvesting the previous one, by tilling and sowing again.",
                    PhysicalWorkFactor = moderateWork,
                    Stances = kneelingProduction,
                    Inputs = new[] { new Input() { Entity = "item:glassyCreeperPods", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1 } } },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallHarvestingTask },
                    RequiredSkill = "farming",
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion
                #region plantCrystalBerriesInGreenhouse
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = "Sow Crystal berries", //shown in task manager (without an added type name) and tv screen. Should be written in imperative (NOT ing- form)
                    KeyName = "plantCrystalBerriesInGreenhouse",
                    JobTypeKey = "sowingAndHarvestingJobType",
                    UserCannotCancelReason = cannotCancelPlantingReason,
                    UserCanCancel = false,
                    SummaryDescription = "Sow Crystal berries",//
                    Description = "This plant should give a decent yield provided the plot is weeded regularly during the growth period. Once the Crystal berry shrub is fully grown the berries should be harvested without delay, as the plant and crops will wither quickly. A new batch of crystal berries can be started immediately after harvesting the previous one, by tilling and sowing again.",
                    PhysicalWorkFactor = moderateWork,
                    Stances = kneelingProduction,
                    Inputs = new[] { new Input() { Entity = "item:crystalBerries", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1 } } },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallHarvestingTask },
                    RequiredSkill = "farming",
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion
                #endregion
                #region weed plot
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = "Weeding plot", //not shown as special action, so use -ing verb form
                    KeyName = "weedPlot",
                    JobTypeKey = "weedingAndFertilizingJobType",
                    UserCannotCancelReason = cannotCancelWeedingReason,
                    UserCanCancel = false,
                    PhysicalWorkFactor = moderateWork,
                    Stances = kneelingProduction, 
                    RequiredSkill = "weeding",
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    ProcessToolSetKey = "toolSetSowingWeedingFertilizingFarmPlot",
                    AgentActionState = AnimAction.Tilling,
                   // LabelType = ProcessLabelTypes.Green
                });
                #endregion
                #region weedGreenhouse
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = "Weeding greenhouse",
                    KeyName = "weedGreenhouse",
                    JobTypeKey = "weedingAndFertilizingJobType",
                    UserCannotCancelReason = cannotCancelWeedingReason,
                    UserCanCancel = false,
                    PhysicalWorkFactor = moderateWork,
                    Stances = kneelingProduction,
                    RequiredSkill = "weeding",
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    ProcessToolSetKey = "toolSetGreenhouseHarvest",
                    AgentActionState = AnimAction.Harvesting,
                    AgentAnimationStates = new[] { AnimModifier.Low }
                });
                #endregion
                #region weed large plot
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = "Weeding plot",
                    KeyName = "weedLargePlot",
                    JobTypeKey = "weedingAndFertilizingJobType",
                    UserCannotCancelReason = cannotCancelWeedingReason,
                    UserCanCancel = false,
                    PhysicalWorkFactor = moderateWork,
                    Stances = kneelingProduction,
                    RequiredSkill = "weeding",
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForBigCraftingTask },
                    ProcessToolSetKey = "toolSetSowingWeedingFertilizingFarmPlot",
                    AgentActionState = AnimAction.Tilling
                });
                #endregion
                string useFertilizerDescription = "To ensure a high crop yield, we will fertilize the plot whenever the soil quality drops as long as we have the selected fertilizer available.";
                #region organicFertilizePlot
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = "Fertilize plot", 
                    KeyName = "organicFertilizePlot",
                    JobTypeKey = "weedingAndFertilizingJobType",
                    UserCannotCancelReason = cannotCancelFertilizeReason,
                    UserCanCancel = false,
                    PhysicalWorkFactor = moderateWork,
                    Stances = kneelingProduction,
                    RequiredSkill = "farming",
                    Inputs = new[]
                    {
                        new Input(){ Entity = "item:organicFertilizer",  IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 4 }},
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    ProcessToolSetKey = "toolSetSowingWeedingFertilizingFarmPlot", 
                    AgentActionState = AnimAction.Tilling
                });
                #endregion
                #region organicFertilizeLargePlot
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = "Fertilize plot", 
                    KeyName = "organicFertilizeLargePlot",
                    JobTypeKey = "weedingAndFertilizingJobType",
                    UserCannotCancelReason = cannotCancelFertilizeReason,
                    UserCanCancel = false,
                    PhysicalWorkFactor = moderateWork,
                    Stances = kneelingProduction,
                    RequiredSkill = "farming",
                    Inputs = new[] { new Input() { Entity = "item:organicFertilizer", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 8 } } },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForBigCraftingTask },
                    ProcessToolSetKey = "toolSetSowingWeedingFertilizingFarmPlot",
                    AgentActionState = AnimAction.Tilling
                });
                #endregion
                #region guanoFertilizePlot
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = "Fertilize plot", 
                    KeyName = "guanoFertilizePlot",
                    JobTypeKey = "weedingAndFertilizingJobType",
                    UserCannotCancelReason = cannotCancelFertilizeReason,
                    UserCanCancel = false,
                    PhysicalWorkFactor = moderateWork,
                    Stances = kneelingProduction,
                    RequiredSkill = "farming",
                    Inputs = new[]
                    {
                        new Input(){ Entity = "item:guanoFertilizer",  IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 1 }},
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    ProcessToolSetKey = "toolSetSowingWeedingFertilizingFarmPlot",
                    AgentActionState = AnimAction.Tilling
                });
                #endregion
                #region guanoFertilizeLargePlot
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = "Fertilize plot", 
                    KeyName = "guanoFertilizeLargePlot",
                    JobTypeKey = "weedingAndFertilizingJobType",
                    UserCannotCancelReason = cannotCancelFertilizeReason,
                    UserCanCancel = false,
                    PhysicalWorkFactor = moderateWork,
                    Stances = kneelingProduction,
                    RequiredSkill = "farming",
                    Inputs = new[] { new Input() { Entity = "item:guanoFertilizer", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 2 } } },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForBigCraftingTask },
                    ProcessToolSetKey = "toolSetSowingWeedingFertilizingFarmPlot",
                    AgentActionState = AnimAction.Tilling
                });
                #endregion

                int compostOrder = 110;
                int guanoOrder = 120;

                #region useOrganicFertilizer
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = "Use compost fertilizer", //mp this is a special action shown on the special action tooltip (next to BEGIN) also shown in task manager (without an added type name) and tv screen. Should be written in imperative (NOT ing- form)
                    KeyName = "useOrganicFertilizer",
                    JobTypeKey = "weedingAndFertilizingJobType",
                    UserCanCancel = false,
                    SummaryDescription = "The type of fertilizer to use once the plot needs fertilizing",
                    Description = "We will need to use 4 bags of compost for fertilizing this small farm plot when the soil is low on nutrients." + useFertilizerDescription,
                    PhysicalWorkFactor = 0, //shouldn't use any energy for changing a property
                    WorkNeeded = WorkerNeededOptions.StartRemotely, 
                    IsMetaAction = true,
                    SortOrder = compostOrder,
                    //TogglesSpecialActionProcess = "stopUsingOrganicFertilizer",
                    DisablesSpecialActionLockProcesses = new[] { "useOrganicFertilizer", "stopUsingGuanoFertilizer" },
                    EnablesSpecialActionLockProcesses = new[] { "stopUsingOrganicFertilizer", "useGuanoFertilizer" },
                    Stances = kneelingProduction,
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 0 },
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion
                #region useLargeOrganicFertilizer
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = "Use compost fertilizer",
                    KeyName = "useLargeOrganicFertilizer",
                    JobTypeKey = "weedingAndFertilizingJobType",
                    UserCanCancel = false,
                    SummaryDescription = "The type of fertilizer to use once the plot needs fertilizing",
                    Description = "We will need to use 8 bags of compost for fertilizing this large farm plot when the soil is low on nutrients" + useFertilizerDescription,
                    PhysicalWorkFactor = 0, //shouldn't use any energy for changing a property
                    WorkNeeded = WorkerNeededOptions.StartRemotely,
                    IsMetaAction = true,
                    SortOrder = compostOrder,
                    //TogglesSpecialActionProcess = "stopUsingLargeOrganicFertilizer",
                    DisablesSpecialActionLockProcesses = new[] { "useLargeOrganicFertilizer", "stopUsingLargeGuanoFertilizer" },
                    EnablesSpecialActionLockProcesses = new[] { "stopUsingLargeOrganicFertilizer", "useLargeGuanoFertilizer" },
                    Stances = kneelingProduction,
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 0 },
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion
                #region useGuanoFertilizer
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = "Use guano fertilizer",
                    KeyName = "useGuanoFertilizer",
                    JobTypeKey = "weedingAndFertilizingJobType",
                    UserCanCancel = false,
                    SummaryDescription = "The type of fertilizer to use once the plot needs fertilizing",
                    Description = "We will need to use 1 bag of guano fertilizer for fertilizing this small farm plot when the soil is low on nutrients" + useFertilizerDescription,
                    PhysicalWorkFactor = 0, //shouldn't use any energy for changing a property
                    WorkNeeded = WorkerNeededOptions.StartRemotely,
                    IsMetaAction = true,
                    SortOrder = guanoOrder,
                   // TogglesSpecialActionProcess = "stopUsingGuanoFertilizer",
                    DisablesSpecialActionLockProcesses = new[] { "useGuanoFertilizer", "stopUsingOrganicFertilizer" },
                    EnablesSpecialActionLockProcesses = new[] { "stopUsingGuanoFertilizer", "useOrganicFertilizer" },
                    Stances = kneelingProduction,
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 0 },
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion
                #region useLargeGuanoFertilizer
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = "Use guano fertilizer",
                    KeyName = "useLargeGuanoFertilizer",
                    JobTypeKey = "weedingAndFertilizingJobType",
                    UserCanCancel = false,
                    SummaryDescription = "The type of fertilizer to use once the plot needs fertilizing",
                    Description = "We will need to use 2 bags of guano fertilizer for fertilizing this large farm plot when the soil is low on nutrients" + useFertilizerDescription,
                    PhysicalWorkFactor = 0, //shouldn't use any energy for changing a property
                    WorkNeeded = WorkerNeededOptions.StartRemotely,
                    IsMetaAction = true,
                    SortOrder = 100,
                  //  TogglesSpecialActionProcess = "stopUsingLargeGuanoFertilizer", 
                    DisablesSpecialActionLockProcesses = new[] { "useLargeGuanoFertilizer", "stopUsingLargeOrganicFertilizer" },
                    EnablesSpecialActionLockProcesses = new[] { "stopUsingLargeGuanoFertilizer", "useLargeOrganicFertilizer" },
                    Stances = kneelingProduction,
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 0 },
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                string stopUsingGuanoFertilizerName = "Stop using guano fertilizer";
                string stopUsingOrganicFertilizerName = "Stop using compost fertilizer";

                string stopUsingFertilizerCaption = "SELECT";

                #region stopUsingLargeGuanoFertilizer
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = stopUsingGuanoFertilizerName,
                    KeyName = "stopUsingLargeGuanoFertilizer",
                    JobTypeKey = "weedingAndFertilizingJobType",
                    UserCanCancel = false,
                    SpecialActionCaption = stopUsingFertilizerCaption,
                    SummaryDescription = "",
                    Description = "",
                    PhysicalWorkFactor = 0,  
                    WorkNeeded = WorkerNeededOptions.StartRemotely,     
                    IsMetaAction = true,
                    SortOrder = guanoOrder,
                    Stances = kneelingProduction,
                    EnablesSpecialActionLockProcesses = new[] { "useLargeGuanoFertilizer" },
                    DisablesSpecialActionLockProcesses = new[] { "stopUsingLargeGuanoFertilizer" }, 
                  //  TogglesSpecialActionProcess = "useLargeGuanoFertilizer",
                    ShowDisabledSpecialAction = false,// hide from menu until needed
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 0 },
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region stopUsingLargeOrganicFertilizer
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = stopUsingOrganicFertilizerName,
                    KeyName = "stopUsingLargeOrganicFertilizer",
                    JobTypeKey = "weedingAndFertilizingJobType",
                    UserCanCancel = false,
                    SpecialActionCaption = stopUsingFertilizerCaption,
                    SummaryDescription = "",
                    Description = "",
                    PhysicalWorkFactor = 0,
                    WorkNeeded = WorkerNeededOptions.StartRemotely,
                    IsMetaAction = true,
                    SortOrder = compostOrder,
                    Stances = kneelingProduction,
                    EnablesSpecialActionLockProcesses = new[] { "useLargeOrganicFertilizer" },
                    DisablesSpecialActionLockProcesses = new[] { "stopUsingLargeOrganicFertilizer" }, 
                   // TogglesSpecialActionProcess = "useLargeOrganicFertilizer",
                    ShowDisabledSpecialAction = false,// hide from menu until needed
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 0 },
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region stopUsingGuanoFertilizer
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = stopUsingGuanoFertilizerName,
                    KeyName = "stopUsingGuanoFertilizer",
                    JobTypeKey = "weedingAndFertilizingJobType",
                    UserCanCancel = false,
                    SpecialActionCaption = stopUsingFertilizerCaption,
                    SummaryDescription = "",
                    Description = "",
                    PhysicalWorkFactor = 0,
                    WorkNeeded = WorkerNeededOptions.StartRemotely,
                    IsMetaAction = true,
                    SortOrder = guanoOrder,
                    Stances = kneelingProduction,
                    EnablesSpecialActionLockProcesses = new[] { "useGuanoFertilizer" },
                    DisablesSpecialActionLockProcesses = new[] { "stopUsingGuanoFertilizer" }, 
                   // TogglesSpecialActionProcess = "useGuanoFertilizer",
                    ShowDisabledSpecialAction = false,// hide from menu until needed
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 0 },
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region stopUsingOrganicFertilizer
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = stopUsingOrganicFertilizerName,
                    KeyName = "stopUsingOrganicFertilizer",
                    JobTypeKey = "weedingAndFertilizingJobType",
                    UserCanCancel = false,
                    SpecialActionCaption = stopUsingFertilizerCaption,
                    SummaryDescription = "",
                    Description = "",
                    PhysicalWorkFactor = 0,
                    WorkNeeded = WorkerNeededOptions.StartRemotely,
                    IsMetaAction = true,
                    SortOrder = compostOrder,
                    Stances = kneelingProduction,
                    EnablesSpecialActionLockProcesses = new[] { "useOrganicFertilizer" },
                    DisablesSpecialActionLockProcesses = new[] { "stopUsingOrganicFertilizer" },
                  //  TogglesSpecialActionProcess = "useOrganicFertilizer",
                    ShowDisabledSpecialAction = false,// hide from menu until needed
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 0 },
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region Growing processes
                string stopGrowingGlassyName = "Stop growing glassy creeper pods";
                string stopGrowingCrystalName = "Stop growing crystal berries";
                string stopGrowingCottonName = "Stop growing cotton";
                string stopGrowingFingerName = "Stop growing finger fruit";
                string stopGrowingSummaryDescription = "Stops tending the current crops. NOTE: to replant with different crop, first clear away the farm plot";
                string stopGrowingDescription = "After stopping, we can resume tending the current crops at any point. But if we want to re-plant with a different crop immediately, we have to clear away the plot, then re-establish the plot, then plant the new crop.";

                string growGlassyName = "Grow glassy creeper pods";
                string growGlassyDescription = "The crops will be planted, tended and harvested in a continuous cycle until an order is given to stop.";
         //       string growGlassySummary = "Use this plot for growing glassy creeper pods";

                string growCrystalName = "Grow crystal berries";
                string growCrystalDescription = "The crops will be planted, tended and harvested in a continuous cycle until an order is given to stop.";
         //       string growCrystalSummary = "Use this plot for growing crystal berries";

                string growCottonName = "Grow cotton";
                string growCottonDescription = "The crops will be planted, tended and harvested in a continuous cycle until an order is given to stop.";
           //     string growCottonSummary = "Use this plot for growing cotton";
             
                string growFingerFruitName = "Grow finger fruit";
                string growFingerFruitDescription = "The finger fruit plant requires a greenhouse and should give a decent yield provided the plot is weeded regularly during the growth period. The crops will be planted, tended and harvested in a continuous cycle until an order is given to stop.";
          //      string growFingerFruitSummary = "Use this greenhouse for growing finger fruit";

                int crystalBerryOrder = 5;
                int glassyCreeperOrder = 15;

                string stopGrowingCaption = "SELECT";

                #region growCrystalBerriesInLargePlot
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = growCrystalName,
                    KeyName = "growCrystalBerriesInLargePlot",
                    JobTypeKey = "sowingAndHarvestingJobType",
                    UserCanCancel = false,
                    SummaryDescription = "Use this plot for growing crystal berries. 2 crystal berries are needed for sowing.",
                    Description = "We need 2 crystal berry items to start growing on this large plot. " + growCrystalDescription,
                    PhysicalWorkFactor = 0, //shouldn't use any energy for changing a property
                    WorkNeeded = WorkerNeededOptions.StartRemotely, // we don't need to go there!     
                    IsMetaAction = true,
                    SortOrder = crystalBerryOrder,
                    Stances = kneelingProduction,
               //     TogglesSpecialActionProcess = "stopGrowingCrystalBerriesInLargePlot",
                    DisablesSpecialActionLockProcesses = new[] { "growCrystalBerriesInLargePlot", "stopGrowingGlassyCreeperPodsInLargePlot", "stopGrowingCottonInLargePlot" },
                    EnablesSpecialActionLockProcesses = new[] { "stopGrowingCrystalBerriesInLargePlot", "growGlassyCreeperPodsInLargePlot", "growCottonInLargePlot" },                               
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 0 },
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion
              
                #region stopGrowingCrystalBerriesInLargePlot
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = stopGrowingCrystalName,
                    KeyName = "stopGrowingCrystalBerriesInLargePlot",
                    JobTypeKey = "sowingAndHarvestingJobType",
                    UserCanCancel = false,
                    SpecialActionCaption = stopGrowingCaption,
                    SummaryDescription = stopGrowingSummaryDescription,
                    Description = stopGrowingDescription,
                    PhysicalWorkFactor = 0, //shouldn't use any energy for changing a property     
                    WorkNeeded = WorkerNeededOptions.StartRemotely, // we don't need to go there!       
                    IsMetaAction = true,
                    SortOrder = crystalBerryOrder,
                    Stances = kneelingProduction,
                    EnablesSpecialActionLockProcesses = new[] { "growCrystalBerriesInLargePlot" },
                    DisablesSpecialActionLockProcesses = new[] { "stopGrowingCrystalBerriesInLargePlot" },
                   // TogglesSpecialActionProcess = "growCrystalBerriesInLargePlot",
                    ShowDisabledSpecialAction = false,// hide from menu until needed
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 0 },
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region growGlassyCreeperPodsInLargePlot
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = growGlassyName,
                    KeyName = "growGlassyCreeperPodsInLargePlot",
                    JobTypeKey = "sowingAndHarvestingJobType",
                    UserCanCancel = false,
                    SummaryDescription = "Use this plot for growing glassy creeper pods. 2 glassy pods are needed for sowing.",
                    Description = "We need 2 glassy creeper pods to start growing on this large plot. " + growGlassyDescription,
                    PhysicalWorkFactor = 0, //shouldn't use any energy for changing a property
                    WorkNeeded = WorkerNeededOptions.StartRemotely, // we don't need to go there! 
                    IsMetaAction = true,
                    SortOrder = glassyCreeperOrder,
                    Stances = kneelingProduction,
                   // TogglesSpecialActionProcess = "stopGrowingGlassyCreeperPodsInLargePlot",
                    DisablesSpecialActionLockProcesses = new[] { "growGlassyCreeperPodsInLargePlot", "stopGrowingCrystalBerriesInLargePlot", "stopGrowingCottonInLargePlot" },
                    EnablesSpecialActionLockProcesses = new[] { "stopGrowingGlassyCreeperPodsInLargePlot", "growCrystalBerriesInLargePlot", "growCottonInLargePlot" },                
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 0 },
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region stopGrowingGlassyCreeperPodsInLargePlot
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = stopGrowingGlassyName,
                    KeyName = "stopGrowingGlassyCreeperPodsInLargePlot",
                    JobTypeKey = "sowingAndHarvestingJobType",
                    UserCanCancel = false,
                    SpecialActionCaption = stopGrowingCaption,
                    SummaryDescription = stopGrowingSummaryDescription,
                    Description = stopGrowingDescription,
                    PhysicalWorkFactor = 0,         
                    Stances = kneelingProduction,
                    EnablesSpecialActionLockProcesses = new[] { "growGlassyCreeperPodsInLargePlot" },
                    DisablesSpecialActionLockProcesses = new[] { "stopGrowingGlassyCreeperPodsInLargePlot" },             
                  //  TogglesSpecialActionProcess = "growGlassyCreeperPodsInLargePlot",
                    WorkNeeded = WorkerNeededOptions.StartRemotely, // we don't need to go there! 
                    IsMetaAction = true,
                    SortOrder = glassyCreeperOrder,
                    ShowDisabledSpecialAction = false,// hide from menu until needed
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 0 },
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region growCottonInLargePlot
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = growCottonName,
                    KeyName = "growCottonInLargePlot",
                    JobTypeKey = "sowingAndHarvestingJobType",
                    UserCanCancel = false,
                    SummaryDescription = "Use this plot for growing cotton. 2 cotton are needed for sowing.",
                    Description = "We need 2 cotton items to start growing on this large plot. " + growCottonDescription,
                    PhysicalWorkFactor = 0, //shouldn't use any energy for changing a property
                    WorkNeeded = WorkerNeededOptions.StartRemotely, // we don't need to go there! 
                    IsMetaAction = true,
                    SortOrder = 20,
                    Stances = kneelingProduction,
                  //  TogglesSpecialActionProcess = "stopGrowingCottonInLargePlot",
                    DisablesSpecialActionLockProcesses = new[] { "growCottonInLargePlot", "stopGrowingCrystalBerriesInLargePlot", "stopGrowingGlassyCreeperPodsInLargePlot" },
                    EnablesSpecialActionLockProcesses = new[] { "stopGrowingCottonInLargePlot", "growCrystalBerriesInLargePlot", "growGlassyCreeperPodsInLargePlot" },
                
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 0 },
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region stopGrowingCottonInLargePlot
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = stopGrowingCottonName,
                    KeyName = "stopGrowingCottonInLargePlot",
                    JobTypeKey = "sowingAndHarvestingJobType",
                    UserCanCancel = false,
                    SpecialActionCaption = stopGrowingCaption,
                    SummaryDescription = stopGrowingSummaryDescription,
                    Description = stopGrowingDescription,
                    PhysicalWorkFactor = 0,
                    WorkNeeded = WorkerNeededOptions.StartRemotely, // we don't need to go there! 
                    IsMetaAction = true,
                    SortOrder = 20,
                    Stances = kneelingProduction,
                    EnablesSpecialActionLockProcesses = new[] { "growCottonInLargePlot" },
                    DisablesSpecialActionLockProcesses = new[] { "stopGrowingCottonInLargePlot" },     
                  //  TogglesSpecialActionProcess = "growCottonInLargePlot",
                    ShowDisabledSpecialAction = false,// hide from menu until needed
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 0 },
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                //**** small plot
                #region growCrystalBerriesInSmallPlot
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = "Grow crystal berries",
                    KeyName = "growCrystalBerriesInSmallPlot",
                    JobTypeKey = "sowingAndHarvestingJobType",
                    UserCanCancel = false,
                    SummaryDescription = "Use this plot for growing crystal berries. 1 crystal berry is needed for sowing.",
                    Description = "We need 1 crystal berry item to start growing on this small plot. " + growCrystalDescription,
                    PhysicalWorkFactor = 0, //shouldn't use any energy for changing a property
                    WorkNeeded = WorkerNeededOptions.StartRemotely, // we don't need to go there! 
                    IsMetaAction = true,
                    SortOrder = crystalBerryOrder,
                    Stances = kneelingProduction,
                    //TogglesSpecialActionProcess = "stopGrowingCrystalBerriesInSmallPlot",
                    DisablesSpecialActionLockProcesses = new[] { "growCrystalBerriesInSmallPlot", "stopGrowingCottonInSmallPlot", "stopGrowingGlassyCreeperPodsInSmallPlot" },
                    EnablesSpecialActionLockProcesses = new[] { "stopGrowingCrystalBerriesInSmallPlot", "growGlassyCreeperPodsInSmallPlot", "growCottonInSmallPlot", },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 0 },
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region stopGrowingCrystalBerriesInSmallPlot
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = stopGrowingCrystalName,
                    KeyName = "stopGrowingCrystalBerriesInSmallPlot",
                    JobTypeKey = "sowingAndHarvestingJobType",
                    UserCanCancel = false,
                    SpecialActionCaption = stopGrowingCaption,
                    SummaryDescription = stopGrowingSummaryDescription,
                    Description = stopGrowingDescription,
                    PhysicalWorkFactor = 0, //shouldn't use any energy for changing a property  
                    WorkNeeded = WorkerNeededOptions.StartRemotely, // we don't need to go there! 
                    IsMetaAction = true,
                    SortOrder = crystalBerryOrder,
                    Stances = kneelingProduction,
                    EnablesSpecialActionLockProcesses = new[] { "growCrystalBerriesInSmallPlot" },
                    DisablesSpecialActionLockProcesses = new[] { "stopGrowingCrystalBerriesInSmallPlot" },               
                   // TogglesSpecialActionProcess = "growCrystalBerriesInSmallPlot",
                    ShowDisabledSpecialAction = false,// hide from menu until needed
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 0 },
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region growGlassyCreeperPodsInSmallPlot
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = growGlassyName,
                    KeyName = "growGlassyCreeperPodsInSmallPlot",
                    JobTypeKey = "sowingAndHarvestingJobType",
                    UserCanCancel = false,
                    SummaryDescription = "Use this plot for growing glassy creeper pods. 1 glassy creeper is needed for sowing.",
                    Description = "We need 1 glassy creeper pod to start growing on this small plot. " + growGlassyDescription,
                    PhysicalWorkFactor = 0, //shouldn't use any energy for changing a property
                    WorkNeeded = WorkerNeededOptions.StartRemotely, // we don't need to go there! 
                    IsMetaAction = true,
                    SortOrder = glassyCreeperOrder,
                    Stances = kneelingProduction,
                  //  TogglesSpecialActionProcess = "stopGrowingGlassyCreeperPodsInSmallPlot",
                    DisablesSpecialActionLockProcesses = new[] { "growGlassyCreeperPodsInSmallPlot", "stopGrowingCottonInSmallPlot", "stopGrowingCrystalBerriesInSmallPlot" },
                    EnablesSpecialActionLockProcesses = new[] { "stopGrowingGlassyCreeperPodsInSmallPlot", "growCrystalBerriesInSmallPlot", "growCottonInSmallPlot", },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 0 },
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region stopGrowingGlassyCreeperPodsInSmallPlot
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = stopGrowingGlassyName,
                    KeyName = "stopGrowingGlassyCreeperPodsInSmallPlot",
                    JobTypeKey = "sowingAndHarvestingJobType",
                    UserCanCancel = false,
                    SpecialActionCaption = stopGrowingCaption,
                    SummaryDescription = stopGrowingSummaryDescription,
                    Description = stopGrowingDescription,
                    PhysicalWorkFactor = 0,
                    WorkNeeded = WorkerNeededOptions.StartRemotely, // we don't need to go there! 
                    IsMetaAction = true,
                    SortOrder = glassyCreeperOrder,
                    Stances = kneelingProduction,
                    EnablesSpecialActionLockProcesses = new[]{ "growGlassyCreeperPodsInSmallPlot" },
                    DisablesSpecialActionLockProcesses = new[] { "stopGrowingGlassyCreeperPodsInSmallPlot" },
                   // TogglesSpecialActionProcess = "growGlassyCreeperPodsInSmallPlot",
                    ShowDisabledSpecialAction = false,// hide from menu until needed
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 0 },
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region growCottonInSmallPlot
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = growCottonName,
                    KeyName = "growCottonInSmallPlot",
                    JobTypeKey = "sowingAndHarvestingJobType",
                    UserCanCancel = false,
                    SummaryDescription = "Use this plot for growing cotton. 1 cotton is needed for sowing.",
                    Description = "We need 1 cotton item to start growing on this small plot. " + growCottonDescription,
                    PhysicalWorkFactor = 0, //shouldn't use any energy for changing a property
                    WorkNeeded = WorkerNeededOptions.StartRemotely, // we don't need to go there! 
                    IsMetaAction = true,
                    SortOrder = 20,
                    Stances = kneelingProduction,
                    //TogglesSpecialActionProcess = "stopGrowingCottonInSmallPlot",
                    DisablesSpecialActionLockProcesses = new[] { "growCottonInSmallPlot", "stopGrowingGlassyCreeperPodsInSmallPlot", "stopGrowingCrystalBerriesInSmallPlot" },
                    EnablesSpecialActionLockProcesses = new[] { "stopGrowingCottonInSmallPlot", "growCrystalBerriesInSmallPlot", "growGlassyCreeperPodsInSmallPlot" },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 0 },
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region stopGrowingCottonInSmallPlot
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = stopGrowingCottonName,
                    KeyName = "stopGrowingCottonInSmallPlot",
                    JobTypeKey = "sowingAndHarvestingJobType",
                    UserCanCancel = false,
                    SpecialActionCaption = stopGrowingCaption,
                    SummaryDescription = stopGrowingSummaryDescription,
                    Description = stopGrowingDescription,
                    PhysicalWorkFactor = 0,
                    WorkNeeded = WorkerNeededOptions.StartRemotely, // we don't need to go there! 
                    IsMetaAction = true,
                    SortOrder = 20,
                    Stances = kneelingProduction,
                    EnablesSpecialActionLockProcesses = new[] { "growCottonInSmallPlot" },
                    DisablesSpecialActionLockProcesses = new[] { "stopGrowingCottonInSmallPlot" },   
                  //  TogglesSpecialActionProcess = "growCottonInSmallPlot",
                    ShowDisabledSpecialAction = false,// hide from menu until needed
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 0 },
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion


            // *** greenhouse
             
                #region growCrystalBerriesInGreenhouse
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = "Grow crystal berries",
                    KeyName = "growCrystalBerriesInGreenhouse",
                    JobTypeKey = "sowingAndHarvestingJobType",
                    UserCanCancel = false,
                    SummaryDescription = "Use this greenhouse for growing crystal berries. 1 crystal berry is needed for sowing.",
                    Description = "We need 1 crystal berry item to start growing. " + growCrystalDescription,
                    PhysicalWorkFactor = 0, //shouldn't use any energy for changing a property
                    WorkNeeded = WorkerNeededOptions.StartRemotely, // we don't need to go there! 
                    IsMetaAction = true,
                    SortOrder = crystalBerryOrder,
                    Stances = kneelingProduction,
                  //  TogglesSpecialActionProcess = "stopGrowingCrystalBerriesInGreenhouse",
                    DisablesSpecialActionLockProcesses = new[] { "growCrystalBerriesInGreenhouse", "stopGrowingGlassyCreeperPodsInGreenhouse", "stopGrowingFingerFruitInGreenhouse" },
                    EnablesSpecialActionLockProcesses = new[] { "stopGrowingCrystalBerriesInGreenhouse", "growGlassyCreeperPodsInGreenhouse", "growFingerFruitInGreenhouse" },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 0 },
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region stopGrowingCrystalBerriesInGreenhouse
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = stopGrowingCrystalName,
                    KeyName = "stopGrowingCrystalBerriesInGreenhouse",
                    JobTypeKey = "sowingAndHarvestingJobType",
                    UserCanCancel = false,
                    SpecialActionCaption = stopGrowingCaption,
                    SummaryDescription = stopGrowingSummaryDescription,
                    Description = stopGrowingDescription,
                    PhysicalWorkFactor = 0, //shouldn't use any energy for changing a property      
                    WorkNeeded = WorkerNeededOptions.StartRemotely, // we don't need to go there! 
                    IsMetaAction = true,
                    SortOrder = crystalBerryOrder,
                    Stances = kneelingProduction,
                    EnablesSpecialActionLockProcesses = new[] { "growCrystalBerriesInGreenhouse" },
                    DisablesSpecialActionLockProcesses = new[] { "stopGrowingCrystalBerriesInGreenhouse" },   
                  //  TogglesSpecialActionProcess = "growCrystalBerriesInGreenhouse",                 
                    ShowDisabledSpecialAction = false,// hide from menu until needed
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 0 },
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region growGlassyCreeperPodsInGreenhouse
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = growGlassyName,
                    KeyName = "growGlassyCreeperPodsInGreenhouse",
                    JobTypeKey = "sowingAndHarvestingJobType",
                    UserCanCancel = false,
                    SummaryDescription = "Use this greenhouse for growing glassy creepers. 1 glassy creeper pod is needed for sowing.",
                    Description = "We need 1 glassy creeper pod to start growing. " + growGlassyDescription,
                    PhysicalWorkFactor = 0, //shouldn't use any energy for changing a property
                    WorkNeeded = WorkerNeededOptions.StartRemotely, // we don't need to go there! 
                    IsMetaAction = true,
                    SortOrder = glassyCreeperOrder,
                    Stances = kneelingProduction,
                  //  TogglesSpecialActionProcess = "stopGrowingGlassyCreeperPodsInGreenhouse",
                    DisablesSpecialActionLockProcesses = new[] { "growGlassyCreeperPodsInGreenhouse", "stopGrowingCrystalBerriesInGreenhouse", "stopGrowingFingerFruitInGreenhouse" },
                    EnablesSpecialActionLockProcesses = new[] { "stopGrowingGlassyCreeperPodsInGreenhouse", "growCrystalBerriesInGreenhouse", "growFingerFruitInGreenhouse" },                  
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 0 },
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region stopGrowingGlassyCreeperPodsInGreenhouse
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = stopGrowingGlassyName,
                    KeyName = "stopGrowingGlassyCreeperPodsInGreenhouse",
                    JobTypeKey = "sowingAndHarvestingJobType",
                    UserCanCancel = false,
                    SpecialActionCaption = stopGrowingCaption,
                    SummaryDescription = stopGrowingSummaryDescription,
                    Description = stopGrowingDescription,
                    PhysicalWorkFactor = 0,
                    WorkNeeded = WorkerNeededOptions.StartRemotely,
                    IsMetaAction = true,
                    SortOrder = glassyCreeperOrder,
                    Stances = kneelingProduction,
                    EnablesSpecialActionLockProcesses = new[] { "growGlassyCreeperPodsInGreenhouse" },
                    DisablesSpecialActionLockProcesses = new[] { "stopGrowingGlassyCreeperPodsInGreenhouse" },   
                  //  TogglesSpecialActionProcess = "growGlassyCreeperPodsInGreenhouse",
                    ShowDisabledSpecialAction = false,// hide from menu until needed
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 0 },
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region growFingerFruitInGreenhouse
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = growFingerFruitName,
                    KeyName = "growFingerFruitInGreenhouse",
                    JobTypeKey = "sowingAndHarvestingJobType",
                    UserCanCancel = false,
                    SummaryDescription = "Use this greenhouse for growing finger fruit. 1 finger fruit is needed for sowing.",
                    Description = "We need 1 finger fruit item to start growing this. " + growFingerFruitDescription,
                    PhysicalWorkFactor = 0, //shouldn't use any energy for changing a property
                    WorkNeeded = WorkerNeededOptions.StartRemotely,
                    IsMetaAction = true,
                    SortOrder = 25,
                    Stances = kneelingProduction,
                   // TogglesSpecialActionProcess = "stopGrowingFingerFruitInGreenhouse",
                    DisablesSpecialActionLockProcesses = new[] { "growFingerFruitInGreenhouse", "stopGrowingGlassyCreeperPodsInGreenhouse", "stopGrowingCrystalBerriesInGreenhouse" },
                    EnablesSpecialActionLockProcesses = new[] { "stopGrowingFingerFruitInGreenhouse", "growCrystalBerriesInGreenhouse", "growGlassyCreeperPodsInGreenhouse" },   
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 0 },
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region stopGrowingFingerFruitInGreenhouse
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = stopGrowingFingerName,
                    KeyName = "stopGrowingFingerFruitInGreenhouse",
                    JobTypeKey = "sowingAndHarvestingJobType",
                    UserCanCancel = false,
                    SpecialActionCaption = stopGrowingCaption,
                    SummaryDescription = stopGrowingSummaryDescription,
                    Description = stopGrowingDescription,
                    PhysicalWorkFactor = 0,
                    WorkNeeded = WorkerNeededOptions.StartRemotely,
                    IsMetaAction = true,
                    SortOrder = 25,
                    Stances = kneelingProduction,
                    EnablesSpecialActionLockProcesses = new[] { "growFingerFruitInGreenhouse" },
                    DisablesSpecialActionLockProcesses = new[] { "stopGrowingFingerFruitInGreenhouse" }, 
                //    TogglesSpecialActionProcess = "growFingerFruitInGreenhouse",
                    ShowDisabledSpecialAction = false,// hide from menu until needed
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 0 },
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion


                #endregion


                #region harvest crops
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = "Harvest crops", //mp this is a special action shown on the special action tooltip (next to BEGIN) also shown in task manager (without an added type name) and tv screen. Should be written in imperative (NOT ing- form)
                    KeyName = "harvestCrops",
                    JobTypeKey = "sowingAndHarvestingJobType",
                    UserCanCancel = false,
                    UserCannotCancelReason = "Harvesting cannot be cancelled. To stop growing crops, use the 'Stop growing' action on the farm plot.",
                    SummaryDescription = "Crops can be harvested when they are fully grown. If left too long, they will wither", //mp not necessarily ready to harvest when player looks at tooltip
                    Description = "The window for harvesting varies depending on the crop type. Some crops will decay quickly while some can be left on the plant for longer.",
                    RequiredSkill = "farming",
                    PhysicalWorkFactor = moderateWork,
                    Stances = kneelingProduction,
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallHarvestingTask },
                    ProcessToolSetKey = "toolSetHarvestFieldCrops",
                    AgentActionState = AnimAction.Harvesting
                });
                #endregion
                #region harvestGreenhouseCrops
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = "Harvest crops",
                    KeyName = "harvestGreenhouseCrops",
                    JobTypeKey = "sowingAndHarvestingJobType",
                    UserCanCancel = false,
                    UserCannotCancelReason = "Harvesting cannot be cancelled. To stop growing crops, use the 'Stop growing' action on the greenhouse.",
                    SummaryDescription = "Crops can be harvested when they are fully grown. If left too long, they will wither", //mp not necessarily ready to harvest when player looks at tooltip
                    Description = "The window for harvesting varies, depending on the crop type. Some crops will decay quickly while some can be left on the plant for longer.",
                    RequiredSkill = "farming",
                    PhysicalWorkFactor = moderateWork,
                    Stances = kneelingProduction,
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallHarvestingTask },
                    ProcessToolSetKey = "toolSetGreenhouseHarvest",
                    AgentActionState = AnimAction.Harvesting
                });
                #endregion
                #region harvest large crops
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = "Harvest crops",
                    KeyName = "harvestLargeCrops",
                    JobTypeKey = "sowingAndHarvestingJobType",
                    UserCanCancel = false,
                    UserCannotCancelReason = "Harvesting cannot be cancelled. To stop growing crops, use the 'Stop growing' action on the farm plot.",
                    SummaryDescription = "Crops can be harvested when they are fully grown. If left too long, they will wither", //mp not necessarily ready to harvest when player looks at tooltip
                    Description = "The window for harvesting varies, depending on the crop type. Some crops will decay quickly while some can be left on the plant for longer.",
                    RequiredSkill = "farming",
                    PhysicalWorkFactor = moderateWork,
                    Stances = kneelingProduction,
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumHarvestingTask },
                    ProcessToolSetKey = "toolSetHarvestFieldCrops",
                    AgentActionState = AnimAction.Harvesting
                });
                #endregion
              
                #endregion

                #region Salvage upgrades
#region salvageSimpleStove
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = removingName,
                  //  SummaryDescription = //not needed because not shown anywhere for salvaging upgrade
                    KeyName = "salvageSimpleStove",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = salvageStructure,
                    IsSalvageProcess = true,
                    Stances = kneelingProduction,
                    Inputs = new[] {
                            new Input(){ Entity = "item:simpleStoveUpgrade", Amount = new InputAmount(){  NoOfItems = 1 } }},
                    Outputs = new[] {
                                    new Output() { EntityTypeToCreate = "item:stones", Amount = new OutputAmount() { NoOfItems = 1 } },
                                    new Output() { EntityTypeToCreate = "item:solidMudBrick", Amount = new OutputAmount() { NoOfItems = 1 } }                                  
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    AgentActionState = AnimAction.Mending,
                    AgentAnimationStates = null,

                });
#endregion
                #region salvageCommunityHall
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = removingName,
                    KeyName = "salvageCommunityHall",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = salvageStructure,
                    IsSalvageProcess = true,
                    Stances = GetSalvageStructureStances(),
                    Inputs = new[] {
                            new Input(){ Entity = "item:communityHallUpgrade", Amount = new InputAmount(){  NoOfItems = 1 } }},
                    Outputs = new[] {
                                    new Output() { EntityTypeToCreate = "item:spoakShingles", Amount = new OutputAmount() { NoOfItems = 2 } },
                                    new Output() { EntityTypeToCreate = "item:solidMudBrick", Amount = new OutputAmount() { NoOfItems = 2 } },
                                    new Output() { EntityTypeToCreate = "item:spoakBranchesTrimmed", Amount = new OutputAmount() { NoOfItems = 1 } }                                  
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    AgentActionState = AnimAction.Salvaging,
                    AgentAnimationStates = null,

                });
#endregion

                #region salvageSmokeOvenUpgrade
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = removingName,
                    KeyName = "salvageSmokeOvenUpgrade",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = salvageStructure,
                    IsSalvageProcess = true,
                    Stances = GetSalvageStructureStances(),
                    Inputs = new[] {
                            new Input(){ Entity = "item:smokeOvenUpgrade", Amount = new InputAmount(){  NoOfItems = 1 } }},
                    Outputs = new[] {
                                    new Output() { EntityTypeToCreate = "item:solidMudBrick", Amount = new OutputAmount() { NoOfItems = 1 } },                                  
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    AgentActionState = AnimAction.Salvaging,
                    AgentAnimationStates = null,

                });
                #endregion

                #region salvageDryingShedUpgrade
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = removingName,
                    KeyName = "salvageDryingShedUpgrade",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = salvageStructure,
                    IsSalvageProcess = true,
                    Stances = GetSalvageStructureStances(),
                    Inputs = new[] {
                            new Input(){ Entity = "item:dryingShedUpgrade", Amount = new InputAmount(){  NoOfItems = 1 } }},
                    Outputs = new[] {
                                    new Output() { EntityTypeToCreate = "item:solidMudBrick", Amount = new OutputAmount() { NoOfItems = 1 } },  
                                    new Output() { EntityTypeToCreate = "item:shadeleafCanes", Amount = new OutputAmount() { NoOfItems = 1 } },                                  
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    AgentActionState = AnimAction.Salvaging,
                    AgentAnimationStates = null,

                });
                #endregion

                var salvageMatsStances = GetSalvageStructureStances();
                float salvageMatsBaseTime = timeForInstantCraftingTask;
                var salvageMatActionState = AnimAction.Salvaging;

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = removingName,
                    KeyName = "salvageWingweedMats1People",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = salvageStructure,
                    IsSalvageProcess = true,
                    Stances = salvageMatsStances,
                    Inputs = new[] {
                            new Input(){ Entity = "item:wingweedMats1People", Amount = new InputAmount(){  NoOfItems = 1 } }},
                    Outputs = new[] {
                                    new Output() { EntityTypeToCreate = "item:wingweedMat", Amount = new OutputAmount() { NoOfItems = 1 } }
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = salvageMatsBaseTime },
                    AgentActionState = salvageMatActionState,
                    AgentAnimationStates = null,

                });

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = removingName,
                    KeyName = "salvageWingweedMats2People",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = salvageStructure,
                    IsSalvageProcess = true,
                    Stances = salvageMatsStances,
                    Inputs = new[] {
                            new Input(){ Entity = "item:wingweedMats2People", Amount = new InputAmount(){  NoOfItems = 1 } }},
                    Outputs = new[] {
                                    new Output() { EntityTypeToCreate = "item:wingweedMat", Amount = new OutputAmount() { NoOfItems = 2 } }
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = salvageMatsBaseTime * 1.5f },
                    AgentActionState = salvageMatActionState,
                    AgentAnimationStates = null,

                });

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = removingName,
                    KeyName = "salvageWingweedMats3People",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = salvageStructure,
                    IsSalvageProcess = true,
                    Stances = salvageMatsStances,
                    Inputs = new[] {
                            new Input(){ Entity = "item:wingweedMats3People", Amount = new InputAmount(){  NoOfItems = 1 } }},
                    Outputs = new[] {
                                    new Output() { EntityTypeToCreate = "item:wingweedMat", Amount = new OutputAmount() { NoOfItems = 3 } }
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = salvageMatsBaseTime * 2f },
                    AgentActionState = salvageMatActionState,
                    AgentAnimationStates = null,

                });

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = removingName,
                    KeyName = "salvageWingweedMats4People",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = salvageStructure,
                    IsSalvageProcess = true,
                    Stances = salvageMatsStances,
                    Inputs = new[] {
                            new Input(){ Entity = "item:wingweedMats4People", Amount = new InputAmount(){  NoOfItems = 1 } }},
                    Outputs = new[] {
                                    new Output() { EntityTypeToCreate = "item:wingweedMat", Amount = new OutputAmount() { NoOfItems = 4 } }
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = salvageMatsBaseTime * 2.5f },
                    AgentActionState = salvageMatActionState,
                    AgentAnimationStates = null,

                });

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = removingName,
                    KeyName = "salvageBeds3People",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = salvageStructure,
                    IsSalvageProcess = true,
                    Stances = GetSalvageStructureStances(),
                    Inputs = new[] {
                            new Input(){ Entity = "item:beds3People", Amount = new InputAmount(){  NoOfItems = 1 } }},
                    Outputs = new[] {
                                    new Output() { EntityTypeToCreate = "item:bedFrame", Amount = new OutputAmount() { NoOfItems = 3 } },
                                    new Output() { EntityTypeToCreate = "item:textile", Amount = new OutputAmount() { NoOfItems = 3 } }
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    AgentActionState = AnimAction.Salvaging,
                    AgentAnimationStates = null,

                });

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = removingName,
                    KeyName = "salvageBeds4People",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = salvageStructure,
                    IsSalvageProcess = true,
                    Stances = GetSalvageStructureStances(),
                    Inputs = new[] {
                            new Input(){ Entity = "item:beds4People", Amount = new InputAmount(){  NoOfItems = 1 } }},
                    Outputs = new[] {
                                    new Output() { EntityTypeToCreate = "item:bedFrame", Amount = new OutputAmount() { NoOfItems = 4 } },
                                    new Output() { EntityTypeToCreate = "item:textile", Amount = new OutputAmount() { NoOfItems = 4 } }
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    AgentActionState = AnimAction.Salvaging,
                    AgentAnimationStates = null,

                });

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = removingName,
                    KeyName = "salvageFurniture4People",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = salvageStructure,
                    IsSalvageProcess = true,
                    Stances = GetSalvageStructureStances(),
                    Inputs = new[] {
                            new Input(){ Entity = "item:furniture4People", Amount = new InputAmount(){  NoOfItems = 1 } }},
                    Outputs = new[] {
                                    new Output() { EntityTypeToCreate = "item:furniture", Amount = new OutputAmount() { NoOfItems = 1 } }
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    AgentActionState = AnimAction.Salvaging,
                    AgentAnimationStates = null,

                });
                #endregion

                #region ///Salvage Tools///

                SerializableDictionary<string, ChanceToTakeStance[]> salvageToolStances = kneelingProduction;

                float salvageTool = sittingLightWork;

                //  READ, READ!:
                // !!!!!!   REMEMBER TO PUT THE NAME OF THE SalvageProcess in ItemLoader also!!  like this: SalvageProcess = "salvageFireSuppressantCartridge" beneath DegradeType under  "item:fireSuppressantCartridge"




                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = disassembleName,
                    SummaryDescription = disassembleWithoutLossSummary,
                    KeyName = "salvageFireSuppressantCartridge",
                    RequiredSkill = "menial", //mp because we dont have UI feedback about skill needed for salvaging. when we do, use: "mechanics"
                    PhysicalWorkFactor = salvageTool,
                    IsSalvageProcess = true,
                    Stances = salvageToolStances,
                    Inputs = new[] {
                        new Input(){ Entity = "item:fireSuppressantCartridge", Amount = new InputAmount(){  NoOfItems = 1 } }
                    },

                    Outputs = new[] {
                        new Output() { EntityTypeToCreate = "item:emptyCartridge", IsWasteProduct = false, Amount = new OutputAmount() { NoOfItems = 1 }
                        }
                    },

                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForInstantCraftingTask }

                });


                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = salvagingName,
                    KeyName = "salvageFieldLabPacked", //cannibalizing the field lab. should be able to rebuild it tho if the components have not been used for gun.
                    RequiredSkill = "menial", //mp because we dont have UI feedback about skill needed for salvaging. when we do, use: "mechanics"
                    PhysicalWorkFactor = salvageTool,
                    IsSalvageProcess = true,
                    Stances = salvageToolStances,
                    Inputs = new[] {
                        new Input(){ Entity = "item:fieldLabPacked", Amount = new InputAmount(){  NoOfItems = 1 } }
                    },

                    Outputs = new[] {
                        new Output() { EntityTypeToCreate = "item:labComponents", IsWasteProduct = false, Amount = new OutputAmount() { NoOfItems = 1 }
                        }
                    },

                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
          //          ProcessToolSetKey = "toolSetShapenSimpleSmallMetal" //mp removed tool demands until we get production tooltip feedback  for the player

                });

                #region salvageMusket
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = disassembleName, //no loss of parts
                    SummaryDescription = disassembleWithoutLossSummary,
                    KeyName = "salvageMusket", // 
                    RequiredSkill = "smithing", //
                    PhysicalWorkFactor = salvageTool,
                    IsSalvageProcess = true,
                    Stances = salvageToolStances,
                    Inputs = new[] {
                        new Input(){ Entity = "item:musket", Amount = new InputAmount(){  NoOfItems = 1 } }
                    },

                    Outputs = new[] {
                        new Output() { EntityTypeToCreate = "item:gunBarrelSmoothLong", IsWasteProduct = false, Amount = new OutputAmount() { NoOfItems = 1 } },
                        new Output() { EntityTypeToCreate = "item:gunStock", IsWasteProduct = false, Amount = new OutputAmount() { NoOfItems = 1 } },
                        new Output() { EntityTypeToCreate = "item:flintlockMechanism", IsWasteProduct = false, Amount = new OutputAmount() { NoOfItems = 1 } },
                    },

                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    //        ProcessToolSetKey = "toolSetSimpleForging" //mp removed tool demands until we get production tooltip feedback  for the player
                });
                #endregion

                #region salvageMusketoon
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = disassembleName, //no loss of parts
                    SummaryDescription = disassembleWithoutLossSummary,
                    KeyName = "salvageMusketoon", // 
                    RequiredSkill = "smithing", //
                    PhysicalWorkFactor = salvageTool,
                    IsSalvageProcess = true,
                    Stances = salvageToolStances,
                    Inputs = new[] {
                        new Input(){ Entity = "item:musketoon", Amount = new InputAmount(){  NoOfItems = 1 } }
                    },

                    Outputs = new[] {
                        new Output() { EntityTypeToCreate = "item:gunBarrelSmoothShort", IsWasteProduct = false, Amount = new OutputAmount() { NoOfItems = 1 } },
                        new Output() { EntityTypeToCreate = "item:gunStock", IsWasteProduct = false, Amount = new OutputAmount() { NoOfItems = 1 } },
                        new Output() { EntityTypeToCreate = "item:flintlockMechanism", IsWasteProduct = false, Amount = new OutputAmount() { NoOfItems = 1 } },
                    },

                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    //          ProcessToolSetKey = "toolSetSimpleForging" //mp removed tool demands until we get production tooltip feedback  for the player
                });
                #endregion

                #region salvageGunpowderRifle
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = disassembleName, //no loss of parts
                    SummaryDescription = disassembleWithoutLossSummary,
                    KeyName = "salvageGunpowderRifle", // 
                    RequiredSkill = "smithing", //
                    PhysicalWorkFactor = salvageTool,
                    IsSalvageProcess = true,
                    Stances = salvageToolStances,
                    Inputs = new[] {
                        new Input(){ Entity = "item:gunpowderRifle", Amount = new InputAmount(){  NoOfItems = 1 } }
                    },

                    Outputs = new[] {
                        new Output() { EntityTypeToCreate = "item:gunBarrelRifled", IsWasteProduct = false, Amount = new OutputAmount() { NoOfItems = 1 } },
                        new Output() { EntityTypeToCreate = "item:gunStock", IsWasteProduct = false, Amount = new OutputAmount() { NoOfItems = 1 } },
                        new Output() { EntityTypeToCreate = "item:flintlockMechanism", IsWasteProduct = false, Amount = new OutputAmount() { NoOfItems = 1 } },
                    },

                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
               //     ProcessToolSetKey = "toolSetSimpleForging" //mp removed tool demands until we get production tooltip feedback  for the player
                });
                #endregion

                #region salvageBoltActionRifle
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = disassembleName, //no loss of parts
                    SummaryDescription = disassembleWithoutLossSummary,
                    KeyName = "salvageBoltActionRifle", // 
                    RequiredSkill = "smithing", //
                    PhysicalWorkFactor = salvageTool,
                    IsSalvageProcess = true,
                    Stances = salvageToolStances,
                    Inputs = new[] {
                        new Input(){ Entity = "item:boltActionRifle", Amount = new InputAmount(){  NoOfItems = 1 } }
                    },

                    Outputs = new[] {
                        new Output() { EntityTypeToCreate = "item:gunBarrelRifled", IsWasteProduct = false, Amount = new OutputAmount() { NoOfItems = 1 } },
                        new Output() { EntityTypeToCreate = "item:gunStock", IsWasteProduct = false, Amount = new OutputAmount() { NoOfItems = 1 } },
                        new Output() { EntityTypeToCreate = "item:boltActionMechanism", IsWasteProduct = false, Amount = new OutputAmount() { NoOfItems = 1 } },
                    },

                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
         //           ProcessToolSetKey = "toolSetSimpleForging" //mp removed tool demands until we get production tooltip feedback  for the player
                });
                #endregion

                #region salvageSentryItem
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = disassembleName, //no loss of parts
                    SummaryDescription = disassembleWithoutLossSummary,
                    KeyName = "salvageSentryItem", //cannibalizing the sentry item. 
                    RequiredSkill = "mechanics", //
                    PhysicalWorkFactor = salvageTool,
                    IsSalvageProcess = true,
                    Stances = salvageToolStances,
                    Inputs = new[] {
                        new Input(){ Entity = "item:sentry", Amount = new InputAmount(){  NoOfItems = 1 } }
                    },

                    Outputs = new[] {
                        new Output() { EntityTypeToCreate = "item:sentryGun", IsWasteProduct = false, Amount = new OutputAmount() { NoOfItems = 1 } },
                        new Output() { EntityTypeToCreate = "item:sentryWeaponMount", IsWasteProduct = false, Amount = new OutputAmount() { NoOfItems = 1 } },
                    },

                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
             //       ProcessToolSetKey = "toolSetShapenSimpleSmallMetal" //mp removed tool demands until we get production tooltip feedback  for the player
                });
                #endregion
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = salvagingName,
                    SummaryDescription = salvageWithLossSummary,
                    KeyName = "salvageSpraySentryItem", //taking apart the spray sentry item again
                    RequiredSkill = "mechanics", 
                    PhysicalWorkFactor = salvageTool,
                    IsSalvageProcess = true,
                    Stances = salvageToolStances,
                    Inputs = new[] {
                        new Input(){ Entity = "item:spraySentry", Amount = new InputAmount(){  NoOfItems = 1 } }
                    },

                    Outputs = new[] {//mp: the fire extinguisher and lab components are lost when salvaging. So, only relevant if player wants to put machine gun back on, or mount another weapon. Discuss

                        new Output() { EntityTypeToCreate = "item:sentryWeaponMount", IsWasteProduct = false, Amount = new OutputAmount() { NoOfItems = 1 } },
                    },

                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
              //      ProcessToolSetKey = "toolSetShapenSimpleSmallMetal" //mp removed tool demands until we get production tooltip feedback  for the player
                });

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = salvagingName,
                    SummaryDescription = salvageWithLossSummary,
                    KeyName = "salvageShotgunSentryItem", //taking apart the shotgun sentry item again
                    RequiredSkill = "mechanics", //
                    PhysicalWorkFactor = salvageTool,
                    IsSalvageProcess = true,
                    Stances = salvageToolStances,
                    Inputs = new[] {
                        new Input(){ Entity = "item:shotgunSentry", Amount = new InputAmount(){  NoOfItems = 1 } }
                    },

                    Outputs = new[] {//mp: the shotgun barrel is lost when salvaging. So, only relevant if player wants to put machine gun back on, or mount another weapon. Discuss

                        new Output() { EntityTypeToCreate = "item:sentryWeaponMount", IsWasteProduct = false, Amount = new OutputAmount() { NoOfItems = 1 } },

                    },

                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
           //         ProcessToolSetKey = "toolSetShapenSimpleSmallMetal" //mp removed tool demands until we get production tooltip feedback  for the player
                });

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = disassembleName, //no loss of parts
                    SummaryDescription = disassembleWithoutLossSummary,
                    KeyName = "salvageShotgun", //taking apart the shotgun
                    RequiredSkill = "mechanics", 
                    PhysicalWorkFactor = salvageTool,
                    IsSalvageProcess = true,
                    Stances = salvageToolStances,
                    Inputs = new[] {
                        new Input(){ Entity = "item:shotgun", Amount = new InputAmount(){  NoOfItems = 1 } }
                    },

                    Outputs = new[] {//

                        new Output() { EntityTypeToCreate = "item:gunBarrelSmoothShort", IsWasteProduct = false, Amount = new OutputAmount() { NoOfItems = 1 } },
                        new Output() { EntityTypeToCreate = "item:gunStock", IsWasteProduct = false, Amount = new OutputAmount() { NoOfItems = 1 } }, //
                    },

                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
              //      ProcessToolSetKey = "toolSetShapenSimpleSmallMetal" //mp removed tool demands until we get production tooltip feedback  for the player
                });
                #region salvageBlackPowderRifleAmmo
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = disassembleName, //no loss of parts
                    SummaryDescription = disassembleWithoutLossSummary,
                    KeyName = "salvageBlackPowderRifleAmmo",
                    RequiredSkill = "bushcraft", //
                    PhysicalWorkFactor = salvageTool,
                    IsSalvageProcess = true,
                    Stances = salvageToolStances,
                    Inputs = new[] 
                    {
                        new Input(){ Entity = "item:blackPowderRifleAmmo", Amount = new InputAmount(){  NoOfItems = 1 } }
                    },
                    Outputs = new[] 
                    {
                        new Output() { EntityTypeToCreate = "item:goldBullet", IsWasteProduct = false, Amount = new OutputAmount() { NoOfItems = 1 }},
                        new Output() { EntityTypeToCreate = "item:blackPowder", IsWasteProduct = false, Amount = new OutputAmount() { NoOfItems = 1 }},
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForInstantCraftingTask },
                });
                #endregion
                #region salvageBlackPowderShotAmmo
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = disassembleName, //no loss of parts
                    SummaryDescription = disassembleWithoutLossSummary,
                    KeyName = "salvageBlackPowderShotAmmo",
                    RequiredSkill = "bushcraft", //
                    PhysicalWorkFactor = salvageTool,
                    IsSalvageProcess = true,
                    Stances = salvageToolStances,
                    Inputs = new[] 
                    {
                        new Input(){ Entity = "item:blackPowderShotAmmo", Amount = new InputAmount(){  NoOfItems = 1 } }
                    },
                    Outputs = new[] 
                    {
                        new Output() { EntityTypeToCreate = "item:blunderbussBalls", IsWasteProduct = false, Amount = new OutputAmount() { NoOfItems = 1 }},
                        new Output() { EntityTypeToCreate = "item:blackPowder", IsWasteProduct = false, Amount = new OutputAmount() { NoOfItems = 1 }},
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForInstantCraftingTask },
                });
                #endregion
                #region salvageKnifeSpear
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = disassembleName, //no loss of parts
                    SummaryDescription = disassembleWithoutLossSummary,
                    KeyName = "salvageKnifeSpear",
                    RequiredSkill = "bushcraft", //
                    PhysicalWorkFactor = salvageTool,
                    IsSalvageProcess = true,
                    Stances = salvageToolStances,
                    Inputs = new[] 
                    {
                        new Input(){ Entity = "item:advancedKnifeSpear", Amount = new InputAmount(){  NoOfItems = 1 } }
                    },
                    Outputs = new[] 
                    {
                        new Output() { EntityTypeToCreate = "item:advancedKnife", IsWasteProduct = false, Amount = new OutputAmount() { NoOfItems = 1 }},
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                });
                #endregion

                #region salvageBlacksmithsToolbox
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = disassembleName, //no loss of parts
                    SummaryDescription = disassembleWithoutLossSummary,
                    KeyName = "salvageBlacksmithsToolbox",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = salvageTool,
                    IsSalvageProcess = true,
                    Stances = salvageToolStances,
                    Inputs = new[] 
                    {
                        new Input(){ Entity = "item:blacksmithsToolbox", Amount = new InputAmount(){  NoOfItems = 1 } }
                    },
                    Outputs = new[] 
                    {
                        new Output() { EntityTypeToCreate = "item:hammer", IsWasteProduct = false, Amount = new OutputAmount() { NoOfItems = 1 }},
                        new Output() { EntityTypeToCreate = "item:file", IsWasteProduct = false, Amount = new OutputAmount() { NoOfItems = 1 }},
                        new Output() { EntityTypeToCreate = "item:tongs", IsWasteProduct = false, Amount = new OutputAmount() { NoOfItems = 1 }},
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForInstantCraftingTask },
                });
                #endregion

                #region salvageMetalworkersToolbox
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = disassembleName,
                    SummaryDescription = disassembleWithoutLossSummary,
                    KeyName = "salvageMetalworkersToolbox",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = salvageTool,
                    IsSalvageProcess = true,
                    Stances = salvageToolStances,
                    Inputs = new[] 
                    {
                        new Input(){ Entity = "item:metalWorkersToolbox", Amount = new InputAmount(){  NoOfItems = 1 } }
                    },
                    Outputs = new[] 
                    {
                        new Output() { EntityTypeToCreate = "item:hammer", IsWasteProduct = false, Amount = new OutputAmount() { NoOfItems = 1 }},
                        new Output() { EntityTypeToCreate = "item:file", IsWasteProduct = false, Amount = new OutputAmount() { NoOfItems = 1 }},
                        new Output() { EntityTypeToCreate = "item:tongs", IsWasteProduct = false, Amount = new OutputAmount() { NoOfItems = 1 }},
                        new Output() { EntityTypeToCreate = "item:handDrill", IsWasteProduct = false, Amount = new OutputAmount() { NoOfItems = 1 }},
                        new Output() { EntityTypeToCreate = "item:hacksaw", IsWasteProduct = false, Amount = new OutputAmount() { NoOfItems = 1 }},
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForInstantCraftingTask },
                });
                #endregion

                #region salvageCarpentersToolbox
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = disassembleName, //no loss of parts
                    SummaryDescription = disassembleWithoutLossSummary,
                    KeyName = "salvageCarpentersToolbox",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = salvageTool,
                    IsSalvageProcess = true,
                    Stances = salvageToolStances,
                    Inputs = new[] 
                    {
                        new Input(){ Entity = "item:carpentersToolbox", Amount = new InputAmount(){  NoOfItems = 1 } }
                    },
                    Outputs = new[] 
                    {                      
                        new Output() { EntityTypeToCreate = "item:hammer", IsWasteProduct = false, Amount = new OutputAmount() { NoOfItems = 1 }},
                        new Output() { EntityTypeToCreate = "item:file", IsWasteProduct = false, Amount = new OutputAmount() { NoOfItems = 1 }},
                        new Output() { EntityTypeToCreate = "item:steelHandAxe", IsWasteProduct = false, Amount = new OutputAmount() { NoOfItems = 1 }},
                        new Output() { EntityTypeToCreate = "item:handDrill", IsWasteProduct = false, Amount = new OutputAmount() { NoOfItems = 1 }},
                        new Output() { EntityTypeToCreate = "item:bowSaw", IsWasteProduct = false, Amount = new OutputAmount() { NoOfItems = 1 }},
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForInstantCraftingTask },
                });
                #endregion
                #endregion


                #region Upgrading

                #region upgradePolymerWorkshop
                //
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = upgradingName,
                    KeyName = "upgradePolymerWorkshop",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "mechanics",
                    PhysicalWorkFactor = moderateWork,
                    Stances = kneelingOrStandingProduction,
                    Inputs = new[] 
                    {                         
                        new Input(){ Entity = "item:extrusionMachineComponents",  IsConsumed = false, Amount = new InputAmount(){  NoOfItems = 1 } },
                        new Input(){ Entity = "item:wroughtIron",  IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 1 } },
                        new Input(){ Entity = "item:solidMudBrick",  IsConsumed = false, Amount = new InputAmount(){  NoOfItems = 3 } }  // for fireplace, which is now part of the upgrade                     
                    },
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:polymerWorkshopUpgrade", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForBigCraftingTask },
                    AgentActionState = AnimAction.Building,
                    ProcessToolSetKey = "toolSetAssembleMetalMachine" //

                });
                #endregion

                #region upgrade metal shop
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = upgradingName,
                    KeyName = "upgradeMetalLatheShopHumanPowered",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "mechanics",
                    PhysicalWorkFactor = moderateWork,
                    Stances = kneelingOrStandingProduction,

                    Inputs = new[] { new Input() { Entity = "item:metalLatheComponents", IsConsumed = false, Amount = new InputAmount() { NoOfItems = 1 } }, // the major components
                                     new Input() { Entity = "item:humanPowerUnit", IsConsumed = false, Amount = new InputAmount() { NoOfItems = 1 } },
                                     new Input() { Entity = "item:blisterSteel",  IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 1 }}, // for toolheads
                                    new Input(){ Entity = "item:wroughtIron",  IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 1 }}, // for various bits and pieces                                
                                    new Input(){ Entity = "item:thunderChickenTannedHide",  IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 1 }} //for the belt drive                                     

                    },
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:metalLatheShopHumanPoweredUpgrade", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForBigCraftingTask },
                    ProcessToolSetKey = "toolSetAssembleMetalMachine", // the metalworker toolbox and some attachment tool
                    AgentActionState = AnimAction.Building,
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region upgradeCarpenterWorkshop
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = upgradingName,
                    KeyName = "upgradeCarpenterWorkshop",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "construction",
                    PhysicalWorkFactor = moderateWork,
                    Stances = kneelingOrStandingProduction,
                    Inputs = new[] 
                    {                         //todo what about some sticks or shadeleafcane
                        new Input(){ Entity = "item:barClamps",  IsConsumed = false, Amount = new InputAmount(){  NoOfItems = 1 } },
                        new Input(){ Entity = "item:solidMudBrick",  IsConsumed = false, Amount = new InputAmount(){  NoOfItems = 2 } },
                        new Input(){ Entity = "item:sticks",  IsConsumed = false, Amount = new InputAmount(){  NoOfItems = 2 } },
                        new Input(){ Entity = "item:shadeleafCanes",  IsConsumed = false, Amount = new InputAmount(){  NoOfItems = 1 } }                     
                               
                    },
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:carpenterWorkshopUpgrade", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForBigCraftingTask },
                    AgentActionState = AnimAction.Building,
                    ProcessToolSetKey = "toolSetCordage" // 
                });            

                #endregion

                #region upgradeTextileWorkshop
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = upgradingName,
                    KeyName = "upgradeTextileWorkshop",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "construction",
                    PhysicalWorkFactor = moderateWork,
                    Stances = kneelingOrStandingProduction,
                    Inputs = new[] 
                    {                         
                        new Input(){ Entity = "item:loomComponents",  IsConsumed = false, Amount = new InputAmount(){  NoOfItems = 1 } },
                        new Input(){ Entity = "item:waterCaneStem",  IsConsumed = false, Amount = new InputAmount(){  NoOfItems = 4 } },
                        new Input(){ Entity = "item:thunderChickenTannedHide",  IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 1 }} //for the strings  
                    },
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:textileWorkshopUpgrade", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForBigCraftingTask },
                    AgentActionState = AnimAction.Building,
                    ProcessToolSetKey = "toolSetCordage"
                });

                #endregion

                #region Upgrade stove
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = upgradingName,
                    KeyName = "upgradeSimpleStove",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "construction",
                    PhysicalWorkFactor = constructionExertion,
                    Stances = constructionStances,
                    Inputs = new[] {  
                        new Input(){ Entity = "item:stones",  IsConsumed = false, Amount = new InputAmount(){  NoOfItems = 1 }}, 
                        new Input(){ Entity = "item:solidMudBrick",  IsConsumed = false, Amount = new InputAmount(){  NoOfItems = 1 }}                                                       
                    },
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:simpleStoveUpgrade", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    AgentActionState = AnimAction.Building,
                    ProcessToolSetKey = "toolSetSmallDiggingConstruction",
                    AgentAnimationStates = new[] { AnimModifier.Improvised }

                });

                #endregion

                #region Upgrade community hall
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = upgradingName,
                    KeyName = "upgradeCommunityHall",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "construction",
                    PhysicalWorkFactor = constructionExertion,
                    Stances = constructionStances,
                    Inputs = new[] {   
                        new Input(){ Entity = "item:solidMudBrick",  IsConsumed = false, Amount = new InputAmount(){  NoOfItems = 4 }},
                        new Input(){ Entity = "item:spoakBranchesTrimmed",  IsConsumed = false, Amount = new InputAmount(){  NoOfItems = 2 }}, 
                        new Input(){ Entity = "item:spoakShingles",  IsConsumed = false, Amount = new InputAmount(){  NoOfItems = 3 }}                                                          
                    },
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:communityHallUpgrade", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForBigCraftingTask },
                    AgentActionState = AnimAction.Building,
                    ProcessToolSetKey = "toolSetDiggingConstruction",
                    AgentAnimationStates = new[] { AnimModifier.Improvised }

                });

                #endregion

                #region Upgrade Smoke Oven
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = upgradingName,
                    KeyName = "upgradeSmokeOven",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "construction",
                    PhysicalWorkFactor = constructionExertion,
                    Stances = constructionStances,
                    Inputs = new[] {   
                        new Input(){ Entity = "item:solidMudBrick",  IsConsumed = false, Amount = new InputAmount(){  NoOfItems = 2 }},
                        new Input(){ Entity = "item:stones",  IsConsumed = false, Amount = new InputAmount(){  NoOfItems = 1 }}, 
                                                         
                    },
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:smokeOvenUpgrade", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    AgentActionState = AnimAction.Building,
                    ProcessToolSetKey = "toolSetSmallDiggingConstruction",
                    AgentAnimationStates = new[] { AnimModifier.Improvised }

                });

                #endregion

                #region Upgrade Drying shed
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = upgradingName,
                    KeyName = "upgradeDryingShed",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "construction",
                    PhysicalWorkFactor = constructionExertion,
                    Stances = constructionStances,
                    Inputs = new[] {   
                        new Input(){ Entity = "item:solidMudBrick",  IsConsumed = false, Amount = new InputAmount(){  NoOfItems = 1 }},
                        new Input(){ Entity = "item:spoakShingles",  IsConsumed = false, Amount = new InputAmount(){  NoOfItems = 1 }}, 
                        new Input(){ Entity = "item:shadeleafCanes",  IsConsumed = false, Amount = new InputAmount(){  NoOfItems = 2 }},
                                                         
                    },
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:dryingShedUpgrade", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    AgentActionState = AnimAction.Building,
                    ProcessToolSetKey = "toolSetSmallDiggingConstruction",
                    AgentAnimationStates = new[] { AnimModifier.Improvised }

                });

                #endregion

                #region upgrade WingweedMats

                var matsStances = kneelingProduction;
                var matsAction = AnimAction.Mending;
                var upgradeMatsBaseTime = 0.6f * timeForSmallCraftingTask;

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = upgradingName,
                    KeyName = "upgradeWingweedMats1People",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = null,
                    //IsUpgrade = true,
                    PhysicalWorkFactor = moderateWork,
                    Stances = matsStances,
                    Inputs = new[] {  
                        new Input(){ Entity = "item:wingweedMat",  IsConsumed = false, Amount = new InputAmount(){  NoOfItems = 1 } }                                                        
                    },
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:wingweedMats1People", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 1f * upgradeMatsBaseTime },
                    AgentActionState = matsAction

                });

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = upgradingName,
                    KeyName = "upgradeWingweedMats2People",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = null,
                    //IsUpgrade = true,
                    PhysicalWorkFactor = moderateWork,
                    Stances = matsStances,
                    Inputs = new[] {  
                        new Input(){ Entity = "item:wingweedMat",  IsConsumed = false, Amount = new InputAmount(){  NoOfItems = 2 } }                                                        
                    },
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:wingweedMats2People", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 1.5f * upgradeMatsBaseTime },
                    AgentActionState = matsAction

                });

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = upgradingName,
                    KeyName = "upgradeWingweedMats3People",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = null,
                    //IsUpgrade = true,
                    PhysicalWorkFactor = moderateWork,
                    Stances = matsStances,
                    Inputs = new[] {  
                        new Input(){ Entity = "item:wingweedMat",  IsConsumed = false, Amount = new InputAmount(){  NoOfItems = 3 } }                                                        
                    },
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:wingweedMats3People", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 2f * upgradeMatsBaseTime },
                    AgentActionState = matsAction

                });

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = upgradingName,
                    KeyName = "upgradeWingweedMats4People",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = null,
                    //IsUpgrade = true,
                    PhysicalWorkFactor = moderateWork,
                    Stances = matsStances,
                    Inputs = new[] {  
                        new Input(){ Entity = "item:wingweedMat",  IsConsumed = false, Amount = new InputAmount(){  NoOfItems = 4 } }                                                        
                    },
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:wingweedMats4People", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 2.5f * upgradeMatsBaseTime },
                    AgentActionState = matsAction                   

                });
                #endregion

                #region upgrade beds

                var bedsStances = kneelingProduction;
                var bedsAction = AnimAction.Mending;
                var upgradeBedsBaseTime = timeForSmallCraftingTask;

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = upgradingName,
                    KeyName = "upgradeBeds3People",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = null,
                    PhysicalWorkFactor = moderateWork,
                    Stances = bedsStances,
                    Inputs = new[] {  
                        new Input(){ Entity = "item:bedFrame",  IsConsumed = false, Amount = new InputAmount(){  NoOfItems = 3 } },
                        new Input(){ Entity = "item:textile",  IsConsumed = false, Amount = new InputAmount(){  NoOfItems = 3 } }                                  
                    },
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:beds3People", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 2f * timeForSmallCraftingTask },
                    AgentActionState = bedsAction

                });

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = upgradingName,
                    KeyName = "upgradeBeds4People",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = null,
                    PhysicalWorkFactor = moderateWork,
                    Stances = bedsStances,
                    Inputs = new[] {  
                        new Input(){ Entity = "item:bedFrame",  IsConsumed = false, Amount = new InputAmount(){  NoOfItems = 4 } },
                        new Input(){ Entity = "item:textile",  IsConsumed = false, Amount = new InputAmount(){  NoOfItems = 4 } }                                 
                    },
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:beds4People", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 2.5f * upgradeBedsBaseTime },
                    AgentActionState = bedsAction

                });
                #endregion

                #region Upgrade furniture

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = upgradingName,
                    KeyName = "upgradeFurniture4People",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = null,
                    PhysicalWorkFactor = moderateWork,
                    Stances = bedsStances,
                    Inputs = new[] {  
                        new Input(){ Entity = "item:furniture",  IsConsumed = false, Amount = new InputAmount(){  NoOfItems = 1 } }                         
                    },
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:furniture4People", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 1f * upgradeBedsBaseTime },
                    AgentActionState = bedsAction

                });

                #endregion

                #endregion

                #region ///Producing and crafting materials and items


                float sharpening = 1.1f * sittingLightWork;


                #region makeImprovisedGreenHouseCover
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeCleanTurnipGuts",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = assembleTool,
                    Stances = GetToolMakingStances(),
                    WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart,    //mp after rubbing in the brain, the hide is now put in a vat. instead of drying on the hide rack (because not enough room for that many)               
                    Inputs = new[] 
                    { 
                        new Input() { Entity = "item:turnipGuts", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 10, } },
                        new Input() { Entity = "item:turnipBrain", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1, } },
                        new Input() { Entity = "item:salt", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1, } }
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:improvisedGreenHouseCover", Amount = new OutputAmount(){  NoOfItems = 10 },  ToolContainerTagsToPlaceIn = new[]{"liquidContainerNoHeat"}},//mp was hide rack, but is not big enough to contain that many items.
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForShortStandaloneTask }, //
                    ProcessToolSetKey = "toolSetDissolveInedibleMatter",
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

 


                #region makeThunderChickenRawhide
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeThunderChickenRawhide",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = assembleTool,
                    Stances = GetToolMakingStances(),
                    WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart,
                    //RequiresWork = WorkerNeeded. false, //mp after rubbing in the brain and scraping, the hide is drying on the hide rack on its own
                    Inputs = new[] 
                    { 
                        new Input() { Entity = "item:thunderChickenGreenHide", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1, } },
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:thunderChickenRawhide", Amount = new OutputAmount(){  NoOfItems = 1}, ToolContainerTagsToPlaceIn = new[]{"hideRack"}}, //
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForShortStandaloneTask },
                    ProcessToolSetKey = "toolSetCleanHide", //uses a brain as one of the tools
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion
                #region makeThunderChickenTannedHide
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeThunderChickenTannedHide",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = assembleTool,
                    Stances = GetToolMakingStances(),
                    Inputs = new[] 
                    { 
                        new Input() { Entity = "item:thunderChickenRawhide", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1, } } 
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:thunderChickenTannedHide", Amount = new OutputAmount(){ NoOfItems = 1 }},
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    ProcessToolSetKey = "toolSetFireplace",
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion
                #region makewhipjawRawhide
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeWhipjawRawhide",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = assembleTool,
                    Stances = GetToolMakingStances(),
                    WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart, //mp after rubbing in the brain and scraping, the hide is drying on the hide rack on its own
                    Inputs = new[] 
                    { 
                        new Input() { Entity = "item:whipjawGreenHide", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1, } },
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:whipjawRawhide", Amount = new OutputAmount(){  NoOfItems = 1}, ToolContainerTagsToPlaceIn = new[]{"hideRack"}}, //
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForShortStandaloneTask },
                    ProcessToolSetKey = "toolSetCleanHide", //uses a brain as one of the tools
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion
                #region makewhipjawTannedHide
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeWhipjawTannedHide",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = assembleTool,
                    Stances = GetToolMakingStances(),
                    Inputs = new[] 
                    { 
                        new Input() { Entity = "item:whipjawRawhide", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1, } } 
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:whipjawTannedHide", Amount = new OutputAmount(){ NoOfItems = 1 }},
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    ProcessToolSetKey = "toolSetFireplace",
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion
                #region makeMegapodRawhide
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeMegapodRawhide",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = assembleTool,
                    Stances = GetToolMakingStances(),
                    WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart, //mp after rubbing in the brain and scraping, the hide is drying on the hide rack on its own
                    Inputs = new[] 
                    { 
                        new Input() { Entity = "item:megapodGreenHide", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1, } },
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:megapodRawhide", Amount = new OutputAmount(){  NoOfItems = 1}, ToolContainerTagsToPlaceIn = new[]{"hideRack"}}, //
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForShortStandaloneTask },
                    ProcessToolSetKey = "toolSetCleanHide", //uses a brain as one of the tools
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion
                #region makeMegapodTannedHide
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeMegapodTannedHide",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = assembleTool,
                    Stances = GetToolMakingStances(),
                    Inputs = new[] 
                    { 
                        new Input() { Entity = "item:megapodRawhide", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1, } } 
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:megapodTannedHide", Amount = new OutputAmount(){ NoOfItems = 1 }},
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    ProcessToolSetKey = "toolSetFireplace",
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion
                #region makeOrganicFertilizer
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeOrganicFertilizer",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "farming",
                    PhysicalWorkFactor = assembleTool,
                    Stances = GetToolMakingStances(),
                    Inputs = new[] { new Input() { Entity = "item:organicMatter", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1, } } },
                    Outputs = new[]
                     {
                        new Output(){ EntityTypeToCreate = "item:organicFertilizer", Amount = new OutputAmount(){  Bulk = new Bulk(){FractionOfInputBulk = 1}}},
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask, MultiplyByBulk = true },
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion
                #region makeLandMine
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeLandMine",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "bushcraft", //
                    PhysicalWorkFactor = assembleTool,
                    Stances = GetToolMakingStances(),
                    Inputs = new[]
                    {
                        new Input(){ Entity = "item:blackPowder",  IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 1 }}, //explosives
                        new Input(){ Entity = "item:flintlockMechanism",  IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 1 }}, //triggering mechanism 
                        new Input(){ Entity = "item:waterCaneStem",  IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 2 }} //container
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:landMine", Amount = new OutputAmount(){ NoOfItems = 4 }} //mp hmm does not match the number of mechanisms...
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    ProcessToolSetKey = "toolSetBluntTool", // is there a beetter tool?
                    AgentAnimationStates = new[] { AnimModifier.Improvised }

                });
                #endregion

                #region makeVarmintBomb
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeVarmintBomb",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = assembleTool,
                    Stances = GetToolMakingStances(),
                    Inputs = new[]
                    {
                        new Input(){ Entity = "item:blackPowder",  IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 1 }}, //explosives
                        new Input(){ Entity = "item:waterCaneStem",  IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 1 }} //container
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:varmintBomb", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    ProcessToolSetKey = "toolSetBluntTool", // is there a beetter tool?
                    AgentAnimationStates = new[] { AnimModifier.Improvised }

                });
                #endregion
                #region makeBlackPowderRifleAmmo
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeBlackPowderRifleAmmo", //may change name, check itemLoader
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = assembleTool,
                    Stances = GetToolMakingStances(),
                    Inputs = new[]
                    {
                        new Input(){ Entity = "item:blackPowder",  IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 1 }},
                        new Input(){ Entity = "item:goldBullet",  IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 4 }},

                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:blackPowderRifleAmmo", Amount = new OutputAmount(){ NoOfItems = 4 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForInstantCraftingTask }, //just putting it in pouch and bag. that he already has, I guess.
                    AgentAnimationStates = new[] { AnimModifier.Improvised }

                });
                #endregion
                #region makeBlackPowderShotAmmo
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeBlackPowderShotAmmo", //may change name, check itemLoader
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = assembleTool,
                    Stances = GetToolMakingStances(),
                    Inputs = new[]
                    {
                        new Input(){ Entity = "item:blackPowder",  IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 1 }},
                        new Input(){ Entity = "item:blunderbussBalls",  IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 4 }},
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:blackPowderShotAmmo", Amount = new OutputAmount(){ NoOfItems = 4 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForInstantCraftingTask }, //just putting it in pouch and bag. that he already has, I guess.
                    AgentAnimationStates = new[] { AnimModifier.Improvised }

                });
                #endregion
            //casting related
                #region makeGoldBullet
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeGoldBullet", //mp no rifling??
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "chemistry",
                    PhysicalWorkFactor = assembleTool,
                    Stances = GetToolMakingStances(),
                    Inputs = new[]
                    {
                        new Input(){ Entity = "item:gold",  IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 1 }},
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:goldBullet", Amount = new OutputAmount(){ NoOfItems = 4 }} //same amount as "makeBlunderbussBalls"
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    ProcessToolSetKey = "toolSetBulletCasting",
                    AgentActionState = AnimAction.Mending, //note: mending is default , so it's not really necessary to put it here.
                    AgentAnimationStates = new[] { AnimModifier.Metal }//mp an attempt to make it select the standing up hammer anim at the anvil

                });
                #endregion

                #region makeSandMold
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeSandMold",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "chemistry",
                    PhysicalWorkFactor = moderateWork,
                    Stances = GetToolMakingStances(),
                    Inputs = new[]
                    {
                         new Input(){ Entity = "item:sticks",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } },
                         new Input(){ Entity = "item:clay",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } } //a lot of clay for this, can be expælained because they sort it for a special kind.
                    },

                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:sandMold", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    ProcessToolSetKey = "toolSetShapenSmallWood",
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion
                #region makeGoldPot
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeGoldPot", //
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "chemistry",
                    PhysicalWorkFactor = hardWork,
                    Stances = GetToolMakingStances(),
                    Inputs = new[]
                    {
                        new Input(){ Entity = "item:gold",  IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 1 }},
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:goldPot", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    ProcessToolSetKey = "toolSetPrimitiveCasting",
                    AgentActionState = AnimAction.Mending, //note: mending is default , so it's not really necessary to put it here.
                    AgentAnimationStates = new[] { AnimModifier.Improvised }//TODO

                });
                #endregion
                #region makeMetalLatheComponents
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeMetalLatheComponents", //
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "mechanics",
                    PhysicalWorkFactor = hardWork,
                    Stances = GetToolMakingStances(),
                    Inputs = new[]
                    {
                        new Input(){ Entity = "item:gold",  IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 2 }},
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:metalLatheComponents", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForBigCraftingTask },
                    ProcessToolSetKey = "toolSetSimpleCasting", //removing sprues and also shaping and drilling the cast objects to some extent.
                    AgentActionState = AnimAction.Mending, //note: mending is default , so it's not really necessary to put it here.
                    AgentAnimationStates = new[] { AnimModifier.Improvised }//TODO

                });
                #endregion


                #region makeExtrusionMachineComponents
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeExtrusionMachineComponents", //
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "mechanics",
                    PhysicalWorkFactor = hardWork,
                    Stances = GetToolMakingStances(),
                    Inputs = new[]
                    {
                        new Input(){ Entity = "item:gold",  IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 2 }},
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:extrusionMachineComponents", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForBigCraftingTask },
                    ProcessToolSetKey = "toolSetSimpleCasting", //removing sprues and also shaping and drilling the cast objects to some extent.
                    AgentActionState = AnimAction.Mending, //note: mending is default , so it's not really necessary to put it here.
                    AgentAnimationStates = new[] { AnimModifier.Improvised }//TODO

                });
                #endregion



                #region makeGoldSheet
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeGoldSheet", //
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "chemistry",
                    PhysicalWorkFactor = hardWork,
                    Stances = GetToolMakingStances(),
                    Inputs = new[]
                    {
                        new Input(){ Entity = "item:gold",  IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 2 }},
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:goldSheet", Amount = new OutputAmount(){ NoOfItems = 2 }} //mass conservation
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForBigCraftingTask },
                    ProcessToolSetKey = "toolSetSimpleCasting", // shaping  the cast objects to some extent.
                    AgentActionState = AnimAction.Mending, //note: mending is default , so it's not really necessary to put it here.
                    AgentAnimationStates = new[] { AnimModifier.Improvised }//TODO

                });
                #endregion

                #region makeStillComponents
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeStillComponents",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "smithing",
                    PhysicalWorkFactor = hardWork,
                    Stances = GetSmithingStance(), // 
                    Inputs = new[]
                    {
                         new Input(){ Entity = "item:goldSheet",IsConsumed = true, Amount = new InputAmount() { NoOfItems = 2}}  
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:stillComponents", Amount = new OutputAmount(){ NoOfItems = 1 }},
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForBigCraftingTask },//
                    ProcessToolSetKey = "toolSetSimpleForging",
                    AgentActionState = AnimAction.Mending, //note: mending is default , so it's not really necessary to put it here.
                    AgentAnimationStates = new[] { AnimModifier.Metal }//mp an attempt to make it select the standing up hammer anim at the anvil
                });
                #endregion

                #region makehumanPowerUnitComponents
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "humanPowerUnitComponents", //
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "mechanics",
                    PhysicalWorkFactor = moderateHardWork,
                    Stances = GetToolMakingStances(),
                    Inputs = new[]
                    {
                        new Input(){ Entity = "item:gold",  IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 1 }},
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:humanPowerUnitComponents", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForBigCraftingTask },
                    ProcessToolSetKey = "toolSetSimpleCasting", //removing sprues and also shaping and drilling the cast objects to some extent.
                    AgentActionState = AnimAction.Mending, //note: mending is default , so it's not really necessary to put it here.
                    AgentAnimationStates = new[] { AnimModifier.Improvised }//TODO

                });
                #endregion

                #region makeHumanPowerUnit
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeHumanPowerUnit",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "mechanics", //
                    PhysicalWorkFactor = moderateWork,
                    Stances = GetToolMakingStances(),
                    Inputs = new[]
                    {
                        new Input(){ Entity = "item:sticks",  IsConsumed = false, Amount = new InputAmount(){  NoOfItems = 2 }}, // the frame
                        new Input(){ Entity = "item:humanPowerUnitComponents",  IsConsumed = false, Amount = new InputAmount(){  NoOfItems = 1 }}, // major components

                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:humanPowerUnit", Amount = new OutputAmount(){ NoOfItems = 1 }} //
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForBigCraftingTask },
                    ProcessToolSetKey = "toolSetAssembleMetalMachine", // the metalworker toolbox and some attachment tool
                    AgentAnimationStates = new[] { AnimModifier.Improvised }

                });
                #endregion


                #region blackPowder
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeBlackPowder",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "bushcraft", //chemistry todo
                    PhysicalWorkFactor = assembleTool,
                    Stances = GetToolMakingStances(),
                    Inputs = new[] //needs balancing test
                    {
                        new Input(){ Entity = "item:saltpeter",  IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 1 }},
                        new Input(){ Entity = "item:charcoal",  IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 1 }},
                        new Input(){ Entity = "item:sulfurPowder",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 }}
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:blackPowder", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    ProcessToolSetKey = "toolSetMixingBlackPowder",
                    AgentAnimationStates = new[] { AnimModifier.Improvised }

                });
                #endregion

                #region makeTappingBucket //
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeTappingBucket",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = assembleTool,
                    Inputs = new[]
                    {
                         new Input(){ Entity = "item:clayJar",  IsConsumed = false, Amount = new InputAmount(){ NoOfItems = 1 } }, //
                         new Input(){ Entity = "item:sticks",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } },
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:tappingBucket", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    ProcessToolSetKey = "toolSetShapenSmallWood",
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion
                #region salvageTappingBucket
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = disassembleName, //no loss of parts
                    KeyName = "salvageTappingBucket",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = sittingLightWork,
                    IsSalvageProcess = true,
                    Stances = kneelingProduction,
                    Inputs = new[] {
                        new Input(){ Entity = "item:tappingBucket", Amount = new InputAmount(){  NoOfItems = 1 } }},


                    Outputs = new[] {new Output() { EntityTypeToCreate = "item:clayJar",  Amount = new OutputAmount() { NoOfItems = 1 } } 
                                
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForInstantCraftingTask },

                });
                #endregion
                #region makePlasticTappingBucket //
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makePlasticTappingBucket",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = assembleTool,
                    Inputs = new[]
                    {
                         new Input(){ Entity = "item:improvisedPlasticJar",  IsConsumed = false, Amount = new InputAmount(){ NoOfItems = 1 } }, //
                         new Input(){ Entity = "item:sticks",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } },
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:plasticTappingBucket", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    ProcessToolSetKey = "toolSetShapenSmallWood",
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion
                #region salvagePlasticTappingBucket
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = disassembleName, //no loss of parts
                    KeyName = "salvagePlasticTappingBucket",
                    JobTypeKey = "craftingJobType", //yeah, not crafting but anyway.
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = sittingLightWork,
                    IsSalvageProcess = true,
                    Stances = kneelingProduction,
                    Inputs = new[] {
                        new Input(){ Entity = "item:plasticTappingBucket", Amount = new InputAmount(){  NoOfItems = 1 } }},


                    Outputs = new[] {new Output() { EntityTypeToCreate = "item:improvisedPlasticJar",  Amount = new OutputAmount() { NoOfItems = 1 } } 
                                
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForInstantCraftingTask },

                });
                #endregion
                #region makeVat
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName, //check if this is what the rest are called
                    KeyName = "makeVat",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "bushcraft", 
                    PhysicalWorkFactor = moderateWork,
                    Inputs = new[]
                    {
                         new Input(){ Entity = "item:spoakShingles",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } }, //has to be watertight, so i'm using shingles.
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:vat", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    ProcessToolSetKey = "toolSetCombineLightImprovisedObjects",
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion
                // todo: fix degrade profile
                #region makeSaltpeterSolution
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName, //check if this is what the rest are called
                    KeyName = "makeSaltpeterSolution",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "bushcraft", //
                    PhysicalWorkFactor = moderateWork,
                    WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart, //Needs To dissolve on its own
                    Inputs = new[]
                    {

                         new Input(){ Entity = "item:guano",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } },
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:saltpeterSolution", Amount = new OutputAmount(){ NoOfItems = 1 },  ToolContainerTagsToPlaceIn = new[]{"liquidContainerNoHeat"}} //
                        
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForLongStandaloneTask }, //mp make longer
                    ProcessToolSetKey = "toolSetDissolveInedibleMatter", //                    
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion
      

                #region makeSaltpeter
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeSaltpeter",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "bushcraft", //
                    PhysicalWorkFactor = moderateWork,                  
                    Inputs = new[]
                    {
                         new Input(){ Entity = "item:saltpeterSolution",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } },
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:saltpeter", Amount = new OutputAmount(){ NoOfItems = 1 }},

                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask }, 
                    ProcessToolSetKey = "toolSetMakeStew", //pours it into a pot and heats it. maybe make different toolset so they don't use cooking pot?
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region makeGuanoFertilizer
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeGuanoFertilizer",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "farming", //
                    PhysicalWorkFactor = moderateWork,
                    Inputs = new[]
                    {
                         new Input(){ Entity = "item:guano",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } },
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:guanoFertilizer", Amount = new OutputAmount(){ NoOfItems = 2 }},
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region makeSulfurPowder
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeSulfurPowder",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "bushcraft", //chemistry
                    PhysicalWorkFactor = moderateWork,
                    Inputs = new[]
                    {
                         new Input(){ Entity = "item:sulfurBlocks",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } },
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:sulfurPowder", Amount = new OutputAmount(){ NoOfItems = 3 }},
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForTinyCraftingTask },
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region makeFlintlockMechanism
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeFlintlockMechanism",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "smithing",
                    PhysicalWorkFactor = moderateWork,
                    Stances = GetSmithingStance(), // GetToolMakingStances(),
                    Inputs = new[]
                    {
                         new Input(){ Entity = "item:wroughtIron",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } },
                         new Input(){ Entity = "item:flintRough",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } },
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:flintlockMechanism", Amount = new OutputAmount(){ NoOfItems = 2 }} //mp TODO: balance this against iron knife etc.. 
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    ProcessToolSetKey = "toolSetHarderImprovisedForging", //requires filing
                    AgentActionState = AnimAction.Mending, //note: mending is default , so it's not really necessary to put it here.
                    AgentAnimationStates = new[] { AnimModifier.Metal }//mp an attempt to make it select the standing up hammer anim at the anvil
                });
                #endregion

                #region makeBlunderbussBalls
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeBlunderbussBalls",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "smithing",
                    PhysicalWorkFactor = moderateWork,
                    Stances = GetSmithingStance(), // GetToolMakingStances(),
                    Inputs = new[]
                    {
                         new Input(){ Entity = "item:gold",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } },
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:blunderbussBalls", Amount = new OutputAmount(){ NoOfItems = 4 }} // same amount as "makeGoldBullet"
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    ProcessToolSetKey = "toolSetHarderImprovisedForging", //requires filing. the gold nuggets are not melted, just heated and can have different sizes as simple ammo.
                    AgentActionState = AnimAction.Mending, //note: mending is default , so it's not really necessary to put it here.
                    AgentAnimationStates = new[] { AnimModifier.Metal }//mp an attempt to make it select the standing up hammer anim at the anvil
                });
                #endregion


                #region makeSpikeTrap
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeSpikeTrap",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "smithing",
                    PhysicalWorkFactor = moderateWork,
                    Stances = GetSmithingStance(), // GetToolMakingStances(),
                    
                    Inputs = new[]
                    {
                         new Input(){ Entity = "item:wroughtIron",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1} }, //not used:  Substances = new[]{"iron"} 
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:spikeTrap",Amount = new OutputAmount(){ NoOfItems = 1 }} //
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    ProcessToolSetKey = "toolSetHarderImprovisedForging", //requires filing
                    AgentActionState = AnimAction.Mending, //note: mending is default , so it's not really necessary to put it here.
                    AgentAnimationStates = new[] { AnimModifier.Metal }//mp an attempt to make it select the standing up hammer anim at the anvil
                });
                #endregion

                #region makeBellows
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeBellows",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = moderateWork,
                    Stances = GetToolMakingStances(),
                    Inputs = new[] //
                    {
                         new Input(){ Entity = "item:thunderChickenTannedHide",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } },
                         new Input(){ Entity = "item:sticks",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } }
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:bellows", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForBigCraftingTask }, //seems like complicated task. mp
                    ProcessToolSetKey = "toolSetCombineLightImprovisedObjects",
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region makeRawhideString
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeRawhideString",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = moderateWork,
                    Stances = GetToolMakingStances(),
                    Inputs = new[] //
                    {
                         new Input(){ Entity = "item:thunderChickenRawhide",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } },

                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:rawhideString", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask }, //
                    ProcessToolSetKey = "toolSetButcherFlesh", //
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region makeBlowpipe
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeBlowpipe",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = moderateWork,
                    Stances = GetToolMakingStances(),
                    Inputs = new[]
                    {
                         new Input(){ Entity = "item:waterCaneStem",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } },

                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:blowpipe", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    ProcessToolSetKey = "toolSetShapenSmallWood",
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region makeHammer
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeHammer",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "smithing",
                    PhysicalWorkFactor = hardWork,
                    Stances = GetSmithingStance(), // GetToolMakingStances(),
                    Inputs = new[]
                    {
                         new Input(){ Entity = "item:wroughtIron",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } },
                         new Input(){ Entity = "item:sticks",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } } 
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:hammer", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmithingBigTool },
                    ProcessToolSetKey = "toolSetImprovisedForging", //required to be made without filing in order to be able to tech up (toolbox)
                    AgentActionState = AnimAction.Mending, //note: mending is default , so it's not really necessary to put it here.
                    AgentAnimationStates = new[] { AnimModifier.Metal }//mp an attempt to make it select the standing up hammer anim at the anvil
                });
                #endregion



                #region makeStoneHammer
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeStoneHammer",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = assembleTool,
                    Stances = GetToolMakingStances(),
                    Inputs = new[]
                    {
                         new Input(){ Entity = "item:stones",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } }
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:stoneHammer", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForTinyCraftingTask },
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region makeGunStock
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeGunStock",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = moderateWork,
                    Stances = GetToolMakingStances(),
                    Inputs = new[]
                    {
                         new Input(){ Entity = "item:sticks",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } },

                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:gunStock", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    ProcessToolSetKey = "toolSetShapenSmallWood",
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region makeGunBarrelUnbored
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeGunBarrelUnbored",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "smithing",
                    PhysicalWorkFactor = hardWork,
                    Stances = GetSmithingStance(), // 
                    Inputs = new[]
                    {
                         new Input(){ Entity = "item:wroughtIron",IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1}},
                         new Input(){ Entity = "item:blisterSteel",IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1}}  
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:gunBarrelUnbored", Amount = new OutputAmount(){ NoOfItems = 1 }},
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForBigCraftingTask },//it took a long time to make rifles with this method http://firearmshistory.blogspot.dk/2010/05/barrel-making-early-barrel-making-in_29.html
                    ProcessToolSetKey = "toolSetSimpleForging",
                    AgentActionState = AnimAction.Mending, //note: mending is default , so it's not really necessary to put it here.
                    AgentAnimationStates = new[] { AnimModifier.Metal }//mp an attempt to make it select the standing up hammer anim at the anvil
                });
                #endregion

                #region makeGunBarrelSmoothLong
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeGunBarrelSmoothLong",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "smithing", //TODO machinist skill?
                    PhysicalWorkFactor = moderateWork,
                    Inputs = new[]
                    {
                         new Input(){ Entity = "item:gunBarrelUnbored",IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1}},
  
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:gunBarrelSmoothLong", Amount = new OutputAmount(){ NoOfItems = 1 }},
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },// the tool should have a lot to say here.
                    ProcessToolSetKey = "toolSetBarrelBoring", //option between simple smithy and lathe shop
                    AgentActionState = AnimAction.Mending, //note: mending is default , so it's not really necessary to put it here.
                    AgentAnimationStates = new[] { AnimModifier.Improvised } //TODO?

                });
                #endregion

                #region makeGunBarrelSmoothShort
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeGunBarrelSmoothShort",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "smithing", //TODO machinist skill?
                    PhysicalWorkFactor = moderateWork,
                    Inputs = new[]
                    {
                         new Input(){ Entity = "item:gunBarrelSmoothLong",IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1}},
  
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:gunBarrelSmoothShort", Amount = new OutputAmount(){ NoOfItems = 1 }},
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },// just cutting it shorter (or maybe expanding the caliber a bit by boring?)
                    ProcessToolSetKey = "toolSetBarrelBoring", //option between simple smithy and lathe shop
                    AgentActionState = AnimAction.Mending, //note: mending is default , so it's not really necessary to put it here.
                    AgentAnimationStates = new[] { AnimModifier.Improvised } //TODO?

                });
                #endregion

                #region makeGunBarrelRifled
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeGunBarrelRifled",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "smithing", //TODO machinist skill?
                    PhysicalWorkFactor = moderateWork, //
                    Inputs = new[]
                    {
                         new Input(){ Entity = "item:gunBarrelSmoothLong",IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1}},
  
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:gunBarrelRifled", Amount = new OutputAmount(){ NoOfItems = 1 }},
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },//
                    ProcessToolSetKey = "toolSetMetalLathe", //
                    AgentActionState = AnimAction.Mending, //note: mending is default , so it's not really necessary to put it here.
                    AgentAnimationStates = new[] { AnimModifier.Improvised } //TODO?

                });
                #endregion

                #region makeGunpowderRifle
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeGunpowderRifle",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "smithing",
                    Stances = GetSmithingStance(),
                    PhysicalWorkFactor = moderateHardWork, 
                    Inputs = new[]
                    {
                         new Input(){ Entity = "item:gunBarrelRifled",IsConsumed = false, Amount = new InputAmount() { NoOfItems = 1}}, 
                         new Input(){ Entity = "item:gunStock",IsConsumed = false, Amount = new InputAmount() { NoOfItems = 1}} ,
                         new Input(){ Entity = "item:flintlockMechanism",IsConsumed = false, Amount = new InputAmount() { NoOfItems = 1}} 
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:gunpowderRifle", Amount = new OutputAmount(){ NoOfItems = 1 }},
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },//
                    ProcessToolSetKey = "toolSetSimpleForging",
                    AgentActionState = AnimAction.Mending, //note: mending is default , so it's not really necessary to put it here.
                    AgentAnimationStates = new[] { AnimModifier.Metal }//mp an attempt to make it select the standing up hammer anim at the anvil
                });
                #endregion

                #region makeMusketoon
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeMusketoon",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "smithing",
                    Stances = GetSmithingStance(),
                    PhysicalWorkFactor = moderateHardWork, //
                    Inputs = new[]
                    {
                         new Input(){ Entity = "item:gunBarrelSmoothShort",IsConsumed = false, Amount = new InputAmount() { NoOfItems = 1}}, 
                         new Input(){ Entity = "item:gunStock",IsConsumed = false, Amount = new InputAmount() { NoOfItems = 1}} ,
                         new Input(){ Entity = "item:flintlockMechanism",IsConsumed = false, Amount = new InputAmount() { NoOfItems = 1}} 
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:musketoon", Amount = new OutputAmount(){ NoOfItems = 1 }},
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },//
                    ProcessToolSetKey = "toolSetSimpleForging",
                    AgentActionState = AnimAction.Mending, //note: mending is default , so it's not really necessary to put it here.
                    AgentAnimationStates = new[] { AnimModifier.Metal }//mp an attempt to make it select the standing up hammer anim at the anvil
                });
                #endregion

                #region makeMusket
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeMusket",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "smithing",
                    Stances = GetSmithingStance(),
                    PhysicalWorkFactor = moderateHardWork, //
                    Inputs = new[]
                    {
                         new Input(){ Entity = "item:gunBarrelSmoothLong",IsConsumed = false, Amount = new InputAmount() { NoOfItems = 1}}, 
                         new Input(){ Entity = "item:gunStock",IsConsumed = false, Amount = new InputAmount() { NoOfItems = 1}} ,
                         new Input(){ Entity = "item:flintlockMechanism",IsConsumed = false, Amount = new InputAmount() { NoOfItems = 1}} 
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:musket", Amount = new OutputAmount(){ NoOfItems = 1 }},
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },//
                    ProcessToolSetKey = "toolSetSimpleForging",
                    AgentActionState = AnimAction.Mending, //note: mending is default , so it's not really necessary to put it here.
                    AgentAnimationStates = new[] { AnimModifier.Metal }//mp an attempt to make it select the standing up hammer anim at the anvil
                });
                #endregion

                #region makeAnvil
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeAnvil",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "smithing",
                    Stances = GetSmithingStance(),  
                    PhysicalWorkFactor = hardWork,
                    Inputs = new[] 
                    {
                        new Input(){ Entity = "item:wroughtIron",  IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 5 } } 
                    },
                    Outputs = new[] 
                    {
                        new Output(){ EntityTypeToCreate = "item:anvil", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForBigCraftingTask },
                    ProcessToolSetKey = "toolSetImprovisedForging",
                    AgentActionState = AnimAction.Mending, //note: mending is default , so it's not really necessary to put it here.
                    AgentAnimationStates = new[] { AnimModifier.Metal }//mp an attempt to make it select the standing up hammer anim at the anvil
                });
                #endregion
 
                #region makeSteelHoe
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeSteelHoe",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "smithing",
                    Stances = GetSmithingStance(),
                    PhysicalWorkFactor = hardWork,
                    Inputs = new[] 
                    {
                        new Input(){ Entity = "item:blisterSteel",  IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 1 } },
                        new Input(){ Entity = "item:waterCaneStem",  IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 1 } } 
                    },
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:steelHoe", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmithingBigTool },
                    ProcessToolSetKey = "toolSetSimpleForging",
                    AgentActionState = AnimAction.Mending, //note: mending is default , so it's not really necessary to put it here.
                    AgentAnimationStates = new[] { AnimModifier.Metal }//mp an attempt to make it select the standing up hammer anim at the anvil
                });
                #endregion
 
                #region makeIronHooks
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeIronHooks",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "smithing",
                    Stances = GetSmithingStance(),  
                    PhysicalWorkFactor = moderateWork,
                    Inputs = new[] 
                    {
                        new Input(){ Entity = "item:wroughtIron",  IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 1 } } 
                    },
                    Outputs = new[] 
                    {
                        new Output(){ EntityTypeToCreate = "item:ironHooks", Amount = new OutputAmount(){ NoOfItems = 5 }} //mp a hook is such a tiny little thing..
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    ProcessToolSetKey = "toolSetImprovisedForging",
                    AgentActionState = AnimAction.Mending, //note: mending is default , so it's not really necessary to put it here.
                    AgentAnimationStates = new[] { AnimModifier.Metal }//mp an attempt to make it select the standing up hammer anim at the anvil
                });
                #endregion
                #region makeSteelKnife
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeSteelKnife",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "smithing",
                    Stances = GetSmithingStance(),  
                    PhysicalWorkFactor = hardWork,
                    Inputs = new[] 
                    {
                        new Input(){ Entity = "item:blisterSteel",  IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 1 } },
                        new Input(){ Entity = "item:sticks",  IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 1 } } 
                    },
                    Outputs = new[] 
                    {
                        new Output(){ EntityTypeToCreate = "item:steelKnife", Amount = new OutputAmount(){ NoOfItems = 2 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    ProcessToolSetKey = "toolSetSimpleForging",
                    AgentActionState = AnimAction.Mending, //note: mending is default , so it's not really necessary to put it here.
                    AgentAnimationStates = new[] { AnimModifier.Metal }//mp an attempt to make it select the standing up hammer anim at the anvil
                });
                #endregion


                #region makeTinnerSnips
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeTinnerSnips", //does not require wooden handles. It's two long metal pieces joined with a bolt.
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "smithing",
                    Stances = GetSmithingStance(),  
                    PhysicalWorkFactor = hardWork,
                    Inputs = new[] 
                    {
                        new Input(){ Entity = "item:blisterSteel",  IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 1 } } 
                    },
                    Outputs = new[] 
                    {
                        new Output(){ EntityTypeToCreate = "item:tinnerSnips", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmithingBigTool },
                    ProcessToolSetKey = "toolSetSimpleForging",
                    AgentActionState = AnimAction.Mending, //note: mending is default , so it's not really necessary to put it here.
                    AgentAnimationStates = new[] { AnimModifier.Metal }//mp an attempt to make it select the standing up hammer anim at the anvil
                });
                #endregion

                #region makeTongs
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeTongs", //does not require assembly with wooden handles. It's two long metal pieces joined with a bolt.
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "smithing",
                    Stances = GetSmithingStance(),  
                    PhysicalWorkFactor = hardWork,
                    Inputs = new[] 
                    {
                        new Input(){ Entity = "item:wroughtIron",  IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 1 } } 
                    },
                    Outputs = new[] 
                    {
                        new Output(){ EntityTypeToCreate = "item:tongs", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmithingBigTool },
                    ProcessToolSetKey = "toolSetImprovisedForging", //required to be made without filing in order to be able to tech up (toolbox)
                    AgentActionState = AnimAction.Mending, //note: mending is default , so it's not really necessary to put it here.
                    AgentAnimationStates = new[] { AnimModifier.Metal }//mp an attempt to make it select the standing up hammer anim at the anvil
                });
                #endregion

                #region makeFile
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeFile", //should actually require a hardening process, or use of steel. but simplified it.       ..does not require assembly with wooden handle
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "smithing",
                    Stances = GetSmithingStance(),  
                    PhysicalWorkFactor = hardWork,
                    Inputs = new[] 
                    {
                        new Input(){ Entity = "item:wroughtIron",  IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 1 } } 
                    },
                    Outputs = new[] 
                    {
                        new Output(){ EntityTypeToCreate = "item:file", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmithingBigTool },
                    ProcessToolSetKey = "toolSetImprovisedForging", //required to be made without filing in order to be able to tech up (toolbox)
                    AgentActionState = AnimAction.Mending, //note: mending is default , so it's not really necessary to put it here.
                    AgentAnimationStates = new[] { AnimModifier.Metal }//mp an attempt to make it select the standing up hammer anim at the anvil
                });
                #endregion

                #region makeBarClamps
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeBarClamps", //
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "smithing",
                    PhysicalWorkFactor = hardWork,
                    Inputs = new[] 
                    {
                        new Input(){ Entity = "item:wroughtIron",  IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 1 } } 
                    },
                    Outputs = new[] 
                    {
                        new Output(){ EntityTypeToCreate = "item:barClamps", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    ProcessToolSetKey = "toolSetHarderImprovisedForging", //requires a file, but is not a threaded rod.
                    AgentActionState = AnimAction.Mending, //note: mending is default , so it's not really necessary to put it here.
                    AgentAnimationStates = new[] { AnimModifier.Metal }//mp an attempt to make it select the standing up hammer anim at the anvil
                });
                #endregion

                #region makeHandDrill
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeHandDrill", //
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "smithing",
                    Stances = GetSmithingStance(),  
                    PhysicalWorkFactor = hardWork,
                    Inputs = new[] 
                    {
                        new Input(){ Entity = "item:blisterSteel",  IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 1 } } //steel for the drill bit. wrought iron for the mechanism?
                    },
                    Outputs = new[] 
                    {
                        new Output(){ EntityTypeToCreate = "item:handDrill", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForBigCraftingTask },
                    ProcessToolSetKey = "toolSetSimpleForging", //making the moving parts and the drill bit is rather advanced
                    AgentActionState = AnimAction.Mending, //note: mending is default , so it's not really necessary to put it here.
                    AgentAnimationStates = new[] { AnimModifier.Metal }//mp an attempt to make it select the standing up hammer anim at the anvil
                });
                #endregion

                #region makeHacksaw
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeHacksaw", //
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "smithing",
                    Stances = GetSmithingStance(),  
                    PhysicalWorkFactor = hardWork,
                    Inputs = new[] 
                    {
                        new Input(){ Entity = "item:blisterSteel",  IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 1 } } //steel for the sawblade. wrought iron for the handle/frame?
                    },
                    Outputs = new[] 
                    {
                        new Output(){ EntityTypeToCreate = "item:hacksaw", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmithingBigTool },
                    ProcessToolSetKey = "toolSetSimpleForging", //making the tough sawblade is rather advanced
                    AgentActionState = AnimAction.Mending, //note: mending is default , so it's not really necessary to put it here.
                    AgentAnimationStates = new[] { AnimModifier.Metal }//mp an attempt to make it select the standing up hammer anim at the anvil
                });
                #endregion

                #region makeBowSaw
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeBowSaw", //
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "smithing",
                    Stances = GetSmithingStance(),
                    PhysicalWorkFactor = hardWork,
                    Inputs = new[] 
                    {
                        new Input(){ Entity = "item:blisterSteel",  IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 1 } }, 
                        new Input(){ Entity = "item:waterCaneStem",  IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 1 } }
                    },
                    Outputs = new[] 
                    {
                        new Output(){ EntityTypeToCreate = "item:bowSaw", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmithingBigTool },
                    ProcessToolSetKey = "toolSetSimpleForging", //making the tough sawblade is rather advanced
                    AgentActionState = AnimAction.Mending,
                    AgentAnimationStates = new[] { AnimModifier.Metal }
                });
                #endregion


                #region makeBlacksmithsToolbox
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeBlacksmithsToolbox",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "menial", //just the act of collecting them.
                    PhysicalWorkFactor = assembleTool,
                    Inputs = new[]
                    {
                         new Input(){ Entity = "item:file",  IsConsumed = false, Amount = new InputAmount(){ NoOfItems = 1 } },
                         new Input(){ Entity = "item:tongs",  IsConsumed = false, Amount = new InputAmount(){ NoOfItems = 1 } },
                         new Input(){ Entity = "item:hammer",  IsConsumed = false, Amount = new InputAmount(){ NoOfItems = 1 } },
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:blacksmithsToolbox", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForInstantCraftingTask },
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region makeMetalworkersToolbox
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeMetalworkersToolbox",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "menial", //just the act of collecting them.
                    PhysicalWorkFactor = assembleTool,
                    Inputs = new[]
                    {
                         new Input(){ Entity = "item:blacksmithsToolbox",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } }, //mp yes I know. should conserve the degrade status of the hammer etc, but since we don't have repair/tool degrade system, doesn't matter yet.
                         new Input(){ Entity = "item:handDrill",  IsConsumed = false, Amount = new InputAmount(){ NoOfItems = 1 } },
                         new Input(){ Entity = "item:hacksaw",  IsConsumed = false, Amount = new InputAmount(){ NoOfItems = 1 } },
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:metalWorkersToolbox", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForInstantCraftingTask },
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region makeMetalworkersToolbox
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeCarpentersToolbox",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "menial", //just the act of collecting them.
                    PhysicalWorkFactor = assembleTool,
                    Inputs = new[]
                    {
                         new Input(){ Entity = "item:file",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } }, 
                         new Input(){ Entity = "item:handDrill",  IsConsumed = false, Amount = new InputAmount(){ NoOfItems = 1 } },
                         new Input(){ Entity = "item:hammer",  IsConsumed = false, Amount = new InputAmount(){ NoOfItems = 1 } },
                         new Input(){ Entity = "item:steelHandAxe",  IsConsumed = false, Amount = new InputAmount(){ NoOfItems = 1 } },
                         new Input(){ Entity = "item:bowSaw",  IsConsumed = false, Amount = new InputAmount(){ NoOfItems = 1 } },
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:carpentersToolbox", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForInstantCraftingTask },
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion
               


                #region makeSteelMachete
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeSteelMachete",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "smithing",
                    Stances = GetSmithingStance(),
                    PhysicalWorkFactor = hardWork,
                    Inputs = new[]
                    {
                         new Input(){ Entity = "item:blisterSteel",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } },
                         new Input(){ Entity = "item:sticks",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } }
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:steelMachete", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmithingBigTool },
                    ProcessToolSetKey = "toolSetSimpleForging",
                    AgentActionState = AnimAction.Mending, //note: mending is default , so it's not really necessary to put it here.
                    AgentAnimationStates = new[] { AnimModifier.Metal }//mp an attempt to make it select the standing up hammer anim at the anvil
                });
                #endregion

                #region makeImprovisedAxe
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeImprovisedAxe",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = sharpening,
                    Stances = GetToolMakingStances(),
                    Inputs = new[] 
                    {
                         new Input(){ Entity = "item:scrapMetal",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } },
                         new Input(){ Entity = "item:waterCaneStem",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } } 
                    },
                    Outputs = new[] 
                    {
                        new Output(){ EntityTypeToCreate = "item:improvisedHandAxe", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    ProcessToolSetKey = "toolSetMakeImprovisedTool", // "toolSetSharpenBlade",
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region makeSteelHandAxe
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeSteelHandAxe",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "smithing",
                    Stances = GetSmithingStance(),
                    PhysicalWorkFactor = hardWork,
                    Inputs = new[]
                    {
                         new Input(){ Entity = "item:blisterSteel",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } },
                         new Input(){ Entity = "item:waterCaneStem",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } },
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:steelHandAxe", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmithingBigTool },
                    ProcessToolSetKey = "toolSetSimpleForging",
                    AgentActionState = AnimAction.Mending, //note: mending is default , so it's not really necessary to put it here.
                    AgentAnimationStates = new[] { AnimModifier.Metal }//mp an attempt to make it select the standing up hammer anim at the anvil
                });
                #endregion


                #region makeImprovisedPickaxe
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeImprovisedPickaxe",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = assembleTool,
                    Inputs = new[]
                    {
                         new Input(){ Entity = "item:scrapMetal",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } }, // NA Removed item:improvisedPickaxeHead
                         new Input(){ Entity = "item:waterCaneStem",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } },
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:improvisedPickaxe", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask }, // NA was Small
                    ProcessToolSetKey = "toolSetSharpenBladeAndtoolSetAttachWoodAndMetal",
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion
 

                #region makeSteelPickaxe
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeSteelPickaxe",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "smithing",
                    Stances = GetSmithingStance(),
                    PhysicalWorkFactor = hardWork,
                    Inputs = new[]
                    {
                         new Input(){ Entity = "item:blisterSteel",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } },
                          new Input(){ Entity = "item:waterCaneStem",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } }
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:steelPickaxe", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmithingBigTool },
                    ProcessToolSetKey = "toolSetImprovisedForging",
                    AgentActionState = AnimAction.Mending, //note: mending is default , so it's not really necessary to put it here.
                    AgentAnimationStates = new[] { AnimModifier.Metal }//mp an attempt to make it select the standing up hammer anim at the anvil
                });
                #endregion


                #region makeImprovisedSpade
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeImprovisedSpade",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = sharpening,
                    Stances = GetToolMakingStances(),
                    Inputs = new[] 
                    {
                         new Input(){ Entity = "item:scrapMetal",  IsConsumed = true,Amount = new InputAmount(){ NoOfItems = 1 } },
                         new Input(){ Entity = "item:waterCaneStem",  IsConsumed = true,Amount = new InputAmount(){ NoOfItems = 1 } } 
                    },
                    Outputs = new[] 
                    {
                        new Output(){ EntityTypeToCreate = "item:improvisedSpade", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    ProcessToolSetKey = "toolSetMakeImprovisedTool",
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion



                #region makeSteelSpade
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeSteelSpade",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "smithing",
                    Stances = GetSmithingStance(),
                    PhysicalWorkFactor = hardWork,
                    Inputs = new[]
                    {
                         new Input(){ Entity = "item:blisterSteel",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } },
                         new Input(){ Entity = "item:waterCaneStem",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } }
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:steelSpade", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmithingBigTool },
                    ProcessToolSetKey = "toolSetSimpleForging",
                    AgentActionState = AnimAction.Mending, //note: mending is default , so it's not really necessary to put it here.
                    AgentAnimationStates = new[] { AnimModifier.Metal }//mp an attempt to make it select the standing up hammer anim at the anvil
                });
                #endregion



                #region makeTurnipCracker
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeTurnipCracker", //
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "smithing",
                    Stances = GetSmithingStance(),  
                    PhysicalWorkFactor = hardWork,
                    Inputs = new[]
                    {
                         new Input(){ Entity = "item:wroughtIron",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 2 } } 
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:turnipCracker",Amount = new OutputAmount(){  NoOfItems = 1}}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    ProcessToolSetKey = "toolSetHarderImprovisedForging", //requires filing
                    AgentActionState = AnimAction.Mending, //note: mending is default , so it's not really necessary to put it here.
                    AgentAnimationStates = new[] { AnimModifier.Metal }//mp an attempt to make it select the standing up hammer anim at the anvil
                });
                #endregion
                #region makeIronSpear
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeIronSpear",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "smithing",
                    PhysicalWorkFactor = assembleTool,
                    Stances = GetToolMakingStances(),
                    Inputs = new[] {
                        new Input(){ Entity = "item:waterCaneStem",  IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 1 } } ,
                        new Input(){ Entity = "item:wroughtIron",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } } },


                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:ironSpear", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    ProcessToolSetKey = "toolSetHarderImprovisedForging",
                    AgentAnimationStates = new[] { AnimModifier.Improvised }

                });
                #endregion

                #region makeIronArrow
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeIronArrow",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "smithing",
                    PhysicalWorkFactor = assembleTool,
                    Stances = GetToolMakingStances(),
                    Inputs = new[]
                    {
                        new Input(){ Entity = "item:improvisedArrowShaftBundle",  IsConsumed = false, Amount = new InputAmount(){  NoOfItems = 1 } },
                        new Input(){ Entity = "item:waterCaneLeaves",  IsConsumed = true,Amount = new InputAmount(){ NoOfItems = 1 } },
                        new Input(){ Entity = "item:wroughtIron",  IsConsumed = true,Amount = new InputAmount(){ NoOfItems = 1 } },
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:ironArrow", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    ProcessToolSetKey = "toolSetHarderImprovisedForging",
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion
             


                #region makeRoughBloomIron
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeRoughBloomIron",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "smithing",
                    Stances = GetSmithingStance(),  
                    PhysicalWorkFactor = hardWork,
                    Inputs = new[]
                    {
                         new Input(){ Entity = "item:bogOre",IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1}} 
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:roughBloomIron", Amount = new OutputAmount(){ NoOfItems = 5 }},
                 
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForBigCraftingTask },
                    ProcessToolSetKey = "toolSetImprovisedForging",
                    AgentActionState = AnimAction.Mending, //note: mending is default , so it's not really necessary to put it here.
                    AgentAnimationStates = new[] { AnimModifier.Metal }//mp an attempt to make it select the standing up hammer anim at the anvil
                });
                #endregion

                #region makeGold
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeGold",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "chemistry",
                    PhysicalWorkFactor = hardWork,
                    Inputs = new[]
                    {
                         new Input(){ Entity = "item:goldOre",IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1} }
                    },
                    Outputs = new[]
                    {
                 
                        new Output(){ EntityTypeToCreate = "item:gold", Amount = new OutputAmount(){ NoOfItems = 5 }},
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForBigCraftingTask, MultiplyByBulk = true },
                    ProcessToolSetKey = "toolSetSimpleCasting",
                    AgentActionState = AnimAction.Mending, //note: mending is default , so it's not really necessary to put it here.
                    AgentAnimationStates = new[] { AnimModifier.Metal }//mp an attempt to make it select the standing up hammer anim at the anvil
                });
                #endregion

                #region makeWroughtIron
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeWroughtIron",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "smithing",
                    Stances = GetSmithingStance(),  
                    PhysicalWorkFactor = hardWork,
                    Inputs = new[]
                    {
                         new Input(){ Entity = "item:roughBloomIron",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1/*, Substances = new[]{"iron"}*/ } } 
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:wroughtIron",Amount = new OutputAmount(){  NoOfItems = 1} /*Amount = new OutputAmount(){  Bulk = new Bulk(){InputSubstance = "iron"}}*/}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForBigCraftingTask, MultiplyByBulk = true },
                    ProcessToolSetKey = "toolSetImprovisedForging",
                    AgentActionState = AnimAction.Mending, //note: mending is default , so it's not really necessary to put it here.
                    AgentAnimationStates = new[] { AnimModifier.Metal }//mp an attempt to make it select the standing up hammer anim at the anvil
                });
                #endregion

                #region makeBlisterSteel
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeBlisterSteel",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "smithing",
                    Stances = GetSmithingStance(),  
                    PhysicalWorkFactor = hardWork,
                    WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart, //needs to 'cement' for a long time
                    Inputs = new[]
                    {
                         new Input(){ Entity = "item:wroughtIron",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 4/*, Substances = new[]{"iron"}*/ } } 
                    },
                    Outputs = new[]
                    {//
                        new Output(){ EntityTypeToCreate = "item:blisterSteel", Amount = new OutputAmount(){  NoOfItems = 4} /*Amount = new OutputAmount(){  Bulk = new Bulk(){InputSubstance = "iron"}}*/}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForLongStandaloneTask, MultiplyByBulk = true },
                    ProcessToolSetKey = "toolSetKilnBigAndSmall", //maybe the small kiln is good enough?
                    AgentActionState = AnimAction.Mending, //note: mending is default , so it's not really necessary to put it here.
                    AgentAnimationStates = new[] { AnimModifier.Metal }//mp an attempt to make it select the standing up hammer anim at the anvil
                });
                #endregion

                #region makeRareMetal
                //NA MINING CAMP MATERIAL
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeRareMetal",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = assembleTool,
                    Stances = GetToolMakingStances(),
                    WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart,
                    
                    Inputs = new[] 
                    { 
                        new Input() { Entity = "item:scandiumOre", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 3, } },
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:scandium", Amount = new OutputAmount(){  NoOfItems = 3}, ToolContainerTagsToPlaceIn = new[]{"rareMetalRefinery"}}, //
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForShortStandaloneTask },
                    ProcessToolSetKey = "toolSetRefineRareMetal", 
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                }); 
                #endregion
                #region makeRareMetal2
                //NA MINING CAMP MATERIAL
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeRareMetal2",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = assembleTool,
                    Stances = GetToolMakingStances(),
                    WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart,

                    Inputs = new[] 
                    { 
                        new Input() { Entity = "item:terbiumOre", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 3, } },
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:terbium", Amount = new OutputAmount(){  NoOfItems = 3}, ToolContainerTagsToPlaceIn = new[]{"rareMetalRefinery"}}, //
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForShortStandaloneTask },
                    ProcessToolSetKey = "toolSetRefineRareMetal", //uses a brain as one of the tools
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                }); 
                #endregion


                #region makeCharcoal
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeCharcoal",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = moderateWork,
                    WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart, //made in kiln
                    Stances = GetToolMakingStances(),
                    Inputs = new[]
                    {
                         new Input(){ Entity = "item:firewood",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } } 
                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:charcoal", Amount = new OutputAmount(){ NoOfItems = 1 },  ToolContainerTagsToPlaceIn = new[]{"kiln"} }
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForShortStandaloneTask },
                    ProcessToolSetKey = "toolSetKilnBigAndSmall", 
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region makeDryPeat
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeDryPeat",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = moderateWork,
                    Stances = standingProduction,
                    WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart, //mp 
                    Inputs = new[] 
                    { 
                        new Input() { Entity = "item:wetPeat", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 10, } }, //align with toolcontainer capacity

                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:dryPeat", Amount = new OutputAmount(){  NoOfItems = 10}, ToolContainerTagsToPlaceIn = new[]{"peatStackTool"}}, 
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForLongStandaloneTask },
                    ProcessToolSetKey = "toolSetDryingPeat",
                    AgentAnimationStates = new[] { AnimModifier.Improvised } 

                });
                #endregion

                #region makeFirewood
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeFirewood",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = moderateWork,
                    Stances = standingProduction,
                    WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart, //mp 
                    Inputs = new[] 
                    { 
                        new Input() { Entity = "item:wetFirewood", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 10, } }, //align with toolcontainer capacity

                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:firewood", Amount = new OutputAmount(){  NoOfItems = 10}, ToolContainerTagsToPlaceIn = new[]{"woodpileTool"}}, 
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForLongStandaloneTask },
                    ProcessToolSetKey = "toolSetDryingFirewood",
                    AgentAnimationStates = new[] { AnimModifier.Improvised }

                });
                #endregion

                #region makePlasticJar
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeImprovisedPlasticJar",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "bushcraft", // 
                    PhysicalWorkFactor = sittingLightWork,
                    Stances = GetToolMakingStances(),
                    Inputs = new[]
                    {
                         new Input(){ Entity = "item:panelScraps",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } } 
                    },

                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:improvisedPlasticJar", Amount = new OutputAmount(){ NoOfItems = 2 }} //making 2 because it's a big input amount.
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    ProcessToolSetKey = "toolSetShapenSmallWood", //
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion


                #region makeUnfiredClayJar
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeUnfiredClayJar",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "bushcraft", // todo
                    PhysicalWorkFactor = sittingLightWork,
                    Stances = GetToolMakingStances(),
                    Inputs = new[]
                    {
                         new Input(){ Entity = "item:clay",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } } 
                    },

                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:unfiredClayJar", Amount = new OutputAmount(){ NoOfItems = 2 }} //its a big pile of clay
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    ProcessToolSetKey = "toolSetShapePottery", //
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region makeUnfiredBulletMold
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeUnfiredBulletMold",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "chemistry", // has to do with casting
                    PhysicalWorkFactor = sittingLightWork,
                    Stances = GetToolMakingStances(),
                    Inputs = new[]
                    {
                         new Input(){ Entity = "item:clay",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } } 
                    },

                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:unfiredBulletMold", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    ProcessToolSetKey = "toolSetShapePottery", //
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region makeBulletMold
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeBulletMold",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "chemistry", // has to do with casting
                    PhysicalWorkFactor = moderateWork,
                    WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart, //made in kiln
                    Stances = GetToolMakingStances(),
                    Inputs = new[]
                    {

                         new Input(){ Entity = "item:unfiredBulletMold",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } } 
                    },

                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:bulletMold", Amount = new OutputAmount(){ NoOfItems = 1 },  ToolContainerTagsToPlaceIn = new[]{"kiln"} }//the product should appear inside the kiln.
                         
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForShortStandaloneTask },
                    ProcessToolSetKey = "toolSetKilnBigAndSmall",
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

               #region makeUnfinishedFirebricks
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeUnfinishedFirebricks",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "chemistry", // has to do with casting
                    PhysicalWorkFactor = sittingLightWork,
                    Stances = GetToolMakingStances(),
                    Inputs = new[]
                    {
                         new Input(){ Entity = "item:clay",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } } //maybe use a special clay for this.
                    },

                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:unfinishedFirebricks", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    ProcessToolSetKey = "toolSetShapePottery", //
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

              #region makeFirebricks
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeFirebricks",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "chemistry", // has to do with casting
                    PhysicalWorkFactor = moderateWork,
                    WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart, //made in kiln
                    Stances = GetToolMakingStances(),
                    Inputs = new[]
                    {

                         new Input(){ Entity = "item:unfinishedFirebricks",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } } 
                    },

                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:firebricks", Amount = new OutputAmount(){ NoOfItems = 1 },  ToolContainerTagsToPlaceIn = new[]{"kiln"} }//the product should appear inside the kiln.
                         
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForShortStandaloneTask },
                    ProcessToolSetKey = "toolSetKilnBig",
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region makeGlazedClayJar
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeGlazedClayJar",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "bushcraft", //todo
                    PhysicalWorkFactor = moderateWork,
                    WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart, //made in kiln
                    Stances = GetToolMakingStances(),
                    Inputs = new[]
                    {
                        new Input(){ Entity = "item:salt",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } } ,
                         new Input(){ Entity = "item:unfiredClayJar",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } } 
                    },

                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:clayJar", Amount = new OutputAmount(){ NoOfItems = 1 },  ToolContainerTagsToPlaceIn = new[]{"kiln"} }//the product should appear inside the kiln.
                         
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForShortStandaloneTask },
                    ProcessToolSetKey = "toolSetKilnBig",
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region makeUnfiredClayPot
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeUnfiredClayPot",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "bushcraft", // todo
                    PhysicalWorkFactor = sittingLightWork,
                    Stances = GetToolMakingStances(),
                    Inputs = new[]
                    {
                         new Input(){ Entity = "item:clay",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } } 
                    },

                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:unfiredClayPot", Amount = new OutputAmount(){ NoOfItems = 2 }} //big pile of clay
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    ProcessToolSetKey = "toolSetShapePottery", //
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region makeClayPotUnglazed
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeClayPotUnglazed",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "bushcraft", //todo
                    PhysicalWorkFactor = moderateWork,
                    WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart, //made in kiln
                    Stances = GetToolMakingStances(),
                    Inputs = new[]
                    {

                         new Input(){ Entity = "item:unfiredClayPot",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } } 
                    },

                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:clayPotUnglazed", Amount = new OutputAmount(){ NoOfItems = 1 },  ToolContainerTagsToPlaceIn = new[]{"kiln"} }//the product should appear inside the kiln.
                         
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForShortStandaloneTask }, 
                    ProcessToolSetKey = "toolSetKilnBigAndSmall",
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                //cultivating the moss/shroom in the little farm pit:
                #region makeFavorbread
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = cultivatingName,
                    KeyName = "makeFavorbread",
                    JobTypeKey = "sowingAndHarvestingJobType", //note this is different from  "craftingJobType", because it produces food from 'farming', and player wants to control that priority.
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = sittingLightWork,
                    Stances = kneelingProduction,                     
                    WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart,  
                    Inputs = new[] 
                    { 
                        new Input() { Entity = "item:blackpulp", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1, } } 

                    },
                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:favorbread", Amount = new OutputAmount(){  NoOfItems = 8}, ToolContainerTagsToPlaceIn = new[]{"favorbreadFarm"}}, 
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForVeryLongStandaloneTask },
                    ProcessToolSetKey = "toolSetFavorbreadFarm",
                    AgentAnimationStates = new[] { AnimModifier.Improvised } //

                });
                #endregion

                #region //extraction of resources from pits and mines  
                #region makeClayFromPit

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = primaryExtraction,
                    KeyName = "makeClayFromPit",
                    JobTypeKey = "extractFromDepositJobType",
                    RequiredSkill = "menial",                 
                    MoveOutputToWorkerWhenCompleted = false,
                    PhysicalWorkFactor = moderateHardWork,
                    Stances = kneelingOrStandingProduction, // using shovel is standing and digging with trowel/hands is kneeling...
                    Outputs = new[] 
                    {
                       // new Output(){ EntityTypeToCreate = "item:clay", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        new Output(){ EntityTypeToCreate = "item:clay", Amount = new OutputAmount(){ NoOfItems = 4 }} // increased items per job for less travelling
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 4 * timeForSmallHarvestingTask },
                    AgentActionState = AnimAction.Digging,
                    ProcessToolSetKey = "toolSetClayPit", // 
                });
               
                #endregion

                #region makeSaltFromMine

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = primaryExtraction,
                    KeyName = "makeSaltFromMine",
                    JobTypeKey = "extractFromDepositJobType",
                    RequiredSkill = "menial",
                    MoveOutputToWorkerWhenCompleted = false,
                    PhysicalWorkFactor = moderateHardWork,
                    Stances = kneelingOrStandingProduction, // using shovel is standing and digging with trowel/hands is kneeling...
                    Outputs = new[] 
                    {
                        new Output(){ EntityTypeToCreate = "item:salt", Amount = new OutputAmount(){ NoOfItems = 4 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 4 * timeForSmallHarvestingTask },
                    AgentActionState = AnimAction.Digging,
                    ProcessToolSetKey = "toolSetSaltMine", // 
                });

                #endregion

                #region makeBogOreFromPit

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = primaryExtraction,
                    KeyName = "makeBogOreFromPit",
                    JobTypeKey = "extractFromDepositJobType",
                    //RequiredSkill = "menial",
                    MoveOutputToWorkerWhenCompleted = false,
                    PhysicalWorkFactor = moderateHardWork,
                    Stances = kneelingOrStandingProduction, // using shovel is standing and digging with trowel/hands is kneeling...
                    Outputs = new[] 
                    {
                        new Output(){ EntityTypeToCreate = "item:bogOre", Amount = new OutputAmount(){ NoOfItems = 4 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 4 * timeForSmallHarvestingTask },
                    AgentActionState = AnimAction.Digging,
                    ProcessToolSetKey = "toolSetBogOrePit", // 
                });

                #endregion

                #region makeRareMetalOreFromPit1
                //NA MINING CAMP MATERIAL
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = primaryExtraction,
                    KeyName = "makeRareMetalOreFromPit",
                    JobTypeKey = "extractFromDepositJobType",
                    //RequiredSkill = "menial",
                    MoveOutputToWorkerWhenCompleted = false,
                    PhysicalWorkFactor = moderateHardWork,
                    Stances = kneelingOrStandingProduction, // using shovel is standing and digging with trowel/hands is kneeling...
                    Outputs = new[] 
                    {
                        new Output(){ EntityTypeToCreate = "item:scandiumOre", Amount = new OutputAmount(){ NoOfItems = 4 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 4 * timeForSmallHarvestingTask },
                    AgentActionState = AnimAction.Digging,
                    ProcessToolSetKey = "toolSetRareMetalOrePit", // 
                });

                #endregion
                #region makeRareMetalOreFromPit2
                //NA MINING CAMP MATERIAL
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = primaryExtraction,
                    KeyName = "makeRareMetalOreFromPit2",
                    JobTypeKey = "extractFromDepositJobType",
                    //RequiredSkill = "menial",
                    MoveOutputToWorkerWhenCompleted = false,
                    PhysicalWorkFactor = moderateHardWork,
                    Stances = kneelingOrStandingProduction, // using shovel is standing and digging with trowel/hands is kneeling...
                    Outputs = new[] 
                    {
                        new Output(){ EntityTypeToCreate = "item:terbiumOre", Amount = new OutputAmount(){ NoOfItems = 4 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 4 * timeForSmallHarvestingTask },
                    AgentActionState = AnimAction.Digging,
                    ProcessToolSetKey = "toolSetRareMetalOrePit2", // 
                });

                #endregion
                

                #region makeWetPeatFromBank

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = primaryExtraction,
                    KeyName = "makeWetPeatFromBank",
                    JobTypeKey = "extractFromDepositJobType",
                    RequiredSkill = "menial",
                    MoveOutputToWorkerWhenCompleted = false,
                    PhysicalWorkFactor = moderateHardWork,
                    Stances = standingProduction,//copied from establish plot since this is also firegrass turf
                    Outputs = new[] 
                    {
                        new Output(){ EntityTypeToCreate = "item:wetPeat", Amount = new OutputAmount(){ NoOfItems = 4 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 4 * timeForSmallHarvestingTask },
                    AgentActionState = AnimAction.Tilling,//copied from establish plot since this is also firegrass turf
                    ProcessToolSetKey = "toolSetPeatBank", // 
                });

                #endregion

                #endregion

                #region makeSolidMudBrick
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeSolidMudBrick",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "construction",
                    PhysicalWorkFactor = moderateWork,
                    WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart, //made in kiln
                    Stances = GetToolMakingStances(),
                    Inputs = new[]
                    {
                         new Input(){ Entity = "item:wetMudBrick",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 3 } } 
                    },

                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:solidMudBrick", Amount = new OutputAmount(){ NoOfItems = 3 },  ToolContainerTagsToPlaceIn = new[]{"kiln"} }//the product should appear inside the kiln.
                         
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForShortStandaloneTask },
                    ProcessToolSetKey = "toolSetKilnBig",
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region mudBrickMolding
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "mudBrickMolding",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "menial", //bso: it's such a simple task that I don't think it requires a special skill. mp yeah but they need to know how it should look...
                    PhysicalWorkFactor = moderateWork,
                    Stances = GetToolMakingStances(),
                    Inputs = new[]
                    {
                         new Input(){ Entity = "item:clay",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } } 
                    },

                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:wetMudBrick", Amount = new OutputAmount(){ NoOfItems = 1 }} //
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    ProcessToolSetKey = "toolSetMold",
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region makeBrickMold
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeBrickMold",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "construction",
                    PhysicalWorkFactor = moderateWork,
                    Stances = GetToolMakingStances(),
                    Inputs = new[]
                    {
                         new Input(){ Entity = "item:sticks",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } } 
                    },

                    Outputs = new[]
                    {
                        new Output(){ EntityTypeToCreate = "item:brickMold", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    ProcessToolSetKey = "toolSetShapenSmallWood",
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region chopFirewood
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = "Chopping", //
                    KeyName = "chopFirewood",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = moderateWork,
                    Stances = GetToolMakingStances(),
                    Inputs = new[] {
                         new Input(){ Entity = "item:spoakBranches",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } } 
                    },
                    Outputs = new[] 
                    {
                        new Output(){ EntityTypeToCreate = "item:wetFirewood", Amount = new OutputAmount(){ NoOfItems = 2 }}, //mp 2 because to better balance with the amount of firewood on ground.
                        new Output(){ EntityTypeToCreate = "item:spoakLeaves", Amount = new OutputAmount(){ NoOfItems = 1 }, IsWasteProduct = true}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForTinyCraftingTask },
                    ProcessToolSetKey = "toolSetChopToughWood",
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region trimSpoakBranches
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = "Trimming", //
                    KeyName = "trimSpoakBranches",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = moderateWork,
                    Stances = GetToolMakingStances(),
                    Inputs = new[] {
                         new Input(){ Entity = "item:spoakBranches",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } } },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:spoakBranchesTrimmed", Amount = new OutputAmount(){ NoOfItems = 1 }},
                        new Output(){ EntityTypeToCreate = "item:spoakLeaves", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForTinyCraftingTask },
                    ProcessToolSetKey = "toolSetChopWeakWood", //mp maybe chop tough wood. but it's mostly just removing leaves.
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion


                #region makeWoodenCookingPot
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeWoodenCookingPot",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = sharpening,
                    Stances = GetToolMakingStances(),
                    Inputs = new[] {
                         new Input(){ Entity = "item:giantHollowBud",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } } },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:woodenCookingPot", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForTinyCraftingTask },
                    ProcessToolSetKey = "toolSetShapenSmallWood",
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region makeImprovisedMetalKnife
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeImprovisedKnife",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = sharpening,
                    Stances = GetToolMakingStances(),
                    Inputs = new[] {
                         new Input(){ Entity = "item:scrapMetal",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } },
                         new Input(){ Entity = "item:sticks",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } }
                    },
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:improvisedKnife", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    ProcessToolSetKey = "toolSetMakeImprovisedTool", // "toolSetSharpenBlade",
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion



                #region makeSulfurSmokeBomb
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = cookingName,
                    KeyName = "makeSulfurSmokeBomb",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = sittingLightWork,
                    Stances = kneelingProduction,
                    Inputs = new[] { new Input() { Entity = "item:sulfurBlocks", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1 } } },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:sulfurSmokeBomb", Amount = new OutputAmount(){ NoOfItems = 1 }}  // 
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    ProcessToolSetKey = "toolSetMakeSulfurSmokeBomb", //mp also used for making sulfur powder
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion


                #region makeImprovisedTrowel
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeImprovisedTrowel",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = sharpening,
                    Stances = GetToolMakingStances(),
                    Inputs = new[] {
                         new Input(){ Entity = "item:sticks",  IsConsumed = true,Amount = new InputAmount(){ NoOfItems = 1 } } },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:improvisedTrowel", Amount = new OutputAmount(){ NoOfItems = 2 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    //mp no knife needed. must be very simple to do.
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region makeBluntKnife
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeBluntKnife",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = sharpening,
                    Stances = GetToolMakingStances(),
                    Inputs = new[] {
                         new Input(){ Entity = "item:sticks",  IsConsumed = true,Amount = new InputAmount(){ NoOfItems = 1 } } },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:bluntKnife", Amount = new OutputAmount(){ NoOfItems = 2 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    //mp no knife needed. must be very simple to do.
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion
 

                #region makeFlintKnife
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeFlintKnife",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = sharpening,
                    Stances = GetToolMakingStances(),
                    Inputs = new[] {
                         new Input(){ Entity = "item:flintRough",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 }}, 
                         new Input(){ Entity = "item:sticks",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } }
                    },
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:flintKnife", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    ProcessToolSetKey = "toolSetShapenSmallWood",  
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask /* SmallCraftingTask*/ }, 
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

              
                #region makeStrongBugNet
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeStrongBugNet",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = assembleTool,
                    Stances = GetToolMakingStances(),
                    Inputs = new[] {
                         new Input(){ Entity = "item:sticks",  IsConsumed = true,Amount = new InputAmount(){ NoOfItems = 1 } } ,
                         new Input(){ Entity = "item:textile",  IsConsumed = true,Amount = new InputAmount(){ NoOfItems = 1 } } }, // IsConsumed was false

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:strongBugNet", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForTinyCraftingTask },
                    ProcessToolSetKey = "toolSetAttachWoodAndMetal",
                    AgentAnimationStates = new[] { AnimModifier.Improvised }

                });
                #endregion

                #region makeImprovisedBowLimb
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeImprovisedBowLimb",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = sharpening,
                    Stances = GetToolMakingStances(),
                    Inputs = new[] {
                         new Input(){ Entity = "item:shadeleafBowStave",  IsConsumed = true,Amount = new InputAmount(){ NoOfItems = 1 } } },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:improvisedBowLimb", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    ProcessToolSetKey = "toolSetShapenSmallWood",
                    AgentAnimationStates = new[] { AnimModifier.Improvised }

                });
                #endregion

                #region makeImprovisedBasicSpear
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeImprovisedBasicSpear",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = sharpening,
                    Stances = GetToolMakingStances(),
                    Inputs = new[] {
                         new Input(){ Entity = "item:waterCaneStem",  IsConsumed = true,Amount = new InputAmount(){ NoOfItems = 1 } } }, //mp dec 2014. used to be: sticks

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:improvisedBasicSpear", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForTinyCraftingTask },
                    ProcessToolSetKey = "toolSetShapenSmallWood",
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region makeKnifeSpear //test
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeKnifeSpear",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = assembleTool,
                    Stances = GetToolMakingStances(),
                    Inputs = new[] {
                         new Input(){ Entity = "item:waterCaneStem",  IsConsumed = true,Amount = new InputAmount(){ NoOfItems = 1 } },
                         new Input(){ Entity = "item:advancedKnife",  IsConsumed = false,Amount = new InputAmount(){ NoOfItems = 1 } } 
                         },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:advancedKnifeSpear", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForTinyCraftingTask },
                    ProcessToolSetKey = "toolSetAttachWoodAndMetal", //mp make sure that a knife tool is NOT used, since it is already an input material. (will lead to a production bug)
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region makeImprovisedGoodSpear
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeImprovisedGoodSpear",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = assembleTool,
                    Stances = GetToolMakingStances(),
                    Inputs = new[] 
                    {
                        new Input(){ Entity = "item:waterCaneStem",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } },
                        new Input(){ Entity = "item:scrapMetal",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } } },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:improvisedGoodSpear", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    ProcessToolSetKey = "toolSetShapenSimpleSmallMetalAndtoolSetAttachWoodAndMetal",
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion


                #region makeFarmingHoe
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeFarmingHoe",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = assembleTool,
                    Stances = GetToolMakingStances(),
                    Inputs = new[] {
                         new Input(){ Entity = "item:waterCaneStem",  IsConsumed = true,Amount = new InputAmount(){ NoOfItems = 1 } },
                     new Input(){ Entity = "item:scrapMetal",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } } }, // NA Used to be "item:farmingHoeBlade"

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:farmingHoe", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    ProcessToolSetKey = "toolSetShapenSimpleSmallMetalAndtoolSetAttachWoodAndMetal",
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region makeImprovisedFlintSpear
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeImprovisedFlintSpear",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = assembleTool,
                    Stances = GetToolMakingStances(),
                    Inputs = new[] {
                         new Input(){ Entity = "item:waterCaneStem",  IsConsumed = true,Amount = new InputAmount(){ NoOfItems = 1 } },
                     new Input(){ Entity = "item:flintRough",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } } }, // NA changed this from item:flintSpearhead.

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:improvisedFlintSpear", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    ProcessToolSetKey = "toolSetAttachWoodAndMetal",
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region assembleImprovisedBow
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "assembleImprovisedBow",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = assembleTool,
                    Stances = GetToolMakingStances(),
                    Inputs = new[] {
                        new Input(){ Entity = "item:improvisedBowLimb",  IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 1 }}},



                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:improvisedBow", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    ProcessToolSetKey = "toolSetLightString",
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

     

                #region makeImprovisedArrowShaft
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeImprovisedArrowShaftBundle",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = assembleTool,
                    Stances = GetToolMakingStances(),
                    Inputs = new[] {
                         new Input(){ Entity = "item:shadeleafCanes",  IsConsumed = true,Amount = new InputAmount(){ NoOfItems = 1 } } },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:improvisedArrowShaftBundle", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    ProcessToolSetKey = "toolSetShapenSmallWoodImprovisedWithFire", // cut with knife and straighten the shoot with fire, bending it
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region makeImprovisedBasicArrow
                listOfProcessTypes.Add(new ProcessType()
                    {
                        Name = toolmakingName,
                        KeyName = "makeImprovisedBasicArrow",
                        JobTypeKey = "craftingJobType",
                        RequiredSkill = "bushcraft",
                        PhysicalWorkFactor = sharpening,
                        Stances = GetToolMakingStances(),
                        Inputs = new[] {
                        new Input(){ Entity = "item:improvisedArrowShaftBundle",  IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 1 } } },

                        Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:improvisedBasicArrow", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                        WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForInstantCraftingTask },
                        ProcessToolSetKey = "toolSetShapenSmallWood", 
                        AgentAnimationStates = new[] { AnimModifier.Improvised }
                    });
                #endregion

                #region makeImprovisedChitinousArrow
                listOfProcessTypes.Add(new ProcessType()
               {
                   Name = toolmakingName,
                   KeyName = "makeImprovisedChitinousArrow",
                   JobTypeKey = "craftingJobType",
                   RequiredSkill = "bushcraft",
                   PhysicalWorkFactor = assembleTool,
                   Stances = GetToolMakingStances(),
                   Inputs = new[] {
                        new Input(){ Entity = "item:improvisedArrowShaftBundle",  IsConsumed = false, Amount = new InputAmount(){  NoOfItems = 1 } } , // NA was 1
                        new Input(){ Entity = "item:waterCaneLeaves",  IsConsumed = true,Amount = new InputAmount(){ NoOfItems = 1 } } ,  // NA was 1             
                        new Input(){ Entity = "item:twinklerPlating",  IsConsumed = true,Amount = new InputAmount(){ NoOfItems = 1 } } }, // NA was item:chitinousArrowhead

                   Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:improvisedChitinousArrow", Amount = new OutputAmount(){ NoOfItems = 1 }} // NA was 1
                    },
                   WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                   ProcessToolSetKey = "toolSetAttachLightObjectsAndShapeSmallWood",
                   AgentAnimationStates = new[] { AnimModifier.Improvised }

               });
                #endregion

                #region makeImprovisedMetalArrow
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeImprovisedMetalArrow",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = assembleTool,
                    Stances = GetToolMakingStances(),
                    Inputs = new[] {
                        new Input(){ Entity = "item:improvisedArrowShaftBundle",  IsConsumed = false, Amount = new InputAmount(){  NoOfItems = 1 } } , // NA was 1 and was "item:improvisedArrowShaft"
                        new Input(){ Entity = "item:waterCaneLeaves",  IsConsumed = true,Amount = new InputAmount(){ NoOfItems = 1 } } , //NA was 1       //mp 2016 removed fletchings       
                        new Input(){ Entity = "item:scrapMetal",  IsConsumed = true,Amount = new InputAmount(){ NoOfItems = 1 } } }, // NA was "item:improvisedMetalArrowHead"

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:improvisedMetalArrow", Amount = new OutputAmount(){ NoOfItems = 1 }} // was 1
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    ProcessToolSetKey = "toolSetShapenSimpleSmallMetalAndCombineLightImprovisedObjects", // NA was "toolSetCombineLightImprovisedObjects"
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion
        

                #region makeImprovisedMetalHooks
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeImprovisedMetalHooks",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = sharpening,
                    Inputs = new[] {
                        new Input(){ Entity = "item:scrapMetal",  IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 1 } } },


                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:improvisedMetalHooks", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForTinyCraftingTask },
                    ProcessToolSetKey = "toolSetShapenSimpleSmallMetal",
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region makeWoodenHooks
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeWoodenHooks",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = sharpening,
                    Inputs = new[] {
                        new Input(){ Entity = "item:sticks",  IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 1 } } },


                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:woodenHooks", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForTinyCraftingTask },
                    ProcessToolSetKey = "toolSetShapenSmallWood",
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

   
                #region makeFieldLabPacked
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeFieldLabPacked", //re-assembling the cannibalized fieldLabPacked item in case player changes his mind.
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "mechanics", //
                    PhysicalWorkFactor = assembleTool,
                    Stances = GetToolMakingStances(),
                    Inputs = new[] {
                        new Input(){ Entity = "item:labComponents",  IsConsumed = false, Amount = new InputAmount(){  NoOfItems = 1 } } ,
                            },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:fieldLabPacked", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    ProcessToolSetKey = "toolSetShapenSimpleSmallMetal",
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region makeImprovedFireExtinguisher
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeImprovedFireExtinguisher",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "mechanics", 
                    PhysicalWorkFactor = assembleTool,
                    Stances = GetToolMakingStances(),
                    Inputs = new[] {
                        new Input(){ Entity = "item:basicFireExtinguisher",  IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 1 } } ,
                        new Input(){ Entity = "item:labComponents",  IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 1 } } },



                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:improvedFireExtinguisher", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    ProcessToolSetKey = "toolSetShapenSimpleSmallMetal",
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region makeSentryItem
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeSentryItem", //re-assembling the cannibalized sentry item in case player changes his mind.
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "mechanics", //
                    PhysicalWorkFactor = assembleTool,
                    Stances = GetToolMakingStances(),
                    Inputs = new[] {
                        new Input(){ Entity = "item:sentryGun",  IsConsumed = false, Amount = new InputAmount(){  NoOfItems = 1 } } ,
                        new Input(){ Entity = "item:sentryWeaponMount",  IsConsumed = false, Amount = new InputAmount(){  NoOfItems = 1 } } ,
                            },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:sentry", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    ProcessToolSetKey = "toolSetShapenSimpleSmallMetal",
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion


                #region makeSprayGunSentry Item
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeSprayGunSentry",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "mechanics", 
                    PhysicalWorkFactor = assembleTool,
                    Stances = GetToolMakingStances(),
                    Inputs = new[] {
                        new Input(){ Entity = "item:basicFireExtinguisher",  IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 1 } } ,
                        new Input(){ Entity = "item:labComponents",  IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 1 } } ,
                        new Input(){ Entity = "item:sentryWeaponMount",  IsConsumed = false, Amount = new InputAmount(){  NoOfItems = 1 } } },



                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:spraySentry", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    ProcessToolSetKey = "toolSetShapenSimpleSmallMetal",
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region makeShotgunSentry Item
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeShotgunSentry",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "mechanics",
                    PhysicalWorkFactor = assembleTool,
                    Stances = GetToolMakingStances(),
                    Inputs = new[] {
                        new Input(){ Entity = "item:gunBarrelSmoothShort",  IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 1 } } , //
                        new Input(){ Entity = "item:scrapMetal",  IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 1 } } ,
                        new Input(){ Entity = "item:sentryWeaponMount",  IsConsumed = false, Amount = new InputAmount(){  NoOfItems = 1 } } },



                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:shotgunSentry", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    ProcessToolSetKey = "toolSetShapenSimpleSmallMetal",
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region cleanUrsinix
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,  // MP: didn't put this in butcher processes, because not sure if there are unintentional consequences from that?
                    KeyName = "cleanUrsinix",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = butcher,
                    Stances = standingProduction,
                   
                    Inputs = new[] {
                        new Input(){ Entity = "item:ursinix",  IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 1 } } },


                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:cleanedUrsinix", Amount = new OutputAmount(){ NoOfItems = 3 }},
                        new Output(){ EntityTypeToCreate = "item:ursinixVenomGland", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    ProcessToolSetKey = "toolSetButcherFlesh",

                    AgentActionState = AnimAction.Butchering,
                    AgentAnimationStates = null,

                });
                #endregion


                #region makeBushDragonCartridge
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeBushDragonCartridge",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = assembleTool,
                    Stances = GetToolMakingStances(),                 
                    Inputs = new[] {
                        new Input(){ Entity = "item:bushDragonPoisonGlands",  IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 1 } } ,
                        new Input(){ Entity = "item:emptyCartridge",  IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 1 } } },


                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:bushDragonCartridge", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForTinyCraftingTask },
                    AgentAnimationStates = new[] { AnimModifier.Improvised }

                });
                #endregion
    
       

                #region makeSpoakShingles
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeSpoakShingles",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = sharpening,
                    Stances = GetToolMakingStances(),
                    Inputs = new[] {  
                        new Input(){ Entity = "item:spoakLeaves",  IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 1 } } , 
                                        }, 

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:spoakShingles", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    ProcessToolSetKey = "toolSetMakeSpoakShingles",
                    AgentAnimationStates = new[] { AnimModifier.Improvised }

                });
                #endregion

                #region makeWingweedMats
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeWingweedMats",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = assembleTool,
                    Stances = GetToolMakingStances(),
                    Inputs = new[] {  
                        new Input(){ Entity = "item:wingweedLeaves",  IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 1 } } , 
                                                        }, 

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:wingweedMat", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask /* timeForSmallCraftingTask*/ },
                    ProcessToolSetKey = "toolSetMakeSpoakShingles",
                    AgentAnimationStates = new[] { AnimModifier.Improvised }

                });
                #endregion

                #region makeTextile
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = producingName,
                    KeyName = "makeTextile",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "weaving", //
                    PhysicalWorkFactor = moderateWork,

                    Stances = GetToolMakingStances(),
                    Inputs = new[] 
                    {  
                       // new Input(){ Entity = "item:fiber", IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 5 } } 
                        new Input(){ Entity = "item:cotton", IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 5 } } 
                    },
                    Outputs = new[] 
                    {
                        new Output(){ EntityTypeToCreate = "item:textile", Amount = new OutputAmount(){ NoOfItems = 5 }, 
                            ToolContainerTypesToPlaceIn = new[]{ "item:textileWorkshopUpgrade"} // NEW
                        }
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    ProcessToolSetKey = "toolSetMakeTextile",
                    //   AgentAnimationStates = new[] { AnimModifier.Improvised }

                });
                #endregion

                #region makeCottonString
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = producingName,
                    KeyName = "makeCottonString",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "weaving", //
                    PhysicalWorkFactor = moderateWork,

                    Stances = GetToolMakingStances(),
                    Inputs = new[] 
                    {  
                        new Input(){ Entity = "item:cotton", IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 3 } } 
                    },
                    Outputs = new[] 
                    {
                        new Output(){ EntityTypeToCreate = "item:cottonString", Amount = new OutputAmount(){ NoOfItems = 3 }, 
                            ToolContainerTypesToPlaceIn = new[]{ "item:textileWorkshopUpgrade"} // NEW
                        }
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    ProcessToolSetKey = "toolSetMakeTextile",
                    //   AgentAnimationStates = new[] { AnimModifier.Improvised }

                });
                #endregion

                #region make bed frame
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = producingName,
                    KeyName = "makeBedFrame",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "carpentry", //
                    PhysicalWorkFactor = moderateWork,

                    Stances = GetToolMakingStances(),
                    Inputs = new[] 
                    {  
                        new Input(){ Entity = "item:spoakBranchesTrimmed", IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 2 } } 
                    },
                    Outputs = new[] 
                    {
                        new Output(){ EntityTypeToCreate = "item:bedFrame", Amount = new OutputAmount(){ NoOfItems = 1 }, 
                            ToolContainerTypesToPlaceIn = new[]{ "item:carpenterWorkshopUpgrade"} // NEW
                        }
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    ProcessToolSetKey = "toolSetCarpentry",
                    //   AgentAnimationStates = new[] { AnimModifier.Improvised }

                });
                #endregion

                #region make furniture
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = producingName,
                    KeyName = "makeFurniture",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "carpentry", //
                    PhysicalWorkFactor = moderateWork,

                    Stances = GetToolMakingStances(),
                    Inputs = new[] 
                    {  
                        new Input(){ Entity = "item:spoakBranchesTrimmed", IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 3 } }, 
                        new Input(){ Entity = "item:thunderChickenTannedHide", IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 2 } }, 
                    },
                    Outputs = new[] 
                    {
                        new Output(){ EntityTypeToCreate = "item:furniture", Amount = new OutputAmount(){ NoOfItems = 1 }, 
                            ToolContainerTypesToPlaceIn = new[]{ "item:carpenterWorkshopUpgrade"} // NEW
                        }
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    ProcessToolSetKey = "toolSetCarpentry",
                    //   AgentAnimationStates = new[] { AnimModifier.Improvised }

                });
                #endregion

                #region makeLoomComponents
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = producingName,
                    KeyName = "makeLoomComponents",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "carpentry", //
                    PhysicalWorkFactor = moderateWork,

                    Stances = GetToolMakingStances(),
                    Inputs = new[] 
                    {  
                        new Input(){ Entity = "item:waterCaneStem", IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 3 } },
                        new Input(){ Entity = "item:spoakBranchesTrimmed", IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 1 } } 
                    },
                    Outputs = new[] 
                    {
                        new Output(){ EntityTypeToCreate = "item:loomComponents", Amount = new OutputAmount(){ NoOfItems = 1 }, 
                            ToolContainerTypesToPlaceIn = new[]{ "item:carpenterWorkshopUpgrade"} 
                        }
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForBigCraftingTask }, //was timeForExtraBigCraftingTask
                    ProcessToolSetKey = "toolSetCarpentry",
                    //   AgentAnimationStates = new[] { AnimModifier.Improvised }

                });
                #endregion

                #region makeGaskets
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeGaskets",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "chemistry", //
                    PhysicalWorkFactor = moderateWork,
         
                    Stances = GetToolMakingStances(),
                    Inputs = new[] 
                    {  
                        new Input(){ Entity = "item:marshcotSap",  IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 5 } } , 
                        new Input(){ Entity = "item:sulfurPowder",  IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 1 } } , 
                    },

                    Outputs = new[] 
                    {
                        new Output(){ EntityTypeToCreate = "item:gaskets", Amount = new OutputAmount(){ NoOfItems = 5 }, 
                            ToolContainerTypesToPlaceIn = new[]{ "item:polymerWorkshopUpgrade"} // NEW
                        }
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    ProcessToolSetKey = "toolSetMakeRubber",
                 //   AgentAnimationStates = new[] { AnimModifier.Improvised }

                });
                #endregion

                //
               #region makeFishTrap items  


                    #region makeFishTrapBasket  item
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeFishTrapBasket",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = assembleTool,
                    Stances = GetToolMakingStances(),
                    Inputs = new[] {
                         new Input(){ Entity = "item:shadeleafCanes",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 3 } }
                    },
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:fishTrapBasket", Amount = new OutputAmount(){ NoOfItems = 1 } }
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    ProcessToolSetKey = "toolSetCordage"
                });
                    #endregion

                #region makeFishTrapHoopNet  item
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeFishTrapHoopNet",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "fishing",
                    PhysicalWorkFactor = moderateWork,
                    Stances = GetToolMakingStances(),
                    Inputs = new[] {
                         new Input(){ Entity = "item:spoakBranchesTrimmed",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } },
                         new Input(){ Entity = "item:fishingNet",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } }
                    },
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:fishTrapHoopNet", Amount = new OutputAmount(){ NoOfItems = 1 } }
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    ProcessToolSetKey = "toolSetShapenSmallWoodImprovisedWithFire"
                });
                #endregion

                #region makeFishingNet  
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeFishingNet", 
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "fishing",
                    PhysicalWorkFactor = assembleTool,
                    Stances = GetToolMakingStances(),
                    Inputs = new[] {
                         new Input(){ Entity = "item:cottonString",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } },
                    },
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:fishingNet", Amount = new OutputAmount(){ NoOfItems = 1 } }
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    ProcessToolSetKey = "toolSetMakeSpoakShingles" //because of the resin used to protect it. requires workbench//IRL weaving it only takes a few simple hand tools http://www.wikihow.com/Make-a-Handmade-Fishing-Net
                                                                    
                });
                #endregion
                #endregion


                #region MOLECULAR ASSEMBLY


                float useAssemblerWorkFactor = 0.9f * sittingLightWork;


                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeMolecularKnife",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = useAssemblerWorkFactor,
                    Stances = standingProduction,
                    Inputs = new[] {
                         new Input(){ Entity = "item:acetylene", IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } } },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:advancedKnife", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMolecularAssembly },
                    ProcessToolSetKey = "toolSetUseAssemblerPlateA"

                });

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeMolecularMachete",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = useAssemblerWorkFactor,
                    Stances = standingProduction,
                    Inputs = new[] {
                         new Input(){ Entity = "item:acetylene", IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } } },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:advancedMachete", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMolecularAssembly },
                    ProcessToolSetKey = "toolSetUseAssemblerPlateA"

                });

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeMolecularCookingPot",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = useAssemblerWorkFactor,
                    Stances = standingProduction,
                    Inputs = new[] {
                         new Input(){ Entity = "item:acetylene", IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } } },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:advancedCookingPot", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMolecularAssembly },
                    ProcessToolSetKey = "toolSetUseAssemblerPlateA"

                });


                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeRifleAmmo",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = useAssemblerWorkFactor,
                    Stances = standingProduction,
                    Inputs = new[] {
                         new Input(){ Entity = "item:acetylene", IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } },
                         new Input(){ Entity = "item:ironCanister", IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } } },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:coilRifleAmmo", Amount = new OutputAmount(){ NoOfItems = 4 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMolecularAssembly },
                    ProcessToolSetKey = "toolSetUseAssemblerPlateB"

                });



                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeAssemblerPlateA",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = useAssemblerWorkFactor,
                    Stances = standingProduction,
                    Inputs = new[] {
                         new Input(){ Entity = "item:acetylene", IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } } },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:assemblerPlateA", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMolecularPlateAssembly },
                    ProcessToolSetKey = "toolSetUseMasterAssemblerPlateA"

                });

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = toolmakingName,
                    KeyName = "makeMasterAssemblerPlateA",
                    JobTypeKey = "craftingJobType",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = useAssemblerWorkFactor,
                    Stances = standingProduction,
                    Inputs = new[] {
                         new Input(){ Entity = "item:acetylene", IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } } },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:masterAssemblerPlateA", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMolecularPlateAssembly },
                    ProcessToolSetKey = "toolSetUseMasterAssemblerPlateA"

                });

                #endregion

                #endregion



                #region Pseudo processes - only for feedback
                #region hunting/killing
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = huntingName,
                    KeyName = "huntingBinalRat",
                    RequiredSkill = "hunting",
                    Inputs = new[] { new Input() { Entity = "entity:binalRat", Amount = new InputAmount() { NoOfItems = 1 } } },
                    IsPseudoProcess = true,
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:binalRatCarcass", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 0.03f },  //what does this number mean? is it used?? -MP

                });


                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = huntingName,
                    KeyName = "huntingTurnip",
                    RequiredSkill = "hunting",
                    Inputs = new[] { new Input() { Entity = "entity:turnip", Amount = new InputAmount() { NoOfItems = 1 } } },
                    IsPseudoProcess = true,
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:turnipCarcass", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 0.03f },  //what does this number mean? is it used?? -MP

                });



                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = huntingName,
                    KeyName = "huntingPygmyThunderChicken",
                    RequiredSkill = "hunting",
                    Inputs = new[] { new Input() { Entity = "entity:pygmyThunderChicken", Amount = new InputAmount() { NoOfItems = 1 } } },
                    IsPseudoProcess = true,
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:thunderChickenCarcass", Amount = new OutputAmount(){ NoOfItems = 1 }} //not used: "item:pygmyThunderChickenCarcass"
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 0.03f },

                });
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = huntingName,
                    KeyName = "huntingWhiteThunderChicken",
                    RequiredSkill = "hunting",
                    Inputs = new[] { new Input() { Entity = "entity:whiteThunderChicken", Amount = new InputAmount() { NoOfItems = 1 } } },
                    IsPseudoProcess = true,
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:thunderChickenCarcass", Amount = new OutputAmount(){ NoOfItems = 1 }} //
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 0.03f },

                });

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = huntingName,
                    KeyName = "huntingBulkyThunderChicken",
                    RequiredSkill = "hunting",
                    Inputs = new[] { new Input() { Entity = "entity:studdedThunderChicken", Amount = new InputAmount() { NoOfItems = 1 } } },
                    IsPseudoProcess = true,
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:thunderChickenCarcass", Amount = new OutputAmount(){ NoOfItems = 1 }} //not used: "item:bulkyThunderChickenCarcass"
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 0.03f },

                });

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = huntingName,
                    KeyName = "huntingThinThunderChicken",
                    RequiredSkill = "hunting",
                    Inputs = new[] { new Input() { Entity = "entity:bajingan", Amount = new InputAmount() { NoOfItems = 1 } } },
                    IsPseudoProcess = true,
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:thunderChickenCarcass", Amount = new OutputAmount(){ NoOfItems = 1 }} //not used: "item:thinThunderChickenCarcass"
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 0.03f },

                });
                //////////////////////



                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = huntingName,
                    KeyName = "huntingTwinkler",
                    RequiredSkill = "hunting",
                    Inputs = new[] { new Input() { Entity = "entity:twinkler", Amount = new InputAmount() { NoOfItems = 1 } } },
                    IsPseudoProcess = true,
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:quaditeCarcass", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 0.03f },

                });
                #region huntingLeafcutter
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = huntingName,
                    KeyName = "huntingLeafcutter",
                    RequiredSkill = "hunting",
                    Inputs = new[] { new Input() { Entity = "entity:fieldQuadite", Amount = new InputAmount() { NoOfItems = 1 } } },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:leafcutterCarcass", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 0.03f },

                });
                #endregion

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = huntingName,
                    KeyName = "huntingBushDragon",
                    RequiredSkill = "hunting",
                    Inputs = new[] { new Input() { Entity = "entity:bushDragon", Amount = new InputAmount() { NoOfItems = 1 } } },
                    IsPseudoProcess = true,
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:bushDragonCarcass", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 0.03f },

                });

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = huntingName,
                    KeyName = "huntingPatrician",
                    RequiredSkill = "hunting",
                    Inputs = new[] { new Input() { Entity = "entity:patrician", Amount = new InputAmount() { NoOfItems = 1 } } },
                    IsPseudoProcess = true,
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:patricianCarcass", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 0.03f },

                });

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = huntingName,
                    KeyName = "huntingForestGuardian",
                    RequiredSkill = "hunting",
                    Inputs = new[] { new Input() { Entity = "entity:forestGuardian", Amount = new InputAmount() { NoOfItems = 1 } } },
                    IsPseudoProcess = true,
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:forestGuardianCarcass", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 0.03f },

                });

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = huntingName,
                    KeyName = "huntingWhipjaw",
                    RequiredSkill = "hunting",
                    Inputs = new[] { new Input() { Entity = "entity:whipjaw", Amount = new InputAmount() { NoOfItems = 1 } } },
                    IsPseudoProcess = true,
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:whipjawCarcass", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 0.03f },

                });

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = huntingName,
                    KeyName = "huntingLesserWhipjaw",
                    RequiredSkill = "hunting",
                    Inputs = new[] { new Input() { Entity = "entity:lesserWhipjaw", Amount = new InputAmount() { NoOfItems = 1 } } },
                    IsPseudoProcess = true,
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:whipjawCarcass", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 0.03f },

                });
                #region huntingDemonTree
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = huntingName,
                    KeyName = "huntingDemonTree",
                    RequiredSkill = "hunting",
                    Inputs = new[] { new Input() { Entity = "entity:spoakDendront", Amount = new InputAmount() { NoOfItems = 1 } } },
                    IsPseudoProcess = true,
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:demontreeCarcass", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 0.03f },

                });
                #endregion
                #region huntingSwampDemonTree
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = huntingName,
                    KeyName = "huntingSwampDemonTree",
                    RequiredSkill = "hunting",
                    Inputs = new[] { new Input() { Entity = "entity:swampDendront", Amount = new InputAmount() { NoOfItems = 1 } } },
                    IsPseudoProcess = true,
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:swampDemonTreeCarcass", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 0.03f },

                });
                #endregion
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = huntingName,
                    KeyName = "huntingMegapod",
                    RequiredSkill = "hunting",
                    Inputs = new[] { new Input() { Entity = "entity:megapod", Amount = new InputAmount() { NoOfItems = 1 } } },
                    IsPseudoProcess = true,
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:megapodCarcass", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 0.03f },

                });

                #endregion

            
            
                #region farming
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = growingName,
                    KeyName = "growingGlassyCreeperPodsInFarmPlot",
                    RequiredSkill = "farming",
                   // TierOrArea = TierOrArea = new Policies.TierOrArea() { Tier = "basic", Area = RatingTypes.Food },
                    IsPseudoProcess = true,
                    Inputs = new[] { new Input() { Entity = "item:glassyCreeperPods", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1 } } },
                    ProcessToolSetKey = "toolSetPseudoFarmplot",
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:glassyCreeperPods", Amount = new OutputAmount(){ NoOfItems = 5 }}
                        }

                });

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = growingName,
                    KeyName = "growingCrystalBerriesInFarmPlot",
                    RequiredSkill = "farming",
                   // TierOrArea = new Policies.TierOrArea() { Tier = "basic", Area = RatingTypes.Food },
                    IsPseudoProcess = true,
                    Inputs = new[] { new Input() { Entity = "item:crystalBerries", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1 } } },
                    ProcessToolSetKey = "toolSetPseudoFarmplot",
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:crystalBerries", Amount = new OutputAmount(){ NoOfItems = 5 }}
                        }
                });

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = growingName,
                    KeyName = "growingCottonInFarmPlot",
                    RequiredSkill = "farming",
                    IsPseudoProcess = true,
                    Inputs = new[] { new Input() { Entity = "item:cotton", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1 } } },
                    ProcessToolSetKey = "toolSetPseudoFarmplot",
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:cotton", Amount = new OutputAmount(){ NoOfItems = 5 }}
                        }       
                });


                // sowing in greenhouse is without tools. Also the harvesting tools are different.
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = growingName,
                    KeyName = "growingGlassyCreeperPodsInGreenhouse",
                    RequiredSkill = "farming",
                    IsPseudoProcess = true,
                    Inputs = new[] { new Input() { Entity = "item:glassyCreeperPods", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1 } } },
                    ProcessToolSetKey = "toolSetPseudoGreenhouse",
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:glassyCreeperPods", Amount = new OutputAmount(){ NoOfItems = 5 }}
                        },
                    // WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 0.03f },  

                });

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = growingName,
                    KeyName = "growingCrystalBerriesInGreenhouse",
                    RequiredSkill = "farming",
                    IsPseudoProcess = true,
                    Inputs = new[] { new Input() { Entity = "item:crystalBerries", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1 } } },
                    ProcessToolSetKey = "toolSetPseudoGreenhouse",
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:crystalBerries", Amount = new OutputAmount(){ NoOfItems = 5 }}
                        }

                });

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = growingName,
                    KeyName = "growingFingerFruitInGreenhouse",
                    RequiredSkill = "farming",
                    IsPseudoProcess = true,
                    Inputs = new[] { new Input() { Entity = "item:fingerFruit", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1 } } },
                    ProcessToolSetKey = "toolSetPseudoGreenhouse",
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:fingerFruit", Amount = new OutputAmount(){ NoOfItems = 5 }}
                        }

                });
                #endregion

                #region Fishing
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = fishingName,
                    KeyName = "catchingCarbonTail",
                    RequiredSkill = "fishing",
                    IsPseudoProcess = true,      
                    ProcessToolSetKey = "toolSetCarbonTailTraps",
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:carbonTail", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        }

                });

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = fishingName,
                    KeyName = "catchingStreakFin",
                    RequiredSkill = "fishing",
                    IsPseudoProcess = true,
                    ProcessToolSetKey = "toolSetStreakFinTraps",
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:streakFin", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        }

                });
                #endregion
            
 
                #endregion
           

                #region ///Gathering///

                // the first half needs reviewing for stances and missing anim state!  ...do you mean put in "stance kneeling" instead of "Low" ? -MP feb 2014
                // I want to not be able to hunt diamondbird and binalrat. why do those options appear in UI if they are correctly not defined here


                float pickingFruit = 1.2f * sittingLightWork;

                listOfProcessTypes.Add(new ProcessType()
              {
                  Name = harvestingName,
                  KeyName = "harvestBlackpulp",
                  RequiredSkill = "menial",
                  JobTypeKey = "gatherFoodJobType",
                  IsGathering = true,
                  MoveOutputToWorkerWhenCompleted = true,
                  PhysicalWorkFactor = pickingFruit,
                  Stances = kneelingProduction,
                
                  Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:blackpulp", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                  WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallHarvestingTask },
                  AgentActionState = AnimAction.Harvesting,
              });


                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = harvestingName,
                    KeyName = "harvestFavorbread", //harvesting from the wild
                    RequiredSkill = "menial",
                    JobTypeKey = "gatherFoodJobType",
                    IsGathering = true,
                    MoveOutputToWorkerWhenCompleted = true,
                    PhysicalWorkFactor = pickingFruit,
                    Stances = kneelingProduction,

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:favorbread", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallHarvestingTask },
                    AgentActionState = AnimAction.Harvesting,
                });



                CreateHarvestSpoakBranches(listOfProcessTypes);

/* we harvest branches now
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = harvestingName,
                    KeyName = "harvestSpoakLeaves",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = moderateWork,
                    Stances = standing,
                 
                    Inputs = new[] { new Input() { Entity = "tree:spoak", Amount = new InputAmount() { NoOfItems = 1 } } },

                    Outputs = new[] {
                        new Output(){ EntityType = "item:spoakLeaves", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysOfWorkNeeded = timeForTinyHarvestingTask },
                    Tools = "harvestSmallBranches",
                    AgentActionState = AnimAction.Harvesting,
                    AgentAnimationStates = new[] { AnimModifier.Low }
                });
*/



                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = harvestingName,
                    KeyName = "harvestWaterCaneLeaves",
                    RequiredSkill = "menial",
                    IsGathering = true,
                    JobTypeKey = "gatherMaterialsJobType",
                    MoveOutputToWorkerWhenCompleted = true, 
                    PhysicalWorkFactor = pickingFruit,
                  
                    Inputs = new[] { new Input() { Entity = "tree:riveraxle", Amount = new InputAmount() { NoOfItems = 1 } } },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:waterCaneLeaves", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForTinyHarvestingTask },
                    ProcessToolSetKey = "toolSetHarvestSmallBranches",
                    AgentActionState = AnimAction.Harvesting,
                    AgentAnimationStates = new[] { AnimModifier.Low }
                });

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = harvestingName,
                    KeyName = "harvestWaterCaneStem",
                    RequiredSkill = "menial",
                    IsGathering = true,
                    JobTypeKey = "gatherMaterialsJobType",
                    MoveOutputToWorkerWhenCompleted = false, // seems more natural to leave them on the ground                  
                    PhysicalWorkFactor = moderateHardWork,
                 
                    Inputs = new[] { new Input() { Entity = "tree:riveraxle", Amount = new InputAmount() { NoOfItems = 1 } } },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:waterCaneStem", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForTinyHarvestingTask },
                    ProcessToolSetKey = "toolSetChopWeakWood", // changed from "chopToughWood" MP it took too long with a diamond knife
                    AgentActionState = AnimAction.Harvesting,
                    AgentAnimationStates = new[] { AnimModifier.Low }
                });

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = gatheringName,
                    KeyName = "harvestWaterCaneSeeds",
                    RequiredSkill = "menial",
                    IsGathering = true,
                    JobTypeKey = "gatherFoodJobType",
                    MoveOutputToWorkerWhenCompleted = true,                 
                    PhysicalWorkFactor = pickingFruit,
                  
                    Inputs = new[] { new Input() { Entity = "tree:riveraxle", Amount = new InputAmount() { NoOfItems = 1 } } },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:waterCaneSeeds", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForTinyHarvestingTask },
                    AgentActionState = AnimAction.Harvesting
                    
                });



                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = gatheringName,
                    KeyName = "harvestShadeleafBowStave",
                    RequiredSkill = "menial",
                    IsGathering = true,
                    JobTypeKey = "gatherMaterialsJobType",
                    MoveOutputToWorkerWhenCompleted = true, 
                    PhysicalWorkFactor = pickingFruit,
                 
                    Inputs = new[] { new Input() { Entity = "tree:shadeleaf", Amount = new InputAmount() { NoOfItems = 1 } } },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:shadeleafBowStave", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForTinyHarvestingTask },
                    ProcessToolSetKey = "toolSetChopWeakWood",
                    AgentActionState = AnimAction.Harvesting,
                    AgentAnimationStates = new[] { AnimModifier.Low }
                });


                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = harvestingName,
                    KeyName = "harvestShadeleafCanes",
                    RequiredSkill = "menial",
                    MoveOutputToWorkerWhenCompleted = false, // seems more natural to leave them on the ground                  
                    IsGathering = true,
                    JobTypeKey = "gatherMaterialsJobType",
                    PhysicalWorkFactor = moderateHardWork,
                 
                    Inputs = new[] { new Input() { Entity = "tree:shadeleaf", Amount = new InputAmount() { NoOfItems = 1 } } },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:shadeleafCanes", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForTinyHarvestingTask },
                    ProcessToolSetKey = "toolSetChopWeakWood",
                    AgentActionState = AnimAction.Harvesting,
                    AgentAnimationStates = new[] { AnimModifier.Low }
                });



                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = harvestingName,
                    KeyName = "harvestShadeleafResin",
                    RequiredSkill = "menial",
                    MoveOutputToWorkerWhenCompleted = true,                   
                    IsGathering = true,
                    JobTypeKey = "gatherMaterialsJobType",
                    PhysicalWorkFactor = sittingLightWork,
                    Stances = kneelingOrStandingProduction,
               
                    Inputs = new[] { new Input() { Entity = "tree:shadeleaf", Amount = new InputAmount() { NoOfItems = 1 } } },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:shadeleafResin", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForTinyHarvestingTask },
                    AgentActionState = AnimAction.Harvesting,
                    AgentAnimationStates = new[] { AnimModifier.Low }
                });


                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = gatheringName,
                    KeyName = "harvestGiantHollowBud",
                    RequiredSkill = "menial",
                    IsGathering = true,
                    JobTypeKey = "gatherMaterialsJobType",
                    MoveOutputToWorkerWhenCompleted = true,                   
                    PhysicalWorkFactor = pickingFruit,
                    Stances = kneelingOrStandingProduction,

                    Inputs = new[] { new Input() { Entity = "tree:gianthollow", Amount = new InputAmount() { NoOfItems = 1 } } },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:giantHollowBud", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForTinyHarvestingTask },
                    AgentActionState = AnimAction.Harvesting,
                    AgentAnimationStates = new[] { AnimModifier.Low }
                });


                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = gatheringName,
                    KeyName = "harvestWingweedLeaves",
                    RequiredSkill = "menial",
                    IsGathering = true,
                    JobTypeKey = "gatherMaterialsJobType",
                    MoveOutputToWorkerWhenCompleted = true,                   
                    PhysicalWorkFactor = moderateWork,
                 
                    Inputs = new[] { new Input() { Entity = "tree:wingweed", Amount = new InputAmount() { NoOfItems = 1 } } },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:wingweedLeaves", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForTinyHarvestingTask },
                    ProcessToolSetKey = "toolSetHarvestSmallBranches",
                    AgentActionState = AnimAction.Harvesting,
                    AgentAnimationStates = new[] { AnimModifier.Low }
                });


                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = harvestingName,
                    KeyName = "harvestDaysheenLeaves",
                    RequiredSkill = "menial",
                    IsGathering = true,
                    JobTypeKey = "gatherMaterialsJobType",
                    MoveOutputToWorkerWhenCompleted = false, // seems more natural to leave them on the ground                  
                    PhysicalWorkFactor = moderateWork,                 
                    Inputs = new[] { new Input() { Entity = "tree:daysheen", Amount = new InputAmount() { NoOfItems = 1 } } },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:daysheenLeaves", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForTinyHarvestingTask },
                    ProcessToolSetKey = "toolSetChopWeakWood",
                    AgentActionState = AnimAction.Harvesting,
                    AgentAnimationStates = new[] { AnimModifier.Low }
                });

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = gatheringName,
                    KeyName = "harvestVines",
                    RequiredSkill = "menial",
                    IsGathering = true,
                    JobTypeKey = "gatherMaterialsJobType",
                    MoveOutputToWorkerWhenCompleted = true, 
                    PhysicalWorkFactor = moderateWork,
                 
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:vine", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForTinyHarvestingTask },
                    ProcessToolSetKey = "toolSetChopWeakWood",
                    AgentActionState = AnimAction.Harvesting,
                    AgentAnimationStates = new[] { AnimModifier.Low }
                });

                #region harvest marshcot sap
                listOfProcessTypes.Add(new ProcessType() //same as below
                {
                    Name = harvestingName,
                    KeyName = "harvestMarshcotSapMarshcotFlower",
                    RequiredSkill = "bushcraft",
                    IsGathering = true,
                    WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart,    //tapping bucket              
                    PhysicalWorkFactor = moderateWork,
                    Stances = kneelingProduction, 

                    Inputs = new[] { new Input() { Entity = "tree:marshcotflower", Amount = new InputAmount() { NoOfItems = 1 } } },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:marshcotSap", Amount = new OutputAmount(){ NoOfItems = 1 },  ToolContainerTagsToPlaceIn = new[]{"liquidContainerNoHeat"}} //
                        },                                                  //OutputAmount NoOfItems = 2 ....NA - changed back to 1, as an check in FoW is added -MP made bigger output because the standalone bucket can only extract once. so I compensate for that.
                        WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForShortStandaloneTask }, //medium is very slow??
                    ProcessToolSetKey = "toolSetHarvestSap", // non-hand tool - the item must have ToolHandling = ToolHandlingType.Stationary, specified in itemloader.
                    AgentActionState = AnimAction.Harvesting, 

                });


                listOfProcessTypes.Add(new ProcessType() //same as above
                {
                    Name = harvestingName,
                    KeyName = "harvestMarshcotSapMarshcotLeaf", 
                    RequiredSkill = "bushcraft",
                    IsGathering = true,
                    WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart, //tapping bucket
                    PhysicalWorkFactor = moderateWork,
                    Stances = kneelingProduction,

                    Inputs = new[] { new Input() { Entity = "tree:marshcotflower", Amount = new InputAmount() { NoOfItems = 1 } } },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:marshcotSap", Amount = new OutputAmount(){ NoOfItems = 1 },  ToolContainerTagsToPlaceIn = new[]{"liquidContainerNoHeat"}} //
                        },                                            //OutputAmount NoOfItems = 2 ....NA - changed back to 1, as an check in FoW is added -MP made bigger output because the standalone bucket can only extract once. so I compensate for that.
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForShortStandaloneTask }, //
                    ProcessToolSetKey = "toolSetHarvestSap", // non-hand tool - the item must have ToolHandling = ToolHandlingType.Stationary, specified in itemloader.
                    AgentActionState = AnimAction.Harvesting, 
                });
                #endregion

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = gatheringName,
                    KeyName = "gatherUrsinix",
                    PhysicalWorkFactor = 1.2f * sittingLightWork,
                    RequiredSkill = "bushcraft",
                    IsGathering = true,
                    JobTypeKey = "gatherFoodJobType",
                    MoveOutputToWorkerWhenCompleted = true,                   
                    Stances = kneelingProduction,
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:ursinix", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForTinyHarvestingTask },
                    ProcessToolSetKey = "toolSetFishAlabasterRay", //spear
                    AgentActionState = AnimAction.Fishing //I use spear fishing for this..
                });


                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = catchingName,
                    KeyName = "catchNeonHornets",
                    PhysicalWorkFactor = 1.2f * sittingLightWork,
                    RequiredSkill = "bushcraft",
                    IsGathering = true,
                    JobTypeKey = "gatherMaterialsJobType",
                    MoveOutputToWorkerWhenCompleted = true,                   
                    Stances = kneelingOrStandingProduction,
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:neonHornetsLive", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForTinyHarvestingTask },
                    ProcessToolSetKey = "toolSetStrongBugNet",
                    AgentActionState = AnimAction.Harvesting
                });

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = catchingName,
                    KeyName = "catchPigFlies",
                    PhysicalWorkFactor = 1.2f * sittingLightWork,
                    RequiredSkill = "bushcraft",
                    IsGathering = true,
                    JobTypeKey = "gatherMaterialsJobType",
                    MoveOutputToWorkerWhenCompleted = true,                   
                    Stances = kneelingOrStandingProduction,
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:pigFliesLive", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForTinyHarvestingTask },
                    ProcessToolSetKey = "toolSetStrongBugNet",
                    AgentActionState = AnimAction.Harvesting
                });


                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = catchingName,
                    KeyName = "catchPhantomWeaver",
                    PhysicalWorkFactor = 1.2f * sittingLightWork,
                    RequiredSkill = "bushcraft",
                    IsGathering = true,
                    JobTypeKey = "gatherFoodJobType",
                    MoveOutputToWorkerWhenCompleted = true, 
                    Stances = kneelingOrStandingProduction, 
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:phantomWeaver", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForTinyHarvestingTask },
                    ProcessToolSetKey = "toolSetStrongBugNet",
                    AgentActionState = AnimAction.Harvesting,
                    AgentAnimationStates = new[] { AnimModifier.Low }
                });


                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = catchingName,
                    KeyName = "catchWebWing",
                    PhysicalWorkFactor = 1.2f * sittingLightWork,
                    RequiredSkill = "bushcraft",
                    IsGathering = true,
                    JobTypeKey = "gatherFoodJobType",
                    MoveOutputToWorkerWhenCompleted = true, 
                    Stances = kneelingOrStandingProduction,
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:webWing", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForTinyHarvestingTask },
                    ProcessToolSetKey = "toolSetStrongBugNet",
                    AgentActionState = AnimAction.Harvesting
                });


                

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = fishingName,
                    KeyName = "fishMinnows",
                    RequiredSkill = "fishing",
                    IsGathering = true,
                    JobTypeKey = "gatherFoodJobType",
                    MoveOutputToWorkerWhenCompleted = true, 
                    PhysicalWorkFactor = fishing,
                    Stances = kneelingProduction,
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:minnowsLive", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForTinyHarvestingTask },
                    ProcessToolSetKey = "toolSetFishMinnows",
                    AgentActionState = AnimAction.Harvesting,
                    AgentAnimationStates = new[] { AnimModifier.Low }

                });


                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = fishingName,
                    KeyName = "fishAlabasterRay",
                    RequiredSkill = "fishing",
                    IsGathering = true,
                    JobTypeKey = "gatherFoodJobType",
                    MoveOutputToWorkerWhenCompleted = true, 
                    PhysicalWorkFactor = fishing,
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:alabasterRay", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallHarvestingTask },
                    ProcessToolSetKey = "toolSetFishAlabasterRay",
                    AgentActionState = AnimAction.Fishing,

                });


                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = fishingName,
                    KeyName = "fishDaggermouth",
                    RequiredSkill = "fishing",
                    IsGathering = true,
                    JobTypeKey = "gatherFoodJobType",
                    MoveOutputToWorkerWhenCompleted = true, 
                    PhysicalWorkFactor = fishing,
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:daggermouth", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallHarvestingTask },
                    AgentActionState = AnimAction.Fishing,
                    ProcessToolSetKey = "toolSetFishAlabasterRay"


                });


                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = fishingName,
                    KeyName = "fishStreakFin",
                    RequiredSkill = "fishing",
                    IsGathering = true,
                    JobTypeKey = "gatherFoodJobType",
                    MoveOutputToWorkerWhenCompleted = true, 
                    PhysicalWorkFactor = fishing,
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:streakFin", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallHarvestingTask },
                    ProcessToolSetKey = "toolSetFishStreakFin",
                    AgentActionState = AnimAction.Fishing,

                });


                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = fishingName,
                    KeyName = "fishCarbonTail",
                    RequiredSkill = "fishing",
                    IsGathering = true,
                    JobTypeKey = "gatherFoodJobType",
                    MoveOutputToWorkerWhenCompleted = true, 
                    PhysicalWorkFactor = fishing,
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:carbonTail", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallHarvestingTask },
                    ProcessToolSetKey = "toolSetFishCarbonTail",
                    AgentActionState = AnimAction.Fishing,
                });
            
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = gatheringName,
                    KeyName = "gatherFirewood",
                    RequiredSkill = "grasping", // "menial",
                    IsGathering = true,
                    JobTypeKey = "gatherFuelJobType",
                    MoveOutputToWorkerWhenCompleted = true, 
                    PhysicalWorkFactor = walkingLightWork,
                    Stances = kneelingProduction,
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:firewood", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallHarvestingTask },
                     AgentActionState = AnimAction.Harvesting

                });
                
             
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = harvestingName,
                    KeyName = "gatherHexapineLeaves",
                    RequiredSkill = "grasping",
                    IsGathering = true,
                    JobTypeKey = "gatherFoodJobType",
                    MoveOutputToWorkerWhenCompleted = true, 
                    PhysicalWorkFactor = walkingLightWork,
                    Stances = kneelingProduction,
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:hexapineLeaves", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallHarvestingTask },                    
                    AgentActionState = AnimAction.Harvesting/*,
                    AgentAnimationStates = new AnimModifier[] { AnimModifier.Kneeling }*/

                });

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = gatheringName,
                    KeyName = "gatherSticks",
                    RequiredSkill = "grasping", // null,
                    IsGathering = true,
                    JobTypeKey = "gatherMaterialsJobType",
                    MoveOutputToWorkerWhenCompleted = true, 
                    PhysicalWorkFactor = walkingLightWork,
                    Stances = kneelingProduction,
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:sticks", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallHarvestingTask },
                    AgentActionState = AnimAction.Harvesting/*,
                    AgentAnimationStates = new AnimModifier[] { AnimModifier.Kneeling }*/

                });



                #region digging
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = miningName,
                    KeyName = "gatherGoldOre",
                    RequiredSkill = "menial",
                    IsGathering = true,
                    JobTypeKey = "gatherMaterialsJobType",
                    PhysicalWorkFactor = moderateHardWork,
                    Stances = kneelingOrStandingProduction, // using shovel is standing and digging with trowel/hands is kneeling...
                    MoveOutputToWorkerWhenCompleted = false, 
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:goldOre", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallHarvestingTask },
                    AgentActionState = AnimAction.Digging,
                    ProcessToolSetKey = "toolSetDiggingSoil",
                });


                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = miningName,
                    KeyName = "gatherBogOre",
                    RequiredSkill = "menial",
                    IsGathering = true,
                    JobTypeKey = "gatherMaterialsJobType",
                    MoveOutputToWorkerWhenCompleted = false,
                    PhysicalWorkFactor = moderateHardWork,
                    Stances = kneelingOrStandingProduction, // using shovel is standing and digging with trowel/hands is kneeling...
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:bogOre", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallHarvestingTask },
                    AgentActionState = AnimAction.Digging,
                    ProcessToolSetKey = "toolSetDiggingSoil",
                });

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = miningName,
                    KeyName = "gatherPodlac",
                    RequiredSkill = "menial",
                    IsGathering = true,
                    JobTypeKey = "gatherMaterialsJobType",
                    PhysicalWorkFactor = moderateHardWork,
                    Stances = kneelingOrStandingProduction, // using shovel is standing and digging with trowel/hands is kneeling...
                    MoveOutputToWorkerWhenCompleted = false,
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:podlacUnrefined", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallHarvestingTask },
                    AgentActionState = AnimAction.Digging,
                    ProcessToolSetKey = "toolSetDiggingSoil",
                });

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = gatheringName,
                    KeyName = "gatherGuano",
                    RequiredSkill = "menial",
                    IsGathering = true,
                    JobTypeKey = "gatherMaterialsJobType",
                    MoveOutputToWorkerWhenCompleted = false, 
                    PhysicalWorkFactor = moderateHardWork,
                    Stances = kneelingOrStandingProduction, // using shovel is standing and digging with trowel/hands is kneeling...
                    Outputs = new[] 
                    {
                        new Output(){ EntityTypeToCreate = "item:guano", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallHarvestingTask },
                    AgentActionState = AnimAction.Digging,
                    ProcessToolSetKey = "toolSetDiggingSoil",
                });

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = gatheringName,
                    KeyName = "gatherClay",
                    RequiredSkill = "menial",
                    IsGathering = true,
                    JobTypeKey = "gatherMaterialsJobType",
                    MoveOutputToWorkerWhenCompleted = false,
                    PhysicalWorkFactor = moderateHardWork,
                    Stances = kneelingOrStandingProduction, // using shovel is standing and digging with trowel/hands is kneeling...
                    Outputs = new[] 
                    {
                        new Output(){ EntityTypeToCreate = "item:clay", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallHarvestingTask },
                    AgentActionState = AnimAction.Digging,
                    ProcessToolSetKey = "toolSetDiggingClay",               
                });

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = gatheringName,
                    KeyName = "gatherSalt",
                    RequiredSkill = "menial",
                    IsGathering = true,
                    JobTypeKey = "gatherMaterialsJobType",
                    MoveOutputToWorkerWhenCompleted = false,
                    PhysicalWorkFactor = moderateHardWork,
                    Stances = kneelingOrStandingProduction, // using shovel is standing and digging with trowel/hands is kneeling...
                    Outputs = new[] 
                    {
                        new Output(){ EntityTypeToCreate = "item:salt", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallHarvestingTask },
                    AgentActionState = AnimAction.Digging,
                    ProcessToolSetKey = "toolSetDiggingSalt",
                });

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = gatheringName,
                    KeyName = "gatherFiregrassSod",
                    RequiredSkill = "menial",
                    IsGathering = true,
                    JobTypeKey = "gatherMaterialsJobType",
                    MoveOutputToWorkerWhenCompleted = false,
                    PhysicalWorkFactor = moderateHardWork,
                    Stances = kneelingOrStandingProduction, // using shovel is standing and digging with trowel/hands is kneeling...
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:firegrassSod", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallHarvestingTask },
                    AgentActionState = AnimAction.Digging,
                    ProcessToolSetKey = "toolSetDiggingSoil",

                });

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = gatheringName,
                    KeyName = "gatherSulfurBlocks",
                    RequiredSkill = "grasping",
                    IsGathering = true,
                    JobTypeKey = "gatherMaterialsJobType",
                    MoveOutputToWorkerWhenCompleted = false,
                    PhysicalWorkFactor = moderateHardWork,
                    Stances = kneelingOrStandingProduction, // using shovel is standing and digging with trowel/hands is kneeling...
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:sulfurBlocks", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallHarvestingTask },
                    AgentActionState = AnimAction.Digging,
                    ProcessToolSetKey = "toolSetDiggingSoil",
                });
                #endregion

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = gatheringName,
                    KeyName = "gatherStones",
                    RequiredSkill = "grasping",
                    IsGathering = true,
                    JobTypeKey = "gatherMaterialsJobType",
                    MoveOutputToWorkerWhenCompleted = true,
                    PhysicalWorkFactor = moderateHardWork,
                    Stances = kneelingProduction,
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:stones", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallHarvestingTask },
                    AgentActionState = AnimAction.Harvesting/*,
                    AgentAnimationStates = new AnimModifier[] { AnimModifier.Kneeling }*/

                });


                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = gatheringName,
                    KeyName = "gatherFlint",
                    RequiredSkill = "grasping",
                    IsGathering = true,
                    JobTypeKey = "gatherMaterialsJobType",
                    MoveOutputToWorkerWhenCompleted = true, 
                    PhysicalWorkFactor = moderateWork,
                    Stances = kneelingProduction,
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:flintRough", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForTinyHarvestingTask },
                    AgentActionState = AnimAction.Harvesting/*,
                    AgentAnimationStates = new AnimModifier[] { AnimModifier.Kneeling }*/

                });



                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = gatheringName,
                    KeyName = "gatherSmoothSandstone",
                    RequiredSkill = "grasping",
                    IsGathering = true,
                    JobTypeKey = "gatherMaterialsJobType",
                    MoveOutputToWorkerWhenCompleted = true, 
                    PhysicalWorkFactor = moderateWork,
                    Stances = kneelingProduction,
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:smoothSandstone", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForTinyHarvestingTask },
                    AgentActionState = AnimAction.Harvesting/*,
                    AgentAnimationStates = new AnimModifier[] { AnimModifier.Kneeling }*/

                });





                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = gatheringName,
                    KeyName = "gatherTorux",
                    RequiredSkill = "grasping",
                    IsGathering = true,
                    JobTypeKey = "gatherFoodJobType",
                    MoveOutputToWorkerWhenCompleted = true, 
                    PhysicalWorkFactor = walkingLightWork,
                    Stances = kneelingProduction, 
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:torux", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallHarvestingTask },
                    AgentActionState = AnimAction.Harvesting/*,
                    AgentAnimationStates = new AnimModifier[] { AnimModifier.Kneeling }*/

                });

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = gatheringName,
                    KeyName = "gatherClamwich",
                    RequiredSkill = "grasping",
                    IsGathering = true,
                    JobTypeKey = "gatherFoodJobType",
                    MoveOutputToWorkerWhenCompleted = true, 
                    PhysicalWorkFactor = walkingLightWork,
                    Stances = kneelingProduction,
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:clamwich", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallHarvestingTask },
                    AgentActionState = AnimAction.Harvesting/*,
                    AgentAnimationStates = new AnimModifier[] { AnimModifier.Kneeling }*/

                });          

     


                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = catchingName,
                    KeyName = "gatherCrestedFoiler",
                    RequiredSkill = "bushcraft",
                    IsGathering = true,
                    JobTypeKey = "gatherFoodJobType",
                    MoveOutputToWorkerWhenCompleted = true, 
                    PhysicalWorkFactor = walkingLightWork,
                    Stances = kneelingOrStandingProduction, // ??
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:crestedFoiler", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallHarvestingTask },
                    ProcessToolSetKey = "toolSetChopWeakWood",
                    AgentActionState = AnimAction.Harvesting

                });

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = catchingName,
                    KeyName = "gatherGoldenCenobite",
                    RequiredSkill = "bushcraft",
                    IsGathering = true,
                    JobTypeKey = "gatherFoodJobType",
                    MoveOutputToWorkerWhenCompleted = true, 
                    PhysicalWorkFactor = walkingLightWork,
                    Stances = kneelingProduction, // ??
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:goldenCenobite", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallHarvestingTask },
                    AgentActionState = AnimAction.Harvesting,
                    ProcessToolSetKey = "toolSetChopWeakWood"/*,
                    AgentAnimationStates = new AnimModifier[] { AnimModifier.Kneeling }*/

                });

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = catchingName,
                    KeyName = "gatherTreeScuttler",
                    RequiredSkill = "bushcraft",
                    IsGathering = true,
                    JobTypeKey = "gatherFoodJobType",
                    MoveOutputToWorkerWhenCompleted = true, 
                    PhysicalWorkFactor = walkingLightWork,
                    Stances = kneelingOrStandingProduction, // ??
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:treeScuttler", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallHarvestingTask },
                    AgentActionState = AnimAction.Harvesting/*,
                    AgentAnimationStates = new AnimModifier[] { AnimModifier.Kneeling }*/

                });

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = catchingName,
                    KeyName = "gatherMuckGrinder",
                    RequiredSkill = "bushcraft",
                    IsGathering = true,
                    JobTypeKey = "gatherFoodJobType",
                    MoveOutputToWorkerWhenCompleted = true, 
                    PhysicalWorkFactor = walkingLightWork,
                    Stances = kneelingProduction, // ??
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:muckGrinder", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallHarvestingTask },
                    AgentActionState = AnimAction.Harvesting/*,
                    AgentAnimationStates = new AnimModifier[] { AnimModifier.Kneeling }*/

                });

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = gatheringName,
                    KeyName = "gatherSpriteSlug",
                    RequiredSkill = "bushcraft",
                    IsGathering = true,
                    JobTypeKey = "gatherFoodJobType",
                    MoveOutputToWorkerWhenCompleted = true, 
                    PhysicalWorkFactor = walkingLightWork,
                    Stances = kneelingOrStandingProduction, // ??
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:spriteSlug", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallHarvestingTask },
                    AgentActionState = AnimAction.Harvesting,
                    ProcessToolSetKey = "toolSetChopWeakWood"/*,
                    AgentAnimationStates = new AnimModifier[] { AnimModifier.Kneeling }*/

                });


                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = catchingName,
                    KeyName = "gatherCrazyDweller",
                    RequiredSkill = "bushcraft",
                    IsGathering = true,
                    JobTypeKey = "gatherFoodJobType",
                    MoveOutputToWorkerWhenCompleted = true, 
                    PhysicalWorkFactor = walkingLightWork,
                    Stances = kneelingOrStandingProduction, // ??
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:crazyDweller", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallHarvestingTask },
                    AgentActionState = AnimAction.Harvesting,
                    ProcessToolSetKey = "toolSetChopWeakWood"/*,
                    AgentAnimationStates = new AnimModifier[] { AnimModifier.Kneeling }*/

                });




                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = gatheringName,
                    KeyName = "gatherScampBeetle",
                    RequiredSkill = "bushcraft",
                    IsGathering = true,
                    JobTypeKey = "gatherFoodJobType",
                    MoveOutputToWorkerWhenCompleted = true, 
                    PhysicalWorkFactor = walkingLightWork,
                    Stances = kneelingProduction,
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:scampBeetle", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallHarvestingTask },
                    AgentActionState = AnimAction.Harvesting,
                    ProcessToolSetKey = "toolSetChopWeakWood"/*,
                    AgentAnimationStates = new AnimModifier[] { AnimModifier.Kneeling }*/

                });

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = gatheringName,
                    KeyName = "gatherScampGrub",
                    RequiredSkill = "bushcraft",
                    IsGathering = true,
                    JobTypeKey = "gatherFoodJobType",
                    MoveOutputToWorkerWhenCompleted = true, 
                    PhysicalWorkFactor = walkingLightWork,
                    Stances = kneelingProduction,
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:scampGrub", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallHarvestingTask },
                    AgentActionState = AnimAction.Harvesting/*,
                    AgentAnimationStates = new AnimModifier[] { AnimModifier.Kneeling }*/

                });

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = catchingName,
                    KeyName = "gatherImpEel",
                    RequiredSkill = "fishing",
                    IsGathering = true,
                    JobTypeKey = "gatherFoodJobType",
                    MoveOutputToWorkerWhenCompleted = true, 
                    PhysicalWorkFactor = walkingLightWork,
                    Stances = kneelingProduction,
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:impEel", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallHarvestingTask },
                    ProcessToolSetKey = "toolSetChopWeakWood",
                    AgentActionState = AnimAction.Harvesting/*,
                    AgentAnimationStates = new AnimModifier[] { AnimModifier.Kneeling }*/

                });


                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = gatheringName,
                    KeyName = "gatherCommonOilTubers",
                    RequiredSkill = "bushcraft",
                    MoveOutputToWorkerWhenCompleted = true, 
                    IsGathering = true,
                    JobTypeKey = "gatherFoodJobType",
                    PhysicalWorkFactor = walkingLightWork,
                    Stances = kneelingProduction,
                    Outputs = new[] {   
                        new Output(){ EntityTypeToCreate = "item:commonOilTubers", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallHarvestingTask },
                    AgentActionState = AnimAction.Harvesting/*,
                    AgentAnimationStates = new AnimModifier[] { AnimModifier.Kneeling }*/

                });

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = gatheringName,
                    KeyName = "gatherSpottedOilTubers",
                    RequiredSkill = "bushcraft",
                    MoveOutputToWorkerWhenCompleted = true, 
                    IsGathering = true,
                    JobTypeKey = "gatherFoodJobType",
                    PhysicalWorkFactor = walkingLightWork,
                    Stances = kneelingProduction,
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:spottedOilTubers", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallHarvestingTask },
                    AgentActionState = AnimAction.Harvesting/*,
                    AgentAnimationStates = new AnimModifier[] { AnimModifier.Kneeling }*/

                });

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = gatheringName,
                    KeyName = "gatherGlassyCreeperPods",
                    RequiredSkill = "fruitPicking",
                    MoveOutputToWorkerWhenCompleted = true, 
                    IsGathering = true,
                    JobTypeKey = "gatherFoodJobType",
                    PhysicalWorkFactor = walkingLightWork,
                    Stances = kneelingProduction,
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:glassyCreeperPods", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    ProcessToolSetKey = "toolSetChopWeakWood", // NEW
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallHarvestingTask },
                    AgentActionState = AnimAction.Harvesting/*,
                    AgentAnimationStates = new AnimModifier[] { AnimModifier.Kneeling }*/

                });

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = gatheringName,
                    KeyName = "gatherCrystalBerries",
                    RequiredSkill = "fruitPicking",
                    JobTypeKey = "gatherFoodJobType",
                    MoveOutputToWorkerWhenCompleted = true, 
                    IsGathering = true,
                    PhysicalWorkFactor = walkingLightWork,
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:crystalBerries", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallHarvestingTask },
                    AgentActionState = AnimAction.Harvesting                   

                });

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = gatheringName,
                    KeyName = "gatherFingerFruit",
                    RequiredSkill = "fruitPicking",
                    JobTypeKey = "gatherFoodJobType",
                    MoveOutputToWorkerWhenCompleted = true, 
                    IsGathering = true,
                    PhysicalWorkFactor = walkingLightWork,
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:fingerFruit", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallHarvestingTask },
                    AgentActionState = AnimAction.Harvesting

                });

                #endregion


                #region ///Construction///


            
                #region constructGreenhouse
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = constructingStructureName,
                    KeyName = "constructImprovisedGreenhouse",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "construction",
                    PhysicalWorkFactor = constructionExertion,
                    Stances = constructionStances,
                    Inputs = new[]
                    {
                        new Input(){ Entity = "item:improvisedGreenHouseCover", IsConsumed = false, Amount = new InputAmount(){ NoOfItems = 2 }},
                        new Input(){ Entity = "item:shadeleafCanes", IsConsumed = false, Amount = new InputAmount(){ NoOfItems = 3 }},
                    },
                    Outputs = new[] 
                    {
                        new Output(){ EntityTypeToCreate = "structure:improvisedGreenhouse", Amount = new OutputAmount(){ NoOfItems = 1 }},
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    ProcessToolSetKey = "toolSetDiggingConstruction",
                    AgentActionState = AnimAction.Building,
                    //   AgentAnimationStates = new[] {  }
                });

                listOfProcessTypes.Add(new ProcessType() 
                {
                    Name = constructingStructureName,
                    KeyName = "constructGreenhouse",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "construction",
                    PhysicalWorkFactor = constructionExertion,
                    Stances = constructionStances,
                    Inputs = new[]
                    {
                        new Input(){ Entity = "item:diamondGlass", IsConsumed = false, Amount = new InputAmount(){ NoOfItems = 5 }},
                        new Input(){ Entity = "item:shadeleafCanes", IsConsumed = false, Amount = new InputAmount(){ NoOfItems = 3 }},
                    },
                    Outputs = new[] 
                    {
                        new Output(){ EntityTypeToCreate = "structure:greenhouse", Amount = new OutputAmount(){ NoOfItems = 1 }},
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    ProcessToolSetKey = "toolSetDiggingConstruction",
                    AgentActionState = AnimAction.Building
                });
                #endregion

                #region constructMeatDryingRack
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = constructingStructureName,
                    KeyName = "constructMeatDryingRack",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = constructionExertion,
                    Stances = constructionStances,
                    Inputs = new[]
                    {
                        new Input(){ Entity = "item:sticks", IsConsumed = false, Amount = new InputAmount(){ NoOfItems = 3 }},
                    },
                    Outputs = new[] 
                    {
                        new Output(){ EntityTypeToCreate = "structure:meatDryingRack", Amount = new OutputAmount(){ NoOfItems = 1 }},
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    AgentActionState = AnimAction.Building,
                    ProcessToolSetKey = "toolSetCordage",
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region constructHideRack
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = constructingStructureName,
                    KeyName = "constructHideRack",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = constructionExertion,
                    Stances = constructionStances,
                    Inputs = new[]
                    {
                        new Input(){ Entity = "item:sticks", IsConsumed = false, Amount = new InputAmount(){ NoOfItems = 2 }},
                    },
                    Outputs = new[] 
                    {
                        new Output(){ EntityTypeToCreate = "structure:hideRack", Amount = new OutputAmount(){ NoOfItems = 1 }},
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    AgentActionState = AnimAction.Building,
                    ProcessToolSetKey = "toolSetCordage",
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion
                #region constructRareMetalRefinery
                //NA MINING CAMP MATERIAL
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = constructingStructureName,
                    KeyName = "constructRareMetalRefinery",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "menial",//keep it simple?
                    PhysicalWorkFactor = constructionExertion,
                    Stances = constructionStances,
                    Inputs = new[]
                    {
                        new Input(){ Entity = "item:metalRefineryEquipment", IsConsumed = false, Amount = new InputAmount(){ NoOfItems = 1 }},
                        new Input(){ Entity = "item:metalRefineryPart1", IsConsumed = false, Amount = new InputAmount(){ NoOfItems = 1 }},
                    },
                    Outputs = new[] 
                    {
                        new Output(){ EntityTypeToCreate = "structure:rareMetalRefinery", Amount = new OutputAmount(){ NoOfItems = 1 }},
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    AgentActionState = AnimAction.Building,
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region constructFirewoodStack
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = constructingStructureName,
                    KeyName = "constructFirewoodStack",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = constructionExertion,
                    Stances = constructionStances,
                    Inputs = new[]
                    {
                        new Input(){ Entity = "item:sticks", IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 }},
                    },
                    Outputs = new[] 
                    {
                        new Output(){ EntityTypeToCreate = "structure:firewoodStack", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    AgentActionState = AnimAction.Building,
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region constructPeatStack
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = constructingStructureName,
                    KeyName = "constructPeatStack",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = constructionExertion,
                    Stances = constructionStances,
                    Inputs = new[]
                    {
                        new Input(){ Entity = "item:sticks", IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 }},
                    },
                    Outputs = new[] 
                    {
                        new Output(){ EntityTypeToCreate = "structure:peatStack", Amount = new OutputAmount(){ NoOfItems = 1 }},

                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask }, //
                    AgentActionState = AnimAction.Building,
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region constructCompostPit
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = constructingStructureName,
                    KeyName = "constructCompostPit",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = moderateHardWork,
                    Stances = kneelingOrStandingProduction,
                    Inputs = new[]
                    {
                        new Input(){ Entity = "item:stones", IsConsumed = false, Amount = new InputAmount(){ NoOfItems = 1 }}, //mp nov 2015 currently only possible to build a structure if it has a material input. not possible to build if it does not have input. maybe add validation for this. also, every structure must have parts, else the structure dissappears when integrity degrades.
                    },
                    Outputs = new[] 
                    {
                        new Output(){ EntityTypeToCreate = "structure:compostPit", Amount = new OutputAmount(){ NoOfItems = 1 }},
                        new Output(){ EntityTypeToCreate = "item:soil", Amount = new OutputAmount(){ NoOfItems = 1 }, IsWasteProduct = true},
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    AgentActionState = AnimAction.Digging,
                    ProcessToolSetKey = "toolSetDiggingSoil",

                });
                #endregion

                #region constructCompostBin
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = constructingStructureName,
                    KeyName = "constructCompostBin",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = constructionExertion,
                    Stances = constructionStances,
                    Inputs = new[]
                    {
                        new Input(){ Entity = "item:sticks", IsConsumed = false, Amount = new InputAmount(){ NoOfItems = 2 }},
                    },
                    Outputs = new[] 
                    {
                        new Output(){ EntityTypeToCreate = "structure:compostBin", Amount = new OutputAmount(){ NoOfItems = 1 }},
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    AgentActionState = AnimAction.Building,
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region constructDryingShed
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = constructingStructureName,
                    KeyName = "constructDryingShed",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "construction",
                    PhysicalWorkFactor = constructionExertion,
                    Stances = constructionStances,
                    Inputs = new[] 
                    { 
                        new Input() { Entity = "item:spoakShingles", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false },
                        new Input() { Entity = "item:solidMudBrick", Amount = new InputAmount() { NoOfItems = 2 }, IsConsumed = false },
                        new Input() { Entity = "item:shadeleafCanes", Amount = new InputAmount() { NoOfItems = 3 }, IsConsumed = false },
                    },
                    Outputs = new[] 
                    {
                        new Output(){ EntityTypeToCreate = "structure:dryingShed", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForBigCraftingTask },
                    AgentActionState = AnimAction.Building,
                    ProcessToolSetKey = "toolSetDiggingConstruction",
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion 

                #region constructToolshed
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = constructingStructureName,
                    KeyName = "constructToolshed",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "construction",
                    PhysicalWorkFactor = constructionExertion,
                    Stances = constructionStances,
                    Inputs = new[] 
                    { 
                        new Input() { Entity = "item:solidMudBrick", Amount = new InputAmount() { NoOfItems = 3 }, IsConsumed = false },
                        new Input() { Entity = "item:waterCaneStem", Amount = new InputAmount() { NoOfItems = 2 }, IsConsumed = false },
                    },
                    Outputs = new[] 
                    {
                        new Output(){ EntityTypeToCreate = "structure:toolshed", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForBigCraftingTask },
                    AgentActionState = AnimAction.Building,
                    ProcessToolSetKey = "toolSetDiggingConstruction",
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion 

                #region constructClayGranary
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = constructingStructureName,
                    KeyName = "constructClayGranary",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "construction",
                    PhysicalWorkFactor = constructionExertion,
                    Stances = constructionStances,
                    Inputs = new[] 
                    { 
                        new Input() { Entity = "item:spoakShingles", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false },
                        new Input() { Entity = "item:solidMudBrick", Amount = new InputAmount() { NoOfItems = 5 }, IsConsumed = false },
                    },
                    Outputs = new[] 
                    {
                        new Output(){ EntityTypeToCreate = "structure:clayGranary", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForBigCraftingTask },
                    AgentActionState = AnimAction.Building,
                    ProcessToolSetKey = "toolSetDiggingConstruction", //
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion 

                #region constructCaneHut
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = constructingStructureName,
                    KeyName = "constructCaneHut",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "construction",
                    PhysicalWorkFactor = constructionExertion,
                    Stances = constructionStances,

                    Inputs = new[] 
                    { 
                        new Input() { Entity = "item:waterCaneStem", Amount = new InputAmount() { NoOfItems = 7 }, IsConsumed = false },
                        new Input() { Entity = "item:solidMudBrick", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false }, //mp feb 2016  increased complexity.
                    },
                    Outputs = new[] 
                    {
                        new Output(){ EntityTypeToCreate = "structure:caneHut", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForBigCraftingTask },
                    AgentActionState = AnimAction.Building,
                    ProcessToolSetKey = "toolSetCordage",
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region constructclayHut
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = constructingStructureName,
                    KeyName = "constructClayHut",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "construction",
                    PhysicalWorkFactor = constructionExertion,
                    Stances = constructionStances,

                    Inputs = new[] 
                    { 
                        new Input() { Entity = "item:solidMudBrick", Amount = new InputAmount() { NoOfItems = 5 }, IsConsumed = false },
                        new Input() { Entity = "item:spoakBranchesTrimmed", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false },
                        new Input() { Entity = "item:spoakShingles", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false },
                        new Input() { Entity = "item:stones", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false },
                    },
                    Outputs = new[] 
                    {
                        new Output(){ EntityTypeToCreate = "structure:clayHut", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForBigCraftingTask },
                    ProcessToolSetKey = "toolSetDiggingConstruction", //
                    AgentActionState = AnimAction.Building,
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region constructTurnipHut
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = " Build turnip hut", // special action, not accessed via build menu
                    KeyName = "constructTurnipHut",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "construction",
                    PhysicalWorkFactor = constructionExertion,
                    Stances = constructionStances,
                    Inputs = new[] 
                    { 
                        new Input() { Entity = "item:turnipShell", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false },
                        new Input() { Entity = "item:sticks", Amount = new InputAmount() { NoOfItems = 2 }, IsConsumed = false },
                        new Input() { Entity = "item:firegrassSod", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false},
                    },

                    Outputs = new[] 
                    {
                        new Output(){ EntityTypeToCreate = "structure:turnipHut", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    ProcessToolSetKey = "toolSetCordage",
                    AgentActionState = AnimAction.Building,
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region constructImprovisedSmithy
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = constructingStructureName,
                    KeyName = "constructImprovisedSmithy",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "construction",
                    PhysicalWorkFactor = constructionExertion,
                    Stances = constructionStances,
                    Inputs = new[] 
                    { 
                        new Input() { Entity = "item:solidMudBrick", Amount = new InputAmount() { NoOfItems = 3 }, IsConsumed = false },
                        new Input() { Entity = "item:stones", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false },
                    },
                    Outputs = new[] 
                    {
                        new Output(){ EntityTypeToCreate = "structure:improvisedSmithy", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    ProcessToolSetKey = "toolSetSmallDiggingConstruction", //no spade required
                    AgentActionState = AnimAction.Building,
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region constructSimpleSmithy
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = constructingStructureName,
                    KeyName = "constructSimpleSmithy",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "construction",
                    PhysicalWorkFactor = constructionExertion,
                    Stances = constructionStances,
                    Inputs = new[] 
                    { 
                        new Input() { Entity = "item:solidMudBrick", Amount = new InputAmount() { NoOfItems = 3 }, IsConsumed = false },
                        new Input() { Entity = "item:anvil", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false },
                        new Input() { Entity = "item:barClamps", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false },
                    },
                    Outputs = new[] 
                    {
                        new Output(){ EntityTypeToCreate = "structure:simpleSmithy", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    ProcessToolSetKey = "toolSetSmallDiggingConstruction", //
                    AgentActionState = AnimAction.Building,
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion



                #region constructKiln
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = constructingStructureName,
                    KeyName = "constructKiln",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "construction",
                    PhysicalWorkFactor = constructionExertion,
                    Stances = constructionStances,
                    Inputs = new[] 
                    { 
                        new Input() { Entity = "item:clay", Amount = new InputAmount() { NoOfItems = 4 }, IsConsumed = true }, //"item:solidMudBrick" mo removed this because the UI feedback was confusing players (mudbricks made with kiln, but no prod info that mudbricks dry on their own)
                        new Input() { Entity = "item:stones", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false },
                    },

                    Outputs = new[] 
                    {
                        new Output(){ EntityTypeToCreate = "structure:kiln", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    ProcessToolSetKey = "toolSetDiggingConstruction", //
                    AgentActionState = AnimAction.Building,
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region constructGoldFurnace
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = constructingStructureName,
                    KeyName = "constructGoldFurnace",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "construction",
                    PhysicalWorkFactor = constructionExertion,
                    Stances = constructionStances,
                    Inputs = new[] 
                    { 
                        new Input() { Entity = "item:firebricks", Amount = new InputAmount() { NoOfItems = 3 }, IsConsumed = false }, //
                        new Input() { Entity = "item:clay", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = true },
                    },

                    Outputs = new[] 
                    {
                        new Output(){ EntityTypeToCreate = "structure:goldFurnace", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    ProcessToolSetKey = "toolSetDiggingConstruction", //
                    AgentActionState = AnimAction.Building,
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region constructKilnImprovisedSmall
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = constructingStructureName,
                    KeyName = "constructKilnImprovisedSmall",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = constructionExertion,
                    Stances = constructionStances,
                    Inputs = new[] 
                    { 
                       
                        new Input() { Entity = "item:stones", Amount = new InputAmount() { NoOfItems = 3 }, IsConsumed = false },
                    },

                    Outputs = new[] 
                    {
                        new Output(){ EntityTypeToCreate = "structure:kilnImprovisedSmall", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    AgentActionState = AnimAction.Building,
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region constructSpikeTrap
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = settingUpName,
                    KeyName = "constructSpikeTrap",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = constructionExertion,
                    Stances = constructionStances,

                    Inputs = new[] 
                    { 
                        new Input() { Entity = "item:spikeTrap", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false },
                    },
                    Outputs = new[] 
                    {
                        new Output(){ EntityTypeToCreate = "structure:spikeTrap", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForInstantCraftingTask },
                    AgentActionState = AnimAction.Building,
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region constructCollapseTrap (commented out)
                /* WIP remember to outcomment the eventhooks
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = constructingStructureName,
                    KeyName = "constructCollapseTrap",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = constructionExertion,
                    Stances = constructionStances,

                    Inputs = new[] { new Input() { Entity = "item:shadeleafCanes", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false }, 

                    },

                    Outputs = new[] 
                    {
                        new Output(){ EntityType = "structure:collapseTrap", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysOfWorkNeeded = timeForTinyCraftingTask },
                    //Tools = "cordage",
                    AgentActionState = AnimAction.Building,
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
            */
                #endregion

                #region constructDeadfallTrap
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = constructingStructureName,
                    KeyName = "constructDeadfallTrap",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = constructionExertion,
                    Stances = constructionStances,

                    Inputs = new[] { new Input() { Entity = "item:stones", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false }, 

                    },

                    Outputs = new[] 
                    {
                        new Output(){ EntityTypeToCreate = "structure:deadfallTrap", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForTinyCraftingTask },
                    //Tools = "cordage",
                    //AgentActionState = AnimAction.Building,
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region constructSpringSnare
                listOfProcessTypes.Add(new ProcessType() 
                {
                    Name = constructingStructureName,
                    KeyName = "constructSpringSnare",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = constructionExertion,
                    Stances = constructionStances,

                    Inputs = new[] { new Input() { Entity = "item:shadeleafCanes", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false }, 

                    },

                    Outputs = new[] 
                    {
                        new Output(){ EntityTypeToCreate = "structure:springSnare", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForTinyCraftingTask },
                    ProcessToolSetKey = "toolSetCordage", // mp: string needed for this.
                    //AgentActionState = AnimAction.Building,
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region constructLandMine
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = settingUpName,
                    KeyName = "constructLandMine",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = constructionExertion,
                    Stances = constructionStances,

                    Inputs = new[] { new Input() { Entity = "item:landMine", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false }
                    },

                    Outputs = new[] 
                    {
                        new Output(){ EntityTypeToCreate = "structure:landMine", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForTinyCraftingTask },
                  //  Tools = "cordage",
                    AgentActionState = AnimAction.Building,
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region constructHelipad
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = constructingStructureName,
                    KeyName = "constructHelipad",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "construction",
                    PhysicalWorkFactor = constructionExertion,
                    Stances = constructionStances,
                    Inputs = new[] { new Input() { Entity = "item:paint", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = true },
                                     new Input() { Entity = "item:stones", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false }, //MP jan 2016: without parts it will dissappear when degraded! this confuses the player.

               },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "structure:helipad", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    AgentActionState = AnimAction.Building, //MS: I put this here so it would pick the right SoundAndAnimationSet
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion
                #region constructHelipadBig
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = constructingStructureName,
                    KeyName = "constructHelipadBig",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "construction",
                    PhysicalWorkFactor = constructionExertion,
                    Stances = constructionStances,
                    Inputs = new[] { new Input() { Entity = "item:structurePanels", Amount = new InputAmount() { NoOfItems = 2 }, IsConsumed = false }

               },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "structure:helipadBig", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumPrimitiveShelter },
                    ProcessToolSetKey = "toolSetShapenSmallWood",
                    AgentActionState = AnimAction.Building, //
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                }); //
                #endregion
                #region constructSatelliteGroundStation
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = settingUpName,
                    KeyName = "constructSatelliteGroundStation",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "construction",
                    PhysicalWorkFactor = constructionExertion,
                    Stances = constructionStances,

                    Inputs = new[] { new Input() { Entity = "item:satelliteGroundStation", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false }

               },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "structure:satelliteGroundStation", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    AgentActionState = AnimAction.Building, //MS: I put this here so it would pick the right SoundAndAnimationSet
                    AgentAnimationStates = new[] { AnimModifier.Electronic }
                });
                #endregion

                #region constructSmallTent
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = settingUpName,
                    KeyName = "constructSmallTent",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = constructionExertion,
                    Stances = constructionStances,

                    Inputs = new[] { new Input() { Entity = "item:smallTent", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false }

               },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "structure:smallTent", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeToBuildAbatis },//very quick to set up
                    AgentActionState = AnimAction.Building, //MS: I put this here so it would pick the right SoundAndAnimationSet
                    AgentAnimationStates = new[] { AnimModifier.Tarp }
                });
                #endregion

                #region constructOctagonalTent
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = settingUpName,
                    KeyName = "constructOctagonalTent",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = constructionExertion,
                    Stances = constructionStances,

                    Inputs = new[] { new Input() { Entity = "item:octagonalTent", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false }

               },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "structure:octagonalTent", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeToBuildAbatis },//very quick to set up
                    AgentActionState = AnimAction.Building, //MS: I put this here so it would pick the right SoundAndAnimationSet
                    AgentAnimationStates = new[] { AnimModifier.Metal }
                });
                #endregion

                #region constructDomeTent
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = settingUpName,
                    KeyName = "constructDomeTent",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = constructionExertion,
                    Stances = constructionStances,

                    Inputs = new[] { new Input() { Entity = "item:domeTent", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false }

               },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "structure:domeTent", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeToBuildAbatis },//very quick to set up
                    AgentActionState = AnimAction.Building, //MS: I put this here so it would pick the right SoundAndAnimationSet
                    AgentAnimationStates = new[] { AnimModifier.Metal }
                });
                #endregion

                #region constructA-frameTarp
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = constructingStructureName,
                    KeyName = "constructA-frameTarp",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = constructionExertion,
                    Stances = constructionStances,

                    Inputs = new[] { new Input() { Entity = "item:sticks", Amount = new InputAmount() { NoOfItems = 2 }, IsConsumed = false }, //AF 30/5-14 noOfItems change from 2
                                     new Input() { Entity = "item:wingweedLeaves", Amount = new InputAmount() { NoOfItems = 2 }, IsConsumed = false },
                                     new Input() { Entity = "item:thermalTarp", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false }
               },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "structure:A-frameTarp", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallPrimitiveShelter },//DaysOfWorkNeeded = 0.05f
                    ProcessToolSetKey = "toolSetCordage",
                    AgentActionState = AnimAction.Building,
                    AgentAnimationStates = new[] { AnimModifier.Improvised, AnimModifier.Tarp }
                });
                #endregion
                #region constructA-frameSpoakLeaves
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = constructingStructureName,
                    KeyName = "constructA-frameSpoakLeaves",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = constructionExertion,
                    Stances = constructionStances,

                    Inputs = new[] { new Input() { Entity = "item:spoakBranchesTrimmed", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false }, //

                                     new Input() { Entity = "item:spoakLeaves", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false }
               },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "structure:A-frameSpoakLeaves", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallPrimitiveShelter },//DaysOfWorkNeeded = 0.05f
                    ProcessToolSetKey = "toolSetCordage",
                    AgentActionState = AnimAction.Building,
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion
                #region constructA-frameScraps
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = constructingStructureName,
                    KeyName = "constructA-frameScraps",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = constructionExertion,
                    Stances = constructionStances,

                    Inputs = new[] { new Input() { Entity = "item:sticks", Amount = new InputAmount() { NoOfItems = 4 }, IsConsumed = false }, //AF 30/5-14 noOfItems change from 2
                                     new Input() { Entity = "item:seatCushions", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false },
                                     new Input() { Entity = "item:panelScraps", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false }
               },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "structure:A-frameScraps", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallPrimitiveShelter },//DaysOfWorkNeeded = 0.05f
                    ProcessToolSetKey = "toolSetCordage",
                    AgentActionState = AnimAction.Building,
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion
                #region constructDaysheenTipi

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = constructingStructureName,
                    KeyName = "constructDaysheenTipi",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = constructionExertion,
                    Stances = constructionStances,
                
                    Inputs = new[] { new Input() { Entity = "item:daysheenLeaves", Amount = new InputAmount() { NoOfItems = 2 }, IsConsumed = false }                                
        
               },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "structure:daysheenTipi", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 0.9f * timeForSmallPrimitiveShelter },//DaysOfWorkNeeded = 0.05f //AF 30/5-14 added 0.9f
                    ProcessToolSetKey = "toolSetCordage",
                    AgentActionState = AnimAction.Building,
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion
                #region constructLean-toTarp

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = constructingStructureName,
                    KeyName = "constructLean-toTarp",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = constructionExertion,
                    Stances = constructionStances,

                    Inputs = new[] { 
                        new Input() { Entity = "item:sticks", Amount = new InputAmount() { NoOfItems = 6 }, IsConsumed = false }, //AF 30/5-14 noOfItems change from 1                                  
                        new Input() { Entity = "item:thermalTarp", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false }
                    },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "structure:lean-toTarp", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 1.2f * timeForSmallPrimitiveShelter },
                    ProcessToolSetKey = "toolSetCordage",
                    AgentActionState = AnimAction.Building,
                    AgentAnimationStates = new[] { AnimModifier.Improvised, AnimModifier.Tarp }
                });

                #endregion
                #region constructLean-toSpoakLeaves
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = constructingStructureName,
                    KeyName = "constructLean-toSpoakLeaves",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = constructionExertion,
                    Stances = constructionStances,

                    Inputs = new[] { new Input() { Entity = "item:sticks", Amount = new InputAmount() { NoOfItems = 4 }, IsConsumed = false },   // 
                                      new Input() { Entity = "item:wingweedLeaves", Amount = new InputAmount() { NoOfItems = 2 }, IsConsumed = false },                              
                                     new Input() { Entity = "item:spoakLeaves", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false } //mp 2016
               },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "structure:lean-toSpoakLeaves", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 1.2f * timeForSmallPrimitiveShelter }, //AF ADDED 1.2f 30/5-14
                    ProcessToolSetKey = "toolSetCordage",
                    AgentActionState = AnimAction.Building,
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });

                #endregion
                #region constructLean-toScraps
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = constructingStructureName,
                    KeyName = "constructLean-toScraps",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = constructionExertion,
                    Stances = constructionStances,

                    Inputs = new[] { new Input() { Entity = "item:sticks", Amount = new InputAmount() { NoOfItems = 6 }, IsConsumed = false },   //AF 30/5-14 noOfItems change from 2                                 
                                     new Input() { Entity = "item:panelScraps", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false }
               },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "structure:lean-toScraps", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 1.2f * timeForSmallPrimitiveShelter }, //AF ADDED 1.2f 30/5-14
                    ProcessToolSetKey = "toolSetCordage",
                    AgentActionState = AnimAction.Building,
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion
                #region constructDomeShelterTarp

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = constructingStructureName,
                    KeyName = "constructDomeShelterTarp",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = constructionExertion,
                    Stances = constructionStances,

                    Inputs = new[] { new Input() { Entity = "item:shadeleafCanes", Amount = new InputAmount() { NoOfItems = 4 }, IsConsumed = false },    //AF 30/5-14 noOfItems change from 2                               
                                     new Input() { Entity = "item:thermalTarp", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false }
               },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "structure:domeShelterTarp", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 1.2f * timeForMediumPrimitiveShelter },
                    ProcessToolSetKey = "toolSetCordage",
                    AgentActionState = AnimAction.Building,
                    AgentAnimationStates = new[] { AnimModifier.Improvised, AnimModifier.Tarp }
                });
                #endregion
                #region constructDomeShelterSpoakShingles

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = constructingStructureName,
                    KeyName = "constructDomeShelterSpoakShingles",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = constructionExertion,
                    Stances = constructionStances,

                    Inputs = new[] { new Input() { Entity = "item:shadeleafCanes", Amount = new InputAmount() { NoOfItems = 4 }, IsConsumed = false },                                  
                                     new Input() { Entity = "item:spoakShingles", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false }
               },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "structure:domeShelterSpoakShingles", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumPrimitiveShelter },
                    ProcessToolSetKey = "toolSetCordage",
                    AgentActionState = AnimAction.Building,
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion
                #region constructRadioHutImprovised

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = constructingStructureName,
                    KeyName = "constructRadioHutImprovised",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "construction", //not electricalengineering. dont want the engineer to become such a bottleneck if hut breaks down.
                    PhysicalWorkFactor = constructionExertion,
                    Stances = constructionStances,

                    Inputs = new[] { new Input() { Entity = "item:shadeleafCanes", Amount = new InputAmount() { NoOfItems = 3 }, IsConsumed = false },                                   
                                     new Input() { Entity = "item:spoakLeaves", Amount = new InputAmount() { NoOfItems = 2 }, IsConsumed = false },
                                     new Input() { Entity = "item:radio", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false },
                                     new Input() { Entity = "item:radioAntenna", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false }
               },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "structure:radioHutImprovised", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumPrimitiveShelter },
                    ProcessToolSetKey = "toolSetCordage",
                    AgentActionState = AnimAction.Building,
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

                #region constructRadioHut

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = constructingStructureName,
                    KeyName = "constructRadioHut",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "construction", //not electricalengineering. dont want the engineer to become such a bottleneck if hut breaks down.
                    PhysicalWorkFactor = constructionExertion,
                    Stances = constructionStances,

                    Inputs = new[] { new Input() { Entity = "item:shadeleafCanes", Amount = new InputAmount() { NoOfItems = 3 }, IsConsumed = false },                                   
                                     new Input() { Entity = "item:spoakShingles", Amount = new InputAmount() { NoOfItems = 2 }, IsConsumed = false },
                                     new Input() { Entity = "item:radio", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false },
                                     new Input() { Entity = "item:radioAntenna", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false }
               },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "structure:radioHut", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumPrimitiveShelter },
                    ProcessToolSetKey = "toolSetCordage",
                    AgentActionState = AnimAction.Building,
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion

               
                #region constructWigwamSpoakShingles
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = constructingStructureName,
                    KeyName = "constructWigwamSpoakShingles",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = constructionExertion,
                    Stances = constructionStances,
                    //mp it needs to be significantly harder to make than domeShelterSpoakShingles, else players will always make the wigwam once they are able to produce shingles.
                    Inputs = new[] { new Input() { Entity = "item:spoakBranchesTrimmed", Amount = new InputAmount() { NoOfItems = 3 }, IsConsumed = false },                                   
                                     new Input() { Entity = "item:spoakShingles", Amount = new InputAmount() { NoOfItems = 3 }, IsConsumed = false }, //
                                     new Input() { Entity = "item:firegrassSod", Amount = new InputAmount() { NoOfItems = 2 }, IsConsumed = false },
                                     new Input() { Entity = "item:stones", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false }
               },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "structure:wigwamSpoakShingles", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForLargePrimitiveShelter },
                    ProcessToolSetKey = "toolSetCordage",
                    AgentActionState = AnimAction.Building,
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion
                #region constructCampfire

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = constructingStructureName,
                    KeyName = "constructCampfire",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "menial", // "bushcraft",
                    PhysicalWorkFactor = constructionExertion,
                    Stances = constructionStances,

                    Inputs = new[] { 
                        new Input() { Entity = "item:firewood", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = true }, 
                        new Input() { Entity = "item:stones", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false }             
                    },
                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "structure:campfire", Amount = new OutputAmount(){ NoOfItems = 1 }},
                        new Output(){ EntityTypeToCreate = "item:firewood", IsWasteProduct = true, Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },//should be very quick
                    AgentActionState = AnimAction.Building,
                    AgentAnimationStates = new[] { AnimModifier.Improvised }

                });

                #endregion
                #region constructFieldKitchen
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = settingUpName,
                    KeyName = "constructFieldKitchen",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "menial", 
                    PhysicalWorkFactor = constructionExertion,
                    Stances = constructionStances,

                    Inputs = new[] { new Input() { Entity = "item:fieldKitchenStove", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false },
                                     new Input() { Entity = "item:fieldKitchenEquipment", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false }
                    },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "structure:fieldKitchen", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    AgentActionState = AnimAction.Building,
                    AgentAnimationStates = new[] { AnimModifier.Metal }

                });
                #endregion
                #region constructFieldLab

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = settingUpName,
                    KeyName = "constructFieldLab",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = sittingLightWork,
                    Stances = kneelingProduction,

                    Inputs = new[] { new Input() { Entity = "item:fieldLabPacked", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false }

                    },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "structure:fieldLab", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    AgentActionState = AnimAction.Building,
                    AgentAnimationStates = new[] { AnimModifier.Metal }
                });

                #endregion
                #region constructImprovisedKitchen
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = settingUpName,
                    KeyName = "constructImprovisedKitchen",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = constructionExertion,
                    Stances = constructionStances,

                    Inputs = new[] {new Input() { Entity = "item:firewood", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = true },
                                     new Input() { Entity = "item:panelScraps", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false },
                                     new Input() { Entity = "item:sticks", Amount = new InputAmount() { NoOfItems = 2 }, IsConsumed = false }, //
                                     new Input() { Entity = "item:spoakShingles", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false }
                    },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "structure:improvisedKitchen", Amount = new OutputAmount(){ NoOfItems = 1 }},
                        new Output(){ EntityTypeToCreate = "item:firewood", IsWasteProduct = true, Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumPrimitiveShelter },
                    ProcessToolSetKey = "toolSetCordage",
                    AgentActionState = AnimAction.Building,
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });

                #endregion
                #region constructMudbrickKitchen
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = constructingStructureName,
                    KeyName = "constructMudbrickKitchen",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = constructionExertion,
                    Stances = constructionStances,

                    Inputs = new[] {new Input() { Entity = "item:firewood", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = true },
                                     new Input() { Entity = "item:clay", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = true },//mp feb 2016
                                     new Input() { Entity = "item:sticks", Amount = new InputAmount() { NoOfItems = 2 }, IsConsumed = false }, //
                                     new Input() { Entity = "item:spoakShingles", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false }
                    },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "structure:mudBrickKitchen", Amount = new OutputAmount(){ NoOfItems = 1 }},
                        new Output(){ EntityTypeToCreate = "item:firewood", IsWasteProduct = true, Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumPrimitiveShelter },
                    ProcessToolSetKey = "toolSetCordage",
                    AgentActionState = AnimAction.Building,
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });

                #endregion
                #region constructImprovisedWorkbench

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = constructingStructureName,
                    KeyName = "constructImprovisedWorkbench",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = constructionExertion,
                    Stances = constructionStances,

                    Inputs = new[] { new Input() { Entity = "item:panelScraps", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false },
                                     new Input() { Entity = "item:sticks", Amount = new InputAmount() { NoOfItems = 3 }, IsConsumed = false }, //

                    },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "structure:improvisedWorkbench", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumPrimitiveShelter },
                    ProcessToolSetKey = "toolSetCordage",
                    AgentActionState = AnimAction.Building,
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });
                #endregion
                #region constructMudbrickWorkbench
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = constructingStructureName,
                    KeyName = "constructMudbrickWorkbench",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = constructionExertion,
                    Stances = constructionStances,

                    Inputs = new[] { new Input() { Entity = "item:clay", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = true },//mp feb 2016
                                     new Input() { Entity = "item:sticks", Amount = new InputAmount() { NoOfItems = 3 }, IsConsumed = false }, //

                    },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "structure:mudBrickWorkbench", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumPrimitiveShelter },
                    ProcessToolSetKey = "toolSetCordage",
                    AgentActionState = AnimAction.Building,
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });

                #endregion
               

                #region constructWorkshopBuilding
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = constructingStructureName,
                    KeyName = "constructWorkshopBuilding",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "construction",
                    PhysicalWorkFactor = constructionExertion,
                    Stances = constructionStances,

                    Inputs = new[] { 
                                     new Input() { Entity = "item:solidMudBrick", Amount = new InputAmount() { NoOfItems = 5 }, IsConsumed = false },
                                     new Input() { Entity = "item:waterCaneStem", Amount = new InputAmount() { NoOfItems = 3 }, IsConsumed = false },
                                     new Input() { Entity = "item:stones", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false },

                    },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "structure:workshopBuilding", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForLargePrimitiveShelter },
                    ProcessToolSetKey = "toolSetCordage",
                    AgentActionState = AnimAction.Building,
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });

                #endregion

                #region constructCookhouse
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = constructingStructureName,
                    KeyName = "constructCookhouse",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "construction",
                    PhysicalWorkFactor = constructionExertion,
                    Stances = constructionStances,

                    Inputs = new[] { 
                                     new Input() { Entity = "item:solidMudBrick", Amount = new InputAmount() { NoOfItems = 5 }, IsConsumed = false },
                                     new Input() { Entity = "item:waterCaneStem", Amount = new InputAmount() { NoOfItems = 3 }, IsConsumed = false },
                                     new Input() { Entity = "item:spoakBranchesTrimmed", Amount = new InputAmount() { NoOfItems = 2 }, IsConsumed = false },
                                     new Input() { Entity = "item:spoakShingles", Amount = new InputAmount() { NoOfItems = 2 }, IsConsumed = false },
                                     new Input() { Entity = "item:textile", Amount = new InputAmount() { NoOfItems = 3 }, IsConsumed = false },
                                     new Input() { Entity = "item:stones", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false },

                    },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "structure:cookhouse", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForLargePrimitiveShelter },
                    ProcessToolSetKey = "toolSetCordage",
                    AgentActionState = AnimAction.Building,
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });

                #endregion

                #region constructStill
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = constructingStructureName,
                    KeyName = "constructStill",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "construction", //mp construction or engineering?
                    PhysicalWorkFactor = constructionExertion,
                    Stances = constructionStances,

                    Inputs = new[] { new Input() { Entity = "item:stillComponents", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false },
                                     new Input() { Entity = "item:stones", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false },
                                     new Input() { Entity = "item:clayJar", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false },
                                     new Input() { Entity = "item:sticks", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false },

                    },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "structure:still", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumPrimitiveShelter },
                    ProcessToolSetKey = "toolSetCordage",
                    AgentActionState = AnimAction.Building,
                    AgentAnimationStates = new[] { AnimModifier.Improvised }
                });

                #endregion

                #region constructSensor

                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = settingUpName,
                    KeyName = "constructSensor",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "menial", 
                    PhysicalWorkFactor = sittingLightWork,
                    Stances = kneelingProduction,

                    Inputs = new[] { new Input() { Entity = "item:sensor", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false }          
               },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "structure:sensor", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForTinyCraftingTask },//should be very quick
                    AgentActionState = AnimAction.Building,
                    AgentAnimationStates = new [] { AnimModifier.Electronic }

                });
                #endregion

                #region constructSentry
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = settingUpName,
                    KeyName = "constructSentry", //setting up the structure
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = sittingLightWork,
                    Stances = kneelingProduction,

                    Inputs = new[] { new Input() { Entity = "item:sentry", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false }          
               },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "structure:sentry", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForTinyCraftingTask },//should be very quick
                    AgentActionState = AnimAction.Building,
                    AgentAnimationStates = new[] { AnimModifier.Electronic }

                });
                #endregion
                #region constructSprayGunSentry
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = settingUpName,
                    KeyName = "constructSprayGunSentry", //setting up the structure
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = sittingLightWork,
                    Stances = kneelingProduction,

                    Inputs = new[] { new Input() { Entity = "item:spraySentry", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false }          
               },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "structure:sprayGunSentry", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForTinyCraftingTask },//should be very quick
                    AgentActionState = AnimAction.Building,
                    AgentAnimationStates = new[] { AnimModifier.Electronic }

                });

                #endregion
                #region constructShotgunSentry
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = settingUpName,
                    KeyName = "constructShotgunSentry", //setting up the structure
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = sittingLightWork,
                    Stances = kneelingProduction,

                    Inputs = new[] { new Input() { Entity = "item:shotgunSentry", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false }          
               },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "structure:shotgunSentry", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForTinyCraftingTask },//should be very quick
                    AgentActionState = AnimAction.Building,
                    AgentAnimationStates = new[] { AnimModifier.Electronic }

                });

                #endregion

                #region constructWeatherStation
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = settingUpName,
                    KeyName = "constructWeatherStation",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = constructionExertion,
                    Stances = kneelingProduction,

                    Inputs = new[] { new Input() { Entity = "item:weatherStationMast", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false },
                                     new Input() { Entity = "item:weatherStationSensors", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false }    
                    },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "structure:weatherStation", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                    AgentActionState = AnimAction.Building,
                    AgentAnimationStates = new[] { AnimModifier.Electronic }

                });
                #endregion
                #region constructMolecularAssembler
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = settingUpName,
                    KeyName = "constructMolecularAssembler",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "menial",
                    PhysicalWorkFactor = constructionExertion,
                    Stances = kneelingProduction,

                    Inputs = new[] { new Input() { Entity = "item:vacuumChamber", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false },
                                     new Input() { Entity = "item:assemblerCabinet", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false },
                                     new Input() { Entity = "item:assemblerCooling", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false } 
                    },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "structure:molecularAssembler", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForMediumCraftingTask },
                    AgentActionState = AnimAction.Building,
                    AgentAnimationStates = new[] { AnimModifier.Metal }

                });
                #endregion
                #region constructStorageHole
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = constructingStructureName,
                    KeyName = "constructStorageHole",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = moderateHardWork,
                    Stances = constructionStances,
               
                    Inputs = new[] { new Input() { Entity = "item:stones", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false },
                                new Input() { Entity = "item:spoakLeaves", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false }
               },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "structure:storageHole", Amount = new OutputAmount(){ NoOfItems = 1 }},
                        new Output(){ EntityTypeToCreate = "item:soil", IsWasteProduct = true, Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeToDigHole },//DaysOfWorkNeeded = 0.1f
                    AgentActionState = AnimAction.Building,
                    AgentAnimationStates = new[] { AnimModifier.Improvised }

                });

                #endregion
                #region constructCooledFoodCache
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = constructingStructureName,
                    KeyName = "constructCooledFoodCache",
                    JobTypeKey = "constructionJobType",
                    RequiredSkill = "bushcraft",
                    PhysicalWorkFactor = moderateHardWork,
                    Stances = constructionStances,
                 
                    Inputs = new[] { new Input() { Entity = "item:stones", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false },
                                new Input() { Entity = "item:spoakLeaves", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false },
                                new Input() { Entity = "item:inactivatedFoodCoolerUnit", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = true }
               },

                    Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "structure:cooledFoodCache", Amount = new OutputAmount(){ NoOfItems = 1 }},
                        new Output(){ EntityTypeToCreate = "item:soil", IsWasteProduct = true, Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 1.1f * timeToDigHole },
                    AgentActionState = AnimAction.Building,
                    AgentAnimationStates = new[] { AnimModifier.Improvised }

                });

                #endregion
                #region constructSmokeOven

                listOfProcessTypes.Add(new ProcessType()
                    {
                        Name = constructingStructureName,
                        KeyName = "constructSmokeOven",
                        JobTypeKey = "constructionJobType",
                        RequiredSkill = "bushcraft",
                        PhysicalWorkFactor = constructionExertion,
                        Stances = constructionStances,
                 
                        Inputs = new[] { new Input() { Entity = "item:sticks", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false },
                                    new Input() { Entity = "item:stones", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false }                                ,
                                    new Input() { Entity = "item:firegrassSod", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false }
                   },

                        Outputs = new[] {
                            new Output(){ EntityTypeToCreate = "structure:smokeOven", Amount = new OutputAmount(){ NoOfItems = 1 }}
                       
                            },
                        WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallPrimitiveShelter },
                        AgentActionState = AnimAction.Building,
                        AgentAnimationStates = new[] { AnimModifier.Improvised }

                    });
                #endregion
                #region constructAbatis1

                    listOfProcessTypes.Add(new ProcessType()
                    {
                        Name = constructingStructureName,
                        KeyName = "constructAbatis1",
                        JobTypeKey = "constructionJobType",
                        RequiredSkill = "bushcraft",
                        PhysicalWorkFactor = constructionExertion,
                        Stances = constructionStances,

                        Inputs = new[] { 
                            //new Input() { Entity = "item:smallTent", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = true }, 
                            new Input() { Entity = "item:spoakBranches", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false }                           
                        },

                        Outputs = new[] {
                            new Output(){ EntityTypeToCreate = "structure:abatis", Amount = new OutputAmount(){ NoOfItems = 1 }}
                       
                            },
                        WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeToBuildAbatis },
                        AgentActionState = AnimAction.Building,
                        AgentAnimationStates = new[] { AnimModifier.Improvised }

                    });
                    #endregion
                #region constructScarecrow
                    listOfProcessTypes.Add(new ProcessType()
                    {
                        Name = constructingStructureName,
                        KeyName = "constructScarecrow",
                        JobTypeKey = "constructionJobType",
                        RequiredSkill = "bushcraft",
                        PhysicalWorkFactor = constructionExertion,
                        Stances = constructionStances,

                        Inputs = new[]
                        {
                            new Input() { Entity = "item:twinklerPlating", Amount = new InputAmount() {NoOfItems = 1}, IsConsumed = false },
                            new Input() { Entity = "item:twinklerPheromone", Amount = new InputAmount() {NoOfItems = 1}, IsConsumed = false }
                        },

                        Outputs = new []
                        {
                            new Output(){ EntityTypeToCreate = "structure:scarecrow", Amount = new OutputAmount (){ NoOfItems = 1 }}
                        },
                        WorkOrTimeNeeded = new WorkOrTime () {DaysNeeded = timeToBuildAbatis },
                        AgentActionState = AnimAction.Building,
                        AgentAnimationStates = new[] { AnimModifier.Improvised }
                    });

                    #endregion
                #endregion

                    string customRepairName = "Doing maintenance";
                    const string partsConditionProcessSummary = "Repairing {0} to improve their condition.";

                    #region Custom repair processes
                    #region reconditionSpokBranchesTrimmedPart
                    listOfProcessTypes.Add(new ProcessType()
                    {
                        Name = customRepairName,
                        SummaryDescription = string.Format(partsConditionProcessSummary, "trimmed spoak branches"),
                        KeyName = "reconditionSpokBranchesTrimmedPart",                       
                        RequiredSkill = "construction",
                        PhysicalWorkFactor = constructionExertion,
                        Stances = constructionStances,
                        WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeReconditioningSurvivalPart },
                        RepairAction = RepairAction.PartsCondition,
                        AgentActionState = AnimAction.Mending,                      
                        AgentAnimationStates = new[] { AnimModifier.Improvised }
                    });
                    #endregion 
                    #region reconditionSpoakLeaves
                    listOfProcessTypes.Add(new ProcessType()
                    {
                        Name = customRepairName,
                        SummaryDescription = string.Format(partsConditionProcessSummary, "spoak leaves"),
                        KeyName = "reconditionSpoakLeaves",
                        RequiredSkill = "construction",
                        PhysicalWorkFactor = constructionExertion,
                        Stances = constructionStances,
                        WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeReconditioningSurvivalPart },
                        RepairAction = RepairAction.PartsCondition,
                        AgentActionState = AnimAction.Mending,
                        AgentAnimationStates = new[] { AnimModifier.Improvised }
                    });
                    #endregion 
                    #region reconditionDaysheenLeaves
                    listOfProcessTypes.Add(new ProcessType()
                    {
                        Name = customRepairName,
                        SummaryDescription = string.Format(partsConditionProcessSummary, "daysheen leaves"),
                        KeyName = "reconditionDaysheenLeaves",
                        RequiredSkill = "construction",
                        PhysicalWorkFactor = constructionExertion,
                        Stances = constructionStances,
                        WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeReconditioningSurvivalPart },
                        RepairAction = RepairAction.PartsCondition,
                        AgentActionState = AnimAction.Mending,
                        AgentAnimationStates = new[] { AnimModifier.Improvised }
                    });
                    #endregion 
                    #region reconditionSticks
                    listOfProcessTypes.Add(new ProcessType()
                    {
                        Name = customRepairName,
                        SummaryDescription = string.Format(partsConditionProcessSummary, "sticks"),
                        KeyName = "reconditionSticks",
                        RequiredSkill = "construction",
                        PhysicalWorkFactor = constructionExertion,
                        Stances = constructionStances,
                        WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeReconditioningSurvivalPart },
                        RepairAction = RepairAction.PartsCondition,
                        AgentActionState = AnimAction.Mending,
                        AgentAnimationStates = new[] { AnimModifier.Improvised }
                    });
                    #endregion 

                    #region repairPrimitiveIntegrity
                    listOfProcessTypes.Add(new ProcessType()
                    {
                        Name = customRepairName,
                        SummaryDescription = RepairType.integrityProcessSummary,
                        KeyName = "repairPrimitiveIntegrity",
                        RequiredSkill = "construction",
                        PhysicalWorkFactor = constructionExertion,
                        Stances = constructionStances,
                        WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForSmallCraftingTask },
                        RepairAction = RepairAction.Integrity,
                        ProcessToolSetKey = "toolSetCordage",
                        AgentActionState = AnimAction.Mending,
                        AgentAnimationStates = new[] { AnimModifier.Improvised }
                    });          

                    #endregion 
                    #region repairPrimitiveCondition
                    listOfProcessTypes.Add(new ProcessType()
                    {
                        Name = customRepairName,
                        SummaryDescription = RepairType.partsConditionProcessSummaryNoPlaceholder,
                        KeyName = "repairPrimitiveCondition",
                        RequiredSkill = "construction",
                        PhysicalWorkFactor = constructionExertion,
                        Stances = constructionStances,
                        WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeReconditioningSurvivalPart },
                        RepairAction = RepairAction.PartsCondition,                      
                        AgentActionState = AnimAction.Mending,
                        AgentAnimationStates = new[] { AnimModifier.Improvised }
                    });          

                    #endregion 
            ////
                    #region repairPrimitiveIntegrityWithDigging
                    listOfProcessTypes.Add(new ProcessType()
                    {
                        Name = customRepairName,
                        SummaryDescription = RepairType.integrityProcessSummary,
                        KeyName = "repairPrimitiveIntegrityWithDigging",
                        RequiredSkill = "construction",
                        PhysicalWorkFactor = constructionExertion,
                        Stances = constructionStances,
                        WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 0.7f * timeToCreatePit },
                        RepairAction = RepairAction.Integrity,
                        ProcessToolSetKey = "toolSetDiggingConstruction",//Changed from only needing trowel to need same tools as its being build with
                        AgentActionState = AnimAction.Digging
                    });

                    #endregion
                    #region repairPrimitiveIntegrityWithPlowing
                    listOfProcessTypes.Add(new ProcessType()
                    {
                        Name = customRepairName,
                        SummaryDescription = RepairType.integrityProcessSummary,
                        KeyName = "repairPrimitiveIntegrityWithPlowing",
                        RequiredSkill = "construction",
                        PhysicalWorkFactor = constructionExertion,
                        Stances = constructionStances,
                        WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 0.7f * timeToCreatePit },
                        RepairAction = RepairAction.Integrity,
                        ProcessToolSetKey = "toolSetPlowingTools", // "toolSetSmallDiggingConstruction",
                        AgentActionState = AnimAction.Digging
                    });

                    #endregion      



                    #endregion

                    #region ///Creature food extraction///



                    float twinklerExtractMeatTime = 0.001f; //0.003f MP want it faster , note this is not eating, eating the item time is defined in creatureloader:  TimeToConsumeFullMealInDays = 0.015f, 
                #region extractThunderChickenMeat
                    listOfProcessTypes.Add(new ProcessType()
                    {
                        Name = extractingName,
                        KeyName = "extractThunderChickenMeat",
                        //RequiredSkill = "menial", // skill???
                        PhysicalWorkFactor = moderateWork,
                        Stances = kneelingProduction,
                        Inputs = new[] { new Input() { Entity = "item:thunderChickenCarcass", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1, Substances = new[]{ "meat" }   } }},
                        Outputs = new[] { new Output() { EntityTypeToCreate = "item:thunderChickenShred", Amount = new OutputAmount() { Bulk = new Bulk() { InputSubstance = "meat" }  } } }, // Important to define the substance here, otherwise total bulk will be used. have mot specified number of items. It will be computed automatically. Have also removed maximum bulk on shred item.                 
                        WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = twinklerExtractMeatTime },
  
                    });
                #endregion
                #region twinklerExtractThunderChickenGuts (outcommented)
                    /* MP : guts are included in chicken shreds...
 
                    listOfProcessTypes.Add(new ProcessType()
                    {
                        Name = extractingName,
                        KeyName = "twinklerExtractThunderChickenGuts",
                    //    RequiredSkill = "menial",
                        PhysicalWorkFactor = moderateWork,
                        Stances = kneeling,
                        Inputs = new[] { new Input() { Entity = "item:thunderChickenCarcass", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1, Substances = new[] { "guts" } } } },
                        Outputs = new[] { new Output() { EntityType = "item:thunderChickenGuts", Amount = new OutputAmount() { NoOfItems = 1 } } },
                        WorkOrTimeNeeded = new WorkOrTime() { DaysOfWorkNeeded = twinklerExtractMeatTime },

                    });
    */
                #endregion
                #region extractBinalRatMeat
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = extractingName,
                    KeyName = "extractBinalRatMeat",
                    //RequiredSkill = "menial", // skill???
                    PhysicalWorkFactor = moderateWork,
                    Stances = kneelingProduction,
                    Inputs = new[] { new Input() { Entity = "item:binalRatCarcass", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1, Substances = new[] { "meat" } } } },
                    Outputs = new[] { new Output() { EntityTypeToCreate = "item:binalRatShred", Amount = new OutputAmount() { Bulk = new Bulk() { InputSubstance = "meat" } } } }, // Important to define the substance here, otherwise total bulk will be used. have mot specified number of items. It will be computed automatically. Have also removed maximum bulk on shred item.                                
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = twinklerExtractMeatTime },

                });
                #endregion
                #region extractTwinklerMeat
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = extractingName,
                    KeyName = "extractTwinklerMeat",
                    //RequiredSkill = "menial", // skill???
                    PhysicalWorkFactor = moderateWork,
                    Stances = kneelingProduction,
                    Inputs = new[] { new Input() { Entity = "item:quaditeCarcass", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1, Substances = new[] { "meat" } } } },
                    Outputs = new[] { new Output() { EntityTypeToCreate = "item:twinklerShred", Amount = new OutputAmount() { Bulk = new Bulk() { InputSubstance = "meat" } } } }, // Important to define the substance here, otherwise total bulk will be used. have mot specified number of items. It will be computed automatically. Have also removed maximum bulk on shred item.                                
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = twinklerExtractMeatTime },

                });
                #endregion
                #region extractDemonTreeMeat
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = extractingName,
                    KeyName = "extractDemonTreeMeat",
                    //RequiredSkill = "menial", // skill???
                    PhysicalWorkFactor = moderateWork,
                    Stances = kneelingProduction,
                    Inputs = new[] { new Input() { Entity = "item:demontreeCarcass", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1, Substances = new[] { "meat" } } } },
                    Outputs = new[] { new Output() { EntityTypeToCreate = "item:demonTreeShred", Amount = new OutputAmount() { Bulk = new Bulk() { InputSubstance = "meat" } } } }, // Important to define the substance here, otherwise total bulk will be used. have mot specified number of items. It will be computed automatically. Have also removed maximum bulk on shred item.                                
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = twinklerExtractMeatTime },

                });
                #endregion
                #region extractSwampDemonTreeMeat
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = extractingName,
                    KeyName = "extractSwampDemonTreeMeat",
                    //RequiredSkill = "menial", // skill???
                    PhysicalWorkFactor = moderateWork,
                    Stances = kneelingProduction,
                    Inputs = new[] { new Input() { Entity = "item:swampDemonTreeCarcass", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1, Substances = new[] { "meat" } } } },
                    Outputs = new[] { new Output() { EntityTypeToCreate = "item:swampDemonTreeShred", Amount = new OutputAmount() { Bulk = new Bulk() { InputSubstance = "meat" } } } }, // Important to define the substance here, otherwise total bulk will be used. have mot specified number of items. It will be computed automatically. Have also removed maximum bulk on shred item.                                
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = twinklerExtractMeatTime },

                });
                #endregion
                #region extractPatricianMeat
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = extractingName,
                    KeyName = "extractPatricianMeat",
                    //RequiredSkill = "menial", // skill???
                    PhysicalWorkFactor = moderateWork,
                    Stances = kneelingProduction,
                    Inputs = new[] { new Input() { Entity = "item:patricianCarcass", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1, Substances = new[] { "meat" } } } },
                    Outputs = new[] { new Output() { EntityTypeToCreate = "item:patricianShred", Amount = new OutputAmount() { Bulk = new Bulk() { InputSubstance = "meat" } } } }, // Important to define the substance here, otherwise total bulk will be used. have mot specified number of items. It will be computed automatically. Have also removed maximum bulk on shred item.                                
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = twinklerExtractMeatTime },

                });
                #endregion
                #region extractBushDragonMeat
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = extractingName,
                    KeyName = "extractBushDragonMeat",
                    PhysicalWorkFactor = moderateWork,
                    Stances = kneelingProduction,
                    Inputs = new[] { new Input() { Entity = "item:bushDragonCarcass", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1, Substances = new[] { "meat" } } } },
                    Outputs = new[] { new Output() { EntityTypeToCreate = "item:bushDragonShred", Amount = new OutputAmount() { Bulk = new Bulk() { InputSubstance = "meat" } } } }, // Important to define the substance here, otherwise total bulk will be used. have mot specified number of items. It will be computed automatically. Have also removed maximum bulk on shred item.                                
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = twinklerExtractMeatTime },

                });
                #endregion
                #region extractTurnipMeat
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = extractingName,
                    KeyName = "extractTurnipMeat",
                    PhysicalWorkFactor = moderateWork,
                    Stances = kneelingProduction,
                    Inputs = new[] { new Input() { Entity = "item:turnipCarcass", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1, Substances = new[] { "meat" } } } },
                    Outputs = new[] { new Output() { EntityTypeToCreate = "item:turnipShred", Amount = new OutputAmount() { Bulk = new Bulk() { InputSubstance = "meat" } } } }, // Important to define the substance here, otherwise total bulk will be used. have mot specified number of items. It will be computed automatically. Have also removed maximum bulk on shred item.                                
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = twinklerExtractMeatTime },

                });
                #endregion
                #region extractMegapodMeat
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = extractingName,
                    KeyName = "extractMegapodMeat",
                    PhysicalWorkFactor = moderateWork,
                    Stances = kneelingProduction,
                    Inputs = new[] { new Input() { Entity = "item:megapodCarcass", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1, Substances = new[] { "meat" } } } },
                    Outputs = new[] { new Output() { EntityTypeToCreate = "item:megapodShred", Amount = new OutputAmount() { Bulk = new Bulk() { InputSubstance = "meat" } } } }, // Important to define the substance here, otherwise total bulk will be used. have mot specified number of items. It will be computed automatically. Have also removed maximum bulk on shred item.                                
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = twinklerExtractMeatTime },

                });
                #endregion
                #region extractWhipjawMeat
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = extractingName,
                    KeyName = "extractWhipjawMeat",
                    PhysicalWorkFactor = moderateWork,
                    Stances = kneelingProduction,
                    Inputs = new[] { new Input() { Entity = "item:whipjawCarcass", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1, Substances = new[] { "meat" } } } },
                    Outputs = new[] { new Output() { EntityTypeToCreate = "item:whipjawShred", Amount = new OutputAmount() { Bulk = new Bulk() { InputSubstance = "meat" } } } }, // Important to define the substance here, otherwise total bulk will be used. have mot specified number of items. It will be computed automatically. Have also removed maximum bulk on shred item.                                
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = twinklerExtractMeatTime },

                });
                #endregion
                #region extractSpikePlantMeat
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = extractingName,
                    KeyName = "extractSpikePlantMeat",
                    PhysicalWorkFactor = moderateWork,
                    Stances = kneelingProduction,
                    Inputs = new[] { new Input() { Entity = "item:spikePlantCarcass", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1, Substances = new[] { "meat" } } } },
                    Outputs = new[] { new Output() { EntityTypeToCreate = "item:spikePlantShred", Amount = new OutputAmount() { Bulk = new Bulk() { InputSubstance = "meat" } } } }, // Important to define the substance here, otherwise total bulk will be used. have mot specified number of items. It will be computed automatically. Have also removed maximum bulk on shred item.                                
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = twinklerExtractMeatTime },

                });
                #endregion
                #region extractForestGuardianMeat
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = extractingName,
                    KeyName = "extractForestGuardianMeat",
                    PhysicalWorkFactor = moderateWork,
                    Stances = kneelingProduction,
                    Inputs = new[] { new Input() { Entity = "item:forestGuardianCarcass", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1, Substances = new[] { "meat" } } } },
                    Outputs = new[] { new Output() { EntityTypeToCreate = "item:forestguardianShred", Amount = new OutputAmount() { Bulk = new Bulk() { InputSubstance = "meat" } } } }, // Important to define the substance here, otherwise total bulk will be used. have mot specified number of items. It will be computed automatically. Have also removed maximum bulk on shred item.                                
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = twinklerExtractMeatTime },

                });
                #endregion
                #region extractThinThunderChickenMeat //not used, because that carcass is not used.
        /*        listOfProcessTypes.Add(new ProcessType()
                {
                    Name = extractingName,
                    KeyName = "extractThinThunderChickenMeat",
                    PhysicalWorkFactor = moderateWork,
                    Stances = kneelingProduction,
                    Inputs = new[] { new Input() { Entity = "item:thinThunderChickenCarcass", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1, Substances = new[] { "meat" } } } },
                    Outputs = new[] { new Output() { EntityTypeToCreate = "item:thinThunderChickenShred", Amount = new OutputAmount() { Bulk = new Bulk() { InputSubstance = "meat" } } } }, // Important to define the substance here, otherwise total bulk will be used. have mot specified number of items. It will be computed automatically. Have also removed maximum bulk on shred item.                                
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = twinklerExtractMeatTime },

                });*/
                #endregion
                #region extractLeafCutterMeat
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = extractingName,
                    KeyName = "extractLeafCutterMeat",
                    PhysicalWorkFactor = moderateWork,
                    Stances = kneelingProduction,
                    Inputs = new[] { new Input() { Entity = "item:leafcutterCarcass", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1, Substances = new[] { "meat" } } } },
                    Outputs = new[] { new Output() { EntityTypeToCreate = "item:leafcutterShred", Amount = new OutputAmount() { Bulk = new Bulk() { InputSubstance = "meat" } } } }, // Important to define the substance here, otherwise total bulk will be used. have mot specified number of items. It will be computed automatically. Have also removed maximum bulk on shred item.                                
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = twinklerExtractMeatTime },

                });
                #endregion
                #region extractMudWormMeat
                listOfProcessTypes.Add(new ProcessType()
                {
                    Name = extractingName,
                    KeyName = "extractMudWormMeat",
                    PhysicalWorkFactor = moderateWork,
                    Stances = kneelingProduction,
                    Inputs = new[] { new Input() { Entity = "item:mudWormCarcass", IsConsumed = true, Amount = new InputAmount() { NoOfItems = 1, Substances = new[] { "meat" } } } },
                    Outputs = new[] { new Output() { EntityTypeToCreate = "item:mudWormShred", Amount = new OutputAmount() { Bulk = new Bulk() { InputSubstance = "meat" } } } }, // Important to define the substance here, otherwise total bulk will be used. have mot specified number of items. It will be computed automatically. Have also removed maximum bulk on shred item.                                
                    WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = twinklerExtractMeatTime },

                });
                #endregion
                #endregion

  

                return listOfProcessTypes;


        }

        /// <summary>
        /// tut overrides this with some changes
        /// </summary>
        /// <param name="listOfProcessTypes"></param>
        public static ProcessType CreateHarvestSpoakBranches(List<ProcessType> listOfProcessTypes)
        {
            ProcessType process = new ProcessType()
            {
                Name = harvestingName,
                KeyName = "harvestSpoakBranches",
                RequiredSkill = "menial",
                JobTypeKey = "gatherMaterialsJobType",
                IsGathering = true,
                MoveOutputToWorkerWhenCompleted = false, // seems more natural to leave them on the ground
                PhysicalWorkFactor = moderateHardWork,
                Stances = standingProduction,

                Inputs = new[] { new Input() { Entity = "tree:spoak", Amount = new InputAmount() { NoOfItems = 1 } } },

                Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:spoakBranches", Amount = new OutputAmount(){ NoOfItems = 1 }}
                        },
                WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = timeForTinyHarvestingTask },
                ProcessToolSetKey = "toolSetChopToughWood",
                AgentActionState = AnimAction.Harvesting,
                AgentAnimationStates = new[] { AnimModifier.Low }
            };
            listOfProcessTypes.Add(process);

            return process;
        }

    }
}

