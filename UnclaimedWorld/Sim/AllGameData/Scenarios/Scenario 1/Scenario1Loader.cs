using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Scenarios;
using UWGame.SimSide.InGameEvents.Actions;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.AllGameData.Scenarios.Scenario_1.Data;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_1 // I chose a generic name so scenario name changes don't affect us so much...
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
    public class Scenario1Loader : ScenarioLoader // I chose a generic name so scenario name changes don't affect us so much...
    {
        //private const string folderName = ;


        public override string FolderName
        {
            get { return "Twinkler Island"; } //
        }

        protected override Scenario InitScenarioHeader()
        {
            Scenario scenario;
            
                scenario = new Scenario()
                {                    
                 
                    Name = FolderName, 
                    DisplayName = "Twinkler Island",
                    TimeDateYear = new DateAndTime.TimeDateYear(){ Year = 0, Day = 1, TimeOfDay = 0.36 },
                    MapKey = "fx DemoIsland",
                    MapSize = SimSide.Scenarios.MapSize.Small,
                    SummaryDescription = "SCENARIO: After escaping a deadly swarm, a handful of PRECOL explorers crash-land on an island and must survive until help arrives. \n \nERA: Planetfall",
                    Description = "SCENARIO. Playthrough 2-3 hrs. \n \n'AUDIO LOG: We have escaped the catastrophic attack by quadites that occurred just hours ago at Colony #1. We managed to get away in an aircraft but after a short flight we were forced to crash-land on a nearby island. \nWe've landed in a firegrass biome. Geographical data are incomplete, but we know enough about this environment to expect large numbers of quadites.'", // 
                    Allow32Bit = true,

                    // Scenarios\Scenario 1\Scenario Screen\aircraftWreck_scenario_thumb.png
                    ThumbnailImage = "Scenarios/Scenario 1/Scenario Screen/aircraftWreck_scenario_thumb",
                    Image = "Survival", 
                    SortOrder = 3,
                    IsInDevelopment = false, //this is the new twinkler island 1x

                };
               
          

            return scenario;
        }

        public override DataLoader GetDataLoader()
        {
            DataLoader dataLoader = new Scenario1DataLoader();
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

          //          LoadingDialogText = "JOURNAL ENTRY #1 \nDATE: 03-10 2238 \nLOCATION: 43 22.5N 124 17.7W ", //    \nRECORDED BY: Ward Conlan. ALSO PRESENT: Joaquin Lehner, Augustine Yeboah and Ilya Khan \n \nWe have no way of contacting the other mission members as long as our satellite transmitter is defective. Until connection is back we will keep a locally stored journal. This is the first entry. \n  \nWe have escaped the catastrophic attack by quadites that happened app. 2 hours ago. Only minor injuries are reported. Our vehicle however, is non-functional after damage sustained in the attack and a subsequent crash-landing. \n  \nWe've landed in a firegrass biome. Geographical data are incomplete, and this biome has only been partially documented during the previous months of research. Still, we know enough about this environment to expect species of quadites. \n \nWe only hope not to see the vicious swarmer quadite that attacked us earlier today.",
                    //I commented out this general header LoadingDialogText. use it for a top header or text section that should be displayed regardless of custom settings.
                    LoadingDialogImage = "FleeingSkimmer",
                    LoadingDialogHeading = "JOURNAL",

                    SpawnWorldAction = "spawnWorld",
                    SpawnSiteAction = "spawnPlaySite",

                    WorldMapImage = "Scenarios/Default/GUI/regionalMap_4",


                    EnableMissions = false,
                     
                    //global events to register regardless of customization/difficulty: global events are characterized by having a continous polling or time condition as a condition. (If time=0 it is a start event and then it should Not be here, and should go to eventactionloader just like events with other conditions should go to eventactionloader)
                    //however, if an event has several actions in it, such as an event with multiple spawns after each other, then it needs to go through globalconditionaleventsloader and then be called with ActionSetsKey from ActionSetsLoader. because only single-batch spawns can be called from scenarioloader.
                    ConditionalEvents = new[] {                     
                        "DEMOISLANDMAP_musicTrackList",
                        "DEMOISLANDMAP_winGame",
                        "DEMOISLANDMAP_minorWinGame",
                        "DEMOISLANDMAP_checkIfOnlyOneMemberLeft",
                        "DEMOISLANDMAP_checkIfOnlyTwoMembersLeft",
                        "DEMOISLANDMAP_checkIfOnlyThreeMembersLeft",
                        "DEMOISLANDMAP_loseGame",
                        "checkBushDragonDetectedShortDelay",
                        "checkBushDragonDetectedLongDelay",
                        "DEMOISLANDMAP_midnightText",                   
                        "DEMOISLANDMAP_rescueAndPRECOLDialogue",
                        "DEMOISLANDMAP_triggerVolcanicLandscape",
                        "DEMOISLANDMAP_triggerMuckroot",

 //////////////////Necessary for enabling farming and fish traps (they call docs outside of the scenario files):
                        "initializeGlobalFarmingProperties",                       
                        "initializeGlobalFishTrapProperties",
                        "initializeGlobalAnimalTrapProperties",
                        //"fishTrapCounter", //bso outcommented because the old system have been replaced
                        //"fishTrapSpawningLoop", //bso outcommented because the old system have been replaced
                        //"fishTrapCheckingLoop", //bso outcommented because the old system have been replaced
///////////////////////////////////////////////////  



                        //MP: twinkler spawn events are called under fauna settings -further below. I spawn an allegiance "DEMOISLANDMAP_spawnTwinklerAllegiance" to keep them scouting within a circle.

                        "DEMOISLANDMAP_continualSpawnBinalRatsSouthWest", //MP  binalrats expeditions are created automatically 
                        "DEMOISLANDMAP_continualSpawnBinalRatsCenterEast",
                        "DEMOISLANDMAP_continualSpawnBinalRatsNorth",

                        //Bush dragons spawned once and their expeditions are created automatically.

                        "DEMOISLANDMAP_continualSpawnThunderChickensNorth", //MP thunder chicks expeditions are set by designer in the action below ("DEMOISLANDMAP_spawnThunderChickenAllegianceNorth")
                        "DEMOISLANDMAP_continualSpawnThunderChickensSouth",

                   //     "SpawnAllTheFishAndFarms" // comment back in to enable loading of all the fish and farms via the globalConditionalEventLoader
                    },

                    //hvad er så forskellen på actionsetsloader og eventactionloader: actionsets har flere betingelser, for at gøre dialoger lettere at skrive. actionsets indeholder flere eventactions.

  ////////////////// these are actions that are always performed regardless of difficulty:
                    Actions = new string[]
                      {

//these lines are for testing purposes:---!!!!!!!---COMMENT OUT BEFORE DEPLOY----------------------------!!!!!-----------------------
// "startSpear", "startBait", "startHook", "startHoe", "startHoe", "startCreeperPods", "startCreeperPods", "startCrystalBerries", "startCrystalBerries",
       //      "startSpear",
             //           "startTwinklerMeat", "startTwinklerMeat", "startTwinklerMeat",
              //         "startVine",
            //           "startVine",
    //                    "startSulfurBomb",
             //           "startEnzyme",
          //   "startWaterCaneStem",
           //  "startVat",
  //           "startImprovedFireExtinguisher",
  //             "startBushDragonCartridge",
  //             "startBushDragonCartridge",
  //             "startSentry",
  //             "startSentryGunAmmo",
 //              "startSprayGunSentry",
 //              "startShotgunSentry",
 //              "startShotgunAmmo",
 //              "startShotgunAmmo",
 //              "startShotgun",
//-------------------------------------------------!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!--------------------------------------------


                  //      "DEMOISLANDMAP_spawnTwinklerAllegianceSouth", // only one allegiance, else they fight eachother
                  //      "DEMOISLANDMAP_spawnTwinklerAllegianceEast",
                       "DEMOISLANDMAP_spawnTwinklerAllegiance",
                       "DEMOISLANDMAP_spawnThunderChickenAllegianceNorth",
                       "DEMOISLANDMAP_spawnThunderChickenAllegianceSouth",
                       "DEMOISLANDMAP_thinThunderChickenAllegianceSouth",
                       "DEMOISLANDMAP_thinThunderChickenAllegianceNorth",

                       
#region Spawn wildernessSite1                
                          "spawnWildernessSite1",  "spawnWildernessSite1Allegiance1", "spawnWildernessSite1Expedition1",                                
                          "spawnPlaySiteWildernessSite1Route", 
#endregion
                       
                       "naturalTerminalSW", "naturalTerminalN", 

                  
                          "initSulfurDetected","initNestDetected","initNestDestroyed", "initBushDragonDetectedShortDelay", "initBushDragonDetectedLongDelay", "initFieldLabCannibalized",
                          "initGameOver", "initIncludeDateInBurial", "initBurialText1", "initBurialText2", "initBurialText3", "initBurialText4", "initBurialText5",

                           #region Group meetings
                           "initEnableGroupMeetings", "initTimeBeforeGroupMeeting",
                           "meetingEmigrateThreat", "meetingEmigrateThreatAllUnhappy",                        
                           "meeting3Security", "meeting3Food", "meeting3Comfort",
                           "meeting2Security", "meeting2Food", "meeting2Comfort",
                           "meetingSecurityAllUnhappy", "meetingFoodAllUnhappy", "meetingComfortAllUnhappy",
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

                          "initDeathCounter",

                         "placeExpedition", "setView", "destroySkimmerHull", "destroySkimmerEngineSide", "destroySkimmerEngineTop", "destroySkimmerTail",
                         "blackSmoke", "smallFog1", "smallFog2", "smallFog3", "sulphurousSmoke1", "sulphurousSmoke2", "haze1", "haze2",
                         "spawnPlayerAllegiance",
                         //skimmer position is no longer always close to camp center: "startSkimmerEngineTop", "startSkimmerEngineSide", "startSkimmerHull",
                      },

                     MainDifficultySettings = new[]
                     {


                        new Difficulty()
                         {
                              KeyName = "veryEasy", 
                              Name = "Very easy"
                            
                          /*       OptionsToUse = new SerializableDictionary<string, string[]>()
                                {
                                    { "startingLocations", new []{ "southCoast" } },
                                    { "people", new []{ "4people" } },
                                    { "equipment", new []{ "lotsOfWeaponsSomeGear" } },
                                    { "fauna" , new []{ "benign" }},
                                    { "resources" , new []{ "plenty" }},
                                    { "gameDuration" , new []{ "medium" }}
                                }*/
                         },

 
                         new Difficulty()
                         {
                              KeyName = "easy",
                              Name = "Normal (Recommended start)",// (Recommended start) //renamed
                              IsDefault = true,
                                Description = "On NORMAL difficulty, the explorers have crashed in a relatively safe corner of the island. (The start location is randomized). \nThey have enough weapons and equipment to see them through for a while but will have to improvise while waiting for rescue.",
                                OptionsToUse = new SerializableDictionary<string, string[]>()
                                {
                                    { "startingLocations", new []{"southCoast", "northCoast" } },
                                    { "people", new []{ "4people" } },
                                    { "equipment", new []{ "oneSentrySomeGear" } }, //mp, I want a reason to build extinguisher (and bows) so only one rifle. not: "lotsOfWeaponsSomeGear"
                                    { "fauna" , new []{ "average" }}, //MP feb 2016 was: "benign"
                                    { "resources" , new []{ "plenty" }},
                                    { "gameDuration" , new []{ "medium" }}
                                }
                         },
                          new Difficulty()
                         {
                              KeyName = "normal", // used in achievements. If renaming, rename it in WinScenario also.
                              Name = "Hard",//renamed
                              Description = "On HARD difficulty, the explorers crashed on top of a nest of dangerous predators and were forced to abandon an injured crew member and their gear. \nThey now prepare to go back, rescue him and if possible reclaim their equipment.",
                               
                                OptionsToUse = new SerializableDictionary<string, string[]>()
                                {
                                    { "startingLocations", new []{  "fledToSouthWestWreckAtSandstone", "fledToSouthWestWreckAtBramble", "fledToSouthEastWreckAtBramble" } }, 
                                    { "people", new []{ "4people" } }, 
                                    { "equipment", new []{ "oneShotgunSomeGear" } } ,// TODO: validate that the value array keys are valid and exist in Options
                                    { "fauna" , new []{ "fierce" }}, //feb 2016 was "average"
                                    { "resources" , new []{ "average" }},
                                    { "gameDuration" , new []{ "medium" }} //test
                                }
                         },
                          new Difficulty()
                         {
                              KeyName = "hard",
                              Name = "Hard",
                     /*           OptionsToUse = new SerializableDictionary<string, string[]>()
                                {
                                    { "startingLocations", new []{ "southCoast", "northCoast" } },
                                    { "people", new []{ "3people" } }, 
                                    { "equipment", new []{ "fewSupplies" } } ,
                                    { "fauna" , new []{ "fierce" }},
                                    { "resources" , new []{ "average" }},
                                    { "gameDuration" , new []{ "medium" }} 
                                }   */                         
                         },
                        new Difficulty()
                         {
                              KeyName = "veryHard",
                              Name = "Very hard" , //don't want it in main difficulty list (but want the very hard options in custom), so i commented this out:
                             /*   OptionsToUse = new SerializableDictionary<string, string[]>()
                                {
                                    { "startingLocations", new []{ "southCoast", "northCoast" } },
                                    { "people", new []{ "2people" } }, 
                                    { "equipment", new []{ "fewSupplies" } },
                                    { "fauna" , new []{ "fierce" }},
                                    { "resources" , new []{ "sparse" }},
                                    { "gameDuration" , new []{ "medium" }}   
                                }  */                          
                         },
                          new Difficulty()
                         {
                              KeyName = "impossible",
                              Name = "Impossible"                             
                         }

                     },

                    
                  
                    
                    OptionSets = new[]
                     {
                          new OptionSet(){
                              KeyName = "startingLocations",
                               Name = "Camp location",
                               DisplayGroup = 0,
                                Options = new[] { 
                                    new Option()
                                    {
                                      KeyName = "southCoast", // wreck and camp at south west.
                                      Name = "Grassland shore SW",
                                     
                                      LoadingDialogText = " \nJOURNAL RECORDED BY: Ward Conlan \nWe have no way of contacting the other mission members as long as our satellite transmitter is defective. Until connection is back we will keep a locally stored journal. This is the first entry. \n \nWe have escaped the catastrophic attack by quadites that happened just hours ago. Only minor injuries are reported. Our vehicle however, is non-functional after damage sustained in the attack and a subsequent crash-landing. \n  \nWe've landed in a firegrass biome. Geographical data are incomplete, and this biome has only been partially documented during the previous months of research. Still, we know enough about this environment to expect species of quadites. \n \nWe only hope not to see the vicious swarmer quadite that attacked us earlier today.", 
                                      LoadingDialogTextMode = Option.LoadingDialogTextModes.Append,
                                      LoadingDialogTextOrder = 2,

                                      Difficulty = new CustomDifficulty()
                                      {
                                           KeyName = "easy",
                                           Name = "Next to wreck", //"Coastline"
                                           //Name = "Normal",    
                                           ScoreModifier = 1f
                                      },
                                       ConditionalEvents = new[]{"DEMOISLANDMAP_beginningBushDragonPopulationEast", "DEMOISLANDMAP_triggerMarsh", "DEMOISLANDMAP_triggerNorthEastCloseRockCrevice", "DEMOISLANDMAP_triggerNorthRockCrevice", "DEMOISLANDMAP_checkQuaditeCarcassDetected", "DEMOISLANDMAP_checkQuaditeCarcassDetectedDelay", "DEMOISLANDMAP_checkNestDetectedDelay", "DEMOISLANDMAP_checkNestExpositionEventHasFired", "DEMOISLANDMAP_checkNestExpositionEventHasFiredAndSulfurSeen", },
                                       ActionKeys = new[]{ "startSkimmerHullSouth", "startSkimmerEngineTopSouth", "startSkimmerEngineSideSouth", "startSkimmerTailSouth", "placeNestSandstone", 
                                           "placeNestRockEast" , "setStartingLocationSouth", "exploreShroudSouth", "initNoSandstoneWreckage", "initNotFledAtStart",
                
                                           "startFieldLabInSkimmer", "startBasicFireExtinguisherInSkimmer", "startEmptyCartridgeInSkimmer", "startEmptyCartridgeInSkimmer", "startEmptyCartridgeInSkimmer" }
                                      
                                    },
                                    new Option() // 
                                    {
                                      KeyName = "northCoast", //wreck lies at camp, to the north. tail lies to the west at sandstone. has crates lying at sandstone crevice.
                                      Name = "Rocky river bank NE",

                                      LoadingDialogText = " \nJOURNAL RECORDED BY: Ward Conlan \nWe have no way of contacting the other mission members as long as our satellite transmitter is defective. Until connection is back we will keep a locally stored journal. This is the first entry. \n \nWe have escaped the catastrophic attack by quadites that happened just hours ago. Only minor injuries are reported. Our vehicle however, is non-functional after damage sustained in the attack and a subsequent crash-landing. \n  \nWe've landed in a firegrass biome. Geographical data are incomplete, and this biome has only been partially documented during the previous months of research. Still, we know enough about this environment to expect species of quadites. \n \nWe only hope not to see the vicious swarmer quadite that attacked us earlier today.", 
                                      LoadingDialogTextMode = Option.LoadingDialogTextModes.Append,
                                      LoadingDialogTextOrder = 2,
                                      Difficulty = new CustomDifficulty()
                                      {
                                           KeyName = "easy",
                                           Name = "Next to wreck",
                                           //Name = "Normal",   
                                           ScoreModifier = 1f
                                      },
                                      ConditionalEvents = new[]{"DEMOISLANDMAP_beginningBushDragonPopulationWest", "DEMOISLANDMAP_triggerMarsh", "DEMOISLANDMAP_triggerNorthEastCloseRockCrevice", "DEMOISLANDMAP_triggerNorthRockCrevice", "DEMOISLANDMAP_checkQuaditeCarcassDetected", "DEMOISLANDMAP_checkQuaditeCarcassDetectedDelay", "DEMOISLANDMAP_checkNestDetectedDelay", "DEMOISLANDMAP_checkNestExpositionEventHasFired", "DEMOISLANDMAP_checkNestExpositionEventHasFiredAndSulfurSeen", }, // "DEMOISLANDMAP_triggerSandstoneCanyon"
                                      ActionKeys = new[]{"startSkimmerHullNorth", "startSkimmerEngineTopNorth", "startSkimmerEngineSideNorth", "startSkimmerTailSandstone", "placeCratesSandstone", "placeNestSandstone", "placeNestRockEast" , "setStartingLocationNorth",
                                          "placeCratesSandstone", "exploreShroudNorth", "initSandstoneWreckage", "initNotFledAtStart",
                
                                         "startFieldLabInSkimmer", "startBasicFireExtinguisherInSkimmer", "startEmptyCartridgeInSkimmer", "startEmptyCartridgeInSkimmer", "startEmptyCartridgeInSkimmer"}
                                      
                                    },



                                    //the below have exposition and talk about an unconscious team member. either at rocks or sandstone, but talk should be the same. they also don't talk about the empty crevices because those caves are housing nests.

                                   new Option()
                                    {
                                      KeyName = "fledToSouthWestWreckAtSandstone", //wreck lies on sandstone to the west, they have fled south west
                                      Name = "Wreck at north", //"Fled SW, wreck at sandstone"
                                     
                                      LoadingDialogText = " \n//PPU AUTO-GENERATED REPORT: \n \n00.00: Aircraft crash-landed - Severe damage prevents flight and communication. \nOne passenger lost during crash and isolated from team in inaccessible terrain - Alive but unconscious. \n \n01.20: Dangerous species (Twinkler quadites) detected in immediate vicinity. \n02.00: Team members attacked by numerous Twinklers. \n09.50: Camp relocated 1.7 km south of crash site.", 
                                      LoadingDialogTextMode = Option.LoadingDialogTextModes.Append,
                                      LoadingDialogTextOrder = 2,
                                      Difficulty = new CustomDifficulty()
                                      {
                                           KeyName = "normal",
                                           Name = "Fled from wreck", //"Coastline"
                                           //Name = "Normal",    
                                           ScoreModifier = 1f
                                      },
                                       ConditionalEvents = new[]{  "DEMOISLANDMAP_beginningTwinklerPopulationSandstone", "DEMOISLANDMAP_beginningBushDragonPopulationEast", "DEMOISLANDMAP_dialogueRescueCountdown", "DEMOISLANDMAP_unconsciousDies", "DEMOISLANDMAP_checkCasualtyRescued" },
                                       ActionKeys = new[]{"startSkimmerTailSandstone", "startSkimmerHullSandstone", "startSkimmerEngineTopSandstone", "startSkimmerEngineSideSandstone",
                                           //put some goodies next to crash site:
                                           "startFieldLabSandstone", "startRationSandstone", "startBasicFireExtinguisherSandstone", "startEmptyCartridgeSandstone", "startEmptyCartridgeSandstone", "startEmptyCartridgeSandstone", "startSentrySandstone", "startSentryWeaponMountSandstone", "startShotgunAmmoSandstone", "startShotgunAmmoSandstone",

                                           "placeNestSandstone", "placeNestRockEast" , "setStartingLocationSouth", "exploreShroudFledSouthWestWreckAtSandstone", "exploreShroudFledSouthWestWreckAtSandstoneFlightPath", "placeUnconsciousAtSandstone", "setRescueSpawnLocationSandstone",
                                           "initUnconsciousAndAlive","initSandstoneWreckage", "initFledAtStart", "initCasualtyRescued"
                                                              }  // what is this: "initSandstoneWreckage" it determines dialogue when detecting the tail
                                
                                    },


                                 new Option()
                                    {
                                      KeyName = "fledToSouthWestWreckAtBramble", //wreck lies close to gorge and iron bramble, they have fled south west
                                      Name = "Wreck at northeast", //"Fled SW, wreck at bramble"
                                     
                             
                                      LoadingDialogText = " \n//PPU AUTO-GENERATED REPORT: \n \n00.00: Aircraft crash-landed - Severe damage prevents flight and communication. \nOne passenger lost during crash and isolated from team in inaccessible terrain - Alive but unconscious. \n \n01.20: Dangerous species (Twinkler quadites) detected in immediate vicinity. \n02.00: Team members attacked by numerous Twinklers. \n09.50: Camp relocated 1.8 km southwest of crash site.", 
                                      LoadingDialogTextMode = Option.LoadingDialogTextModes.Append,
                                      LoadingDialogTextOrder = 2,
                                      Difficulty = new CustomDifficulty()
                                      {
                                           KeyName = "normal",
                                           Name = "Fled from wreck",
                                           //Name = "Normal",    
                                           ScoreModifier = 1f
                                      },
                                       ConditionalEvents = new[]{ "DEMOISLANDMAP_beginningTwinklerPopulationBramble", "DEMOISLANDMAP_beginningBushDragonPopulationEast", "DEMOISLANDMAP_dialogueRescueCountdown", "DEMOISLANDMAP_unconsciousDies" , "DEMOISLANDMAP_checkCasualtyRescued"},
                                       ActionKeys = new[]{ "startSkimmerHullBramble", "startSkimmerEngineTopBramble", "startSkimmerEngineSideBramble", "startSkimmerTailBramble", 
                                           //put some goodies next to crash site:
                                           "startFieldLabBrambleEast", "startRationBrambleEast", "startBasicFireExtinguisherBrambleEast","startEmptyCartridgeBrambleEast","startEmptyCartridgeBrambleEast","startEmptyCartridgeBrambleEast", "startSnipsBrambleEast","startSentryBrambleEast", "startSentryWeaponMountBrambleEast", "startShotgunAmmoBrambleEast", "startShotgunAmmoBrambleEast", "startMacheteBrambleWest", 

                                           "placeNestSandstone", "placeNestRockEast" ,"placeNestRockSouth", "placeNestRockNorth", "setStartingLocationSouth", "exploreShroudFledSouthWestWreckAtBramble", "exploreShroudWreckAtBrambleFlightPath",
                                           "placeUnconsciousAtBramble", "setRescueSpawnLocationBramble",
                                           "initUnconsciousAndAlive", "initSandstoneWreckage", "initFledAtStart", "initCasualtyRescued" }  // what is this: "initSandstoneWreckage" it determines dialogue when detecting the tail
                                                                     
                                    },


                                 new Option()
                                    {
                                      KeyName = "fledToSouthEastWreckAtBramble", //wreck lies close to gorge and iron bramble, they have fled south east
                                      Name = "Wreck at northwest", //"Fled SE, wreck at bramble"
                                     
                                   //   LoadingDialogText = "we have fled", LoadingDialogTextMode = Option.LoadingDialogTextModes.Append, LoadingDialogTextOrder = 5,

                                       LoadingDialogText = " \n//PPU AUTO-GENERATED REPORT: \n \n00.00: Aircraft crash-landed - Severe damage prevents flight and communication. \nOne passenger lost during crash and isolated from team in inaccessible terrain -  Alive but unconscious. \n \n01.20: Dangerous species (Twinkler quadites) detected in immediate vicinity. \n02.00: Team members attacked by numerous Twinklers. \n09.50: Camp relocated 1.4 km southeast of crash site.", 
                                      LoadingDialogTextMode = Option.LoadingDialogTextModes.Append,
                                      LoadingDialogTextOrder = 2,
                                      Difficulty = new CustomDifficulty()
                                      {
                                           KeyName = "normal",
                                           Name = "Fled from wreck",
                                           //Name = "Normal",    
                                           ScoreModifier = 1f
                                      },
                                       ConditionalEvents = new[]{ "DEMOISLANDMAP_beginningTwinklerPopulationBramble", "DEMOISLANDMAP_beginningBushDragonPopulationWest", "DEMOISLANDMAP_dialogueRescueCountdown", "DEMOISLANDMAP_unconsciousDies", "DEMOISLANDMAP_checkCasualtyRescued"  },
                                       ActionKeys = new[]{"startSkimmerHullBramble", "startSkimmerEngineTopBramble", "startSkimmerEngineSideBramble", "startSkimmerTailBramble",
                                           //put some goodies next to crash site:
                                           "startFieldLabBrambleEast", "startRationBrambleEast", "startBasicFireExtinguisherBrambleEast","startEmptyCartridgeBrambleEast","startEmptyCartridgeBrambleEast","startEmptyCartridgeBrambleEast","startSnipsBrambleEast","startSentryBrambleEast", "startSentryWeaponMountBrambleEast", "startShotgunAmmoBrambleEast", "startShotgunAmmoBrambleEast", "startMacheteBrambleWest",    
                                           "placeNestSandstone", "placeNestRockEast" ,
                                           "setStartingLocationSouthEast", "exploreShroudFledSouthEastWreckAtBramble", "exploreShroudWreckAtBrambleFlightPath",  "placeUnconsciousAtBramble", "setRescueSpawnLocationBramble", 
                                           "initUnconsciousAndAlive", "initSandstoneWreckage", "initFledAtStart", "initCasualtyRescued" }  // what is this: "initSandstoneWreckage" it determines dialogue when detecting the tail
                                                                      
                                    },

                                }
                          },
                          new OptionSet(){
                               KeyName = "people",
                               Name = "Camp members",
                                DisplayGroup = 1,
                                Options = new[] { 
                                    new Option()
                                    {
                                      KeyName = "4people",
                                      Name = "4 people",
                                      ShortDescription = "Group of 4 people",

                                     // LoadingDialogText = " \nWe are 4 people", LoadingDialogTextMode = Option.LoadingDialogTextModes.Append, LoadingDialogTextOrder = 1,

                                      LoadingDialogText = "//CAMP MEMBERS: Conlan, Lehner, Yeboah, Khan", //Ward Conlan, Joaquin Lehner, Augustine Yeboah and Ilya Khan
                                      LoadingDialogTextMode = Option.LoadingDialogTextModes.Append,
                                      LoadingDialogTextOrder = 1,
                                      Difficulty = new CustomDifficulty()
                                      {
                                           KeyName = "normal",
                                            IsDefault = true,
                                           //Name = "Normal",
                                           Name = "4 people",
                                           ScoreModifier = 1f
                                      },
                                      ConditionalEvents = new[]{"DEMOISLANDMAP_introDialogue3People", "DEMOISLANDMAP_introDialogue3PeopleFled", "DEMOISLANDMAP_onlyThreeMembersLeftDialogue", "DEMOISLANDMAP_onlyTwoMembersLeftDialogue", "DEMOISLANDMAP_onlyOneMemberLeftDialogue",},
                                      ActionKeys = new[]{ "spawnConlan", "spawnLehner", "spawnKahn", "spawnYeboah"  }                                     
                                    },

                                    new Option()
                                    {
                                      KeyName = "3people",
                                      Name = "3 people",
                                      ShortDescription = "Group of 3 people",


                                      LoadingDialogText = "//CAMP MEMBERS: Conlan, Lehner, Yeboah", 
                                      LoadingDialogTextMode = Option.LoadingDialogTextModes.Append,
                                      LoadingDialogTextOrder = 1,

                                      Difficulty = new CustomDifficulty()
                                      {
                                           KeyName = "hard",
                                           //Name = "Normal",
                                           Name = "3 people",
                                           ScoreModifier = 1.5f
                                      },
                                      ConditionalEvents = new[]{"DEMOISLANDMAP_introDialogue3People", "DEMOISLANDMAP_introDialogue3PeopleFled", "DEMOISLANDMAP_onlyTwoMembersLeftDialogue", "DEMOISLANDMAP_onlyOneMemberLeftDialogue",},
                                      ActionKeys = new[]{ "spawnConlan", "spawnLehner", "spawnYeboah"  }                                     
                                    },

                                    new Option()
                                    {
                                      KeyName = "2people",
                                      Name = "2 people",
                                      ShortDescription = "Group of 2 people",

                                      LoadingDialogText = "//CAMP MEMBERS: Conlan, Lehner", 
                                      LoadingDialogTextMode = Option.LoadingDialogTextModes.Append,
                                      LoadingDialogTextOrder = 1,

                                      Difficulty = new CustomDifficulty()
                                      {
                                           KeyName = "veryHard",
                                           Name = "2 people",
                                           ScoreModifier = 2f
                                      },
                                      ConditionalEvents = new[]{"DEMOISLANDMAP_introDialogue2People", "DEMOISLANDMAP_introDialogue2PeopleFled", "DEMOISLANDMAP_onlyOneMemberLeftDialogue",},
                                       ActionKeys = new[]{ "spawnConlan", "spawnLehner" }
                                    },


                                }
                          },                          
                          new OptionSet(){
                               KeyName = "equipment",
                               Name = "Supplies",
                                DisplayGroup = 0,
                                Options = new[] { 


                                    new Option()
                                    {
                                      KeyName = "lotsOfWeaponsSomeGear",
                                      Name = "Several sentries and firearms",
                                      Difficulty = new CustomDifficulty()
                                      {
                                           KeyName = "veryEasy",
                                           Name = "Many firearms",
                                           ScoreModifier = 0.3f
                                      },
                                      ConditionalEvents = new[]{"DEMOISLANDMAP_introDialogueLotsOfSupplies", "DEMOISLANDMAP_introSuggestionLotsOfSupplies"}, //change to dialogue about sentry??
                                       ActionKeys = new[]
                                       {
                                             "startKnife", "startKnife", "startKnife", "startSnips", "startString", "startString", "startThermalTarp", 
                                               "startHuntingRifle", "startSentry", "startSentry", "startShotgun", "startSentryGunAmmo", "startSentryGunAmmo", "startRifleAmmo",  "startRifleAmmo",  "startShotgunAmmo",
                                                "startMachete", "startSensor",  "startRation", "startRation", "startGoggles"
               
                                       }                                     
                                    },
                                    new Option()
                                    {
                                      KeyName = "oneSentrySomeGear", // mp was: "oneRifleSomeGear"
                                      Name = "Sentry + rifle", //must be very short to fit in combobox
                                      Difficulty = new CustomDifficulty()
                                      {
                                           KeyName = "normal",
                                           Name = "Some gear",
                                           IsDefault = true,
                                           ScoreModifier = 1f
                                      },
                                     ConditionalEvents = new[]{"DEMOISLANDMAP_introDialogueSomeSupplies", "DEMOISLANDMAP_introSuggestionSomeSupplies"}, //dialogue about sentry
                                       ActionKeys = new[]
                                       {
                                          
                                            "startKnife", "startKnife", "startKnife", "startSnips", "startString", "startString", "startThermalTarp", 
                                               "startHuntingRifle", "startRifleAmmo",  "startMachete", "startSentry" , "startSentryGunAmmo", "startGoggles" 
               
                                       }                                     
                                    },                                  
                                  
                       
                             new Option()
                                    {
                                      KeyName = "oneShotgunSomeGear", //
                                      Name = "Shotgun + some gear",
                                      Difficulty = new CustomDifficulty()
                                      {
                                           KeyName = "normal",
                                           Name = "Some gear",                                           
                                           ScoreModifier = 1f
                                      },
                                     ConditionalEvents = new[]{"DEMOISLANDMAP_introDialogueSomeSupplies", "DEMOISLANDMAP_introSuggestionSomeSupplies"}, //Note, when selecting the Hard (Fled) difficulty setting, or the 'Fled from wreck' start location in CUSTOM, this dialogue gets overwritten by talk about Laurent: "DEMOISLANDMAP_introDialogue3PeopleFled". 
                                       ActionKeys = new[]
                                       {
                                          
                                            "startKnife", "startKnife", "startKnife", "startSnips", "startString", "startString", "startThermalTarp", 
                                               "startShotgun", "startShotgunAmmo",  "startMachete", "startSensor",
               
                                       }                                     
                                    },  
                       
                                    new Option()
                                    {
                                      KeyName = "fewSupplies",
                                      Name = "Little equipment",
                                      Difficulty = new CustomDifficulty()
                                      {
                                           KeyName = "hard",
                                           Name = "Little equipment",
                                           ScoreModifier = 1.5f
                                      },
                                     ConditionalEvents = new[]{"DEMOISLANDMAP_introDialogueFewSupplies", "DEMOISLANDMAP_introSuggestionFewSupplies"},
                                       ActionKeys = new[]
                                       {
                                            "startKnife", "startKnife", 
                                               
               
                                       }                                     
                                    },  

                                }
                          },
                          new OptionSet(){
                              KeyName = "fauna",
                               Name = "Fauna",
                               DisplayGroup = 2,
                                Options = new[] { 
                                    new Option()
                                    {
                                      KeyName = "benign",
                                      Name = "Benign",
                                     
                                      Difficulty = new CustomDifficulty()
                                      {
                                           KeyName = "easy",
                                           Name = "Benign",
                                           //Name = "Normal",
                                           ScoreModifier = 1f
                                      },
                                       ConditionalEvents = new[]{"DEMOISLANDMAP_continualSpawnThinThunderChickensSouth","DEMOISLANDMAP_continualSpawnThinThunderChickensNorth","DEMOISLANDMAP_timedSpawnBeginningPopulationEasy", "DEMOISLANDMAP_continualHunterSpawnSouthSandstoneCave", "DEMOISLANDMAP_continualHunterSpawnEastRockCave", "DEMOISLANDMAP_triggerHunterSouthSandstoneCave", "DEMOISLANDMAP_triggerHunterRockCaveEast"},
                                       ActionKeys = new[]{"setMaxThinThunderChickenLow","setThinThunderChickenSpawnIntervalOften","setThunderChickenSpawnIntervalHigh","setMaxTwinklersLow", "setMaxThunderChickensHigh", "setMaxBinalRatsLow", "setBinalRatSpawnSeldom",  "setTwinklerSpawnIntervalSouthSeldom", "setTwinklerSpawnIntervalEastSeldom" }
                                      
                                    },
                                    new Option()
                                    {
                                      KeyName = "average",
                                      Name = "Average",
                                     
                                      Difficulty = new CustomDifficulty()
                                      {
                                           KeyName = "normal",
                                           Name = "Average",
                                           IsDefault = true,
                                           //Name = "Normal",
                                           ScoreModifier = 1f
                                      },
                                       ConditionalEvents = new[]{"DEMOISLANDMAP_continualSpawnThinThunderChickensSouth","DEMOISLANDMAP_continualSpawnThinThunderChickensNorth","DEMOISLANDMAP_timedSpawnBeginningPopulationNormal", "DEMOISLANDMAP_continualSpawnTwinklerSouthSandstoneCave","DEMOISLANDMAP_continualSpawnTwinklerSouthRockCave",  "DEMOISLANDMAP_continualSpawnTwinklerEastRockCave", "DEMOISLANDMAP_triggerGuardsSouthSandstoneCave", "DEMOISLANDMAP_triggerGuardsRockCaveEast"},
                                       ActionKeys = new[]{"setMaxThinThunderChickenNormal","setThinThunderChickenSpawnIntervalOften","setThunderChickenSpawnIntervalNormal","setMaxTwinklersNormal", "setMaxThunderChickensNormal", "setMaxBinalRatsNormal", "setBinalRatSpawnOften", "setTwinklerSpawnIntervalSouthSeldom", "setTwinklerSpawnIntervalEastOften" }
                                      
                                    },
                                    new Option() 
                                    {
                                      KeyName = "fierce",
                                      Name = "Fierce",
                                      Difficulty = new CustomDifficulty()
                                      {
                                           KeyName = "hard",
                                           Name = "Fierce",
                                           ScoreModifier = 1f
                                      },
                                      ConditionalEvents = new[]{"DEMOISLANDMAP_continualSpawnThinThunderChickensSouth","DEMOISLANDMAP_continualSpawnThinThunderChickensNorth","DEMOISLANDMAP_timedSpawnBeginningPopulationNormal", "DEMOISLANDMAP_continualSpawnTwinklerSouthSandstoneCave", "DEMOISLANDMAP_continualSpawnTwinklerSouthRockCave", "DEMOISLANDMAP_continualSpawnTwinklerEastRockCave", "DEMOISLANDMAP_triggerGuardsSouthSandstoneCave", "DEMOISLANDMAP_triggerGuardsRockCaveEast"},
                                      ActionKeys = new[]{"setMaxThinThunderChickenHigh","setThinThunderChickenSpawnIntervalOften","setThunderChickenSpawnIntervalLow","setMaxTwinklersHigh", "setMaxThunderChickensNormal", "setMaxBinalRatsHigh", "setBinalRatSpawnOften","setTwinklerSpawnIntervalSouthOften", "setTwinklerSpawnIntervalEastOften" }  
                                      
                                    }
                                }
                               
                          },
                         
                          
                            new OptionSet(){
                              KeyName = "resources",
                               Name = "Resources",
                               DisplayGroup = 2,
                                Options = new[] { 
                                    new Option()
                                    {
                                      KeyName = "plenty",
                                      Name = "Plenty",
                                     
                                      Difficulty = new CustomDifficulty()
                                      {
                                           KeyName = "easy",
                                           Name = "Plenty",
                                           ScoreModifier = 1f
                                      },
                                       ActionKeys = new[]{ "setFishSchoolMedium", "setPlentyResources", "startFarmSpotSmall1", "startFishTrapCreek1", "startFishTrapCoast1", "startFishTrapCoast2", "startFishTrapShore1", "startFishTrapShore2", "startFishTrapShore3" } //mp may 2015 - was "setPlentyResources".now using reduced number of resources because  people's calorie decrease has been adjusted so they don't need so mouch food on the map.  add noise with added weight // "setPlentyResources"
                                      
                                    },

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
                                       ActionKeys = new[]{ "setFishSchoolSparse", "setSparseResources", "startFarmSpotSmall1", "startFishTrapCreek1", "startFishTrapCoast1", "startFishTrapCoast2", "startFishTrapShore1"}  //mp may 2015 - was "setFishSchoolMedium", "setNormalResources",.. now using reduced number of resources because  people's calorie decrease has been adjusted so they don't need so mouch food on the map. // add noise with neutral weight                                   
                                    },                                   
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
                                      ActionKeys = new[]{ "setFishSchoolSparse", "setSparseResources", "startFarmSpotSmall1", "startFishTrapCoast1", "startFishTrapCoast2",} // add noise with decreased weight
                                    }
                                }
                            },


                            new OptionSet(){
                              KeyName = "gameDuration", // time before rescue arrives.
                               Name = "Duration",
                               DisplayGroup = 3,
                                Options = new[] { 
                                    new Option()
                                    {
                                      KeyName = "short",
                                      Name = "Short",
                                     
                                      Difficulty = new CustomDifficulty()
                                      {
                                           KeyName = "easy",
                                           Name = "Short",
                                           ScoreModifier = 1f
                                      },
                                       ActionKeys = new[]{ "setWinGameEarly"  } // todo
                                      
                                    },

                                    new Option()
                                    {
                                      KeyName = "medium",
                                      Name = "Medium",
                                     
                                      Difficulty = new CustomDifficulty()
                                      {
                                           KeyName = "normal",
                                           Name = "Medium",
                                           IsDefault = true,
                                           ScoreModifier = 1f
                                      },
                                       ActionKeys = new[]{ "setWinGameMedium" }   //                                   
                                    },                                   
                                    new Option()
                                    {
                                      KeyName = "long",
                                      Name = "Long",
                                     
                                      Difficulty = new CustomDifficulty()
                                      {
                                           KeyName = "hard",
                                           Name = "Long",
                                           //Name = "Normal",
                                           ScoreModifier = 1f
                                      },                                     
                                      ActionKeys = new[]{ "setWinGameLate" } // 
                                    }
                                }
                            }


                           
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
