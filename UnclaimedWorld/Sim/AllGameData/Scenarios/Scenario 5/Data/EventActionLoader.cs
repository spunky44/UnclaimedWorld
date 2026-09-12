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
using UWGame.SimSide.Entities;
using UWGame.ClientSide.Interface;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Trade;
using UWGame.SimSide.XmlCollections;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Systems;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_5.Data
{
    public class EventActionLoader
    {
        public static List<EventActionType> Init()
        {
            List<EventActionType> list = new List<EventActionType>();

            double delayForSpots = 0; // need to be before structures that reference them
            double delayForStructures = 0.25;
            double delayForItemsAndAgents = 0.5;
            double delayForProcesses = 0.6; // needs people to be spawned first to act as 'workers'
            double delayForUpgrades = 0.6; // after structures

            #region Init. // burials, meetings, emigration texts
            #region burials
            list.Add(new SetPropertyAction()
            {
                KeyName = "initBurialText1",
                PropertyKey = "burialTextStart",
                Value = new ValueNode() { String = " \n \n" }  //mp no header for the burial event, just some line breaks
            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "initBurialText2", //new burial texts for Headway.
                PropertyKey = "burialTextMultipleDeathsMultipleSurvivors",
                Value = new ValueNode() { String = "The town of Headway had seen its share of tragedies over the years. Everyone had hoped that those days were over, when suddenly they lost #NAMEOFDECEASED#CAUSEOFDEATH. At the burial, #EUOLOGYGIVER urged everyone to find meaning in the sacrifice that #NAMEOFDECEASED had made to rebuild their community." }
            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "initBurialText3", //mp seems we no longer have different texts for these 2 situations. (singleDeath / multiple deaths)
                PropertyKey = "burialTextSingleDeathMultipleSurvivors",
                Value = new ValueNode() { String = "The town of Headway had seen its share of tragedies over the years. Everyone had hoped that those days were over, when suddenly they lost #NAMEOFDECEASED#CAUSEOFDEATH. At the burial, #EUOLOGYGIVER urged everyone to find meaning in the sacrifice that #NAMEOFDECEASED had made to rebuild their community." }
            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "initBurialText4",
                PropertyKey = "burialTextSingleDeathSingleSurvivor",
                Value = new ValueNode() { String = "Losing #NAMEOFDECEASED came as a natural continuation of past tragedies more than a sudden shock. #EUOLOGYGIVER carried out the burial with as much dignity as possible and without reflecting on the past nor speculating on the future." }
            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "initBurialText5",
                PropertyKey = "burialTextMultipleDeathsSingleSurvivor",
                Value = new ValueNode() { String = "Losing #NAMEOFDECEASED came as a natural continuation of past tragedies more than a sudden shock. #EUOLOGYGIVER carried out the burial with as much dignity as possible and without reflecting on the past nor speculating on the future." }
            });
            #endregion

            list.Add(new SetPropertyAction()
            {
                KeyName = "initGameOver",
                PropertyKey = "gameOver",
                Value = new ValueNode() { Bool = false }
            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "initComfortTarget",
                PropertyKey = "comfortTarget",
                Value = new ValueNode() { Decimal = .40f }
            });
            list.Add(new SetPropertyAction()
            {
                KeyName = "initFoodTarget",
                PropertyKey = "foodTarget",
                Value = new ValueNode() { Decimal = .40f }
            });
            list.Add(new SetPropertyAction()
            {
                KeyName = "initSecurityTarget",
                PropertyKey = "securityTarget",
                Value = new ValueNode() { Decimal = .27f }
            });

            #region Group meeting dialog texts
            //
            list.Add(new SetPropertyAction()
            {
                KeyName = "initTimeBeforeGroupMeeting",
                PropertyKey = "timeInGameSecondsBeforeGroupMeeting",
                Value = new ValueNode() { Decimal = 200 }//hmm bigger delay for the tutorial? will it clash?
            });

            list.Add(new SetPropertyAction() //these 2 are used whereever it says #EMIGRATETHREAT - for the cases where all are unhappy, it uses "meetingEmigrateThreatAllUnhappy"
            {
                KeyName = "meetingEmigrateThreat",
                PropertyKey = "meetingEmigrateThreat",
                Value = new ValueNode() { String = "If not...well, I might leave for #EMIGRATETO and start over." }
            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "meetingEmigrateThreatAllUnhappy",
                PropertyKey = "meetingEmigrateThreatAllUnhappy",
                Value = new ValueNode() { String = "I would hate to see our town abandoned. But it seems like everyone thinks of leaving for #EMIGRATETO and starting over." }
            });

            #region 3 people -meetings
            list.Add(new SetPropertyAction()
            {
                KeyName = "meeting3Security",

                PropertyKey = "meeting3Security",
                Value = new ValueNode()
                {
                    String = "#UNHAPPY: Friends. I'm worried about security. I think we need to work entirely on security for awhile. \n \n" //
                                + "#CONTENT: We made a plan, remember? To match Eden Plains in every area. That means we focus on food and comfort as well. \n \n"
                                + "#UNHAPPY: Yes, I'm aware of the goals that were set. But our surroundings are dangerous. We have great whipjaws right here on our doorstep. Swamp men and megapods nearby! \n \n"
                                + "#CONTENT: Security is one of our priorities. \n \n"
                                + "#UNHAPPY: Look - if we don't get more guns soon, something bad will happen. So let's change priorities! #EMIGRATETHREAT \n \n"
                                + "#CONTENT: Alright. This has been noted. Anything else?"


                    /* 
                      //Alternative:
                                            String = "#UNHAPPY: What I want to say is, I'm worried about the security situation in this town. \n \n"
                                                        + "#CONTENT: Security is one of the areas we want to improve. \n \n"
                                                        + "#UNHAPPY: Yes, but it's not going quick enough. I'm terrified that we'll see someone get mauled by a great whipjaw or dragged down by the swamp man. So I really hope we can focus on security. #EMIGRATETHREAT \n \n"
                                                        + "#CONTENT: Let's make sure it doesn't come to that. Anyone has more to add?"
                     */
                }

            });



            list.Add(new SetPropertyAction() // 
            {
                KeyName = "meeting3Food",

                PropertyKey = "meeting3Food",
                Value = new ValueNode()
                {
                    String = "#UNHAPPY: I want to talk about our food stores. They are dangerously low. \n \n"
                                + "#CONTENT: Well, we already set a goal for the size of food stockpiles. \n \n"
                                + "#UNHAPPY: I know that it's part of the bigger plan of matching Eden Plains. But food supply needs much more attention! \n \n"
                                + "#CONTENT: You do notice that everyone here is working hard? \n \n"
                                + "#UNHAPPY: Yes. But we need to focus on preserving fish and staples. Right now, a minor event could cause us to starve. I hope you come to your senses! #EMIGRATETHREAT \n \n"
                                + "#CONTENT: Ok, we've heard your concerns. Anyone has something to add?"

                    /*
                     * //alternative:
                                            String = "#UNHAPPY: I'm concerned that we're not stockpiling enough food. \n \n"
                                                        + "#CONTENT: We are working hard to improve this town in every area and that includes food. \n \n"
                                                        + "#UNHAPPY: That's exactly the problem. We're focusing on too many things. Instead, we need more smoked meat and fish, dry staples...I don't want to see starvation happen here, so please, focus on this. #EMIGRATETHREAT  \n \n"
                                                        + "#CONTENT: Ok, we've noted down these concerns. Who else has anything to add?"
                     */
                }

            });


            list.Add(new SetPropertyAction() //
            {
                KeyName = "meeting3Comfort",

                PropertyKey = "meeting3Comfort",
                Value = new ValueNode()
                {
                    String = "#UNHAPPY: I'll get right to it: This town is not a comfortable place to live. Far from it. \n \n"
                                + "#CONTENT: Everyone here is working hard to improve the place. \n \n"
                                + "#UNHAPPY: Look. We're putting enormous emphasis on stockpiling food and weapons. Why? \n \n"
                                + "#CONTENT: Because those areas are also part of the goal that we set. \n \n"
                                + "#UNHAPPY: Yeah, but maybe those goals need to be revised. I'm getting fed up with the squalor here. Let's focus attention on our houses and at least get some minor enjoyment. #EMIGRATETHREAT \n \n" //and well-being
                                + "#CONTENT: Sad to hear that you're not happy here. Anyone else has complaints they'd like to share?"

                    /*
                     //alternative:
                                            String = "#UNHAPPY: I'm not impressed by the living standards in this town. Why are we working so hard on stockpiling food and weapons? \n \n"
                                                        + "#CONTENT: Well, we set out to match what they have in Eden Plains. \n \n"
                                                        + "#UNHAPPY: Yeah, but maybe those goals need to be revised. I'm getting fed up with the squalor here, so I hope we can start improving our houses and at least get some minor enjoyment. #EMIGRATETHREAT \n \n" //
                                                        + "#CONTENT: Sad to hear that you're not happy here. Anyone else has complaints they'd like to talk about?"
                     */
                }

            });
            #endregion
            #region 2 people - meetings
            list.Add(new SetPropertyAction()
            {
                KeyName = "meeting2Security",

                PropertyKey = "meeting2Security",
                Value = new ValueNode()
                {
                    String = "-#UNHAPPY: I've tried to convince you before. This is getting out of hand. We need to improve security. \n \n"
                                + "-#CONTENT: I don't see... \n \n"
                                + "-#UNHAPPY: Listen. We're only two people left. We need to look out for each other. We need to get better weapons, take fewer risks... right now! I hope you understand! #EMIGRATETHREAT \n \n"
                                + "-#CONTENT: I don't know what to say. I think there are so many other things that are more important."
                }

            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "meeting2Food",

                PropertyKey = "meeting2Food",
                Value = new ValueNode()
                {
                    String = "#UNHAPPY: We need to stock up on food. \n \n"
                                + "#CONTENT: Yeah, you keep saying this, but I think you're obsessing over something trivial. We have enough to eat. \n \n"
                                + "#UNHAPPY: We're only two people left here. I don't want us to fight over food. So please, see reason. #EMIGRATETHREAT \n \n"

                }

            });

            list.Add(new SetPropertyAction() //
            {
                KeyName = "meeting2Comfort",

                PropertyKey = "meeting2Comfort",
                Value = new ValueNode()
                {
                    String = "#UNHAPPY: We need better living conditions. \n \n"
                                + "#CONTENT: We have all we need. \n \n"
                                + "#UNHAPPY: Look, the conditions here are horrible, even for two people. Please, let's work on making life more tolerable. #EMIGRATETHREAT \n \n"
                }

            });
            #endregion
            #region All unhappy - meetings
            list.Add(new SetPropertyAction()
            {
                KeyName = "meetingSecurityAllUnhappy",

                PropertyKey = "meetingSecurityAllUnhappy",
                Value = new ValueNode()
                {
                    String = "#UNHAPPY:  Look, we all want the security situation to improve! So let's get our act together and start working together! #EMIGRATETHREAT"
                }

            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "meetingFoodAllUnhappy",

                PropertyKey = "meetingFoodAllUnhappy",
                Value = new ValueNode()
                {
                    String = "#UNHAPPY:  Look, we all want the food situation to improve! So let's get our act together and start working together! #EMIGRATETHREAT"
                }

            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "meetingComfortAllUnhappy",

                PropertyKey = "meetingComfortAllUnhappy",
                Value = new ValueNode()
                {
                    String = "#UNHAPPY:  Look, we all want the comfort conditions to improve! So let's get our act together and start working together! #EMIGRATETHREAT"
                }

            });
            #endregion
            #endregion

            #region Emigrate dialog texts

            //
            list.Add(new SetPropertyAction()
            {
                KeyName = "initEmigrateSecurityDialogText",
                PropertyKey = "securityEmigrateEventDialogText",
                Value = new ValueNode() { String = "AUDIO LOG, #JOURNALDATE \n \n#NAME1: Security around here is appalling. I can't believe the risks that we're taking. \n \n#NAME2: Is it about the whipjaw? The megapods? We can deal with them. \n \n#NAME1: When an animal attack happens - AND IT WILL! I'm not going to be around. I'm leaving now. You can find me at #EMIGRATIONTARGET if you want to get in touch." } //followed up with some banter.
            });

            // 
            list.Add(new SetPropertyAction()
            {
                KeyName = "initEmigrateSecurityNoConversationDialogText",
                PropertyKey = "securityEmigrateEventNoConversationDialogText",
                Value = new ValueNode() { String = "TEXT LOG, #JOURNALDATE \n \n#NAME1: When you read this, I'll be leaving. We're in danger from wild animals here, but I seem to be the only one who takes this threat seriously. I'm going to #EMIGRATIONTARGET where I'll be safe. \nGoodbye." }
            });


            list.Add(new SetPropertyAction()
            {
                KeyName = "initEmigrateComfortDialogText",
                PropertyKey = "comfortEmigrateEventDialogText",
                Value = new ValueNode() { String = "AUDIO LOG, #JOURNALDATE \n \n#NAME1: I've had it with the poor housing here. Also, there's no enjoyment to be had. \n \n#NAME2: Oh yeah? You have higher standards? \n \n#NAME1: I've endured enough. I'm fed up. I'm leaving for #EMIGRATIONTARGET. I can do better on my own." } //followed up with some banter.
            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "initEmigrateComfortNoConversationDialogText",
                PropertyKey = "comfortEmigrateEventNoConversationDialogText",
                Value = new ValueNode() { String = "TEXT LOG, #JOURNALDATE \n \n#NAME1: This is my final goodbye. I'm fed up with this squalor. These conditions here, they're way below my limits. I'm going to #EMIGRATIONTARGET. I can do better on my own." }
            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "initEmigrateFoodDialogText",
                PropertyKey = "foodEmigrateEventDialogText",
                Value = new ValueNode() { String = "AUDIO LOG, #JOURNALDATE \n \n#NAME1: I can't watch this anymore. Can't you see we're this close to starvation? \n \n#NAME2: Hey! We're working hard in a lot of areas! \n#NAME1: What could be more important than food?! I've seen starvation before and I'm not about to witness it again. I'm going to #EMIGRATIONTARGET." }//followed up with some banter.
            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "initEmigrateFoodNoConversationDialogText",
                PropertyKey = "foodEmigrateEventNoConversationDialogText",
                Value = new ValueNode() { String = "TEXT LOG, #JOURNALDATE \n \n#NAME1: When you read this, I'll be on my way. This constant fretting about food and waiting for an inevitable disaster has me worried sick. I'm going to #EMIGRATIONTARGET. Don't come knocking when your food runs out." }
            });

            #endregion
            #endregion



            #region startNeeds. Used for all characters when they appear.
            SerializableDictionary<string, NeedData> startNeeds = new SerializableDictionary<string, NeedData>() 
            {
                { "protein", new NeedData(){ Level = new NormalDistribution() { Mean = 0.9f, StandardDeviation = 0.02f } }},
                { "foodEnergy", new NeedData(){ Level = new NormalDistribution() { Mean = 0.9f, StandardDeviation = 0.02f } }},
                { "micronutrients", new NeedData(){ Level = new NormalDistribution() { Mean = 0.9f, StandardDeviation = 0.02f } }},
                { "stimulants", new NeedData(){ Level = new NormalDistribution() { Mean = 0.6f, StandardDeviation = 0.02f } }}
            };
            #endregion
            #region //ON- AND OFFMAP SITES AND ALLEGIANCES

            #region spawnWorld
            list.Add(
                new SpawnWorldAction()
                {
                    KeyName = "spawnWorld",

                    WorldData = new WorldData()
                    {
                        WorldRadius = GameData.Instance.Constants.DefaultWorldRadius,
                        //map gui: w:527px, h: 442px

                        ViewLongitudeStart = 16, // w 
                        ViewLongitudeEnd = 20,
                        ViewLatitudeStart = 69, // h
                        ViewLatitudeEnd = 72
                    }

                });

            #endregion

            // Playsite:
            #region spawnPlaySite
            list.Add(
                new SpawnSiteAction()
                {
                    KeyName = "spawnPlaySite",

                    SiteData = new SiteData()
                    {
                        Name = "Headway", 
                        KeyName = "playSite",
                        Description = "For many years our town was a thriving farm community until events forced us to find other ways to earn a living.",
                        Coords = new Overland.Locations.GeodeticCoordinate(17.72d, 70.65d), //make sure they can travel by boat in straight line to and from. //Latitude = , Longitude =  },
                        IsPlaySite = true,
                        ShowLabel = true,
                        ShowTallPin = false,
                        SiteMarkerOrder = 10
                    }


                });
            #endregion
            #region spawnPlayerAllegiance, setPlayerCredits
            list.Add(
                new SpawnAllegianceAction()
                {
                    KeyName = "spawnPlayerAllegiance",

                    Site = "playSite",
                    AllegianceData = new AllegianceData()
                    {

                        Name = "Headway", //"Player Allegiance",
                        KeyName = "playerAllegiance",
                        EntityType = "entity:human",
                        AllegianceType = Allegiances.AllegianceType.Player,
                        StatsData = new StatsData()
                        /*    {
                                Security = 1f,
                                Comfort = 1f,
                                FoodSupply = 1f,
                            }*/
                    }

                });

            list.Add(
                new ChangeCreditsAction()
                {
                    KeyName = "setPlayerCredits",

                    AllegianceKey = "playerAllegiance",
                    Amount = new ValueNode()
                    {
                        Decimal = 240 //enough for "item:extrusionMachineComponents" + 1st transport 
                    }

                });
            #endregion

            //Othersite1. Mining town for buying ore and minerals (sulfur) and metal goods:
            #region spawnOtherSite1
            list.Add(
                new SpawnSiteAction()
                {
                    KeyName = "spawnOtherSite1",

                    SiteData = new SiteData()
                    {
                        Name = "Zenig Station", //
                        KeyName = "otherSite1",
                        Description = "A site with a booming mining and metalworking industry. Sells minerals and tools and buys rubber and foodstuff.",
                        Coords = new Overland.Locations.GeodeticCoordinate(17.25d, 69.6d),
                        IsPlaySite = false
                    }

                });
            #endregion
            #region spawnOtherSite1Allegiance1
            list.Add(
                new SpawnAllegianceAction()
                {
                    KeyName = "spawnOtherSite1Allegiance1",

                    Site = "otherSite1",
                    AllegianceData = new AllegianceData()
                    {
                        Name = "Zenig Station",
                        KeyName = "otherSite1Allegiance1",
                        EntityType = "entity:human",
                        AllegianceType = Allegiances.AllegianceType.Other,
                        StatsData = new StatsData()
                        {
                            Security = .41f,
                            Comfort = .52f,
                            FoodSupply = .30f,
                        }
                    }

                });
            #endregion
            #region spawnOtherSite1Expedition1 
            list.Add(new CreateExpeditionAction()
            {
                KeyName = "spawnOtherSite1Expedition1",// zenig station
                DelayInSeconds = 0.1, // wait for starting location property to have been set!
                //   ironville  : deals in minerals, ores, steel tools and firearms     
                ExpeditionData = new ExpeditionData()
                {
                    KeyName = "otherSite1Expedition1",//not player's site.
                    Name = "The Wharf",
                    SizeFactor = 1.2f,
                    AllegianceKey = "otherSite1Allegiance1",
                    TradeProfile = "industryTradeProfile",
                    AvailableForTrade = new SerializableDictionary<string, TradeAmountType>() //override, part of tut/scenario:
                    {
                        { "item:gaskets", new TradeAmountType() { StartAmount = new NormalDistribution(){ Mean = 30 }, MaxAmountForSale = 0,  AmountToBuy = 30, MaxAmountToBuy = 30, LinearConsumptionPerDay = 6,  } },
                        { "item:sulfurPowder", new TradeAmountType() { StartAmount = new NormalDistribution() { Mean = 12 }, MaxAmountForSale = 12 } } 
                    },                   
                    VehiclesProfile = "bargeProfile",
                    StructuresProfile = "largePierProfile",
                    PricesProfile = "descentEraPrices"
                }
            });
            #endregion           
            #region OtherSite1 Relations
            list.Add(
                new SpawnAllegianceRelationAction()
                {
                    KeyName = "spawnFriendlyOtherSite1Allegiance1Relation",
                    DelayInSeconds = 1,

                    AllegianceRelationData = new AllegianceRelationData()
                    {
                        Allegiance1 = "playerAllegiance",
                        Allegiance2 = "otherSite1Allegiance1",
                        Relation = 1f
                    }

                });

            list.Add(
                new SpawnAllegianceRelationAction()
                {
                    KeyName = "spawnNeutralOtherSite1Allegiance1Relation",
                    DelayInSeconds = 1,

                    AllegianceRelationData = new AllegianceRelationData()
                    {
                        Allegiance1 = "playerAllegiance",
                        Allegiance2 = "otherSite1Allegiance1",
                        Relation = .5f
                    }

                });

            list.Add(
                new SpawnAllegianceRelationAction()
                {
                    KeyName = "spawnHostileOtherSite1Allegiance1Relation",
                    DelayInSeconds = 1,

                    AllegianceRelationData = new AllegianceRelationData()
                    {
                        Allegiance1 = "playerAllegiance",
                        Allegiance2 = "otherSite1Allegiance1",
                        Relation = 0f
                    }

                });

            #endregion

            #region Route to otherSite1
            list.Add(
                new SpawnRouteAction()
                {
                    KeyName = "spawnPlaySiteSite1Route",
                    DelayInSeconds = 1,

                    RouteData = new RouteData()
                    {
                        Name = "Batten Creek",
                        FromSite = "playSite",
                        ToSite = "otherSite1",
                        Length = 145, //twice as long as eden plains  //was:a bit longer than the one to Eden Plains
                        RouteType = RouteType.CalmWater
                    }

                });

            #endregion

      
            #region OtherSite1 Possible Immigrants

            #region spawnImmigrantSmithingSpecialist
            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnImmigrantOtherSite1SmithingSpecialist",
                DelayInSeconds = delayForItemsAndAgents,

                // DynamicLocation = new DynamicLocation()   PropertyKey = "startingLocation"  },
                EntityData = new Maps.MapEditor.EntityData()
                {
                    //  Location = new Microsoft.Xna.Framework.Vector3(170f, -30f, 0),
                    EntityKey = "entity:human",
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "otherSite1Allegiance1",
                        ExpeditionKey = "otherSite1Expedition1"
                    },
                    Person = new Maps.MapEditor.Person()
                    {
                        PersonalityType = "survivalTierPersonality",

                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {
                        TraitTemplates = new StringChance[] {                                  
                                new StringChance() { Edge = 1f, String = "smithingSpecialist" } },

                        #region CultureTemplates. Future, 100% equality
                        CultureTemplates = new StringChance[] {
                                //equal distribution:
                                new StringChance() { Edge = 0.125f, String = "maleDescendantWhiteCulture2" },
                                new StringChance() { Edge = 0.25f, String = "maleDescendantAsianCulture2" }, 
                                new StringChance() { Edge = 0.375f, String = "maleDescendantHispanicCulture2" },
                                new StringChance() { Edge = 0.5f, String = "maleDescendantBlackCulture2" },  

                                new StringChance() { Edge = 0.75f, String = "femaleDescendantWhiteCulture2" },                                
                                new StringChance() { Edge = 1f, String = "femaleDescendantBlackCulture2" },                         
                            }
                        #endregion
                    },
                    NeedLevels = startNeeds
                }

            });
            #endregion
            #region spawnImmigrantOtherSite1MenialSpecialist
            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnImmigrantOtherSite1MenialSpecialist",
                DelayInSeconds = delayForItemsAndAgents,

                // DynamicLocation = new DynamicLocation()   PropertyKey = "startingLocation"  },
                EntityData = new Maps.MapEditor.EntityData()
                {
                    //  Location = new Microsoft.Xna.Framework.Vector3(170f, -30f, 0),
                    EntityKey = "entity:human",
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "otherSite1Allegiance1",
                        ExpeditionKey = "otherSite1Expedition1"
                    },
                    Person = new Maps.MapEditor.Person()
                    {
                        PersonalityType = "survivalTierPersonality", //

                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {
                        TraitTemplates = new StringChance[] {                                  
                                new StringChance() { Edge = 1f, String = "menialSpecialist" } },

                        #region CultureTemplates. Future, 100% equality
                        CultureTemplates = new StringChance[] {
                                //equal distribution:
                                new StringChance() { Edge = 0.125f, String = "maleDescendantWhiteCulture2" },
                                new StringChance() { Edge = 0.25f, String = "maleDescendantAsianCulture2" }, 
                                new StringChance() { Edge = 0.375f, String = "maleDescendantHispanicCulture2" },
                                new StringChance() { Edge = 0.5f, String = "maleDescendantBlackCulture2" },  

                                new StringChance() { Edge = 0.75f, String = "femaleDescendantWhiteCulture2" },                                
                                new StringChance() { Edge = 1f, String = "femaleDescendantBlackCulture2" },                         
                            }
                        #endregion
                    },
                    NeedLevels = startNeeds

                }

            });
            #endregion
            #region spawnImmigrantOtherSite1Random
            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnImmigrantOtherSite1Random",
                DelayInSeconds = delayForItemsAndAgents,

                // DynamicLocation = new DynamicLocation()   PropertyKey = "startingLocation"  },
                EntityData = new Maps.MapEditor.EntityData()
                {
                    //  Location = new Microsoft.Xna.Framework.Vector3(170f, -30f, 0),
                    EntityKey = "entity:human",
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "otherSite1Allegiance1",
                        ExpeditionKey = "otherSite1Expedition1"
                    },
                    Person = new Maps.MapEditor.Person()
                    {
                        PersonalityType = "randomTierPersonality",

                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {
                        #region TraitTemplates.  100% random
                        TraitTemplates = new StringChance[] { 
                                new StringChance() { Edge = 0.035f, String = "chemistrySpecialist" }, 
                                new StringChance() { Edge = 0.07f, String = "mechanicsSpecialist" }, 
                                new StringChance() { Edge = 0.11f, String = "electronicsSpecialist" },     
                                new StringChance() { Edge = 0.22f, String = "smithingSpecialist" },                           
                                new StringChance() { Edge = 0.33f, String = "farmingSpecialist" }, 
                                new StringChance() { Edge = 0.44f, String = "constructionSpecialist" },                             
                                new StringChance() { Edge = 0.55f, String = "huntingSpecialist" }, 
                                new StringChance() { Edge = 0.66f, String = "menialSpecialist" },                           
                                new StringChance() { Edge = 0.77f, String = "cookingSpecialist" }, 
                                new StringChance() { Edge = 0.88f, String = "bushcraftSpecialist" }, 
                                new StringChance() { Edge = 1f, String = "securitySpecialist" }, 
                           
                            },
                        #endregion
                        #region CultureTemplates. Future, 100% equality
                        CultureTemplates = new StringChance[] {
                                //equal distribution:
                                new StringChance() { Edge = 0.125f, String = "maleDescendantWhiteCulture2" },
                                new StringChance() { Edge = 0.25f, String = "maleDescendantAsianCulture2" }, 
                                new StringChance() { Edge = 0.375f, String = "maleDescendantHispanicCulture2" },
                                new StringChance() { Edge = 0.5f, String = "maleDescendantBlackCulture2" },  

                                new StringChance() { Edge = 0.75f, String = "femaleDescendantWhiteCulture2" },                                
                                new StringChance() { Edge = 1f, String = "femaleDescendantBlackCulture2" },                         
                            }
                        #endregion
                    },
                    NeedLevels = startNeeds

                }

            });
            #endregion

            #endregion


            //////////////////////////////////////////////////////////////////////////////////////////////
            //Othersite2. FARMING TOWN for buying agricultural products:
            #region spawnOtherSite2
            list.Add(
                new SpawnSiteAction()
                {
                    KeyName = "spawnOtherSite2",

                    SiteData = new SiteData()
                    {
                        Name = "Eden Plains", //
                        KeyName = "otherSite2",
                        Description = "A well-developed farm village. The farmers here sell their crops and occasionally buy tools.",
                        Coords = new Overland.Locations.GeodeticCoordinate(18.2d, 71.18d), //want to make transport shorter. was 18.6d, 71.28d
                        IsPlaySite = false,
                                                 
                    }

                });
            #endregion
            #region spawnOtherSite2Allegiance1
            list.Add(
                new SpawnAllegianceAction()
                {
                    KeyName = "spawnOtherSite2Allegiance1",

                    Site = "otherSite2",
                    AllegianceData = new AllegianceData()
                    {
                        Name = "Eden Plains",
                        KeyName = "otherSite2Allegiance1",
                        EntityType = "entity:human",
                        AllegianceType = Allegiances.AllegianceType.Other,
                        StatsData = new StatsData()//these stats are used as win condition. sync the GOALS numbers for "introDialogueScreen", "spawnOtherSite2Allegiance1" and "winGame"
                        {
                            /* FoodSupply = .40f,//was FoodSupply = .32f,
                             Security = .27f, //was Security = .39f
                             Comfort = .40f, //was Comfort = .46f,
                            */
                            DynamicFood = new ValueNode() { PropertyKey = "foodTarget" },
                            DynamicSecurity = new ValueNode() { PropertyKey = "securityTarget" },
                            DynamicComfort = new ValueNode() { PropertyKey = "comfortTarget" }

                        }
                    }

                });
            #endregion
            #region spawnOtherSite2Expedition1 
            list.Add(new CreateExpeditionAction()
            {
                KeyName = "spawnOtherSite2Expedition1",// not player's site.
                DelayInSeconds = 0.1, // wait for starting location property to have been set!

                ExpeditionData = new ExpeditionData()
                {
                    KeyName = "otherSite2Expedition1",//not player's site.
                    Name = "East Wharf", // 
                    SizeFactor = 1.2f,
                    AllegianceKey = "otherSite2Allegiance1",
                    TradeProfile = "farmingTradeProfile",
                    AvailableForTrade = new SerializableDictionary<string, TradeAmountType>() //override, part of tut/scenario:
                    {
                        { "item:extrusionMachineComponents", new TradeAmountType() { StartAmount = new NormalDistribution() { Mean = 1 }, MaxAmountForSale = 1 } },
                        { "item:sulfurPowder", new TradeAmountType() { StartAmount = new NormalDistribution() { Mean = 12 }, MaxAmountForSale = 12 } } 
                    },
                    VehiclesProfile = "bargeProfile",
                    StructuresProfile = "mediumPierProfile",
                    PricesProfile = "descentEraPrices"
                }
            });
            #endregion
           
            #region OtherSite2 Relations
            list.Add(
                new SpawnAllegianceRelationAction()
                {
                    KeyName = "spawnFriendlyOtherSite2Allegiance1Relation",
                    DelayInSeconds = 1,

                    AllegianceRelationData = new AllegianceRelationData()
                    {
                        Allegiance1 = "playerAllegiance",
                        Allegiance2 = "otherSite2Allegiance1",
                        Relation = 1f
                    }

                });

            list.Add(
                new SpawnAllegianceRelationAction()
                {
                    KeyName = "spawnNeutralOtherSite2Allegiance1Relation",
                    DelayInSeconds = 1,

                    AllegianceRelationData = new AllegianceRelationData()
                    {
                        Allegiance1 = "playerAllegiance",
                        Allegiance2 = "otherSite2Allegiance1",
                        Relation = .5f
                    }

                });

            list.Add(
                new SpawnAllegianceRelationAction()
                {
                    KeyName = "spawnHostileOtherSite2Allegiance1Relation",
                    DelayInSeconds = 1,

                    AllegianceRelationData = new AllegianceRelationData()
                    {
                        Allegiance1 = "playerAllegiance",
                        Allegiance2 = "otherSite2Allegiance1",
                        Relation = 0f
                    }

                });

            #endregion

            #region Route to otherSite2
            list.Add(
                new SpawnRouteAction()
                {
                    KeyName = "spawnPlaySiteSite2Route",
                    DelayInSeconds = 1,

                    RouteData = new RouteData()
                    {
                        Name = "Ritchel's Strait",
                        FromSite = "playSite",
                        ToSite = "otherSite2",
                        Length = 70, //   now: half as long as headway to zenig station   was: 120
                        RouteType = RouteType.CalmWater
                    }

                });

            #endregion
                       
            #region OtherSite2 Possible Immigrants
            #region spawnImmigrantOtherSite2Chemist1
            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnImmigrantOtherSite2Chemist1",
                DelayInSeconds = delayForItemsAndAgents,

                // DynamicLocation = new DynamicLocation()   PropertyKey = "startingLocation"  },
                EntityData = new Maps.MapEditor.EntityData()
                {
                    //  Location = new Microsoft.Xna.Framework.Vector3(170f, -30f, 0),
                    EntityKey = "entity:human",
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "otherSite2Allegiance1",
                        ExpeditionKey = "otherSite2Expedition1"
                    },
                    Person = new Maps.MapEditor.Person()
                    {
                        FirstName = "Jane", //a family of chemists..
                        LastName = "Neson",
                        PersonalityType = "earlyJoinerPersonality", //guaranteed: early joiner!!
                        Portrait = "human_w_f_adult_1"
                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {
                        AgeInYears = new NormalDistribution() { Mean = 40f },
                        CasteKey = "female",
                        ModelTextureName = "ManGreenSolid1Texture", //uniforms
                        RaceKey = "whiteHumanDescendant",
                        TraitTemplates = new StringChance[] { 
                                new StringChance() {  String = "chemistrySpecialist" }, 
                                },
                    },
                    NeedLevels = startNeeds
                }

            });
            #endregion
            #region spawnImmigrantOtherSite2Chemist2
            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnImmigrantOtherSite2Chemist2",
                DelayInSeconds = delayForItemsAndAgents,

                // DynamicLocation = new DynamicLocation()   PropertyKey = "startingLocation"  },
                EntityData = new Maps.MapEditor.EntityData()
                {
                    //  Location = new Microsoft.Xna.Framework.Vector3(170f, -30f, 0),
                    EntityKey = "entity:human",
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "otherSite2Allegiance1",
                        ExpeditionKey = "otherSite2Expedition1"
                    },
                    Person = new Maps.MapEditor.Person()
                    {
                        FirstName = "Roy", //a family of chemists..
                        LastName = "Neson",
                        PersonalityType = "survivalTierPersonality", //not necessarily early joiner!
                        Portrait = "human_w_m_adult_1"
                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {
                        AgeInYears = new NormalDistribution() { Mean = 26f },
                        CasteKey = "male",
                        ModelTextureName = "ManGreenSolid1Texture", //uniforms
                        RaceKey = "whiteHumanDescendant",
                        TraitTemplates = new StringChance[] { 
                                new StringChance() {  String = "chemistrySpecialist" }, 
                                },
                    },
                    NeedLevels = startNeeds
                }

            });
            #endregion
            #region spawnImmigrantOtherSite2Chemist2
            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnImmigrantOtherSite2Chemist3",
                DelayInSeconds = delayForItemsAndAgents,

                // DynamicLocation = new DynamicLocation()   PropertyKey = "startingLocation"  },
                EntityData = new Maps.MapEditor.EntityData()
                {
                    //  Location = new Microsoft.Xna.Framework.Vector3(170f, -30f, 0),
                    EntityKey = "entity:human",
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "otherSite2Allegiance1",
                        ExpeditionKey = "otherSite2Expedition1"
                    },
                    Person = new Maps.MapEditor.Person()
                    {
                        FirstName = "Ben", //a family of chemists..
                        LastName = "Neson",
                        PersonalityType = "survivalTierPersonality", //not necessarily early joiner!
                        Portrait = "human_w_m_adult_1"
                    },
                    //  SimulateJoinedExpeditionNow = true,
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {
                        AgeInYears = new NormalDistribution() { Mean = 29f },
                        CasteKey = "male",
                        ModelTextureName = "ManGreenSolid1Texture", //uniforms
                        RaceKey = "whiteHumanDescendant",
                        TraitTemplates = new StringChance[] { 
                                new StringChance() {  String = "chemistrySpecialist" }, 
                                },
                    },
                    NeedLevels = startNeeds
                }

            });
            #endregion
            #region spawnImmigrantOtherSite2MenialSpecialist
            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnImmigrantOtherSite2MenialSpecialist",
                DelayInSeconds = delayForItemsAndAgents,

                // DynamicLocation = new DynamicLocation()   PropertyKey = "startingLocation"  },
                EntityData = new Maps.MapEditor.EntityData()
                {
                    //  Location = new Microsoft.Xna.Framework.Vector3(170f, -30f, 0),
                    EntityKey = "entity:human",
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "otherSite2Allegiance1",
                        ExpeditionKey = "otherSite2Expedition1"
                    },
                    Person = new Maps.MapEditor.Person()
                    {
                        PersonalityType = "survivalTierPersonality", //

                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {
                        TraitTemplates = new StringChance[] {                                  
                                new StringChance() { Edge = 1f, String = "menialSpecialist" } },

                        #region CultureTemplates. Future, 100% equality
                        CultureTemplates = new StringChance[] {
                                //equal distribution:
                                new StringChance() { Edge = 0.125f, String = "maleDescendantWhiteCulture2" },
                                new StringChance() { Edge = 0.25f, String = "maleDescendantAsianCulture2" }, 
                                new StringChance() { Edge = 0.375f, String = "maleDescendantHispanicCulture2" },
                                new StringChance() { Edge = 0.5f, String = "maleDescendantBlackCulture2" },  

                                new StringChance() { Edge = 0.75f, String = "femaleDescendantWhiteCulture2" },                                
                                new StringChance() { Edge = 1f, String = "femaleDescendantBlackCulture2" },                         
                            }
                        #endregion
                    },
                    NeedLevels = startNeeds

                }

            });
            #endregion
            #region spawnImmigrantOtherSite2Random
            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnImmigrantOtherSite2Random",
                DelayInSeconds = delayForItemsAndAgents,

                // DynamicLocation = new DynamicLocation()   PropertyKey = "startingLocation"  },
                EntityData = new Maps.MapEditor.EntityData()
                {
                    //  Location = new Microsoft.Xna.Framework.Vector3(170f, -30f, 0),
                    EntityKey = "entity:human",
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "otherSite2Allegiance1",
                        ExpeditionKey = "otherSite2Expedition1"
                    },
                    Person = new Maps.MapEditor.Person()
                    {
                        PersonalityType = "randomTierPersonality",

                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {
                        #region TraitTemplates.  100% random
                        TraitTemplates = new StringChance[] { 
                                new StringChance() { Edge = 0.035f, String = "chemistrySpecialist" }, 
                                new StringChance() { Edge = 0.07f, String = "mechanicsSpecialist" }, 
                                new StringChance() { Edge = 0.11f, String = "electronicsSpecialist" },   
                                new StringChance() { Edge = 0.22f, String = "smithingSpecialist" },                           
                                new StringChance() { Edge = 0.33f, String = "farmingSpecialist" }, 
                                new StringChance() { Edge = 0.44f, String = "constructionSpecialist" },                             
                                new StringChance() { Edge = 0.55f, String = "huntingSpecialist" }, 
                                new StringChance() { Edge = 0.66f, String = "menialSpecialist" },                           
                                new StringChance() { Edge = 0.77f, String = "cookingSpecialist" }, 
                                new StringChance() { Edge = 0.88f, String = "bushcraftSpecialist" }, 
                                new StringChance() { Edge = 1f, String = "securitySpecialist" }, 
                           
                            },
                        #endregion
                        #region CultureTemplates. Future, 100% equality
                        CultureTemplates = new StringChance[] {
                                //equal distribution:
                                new StringChance() { Edge = 0.125f, String = "maleDescendantWhiteCulture2" },
                                new StringChance() { Edge = 0.25f, String = "maleDescendantAsianCulture2" }, 
                                new StringChance() { Edge = 0.375f, String = "maleDescendantHispanicCulture2" },
                                new StringChance() { Edge = 0.5f, String = "maleDescendantBlackCulture2" },  

                                new StringChance() { Edge = 0.75f, String = "femaleDescendantWhiteCulture2" },                                
                                new StringChance() { Edge = 1f, String = "femaleDescendantBlackCulture2" },                         
                            }
                        #endregion
                    },
                    NeedLevels = startNeeds

                }

            });
            #endregion

            #endregion

            ////////////////////////////////////////////
            //Wilderness
            #region spawnWildernessSite1

            list.Add(
               new SpawnSiteAction()
               {
                   KeyName = "spawnWildernessSite1",

                   //   DistanceFromPlaySite = 9f, // 9 km away MP uncomment this to test distance. ('yelling' distance is below 10 km.) (comment out the coords below when testing)
                   //    BearingFromPlaySite = 2f, // MP: DOES NOT WORK // radians

                   SiteData = new SiteData()
                   {
                       Name = "Bird Hill",
                       KeyName = "wildernessSite1",
                       Coords = new Overland.Locations.GeodeticCoordinate(17.83d, 70.73d),
                       IsPlaySite = false,
                       ShowLabel = false,
                       ShowTallPin = true, //...........this site always sorts on top of the other one, apparently.
                   }


               });

            list.Add(
               new SpawnAllegianceAction()
               {
                   KeyName = "spawnWildernessSite1Allegiance1",

                   Site = "wildernessSite1",
                   AllegianceData = new AllegianceData()
                   {
                       Name = "Bird Hill",
                       KeyName = "wildernessSite1Allegiance1",
                       EntityType = "entity:human",
                       AllegianceType = Allegiances.AllegianceType.Other,
                       PermitsImmigration = true,
                       StatsData = new StatsData()
                       {
                           Security = 0f,
                           Comfort = 0f,
                           FoodSupply = 0.1f // corresponds to BaseLine for food when 0 stockpile. Makes sense, since noone have starved here (yet)
                       }
                   }

               });

            list.Add(new CreateExpeditionAction()
            {
                KeyName = "spawnWildernessSite1Expedition1",
                DelayInSeconds = 0.1, // wait for starting location property to have been set!

                ExpeditionData = new ExpeditionData()
                {
                    KeyName = "wildernessSite1Expedition1",
                    Name = "Camp", // 
                    AllegianceKey = "wildernessSite1Allegiance1",
                }

            });
            #endregion
            #region Route to Wilderness site
            list.Add(
                new SpawnRouteAction()
                {
                    KeyName = "spawnPlaySiteWildernessSite1Route",
                    DelayInSeconds = 1,

                    RouteData = new RouteData()
                    {
                        Name = "Road to Bird Hill", //
                        FromSite = "playSite",
                        ToSite = "wildernessSite1",
                        Length = 10,
                        RouteType = RouteType.Land
                    }

                });
            #endregion

            #endregion
            //GAME AREA:
            #region Characters



            #region spawnMillet //mentioned in story.  electronicsSpecialist
            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnMillet",
                DelayInSeconds = delayForItemsAndAgents + 1, // add one second so they dont all exit building at same time

                /*      DynamicLocation = new DynamicLocation()
                      {
                          PropertyKey = "startingLocation"
                      },*/
                EntityData = new Maps.MapEditor.EntityData()
                {
                    Location = new Microsoft.Xna.Framework.Vector3(1370f, 1589f, 0), //exits  house
                    EntityKey = "entity:human",
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "playerAllegiance",
                        ExpeditionKey = "Camp"
                    },
                   
                    Person = new Maps.MapEditor.Person()
                    {
                        FirstName = "John", //Keep this. He's mentioned in story
                        LastName = "Millet",
                        PersonalityType = "survivalTierPersonality",
                        Portrait = "human_w_m_adult_1",
                        SimulateJoinedExpeditionNow = true,
                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {
                        AgeInYears = new NormalDistribution() { Mean = 64f },
                        CasteKey = "male",
                        ModelTextureName = "ManCurryClothesRedHairTexture",
                        RaceKey = "whiteHumanDescendant",
                        //CultureTemplates = new StringChance[] { new StringChance() { String = "maleDescendantWhiteCulture2" } },
                        //RaceKey = "ManOchreClothesBlackHairTexture",
                        TraitTemplates = new StringChance[] { 
                                new StringChance() {  String = "electronicsSpecialist" }, //he sets up the radio
                                },

                    },
                    NeedLevels = startNeeds,
                    Properties = new SerializableDictionary<string, PropertyResult>() { { "origin", new PropertyResult() { StringResult = "playSite" } } }  // Lars demo condition - any other word than playSite can be used, it is just a tag                   
                }

            });
            #endregion
            #region spawnRains //mentioned in story
            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnRains",
                DelayInSeconds = delayForItemsAndAgents,

                EntityData = new Maps.MapEditor.EntityData()
                {
                    Location = new Microsoft.Xna.Framework.Vector3(1370f, 1589f, 0), //exits  house
                    EntityKey = "entity:human",
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "playerAllegiance",
                        ExpeditionKey = "Camp"
                    },
                   
                    Person = new Maps.MapEditor.Person()
                    {
                        FirstName = "Tereza", //Keep this. mentioned in story
                        LastName = "Rains",
                        PersonalityType = "survivalTierPersonality",
                        Portrait = "human_w_f_adult_1",
                        SimulateJoinedExpeditionNow = true,
                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {
                        AgeInYears = new NormalDistribution() { Mean = 44f },
                        CasteKey = "female",
                        ModelTextureName = "ManBlueBrownClothes1Texture",
                        RaceKey = "whiteHumanDescendant",
                        TraitTemplates = new StringChance[] { 
                                new StringChance() {  String = "farmingSpecialist" }, 
                                },

                    },
                    NeedLevels = startNeeds,
                    Properties = new SerializableDictionary<string, PropertyResult>() { { "origin", new PropertyResult() { StringResult = "playSite" } } } // Lars demo condition - any other word than playSite can be used, it is just a tag                                    
                }

            });
            #endregion
            #region spawnChemistTest //FOR TESTING! test. delete
            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnChemistTest",
                DelayInSeconds = delayForItemsAndAgents,

                EntityData = new Maps.MapEditor.EntityData()
                {
                    Location = new Microsoft.Xna.Framework.Vector3(1434f, 1596f, 0),
                    EntityKey = "entity:human",
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "playerAllegiance",
                        ExpeditionKey = "Camp"
                    },
                
                    Person = new Maps.MapEditor.Person()
                    {
                        FirstName = "Jane", //a family of chemists..
                        LastName = "Neson",
                        PersonalityType = "survivalTierPersonality", //
                        Portrait = "human_w_f_adult_1",
                        SimulateJoinedExpeditionNow = true,
                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {
                        AgeInYears = new NormalDistribution() { Mean = 40f },
                        CasteKey = "female",
                        ModelTextureName = "ManGreenSolid1Texture", //uniforms
                        RaceKey = "whiteHumanDescendant",
                        TraitTemplates = new StringChance[] { 
                                new StringChance() {  String = "chemistrySpecialist" }, 
                                },
                    },
                    NeedLevels = startNeeds,
                    Properties = new SerializableDictionary<string, PropertyResult>() { { "origin", new PropertyResult() { StringResult = "playSite" } } } // Lars demo condition - any other word than playSite can be used, it is just a tag                                    
                }

            });
            #endregion

            #region spawnBushcraftSpecialist1
            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnBushcraftSpecialist1",
                DelayInSeconds = delayForItemsAndAgents,

                DynamicLocation = new DynamicLocation()
                {
                    PropertyKey = "startingLocation"
                },
                EntityData = new Maps.MapEditor.EntityData()
                {
                    Location = new Microsoft.Xna.Framework.Vector3(-20f, 0f, 0),
                    EntityKey = "entity:human",
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "playerAllegiance",
                        ExpeditionKey = "Camp"
                    },
                   
                    Person = new Maps.MapEditor.Person()
                    {
                        PersonalityType = "survivalTierPersonality",
                        SimulateJoinedExpeditionNow = true,
                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {

                        TraitTemplates = new StringChance[] { 
                                new StringChance() {  String = "bushcraftSpecialist" },},
                        #region CultureTemplates. Future, 100% equality
                        CultureTemplates = new StringChance[] {
                                //equal distribution:
                                new StringChance() { Edge = 0.125f, String = "maleDescendantWhiteCulture2" },
                                new StringChance() { Edge = 0.25f, String = "maleDescendantAsianCulture2" }, 
                                new StringChance() { Edge = 0.375f, String = "maleDescendantHispanicCulture2" },
                                new StringChance() { Edge = 0.5f, String = "maleDescendantBlackCulture2" },  

                                new StringChance() { Edge = 0.75f, String = "femaleDescendantWhiteCulture2" },                                
                                new StringChance() { Edge = 1f, String = "femaleDescendantBlackCulture2" },                         
                            }
                        #endregion
                    },
                    NeedLevels = startNeeds,
                    Properties = new SerializableDictionary<string, PropertyResult>() { { "origin", new PropertyResult() { StringResult = "playSite" } } }  // Lars demo condition - any other word than playSite can be used, it is just a tag  
                }

            });

            #endregion
            #region spawnBushcraftSpecialist2
            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnBushcraftSpecialist2",
                DelayInSeconds = delayForItemsAndAgents,

                DynamicLocation = new DynamicLocation()
                {
                    PropertyKey = "startingLocation"
                },
                EntityData = new Maps.MapEditor.EntityData()
                {
                    Location = new Microsoft.Xna.Framework.Vector3(-0f, 0f, 0), //
                    EntityKey = "entity:human",
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "playerAllegiance",
                        ExpeditionKey = "Camp"
                    },
                   
                    Person = new Maps.MapEditor.Person()
                    {
                        PersonalityType = "survivalTierPersonality",
                        SimulateJoinedExpeditionNow = true,
                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {
                        TraitTemplates = new StringChance[] { 
                                new StringChance() {  String = "bushcraftSpecialist" },},
                        #region CultureTemplates. Future, 100% equality
                        CultureTemplates = new StringChance[] {
                                //equal distribution:
                                new StringChance() { Edge = 0.125f, String = "maleDescendantWhiteCulture2" },
                                new StringChance() { Edge = 0.25f, String = "maleDescendantAsianCulture2" }, 
                                new StringChance() { Edge = 0.375f, String = "maleDescendantHispanicCulture2" },
                                new StringChance() { Edge = 0.5f, String = "maleDescendantBlackCulture2" },  

                                new StringChance() { Edge = 0.75f, String = "femaleDescendantWhiteCulture2" },                                
                                new StringChance() { Edge = 1f, String = "femaleDescendantBlackCulture2" },                         
                            }

                        #endregion

                    },
                    NeedLevels = startNeeds,
                    Properties = new SerializableDictionary<string, PropertyResult>() { { "origin", new PropertyResult() { StringResult = "playSite" } } }  // Lars demo condition - any other word than playSite can be used, it is just a tag  
                }

            });
            #endregion
            #region spawnSecuritySpecialist1
            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnSecuritySpecialist1",
                DelayInSeconds = delayForItemsAndAgents,

                DynamicLocation = new DynamicLocation()
                {
                    PropertyKey = "startingLocation"
                },
                EntityData = new Maps.MapEditor.EntityData()
                {
                    Location = new Microsoft.Xna.Framework.Vector3(0f, 0f, 0),
                    EntityKey = "entity:human",
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "playerAllegiance",
                        ExpeditionKey = "Camp"
                    },
                   
                    Person = new Maps.MapEditor.Person()
                    {
                        PersonalityType = "survivalTierPersonality",
                        SimulateJoinedExpeditionNow = true,
                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {

                        TraitTemplates = new StringChance[] { 
                                new StringChance() {  String = "securitySpecialist" },},

                        #region CultureTemplates. Future, 100% equality
                        CultureTemplates = new StringChance[] {
                                //equal distribution:
                                new StringChance() { Edge = 0.125f, String = "maleDescendantWhiteCulture2" },
                                new StringChance() { Edge = 0.25f, String = "maleDescendantAsianCulture2" }, 
                                new StringChance() { Edge = 0.375f, String = "maleDescendantHispanicCulture2" },
                                new StringChance() { Edge = 0.5f, String = "maleDescendantBlackCulture2" },  

                                new StringChance() { Edge = 0.75f, String = "femaleDescendantWhiteCulture2" },                                
                                new StringChance() { Edge = 1f, String = "femaleDescendantBlackCulture2" },                         
                            }
                        #endregion
                    },
                    NeedLevels = startNeeds,
                    Properties = new SerializableDictionary<string, PropertyResult>() { { "origin", new PropertyResult() { StringResult = "playSite" } } }  // Lars demo condition - any other word than playSite can be used, it is just a tag  
                }

            });
            #endregion
            #region spawnCookingSpecialist1
            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnCookingSpecialist1",
                DelayInSeconds = delayForItemsAndAgents,

            /*    DynamicLocation = new DynamicLocation()
                {
                    PropertyKey = "startingLocation"
                },*/
                EntityData = new Maps.MapEditor.EntityData()
                {
                    Location = new Microsoft.Xna.Framework.Vector3(1442f, 1537f, 0), //a bit further away from cookhouse
                    EntityKey = "entity:human",
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "playerAllegiance",
                        ExpeditionKey = "Camp"
                    },
                   
                    Person = new Maps.MapEditor.Person()
                    {
                        PersonalityType = "survivalTierPersonality",
                        SimulateJoinedExpeditionNow = true,
                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {

                        TraitTemplates = new StringChance[] { 
                                new StringChance() {  String = "cookingSpecialist" },},
                        #region CultureTemplates. Future, 100% equality
                        CultureTemplates = new StringChance[] {
                                //equal distribution:
                                new StringChance() { Edge = 0.125f, String = "maleDescendantWhiteCulture2" },
                                new StringChance() { Edge = 0.25f, String = "maleDescendantAsianCulture2" }, 
                                new StringChance() { Edge = 0.375f, String = "maleDescendantHispanicCulture2" },
                                new StringChance() { Edge = 0.5f, String = "maleDescendantBlackCulture2" },  

                                new StringChance() { Edge = 0.75f, String = "femaleDescendantWhiteCulture2" },                                
                                new StringChance() { Edge = 1f, String = "femaleDescendantBlackCulture2" },                         
                            }
                        #endregion
                    },
                    NeedLevels = startNeeds,
                    Properties = new SerializableDictionary<string, PropertyResult>() { { "origin", new PropertyResult() { StringResult = "playSite" } } }  // Lars demo condition - any other word than playSite can be used, it is just a tag  
                }

            });
            #endregion
            #region spawnCookingSpecialist2
            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnCookingSpecialist2",
                DelayInSeconds = delayForItemsAndAgents,

                DynamicLocation = new DynamicLocation()
                {
                    PropertyKey = "startingLocation"
                },
                EntityData = new Maps.MapEditor.EntityData()
                {
                    Location = new Microsoft.Xna.Framework.Vector3(-10f, 0f, 0),
                    EntityKey = "entity:human",
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "playerAllegiance",
                        ExpeditionKey = "Camp"
                    },
                   
                    Person = new Maps.MapEditor.Person()
                    {

                        PersonalityType = "survivalTierPersonality",
                        SimulateJoinedExpeditionNow = true,
                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {

                        TraitTemplates = new StringChance[] { 
                                new StringChance() {  String = "cookingSpecialist" },},
                        #region CultureTemplates. Future, 100% equality
                        CultureTemplates = new StringChance[] {
                                //equal distribution:
                                new StringChance() { Edge = 0.125f, String = "maleDescendantWhiteCulture2" },
                                new StringChance() { Edge = 0.25f, String = "maleDescendantAsianCulture2" }, 
                                new StringChance() { Edge = 0.375f, String = "maleDescendantHispanicCulture2" },
                                new StringChance() { Edge = 0.5f, String = "maleDescendantBlackCulture2" },  

                                new StringChance() { Edge = 0.75f, String = "femaleDescendantWhiteCulture2" },                                
                                new StringChance() { Edge = 1f, String = "femaleDescendantBlackCulture2" },                         
                            }
                        #endregion
                    },
                    NeedLevels = startNeeds,
                    Properties = new SerializableDictionary<string, PropertyResult>() { { "origin", new PropertyResult() { StringResult = "playSite" } } }  // Lars demo condition - any other word than playSite can be used, it is just a tag  
                }

            });
            #endregion

            #region spawnHuntingSpecialist1
            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnHuntingSpecialist1",
                DelayInSeconds = delayForItemsAndAgents,

                DynamicLocation = new DynamicLocation()
                {
                    PropertyKey = "startingLocation"
                },
                EntityData = new Maps.MapEditor.EntityData()
                {
                    Location = new Microsoft.Xna.Framework.Vector3(-3f, -10f, 0),
                    EntityKey = "entity:human",
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "playerAllegiance",
                        ExpeditionKey = "Camp"
                    },
                  
                    Person = new Maps.MapEditor.Person()
                    {
                        PersonalityType = "survivalTierPersonality",
                        SimulateJoinedExpeditionNow = true,
                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {

                        TraitTemplates = new StringChance[] { 
                                new StringChance() {  String = "huntingSpecialist" },},
                        #region CultureTemplates. Future, 100% equality
                        CultureTemplates = new StringChance[] {
                                //equal distribution:
                                new StringChance() { Edge = 0.125f, String = "maleDescendantWhiteCulture2" },
                                new StringChance() { Edge = 0.25f, String = "maleDescendantAsianCulture2" }, 
                                new StringChance() { Edge = 0.375f, String = "maleDescendantHispanicCulture2" },
                                new StringChance() { Edge = 0.5f, String = "maleDescendantBlackCulture2" },  

                                new StringChance() { Edge = 0.75f, String = "femaleDescendantWhiteCulture2" },                                
                                new StringChance() { Edge = 1f, String = "femaleDescendantBlackCulture2" },                         
                            }
                        #endregion

                    },
                    NeedLevels = startNeeds,
                    Properties = new SerializableDictionary<string, PropertyResult>() { { "origin", new PropertyResult() { StringResult = "playSite" } } }  // Lars demo condition - any other word than playSite can be used, it is just a tag  
                }

            });
            #endregion
            #region spawnHuntingSpecialist2
            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnHuntingSpecialist2",
                DelayInSeconds = delayForItemsAndAgents,

                DynamicLocation = new DynamicLocation()
                {
                    PropertyKey = "startingLocation"
                },
                EntityData = new Maps.MapEditor.EntityData()
                {
                    Location = new Microsoft.Xna.Framework.Vector3(-1f, -6f, 0),
                    EntityKey = "entity:human",
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "playerAllegiance",
                        ExpeditionKey = "Camp"
                    },
                   
                    Person = new Maps.MapEditor.Person()
                    {
                        PersonalityType = "survivalTierPersonality",
                        SimulateJoinedExpeditionNow = true,
                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {
                        TraitTemplates = new StringChance[] { 
                                new StringChance() {  String = "huntingSpecialist" },},
                        #region CultureTemplates. Future, 100% equality
                        CultureTemplates = new StringChance[] {
                                //equal distribution:
                                new StringChance() { Edge = 0.125f, String = "maleDescendantWhiteCulture2" },
                                new StringChance() { Edge = 0.25f, String = "maleDescendantAsianCulture2" }, 
                                new StringChance() { Edge = 0.375f, String = "maleDescendantHispanicCulture2" },
                                new StringChance() { Edge = 0.5f, String = "maleDescendantBlackCulture2" },  

                                new StringChance() { Edge = 0.75f, String = "femaleDescendantWhiteCulture2" },                                
                                new StringChance() { Edge = 1f, String = "femaleDescendantBlackCulture2" },                         
                            }
                        #endregion
                    },
                    NeedLevels = startNeeds,
                    Properties = new SerializableDictionary<string, PropertyResult>() { { "origin", new PropertyResult() { StringResult = "playSite" } } }  // Lars demo condition - any other word than playSite can be used, it is just a tag  
                }

            });
            #endregion
            #region spawnMenialSpecialist1
            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnMenialSpecialist1",
                DelayInSeconds = delayForItemsAndAgents,

                DynamicLocation = new DynamicLocation()
                {
                    PropertyKey = "startingLocation"
                },
                EntityData = new Maps.MapEditor.EntityData()
                {
                    Location = new Microsoft.Xna.Framework.Vector3(-42f, 120f, 0),
                    EntityKey = "entity:human",
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "playerAllegiance",
                        ExpeditionKey = "Camp"
                    },
                   
                    Person = new Maps.MapEditor.Person()
                    {
                        PersonalityType = "survivalTierPersonality",
                        SimulateJoinedExpeditionNow = true,
                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {

                        TraitTemplates = new StringChance[] { 
                                new StringChance() {  String = "menialSpecialist" },},
                        #region CultureTemplates. Future, 100% equality
                        CultureTemplates = new StringChance[] {
                                //equal distribution:
                                new StringChance() { Edge = 0.125f, String = "maleDescendantWhiteCulture2" },
                                new StringChance() { Edge = 0.25f, String = "maleDescendantAsianCulture2" }, 
                                new StringChance() { Edge = 0.375f, String = "maleDescendantHispanicCulture2" },
                                new StringChance() { Edge = 0.5f, String = "maleDescendantBlackCulture2" },  

                                new StringChance() { Edge = 0.75f, String = "femaleDescendantWhiteCulture2" },                                
                                new StringChance() { Edge = 1f, String = "femaleDescendantBlackCulture2" },                         
                            }
                        #endregion
                    },
                    NeedLevels = startNeeds,
                    Properties = new SerializableDictionary<string, PropertyResult>() { { "origin", new PropertyResult() { StringResult = "playSite" } } }  // Lars demo condition - any other word than playSite can be used, it is just a tag  
                }

            });
            #endregion
            #region spawnMenialSpecialist2
            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnMenialSpecialist2",
                DelayInSeconds = delayForItemsAndAgents,

                DynamicLocation = new DynamicLocation()
                {
                    PropertyKey = "startingLocation"
                },
                EntityData = new Maps.MapEditor.EntityData()
                {
                    Location = new Microsoft.Xna.Framework.Vector3(-51f, 10f, 0),
                    EntityKey = "entity:human",
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "playerAllegiance",
                        ExpeditionKey = "Camp"
                    },
                   
                    Person = new Maps.MapEditor.Person()
                    {
                        PersonalityType = "survivalTierPersonality",
                        SimulateJoinedExpeditionNow = true,
                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {

                        TraitTemplates = new StringChance[] { 
                                new StringChance() {  String = "menialSpecialist" },},
                        #region CultureTemplates. Future, 100% equality
                        CultureTemplates = new StringChance[] {
                                //equal distribution:
                                new StringChance() { Edge = 0.125f, String = "maleDescendantWhiteCulture2" },
                                new StringChance() { Edge = 0.25f, String = "maleDescendantAsianCulture2" }, 
                                new StringChance() { Edge = 0.375f, String = "maleDescendantHispanicCulture2" },
                                new StringChance() { Edge = 0.5f, String = "maleDescendantBlackCulture2" },  

                                new StringChance() { Edge = 0.75f, String = "femaleDescendantWhiteCulture2" },                                
                                new StringChance() { Edge = 1f, String = "femaleDescendantBlackCulture2" },                         
                            }
                        #endregion
                    },
                    NeedLevels = startNeeds,
                    Properties = new SerializableDictionary<string, PropertyResult>() { { "origin", new PropertyResult() { StringResult = "playSite" } } }  // Lars demo condition - any other word than playSite can be used, it is just a tag  
                }

            });
            #endregion
            #region spawnConstructionSpecialist1
            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnConstructionSpecialist1",
                DelayInSeconds = delayForItemsAndAgents + 2f, // add 2 seconds so they dont all exit building at same time


                EntityData = new Maps.MapEditor.EntityData()
                {
                    Location = new Microsoft.Xna.Framework.Vector3(1370f, 1589f, 0), //exits  house
                    EntityKey = "entity:human",
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "playerAllegiance",
                        ExpeditionKey = "Camp"
                    },
                  
                    Person = new Maps.MapEditor.Person()
                    {
                        PersonalityType = "survivalTierPersonality",
                        SimulateJoinedExpeditionNow = true,
                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {
                        TraitTemplates = new StringChance[] { 
                                new StringChance() {  String = "constructionSpecialist" },},
                        #region CultureTemplates. Future, 100% equality
                        CultureTemplates = new StringChance[] {
                                //equal distribution:
                                new StringChance() { Edge = 0.125f, String = "maleDescendantWhiteCulture2" },
                                new StringChance() { Edge = 0.25f, String = "maleDescendantAsianCulture2" }, 
                                new StringChance() { Edge = 0.375f, String = "maleDescendantHispanicCulture2" },
                                new StringChance() { Edge = 0.5f, String = "maleDescendantBlackCulture2" },  

                                new StringChance() { Edge = 0.75f, String = "femaleDescendantWhiteCulture2" },                                
                                new StringChance() { Edge = 1f, String = "femaleDescendantBlackCulture2" },                         
                            }
                        #endregion
                    },
                    NeedLevels = startNeeds,
                    Properties = new SerializableDictionary<string, PropertyResult>() { { "origin", new PropertyResult() { StringResult = "playSite" } } }  // Lars demo condition - any other word than playSite can be used, it is just a tag  
                }

            });
            #endregion
            #region spawnConstructionSpecialist2
            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnConstructionSpecialist2",
                DelayInSeconds = delayForItemsAndAgents,

                DynamicLocation = new DynamicLocation()
                {
                    PropertyKey = "startingLocation"
                },
                EntityData = new Maps.MapEditor.EntityData()
                {
                    Location = new Microsoft.Xna.Framework.Vector3(-81f, 24f, 0),
                    EntityKey = "entity:human",
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "playerAllegiance",
                        ExpeditionKey = "Camp"
                    },
                  
                    Person = new Maps.MapEditor.Person()
                    {

                        PersonalityType = "survivalTierPersonality",
                        SimulateJoinedExpeditionNow = true,
                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {
                        TraitTemplates = new StringChance[] { 
                                new StringChance() {  String = "constructionSpecialist" },},
                        #region CultureTemplates. Future, 100% equality
                        CultureTemplates = new StringChance[] {
                                //equal distribution:
                                new StringChance() { Edge = 0.125f, String = "maleDescendantWhiteCulture2" },
                                new StringChance() { Edge = 0.25f, String = "maleDescendantAsianCulture2" }, 
                                new StringChance() { Edge = 0.375f, String = "maleDescendantHispanicCulture2" },
                                new StringChance() { Edge = 0.5f, String = "maleDescendantBlackCulture2" },  

                                new StringChance() { Edge = 0.75f, String = "femaleDescendantWhiteCulture2" },                                
                                new StringChance() { Edge = 1f, String = "femaleDescendantBlackCulture2" },                         
                            }
                        #endregion
                    },
                    NeedLevels = startNeeds,
                    Properties = new SerializableDictionary<string, PropertyResult>() { { "origin", new PropertyResult() { StringResult = "playSite" } } }  // Lars demo condition - any other word than playSite can be used, it is just a tag  
                }

            });
            #endregion
            #region spawnSmithingSpecialist1
            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnSmithingSpecialist1",
                DelayInSeconds = delayForItemsAndAgents,

                DynamicLocation = new DynamicLocation()
                {
                    PropertyKey = "startingLocation"
                },
                EntityData = new Maps.MapEditor.EntityData()
                {
                    Location = new Microsoft.Xna.Framework.Vector3(-96f, 23f, 0),
                    EntityKey = "entity:human",
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "playerAllegiance",
                        ExpeditionKey = "Camp"
                    },
                   
                    Person = new Maps.MapEditor.Person()
                    {
                        PersonalityType = "survivalTierPersonality",
                        SimulateJoinedExpeditionNow = true,
                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {

                        TraitTemplates = new StringChance[] { 
                                new StringChance() {  String = "smithingSpecialist" },},
                        #region CultureTemplates. Future, 100% equality
                        CultureTemplates = new StringChance[] {
                                //equal distribution:
                                new StringChance() { Edge = 0.125f, String = "maleDescendantWhiteCulture2" },
                                new StringChance() { Edge = 0.25f, String = "maleDescendantAsianCulture2" }, 
                                new StringChance() { Edge = 0.375f, String = "maleDescendantHispanicCulture2" },
                                new StringChance() { Edge = 0.5f, String = "maleDescendantBlackCulture2" },  

                                new StringChance() { Edge = 0.75f, String = "femaleDescendantWhiteCulture2" },                                
                                new StringChance() { Edge = 1f, String = "femaleDescendantBlackCulture2" },                         
                            }
                        #endregion
                    },
                    NeedLevels = startNeeds,
                    Properties = new SerializableDictionary<string, PropertyResult>() { { "origin", new PropertyResult() { StringResult = "playSite" } } }  // Lars demo condition - any other word than playSite can be used, it is just a tag  
                }

            });
            #endregion
            #region spawnSmithingSpecialist2
            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnSmithingSpecialist2",
                DelayInSeconds = delayForItemsAndAgents,

                DynamicLocation = new DynamicLocation()
                {
                    PropertyKey = "startingLocation"
                },
                EntityData = new Maps.MapEditor.EntityData()
                {
                    Location = new Microsoft.Xna.Framework.Vector3(-96f, 47f, 0),
                    EntityKey = "entity:human",
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "playerAllegiance",
                        ExpeditionKey = "Camp"
                    },
                   
                    Person = new Maps.MapEditor.Person()
                    {

                        PersonalityType = "survivalTierPersonality",
                        SimulateJoinedExpeditionNow = true,
                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {
                        TraitTemplates = new StringChance[] { 
                                new StringChance() {  String = "smithingSpecialist" },},
                        #region CultureTemplates. Future, 100% equality
                        CultureTemplates = new StringChance[] {
                                //equal distribution:
                                new StringChance() { Edge = 0.125f, String = "maleDescendantWhiteCulture2" },
                                new StringChance() { Edge = 0.25f, String = "maleDescendantAsianCulture2" }, 
                                new StringChance() { Edge = 0.375f, String = "maleDescendantHispanicCulture2" },
                                new StringChance() { Edge = 0.5f, String = "maleDescendantBlackCulture2" },  

                                new StringChance() { Edge = 0.75f, String = "femaleDescendantWhiteCulture2" },                                
                                new StringChance() { Edge = 1f, String = "femaleDescendantBlackCulture2" },                         
                            }
                        #endregion
                    },
                    NeedLevels = startNeeds,
                    Properties = new SerializableDictionary<string, PropertyResult>() { { "origin", new PropertyResult() { StringResult = "playSite" } } }  // Lars demo condition - any other word than playSite can be used, it is just a tag  
                }

            });
            #endregion
            #region spawnFarmingSpecialist1
            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnFarmingSpecialist1",
                DelayInSeconds = delayForItemsAndAgents,


                EntityData = new Maps.MapEditor.EntityData()
                {
                    Location = new Microsoft.Xna.Framework.Vector3(1370f, 1589f, 0), //exits  house
                    EntityKey = "entity:human",
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "playerAllegiance",
                        ExpeditionKey = "Camp"
                    },
                   
                    Person = new Maps.MapEditor.Person()
                    {
                        PersonalityType = "survivalTierPersonality",
                        SimulateJoinedExpeditionNow = true,
                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {
                        TraitTemplates = new StringChance[] { 
                                new StringChance() {  String = "farmingSpecialist" },},
                        #region CultureTemplates. Future, 100% equality
                        CultureTemplates = new StringChance[] {
                                //equal distribution:
                                new StringChance() { Edge = 0.125f, String = "maleDescendantWhiteCulture2" },
                                new StringChance() { Edge = 0.25f, String = "maleDescendantAsianCulture2" }, 
                                new StringChance() { Edge = 0.375f, String = "maleDescendantHispanicCulture2" },
                                new StringChance() { Edge = 0.5f, String = "maleDescendantBlackCulture2" },  

                                new StringChance() { Edge = 0.75f, String = "femaleDescendantWhiteCulture2" },                                
                                new StringChance() { Edge = 1f, String = "femaleDescendantBlackCulture2" },                         
                            }
                        #endregion
                    },
                    NeedLevels = startNeeds,
                    Properties = new SerializableDictionary<string, PropertyResult>() { { "origin", new PropertyResult() { StringResult = "playSite" } } }  // Lars demo condition - any other word than playSite can be used, it is just a tag  
                }

            });
            #endregion
            #region spawnFarmingSpecialist2
            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnFarmingSpecialist2",
                DelayInSeconds = delayForItemsAndAgents,


                EntityData = new Maps.MapEditor.EntityData()
                {
                    Location = new Microsoft.Xna.Framework.Vector3(1370f, 1589f, 0), //exits  house
                    EntityKey = "entity:human",
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "playerAllegiance",
                        ExpeditionKey = "Camp"
                    },
                   
                    Person = new Maps.MapEditor.Person()
                    {
                        PersonalityType = "survivalTierPersonality",
                        SimulateJoinedExpeditionNow = true,
                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {
                        TraitTemplates = new StringChance[] { 
                                new StringChance() {  String = "farmingSpecialist" },},
                        #region CultureTemplates. Future, 100% equality
                        CultureTemplates = new StringChance[] {
                                //equal distribution:
                                new StringChance() { Edge = 0.125f, String = "maleDescendantWhiteCulture2" },
                                new StringChance() { Edge = 0.25f, String = "maleDescendantAsianCulture2" }, 
                                new StringChance() { Edge = 0.375f, String = "maleDescendantHispanicCulture2" },
                                new StringChance() { Edge = 0.5f, String = "maleDescendantBlackCulture2" },  

                                new StringChance() { Edge = 0.75f, String = "femaleDescendantWhiteCulture2" },                                
                                new StringChance() { Edge = 1f, String = "femaleDescendantBlackCulture2" },                         
                            }
                        #endregion
                    },
                    NeedLevels = startNeeds,
                    Properties = new SerializableDictionary<string, PropertyResult>() { { "origin", new PropertyResult() { StringResult = "playSite" } } }  // Lars demo condition - any other word than playSite can be used, it is just a tag  
                }

            });
            #endregion

            #endregion

            #region spawnDog
            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnDog",
                DelayInSeconds = delayForItemsAndAgents,

                DynamicLocation = new DynamicLocation()
                {
                    PropertyKey = "startingLocation"
                },
                EntityData = new EntityData()
                {//  Name = "Rover", add name , it only shows in personnel list. remebmer to make 2 different on Hard. 
                    Location = new Microsoft.Xna.Framework.Vector3(10f, 28f, 0),
                    EntityKey = "entity:dog",
                    BioEntity = new Maps.MapEditor.BiologicalEntity() { AgeInYears = new NormalDistribution() { Mean = 4f } },
                    OwnedBy = new AllegianceAndExpedition() { AllegianceKey = "playerAllegiance", ExpeditionKey = "Camp" },
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "playerAllegiance",
                        ExpeditionKey = "Camp"
                    },

                },

            });
            #endregion

            #region spawnHaulRobot
            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnHaulRobot",
                DelayInSeconds = delayForItemsAndAgents,

                DynamicLocation = new DynamicLocation()
                {
                    PropertyKey = "startingLocation"
                },
                EntityData = new EntityData()
                {
                    Location = new Microsoft.Xna.Framework.Vector3(-13f, 10f, 0),
                    EntityKey = "entity:haulingRobot", //"entity:patrolRobot"
                    OwnedBy = new AllegianceAndExpedition() { AllegianceKey = "playerAllegiance", ExpeditionKey = "Camp" },
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "playerAllegiance",
                        ExpeditionKey = "Camp"
                    }
                }

            });
            #endregion

            #region Upgrade settings
            list.Add(new SpawnUpgradeAction()
            {
                KeyName = "startUpgradeSettingClayHut1Beds",
                DelayInSeconds = delayForUpgrades,

                UpgradeTargetEntityName = "Clay hut 1",

                EntityTypeKey = "item:wingweedMats4People",
                UpgradeCategoryKey = "bedsOrMats4People",

            });

            list.Add(new SpawnUpgradeAction()
            {
                KeyName = "startUpgradeSettingCookhouseStove",
                DelayInSeconds = delayForUpgrades,

                UpgradeTargetEntityName = "Cookhouse",

                EntityTypeKey = "item:simpleStoveUpgrade",
                UpgradeCategoryKey = "stove",

            });

            list.Add(new SpawnUpgradeAction()
            {
                KeyName = "startUpgradeSettingCookhouseCommunityHall",
                DelayInSeconds = delayForUpgrades,

                UpgradeTargetEntityName = "Cookhouse",

                EntityTypeKey = "item:communityHallUpgrade",
                UpgradeCategoryKey = "communityHall",

            });
/*
            list.Add(new SpawnUpgradeAction()
            {
                KeyName = "startUpgradeSettingCanopyBeds",
                DelayInSeconds = delayForUpgrades,

                UpgradeTargetEntityName = "Canopy house",

                EntityTypeKey = "item:wingweedMats4People",
                UpgradeCategoryKey = "bedsOrMats4People",

            });
*/

           /* list.Add(new SpawnUpgradeAction()
            {
                KeyName = "startUpgradeSettingClayHut1Stove",
                DelayInSeconds = delayForUpgrades,

                UpgradeTargetEntityName = "clayHut1",

                EntityTypeKey = "item:simpleStoveUpgrade",
                UpgradeCategoryKey = "stove",
                Value = true
            });*/

            #endregion

            #region spawn stockpiles
            list.Add(new SpawnStockpileAction()
          {
              KeyName = "startBricksStockpile",
              DelayInSeconds = delayForItemsAndAgents,

              CoveredArea = new Point[] { new Point(37, 31) },
              StartDragTilePosition = new Point(37, 31),

              // item overrides category. if neither item nor category has been set, then default is Allow.                  
              MayStockpileItem = new SerializableDictionary<string, int>()
                  {
                      {"item:solidMudBrick", -1 },
                      {"item:charcoal", -1 }
                  },
              MayStockpileCategory = new SerializableDictionary<string, bool>()
                  {                    
                      { "preparedFood", false }, 
                      { "ingredients", false },
                      { "waste", false }, 
                      { "bodies", false }, 
                      { "rawMaterials", false },  
                      { "tools", false }, 
                      { "weapons", false }, 
                      { "ammunition", false }, 
                      { "equipment", false } 
                  },
              OwnedBy = new AllegianceAndExpedition()
              {
                  AllegianceKey = "playerAllegiance",
                  ExpeditionKey = "Camp"
              }


          });

            list.Add(new SpawnStockpileAction()
            {
                KeyName = "startFirewoodStockpile",
                DelayInSeconds = delayForItemsAndAgents,

                CoveredArea = new Point[] { new Point(30, 30) },
                StartDragTilePosition = new Point(30, 30),

                // item overrides category. if neither item nor category has been set, then default is Allow.                  
                MayStockpileItem = new SerializableDictionary<string, int>()
                  {
                      {"item:firewood", -1 }
                  },
                MayStockpileCategory = new SerializableDictionary<string, bool>()
                  {                    
                      { "preparedFood", false }, 
                      { "ingredients", false },
                      { "waste", false }, 
                      { "bodies", false }, 
                      { "rawMaterials", false },  
                      { "tools", false }, 
                      { "weapons", false }, 
                      { "ammunition", false }, 
                      { "equipment", false } 
                  },
                OwnedBy = new AllegianceAndExpedition()
                {
                    AllegianceKey = "playerAllegiance",
                    ExpeditionKey = "Camp"
                }


            });

            list.Add(new SpawnStockpileAction()
            {
                KeyName = "startMaterialsStockpile",
                DelayInSeconds = delayForItemsAndAgents,

                CoveredArea = new Point[] { new Point(39, 34) },
                StartDragTilePosition = new Point(39, 34),

                // item overrides category. if neither item nor category has been set, then default is Allow.                  
                MayStockpileItem = new SerializableDictionary<string, int>()
                  {
                      {"item:sticks", -1 },
                      {"item:stones", -1 },
                      {"item:spoakShingles", -1 },
                      {"item:waterCaneStem", -1 }
                  },
                MayStockpileCategory = new SerializableDictionary<string, bool>()
                  {                    
                      { "preparedFood", false }, 
                      { "ingredients", false },
                      { "waste", false }, 
                      { "bodies", false }, 
                      { "rawMaterials", false },  
                      { "tools", false }, 
                      { "weapons", false }, 
                      { "ammunition", false }, 
                      { "equipment", false } 
                  },
                OwnedBy = new AllegianceAndExpedition()
                {
                    AllegianceKey = "playerAllegiance",
                    ExpeditionKey = "Camp"
                }


            });
            #endregion

            #region Starting equipment
            //
            // For testing
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startPanelScraps", new Vector2(-136f, 182f), "item:panelScraps", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSpikeTrap", new Vector2(-136f, 160f), "item:spikeTrap", "playerAllegiance", null, delayForItemsAndAgents)); //
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startVarmintBomb", new Vector2(-356f, -320f), "item:varmintBomb", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startExtrusionMachineComponents", new Vector2(0f, 0f), "item:extrusionMachineComponents", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startBeds", new Vector2(0f, 0f), "item:bedFrame", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startStonesTest", new Vector2(0f, 0f), "item:stones", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSpoakTest", new Vector2(0f, 0f), "item:spoakBranches", "playerAllegiance", null, delayForItemsAndAgents));
           
            ////////////////   
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startMudBricks", new Vector2(0f, 0f), "item:solidMudBrick", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startScrapMetal", new Vector2(0f, 0f), "item:scrapMetal", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startTextile", new Vector2(0f, 0f), "item:textile", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startPeat", new Vector2(0f, 0f), "item:dryPeat", "playerAllegiance", null, delayForItemsAndAgents));
            //   list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startWroughtIron", new Vector2(-136f, 182f), "item:wroughtIron", "playerAllegiance", null, delayForItemsAndAgents));
            //   list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startBlisterSteel", new Vector2(-136f, 182f), "item:blisterSteel", "playerAllegiance", null, delayForItemsAndAgents));
            #region food
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startJerky", new Vector2(0f, 0f), "item:driedBeef", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startBlackpulp", new Vector2(0f, 0f), "item:blackpulp", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSmokedCarbonTail", new Vector2(0f, 0f), "item:smokedCarbonTail", "playerAllegiance", null, delayForItemsAndAgents));
            #endregion
            #region seeds
            //    list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startGlassyCreeper", new Vector2(-136f, 182f), "item:glassyCreeperPods", "playerAllegiance", null, delayForItemsAndAgents));       
            #endregion
            #region weapons
            //    list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startGunpowderRifle", new Vector2(-146f, 176f), "item:gunpowderRifle", "playerAllegiance", null, delayForItemsAndAgents));
            //    list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startGunpowderAmmo", new Vector2(-146f, 176f), "item:blackPowderRifleAmmo", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startBlunderbuss", new Vector2(-146f, 176f), "item:musketoon", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startBlackPowderShotAmmo", new Vector2(-146f, 176f), "item:blackPowderShotAmmo", "playerAllegiance", null, delayForItemsAndAgents));

            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startBoltActionRifle", new Vector2(-146f, 176f), "item:boltActionRifle", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startBoltActionAmmo", new Vector2(-146f, 176f), "item:corditeAmmo", "playerAllegiance", null, delayForItemsAndAgents));

            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSentry", new Vector2(-146f, 176f), "item:sentry", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSentryAmmo", new Vector2(-146f, 176f), "item:sentryGunAmmo", "playerAllegiance", null, delayForItemsAndAgents));

            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startIronSpear", new Vector2(-146f, 176f), "item:ironSpear", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startCoilRifle", new Vector2(0f, 0f), "item:coilRifle", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startCoilRifleAmmo", new Vector2(0f, 0f), "item:coilRifleAmmo", "playerAllegiance", null, delayForItemsAndAgents));

            //   list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startImprovisedBow", new Vector2(-146f, 176f), "item:improvisedBow", "playerAllegiance", null, delayForItemsAndAgents));
            //   list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startIronArrow", new Vector2(-146f, 176f), "item:ironArrow", "playerAllegiance", null, delayForItemsAndAgents));
            #endregion
            #region Tools
            //     list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSteelMachete", new Vector2(-156f, 190f), "item:steelMachete", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSnips", new Vector2(-156f, 190f), "item:advancedSnips", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startString", new Vector2(-156f, 190f), "item:advancedString", "playerAllegiance", null, delayForItemsAndAgents));
            //   list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startMetalWire", new Vector2(-156f, 190f), "item:metalWire", "playerAllegiance", null, delayForItemsAndAgents));
            //   list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startKnife", new Vector2(-156f, 190f), "item:steelKnife", "playerAllegiance", null, delayForItemsAndAgents));

            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startCookingPot", new Vector2(-156f, 190f), "item:advancedCookingPot", "playerAllegiance", null, delayForItemsAndAgents));
            //    list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startGoldPot", new Vector2(-156f, 190f), "item:goldPot", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startImprovisedCookingPot", new Vector2(-156f, 190f), "item:improvisedCookingPot", "playerAllegiance", null, delayForItemsAndAgents));

            //  list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startHammer", new Vector2(-156f, 190f), "item:hammer", "playerAllegiance", null, delayForItemsAndAgents));
            //  list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startBellows", new Vector2(-156f, 190f), "item:bellows", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSulfurSmokeBomb", new Vector2(-156f, 190f), "item:bigBomb", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSensor", new Vector2(-156f, 190f), "item:sensor", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startTurnipCracker", new Vector2(-156f, 190f), "item:turnipCracker", "playerAllegiance", null, delayForItemsAndAgents));
            //   list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startBugNet", new Vector2(-156f, 190f), "item:strongBugNet", "playerAllegiance", null, delayForItemsAndAgents));
            //      list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startIronHooks", new Vector2(-156f, 190f), "item:ironHooks", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startNeonHornetsLive", new Vector2(-156f, 190f), "item:neonHornetsLive", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startFishTrapBasket", new Vector2(-156f, 190f), "item:fishTrapBasket", "playerAllegiance", null, delayForItemsAndAgents)); //carbon tail
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startFishTrapHoopNet", new Vector2(-156f, 190f), "item:fishTrapHoopNet", "playerAllegiance", null, delayForItemsAndAgents)); //streak fin
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startPigFliesLive", new Vector2(-156f, 190f), "item:pigFliesLive", "playerAllegiance", null, delayForItemsAndAgents));

            //stuff placed closer to their campsite stockpile so they dont need to haul so much in beginning.:
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startRadioAntenna", new Vector2(-100f, 100f), "item:radioAntenna", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startRadio", new Vector2(-100f, 100f), "item:radio", "playerAllegiance", null, delayForItemsAndAgents));
            //      list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startShadeleafResin", new Vector2(-100f, 100f), "item:shadeleafResin", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startRawhideString", new Vector2(-100f, 100f), "item:rawhideString", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startFlintKnife", new Vector2(-100f, 100f), "item:flintKnife", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startMetalworkersToolbox", new Vector2(-100f, 100f), "item:metalWorkersToolbox", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startAnvil", new Vector2(-100f, 100f), "item:anvil", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startBarClamps", new Vector2(-100f, 100f), "item:barClamps", "playerAllegiance", null, delayForItemsAndAgents));
            //   list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startClayJar", new Vector2(-100f, 100f), "item:clayJar", "playerAllegiance", null, delayForItemsAndAgents));
            //      list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startBrickMold", new Vector2(-100f, 100f), "item:brickMold", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startRefrigerator", new Vector2(-100f, 100f), "item:inactivatedFoodCoolerUnit", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startDomeTent", new Vector2(-100f, 100f), "item:domeTent", "playerAllegiance", null, delayForItemsAndAgents));

            //mostly for testing://
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startBlackpowder", new Vector2(1814f, 1516f), "item:blackPowder", "playerAllegiance", null, delayForItemsAndAgents));//
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startBlowpipe", new Vector2(1814f, 1516f), "item:blowpipe", "playerAllegiance", null, delayForItemsAndAgents));//
            //  list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startPickaxe", new Vector2(1814f, 1516f), "item:steelPickaxe", "playerAllegiance", null, delayForItemsAndAgents));//
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSpade", new Vector2(1814f, 1516f), "item:improvisedSpade", "playerAllegiance", null, delayForItemsAndAgents));//
            //    list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSteelSpade", new Vector2(1814f, 1516f), "item:steelSpade", "playerAllegiance", null, delayForItemsAndAgents));//
            //    list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startCharcoal", new Vector2(1814f, 1516f), "item:charcoal", "playerAllegiance", null, delayForItemsAndAgents));//
            // list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startWetFirewood", new Vector2(1814f, 1516f), "item:wetFirewood", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startGoldOre", new Vector2(1814f, 1516f), "item:goldOre", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startBogOre", new Vector2(1814f, 1516f), "item:bogOre", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startCleanTurnipGuts", new Vector2(1814f, 1516f), "item:cleanTurnipGuts", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startFingerFruit", new Vector2(1814f, 1516f), "item:fingerFruit", "playerAllegiance", null, delayForItemsAndAgents));
            //  list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startCrystalBerries", new Vector2(1814f, 1516f), "item:crystalBerries", "playerAllegiance", null, delayForItemsAndAgents));
            //   list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSalt", new Vector2(1814f, 1516f), "item:salt", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startClayPotUnglazed", new Vector2(1814f, 1516f), "item:clayPotUnglazed", "playerAllegiance", null, delayForItemsAndAgents));
            //  list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startTappingBucket", new Vector2(1814f, 1516f), "item:tappingBucket", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startClay", new Vector2(1814f, 1516f), "item:clay", "playerAllegiance", null, delayForItemsAndAgents));


            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSpoakLeaves", new Vector2(1814f, 1516f), "item:spoakLeaves", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSpoakBranchesTrimmed", new Vector2(1814f, 1516f), "item:spoakBranchesTrimmed", "playerAllegiance", null, delayForItemsAndAgents));
            //    list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startMarshcotSap", new Vector2(-156f, 190f), "item:marshcotSap", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startWingweedMats", new Vector2(1814f, 1516f), "item:wingweedMat", "playerAllegiance", null, delayForItemsAndAgents));

            //   list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startIronHandAxe", new Vector2(1814f, 1516f), "item:steelHandAxe", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startWaterCaneStem", new Vector2(1814f, 1516f), "item:waterCaneStem", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startShadeleafCanes", new Vector2(1814f, 1516f), "item:shadeleafCanes", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startVat", new Vector2(1814f, 1516f), "item:vat", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startImprovisedGreenHouseCover", new Vector2(1814f, 1516f), "item:improvisedGreenHouseCover", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startTurnipSalami", new Vector2(1814f, 1516f), "item:turnipSalami", "playerAllegiance", null, delayForItemsAndAgents));
            //        list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startDriedSaltedStreakFin", new Vector2(1814f, 1516f), "item:driedSaltedStreakFin", "playerAllegiance", null, delayForItemsAndAgents));
            //  list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startVinegar", new Vector2(1814f, 1516f), "item:vinegar", "playerAllegiance", null, delayForItemsAndAgents));



            //for testing:
            //   list.Add(Scenarios.ScenarioLoader.SpawnEntity("startMarshcotSap", new Vector2(1200f, 1634f), "item:marshcotSap", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnEntity("startSulfurPowder", new Vector2(1200f, 1634f), "item:sulfurPowder", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnEntity("startStreakFin", new Vector2(1814f, 1516f), "item:streakFin", "playerAllegiance", null, delayForItemsAndAgents));
            /////////////////////////////////

            //materials stockpiled in open:
            list.Add(Scenarios.ScenarioLoader.SpawnEntity("startSolidMudBrick", new Vector2(1814f, 1516f), "item:solidMudBrick", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnEntity("startCharcoal", new Vector2(1810f, 1512f), "item:charcoal", "playerAllegiance", null, delayForItemsAndAgents));

            list.Add(Scenarios.ScenarioLoader.SpawnEntity("startSpoakShingles", new Vector2(1890f, 1656f), "item:spoakShingles", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnEntity("startSticks", new Vector2(1896f, 1654f), "item:sticks", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnEntity("startStones", new Vector2(1886f, 1644f), "item:stones", "playerAllegiance", null, delayForItemsAndAgents));

            //dry firewood next to woodpile: Absolute coordinates!!!
            list.Add(Scenarios.ScenarioLoader.SpawnEntity("startFirewood", new Vector2(1459f, 1478f), "item:firewood", "playerAllegiance", null, delayForItemsAndAgents));


            #endregion

            //spawn inside containers:///////


            //woodpile:
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startFirewoodInWoodpile", "Woodpile", "item:firewood", "playerAllegiance", null, delayForItemsAndAgents, isProductionOutput: true));

            //compost pit holds different types of waste:
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startOrganicMatter", "Compost pit", "item:organicMatter", "playerAllegiance", null, delayForItemsAndAgents, storageCondition: "moist"));
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startRottenVegetables", "Compost pit", "item:rottenVegetables", "playerAllegiance", null, delayForItemsAndAgents, storageCondition: "moist"));

            //toolshed:
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startSteelMachete", "Tool shed", "item:steelMachete", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startHoe", "Tool shed", "item:farmingHoe", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startSteelSpade", "Tool shed", "item:steelSpade", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startPickaxe", "Tool shed", "item:steelPickaxe", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startIronHandAxe", "Tool shed", "item:steelHandAxe", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startTappingBucket", "Tool shed", "item:tappingBucket", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startBugNet", "Tool shed", "item:strongBugNet", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startIronHooks", "Tool shed", "item:ironHooks", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startBrickMold", "Tool shed", "item:brickMold", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startImprovisedTrowel", "Tool shed", "item:improvisedTrowel", "playerAllegiance", null, delayForItemsAndAgents));
            //granary:
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startGlassyCreeper", "Granary", "item:glassyCreeperPods", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startCrystalBerries", "Granary", "item:crystalBerries", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startDriedSaltedStreakFin", "Granary", "item:driedSaltedStreakFin", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startPickledCarbonTail", "Granary", "item:pickledCarbonTail", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startSmokedStreakFin", "Granary", "item:smokedStreakFin", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startHardtack", "Granary", "item:hardtack", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startSalt", "Granary", "item:salt", "playerAllegiance", null, delayForItemsAndAgents));

            //workbench:
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startRubber", "Workbench", "item:gaskets", "playerAllegiance", null, delayForItemsAndAgents));
            
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startKnife", "Workbench", "item:steelKnife", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startMetalWire", "Workbench", "item:metalWire", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startMarshcotSap", "Workbench", "item:marshcotSap", "playerAllegiance", null, delayForItemsAndAgents)); //spawned 1 so that tutorial is easier to write
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startShadeleafResin", "Workbench", "item:shadeleafResin", "playerAllegiance", null, delayForItemsAndAgents));
            //smithy: // 
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startBlacksmithsToolbox", "Smithy", "item:blacksmithsToolbox", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startMetalWorkersToolbox", "Smithy", "item:metalWorkersToolbox", "playerAllegiance", null, delayForItemsAndAgents));
         
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startBellows", "Smithy", "item:bellows", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startWroughtIron", "Smithy", "item:wroughtIron", "playerAllegiance", null, delayForItemsAndAgents)); //fails to spawn in smithy if not room enough
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startBlisterSteel", "Smithy", "item:blisterSteel", "playerAllegiance", null, delayForItemsAndAgents)); //fails to spawn in smithy if not room enough
        //    list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startCharcoal", "Smithy", "item:charcoal", "playerAllegiance", null, delayForItemsAndAgents)); //not room enough

            //in cookhouse:
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startGoldPot", "Cookhouse", "item:goldPot", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startKnifeInKitchen", "Cookhouse", "item:steelKnife", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startClayJar", "Cookhouse", "item:clayJar", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startVinegar", "Cookhouse", "item:vinegar", "playerAllegiance", null, delayForItemsAndAgents));
            // stuff that kinda makes sense they put in community hall: currently 'home storage' on cookhouse.
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startGunpowderRifle", "Cookhouse", "item:gunpowderRifle", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startGunpowderAmmo", "Cookhouse", "item:blackPowderRifleAmmo", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startImprovisedBow", "Cookhouse", "item:improvisedBow", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startIronArrow", "Cookhouse", "item:ironArrow", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(ScenarioLoader.SpawnItemInsideContainer("startCrystalWine", "Cookhouse", "item:crystalWine", "playerAllegiance", null, delayForItemsAndAgents));


             
            #endregion

            #region Upgrade items
            list.Add(ScenarioLoader.SpawnItemInsideContainer("startUpgradeCookhouseStove", "Cookhouse", "item:simpleStoveUpgrade", "playerAllegiance", null, delayForUpgrades, upgradeCategory: "stove"));
            list.Add(ScenarioLoader.SpawnItemInsideContainer("startUpgradeCookhouseCommunityHall", "Cookhouse", "item:communityHallUpgrade", "playerAllegiance", null, delayForUpgrades, upgradeCategory: "communityHall"));

            list.Add(ScenarioLoader.SpawnItemInsideContainer("startUpgradeClayHut1Mats", "Clay hut 1", "item:wingweedMats4People", "playerAllegiance", null, delayForUpgrades, upgradeCategory: "bedsOrMats4People"));
            list.Add(ScenarioLoader.SpawnItemInsideContainer("startUpgradeCanopyMats", "Canopy house", "item:wingweedMats4People", "playerAllegiance", null, delayForUpgrades, upgradeCategory: "bedsOrMats4People"));
           
            #endregion

            #region Natural terminals

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startNaturalTerminal",
                DelayInSeconds = 1,

                // DynamicLocation = new DynamicLocation()  {  PropertyKey = "startingLocation" },
                EntityData = new EntityData()
                {
                    EntityKey = "terrain:naturalLandTerminal",
                    Name = "To: Bird Hill",
                    Location = new Vector3(3024f, 1255f, 0f) //

                },

            });

            #endregion
            /*  #region detect passage from the start
          list.Add(new EventActionType()
          {
              Comments = "detect passage from the start",
              KeyName = "exploreShroudNaturalTerminal",
              DelayInSeconds = delayForItemsAndAgents + 1, // must execute last, after members have been added
              ExploreAction = new ExploreAction()
              {                   
                  OffsetLocationStart = new Vector2(3024f, 1255f), //new Vector2(1800f, 0f),
                  RadiusStart = 120f,
                  PerformDetection = true,

                  EntityToExploreWith = new InGameEvents.PropertyObjects.TargetObject()
                  {
                      GetList = new InGameEvents.PropertyObjects.GetList()
                      {
                          // get a random allegiance member
                          HasPropertiesListKey = "allegiances",
                          FilterCondition = new InGameEvents.Conditions.PropertyCondition() { PropertyKey = "keyName", ConstantStringEqual = "playerAllegiance" },
                          NextList = new InGameEvents.PropertyObjects.GetList()
                          {
                              HasPropertiesListKey = "members" // returns all members of player allegiance, then further up, the first item is picked implicitly
                          }
                      }
                  }
              }
          });
          #endregion */

            #region Starting Structures
            ///for testing: 

            list.Add(ScenarioLoader.SpawnEntity("startWorkshopBuilding", new Vector2(1130f, 1544f), "structure:workshopBuilding", "playerAllegiance", null, delayForStructures));
            //   list.Add(ScenarioLoader.SpawnEntity("startStill", new Vector2(1110f, 1544f), "structure:still", "playerAllegiance", null, delayForStructures));
            //


            ////////////////////////////

            ///town starting structures:
            list.Add(ScenarioLoader.SpawnEntity("startSmokeOven", new Vector2(1895f, 1608f), "structure:smokeOven", "playerAllegiance", null, delayForStructures));
            list.Add(ScenarioLoader.SpawnEntity("startStructureCompostPit", new Vector2(1252f, 1392f), "structure:compostPit", "playerAllegiance", null, delayForStructures, "Compost pit"));
          
            list.Add(ScenarioLoader.SpawnEntity("startStructureCookhouse", new Vector2(1396f, 1637f), "structure:cookhouse", "playerAllegiance", null, delayForStructures, "Cookhouse"));
            list.Add(ScenarioLoader.SpawnEntity("startStructureToolshed", new Vector2(1306f, 1478f), "structure:toolshed", "playerAllegiance", null, delayForStructures, "Tool shed"));
            list.Add(ScenarioLoader.SpawnEntity("startStructureFirewoodStack", new Vector2(1404f, 1478f), "structure:firewoodStack", "playerAllegiance", null, delayForStructures, "Woodpile"));

            list.Add(ScenarioLoader.SpawnEntity("startStructureClayGranary", new Vector2(1302f, 1595f), "structure:clayGranary", "playerAllegiance", null, delayForStructures, "Granary"));
            list.Add(ScenarioLoader.SpawnEntity("startStructureMeatDryingRack", new Vector2(1245f, 1544f), "structure:meatDryingRack", "playerAllegiance", null, delayForStructures));
       //     list.Add(ScenarioLoader.SpawnEntity("startStructureMudBrickKitchen", new Vector2(1530f, 1546f), "structure:mudBrickKitchen", "playerAllegiance", null, delayForStructures, "Kitchen"));
            list.Add(ScenarioLoader.SpawnEntity("startStructureKilnImprovisedSmall", new Vector2(1400f, 1556f), "structure:kilnImprovisedSmall", "playerAllegiance", null, delayForStructures)); //remove the small oven when we have communal kitchen

            list.Add(ScenarioLoader.SpawnEntity("startStructureImprovisedWorkbench", new Vector2(1584f, 1614f), "structure:improvisedWorkbench", "playerAllegiance", null, delayForStructures, "Workbench"));
            list.Add(ScenarioLoader.SpawnEntity("startStructureRadioHut", new Vector2(1633f, 1520f), "structure:radioHut", "playerAllegiance", null, delayForStructures));

            list.Add(ScenarioLoader.SpawnEntity("startStructureCaneHut", new Vector2(1530f, 1546f), "structure:caneHut", "playerAllegiance", null, delayForStructures, "Cane hut"));
            list.Add(ScenarioLoader.SpawnEntity("startStructureClayHut", new Vector2(1688f, 1566f), "structure:clayHut", "playerAllegiance", null, delayForStructures, "Clay hut 1"));



            list.Add(ScenarioLoader.SpawnEntity("startStructureSimpleSmithy", new Vector2(1778f, 1536f), "structure:simpleSmithy", "playerAllegiance", null, delayForStructures, "Smithy"));
            list.Add(ScenarioLoader.SpawnEntity("startStructureKiln", new Vector2(1850f, 1526f), "structure:kiln", "playerAllegiance", null, delayForStructures));

            // process actions must be done after people spawns:
            list.Add(ScenarioLoader.SpawnEntity("startStructureGreenhouse", new Vector2(1210f, 1744f), "structure:greenhouse", "playerAllegiance", null, delayForProcesses, processToUse: "constructGreenhouse")); // needs a process to init correctly
            list.Add(ScenarioLoader.SpawnEntity("startStructureSimplePort", new Vector2(1760f, 1652f), "structure:simplePort", "playerAllegiance", null, delayForProcesses, "Pier", processToUse: "buildSimplePort", actingOnEntity: "Pier spot"));
            
            //build fish trap on east side of town:
            list.Add(ScenarioLoader.SpawnEntity("startStructureFishTrapCoast2", new Vector2(1977f, 1814f), "structure:fishTrapCoast", "playerAllegiance", null, delayForProcesses, "Fish trap", 
               // processToUse: "placeFishTrapCoast", 
                actingOnEntity: "Fish trap spot Saltwater 2"));
           
           // list.Add(ScenarioLoader.RunProcess("startStructureFishTrapCoast2", delayForProcesses, "placeFishTrapCoast", "Fish trap spot Saltwater 2"));


            //tilled farmplots (must be called with Process to init correctly):
            list.Add(ScenarioLoader.RunProcess("startStructureSmallPlot1", delayForProcesses, "establishSmallPlot", "Small plot 1"));
            list.Add(ScenarioLoader.RunProcess("startStructureSmallPlot2", delayForProcesses, "establishSmallPlot", "Small plot 2"));
            list.Add(ScenarioLoader.RunProcess("startStructureSmallPlot3", delayForProcesses, "establishSmallPlot", "Small plot 3"));
            list.Add(ScenarioLoader.RunProcess("startStructureSmallPlot4", delayForProcesses, "establishSmallPlot", "Small plot 4"));
            list.Add(ScenarioLoader.RunProcess("startStructureLargePlot1", delayForProcesses, "establishLargePlot", "Large plot 1"));
            // greenhouse
            // list.Add(ScenarioLoader.SpawnEntity("startStructureGreenhouse", new Vector2(1210f, 1744f), "structure:greenhouse", "playerAllegiance", null, delayForStructures));

            #endregion

            #region placeExpedition
            list.Add(new CreateExpeditionAction()
              {
                  KeyName = "placeExpedition",
                  DelayInSeconds = 0.1,

                  ExpeditionData = new ExpeditionData()
                  {
                      KeyName = "Camp",
                      Name = "Town Centre",
                      AllegianceKey = "playerAllegiance",
                      PolicyData = new Policies.ExpeditionPolicyData()
                      {
                          FractionIndependentsAllowedToSleep = 0.7f, 
                          AllowAmmoUseAgainstVermin = new SerializableDictionary<string, bool>()
                          {
                                { "item:coilRifleAmmo", false }, // conserve ammo
                                { "item:sentryGunAmmo", false },
                                { "item:shotgunAmmo", false },
                                { "item:bushDragonCartridge", false }                            
                          }, 
                          CurrentTiers = new SerializableDictionary<RatingTypes, string>()
                            {
                                { RatingTypes.Comfort, "basic"  },
                                { RatingTypes.Food, "medium"  }, //mp raised from basic because fertilizer is now medium tier TODO: maybe increase some of their principles? or not.
                                { RatingTypes.Security, "basic"  }
                            }
                      },

                      Location = new ValueNode()
                      {
                          PropertyKey = "startingLocation"
                      }
                  }

              });

            #endregion

            #region setStartingLocation


            list.Add(new SetPropertyAction()
            {
                KeyName = "setStartingLocation",

                PropertyKey = "startingLocation",
                Value = new ValueNode()
                {
                    Location = new Microsoft.Xna.Framework.Vector2(1632f, 1652f) // 
                }

            });



            #endregion

            #region Start View and Fog of war

            list.Add(new SetViewAction()
            {
                KeyName = "setView",
                DelayInSeconds = delayForItemsAndAgents,

                CenterOnLocation = new Vector2(0f, 0f), //240f, 240f
                OffsetToLocation = new DynamicLocation()
                {
                    PropertyKey = "startingLocation"
                }

            });


            list.Add(new ExploreAction() //
            {
                KeyName = "exploreEntireMap",
                DelayInSeconds = delayForItemsAndAgents + 1,

                ExploreWholeMap = true,
                DetectMode = InGameEvents.Actions.DetectMode.RollToDetectHiddenEntities,
                EntityToExploreWith = new InGameEvents.PropertyObjects.TargetObject()
                {
                    GetList = new InGameEvents.PropertyObjects.GetList()
                    {
                        // get a random allegiance member
                        HasPropertiesListKey = "allegiances",
                        FilterCondition = new InGameEvents.Conditions.PropertyCondition() { PropertyKey = "keyName", ConstantStringEqual = "playerAllegiance" },
                        NextList = new InGameEvents.PropertyObjects.GetList()
                        {
                            HasPropertiesListKey = "persons" // returns all persons of player allegiance, then further up, the first item is picked implicitly
                        }
                    }
                }

            });

            #endregion

            #region FAUNA


            #region Fauna Expeditions and allegiance

            #region Bird
            list.Add(new SpawnAllegianceAction()
            {
                KeyName = "spawnBirdExpedition#1",

                Site = "playSite",
                ExpeditionData = new ExpeditionData()
                {
                    KeyName = "birdExpedition",
                    AllegianceKey = "birdAllegiance",
                    Location = new ValueNode()
                    {
                        Location = new Vector2(1680, 2000)
                    },
                    PopulationData = new PopulationData()
                    {                      
                        StartMembersList = new[] { "bird#1", "bird#2", "bird#3", "bird#4", "bird#5"},
                        StartMembers = 5,
                        MaxMembers = 5,
                        GrowthInMembersPerDay = 0.9f
                    }
                },
                AllegianceData = new AllegianceData()
                {                   
                    KeyName = "birdAllegiance",
                    EntityType = "entity:bird",
                    AllegianceType = Allegiances.AllegianceType.Other,
                    StatsData = new StatsData()
                    {
                        Security = 1f,
                        Comfort = 1f,
                        FoodSupply = 1f,
                    }
                }

            });
            #endregion

            #region Turnip north
            list.Add(new SpawnAllegianceAction()
            {
                KeyName = "spawnTurnipExpeditionNorth",

                Site = "playSite",
                ExpeditionData = new ExpeditionData()
                {
                    KeyName = "turnipAllegianceNorth",
                    Name = "Turnip Allegiance North",
                    AllegianceKey = "turnipAllegianceNorth",
                    Location = new ValueNode()
                    {
                        Location = new Vector2(816, 1104)
                    },
                    PopulationData = new PopulationData()
                    {
                         SpawnRadius = 100,
                         StartMembers = 1,
                         MaxMembers = 1,
                         GrowthInMembersPerDay = 0.2f //turnip should be a rare animal. once killed it should take a while before a new emerges

                    }
                },
                AllegianceData = new AllegianceData()
                {
                    ForageAndHuntingRadius = 300,
                    Name = "Turnip Allegiance North",
                    KeyName = "turnipAllegianceNorth",
                    EntityType = "entity:turnip",
                    AllegianceType = Allegiances.AllegianceType.Other,
                    StatsData = new StatsData()
                    {
                        Security = 1f,
                        Comfort = 1f,
                        FoodSupply = 1f,
                    }
                }

            });
            #endregion


            #region Binal rat #1
            list.Add(new SpawnAllegianceAction()
            {
                KeyName = "spawnBinalRatExpedition#1",

                Site = "playSite",
                ExpeditionData = new ExpeditionData()
                {
                    KeyName = "binalRatAllegiance#1",
                    Name = "Binal Rat Allegiance #1",
                    AllegianceKey = "binalRatAllegiance#1",
                    Location = new ValueNode()
                    {
                        Location = new Vector2(2304, 528)
                    },
                    PopulationData = new PopulationData()
                    {
                        SpawnRadius = 200,
                        StartMembers = 1,
                        MaxMembers = 3,
                        GrowthInMembersPerDay = 10f
                    }
                },
                AllegianceData = new AllegianceData()
                {
                    ForageAndHuntingRadius = 600,
                    Name = "Binal Rat Allegiance #1",
                    KeyName = "binalRatAllegiance#1",
                    EntityType = "entity:binalRat",
                    AllegianceType = Allegiances.AllegianceType.Other,
                    StatsData = new StatsData()
                    {
                        Security = 1f,
                        Comfort = 1f,
                        FoodSupply = 1f,
                    }
                }

            });
            #endregion

            #region Binal Rat #2
            list.Add(new SpawnAllegianceAction()
            {
                KeyName = "spawnBinalRatExpedition#2",

                Site = "playSite",
                ExpeditionData = new ExpeditionData()
                {
                    KeyName = "binalRatAllegiance#2",
                    Name = "Binal Rat Allegiance #2",
                    AllegianceKey = "binalRatAllegiance#2",
                    Location = new ValueNode()
                    {
                        Location = new Vector2(960, 2064)
                    },
                    PopulationData = new PopulationData()
                    {
                        SpawnRadius = 200,
                        StartMembers = 2,
                        MaxMembers = 3,
                        GrowthInMembersPerDay = 10f
                    }
                },
                AllegianceData = new AllegianceData()
                {
                    ForageAndHuntingRadius = 800,
                    Name = "Binal Rat Allegiance #2",
                    KeyName = "binalRatAllegiance#2",
                    EntityType = "entity:binalRat",
                    AllegianceType = Allegiances.AllegianceType.Other,
                    StatsData = new StatsData()
                    {
                        Security = 1f,
                        Comfort = 1f,
                        FoodSupply = 1f,
                    }
                }

            });
            #endregion

            #region Binal rat #3
            list.Add(new SpawnAllegianceAction()
            {
                KeyName = "spawnBinalRatExpedition#3",

                Site = "playSite",
                ExpeditionData = new ExpeditionData()
                {
                    KeyName = "binalRatAllegiance#3",
                    Name = "Binal Rat Allegiance #3",
                    AllegianceKey = "binalRatAllegiance#3",
                    Location = new ValueNode()
                    {
                        Location = new Vector2(768, 1200)
                    },
                    PopulationData = new PopulationData()
                    {
                        SpawnRadius = 200, 
                        StartMembers = 2,
                        MaxMembers = 3,
                        GrowthInMembersPerDay = 10f
                    }
                },
                AllegianceData = new AllegianceData()
                {
                    ForageAndHuntingRadius = 600,
                    Name = "Binal Rat Allegiance #3",
                    KeyName = "binalRatAllegiance#3",
                    EntityType = "entity:binalRat",
                    AllegianceType = Allegiances.AllegianceType.Other,
                    StatsData = new StatsData()
                    {
                        Security = 1f,
                        Comfort = 1f,
                        FoodSupply = 1f,
                    }
                }

            });
            #endregion

            #region swamp Demon Tree #2 north
            list.Add(new SpawnAllegianceAction()
            {
                KeyName = "spawnSwampDemonTreeExpedition#2",

                Site = "playSite",
                ExpeditionData = new ExpeditionData()
                {
                    KeyName = "swampDemonTreeExpedition#2",
                    Name = "Swamp Demon Tree Allegiance #2",
                    AllegianceKey = "swampDemonTreeAllegiance#2",
                    Location = new ValueNode()
                    {
                        Location = new Vector2(2600, 620)
                    },
                    PopulationData = new PopulationData()
                    {
                        SpawnRadius = 60, // also child, hmm
                        StartMembers = 1,
                        MaxMembers = 1,
                        GrowthInMembersPerDay = 0.44f
                    }
                },
                AllegianceData = new AllegianceData()
                {
                    ForageAndHuntingRadius = 250,
                    Name = "Swamp Demon Tree Allegiance #2",
                    KeyName = "swampDemonTreeAllegiance#2",
                    EntityType = "entity:swampDendront",
                    AllegianceType = Allegiances.AllegianceType.Other,
                    StatsData = new StatsData()
                    {
                        Security = 1f,
                        Comfort = 1f,
                        FoodSupply = 1f,
                    }
                }

            });
            #endregion

            #region Megapod
            list.Add(new SpawnAllegianceAction()
            {
                KeyName = "spawnSlugExpedition#1",

                Site = "playSite",
                ExpeditionData = new ExpeditionData()
                {
                    KeyName = "slugAllegiance#1",
                    Name = "Slug Allegiance #1",
                    AllegianceKey = "slugAllegiance#1",
                    Location = new ValueNode()
                    {
                        Location = new Vector2(2400, 480)
                    },
                    PopulationData = new PopulationData()
                    {
                         SpawnRadius = 150f, 
                         StartMembers = 3,
                         MaxMembers = 6,
                         GrowthInMembersPerDay = 0.9f   //megapods can be more common in swamp. player needs to protect his tapping buckets. Also, they get killed by the swamp dendront so need to replenish.                     
                    }
                },
                AllegianceData = new AllegianceData()
                {
                    ForageAndHuntingRadius = 400,
                    Name = "Slug Allegiance #1",
                    KeyName = "slugAllegiance#1",
                    EntityType = "entity:megapod",
                    AllegianceType = Allegiances.AllegianceType.Other,
                    StatsData = new StatsData()
                    {
                        Security = 1f,
                        Comfort = 1f,
                        FoodSupply = 1f,
                    }
                }

            });
            #endregion


            #region snatcher
            list.Add(new SpawnAllegianceAction()
            {
                KeyName = "spawnSnatcherExpedition#1",

                Site = "playSite",
                ExpeditionData = new ExpeditionData()
                {
                    KeyName = "snatcherExpedition#1",
                    Name = "Whipjaw Allegiance",
                    AllegianceKey = "snatcherAllegiance#1",
                    Location = new ValueNode()
                    {
                        Location = new Vector2(1104, 2400)
                    },
                    PopulationData = new PopulationData()
                    {
                        SpawnRadius = 60,
                        StartMembers = 1,
                        MaxMembers = 1,
                        GrowthInMembersPerDay = 0.38f //was 0.38f testing. the south west should be relatively safe for a few days after you kill the snatcher.
                    }
                },
                AllegianceData = new AllegianceData()
                {
                    ForageAndHuntingRadius = 300,
                    Name = "Whipjaw Allegiance",
                    KeyName = "snatcherAllegiance#1",
                    EntityType = "entity:whipjaw",
                    AllegianceType = Allegiances.AllegianceType.Other,
                    StatsData = new StatsData()
                    {
                        Security = 1f,
                        Comfort = 1f,
                        FoodSupply = 1f,
                    }
                }

            });
            #endregion




            #region Thunder Chicken #2
            list.Add(new SpawnAllegianceAction()
            {
                KeyName = "spawnThunderChickenExpedition#1",

                Site = "playSite",
                ExpeditionData = new ExpeditionData()
                {
                    KeyName = "thunderChickenExpedition#1",
                    Name = "Thunder Chicken Allegiance",
                    AllegianceKey = "thunderChickenAllegiance#1",
                    Location = new ValueNode()
                    {
                        Location = new Vector2(1488, 1008)
                    },
                    PopulationData = new PopulationData()
                    {
                       // SpawnRadius = 100,
                        RandomMembers = new StringChance[] { new StringChance() { Edge = 1f, String = "thunderChicken" }}, // spawns far away
                        StartMembers = 1,
                        MaxMembers = 2,
                        GrowthInMembersPerDay = 0.44f //chickens are rare here.
                    }
                },
                AllegianceData = new AllegianceData()
                {
                    ForageAndHuntingRadius = 400,
                    Name = "Thunder Chicken Allegiance",
                    KeyName = "thunderChickenAllegiance#1",
                    EntityType = "entity:studdedThunderChicken",
                    AllegianceType = Allegiances.AllegianceType.Other,
                    StatsData = new StatsData()
                    {
                        Security = 1f,
                        Comfort = 1f,
                        FoodSupply = 1f,
                    }
                }

            });
            #endregion


            #region leafcutter expedition #1

            list.Add(new SpawnAllegianceAction()
            {
                KeyName = "spawnLeafcutterExpedition#1",

                Site = "playSite",
                ExpeditionData = new ExpeditionData()
                {
                    KeyName = "leafcutterExpedition#1",
                    Name = "Leafcutter Expedition",
                    AllegianceKey = "leafcutterAllegiance#1",
                    Location = new ValueNode()
                    {
                        Location = new Vector2(1296, 1056)//
                    },
                    PopulationData = new PopulationData()
                    {
                        SpawnSources = new string[] { "Field quadite nest 1" },
                        StartSpawnSources = new string[] { "fieldQuaditeNest1" }, // needed to allow a starting pop
                        StartMembers = 1,
                        MaxMembers = 5,
                        GrowthInMembersPerDay = 9.2f
                    }
                },
                AllegianceData = new AllegianceData()
                {
                    ForageAndHuntingRadius = 500, //
                    Name = "Leafcutter allegiance",
                    KeyName = "leafcutterAllegiance#1",
                    EntityType = "entity:fieldQuadite",
                    AllegianceType = Allegiances.AllegianceType.Other,
                    StatsData = new StatsData()
                    {
                        Security = 1f,
                        Comfort = 1f,
                        FoodSupply = 1f,
                    }
                }

            });

            #endregion


            #endregion

            #region Spawn intervals and pop caps

            #region spawn intervals



            //  there's 1600 sec / day
            // scenario starts at TimeOfDay = 0.50   
            //RelativeNoOfDays = 0.5 //getting dark
            //RelativeNoOfDays = 0.795  // morning
            //RelativeNoOfDays = 1.123  //afternoon              
            //RelativeNoOfDays = 1.695  //night 
            /*
            list.Add(new SetPropertyAction()
            {
                KeyName = "setSpawnIntervalBinalRatsNormal",

                PropertyKey = "binalRatSpawnInterval",
                Value = new ValueNode() { Int = 160 },

            });
            list.Add(new SetPropertyAction()
            {
                KeyName = "setSpawnIntervalLeafcutterNormal",

                PropertyKey = "leafcutterSpawnInterval",
                Value = new ValueNode() { Int = 173 },

            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "setSpawnIntervalTurnipsNormal",

                PropertyKey = "turnipSpawnInterval",
                Value = new ValueNode() { Int = 8600 }, //turnip should be a rare animal. once killed it should take a while before a new emerges

            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "setSpawnIntervalThunderChickensNormal",

                PropertyKey = "thunderChickenSpawnInterval",
                Value = new ValueNode() { Int = 3600 }, //chickens are rare here.

            });
            list.Add(new SetPropertyAction()
            {
                KeyName = "setSpawnIntervalSnatchersNormal",

                PropertyKey = "snatcherSpawnInterval",
                Value = new ValueNode() { Int = 4200 },// the south west should be relatively safe for a few days after you kill the snatcher.

            });
            list.Add(new SetPropertyAction()
            {
                KeyName = "setSpawnIntervalDemonTreeNormal",

                PropertyKey = "demonTreeSpawnInterval",
                Value = new ValueNode() { Int = 3600 },// takes a few days to reappear

            });*/
          /*  list.Add(new SetPropertyAction()
            {
                KeyName = "setSpawnIntervalSlugsNormal",

                PropertyKey = "slugSpawnInterval",
                Value = new ValueNode() { Int = 1800 }, //(there's 1600 sec / day) megapods can be more common in swamp. player needs to protect his tapping buckets. Also, they get killed by the swamp dendront so need to replenish.

            });*/

            #endregion
/*
            #region max pop

            // Remember that the actual spawned amount is the Value +1  ........MP what does that mean???



            list.Add(new SetPropertyAction()
            {
                KeyName = "setMaxDemonTreesNormal",

                PropertyKey = "maxDemonTree",
                Value = new ValueNode() { Int = 1 },

            });
            list.Add(new SetPropertyAction()
            {
                KeyName = "setMaxTurnipsNormal",

                PropertyKey = "maxTurnips",
                Value = new ValueNode() { Int = 2 },

            });


            list.Add(new SetPropertyAction()
            {
                KeyName = "setMaxThunderChickensNormal",

                PropertyKey = "maxThunderChickens",
                Value = new ValueNode() { Int = 2 },

            });


            list.Add(new SetPropertyAction()
            {
                KeyName = "setMaxSnatcherNormal",

                PropertyKey = "maxSnatchers",
                Value = new ValueNode() { Int = 1 },

            });
            list.Add(new SetPropertyAction()
            {
                KeyName = "setMaxBinalRatsNormal",

                PropertyKey = "maxBinalRats",
                Value = new ValueNode() { Int = 3 },

            });
            list.Add(new SetPropertyAction()
            {
                KeyName = "setMaxLeafcuttersNormal",

                PropertyKey = "maxLeafcutters",
                Value = new ValueNode() { Int = 5 },

            });
         
            #endregion
            */

            #endregion
            #endregion

            #region Resources


            list.Add(new ChangeResourcesAction()
            {
                KeyName = "setPlentyResources",


                AllResources = true,
                ExcludeResourceTypes = new[] { "stones", "clay", "firegrassSod","vine", //because they should follow the terrain graphics closely
                                                "crop:sticks",                      //because they are too important to risk big variation
                                                "sulfurDeposit", "guanoDeposit", //because they are too localized to risk big variation
                                                "streakFin", "carbonTail", "alabasterRay", "daggermouth", "clamwich", "torux", "minnowsLive", "phantomWeaver", "ursinix", "webWing", "crestedFoiler", "goldenCenobite", "muckGrinder" },//exclude those that are included in fish school.
                OperationToUse = ChangeResourcesAction.Operation.Multiply,
                NoiseParameters = new NoiseParams()
                {
                    NoiseAddend = 0.175f,
                    NoiseAmplitude = 0.75f,
                    NoiseFrequency = 0.1f
                }

            });

            list.Add(new ChangeResourcesAction()
            {
                KeyName = "setNormalResources",


                AllResources = true,
                ExcludeResourceTypes = new[] { "stones", "clay", "firegrassSod", "vine", //because they should follow the terrain graphics closely
                                                "crop:sticks",                      //because they are too important to risk big variation
                                                "sulfurDeposit", "guanoDeposit", //because they are too localized to risk big variation
                                                "streakFin", "carbonTail", "alabasterRay", "daggermouth", "clamwich", "torux", "minnowsLive", "phantomWeaver", "ursinix", "webWing", "crestedFoiler", "goldenCenobite", "muckGrinder" },//exclude those that are included in fish school.
                OperationToUse = ChangeResourcesAction.Operation.Multiply,

                NoiseParameters = new NoiseParams()
                {
                    NoiseAddend = 0.15f,
                    NoiseAmplitude = 0.75f,
                    NoiseFrequency = 0.1f
                }

            });



            list.Add(new ChangeResourcesAction()
            {
                KeyName = "setFishSchoolMedium", //this is for the resources where we want a big difference from game to game in where they are found. (meaning they have many possible habitats on the map). some game3s, they will be in one corner of the screen, other games, in the other corner. therefore place clusters  about 2- 3 places on each screen, well apart. not sure if the base number should be 1-1 or 0-4 for example.


                ResourceType = new[] { "streakFin", "carbonTail", "alabasterRay", "daggermouth", "clamwich", "torux", "minnowsLive", "phantomWeaver", "ursinix", "webWing", "crestedFoiler", "goldenCenobite", "muckGrinder" },
                OperationToUse = ChangeResourcesAction.Operation.Multiply,
                NoiseParameters = new NoiseParams()
                {
                    NoiseAmplitude = 1.5f,
                    NoiseAddend = -1f,
                    NoiseFrequency = 0.035f // high means "white noise". 0.01f not useful, just a big gradient......0.05f kinda bigger
                }

            });
            #endregion

            #region Pier Spots


            list.Add(new SpawnEntityAction() //
            {
                KeyName = "startPierSpot1",
                DelayInSeconds = delayForSpots,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:pierSpot",
                    Name = "Pier spot",
                    Location = new Vector3(1760f, 1652f, 0) //was 1536f, 1536f, 0
                },

            });


            #endregion

            #region Farmplots Small

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFarmSpotSmall1",
                DelayInSeconds = delayForSpots,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:smallPlotSpot",
                    Name = "Small plot 1",
                    Location = new Vector3(1104f, 1344f, 0)
                },

            });

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFarmSpotSmall2",
                DelayInSeconds = delayForSpots,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:smallPlotSpot",
                    Name = "Small plot 2",
                    Location = new Vector3(1015f, 1480f, 0)
                },

            }); //next to this one:

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFarmSpotSmall3",
                DelayInSeconds = delayForSpots,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:smallPlotSpot",
                    Name = "Small plot 3",
                    Location = new Vector3(1192f, 1480f, 0)
                },

            });


            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFarmSpotSmall4",
                DelayInSeconds = delayForSpots,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:smallPlotSpot",
                    Name = "Small plot 4",
                    Location = new Vector3(1566f, 1296f, 0)
                },

            });

            //up north west, not developed:
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFarmSpotSmall5",
                DelayInSeconds = delayForSpots,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:smallPlotSpot",
                    Name = "Small plot 5",
                    Location = new Vector3(1039f, 1193f, 0)
                },

            });
            //down south, not developed:
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFarmSpotSmall6",
                DelayInSeconds = delayForSpots,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:smallPlotSpot",
                    Name = "Small plot 6",
                    Location = new Vector3(1366f, 1996f, 0)
                },

            });

            #endregion

            #region Farmplots Large
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFarmSpotLarge1",
                DelayInSeconds = delayForSpots,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:largePlotSpot",
                    Name = "Large plot 1",
                    Location = new Vector3(1340f, 1268f, 0)
                },

            });


            #endregion

            #region Fish Trap Saltwater Spots (Coast)

            //northwest
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFishTrapCoast1",
                DelayInSeconds = delayForSpots,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:fishTrapSpotCoast",
                    Name = "Fish trap spot Saltwater 1",
                    Location = new Vector3(100f, 100f, 0)
                },

            });

            //cove east
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFishTrapCoast2",
                DelayInSeconds = delayForSpots,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:fishTrapSpotCoast",
                    Name = "Fish trap spot Saltwater 2",
                    Location = new Vector3(1977f, 1814f, 0)
                },

            });
            //cove west
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFishTrapCoast3",
                DelayInSeconds = delayForSpots,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:fishTrapSpotCoast",
                    Name = "Fish trap spot Saltwater 3",
                    Location = new Vector3(1625f, 2050f, 0)
                },

            });


            #endregion

            #region Fish Trap Freshwater Spots (Shore)

            //swamp
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFishTrapShore1",
                DelayInSeconds = delayForSpots,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:fishTrapSpotShore",
                    Name = "Fish trap spot Freshwater 1",
                    Location = new Vector3(2160f, 816f, 0)
                },

            });
            //swamp
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFishTrapShore2",
                DelayInSeconds = delayForSpots,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:fishTrapSpotShore",
                    Name = "Fish trap spot Freshwater 2",
                    Location = new Vector3(2016f, 560f, 0)
                },

            });

            //west
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFishTrapShore3",
                DelayInSeconds = delayForSpots,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:fishTrapSpotShore",
                    Name = "Fish trap spot Freshwater 3",
                    Location = new Vector3(675f, 1393f, 0)
                },

            });


            #endregion

            #region clay deposit
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startClayDeposit1",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:clayDeposit",
                    Name = "Clay deposit",
                    Location = new Vector3(2648f, 1584f, 0) //
                },

            });
            #endregion

            #region bog ore deposit //
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startBogOreDeposit1",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:bogOreDeposit",
                    Name = "Bog ore deposit",
                    Location = new Vector3(432f, 624f, 0) //
                },

            });
            #endregion

            #region peat deposit //
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startPeatDeposit1", // North west, not far from town
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:peatDeposit",
                    Name = "Peat deposit",
                    Location = new Vector3(960f, 1056f, 0) //
                },

            });
            #endregion

            #region salt deposit //
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startSaltDeposit1",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:saltDeposit",
                    Name = "Salt deposit",
                    Location = new Vector3(1728f, 2496f, 0) //
                },

            });
            #endregion

            #region Particles

            #region small fog
            list.Add(
              new ParticleEffectAction()
              {
                  KeyName = "smallFog1",     //Little ""steam" rising from pond      

                  Location = new ValueNode()
                  {
                      Location = new Microsoft.Xna.Framework.Vector2(2060f, 1816f)
                      //     Location = Maps.MapManager.TileToWorldPosVector2(new Point(20, 49))
                  },
                  ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "smallFog" } }

              });

            list.Add(
              new ParticleEffectAction()
              {
                  KeyName = "smallFog2",     //Little ""steam" rising from pond      

                  Location = new ValueNode()
                  {
                      Location = new Microsoft.Xna.Framework.Vector2(2400f, 1758f)
                      //    Location = Maps.MapManager.TileToWorldPosVector2(new Point(18, 43))
                  },
                  ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "smallFog" } }

              });


            #endregion
            #region swamp fog


            list.Add(
              new ParticleEffectAction()
              {
                  KeyName = "fog1",     //smaller fog on swamp top right     

                  Scale = 5f,
                  Location = new ValueNode()
                  {
                      Location = Maps.MapManager.TileToWorldPosVector2(new Point(42, 10))

                  },
                  ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "fog" } }

              });

            list.Add(
              new ParticleEffectAction()
              {
                  KeyName = "fog2",     //smaller fog on swamp    

                  Scale = 5f,
                  Location = new ValueNode()
                  {
                      Location = Maps.MapManager.TileToWorldPosVector2(new Point(39, 9))

                  },
                  ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "fog" } }

              });

            #region
            list.Add(
              new ParticleEffectAction()
              {
                  KeyName = "fog3",     //Large fog on swamp    

                  Scale = 7f,
                  Location = new ValueNode()
                  {
                      Location = Maps.MapManager.TileToWorldPosVector2(new Point(45, 16))

                  },
                  ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "fog" } }

              });
            #endregion
            #region
            list.Add(
              new ParticleEffectAction()
              {
                  KeyName = "fog4",     //Large fog on swamp    

                  Scale = 7f,
                  Location = new ValueNode()
                  {
                      Location = Maps.MapManager.TileToWorldPosVector2(new Point(46, 7))

                  },
                  ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "fog" } }

              });
            #endregion
            #region
            list.Add(
              new ParticleEffectAction()
              {
                  KeyName = "fog5",     //Large fog on swamp    

                  Scale = 7f,
                  Location = new ValueNode()
                  {
                      Location = Maps.MapManager.TileToWorldPosVector2(new Point(48, 10))

                  },
                  ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "fog" } }

              });
            #endregion

            #region
            list.Add(
              new ParticleEffectAction()
              {
                  KeyName = "fog6",     //Large fog on swamp    

                  Scale = 9f,
                  Location = new ValueNode()
                  {
                      Location = Maps.MapManager.TileToWorldPosVector2(new Point(48, 14))

                  },
                  ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "fog" } }

              });
            #endregion

            #region
            list.Add(
              new ParticleEffectAction()
              {
                  KeyName = "fog7",     //Large fog on swamp    

                  Scale = 9f,
                  Location = new ValueNode()
                  {
                      Location = Maps.MapManager.TileToWorldPosVector2(new Point(52, 17))

                  },
                  ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "fog" } }

              });
            #endregion

            #region
            list.Add(
              new ParticleEffectAction()
              {
                  KeyName = "fog8",     //Large fog on swamp    

                  Scale = 9f,
                  Location = new ValueNode()
                  {
                      Location = Maps.MapManager.TileToWorldPosVector2(new Point(57, 14))

                  },
                  ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "fog" } }

              });
            #endregion

            #region
            list.Add(
              new ParticleEffectAction()
              {
                  KeyName = "fog9",     //Large fog on swamp    

                  Scale = 6f,
                  Location = new ValueNode()
                  {
                      Location = Maps.MapManager.TileToWorldPosVector2(new Point(54, 7))

                  },
                  ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "fog" } }

              });
            #endregion
            #region
            list.Add(
              new ParticleEffectAction()
              {
                  KeyName = "fog10",     //Large fog on swamp    

                  Scale = 6f,
                  Location = new ValueNode()
                  {
                      Location = Maps.MapManager.TileToWorldPosVector2(new Point(58, 4))

                  },
                  ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "fog" } }

              });
            #endregion

            #region
            list.Add(
              new ParticleEffectAction()
              {
                  KeyName = "fog11",     //Large fog on swamp    

                  Scale = 8f,
                  Location = new ValueNode()
                  {
                      Location = Maps.MapManager.TileToWorldPosVector2(new Point(53, 3))

                  },
                  ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "fog" } }

              });
            #endregion

            #region
            list.Add(
              new ParticleEffectAction()
              {
                  KeyName = "fog12",     //Large fog on swamp    

                  Scale = 10f,
                  Location = new ValueNode()
                  {
                      Location = Maps.MapManager.TileToWorldPosVector2(new Point(55, 10))

                  },
                  ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "fog" } }

              });
            #endregion

            #endregion






            #endregion






            return list;
        }
    }
}
