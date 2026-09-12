using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.InGameEvents.Actions;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.AllGameData.Scenarios;
using UWGame.ClientSide.GameEvents;
using UWGame.SimSide.InGameEvents.PropertyObjects;
using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.Client.Particles;
using UWGame.SimSide.InGameEvents.Expressions;
using UWGame.SimSide.Overland;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.XmlCollections;
using UWGame.SimSide.Systems;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_3.Data
{
    public class EventActionLoader
    {
        public static List<EventActionType> Init()
        {
            List<EventActionType> list = new List<EventActionType>();


            double delayForStructures = 0.25;
            double delayForItemsAndAgents = 0.5;

            #region Init

            list.Add(new SetPropertyAction()
            {
                KeyName = "initIncludeDateInBurial",
                PropertyKey = "includeDateInBurialHeader",
                Value = new ValueNode() { Bool = true }
            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "initBurialText1",
                PropertyKey = "burialTextStart",
                Value = new ValueNode() { String = "Location: 4° 12' 22'' South, 7° 12' 19.2'' West \nJournal entry #2 \n#JOURNALNAMES \n \n" }
            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "initBurialText2",
                PropertyKey = "burialTextMultipleDeathsMultipleSurvivors",
                Value = new ValueNode() { String = "We have lost #NAMEOFDECEASED. At the burial a eulogy was delivered by #EUOLOGYGIVER. The speech is included as an audio file. \n \nGoodbye, #NAMEOFDECEASED. You will be remembered as a shining example for future generations of settlers. Rest in peace." }
            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "initBurialText3",
                PropertyKey = "burialTextSingleDeathMultipleSurvivors",
                Value = new ValueNode() { String = "We have lost #NAMEOFDECEASED#CAUSEOFDEATH. At the burial a eulogy was delivered by #EUOLOGYGIVER. The speech is included as an audio file. \n \nGoodbye, #NAMEOFDECEASED. You will be remembered as a shining example for future generations of settlers. Rest in peace." }
            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "initBurialText4",
                PropertyKey = "burialTextSingleDeathSingleSurvivor",
                Value = new ValueNode() { String = "#NAMEOFDECEASED is also dead now. \nI buried the remains as best I could. Guess it's only me now..." }
            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "initBurialText5",
                PropertyKey = "burialTextMultipleDeathsSingleSurvivor",
                Value = new ValueNode() { String = "#NAMEOFDECEASED is also dead now. \nI buried the remains as best I could. Guess it's only me now..." }
            });


            list.Add(new SetPropertyAction()
            {
                KeyName = "initGorgeDetected",
                PropertyKey = "gorgeDetected",
                Value = new ValueNode() { Bool = false }
            });


            list.Add(new SetPropertyAction()
            {
                KeyName = "initCampfireFinished",
                PropertyKey = "campfireFinished",
                Value = new ValueNode() { Bool = false }
            });


            list.Add(new SetPropertyAction()
            {
                KeyName = "initspearReadyToBeMade",
                PropertyKey = "spearReadyToBeMade",
                Value = new ValueNode() { Bool = false }
            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "initFlintSpearMade",
                PropertyKey = "flintSpearMade",
                Value = new ValueNode() { Bool = false }
            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "initSignalPyreLit",
                PropertyKey = "signalPyreLit",
                Value = new ValueNode() { Bool = false }
            });


            list.Add(new SetPropertyAction()
            {
                KeyName = "initGameOver",
                PropertyKey = "gameOver",
                Value = new ValueNode() { Bool = false }
            });



            #endregion


            #region Characters
            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnHarron",
                DelayInSeconds = delayForItemsAndAgents,

                DynamicLocation = new DynamicLocation()
                {
                    PropertyKey = "startingLocation"
                },
                EntityData = new Maps.MapEditor.EntityData()
                {
                    Location = new Microsoft.Xna.Framework.Vector3(-30f, 68f, 0), //38f
                    EntityKey = "entity:human",
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "playerAllegiance",
                        ExpeditionKey = "Camp"
                    },                   
                    Person = new Maps.MapEditor.Person()
                    {
                        FirstName = "Celoy",
                        LastName = "Harron",
                        PersonalityType = "survivalTierPersonality",
                        Portrait = "human_w_m_adult_1",
                        SimulateJoinedExpeditionNow = true,
                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {
                        AgeInYears = new NormalDistribution() { Mean = 52f },
                        CasteKey = "male",
                        RaceKey = "greenBlueClothes1",
                        StomachContent = new NormalDistribution() { Mean = 0f }, // fixed value, no deviation 
                        Skills = new SerializableDictionary<string, float>()
                                            {
                                                { "bushcraft", 0.9f},
                                                { "hunting", 1f},
                                                { "butchering", 0.8f},
                                                { "fishing", 1f},
                                                { "foraging", 1f},
                                                { "cooking", 0.8f},
                                                { "menial", 0.7f},
                                                { "shooting", 1f},
                                                { "armedMelee", 1f},
                                                { "unarmedFighting", 0.8f},
                                                { "psychology", 0.1f},
                                                { "biology", 0.08f},
                                                // skills that was missing
                                                { "farming", 0.8f},
                                                { "weeding", 0.8f},
                                                { "grasping", 0.8f},
                                                { "fruitPicking", 0.8f}, 
                                                { "construction", 0.5f},   
                                                { "smithing", 0.5f},  
                                               
                                                 //new
                                                 { "mechanics", 0.2f},
                                                 { "electronics", 0.08f},
                                                 { "chemistry", 0.05f},
                                                 //  
                                                { "archery", 0.5f},   
                                                { "sneaking", 0.5f},
                                                { "medicine", 0.5f}, 
                                                // new NA random value. might need to be diffrent?
                                                {"carpentry", 0.5f},
                                                {"weaving", 0.5f}
                                               
                                            }

                    },
                    NeedLevels = new SerializableDictionary<string, NeedData>() // optional starting needs!
                        {
                            { "protein", new NeedData(){ Level = new NormalDistribution() { Mean = 0.3f } }},
                            { "foodEnergy", new NeedData(){ Level = new NormalDistribution() { Mean = 0.2f } }},
                            { "micronutrients", new NeedData(){ Level = new NormalDistribution() { Mean = 0.2f } }},
                            { "stimulants", new NeedData(){ Level = new NormalDistribution() { Mean = 0.2f } }}

                      //      { "protein", new NeedData(){ Level = new NormalDistribution() { Mean = 0.49f } }},
                      //      { "foodEnergy", new NeedData(){ Level = new NormalDistribution() { Mean = 0.05f } }},
                      //      { "micronutrients", new NeedData(){ Level = new NormalDistribution() { Mean = 0.49f } }},
                      ////      { "stimulants", new NeedData(){ Level = new NormalDistribution() { Mean = 0.2f } }}    //mp weighs in the calculation of hunger level for some reason (Bug )
                      }
                }

            });

            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnSantilla",
                DelayInSeconds = delayForItemsAndAgents,

                DynamicLocation = new DynamicLocation()
                {
                    PropertyKey = "startingLocation"
                },
                EntityData = new Maps.MapEditor.EntityData()
                {
                    Location = new Microsoft.Xna.Framework.Vector3(-44f, 96f, 0),
                    EntityKey = "entity:human",
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "playerAllegiance",
                        ExpeditionKey = "Camp"
                    },
                   
                    Person = new Maps.MapEditor.Person()
                    {
                        FirstName = "Javin",
                        LastName = "Santilla",
                        PersonalityType = "survivalTierPersonality",
                        Portrait = "human_h_m_adult_1",
                        SimulateJoinedExpeditionNow = true,
                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {
                        AgeInYears = new NormalDistribution() { Mean = 44f },
                        CasteKey = "male",
                        RaceKey = "greyClothes1",
                        StomachContent = new NormalDistribution() { Mean = 0f }, // fixed value, no deviation                           
                        Skills = new SerializableDictionary<string, float>()
                                            {       
                                                { "bushcraft", 0.9f},
                                                { "hunting", 0.7f},
                                                { "butchering", 0.4f},
                                                { "fishing", 0.5f},
                                                { "foraging", 0.5f},
                                                { "cooking", 0.5f},
                                                { "menial", 0.7f},
                                                { "shooting", 1f},
                                                { "armedMelee", 0.8f},
                                                { "unarmedFighting", 0.9f},
                                                { "medicine", 0.6f},
                                                { "psychology", 0.8f},
                                                { "biology", 0.1f} ,
                                                { "smithing", 0.5f}, 
                                          
                                                 //new
                                                 { "mechanics", 0.1f},
                                                 { "electronics", 0.07f},
                                                 { "chemistry", 0.05f},
                                                 //  
                                                { "archery", 0.5f},   
                                                { "sneaking", 0.5f},
                                                 // skills that was missing
                                                { "farming", 0.8f},
                                                { "weeding", 0.8f},
                                                { "grasping", 0.8f},
                                                { "fruitPicking", 0.8f}, 
                                                { "construction", 0.5f}, 
                                                // new NA random value. might need to be diffrent?
                                                {"carpentry", 0.5f},
                                                {"weaving", 0.5f}
                                                               }
                    },
                    NeedLevels = new SerializableDictionary<string, NeedData>() // optional starting needs!
                        {
                            { "protein", new NeedData(){ Level = new NormalDistribution() { Mean = 0.3f } }},
                            { "foodEnergy", new NeedData(){ Level = new NormalDistribution() { Mean = 0.2f } }},
                            { "micronutrients", new NeedData(){ Level = new NormalDistribution() { Mean = 0.2f } }},
                            { "stimulants", new NeedData(){ Level = new NormalDistribution() { Mean = 0.2f } }}
                        }
                }

            });




            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnScoyd",
                DelayInSeconds = delayForItemsAndAgents,

                DynamicLocation = new DynamicLocation()
                {
                    PropertyKey = "startingLocation"
                },
                EntityData = new Maps.MapEditor.EntityData()
                {
                    Location = new Microsoft.Xna.Framework.Vector3(-94f, 74f, 0),
                    EntityKey = "entity:human",
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "playerAllegiance",
                        ExpeditionKey = "Camp"
                    },
                   
                    Person = new Maps.MapEditor.Person()
                    {
                        FirstName = "Taryn",
                        LastName = "Scoyd",
                        PersonalityType = "survivalTierPersonality",
                        Portrait = "human_a_m_adult_1",
                        SimulateJoinedExpeditionNow = true,
                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {
                        AgeInYears = new NormalDistribution() { Mean = 38f },
                        CasteKey = "male",
                        RaceKey = "greenGreyClothes1",
                        StomachContent = new NormalDistribution() { Mean = 0f }, // fixed value, no deviation       
                        Skills = new SerializableDictionary<string, float>()
                                        {                                                                   
                                            { "bushcraft", 0.9f},
                                            { "hunting", 0.7f},
                                            { "butchering", 0.6f},
                                            { "fishing", 0.5f},
                                            { "foraging", 0.5f},
                                            { "cooking", 0.9f},
                                            { "menial", 0.8f},
                                            { "shooting", 1f},
                                            { "armedMelee", 0.8f},
                                            { "unarmedFighting", 0.9f},
                                            { "medicine", 0.3f},
                                            { "psychology", 0.1f},
                                            { "biology", 0.06f} ,
                                            { "smithing", 0.5f}, 

                                                 //new
                                                 { "mechanics", 0.1f},
                                                 { "electronics", 0.05f},
                                                 { "chemistry", 0.02f},
                                                 // 
                                            { "archery", 0.5f},   
                                            { "sneaking", 0.5f},
                                            // skills that was missing
                                            { "farming", 0.8f},
                                            { "weeding", 0.8f},
                                            { "grasping", 0.8f},
                                            { "fruitPicking", 0.8f}, 
                                            { "construction", 0.5f},
                                            // new NA - random value. might need to be diffrent?
                                            {"carpentry", 0.5f},
                                            {"weaving", 0.5f}
                                        }
                    },
                    NeedLevels = new SerializableDictionary<string, NeedData>() // optional starting needs!
                        {
                            { "protein", new NeedData(){ Level = new NormalDistribution() { Mean = 0.3f } }},
                            { "foodEnergy", new NeedData(){ Level = new NormalDistribution() { Mean = 0.2f } }},
                            { "micronutrients", new NeedData(){ Level = new NormalDistribution() { Mean = 0.2f } }},
                            { "stimulants", new NeedData(){ Level = new NormalDistribution() { Mean = 0.2f } }}
                        }


                }
            });

            #endregion

            #region Particles



            #endregion

            #region Starting locations

            list.Add(new CreateExpeditionAction()
            {
                KeyName = "placeExpedition",
                DelayInSeconds = 0.1, // wait for starting location property to have been set!

                ExpeditionData = new ExpeditionData()
                {
                    KeyName = "Camp",
                    Name = "Camp",
                    AllegianceKey = "playerAllegiance",
                    Location = new ValueNode()
                    {
                        PropertyKey = "startingLocation"
                    }
                }

            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "setStartingLocationSouth",

                PropertyKey = "startingLocation",
                Value = new ValueNode() { Location = new Microsoft.Xna.Framework.Vector2(3056f, 3890f) }//3106f, 3940f

            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "setStartingLocationNorth", //for midway test purpose

                // has to be set first, because the other spawns depend on the value!
                PropertyKey = "startingLocation",
                Value = new ValueNode() { Location = new Microsoft.Xna.Framework.Vector2(3024f, 3216f) } //2880f, 3216f

            });


            #endregion


            #region Resources

            list.Add(new ChangeResourcesAction()
            {
                KeyName = "setFishSchoolMedium", //this is for the resources where we want a big difference from game to game in where they are found. (meaning they have many possible habitats on the map). some game3s, they will be in one corner of the screen, other games, in the other corner. therefore place clusters  about 2- 3 places on each screen, well apart. not sure if the base number should be 1-1 or 0-4 for example.


                ResourceType = new[] { "streakFin", "carbonTail", "alabasterRay", "daggermouth", "clamwich", "torux", "minnowsLive", "phantomWeaver", "ursinix", "webWing", "crestedFoiler", "goldenCenobite", "muckGrinder" },
                OperationToUse = ChangeResourcesAction.Operation.Multiply,
                //   NoiseAmplitude = 0.5f,
                NoiseParameters = new NoiseParams()
                {
                    NoiseAmplitude = 1.5f,
                    NoiseAddend = -1f,
                    NoiseFrequency = 0.035f // high means "white noise". 0.01f not useful, just a big gradient......0.05f kinda bigger
                }

            });


            list.Add(new ChangeResourcesAction()
            {
                KeyName = "setFishSchoolSparse",


                ResourceType = new[] { "streakFin", "carbonTail", "alabasterRay", "daggermouth", "clamwich", "torux", "minnowsLive", "phantomWeaver", "ursinix", "webWing", "crestedFoiler", "goldenCenobite", "muckGrinder" },
                OperationToUse = ChangeResourcesAction.Operation.Multiply,
                //   NoiseAmplitude = 0.5f,
                NoiseParameters = new NoiseParams()
                {
                    NoiseAmplitude = 1.5f,
                    NoiseAddend = -2f,
                    NoiseFrequency = 0.035f // high means "white noise". 0.01f not useful, just a big gradient......0.05f kinda bigger
                }

            });

            list.Add(new ChangeResourcesAction()
            {
                KeyName = "setPlentyResources",

                AllResources = true,
                ExcludeResourceTypes = new[] { "stones", "firewood", "flint", "crop:waterCaneStem", "commonOilTubers" }, //MP I want no variance on these because they are essential to tutorial
                OperationToUse = ChangeResourcesAction.Operation.Multiply,
                //   NoiseAmplitude = 0.5f,
                NoiseParameters = new NoiseParams()
                {
                    NoiseAddend = 0.2f, // add 'X %' 
                    NoiseFrequency = 0.1f // high means "white noise"
                }

            });

            list.Add(new ChangeResourcesAction()
            {
                KeyName = "setNormalResources",


                AllResources = true,
                ExcludeResourceTypes = new[] { "stones", "firewood", "flint", "crop:waterCaneStem", "commonOilTubers" },//MP I want no variance on these because they are essential to tutorial
                OperationToUse = ChangeResourcesAction.Operation.Multiply,

                NoiseParameters = new NoiseParams()
                {
                    NoiseFrequency = 0.1f
                }

                //  NoiseAmplitude = 0.5f,
                // NoiseAddend = 1f // neutral variation

            });

            list.Add(new ChangeResourcesAction()
            {
                KeyName = "setSparseResources",


                AllResources = true,
                ExcludeResourceTypes = new[] { "stones", "firewood", "flint", "crop:waterCaneStem", "commonOilTubers" }, //MP I want no variance on these because they are essential to tutorial
                OperationToUse = ChangeResourcesAction.Operation.Multiply,
                //   NoiseAmplitude = 0.5f,
                NoiseParameters = new NoiseParams()
                   {
                       NoiseAddend = -0.4f, //minus 'X %'     MP may23, was: -0.2f                      
                       NoiseFrequency = 0.1f
                   }

            });

            #endregion

            #region Sites and Allegiances

            #region spawnWorld
            list.Add(
                new SpawnWorldAction()
                {
                    KeyName = "spawnWorld",

                    WorldData = new WorldData()
                    {
                        WorldRadius = GameData.Instance.Constants.DefaultWorldRadius,

                        ViewLongitudeStart = -5, // TODO...
                        ViewLongitudeEnd = 35,
                        ViewLatitudeStart = 40,
                        ViewLatitudeEnd = 85
                    }

                });

            #endregion

            list.Add(
                new SpawnSiteAction()
                {
                    KeyName = "spawnPlaySite",

                    SiteData = new SiteData()
                    {
                        Name = "TUTORIAL: Castaways",
                        KeyName = "playSite",
                        IsPlaySite = true,
                        ShowLabel = true,
                        ShowTallPin = true,
                        SiteMarkerOrder = 10
                    }


                });

            //

            list.Add(
                 new SpawnAllegianceAction()
                 {
                     KeyName = "spawnBushDragonTutAllegianceNorth",

                     Site = "playSite",

                     ExpeditionData = new ExpeditionData()
                     {
                         KeyName = "bushDragonTutAllegianceNorth",
                         Name = "bushDragonTutAllegianceNorth",
                         AllegianceKey = "bushDragonTutAllegianceNorth",
                         Location = new ValueNode()
                        {
                            Location = new Vector2(3504, 2458)
                        }
                     },

                     AllegianceData = new AllegianceData()
                     {

                         ForageAndHuntingRadius = 95,

                         Name = "Bush Dragon Allegiance",
                         KeyName = "bushDragonTutAllegianceNorth",
                         EntityType = "entity:bushDragon",
                         AllegianceType = Allegiances.AllegianceType.Other,
                         StatsData = new StatsData()
                         {
                             Security = 1f,
                             Comfort = 1f,
                             FoodSupply = 1f,
                         }
                     }

                 });



            list.Add(
                 new SpawnAllegianceAction()
                 {
                     KeyName = "spawnBushDragonTutAllegianceSouth",

                     Site = "playSite",

                     ExpeditionData = new ExpeditionData()
                     {
                         KeyName = "bushDragonTutAllegianceSouth",
                         Name = "bushDragonTutAllegianceSouth",
                         AllegianceKey = "bushDragonTutAllegianceSouth",
                         Location = new ValueNode()
                        {
                            Location = new Vector2(3456, 2688)
                        }
                     },

                     AllegianceData = new AllegianceData()
                     {

                         ForageAndHuntingRadius = 50,

                         Name = "Bush Dragon Allegiance",
                         KeyName = "bushDragonTutAllegianceSouth",
                         EntityType = "entity:bushDragon",
                         AllegianceType = Allegiances.AllegianceType.Other,
                         StatsData = new StatsData()
                         {
                             Security = 1f,
                             Comfort = 1f,
                             FoodSupply = 1f,
                         }
                     }

                 });


            list.Add(
                new SpawnAllegianceAction()
                {
                    KeyName = "spawnPlayerAllegiance",

                    Site = "playSite",
                    AllegianceData = new AllegianceData()
                    {
                        Name = "Player Allegiance",
                        KeyName = "playerAllegiance",
                        EntityType = "entity:human",
                        AllegianceType = Allegiances.AllegianceType.Player
                    }

                });


            #endregion


            #region Place impassable Crevice/Gorge


            list.Add(new SpawnEntityAction()
            {
                KeyName = "placeCrevice",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:crevice",
                    Name = "Impassable gorge",
                    Location = new Vector3(2976, 3528, 0)
                }


            });


            #endregion



            list.Add(new SetViewAction()
            {
                KeyName = "setView",
                DelayInSeconds = delayForItemsAndAgents,

                CenterOnLocation = new Vector2(240f, 0f),
                OffsetToLocation = new DynamicLocation()
                {
                    PropertyKey = "startingLocation"
                }

            });



            list.Add(new ExploreAction()
            {

                KeyName = "exploreShroudSouth",
                DelayInSeconds = delayForItemsAndAgents + 1, // must execute last, after members have been added

                DynamicLocationStart = new ValueNode()
                {
                    PropertyKey = "startingLocation"
                },
                RadiusStart = 300f,
                RadiusEnd = 500f,
                DetectMode = InGameEvents.Actions.DetectMode.DetectAlwaysSeenEntities,
                //PerformDetection = true,
                OffsetLocationEnd = new Microsoft.Xna.Framework.Vector2(3624f, 5904f),// 

                EntityToExploreWith = new InGameEvents.PropertyObjects.TargetObject()
                {
                    GetList = new InGameEvents.PropertyObjects.GetList()
                    {
                        // get a random allegiance member
                        HasPropertiesListKey = "allegiances",
                        FilterCondition = new InGameEvents.Conditions.PropertyCondition()
                        {
                            PropertyKey = "keyName",
                            ConstantStringEqual = "playerAllegiance"
                        },
                        //  FilterProperty = "name",
                        //  FilterValue = "playerAllegiance",
                        NextList = new InGameEvents.PropertyObjects.GetList()
                        {
                            HasPropertiesListKey = "persons" // returns all persons of player allegiance, then further up, the first item is picked implicitly
                        }
                    }
                }

                //  EntityToExploreWith = TargetEntityOfAction.SpecifiedEntity,
                //  EntityName = "Ward Conlan"

            });


            list.Add(new MusicAction()
            {
                KeyName = "stopMusic",

                StopMusic = true

            });


            // for testing:
            //       list.Add(ScenarioLoader.SpawnItemAtStartLocation("startSignalPyre", new Vector2(-24f, -68f), "structure:signalPyre", "playerAllegiance", null, delayForStructures, "Signal pyre"));

            #region Starting equipment



            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startKnife", new Vector2(8f, 4f), "item:advancedKnife", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startCookingPot", new Vector2(0f, 0f), "item:advancedCookingPot", "playerAllegiance", null, delayForItemsAndAgents));

            //for testing:
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startLines", new Vector2(0f, -24f), "item:lines", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startFirewood", new Vector2(0f, -24f), "item:firewood", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSpoakLeaves", new Vector2(0f, -24f), "item:spoakLeaves", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startcommonOilTubers", new Vector2(0f, -24f), "item:commonOilTubers", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startMetalWire", new Vector2(0f, -24f), "item:metalWire", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startImprovisedFlintSpear", new Vector2(0f, -24f), "item:improvisedFlintSpear", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startMashedcommonOilTubers", new Vector2(0f, -24f), "item:mashedcommonOilTubers", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startStones", new Vector2(0f, -24f), "item:stones", "playerAllegiance", null, delayForItemsAndAgents));

            //replaced with terrain catamaran:
            //         list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startCatamaranItem", new Vector2(153f, 70f), "item:catamaranStart", "playerAllegiance", null, delayForItemsAndAgents, "Catamaran start")); //(103f, 20f)
            //         list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startCatamaranMastItem", new Vector2(69f, 129f), "item:catamaranMastSail", "playerAllegiance", null, delayForItemsAndAgents, "Catamaran mast start")); //(19f, 79f)

            /*
                    //    list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSulfurSmokeBomb", new Vector2(124f, -24f), "item:sulfurSmokeBomb", "playerAllegiance", null, delayForItemsAndAgents)); // testsulfur!
                        list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSnips", new Vector2(124f, -24f), "item:advancedSnips", "playerAllegiance", null, delayForItemsAndAgents));
                        list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startString", new Vector2(124f, -24f), "item:advancedString", "playerAllegiance", null, delayForItemsAndAgents));
                        list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startThermalTarp", new Vector2(124f, -24f), "item:thermalTarp", "playerAllegiance", null, delayForItemsAndAgents));
                        list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startFireSuppressantCartridge", new Vector2(124f, -24f), "item:fireSuppressantCartridge", "playerAllegiance", null, delayForItemsAndAgents));
            */




            #endregion
            /* deleting this, doesn't look like it should be there?
            List<string> validationErrors;
            Dictionary<string, List<string>> allPostLoadContentValidationErrors = new Dictionary<string,List<string>>();
            foreach (var item in list)
            {
                validationErrors = new List<string>();
                allPostLoadContentValidationErrors.Add(item.KeyName, validationErrors);

                item.PostLoadContentValidate(ref validationErrors);
            }

            BaseDataLoader.DisplayAllValidationErrors(allPostLoadContentValidationErrors);
            */
            return list;


        }

    }
}
