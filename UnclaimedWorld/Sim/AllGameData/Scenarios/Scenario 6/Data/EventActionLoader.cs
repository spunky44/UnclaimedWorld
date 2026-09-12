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

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_6.Data
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
                KeyName = "initBurialText2", //
                PropertyKey = "burialTextMultipleDeathsMultipleSurvivors",
                Value = new ValueNode() { String = "The crew in the clay pit were all too familiar with hardship and tragedies. Else they wouldn't have signed up for this work. When they lost #NAMEOFDECEASED#CAUSEOFDEATH,  #EUOLOGYGIVER made it clear that everyone was here voluntarily and could leave at any time..." }
            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "initBurialText3", //mp same text as above.
                PropertyKey = "burialTextSingleDeathMultipleSurvivors",
                Value = new ValueNode() { String = "The crew in the clay pit were all too familiar with hardship and tragedies. Else they wouldn't have signed up for this work. When they lost #NAMEOFDECEASED#CAUSEOFDEATH,  #EUOLOGYGIVER made it clear that everyone was here voluntarily and could leave at any time..." }
            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "initBurialText4",
                PropertyKey = "burialTextSingleDeathSingleSurvivor",
                Value = new ValueNode() { String = "Losing #NAMEOFDECEASED came as a natural continuation of past tragedies more than a sudden shock. #EUOLOGYGIVER carried out the burial with as much dignity as possible and without reflecting on the past nor speculating on the future..." }
            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "initBurialText5",
                PropertyKey = "burialTextMultipleDeathsSingleSurvivor",
                Value = new ValueNode() { String = "Losing #NAMEOFDECEASED came as a natural continuation of past tragedies more than a sudden shock. #EUOLOGYGIVER carried out the burial with as much dignity as possible and without reflecting on the past nor speculating on the future..." }
            });
            #endregion

            list.Add(new SetPropertyAction()
            {
                Comments = "used for determining when the player has continued game after story part is over",
                KeyName = "initStoryPartOver",
                PropertyKey = "storyPartOver",
                Value = new ValueNode() { Bool = false }
            });

            list.Add(new SetPropertyAction()
            {
                Comments = "Used to check on when the player have achived the 40 mudbricks objective",
                KeyName = "initStores40Mudbricks",
                PropertyKey = "Stores40Mudbricks",
                Value = new ValueNode() { Bool = false }
            });

            list.Add(new SetPropertyAction()
            {
                Comments = "play with tut",
                KeyName = "setTutorialOn",
                PropertyKey = "tutorialOn",
                Value = new ValueNode() { Bool = true }
            });
            list.Add(new SetPropertyAction()
            {
                Comments = "play without tut",
                KeyName = "setTutorialOff",
                PropertyKey = "tutorialOn",
                Value = new ValueNode() { Bool = false }
            });
            #region Group meeting dialog texts

            list.Add(new SetPropertyAction()
            {
                KeyName = "initTimeBeforeGroupMeeting",
                Comments = "200 seconds after contract ends",
                //  PropertyKey = "timeInGameSecondsBeforeGroupMeeting", Value = new ValueNode() { Decimal = 3400 } }  

                PropertyKey = "timeInGameSecondsBeforeGroupMeeting",
                Value = new FunctionNode()
                {
                    Left = new UnaryFunctionNode() { Operator = UnaryExpressionOperator.DateToRelativeSeconds, Operand = new ValueNode() { PropertyKey = "endDate" } },
                    Right = new ValueNode() { Decimal = 200f },
                    Operator = ExpressionOperator.Plus
                }

            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "meetingEmigrateThreat",
                PropertyKey = "meetingEmigrateThreat",
                Value = new ValueNode() { String = "If not, then I'm gonna go back to #EMIGRATETO." }
            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "meetingEmigrateThreatAllUnhappy",
                PropertyKey = "meetingEmigrateThreatAllUnhappy",
                Value = new ValueNode() { String = "I'm thinking, maybe we should all just give up and go back to #EMIGRATETO." }
            });

            #region 3 people -meetings
            list.Add(new SetPropertyAction()
            {
                KeyName = "meeting3Security",

                PropertyKey = "meeting3Security",
                Value = new ValueNode()
                {
                    String = "#UNHAPPY: Guys, thanks for hearing me out...it's about the security here... \n \n" //
                                + "#CONTENT: What is it? \n \n"
                                + "#UNHAPPY: I think we need to be much more careful here. And we need more weapons. \n \n"
                                + "#CONTENT: It's up to all of us to keep an eye out. \n \n"
                                + "#UNHAPPY: It's not enough - if we don't get better weapons soon, something bad will happen. So let's do something about it! #EMIGRATETHREAT \n \n"
                                + "#CONTENT: Well, you're free to leave if you're afraid. Anything else?"


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
                    String = "#UNHAPPY: I want to talk about the food here. Or the lack of it. \n \n"
                                + "#CONTENT: We don't want to waste money on expensive provisions. \n \n"
                                + "#UNHAPPY: No, but if people are hungry, we can't work hard. So we need to get more food soon. #EMIGRATETHREAT \n \n"
                                + "#CONTENT: Who else has something to complain about?"

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
                    String = "#UNHAPPY: I don't normally complain, but the living conditions here are testing my limits. \n \n"
                                + "#CONTENT: You're not comfortable here? \n \n"
                                + "#UNHAPPY: Look. I don't ask for much. Let's set aside some time for making good shelters. #EMIGRATETHREAT  \n \n"
                                + "#CONTENT: It would be great if we could have a comfortable time here. Not sure if that's possible though. Anything else?"

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
                                + "#UNHAPPY: Look, the conditions here are horrible, even for two people. Please, let's work on the living conditions. #EMIGRATETHREAT \n \n"
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
                    String = "#UNHAPPY:  Look, we all want the security situation to improve! So let's get our act together and start working as a team! What are you waiting for? #EMIGRATETHREAT"
                }

            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "meetingFoodAllUnhappy",

                PropertyKey = "meetingFoodAllUnhappy",
                Value = new ValueNode()
                {
                    String = "#UNHAPPY:  Look, we all want the food situation to improve! So let's get our act together and start working as a team! What are you waiting for? #EMIGRATETHREAT"
                }

            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "meetingComfortAllUnhappy",

                PropertyKey = "meetingComfortAllUnhappy",
                Value = new ValueNode()
                {
                    String = "#UNHAPPY:  Look, we all want the comfort conditions to improve! So let's get our act together and start working as a team! What are you waiting for? #EMIGRATETHREAT"
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
                Value = new ValueNode() { String = "AUDIO LOG, #JOURNALDATE \n \n#NAME1: I'll keep it short - I'm quitting because of the bad security here. \n \n#NAME2: Yeah well, this is no place for sissies. If you quit, you're not getting paid. That was the deal. \n \n#NAME1: Whatever. Good luck fighting the patricians. I hope to see you back in #EMIGRATIONTARGET some day." } //followed up with some banter.
            });

            // 
            list.Add(new SetPropertyAction()
            {
                KeyName = "initEmigrateSecurityNoConversationDialogText",
                PropertyKey = "securityEmigrateEventNoConversationDialogText",
                Value = new ValueNode() { String = "TEXT LOG, #JOURNALDATE \n \n#NAME1: When you read this, I'll be leaving. We're in danger from wild animals here, but I seem to be the only one who takes this threat seriously. I'm going back to #EMIGRATIONTARGET. Keep my share of the money." }
            });


            list.Add(new SetPropertyAction()
            {
                KeyName = "initEmigrateComfortDialogText",
                PropertyKey = "comfortEmigrateEventDialogText",
                Value = new ValueNode() { String = "AUDIO LOG, #JOURNALDATE \n \n#NAME1: I've had enough with the bad shelters, the cold and the filth. \n \n#NAME2: If you don't like it, you're free to leave before time. But then you're not getting paid. That was the deal. \n \n#NAME1: Yeah, yeah. I'm going back to #EMIGRATIONTARGET. See you there." } //followed up with some banter.
            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "initEmigrateComfortNoConversationDialogText",
                PropertyKey = "comfortEmigrateEventNoConversationDialogText",
                Value = new ValueNode() { String = "TEXT LOG, #JOURNALDATE \n \n#NAME1: This is a note to let you know that I'm quitting. I'm fed up with this squalor and these bad shelters. I'm going back to #EMIGRATIONTARGET. You can keep my share of the money." }
            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "initEmigrateFoodDialogText",
                PropertyKey = "foodEmigrateEventDialogText",
                Value = new ValueNode() { String = "AUDIO LOG, #JOURNALDATE \n \n#NAME1: I'm quitting because of the lack of food. I need to eat, you know. \n \n#NAME2: You can go back to #EMIGRATIONTARGET. That's one mouth less to feed. But you're not getting your share of the money. That was the deal.  \n#NAME1: Yeah I know. Enjoy your stay." }//followed up with some banter.
            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "initEmigrateFoodNoConversationDialogText",
                PropertyKey = "foodEmigrateEventNoConversationDialogText",
                Value = new ValueNode() { String = "TEXT LOG, #JOURNALDATE \n \n#NAME1: This is a note to let you know I'm quitting because of the lack of food here. I'm going back to #EMIGRATIONTARGET. Keep my share of the money." }
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

                        ViewLongitudeStart = 17.6f, // w 
                        ViewLongitudeEnd = 17.9f,
                        ViewLatitudeStart = 70.6f, // h
                        ViewLatitudeEnd = 70.8f
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
                        Name = "Clay Pit", //
                        KeyName = "playSite",
                        Description = "Our camp: Next to a muddy creek, large deposits of clay and not much else. To the north is a dangerous marsh with some iron.",
                        Coords = new Overland.Locations.GeodeticCoordinate(17.775d, 70.75d), //dmake sure they can travel by boat in straight line to and from. //Latitude = , Longitude =  },
                        IsPlaySite = true,
                        ShowLabel = true,
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

                        Name = "Clay Pit", //"Player Allegiance",
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
                        Decimal = 10 //
                    }

                });
            #endregion

            //
            #region spawnOtherSite1
            list.Add(
                new SpawnSiteAction()
                {
                    KeyName = "spawnOtherSite1",

                    SiteData = new SiteData()
                    {
                        Name = "Tellus", //
                        KeyName = "otherSite1",
                        Description = "The small town where we come from. A recent flooding caused a demand for mudbricks which we try to fulfil.",
                        Coords = new Overland.Locations.GeodeticCoordinate(17.825d, 70.68d),    //  was 17.729d, 70.66d                    
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
                        Name = "Tellus",
                        KeyName = "otherSite1Allegiance1",
                        EntityType = "entity:human",
                        AllegianceType = Allegiances.AllegianceType.Other,
                        PermitsImmigration = true,
                        StatsData = new StatsData()
                        {
                            Security = .32f,
                            Comfort = .41f,
                            FoodSupply = .29f,
                        }
                    }

                });
            #endregion
            #region spawnOtherSite1Expedition1 
            list.Add(new CreateExpeditionAction()
            {
                KeyName = "spawnOtherSite1Expedition1",// not player's site.
                DelayInSeconds = 0.1, // wait for starting location property to have been set!

                ExpeditionData = new ExpeditionData()
                {
                    KeyName = "otherSite1Expedition1",//not player's site.
                    Name = "The Wharf", // 
                    AllegianceKey = "otherSite1Allegiance1",
                    TradeProfile = "farmingTradeProfile", // use the generic profile for longplay. override important starting goods below, for example to set a great demand at startup.
                    AvailableForTrade = new SerializableDictionary<string, Trade.TradeAmountType>() //override, part of tut/scenario:
                            { 
                        //Sells:
                        { "item:commonOilTubers", new TradeAmountType() { StartAmount = new NormalDistribution() { Mean = 55 },   MaxAmountForSale = 55 } }, //this makes it so that exactly 55 items are for sale at beginning
                        { "item:hardtack", new TradeAmountType() { StartAmount = new NormalDistribution() { Mean = 30 }, MaxAmountForSale = 30 } },
                        { "item:driedThunderChicken", new TradeAmountType() { StartAmount = new NormalDistribution() { Mean = 18 }, MaxAmountForSale = 18 } }, 

                        { "item:anvil", new TradeAmountType() { StartAmount = new NormalDistribution() { Mean = 1 }, MaxAmountForSale = 1 } }, 
                        { "item:hammer", new TradeAmountType() { StartAmount = new NormalDistribution() { Mean = 2 }, MaxAmountForSale = 2 } }, 

                        { "item:musket", new TradeAmountType() { StartAmount = new NormalDistribution() { Mean = 1 },  LinearIncreasePerDay = 0.7f,  MaxAmountForSale = 5,} }, //to raise security rating.
                        { "item:blackPowderShotAmmo", new TradeAmountType() { StartAmount = new NormalDistribution() { Mean = 2 }, LinearIncreasePerDay = 3f,  MaxAmountForSale = 8,  } }, 

                        //Buys:
                        { "item:solidMudBrick", new TradeAmountType() { StartAmount = new NormalDistribution() { Mean = 0 }, AmountToBuy = 100, MaxAmountToBuy = 100, LinearConsumptionPerDay = 10, MaxAmountForSale = 0 } },
                     
                            },

                    StructuresProfile = "largePierProfile",
                    PricesProfile = "descentEraPrices", 
                      
                    #region Vehicles for hire /////////////                         
                    VehiclesForHire = new SerializableDictionary<string, VehiclesForHireType>()
                        {
                            {
                                "entity:smallBarge", new VehiclesForHireType() //can only carry 40 mudbricks.
                                {
                                        SpecificPrice = new NormalDistribution() { Mean = 20, StandardDeviation = 0d }, 
                                        StartAmount = 1
                                }
                            }
                        }
                    #endregion
                   
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
                        Name = "Tellus River",
                        FromSite = "playSite",
                        ToSite = "otherSite1",
                        Length = 12,
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






            ////////////////////////////////////////////




            //Wilderness
            #region spawnWildernessSite1
            /*
            list.Add(
               new EventActionType()
               {
                   KeyName = "spawnWildernessSite1",
                   SpawnSite = new SpawnSiteAction()
                   {
                       //   DistanceFromPlaySite = 9f, // 9 km away MP uncomment this to test distance. ('yelling' distance is below 10 km.) (comment out the coords below when testing)
                       //    BearingFromPlaySite = 2f, // MP: DOES NOT WORK // radians

                       SiteData = new SiteData()
                       {
                           Name = "Bird Hill",
                           Key = "wildernessSite1",
                           Coords = new Overland.Locations.GeodeticCoordinate(17.77d, 70.68d),//
                           IsPlaySite = false,
                           ShowLabel = false,
                           ShowTallPin = true, //...........this site always sorts on top of the other one, apparently.
                       }
                   }

               });

            list.Add(
               new EventActionType()
               {
                   KeyName = "spawnWildernessSite1Allegiance1",
                   SpawnAllegiance = new SpawnAllegianceAction()
                   {
                       Site = "wildernessSite1",
                       AllegianceData = new AllegianceData()
                       {
                           Name = "Bird Hill",
                           Key = "wildernessSite1Allegiance1",
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
                   }
               });

            list.Add(new EventActionType()
            {
                KeyName = "spawnWildernessSite1Expedition1",
                DelayInSeconds = 0.1, // wait for starting location property to have been set!
                CreateExpedition = new CreateExpeditionAction()
                {
                    ExpeditionData = new ExpeditionData()
                    {
                        Key = "wildernessSite1Expedition1",
                        Name = "Camp", // 
                        AllegianceKey = "wildernessSite1Allegiance1",
                    }
                }
            });*/
            #endregion
            #region Route to Tellus site
            list.Add(
                new SpawnRouteAction()
                {
                    KeyName = "spawnPlaySiteSite1LandRoute",
                    DelayInSeconds = 1,

                    RouteData = new RouteData()
                    {
                        Name = "Road to Tellus", //
                        FromSite = "playSite",
                        ToSite = "otherSite1",
                        Length = 10,
                        RouteType = RouteType.Land
                    }

                });
            #endregion

            #endregion
            //GAME AREA:
            #region Natural terminals

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startNaturalTerminal",
                DelayInSeconds = 1,

                // DynamicLocation = new DynamicLocation()  {  PropertyKey = "startingLocation" },
                EntityData = new EntityData()
                {
                    EntityKey = "terrain:naturalLandTerminal",
                    Name = "To: Tellus",
                    Location = new Vector3(1210f, 1902f, 0f) //

                },

            });

            #endregion
            #region detect passage from the start //do not use
            /*   list.Add(new EventActionType()
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
          });*/
            #endregion

            #region Characters
            //menial 1-5 with survivaltier personaltiy
            #region spawnMenialSpecialist1
            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnMenialSpecialist1",
                DelayInSeconds = delayForItemsAndAgents,


                EntityData = new Maps.MapEditor.EntityData()
                {
                    Location = new Microsoft.Xna.Framework.Vector3(1560f, 1584f, 0), //top left
                    EntityKey = "entity:human",
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "playerAllegiance",
                        ExpeditionKey = "Camp"
                    },
                   
                    Person = new Maps.MapEditor.Person()
                    {
                        PersonalityType = "survivalTierPersonality", //Clay pit worker
                        SimulateJoinedExpeditionNow = true,
                    },
                    EffectProfiles = new[] { "contract" },
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


                EntityData = new Maps.MapEditor.EntityData()
                {
                    Location = new Microsoft.Xna.Framework.Vector3(1546f, 1600f, 0), //top left
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
                    EffectProfiles = new[] { "contract" },
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
            #region spawnMenialSpecialist3
            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnMenialSpecialist3",
                DelayInSeconds = delayForItemsAndAgents,


                EntityData = new Maps.MapEditor.EntityData()
                {
                    Location = new Microsoft.Xna.Framework.Vector3(1534f, 1718f, 0), //down left
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
                    EffectProfiles = new[] { "contract" },
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
            #region spawnMenialSpecialist4
            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnMenialSpecialist4",
                DelayInSeconds = delayForItemsAndAgents,


                EntityData = new Maps.MapEditor.EntityData()
                {
                    Location = new Microsoft.Xna.Framework.Vector3(1488f, 1766f, 0), //down left
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
                    EffectProfiles = new[] { "contract" },
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
            #region spawnMenialSpecialist5
            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnMenialSpecialist5",
                DelayInSeconds = delayForItemsAndAgents,


                EntityData = new Maps.MapEditor.EntityData()
                {
                    Location = new Microsoft.Xna.Framework.Vector3(1524f, 1756f, 0), //down left
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
                    EffectProfiles = new[] { "contract" },
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
            //
            #region spawnMenialSpecialist6
            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnMenialSpecialist6",
                DelayInSeconds = delayForItemsAndAgents,


                EntityData = new Maps.MapEditor.EntityData()
                {
                    Location = new Microsoft.Xna.Framework.Vector3(1564f, 1762f, 0), //down left
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
                    EffectProfiles = new[] { "contract" },
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
            #region spawnMenialSpecialist7
            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnMenialSpecialist7",
                DelayInSeconds = delayForItemsAndAgents,


                EntityData = new Maps.MapEditor.EntityData()
                {
                    Location = new Microsoft.Xna.Framework.Vector3(1695f, 1594f, 0), //top right
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
                    EffectProfiles = new[] { "contract" },
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
            #region spawnMenialSpecialist8
            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnMenialSpecialist8",
                DelayInSeconds = delayForItemsAndAgents,


                EntityData = new Maps.MapEditor.EntityData()
                {
                    Location = new Microsoft.Xna.Framework.Vector3(1758f, 1594f, 0), //top right
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
                    EffectProfiles = new[] { "contract" },
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
            #region spawnMenialSpecialist9
            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnMenialSpecialist9",
                DelayInSeconds = delayForItemsAndAgents,


                EntityData = new Maps.MapEditor.EntityData()
                {
                    Location = new Microsoft.Xna.Framework.Vector3(1728f, 1612f, 0), //top right
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
                    EffectProfiles = new[] { "contract" },
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
            #region spawnMenialSpecialist10
            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnMenialSpecialist10",
                DelayInSeconds = delayForItemsAndAgents,


                EntityData = new Maps.MapEditor.EntityData()
                {
                    Location = new Microsoft.Xna.Framework.Vector3(1779f, 1591f, 0), //top right
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
                    EffectProfiles = new[] { "contract" },
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
            //
            //not used for now:
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
                    EffectProfiles = new[] { "contract" },
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
                    EffectProfiles = new[] { "contract" },
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
                    EffectProfiles = new[] { "contract" },
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
                    EffectProfiles = new[] { "contract" },
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
                    EffectProfiles = new[] { "contract" },
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
                    EffectProfiles = new[] { "contract" },
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
                    EffectProfiles = new[] { "contract" },
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


            #region spawnConstructionSpecialist1
            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnConstructionSpecialist1",
                DelayInSeconds = delayForItemsAndAgents,


                EntityData = new Maps.MapEditor.EntityData()
                {
                    Location = new Microsoft.Xna.Framework.Vector3(1334f, 1696f, 0),
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
                        SimulateJoinedExpeditionNow = true,
                        PersonalityType = "survivalTierPersonality",

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
                    Location = new Microsoft.Xna.Framework.Vector3(1334f, 1696f, 0),
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
                    Location = new Microsoft.Xna.Framework.Vector3(1334f, 1696f, 0),
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


            #region spawn stockpiles
            list.Add(new SpawnStockpileAction()
          {
              KeyName = "startBricksStockpile",
              DelayInSeconds = delayForItemsAndAgents,

              CoveredArea = new Point[] { new Point(31, 38) }, //put it close to the port so they don't need to haul that far.
              StartDragTilePosition = new Point(31, 38),

              // item overrides category. if neither item nor category has been set, then default is Allow.                  
              MayStockpileItem = new SerializableDictionary<string, int>()
                  {
                      {"item:solidMudBrick", -1 },
                      
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

            // list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startmudbrick", new Vector2(-136f, 182f), "item:solidMudBrick", "playerAllegiance", null, delayForItemsAndAgents));
            //   list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startBlisterSteel", new Vector2(-136f, 182f), "item:blisterSteel", "playerAllegiance", null, delayForItemsAndAgents));


            #region weapons for test
            /*     //    list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startGunpowderRifle", new Vector2(-146f, 176f), "item:gunpowderRifle", "playerAllegiance", null, delayForItemsAndAgents));
      //    list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startGunpowderAmmo", new Vector2(-146f, 176f), "item:blackPowderRifleAmmo", "playerAllegiance", null, delayForItemsAndAgents));
          list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startBlunderbuss", new Vector2(-146f, 176f), "item:musketoon", "playerAllegiance", null, delayForItemsAndAgents));
          list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startBlackPowderShotAmmo", new Vector2(-146f, 176f), "item:blackPowderShotAmmo", "playerAllegiance", null, delayForItemsAndAgents));

          list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startBoltActionRifle", new Vector2(-146f, 176f), "item:boltActionRifle", "playerAllegiance", null, delayForItemsAndAgents));
          list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startBoltActionAmmo", new Vector2(-146f, 176f), "item:corditeAmmo", "playerAllegiance", null, delayForItemsAndAgents));

          list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSentry", new Vector2(-146f, 176f), "item:sentry", "playerAllegiance", null, delayForItemsAndAgents));
          list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSentryAmmo", new Vector2(-146f, 176f), "item:sentryGunAmmo", "playerAllegiance", null, delayForItemsAndAgents));

          list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startIronSpear", new Vector2(-146f, 176f), "item:ironSpear", "playerAllegiance", null, delayForItemsAndAgents));

       //   list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startImprovisedBow", new Vector2(-146f, 176f), "item:improvisedBow", "playerAllegiance", null, delayForItemsAndAgents));
       //   list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startIronArrow", new Vector2(-146f, 176f), "item:ironArrow", "playerAllegiance", null, delayForItemsAndAgents));*/
            #endregion
            #region Tools for test
            /*    //     list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSteelMachete", new Vector2(-156f, 190f), "item:steelMachete", "playerAllegiance", null, delayForItemsAndAgents));
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
          list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startBlackpowder", new Vector2(-156f, 190f), "item:blackPowder", "playerAllegiance", null, delayForItemsAndAgents));//
          list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startBlowpipe", new Vector2(-156f, 190f), "item:blowpipe", "playerAllegiance", null, delayForItemsAndAgents));//
        //  list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startPickaxe", new Vector2(-156f, 190f), "item:steelPickaxe", "playerAllegiance", null, delayForItemsAndAgents));//
          list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSpade", new Vector2(-156f, 190f), "item:improvisedSpade", "playerAllegiance", null, delayForItemsAndAgents));//
      //    list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSteelSpade", new Vector2(-156f, 190f), "item:steelSpade", "playerAllegiance", null, delayForItemsAndAgents));//
      //    list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startCharcoal", new Vector2(-156f, 190f), "item:charcoal", "playerAllegiance", null, delayForItemsAndAgents));//
         // list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startWetFirewood", new Vector2(-156f, 190f), "item:wetFirewood", "playerAllegiance", null, delayForItemsAndAgents));
          list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startGoldOre", new Vector2(-156f, 190f), "item:goldOre", "playerAllegiance", null, delayForItemsAndAgents));
          list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startBogOre", new Vector2(-156f, 190f), "item:bogOre", "playerAllegiance", null, delayForItemsAndAgents));
          list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startCleanTurnipGuts", new Vector2(-156f, 190f), "item:cleanTurnipGuts", "playerAllegiance", null, delayForItemsAndAgents));
          list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startFingerFruit", new Vector2(-156f, 190f), "item:fingerFruit", "playerAllegiance", null, delayForItemsAndAgents));
        //  list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startCrystalBerries", new Vector2(-156f, 190f), "item:crystalBerries", "playerAllegiance", null, delayForItemsAndAgents));
       //   list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSalt", new Vector2(-156f, 190f), "item:salt", "playerAllegiance", null, delayForItemsAndAgents));
          list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startClayPotUnglazed", new Vector2(-156f, 190f), "item:clayPotUnglazed", "playerAllegiance", null, delayForItemsAndAgents));
        //  list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startTappingBucket", new Vector2(-156f, 190f), "item:tappingBucket", "playerAllegiance", null, delayForItemsAndAgents));
          list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startClay", new Vector2(-156f, 190f), "item:clay", "playerAllegiance", null, delayForItemsAndAgents));


          list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSpoakLeaves", new Vector2(-156f, 190f), "item:spoakLeaves", "playerAllegiance", null, delayForItemsAndAgents));
          list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSpoakBranchesTrimmed", new Vector2(-156f, 190f), "item:spoakBranchesTrimmed", "playerAllegiance", null, delayForItemsAndAgents));
      //    list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startMarshcotSap", new Vector2(-156f, 190f), "item:marshcotSap", "playerAllegiance", null, delayForItemsAndAgents));
          list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startWingweedMats", new Vector2(-156f, 190f), "item:wingweedMat", "playerAllegiance", null, delayForItemsAndAgents));

       //   list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startIronHandAxe", new Vector2(-156f, 190f), "item:steelHandAxe", "playerAllegiance", null, delayForItemsAndAgents));
          list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startWaterCaneStem", new Vector2(-156f, 190f), "item:waterCaneStem", "playerAllegiance", null, delayForItemsAndAgents));
          list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startShadeleafCanes", new Vector2(-156f, 190f), "item:shadeleafCanes", "playerAllegiance", null, delayForItemsAndAgents));
          list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startVat", new Vector2(-156f, 190f), "item:vat", "playerAllegiance", null, delayForItemsAndAgents));
          list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startImprovisedGreenHouseCover", new Vector2(-156f, 190f), "item:improvisedGreenHouseCover", "playerAllegiance", null, delayForItemsAndAgents));
          list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startTurnipSalami", new Vector2(-156f, 190f), "item:turnipSalami", "playerAllegiance", null, delayForItemsAndAgents));
  //        list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startDriedSaltedStreakFin", new Vector2(-156f, 190f), "item:driedSaltedStreakFin", "playerAllegiance", null, delayForItemsAndAgents));
        //  list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startVinegar", new Vector2(-156f, 190f), "item:vinegar", "playerAllegiance", null, delayForItemsAndAgents));
   


*/
            /////////////////////////////////

            //FOR TESTING:
            list.Add(Scenarios.ScenarioLoader.SpawnEntity("startClay", new Vector2(1814f, 1516f), "item:clay", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnEntity("startStone", new Vector2(1814f, 1516f), "item:stones", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnEntity("startWetMudBricks", new Vector2(1814f, 1516f), "item:wetMudBrick", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnEntity("startSolidMudBricks", new Vector2(1814f, 1516f), "item:solidMudBrick", "playerAllegiance", null, delayForItemsAndAgents, amount: 40));
            list.Add(Scenarios.ScenarioLoader.SpawnEntity("startFirewood", new Vector2(1814f, 1516f), "item:firewood", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnEntity("startFlintRough", new Vector2(1814f, 1516f), "item:flintRough", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnEntity("startScrapMetal", new Vector2(1814f, 1516f), "item:scrapMetal", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnEntity("startwaterCaneLeaves", new Vector2(1814f, 1516f), "item:waterCaneLeaves", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnEntity("startarrowshafts", new Vector2(1814f, 1516f), "item:improvisedArrowShaftBundle", "playerAllegiance", null, delayForItemsAndAgents));
            ////////////

            #endregion

            //materials stockpiled in open: 

            //     list.Add(Scenarios.ScenarioLoader.SpawnEntity("startCommonOilTubers", new Vector2(1814f, 1516f), "item:commonOilTubers", "playerAllegiance", null, delayForItemsAndAgents));

            //spawn inside containers:///////

            //in port: mp could not spawn items in port, I get an error.


            //in "Lean-to":
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startSmokedThunderChicken", "Lean-to1", "item:smokedThunderChicken", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startCommonOilTubers", "Lean-to1", "item:commonOilTubers", "playerAllegiance", null, delayForItemsAndAgents));


            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startPickaxe", "Lean-to1", "item:steelPickaxe", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startSteelSpade", "Lean-to1", "item:steelSpade", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startHoe", "Lean-to1", "item:farmingHoe", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startKnife", "Lean-to1", "item:steelKnife", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startGoldPot", "Lean-to1", "item:goldPot", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startBrickMold", "Lean-to1", "item:brickMold", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startRawhideString", "Lean-to1", "item:rawhideString", "playerAllegiance", null, delayForItemsAndAgents));

            #endregion

            #region Game duration


            // "#CLAYPIT_ENDDATE"
            list.Add(new SetPropertyAction()
            {
                KeyName = "initEndDate",

                PropertyKey = "endDate",
                Value = new UnaryFunctionNode() { Operator = UnaryExpressionOperator.DateFromRelativeDays, Operand = new ValueNode() { Decimal = 2f } }, //in-game days. Decimal = 2f                  

            });




            #endregion



            #region Starting Structures


            ////////////////////////////

            ///starting structures:
            list.Add(ScenarioLoader.SpawnEntity("startStructureKiln", new Vector2(1452f, 1726f), "structure:kiln", "playerAllegiance", null, delayForStructures));
            list.Add(ScenarioLoader.SpawnEntity("startStructureCampfire", new Vector2(1642f, 1748f), "structure:campfire", "playerAllegiance", null, delayForStructures));
            list.Add(ScenarioLoader.SpawnEntity("startStructureLean-toSpoakLeaves1", new Vector2(1630f, 1646f), "structure:lean-toSpoakLeaves", "playerAllegiance", null, delayForStructures, "Lean-to1"));
            list.Add(ScenarioLoader.SpawnEntity("startStructureLean-toSpoakLeaves2", new Vector2(1733f, 1796f), "structure:lean-toSpoakLeaves", "playerAllegiance", null, delayForStructures));
            list.Add(ScenarioLoader.SpawnEntity("startStructureA-frameSpoakLeaves1", new Vector2(1584f, 1692f), "structure:A-frameSpoakLeaves", "playerAllegiance", null, delayForStructures));
            list.Add(ScenarioLoader.SpawnEntity("startStructureA-frameSpoakLeaves2", new Vector2(1720f, 1709f), "structure:A-frameSpoakLeaves", "playerAllegiance", null, delayForStructures)); //1710f, 1586f



            list.Add(ScenarioLoader.SpawnEntity("startStructureAbatis1", new Vector2(1528f, 1324f), "structure:abatis", "playerAllegiance", null, delayForStructures));
            list.Add(ScenarioLoader.SpawnEntity("startStructureAbatis2", new Vector2(1544f, 1324f), "structure:abatis", "playerAllegiance", null, delayForStructures));
            list.Add(ScenarioLoader.SpawnEntity("startStructureAbatis3", new Vector2(1560f, 1324f), "structure:abatis", "playerAllegiance", null, delayForStructures));
            list.Add(ScenarioLoader.SpawnEntity("startStructureAbatis4", new Vector2(1576f, 1324f), "structure:abatis", "playerAllegiance", null, delayForStructures));
            list.Add(ScenarioLoader.SpawnEntity("startStructureAbatis5", new Vector2(1592f, 1324f), "structure:abatis", "playerAllegiance", null, delayForStructures));
            list.Add(ScenarioLoader.SpawnEntity("startStructureAbatis6", new Vector2(1608f, 1324f), "structure:abatis", "playerAllegiance", null, delayForStructures));

            // process actions must be done after people spawns:
            list.Add(ScenarioLoader.SpawnEntity("startStructureCanopyPort", new Vector2(1584f, 1898f), "structure:canopyPort", "playerAllegiance", null, delayForProcesses, "Port", processToUse: "buildCanopyPort", actingOnEntity: "Pier spot"));
            //subtile=48/3=16


            #endregion

            #region placeExpedition
            list.Add(new CreateExpeditionAction()
              {
                  KeyName = "placeExpedition",
                  DelayInSeconds = 0.1,

                  ExpeditionData = new ExpeditionData()
                  {
                      KeyName = "Camp",
                      Name = "Camp",
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
                                { RatingTypes.Comfort, "survival"  },
                                { RatingTypes.Food, "survival"  },
                                { RatingTypes.Security, "basic"  }
                            }                          
                      },
                      Location = new ValueNode()
                      {
                          PropertyKey = "startingLocation"
                      },
                     
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
                    Location = new Microsoft.Xna.Framework.Vector2(1632f, 1752f) // 
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


            list.Add(new ExploreAction()
            {

                KeyName = "exploreShroud",
                DelayInSeconds = delayForItemsAndAgents + 1, // must execute last, after members have been added


                DynamicLocationStart = new ValueNode()
                {
                    PropertyKey = "startingLocation"
                },
                /*
                new DynamicLocation()
                {
                    PropertyKey = "startingLocation"
                },*/
                RadiusStart = 500f,
                RadiusEnd = 600f,
                DetectMode = InGameEvents.Actions.DetectMode.DetectAlwaysSeenEntities,
                //PerformDetection = true,
                OffsetLocationEnd = new Microsoft.Xna.Framework.Vector2(1152f, 3024f), //mp the coordinates for the end point of the explored path

                EntityToExploreWith = new InGameEvents.PropertyObjects.TargetObject()
                {
                    GetList = new InGameEvents.PropertyObjects.GetList()
                    {
                        // get a random allegiance member
                        HasPropertiesListKey = "allegiances",
                        FilterCondition = new InGameEvents.Conditions.PropertyCondition() { PropertyKey = "keyName", ConstantStringEqual = "playerAllegiance" },
                        //   FilterProperty = "name",
                        //   FilterValue = "playerAllegiance",
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




            #region Binal rat Expedition #1  //west of camp
            list.Add(new SpawnAllegianceAction()
            {
                KeyName = "spawnBinalRatExpedition#1", //west of camp

                Site = "playSite",
                ExpeditionData = new ExpeditionData()
                {
                    KeyName = "binalRatAllegiance#1",
                    Name = "Binal Rat Allegiance #1",
                    AllegianceKey = "binalRatAllegiance#1",
                    Location = new ValueNode()
                    {
                        Location = new Vector2(624, 1930)
                    },
                    PopulationData = new PopulationData()
                    {
                        MaxMembers = 5,
                        StartMembers = 2,
                        GrowthInMembersPerDay = 10f,
                        RandomMembers = new StringChance[] 
                            {                                  
                                new StringChance() { Edge = 1f, String = "binalRat#1Allegiance#1" }                                            
                            }
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

            #region Binal Rat Expedition #2 // east of camp
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
                            Location = new Vector2(2074, 1872)
                        },
                    PopulationData = new PopulationData()
                    {
                        MaxMembers = 5,
                        StartMembers = 4,
                        GrowthInMembersPerDay = 10f,
                        RandomMembers = new StringChance[] {
                                    //equal distribution:
                                    new StringChance() { Edge = 0.5f, String = "binalRat#1Allegiance#2" },
                                    new StringChance() { Edge = 1f, String = "binalRat#2Allegiance#2" }                    
                                 }
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

            #region Binal rat Expedition #3 //North, blue creek
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
                        Location = new Vector2(1728, 894)
                    },
                    PopulationData = new PopulationData()
                    {
                        MaxMembers = 5,
                        StartMembers = 2,
                        GrowthInMembersPerDay = 10f,
                        RandomMembers = new StringChance[] 
                            {                                  
                                new StringChance() { Edge = 0.5f, String = "binalRat#1Allegiance#3" },
                                new StringChance() { Edge = 1f, String = "binalRat#2Allegiance#3" }                              
                            }
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


            #region Patrician #1
            list.Add(new SpawnAllegianceAction()
            {
                KeyName = "spawnPatricianExpedition#1",

                Site = "playSite",
                ExpeditionData = new ExpeditionData()
                {
                    KeyName = "patricianAllegiance#1",
                    Name = "Patrician Allegiance",
                    AllegianceKey = "patricianAllegiance#1",
                    Location = new ValueNode()
                    {
                        Location = new Vector2(1256, 300) //patricians will leave scout zone quite a lot in pursuit. so keep the expedition at a distance from camp.
                    },
                    PopulationData = new PopulationData()
                    {
                        MaxMembers = 5,
                        StartMembers = 4,
                        GrowthInMembersPerDay = 0.8f,
                        SpawnRadius = 300f,
                        RandomMembers = new StringChance[] {                              
                                new StringChance() { Edge = 0.20f, String = "patrician#1" },
                                new StringChance() { Edge = 1f, String = "patrician#2" }
                              /*  new StringChance() { Edge = 0.75f, String = "patrician#3" },
                                new StringChance() { Edge = 1f, String = "patrician#4" }*/
                             }
                    }
                },
                AllegianceData = new AllegianceData()
                {
                    ForageAndHuntingRadius = 480,
                    Name = "Patrician Allegiance",
                    KeyName = "patricianAllegiance#1",
                    EntityType = "entity:patrician",
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

            #region Mud Worm #1
            list.Add(new SpawnAllegianceAction()
            {
                KeyName = "spawnMudWormExpedition#1",

                Site = "playSite",
                ExpeditionData = new ExpeditionData()
                {
                    KeyName = "mudWormAllegiance#1",
                    Name = "Mud Worm Allegiance",
                    AllegianceKey = "mudWormAllegiance#1",
                    Location = new ValueNode()
                    {
                        Location = new Vector2(1152, 616) //placed to the north
                    },
                    PopulationData = new PopulationData()
                    {
                        MaxMembers = 15,
                        StartMembers = 3,
                        GrowthInMembersPerDay = 8f, // 10f,
                        RandomMembers = new StringChance[] {                               
                               new StringChance() { Edge = 1f, String = "mudWorm#1" }
                             }
                    }
                },
                AllegianceData = new AllegianceData()
                {
                    ForageAndHuntingRadius = 380,
                    Name = "Mud Worm Allegiance",
                    KeyName = "mudWormAllegiance#1",
                    EntityType = "entity:mudWorm",
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
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            #region Mud Worm #2
            list.Add(new SpawnAllegianceAction()
            {
                KeyName = "spawnMudWormExpedition#2",

                Site = "playSite",
                ExpeditionData = new ExpeditionData()
                {
                    KeyName = "mudWormAllegiance#2",
                    Name = "Mud Worm Allegiance",
                    AllegianceKey = "mudWormAllegiance#2",
                    Location = new ValueNode()
                    {
                        Location = new Vector2(2037, 1809) //Trigger spawn at trees might have to be put further down
                    }
                    //Lars:  no members it seems! delete?
                },
                AllegianceData = new AllegianceData()
                {
                    ForageAndHuntingRadius = 180,
                    Name = "Mud Worm Allegiance",
                    KeyName = "mudWormAllegiance#2",
                    EntityType = "entity:mudWorm",
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


            #region Thunder Chicken Expedition #2  //to the north east of blue creek
            list.Add(new SpawnAllegianceAction()
            {
                KeyName = "spawnThunderChickenExpedition#2",

                Site = "playSite",
                ExpeditionData = new ExpeditionData()
                {
                    KeyName = "thunderChickenAllegiance#2",
                    Name = "Thunder Chicken Allegiance",
                    AllegianceKey = "thunderChickenAllegiance#2",
                    Location = new ValueNode()
                    {
                        Location = new Vector2(2824, 432)
                    },
                    PopulationData = new PopulationData()
                    {
                        MaxMembers = 2,
                        StartMembers = 1,
                        GrowthInMembersPerDay = 0.4f, //chickens are rare here.
                        RandomMembers = new StringChance[] {
                                //equal distribution:
                                new StringChance() { Edge = 0.33f, String = "studdedThunderChicken#1" },
                                new StringChance() { Edge = 0.66f, String = "studdedThunderChicken#2" },
                                new StringChance() { Edge = 1f, String = "studdedThunderChicken#3" }             
                             }
                    }
                },
                AllegianceData = new AllegianceData()
                {
                    ForageAndHuntingRadius = 400,
                    Name = "Thunder Chicken Allegiance",
                    KeyName = "thunderChickenAllegiance#2",
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

            #region Birds
            list.Add(new SpawnAllegianceAction()
            {
                KeyName = "spawnBirdExpedition#1",

                Site = "playSite",
                ExpeditionData = new ExpeditionData()
                {
                    KeyName = "birdExpedition#1",
                    AllegianceKey = "birdAllegiance#1",
                    Location = new ValueNode()
                    {
                        Location = new Vector2(2688, 1008)
                    },
                    PopulationData = new PopulationData()
                    {
                        StartMembersList = new string[]
                            {
                                "bird#1",  "bird#2",  "bird#3",  "bird#4",  "bird#5", "bird#6",
                            }
                    }
                },
                AllegianceData = new AllegianceData()
                {
                    KeyName = "birdAllegiance#1",
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




            #endregion

            #region Spawn intervals and pop caps. DELETE. moved to expeditions above

            #region spawn intervals



            //  there's 1600 sec / day
            // scenario starts at TimeOfDay = 0.50   
            //RelativeNoOfDays = 0.5 //getting dark
            //RelativeNoOfDays = 0.795  // morning
            //RelativeNoOfDays = 1.123  //afternoon              
            //RelativeNoOfDays = 1.695  //night 

            /* list.Add(new EventActionType()
             {
                 KeyName = "setSpawnIntervalPatriciansNormal",
                
                     PropertyKey = "patriciansSpawnInterval",
                     Value = new ValueNode() { Int = 2000 },//2000
                 }
             });*/

            /*
            list.Add(new EventActionType()
            {
                KeyName = "setSpawnIntervalMudWormsNormal",
                
                    PropertyKey = "mudWormsSpawnInterval",
                    Value = new ValueNode() { Int = 160 },
                }
            });*/

            /* list.Add(new EventActionType()
             {
                 KeyName = "setSpawnIntervalBinalRatsNormal",
                
                     PropertyKey = "binalRatSpawnInterval",
                     Value = new ValueNode() { Int = 160 },
                 }
             });*/

            /*
            list.Add(new EventActionType() 
            {
                KeyName = "setSpawnIntervalThunderChickensNormal",
                
                    PropertyKey = "thunderChickenSpawnInterval",
                    Value = new ValueNode() { Int = 3600 }, //chickens are rare here.
                }
            });
            */


            #endregion

            #region max pop

            // Remember that the actual spawned amount is the Value +1  ........MP it is true. if max is set to 5 then they can spawn until 6 is on the map. but why???

            /* list.Add(new EventActionType()
             {
                 KeyName = "setMaxPatriciansNormal",
                
                     PropertyKey = "maxPatricians",
                     Value = new ValueNode() { Int = 4 }, //gives a max of 5
                 }
             });*/

            /* list.Add(new EventActionType()
             {
                 KeyName = "setMaxMudWormsNormal",
                
                     PropertyKey = "maxMudWorms",
                     Value = new ValueNode() { Int = 15 },
                 }
             });*/


            /* list.Add(new EventActionType()
             {
                 KeyName = "setMaxThunderChickensNormal",
                
                     PropertyKey = "maxThunderChickens",
                     Value = new ValueNode() { Int = 2 },
                 }
             });*/

            /*
            list.Add(new EventActionType()
            {
                KeyName = "setMaxBinalRatsNormal",
                
                    PropertyKey = "maxBinalRats",
                    Value = new ValueNode() { Int = 5 }, 
                }
            });*/



            #endregion

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
                    Location = new Vector3(1584f, 1898f, 0) //was 1536f, 1536f, 0
                },

            });


            #endregion
            //mp no fish traps made from shadeleaf because there are no shadeleaf trees on map.
            #region Fish weir spots
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFishTrapCreek1",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:fishTrapSpotCreek",
                    Name = "Fish weir spot",
                    Location = new Vector3(1018f, 1000f, 0) //
                },

            });

            // 
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFishTrapCreek2",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:fishTrapSpotCreek",
                    Name = "Fish weir spot",
                    Location = new Vector3(1610f, 1132f, 0)
                },

            });
            // 
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFishTrapCreek3",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:fishTrapSpotCreek",
                    Name = "Fish weir spot",
                    Location = new Vector3(1980f, 1014f, 0)
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
                    Location = new Vector3(790f, 1510f, 0) //
                },

            });
            #endregion
            #region bog ore deposit 
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startBogOreDeposit1",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:bogOreDeposit",
                    Name = "Bog ore deposit",
                    Location = new Vector3(624f, 528f, 0) //
                },

            });
            #endregion

            #region peat deposit
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startPeatDeposit1",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:peatDeposit",
                    Name = "Bog ore deposit",
                    Location = new Vector3(2190f, 630f, 0) //north east. quite far away
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
                      Location = new Microsoft.Xna.Framework.Vector2(1930f, 1500f)
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
                      Location = new Microsoft.Xna.Framework.Vector2(1968f, 1392f)
                      //    Location = Maps.MapManager.TileToWorldPosVector2(new Point(18, 43))
                  },
                  ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "smallFog" } }

              });

            list.Add(
              new ParticleEffectAction()
              {
                  KeyName = "smallFog3",     //Little ""steam" rising from pond      

                  Location = new ValueNode()
                  {
                      Location = new Microsoft.Xna.Framework.Vector2(1344f, 960f)
                      //    Location = Maps.MapManager.TileToWorldPosVector2(new Point(18, 43))
                  },
                  ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "smallFog" } }

              });
            #endregion
            #region swamp fog


            list.Add(
              new ParticleEffectAction()
              {
                  KeyName = "fog1",     //smaller fog at right     

                  Scale = 5f,
                  Location = new ValueNode()
                  {
                      Location = new Microsoft.Xna.Framework.Vector2(1920f, 1440f)

                  },
                  ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "fog" } }

              });






            #endregion






            #endregion






            return list;
        }
    }
}
