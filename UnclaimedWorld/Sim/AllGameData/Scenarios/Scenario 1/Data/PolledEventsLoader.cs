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
using UWGame.SimSide.XmlCollections;
using UWGame.SimSide.Entities.Biological;
//using UWGame.SimSide.InGameEvents.Expressions;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_1.Data
{
    public class PolledEventsLoader
    {

        public static List<InGameEvents.PolledEventType> Init()
        {
            List<PolledEventType> list = new List<PolledEventType>();

 
            #region DEMOISLANDMAP_musicTrackList //
            list.Add(new PolledEventType()
            {
                KeyName = "DEMOISLANDMAP_musicTrackList", //mp difference from fields of tau ceti is that i've moved Settle to the top and swapped its place with building a home/prosperous froniter and muckroot toil
                PollInterval = new ValueNode() { Decimal = 3200f }, //cycle 2 days
                AllowRandomTimeOffset = false, //no staggering
                ActionSets = new ActionSets()
                {
                    SetsOfActions = new []{ new ActionSetType("24erwtyerwytrwywtrya1ebcabe0b83")
                    {                       
                        Actions = new EventActionType[]
                        {      
                            new MusicAction("wretw6537536736573567yutydtyudtyu4674674674tyu68")
                            {   DelayInSeconds = 0f, //
                                
                                    Song = "Martin Hasseldam - Settle"  // duration 15 min 40 sec = 940 sec  ....BUT, it has a lot of silence in the end (after 15 min 15 sec), so the more accurate, cropped lenght is 915 sec......number of days:  1/1600=0,000625 *915=0,572  (cropped)
                                
                            },


                            new MusicAction("a5e635675368735874durtydsu5sw65eswtywraa068")  //kinda getting dark
                            {   DelayInSeconds = 915f,//previous cropped
                                
                                    Song = "Jesper Lundager - Cetian Skies_320"  // duration - 4 min 42 sec   = 282 sec ...hard cropped: 4:34 = 274 sec........number of days: 1/1600=0,000625 *282 = 0,176.....0,171 (hard cropped)
                                
                            },
                            new MusicAction("ad356753673567eyudygjutsduyudtyu68")
                            {   DelayInSeconds = 1189f, ////915f+274f    hard cropped previous
                                
                                    Song = "Martin Hasseldam - Unfamiliar Starlight"  // duration 3 min 58 sec = 238 sec .......number of days: 1/1600=0,000625 *238 = 0,149
                                
                            },
                            new MusicAction("add356756387358738utydtyudtyu4674674674tyu68")
                            {   DelayInSeconds = 1427f, //915f+274f+238f
                                
                                    Song = "Martin Hasseldam - Life in the Wilderness"  // duration 9 min 10 sec = 550 sec. ...with a lot of the end silence cropped, it's: 8 min 44 sec = 524 sec........number of days: 1/1600=0,000625 *550=0,344  ...Cropped: 0,000625 * 524= 0,328
                                
                            },
                            new MusicAction("faf1467847686478674yyteyuteyutes3bab")
                            {   DelayInSeconds = 1951f, //915f+274f+238f+524f       previous cropped
                                
                                    Song = "Jesper Lundager - Building a Home_320" // duration  -  5 min 2 sec = 302 sec   duration number of days = 0,189.
                                
                            },
                            new MusicAction("a4678674864786746-8903-30d4473aa068")
                            {   DelayInSeconds = 2253f, //915f+274f+238f+524f +302f
                                
                                    Song = "Jesper Lundager - Prosperous Frontier_320"  // duration -  4 min 19 sec =  259 sec . number of days: 1/1600=0,000625 *259 = 0,162
                                
                            },
                            new MusicAction("a8467867486748647reewtyuw5rtyutywrutywraa068")
                            {   DelayInSeconds = 2512f, //915f+274f+238f+524f +302f + 259f
                                
                                    Song = "Jesper Lundager - Muckroot Toil_320"  // duration -  3 min 19 sec  = 199 sec ...number of days: 1/1600=0,000625 *199 = 0,124
                                
                            },
                            
                            new MusicAction("yt467864786748tyutyutyudtyudtyu4674674674tyu68")
                            {   DelayInSeconds = 2711f, //915f+274f+238f+524f +302f + 259f+199f
                                
                                    Song = "Jesper Lundager - The Diamond Birds_320"  //  duration - 3 min 1 sec    = 181 sec ........number of days: 1/1600=0,000625 *181 = 0,113
                                
                            },
                            new MusicAction("k476876486478iyuiyuiyuiyuidtyudtyu4674674674tyu68")
                            {   DelayInSeconds = 2892f, //915f+274f+238f+524f +302f + 259f+199f+181f
                                
                                    Song = "Martin Hasseldam - A New World (Alt3) 320kBit"  //  duration: 5 min 12 sec =312 sec ...hard cropped: 5 min 4 sec = 304 sec........number of days: 1/1600=0,000625 *312= 0,195 .....0,190 (hard cropped)
                                
                            }
                                 //cycle ends at 2892f+312= 3204f  ..so 4 seconds cut off this last track which is ok.
                        }
                    }
                }
                }
            });
            #endregion


            #region DEMOISLANDMAP Win/lose and timed exposition




            string endGameTooltip = "You have succeeded: At least one explorer has survived long enough to get rescued. Clicking here will end your current game and take you to the main menu.";
            string continueGameTooltip = "You have achieved the goal of the scenario, but you can keep on playing by selecting this option - you will NOT get rescued again however!";

            



            list.Add(new PolledEventType()
            {
                KeyName = "DEMOISLANDMAP_checkIfOnlyOneMemberLeft",  // sets a propertykey some time after the second last person dies
                StartTimePoint = new TimePoint() { RelativeTimeInSeconds = new ValueNode() { Decimal = 2f }},
                PollInterval = new ValueNode() { Decimal = 1f },  // seconds
                AllowRandomTimeOffset = true,
                Condition = new PlayerAllegiancePersons()
                {
                    MaxMembers = 1,
                    AllowWhileAllPlayerMembersAreSleepingOrCollapsed = true,
                    AllowWhilePlayerThreatened = true,  // player allegiance threatened
                    AllowWhilePlayerMemberIsFighting = true                    
                },

                ActionSets = new ActionSets()
                {

                    SetsOfActions = new []{ new ActionSetType("abd7c994-48ce-4407-95ff-7e80b057d7a1")
                    {      
                         MaxFirings = 1,
                        Actions = new EventActionType[]{
           
                    new SetPropertyAction("713985f2-090c-49c9-9c35-d20f492b3545")
                    { 
                        DelayInSeconds = 15,                    
                         PropertyKey = "onlyOneMemberLeft", 
                        Value = new ValueNode() { Bool = true} // causes single-survivor  dialogue: "DEMOISLANDMAP_onlyOneMemberLeftDialogue"
                    },
                  new SetPropertyAction("16731989-48fe-40e8-bf30-d39fdc58ce4c")
                            { 
                                DelayInSeconds = 200,
                                 PropertyKey = "onlyOneMemberLeftDelay", 
                                Value = new ValueNode() { Bool = true}} //causes minor win: "DEMOISLANDMAP_minorWinGame" below
                            
         
                        }
                    }
                }
                }
            });

            list.Add(new PolledEventType()
            {
                KeyName = "DEMOISLANDMAP_checkIfOnlyTwoMembersLeft",  // sets a propertykey some time after the second last person dies
                PollInterval = new ValueNode() { Decimal = 1f },  // seconds
                StartTimePoint = new TimePoint() { RelativeTimeInSeconds = new ValueNode() { Decimal = 2f } },
                AllowRandomTimeOffset = true,
                Condition = new PlayerAllegiancePersons()
                {
                    MinMembers = 2, MaxMembers = 2,
                    AllowWhileAllPlayerMembersAreSleepingOrCollapsed = true,
                    AllowWhilePlayerThreatened = true,  // player allegiance threatened
                    AllowWhilePlayerMemberIsFighting = true                    
                },

                ActionSets = new ActionSets()
                {

                    SetsOfActions = new []{ new ActionSetType("9eff5e0a-bd3f-4b88-8b60-900754228af1")
                    {      
                         MaxFirings = 1,
                        Actions = new EventActionType[]{
           
                    new SetPropertyAction("a02393b3-8ad6-41e3-b955-985634c278a7")
                { DelayInSeconds = 15,
                    
                         PropertyKey = "onlyTwoMembersLeft", 
                        Value = new ValueNode() { Bool = true }} // causes 2 survivor  dialogue: "DEMOISLANDMAP_onlyTwoMembersLeftDialogue"
                
                        }
                    }
                }
                }
            });

            list.Add(new PolledEventType()
            {
                KeyName = "DEMOISLANDMAP_checkIfOnlyThreeMembersLeft",  // sets a propertykey some time after the first person dies (first out of four)
                PollInterval = new ValueNode() { Decimal = 1f },  // seconds
                StartTimePoint = new TimePoint() { RelativeTimeInSeconds = new ValueNode() { Decimal = 2f } },
                AllowRandomTimeOffset = true,
                Condition = new PlayerAllegiancePersons()
                {
                    MinMembers = 3, MaxMembers = 3,
                    AllowWhileAllPlayerMembersAreSleepingOrCollapsed = true,
                    AllowWhilePlayerThreatened = true,  // player allegiance threatened
                    AllowWhilePlayerMemberIsFighting = true,

                    
                },

                ActionSets = new ActionSets()
                {

                    SetsOfActions = new []{ new ActionSetType("8761a295-462a-498a-baa3-fe6fe388abdd")
                    {      
                         MaxFirings = 1,
                        Actions = new EventActionType[]{
           
                    new SetPropertyAction("4ed443be-5425-4891-bbb2-559bd42454a0")
                { DelayInSeconds = 10,
                    
                         PropertyKey = "onlyThreeMembersLeft", 
                            Value = new ValueNode() { Bool = true}} // causes 3 survivor  dialogue: "DEMOISLANDMAP_onlyThreeMembersLeftDialogue"
                
                        }
                    }
                }
                }
            });




            list.Add(new PolledEventType()
            {
                KeyName = "DEMOISLANDMAP_minorWinGame",  // looks for a property that has been set some time after the second last person died..so we get a delay. MP
                PollInterval = new ValueNode() { Decimal = 1f },  // seconds
                StartTimePoint = new TimePoint() { RelativeTimeInSeconds = new ValueNode() { Decimal = 2f } },
                AllowRandomTimeOffset = true,
                Condition = new ConditionFunction()
                {
                    Operator = OperatorType.And,
                    Left = new CustomCondition()
                    {
                        AllowWhileAllPlayerMembersAreSleepingOrCollapsed = true,
                        AllowWhilePlayerThreatened = true,  // player allegiance threatened
                        AllowWhilePlayerMemberIsFighting = false,
                        //  TimeCondition = new TimeCondition() { RelativeTimeInSeconds = new ValueNode() { Int = 1 } },// WINTEST
                            PropertyCondition = new PropertyCondition() { PropertyKey = "onlyOneMemberLeftDelay", BoolValue = true }
                            
                    },
                    Right = new CustomCondition()
                    {
                        PropertyCondition = new PropertyCondition()
                        {
                            PropertyKey = "gameOver",
                            BoolValue = false
                        }
                    }    
                },
                ActionSets = new ActionSets()
                {
                    SetsOfActions = new []{ new ActionSetType("78730250-718f-467a-a362-eb6777819192")
                    {      
                        Actions = new EventActionType[]{
           ///MINOR WIN when only one is left. we make regularily checks to see if only one is left, so s(he) can get rescued...not so depressing!!
            new WinGameAction("0a175aee-3333-4ec2-ae43-df76d36b77a9")  //this is a modal event screen dialog on the game area
            {
                // this can be null. then the modal dialog will be skipped:
                ModalDialogText = new DynamicText()
                {
                    Text = "JOURNAL ENTRY #6 \n#JOURNALDATE \nLOCATION: 43 22.5N 124 17.7W \n#JOURNALNAMES  \n \nI knew they'd find me! An RM-22 aircraft is setting down. Cannot wait to leave this place!",
                    SubstitutionValues = new[]
                    {
                        new SubstituteValue(){ Placeholder = "#JOURNALDATE", PropertyName = "getDate", Formatting = FormattingOptions.BothDates },
                        new SubstituteValue() { Placeholder = "#JOURNALNAMES", PropertyName = "getJournalHeaderNames" }
                    }
                },
                ModalDialogImage = "Rescue",

                WinScreenText = "SEARCH AND RESCUE MISSION REPORT \n \nMISSION LEADER: Huan Yeoh \nDATE: 11-11 2134 \n \nAt 0653 we picked up a comm signal from 43 22.5N 124 17.7W and shortly after located one of the missing individuals. There were no other survivors. We provided treatment and transport back to Duke's Landing. \nAll missing people from the swarm attack have now been accounted for. This concludes the search and rescue mission.",

                EndGameTooltip = endGameTooltip,
                ContinueGameTooltip = continueGameTooltip,

                ContinueActions = new ActionSets()
                {
                    SetsOfActions = new ActionSetType[]
                    {
                        new ActionSetType("7cb86151-29db-43d3-87d8-52d6e9d9c660")
                        {
                            Actions = new EventActionType[]
                            {
                                new SetPropertyAction("d5330503-d03d-441a-bc05-8c18562627ab")
                                {
                                    
                                        PropertyKey = "gameOver", 
                                        Value = new ValueNode() { Bool = true }
                                    
                                }
                            }
                        }
                    }
                }
            },
                  

                new MusicAction("5475edac-161b-47c8-a067-11228193aa82")
                {
                        
                            Song = "Martin Hasseldam - A New World (Alt3) 320kBit" 
                        
                }
                        }
                    }
                }
                }
            });




            ///////////////////////////////////////////////////////////////////////////////
            list.Add(new PolledEventType()
            {
                KeyName = "DEMOISLANDMAP_winGame",
                StartTimePoint = new TimePoint()
                { 
                    DynamicRelativeNoOfDays = new ValueNode() { PropertyKey = "winGameTime"  },  
                },
                Condition = new ConditionFunction()
                {                        
                    Operator = OperatorType.And,
                    Left = new PlayerAllegiancePersons()
                    {
                        MinMembers = 2
                    },
                    Right = new CustomCondition()
                    {
                        PropertyCondition = new PropertyCondition()
                        {
                            PropertyKey = "gameOver",
                            BoolValue = false
                        }
                                                                   
                    }                            
                        
                    
                }
                ,
                ActionSets = new ActionSets()
                {
                    SetsOfActions = new []{ new ActionSetType("c8a8e18a-d51e-4c7a-92a0-251990e6a1f2")
                    {                       
                        Actions = new EventActionType[]{
          
                    new WinGameAction("b21eb90e-b8ef-4529-9261-083b7c39bdba")//this is a modal event screen dialog on the game area
                {
                       
                            // this can be null. then the modal dialog will be skipped:
                            ModalDialogText = new DynamicText(){ Text = "JOURNAL ENTRY #6 \n#DATE \nLOCATION: 43 22.5N 124 17.7W \n#JOURNALNAMES  \n \nOur long wait is over! We've been found. An RM-22 aircraft is setting down. Cannot wait to leave this place!",
                                SubstitutionValues = new[]
                                {
                                    new SubstituteValue() { Placeholder = "#JOURNALNAMES", PropertyName = "getJournalHeaderNames" },
                                    new SubstituteValue() { Placeholder = "#DATE", PropertyName = "getDate" }
                                }
                            },
                            ModalDialogImage = "Rescue",

                            WinScreenText = "SEARCH AND RESCUE MISSION REPORT \n \nMISSION LEADER: Huan Yeoh \nDATE: 11-11 2134 \n \nAt 0653 we picked up a comm signal from 43 22.5N 124 17.7W and shortly after located the missing individuals. They were given treatment and transported back to Duke's Landing. \nAll missing people from the swarm attack have now been accounted for. This concludes the search and rescue mission.",
                        
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
                                                
                                                    PropertyKey = "gameOver", 
                                                    Value = new ValueNode() { Bool = true }
                                                
                                            }
                                        }
                                    }
                                }
                            }
                          
                        
                        

                },

                new MusicAction("df5c6b7c-f7c9-40b6-9486-c74075d649f4")
                {
                        
                            Song = "Martin Hasseldam - A New World (Alt3) 320kBit" 
                        
                }
                        }
                    }
                }
                }
            });


            list.Add(new PolledEventType()
            {
                KeyName = "DEMOISLANDMAP_loseGame",
                UseDefaultPollInterval = true,
                AllowRandomTimeOffset = true,
                StartTimePoint = new TimePoint() { RelativeTimeInSeconds = new ValueNode() {  Decimal = 2 } }, 
           
                Condition = new PlayerAllegiancePersons() // condition: all dead -MP
                        { MaxMembers = 0 
                        
                        //TimeCondition = new TimeCondition() { RelativeTimeInSeconds = new ValueNode() { Int = 1 } },// LOSETEST                    
                }
                ,
                ActionSets = new ActionSets()
                {
                    SetsOfActions = new []{ new ActionSetType("2452d652-43ht3esrthyuipdcvb-69-9c4e-a1ebcabe0b83")
                    {                       
                        Actions = new EventActionType[]
                        {          
                            new LoseGameAction("db725gd1a4-fe7b-413d-a67a-1ec7bbe0c209") // I don't want any modal text here, just fade to main screen for the end game message  -MP
                            {
                                                       
                                    LoseScreenText = "SEARCH AND RESCUE MISSION REPORT \n \nMISSION LEADER: Huan Yeoh \nDATE: 11-11 2234 \n \nAt 0653 we detected a survival suit beacon from 43 22.5N 124 17.7W. We found human remains and took DNA samples that decisively establish that these were the missing individuals we've been searching for. There were no survivors. \n \nWe identified and marked the graves that we found. Unburied remains were recovered along with any personal items and were transported back to Duke's Landing. \nAmong the recovered items are Pioneer tablets with a journal of events. The file is included with this mission report. \n \nAll missing people from the swarm attack have now been accounted for. This concludes the search and rescue mission."
                                
                            },
                            new MusicAction("hhha8454060-6bb5-417hhy6-8903-30d4473aa068")
                            { 
                                
                                    Song = "Martin Hasseldam - Unfamiliar Starlight" 
                                
                            }
                        }
                    }
                }
                }
            });

            //////////

            # region Twinkler + nest + bushdragon exposition checks

            list.Add(new PolledEventType()
            {
                KeyName = "DEMOISLANDMAP_checkQuaditeCarcassDetected",  // looks for a property that has been set just when the quadite carcass is detected
                PollInterval = new ValueNode() { Decimal = 10f },  //I'm setting some seconds between checks, reduces risk of firing immediately when a fight ends
                AllowRandomTimeOffset = true,
                StartTimePoint = new TimePoint() { RelativeTimeInSeconds = new ValueNode() { Decimal = 2 }},
                Condition = new CustomCondition()
                {
                    AllowWhileAllPlayerMembersAreSleepingOrCollapsed = false,
                    AllowWhilePlayerThreatened = false,  // player allegiance threatened
                    AllowWhilePlayerMemberIsFighting = false,
                    PropertyCondition = new PropertyCondition() { PropertyKey = "quaditeCarcassDetected", BoolValue = true }
                                            
                },
                ActionSetsKey = "quaditeCarcassDetectedEvent"
            });

            list.Add(new PolledEventType()
            {
                KeyName = "DEMOISLANDMAP_checkQuaditeCarcassDetectedDelay",  // looks for a property that has been set some time after the quadite carcass is detected..so we get a delay. MP
                PollInterval = new ValueNode() { Decimal = 10f },  // seconds
                AllowRandomTimeOffset = true,
                StartTimePoint = new TimePoint() { RelativeTimeInSeconds = new ValueNode() { Decimal = 2 } },
                Condition = new CustomCondition()
                {
                    AllowWhileAllPlayerMembersAreSleepingOrCollapsed = false,
                    AllowWhilePlayerThreatened = false,  // player allegiance threatened
                    AllowWhilePlayerMemberIsFighting = false,
                    PropertyCondition = new PropertyCondition() { PropertyKey = "quaditeCarcassDetectedDelay", BoolValue = true }                       
                    
                },
                ActionSetsKey = "quaditeCarcassDetectedDelayEvent"
            });


            list.Add(new PolledEventType()
            {
                KeyName = "DEMOISLANDMAP_checkNestDetectedDelay",  // looks for a property that has been set some time after the nest was detected..so we get a delay. MP
                PollInterval = new ValueNode() { Decimal = 8f },  // seconds
                AllowRandomTimeOffset = true,
                StartTimePoint = new TimePoint() { RelativeTimeInSeconds = new ValueNode() { Decimal = 2 } },
                Condition = new CustomCondition()
                {
                    AllowWhileAllPlayerMembersAreSleepingOrCollapsed = false,
                    AllowWhilePlayerThreatened = false,  // player allegiance threatened
                    AllowWhilePlayerMemberIsFighting = false,
                    PropertyCondition = new PropertyCondition() { PropertyKey = "nestDetectedDelay", BoolValue = true }
                                            
                },
                ActionSetsKey = "nestExpositionEvent"
            });


            list.Add(new PolledEventType()
            {
                KeyName = "checkBushDragonDetectedShortDelay",  // looks for a property that has been set shortly after the dragon was detected..so we get a delay. MP
                PollInterval = new ValueNode() { Decimal = 8f },  // seconds
                AllowRandomTimeOffset = true,
                StartTimePoint = new TimePoint() { RelativeTimeInSeconds = new ValueNode() { Decimal = 2 } },
                Condition = new CustomCondition()
                {
                    AllowWhileAllPlayerMembersAreSleepingOrCollapsed = false,
                    AllowWhilePlayerThreatened = false,  // player allegiance threatened
                    AllowWhilePlayerMemberIsFighting = false,
                    PropertyCondition = new PropertyCondition() { PropertyKey = "bushDragonDetectedShortDelay", BoolValue = true }                        
                    
                },
                ActionSetsKey = "bushDragonDetectedDelayEvent"
            });

            list.Add(new PolledEventType()
            {
                KeyName = "checkBushDragonDetectedLongDelay",  // looks for a property that has been set long after the dragon was detected..so we get a delay. MP
                PollInterval = new ValueNode() { Decimal = 8f },  // seconds
                AllowRandomTimeOffset = true,
                StartTimePoint = new TimePoint() { RelativeTimeInSeconds = new ValueNode() { Decimal = 2 } },
                Condition = new CustomCondition()
                {
                    AllowWhileAllPlayerMembersAreSleepingOrCollapsed = false,
                    AllowWhilePlayerThreatened = false,  // player allegiance threatened
                    AllowWhilePlayerMemberIsFighting = false,
                    PropertyCondition = new PropertyCondition() { PropertyKey = "bushDragonDetectedLongDelay", BoolValue = true }                        
                    
                },
                ActionSetsKey = "bushDragonDetectedDelayEvent"
            });




            # endregion

            ////

            #region general exposition (regardless of customization) such as night shift screen
            list.Add(new PolledEventType()
            {
                KeyName = "DEMOISLANDMAP_midnightText",
                StartTimePoint = new TimePoint()
                { 
                    Date = new DateAndTime.TimeDateYear() { Year = 0, Day = 1, TimeOfDay = 0.85 } //mp may 2015 this often fires at random times, way into morning.                       
                },
                Condition = new DummyCondition()
                {
                    AllowWhileAllPlayerMembersAreSleepingOrCollapsed = false,
                    AllowWhilePlayerThreatened = false,  // player allegiance threatened
                    AllowWhilePlayerMemberIsFighting = false                        
                } 
                    // old
                   /* Value = new TimeCondition()
                    {
                        AllowWhileAllPlayerMembersAreSleepingOrCollapsed = false,
                        AllowWhilePlayerThreatened = false,  // player allegiance threatened
                        AllowWhilePlayerMemberIsFighting = false                       
                    }  */                  
                
                ,
                ActionSets = new ActionSets()
                {
                    SetsOfActions = new []{ new ActionSetType("e3e458d6-1ddf-47ba-addb-260d45b1ad27")
                {
                        Condition = new PlayerAllegiancePersons() { MinMembers = 2 }
                        ,
                        Actions = new EventActionType[]
                        { 
                            
                        new EventActionDialog("9906341e-3ca2-4031-a7dd-cd7fcc7dbf1e")
                        {
                                
                                    DisplayText = new DynamicText(){ Text = "JOURNAL ENTRY #6 \n#DATE: 06-10 2238 \nLOCATION: 43 22.5N 124 17.7W \n#JOURNALNAMES \n \nWe will sleep in shifts to maintain security during the night. If predators appear, we should have sufficient warning.",
                                        SubstitutionValues = new[]
                                        {
                                            new SubstituteValue() { Placeholder = "#JOURNALNAMES", PropertyName = "getJournalHeaderNames" },
                                            new SubstituteValue() { Placeholder = "#DATE", PropertyName = "getDate", Formatting = FormattingOptions.BothDates }
                                        }
                                    },
                                    DisplayImage = "NightTime"
                                

                        }
                                
                    
                        }
                },
                new ActionSetType("ff66b224-84a3-42c2-b0e4-f5272abe9a4a")
                {
                        Condition = new PlayerAllegiancePersons() { MaxMembers = 1 }
                        ,
                        Actions = new EventActionType[]
                        {                            
                        new EventActionDialog("17bb739b-108e-4bce-b278-b8e461047c12")
                        {
                                
                                    DisplayText = new DynamicText()
                                    { 
                                        Text = "JOURNAL ENTRY #6 \n#DATE \nLOCATION: 43 22.5N 124 17.7W \n#JOURNALNAMES  \n\nAt some time in the night I will have to get some sleep. Hopefully the predators will stay away.",
                                        SubstitutionValues = new[]
                                        {
                                            new SubstituteValue() { Placeholder = "#JOURNALNAMES", PropertyName = "getJournalHeaderNames" },
                                            new SubstituteValue() { Placeholder = "#DATE", PropertyName = "getDate", Formatting = FormattingOptions.BothDates }
                                        }
                                    },
                                    DisplayImage = "NightTime"
                                

                        }
                                
                   
                        }
                }
                }
                }
            });
            #endregion


            #region intro dialogue: Start next to wreck




            list.Add(new PolledEventType()
            {
                KeyName = "DEMOISLANDMAP_introDialogue3People",
                StartTimePoint = new TimePoint()
                {
                    RelativeNoOfDays = 0.0038
                },
                Condition = new CustomCondition()
                {
                    PropertyCondition = new PropertyCondition()
                    {
                        PropertyKey = "fledAtStart",
                        BoolValue = false
                    }
                    
                },
                ActionSets = new ActionSets()
                {
                    SetsOfActions = new []{ new ActionSetType("6f02814d-0a04-4796-90a4-c2c685b0ee92")
                { 
                        Actions = new EventActionType[]
                        {
                            new TalkAction("54b75b99-b8ad-4687-9442-bcc26655ce27")
                        {                                 
                                
                                    TalkPriority = TalkAction.TalkActionPriority.High,
                                    CanTalkWhileFighting = false,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = false,
                                    TurnTowardsListeners = true,
                                    ActionByAgent = ActionByAgent.RandomInAllegiance,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                                    LineKey = "crashQuestion",
                                    DefaultText = "Is everyone OK?" //"It looks like everyone is OK. Let's see if anymore supplies survived the crash." //  "Again, sorry about the rough landing! I'm glad that you're all ok. Let's see if any supplies survived the crash."
                                
                        },
                        new TalkAction("cd2c0b10-34dd-4193-a1ac-1aa56a68c053")
                        {
                                DelayInSeconds = 3,
                                
                                    TalkPriority = TalkAction.TalkActionPriority.High,
                                    CanTalkWhileFighting = false,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = false,
                                    TurnTowardsListeners = true,
                                    ActionByAgent = ActionByAgent.RandomInAllegiance,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                                    LineKey = "crashAnswer",
                                    DefaultText = "A few bruises is all." // It looks like everyone is OK. Let's see if anymore supplies survived the crash." //  "Again, sorry about the rough landing! I'm glad that you're all ok. Let's see if any supplies survived the crash."
                                
                        },
                        new TalkAction("fc664df2-85ce-435a-844e-15a30e649ef8")
                        {
                                DelayInSeconds = 6,
                                
                                    TalkPriority = TalkAction.TalkActionPriority.High,
                                    CanTalkWhileFighting = false,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = false,
                                    TurnTowardsListeners = true,
                                    ActionByAgent = ActionByAgent.RandomInAllegiance,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                                    LineKey = "crashConclusion",
                                    DefaultText = "We might just have a chance, then." //  
                                
                        },
                        new TalkAction("d3ec94d9-1f83-4114-941b-64d202707cbb")
                        {
                                DelayInSeconds = 10,
                                
                                    TalkPriority = TalkAction.TalkActionPriority.High,
                                    CanTalkWhileFighting = false,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = false,
                                    TurnTowardsListeners = true,
                                    ActionByAgent = ActionByAgent.RandomInAllegiance,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.Third,
                                    LineKey = "crashSuggestion",
                                    DefaultText = "Let's look through our supplies, see what things survived the crash." //always about supplies because they start unloading.
                                
                        },
                        }
                }
                }

                }
            });




            list.Add(new PolledEventType()
            {
                KeyName = "DEMOISLANDMAP_introDialogue2People",
                StartTimePoint = new TimePoint()
                {
                    RelativeNoOfDays = 0.0038
                },
                Condition = new CustomCondition()
                {
                    PropertyCondition = new PropertyCondition()
                    {
                        PropertyKey = "fledAtStart",
                        BoolValue = false
                    }     
                    
                },
                
                ActionSets = new ActionSets()
                {
                    SetsOfActions = new []{ new ActionSetType("79d3f7c8-56da-43d1-b2de-ed0d97996e20")
                { 
                        Actions = new EventActionType[]
                        {
                            new TalkAction("4e8afb25-1d6f-4808-b751-8d7245a8503c")
                        {                                 
                                
                                    TalkPriority = TalkAction.TalkActionPriority.High,
                                    CanTalkWhileFighting = false,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = false,
                                    TurnTowardsListeners = true,
                                    ActionByAgent = ActionByAgent.RandomInAllegiance,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                               //     LineKey = "crashQuestion",
                                    DefaultText = "Rough landing. You OK?" //"It looks like everyone is OK. Let's see if anymore supplies survived the crash." //  "Again, sorry about the rough landing! I'm glad that you're all ok. Let's see if any supplies survived the crash."
                                
                        },
                        new TalkAction("ab94d087-d715-47e0-a1a6-bfbbcab8de9a")
                        {
                                DelayInSeconds = 3,
                                
                                    TalkPriority = TalkAction.TalkActionPriority.High,
                                    CanTalkWhileFighting = false,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = false,
                                    TurnTowardsListeners = true,
                                    ActionByAgent = ActionByAgent.RandomInAllegiance,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                              //      LineKey = "crashAnswer",
                                    DefaultText = "I'm alright." // It looks like everyone is OK. Let's see if anymore supplies survived the crash." //  "Again, sorry about the rough landing! I'm glad that you're all ok. Let's see if any supplies survived the crash."
                                
                        },
                        new TalkAction("f6d39acd-b7c1-4160-8be8-20c82fcb5824")
                        {
                                DelayInSeconds = 5,
                                
                                    TalkPriority = TalkAction.TalkActionPriority.High,
                                    CanTalkWhileFighting = false,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = false,
                                    TurnTowardsListeners = true,
                                    ActionByAgent = ActionByAgent.RandomInAllegiance,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                             //       LineKey = "crashConclusion",
                                    DefaultText = "Glad to hear that. We need to look out for each other now." 
                                
                        },
                        new TalkAction("c11a21ac-44bd-41f3-98b7-aa13b48ffa70")
                        {
                                DelayInSeconds = 8,
                                
                                    TalkPriority = TalkAction.TalkActionPriority.High,
                                    CanTalkWhileFighting = false,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = false,
                                    TurnTowardsListeners = true,
                                    ActionByAgent = ActionByAgent.RandomInAllegiance,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                           //         LineKey = "crashSuggestion",
                                    DefaultText = "Yes. Now let's look through our supplies, see what things survived the crash." 
                                
                        },
                        }
                }
                }

                }
            });

            list.Add(new PolledEventType()
            {
                KeyName = "DEMOISLANDMAP_introDialogueLotsOfSupplies",
                StartTimePoint = new TimePoint()
                {
                    RelativeNoOfDays = 0.02
                },
                Condition = new CustomCondition()
                    {
                        PropertyCondition = new PropertyCondition()
                        {
                            PropertyKey = "fledAtStart",
                            BoolValue = false
                        }
                    
                },

                ActionSets = new ActionSets()
                {
                    FireMode = ActionSetsToFire.RandomValid,
                    ChanceToFire = 1f, //chooses from below
                    SetsOfActions = new []{ 


                         new ActionSetType("1b2ba6af-fbdf-4fdb-a0ca-f55d967c4a76")
                         {      
                //              ChanceToFire = 0.1f,
                              MaxFirings = 1,
                              Condition = new PlayerAllegiancePersons()
                                  {
                                    MinMembers = 2 
                               }
                               , 
                               Actions = new EventActionType[]{new TalkAction("31bb0c3d-87ef-4e0e-a4a3-00c1b2f1d25b") {  
                                
                                TalkPriority = TalkAction.TalkActionPriority.High,
                                CanTalkWhileFighting = false,
                                CanTalkWhileSleeping = false,
                                CanTalkWhileThreatened = false,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
             //                   ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                DefaultText = "Weapons, equipment...I think we're covered." }, 

                              new TalkAction("5d93cff2-4685-4ffe-b5f5-24f6c1d591ca") { DelayInSeconds = 3.5, 
                              
                              TalkPriority = TalkAction.TalkActionPriority.High,
                              TurnTowardsListeners = false,
                              CanTalkWhileFighting = false,
                              CanTalkWhileSleeping = false,
                              CanTalkWhileThreatened = false,
                              SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
            //                  ActionByAgent = ActionByAgent.RandomInAllegiance,
                              DefaultText = "You managed to load a lot of gear, all up to the last minute. Well done." }, //We got a lot of supplies with us. No wonder the skimmer felt heavy.

                              new TalkAction("b0716c2e-62da-467c-b031-58135f4cf4a1") { DelayInSeconds = 7, 
                              
                              TalkPriority = TalkAction.TalkActionPriority.High,
                              TurnTowardsListeners = false,
                              CanTalkWhileFighting = false,
                              CanTalkWhileSleeping = false,
                              CanTalkWhileThreatened = false,
                              SpeakerDenomination = TalkAction.SpeakerInConversation.Second,  
            //                  ActionByAgent = ActionByAgent.RandomInAllegiance,
                              DefaultText = "Yeah, tried to grab as much stuff as possible." //removed we so it could be used for 2 people scenario: Yeah, we tried to grab as much stuff as possible.   // I don't believe in packing light.
                            
                        },
                        }
                }
                }

                }
            });


            list.Add(new PolledEventType()
            {
                KeyName = "DEMOISLANDMAP_introDialogueSomeSupplies",
                StartTimePoint = new TimePoint()
                {
                    RelativeNoOfDays = 0.02
                },
                Condition = new CustomCondition()
                {         
                    PropertyCondition = new PropertyCondition()
                    {
                        PropertyKey = "fledAtStart",
                        BoolValue = false
                    }    
                    
                },
                ActionSets = new ActionSets()
                {
                    FireMode = ActionSetsToFire.RandomValid,
                    ChanceToFire = 1f, //chooses from below
                    SetsOfActions = new []{ 

                         new ActionSetType("144bb697-e75d-4cc8-9f11-2dd4abc21fc0")
                         {      
                //              ChanceToFire = 0.1f,
                              MaxFirings = 1,
                              Condition = new PlayerAllegiancePersons(){ MinMembers = 2 }                               
                               , 
                               Actions = new EventActionType[]{new TalkAction("0ea00531-3555-4e1f-9cf2-c2c18b982d79") {  
                                
                                TalkPriority = TalkAction.TalkActionPriority.High,
                                CanTalkWhileFighting = false,
                                CanTalkWhileSleeping = false,
                                CanTalkWhileThreatened = false,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First, 
             //                
                                DefaultText = "So glad we grabbed that sentry unit in the last minute." // ....... "I wish I hadn't lost that sentry unit when we took off." 
                            }, 

                              new TalkAction("95d2c090-1f14-4996-82d3-651ed7638806") { DelayInSeconds = 4, 
                              
                              TalkPriority = TalkAction.TalkActionPriority.High,
                              TurnTowardsListeners = false,
                              CanTalkWhileFighting = false,
                              CanTalkWhileSleeping = false,
                              CanTalkWhileThreatened = false,
                              SpeakerDenomination = TalkAction.SpeakerInConversation.Second,  
            //                  ActionByAgent = ActionByAgent.RandomInAllegiance,
                              DefaultText = "Yep. It'll keep us safe while we settle down." 
                            //  "A Swarmer came straight at you. You had to drop it." 
                            },

                              new TalkAction("87b5b611-d9ad-440b-840a-ae7e309ecd63") { DelayInSeconds = 9, 
                              
                              TalkPriority = TalkAction.TalkActionPriority.High,
                              TurnTowardsListeners = false,
                              CanTalkWhileFighting = false,
                              CanTalkWhileSleeping = false,
                              CanTalkWhileThreatened = false,
                              SpeakerDenomination = TalkAction.SpeakerInConversation.First,             
                              DefaultText = "Let's hope it won't see too much action." },                              
                               
                               }
                            }

                            }
                }
            });


            list.Add(new PolledEventType()
            {
                KeyName = "DEMOISLANDMAP_introDialogueFewSupplies",
                StartTimePoint = new TimePoint()
                {
                    RelativeNoOfDays = 0.02
                },
                Condition = new CustomCondition()
                {
                    PropertyCondition = new PropertyCondition()
                    {
                        PropertyKey = "fledAtStart",
                        BoolValue = false
                    }                     
                },
                ActionSets = new ActionSets()
                {
                    FireMode = ActionSetsToFire.RandomValid,
                    ChanceToFire = 1f, //chooses from below
                    SetsOfActions = new []{ 


                         new ActionSetType("383444ca-468d-44a9-b12c-ce11653897f8")
                         {      
                //              ChanceToFire = 0.1f,
                              MaxFirings = 1,
                              Condition = new PlayerAllegiancePersons(){ MinMembers = 2 }                               
                               , 
                               Actions = new EventActionType[]{new TalkAction("566234d4-fc33-4be6-9df2-7b5bdb5db6bd") {  
                                
                                TalkPriority = TalkAction.TalkActionPriority.High,
                                CanTalkWhileFighting = false,
                                CanTalkWhileSleeping = false,
                                CanTalkWhileThreatened = false,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First, 
             //                
                                DefaultText = "We packed in an awful hurry..." //"I wish we could have saved more equipment. Like that sentry unit."
                            }, 

                              new TalkAction("680450b6-ece7-49ab-bc72-428c1ffc7ef1") { DelayInSeconds = 4, 
                              
                              TalkPriority = TalkAction.TalkActionPriority.High,
                              TurnTowardsListeners = false,
                              CanTalkWhileFighting = false,
                              CanTalkWhileSleeping = false,
                              CanTalkWhileThreatened = false,
                              SpeakerDenomination = TalkAction.SpeakerInConversation.Second,  
            //                  ActionByAgent = ActionByAgent.RandomInAllegiance,
                              DefaultText = "Yeah...with this few supplies, I see some tough times ahead." //"If we had spent more time in that place we might be dead now." 
                            //  "A Swarmer came straight at you. You had to drop it." 
                            },

                              new TalkAction("8088b451-372f-4c78-a96e-29a0a8a905c7") { DelayInSeconds = 9, 
                              
                              TalkPriority = TalkAction.TalkActionPriority.High,
                              TurnTowardsListeners = false,
                              CanTalkWhileFighting = false,
                              CanTalkWhileSleeping = false,
                              CanTalkWhileThreatened = false,
                              SpeakerDenomination = TalkAction.SpeakerInConversation.First,             
                              DefaultText = "All depends on what we encounter. But let's prepare for the worst." }}, //"I guess. But we could really use a turret here."                             
                               
                               
                            }

                            }
                }
            });

            list.Add(new PolledEventType()
            {
                KeyName = "DEMOISLANDMAP_introSuggestionLotsOfSupplies",
                StartTimePoint = new TimePoint()
                {
                    RelativeNoOfDays = 0.03
                },
                Condition = new CustomCondition()
                    {
                        PropertyCondition = new PropertyCondition()
                        {
                            PropertyKey = "fledAtStart",
                            BoolValue = false
                        }                   

                },
                ActionSets = new ActionSets()
                {
                    FireMode = ActionSetsToFire.RandomValid,
                    ChanceToFire = 1f, //chooses from below
                    SetsOfActions = new []{ 


                         new ActionSetType("4bc44169-3739-4607-9446-23ec3c4a55b0")
                         {      
                //              ChanceToFire = 0.1f,
                              MaxFirings = 1,
                              Condition = new PlayerAllegiancePersons(){ MinMembers = 2 }                               
                               , 
                               Actions = new EventActionType[]{new TalkAction("de86768a-b968-49e7-bdb0-fd0cccacc163") {  
                                
                                TalkPriority = TalkAction.TalkActionPriority.Low,
                                CanTalkWhileFighting = false,
                                CanTalkWhileSleeping = false,
                                CanTalkWhileThreatened = false,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,              //                
                                DefaultText = "Let's scout the surroundings, find out what kind of place this is." 
                            },                   
                                                         
                               
                               }
                            }

                            }
                }
            });


            list.Add(new PolledEventType()
            {
                KeyName = "DEMOISLANDMAP_introSuggestionSomeSupplies",
                 StartTimePoint = new TimePoint()
                 {
                     RelativeNoOfDays = 0.03
                 },
                Condition = new CustomCondition()
                {
                    PropertyCondition = new PropertyCondition()
                    {
                        PropertyKey = "fledAtStart",
                        BoolValue = false
                    }                                
                    
                },
                ActionSets = new ActionSets()
                {
                    FireMode = ActionSetsToFire.RandomValid,
                    ChanceToFire = 1f, //chooses from below
                    SetsOfActions = new []{ 


                         new ActionSetType("7b66eb54-a1c2-4400-9a38-2470e809c8fa")
                         {      
                //              ChanceToFire = 0.1f,
                              MaxFirings = 1,
                              Condition = new PlayerAllegiancePersons(){ MinMembers = 2 }                               
                               , 
                               Actions = new EventActionType[]{new TalkAction("b447fb16-2b0e-48e4-80b4-ec8ce1defd12") {  
                                
                                TalkPriority = TalkAction.TalkActionPriority.Low,
                                CanTalkWhileFighting = false,
                                CanTalkWhileSleeping = false,
                                CanTalkWhileThreatened = false,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,              //                
                                DefaultText = "Until we're rescued, I think our biggest concerns will be food and security." //Apart from security, we'll need to be concerned about food.  ////  "I'd recommend that we make some more weapons, the sooner the better."
                            },                   
                                                         
                               
                               }
                            }

                            }
                }
            });


            list.Add(new PolledEventType()
            {
                KeyName = "DEMOISLANDMAP_introSuggestionFewSupplies",
                StartTimePoint = new TimePoint()
                {
                    RelativeNoOfDays = 0.03
                },
                Condition = new CustomCondition()
                    {  
                        PropertyCondition = new PropertyCondition()
                        {
                            PropertyKey = "fledAtStart",
                            BoolValue = false
                        }     
                    
                },
                ActionSets = new ActionSets()
                {
                    FireMode = ActionSetsToFire.RandomValid,
                    ChanceToFire = 1f, //chooses from below
                    SetsOfActions = new []{ 


                         new ActionSetType("bb006220-852d-4fa5-a983-778376212d1a")
                         {      
                //              ChanceToFire = 0.1f,
                              MaxFirings = 1,
                              Condition = new PlayerAllegiancePersons(){ MinMembers = 2 }                               
                               , 
                               Actions = new EventActionType[]{new TalkAction("b9c3b54e-2f87-4298-99f5-6ef09c045641") {  
                                
                                TalkPriority = TalkAction.TalkActionPriority.Low,
                                CanTalkWhileFighting = false,
                                CanTalkWhileSleeping = false,
                                CanTalkWhileThreatened = false,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,              //                
                                DefaultText = "We're going to need more materials. Since the aircraft won't fly again, we might as well start scrapping it." 
                            },                   
                                                         
                               
                               }
                            }

                            }
                }
            });

            # endregion


            #region exposition: Start fled away from wreck ("fledAtStart" true)

//todo: add more. about unconscious team member.

            list.Add(new PolledEventType()
            {
                KeyName = "DEMOISLANDMAP_introDialogue3PeopleFled",
                StartTimePoint = new TimePoint()
                {
                    RelativeNoOfDays = 0.0038
                },
                Condition = new CustomCondition()
                {
                    PropertyCondition = new PropertyCondition()
                    {
                        PropertyKey = "fledAtStart",
                        BoolValue = true
                    }    
                    
                },
                ActionSets = new ActionSets()
                {
                    SetsOfActions = new []{ new ActionSetType("6fsfghgfshgfshhhhhhhhgfshgfsh2")
                { 
                        Actions = new EventActionType[]
                        {
                            new TalkAction("54b7sfghfghfghfghghfsfghgfsh27")
                        {                                 
                                
                                    TalkPriority = TalkAction.TalkActionPriority.High,
                                    CanTalkWhileFighting = false,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = false,
                                    TurnTowardsListeners = true,
                                    ActionByAgent = ActionByAgent.RandomInAllegiance,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                              //      LineKey = "crashQuestion",
                                    DefaultText = "That was a close escape. We found a bad place to crash-land, huh." //"It looks like everyone is OK. Let's see if anymore supplies survived the crash." //  "Again, sorry about the rough landing! I'm glad that you're all ok. Let's see if any supplies survived the crash."
                                
                        },
                        new TalkAction("cddgjhdgjkfhjhjhjhjhjhjhjhjhjkfhjhfjf3")
                        {
                                DelayInSeconds = 4,
                                
                                    TalkPriority = TalkAction.TalkActionPriority.High,
                                    CanTalkWhileFighting = false,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = false,
                                    TurnTowardsListeners = true,
                                    ActionByAgent = ActionByAgent.RandomInAllegiance,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                              //      LineKey = "crashAnswer",
                                    DefaultText = "Yeah, those twinklers were suddenly everywhere." // It looks like everyone is OK. Let's see if anymore supplies survived the crash." //  "Again, sorry about the rough landing! I'm glad that you're all ok. Let's see if any supplies survived the crash."
                                
                        },


                      new EventActionDialog("5323567777777etuutyuutyutut56751") { DelayInSeconds = 8, 
                        DisplayText = new DynamicText(){ Text = "AUDIO LOG: MEETING #1 \n \nLEHNER: So. What do we do now? \n \nYEBOAH: We have life signs from Laurent's medic unit. He's alive but sadly he's unconscious. Condition is stable though. \n \nCONLAN: His location? \n \nYEBOAH: Right where he fell out, when we slammed into that cliff. But he seems to be below ground. \n \nCONLAN: Poor guy fell down one of those crevices. \n \nLEHNER: So what's the decision guys. Ready to go back and find him? We can handle those couple of twinklers. \n \nYEBOAH: Might be better to take a measured approach. We don't have enough coil guns. There's a reason why we retreated in the first place. \n \nLEHNER: Then I suggest sharpening some cane to use as weapons. And we're gonna need some rope to haul him out of that hole. Let's get going! We might lose our friend while we stand here talking." ,  //  
                        },
                        DisplayImage = "GroupMeeting",
                                             
                                },

                        new TalkAction("fdhhhhhhhhhhhhhhhhjtyuirtyikryueir76iu6r78i678")
                        {
                                DelayInSeconds = 9,
                                
                                    TalkPriority = TalkAction.TalkActionPriority.High,
                                    CanTalkWhileFighting = false,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = false,
                                    TurnTowardsListeners = true,
                                    ActionByAgent = ActionByAgent.OnlySpecific,
                                    NameOfSpeaker = "Joaquin Lehner", // should match the person's opinion in the audio log
                               //     SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                               //     LineKey = "crashConclusion",
                                    DefaultText = "Let's get our stuff together and get back there quickly!" //  
                                
                        },

                        }
                }
                }

                }
            });




            list.Add(new PolledEventType()
            {
                KeyName = "DEMOISLANDMAP_introDialogue2PeopleFled",
                StartTimePoint = new TimePoint()
                {
                    RelativeNoOfDays = 0.0038
                },
                Condition = new CustomCondition()
                    {
                        PropertyCondition = new PropertyCondition()
                        {
                            PropertyKey = "fledAtStart",
                            BoolValue = true
                        }   
                    
                },

                ActionSets = new ActionSets()
                {
                    SetsOfActions = new []{ new ActionSetType("6yufkhfjkhfjkfhjkfhjkhfjkfhjkh2")
                { 
                        Actions = new EventActionType[]
                        {
                            new TalkAction("5fhjjjjjjjjjkfhjjjjjjjjjjkhfjkfhjkfhjk7")
                        {                                 
                                
                                    TalkPriority = TalkAction.TalkActionPriority.High,
                                    CanTalkWhileFighting = false,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = false,
                                    TurnTowardsListeners = true,
                                    ActionByAgent = ActionByAgent.RandomInAllegiance,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                              //      LineKey = "crashQuestion",
                                    DefaultText = "That was a close escape. We found a bad place to crash-land, huh." //"It looks like everyone is OK. Let's see if anymore supplies survived the crash." //  "Again, sorry about the rough landing! I'm glad that you're all ok. Let's see if any supplies survived the crash."
                                
                        },
                        new TalkAction("cddfhjjjjjjjjjjjjjjkhfjkfhjkfhjkfhjkhfjkjf3")
                        {
                                DelayInSeconds = 4,
                                
                                    TalkPriority = TalkAction.TalkActionPriority.High,
                                    CanTalkWhileFighting = false,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = false,
                                    TurnTowardsListeners = true,
                                    ActionByAgent = ActionByAgent.RandomInAllegiance,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                              //      LineKey = "crashAnswer",
                                    DefaultText = "Yeah, those twinklers were suddenly everywhere." // It looks like everyone is OK. Let's see if anymore supplies survived the crash." //  "Again, sorry about the rough landing! I'm glad that you're all ok. Let's see if any supplies survived the crash."
                                
                        },


                      new EventActionDialog("5fhjjjjjjjjjjjjkyuyuyurryuyu1") { DelayInSeconds = 8, 
                        DisplayText = new DynamicText(){ Text = "AUDIO LOG: MEETING #1 \n \nLEHNER: So. What do we do now? \n \nCONLAN: We have life signs from Laurent's medic unit. He's alive but sadly he's unconscious. Condition is stable though. \n \nLEHNER: His location? \n \nCONLAN: Right where he fell out, when we slammed into that cliff. But he seems to be below ground. \n \nLEHNER: Poor guy fell down one of those crevices. \n \nCONLAN: So, what's the decision. Ready to go back and find him? We can handle those couple of twinklers. \n \nLEHNER: Might be better to take a measured approach. We don't have enough coil guns. There's a reason why we retreated in the first place. \n \nCONLAN: Then I suggest sharpening some cane to use as weapons. And we're gonna need some rope to haul him out of that hole. Let's get going! We might lose our friend while we stand here talking." ,  //  
                        },
                        DisplayImage = "GroupMeeting",
                                              
                                },

                        new TalkAction("fdhhjgkliiiiiiiiiiiiiiiiiiiiiiiiuikuiyfhkfkfk8")
                        {
                                DelayInSeconds = 9,
                                
                                    TalkPriority = TalkAction.TalkActionPriority.High,
                                    CanTalkWhileFighting = false,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = false,
                                    TurnTowardsListeners = true,
                                    ActionByAgent = ActionByAgent.OnlySpecific,
                                    NameOfSpeaker = "Ward Conlan", // should match the person's opinion in the audio log
                               //     SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                               //     LineKey = "crashConclusion",
                                    DefaultText = "Let's get our stuff together and get back there quickly!" //  
                                
                        },

                        }
                }
                }

                }
            });




            list.Add(new PolledEventType()
            {
                KeyName = "DEMOISLANDMAP_dialogueRescueCountdown", // written for one or more members
                StartTimePoint = new TimePoint()
                {
                    RelativeNoOfDays = 0.3//12800 = 8 in-game days. =3.5 real hours. 1600 seconds per day...
                                    //compare with   KeyName = "DEMOISLANDMAP_introDialogueSomeSupplies" fires at  RelativeTime = 0.02. this means it fires at in-game clock=32
                },
                Condition = new ConditionFunction()
                    {
                        Operator = OperatorType.And,
                        Left = new PlayerAllegiancePersons()
                        {
                            MinMembers = 1 //written so it works for one member or more.
                                                                                              
                        },
                        Right = new CustomCondition()
                        {
                            PropertyCondition = new PropertyCondition()
                            {
                                PropertyKey = "unconsciousAndAlive", //this value gets set to false immediately at the time of rescue.
                                BoolValue = true
                            }                                                                    
                        }
                },
                ActionSets = new ActionSets()
                {
                    SetsOfActions = new []{ new ActionSetType("635677777777777777tyutyuetuet2")
                { 
                        Actions = new EventActionType[]
                        {
                            new TalkAction("5etttttttttttttttughjgfhjgfjfgjh5677")
                        {                                 
                                
                                    TalkPriority = TalkAction.TalkActionPriority.High,
                                    CanTalkWhileFighting = false,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = true,
                                    TurnTowardsListeners = true,
                                    ActionByAgent = ActionByAgent.RandomInAllegiance,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                              //      LineKey = "crashQuestion",
                                    DefaultText = "Oh-oh...Laurent needs medical attention. Not much time now..." //
                                
                        },
             


                      new EventActionDialog("53eeeeeeeeeeeeeeeeetyuyeueeteyu5651") 
                      { 
                          DelayInSeconds = 3, 
                        
                          DisplayText = new DynamicText()
                          { 
                              Text = "//MEDIC UNIT - Sefu Laurent// \n \nWARNING: Patient condition (brain injury) worsening. \nSTATUS: Intracranial pressure rising, blood flow falling. \nPREDICTED OUTCOME: Death within 12 hrs. \nRECOMMENDATION: Supply medic unit with more nanobots ASAP."   
                                        
                            }                      
                                
                      },              
                        new TalkAction("5dghjkhyjfkydfgghdggfhgfuilkjlkjlkjljljh5677")
                        {    DelayInSeconds = 5,                             
                                
                                    TalkPriority = TalkAction.TalkActionPriority.High,
                                    CanTalkWhileFighting = false,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = true,
                                    TurnTowardsListeners = true,
                                    ActionByAgent = ActionByAgent.RandomInAllegiance,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                              //      LineKey = "crashQuestion",
                                    DefaultText = "Hang in there, Laurent!" //
                                
                        },

                        }
                }
                }

                }
            });






            #region Unconscious dies

            list.Add(new PolledEventType()
            {
                KeyName = "DEMOISLANDMAP_unconsciousDies",
                 StartTimePoint = new TimePoint()
                 { 
                     RelativeNoOfDays = 0.6, //12800 = 8 in-game days. =3.5 real hours. 1600 seconds per day...
                                    //compare with   KeyName = "DEMOISLANDMAP_introDialogueSomeSupplies" fires at  RelativeTime = 0.02. this means it fires at in-game clock=32
                 },
                Condition = new ConditionFunction()
                {
                    Operator = OperatorType.And,
                    Left = new PlayerAllegiancePersons()
                            {
                                MinMembers = 1 //written so it works for one member or more.

                            }                                    
                    ,
                    Right = new CustomCondition()
                    {
                        PropertyCondition = new PropertyCondition()
                        {
                            PropertyKey = "unconsciousAndAlive",
                            BoolValue = true
                        }                                                              
                    }       
                }
                ,
                ActionSets = new ActionSets()
                {
                    SetsOfActions = new []{ new ActionSetType("cetttttttttyurtyurfsthgfshfsghfd546642")
                    {                       
                        Actions = new EventActionType[]{
 
         
                            new TalkAction("4dggggggghjfhjdtyjutydui5e76765e7ye5e3c")
                        {                                 
                                
                                    TalkPriority = TalkAction.TalkActionPriority.High,
                                    CanTalkWhileFighting = false,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = true,
                                    TurnTowardsListeners = true,
                                    ActionByAgent = ActionByAgent.RandomInAllegiance,
                            //        SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                                    DefaultText = "Oh no... it's too late. Laurent is gone." 
                                
                        },

                    new EventActionDialog("bdgfjhdhcghjbcvnmbcvncvbncvnbvc45646646446a")
         {          DelayInSeconds = 4,
                                
                                    DisplayText = new DynamicText()
                                    { 
                                        Text = "JOURNAL \n#DATE \nLOCATION: 43 22.5N 124 17.7W \n#JOURNALNAMES  \n \nLaurent's life signs have stopped. No point in attempting a rescue now - he is dead. \nHopefully, some day his remains can be retrieved. \nGoodbye Sefu Laurent, rest in peace.",
                                        SubstitutionValues = new[]
                                        {
                                            new SubstituteValue() { Placeholder = "#JOURNALNAMES", PropertyName = "getJournalHeaderNames" },
                                            new SubstituteValue() { Placeholder = "#DATE", PropertyName = "getDate", Formatting = FormattingOptions.BothDates }
                                        }
                                    },
              //                          DisplayImage = "NightTime"
                                

                        },

                                            new SetPropertyAction("1sfgdgdgdgdgdgdgdgdgdgdgdgjhjdgjdgjdtdg6")
                                            {
                                                
                                                    PropertyKey = "unconsciousAndAlive", 
                                                    Value = new ValueNode() { Bool = false }
                                                
                                            },

                      new DestroyEntityAction("88tdgggggghjjiryuuiyuriryioiiuoyiyuyud60") 
                      { 
                          Comments = "EntityName is the one set in the spawn event in eventactionloader",
                          DelayInSeconds = 0, 
                      
                          EntityName = "Unconscious" 
                      }, 

                        new TalkAction("4rerererererererererereydrgfhdgfhdgfhdgfe3c")
                        {        DelayInSeconds = 5,                          
                                
                                    TalkPriority = TalkAction.TalkActionPriority.High,
                                    CanTalkWhileFighting = false,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = true,
                                    TurnTowardsListeners = true,
                                    ActionByAgent = ActionByAgent.RandomInAllegiance,
                            //        SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                                    DefaultText = "Dammit! I wish something could have been done." //
                                
                        },


                        }
                    }
                }
                }
            });


//
            list.Add(new PolledEventType()
            {
                KeyName = "DEMOISLANDMAP_checkCasualtyRescued",  // looks for a property that has been set some time after he was rescued. MP
                PollInterval = new ValueNode() { Decimal = 2f },  // seconds    
                StartTimePoint = new TimePoint() { RelativeTimeInSeconds = new ValueNode() { Decimal = 2f } },
                AllowRandomTimeOffset = true,
                Condition = new CustomCondition()
                    {
                        AllowWhileAllPlayerMembersAreSleepingOrCollapsed = false,
                        AllowWhilePlayerThreatened = false,  // player allegiance threatened
                        AllowWhilePlayerMemberIsFighting = false,
                        PropertyCondition = new PropertyCondition() { PropertyKey = "casualtyRescued", BoolValue = true }                        
                    
                },
                ActionSetsKey = "casualtyRescuedDebriefing"
            });




            #endregion

/////////////


            # endregion

 


            #region only one member left monologue
            // checks to see if only one left, then we need some solo ramblings:

            list.Add(new PolledEventType()
            {
                Comment = "looks for a property that has been set a little while after the guy became alone. Also requires at least one death to have occurred",
                KeyName = "DEMOISLANDMAP_onlyOneMemberLeftDialogue",  //  MP
                PollInterval = new ValueNode() { Decimal = 120f },  // seconds. was 60f
                StartTimePoint = new TimePoint() { RelativeTimeInSeconds = new ValueNode() { Decimal = 2f } },
                AllowRandomTimeOffset = true,
                Condition = new ConditionFunction()
                {
                    Left = new CustomCondition()
                    {
                        AllowWhileAllPlayerMembersAreSleepingOrCollapsed = false,
                        AllowWhilePlayerThreatened = false,  // player allegiance threatened
                        AllowWhilePlayerMemberIsFighting = false,

                        PropertyCondition = new PropertyCondition() { PropertyKey = "onlyOneMemberLeft", BoolValue = true }
                          
                    },
                    Operator = OperatorType.And,
                    Right = new ConditionFunction()
                    {
                        Left = new CustomCondition()
                        {
                            PropertyCondition = new PropertyCondition() { PropertyKey = "deathCounter", NumberMinimumInclusive = new ValueNode() { Int = 1 } }
                                    
                        },
                        Operator = OperatorType.And,
                        Right = new PlayerAllegiancePersons(){ MinMembers = 1, MaxMembers = 1 }        
                    }    
                },
                ActionSets = new ActionSets()
                {
                    FireMode = ActionSetsToFire.RandomValid,
                    ChanceToFire = 0.3f, //chooses from below
                    SetsOfActions = new []{ 


                         new ActionSetType("260a9cd8-5f8d-4d52-94bf-528845e21d5c")
                         {      
                //              ChanceToFire = 0.1f,
                              MaxFirings = 1,
                               Actions = new EventActionType[]{new TalkAction("59fd63d4-0ec2-4dc9-8900-59bb4c57ab3d") {  
                                
                                TalkPriority = TalkAction.TalkActionPriority.High,
                                CanTalkWhileFighting = false,
                                CanTalkWhileSleeping = false,
                                CanTalkWhileThreatened = false,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
             //                   ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                DefaultText = "Fellow colonists...when you find this journal:" }, 

                              new TalkAction("12ba0d96-1bd3-49b7-bbfe-239d17cefc9a") { DelayInSeconds = 4, 
                              
                              TalkPriority = TalkAction.TalkActionPriority.High,
                              TurnTowardsListeners = false,
                              CanTalkWhileFighting = false,
                              CanTalkWhileSleeping = false,
                              CanTalkWhileThreatened = false,
                              SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
            //                  ActionByAgent = ActionByAgent.RandomInAllegiance,
                              DefaultText = "On behalf of the PRECOL mission: I'm sorry we were wrong about the landing site..." },

                              new TalkAction("23298910-c8c4-4af9-8497-47cb6b97ffd7") { DelayInSeconds = 9, 
                              
                              TalkPriority = TalkAction.TalkActionPriority.High,
                              TurnTowardsListeners = false,
                              CanTalkWhileFighting = false,
                              CanTalkWhileSleeping = false,
                              CanTalkWhileThreatened = false,
                              SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
            //                  ActionByAgent = ActionByAgent.RandomInAllegiance,
                              DefaultText = "We failed you. I wish you good luck on the other continent." },                              
                               
                               }
                            },
                         new ActionSetType("d99d76f7-623c-4ebf-902b-021284544cb1")
                         {      
                //              ChanceToFire = 0.1f,
                              MaxFirings = 1,
                               Actions = new EventActionType[]{new TalkAction("88a2be61-f50c-4f62-9296-50e33e758d7d") {  
                                
                                TalkPriority = TalkAction.TalkActionPriority.High,
                                CanTalkWhileFighting = false,
                                CanTalkWhileSleeping = false,
                                CanTalkWhileThreatened = false,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
             //                   ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                DefaultText = "Colonists...You're probably disappointed with my PRECOL work." }, 

                              new TalkAction("6a1f8409-ac05-4b98-a086-123be23ee05c") { DelayInSeconds = 4, 
                              
                              TalkPriority = TalkAction.TalkActionPriority.High,
                              TurnTowardsListeners = false,
                              CanTalkWhileFighting = false,
                              CanTalkWhileSleeping = false,
                              CanTalkWhileThreatened = false,
                              SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
            //                  ActionByAgent = ActionByAgent.RandomInAllegiance,
                              DefaultText = "But if you find me before it's too late, I'll make it up to you." },                                  
                               
                               }
                            },
                        new ActionSetType("5eb0ae0e-eaa6-40d3-99f2-176f1d13cf1c")
                         {      
                //              ChanceToFire = 0.1f,
                              MaxFirings = 1,
                               Actions = new EventActionType[]{new TalkAction("af14fcc8-89e2-4d4e-b34c-380bba21bce2") {  
                                
                                TalkPriority = TalkAction.TalkActionPriority.High,
                                CanTalkWhileFighting = false,
                                CanTalkWhileSleeping = false,
                                CanTalkWhileThreatened = false,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
             //                   ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                DefaultText = "I belong in the top of the food chain..." }, 

                              new TalkAction("1fdcd469-a069-411a-8c00-147971d5ef31") { DelayInSeconds = 4, 
                              
                              TalkPriority = TalkAction.TalkActionPriority.High,
                              TurnTowardsListeners = false,
                              CanTalkWhileFighting = false,
                              CanTalkWhileSleeping = false,
                              CanTalkWhileThreatened = false,
                              SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
            //                  ActionByAgent = ActionByAgent.RandomInAllegiance,
                              DefaultText = "This is a complete misunderstanding." },

                              new TalkAction("21a9fe48-bb1a-4750-ae5b-6aedf3cfa340") { DelayInSeconds = 8, 
                              
                              TalkPriority = TalkAction.TalkActionPriority.High,
                              TurnTowardsListeners = false,
                              CanTalkWhileFighting = false,
                              CanTalkWhileSleeping = false,
                              CanTalkWhileThreatened = false,
                              SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
            //                  ActionByAgent = ActionByAgent.RandomInAllegiance,
                              DefaultText = "Ha." },                              
                               
                               }
                            },

                        new ActionSetType("47ae2ed7-2740-4e15-9d8a-a9e23fb8c7f6")
                         {      
                //              ChanceToFire = 0.1f,
                              MaxFirings = 1,
                               Actions = new EventActionType[]{new TalkAction("076c1fec-45ce-4f56-a47c-a26ea2d0b41b") {  
                                
                                TalkPriority = TalkAction.TalkActionPriority.High,
                                CanTalkWhileFighting = false,
                                CanTalkWhileSleeping = false,
                                CanTalkWhileThreatened = false,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
             //                   ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                DefaultText = "You harsh, unforgiving, beautiful planet. You win this round..." },                            
                               
                               }
                            },

                        new ActionSetType("344a8f91-0f66-4f33-affe-d3fd1db357ce")
                         {      
                //              ChanceToFire = 0.1f,
                              MaxFirings = 1,
                               Actions = new EventActionType[]{new TalkAction("0ee6cb52-eccc-46ad-b875-823bc9325d5e") {  
                                
                                TalkPriority = TalkAction.TalkActionPriority.High,
                                CanTalkWhileFighting = false,
                                CanTalkWhileSleeping = false,
                                CanTalkWhileThreatened = false,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
             //                   ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                DefaultText = "Planet Antheia..." }, 

                              new TalkAction("fa909cd9-e367-4e11-96d8-6bbb0847b98e") { DelayInSeconds = 3, 
                              
                              TalkPriority = TalkAction.TalkActionPriority.High,
                              TurnTowardsListeners = false,
                              CanTalkWhileFighting = false,
                              CanTalkWhileSleeping = false,
                              CanTalkWhileThreatened = false,
                              SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
            //                  ActionByAgent = ActionByAgent.RandomInAllegiance,
                              DefaultText = "Thank you for having me." },

                              new TalkAction("ffa45921-0915-4d15-907b-c7655c68d4e5") { DelayInSeconds = 5.5, 
                              
                              TalkPriority = TalkAction.TalkActionPriority.High,
                              TurnTowardsListeners = false,
                              CanTalkWhileFighting = false,
                              CanTalkWhileSleeping = false,
                              CanTalkWhileThreatened = false,
                              SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
            //                  ActionByAgent = ActionByAgent.RandomInAllegiance,
                              DefaultText = "But I get the feeling that I overstayed my welcome." },                              
                               
                               }
                            },


                         new ActionSetType("724d7908-3c57-4136-8a5a-7beca8402c2e")
                         {      
                //              ChanceToFire = 0.1f,
                              MaxFirings = 1,
                               Actions = new EventActionType[]{new TalkAction("27f8c499-5c7e-46ce-ae87-48e952eb7e0b") {  
                                
                                TalkPriority = TalkAction.TalkActionPriority.High,
                                CanTalkWhileFighting = false,
                                CanTalkWhileSleeping = false,
                                CanTalkWhileThreatened = false,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
             //                   ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                DefaultText = "I'm starting to doubt whether there's a niche for me in this ecosystem." },                         
                               
                               }
                            },


                        new ActionSetType("fa7d3d7d-ed41-4950-8df8-29a422b3d5bd")
                         {      
                //              ChanceToFire = 0.1f,
                              MaxFirings = 1,
                               Actions = new EventActionType[]{new TalkAction("e321b6d2-f629-4dd0-a807-ee986ff34a2a") {  
                                
                                TalkPriority = TalkAction.TalkActionPriority.High,
                                CanTalkWhileFighting = false,
                                CanTalkWhileSleeping = false,
                                CanTalkWhileThreatened = false,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
             //                   ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                DefaultText = "Why did we come to this planet. Nobody invited us." }, 

                              new TalkAction("d8411ff0-14b0-4c98-b3c8-16f0c1a6304f") { DelayInSeconds = 4, 
                              
                              TalkPriority = TalkAction.TalkActionPriority.High,
                              TurnTowardsListeners = false,
                              CanTalkWhileFighting = false,
                              CanTalkWhileSleeping = false,
                              CanTalkWhileThreatened = false,
                              SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
            //                  ActionByAgent = ActionByAgent.RandomInAllegiance,
                              DefaultText = "Because we weren't welcome, that's why..." },
                            }
                        }

                    }
                }
            });

            #endregion

            //

            #region two members left dialogue

            list.Add(new PolledEventType()
            {
                Comment = "looks for a property that has been set a little while after the two guys became alone. Also requires at least one death.",
                KeyName = "DEMOISLANDMAP_onlyTwoMembersLeftDialogue",
                PollInterval = new ValueNode() { Decimal = 180f },  // seconds . was 60f. MP increased this, so that it doesn't fire right after a death
                StartTimePoint = new TimePoint() { RelativeTimeInSeconds = new ValueNode() { Decimal = 2f } },
                AllowRandomTimeOffset = true,
                Condition = new ConditionFunction()
                {
                    Left = new CustomCondition()
                    {
                        AllowWhileAllPlayerMembersAreSleepingOrCollapsed = false,
                        AllowWhilePlayerThreatened = false,
                        AllowWhilePlayerMemberIsFighting = false,
                        PropertyCondition = new PropertyCondition() { PropertyKey = "onlyTwoMembersLeft", BoolValue = true }
                          
                    },
                    Operator = OperatorType.And,
                    Right = new ConditionFunction()
                    {
                        Left = new CustomCondition()
                        {
                            PropertyCondition = new PropertyCondition() { PropertyKey = "deathCounter", NumberMinimumInclusive = new ValueNode() { Int = 1 } }
                                    
                        },
                        Operator = OperatorType.And,
                        Right = new PlayerAllegiancePersons(){ MinMembers = 2, MaxMembers = 2 }  
                    }
                },
                ActionSets = new ActionSets()
                {
                    FireMode = ActionSetsToFire.RandomValid,
                    ChanceToFire = 0.3f, //chooses from below
                    SetsOfActions = new []
                    { 
                         new ActionSetType("fe180a4b-ce78-4404-9f99-906d8e632274")
                         {      
                //              ChanceToFire = 0.1f,
                              MaxFirings = 1,                              
                               Actions = new EventActionType[]{new TalkAction("6b66364d-319d-4ad9-86a3-fe0b66da5435") {  
                                
                                TalkPriority = TalkAction.TalkActionPriority.High,
                                CanTalkWhileFighting = false,
                                CanTalkWhileSleeping = false,
                                CanTalkWhileThreatened = false,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
             //                   ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                DefaultText = "We're not many left." }, 

                              new TalkAction("a9a421e4-dd65-4c6e-bd5e-9387d05cbb60") { DelayInSeconds = 3.5, 
                              
                              TalkPriority = TalkAction.TalkActionPriority.High,
                              TurnTowardsListeners = false,
                              CanTalkWhileFighting = false,
                              CanTalkWhileSleeping = false,
                              CanTalkWhileThreatened = false,
                              SpeakerDenomination = TalkAction.SpeakerInConversation.Second,  
            //                  ActionByAgent = ActionByAgent.RandomInAllegiance,
                              DefaultText = "No. This planet has not been nice to us." },

                              new TalkAction("2c116070-398e-438f-b483-b4514743623d") { DelayInSeconds = 8, 
                              
                              TalkPriority = TalkAction.TalkActionPriority.High,
                              TurnTowardsListeners = false,
                              CanTalkWhileFighting = false,
                              CanTalkWhileSleeping = false,
                              CanTalkWhileThreatened = false,
                              SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
            //                  ActionByAgent = ActionByAgent.RandomInAllegiance,
                              DefaultText = "You can say that again." },                              
                               
                               }
                            },
                         new ActionSetType("c40c1d75-8a3e-4c70-add1-d5b807471cb7")
                         {      
                //              ChanceToFire = 0.1f,
                              MaxFirings = 1,
                               Actions = new EventActionType[]{new TalkAction("77295e4f-8939-43fa-9504-280b3b132a84") {  
                                
                                TalkPriority = TalkAction.TalkActionPriority.High,
                                CanTalkWhileFighting = false,
                                CanTalkWhileSleeping = false,
                                CanTalkWhileThreatened = false,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
             //                   ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                DefaultText = "What are your plans if we get out of this?"}, 

                              new TalkAction("ae9139ba-1539-48e3-bfa6-831c82344cbf") { DelayInSeconds = 4, 
                              
                              TalkPriority = TalkAction.TalkActionPriority.High,
                              TurnTowardsListeners = false,
                              CanTalkWhileFighting = false,
                              CanTalkWhileSleeping = false,
                              CanTalkWhileThreatened = false,
                              SpeakerDenomination = TalkAction.SpeakerInConversation.Second,  
            //                  ActionByAgent = ActionByAgent.RandomInAllegiance,
                              DefaultText = "If we're leaving this biome, I actually hope I can come back some day, do some research." }, 
                                 
                              new TalkAction("62203799-8d7f-47bd-84ec-7c17cd9faf4f") { DelayInSeconds = 9, 
                              
                              TalkPriority = TalkAction.TalkActionPriority.High,
                              TurnTowardsListeners = false,
                              CanTalkWhileFighting = false,
                              CanTalkWhileSleeping = false,
                              CanTalkWhileThreatened = false,
                              SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
            //                  ActionByAgent = ActionByAgent.RandomInAllegiance,
                              DefaultText = "You're kidding?" }, 
        
                              new TalkAction("5ce375a0-425c-4bfa-997f-751b42a37c19") { DelayInSeconds = 12, 
                              
                              TalkPriority = TalkAction.TalkActionPriority.High,
                              TurnTowardsListeners = false,
                              CanTalkWhileFighting = false,
                              CanTalkWhileSleeping = false,
                              CanTalkWhileThreatened = false,
                              SpeakerDenomination = TalkAction.SpeakerInConversation.Second,  
            //                  ActionByAgent = ActionByAgent.RandomInAllegiance,
                              DefaultText = "This area has some of the most interesting phenomena I've seen." },                            

                               }
                            },
                        new ActionSetType("af80e299-5399-471a-9b0e-13cb79f26ae3")
                         {      
                //              ChanceToFire = 0.1f,
                              MaxFirings = 1,
                               Actions = new EventActionType[]{new TalkAction("e6777e7c-30cc-40d9-927b-f861e0010abb") {  
                                
                                TalkPriority = TalkAction.TalkActionPriority.High,
                                CanTalkWhileFighting = false,
                                CanTalkWhileSleeping = false,
                                CanTalkWhileThreatened = false,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
             //                   ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                DefaultText = "I'm kind of looking forward to seeing other faces." }, 

                              new TalkAction("2d8d7c8e-29c9-4b2f-8e55-67da892aaa37") { DelayInSeconds = 4, 
                              
                              TalkPriority = TalkAction.TalkActionPriority.High,
                              TurnTowardsListeners = false,
                              CanTalkWhileFighting = false,
                              CanTalkWhileSleeping = false,
                              CanTalkWhileThreatened = false,
                              SpeakerDenomination = TalkAction.SpeakerInConversation.Second,  
            //                  ActionByAgent = ActionByAgent.RandomInAllegiance,
                              DefaultText = "Yeah, me too. I'm gonna be glad to see my family again." },

                              new TalkAction("d235b833-92be-454d-afc4-b82a68ff615f") { DelayInSeconds = 8, 
                              
                              TalkPriority = TalkAction.TalkActionPriority.High,
                              TurnTowardsListeners = false,
                              CanTalkWhileFighting = false,
                              CanTalkWhileSleeping = false,
                              CanTalkWhileThreatened = false,
                              SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
            //                  ActionByAgent = ActionByAgent.RandomInAllegiance,
                              DefaultText = "Your family's among the colonists?" },                              
 
                              new TalkAction("78d3d04d-d67d-45de-b5c4-f5ae8c77328c") { DelayInSeconds = 12, 
                              
                              TalkPriority = TalkAction.TalkActionPriority.High,
                              TurnTowardsListeners = false,
                              CanTalkWhileFighting = false,
                              CanTalkWhileSleeping = false,
                              CanTalkWhileThreatened = false,
                              SpeakerDenomination = TalkAction.SpeakerInConversation.Second,  
            //                  ActionByAgent = ActionByAgent.RandomInAllegiance,
                              DefaultText = "Yep. But I haven't seen them since we boarded the ship...a century ago!" },                                

                               }
                            },


                        new ActionSetType("98015946-5a20-4343-88c9-84f3ee4a1c3c")
                         {      
                //              ChanceToFire = 0.1f,
                              MaxFirings = 1,
                               Actions = new EventActionType[]{new TalkAction("d257dbf7-3c53-4b70-b650-031f0437817d") {  
                                
                                TalkPriority = TalkAction.TalkActionPriority.High,
                                CanTalkWhileFighting = false,
                                CanTalkWhileSleeping = false,
                                CanTalkWhileThreatened = false,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
             //                   ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                DefaultText = "You don't look too good." }, 

                              new TalkAction("e0018dfe-4061-4964-b50a-77b97d38f659") { DelayInSeconds = 3.5, 
                              
                              TalkPriority = TalkAction.TalkActionPriority.High,
                              TurnTowardsListeners = false,
                              CanTalkWhileFighting = false,
                              CanTalkWhileSleeping = false,
                              CanTalkWhileThreatened = false,
                              SpeakerDenomination = TalkAction.SpeakerInConversation.Second,  
            //                  ActionByAgent = ActionByAgent.RandomInAllegiance,
                              DefaultText = "I know. My medic unit is working overtime...battling some very interesting pathogens." },

                              new TalkAction("a0007280-612b-4839-a9a6-dcd0c4457936") { DelayInSeconds = 8, 
                              
                              TalkPriority = TalkAction.TalkActionPriority.High,
                              TurnTowardsListeners = false,
                              CanTalkWhileFighting = false,
                              CanTalkWhileSleeping = false,
                              CanTalkWhileThreatened = false,
                              SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
            //                  ActionByAgent = ActionByAgent.RandomInAllegiance,
                              DefaultText = "I'm rooting for you. Don't want to dig anymore graves." },                              

                              new TalkAction("c6abfd1b-6f35-4e32-bb2b-c612d904b8a5") { DelayInSeconds = 12, 
                              
                              TalkPriority = TalkAction.TalkActionPriority.High,
                              TurnTowardsListeners = false,
                              CanTalkWhileFighting = false,
                              CanTalkWhileSleeping = false,
                              CanTalkWhileThreatened = false,
                              SpeakerDenomination = TalkAction.SpeakerInConversation.Second,  
            //                  ActionByAgent = ActionByAgent.RandomInAllegiance,
                              DefaultText = "Don't worry, you won't have to." },                               
 
                               }
                            }
                     }
                }
            });
            #endregion

            #region three members left (after burial) talk

            list.Add(new PolledEventType()
            {
                Comment = "looks for a property that has been set a little while after the first out of four dies, so this is post burial talk.",
                KeyName = "DEMOISLANDMAP_onlyThreeMembersLeftDialogue", 
                PollInterval = new ValueNode() { Decimal = 10f },  // seconds
                StartTimePoint = new TimePoint() { RelativeTimeInSeconds = new ValueNode() { Decimal = 2f } },
                AllowRandomTimeOffset = true,
                Condition = new ConditionFunction()
                {
                    Left = new CustomCondition()
                    {
                        AllowWhileAllPlayerMembersAreSleepingOrCollapsed = false,
                        AllowWhilePlayerThreatened = false, 
                        AllowWhilePlayerMemberIsFighting = false,

                        PropertyCondition = new PropertyCondition() { PropertyKey = "onlyThreeMembersLeft", BoolValue = true }
                          
                    },
                    Operator = OperatorType.And,
                    Right = new CustomCondition()
                    {
                        PropertyCondition = new PropertyCondition() { PropertyKey = "deathCounter", NumberMinimumInclusive = new ValueNode() { Int = 1 } }
                    }
                },
                ActionSets = new ActionSets()
                {
                    FireMode = ActionSetsToFire.RandomValid,
                    ChanceToFire = 1f, //chooses from below                    
                    SetsOfActions = new []{ 


                         new ActionSetType("9f06fbfe-aad6-4d3c-a78a-dd20f1e92fac")
                         {      
                //              ChanceToFire = 0.1f,
                              MaxFirings = 1,
                              Condition = new PlayerAllegiancePersons(){ MinMembers = 3, MaxMembers = 3 }                               
                               , 
                               Actions = new EventActionType[]{new TalkAction("622caf4a-8bb2-4aa9-b58c-ecf5d442aacd") {  
                                
                                TalkPriority = TalkAction.TalkActionPriority.High,
                                CanTalkWhileFighting = false,
                                CanTalkWhileSleeping = false,
                                CanTalkWhileThreatened = false,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
             //                   ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                DefaultText = "We need to stick together now." , // ALT: From now on, we have to be extra careful.
                                LineKey = "onlyThreeMembersLeftComment"}, 
                                
                                

                              new TalkAction("5126711b-81c1-4f08-b8ec-ab75bbd5d1e3") { DelayInSeconds = 4, 
                              
                              TalkPriority = TalkAction.TalkActionPriority.High,
                              TurnTowardsListeners = false,
                              CanTalkWhileFighting = false,
                              CanTalkWhileSleeping = false,
                              CanTalkWhileThreatened = false,
                              SpeakerDenomination = TalkAction.SpeakerInConversation.Second,  
            //                  ActionByAgent = ActionByAgent.RandomInAllegiance,
                              DefaultText = "Agreed. This must not happen again.", // ALT: Yes. We can make it, but only if we don't take unnecessary chances.
                              LineKey = "onlyThreeMembersLeftAnswer"},                                
                               
                               }
                            },
                    
 
                            }
                }
            });

            #endregion

            #region rescue and precol dialogue

            list.Add(new PolledEventType()
            {
                KeyName = "DEMOISLANDMAP_rescueAndPRECOLDialogue",
                StartTimePoint = new TimePoint()
                {
                    Date = new DateAndTime.TimeDateYear() { Year = 0, Day = 1, TimeOfDay = 0.90 }   // TimeOfDayToGoToSleep 0.95..                   
                },              
                ActionSets = new ActionSets()
                {
                    FireMode = ActionSetsToFire.RandomValid,
                    ChanceToFire = 1f, //chooses from below
                    SetsOfActions = new []{ 


                         new ActionSetType("3e830f75-60db-46da-88ed-eb84375af8cd")
                         {      
                //              ChanceToFire = 0.1f,
                              MaxFirings = 1,
                              Condition = new PlayerAllegiancePersons(){ MinMembers = 3 }                               
                               , 
                               Actions = new EventActionType[]{new TalkAction("913c3809-d8ea-4dbb-a15c-1cdbc54ca616") {  
                                
                                TalkPriority = TalkAction.TalkActionPriority.High,
                                CanTalkWhileFighting = false,
                                CanTalkWhileSleeping = false,
                                CanTalkWhileThreatened = false,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
             //                   ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                DefaultText = "When do you think we'll get rescued?" }, 

                              new TalkAction("653f3ba5-add8-4d0a-8cfe-2533bb364cba") { DelayInSeconds = 4, 
                              
                              TalkPriority = TalkAction.TalkActionPriority.High,
                              TurnTowardsListeners = false,
                              CanTalkWhileFighting = false,
                              CanTalkWhileSleeping = false,
                              CanTalkWhileThreatened = false,
                              SpeakerDenomination = TalkAction.SpeakerInConversation.Second,  
            //                  ActionByAgent = ActionByAgent.RandomInAllegiance,
                              DefaultText = "Weeks, months...it depends on multiple factors such as their priorities and the resources available to them." },

                              new TalkAction("b7f64d96-023e-4503-8742-a661f9d2955d") { DelayInSeconds = 9, 
                              
                              TalkPriority = TalkAction.TalkActionPriority.High,
                              TurnTowardsListeners = false,
                              CanTalkWhileFighting = false,
                              CanTalkWhileSleeping = false,
                              CanTalkWhileThreatened = false,
                              SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
            //                  ActionByAgent = ActionByAgent.RandomInAllegiance,
                              DefaultText = "I hope we're high on their to-do list." },                              
                               
                               }
                            },
                         new ActionSetType("f5288de5-bec9-4bff-8ce9-1e53a2da8e06")
                         {      
                //              ChanceToFire = 0.1f,
                              MaxFirings = 1,
                              Condition = new PlayerAllegiancePersons(){ MinMembers = 3 }                               
                               , 
                               Actions = new EventActionType[]{new TalkAction("390f0191-39ac-4e6d-83d7-81ef21d103e0") {  
                                
                                TalkPriority = TalkAction.TalkActionPriority.High,
                                CanTalkWhileFighting = false,
                                CanTalkWhileSleeping = false,
                                CanTalkWhileThreatened = false,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
             //                   ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                DefaultText = "I can't help but thinking they are not even searching for us." }, 

                              new TalkAction("45269c89-7369-4db9-a98c-93901626a5f0") { DelayInSeconds = 4, 
                              
                              TalkPriority = TalkAction.TalkActionPriority.High,
                              TurnTowardsListeners = false,
                              CanTalkWhileFighting = false,
                              CanTalkWhileSleeping = false,
                              CanTalkWhileThreatened = false,
                              SpeakerDenomination = TalkAction.SpeakerInConversation.Second,  
            //                  ActionByAgent = ActionByAgent.RandomInAllegiance,
                              DefaultText = "Because they think we're dead?" },

                              new TalkAction("efe68486-c947-4932-80e6-f61c031f8493") { DelayInSeconds = 7, 
                              
                              TalkPriority = TalkAction.TalkActionPriority.High,
                              TurnTowardsListeners = false,
                              CanTalkWhileFighting = false,
                              CanTalkWhileSleeping = false,
                              CanTalkWhileThreatened = false,
                              SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
            //                  ActionByAgent = ActionByAgent.RandomInAllegiance,
                              DefaultText = "No - more like they don't care. Maybe they're so disappointed in our failed PRECOL mission..." }, 
                             
                             new TalkAction("c904de34-ca85-4bd7-b2b8-5ca959be9056") { DelayInSeconds = 12, 
                              
                              TalkPriority = TalkAction.TalkActionPriority.High,
                              TurnTowardsListeners = false,
                              CanTalkWhileFighting = false,
                              CanTalkWhileSleeping = false,
                              CanTalkWhileThreatened = false,
                              SpeakerDenomination = TalkAction.SpeakerInConversation.Third,  
            //                  ActionByAgent = ActionByAgent.RandomInAllegiance,
                              DefaultText = "That they'll just leave us to die? Come on." },                              
 
                               }
                            },
                         new ActionSetType("ccca73d7-9cc1-4997-9ad7-a32f86f0101f")
                         {      
                //              ChanceToFire = 0.1f,
                              MaxFirings = 1,
                              Condition = new PlayerAllegiancePersons(){ MinMembers = 4 }                               
                               , 
                               Actions = new EventActionType[]{new TalkAction("6f2b62b0-e6ba-4d0d-9782-4e48524c55f4") {  
                                
                                TalkPriority = TalkAction.TalkActionPriority.High,
                                CanTalkWhileFighting = false,
                                CanTalkWhileSleeping = false,
                                CanTalkWhileThreatened = false,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
             //                   ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                DefaultText = "So, how long do you think we need to camp out here before they find us?" }, 

                              new TalkAction("1ce500d8-3fbe-41da-b672-b3092042c6ad") { DelayInSeconds = 5, 
                              
                              TalkPriority = TalkAction.TalkActionPriority.High,
                              TurnTowardsListeners = false,
                              CanTalkWhileFighting = false,
                              CanTalkWhileSleeping = false,
                              CanTalkWhileThreatened = false,
                              SpeakerDenomination = TalkAction.SpeakerInConversation.Second,  
            //                  ActionByAgent = ActionByAgent.RandomInAllegiance,
                              DefaultText = "It could take a while. I'd say a couple of weeks, at least." },

                              new TalkAction("358577e5-f2c7-4c0c-a998-2b6980efc7be") { DelayInSeconds = 9, 
                              
                              TalkPriority = TalkAction.TalkActionPriority.High,
                              TurnTowardsListeners = false,
                              CanTalkWhileFighting = false,
                              CanTalkWhileSleeping = false,
                              CanTalkWhileThreatened = false,
                              SpeakerDenomination = TalkAction.SpeakerInConversation.Third,  
            //                  ActionByAgent = ActionByAgent.RandomInAllegiance,
                              DefaultText = "That's optimistic. They are setting up the main colony on the other side of the planet you know. Consider the amount of work they are facing..." }, 
                             
                             new TalkAction("f09937d8-f200-42a6-9472-e16f4be651f4") { DelayInSeconds = 13, 
                              
                              TalkPriority = TalkAction.TalkActionPriority.High,
                              TurnTowardsListeners = false,
                              CanTalkWhileFighting = false,
                              CanTalkWhileSleeping = false,
                              CanTalkWhileThreatened = false,
                              SpeakerDenomination = TalkAction.SpeakerInConversation.Third,  
            //                  ActionByAgent = ActionByAgent.RandomInAllegiance,
                              DefaultText = "Don't expect to see rescue teams within several months." },                              
 
                               }
                            },


                         new ActionSetType("88db306a-4396-4113-9f0c-dba2d1eefb37")
                         {      
                //              ChanceToFire = 0.1f,
                              MaxFirings = 1,
                              Condition = new PlayerAllegiancePersons(){ MinMembers = 3 }                               
                               , 
                               Actions = new EventActionType[]{new TalkAction("c892231f-3ddd-4b08-bd0d-9dc98404ff8d") {  
                                
                                TalkPriority = TalkAction.TalkActionPriority.High,
                                CanTalkWhileFighting = false,
                                CanTalkWhileSleeping = false,
                                CanTalkWhileThreatened = false,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
             //                   ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                DefaultText = "Some mess we caused in the PRECOL mission..." }, 

                              new TalkAction("8b1f04f3-1083-4f24-9a89-b5ffd7b53172") { DelayInSeconds = 3.5, 
                              
                              TalkPriority = TalkAction.TalkActionPriority.High,
                              TurnTowardsListeners = false,
                              CanTalkWhileFighting = false,
                              CanTalkWhileSleeping = false,
                              CanTalkWhileThreatened = false,
                              SpeakerDenomination = TalkAction.SpeakerInConversation.Second,  
            //                  ActionByAgent = ActionByAgent.RandomInAllegiance,
                              DefaultText = "Don't dwell on it. We'll just have to work hard and make it up to them." },

                              new TalkAction("965d976f-47e0-4584-93fe-f077b993e9d0") { DelayInSeconds = 7, 
                              
                              TalkPriority = TalkAction.TalkActionPriority.High,
                              TurnTowardsListeners = false,
                              CanTalkWhileFighting = false,
                              CanTalkWhileSleeping = false,
                              CanTalkWhileThreatened = false,
                              SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
            //                  ActionByAgent = ActionByAgent.RandomInAllegiance,
                              DefaultText = "If we get a second chance, yeah. But they have to rescue us first. Who says they will even bother?" },                              
 
                              new TalkAction("5f4e3c23-b8b8-4948-a88b-b259befce5be") { DelayInSeconds = 11, 
                              
                              TalkPriority = TalkAction.TalkActionPriority.High,
                              TurnTowardsListeners = false,
                              CanTalkWhileFighting = false,
                              CanTalkWhileSleeping = false,
                              CanTalkWhileThreatened = false,
                              SpeakerDenomination = TalkAction.SpeakerInConversation.Second,  
            //                  ActionByAgent = ActionByAgent.RandomInAllegiance,
                              DefaultText = "Of course they will. They'll turn up soon enough." },
                               }
                            },

                        new ActionSetType("9573f2a1-f4f0-46a1-a1d0-4a3a0765a413")
                         {      
                //              ChanceToFire = 0.1f,
                              MaxFirings = 1,
                              Condition = new PlayerAllegiancePersons(){ MinMembers = 3 }                               
                               , 
                               Actions = new EventActionType[]{new TalkAction("99e335a1-c54f-435f-9b9a-d447672d36c2") {  
                                
                                TalkPriority = TalkAction.TalkActionPriority.High,
                                CanTalkWhileFighting = false,
                                CanTalkWhileSleeping = false,
                                CanTalkWhileThreatened = false,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
             //                   ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                DefaultText = "We botched the PRECOL mission really badly..." }, 

                              new TalkAction("8c7b9f7a-2e8b-4229-b3d4-2655ebe1b8dd") { DelayInSeconds = 3.5, 
                              
                              TalkPriority = TalkAction.TalkActionPriority.High,
                              TurnTowardsListeners = false,
                              CanTalkWhileFighting = false,
                              CanTalkWhileSleeping = false,
                              CanTalkWhileThreatened = false,
                              SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
            //                  ActionByAgent = ActionByAgent.RandomInAllegiance,
                              DefaultText = "...for all our supposed expertise." },

                              new TalkAction("e77d3db8-f7d1-48bb-b99f-8fc9e29b14c4") { DelayInSeconds = 7, 
                              
                              TalkPriority = TalkAction.TalkActionPriority.High,
                              TurnTowardsListeners = false,
                              CanTalkWhileFighting = false,
                              CanTalkWhileSleeping = false,
                              CanTalkWhileThreatened = false,
                              SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
            //                  ActionByAgent = ActionByAgent.RandomInAllegiance,
                              DefaultText = "It makes it hard for me to trust my own judgment." },                              
 
                              new TalkAction("d00094d5-d362-4e27-bec3-a93f930eaabe") { DelayInSeconds = 10, 
                              
                              TalkPriority = TalkAction.TalkActionPriority.High,
                              TurnTowardsListeners = false,
                              CanTalkWhileFighting = false,
                              CanTalkWhileSleeping = false,
                              CanTalkWhileThreatened = false,
                              SpeakerDenomination = TalkAction.SpeakerInConversation.Second,  
            //                  ActionByAgent = ActionByAgent.RandomInAllegiance,
                              DefaultText = "We just have to do better from now on." },
                               }
                            },


                        new ActionSetType("8523717b-f1c8-4e3a-8a98-01a958f5c570")
                         {      
                //              ChanceToFire = 0.1f,
                              MaxFirings = 1,
                              Condition = new PlayerAllegiancePersons(){ MinMembers = 4 }                               
                               , 
                               Actions = new EventActionType[]{new TalkAction("d8502d4e-f2ca-4bb7-8dcc-94d1b06dd3d2") {  
                                
                                TalkPriority = TalkAction.TalkActionPriority.High,
                                CanTalkWhileFighting = false,
                                CanTalkWhileSleeping = false,
                                CanTalkWhileThreatened = false,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
             //                   ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                DefaultText = "I can't believe so many PRECOL fellows are gone. Eaten up. Just like that." }, 

                              new TalkAction("82cae823-0ea5-4a39-8522-2011cb9aea87") { DelayInSeconds = 4, 
                              
                              TalkPriority = TalkAction.TalkActionPriority.High,
                              TurnTowardsListeners = false,
                              CanTalkWhileFighting = false,
                              CanTalkWhileSleeping = false,
                              CanTalkWhileThreatened = false,
                              SpeakerDenomination = TalkAction.SpeakerInConversation.Second,  
            //                  ActionByAgent = ActionByAgent.RandomInAllegiance,
                              DefaultText = "We have to look ahead now." },                           
 
                              new TalkAction("d25aa7d8-8872-41b0-a9f8-716888eb29c1") { DelayInSeconds = 8, 
                              
                              TalkPriority = TalkAction.TalkActionPriority.High,
                              TurnTowardsListeners = false,
                              CanTalkWhileFighting = false,
                              CanTalkWhileSleeping = false,
                              CanTalkWhileThreatened = false,
                              SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
            //                  ActionByAgent = ActionByAgent.RandomInAllegiance,
                              DefaultText = "You're right. Focus on the tasks at hand." }


                               }
                            }

                            }
                }
            });

            #endregion





            #endregion


            #region checks for nest and sulfur exposition

            list.Add(new PolledEventType()
            {
                KeyName = "DEMOISLANDMAP_checkNestExpositionEventHasFiredAndSulfurSeen",
                PollInterval = new ValueNode() { Decimal = 4f },  // seconds
                StartTimePoint = new TimePoint() { RelativeTimeInSeconds = new ValueNode() { Decimal = 2f } },
                AllowRandomTimeOffset = true,
                Condition = new ConditionFunction()
                {
                    Left = new CustomCondition()
                        {
                            AllowWhileAllPlayerMembersAreSleepingOrCollapsed = false,
                            AllowWhilePlayerThreatened = false,  // player allegiance threatened
                            AllowWhilePlayerMemberIsFighting = false,
                                
                            PropertyCondition = new PropertyCondition() { PropertyKey = "sulfurDetected", BoolValue = true }
                               
                        }
                    ,
                    Operator = OperatorType.And, //And,
                    Right = new CustomCondition()
                    {
                        PropertyCondition = new PropertyCondition() { PropertyKey = "nestExpositionEventHasFired", BoolValue = true }
                    }
                },
                ActionSetsKey = "talkSulfurAndNest"

            });

            // when a nest has been found and the nest exposition has fired, but sulfur has not been found, and no nest has been destroyed:
            list.Add(new PolledEventType()
            {
                KeyName = "DEMOISLANDMAP_checkNestExpositionEventHasFired",
                PollInterval = new ValueNode() { Decimal = 8f },  // seconds
                AllowRandomTimeOffset = true,
                Condition = new ConditionFunction()
                {
                    Left = new CustomCondition()
                    {
                        AllowWhileAllPlayerMembersAreSleepingOrCollapsed = false,
                        AllowWhilePlayerThreatened = false,  // player allegiance threatened
                        AllowWhilePlayerMemberIsFighting = false,
                        PropertyCondition = new PropertyCondition() { PropertyKey = "sulfurDetected", BoolValue = false }                                
                    },
                    Operator = OperatorType.And, //And,
                    Right = new ConditionFunction()
                    {
                        Left = new CustomCondition()
                        {

                            PropertyCondition = new PropertyCondition() { PropertyKey = "nestExpositionEventHasFired", BoolValue = true }  // "nestSeen"
                                    
                        },
                        Operator = OperatorType.And,
                        Right = new CustomCondition()
                        {
                            PropertyCondition = new PropertyCondition() { PropertyKey = "nestDestroyed", BoolValue = false }
                                   
                        }                            
                    }
                    
                }
                ,
                ActionSetsKey = "nestExpositionRecapEvent"

            });

            # endregion






            //do not migrate to eventaction.  if an event has several actions in it, such as multiple start spawn, then it needs to go through global.
            #region DEMOISLANDMAP Beginning population spawn


            #region start twinkler spawns on top of wreck

            list.Add(new PolledEventType()
            {
                KeyName = "DEMOISLANDMAP_beginningTwinklerPopulationSandstone", // twinklers that are on top of the wreck at sandstone.
               /* ConditionSet = new ConditionSet()
                {
                    Value = new TimeCondition()
                        {
                            RelativeNoOfDays = 0.0
                        }                    
                }
                ,*/
                ActionSets = new ActionSets()
                {
                    SetsOfActions = new []{ new ActionSetType("954dfsghhhhhhhhhhhhhhhgfshgfshgfshjshjdhj9cbe")
                    {                                  
                        Actions = new EventActionType[]{
                        new SpawnEntityAction("5536777787euryhjsw56he3e47u5wu64w7yu6ws52f") { DelayInSeconds = 0,
                        EntityData = new EntityData()
                            { EntityKey = "entity:twinkler",  Name = "twinkler13", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "twinklerAllegiance" },
                              Location = new Vector3(1014, 1344, 0), Bulk = 0.25f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult, CasteKey = "guard" } }, },


                        new SpawnEntityAction("650dtyu5e6753e7657eeeeeeeeeeeuydrtfsthsfgh8") { DelayInSeconds = 0,
                        EntityData = new EntityData()
                            { EntityKey = "entity:twinkler",  Name = "twinkler14", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "twinklerAllegiance" },
                              Location = new Vector3(1020, 1344, 0), Bulk = 0.25f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult, CasteKey = "guard" } }, },



                        new SpawnEntityAction("5e5656565656565656565656565656tegurturyuteud61") { DelayInSeconds = 0,
                        EntityData = new EntityData()
                            { EntityKey = "entity:twinkler",  Name = "twinkler15", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "twinklerAllegiance" },
                              Location = new Vector3(1050, 1392, 0), Bulk = 0.25f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult, CasteKey = "guard" } }, },




                        }
                    }
                    }
                }
            });


            list.Add(new PolledEventType()
            {
                KeyName = "DEMOISLANDMAP_beginningTwinklerPopulationBramble", // twinklers that are on top of the wreck at sandstone. // TODO: make this an action instead
             /*   ConditionSet = new ConditionSet()
                {
                    Value = new TimeCondition()
                        {
                            RelativeNoOfDays = 0.0
                        }                    
                }
                ,*/
                ActionSets = new ActionSets()
                {
                    SetsOfActions = new []{ new ActionSetType("95356y7twrsyhsrtyhassgrfhbe")
                    {                                  
                        Actions = new EventActionType[]{
                        new SpawnEntityAction("553shfjw56w537uyw65yu5yuw56syhw6s52f") { DelayInSeconds = 0,
                        EntityData = new EntityData()
                            { EntityKey = "entity:twinkler",  MemberOf = new AllegianceAndExpedition() { AllegianceKey = "twinklerAllegiance" },
                              Location = new Vector3(1968, 1690, 0), Bulk = 0.25f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult, CasteKey = "guard" } }, },


                        new SpawnEntityAction("650dtyuw65uyhwshrsghsghsghshsgshgshgshgfgh8") { DelayInSeconds = 0,
                        EntityData = new EntityData()
                            { EntityKey = "entity:twinkler",   MemberOf = new AllegianceAndExpedition() { AllegianceKey = "twinklerAllegiance" },
                              Location = new Vector3(1920, 1824, 0), Bulk = 0.25f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult, CasteKey = "guard" } }, },



                        new SpawnEntityAction("5ew56usfytujfgshsfghsfhghsgshghgsshhsgsud61") { DelayInSeconds = 0,
                        EntityData = new EntityData()
                            { EntityKey = "entity:twinkler",  MemberOf = new AllegianceAndExpedition() { AllegianceKey = "twinklerAllegiance" },
                              Location = new Vector3(1536, 1920, 0), Bulk = 0.25f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult, CasteKey = "guard" } }, },


                        }
                    }
                    }
                }
            });


            #endregion




            #region Bush Dragon Populations


            list.Add(new PolledEventType()
            {
                KeyName = "DEMOISLANDMAP_beginningBushDragonPopulationEast", // TODO: make this an action instead               
                ActionSets = new ActionSets()
                {
                    SetsOfActions = new []{ new ActionSetType("954sad326532d6cca-d56c-48a8-a148-647fc2e99cbe")
                    {     
                             
                        Actions = new EventActionType[]
                        {
                            new SpawnEntityAction("215329de-d1a6-4592-b9fc-2942149b61ed") { DelayInSeconds = 0,
                            EntityData = new EntityData()
                                { EntityKey = "entity:bushDragon", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "bushDragonAllegianceSouthEast" },  //jan 2015: uses an autogenerated expedition
                                  Location = new Vector3(2160, 2400, 0), Bulk = 0.98f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult } }, },


                            new SpawnEntityAction("7b3da8c1-023b-4e8a-948a-ce15a0a0b53d") { DelayInSeconds = 0,
                            EntityData = new EntityData()
                                { EntityKey = "entity:bushDragon", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "bushDragonAllegianceSouthEast" },  
                                  Location = new Vector3(2400, 2544, 0), Bulk = 1.2f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult } }, },


                                // on lagoon beach:
                            new SpawnEntityAction("66e99fd3-7f9e-4573-8bc8-dff76e4dbaa9") { DelayInSeconds = 0,
                            EntityData = new EntityData()
                                { EntityKey = "entity:bushDragon", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "bushDragonAllegianceLagoon" },  //jan 2015: uses an autogenerated expedition
                                  Location = new Vector3(1632, 2160, 0), Bulk = 0.9f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult } }, },
                        }
                    }
                    }
                }
            });



            list.Add(new PolledEventType()
            {
                KeyName = "DEMOISLANDMAP_beginningBushDragonPopulationWest",
                ActionSets = new ActionSets()
                {
                    SetsOfActions = new []{ new ActionSetType("95sfghfhjfsjdhdgtsjydtjdgfdhdgdgjdghjdgcbe")
                    {     
                             
                        Actions = new EventActionType[]{

                        new SpawnEntityAction("215ettttttttttttttttttttttttt5673567563735753d") { DelayInSeconds = 0,
                        EntityData = new EntityData()
                            { EntityKey = "entity:bushDragon", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "bushDragonAllegianceSouthWest" },  //jan 2015: uses an autogenerated expedition
                              Location = new Vector3(1056, 2544, 0), Bulk = 0.98f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult } }, },


                        new SpawnEntityAction("7356777777777777777eyuyuetuetu3d") { DelayInSeconds = 0,
                        EntityData = new EntityData()
                            { EntityKey = "entity:bushDragon", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "bushDragonAllegianceSouthWest" },  
                              Location = new Vector3(816, 2496, 0), Bulk = 1.2f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult } }, },


                            // on lagoon beach:
                        new SpawnEntityAction("66ettttttttttttttu567u7u7u7u7u7u7u7u5ue9") { DelayInSeconds = 0,
                        EntityData = new EntityData()
                            { EntityKey = "entity:bushDragon", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "bushDragonAllegianceLagoon" },  //jan 2015: uses an autogenerated expedition
                              Location = new Vector3(1632, 2160, 0), Bulk = 0.9f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult } }, },


                        }
                    }
                    }
                }
            });




            #endregion




            #region DEMOISLANDMAP_timedSpawnBeginningPopulationNormal // TODO: make this an action instead
            ////Normal and hard fauna: Beginning animal population (spawn). //Bushdragons are spawned separately because they are dependent on camp start location

            list.Add(new PolledEventType()
            {
                KeyName = "DEMOISLANDMAP_timedSpawnBeginningPopulationNormal",
               /* ConditionSet = new ConditionSet()
                {
                    Value = new TimeCondition()
                        {
                            RelativeNoOfDays = 0.0
                        }
                    
                }
                ,*/
                ActionSets = new ActionSets()
                {
                    SetsOfActions = new []{ new ActionSetType("954d6cca-d56c-48a8-aasf232148-647fc2e99cbe")
                    {     
                             
                        Actions = new EventActionType[]{

// Thunder chicken Start Spawns //
                        new SpawnEntityAction("38bbdasf3265970-96d3-49ec-9afc-5808e1e90714") { DelayInSeconds = 0,
                            EntityData = new EntityData()
                            { EntityKey = "entity:whiteThunderChicken", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "thunderChickenAllegianceSouth" }, 
                              Location = new Vector3(1546,2256, 0), Rotation = 115, Bulk = 0.25f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult } }, },
                        new SpawnEntityAction("3asf235235970-96d3-49ec-9afc-5808e1e90714") { DelayInSeconds = 0,
                            EntityData = new EntityData()
                            { EntityKey = "entity:whiteThunderChicken", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "thunderChickenAllegianceSouth" }, 
                              Location = new Vector3(1546,2256, 0), Rotation = 115, Bulk = 0.25f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult } },},
                        new SpawnEntityAction("3asf26262970-9ff6d3-49ec-9afc-5808e1e90714") { DelayInSeconds = 0,
                            EntityData = new EntityData()
                            { EntityKey = "entity:pygmyThunderChicken", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "thunderChickenAllegianceNorth" }, 
                              Location = new Vector3(826,1114, 0), Rotation = 115, Bulk = 0.25f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult } }, },
                        new SpawnEntityAction("382627327fsa70-96d3-49ec-9afc-5808e1e90714") { DelayInSeconds = 0,
                            EntityData = new EntityData()
                            { EntityKey = "entity:pygmyThunderChicken", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "thunderChickenAllegianceNorth" }, 
                              Location = new Vector3(826,1114, 0), Rotation = 115, Bulk = 0.25f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult } }, },

//birds
                        new SpawnEntityAction("e4c82703-0b38-4bfb-a54f-ae269e4a6bd4") { DelayInSeconds = 0,
                        EntityData = new EntityData()
                            { EntityKey = "entity:bird", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "Allegiance #43" },  
                              Location = new Vector3(448, 2719, 0), Rotation = 115, Bulk = 0.12f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult} }, },

                        new SpawnEntityAction("73e9aab1-12ab-491a-8d2c-d6f5116254d9") { DelayInSeconds = 1,
                        EntityData = new EntityData()
                            { EntityKey = "entity:bird", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "Allegiance #43" },
                              Location = new Vector3(489, 2734, 0), Rotation = 167, Bulk = 0.12f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult} }, },

                        new SpawnEntityAction("55fd2217-b262-4d37-acc8-9d55bf0b6927") { DelayInSeconds = 1.85,
                        EntityData = new EntityData()
                            { EntityKey = "entity:bird", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "Allegiance #43" },
                              Location = new Vector3(565, 2869, 0), Rotation = 208, Bulk = 0.12f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult} }, },

                        new SpawnEntityAction("a9d1d7d2-21d6-4985-bcc2-ac0d27bf2c8a") { DelayInSeconds = 2.6,
                        EntityData = new EntityData()
                            { EntityKey = "entity:bird", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "Allegiance #43" },
                              Location = new Vector3(580, 2839, 0), Rotation = 137, Bulk = 0.12f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult} }, },

                        new SpawnEntityAction("fa476edd-eb22-40df-8667-9103562d4879") { DelayInSeconds = 3.2,
                        EntityData = new EntityData()
                            { EntityKey = "entity:bird", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "Allegiance #43" },
                              Location = new Vector3(626, 2854, 0), Rotation = 316, Bulk = 0.12f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult} }, },

                        new SpawnEntityAction("f8252656-c471-4e33-87ba-ae53a5a4eb66") { DelayInSeconds = 2,
                        EntityData = new EntityData()
                            { EntityKey = "entity:bird", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "Allegiance #43" },
                              Location = new Vector3(659, 2908, 0), Rotation = 112, Bulk = 0.12f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult} }, },

//binal rats
                        new SpawnEntityAction("d9c660safa6-4296-48ed-a014-0e26362c29b836976") { DelayInSeconds = 9,
                            EntityData = new EntityData()
                            { EntityKey = "entity:binalRat", Name = "BinalRat11", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegianceSouthWest" },
                            Location = new Vector3(1008, 2304, 0), Bulk = 0.21f, BioEntity = new Maps.MapEditor.BiologicalEntity() //sw
                            { AgeGroup = AIAgeGroup.Adult} }, },

                        new SpawnEntityAction("ff90216c-af352535c-42df-92da-fc9fb3429f22") { DelayInSeconds = 0,
                            EntityData = new EntityData()
                            { EntityKey = "entity:binalRat", Name = "BinalRat6", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegianceSouthWest" },
                            Location = new Vector3(480, 2169, 0), Bulk = 0.21f, BioEntity = new Maps.MapEditor.BiologicalEntity()//sw
                            { AgeGroup = AIAgeGroup.Adult } }, },

                        new SpawnEntityAction("484e1d8d-af3252626-4d0e-a15e-499a609231eb") { DelayInSeconds = 8,
                            EntityData = new EntityData()
                            { EntityKey = "entity:binalRat", Name = "BinalRat5", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegianceCenterEast" },
                            Location = new Vector3(2256, 2256, 0), Bulk = 0.21f, BioEntity = new Maps.MapEditor.BiologicalEntity() //ce
                            { AgeGroup = AIAgeGroup.Adult } }, }, 

                        new SpawnEntityAction("b4fc6b42-c9saf32626-4de8-adef-ff442a207409") { DelayInSeconds = 2,
                             EntityData = new EntityData()
                             { EntityKey = "entity:binalRat", Name = "BinalRat7", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegianceCenterEast" },
                             Location = new Vector3(1971, 1943, 0), Bulk = 0.21f, BioEntity = new Maps.MapEditor.BiologicalEntity() //ce
                             { AgeGroup = AIAgeGroup.Adult} }, },

                        new SpawnEntityAction("7114a0e5-asf2365262e-4d8c-995d-c28dc842847c") { DelayInSeconds = 9,
                             EntityData = new EntityData()
                             { EntityKey = "entity:binalRat", Name = "BinalRat3", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegianceNorth" },
                             Location = new Vector3(1641, 953, 0), Bulk = 0.21f, BioEntity = new Maps.MapEditor.BiologicalEntity()//n
                             { AgeGroup = AIAgeGroup.Adult} }, },

                        new SpawnEntityAction("befad2d9-e5saf53qq4-4664-bd2d-e6aa13c1f489") { DelayInSeconds = 6,
                             EntityData = new EntityData()
                             { EntityKey = "entity:binalRat", Name = "BinalRat4", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegianceNorth" },
                             Location = new Vector3(2424, 672, 0), Bulk = 0.21f, BioEntity = new Maps.MapEditor.BiologicalEntity() //n
                             { AgeGroup = AIAgeGroup.Adult } }, },
                        new SpawnEntityAction("d9c660a6-4296-48ed-a02353214-0ec29asf3256b836976") { DelayInSeconds = 9,
                            EntityData = new EntityData()
                            { EntityKey = "entity:binalRat", Name = "BinalRat11", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegianceSouthWest" },
                            Location = new Vector3(1008, 2304, 0), Bulk = 0.21f, BioEntity = new Maps.MapEditor.BiologicalEntity() //sw
                            { AgeGroup = AIAgeGroup.Adult} }, },

                        new SpawnEntityAction("ff90216c-395c-4fsa2623325f-92da-fc9fb3429f22") { DelayInSeconds = 0,
                            EntityData = new EntityData()
                            { EntityKey = "entity:binalRat", Name = "BinalRat6", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegianceSouthWest" },
                            Location = new Vector3(480, 2169, 0), Bulk = 0.21f, BioEntity = new Maps.MapEditor.BiologicalEntity()//sw
                            { AgeGroup = AIAgeGroup.Adult } }, },

                        new SpawnEntityAction("484e1d8d-4b34-4asf365236e-a15e-499a609231eb") { DelayInSeconds = 8,
                            EntityData = new EntityData()
                            { EntityKey = "entity:binalRat", Name = "BinalRat5", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegianceCenterEast" },
                            Location = new Vector3(2256, 2256, 0), Bulk = 0.21f, BioEntity = new Maps.MapEditor.BiologicalEntity() //ce
                            { AgeGroup = AIAgeGroup.Adult} }, }, 

                        new SpawnEntityAction("b4fc6b42-c962-4fsa356238-adef-ff442a207409") { DelayInSeconds = 2,
                             EntityData = new EntityData()
                             { EntityKey = "entity:binalRat", Name = "BinalRat7", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegianceCenterEast" },
                             Location = new Vector3(1971, 1943, 0), Bulk = 0.21f, BioEntity = new Maps.MapEditor.BiologicalEntity() //ce
                             { AgeGroup = AIAgeGroup.Adult } }, },

                        new SpawnEntityAction("7114a0e5-072e-af23626262dghhyc-995d-c28dc842847c") { DelayInSeconds = 9,
                             EntityData = new EntityData()
                             { EntityKey = "entity:binalRat", Name = "BinalRat3", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegianceNorth" },
                             Location = new Vector3(1641, 953, 0), Bulk = 0.21f, BioEntity = new Maps.MapEditor.BiologicalEntity()//n
                             { AgeGroup = AIAgeGroup.Adult } }, },

                        new SpawnEntityAction("befad2d9-e5e4-46faq36534-bd2d-e6aa13c1f489") { DelayInSeconds = 6,
                             EntityData = new EntityData()
                             { EntityKey = "entity:binalRat", Name = "BinalRat4", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegianceNorth" },
                             Location = new Vector3(2424, 672, 0), Bulk = 0.21f, BioEntity = new Maps.MapEditor.BiologicalEntity() //n
                             { AgeGroup = AIAgeGroup.Adult } }, },
                             
//twinkler

                        new SpawnEntityAction("6d578ffsa5qac-ccq53a4-4df5-8d24-51adf5626ca9") { DelayInSeconds = 0,
                        EntityData = new EntityData()
                            { EntityKey = "entity:twinkler",  Name = "twinkler3", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "twinklerAllegiance" }, 
                                Location = new Vector3(1008, 1632, 0), Bulk = 0.25f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult, CasteKey = "hunter" } }, },

//for video june 2016 remove:
       /*                 new EventActionType("6d57saf58fac-cca4-4df5-8d24-51adf5626ca9") { DelayInSeconds = 10,
                        SpawnEntity = new SpawnEntityAction() {EntityData = new EntityData()
                            { EntityKey = "entity:twinkler",  Name = "twinkler3", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "twinklerAllegiance" }, 
                                Location = new Vector3(1584, 1152, 0), Bulk = 0.25f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeInYears = 12, CasteKey = "hunter" } }, }},
                        new EventActionType("6d578dsa5fac-cca4-4d5325f5-8d24-51adf5626ca9") { DelayInSeconds = 20,
                        SpawnEntity = new SpawnEntityAction() {EntityData = new EntityData()
                            { EntityKey = "entity:twinkler",  Name = "twinkler3", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "twinklerAllegiance" }, 
                                Location = new Vector3(1584, 1152, 0), Bulk = 0.25f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeInYears = 12, CasteKey = "hunter" } }, }},
                        new EventActionType("6d572538fac-ccsaa4-4df5-8d24-51adf5626ca9") { DelayInSeconds = 20,
                        SpawnEntity = new SpawnEntityAction() {EntityData = new EntityData()
                            { EntityKey = "entity:twinkler",  Name = "twinkler3", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "twinklerAllegiance" }, 
                                Location = new Vector3(1584, 1152, 0), Bulk = 0.25f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeInYears = 12, CasteKey = "hunter" } }, }},
                        new EventActionType("6d5fsa578fac-cca4-4d325f5-8d24532-51adf5626ca9") { DelayInSeconds = 23,
                        SpawnEntity = new SpawnEntityAction() {EntityData = new EntityData()
                            { EntityKey = "entity:twinkler",  Name = "twinkler3", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "twinklerAllegiance" }, 
                                Location = new Vector3(1584, 1152, 0), Bulk = 0.25f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeInYears = 12, CasteKey = "hunter" } }, }},
                        new EventActionType("6d57afs538fac-ccdgsa4-4df5-8d24-51adf5626ca9") { DelayInSeconds = 23,
                        SpawnEntity = new SpawnEntityAction() {EntityData = new EntityData()
                            { EntityKey = "entity:twinkler",  Name = "twinkler3", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "twinklerAllegiance" }, 
                                Location = new Vector3(1584, 1152, 0), Bulk = 0.25f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeInYears = 12, CasteKey = "hunter" } }, }},
                        new EventActionType("6d57ppoo8fac-cca4-4df5-8d24-51adf5626ca9") { DelayInSeconds = 23,
                        SpawnEntity = new SpawnEntityAction() {EntityData = new EntityData()
                            { EntityKey = "entity:twinkler",  Name = "twinkler3", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "twinklerAllegiance" }, 
                                Location = new Vector3(1584, 1152, 0), Bulk = 0.25f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeInYears = 12, CasteKey = "hunter" } }, }},
                        new EventActionType("6d5ii78fac-cca4-4df5-8d24-51adf5626ca9") { DelayInSeconds = 23,
                        SpawnEntity = new SpawnEntityAction() {EntityData = new EntityData()
                            { EntityKey = "entity:twinkler",  Name = "twinkler3", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "twinklerAllegiance" }, 
                                Location = new Vector3(1584, 1152, 0), Bulk = 0.25f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeInYears = 12, CasteKey = "hunter" } }, }},

                        new EventActionType("6d57uu8fac-cca4-4df5-8d24-51adf5626ca9") { DelayInSeconds = 27,
                        SpawnEntity = new SpawnEntityAction() {EntityData = new EntityData()
                            { EntityKey = "entity:twinkler",  Name = "twinkler3", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "twinklerAllegiance" }, 
                                Location = new Vector3(1584, 1152, 0), Bulk = 0.25f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeInYears = 12, CasteKey = "hunter" } }, }},
                        new EventActionType("6d57yy8fac-cca4-4df5-8d24-51adf5626ca9") { DelayInSeconds = 27,
                        SpawnEntity = new SpawnEntityAction() {EntityData = new EntityData()
                            { EntityKey = "entity:twinkler",  Name = "twinkler3", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "twinklerAllegiance" }, 
                                Location = new Vector3(1584, 1152, 0), Bulk = 0.25f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeInYears = 12, CasteKey = "hunter" } }, }},
                        new EventActionType("6d57tt8fac-cca4-4df5-8d24-51adf5626ca9") { DelayInSeconds = 27,
                        SpawnEntity = new SpawnEntityAction() {EntityData = new EntityData()
                            { EntityKey = "entity:twinkler",  Name = "twinkler3", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "twinklerAllegiance" }, 
                                Location = new Vector3(1584, 1152, 0), Bulk = 0.25f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeInYears = 12, CasteKey = "hunter" } }, }},
       */

                        }
                    }
                    }
                }
            });

            #endregion

            #region DEMOISLANDMAP_timedSpawnBeginningPopulationEasy

            //  easy fauna: Beginning animal population (spawn)..only a hunter:
            list.Add(new PolledEventType()
            {
                KeyName = "DEMOISLANDMAP_timedSpawnBeginningPopulationEasy",
                StartTimePoint = new TimePoint()
                {
                    RelativeNoOfDays = 0.0
                },               
                ActionSets = new ActionSets()
                {
                    SetsOfActions = new []{ new ActionSetType("7f82ccc4-a32c-4367-aa10-675ad840b20e")
                    {     
                             
                        Actions = new EventActionType[]{

// Thunder chicken Start Spawns //
                        new SpawnEntityAction("38asf62774270-96d3-49ec-9aafafc-5808e1e90714") { DelayInSeconds = 0,
                            EntityData = new EntityData()
                            { EntityKey = "entity:whiteThunderChicken", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "thunderChickenAllegianceSouth" }, 
                              Location = new Vector3(1546,2256, 0), Rotation = 115, Bulk = 0.25f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult } }, },
                        new SpawnEntityAction("38bbd970-9af626263-af262626ec-9afc-5808e1e90714") { DelayInSeconds = 0,
                            EntityData = new EntityData()
                            { EntityKey = "entity:whiteThunderChicken", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "thunderChickenAllegianceSouth" }, 
                              Location = new Vector3(1546,2256, 0), Rotation = 115, Bulk = 0.25f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult } }, },
                        new SpawnEntityAction("38af26267utipp0-96d3-49ec-9afc-5808e1e90714") { DelayInSeconds = 0,
                            EntityData = new EntityData()
                            { EntityKey = "entity:pygmyThunderChicken", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "thunderChickenAllegianceNorth" }, 
                              Location = new Vector3(826,1114, 0), Rotation = 115, Bulk = 0.25f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult } }, },
                        new SpawnEntityAction("3asxczxwetwet0-96d3-49ec-9afc-5808e1e90714") { DelayInSeconds = 0,
                            EntityData = new EntityData()
                            { EntityKey = "entity:pygmyThunderChicken", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "thunderChickenAllegianceNorth" }, 
                              Location = new Vector3(826,1114, 0), Rotation = 115, Bulk = 0.25f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult } }, },
//birds
                        new SpawnEntityAction("38bafs62626tiupp0-96d3-49fsec-9afc-5808e1e90714") { DelayInSeconds = 0,
                        EntityData = new EntityData()
                            { EntityKey = "entity:bird", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "Allegiance #43" }, 
                              Location = new Vector3(448, 2719, 0), Rotation = 115, Bulk = 0.12f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult} }, },

                        new SpawnEntityAction("234dc157-b3ad-4449-b6c1-54b98bfe7dd1") { DelayInSeconds = 1,
                        EntityData = new EntityData()
                            { EntityKey = "entity:bird", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "Allegiance #43" }, 
                              Location = new Vector3(489, 2734, 0), Rotation = 167, Bulk = 0.12f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult} }, },

                        new SpawnEntityAction("09e5663f-bbfd-48d7-b7e5-8c808d30bbf8") { DelayInSeconds = 1.85,
                        EntityData = new EntityData()
                            { EntityKey = "entity:bird", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "Allegiance #43" }, 
                              Location = new Vector3(565, 2869, 0), Rotation = 208, Bulk = 0.12f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult} }, },

                        new SpawnEntityAction("a59c4228-1620-4d56-95ec-35b8b98f72c8") { DelayInSeconds = 2.6,
                        EntityData = new EntityData()
                            { EntityKey = "entity:bird", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "Allegiance #43" }, 
                              Location = new Vector3(580, 2839, 0), Rotation = 137, Bulk = 0.12f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult} }, },

                        new SpawnEntityAction("434a7d89-400a-4a1c-a56d-eed1716d6f7a") { DelayInSeconds = 3.2,
                        EntityData = new EntityData()
                            { EntityKey = "entity:bird", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "Allegiance #43" }, 
                              Location = new Vector3(626, 2854, 0), Rotation = 316, Bulk = 0.12f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult} }, },

                        new SpawnEntityAction("e5a3e20f-fe7d-4a1d-aecd-2194ce8e13dd") { DelayInSeconds = 2,
                        EntityData = new EntityData()
                            { EntityKey = "entity:bird", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "Allegiance #43" }, 
                              Location = new Vector3(659, 2908, 0), Rotation = 112, Bulk = 0.12f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult} }, },
//binal rats
                        new SpawnEntityAction("d9c660a6-4296-4vx8ed-a014-0asfdzbzbec29b836976") { DelayInSeconds = 9,
                            EntityData = new EntityData()
                            { EntityKey = "entity:binalRat", Name = "BinalRat11", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegianceSouthWest" },
                            Location = new Vector3(1008, 2304, 0), Bulk = 0.21f, BioEntity = new Maps.MapEditor.BiologicalEntity() //sw
                            { AgeGroup = AIAgeGroup.Adult} }, },

                        new SpawnEntityAction("ff90216c-395c-42df-9fa25326a-fc9fb3429f22") { DelayInSeconds = 0,
                            EntityData = new EntityData()
                            { EntityKey = "entity:binalRat", Name = "BinalRat6", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegianceSouthWest" },
                            Location = new Vector3(480, 2169, 0), Bulk = 0.21f, BioEntity = new Maps.MapEditor.BiologicalEntity()//sw
                            { AgeGroup = AIAgeGroup.Adult } }, },

                        new SpawnEntityAction("484e1d8d-4b34-4d0e-aahtuyipppe-499a609231eb") { DelayInSeconds = 8,
                            EntityData = new EntityData()
                            { EntityKey = "entity:binalRat", Name = "BinalRat5", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegianceCenterEast" },
                            Location = new Vector3(2256, 2256, 0), Bulk = 0.21f, BioEntity = new Maps.MapEditor.BiologicalEntity() //ce
                            { AgeGroup = AIAgeGroup.Adult } }, }, 

                        new SpawnEntityAction("b4fc6b42-c962-4de8-afa62626-ff442a207409") { DelayInSeconds = 2,
                             EntityData = new EntityData()
                             { EntityKey = "entity:binalRat", Name = "BinalRat7", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegianceCenterEast" },
                             Location = new Vector3(1971, 1943, 0), Bulk = 0.21f, BioEntity = new Maps.MapEditor.BiologicalEntity() //ce
                             { AgeGroup = AIAgeGroup.Adult } }, },

                        new SpawnEntityAction("7114a0e5-072e-4d8c-9fa2626-c28dc842847c") { DelayInSeconds = 9,
                             EntityData = new EntityData()
                             { EntityKey = "entity:binalRat", Name = "BinalRat3", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegianceNorth" },
                             Location = new Vector3(1641, 953, 0), Bulk = 0.21f, BioEntity = new Maps.MapEditor.BiologicalEntity()//n
                             { AgeGroup = AIAgeGroup.Adult } }, },

                        new SpawnEntityAction("befad2d9-e5e4-4664fa5q235d-e6aa13c1f489") { DelayInSeconds = 6,
                             EntityData = new EntityData()
                             { EntityKey = "entity:binalRat", Name = "BinalRat4", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegianceNorth" },
                             Location = new Vector3(2424, 672, 0), Bulk = 0.21f, BioEntity = new Maps.MapEditor.BiologicalEntity() //n
                             { AgeGroup = AIAgeGroup.Adult } }, },
                        new SpawnEntityAction("d9c66pp0a6-4pp296-4pp8ed-a014-0ec29b836976") { DelayInSeconds = 9,
                            EntityData = new EntityData()
                            { EntityKey = "entity:binalRat", Name = "BinalRat11", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegianceSouthWest" },
                            Location = new Vector3(1008, 2304, 0), Bulk = 0.21f, BioEntity = new Maps.MapEditor.BiologicalEntity() //sw
                            { AgeGroup = AIAgeGroup.Adult} }, },

                        new SpawnEntityAction("ff90216c-395c-42df-92da-fc9ffa233652652639f22") { DelayInSeconds = 0,
                            EntityData = new EntityData()
                            { EntityKey = "entity:binalRat", Name = "BinalRat6", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegianceSouthWest" },
                            Location = new Vector3(480, 2169, 0), Bulk = 0.21f, BioEntity = new Maps.MapEditor.BiologicalEntity()//sw
                            { AgeGroup = AIAgeGroup.Adult } }, },

                        new SpawnEntityAction("484e1d8d-4b34-4d0e-a15e-499fa262326oupzz231eb") { DelayInSeconds = 8,
                            EntityData = new EntityData()
                            { EntityKey = "entity:binalRat", Name = "BinalRat5", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegianceCenterEast" },
                            Location = new Vector3(2256, 2256, 0), Bulk = 0.21f, BioEntity = new Maps.MapEditor.BiologicalEntity() //ce
                            { AgeGroup = AIAgeGroup.Adult } }, }, 

                        new SpawnEntityAction("b4fc6b42-c962-4de8-adef-ff44fa26326uippp07409") { DelayInSeconds = 2,
                             EntityData = new EntityData()
                             { EntityKey = "entity:binalRat", Name = "BinalRat7", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegianceCenterEast" },
                             Location = new Vector3(1971, 1943, 0), Bulk = 0.21f, BioEntity = new Maps.MapEditor.BiologicalEntity() //ce
                             { AgeGroup = AIAgeGroup.Adult } }, },

                        new SpawnEntityAction("7114a0e5-072e-4d8c-995d-c28dafs276897opp847c") { DelayInSeconds = 9,
                             EntityData = new EntityData()
                             { EntityKey = "entity:binalRat", Name = "BinalRat3", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegianceNorth" },
                             Location = new Vector3(1641, 953, 0), Bulk = 0.21f, BioEntity = new Maps.MapEditor.BiologicalEntity()//n
                             { AgeGroup = AIAgeGroup.Adult } }, },

                        new SpawnEntityAction("befad2d9-e5e4-4664-bd2d-efqq5353wafaew3c1f489") { DelayInSeconds = 6,
                             EntityData = new EntityData()
                             { EntityKey = "entity:binalRat", Name = "BinalRat4", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegianceNorth" },
                             Location = new Vector3(2424, 672, 0), Bulk = 0.21f, BioEntity = new Maps.MapEditor.BiologicalEntity() //n
                             { AgeGroup = AIAgeGroup.Adult } }, },

//twinkler

                        new SpawnEntityAction("6d578sksjhsgerdxffac-cca4-4df5-8d24-51adf5626ca9") { DelayInSeconds = 0,
                        EntityData = new EntityData()
                            { EntityKey = "entity:twinkler",  Name = "twinkler3", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "twinklerAllegiance" }, 
                                Location = new Vector3(1008, 1632, 0), Bulk = 0.25f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult, CasteKey = "hunter" } }, },



                        }
                    }
                    }
                }
            });

            #endregion




            #endregion

            //continual polling, that's why these spawns are in this doc:
            #region   DEMOISLANDMAP Rat Nest Respawns
           /* list.Add(new GlobalConditionalEvent()
            {
                KeyName = "DEMOISLANDMAP_respawnRatNestSouth",
                ConditionSet = new ConditionSet()
                {
                    PollInterval = new ValueNode() { PropertyKey = "RatNestSpawnInterval" },  //  sec. .......(there's 1600 sec / day) 
                            Value = new Condition()
                            {
                                CustomCondition = new CustomCondition()
                                {
                                    TargetObject = new TargetObject()
                                    {
                                        GetList = new GetList()
                                        {
                                            HasPropertiesListKey = "entities",
                                            FilterCondition = new PropertyCondition()
                                            {
                                                PropertyKey = "name",
                                                StringEqual = "Rat nest (coord. 25;52)"
                                            }
                                        }
                                    },
                                    ListCondition = new ListCondition() { CountEqual = 0 }
                                }

                            }
                },
                ActionSetsKey = "respawnRatNestSouth"
            });
            list.Add(new GlobalConditionalEvent()
            {
                KeyName = "DEMOISLANDMAP_respawnRatNestCenter",
                ConditionSet = new ConditionSet()
                {
                    PollInterval = new ValueNode() { PropertyKey = "RatNestSpawnInterval" },  //  sec. .......(there's 1600 sec / day) 
                    Value = new Condition()
                    {
                        CustomCondition = new CustomCondition()
                        {
                            TargetObject = new TargetObject()
                            {
                                GetList = new GetList()
                                {
                                    HasPropertiesListKey = "entities",
                                    FilterCondition = new PropertyCondition()
                                    {
                                        PropertyKey = "name",
                                        StringEqual = "Rat nest (coord. 28;29)"
                                    }
                                }
                            },
                            ListCondition = new ListCondition() { CountEqual = 0 }
                        }

                    }
                },
                ActionSetsKey = "respawnRatNestCenter"
            });
            list.Add(new GlobalConditionalEvent()
            {
                KeyName = "DEMOISLANDMAP_respawnRatNestNorth",
                ConditionSet = new ConditionSet()
                {
                    PollInterval = new ValueNode() { PropertyKey = "RatNestSpawnInterval" },  //  sec. .......(there's 1600 sec / day) 
                    Value = new Condition()
                    {
                        CustomCondition = new CustomCondition()
                        {
                            TargetObject = new TargetObject()
                            {
                                GetList = new GetList()
                                {
                                    HasPropertiesListKey = "entities",
                                    FilterCondition = new PropertyCondition()
                                    {
                                        PropertyKey = "name",
                                        StringEqual = "Rat nest (coord. 56;17)"
                                    }
                                }
                            },
                            ListCondition = new ListCondition() { CountEqual = 0 }
                        }

                    }
                },
                ActionSetsKey = "respawnRatNestNorth"
            });*/
            #endregion

            #region DEMOISLANDMAP Binal Rats spawn

            list.Add(new PolledEventType()
            {
                KeyName = "DEMOISLANDMAP_continualSpawnBinalRatsSouthWest",
                PollInterval = new ValueNode() { PropertyKey = "binalRatSpawnInterval" /*Decimal = 340f*/ },  //  sec. .......(there's 1600 sec / day) 
                StartAfterInterval = true,
                Condition = new ConditionFunction()
                {
                    Operator = OperatorType.And,
                    Left = new CustomCondition()
                    {
                        // don't exceed max number of binal rats:
                        TargetObject = new TargetObject()
                        {
                            GetList = new GetList()
                            {
                                HasPropertiesListKey = "allegiances",
                                FilterCondition = new PropertyCondition()
                                {
                                    PropertyKey = "keyName",
                                    ConstantStringEqual = "binalRatAllegianceSouthWest"
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
                    Right = new CustomCondition()
                    {
                        TargetObject = new TargetObject()
                        {
                            GetList = new GetList()
                            {
                                HasPropertiesListKey = "entities",
                                FilterCondition = new PropertyCondition()
                                {
                                    PropertyKey = "name",
                                    ConstantStringEqual = "Rat nest (coord. 25;52)"
                                }
                            }
                        },
                        ListCondition = new ListCondition() { CountEqual = 1 }
                    }
                },
                ActionSetsKey = "continualSpawnBinalRatsSouthWest"
            });


            list.Add(new PolledEventType()
            {
                KeyName = "DEMOISLANDMAP_continualSpawnBinalRatsCenterEast",
                PollInterval = new ValueNode() { PropertyKey = "binalRatSpawnInterval" /*Decimal = 340f*/ },  //  sec. .......(there's 1600 sec / day) 
                StartAfterInterval = true,
                Condition = new ConditionFunction()
                    {
                        Operator = OperatorType.And,
                        Left = new CustomCondition()
                        {
                            // don't exceed max number of binal rats:
                            TargetObject = new TargetObject()
                            {
                                GetList = new GetList()
                                {
                                    HasPropertiesListKey = "allegiances",
                                    FilterCondition = new PropertyCondition()
                                    {
                                        PropertyKey = "keyName",
                                        ConstantStringEqual = "binalRatAllegianceCenterEast"
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
                        Right = new CustomCondition()
                        {
                            TargetObject = new TargetObject()
                            {
                                GetList = new GetList()
                                {
                                    HasPropertiesListKey = "entities",
                                    FilterCondition = new PropertyCondition()
                                    {
                                        PropertyKey = "name",
                                        ConstantStringEqual = "Rat nest (coord. 28;29)"
                                    }
                                }
                            },
                            ListCondition = new ListCondition() { CountEqual = 1 }
                        }
                },
                ActionSetsKey = "continualSpawnBinalRatsCenterEast"
            });






            list.Add(new PolledEventType()
            {
                KeyName = "DEMOISLANDMAP_continualSpawnBinalRatsNorth",
                PollInterval = new ValueNode() { PropertyKey = "binalRatSpawnInterval" /*Decimal = 340f*/  },  //  sec. .......(there's 1600 sec / day) 
                StartAfterInterval = true,
                Condition = new ConditionFunction()
                    {
                        Operator = OperatorType.And,
                        Left = new CustomCondition()
                            {
                                // don't exceed max number of binal rats:
                                TargetObject = new TargetObject()
                                {
                                    GetList = new GetList()
                                    {
                                        HasPropertiesListKey = "allegiances",
                                        FilterCondition = new PropertyCondition()
                                        {
                                            PropertyKey = "keyName",
                                            ConstantStringEqual = "binalRatAllegianceNorth"
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
                        Right = new CustomCondition()
                        {
                            TargetObject = new TargetObject()
                            {
                                GetList = new GetList()
                                {
                                    HasPropertiesListKey = "entities",
                                    FilterCondition = new PropertyCondition()
                                    {
                                        PropertyKey = "name",
                                        ConstantStringEqual = "Rat nest (coord. 56;17)"
                                    }
                                }
                            },
                            ListCondition = new ListCondition() { CountEqual = 1 }
                        }                           
                        
                    
                },

                ActionSetsKey = "continualSpawnBinalRatsNorth"
            });



            #endregion

            #region DEMOISLANDMAP Thunder chicken spawn

            list.Add(new PolledEventType()
            {
                KeyName = "DEMOISLANDMAP_continualSpawnThunderChickensSouth",
                PollInterval = new ValueNode() { PropertyKey = "thunderChickenSpawnInterval" }, //Bso changed from 12 to 800 so it isn't an unlimited food supply  //  sec. .......(there's 1600 sec / day)  
                StartAfterInterval = true,
                Condition = new CustomCondition()
                {
                    // don't exceed max number of binal rats:
                    TargetObject = new TargetObject()
                    {
                        GetList = new GetList()
                        {
                            HasPropertiesListKey = "allegiances",
                            FilterCondition = new PropertyCondition()
                            {
                                PropertyKey = "keyName",
                                ConstantStringEqual = "thunderChickenAllegianceSouth"
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

                ActionSetsKey = "continualSpawnThunderChickensSouth"
            });

            list.Add(new PolledEventType()
            {
                KeyName = "DEMOISLANDMAP_continualSpawnThunderChickensNorth",
                PollInterval = new ValueNode() { PropertyKey = "thunderChickenSpawnInterval" },  //Bso changed from 12 to 800 so it isn't an unlimited food supply //  sec. .......(there's 1600 sec / day) 
                StartAfterInterval = true,
                Condition = new CustomCondition()
                        {
                        // don't exceed max number of chickens:
                        TargetObject = new TargetObject()
                        {
                            GetList = new GetList()
                            {
                                HasPropertiesListKey = "allegiances",
                                FilterCondition = new PropertyCondition()
                                {
                                    PropertyKey = "keyName",
                                    ConstantStringEqual = "thunderChickenAllegianceNorth"
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

                ActionSetsKey = "continualSpawnThunderChickensNorth"
            });

            #endregion

            #region DEMOISLANDMAP Thin thunder chicken spawn
            //bso the thunder chicken spawn depends on the spawn location of the human expediton
            list.Add(new PolledEventType()
            {
                KeyName = "DEMOISLANDMAP_continualSpawnThinThunderChickensSouth",
                PollInterval = new ValueNode() { PropertyKey = "thinThunderChickenSpawnInterval" }, //Bso changed from 12 to 800 so it isn't an unlimited food supply  //  sec. .......(there's 1600 sec / day)  
                StartAfterInterval = true,
                Condition = new CustomCondition()
                    {
                        // don't exceed max number of binal rats:
                        TargetObject = new TargetObject()
                        {
                            GetList = new GetList()
                            {
                                HasPropertiesListKey = "allegiances",
                                FilterCondition = new PropertyCondition()
                                {
                                    PropertyKey = "keyName",
                                    ConstantStringEqual = "thinThunderChickenAllegianceSouth"
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
                                PropertyKey = "maxThinThunderChicken"
                            }
                        }
                },

                ActionSetsKey = "continualSpawnThinThunderChickensSouth"
            });
            list.Add(new PolledEventType()
            {
                KeyName = "DEMOISLANDMAP_continualSpawnThinThunderChickensNorth",
                PollInterval = new ValueNode() { PropertyKey = "thinThunderChickenSpawnInterval" }, //Bso changed from 12 to 800 so it isn't an overflow food supply  //  sec. .......(there's 1600 sec / day)  
                StartAfterInterval = true,
                Condition = new CustomCondition()
                    {
                        // don't exceed max number of binal rats:
                        TargetObject = new TargetObject()
                        {
                            GetList = new GetList()
                            {
                                HasPropertiesListKey = "allegiances",
                                FilterCondition = new PropertyCondition()
                                {
                                    PropertyKey = "keyName",
                                    ConstantStringEqual = "thinThunderChickenAllegianceNorth"
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
                                PropertyKey = "maxThinThunderChicken"
                            }
                        }
                },

                ActionSetsKey = "continualSpawnThinThunderChickensNorth"
            });

            #endregion

            #region DEMOISLANDMAP Twinkler spawn


            // two nests spawn at the same time??? start time should be offset. MP


            int maxTwinklers = 25;



            list.Add(new PolledEventType()
            {
                KeyName = "DEMOISLANDMAP_continualSpawnTwinklerSouthSandstoneCave", // For medium and hard difficulty, spawns guards and hunters, building up armies:
                PollInterval = new ValueNode() { PropertyKey = "twinklerSpawnIntervalSouth" /*Decimal = 340f*/  },  //  sec. .......(there's 1600 sec / day)  //spawns more rarely
                StartAfterInterval = true,
                Condition = new ConditionFunction()
                    {
                        Operator = OperatorType.And,
                        Left = new CustomCondition()
                            {
                                // don't exceed max number of twinklers:
                                TargetObject = new TargetObject()
                                {
                                    GetList = new GetList()
                                    {
                                        HasPropertiesListKey = "allegiances",
                                        FilterCondition = new PropertyCondition()
                                        {
                                            PropertyKey = "keyName",
                                            ConstantStringEqual = "twinklerAllegiance"
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
                                            PropertyKey = "maxTwinklers" //mp maybe make a new one "maxTwinklersSouth"
                                        }
                                        
                                }
                        },
                        Right = new CustomCondition()
                        {
                            TargetObject = new TargetObject()
                            {
                                GetList = new GetList()
                                {
                                    HasPropertiesListKey = "entities",
                                    FilterCondition = new PropertyCondition()
                                    {
                                        PropertyKey = "name",
                                        ConstantStringEqual = "Quadite nest (coord. 15;34)"
                                    }
                                }
                            },
                            ListCondition = new ListCondition() { CountEqual = 1 }
                        }          
                },

                ActionSetsKey = "continualTwinklerSpawnSouthSandstoneCave"

            });






            list.Add(new PolledEventType()
            {
                KeyName = "DEMOISLANDMAP_continualSpawnTwinklerEastRockCave",  // For medium and hard difficulty, spawns guards and hunters, building up armies:
                PollInterval = new ValueNode() { PropertyKey = "twinklerSpawnIntervalEast" /* Decimal = 260*/  },  // seconds //spawns more often 
                StartAfterInterval = true,
                Condition = new ConditionFunction()
                    {
                        Operator = OperatorType.And,
                        Left = new CustomCondition()
                            {
                                // don't exceed max number of twinklers:
                                TargetObject = new TargetObject()
                                {
                                    GetList = new GetList()
                                    {
                                        HasPropertiesListKey = "allegiances",
                                        FilterCondition = new PropertyCondition()
                                        {
                                            PropertyKey = "keyName",
                                            ConstantStringEqual = "twinklerAllegiance"
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
                                            PropertyKey = "maxTwinklers" //mp maybe make a new one "maxTwinklersEast"
                                        }
                                        
                                }
                                                         
                            
                        },
                        Right = new CustomCondition()
                        {
                            TargetObject = new TargetObject()
                            {
                                GetList = new GetList()
                                {
                                    HasPropertiesListKey = "entities",
                                    FilterCondition = new PropertyCondition()
                                    {
                                        PropertyKey = "name",
                                        ConstantStringEqual = "Quadite nest (coord. 38;38)"
                                    }
                                }
                            },
                            ListCondition = new ListCondition() { CountEqual = 1 }
                            
                        }                    
                },

                ActionSetsKey = "continualTwinklerSpawnEastRockCave"

            });



            list.Add(new PolledEventType()
            {
                KeyName = "DEMOISLANDMAP_continualSpawnTwinklerSouthRockCave",  // For medium and hard difficulty, spawns guards and hunters, building up armies:
                PollInterval = new ValueNode() { PropertyKey = "twinklerSpawnIntervalEast" /* Decimal = 260*/  },  // seconds //spawns more often 
                StartAfterInterval = true,
                Condition = new ConditionFunction()
                    {
                        Operator = OperatorType.And,
                        Left = new CustomCondition()
                            {
                                // don't exceed max number of twinklers:
                                TargetObject = new TargetObject()
                                {
                                    GetList = new GetList()
                                    {
                                        HasPropertiesListKey = "allegiances",
                                        FilterCondition = new PropertyCondition()
                                        {
                                            PropertyKey = "keyName",
                                            ConstantStringEqual = "twinklerAllegiance"
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
                                            PropertyKey = "maxTwinklers" //mp maybe make a new one "maxTwinklersEast"
                                        }
                                        
                                }
                                                         
                            
                        },
                        Right = new CustomCondition()
                        {
                            TargetObject = new TargetObject()
                            {
                                GetList = new GetList()
                                {
                                    HasPropertiesListKey = "entities",
                                    FilterCondition = new PropertyCondition()
                                    {
                                        PropertyKey = "name",
                                        ConstantStringEqual = "Quadite nest (coord. 25;44)"
                                    }
                                }
                            },
                            ListCondition = new ListCondition() { CountEqual = 1 }
                        }
                    
                },

                //ActionSets = GameData.Instance.AllActionSets["continualTwinklerSpawnSouthRockCave"] gives duplicate key error
                ActionSetsKey = "continualTwinklerSpawnSouthRockCave"
            });



            list.Add(new PolledEventType()
            {
                KeyName = "DEMOISLANDMAP_continualSpawnTwinklerNorthRockCave",  // For medium and hard difficulty, spawns guards and hunters, building up armies:
                PollInterval = new ValueNode() { PropertyKey = "twinklerSpawnIntervalEast" },
                StartAfterInterval = true,
                Condition = new ConditionFunction()
                    {
                        Operator = OperatorType.And,
                        Left = new CustomCondition()
                            {
                                // don't exceed max number of twinklers:
                                TargetObject = new TargetObject()
                                {
                                    GetList = new GetList()
                                    {
                                        HasPropertiesListKey = "allegiances",
                                        FilterCondition = new PropertyCondition()
                                        {
                                            PropertyKey = "keyName",
                                            ConstantStringEqual = "twinklerAllegiance"
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
                                            PropertyKey = "maxTwinklers" //mp maybe make a new one "maxTwinklersEast"
                                        }
                                        
                                
                            }                                
                            
                        },
                        Right = new CustomCondition()
                        {
                            TargetObject = new TargetObject()
                            {
                                GetList = new GetList()
                                {
                                    HasPropertiesListKey = "entities",
                                    FilterCondition = new PropertyCondition()
                                    {
                                        PropertyKey = "name",
                                        ConstantStringEqual = "Quadite nest (coord. 36;20)"
                                    }
                                }
                            },
                            ListCondition = new ListCondition() { CountEqual = 1 }
                            
                            
                        }                    
                },

              //  ActionSets = GameData.Instance.AllActionSets["continualTwinklerSpawnNorthRockCave"]
                ActionSetsKey = "continualTwinklerSpawnNorthRockCave"

            });




            ///////////easy twinkler spawn


            list.Add(new PolledEventType()
            {
                KeyName = "DEMOISLANDMAP_continualHunterSpawnSouthSandstoneCave", // For easy difficulty, spawns only  hunters, not building up armies. only diff is:    ActionSetsKey = "continualHunterSpawnSouthSandstoneCave"]
                PollInterval = new ValueNode() { PropertyKey = "twinklerSpawnIntervalSouth" },
                StartAfterInterval = true,
                Condition = new ConditionFunction()
                    {
                        Operator = OperatorType.And,
                        Left = new CustomCondition()
                        {
                            // don't exceed max number of twinklers:
                            TargetObject = new TargetObject()
                            {
                                GetList = new GetList()
                                {
                                    HasPropertiesListKey = "allegiances",
                                    FilterCondition = new PropertyCondition()
                                    {
                                        PropertyKey = "keyName",
                                        ConstantStringEqual = "twinklerAllegiance"
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
                                        PropertyKey = "maxTwinklers" //mp maybe make a new one "maxTwinklersSouth"
                                    }
                                        
                            }   
                        },
                        Right = new CustomCondition()
                        {
                            TargetObject = new TargetObject()
                            {
                                GetList = new GetList()
                                {
                                    HasPropertiesListKey = "entities",
                                    FilterCondition = new PropertyCondition()
                                    {
                                        PropertyKey = "name",
                                        ConstantStringEqual = "Quadite nest (coord. 15;34)"
                                    }
                                }
                            },
                            ListCondition = new ListCondition() { CountEqual = 1 }
                                                      
                        }
                    
                },

                ActionSetsKey = "continualHunterSpawnSouthSandstoneCave"

            });






            list.Add(new PolledEventType()
            {
                KeyName = "DEMOISLANDMAP_continualHunterSpawnEastRockCave",  // For easy difficulty, spawns only  hunters, not building up armies. only diff is ActionSetsKey = "continualHunterSpawnEastRockCave"]
                PollInterval = new ValueNode() { PropertyKey = "twinklerSpawnIntervalEast" },
                StartAfterInterval = true,
                Condition = new ConditionFunction()
                    {
                        Operator = OperatorType.And,
                        Left = new CustomCondition()
                            {
                                // don't exceed max number of twinklers:
                                TargetObject = new TargetObject()
                                {
                                    GetList = new GetList()
                                    {
                                        HasPropertiesListKey = "allegiances",
                                        FilterCondition = new PropertyCondition()
                                        {
                                            PropertyKey = "keyName",
                                            ConstantStringEqual = "twinklerAllegiance"
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
                                            PropertyKey = "maxTwinklers" //mp maybe make a new one "maxTwinklersEast"
                                        }
                                        
                                }
                            
                            
                        },
                        Right = new CustomCondition()
                            {
                                TargetObject = new TargetObject()
                                {
                                    GetList = new GetList()
                                    {
                                        HasPropertiesListKey = "entities",
                                        FilterCondition = new PropertyCondition()
                                        {
                                            PropertyKey = "name",
                                            ConstantStringEqual = "Quadite nest (coord. 38;38)"
                                        }
                                    }
                                },
                                ListCondition = new ListCondition() { CountEqual = 1 }
                                                        
                        }
                    
                },

                ActionSetsKey = "continualHunterSpawnEastRockCave"

            });




            #endregion

//the area triggers are at home here because they are continually polled:
            #region DEMOISLANDMAP Area triggers


            // Rock crevice. close, NE
            list.Add(new PolledEventType()
            {
                KeyName = "DEMOISLANDMAP_triggerNorthEastCloseRockCrevice",
                UseDefaultPollInterval = true,
                AllowRandomTimeOffset = true,
                Condition = new AreaCondition()
                {
                    Area = new Rectangle(1104, 2016, 288, 192)  // upper left corner coord.(X,Y) , width, height. (pixels)
                }
                ,
                ActionSetsKey = "talkEmptyQuaditeCrevice"

            });



            //  triggered south sandstone cave, medium and hard:
            list.Add(new PolledEventType()
            {
                KeyName = "DEMOISLANDMAP_triggerGuardsSouthSandstoneCave",
                UseDefaultPollInterval = true,
                AllowRandomTimeOffset = true,
                Condition = new AreaCondition()
                {
                    Area = new Rectangle(624, 1488, 192, 144)  // upper left corner coord.(X,Y) , width, height. (pixels)
                }
                ,
                ActionSetsKey = "spawnFirstGuardsSouthSandstoneCave" //;has talk before spawn

            });


            //   triggered rock cave East, medium and hard:
            list.Add(new PolledEventType()
            {
                KeyName = "DEMOISLANDMAP_triggerGuardsRockCaveEast",
                UseDefaultPollInterval = true,
                AllowRandomTimeOffset = true,
                Condition = new AreaCondition()
                {
                    Area = new Rectangle(1680, 1776, 240, 168)  // upper left corner coord.(X,Y) , width, height. (pixels)
                }
                ,
                ActionSetsKey = "spawnFirstGuardsEastRockCave" //;"talkCave"]

            });

            ////////////

            //  triggered south sandstone cave, easy:
            list.Add(new PolledEventType()
            {
                KeyName = "DEMOISLANDMAP_triggerHunterSouthSandstoneCave",
                UseDefaultPollInterval = true,
                AllowRandomTimeOffset = true,
                Condition = new AreaCondition()
                {
                    Area = new Rectangle(624, 1488, 192, 144)  // upper left corner coord.(X,Y) , width, height. (pixels)
                }
                ,
                ActionSetsKey = "spawnFirstHunterSouthSandstoneCave" //;has talk before spawn

            });


            //   triggered rock cave East, easy:
            list.Add(new PolledEventType()
            {
                KeyName = "DEMOISLANDMAP_triggerHunterRockCaveEast",
                UseDefaultPollInterval = true,
                AllowRandomTimeOffset = true,
                Condition = new AreaCondition()
                {
                    Area = new Rectangle(1680, 1776, 240, 168)  // upper left corner coord.(X,Y) , width, height. (pixels)
                }
                ,
                ActionSetsKey = "spawnFirstHunterEastRockCave" //;"talkCave"]

            });

            /////////////////
            list.Add(new PolledEventType()
            {
                KeyName = "DEMOISLANDMAP_triggerVolcanicLandscape",
                UseDefaultPollInterval = true,
                AllowRandomTimeOffset = true,
                Condition = new AreaCondition()
                {
                    Area = new Rectangle(1680, 1008, 580, 480)  // upper left corner coord.(X,Y) , width, height. (pixels)
                },
                ActionSetsKey = "talkVolcanicLandscape"

            });

            //  trigger when into marsh
            list.Add(new PolledEventType()
            {
                KeyName = "DEMOISLANDMAP_triggerMarsh",
                UseDefaultPollInterval = true,
                AllowRandomTimeOffset = true,
                Condition = new AreaCondition()
                {
                    Area = new Rectangle(2208, 1920, 528, 862)  // upper left corner coord.(X,Y) , width, height. (pixels)
                },
                ActionSetsKey = "talkMarsh"

            });
            /*
                        //  trigger north sandstone crevice  //not using
                        list.Add(new GlobalConditionalEvent()
                        {
                            KeyName = "DEMOISLANDMAP_triggerNorthSandstoneCrevice",
                            Condition = new GlobalCondition()
                            {
                                AreaCondition = new AreaCondition()
                                {
                                    Area = new Rectangle(432, 1392, 340, 145)  // upper left corner coord.(X,Y) , width, height. (pixels)
                                }
                            }
                            ,
                          ActionSetsKey = "spawnNorthSandstoneCrevice"] //"talkQuaditeCrevice"]

                         });  
            */

            //  trigger north rock crevice   houses no quadites because of volcanic gas
            list.Add(new PolledEventType()
            {
                KeyName = "DEMOISLANDMAP_triggerNorthRockCrevice",  // north rock crevice
                UseDefaultPollInterval = true,
                AllowRandomTimeOffset = true,
                Condition = new AreaCondition()
                {
                    Area = new Rectangle(1680, 864, 238, 238)  //  upper left corner coord.(X,Y) , width, height. (pixels)
                },
                ActionSetsKey = "talkNorthRockCrevice"

            });

            list.Add(new PolledEventType()
            {
                KeyName = "DEMOISLANDMAP_triggerMuckroot",  // north rock crevice
                Condition = new AreaCondition()
                {
                    Area = new Rectangle(912, 528, 432, 240)  //  upper left corner coord.(X,Y) , width, height. (pixels)
                },
                ActionSetsKey = "talkMuckroot"

            });



            #endregion


            #region Fish and Farms
            /* // use soemthing like this to spawm all the fish and farms in a more ordered and easily readable fashion
            list.Add(new GlobalConditionalEvent()
            {
                KeyName = "SpawnAllTheFishAndFarms",
                ConditionSet = new ConditionSet()
                {
                    Value = new Condition()
                    {
                        TimeCondition = new TimeCondition()
                        {
                            RelativeNoOfDays = 0.0
                        }
                    }
                }
                ,
                ActionSetsKey = "ActuallySpawnAllTheFishAndFarms"

            });
            */
            #endregion
            return list;
        }
    }
}