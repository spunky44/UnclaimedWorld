using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.InGameEvents;
using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.ClientSide.GameEvents;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.Maps;
using UWGame.SimSide.InGameEvents.PropertyObjects;
using UWGame.Client.Particles;
using UWGame.SimSide.InGameEvents.Expressions;
using UWGame.ClientSide.Interface;
using UWGame.SimSide.XmlCollections;
using UWGame.SimSide.Entities.Biological;
//using UWGame.SimSide.InGameEvents.Expressions;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_5.Data
{
    public class PolledEventsLoader
    {

        public static List<InGameEvents.PolledEventType> Init()
        {
            List<PolledEventType> list = new List<PolledEventType>();



            #region intro dialogue , barks
            //

            list.Add(new PolledEventType()
            {
                KeyName = "introDialogue",
                StartTimePoint = new TimePoint()
                {
                    RelativeNoOfDays = 0.0038
                },
                ActionSets = new ActionSets()
                {
                    SetsOfActions = new[]
                    {
                        new ActionSetType("793567w5zgdewqewrtyiuopppyurtysusrtyurysu20")
                        {
                            Actions = new EventActionType[]
                            {
                                new TalkAction("ab94etyaf324trthgjuiop735673567u5e6uetyueyuyurt9a")
                                {
                                        DelayInSeconds = 2,
                                      
                                            TalkPriority = TalkAction.TalkActionPriority.High,
                                            CanTalkWhileFighting = false,
                                            CanTalkWhileSleeping = false,
                                            CanTalkWhileThreatened = false,
                                            TurnTowardsListeners = true,
                                            ActionByAgent = ActionByAgent.RandomInAllegiance,
                                            SpeakerDenomination = TalkAction.SpeakerInConversation.First,    
                                            DefaultText = "I don't know...the swamp scares me." // 
                                        
                                },
                                new TalkAction("4e8dstyjutyadkfhaifhiysdjr657eavyaeuf322378ue585e6756e7c")
                                {
                                    DelayInSeconds = 5,
                                   
                                        TalkPriority = TalkAction.TalkActionPriority.High,
                                        CanTalkWhileFighting = false,
                                        CanTalkWhileSleeping = false,
                                            CanTalkWhileThreatened = false,
                                        TurnTowardsListeners = true, //
                                        ActionByAgent = ActionByAgent.OnlySpecific,
                                        NameOfSpeaker = "John Millet", //
                                        SpeakerDenomination = TalkAction.SpeakerInConversation.Second,                                   
                                        DefaultText = "We just need to take precautions." //
                                    
                                },
                                new TalkAction("agfasfaefaeqqgfedhfffwchvgefqjvfg56u65edrtyuhjdyrtuhjsytruysrt9a")
                                {
                                        DelayInSeconds = 8,
                                      
                                            TalkPriority = TalkAction.TalkActionPriority.High,
                                            CanTalkWhileFighting = false,
                                            CanTalkWhileSleeping = false,
                                            CanTalkWhileThreatened = false,
                                            TurnTowardsListeners = true,
                                            ActionByAgent = ActionByAgent.RandomInAllegiance,
                                            SpeakerDenomination = TalkAction.SpeakerInConversation.Third,    
                                            DefaultText = "But we need one of the chemists from Eden Plains?" // 
                                        
                                },
                                new TalkAction("cetyu4w6y555555555555yw45ghjsyhtr0")
                                {
                                        DelayInSeconds = 11,
                                      
                                            TalkPriority = TalkAction.TalkActionPriority.High,
                                            CanTalkWhileFighting = false,
                                            CanTalkWhileSleeping = false,
                                            CanTalkWhileThreatened = false,
                                            TurnTowardsListeners = true,
                                            ActionByAgent = ActionByAgent.OnlySpecific,
                                            NameOfSpeaker = "John Millet", //
                                            SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                                            DefaultText = "Yes, like I said. But they will come!" 
                                        
                                },
                                new TalkAction("cetafs32525235adgagayu567hahhueysdtrhjufsghsrfhjsyhtr0")
                                {
                                        DelayInSeconds = 14,
                                      
                                            TalkPriority = TalkAction.TalkActionPriority.High,
                                            CanTalkWhileFighting = false,
                                            CanTalkWhileSleeping = false,
                                            CanTalkWhileThreatened = false,
                                            TurnTowardsListeners = true,
                                            ActionByAgent = ActionByAgent.OnlySpecific,
                                            NameOfSpeaker = "John Millet", //
                                            SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                                            DefaultText = "Everyone! Please read my plan and let's get this started!" 
                                        
                                },
                            }
                        }
                    }
                }
            });
            #endregion


            #region intro Dialogue screen
            list.Add(new PolledEventType()
            {
                KeyName = "introDialogueScreen",
                StartTimePoint = new TimePoint()
                {
                    RelativeNoOfDays = 0.015
                },
                ActionSets = new ActionSets()
                {
                    SetsOfActions = new[]{ new ActionSetType("6f046786748674886748674ga352jhiop6478ee92")
                { 
                        Actions = new EventActionType[]
                        {

                      new EventActionDialog("53253675637gftyhy78563876487648644f4e51") { DelayInSeconds = 0, 
                        //sync the GOALS numbers with "spawnOtherSite2Allegiance1" and "winGame"
                        
                            DisplayText = new DynamicText()
                            { 
                               // Text = "Headway - ANNUAL TOWN MEETING \n#JOURNALNAMES \n///////////////////////////////////////////////////// \nPROPOSAL ADOPTED - Work toward the following GOAL: \n \n-Increase population to 15 \nAND \n-Achieve conditions that are equal to Eden Plains: \nFOOD: 40%    SECURITY: 27%    COMFORT: 40% \n///////////////////////////////////////////////////// \nThe following plan was presented: \n \nHire a barge from Eden Plains which carries: \n-One or more chemists from the Neson family \n-An extrusion machine \n-A stock of sulfur powder \n \nOnce this arrives, we will construct the polymer workshop and start harvesting marshcot sap from the swamp. The Nesons will help us produce rubber parts that we can sell, thereby financing our growth. \n \nNOTE: For instructions on how to hire the barge, click the GUIDE button which accompanies this screen." ,  //  
                         Text = "Headway - ANNUAL TOWN MEETING \n#JOURNALNAMES \n///////////////////////////////////////////////////// \nPROPOSAL ADOPTED - Work toward the following GOAL: \n \n-Increase population to 15 \nAND \n-Achieve conditions that are equal to Eden Plains: \n#FOODTARGET    #SECURITYTARGET    #COMFORTTARGET \n///////////////////////////////////////////////////// \nThe following plan was presented: \n \nHire a barge from Eden Plains which carries: \n-One or more chemists from the Neson family \n-An extrusion machine \n-A stock of sulfur powder \n \nOnce this arrives, we will construct the polymer workshop and start harvesting marshcot sap from the swamp. The Nesons will help us produce rubber parts that we can sell, thereby financing our growth. \n \nNOTE: For instructions on how to hire the barge, click the GUIDE button which accompanies this screen." ,  //  
                                SubstitutionValues = new[]
                                {
                                    new SubstituteValue() { Placeholder = "#JOURNALNAMES", PropertyName = "getJournalHeaderNames" },
                                    new SubstituteValue() { Placeholder = "#COMFORTTARGET", Property = new UnaryFunctionNode() { Operator = UnaryExpressionOperator.ComfortRatingToString, Operand = new ValueNode() { PropertyKey = "comfortTarget" }}},
                                    new SubstituteValue() { Placeholder = "#FOODTARGET", Property = new UnaryFunctionNode() { Operator = UnaryExpressionOperator.FoodRatingToString, Operand = new ValueNode() { PropertyKey = "foodTarget" }}},
                                    new SubstituteValue() { Placeholder = "#SECURITYTARGET", Property = new UnaryFunctionNode() { Operator = UnaryExpressionOperator.SecurityRatingToString, Operand = new ValueNode() { PropertyKey = "securityTarget" }}}
                                }
                            },   
                          
                            DisplayImage = "IndoorMeeting",
                             DialogOptions = new[] { new DialogOption()
                             {
                                Text = "GUIDE #1", Tooltip = "See how to hire the barge (opens separate window.)", ActiveInArchive = true,
                                ActionSet = "showTutorialRubberScenario5_1" 
                             }}
                        
                           }                      
                          }
                        
                }
                }

                }
            });
            #endregion

            #region extrusion machine arrival Check
            list.Add(new PolledEventType()
            {
                KeyName = "extrusionMachineArrivalCheck",  // 
                PollInterval = new ValueNode() { Decimal = 1f },  // seconds               
                AllowRandomTimeOffset = true,
                Condition = new CustomCondition()
                {
                    AllowWhileAllPlayerMembersAreSleepingOrCollapsed = false,
                    AllowWhilePlayerThreatened = false,  // player allegiance threatened
                    AllowWhilePlayerMemberIsFighting = false,

                    TargetObject = new TargetObject()
                    {
                        GetList = new GetList()
                        {
                            HasPropertiesListKey = "finishedEntities",
                            FilterCondition = new PropertyCondition()
                            {
                                PropertyKey = "type",
                                ConstantStringEqual = "item:extrusionMachineComponents"
                            }
                        }
                    },
                    ListCondition = new ListCondition()
                    {
                        CountMinimum = new ValueNode()
                        {
                            Decimal = 1.0f
                        }
                    }

                },
                ActionSetsKey = "extrusionMachineArrivalScreen"

            });

            #endregion
            #region win condition

            string endGameTooltip = "You have succeeded: The colony has grown and reached the goals set by the townspeople. Clicking here will end your current game and take you to the main menu.";
            string continueGameTooltip = "You have achieved the goal of the scenario, but you can keep on playing by selecting this option. How far can you take the town from here?";


            list.Add(new PolledEventType()
            {
                KeyName = "winGame",
                Comment = "Tests the ratings of the player allegiance as well as the member count",
                UseDefaultPollInterval = true,
                AllowRandomTimeOffset = true,
                Condition = new ConditionFunction()
                {
                    Operator = OperatorType.And,
                    Left = new ConditionFunction()
                    {
                        Left = new PlayerAllegiancePersons()
                        {
                            MinMembers = 12 //sync the members numbers with "introDialogueScreen"
                        },
                        Operator = OperatorType.And,
                        Right = new CustomCondition()
                        {
                            PropertyCondition = new PropertyCondition()
                            {
                                PropertyKey = "gameOver",
                                BoolValue = false
                            }

                        }

                    },
                    Right = new ConditionFunction()
                        {
                            Left = new ConditionFunction()
                                {
                                    Left = new CustomCondition()
                                    {
                                        TargetObject = new TargetObject()
                                        {
                                            TargetObjectType = TargetObjectType.Root,
                                            GetList = new GetList()
                                            {
                                                HasPropertiesListKey = "allegiances",
                                                FilterCondition = new InGameEvents.Conditions.PropertyCondition()
                                                {
                                                    PropertyKey = "keyName",
                                                    ConstantStringEqual = "playerAllegiance"
                                                }
                                            }
                                        }, //per the story: they want to match the neighbor 'Farming' town - sync the GOALS numbers with "spawnOtherSite2Allegiance1" and "winGame" and "introDialogueScreen"
                                        PropertyCondition = new PropertyCondition()
                                        {
                                            PropertyKey = "foodRating",
                                            NumberMinimumInclusive = new ValueNode()
                                            {
                                                PropertyKey = "foodTarget"
                                                //Decimal = .40f //was .32f
                                            }
                                        }

                                    },
                                    Operator = OperatorType.And,
                                    Right = new CustomCondition()
                                    {
                                        TargetObject = new TargetObject()
                                        {
                                            TargetObjectType = TargetObjectType.Root,
                                            GetList = new GetList()
                                            {
                                                HasPropertiesListKey = "allegiances",
                                                FilterCondition = new InGameEvents.Conditions.PropertyCondition()
                                                {
                                                    PropertyKey = "keyName",
                                                    ConstantStringEqual = "playerAllegiance"
                                                }
                                            }
                                        },
                                        PropertyCondition = new PropertyCondition()
                                        {
                                            PropertyKey = "comfortRating",
                                            NumberMinimumInclusive = new ValueNode()
                                            {
                                                PropertyKey = "comfortTarget"
                                                //Decimal = .40f //was .46f
                                            }
                                        }
                                    }

                                },
                            Operator = OperatorType.And,
                            Right = new CustomCondition()
                            {
                                TargetObject = new TargetObject()
                                {
                                    TargetObjectType = TargetObjectType.Root,
                                    GetList = new GetList()
                                    {
                                        HasPropertiesListKey = "allegiances",
                                        FilterCondition = new InGameEvents.Conditions.PropertyCondition()
                                        {
                                            PropertyKey = "keyName",
                                            ConstantStringEqual = "playerAllegiance"
                                        }
                                    }
                                },
                                PropertyCondition = new PropertyCondition()
                                {
                                    PropertyKey = "securityRating",
                                    NumberMinimumInclusive = new ValueNode()
                                    {
                                        PropertyKey = "securityTarget"
                                        //Decimal = .27f //was .39f
                                    }
                                }
                            }

                        }
                }
                ,
                ActionSets = new ActionSets()
                {
                    SetsOfActions = new[]{ new ActionSetType("c8a8e18a-d51e-4c7a-92a0-251990e6a1f2")
                    {                       
                        Actions = new EventActionType[]{          
                            new WinGameAction("b21eb90e-b8ef-4529-9261-083b7c39bdba")
                            {
                     
                                Comments = "this is a modal event screen dialog on the game area",
                                                             
                                // this can be null. then the modal dialog will be skipped:
                                ModalDialogText = new DynamicText(){ Text = "A happy murmur filled the crowded meeting room but died slightly down when the toastmaster stood up. \n-Great to have you all here. Welcome to our newcomers - good to see that you're fitting in so well! As you all know, this place was in a slump not so long ago. But we turned things around. We set a goal for our town, and today we've reached it. I know there's been disagreements along the way but I think we can all be proud of what we've achieved. I honestly feel there's no limit to what we can do! Let's make a toast to the future of Headway!" }, 
                                       
                                ModalDialogImage = "IndoorMeeting",

                                WinScreenText = "The two old friends smiled as they crossed paths. They looked at their village, now a bustling place, voices and sounds of activity coming from all directions. \n-Making headway, huh? \n-We sure are. Hey - it's been awhile since we sat down and talked. \n-Yeah, it's hard to find the time what with all these new people and projects. So many new faces. I've heard that some of the farmers who left for Eden Plains are thinking of coming back! \n-That sounds great. It'll be just like the old days, right? \n-Definitely not! Life is a lot better now!", //
                        
                                EndGameTooltip = endGameTooltip,
                                ContinueGameTooltip = continueGameTooltip,

                                ContinueActions = new ActionSets()
                                {
                                    SetsOfActions = new ActionSetType[]
                                    {
                                        new ActionSetType("beafa586-82f3-4a3b-a16f-1021b1a0893f")
                                        {
                                            Actions = new EventActionType[]
                                            {
                                                new SetPropertyAction("d85e6b69-252a-4f7f-836d-0bc3046c0066")
                                                {
                                                            
                                                        PropertyKey = "gameOver", 
                                                        Value = new ValueNode() { Bool = true }
                                                            
                                                }
                                            }
                                        }
                                    }
                                }   
                            },

                            new MusicAction("03855c4b-ff01-42f6-b079-92afd0a87e50")
                            {
                                   
                                  Song = "Martin Hasseldam - A New World (Alt3) 320kBit" 
                                    
                            }
                        }
                    }
                }
                }
            });

            #endregion

            #region lose condition

            list.Add(new PolledEventType()
            {
                KeyName = "SANDBOXMAP_loseGame",
                UseDefaultPollInterval = true,
                AllowRandomTimeOffset = true,
                Condition = new PlayerAllegiancePersons() // condition: all dead -MP
                {
                    MaxMembers = 0
                },
                ActionSets = new ActionSets()
                {
                    SetsOfActions = new[]{ new ActionSetType("2452d652-4370-4469-9c4e-a1er4efedg-rhiopxcv0b83")
                    {                       
                        Actions = new EventActionType[]
                        {          
                            new LoseGameAction("db7871a4-fe7agagda-413d-a67a-1ec7bbe0c209")
                            {                                                          
                                    LoseScreenText = " \nWith the instinctive willpower of pioneers, humans strove to tame a planet whose instincts told it to resist. This duel went on for generations as hope was built, crushed and rebuilt, and lessons were repeatedly learned and forgotten. \n \nTime would tell if the human presence on Antheia was just a temporary incursion or if they were destined to dominate this biosphere just like Earth." 
                               
                            },
                            new MusicAction("a8454060-6bb5-4176-89nnn03-30d4473aa068")
                            {
                               
                                    Song = "Martin Hasseldam - Unfamiliar Starlight" 
                                
                            }
                        }
                    }
                }
                }
            });
            #endregion

  /*
            #region COVEMAP_timedSpawnBeginningPopulationNormal 
            list.Add(new PolledEventType()
            {
                KeyName = "COVEMAP_timedSpawnBeginningPopulationNormal",
                ActionSets = new ActionSets()
                {
                    SetsOfActions = new[]{ new ActionSetType("0a4f3522-1ea4-46f1-b2e4-7a4305b5a326")
                    {         
                        Actions = new EventActionType[]
                        {
                            #region Birds
                                //north west
                            new SpawnEntityAction("b1232ac8-cbf8-48d4-9973-5f4bdb043f75") { DelayInSeconds = 0.1,
                           EntityData = new EntityData()
                                { EntityKey = "entity:bird", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "birdAllegiance" },  
                                Location = new Vector3(200, 200, 0), Rotation = 100, Bulk = 0.12f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, RaceKey = "dark" }  }},
                            new SpawnEntityAction("ffd9157e-351d-41c6-bc29-2115475cd555") { DelayInSeconds = 3.5,
                           EntityData = new EntityData()
                                { EntityKey = "entity:bird", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "birdAllegiance" },  
                                Location = new Vector3(220, 210, 0), Rotation = 190, Bulk = 0.12f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, RaceKey = "pale" }  }},

                                // at inlet
                            new SpawnEntityAction("6e06ef9f-996e-4e69-bb23-5d725699af93") { DelayInSeconds = 0.25,
                           EntityData = new EntityData()
                                { EntityKey = "entity:bird", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "birdAllegiance" },  
                                Location = new Vector3(1680, 2064, 0), Rotation = 100, Bulk = 0.12f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, RaceKey = "purple" }  }},
                            new SpawnEntityAction("0dfce0bf-9d9b-468c-9e2e-4d3c400afaa4") { DelayInSeconds = 1.5,
                           EntityData = new EntityData()
                                { EntityKey = "entity:bird", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "birdAllegiance" },  
                                Location = new Vector3(1700, 2044, 0), Rotation = 175, Bulk = 0.12f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, RaceKey = "purple" }  }},
                            new SpawnEntityAction("7fc004f6-79b8-4ec3-8bdb-5b27ae7c8d1d") { DelayInSeconds = 2.75,
                           EntityData = new EntityData()
                                { EntityKey = "entity:bird", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "birdAllegiance" },  
                                Location = new Vector3(1730, 2020, 0), Rotation = 250, Bulk = 0.12f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, RaceKey = "purple" }  }}
                        
 
                        #endregion
                        }
                    }
                    }
                }
            });*/
                          
                            /*
                            #region swampDemonTree #2 north
                            new SpawnEntityAction("aa089ba6-e54af2424a48a-dfb1f37600e7")
                            {
                                
                                    EntityData = new EntityData()
                                    {
                                        EntityKey = "entity:swampDendront", Name = "Swamp Demon Tree", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "swampDemonTreeAllegiance#2" },
                                        Location = new Vector3(2602,624, 0), Bulk = 1.3f, BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult } 
                                    }, 
                                
                            },
                            #endregion
                           
                            */



                          /*  #region Thunder Chicken #2

                            new SpawnEntityAction("91dfzsb2390e2-677fwafwa2e4248-bdse56-86a2f352d843") 
                            { 
                                DelayInSeconds = 0.1,
                               
                                    EntityData = new EntityData()
                                    { 
                                        EntityKey = "entity:studdedThunderChicken", Name = "ThunderChicken(1584,2736)", 
                                        MemberOf = new AllegianceAndExpedition() { AllegianceKey = "thunderChickenAllegiance#2" },
                                        Location = new Vector3(1384,70, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult} 
                                    }, 
                                
                            },                         


                            #endregion*/
  

                        /*    #region Turnip
                  
                            new SpawnEntityAction("6f0be88a-eee5-42c9-8483-252947b950f1") { DelayInSeconds = 0.1,
                           EntityData = new EntityData()
                                { EntityKey = "entity:turnip", Name = "Turnip2", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "turnipAllegianceNorth" },
                                  Location = new Vector3(768,1200, 0), Bulk = 4.2f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, RaceKey = "Pale Turnip" }  }},


                            #endregion
                        */
                          /*  #region Megapod (Slug)
                            new SpawnEntityAction("b4f9ffwa24242d0-a0e7-e8436d24327c")
                            {
                                
                                    EntityData = new EntityData()
                                    {
                                        EntityKey = "entity:megapod", Name = "Megapod1", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "slugAllegiance#1" },
                                        Location = new Vector3(2410,485, 0), Bulk = 1.1f, BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult } 
                                    }, 
                                
                            },
                            new SpawnEntityAction("b4f9f25d-7afws4224e7-e8436d24327c")
                            {
                                
                                    EntityData = new EntityData()
                                    {
                                        EntityKey = "entity:megapod", Name = "Megapod2", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "slugAllegiance#1" },
                                        Location = new Vector3(2300,456, 0), Bulk = 1.1f, BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult } 
                                    }, 
                                
                            },
                            new SpawnEntityAction("b4f9f25aw4235-a0e7-e8436d24327c")
                            {
                                
                                    EntityData = new EntityData()
                                    {
                                        EntityKey = "entity:megapod", Name = "Megapod3", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "slugAllegiance#1" },
                                        Location = new Vector3(2100,508, 0), Bulk = 1.1f, BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult } 
                                    }, 
                                
                            },

                            #endregion*/
                            
                           /* #region Snatcher
                            new SpawnEntityAction("9c4bdc61-13bd-49f7-91c0-db90b9fd3b0d") { DelayInSeconds = 0.1,
                           EntityData = new EntityData()
                                { EntityKey = "entity:whipjaw", Name = "Whipjaw", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "snatcherAllegiance#1" },
                                  Location = new Vector3(1109,2406, 0), Bulk = 2f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, }  }},

                            #endregion*/
                            
                          
/*
                            #region varmint scavengers

                            #region ratspawns
                            #region binalRatAllegiance#3
                                    new SpawnEntityAction("f39d3dff8-2268-4613-9477-13wa2424as10fxdr98") 
                                    { 
                                        DelayInSeconds = 0.1,
                                        
                                            EntityData = new EntityData()
                                            { 
                                                EntityKey = "entity:binalRat", Name = "BinalRat1(6048,4800)",
                                                MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegiance#3" },
                                                Location = new Vector3(766,1188, 0), Bulk = 0.28f, BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult } 
                                            }, 
                                        
                                    },
                                    new SpawnEntityAction("f608b57c-0847-442d-ab93-c57fwfa24r7f395de") 
                                    { 
                                        DelayInSeconds = 0.1,
                                        
                                            EntityData = new EntityData()
                                            { 
                                                EntityKey = "entity:binalRat", Name = "BinalRat(5280,5040)", 
                                                MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegiance#3" },
                                                Location = new Vector3(577,1299, 0), Bulk = 0.28f, BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult } 
                                            }, 
                                        
                                    },
                            #endregion
                            #region "binalRatAllegiance#2"
                                    new SpawnEntityAction("99875cc012qwfs-4f81-82fe-06cdc739ecfe")
                                    {
                                        
                                            EntityData = new EntityData()
                                            { 
                                                EntityKey = "entity:binalRat", Name = "BinalRat(1296,1344)", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegiance#2" },
                                                Location = new Vector3(965,2069, 0), Bulk = 0.21f,
                                                BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult } 
                                            }, 
                                        
                                    },
                                    new SpawnEntityAction("99875cc252q32arwf81-82fe-06cdc739ecfe")
                                    {
                                        
                                            EntityData = new EntityData()
                                            { 
                                                EntityKey = "entity:binalRat", Name = "BinalRat(480,816)", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegiance#2" },
                                                Location = new Vector3(900,2000, 0), Bulk = 0.21f,
                                                BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult } 
                                            }, 
                                        
                                    },

#endregion
                            #region "binalRatAllegiance#1"
                                    new SpawnEntityAction("99875cc0-aw21rfw4f81-82fe-06cdc739ecfe")
                                    {
                                        
                                            EntityData = new EntityData()
                                            { 
                                                EntityKey = "entity:binalRat", Name = "BinalRat(5856,528)", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegiance#1" },
                                                Location = new Vector3(2309,530, 0), Bulk = 0.21f,
                                                BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult} 
                                            }, 
                                        
                                    }, 
 
#endregion


                            #endregion*/
                                    /*
                            #region leafcutter
#region leafcutterAllegiance#1                                    
                            new SpawnEntityAction("99875cc0-30ae-4f81-a25rfwe3739ecfe")
                            {
                               
                                    EntityData = new EntityData()
                                    { 
                                        EntityKey = "entity:fieldQuadite", Name = "leafcutter(1296,1344)", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "leafcutterAllegiance#1" },
                                        Location = new Vector3(1210,1162, 0), Bulk = 0.20f,
                                        BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult} 
                                    }, 
                                
                            },
#endregion

                            #endregion
                                  

                            #endregion

                        }
                    }
                    }
                }
            };
            list.Add(polledEvent);

  */

         

            /*
            #region///////Continual spawning




            #region Turnips north
            list.Add(new PolledEventType()
            {
                KeyName = "COVEMAP_continualSpawnTurnipsNorth",
                PollInterval = new ValueNode() { PropertyKey = "turnipSpawnInterval" },
                StartAfterInterval = true,
                Condition = new CustomCondition()
                    {
                        TargetObject = new TargetObject()
                        {
                            GetList = new GetList()
                            {
                                HasPropertiesListKey = "allegiances",
                                FilterCondition = new PropertyCondition()
                                {
                                    PropertyKey = "keyName",
                                    ConstantStringEqual = "turnipAllegianceNorth"
                                },
                                NextList = new GetList()
                                {
                                    HasPropertiesListKey = "members"
                                }
                            }
                        },
                        ListCondition = new ListCondition()
                        {
                            CountMaximum = new ValueNode()
                            {
                                PropertyKey = "maxTurnips"
                            }
                        }


                    },

                ActionSetsKey = "continualSpawnTurnipNorth"
            });
            #endregion

            #region Binal rats #3
            list.Add(new PolledEventType()
            {
                KeyName = "COVEMAP_continualSpawnBinalRats#3",
                PollInterval = new ValueNode() { PropertyKey = "binalRatSpawnInterval" },
                StartAfterInterval = true,
                Condition = new CustomCondition()
                {
                    TargetObject = new TargetObject()
                    {
                        GetList = new GetList()
                        {
                            HasPropertiesListKey = "allegiances",
                            FilterCondition = new PropertyCondition()
                            {
                                PropertyKey = "keyName",
                                ConstantStringEqual = "binalRatAllegiance#3"
                            },
                            NextList = new GetList()
                            {
                                HasPropertiesListKey = "members"
                            }
                        }
                    },
                    ListCondition = new ListCondition()
                    {
                        CountMaximum = new ValueNode()
                        {
                            PropertyKey = "maxBinalRats"
                        }
                    }
                },
                ActionSetsKey = "continualSpawnBinalRats#3"
            });
            #endregion

            #region Binal rats #2
            list.Add(new PolledEventType()
            {
                KeyName = "COVEMAP_continualSpawnBinalRats#2",
                PollInterval = new ValueNode() { PropertyKey = "binalRatSpawnInterval" },
                StartAfterInterval = true,
                Condition = new CustomCondition()
                    {
                        TargetObject = new TargetObject()
                        {
                            GetList = new GetList()
                            {
                                HasPropertiesListKey = "allegiances",
                                FilterCondition = new PropertyCondition()
                                {
                                    PropertyKey = "keyName",
                                    ConstantStringEqual = "binalRatAllegiance#2"
                                },
                                NextList = new GetList()
                                {
                                    HasPropertiesListKey = "members"
                                }
                            }
                        },
                        ListCondition = new ListCondition()
                        {
                            CountMaximum = new ValueNode()
                            {
                                PropertyKey = "maxBinalRats"
                            }
                        }
                    }
                ,

                ActionSetsKey = "continualSpawnBinalRats#2"
            });
            #endregion

            #region Binal rats #1
            list.Add(new PolledEventType()
            {
                KeyName = "COVEMAP_continualSpawnBinalRats#1",
                PollInterval = new ValueNode() { PropertyKey = "binalRatSpawnInterval" },
                StartAfterInterval = true,
                Condition = new CustomCondition()
                        {
                            TargetObject = new TargetObject()
                            {
                                GetList = new GetList()
                                {
                                    HasPropertiesListKey = "allegiances",
                                    FilterCondition = new PropertyCondition()
                                    {
                                        PropertyKey = "keyName",
                                        ConstantStringEqual = "binalRatAllegiance#1"
                                    },
                                    NextList = new GetList()
                                    {
                                        HasPropertiesListKey = "members"
                                    }
                                }
                            },
                            ListCondition = new ListCondition()
                            {
                                CountMaximum = new ValueNode()
                                {
                                    PropertyKey = "maxBinalRats"
                                }
                            }
                        },

                ActionSetsKey = "continualSpawnBinalRats#1"
            });
            #endregion
            */

            /*
            #region thunder chicken # 2
            list.Add(new PolledEventType()
            {
                KeyName = "COVEMAP_continualSpawnThunderChickens#2",
                PollInterval = new ValueNode() { PropertyKey = "thunderChickenSpawnInterval" },
                StartAfterInterval = true,
                Condition = new CustomCondition()
                {
                    TargetObject = new TargetObject()
                    {
                        GetList = new GetList()
                        {
                            HasPropertiesListKey = "allegiances",
                            FilterCondition = new PropertyCondition()
                            {
                                PropertyKey = "keyName",
                                ConstantStringEqual = "thunderChickenAllegiance#2"
                            },
                            NextList = new GetList()
                            {
                                HasPropertiesListKey = "members"
                            }
                        }
                    },
                    ListCondition = new ListCondition()
                    {
                        CountMaximum = new ValueNode()
                        {
                            PropertyKey = "maxThunderChickens"
                        }
                    }


                },
                ActionSetsKey = "continualSpawnThunderChicken#2"
            });
            #endregion
            */


         /*   #region snatcher
            list.Add(new PolledEventType()
            {
                KeyName = "COVEMAP_continualSpawnSnatcher",
                PollInterval = new ValueNode() { PropertyKey = "snatcherSpawnInterval" },
                StartAfterInterval = true,
                Condition = new CustomCondition()
                    {
                        TargetObject = new TargetObject()
                        {
                            GetList = new GetList()
                            {
                                HasPropertiesListKey = "allegiances",
                                FilterCondition = new PropertyCondition()
                                {
                                    PropertyKey = "keyName",
                                    ConstantStringEqual = "snatcherAllegiance#1"
                                },
                                NextList = new GetList()
                                {
                                    HasPropertiesListKey = "members"
                                }
                            }
                        },
                        ListCondition = new ListCondition()
                        {
                            CountMaximum = new ValueNode()
                            {
                                PropertyKey = "maxSnatchers"
                            }
                        }
                    }
                ,

                ActionSetsKey = "continualSpawnSnatchers#1"
            });
            #endregion
            */


         /*   #region swampDemonTree #2
            list.Add(new PolledEventType()
            {
                KeyName = "COVEMAP_continualSpawnSwampDemonTree#2",
                PollInterval = new ValueNode() { PropertyKey = "demonTreeSpawnInterval" },
                StartAfterInterval = true,
                Condition = new CustomCondition()
                    {
                        TargetObject = new TargetObject()
                        {
                            GetList = new GetList()
                            {
                                HasPropertiesListKey = "allegiances",
                                FilterCondition = new PropertyCondition()
                                {
                                    PropertyKey = "keyName",
                                    ConstantStringEqual = "swampDemonTreeAllegiance#2"
                                },
                                NextList = new GetList()
                                {
                                    HasPropertiesListKey = "members"
                                }
                            }
                        },
                        ListCondition = new ListCondition()
                        {
                            CountMaximum = new ValueNode()
                            {
                                PropertyKey = "maxDemonTree"
                            }
                        }

                    },
                ActionSetsKey = "continualSwampDemonTree#2"
            });
            #endregion
            */
            /*
            #region slugs megapods

            list.Add(new PolledEventType()
            {
                KeyName = "COVEMAP_continualSpawnSlugs",
                PollInterval = new ValueNode() { PropertyKey = "slugSpawnInterval" },
                StartAfterInterval = true,
                Condition = new CustomCondition()
                {
                    TargetObject = new TargetObject()
                    {
                        GetList = new GetList()
                        {
                            HasPropertiesListKey = "allegiances",
                            FilterCondition = new PropertyCondition()
                            {
                                PropertyKey = "keyName",
                                ConstantStringEqual = "slugAllegiance#1"
                            },
                            NextList = new GetList()
                            {
                                HasPropertiesListKey = "members"
                            }
                        }
                    },
                    ListCondition = new ListCondition()
                    {
                        CountMaximum = new ValueNode()
                        {
                            PropertyKey = "maxSlugs"
                        }

                    }
                },
                ActionSetsKey = "continualSpawnSlugs"
            });
            #endregion
            */
            /*
            #region leafcutter spawns

            #region leafcutter #1
            list.Add(new PolledEventType()
            {
                KeyName = "COVEMAP_continualSpawnLeafcutter#1",
                PollInterval = new ValueNode() { PropertyKey = "leafcutterSpawnInterval" },
                StartAfterInterval = true,
                Condition = new ConditionFunction()
                    {
                        Left = new CustomCondition()
                        {
                            TargetObject = new TargetObject()
                            {
                                GetList = new GetList()
                                {
                                    HasPropertiesListKey = "allegiances",
                                    FilterCondition = new PropertyCondition()
                                    {
                                        PropertyKey = "keyName",
                                        ConstantStringEqual = "leafcutterAllegiance#1"
                                    },
                                    NextList = new GetList()
                                    {
                                        HasPropertiesListKey = "members"
                                    }
                                }
                            },
                            ListCondition = new ListCondition()
                            {
                                CountMaximum = new ValueNode()
                                {
                                    PropertyKey = "maxLeafcutters"
                                }
                            }
                        },
                        Operator = OperatorType.And,
                        Right = new CustomCondition()
                        {
                            TargetObject = new TargetObject() { TargetObjectType = TargetObjectType.Root, 
                            GetList = new GetList()
                            {
                                    HasPropertiesListKey = "entities", FilterCondition = new PropertyCondition() { PropertyKey = "name", ConstantStringEqual = "Field quadite nest 1" }
                            }},
                             ListCondition = new ListCondition()
                             {
                                  CountEqual = 1
                             }
                        }
                    },

                ActionSetsKey = "continualSpawnLeafcutter#1"
            });
            #endregion
            

            #endregion*/

         /*
            #region leafCutterNest
            list.Add(new PolledEventType()
            {
                KeyName = "COVEMAP_spawnFieldQuaditeNests",
                ActionSets = new ActionSets()
                {
                    SetsOfActions = new[]
                    {
                        new ActionSetType("79dfgshsfghfsgzjshgfjs346x34fhjhs6e20")
                        {
                            Actions = new EventActionType[]
                            {
                                new SpawnEntityAction("4e8dghzjdgjdghjd56666665gjdgxjdg503c1")
                                {                
                                    EntityDataKey = new ValueNode() { String = "fieldQuaditeNest1" }                                                                
                                }                               
                            }
                        },
                    }
                }
            });

            #endregion*/

            return list;
        }
    }
}
