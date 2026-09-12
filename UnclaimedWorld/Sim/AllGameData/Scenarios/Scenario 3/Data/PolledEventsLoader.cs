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
using UWGame.SimSide.Entities.Biological;
//using UWGame.SimSide.InGameEvents.Expressions;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_3.Data
{
    public class PolledEventsLoader
    {

        public static List<InGameEvents.PolledEventType> Init()
        {
            List<PolledEventType> list = new List<PolledEventType>();

            #region TUTORIAL_musicTrackList //
            list.Add(new PolledEventType()
            {
                KeyName = "TUTORIAL_musicTrackList", //mp soothing playlist for the tut, which does not take into account time of day.
                PollInterval = new ValueNode() { Decimal = 1461f }, //cycle ends at  1461f
                AllowRandomTimeOffset = false, //no staggering
                ActionSets = new ActionSets()
                {
                    SetsOfActions = new []{ new ActionSetType("2452d652-4370-4469-9fsg-frt5hyjuip-a1ebcabe0b83")
                    {                       
                        Actions = new EventActionType[]
                        {   
                            new MusicAction("ytttryrty6547456456456utyudtyudtyu4674674674tyu68")
                            {   DelayInSeconds = 0f, // 
                                
                                    Song = "Jesper Lundager - The Diamond Birds_320"  //  duration - 3 min 1 sec    = 181 sec ........number of days: 1/1600=0,000625 *181 = 0,113
                                
                            },
       
                            new MusicAction("f1254134534151345yteyuteyutes3bab")
                            {   DelayInSeconds = 181f, // 
                                
                                    Song = "Jesper Lundager - Building a Home_320" // duration  -  5 min 2 sec = 302 sec   duration number of days = 0,189.
                                
                            },
                            new MusicAction("a13451345316214624576-8903-30d4473aa068")
                            {   DelayInSeconds = 483f, //181f+302
                                
                                    Song = "Jesper Lundager - Prosperous Frontier_320"  // duration -  4 min 19 sec =  259 sec . number of days: 1/1600=0,000625 *259 = 0,162
                                
                            },
                            new MusicAction("a84245654264256425642ewtyuw5rtyutywrutywraa068")
                            {   DelayInSeconds = 742f, //181f+302f+259f
                                
                                    Song = "Jesper Lundager - Muckroot Toil_320"  // duration -  3 min 19 sec  = 199 sec ...number of days: 1/1600=0,000625 *199 = 0,124
                                
                            },
                            new MusicAction("a524564256425642564256u5sw65eswtywraa068")
                            {   DelayInSeconds = 941f,//181f+302f+259f+199
                                
                                    Song = "Jesper Lundager - Cetian Skies_320"  // duration - 4 min 42 sec   = 282 sec ...hard cropped: 4:34 = 274 sec........number of days: 1/1600=0,000625 *282 = 0,176.....0,171 (hard cropped)
                                
                            },
                            new MusicAction("ad24564256245642564256dygjutsduyudtyu68")
                            {   DelayInSeconds = 1223f, //181f+302f+259f+199+282    
                                
                                    Song = "Martin Hasseldam - Unfamiliar Starlight"  // duration 3 min 58 sec = 238 sec .......number of days: 1/1600=0,000625 *238 = 0,149
                                
                            },



                                 //cycle ends at 1223f+238= 1461f  
                        }
                    }
                }
                }
            });
            #endregion

            #region TUTORIAL timed exposition



            list.Add(new PolledEventType()
            {
                KeyName = "TUTORIAL_introDialogue",
                StartTimePoint = new TimePoint()
                {
                    RelativeNoOfDays = 0.0038
                },
                ActionSets = new ActionSets()
                {
                    SetsOfActions = new []{ new ActionSetType("6f02yyyy814d-0a04-4796-90a4-c2c685b0ee92")
                { 
                        Actions = new EventActionType[]
                        {
                            new TalkAction("54b75xxxxxb99-b8ad-4687-9442-bcc26655ce27")
                        {                                 
                                
                                    TalkPriority = TalkAction.TalkActionPriority.High,
                                    CanTalkWhileFighting = false,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = false,
                                    TurnTowardsListeners = true,
                                    ActionByAgent = ActionByAgent.RandomInAllegiance,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                                    DefaultText = "The storm has finally died down." //
                                
                        },
                        new TalkAction("cd2cxxxxxx0b10-34dd-4193-a1ac-1aa56a68c053")
                        {
                                DelayInSeconds = 3,
                                
                                    TalkPriority = TalkAction.TalkActionPriority.High,
                                    CanTalkWhileFighting = false,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = false,
                                    TurnTowardsListeners = true,
                                    ActionByAgent = ActionByAgent.RandomInAllegiance,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                                    DefaultText = "Those were the tallest waves I've ever seen." // 
                                
                        },

                      new EventActionDialog("532fxxxxxx-ad62-4854-bes8e-f2134bhdde4f4e51") { DelayInSeconds = 7, 
                        DisplayText = new DynamicText(){ Text = "DECISION #1 - Use the PPU \n \nHARRON: I'm just checking our position on the PPU. No info. It's an uncharted island. \n \nSANTILLA: What's that instrument you're using? \n \nHARRON: The PPU? The Pioneer Planning Unit. I got a few of these. I think you should take one each and hang on to them. \n \nSANTILLA: Ok, thanks! Wow...pioneer tech. I've seen people use them but never really knew what they were for... \n \nHARRON: The pioneers had these with them and used them for survival in the early days. \nThe thing tells us about the surroundings. It helps us with keeping track of everything. It'll tell us how to make things, what plants we can eat and so on. To make it out here, we have to rely on the PPU. \n \nSCOYD: They are pretty easy to use -  if you're in doubt, click the button says GUIDE. \n \nHARRON: Yeah do that. It'll get you up to speed. \n \n- // Click the GUIDE button //" ,  //  
                        },
                        DisplayImage = "GroupMeeting",
                         DialogOptions = new[] { new DialogOption()
                         {
                            Text = "GUIDE #1", Tooltip = "See how to carry out this decision (opens separate window.)", ActiveInArchive = true,
                            ActionSet = "showTutorial1" 
                         }}
                      
                                },

                        }
                }
                }

                }
            });

            /////////////

            list.Add(new PolledEventType()
            {
                Comment = "should fire app 27 seconds after decision#1, skip this decision if player has already seen gorge",
                KeyName = "TUTORIAL_scoutDecision",
                StartTimePoint = new TimePoint()
                {
                    RelativeTimeInSeconds = new ValueNode() { Decimal = 35f } //mp was 22f but waay too quick. dammit Lars!    Lars: reduced the waiting time by 18 secs
                    //RelativeNoOfDays = 0.0250 //   
                },
                Condition = new CustomCondition()
                {
                    PropertyCondition = new PropertyCondition()
                    {
                        PropertyKey = "gorgeDetected",
                        BoolValue = false
                    }

                },

                ActionSets = new ActionSets()
                {
                    SetsOfActions = new []{ new ActionSetType("6f035625465345yrtyhrsthyrty646674y76w2")
                { 
                        Actions = new EventActionType[]
                        {
                

                    new EventActionDialog("532fxxrtyurwtyujtyuessyrtsyshgfdhghfhdde4f4e51") { DelayInSeconds = 0, 
                        DisplayText = new DynamicText(){ Text = "DECISION #2 SCOUT \n \nHARRON: Well, there's nothing on this beach, apart from rocks and thorny bramble. \nWe're gonna starve to death if we stay. \n \nSANTILLA: No mussels, no anything? \n \nHARRON: Nah, already checked the area. Look in the PPU. You can see what resources are here. \n \nSCOYD:Let's get going and explore the area further up. \n \n- //Scout the area north of the wreck // -" ,  //  
                        },
                        DisplayImage = "GroupMeeting",
                         DialogOptions = new[] { new DialogOption()
                         {
                            Text = "GUIDE #2", Tooltip = "See how to carry out this decision (opens separate window.)", ActiveInArchive = true,
                            ActionSet = "showTutorial2" 
                         }}
                      
                                }


                        }
                }
                }

                }
            });




            #endregion





            #region  Beginning population spawn and place catamaran and crevice // TODO: make this an action instead


            list.Add(new PolledEventType()
            {
                KeyName = "TUTORIAL_placeCatamaran",
                /* ConditionSet = new ConditionSet()
                 {
                     Value = new TimeCondition()
                         {
                             RelativeNoOfDays = 0.0
                         }
                 },*/
                ActionSetsKey = "placeCatamaranTerrain"
            });


            //mp migrate this to eventactionloader just like placecrevice:

            list.Add(new PolledEventType()
            {
                KeyName = "TUTORIAL_placeTerritoryLock1",
                /*   ConditionSet = new ConditionSet()
                   {
                       Value = new TimeCondition()
                           {
                               RelativeNoOfDays = 0.0
                           }
                   },*/
                ActionSetsKey = "placeTerritoryLock1"
            });

            //mp migrate this to eventactionloader just like placecrevice:
            list.Add(new PolledEventType()
            {
                KeyName = "TUTORIAL_placeTerritoryLock2",
                /* ConditionSet = new ConditionSet()
                 {
                     Value = new TimeCondition()
                         {
                             RelativeNoOfDays = 0.0
                         }
                 },*/
                ActionSetsKey = "placeTerritoryLock2"
            });



            ////Beginning animal population (spawn):

            list.Add(new PolledEventType()
            {
                KeyName = "TUTORIAL_timedSpawnBeginningPopulation",
                ActionSets = new ActionSets()
                {
                    SetsOfActions = new []{ new ActionSetType("382aa085-44c6-48d3-b7a4-a91d7d9c1ca6")
                    {     
                             
                        Actions = new EventActionType[]{

                //Bushdragons
                        new SpawnEntityAction("6a2dfa36-7055-4b40-858d-ca3d7c3bc142") { DelayInSeconds = 0,
                       EntityData = new EntityData()
                            { EntityKey = "entity:bushDragon", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "bushDragonTutAllegianceNorth" },
                              Location = new Vector3(3504, 2420, 0), Bulk = 1.4f, //bulk set high so they dont carry it when hunting.
                              BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult } }, },

                            
                        new SpawnEntityAction("3daeff2f-2bf9-41ae-8dbf-10b386e9581d") { DelayInSeconds = 0,
                       EntityData = new EntityData()
                            { EntityKey = "entity:bushDragon", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "bushDragonTutAllegianceNorth" },
                              Location = new Vector3(3600, 2448, 0), Bulk = 1.4f, //bulk set high so they dont carry it when hunting.
                              BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult } }, },


                       new SpawnEntityAction("df0971ec-ef16-4d39-8b1a-6ee4d752079c") { DelayInSeconds = 0,
                       EntityData = new EntityData()
                            { EntityKey = "entity:bushDragon", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "bushDragonTutAllegianceNorth" },
                              Location = new Vector3(3620, 2496, 0), Bulk = 1.4f, //bulk set high so they dont carry it when hunting.
                              BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult } }, },




                       new SpawnEntityAction("a847c30c-0fea-42e4-8608-33acb1f76fcb") { DelayInSeconds = 0,
                       EntityData = new EntityData()
                            { EntityKey = "entity:bushDragon", Name = "bushDragonSouth", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "bushDragonTutAllegianceSouth" },
                              Location = new Vector3(3456, 2688, 0), Bulk = 1.4f, //bulk set high so they dont carry it when hunting.
                              BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult } }, },

                        }
                    }
                    }
                }
            });



            #endregion


            #region  Area trigger Before crevice

            list.Add(new PolledEventType()
            {
                KeyName = "TUTORIAL_triggerBeforeCrevice", // triggers an event window about how to using RMB-navigation
                UseDefaultPollInterval = true,
                AllowRandomTimeOffset = true,
                Condition = new AreaCondition()
                        {
                            Area = new Rectangle(2880, 3696, 432, 192)  //TODO  upper left corner coord.(X,Y) , width, height. (pixels)
                        }
                ,
                ActionSetsKey = "beforeCreviceRemark"

            });

            #endregion


            /*
            #region  Area trigger Past bridge

            list.Add(new GlobalConditionalEvent()
            {
                KeyName = "TUTORIAL_triggerPastBridge",
                ConditionSet = new ConditionSet()
                {
                    Value = new Condition()
                    {
                        AreaCondition = new AreaCondition()
                        {
                            Area = new Rectangle(2832, 3312, 240, 144)  // upper left corner coord.(X,Y) , width, height. (pixels)
                        }
                    }
                }
                ,
                ActionSetsKey = "pastBridgeRemark"]

            });

            #endregion
*/



            // gathered 3 oil tubers:

            list.Add(new PolledEventType()
            {
                KeyName = "TUTORIAL_3commonOilTubers",  // 
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
                                    ConstantStringEqual = "item:commonOilTubers"
                                }
                            }
                        },
                        ListCondition = new ListCondition()
                        {
                            CountMinimum = new ValueNode()
                            {
                                Decimal = 3.0f
                            }
                        }

                    },


                ActionSetsKey = "3commonOilTubers"

            });


            list.Add(new PolledEventType()
            {
                KeyName = "TUTORIAL_checkcommonOilTubersGatheredDelay",  // looks for a property that has been set some time after the commonOilTubers were gathered..so we get a delay. MP
                PollInterval = new ValueNode() { Decimal = 1f },  // seconds
                AllowRandomTimeOffset = true,
                Condition = new CustomCondition()
                {
                    AllowWhileAllPlayerMembersAreSleepingOrCollapsed = false,
                    AllowWhilePlayerThreatened = false,  // player allegiance threatened
                    AllowWhilePlayerMemberIsFighting = false,
                    PropertyCondition = new PropertyCondition() { PropertyKey = "commonOilTubersGatheredDelay", BoolValue = true }

                },
                ActionSetsKey = "commonOilTubersGatheredDelayEvent" // 
            });




            #region build fireplace make food




            list.Add(new PolledEventType()
            {
                KeyName = "TUTORIAL_ReadyToBuildCampfire",
                PollInterval = new ValueNode() { Decimal = 1f },  //  sec. .......(there's 1600 sec / day)  
                AllowRandomTimeOffset = true,
                Condition = new ConditionFunction()
                    {
                        Operator = OperatorType.And,
                        Left = new CustomCondition()
                                {

                                    TargetObject = new TargetObject()
                                    {
                                        GetList = new GetList()
                                        {
                                            HasPropertiesListKey = "finishedEntities", // "entities",
                                            FilterCondition = new PropertyCondition()
                                            {
                                                PropertyKey = "type",
                                                ConstantStringEqual = "item:stones"
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
                        Right = new CustomCondition()
                        {

                            TargetObject = new TargetObject()
                            {
                                GetList = new GetList()
                                {
                                    HasPropertiesListKey = "finishedEntities", //"entities",
                                    FilterCondition = new PropertyCondition()
                                    {
                                        PropertyKey = "type",
                                        ConstantStringEqual = "item:firewood"
                                    }
                                }
                            },
                            ListCondition = new ListCondition()
                            {
                                CountMinimum = new ValueNode()
                                {
                                    Decimal = 3.0f
                                }
                            }

                        }

                    },

                ActionSetsKey = "readyToBuildCampfire"

            });




            list.Add(new PolledEventType()
            {
                KeyName = "TUTORIAL_ReadyToCookcommonOilTubers",
                PollInterval = new ValueNode() { Decimal = 1f },  //  sec. .......(there's 1600 sec / day)  
                AllowRandomTimeOffset = true,
                Condition = new ConditionFunction()
                    {
                        Operator = OperatorType.And,
                        Left = new CustomCondition()
                        {

                            TargetObject = new TargetObject()
                            {
                                GetList = new GetList()
                                {
                                    HasPropertiesListKey = "finishedEntities",
                                    FilterCondition = new PropertyCondition()
                                    {
                                        PropertyKey = "type",
                                        ConstantStringEqual = "item:commonOilTubers" // does not validate spelling!!! //if this is a structure, then event will fire as soon as you click "BUILD", that is , even before the building ghost image is placed, the building is counted as part of the colony...
                                    }
                                }
                            },
                            ListCondition = new ListCondition()
                            {
                                CountMinimum = new ValueNode()
                                {
                                    Decimal = 3.0f
                                }
                            }

                        },
                        Right = new CustomCondition()
                        {
                            PropertyCondition = new PropertyCondition()
                            {
                                PropertyKey = "campfireFinished",
                                BoolValue = true
                            }

                        }

                    },

                ActionSetsKey = "readyToCookcommonOilTubers"

            });


            #endregion


            #region  Area trigger -detect bush dragon backup (in case they dont detect it)

            list.Add(new PolledEventType()
            {
                KeyName = "TUTORIAL_triggerDetectBushDragonBackup",
                UseDefaultPollInterval = true,
                AllowRandomTimeOffset = true,
                Condition = new AreaCondition()
                        {
                            Area = new Rectangle(3168, 2640, 192, 144)  //TODO  upper left corner coord.(X,Y) , width, height. (pixels)
                        }
                ,
                ActionSetsKey = "detectBushDragon" // triggers the same event as detect bush dragon. but it can only fire once.

            });

            #endregion




            #region make 3 spears


             list.Add(new PolledEventType()
            {
                KeyName = "TUTORIAL_3Flint3WaterCaneStems",
                PollInterval = new ValueNode() { Decimal = 1f },  //  sec. .......(there's 1600 sec / day)  
                AllowRandomTimeOffset = true,
                Condition = new ConditionFunction()
                    {
                        Operator = OperatorType.And,
                        Left = new CustomCondition()
                        {
                            TargetObject = new TargetObject()
                            {
                                GetList = new GetList()
                                {
                                    HasPropertiesListKey = "finishedEntities",
                                    FilterCondition = new PropertyCondition()
                                    {
                                        PropertyKey = "type",
                                        ConstantStringEqual = "item:flintRough"
                                    }
                                }
                            },
                            ListCondition = new ListCondition()
                            {
                                CountMinimum = new ValueNode()
                                {
                                    Decimal = 3.0f
                                }
                            }

                        },
                        Right = new CustomCondition()
                        {

                            TargetObject = new TargetObject()
                            {
                                GetList = new GetList()
                                {
                                    HasPropertiesListKey = "finishedEntities",
                                    FilterCondition = new PropertyCondition()
                                    {
                                        PropertyKey = "type",
                                        ConstantStringEqual = "item:waterCaneStem"
                                    }
                                }
                            },
                            ListCondition = new ListCondition()
                            {
                                CountMinimum = new ValueNode()
                                {
                                    Decimal = 3.0f
                                }
                            }
                        }
                    },

                ActionSetsKey = "readyToMakeSpears"

            });






           /* list.Add(new PolledEventType()
            {
                KeyName = "TUTORIAL_3Spearhead3WaterCaneStems",
                PollInterval = new ValueNode() { Decimal = 1f },  //  sec. .......(there's 1600 sec / day)  
                AllowRandomTimeOffset = true,
                Condition = new ConditionFunction()
                    {
                        Operator = OperatorType.And,
                        Left = new CustomCondition()
                        {

                            TargetObject = new TargetObject()
                            {
                                GetList = new GetList()
                                {
                                    HasPropertiesListKey = "finishedEntities",
                                    FilterCondition = new PropertyCondition()
                                    {
                                        PropertyKey = "type",
                                        ConstantStringEqual = "item:flintSpearhead"
                                    }
                                }
                            },
                            ListCondition = new ListCondition()
                            {
                                CountMinimum = new ValueNode()
                                {
                                    Decimal = 3.0f
                                }
                            }

                        },
                        Right = new CustomCondition()
                        {

                            TargetObject = new TargetObject()
                            {
                                GetList = new GetList()
                                {
                                    HasPropertiesListKey = "finishedEntities",
                                    FilterCondition = new PropertyCondition()
                                    {
                                        PropertyKey = "type",
                                        ConstantStringEqual = "item:waterCaneStem"
                                    }
                                }
                            },
                            ListCondition = new ListCondition()
                            {
                                CountMinimum = new ValueNode()
                                {
                                    Decimal = 3.0f
                                }
                            }
                        }
                    },

                ActionSetsKey = "readyToMakeSpearheads"

            }); */

            list.Add(new PolledEventType()
             {
                 KeyName = "TUTORIAL_3SpearsFinished",  // 
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
                                     ConstantStringEqual = "item:improvisedFlintSpear"
                                 }
                             }
                         },
                         ListCondition = new ListCondition()
                         {
                             CountMinimum = new ValueNode()
                             {
                                 Decimal = 3.0f
                             }
                         }
                     },

                 ActionSetsKey = "3SpearsFinished"

             });

            /*              ActionSets = new ActionSets()
                          {
                              FireMode = ActionSetsToFire.AllValid,
                              ChanceToFire = 1f,
                              SetsOfActions = new []{ 


                                   new ActionSetType("9f789999999433333333333333333333399999ac")
                                   {      
                          //              ChanceToFire = 0.1f,
                                        MaxFirings = 1,
                                        Condition = new ConditionSet()
                                       {
                                            Value = new Condition()
                                            {
                                          SiteAllegianceMembers = new PlayerAllegiancePersons(){ MinMembers = 3, MaxMembers = 3 }
                                         }
                                         }, 
                                         Actions = new EventActionType[]{new EventActionType("62647984eyrrrrrrrrrrrrrrrryerye7867864acd") {  
                              
                                          TalkPriority = TalkAction.TalkActionPriority.High,
                                          CanTalkWhileFighting = false,
                                          CanTalkWhileSleeping = false,
                                          CanTalkWhileThreatened = false,
                                          SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
                       //                   ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                          DefaultText = "Wait, what is that...out there? You think we 3 are gonna get rescued?" , 
                                          }}, 
          */


            #endregion



            #region  Area trigger Before bush dragon fight



            list.Add(new PolledEventType()
            {
                KeyName = "TUTORIAL_triggerBeforeBushDragonFight", //triggers when they reach the entrance and have 3 spears.
                UseDefaultPollInterval = true,
                AllowRandomTimeOffset = true,
                Condition = new ConditionFunction()
                    {
                        Operator = OperatorType.And,
                        Left = new AreaCondition()
                         {
                             Area = new Rectangle(3168, 2544, 192, 192)  //(3264, 2544, 144, 144) upper left corner coord.(X,Y) , width, height. (pixels)

                         },

                        Right = new CustomCondition()
                         {
                             TargetObject = new TargetObject()
                             {
                                 GetList = new GetList()
                                 {
                                     HasPropertiesListKey = "finishedEntities",
                                     FilterCondition = new PropertyCondition()
                                     {
                                         PropertyKey = "type",
                                         ConstantStringEqual = "item:improvisedFlintSpear" // does not validate spelling!!! //if this is a structure, then event will fire as soon as you click "BUILD", that is , even before the building ghost image is placed, the building is counted as part of the colony...
                                     }
                                 }
                             },
                             ListCondition = new ListCondition()
                             {
                                 CountMinimum = new ValueNode()
                                 {
                                     Decimal = 3.0f
                                 }
                             }
                         }
                    },

                ActionSetsKey = "readyToAttackBushDragons"

            });

            #endregion

            #region  bush dragons killed
            list.Add(new PolledEventType()
            {
                Comment = "triggers when they have 3 dragons killed, condition:  kill all 'guards' in north allegiance. the south 'scout' I destroyed by a destroyentity event..",
                KeyName = "TUTORIAL_triggerAfterBushDragonFight",
                PollInterval = new ValueNode() { Decimal = 1f },  // seconds
                StartTimePoint = new TimePoint() { RelativeTimeInSeconds = new ValueNode() { Decimal = 5f } },
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
                                HasPropertiesListKey = "allegiances",
                                FilterCondition = new PropertyCondition()
                                {
                                    PropertyKey = "keyName",
                                    ConstantStringEqual = "bushDragonTutAllegianceNorth"
                                },
                                NextList = new GetList()
                                {
                                    HasPropertiesListKey = "members"
                                }
                            }
                        },
                        ListCondition = new ListCondition()
                        {
                            CountEqual = 0,   //CountMinimum = 1,
                        }

                    },
                ActionSetsKey = "afterBushDragonFight"

            });


            /*
                        //old version with trigger area. i removed the area condition because it's a needless complication i think.
                        list.Add(new GlobalConditionalEvent() //poll instead of area
                        {
                            KeyName = "TUTORIAL_triggerAfterBushDragonFight", //triggers when they are at the forest area and have 3 dragons killed. (bushies can die from starvation?  area condition)
                            ConditionSet = new ConditionSet()
                            {

                                Function = new ConditionFunction()
                                {
                                    Operator = OperatorType.And,
                                    Left = new ConditionSet()
                                    {
                                        Value = new Condition()
                                        {
                                            AreaCondition = new AreaCondition()
                                            {
                                                Area = new Rectangle(3360, 2208, 672, 480)  //14*10   tiles. // upper left corner coord.(X,Y) , width, height. (pixels)
                                            }
                                        }
                                    },

                                    Right = new ConditionSet()
                                    {
                                        Value = new Condition()
                                        {
                                            CustomCondition = new CustomCondition()
                                            {
                                                //condition:  kill all "guards" in north allegiance. the south "scout" i destroyed by a destroyentity event..
                                                TargetObject = new TargetObject()
                                                {
                                                    GetList = new GetList()
                                                    {
                                                        HasPropertiesListKey = "allegiances",
                                                        FilterCondition = new PropertyCondition()
                                                        {
                                                            PropertyKey = "keyName",
                                                            StringEqual = "bushDragonTutAllegianceNorth"
                                                        },
                                                        NextList = new GetList()
                                                        {
                                                            HasPropertiesListKey = "members"
                                                        }
                                                    }
                                                },
                                                ListCondition = new ListCondition()
                                                {
                                                    CountEqual = 0,   //CountMinimum = 1,
                                                }
                                            }

                                        }
                                    },
                                }
                            },

                            ActionSetsKey = "afterBushDragonFight"]

                        });
            */


            ////




            #endregion


            #region rescue

            string endGameTooltip = "Fast forward till we are rescued! (Clicking here will end the game and take you to the end screen.)"; //"Rescued! Let's get to that boat! (Clicking here will end your current game and take you to the main menu.)"        "You have succeeded: At least one explorer has survived long enough to get rescued. Clicking here will end your current game and take you to the main menu."
            string continueGameTooltip = "Continue playing till we are rescued. (Clicking here will continue the game - there may never be a rescue, though!)"; // "There's something odd about that boat. Let's say no thanks and send them on their way...(Clicking here will make you continue playing.)"       "You have achieved the goal of the scenario, but you can keep on playing by selecting this option - you will NOT get rescued again however!"


            //// 3 people left: (they are immortal so always the case.)

            list.Add(new PolledEventType()
            {
                KeyName = "TUTORIAL_Rescue_3Left",  // looks for a property that has been set a little while after the signal pyre was lit. MP
                PollInterval = new ValueNode() { Decimal = 10f },  // seconds
                AllowRandomTimeOffset = true,
                StartTimePoint = new TimePoint() { RelativeTimeInSeconds = new ValueNode() { Decimal = 2 } },
                Condition = new CustomCondition()
                    {
                        AllowWhileAllPlayerMembersAreSleepingOrCollapsed = false,
                        AllowWhilePlayerThreatened = false,  // player allegiance threatened
                        AllowWhilePlayerMemberIsFighting = false,
                        PropertyCondition = new PropertyCondition() { PropertyKey = "signalPyreLit", BoolValue = true }

                    },
                ActionSets = new ActionSets()
                {
                    FireMode = ActionSetsToFire.AllValid,
                    ChanceToFire = 1f,
                    SetsOfActions = new []{ 


                    new ActionSetType("9f789999999999999999999999999999999999ac")
                    {      
        //              ChanceToFire = 0.1f,
                        MaxFirings = 1,
                        Condition = new PlayerAllegiancePersons(){ MinMembers = 3, MaxMembers = 3 }
                        , 
                        Actions = new EventActionType[]{
                            new TalkAction("626479844444444478984677867864acd") {  
                              
                        TalkPriority = TalkAction.TalkActionPriority.High,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = false,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
        //                   ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                        DefaultText = "Wait, what is that...out there? You think we've been seen already?" , 
                        }, 
 

                  new WinGameAction("b21e3567567567567567567567567567567567567dba")
                      { 
                          Comments = "this is a modal event screen dialog on the game area",
                          DelayInSeconds = 4, 
                       
                            // this can be null. then the modal dialog will be skipped:
                            ModalDialogText = new DynamicText(){ Text = "SCOYD: Nah...not a boat. Looks like a flock of glowbirds. \nHARRON: Well, maybe next time. Let's keep the pyre burning from now on. Until a boat passes by, we should be able to survive here. \nSANTILLA:  Sure hope we don't have to wait too long.",
                                },
                            ModalDialogImage = "GroupMeeting",

                            WinScreenText = "SHIP'S LOG: The Starhawk \nDATE: 11-11 2264 \nTEMP: 16 C; WIND: SSW; WEATHER: SUNNY; \n \nAt 13:20 we spotted a smoke signal coming from the normally uninhabited Brightburn Island, went ashore and found 3 stranded sailors from Noame. They were in good health considering they had been stranded for several months. \nWe will take them back to Noame once we complete our round trip.",
                                                  
                            EndGameTooltip = endGameTooltip,
                          //  ContinueGameTooltip = continueGameTooltip, 
                            AllowContinueGame = false,

                            ContinueActions = new ActionSets()
                            {
                                SetsOfActions = new ActionSetType[]
                                {
                                    new ActionSetType("c28346786786786786786786786786786786786785b0")
                                    {
                                        Actions = new EventActionType[]
                                        {
                                            new SetPropertyAction("146786786988888888888888888775416")
                                            {
                                                
                                                    PropertyKey = "gameOver", 
                                                    Value = new ValueNode() { Bool = true }
                                                
                                            }
                                        }
                                    }
                                }
                            }
                          
                        
                        

                },
                                                           
                   new MusicAction("dfryuuuuuuuuuuujfxdgjhjdghf4")
                   { DelayInSeconds = 4, //has to be the same as for wingameaction, so that the music will fire on end game screen.
                    
                            Song = "Martin Hasseldam - A New World (Alt3) 320kBit" 
                        
                }
  
                               }
                            },
                    
 
                            }
                }
            });



            #endregion


            return list;
        }
    }
}
