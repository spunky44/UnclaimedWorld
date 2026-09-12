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

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_7.Data
{
    public class EventActionLoader
    {
        public static List<EventActionType> Init()
        {
            List<EventActionType> list = new List<EventActionType>();


            double delayForStructures = 0.25;
            double delayForItemsAndAgents = 0.5;

            

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
                KeyName = "initBurialText2",
                PropertyKey = "burialTextMultipleDeathsMultipleSurvivors",
                Value = new ValueNode() { String = "Life on Antheia was hard and death could happen at any moment. \nLosing #NAMEOFDECEASED#CAUSEOFDEATH was a cause of grief, but at the burial, #EUOLOGYGIVER emphasized the value of a life in freedom, no matter its duration." }
            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "initBurialText3",
                PropertyKey = "burialTextSingleDeathMultipleSurvivors",
                Value = new ValueNode() { String = "Life on Antheia was hard and death could happen at any moment. \nLosing #NAMEOFDECEASED#CAUSEOFDEATH was a cause of grief, but at the burial, #EUOLOGYGIVER emphasized the value of a life in freedom, no matter its duration." }
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
            #region Group meeting dialog texts
            //
            list.Add(new SetPropertyAction()
            {
                KeyName = "initTimeBeforeGroupMeeting",
                PropertyKey = "timeInGameSecondsBeforeGroupMeeting",
                Value = new ValueNode() { Decimal = 200 }
            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "meetingEmigrateThreat",
                PropertyKey = "meetingEmigrateThreat",
                Value = new ValueNode() { String = "If not...well, I might leave for #EMIGRATETO and start over." }
            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "meetingEmigrateThreatAllUnhappy",
                PropertyKey = "meetingEmigrateThreatAllUnhappy",
                Value = new ValueNode() { String = "It seems like everyone is just waiting for an excuse to pack up and leave for #EMIGRATETO to start over." }
            });

            #region 3 people -meetings
            list.Add(new SetPropertyAction()
            {
                KeyName = "meeting3Security",

                PropertyKey = "meeting3Security",
                Value = new ValueNode()
                {
                    String = "#UNHAPPY: Thanks for taking time to listen... It's about the security situation. The way it's handled, I don't think we're safe here. \n \n"
                                + "#CONTENT: I don't see the problem. What do you want done? \n \n"
                                + "#UNHAPPY: I want us to beef up security. How we do it? Stock more and better weapons, take fewer risks... There's a number of ways we can protect our colony better. Main thing is that we take action now. \n \n"
                                + "#CONTENT: This is all a question of priorities... \n \n"
                                + "#UNHAPPY: Exactly. And if we value our lives, we need better protection from wild animals. So I hope you're with me. #EMIGRATETHREAT \n \n"
                                + "#CONTENT: Let's make sure it doesn't come to that. Anyone has more to add?"
                }

            });



            list.Add(new SetPropertyAction() // 
            {
                KeyName = "meeting3Food",

                PropertyKey = "meeting3Food",
                Value = new ValueNode()
                {
                    String = "#UNHAPPY: I'm really worried about the food situation. \n \n"
                                + "#CONTENT: What do you want done? \n \n"
                                + "#UNHAPPY: I want us to build up a bigger stockpile of food. We need more smoked meat and fish, dry staples...  \n \n"
                                + "#CONTENT: But aren't we already working on that? \n \n"
                                + "#UNHAPPY: It's not going quick enough. At this rate, a minor event could cause starvation. So I hope you'll improve this. #EMIGRATETHREAT \n \n"
                                + "#CONTENT: That would be pity. Who else has any gripes?"
                }

            });


            list.Add(new SetPropertyAction() // 
            {
                KeyName = "meeting3Comfort",

                PropertyKey = "meeting3Comfort",
                Value = new ValueNode()
                {
                    String = "#UNHAPPY: Ok, I'm gonna go out on a limb here, at the risk of sounding like I'm whining... \n \n"
                                + "#CONTENT: Speak up. \n \n"
                                + "#UNHAPPY: When I came to this place, I was prepared to endure hardships on the frontier, but... \n \n" //toughen it out
                                + "#CONTENT: But? \n \n"
                                + "#UNHAPPY: Living conditions here are dreadful and they improve so slowly. Could we please work on getting better houses and some minor luxuries. I'm not asking for much, so I hope you agree. #EMIGRATETHREAT \n \n"
                                + "#CONTENT: As if we don't have enough to worry about. Anyone else has complaints?"
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
                    String = "#UNHAPPY: We need better shelters. \n \n"
                                + "#CONTENT: Why is it so important. We have all we need. \n \n"
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
                    String = "#UNHAPPY:  Look, we all want the security situation to improve! So let's get our act together and start working as a team! #EMIGRATETHREAT"
                }

            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "meetingFoodAllUnhappy",

                PropertyKey = "meetingFoodAllUnhappy",
                Value = new ValueNode()
                {
                    String = "#UNHAPPY:  Look, we all want the food situation to improve! So let's get our act together and start working as a team! #EMIGRATETHREAT"
                }

            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "meetingComfortAllUnhappy",

                PropertyKey = "meetingComfortAllUnhappy",
                Value = new ValueNode()
                {
                    String = "#UNHAPPY:  Look, we all want the comfort conditions to improve! So let's get our act together and start working as a team! #EMIGRATETHREAT"
                }

            });
            #endregion
            #endregion

            #region Emigrate dialog texts


            list.Add(new SetPropertyAction()
            {
                KeyName = "initEmigrateSecurityDialogText",
                PropertyKey = "securityEmigrateEventDialogText",
                Value = new ValueNode() { String = "AUDIO LOG, #JOURNALDATE \n \n#NAME1: Security around here is appalling. We're in the middle of a dangerous wilderness and I can't believe the risks that we're taking. It's come to the point that I'm afraid to sleep. \n \n#NAME2: Come on. Of course there are wild animals out there but nothing we can't deal with. \n \n#NAME1: When an animal attack happens - AND IT WILL! I'm not going to be around. I'm leaving now. You can find me at #EMIGRATIONTARGET if you want to get in touch." } //followed up with some banter.
            });


            list.Add(new SetPropertyAction()
            {
                KeyName = "initEmigrateSecurityNoConversationDialogText",
                PropertyKey = "securityEmigrateEventNoConversationDialogText",
                Value = new ValueNode() { String = "TEXT LOG, #JOURNALDATE \n \n#NAME1: When you read this, I'll be leaving. This place scares me. We're in danger from wild animals here, but I seem to be the only one who takes this threat seriously. I'm going to #EMIGRATIONTARGET where I'll be safe. \nGoodbye." }
            });


            list.Add(new SetPropertyAction()
            {
                KeyName = "initEmigrateComfortDialogText",
                PropertyKey = "comfortEmigrateEventDialogText",
                Value = new ValueNode() { String = "AUDIO LOG, #JOURNALDATE \n \n#NAME1: This place is a pig sty. I can't stand it. The cold, the bugs and the filth...why are you ok with living like this? \n \n#NAME2: Because there are other things that are more important? \n \n#NAME1: I'm fed up. These conditions here, they're subhuman. I'm leaving for #EMIGRATIONTARGET. I can do better on my own." } //followed up with some banter.
            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "initEmigrateComfortNoConversationDialogText",
                PropertyKey = "comfortEmigrateEventNoConversationDialogText",
                Value = new ValueNode() { String = "TEXT LOG, #JOURNALDATE \n \n#NAME1: This is my final goodbye. I'm fed up with this squalor. These conditions here, they're subhuman. I'm going to #EMIGRATIONTARGET. I can do better on my own." }
            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "initEmigrateFoodDialogText",
                PropertyKey = "foodEmigrateEventDialogText",
                Value = new ValueNode() { String = "AUDIO LOG, #JOURNALDATE \n \n#NAME1: I can't do it anymore. I'm so worried about food all the time! Can't you see we're this close to starvation? \n \n#NAME2: Hey! We agreed to focus on other things! \n#NAME1: What could be more important than food?! I've had it. I'm going to #EMIGRATIONTARGET." }//followed up with some banter.
            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "initEmigrateFoodNoConversationDialogText",
                PropertyKey = "foodEmigrateEventNoConversationDialogText",
                Value = new ValueNode() { String = "TEXT LOG, #JOURNALDATE \n \n#NAME1: When you read this, I'll be on my way. This constant fretting about food and waiting for an inevitable disaster has me worried sick. I'm going to #EMIGRATIONTARGET. Don't come knocking when your food runs out." }
            });

            #endregion

            list.Add(new SetPropertyAction()
            {
                KeyName = "initGameOver",
                PropertyKey = "gameOver",
                Value = new ValueNode() { Bool = false }
            });

            #endregion


            #region startNeeds. Used for all characters when they appear.
            SerializableDictionary<string, NeedData> startNeeds = new SerializableDictionary<string, NeedData>() 
            {
                { "protein", new NeedData(){ Level = new NormalDistribution() { Mean = 0.9f, StandardDeviation = 0.02f } }},
                { "foodEnergy", new NeedData(){ Level = new NormalDistribution() { Mean = 0.9f, StandardDeviation = 0.02f } }},
                { "micronutrients", new NeedData(){ Level = new NormalDistribution() { Mean = 0.9f, StandardDeviation = 0.02f } }},
                { "stimulants", new NeedData(){ Level = new NormalDistribution() { Mean = 0.4f, StandardDeviation = 0.02f } }}
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

                        ViewLongitudeStart = 5, 
                        ViewLongitudeEnd = 30,
                        ViewLatitudeStart = 50,
                        ViewLatitudeEnd = 75 
                    }

                });

            #endregion

            //Playsite:
            #region spawnPlaySite
            list.Add(
                new SpawnSiteAction()
                {
                    KeyName = "spawnPlaySite",
                 //   SiteDataKey = "playSite",// not linked to sitedataloader
                    
                    SiteData = new SiteData()
                    {
                        Name = "Nadova's Site", //"Zone J-23"
                        KeyName = "playSite",
                        Description = "The location of the Nadova Mining expedition. Designated by the Overseer AI as a mineral extraction site for Project CANOPY", //"One of several mineral extraction sites for Project CANOPY, designated by the Overseer AI"
                        Coords = new Overland.Locations.GeodeticCoordinate(14d, 59d), //make sure they can travel by boat in straight line to and from. //Latitude = , Longitude =  },
                        IsPlaySite = true,
                        ShowLabel = true,
                        ShowTallPin = true,
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
                        Name = "Mining Camp", 
                        KeyName = "playerAllegiance",
                        EntityType = "entity:human",
                        AllegianceType = Allegiances.AllegianceType.Player                       
                    }
                });

            list.Add(
                new ChangeCreditsAction()
                {
                    KeyName = "setPlayerCredits",
                    
                    AllegianceKey = "playerAllegiance",
                    Amount = new ValueNode()
                    {
                        Decimal = 350 // 350
                    }

                });
            #endregion

            #region spawnOtherSite2 - random site profile close by
            list.Add(
                new SpawnSiteAction()
                {
                    KeyName = "spawnOtherSite2", 
                   // SiteDataKey = "randomSmallAdvancedSite" 
                   
                    SiteData = new SiteData()
                    { 
                        KeyName = "randomSmallAdvancedSite",
                        SiteTemplates = new[] { new StringChance() { Edge = 0.33f, String = "smallAdvancedFarmingSite" }, new StringChance() { Edge = 0.66f, String = "smallAdvancedMiningSite" }, new StringChance() { Edge = 1f, String = "smallAdvancedFishingSite" } },
                        Coords = new Overland.Locations.GeodeticCoordinate(15.5d, 58.6d),
                        AllegianceKeyName = "othersite2Allegiance", // overrides template key names
                        ExpeditionKeyName = "othersite2Expedition", // overrides template key names
                        IsPlaySite = false,
                        ShowLabel = true,
                        ShowTallPin = false,
                        SiteMarkerOrder = 10
                    }       
                });
            #endregion
        


            //Othersite1. for trading:
            #region spawnOtherSite1
            list.Add(
                new SpawnSiteAction()
                {
                    KeyName = "spawnOtherSite1",                   
                    SiteData = new SiteData()
                    {                      
                        KeyName = "dukesLanding",
                        Name = "Duke's Landing",                     
                        Description = "The planet's largest settlement is the focal point for the Project CANOPY effort. They will receive our shipments of minerals.",
                        Coords = new Overland.Locations.GeodeticCoordinate(16d, 71d), //8.24655d, 69.83451d),                       
                        IsPlaySite = false,
                        ShowLabel = true,
                        ShowTallPin = true,
                        SiteMarkerOrder = 10
                    }
                });
            #endregion
            #region spawnOtherSite1Allegiance1
            list.Add(
                new SpawnAllegianceAction()
                {
                    KeyName = "spawnOtherSite1Allegiance1",

                    Site = "dukesLanding", //PlanetFallSite", // "otherSite1",
                    AllegianceData = new AllegianceData()
                    {
                        KeyName = "dukesLandingAllegiance",
                        Name = "Duke's Landing",                      
                        EntityType = "entity:human",
                        PermitsImmigration = false,
                        AllegianceType = Allegiances.AllegianceType.Other,
                        StatsData = new StatsData()
                        {
                            Security = .60f,
                            Comfort = .40f,
                            FoodSupply = .35f,
                        }                    
                    },
                   // AllegianceDataKey = "dukesLandingAllegiance"
                });
            #endregion
            #region spawnOtherSite1Expedition1
            list.Add(new CreateExpeditionAction()
            {
                KeyName = "spawnOtherSite1Expedition1",// not player's site.
                DelayInSeconds = 0.1, // wait for starting location property to have been set!

                AllegianceKey = "dukesLandingAllegiance",              
                ExpeditionData = new ExpeditionData()
                {
                    KeyName = "dukesLandingPlanetFallExpedition", //"bigTradingHubProfileExpedition",
                    Name = "Duke's Landing", // the wharf
                    AllegianceKey = "dukesLandingAllegiance", // required..(!)
                    SizeFactor = 2.5f,
                    AvailableForTrade = new SerializableDictionary<string,TradeAmountType>()
                    {
                        // set fixed amounts
                          { "item:scandium", new TradeAmountType(){ LinearIncreasePerDay = 0f, LinearConsumptionPerDay = 0f, AmountToBuy = 40, MaxAmountToBuy = 40  }},
                          { "item:terbium", new TradeAmountType(){ LinearIncreasePerDay = 0f, LinearConsumptionPerDay = 0f, AmountToBuy = 20, MaxAmountToBuy = 20  }},

                          { "item:sentry", new TradeAmountType(){ MaxAmountForSale = 12, StartAmount = new NormalDistribution() { Mean = 12 }  }},
                          { "item:sentryGunAmmo", new TradeAmountType(){ MaxAmountForSale = 100, StartAmount = new NormalDistribution() { Mean = 100 }  }},
                          { "item:coilRifle", new TradeAmountType(){ MaxAmountForSale = 15, StartAmount = new NormalDistribution() { Mean = 15 }  }},
                          { "item:shotgun", new TradeAmountType(){ MaxAmountForSale = 10, StartAmount = new NormalDistribution() { Mean = 10 }  }},
                          { "item:coilRifleAmmo", new TradeAmountType(){ MaxAmountForSale = 75, StartAmount = new NormalDistribution() { Mean = 75 }  }},                      
                          { "item:shotgunAmmo", new TradeAmountType(){ MaxAmountForSale = 50, StartAmount = new NormalDistribution() { Mean = 50 }  }},
       
                          { "item:astroRation", new TradeAmountType(){ MaxAmountForSale = 50, StartAmount = new NormalDistribution() { Mean = 50 }  }},
                          { "item:simCoffeeBeans", new TradeAmountType(){ MaxAmountForSale = 40, StartAmount = new NormalDistribution() { Mean = 40 }  }},

                          { "item:advancedKnife", new TradeAmountType(){ MaxAmountForSale = 20, StartAmount = new NormalDistribution() { Mean = 20 }  }},
                          { "item:advancedString", new TradeAmountType(){ MaxAmountForSale = 30, StartAmount = new NormalDistribution() { Mean = 30 }  }},
                          { "item:advancedMachete", new TradeAmountType(){ MaxAmountForSale = 15, StartAmount = new NormalDistribution() { Mean = 15 }  }},
                          { "item:advancedSnips", new TradeAmountType(){ MaxAmountForSale = 10, StartAmount = new NormalDistribution() { Mean = 10 }  }},
                          { "item:advancedCookingPot", new TradeAmountType(){ MaxAmountForSale = 10, StartAmount = new NormalDistribution() { Mean = 10 }  }},                   

                          { "item:fieldLabPacked", new TradeAmountType(){ MaxAmountForSale = 5, StartAmount = new NormalDistribution() { Mean = 5 }  }}, 
                          { "item:groundScanner", new TradeAmountType(){ MaxAmountForSale = 5, StartAmount = new NormalDistribution() { Mean = 5 }  }}, 
                          { "item:cloak", new TradeAmountType(){ MaxAmountForSale = 5, StartAmount = new NormalDistribution() { Mean = 5 }  }}, 
                          { "item:nightVisionGoggles", new TradeAmountType(){ MaxAmountForSale = 10, StartAmount = new NormalDistribution() { Mean = 10 }  }}, 
                          { "item:sensor", new TradeAmountType(){ MaxAmountForSale = 10, StartAmount = new NormalDistribution() { Mean = 10 }  }}, 
                          { "item:satelliteGroundStation", new TradeAmountType(){ MaxAmountForSale = 3, StartAmount = new NormalDistribution() { Mean = 3 }  }}, 
                          { "item:octagonalTent", new TradeAmountType(){ MaxAmountForSale = 6, StartAmount = new NormalDistribution() { Mean = 6 }  }}, 
                          { "item:smallTent", new TradeAmountType(){ MaxAmountForSale = 8, StartAmount = new NormalDistribution() { Mean = 8 }  }}, 
                          { "item:domeTent", new TradeAmountType(){ MaxAmountForSale = 5, StartAmount = new NormalDistribution() { Mean = 5 }  }}, 
                          { "item:thermalTarp", new TradeAmountType(){ MaxAmountForSale = 6, StartAmount = new NormalDistribution() { Mean = 6 }  }}, 
                          { "item:diamondGlass", new TradeAmountType(){ MaxAmountForSale = 10, StartAmount = new NormalDistribution() { Mean = 10 }  }}, 
                          { "item:fieldKitchenStove", new TradeAmountType(){ MaxAmountForSale = 4, StartAmount = new NormalDistribution() { Mean = 4 }  }}, 
                          { "item:fieldKitchenEquipment", new TradeAmountType(){ MaxAmountForSale = 4, StartAmount = new NormalDistribution() { Mean = 4 }  }}, 
                          { "item:liquidGas", new TradeAmountType(){ MaxAmountForSale = 30, StartAmount = new NormalDistribution() { Mean = 30 }  }},       
                   
                    },
                   // TradeProfile = "advancedTradingProfile",
                    StructuresProfile = "largeHeliportProfile",
                    VehiclesProfile = "airliftProfile",
                    PricesProfile = "planetFallEraPrices"
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
                        Allegiance2 = "dukesLandingAllegiance",
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
                        Allegiance2 = "dukesLandingAllegiance",
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
                        Allegiance2 = "dukesLandingAllegiance",
                        Relation = 0f
                    }

                });

            #endregion


            #region Routes
            list.Add(
                new SpawnRouteAction()
                {
                    KeyName = "spawnPlaySiteWildernessSite1Route",
                    DelayInSeconds = 1,

                    RouteData = new RouteData()
                    {
                        Name = "Passage to The Plains", //was: North passage
                        FromSite = "playSite",
                        ToSite = "wildernessSite1",
                        Length = 10,
                        RouteType = RouteType.Land
                    }

                });

            list.Add(
               new SpawnRouteAction()
               {
                   KeyName = "spawnPlaySiteSite2Route",
                   DelayInSeconds = 1,

                   RouteData = new RouteData()
                   {
                       Name = "North coast",
                       FromSite = "playSite",
                       ToSite = "randomSmallAdvancedSite", // "dukesLandingPlanetFallSite",
                       Length = 100,
                       RouteType = RouteType.CalmWater
                   }

               });

           /* list.Add(
               new SpawnRouteAction()
               {
                   KeyName = "spawnPlaySiteSite1Route",
                   DelayInSeconds = 1,

                   RouteData = new RouteData()
                   {
                       Name = "Air corridor",
                       FromSite = "playSite",
                       ToSite = "dukesLanding", // "dukesLandingPlanetFallSite",
                       Length = 120, 
                       RouteType = RouteType.CalmWater
                   }

               });*/
            #endregion

            #region spawnWildernessSite1

            list.Add(
               new SpawnSiteAction()
               {
                   KeyName = "spawnWildernessSite1",
                   //mp: used to have VisualCommunicationRangeInKms = 10  (yelling distance)but was too small to make it work on the map, so increased it in Constants.cs
                   //   DistanceFromPlaySite = 9f, // 9 km away MP uncomment this to test distance. ('yelling' distance is below 10 km.) (comment out the coords below when testing)
                   //    BearingFromPlaySite = 2f, // MP: DOES NOT WORK // radians

                   SiteData = new SiteData()
                   {
                       Name = "The Valley",
                       KeyName = "wildernessSite1",
                       Description = "An alternative location in the wilderness, considered by some as a better place for settling.",
                       Coords = new Overland.Locations.GeodeticCoordinate(13.8, 58.8d),//9.22d, 71.8d // 10.24655d, 55.83451d
                       IsPlaySite = false,
                       ShowLabel = false,
                       ShowTallPin = false, //
                       //  SiteMarkerOrder = 10,
                   }


               });

            list.Add(
               new SpawnAllegianceAction()
               {
                   KeyName = "spawnWildernessSite1Allegiance1",

                   Site = "wildernessSite1",
                   AllegianceData = new AllegianceData()
                   {
                       Name = "The Valley",
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
            
            #region OtherSite1 Possible Immigrants

            #region spawnImmigrantSmithingSpecialist
            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnImmigrantSmithingSpecialist",
                DelayInSeconds = delayForItemsAndAgents,

                // DynamicLocation = new DynamicLocation()   PropertyKey = "startingLocation"  },
                EntityData = new Maps.MapEditor.EntityData()
                {
                    //  Location = new Microsoft.Xna.Framework.Vector3(170f, -30f, 0),
                    EntityKey = "entity:human",
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "dukesLandingAllegiance",
                        ExpeditionKey = "dukesLandingPlanetFallExpedition"
                    },
                    Person = new Maps.MapEditor.Person()
                    {
                        PersonalityType = "earlyJoinerPersonality", //early immigrant

                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {
                        TraitTemplates = new StringChance[] {                                  
                                new StringChance() { Edge = 1f, String = "smithingSpecialist" } },

                        #region CultureTemplates. Future, 100% equality
                        CultureTemplates = new StringChance[] {
                                //equal distribution:
                                new StringChance() { Edge = 0.2f, String = "malePlanetfallWhiteAngloCulture" },
                                new StringChance() { Edge = 0.25f, String = "malePlanetfallWhiteRussianCulture" }, 
                                new StringChance() { Edge = 0.3f, String = "malePlanetfallChineseCulture" },
                                new StringChance() { Edge = 0.35f, String = "malePlanetfallJapaneseCulture" },  
                                new StringChance() { Edge = 0.45f, String = "malePlanetfallHispanicCulture" }, 
                                new StringChance() { Edge = 0.5f, String = "malePlanetfallAfricanCulture" }, 

                                new StringChance() { Edge = 0.75f, String = "femalePlanetfallWhiteAngloCulture" },                                
                                new StringChance() { Edge = 0.89f, String = "femalePlanetfallRussianCulture" }, 
                                new StringChance() { Edge = 1f, String = "femalePlanetfallAfricanCulture" },                         
                            }
                        #endregion
                    },
                    NeedLevels = startNeeds                   
                }

            });
            #endregion
            #region spawnImmigrantMenialSpecialist
            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnImmigrantMenialSpecialist",
                DelayInSeconds = delayForItemsAndAgents,

                // DynamicLocation = new DynamicLocation()   PropertyKey = "startingLocation"  },
                EntityData = new Maps.MapEditor.EntityData()
                {
                    //  Location = new Microsoft.Xna.Framework.Vector3(170f, -30f, 0),
                    EntityKey = "entity:human",
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "dukesLandingAllegiance",
                        ExpeditionKey = "dukesLandingPlanetFallExpedition"
                    },
                    Person = new Maps.MapEditor.Person()
                    {
                        PersonalityType = "earlyJoinerPersonality", //early immigrant

                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {
                        TraitTemplates = new StringChance[] {                                  
                                new StringChance() { Edge = 1f, String = "menialSpecialist" } },

                        #region CultureTemplates. Future, 100% equality
                        CultureTemplates = new StringChance[] {
                                //equal distribution:
                                new StringChance() { Edge = 0.2f, String = "malePlanetfallWhiteAngloCulture" },
                                new StringChance() { Edge = 0.25f, String = "malePlanetfallWhiteRussianCulture" }, 
                                new StringChance() { Edge = 0.3f, String = "malePlanetfallChineseCulture" },
                                new StringChance() { Edge = 0.35f, String = "malePlanetfallJapaneseCulture" },  
                                new StringChance() { Edge = 0.45f, String = "malePlanetfallHispanicCulture" }, 
                                new StringChance() { Edge = 0.5f, String = "malePlanetfallAfricanCulture" }, 

                                new StringChance() { Edge = 0.75f, String = "femalePlanetfallWhiteAngloCulture" },                                
                                new StringChance() { Edge = 0.89f, String = "femalePlanetfallRussianCulture" }, 
                                new StringChance() { Edge = 1f, String = "femalePlanetfallAfricanCulture" },                         
                            }
                        #endregion
                    },
                    NeedLevels = startNeeds

                }

            });
            #endregion

            //MP: the survival and basic tier immigrants will often be equally likely to join early, becuase of the influence ofhappiness at othersite and the different personal interest number.  
            //(NOTE: I'm not using personal circumstances because it complicates it too much)
            #region spawnImmigrantMediumTier //the first immigrants that the player can get.
            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnImmigrantMediumTier",
                DelayInSeconds = delayForItemsAndAgents,

                // DynamicLocation = new DynamicLocation()   PropertyKey = "startingLocation"  },
                EntityData = new Maps.MapEditor.EntityData()
                {
                    //  Location = new Microsoft.Xna.Framework.Vector3(170f, -30f, 0),
                    EntityKey = "entity:human",
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "dukesLandingAllegiance",
                        ExpeditionKey = "dukesLandingPlanetFallExpedition"
                    },
                    Person = new Maps.MapEditor.Person()
                    {
                        PersonalityType = "mediumTierPersonality", //Was survivalTierPersonality

                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {
                        #region TraitTemplates.  100% random
                        TraitTemplates = new StringChance[] { 
                                new StringChance() { Edge = 0.091f, String = "electronicsSpecialist" },
                                new StringChance() { Edge = 0.182f, String = "mechanicsSpecialist" },
                                new StringChance() { Edge = 0.273f, String = "chemistrySpecialist" },  
                                new StringChance() { Edge = 0.364f, String = "smithingSpecialist" },                           
                                new StringChance() { Edge = 0.455f, String = "farmingSpecialist" }, 
                                new StringChance() { Edge = 0.545f, String = "constructionSpecialist" },                             
                                new StringChance() { Edge = 0.636f, String = "huntingSpecialist" }, 
                                new StringChance() { Edge = 0.727f, String = "menialSpecialist" },                           
                                new StringChance() { Edge = 0.818f, String = "cookingSpecialist" }, 
                                new StringChance() { Edge = 0.909f, String = "bushcraftSpecialist" }, 
                                new StringChance() { Edge = 1f,     String = "securitySpecialist" }, 
                           
                            },
                        #endregion

                        #region CultureTemplates. Future, 100% equality
                        CultureTemplates = new StringChance[] {
                                //equal distribution:
                                new StringChance() { Edge = 0.2f, String = "malePlanetfallWhiteAngloCulture" },
                                new StringChance() { Edge = 0.25f, String = "malePlanetfallWhiteRussianCulture" }, 
                                new StringChance() { Edge = 0.3f, String = "malePlanetfallChineseCulture" },
                                new StringChance() { Edge = 0.35f, String = "malePlanetfallJapaneseCulture" },  
                                new StringChance() { Edge = 0.45f, String = "malePlanetfallHispanicCulture" }, 
                                new StringChance() { Edge = 0.5f, String = "malePlanetfallAfricanCulture" }, 

                                new StringChance() { Edge = 0.75f, String = "femalePlanetfallWhiteAngloCulture" },                                
                                new StringChance() { Edge = 0.89f, String = "femalePlanetfallRussianCulture" }, 
                                new StringChance() { Edge = 1f, String = "femalePlanetfallAfricanCulture" },                         
                            }
                        #endregion
                    },
                    NeedLevels = startNeeds

                }

            });
            #endregion

            #region spawnImmigrantAdvancedTier 
            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnImmigrantAdvancedTier",
                DelayInSeconds = delayForItemsAndAgents,

                // DynamicLocation = new DynamicLocation()   PropertyKey = "startingLocation"  },
                EntityData = new Maps.MapEditor.EntityData()
                {
                    //  Location = new Microsoft.Xna.Framework.Vector3(170f, -30f, 0),
                    EntityKey = "entity:human",
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "dukesLandingAllegiance",
                        ExpeditionKey = "dukesLandingPlanetFallExpedition"
                    },
                    Person = new Maps.MapEditor.Person()
                    {
                        PersonalityType = "advancedSecurityPersonality", //was basicTierPersonality

                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {
                        #region TraitTemplates.  100% random
                        TraitTemplates = new StringChance[] { 
                                new StringChance() { Edge = 0.091f, String = "electronicsSpecialist" },
                                new StringChance() { Edge = 0.182f, String = "mechanicsSpecialist" },
                                new StringChance() { Edge = 0.273f, String = "chemistrySpecialist" },  
                                new StringChance() { Edge = 0.364f, String = "smithingSpecialist" },                           
                                new StringChance() { Edge = 0.455f, String = "farmingSpecialist" }, 
                                new StringChance() { Edge = 0.545f, String = "constructionSpecialist" },                             
                                new StringChance() { Edge = 0.636f, String = "huntingSpecialist" }, 
                                new StringChance() { Edge = 0.727f, String = "menialSpecialist" },                           
                                new StringChance() { Edge = 0.818f, String = "cookingSpecialist" }, 
                                new StringChance() { Edge = 0.909f, String = "bushcraftSpecialist" }, 
                                new StringChance() { Edge = 1f,     String = "securitySpecialist" }, 
                           
                            },
                        #endregion

                        #region CultureTemplates. Future, 100% equality
                        CultureTemplates = new StringChance[] {
                                //equal distribution:
                                new StringChance() { Edge = 0.2f, String = "malePlanetfallWhiteAngloCulture" },
                                new StringChance() { Edge = 0.25f, String = "malePlanetfallWhiteRussianCulture" }, 
                                new StringChance() { Edge = 0.3f, String = "malePlanetfallChineseCulture" },
                                new StringChance() { Edge = 0.35f, String = "malePlanetfallJapaneseCulture" },  
                                new StringChance() { Edge = 0.45f, String = "malePlanetfallHispanicCulture" }, 
                                new StringChance() { Edge = 0.5f, String = "malePlanetfallAfricanCulture" }, 

                                new StringChance() { Edge = 0.75f, String = "femalePlanetfallWhiteAngloCulture" },                                
                                new StringChance() { Edge = 0.89f, String = "femalePlanetfallRussianCulture" }, 
                                new StringChance() { Edge = 1f, String = "femalePlanetfallAfricanCulture" },                         
                            }
                        #endregion
                    },
                    NeedLevels = startNeeds

                }

            });
            #endregion

            #region spawnImmigrantRandom
            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnImmigrantRandom",
                DelayInSeconds = delayForItemsAndAgents,

                // DynamicLocation = new DynamicLocation()   PropertyKey = "startingLocation"  },
                EntityData = new Maps.MapEditor.EntityData()
                {
                    //  Location = new Microsoft.Xna.Framework.Vector3(170f, -30f, 0),
                    EntityKey = "entity:human",
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "dukesLandingAllegiance",
                        ExpeditionKey = "dukesLandingPlanetFallExpedition"
                    },
                    Person = new Maps.MapEditor.Person()
                    {
                        PersonalityType = "randomTierPersonality",

                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {
                        #region TraitTemplates.  100% random
                        TraitTemplates = new StringChance[] { 
                                new StringChance() { Edge = 0.091f, String = "electronicsSpecialist" },
                                new StringChance() { Edge = 0.182f, String = "mechanicsSpecialist" },
                                new StringChance() { Edge = 0.273f, String = "chemistrySpecialist" },  
                                new StringChance() { Edge = 0.364f, String = "smithingSpecialist" },                           
                                new StringChance() { Edge = 0.455f, String = "farmingSpecialist" }, 
                                new StringChance() { Edge = 0.545f, String = "constructionSpecialist" },                             
                                new StringChance() { Edge = 0.636f, String = "huntingSpecialist" }, 
                                new StringChance() { Edge = 0.727f, String = "menialSpecialist" },                           
                                new StringChance() { Edge = 0.818f, String = "cookingSpecialist" }, 
                                new StringChance() { Edge = 0.909f, String = "bushcraftSpecialist" }, 
                                new StringChance() { Edge = 1f,     String = "securitySpecialist" }, 
                           
                            },
                        #endregion
                        #region CultureTemplates. Future, 100% equality
                        CultureTemplates = new StringChance[] {
                                //equal distribution:
                                new StringChance() { Edge = 0.2f, String = "malePlanetfallWhiteAngloCulture" },
                                new StringChance() { Edge = 0.25f, String = "malePlanetfallWhiteRussianCulture" }, 
                                new StringChance() { Edge = 0.3f, String = "malePlanetfallChineseCulture" },
                                new StringChance() { Edge = 0.35f, String = "malePlanetfallJapaneseCulture" },  
                                new StringChance() { Edge = 0.45f, String = "malePlanetfallHispanicCulture" }, 
                                new StringChance() { Edge = 0.5f, String = "malePlanetfallAfricanCulture" }, 

                                new StringChance() { Edge = 0.75f, String = "femalePlanetfallWhiteAngloCulture" },                                
                                new StringChance() { Edge = 0.89f, String = "femalePlanetfallRussianCulture" }, 
                                new StringChance() { Edge = 1f, String = "femalePlanetfallAfricanCulture" },                         
                            }
                        #endregion
                    },
                    NeedLevels = startNeeds

                }

            });
            #endregion

            #endregion
            #endregion

            #region OtherSite2 Possible Immigrants

            #region spawnImmigrantAdvancedTier2
            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnImmigrantAdvancedTier2",
                DelayInSeconds = delayForItemsAndAgents,

                EntityData = new Maps.MapEditor.EntityData()
                {                   
                    EntityKey = "entity:human",
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "othersite2Allegiance", // "advancedAllegiance", //"othersite2Allegiance",
                        ExpeditionKey = "othersite2Expedition"
                    },
                    Person = new Maps.MapEditor.Person()
                    {
                        PersonalityType = "advancedSecurityPersonality", 

                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {
                        #region TraitTemplates.  100% random
                        TraitTemplates = new StringChance[] { 
                                new StringChance() { Edge = 0.091f, String = "electronicsSpecialist" },
                                new StringChance() { Edge = 0.182f, String = "mechanicsSpecialist" },
                                new StringChance() { Edge = 0.273f, String = "chemistrySpecialist" },  
                                new StringChance() { Edge = 0.364f, String = "smithingSpecialist" },                           
                                new StringChance() { Edge = 0.455f, String = "farmingSpecialist" }, 
                                new StringChance() { Edge = 0.545f, String = "constructionSpecialist" },                             
                                new StringChance() { Edge = 0.636f, String = "huntingSpecialist" }, 
                                new StringChance() { Edge = 0.727f, String = "menialSpecialist" },                           
                                new StringChance() { Edge = 0.818f, String = "cookingSpecialist" }, 
                                new StringChance() { Edge = 0.909f, String = "bushcraftSpecialist" }, 
                                new StringChance() { Edge = 1f,     String = "securitySpecialist" }, 
                           
                            },
                        #endregion

                        #region CultureTemplates. Future, 100% equality
                        CultureTemplates = new StringChance[] {
                                //equal distribution:
                                new StringChance() { Edge = 0.2f, String = "malePlanetfallWhiteAngloCulture" },
                                new StringChance() { Edge = 0.25f, String = "malePlanetfallWhiteRussianCulture" }, 
                                new StringChance() { Edge = 0.3f, String = "malePlanetfallChineseCulture" },
                                new StringChance() { Edge = 0.35f, String = "malePlanetfallJapaneseCulture" },  
                                new StringChance() { Edge = 0.45f, String = "malePlanetfallHispanicCulture" }, 
                                new StringChance() { Edge = 0.5f, String = "malePlanetfallAfricanCulture" }, 

                                new StringChance() { Edge = 0.75f, String = "femalePlanetfallWhiteAngloCulture" },                                
                                new StringChance() { Edge = 0.89f, String = "femalePlanetfallRussianCulture" }, 
                                new StringChance() { Edge = 1f, String = "femalePlanetfallAfricanCulture" },                         
                            }
                        #endregion
                    },
                    NeedLevels = startNeeds

                }

            });
            #endregion

            #region spawnImmigrantMediumTier2 
            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnImmigrantMediumTier2",
                DelayInSeconds = delayForItemsAndAgents,

                EntityData = new Maps.MapEditor.EntityData()
                {                 
                    EntityKey = "entity:human",
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "othersite2Allegiance", // "advancedAllegiance", // "othersite2Allegiance",
                        ExpeditionKey = "othersite2Expedition"
                    },
                    Person = new Maps.MapEditor.Person()
                    {
                        PersonalityType = "mediumTierPersonality", //Was survivalTierPersonality

                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {
                        #region TraitTemplates.  100% random
                        TraitTemplates = new StringChance[] { 
                                new StringChance() { Edge = 0.091f, String = "electronicsSpecialist" },
                                new StringChance() { Edge = 0.182f, String = "mechanicsSpecialist" },
                                new StringChance() { Edge = 0.273f, String = "chemistrySpecialist" },  
                                new StringChance() { Edge = 0.364f, String = "smithingSpecialist" },                           
                                new StringChance() { Edge = 0.455f, String = "farmingSpecialist" }, 
                                new StringChance() { Edge = 0.545f, String = "constructionSpecialist" },                             
                                new StringChance() { Edge = 0.636f, String = "huntingSpecialist" }, 
                                new StringChance() { Edge = 0.727f, String = "menialSpecialist" },                           
                                new StringChance() { Edge = 0.818f, String = "cookingSpecialist" }, 
                                new StringChance() { Edge = 0.909f, String = "bushcraftSpecialist" }, 
                                new StringChance() { Edge = 1f,     String = "securitySpecialist" }, 
                           
                            },
                        #endregion

                        #region CultureTemplates. Future, 100% equality
                        CultureTemplates = new StringChance[] {
                                //equal distribution:
                                new StringChance() { Edge = 0.2f, String = "malePlanetfallWhiteAngloCulture" },
                                new StringChance() { Edge = 0.25f, String = "malePlanetfallWhiteRussianCulture" }, 
                                new StringChance() { Edge = 0.3f, String = "malePlanetfallChineseCulture" },
                                new StringChance() { Edge = 0.35f, String = "malePlanetfallJapaneseCulture" },  
                                new StringChance() { Edge = 0.45f, String = "malePlanetfallHispanicCulture" }, 
                                new StringChance() { Edge = 0.5f, String = "malePlanetfallAfricanCulture" }, 

                                new StringChance() { Edge = 0.75f, String = "femalePlanetfallWhiteAngloCulture" },                                
                                new StringChance() { Edge = 0.89f, String = "femalePlanetfallRussianCulture" }, 
                                new StringChance() { Edge = 1f, String = "femalePlanetfallAfricanCulture" },                         
                            }
                        #endregion
                    },
                    NeedLevels = startNeeds

                }

            });
            #endregion

            #endregion

            

            //GAME AREA:
            #region Characters

            #region spawnNadova //mentioned in story.  constructionSpecialist
            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnNadova",
                DelayInSeconds = delayForItemsAndAgents, // 
                DynamicLocation = new DynamicLocation()
                {
                    PropertyKey = "startingLocation"
                },
                EntityData = new Maps.MapEditor.EntityData()
                {
                    Location = new Microsoft.Xna.Framework.Vector3(-60f, 100f, 0), //
                    EntityKey = "entity:human",
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "playerAllegiance",
                        ExpeditionKey = "Camp"
                    },

                    Person = new Maps.MapEditor.Person()
                    {
                        FirstName = "Irina", //Keep this. She's mentioned in story
                        LastName = "Nadova",
                        PersonalityType = "advancedSecurityPersonality",
                        Portrait = "human_w_f_adult_2",
                        SimulateJoinedExpeditionNow = true,
                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {
                        AgeInYears = new NormalDistribution() { Mean = 44f },
                        CasteKey = "female",
                        ModelTextureName = "ManGreyClothes1Texture",
                        RaceKey = "whiteHumanDescendant",
                        TraitTemplates = new StringChance[] { 
                                new StringChance() {  String = "constructionSpecialist" }, 
                                },

                    },
                    EffectProfiles = new[] { "leader" }, // won't emigrate or complain at group meetings
                    NeedLevels = startNeeds
                }

            });
            #endregion

            #region spawnMike //mentioned in story.  securitySpecialist
            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnMike",
                DelayInSeconds = delayForItemsAndAgents, // 
                DynamicLocation = new DynamicLocation()
                {
                    PropertyKey = "startingLocation"
                },
                EntityData = new Maps.MapEditor.EntityData()
                {
                    Location = new Microsoft.Xna.Framework.Vector3(-75f, -8f, 0), //
                    EntityKey = "entity:human",
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "playerAllegiance",
                        ExpeditionKey = "Camp"
                    },

                    Person = new Maps.MapEditor.Person()
                    {
                        FirstName = "Mike", //Keep this. he's mentioned in story
                        LastName = "Lewis",
                        PersonalityType = "advancedSecurityPersonality",
                        Portrait = "human_w_m_adult_1",
                        SimulateJoinedExpeditionNow = true,
                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {
                        AgeInYears = new NormalDistribution() { Mean = 28f },
                        CasteKey = "male",
                        ModelTextureName = "ManGreyClothes1Texture",
                        RaceKey = "whiteHumanDescendant",
                        TraitTemplates = new StringChance[] { 
                                new StringChance() {  String = "securitySpecialist" }, 
                                }
                    },                  
                    NeedLevels = startNeeds
                }

            });
            #endregion
            ////////////
            #region spawn coords
            /*Spawn coords:
            //-38f, 48f  "spawnPerson1"

            -45f, 41f

            //-48f, 38f, "spawnPerson5"

            -55f, 30f
            -61f, 22f
            -68f, 15f
            -72f, 10f
            -73f,5f
            -51f, 0f

            //-75f, -8f, "spawnPerson4"

            -79f, 6f
            -81f, 12f
            -84f,24f
            -85f, 31f
            -86f, 40f
            -87f, 48f

            //-88f, 56f, "spawnPerson7"

            -90f, 45f,
            -92f, 34f,
            -93f, 21f,
            -94f, 15f,
            -95f, 8f
            //-96f, 0f, "spawnPerson3"

            -96f, 23f,
            -96, 47f,
             * below not used:
            -96f, 61f,
            -96, 78f,



            //-96f, 128f "spawnPerson6"


            /////////////////////////////////////////
            -146f, 186f "spawnPerson2" Castor Hernes stands at coast alone, talking to the group. as per the story.
             */
            #endregion

            // specialist1 is survivaltier personality. specialist2 is basictier personality
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
                    Location = new Microsoft.Xna.Framework.Vector3(-48f, 38f, 0),
                    EntityKey = "entity:human",
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "playerAllegiance",
                        ExpeditionKey = "Camp"
                    },
                   
                    Person = new Maps.MapEditor.Person()
                    {
                        PersonalityType = "advancedSecurityPersonality",
                        SimulateJoinedExpeditionNow = true,
                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {

                        TraitTemplates = new StringChance[] { 
                                new StringChance() {  String = "bushcraftSpecialist" },},
                        #region CultureTemplates. Future, 100% equality
                        CultureTemplates = new StringChance[] {
                                //equal distribution:
                                new StringChance() { Edge = 0.2f, String = "malePlanetfallWhiteAngloCulture" },
                                new StringChance() { Edge = 0.25f, String = "malePlanetfallWhiteRussianCulture" }, 
                                new StringChance() { Edge = 0.3f, String = "malePlanetfallChineseCulture" },
                                new StringChance() { Edge = 0.35f, String = "malePlanetfallJapaneseCulture" },  
                                new StringChance() { Edge = 0.45f, String = "malePlanetfallHispanicCulture" }, 
                                new StringChance() { Edge = 0.5f, String = "malePlanetfallAfricanCulture" }, 

                                new StringChance() { Edge = 0.75f, String = "femalePlanetfallWhiteAngloCulture" },                                
                                new StringChance() { Edge = 0.89f, String = "femalePlanetfallRussianCulture" }, 
                                new StringChance() { Edge = 1f, String = "femalePlanetfallAfricanCulture" },                          
                            }
                        #endregion
                    },
                    NeedLevels = startNeeds
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
                    Location = new Microsoft.Xna.Framework.Vector3(-38f, 48f, 0), //
                    EntityKey = "entity:human",
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "playerAllegiance",
                        ExpeditionKey = "Camp"
                    },
                  
                    Person = new Maps.MapEditor.Person()
                    {
                        PersonalityType = "advancedSecurityPersonality",
                        SimulateJoinedExpeditionNow = true,
                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {
                        TraitTemplates = new StringChance[] { 
                                new StringChance() {  String = "bushcraftSpecialist" },},
                        #region CultureTemplates. Future, 100% equality
                        CultureTemplates = new StringChance[] {
                                //equal distribution:
                                new StringChance() { Edge = 0.2f, String = "malePlanetfallWhiteAngloCulture" },
                                new StringChance() { Edge = 0.25f, String = "malePlanetfallWhiteRussianCulture" }, 
                                new StringChance() { Edge = 0.3f, String = "malePlanetfallChineseCulture" },
                                new StringChance() { Edge = 0.35f, String = "malePlanetfallJapaneseCulture" },  
                                new StringChance() { Edge = 0.45f, String = "malePlanetfallHispanicCulture" }, 
                                new StringChance() { Edge = 0.5f, String = "malePlanetfallAfricanCulture" }, 

                                new StringChance() { Edge = 0.75f, String = "femalePlanetfallWhiteAngloCulture" },                                
                                new StringChance() { Edge = 0.89f, String = "femalePlanetfallRussianCulture" }, 
                                new StringChance() { Edge = 1f, String = "femalePlanetfallAfricanCulture" },                          
                            }

                        #endregion

                    },
                    NeedLevels = startNeeds
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
                    Location = new Microsoft.Xna.Framework.Vector3(-85f, -18f, 0),
                    EntityKey = "entity:human",
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "playerAllegiance",
                        ExpeditionKey = "Camp"
                    },
                  
                    Person = new Maps.MapEditor.Person()
                    {
                        PersonalityType = "advancedSecurityPersonality",
                        SimulateJoinedExpeditionNow = true,
                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {

                        TraitTemplates = new StringChance[] { 
                                new StringChance() {  String = "securitySpecialist" },},

                        #region CultureTemplates. Future, 100% equality
                        CultureTemplates = new StringChance[] {
                                //equal distribution:
                                new StringChance() { Edge = 0.2f, String = "malePlanetfallWhiteAngloCulture" },
                                new StringChance() { Edge = 0.25f, String = "malePlanetfallWhiteRussianCulture" }, 
                                new StringChance() { Edge = 0.3f, String = "malePlanetfallChineseCulture" },
                                new StringChance() { Edge = 0.35f, String = "malePlanetfallJapaneseCulture" },  
                                new StringChance() { Edge = 0.45f, String = "malePlanetfallHispanicCulture" }, 
                                new StringChance() { Edge = 0.5f, String = "malePlanetfallAfricanCulture" }, 

                                new StringChance() { Edge = 0.75f, String = "femalePlanetfallWhiteAngloCulture" },                                
                                new StringChance() { Edge = 0.89f, String = "femalePlanetfallRussianCulture" }, 
                                new StringChance() { Edge = 1f, String = "femalePlanetfallAfricanCulture" },                           
                            }
                        #endregion
                    },
                    NeedLevels = startNeeds
                }

            });
            #endregion
            #region spawnSecuritySpecialist2
            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnSecuritySpecialist2",
                DelayInSeconds = delayForItemsAndAgents,

                DynamicLocation = new DynamicLocation()
                {
                    PropertyKey = "startingLocation"
                },
                EntityData = new Maps.MapEditor.EntityData()
                {
                    Location = new Microsoft.Xna.Framework.Vector3(-70f, -18f, 0),
                    EntityKey = "entity:human",
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "playerAllegiance",
                        ExpeditionKey = "Camp"
                    },
                 
                    Person = new Maps.MapEditor.Person()
                    {
                        PersonalityType = "basicTierPersonality",
                        SimulateJoinedExpeditionNow = true,
                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {

                        TraitTemplates = new StringChance[] { 
                                new StringChance() {  String = "securitySpecialist" },},

                        #region CultureTemplates. Future, 100% equality
                        CultureTemplates = new StringChance[] {
                                //equal distribution:
                                new StringChance() { Edge = 0.2f, String = "malePlanetfallWhiteAngloCulture" },
                                new StringChance() { Edge = 0.25f, String = "malePlanetfallWhiteRussianCulture" }, 
                                new StringChance() { Edge = 0.3f, String = "malePlanetfallChineseCulture" },
                                new StringChance() { Edge = 0.35f, String = "malePlanetfallJapaneseCulture" },  
                                new StringChance() { Edge = 0.45f, String = "malePlanetfallHispanicCulture" }, 
                                new StringChance() { Edge = 0.5f, String = "malePlanetfallAfricanCulture" }, 

                                new StringChance() { Edge = 0.75f, String = "femalePlanetfallWhiteAngloCulture" },                                
                                new StringChance() { Edge = 0.89f, String = "femalePlanetfallRussianCulture" }, 
                                new StringChance() { Edge = 1f, String = "femalePlanetfallAfricanCulture" },                          
                            }
                        #endregion
                    },
                    NeedLevels = startNeeds
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
                    Location = new Microsoft.Xna.Framework.Vector3(-96f, 128f, 0),
                    EntityKey = "entity:human",
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "playerAllegiance",
                        ExpeditionKey = "Camp"
                    },
                  
                    Person = new Maps.MapEditor.Person()
                    {
                        PersonalityType = "advancedSecurityPersonality",
                        SimulateJoinedExpeditionNow = true,
                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {

                        TraitTemplates = new StringChance[] { 
                                new StringChance() {  String = "cookingSpecialist" },},
                        #region CultureTemplates. Future, 100% equality
                        CultureTemplates = new StringChance[] {
                                //equal distribution:
                                new StringChance() { Edge = 0.2f, String = "malePlanetfallWhiteAngloCulture" },
                                new StringChance() { Edge = 0.25f, String = "malePlanetfallWhiteRussianCulture" }, 
                                new StringChance() { Edge = 0.3f, String = "malePlanetfallChineseCulture" },
                                new StringChance() { Edge = 0.35f, String = "malePlanetfallJapaneseCulture" },  
                                new StringChance() { Edge = 0.45f, String = "malePlanetfallHispanicCulture" }, 
                                new StringChance() { Edge = 0.5f, String = "malePlanetfallAfricanCulture" }, 

                                new StringChance() { Edge = 0.75f, String = "femalePlanetfallWhiteAngloCulture" },                                
                                new StringChance() { Edge = 0.89f, String = "femalePlanetfallRussianCulture" }, 
                                new StringChance() { Edge = 1f, String = "femalePlanetfallAfricanCulture" },                           
                            }
                        #endregion
                    },
                    NeedLevels = startNeeds
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
                    Location = new Microsoft.Xna.Framework.Vector3(-88f, 56f, 0),
                    EntityKey = "entity:human",
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "playerAllegiance",
                        ExpeditionKey = "Camp"
                    },
                  
                    Person = new Maps.MapEditor.Person()
                    {

                        PersonalityType = "basicTierPersonality",
                        SimulateJoinedExpeditionNow = true,
                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {

                        TraitTemplates = new StringChance[] { 
                                new StringChance() {  String = "cookingSpecialist" },},
                        #region CultureTemplates. Future, 100% equality
                        CultureTemplates = new StringChance[] {
                                //equal distribution:
                                new StringChance() { Edge = 0.2f, String = "malePlanetfallWhiteAngloCulture" },
                                new StringChance() { Edge = 0.25f, String = "malePlanetfallWhiteRussianCulture" }, 
                                new StringChance() { Edge = 0.3f, String = "malePlanetfallChineseCulture" },
                                new StringChance() { Edge = 0.35f, String = "malePlanetfallJapaneseCulture" },  
                                new StringChance() { Edge = 0.45f, String = "malePlanetfallHispanicCulture" }, 
                                new StringChance() { Edge = 0.5f, String = "malePlanetfallAfricanCulture" }, 

                                new StringChance() { Edge = 0.75f, String = "femalePlanetfallWhiteAngloCulture" },                                
                                new StringChance() { Edge = 0.89f, String = "femalePlanetfallRussianCulture" }, 
                                new StringChance() { Edge = 1f, String = "femalePlanetfallAfricanCulture" },                          
                            }
                        #endregion
                    },
                    NeedLevels = startNeeds
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
                    Location = new Microsoft.Xna.Framework.Vector3(-39f, 99f, 0),
                    EntityKey = "entity:human",
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "playerAllegiance",
                        ExpeditionKey = "Camp"
                    },
                   
                    Person = new Maps.MapEditor.Person()
                    {
                        PersonalityType = "advancedSecurityPersonality",
                        SimulateJoinedExpeditionNow = true,
                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {

                        TraitTemplates = new StringChance[] { 
                                new StringChance() {  String = "huntingSpecialist" },},
                        #region CultureTemplates. Future, 100% equality
                        CultureTemplates = new StringChance[] {
                                //equal distribution:
                                new StringChance() { Edge = 0.2f, String = "malePlanetfallWhiteAngloCulture" },
                                new StringChance() { Edge = 0.25f, String = "malePlanetfallWhiteRussianCulture" }, 
                                new StringChance() { Edge = 0.3f, String = "malePlanetfallChineseCulture" },
                                new StringChance() { Edge = 0.35f, String = "malePlanetfallJapaneseCulture" },  
                                new StringChance() { Edge = 0.45f, String = "malePlanetfallHispanicCulture" }, 
                                new StringChance() { Edge = 0.5f, String = "malePlanetfallAfricanCulture" }, 

                                new StringChance() { Edge = 0.75f, String = "femalePlanetfallWhiteAngloCulture" },                                
                                new StringChance() { Edge = 0.89f, String = "femalePlanetfallRussianCulture" }, 
                                new StringChance() { Edge = 1f, String = "femalePlanetfallAfricanCulture" },                           
                            }
                        #endregion

                    },
                    NeedLevels = startNeeds
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
                    Location = new Microsoft.Xna.Framework.Vector3(-118f, -6f, 0),
                    EntityKey = "entity:human",
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "playerAllegiance",
                        ExpeditionKey = "Camp"
                    },
                  
                    Person = new Maps.MapEditor.Person()
                    {
                        PersonalityType = "basicTierPersonality",
                        SimulateJoinedExpeditionNow = true,
                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {
                        TraitTemplates = new StringChance[] { 
                                new StringChance() {  String = "huntingSpecialist" },},
                        #region CultureTemplates. Future, 100% equality
                        CultureTemplates = new StringChance[] {
                                //equal distribution:
                                new StringChance() { Edge = 0.2f, String = "malePlanetfallWhiteAngloCulture" },
                                new StringChance() { Edge = 0.25f, String = "malePlanetfallWhiteRussianCulture" }, 
                                new StringChance() { Edge = 0.3f, String = "malePlanetfallChineseCulture" },
                                new StringChance() { Edge = 0.35f, String = "malePlanetfallJapaneseCulture" },  
                                new StringChance() { Edge = 0.45f, String = "malePlanetfallHispanicCulture" }, 
                                new StringChance() { Edge = 0.5f, String = "malePlanetfallAfricanCulture" }, 

                                new StringChance() { Edge = 0.75f, String = "femalePlanetfallWhiteAngloCulture" },                                
                                new StringChance() { Edge = 0.89f, String = "femalePlanetfallRussianCulture" }, 
                                new StringChance() { Edge = 1f, String = "femalePlanetfallAfricanCulture" },                          
                            }
                        #endregion
                    },
                    NeedLevels = startNeeds
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
                        PersonalityType = "advancedSecurityPersonality",
                        SimulateJoinedExpeditionNow = true,
                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {

                        TraitTemplates = new StringChance[] { 
                                new StringChance() {  String = "menialSpecialist" },},
                        #region CultureTemplates. Future, 100% equality
                        CultureTemplates = new StringChance[] {
                                //equal distribution:
                                new StringChance() { Edge = 0.2f, String = "malePlanetfallWhiteAngloCulture" },
                                new StringChance() { Edge = 0.25f, String = "malePlanetfallWhiteRussianCulture" }, 
                                new StringChance() { Edge = 0.3f, String = "malePlanetfallChineseCulture" },
                                new StringChance() { Edge = 0.35f, String = "malePlanetfallJapaneseCulture" },  
                                new StringChance() { Edge = 0.45f, String = "malePlanetfallHispanicCulture" }, 
                                new StringChance() { Edge = 0.5f, String = "malePlanetfallAfricanCulture" }, 

                                new StringChance() { Edge = 0.75f, String = "femalePlanetfallWhiteAngloCulture" },                                
                                new StringChance() { Edge = 0.89f, String = "femalePlanetfallRussianCulture" }, 
                                new StringChance() { Edge = 1f, String = "femalePlanetfallAfricanCulture" },                           
                            }
                        #endregion
                    },
                    NeedLevels = startNeeds
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
                        PersonalityType = "basicTierPersonality",
                        SimulateJoinedExpeditionNow = true,
                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {

                        TraitTemplates = new StringChance[] { 
                                new StringChance() {  String = "menialSpecialist" },},
                        #region CultureTemplates. Future, 100% equality
                        CultureTemplates = new StringChance[] {
                                //equal distribution:
                                new StringChance() { Edge = 0.2f, String = "malePlanetfallWhiteAngloCulture" },
                                new StringChance() { Edge = 0.25f, String = "malePlanetfallWhiteRussianCulture" }, 
                                new StringChance() { Edge = 0.3f, String = "malePlanetfallChineseCulture" },
                                new StringChance() { Edge = 0.35f, String = "malePlanetfallJapaneseCulture" },  
                                new StringChance() { Edge = 0.45f, String = "malePlanetfallHispanicCulture" }, 
                                new StringChance() { Edge = 0.5f, String = "malePlanetfallAfricanCulture" }, 

                                new StringChance() { Edge = 0.75f, String = "femalePlanetfallWhiteAngloCulture" },                                
                                new StringChance() { Edge = 0.89f, String = "femalePlanetfallRussianCulture" }, 
                                new StringChance() { Edge = 1f, String = "femalePlanetfallAfricanCulture" },                          
                            }
                        #endregion
                    },
                    NeedLevels = startNeeds
                }

            });
            #endregion
            #region spawnConstructionSpecialist1
            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnConstructionSpecialist1",
                DelayInSeconds = delayForItemsAndAgents,

                DynamicLocation = new DynamicLocation()
                {
                    PropertyKey = "startingLocation"
                },
                EntityData = new Maps.MapEditor.EntityData()
                {
                    Location = new Microsoft.Xna.Framework.Vector3(-65f, 109f, 0),
                    EntityKey = "entity:human",
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "playerAllegiance",
                        ExpeditionKey = "Camp"
                    },
                  
                    Person = new Maps.MapEditor.Person()
                    {
                        PersonalityType = "advancedSecurityPersonality",
                        SimulateJoinedExpeditionNow = true,
                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {
                        TraitTemplates = new StringChance[] { 
                                new StringChance() {  String = "constructionSpecialist" },},
                        #region CultureTemplates. Future, 100% equality
                        CultureTemplates = new StringChance[] {
                                //equal distribution:
                                new StringChance() { Edge = 0.2f, String = "malePlanetfallWhiteAngloCulture" },
                                new StringChance() { Edge = 0.25f, String = "malePlanetfallWhiteRussianCulture" }, 
                                new StringChance() { Edge = 0.3f, String = "malePlanetfallChineseCulture" },
                                new StringChance() { Edge = 0.35f, String = "malePlanetfallJapaneseCulture" },  
                                new StringChance() { Edge = 0.45f, String = "malePlanetfallHispanicCulture" }, 
                                new StringChance() { Edge = 0.5f, String = "malePlanetfallAfricanCulture" }, 

                                new StringChance() { Edge = 0.75f, String = "femalePlanetfallWhiteAngloCulture" },                                
                                new StringChance() { Edge = 0.89f, String = "femalePlanetfallRussianCulture" }, 
                                new StringChance() { Edge = 1f, String = "femalePlanetfallAfricanCulture" },                           
                            }
                        #endregion
                    },
                    NeedLevels = startNeeds
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

                        PersonalityType = "basicTierPersonality",
                        SimulateJoinedExpeditionNow = true,
                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {
                        TraitTemplates = new StringChance[] { 
                                new StringChance() {  String = "constructionSpecialist" },},
                        #region CultureTemplates. Future, 100% equality
                        CultureTemplates = new StringChance[] {
                                //equal distribution:
                                new StringChance() { Edge = 0.2f, String = "malePlanetfallWhiteAngloCulture" },
                                new StringChance() { Edge = 0.25f, String = "malePlanetfallWhiteRussianCulture" }, 
                                new StringChance() { Edge = 0.3f, String = "malePlanetfallChineseCulture" },
                                new StringChance() { Edge = 0.35f, String = "malePlanetfallJapaneseCulture" },  
                                new StringChance() { Edge = 0.45f, String = "malePlanetfallHispanicCulture" }, 
                                new StringChance() { Edge = 0.5f, String = "malePlanetfallAfricanCulture" }, 

                                new StringChance() { Edge = 0.75f, String = "femalePlanetfallWhiteAngloCulture" },                                
                                new StringChance() { Edge = 0.89f, String = "femalePlanetfallRussianCulture" }, 
                                new StringChance() { Edge = 1f, String = "femalePlanetfallAfricanCulture" },                          
                            }
                        #endregion
                    },
                    NeedLevels = startNeeds
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
                        PersonalityType = "advancedSecurityPersonality",
                        SimulateJoinedExpeditionNow = true,
                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {

                        TraitTemplates = new StringChance[] { 
                                new StringChance() {  String = "smithingSpecialist" },},
                        #region CultureTemplates. Future, 100% equality
                        CultureTemplates = new StringChance[] {
                                //equal distribution:
                                new StringChance() { Edge = 0.2f, String = "malePlanetfallWhiteAngloCulture" },
                                new StringChance() { Edge = 0.25f, String = "malePlanetfallWhiteRussianCulture" }, 
                                new StringChance() { Edge = 0.3f, String = "malePlanetfallChineseCulture" },
                                new StringChance() { Edge = 0.35f, String = "malePlanetfallJapaneseCulture" },  
                                new StringChance() { Edge = 0.45f, String = "malePlanetfallHispanicCulture" }, 
                                new StringChance() { Edge = 0.5f, String = "malePlanetfallAfricanCulture" }, 

                                new StringChance() { Edge = 0.75f, String = "femalePlanetfallWhiteAngloCulture" },                                
                                new StringChance() { Edge = 0.89f, String = "femalePlanetfallRussianCulture" }, 
                                new StringChance() { Edge = 1f, String = "femalePlanetfallAfricanCulture" },                          
                            }
                        #endregion
                    },
                    NeedLevels = startNeeds
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

                        PersonalityType = "basicTierPersonality",
                        SimulateJoinedExpeditionNow = true,
                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {
                        TraitTemplates = new StringChance[] { 
                                new StringChance() {  String = "smithingSpecialist" },},
                        #region CultureTemplates. Future, 100% equality
                        CultureTemplates = new StringChance[] {
                                //equal distribution:
                                new StringChance() { Edge = 0.2f, String = "malePlanetfallWhiteAngloCulture" },
                                new StringChance() { Edge = 0.25f, String = "malePlanetfallWhiteRussianCulture" }, 
                                new StringChance() { Edge = 0.3f, String = "malePlanetfallChineseCulture" },
                                new StringChance() { Edge = 0.35f, String = "malePlanetfallJapaneseCulture" },  
                                new StringChance() { Edge = 0.45f, String = "malePlanetfallHispanicCulture" }, 
                                new StringChance() { Edge = 0.5f, String = "malePlanetfallAfricanCulture" }, 

                                new StringChance() { Edge = 0.75f, String = "femalePlanetfallWhiteAngloCulture" },                                
                                new StringChance() { Edge = 0.89f, String = "femalePlanetfallRussianCulture" }, 
                                new StringChance() { Edge = 1f, String = "femalePlanetfallAfricanCulture" },                         
                            }
                        #endregion
                    },
                    NeedLevels = startNeeds
                }

            });
            #endregion
            #region spawnFarmingSpecialist1
            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnFarmingSpecialist1",
                DelayInSeconds = delayForItemsAndAgents,

                DynamicLocation = new DynamicLocation()
                {
                    PropertyKey = "startingLocation"
                },
                EntityData = new Maps.MapEditor.EntityData()
                {
                    Location = new Microsoft.Xna.Framework.Vector3(-96f, 61f, 0),
                    EntityKey = "entity:human",
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "playerAllegiance",
                        ExpeditionKey = "Camp"
                    },
                   
                    Person = new Maps.MapEditor.Person()
                    {
                        PersonalityType = "advancedSecurityPersonality",
                        SimulateJoinedExpeditionNow = true,
                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {
                        TraitTemplates = new StringChance[] { 
                                new StringChance() {  String = "farmingSpecialist" },},
                        #region CultureTemplates. Future, 100% equality
                        CultureTemplates = new StringChance[] {
                                //equal distribution:
                                new StringChance() { Edge = 0.2f, String = "malePlanetfallWhiteAngloCulture" },
                                new StringChance() { Edge = 0.25f, String = "malePlanetfallWhiteRussianCulture" }, 
                                new StringChance() { Edge = 0.3f, String = "malePlanetfallChineseCulture" },
                                new StringChance() { Edge = 0.35f, String = "malePlanetfallJapaneseCulture" },  
                                new StringChance() { Edge = 0.45f, String = "malePlanetfallHispanicCulture" }, 
                                new StringChance() { Edge = 0.5f, String = "malePlanetfallAfricanCulture" }, 

                                new StringChance() { Edge = 0.75f, String = "femalePlanetfallWhiteAngloCulture" },                                
                                new StringChance() { Edge = 0.89f, String = "femalePlanetfallRussianCulture" }, 
                                new StringChance() { Edge = 1f, String = "femalePlanetfallAfricanCulture" },                           
                            }
                        #endregion
                    },
                    NeedLevels = startNeeds
                }

            });
            #endregion
            #region spawnFarmingSpecialist2
            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnFarmingSpecialist2",
                DelayInSeconds = delayForItemsAndAgents,

                DynamicLocation = new DynamicLocation()
                {
                    PropertyKey = "startingLocation"
                },
                EntityData = new Maps.MapEditor.EntityData()
                {
                    Location = new Microsoft.Xna.Framework.Vector3(-96, 78f, 0),
                    EntityKey = "entity:human",
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "playerAllegiance",
                        ExpeditionKey = "Camp"
                    },
                   
                    Person = new Maps.MapEditor.Person()
                    {
                        PersonalityType = "basicTierPersonality",
                        SimulateJoinedExpeditionNow = true,
                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {
                        TraitTemplates = new StringChance[] { 
                                new StringChance() {  String = "farmingSpecialist" },},
                        #region CultureTemplates. Future, 100% equality
                        CultureTemplates = new StringChance[] {
                                //equal distribution:
                                new StringChance() { Edge = 0.2f, String = "malePlanetfallWhiteAngloCulture" },
                                new StringChance() { Edge = 0.25f, String = "malePlanetfallWhiteRussianCulture" }, 
                                new StringChance() { Edge = 0.3f, String = "malePlanetfallChineseCulture" },
                                new StringChance() { Edge = 0.35f, String = "malePlanetfallJapaneseCulture" },  
                                new StringChance() { Edge = 0.45f, String = "malePlanetfallHispanicCulture" }, 
                                new StringChance() { Edge = 0.5f, String = "malePlanetfallAfricanCulture" }, 

                                new StringChance() { Edge = 0.75f, String = "femalePlanetfallWhiteAngloCulture" },                                
                                new StringChance() { Edge = 0.89f, String = "femalePlanetfallRussianCulture" }, 
                                new StringChance() { Edge = 1f, String = "femalePlanetfallAfricanCulture" },                          
                            }
                        #endregion
                    },
                    NeedLevels = startNeeds
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
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {
                        AgeInYears = new NormalDistribution() { Mean = 4f },
                        CultureTemplates = new StringChance[] { new StringChance() { Edge = 1f, String = "dogCulture" } }
                    },
                    OwnedBy = new AllegianceAndExpedition() { AllegianceKey = "playerAllegiance", ExpeditionKey = "Camp" },
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "playerAllegiance",
                        ExpeditionKey = "Camp"
                    }

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
                    Location = new Microsoft.Xna.Framework.Vector3(-30f, 10f, 0),
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
            #region spawnHaulRobot 2
            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnHaulRobot2",
                DelayInSeconds = delayForItemsAndAgents,

                DynamicLocation = new DynamicLocation()
                {
                    PropertyKey = "startingLocation"
                },
                EntityData = new EntityData()
                {
                    Location = new Microsoft.Xna.Framework.Vector3(-80f, 15f, 0),
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
            #region spawnHaulRobot 3
            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnHaulRobot3",
                DelayInSeconds = delayForItemsAndAgents,

                DynamicLocation = new DynamicLocation()
                {
                    PropertyKey = "startingLocation"
                },
                EntityData = new EntityData()
                {
                    Location = new Microsoft.Xna.Framework.Vector3(50f, 5f, 0),
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
            #region spawnMiningRobot
            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnMiningRobot",
                DelayInSeconds = delayForItemsAndAgents,

                DynamicLocation = new DynamicLocation()
                {
                    PropertyKey = "startingLocation"
                },
                EntityData = new EntityData()
                {
                    Location = new Microsoft.Xna.Framework.Vector3(-40f, -50f, 0),
                    EntityKey = "entity:diggingRobot", //"entity:patrolRobot"
                    OwnedBy = new AllegianceAndExpedition() { AllegianceKey = "playerAllegiance", ExpeditionKey = "Camp" },
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "playerAllegiance",
                        ExpeditionKey = "Camp"
                    }
                }

            });
            #endregion
            #region spawnGuardRobot HOUND?
            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnGuardRobot",
                DelayInSeconds = delayForItemsAndAgents,

                DynamicLocation = new DynamicLocation()
                {
                    PropertyKey = "startingLocation"
                },
                EntityData = new EntityData()
                {
                    Location = new Microsoft.Xna.Framework.Vector3(-40f, 10f, 0),
                    EntityKey = "entity:guardRobot", //"entity:patrolRobot"
                    OwnedBy = new AllegianceAndExpedition() { AllegianceKey = "playerAllegiance", ExpeditionKey = "Camp" },
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "playerAllegiance",
                        ExpeditionKey = "Camp"
                    }
                }

            });
            #endregion

            #region Starting equipment

            // used to get stuff closer to the camp so that less hauling is needed
            float closeDistanceX = -100f;
            float closeDistanceY = 100f;
            
            // will place items to the left and south.
            float toolsDistanceX = 160f;
            float toolsDistanceY = 144f;

            //places food and seed in a pile
            float foodDistanceX = 160f;
            float foodDistanceY = 144f;

            float weaponsDistanceX = 160f;
            float weaponsDistanceY = 144f;

            // variable for testing. used on all the testing things.
            float testDistanceX = -156f;
            float testDistanceY = 0f;

            float xItemPosition = -24f;
            float yItemPosition = 80f;

             // For testing
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startPanelScraps", new Vector2(testDistanceX, testDistanceY), "item:panelScraps", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSpikeTrap", new Vector2(testDistanceX, testDistanceY), "item:spikeTrap", "playerAllegiance", null, delayForItemsAndAgents)); //mp with -136f, 182f it was not accessible onHunting start loc.
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startVarmintBomb", new Vector2(testDistanceX, testDistanceY), "item:varmintBomb", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startNet", new Vector2(testDistanceX, testDistanceY), "item:fishingNet", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startTextile", new Vector2(testDistanceX, testDistanceY), "item:textile", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startPeat", new Vector2(testDistanceX, testDistanceY), "item:dryPeat", "playerAllegiance", null, delayForItemsAndAgents, amount: 20));



            #region food
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startRation", new Vector2(foodDistanceX, foodDistanceY), "item:astroRation", "playerAllegiance", null, delayForItemsAndAgents));
           
            
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startJerky", new Vector2(foodDistanceX, foodDistanceY), "item:driedBeef", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSentryGunAmmo", new Vector2(foodDistanceX, foodDistanceY), "item:sentryGunAmmo", "playerAllegiance", null, delayForItemsAndAgents));
            
            
            
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startBlackpulp", new Vector2(foodDistanceX, foodDistanceY), "item:blackpulp", "playerAllegiance", null, delayForItemsAndAgents));
            #endregion
            #region seeds
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startGlassyCreeper", new Vector2(foodDistanceX, foodDistanceY), "item:glassyCreeperPods", "playerAllegiance", null, delayForItemsAndAgents));
            #endregion
            #region weapons
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startGunpowderRifle", new Vector2(weaponsDistanceX, weaponsDistanceY), "item:gunpowderRifle", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startGunpowderAmmo", new Vector2(weaponsDistanceX, weaponsDistanceY), "item:blackPowderRifleAmmo", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startBlunderbuss", new Vector2(weaponsDistanceX, weaponsDistanceY), "item:musketoon", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startBlackPowderShotAmmo", new Vector2(weaponsDistanceX, weaponsDistanceY), "item:blackPowderShotAmmo", "playerAllegiance", null, delayForItemsAndAgents));

            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startBoltActionRifle", new Vector2(weaponsDistanceX, weaponsDistanceY), "item:boltActionRifle", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startBoltActionAmmo", new Vector2(weaponsDistanceX, weaponsDistanceY), "item:corditeAmmo", "playerAllegiance", null, delayForItemsAndAgents));

            
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startIronSpear", new Vector2(weaponsDistanceX, weaponsDistanceY), "item:ironSpear", "playerAllegiance", null, delayForItemsAndAgents));

            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startImprovisedBow", new Vector2(weaponsDistanceX, weaponsDistanceY), "item:improvisedBow", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startCoilRifle", new Vector2(weaponsDistanceX, weaponsDistanceY), "item:coilRifle", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startCoilRifleAmmo", new Vector2(weaponsDistanceX, weaponsDistanceY), "item:coilRifleAmmo", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startShotgun", new Vector2(weaponsDistanceX, weaponsDistanceY), "item:shotgun", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startShotgunAmmo", new Vector2(weaponsDistanceX, weaponsDistanceY), "item:shotgunAmmo", "playerAllegiance", null, delayForItemsAndAgents));
           
            
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startIronArrow", new Vector2(weaponsDistanceX, weaponsDistanceY), "item:ironArrow", "playerAllegiance", null, delayForItemsAndAgents));
            #endregion
            #region Tools
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startMetalWire", new Vector2(toolsDistanceX, toolsDistanceY), "item:metalWire", "playerAllegiance", null, delayForItemsAndAgents));
           
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startGoldPot", new Vector2(toolsDistanceX, toolsDistanceY), "item:goldPot", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startImprovisedCookingPot", new Vector2(toolsDistanceX, toolsDistanceY), "item:improvisedCookingPot", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startHoe", new Vector2(toolsDistanceX, toolsDistanceY), "item:farmingHoe", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startHammer", new Vector2(toolsDistanceX, toolsDistanceY), "item:hammer", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startBellows", new Vector2(toolsDistanceX, toolsDistanceY), "item:bellows", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSulfurSmokeBomb", new Vector2(toolsDistanceX, toolsDistanceY), "item:bigBomb", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSensor", new Vector2(toolsDistanceX, toolsDistanceY), "item:sensor", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startTurnipCracker", new Vector2(toolsDistanceX, toolsDistanceY), "item:turnipCracker", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startBugNet", new Vector2(toolsDistanceX, toolsDistanceY), "item:strongBugNet", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startIronHooks", new Vector2(toolsDistanceX, toolsDistanceY), "item:ironHooks", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startNeonHornetsLive", new Vector2(toolsDistanceX, toolsDistanceY), "item:neonHornetsLive", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startFishTrapBasket", new Vector2(toolsDistanceX, toolsDistanceY), "item:fishTrapBasket", "playerAllegiance", null, delayForItemsAndAgents)); //carbon tail
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startFishTrapHoopNet", new Vector2(toolsDistanceX, toolsDistanceY), "item:fishTrapHoopNet", "playerAllegiance", null, delayForItemsAndAgents)); //streak fin
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startPigFliesLive", new Vector2(toolsDistanceX, toolsDistanceY), "item:pigFliesLive", "playerAllegiance", null, delayForItemsAndAgents));
            //stuff placed closer to their campsite stockpile so they dont need to haul so much in beginning.:
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startRadioAntenna", new Vector2(closeDistanceX, closeDistanceY), "item:radioAntenna", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startRadio", new Vector2(closeDistanceX, closeDistanceY), "item:radio", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startShadeleafResin", new Vector2(closeDistanceX, closeDistanceY), "item:shadeleafResin", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startRawhideString", new Vector2(closeDistanceX, closeDistanceY), "item:rawhideString", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startFlintKnife", new Vector2(closeDistanceX, closeDistanceY), "item:flintKnife", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startMetalworkersToolbox", new Vector2(closeDistanceX, closeDistanceY), "item:metalWorkersToolbox", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startAnvil", new Vector2(closeDistanceX, closeDistanceY), "item:anvil", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startBarClamps", new Vector2(closeDistanceX, closeDistanceY), "item:barClamps", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startClayJar", new Vector2(closeDistanceX, closeDistanceY), "item:clayJar", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startBrickMold", new Vector2(closeDistanceX, closeDistanceY), "item:brickMold", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startRefrigerator", new Vector2(closeDistanceX, closeDistanceY), "item:inactivatedFoodCoolerUnit", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startDomeTent", new Vector2(closeDistanceX, closeDistanceY), "item:domeTent", "playerAllegiance", null, delayForItemsAndAgents));

          //mostly for testing://
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startFishtrapTest", new Vector2(testDistanceX, testDistanceY), "item:fishTrapBasket", "playerAllegiance", null, delayForItemsAndAgents));

            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startDaysheenLeaves", new Vector2(testDistanceX, testDistanceY), "item:daysheenLeaves", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startFiregrassSod", new Vector2(testDistanceX, testDistanceY), "item:firegrassSod", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startStones", new Vector2(testDistanceX, testDistanceY), "item:stones", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSticks", new Vector2(testDistanceX, testDistanceY), "item:sticks", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startBlackpowder", new Vector2(testDistanceX, testDistanceY), "item:blackPowder", "playerAllegiance", null, delayForItemsAndAgents));//
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startBlowpipe", new Vector2(testDistanceX, testDistanceY), "item:blowpipe", "playerAllegiance", null, delayForItemsAndAgents));//
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startPickaxe", new Vector2(testDistanceX, testDistanceY), "item:steelPickaxe", "playerAllegiance", null, delayForItemsAndAgents));//
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSpade", new Vector2(testDistanceX, testDistanceY), "item:steelSpade", "playerAllegiance", null, delayForItemsAndAgents));//
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSteelSpade", new Vector2(testDistanceX, testDistanceY), "item:improvisedSpade", "playerAllegiance", null, delayForItemsAndAgents));//
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startCharcoal", new Vector2(testDistanceX, testDistanceY), "item:charcoal", "playerAllegiance", null, delayForItemsAndAgents));//
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startWetFirewood", new Vector2(testDistanceX, testDistanceY), "item:wetFirewood", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startGoldOre", new Vector2(testDistanceX, testDistanceY), "item:goldOre", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startBogOre", new Vector2(testDistanceX, testDistanceY), "item:bogOre", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startCleanTurnipGuts", new Vector2(testDistanceX, testDistanceY), "item:cleanTurnipGuts", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startFingerFruit", new Vector2(testDistanceX, testDistanceY), "item:fingerFruit", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startCrystalBerries", new Vector2(testDistanceX, testDistanceY), "item:crystalBerries", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSalt", new Vector2(testDistanceX, testDistanceY), "item:salt", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startClayPotUnglazed", new Vector2(testDistanceX, testDistanceY), "item:clayPotUnglazed", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startTappingBucket", new Vector2(testDistanceX, testDistanceY), "item:tappingBucket", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startClay", new Vector2(testDistanceX, testDistanceY), "item:clay", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startFirewood", new Vector2(testDistanceX, testDistanceY), "item:firewood", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSolidMudBrick", new Vector2(testDistanceX, testDistanceY), "item:solidMudBrick", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSpoakLeaves", new Vector2(testDistanceX, testDistanceY), "item:spoakLeaves", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSpoakBranchesTrimmed", new Vector2(testDistanceX, testDistanceY), "item:spoakBranchesTrimmed", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startMarshcotSap", new Vector2(testDistanceX, testDistanceY), "item:marshcotSap", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startWingweedMats", new Vector2(testDistanceX, testDistanceY), "item:wingweedMat", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSpoakShingles", new Vector2(testDistanceX, testDistanceY), "item:spoakShingles", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startIronHandAxe", new Vector2(testDistanceX, testDistanceY), "item:steelHandAxe", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startWaterCaneStem", new Vector2(testDistanceX, testDistanceY), "item:waterCaneStem", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startShadeleafCanes", new Vector2(testDistanceX, testDistanceY), "item:shadeleafCanes", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startVat", new Vector2(testDistanceX, testDistanceY), "item:vat", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startImprovisedGreenHouseCover", new Vector2(testDistanceX, testDistanceY), "item:improvisedGreenHouseCover", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startTurnipSalami", new Vector2(testDistanceX, testDistanceY), "item:turnipSalami", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startDriedSaltedStreakFin", new Vector2(testDistanceX, testDistanceY), "item:driedSaltedStreakFin", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startVinegar", new Vector2(testDistanceX, testDistanceY), "item:vinegar", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startHardtack", new Vector2(testDistanceX, testDistanceY), "item:hardtack", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startRareMetal2", new Vector2(testDistanceX, testDistanceY), "item:terbium", "playerAllegiance", null, delayForItemsAndAgents));
          
          
            #endregion
            #region Structures
            list.Add(ScenarioLoader.SpawnItemAtStartLocation("startStructureSmallTent", new Vector2(0f, 0f), "structure:smallTent", "playerAllegiance", null, delayForStructures, "Small Tent"));
            list.Add(ScenarioLoader.SpawnItemAtStartLocation("startStructureDomeTent", new Vector2(80f, 0f), "structure:domeTent", "playerAllegiance", null, delayForStructures, "Dome Tent"));
            list.Add(ScenarioLoader.SpawnItemAtStartLocation("startStructureFieldKitchen", new Vector2(128f, 80f), "structure:fieldKitchen", "playerAllegiance", null, delayForStructures));
            list.Add(ScenarioLoader.SpawnItemAtStartLocation("startStructureHelipadBig", new Vector2(0f, 188f), "structure:helipadBig", "playerAllegiance", null, delayForStructures, "Helipad"));
            list.Add(ScenarioLoader.SpawnItemAtStartLocation("startStructureHelipadBig2", new Vector2(-100f, 188f), "structure:helipadBig", "playerAllegiance", null, delayForStructures, "Helipad2"));
         
            #endregion

            #region CustomScenario gear
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startCloak", new Vector2(xItemPosition, yItemPosition), "item:cloak", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startNightVisionGoggles", new Vector2(xItemPosition, yItemPosition), "item:nightVisionGoggles", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startGroundScanner", new Vector2(xItemPosition, yItemPosition), "item:groundScanner", "playerAllegiance", null, delayForItemsAndAgents));


            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startRefinery1", new Vector2(xItemPosition, yItemPosition), "item:metalRefineryEquipment", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startRefinery2", new Vector2(xItemPosition, yItemPosition), "item:metalRefineryPart1", "playerAllegiance", null, delayForItemsAndAgents));
           
            
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSentryOutside", new Vector2(xItemPosition, yItemPosition), "item:sentry", "playerAllegiance", null, delayForItemsAndAgents));

            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSatelliteGroundStation", new Vector2(xItemPosition, yItemPosition), "item:satelliteGroundStation", "playerAllegiance", null, delayForItemsAndAgents));

            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startLiquidGas", new Vector2(xItemPosition, yItemPosition), "item:liquidGas", "playerAllegiance", null, delayForItemsAndAgents));
            
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startThermalTarp", new Vector2(xItemPosition, yItemPosition), "item:thermalTarp", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startIronCanisterForSale", "Helipad", "item:ironCanister", "playerAllegiance", null, delayForItemsAndAgents, offerForSale: true));

            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startHuntingRifle", "Small Tent", "item:coilRifle", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startRifleAmmo", "Small Tent", "item:coilRifleAmmo", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startSentry", "Small Tent", "item:sentry", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSentryAmmo", new Vector2(xItemPosition, yItemPosition), "item:sentryGunAmmo", "playerAllegiance", null, delayForItemsAndAgents));

            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSimCoffeeBeans", new Vector2(xItemPosition, yItemPosition), "item:simCoffeeBeans", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startCookingPot", "Dome Tent", "item:advancedCookingPot", "playerAllegiance", null, delayForItemsAndAgents));

            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startMachete", new Vector2(xItemPosition, yItemPosition), "item:advancedMachete", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startSnips", "Dome Tent", "item:advancedSnips", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startString", new Vector2(xItemPosition, yItemPosition), "item:advancedString", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startKnife", new Vector2(xItemPosition, yItemPosition), "item:advancedKnife", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startBasicFireExtinguisher", new Vector2(xItemPosition, yItemPosition), "item:basicFireExtinguisher", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startChickenMeat", new Vector2(xItemPosition, yItemPosition), "item:thunderChickenMeat", "playerAllegiance", null, delayForItemsAndAgents));

            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startFieldKitchenStove", new Vector2(xItemPosition, yItemPosition), "item:fieldKitchenStove", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startFieldKitchenEquipment", new Vector2(xItemPosition, yItemPosition), "item:fieldKitchenEquipment", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startDomeTentItem", new Vector2(xItemPosition, yItemPosition), "item:domeTent", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSmallTentItem", new Vector2(xItemPosition, yItemPosition), "item:smallTent", "playerAllegiance", null, delayForItemsAndAgents));
            

            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSatteliteGroundStationItem", new Vector2(xItemPosition, yItemPosition), "item:satelliteGroundStation", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startFieldLabPacked", new Vector2(xItemPosition, yItemPosition), "item:fieldLabPacked", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startStructurePanels", new Vector2(xItemPosition, yItemPosition), "item:structurePanels", "playerAllegiance", null, delayForItemsAndAgents));
            
            #endregion

            #endregion



            #region Natural terminals

            #region used for most. relative position
            list.Add(new SpawnEntityAction()
          {
              KeyName = "startNaturalTerminalGenericPosition",
              DelayInSeconds = 1,

              DynamicLocation = new DynamicLocation() // dynamic location!
              {
                  PropertyKey = "startingLocation"
              },
              EntityData = new EntityData()
              {
                  EntityKey = "terrain:naturalLandTerminal",
                  Name = "To: The Valley",
                  Location = new Vector3(0f, -150f, 0f) //

              },

          });

          #endregion

          #region Natural terminals

          list.Add(new SpawnEntityAction()
          {
              KeyName = "startNaturalTerminalFishingPosition",
              DelayInSeconds = 1,

              /*   DynamicLocation = new DynamicLocation() 
                 {
                     PropertyKey = "startingLocation"
                 },*/
              EntityData = new EntityData()
              {
                  EntityKey = "terrain:naturalLandTerminal",
                  Name = "To: The Plains",
                  Location = new Vector3(3360f, 3024f, 0f)
                  

              },

          });

          #endregion

          #endregion

            #region detect North passage from the start
            list.Add(new ExploreAction()
            {
                Comments = "detect North passage from the start",
                KeyName = "exploreShroudNaturalTerminal",
                DelayInSeconds = delayForItemsAndAgents + 1, // must execute last, after members have been added

                OffsetLocationStart = new Vector2(1800f, 0f),
                RadiusStart = 120f,
                DetectMode = InGameEvents.Actions.DetectMode.DetectAlwaysSeenEntities,
                //PerformDetection = true,

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

            #region Starting Structures //for testing


            list.Add(ScenarioLoader.SpawnItemAtStartLocation("startStructureGroundStation", new Vector2(-100f, 40f), "structure:satelliteGroundStation", "playerAllegiance", null, delayForStructures));
            list.Add(ScenarioLoader.SpawnItemAtStartLocation("startStructureHelipad", new Vector2(-200f, 40f), "structure:helipadBig", "playerAllegiance", null, delayForStructures));           
            list.Add(ScenarioLoader.SpawnItemAtStartLocation("startStructureLanding", new Vector2(0f, -480f), "structure:landingImprovised", "playerAllegiance", null, delayForStructures, "Landing"));
            list.Add(ScenarioLoader.SpawnItemAtStartLocation("startStructureSimplePort", new Vector2(0f, 0f) /* new Vector2(0f, 188f)*/, "structure:simplePort", "playerAllegiance", null, delayForStructures, "Pier"));
            list.Add(ScenarioLoader.SpawnItemAtStartLocation("startStructureRadioHutImprovised", new Vector2(0f, -100f), "structure:radioHutImprovised", "playerAllegiance", null, delayForStructures));
            list.Add(ScenarioLoader.SpawnItemAtStartLocation("startStructureImprovisedSmithy", new Vector2(-80f, -50f), "structure:improvisedSmithy", "playerAllegiance", null, delayForStructures));

            // list.Add(ScenarioLoader.SpawnItemAtStartLocation("startHarpyTest", new Vector2(-100f, -40f), "entity:harpy", "playerAllegiance", null, delayForStructures, rotation: 3f * MathHelper.PiOver4));


            #endregion

            #region placeExpedition. 
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
                                { RatingTypes.Comfort, "basic"  },
                                { RatingTypes.Food, "basic"  }, 
                                { RatingTypes.Security, "advanced"  }
                            }
                      },
                      Location = new ValueNode()
                      {
                          PropertyKey = "startingLocation"
                      }
                  }

              });

            #endregion

            #region setStartingLocations
            list.Add(new SetPropertyAction()
            {
                KeyName = "setStartingLocationSouth", //was "setStartingLocationRiverBank"

                PropertyKey = "startingLocation",
                Value = new ValueNode()
                {
                    Location = new Microsoft.Xna.Framework.Vector2(2640f, 2880f) //
                }

            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "setStartingLocationNorth",

                PropertyKey = "startingLocation",
                Value = new ValueNode()
                {
                    Location = new Microsoft.Xna.Framework.Vector2(2640f, 2880f) // 
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

                KeyName = "exploreShroudFromSouth", //was "exploreShroudRiverBank"
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
                RadiusStart = 300f,
                RadiusEnd = 500f,
                DetectMode = InGameEvents.Actions.DetectMode.DetectAlwaysSeenEntities,
                //PerformDetection = true,
                OffsetLocationEnd = new Microsoft.Xna.Framework.Vector2(1000f, 0f),
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



/*
            list.Add(new ExploreAction()
            {

                KeyName = "exploreShroudNorthArableLand",
                DelayInSeconds = delayForItemsAndAgents + 1, // must execute last, after members have been added


                DynamicLocationStart = new ValueNode()
                {
                    PropertyKey = "startingLocation"
                },

                RadiusStart = 300f,
                RadiusEnd = 500f,
                DetectMode = InGameEvents.Actions.DetectMode.DetectAlwaysSeenEntities,
                //PerformDetection = true,
                OffsetLocationEnd = new Microsoft.Xna.Framework.Vector2(4704f, 96f),

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


            list.Add(new ExploreAction()
            {

                KeyName = "exploreShroudSouthEastRockyFiregrass",
                DelayInSeconds = delayForItemsAndAgents + 1, // must execute last, after members have been added


                DynamicLocationStart = new ValueNode()
                {
                    PropertyKey = "startingLocation"
                },

                RadiusStart = 300f,
                RadiusEnd = 500f,
                DetectMode = InGameEvents.Actions.DetectMode.DetectAlwaysSeenEntities,
                //PerformDetection = true,
                OffsetLocationEnd = new Microsoft.Xna.Framework.Vector2(4128f, 6096f),

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
*/


            #endregion

            #region FAUNA
            #region Spawn trigger: destroys animals at map edge.
            list.Add(new SpawnTriggerAction()
            {
                KeyName = "spawnAnimalMigrateTriggerWest", //zone at edge of map where animals are destroyed to simulate them leaving the map

                Location = new ValueNode()
                {

                    Location = new Vector2(48, 2112) // this is the center of the box, not the upper left corner.
                },
                AreaDimensions = new Vector2(36f, 500f),
                TriggerType = "animalMigrateTrigger" //defined for the animals lesser whipjaw and bajingan in creatureloader

            });

            list.Add(new SpawnTriggerAction()
            {
                KeyName = "spawnAnimalMigrateTriggerRiver", //zone at edge of map where animals are destroyed to simulate them leaving the map

                Location = new ValueNode()
                {

                    Location = new Vector2(3168, 3744) // this is the center of the box, not the upper left corner.
                },
                AreaDimensions = new Vector2(300f, 38f),
                TriggerType = "animalMigrateTrigger" //defined for the animals lesser whipjaw and bajingan in creatureloader

            });
            #endregion

            #region Fauna Expeditions and allegiance


            #region Birds #1
            list.Add(new SpawnAllegianceAction()
            {
                KeyName = "spawnBirdExpedition#1", // at rocky river bed start location

                Site = "playSite",
                ExpeditionData = new ExpeditionData()
                {
                    KeyName = "birdExpedition#1",
                    AllegianceKey = "birdAllegiance#1",
                    Location = new ValueNode()
                    {
                        Location = new Vector2(2736, 5280)
                    },
                    PopulationData = new PopulationData()
                    {
                        StartMembersList = new[] { "bird#1", "bird#2", "bird#3" }, //birds require racekey
                        StartMembers = 3,
                        MaxMembers = 3,                         
                        GrowthInMembersPerDay = 0f
                        //     SpawnRadius = not used bc of member loc spawns, see that doc
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

            #region Birds #2 tiny
            list.Add(new SpawnAllegianceAction()
            {
                KeyName = "spawnBirdExpedition#2", //tiny guano birds north east caves

                Site = "playSite",
                ExpeditionData = new ExpeditionData()
                {
                    KeyName = "birdExpedition#2",
                    AllegianceKey = "birdAllegiance#2",
                    Location = new ValueNode()
                    {
                        Location = new Vector2(5520, 4080)
                    },
                    PopulationData = new PopulationData()
                    {
                        StartMembersList = new[] { "bird#4", "bird#5", "bird#6" }, //birds require racekey
                        StartMembers = 3,
                        MaxMembers = 3,
                        GrowthInMembersPerDay = 0f
                        //     SpawnRadius =  not used bc of member loc spawns, see that doc
                    }
                },
                AllegianceData = new AllegianceData()
                {
                    KeyName = "birdAllegiance#2",
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

 


            #region Bushdragon North
            list.Add(new SpawnAllegianceAction()
            {
                KeyName = "spawnBushDragonExpedition#1",

                Site = "playSite",
                ExpeditionData = new ExpeditionData()
                {
                    KeyName = "bushDragonAllegianceNorth",
                    Name = "Bush Dragon Allegiance North",
                    AllegianceKey = "bushDragonAllegianceNorth",
                    Location = new ValueNode()
                    {
                        Location = new Vector2(2592, 432)
                    },
                    PopulationData = new PopulationData()
                    {
                        SpawnRadius = 150f,
                        StartMembers = 2,
                        MaxMembers = 2,
                        GrowthInMembersPerDay = 0.3f                        
                    }
                },
                AllegianceData = new AllegianceData()
                {
                    ForageAndHuntingRadius = 400,
                    Name = "Bush Dragon Allegiance North",
                    KeyName = "bushDragonAllegianceNorth",
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
                            Location = new Vector2(3456, 3984)//(2016, 2496) //4384, 2189 // 3072, 1488 //
                        },
                    PopulationData = new PopulationData()
                    {
                        MaxMembers = 4,
                        StartMembers = 4,
                        GrowthInMembersPerDay = 10f,
                        SpawnRadius = 1500


                    }
                },
                AllegianceData = new AllegianceData()
                {
                    ForageAndHuntingRadius = 2000,
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
                        Location = new Vector2(3446, 3974)//(1536, 1104) //4900,4900
                    },
                    PopulationData = new PopulationData()
                    {
                        MaxMembers = 4,
                        StartMembers = 4,
                        GrowthInMembersPerDay = 10f,
                        SpawnRadius = 1500

                    }
                },
                AllegianceData = new AllegianceData()
                {
                    ForageAndHuntingRadius = 2000,
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
                        Location = new Vector2(4224, 1104)
                    },
                    PopulationData = new PopulationData()
                    {
                        SpawnRadius = 150f,
                        StartMembers = 3,
                        MaxMembers = 6,
                        GrowthInMembersPerDay = 0.9f                        
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

            #region Turnip south
            list.Add(new SpawnAllegianceAction()
            {
                KeyName = "spawnTurnipExpeditionSouth",

                Site = "playSite",
                ExpeditionData = new ExpeditionData()
                {
                    KeyName = "turnipAllegianceNorth",
                    Name = "Turnip Allegiance North",
                    AllegianceKey = "turnipAllegianceNorth",
                    Location = new ValueNode()
                    {
                        Location = new Vector2(3456, 4416)
                    },
                    PopulationData = new PopulationData()
                    {
                        SpawnRadius = 100,
                        StartMembers = 5,
                        MaxMembers = 5,
                        GrowthInMembersPerDay = 0.2f //turnip should be a rare animal. once killed it should take a while before a new emerges

                    }
                },
                AllegianceData = new AllegianceData()
                {
                    ForageAndHuntingRadius = 500,
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
                        Location = new Vector2(4368, 4848)//
                    },
                    PopulationData = new PopulationData()
                    {
                        SpawnSources = new string[] { "Field Quadite Nest 1" },
                        StartSpawnSources = new string[] { "fieldQuaditeNest1" }, // needed to allow a starting pop
                        StartMembers = 2,
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

            #region Demon Tree #3 northwest not used
            list.Add(new SpawnAllegianceAction()
            {
                KeyName = "spawnDemonTreeExpedition#3",

                Site = "playSite",
                ExpeditionData = new ExpeditionData()
                {
                    KeyName = "demonTreeAllegiance#3",
                    Name = "Demon Tree Allegiance #3",
                    AllegianceKey = "demonTreeAllegiance#3",
                    Location = new ValueNode()
                    {
                        Location = new Vector2(2496, 480)
                    },
                    PopulationData = new PopulationData()
                    {
                        MaxMembers = 1,
                        StartMembers = 1,
                        GrowthInMembersPerDay = 0.7f,
                        SpawnRadius = 100

                    }
                },
                AllegianceData = new AllegianceData()
                {
                    ForageAndHuntingRadius = 300,
                    Name = "Demon Tree Allegiance #3",
                    KeyName = "demonTreeAllegiance#3",
                    EntityType = "entity:spoakDendront",
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

            #region Patrician Placed southwest
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
                        Location = new Vector2(1152, 4804) //patricians will leave scout zone quite a lot in pursuit. so keep the expedition at a distance from camp.
                    },
                    PopulationData = new PopulationData()
                    {
                        MaxMembers = 5,
                        StartMembers = 3,
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


            #region Demon Tree #1 north east
            list.Add(new SpawnAllegianceAction()
            {
                KeyName = "spawnDemonTreeExpedition#1",

                Site = "playSite",
                ExpeditionData = new ExpeditionData()
                {
                    KeyName = "demonTreeAllegiance#1",
                    Name = "Demon Tree Allegiance #1",
                    AllegianceKey = "demonTreeAllegiance#1",
                    Location = new ValueNode()
                    {
                        Location = new Vector2(4608, 2976)
                    },
                    PopulationData = new PopulationData()
                    {
                        MaxMembers = 1,
                        StartMembers = 1,
                        GrowthInMembersPerDay = 0.7f,
                        SpawnRadius = 100

                    }
                },
                AllegianceData = new AllegianceData()
                {
                    ForageAndHuntingRadius = 200,
                    Name = "Demon Tree Allegiance #1",
                    KeyName = "demonTreeAllegiance#1",
                    EntityType = "entity:spoakDendront",
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
                    KeyName = "swampDemonTreeAllegiance#2",
                    Name = "Swamp Demon Tree Allegiance #2",
                    AllegianceKey = "swampDemonTreeAllegiance#2",
                    Location = new ValueNode()
                    {
                        Location = new Vector2(5088, 480)
                    },
                    PopulationData = new PopulationData()
                    {
                        MaxMembers = 1,
                        StartMembers = 1,
                        GrowthInMembersPerDay = 0.7f,
                        SpawnRadius = 100

                    }
                },
                AllegianceData = new AllegianceData()
                {
                    ForageAndHuntingRadius = 200,
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


            #region snatcher whipjaw southwest
            //NA  been placed bottem left site(volcanic)
            list.Add(new SpawnAllegianceAction()
            {
                KeyName = "spawnSnatcherExpedition#1",

                Site = "playSite",
                ExpeditionData = new ExpeditionData()
                {
                    KeyName = "snatcherAllegiance#1",
                    Name = "Whipjaw Allegiance #1",
                    AllegianceKey = "snatcherAllegiance#1",
                    Location = new ValueNode()
                    {
                        Location = new Vector2(768, 3264)//
                    },
                    PopulationData = new PopulationData()
                    {
                        MaxMembers = 1,
                        StartMembers = 1,
                        GrowthInMembersPerDay = 0.7f,
                        SpawnRadius = 250

                    }
                },
                AllegianceData = new AllegianceData()
                {
                    ForageAndHuntingRadius = 500,
                    Name = "Whipjaw Allegiance #1",
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
                KeyName = "spawnThunderChickenExpedition#2",

                Site = "playSite",
                ExpeditionData = new ExpeditionData()
                {
                    KeyName = "thunderChickenAllegiance#2",
                    Name = "Thunder Chicken Allegiance #2",
                    AllegianceKey = "thunderChickenAllegiance#2",
                    Location = new ValueNode()
                    {
                        Location = new Vector2(5472, 2400)                        
                    },
                    PopulationData = new PopulationData()
                    {
                         MaxMembers = 5,//was 2  july 2016
                         StartMembers = 3,
                         GrowthInMembersPerDay = 10f,
                         SpawnRadius = 400

                    }
                },
                AllegianceData = new AllegianceData()
                {
                    ForageAndHuntingRadius = 700,
                    Name = "Thunder Chicken Allegiance #2",
                    KeyName = "thunderChickenAllegiance#2",
                    EntityType = "entity:whiteThunderChicken",
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

            
            #region Swarmer expedition 1#
            // one expedition of swarmers(which is highly aggressive) set up using CreateExpeditionAction instead of CreateAlligianceAction
            list.Add(new CreateExpeditionAction()
            {
                KeyName = "spawnSwarmerExpedition1",// not player's site.
                DelayInSeconds = 0.1, // wait for starting location property to have been set!

                
                ExpeditionData = new ExpeditionData()
            {
                KeyName = "swarmerExpedition#1",
                AllegianceKey = "swarmerAllegiance#1",
                    Name = "Swarmer Expedition",
                    Location = new ValueNode()
                    {
                        Location = new Vector2(960, 1104)
                    },
                    PopulationData = new PopulationData()
                    {
                        SpawnSources = new string[] { "Swarmer nest 3", "Swarmer nest 2", "Swarmer nest 4" },
                        StartSpawnSources = new string[] { "swarmerNest3", "swarmerNest2", "swarmerNest4" }, // needed to allow a starting pop
                        StartMembers = 5,
                        MaxMembers = 10,
                        GrowthInMembersPerDay = 13.2f
                    },

                
            },
                //AllegianceKey = "swarmerAllegiance#1",
                //ExpeditionDataKey = "swarmerExpedition#1"
            });

            #endregion
            #region Swarmer expedition 2#
            // one expedition of swarmers(which is highly aggressive) set up using CreateExpeditionAction instead of CreateAlligianceAction
            list.Add(new CreateExpeditionAction()
            {
                KeyName = "spawnSwarmerExpedition2",// not player's site.
                DelayInSeconds = 0.1, // wait for starting location property to have been set!


                ExpeditionData = new ExpeditionData()
                {
                    KeyName = "swarmerExpedition#2",
                    AllegianceKey = "swarmerAllegiance#1",
                    Name = "Swarmer Expedition",
                    Location = new ValueNode()
                    {
                        Location = new Vector2(1824, 1632)// was 3600. 1248
                    },
                    PopulationData = new PopulationData()
                    {
                        SpawnSources = new string[] { "Swarmer nest 1", "Swarmer nest 5", "Swarmer nest 6" },
                        StartSpawnSources = new string[] { "swarmerNest1", "swarmerNest5","swarmerNest6" }, // needed to allow a starting pop
                        StartMembers = 5,
                        MaxMembers = 10,
                        GrowthInMembersPerDay = 13.2f
                    }
                },
                //AllegianceKey = "swarmerAllegiance#1",
                //ExpeditionDataKey = "swarmerExpedition#1"
            });
            #endregion
            #region "spawnSwarmerAllegiance1"
            list.Add(
                new SpawnAllegianceAction()
                {
                    KeyName = "spawnSwarmerAllegiance1",

                    Site = "playSite", // "otherSite1",
                    AllegianceData = new AllegianceData()
                {
                    ForageAndHuntingRadius = 700,
                    Name = "Swarmer allegiance ",
                    KeyName = "swarmerAllegiance#1",
                    EntityType = "entity:swarmer", 
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
            
            //only expedition 3 is used at the moment.. maybe fix alligiance so that more nests can be used?
           

            #endregion

            #region Spawn intervals and pop caps //move data to expeditions above

            #region spawn intervals

            //todo migrate to the new system used in scenario5    

            list.Add(new SetPropertyAction() //todo migrate to the new system used in scenario5
            {
                KeyName = "setSpawnIntervalLeafcutterNormal",

                PropertyKey = "leafcutterSpawnInterval",
                Value = new ValueNode() { Int = 173 },

            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "setSpawnIntervalLesserWhipjawMigration",

                PropertyKey = "lesserWhipjawMigrationInterval",
                Value = new ValueNode() { Int = 3200 },//mp was 800      maybe around morning first time?....this should be told to the player so he can prepare for the event.

            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "setSpawnIntervalBajinganMigration",

                PropertyKey = "bajinganMigrationInterval",
                Value = new ValueNode() { Int = 4800 },//mp was 1800            best not during night.. ..this should be told to the player so he can prepare for the event.

            });
            #endregion

            #region max pop //todo migrate to the new system used in scenario5

            // Remember that the actual spawned amount is the Value +1  ........MP what does that mean???

 
            list.Add(new SetPropertyAction()
            {
                KeyName = "setMaxLeafcuttersNormal",

                PropertyKey = "maxLeafcutters",
                Value = new ValueNode() { Int = 5 },

            });


            list.Add(new SetPropertyAction()
            {
                KeyName = "setMaxLesserWhipjawNormal",

                PropertyKey = "maxLesserWhipjaw",
                Value = new ValueNode() { Int = 14 }, //mp 9 individuals per migration event currently

            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "setMaxBajinganNormal",

                PropertyKey = "maxBajingan",
                Value = new ValueNode() { Int = 19 }, //mp 14 individuals per migration event currently

            });
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

            #region Resources places like farms fish spots deposits etc.

            #region Farmplots Small
            // group Top North
            #region #1
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFarmSpotSmall1",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:smallPlotSpot",
                    Name = "Small plot 1",
                    Location = new Vector3(3446f, 886f, 0)
                },


            });
            #endregion
            #region #2

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFarmSpotSmall2",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:smallPlotSpot",
                    Name = "Small plot 2",
                    Location = new Vector3(3362f, 1005f, 0)
                },


            });
            #endregion
            #region #3
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFarmSpotSmall3",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:smallPlotSpot",
                    Name = "Small plot 3",
                    Location = new Vector3(2923f, 1013f, 0)
                },


            });
            #endregion
            #region #4
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFarmSpotSmall4",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:smallPlotSpot",
                    Name = "Small plot 4",
                    Location = new Vector3(2971f, 1132f, 0)
                },


            });
            #endregion

            // group east on map
            #region #5
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFarmSpotSmall5",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:smallPlotSpot",
                    Name = "Small plot 5",
                    Location = new Vector3(4889f, 2158f, 0)
                },


            });
            #endregion
            #region #6
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFarmSpotSmall6",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:smallPlotSpot",
                    Name = "Small plot 6",
                    Location = new Vector3(3884f, 2820f, 0)
                },


            });

            #endregion
            #region #7
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFarmSpotSmall7",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:smallPlotSpot",
                    Name = "Small plot 7",
                    Location = new Vector3(4075f, 3077f, 0)
                },


            });
            #endregion
            #region #8
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFarmSpotSmall8",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:smallPlotSpot",
                    Name = "Small plot 8",
                    Location = new Vector3(5142f, 3156f, 0)
                },


            });
            #endregion

            // group Center on map
            #region #9

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFarmSpotSmall9",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:smallPlotSpot",
                    Name = "Small plot 9",
                    Location = new Vector3(2255f, 2827f, 0)
                },


            });
            #endregion
            #region #10
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFarmSpotSmall10",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:smallPlotSpot",
                    Name = "Small plot 10",
                    Location = new Vector3(2589f, 2996f, 0)
                },


            });
            #endregion
            #region #11
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFarmSpotSmall11",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:smallPlotSpot",
                    Name = "Small plot 11",
                    Location = new Vector3(2985f, 2975f, 0)
                },


            });
            #endregion

            // biggest group south
            #region #12
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFarmSpotSmall12",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:smallPlotSpot",
                    Name = "Small plot 12",
                    Location = new Vector3(3189f, 4296f, 0)
                },


            });
            #endregion
            #region #13
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFarmSpotSmall13",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:smallPlotSpot",
                    Name = "Small plot 13",
                    Location = new Vector3(4164f, 4379f, 0)
                },


            });

            #endregion
            #region #14
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFarmSpotSmall14",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:smallPlotSpot",
                    Name = "Small plot 14",
                    Location = new Vector3(3887f, 3996f, 0)
                },


            });
            #endregion
            #region #15
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFarmSpotSmall15",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:smallPlotSpot",
                    Name = "Small plot 15",
                    Location = new Vector3(3444f, 4609f, 0)
                },


            });
            #endregion
            #region #16
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFarmSpotSmall16",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:smallPlotSpot",
                    Name = "Small plot 16",
                    Location = new Vector3(4180f, 4813f, 0)
                },


            });
            #endregion

            // Group far to south west on the map
            #region #17
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFarmSpotSmall17",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:smallPlotSpot",
                    Name = "Small plot 17",
                    Location = new Vector3(980f, 4716f, 0)
                },


            });

            #endregion
            #region #18
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFarmSpotSmall18",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:smallPlotSpot",
                    Name = "Small plot 18",
                    Location = new Vector3(999f, 4835f, 0)
                },


            });
            #endregion

            #endregion

            #region Farmplots Large
            #region #1
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFarmSpotLarge1",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:largePlotSpot",
                    Name = "Large plot 1",
                    Location = new Vector3(3221f, 867f, 0)
                },

            });
            #endregion
            #region #2
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFarmSpotLarge2",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:largePlotSpot",
                    Name = "Large plot 2",
                    Location = new Vector3(3825f, 3176f, 0)
                },

            });
            #endregion
            #region #3
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFarmSpotLarge3",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:largePlotSpot",
                    Name = "Large plot 3",
                    Location = new Vector3(3420f, 4314f, 0)
                },

            });
            #endregion
            #region #4 Not in use for now.
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFarmSpotLarge4",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:largePlotSpot",
                    Name = "Large plot 4",
                    Location = new Vector3(2126f, 2600f, 0)
                },

            });
            #endregion


            #endregion

            #region Pier Spots



            #region #1 
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startPierSpot1",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:pierSpot",
                    Name = "Pier spot",
                    Location = new Vector3(3840f, 922f, 0)
                },

            });
            #endregion
            #region #2
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startPierSpot2",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:pierSpot",
                    Name = "Pier spot",
                    Location = new Vector3(1865f, 4810f, 0)
                },

            });
            #endregion
            #region #3 
            list.Add(new SpawnEntityAction() //north east
            {
                KeyName = "startPierSpot3",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:pierSpot",
                    Name = "Pier spot",
                    Location = new Vector3(4684f, 5084f, 0) //was 3330f, 650f, 0  but too far away for bugfree hauling
                },

            });
            #endregion
            #endregion

            #region Fish weir spots
            // sorted by most north to most south
            #region #1
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFishTrapCreek1",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:fishTrapSpotCreek",
                    Name = "Fish weir spot (5360f, 969f)",
                    Location = new Vector3(5360f, 969f, 0)
                },

            });
            #endregion
            #region #2

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFishTrapCreek2",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:fishTrapSpotCreek",
                    Name = "Fish weir spot (5618f, 1767f)",
                    Location = new Vector3(5618f, 1767f, 0) 
                },

            });
            #endregion
            #region #3
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFishTrapCreek3",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:fishTrapSpotCreek",
                    Name = "Fish weir spot (4978f, 1881f)",
                    Location = new Vector3(4978f, 1881f, 0) //was inland: 3792f, 2736f,
                },

            });
            #endregion
            #region #4
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFishTrapCreek4",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:fishTrapSpotCreek",
                    Name = "Fish weir spot (2690f, 2691f)",
                    Location = new Vector3(2690f, 2691f, 0) //was inland: 3792f, 2736f,
                },

            });
            #endregion
            #region #5
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFishTrapCreek5",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:fishTrapSpotCreek",
                    Name = "Fish weir spot (3421f, 3206f)",
                    Location = new Vector3(3421f, 3206f, 0)
                },

            });
            #endregion
            #region #6

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFishTrapCreek6",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:fishTrapSpotCreek",
                    Name = "Fish weir spot (5454f, 3356f)",
                    Location = new Vector3(5454f, 3356f, 0) //was inland: 3792f, 2736f,
                },

            });
            #endregion
            #region #7
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFishTrapCreek7",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:fishTrapSpotCreek",
                    Name = "Fish weir spot (3908f, 3440f)",
                    Location = new Vector3(3908f, 3470f, 0) //was inland: 3792f, 2736f,
                },

            });
            #endregion
            #region #8
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFishTrapCreek8",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:fishTrapSpotCreek",
                    Name = "Fish weir spot (3305f, 3770f)",
                    Location = new Vector3(3305f, 3770f, 0) //was inland: 3792f, 2736f,
                },

            });
            #endregion
            #region #9
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFishTrapCreek9",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:fishTrapSpotCreek",
                    Name = "Fish weir spot (1640f, 4242f)",
                    Location = new Vector3(1640f, 4242f, 0)
                },

            });
            #endregion
            #region #10
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFishTrapCreek10",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:fishTrapSpotCreek",
                    Name = "Fish weir spot (2380f, 3200f)",
                    Location = new Vector3(1122f, 4292f, 0) //was inland: 3792f, 2736f,
                },

            });
            #endregion
            #endregion

            #region Fish Trap Saltwater Spots (Coast)
            // from most north to most south
            #region #1
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFishTrapCoast1",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:fishTrapSpotCoast",
                    Name = "Fish trap spot coast",
                    Location = new Vector3(2089f, 4777f, 0)
                },

            });
            #endregion
            #region #2
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFishTrapCoast2",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:fishTrapSpotCoast",
                    Name = "Fish trap spot coast",
                    Location = new Vector3(5808f, 4944f, 0)
                },

            });
            #endregion
            #region #3
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFishTrapCoast3",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:fishTrapSpotCoast",
                    Name = "Fish trap spot coast",
                    Location = new Vector3(634f, 5331f, 0)
                },

            });

            #endregion
            #region #4
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFishTrapCoast4",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:fishTrapSpotCoast",
                    Name = "Fish trap spot coast",
                    Location = new Vector3(1838f, 5680f, 0)
                },

            });
            #endregion
            #region #5
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFishTrapCoast5",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:fishTrapSpotCoast",
                    Name = "Fish trap spot coast",
                    Location = new Vector3(5116f, 5542f, 0)
                },

            });
            #endregion
            #region #6

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFishTrapCoast6",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:fishTrapSpotCoast",
                    Name = "Fish trap spot coast",
                    Location = new Vector3(2612f, 5732f, 0) 
                },

            });
            #endregion
            #region #7
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFishTrapCoast7",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:fishTrapSpotCoast",
                    Name = "Fish trap spot coast",
                    Location = new Vector3(4103f, 5749f, 0)
                },

            });
            #endregion
            

            #endregion

            #region Fish Trap Freshwater Spots (Shore)
            // from most north to most south
            #region #1
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFishTrapShore1",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:fishTrapSpotShore",
                    Name = "Fish trap spot Freshwater",
                    Location = new Vector3(3362f, 469f, 0)
                },

            });
            #endregion
            #region #2
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFishTrapShore2",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:fishTrapSpotShore",
                    Name = "Fish trap spot Freshwater",
                    Location = new Vector3(616f, 533f, 0)
                },

            });
            #endregion
            #region #3
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFishTrapShore3",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:fishTrapSpotShore",
                    Name = "Fish trap spot Freshwater",
                    Location = new Vector3(4630f, 585f, 0)
                },

            });

            #endregion
            #region #4
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFishTrapShore4",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:fishTrapSpotShore",
                    Name = "Fish trap spot Freshwater",
                    Location = new Vector3(3582f, 1110f, 0)
                },

            });
            #endregion
            #region #5
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFishTrapShore5",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:fishTrapSpotShore",
                    Name = "Fish trap spot Freshwater",
                    Location = new Vector3(1679f, 2311f, 0)
                },

            });
            #endregion
            #region #6

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFishTrapShore6",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:fishTrapSpotShore",
                    Name = "Fish trap spot Freshwater",
                    Location = new Vector3(3312f, 2616f, 0)
                },

            });
            #endregion

            #endregion

            #region bog ore deposits
            // from ost north to most south
            #region bog ore deposit 1
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startBogOreDeposit1", 
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:bogOreDeposit",
                    Name = "Bog ore deposit",
                    Location = new Vector3(2612f, 412f, 0) // south east corner 
                },

            });
            #endregion
            #region bog ore deposit 2
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startBogOreDeposit2",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:bogOreDeposit",
                    Name = "Bog ore deposit",
                    Location = new Vector3(1724f, 1435f, 0) // south east corner 
                },

            });
            #endregion
            #region bog ore deposit 3
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startBogOreDeposit3",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:bogOreDeposit",
                    Name = "Bog ore deposit",
                    Location = new Vector3(2480f, 2783f, 0) // south east corner 
                },

            });
            #endregion
            #region bog ore deposit 4
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startBogOreDeposit4",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:bogOreDeposit",
                    Name = "Bog ore deposit",
                    Location = new Vector3(3528f, 4118f, 0) // south east corner 
                },

            });
            #endregion
            #endregion

            #region peat deposits
            // from most north to most south
            #region peat deposit 1
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startPeatDeposit1", 
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:peatDeposit",
                    Name = "Peat deposit",
                    Location = new Vector3(3026f, 850f, 0) //
                },

             });
            #endregion
            #region peat deposit 2
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startPeatDeposit2",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:peatDeposit",
                    Name = "Peat deposit",
                    Location = new Vector3(1495f, 1292f, 0) //
                },

            });
            #endregion
            #region peat deposit 3
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startPeatDeposit3",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:peatDeposit",
                    Name = "Peat deposit",
                    Location = new Vector3(5100f, 1442f, 0) //
                },

            });
            #endregion
            #region peat deposit 4
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startPeatDeposit4",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:peatDeposit",
                    Name = "Peat deposit",
                    Location = new Vector3(3693f, 2420f, 0) //
                },

            });
            #endregion
            #region peat deposit 5
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startPeatDeposit5",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:peatDeposit",
                    Name = "Peat deposit",
                    Location = new Vector3(2789f, 2693f, 0) //
                },

            });
            #endregion
            #region peat deposit 6
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startPeatDeposit6",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:peatDeposit",
                    Name = "Peat deposit",
                    Location = new Vector3(5086f, 3547f, 0) //
                },

            });
            #endregion
            #region peat deposit 7
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startPeatDeposit7",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:peatDeposit",
                    Name = "Peat deposit",
                    Location = new Vector3(4100f, 4274f, 0) //
                },

            });
            #endregion
            #region peat deposit 8
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startPeatDeposit8",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:peatDeposit",
                    Name = "Peat deposit",
                    Location = new Vector3(2880f, 4343f, 0) //
                },

            });
            #endregion
            #region peat deposit 9
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startPeatDeposit9",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:peatDeposit",
                    Name = "Peat deposit",
                    Location = new Vector3(1440f, 4992f, 0) //
                },

            });
            #endregion
            #endregion

            #region salt deposits
            // from ost north to most south
            #region salt deposit 1
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startSaltDeposit1",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:saltDeposit",
                    Name = "Salt deposit",
                    Location = new Vector3(1578f, 2885f, 0) //very close to raremetal. not good. i removed it  mp
                },

            });
            #endregion
            #region salt deposit 2
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startSaltDeposit2",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:saltDeposit",
                    Name = "Salt deposit",
                    Location = new Vector3(3295f, 3503f, 0)
                },

            });
            #endregion
            #region salt deposit 3
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startSaltDeposit3",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:saltDeposit",
                    Name = "Salt deposit",
                    Location = new Vector3(5487f, 4707f, 0) 
                },

            });
            #endregion
            #region salt deposit 4
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startSaltDeposit4",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:saltDeposit",
                    Name = "Salt deposit",
                    Location = new Vector3(624f, 5040f, 0)
                },

            });
            #endregion
            #region salt deposit 5
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startSaltDeposit5",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:saltDeposit",
                    Name = "Salt deposit",
                    Location = new Vector3(4125f, 5146f, 0)
                },

            });
            #endregion
            #endregion

            #region clay deposits
            // from ost north to most south
            #region clay deposit 1
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startClayDeposit1",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:clayDeposit",
                    Name = "Clay deposit",
                    Location = new Vector3(3934f, 710f, 0) 
                },

            });
            #endregion
            #region clay deposit 2
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startClayDeposit2",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:clayDeposit",
                    Name = "Clay deposit",
                    Location = new Vector3(4810f, 2966f, 0) 
                },

            });
            #endregion
            #region clay deposit 3
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startClayDeposit3",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:clayDeposit",
                    Name = "Clay deposit",
                    Location = new Vector3(4171f, 3115f, 0) //north west
                },

            });
            #endregion
            #region clay deposit 4
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startClayDeposit4",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:clayDeposit",
                    Name = "Clay deposit",
                    Location = new Vector3(4318f, 3908f, 0) //at emerald river, mid 
                },

            });
            #endregion
            #region clay deposit 5
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startClayDeposit5",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:clayDeposit",
                    Name = "Clay deposit",
                    Location = new Vector3(1272f, 5139f, 0) //north west
                },

            });
            #endregion
            #endregion

            // NA: mock up for new material being added later. 
            // also rareMetal should be changed to the right name.
            #region RareMetalOreDeposit   Main objective Material
            #region North
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startRareMetalOreDepositNorth",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:rareMetalOreDeposit2",
                    Name = "RareMetal ore Deposit2",
                    Location = new Vector3(1440f, 288f, 0) // south east corner 
                },

            });
            #endregion
            #region Center
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startRareMetalOreDepositCenter",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:rareMetalOreDeposit1",
                    Name = "RareMetal ore Deposit1",
                    Location = new Vector3(1536f, 3120f, 0) 
                },

            });
            #endregion
            
            #endregion
            #endregion

         

            // Should handle this?
            #region Particles

            #region small fog
            list.Add(
              new ParticleEffectAction()
              {
                  KeyName = "smallFog1",     //Little ""steam" rising from pond      

                  Location = new ValueNode()
                  {
                      Location = new Microsoft.Xna.Framework.Vector2(672f, 826f)
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
                      Location = new Microsoft.Xna.Framework.Vector2(1200f, 1200f)
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
                      Location = new Microsoft.Xna.Framework.Vector2(1104f, 1440f)
                      //      Location = Maps.MapManager.TileToWorldPosVector2(new Point(19, 46))
                  },
                  ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "smallFog" } }

              });
            list.Add(
              new ParticleEffectAction()
              {
                  KeyName = "smallFog4",     //Little ""steam" rising from pond      

                  Location = new ValueNode()
                  {
                      Location = new Microsoft.Xna.Framework.Vector2(1968f, 1440f)
                      //                Location = Maps.MapManager.TileToWorldPosVector2(new Point(19, 46))
                  },
                  ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "smallFog" } }

              });
            list.Add(
              new ParticleEffectAction()
              {
                  KeyName = "smallFog5",     //Little ""steam" rising from pond      

                  Location = new ValueNode()
                  {
                      Location = new Microsoft.Xna.Framework.Vector2(2688f, 1152f)

                  },
                  ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "smallFog" } }

              });
            list.Add(
              new ParticleEffectAction()
              {
                  KeyName = "smallFog6",     //Little ""steam" rising from pond      

                  Location = new ValueNode()
                  {
                      Location = new Microsoft.Xna.Framework.Vector2(1872f, 864f)

                  },
                  ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "smallFog" } }

              });

            list.Add(
              new ParticleEffectAction()
              {
                  KeyName = "smallFog7",     //Little ""steam" rising from pond      

                  Location = new ValueNode()
                  {
                      Location = new Microsoft.Xna.Framework.Vector2(2688f, 1104f)

                  },
                  ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "smallFog" } }

              });

            list.Add(
              new ParticleEffectAction()
              {
                  KeyName = "smallFog8",     //Little ""steam" rising from pond      

                  Location = new ValueNode()
                  {
                      Location = new Microsoft.Xna.Framework.Vector2(3264f, 1872f)

                  },
                  ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "smallFog" } }

              });

            list.Add(
              new ParticleEffectAction()
              {
                  KeyName = "smallFog9",     //Little ""steam" rising from pond      

                  Location = new ValueNode()
                  {
                      Location = new Microsoft.Xna.Framework.Vector2(3744f, 1920f)

                  },
                  ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "smallFog" } }

              });

            list.Add(
              new ParticleEffectAction()
              {
                  KeyName = "smallFog10",     //Little ""steam" rising from pond      

                  Location = new ValueNode()
                  {
                      Location = new Microsoft.Xna.Framework.Vector2(3072f, 2640f)

                  },
                  ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "smallFog" } }

              });

            list.Add(
              new ParticleEffectAction()
              {
                  KeyName = "smallFog11",     //Little ""steam" rising from pond      

                  Location = new ValueNode()
                  {
                      Location = new Microsoft.Xna.Framework.Vector2(1824f, 2352f)

                  },
                  ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "smallFog" } }

              });

            list.Add(
              new ParticleEffectAction()
              {
                  KeyName = "smallFog12",     //Little ""steam" rising from pond      

                  Location = new ValueNode()
                  {
                      Location = new Microsoft.Xna.Framework.Vector2(3792f, 3936f)

                  },
                  ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "smallFog" } }

              });

            list.Add(
              new ParticleEffectAction()
              {
                  KeyName = "smallFog13",     //Little ""steam" rising from pond      

                  Location = new ValueNode()
                  {
                      Location = new Microsoft.Xna.Framework.Vector2(2352f, 4224f)

                  },
                  ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "smallFog" } }

              });

            list.Add(
              new ParticleEffectAction()
              {
                  KeyName = "smallFog14",     //Little ""steam" rising from pond      

                  Location = new ValueNode()
                  {
                      Location = new Microsoft.Xna.Framework.Vector2(2688f, 4944f)

                  },
                  ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "smallFog" } }

              });

            list.Add(
              new ParticleEffectAction()
              {
                  KeyName = "smallFog15",     //Little ""steam" rising from pond      

                  Location = new ValueNode()
                  {
                      Location = new Microsoft.Xna.Framework.Vector2(2976f, 5184f)

                  },
                  ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "smallFog" } }

              });
            #endregion
            #region swamp fog

            #region North East swamp
            list.Add(
              new ParticleEffectAction()
              {
                  KeyName = "fog1",     //smaller fog on swamp      

                  Scale = 5f,
                  Location = new ValueNode()
                  {
                      Location = Maps.MapManager.TileToWorldPosVector2(new Point(88, 21))

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
                      Location = Maps.MapManager.TileToWorldPosVector2(new Point(86, 29))

                  },
                  ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "fog" } }

              });



            list.Add(
              new ParticleEffectAction()
              {
                  KeyName = "fog3",     //Larger fog on swamp     

                  Scale = 7f,
                  Location = new ValueNode()
                  {
                      Location = Maps.MapManager.TileToWorldPosVector2(new Point(92, 21))

                  },
                  ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "fog" } }

              });


            list.Add(
             new ParticleEffectAction()
             {
                 KeyName = "fog4",     //Larger fog on swamp     

                 Scale = 7f,
                 Location = new ValueNode()
                 {
                     Location = Maps.MapManager.TileToWorldPosVector2(new Point(95, 14))

                 },
                 ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "fog" } }

             });

            list.Add(
              new ParticleEffectAction()
              {
                  KeyName = "fog5",     //smaller fog on swamp     

                  Scale = 5f,
                  Location = new ValueNode()
                  {
                      Location = Maps.MapManager.TileToWorldPosVector2(new Point(98, 20))

                  },
                  ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "fog" } }

              });

            list.Add(
              new ParticleEffectAction()
              {
                  KeyName = "fog6",     //smaller fog on swamp     

                  Scale = 5f,
                  Location = new ValueNode()
                  {
                      Location = Maps.MapManager.TileToWorldPosVector2(new Point(99, 10))

                  },
                  ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "fog" } }

              });

            list.Add(
              new ParticleEffectAction()
              {
                  KeyName = "fog7",     //smaller fog on swamp     

                  Scale = 5f,
                  Location = new ValueNode()
                  {
                      Location = Maps.MapManager.TileToWorldPosVector2(new Point(108, 10))

                  },
                  ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "fog" } }

              });

            list.Add(
              new ParticleEffectAction()
              {
                  KeyName = "fog8",     //smaller fog on swamp     

                  Scale = 5f,
                  Location = new ValueNode()
                  {
                      Location = Maps.MapManager.TileToWorldPosVector2(new Point(116, 7))

                  },
                  ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "fog" } }

              });

            list.Add(
              new ParticleEffectAction()
              {
                  KeyName = "fog9",     //smaller fog on swamp     

                  Scale = 5f,
                  Location = new ValueNode()
                  {
                      Location = Maps.MapManager.TileToWorldPosVector2(new Point(121, 8))

                  },
                  ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "fog" } }

              });

            list.Add(
              new ParticleEffectAction()
              {
                  KeyName = "fog10",     //smaller fog on swamp     

                  Scale = 5f,
                  Location = new ValueNode()
                  {
                      Location = Maps.MapManager.TileToWorldPosVector2(new Point(89, 28))

                  },
                  ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "fog" } }

              });

            #endregion




            #region south swamp
            list.Add(
              new ParticleEffectAction()
              {
                  KeyName = "fog11",     //smaller fog on swamp     

                  Scale = 5f,
                  Location = new ValueNode()
                  {
                      Location = Maps.MapManager.TileToWorldPosVector2(new Point(97, 76))

                  },
                  ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "fog" } }

              });

            list.Add(
              new ParticleEffectAction()
              {
                  KeyName = "fog12",     //smaller fog on swamp     

                  Scale = 5f,
                  Location = new ValueNode()
                  {
                      Location = Maps.MapManager.TileToWorldPosVector2(new Point(102, 86))

                  },
                  ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "fog" } }

              });

            list.Add(
              new ParticleEffectAction()
              {
                  KeyName = "fog13",     //smaller fog on swamp     

                  Scale = 5f,
                  Location = new ValueNode()
                  {
                      Location = Maps.MapManager.TileToWorldPosVector2(new Point(101, 98))

                  },
                  ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "fog" } }

              });

            list.Add(
              new ParticleEffectAction()
              {
                  KeyName = "fog14",     //smaller fog on swamp     

                  Scale = 5f,
                  Location = new ValueNode()
                  {
                      Location = Maps.MapManager.TileToWorldPosVector2(new Point(104, 104))

                  },
                  ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "fog" } }

              });
            #endregion
            #endregion

            #region sulfur

            list.Add(
                    new ParticleEffectAction()
                    {
                        KeyName = "sulphurousSmoke1",     //sulphurous lakes          

                        Scale = 2f,
                        TimeBetweenEmissions = 0.5f,
                        Location = new ValueNode()
                        {
                            Location = new Microsoft.Xna.Framework.Vector2(480f, 2976f)

                        },
                        ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "sulphurousSmoke" } }


                    });

            list.Add(
                    new ParticleEffectAction()
                    {
                        KeyName = "sulphurousSmoke2",     //sulphurous lakes          

                        Scale = 2f,
                        TimeBetweenEmissions = 0.5f,
                        Location = new ValueNode()
                        {
                            Location = new Microsoft.Xna.Framework.Vector2(528f, 3120f)

                        },
                        ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "sulphurousSmoke" } }


                    });

            list.Add(
                    new ParticleEffectAction()
                    {
                        KeyName = "sulphurousSmoke3",     //sulphurous lakes          

                        Scale = 3f,
                        TimeBetweenEmissions = 0.4f,
                        Location = new ValueNode()
                        {
                            Location = new Microsoft.Xna.Framework.Vector2(864f, 3024f)

                        },
                        ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "sulphurousSmoke" } }


                    });

            list.Add(
                    new ParticleEffectAction()
                    {
                        KeyName = "sulphurousSmoke4",     //sulphurous lakes          

                        Scale = 2.5f,
                        TimeBetweenEmissions = 0.4f,
                        Location = new ValueNode()
                        {
                            Location = new Microsoft.Xna.Framework.Vector2(1104f, 3168f)

                        },
                        ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "sulphurousSmoke" } }


                    });
            #endregion
            #region haze


            list.Add(
                      new ParticleEffectAction()
                      {
                          KeyName = "haze1",     // near sulfur           

                          Scale = 3f,
                          Location = new ValueNode()
                          {
                              Location = new Microsoft.Xna.Framework.Vector2(864f, 3168f)

                          },
                          ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "haze" } }
                          // The.Client.ParticleManager.AddEmitter("haze", MapManager.TileToWorldPosVector2(new Point(39, 27)), 4f, null); migrated from this....how to migrate size etc???? MP
                          // 1st argument: Tile position, 2nd argument: offset in pixels from center of tile. 3rd argument: Scale of clouds (null = use default) 4th argument: Time between puffs in seconds (null = use default)

                      });

            list.Add(
                      new ParticleEffectAction()
                      {
                          KeyName = "haze2",     // near sulfur           

                          Scale = 3f,
                          Location = new ValueNode()
                          {
                              Location = new Microsoft.Xna.Framework.Vector2(1104f, 3360f)

                          },
                          ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "haze" } }
 

                      });

            list.Add(
                      new ParticleEffectAction()
                      {
                          KeyName = "haze3",     // near sulfur           

                          Scale = 3f,
                          Location = new ValueNode()
                          {
                              Location = new Microsoft.Xna.Framework.Vector2(1872f, 3505f)

                          },
                          ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "haze" } }


                      });


            list.Add(
                      new ParticleEffectAction()
                      {
                          KeyName = "haze4",     // north          

                          Scale = 3f,
                          Location = new ValueNode()
                          {
                              Location = new Microsoft.Xna.Framework.Vector2(1056f, 432f)

                          },
                          ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "haze" } }


                      });

            #endregion



            #endregion






            return list;
        }
    }
}
