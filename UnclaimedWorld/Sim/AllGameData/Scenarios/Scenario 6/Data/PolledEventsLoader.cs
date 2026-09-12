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
//using UWGame.SimSide.InGameEvents.Expressions;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_6.Data
{
    public class PolledEventsLoader
    {

        public static List<InGameEvents.PolledEventType> Init()
        {
            List<PolledEventType> list = new List<PolledEventType>();
            
            #region lock above fence
            list.Add(new PolledEventType()
            {
                KeyName = "placeTerritoryLockFence",

                ActionSetsKey = "placeTerritoryLockFenceEvent"
            });

            #endregion

            #region  Area trigger Before fence

            list.Add(new PolledEventType()
            {
                KeyName = "triggerAtFence", // triggers an event window about the patricians to the north
                UseDefaultPollInterval = true,
                AllowRandomTimeOffset = true,
                Condition = new AreaCondition()
                {
                    Area = new Rectangle(1392, 1296, 288, 184)  // close to abatis fence (X,Y) , width, height. (pixels)
                },

                ActionSetsKey = "atFenceEvent"

            });

            #endregion

            #region 'Win' game (end of Story part)

            string endGameTooltip = "GO HOME. Clicking here will end your current game and take you to the main menu.";
            string continueGameTooltip = "STAY. Continue playing and see if you can build up the colony.";

            ///////////////////////////////////////////////////////////////////////////////
            list.Add(new PolledEventType()
            {
                KeyName = "Scenario6_winGame",
                StartTimePoint = new TimePoint()
                {
                    AbsoluteDate = new ValueNode() { PropertyKey = "endDate" }                   
                },
                Condition = new ConditionFunction()
                {
                    Left = new PlayerAllegiancePersons() { MinMembers = 3 },
                    Right = new CustomCondition()
                    {
                        PropertyCondition = new PropertyCondition() { PropertyKey = "Stores40Mudbricks", BoolValue = true } //so that it only fires when objective has been reached.  (this event comes from eventhooksloader, not like the other screens, that's why this is needed.)
                    }
                },

                
                ActionSets = new ActionSets()
                {
                    SetsOfActions = new []{ new ActionSetType("c8a8e1843w5354dgfhjyppe-4c7a-92wda0-251990e6a1f2")
                    {                       
                        Actions = new EventActionType[]{
          
                    new WinGameAction("b21eb90af42545ylooipp529-9261-083dwb7c39bdba") //this is a modal event screen dialog on the game area
                {
                       
                            // this can be null. then the modal dialog will be skipped:
                            ModalDialogText = new DynamicText(){ 
                                Text = "-Today is the #ENDDATE and that's the end of our contract! Let's get the barge to pick up our tools and any remaining mudbricks and then we'll go home. \n-About that...Since we work so good together, we've been talking about staying, making a permanent settlement. We're tired of doing these small-time operations. When the clay pit is empty, we can dig bog ore in the bog to the north. Start some blacksmithing, sell tools to Tellus.... \n-And the patricians? I don't think they'll be happy with us invading their fishing spots. \n-We can deal with them. It's just a small flock. \n-Well, who's in? Let's see a show of hands!", // NA: if not filled it will not fire
                            SubstitutionValues = new []
                                         {
                                             new SubstituteValue(){ Placeholder = "#ENDDATE", PropertyName = "endDate" }                                           
                                         }
                            
                            },
                            ModalDialogImage = "GroupMeeting",


                            WinScreenText = "-Can't say that I'm happy about leaving already. \n-You like digging clay? \n-The work was hard up there...but I think we had a chance of real freedom, of building something new. Too bad people gave up so early. \n-Pff... Going up against patricians with pickaxes and shovels. No thank you. I'm just happy to get back alive, a bit of money in my pocket. \n-Maybe next year then.",
                        
                            EndGameTooltip = endGameTooltip,
                            ContinueGameTooltip = continueGameTooltip,

                            ContinueActions = new ActionSets()
                            {
                                SetsOfActions = new ActionSetType[]
                                {
                                    new ActionSetType("c28bc87f-9ffb-4f53-a265-b3696ec365b0")
                                    {
                                        Actions = new EventActionType[]
                                        {
                                            new SetPropertyAction("1be5b4af-1357-4f84-860d-168b5ce4b416")
                                            {
                                               
                                                    PropertyKey = "storyPartOver", 
                                                    Value = new ValueNode() { Bool = true }
                                                
                                            }
                                        }
                                    }
                                }
                            }
                          
                        
                        

                },

                ////////// A territory blocker protects the abatis from being salvaged before the tut is over. after that, it is destroyed, making the abatis accessible. 
                ////destroys the territory locks on the map 
                new DestroyEntityAction("88d113cdfghdfghdgfhdfgddhdfghgf0a4760") { DelayInSeconds = 1, 
               			EntityName = "territoryLockClaypit"  },
/*
                new EventActionType("88d113cdfghdfghdgfdsahdfghdfghgf0a4760") { DelayInSeconds = 1, 
                DestroyEntity = new DestroyEntityAction(){EntityName = "territoryLockClaypit2" } },*/
                                            ///////////////////////////////////////////////////////

//maybe this track destroys the playlist?does music play after continuing game?:
                new MusicAction("df5c6b7c-f7c9-40b6-9486-c74075d649f4")
                {
                       
                            Song = "Martin Hasseldam - A New World (Alt3) 320kBit" 
                        
                }
                        }
                    }
                }
                }
            });
            #endregion

            #region Contract ended
            list.Add(new PolledEventType()
            {
                KeyName = "Scenario6_loseContract",
                StartTimePoint = new TimePoint()
                {
                    AbsoluteDate = new ValueNode() { PropertyKey = "endDate" }                   
                },
                
                Condition = new CustomCondition()
                {

                    PropertyCondition = new PropertyCondition() { PropertyKey = "Stores40Mudbricks", BoolValue = false } //The check that tells if the objective has been done or not -NA
                    
                },


                ActionSets = new ActionSets()
                {
                    SetsOfActions = new []{ new ActionSetType("2452ddwd652-4370-4469-9c4e-a1ebcabe0b83")
                    {                       
                        Actions = new EventActionType[]
                        {          
                            new LoseGameAction("db7871a4-fe7bw-413ddd-a67a-1ec7babe0c209")

                            {
                                    
                                    // text on first box that fires -NA
                                    ModalDialogText = new DynamicText(){ Text = "-Time's up, folks. Our contract ends today, on the #ENDDATE! Unfortunately, we didn't manage to produce the required amount of 40 mudbricks. The barge will be here shortly to pick up our tools and then we'll go home. \n-How did this happen, guys? \n-I think each of us has their own opinion on that...let's just keep it that way.",

                                        
                                        SubstitutionValues = new[]
                                {
                                    new SubstituteValue(){ Placeholder = "#ENDDATE", PropertyName = "endDate" } ,
                                 //   new SubstituteValue() { Placeholder = "#DATE", PropertyName = "getDate" } not used here
                                }
                            },
                                    ModalDialogImage = "GroupMeeting",

                                    LoseScreenText = " \n-Hey, I heard about your operation...what went wrong up there? \n-I dunno. The crew couldn't agree so we didn't meet our quota of mudbricks. I might go back next year though. Unless it's the same group of clowns that sign up..." 
                                  
                            },
                            new MusicAction("a845406sssda0-6bbsd5-4176-8903-30d4473aa068")
                            {
                               
                                    Song = "Martin Hasseldam - Unfamiliar Starlight" 
                                
                            }
                        }
                    }
                }
                }
            });
            #endregion

            #region intro dialogue , barks
            //

            list.Add(new PolledEventType()
            {
                KeyName = "CLAYPIT_introDialogue",
                StartTimePoint = new TimePoint()
                {
                    RelativeNoOfDays = 0.0038
                },                
                ActionSets = new ActionSets()
                {
                    SetsOfActions = new []
                    {
                        new ActionSetType("79356asfxzzxcewrweeyurtysusrtyurysu20")
                        {
                            Actions = new EventActionType[]
                            {
                                new TalkAction("ab94etyu5xzcvcvrewt673567u5e6uetyueyuyurt9a")
                                {
                                        DelayInSeconds = 0,
                                       
                                            TalkPriority = TalkAction.TalkActionPriority.High,
                                            CanTalkWhileFighting = false,
                                            CanTalkWhileSleeping = false,
                                            CanTalkWhileThreatened = false,
                                            TurnTowardsListeners = true,
                                            ActionByAgent = ActionByAgent.RandomInAllegiance,
                                            SpeakerDenomination = TalkAction.SpeakerInConversation.First,    
                                            DefaultText = "Alright, done. So - what's next?" // 
                                        
                                },


                            }
                        }
                    }
                }
            });

#endregion


            #region intro Dialogue screen Tut
            list.Add(new PolledEventType()
            {
                KeyName = "CLAYPIT_introScreenTut",  //match "CLAYPIT_introScreenNoTut"  with most things.
                StartTimePoint = new TimePoint()
                {
                    RelativeNoOfDays = 0.006
                },               
                ActionSets = new ActionSets()
                {
                    SetsOfActions = new []{ new ActionSetType("6f0467867486xzxzxzccxvb8674864786478ee92")
                    { 
                        Actions = new EventActionType[]
                        {

                            new EventActionDialog("5325367563756dafry646563876487648644f4e51") 
                            {                          
                               
                                    DisplayText = new DynamicText()
                                    { 
                                        Text = "-Nice work setting up the camp. Those abatis are guaranteed to keep the patricians out. Now, you know what we're here for: Mudbricks. We want to ship as many mudbricks as possible back to Tellus before the contract ends. So, this is the deal: \n \n-Before #ENDDATE we must store AT LEAST 40 mudbricks in our camp! \n \n-But we'll do this step by step! First, let's build the kiln. We're going to need more stones for that. \n \n- // Build a kiln // -" ,  //diff from below  
                                         SubstitutionValues = new []
                                         {
                                             new SubstituteValue(){ Placeholder = "#ENDDATE", PropertyName = "endDate" }                                           
                                         }
                                    },
                                    DisplayImage = "GroupMeeting",
                                    DialogOptions = new[] 
                                    { 
                                        new DialogOption() //diff from below
                                        {
                                            Text = "GUIDE #1", Tooltip = "See how to build a Kiln (opens separate window.)", ActiveInArchive = true,
                                            ActionSet = "showTutorialClayScenario6_1" // Tutorial_Scenario6Kiln
                                        }
                                    }                        
                                                  
                            },
                        }
                    }
                }

                }
            });
            #endregion

            #region intro Dialogue screen NO Tut
            list.Add(new PolledEventType()
            {
                KeyName = "CLAYPIT_introScreenNoTut",
                StartTimePoint = new TimePoint()
                {
                    RelativeNoOfDays = 0.006
                },
                ActionSets = new ActionSets()
                {
                    SetsOfActions = new []{ new ActionSetType("6f0467867www48674886748674864786478ee92")
                { 
                        Actions = new EventActionType[]
                        {

                      new EventActionDialog("53253675637sd5rtyhrsthsgfhgfsh48644f4e51") { DelayInSeconds = 0, 
                       
                                    DisplayText = new DynamicText()
                                    { 
                                        Text = "-Good job setting up the camp. Those abatis are guaranteed to keep the patricians out. Now, you know what we're here for: Mudbricks. We want to ship as many mudbricks as possible back to Tellus before the contract ends. So, this is the deal: \n \n-Before #ENDDATE we must store AT LEAST 40 mudbricks in our camp! \n \n-But I expect we can make and sell a lot more. Let's get started!" ,  //  diff from above.
                                SubstitutionValues = new []
                                         {
                                             new SubstituteValue(){ Placeholder = "#ENDDATE", PropertyName = "endDate" }                                           
                                         }
                                    },
                        DisplayImage = "GroupMeeting",
                           
                      
                                },
                        

                        }
                }
                }

                }
            });
            #endregion




            #region 40 mudbricks stored
            list.Add(new PolledEventType()
            {
                KeyName = "TUTORIAL_40mudBricksStored",  // 
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
                                ConstantStringEqual = "item:solidMudBrick"
                            }
                        }
                    },
                    ListCondition = new ListCondition()
                    {
                        CountMinimum = new ValueNode()
                        {
                            Decimal = 40.0f
                        }
                    }
                },
                ActionSetsKey = "40mudBricks"
            });

            list.Add(new PolledEventType()
            {
                KeyName = "TUTORIAL_check40MudBricks",  // looks for a property that has been set some time after the 40 mudbricks were made..so we get a delay. MP
                PollInterval = new ValueNode() { Decimal = 1f },  // seconds
                AllowRandomTimeOffset = true,
                Condition = new CustomCondition()
                {
                    AllowWhileAllPlayerMembersAreSleepingOrCollapsed = false,
                    AllowWhilePlayerThreatened = false,  // player allegiance threatened
                    AllowWhilePlayerMemberIsFighting = false,
                    PropertyCondition = new PropertyCondition() { PropertyKey = "40MudBricksStoredDelay", BoolValue = true }

                },
                ActionSetsKey = "makeMoreMudBricksDelayEvent" // 
            });

            #endregion
            #region
            list.Add(new PolledEventType()
            {
                KeyName = "TUTORIAL_checkIfContinueGame",  // looks for a property that is set immediately when player chooses to continue the game at the end of tutorial MP
                PollInterval = new ValueNode() { Decimal = 1f },  // seconds
                AllowRandomTimeOffset = true,
                Condition = new CustomCondition()
                {
                    AllowWhileAllPlayerMembersAreSleepingOrCollapsed = false,
                    AllowWhilePlayerThreatened = false,  // player allegiance threatened
                    AllowWhilePlayerMemberIsFighting = false,
                    PropertyCondition = new PropertyCondition() { PropertyKey = "storyPartOver", BoolValue = true }

                },
                ActionSetsKey = "continueGameEvent" // 
            });
             #endregion           

            #region Stores40Mudbricks
            list.Add(new PolledEventType()
            {
                KeyName = "CONTRACTEND_40mudBricksStored",  // 40 stored
                PollInterval = new ValueNode() { Decimal = 1f },  // seconds               
                AllowRandomTimeOffset = true,
                Condition = new CustomCondition()
                {
                    AllowWhileAllPlayerMembersAreSleepingOrCollapsed = false,
                    AllowWhilePlayerThreatened = false,  
                    AllowWhilePlayerMemberIsFighting = false,

                    TargetObject = new TargetObject()
                    {
                        GetList = new GetList()
                        {
                            HasPropertiesListKey = "finishedEntities",
                            FilterCondition = new PropertyCondition()
                            {
                                PropertyKey = "type",
                                ConstantStringEqual = "item:solidMudBrick"
                            }
                        }
                    },
                    ListCondition = new ListCondition()
                    {
                        CountMinimum = new ValueNode()
                        {
                            Decimal = 40.0f
                        }
                    }
                },

               ActionSets = new ActionSets()
               {
                   SetsOfActions = new []{ new ActionSetType("c8a8e18afadsft4t3w4iopdxx1e-4c7a-92wda0-251990e6a1f2")
                    {                       
                        Actions = new EventActionType[]{         
                    
                            new SetPropertyAction("b21ebfa3254wt5yuujhiopp9261-083dwb7c39bdba")                
                            {
                                  
                                PropertyKey = "Stores40Mudbricks", 
                                Value = new ValueNode() { Bool = true }
                            }
                        }
                    }          
                }
               },

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
                    SetsOfActions = new []{ new ActionSetType("2452d652-4370-4469-9c4e-a1e3254tg-fhjops3frt-yuiopvvbgfddss")
                    {                       
                        Actions = new EventActionType[]
                        {          
                            new LoseGameAction("db7871a4-fe7b-4saf3265233d-a67a-1ec7bbe0c209") // I don't want any modal text here, just fade to main screen for the end game message  -MP
                            {                                                         
                                LoseScreenText = " \nWith the instinctive willpower of pioneers, humans strove to tame a planet whose instincts told it to resist. This duel went on for generations as hope was built, crushed and rebuilt, and lessons were repeatedly learned and forgotten. \n \nTime would tell if the human presence on Antheia was just a temporary incursion or if they were destined to dominate this biosphere just like Earth." 
                                  
                            },
                            new MusicAction("a845406ttt0-6bb5-4176-8903-30d4473aa068")
                            {                               
                                Song = "Martin Hasseldam - Unfamiliar Starlight"                                 
                            }
                        }
                    }
                }
                }
            });
            #endregion


     
            
            #region  Area trigger zones
            //the area triggers are at home here because they are continually polled:
            // 
            list.Add(new PolledEventType()
            {
                KeyName = "CLAYPIT_triggerMudWormsWest",
                UseDefaultPollInterval = true,
                AllowRandomTimeOffset = true,
                Condition = new AreaCondition()
                {
                    Area = new Rectangle(768, 768, 144, 144)  // upper left corner coord.(X,Y) , width, height. (pixels)
                }
                ,
                ActionSetsKey = "spawnMudWormsWest" //;put some  talk at spawn?

            });


            list.Add(new PolledEventType()
            {
                KeyName = "CLAYPIT_triggerMudWormsCenter",
                UseDefaultPollInterval = true,
                AllowRandomTimeOffset = true,
                Condition = new AreaCondition()
                {
                    Area = new Rectangle(1920, 1776, 144, 144)  // upper left corner coord.(X,Y) , width, height. (pixels)
                }
                ,
                ActionSetsKey = "spawnMudWormsCenter" //;put some  talk at spawn?

            });

            #endregion

            return list;
        }
    }
}
