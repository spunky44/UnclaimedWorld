using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.InGameEvents;
using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.ClientSide.GameEvents;
using UWGame.SimSide.Maps.MapEditor;
using Microsoft.Xna.Framework;
using UWGame.SimSide.InGameEvents.PropertyObjects;
using UWGame.Client.Particles;
using UWGame.SimSide.InGameEvents.Expressions;
using UWGame.SimSide.Overland;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Entities.Biological;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_6.Data
{
    public class ActionSetsLoader
    {

        public static List<ActionSets> Init()
        {
            List<ActionSets> list = new List<ActionSets>();


            #region General eating talk
            list.Add(new ActionSets()
            {
                KeyName = "humanEatingRemark",
                //  once the remark actionset is set to fire, it chooses a line from the below which each have an equal chance of being selected
                FireMode = ActionSetsToFire.RandomValid,
                ChanceToFire = 1f, //the chance for the set to fire, then chooses from below
                SetsOfActions = new []
                { 
                    new ActionSetType("64838843-dd71-44bb-8f87-5134406ed247")
                    {      
                        MaxFirings = 1,
                        Condition = new PlayerAllegiancePersons(){ MinMembers = 2 }                              
                               , 
                               Actions = new EventActionType[]{new TalkAction("339f1a3d-e540-439f-9ae5-654b31ca69f4") {  
                              
                                TalkPriority = TalkAction.TalkActionPriority.Low,
                                CanTalkWhileFighting = false,
                                CanTalkWhileSleeping = false,
                                CanTalkWhileThreatened = false,
                                TurnTowardsListeners = false,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
                                ActionByAgent = ActionByAgent.OnlyTriggeringEntity, //
                                DefaultText = "I'm so hungry!" }, 

                                new TalkAction("25a58cdc-156c-4826-8f42-c6bdfa8a8e4b") { DelayInSeconds = 2.5, 
                     
                        TalkPriority = TalkAction.TalkActionPriority.Low,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = false,
                        TurnTowardsListeners = false,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.First, 
                        ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                        DefaultText = "This is good." }},
                        }
                            

    
        
                     
                    }

            });

            #endregion

            #region Immigrants talk
            list.Add(new ActionSets()
            {
                KeyName = "newPlayerAllegianceMemberRemark",
                //  once the remark actionset is set to fire, it chooses a line from the below which each have an equal chance of being selected
                FireMode = ActionSetsToFire.RandomValid,
                ChanceToFire = 1f, //the chance for the set to fire, then chooses from below
                SetsOfActions = new []{ new ActionSetType("1df39063-8504-afds325456uyjip-ggsefs3-a65c34756a91")
                        {    
                       //     ChanceToFire = 0.5f,
                            MaxFirings = 1,  
                        Condition = new PlayerAllegiancePersons(){ MinMembers = 2 }                            
                            , 
                       Actions = new EventActionType[]{new TalkAction("de0c6aa-dd5dsdwasdwad-a098-e3c68a3150a3") {  
                     
                        TalkPriority = TalkAction.TalkActionPriority.Normal,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = false,
                        TurnTowardsListeners = true,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.First, 
                        ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                        DefaultText = "Thanks for letting me join! Hope I can be of use!" }, //

                    new TalkAction("fecaff265439c-56c4-41gsh4475-8204-7f38adfagdsa3b0878") { DelayInSeconds = 3, 
                     
                        TalkPriority = TalkAction.TalkActionPriority.Normal,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = false,
                        TurnTowardsListeners = false,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.Second, 
                        ActionByAgent = ActionByAgent.RandomInAllegiance,
                        CanTalk = new CustomCondition(){
                                TargetObject = new TargetObject() { TargetObjectType = TargetObjectType.TriggeringEntity },
                                //important for immigrant talk:
                                PropertyCondition = new PropertyCondition() { PropertyKey = "origin",  ConstantStringEqual = "playSite" }                                                  
                                            }, 
                        DefaultText = "Welcome to the clay pit!" },
                        
                       }
                        },
                  new ActionSetType("0af71asfd32545yupp-sdfqcxb-9c47-de1f2fdcb2c2")
                         {      
                             Condition = new PlayerAllegiancePersons(){ MinMembers = 2 }                               
                               ,
                          //    ChanceToFire = 0.1f,      
                               Actions = new EventActionType[]{new TalkAction("3729afs2354uopp4e48-9445-39d4d79c959f") {  
                              
                                TalkPriority = TalkAction.TalkActionPriority.Normal,
                                CanTalkWhileFighting = false,
                                CanTalkWhileSleeping = false,
                                CanTalkWhileThreatened = false,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
                                TurnTowardsListeners = true,
                                ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                DefaultText = "Hello everyone. I'm glad to be part of this crew!" }, 
                        
                            new TalkAction("9c098af32534trhjuiopfdwwxxad48-fb385d2a5a3a") {  
                                    
                                DelayInSeconds = 3, 
                              
                                TalkPriority = TalkAction.TalkActionPriority.Normal,
                                CanTalkWhileFighting = false,
                                CanTalkWhileSleeping = false,
                                CanTalkWhileThreatened = false,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.Second,  
                                ActionByAgent = ActionByAgent.RandomInAllegiance,
                                CanTalk = new CustomCondition(){
                                TargetObject = new TargetObject() { TargetObjectType = TargetObjectType.TriggeringEntity },
                                //important for immigrant talk:
                                PropertyCondition = new PropertyCondition() { PropertyKey = "origin",  ConstantStringEqual = "playSite" }                                                  
                                            }, 
                                DefaultText = "Glad to have you here!" }, 
                        }
                            }    
                    }

            });
            #endregion

           
          
                       

            #region place territory lock
            list.Add(new ActionSets()
            {
                // 
                KeyName = "placeTerritoryLockFenceEvent",
                SetsOfActions = new []{ new ActionSetType("3c89f1dgjdhdadetghjey340db9")
                        {     
                                                 
                            Actions = new EventActionType[]{

                            new SpawnEntityAction("37e4a4eghhhhhhhhhhhhffgfgffgc5238ef9a") { DelayInSeconds = 1,
                            EntityData = new EntityData()
                            { EntityKey = "terrain:terrainBlockerWide", Name = "territoryLockClaypit",  Location = new Vector3(1560, 1370, 0)   
                            //on top of abatis to prevent salvage, HAS TO HAVE A GAP between the blocker and the structures, else the accesspoint will just be extended waaay south of the structure, providing access on the far side of the terrain blocker.
                                }}


                            }
                        }
                    }
            });


            #endregion

            #region Detect creatures


            list.Add(new ActionSets()
            {
                KeyName = "detectWhiteThunderChicken",
                SetsOfActions = new []{ new ActionSetType("a5232bf4-6764-4d5c-be7b-0f0d1e4b2a55")
                        {     
                             Condition = new PlayerAllegiancePersons(){ MinMembers = 2 }
                             , 
                        MaxFirings = 1,
                        Actions = new EventActionType[]{ new TalkAction("c1618c98-c8d3-4e19-9e82-1115dc6d4507") {   
                        DelayInSeconds = 0,
                          
                        TalkPriority = TalkAction.TalkActionPriority.Low,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = false,
                        ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                        DefaultText = "I could eat one of those thunder chickens." }, 

                    new TalkAction("f3f4209f-a0a5-4019-bb29-dadc47271ee0") { DelayInSeconds = 2, 
                     
                        TalkPriority = TalkAction.TalkActionPriority.Low,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = false,
                        ActionByAgent = ActionByAgent.RandomInAllegiance,
                        DefaultText = "Anyone up for some hunting?" }}
                    }
                    }

            });


            list.Add(new ActionSets() //talkAction 
            {
                FireMode = ActionSetsToFire.RandomValid,
                KeyName = "detectPatrician",//
                SetsOfActions = new []
                {
                    #region actionSet#1
                    new ActionSetType("sxdza5fsghgfsjghsjdgjdg0f0d1e4b2a55")
                    {
                        Condition = new PlayerAllegiancePersons(){ MinMembers = 2 }
                        ,
                        MaxFirings = 1,
                        Actions = new EventActionType[]
                        {
                            new TalkAction("c1618c9setyuyeuee19-9e82-1115dc6d4507")
                            {
                                DelayInSeconds = 0,
                               
                                    TalkPriority = TalkAction.TalkActionPriority.Normal,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                                    CanTalkWhileFighting = true,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = true,
                                    ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                    DefaultText = "Uh-oh. Patricans!" 
                                
                            },
                            new TalkAction("f3f4209f-a0a5-4eyutyu34tdc47271ee0")
                            {
                                DelayInSeconds = 2.5,
                                
                                    TalkPriority = TalkAction.TalkActionPriority.Normal,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                                    CanTalkWhileFighting = true,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = true,
                                    ActionByAgent = ActionByAgent.RandomInAllegiance,
                                    DefaultText = "If they've seen us, we might have to fight them!" //because colonists 'home in' on them
                                
                            }
                        }
                    },
                    #endregion
                    #region actionSet#2
                    new ActionSetType("a5232bf4tryjdgddkhfjkdhfjkfhjk4bzsae2a55")
                    {
                        Condition = new PlayerAllegiancePersons(){ MinMembers = 2 }
                        ,
                        MaxFirings = 1,
                        Actions = new EventActionType[]
                        {
                            new TalkAction("c1618c9sdfg8-c8tyuyeye15dcze6d4507")
                            {
                                DelayInSeconds = 0,
                               
                                    TalkPriority = TalkAction.TalkActionPriority.Normal,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                                    CanTalkWhileFighting = true,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = true,
                                    ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                    DefaultText = "The patricians are still here!" 
                                
                            },
                            new TalkAction("f3f42retyua34tdcjhkldfghgfhdgfjd47271ee0")
                            {
                                DelayInSeconds = 2.5,
                                
                                    TalkPriority = TalkAction.TalkActionPriority.Normal,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                                    CanTalkWhileFighting = true,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = true,
                                    ActionByAgent = ActionByAgent.RandomInAllegiance,
                                    DefaultText = "We might need to take them on!"  //because colonists 'home in' on them
                                
                            }
                        }
                    }
                    #endregion
                }
            });


            list.Add(new ActionSets() //talkAction 
            {
                FireMode = ActionSetsToFire.RandomValid,
                KeyName = "detectMudWorm",//
                SetsOfActions = new []
                {
                    #region actionSet#1
                    new ActionSetType("sxdza523dfghjdhjhdghjdhd1e4b2a55")
                    {
                        Condition = new PlayerAllegiancePersons(){ MinMembers = 2 }
                        ,
                        MaxFirings = 1,
                        Actions = new EventActionType[]
                        {
                            new TalkAction("c161dghjdhdfsjsfgjfsgjsfgj6d4507")
                            {
                                DelayInSeconds = 0,
                               
                                    TalkPriority = TalkAction.TalkActionPriority.Normal,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                                    CanTalkWhileFighting = true,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = true,
                                    ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                    DefaultText = "Ugh, where do these worms come from!" 
                                
                            },
                            new TalkAction("f3fasdfqw-etypxzxzxz-a0a5-4019-bb29-da34tdc47271ee0")
                            {
                                DelayInSeconds = 2.5,
                                
                                    TalkPriority = TalkAction.TalkActionPriority.Normal,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                                    CanTalkWhileFighting = true,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = true,
                                    ActionByAgent = ActionByAgent.RandomInAllegiance,
                                    DefaultText = "They come out of the ground if you step on their tunnels." 
                                
                            }
                        }
                    },
                    #endregion
                    #region actionSet#2
                    new ActionSetType("a5232bf4-6764-4d5csdf-be7b-0f0d1e4bzs-asf25453-5oippae2a55")
                    {
                        Condition = new PlayerAllegiancePersons(){ MinMembers = 2 }
                        ,
                        MaxFirings = 1,
                        Actions = new EventActionType[]
                        {
                            new TalkAction("asdc1618c9sdfg8-c8d3-4e19-9efs82-1115dcze6dgg4507")
                            {
                                DelayInSeconds = 0,
                               
                                    TalkPriority = TalkAction.TalkActionPriority.Normal,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                                    CanTalkWhileFighting = true,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = true,
                                    ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                    DefaultText = "Mud worms are coming out of the ground here." 
                                
                            },
                   /*         new EventActionType("f3f42asf235op900-bb29-da34tdc47271ee0")
                            {
                                DelayInSeconds = 2.5,
                                
                                    TalkPriority = TalkAction.TalkActionPriority.Normal,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                                    CanTalkWhileFighting = true,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = true,
                                    ActionByAgent = ActionByAgent.RandomInAllegiance,
                                    DefaultText = "Try not to disturb their nests." 
                                }
                            },
                            new EventActionType("f3f4209rtyutyuirtyuic47271ee0")
                            {
                                DelayInSeconds = 4.5,
                                
                                    TalkPriority = TalkAction.TalkActionPriority.Normal,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.Third,
                                    CanTalkWhileFighting = true,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = true,
                                    ActionByAgent = ActionByAgent.RandomInAllegiance,
                                    DefaultText = "We need to kill those worms. They attract patricians." 
                                }
                            }
                            */
                        }
                    }
                    #endregion
                }
            });



            list.Add(new ActionSets()
            {
                KeyName = "detectPygmyThunderChicken",
                SetsOfActions = new []{ new ActionSetType("bce52303-d312-4556-b578-1f1dec8dds5555abd")
                        {     
                             Condition = new PlayerAllegiancePersons(){ MinMembers = 2 }
                             ,  
                        MaxFirings = 1,
                        Actions = new EventActionType[]{ new TalkAction("9df62c8c-5a7e-47b2-b41b-05693esedrg3e542ab9d4") {   
                        DelayInSeconds = 0,
                          
                        TalkPriority = TalkAction.TalkActionPriority.Low,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = false,
                        ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                        DefaultText = "There's some good game here." }} 



                        
                    }
                    }

            });







            #endregion

            #region at the fence trigger

            list.Add(new ActionSets()
            {
                KeyName = "atFenceEvent",

                ChanceToFire = 1f, //the chance for the set to fire, then chooses from below
                SetsOfActions = new []{ new ActionSetType("5a6etyututyuyutyuyueyueyuuebfe33")
                    {    
                        MaxFirings = 1,
                         Condition = new CustomCondition()
                         {
                              PropertyCondition = new PropertyCondition()
                              {
                                    PropertyKey = "storyPartOver", // similar to gameOver in twinkler island... 
                                   BoolValue = false
                              }

                         },

                        
                    
                        Actions = new EventActionType[]
                        {

                      new EventActionDialog("53253675637sd56378563876487648644f4e51") 
                      { DelayInSeconds = 0, 
                            DisplayText = new DynamicText(){ Text = " \n-Hey, don't go near the fence! We put that up for a reason you know. \n-I don't see any patricians behind it. \n-Trust me, they know we're here. So stay away, OK?" ,  //  
                            },
                            DisplayImage = "GroupMeeting",
                           
                      },
                        new TalkAction("ab94etyu56356735afaf3252wy7u5e6uetyueyuyurt9a")
                        {
                                DelayInSeconds = 2,
                                       
                                    TalkPriority = TalkAction.TalkActionPriority.High,
                                    CanTalkWhileFighting = false,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = true,
                                    TurnTowardsListeners = true,
                                    ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.First,    
                                    DefaultText = "Yeah, yeah I get it... Don't touch the fence..." // 
                                        
                        }
                    }
                }
                

                
            }
            });


            #endregion

            #region CLAYPIT //tutorial screens

            #region constructKilnFinishedEvent
            // 
            list.Add(new ActionSets()
            {
                KeyName = "constructKilnFinishedEvent",
                SetsOfActions = new []{ new ActionSetType("d4b0c097-f997-4111-94d3-28b4ba8d26c02")
                     {                     
                        MaxFirings = 1, 
                        Condition = new ConditionFunction()
                        {
                            Left = new PlayerAllegiancePersons(){ MinMembers = 2 },                             
                            Operator = OperatorType.And,
                            Right = new CustomCondition()
                            {
                                PropertyCondition = new PropertyCondition() { PropertyKey = "tutorialOn", BoolValue = true } //so that it doesn't fire in no tutorial-mode.  (this event comes from eventhooksloader, not like the other screens, that's why this is needed.)
                            }   
                        },                         
                       Actions = new EventActionType[]{
                        new TalkAction("0ae24413-11a8-4fc3-a3f9-ce2e0c016b952") {  
                     
                        TalkPriority = TalkAction.TalkActionPriority.High,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = false,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.First, 
                        ActionByAgent = ActionByAgent.RandomInAllegiance, //cannot use OnlyTriggering or Prefertriggering. no suitable agent found.. because its called from globalconditionaleventsloader. needs to be done through eventhooks for that.
                        DefaultText = "Kiln is ready!" }, // 


                    new EventActionDialog("tyudujdgeeeeeeeeetyuetyutyetuityuidtytyhdde4f4e512") { DelayInSeconds = 3, 
                        DisplayText = new DynamicText(){ Text = "-Now, mudbricks can of course dry in the sun but firing them in the kiln is quicker. So let's see if we can keep the kiln burning round the clock, huh? Let's fire a batch of 3 mudbricks. Start by digging more clay and gathering firewood. \n-To speed up things, someone should make some more brick molds. \n \n- // Fire 3X Mudbricks //" ,  //  
                        },
                        DisplayImage = "GroupMeeting",
                         DialogOptions = new[] { new DialogOption()
                         {
                            Text = "GUIDE #2", Tooltip = "See how to carry out this decision (opens separate window.)", ActiveInArchive = true,
                            ActionSet = "showTutorialClayScenario6_2" 
                         }}}                  
                               

                    }

                        }
                     }
            });
            #endregion

            



            #region 3 mudbricks made:  "makeSolidMudBrickFinishedEvent"
            // 
            list.Add(new ActionSets()
            {
                KeyName = "makeSolidMudBrickFinishedEvent",
                SetsOfActions = new []{ new ActionSetType("d4b0c097-dtyjudtysj6e8b4ba8d26c02")
                     {                     
                        MaxFirings = 1, 
                        Condition = new ConditionFunction()
                        {
                            Left = new PlayerAllegiancePersons(){ MinMembers = 2 },                             
                            Operator = OperatorType.And,
                            Right = new CustomCondition()
                            {
                                PropertyCondition = new PropertyCondition() { PropertyKey = "tutorialOn", BoolValue = true } //so that it doesn't fire in no tutorial-mode.  (this event comes from eventhooksloader, not like the other screens, that's why this is needed.)
                            }   
                        },                         
                       Actions = new EventActionType[]{
                        new TalkAction("0ae244135e6uey5rrrrrrrrrrrrdfusd016b952") {  
                     
                        TalkPriority = TalkAction.TalkActionPriority.High,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = false,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.First, 
                        ActionByAgent = ActionByAgent.RandomInAllegiance, //cannot use OnlyTriggering or Prefertriggering. no suitable agent found.. because its called from globalconditionaleventsloader. needs to be done through eventhooks for that.
                        DefaultText = "Got 3 mudbricks now." }, // 


                        new EventActionDialog("53sgfhhhghjktyrkutejitdesyjurstyuhrstghfghfhg51") { DelayInSeconds = 3, // 4, 
                        DisplayText = new DynamicText(){ Text = "Right, that's the first batch from the kiln... Get those mudbricks over to the port and let's continue working. I'm sure one more kiln would speed things up. Each time we have 40 mudbricks we'll signal the barge to come and get those in exchange for food. \nThe quicker we get this done, the quicker we can get a bite to eat. \n \n- // Place Mudbricks in port. Stock 40X Mudbricks // -", 
                        },
                        DisplayImage = "GroupMeeting",

                       
                         DialogOptions = new[] { new DialogOption()
                         {
                            Text = "GUIDE #3", Tooltip = "See how to carry out this decision (opens separate window.)", ActiveInArchive = true,
                            ActionSet = "showTutorialClayScenario6_3" 
                         }}}
                      
                                                         
                        }
                    }
                    }
            });
            #endregion





            #region 40mudBricks

            list.Add(new ActionSets()
            {
                KeyName = "40mudBricks",
                SetsOfActions = new []{ new ActionSetType("51xxdfsgdfsghdfshadgfhdfasgdafgdafgdfad88b")
                        {     
                             Condition = new PlayerAllegiancePersons(){ MinMembers = 2 }
                             , 
                        MaxFirings = 1,
                        Actions = new EventActionType[]{ new TalkAction("9xxsgfdhsdfggsdfdghfshgfghfsghsgshghghs08") {   
                        DelayInSeconds = 0,
                          
                        ActionByAgent = ActionByAgent.RandomInAllegiance, //cannot use OnlyTriggering or Prefertriggering. no suitable agent found.. because its called from globalconditionaleventsloader. needs to be done through eventhooks for that.
                        TalkPriority = TalkAction.TalkActionPriority.High,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = true,

                        DefaultText = "We got 40 mudbricks now!" },


                    new SetPropertyAction("671fcsyghfdgggggggggggggggggggfhhgfshssgfhedbbb")
                    {  
                        DelayInSeconds = 62f, // // sets a propertykey after xf seconds, to trigger the next event screen
                       
                            PropertyKey = "40MudBricksStoredDelay", //   
                            Value = new ValueNode() { Bool = true }
                                                    
                                  
                    },

                        new EventActionDialog("53sgfhhhhhhhhhhhhhhhhhhhghfghfghfhsdg51") { DelayInSeconds = 2, // 4, 
                        DisplayText = new DynamicText(){ Text = "Alright. As soon as we got 40 mudbricks in the port, we'll signal the barge to come and pick them up. In exchange, we should buy as many provisions as we can afford. Let's take the cheap oil tubers, none of that fancy food! \n \n- // Sell Mudbricks. Buy Oil tubers. // -"  
                        },
                        DisplayImage = "GroupMeeting",

                        
                         DialogOptions = new[] { new DialogOption()
                         {
                            Text = "GUIDE #4", Tooltip = "See how to carry out this decision (opens separate window.)", ActiveInArchive = true,
                            ActionSet = "showTutorialClayScenario6_4" 
                         }}}                      
                                                     
                        }
                    }
                    }
            });
            #endregion

            #region make more mudbricks delay 

            list.Add(new ActionSets()
            {
                KeyName = "makeMoreMudBricksDelayEvent", // triggered some time after 40 mudbricks made
                SetsOfActions = new []{ 
                    
                    new ActionSetType("51xgfshssssssssssssssssssgddwhffghgfhhgf9d88b")
                        {     
                         
                        MaxFirings = 1,
                        Actions = new EventActionType[]{ 

                        new EventActionDialog("53z5367777777777777787565656565656dwd56565656e4f4e51") 
                        { DelayInSeconds = 4, 
                        DisplayText = new DynamicText(){ Text = "-I see a lot of people standing around. Hop to it! We need to make and sell as many mudbricks as possible. \n \n- // Continue making and trading mudbricks // -"
                        },
                        DisplayImage = "GroupMeeting",
                         DialogOptions = new[] { new DialogOption()
                         {
                            Text = "GUIDE #5", Tooltip = "See how to carry out this decision (will open another window)", ActiveInArchive = true,
                            ActionSet = "showTutorialClayScenario6_5" 
                         }}}
                      
                                                        
                        }
                    }
                    }
            });
            #endregion

            #region "continueGameEvent"  continue play after tutorial
            list.Add(new ActionSets()
            {
                KeyName = "continueGameEvent", // triggered right after player chooses to continue game.
                FireMode = ActionSetsToFire.AllValid, //fires all of the banter, then the screen
                SetsOfActions = new []{ new ActionSetType("51xxdfsgdryhjddgdafgdgggfad88b")
                        {     
                             Condition = new PlayerAllegiancePersons(){ MinMembers = 2 }
                             , 
                        MaxFirings = 1,
                        Actions = new EventActionType[]{ new TalkAction("9xwrtyuregjgfshjeyydtjdtsyjshghghs08") {   
                        DelayInSeconds = 0,
                          
                        ActionByAgent = ActionByAgent.RandomInAllegiance, //cannot use OnlyTriggering or Prefertriggering. no suitable agent found.. because its called from globalconditionaleventsloader. needs to be done through eventhooks for that.
                        TalkPriority = TalkAction.TalkActionPriority.High,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = true,

                        DefaultText = "I'm in." },

                        new TalkAction("9xwrtywrtyrtwyjeyydtjdtsyjshghghs08") {   
                        DelayInSeconds = 2,
                          
                        ActionByAgent = ActionByAgent.RandomInAllegiance, //cannot use OnlyTriggering or Prefertriggering. no suitable agent found.. because its called from globalconditionaleventsloader. needs to be done through eventhooks for that.
                        TalkPriority = TalkAction.TalkActionPriority.High,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = true,

                        DefaultText = "Me too." },

                        new TalkAction("9xxsgfdhsddghjerwtyrtwurywuwrtyghghs08") {   
                        DelayInSeconds = 3,
                          
                        ActionByAgent = ActionByAgent.RandomInAllegiance, //cannot use OnlyTriggering or Prefertriggering. no suitable agent found.. because its called from globalconditionaleventsloader. needs to be done through eventhooks for that.
                        TalkPriority = TalkAction.TalkActionPriority.High,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.Third,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = true,

                        DefaultText = "I also want to stay." },


                        new EventActionDialog("53sgewrrtewyrtwyhfshfghfhg51") { DelayInSeconds = 6,  
                        DisplayText = new DynamicText(){ Text = "-We all want to stay! That's great! \n-Now, as you know, the contract is over, so all of you are NOW FREE TO LEAVE if you don't like it here. Ok, who has a plan in mind? \n-I have a suggestion: \n \n-First, we break down the abatis and go have a look. Everyone should be ready to patrol the river bank. \nWhen we've chased away any patricians, we'll examine the river banks and set up fish weirs. With some smoke ovens running, we should be ok with food as we start digging bog ore in the marsh and set up a smithy here. \n-Yep. And whatever extra things we need, we can buy from Tellus. \n-Let's do it. \n \n- // 'Salvage' the abatis. Build up a settlement. // -",

                        },
                        DisplayImage = "GroupMeeting",
                         DialogOptions = new[] { new DialogOption()
                         {
                            Text = "GUIDE #6", Tooltip = "See how to carry out this decision (opens separate window.)", ActiveInArchive = true,
                            ActionSet = "showTutorialClayScenario6_6" 
                         }}
                      
                               },                           
                        }
                    }
                    }
            });

            #endregion



            #endregion

            #region //Tutorial windows (scenario 6)  REDIRECTION.

            /////   //tutorial texts are in BaseData\BaseDataLoader.cs///////

            list.Add(new ActionSets()
            {
                // opens the tut window when the dialog button is clicked
                KeyName = "showTutorialClayScenario6_1",
                SetsOfActions = new []{ new ActionSetType("645teyujtey7teytjtjdgj8sd2b")
                {                                 
                    Actions = new EventActionType[]{ new ShowTutorialAction("01f2dghjtytejtujktdejdtjtyjdgsdfgd7d2f30") 
                    {   
                        
                            TutorialPageKey = "tutorialClayScenario6_1"
                                                                        
                    }
                }
                }
            }
            });

            list.Add(new ActionSets()
            {
                // opens the tut window when the dialog button is clicked
                KeyName = "showTutorialClayScenario6_2",
                SetsOfActions = new []{ new ActionSetType("645teyujtey7teytasdjtjdgj8sd2b")
                {                                 
                    Actions = new EventActionType[]{ new ShowTutorialAction("01f2dghjtytejtujktdejdsadtjtyjdgsdfgd7d2f30") 
                    {   
                        
                            TutorialPageKey = "tutorialClayScenario6_2"
                                                                        
                    }
                }
                }
            }
            });

            list.Add(new ActionSets()
            {
                // opens the tut window when the dialog button is clicked
                KeyName = "showTutorialClayScenario6_3",
                SetsOfActions = new []{ new ActionSetType("645teyujtey7tesdasytasdjtjdgj8sd2b")
                {                                 
                    Actions = new EventActionType[]{ new ShowTutorialAction("01f2dghjtytejtujdwktdejdsadtjtyjdgsdfgd7d2f30") 
                    {   
                        
                            TutorialPageKey = "tutorialClayScenario6_3"
                                                                        
                    }
                }
                }
            }
            });

            list.Add(new ActionSets()
            {
                // opens the tut window when the dialog button is clicked
                KeyName = "showTutorialClayScenario6_4",
                SetsOfActions = new []{ new ActionSetType("645wteyujtey7teytaawasdjtjdgj8sd2b")
                {                                 
                    Actions = new EventActionType[]{ new ShowTutorialAction("01f2dghjtytejtsdfujktdejdsadtjtyjdgsdfgd7d2f30") 
                    {   
                        
                            TutorialPageKey = "tutorialClayScenario6_4"
                                                                     
                    }
                }
                }
            }
            });

            list.Add(new ActionSets()
            {
                // opens the tut window when the dialog button is clicked
                KeyName = "showTutorialClayScenario6_5",
                SetsOfActions = new []{ new ActionSetType("645teyujtey7teytasdjtsadjdgj8sd2b")
                {                                 
                    Actions = new EventActionType[]{ new ShowTutorialAction("01f2dghjtytejtujktdejdsadtjsdsdtyjdgsdfgd7d2f30") 
                    {   
                        
                            TutorialPageKey = "tutorialClayScenario6_5"
                                                                       
                    }
                }
                }
            }
            });

            list.Add(new ActionSets()
            {
                // opens the tut window when the dialog button is clicked
                KeyName = "showTutorialClayScenario6_6",
                SetsOfActions = new []{ new ActionSetType("645teyu546756y7uwrtyurtywuwryuwsd2b")
                {                                 
                    Actions = new EventActionType[]{ new ShowTutorialAction("01f2dghjtrwtysrtyhrt6rtsyrtydtyjdgsdfgd7d2f30") 
                    {   
                        
                            TutorialPageKey = "tutorialClayScenario6_6"
                                                                      
                    }
                }
                }
            }
            });

            #endregion 
   
          

            #region spawn Mud worms triggered by trigger zones

            list.Add(new ActionSets()
            {
                KeyName = "spawnMudWormsWest",
                SetsOfActions = new []{ new ActionSetType("ea39rsgthysrthnjes5y7tyrshsrfhfsgha3933")
                        {     
                     /*          //  NO Nest condition for spawning
                        Condition = new CustomCondition()
                             { 
                                 TargetObject = new TargetObject()
                                 {
                                     GetList = new GetList() { HasPropertiesListKey = "entities", 
                                          FilterCondition = new PropertyCondition() { PropertyKey = "name", ConstantStringEqual = "Quadite nest (coord. 15;34)"}
                                     }
                                         //FilterProperty = "name", FilterValue = } //southSandstoneCaveNest
                                 }, 
                                 ListCondition = new ListCondition(){ CountEqual = 1 } 
                               },*/
                        
                            MaxFirings = 1, //important if you don't want a massive stream of worms. else, maybe use popcap to limit them and bigger poll interval??
                            Actions = new EventActionType[]{    


                        new SpawnEntityAction("1e5kmjnbfhvchuxchcxjhjsqwererppjsrtyureseyurw79b9") { DelayInSeconds = 2,
                        EntityData = new EntityData()
                            { EntityKey = "entity:mudWorm", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "mudWormAllegiance#1" },
                                Location = new Vector3(778, 772, 0), BioEntity = new Maps.MapEditor.BiologicalEntity() { AgeGroup = AIAgeGroup.Adult }  }},

                        new SpawnEntityAction("1e5kmvhbhgdhgdetrppejjhdgzxxzzgehjsrtyureseyurw79b9") { DelayInSeconds = 3,
                        EntityData = new EntityData()
                            { EntityKey = "entity:mudWorm", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "mudWormAllegiance#1" },
                                Location = new Vector3(800, 780, 0), BioEntity = new Maps.MapEditor.BiologicalEntity() { AgeGroup = AIAgeGroup.Adult }  }},

                        new SpawnEntityAction("1e5dtgfjuhy56euuurghsfgseyurw79b9") { DelayInSeconds = 4,
                        EntityData = new EntityData()
                            { EntityKey = "entity:mudWorm", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "mudWormAllegiance#1" },
                                Location = new Vector3(800, 780, 0), BioEntity = new Maps.MapEditor.BiologicalEntity() { AgeGroup = AIAgeGroup.Adult }  }},

                        new SpawnEntityAction("1e5dtgfjuhy56esfghsfghjsrtyureseyurw79b9") { DelayInSeconds = 2,
                        EntityData = new EntityData()
                            { EntityKey = "entity:mudWorm", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "mudWormAllegiance#1" },
                                Location = new Vector3(816, 816, 0), BioEntity = new Maps.MapEditor.BiologicalEntity() { AgeGroup = AIAgeGroup.Adult }  }},

                        new SpawnEntityAction("1e5dtsfghgfs56euuutdgehjsrtyureseyurw79b9") { DelayInSeconds = 1,
                        EntityData = new EntityData()
                            { EntityKey = "entity:mudWorm", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "mudWormAllegiance#1" },
                                Location = new Vector3(816, 816, 0), BioEntity = new Maps.MapEditor.BiologicalEntity() { AgeGroup = AIAgeGroup.Adult }  }},

                        new SpawnEntityAction("1e5dtgsfghgfsh56euuutdgehjsrtyureseyurw79b9") { DelayInSeconds = 2,
                        EntityData = new EntityData()
                            { EntityKey = "entity:mudWorm", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "mudWormAllegiance#1" },
                                Location = new Vector3(864, 836, 0), BioEntity = new Maps.MapEditor.BiologicalEntity() { AgeGroup = AIAgeGroup.Adult }  }},

                        new SpawnEntityAction("1e5dtgfjghkdhfjkhfj6euuutdgehjsrtyureseyurw79b9") { DelayInSeconds = 2.5,
                        EntityData = new EntityData()
                            { EntityKey = "entity:mudWorm", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "mudWormAllegiance#1" },
                                Location = new Vector3(864, 836, 0), BioEntity = new Maps.MapEditor.BiologicalEntity() { AgeGroup = AIAgeGroup.Adult }  }},

                            }}}
            }

 );



            list.Add(new ActionSets()
            {
                KeyName = "spawnMudWormsCenter",
                SetsOfActions = new []{ new ActionSetType("ea39rsgthysrthdrghjdfgjdjhsrfhfsgha3933")
                        {     
                     /*          //  NO Nest condition for spawning
                        Condition = new CustomCondition()
                             { 
                                 TargetObject = new TargetObject()
                                 {
                                     GetList = new GetList() { HasPropertiesListKey = "entities", 
                                          FilterCondition = new PropertyCondition() { PropertyKey = "name", ConstantStringEqual = "Quadite nest (coord. 15;34)"}
                                     }
                                         //FilterProperty = "name", FilterValue = } //southSandstoneCaveNest
                                 }, 
                                 ListCondition = new ListCondition(){ CountEqual = 1 } 
                               },*/
                        
                            MaxFirings = 1, //important if you don't want a massive stream of worms. else, maybe use popcap to limit them and bigger poll interval??
                            Actions = new EventActionType[]{    


                        new SpawnEntityAction("1e5ddjdw79hjklhjlhjlhjb9") { DelayInSeconds = 2,
                        EntityData = new EntityData()
                            { EntityKey = "entity:mudWorm", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "mudWormAllegiance#1" },
                                Location = new Vector3(2037, 1809, 0), BioEntity = new Maps.MapEditor.BiologicalEntity() { AgeGroup = AIAgeGroup.Adult }  }},

                        new SpawnEntityAction("1e5dtgfjuhy56dghjdgjdtyureseyurw79b9") { DelayInSeconds = 3,
                        EntityData = new EntityData()
                            { EntityKey = "entity:mudWorm", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "mudWormAllegiance#1" },
                                Location = new Vector3(2037, 1809, 0), BioEntity = new Maps.MapEditor.BiologicalEntity() { AgeGroup = AIAgeGroup.Adult }  }},

                        new SpawnEntityAction("1e5dtgdrsy6thrustyhu6ryrghsfgseyurw79b9") { DelayInSeconds = 4,
                        EntityData = new EntityData()
                            { EntityKey = "entity:mudWorm", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "mudWormAllegiance#1" },
                                Location = new Vector3(2037, 1809, 0), BioEntity = new Maps.MapEditor.BiologicalEntity() { AgeGroup = AIAgeGroup.Adult }  }},

                        new SpawnEntityAction("1e5dtgfjuhy56esfghsoir7r867oreseyurw79b9") { DelayInSeconds = 2,
                        EntityData = new EntityData()
                            { EntityKey = "entity:mudWorm", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "mudWormAllegiance#1" },
                                Location = new Vector3(2037, 1809, 0), BioEntity = new Maps.MapEditor.BiologicalEntity() { AgeGroup = AIAgeGroup.Adult }  }},

                        new SpawnEntityAction("1e5dtsfryuiryuirehjsryuiryui79b9") { DelayInSeconds = 1,
                        EntityData = new EntityData()
                            { EntityKey = "entity:mudWorm", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "mudWormAllegiance#1" },
                                Location = new Vector3(2037, 1809, 0), BioEntity = new Maps.MapEditor.BiologicalEntity() { AgeGroup = AIAgeGroup.Adult }  }},

                        new SpawnEntityAction("1e5dtsfryurtyuetyutui79b9") { DelayInSeconds = 1,
                        EntityData = new EntityData()
                            { EntityKey = "entity:mudWorm", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "mudWormAllegiance#1" },
                                Location = new Vector3(2037, 1809, 0), BioEntity = new Maps.MapEditor.BiologicalEntity() { AgeGroup = AIAgeGroup.Adult } }, },
                            }}}
            }

 );



            #endregion


          
            return list;
         
        }
    }
}
