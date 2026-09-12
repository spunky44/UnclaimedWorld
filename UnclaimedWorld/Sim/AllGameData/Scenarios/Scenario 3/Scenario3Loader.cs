using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Scenarios;
using UWGame.SimSide.InGameEvents.Actions;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.AllGameData.Scenarios.Scenario_3.Data;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_3 // I chose a generic name so scenario name changes don't affect us so much...
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
    public class Scenario3Loader : ScenarioLoader // I chose a generic name so scenario name changes don't affect us so much...
    {
        //private const string folderName = ;


        public override string FolderName
        {
            get { return "TUTORIAL - Castaways"; }
        }

        protected override Scenario InitScenarioHeader()
        {
            Scenario scenario;
            
                scenario = new Scenario()
                {                     

                    Name = FolderName, 
                    DisplayName = "Castaways",
                    TimeDateYear = new DateAndTime.TimeDateYear(){ Year = 120, Day = 1, TimeOfDay = 0.31 },
                    MapKey = "i Tutorial Island",
                    MapSize = SimSide.Scenarios.MapSize.Small,
                    SummaryDescription = "TUTORIAL 1: In a primitive future, a crew of sailors are cast ashore on an island and must find food and signal for help. \n \nERA: The Great Descent",
                    Description = "TUTORIAL NO. 1 \nPlaythrough time 45-90 min \n \nCenturies after planetfall, the human colony on Antheia has regressed and lost most of the knowledge and technology the pioneers brought to the planet. Still, some artifacts remain - among them, the PPU: An instrument used by the pioneers for analyzing the alien environment and planning a frontier colony. \nNow, three castaways must learn to use the PPU to survive on an uncharted island.", // 
                    Allow32Bit = true,

                    // Scenarios\Scenario 1\Scenario Screen\aircraftWreck_scenario_thumb.png
                    ThumbnailImage = "Scenarios/Scenario 3/Scenario Screen/catamaranWreck_scenario_thumb",
                    Image = "BoatStorm",
                    IsInDevelopment = false,
                    SortOrder = 0
                    
                };              
            

            return scenario;
        }

        public override DataLoader GetDataLoader()
        {
            DataLoader dataLoader = new Scenario3DataLoader();
            dataLoader.FolderName = FolderName;

            return dataLoader;
        }


        

        protected override ScenarioData InitScenarioData()
        {
            //List<PersonalityType> list = new List<PersonalityType>();

            ScenarioData scenarioData = null;

                     
                scenarioData = new ScenarioData()
                {
                    LoadingBackgroundImage = "Scenarios/Scenario 3/Scenario Screen/TitleImgBoat_1920px",

                    LoadingDialogText = "Our people are descendants of the pioneers who came to Antheia two centuries ago. They were great minds, highly skilled in every way. Even though we lost their might in the Great Descent, we still practice the customs of the pioneers. We respect our ancestors - they were not to blame for the tragedy that happened. We try to follow their example and make the best use of their technology that remains. But we are a simple people - farmers, hunters and sailors - and have to make our own way of life on planet Antheia.", //\nRECORDED BY: Ward Conlan. ALSO PRESENT: Joaquin Lehner, Augustine Yeboah and Ilya Khan \n \nWe have no way of contacting the other mission members as long as our satellite transmitter is defective. Until connection is back we will keep a locally stored journal. This is the first entry. \n  \nWe have escaped the catastrophic attack by quadites that happened app. 2 hours ago. Only minor injuries are reported. Our vehicle however, is non-functional after damage sustained in the attack and a subsequent crash-landing. \n  \nWe've landed in a firegrass biome. Geographical data are incomplete, and this biome has only been partially documented during the previous months of research. Still, we know enough about this environment to expect species of quadites. \n \nWe only hope not to see the vicious swarmer quadite that attacked us earlier today.",
                    LoadingDialogImage = "NightTime", // I put image in the crt content folder in GUI as per the tooltip. NOT in the scenario folders. because not sure it works.

                    SpawnWorldAction = "spawnWorld",
                    SpawnSiteAction = "spawnPlaySite",

                    EnableMissions = false,

                    EnableGraphs = false,
                    EnableContacts = false,
                    EnablePersonell = false,
                    EnableWorldMap = false,
                    EnablePolicy = false,
                    EnableLedger = false,

                    //global events to register regardless of customization/difficulty:
                    ConditionalEvents = new[] {                     
                     "TUTORIAL_musicTrackList",

                        "TUTORIAL_placeCatamaran",
                         // should be an action instead  ....MP: not sure what this means. how? need something to copy paste from 
                                                    //LARS: it means it should be performed unconditionally, so remove the condition and create an action below instead. they have to be put in the doc Eventactionloader.cs  see "placeCrevice"
                        "TUTORIAL_placeTerritoryLock1",  // copy+pasted from placeCrevice..
                        "TUTORIAL_placeTerritoryLock2",
                        "TUTORIAL_introDialogue",
                        "TUTORIAL_scoutDecision",
                        "TUTORIAL_timedSpawnBeginningPopulation",
                        "TUTORIAL_triggerBeforeCrevice",
                   //     "TUTORIAL_triggerPastBridge",
                        "TUTORIAL_3commonOilTubers",
                        "TUTORIAL_checkcommonOilTubersGatheredDelay",
                        "TUTORIAL_ReadyToBuildCampfire",
                        "TUTORIAL_ReadyToCookcommonOilTubers",
                        "TUTORIAL_3Flint3WaterCaneStems",
                        //"TUTORIAL_3Spearheads3WaterCaneStems",
                        "TUTORIAL_3SpearsFinished",
                        "TUTORIAL_triggerBeforeBushDragonFight",
                        "TUTORIAL_triggerAfterBushDragonFight",
                    //    "TUTORIAL_Rescue_1Left",
                   //     "TUTORIAL_Rescue_2Left",
                        "TUTORIAL_Rescue_3Left"

                       },

                    // these are actions that are always performed regardless of difficulty - listed in the doc EventActionLoader.cs :
                    Actions = new string[]
                      {
                        //  "stopMusic", // uncomment this if we change our mind about playing music again
                          "spawnBushDragonTutAllegianceNorth", "spawnBushDragonTutAllegianceSouth",
                          "setStartingLocationSouth",
                           "exploreShroudSouth",
                          "spawnHarron", "spawnSantilla", "spawnScoyd",
                          "placeCrevice",
                          //Whenever a propertykey with boolean (true/false) is used, it needs to be set here first:
                         "initGorgeDetected", "initCampfireFinished", "initspearReadyToBeMade", "initFlintSpearMade", "initSignalPyreLit",  "initGameOver", "initIncludeDateInBurial", "initBurialText1", "initBurialText2", "initBurialText3", "initBurialText4", "initBurialText5",  

                         "placeExpedition", "setView",  

                         "startKnife", "startCookingPot",

//these lines are for testing purposes:----------
                 //       "startImprovisedFlintSpear","startImprovisedFlintSpear","startImprovisedFlintSpear", "startcommonOilTubers","startFirewood","startFirewood", "startFirewood",
                    //     "startLines" , "startSpoakLeaves","startSpoakLeaves","startSpoakLeaves", "startFirewood","startFirewood","startFirewood", "startSignalPyre", "startcommonOilTubers", "startMetalWire","startMashedcommonOilTubers",
                   //      "setStartingLocationNorth", //if using, comment out "setStartingLocationSouth" above
//-----------------------------------------------


                         //"setMaxTwinklersNormal", "setMaxThunderChickensNormal", "setMaxBinalRatsNormal", "setBinalRatSpawnOften", "setTwinklerSpawnIntervalSouthSeldom", "setTwinklerSpawnIntervalEastOften",
                    //     "setFishSchoolMedium", 
                    "setSparseResources", //"setNormalResources",
                        "spawnPlayerAllegiance"

                 //replaced with terrain catamaran:
                         //    "startCatamaranItem", "startCatamaranMastItem",
                         //"startBoatWreck", "destroyBoatPart",

                      },

                     MainDifficultySettings = new[]
                     {
                          new Difficulty()
                         {
                              KeyName = "normal",
                              Name = "Normal",
                              IsDefault = true,
                                OptionsToUse = new SerializableDictionary<string, string[]>()
                                {
                             /*       { "startingLocations", new []{ "southCoast" } },
                                    { "people", new []{ "3people" } }, 
                                    { "equipment", new []{ "castawaysGear" } } ,
                                    { "fauna" , new []{ "average" }},
                                    { "resources" , new []{ "average" }} */
                                }
                         }

                     },

////////////////////    
// MP :  the below is not being used because its commented out above, in OptionsToUse. but I can't delete it then I get an error./////////////////////////////////
/////////////////////////


/////////////////////////

          ///

/////////////////////////







                    
                    OptionSets = new[]
                     {
                          new OptionSet(){
                              KeyName = "startingLocations", // MP :   not being used because its commented out above, in OptionsToUse. but I can't delete it then I get an error.///
                               Name = "Crash site",
                               DisplayGroup = 0,
                                Options = new[] { 
                                    new Option()
                                    {
                                      KeyName = "southCoast",
                                      Name = "Grassland shore SW",
                                     
                                   //   LoadingDialogText = " \nWe have landed on the coast", LoadingDialogTextMode = Option.LoadingDialogTextModes.Append, LoadingDialogTextOrder = 5,

                                      Difficulty = new CustomDifficulty()
                                      {
                                           KeyName = "normal",
                                           Name = "Coastline",
                                           //Name = "Normal",    
                                           ScoreModifier = 1f
                                      },
                         //              ConditionalEvents = new[]{"startSkimmerTailSouth" },
                          //             ActionKeys = new[]{ "setStartingLocationSouth", "exploreShroudSouth"}
                                      
                                    },
           
                                }
                               
                          },
                          new OptionSet(){
                               KeyName = "people", // MP :   not being used because its commented out above, in OptionsToUse. but I can't delete it then I get an error.///
                               Name = "Camp members",
                                DisplayGroup = 1,
                                Options = new[] { 
                                    new Option()
                                    {
                                      KeyName = "3people",
                                      Name = "3 people",
                                      ShortDescription = "Group of 3 people",

                                     // LoadingDialogText = " \nWe are 4 people", LoadingDialogTextMode = Option.LoadingDialogTextModes.Append, LoadingDialogTextOrder = 1,

                                      LoadingDialogText = "\nGROUP OF 3 PEOPLE TEXT", 
                                      LoadingDialogTextMode = Option.LoadingDialogTextModes.Append,

                                      Difficulty = new CustomDifficulty()
                                      {
                                           KeyName = "normal",
                                            IsDefault = true,
                                           //Name = "Normal",
                                           Name = "3 people",
                                           ScoreModifier = 1f
                                      },
                                      ConditionalEvents = new[]{"DEMOISLANDMAP_introDialogue3People"},
                                      ActionKeys = new[]{ "spawnConlan", "spawnLehner", "spawnKahn" }                                     
                                    },  

                                }
                          },                          
                          new OptionSet(){
                               KeyName = "equipment", // MP :   not being used because its commented out above, in OptionsToUse. but I can't delete it then I get an error.///
                               Name = "Supplies",
                                DisplayGroup = 0,
                                Options = new[] { 
                              
                                    new Option()
                                    {
                                      KeyName = "castawaysGear",
                                      Name = "Castaways' gear",
                                      Difficulty = new CustomDifficulty()
                                      {
                                           KeyName = "normal",
                                           Name = "Some gear",
                                           IsDefault = true,
                                           ScoreModifier = 1f
                                      },
                                     ConditionalEvents = new[]{"DEMOISLANDMAP_introDialogueSomeSupplies", "DEMOISLANDMAP_introSuggestionSomeSupplies"},
                                       ActionKeys = new[]
                                       {
                                          // "startSulfurSmokeBomb", // testsulfur
                                            "startKnife", "startCookingPot"
               
                                       }                                     
                                    }

                                }
                          },
                        /*  new OptionSet(){
                              KeyName = "fauna", // MP :   not being used because its commented out above, in OptionsToUse. but I can't delete it then I get an error.///
                               Name = "Fauna",
                               DisplayGroup = 2,
                                Options = new[] { 
              
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
                                       ConditionalEvents = new[]{"DEMOISLANDMAP_timedSpawnBeginningPopulationNormal"},
                                       ActionKeys = new[]{ "setMaxTwinklersNormal", "setMaxThunderChickensNormal", "setMaxBinalRatsNormal", "setBinalRatSpawnOften", "setTwinklerSpawnIntervalSouthSeldom", "setTwinklerSpawnIntervalEastOften" }
                                      
                                    }
                                }
                               
                          },*/
                         
                          /*
                            new OptionSet(){
                              KeyName = "resources", // MP :   not being used because its commented out above, in OptionsToUse. but I can't delete it then I get an error.///
                               Name = "Resources",
                               DisplayGroup = 2,
                                Options = new[] { 
                           
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
                                    }
                                }
                            }*/
                           
                                               } 

                };


            return scenarioData;

        }


       

    }
}
