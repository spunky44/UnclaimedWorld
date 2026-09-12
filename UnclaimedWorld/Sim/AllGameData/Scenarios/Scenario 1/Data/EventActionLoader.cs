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

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_1.Data
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
                Value = new ValueNode() { String = "#NAMEOFDECEASED just died. \nI buried the remains as best I could. Guess it's only me now..." }  //not used because not necessarily any deaths before this one: "#NAMEOFDECEASED is also dead now. \nI buried the remains as best I could. Guess it's only me now..."
            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "initBurialText5",
                PropertyKey = "burialTextMultipleDeathsSingleSurvivor",
                Value = new ValueNode() { String = "#NAMEOFDECEASED just died. \nI buried the remains as best I could. Guess it's only me now..." }
            });
            #endregion

            #region Group meeting dialog texts //they have a sciency tone of voice on twinkler island

            list.Add(new SetPropertyAction()
            {
                KeyName = "initTimeBeforeGroupMeeting",
                PropertyKey = "timeInGameSecondsBeforeGroupMeeting",
                Value = new ValueNode() { Decimal = 200 }
            });


            list.Add(new SetPropertyAction() //these 2 are used whereever it says #EMIGRATETHREAT - for the cases where all are unhappy, it uses "meetingEmigrateThreatAllUnhappy" 
            {
                KeyName = "meetingEmigrateThreat",
                PropertyKey = "meetingEmigrateThreat",
                Value = new ValueNode() { String = "If not... well, I might leave for #EMIGRATETO and start over." }
            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "meetingEmigrateThreatAllUnhappy",
                PropertyKey = "meetingEmigrateThreatAllUnhappy",
                Value = new ValueNode() { String = "Seriously, I'm sometimes wondering whether it would be better to go to #EMIGRATETO and start over." }
            });

            #region 3 people -meetings
            list.Add(new SetPropertyAction()
            {
                KeyName = "meeting3Security",
                PropertyKey = "meeting3Security",
                Value = new ValueNode()
                {
                    String = "-#UNHAPPY: Thanks for listening... I want to talk about the security situation. The way it's handled, I don't think we're safe here. \n \n"
                                + "-#CONTENT: Could you elaborate on that? \n \n"
                                + "-#UNHAPPY: I want us to beef up security. How do we do that? Craft better weapons, take fewer risks... There's a number of ways we can protect ourselves better. Main thing is that we take action now. \n \n"
                                + "-#CONTENT: This is all a question of priorities... \n \n"
                                + "-#UNHAPPY: Exactly. And if we value our lives, we need better protection from the animals on this island. So I hope you're with me. #EMIGRATETHREAT \n \n"
                                + "-#CONTENT: Alright, thanks for sharing your thoughts. Anything else?"
                }
            });



            list.Add(new SetPropertyAction() // 
            {
                KeyName = "meeting3Food",

                PropertyKey = "meeting3Food",
                Value = new ValueNode()
                {
                    String = "#UNHAPPY: I want to talk about the nutritional situation. \n \n"
                                + "#CONTENT: What is your concern? \n \n"
                                + "#UNHAPPY: Our foodstocks are grossly inadequate. We are at high risk of starvation. \n \n"
                                + "#CONTENT: Food has a high priority. \n \n"
                                + "#UNHAPPY: Not high enough. We need to work harder on acquiring and preserving food to avoid starvation. I hope you can see reason. #EMIGRATETHREAT \n \n"
                                + "#CONTENT: Ok we've heard you. Who else?"
                }

            });


            list.Add(new SetPropertyAction() // 
            {
                KeyName = "meeting3Comfort",

                PropertyKey = "meeting3Comfort",
                Value = new ValueNode()
                {
                    String = "#UNHAPPY: Friends, the living conditions in this camp are distressing. \n \n"
                                + "#CONTENT: But surely, food and security are more important? \n \n"
                                + "#UNHAPPY: No. Living in squalor has a highly detrimental effect, mentally and physically. \n \n"
                                + "#CONTENT: We have to focus our efforts where they count. \n \n"
                                + "#UNHAPPY: The bad shelters and lack of recreation puts us in danger of disease and mental breakdown. I wish you would take this more seriously. #EMIGRATETHREAT \n \n"
                                + "#CONTENT: We'll keep this in mind. Who has something to add?"
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
                                + "-#UNHAPPY: Listen. We're in danger here. We need to craft better weapons, take fewer risks... and we have to act now! I hope you understand! #EMIGRATETHREAT \n \n"
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
                                + "#UNHAPPY: You and me. We need to make this work. I do not want to end up in a situation where cannibalism is our only option. So please, see reason. #EMIGRATETHREAT \n \n"

                }

            });

            list.Add(new SetPropertyAction() //
            {
                KeyName = "meeting2Comfort",

                PropertyKey = "meeting2Comfort",
                Value = new ValueNode()
                {
                    String = "#UNHAPPY: We need better shelters. \n \n"
                                + "#CONTENT: Why is it so important. We are wearing survival suits. \n \n"
                                + "#UNHAPPY: Look, the filth, the cold and the bugs are driving me mad. Please, let's work on the living conditions. #EMIGRATETHREAT \n \n"
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
                    String = "#UNHAPPY: Look, we all want the security situation to improve! So let's get our act together and start working as a team! What are you waiting for? #EMIGRATETHREAT"
                }

            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "meetingFoodAllUnhappy",

                PropertyKey = "meetingFoodAllUnhappy",
                Value = new ValueNode()
                {
                    String = "#UNHAPPY: Look, we all want the food situation to improve! So let's get our act together and start working as a team! What are you waiting for? #EMIGRATETHREAT"
                }

            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "meetingComfortAllUnhappy",

                PropertyKey = "meetingComfortAllUnhappy",
                Value = new ValueNode()
                {
                    String = "#UNHAPPY: Look, we all want the comfort conditions to improve! So let's get our act together and start working as a team! What are you waiting for? #EMIGRATETHREAT"
                }

            });
            #endregion

            #endregion

            #region Emigrate dialog texts

            list.Add(new SetPropertyAction()
            {
                KeyName = "initEmigrateSecurityDialogText",
                PropertyKey = "securityEmigrateEventDialogText",
                Value = new ValueNode() { String = "AUDIO LOG, #JOURNALDATE \n \n#NAME1: We're in danger here. The twinklers scare me. But it scares me even more that you seem so cavalier about the threats we're facing. I'm leaving now. I know there's a safer island nearby. \n \n#NAME2: -You're actually gonna paddle to Knoll Island? You realize that's half an hour on the open sea? \n \n#NAME1: -The water is calm now and the currents are favorable. There's no doubt in my mind that I'm in greater peril if I stay here. Goodbye." }
            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "initEmigrateSecurityNoConversationDialogText",
                PropertyKey = "securityEmigrateEventNoConversationDialogText",
                Value = new ValueNode() { String = "TEXT LOG, #JOURNALDATE \n \n#NAME1: When you read this, I'll be on my way. This place scares me. - We're in danger here. The twinklers scare me. But it scares me even more that you seem so cavalier about the threats we're facing. I'm leaving now. I know there's a safer island nearby. \nGoodbye." }
            });


            list.Add(new SetPropertyAction()
            {
                KeyName = "initEmigrateComfortDialogText",
                PropertyKey = "comfortEmigrateEventDialogText",
                Value = new ValueNode() { String = "AUDIO LOG, #JOURNALDATE \n \n#NAME1: This place is a pig sty. I can't stand it. The cold, the bugs and the filth...why are you ok with living like this? \n \n#NAME2: Because there are other things that are more important? \n \n#NAME1: I'm fed up. These conditions here, they're subhuman. I'm leaving for Knoll Island. I can do better on my own." }
            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "initEmigrateComfortNoConversationDialogText",
                PropertyKey = "comfortEmigrateEventNoConversationDialogText",
                Value = new ValueNode() { String = "TEXT LOG, #JOURNALDATE \n \n#NAME1: When you read this, I'll be on my way. I'm fed up. These conditions here, they're subhuman. I'm leaving for Knoll Island. I can do better on my own." }
            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "initEmigrateFoodDialogText",
                PropertyKey = "foodEmigrateEventDialogText",
                Value = new ValueNode() { String = "AUDIO LOG, #JOURNALDATE \n \n#NAME1: I can't do it anymore. I'm so hungry all the time. We're on the brink of starvation!  Why are you not addressing this problem? Look at us! We're dying here! \n \n#NAME2: Hey! We agreed to focus on other things! \n#NAME1: What could be more important than food?! I've had it. I'm going to Knoll Island. \n \n#NAME2: Good luck with crossing that water. Hope you'll find what you're looking for." }
            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "initEmigrateFoodNoConversationDialogText",
                PropertyKey = "foodEmigrateEventNoConversationDialogText",
                Value = new ValueNode() { String = "TEXT LOG, #JOURNALDATE \n \n#NAME1: When you read this, I'll be on my way. I can't do it anymore. I'm so hungry all the time. We're on the brink of starvation. I've had it. I'm going to Knoll Island." }
            });

            #endregion

            list.Add(new SetPropertyAction()
            {
                KeyName = "initDeathCounter",
                PropertyKey = "deathCounter",
                Value = new ValueNode() { Int = 0 }
            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "initSulfurDetected",
                PropertyKey = "sulfurDetected",
                Value = new ValueNode() { Bool = false }
            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "initNestDetected",
                PropertyKey = "nestDetected",
                Value = new ValueNode() { Bool = false }
            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "initNestDestroyed",
                PropertyKey = "nestDestroyed",
                Value = new ValueNode() { Bool = false }
            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "initBushDragonDetectedShortDelay",
                PropertyKey = "bushDragonDetectedShortDelay",
                Value = new ValueNode() { Bool = false }
            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "initBushDragonDetectedLongDelay",
                PropertyKey = "bushDragonDetectedLongDelay",
                Value = new ValueNode() { Bool = false }
            });


            list.Add(new SetPropertyAction()
            {
                KeyName = "initGameOver",
                PropertyKey = "gameOver",
                Value = new ValueNode() { Bool = false }
            });
            /////////
            list.Add(new SetPropertyAction()
            {
                KeyName = "initSandstoneWreckage",
                PropertyKey = "sandstoneWreckage",
                Value = new ValueNode() { Bool = true } //determines dialogue when detecting the tail
            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "initNoSandstoneWreckage",
                PropertyKey = "sandstoneWreckage",
                Value = new ValueNode() { Bool = false }
            });
            ////////

            ////////
            list.Add(new SetPropertyAction()
            {
                KeyName = "initFledAtStart",
                PropertyKey = "fledAtStart",
                Value = new ValueNode() { Bool = true } //determines intro dialogue about fleeing and knowledge of twinklers
            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "initNotFledAtStart",
                PropertyKey = "fledAtStart",
                Value = new ValueNode() { Bool = false }
            });
            //////////

            list.Add(new SetPropertyAction()
            {
                KeyName = "initUnconsciousAndAlive",
                PropertyKey = "unconsciousAndAlive",
                Value = new ValueNode() { Bool = true }  //determines firing of dead team member event
            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "initCasualtyRescued",
                PropertyKey = "casualtyRescued",
                Value = new ValueNode() { Bool = false }  //determines post- rescue dialogue and event screen
            });

            //////////

            list.Add(new SetPropertyAction()
            {
                KeyName = "initFieldLabCannibalized",
                PropertyKey = "fieldLabCannibalized",
                Value = new ValueNode() { Bool = false }  ////determines whether we have field lab dilemma event screen
            });

            #endregion


            #region Characters

            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnConlan",
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
                        FirstName = "Ward",
                        LastName = "Conlan",
                        PersonalityType = "Conlan",
                        Portrait = "human_w_m_adult_1",
                        SimulateJoinedExpeditionNow = true,
                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {
                        AgeInYears = new NormalDistribution() { Mean = 52f },
                        CasteKey = "male",
                        RaceKey = "grey1",

                        Skills = new SerializableDictionary<string, float>()
                                            {
                                                { "bushcraft", 1f},
                                                { "hunting", 1f},
                                                { "butchering", 0.8f},
                                                { "fishing", 1f},
                                                { "foraging", 1f},
                                                { "cooking", 0.8f},
                                                { "menial", 1f},
                                                { "shooting", 1f},
                                                { "armedMelee", 1f},
                                                { "unarmedFighting", 0.8f},
                                                { "psychology", 0.1f},
                                                { "biology", 0.2f},   
                                                 { "smithing", 0.8f},
                                                 { "mechanics", 0.8f},
                                                 { "electronics", 0.7f},
                                                 { "chemistry", 0.6f},
                                                { "weaving", 0.15f},
                                                { "carpentry", 0.6f},                                          
                                                { "farming", 0.8f},
                                                { "weeding", 0.8f},
                                                { "grasping", 0.8f},
                                                { "fruitPicking", 0.8f}, 
                                                { "construction", 0.5f},       
                                                { "archery", 0.5f},    
                                                { "medicine", 0.5f},    
                                                { "sneaking", 0.5f}   
                                            }
                    },
                    NeedLevels = new SerializableDictionary<string, NeedData>() 
                        {
                            { "protein", new NeedData(){ Level = new NormalDistribution() { Mean = 0.9f, StandardDeviation = 0.02f } }},
                            { "foodEnergy", new NeedData(){ Level = new NormalDistribution() { Mean = 0.9f, StandardDeviation = 0.02f } }},
                            { "micronutrients", new NeedData(){ Level = new NormalDistribution() { Mean = 0.9f, StandardDeviation = 0.02f } }},
                            { "stimulants", new NeedData(){ Level = new NormalDistribution() { Mean = 0.4f, StandardDeviation = 0.02f } }}
                        }
                }

            });

            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnLehner",
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
                        FirstName = "Joaquin",
                        LastName = "Lehner",
                        PersonalityType = "Lehner",
                        Portrait = "human_h_m_adult_1",
                        SimulateJoinedExpeditionNow = true,
                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {
                        AgeInYears = new NormalDistribution() { Mean = 44f },
                        CasteKey = "male",
                        RaceKey = "green2",

                        Skills = new SerializableDictionary<string, float>()
                                                               {       
                                                                    { "bushcraft", 0.4f},
                                                                    { "hunting", 0.7f},
                                                                    { "butchering", 0.4f},
                                                                    { "fishing", 0.5f},
                                                                    { "foraging", 0.5f},
                                                                    { "cooking", 0.5f},
                                                                    { "menial", 0.6f},
                                                                    { "shooting", 1f},
                                                                    { "armedMelee", 0.8f},
                                                                    { "unarmedFighting", 0.9f},
                                                                    { "medicine", 0.6f},
                                                                    { "psychology", 0.8f},
                                                                     { "smithing", 0.8f},                                                                     
                                                                     { "mechanics", 0.7f},
                                                                     { "electronics", 0.6f},
                                                                     { "chemistry", 0.8f},
                                                                    { "weaving", 0.28f},
                                                                    { "carpentry", 0.4f},
                                                                    { "biology", 0.6f} ,   
                                                                   
                                                                    { "farming", 0.4f},
                                                                    { "weeding", 0.4f},
                                                                    { "grasping", 0.6f},
                                                                    { "fruitPicking", 0.5f},
                                                                    { "construction", 0.5f},      
                                                                    { "archery", 0.5f},                                                       
                                                                    { "sneaking", 0.5f}  
                                                               }
                    },
                    NeedLevels = new SerializableDictionary<string, NeedData>() 
                        {
                            { "protein", new NeedData(){ Level = new NormalDistribution() { Mean = 0.9f, StandardDeviation = 0.02f } }},
                            { "foodEnergy", new NeedData(){ Level = new NormalDistribution() { Mean = 0.9f, StandardDeviation = 0.02f } }},
                            { "micronutrients", new NeedData(){ Level = new NormalDistribution() { Mean = 0.9f, StandardDeviation = 0.02f } }},
                            { "stimulants", new NeedData(){ Level = new NormalDistribution() { Mean = 0.6f, StandardDeviation = 0.02f } }}
                        }


                }

            });


            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnYeboah",
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
                        FirstName = "Augustine",
                        LastName = "Yeboah",
                        PersonalityType = "Yeboah",
                        Portrait = "human_b_f_adult_1",
                        SimulateJoinedExpeditionNow = true
                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {
                        AgeInYears = new NormalDistribution() { Mean = 39f },
                        CasteKey = "female",
                        RaceKey = "blue2",

                        Skills = new SerializableDictionary<string, float>()
                                                                   {                                                                          
                                                                        { "bushcraft", 0.6f},
                                                                        { "hunting", 1f},
                                                                        { "butchering", 0.7f},
                                                                        { "fishing", 1f},
                                                                        { "foraging", 0.6f},
                                                                        { "cooking", 0.6f},
                                                                        { "menial", 0.6f},
                                                                        { "shooting", 1f},
                                                                        { "armedMelee", 0.6f},
                                                                        { "unarmedFighting", 0.6f},
                                                                        { "medicine", 0.7f},
                                                                        { "psychology", 0.5f},
                                                                         { "smithing", 0.8f},
                                                                        
                                                                         //new
                                                                         { "mechanics", 0.6f},
                                                                         { "electronics", 0.7f},
                                                                         { "chemistry", 0.8f},
                                                                         { "weaving", 0.18f},
                                                                         { "carpentry", 0.34f},
                                                                         //  
                                                                        { "biology", 1f},                                                                      
                                                                        { "farming", 0.6f},
                                                                        { "weeding", 0.6f},
                                                                        { "grasping", 0.8f},
                                                                        { "fruitPicking", 0.8f}, 
                                                                        { "construction", 0.5f}, 
                                                                        { "archery", 0.5f},                                                       
                                                                        { "sneaking", 0.5f}  
                                                                   }
                    },
                    NeedLevels = new SerializableDictionary<string, NeedData>() 
                        {
                            { "protein", new NeedData(){ Level = new NormalDistribution() { Mean = 0.9f, StandardDeviation = 0.02f } }},
                            { "foodEnergy", new NeedData(){ Level = new NormalDistribution() { Mean = 0.9f, StandardDeviation = 0.02f } }},
                            { "micronutrients", new NeedData(){ Level = new NormalDistribution() { Mean = 0.9f, StandardDeviation = 0.02f } }},
                            { "stimulants", new NeedData(){ Level = new NormalDistribution() { Mean = 0.5f, StandardDeviation = 0.02f } }}
                        }


                }

            });

            list.Add(new SpawnEntityAction()
            {
                KeyName = "spawnKahn",
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
                        FirstName = "Ilya",
                        LastName = "Khan",
                        PersonalityType = "Khan",
                        Portrait = "human_a_m_adult_1",
                        SimulateJoinedExpeditionNow = true,
                    },
                    BioEntity = new Maps.MapEditor.BiologicalEntity()
                    {
                        AgeInYears = new NormalDistribution() { Mean = 38f },
                        CasteKey = "male",
                        RaceKey = "red1",

                        Skills = new SerializableDictionary<string, float>()
                                        {                                                                   
                                            { "bushcraft", 0.4f},
                                            { "hunting", 0.7f},
                                            { "butchering", 0.6f},
                                            { "fishing", 0.5f},
                                            { "foraging", 0.5f},
                                            { "cooking", 0.9f},
                                            { "menial", 0.6f},
                                            { "shooting", 1f},
                                            { "armedMelee", 0.8f},
                                            { "unarmedFighting", 0.9f},
                                            { "medicine", 0.3f},
                                            { "psychology", 0.2f},
                                            { "smithing", 0.8f},   
                                            { "mechanics", 0.5f},
                                            { "electronics", 0.5f},
                                            { "chemistry", 0.6f},
                                            { "weaving", 0.28f},
                                            { "carpentry", 0.2f},                                               
                                            { "biology", 0.6f},                                           
                                            { "farming", 0.4f},
                                            { "weeding", 0.4f},
                                            { "grasping", 0.8f},
                                            { "fruitPicking", 0.8f}, 
                                            { "construction", 0.5f}, 
                                            { "archery", 0.5f},                                                       
                                            { "sneaking", 0.5f}  
                                        }
                    },
                    NeedLevels = new SerializableDictionary<string, NeedData>() 
                        {
                            { "protein", new NeedData(){ Level = new NormalDistribution() { Mean = 0.9f, StandardDeviation = 0.02f } }},
                            { "foodEnergy", new NeedData(){ Level = new NormalDistribution() { Mean = 0.9f, StandardDeviation = 0.02f } }},
                            { "micronutrients", new NeedData(){ Level = new NormalDistribution() { Mean = 0.9f, StandardDeviation = 0.02f } }},
                            { "stimulants", new NeedData(){ Level = new NormalDistribution() { Mean = 0.3f, StandardDeviation = 0.02f } }}
                        }


                }

            });

            #endregion

            #region Particles

            list.Add(
                new ParticleEffectAction()
                {
                    KeyName = "blackSmoke",
                    DelayInSeconds = delayForItemsAndAgents,

                    UseLocationOfEntity = new TargetObject()
                        {
                            TargetObjectType = TargetObjectType.Root,
                            GetList = new GetList() { HasPropertiesListKey = "entities", FilterCondition = new PropertyCondition() { PropertyKey = "type", ConstantStringEqual = "structure:skimmerHull" } }
                        },
                    /*  DynamicLocation = new DynamicLocation()
                      {
                          PropertyKey = "location",
                          TargetObject = new TargetObject()
                          {
                              TargetElement = TargetObjectType.Root,
                              GetList = new GetList() { HasPropertiesListKey = "entities", FilterCondition = new PropertyCondition() { PropertyKey = "type", StringEqual = "structure:skimmerHull" } }
                          }
                      },*/

                    DurationInSeconds = 20, // lasts x seconds
                    ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "signalSmoke" } }

                });

            //   The.Client.ParticleManager.AddEmitter("haze", MapManager.TileToWorldPosVector2(new Point(3, 59)), 10f, null);




            list.Add(
              new ParticleEffectAction()
              {
                  KeyName = "smallFog1",     //Little ""steam" rising from pond      

                  Location = new ValueNode()
                  {
                      Location = Maps.MapManager.TileToWorldPosVector2(new Point(20, 49))
                  },
                  ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "smallFog" } }

              });

            list.Add(
              new ParticleEffectAction()
              {
                  KeyName = "smallFog2",     //Little ""steam" rising from pond      

                  Location = new ValueNode()
                  {
                      Location = Maps.MapManager.TileToWorldPosVector2(new Point(18, 43))
                  },
                  ParticleEmitters = new[] { new ParticleEmitterEffect() { ParticleSystemKey = "smallFog" } }

              });

            list.Add(
              new ParticleEffectAction()
              {
                  KeyName = "smallFog3",     //Little ""steam" rising from pond      

                  Location = new ValueNode()
                  {
                      Location = Maps.MapManager.TileToWorldPosVector2(new Point(19, 46))
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
                  KeyName = "sulphurousSmoke2",     //sulphurous lakes          

                  TimeBetweenEmissions = 0.4f,
                  Location = new ValueNode()
                  {
                      Location = Maps.MapManager.TileToWorldPosVector2(new Point(41, 25))
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
                      Location = Maps.MapManager.TileToWorldPosVector2(new Point(39, 27))
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
                    PolicyData = new Policies.ExpeditionPolicyData()
                    {                       
                        FractionIndependentsAllowedToSleep = 0.5f,
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

            list.Add(new SetPropertyAction()
            {
                KeyName = "setStartingLocationSouth",

                PropertyKey = "startingLocation",
                Value = new ValueNode() { Location = new Microsoft.Xna.Framework.Vector2(576f, 2400f) }// 624f, 2448f

            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "setStartingLocationNorth",

                // has to be set first, because the other spawns depend on the value!
                PropertyKey = "startingLocation",
                Value = new ValueNode() { Location = new Microsoft.Xna.Framework.Vector2(1948f, 744f) } //  MP: was: 1708f, 800f

            });


            list.Add(new SetPropertyAction()
            {
                KeyName = "setStartingLocationSouthEast",

                // has to be set first, because the other spawns depend on the value!
                PropertyKey = "startingLocation",
                Value = new ValueNode() { Location = new Microsoft.Xna.Framework.Vector2(2304f, 2400f) }

            });





            /*
                                    new EventActionType() { DelayInSeconds = 7,
                                    SpawnEntity = new SpawnEntityAction() {EntityData = new EntityData()
                                        { EntityKey = "entity:twinkler",  Location = new Vector3(720, 1536, 0), Bulk = 0.25f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                        { AgeInYears = 17, CasteKey = "guard", AllegianceName = "twinklerAllegiance", } }, }},

            */

            #endregion

            #region Game duration

            //These numbers will work like the relative numbers already do in the TimeCondition.
            list.Add(new SetPropertyAction()
            {
                KeyName = "setWinGameEarly",

                PropertyKey = "winGameTime",
                Value = new ValueNode() { Decimal = 3f },

            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "setWinGameMedium",

                PropertyKey = "winGameTime",
                Value = new ValueNode() { Decimal = 5f }, //mp feb 2015: 6f in-game days is a lot of hours with the current content.  ### Bso changed from 5 for testing

            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "setWinGameLate",

                PropertyKey = "winGameTime",
                Value = new ValueNode() { Decimal = 8f }

            });
            #endregion

            #region Set spawn intervals and pop caps

            // instead of setting each cave explicitly, this could also be made as an added number that affects the two caves.
            // Interval speeds //
            list.Add(new SetPropertyAction() //bso
            {
                KeyName = "setRatNestSpawnIntervalOften",

                PropertyKey = "RatNestSpawnInterval",
                Value = new ValueNode() { Int = 1600 }

            });
            list.Add(new SetPropertyAction() //bso
            {
                KeyName = "setRatNestSpawnIntervalSeldom",

                PropertyKey = "RatNestSpawnInterval",
                Value = new ValueNode() { Int = 3200 }

            });
            list.Add(new SetPropertyAction()
            {
                KeyName = "setTwinklerSpawnIntervalSouthOften",

                PropertyKey = "twinklerSpawnIntervalSouth",
                Value = new ValueNode() { Int = 360 }, //sandstone

            });
            list.Add(new SetPropertyAction()
            {
                KeyName = "setTwinklerSpawnIntervalEastOften",

                PropertyKey = "twinklerSpawnIntervalEast",
                Value = new ValueNode() { Int = 260 }, //

            });
            list.Add(new SetPropertyAction()
            {
                KeyName = "setTwinklerSpawnIntervalSouthSeldom",

                PropertyKey = "twinklerSpawnIntervalSouth", //sandstone
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
                KeyName = "setThunderChickenSpawnIntervalLow",

                PropertyKey = "thunderChickenSpawnInterval",
                Value = new ValueNode() { Int = 1600 }

            });
            list.Add(new SetPropertyAction()
            {
                KeyName = "setThunderChickenSpawnIntervalNormal",

                PropertyKey = "thunderChickenSpawnInterval",
                Value = new ValueNode() { Int = 1200 }

            });
            list.Add(new SetPropertyAction()
            {
                KeyName = "setThunderChickenSpawnIntervalHigh",

                PropertyKey = "thunderChickenSpawnInterval",
                Value = new ValueNode() { Int = 800 }

            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "setBinalRatSpawnSeldom",

                PropertyKey = "binalRatSpawnInterval",
                Value = new ValueNode() { Int = 120 }, //bso increased from 70 to 120 to give the humans a chance to lower the binalrat population for a bit //spawns are now small, only 1-2 rats per spawn, so... before with big spawns (8 rats), was 560

            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "setBinalRatSpawnOften",

                PropertyKey = "binalRatSpawnInterval",
                Value = new ValueNode() { Int = 60 }, //bso increased from 32 to 60 //spawns are now small, only 1-2 rats per spawn, so... before with big spawns (8 rats), was 260

            });
            list.Add(new SetPropertyAction()
            {
                KeyName = "setThinThunderChickenSpawnIntervalSeldom",

                PropertyKey = "thinThunderChickenSpawnInterval",
                Value = new ValueNode() { Int = 400 }

            });
            list.Add(new SetPropertyAction()
            {
                KeyName = "setThinThunderChickenSpawnIntervalOften",

                PropertyKey = "thinThunderChickenSpawnInterval",
                Value = new ValueNode() { Int = 200 }

            });
            // Max values //
            list.Add(new SetPropertyAction()
            {
                KeyName = "setMaxThinThunderChickenLow",

                PropertyKey = "maxThinThunderChicken",
                Value = new ValueNode() { Int = 1 }

            });
            list.Add(new SetPropertyAction()
            {
                KeyName = "setMaxThinThunderChickenNormal",

                PropertyKey = "maxThinThunderChicken",
                Value = new ValueNode() { Int = 2 }

            });
            list.Add(new SetPropertyAction()
            {
                KeyName = "setMaxThinThunderChickenHigh",

                PropertyKey = "maxThinThunderChicken",
                Value = new ValueNode() { Int = 3 }

            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "setMaxTwinklersLow",

                PropertyKey = "maxTwinklers",
                Value = new ValueNode() { Int = 4 }, // mp feb 2015: increased from 2
                // on easy, there are only hunters spawned. this setting ensures that player will only get attacked by maximum of 4 hunters

            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "setMaxTwinklersNormal",

                PropertyKey = "maxTwinklers",
                Value = new ValueNode() { Int = 12 }, //

            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "setMaxTwinklersHigh",

                PropertyKey = "maxTwinklers",
                Value = new ValueNode() { Int = 25 }, //

            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "setMaxThunderChickensLow",

                PropertyKey = "maxThunderChickens",
                Value = new ValueNode() { Int = 1 }

            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "setMaxThunderChickensNormal",

                PropertyKey = "maxThunderChickens",
                Value = new ValueNode() { Int = 2 }, //Bso Reduced all the thunderchicken max values to what the values was in the spawners to replace them, so that these values once again can be taken in use

            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "setMaxThunderChickensHigh",

                PropertyKey = "maxThunderChickens",
                Value = new ValueNode() { Int = 3 }

            });


            list.Add(new SetPropertyAction()
            {
                KeyName = "setMaxBinalRatsLow",

                PropertyKey = "maxBinalRats",
                Value = new ValueNode() { Int = 3 }, //per allegiance. we have currently 3 allegiances

            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "setMaxBinalRatsNormal",

                PropertyKey = "maxBinalRats",
                Value = new ValueNode() { Int = 6 }, //per allegiance. we have currently 3 allegiances

            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "setMaxBinalRatsHigh",

                PropertyKey = "maxBinalRats",
                Value = new ValueNode() { Int = 10 }, //per allegiance. we have currently 3 allegiances

            });
            #endregion

            #region Farmplots and fish spots

            #region Farmplots
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFarmSpotSmall1",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:smallPlotSpot",
                    Name = "Small plot ()",
                    Location = new Vector3(2438f, 2574f, 0)
                },


            });
            #endregion

            #region Fish weir spots
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFishTrapCreek1",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:fishTrapSpotCreek",
                    Name = "Fish weir spot ()",
                    Location = new Vector3(2424f, 1728f, 0)
                }

            });
            #endregion

            #region Fish Trap Saltwater Spots (Coast)

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFishTrapCoast1",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:fishTrapSpotCoast",
                    Name = "Fish trap spot Saltwater ()",
                    Location = new Vector3(2661f, 1395f, 0)
                }

            });

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFishTrapCoast2",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:fishTrapSpotCoast",
                    Name = "Fish trap spot Saltwater ()",
                    Location = new Vector3(336f, 1814f, 0)
                }

            });

            #endregion

            #region Fish Trap Freshwater Spots (Shore)

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFishTrapShore1",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:fishTrapSpotShore",
                    Name = "Fish trap spot Freshwater ()",
                    Location = new Vector3(1086f, 832f, 0)
                }

            });


            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFishTrapShore2",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:fishTrapSpotShore",
                    Name = "Fish trap spot Freshwater ()",
                    Location = new Vector3(2207f, 1646f, 0)
                }

            });

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFishTrapShore3",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:fishTrapSpotShore",
                    Name = "Fish trap spot Freshwater ()",
                    Location = new Vector3(980f, 1331f, 0)
                }

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
                    NoiseAmplitude = 1f,//old: 1.5f,
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
                    NoiseAmplitude = 1f,//old: 1.5f,
                    NoiseAddend = -2f,
                    NoiseFrequency = 0.035f // high means "white noise". 0.01f not useful, just a big gradient......0.05f kinda bigger
                }

            });

            list.Add(new ChangeResourcesAction()
            {
                KeyName = "setPlentyResources",


                AllResources = true,
                ExcludeResourceTypes = new[] { "stones", "firegrassSod", "crop:sticks", "crop:pigFlies", "vine", "sulfurDeposit", "streakFin", "carbonTail", "alabasterRay", "daggermouth", "clamwich", "torux", "minnowsLive", "phantomWeaver", "ursinix", "webWing", "crestedFoiler", "goldenCenobite", "muckGrinder", "crop:daysheenLeaves", "crop:waterCaneStem", "crop:shadeleafCanes" }, //MP I want "stones", "firegrassSod" to follow the landscape sprites, so no variation there.  Sulfur part of scenario cannot risk having none. Daysheen leaves, watercane and shadeleaf canes are too rare to risk being cancelled out.
                OperationToUse = ChangeResourcesAction.Operation.Multiply,
                //   NoiseAmplitude = 0.5f,
                NoiseParameters = new NoiseParams()
                {
                    NoiseAddend = 0.175f, //bso old: 0.2f// add 'X %' 
                    NoiseAmplitude = 0.75f,
                    NoiseFrequency = 0.1f//bso old: 0.1 // high means "white noise"
                }

            });

            list.Add(new ChangeResourcesAction()
            {
                KeyName = "setNormalResources",


                AllResources = true,
                ExcludeResourceTypes = new[] { "stones", "firegrassSod", "crop:sticks", "vine", "sulfurDeposit", "streakFin", "carbonTail", "alabasterRay", "daggermouth", "clamwich", "torux", "minnowsLive", "phantomWeaver", "ursinix", "webWing", "crestedFoiler", "goldenCenobite", "muckGrinder", "crop:daysheenLeaves", "crop:waterCaneStem", "crop:shadeleafCanes" }, // same as above
                OperationToUse = ChangeResourcesAction.Operation.Multiply,

                NoiseParameters = new NoiseParams()
                {
                    NoiseAddend = 0.15f,
                    NoiseAmplitude = 0.75f,
                    NoiseFrequency = 0.1f
                }

                //  NoiseAmplitude = 0.5f,
                // NoiseAddend = 1f // neutral variation

            });

            list.Add(new ChangeResourcesAction()
            {
                KeyName = "setSparseResources",


                AllResources = true,
                ExcludeResourceTypes = new[] { "stones", "firegrassSod", "crop:sticks", "vine", "sulfurDeposit", "streakFin", "carbonTail", "alabasterRay", "daggermouth", "clamwich", "torux", "minnowsLive", "phantomWeaver", "ursinix", "webWing", "crestedFoiler", "goldenCenobite", "muckGrinder", "crop:daysheenLeaves", "crop:waterCaneStem", "crop:shadeleafCanes" },// same as above
                OperationToUse = ChangeResourcesAction.Operation.Multiply,
                //   NoiseAmplitude = 0.5f,
                NoiseParameters = new NoiseParams()
                   {
                       NoiseAddend = 0.1f,//bso old-0.4 //minus 'X %'     MP may23, was: -0.2f  
                       NoiseAmplitude = 0.5f,
                       NoiseFrequency = 0.1f
                   }

            });

            #endregion

            #region Natural terminals

            list.Add(new SpawnEntityAction()
            {
                KeyName = "naturalTerminalSW",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:naturalPseudoWaterTerminal",
                    Name = "To Knoll Island",  //so the player knows what the characters are talking about. was "Strait crossing"                       
                    Location = new Vector3(326f, 2496f, 0f) //was 315f, 2300f, 0f

                }

            });

            list.Add(new SpawnEntityAction()
            {
                KeyName = "naturalTerminalN",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:naturalPseudoWaterTerminal",
                    Name = "To Knoll Island", //was "Strait crossing"
                    Location = new Vector3(2090, 400f, 0f) //was 2290, 506f, 0f
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

                        ViewLongitudeStart = 11, // 
                        ViewLongitudeEnd = 15,
                        ViewLatitudeStart = 69,
                        ViewLatitudeEnd = 72
                    }

                });

            #endregion

            #region spawnWildernessSite1

            list.Add(
               new SpawnSiteAction()
               {
                   KeyName = "spawnWildernessSite1",


                   SiteData = new SiteData()
                   {
                       Name = "Knoll Island",
                       KeyName = "wildernessSite1",
                       Description = "A tiny island some kilometers away. Just might have better conditions, but we wouldn't know unless we crossed the water somehow.",
                       Coords = new Overland.Locations.GeodeticCoordinate(13.08d, 70.755d),
                       IsPlaySite = false,
                       ShowLabel = false,
                       ShowTallPin = true
                   }


               });

            list.Add(
               new SpawnAllegianceAction()
               {
                   KeyName = "spawnWildernessSite1Allegiance1",

                   Site = "wildernessSite1",
                   AllegianceData = new AllegianceData()
                   {
                       Name = "",
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


            list.Add(
                new SpawnSiteAction()
                {
                    KeyName = "spawnPlaySite",

                    SiteData = new SiteData()
                    {
                        Name = "Twinkler Island",
                        KeyName = "playSite",
                        Description = "Where we've crashed: An uncharted, vulcanic island. The biome seems to support a substantial amount of larger predators.",
                        Coords = new Overland.Locations.GeodeticCoordinate(13.2d, 70.68d),
                        IsPlaySite = true,
                        ShowLabel = true,
                        ShowTallPin = false,
                        SiteMarkerOrder = 10
                    }


                });

            #region Routes
            list.Add(
                new SpawnRouteAction()
                {
                    KeyName = "spawnPlaySiteWildernessSite1Route",
                    DelayInSeconds = 1,

                    RouteData = new RouteData()
                    {
                        Name = "Strait crossing",
                        FromSite = "playSite",
                        ToSite = "wildernessSite1",
                        Length = 8,
                        RouteType = RouteType.Land
                    }

                });

            #endregion

            /* // mp not necessary, it will autogenerate an expedition:
 
            list.Add(
                new EventActionType()
                {
                    KeyName = "DEMOISLANDMAP_spawnBushDragonAllegianceSouthEast", // mp new key dec 1 2014
                    
                        Site = "playSite",

                        ExpeditionData = new ExpeditionData()
                        {
                            Name = "bushDragonAllegianceSouthEast",
                            AllegianceKey = "bushDragonAllegianceSouthEast",
                            LocationOffset = new Vector2(2160, 2400)
                        },

                        AllegianceData = new AllegianceData()
                        {



                            ForageAndHuntingRadius = 300,


                            Name = "Bush Dragon Allegiance",
                            Key = "bushDragonAllegianceSouthEast",
                            EntityType = "entity:bushDragon",
                            AllegianceType = Allegiances.AllegianceType.Other,
                            PossibleEmigrants = null,
                            StatsData = new StatsData()
                            {
                                Security = 1f,
                                Comfort = 1f,
                                FoodSupply = 1f,
                            }
                        }
                    }
                });

*/

            list.Add(
                new SpawnAllegianceAction()
                {
                    KeyName = "DEMOISLANDMAP_spawnThunderChickenAllegianceNorth",

                    Site = "playSite",

                    ExpeditionData = new ExpeditionData()
                    {
                        KeyName = "thunderChickenAllegianceNorth",
                        Name = "thunderChickenAllegianceNorth",
                        AllegianceKey = "thunderChickenAllegianceNorth",
                        Location = new ValueNode()
                        {
                            Location = new Vector2(816, 1392)
                        }
                    },

                    AllegianceData = new AllegianceData()
                    {
                        ForageAndHuntingRadius = 600,
                        Name = "Thunderchicken Allegiance",
                        KeyName = "thunderChickenAllegianceNorth",
                        EntityType = "entity:pygmyThunderChicken",
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
                new SpawnAllegianceAction() //bso
                {
                    KeyName = "DEMOISLANDMAP_thinThunderChickenAllegianceSouth",

                    Site = "playSite",

                    ExpeditionData = new ExpeditionData()
                    {
                        KeyName = "thinThunderChickenAllegianceSouth",
                        Name = "thinThunderChickenAllegianceSouth",
                        AllegianceKey = "thinThunderChickenAllegianceSouth",
                        Location = new ValueNode()
                        {
                            Location = new Vector2(960, 2640)
                        }
                    },

                    AllegianceData = new AllegianceData()
                    {
                        ForageAndHuntingRadius = 1200, //bso Hopefully this cover a big area of the island
                        Name = "Thin Thunder Chicken Allegiance South",
                        KeyName = "thinThunderChickenAllegianceSouth",
                        EntityType = "entity:bajingan",
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
                new SpawnAllegianceAction() //bso
                {
                    KeyName = "DEMOISLANDMAP_thinThunderChickenAllegianceNorth",

                    Site = "playSite",

                    ExpeditionData = new ExpeditionData()
                    {
                        KeyName = "thinThunderChickenAllegianceNorth",
                        Name = "thinThunderChickenAllegianceNorth",
                        AllegianceKey = "thinThunderChickenAllegianceNorth",
                        Location = new ValueNode()
                        {
                            Location = new Vector2(2688, 816)
                        }
                    },

                    AllegianceData = new AllegianceData()
                    {



                        ForageAndHuntingRadius = 1200,


                        Name = "Thin Thunder Chicken Allegiance North",
                        KeyName = "thinThunderChickenAllegianceNorth",
                        EntityType = "entity:bajingan",
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
                    KeyName = "DEMOISLANDMAP_spawnThunderChickenAllegianceSouth",

                    Site = "playSite",

                    ExpeditionData = new ExpeditionData()
                    {
                        KeyName = "thunderChickenAllegianceSouth",
                        Name = "thunderChickenAllegianceSouth",
                        AllegianceKey = "thunderChickenAllegianceSouth",
                        Location = new ValueNode()
                        {
                            Location = new Vector2(1632, 2064) //1248, 1104
                        }
                    },

                    AllegianceData = new AllegianceData()
                    {
                        ForageAndHuntingRadius = 600,

                        Name = "Thunderchicken Allegiance",
                        KeyName = "thunderChickenAllegianceSouth",
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

            list.Add(
                new SpawnAllegianceAction()
                {
                    KeyName = "DEMOISLANDMAP_spawnTwinklerAllegiance", //big circle that encompasses most of map apart from amager island . mp new key dec 9 2014

                    Site = "playSite",
                    ExpeditionData = new ExpeditionData()
                    {
                        Name = "twinklerAllegiance",
                        AllegianceKey = "twinklerAllegiance",
                        Location = new ValueNode()
                        {
                            Location = new Vector2(1296, 1464)
                        }
                    },
                    AllegianceData = new AllegianceData()
                    {
                        ForageAndHuntingRadius = 1082,
                        Name = "Twinkler Allegiance",
                        KeyName = "twinklerAllegiance",
                        EntityType = "entity:twinkler",
                        AllegianceType = Allegiances.AllegianceType.Other,
                        StatsData = new StatsData()
                        {
                            Security = 1f,
                            Comfort = 1f,
                            FoodSupply = 1f,
                        }
                    }

                });


            /*
                        list.Add(
                            new EventActionType()
                            {
                                KeyName = "DEMOISLANDMAP_spawnTwinklerAllegianceSouth", // dont want 2 allegiances that fight eachother //mp new key dec 2 2014
                    
                                    Site = "playSite",

                                    ExpeditionData = new ExpeditionData()
                                    {
                                        Name = "twinklerAllegianceSouth",
                                        AllegianceKey = "twinklerAllegianceSouth",
                                        LocationOffset = new Vector2(720, 1872)
                                    },

                                    AllegianceData = new AllegianceData()
                                    {



                                        ForageAndHuntingRadius = 672,


                                        Name = "Twinkler Allegiance South",
                                        Key = "twinklerAllegianceSouth",
                                        EntityType = "entity:twinkler",
                                        AllegianceType = Allegiances.AllegianceType.Other,
                                        PossibleEmigrants = null,
                                        StatsData = new StatsData()
                                        {
                                            Security = 1f,
                                            Comfort = 1f,
                                            FoodSupply = 1f,
                                        }
                                    }
                                }
                            });


                        list.Add(
                            new EventActionType()
                            {
                                KeyName = "DEMOISLANDMAP_spawnTwinklerAllegianceEast", //mp new key dec 2 2014
                    
                                    Site = "playSite",

                                    ExpeditionData = new ExpeditionData()
                                    {
                                        Name = "twinklerAllegianceEast",
                                        AllegianceKey = "twinklerAllegianceEast",
                                        LocationOffset = new Vector2(1968, 1296)
                                    },

                                    AllegianceData = new AllegianceData()
                                    {



                                        ForageAndHuntingRadius = 672,


                                        Name = "Twinkler Allegiance East",
                                        Key = "twinklerAllegianceEast",
                                        EntityType = "entity:twinkler",
                                        AllegianceType = Allegiances.AllegianceType.Other,
                                        PossibleEmigrants = null,
                                        StatsData = new StatsData()
                                        {
                                            Security = 1f,
                                            Comfort = 1f,
                                            FoodSupply = 1f,
                                        }
                                    }
                                }
                            });

            */

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

            #region Place nests

            list.Add(new SpawnEntityAction()
            {
                KeyName = "placeNestSandstone", //"southSandstoneCaveNest"
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:quaditeNest",
                    Name = "Quadite nest (coord. 15;34)",
                    Location = new Vector3(732, 1510, 0),
                    Threat = new Threat() { ThreatGroupName = "twinklerAllegiance" }
                }

            });

            list.Add(new SpawnEntityAction()
            {
                KeyName = "placeNestRockEast",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:quaditeNest",
                    Name = "Quadite nest (coord. 38;38)",
                    Location = new Vector3(1800, 1800, 0),
                    Threat = new Threat() { ThreatGroupName = "twinklerAllegiance" }
                }

            });


            list.Add(new SpawnEntityAction()
            {
                KeyName = "placeNestRockSouth",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:quaditeNest",
                    Name = "Quadite nest (coord. 25;44)",
                    Location = new Vector3(1220, 2130, 0),
                    Threat = new Threat() { ThreatGroupName = "twinklerAllegiance" }
                }

            });


            list.Add(new SpawnEntityAction()
            {
                KeyName = "placeNestRockNorth",
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:quaditeNest",
                    Name = "Quadite nest (coord. 36;20)",
                    Location = new Vector3(1728, 980, 0),
                    Threat = new Threat() { ThreatGroupName = "twinklerAllegiance" }
                }

            });


            #endregion

            #region Place crates/supplies in crevice

            list.Add(new SpawnEntityAction()
            {
                KeyName = "placeCratesSandstone", //at sandstone. make one for rocks also?
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:crates",
                    Name = "Crates",
                    Location = new Vector3(552, 1397, 0) //outside blocked terrain
                }
            });

            list.Add(new SpawnEntityAction()
            {
                KeyName = "placeUnconsciousAtBramble", // 
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:unconscious",
                    Name = "Unconscious",
                    Location = new Vector3(1400, 1710, 0) //1392, 1728, 0  outside blocked terrain
                }
            });


            list.Add(new SpawnEntityAction()
            {
                KeyName = "placeUnconsciousAtSandstone", // 
                DelayInSeconds = 1,

                EntityData = new EntityData()
                {
                    EntityKey = "terrain:unconscious",
                    Name = "Unconscious",
                    Location = new Vector3(552, 1397, 0) //  outside blocked terrain
                }
            });



            #endregion

            #region set rescue spawn location


            list.Add(new SetPropertyAction()
            {
                KeyName = "setRescueSpawnLocationSandstone", //determines where the rescued dude shows up.

                PropertyKey = "rescueSpawnLocation",
                Value = new ValueNode() { Location = new Microsoft.Xna.Framework.Vector2(480f, 1392f) }//

            });

            list.Add(new SetPropertyAction()
            {
                KeyName = "setRescueSpawnLocationBramble",

                PropertyKey = "rescueSpawnLocation",
                Value = new ValueNode() { Location = new Microsoft.Xna.Framework.Vector2(1392f, 1776f) }//

            });

            #endregion



            #region view and fog of war


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

                KeyName = "exploreShroudNorth",
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
                OffsetLocationEnd = new Microsoft.Xna.Framework.Vector2(324f, 1500f),  //324f, 800f, 

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

                //  TargetEntityOfAction.SpecifiedEntity,
                //  EntityName = "Ward Conlan"

            });

            list.Add(new ExploreAction()
            {

                KeyName = "exploreShroudSouth",// used in scenario where they start next to hull on south west island

                DelayInSeconds = delayForItemsAndAgents + 1, // must execute last, after members have been added

                DynamicLocationStart = new ValueNode()
                {
                    PropertyKey = "startingLocation"
                },
                RadiusStart = 300f,
                RadiusEnd = 500f,
                DetectMode = InGameEvents.Actions.DetectMode.DetectAlwaysSeenEntities,
                //PerformDetection = true,
                OffsetLocationEnd = new Microsoft.Xna.Framework.Vector2(324f, 2648f),// TODO

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


            list.Add(new ExploreAction()
            {

                KeyName = "exploreShroudFledSouthWestWreckAtSandstone",
                DelayInSeconds = delayForItemsAndAgents + 1, // must execute last, after members have been added

                DynamicLocationStart = new ValueNode()
                {
                    PropertyKey = "startingLocation"
                },
                RadiusStart = 300f,
                RadiusEnd = 300f,
                DetectMode = InGameEvents.Actions.DetectMode.DetectAlwaysSeenEntities,
                //PerformDetection = true,
                OffsetLocationEnd = new Microsoft.Xna.Framework.Vector2(1104f, 1392f),// TODO

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

            });

            list.Add(new ExploreAction()
            {

                KeyName = "exploreShroudFledSouthWestWreckAtSandstoneFlightPath", //the plane's flight path
                DelayInSeconds = delayForItemsAndAgents + 1, // must execute last, after members have been added

                OffsetLocationStart = new Microsoft.Xna.Framework.Vector2(48f, 1344f),

                RadiusStart = 300f,
                RadiusEnd = 300f,
                //PerformDetection = true,
                DetectMode = InGameEvents.Actions.DetectMode.DetectAlwaysSeenEntities,
                OffsetLocationEnd = new Microsoft.Xna.Framework.Vector2(1104f, 1392f),// wreck location

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

            });

            ////

            list.Add(new ExploreAction()
            {

                KeyName = "exploreShroudFledSouthWestWreckAtBramble",
                DelayInSeconds = delayForItemsAndAgents + 1, // must execute last, after members have been added

                DynamicLocationStart = new ValueNode()
                {
                    PropertyKey = "startingLocation"
                },
                RadiusStart = 300f,
                RadiusEnd = 300f,
                DetectMode = InGameEvents.Actions.DetectMode.DetectAlwaysSeenEntities,
                //PerformDetection = true,
                OffsetLocationEnd = new Microsoft.Xna.Framework.Vector2(1872f, 1680f),// TODO

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

            });

            list.Add(new ExploreAction()
            {

                KeyName = "exploreShroudWreckAtBrambleFlightPath",//the flight path
                DelayInSeconds = delayForItemsAndAgents + 1, // must execute last, after members have been added

                OffsetLocationStart = new Microsoft.Xna.Framework.Vector2(48f, 2000f),

                RadiusStart = 300f,
                RadiusEnd = 300f,
                DetectMode = InGameEvents.Actions.DetectMode.DetectAlwaysSeenEntities,
                //PerformDetection = true,
                OffsetLocationEnd = new Microsoft.Xna.Framework.Vector2(1872f, 1680f),// TODO

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

            });

            ///////////

            list.Add(new ExploreAction()
            {

                KeyName = "exploreShroudFledSouthEastWreckAtBramble",
                DelayInSeconds = delayForItemsAndAgents + 1, // must execute last, after members have been added

                DynamicLocationStart = new ValueNode()
                {
                    PropertyKey = "startingLocation"
                },
                RadiusStart = 300f,
                RadiusEnd = 300f,
                DetectMode = InGameEvents.Actions.DetectMode.DetectAlwaysSeenEntities,
                //PerformDetection = true,
                OffsetLocationEnd = new Microsoft.Xna.Framework.Vector2(1872f, 1680f),// TODO

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

            });





            #endregion


            #region Destroy Skimmer structure


            list.Add(new DestroyEntityAction()
            {
                KeyName = "destroySkimmerHull",
                DelayInSeconds = 1,

                TargetObject = new InGameEvents.PropertyObjects.TargetObject()
                {
                    GetList = new InGameEvents.PropertyObjects.GetList()
                    {
                        HasPropertiesListKey = "entities",
                        FilterCondition = new InGameEvents.Conditions.PropertyCondition()
                        {
                            PropertyKey = "type",
                            ConstantStringEqual = "structure:skimmerHull"
                        },
                        // FilterProperty = "type",
                        // FilterValue = "structure:skimmerHull",
                        NextList = new InGameEvents.PropertyObjects.GetList()
                        {
                            HasPropertiesListKey = "itemParts", // get a part to destroy
                            FilterCondition = new InGameEvents.Conditions.PropertyCondition()
                            {
                                PropertyKey = "type",
                                ConstantStringEqual = "item:scrapMetal"
                            }

                        }
                    }
                }

            });


            list.Add(new DestroyEntityAction()
            {
                KeyName = "destroySkimmerEngineSide",
                DelayInSeconds = 1,

                TargetObject = new InGameEvents.PropertyObjects.TargetObject()
                {
                    GetList = new InGameEvents.PropertyObjects.GetList()
                    {
                        HasPropertiesListKey = "entities",
                        FilterCondition = new InGameEvents.Conditions.PropertyCondition()
                        {
                            PropertyKey = "type",
                            ConstantStringEqual = "structure:skimmerEngineSide"
                        },
                        // FilterProperty = "type",
                        // FilterValue = "structure:skimmerHull",
                        NextList = new InGameEvents.PropertyObjects.GetList()
                        {
                            HasPropertiesListKey = "itemParts", // get a part to destroy
                            FilterCondition = new InGameEvents.Conditions.PropertyCondition()
                            {
                                PropertyKey = "type",
                                ConstantStringEqual = "item:scrapMetal"
                            }

                        }
                    }
                }

            });

            list.Add(new DestroyEntityAction()
            {
                KeyName = "destroySkimmerEngineTop",
                DelayInSeconds = 1,

                TargetObject = new InGameEvents.PropertyObjects.TargetObject()
                {
                    GetList = new InGameEvents.PropertyObjects.GetList()
                    {
                        HasPropertiesListKey = "entities",
                        FilterCondition = new InGameEvents.Conditions.PropertyCondition()
                        {
                            PropertyKey = "type",
                            ConstantStringEqual = "structure:skimmerEngineTop"
                        },
                        // FilterProperty = "type",
                        // FilterValue = "structure:skimmerHull",
                        NextList = new InGameEvents.PropertyObjects.GetList()
                        {
                            HasPropertiesListKey = "itemParts", // get a part to destroy
                            FilterCondition = new InGameEvents.Conditions.PropertyCondition()
                            {
                                PropertyKey = "type",
                                ConstantStringEqual = "item:scrapMetal"
                            }

                        }
                    }
                }

            });

            list.Add(new DestroyEntityAction()
            {
                KeyName = "destroySkimmerTail",
                DelayInSeconds = 1,

                TargetObject = new InGameEvents.PropertyObjects.TargetObject()
                {
                    GetList = new InGameEvents.PropertyObjects.GetList()
                    {
                        HasPropertiesListKey = "entities",
                        FilterCondition = new InGameEvents.Conditions.PropertyCondition()
                        {
                            PropertyKey = "type",
                            ConstantStringEqual = "structure:skimmerTail"
                        },
                        // FilterProperty = "type",
                        // FilterValue = "structure:skimmerHull",
                        NextList = new InGameEvents.PropertyObjects.GetList()
                        {
                            HasPropertiesListKey = "itemParts", // get a part to destroy
                            FilterCondition = new InGameEvents.Conditions.PropertyCondition()
                            {
                                PropertyKey = "type",
                                ConstantStringEqual = "item:scrapMetal"
                            }

                        }
                    }
                }

            });



            #region skimmer at south coast //

            ///////////  //skimmer at south coast: //

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startSkimmerHullSouth", //relative position:   engine top: (-16, -48)   engine side: (+32, +32)
                DelayInSeconds = delayForStructures, // 0,

                EntityData = new EntityData()
                {
                    Name = "Aircraft wreck (hull)",
                    EntityKey = "structure:skimmerHull",
                    OwnedBy = new AllegianceAndExpedition() { AllegianceKey = "playerAllegiance" },
                    Location = new Vector3(672, 2450, 0), //672, 2480
                }

            });

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startSkimmerEngineTopSouth",
                DelayInSeconds = delayForStructures, //0,

                EntityData = new EntityData()
                {
                    EntityKey = "structure:skimmerEngineTop",
                    OwnedBy = new AllegianceAndExpedition() { AllegianceKey = "playerAllegiance" },
                    Location = new Vector3(656, 2402, 0), //656, 2432
                }

            });

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startSkimmerEngineSideSouth",
                DelayInSeconds = delayForStructures, //0,

                EntityData = new EntityData()
                {
                    EntityKey = "structure:skimmerEngineSide",
                    OwnedBy = new AllegianceAndExpedition() { AllegianceKey = "playerAllegiance" },
                    Location = new Vector3(704, 2482, 0), //704, 2512
                }

            });


            list.Add(new SpawnEntityAction()
            {
                KeyName = "startSkimmerTailSouth",
                DelayInSeconds = delayForStructures, //0,

                EntityData = new EntityData()
                {
                    EntityKey = "structure:skimmerTail",
                    OwnedBy = new AllegianceAndExpedition() { AllegianceKey = "playerAllegiance" },
                    Location = new Vector3(576, 2450, 0), //576, 2480,
                }

            });



            #endregion



            #region skimmer at north coast //



            list.Add(new SpawnEntityAction()
            {
                KeyName = "startSkimmerHullNorth", //relative position:   engine top: (-16, -48)   engine side: (+32, +32)
                DelayInSeconds = delayForStructures, // 0,

                EntityData = new EntityData()
                {
                    Name = "Aircraft wreck (hull)",
                    EntityKey = "structure:skimmerHull",
                    OwnedBy = new AllegianceAndExpedition() { AllegianceKey = "playerAllegiance" },
                    Location = new Vector3(2036, 778, 0),
                }
            });

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startSkimmerEngineTopNorth",
                DelayInSeconds = delayForStructures, //0,

                EntityData = new EntityData()
                {
                    EntityKey = "structure:skimmerEngineTop",
                    OwnedBy = new AllegianceAndExpedition() { AllegianceKey = "playerAllegiance" },
                    Location = new Vector3(2020, 730, 0), //
                }
            });

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startSkimmerEngineSideNorth",
                DelayInSeconds = delayForStructures, //0,

                EntityData = new EntityData()
                {
                    EntityKey = "structure:skimmerEngineSide",
                    OwnedBy = new AllegianceAndExpedition() { AllegianceKey = "playerAllegiance" },
                    Location = new Vector3(2068, 810, 0), //
                }
            });


            //skimmer tail is at sandstone, broken off.



            #endregion




            #region skimmer at sandstone

            ///////////  //skimmer at sandstone: 

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startSkimmerHullSandstone", //relative position:   engine top: (-16, -48)   engine side: (+32, +32)
                DelayInSeconds = delayForStructures, //0,

                EntityData = new EntityData()
                {
                    Name = "Aircraft wreck (hull)",
                    EntityKey = "structure:skimmerHull",
                    Location = new Vector3(1104, 1344, 0), //owner set to none.
                }
            });

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startSkimmerEngineTopSandstone",
                DelayInSeconds = delayForStructures,

                EntityData = new EntityData()
                {
                    EntityKey = "structure:skimmerEngineTop",
                    Location = new Vector3(1100, 1304, 0), //owner set to none.
                }
            });

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startSkimmerEngineSideSandstone",
                DelayInSeconds = delayForStructures,

                EntityData = new EntityData()
                {
                    EntityKey = "structure:skimmerEngineSide",
                    Location = new Vector3(1136, 1376, 0), //owner set to none.
                }
            });


            list.Add(new SpawnEntityAction()
            {
                KeyName = "startSkimmerTailSandstone",
                DelayInSeconds = delayForStructures,

                EntityData = new EntityData()
                {
                    EntityKey = "structure:skimmerTail",
                    Location = new Vector3(624, 1358, 0) //owner set to none.
                }

            });



            #endregion

            #region supplies lying next to wreck (away from camp)

            #region at sandstone wreck
            //supplies laying on ground at sandstone. rations that twinklers eat.


            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFieldLabSandstone",
                DelayInSeconds = 0,

                EntityData = new EntityData()
                {
                    EntityKey = "item:fieldLabPacked",
                    Location = new Vector3(1026, 1372, 0), //owner set to none.
                }
            });

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startRationSandstone",
                DelayInSeconds = 0,

                EntityData = new EntityData()
                {
                    EntityKey = "item:astroRation",
                    Location = new Vector3(1056, 1392, 0), //owner set to none.
                }
            });

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startSentrySandstone",
                DelayInSeconds = 0,

                EntityData = new EntityData()
                {
                    EntityKey = "item:sentry",
                    Location = new Vector3(1056, 1392, 0), //owner set to none.
                }
            });

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startSentryWeaponMountSandstone",
                DelayInSeconds = 0,

                EntityData = new EntityData()
                {
                    EntityKey = "item:sentryWeaponMount",
                    Location = new Vector3(1056, 1392, 0), //owner set to none.
                }
            });

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startShotgunAmmoSandstone",
                DelayInSeconds = 0,

                EntityData = new EntityData()
                {
                    EntityKey = "item:shotgunAmmo",
                    Location = new Vector3(1056, 1392, 0), //owner set to none.
                }
            });

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startSentryGunAmmoSandstone",
                DelayInSeconds = 0,

                EntityData = new EntityData()
                {
                    EntityKey = "item:sentryGunAmmo",
                    Location = new Vector3(1056, 1392, 0), //owner set to none.
                }
            });

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startBasicFireExtinguisherSandstone",
                DelayInSeconds = 0,

                EntityData = new EntityData()
                {
                    EntityKey = "item:basicFireExtinguisher",
                    Location = new Vector3(1056, 1392, 0), //owner set to none.
                }
            });

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFireSuppressantCartridgeSandstone",
                DelayInSeconds = 0,

                EntityData = new EntityData()
                {
                    EntityKey = "item:fireSuppressantCartridge",
                    Location = new Vector3(1056, 1392, 0), //owner set to none.
                }
            });

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startEmptyCartridgeSandstone",
                DelayInSeconds = 0,

                EntityData = new EntityData()
                {
                    EntityKey = "item:emptyCartridge",
                    Location = new Vector3(1056, 1392, 0), //owner set to none.
                }
            });

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startSnipsSandstone",
                DelayInSeconds = 0,

                EntityData = new EntityData()
                {
                    EntityKey = "item:advancedSnips",
                    Location = new Vector3(1056, 1392, 0), //owner set to none.
                }
            });

            # endregion

            #region at bramble wreck
            //supplies laying on ground at bramble east of hill. 


            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFieldLabBrambleEast",
                DelayInSeconds = 0,

                EntityData = new EntityData()
                {
                    EntityKey = "item:fieldLabPacked",
                    Location = new Vector3(1862, 1733, 0), //owner set to none.
                }
            });


            //rations that twinklers eat.
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startRationBrambleEast",
                DelayInSeconds = 0,

                EntityData = new EntityData()
                {
                    EntityKey = "item:astroRation",
                    Location = new Vector3(1882, 1753, 0), //owner set to none.
                }
            });

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startSentryBrambleEast",
                DelayInSeconds = 0,

                EntityData = new EntityData()
                {
                    EntityKey = "item:sentry",
                    Location = new Vector3(1882, 1753, 0), //owner set to none.
                }
            });

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startSentryWeaponMountBrambleEast",
                DelayInSeconds = 0,

                EntityData = new EntityData()
                {
                    EntityKey = "item:sentryWeaponMount",
                    Location = new Vector3(1882, 1753, 0), //owner set to none.
                }

            });

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startShotgunAmmoBrambleEast",
                DelayInSeconds = 0,

                EntityData = new EntityData()
                {
                    EntityKey = "item:shotgunAmmo",
                    Location = new Vector3(1882, 1753, 0), //owner set to none.
                }

            });

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startSentryGunAmmoBrambleEast",
                DelayInSeconds = 0,

                EntityData = new EntityData()
                {
                    EntityKey = "item:sentryGunAmmo",
                    Location = new Vector3(1882, 1753, 0), //owner set to none.
                }

            });

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startBasicFireExtinguisherBrambleEast",
                DelayInSeconds = 0,

                EntityData = new EntityData()
                {
                    EntityKey = "item:basicFireExtinguisher",
                    Location = new Vector3(1882, 1753, 0), //owner set to none.
                }

            });

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startFireSuppressantCartridgeBrambleEast",
                DelayInSeconds = 0,

                EntityData = new EntityData()
                {
                    EntityKey = "item:fireSuppressantCartridge",
                    Location = new Vector3(1882, 1753, 0), //owner set to none.
                }

            });

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startEmptyCartridgeBrambleEast",
                DelayInSeconds = 0,

                EntityData = new EntityData()
                {
                    EntityKey = "item:emptyCartridge",
                    Location = new Vector3(1882, 1753, 0), //owner set to none.
                }

            });

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startSnipsBrambleEast",
                DelayInSeconds = 0,

                EntityData = new EntityData()
                {
                    EntityKey = "item:advancedSnips",
                    Location = new Vector3(1882, 1753, 0), //owner set to none.
                }

            });

            //lying west of hill, with the tail:
            list.Add(new SpawnEntityAction()
            {
                KeyName = "startMacheteBrambleWest",
                DelayInSeconds = 0,

                EntityData = new EntityData()
                {
                    EntityKey = "item:advancedMachete",
                    Location = new Vector3(1375, 1759, 0), //owner set to none.
                }

            });


            # endregion



            #endregion

            #region skimmer at bramble
            ///////////  //skimmer at bramble:

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startSkimmerHullBramble", //relative position:   engine top: (-16, -48)   engine side: (+32, +32)
                DelayInSeconds = delayForStructures,

                EntityData = new EntityData()
                {
                    Name = "Aircraft wreck (hull)",
                    EntityKey = "structure:skimmerHull",
                    Location = new Vector3(1852, 1700, 0) //owner set to none.
                }

            });

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startSkimmerEngineTopBramble",
                DelayInSeconds = delayForStructures,

                EntityData = new EntityData()
                {
                    EntityKey = "structure:skimmerEngineTop",
                    Location = new Vector3(1836, 1652, 0) //owner set to none.
                }

            });

            list.Add(new SpawnEntityAction()
            {
                KeyName = "startSkimmerEngineSideBramble",
                DelayInSeconds = delayForStructures,

                EntityData = new EntityData()
                {
                    EntityKey = "structure:skimmerEngineSide",
                    Location = new Vector3(1884, 1732, 0) //owner set to none.
                }

            });


            list.Add(new SpawnEntityAction()
            {
                KeyName = "startSkimmerTailBramble",
                DelayInSeconds = delayForStructures,

                EntityData = new EntityData()
                {
                    EntityKey = "structure:skimmerTail",
                    Location = new Vector3(1460, 1760, 0) //owner set to none.
                }

            });
            #endregion


            #endregion

            #region Starting equipment
            //for testing:--------------------------------
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startBow", new Vector2(124f, -24f), "item:improvisedBow", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startArrows", new Vector2(124f, -24f), "item:improvisedBasicArrow", "playerAllegiance", null, delayForItemsAndAgents));

            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startCrystalBerries", new Vector2(124f, -24f), "item:crystalBerries", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startCreeperPods", new Vector2(124f, -24f), "item:glassyCreeperPods", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startHoe", new Vector2(124f, -24f), "item:farmingHoe", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSpear", new Vector2(124f, -24f), "item:improvisedGoodSpear", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startBait", new Vector2(124f, -24f), "item:neonHornetsLive", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startHook", new Vector2(124f, -24f), "item:improvisedMetalHooks", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startTwinklerMeat", new Vector2(124f, -24f), "item:twinklerMeat", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startVat", new Vector2(124f, -24f), "item:vat", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startWaterCaneStem", new Vector2(124f, -24f), "item:waterCaneStem", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startVine", new Vector2(124f, -24f), "item:vine", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSulfurBomb", new Vector2(124f, -24f), "item:sulfurSmokeBomb", "playerAllegiance", null, delayForItemsAndAgents));
            //   list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startEnzyme", new Vector2(124f, -24f), "item:spottedOilTuberEnzyme", "playerAllegiance", null, delayForItemsAndAgents));
            //    list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSulfurSmokeBomb", new Vector2(124f, -24f), "item:sulfurSmokeBomb", "playerAllegiance", null, delayForItemsAndAgents)); // testsulfur!
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startImprovedFireExtinguisher", new Vector2(124f, -24f), "item:improvedFireExtinguisher", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startBushDragonCartridge", new Vector2(124f, -24f), "item:bushDragonCartridge", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSentry", new Vector2(124f, -24f), "item:sentry", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSentryWeaponMount", new Vector2(124f, -24f), "item:sentryWeaponMount", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSprayGunSentry", new Vector2(124f, -24f), "item:spraySentry", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSentryGunAmmo", new Vector2(124f, -24f), "item:sentryGunAmmo", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startShotgunSentry", new Vector2(124f, -24f), "item:shotgunSentry", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startShotgunAmmo", new Vector2(124f, -24f), "item:shotgunAmmo", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startShotgun", new Vector2(124f, -24f), "item:shotgun", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startGoggles", new Vector2(124f, -24f), "item:nightVisionGoggles", "playerAllegiance", null, delayForItemsAndAgents));

            //--------------------------------------------

            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSensor", new Vector2(124f, -24f), "item:sensor", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startKnife", new Vector2(124f, -24f), "item:advancedKnife", "playerAllegiance", null, delayForItemsAndAgents));

            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startSnips", new Vector2(124f, -24f), "item:advancedSnips", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startString", new Vector2(124f, -24f), "item:advancedString", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startThermalTarp", new Vector2(124f, -24f), "item:thermalTarp", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startRation", new Vector2(124f, -24f), "item:astroRation", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startHuntingRifle", new Vector2(124f, -24f), "item:coilRifle", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startRifleAmmo", new Vector2(124f, -24f), "item:coilRifleAmmo", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("startMachete", new Vector2(124f, -24f), "item:advancedMachete", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemAtStartLocation("testShadeleafCanes", new Vector2(124f, -24f), "item:shadeleafCanes", "playerAllegiance", null, delayForItemsAndAgents));

            //inside skimmer hull. spawn after buildings:  //the only items I put inside are used in the 'next to skimmer' scenarios (Normal) . to avoid automated offloading haul from far away in the 'fled from skimmer scenarios'.
            //THERE needs to be something inside the hull for the offloading anim to happen! even on very hard scenario.
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startEmptyCartridgeInSkimmer", "Aircraft wreck (hull)", "item:emptyCartridge", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startBasicFireExtinguisherInSkimmer", "Aircraft wreck (hull)", "item:basicFireExtinguisher", "playerAllegiance", null, delayForItemsAndAgents));  //
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startFireSuppressantCartridgeInSkimmer", "Aircraft wreck (hull)", "item:fireSuppressantCartridge", "playerAllegiance", null, delayForItemsAndAgents));
            list.Add(Scenarios.ScenarioLoader.SpawnItemInsideContainer("startFieldLabInSkimmer", "Aircraft wreck (hull)", "item:fieldLabPacked", "playerAllegiance", null, delayForItemsAndAgents));



            #endregion

            /* deleting this, doesn't look like it should be there?
            List<string> validationErrors;
            Dictionary<string, List<string>> allPostLoadContentValidationErrors = new Dictionary<string,List<string>>();
            foreach (var item in list)
            {
                validationErrors = new List<string>();
                allPostLoadContentValidationErrors.Add(item.KeyName, validationErrors);

                item.PostLoadContentValidate(validationErrors);
            }

            BaseDataLoader.DisplayAllValidationErrors(allPostLoadContentValidationErrors);
            
            
        */

            return list;
        }

    }
}
