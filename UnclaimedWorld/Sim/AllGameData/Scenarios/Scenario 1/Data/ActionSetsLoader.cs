using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.Client.Particles;
using UWGame.ClientSide.GameEvents;
using UWGame.SimSide.InGameEvents;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.SimSide.InGameEvents.Expressions;
using UWGame.SimSide.InGameEvents.PropertyObjects;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.XmlCollections;
using UWGame.SimSide.Entities.Biological;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_1.Data
{
    public class ActionSetsLoader
    {

        public static List<ActionSets> Init()
        {
            List<ActionSets> list = new List<ActionSets>();


            #region Farms and fishs (old)
            /* //put all theplots and fishtraps you wanna spawn in here and call it from the globalConditionalEventLoader for an easier readable display... stuff you know what I mean
            list.Add(new ActionSets()
            {
                KeyName = "ActuallySpawnAllTheFishAndFarms", //"southSandstoneCaveNest"
                FireMode = ActionSetsToFire.AllValid,
                SetsOfActions = new []
                {
                    new ActionSetType("e3c657c7-a3ff-476a-b319-a3e9948960f9")
                    {
                        Actions = new EventActionType[]
                        {
                            new EventActionType("960b3344-1913-4d1f-93c8-88508b43dd59")
                            {
                                DelayInSeconds = 1,
                                
                                    EntityData = new EntityData()
                                    {
                                        EntityKey = "terrain:smallPlotSpot",
                                        Name = "South/East spot",
                                        Location = new Vector3(2496f, 2640f, 0)
                                    },   
                                },
                            }
                        }
                    },
                    
                    new ActionSetType("c3632b40-3613-457e-afee-77f8bbacfd5f")
                    {
                        Actions = new EventActionType[]
                        {
                            new EventActionType("a68e8546-581a-4a5f-8186-71ed914793e5")
                            {
                                DelayInSeconds = 1,
                                
                                    EntityData = new EntityData()
                                    {
                                        EntityKey = "terrain:smallPlotSpot",
                                        Name = "South/East spot",
                                        Location = new Vector3(2496f, 2600f, 0)
                                    },   
                                },
                            }
                        }
                    }
                }
            });
            */
            #endregion


            //In twinkler island, they are PRECOL so they speak more scientific and they also are more desperate towards enemy animals because they are crashlanded and alone..also, their enemies are twinklers, specifically.

            #region Fish and fishtrap talk
            #region Finding the fish trap spot

            /*
             * the first set will only be spoaken once per game, the 2nd is used for every other trap that is found
             */
            list.Add(new ActionSets() //bso TODO: move to baseData if posible
            {
                KeyName = "detectFishTrapCoastRemark",
                FireMode = ActionSetsToFire.FirstValid,
                ChanceToFire = 1f, //the chance for the set to fire, then chooses from below
                SetsOfActions = new []
                { 
                    new ActionSetType("c23ab9fea96-9686awaf-48afaaefdf-970e-cde6e4456h8bea2")
                    {
                        MaxFirings = 1,
                        Condition = new ConditionFunction()
                        {
                            Left = new CustomCondition()
                            {
                                PropertyCondition = new PropertyCondition()
                                {
                                    PropertyKey = "spotted",
                                    BoolValue = false
                                }
                                
                            },
                            Operator = OperatorType.And,
                            Right = new PlayerAllegiancePersons(){ MinMembers = 2 }                            
                            
                        },
                        Actions = new EventActionType[]
                        {
                            new TalkAction("8dc3ca94-b352asf7bc-4afaf57ef-9f36-59ddsf322asfa1503gs5bf32")
                            {
                                DelayInSeconds = 0,
                                
                                    CanTalkWhileFighting = false, 
                                    CanTalkWhileThreatened = false, 
                                    CanTalkWhileSleeping = false,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                                    ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                    TalkPriority = TalkAction.TalkActionPriority.Normal,
                                    DefaultText = "Plenty of streak fin in this water."
                                
                            },
                            new TalkAction("35asf3252515eeae-9dfa88-4b4235e-ad2s5tsb6f02")
                            {
                                DelayInSeconds = 4,
                                
                                    CanTalkWhileFighting = false,
                                    CanTalkWhileThreatened = false,
                                    CanTalkWhileSleeping = false,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                                    ActionByAgent = ActionByAgent.RandomInAllegiance,
                                    TalkPriority = TalkAction.TalkActionPriority.Normal,
                                    DefaultText = "Yeah? Maybe we could set up a fish trap."
                                
                            },
                            new SetPropertyAction("csad3252fd88-8eb9-43ed-a949-25356564ggdsaaa8c871ac")
                            {
                                DelayInSeconds = 0f,
                                
                                    TargetObject = new TargetObject(){TargetObjectType = TargetObjectType.TargetEntity},
                                    PropertyKey = "spotted",
                                    Value = new ValueNode(){Bool = true}
                                
                            }
                        }
                    },
                    new ActionSetType("26b31db3-63fds322352578-413d-afbe-34s5ts506e197") //bso there is no maxfiring on this set, so use this for the rest
                    {
                        Condition = new ConditionFunction()
                        {
                            Left = new CustomCondition()
                            {
                                PropertyCondition = new PropertyCondition()
                                {
                                    PropertyKey = "spotted",
                                    BoolValue = false
                                }                                
                            },
                            Operator = OperatorType.And,
                            Right = new PlayerAllegiancePersons(){ MinMembers = 2 }                                
                            
                        },
                        Actions = new EventActionType[]
                        {
                            new SetPropertyAction("sdad3sad46ffiopb88-8eb9-43ed-a949-2120a8c871ac")
                            {
                                DelayInSeconds = 0f,
                                
                                    TargetObject = new TargetObject(){TargetObjectType = TargetObjectType.TargetEntity},
                                    PropertyKey = "spotted",
                                    Value = new ValueNode(){Bool = true}
                                
                            },
                            new TalkAction("174a99e8-0a2e-asf3252462ys5-5578c95422d9")
                            {
                                DelayInSeconds = 0,
                                
                                    CanTalkWhileFighting = false,
                                    CanTalkWhileThreatened = false,
                                    CanTalkWhileSleeping = false,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                                    ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                    TalkPriority = TalkAction.TalkActionPriority.Low,
                                    DefaultText = "More streak fin. Good spot for a fish trap."
                                
                            },

                        }
                    },
                }
            });

            
             // the first set will only be spoaken once per game, the 2nd is used for every other trap that is found
             
            list.Add(new ActionSets() 
            {
                KeyName = "detectFishTrapShoreRemark",
                FireMode = ActionSetsToFire.FirstValid,
                ChanceToFire = 1f, //the chance for the set to fire, then chooses from below
                SetsOfActions = new []{ 
                            new ActionSetType("c23ab9saf96-9623fa86-48dafaf4ff-970e-cde6e4456h8bea2")
                            {   MaxFirings = 1,
                                Condition = new ConditionFunction()
                                {
                                    Left = new CustomCondition()
                                    {
                                        PropertyCondition = new PropertyCondition()
                                        {
                                            PropertyKey = "spotted",
                                            BoolValue = false
                                        }                                            
                                    },
                                    Operator = OperatorType.And,
                                    Right = new PlayerAllegiancePersons(){ MinMembers = 2 }                                        
                                    
                                },
                                Actions = new EventActionType[]{ 
                                    new TalkAction("saf32528dc3ca94-b2357bc-47easff-9f33256-591503gs5bf32") 
                                    { 
                                        DelayInSeconds = 0,
                                         
                                            CanTalkWhileFighting = false, CanTalkWhileThreatened = false, CanTalkWhileSleeping = false,
                                            SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                                            ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                            TalkPriority = TalkAction.TalkActionPriority.Normal,
                                            DefaultText = "I see carbon tail. We could set up a fish trap here."
                                        
                                    },
                                    new TalkAction("3515easfaeae-9d5325aagh88-4b4e-ad2sadh5tsb6f02") 
                                    { 
                                        DelayInSeconds = 4,
                                         
                                            CanTalkWhileFighting = false, CanTalkWhileThreatened = false, CanTalkWhileSleeping = false,
                                            SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                                            ActionByAgent = ActionByAgent.RandomInAllegiance,
                                            TalkPriority = TalkAction.TalkActionPriority.Normal,
                                            DefaultText = "That's worth considering." 
                                        
                                    },
                                    new SetPropertyAction("c01sa411677dsf688428-8ebsdaw5-43ed-a949-2120a8c871ac")
                                    {
                                        DelayInSeconds = 0f,
                                        
                                            TargetObject = new TargetObject(){TargetObjectType = TargetObjectType.TargetEntity},
                                            PropertyKey = "spotted",
                                            Value = new ValueNode(){Bool = true}
                                        
                                    },
                                }                    
                            },
                  
                            new ActionSetType("26b3fas1db3-6378-413d-afbe-34s5ts506e197") //bso there is no maxfiring on this set, so use this for the rest
                            { 
                                Condition = new ConditionFunction()
                                {
                                    Left = new CustomCondition()
                                    {
                                        PropertyCondition = new PropertyCondition()
                                        {
                                            PropertyKey = "spotted",
                                            BoolValue = false
                                        }                                            
                                    },
                                    Operator = OperatorType.And,
                                    Right = new PlayerAllegiancePersons(){ MinMembers = 2 }                                        
                                    
                                },                 
                                Actions = new EventActionType[]
                                { 
                                    new TalkAction("174a99e8-0aafs252e-462ys5-5572358c95422d9") 
                                    { 
                                        DelayInSeconds = 0,
                                         
                                            CanTalkWhileFighting = false, CanTalkWhileThreatened = false, CanTalkWhileSleeping = false,
                                            SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                                            ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                            TalkPriority = TalkAction.TalkActionPriority.Low,
                                            DefaultText = "Lots of carbon tail here...another good place for a fish trap." 
                                        
                                    },

                                    new SetPropertyAction("csdadw4676ipplsad4yy88-8eb9-43ed-a949-2120a8c871ac")
                                    {
                                        DelayInSeconds = 0f,
                                        
                                            TargetObject = new TargetObject(){TargetObjectType = TargetObjectType.TargetEntity},
                                            PropertyKey = "spotted",
                                            Value = new ValueNode(){Bool = true}
                                        
                                    },
                                }  
                            }  
                }
            });
            #endregion
            #endregion

         

            #region General Combat Talk


            #region human hit enemy
            list.Add(new ActionSets()
            {
                KeyName = "humanHitEnemyRemark", // general ... once the remark actionset is set to fire, it chooses a line from the below which each have an equal chance of being selected
                FireMode = ActionSetsToFire.RandomValid,
                ChanceToFire = 0.15f, //the chance for the set to fire, then chooses from below

                SetsOfActions = new []{

                        new ActionSetType("4ffc2386-3cfd-40c7-bf8c-2665addaabc4")
                         {      
                         //     ChanceToFire = 0.5f,                              
                              Condition = new PlayerAllegiancePersons(){ MinMembers = 1 }                               
                               , 
                               Actions = new EventActionType[]{new TalkAction("55ace4e1-b6dd-4b5d-9b0f-f36153ce0614") {  
                                
                                TalkPriority = TalkAction.TalkActionPriority.Low,
                                 CanTalkWhileFighting = true,
                                 CanTalkWhileSleeping = false,
                                 CanTalkWhileThreatened = true,
                                 TurnTowardsListeners = false,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
                                ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                DefaultText = "I'm gonna squash you!" }, 
                        }
                            }, 
 

                        new ActionSetType("ad0daf9e-4eac-47e5-891c-c6f1f8ba4871")
                         {      
                        //      ChanceToFire = 0.5f,                              
                              Condition = new PlayerAllegiancePersons(){ MinMembers = 1 }                               
                               , 
                               Actions = new EventActionType[]{new TalkAction("01f30fa2-5749-4989-90a4-0d016b6eab8d") {  
                               
                                TalkPriority = TalkAction.TalkActionPriority.Low,
                                 CanTalkWhileFighting = true,
                                 CanTalkWhileSleeping = false,
                                 CanTalkWhileThreatened = true,
                                 TurnTowardsListeners = false,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
                                ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                DefaultText = "How'd you like that?" }, 
                        }
                            }, 
                            

                        // one left. when hitting enemy:


                         new ActionSetType("befe68b2-0038-467b-936e-2f45239300e1")
                         {      
                       //       ChanceToFire = 0.2f,                              
                              Condition = new PlayerAllegiancePersons(){ MinMembers = 1, MaxMembers = 1 }                               
                               , 
                               Actions = new EventActionType[]{new TalkAction("6c90eb07-cfcd-4444-b6c9-590869506da8") {  
                               
                                TalkPriority = TalkAction.TalkActionPriority.Low,
                                 CanTalkWhileFighting = true,
                                 CanTalkWhileSleeping = false,
                                 CanTalkWhileThreatened = true,
                                 TurnTowardsListeners = false,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
                                ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                DefaultText = "Get lost you ugly creep!" }, 
                        }
                            }, 

                          new ActionSetType("7871eca9-e8e4-4dfa-94a0-dcf7afd947ae")
                         {      
                        //      ChanceToFire = 0.2f,                              
                              Condition = new PlayerAllegiancePersons(){ MinMembers = 1, MaxMembers = 1 }                               
                               , 
                               Actions = new EventActionType[]{new TalkAction("ca274308-b524-4346-ae6b-2c1738a7bc9e") {  
                               
                                TalkPriority = TalkAction.TalkActionPriority.Low,
                                 CanTalkWhileFighting = true,
                                 CanTalkWhileSleeping = false,
                                 CanTalkWhileThreatened = true,
                                 TurnTowardsListeners = false,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
                                ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                DefaultText = "Leave me alone you beast!" }, 
                        }
                            }, 

                         new ActionSetType("acdb72d9-9983-4967-b98b-ee34b478f517")
                         {      
                       //       ChanceToFire = 0.2f,                              
                              Condition = new PlayerAllegiancePersons(){ MinMembers = 1, MaxMembers = 1 }                               
                               , 
                               Actions = new EventActionType[]{new TalkAction("44925708-a104-45bc-8fa2-f859277cbe9c") {  
                               
                                TalkPriority = TalkAction.TalkActionPriority.Low,
                                 CanTalkWhileFighting = true,
                                 CanTalkWhileSleeping = false,
                                 CanTalkWhileThreatened = true,
                                 TurnTowardsListeners = false,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
                                ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                DefaultText = "I'm so sick of you monsters." }, 
                        }
                            }, 

                         new ActionSetType("9b91243e-5615-4e02-9860-1d3b04972cae")
                         {      
                      //        ChanceToFire = 0.2f,                              
                              Condition = new PlayerAllegiancePersons(){ MinMembers = 1, MaxMembers = 1 }                               
                               , 
                               Actions = new EventActionType[]{new TalkAction("5ecd9d13-c703-4e66-b4aa-4de32293f50f") {  
                               
                                TalkPriority = TalkAction.TalkActionPriority.Low,
                                 CanTalkWhileFighting = true,
                                 CanTalkWhileSleeping = false,
                                 CanTalkWhileThreatened = true,
                                 TurnTowardsListeners = false,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
                                ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                DefaultText = "UGLY UGLY REVOLTING BEAST!!" }, 
                        }
                            }, 


                         new ActionSetType("8aba886b-9cd7-4ed8-ada7-9062b7fe5db7")
                         {      
                        //      ChanceToFire = 0.2f,                              
                              Condition = new PlayerAllegiancePersons(){ MinMembers = 1, MaxMembers = 1 }                               
                               , 
                               Actions = new EventActionType[]{new TalkAction("63821522-6709-4536-a824-daca80f51fbc") {  
                               
                                TalkPriority = TalkAction.TalkActionPriority.Low,
                                 CanTalkWhileFighting = true,
                                 CanTalkWhileSleeping = false,
                                 CanTalkWhileThreatened = true,
                                 TurnTowardsListeners = false,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
                                ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                DefaultText = "Get lost. Find another source of protein." }, 
                        }
                            },

                         new ActionSetType("2e35d0b3-f30e-4f49-9af8-cc57a5518d19")
                         {      
                   //           ChanceToFire = 0.2f,                              
                              Condition = new PlayerAllegiancePersons(){ MinMembers = 1, MaxMembers = 1 }                               
                               , 
                               Actions = new EventActionType[]{new TalkAction("d41d7c02-c555-4848-ab50-e771b4c34300") {  
                               
                                TalkPriority = TalkAction.TalkActionPriority.Low,
                                 CanTalkWhileFighting = true,
                                 CanTalkWhileSleeping = false,
                                 CanTalkWhileThreatened = true,
                                 TurnTowardsListeners = false,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
                                ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                DefaultText = "I don't like your species." }, 
                        }
                            },

                       new ActionSetType("77fed9a5-9e72-435a-beb5-55f0d02458f5")
                         {      
                      //        ChanceToFire = 0.5f,                              
                              Condition = new PlayerAllegiancePersons(){ MinMembers = 1, MaxMembers = 1 }                               
                               , 
                               Actions = new EventActionType[]{new TalkAction("2f29c7cb-d684-45b8-8966-24d0fba8cb49") {  
                               
                                TalkPriority = TalkAction.TalkActionPriority.Low,
                                 CanTalkWhileFighting = true,
                                 CanTalkWhileSleeping = false,
                                 CanTalkWhileThreatened = true,
                                 TurnTowardsListeners = false,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
                                ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                DefaultText = "You. You again." }, 
                        }
                            },

                        new ActionSetType("680c1ddc-c0c0-4fd4-b1f5-ae4839e9c0f9")
                         {      
                     //         ChanceToFire = 0.1f,                              
                              Condition = new PlayerAllegiancePersons(){ MinMembers = 1, MaxMembers = 1 }                               
                               , 
                               Actions = new EventActionType[]{new TalkAction("037f33d3-5dae-44b3-8d33-e8e2a1dad176") {  
                               
                                TalkPriority = TalkAction.TalkActionPriority.Low,
                                 CanTalkWhileFighting = true,
                                 CanTalkWhileSleeping = false,
                                 CanTalkWhileThreatened = true,
                                 TurnTowardsListeners = false,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
                                ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                DefaultText = "You eerie beauty. I'm gonna crush you." }, 
                        }
                            },

                         new ActionSetType("f989c3c1-f58c-40ff-8a5d-136f82c251af")
                         {      
                     //         ChanceToFire = 0.1f,                              
                              Condition = new PlayerAllegiancePersons(){ MinMembers = 1, MaxMembers = 1 }                               
                               , 
                               Actions = new EventActionType[]{new TalkAction("2451587f-b2d9-4497-ad32-ff77f7964259") {  
                               
                                TalkPriority = TalkAction.TalkActionPriority.Low,
                                 CanTalkWhileFighting = true,
                                 CanTalkWhileSleeping = false,
                                 CanTalkWhileThreatened = true,
                                 TurnTowardsListeners = false,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
                                ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                DefaultText = "I'm gonna make you into furniture." }, 
                        }
                            },

                       new ActionSetType("604c9d90-9ed0-4966-a564-4150232f8703")
                         {      
                    //          ChanceToFire = 0.1f,                              
                              Condition = new PlayerAllegiancePersons(){ MinMembers = 1, MaxMembers = 1 }                               
                               , 
                               Actions = new EventActionType[]{new TalkAction("9fde08ee-1dbd-405c-9e11-624f999a8688") {  
                               
                                TalkPriority = TalkAction.TalkActionPriority.Low,
                                 CanTalkWhileFighting = true,
                                 CanTalkWhileSleeping = false,
                                 CanTalkWhileThreatened = true,
                                 TurnTowardsListeners = false,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
                                ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                DefaultText = "Stop with the blinking!" }, 
                        }
                            },



                    }

            });

            #endregion

            #region human killed enemy remark

            list.Add(new ActionSets()
            {
                KeyName = "humanKilledEnemyRemark", // ... once the remark actionset is set to fire, it chooses a line from the below which each have an equal chance of being selected
                FireMode = ActionSetsToFire.RandomValid,
                ChanceToFire = 0.5f, //the chance for the set to fire, then chooses from below
                SetsOfActions = new []
                {
                    new ActionSetType("7a9640d7-9e5f-4546-8822-d035a31de3b2")
                    {
                        Condition = new PlayerAllegiancePersons(){ MinMembers = 1 }
                        ,
                        Actions = new EventActionType[]
                        {
                            new TalkAction("49eab31e-470a-4e6e-b77e-9464fb31e13d")
                            {
                                 
                                    TalkPriority = TalkAction.TalkActionPriority.Low,
                                    CanTalkWhileFighting = true,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = true,
                                    TurnTowardsListeners = false,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
                                    ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                    DefaultText = "DIE!!!" 
                                
                            } 
                        }
                    }
                }
            });
            #endregion

            #region human killed in combat remark
//mp I put some radio style talk in here
            list.Add(new ActionSets()
            {
                KeyName = "humanKilledInCombatRemark", // ... once the remark actionset is set to fire, it chooses a line from the below which each have an equal chance of being selected
                FireMode = ActionSetsToFire.RandomValid,
                ChanceToFire = 1f, //the chance for the set to fire, then chooses from below

                SetsOfActions = new []{ 
                        new ActionSetType("584120c4-3fa5-4a55-9a65-cbbe71c6874a")
                         {      
                          //    ChanceToFire = 0.1f,                              
                         Condition = new PlayerAllegiancePersons(){ MinMembers = 2 }                          
                          , 
                        Actions = new EventActionType[]{new TalkAction("656f7911-718c-4142-a793-4b46b62bce15") {  
                        
                        TalkPriority = TalkAction.TalkActionPriority.High,
                        CanTalkWhileFighting = true,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = true,
                        TurnTowardsListeners = false,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
                                ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                DefaultText = "..Hnnngrlll..." }, 

                      new TalkAction("2f152b7c-6f65-444b-a8db-cef47aa2891f") { DelayInSeconds = 2, 
                     
                        TalkPriority = TalkAction.TalkActionPriority.High,
                        CanTalkWhileFighting = true,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = true,
                        TurnTowardsListeners = false,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.Second, 
                        ActionByAgent = ActionByAgent.RandomInAllegiance,
                        DefaultText = "Hey! Where are you?! I'm losing you!! Come back!!" },
                        }
                            }    
                        ,
                       new ActionSetType("80e2d817-d6bb-4fc4-baf2-03aff2bc2c49")
                         {      
                          //    ChanceToFire = 0.1f,                              
                              Condition = new PlayerAllegiancePersons(){ MinMembers = 3 }                               
                               , 
                               Actions = new EventActionType[]{new TalkAction("eff32ad3-d68a-4ef0-9b6e-e4f1b744b766") {  
                               
                                TalkPriority = TalkAction.TalkActionPriority.High,
                        CanTalkWhileFighting = true,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = true,
                        TurnTowardsListeners = false,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
                                ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                DefaultText = "..HGHKRRRLL.." }, 

                      new TalkAction("72ead9c0-5aae-43f0-aa1c-5a74688fc3cc") { DelayInSeconds = 2, 
                     
                        TalkPriority = TalkAction.TalkActionPriority.High,
                        CanTalkWhileFighting = true,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = true,
                        TurnTowardsListeners = false,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.Second, 
                        ActionByAgent = ActionByAgent.RandomInAllegiance,
                        DefaultText = "What's happening?! We're losing you! Hang in there!" },
                        }
                            }    
                        ,
 
                    }

            });
            #endregion

            #region human fleeing remark

            list.Add(new ActionSets()
            {
                KeyName = "humanFleeingRemark", // ... once the remark actionset is set to fire, it chooses a line from the below which each have an equal chance of being selected
                FireMode = ActionSetsToFire.RandomValid,
                ChanceToFire = 0.85f, //the chance for the set to fire, then chooses from below


                SetsOfActions = new []{ 
                            // fleeing. one left:

                        new ActionSetType("67812bdc-c1b2-4c93-a248-af6a4e458602")
                         {      
                    //          ChanceToFire = 0.5f,                              
                              Condition = new PlayerAllegiancePersons(){ MinMembers = 1}                               
                               , 
                               Actions = new EventActionType[]{new TalkAction("3fd4b044-3a3e-4230-8f51-35aecc264851") {  
                               
                                TalkPriority = TalkAction.TalkActionPriority.Low,
                                 CanTalkWhileFighting = true,
                                 CanTalkWhileSleeping = false,
                                 CanTalkWhileThreatened = true,
                                 TurnTowardsListeners = false,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
                                ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                DefaultText = "D-do they never give up?" }, //only suited for twinkler island
                        }
                            },               



                         new ActionSetType("2c2ffae6-e812-4dd8-b1e5-f0b3318317fc")
                         {      
                 //             ChanceToFire = 0.5f,                              
                              Condition = new PlayerAllegiancePersons(){ MinMembers = 1, MaxMembers = 1}                               
                               , 
                               MaxFirings = 1,
                               Actions = new EventActionType[]{new TalkAction("cdc33d7d-deb1-4618-9da9-f3ab64d18f11") {  
                               
                                TalkPriority = TalkAction.TalkActionPriority.Low,
                                 CanTalkWhileFighting = true,
                                 CanTalkWhileSleeping = false,
                                 CanTalkWhileThreatened = true,
                                 TurnTowardsListeners = false,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
                                ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                DefaultText = "Epinephrine...my old friend...help me out..." }, //PRECOL talk
                        }
                            },

                             new ActionSetType("6d248223-a377-4445-a10a-949790841238")
                         {      
                  //            ChanceToFire = 0.5f,                              
                              Condition = new PlayerAllegiancePersons(){ MinMembers = 1, MaxMembers = 1}                               
                               , 
                               MaxFirings = 2,
                               Actions = new EventActionType[]{new TalkAction("5550dc23-96d8-40fa-ba33-f8bc1d9341ca") {  
                               
                                TalkPriority = TalkAction.TalkActionPriority.Low,
                                 CanTalkWhileFighting = true,
                                 CanTalkWhileSleeping = false,
                                 CanTalkWhileThreatened = true,
                                 TurnTowardsListeners = false,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
                                ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                DefaultText = "Not again...!" }, //twinkler island
                        }
                            },

                         new ActionSetType("5bcd357a-d137-43d2-96ca-8caa853144bc")
                         {      
                   //          ChanceToFire = 0.5f,                              
                              Condition = new PlayerAllegiancePersons(){ MinMembers = 1, MaxMembers = 1}                               
                               , 
                               MaxFirings = 2,
                               Actions = new EventActionType[]{new TalkAction("108d1aa4-c84d-4f62-9d74-f99d4d9d2981") {  
                               
                                TalkPriority = TalkAction.TalkActionPriority.Low,
                                 CanTalkWhileFighting = true,
                                 CanTalkWhileSleeping = false,
                                 CanTalkWhileThreatened = true,
                                 TurnTowardsListeners = false,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
                                ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                DefaultText = "Flight. F-flight is the right choice now." }, //PRECOL
                        }
                            },

                         new ActionSetType("d46d332e-71fb-4db5-83ad-be1ca8d3bc35")
                         {      
                      //        ChanceToFire = 0.5f,                              
                              Condition = new PlayerAllegiancePersons(){ MinMembers = 1, MaxMembers = 1}                               
                               ,  
                               MaxFirings = 2,
                               Actions = new EventActionType[]{new TalkAction("b5c269c8-5d15-4bf7-b41e-55e266f2c305") {   
                                
                                TalkPriority = TalkAction.TalkActionPriority.Low,
                                 CanTalkWhileFighting = true,
                                 CanTalkWhileSleeping = false,
                                 CanTalkWhileThreatened = true,
                                 TurnTowardsListeners = false,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
                                ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                DefaultText = "Fight or flight...what to choose...fight or fl..." }, //PRECOL
                        }
                            },


 
                    }

            });
            #endregion

            #endregion
          
           


            //

 

            #region General construction shelter talk


            list.Add(new ActionSets()
            {
                KeyName = "startConstructShelter",
                // ... once the remark actionset is set to fire, it chooses a line from the below which each have an equal chance of being selected
                FireMode = ActionSetsToFire.RandomValid,
                ChanceToFire = 1f, //the chance for the set to fire, then chooses from below
                SetsOfActions = new []{ new ActionSetType("9540fb6e-2b4c-44dc-839b-7697f0f5393d")
                        {    
                        //    ChanceToFire = 0.5f,
                            MaxFirings = 1,
                             Condition = new PlayerAllegiancePersons(){ MinMembers = 3 }                                 
                             ,
                       Actions = new EventActionType[]{new TalkAction("41712140-3cd8-489b-8377-1b243b93dea2") {  
                       
                        TalkPriority = TalkAction.TalkActionPriority.Low,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = false,
                        TurnTowardsListeners = true,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.First, 
                        ActionByAgent = ActionByAgent.PreferTriggeringEntity,
                        DefaultText = "We are getting a new shelter now. I hope it doesn't mean we take up permanent residence here." }, //PRECOL style talk

                    new TalkAction("61fd5dca-3afc-43a4-9b99-b5bf6fd40bad") { DelayInSeconds = 3, 
                     
                        TalkPriority = TalkAction.TalkActionPriority.Low,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = false,
                        TurnTowardsListeners = true,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.Second, 
                        ActionByAgent = ActionByAgent.RandomInAllegiance,
                        DefaultText = "Yeah, you made your opinion clear but you were outvoted, remember?" },
                       }
                        },
                    }


            });

            #endregion


            #region General harvest start talk
            list.Add(new ActionSets()
            {
                KeyName = "startHarvestRemark", // general harvest chat... once the remark actionset is set to fire, it chooses a line from the below which each have an equal chance of being selected
                FireMode = ActionSetsToFire.RandomValid,
                ChanceToFire = 0.13f, //the chance for the set to fire, then chooses from below
                
                SetsOfActions = new []{ 

                         new ActionSetType("2c747860-61d7-4882-8cec-0a5cd058dffc")
                         {                                    
                               MaxFirings = 1,
                               Condition = new PlayerAllegiancePersons() { MinMembers = 2 }, 
                               Actions = new EventActionType[]{new TalkAction("5167389a-2ca3-400b-a44b-07eb149dbc1b") {  
                               
                                TalkPriority = TalkAction.TalkActionPriority.Low,
                                CanTalkWhileFighting = false,
                                CanTalkWhileSleeping = false,
                                CanTalkWhileThreatened = false,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
                                ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                DefaultText = "I'm not happy about going in among those plants." }, 

                              new TalkAction("5b0a38c5-8121-4774-97ee-293ce766cb7a") { DelayInSeconds = 3, 
                              
                              TalkPriority = TalkAction.TalkActionPriority.Low,
                              CanTalkWhileFighting = false,
                              CanTalkWhileSleeping = false,
                              CanTalkWhileThreatened = false,
                              SpeakerDenomination = TalkAction.SpeakerInConversation.Second,  
                              ActionByAgent = ActionByAgent.RandomInAllegiance,
                              DefaultText = "Why not? Think of what you might discover." }} //PRECOL style.
                            },

                            }
            });
            #endregion

            #region General harvest end talk NO COPY PASTE!!!!!! NO COPY PASTE!!!!!! NO COPY PASTE!!!!!!NO COPY PASTE!!!!!! NO COPY PASTE!!!!!! NO COPY PASTE!!!!!!NO COPY PASTE!!!!!! NO COPY PASTE!!!!!! NO COPY PASTE!!!!!!
            //endHarvestRemark

//////////////PRECOL style...

            list.Add(new ActionSets()
            {
                KeyName = "endHarvestRemark", // general harvest chat... once the remark actionset is set to fire, it chooses a line from the below which each have an equal chance of being selected
                FireMode = ActionSetsToFire.RandomValid,
                ChanceToFire = 0.13f, //the chance for the set to fire, then chooses from below

                SetsOfActions = new [] { 
                        new ActionSetType("50934cf8-7b18-4508-8417-856fe4d556fe")
                        {   
                       //   ChanceToFire = 0.05f,
                           MaxFirings = 1,
                           Condition = new PlayerAllegiancePersons() { MinMembers = 2 },
                           Actions = new EventActionType[]{
                           	new TalkAction("f0ac7b7c-d715-4a18-ab15-b8ad3be45c60") 
                           	{  
                           
                            TalkPriority = TalkAction.TalkActionPriority.Low,
                            CanTalkWhileFighting = false,
                            CanTalkWhileSleeping = false,
                            CanTalkWhileThreatened = false,
                            ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                            DefaultText = "I'm so grateful for these gloves!" 
                            }, 
                           
                         }
                         },       
                     
  
                         new ActionSetType("549c15fe-d445-4e60-a027-3fed0b302809")
                         {
                             MaxFirings = 1,
                             Condition = new PlayerAllegiancePersons(){ MinMembers = 2 }
                             ,
                             Actions = new EventActionType[]
                             {
                                 new TalkAction("3b751dfe-e117-4473-a3f1-e54c4ca1e86a")
                                 {
                                     
                                         TalkPriority = TalkAction.TalkActionPriority.Low,
                                         CanTalkWhileFighting = false,
                                         CanTalkWhileSleeping = false,
                                         CanTalkWhileThreatened = false,
                                         SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                                         ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                         DefaultText = "Ouch! This little devil bit me!"
                                     
                                 },
                                 new TalkAction("fb119c28-4a7d-439e-b55a-6eae6d0278c5")
                                 {
                                     DelayInSeconds = 2,
                                     
                                         TalkPriority = TalkAction.TalkActionPriority.Low,
                                          CanTalkWhileFighting = false,
                                          CanTalkWhileSleeping = false,
                                          CanTalkWhileThreatened = false,
                                          SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                                          ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                          DefaultText = "I'm going to dissect you later...you just wait."
                                     
                                 }
                             }
                         },
                              new ActionSetType("6d843211-1d17-4f42-910d-a944cc1e05d0")
                         {      
                            //  ChanceToFire = 0.1f,
                              MaxFirings = 1,
                              Condition = new PlayerAllegiancePersons(){ MinMembers = 2 }                               
                               , 
                               Actions = new EventActionType[]{new TalkAction("0b23ef67-7c38-4dfb-a982-db47c13c2982") {  
                               
                                TalkPriority = TalkAction.TalkActionPriority.Low,
                                CanTalkWhileFighting = false,
                                CanTalkWhileSleeping = false,
                                CanTalkWhileThreatened = false,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
                                ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                DefaultText = "Found a strangely behaving arthropod here...it seems to like me!" }, 

                              new TalkAction("5db72be4-5d53-4581-bbc3-d8d800e5d6dc") { DelayInSeconds = 2, 
                              
                              TalkPriority = TalkAction.TalkActionPriority.Low,
                              CanTalkWhileFighting = false,
                              CanTalkWhileSleeping = false,
                              CanTalkWhileThreatened = false,
                              SpeakerDenomination = TalkAction.SpeakerInConversation.Second,  
                              ActionByAgent = ActionByAgent.RandomInAllegiance,
                              DefaultText = "Ha! Yeah that is indeed strange." 
  
                               }
                            }
                          },

                          new ActionSetType("8502aca1-4c0c-490c-a46a-2764b49c432a")
                         {      
                           //   ChanceToFire = 0.1f,
                              MaxFirings = 1,
                              Condition = new PlayerAllegiancePersons(){ MinMembers = 2 }                               
                               , 
                               Actions = new EventActionType[]{new TalkAction("f9bb0fcd-de24-4237-9585-a4eba1a6b52b") {  
                               
                                TalkPriority = TalkAction.TalkActionPriority.Low,
                                CanTalkWhileFighting = false,
                                CanTalkWhileSleeping = false,
                                CanTalkWhileThreatened = false,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
                                ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                DefaultText = "Whoa! There's a species here that is crying out to be documented..." }, 

                              new TalkAction("8de8efe9-eea8-4327-8985-2355191738d4") { DelayInSeconds = 2, 
                              
                              TalkPriority = TalkAction.TalkActionPriority.Low,
                              CanTalkWhileFighting = false,
                              CanTalkWhileSleeping = false,
                              CanTalkWhileThreatened = false,
                              SpeakerDenomination = TalkAction.SpeakerInConversation.Second,  
                              ActionByAgent = ActionByAgent.RandomInAllegiance,
                              DefaultText = "If it's not dangerous or edible it'll have to wait. We need to prioritize." }
  
                               }
                            },


                            }
            });

            #endregion


            #region General eating talk NO COPY PASTE!!!!!! NO COPY PASTE!!!!!! NO COPY PASTE!!!!!!


            list.Add(new ActionSets()
            {
                KeyName = "humanEatingRemark",
                //  once the remark actionset is set to fire, it chooses a line from the below which each have an equal chance of being selected
                FireMode = ActionSetsToFire.RandomValid,
                ChanceToFire = 1f, //the chance for the set to fire, then chooses from below
                SetsOfActions = new []{ 

  

                        new ActionSetType("491ea104-b48f-4563-907d-2378eeaa8f5d")
                         {      
                        //      ChanceToFire = 0.1f, 
                              MaxFirings = 1,
                              Condition = new PlayerAllegiancePersons(){ MinMembers = 2 }                               
                               , 
                               Actions = new EventActionType[]{new TalkAction("88e03d90-627d-4aa7-90ff-eef81b1e8cb8") {  
                               
                                TalkPriority = TalkAction.TalkActionPriority.Low,
                                CanTalkWhileFighting = false,
                                CanTalkWhileSleeping = false,
                                CanTalkWhileThreatened = false,
                                TurnTowardsListeners = false,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
                                ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                DefaultText = "God I'm hungry. I could eat a horse." }, 

                                new TalkAction("cb20d639-f6d1-42e6-929f-8a2f1c4f046a") { DelayInSeconds = 2.5, 
                     
                        TalkPriority = TalkAction.TalkActionPriority.Low,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = false,
                        TurnTowardsListeners = false,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.First, 
                        ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                        DefaultText = "Or whatever passes for horses on this planet." },
                        }
                            },

                          new ActionSetType("51fe0371-d278-4016-b577-25d032fb459d")
                         {      
                      //        ChanceToFire = 0.1f, 
                              MaxFirings = 1,
                              Condition = new PlayerAllegiancePersons(){ MinMembers = 1, MaxMembers = 1 }                               
                               , 
                               Actions = new EventActionType[]{new TalkAction("6f2709f7-733d-4887-8693-abd4ac6fad9f") {  
                               
                                TalkPriority = TalkAction.TalkActionPriority.Low,
                                CanTalkWhileFighting = false,
                                CanTalkWhileSleeping = false,
                                CanTalkWhileThreatened = false,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
                                ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                DefaultText = "A sturdy red wine would go well with this." } 
                        }
                         },
                         new ActionSetType("829551c7-a652-4884-a352-04bb08f20258")
                         {      
                     //         ChanceToFire = 0.1f, 
                              MaxFirings = 1,
                              Condition = new PlayerAllegiancePersons(){ MinMembers = 1, MaxMembers = 2 }                               
                               , 
                               Actions = new EventActionType[]{new TalkAction("c9d28ef1-2914-4426-a116-b125235f33c8") {  
                               
                                TalkPriority = TalkAction.TalkActionPriority.Low,
                                CanTalkWhileFighting = false,
                                CanTalkWhileSleeping = false,
                                CanTalkWhileThreatened = true,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
                                ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                DefaultText = "Food doesn't taste the same anymore..." } 
                        }
                         },

                     
                    }

            });

            #endregion


            #region Make strange meal NO COPY PASTE!!!!!!

            list.Add(new ActionSets()
            {
                KeyName = "startMakeStrangeAnimalMeal",
                //  once the remark actionset is set to fire, it chooses a line from the below which each have an equal chance of being selected
                FireMode = ActionSetsToFire.RandomValid,
                ChanceToFire = 1f, //the chance for the set to fire, then chooses from below
                SetsOfActions = new []{ new ActionSetType("8b6854b1-4bba-4010-b502-6d620b79ac1d")
                        {    
                       //     ChanceToFire = 0.5f,
                            MaxFirings = 1,
                             Condition = new PlayerAllegiancePersons(){ MinMembers = 1 }                                 
                             ,
                       Actions = new EventActionType[]{new TalkAction("d63c0dc2-1564-493d-98d0-36974298c8f3") {  
                       
                        TalkPriority = TalkAction.TalkActionPriority.Normal,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = false,
                        TurnTowardsListeners = false,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.First, 
                        ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                        DefaultText = "I remember first documenting this species down in sector Beta..." }, 

                    new TalkAction("df5d1b64-b587-49af-a353-e6f436ca8d75") { DelayInSeconds = 3, 
                     
                        TalkPriority = TalkAction.TalkActionPriority.Normal,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = false,
                        TurnTowardsListeners = true,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.First, 
                        ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                        DefaultText = "Never dreamed I would see it on a plate one day." },
                       }
                        },



                        new ActionSetType("4c87da6e-cd66-4357-9269-1dc8dc746082")
                         {      
                              MaxFirings = 1,                     
                              Condition = new PlayerAllegiancePersons(){ MinMembers = 3 }                               
                               , 
                               Actions = new EventActionType[]{new TalkAction("e0b405f5-3064-446b-8c3a-37585815b968") {  
                               
                                TalkPriority = TalkAction.TalkActionPriority.Low,
                                CanTalkWhileFighting = false,
                                CanTalkWhileSleeping = false,
                                CanTalkWhileThreatened = false,
                                TurnTowardsListeners = false,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
                                ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                DefaultText = "Are you guys serious about eating this?" }, 

                               new TalkAction("ad3d83b2-2531-4659-97bb-407459c80d1c") {   DelayInSeconds = 3, 
                               
                                TalkPriority = TalkAction.TalkActionPriority.Low,
                                CanTalkWhileFighting = false,
                                CanTalkWhileSleeping = false,
                                CanTalkWhileThreatened = false,
                                TurnTowardsListeners = true,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.Second,  
                                ActionByAgent = ActionByAgent.RandomInAllegiance,
                                DefaultText = "Do your best and try to make it look like a meal." },  
 
                               new TalkAction("f23efbf7-d7d2-4520-8fd1-25a366366fc5") {   DelayInSeconds = 6, 
                               
                                TalkPriority = TalkAction.TalkActionPriority.Low,
                                CanTalkWhileFighting = false,
                                CanTalkWhileSleeping = false,
                                CanTalkWhileThreatened = false,
                                TurnTowardsListeners = false,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
                                ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                DefaultText = "Alright, here goes." },  

                        }
                            }, 
 
                        new ActionSetType("7a35ebd7-c7d9-46e5-9a16-39b80d9df11b")
                         {      
                               MaxFirings = 1,                            
                              Condition = new PlayerAllegiancePersons(){ MinMembers = 2 }                               
                               , 
                               Actions = new EventActionType[]{new TalkAction("6bf0409c-0ae0-4c35-901f-e6e6c009e4e5") {  
                               
                                TalkPriority = TalkAction.TalkActionPriority.Low,
                                CanTalkWhileFighting = false,
                                CanTalkWhileSleeping = false,
                                CanTalkWhileThreatened = false,
                                TurnTowardsListeners = false,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
                                ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                DefaultText = "When you eat this dish...please be aware of any symptoms." }, 

                               new TalkAction("11fffa51-64d7-45d4-9b97-5722ddc6af01") {   DelayInSeconds = 3, 
                               
                                TalkPriority = TalkAction.TalkActionPriority.Low,
                                CanTalkWhileFighting = false,
                                CanTalkWhileSleeping = false,
                                CanTalkWhileThreatened = false,
                                TurnTowardsListeners = true,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
                                ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                DefaultText = "'Cause I'm not entirely sure what pathogens it contains." },  
                 
                        }
                            },             

                        new ActionSetType("0a040f52-2294-4a4d-8fb6-b94ec12b0595")
                         {      
                               MaxFirings = 1,                              
                              Condition = new PlayerAllegiancePersons(){ MinMembers = 2 }                               
                               , 
                               Actions = new EventActionType[]{new TalkAction("ec5d1afc-5695-4a1f-b481-cf69cd751ba6") {  
                               
                                TalkPriority = TalkAction.TalkActionPriority.Low,
                                CanTalkWhileFighting = false,
                                CanTalkWhileSleeping = false,
                                CanTalkWhileThreatened = false,
                                TurnTowardsListeners = false,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
                                ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                DefaultText = "You ever tried eating this?" }, 

                               new TalkAction("a6c67ee9-f63f-4428-8be2-44d9df1c4d19") {   DelayInSeconds = 3, 
                               
                                TalkPriority = TalkAction.TalkActionPriority.Low,
                                CanTalkWhileFighting = false,
                                CanTalkWhileSleeping = false,
                                CanTalkWhileThreatened = false,
                                TurnTowardsListeners = true,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.Second,  
                                ActionByAgent = ActionByAgent.RandomInAllegiance,
                                DefaultText = "Never had the opportunity, no." }, 

                               new TalkAction("c58dcfc1-1b3f-49a8-bb38-45cfc662f5a3") {  DelayInSeconds = 6,
                               
                                TalkPriority = TalkAction.TalkActionPriority.Low,
                                CanTalkWhileFighting = false,
                                CanTalkWhileSleeping = false,
                                CanTalkWhileThreatened = false,
                                TurnTowardsListeners = false,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
                                ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                DefaultText = "Well, you're in for a treat." },

                 
                        }
                            },


                       new ActionSetType("4e5a0bcd-ab7d-40dc-a3d9-e9eecaac10a2")
                         {      
                               MaxFirings = 1,                             
                              Condition = new PlayerAllegiancePersons(){ MinMembers = 3 }                               
                               , 
                               Actions = new EventActionType[]{new TalkAction("be04a6ec-3114-4f96-b608-84060e9f95c0") {  
                               
                                TalkPriority = TalkAction.TalkActionPriority.Low,
                                CanTalkWhileFighting = false,
                                CanTalkWhileSleeping = false,
                                CanTalkWhileThreatened = false,
                                TurnTowardsListeners = false,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
                                ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                DefaultText = "One of you wrote the notes on this species, right?" }, 

                               new TalkAction("e58a5957-1fb6-420d-b162-7312b9b51bbb") {   DelayInSeconds = 3, 
                               
                                TalkPriority = TalkAction.TalkActionPriority.Low,
                                CanTalkWhileFighting = false,
                                CanTalkWhileSleeping = false,
                                CanTalkWhileThreatened = false,
                                TurnTowardsListeners = true,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.Second,  
                                ActionByAgent = ActionByAgent.RandomInAllegiance,
                                DefaultText = "Yes, I did. Anything wrong?" }, 

                               new TalkAction("f1ff9cd1-2080-4050-b045-46dd15e6b729") {  DelayInSeconds = 6,
                               
                                TalkPriority = TalkAction.TalkActionPriority.Low,
                                CanTalkWhileFighting = false,
                                CanTalkWhileSleeping = false,
                                CanTalkWhileThreatened = false,
                                TurnTowardsListeners = false,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
                                ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                DefaultText = "You've stated that it's edible. But I don't see any evidence supporting that claim." },

                               new TalkAction("2783a967-a9d3-4d44-ac0f-c7088e7375e3") {   DelayInSeconds = 9, 
                               
                                TalkPriority = TalkAction.TalkActionPriority.Low,
                                CanTalkWhileFighting = false,
                                CanTalkWhileSleeping = false,
                                CanTalkWhileThreatened = false,
                                TurnTowardsListeners = true,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.Second,  
                                ActionByAgent = ActionByAgent.RandomInAllegiance,
                                DefaultText = "I'll gladly prove it." }, 

                 
                        }
                            },

                 new ActionSetType("fcea2407-0a08-4545-a2ba-ab0c8f28da5b")
                         {      
                               MaxFirings = 1,                               
                              Condition = new PlayerAllegiancePersons(){ MinMembers = 2 }                               
                               , 
                               Actions = new EventActionType[]{new TalkAction("1bc359d8-e6c6-4ac4-b1bb-5c0908a1636c") {  
                               
                                TalkPriority = TalkAction.TalkActionPriority.Low,
                                CanTalkWhileFighting = false,
                                CanTalkWhileSleeping = false,
                                CanTalkWhileThreatened = false,
                                TurnTowardsListeners = false,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
                                ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                DefaultText = "Hmm? The pulmonary tract connects...there??" }, 

                               new TalkAction("765e8a98-754c-4acd-9784-52e1fbda61cb") {   DelayInSeconds = 3, 
                               
                                TalkPriority = TalkAction.TalkActionPriority.Low,
                                CanTalkWhileFighting = false,
                                CanTalkWhileSleeping = false,
                                CanTalkWhileThreatened = false,
                                TurnTowardsListeners = true,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.Second,  
                                ActionByAgent = ActionByAgent.RandomInAllegiance,
                                DefaultText = "You were supposed to prepare a meal, not do a dissection." }, 
  
                 
                        }
                            },




                    }
            });



            #endregion


            #region Death counter
                      
            list.Add(new ActionSets()
            {
                KeyName = "increaseDeathCount",              
                SetsOfActions = new []{ 
                    new ActionSetType("d145a602-7004-48c1-9ebb-96c9669d74d0")
                        {              
                             Actions = new EventActionType[]
                             {
                                 new SetPropertyAction("47469789-23eb-49bd-b1d1-8933fed54f05")
                                 { 
                                     
                                          PropertyKey = "deathCounter", 
                                      
                                          Value = new FunctionNode()
                                          {
                                              Left = new ValueNode()
                                              {
                                                   PropertyKey = "deathCounter"                                          
                                              },
                                              Operator = ExpressionOperator.Plus,
                                              Right = new ValueNode()
                                              {
                                                   Int = 1
                                              }
                                          }
                                     
                                 }
                             }

                        }
                }
            });



            #endregion

            #region Migrate talk and dialogs

          


           

            #endregion


            //mp these never fire:
            #region General going to sleep talk


            list.Add(new ActionSets()
            {
                KeyName = "humanGoingToSleepRemark",
                //  once the remark actionset is set to fire, it chooses a line from the below which each have an equal chance of being selected
                FireMode = ActionSetsToFire.RandomValid,
                ChanceToFire = 1f, //the chance for the set to fire, then chooses from below
                SetsOfActions = new []{ new ActionSetType("1f51959e-a14a-4505-bdca-467e0dd49b2e")
                        {    
                      //      ChanceToFire = 0.5f,
                            MaxFirings = 1,
                             Condition = new PlayerAllegiancePersons(){ MinMembers = 3 }                                 
                             ,
                       Actions = new EventActionType[]{new TalkAction("1d0731da-c5f5-49cb-9f9b-79c4ee5b3745") {  
                       
                        TalkPriority = TalkAction.TalkActionPriority.Low,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = true,
                        CanTalkWhileThreatened = false,
                        TurnTowardsListeners = true,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.First, 
                        ActionByAgent = ActionByAgent.RandomInAllegiance, //the only way it fires.
                        DefaultText = "You should go to sleep. You look exhausted." }, 

                    new TalkAction("4053d64d-9963-488a-9095-2f9be26ce03e") { DelayInSeconds = 3, 
                     
                        TalkPriority = TalkAction.TalkActionPriority.Low,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = false,
                        TurnTowardsListeners = true,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.Second, 
                        ActionByAgent = ActionByAgent.RandomInAllegiance,
                        DefaultText = "We are all exhausted." },
                       }
                        },


 
// 1 person://///////////////

                          new ActionSetType("43e4dde9-0fe1-4ec1-9bf8-dc4cfd27bee3")
                         {      
                     //         ChanceToFire = 0.1f, 
                              MaxFirings = 1,
                              Condition = new PlayerAllegiancePersons(){ MinMembers = 1, MaxMembers = 1 }                               
                               , 
                               Actions = new EventActionType[]{new TalkAction("37f27846-3a56-4289-a1df-a184579c73c0") {  
                               
                                TalkPriority = TalkAction.TalkActionPriority.Low,
                                CanTalkWhileFighting = false,
                                CanTalkWhileSleeping = true,
                                CanTalkWhileThreatened = false,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
                                ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                DefaultText = "Look at those stars. Twinkling like a thousand twinklers. Not a pretty sight." }
                        }
                         },
                         new ActionSetType("9346db96-af1e-483a-9e8c-c343fe848c10")
                         {      
                      //        ChanceToFire = 0.1f, 
                              MaxFirings = 1,
                              Condition = new PlayerAllegiancePersons(){ MinMembers = 1, MaxMembers = 1 }                               
                               , 
                               Actions = new EventActionType[]{new TalkAction("7f2f14b0-b28f-4364-b7e4-9a99daa6d87a") {  
                               
                                TalkPriority = TalkAction.TalkActionPriority.Low,
                                CanTalkWhileFighting = false,
                                CanTalkWhileSleeping = true,
                                CanTalkWhileThreatened = true,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
                                ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                DefaultText = "..So, so tired...motion sensor, tell me if something approaches, please..." } 
                        }
                         },

                        new ActionSetType("a8823556-927d-4aeb-a8fc-d0910ccee7dc")
                         {      
                     //         ChanceToFire = 0.1f,                               
                              Condition = new PlayerAllegiancePersons(){ MinMembers = 1, MaxMembers = 1 }                               
                               , 
                               Actions = new EventActionType[]{new TalkAction("9ce0d858-fd42-497c-a13f-8b8b17ffb40f") {  
                               
                                TalkPriority = TalkAction.TalkActionPriority.Low,
                                CanTalkWhileFighting = false,
                                CanTalkWhileSleeping = true,
                                CanTalkWhileThreatened = true,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
                                ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                DefaultText = "...No getting around it...have to sleep...." } 
                        }
                         }
                     
                    }

            });
            #endregion


            #region production exposition

            list.Add(new ActionSets()
            {
                KeyName = "DEMOISLANDMAP_producePropellerDome",

                SetsOfActions = new []{ new ActionSetType("ae5ac870-3733-4cc6-8a01-95082109644b")
                    {     
                        MaxFirings = 1,
                        Condition = new PlayerAllegiancePersons(){ MinMembers = 2  },
                    Actions = new EventActionType[]{new TalkAction("e62e5467-e228-4061-ac48-b4caebc5de01") { DelayInSeconds = 1.5,   
                     CanTalkWhileFighting = false,
                    CanTalkWhileThreatened = false,
                    CanTalkWhileSleeping = false,
                    ActionByAgent = ActionByAgent.PreferTriggeringEntity,
                    DefaultText = "This metal dome we got off the propeller might be useful as a vessel for cooking." }}
                    }
                    }
            });


            list.Add(new ActionSets()
            {
                KeyName = "DEMOISLANDMAP_makeImprovisedCookingPot",

                SetsOfActions = new []{ new ActionSetType("ee5f0ce5-458c-4a7c-abdd-8750a51be1dc")
                    {     
                        MaxFirings = 1,
                        Condition = new PlayerAllegiancePersons(){ MinMembers = 2 },
                    Actions = new EventActionType[]{new TalkAction("76eebcea-e650-4017-85d7-5a14bf943144") {    
                     CanTalkWhileFighting = false,
                    CanTalkWhileThreatened = false,
                    CanTalkWhileSleeping = false,
                    ActionByAgent = ActionByAgent.PreferTriggeringEntity,
                    DefaultText = "We can make a stew in this." }}
                    }
                    }
            });


            list.Add(new ActionSets() // "item:bushDragonPoisonGlands"
            {
                KeyName = "DEMOISLANDMAP_produceBushdragonPoisonGlands",

                SetsOfActions = new []{ new ActionSetType("aab62af2-1e7d-4794-bdce-355c8a3c6532")
                    {     
                          MaxFirings = 1,
                            Condition = new PlayerAllegiancePersons(){ MinMembers = 2 }                                
                            ,    
                        Actions = new EventActionType[]{new TalkAction("9a57d085-4c33-4982-b9f6-00b6f80b01ef") {  
                                                
                        TurnTowardsListeners = true,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = false,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                        ActionByAgent = ActionByAgent.OnlyTriggeringEntity, 
                        DefaultText = "I finished dissecting the bush dragon and separated the poison glands. It should be possible to extract the poison." }, 

                    new TalkAction("3a6705a8-aea8-46dd-a11c-2474dc7f5a2e") { DelayInSeconds = 3, 
                       
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = false,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                        ActionByAgent = ActionByAgent.RandomInAllegiance,
                        DefaultText = "As long as I don't have to touch it..." }}
                        }
                    }
            });


            list.Add(new ActionSets() 
            {
                KeyName = "DEMOISLANDMAP_makeBushDragonCartridge",

                SetsOfActions = new []{ new ActionSetType("aabdghjte65e7urysujfsghjgfsgh6532")
                    {     
                          MaxFirings = 1,
                            Condition = new PlayerAllegiancePersons(){ MinMembers = 2 }                                
                            ,    
                        Actions = new EventActionType[]{new TalkAction("9a5sfghhhhhhhhhhhhhjbvgjsgfhjfshjsfh1ef") {  
                                                
                        TurnTowardsListeners = true,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = false,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                        ActionByAgent = ActionByAgent.OnlyTriggeringEntity, 
                        DefaultText = "Alright, the cartridge should hold enough for several bursts, but spend it wisely." }, 
}
                        }
                    }
            });




/*mp no longer used
            list.Add(new ActionSets() // "item:bushDragonPoison"
            {
                KeyName = "DEMOISLANDMAP_produceBushdragonPoison",

                SetsOfActions = new []{ new ActionSetType("b8ae0bd8-f180-4345-bd8c-3d99646a9b13")
                    {     
                         MaxFirings = 1,
                            Condition = new ConditionSet()
                             {
                                  Value = new Condition()
                                  {
                                    SiteAllegianceMembers = new PlayerAllegiancePersons(){ MinMembers = 2 }
                                }
                            },
                        Actions = new EventActionType[]{new EventActionType("4c292dbf-e672-4c71-a905-cff849c33938") {  
                            
                            TurnTowardsListeners = true,
                            CanTalkWhileFighting = false,
                            CanTalkWhileSleeping = false,
                            CanTalkWhileThreatened = false,
                            SpeakerDenomination = TalkAction.SpeakerInConversation.First,    
                            ActionByAgent = ActionByAgent.OnlyTriggeringEntity, 
                            DefaultText = "This poison can definitely be used as a chemical weapon. What could we use as a delivery system?" }}, 

                        new EventActionType("0b16e5de-ad5b-4c4e-ab17-bcd41cdc2781") { DelayInSeconds = 3, 
                            TalkAction = new TalkAction()
                        { 
                            CanTalkWhileFighting = false,
                            CanTalkWhileSleeping = false,
                            CanTalkWhileThreatened = false,
                            SpeakerDenomination = TalkAction.SpeakerInConversation.Second, 
                            ActionByAgent = ActionByAgent.RandomInAllegiance,
                            DefaultText = "Maybe a modified fire extinguisher? Or a liquid gun made from scraps." }}}
                            }
                    }

            });
*/
            list.Add(new ActionSets()
            {
                KeyName = "detectThunderChickenCarcass",
                SetsOfActions = new []{ new ActionSetType("145971c6-5b2a-4823-930f-ba8ed2d755f4")
                    {     
                            Condition = new PlayerAllegiancePersons(){ MinMembers = 2 }                                
                            ,
                    MaxFirings = 1,
                    Actions = new EventActionType[]{new TalkAction("9510d49b-407d-4859-8ba1-9f249340c64d") {  
                     
                    CanTalkWhileFighting = false,
                    CanTalkWhileSleeping = false,
                    CanTalkWhileThreatened = false,
                    SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                    TurnTowardsListeners = true,
                    ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                    DefaultText = "What are you up for, thunder chicken stew or roast?" }, 

                new TalkAction("dc52c517-0a83-45d3-a7a6-ae19afd325f2") { DelayInSeconds = 3, 
                    
                    CanTalkWhileFighting = false,
                    CanTalkWhileSleeping = false,
                    CanTalkWhileThreatened = false,
                    SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                    TurnTowardsListeners = true,
                    ActionByAgent = ActionByAgent.RandomInAllegiance,
                    DefaultText = "Roast! Medium rare, please." }                  
                    
                         
                    
                }
                    }
                       

                }
            });
            #endregion

            #region detect Exposition


            list.Add(new ActionSets()
            {
                KeyName = "detectQuaditeCarcass",
                FireMode = ActionSetsToFire.AllValid,
                SetsOfActions = new []{ new ActionSetType("42bff033-d639-4010-90bf-28b3b4552955")
                    {     MaxFirings = 1,
                            Condition = new PlayerAllegiancePersons(){ MinMembers = 2 }                                
                            ,                  
                  Actions = new EventActionType[]{
                    
                  new SetPropertyAction("147a7502-2c8f-4149-b491-ac7b337e977b") { DelayInSeconds = 0, 
                                         PropertyKey = "quaditeCarcassDetected", 
                                             Value = new ValueNode() { Bool = true }
                                    },
                          
                                    new SetPropertyAction("bc76f128-c19c-4329-a9bf-9f6a4fd8369d") { DelayInSeconds = 145, 
                                         PropertyKey = "quaditeCarcassDetectedDelay", 
                                         Value = new ValueNode() { Bool = true }
                                    },     
                       }                    
                    }}
            });



            list.Add(new ActionSets()
            {
                KeyName = "quaditeCarcassDetectedEvent",
                FireMode = ActionSetsToFire.AllValid,

                SetsOfActions = new []{ new ActionSetType("3ae23e71-7c4b-4bc2-868e-c38f10584f1c")
                    {     
                           MaxFirings = 1,
                            Condition = new PlayerAllegiancePersons(){ MinMembers = 2 }                                
                            ,                  
                    Actions = new EventActionType[]{   

     new TalkAction("d214187b-3f7b-498d-99d2-bf9ecff251e2") { DelayInSeconds = 0,                        
                                     
                                    ActionByAgent = ActionByAgent.RandomInAllegiance,
                                    TalkPriority = TalkAction.TalkActionPriority.High,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                                    CanTalkWhileFighting = false,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = false,
                                    TurnTowardsListeners = true,
                                    DefaultText = "It would be worthwhile to write up a risk assessment about the quadites." },

                                new TalkAction("0befd890-ae8f-4ed4-8d22-df81246d0463") { DelayInSeconds = 2.5,                        
                                     
                                    ActionByAgent = ActionByAgent.RandomInAllegiance,
                                    TalkPriority = TalkAction.TalkActionPriority.High,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                                    CanTalkWhileFighting = false,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = false,
                                    TurnTowardsListeners = true,
                                    DefaultText = "Couldn't agree more. We need a thorough analysis of what we're facing here." },  
                    } },
                }
            });




            list.Add(new ActionSets()
            {
                KeyName = "quaditeCarcassDetectedDelayEvent",
                FireMode = ActionSetsToFire.AllValid,
                SetsOfActions = new []{ new ActionSetType("8df3f764-e4ee-4fed-a121-4bd8abd75f4d")
                    {     MaxFirings = 1,
                          Condition = new PlayerAllegiancePersons(){ MinMembers = 2 }                                
                            , 
                    
                    Actions = new EventActionType[]{   

                                 new TalkAction("f709ce43-adb7-47f4-b280-28402d321002") { DelayInSeconds = 0,                        
                                   
                                    ActionByAgent = ActionByAgent.RandomInAllegiance,
                                    TalkPriority = TalkAction.TalkActionPriority.High,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                                    CanTalkWhileFighting = false,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = false,
                                    TurnTowardsListeners = true,
                                    DefaultText = "We have a risk assessment ready." }                               
                                    ,

                                 new EventActionDialog("b35eb86a-25c8-4894-8e25-e82b99fc8c2c") { DelayInSeconds = 3,                                      
                                 DisplayText = new DynamicText()
                                { 
                                    Text = "RISK ASSESSMENT OF QUADITE ATTACKS \n#DATE \nLOCATION: 43 22.5N 124 17.7W \n#JOURNALNAMES  \n \n SUMMARY:\n After identifying the nearby quadite population I can conclude that they pose a threat that will have to be addressed. The species is a well-documented variant of the quadite order called the twinkler quadite. (Thankfully, there's no evidence that the much more ferocious swarmer quadite is present.)  \nWe can expect regular attacks from packs of twinklers actively hunting. They will also aggressively protect their territory.\n RECOMMENDATION:\n It must be ensured that we have enough weapons available. Steps should especially be taken to acquire ranged weapons. The surroundings should be explored to locate the nests - when doing so, we must be wary of twinklers guarding the entrance. Once the nests are identified we can either attempt to cut off any access points between our camp and the nests or we can try to eradicate them." ,
                                    SubstitutionValues = new[]
                                        {
                                            new SubstituteValue() { Placeholder = "#JOURNALNAMES", PropertyName = "getJournalHeaderNames" },
                                            new SubstituteValue() { Placeholder = "#DATE", PropertyName = "getDate", Formatting = FormattingOptions.BothDates }
                                        }
                                    },
                                    DisplayImage = "Quadite"
                      
                                },

                             new TalkAction("5aca70f4-f870-41a4-b3b5-58e364424e5b") { DelayInSeconds = 4,                        
                                     
                                    ActionByAgent = ActionByAgent.RandomInAllegiance,
                                    TalkPriority = TalkAction.TalkActionPriority.High,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                                    CanTalkWhileFighting = false,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = false,
                                    TurnTowardsListeners = true,
                                    DefaultText = "Good work. Let's have a think and make some decisions soon." },
                    } },
                }
            });

            list.Add(new ActionSets()
            {
                KeyName = "detectQuaditeNestSetProperty",
                FireMode = ActionSetsToFire.RandomValid,

                ChanceToFire = 1f, //the chance for the set to fire, then chooses from below
                SetsOfActions = new []{ 
                            new ActionSetType("b012fa9e-fa3d-4a70-bed4-a3a5bbc98ba9")
                            { MaxFirings = 1,
                                  Actions = new EventActionType[]{
                                    new SetPropertyAction("7fc55e6f-a432-4141-a249-8a1de712ff3b") { DelayInSeconds = 0, 
                                         PropertyKey = "nestDetected", Value = new ValueNode() { Bool = true }  
                                    },           

                           
                                    new SetPropertyAction("3fa7f174-3150-4cd3-b904-81cdd0d0ed03") { DelayInSeconds = 145, 
                                         PropertyKey = "nestDetectedDelay", Value = new ValueNode() { Bool = true }
                                    }
                                  }
                            }
                        }

            });


            list.Add(new ActionSets()
            {
                KeyName = "detectQuaditeNestRemark",
                FireMode = ActionSetsToFire.RandomValid,

                ChanceToFire = 1f, //the chance for the set to fire, then chooses from below
                SetsOfActions = new []
                {
                    new ActionSetType("c23ab996-9686-48df-970e-cde6e4718bea")
                    {
                        MaxFirings = 1,
                        Condition = new ConditionFunction()
                        {
                            Operator = OperatorType.And,
                            Left = new PlayerAllegiancePersons(){ MinMembers = 2 }
                            ,
                            Right =  new CustomCondition()
                                {
                                    TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.TargetEntity },
                                    PropertyCondition = new PropertyCondition()
                                    {
                                        PropertyKey = "name", //  This here ensures that they say this once when they detect this nest
                                        ConstantStringEqual = "Quadite nest (coord. 15;34)"  //name of nest is used as condition in many events, make sure it's updated everywhere      was:North quadite nest
                                    }
                                    
                            }
                            
                        },
                        Actions = new EventActionType[]
                        {
                            new TalkAction("8dc3ca94-b7bc-47ef-9f36-5915067d1bf3")
                            {
                                DelayInSeconds = 0,
                                
                                    CanTalkWhileFighting = false, CanTalkWhileThreatened = true, CanTalkWhileSleeping = false,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                                    ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                    TalkPriority = TalkAction.TalkActionPriority.High,
                                    DefaultText = "I see some worrying signs here...this cave could be the home for a large colony of quadites." 
                                
                            },
                            new TalkAction("3515eeae-9d88-4b4e-ad23-81c9fc60b6f0")
                            {
                                DelayInSeconds = 4,
                                
                                    CanTalkWhileFighting = false, CanTalkWhileThreatened = false, CanTalkWhileSleeping = false,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                                    ActionByAgent = ActionByAgent.RandomInAllegiance,
                                    TalkPriority = TalkAction.TalkActionPriority.High,
                                    DefaultText = "Better stay the heck away from there!"
                                
                            },
                        }
                    },
                    new ActionSetType("26b31db3-6378-413d-afbe-34739406e197")
                    {
                        MaxFirings = 1,
                        Condition = new ConditionFunction()
                        {
                            Operator = OperatorType.And,
                            Left = new PlayerAllegiancePersons(){ MinMembers = 2 }
                            ,
                            Right = new CustomCondition()
                            {
                                TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.TargetEntity },
                                PropertyCondition = new PropertyCondition()
                                {
                                    PropertyKey = "name", //  //  This here ensures that they say this once when they detect this nest
                                    ConstantStringEqual = "Quadite nest (coord. 38;38)"  //name of nest is used as condition in many events, make sure it's updated everywhere.  was: East quadite nest
                                }                                    
                            }                            
                        },
                        Actions = new EventActionType[]
                        {
                            new TalkAction("174a99e8-0a2e-4629-9090-5578c95422d9")
                            {
                                DelayInSeconds = 0,
                                
                                    CanTalkWhileFighting = false, CanTalkWhileThreatened = true, CanTalkWhileSleeping = false,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                                    ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                    TalkPriority = TalkAction.TalkActionPriority.High,
                                    DefaultText = "My sensor is picking up a lot of underground activity. This cave could be inhabited by quadites."
                                
                            },
                            new TalkAction("a006d63a-6aac-4b42-ad6e-b16308dd2e4c")
                            {
                                DelayInSeconds = 4,
                                
                                    CanTalkWhileFighting = false, CanTalkWhileThreatened = false, CanTalkWhileSleeping = false,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                                    ActionByAgent = ActionByAgent.RandomInAllegiance,
                                    TalkPriority = TalkAction.TalkActionPriority.High,
                                    DefaultText = "Be extremely cautious, please!"
                                
                            },
                        }
                    },
                    new ActionSetType("f0ce690d-67ab-40a9-85ac-f61046c03088")
                    {
                        MaxFirings = 1,
                        Condition = new PlayerAllegiancePersons(){ MinMembers = 3 }
                        ,
                        Actions = new EventActionType[]
                        {
                            new TalkAction("f61fdf2f-f5ff-4428-85d1-bbcebb66f4b5")
                            {
                                DelayInSeconds = 0,
                                
                                    TalkPriority = TalkAction.TalkActionPriority.High,
                                    CanTalkWhileFighting = false,
                                    CanTalkWhileThreatened = false,
                                    CanTalkWhileSleeping = false,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                                    ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                    DefaultText = "Guys...I'm getting some sensor readings here...I think quadites have a nest nearby."
                                
                            },
                            new TalkAction("49c57a3e-b93b-4676-a4da-1baf353e846d")
                            {
                                DelayInSeconds = 4,
                                
                                    TalkPriority = TalkAction.TalkActionPriority.High,
                                    CanTalkWhileFighting = false,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = false,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.Second, 
                                    ActionByAgent = ActionByAgent.RandomInAllegiance,
                                    DefaultText = "Don't go near it alone!"
                                
                            }
                        }
                    }
                }
            });


            list.Add(new ActionSets()
            {
                KeyName = "nestExpositionEvent",
                FireMode = ActionSetsToFire.AllValid,
                SetsOfActions = new []{ new ActionSetType("4a07f6c7-1d44-4c26-a3da-659d7f7a5db6")
                    {     
                            Condition = new PlayerAllegiancePersons(){ MinMembers = 2 }                                
                            , 
                    MaxFirings = 1,
                    Actions = new EventActionType[]{   

                                 new TalkAction("267f885e-84a6-4a71-a687-eeb193a1687b") { DelayInSeconds = 0,                        
                                   
                                    ActionByAgent = ActionByAgent.RandomInAllegiance,
                                    TalkPriority = TalkAction.TalkActionPriority.High,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                                    CanTalkWhileFighting = false,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = false,
                                    TurnTowardsListeners = true,
                                    DefaultText = "Could you take a moment to study the report about our recent findings." }                                 
                                    ,

                                new EventActionDialog("5327e22a-ad62-4854-be8e-f2134b4f4e51") { DelayInSeconds = 4, 
                                 
                                    DisplayText = new DynamicText(){ Text = "LOCAL GEOLOGY AND QUADITE HABITATS \n#DATE \nLOCATION: 43 22.5N 124 17.7W \n#JOURNALNAMES  \n \nSUMMARY:\n We've now located a quadite colony. The nest lies in a cave which is likely part of a bigger underground system of so-called lava caves which have been formed from ancient volcanic activity. The cave network could hold countless individuals - this number would depend on the amount of prey available in the area.\n  I would expect to see other entrances to the underground network which could hold other quadite nests." ,  //  We are also likely to find a nearby area of current volcanic activity.
                                    SubstitutionValues = new[]
                                        {
                                            new SubstituteValue() { Placeholder = "#JOURNALNAMES", PropertyName = "getJournalHeaderNames" },
                                            new SubstituteValue() { Placeholder = "#DATE", PropertyName = "getDate", Formatting = FormattingOptions.BothDates }
                                        }
                                    },
                                    DisplayImage = "Quadite"
                      
                                },

                                new TalkAction("sadb2235a03562-8ff2-442af0-9ad7-c62a23a191bc") { DelayInSeconds = 5,                        
                                     
                                    ActionByAgent = ActionByAgent.RandomInAllegiance,
                                    TalkPriority = TalkAction.TalkActionPriority.High,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                                    CanTalkWhileFighting = false,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = false,
                                     TurnTowardsListeners = true,
                                    DefaultText = "So, we're like little bugs scurrying around on a giant anthill." }                                 
                                    ,

                                  new TalkAction("07a5fd6b-cc23-4941-982c-b3afcaf6bca7") { DelayInSeconds = 8.5,                        
                                     
                                    ActionByAgent = ActionByAgent.RandomInAllegiance,
                                    TalkPriority = TalkAction.TalkActionPriority.High,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                                    CanTalkWhileFighting = false,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = false,
                                    TurnTowardsListeners = true,
                                    DefaultText = "I'm going to walk a lot softer from now on." }                                 
                                    ,

                                    
                                    new SetPropertyAction("b7fab67a-f616-45b3-abba-38ea9cce3b6d") { DelayInSeconds = 60, 
                                         PropertyKey = "nestExpositionEventHasFired", Value = new ValueNode() { Bool = true }
                                    }

                    } },
                }
            });




            // when a nest has been found and the nest exposition has fired, but sulfur has not been found, and no nest has been destroyed:
            list.Add(new ActionSets()
            {
                KeyName = "nestExpositionRecapEvent",
                SetsOfActions = new []{ new ActionSetType("9c875e0c-6baa-4d8f-afc3-e18524b47777")
                    {     
                            Condition = new PlayerAllegiancePersons(){ MinMembers = 3 }  
                            ,
                    MaxFirings = 1,
                    Actions = new EventActionType[]{new TalkAction("885f2bef-04a5-489b-92cd-3fc800552720") {  DelayInSeconds = 0, 
                     
                    CanTalkWhileFighting = false,
                    CanTalkWhileSleeping = false,
                    CanTalkWhileThreatened = false,
                    SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                    TurnTowardsListeners = true,
                    ActionByAgent = ActionByAgent.RandomInAllegiance,
                    TalkPriority = TalkAction.TalkActionPriority.High,
                    DefaultText = "What are your thoughts about dealing with the twinklers?" }, 

                new TalkAction("073b2d1b-c157-4ecf-9005-1a750fac003b") { DelayInSeconds = 3, 
                    
                    CanTalkWhileFighting = false,
                    CanTalkWhileSleeping = false,
                    CanTalkWhileThreatened = false,
                    SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                    TurnTowardsListeners = true,
                    ActionByAgent = ActionByAgent.RandomInAllegiance,
                    TalkPriority = TalkAction.TalkActionPriority.High,
                    DefaultText = "Since a peace offering is out of the question, we can either fight them or flee." } , 

                new TalkAction("10abd0e1-b6b9-433f-b41b-ad95ed3082b4") { DelayInSeconds = 7, 
                    
                    CanTalkWhileFighting = false,
                    CanTalkWhileSleeping = false,
                    CanTalkWhileThreatened = false,
                    SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                    TurnTowardsListeners = true,
                    ActionByAgent = ActionByAgent.RandomInAllegiance,
                    TalkPriority = TalkAction.TalkActionPriority.High,
                    DefaultText = "Fleeing, as in moving camp to a safer area...this makes sense to me." } , 
 
                new TalkAction("73f64f1d-8374-48ab-b42e-dcc4a27872bf") { DelayInSeconds = 10, 
                    
                    CanTalkWhileFighting = false,
                    CanTalkWhileSleeping = false,
                    CanTalkWhileThreatened = false,
                    SpeakerDenomination = TalkAction.SpeakerInConversation.Third,
                    TurnTowardsListeners = true,
                    ActionByAgent = ActionByAgent.RandomInAllegiance,
                    TalkPriority = TalkAction.TalkActionPriority.High,
                    DefaultText = "We should smoke out their nest." } , 

                 new TalkAction("2e5afd70-45b7-44f3-a855-5f7b19997d7c") { DelayInSeconds = 13, 
                    
                    CanTalkWhileFighting = false,
                    CanTalkWhileSleeping = false,
                    CanTalkWhileThreatened = false,
                    SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                    TurnTowardsListeners = true,
                    ActionByAgent = ActionByAgent.RandomInAllegiance,
                    TalkPriority = TalkAction.TalkActionPriority.High,
                    DefaultText = "With what?" } , 

                 new TalkAction("f01dbf8a-db5c-4deb-8afd-54c330c73584") { DelayInSeconds = 16, 
                    
                    CanTalkWhileFighting = false,
                    CanTalkWhileSleeping = false,
                    CanTalkWhileThreatened = false,
                    SpeakerDenomination = TalkAction.SpeakerInConversation.Third,
                    TurnTowardsListeners = true,
                    ActionByAgent = ActionByAgent.RandomInAllegiance,
                    TalkPriority = TalkAction.TalkActionPriority.High,
                    DefaultText = "We'll come up with something." } ,


                }
                    }
                    }
            });


            ///triggered with property triggers from detectsulfur and detectnest:
            list.Add(new ActionSets()
            {
                KeyName = "talkSulfurAndNest",
                SetsOfActions = new []{ new ActionSetType("a0e885f0-ae94-46c4-9036-3e2539f3044d")
                    {     
                            Condition = new PlayerAllegiancePersons(){ MinMembers = 2 }   
                            ,
                    MaxFirings = 1,
                    Actions = new EventActionType[]{new TalkAction("cc40dd2e-e1cb-4368-8ea3-81a79f028786") {  DelayInSeconds = 0, 
                     
                    CanTalkWhileFighting = false,
                    CanTalkWhileSleeping = false,
                    CanTalkWhileThreatened = true,
                    SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                    TurnTowardsListeners = true,
                    ActionByAgent = ActionByAgent.RandomInAllegiance,
                    TalkPriority = TalkAction.TalkActionPriority.High,
                    DefaultText = "We have access to sulfur...let's put it to use against a twinkler nest." }, 

                new TalkAction("58e67828-bedb-4701-99b9-b2d59c42db91") { DelayInSeconds = 4, 
                    
                    CanTalkWhileFighting = false,
                    CanTalkWhileSleeping = false,
                    CanTalkWhileThreatened = false,
                    SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                    TurnTowardsListeners = true,
                    ActionByAgent = ActionByAgent.RandomInAllegiance,
                    TalkPriority = TalkAction.TalkActionPriority.High,
                    DefaultText = "Yeah...should be possible to smoke them out." }  
                    
                }
                    }
                    }
            });



/* mp not used at the moment
 
            list.Add(new ActionSets()
            {
                KeyName = "detectBushdragonCarcass",
                SetsOfActions = new []{ new ActionSetType("e8442dfe-de3a-4723-b1a8-3b1f0cba42e0")
                        {     
                             Condition = new ConditionSet()
                             {
                                  Value = new Condition()
                                  {
                                      SiteAllegianceMembers = new PlayerAllegiancePersons(){ MinMembers = 2 }
                                 }
                             },
                       MaxFirings = 1,
                       Actions = new EventActionType[]{new EventActionType("1cafa526-625c-49c8-a44d-7dfce274499a") { DelayInSeconds = 5, 
                       CanTalkWhileFighting = false,
                        CanTalkWhileThreatened = false,
                        CanTalkWhileSleeping = false,
                         TurnTowardsListeners = true,
                        ActionByAgent = ActionByAgent.PreferTriggeringEntity,
                        DefaultText = "The bush dragon uses a toxic spray as a defense against predators. I think we should dissect a specimen to learn more." }}}
                        }
                     }

            });

*/





//
            list.Add(new ActionSets()
            {
                KeyName = "detectSkimmerTailRemark",
                FireMode = ActionSetsToFire.AllValid,

                SetsOfActions = new []{ 
                            new ActionSetType("3a5e4e9d-2860-43eb-9aac-0b20f0a18563")
                            { MaxFirings = 1,
                            Condition = new ConditionFunction()
                                {
                                    Operator = OperatorType.And,
                                    Left = new PlayerAllegiancePersons(){ MinMembers = 2 }
                                    ,
                                    Right = new CustomCondition(){ 
                                                    TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.TargetEntity },
                                                    PropertyCondition = new PropertyCondition()  { PropertyKey = "sandstoneWreckage", BoolValue = true  } //only triggered in scenario where tail is lying separate
                                             
                                            
                                }
                                  
                             },               
                    Actions = new EventActionType[]{   

     new TalkAction("65959f37-4ca8-4563-b7f6-88dc94e84002") { DelayInSeconds = 0,                        
                                     
                                    ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                    TalkPriority = TalkAction.TalkActionPriority.Normal, //prio lower than supplies spotted
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                                    CanTalkWhileFighting = false,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = false,
                                    TurnTowardsListeners = false,
                                    DefaultText = "I got a visual on some wreckage here." },

                                new TalkAction("1408698e-456c-49da-9ac7-6a3a96e4dd1b") { DelayInSeconds = 3,                        
                                     
                                    ActionByAgent = ActionByAgent.RandomInAllegiance,
                                    TalkPriority = TalkAction.TalkActionPriority.Normal,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                                    CanTalkWhileFighting = false,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = false,
                                    TurnTowardsListeners = true,
                                    DefaultText = "OK. Maybe we can even find our missing supplies?" }
                      
                    } },
                }
            });





            list.Add(new ActionSets()
            {
                KeyName = "detectCratesRemark",
                FireMode = ActionSetsToFire.AllValid,

                SetsOfActions = new []{ new ActionSetType("27889373-4d2b-4504-a572-c1260af21a90")
                    {     
                           MaxFirings = 1,
                            Condition = new PlayerAllegiancePersons(){ MinMembers = 2 }                                
                            ,                  
                    Actions = new EventActionType[]{   

     new TalkAction("5f75252c-d0c8-4bfe-8289-2b39c7a3b95a") { DelayInSeconds = 0,                        
                                     
                                    ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                    TalkPriority = TalkAction.TalkActionPriority.High,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                                    CanTalkWhileFighting = false,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = false,
                                    TurnTowardsListeners = false,
                                    DefaultText = "Hey! I can see some of our supplies... they've fallen down this crevice." },

                                new TalkAction("e7409512-3536-447d-ad12-4aeb783426ea") { DelayInSeconds = 3,                        
                                     
                                    ActionByAgent = ActionByAgent.RandomInAllegiance,
                                    TalkPriority = TalkAction.TalkActionPriority.High,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                                    CanTalkWhileFighting = false,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = false,
                                    TurnTowardsListeners = true,
                                    DefaultText = "Yeah? Can we get them up?" }, 
 
                                  new TalkAction("7fb5907c-05eb-48a1-a063-1e48315d576f")   { DelayInSeconds = 6,                        
                                     
                                    ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                    TalkPriority = TalkAction.TalkActionPriority.High,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                                    CanTalkWhileFighting = false,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = false,
                                    TurnTowardsListeners = false,
                                    DefaultText = "I think so...should be possible to climb down using rope." },
                    } },
                }
            });
            /*
            list.Add(new ActionSets()
            {
                KeyName = "detectShadeleafCanes",
                FireMode = ActionSetsToFire.FirstValid,
                ChanceToFire = 1f, //the chance for the set to fire, then chooses from below
                SetsOfActions = new []{ 
                            new ActionSetType("c23ab996-9686-48df-970e-cde6e44sr8u56h8bea2")
                            {   MaxFirings = 1,
                                Condition = new ConditionSet()
                                {
                                    Value = new Condition()
                                    {
                                        SiteAllegianceMembers = new PlayerAllegiancePersons(){ MinMembers = 2 }
                                    }
                                },
                                Actions = new EventActionType[]{ 
                                    new EventActionType("8dc3ca94-b7bc-47ef-9f36sdg4-591503zsd3gs5bf32") 
                                    { 
                                        DelayInSeconds = 0,
                                         
                                            CanTalkWhileFighting = false, CanTalkWhileThreatened = true, CanTalkWhileSleeping = false,
                                            SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                                            ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                            TalkPriority = TalkAction.TalkActionPriority.Normal,
                                            DefaultText = "These Shadeleaf canes looks flexible and sturdy, could be usefull" //bso
                                        }
                                    },
                                    new EventActionType("3515eeae-9d88-4b4e-ad2s553tsb6f02") 
                                    { 
                                        DelayInSeconds = 4,
                                         
                                            CanTalkWhileFighting = false, CanTalkWhileThreatened = false, CanTalkWhileSleeping = false,
                                            SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                                            ActionByAgent = ActionByAgent.RandomInAllegiance,
                                            TalkPriority = TalkAction.TalkActionPriority.Normal,
                                            DefaultText = "Maybe we could make a fish trap out of them" //bso
                                        }
                                    }
                                }   
                            },   
                }
            });*/

            #endregion

            #region gather spoak branches

            list.Add(new ActionSets() // "item:spoakBranches"
            {
                KeyName = "DEMOISLANDMAP_produceSpoakBranches",

                SetsOfActions = new []{ new ActionSetType("59f34ccf-afbe-49b3-8f34-b545826b6e26")
                    {                               
                            Condition = new PlayerAllegiancePersons(){ MinMembers = 2 }                                
                            ,
                    MaxFirings = 1,
                    Actions = new EventActionType[]{new TalkAction("4134db30-5f63-4424-a200-75856d8bd567") {  
                     
                    TurnTowardsListeners = false,
                    CanTalkWhileFighting = false,
                    CanTalkWhileSleeping = false,
                    CanTalkWhileThreatened = false,
                    SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                    ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                    LineKey = "branchesQuestion",               
                    DefaultText = "So we got spoak branches. What was it you wanted them for?" }, 

                new TalkAction("c850323e-57b6-47a2-84ca-87d9bc3b5a2e") { DelayInSeconds = 2.5, 
                    
                    CanTalkWhileFighting = false,
                    CanTalkWhileSleeping = false,
                    CanTalkWhileThreatened = false,
                    SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                    ActionByAgent = ActionByAgent.RandomInAllegiance,
                    LineKey = "branchesAnswer", 
                    DefaultText = "Fences. Arrange them tightly, and they should keep out any quadites." },

                new TalkAction("69776c99-62ad-4a0f-ae1c-29aaa279b956") { DelayInSeconds = 4.5, 
                    
                    CanTalkWhileFighting = false,
                    CanTalkWhileSleeping = false,
                    CanTalkWhileThreatened = false,
                    SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                    LineKey = "branchesAnswerComeback",
                    DefaultText = "'Should'." }}
                    }
                    }

            });

            #endregion

            #region Rat nest talk
            //bso this shouold be deleted
            #region Finding the nest
            /*
            list.Add(new ActionSets() //bso TO DO: this is just a copypaste of quadite nest, customice to rats needed
            {
                KeyName = "detectRatNestRemark",
                FireMode = ActionSetsToFire.RandomValid,

                ChanceToFire = 1f, //the chance for the set to fire, then chooses from below
                SetsOfActions = new []{ 
                            new ActionSetType("c23ab996-9686-48df-970e-cde6e4718bea2")
                            {   MaxFirings = 1,
                                Condition = new ConditionSet()
                                {
                                    Value = new Condition()
                                    {
                                        SiteAllegianceMembers = new PlayerAllegiancePersons(){ MinMembers = 2 }
                                    }
                                },
                                Actions = new EventActionType[]{ 
                                    new EventActionType("8dc3ca94-b7bc-47ef-9f36-5915067d1bf32") 
                                    { 
                                        DelayInSeconds = 0,
                                         
                                            CanTalkWhileFighting = false, CanTalkWhileThreatened = true, CanTalkWhileSleeping = false,
                                            SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                                            ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                            TalkPriority = TalkAction.TalkActionPriority.High,
                                            DefaultText = "I've found a rat nest talk#1"
                                        }
                                    },
                                    new EventActionType("3515eeae-9d88-4b4e-ad23-81c9fc60b6f02") 
                                    { 
                                        DelayInSeconds = 4,
                                         
                                            CanTalkWhileFighting = false, CanTalkWhileThreatened = false, CanTalkWhileSleeping = false,
                                            SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                                            ActionByAgent = ActionByAgent.RandomInAllegiance,
                                            TalkPriority = TalkAction.TalkActionPriority.High,
                                            DefaultText = "Rat talk reply#1" 
                                        }
                                    },
                                }                    
                            },
                  
                            new ActionSetType("26b31db3-6378-413d-afbe-34739406e197") //bso there is no maxfiring on this set, so use this for the rest
                            { 
                                Condition = new ConditionSet()
                                {
                                    Value = new Condition()
                                    {
                                        SiteAllegianceMembers = new PlayerAllegiancePersons(){ MinMembers = 2 }
                                    }
                                },                   
                                Actions = new EventActionType[]
                                { 
                                    new EventActionType("174a99e8-0a2e-4629-9090-5578c95422d9") 
                                    { 
                                        DelayInSeconds = 0,
                                         
                                            CanTalkWhileFighting = false, CanTalkWhileThreatened = true, CanTalkWhileSleeping = false,
                                            SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                                            ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                            TalkPriority = TalkAction.TalkActionPriority.High,
                                            DefaultText = "I've found a rat nest talk#2" 
                                        }
                                    },
                                    new EventActionType("a006d63a-6aac-4b42-ad6e-b16308dd2e4c") 
                                    { 
                                        DelayInSeconds = 4,
                                         
                                            CanTalkWhileFighting = false, CanTalkWhileThreatened = false, CanTalkWhileSleeping = false,
                                            SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                                            ActionByAgent = ActionByAgent.RandomInAllegiance,
                                            TalkPriority = TalkAction.TalkActionPriority.High,
                                            DefaultText = "Rat talk reply#2" 
                                        }
                                    }, 
                                }  
                            },   
                }
            });*/
            #endregion
            #region Rat nest Destroyed
         /*   list.Add(new ActionSets()
            {
                KeyName = "destroyRatNestTalk",
                //  once the remark actionset is set to fire, it chooses a line from the below which each have an equal chance of being selected
                FireMode = ActionSetsToFire.RandomValid,
                ChanceToFire = 1f, //the chance for the set to fire, then chooses from below
                SetsOfActions = new []
                { 
                    new ActionSetType("96487f70-b8a9-4fd8-a21b-554ff219cd2a2")
                    {    
                        //ChanceToFire = 0.5f,
                        //MaxFirings = 1,
                        Condition = new ConditionSet()
                        {
                            Value = new Condition()
                            {
                                SiteAllegianceMembers = new PlayerAllegiancePersons(){ MinMembers = 3 }
                            }
                        },
                        Actions = new EventActionType[]
                        {
                            new EventActionType("19eb3216-0108-4918-a1da-e9790c9ff1952") 
                            {  
                                 
                                    TalkPriority = TalkAction.TalkActionPriority.High,
                                    CanTalkWhileFighting = false,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = true,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                                    ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                    DefaultText = "nest destroyed start talk#1" 
                                }
                            }, 
                            new EventActionType("d4687b4f-d3c3-4f32-9046-721e1676288b2") 
                            { 
                                DelayInSeconds = 4, 
                                
                                    TalkPriority = TalkAction.TalkActionPriority.High,
                                    CanTalkWhileFighting = false,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = true,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                                    ActionByAgent = ActionByAgent.RandomInAllegiance,
                                    DefaultText = "nest destroyed reply#1" 
                                }
                            },
                        }
                    },
                    new ActionSetType("ff6d2623-a295-48af-a4fa-396ceb3cded22")
                    {
                        //      ChanceToFire = 0.5f,
                        //MaxFirings = 1,
                        Condition = new ConditionSet()
                        {
                            Value = new Condition()
                            {
                                SiteAllegianceMembers = new PlayerAllegiancePersons(){ MinMembers = 2 }
                            }
                        },
                        Actions = new EventActionType[]
                        {
                            new EventActionType("4ce1bff0-1484-4377-89e7-994c88175d852") 
                            {  
                                 
                                    TalkPriority = TalkAction.TalkActionPriority.High,
                                    CanTalkWhileFighting = false,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = true,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                                    ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                    DefaultText = "nest destroyed start talk#1" 
                                }
                            }, 
                            new EventActionType("0af58c97-26d1-4421-ab96-7b6eda7b33382") 
                            { 
                                DelayInSeconds = 4, 
                                
                                    TalkPriority = TalkAction.TalkActionPriority.High,
                                    CanTalkWhileFighting = false,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = true,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                                    ActionByAgent = ActionByAgent.RandomInAllegiance,
                                    DefaultText = "nest destroyed reply#1" 
                                }
                            },
                        }
                    }
                }
            });*/
            #endregion
            #endregion



         

            #region construction wigwam

            list.Add(new ActionSets()
            {
                KeyName = "startConstructWigwamSpoakShingles",
                //  once the remark actionset is set to fire, it chooses a line from the below which each have an equal chance of being selected
                FireMode = ActionSetsToFire.RandomValid,
                ChanceToFire = 1f, //the chance for the set to fire, then chooses from below
                SetsOfActions = new []{ new ActionSetType("ec5a16f6-143e-4f6b-81b9-a0bf6e4c31d2")
                        {    
                       //     ChanceToFire = 0.5f,
                            MaxFirings = 1,
                             Condition = new PlayerAllegiancePersons(){ MinMembers = 3 }                                 
                             ,
                       Actions = new EventActionType[]{new TalkAction("8e589a39-2ec7-4b91-892f-0c0cbd69ea7e") {  
                       
                        TalkPriority = TalkAction.TalkActionPriority.Normal,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = false,
                        TurnTowardsListeners = false,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.First, 
                        ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                        DefaultText = "So we want a wigwam, huh. It's gonna take a while." }, 

                    new TalkAction("c5552ff1-7a51-40ca-8990-117cab48a57c") { DelayInSeconds = 3, 
                     
                        TalkPriority = TalkAction.TalkActionPriority.Normal,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = false,
                        TurnTowardsListeners = true,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.Second, 
                        ActionByAgent = ActionByAgent.RandomInAllegiance,
                        DefaultText = "You can do it - we've already done half of the work, making those shingles." },  //"You can do it - we've already done half of the work, weaving those mats." 
                       }
                        },
                        new ActionSetType("f8ae8afb-07f8-4222-ab10-52797413c476")
                         {      
                       //       ChanceToFire = 0.1f,                              
                              Condition = new PlayerAllegiancePersons(){ MaxMembers = 1 }                               
                               , 
                               Actions = new EventActionType[]{new TalkAction("9fb96d7b-6dd4-4bee-83be-8e2cf0857b53") {  
                               
                                TalkPriority = TalkAction.TalkActionPriority.Low,
                                CanTalkWhileFighting = false,
                                CanTalkWhileSleeping = false,
                                CanTalkWhileThreatened = false,
                                TurnTowardsListeners = false,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
                                ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                DefaultText = "I can't believe I'm actually building this." }, 
     
                        }
                            },                
                    }
            });

            ////////


            list.Add(new ActionSets()
            {
                KeyName = "endConstructWigwamSpoakShingles",
                SetsOfActions = new []{ new ActionSetType("40dce68e-02f1-419c-a0a9-ba6afc94e0a7")
                     {                     
                        MaxFirings = 1, 
                        Condition = new PlayerAllegiancePersons(){ MinMembers = 3 }                            
                        ,                        
                       Actions = new EventActionType[]{new TalkAction("c942a448-38cc-454c-8c06-5f3399998434") {  
                       
                        TalkPriority = TalkAction.TalkActionPriority.Normal,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = false,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.First, 
                        ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                        DefaultText = "Your Excellencies, behold: The palace is complete." }, 

                    new TalkAction("30a4e179-3d64-4c62-84d5-210e5b2ed082") { DelayInSeconds = 3, 
                     
                        TalkPriority = TalkAction.TalkActionPriority.Normal,
                        TurnTowardsListeners = true,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = false,                       
                        SpeakerDenomination = TalkAction.SpeakerInConversation.Second,  
                        ActionByAgent = ActionByAgent.RandomInAllegiance,
                        DefaultText = "Wow! Awesome!" },

                      new TalkAction("627ea0f7-e546-45da-84e7-7221ecfe8681") { DelayInSeconds = 6, 
                     
                        TalkPriority = TalkAction.TalkActionPriority.Normal,
                        TurnTowardsListeners = true,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = false,                       
                        SpeakerDenomination = TalkAction.SpeakerInConversation.Third,  
                        ActionByAgent = ActionByAgent.RandomInAllegiance,
                        DefaultText = "Ahaaa. Very good. We shall be delighted to take up residence there." },  
                   
                     new TalkAction("b1dd7b39-049d-4648-947b-fcb1f67749c2") { DelayInSeconds = 9, 
                     
                        TalkPriority = TalkAction.TalkActionPriority.Normal,
                        TurnTowardsListeners = true,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = false,                       
                        SpeakerDenomination = TalkAction.SpeakerInConversation.Second,  
                        ActionByAgent = ActionByAgent.RandomInAllegiance,
                        DefaultText = "Yes...and rule over this island with an iron fist!" }                      
                       
                       }
                        }
                     }
            });


            #endregion

            #region construction sensor
            list.Add(new ActionSets()
            {
                KeyName = "endConstructSensor",
                SetsOfActions = new []{ new ActionSetType("1178a3d4-9f8f-4eb0-9a72-79a38f7a3a95")
                     {                     
                        MaxFirings = 1, 
                        Condition = new PlayerAllegiancePersons(){ MinMembers = 2 }                            
                        ,                        
                       Actions = new EventActionType[]{new TalkAction("03410af5-39a2-483b-9eba-ff5606e4bb13") {  
                       
                        TalkPriority = TalkAction.TalkActionPriority.Normal,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = false,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.First, 
                        ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                        DefaultText = "Sensor is online. Now we can just sit back and watch." }, 

                 
                       
                       }
                        }
                     }
            });

            #endregion

            #region construction dome shelter spoak shingles



            list.Add(new ActionSets()
            {
                KeyName = "endConstructDomeShelterSpoakShingles",
                SetsOfActions = new []{ new ActionSetType("704d3724-8b91-49df-bd15-50ca38957088")
                     {                     
                        MaxFirings = 1, 
                        Condition = new PlayerAllegiancePersons(){ MinMembers = 2 }                            
                        ,                        
                       Actions = new EventActionType[]{new TalkAction("6606d607-7b18-4a0d-b9e3-97af8352a7c2") {  
                       
                        TalkPriority = TalkAction.TalkActionPriority.Normal,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = false,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.First, 
                        ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                        DefaultText = "If we've done it right, this shelter should remain free of scuttler bugs." }, 

                    new TalkAction("dd66bd58-1203-447e-9ba4-f7d7ef6e9e29") { DelayInSeconds = 3, 
                     
                        TalkPriority = TalkAction.TalkActionPriority.Normal,
                        TurnTowardsListeners = true,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = false,                       
                        SpeakerDenomination = TalkAction.SpeakerInConversation.Second,  
                        ActionByAgent = ActionByAgent.RandomInAllegiance,
                        DefaultText = "Can't wait to move in - those bugs are getting on my nerves." }
                 
                       
                       }
                        }
                     }
            });



            #endregion



            #region construction abatis

            list.Add(new ActionSets()
            {
                KeyName = "produceAbatis",
                SetsOfActions = new []{ new ActionSetType("f6d685fd-e392-47d1-aa8e-0a30ba0df78e")
                        {      
                              MaxFirings = 1,
                             Condition = new PlayerAllegiancePersons(){ MinMembers = 2 }                                 
                             ,                            
                            Actions = new EventActionType[]{new TalkAction("007933d6-0490-4236-9cb0-e76ae8460376") {  
                       
                        TalkPriority = TalkAction.TalkActionPriority.Low,
                        TurnTowardsListeners = true,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = false,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                        ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                        DefaultText = "We're going to need a lot of these obstacles to keep out quadites." }, 

                    new TalkAction("1f1aefff-53d3-4c57-b822-0d572b5e00e3") { DelayInSeconds = 3, 
                     
                        TalkPriority = TalkAction.TalkActionPriority.Low,
                        TurnTowardsListeners = true,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = false,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                        ActionByAgent = ActionByAgent.RandomInAllegiance,
                        DefaultText = "That's why I recommend we make it top priority." },

                    new TalkAction("85019db0-e51d-4efc-8873-b9cd6311dcaf") { DelayInSeconds = 7, 
                     
                        TalkPriority = TalkAction.TalkActionPriority.Low,
                        TurnTowardsListeners = true,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = false,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                        ActionByAgent = ActionByAgent.PreferTriggeringEntity,
                        DefaultText = "Wouldn't it be better to move the camp somewhere safer?" }}
                        }
                     }
            });

            #endregion


            #region Sulfur attack

            list.Add(new ActionSets()
            {
                KeyName = "startUseSulfurSmokeBombRemark",
                //  once the remark actionset is set to fire, it chooses a line from the below which each have an equal chance of being selected
                FireMode = ActionSetsToFire.RandomValid,
                ChanceToFire = 1f, //the chance for the set to fire, then chooses from below
                SetsOfActions = new []{ new ActionSetType("96487f70-b8a9-4fd8-a21b-554ff219cd2a")
                        {    
                      //      ChanceToFire = 0.5f,
                            MaxFirings = 1,
                             Condition = new PlayerAllegiancePersons(){ MinMembers = 3 }                                 
                             ,
                            Actions = new EventActionType[]{new TalkAction("19eb3216-0108-4918-a1da-e9790c9ff195") {  
                       
                        TalkPriority = TalkAction.TalkActionPriority.High,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = true,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                        ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                        DefaultText = "Alright let's do this. Anyone watching my back?" }, 

                    new TalkAction("d4687b4f-d3c3-4f32-9046-721e1676288b") { DelayInSeconds = 4, 
                     
                        TalkPriority = TalkAction.TalkActionPriority.High,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = true,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                        ActionByAgent = ActionByAgent.RandomInAllegiance,
                        DefaultText = "Be careful." },
                    
                            }
                        },
                     

                 new ActionSetType("ff6d2623-a295-48af-a4fa-396ceb3cded2")
                 {
                     //      ChanceToFire = 0.5f,
                        MaxFirings = 1,
                             Condition = new PlayerAllegiancePersons(){ MinMembers = 2 }                                 
                             ,
                            Actions = new EventActionType[]{new TalkAction("4ce1bff0-1484-4377-89e7-994c88175d85") {  
                       
                        TalkPriority = TalkAction.TalkActionPriority.High,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = true,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                        ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                        DefaultText = "You ready? I'm setting it off now." }, 

                    new TalkAction("0af58c97-26d1-4421-ab96-7b6eda7b3338") { DelayInSeconds = 4, 
                     
                        TalkPriority = TalkAction.TalkActionPriority.High,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = true,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                        ActionByAgent = ActionByAgent.RandomInAllegiance,
                        DefaultText = "Fingers crossed." },
                    
                            }
                        }

            }
            });

            list.Add(new ActionSets()
            {
                KeyName = "sulfurBombActivated",
                FireMode = ActionSetsToFire.AllValid,
                SetsOfActions = new []
                {
                    new ActionSetType("ab9afb19-eea6-44b7-a36d-391f7f4262a8"){   Actions = new EventActionType[]
                    { 
                        new DestroyEntityAction("71cf86ae-fbfb-4e15-bb67-bd64d7f263bb")
                        {  // kills the nest after 5 s
                            DelayInSeconds = 5f,                        
                            TargetObject = new TargetObject()
                            {
                                TargetObjectType = TargetObjectType.TargetEntity
                            }
                           // EntityToDestroy = TargetEntityOfAction.TargetEntity                    
                        },
                        new SetPropertyAction("0d310cf7-41e3-46c7-8917-b92ac8358064")
                        {  // kills the nest after 5 s
                            DelayInSeconds = 5f,                        
                            PropertyKey = "nestDestroyed",// to avoid exposition about how to kill a nest once the player has learned this
                            Value = new ValueNode() { Bool = true }  
                        },
                                
                        new ParticleEffectAction("9aefawf3-51efe191-d0faf51-43ce-859f-24a284ce1504")
                        {                        
                            UseLocationOfEntity = new TargetObject(){ TargetObjectType = TargetObjectType.TargetEntity },                          
                                // use entity location, don't attach emitter
                            DurationInSeconds = 15d,//mp april 2015 was 12d
                            ParticleEmitters = new[]
                            {
                                new ParticleEmitterEffect()
                                {
                                    AttachToEntity = false,
                                    ParticleSystemKey = "sulfurBomb",
                                }
                            }   
                        }                    
                    
                    }} 
            }
            });


            list.Add(new ActionSets()
            {
                KeyName = "sulfurBombActivatedRemark", // 
                //  once the remark actionset is set to fire, it chooses a line from the below which each have an equal chance of being selected
                FireMode = ActionSetsToFire.RandomValid,
                ChanceToFire = 1f, //the chance for the set to fire, then chooses from below
                SetsOfActions = new []{

                             new ActionSetType("5b76e317-9a13-4ec9-9edb-a5f1bd9b6b54"){   
                                MaxFirings = 1,
                                Condition = new PlayerAllegiancePersons(){ MinMembers = 2 },
                                 Actions = new EventActionType[]{ 
                    
                     new TalkAction("f601834d-9fef-477d-a7fe-79a3d5742943") { DelayInSeconds = 1, //mp to help it getting executed without being blocked by the preceding dialogue
                     
                        TalkPriority = TalkAction.TalkActionPriority.High,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = true,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                        ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                        DefaultText = "Setting it off!" },

                    new TalkAction("98e1bd58-c73e-4ea1-a6c5-41263494329f") { DelayInSeconds = 6, 
                     
                        TalkPriority = TalkAction.TalkActionPriority.High,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = false,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                        ActionByAgent = ActionByAgent.PreferTriggeringEntity,
                        DefaultText = "Alright. Activity in the cave seems to have died out." },

                     new TalkAction("76cad920-3cf8-4a36-9dcb-d25868749044") { DelayInSeconds = 9, 
                     
                        TalkPriority = TalkAction.TalkActionPriority.High,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = false,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                        ActionByAgent = ActionByAgent.RandomInAllegiance,
                        DefaultText = "Excellent work. Let's hope we got them all!" },
                    
                             }},


                              new ActionSetType("975f4305-60b8-46fd-acf0-e36dc7fd922f"){   
                                  MaxFirings = 1,
                                  Condition = new PlayerAllegiancePersons(){ MinMembers = 2 }                                 
                             ,
                                 Actions = new EventActionType[]{ 
                    
                     new TalkAction("a48fec44-11b9-4b31-ba1f-62c215ed1d0c") { DelayInSeconds = 1, //mp to help it getting executed without being blocked by the preceding dialogue
                     
                        TalkPriority = TalkAction.TalkActionPriority.High,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = true,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                        ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                        DefaultText = "FIRE IN THE HOLE!" },

                    new TalkAction("6e0d76c6-445d-4c2b-9343-ecf78e26bfba") { DelayInSeconds = 6, 
                     
                        TalkPriority = TalkAction.TalkActionPriority.High,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = false,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                        ActionByAgent = ActionByAgent.PreferTriggeringEntity,
                        DefaultText = "Sensor picks up very few life signs from underground. Think we got them!" },

                     new TalkAction("cb6946f5-c14f-4571-bec4-12d89fca9ff2") { DelayInSeconds = 9, 
                     
                        TalkPriority = TalkAction.TalkActionPriority.High,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = false,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                        ActionByAgent = ActionByAgent.RandomInAllegiance,
                        DefaultText = "Well done!" },
                    
                             }} 

                         }
            });
            #endregion


            #region field lab

            list.Add(new ActionSets()
            {
                KeyName = "cannibalizeFieldLabSetProperty",
                FireMode = ActionSetsToFire.AllValid,
                SetsOfActions = new []{

                    new ActionSetType("ab9afbwrtyyyyyyyyyyyyyrwtyrwtywrtyrytw62a8"){   Actions = new EventActionType[]{ 

                    new SetPropertyAction("0d3dgggggggg462546246245gdghjdghjdgj064")
                    {  
                        DelayInSeconds = 1f,
                        
                            PropertyKey = "fieldLabCannibalized",// to avoid the event screen about making the liquid gun from field lab if the player is already doing it.
                            Value = new ValueNode() { Bool = true }
                        }                                   
                                  
                    },
                     
                    
                             }
                         }
            });



            list.Add(new ActionSets()
            {
                KeyName = "cannibalizeFieldLabRemark",
                //  once the remark actionset is set to fire, it chooses a line from the below which each have an equal chance of being selected
                FireMode = ActionSetsToFire.RandomValid,
                ChanceToFire = 1f, //the chance for the set to fire, then chooses from below
                SetsOfActions = new []{ new ActionSetType("a6wrtyuyyyyyyyyyyhugfshsfghsfghsfgh3d")
                        {    
                      //      ChanceToFire = 0.5f,
                            MaxFirings = 1,
                             Condition = new PlayerAllegiancePersons(){ MinMembers = 2 }                                 
                             ,
                            Actions = new EventActionType[]{new TalkAction("c2109f3bdgjdhdghjdghjdgjdgjdj138c99") {  
                       
                        TalkPriority = TalkAction.TalkActionPriority.High,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = true,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                        ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                        DefaultText = "This was the right decision. We can make a powerful weapon with these parts." }, 

                    new TalkAction("dc7bd45555555555552254624565469c0") { DelayInSeconds = 4, 
                     
                        TalkPriority = TalkAction.TalkActionPriority.High,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = true,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                        ActionByAgent = ActionByAgent.RandomInAllegiance,
                        DefaultText = "Hope you're right." },
                    
                            }
                        }}
            });


            #endregion

            #region retrieve crates


            list.Add(new ActionSets()
            {
                KeyName = "startRetrieveCratesRemark",
                //  once the remark actionset is set to fire, it chooses a line from the below which each have an equal chance of being selected
                FireMode = ActionSetsToFire.RandomValid,
                ChanceToFire = 1f, //the chance for the set to fire, then chooses from below
                SetsOfActions = new []
                {
                    new ActionSetType("a68fdd08-41ba-456e-8665-9c99c0e6a83d")
                    {
                        MaxFirings = 1,
                        Condition = new PlayerAllegiancePersons(){ MinMembers = 2 }
                        ,
                        Actions = new EventActionType[]
                        {
                            new TalkAction("c2109f3b-96cf-48a2-933a-4b5392138c99")
                            {
                                
                                    TalkPriority = TalkAction.TalkActionPriority.High,
                                    CanTalkWhileFighting = false,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = true,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                                    ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                    DefaultText = "I'm giving this a try now."
                                
                            },
                            new TalkAction("dc7beaae-926d-4fa4-86d3-5b64bca5c9c0")
                            {
                                DelayInSeconds = 4,
                                
                                    TalkPriority = TalkAction.TalkActionPriority.High,
                                    CanTalkWhileFighting = false,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = true,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                                    ActionByAgent = ActionByAgent.RandomInAllegiance,
                                    DefaultText = "Take care."
                                
                            },
                        }
                    },
                    new ActionSetType("8738c60a-7194-439a-86f1-87b8fab427bc")
                    {
                        MaxFirings = 1,
                        Condition = new PlayerAllegiancePersons(){ MaxMembers = 1 }
                        ,
                        Actions = new EventActionType[]
                        {
                            new TalkAction("aae85848-b663-41ea-8d41-4ffb6600ee43")
                            {
                                
                                    TalkPriority = TalkAction.TalkActionPriority.High,
                                    CanTalkWhileFighting = false,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = true,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                                    ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                    DefaultText = "I hope this is worth it..."
                                
                            }
                        }
                    }
                }
            });



            list.Add(new ActionSets()
            {
                KeyName = "endRetrieveCrates",
                SetsOfActions = new []
                {
                    new ActionSetType("74dddd95-cfa9-43e1-a4f5-7e499893dfde")
                    {
                        Actions = new EventActionType[]{
                            new DestroyEntityAction("ec3cf9d4-5a1d-4a15-855b-892bcabbadbd")
                            {
                                DelayInSeconds = 5f,
                                
                                 TargetObject = new TargetObject()
                                 {
                                      TargetObjectType = TargetObjectType.TargetEntity
                                 }
                       // EntityToDestroy = TargetEntityOfAction.TargetEntity
                                
                            },
     /*               new EventActionType("c2764572-5b34-4850-b5ae-a2686c547fff")
                    {  
                        DelayInSeconds = 5f,
                        
                            PropertyKey = "cratesRetrieved",//NOT USED dec 2014. to avoid exposition about how to retrieve a crate once the player has learned this
                            Value = new ValueNode() { Bool = true }
                        }                                   
                                  
                    },
    */                            
                            new SpawnEntityAction("969dbaf3-2cee-43da-b447-7b76958766db")
                            {
                                DelayInSeconds = 5f,
                                
                                    EntityData = new EntityData()
                                    {
                                        EntityKey = "item:coilRifle",
                                        OwnedBy = new AllegianceAndExpedition() { AllegianceKey = "playerAllegiance" },
                                        Location = new Vector3(520, 1402, 0)
                                    },
                                
                            },
                            new SpawnEntityAction("afb8d424-1a8e-4cc1-b51a-c017f962f5e6")
                            {
                                DelayInSeconds = 5f,
                                
                                    EntityData = new EntityData()
                                    {
                                        EntityKey = "item:coilRifleAmmo",
                                        OwnedBy = new AllegianceAndExpedition() { AllegianceKey = "playerAllegiance" },
                                        Location = new Vector3(520, 1402, 0)
                                    },
                                
                            },
                            new SpawnEntityAction("f9dc52ec-db36-425c-bd1f-fab718f20b35")
                            {
                                DelayInSeconds = 5f,
                                EntityData = new EntityData()
                                {
                                    EntityKey = "item:sensor",
                                    OwnedBy = new AllegianceAndExpedition() { AllegianceKey = "playerAllegiance" },
                                    Location = new Vector3(520, 1402, 0)
                                },
                                
                            },
                        }
                    }
                }
            });


            list.Add(new ActionSets()
            {
                KeyName = "endRetrieveCratesRemark", // mp this rarely fires, why is that. NOEXEC.  //take care that the conversation before is over before this one is called.
                //  once the remark actionset is set to fire, it chooses a line from the below which each have an equal chance of being selected
                FireMode = ActionSetsToFire.RandomValid,
                ChanceToFire = 1f, //the chance for the set to fire, then chooses from below
                SetsOfActions = new []
                {
                    new ActionSetType("67a56111-daasf32500-4cba-85523asf70-6019738c6e73")
                    {
                        MaxFirings = 1,
                        Condition = new PlayerAllegiancePersons(){ MinMembers = 2 }
                        ,
                        Actions = new EventActionType[]
                        {
                            new TalkAction("0a4d5f38-21afe6-45be-993b-0b7257fddcaf")
                            {
                                DelayInSeconds = 3,
                                
                                    TalkPriority = TalkAction.TalkActionPriority.High,
                                    CanTalkWhileFighting = false,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = true,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                                    ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                    DefaultText = "I got it."
                                
                            },
                            new TalkAction("643a7sda25f9a-afb8-44e0-8cbd-e8601b21e748")
                            {
                                DelayInSeconds = 6,
                                
                                    TalkPriority = TalkAction.TalkActionPriority.High,
                                    CanTalkWhileFighting = false,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = false,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                                    ActionByAgent = ActionByAgent.PreferTriggeringEntity,
                                    DefaultText = "Well well, look at this."
                                
                            },
                            new TalkAction("0516asd557df8-f69d-4e1b-bca5-ccc6e3ea23ba")
                            {
                                DelayInSeconds = 9,
                                
                                    TalkPriority = TalkAction.TalkActionPriority.High,
                                    CanTalkWhileFighting = false,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = false,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                                    ActionByAgent = ActionByAgent.RandomInAllegiance,
                                    DefaultText = "Great work. This stuff will come in handy."
                                
                            },
                        }
                    },
                    new ActionSetType("4b08023523740-a6sa58b-4f5d-9557-a7c2f30fa959")
                    {
                        MaxFirings = 1,
                        Condition = new PlayerAllegiancePersons(){ MaxMembers = 1 } //when only one.
                        ,
                        Actions = new EventActionType[]
                        {
                            new TalkAction("2dd13f9c-ebef-4611-a60e-e326e50fb987")
                            {
                                DelayInSeconds = 3,
                                
                                    TalkPriority = TalkAction.TalkActionPriority.High,
                                    CanTalkWhileFighting = false,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = true,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                                    ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                    DefaultText = "Yes..."
                                
                            },
                            new TalkAction("046b4f51-1616-43cd-9977-7167de1f5206")
                            {
                                DelayInSeconds = 6,
                                
                                    TalkPriority = TalkAction.TalkActionPriority.High,
                                    CanTalkWhileFighting = false,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = false,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                                    ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                    DefaultText = "Ho ho ho...this is all for me."
                                
                            }
                        }
                    }
                }
            });

            #endregion

/////////////
            #region rescue team member


            list.Add(new ActionSets()
            {
                KeyName = "startRescueColleagueRemark",
                //  once the remark actionset is set to fire, it chooses a line from the below which each have an equal chance of being selected
                FireMode = ActionSetsToFire.RandomValid,
                ChanceToFire = 1f, //the chance for the set to fire, then chooses from below
                SetsOfActions = new []
                {
                    new ActionSetType("a3567777wywrtyyyyyyyyyyy83d")
                    {
                        MaxFirings = 1,
                        Condition = new PlayerAllegiancePersons(){ MinMembers = 2 }
                        ,
                        Actions = new EventActionType[]
                        {
                            new TalkAction("csrtyyyyyyyyy5555555555555559")
                            {
                                
                                    TalkPriority = TalkAction.TalkActionPriority.High,
                                    CanTalkWhileFighting = false,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = true,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                                    ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                    DefaultText = "I'm going down there."
                                
                            },
                        }
                    },
                    new ActionSetType("8srtyyyyyyyyyyyyyyygyfhxgfxhxdfghfshc")
                    {
                        MaxFirings = 1,
                        Condition = new PlayerAllegiancePersons(){ MaxMembers = 1 }
                        ,
                            Actions = new EventActionType[]
                            {
                                new TalkAction("aasgfhfghrwshtgfxdhxfhxfh3")
                                {
                                    
                                    TalkPriority = TalkAction.TalkActionPriority.High,
                                    CanTalkWhileFighting = false,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = true,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                                    ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                    DefaultText = "I'm coming...hang on."
                                    
                                }
                            }
                    }
                }
            });



            list.Add(new ActionSets()
            {
                KeyName = "endRescueColleague", //
                SetsOfActions = new []
                {
                    new ActionSetType("74etyuty6drtyuuuuuudfudgfhe")
                    {
                        Actions = new EventActionType[]
                        {
                            new DestroyEntityAction("ecdyuuuuuuuuhjhdddgjdgjjdjd")
                            {
                                DelayInSeconds = 0f,
                                
                                    TargetObject = new TargetObject()
                                    {
                                        TargetObjectType = TargetObjectType.TargetEntity
                                    }
                                
                            },
   /*                 new EventActionType("cdgggggggggggjtytytytytytytytytytytytyjghcf")
                    {  
                        DelayInSeconds = 5f,
                        
                            PropertyKey = "cratesRetrieved",// to avoid exposition about how to retrieve a crate once the player has learned this
                            Value = new ValueNode() { Bool = true }
                        }                                   
                                  
                    },
    */                            
                       

                            new SpawnEntityAction("f9dc2444444444445547276567567563335")
                            {
                                DelayInSeconds = 5f,
                               
                                    DynamicLocation = new DynamicLocation()
                                    {
                                        PropertyKey = "rescueSpawnLocation" //480, 1392 sandstone. make one for sandstone and one for bramble. make sure spawn point has a good distance from blocked subtiles else will not spawn.
                                    },
                                    EntityData = new EntityData()
                                    {
                                        Location = new Microsoft.Xna.Framework.Vector3(0, 0, 0), // remove this, not used.
                                        EntityKey = "entity:human",
                                        MemberOf = new AllegianceAndExpedition()
                                        {
                                            AllegianceKey = "playerAllegiance",
                                            ExpeditionKey = "Camp"
                                        },
                                       
                                        Person = new Maps.MapEditor.Person()
                                        {
                                            FirstName = "Sefu",
                                            LastName = "Laurent", //"Azat"
                             //               PersonalityType = "Conlan",
                                            Portrait = "human_b_m_adult_1",
                                             SimulateJoinedExpeditionNow = true,
                                        },
                                        BioEntity = new Maps.MapEditor.BiologicalEntity() //mp would be nice if he had some injuries when he spawns, but do not know how to.
                                        {
                                            AgeInYears = new NormalDistribution() { Mean = 42f },
                                            CasteKey = "male",
                                            RaceKey = "blue2",

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

                                                { "farming", 0.4f},
                                                { "weeding", 0.4f},
                                                { "grasping", 0.8f},
                                                { "fruitPicking", 0.8f}, 
                                                { "construction", 0.5f}, 
                                                { "archery", 0.5f},   
                                                { "medicine", 0.5f},      
                                                { "sneaking", 0.5f},  
                                                { "smithing", 0.5f},
                                                 //new
                                                 { "mechanics", 0.5f},
                                                 { "electronics", 0.5f},
                                                 { "chemistry", 0.6f},
                                                 { "weaving", 0.18f},
                                                 { "carpentry", 0.24f},
                                                 //                                                 
                                            }                                             
                                        },
                                        NeedLevels = new SerializableDictionary<string, NeedData>() 
                                        {
                                            { "protein", new NeedData(){ Level = new NormalDistribution() { Mean = 0.3f, StandardDeviation = 0.02f } }},
                                            { "foodEnergy", new NeedData(){ Level = new NormalDistribution() { Mean = 0.2f, StandardDeviation = 0.02f } }},
                                            { "micronutrients", new NeedData(){ Level = new NormalDistribution() { Mean = 0.2f, StandardDeviation = 0.02f } }},
                                            { "stimulants", new NeedData(){ Level = new NormalDistribution() { Mean = 0.2f, StandardDeviation = 0.02f } }}

                                        }
                                    }
                                
                            },
                            new SetPropertyAction("sdfgdfsghgfshgfshfsg9878979879b")
                            {
                                DelayInSeconds = 0,
                                
                                    PropertyKey = "unconsciousAndAlive",
                                    Value = new ValueNode() { Bool = false }
                                  //to avoid firing "DEMOISLANDMAP_unconsciousDies"
                            },
                            new SetPropertyAction("steteteteteteteteteteteteyuub")
                            {
                                DelayInSeconds = 10, //giving it a delay so there's time to settle down.
                                
                                    PropertyKey = "casualtyRescued",
                                    Value = new ValueNode() { Bool = true }
                                  //for  firing debriefing talk.
                            },
                        }
                    }
                }
            });


            list.Add(new ActionSets()
            {
                KeyName = "endRescueColleagueRemark", // 
                //  once the remark actionset is set to fire, it chooses a line from the below which each have an equal chance of being selected
                FireMode = ActionSetsToFire.RandomValid,
                ChanceToFire = 1f, //the chance for the set to fire, then chooses from below
                SetsOfActions = new []{

                             new ActionSetType("67a561asf53211-da00-4cba-8570-6019738c6e73"){   
                                  MaxFirings = 1,
                                          Condition = new PlayerAllegiancePersons(){ MinMembers = 2 }                                 
                             ,
                                 Actions = new EventActionType[]{ 
                    
                     new TalkAction("0a4d5f38-21e6-45253235asfbe-993b-0b7257fddcaf") { DelayInSeconds = 1, //take care that the conversation before is over before this one is called.
                     
                        TalkPriority = TalkAction.TalkActionPriority.High,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = true,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                        ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                        DefaultText = "Hey Sefu, hang in there." },


                    new EventActionDialog("5425555555555556rwtyrtwyrtwyrtw651") { DelayInSeconds = 4, 
                        DisplayText = new DynamicText(){ Text = "//AUDIO LOG// \n \n-I got him. I'm resupplying his medic unit with more nanobots now. Shouldn't take long before we see an improvement. \n-Yep, they are doing their job. His vitals are already looking better. \nAlright, I'm waking him up." ,  //  todo text
                        },
                  //      DisplayImage = "GroupMeeting",
                                             
                                }, 

                    new TalkAction("643afa7f9a-afasf25b8-44e0-8cbd-e8601b21e748") { DelayInSeconds = 5, 
                     
                        TalkPriority = TalkAction.TalkActionPriority.High,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = true,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                        ActionByAgent = ActionByAgent.OnlySpecific,
                        NameOfSpeaker = "Sefu Laurent", //if person's name is changed, remember to change it here too.
                        DefaultText = "Oh man, am I glad to see you. What happened?" }, //mp didnt fire. no speaker found.

                     new TalkAction("05167df8-f69d-4e1b-bca5-cc235sdgc6e3ea23ba") { DelayInSeconds = 7, 
                     
                        TalkPriority = TalkAction.TalkActionPriority.High,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = true,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                        ActionByAgent = ActionByAgent.RandomInAllegiance,
                        DefaultText = "You fell out during the crash. Good to have you back." },
                    
                             }},


                              new ActionSetType("4b080740-a68b-4f5d-955asf7-a7c2f30f235a959"){   
                                  MaxFirings = 1,
                                  Condition = new PlayerAllegiancePersons(){ MaxMembers = 1 } //not sure if this will work with the new member appearing. I have a delay on the guy appearing, maybe that can solve it.
                                 
                             ,
                                 Actions = new EventActionType[]{ 
                    
                     new TalkAction("2eeeeeeeeeeeeeeeee535673567368387") { DelayInSeconds = 3, 
                     
                        TalkPriority = TalkAction.TalkActionPriority.High,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = true,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                        ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                        DefaultText = "I got you buddy." },


                    new EventActionDialog("54255sfghhhhhhhhhhsfghsfghrtw651") { DelayInSeconds = 4, 
                         DisplayText = new DynamicText(){ Text = "//AUDIO LOG// \n \n-I got him. I'm resupplying his medic unit with more nanobots now. Shouldn't take long before we see an improvement... \n-Yep, they are doing their job. His vitals are already looking better. \nAlright, I'm waking him up." ,  //  todo text
                        },
                  //      DisplayImage = "GroupMeeting",
                                              
                    }, 
                    new TalkAction("036767676767676767676767676767eeie6") { DelayInSeconds = 6, 
                     
                        TalkPriority = TalkAction.TalkActionPriority.High,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = true,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                        ActionByAgent = ActionByAgent.OnlySpecific,
                        NameOfSpeaker = "Sefu Laurent", //if person's name is changed, remember to change it here too.
                        DefaultText = "Oh man, am I glad to see you. What happened?" },

                   new TalkAction("0463567777777777777775378738306") { DelayInSeconds = 9, 
                     
                        TalkPriority = TalkAction.TalkActionPriority.High,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = true,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                        ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                        DefaultText = "You fell out during the crash. Good to have you back." },
                    
                             }} 

                         }
            });






            list.Add(new ActionSets()
            {
                KeyName = "casualtyRescuedDebriefing",
                FireMode = ActionSetsToFire.AllValid,
                SetsOfActions = new []{ new ActionSetType("4a07dfdfdfdfdfdfdfdfdfgjjdgjdgjb6")
                    {     
                            Condition = new PlayerAllegiancePersons(){ MinMembers = 2 }                                
                            , 
                    MaxFirings = 1,
                    Actions = new EventActionType[]{   

                           

                                new TalkAction("b2a03562-8ff2-4420-9ad7-c62a2323525af-253325a191bc") { DelayInSeconds = 3,                        
                                     
                                    ActionByAgent = ActionByAgent.OnlySpecific,
                                    NameOfSpeaker = "Sefu Laurent", //if person's name is changed, remember to change it here too.
                                    TalkPriority = TalkAction.TalkActionPriority.High,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                                    CanTalkWhileFighting = false,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = true,
                                     TurnTowardsListeners = true,
                                    DefaultText = "Still feeling a bit groggy, but I'm getting better." }     //it is not possible to have an agent ask him something, because cannot define agents different from him.                         
                                    ,

                                new EventActionDialog("5wrtyyyyyyyyyyyyyrtyrtwyrwty51") { DelayInSeconds = 6,                                 
                                 DisplayText = new DynamicText(){ Text = "//AUDIO LOG - MEETING \n \n-Glad to have you back Laurent. Ok everyone - what is our next priority? \n-Well, it could take a while before we are rescued. Our biggest challenges will be food and twinkler attacks. We have to build up food stores while keeping an eye on the twinklers. \n-There's bound to be trouble. Twinklers have lairs scattered all over the island. I suggest we make plans for eradicating the nests. \n-Hmm. We'll have to make some decisions soon." ,  //  
                                    },
                                    DisplayImage = "GroupMeeting"
                      					
                                },

                    } },
                }
            });








            #endregion




            #region build rope bridge


            list.Add(new ActionSets()
            {
                KeyName = "startBuildRopeBridgeRemark",

                ChanceToFire = 1f, //the chance for the set to fire, then chooses from below
                SetsOfActions = new []{ new ActionSetType("5rwtyyyyyyyyyyyyyyyuytuetytydytudyu33")
                        {    
                      //      ChanceToFire = 0.5f,
                            MaxFirings = 1,
                             Condition = new PlayerAllegiancePersons(){ MinMembers = 1 }                                 
                             ,
                            Actions = new EventActionType[]{new TalkAction("5eeeeeeeeeeeeeeeeedtyudtyudyguytujdyudj3") {  
                       
                        TalkPriority = TalkAction.TalkActionPriority.High,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = true,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                        ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                        DefaultText = "This shouldn't be too difficult..." }, 

                    
                            }
                        }

            }
            });



            list.Add(new ActionSets()
            {
                KeyName = "ropeBridgeFinished", // 

                SetsOfActions = new []{

                    new ActionSetType("c4ceeeeeeeeeeeeeeeeeeeyueyuetyuetyu0019"){   Actions = new EventActionType[]{ 
                    new DestroyEntityAction("26eeeeeeeeeeeeeeeeutyuuiyuiryuid4")
                    {  // destroys the blocker after 4 s
                        DelayInSeconds = 4f,
                        
                             TargetObject = new TargetObject()
                             {
                                  TargetObjectType = TargetObjectType.TargetEntity
                             }
                    
                    },
                    new SetPropertyAction("6ryuuuuuuuuuiyuiryuotioriobbb")
                    {  // sets a propertykey after xf sec
                        DelayInSeconds = 2f,
                        
                            PropertyKey = "bridgeBuilt", // mp: not used?!                    
                            Value = new ValueNode() { Bool = true }
                                                          
                                  
                    },
                               
                    new SpawnEntityAction("bdtuiiiiiiiiiothfjjjjjjjjjjjjjkfkkf4")
                    { DelayInSeconds = 0f,
                        EntityData = new EntityData()
                            { EntityKey = "terrain:ropeBridge",  Location = new Vector3(1428, 1871, 0) //
                            }, },                  
                 
                             }} 
                         }
            });


            list.Add(new ActionSets()
            {
                KeyName = "ropeBridgeFinishedRemark", //  
                FireMode = ActionSetsToFire.RandomValid,
                SetsOfActions = new []{

                             new ActionSetType("5fhjjjjjjjjjjjjjkjhlkjglgkjlgjklgkjl8"){   
                                  MaxFirings = 1,
                                          Condition = new PlayerAllegiancePersons(){ MinMembers = 2 }                                 
                             ,
                                 Actions = new EventActionType[]{ 
                    
                     new TalkAction("3b1gjkllllllllllllllgjklgkjlgjklgjklgkja") {   DelayInSeconds = 1, //give some time so that the previous dialogue does not force a NO EXEC
                     
                        TalkPriority = TalkAction.TalkActionPriority.High,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = true,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                        ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                        DefaultText = "Done. We can get past now." },

                    
                             }},      

                         }
            });


            #endregion




            #region triggered exposition talk


            list.Add(new ActionSets()
            {
                KeyName = "talkCrystalBerries",
                FireMode = ActionSetsToFire.FirstValid,
                SetsOfActions = new []
                {
                    new ActionSetType("9b4b5123-a33c-4cf1-81ef-8b7f007e46bf")
                    {
                        MaxFirings = 1,
                        Condition = new PlayerAllegiancePersons(){ MinMembers = 2 }
                        ,
                        Actions = new EventActionType[]
                        {
                            new TalkAction("3b18023781-778a-4848-a460-b6271fdsffaaa90")
                            {
                                DelayInSeconds = 0,
                                
                                    TalkPriority = TalkAction.TalkActionPriority.Low,
                                    CanTalkWhileFighting = false,
                                    CanTalkWhileThreatened = false,
                                    CanTalkWhileSleeping = false,
                                    ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                                    DefaultText = "I've found a species of crystal berries here. Didn't know they grow in this biome."
                                
                            },
                            new TalkAction("b183781-778a-4848-a460-b6271faaa90")
                            {
                                DelayInSeconds = 4,
                                
                                    TalkPriority = TalkAction.TalkActionPriority.Low,
                                    CanTalkWhileFighting = false,
                                    CanTalkWhileThreatened = false,
                                    CanTalkWhileSleeping = false,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                                    ActionByAgent = ActionByAgent.RandomInAllegiance,
                                    DefaultText = "Yes, they do, and we could even grow them."
                                
                            }
                        }
                    },/*
                    new ActionSetType("9b4b5123-a33c-4cf1-81ef-8b7f007e46bf")
                    {
                        MaxFirings = 1,
                        Condition = new ConditionSet()
                        {
                            Value = new PlayerAllegiancePersons(){ MinMembers = 2 }
                        },
                        Actions = new EventActionType[]
                        {
                            new EventActionType("3b180781-778a-4848-a460-b6271ffaaa90")
                            {
                                DelayInSeconds = 0,
                                
                                    TalkPriority = TalkAction.TalkActionPriority.High,
                                    CanTalkWhileFighting = false,
                                    CanTalkWhileThreatened = false,
                                    CanTalkWhileSleeping = false,
                                    ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                    DefaultText = "Interesting. I found a species of crystal berries. Didn't know they grew in this biome. They can be an important supplement to our diet."
                                }
                            }
                        }
                    }*/
                }
            });



            list.Add(new ActionSets()
            {
                KeyName = "talkVolcanicLandscape",
                SetsOfActions = new []{ new ActionSetType("4194c680-e519-4c0d-a92e-2d1cdccf4b53")
                    {   MaxFirings = 1,
                        Condition = new PlayerAllegiancePersons(){ MinMembers = 2 },
                    Actions = new EventActionType[]{new TalkAction("621bb152-d1c9-4685-b24a-6d59af092f5c") { DelayInSeconds = 0, 
                    TalkPriority = TalkAction.TalkActionPriority.High,
                       CanTalkWhileFighting = false, 
                       CanTalkWhileThreatened = false, 
                       CanTalkWhileSleeping = false,
                       SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                    ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                    DefaultText = "This is an active volcanic landscape." },
                    
                      new TalkAction("671f5772-c20f-49ab-ae76-aefc7faf5d34") { DelayInSeconds = 4, 
                      TalkPriority = TalkAction.TalkActionPriority.High,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = false,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.Second, 
                        ActionByAgent = ActionByAgent.RandomInAllegiance,
                        DefaultText = "Yeah, I can see you got some rich air readings...got your mask on?" },
                    
                      new TalkAction("0040ef65-5b81-4399-8a2f-13e535f91bdf") { DelayInSeconds = 8, 
                       TalkPriority = TalkAction.TalkActionPriority.High,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = false,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.First, 
                        ActionByAgent = ActionByAgent.RandomInAllegiance,
                        DefaultText = "You bet." }  
                    }
                        }
                    }
            });


            list.Add(new ActionSets()
            {
                KeyName = "talkMuckroot",
                SetsOfActions = new []{ new ActionSetType("67046612-f1df-4f78-bd40-132a4b8c53ce")
                    {   MaxFirings = 1,
                        Condition = new PlayerAllegiancePersons(){ MinMembers = 2 },
                    Actions = new EventActionType[]{new TalkAction("41c7300f-88ef-4474-9286-eea546e93217") { DelayInSeconds = 0, 
                    TalkPriority = TalkAction.TalkActionPriority.Normal,
                       CanTalkWhileFighting = false, 
                       CanTalkWhileThreatened = false, 
                       CanTalkWhileSleeping = false,
                       SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                    ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                    DefaultText = "A patch of wetland here." },
                    
                      new TalkAction("481f79e4-643e-4e3b-bfd3-603253a44030") { DelayInSeconds = 3.5, 
                      TalkPriority = TalkAction.TalkActionPriority.Normal,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = false,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.Second, 
                        ActionByAgent = ActionByAgent.RandomInAllegiance,
                        DefaultText = "Worth investigating?" },
                    
                      new TalkAction("50dd63b6-e93f-4a2b-9d4a-94998150ef45") { DelayInSeconds = 7, 
                       TalkPriority = TalkAction.TalkActionPriority.Normal,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = false,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.First, 
                        ActionByAgent = ActionByAgent.RandomInAllegiance,
                        DefaultText = "Yeah, this kind of wetland can have resources not found elsewhere." } 
                    }
                        }
                    }
            });




            list.Add(new ActionSets()
            {
                KeyName = "talkMarsh",
                SetsOfActions = new []{ new ActionSetType("f8f35aae-6086-448a-a196-9e3b186fc04a")
                    {   MaxFirings = 1,
                        Condition = new PlayerAllegiancePersons(){ MinMembers = 2 },
                    Actions = new EventActionType[]{new TalkAction("e7a056b3-8894-4e21-b301-d3c40a70ad2f") { DelayInSeconds = 1, 
                    TalkPriority = TalkAction.TalkActionPriority.High,
                       CanTalkWhileFighting = false, 
                       CanTalkWhileThreatened = false, 
                       CanTalkWhileSleeping = false,
                    ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                    DefaultText = "A marsh. Quadites are usually not seen in such a landscape." }}}}
            });

            list.Add(new ActionSets()
            {
                KeyName = "talkEmptyQuaditeCrevice",
                SetsOfActions = new []{ new ActionSetType("04aa1296-5e56-45bc-8ac7-155f764cad88")
                    {   MaxFirings = 1,
                        Condition = new PlayerAllegiancePersons(){ MinMembers = 2 },
                    Actions = new EventActionType[]{new TalkAction("45affe27-07b5-417f-bdeb-e860f3f2d9c7") { DelayInSeconds = 1, 
                    TalkPriority = TalkAction.TalkActionPriority.High,
                       CanTalkWhileFighting = false, 
                       CanTalkWhileThreatened = false, 
                       CanTalkWhileSleeping = false,
                    ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                    DefaultText = "This crevice here... it's evidently been used by quadites as a nest...but it seems to be abandoned now." }}}}
            });
/*bso delete
            list.Add(new ActionSets()
            {
                KeyName = "talkSouthSandstoneCave",
                SetsOfActions = new []{ 
                    new ActionSetType("f0ce690d-67ab-40a9-85ac-f61046c03088")
                    {   MaxFirings = 1,
                        Condition = new ConditionSet()
                             {
                                  Value = new PlayerAllegiancePersons(){ MinMembers = 3 }},
                    Actions = new EventActionType[]{new EventActionType("f61fdf2f-f5ff-4428-85d1-bbcebb66f4b5") { DelayInSeconds = 0, 
                    TalkPriority = TalkAction.TalkActionPriority.High,
                       CanTalkWhileFighting = false, 
                       CanTalkWhileThreatened = false, 
                       CanTalkWhileSleeping = false, 
                       SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                    ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                    DefaultText = "Guys...I'm getting some sensor readings here...I think quadites have a nest nearby." }},
                    
                      new EventActionType("49c57a3e-b93b-4676-a4da-1baf353e846d") { DelayInSeconds = 4, 
                     TalkPriority = TalkAction.TalkActionPriority.High,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = false,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.Second, 
                        ActionByAgent = ActionByAgent.RandomInAllegiance,
                        DefaultText = "Don't go near it alone!" }}}
                        }
                    }
            });*/

            list.Add(new ActionSets()
            {
                KeyName = "talkNorthRockCrevice",
                SetsOfActions = new []{ new ActionSetType("3f119689-3dbb-4b53-9d97-31ff1360ba06")
                    {   MaxFirings = 1,
                        Condition = new PlayerAllegiancePersons(){ MinMembers = 2 },
                    Actions = new EventActionType[]{new TalkAction("00e57b0d-47c7-49d3-9d6f-e494a475868c") { DelayInSeconds = 0, 
                    TalkPriority = TalkAction.TalkActionPriority.High,
                       CanTalkWhileFighting = false, 
                       CanTalkWhileThreatened = false, 
                       CanTalkWhileSleeping = false,
                    ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                    DefaultText = "Another crevice. Wonder if it could house anything living. Has to be resistant to volcanic gas." }}}}
            });





            #endregion




            #region Detect creatures

            list.Add(new ActionSets()
            {
                KeyName = "detectTwinkler", //not for fled at start scenario. they already met and know what a twinkler is
                SetsOfActions = new []{ new ActionSetType("c1fae78f-7f66-4cd1-89ac-8ac9c1a5867c")
                    {     
                       Condition = new ConditionFunction()
                        {
                            Operator = OperatorType.And,
                            Left = new PlayerAllegiancePersons(){ MinMembers = 2 },
                            Right = new CustomCondition()
                            {
                                PropertyCondition = new PropertyCondition()
                                {
                                    PropertyKey = "fledAtStart",
                                    BoolValue = false
                                }
                            }
                    },  
                    MaxFirings = 1,
                    Actions = new EventActionType[]{
                        new TalkAction("bea4dada-0c1e-4ac8-9705-4db755f14b82") { DelayInSeconds = 0,                        
                                     
                                    ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                    TalkPriority = TalkAction.TalkActionPriority.High,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                                    CanTalkWhileFighting = true,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = true,
                                    DefaultText = "Quadite spotted! Not a Swarmer though. It's one of the slower species." },

                                new TalkAction("2987d374-f76a-4372-b1b4-bda5e416e9f5") { DelayInSeconds = 3.5,                        
                                     
                                    ActionByAgent = ActionByAgent.RandomInAllegiance,
                                    TalkPriority = TalkAction.TalkActionPriority.High,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                                    CanTalkWhileFighting = true,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = true,
                                    DefaultText = "Yeah - looks like a Twinkler to me. Be wary!" }
                                    
       
                    
                    }
                            }}
            });

            list.Add(new ActionSets()
            {
                KeyName = "detectWhiteThunderChicken", //not for fled at start scenario. they are in urgency.
                SetsOfActions = new []{ new ActionSetType("a461877b-340f-476e-a5d2-ab1d0473fc6d")
                        {     
                     Condition = new ConditionFunction()
                        {
                            Operator = OperatorType.And,
                            Left = new PlayerAllegiancePersons(){ MinMembers = 2 }
                            ,
                            Right = new CustomCondition()
                            {
                                PropertyCondition = new PropertyCondition()
                                {
                                    PropertyKey = "fledAtStart",
                                    BoolValue = false
                                }  
                            }
                        },  
                        MaxFirings = 1,
                        Actions = new EventActionType[]{ new TalkAction("fd7e712b-e02e-4334-a477-ace2d001fe50") {   
                        DelayInSeconds = 0,
                            
                        TalkPriority = TalkAction.TalkActionPriority.Low,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = false,
                        ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                        DefaultText = "Look, thunder chickens!" }, 

                    new TalkAction("32136071-4a6a-4544-98c2-79950b101a97") { DelayInSeconds = 2, 
                     
                        TalkPriority = TalkAction.TalkActionPriority.Low,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = false,
                        ActionByAgent = ActionByAgent.RandomInAllegiance,
                        DefaultText = "Well, at least we have that. They can make a nice meal." }}
                    }
                    }

            });

            list.Add(new ActionSets()
            {
                KeyName = "detectPygmyThunderChicken", //not for fled at start scenario. they are in urgency.
                SetsOfActions = new []{ new ActionSetType("591a8644-5d08-484b-83ed-0777bb928283")
                        {     
                            Condition = new ConditionFunction()
                            {
                                Operator = OperatorType.And,
                                Left = new PlayerAllegiancePersons(){ MinMembers = 2 }
                                ,
                                Right = new CustomCondition()
                                {
                                    PropertyCondition = new PropertyCondition()
                                    {
                                        PropertyKey = "fledAtStart",
                                        BoolValue = false
                                    }
                            }},  
                        MaxFirings = 1,
                        Actions = new EventActionType[]{ new TalkAction("636c632a-e0e2-4997-a6ca-ee80284177a7") {   
                        DelayInSeconds = 0,
                            
                        TalkPriority = TalkAction.TalkActionPriority.Low,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = false,
                        ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                        DefaultText = "There's one of these smaller ones...what do we call them?" }, 


                    new TalkAction("5cf1dac6-5a3a-4f9d-b8a5-bce248b2ce5e") { DelayInSeconds = 3, 
                     
                        TalkPriority = TalkAction.TalkActionPriority.Low,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = false,
                        ActionByAgent = ActionByAgent.RandomInAllegiance,
                        DefaultText = "Pygmy thunder chickens." }

                        }
                    }
                    }

            });




            #endregion

            #region Bush dragon + field lab exposition

            list.Add(new ActionSets()
            {
                KeyName = "detectBushDragonRemark",
                SetsOfActions = new []{ new ActionSetType("e3a0237d-6b1e-4551-8e00-6ec3be934270")
                        {     
                             Condition = new PlayerAllegiancePersons(){ MinMembers = 2 }                                 
                             , 
                        MaxFirings = 1,
                        Actions = new EventActionType[]{ new TalkAction("15514474-022c-40c3-89da-c93f374e8a98") {   
                        DelayInSeconds = 0,
                            
                        TalkPriority = TalkAction.TalkActionPriority.High,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = true,
                        ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                        DefaultText = "Bush dragons." }, 

                    new TalkAction("b0b78a95-9625-44d8-bc68-5ccb9700edc6") { DelayInSeconds = 2.5, 
                     
                        TalkPriority = TalkAction.TalkActionPriority.High,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = false,
                        ActionByAgent = ActionByAgent.RandomInAllegiance,
                        DefaultText = "They are plant eaters, but ornery. Best stay clear of them!" }}
                    }
                    }

            });



            list.Add(new ActionSets()
            {
                KeyName = "detectBushDragonSetDelay", // 
                FireMode = ActionSetsToFire.AllValid,
                ChanceToFire = 1f, //the chance for the set to fire, then chooses from below


                SetsOfActions = new []{ 
                        new ActionSetType("443ertyueuytjhsftghjsgfh69")
                        {    
                                                      
                             Condition = new CustomCondition()
                             {                                                      
                                    PropertyCondition = new PropertyCondition()  { PropertyKey = "fledAtStart", BoolValue = false  } // triggered for the normal difficulty scenario                                             
                                            
                             },
                       Actions = new EventActionType[]{  new SetPropertyAction("7fc55edfffffffffffhrythjur6hwy62ff3b") { DelayInSeconds = 7, 
                                         PropertyKey = "bushDragonDetectedShortDelay", Value = new ValueNode() { Bool = true }
                                    },  
        
                       }
                        },

                         new ActionSetType("443ersdddddddhryjutykifjdgkdg9")
                        {    
                                                      
                             Condition = new CustomCondition()
                             {                                                      
                                    PropertyCondition = new PropertyCondition()  { PropertyKey = "fledAtStart", BoolValue = true  } // triggered for the hard difficulty scenario
                            }                                            
                             ,
                       Actions = new EventActionType[]{  new SetPropertyAction("7fc55edfffdghjye64w4454sw5ytse5yr62ff3b") { DelayInSeconds = 300, 
                                         PropertyKey = "bushDragonDetectedLongDelay", Value = new ValueNode() { Bool = true }  
                                    },  
        
                       }
                        },


                    }

            });

            list.Add(new ActionSets()
            {
                KeyName = "bushDragonDetectedDelayEvent",
                FireMode = ActionSetsToFire.AllValid,
                SetsOfActions = new []{ new ActionSetType("8sdgfhgfshsrgfhsttsrhfsghsfghfsgh4d")
                    {     MaxFirings = 1,
                          Condition = new ConditionFunction()
                            {
                                Operator = OperatorType.And,
                                Left = new PlayerAllegiancePersons()
                                { 
                                    MinMembers = 2, 
                                    AgentCondition = new AgentCondition()  // use default flags    
                                },
                                Right = new CustomCondition()
                                {
                                    PropertyCondition = new PropertyCondition()
                                    {
                                        PropertyKey = "fieldLabCannibalized", //to avoid talk about this if the player has already begun.
                                        BoolValue = false
                                    }
                                }                              
                            }, 
                    
                    Actions = new EventActionType[]{   

                                 new TalkAction("f7dsfghhhhhhhhhhhjsfbgjxvbnjxvfbn1002") { DelayInSeconds = 0,                        
                                   
                                    ActionByAgent = ActionByAgent.RandomInAllegiance,
                                    TalkPriority = TalkAction.TalkActionPriority.High,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                                    CanTalkWhileFighting = false,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = false,
                                    TurnTowardsListeners = true,
                                    DefaultText = "Do you have a minute..?" }   //not "guys..." in case they are only 2.                             
                                    ,

                                 new EventActionDialog("b3xvbnnnnnnnnsrynrstyhsrhtsfghc2c") { DelayInSeconds = 3,                                      
                                 
                                    DisplayText = new DynamicText(){ Text = "AUDIO LOG - Meeting (Subject: Weaponry) \n \n - I called this meeting because our finding a bush dragon population has given me an idea. \n \n-Go on. \n \n-Well, I'm sure we all agree that we are going to need more weapons to defend against quadites. The bush dragon, when threatened, disperses a chemical which is hazardous to most quadite species. I suggest we try to harvest this chemical from a dead bush dragon. \n \n-OK. That's the ammunition. What's the weapon then? \n \n-For projecting the poison, I believe we can use a fire extinguisher. But we need to increase its output. The field lab has components that can be... \n \n-Out of the question! You're not going to cannibalize the field lab! The lab is vital for making us able to eat the vegetables we find here! \n \n-Yes. The choice stands between using the field lab for preparing vegetables or salvaging the field lab to make a defensive weapon. Are we ready to vote and make a decision?" ,
                                      /*  SubstitutionValues = new[] // no way to get two different speakers
                                        {
                                            new ValueNode()
                                            {
                                                TargetObject = new TargetObject()
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
                                                        NextList = new InGameEvents.PropertyObjects.GetList()
                                                        {                                                           
                                                            HasPropertiesListKey = "randomPersons",
                                                            FilterCondition = new AgentCondition()   // use default flags                                                     
                                                        }
                                                    }
                                                },
                                                PropertyKey = "name"
                                            }   
                                        }*/
                                    },                                       
                                    DisplayImage = "BushDragon"}                      
                               ,
                             new TalkAction("5sfghfghfsghxfxcv456456hsfghfghe5b") { DelayInSeconds = 4,                        
                                     
                                    ActionByAgent = ActionByAgent.RandomInAllegiance,
                                    TalkPriority = TalkAction.TalkActionPriority.High,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                                    CanTalkWhileFighting = false,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = false,
                                    TurnTowardsListeners = true,
                                    DefaultText = "Hmmm. It's a tough choice." },
                    } },
                }
            });


#endregion

            #region Detect resource
            // tree:shadeleafdead
            list.Add(new ActionSets()
            {
                KeyName = "detectShadeleafResin", //not for fled at start, because of the start location south east which is right on top of shadeleaf trees. this dialogue clashes with intro dialogue.
                SetsOfActions = new []{ new ActionSetType("3c43ab7e-0189-47f5-a169-bdf81ac39a31")
                    {     
                       Condition = new ConditionFunction()
                        {
                            Operator = OperatorType.And,
                            Left = new PlayerAllegiancePersons(){ MinMembers = 2 }
                            ,
                            Right = new CustomCondition()
                            {
                                PropertyCondition = new PropertyCondition()
                                {
                                    PropertyKey = "fledAtStart",
                                    BoolValue = false
                                }
                            }                            
                    },  
                    MaxFirings = 1,
                    Actions = new EventActionType[]{ new TalkAction("8bb1bf42-53c0-4872-86c7-0c6af07ebecc") {   
                    DelayInSeconds = 0,
                         
                    TalkPriority = TalkAction.TalkActionPriority.Normal,
                    SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                    CanTalkWhileFighting = false,
                    CanTalkWhileSleeping = false,
                    CanTalkWhileThreatened = false,
                    ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                    DefaultText = "Shadeleaf trees." },  

                new TalkAction("e0f0f903-9ac2-4046-bcc0-e917b0474a9a") { DelayInSeconds = 3,  
                    
                    TalkPriority = TalkAction.TalkActionPriority.Normal,
                    SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                    TurnTowardsListeners = true,
                    CanTalkWhileFighting = false,
                    CanTalkWhileSleeping = false,
                    CanTalkWhileThreatened = false,
                    ActionByAgent = ActionByAgent.RandomInAllegiance,
                    DefaultText = "Their resin is very valuable! It protects shelters from scuttler bugs." }, //"Keep looking for healthy shadeleaf trees. The wood is good for making bows."
                    }
                }
                }

            });

            //

            list.Add(new ActionSets()
            {
                KeyName = "detectShadeleafBowStave",
                SetsOfActions = new []{ new ActionSetType("518b9556-a612-4ef3-ae4d-6424aff922bb")
                    {     
                            Condition = new PlayerAllegiancePersons(){ MinMembers = 2 }                                
                            ,  
                    MaxFirings = 1,
                    Actions = new EventActionType[]{ new TalkAction("2d994bb1-b3bf-4fda-879a-a9441cc54e81") {   
                    DelayInSeconds = 0,
                         
                    TalkPriority = TalkAction.TalkActionPriority.High,
                    SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                    CanTalkWhileFighting = false,
                    CanTalkWhileSleeping = false,
                    CanTalkWhileThreatened = false,
                    ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                    DefaultText = "I found wood for making bows here!" },  

                    
                new TalkAction("583e0669-052d-4fb4-822c-937f06474948") { DelayInSeconds = 2,  
                    
                    TalkPriority = TalkAction.TalkActionPriority.High,
                    SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                    TurnTowardsListeners = true,
                    CanTalkWhileFighting = false,
                    CanTalkWhileSleeping = false,
                    CanTalkWhileThreatened = false,
                    ActionByAgent = ActionByAgent.RandomInAllegiance,
                    DefaultText = "Great! We can always use ranged weapons." },

                    }
                }
                
                }         
                       
            });

            list.Add(new ActionSets()
            {
                KeyName = "detectShadeleafCanes",
                FireMode = ActionSetsToFire.RandomValid,
                SetsOfActions = new []
                { 

                    new ActionSetType("4132a02a-0308-4bdd-9fc5-e70247f3bd1a")
                    {     
                        Condition = new PlayerAllegiancePersons(){ MinMembers = 2 }                            
                        ,
                        MaxFirings = 1,
                        Actions = new EventActionType[]
                        { 
                            new TalkAction("bf428102-1c08-451b-b0da-4d77a7c3edce") 
                            {   
                                DelayInSeconds = 0,
                                 
                                    TalkPriority = TalkAction.TalkActionPriority.Normal,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                                    CanTalkWhileFighting = false,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = false,
                                    ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                    DefaultText = "There's a grove of healthy shadeleaf trees..." 
                                
                            },  //maybe when finding the trees: There's a grove of healthy shadeleaf trees. Might be a chance for finding wood for a bow.
                            new TalkAction("1a6361d1-1682-427c-a7f8-ac4108f120c9") 
                            { 
                                DelayInSeconds = 3,  
                                
                                    TalkPriority = TalkAction.TalkActionPriority.Normal,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                                    TurnTowardsListeners = true,
                                    CanTalkWhileFighting = false,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = false,
                                    ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                    DefaultText = "..might be a chance for finding wood for a bow." 
                                
                            }
                        }
                    }
                }
            });

            list.Add(new ActionSets()
            {
                KeyName = "detectSulfur",
                FireMode = ActionSetsToFire.AllValid,
                SetsOfActions = new []
                { 
                    new ActionSetType("cd0e8571-8e9d-4d1f-be31-06c194de2aaf")
                    {     
                            Condition = new PlayerAllegiancePersons(){ MinMembers = 2 }, 
                        MaxFirings = 1,
                        Actions = new EventActionType[]
                        { 
                            new TalkAction("0a332d0b-9434-431f-8d65-82794fee328d") {   
                                DelayInSeconds = 0,
                         
                                TalkPriority = TalkAction.TalkActionPriority.High,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                                CanTalkWhileFighting = false,
                                CanTalkWhileSleeping = false,
                                CanTalkWhileThreatened = false,
                                ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                DefaultText = "The hot springs here are lined with thick deposits of sulfur." 
                            },  
                            new TalkAction("c70bf4a8-1192-4cad-8723-a48c2132a51e") { DelayInSeconds = 4,  
                    
                                TalkPriority = TalkAction.TalkActionPriority.High,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                                CanTalkWhileFighting = false,
                                CanTalkWhileSleeping = false,
                                CanTalkWhileThreatened = false,
                                ActionByAgent = ActionByAgent.RandomInAllegiance,
                                DefaultText = "What are you thinking?" 
                            },                    
                          new TalkAction("a4701335-86c5-4bb0-91fb-86ea80f1f7cb") { DelayInSeconds = 6.5,  
                    
                                TalkPriority = TalkAction.TalkActionPriority.High,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                                CanTalkWhileFighting = false,
                                CanTalkWhileSleeping = false,
                                CanTalkWhileThreatened = false,
                                ActionByAgent = ActionByAgent.RandomInAllegiance,
                                DefaultText = "I'm thinking that there's always a use for sulfur." 
                          }
                        }
                    },
                    new ActionSetType("fe6d06d3-a1d3-45b4-9532-1f1bf2dd060e")
                    {     
                        Actions = new EventActionType[]
                        {
                            new SetPropertyAction("7447484f-a957-4e27-82db-972aab3ac771") 
                            { 
                                DelayInSeconds = 15f,
                                PropertyKey = "sulfurDetected", Value = new ValueNode() { Bool = true }
                            }
                        }
                    }
                }  
            });

            #endregion


            #region EVENTS WITH NO TALK

            #region Respawn nest
            /*
            list.Add(new ActionSets()
            {
                FireMode = ActionSetsToFire.RandomValid,  // chooses randomly between the groups below  for continous spawning.

                KeyName = "respawnRatNestSouth",
                SetsOfActions = new []{ new ActionSetType("8c5578de-0942-421f-907b-ac3f9742084b2")
                                {                                                       
                                    Actions = new EventActionType[]
                                    {   
           
                                        new EventActionType("d9c660a6-4asf96-48ed-a014-0ec29b8369762"){
                                            KeyName = "spawnRatNestSouth",
                                            DelayInSeconds = 1,
                                            SpawnEntity = new SpawnEntityAction()
                                            {
                                                EntityData = new EntityData()
                                                {
                                                    EntityKey = "terrain:ratNest",
                                                    Name = "Rat nest (coord. 25;52)",
                                                    Location = new Vector3(1225, 2548, 0),
                                                    Threat = new Threat() { ThreatGroupName = "twinklerAllegiance" }
                                                },
                                            }

                                        }
                                    }    
                                }
                }


            });
            list.Add(new ActionSets()
            {
                FireMode = ActionSetsToFire.RandomValid,  // chooses randomly between the groups below  for continous spawning.

                KeyName = "respawnRatNestCenter",
                SetsOfActions = new []{ new ActionSetType("8c5578de-0942-421f-907b-ac3f9742084b3")
                                {                                                       
                                    Actions = new EventActionType[]
                                    {   
           
                                        new EventActionType("d9c6afs6262626dhu60a6-4296-48ed-a014-0ec29b8369763"){
                                            KeyName = "spawnRatNestCenter",
                                            DelayInSeconds = 1,
                                            SpawnEntity = new SpawnEntityAction()
                                            {
                                                EntityData = new EntityData()
                                                {
                                                    EntityKey = "terrain:ratNest",
                                                    Name = "Rat nest (coord. 28;29)",
                                                    Location = new Vector3(1350, 1425, 0),
                                                    Threat = new Threat() { ThreatGroupName = "twinklerAllegiance" }
                                                },
                                            }

                                        }
                                    }    
                                }
                }


            });
            list.Add(new ActionSets()
            {
                FireMode = ActionSetsToFire.RandomValid,  // chooses randomly between the groups below  for continous spawning.

                KeyName = "respawnRatNestNorth",
                SetsOfActions = new []{ new ActionSetType("8c5578de-0942-421f-907b-ac3f9742084b2")
                                {                                                       
                                    Actions = new EventActionType[]
                                    {   
           
                                        new EventActionType("d9c660a6-4asf626296-48ed-a014-0ec29b8369762"){
                                            KeyName = "spawnRatNestNorth",
                                            DelayInSeconds = 1,
                                            SpawnEntity = new SpawnEntityAction()
                                            {
                                                EntityData = new EntityData()
                                                {
                                                    EntityKey = "terrain:ratNest",
                                                    Name = "Rat nest (coord. 56;17)",
                                                    Location = new Vector3(2688, 864, 0),
                                                    Threat = new Threat() { ThreatGroupName = "twinklerAllegiance" }
                                                },
                                            }

                                        }
                                    }    
                                }
                }


            });*/
            #endregion
///////

            #region Spawn Quadites





/////////

            list.Add(new ActionSets()
            {
                KeyName = "spawnFirstGuardsSouthSandstoneCave",
                SetsOfActions = new []{ new ActionSetType("ea3943e9-452f-4e73-913c-5a1ba91a3933")
                        {     
                               //  Nest condition for spawning:
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
                               },
                        
                            MaxFirings = 1,
                            Actions = new EventActionType[]{    


                        new SpawnEntityAction("1e55b632-b831-4b41-89c4-9d0162aa79b9") { DelayInSeconds = 7,
                        EntityData = new EntityData()
                            { EntityKey = "entity:twinkler", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "twinklerAllegiance" },
                                Location = new Vector3(720, 1536, 0), Bulk = 0.25f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult, CasteKey = "guard" } }, },


                        new SpawnEntityAction("acd98bc5-e32e-49b2-af1b-2435f60c020a") { DelayInSeconds = 7.1,
                        EntityData = new EntityData()
                            { EntityKey = "entity:twinkler", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "twinklerAllegiance" },
                              Location = new Vector3(720, 1536, 0), Bulk = 0.25f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult, CasteKey = "guard" } }, },


                        new SpawnEntityAction("1b7f10b1-ac1c-472d-b925-6bc32059cd05") { DelayInSeconds = 7.2,
                        EntityData = new EntityData()
                            { EntityKey = "entity:twinkler", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "twinklerAllegiance" },
                                Location = new Vector3(720, 1536, 0), Bulk = 0.25f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult, CasteKey = "guard" } }, },

/*
                        new EventActionType("56c3e4eb-2202-4986-84cc-1287c6fa9897") { DelayInSeconds = 7.5,
                        SavedMapEntity = new SavedMapEntity()
                            { EntityKey = "entity:twinkler", Location = new Vector3(720, 1536, 0), Bulk = 0.25f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult, CasteKey = "guard", AllegianceName = "twinklerAllegiance", } }, }},

*/
                            }}}
            }

 );



            list.Add(new ActionSets()
            {
                KeyName = "spawnFirstGuardsEastRockCave",
                SetsOfActions = new []{ new ActionSetType("fa8bd312-669a-4e80-be96-3c089fb8ed86")
                        {     
                               //  Nest condition for spawning:
                        Condition = new CustomCondition()
                             {  TargetObject = new TargetObject()
                                 {
                                     GetList = new GetList() { HasPropertiesListKey = "entities", 
                                          FilterCondition = new PropertyCondition(){ PropertyKey = "name", ConstantStringEqual = "Quadite nest (coord. 38;38)"}}
                                        
                                 }, 
                                ListCondition = new ListCondition(){ CountEqual = 1 }                             
                               },
                        
                            MaxFirings = 1,
                            Actions = new EventActionType[]{
                        new SpawnEntityAction("8fcfd502-0d4f-4f93-80a9-b82345244955") { DelayInSeconds = 7,
                        EntityData = new EntityData()
                            { EntityKey = "entity:twinkler", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "twinklerAllegiance" },
                                Location = new Vector3(1860, 1824, 0), Bulk = 0.25f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult, CasteKey = "guard" } } },


                        new SpawnEntityAction("cdb13aee-08f9-4d8f-ac1f-73b57339742a") { DelayInSeconds = 7.2,
                        EntityData = new EntityData()
                            { EntityKey = "entity:twinkler", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "twinklerAllegiance" },
                              Location = new Vector3(1860, 1824, 0), Bulk = 0.25f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult, CasteKey = "guard" } }, },

                        new SpawnEntityAction("383b956f-be07-410b-9039-037cbe0ef435") { DelayInSeconds = 7.7,
                        EntityData = new EntityData()
                            { EntityKey = "entity:twinkler", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "twinklerAllegiance" },
                              Location = new Vector3(1860, 1824, 0), Bulk = 0.25f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult, CasteKey = "guard" } },},
/*

                        new EventActionType("b216d1de-c435-493c-8c8c-37c95928c7c5") { DelayInSeconds = 8,
                        SavedMapEntity = new SavedMapEntity()
                            { EntityKey = "entity:twinkler", Location = new Vector3(1860, 1824, 0), Bulk = 0.25f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeInYears = 3, CasteKey = "guard", AllegianceName = "twinklerAllegiance", } }, }},
*/

                            }}}
            }

					);

            ///////////Easy. only spawns one twinkler (a hunter) at entrance:


            list.Add(new ActionSets()
            {
                KeyName = "spawnFirstHunterSouthSandstoneCave",
                SetsOfActions = new []{ new ActionSetType("e3cb1b8c-8ea3-4d6d-8fe3-0fdfb20a7227")
                        {     
                               //  Nest condition for spawning:
                        Condition = new CustomCondition()
                        { 
                            TargetObject = new TargetObject()
                            {
                                GetList = new GetList() { HasPropertiesListKey = "entities", 
                                    FilterCondition = new PropertyCondition() { PropertyKey = "name", ConstantStringEqual = "Quadite nest (coord. 15;34)"}
                                }
                                    //FilterProperty = "name", FilterValue = } //southSandstoneCaveNest
                            }, 
                            ListCondition = new ListCondition(){ CountEqual = 1 }} 
                        ,                        
                        MaxFirings = 1,
                        Actions = new EventActionType[]
                        {   

                        new SpawnEntityAction("a0bcaa1e-b81f-40ce-a0f4-bb369f2aaadd") { DelayInSeconds = 7,
                        EntityData = new EntityData()
                            { EntityKey = "entity:twinkler", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "twinklerAllegiance" },
                              Location = new Vector3(720, 1536, 0), Bulk = 0.25f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult, CasteKey = "hunter" } }, }

                            }}}
            }
 );



            list.Add(new ActionSets()
            {
                KeyName = "spawnFirstHunterEastRockCave",
                SetsOfActions = new []{ new ActionSetType("211760f2-9c8d-4cdd-af73-9d46a971fa78")
                        {     
                               //  Nest condition for spawning:
                        Condition = new CustomCondition()
                             {  TargetObject = new TargetObject()
                                 {
                                     GetList = new GetList() { HasPropertiesListKey = "entities", 
                                          FilterCondition = new PropertyCondition(){ PropertyKey = "name", ConstantStringEqual = "Quadite nest (coord. 38;38)"}}
                                        
                                 }, 
                                ListCondition = new ListCondition(){ CountEqual = 1 }} 
                             ,                        
                            MaxFirings = 1,
                            Actions = new EventActionType[]{      
                        new SpawnEntityAction("c59c5b0e-2194-40af-b0d3-b13751a5a99e") { DelayInSeconds = 7,
                        EntityData = new EntityData()
                            { EntityKey = "entity:twinkler", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "twinklerAllegiance" },
                              Location = new Vector3(1860, 1824, 0), Bulk = 0.25f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult, CasteKey = "hunter" } }, }

                            }}}
            }

);




            /////////////////////

            list.Add(new ActionSets()
            {
                FireMode = ActionSetsToFire.RandomValid,  // chooses randomly between the two below ( a hunter and two guards.) for continous spawning.

                KeyName = "continualTwinklerSpawnSouthSandstoneCave",
                SetsOfActions = new []{ new ActionSetType("a9322e11-806c-4238-9eaf-a06d0437e31c")
                                {     
                                                  
                                    Actions = new EventActionType[]{                              
                                          
                                new SpawnEntityAction("6e7193ca-53ab-4f79-941b-b4ca0045f1f6") { 
                                EntityData = new EntityData()
                                    { EntityKey = "entity:twinkler", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "twinklerAllegiance" },
                                      Location = new Vector3(720, 1536, 0), Bulk = 0.25f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                    { AgeGroup = AIAgeGroup.Adult, CasteKey = "hunter" } }, }
                                    }
                                },
                                 new ActionSetType("b77a6558-ff20-4376-9eaf-750a86b4cc98")
                                {     
                                                
                                    Actions = new EventActionType[]{                              
                                          
                                new SpawnEntityAction("6968f5d6-99fd-41b6-8027-6c176a8df114") { 
                                EntityData = new EntityData()
                                    { EntityKey = "entity:twinkler", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "twinklerAllegiance" },
                                      Location = new Vector3(720, 1536, 0), Bulk = 0.25f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                    { AgeGroup = AIAgeGroup.Adult, CasteKey = "guard" } }, },
                                new SpawnEntityAction("7c9a5141-ac19-4721-b690-3d439cbcf3d4") { 
                                EntityData = new EntityData()
                                    { EntityKey = "entity:twinkler", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "twinklerAllegiance" },
                                      Location = new Vector3(720, 1536, 0), Bulk = 0.25f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                    { AgeGroup = AIAgeGroup.Adult, CasteKey = "guard" } }, }

                                    }
                                }
                                   }
            });



            list.Add(new ActionSets()
            {
                FireMode = ActionSetsToFire.RandomValid,  // chooses randomly between the two below ( a hunter and two guards.) for continous spawning.                                   
                KeyName = "continualTwinklerSpawnEastRockCave",
                SetsOfActions = new []{ new ActionSetType("4d2d58b8-050d-4421-9067-f0da6161ecb2")
                                {     
                                                      
                                    Actions = new EventActionType[]{                              
                                          
                                new SpawnEntityAction("88fdcdac-1f52-431a-a828-a6d0b1e652a6") { 
                                EntityData = new EntityData()
                                    { EntityKey = "entity:twinkler", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "twinklerAllegiance" }, 
                                      Location = new Vector3(1860, 1824, 0), Bulk = 0.25f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                    { AgeGroup = AIAgeGroup.Adult, CasteKey = "hunter" } }, }
                                    }
                                },
                                 new ActionSetType("21d6cbae-64f5-4bc8-b1d0-7b01b80d7f3c")
                                {     
                                                       
                                    Actions = new EventActionType[]{                              
                                          
                                new SpawnEntityAction("99fe4914-e580-413f-a5ff-83d963852b02") { 
                                EntityData = new EntityData()
                                    { EntityKey = "entity:twinkler", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "twinklerAllegiance" }, 
                                      Location = new Vector3(1860, 1824, 0), Bulk = 0.25f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                    { AgeGroup = AIAgeGroup.Adult, CasteKey = "guard" } }, },
                                new SpawnEntityAction("ecb11dc1-c81a-425c-a814-326831799d0b") { 
                                EntityData = new EntityData()
                                    { EntityKey = "entity:twinkler",  MemberOf = new AllegianceAndExpedition() { AllegianceKey = "twinklerAllegiance" }, 
                                      Location = new Vector3(1860, 1824, 0), Bulk = 0.25f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                    { AgeGroup = AIAgeGroup.Adult, CasteKey = "guard" } }, }

                                    }
                                }
                                   }
            });


            list.Add(new ActionSets()
            {
                FireMode = ActionSetsToFire.RandomValid,  // chooses randomly between the two below ( a hunter and  guard.) for continous spawning.                                   
                KeyName = "continualTwinklerSpawnSouthRockCave",
                SetsOfActions = new []{ new ActionSetType("4d5673yvsegsgffghdgfjjjtttttttttttttrwtyurtwurb2")
                                {     
                                                      
                                    Actions = new EventActionType[]{                              
                                          
                                new SpawnEntityAction("8etttsaasf325235f2q2dghjhdkdgkkttttttttttyu5678u5uw46wa6") { 
                                EntityData = new EntityData()
                                    { EntityKey = "entity:twinkler", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "twinklerAllegiance" }, 
                                      Location = new Vector3(1268, 2170, 0), Bulk = 0.25f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                    { AgeGroup = AIAgeGroup.Adult, CasteKey = "hunter" } }, }
                                    }
                                },
                                 new ActionSetType("2e56777yutfsa2asf3dfhjddghkdgjkdgkjduuuuuuuuuafawfr335253rdudtyuf3c")
                                {     
                                                       
                                    Actions = new EventActionType[]{                              
                                          
                                new SpawnEntityAction("9ettttttttfa35w456y7rtysrtysty756e7uy65euery7ure02") { 
                                EntityData = new EntityData()
                                    { EntityKey = "entity:twinkler", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "twinklerAllegiance" }, 
                                      Location = new Vector3(1268, 2170, 0), Bulk = 0.25f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                    { AgeGroup = AIAgeGroup.Adult, CasteKey = "guard" } }, },

                                    }
                                }
                                   }
            });


            list.Add(new ActionSets()
            {
                FireMode = ActionSetsToFire.RandomValid,  // chooses randomly between the two below ( a hunter and  guard.) for continous spawning.                                   
                KeyName = "continualTwinklerSpawnNorthRockCave",
                SetsOfActions = new []{ new ActionSetType("4d5faawryjurystjxghfjfsghfsgfjkgljkgjlkjglgkljlb2")
                                {     
                                                      
                                    Actions = new EventActionType[]{                              
                                          
                                new SpawnEntityAction("8ettttafdsgjklhfjhjssffsfsfsjlgkjlgjklgkjlgkjlgkjwa6") { 
                                EntityData = new EntityData()
                                    { EntityKey = "entity:twinkler", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "twinklerAllegiance" }, 
                                      Location = new Vector3(1720, 1000, 0), Bulk = 0.25f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                    { AgeGroup = AIAgeGroup.Adult, CasteKey = "hunter" } }, }
                                    }
                                },
                                 new ActionSetType("2e5gsaf3tqefdhjghfjdhrsthtsdhauuuuurdudtyuf3c")
                                {     
                                                       
                                    Actions = new EventActionType[]{                              
                                          
                                new SpawnEntityAction("9etfsfgjfhsjkdtuskrtysu5665guystrursfsaysrysrry7ure02") { 
                                EntityData = new EntityData()
                                    { EntityKey = "entity:twinkler", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "twinklerAllegiance" }, 
                                      Location = new Vector3(1720, 1000, 0), Bulk = 0.25f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                    { AgeGroup = AIAgeGroup.Adult, CasteKey = "guard" } }, },

                                    }
                                }
                                   }
            });



            // Easy mode. does not have guards, so no build up of armies. instead hunters will attack solitary or in pairs:
            list.Add(new ActionSets()
            {
                FireMode = ActionSetsToFire.RandomValid,  // chooses randomly between the two below  for continous spawning.    MP: not relevant here, don't know how to remove this, it was copy+pasted from above                               
                KeyName = "continualHunterSpawnEastRockCave",
                SetsOfActions = new []{ new ActionSetType("90e00a1d-ac5e-4019-9293-abbc42eef741")
                                {     
                                                      
                                    Actions = new EventActionType[]{                              
                                          
                                new SpawnEntityAction("0989ddd8-b4b4-471a-bde9-61b8edb2f676") { 
                                EntityData = new EntityData()
                                    { EntityKey = "entity:twinkler", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "twinklerAllegiance" },
                                      Location = new Vector3(1860, 1824, 0), Bulk = 0.25f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                    { AgeGroup = AIAgeGroup.Adult, CasteKey = "hunter" } }, }
                                    }
                                },
                                 new ActionSetType("0aee562a-0baf-430a-959e-28a4212793f9")
                                {     
                                                       
                                    Actions = new EventActionType[]{                              
                                          
               
                                new SpawnEntityAction("e2e4343c-5ecd-4ffb-8706-c2b7bedd2efc") { 
                                EntityData = new EntityData()
                                    { EntityKey = "entity:twinkler", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "twinklerAllegiance" },
                                      Location = new Vector3(1860, 1824, 0), Bulk = 0.25f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                    { AgeGroup = AIAgeGroup.Adult, CasteKey = "hunter" } }, }

                                    }
                                }
                                   }
            });


            list.Add(new ActionSets()
            {
                FireMode = ActionSetsToFire.RandomValid,  // chooses randomly between the two below  for continous spawning.    MP: not relevant here, don't know how to remove this, it was copy+pasted from above                               

                KeyName = "continualHunterSpawnSouthSandstoneCave",
                SetsOfActions = new []{ new ActionSetType("fcbbb312-e0da-44d9-b96c-043f252181f4")
                                {     
                                                  
                                    Actions = new EventActionType[]{                              
                                          
                                new SpawnEntityAction("aee8eb7c-d861-4aef-9524-dfe42a46e6ea") { 
                                EntityData = new EntityData()
                                    { EntityKey = "entity:twinkler", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "twinklerAllegiance" },
                                      Location = new Vector3(720, 1536, 0), Bulk = 0.25f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                    { AgeGroup = AIAgeGroup.Adult, CasteKey = "hunter" } }, }
                                    }
                                },
                                 new ActionSetType("2296ad80-7034-4fe2-bc5e-99d306cc931f")
                                {     
                                                
                                    Actions = new EventActionType[]{                          

                                new SpawnEntityAction("64c1085d-5673-434b-83d9-9b461768986f") { 
                                EntityData = new EntityData()
                                    { EntityKey = "entity:twinkler", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "twinklerAllegiance" },
                                      Location = new Vector3(720, 1536, 0), Bulk = 0.25f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                    { AgeGroup = AIAgeGroup.Adult, CasteKey = "hunter" } }, }

                                    }
                                }
                                   }
            });



            #endregion

            #region Spawn Thunder chickens

            list.Add(new ActionSets()
            {
                FireMode = ActionSetsToFire.RandomValid,  // chooses randomly between the groups below  for continous spawning.

                KeyName = "continualSpawnThunderChickensSouth",
                SetsOfActions = new []{ new ActionSetType("333sad3253567ttt-555-45435asdv")
                {                                                       
                                                     
                    Actions = new EventActionType[]
                    {                            
                                          
                        new SpawnEntityAction("674721sda3252133457d9-9335-08507cae3a484234") { DelayInSeconds = 0,
                        EntityData = new EntityData()
                            { EntityKey = "entity:whiteThunderChicken", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "thunderChickenAllegianceSouth" },
                              Location = new Vector3(1546,2256, 0), Bulk = 0.35f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult } }, } //age must be set to adult or they wont eat             
 
                    }
                }
            }
            });

            list.Add(new ActionSets()
            {
                FireMode = ActionSetsToFire.RandomValid,  // chooses randomly between the groups below  for continous spawning.

                KeyName = "continualSpawnThunderChickensNorth",
                SetsOfActions = new []{ new ActionSetType("PRRTT-_TTRTTADB45451")
                {                                                       
                                                     
                    Actions = new EventActionType[]
                    {                            
                                          
                        new SpawnEntityAction("a3fff6cF9-9hnwnwfcc71") { DelayInSeconds = 0,
                        EntityData = new EntityData()
                            { EntityKey = "entity:pygmyThunderChicken", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "thunderChickenAllegianceNorth" },
                              Location = new Vector3(826,1114, 0), Bulk = 0.25f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult } }, } //age must be set to adult or they wont eat
    
 
                    }
                }
            }
            });


            # endregion

            #region Spawn Thin thunder chickens
            list.Add(new ActionSets()
            {
                FireMode = ActionSetsToFire.RandomValid,  // chooses randomly between the groups below  for continous spawning.

                KeyName = "continualSpawnThinThunderChickensSouth",
                SetsOfActions = new []{ new ActionSetType("3333567ttt-5235sdgsshsr55-45435asdv")
                {                                                       
                                                     
                    Actions = new EventActionType[]
                    {                            

                        new SpawnEntityAction("674721133457d9-9335-08asf62662507cae3a484234") { DelayInSeconds = 0,
                        EntityData = new EntityData()
                            { EntityKey = "entity:bajingan", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "thinThunderChickenAllegianceSouth" },
                              Location = new Vector3(960,2640, 0), Bulk = 0.35f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult } }, }              
 
                    }
                }

            }

            });
            
            list.Add(new ActionSets()
            {
                FireMode = ActionSetsToFire.RandomValid,  // chooses randomly between the groups below  for continous spawning.

                KeyName = "continualSpawnThinThunderChickensNorth",
                SetsOfActions = new []{ new ActionSetType("3333567ttt-555-4543sad2352365asdv")
                {                                                       
                                                     
                    Actions = new EventActionType[]
                    {                            

                        new SpawnEntityAction("674721133457d9-9asf2626335-08507cae3a484234") { DelayInSeconds = 0,
                        EntityData = new EntityData()
                            { EntityKey = "entity:bajingan", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "thinThunderChickenAllegianceNorth" },
                              Location = new Vector3(2688,816, 0), Bulk = 0.35f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                            { AgeGroup = AIAgeGroup.Adult } }, }              
 
                    }
                }

            }


            });
            #endregion

            #region Spawn  binal rats






            list.Add(new ActionSets()
            {
                FireMode = ActionSetsToFire.RandomValid,  // chooses randomly between the groups below  for continous spawning.

                KeyName = "continualSpawnBinalRatsSouthWest",
                SetsOfActions = new []{ new ActionSetType("8c5578de-0942-421f-907b-ac3f9742084b")
                                {                                                       
                                    Actions = new EventActionType[]
                                    {   
           
                                        new SpawnEntityAction("d9c660a6-4296-48ab fnuied-a014-0ec29b836976") { DelayInSeconds = 9,
                                        EntityData = new EntityData()
                                            { EntityKey = "entity:binalRat", Name = "BinalRat11", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegianceSouthWest" },
                                              Location = new Vector3(1008, 2304, 0), Bulk = 0.21f, BioEntity = new Maps.MapEditor.BiologicalEntity() //sw
                                            { AgeGroup = AIAgeGroup.Adult} }, },

                                      new SpawnEntityAction("fffa235325c-395c-42df-92da-fc9fb3429f22") { DelayInSeconds = 0,
                                        EntityData = new EntityData()
                                            { EntityKey = "entity:binalRat", Name = "BinalRat6", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegianceSouthWest" },
                                              Location = new Vector3(480, 2169, 0), Bulk = 0.21f, BioEntity = new Maps.MapEditor.BiologicalEntity()//sw
                                            { AgeGroup = AIAgeGroup.Adult} }, },

          

                                    }    
                                },
                                new ActionSetType("db871233-37a2-4ad7-b458-c8489b40acde")
                                {                                                     
                                    Actions = new EventActionType[]
                                    {                            
                                          
                                        new SpawnEntityAction("b8ff1d30-fe4c-49e8-86a4-373106356265") { DelayInSeconds = 1,    
                                        EntityData = new EntityData()
                                            { EntityKey = "entity:binalRat", Name = "BinalRat15", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegianceSouthWest" },
                                              Location = new Vector3(432, 2448, 0), Bulk = 0.21f, BioEntity = new Maps.MapEditor.BiologicalEntity()//sw
                                            { AgeGroup = AIAgeGroup.Adult } }, },

                                       new SpawnEntityAction("53c86026-c380-4c2f-9f52-bf6b9fc091fe") { DelayInSeconds = 0,
                                            EntityData = new EntityData()
                                                { EntityKey = "entity:binalRat", Name = "BinalRat1", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegianceSouthWest" },
                                                  Location = new Vector3(889, 1794, 0), Bulk = 0.21f, BioEntity = new Maps.MapEditor.BiologicalEntity()//sw
                                                { AgeGroup = AIAgeGroup.Adult } }, },
 
                                    }
                                },

                                new ActionSetType("41ef8ab5-ade1wegahh-rand-b-i-n-a-l-8d83ffdef")
                                {                                                     
                                    Actions = new EventActionType[]
                                    {                            
                                          
                                        new SpawnEntityAction("441c6bd6-4002-49df-bcf5-5cbbabeb68e3") { DelayInSeconds = 2,
                                        EntityData = new EntityData()
                                            { EntityKey = "entity:binalRat", Name = "BinalRat2", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegianceSouthWest" },
                                              Location = new Vector3(960, 2640, 0), Bulk = 0.21f, BioEntity = new Maps.MapEditor.BiologicalEntity()//sw
                                            { AgeGroup = AIAgeGroup.Adult} }, },

                                       new SpawnEntityAction("b4d2a25a-2ad0-48fc-b332-8be24598eaec") { DelayInSeconds = 8,
                                        EntityData = new EntityData()
                                            { EntityKey = "entity:binalRat", Name = "BinalRat10", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegianceSouthWest" },
                                              Location = new Vector3(608, 2081, 0), Bulk = 0.21f, BioEntity = new Maps.MapEditor.BiologicalEntity()//sw   
                                            { AgeGroup = AIAgeGroup.Adult } }, },

 
                                    }
                                },

                                new ActionSetType("b937ff5c-0314-498f-a4bd-23d5814df384")
                                {                                                     
                                    Actions = new EventActionType[]
                                    {                            
                                          
                                        new SpawnEntityAction("3c8421e7-16bf-42ad-a41f-3c0db139781a") { DelayInSeconds = 0,
                                        EntityData = new EntityData()
                                            { EntityKey = "entity:binalRat", Name = "BinalRat16", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegianceSouthWest" },
                                              Location = new Vector3(788, 2610, 0), Bulk = 0.21f, BioEntity = new Maps.MapEditor.BiologicalEntity()//sw
                                            { AgeGroup = AIAgeGroup.Adult} }, },
 
                                    }
                                }

                                   }


            });




            list.Add(new ActionSets()
            {
                FireMode = ActionSetsToFire.RandomValid,  // chooses randomly between the groups below  for continous spawning.

                KeyName = "continualSpawnBinalRatsCenterEast",
                SetsOfActions = new []{ new ActionSetType("5e7d286d-1b22-4fa1-981b-e72bab601ac8")
                                {                                                       
                                    Actions = new EventActionType[]
                                    {   
           
                                       new SpawnEntityAction("4fafaw1d8d-4b34-4d0e-a15e-499a609231eb") { DelayInSeconds = 8,
                                        EntityData = new EntityData()
                                            { EntityKey = "entity:binalRat", Name = "BinalRat5", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegianceCenterEast" },
                                              Location = new Vector3(2256, 2256, 0), Bulk = 0.21f, BioEntity = new Maps.MapEditor.BiologicalEntity() //ce
                                            { AgeGroup = AIAgeGroup.Adult } }, }, 

                                       new SpawnEntityAction("b4fsa253256b42-c962-4de8-adef-ff442a207409") { DelayInSeconds = 2,
                                        EntityData = new EntityData()
                                            { EntityKey = "entity:binalRat", Name = "BinalRat7", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegianceCenterEast" },
                                              Location = new Vector3(1971, 1943, 0), Bulk = 0.21f, BioEntity = new Maps.MapEditor.BiologicalEntity() //ce
                                            { AgeGroup = AIAgeGroup.Adult} }, },
         

                                    }    
                                },
                                new ActionSetType("fee81fe8-080f-415d-9dd1-bc7950555c67")
                                {                                                     
                                    Actions = new EventActionType[]
                                    {                            


                                        new SpawnEntityAction("66a5cce3-eda7-4301-822d-952d166168bc") { DelayInSeconds = 14,
                                        EntityData = new EntityData()
                                            { EntityKey = "entity:binalRat", Name = "BinalRat8", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegianceCenterEast" },
                                              Location = new Vector3(1248, 1728, 0), Bulk = 0.21f, BioEntity = new Maps.MapEditor.BiologicalEntity() //ce
                                            { AgeGroup = AIAgeGroup.Adult } }, }

 
                                    }
                                },

                                new ActionSetType("9019782c-a9f5-432a-a9d2-013a67fc5aba")
                                {                                                     
                                    Actions = new EventActionType[]
                                    {                            
                                          
                                        new SpawnEntityAction("bb5809e3-1f86-4239-baa7-a809c695ac6a") { DelayInSeconds = 6,
                                        EntityData = new EntityData()
                                            { EntityKey = "entity:binalRat", Name = "BinalRat9", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegianceCenterEast" },
                                              Location = new Vector3(1776, 2020, 0), Bulk = 0.21f, BioEntity = new Maps.MapEditor.BiologicalEntity() //ce
                                            { AgeGroup = AIAgeGroup.Adult } }, },
                                    }
                                }

                                   }

            });





            list.Add(new ActionSets()
            {
                FireMode = ActionSetsToFire.RandomValid,  // chooses randomly between the groups below  for continous spawning.

                KeyName = "continualSpawnBinalRatsNorth",
                SetsOfActions = new []{ new ActionSetType("0ceba4e5-ee58-458a-a70b-6b6f056688e3")
                                {                                                       
                                    Actions = new EventActionType[]
                                    {   
           
                                        new SpawnEntityAction("711af2352350e5-072e-4d8c-995d-c28dc842847c") { DelayInSeconds = 9,
                                        EntityData = new EntityData()
                                            { EntityKey = "entity:binalRat", Name = "BinalRat3", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegianceNorth" },
                                              Location = new Vector3(1641, 953, 0), Bulk = 0.21f, BioEntity = new Maps.MapEditor.BiologicalEntity()//n
                                            { AgeGroup = AIAgeGroup.Adult } }, },

                                        new SpawnEntityAction("befa5q3535afedad2d9-e5e4-4664-bd2d-e6aa13c1f489") { DelayInSeconds = 6,
                                        EntityData = new EntityData()
                                            { EntityKey = "entity:binalRat", Name = "BinalRat4", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegianceNorth" },
                                              Location = new Vector3(2424, 672, 0), Bulk = 0.21f, BioEntity = new Maps.MapEditor.BiologicalEntity() //n
                                            { AgeGroup = AIAgeGroup.Adult } }, }          

                                    }    
                                },
                                new ActionSetType("c375ff22-96fd-4cde-9ff0-bde78bcbea75")
                                {                                                     
                                    Actions = new EventActionType[]
                                    {                            
                                          
                                        new SpawnEntityAction("9bce14b3-27de-4eec-b5b6-ec618d12826f") { DelayInSeconds = 2,
                                        EntityData = new EntityData()
                                            { EntityKey = "entity:binalRat", Name = "BinalRat14", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegianceNorth" },
                                              Location = new Vector3(2112, 1104, 0), Bulk = 0.21f, BioEntity = new Maps.MapEditor.BiologicalEntity() //n
                                            { AgeGroup = AIAgeGroup.Adult } }, },

                                        new SpawnEntityAction("59409421-5384-4634-b99f-5ca7c5b45466") { DelayInSeconds = 8,
                                        EntityData = new EntityData()
                                            { EntityKey = "entity:binalRat", Name = "BinalRat12", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegianceNorth" },
                                              Location = new Vector3(1008, 1104, 0), Bulk = 0.21f, BioEntity = new Maps.MapEditor.BiologicalEntity() //n
                                            { AgeGroup = AIAgeGroup.Adult} }, },
 
                                    }
                                },

                                new ActionSetType("70843cc5-4a3e-409f-8abe-f3baa705e20f")
                                {                                                     
                                    Actions = new EventActionType[]
                                    {                            
                                          
                                         new SpawnEntityAction("f1ab0583-1ca1-4ded-bb1e-5240db9e3199") { DelayInSeconds = 22,
                                         EntityData = new EntityData()
                                            { EntityKey = "entity:binalRat", Name = "BinalRat17", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegianceNorth" },
                                              Location = new Vector3(2640, 864, 0), Bulk = 0.21f, BioEntity = new Maps.MapEditor.BiologicalEntity()//n
                                            { AgeGroup = AIAgeGroup.Adult } }, }, 
 
                                    }
                                }

                                   }


            });





            #endregion



            #region Place Quadite Nests

/*
            list.Add(new ActionSets()
            {
                KeyName = "placeNests",
                SetsOfActions = new []{ new ActionSetType("c2bf8033-c8ff-480d-989f-171f4dd2633a")
                        {     
                                                 
                            Actions = new EventActionType[]{

                            new EventActionType("9a211055-264b-44f5-9067-a9028b4de67c") { DelayInSeconds = 1,
                            EntityData = new EntityData()
                            { EntityKey = "terrain:quaditeNest", Name = "Quadite nest (coord. 15;34)",  Location = new Vector3(732, 1510, 0),  Threat = new Threat() { ThreatGroupName = "twinklerAllegiance" }   //"southSandstoneCaveNest"
                               }, }},

                            new EventActionType("499f69d8-c464-4be7-bd80-edc9a82e6c9f") { DelayInSeconds = 1,
                            EntityData = new EntityData()
                            { EntityKey = "terrain:quaditeNest", Name = "Quadite nest (coord. 38;38)",  Location = new Vector3(1800, 1800, 0),  Threat = new Threat() { ThreatGroupName = "twinklerAllegiance" }    //"eastRockCaveNest"
                               }, }},

                            }
                        }
                    }
            });

*/


            //////Unused caves:
            /*
            //////abandoned nest
                        new SpawnEntityAction("8b27bff5-8796-4a1f-af1e-8497fef56360") { DelayInSeconds = 1,
                        SavedMapEntity = new SavedMapEntity()
                        { EntityKey = "terrain:quaditeNest", Name = "northEastCloseRockCreviceNest",  Location = new Vector3(1248, 2112, 0),    
                           }, }},

////////enough with the cave nest
                        new SpawnEntityAction("b8680951-0cbb-476f-bf0c-a7ee5e8ce7e4") { DelayInSeconds = 1,
                        SavedMapEntity = new SavedMapEntity()
                        { EntityKey = "terrain:quaditeNest", Name = "northSandstoneCreviceNest",  Location = new Vector3(624, 1488, 0),    
                           }, }},
*/

           

            #endregion

            #endregion




            return list;


        }

    }
}
