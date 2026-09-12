using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Scenarios;
using UWGame.SimSide.InGameEvents.Actions;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.AllGameData.Scenarios.Scenario_6.Data;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_6 // I chose a generic name so scenario name changes don't affect us so much...
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
    public class Scenario6Loader : ScenarioLoader // I chose a generic name so scenario name changes don't affect us so much...
    {
        //private const string folderName = ;


        public override string FolderName
        {
            get { return "The Clay Pit"; }
        }

        protected override Scenario InitScenarioHeader()
        {
            Scenario scenario;

            scenario = new Scenario()
            {

                Name = FolderName,
                DisplayName = "The Clay Pit",
                TimeDateYear = new DateAndTime.TimeDateYear() { Year = 193, Day = 2, TimeOfDay = 0.50 }, //was Year = 180, Day = 5,         match TimeOfDay with music track list (night/day music).  and match date/day with event screen texts
                MapKey = "n Clay River Bank",
                MapSize = SimSide.Scenarios.MapSize.Small,                
                Allow32Bit = true,
                SummaryDescription = "TUTORIAL 2 + OPEN-ENDED: A work crew signs up for making mudbricks in a clay pit. Their temporary camp might turn into a settlement. \n \nERA: The Great Descent",
                Description = "TUTORIAL NO. 2 + OPEN-ENDED \n \n-You must all be hard up for money, else you wouldn't be interested in this... It's back breaking labor, digging clay and making mudbricks. The site is a short trip up the river. Questions? \n-When do we get paid and how much? \n-The contract ends in a couple of months. Then we'll split the profit evenly and head back to Tellus. \n-What about food? \n-When we sell our first boatload of mudbricks we'll buy provisions for some of the money. Now, who wants to sign up?",
                ThumbnailImage = "Scenarios/Scenario 6/Scenario Screen/clayPit_scenario_thumb", //
                Image = "GroupMeeting", 
                IsInDevelopment = false,
                SortOrder = 1,
            };


            return scenario;
        }

        public override DataLoader GetDataLoader()
        {
            DataLoader dataLoader = new Scenario6DataLoader();
            dataLoader.FolderName = FolderName;

            return dataLoader;
        }


        protected override ScenarioData InitScenarioData()
        {
            

            ScenarioData scenarioData = null;
                        
           
                scenarioData = new ScenarioData()
                {
                    LoadingBackgroundImage = "Scenarios/Scenario 4/Scenario Screen/TitleImgManFence_1920px",

                    LoadingDialogText = " \n-Hey. I heard there's patricians close to the clay pit? \n-Yeah, they have their territory at Blue Creek. But we just stay away from them. We'll probably put up a fence also. Don't be scared!",

                    LoadingDialogImage = "RiverBoat",

                    SpawnWorldAction = "spawnWorld",
                    SpawnSiteAction = "spawnPlaySite",

                    WorldMapImage = "Scenarios/Default/GUI/regionalMap_5",
                   
                    CustomEnabled = false,

                    //global events to register regardless of customization/difficulty:
                    ConditionalEvents = new[]
                    { 
                        "CLAYPIT_introDialogue",
                       "triggerAtFence", // talk and warning about the patricians
                       "placeTerritoryLockFence", //protects the abatis from being salvaged by player until the contract is over   
                       "CONTRACTEND_40mudBricksStored", // check which keeps an eye on the mudbricks and when 40 is STORED will make a boolean true.
                                                                            
            #region music and game over
                   //this is an exact 2 day playlist: 
                   "SANDBOXNOMADMAP_musicTrackList",
                   "Scenario6_winGame",
                   "Scenario6_loseContract",
                   "SANDBOXMAP_loseGame",
#endregion

                  #region animal spawns
                   // moved to fauna setting:     "CLAYPIT_timedSpawnBeginningPopulationNormal",
                  
                       /*   "CLAYPIT_continualSpawnPatricians",

                        "CLAYPIT_continualSpawnThunderChickens#2",

                        "CLAYPIT_continualSpawnBinalRats#1",
                        "CLAYPIT_continualSpawnBinalRats#2",
                        "CLAYPIT_continualSpawnBinalRats#3",

                        "CLAYPIT_continualSpawnMudWorms#1",                        
                       */

#endregion

                  #region fish trap and farm plots enabling
//////////////////Necessary for enabling farming and fish traps (they call docs outside of the scenario files):
                        "initializeGlobalFarmingProperties",                       
                        "initializeGlobalFishTrapProperties",
                        "initializeGlobalAnimalTrapProperties",
///////////////////////////////////////////////////
                #endregion


                    }, 

            ////////// these are actions that are always performed regardless of difficulty:
                    Actions = new string[]
                    {

//these lines are for testing purposes:---!!!!!!!---COMMENT OUT BEFORE DEPLOY----------------------------!!!!!-----------------------
#region TESTING EQUIPMENT //Comment out!
#if DEBUG || PROFILE
                                 /*  "startStructureKiln",                               
                                     "startClay", "startClay", "startWetMudBricks","startWetMudBricks","startWetMudBricks","startWetMudBricks","startWetMudBricks","startWetMudBricks","startWetMudBricks","startWetMudBricks","startWetMudBricks","startWetMudBricks",
                                    "startWetMudBricks","startWetMudBricks","startWetMudBricks","startWetMudBricks","startWetMudBricks","startWetMudBricks","startWetMudBricks","startWetMudBricks","startWetMudBricks","startWetMudBricks","startWetMudBricks","startWetMudBricks","startWetMudBricks","startWetMudBricks","startWetMudBricks","startWetMudBricks","startWetMudBricks","startWetMudBricks","startWetMudBricks","startWetMudBricks","startWetMudBricks","startWetMudBricks","startWetMudBricks","startWetMudBricks","startWetMudBricks","startWetMudBricks","startWetMudBricks","startWetMudBricks","startWetMudBricks","startWetMudBricks","startWetMudBricks","startWetMudBricks","startWetMudBricks","startWetMudBricks","startWetMudBricks","startWetMudBricks",
                                  "startClay", "startClay", "startClay","startClay","startClay", "startStone",  "startStone","startFirewood", "startFirewood", "startFirewood", "startFirewood",
                                "startSolidMudBricks","startSolidMudBricks","startSolidMudBricks","startSolidMudBricks","startSolidMudBricks",
                                */
                         
                        // "startFlintRough","startFlintRough","startFlintRough",
                               // "startSolidMudBricks", 
                                //"startExtrusionMachine",
//"startPlasticWorkshop", //"startStill",
// "startKilnImprovisedSmall",

//"startMarshcotSap","startMarshcotSap","startMarshcotSap","startMarshcotSap","startMarshcotSap","startMarshcotSap","startMarshcotSap","startMarshcotSap","startMarshcotSap","startMarshcotSap","startMarshcotSap",
//"startSulfurPowder", "startSulfurPowder","startSulfurPowder", "startSulfurPowder","startSulfurPowder", "startSulfurPowder",

/*"startGlassyCreeper", "startGlassyCreeper","startGlassyCreeper", "startGlassyCreeper","startGlassyCreeper", "startGlassyCreeper","startGlassyCreeper", "startGlassyCreeper","startGlassyCreeper", "startGlassyCreeper",
"startGlassyCreeper", "startGlassyCreeper","startGlassyCreeper", "startGlassyCreeper","startGlassyCreeper", "startGlassyCreeper","startGlassyCreeper", "startGlassyCreeper","startGlassyCreeper", "startGlassyCreeper",
"startGlassyCreeper", "startGlassyCreeper","startGlassyCreeper", "startGlassyCreeper","startGlassyCreeper", "startGlassyCreeper","startGlassyCreeper", "startGlassyCreeper","startGlassyCreeper", "startGlassyCreeper",
"startGlassyCreeper", "startGlassyCreeper","startGlassyCreeper", "startGlassyCreeper","startGlassyCreeper", "startGlassyCreeper","startGlassyCreeper", "startGlassyCreeper","startGlassyCreeper", "startGlassyCreeper",
"startGlassyCreeper", "startGlassyCreeper","startGlassyCreeper", "startGlassyCreeper","startGlassyCreeper", "startGlassyCreeper","startGlassyCreeper", "startGlassyCreeper","startGlassyCreeper", "startGlassyCreeper",
"startGlassyCreeper", "startGlassyCreeper","startGlassyCreeper", "startGlassyCreeper","startGlassyCreeper", "startGlassyCreeper","startGlassyCreeper", "startGlassyCreeper","startGlassyCreeper", "startGlassyCreeper",
"startGlassyCreeper", "startGlassyCreeper","startGlassyCreeper", "startGlassyCreeper","startGlassyCreeper", "startGlassyCreeper","startGlassyCreeper", "startGlassyCreeper","startGlassyCreeper", "startGlassyCreeper",
"startGlassyCreeper", "startGlassyCreeper","startGlassyCreeper", "startGlassyCreeper","startGlassyCreeper", "startGlassyCreeper","startGlassyCreeper", "startGlassyCreeper","startGlassyCreeper", "startGlassyCreeper",
"startGlassyCreeper", "startGlassyCreeper","startGlassyCreeper", "startGlassyCreeper","startGlassyCreeper", "startGlassyCreeper","startGlassyCreeper", "startGlassyCreeper","startGlassyCreeper", "startGlassyCreeper",
"startGlassyCreeper", "startGlassyCreeper","startGlassyCreeper", "startGlassyCreeper","startGlassyCreeper", "startGlassyCreeper","startGlassyCreeper", "startGlassyCreeper","startGlassyCreeper", "startGlassyCreeper",
 * */
//"startFishTrapBasket",

//  "startHoe", "startHoe",  "startCrystalBerries", "startCrystalBerries",
             
             //           
              //        
/*                      
            "startSpoakLeaves", "startSpoakLeaves",
             "startWaterCaneStem", "startWaterCaneStem", "startWaterCaneStem",
             "startSpoakShingles", "startSpoakShingles",
             "startSolidMudBrick", "startSolidMudBrick","startSolidMudBrick","startSolidMudBrick","startSolidMudBrick",
             "startSpoakBranchesTrimmed",  "startSpoakBranchesTrimmed",  "startSpoakBranchesTrimmed",

             "startShadeleafCanes", "startShadeleafCanes", "startShadeleafCanes", 
             "startRadioAntenna", "startRadio",
             "startSpikeTrap","startSpikeTrap","startSpikeTrap", */
            // "startSentry",
 
         //     "startWroughtIron", "startWroughtIron", "startWroughtIron",
         //     "startCharcoal", "startCharcoal", "startCharcoal",
         //     "startBellows",
         //     "startHammer",
 /*            "startVat",
 "startSpikeTrap",

 "startBlackpulp",
 "startBlunderbuss", "startBlackPowderShotAmmo", "startBlackPowderShotAmmo",
 "startTurnipCracker", "startNeonHornetsLive",  "startBlackpowder",
"startBlowpipe", "startPickaxe", "startSpade", "startAnvil",
"startWaterCaneStem", "startWaterCaneStem","startWaterCaneStem", "startWaterCaneStem","startWaterCaneStem", "startWaterCaneStem",


"startWetFirewood", "startWetFirewood", "startWetFirewood",
"startGoldOre", "startGoldOre", "startBogOre","startBogOre","startBogOre","startBogOre","startBogOre",
"startImprovisedGreenHouseCover", "startImprovisedGreenHouseCover","startImprovisedGreenHouseCover","startImprovisedGreenHouseCover","startImprovisedGreenHouseCover","startImprovisedGreenHouseCover", 
"startFingerFruit", "startFingerFruit",
"startSalt", "startSalt", "startSalt","startSalt","startSalt","startSalt",
"startClayPotUnglazed", "startTappingBucket","startTappingBucket","startClayJar","startClayJar","startClayJar",
"startClay", "startClay", "startClay","startClay","startClay","startClay",
"startFirewood","startFirewood", "startCharcoal","startCharcoal","startCharcoal",

"startMarshcotSap", "startMarshcotSap",
"startWingweedMats","startWingweedMats","startWingweedMats",

"startIronHooks", ,
"startIronHandAxe", "startIronSpear",
"startTurnipSalami","startTurnipSalami","startTurnipSalami","startTurnipSalami","startTurnipSalami","startTurnipSalami", "startVinegar", "startVinegar",
"startHardtack","startHardtack","startHardtack","startHardtack","startHardtack","startHardtack","startHardtack", "startDriedSaltedStreakFin","startDriedSaltedStreakFin","startDriedSaltedStreakFin","startDriedSaltedStreakFin",
*/

#endif
#endregion   
//-------------------------------------------------!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!--------------------------------------------


                  #region init game over, burial text
                        "initStoryPartOver", "initStores40Mudbricks","initBurialText1", "initBurialText2", "initBurialText3", "initBurialText4", "initBurialText5",
            #endregion

                        "initEndDate", 
                        
                        #region Group meetings
                           "initEnableGroupMeetings", "initTimeBeforeGroupMeeting", 
                           "meetingEmigrateThreat", "meetingEmigrateThreatAllUnhappy", 
                          "meeting3Security",  "meeting3Food",   "meeting3Comfort",
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

                  #region animal expeditions


                      


                          "spawnMudWormExpedition#1", "spawnMudWormExpedition#2", // trigger-spawned worms need nearby expeditions before they appear
                         "spawnPatricianExpedition#1",                      
                         "spawnThunderChickenExpedition#2", "spawnBirdExpedition#1",
                        "spawnBinalRatExpedition#1", "spawnBinalRatExpedition#2","spawnBinalRatExpedition#3",
 



#endregion

                        "spawnPlayerAllegiance", "placeExpedition", "setView", "exploreShroud", 
                        "setPlayerCredits",  
                        "startNaturalTerminal", //natural terminal leads to wilderness

#region Spawn wildernessSite1                
                       //   "spawnWildernessSite1",  "spawnWildernessSite1Allegiance1", "spawnWildernessSite1Expedition1",                                
                           
#endregion

//NEIGHBOR town/////////////////////////////
#region Spawn otherSite1                
                          "spawnOtherSite1",  "spawnOtherSite1Allegiance1", "spawnOtherSite1Expedition1",                                
                          "spawnPlaySiteSite1Route", //for trade
                          "spawnPlaySiteSite1LandRoute", //for emigrating
#endregion
#region otherSite1Expedition1 ////structures and assets //delete 
                  //        "spawnOtherSite1Expedition1Wharf", "spawnOtherSite1Expedition1RadioHut", "spawnOtherSite1Expedition1Barge",  
#endregion


#region otherSite1Expedition1 ///migrants
                          
                       
                       "spawnImmigrantOtherSite1MenialSpecialist","spawnImmigrantOtherSite1MenialSpecialist","spawnImmigrantOtherSite1MenialSpecialist","spawnImmigrantOtherSite1MenialSpecialist","spawnImmigrantOtherSite1MenialSpecialist","spawnImmigrantOtherSite1MenialSpecialist","spawnImmigrantOtherSite1MenialSpecialist","spawnImmigrantOtherSite1MenialSpecialist",
                       
                       "spawnImmigrantOtherSite1Random","spawnImmigrantOtherSite1Random","spawnImmigrantOtherSite1Random","spawnImmigrantOtherSite1Random","spawnImmigrantOtherSite1Random","spawnImmigrantOtherSite1Random",
                       "spawnImmigrantOtherSite1SmithingSpecialist",
#endregion


//***************************



//////////////////////////////////////////
                #region structures

                "startStructureLean-toSpoakLeaves1", "startStructureLean-toSpoakLeaves2",
                "startStructureA-frameSpoakLeaves1",// "startStructureA-frameSpoakLeaves2",
                "startStructureCampfire",
                "startStructureCanopyPort",

                "startStructureAbatis1",
                "startStructureAbatis2",
                "startStructureAbatis3",
                "startStructureAbatis4",
                "startStructureAbatis5",
                "startStructureAbatis6",

            //    "startStructureKiln",

      /*          "startStructureGreenhouse",
                "startStructureCompostPit",
                "startStructureParasolHouse",
                "startStructureToolshed",
                "startStructureFirewoodStack",

                "startStructureClayGranary",
                "startStructureMeatDryingRack",
                "startStructureMudBrickKitchen",

                "startStructureImprovisedWorkbench",
                "startStructureRadioHut",

*/
                #endregion

                #region         stockpiles
                "startBricksStockpile", 
                #endregion

                  #region pier spot
                        "startPierSpot1", 
                  #endregion

                  #region start people, dog, robot spawn ..........moved to options

#endregion 

                  #region particle effects                  
                        "smallFog1", "smallFog2", "smallFog3",
                         "fog1", 
            #endregion

                    },

         ////////////////////////////////

                    #region Difficulties
                    MainDifficultySettings = new[]
                    {
                        

                          new Difficulty()
                         {
                              KeyName = "easy",
                              Name = "Tutorial",
                               Description = "Learn the game by reaching the objective with help from instructions and hints along the way.",
                              IsDefault = true,
                              OptionsToUse = new SerializableDictionary<string, string[]>()
                              {

                                    { "expeditionType", new []{"mudBrickExpedition"} },
                                    { "tutorial" , new []{"tutorialOn"} },//the only difference
                                    { "fauna" , new []{"average"} },
                                    { "resources" , new []{"average"} },
                                   // { "gameDuration" , new []{ "normal" }}//

                              },
                         }, 
                          new Difficulty()
                         {
                              KeyName = "normal",
                              Name = "No tutorial",
                              Description = "Reach the objective without any instructions and hints.",
                              IsDefault = false,
                              OptionsToUse = new SerializableDictionary<string, string[]>()
                              {
                                    { "expeditionType", new []{"mudBrickExpedition"} },
                                    { "tutorial" , new []{"tutorialOff"} }, //the only difference
                                    { "fauna" , new []{"average"} },
                                    { "resources" , new []{"average"} },
                                  //  { "gameDuration" , new []{ "normal" } }//
                              },
                         }, 

                     },
                    #endregion

                    #region OptionSets: Tutorial on/off, Fauna, People, Equipment Loadout, Resources, Relation

                    OptionSets = new[]
                    {
                    #region Fauna options
                        new OptionSet()
                        {
                            KeyName = "fauna",
                            Name = "Fauna",
                            DisplayGroup = 2,
                            Options = new[] 
                            { 

                                new Option()
                                {
                                    KeyName = "average",
                                    Name = "Average",                                
                                    Difficulty = new CustomDifficulty()
                                    {
                                        KeyName = "normal",
                                        Name = "Average", //not shown if only one
                                        IsDefault = true,   
                                        ScoreModifier = 1f
                                    },
                                    ConditionalEvents = new[]{ /*"CLAYPIT_timedSpawnBeginningPopulationNormal",*/ "CLAYPIT_triggerMudWormsWest" , "CLAYPIT_triggerMudWormsCenter"},
                                    /*ActionKeys = new[]
                                    {
                                       // "spawnBeginningPopulationNormal",
                                        "setMaxPatriciansNormal",      "setSpawnIntervalPatriciansNormal",
                                        "setMaxMudWormsNormal",      "setSpawnIntervalMudWormsNormal",
                                        "setMaxBinalRatsNormal",        "setSpawnIntervalBinalRatsNormal",
                                        "setMaxThunderChickensNormal",  "setSpawnIntervalThunderChickensNormal",
                                       
                                    }*/
                                },


                            }
                        },
#endregion
                     

                    #region Tutorial on/off options
                        new OptionSet()
                        {
                            KeyName = "tutorial",
                            Name = "Tutorial",
                            DisplayGroup = 2,
                            Options = new[] 
                            { 

                                new Option()
                                {
                                    KeyName = "tutorialOn",
                                    Name = "Tutorial On",                                
                                    Difficulty = new CustomDifficulty()
                                    {
                                        KeyName = "easy",
                                        Name = "With tutorial", //shown on CUSTOM selector. not shown if only one
                                        IsDefault = true,   
                                        ScoreModifier = 1f
                                    },
                                    ActionKeys = new[]{ "setTutorialOn"},
                                    ConditionalEvents = new[]{  "CLAYPIT_introScreenTut",                                                      
                                                              //kiln ready and 3 mudbricks ready fire through eventhooks
                                                              "TUTORIAL_check40MudBricks",
                                                              "TUTORIAL_40mudBricksStored", 
                                                              "TUTORIAL_checkIfContinueGame",
                                    },

                                },

                                new Option()
                                {
                                    KeyName = "tutorialOff",
                                    Name = "Tutorial Off",                                
                                    Difficulty = new CustomDifficulty()
                                    {
                                        KeyName = "normal",
                                        Name = "Without tutorial", //shown on CUSTOM selector. not shown if only one
                                        IsDefault = true,   
                                        ScoreModifier = 1f
                                    },
                                    ActionKeys = new[]{ "setTutorialOff"},
                                    ConditionalEvents = new[]{ "CLAYPIT_introScreenNoTut"
                                    },

                                },


                            }
                        },
#endregion 

 
                    #region Expedition type options. Intro dialogue, members, items.
                          new OptionSet(){
                               KeyName = "expeditionType",
                               Name = "Expedition type",
                                DisplayGroup = 0,
                                Options = new[] { 


                                    new Option()
                                    {
                                      KeyName = "mudBrickExpedition",
                                      Name = "Mud brick makers",
                                      Difficulty = new CustomDifficulty()
                                      {
                                           KeyName = "normal",
                                           Name = "Specialized", //not shown when only one in this difficulty category
                                           ScoreModifier = 1f
                                      },  
                              /*        ConditionalEvents = new[]{                                                                
                                                                                                     
                                      },*/
                                       ActionKeys = new[]
                                       { //STARTING LOCATION:
                                           "setStartingLocation",
                                           
                                        //PEOPLE:
                                        //menial with survival personality
                                         "spawnMenialSpecialist1", "spawnMenialSpecialist2", //"spawnMenialSpecialist3",
                                         "spawnMenialSpecialist4","spawnMenialSpecialist5",
                                         "spawnMenialSpecialist6","spawnMenialSpecialist7","spawnMenialSpecialist8","spawnMenialSpecialist9",//"spawnMenialSpecialist10",
  



                                          /////LOADOUT:
                                    //spawns in open:
                                     "startPickaxe", "startPickaxe",
                                     "startSteelSpade", "startSteelSpade","startSteelSpade","startSteelSpade","startSteelSpade",                                     
                                     "startKnife",
                                     "startGoldPot",
                                     "startBrickMold",
                                     "startRawhideString",
                                       //dont want them to starve at end of tut: . they are 8 people, around 2-3 meals each.
                                     "startSmokedThunderChicken","startSmokedThunderChicken","startSmokedThunderChicken","startSmokedThunderChicken","startSmokedThunderChicken","startSmokedThunderChicken","startSmokedThunderChicken","startSmokedThunderChicken",
                                     "startSmokedThunderChicken","startSmokedThunderChicken","startSmokedThunderChicken","startSmokedThunderChicken", "startSmokedThunderChicken","startSmokedThunderChicken",
                                    "startCommonOilTubers","startCommonOilTubers","startCommonOilTubers","startCommonOilTubers","startCommonOilTubers", "startCommonOilTubers","startCommonOilTubers","startCommonOilTubers",
                                    
                                    /////////TEST EQUIPMENT AT TOP OF DOC
                                    


                                       }
                                    },
  



                                }
                          },
#endregion

                    #region Resources options 
                    new OptionSet()
                    {
                        KeyName = "resources",
                        Name = "Resources",
                        DisplayGroup = 2,
                        Options = new[] 
                        { 
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
                                ActionKeys = new[]
                                { 
                                    "setFishSchoolMedium", "setNormalResources", 
                                    //mp we have no fish traps made from shadeleaf because there are no shadeleaf trees on map OR the shadeleaf trees are far away.
                                    "startFishTrapCreek1", "startFishTrapCreek2", "startFishTrapCreek3",
 
                                    "startClayDeposit1",
                                    "startBogOreDeposit1", 
                                    "startPeatDeposit1",
                                                                             }
                            },
            

                        }
                    }, 
                    #endregion

                    #region  otherSite1Allegiance1Relation          
                            new OptionSet(){
                              KeyName = "otherSite1Allegiance1Relation",
                               Name = "Other Site 1 Relation",
                               DisplayGroup = 2,
                                Options = new[] { 

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
                                    }
                                }
                            },
                #endregion

                            /*
                    #region timed option end
                            new OptionSet()
                            {
                              KeyName = "gameDuration", // time before rescue arrives.
                               Name = "Duration",
                               DisplayGroup = 3,
                                Options = new[] { 
                              
                                    new Option()
                                    {
                                      KeyName = "normal",
                                      Name = "Normal",
                                     
                                      Difficulty = new CustomDifficulty()
                                      {
                                           KeyName = "normal",
                                           Name = "Normal duration",
                                           IsDefault = true,
                                           ScoreModifier = 1f
                                      },
                                      // ActionKeys = new[]{ "initEndDate" }  
                                     
                                    }
                                  
                                }
                            }  
                    #endregion
*/

                    }
              


    
                
                    #endregion
                };

            return scenarioData;
        }
    } 
}
