using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Scenarios;
using UWGame.SimSide.InGameEvents.Actions;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.AllGameData.Scenarios.Scenario_5.Data;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_5 // I chose a generic name so scenario name changes don't affect us so much...
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
    public class Scenario5Loader : ScenarioLoader // I chose a generic name so scenario name changes don't affect us so much...
    {
        //private const string folderName = ;


        public override string FolderName
        {
            get { return "Making Headway"; }
        }

        protected override Scenario InitScenarioHeader()
        {
            Scenario scenario;

            scenario = new Scenario()
            {

                Name = FolderName,
                DisplayName = "Making Headway",
                TimeDateYear = new DateAndTime.TimeDateYear() { Year = 180, Day = 5, TimeOfDay = 0.50 }, //match with music track list. and match date/day with event screen texts
                MapKey = "l Fjord",
                MapSize = SimSide.Scenarios.MapSize.Small,
                Allow32Bit = true,
                SummaryDescription = "TUTORIAL 3 + OPEN-ENDED: A farm town is in decline when prices for farm produce go down. They make a plan to start manufacturing rubber with the help from chemists. \nERA: The Great Descent",
                Description = "TUTORIAL NO. 3 + OPEN-ENDED \n \nThe two friends nodded at each other as they crossed paths. They looked around their village. It had seen better days. \n-So, another farmer left this morning. \n-Can't blame him. Not much going on here anymore. \n-This place is dying! Why are people so set in their ways? \n-I think they want to improve this place. They just can't agree how. You have an idea? \n-Yes. We should do something useful with that swamp in our backyard. \n-Aaah..It's that rubber tapping you talked about years ago. \n-Yeah. Now is the time. Demand is up. \n- Hm. Ever since what happened...we always stayed away from the swamp. Besides, what do we know about producing rubber? \n-Well, I have a plan. I'm gonna make a case for it at the town meeting.",
                ThumbnailImage = "Scenarios/Scenario 5/Scenario Screen/parasolHouse_scenario_thumb", //
                Image = "Farming",
                IsInDevelopment = false,
                SortOrder = 2,
            };


            return scenario;
        }

        public override DataLoader GetDataLoader()
        {
            DataLoader dataLoader = new Scenario5DataLoader();
            dataLoader.FolderName = FolderName;

            return dataLoader;
        }


        protected override ScenarioData InitScenarioData()
        {


            ScenarioData scenarioData = null;


            scenarioData = new ScenarioData()
            {
                LoadingBackgroundImage = "Scenarios/Scenario 4/Scenario Screen/TitleImgManFence_1920px",

                LoadingDialogText = "Headway - Year 180 \n \nThe annual town meeting used to be a crowded affair, but these days, there was plenty of space for everyone. The old rules of order felt too formal for such a small group and the moderator tried to loosen up the meeting. \n-So. We all agree to try something new. We need to change tack if we want to reach the conditions they have up in Eden Plains. Now, we've heard Tereza's idea to set up a distillery and sell crystal brandy. John, you're next. \n-We should start tapping sap in the swamp...make rubber components. The mining operations down in Zenig Station are booming and they need this stuff for their machines. \nAt the mentioning of the word 'swamp', several people mumbled their disapproval. Though John Millet was ready to counter their arguments, the townspeople were divided when the meeting ended. His plan for rubber production was still up for debate.",

                LoadingDialogImage = "IndoorMeeting",

                SpawnWorldAction = "spawnWorld",
                SpawnSiteAction = "spawnPlaySite",

                WorldMapImage = "Scenarios/Default/GUI/regionalMap_3",

                EnableMissions = true,

                //global events to register regardless of customization/difficulty:
                ConditionalEvents = new[]
                    { 

            #region music and game over
                   //this is an exact 2 day playlist: 
                   "SANDBOXNOMADMAP_musicTrackList",

                   "SANDBOXMAP_loseGame",
                   "winGame",
#endregion

                  #region animal spawns - moved to Population
                   // moved to fauna setting     "COVEMAP_timedSpawnBeginningPopulationNormal",

                   /*
                         "COVEMAP_continualSpawnSnatcher",                      

                        "COVEMAP_continualSpawnThunderChickens#2",

                        "COVEMAP_continualSpawnBinalRats#1",
                        "COVEMAP_continualSpawnBinalRats#2",
                        "COVEMAP_continualSpawnBinalRats#3",

                        "COVEMAP_continualSpawnTurnipsNorth",

                        "COVEMAP_continualSpawnSlugs",  
                      
                        "COVEMAP_continualSpawnLeafcutter#1",  

                        "COVEMAP_continualSpawnSwampDemonTree#2",
                   */

                        //nests:
                       // "COVEMAP_spawnFieldQuaditeNests",                       


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
                       
                    /*    "startPeat", "startPeat","startPeat",
                        "startRubber",
                        "startBeds","startBeds","startBeds","startBeds","startBeds","startBeds","startBeds","startBeds","startBeds","startBeds","startBeds","startBeds","startBeds","startBeds","startBeds","startBeds",
                        "startBeds","startBeds","startBeds","startBeds","startBeds","startBeds","startBeds","startBeds","startBeds","startBeds","startBeds","startBeds","startBeds","startBeds","startBeds","startBeds",
                        "startTextile","startTextile","startTextile","startTextile","startTextile","startTextile","startTextile","startTextile","startTextile","startTextile","startTextile","startTextile","startTextile","startTextile","startTextile",
                         "startBeds","startBeds","startBeds","startBeds","startBeds","startBeds","startBeds","startBeds","startBeds","startBeds","startBeds","startBeds","startBeds","startBeds","startBeds","startBeds",
                        "startTextile","startTextile","startTextile","startTextile","startTextile","startTextile","startTextile","startTextile","startTextile","startTextile","startTextile","startTextile","startTextile","startTextile","startTextile",
                        
                        "startTextile","startTextile","startTextile","startTextile","startTextile","startTextile","startTextile","startTextile","startTextile","startTextile","startTextile","startTextile","startTextile","startTextile","startTextile",
                        "startCoilRifle","startCoilRifle","startCoilRifle","startCoilRifle","startCoilRifle","startCoilRifle","startCoilRifle","startCoilRifle","startCoilRifle","startCoilRifle","startCoilRifle","startCoilRifle","startCoilRifle","startCoilRifle","startCoilRifle","startCoilRifle","startCoilRifle","startCoilRifle","startCoilRifle","startCoilRifle","startCoilRifle","startCoilRifle","startCoilRifle","startCoilRifle","startCoilRifle",
                        "startCoilRifleAmmo","startCoilRifleAmmo","startCoilRifleAmmo","startCoilRifleAmmo","startCoilRifleAmmo","startCoilRifleAmmo","startCoilRifleAmmo","startCoilRifleAmmo","startCoilRifleAmmo","startCoilRifleAmmo","startCoilRifleAmmo","startCoilRifleAmmo","startCoilRifleAmmo","startCoilRifleAmmo","startCoilRifleAmmo","startCoilRifleAmmo","startCoilRifleAmmo","startCoilRifleAmmo","startCoilRifleAmmo","startCoilRifleAmmo","startCoilRifleAmmo","startCoilRifleAmmo",
                        "startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky",
                        "startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky",
                        "startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky",
                        "startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky",
                        "startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky",
                        "startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky",
                        "startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky",
                        "startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky",
                        "startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky",
                        "startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky",
                        "startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky",
                        "startStonesTest","startStonesTest","startStonesTest","startStonesTest","startStonesTest","startStonesTest","startStonesTest","startStonesTest",
                        "startSpoakTest","startSpoakTest","startSpoakTest","startSpoakTest","startSpoakTest","startSpoakTest","startSpoakTest","startSpoakTest","startSpoakTest",
                        "startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky",
                        "startMudBricks","startMudBricks","startMudBricks","startMudBricks","startMudBricks","startMudBricks","startMudBricks","startMudBricks",
                        "startMudBricks","startMudBricks","startMudBricks","startMudBricks","startMudBricks","startMudBricks","startMudBricks","startMudBricks",
                        "startMudBricks","startMudBricks","startMudBricks","startMudBricks","startMudBricks","startMudBricks","startMudBricks","startMudBricks",
                        "startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky","startJerky",*/
/*"startPlasticWorkshop", "startStill",
"spawnChemistTest", "spawnFarmingSpecialist2","spawnFarmingSpecialist2","spawnFarmingSpecialist2","spawnFarmingSpecialist2","spawnCookingSpecialist1",
                                       "startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail",
                                       "startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail",
                                       "startCharcoal","startCharcoal","startFirewoodInWoodpile","startFirewoodInWoodpile","startFirewoodInWoodpile","startFirewoodInWoodpile","startFirewoodInWoodpile","startFirewoodInWoodpile",
                                      "startGoldOre", "startBogOre", "startBogOre", //"startCleanTurnipGuts","startCleanTurnipGuts","startCleanTurnipGuts","startCleanTurnipGuts","startCleanTurnipGuts",
                                       "startWroughtIron", "startWroughtIron","startWroughtIron",*/
                        
                             /*        "startWaterCaneStem", "startWaterCaneStem", "startWaterCaneStem",
                                     "startStreakFin","startStreakFin","startStreakFin","startStreakFin","startStreakFin","startStreakFin","startStreakFin","startStreakFin","startStreakFin","startStreakFin","startStreakFin","startStreakFin","startStreakFin","startStreakFin",
                                       "startStreakFin","startStreakFin","startStreakFin","startStreakFin","startStreakFin","startStreakFin","startStreakFin","startStreakFin","startStreakFin","startStreakFin","startStreakFin","startStreakFin","startStreakFin","startStreakFin",
                                     "startSolidMudBrick", "startSolidMudBrick","startSolidMudBrick","startSolidMudBrick","startSolidMudBrick","startSolidMudBrick","startSolidMudBrick","startSolidMudBrick",
                                     "startShadeleafCanes","startShadeleafCanes","startShadeleafCanes","startShadeleafCanes", "startShadeleafCanes","startShadeleafCanes",
                                     "startSpoakShingles", "startSpoakShingles","startSpoakShingles", "startSpoakShingles","startSpoakShingles", "startSpoakShingles",*/
                      //  "startExtrusionMachineComponents",
                     //   "startWorkshopBuilding", //"startStill",
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


                        "startFirewoodInWoodpile", "startFirewoodInWoodpile", "startFirewoodInWoodpile",
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
                        "initGameOver", "initBurialText1", "initBurialText2", "initBurialText3", "initBurialText4", "initBurialText5", 
                        #endregion

                        #region goals
                        "initComfortTarget", "initFoodTarget", "initSecurityTarget",
                        #endregion

                        #region Group meetings
                            "initEnableGroupMeetings", "initTimeBeforeGroupMeeting",                        
                            "meetingEmigrateThreat", "meetingEmigrateThreatAllUnhappy",        
                            "meeting3Security",   "meeting3Food",  "meeting3Comfort",
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

                        "spawnLeafcutterExpedition#1", 
                        "spawnThunderChickenExpedition#1",
                        "spawnSnatcherExpedition#1",
                        "spawnSlugExpedition#1",
                        "spawnSwampDemonTreeExpedition#2",
                        
                        "spawnBirdExpedition#1",
                        "spawnBinalRatExpedition#1", "spawnBinalRatExpedition#2","spawnBinalRatExpedition#3", 
                        "spawnTurnipExpeditionNorth",


#endregion

                        "spawnPlayerAllegiance", "placeExpedition", "setView", "exploreEntireMap",
                        "setPlayerCredits",  
                        "startNaturalTerminal", //natural terminal leads to wilderness

                #region structures
                "startStructureGreenhouse",
                "startStructureCompostPit",
             //   "startStructureParasolHouse",
                "startStructureCookhouse",
                "startStructureToolshed",
                "startStructureFirewoodStack",

                "startStructureClayGranary",
                "startStructureMeatDryingRack",
           //     "startStructureMudBrickKitchen",

                "startStructureImprovisedWorkbench",
                "startStructureRadioHut",

                "startStructureCaneHut",
                "startStructureClayHut",

                "startStructureSimpleSmithy",
                "startStructureKiln",
                "startSmokeOven",
                "startStructureSimplePort",
           //     "startStructureKilnImprovisedSmall", //remove the small oven when we have communal kitchen.

               "startStructureFishTrapCoast2",//putting fishtrap on fish spot in the cove.

                "startStructureSmallPlot1",  "startStructureSmallPlot2", "startStructureSmallPlot3", "startStructureSmallPlot4", "startStructureLargePlot1",
                #endregion

#region stockpiles
                "startBricksStockpile", "startFirewoodStockpile", "startMaterialsStockpile",
#endregion

#region Upgrades
                "startUpgradeCookhouseStove",
                "startUpgradeSettingCookhouseStove",

                //mp doesnt seem to work properly:
                "startUpgradeCookhouseCommunityHall",
                "startUpgradeSettingCookhouseCommunityHall",

                "startUpgradeClayHut1Mats",// "startUpgradeCanopyMats", //, "startUpgradeClayHut1Stove",
                "startUpgradeSettingClayHut1Beds",// "startUpgradeSettingCanopyBeds",  //, "startUpgradeSettingClayHut1Stove",
#endregion
                  #region pier spot
                        "startPierSpot1", 
                  #endregion

                  #region start people, dog, robot spawn ..........moved to options

#endregion 

                        
                    #region Spawn wildernessSite1                
                          "spawnWildernessSite1",  "spawnWildernessSite1Allegiance1", "spawnWildernessSite1Expedition1",                                
                          "spawnPlaySiteWildernessSite1Route", 
                    #endregion

                    //NEIGHBOR: mining town/////////////////////////////
                    #region Spawn otherSite1                
                          "spawnOtherSite1",  "spawnOtherSite1Allegiance1", "spawnOtherSite1Expedition1",                                
                          "spawnPlaySiteSite1Route",
                    #endregion
                   

                    #region otherSite1Expedition1 ///migrants
                      
                          //has "survivalTierPersonality":
                       "spawnImmigrantOtherSite1SmithingSpecialist","spawnImmigrantOtherSite1SmithingSpecialist","spawnImmigrantOtherSite1SmithingSpecialist",
                       "spawnImmigrantOtherSite1MenialSpecialist","spawnImmigrantOtherSite1MenialSpecialist","spawnImmigrantOtherSite1MenialSpecialist","spawnImmigrantOtherSite1MenialSpecialist",

                       //is random: (..we could make a mining profile immigrant instead of random (smithing, mechanical...))
                       "spawnImmigrantOtherSite1Random","spawnImmigrantOtherSite1Random","spawnImmigrantOtherSite1Random","spawnImmigrantOtherSite1Random","spawnImmigrantOtherSite1Random","spawnImmigrantOtherSite1Random","spawnImmigrantOtherSite1Random","spawnImmigrantOtherSite1Random",
                    #endregion
                 



                //NEIGHBOR: farming town/////////////////////////***************************
                #region Spawn otherSite2                
                                          "spawnOtherSite2",  "spawnOtherSite2Allegiance1", "spawnOtherSite2Expedition1",                                
                                          "spawnPlaySiteSite2Route",
                #endregion
               

                #region otherSite2Expedition2 ///migrants
                                          //1 is early joiner, 2 are survival:
                                       "spawnImmigrantOtherSite2Chemist1","spawnImmigrantOtherSite2Chemist2","spawnImmigrantOtherSite2Chemist3",
                                       //are "survivalTierPersonality":
                                       "spawnImmigrantOtherSite2MenialSpecialist","spawnImmigrantOtherSite2MenialSpecialist","spawnImmigrantOtherSite2MenialSpecialist","spawnImmigrantOtherSite2MenialSpecialist",

                                       //are random: (...maybe make a farming town immigrant profile instead of random..)
                                       "spawnImmigrantOtherSite2Random","spawnImmigrantOtherSite2Random","spawnImmigrantOtherSite2Random","spawnImmigrantOtherSite2Random","spawnImmigrantOtherSite2Random","spawnImmigrantOtherSite2Random","spawnImmigrantOtherSite2Random","spawnImmigrantOtherSite2Random","spawnImmigrantOtherSite2Random","spawnImmigrantOtherSite2Random",
                #endregion

               
//////////////////////////////////////////


                  #region particle effects                  
                        "smallFog1", "smallFog2",
                         "fog1", "fog2","fog3","fog4", "fog5","fog6","fog7", "fog8","fog9","fog10", "fog11","fog12",
            #endregion

                    },

                ////////////////////////////////

                #region Difficulties
                MainDifficultySettings = new[]
                    {
                        

                          new Difficulty()
                         {
                              KeyName = "normal",
                              Name = "Normal",
                              Description ="Try to reach the objectives presented at the start of the game. \nSome tutorial windows will appear along the way, but following them is optional since the objective can be reached in many ways.",
                              IsDefault = true,
                              OptionsToUse = new SerializableDictionary<string, string[]>()
                              {
                                //    { "startingLocations", new []{"riverBank", "northArableLand"} },
                           //         { "people", new []{"6people1dog1robot"} }, //
                                    { "expeditionType", new []{"farmingExpedition"} },
                                  //  { "fauna" , new []{"average"} },
                                    { "resources" , new []{"average"} }, //
                                    //{ "otherSite1Allegiance1Relation", new []{} }
                              },
                         }, 

                     },
                #endregion

                #region OptionSets: Fauna, People, Equipment Loadout, Resources, Relation

                OptionSets = new[]
                    {
                        /*
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
                                    ConditionalEvents = new[]{"COVEMAP_timedSpawnBeginningPopulationNormal", },
                                    ActionKeys = new[]
                                    {

                                        "setMaxLeafcuttersNormal",      "setSpawnIntervalLeafcutterNormal",
                                        "setMaxBinalRatsNormal",        "setSpawnIntervalBinalRatsNormal",
                                       // "setMaxSlugsNormal",            //"setSpawnIntervalSlugsNormal",
                                        "setMaxSnatcherNormal",         "setSpawnIntervalSnatchersNormal",
                                        "setMaxThunderChickensNormal",  "setSpawnIntervalThunderChickensNormal",
                                        "setMaxTurnipsNormal",          "setSpawnIntervalTurnipsNormal",
                                        "setMaxDemonTreesNormal",       "setSpawnIntervalDemonTreeNormal",
                                        
                                    }
                                },


                            }
                        },
#endregion
                    */ 
 
 
                    #region Expedition type options. Intro dialogue, members, items.
                          new OptionSet(){
                               KeyName = "expeditionType",
                               Name = "Expedition type",
                                DisplayGroup = 0,
                                Options = new[] { 


                                    new Option()
                                    {
                                      KeyName = "farmingExpedition",
                                      Name = "Farming",
                                      Difficulty = new CustomDifficulty()
                                      {
                                           KeyName = "normal",
                                           Name = "Specialized", //not shown when only one in this difficulty category
                                           ScoreModifier = 1f
                                      },  
                                      ConditionalEvents = new[]{"introDialogue", //INTRO Banter
                                                                "introDialogueScreen",  // TUT screen #1 
                                                                "extrusionMachineArrivalCheck",  // TUT screen #2                                   
                                      },
                                       ActionKeys = new[]
                                       { //STARTING LOCATION:
                                           "setStartingLocation",
                                           
                                        //PEOPLE: //2 persons are mentioned in story
                                           "spawnMillet", "spawnRains", "spawnFarmingSpecialist1", "spawnFarmingSpecialist2", "spawnConstructionSpecialist1",//exit the cookhouse
                                           "spawnCookingSpecialist1", //a bit further away
                                           "spawnHaulRobot",
   
                                       /////Test items, REMOVE!!*************
                                     //  "spawnConstructionSpecialist1","spawnConstructionSpecialist1","spawnConstructionSpecialist1","spawnConstructionSpecialist1","spawnConstructionSpecialist1","spawnConstructionSpecialist1","spawnConstructionSpecialist1","spawnConstructionSpecialist1","spawnConstructionSpecialist1","spawnConstructionSpecialist1",
                                      // "startVarmintBomb",
                                     //  item:varmintBomb
/*
                                       "startBlunderbuss", "startBlackPowderShotAmmo",
                                 //      "startBoltActionRifle","startBoltActionAmmo",
                                  //     "startBoltActionRifle", "startBoltActionAmmo",

                                       "startGunpowderRifle","startGunpowderAmmo",
                                       "startBlunderbuss", "startBlackPowderShotAmmo",
                                       "startGunpowderRifle","startGunpowderAmmo",
                                       "startBlunderbuss", "startBlackPowderShotAmmo",
                                       "startGunpowderRifle","startGunpowderAmmo",
                                       "startBlunderbuss", "startBlackPowderShotAmmo",
                                       "startGunpowderRifle","startGunpowderAmmo",
                                       "startBlunderbuss", "startBlackPowderShotAmmo",
                                       "startGunpowderRifle","startGunpowderAmmo",
                                       "startBlunderbuss", "startBlackPowderShotAmmo",
                                       "startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail",
                                       "startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail",
                                       "startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail",
                                       "startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail",
                                       "startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail",
                                       "startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail",
                                       "startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail",
                                       "startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail","startSmokedCarbonTail",
                           */            //      "startBoltActionRifle","startBoltActionAmmo",
                                 //      "startBoltActionRifle", "startBoltActionAmmo",

                           //           "spawnChemistTest", "spawnFarmingSpecialist2","spawnFarmingSpecialist2","spawnFarmingSpecialist2","spawnFarmingSpecialist2","spawnCookingSpecialist1",
                              //      "startKnife","startKnife","startKnife","startKnife",
                                       //       "startPickaxe", "startSpade", "startAnvil", "startBlowpipe", "startBlackpowder","startBlackpowder",
                                //       
                                //       "startCharcoal","startCharcoal","startFirewoodInWoodpile","startFirewoodInWoodpile","startFirewoodInWoodpile","startFirewoodInWoodpile","startFirewoodInWoodpile","startFirewoodInWoodpile",
                                //       "startGoldOre", "startBogOre", "startBogOre", "startCleanTurnipGuts","startCleanTurnipGuts","startCleanTurnipGuts","startCleanTurnipGuts","startCleanTurnipGuts",
                                //       "startFingerFruit", "startFingerFruit", "startWroughtIron", "startWroughtIron","startWroughtIron",
                                       ////**************************************

                                          /////LOADOUT:
                                    //spawns in open:
                                    "startSolidMudBrick","startSolidMudBrick","startSolidMudBrick","startSolidMudBrick","startSolidMudBrick","startSolidMudBrick","startSolidMudBrick","startSolidMudBrick","startSolidMudBrick",
                                    "startFirewood","startFirewood","startFirewood","startFirewood","startFirewood","startFirewood","startFirewood","startFirewood","startFirewood","startFirewood",
                                    "startSpoakShingles","startSpoakShingles","startSpoakShingles",
                                    "startSticks","startSticks","startSticks","startSticks",
                                    "startStones","startStones","startStones",


                                    //spawns in woodpile (is instantly move out:
                                   // "startFirewoodInWoodpile","startFirewoodInWoodpile","startFirewoodInWoodpile","startFirewoodInWoodpile","startFirewoodInWoodpile",//"startFirewoodInWoodpile","startFirewoodInWoodpile","startFirewoodInWoodpile","startFirewoodInWoodpile","startFirewoodInWoodpile",
                                    //spawns in compost pit:........don't spawn organic matter, because it has variable bulk, leading to complications: "startOrganicMatter"
                                    "startRottenVegetables","startRottenVegetables","startRottenVegetables","startRottenVegetables","startRottenVegetables","startRottenVegetables","startRottenVegetables","startRottenVegetables","startRottenVegetables",
                                    //spawns in toolshed:
                                    "startSteelMachete", 
                                    "startHoe", "startHoe", 
                                    "startSteelSpade",
                                    "startPickaxe",
                                    "startIronHandAxe",
                                    "startTappingBucket",
                                    "startBugNet",
                                    "startIronHooks",
                                    "startBrickMold",
                                    "startImprovisedTrowel",
 
                                    //in granary:
                                    "startGlassyCreeper", "startGlassyCreeper", "startGlassyCreeper", "startGlassyCreeper", "startGlassyCreeper", "startGlassyCreeper", "startGlassyCreeper", "startGlassyCreeper","startGlassyCreeper", "startGlassyCreeper", "startGlassyCreeper", "startGlassyCreeper",
                                    "startGlassyCreeper", "startGlassyCreeper", "startGlassyCreeper", "startGlassyCreeper", "startGlassyCreeper", "startGlassyCreeper", "startGlassyCreeper", "startGlassyCreeper","startGlassyCreeper", "startGlassyCreeper", "startGlassyCreeper", "startGlassyCreeper",
                                    "startGlassyCreeper", "startGlassyCreeper", "startGlassyCreeper", 

                                    "startCrystalBerries", "startCrystalBerries", "startCrystalBerries", "startCrystalBerries", "startCrystalBerries", "startCrystalBerries", "startCrystalBerries", "startCrystalBerries", "startCrystalBerries", "startCrystalBerries", "startCrystalBerries", "startCrystalBerries",

                                    "startPickledCarbonTail","startPickledCarbonTail","startPickledCarbonTail","startPickledCarbonTail","startPickledCarbonTail","startPickledCarbonTail",
                                    "startDriedSaltedStreakFin","startDriedSaltedStreakFin","startDriedSaltedStreakFin",
                                    "startSmokedStreakFin","startSmokedStreakFin","startSmokedStreakFin","startSmokedStreakFin","startSmokedStreakFin","startSmokedStreakFin","startSmokedStreakFin","startSmokedStreakFin","startSmokedStreakFin",
                                    "startHardtack","startHardtack","startHardtack","startHardtack","startHardtack","startHardtack","startHardtack","startHardtack","startHardtack","startHardtack",

                                    //in workbench:
                                    "startKnife",
                                    "startKnife", 
                                    "startKnife",
                                    "startMetalWire",
                                    "startMetalWire",
                                    "startMarshcotSap",// MP I spawn 1 of this so that the tutorial is easier to write
                                    "startShadeleafResin",
                                    //in smithy:
                                    "startBlacksmithsToolbox", "startBellows", "startMetalWorkersToolbox",
                                    "startCharcoal", "startCharcoal",
                                    "startWroughtIron",
                                    "startBlisterSteel", "startBlisterSteel",
                                    //in cookhouse:
                                    "startGoldPot", "startKnifeInKitchen","startKnifeInKitchen",
                                    "startSalt","startSalt","startSalt","startSalt",
                                    "startClayJar", "startClayJar",
                                    "startVinegar", "startVinegar",                                   
                                           "startGunpowderRifle","startGunpowderAmmo","startGunpowderAmmo", "startImprovisedBow", "startIronArrow", 
                                           "startCrystalWine", "startCrystalWine","startCrystalWine", "startCrystalWine","startCrystalWine", "startCrystalWine",

                                     // upgrades:



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

                                    
                                    "startFarmSpotSmall1", "startFarmSpotSmall2", "startFarmSpotSmall3", "startFarmSpotSmall4",
                                    "startFarmSpotLarge1",  
                                    "startFarmSpotSmall5", "startFarmSpotSmall6", //not developed spots
                                    "startFishTrapCoast1", "startFishTrapCoast2", "startFishTrapCoast3",
                                    "startFishTrapShore1", "startFishTrapShore2", "startFishTrapShore3",

                                    "startClayDeposit1",
                                    "startBogOreDeposit1",
                                    "startPeatDeposit1",
                                    "startSaltDeposit1",
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
                            }
                #endregion
                }
                #endregion
            };

            return scenarioData;
        }
    }
}
