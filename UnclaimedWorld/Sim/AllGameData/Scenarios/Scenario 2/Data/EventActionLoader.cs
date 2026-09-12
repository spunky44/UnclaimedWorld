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
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Systems;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_2.Data
{
    public class EventActionLoader
    {
        public static List<EventActionType> Init()
        {
            List<EventActionType> list = new List<EventActionType>();


            double delayForStructures = 0.25;
            double delayForItemsAndAgents = 0.5;

            #region Init. // burials, meetings, emigration texts

          
            /*
               PropertyResult? burialTextStart = The.Sim.Site.GetPropertyValue("burialTextStart");  // "Date: 03-10 2238 \nLocation: 4° 12' 22'' South, 7° 12' 19.2'' West \n \nJournal entry #2 \n{0} \n \n";
            // these are optional...
            PropertyResult? multipleDeathsMultipleSurvivors = The.Sim.Site.GetPropertyValue("burialTextMultipleDeathsMultipleSurvivors"); // "We have lost #NAMEOFDECEASED. At the burial a eulogy was delivered by #EUOLOGYGIVER. The speech is included as an audio file. \n \nGoodbye, #NAMEOFDECEASED. You will be remembered as a shining example for future generations of settlers. Rest in peace."  
            PropertyResult? singleDeathMultipleSurvivors = The.Sim.Site.GetPropertyValue("burialTextSingleDeathMultipleSurvivors");  // "We have lost #NAMEOFDECEASED#CAUSEOFDEATH. At the burial a eulogy was delivered by #EUOLOGYGIVER. The speech is included as an audio file. \n \nGoodbye, #NAMEOFDECEASED. You will be remembered as a shining example for future generations of settlers. Rest in peace."                      
            PropertyResult? singleDeathSingleSurvivor = The.Sim.Site.GetPropertyValue("burialTextSingleDeathSingleSurvivor"); // "#NAMEOFDECEASED is also dead now. \nI buried the remains as best I could. Guess it's only me now..."
            PropertyResult? multipleDeathsSingleSurvivor = The.Sim.Site.GetPropertyValue("burialTextMultipleDeathsSingleSurvivor"); // "#NAMEOFDECEASED is also dead now. \nI buried the remains as best I could. Guess it's only me now..."
                      
             */

            list.Add(new SetPropertyAction()
            {
                KeyName = "initIncludeDateInBurial",
                PropertyKey = "includeDateInBurialHeader",
                Value = new ValueNode() { Bool = true }
            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "initBurialText2",

                PropertyKey = "burialTextMultipleDeathsMultipleSurvivors",
                Value = new ValueNode() { String = "We have lost #NAMEOFDECEASED. An evacuation aircraft has arrived to carry their remains back to Duke's Landing. \n \nGoodbye, #NAMEOFDECEASED. You will be remembered as a shining example for future generations of settlers." }
                // "We have lost #NAMEOFDECEASED. At the burial a eulogy was delivered by #EUOLOGYGIVER. The speech is included as an audio file. \n \nGoodbye, #NAMEOFDECEASED. You will be remembered as a shining example for future generations of settlers. Rest in peace." } } }
            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "initBurialText3",
                PropertyKey = "burialTextSingleDeathMultipleSurvivors",
                Value = new ValueNode() { String = "We have lost #NAMEOFDECEASED#CAUSEOFDEATH. An evacuation aircraft has arrived to carry the remains back to Duke's Landing. \n \nGoodbye, #NAMEOFDECEASED. You will be remembered as a shining example for future generations of settlers." }
                // "We have lost #NAMEOFDECEASED#CAUSEOFDEATH. At the burial a eulogy was delivered by #EUOLOGYGIVER. The speech is included as an audio file. \n \nGoodbye, #NAMEOFDECEASED. You will be remembered as a shining example for future generations of settlers. Rest in peace."
            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "initBurialText4", // TODO - sole survivor
                PropertyKey = "burialTextSingleDeathSingleSurvivor",
                Value = new ValueNode() { String = "We have lost #NAMEOFDECEASED#CAUSEOFDEATH. An evacuation aircraft has arrived to carry the remains back to Duke's Landing. \n \nGoodbye, #NAMEOFDECEASED. You will be remembered as a shining example for future generations of settlers." } //mp no need to use this: "#NAMEOFDECEASED is also dead now. \nI buried the remains as best I could. Guess it's only me now..."
            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "initBurialText5", // TODO - sole survivor
                PropertyKey = "burialTextMultipleDeathsSingleSurvivor",
                Value = new ValueNode() { String = "We have lost #NAMEOFDECEASED#CAUSEOFDEATH. An evacuation aircraft has arrived to carry the remains back to Duke's Landing. \n \nGoodbye, #NAMEOFDECEASED. You will be remembered as a shining example for future generations of settlers." }
            });
            //****************

            list.Add(new SetPropertyAction()
            {
                KeyName = "initGameOver",
                PropertyKey = "gameOver",
                Value = new ValueNode() { Bool = false }
            });

            #region Group meeting dialog texts
            // Muckroot station...
            list.Add(new SetPropertyAction()
            {
                KeyName = "meeting3Security",

                PropertyKey = "meeting3Security",
                Value = new ValueNode()
                {
                    String = "#UNHAPPY: Thanks for taking time out of your day... I want to talk about the security situation. The way it's handled, I don't think we're safe here. \n \n"
                                + "#CONTENT: What do you want done? \n \n"
                                + "#UNHAPPY: I want us to beef up security. How we do it? Stock more and better weapons, take fewer risks... There's a number of ways we can protect our colony better. Main thing is that we take action now. \n \n"
                                + "#CONTENT: This is all a question of priorities... \n \n"
                                + "#UNHAPPY: Exactly. And if we value our lives, we need better protection from wild animals. So I hope you're with me. #EMIGRATETHREAT \n \n"
                                + "#CONTENT: Alright. This has been noted. Anything else?"
                }

            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "meetingEmigrateThreat",
                PropertyKey = "meetingEmigrateThreat",
                Value = new ValueNode() { String = "If not...well, I might leave for #EMIGRATETO." }
            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "meetingEmigrateThreatAllUnhappy",
                PropertyKey = "meetingEmigrateThreatAllUnhappy",
                Value = new ValueNode() { String = "Seriously, I'm sometimes wondering whether it would be better to go to #EMIGRATETO and start over." }
            });

            list.Add(new SetPropertyAction() // placeholder text
            {
                KeyName = "meeting3Food",

                PropertyKey = "meeting3Food",
                Value = new ValueNode()
                {
                    String = "#UNHAPPY: Thanks for taking time to listen. We need to improve the food situation. \n \n"
                                + "#CONTENT: In what respect? \n \n"
                                + "#UNHAPPY: As an insurance against unforeseen events - we need larger food stores. \n \n"
                                + "#CONTENT: You are worried about running out of food? \n \n"
                                + "#UNHAPPY: Having this little food stockpiled, in my eyes, is very risky. So I hope you will act. #EMIGRATETHREAT \n \n"
                                + "#CONTENT: Alright. This has been noted. Anything else?"
                }

            });

            list.Add(new SetPropertyAction() // placeholder text
            {
                KeyName = "meeting3Comfort",

                PropertyKey = "meeting3Comfort",
                Value = new ValueNode()
                {
                    String = "#UNHAPPY: Living conditions here need to be improved. \n \n"
                                + "#CONTENT: You're not happy with the standard we have here? \n \n"
                                + "#UNHAPPY: No. Living under these conditions has a negative effect on physical abilities, not to mention the risk of conflict within this work community. \n \n"
                                + "#CONTENT: You do realize that we have other concerns as well... \n \n"
                                + "#UNHAPPY: In my eyes, this is vital: We need better housing and we need to increase our well-being. #EMIGRATETHREAT \n \n"
                                + "#CONTENT: Alright - this has been duly noted. Who has anything to add?"
                }

            });

            #endregion

            #region Emigrate dialog texts

            // final:
            list.Add(new SetPropertyAction()
            {
                KeyName = "initEmigrateSecurityDialogText",
                PropertyKey = "securityEmigrateEventDialogText",
                Value = new ValueNode() { String = "AUDIO LOG, #JOURNALDATE \n \n#NAME1: Security around here is appalling. We're in the middle of a dangerous wilderness and I can't believe the risks that we're taking. It's come to the point that I'm afraid to sleep. \n \n#NAME2: Come on. Of course there are wild animals out there but nothing we can't deal with. \n \n#NAME1: When an animal attack happens - AND IT WILL! I'm not going to be around. I'm leaving now. You can find me at #EMIGRATIONTARGET if you want to get in touch." }
            });

            // placeholders:
            list.Add(new SetPropertyAction()
            {
                KeyName = "initEmigrateSecurityNoConversationDialogText",
                PropertyKey = "securityEmigrateEventNoConversationDialogText",
                Value = new ValueNode() { String = "TEXT LOG, #JOURNALDATE \n \n#NAME1: When you read this, I'll be on my way. The security here is appalling. This place scares me. But it scares me even more that you seem so cavalier about the threats we're facing. I'm leaving for #EMIGRATIONTARGET. \nGoodbye." }
            });


            list.Add(new SetPropertyAction()
            {
                KeyName = "initEmigrateComfortDialogText",
                PropertyKey = "comfortEmigrateEventDialogText",
                Value = new ValueNode() { String = "AUDIO LOG, #JOURNALDATE \n \n#NAME1: It's clear now that my concerns about the living standards here have fallen on deaf ears. \n \n#NAME2: We've had to postpone those efforts in favor of other areas. \n \n#NAME1: Well, you won't be hearing my complaints any longer. I'm leaving for #EMIGRATIONTARGET. I can do better on my own." }
            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "initEmigrateComfortNoConversationDialogText",
                PropertyKey = "comfortEmigrateEventNoConversationDialogText",
                Value = new ValueNode() { String = "TEXT LOG, #JOURNALDATE \n \n#NAME1: This is a note to let you know that I'm leaving the camp and will go to #EMIGRATIONTARGET. As I've repeatedly stated, the living conditions here are unacceptable and since there's no sign of improvement you leave me no choice." }
            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "initEmigrateFoodDialogText",
                PropertyKey = "foodEmigrateEventDialogText",
                Value = new ValueNode() { String = "AUDIO LOG, #JOURNALDATE \n \n#NAME1: I'm very disappointed that my concerns about food have been ignored. We are heading for disaster, but I will not be around when that happens. \n \n#NAME2: With all respect - you are overreacting. \n#NAME1: You can run this place your way now. I'm out. I'm going to #EMIGRATIONTARGET." }  //post screen banter
            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "initEmigrateFoodNoConversationDialogText",
                PropertyKey = "foodEmigrateEventNoConversationDialogText",
                Value = new ValueNode() { String = "TEXT LOG, #JOURNALDATE \n \n#NAME1: When you read this, I'll be on my way. My warnings regarding the food situation have been ignored for too long and so I'm leaving. I'm going to #EMIGRATIONTARGET." }
            });

            #endregion

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

                        ViewLongitudeStart = -5, // TODO...
                        ViewLongitudeEnd = 35,
                        ViewLatitudeStart = 40,
                        ViewLatitudeEnd = 85
                    }

                });

            #endregion

            //Muckroot station Playsite:
            #region spawnPlaySite
            list.Add(
                new SpawnSiteAction()
                {
                    KeyName = "spawnPlaySite",

                    SiteData = new SiteData()
                    {
                        Name = "Zone J-23", // 
                        KeyName = "playSite",
                        Coords = new Overland.Locations.GeodeticCoordinate(10.24655d, 55.83451d), // { Latitude = 63.83451d, Longitude = 20.24655d },
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
                        Name = "Muckroot Research Station", //"Player Allegiance", //was "Muckroot Science Station"
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
                        Decimal = 600
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
                        Name = "Duke's Landing",
                        KeyName = "otherSite1",
                        Coords = new Overland.Locations.GeodeticCoordinate(8.24655d, 69.83451d), // { Latitude = 69.83451d, Longitude = 14.24655d },                           
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
                        Name = "North Base",
                        KeyName = "otherSite1Allegiance1",
                        EntityType = "entity:human",
                        PermitsImmigration = true,
                        AllegianceType = Allegiances.AllegianceType.Other,
                        StatsData = new StatsData()
                        {
                            Security = .85f,
                            Comfort = .69f,
                            FoodSupply = .59f,
                        }
                    }

                });
            #endregion
            #region spawnOtherSite1Expedition1 , GOODS prices (Buying), Vehicles for hire
            list.Add(new CreateExpeditionAction()
            {
                KeyName = "spawnOtherSite1Expedition1",// not player's site.
                DelayInSeconds = 0.1, // wait for starting location property to have been set!

                ExpeditionData = new ExpeditionData()
                {
                    KeyName = "otherSite1Expedition1",//not player's site.
                    Name = "Helipad A", // was Central as in Town Central...
                    AllegianceKey = "otherSite1Allegiance1",
                      #region GOODS prices. (Buying)
                        AvailableForTrade = new SerializableDictionary<string, Trade.TradeAmountType>() //
                            {       
                                    //Price is for buying AT THIS SITE.  //NOTE: ONLY PRICE = .... IS USED CURRENTLY!!
                                    //define WHAT ITEMS are AVAILABLE to BUY with the spawn events SpawnItemOtherSite below

                              //      StartAmount = new NormalDistribution() { Mean = 12, StandardDeviation = 2 }, //currently not used. 
                              //      MaxAmount = new NormalDistribution() { Mean = 20, StandardDeviation = 2 },//currently not used. 
                              //      IncreasePerDay = new NormalDistribution() { Mean = 4, StandardDeviation = 0 }}//currently not used.
 
                           
                             #region FOOD 

                       
                                { "item:astroRation", new TradeAmountType(){SpecificSellPrice = new NormalDistribution() { Mean = 2d, StandardDeviation = 0.02d }, 
                                    //StartAmount = new NormalDistribution() { Mean = 12, StandardDeviation = 2 },/ MaxAmount = new NormalDistribution() { Mean = 20, StandardDeviation = 2 },IncreasePerDay = new NormalDistribution() { Mean = 4, StandardDeviation = 0 }
                                }},       
                                { "item:simCoffeeBeans", new TradeAmountType(){SpecificSellPrice = new NormalDistribution() { Mean = 1d, StandardDeviation = 0.02d }, 
                                    //StartAmount = new NormalDistribution() { Mean = 12, StandardDeviation = 2 },/ MaxAmount = new NormalDistribution() { Mean = 20, StandardDeviation = 2 },IncreasePerDay = new NormalDistribution() { Mean = 4, StandardDeviation = 0 }
                                }},

#endregion
                             #region WEAPONS

                                { "item:sentry", new TradeAmountType(){SpecificSellPrice = new NormalDistribution() { Mean = 50d, StandardDeviation = 0.02d }, 
                                    //StartAmount = new NormalDistribution() { Mean = 12, StandardDeviation = 2 },/ MaxAmount = new NormalDistribution() { Mean = 20, StandardDeviation = 2 },IncreasePerDay = new NormalDistribution() { Mean = 4, StandardDeviation = 0 }
                                }},
                                { "item:coilRifle", new TradeAmountType(){SpecificSellPrice = new NormalDistribution() { Mean = 30d, StandardDeviation = 0.02d }, 
                                    //StartAmount = new NormalDistribution() { Mean = 12, StandardDeviation = 2 },/ MaxAmount = new NormalDistribution() { Mean = 20, StandardDeviation = 2 },IncreasePerDay = new NormalDistribution() { Mean = 4, StandardDeviation = 0 }
                                }},

#endregion
                             #region RAW MATERIALS
                         



                                { "item:inactivatedFoodCoolerUnit", new TradeAmountType(){SpecificSellPrice = new NormalDistribution() { Mean = 10d, StandardDeviation = 0.02d }, 
                                    //StartAmount = new NormalDistribution() { Mean = 12, StandardDeviation = 2 },/ MaxAmount = new NormalDistribution() { Mean = 20, StandardDeviation = 2 },IncreasePerDay = new NormalDistribution() { Mean = 4, StandardDeviation = 0 }
                                }},

                                { "item:textile", new TradeAmountType(){SpecificSellPrice = new NormalDistribution() { Mean = 2d, StandardDeviation = 0.02d }, 
                                    //StartAmount = new NormalDistribution() { Mean = 12, StandardDeviation = 2 },/ MaxAmount = new NormalDistribution() { Mean = 20, StandardDeviation = 2 },IncreasePerDay = new NormalDistribution() { Mean = 4, StandardDeviation = 0 }
                                }},

                                { "item:smallTent", new TradeAmountType(){SpecificSellPrice = new NormalDistribution() { Mean = 7d, StandardDeviation = 0.02d }, 
                                    //StartAmount = new NormalDistribution() { Mean = 12, StandardDeviation = 2 },/ MaxAmount = new NormalDistribution() { Mean = 20, StandardDeviation = 2 },IncreasePerDay = new NormalDistribution() { Mean = 4, StandardDeviation = 0 }
                                }},
                                { "item:octagonalTent", new TradeAmountType(){SpecificSellPrice = new NormalDistribution() { Mean = 8d, StandardDeviation = 0.02d }, 
                                    //StartAmount = new NormalDistribution() { Mean = 12, StandardDeviation = 2 },/ MaxAmount = new NormalDistribution() { Mean = 20, StandardDeviation = 2 },IncreasePerDay = new NormalDistribution() { Mean = 4, StandardDeviation = 0 }
                                }},

                                { "item:domeTent", new TradeAmountType(){SpecificSellPrice = new NormalDistribution() { Mean = 8d, StandardDeviation = 0.02d }, 
                                    //StartAmount = new NormalDistribution() { Mean = 12, StandardDeviation = 2 },/ MaxAmount = new NormalDistribution() { Mean = 20, StandardDeviation = 2 },IncreasePerDay = new NormalDistribution() { Mean = 4, StandardDeviation = 0 }
                                }},                                
                                { "item:thermalTarp", new TradeAmountType(){SpecificSellPrice = new NormalDistribution() { Mean = 4d, StandardDeviation = 0.02d }, 
                                    //StartAmount = new NormalDistribution() { Mean = 12, StandardDeviation = 2 },/ MaxAmount = new NormalDistribution() { Mean = 20, StandardDeviation = 2 },IncreasePerDay = new NormalDistribution() { Mean = 4, StandardDeviation = 0 }
                                }},
                            


                #endregion
                             #region AMMUNITION
                                { "item:sentryGunAmmo", new TradeAmountType(){SpecificSellPrice = new NormalDistribution() { Mean = 15d, StandardDeviation = 0.02d }, 
                                    //StartAmount = new NormalDistribution() { Mean = 12, StandardDeviation = 2 },/ MaxAmount = new NormalDistribution() { Mean = 20, StandardDeviation = 2 },IncreasePerDay = new NormalDistribution() { Mean = 4, StandardDeviation = 0 }
                                }},
                              
                                { "item:coilRifleAmmo", new TradeAmountType(){SpecificSellPrice = new NormalDistribution() { Mean = 10d, StandardDeviation = 0.02d }, 
                                    //StartAmount = new NormalDistribution() { Mean = 12, StandardDeviation = 2 },/ MaxAmount = new NormalDistribution() { Mean = 20, StandardDeviation = 2 },IncreasePerDay = new NormalDistribution() { Mean = 4, StandardDeviation = 0 }
                                }},

#endregion
                             #region       BODIES none                                
                            
                            #endregion
                             #region TOOLS   


                                { "item:advancedKnife", new TradeAmountType(){ SpecificSellPrice = new NormalDistribution() { Mean = 3d, StandardDeviation = 0.02d }, 
                                    //StartAmount = new NormalDistribution() { Mean = 12, StandardDeviation = 2 },/ MaxAmount = new NormalDistribution() { Mean = 20, StandardDeviation = 2 },IncreasePerDay = new NormalDistribution() { Mean = 4, StandardDeviation = 0 }
                                }},

                                { "item:advancedMachete", new TradeAmountType(){ SpecificSellPrice = new NormalDistribution() { Mean = 4d, StandardDeviation = 0.02d }, 
                                    //StartAmount = new NormalDistribution() { Mean = 12, StandardDeviation = 2 },/ MaxAmount = new NormalDistribution() { Mean = 20, StandardDeviation = 2 },IncreasePerDay = new NormalDistribution() { Mean = 4, StandardDeviation = 0 }
                                }},

//
                                { "item:steelPickaxe", new TradeAmountType(){ SpecificSellPrice = new NormalDistribution() { Mean = 2d, StandardDeviation = 0.02d }, 
                                    //StartAmount = new NormalDistribution() { Mean = 12, StandardDeviation = 2 },/ MaxAmount = new NormalDistribution() { Mean = 20, StandardDeviation = 2 },IncreasePerDay = new NormalDistribution() { Mean = 4, StandardDeviation = 0 }
                                }},
                                { "item:hammer", new TradeAmountType(){ SpecificSellPrice = new NormalDistribution() { Mean = 2d, StandardDeviation = 0.02d }, 
                                    //StartAmount = new NormalDistribution() { Mean = 12, StandardDeviation = 2 },/ MaxAmount = new NormalDistribution() { Mean = 20, StandardDeviation = 2 },IncreasePerDay = new NormalDistribution() { Mean = 4, StandardDeviation = 0 }
                                }},

                                { "item:metalWorkersToolbox", new TradeAmountType(){ SpecificSellPrice = new NormalDistribution() { Mean = 11d, StandardDeviation = 0.02d }, 
                                    //StartAmount = new NormalDistribution() { Mean = 12, StandardDeviation = 2 },/ MaxAmount = new NormalDistribution() { Mean = 20, StandardDeviation = 2 },IncreasePerDay = new NormalDistribution() { Mean = 4, StandardDeviation = 0 }
                                }},

                                { "item:steelSpade", new TradeAmountType(){ SpecificSellPrice = new NormalDistribution() { Mean = 4d, StandardDeviation = 0.02d }, 
                                    //StartAmount = new NormalDistribution() { Mean = 12, StandardDeviation = 2 },/ MaxAmount = new NormalDistribution() { Mean = 20, StandardDeviation = 2 },IncreasePerDay = new NormalDistribution() { Mean = 4, StandardDeviation = 0 }
                                }},

                                { "item:advancedString", new TradeAmountType(){ SpecificSellPrice = new NormalDistribution() { Mean = 1d, StandardDeviation = 0.02d }, 
                                    //StartAmount = new NormalDistribution() { Mean = 12, StandardDeviation = 2 },/ MaxAmount = new NormalDistribution() { Mean = 20, StandardDeviation = 2 },IncreasePerDay = new NormalDistribution() { Mean = 4, StandardDeviation = 0 }
                                }},
                                { "item:metalWire", new TradeAmountType(){ SpecificSellPrice = new NormalDistribution() { Mean = 1d, StandardDeviation = 0.02d }, 
                                    //StartAmount = new NormalDistribution() { Mean = 12, StandardDeviation = 2 },/ MaxAmount = new NormalDistribution() { Mean = 20, StandardDeviation = 2 },IncreasePerDay = new NormalDistribution() { Mean = 4, StandardDeviation = 0 }
                                }},
//

                                { "item:advancedSnips", new TradeAmountType(){ SpecificSellPrice = new NormalDistribution() { Mean = 2d, StandardDeviation = 0.02d }, 
                                    //StartAmount = new NormalDistribution() { Mean = 12, StandardDeviation = 2 },/ MaxAmount = new NormalDistribution() { Mean = 20, StandardDeviation = 2 },IncreasePerDay = new NormalDistribution() { Mean = 4, StandardDeviation = 0 }
                                }},
//
                                { "item:advancedCookingPot", new TradeAmountType(){ SpecificSellPrice = new NormalDistribution() { Mean = 2d, StandardDeviation = 0.02d }, 
                                    //StartAmount = new NormalDistribution() { Mean = 12, StandardDeviation = 2 },/ MaxAmount = new NormalDistribution() { Mean = 20, StandardDeviation = 2 },IncreasePerDay = new NormalDistribution() { Mean = 4, StandardDeviation = 0 }
                                }},

#endregion
                             #region STRUCTURE ITEMS and PARTS

                                { "item:sensor", new TradeAmountType(){ SpecificSellPrice = new NormalDistribution() { Mean = 6d, StandardDeviation = 0.02d }, 
                                    //StartAmount = new NormalDistribution() { Mean = 12, StandardDeviation = 2 },/ MaxAmount = new NormalDistribution() { Mean = 20, StandardDeviation = 2 },IncreasePerDay = new NormalDistribution() { Mean = 4, StandardDeviation = 0 }
                                }},


#endregion

                            },
                        #endregion
                   
                     #region Vehicles for hire
                    VehiclesForHire = new SerializableDictionary<string, VehiclesForHireType>()
                        {
                            {
                                "entity:skimmer", new VehiclesForHireType()
                                {
                                        SpecificPrice = new NormalDistribution() { Mean = 300, StandardDeviation = 0d }, 
                                        StartAmount = 1 
                                }
                            },
                            {
                                "entity:harpy", new VehiclesForHireType()
                                {
                                        SpecificPrice = new NormalDistribution() { Mean = 500, StandardDeviation = 0d }, 
                                        StartAmount = 1
                                }
                            }
                        }
                    #endregion
                }

            });
            #endregion
            #region OtherSite1 GOODS available (Buying) (amounts defined in scenarioloader)
            // spawn whatever goods that the player should be able to BUY! (if copy-pasitng from other doc, make sure to replace: container:"xxx")

            #region FOOD
            list.Add(Scenarios.ScenarioLoader.SpawnItemOtherSite("otherSite1Expedition1_precolRation", "item:astroRation", "otherSite1Allegiance1", "otherSite1Expedition1", delayForItemsAndAgents, site: "otherSite1", container: "helipad", offerForSale: true));
            list.Add(Scenarios.ScenarioLoader.SpawnItemOtherSite("otherSite1Expedition1_simCoffeeBeans", "item:simCoffeeBeans", "otherSite1Allegiance1", "otherSite1Expedition1", delayForItemsAndAgents, site: "otherSite1", container: "helipad", offerForSale: true));
            #endregion
            #region WEAPONS
            list.Add(Scenarios.ScenarioLoader.SpawnItemOtherSite("otherSite1Expedition1_sentry", "item:sentry", "otherSite1Allegiance1", "otherSite1Expedition1", delayForItemsAndAgents, site: "otherSite1", container: "helipad", offerForSale: true));
            list.Add(Scenarios.ScenarioLoader.SpawnItemOtherSite("otherSite1Expedition1_coilRifle", "item:coilRifle", "otherSite1Allegiance1", "otherSite1Expedition1", delayForItemsAndAgents, site: "otherSite1", container: "helipad", offerForSale: true));
            #endregion

            #region RAW MATERIALS


            list.Add(Scenarios.ScenarioLoader.SpawnItemOtherSite("otherSite1Expedition1_acetylene", "item:acetylene", "otherSite1Allegiance1", "otherSite1Expedition1", delayForItemsAndAgents, site: "otherSite1", container: "helipad", offerForSale: true));
            list.Add(Scenarios.ScenarioLoader.SpawnItemOtherSite("otherSite1Expedition1_liquidGas", "item:liquidGas", "otherSite1Allegiance1", "otherSite1Expedition1", delayForItemsAndAgents, site: "otherSite1", container: "helipad", offerForSale: true));
            list.Add(Scenarios.ScenarioLoader.SpawnItemOtherSite("otherSite1Expedition1_ironCanister", "item:ironCanister", "otherSite1Allegiance1", "otherSite1Expedition1", delayForItemsAndAgents, site: "otherSite1", container: "helipad", offerForSale: true));
            #endregion

            #region AMMUNITION
            list.Add(Scenarios.ScenarioLoader.SpawnItemOtherSite("otherSite1Expedition1_sentryGunAmmo", "item:sentryGunAmmo", "otherSite1Allegiance1", "otherSite1Expedition1", delayForItemsAndAgents, site: "otherSite1", container: "helipad", offerForSale: true));
            list.Add(Scenarios.ScenarioLoader.SpawnItemOtherSite("otherSite1Expedition1_coilRifleAmmo", "item:coilRifleAmmo", "otherSite1Allegiance1", "otherSite1Expedition1", delayForItemsAndAgents, site: "otherSite1", container: "helipad", offerForSale: true));

            #endregion

            #region TOOLS
            list.Add(Scenarios.ScenarioLoader.SpawnItemOtherSite("otherSite1Expedition1_hammer", "item:hammer", "otherSite1Allegiance1", "otherSite1Expedition1", delayForItemsAndAgents, site: "otherSite1", container: "helipad", offerForSale: true));

            list.Add(Scenarios.ScenarioLoader.SpawnItemOtherSite("otherSite1Expedition1_knife", "item:advancedKnife", "otherSite1Allegiance1", "otherSite1Expedition1", delayForItemsAndAgents, site: "otherSite1", container: "helipad", offerForSale: true));
            list.Add(Scenarios.ScenarioLoader.SpawnItemOtherSite("otherSite1Expedition1_metalworkersToolbox", "item:metalWorkersToolbox", "otherSite1Allegiance1", "otherSite1Expedition1", delayForItemsAndAgents, site: "otherSite1", container: "helipad", offerForSale: true));
            list.Add(Scenarios.ScenarioLoader.SpawnItemOtherSite("otherSite1Expedition1_BlacksmithsToolbox", "item:blacksmithsToolbox", "otherSite1Allegiance1", "otherSite1Expedition1", delayForItemsAndAgents, site: "otherSite1", container: "helipad", offerForSale: true));
            list.Add(Scenarios.ScenarioLoader.SpawnItemOtherSite("otherSite1Expedition1_metalWire", "item:metalWire", "otherSite1Allegiance1", "otherSite1Expedition1", delayForItemsAndAgents, site: "otherSite1", container: "helipad", offerForSale: true));

            list.Add(Scenarios.ScenarioLoader.SpawnItemOtherSite("otherSite1Expedition1_machete", "item:advancedMachete", "otherSite1Allegiance1", "otherSite1Expedition1", delayForItemsAndAgents, site: "otherSite1", container: "helipad", offerForSale: true));
            list.Add(Scenarios.ScenarioLoader.SpawnItemOtherSite("otherSite1Expedition1_steelHandAxe", "item:steelHandAxe", "otherSite1Allegiance1", "otherSite1Expedition1", delayForItemsAndAgents, site: "otherSite1", container: "helipad", offerForSale: true));
            list.Add(Scenarios.ScenarioLoader.SpawnItemOtherSite("otherSite1Expedition1_steelPickaxe", "item:steelPickaxe", "otherSite1Allegiance1", "otherSite1Expedition1", delayForItemsAndAgents, site: "otherSite1", container: "helipad", offerForSale: true));

            list.Add(Scenarios.ScenarioLoader.SpawnItemOtherSite("otherSite1Expedition1_inactivatedFoodCoolerUnit", "item:inactivatedFoodCoolerUnit", "otherSite1Allegiance1", "otherSite1Expedition1", delayForItemsAndAgents, site: "otherSite1", container: "helipad", offerForSale: true));
            list.Add(Scenarios.ScenarioLoader.SpawnItemOtherSite("otherSite1Expedition1_textile", "item:textile", "otherSite1Allegiance1", "otherSite1Expedition1", delayForItemsAndAgents, site: "otherSite1", container: "helipad", offerForSale: true));
            list.Add(Scenarios.ScenarioLoader.SpawnItemOtherSite("otherSite1Expedition1_smallTent", "item:smallTent", "otherSite1Allegiance1", "otherSite1Expedition1", delayForItemsAndAgents, site: "otherSite1", container: "helipad", offerForSale: true));
            list.Add(Scenarios.ScenarioLoader.SpawnItemOtherSite("otherSite1Expedition1_octagonalTent", "item:octagonalTent", "otherSite1Allegiance1", "otherSite1Expedition1", delayForItemsAndAgents, site: "otherSite1", container: "helipad", offerForSale: true));
            list.Add(Scenarios.ScenarioLoader.SpawnItemOtherSite("otherSite1Expedition1_domeTent", "item:domeTent", "otherSite1Allegiance1", "otherSite1Expedition1", delayForItemsAndAgents, site: "otherSite1", container: "helipad", offerForSale: true));
            list.Add(Scenarios.ScenarioLoader.SpawnItemOtherSite("otherSite1Expedition1_thermalTarp", "item:thermalTarp", "otherSite1Allegiance1", "otherSite1Expedition1", delayForItemsAndAgents, site: "otherSite1", container: "helipad", offerForSale: true));
            list.Add(Scenarios.ScenarioLoader.SpawnItemOtherSite("otherSite1Expedition1_steelSpade", "item:steelSpade", "otherSite1Allegiance1", "otherSite1Expedition1", delayForItemsAndAgents, site: "otherSite1", container: "helipad", offerForSale: true));
            list.Add(Scenarios.ScenarioLoader.SpawnItemOtherSite("otherSite1Expedition1_string", "item:advancedString", "otherSite1Allegiance1", "otherSite1Expedition1", delayForItemsAndAgents, site: "otherSite1", container: "helipad", offerForSale: true));

            list.Add(Scenarios.ScenarioLoader.SpawnItemOtherSite("otherSite1Expedition1_snips", "item:advancedSnips", "otherSite1Allegiance1", "otherSite1Expedition1", delayForItemsAndAgents, site: "otherSite1", container: "helipad", offerForSale: true));
            list.Add(Scenarios.ScenarioLoader.SpawnItemOtherSite("otherSite1Expedition1_cookingPot", "item:advancedCookingPot", "otherSite1Allegiance1", "otherSite1Expedition1", delayForItemsAndAgents, site: "otherSite1", container: "helipad", offerForSale: true));
            #endregion
            #region STRUCTURE ITEMS and PARTS
            list.Add(Scenarios.ScenarioLoader.SpawnItemOtherSite("otherSite1Expedition1_sensor", "item:sensor", "otherSite1Allegiance1", "otherSite1Expedition1", delayForItemsAndAgents, site: "otherSite1", container: "helipad", offerForSale: true));

            #endregion
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
            #region OtherSite1 vehicles and structures

            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnOtherSite1Expedition1SatelliteGroundStation",
                DelayInSeconds = delayForStructures,

                EntityData = new Maps.MapEditor.EntityData()
                {
                    EntityKey = "structure:satelliteGroundStation",
                    OwnedBy = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "otherSite1Allegiance1", 
                        ExpeditionKey = "otherSite1Expedition1" 
                    }

                }

            });


            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnOtherSite1Expedition1Helipad",
                DelayInSeconds = delayForStructures,

                EntityData = new Maps.MapEditor.EntityData()
                {
                    EntityKey = "structure:heliportLarge", //large capacity!!!large enough??
                    Name = "helipad",
                    OwnedBy = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "otherSite1Allegiance1",
                        ExpeditionKey = "otherSite1Expedition1"
                    }

                }

            });

            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnOtherSite1Expedition1Skimmer",
                DelayInSeconds = delayForItemsAndAgents,

                EntityData = new Maps.MapEditor.EntityData()
                {
                    EntityKey = "entity:skimmer",
                    OwnedBy = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "otherSite1Allegiance1",
                        ExpeditionKey = "otherSite1Expedition1"
                    }

                }

            });

            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnOtherSite1Expedition1Harpy",
                DelayInSeconds = delayForItemsAndAgents,

                EntityData = new Maps.MapEditor.EntityData()
                {
                    EntityKey = "entity:harpy",


                    OwnedBy = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "otherSite1Allegiance1",
                        ExpeditionKey = "otherSite1Expedition1"
                    }

                }

            });

            ////////////////





            #endregion
            #region OtherSite1 Possible Immigrants
            #region spawnMuckrootRandomImmigrant
            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnMuckrootRandomImmigrant",
                DelayInSeconds = delayForItemsAndAgents,

                /* DynamicLocation = new DynamicLocation()
                 {
                     PropertyKey = "startingLocation"
                 },*/
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
                        #region TraitTemplates.  some variety.
                        TraitTemplates = new StringChance[] { 
                                new StringChance() { Edge = 0.1f, String = "electronicsSpecialist" },
                                new StringChance() { Edge = 0.2f, String = "mechanicsSpecialist" },
                                new StringChance() { Edge = 0.3f, String = "chemistrySpecialist" },
                                new StringChance() { Edge = 0.5f, String = "menialSpecialist" }, 
                                new StringChance() { Edge = 0.6f, String = "medicineSpecialist" }, 
                                new StringChance() { Edge = 0.8f, String = "bushcraftSpecialist" }, 
                                new StringChance() { Edge = 0.9f, String = "constructionSpecialist" },
                                new StringChance() { Edge = 1f, String = "securitySpecialist" }, 
                           
                            },
                        #endregion
                        #region CultureTemplates. Some weighting towards the ones where we have most names
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
                    }


                }

            });
            #endregion


            #endregion


            // OtherSite2 Desert Base --This  site is for decoration. no contact with it: 
            #region spawnOtherSite2 (for decoration)
            list.Add(
                new SpawnSiteAction()
                {
                    KeyName = "spawnOtherSite2", //

                    SiteData = new SiteData()
                    {
                        Name = "Zone I-25", // "Sahara Desert"  https://en.wikipedia.org/wiki/List_of_deserts
                        KeyName = "otherSite2",
                        Coords = new Overland.Locations.GeodeticCoordinate(13.24655d, 79.83451d), //{ Latitude = 43.83451d, Longitude = 5.24655d },
                        IsPlaySite = false
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
                    Name = "Desert base",
                    KeyName = "otherSite2Allegiance1",
                    EntityType = "entity:human",
                    AllegianceType = Allegiances.AllegianceType.Other,
                    StatsData = new StatsData()
                    {
                        Security = .32f,
                        Comfort = .27f,
                        FoodSupply = .59f,
                    }
                }

            });
            #endregion
            #region spawnOtherSite2Expedition1
            list.Add(new CreateExpeditionAction()
            {
                KeyName = "spawnOtherSite2Expedition1",
                DelayInSeconds = 0.1, // wait for starting location property to have been set!

                ExpeditionData = new ExpeditionData()
                {
                    KeyName = "otherSite2Expedition1",
                    Name = "Central", // as in Town Central...
                    AllegianceKey = "otherSite2Allegiance1"


                }

            });
            #endregion


            #endregion

            //GAME AREA:
            #region Characters
            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnRandomMuckrootPerson1",
                DelayInSeconds = delayForItemsAndAgents,

                DynamicLocation = new DynamicLocation()
                {
                    PropertyKey = "startingLocation"
                },
                EntityData = new Maps.MapEditor.EntityData()
                {
                    Location = new Microsoft.Xna.Framework.Vector3(170f, -30f, 0),
                    EntityKey = "entity:human",
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "playerAllegiance",
                        ExpeditionKey = "Camp"
                    },
                   
                    Person = new Maps.MapEditor.Person()
                    {
                        PersonalityType = "randomTierPersonality",
                        SimulateJoinedExpeditionNow = true,
                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {
                        #region TraitTemplates.  some variety.
                        TraitTemplates = new StringChance[] { 
                                new StringChance() { Edge = 0.15f, String = "electronicsSpecialist" },
                                new StringChance() { Edge = 0.3f, String = "mechanicsSpecialist" },
                                new StringChance() { Edge = 0.5f, String = "chemistrySpecialist" }, 
                                new StringChance() { Edge = 0.6f, String = "medicineSpecialist" }, 
                                new StringChance() { Edge = 0.8f, String = "bushcraftSpecialist" }, 
                                new StringChance() { Edge = 1f, String = "securitySpecialist" }, 
                           
                            },
                        #endregion
                        #region CultureTemplates. Some weighting towards the ones where we have most names
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
                    NeedLevels = startNeeds,


                }

            });

            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnRandomMuckrootPerson2",
                DelayInSeconds = delayForItemsAndAgents,

                DynamicLocation = new DynamicLocation()
                {
                    PropertyKey = "startingLocation"
                },
                EntityData = new Maps.MapEditor.EntityData()
                {
                    Location = new Microsoft.Xna.Framework.Vector3(206f, 48f, 0),
                    EntityKey = "entity:human",
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "playerAllegiance",
                        ExpeditionKey = "Camp"
                    },
                  
                    Person = new Maps.MapEditor.Person()
                    {

                        PersonalityType = "randomTierPersonality",
                        SimulateJoinedExpeditionNow = true,
                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {
                        #region TraitTemplates.  some variety.
                        TraitTemplates = new StringChance[] { 
                                new StringChance() { Edge = 0.15f, String = "electronicsSpecialist" },
                                new StringChance() { Edge = 0.3f, String = "mechanicsSpecialist" },
                                new StringChance() { Edge = 0.5f, String = "chemistrySpecialist" }, 
                                new StringChance() { Edge = 0.6f, String = "medicineSpecialist" }, 
                                new StringChance() { Edge = 0.8f, String = "bushcraftSpecialist" }, 
                                new StringChance() { Edge = 1f, String = "securitySpecialist" }, 
                           
                            },
                        #endregion
                        #region CultureTemplates. Some weighting towards the ones where we have most names
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
                    NeedLevels = startNeeds,


                }

            });


            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnRandomMuckrootPerson3",
                DelayInSeconds = delayForItemsAndAgents,

                DynamicLocation = new DynamicLocation()
                {
                    PropertyKey = "startingLocation"
                },
                EntityData = new Maps.MapEditor.EntityData()
                {
                    Location = new Microsoft.Xna.Framework.Vector3(144f, -48f, 0),
                    EntityKey = "entity:human",
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "playerAllegiance",
                        ExpeditionKey = "Camp"
                    },
                 
                    Person = new Maps.MapEditor.Person()
                    {

                        PersonalityType = "randomTierPersonality",
                        SimulateJoinedExpeditionNow = true,
                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {
                        #region TraitTemplates.  some variety.
                        TraitTemplates = new StringChance[] { 
                                new StringChance() { Edge = 0.15f, String = "electronicsSpecialist" },
                                new StringChance() { Edge = 0.3f, String = "mechanicsSpecialist" },
                                new StringChance() { Edge = 0.5f, String = "chemistrySpecialist" }, 
                                new StringChance() { Edge = 0.6f, String = "medicineSpecialist" }, 
                                new StringChance() { Edge = 0.8f, String = "bushcraftSpecialist" }, 
                                new StringChance() { Edge = 1f, String = "securitySpecialist" }, 
                           
                            },
                        #endregion
                        #region CultureTemplates. Some weighting towards the ones where we have most names
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
                    NeedLevels = startNeeds,


                }

            });

            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnRandomMuckrootPerson4",
                DelayInSeconds = delayForItemsAndAgents,

                DynamicLocation = new DynamicLocation()
                {
                    PropertyKey = "startingLocation"
                },
                EntityData = new Maps.MapEditor.EntityData()
                {
                    Location = new Microsoft.Xna.Framework.Vector3(96f, 96f, 0),
                    EntityKey = "entity:human",
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "playerAllegiance",
                        ExpeditionKey = "Camp"
                    },                   
                    Person = new Maps.MapEditor.Person()
                    {
                        PersonalityType = "randomTierPersonality",
                        SimulateJoinedExpeditionNow = true,
                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {
                        #region TraitTemplates.  some variety.
                        TraitTemplates = new StringChance[] { 
                                new StringChance() { Edge = 0.15f, String = "electronicsSpecialist" },
                                new StringChance() { Edge = 0.3f, String = "mechanicsSpecialist" },
                                new StringChance() { Edge = 0.5f, String = "chemistrySpecialist" }, 
                                new StringChance() { Edge = 0.6f, String = "medicineSpecialist" }, 
                                new StringChance() { Edge = 0.8f, String = "bushcraftSpecialist" }, 
                                new StringChance() { Edge = 1f, String = "securitySpecialist" }, 
                           
                            },
                        #endregion
                        #region CultureTemplates. Some weighting towards the ones where we have most names
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
                    NeedLevels = startNeeds,


                }

            });

            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnRandomMuckrootPerson5",
                DelayInSeconds = delayForItemsAndAgents,

                DynamicLocation = new DynamicLocation()
                {
                    PropertyKey = "startingLocation"
                },
                EntityData = new Maps.MapEditor.EntityData()
                {
                    Location = new Microsoft.Xna.Framework.Vector3(96f, -48f, 0),
                    EntityKey = "entity:human",
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "playerAllegiance",
                        ExpeditionKey = "Camp"
                    },                 
                    Person = new Maps.MapEditor.Person()
                    {
                        PersonalityType = "randomTierPersonality",
                        SimulateJoinedExpeditionNow = true,
                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {
                        #region TraitTemplates.  some variety.
                        TraitTemplates = new StringChance[] { 
                                new StringChance() { Edge = 0.15f, String = "electronicsSpecialist" },
                                new StringChance() { Edge = 0.3f, String = "mechanicsSpecialist" },
                                new StringChance() { Edge = 0.5f, String = "chemistrySpecialist" }, 
                                new StringChance() { Edge = 0.6f, String = "medicineSpecialist" }, 
                                new StringChance() { Edge = 0.8f, String = "bushcraftSpecialist" }, 
                                new StringChance() { Edge = 1f, String = "securitySpecialist" }, 
                           
                            },
                        #endregion
                        #region CultureTemplates. Some weighting towards the ones where we have most names
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
                    NeedLevels = startNeeds,


                }

            });


            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnRandomMuckrootPerson6",
                DelayInSeconds = delayForItemsAndAgents,

                DynamicLocation = new DynamicLocation()
                {
                    PropertyKey = "startingLocation"
                },
                EntityData = new Maps.MapEditor.EntityData()
                {
                    Location = new Microsoft.Xna.Framework.Vector3(180f, 48f, 0),
                    EntityKey = "entity:human",
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "playerAllegiance",
                        ExpeditionKey = "Camp"
                    },                
                    Person = new Maps.MapEditor.Person()
                    {
                        PersonalityType = "randomTierPersonality",
                        SimulateJoinedExpeditionNow = true,
                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {
                        #region TraitTemplates.  some variety.
                        TraitTemplates = new StringChance[] { 
                                new StringChance() { Edge = 0.15f, String = "electronicsSpecialist" },
                                new StringChance() { Edge = 0.3f, String = "mechanicsSpecialist" },
                                new StringChance() { Edge = 0.5f, String = "chemistrySpecialist" }, 
                                new StringChance() { Edge = 0.6f, String = "medicineSpecialist" }, 
                                new StringChance() { Edge = 0.8f, String = "bushcraftSpecialist" }, 
                                new StringChance() { Edge = 1f, String = "securitySpecialist" }, 
                           
                            },
                        #endregion
                        #region CultureTemplates. Some weighting towards the ones where we have most names
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
                    NeedLevels = startNeeds,


                }

            });



            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnRandomMuckrootPerson7",
                DelayInSeconds = delayForItemsAndAgents,

                DynamicLocation = new DynamicLocation()
                {
                    PropertyKey = "startingLocation"
                },
                EntityData = new Maps.MapEditor.EntityData()
                {
                    Location = new Microsoft.Xna.Framework.Vector3(96f, 0f, 0),
                    EntityKey = "entity:human",
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "playerAllegiance",
                        ExpeditionKey = "Camp"
                    },
                  
                    Person = new Maps.MapEditor.Person()
                    {
                        PersonalityType = "randomTierPersonality",
                        SimulateJoinedExpeditionNow = true,
                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {
                        #region TraitTemplates.  some variety.
                        TraitTemplates = new StringChance[] { 
                                new StringChance() { Edge = 0.15f, String = "electronicsSpecialist" },
                                new StringChance() { Edge = 0.3f, String = "mechanicsSpecialist" },
                                new StringChance() { Edge = 0.5f, String = "chemistrySpecialist" }, 
                                new StringChance() { Edge = 0.6f, String = "medicineSpecialist" }, 
                                new StringChance() { Edge = 0.8f, String = "bushcraftSpecialist" }, 
                                new StringChance() { Edge = 1f, String = "securitySpecialist" }, 
                           
                            },
                        #endregion
                        #region CultureTemplates. Some weighting towards the ones where we have most names
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
                    NeedLevels = startNeeds,


                }

            });


            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnGuardAnimal",
                DelayInSeconds = delayForItemsAndAgents,

                DynamicLocation = new DynamicLocation()
                {
                    PropertyKey = "startingLocation"
                },
                EntityData = new EntityData()
                {
                    Location = new Microsoft.Xna.Framework.Vector3(10f, 20f, 0),
                    EntityKey = "entity:dog",
                    OwnedBy = new AllegianceAndExpedition() { AllegianceKey = "playerAllegiance", ExpeditionKey = "Camp" },
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "playerAllegiance",
                        ExpeditionKey = "Camp"
                    },

                }

            });

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
                    Location = new Microsoft.Xna.Framework.Vector3(3f, 40f, 0),
                    EntityKey = "entity:robotSmall", //"entity:patrolRobot"
                    OwnedBy = new AllegianceAndExpedition() { AllegianceKey = "playerAllegiance", ExpeditionKey = "Camp" },
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "playerAllegiance",
                        ExpeditionKey = "Camp"
                    }
                }

            });

            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnGunDog",
                DelayInSeconds = delayForItemsAndAgents,

                DynamicLocation = new DynamicLocation()
                {
                    PropertyKey = "startingLocation"
                },
                EntityData = new EntityData()
                {
                    Location = new Microsoft.Xna.Framework.Vector3(5f, 50f, 0),
                    EntityKey = "entity:gunDog",
                    OwnedBy = new AllegianceAndExpedition() { AllegianceKey = "playerAllegiance", ExpeditionKey = "Camp" },
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "playerAllegiance",
                        ExpeditionKey = "Camp"
                    }
                }

            });


            #endregion

            #region Starting Structures


            //11, 50 = 552, 2424
            // 1680, 2010
            list.Add(ScenarioLoader.SpawnItemAtStartLocation("startStructureSmallTent", new Vector2(0f, 0f), "structure:smallTent", "playerAllegiance", null, delayForStructures, "Small Tent"));
            list.Add(ScenarioLoader.SpawnItemAtStartLocation("startStructureDomeTent", new Vector2(80f, 0f), "structure:domeTent", "playerAllegiance", null, delayForStructures, "Dome Tent"));
            list.Add(ScenarioLoader.SpawnItemAtStartLocation("startStructureFieldKitchen", new Vector2(128f, 80f), "structure:fieldKitchen", "playerAllegiance", null, delayForStructures));
            list.Add(ScenarioLoader.SpawnItemAtStartLocation("startStructureHelipadBig", new Vector2(0f, 188f), "structure:helipadBig", "playerAllegiance", null, delayForStructures, "Helipad"));
            list.Add(ScenarioLoader.SpawnItemAtStartLocation("startStructureGroundStation", new Vector2(-100f, 40f), "structure:satelliteGroundStation", "playerAllegiance", null, delayForStructures));


            // list.Add(ScenarioLoader.SpawnItemAtStartLocation("startHarpyTest", new Vector2(-100f, -40f), "entity:harpy", "playerAllegiance", null, delayForStructures, rotation: 3f * MathHelper.PiOver4));

            #endregion

            #region Starting equipment
            float xItemPosition = -24f;
            float yItemPosition = 80f;

            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSensor", new Vector2(xItemPosition, yItemPosition), "item:sensor", "playerAllegiance", null, delayForItemsAndAgents));
            //list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("", new Vector2(xItemPosition, yItemPosition), "item:advancedKnife", "playerAllegiance", null, delayForItemsAndAgents));
            //list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("", new Vector2(xItemPosition, yItemPosition), "item:advancedSnips", "playerAllegiance", null, delayForItemsAndAgents));
            //list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("", new Vector2(xItemPosition, yItemPosition), "item:advancedString", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startThermalTarp", new Vector2(xItemPosition, yItemPosition), "item:thermalTarp", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startDomeTent", new Vector2(xItemPosition, yItemPosition), "item:domeTent", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startOctagonalTent", new Vector2(xItemPosition, yItemPosition), "item:octagonalTent", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSmallTent", new Vector2(xItemPosition, yItemPosition), "item:smallTent", "playerAllegiance", null, delayForItemsAndAgents));

            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startFieldKitchenStove", new Vector2(xItemPosition, yItemPosition), "item:fieldKitchenStove", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startFieldKitchenEquipment", new Vector2(xItemPosition, yItemPosition), "item:fieldKitchenEquipment", "playerAllegiance", null, delayForItemsAndAgents));

            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startWeatherStationMast", new Vector2(xItemPosition, yItemPosition), "item:weatherStationMast", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startWeatherStationSensors", new Vector2(xItemPosition, yItemPosition), "item:weatherStationSensors", "playerAllegiance", null, delayForItemsAndAgents));

            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSatelliteGroundStation", new Vector2(xItemPosition, yItemPosition), "item:satelliteGroundStation", "playerAllegiance", null, delayForItemsAndAgents));


            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startAssemblerCabinet", new Vector2(xItemPosition, yItemPosition), "item:assemblerCabinet", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startVacuumChamber", new Vector2(xItemPosition, yItemPosition), "item:vacuumChamber", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startAssemblerCooling", new Vector2(xItemPosition, yItemPosition), "item:assemblerCooling", "playerAllegiance", null, delayForItemsAndAgents));

            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startPaint", new Vector2(xItemPosition, yItemPosition), "item:paint", "playerAllegiance", null, delayForItemsAndAgents));

            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startLiquidGas", new Vector2(xItemPosition, yItemPosition), "item:liquidGas", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startRation", new Vector2(xItemPosition, yItemPosition), "item:astroRation", "playerAllegiance", null, delayForItemsAndAgents));
            // list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startHuntingRifle", new Vector2(xItemPosition, yItemPosition), "item:coilRifle", "playerAllegiance", null, delayForItemsAndAgents));
            //  list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startRifleAmmo", new Vector2(xItemPosition, yItemPosition), "item:coilRifleAmmo", "playerAllegiance", null, delayForItemsAndAgents));
            //  list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSentry", new Vector2(xItemPosition, yItemPosition), "item:sentry", "playerAllegiance", null, delayForItemsAndAgents));
            //   list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSentryAmmo", new Vector2(xItemPosition, yItemPosition), "item:sentryGunAmmo", "playerAllegiance", null, delayForItemsAndAgents));

            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startBasicFireExtinguisher", new Vector2(xItemPosition, yItemPosition), "item:basicFireExtinguisher", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startChickenMeat", new Vector2(xItemPosition, yItemPosition), "item:thunderChickenMeat", "playerAllegiance", null, delayForItemsAndAgents));


            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startAcetylene", new Vector2(xItemPosition, yItemPosition), "item:acetylene", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startIronCanister", new Vector2(xItemPosition, yItemPosition), "item:ironCanister", "playerAllegiance", null, delayForItemsAndAgents));

            // sell test:
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startIronCanisterForSale", "Helipad", "item:ironCanister", "playerAllegiance", null, delayForItemsAndAgents, offerForSale: true));

            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startHuntingRifle", "Small Tent", "item:coilRifle", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startRifleAmmo", "Small Tent", "item:coilRifleAmmo", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startSentry", "Small Tent", "item:sentry", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startSentryAmmo", "Small Tent", "item:sentryGunAmmo", "playerAllegiance", null, delayForItemsAndAgents));

            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startSimCoffeeBeans", "Dome Tent", "item:simCoffeeBeans", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startCookingPot", "Dome Tent", "item:advancedCookingPot", "playerAllegiance", null, delayForItemsAndAgents));

            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startAssemblerPlateA", "Dome Tent", "item:assemblerPlateA", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startAssemblerMasterPlateA", "Dome Tent", "item:masterAssemblerPlateA", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startAssemblerPlateB", "Dome Tent", "item:assemblerPlateB", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startAssemblerMasterPlateB", "Dome Tent", "item:masterAssemblerPlateB", "playerAllegiance", null, delayForItemsAndAgents));

            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startMachete", "Dome Tent", "item:advancedMachete", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startSnips", "Dome Tent", "item:advancedSnips", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startString", "Dome Tent", "item:advancedString", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startKnife", "Dome Tent", "item:advancedKnife", "playerAllegiance", null, delayForItemsAndAgents));




            #endregion

            #region placeExpedition

            list.Add(new CreateExpeditionAction()
            {
                KeyName = "placeExpedition",
                DelayInSeconds = 0.1, // wait for starting location property to have been set!

                ExpeditionData = new ExpeditionData()
                {
                    KeyName = "Camp", //the player's site
                    Name = "Camp",
                    AllegianceKey = "playerAllegiance",
                    PolicyData = new Policies.ExpeditionPolicyData()
                    {
                        FractionIndependentsAllowedToSleep = 0.7f,  //agents allowed to sleep.              
                        CurrentTiers = new SerializableDictionary<RatingTypes, string>()
                            {
                                { RatingTypes.Comfort, "advanced"  },
                                { RatingTypes.Food, "advanced"  },
                                { RatingTypes.Security, "advanced"  }
                            },
                        AllowAmmoUseAgainstVermin = new SerializableDictionary<string, bool>()
                        {
                            { "item:coilRifleAmmo", false }, // conserve ammo
                            { "item:sentryGunAmmo", false },
                            { "item:shotgunAmmo", false },
                            { "item:bushDragonCartridge", false }                            
                        }
                    },
                    Location = new ValueNode()
                    {
                        PropertyKey = "startingLocation"
                    }                  
                },
                StatsData = new StatsData()
                {
                    Security = 1f,
                    Comfort = 1f,
                    FoodSupply = 1f,
                }

            });
            #endregion

            #region setStartingLocations
            list.Add(new SetPropertyAction()
            {
                KeyName = "setStartingLocationSouth",

                PropertyKey = "startingLocation",
                Value = new ValueNode()
                    {
                        //Location = new Microsoft.Xna.Framework.Vector2(288, 1536)
                        Location = new Microsoft.Xna.Framework.Vector2(1680f, 2016f)
                    }
                // 624f, 2448f

            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "setStartingLocationNorth",

                // has to be set first, because the other spawns depend on the value!
                PropertyKey = "startingLocation",
                Value = new ValueNode() { Location = new Microsoft.Xna.Framework.Vector2(2400f, 720f) }// MP: was: 1708f, 800f

            });

            #endregion

            #region Start View and Fog of war
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
                KeyName = "exploreEntireMap",

                DelayInSeconds = delayForItemsAndAgents + 1, // must execute last, after members have been added
                ExploreWholeMap = true,
                DetectMode = DetectMode.NoEntityDetection                
            });
            #endregion

            #region FAUNA
            #region Fauna expeditions and allegiances
            // animal expeditions: important for tree demons because they must not move outside of spoak fo+rests.
            #region spawnDemonTreeExpedition#3
            list.Add(
                new SpawnAllegianceAction()
                {
                    KeyName = "spawnDemonTreeExpedition#3",

                    Site = "playSite",

                    ExpeditionData = new ExpeditionData()
                    {
                        KeyName = "demonTreeAllegiance#3",
                        Name = "demonTreeAllegiance#3",
                        AllegianceKey = "demonTreeAllegiance#3",
                        Location = new ValueNode()
                        {
                            Location = new Vector2(1524, 1680)
                        }
                    },

                    AllegianceData = new AllegianceData()
                    {
                        ForageAndHuntingRadius = 144, //small patch of trees close to camp                            
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
            #region spawnDemonTreeExpedition#1
            list.Add(
                new SpawnAllegianceAction()
                {
                    KeyName = "spawnDemonTreeExpedition#1",

                    Site = "playSite",

                    ExpeditionData = new ExpeditionData()
                    {
                        KeyName = "demonTreeAllegiance#1",
                        Name = "demonTreeAllegiance#1",
                        AllegianceKey = "demonTreeAllegiance#1",
                        Location = new ValueNode()
                        {
                            Location = new Vector2(4224, 2592)
                        }
                    },

                    AllegianceData = new AllegianceData()
                    {
                        ForageAndHuntingRadius = 600, //South east sandstone
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
            #region spawnDemonTreeExpedition#2
            list.Add(
                new SpawnAllegianceAction()
                {
                    KeyName = "spawnDemonTreeExpedition#2",

                    Site = "playSite",

                    ExpeditionData = new ExpeditionData()
                    {
                        KeyName = "demonTreeAllegiance#2",
                        Name = "demonTreeAllegiance#2",
                        AllegianceKey = "demonTreeAllegiance#2",
                        Location = new ValueNode()
                        {
                            Location = new Vector2(2419, 558)
                        }
                    },
                    AllegianceData = new AllegianceData()
                    {

                        ForageAndHuntingRadius = 380, //way up north

                        Name = "Demon Tree Allegiance #2",
                        KeyName = "demonTreeAllegiance#2",
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
            #region binalRatAllegianceNorthWest1

            list.Add(
                new SpawnAllegianceAction()
                {
                    KeyName = "binalRatAllegianceNorthWest1",

                    Site = "playSite",

                    ExpeditionData = new ExpeditionData()
                    {
                        KeyName = "binalRatAllegianceNorthWest1",
                        Name = "binalRatAllegianceNorthWest1",
                        AllegianceKey = "binalRatAllegianceNorthWest1",
                        Location = new ValueNode()
                        {
                            Location = new Vector2(1680, 1872)
                        }
                    },

                    AllegianceData = new AllegianceData()
                    {
                        ForageAndHuntingRadius = 1440,
                        Name = "binalRatAllegianceNorthWest1",
                        KeyName = "binalRatAllegianceNorthWest1",
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
            #region binalRatAllegianceNorthWest2
            list.Add(
                new SpawnAllegianceAction()
                {
                    KeyName = "binalRatAllegianceNorthWest2",

                    Site = "playSite",

                    ExpeditionData = new ExpeditionData()
                    {
                        KeyName = "binalRatAllegianceNorthWest2",
                        Name = "binalRatAllegianceNorthWest2",
                        AllegianceKey = "binalRatAllegianceNorthWest2",
                        Location = new ValueNode()
                        {
                            Location = new Vector2(3312, 1584)
                        }
                    },

                    AllegianceData = new AllegianceData()
                    {
                        ForageAndHuntingRadius = 1440,
                        Name = "binalRatAllegianceNorthWest2",
                        KeyName = "binalRatAllegianceNorthWest2",
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
            #region binalRatAllegianceNorthWest3
            list.Add(
                new SpawnAllegianceAction()
                {
                    KeyName = "binalRatAllegianceNorthWest3",

                    Site = "playSite",

                    ExpeditionData = new ExpeditionData()
                    {
                        KeyName = "binalRatAllegianceNorthWest3",
                        Name = "binalRatAllegianceNorthWest3",
                        AllegianceKey = "binalRatAllegianceNorthWest3",
                        Location = new ValueNode()
                        {
                            Location = new Vector2(480, 2160)
                        }
                    },

                    AllegianceData = new AllegianceData()
                    {
                        ForageAndHuntingRadius = 1440,
                        Name = "binalRatAllegianceNorthWest3",
                        KeyName = "binalRatAllegianceNorthWest3",
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
            #region spawnThunderChickenAllegiance
            list.Add(
                new SpawnAllegianceAction()
                {
                    KeyName = "spawnThunderChickenAllegiance",

                    Site = "playSite",

                    ExpeditionData = new ExpeditionData()
                    {
                        KeyName = "", // ??
                        Name = "",
                        AllegianceKey = "thunderChickenAllegiance",
                        Location = new ValueNode()
                        {
                            Location = new Vector2(500, 500)
                        }
                    },

                    AllegianceData = new AllegianceData()
                    {
                        ForageAndHuntingRadius = 200,

                        Name = "Thunderchicken Allegiance",
                        KeyName = "thunderChickenAllegiance",
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
            #endregion

            #region Spawn intervals and pop caps - Lars: few of these are ever invoked, it seems

            // instead of setting each cave explicitly, this could also be made as an added number that affects the two caves.
            list.Add(new SetPropertyAction()
            {
                KeyName = "setTwinklerSpawnIntervalSouthOften",

                PropertyKey = "twinklerSpawnIntervalSouth",
                Value = new ValueNode() { Int = 5 }, //360

            });
            list.Add(new SetPropertyAction()
            {
                KeyName = "setTwinklerSpawnIntervalEastOften",

                PropertyKey = "twinklerSpawnIntervalEast",
                Value = new ValueNode() { Int = 5 }, //260

            });
            list.Add(new SetPropertyAction()
            {
                KeyName = "setTwinklerSpawnIntervalSouthSeldom",

                PropertyKey = "twinklerSpawnIntervalSouth",
                Value = new ValueNode() { Int = 800 }, // 600

            });
            list.Add(new SetPropertyAction()
            {
                KeyName = "setTwinklerSpawnIntervalEastSeldom",

                PropertyKey = "twinklerSpawnIntervalEast",
                Value = new ValueNode() { Int = 1000 }, //500

            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "setMaxTwinklersLow",

                PropertyKey = "maxTwinklers",
                Value = new ValueNode() { Int = 2 }, // on easy, there are only hunters spawned. this setting ensures that player will only get attacked by maximum of 2 hunters

            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "setMaxTwinklersNormal",

                PropertyKey = "maxTwinklers",
                Value = new ValueNode() { Int = 12 },

            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "setMaxTwinklersHigh",

                PropertyKey = "maxTwinklers",
                Value = new ValueNode() { Int = 30 }, //25

            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "setMaxThunderChickensLow",

                PropertyKey = "maxThunderChickens",
                Value = new ValueNode() { Int = 10 },

            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "setMaxThunderChickensNormal",

                PropertyKey = "maxThunderChickens",
                Value = new ValueNode() { Int = 15 },

            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "setMaxThunderChickensHigh",

                PropertyKey = "maxThunderChickens",
                Value = new ValueNode() { Int = 25 },

            });
            #endregion
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
                ExcludeResourceTypes = new[] { "stones", "firegrassSod", "commonOilTubers", "sulfurDeposit", "streakFin", "carbonTail", "alabasterRay", "daggermouth", "clamwich", "torux", "minnowsLive", "phantomWeaver", "ursinix", "webWing", "crestedFoiler", "goldenCenobite", "muckGrinder" }, //MP I want "stones", "firegrassSod" to follow the landscape sprites, so no variation there. "commoncommonOilTubers" is part of the Tut, no variation wanted. Sulfur part of scenario.
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
                ExcludeResourceTypes = new[] { "stones", "firegrassSod", "commonOilTubers", "sulfurDeposit", "streakFin", "carbonTail", "alabasterRay", "daggermouth", "clamwich", "torux", "minnowsLive", "phantomWeaver", "ursinix", "webWing", "crestedFoiler", "goldenCenobite", "muckGrinder" },
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
                ExcludeResourceTypes = new[] { "stones", "firegrassSod", "commonOilTubers", "sulfurDeposit", "streakFin", "carbonTail", "alabasterRay", "daggermouth", "clamwich", "torux", "minnowsLive", "phantomWeaver", "ursinix", "webWing", "crestedFoiler", "goldenCenobite", "muckGrinder" },
                OperationToUse = ChangeResourcesAction.Operation.Multiply,
                //   NoiseAmplitude = 0.5f,
                NoiseParameters = new NoiseParams()
                   {
                       NoiseAddend = -0.4f, //minus 'X %'     MP may23, was: -0.2f                      
                       NoiseFrequency = 0.1f
                   }

            });

            #endregion

            #region Particles




            list.Add(
              new ParticleEffectAction()
              {
                  KeyName = "smallFog1",     //Little ""steam" rising from pond      

                  Location = new ValueNode()
                  {
                      Location = Maps.MapManager.TileToWorldPosVector2(new Point(23, 30))
                  },
                  ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "smallFog" } }

              });

            list.Add(
              new ParticleEffectAction()
              {
                  KeyName = "smallFog2",     //Little ""steam" rising from pond      

                  Location = new ValueNode()
                  {
                      Location = Maps.MapManager.TileToWorldPosVector2(new Point(16, 28))
                  },
                  ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "smallFog" } }

              });

            list.Add(
              new ParticleEffectAction()
              {
                  KeyName = "smallFog3",     //Little ""steam" rising from pond      

                  Location = new ValueNode()
                  {
                      Location = Maps.MapManager.TileToWorldPosVector2(new Point(7, 39))
                  },
                  ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "smallFog" } }

              });

            list.Add(
             new ParticleEffectAction()
             {
                 KeyName = "smallFog4",     //Little ""steam" rising from pond      

                 Location = new ValueNode()
                 {
                     Location = Maps.MapManager.TileToWorldPosVector2(new Point(20, 38))
                 },
                 ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "smallFog" } }

             });

            list.Add(
             new ParticleEffectAction()
             {
                 KeyName = "smallFog5",     //Little ""steam" rising from pond      

                 Location = new ValueNode()
                 {
                     Location = Maps.MapManager.TileToWorldPosVector2(new Point(25, 25))
                 },
                 ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "smallFog" } }

             });

            list.Add(
             new ParticleEffectAction()
             {
                 KeyName = "smallFog6",     //Little ""steam" rising from pond      

                 Location = new ValueNode()
                 {
                     Location = Maps.MapManager.TileToWorldPosVector2(new Point(35, 49))
                 },
                 ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "smallFog" } }

             });

            list.Add(
             new ParticleEffectAction()
             {
                 KeyName = "smallFog7",     //Little ""steam" rising from pond      

                 Location = new ValueNode()
                 {
                     Location = Maps.MapManager.TileToWorldPosVector2(new Point(22, 43))
                 },
                 ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "smallFog" } }

             });

            list.Add(
              new ParticleEffectAction()
              {
                  KeyName = "sulphurousSmoke1",     //sulphurous lakes          

                  TimeBetweenEmissions = 0.5f,
                  Location = new ValueNode()
                  {
                      Location = Maps.MapManager.TileToWorldPosVector2(new Point(44, 28))
                  },
                  ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "sulphurousSmoke" } }
                  //           The.Client.ParticleManager.AddEmitter("sulphurousSmoke", MapManager.TileToWorldPosVector2(new Point(44, 28)), null, 0.5f);
                  //            // 1st argument: Tile position, 2nd argument: offset in pixels from center of tile. 3rd argument: Scale of clouds (null = use default) 4th argument: Time between puffs in seconds (null = use default)

              });

            list.Add(
              new ParticleEffectAction()
              {
                  KeyName = "haze1",     //desert haze           

                  Scale = 4f,
                  Location = new ValueNode()
                  {
                      Location = Maps.MapManager.TileToWorldPosVector2(new Point(8, 9))
                  },
                  ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "haze" } }
                  // The.Client.ParticleManager.AddEmitter("haze", MapManager.TileToWorldPosVector2(new Point(39, 27)), 4f, null); migrated from this....how to migrate size etc???? MP
                  // 1st argument: Tile position, 2nd argument: offset in pixels from center of tile. 3rd argument: Scale of clouds (null = use default) 4th argument: Time between puffs in seconds (null = use default)

              });

            list.Add(
              new ParticleEffectAction()
              {
                  KeyName = "haze2",     //north steppe haze           

                  Scale = 6f,
                  Location = new ValueNode()
                  {
                      Location = Maps.MapManager.TileToWorldPosVector2(new Point(36, 23))
                  },
                  ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "haze" } }
                  //The.Client.ParticleManager.AddEmitter("haze", MapManager.TileToWorldPosVector2(new Point(42, 16)), 8f, null);
                  // 1st argument: Tile position, 2nd argument: offset in pixels from center of tile. 3rd argument: Scale of clouds (null = use default) 4th argument: Time between puffs in seconds (null = use default)

              });




            #endregion






            // sell test:
            // list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startBinalRatForSale", "Helipad", "item:binalRatCarcass", "playerAllegiance", null, delayForItemsAndAgents, offerForSale: true, bulk: 0.1f));          
            //  list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startThunderChickenMeatForSale", "Helipad", "item:thunderChickenMeat", "playerAllegiance", null, delayForItemsAndAgents, offerForSale: true));          
            // list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startIronCanisterForSale", "Helipad", "item:ironCanister", "playerAllegiance", null, delayForItemsAndAgents, offerForSale: true));


            return list;


        }

    }
}
