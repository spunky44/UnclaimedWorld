using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Scenarios;
using UWGame.SimSide.InGameEvents.Actions;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.AllGameData.Scenarios.Scenario_7.Data;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_7 // I chose a generic name so scenario name changes don't affect us so much...
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
    public class Scenario7Loader : ScenarioLoader // I chose a generic name so scenario name changes don't affect us so much...
    {
        //private const string folderName = ;


        public override string FolderName
        {
            get { return "Muckroot Mining Site"; }
        }

        protected override Scenario InitScenarioHeader()
        {
            Scenario scenario;

            scenario = new Scenario()
            {

                Name = FolderName,
                DisplayName = "Muckroot Mining Site",//"The Mining Camp"
                TimeDateYear = new DateAndTime.TimeDateYear() { Year = 21, Day = 3, TimeOfDay = 0.36 }, //match with music track list
                MapKey = "o Mountain Pass", //
                MapSize = SimSide.Scenarios.MapSize.Large,
                Allow32Bit = false,
                SummaryDescription = "OPEN-ENDED: A mining expedition extracts metals in a dangerous biome, using advanced equipment. The crew consider settling on the nearby grasslands. \nERA: The Great Descent",
                Description = "OPEN-ENDED MAP.\n \nSome decades after their disastrous arrival to Antheia, the pioneers have established a modest existence. \nThey are now further challenged by an approaching risk of solar flares. As protection, the pioneers have begun to construct large electromagnetic shields - a megaproject called CANOPY which requires huge amounts of resources. The project is managed by an A.I. overseer which commissions materials by setting prices and offering incentives. \n \nHowever, there's disagreement about the risk of solar eruptions. Therefore, many pioneers are reluctant to work for CANOPY and instead focus on building their own frontier settlements. Because of the restricted resources, these settlements use a mix of primitive and advanced technology.",
                ThumbnailImage = "Scenarios/Scenario 2/Scenario Screen/muckrootCamp_scenario_thumb", //
                Image = "Survival", 
                IsInDevelopment = false,
                SortOrder = 7,
            };


            return scenario;
        }

        public override DataLoader GetDataLoader()
        {
            DataLoader dataLoader = new Scenario7DataLoader();
            dataLoader.FolderName = FolderName;

            return dataLoader;
        }


        protected override ScenarioData InitScenarioData()
        {
            

            ScenarioData scenarioData = null;

            //mp what?:  this is a smaller version of the Fields map. the map edge stops at around 3800, 3800  or 79,79 (included)
                scenarioData = new ScenarioData()
                {
                    LoadingBackgroundImage = "Scenarios/Scenario 7/Scenario Screen/TitleImgTown1920",
                    //mp the place name has to work with both starting positions. was: BATTEN CREEK. Also work with the location name on map.
                    LoadingDialogText = "MUCKROOT PASS, YEAR 21 \n \n'Hey. See those? Behind that grey clump. There's more over there...' \nIrina looked out the window where Mike was pointing. \n'Woah, those creatures are fast...' \nThe Skimmer aircraft was approaching their destination. On the way in, they were flying above a 'muckroot' landscape: Grey-blue carpets of moss covered the ground between the hills. This was the territory of the ferocious Swarmer quadites. \nAll the miners in Irina's crew were staring at the animals below. Mike turned to Irina: \n'The terbium deposit is right in their neighborhood. Don't you think the swarmers are gonna be, well, swarming all over our operation?' \nIrina nodded. 'Yeah. But we'll land in a safe area to the south. We'll see what we do from there.' \n \nThe aircraft landed as the crew got ready to unload their gear.",
                    
                    LoadingDialogImage = "FleeingSkimmer",

                    SpawnWorldAction = "spawnWorld",
                    SpawnSiteAction = "spawnPlaySite",

                    WorldMapImage = "Scenarios/Default/GUI/regionalMap_1",

                    EnableMissions = true,

                    //global events to register regardless of customization/difficulty:
                    ConditionalEvents = new[]
                    { 

            #region music and game over
                   //this is an exact 2 day playlist: 
                   "SANDBOXNOMADMAP_musicTrackList",

                   "SANDBOXMAP_loseGame",
#endregion

//no animal migrations.

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
// if you need to test a building which needs a higher policy it can be changed in EventActionLoader in the selectet scenario. in the placeExpedition Method
#region loads of random test stuff

//"startPeat","startPeat",
/* "startHoe", "startHoe", "startGlassyCreeper", "startGlassyCreeper", "startCrystalBerries", "startCrystalBerries",
 
              "startFishTrapBasket",
              "startFiregrassSod", "startStones","startStones","startStones","startStones","startStones",
             "startSticks","startSticks","startSticks","startSticks","startSticks","startSticks","startSticks","startSticks",         
            "startSpoakLeaves", "startSpoakLeaves","startSpoakLeaves", "startSpoakLeaves","startSpoakLeaves", "startSpoakLeaves",
             "startWaterCaneStem", "startWaterCaneStem", "startWaterCaneStem","startWaterCaneStem", "startWaterCaneStem",
             "startSpoakShingles", "startSpoakShingles",
             "startSolidMudBrick", "startSolidMudBrick","startSolidMudBrick","startSolidMudBrick","startSolidMudBrick",
             "startSpoakBranchesTrimmed",  "startSpoakBranchesTrimmed",  "startSpoakBranchesTrimmed",

             "startShadeleafCanes", "startShadeleafCanes", "startShadeleafCanes", "startShadeleafCanes","startShadeleafCanes", "startShadeleafCanes", "startShadeleafCanes","startShadeleafCanes",
             "startTextile","startTextile","startTextile","startTextile","startTextile","startTextile",
   "startTextile","startTextile","startTextile","startTextile","startTextile","startTextile","startTextile","startTextile",
"startTurnipSalami","startTurnipSalami","startTurnipSalami","startTurnipSalami","startTurnipSalami","startTurnipSalami", "startVinegar", "startVinegar",
"startHardtack","startHardtack","startHardtack","startHardtack","startHardtack","startHardtack","startHardtack", "startDriedSaltedStreakFin","startDriedSaltedStreakFin","startDriedSaltedStreakFin","startDriedSaltedStreakFin",


             "startSentry",

             "startBellows",
              "startHammer",
            "startVat",
 "startSpikeTrap",

 "startBlackpulp",
 "startBlunderbuss", "startBlackPowderShotAmmo", "startBlackPowderShotAmmo",
 "startTurnipCracker", "startNeonHornetsLive", "startFishTrapBasket", "startBlackpowder",
"startBlowpipe", "startPickaxe", "startSpade", "startAnvil",
"startWaterCaneStem", "startWaterCaneStem","startWaterCaneStem", "startWaterCaneStem","startWaterCaneStem", "startWaterCaneStem",


"startWetFirewood", "startWetFirewood", "startWetFirewood",
"startGoldOre", "startGoldOre", "startBogOre","startBogOre","startBogOre","startBogOre","startBogOre",
"startFingerFruit", "startFingerFruit",
"startSalt", "startSalt", "startSalt","startSalt","startSalt","startSalt",
"startClayPotUnglazed", "startTappingBucket","startTappingBucket","startClayJar","startClayJar","startClayJar",
"startClay", "startClay", "startClay","startClay","startClay","startClay",
"startFirewood","startFirewood", "startCharcoal","startCharcoal","startCharcoal",

"startMarshcotSap", "startMarshcotSap",
"startWingweedMats","startWingweedMats","startWingweedMats",

"startIronHooks",
"startIronHandAxe", "startIronSpear",*/
#endregion

//"startRareMetal2",

//"startSticks","startSticks","startSticks","startSticks",
//"startSpade",
//"startFishtrapTest","startSticks","spawnSecuritySpecialist2", "spawnCookingSpecialist1","spawnSecuritySpecialist2", "spawnCookingSpecialist1","spawnSecuritySpecialist2", "spawnCookingSpecialist1",
//"spawnSecuritySpecialist2", "spawnCookingSpecialist1","spawnSecuritySpecialist2", "spawnCookingSpecialist1","spawnSecuritySpecialist2", "spawnCookingSpecialist1","spawnSecuritySpecialist2", "spawnCookingSpecialist1","spawnSecuritySpecialist2", "spawnCookingSpecialist1",
//"startVarmintBomb","startVarmintBomb","startVarmintBomb","startVarmintBomb","startVarmintBomb","startVarmintBomb","startVarmintBomb",
// "startStructureLanding", 
//"startStructureGroundStation",
//"startStructureSimplePort",
//"startStructureHelipadBig", "startStructureHelipadBig2",
//"startStructureRadioHutImprovised",
//"startFishTrapBasket",
//"startBlackpulp","startBlackpulp","startBlackpulp","startBlackpulp",
 /* "startHoe", "startHoe", "startGlassyCreeper", "startGlassyCreeper", "startCrystalBerries", "startCrystalBerries",
             
             //           
              // 
              "startFishTrapBasket",
              "startFiregrassSod", "startStones","startStones","startStones","startStones","startStones",
             "startSticks","startSticks","startSticks","startSticks","startSticks","startSticks","startSticks","startSticks",         
            "startSpoakLeaves", "startSpoakLeaves","startSpoakLeaves", "startSpoakLeaves","startSpoakLeaves", "startSpoakLeaves",
             "startWaterCaneStem", "startWaterCaneStem", "startWaterCaneStem","startWaterCaneStem", "startWaterCaneStem",
             "startSpoakShingles", "startSpoakShingles",
             "startSolidMudBrick", "startSolidMudBrick","startSolidMudBrick","startSolidMudBrick","startSolidMudBrick",
             "startSpoakBranchesTrimmed",  "startSpoakBranchesTrimmed",  "startSpoakBranchesTrimmed",

             "startShadeleafCanes", "startShadeleafCanes", "startShadeleafCanes", "startShadeleafCanes","startShadeleafCanes", "startShadeleafCanes", "startShadeleafCanes","startShadeleafCanes",

   
"startTurnipSalami","startTurnipSalami","startTurnipSalami","startTurnipSalami","startTurnipSalami","startTurnipSalami", "startVinegar", "startVinegar",
"startHardtack","startHardtack","startHardtack","startHardtack","startHardtack","startHardtack","startHardtack", "startDriedSaltedStreakFin","startDriedSaltedStreakFin","startDriedSaltedStreakFin","startDriedSaltedStreakFin",

/*             "startRadioAntenna", "startRadio",
             "startSpikeTrap","startSpikeTrap","startSpikeTrap", */
  /*           "startSentry",
//structures:
      //   "startStructureSimplePort", 
       //       "startStructureRadioHutImprovised", //"startStructureImprovisedSmithy", // testing

         //     "startCharcoal", "startCharcoal", "startCharcoal",
             "startBellows",
              "startHammer",
            "startVat",
 "startSpikeTrap",

 "startBlackpulp",
 "startBlunderbuss", "startBlackPowderShotAmmo", "startBlackPowderShotAmmo",
 "startTurnipCracker", "startNeonHornetsLive", "startFishTrapBasket", "startBlackpowder",
"startBlowpipe", "startPickaxe", "startSpade", "startAnvil",
"startWaterCaneStem", "startWaterCaneStem","startWaterCaneStem", "startWaterCaneStem","startWaterCaneStem", "startWaterCaneStem",


"startWetFirewood", "startWetFirewood", "startWetFirewood",
"startGoldOre", "startGoldOre", "startBogOre","startBogOre","startBogOre","startBogOre","startBogOre",
//"startImprovisedGreenHouseCover", "startImprovisedGreenHouseCover","startImprovisedGreenHouseCover","startImprovisedGreenHouseCover","startImprovisedGreenHouseCover","startImprovisedGreenHouseCover", 
"startFingerFruit", "startFingerFruit",
"startSalt", "startSalt", "startSalt","startSalt","startSalt","startSalt",
"startClayPotUnglazed", "startTappingBucket","startTappingBucket","startClayJar","startClayJar","startClayJar",
"startClay", "startClay", "startClay","startClay","startClay","startClay",
"startFirewood","startFirewood", "startCharcoal","startCharcoal","startCharcoal",

"startMarshcotSap", "startMarshcotSap",
"startWingweedMats","startWingweedMats","startWingweedMats",

"startIronHooks",
"startIronHandAxe", "startIronSpear",*/
  
 
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
                        
                        "spawnSwarmerExpedition1",
                        "spawnSwarmerExpedition2",
                        "spawnSwarmerAllegiance1",
                        "spawnLeafcutterExpedition#1",
                        "spawnBirdExpedition#1", "spawnBirdExpedition#2",
                       
                        "spawnBinalRatExpedition#1", "spawnBinalRatExpedition#2", 

                        "spawnSnatcherExpedition#1",

                        "spawnThunderChickenExpedition#2",
                        "spawnBushDragonExpedition#1",   
                 
                        "spawnSlugExpedition#1",

                        "spawnPatricianExpedition#1",
                        "spawnTurnipExpeditionSouth",
                 
                        "spawnDemonTreeExpedition#1", // "spawnDemonTreeExpedition#3",

                        "spawnSwampDemonTreeExpedition#2",

                      //  "spawnBajinganExpeditionWest",//migration target - is in spawn critter in actionsetsloader
                        //"spawnLesserWhipjawExpeditionWest",//migration target - is in spawn critter in actionsetsloader
                        //used for destroying migrating animals at map edge:
                        "spawnAnimalMigrateTriggerWest", "spawnAnimalMigrateTriggerRiver", 
#endregion

                        "spawnPlayerAllegiance", "placeExpedition", "setView", "setPlayerCredits",

    "exploreShroudNaturalTerminal", //reveal natural terminal . leads to wilderness


    #region Spawn wildernessSite1                
                          "spawnWildernessSite1",  "spawnWildernessSite1Allegiance1", "spawnWildernessSite1Expedition1",                                
                          "spawnPlaySiteWildernessSite1Route", 
#endregion
    #region Spawn otherSite1                
                          "spawnOtherSite1",  "spawnOtherSite1Allegiance1", "spawnOtherSite1Expedition1",                                
                         
#endregion

    #region otherSite1Expedition1 ///migrants
                          //mp: my aim is to get a smith and a couple more join the player early (bare field). start can be a bit random (should be 2-5 people, always with a smith) 
                          //...then comes a mix of people ranging evenly across the spectrum up to higher demands. professions evenly mixed.

                          //early joiner immigrants
                      // "spawnImmigrantSmithingSpecialist",//important to get a smith early
                     //   "spawnImmigrantMenialSpecialist", //removed this because there was enough early joiners.

                       //next immigrants
                       "spawnImmigrantMediumTier","spawnImmigrantMediumTier","spawnImmigrantMediumTier","spawnImmigrantMediumTier", "spawnImmigrantMediumTier", "spawnImmigrantMediumTier", "spawnImmigrantMediumTier",
                         //also next immigrants (there will be some overlap)
                       "spawnImmigrantAdvancedTier","spawnImmigrantAdvancedTier","spawnImmigrantAdvancedTier","spawnImmigrantAdvancedTier","spawnImmigrantAdvancedTier",
                       //"spawnImmigrantRandom","spawnImmigrantRandom", "spawnImmigrantRandom",
#endregion

    #region Spawn otherSite2 (random)                
                          "spawnOtherSite2",   "spawnPlaySiteSite2Route",   
                       
                          // immigrants
                          "spawnImmigrantMediumTier2", "spawnImmigrantMediumTier2","spawnImmigrantMediumTier2","spawnImmigrantMediumTier2","spawnImmigrantMediumTier2",
                          "spawnImmigrantAdvancedTier2","spawnImmigrantAdvancedTier2","spawnImmigrantAdvancedTier2",
#endregion

                  #region pier spots
                        "startPierSpot1",
                        "startPierSpot2", 
                        "startPierSpot3", 
                  #endregion

                  #region particle effects                  
                        "smallFog1", "smallFog2", "smallFog3","smallFog4", "smallFog5","smallFog6","smallFog7","smallFog8","smallFog9","smallFog10","smallFog11","smallFog12","smallFog13","smallFog14","smallFog15",
                        "sulphurousSmoke1", "sulphurousSmoke2","sulphurousSmoke3","sulphurousSmoke4",
                        "haze1","haze2","haze3","haze4",
                         "fog1", "fog2","fog3", "fog4","fog5", "fog6","fog7", "fog8", "fog9", "fog10", "fog11", "fog12", "fog13", "fog14", 
                          
            #endregion

                    },

         ////////////////////////////////

                    #region Difficulties
                    MainDifficultySettings = new[]
                    {
                        
                    /*    new Difficulty()
                        {
                            KeyName = "easy",
                            Name = "Easy",
                            OptionsToUse = new SerializableDictionary<string, string[]>()
                            {
                         //       { "startingLocations", new []{} },
                          //       { "people", new []{"6people1dog1robot"} },                         
                                  { "expeditionType", new []{"southExpedition"} },
                                { "fauna" , new []{"average"} },
                                { "resources" , new []{"average"} },
                            //    { "otherSite1Allegiance1Relation", new []{} }
                            }
                         },*/
                          new Difficulty()
                         {
                              KeyName = "normal",
                              Name = "Normal",
                              Description ="Your crew of miners can choose to fulfill an order for metals or they can start a settlement however they want.",
                              IsDefault = true,
                              OptionsToUse = new SerializableDictionary<string, string[]>()
                              {
                                //    { "startingLocations", new []{"riverBank", "northArableLand"} },
                           //         { "people", new []{"6people1dog1robot"} }, //
                                    { "expeditionType", new []{"northExpedition"} },
                                    { "fauna" , new []{"average"} },
                                    { "resources" , new []{"average"} }, //
                                    //{ "otherSite1Allegiance1Relation", new []{} }
                              },
                         },
         
                         
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
                                    KeyName = "average",
                                    Name = "Average",                                
                                    Difficulty = new CustomDifficulty()
                                    {
                                        KeyName = "normal",
                                        Name = "Bush dragons", //shown on CUSTOM selector. no room for longer sentence..
                                        IsDefault = true,   
                                        ScoreModifier = 1f
                                    },
      
                                    ActionKeys = new[]
                                    {
                                        //field quadites:
                                        "setMaxLeafcuttersNormal",      "setSpawnIntervalLeafcutterNormal",

                                        //migrating animals:
                                        "setMaxLesserWhipjawNormal",    "setSpawnIntervalLesserWhipjawMigration",
                                        "setMaxBajinganNormal",         "setSpawnIntervalBajinganMigration",


                                    }
                                },


                            }
                        },
#endregion
                     

 
                    #region Expedition type options
                          new OptionSet(){
                               KeyName = "expeditionType",
                               Name = "Expedition type",
                                DisplayGroup = 0,
                                Options = new[] { 

                                    new Option()
                                    {
                                      KeyName = "northExpedition",
                                      Name = "north",
                                      Difficulty = new CustomDifficulty()
                                      {
                                           KeyName = "normal",
                                           Name = "Specialized", 
                                           ScoreModifier = 1f
                                      },  
                                      ConditionalEvents = new[]{"introDialogue",//INTRO Banter
                                                               "introDialogueScreen"}, //INTRO SCREEN
                                    
                                       ActionKeys = new[]
                                       { 
                                     //STARTING LOCATION:
                                           "setStartingLocationNorth", "exploreShroudFromSouth",

                                     //natural terminal to wilderness
                                           "startNaturalTerminalGenericPosition",     
                                          
                                     //PEOPLE: //2 persons are mentioned in story, add 'Mike'!
                                         "spawnNadova",  "spawnMike", "spawnSecuritySpecialist2", "spawnCookingSpecialist1","spawnMiningRobot","spawnHaulRobot","spawnHaulRobot2",  //"spawnHaulRobot3",

                                         "spawnGuardRobot",

                                         //nadova is the : "spawnConstructionSpecialist1",
                                         //mike is the: "spawnSecuritySpecialist1"

                                     /////LOADOUT:
                                     //general tools/weapons:
                                

                                    //       "startBoltActionRifle",
                                    //       "startBoltActionAmmo","startBoltActionAmmo",

                                  //         "startSentryOutside",
                                  //           "startSentryGunAmmo",
                             
                                        "startString","startKnife","startKnife", "startKnife", "startMachete","startMachete",

                                           "startSpade",

                                           "startShotgun",
                                           "startShotgunAmmo","startShotgunAmmo","startShotgunAmmo",
                                           "startCoilRifle","startCoilRifle", "startCoilRifle",
                                           "startCoilRifleAmmo","startCoilRifleAmmo","startCoilRifleAmmo", "startCoilRifleAmmo", "startCoilRifleAmmo",
                                             
                                    //Cooking/food:
                                          "startRation","startRation","startRation","startRation","startRation","startRation","startRation","startRation",
                                          "startRation","startRation","startRation","startRation","startRation","startRation","startRation","startRation",
                                          "startRation","startRation","startRation","startRation","startRation","startRation","startRation","startRation", //mp added extra rations to compensate for lower sec rating
                                          "startSimCoffeeBeans", "startSimCoffeeBeans", "startSimCoffeeBeans", "startSimCoffeeBeans", "startSimCoffeeBeans", 

                                          // Structure items
                                    "startFieldLabPacked","startFieldKitchenStove","startFieldKitchenEquipment", "startCookingPot", "startSensor",
                                    "startDomeTentItem","startSmallTentItem","startSmallTentItem","startStructurePanels","startStructurePanels", //"startSatteliteGroundStationItem",  //"startPaint","startStones", // used for the primitive helipad.
                                      "startRefinery1","startRefinery2",

                                      // Finished structures
                                      "startStructureGroundStation",
                                    //"startStructureDomeTent",  "startStructureSmallTent", "startStructureFieldKitchen", "startStructureHelipadBig",
                                    
                                    // materials
                                     "startLiquidGas", "startLiquidGas", "startLiquidGas"

                                       }
                                    },

                                  /*  new Option()
                                    {
                                      KeyName = "southExpedition",
                                      Name = "south",
                                      Difficulty = new CustomDifficulty()
                                      {
                                           KeyName = "normal",
                                           Name = "Specialized",
                                           ScoreModifier = 1f
                                      },  
                                       ConditionalEvents = new[]{"introDialogueScreen"}, //INTRO DIALOGUE different from the turnip hunters, is about chickens
                                       ActionKeys = new[]
                                       {  //STARTING LOCATION:
                                           "setStartingLocationSouth", "exploreShroudFromSouth",
                                        //removed for mapsize:   "setStartingLocationNorthMuddyCreek", "exploreShroudNorthArableLand",

                                          //natural terminal to wilderness
                                            "startNaturalTerminalGenericPosition", 

                                        //PEOPLE: 
                                        "spawnHuntingSpecialist1", "spawnHuntingSpecialist2", "spawnSecuritySpecialist1","spawnSecuritySpecialist2","spawnCookingSpecialist1", "spawnHaulRobot",

                                           /////LOADOUT:
                                          //general tools/weapons:
                                          "startRadioAntenna", "startRadio",
                                           "startString","startKnife","startKnife","startMachete",

                                           "startGunpowderRifle","startGunpowderAmmo",

                                           //for digging favorbread farm:
                                           "startSteelSpade",
                                           
                                        //Mining/PlanetFall equipment:  
                         "startAssemblerCabinet", "startVacuumChamber", "startAssemblerCooling",
                                               "startAssemblerPlateA", "startAssemblerMasterPlateA", "startAcetylene", "startAcetylene", "startAcetylene", "startAcetylene",
                                               "startAssemblerPlateB", "startAssemblerMasterPlateB", "startIronCanister",
                                              
                                    //Cooking/food:
                                           "startCookingPot", //
                                           "startJerky","startJerky","startJerky","startJerky", "startJerky","startJerky","startJerky",
                                      "startHardtack","startHardtack", "startHardtack","startHardtack","startHardtack","startHardtack", "startHardtack", "startHardtack",
                                             // Structures
                                       "startStructureDomeTent",  "startStructureSmallTent", "startStructureFieldKitchen", "startStructureHelipadBig", "startStructureGroundStation",
                                       
                                           
                                       }
                                    },*/

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
                                     "startRareMetalOreDepositCenter",
                                     "startRareMetalOreDepositNorth",
                                     
                                    
                                     "startFarmSpotSmall1", "startFarmSpotSmall2",  "startFarmSpotSmall3", "startFarmSpotSmall4",  "startFarmSpotSmall5", "startFarmSpotSmall6", "startFarmSpotSmall7",
                                     "startFarmSpotSmall8", "startFarmSpotSmall9",  "startFarmSpotSmall10", "startFarmSpotSmall11",  "startFarmSpotSmall12", "startFarmSpotSmall13", "startFarmSpotSmall14",
                                     "startFarmSpotSmall5",  "startFarmSpotSmall16", "startFarmSpotSmall17", "startFarmSpotSmall18",
                                     "startFarmSpotLarge1", "startFarmSpotLarge2",  "startFarmSpotLarge3",  //"startFarmSpotLarge4",  

                                     "startFishTrapCreek1", "startFishTrapCreek2", "startFishTrapCreek3", "startFishTrapCreek4",                                  
                                     "startFishTrapCreek5", "startFishTrapCreek6", "startFishTrapCreek7", "startFishTrapCreek8",
                                     "startFishTrapCreek9", "startFishTrapCreek10",

                                     "startFishTrapShore1", "startFishTrapShore2", "startFishTrapShore3", "startFishTrapShore4",
                                     "startFishTrapShore5", "startFishTrapShore6",
                                    
                                     "startFishTrapCoast1","startFishTrapCoast2","startFishTrapCoast3","startFishTrapCoast4","startFishTrapCoast5","startFishTrapCoast6","startFishTrapCoast7",

                                     "startBogOreDeposit1","startBogOreDeposit2","startBogOreDeposit3","startBogOreDeposit4",
                                     "startPeatDeposit1","startPeatDeposit2","startPeatDeposit3","startPeatDeposit4","startPeatDeposit5","startPeatDeposit6","startPeatDeposit7","startPeatDeposit8","startPeatDeposit9",
                                      "startSaltDeposit2","startSaltDeposit3","startSaltDeposit4","startSaltDeposit5", //mp removed this because too close to scandium: "startSaltDeposit1",
                                     "startClayDeposit1", "startClayDeposit2","startClayDeposit3","startClayDeposit4","startClayDeposit5",
                             
                                
                                }
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
                                     "startFishTrapCreek5", "startFishTrapCreek6", "startFishTrapCreek7", "startFishTrapCreek8",                                  
                                    "startFishTrapCoast1", "startFishTrapCoast2",
                                    "startFishTrapShore1", "startFishTrapShore2", "startFishTrapShore3", "startFishTrapShore4", "startFishTrapShore6", "startFishTrapShore7", "startFishTrapShore8", "startFishTrapShore9", "startFishTrapShore10", "startFishTrapShore11", "startFishTrapShore12", "startFishTrapShore13", "startFishTrapShore14", "startFishTrapShore15", "startFishTrapShore16" , "startFishTrapShore17", "startFishTrapShore18", "startFishTrapShore19"
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
