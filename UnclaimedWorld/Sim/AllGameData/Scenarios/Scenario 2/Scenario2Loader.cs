using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Scenarios;
using UWGame.SimSide.InGameEvents.Actions;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.AllGameData.Scenarios.Scenario_2.Data;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_2 // I chose a generic name so scenario name changes don't affect us so much...
{
    /// <summary>
    /// 
    ///  data priority: base data - mods - map - scenario 
    /// load base data first, then mods, then map, then scenario - overwrite keys/delete them
    /// 
    /// Scenarios are special!!!
    /// They are split in a header and a body part (2 files) to improve efficieny on the scenario list screen (only headers need to be loaded to show the brief descriptions)
    /// 
    /// Write mode: Write all vanilla scenarios
    /// NoSerialize: Create header instances on the scenario selection page - create scenario data after selection.
    /// Read: Read header instances on the scenario selection page - read scenario data after selection.
    /// </summary>
    public class Scenario2Loader : ScenarioLoader // I chose a generic name so scenario name changes don't affect us so much...
    {
        //private const string folderName = ;


        public override string FolderName
        {
            get { return "Muckroot Research Station"; }
        }

        protected override Scenario InitScenarioHeader()
        {
            Scenario scenario;

            scenario = new Scenario()
            {                
                Name = FolderName,
                DisplayName = "Muckroot Research Station",
                TimeDateYear = new DateAndTime.TimeDateYear() { Year = 11, Day = 3, TimeOfDay = 0.36 },
                MapKey = "h Muckroot Sandbox 128", //
                MapSize = SimSide.Scenarios.MapSize.Large,
                SummaryDescription = "-WORK IN PROGRESS!- A group of scientists set up an outpost in an enigmatic biome. If the area proves habitable, more people may join them. \n \nERA: Planetfall",
                Allow32Bit = false,

                Description = "-NOTE! This scenario is UNDER DEVELOPMENT!-\n \n MISSION: Assess the value of the muckroot biome as a place for settlement.\n OBJECTIVES: Explore the muckroot biome and establish a permanent research base in the vicinity.\n ASSETS: Additional supplies will be provided by air transport. Also, personnel from Duke's Landing have expressed an interest in joining the expedition at a later stage.", //                    
                ThumbnailImage = "Scenarios/Scenario 2/Scenario Screen/muckrootCamp_scenario_thumb",
                Image = "Survival",
                IsInDevelopment = true,
                SortOrder = 8             
            };


            return scenario;
        }

        public override DataLoader GetDataLoader()
        {
            DataLoader dataLoader = new Scenario2DataLoader();
            dataLoader.FolderName = FolderName;

            return dataLoader;
        }

        protected override ScenarioData InitScenarioData()
        {
            //List<PersonalityType> list = new List<PersonalityType>();

            ScenarioData scenarioData = null;
                        
           
                scenarioData = new ScenarioData()
                {
                    LoadingBackgroundImage = "Scenarios/Default/Scenario Screen/TitleImgSurvival_1920px",
//
                    LoadingDialogText = "MISSION: Assess the value of the muckroot biome as a place for settlement.\n OBJECTIVES: Explore the muckroot biome and establish a permanent research base in the vicinity.\n ASSETS: Additional supplies will be provided by air transport. Also, personnel from Duke's Landing have expressed an interest in joining the expedition at a later stage.",  
              
                    LoadingDialogImage = "FleeingSkimmer",

                    SpawnWorldAction = "spawnWorld",
                    SpawnSiteAction = "spawnPlaySite", //this, the 'we are here' place and the off map sites are defined in EventActionLoader

                    WorldMapImage = "Scenarios/Default/GUI/regionalMap_1",

                    EnableMissions = true,

                    //global events to register regardless of customization/difficulty:
                    ConditionalEvents = new[] {                        
                        "MUCKROOTMAP_loseGame",
                        "MUCKROOTMAP_musicTrackList",

                        "MUCKROOTMAP_continualSpawnTwinklersNorthWestCrevice",

                        "MUCKROOTMAP_continualSpawnBinalRatsNorthWest1",
                        "MUCKROOTMAP_continualSpawnBinalRatsNorthWest2",
                        "MUCKROOTMAP_continualSpawnBinalRatsNorthWest3",

                        "MUCKROOTMAP_continualSpawnThunderChickenNorthWest",

                        "MUCKROOTMAP_timedSpawnBeginningPopulationNormal"
                        },

                    // these are actions that are always performed regardless of difficulty:
                    Actions = new string[]
                      {
                  #region init game over, burial text
                          "initIncludeDateInBurial", "initBurialText2", "initBurialText3", "initBurialText4", "initBurialText5",// <- LARS' FINISHED
                           "initGameOver",
            #endregion

                            #region Group meetings
                           "initEnableGroupMeetings", "initTimeBeforeGroupMeeting",                        
                          "meeting3Security", "meetingEmigrateThreat", "meetingEmigrateThreatAllUnhappy",        
                          "meeting3Food", 
                          "meeting3Comfort", 
                        #endregion

                           #region Policy adopted
                           "comfortBasicPolicyAdopted", "foodBasicPolicyAdopted", "securityBasicPolicyAdopted",
                           "comfortMediumPolicyAdopted", "foodMediumPolicyAdopted", "securityMediumPolicyAdopted",
                           "comfortAdvancedPolicyAdopted", "foodAdvancedPolicyAdopted", "securityAdvancedPolicyAdopted",

                            "comfortBasicPolicyAdoptedAllAgree", "foodBasicPolicyAdoptedAllAgree", "securityBasicPolicyAdoptedAllAgree",
                           "comfortMediumPolicyAdoptedAllAgree", "foodMediumPolicyAdoptedAllAgree", "securityMediumPolicyAdoptedAllAgree",
                           "comfortAdvancedPolicyAdoptedAllAgree", "foodAdvancedPolicyAdoptedAllAgree", "securityAdvancedPolicyAdoptedAllAgree",
                        #endregion

                        #region Emigrate dialogs
                          "initEmigrateSecurityDialogText", "initEmigrateComfortDialogText", "initEmigrateFoodDialogText",                         
                          "initEmigrateSecurityNoConversationDialogText", "initEmigrateComfortNoConversationDialogText", "initEmigrateFoodNoConversationDialogText",
                        #endregion

                  #region animal expeditions
                          "spawnDemonTreeExpedition#1", "spawnDemonTreeExpedition#2", "spawnDemonTreeExpedition#3", //tree demons need very specific expeditions sites, must not move outside spoak forests
                // "spawnThunderChickenAllegiance", "placeThunderChickenExpedition",
                  #endregion
                  #region animal popcap                
                "setMaxTwinklersLow",
                #endregion                        

                         "placeExpedition", "spawnPlayerAllegiance",
                         "setPlayerCredits", // make this difficulty dependent  
                      
                         "setView", "exploreEntireMap",
                          
#region Spawn otherSite1 and 2               
                          "spawnOtherSite1",  "spawnOtherSite1Allegiance1", "spawnOtherSite1Expedition1" ,                                ///  "spawnHomeBaseContract", MP what's this?
                          "spawnOtherSite2", "spawnOtherSite2Allegiance1","spawnOtherSite2Expedition1",
#endregion
#region otherSite1Expedition1 ///structures                    
                          
                          "spawnOtherSite1Expedition1Helipad", "spawnOtherSite1Expedition1SatelliteGroundStation", "spawnOtherSite1Expedition1Skimmer", "spawnOtherSite1Expedition1Harpy", //"startHarpyTest",
#endregion
#region otherSite1Expedition1 ///migrants
                        "spawnMuckrootRandomImmigrant", "spawnMuckrootRandomImmigrant","spawnMuckrootRandomImmigrant","spawnMuckrootRandomImmigrant","spawnMuckrootRandomImmigrant","spawnMuckrootRandomImmigrant","spawnMuckrootRandomImmigrant","spawnMuckrootRandomImmigrant","spawnMuckrootRandomImmigrant","spawnMuckrootRandomImmigrant",
#endregion 
#region otherSite1Expedition1////GOODS available to BUY
#region FOOD
"otherSite1Expedition1_precolRation","otherSite1Expedition1_precolRation","otherSite1Expedition1_precolRation","otherSite1Expedition1_precolRation","otherSite1Expedition1_precolRation","otherSite1Expedition1_precolRation","otherSite1Expedition1_precolRation","otherSite1Expedition1_precolRation","otherSite1Expedition1_precolRation","otherSite1Expedition1_precolRation",
"otherSite1Expedition1_simCoffeeBeans","otherSite1Expedition1_simCoffeeBeans","otherSite1Expedition1_simCoffeeBeans","otherSite1Expedition1_simCoffeeBeans","otherSite1Expedition1_simCoffeeBeans","otherSite1Expedition1_simCoffeeBeans","otherSite1Expedition1_simCoffeeBeans",
#endregion
#region WEAPONS
"otherSite1Expedition1_sentry","otherSite1Expedition1_sentry","otherSite1Expedition1_sentry",
"otherSite1Expedition1_coilRifle","otherSite1Expedition1_coilRifle","otherSite1Expedition1_coilRifle","otherSite1Expedition1_coilRifle",

#endregion
#region  RAW MATERIALS                         
"otherSite1Expedition1_acetylene", "otherSite1Expedition1_acetylene","otherSite1Expedition1_acetylene", 
"otherSite1Expedition1_liquidGas","otherSite1Expedition1_liquidGas","otherSite1Expedition1_liquidGas","otherSite1Expedition1_liquidGas",
"otherSite1Expedition1_ironCanister","otherSite1Expedition1_ironCanister","otherSite1Expedition1_ironCanister","otherSite1Expedition1_ironCanister",

"otherSite1Expedition1_inactivatedFoodCoolerUnit",
"otherSite1Expedition1_textile","otherSite1Expedition1_textile","otherSite1Expedition1_textile",
"otherSite1Expedition1_smallTent",
"otherSite1Expedition1_octagonalTent",
"otherSite1Expedition1_domeTent","otherSite1Expedition1_domeTent","otherSite1Expedition1_domeTent",
"otherSite1Expedition1_thermalTarp","otherSite1Expedition1_thermalTarp",
#endregion

            #region AMMUNITION
"otherSite1Expedition1_sentryGunAmmo","otherSite1Expedition1_sentryGunAmmo","otherSite1Expedition1_sentryGunAmmo","otherSite1Expedition1_sentryGunAmmo","otherSite1Expedition1_sentryGunAmmo",
"otherSite1Expedition1_coilRifleAmmo","otherSite1Expedition1_coilRifleAmmo","otherSite1Expedition1_coilRifleAmmo","otherSite1Expedition1_coilRifleAmmo","otherSite1Expedition1_coilRifleAmmo","otherSite1Expedition1_coilRifleAmmo","otherSite1Expedition1_coilRifleAmmo",
   #endregion

#region TOOLS                                                
"otherSite1Expedition1_metalworkersToolbox",                   
"otherSite1Expedition1_hammer",  "otherSite1Expedition1_hammer",
"otherSite1Expedition1_knife","otherSite1Expedition1_knife","otherSite1Expedition1_knife","otherSite1Expedition1_knife",
"otherSite1Expedition1_metalWire",  "otherSite1Expedition1_metalWire", "otherSite1Expedition1_metalWire", 
"otherSite1Expedition1_snips",
"otherSite1Expedition1_cookingPot","otherSite1Expedition1_cookingPot",
"otherSite1Expedition1_machete","otherSite1Expedition1_machete","otherSite1Expedition1_machete","otherSite1Expedition1_machete",
"otherSite1Expedition1_steelPickaxe","otherSite1Expedition1_steelPickaxe","otherSite1Expedition1_steelPickaxe",
"otherSite1Expedition1_string", "otherSite1Expedition1_string", "otherSite1Expedition1_string", "otherSite1Expedition1_string", "otherSite1Expedition1_string", "otherSite1Expedition1_string", 

#endregion
#region STRUCTURE ITEMS and PARTS
"otherSite1Expedition1_sensor","otherSite1Expedition1_sensor","otherSite1Expedition1_sensor",
   #endregion 
   #endregion 



                  #region particle effects 
                  "smallFog1", "smallFog2", "smallFog3","smallFog4","smallFog5","smallFog6","smallFog7","haze1", //  "sulphurousSmoke1", "haze2",
                     #endregion                    
                      },

                    #region MainDifficultySettings
                    MainDifficultySettings = new[]
                     {
                         /*new Difficulty()
                         {
                              KeyName = "veryEasy",
                              Name = "Very easy"
                         },
                         new Difficulty()
                         {
                              KeyName = "easy",
                              Name = "Easy",
                                OptionsToUse = new SerializableDictionary<string, string[]>()
                                {
                                    { "startingLocations", new []{ "southCoast", "northCoast" } },
                                   // { "people", new []{ "6people" } },
                                    { "equipment", new []{ "lotsOfRiflesSomeGear" } },
                                    { "fauna" , new []{ "benign" }},
                                    { "resources" , new []{ "plenty" }},
                                    { "otherSite1Allegiance1Relation", new []{ "friendly" }}
                                }
                         },*/
                          new Difficulty()
                         {
                              KeyName = "normal",
                              Name = "Normal",
                              IsDefault = true,
                                OptionsToUse = new SerializableDictionary<string, string[]>()
                                {
                                    { "startingLocations", new []{ "southCoast"/*, "northCoast"*/ } },
                                    { "people", new []{ "7people" } }, 
                                    { "equipment", new []{ "muckrootStationGear" } } ,
                                   // { "fauna" , new []{ "average" }},
                                    { "resources" , new []{ "average" }},
                                    { "otherSite1Allegiance1Relation", new []{ "neutral" }}
                                }
                         }/*,
                          new Difficulty()
                         {
                              KeyName = "hard",
                              Name = "Hard",
                                OptionsToUse = new SerializableDictionary<string, string[]>()
                                {
                                    { "startingLocations", new []{ "southCoast", "northCoast" } },
                                 //   { "people", new []{ "3people" } }, 
                                    { "equipment", new []{ "fewSupplies" } } ,
                                    { "fauna" , new []{ "fierce" }},
                                    { "resources" , new []{ "average" }},
                                    { "otherSite1Allegiance1Relation", new []{ "neutral" }} 
                                }                            
                         },
                        new Difficulty()
                         {
                              KeyName = "veryHard",
                              Name = "Very hard" ,
                                OptionsToUse = new SerializableDictionary<string, string[]>()
                                {
                                    { "startingLocations", new []{ "southCoast", "northCoast" } },
                                 //   { "people", new []{ "2people" } }, 
                                    { "equipment", new []{ "fewSupplies" } },
                                    { "fauna" , new []{ "fierce" }},
                                    { "resources" , new []{ "sparse" }},
                                    { "otherSite1Allegiance1Relation", new []{ "hostile" }}   
                                }                            
                         },
                          new Difficulty()
                         {
                              KeyName = "impossible",
                              Name = "Impossible"                             
                         }*/

                     },
                    #endregion

                    OptionSets = new[]
                     {
                    #region startingLocations
                          new OptionSet(){
                              KeyName = "startingLocations",
                               Name = "Camp site",
                               DisplayGroup = 0,
                                Options = new[] { 
                                    new Option()
                                    {
                                      KeyName = "southCoast",
                                      Name = "Beach",
                                     
                                   //   LoadingDialogText = " \nWe have landed on the coast", LoadingDialogTextMode = Option.LoadingDialogTextModes.Append, LoadingDialogTextOrder = 5,

                                      Difficulty = new CustomDifficulty()
                                      {
                                           KeyName = "normal",
                                           Name = "Coastline",
                                           //Name = "Normal",    
                                           ScoreModifier = 1f
                                      },
                             //          ConditionalEvents = new[]{"startSkimmerTailSouth" },
                                       ActionKeys = new[]{ "setStartingLocationSouth" } 
                                      
                                    }                                 
                                }
                               
                          },
#endregion
                    #region people
                          new OptionSet(){
                               KeyName = "people",
                               Name = "Camp members",
                                DisplayGroup = 1,
                                Options = new[] { 
                                    new Option()
                                    {
                                      KeyName = "7people",
                                      Name = "7 people",
                                      ShortDescription = "Group of 7 people",     

                                   //   LoadingDialogText = "\nRECORDED BY: Ward Conlan. ALSO PRESENT: Joaquin Lehner, Augustine Yeboah and Ilya Khan \n \nTEXT", 
                                  //    LoadingDialogTextMode = Option.LoadingDialogTextModes.Append,

                                      Difficulty = new CustomDifficulty()
                                      {
                                           KeyName = "normal",
                                            IsDefault = true,
                                           //Name = "Normal",
                                           Name = "7 people",
                                           ScoreModifier = 1f
                                      },
                                   //   ConditionalEvents = new[]{"MUCKROOTMAP_introDialogue3People", "MUCKROOTMAP_onlyThreeMembersLeftDialogue", "MUCKROOTMAP_onlyTwoMembersLeftDialogue", "MUCKROOTMAP_onlyOneMemberLeftDialogue",},
                                      ActionKeys = new[]{ "spawnRandomMuckrootPerson1", "spawnRandomMuckrootPerson2","spawnRandomMuckrootPerson3","spawnRandomMuckrootPerson4","spawnRandomMuckrootPerson5","spawnRandomMuckrootPerson6", "spawnRandomMuckrootPerson7",  "spawnHaulRobot" }  //mp i removed the dog because it was too similar to the other tau ceti maps: "spawnGuardAnimal" also to discourage people from playing this map.   //,,  ------------------------"spawnGunDog" "spawnKwena", ,"spawnPerson1", "spawnPerson2", "spawnPerson3", "spawnPerson4", "spawnPavoulet",                      
                                    }

                                }
                          }, 
#endregion
                    #region equipment
                          new OptionSet(){
                               KeyName = "equipment",
                               Name = "Supplies",
                                DisplayGroup = 0,
                                Options = new[] { 
                                   /* new Option()
                                    {
                                      KeyName = "lotsOfRiflesSomeGear",
                                      Name = "Many rifles, some gear",
                                      Difficulty = new CustomDifficulty()
                                      {
                                           KeyName = "veryEasy",
                                           Name = "Many rifles",
                                           ScoreModifier = 0.3f
                                      },
                                     // ConditionalEvents = new[]{"MUCKROOTMAP_introDialogueLotsOfSupplies", "MUCKROOTMAP_introSuggestionLotsOfSupplies"},
                                       ActionKeys = new[]
                                       {
                                             "startKnife", "startKnife", "startKnife", "startSnips", "startString", "startString", "startThermalTarp", "startDomeTent", "startOctagonalTent", "startSmallTent",
                                               "startHuntingRifle", "startRifleAmmo",  "startHuntingRifle", "startRifleAmmo",  "startHuntingRifle", "startRifleAmmo", "startRifleAmmo", 
                                                "startMachete", "startBasicFireExtinguisher", "startSensor", "startRation", "startRation", "startRation"
               
                                       }                                     
                                    },*/
                                    new Option()
                                    {
                                      KeyName = "muckrootStationGear",
                                      Name = "Muckroot station gear",
                                      Difficulty = new CustomDifficulty()
                                      {
                                           KeyName = "normal",
                                           Name = "Some gear",
                                           IsDefault = true,
                                           ScoreModifier = 1f
                                      },
                                     //ConditionalEvents = new[]{"MUCKROOTMAP_introDialogueSomeSupplies", "MUCKROOTMAP_introSuggestionSomeSupplies"},
                                       ActionKeys = new[]
                                       {
                                          
                                               "startKnife", "startKnife", "startKnife", "startSnips", "startString", "startString", "startThermalTarp", /*"startDomeTent",*/ "startOctagonalTent", "startSmallTent",                                           
                                               "startHuntingRifle", "startHuntingRifle", "startHuntingRifle", "startHuntingRifle", "startHuntingRifle", 
                                               "startSentry", "startSentryAmmo",
                                               "startRifleAmmo", "startRifleAmmo", "startRifleAmmo", "startRifleAmmo", "startRifleAmmo",
                                               "startMachete", "startSensor", "startCookingPot",
                                               //"startFieldKitchenStove", "startFieldKitchenEquipment",
                                               "startWeatherStationMast", "startWeatherStationSensors",
                                               //"startSatelliteGroundStation",
                                               "startLiquidGas", "startLiquidGas", "startLiquidGas", "startLiquidGas", "startLiquidGas",
                                               "startPaint","startPaint","startPaint","startPaint","startPaint","startPaint","startPaint","startPaint","startPaint",
                                               "startSimCoffeeBeans", "startSimCoffeeBeans", 
                                               //"startIronCanisterForSale", "startIronCanisterForSale",
                                              
                                               "startAssemblerCabinet", "startVacuumChamber", "startAssemblerCooling",
                                               "startAssemblerPlateA", "startAssemblerMasterPlateA", "startAcetylene", "startAcetylene", "startAcetylene", "startAcetylene",
                                               "startAssemblerPlateB", "startAssemblerMasterPlateB", "startIronCanister",
                                               "startStructureDomeTent",  "startStructureSmallTent", "startStructureFieldKitchen", "startStructureHelipadBig", "startStructureGroundStation",
                                                    
               
                                       }                                     
                                    }

                                }
                          },

#endregion                      
                    #region resources        
                            new OptionSet(){
                              KeyName = "resources",
                               Name = "Resources",
                               DisplayGroup = 2,
                                Options = new[] { 
                                   /* new Option()
                                    {
                                      KeyName = "plenty",
                                      Name = "Plenty",
                                     
                                      Difficulty = new CustomDifficulty()
                                      {
                                           KeyName = "easy",
                                           Name = "Plenty",
                                           ScoreModifier = 1f
                                      },
                                       ActionKeys = new[]{ "setFishSchoolMedium", "setPlentyResources" } // add noise with added weight // "setPlentyResources"
                                      
                                    },*/

                                    new Option()
                                    {
                                      KeyName = "average",
                                      Name = "Average",
                                     
                                      Difficulty = new CustomDifficulty()
                                      {
                                           KeyName = "normal",
                                           Name = "Average",
                                           IsDefault = true,
                                           ScoreModifier = 1f
                                      },
                                       ActionKeys = new[]{ "setFishSchoolMedium", "setNormalResources"}   // add noise with neutral weight                                   
                                    }/*,                                   
                                    new Option()
                                    {
                                      KeyName = "sparse",
                                      Name = "Sparse",
                                     
                                      Difficulty = new CustomDifficulty()
                                      {
                                           KeyName = "hard",
                                           Name = "Sparse",
                                           //Name = "Normal",
                                           ScoreModifier = 1f
                                      },                                     
                                      ActionKeys = new[]{ "setFishSchoolSparse", "setSparseResources"} // add noise with decreased weight
                                    }*/
                                }
                            },
#endregion                              
                    #region  otherSite1Allegiance1Relation          
                            new OptionSet(){
                              KeyName = "otherSite1Allegiance1Relation",
                               Name = "Other Site 1 Relation",
                               DisplayGroup = 2,
                                Options = new[] { 
                                   /* new Option()
                                    {
                                      KeyName = "friendly",
                                      Name = "Friendly",
                                     
                                      Difficulty = new CustomDifficulty()
                                      {
                                           KeyName = "easy",
                                           Name = "Friendly",
                                           ScoreModifier = 1f
                                      },
                                       ActionKeys = new[]{"spawnFriendlyOtherSite1Allegiance1Relation" }
                                      
                                    },*/

                                    new Option()
                                    {
                                      KeyName = "neutral",
                                      Name = "Neutral",
                                     
                                      Difficulty = new CustomDifficulty()
                                      {
                                           KeyName = "normal",
                                           Name = "Neutral",
                                           IsDefault = true,
                                           ScoreModifier = 1f
                                      },
                                       ActionKeys = new[]{"spawnNeutralOtherSite1Allegiance1Relation"}                               
                                    }/*,                                   
                                    new Option()
                                    {
                                      KeyName = "hostile",
                                      Name = "Hostile",
                                     
                                      Difficulty = new CustomDifficulty()
                                      {
                                           KeyName = "hard",
                                           Name = "Hostile",
                                           ScoreModifier = 1f
                                      },                                     
                                      ActionKeys = new[]{"spawnHostileOtherSite1Allegiance1Relation"}
                                    }*/
                                }
                            }
                #endregion
                }
                };

           /* }
            else
            {
                BaseDataLoader.DeserializeObject(FolderName, "scenarioData.xml", out scenarioData, Config.DataType.RGScenario);
                
            }*/

            return scenarioData;
            
        }
    } 
}
