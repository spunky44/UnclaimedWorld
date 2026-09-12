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
using UWGame.SimSide.Systems;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_4x.Data
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
                Value = new ValueNode() { String = "Legend had it that back on Earth, death was something you planned long in advance. Out here, death happened at any moment. \nLosing #NAMEOFDECEASED#CAUSEOFDEATH was a cause of grief, but at the burial, #EUOLOGYGIVER emphasized the value of a life in freedom, no matter its duration." }
            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "initBurialText3",
                PropertyKey = "burialTextSingleDeathMultipleSurvivors",
                Value = new ValueNode() { String = "Legend had it that back on Earth, death was something you planned long in advance. Out here, death happened at any moment. \nLosing #NAMEOFDECEASED#CAUSEOFDEATH was a cause of grief, but at the burial, #EUOLOGYGIVER emphasized the value of a life in freedom, no matter its duration." }
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
                        //map gui: w:527px, h: 442px

                        ViewLongitudeStart = 8, // 8, ...w
                        ViewLongitudeEnd = 12, // 13, 
                        ViewLatitudeStart = 70, // 70,  ...h
                        ViewLatitudeEnd = 73 // 78 
                    }

                });

            #endregion

            //Fields of Tau Ceti Playsite:
            #region spawnPlaySite
            list.Add(
                new SpawnSiteAction()
                {
                    KeyName = "spawnPlaySite",
                    SiteDataKey = "playSite"
                    /*
                    SiteData = new SiteData()
                    {
                        Name = "Cudgel Hills", //The Cudgels  //has to be generic enough that it covers all the landscape types on map
                        Key = "playSite",
                        Coords = new Overland.Locations.GeodeticCoordinate(9.65d, 72.1d), //make sure they can travel by boat in straight line to and from. //Latitude = , Longitude =  },
                        IsPlaySite = true,
                        ShowLabel = true,
                        ShowTallPin = true,
                        SiteMarkerOrder = 10
                    }*/
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

                        Name = "Castor Homestead", //"Player Allegiance",
                        KeyName = "playerAllegiance",
                        EntityType = "entity:human",
                        AllegianceType = Allegiances.AllegianceType.Player,
                        /*     StatsData = new StatsData()
                             {
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
                        Decimal = 240
                    }

                });
            #endregion

            #region spawnOtherSite2 - random site profile
            list.Add(
                new SpawnSiteAction()
                {
                    KeyName = "spawnOtherSite2",
                    SiteDataKey = "randomSmallSiteDestinyRiver"
                });
            #endregion

            //Othersite1. for trading:
            #region spawnOtherSite1
            list.Add(
                new SpawnSiteAction()
                {
                    KeyName = "spawnOtherSite1",
                    SiteDataKey = "destinyRiverDeltaDescentEraSite"
                     /*
                    SiteData = new SiteData()
                    {
                        Name = "Destiny River Delta", //
                        Key = "otherSite1",
                        Coords = new Overland.Locations.GeodeticCoordinate(10.25d, 71.25d),                            // new Overland.Locations.GeodeticCoordinate(17.0144d, 60.97881d),                           
                        IsPlaySite = false,
                        ShowLabel = true,
                        ShowTallPin = true,
                        SiteMarkerOrder = 10
                    }*/

                });
            #endregion
            #region spawnOtherSite1Allegiance1
            list.Add(
                new SpawnAllegianceAction()
                {
                    KeyName = "spawnOtherSite1Allegiance1",

                    Site = "destinyRiverDeltaDescentEraSite", // "otherSite1",
                    AllegianceDataKey = "destinyRiverDeltaDescentAllegiance"
                });
            #endregion
            #region spawnOtherSite1Expedition1
            list.Add(new CreateExpeditionAction()
            {
                KeyName = "spawnOtherSite1Expedition1",// not player's site.
                DelayInSeconds = 0.1, // wait for starting location property to have been set!

                AllegianceKey = "destinyRiverDeltaDescentAllegiance",
                ExpeditionDataKey = "destinyRiverDeltaDescentExpedition"
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
                        Allegiance2 = "destinyRiverDeltaDescentAllegiance",
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
                        Allegiance2 = "destinyRiverDeltaDescentAllegiance",
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
                        Allegiance2 = "destinyRiverDeltaDescentAllegiance",
                        Relation = 0f
                    }

                });

            #endregion


            #region spawnWildernessSite1

            list.Add(
               new SpawnSiteAction()
               {
                   KeyName = "spawnWildernessSite1",

                   //   DistanceFromPlaySite = 9f, // 9 km away MP uncomment this to test distance. ('yelling' distance is below 10 km.) (comment out the coords below when testing)
                   //    BearingFromPlaySite = 2f, // MP: DOES NOT WORK // radians

                   SiteData = new SiteData()
                   {
                       Name = "The Plains",
                       KeyName = "wildernessSite1",
                       Description = "An alternative location in the wilderness, considered by some as a better place for settling.",
                       Coords = new Overland.Locations.GeodeticCoordinate(9.72d, 72.21d),//was  9.76d, 72.21d    9.74d, 72.1d
                       IsPlaySite = false,
                       ShowLabel = false,
                       ShowTallPin = true, //
                  //     SiteMarkerOrder = 10,
                   }


               });

            list.Add(
               new SpawnAllegianceAction()
               {
                   KeyName = "spawnWildernessSite1Allegiance1",

                   Site = "wildernessSite1",
                   AllegianceData = new AllegianceData()
                   {
                       Name = "The Plains",
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
                   KeyName = "spawnPlaySiteSite1Route",
                   DelayInSeconds = 1,

                   RouteData = new RouteData()
                   {
                       Name = "Batten Creek",
                       FromSite = "playSite",
                       ToSite = "destinyRiverDeltaDescentEraSite",
                       Length = 120,
                       RouteType = RouteType.CalmWater
                   }

               });

            list.Add(
              new SpawnRouteAction()
              {
                  KeyName = "spawnPlaySiteSite2Route",
                  DelayInSeconds = 1,

                  RouteData = new RouteData()
                  {
                      Name = "Batten Creek",
                      FromSite = "playSite",
                      ToSite = "randomSmallSiteDestinyRiver",
                      Length = 60, // about half the length
                      RouteType = RouteType.CalmWater
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
                        AllegianceKey = "destinyRiverDeltaDescentAllegiance",
                        ExpeditionKey = "destinyRiverDeltaDescentExpedition"
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
                                new StringChance() { Edge = 0.125f, String = "maleDescendantWhiteCulture" },
                                new StringChance() { Edge = 0.25f, String = "maleDescendantAsianCulture" }, 
                                new StringChance() { Edge = 0.375f, String = "maleDescendantHispanicCulture" },
                                new StringChance() { Edge = 0.5f, String = "maleDescendantBlackCulture" },  

                                new StringChance() { Edge = 0.75f, String = "femaleDescendantWhiteCulture" },                                
                                new StringChance() { Edge = 1f, String = "femaleDescendantBlackCulture" },                         
                            }
                        #endregion
                    },
                    NeedLevels = startNeeds,
                    Properties = new SerializableDictionary<string, PropertyResult>() { { "origin", new PropertyResult() { StringResult = "othersite" } } } // Lars demo condition

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
                        AllegianceKey = "destinyRiverDeltaDescentAllegiance",
                        ExpeditionKey = "destinyRiverDeltaDescentExpedition"
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
                                new StringChance() { Edge = 0.125f, String = "maleDescendantWhiteCulture" },
                                new StringChance() { Edge = 0.25f, String = "maleDescendantAsianCulture" }, 
                                new StringChance() { Edge = 0.375f, String = "maleDescendantHispanicCulture" },
                                new StringChance() { Edge = 0.5f, String = "maleDescendantBlackCulture" },  

                                new StringChance() { Edge = 0.75f, String = "femaleDescendantWhiteCulture" },                                
                                new StringChance() { Edge = 1f, String = "femaleDescendantBlackCulture" },                         
                            }
                        #endregion
                    },
                    NeedLevels = startNeeds

                }

            });
            #endregion

            //MP: the survival and basic tier immigrants will often be equally likely to join early, becuase of the influence ofhappiness at othersite and the different personal interest number.  
            //(NOTE: I'm not using personal circumstances because it complicates it too much)
            #region spawnImmigrantSurvivalTier //the first immigrants that the player can get.
            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnImmigrantSurvivalTier",
                DelayInSeconds = delayForItemsAndAgents,

                // DynamicLocation = new DynamicLocation()   PropertyKey = "startingLocation"  },
                EntityData = new Maps.MapEditor.EntityData()
                {
                    //  Location = new Microsoft.Xna.Framework.Vector3(170f, -30f, 0),
                    EntityKey = "entity:human",
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "destinyRiverDeltaDescentAllegiance",
                        ExpeditionKey = "destinyRiverDeltaDescentExpedition"
                    },
                    Person = new Maps.MapEditor.Person()
                    {
                        PersonalityType = "survivalTierPersonality", //later immigrant

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
                                new StringChance() { Edge = 0.125f, String = "maleDescendantWhiteCulture" },
                                new StringChance() { Edge = 0.25f, String = "maleDescendantAsianCulture" }, 
                                new StringChance() { Edge = 0.375f, String = "maleDescendantHispanicCulture" },
                                new StringChance() { Edge = 0.5f, String = "maleDescendantBlackCulture" },  

                                new StringChance() { Edge = 0.75f, String = "femaleDescendantWhiteCulture" },                                
                                new StringChance() { Edge = 1f, String = "femaleDescendantBlackCulture" },                         
                            }
                        #endregion
                    },
                    NeedLevels = startNeeds

                }

            });
            #endregion

            #region spawnImmigrantBasicTier //to ensure that player has to improve ratings to get more people.
            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnImmigrantBasicTier",
                DelayInSeconds = delayForItemsAndAgents,

                // DynamicLocation = new DynamicLocation()   PropertyKey = "startingLocation"  },
                EntityData = new Maps.MapEditor.EntityData()
                {
                    //  Location = new Microsoft.Xna.Framework.Vector3(170f, -30f, 0),
                    EntityKey = "entity:human",
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "destinyRiverDeltaDescentAllegiance",
                        ExpeditionKey = "destinyRiverDeltaDescentExpedition"
                    },
                    Person = new Maps.MapEditor.Person()
                    {
                        PersonalityType = "basicTierPersonality", //later immigrant

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
                                new StringChance() { Edge = 0.125f, String = "maleDescendantWhiteCulture" },
                                new StringChance() { Edge = 0.25f, String = "maleDescendantAsianCulture" }, 
                                new StringChance() { Edge = 0.375f, String = "maleDescendantHispanicCulture" },
                                new StringChance() { Edge = 0.5f, String = "maleDescendantBlackCulture" },  

                                new StringChance() { Edge = 0.75f, String = "femaleDescendantWhiteCulture" },                                
                                new StringChance() { Edge = 1f, String = "femaleDescendantBlackCulture" },                         
                            }
                        #endregion
                    },
                    NeedLevels = startNeeds

                }

            });
            #endregion

            #region spawnImmigrantMediumTier //to make player able to advance to medium tier tech
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
                        AllegianceKey = "destinyRiverDeltaDescentAllegiance",
                        ExpeditionKey = "destinyRiverDeltaDescentExpedition"
                    },
                    Person = new Maps.MapEditor.Person()
                    {
                        PersonalityType = "mediumTierPersonality", //later immigrant

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
                                new StringChance() { Edge = 0.125f, String = "maleDescendantWhiteCulture" },
                                new StringChance() { Edge = 0.25f, String = "maleDescendantAsianCulture" }, 
                                new StringChance() { Edge = 0.375f, String = "maleDescendantHispanicCulture" },
                                new StringChance() { Edge = 0.5f, String = "maleDescendantBlackCulture" },  

                                new StringChance() { Edge = 0.75f, String = "femaleDescendantWhiteCulture" },                                
                                new StringChance() { Edge = 1f, String = "femaleDescendantBlackCulture" },                         
                            }
                        #endregion
                    },
                    NeedLevels = startNeeds

                }

            });
            #endregion

            #region spawnImmigrantAdvancedSecurityTier //to make player able to advance to advanced security tier tech. the other principles are lower.
            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnImmigrantAdvancedSecurityTier",
                DelayInSeconds = delayForItemsAndAgents,

                // DynamicLocation = new DynamicLocation()   PropertyKey = "startingLocation"  },
                EntityData = new Maps.MapEditor.EntityData()
                {
                    //  Location = new Microsoft.Xna.Framework.Vector3(170f, -30f, 0),
                    EntityKey = "entity:human",
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "destinyRiverDeltaDescentAllegiance",
                        ExpeditionKey = "destinyRiverDeltaDescentExpedition"
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
                                new StringChance() { Edge = 0.125f, String = "maleDescendantWhiteCulture" },
                                new StringChance() { Edge = 0.25f, String = "maleDescendantAsianCulture" }, 
                                new StringChance() { Edge = 0.375f, String = "maleDescendantHispanicCulture" },
                                new StringChance() { Edge = 0.5f, String = "maleDescendantBlackCulture" },  

                                new StringChance() { Edge = 0.75f, String = "femaleDescendantWhiteCulture" },                                
                                new StringChance() { Edge = 1f, String = "femaleDescendantBlackCulture" },                         
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
                        AllegianceKey = "destinyRiverDeltaDescentAllegiance",
                        ExpeditionKey = "destinyRiverDeltaDescentExpedition"
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
                                new StringChance() { Edge = 0.125f, String = "maleDescendantWhiteCulture" },
                                new StringChance() { Edge = 0.25f, String = "maleDescendantAsianCulture" }, 
                                new StringChance() { Edge = 0.375f, String = "maleDescendantHispanicCulture" },
                                new StringChance() { Edge = 0.5f, String = "maleDescendantBlackCulture" },  

                                new StringChance() { Edge = 0.75f, String = "femaleDescendantWhiteCulture" },                                
                                new StringChance() { Edge = 1f, String = "femaleDescendantBlackCulture" },                         
                            }
                        #endregion
                    },
                    NeedLevels = startNeeds

                }

            });
            #endregion

            #endregion
            #endregion

            //GAME AREA:
            #region Characters


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


            #region spawnCastor // SecuritySpecialist
            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnCastor",
                DelayInSeconds = delayForItemsAndAgents,

                DynamicLocation = new DynamicLocation()
                {
                    PropertyKey = "startingLocation"
                },
                EntityData = new Maps.MapEditor.EntityData()
                {
                    Location = new Microsoft.Xna.Framework.Vector3(-146f, 186f, 0), //stands next to equipment, looking inland at the others, speaks, as per the intro story.
                    EntityKey = "entity:human",
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "playerAllegiance",
                        ExpeditionKey = "Camp"
                    },
                   
                    Person = new Maps.MapEditor.Person()
                    {
                        FirstName = "Castor", //Keep this. He's mentioned in story
                        LastName = "Hernes",
                        PersonalityType = "survivalTierPersonality", //Replaced with Leader effect OLD:"noComplainerPersonality", //  NEVER complains or leaves,
                        Portrait = "human_h_m_adult_1",
                        SimulateJoinedExpeditionNow = true,
                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {
                        AgeInYears = new NormalDistribution() { Mean = 44f },
                        CasteKey = "male",
                        ModelTextureName = "ManOchreClothesBlackHairTexture",
                        RaceKey = "whiteHumanDescendant",
                        //CultureTemplates = new StringChance[] { new StringChance() { String = "maleDescendantWhiteCulture" } },
                        //RaceKey = "ManOchreClothesBlackHairTexture",
                        TraitTemplates = new StringChance[] { 
                                new StringChance() {  String = "electronicsSpecialist" }, //he sets up the radio
                                },

                    },
                    EffectProfiles = new[] { "leader" } // won't emigrate or complain at group meetings
                }

            });
            #endregion
            #region spawnLinsey BushcraftSpecialist
            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnLinsey",
                DelayInSeconds = delayForItemsAndAgents,

                DynamicLocation = new DynamicLocation()
                {
                    PropertyKey = "startingLocation"
                },
                EntityData = new Maps.MapEditor.EntityData()
                {
                    Location = new Microsoft.Xna.Framework.Vector3(-96f, 0f, 0),
                    EntityKey = "entity:human",
                    MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = "playerAllegiance",
                        ExpeditionKey = "Camp"
                    },
                  
                    Person = new Maps.MapEditor.Person()
                    {
                        FirstName = "Linsey", //Keep this. mentioned in story
                        LastName = "Cattier",
                        PersonalityType = "survivalTierPersonality",
                        Portrait = "human_b_f_adult_1",
                        SimulateJoinedExpeditionNow = true,
                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {
                        AgeInYears = new NormalDistribution() { Mean = 39f },
                        CasteKey = "female",
                        ModelTextureName = "ManGreenGreyClothes1Texture",
                        RaceKey = "blackHumanDescendant",
                        TraitTemplates = new StringChance[] { 
                                new StringChance() {  String = "bushcraftSpecialist" }, 
                                },

                    }
                }

            });
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
                                new StringChance() { Edge = 0.125f, String = "maleDescendantWhiteCulture" },
                                new StringChance() { Edge = 0.25f, String = "maleDescendantAsianCulture" }, 
                                new StringChance() { Edge = 0.375f, String = "maleDescendantHispanicCulture" },
                                new StringChance() { Edge = 0.5f, String = "maleDescendantBlackCulture" },  

                                new StringChance() { Edge = 0.75f, String = "femaleDescendantWhiteCulture" },                                
                                new StringChance() { Edge = 1f, String = "femaleDescendantBlackCulture" },                         
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
                        PersonalityType = "basicTierPersonality",
                        SimulateJoinedExpeditionNow = true,
                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {
                        TraitTemplates = new StringChance[] { 
                                new StringChance() {  String = "bushcraftSpecialist" },},
                        #region CultureTemplates. Future, 100% equality
                        CultureTemplates = new StringChance[] {
                                //equal distribution:
                                new StringChance() { Edge = 0.125f, String = "maleDescendantWhiteCulture" },
                                new StringChance() { Edge = 0.25f, String = "maleDescendantAsianCulture" }, 
                                new StringChance() { Edge = 0.375f, String = "maleDescendantHispanicCulture" },
                                new StringChance() { Edge = 0.5f, String = "maleDescendantBlackCulture" },  

                                new StringChance() { Edge = 0.75f, String = "femaleDescendantWhiteCulture" },                                
                                new StringChance() { Edge = 1f, String = "femaleDescendantBlackCulture" },                         
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
                    Location = new Microsoft.Xna.Framework.Vector3(-75f, -8f, 0),
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
                                new StringChance() { Edge = 0.125f, String = "maleDescendantWhiteCulture" },
                                new StringChance() { Edge = 0.25f, String = "maleDescendantAsianCulture" }, 
                                new StringChance() { Edge = 0.375f, String = "maleDescendantHispanicCulture" },
                                new StringChance() { Edge = 0.5f, String = "maleDescendantBlackCulture" },  

                                new StringChance() { Edge = 0.75f, String = "femaleDescendantWhiteCulture" },                                
                                new StringChance() { Edge = 1f, String = "femaleDescendantBlackCulture" },                         
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
                                new StringChance() { Edge = 0.125f, String = "maleDescendantWhiteCulture" },
                                new StringChance() { Edge = 0.25f, String = "maleDescendantAsianCulture" }, 
                                new StringChance() { Edge = 0.375f, String = "maleDescendantHispanicCulture" },
                                new StringChance() { Edge = 0.5f, String = "maleDescendantBlackCulture" },  

                                new StringChance() { Edge = 0.75f, String = "femaleDescendantWhiteCulture" },                                
                                new StringChance() { Edge = 1f, String = "femaleDescendantBlackCulture" },                         
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
                                new StringChance() { Edge = 0.125f, String = "maleDescendantWhiteCulture" },
                                new StringChance() { Edge = 0.25f, String = "maleDescendantAsianCulture" }, 
                                new StringChance() { Edge = 0.375f, String = "maleDescendantHispanicCulture" },
                                new StringChance() { Edge = 0.5f, String = "maleDescendantBlackCulture" },  

                                new StringChance() { Edge = 0.75f, String = "femaleDescendantWhiteCulture" },                                
                                new StringChance() { Edge = 1f, String = "femaleDescendantBlackCulture" },                         
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
                                new StringChance() { Edge = 0.125f, String = "maleDescendantWhiteCulture" },
                                new StringChance() { Edge = 0.25f, String = "maleDescendantAsianCulture" }, 
                                new StringChance() { Edge = 0.375f, String = "maleDescendantHispanicCulture" },
                                new StringChance() { Edge = 0.5f, String = "maleDescendantBlackCulture" },  

                                new StringChance() { Edge = 0.75f, String = "femaleDescendantWhiteCulture" },                                
                                new StringChance() { Edge = 1f, String = "femaleDescendantBlackCulture" },                         
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
                                new StringChance() { Edge = 0.125f, String = "maleDescendantWhiteCulture" },
                                new StringChance() { Edge = 0.25f, String = "maleDescendantAsianCulture" }, 
                                new StringChance() { Edge = 0.375f, String = "maleDescendantHispanicCulture" },
                                new StringChance() { Edge = 0.5f, String = "maleDescendantBlackCulture" },  

                                new StringChance() { Edge = 0.75f, String = "femaleDescendantWhiteCulture" },                                
                                new StringChance() { Edge = 1f, String = "femaleDescendantBlackCulture" },                         
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
                                new StringChance() { Edge = 0.125f, String = "maleDescendantWhiteCulture" },
                                new StringChance() { Edge = 0.25f, String = "maleDescendantAsianCulture" }, 
                                new StringChance() { Edge = 0.375f, String = "maleDescendantHispanicCulture" },
                                new StringChance() { Edge = 0.5f, String = "maleDescendantBlackCulture" },  

                                new StringChance() { Edge = 0.75f, String = "femaleDescendantWhiteCulture" },                                
                                new StringChance() { Edge = 1f, String = "femaleDescendantBlackCulture" },                         
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
                                new StringChance() { Edge = 0.125f, String = "maleDescendantWhiteCulture" },
                                new StringChance() { Edge = 0.25f, String = "maleDescendantAsianCulture" }, 
                                new StringChance() { Edge = 0.375f, String = "maleDescendantHispanicCulture" },
                                new StringChance() { Edge = 0.5f, String = "maleDescendantBlackCulture" },  

                                new StringChance() { Edge = 0.75f, String = "femaleDescendantWhiteCulture" },                                
                                new StringChance() { Edge = 1f, String = "femaleDescendantBlackCulture" },                         
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
                                new StringChance() { Edge = 0.125f, String = "maleDescendantWhiteCulture" },
                                new StringChance() { Edge = 0.25f, String = "maleDescendantAsianCulture" }, 
                                new StringChance() { Edge = 0.375f, String = "maleDescendantHispanicCulture" },
                                new StringChance() { Edge = 0.5f, String = "maleDescendantBlackCulture" },  

                                new StringChance() { Edge = 0.75f, String = "femaleDescendantWhiteCulture" },                                
                                new StringChance() { Edge = 1f, String = "femaleDescendantBlackCulture" },                         
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
                    Location = new Microsoft.Xna.Framework.Vector3(-60f, 100f, 0),
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
                                new StringChance() { Edge = 0.125f, String = "maleDescendantWhiteCulture" },
                                new StringChance() { Edge = 0.25f, String = "maleDescendantAsianCulture" }, 
                                new StringChance() { Edge = 0.375f, String = "maleDescendantHispanicCulture" },
                                new StringChance() { Edge = 0.5f, String = "maleDescendantBlackCulture" },  

                                new StringChance() { Edge = 0.75f, String = "femaleDescendantWhiteCulture" },                                
                                new StringChance() { Edge = 1f, String = "femaleDescendantBlackCulture" },                         
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
                                new StringChance() { Edge = 0.125f, String = "maleDescendantWhiteCulture" },
                                new StringChance() { Edge = 0.25f, String = "maleDescendantAsianCulture" }, 
                                new StringChance() { Edge = 0.375f, String = "maleDescendantHispanicCulture" },
                                new StringChance() { Edge = 0.5f, String = "maleDescendantBlackCulture" },  

                                new StringChance() { Edge = 0.75f, String = "femaleDescendantWhiteCulture" },                                
                                new StringChance() { Edge = 1f, String = "femaleDescendantBlackCulture" },                         
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
                                new StringChance() { Edge = 0.125f, String = "maleDescendantWhiteCulture" },
                                new StringChance() { Edge = 0.25f, String = "maleDescendantAsianCulture" }, 
                                new StringChance() { Edge = 0.375f, String = "maleDescendantHispanicCulture" },
                                new StringChance() { Edge = 0.5f, String = "maleDescendantBlackCulture" },  

                                new StringChance() { Edge = 0.75f, String = "femaleDescendantWhiteCulture" },                                
                                new StringChance() { Edge = 1f, String = "femaleDescendantBlackCulture" },                         
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
                                new StringChance() { Edge = 0.125f, String = "maleDescendantWhiteCulture" },
                                new StringChance() { Edge = 0.25f, String = "maleDescendantAsianCulture" }, 
                                new StringChance() { Edge = 0.375f, String = "maleDescendantHispanicCulture" },
                                new StringChance() { Edge = 0.5f, String = "maleDescendantBlackCulture" },  

                                new StringChance() { Edge = 0.75f, String = "femaleDescendantWhiteCulture" },                                
                                new StringChance() { Edge = 1f, String = "femaleDescendantBlackCulture" },                         
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
                                new StringChance() { Edge = 0.125f, String = "maleDescendantWhiteCulture" },
                                new StringChance() { Edge = 0.25f, String = "maleDescendantAsianCulture" }, 
                                new StringChance() { Edge = 0.375f, String = "maleDescendantHispanicCulture" },
                                new StringChance() { Edge = 0.5f, String = "maleDescendantBlackCulture" },  

                                new StringChance() { Edge = 0.75f, String = "femaleDescendantWhiteCulture" },                                
                                new StringChance() { Edge = 1f, String = "femaleDescendantBlackCulture" },                         
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
                                new StringChance() { Edge = 0.125f, String = "maleDescendantWhiteCulture" },
                                new StringChance() { Edge = 0.25f, String = "maleDescendantAsianCulture" }, 
                                new StringChance() { Edge = 0.375f, String = "maleDescendantHispanicCulture" },
                                new StringChance() { Edge = 0.5f, String = "maleDescendantBlackCulture" },  

                                new StringChance() { Edge = 0.75f, String = "femaleDescendantWhiteCulture" },                                
                                new StringChance() { Edge = 1f, String = "femaleDescendantBlackCulture" },                         
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



            #region Starting equipment
            //mp I place equipment,  to the left and south of of camp site. with this distance (-156f, 190f), they will move the items app. 1 tile north, just enough to give the illusion of people moving supplies from water and inland.
            //very important that camp location is placed well in relation to the coast, so that this movement makes sense (move supplies away from water.)
            // For testing
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startPanelScraps", new Vector2(-136f, 182f), "item:panelScraps", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSpikeTrap", new Vector2(-136f, 160f), "item:spikeTrap", "playerAllegiance", null, delayForItemsAndAgents)); //mp with -136f, 182f it was not accessible onHunting start loc.
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startVarmintBomb", new Vector2(-136f, 182f), "item:varmintBomb", "playerAllegiance", null, delayForItemsAndAgents));

            ////////////////   


            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startScrapMetal", new Vector2(-136f, 182f), "item:scrapMetal", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startTextile", new Vector2(-136f, 182f), "item:textile", "playerAllegiance", null, delayForItemsAndAgents));

            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startWroughtIron", new Vector2(-136f, 182f), "item:wroughtIron", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startBlisterSteel", new Vector2(-136f, 182f), "item:blisterSteel", "playerAllegiance", null, delayForItemsAndAgents));
            #region food
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startJerky", new Vector2(-136f, 182f), "item:driedBeef", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startBlackpulp", new Vector2(-136f, 182f), "item:blackpulp", "playerAllegiance", null, delayForItemsAndAgents));
            #endregion
            #region seeds
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startGlassyCreeper", new Vector2(-136f, 182f), "item:glassyCreeperPods", "playerAllegiance", null, delayForItemsAndAgents));
            #endregion
            #region weapons
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startGunpowderRifle", new Vector2(-146f, 176f), "item:gunpowderRifle", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startGunpowderAmmo", new Vector2(-146f, 176f), "item:blackPowderRifleAmmo", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startBlunderbuss", new Vector2(-146f, 176f), "item:musketoon", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startBlackPowderShotAmmo", new Vector2(-146f, 176f), "item:blackPowderShotAmmo", "playerAllegiance", null, delayForItemsAndAgents));

            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startBoltActionRifle", new Vector2(-146f, 176f), "item:boltActionRifle", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startBoltActionAmmo", new Vector2(-146f, 176f), "item:corditeAmmo", "playerAllegiance", null, delayForItemsAndAgents));

            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSentry", new Vector2(-146f, 176f), "item:sentry", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSentryAmmo", new Vector2(-146f, 176f), "item:sentryGunAmmo", "playerAllegiance", null, delayForItemsAndAgents));

            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startIronSpear", new Vector2(-146f, 176f), "item:ironSpear", "playerAllegiance", null, delayForItemsAndAgents));

            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startImprovisedBow", new Vector2(-146f, 176f), "item:improvisedBow", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startIronArrow", new Vector2(-146f, 176f), "item:ironArrow", "playerAllegiance", null, delayForItemsAndAgents));
            #endregion
            #region Tools
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startMachete", new Vector2(-156f, 190f), "item:advancedMachete", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSnips", new Vector2(-156f, 190f), "item:advancedSnips", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startString", new Vector2(-156f, 190f), "item:advancedString", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startMetalWire", new Vector2(-156f, 190f), "item:metalWire", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startKnife", new Vector2(-156f, 190f), "item:steelKnife", "playerAllegiance", null, delayForItemsAndAgents));

            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startCookingPot", new Vector2(-156f, 190f), "item:advancedCookingPot", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startGoldPot", new Vector2(-156f, 190f), "item:goldPot", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startImprovisedCookingPot", new Vector2(-156f, 190f), "item:improvisedCookingPot", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startHoe", new Vector2(-156f, 190f), "item:farmingHoe", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startHammer", new Vector2(-156f, 190f), "item:hammer", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startBellows", new Vector2(-156f, 190f), "item:bellows", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSulfurSmokeBomb", new Vector2(-156f, 190f), "item:bigBomb", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSensor", new Vector2(-156f, 190f), "item:sensor", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startTurnipCracker", new Vector2(-156f, 190f), "item:turnipCracker", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startBugNet", new Vector2(-156f, 190f), "item:strongBugNet", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startIronHooks", new Vector2(-156f, 190f), "item:ironHooks", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startNeonHornetsLive", new Vector2(-156f, 190f), "item:neonHornetsLive", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startCottonString", new Vector2(-156f, 190f), "item:cottonString", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startFishingNet", new Vector2(-156f, 190f), "item:fishingNet", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startFishTrapBasket", new Vector2(-156f, 190f), "item:fishTrapBasket", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startFishTrapHoopNet", new Vector2(-156f, 190f), "item:fishTrapHoopNet", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startPigFliesLive", new Vector2(-156f, 190f), "item:pigFliesLive", "playerAllegiance", null, delayForItemsAndAgents));

            //stuff placed closer to their campsite stockpile so they dont need to haul so much in beginning.:
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startRadioAntenna", new Vector2(-100f, 100f), "item:radioAntenna", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startRadio", new Vector2(-100f, 100f), "item:radio", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startShadeleafResin", new Vector2(-100f, 100f), "item:shadeleafResin", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startRawhideString", new Vector2(-100f, 100f), "item:rawhideString", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startFlintKnife", new Vector2(-100f, 100f), "item:flintKnife", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startMetalworkersToolbox", new Vector2(-100f, 100f), "item:metalWorkersToolbox", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startAnvil", new Vector2(-100f, 100f), "item:anvil", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startBarClamps", new Vector2(-100f, 100f), "item:barClamps", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startClayJar", new Vector2(-100f, 100f), "item:clayJar", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startBrickMold", new Vector2(-100f, 100f), "item:brickMold", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startRefrigerator", new Vector2(-100f, 100f), "item:inactivatedFoodCoolerUnit", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startDomeTent", new Vector2(-100f, 100f), "item:domeTent", "playerAllegiance", null, delayForItemsAndAgents));

          //mostly for testing://
          list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startDaysheenLeaves", new Vector2(-156f, 190f), "item:daysheenLeaves", "playerAllegiance", null, delayForItemsAndAgents));
          list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startFiregrassSod", new Vector2(-156f, 190f), "item:firegrassSod", "playerAllegiance", null, delayForItemsAndAgents));
          list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startStones", new Vector2(-156f, 190f), "item:stones", "playerAllegiance", null, delayForItemsAndAgents));
          list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSticks", new Vector2(-156f, 190f), "item:sticks", "playerAllegiance", null, delayForItemsAndAgents));
          list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startBlackpowder", new Vector2(-156f, 190f), "item:blackPowder", "playerAllegiance", null, delayForItemsAndAgents));//
          list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startBlowpipe", new Vector2(-156f, 190f), "item:blowpipe", "playerAllegiance", null, delayForItemsAndAgents));//
          list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startPickaxe", new Vector2(-156f, 190f), "item:steelPickaxe", "playerAllegiance", null, delayForItemsAndAgents));//
          list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSpade", new Vector2(-156f, 190f), "item:steelSpade", "playerAllegiance", null, delayForItemsAndAgents));//
          list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSteelSpade", new Vector2(-156f, 190f), "item:improvisedSpade", "playerAllegiance", null, delayForItemsAndAgents));//
          list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startCharcoal", new Vector2(-156f, 190f), "item:charcoal", "playerAllegiance", null, delayForItemsAndAgents));//
          list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startWetFirewood", new Vector2(-156f, 190f), "item:wetFirewood", "playerAllegiance", null, delayForItemsAndAgents));
          list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startGoldOre", new Vector2(-156f, 190f), "item:goldOre", "playerAllegiance", null, delayForItemsAndAgents));
          list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startBogOre", new Vector2(-156f, 190f), "item:bogOre", "playerAllegiance", null, delayForItemsAndAgents));
          list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startCleanTurnipGuts", new Vector2(-156f, 190f), "item:cleanTurnipGuts", "playerAllegiance", null, delayForItemsAndAgents));
          list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startFingerFruit", new Vector2(-156f, 190f), "item:fingerFruit", "playerAllegiance", null, delayForItemsAndAgents));
          list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startCrystalBerries", new Vector2(-156f, 190f), "item:crystalBerries", "playerAllegiance", null, delayForItemsAndAgents));
          list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSalt", new Vector2(-156f, 190f), "item:salt", "playerAllegiance", null, delayForItemsAndAgents));
          list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startClayPotUnglazed", new Vector2(-156f, 190f), "item:clayPotUnglazed", "playerAllegiance", null, delayForItemsAndAgents));
          list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startTappingBucket", new Vector2(-156f, 190f), "item:tappingBucket", "playerAllegiance", null, delayForItemsAndAgents));
          list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startClay", new Vector2(-156f, 190f), "item:clay", "playerAllegiance", null, delayForItemsAndAgents));
          list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startFirewood", new Vector2(-156f, 190f), "item:firewood", "playerAllegiance", null, delayForItemsAndAgents));
          list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSolidMudBrick", new Vector2(-156f, 190f), "item:solidMudBrick", "playerAllegiance", null, delayForItemsAndAgents));
          list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSpoakLeaves", new Vector2(-156f, 190f), "item:spoakLeaves", "playerAllegiance", null, delayForItemsAndAgents));
          list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSpoakBranchesTrimmed", new Vector2(-156f, 190f), "item:spoakBranchesTrimmed", "playerAllegiance", null, delayForItemsAndAgents));
          list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startMarshcotSap", new Vector2(-156f, 190f), "item:marshcotSap", "playerAllegiance", null, delayForItemsAndAgents));
          list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startWingweedMats", new Vector2(-156f, 190f), "item:wingweedMat", "playerAllegiance", null, delayForItemsAndAgents));
          list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSpoakShingles", new Vector2(-156f, 190f), "item:spoakShingles", "playerAllegiance", null, delayForItemsAndAgents));
          list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startIronHandAxe", new Vector2(-156f, 190f), "item:steelHandAxe", "playerAllegiance", null, delayForItemsAndAgents));
          list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startWaterCaneStem", new Vector2(-156f, 190f), "item:waterCaneStem", "playerAllegiance", null, delayForItemsAndAgents));
          list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startShadeleafCanes", new Vector2(-156f, 190f), "item:shadeleafCanes", "playerAllegiance", null, delayForItemsAndAgents));
          list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startVat", new Vector2(-156f, 190f), "item:vat", "playerAllegiance", null, delayForItemsAndAgents));
          list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startImprovisedGreenHouseCover", new Vector2(-156f, 190f), "item:improvisedGreenHouseCover", "playerAllegiance", null, delayForItemsAndAgents));
          list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startTurnipSalami", new Vector2(-156f, 190f), "item:turnipSalami", "playerAllegiance", null, delayForItemsAndAgents));
          list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startDriedSaltedStreakFin", new Vector2(-156f, 190f), "item:driedSaltedStreakFin", "playerAllegiance", null, delayForItemsAndAgents));
          list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startVinegar", new Vector2(-156f, 190f), "item:vinegar", "playerAllegiance", null, delayForItemsAndAgents));
          list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startHardtack", new Vector2(-156f, 190f), "item:hardtack", "playerAllegiance", null, delayForItemsAndAgents));
          
          
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
                  Name = "To: The Plains",
                  Location = new Vector3(0f, -200f, 0f) //

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
                KeyName = "setStartingLocationRiverBank",

                PropertyKey = "startingLocation",
                Value = new ValueNode()
                {
                    Location = new Microsoft.Xna.Framework.Vector2(3196f, 3032f) //3100f, 3000f
                }

            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "setStartingLocationNorthArableLand",

                PropertyKey = "startingLocation",
                Value = new ValueNode()
                {
                    Location = new Microsoft.Xna.Framework.Vector2(2208f, 1248f) // 
                }

            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "setStartingLocationNorthMuddyCreek",

                PropertyKey = "startingLocation",
                Value = new ValueNode()
                {
                    Location = new Microsoft.Xna.Framework.Vector2(2928f, 938f)
                }

            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "setStartingLocationSouthEastRockyFiregrass",

                PropertyKey = "startingLocation",
                Value = new ValueNode()
                {
                    Location = new Microsoft.Xna.Framework.Vector2(4678f, 5136f)
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

                KeyName = "exploreShroudRiverBank",
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
                OffsetLocationEnd = new Microsoft.Xna.Framework.Vector2(144f, 3744f),
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

                KeyName = "exploreShroudNorthArableLand",
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
                /*
                new DynamicLocation()
                {
                    PropertyKey = "startingLocation"
                },*/
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



            #endregion

            #region FAUNA
            #region Spawn trigger: destroys animals at map edge.
            list.Add(new SpawnTriggerAction()
            {
                KeyName = "spawnAnimalMigrateTriggerWest", //zone at edge of map where animals are destroyed to simulate them leaving the map

                Location = new ValueNode()
                {

                    Location = new Vector2(18, 1650) // this is the center of the box, not the upper left corner.
                },
                AreaDimensions = new Vector2(36f, 500f),
                TriggerType = "animalMigrateTrigger" //defined for the animals lesser whipjaw and bajingan in creatureloader

            });

            list.Add(new SpawnTriggerAction()
            {
                KeyName = "spawnAnimalMigrateTriggerRiver", //zone at edge of map where animals are destroyed to simulate them leaving the map

                Location = new ValueNode()
                {

                    Location = new Vector2(18, 3552) // this is the center of the box, not the upper left corner.
                },
                AreaDimensions = new Vector2(36f, 300f),
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
                        Location = new Vector2(2640, 3312)
                    },
                    PopulationData = new PopulationData()
                    {
                        StartMembersList = new[] { "bird#1", "bird#2", "bird#3" }, //birds require racekey
                        StartMembers = 3,
                        MaxMembers = 3,                         
                        GrowthInMembersPerDay = 0f
                        //     SpawnRadius = 
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

            #region Birds #2
            list.Add(new SpawnAllegianceAction()
            {
                KeyName = "spawnBirdExpedition#2", //tiny guano birds westernmost cave

                Site = "playSite",
                ExpeditionData = new ExpeditionData()
                {
                    KeyName = "birdExpedition#2",
                    AllegianceKey = "birdAllegiance#2",
                    Location = new ValueNode()
                    {
                        Location = new Vector2(3370, 2199)
                    },
                    PopulationData = new PopulationData()
                    {
                        StartMembersList = new[] { "bird#4", "bird#5", "bird#6" }, //birds require racekey
                        StartMembers = 3,
                        MaxMembers = 3,
                        GrowthInMembersPerDay = 0f
                        //     SpawnRadius =  not used bc of member loc spawns
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
                        Location = new Vector2(2100, 925)
                    },
                    PopulationData = new PopulationData()
                    {
                        MaxMembers = 5,
                        StartMembers = 5,
                        GrowthInMembersPerDay = 1f, 
                        SpawnRadius = 800 
                    }
                },
                AllegianceData = new AllegianceData()
                {
                    ForageAndHuntingRadius = 1000,
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

            #region Bajingan west (migration test) //moved to actionsetsloader
            /*          list.Add(new SpawnAllegianceAction()
            {
                KeyName = "spawnBajinganExpeditionWest",

                Site = "playSite",
                ExpeditionData = new ExpeditionData()
                {
                    Key = "bajinganAllegianceWest",
                    Name = "Bajingan Allegiance West",
                    AllegianceKey = "bajinganAllegianceWest",
                    Location = new ValueNode()
                    {
                        Location = new Vector2(48, 1776)
                    }
                },
                AllegianceData = new AllegianceData()
                {
                    ForageAndHuntingRadius = 110,
                    Name = "Bajingan Allegiance West",
                    Key = "bajinganAllegianceWest",
                    EntityType = "entity:bajingan",
                    AllegianceType = Allegiances.AllegianceType.Other,
                    StatsData = new StatsData()
                    {
                        Security = 1f,
                        Comfort = 1f,
                        FoodSupply = 1f,
                    }
                }

            });*/
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
                            Location = new Vector2(3072, 1488)//(2016, 2496) //4384, 2189
                        },
                    PopulationData = new PopulationData()
                    {
                        MaxMembers = 3,
                        StartMembers = 3,
                        GrowthInMembersPerDay = 10f,
                        SpawnRadius = 1500


                    }
                },
                AllegianceData = new AllegianceData()
                {
                    ForageAndHuntingRadius = 1500,
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
                        Location = new Vector2(1392, 2064)//(1536, 1104) //4900,4900
                    },
                    PopulationData = new PopulationData()
                    {
                        MaxMembers = 3,
                        StartMembers = 3,
                        GrowthInMembersPerDay = 10f,
                        SpawnRadius = 1500

                    }
                },
                AllegianceData = new AllegianceData()
                {
                    ForageAndHuntingRadius = 1500,
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

            #region Binal rat #3 (south east)
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
                        Location = new Vector2(4944, 5088)
                    },
                    PopulationData = new PopulationData()
                    {
                        MaxMembers = 3,
                        StartMembers = 3,
                        GrowthInMembersPerDay = 10f,
                        SpawnRadius = 1000

                    }
                },
                AllegianceData = new AllegianceData()
                {
                    ForageAndHuntingRadius = 1000,
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



            #region Demon Tree #3 north
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
                        Location = new Vector2(1749, 201)
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


            #region Demon Tree #1 south
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
                        Location = new Vector2(2592, 2750)
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
                        Location = new Vector2(624, 336)
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


            #region snatcher whipjaw east
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
                        Location = new Vector2(3552, 912)//
                    },
                    PopulationData = new PopulationData()
                    {
                        MaxMembers = 1,
                        StartMembers = 1,
                        GrowthInMembersPerDay = 0.7f, //
                        SpawnRadius = 200

                    }
                },
                AllegianceData = new AllegianceData()
                {
                    ForageAndHuntingRadius = 900,
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
                        Location = new Vector2(1824, 2976)                        
                    },
                    PopulationData = new PopulationData()
                    {
                         MaxMembers = 4,//was 2  july 2016
                         StartMembers = 3,
                         GrowthInMembersPerDay = 1.9f,
                         SpawnRadius = 900

                    }
                },
                AllegianceData = new AllegianceData()
                {
                    ForageAndHuntingRadius = 1000,
                    Name = "Thunder Chicken Allegiance #2",
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


            #region leafcutter expedition #1 //todo migrate to the new system used in scenario5

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
                        Location = new Vector2(1584, 1296)// was 2112, 1248
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

            #region leafcutter expedition #3 //todo migrate to the new system used in scenario5

            list.Add(new SpawnAllegianceAction()
            {
                KeyName = "spawnLeafcutterExpedition#3",

                Site = "playSite",
                ExpeditionData = new ExpeditionData()
                {
                    KeyName = "leafcutterExpedition#3",
                    Name = "Leafcutter Expedition",
                    AllegianceKey = "leafcutterAllegiance#3",
                    Location = new ValueNode()
                    {
                        Location = new Vector2(3600, 1248)// was 3600. 1248
                    },
                    PopulationData = new PopulationData()
                    {
                        SpawnSources = new string[] { "Field quadite nest 3" },
                        StartSpawnSources = new string[] { "fieldQuaditeNest3" }, // needed to allow a starting pop
                        StartMembers = 1,
                        MaxMembers = 5,
                        GrowthInMembersPerDay = 9.2f
                    }
                },
                AllegianceData = new AllegianceData()
                {
                    ForageAndHuntingRadius = 500, //
                    Name = "Leafcutter allegiance",
                    KeyName = "leafcutterAllegiance#3",
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

            #region Spawn intervals and pop caps //move data to expeditions above

            #region spawn intervals

            /*   //todo migrate to the new system used in scenario5   
             * list.Add(new EventActionType() //mp not used. the field quadite nests don't respawn. mp. what. yes they should. make sure they do.
            {
                KeyName = "setLeafcutterNestSpawnIntervalOften",
                
                    PropertyKey = "nestSpawnInterval",
                    Value = new ValueNode() { Int = 1600 },
                }
            });*/

            //  there's 1600 sec / day
            // scenario starts at TimeOfDay = 0.50   
            //RelativeNoOfDays = 0.5 //getting dark
            //RelativeNoOfDays = 0.795  // morning
            //RelativeNoOfDays = 1.123  //afternoon              
            //RelativeNoOfDays = 1.695  //night 


            list.Add(new SetPropertyAction() //todo migrate to the new system used in scenario5
            {
                KeyName = "setSpawnIntervalLeafcutterNormal",

                PropertyKey = "leafcutterSpawnInterval",
                Value = new ValueNode() { Int = 173 },

            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "setSpawnIntervalSlugsNormal",

                PropertyKey = "slugSpawnInterval",
                Value = new ValueNode() { Int = 2400 }, //(there's 1600 sec / day)

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

            #region Farmplots Small
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFarmSpotSmall1",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:smallPlotSpot",
                    Name = "Small plot (3168, 3072)",
                    Location = new Vector3(3168f, 3072f, 0)
                },


            });


            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFarmSpotSmall2",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:smallPlotSpot",
                    Name = "Small plot (2260, 2020)",
                    Location = new Vector3(2260f, 2020f, 0)
                },


            });

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFarmSpotSmall3",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:smallPlotSpot",
                    Name = "Small plot (2260, 2020)",
                    Location = new Vector3(1700f, 1844f, 0)
                },


            });

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFarmSpotSmall4",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:smallPlotSpot",
                    Name = "Small plot (1498, 1882)",
                    Location = new Vector3(1498f, 1882f, 0)
                },


            });

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFarmSpotSmall5",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:smallPlotSpot",
                    Name = "Small plot (1498, 1882)",
                    Location = new Vector3(1550f, 1464f, 0)
                },


            });

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFarmSpotSmall6",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:smallPlotSpot",
                    Name = "Small plot (1498, 1882)",
                    Location = new Vector3(2072f, 1256f, 0)
                },


            });


            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFarmSpotSmall7",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:smallPlotSpot",
                    Name = "Small plot (1498, 1882)",
                    Location = new Vector3(1680f, 912f, 0)
                },


            });

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFarmSpotSmall8",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:smallPlotSpot",
                    Name = "Small plot (1498, 1882)",
                    Location = new Vector3(1688f, 624f, 0)
                },


            });

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFarmSpotSmall9",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:smallPlotSpot",
                    Name = "Small plot (2119, 1882)",
                    Location = new Vector3(2119f, 952f, 0)
                },


            });

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFarmSpotSmall10",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:smallPlotSpot",
                    Name = "Small plot (2119, 1882)",
                    Location = new Vector3(2297f, 960f, 0)
                },


            });

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFarmSpotSmall11",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:smallPlotSpot",
                    Name = "Small plot (2119, 1882)",
                    Location = new Vector3(2450f, 682f, 0)
                },


            });


            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFarmSpotSmall12",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:smallPlotSpot",
                    Name = "Small plot (2119, 1882)",
                    Location = new Vector3(3024f, 390f, 0)
                },


            });

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFarmSpotSmall13",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:smallPlotSpot",
                    Name = "Small plot (2119, 1882)",
                    Location = new Vector3(3130f, 272f, 0)
                },


            });

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFarmSpotSmall14",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:smallPlotSpot",
                    Name = "Small plot (2119, 1882)",
                    Location = new Vector3(3498f, 1364f, 0)
                },


            });

            //south east corner:
            /*       list.Add(new EventActionType()
                   {
                       KeyName = "startFarmSpotSmall15",
                       DelayInSeconds = 1,
                
                           EntityData = new EntityData()
                           {
                               EntityKey = "terrain:smallPlotSpot",
                               Name = "Small plot (2119, 1882)",
                               Location = new Vector3(4825f, 5136f, 0)
                           },
                       }
                   });*/

            /*        list.Add(new EventActionType()
                    {
                        KeyName = "startFarmSpotSmall16",
                        DelayInSeconds = 1,
                
                            EntityData = new EntityData()
                            {
                                EntityKey = "terrain:smallPlotSpot",
                                Name = "Small plot (2119, 1882)",
                                Location = new Vector3(4666f, 5136f, 0)
                            },
                        }

                    });*/

            #endregion

            #region Farmplots Large
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFarmSpotLarge1",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:largePlotSpot",
                    Name = "Large plot (2119, 1882)",
                    Location = new Vector3(1209f, 825f, 0)
                },

            });

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFarmSpotLarge2",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:largePlotSpot",
                    Name = "Large plot (2119, 1882)",
                    Location = new Vector3(1882f, 1042f, 0)
                },

            });

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFarmSpotLarge3",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:largePlotSpot",
                    Name = "Large plot (2119, 1882)",
                    Location = new Vector3(1894f, 884f, 0)
                },

            });

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFarmSpotLarge4",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:largePlotSpot",
                    Name = "Large plot (2119, 1882)",
                    Location = new Vector3(2030f, 536f, 0)
                },

            });

            //south east corner:
            /*      list.Add(new EventActionType()
                  {
                      KeyName = "startFarmSpotLarge5",
                      DelayInSeconds = 1,
                
                          EntityData = new EntityData()
                          {
                              EntityKey = "terrain:largePlotSpot",
                              Name = "Large plot (2119, 1882)",
                              Location = new Vector3(4970f, 5626f, 0)
                          },
                      }
                  });*/



            #endregion


            #region Pier Spots




            list.Add(new SpawnEntityAction() //at the big river, center
            {
                KeyName = "startPierSpot1",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:pierSpot",
                    Name = "Pier spot",
                    Location = new Vector3(2804f, 3082f, 0)
                },

            });

            /*    list.Add(new EventActionType() //south east
                {
                    KeyName = "startPierSpot2",
                    DelayInSeconds = 1,
                
                        EntityData = new EntityData()
                        {
                            EntityKey = "terrain:pierSpot",
                            Name = "Pier spot",
                            Location = new Vector3(4340f, 5202f, 0)
                        },
                    }
                });*/

            list.Add(new SpawnEntityAction() //north east
            {
                KeyName = "startPierSpot3",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:pierSpot",
                    Name = "Pier spot",
                    Location = new Vector3(2952f, 1110f, 0) //was 3330f, 650f, 0  but too far away for bugfree hauling
                },

            });

            #endregion


            #region Fish weir spots
            /*     list.Add(new EventActionType()
            {
                KeyName = "startFishTrapCreek1",
                DelayInSeconds = 1,
                
                    EntityData = new EntityData()
                    {
                        EntityKey = "terrain:fishTrapSpotCreek",
                        Name = "Fish weir spot (3792f, 2736f)",
                        Location = new Vector3(4057f, 2758f, 0) //was inland: 3792f, 2736f,
                    },
                }
            });*/

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFishTrapCreek2",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:fishTrapSpotCreek",
                    Name = "Fish weir spot (3504f, 3456f)",
                    Location = new Vector3(3488f, 3300f, 0) //South east corner at emerald river , specially placed for small map.
                },

            });

            /*        list.Add(new EventActionType()
                    {
                        KeyName = "startFishTrapCreek3",
                        DelayInSeconds = 1,
                
                            EntityData = new EntityData()
                            {
                                EntityKey = "terrain:fishTrapSpotCreek",
                                Name = "Fish weir spot (3504f, 3456f)",
                                Location = new Vector3(2496f, 3982f, 0)
                            },
                        }
                    });*/

            /*       list.Add(new EventActionType()
                   {
                       KeyName = "startFishTrapCreek4",
                       DelayInSeconds = 1,
                
                           EntityData = new EntityData()
                           {
                               EntityKey = "terrain:fishTrapSpotCreek",
                               Name = "Fish weir spot (3504f, 3456f)",
                               Location = new Vector3(1900f, 4330f, 0)
                           },
                       }
                   });*/

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFishTrapCreek5",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:fishTrapSpotCreek",
                    Name = "Fish weir spot (3504f, 3456f)",
                    Location = new Vector3(2036f, 1750f, 0) //was 2036f, 1738f, but blocked
                },

            });

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFishTrapCreek6",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:fishTrapSpotCreek",
                    Name = "Fish weir spot (3504f, 3456f)",
                    Location = new Vector3(3075f, 865f, 0)
                },

            });

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFishTrapCreek7",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:fishTrapSpotCreek",
                    Name = "Fish weir spot (3504f, 3456f)",
                    Location = new Vector3(2130f, 761f, 0) //was (2110f, 750f, 0) but blocked
                },

            });

            /*    list.Add(new EventActionType()
                {
                    KeyName = "startFishTrapCreek8",
                    DelayInSeconds = 1,
                
                        EntityData = new EntityData()
                        {
                            EntityKey = "terrain:fishTrapSpotCreek",
                            Name = "Fish weir spot (3504f, 3456f)",
                            Location = new Vector3(4930f, 250f, 0)
                        },
                    }
                });*/


            #endregion

            #region Fish Trap Saltwater Spots (Coast)

            /*      list.Add(new EventActionType()
            {
                KeyName = "startFishTrapCoast1",
                DelayInSeconds = 1,
                
                    EntityData = new EntityData()
                    {
                        EntityKey = "terrain:fishTrapSpotCoast",
                        Name = "Fish trap spot Saltwater",
                        Location = new Vector3(4254f, 233f, 0)
                    },
                }
            });*/

            /*      list.Add(new EventActionType()
                  {
                      KeyName = "startFishTrapCoast2",
                      DelayInSeconds = 1,
                
                          EntityData = new EntityData()
                          {
                              EntityKey = "terrain:fishTrapSpotCoast",
                              Name = "Fish trap spot Saltwater",
                              Location = new Vector3(5303f, 252f, 0)
                          },
                      }
                  });*/

            //south east corner
            /*       list.Add(new EventActionType()
                   {
                       KeyName = "startFishTrapCoast3",
                       DelayInSeconds = 1,
                
                           EntityData = new EntityData()
                           {
                               EntityKey = "terrain:fishTrapSpotCoast",
                               Name = "Fish trap spot Saltwater",
                               Location = new Vector3(4465f, 5800f, 0)
                           },
                       }
                   });*/

            /*      list.Add(new EventActionType()
                  {
                      KeyName = "startFishTrapCoast4",
                      DelayInSeconds = 1,
                
                          EntityData = new EntityData()
                          {
                              EntityKey = "terrain:fishTrapSpotCoast",
                              Name = "Fish trap spot Saltwater",
                              Location = new Vector3(4575f, 5474f, 0)
                          },
                      }
                  });*/

            #endregion

            #region Fish Trap Freshwater Spots (Shore)

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFishTrapShore1",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:fishTrapSpotShore",
                    Name = "Fish trap spot Freshwater",
                    Location = new Vector3(3085f, 1066f, 0)
                },

            });

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFishTrapShore2",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:fishTrapSpotShore",
                    Name = "Fish trap spot Freshwater",
                    Location = new Vector3(1940f, 1431f, 0)
                },

            });

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFishTrapShore3",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:fishTrapSpotShore",
                    Name = "Fish trap spot Freshwater",
                    Location = new Vector3(2568f, 1738f, 0)
                },

            });


            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFishTrapShore4",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:fishTrapSpotShore",
                    Name = "Fish trap spot Freshwater",
                    Location = new Vector3(2708f, 2069f, 0)
                },

            });
            /* //mp terrain changed.
                        list.Add(new EventActionType()
                        {
                            KeyName = "startFishTrapShore5",
                            DelayInSeconds = 1,
                            
                                EntityData = new EntityData()
                                {
                                    EntityKey = "terrain:fishTrapSpotShore",
                                    Name = "Fish trap spot Freshwater",
                                    Location = new Vector3(3172f, 2292f, 0)
                                },
                            }
                        });*/

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFishTrapShore6",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:fishTrapSpotShore",
                    Name = "Fish trap spot Freshwater",
                    Location = new Vector3(1840f, 2024f, 0) //was (1840f, 2036f, 0) but blocked
                },

            });

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFishTrapShore7",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:fishTrapSpotShore",
                    Name = "Fish trap spot Freshwater",
                    Location = new Vector3(1278f, 2420f, 0)
                },

            });

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFishTrapShore8",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:fishTrapSpotShore",
                    Name = "Fish trap spot Freshwater",
                    Location = new Vector3(2080f, 2456f, 0) //was (2089f, 2457f, 0) but moved because blocked
                },

            });
/*              //removed because very close to fish weir. emerald river, east
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFishTrapShore9",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:fishTrapSpotShore",
                    Name = "Fish trap spot Freshwater",
                    Location = new Vector3(3576f, 3128f, 0) //
                },

            });*/



            /*      list.Add(new EventActionType()
                  {
                      KeyName = "startFishTrapShore10",
                      DelayInSeconds = 1,
                            
                          EntityData = new EntityData()
                          {
                              EntityKey = "terrain:fishTrapSpotShore",
                              Name = "Fish trap spot Freshwater",
                              Location = new Vector3(4000f, 2520f, 0)
                          },
                      }
                  });*/

            /*        list.Add(new EventActionType()
                    {
                        KeyName = "startFishTrapShore11",
                        DelayInSeconds = 1,
                
                            EntityData = new EntityData()
                            {
                                EntityKey = "terrain:fishTrapSpotShore",
                                Name = "Fish trap spot Freshwater",
                                Location = new Vector3(4972f, 2150f, 0)
                            },
                        }
                    });*/

            /*     list.Add(new EventActionType()
                 {
                     KeyName = "startFishTrapShore12",
                     DelayInSeconds = 1,
                
                         EntityData = new EntityData()
                         {
                             EntityKey = "terrain:fishTrapSpotShore",
                             Name = "Fish trap spot Freshwater",
                             Location = new Vector3(4618f, 2900f, 0)
                         },
                     }
                 });*/

            /*     list.Add(new EventActionType()
                 {
                     KeyName = "startFishTrapShore13",
                     DelayInSeconds = 1,
                
                         EntityData = new EntityData()
                         {
                             EntityKey = "terrain:fishTrapSpotShore",
                             Name = "Fish trap spot Freshwater",
                             Location = new Vector3(2827f, 3784f, 0)
                         },
                     }
                 });*/

            /*        list.Add(new EventActionType()
                    {
                        KeyName = "startFishTrapShore14",
                        DelayInSeconds = 1,
                
                            EntityData = new EntityData()
                            {
                                EntityKey = "terrain:fishTrapSpotShore",
                                Name = "Fish trap spot Freshwater",
                                Location = new Vector3(3178f, 4210f, 0)
                            },
                        }
                    });*/

            /*      list.Add(new EventActionType()
                  {
                      KeyName = "startFishTrapShore15",
                      DelayInSeconds = 1,
                
                          EntityData = new EntityData()
                          {
                              EntityKey = "terrain:fishTrapSpotShore",
                              Name = "Fish trap spot Freshwater",
                              Location = new Vector3(2342f, 4362f, 0)
                          },
                      }
                  });*/

            /*        list.Add(new EventActionType()
                    {
                        KeyName = "startFishTrapShore16",// number corresponds to psd level design doc
                        DelayInSeconds = 1,
                
                            EntityData = new EntityData()
                            {
                                EntityKey = "terrain:fishTrapSpotShore",
                                Name = "Fish trap spot Freshwater",
                                Location = new Vector3(760f, 4280f, 0)
                            },
                        }
                    });*/

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFishTrapShore17",// number corresponds to psd level design doc
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:fishTrapSpotShore",
                    Name = "Fish trap spot Freshwater",
                    Location = new Vector3(2328f, 3214f, 0) //mp july 2016  was 2328f, 3230f,   but was suddenly blocked. (even tho it shouldn't be)
                },

            });

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFishTrapShore18",// number corresponds to psd level design doc
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:fishTrapSpotShore",
                    Name = "Fish trap spot Freshwater",
                    Location = new Vector3(1752f, 3576f, 0)
                },

            });


            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFishTrapShore19",// number corresponds to psd level design doc
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:fishTrapSpotShore",
                    Name = "Fish trap spot Freshwater",
                    Location = new Vector3(852f, 3465f, 0)
                },

            });

            //south east corner

            /*    list.Add(new EventActionType()
                {
                    KeyName = "startFishTrapShore20",// not in psd doc sep 2015
                    DelayInSeconds = 1,
                
                        EntityData = new EntityData()
                        {
                            EntityKey = "terrain:fishTrapSpotShore",
                            Name = "Fish trap spot Freshwater",
                            Location = new Vector3(4128f, 5148f, 0)
                        },
                    }
                });*/

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
                    Location = new Vector3(650f, 1642f, 0) // far west
                },

            });
            #endregion
            #region bog ore deposit //
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startBogOreDeposit2",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:bogOreDeposit",
                    Name = "Bog ore deposit",
                    Location = new Vector3(1152f, 2496f, 0) // mid west
                },

            });
            #endregion


            #region peat deposit //
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startPeatDeposit1", //medium map. North west
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:peatDeposit",
                    Name = "Peat deposit",
                    Location = new Vector3(1130f, 1438f, 0) //
                },

             });
            #endregion
            #region peat deposit //
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startPeatDeposit2", //medium map. North
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:peatDeposit",
                    Name = "Peat deposit",
                    Location = new Vector3(2286f, 698f, 0) //
                },

            });
            #endregion
            #region peat deposit //
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startPeatDeposit3", //medium map. Center
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:peatDeposit",
                    Name = "Peat deposit",
                    Location = new Vector3(2488f, 2032f, 0) //
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
                    Location = new Vector3(2688f, 3590f, 0) //east, at emerald river
                },

            });
            #endregion

            #region clay deposit //
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startClayDeposit1",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:clayDeposit",
                    Name = "Clay deposit",
                    Location = new Vector3(2064f, 3264f, 0) //at emerald river, close to fishing start
                },

            });
            #endregion
            #region clay deposit //
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startClayDeposit2",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:clayDeposit",
                    Name = "Clay deposit",
                    Location = new Vector3(1556f, 3514f, 0) //at emerald river, mid 
                },

            });
            #endregion
            #region clay deposit //
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startClayDeposit3",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:clayDeposit",
                    Name = "Clay deposit",
                    Location = new Vector3(1580f, 428f, 0) //north west
                },

            });
            #endregion
            #region clay deposit //
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startClayDeposit4",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:clayDeposit",
                    Name = "Clay deposit",
                    Location = new Vector3(1090f, 1000f, 0) // west
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
                      Location = new Microsoft.Xna.Framework.Vector2(2160f, 1776f)
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

            list.Add(
              new ParticleEffectAction()
              {
                  KeyName = "smallFog3",     //Little ""steam" rising from pond      

                  Location = new ValueNode()
                  {
                      Location = new Microsoft.Xna.Framework.Vector2(1200f, 2640f)
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
                      Location = new Microsoft.Xna.Framework.Vector2(3120f, 2304f)
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
                      Location = new Microsoft.Xna.Framework.Vector2(3226f, 912f)

                  },
                  ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "smallFog" } }

              });
            list.Add(
              new ParticleEffectAction()
              {
                  KeyName = "smallFog6",     //Little ""steam" rising from pond      

                  Location = new ValueNode()
                  {
                      Location = new Microsoft.Xna.Framework.Vector2(2400f, 2448f)

                  },
                  ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "smallFog" } }

              });
            #endregion
            #region swamp fog


            list.Add(
              new ParticleEffectAction()
              {
                  KeyName = "fog1",     //smaller fog on swamp top left     

                  Scale = 5f,
                  Location = new ValueNode()
                  {
                      Location = Maps.MapManager.TileToWorldPosVector2(new Point(8, 8))

                  },
                  ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "fog" } }

              });

            list.Add(
              new ParticleEffectAction()
              {
                  KeyName = "fog2",     //smaller fog on swamp top left     

                  Scale = 5f,
                  Location = new ValueNode()
                  {
                      Location = Maps.MapManager.TileToWorldPosVector2(new Point(12, 11))

                  },
                  ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "fog" } }

              });

            ////////////// //SW
            /*          list.Add(
                        new EventActionType()
                        {
                            KeyName = "fog3",     //Large fog on swamp  SW   
                  
                                Scale = 7f,
                                Location = new ValueNode()
                                {
                                    Location = Maps.MapManager.TileToWorldPosVector2(new Point(15, 101))

                                },
                                ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "fog" } }
                            }
                        });
          */

            /*           list.Add(
                         new EventActionType()
                         {
                             KeyName = "fog4",     //smaller fog on swamp      
                  
                                 Scale = 10f,
                                 Location = new ValueNode()
                                 {
                                     Location = Maps.MapManager.TileToWorldPosVector2(new Point(9, 110))

                                 },
                                 ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "fog" } }
                             }
                         });*/
            /*        list.Add(
                      new EventActionType()
                      {

                          KeyName = "fog5",     //smaller fog on swamp      
                  
                              Scale = 10f,
                              Location = new ValueNode()
                              {
                                  Location = Maps.MapManager.TileToWorldPosVector2(new Point(22, 112))

                              },
                              ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "fog" } }
                          }
                      });
                    list.Add(
                      new EventActionType()
                      {
                          KeyName = "fog6",     //smaller fog on swamp      
                  
                              Scale = 12f,
                              Location = new ValueNode()
                              {
                                  Location = Maps.MapManager.TileToWorldPosVector2(new Point(38, 119))

                              },
                              ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "fog" } }
                          }
                      });
                    list.Add(
                      new EventActionType()
                      {
                          KeyName = "fog7",     //smaller fog on swamp      
                  
                              Scale = 13f,
                              Location = new ValueNode()
                              {
                                  Location = Maps.MapManager.TileToWorldPosVector2(new Point(47, 107))

                              },
                              ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "fog" } }
                          }
                      });
                    list.Add(
                      new EventActionType()
                      {
                          KeyName = "fog8",     //smaller fog on swamp      
                  
                              Scale = 13f,
                              Location = new ValueNode()
                              {
                                  Location = Maps.MapManager.TileToWorldPosVector2(new Point(34, 93))

                              },
                              ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "fog" } }
                          }
                      });*/

            #endregion

            #region sulfur
            /*       list.Add(
              new EventActionType()
              {
                  KeyName = "sulphurousSmoke1",     //sulphurous lakes          
                  
                      TimeBetweenEmissions = 0.5f,
                      Location = new ValueNode()
                      {
                          Location = new Microsoft.Xna.Framework.Vector2(5616f, 1248f)

                      },
                      ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "sulphurousSmoke" } }

                  }
              });*/
            /*        list.Add(
                      new EventActionType()
                      {
                          KeyName = "sulphurousSmoke2",     //sulphurous lakes          
                  
                              Scale = 3f,
                              TimeBetweenEmissions = 0.5f,
                              Location = new ValueNode()
                              {
                                  Location = new Microsoft.Xna.Framework.Vector2(5376f, 1258f)

                              },
                              ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "sulphurousSmoke" } }

                          }
                      });*/
            /*       list.Add(
                     new EventActionType()
                     {
                         KeyName = "sulphurousSmoke3",     //sulphurous lakes          
                  
                             Scale = 2f,
                             TimeBetweenEmissions = 0.5f,
                             Location = new ValueNode()
                             {
                                 Location = new Microsoft.Xna.Framework.Vector2(5136f, 1488f)

                             },
                             ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "sulphurousSmoke" } }

                         }
                     });*/

            list.Add(
                    new ParticleEffectAction()
                    {
                        KeyName = "sulphurousSmoke4",     //sulphurous lakes          

                        Scale = 2f,
                        TimeBetweenEmissions = 0.5f,
                        Location = new ValueNode()
                        {
                            Location = new Microsoft.Xna.Framework.Vector2(3312f, 1824f)

                        },
                        ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "sulphurousSmoke" } }


                    });
            #endregion
            #region haze
            /*      list.Add(
              new EventActionType()
              {
                  KeyName = "haze1",     // near sulfur           
                  
                      Scale = 4f,
                      Location = new ValueNode()
                      {
                          Location = new Microsoft.Xna.Framework.Vector2(5472f, 1248f)

                      },
                      ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "haze" } }
                      // The.Client.ParticleManager.AddEmitter("haze", MapManager.TileToWorldPosVector2(new Point(39, 27)), 4f, null); migrated from this....how to migrate size etc???? MP
                      // 1st argument: Tile position, 2nd argument: offset in pixels from center of tile. 3rd argument: Scale of clouds (null = use default) 4th argument: Time between puffs in seconds (null = use default)
                  }
              });*/

            list.Add(
                      new ParticleEffectAction()
                      {
                          KeyName = "haze6",     // near sulfur           

                          Scale = 3f,
                          Location = new ValueNode()
                          {
                              Location = new Microsoft.Xna.Framework.Vector2(3312f, 1824f)

                          },
                          ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "haze" } }
                          // The.Client.ParticleManager.AddEmitter("haze", MapManager.TileToWorldPosVector2(new Point(39, 27)), 4f, null); migrated from this....how to migrate size etc???? MP
                          // 1st argument: Tile position, 2nd argument: offset in pixels from center of tile. 3rd argument: Scale of clouds (null = use default) 4th argument: Time between puffs in seconds (null = use default)

                      });

            /*        list.Add(
                      new EventActionType()
                      {
                          KeyName = "haze2",     // arid peninsula         
                  
                              Scale = 6f,
                              Location = new ValueNode()
                              {
                                  Location = new Microsoft.Xna.Framework.Vector2(5664f, 2496f)

                              },
                              ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "haze" } }
                              //The.Client.ParticleManager.AddEmitter("haze", MapManager.TileToWorldPosVector2(new Point(42, 16)), 8f, null);
                              // 1st argument: Tile position, 2nd argument: offset in pixels from center of tile. 3rd argument: Scale of clouds (null = use default) 4th argument: Time between puffs in seconds (null = use default)
                          }
                      });*/
            /*        list.Add(
                      new EventActionType()
                      {
                          KeyName = "haze3",     // desert        
                  
                              Scale = 6f,
                              Location = new ValueNode()
                              {
                                  Location = new Microsoft.Xna.Framework.Vector2(4608f, 3505f)

                              },
                              ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "haze" } }
                              //The.Client.ParticleManager.AddEmitter("haze", MapManager.TileToWorldPosVector2(new Point(42, 16)), 8f, null);
                              // 1st argument: Tile position, 2nd argument: offset in pixels from center of tile. 3rd argument: Scale of clouds (null = use default) 4th argument: Time between puffs in seconds (null = use default)
                          }
                      });*/
            /*     list.Add(
                   new EventActionType()
                   {
                       KeyName = "haze4",     // desert        
                  
                           Scale = 6f,
                           Location = new ValueNode()
                           {
                               Location = new Microsoft.Xna.Framework.Vector2(5472f, 3648f)

                           },
                           ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "haze" } }
                       }
                   });
                 list.Add(
                   new EventActionType()
                   {
                       KeyName = "haze5",     // desert        
                  
                           Scale = 6f,
                           Location = new ValueNode()
                           {
                               Location = new Microsoft.Xna.Framework.Vector2(3936f, 3840f)

                           },
                           ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "haze" } }
                       }
                   });*/

            #endregion



            #endregion






            return list;
        }
    }
}
