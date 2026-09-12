using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Scenarios;
using UWGame.SimSide.InGameEvents.Actions;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.AllGameData.Scenarios.Scenario_4.Data;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_4 // I chose a generic name so scenario name changes don't affect us so much...
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
    public class Scenario4Loader : ScenarioLoader // I chose a generic name so scenario name changes don't affect us so much...
    {
        //private const string folderName = ;


        public override string FolderName
        {
            get { return "Fields of Tau Ceti - Cudgel Hills (L)"; }//was: (Large)
        }

        protected override Scenario InitScenarioHeader()
        {
            Scenario scenario;

            scenario = new Scenario()
            {
                
                Name = FolderName,
                DisplayName = "Fields of Tau Ceti - Cudgel Hills (L)",
                TimeDateYear = new DateAndTime.TimeDateYear() { Year = 180, Day = 5, TimeOfDay = 0.50 }, //match with music track list
                MapKey = "j SandboxNomads", //
                MapSize = SimSide.Scenarios.MapSize.Large,
                Allow32Bit = false,
                SummaryDescription = "OPEN-ENDED MAP: A small community sets out to build a settlement in the wilderness. Lacking advanced technology, they rely on ancient methods when working the land. \nNOTE: This scenario is the Large map version of CUDGEL HILLS. \nERA: The Great Descent", //Please note that there are still some performance issues with large maps such as this. Sometimes the game can run out of memory right after saving. Also, sometimes the game can perform poorly after 45 mins or so. Play at your own risk until we have solved these issues. \nWe are happy to receive error reports from this map on the forum, they can help us optimize it.
                Description = "OPEN-ENDED MAP. \n \n'Out here, a new life awaited. Any differences lay behind them and they could now do things their own way. What they lacked in possessions they made up for with determination. The wilderness would welcome those who were brave and stood undivided.'",                    
                ThumbnailImage = "Scenarios/Scenario 4/Scenario Screen/colonyFarmDogRobot_scenario_thumb", //
                Image = "Farming", 
                IsInDevelopment = false,
                SortOrder = 6,
            };


            return scenario;
        }

        public override DataLoader GetDataLoader()
        {
            DataLoader dataLoader = new Scenario4DataLoader();
            dataLoader.FolderName = FolderName;

            return dataLoader;
        }


        protected override ScenarioData InitScenarioData()
        {
            

            ScenarioData scenarioData = null;
                        
           
                scenarioData = new ScenarioData()
                {
                    LoadingBackgroundImage = "Scenarios/Scenario 4/Scenario Screen/TitleImgManFence_1920px",
                    //mp the place name has to work with both starting positions. was: BATTEN CREEK. Also work with the location name on map.
                    LoadingDialogText = "CUDGEL HILLS, SUMMER, YEAR 180 \n \nCastor took the last bag ashore and put it next to the others. He looked at his companions who stood dispersed on the riverside and tried to guess their thoughts. Their faces showed expressions ranging from thrill to apprehension. \n \nThe boatman was ready to set off. 'Good luck, people. I don't agree with all that you did back there, but I don't think you were treated fairly either. I know there are a few people who would like to join you, so give a call when you have that radio set up.' \n \nAs the boat sailed away, Castor turned his attention to Linsey who sat hunched, a small display in her hands. She was waking up from a long nap on the boat. Squinting, she studied her PPU which was also coming alive now. It was busily piling up data as her fellow travellers examined the location, revealing its secrets. \n \nShe looked up and smiled: 'It looks promising...good fishing, good soil.' Castor turned towards the other companions and spread out his arms, embracing the orange hills, the yellow trees, the horizon.",
                    //I took out the middle part that explains the PPU. because the text got too long I think.        //...and come knocking. \n \nAs the boat sailed away, Castor turned his attention to Linsey who sat hunched, a small display in her hands. She was waking up from a long nap on the boat. Squinting, she studied her PPU which was also coming alive now. It was busily piling up data as her fellow travellers examined the location, revealing its secrets. \n \nThe Pioneer Planning Units were ancient, amazing devices for wilderness survival. But the PPU didn't know every plant and animal. Most of the knowledge stored within was from the time when humans first came to Antheia. People, for some reason, stopped adding new knowledge a couple hundred years ago. But that was a minor shortcoming. The PPU's were a blessing for anyone settling this planet's wilderness and would serve them well here. \n \nElrina looked up and smiled: 'It looks promising...good fishing, good soil.' Castor turned towards the other companions and spread out his arms, embracing the orange hills, the yellow trees, the horizon.",
                    LoadingDialogImage = "RiverBoat",

                    SpawnWorldAction = "spawnWorld",
                    SpawnSiteAction = "spawnPlaySite",

                    WorldMapImage = "Scenarios/Default/GUI/regionalMap_2",

                    EnableMissions = true,

                    //global events to register regardless of customization/difficulty:
                    ConditionalEvents = new[]
                    { 

            #region music and game over
                   //this is an exact 2 day playlist: 
                   "SANDBOXNOMADMAP_musicTrackList",

                   "SANDBOXMAP_loseGame",
#endregion

                  #region animal spawns
                   // moved to fauna setting     "SANDBOXNOMADMAP_timedSpawnBeginningPopulationNormal",

                         "SANDBOXNOMADMAP_continualSpawnSnatcher",
                        //"SANDBOXNOMADMAP_continualSpawnSnatcher#2",
                      //  "SANDBOXNOMADMAP_continualSpawnThunderChickens#1", mp removed for optimization jan 2016
                        "SANDBOXNOMADMAP_continualSpawnThunderChickens#2",
                        "SANDBOXNOMADMAP_continualSpawnBinalRats#1",
                        "SANDBOXNOMADMAP_continualSpawnBinalRats#2",
                        "SANDBOXNOMADMAP_continualSpawnBinalRats#3",
                     //   "SANDBOXNOMADMAP_continualSpawnBinalRatsSwamp", mp removed for optimization jan 2016
                     //   "SANDBOXNOMADMAP_continualSpawnBinalRatsSwamp2", mp removed for optimization jan 2016
                        "SANDBOXNOMADMAP_continualSpawnTurnipsNorth",
   // moved to fauna setting: "SANDBOXNOMADMAP_continualSpawnBushDragonSouth",
                        "SANDBOXNOMADMAP_continualSpawnSlugs",                        
                        "SANDBOXNOMADMAP_continualSpawnLeafcutter#1",
                   //     "SANDBOXNOMADMAP_continualSpawnLeafcutter#2", mp removed for optimization jan 2016
                        "SANDBOXNOMADMAP_continualSpawnLeafcutter#3",
                        //"SANDBOXNOMADMAP_continualSpawnLeafcutter#4",
                        //"SANDBOXNOMADMAP_continualSpawnLeafcutter#5",
                        "SANDBOXNOMADMAP_continualSpawnDemonTree#1",
                        "SANDBOXNOMADMAP_continualSpawnDemonTree#2",
                        "SANDBOXNOMADMAP_continualSpawnDemonTree#3",
                        "SANDBOXNOMADMAP_continualSpawnSwampDemonTree#1",
                        "SANDBOXNOMADMAP_continualSpawnSwampDemonTree#2",
                        //nests:
                        "SANDBOXNOMADMAP_spawnLeafcutterNests",
                        //animal migration events:
                        "SANDBOXNOMADMAP_migrationLesserWhipjawEast",
                        "SANDBOXNOMADMAP_migrationBajinganNorth",

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
#region TESTING EQUIPMENT 
#if DEBUG || PROFILE
 // "startHoe", "startHoe", "startGlassyCreeper", "startGlassyCreeper", "startCrystalBerries", "startCrystalBerries","startHoe", "startHoe", "startGlassyCreeper", "startGlassyCreeper", "startCrystalBerries", "startCrystalBerries","startHoe", "startHoe", "startGlassyCreeper", "startGlassyCreeper", "startCrystalBerries", "startCrystalBerries","startHoe", "startHoe", "startGlassyCreeper", "startGlassyCreeper", "startCrystalBerries", "startCrystalBerries",
//"startStructureRadioHutImprovised",

 //"startStructureSimplePort", "startStructureRadioHutImprovised",
 
 //"startStructureLanding",              
             
              //        
                     
     //       "startSpoakLeaves", "startSpoakLeaves","startSpoakLeaves", "startSpoakLeaves","startSpoakLeaves", "startSpoakLeaves",
     //         "startWaterCaneStem", "startWaterCaneStem", "startWaterCaneStem",
     //        "startSpoakShingles", "startSpoakShingles",
     //        "startSolidMudBrick", "startSolidMudBrick","startSolidMudBrick","startSolidMudBrick","startSolidMudBrick",
     //        "startSpoakBranchesTrimmed",  "startSpoakBranchesTrimmed",  "startSpoakBranchesTrimmed",
     
      //       "startShadeleafCanes", "startShadeleafCanes", "startShadeleafCanes", "startShadeleafCanes","startShadeleafCanes",

//"startTurnipSalami","startTurnipSalami","startTurnipSalami","startTurnipSalami","startTurnipSalami","startTurnipSalami", "startVinegar", "startVinegar",
//"startHardtack","startHardtack","startHardtack","startHardtack","startHardtack","startHardtack","startHardtack", "startDriedSaltedStreakFin","startDriedSaltedStreakFin","startDriedSaltedStreakFin","startDriedSaltedStreakFin",

/*      "startRadioAntenna", "startRadio",
             "startSpikeTrap","startSpikeTrap","startSpikeTrap", */
            // "startSentry",
//structures:
    //     "startStructureSimplePort", 
    //          "startStructureRadioHutImprovised", //"startStructureImprovisedSmithy", // testing
         //     "startWroughtIron", "startWroughtIron", "startWroughtIron",
         //     "startCharcoal", "startCharcoal", "startCharcoal",
         //     "startBellows",
         //     "startHammer",
 /*            "startVat",
 "startSpikeTrap",

 "startBlackpulp",
 "startBlunderbuss", "startBlackPowderShotAmmo", "startBlackPowderShotAmmo",
 "startTurnipCracker", "startNeonHornetsLive", "startFishTrapBasket", "startBlackpowder",
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

*/

#endif

#endregion   
//-------------------------------------------------!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!--------------------------------------------

                        "initGameOver",
                  #region init  burial text
                         "initBurialText1", "initBurialText2", "initBurialText3", "initBurialText4", "initBurialText5",
 
                    #endregion
                    
                    #region Group meetings
                           "initEnableGroupMeetings", "initTimeBeforeGroupMeeting", 
                           "meetingEmigrateThreat", "meetingEmigrateThreatAllUnhappy",
                          "meeting3Security", "meeting3Food",   "meeting3Comfort", 
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
                        "spawnSlugExpedition#1", 
                        "spawnBinalRatExpedition#1", "spawnBinalRatExpedition#2","spawnBinalRatExpedition#3", 
                        //"spawnBinalRatExpeditionSwamp","spawnBinalRatExpeditionSwamp2", mp removed for optimization jan 2016
                        "spawnLeafcutterExpedition#1", "spawnLeafcutterExpedition#3", //"spawnLeafcutterExpedition#4","spawnLeafcutterExpedition#5",
                     //   "spawnLeafcutterExpedition#2", mp removed for optimization jan 2016
                        "spawnSnatcherExpedition#1",//"spawnSnatcherExpedition#2",
                       // "spawnThunderChickenExpedition#1", mp removed for optimization jan 2016
                        "spawnThunderChickenExpedition#2",
                        "spawnTurnipExpeditionNorth",
                        "spawnBushDragonExpeditionSouth",
                        "spawnDemonTreeExpedition#1",  "spawnDemonTreeExpedition#3",
                      //  "spawnDemonTreeExpedition#2", //  removed for optimization jan 2016
                        "spawnSwampDemonTreeExpedition#2",

                      //  "spawnBajinganExpeditionWest",//migration target - is in spawn critter in actionsetsloader
                        //"spawnLesserWhipjawExpeditionWest",//migration target - is in spawn critter in actionsetsloader
                        //used for destroying migrating animals at map edge:
                        "spawnAnimalMigrateTriggerWest", "spawnAnimalMigrateTriggerRiver", 
#endregion

                        "spawnPlayerAllegiance", "placeExpedition", "setView", "setPlayerCredits",

    "exploreShroudNaturalTerminal", //reveal natural terminal to wilderness

#region Spawn wildernessSite1                
                          "spawnWildernessSite1",  "spawnWildernessSite1Allegiance1", "spawnWildernessSite1Expedition1",                                
                          "spawnPlaySiteWildernessSite1Route", 
#endregion


#region Spawn otherSite1                
                          "spawnOtherSite1",  "spawnOtherSite1Allegiance1", "spawnOtherSite1Expedition1",                                
                          "spawnPlaySiteSite1Route",
#endregion


#region otherSite1Expedition1 ///migrants

                          //mp: my aim is to get a smith and a couple more join the player early (bare field). start can be a bit random (should be 2-5 people, always with a smith) 
                          //...then comes a mix of people ranging evenly across the spectrum up to higher demands. professions evenly mixed.

                          //early joiner immigrants
                       "spawnImmigrantSmithingSpecialist", //important to get a smith early
                    //   "spawnImmigrantMenialSpecialist", //removed this because there was enough early joiners.

                       //next immigrants
                       "spawnImmigrantSurvivalTier","spawnImmigrantSurvivalTier","spawnImmigrantSurvivalTier","spawnImmigrantSurvivalTier", "spawnImmigrantSurvivalTier", "spawnImmigrantSurvivalTier", "spawnImmigrantSurvivalTier",
                         //also next immigrants (there will be some overlap)
                       "spawnImmigrantBasicTier","spawnImmigrantBasicTier","spawnImmigrantBasicTier","spawnImmigrantBasicTier","spawnImmigrantBasicTier","spawnImmigrantBasicTier","spawnImmigrantBasicTier", "spawnImmigrantBasicTier",
                       //for achieving medium tier:
                        "spawnImmigrantMediumTier","spawnImmigrantMediumTier","spawnImmigrantMediumTier","spawnImmigrantMediumTier","spawnImmigrantMediumTier", "spawnImmigrantMediumTier","spawnImmigrantMediumTier","spawnImmigrantMediumTier",
                       //advanced security tier: ..the other principles are basic:
                       "spawnImmigrantAdvancedSecurityTier","spawnImmigrantAdvancedSecurityTier","spawnImmigrantAdvancedSecurityTier",
#endregion

#region Spawn otherSite2 (random)                
                          "spawnOtherSite2",  //"spawnOtherSite1Allegiance1", "spawnOtherSite1Expedition1",                                
                          "spawnPlaySiteSite2Route",
#endregion

                  #region pier spots
                        "startPierSpot1", "startPierSpot2","startPierSpot3",
                  #endregion

                  #region start people, dog, robot spawn ..........moved to options

#endregion 

                  #region particle effects                  
                        "smallFog1", "smallFog2", "smallFog3","smallFog4", "smallFog5","smallFog6",
                        "sulphurousSmoke1", "sulphurousSmoke2","sulphurousSmoke3","sulphurousSmoke4", "haze1","haze2","haze3", "haze4","haze5","haze6",
                         "fog1", "fog2","fog3","fog4","fog5","fog6","fog7","fog8",
            #endregion

                    },

         ////////////////////////////////

                    #region Difficulties
                    MainDifficultySettings = new[]
                    {
                        
                        new Difficulty()
                        {
                            KeyName = "easy",
                            Name = "Easy",
Description ="On EASY difficulty, you will start with an expedition equipped with: \nVERSATILE TOOLS and MANY PROVISIONS \n \nHINT: \nMost of the colony members have low principles, which means that they are content with little and accept bad conditions. It also means that you need to attract immigrants with higher principles if you want to advance your colony's technology.",
                            OptionsToUse = new SerializableDictionary<string, string[]>()
                            {
                         //       { "startingLocations", new []{} },
                          //       { "people", new []{"6people1dog1robot"} },                         
                                  { "expeditionType", new []{"versatileExpedition"} },
                                { "fauna" , new []{"noBushDragons"} },
                                { "resources" , new []{"average"} },
                            //    { "otherSite1Allegiance1Relation", new []{} }
                            }
                         },
                          new Difficulty()
                         {
                              KeyName = "normal",
                              Name = "Normal",
                               Description = "On NORMAL difficulty, you will start with a random expedition, situated and equipped for either: \n \nFARMING,    FISHING   or   HUNTING \n(These can also be individually selected under CUSTOM) \n \nHINT: \nMost of the colony members have low principles, which means that they are content with little and accept bad conditions. It also means that you need to attract immigrants with higher principles if you want to advance your colony's technology.", 
                              IsDefault = true,
                              OptionsToUse = new SerializableDictionary<string, string[]>()
                              {
                                //    { "startingLocations", new []{"riverBank", "northArableLand"} },
                           //         { "people", new []{"6people1dog1robot"} }, //
                                    { "expeditionType", new []{"farmingExpedition", "huntingExpedition", "fishingExpedition"} },
                                    { "fauna" , new []{"average"} },
                                    { "resources" , new []{"average"} }, //
                                    //{ "otherSite1Allegiance1Relation", new []{} }
                              },
                         },
                          new Difficulty()
                         {
                              KeyName = "hard",
                              Name = "Hard", 
                       Description = "On HARD difficulty, you will start with an expedition SPARSELY equipped with: \nBUSHCRAFT TOOLS. \n \nHINT: \nMost of the colony members have low principles, which means that they are content with little and accept bad conditions. It also means that you need to attract immigrants with higher principles if you want to advance your colony's technology.",
                              IsDefault = false,
                                OptionsToUse = new SerializableDictionary<string, string[]>()
                                {
                                    
                              //      { "people", new []{"dozensPeople2dogs"} }, 
                                    { "expeditionType", new []{"bushcraftExpedition"} },
                                    { "fauna" , new []{"average"} },
                                    { "resources" , new []{"average"} },
                                    //{ "otherSite1Allegiance1Relation", new []{} }
                                }
                         }
                         
         
                         /*,
                         new Difficulty()
                         {
                                KeyName = "veryHard",
                                Name = "Very hard" ,
                                OptionsToUse = new SerializableDictionary<string, string[]>()
                                {
                                    { "startingLocations", new []{} },
                                    { "people", new []{} }, 
                                    { "equipment", new []{} },
                                    { "fauna" , new []{} },
                                    { "resources" , new []{} },
                                    { "otherSite1Allegiance1Relation", new []{} }   
                                }                            
                         },
                         */
                     },
                    #endregion

                    #region OptionSets Fauna, People, Loadout, Resources, Relation

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
                                    KeyName = "noBushDragons",
                                    Name = "No bush dragons",                                
                                    Difficulty = new CustomDifficulty()
                                    {
                                        KeyName = "easy",
                                        Name = "No bush dragons", //shown on CUSTOM selector
                                        IsDefault = false,   
                                        ScoreModifier = 1f
                                    },
                                    ConditionalEvents = new[]{"SANDBOXNOMADMAP_timedSpawnBeginningPopulationNormal", },
                                    ActionKeys = new[]
                                    {//only difference is the bush dragons have been removed on easy:
                                    //    "setMaxBushDragonsNormal",      "setSpawnIntervalBushDragonsNormal",

                                        "setMaxLeafcuttersNormal",      "setSpawnIntervalLeafcutterNormal",
                                        "setMaxBinalRatsNormal",        "setSpawnIntervalBinalRatsNormal",
                                        "setMaxSlugsNormal",            "setSpawnIntervalSlugsNormal",
                                        "setMaxSnatcherNormal",         "setSpawnIntervalSnatchersNormal",
                                        "setMaxThunderChickensNormal",  "setSpawnIntervalThunderChickensNormal",
                                        "setMaxTurnipsNormal",          "setSpawnIntervalTurnipsNormal",
                                        "setMaxDemonTreesNormal",       "setSpawnIntervalDemonTreeNormal",
                                        //migrating animals:
                                        "setMaxLesserWhipjawNormal",    "setSpawnIntervalLesserWhipjawMigration",
                                        "setMaxBajinganNormal",         "setSpawnIntervalBajinganMigration",


                                    }
                                },

                                new Option()
                                {
                                    KeyName = "average",
                                    Name = "Average",                                
                                    Difficulty = new CustomDifficulty()
                                    {
                                        KeyName = "normal",
                                        Name = "Bush dragons", //shown on CUSTOM selector. no room for longer sentence..
                                        IsDefault = true,   
                                        ScoreModifier = 1f
                                    },
                                    ConditionalEvents = new[]{"SANDBOXNOMADMAP_timedSpawnBeginningPopulationNormal", "SANDBOXNOMADMAP_timedSpawnBeginningPopulationBushdragonsNormal",  "SANDBOXNOMADMAP_continualSpawnBushDragonSouth"},
                                    ActionKeys = new[]
                                    {//only difference is the bush dragons have been removed on easy:
                                        "setMaxBushDragonsNormal",      "setSpawnIntervalBushDragonsNormal",

                                        "setMaxLeafcuttersNormal",      "setSpawnIntervalLeafcutterNormal",
                                        "setMaxBinalRatsNormal",        "setSpawnIntervalBinalRatsNormal",
                                        "setMaxSlugsNormal",            "setSpawnIntervalSlugsNormal",
                                        "setMaxSnatcherNormal",         "setSpawnIntervalSnatchersNormal",
                                        "setMaxThunderChickensNormal",  "setSpawnIntervalThunderChickensNormal",
                                        "setMaxTurnipsNormal",          "setSpawnIntervalTurnipsNormal",
                                        "setMaxDemonTreesNormal",       "setSpawnIntervalDemonTreeNormal",
                                        //migrating animals:
                                        "setMaxLesserWhipjawNormal",    "setSpawnIntervalLesserWhipjawMigration",
                                        "setMaxBajinganNormal",         "setSpawnIntervalBajinganMigration",


                                    }
                                },
            /*                    new Option()
                                {
                                    KeyName = "hard",
                                    Name = "Hard",                                
                                    Difficulty = new CustomDifficulty()
                                    {
                                        KeyName = "hard",
                                        Name = "Hard",
                                        IsDefault = true,   
                                        ScoreModifier = 1f
                                    },
                                    //ConditionalEvents = new[]{},
                                    //ActionKeys = new[]{""}      
                                },
*/

                            }
                        },
#endregion
                     
                    #region People options
           /*         new OptionSet()
                    {
                        KeyName = "people",
                        Name = "Group members",
                        DisplayGroup = 1,
                        Options = new[] 
                        { 
           */

                     /*      new Option()
                            {
                                KeyName = "7people",
                                Name = "7 people",
                                ShortDescription = "Group of 7 people",
                                Difficulty = new CustomDifficulty()
                                {
                                    KeyName = "easy",
                                    IsDefault = true,
                                    Name = "7 people",
                                    ScoreModifier = 1f
                                },
                                ActionKeys = new[]{  "spawnPerson1", "spawnPerson2", "spawnPerson3", "spawnPerson4", "spawnPerson5", "spawnPerson6", "spawnPerson7"}                                     
                            },
                     */
          /*                  new Option()
                            {
                                KeyName = "6people1dog1robot",
                                Name = "6 people+dog+robot",
                                ShortDescription = "Small group",
                                Difficulty = new CustomDifficulty()
                                {
                                    KeyName = "normal",                                 
                                    Name = "Small group",
                                    IsDefault=true,
                                    ScoreModifier = 1f
                                }, //person 2 and 3 are mentioned in story
                                ActionKeys = new[]{  "spawnCastor", "spawnLinsey", "spawnPerson4", "spawnPerson5", "spawnPerson6","spawnPerson7","spawnDog","spawnHaulRobot"}                                     
                            },*/

                     /*       new Option()
                            {
                                KeyName = "dozensPeople2dogs",
                                Name = "Dozens of people+2 dogs",
                                ShortDescription = "Dozens of people",
                                Difficulty = new CustomDifficulty()
                                {
                                    KeyName = "hard",                                 
                                    Name = "Dozens of people",                                    
                                    ScoreModifier = 1f
                                }, //2 persons 2 are mentioned in story
                                ActionKeys = new[]{  "spawnCastor", "spawnLinsey", "spawnDog", "spawnDog",
                                    "spawnPerson1",  "spawnPerson8", "spawnPerson9", "spawnPerson10", "spawnPerson11", "spawnPerson12","spawnPerson13",
                                    "spawnPerson14", "spawnPerson15", "spawnPerson16", "spawnPerson17", "spawnPerson18","spawnPerson19",
                                    "spawnPerson20", }                                     
                            }*/

         /*               }
                    },
          * */
#endregion
 
                    #region Expedition type options
                          new OptionSet(){
                               KeyName = "expeditionType",
                               Name = "Expedition type",
                                DisplayGroup = 0,
                                Options = new[] { 

                                    new Option()
                                    {
                                      KeyName = "versatileExpedition",
                                      Name = "N/A", //not shown when only one in this difficulty category 
                                      Difficulty = new CustomDifficulty()
                                      {
                                           KeyName = "easy",
                                           Name = "Versatile", 
                                           ScoreModifier = 1f
                                      },  
                                      ConditionalEvents = new[]{"introDialogueVersatile", "introDialogueScreen"}, //INTRO DIALOGUE AND SCREEN
                                       ActionKeys = new[]
                                       {                                             
                                           //STARTING LOCATION:
                                           "setStartingLocationSouthEastRockyFiregrass", "exploreShroudSouthEastRockyFiregrass",  

                                          //natural terminal to wilderness
                                            "startNaturalTerminalGenericPosition", 

                                           //PEOPLE: //2 persons are mentioned in story.  // specialist1 is survivaltier personality. specialist2 is basictier personality
                                           "spawnCastor", "spawnLinsey", "spawnFarmingSpecialist1", "spawnHuntingSpecialist1", "spawnSmithingSpecialist1","spawnConstructionSpecialist2","spawnDog","spawnHaulRobot",
                                           
                                          

                                          /////LOADOUT:
                                          //general tools/weapons:
                                          "startRadioAntenna", "startRadio",
                                             "startString","startString","startKnife","startKnife", "startMachete","startMachete",
                                           "startGunpowderRifle","startGunpowderAmmo","startGunpowderAmmo", "startBoltActionRifle","startBoltActionAmmo",
                                        //   Metalworking:
                                           "startHammer","startWroughtIron", "startMetalworkersToolbox", "startAnvil", "startBarClamps", "startBellows",
                                // equipment:                         
                                           "startGlassyCreeper", "startGlassyCreeper", "startGlassyCreeper", "startGlassyCreeper", "startCrystalBerries","startCrystalBerries",
                                           "startHoe", "startPickaxe", "startBrickMold", "startSpade", 
                                    //Cooking/food:
                                          "startCookingPot", "startRefrigerator", "startClayJar", "startClayJar", "startVinegar", "startSalt",
                                          "startTurnipSalami","startTurnipSalami","startTurnipSalami","startDriedSaltedStreakFin", "startDriedSaltedStreakFin", "startDriedSaltedStreakFin","startDriedSaltedStreakFin", "startHardtack","startHardtack", "startHardtack","startHardtack","startHardtack","startHardtack", "startHardtack", "startHardtack",


                                       }
                                    },


                                    new Option()
                                    {
                                      KeyName = "farmingExpedition",
                                      Name = "Farming",
                                      Difficulty = new CustomDifficulty()
                                      {
                                           KeyName = "normal",
                                           Name = "Specialized", //was "Well equipped"
                                           ScoreModifier = 1f
                                      },  
                                      ConditionalEvents = new[]{"introDialogueFarming", "introDialogueScreen"}, //INTRO DIALOGUE AND SCREEN
                                       ActionKeys = new[]
                                       { //STARTING LOCATION:
                                           "setStartingLocationNorthArableLand", "exploreShroudNorthArableLand",

                                          //natural terminal to wilderness
                                            "startNaturalTerminalGenericPosition",                                            

                                        //PEOPLE: //2 persons are mentioned in story
                                           "spawnCastor", "spawnLinsey", "spawnFarmingSpecialist1", "spawnFarmingSpecialist1", "spawnConstructionSpecialist1","spawnCookingSpecialist2","spawnDog","spawnHaulRobot",
    
                                       ///***//Test items, REMOVE!!*************
                                //       "startPickaxe", "startSpade", "startAnvil", "startBlowpipe", "startBlackpowder","startBlackpowder",
                                //       "startMudbrick","startMudbrick","startMudbrick","startMudbrick","startMudbrick","startMudbrick",
                                //       "startCharcoal","startCharcoal","startWetFirewood","startWetFirewood","startWetFirewood","startWetFirewood","startWetFirewood","startWetFirewood",
                                //       "startGoldOre", "startBogOre", "startBogOre", "startCleanTurnipGuts","startCleanTurnipGuts","startCleanTurnipGuts","startCleanTurnipGuts","startCleanTurnipGuts",
                                //       "startFingerFruit", "startFingerFruit", "startWroughtIron", "startWroughtIron","startWroughtIron",
                                       ////**************************************

                                          /////LOADOUT:
                                          //general tools/weapons:
                                          "startRadioAntenna", "startRadio",
                                             "startString","startKnife","startKnife", "startMachete","startMachete",
                                           "startGunpowderRifle","startGunpowderAmmo","startGunpowderAmmo",
                                        //   Metalworking:
                                       //    "startHammer","startWroughtIron", "startBlisterSteel", "startBlisterSteel",
                                //Farming equipment:                         
                                           "startGlassyCreeper", "startGlassyCreeper", "startGlassyCreeper", "startGlassyCreeper",
                                           "startHoe", "startHoe",
                                    //Cooking/food:
                                          "startGoldPot",  //
                                          "startJerky","startJerky","startJerky","startJerky", "startJerky","startJerky","startJerky",
                                          "startHardtack","startHardtack", "startHardtack","startHardtack","startHardtack","startHardtack", "startHardtack", "startHardtack",


                                       }
                                    },

                                    new Option()
                                    {
                                      KeyName = "huntingExpedition",
                                      Name = "Hunting",
                                      Difficulty = new CustomDifficulty()
                                      {
                                           KeyName = "normal",
                                           Name = "Specialized",
                                           ScoreModifier = 1f
                                      },  
                                       ConditionalEvents = new[]{"introDialogueHunting", "introDialogueScreen"}, //INTRO DIALOGUE AND SCREEN
                                       ActionKeys = new[]
                                       {  //STARTING LOCATION:
                                           "setStartingLocationNorthMuddyCreek", "exploreShroudNorthArableLand",

                                          //natural terminal to wilderness
                                            "startNaturalTerminalGenericPosition", 

                                        //PEOPLE: //2 persons are mentioned in story
                                        "spawnCastor", "spawnLinsey", "spawnHuntingSpecialist1", "spawnHuntingSpecialist1", "spawnSecuritySpecialist1","spawnCookingSpecialist2","spawnDog","spawnDog",

                                           /////LOADOUT:
                                          //general tools/weapons:
                                          "startRadioAntenna", "startRadio",
                                           "startString","startKnife","startKnife","startMachete",
                                           "startGunpowderRifle","startGunpowderAmmo",
                                        //   Metalworking:
                                        //   "startHammer", "startWroughtIron", "startBlisterSteel", "startBlisterSteel",
                                           
                                        //hunting weapons + a tool for cracking open the turnip
                                            "startBoltActionRifle", "startBoltActionRifle","startBoltActionAmmo","startBoltActionAmmo","startBoltActionAmmo",
                                            "startSpikeTrap", "startSpikeTrap",  "startTurnipCracker",
                                    //Cooking/food:
                                           "startCookingPot", //moved "startRefrigerator" to Easy. to focus on preservation
                                           "startJerky","startJerky","startJerky","startJerky", "startJerky","startJerky","startJerky",
                                           "startHardtack","startHardtack", "startHardtack","startHardtack","startHardtack","startHardtack", "startHardtack", "startHardtack",
                                            
                                           
                                       }
                                    },

                                   new Option()
                                    {
                                      KeyName = "fishingExpedition",
                                      Name = "Fishing",
                                      Difficulty = new CustomDifficulty()
                                      {
                                           KeyName = "normal",
                                           Name = "Specialized",
                                           ScoreModifier = 1f
                                      },    
                                      ConditionalEvents = new[]{"introDialogueFishing", "introDialogueScreen"}, //INTRO DIALOGUE AND SCREEN
                                       ActionKeys = new[]
                                       {  //STARTING LOCATION:
                                           "setStartingLocationRiverBank", "exploreShroudRiverBank",
                                        //natural terminal to wilderness
                                        "startNaturalTerminalFishingPosition", //different because of geography

                                        //PEOPLE: //2 persons are mentioned in story
                                       "spawnCastor", "spawnLinsey", "spawnHuntingSpecialist1", "spawnBushcraftSpecialist1", "spawnSecuritySpecialist1","spawnCookingSpecialist2","spawnDog", "spawnHaulRobot",

                                           /////LOADOUT:
                                          //general tools/weapons:
                                          "startRadioAntenna", "startRadio",
                                           "startString", "startString","startString", "startKnife","startKnife","startKnife", "startIronHandAxe",
                                           "startGunpowderRifle","startGunpowderAmmo","startGunpowderAmmo", "startImprovisedBow", "startIronArrow",


                                            //fishing equipment:
                                            "startFishTrapHoopNet",  "startCottonString","startCottonString",// 2 cotton string for making 2 fishing nets
                                            "startString", "startIronHooks", "startIronHooks", "startNeonHornetsLive","startPigFliesLive", "startBugNet", "startIronSpear", 
                                       
                                            "startPickaxe", //MP i'm only giving them a pickaxe to dig resource pits and favorbread but prevent the fishers from starting the farm plot (requires spade/hoe) before they've traded. they have a lot of fish.
                                       //   "startSteelSpade",

                                    //Cooking/food:
                                           "startImprovisedCookingPot", //moved "startRefrigerator" to Easy. to focus on preservation
                                            "startJerky","startJerky","startJerky", "startJerky", "startJerky","startJerky","startJerky", 
                                            "startHardtack","startHardtack", "startHardtack","startHardtack","startHardtack","startHardtack", "startHardtack", "startHardtack",

                                       }
                                    },

                                    new Option()
                                    { //mp no longer a throng
                                      KeyName = "bushcraftExpedition",
                                      Name = "Bushcraft", //not shown when only one in this difficulty category
                                      Difficulty = new CustomDifficulty()
                                      {
                                           KeyName = "normal",
                                           Name = "Specialized", // 
                                           ScoreModifier = 1f
                                      },  
                                      ConditionalEvents = new[]{"introDialogueThrong", "introDialogueScreen"}, //INTRO DIALOGUE AND SCREEN
                                       ActionKeys = new[]
                                       { //STARTING LOCATION:
                                           "setStartingLocationNorthArableLand", "exploreShroudNorthArableLand",
                                          
                                            //natural terminal to wilderness
                                            "startNaturalTerminalGenericPosition", 
    
                                        //PEOPLE: //2 persons are mentioned in story
                                       "spawnCastor", "spawnLinsey",  "spawnHuntingSpecialist1", "spawnBushcraftSpecialist1", "spawnMenialSpecialist1","spawnSecuritySpecialist2","spawnDog",

                                          /////LOADOUT:
                                          //general tools/weapons:
                                          "startRadioAntenna", "startRadio",
                                             "startMetalWire", "startString", "startShadeleafResin", "startRawhideString","startRawhideString","startRawhideString", "startKnife","startKnife","startKnife", "startFlintKnife", "startIronHandAxe","startMachete",
                                           "startGunpowderRifle","startGunpowderAmmo","startGunpowderAmmo",
                                        //   Metalworking:
                                       //    "startHammer","startWroughtIron", "startBlisterSteel", //hmm maybe remove metal materials? too easy on custom?
                                //Farming equipment:                         
                                          "startSteelSpade", 
                                           
                                    //Cooking/food:
                                          "startGoldPot", "startClayPotUnglazed", //
                                          "startHardtack", 


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

                                    //mp I'm now using the same spot spawns as in "Plenty":
                                    "startFarmSpotSmall1", "startFarmSpotSmall2",  "startFarmSpotSmall4", "startFarmSpotSmall5",  "startFarmSpotSmall7", "startFarmSpotSmall8", "startFarmSpotSmall9",  "startFarmSpotSmall11", "startFarmSpotSmall12", "startFarmSpotSmall13", "startFarmSpotSmall14", "startFarmSpotSmall15", "startFarmSpotSmall16",
                                    "startFarmSpotLarge1", "startFarmSpotLarge2",  "startFarmSpotLarge4", "startFarmSpotLarge5",
                                    "startFishTrapCreek1", "startFishTrapCreek2","startFishTrapCreek3", "startFishTrapCreek4", "startFishTrapCreek5", "startFishTrapCreek6", "startFishTrapCreek7", "startFishTrapCreek8",                                  
                                    "startFishTrapCoast1", "startFishTrapCoast2","startFishTrapCoast3","startFishTrapCoast4",
                                    "startFishTrapShore1", "startFishTrapShore2", "startFishTrapShore3", "startFishTrapShore4", // terrain changed: "startFishTrapShore5", 
                                    "startFishTrapShore6", "startFishTrapShore7", "startFishTrapShore8",  "startFishTrapShore10", "startFishTrapShore11", "startFishTrapShore12", "startFishTrapShore13", "startFishTrapShore14", "startFishTrapShore15", "startFishTrapShore16" , "startFishTrapShore17", "startFishTrapShore18", "startFishTrapShore19", "startFishTrapShore20" ,
                                  //  "startFishTrapShore9", removed because too close to fish weir
 
                                    "startBogOreDeposit1","startBogOreDeposit2",
                                    "startBogOreDeposit3",//only for large map

                                    "startPeatDeposit1","startPeatDeposit2","startPeatDeposit3",
                                    "startPeatDeposit4", //only for large map

                                    "startSaltDeposit1",
                                    "startSaltDeposit2",//only for large map

                                    "startClayDeposit1","startClayDeposit2","startClayDeposit3","startClayDeposit4",
                                    "startClayDeposit5",//only for large map


                                },


                            
                            },
            
/////////////////////////////////// farmplots not currently used: "startFarmSpotSmall3", "startFarmSpotSmall6", "startFarmSpotSmall10",


        /*                   new Option()
                            {
                                KeyName = "plenty", //MP: confirm that this has been fixed by Lars: MP I commented this out, because I didn't want it, but it resulted in no farmplot/fishspot spawns at all on the "average"setting coming from CUSTOM..but using NORMAL, they spawn fine. BUG!!!
                                Name = "Plenty",
                                Difficulty = new CustomDifficulty()
                                {
                                    KeyName = "easy",
                                    Name = "Plenty",
                                    IsDefault = false,
                                    ScoreModifier = 1f
                                },
                                ActionKeys = new[]
                                { 
                                    "setFishSchoolMedium", "setNormalResources",

                                    "setFishSchoolMedium", "setPlentyResources", 
                                    "startFarmSpotSmall1", "startFarmSpotSmall2", "startFarmSpotSmall3", "startFarmSpotSmall4", "startFarmSpotSmall5", "startFarmSpotSmall6", "startFarmSpotSmall7", "startFarmSpotSmall8", "startFarmSpotSmall9", "startFarmSpotSmall10", "startFarmSpotSmall11", "startFarmSpotSmall12", "startFarmSpotSmall13", "startFarmSpotSmall14",
                                    "startFarmSpotLarge1", "startFarmSpotLarge2", "startFarmSpotLarge3", "startFarmSpotLarge4",
                                    "startFishTrapCreek1", "startFishTrapCreek2","startFishTrapCreek3", "startFishTrapCreek4", "startFishTrapCreek5", "startFishTrapCreek6", "startFishTrapCreek7", "startFishTrapCreek8",                                  
                                    "startFishTrapCoast1", "startFishTrapCoast2",
                                    "startFishTrapShore1", "startFishTrapShore2", "startFishTrapShore3", "startFishTrapShore4",  "startFishTrapShore6", "startFishTrapShore7", "startFishTrapShore8", "startFishTrapShore9", "startFishTrapShore10", "startFishTrapShore11", "startFishTrapShore12", "startFishTrapShore13", "startFishTrapShore14", "startFishTrapShore15", "startFishTrapShore16" , "startFishTrapShore17", "startFishTrapShore18", "startFishTrapShore19"
                                }                                   
                            },    */  
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
                    #endregion
                };

            return scenarioData;
        }
    } 
}
