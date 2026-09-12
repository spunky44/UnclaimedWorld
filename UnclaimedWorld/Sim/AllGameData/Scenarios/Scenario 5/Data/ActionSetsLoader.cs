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

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_5.Data
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
                                ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                DefaultText = "I'm so hungry!" }, 

                                new TalkAction("25a58cdc-156c-4826-8f42-c6bdfa8a8e4b") { DelayInSeconds = 2.5, 
                     
                        TalkPriority = TalkAction.TalkActionPriority.Low,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = false,
                        TurnTowardsListeners = false,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.First, 
                        ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                        DefaultText = "This is good." },
                        }
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
                SetsOfActions = new []{ new ActionSetType("1dfafd23543uikppppdff83a3-a65c34756a91")
                        {    
                       //     ChanceToFire = 0.5f,
                            MaxFirings = 1,  
                        Condition = new PlayerAllegiancePersons(){ MinMembers = 2 }                            
                            , 
                       Actions = new EventActionType[]{new TalkAction("de0c6aa5-80244355-a098-e3c68a3150a3") {  
                       
                        TalkPriority = TalkAction.TalkActionPriority.Normal,
                        CanTalkWhileFighting = false,
                        CanTalkWhileSleeping = false,
                        CanTalkWhileThreatened = false,
                        TurnTowardsListeners = true,
                        SpeakerDenomination = TalkAction.SpeakerInConversation.First, 
                        ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                        DefaultText = "Thanks for letting me join! Hope I can be of use!" }, //

                    new TalkAction("f253-ewtsgfhiop-awqqfdvc-xcxewwea3b0878") { DelayInSeconds = 3, 
                     
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
                        DefaultText = "Welcome to Headway!" },
                        
                       }
                        },
                  new ActionSetType("0af7fsa325ty-dfqrtpp-xxcr591-9c47-de1f2fdcb2c2")
                         {      
                             Condition = new PlayerAllegiancePersons(){ MinMembers = 2 }                               
                               ,
                          //    ChanceToFire = 0.1f,      
                               Actions = new EventActionType[]{new TalkAction("37292basfa32egfhjukioppgve48-9445-39d4d79c959f") {  
                             
                                TalkPriority = TalkAction.TalkActionPriority.Normal,
                                CanTalkWhileFighting = false,
                                CanTalkWhileSleeping = false,
                                CanTalkWhileThreatened = false,
                                SpeakerDenomination = TalkAction.SpeakerInConversation.First,  
                                TurnTowardsListeners = true,
                                ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                DefaultText = "Hello everyone. I'm glad to be part of your colony!" }, 
                        
                            new TalkAction("9c09asf23asfd-5trthyjupp-48-fb385d2a5a3a") {  
                                    
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

                     

            

            #region Detect creatures
                    

            list.Add(new ActionSets() //talkAction 
            {
                FireMode = ActionSetsToFire.RandomValid,
                KeyName = "detectWhipjawTalk",//new
                SetsOfActions = new []
                {
                    #region actionSet#1
                    new ActionSetType("sxdza5232bf4-6764-4d5csdf-be7b-0f0d1e4b2a55")
                    {
                        Condition = new PlayerAllegiancePersons(){ MinMembers = 2 }
                        ,
                        MaxFirings = 1,
                        Actions = new EventActionType[]
                        {
                            new TalkAction("c1618c9sdfg8-c8d3-4e19-9e82-1115dc6d4507")
                            {
                                DelayInSeconds = 0,
                               
                                    TalkPriority = TalkAction.TalkActionPriority.Normal,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                                    CanTalkWhileFighting = true,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = true,
                                    ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                    DefaultText = "The whipjaw is getting awfully close!" 
                                
                            },
                            new TalkAction("f3f420dvxcv-cvcjkipopoy-gtrfrvfvfgff9-da34tdc47271ee0")
                            {
                                DelayInSeconds = 2.5,
                               
                                    TalkPriority = TalkAction.TalkActionPriority.Normal,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                                    CanTalkWhileFighting = true,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = true,
                                    ActionByAgent = ActionByAgent.RandomInAllegiance,
                                    DefaultText = "We might want to take it down!" 
                                
                            }
                        }
                    },
                    #endregion
                    #region actionSet#2
                    new ActionSetType("a5232bf4-6764-4d5csdf-b5245afa35dgseh-yoipp99e7b-0f0d1e4bzsae2fsaa55")
                    {
                        Condition = new PlayerAllegiancePersons(){ MinMembers = 2 }
                        ,
                        MaxFirings = 1,
                        Actions = new EventActionType[]
                        {
                            new TalkAction("c16afs25462fg8-c8d3-4e19-9e82-1115dcggz-e6d45saff07")
                            {
                                DelayInSeconds = 0,
                               
                                    TalkPriority = TalkAction.TalkActionPriority.Normal,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                                    CanTalkWhileFighting = true,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = true,
                                    ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                    DefaultText = "Whipjaw is on the prowl!" 
                                
                            },
                            new TalkAction("f3f4afd32-vcfmip-9-bb29-da34tdc47271ee0")
                            {
                                DelayInSeconds = 2.5,
                               
                                    TalkPriority = TalkAction.TalkActionPriority.Normal,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                                    CanTalkWhileFighting = true,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = true,
                                    ActionByAgent = ActionByAgent.RandomInAllegiance,
                                    DefaultText = "She has to learn to stay away!" 
                                
                            }
                        }
                    }
                    #endregion
                }
            });

            list.Add(new ActionSets() //talkAction placeholder
            {
                FireMode = ActionSetsToFire.RandomValid,
                KeyName = "detectSwampDemonTreeTalk",
                SetsOfActions = new []
                {
                    #region actionSet#1
                    new ActionSetType("a5232bf4-6764-4dsaffe5466sdf-be7b-0f0d1e4azdase32qb2a55")
                    {
                        Condition = new PlayerAllegiancePersons(){ MinMembers = 2 }
                        ,
                        MaxFirings = 1,
                        Actions = new EventActionType[]
                        {
                            new TalkAction("c161asfa32523sdfg8-c8d3-4eyruuy19-af9e82-1115dc6d43wqasad4507")
                            {
                                DelayInSeconds = 0,
                               
                                    TalkPriority = TalkAction.TalkActionPriority.Normal,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                                    CanTalkWhileFighting = true,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = true,
                                    ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                    DefaultText = "I spotted the old swamp man!" 
                                
                            },
                            new TalkAction("f3f420gg9f-a0a5-401fa9-bzvzvb29-z4rda34twwrdc47271ee0")
                            {
                                DelayInSeconds = 2.5,
                               
                                    TalkPriority = TalkAction.TalkActionPriority.Normal,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                                    CanTalkWhileFighting = true,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = true,
                                    ActionByAgent = ActionByAgent.RandomInAllegiance,
                                    DefaultText = "Stay away from him. He'll drag you down!" 
                                
                            }
                        }
                    },
                    #endregion
                    #region actionSet#2
                    new ActionSetType("dz4a5dd2232bf4-6764-4d55scsdf-be7b-0f0d1ez4xzgta44b2a55")
                    {
                        Condition = new PlayerAllegiancePersons(){ MinMembers = 2 }
                        ,
                        MaxFirings = 1,
                        Actions = new EventActionType[]
                        {
                            new TalkAction("c1618c9sdfg8-c8d3-4e19asf235-1115dc6566-6hyp746d4507")
                            {
                                DelayInSeconds = 0,
                               
                                    TalkPriority = TalkAction.TalkActionPriority.Normal,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.First,
                                    CanTalkWhileFighting = true,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = true,
                                    ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
                                    DefaultText = "The old swamp man is out hunting..." 
                                
                            },
                            new TalkAction("56f3f42fs09f-a0gga5-4019-bb29-dadwa34tdc4dfh-w444xc7271ee0")
                            {
                                DelayInSeconds = 2.5,
                               
                                    TalkPriority = TalkAction.TalkActionPriority.Normal,
                                    SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
                                    CanTalkWhileFighting = true,
                                    CanTalkWhileSleeping = false,
                                    CanTalkWhileThreatened = true,
                                    ActionByAgent = ActionByAgent.RandomInAllegiance,
                                    DefaultText = "Hopefully he stays away, for his own sake!" 
                                
                            }
                        }
                    }
                    #endregion
                }
            });

            

            #endregion

          
   
            #region//// Continuous spawns


            #region Turnips North
            list.Add(new ActionSets()
            {
                FireMode = ActionSetsToFire.RandomValid,  // chooses randomly between the groups below  for continous spawning.
                KeyName = "continualSpawnTurnipNorth",
                SetsOfActions = new []
                { 
                    new ActionSetType("ef6e8bef-0afb-4b0f-8365-50faf2323tyyup01eba")
                    {                                                       
                        Actions = new EventActionType[]
                        {   
                            new SpawnEntityAction("1575ee05-443b-431b-b379-d3sunbae9229") { DelayInSeconds = 0.1,
                           EntityData = new EntityData()
                                { EntityKey = "entity:turnip", Name = "Turnip1", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "turnipAllegianceNorth" },
                                  Location = new Vector3(826,1114, 0), Bulk = 4.2f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, RaceKey = "Pale Turnip" } }, },
                        }    
                    },
                    new ActionSetType ("9833b74b-5120-4181-bsaf3251ea8-d0f7405c7d50")
                    {
                        Actions = new EventActionType[]
                        {
                            new SpawnEntityAction("25ee2bc6-e7cf-4a79-8644-94asf2352346uyyuip6ba4") { DelayInSeconds = 0.1,
                           EntityData = new EntityData()
                                { EntityKey = "entity:turnip", Name = "Turnip2", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "turnipAllegianceNorth" },
                                  Location = new Vector3(768,1200, 0), Bulk = 4.1f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult, RaceKey = "Pale Turnip" } }, },
                        }
                    },         
     
                }
            });
            #endregion

            #region binal rats #3
            list.Add(new ActionSets()
            {
                FireMode = ActionSetsToFire.RandomValid,
                KeyName = "continualSpawnBinalRats#3",
                SetsOfActions = new []
                { 
                    new ActionSetType("3e1fe44e-1e39-49cc-bb64-ffxca6dba0832c")
                    {                                                       
                        Actions = new EventActionType[]
                        {   
                            new SpawnEntityAction("f39d3dff8-2268-4613-9faw242afs-1372b18710fxdr98") 
                            { 
                                DelayInSeconds = 0.1,
                               
                                    EntityData = new EntityData()
                                    { 
                                        EntityKey = "entity:binalRat", Name = "BinalRat1(6048,4800)",
                                        MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegiance#3" },
                                        Location = new Vector3(766,1188, 0), Bulk = 0.28f, BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult } 
                                    }, 
                                
                            },
                        }    
                    },
                    new ActionSetType ("687280f3-a63a-45serd3-8cfd-76csrhadddbaffc")
                    {
                        Actions = new EventActionType[]
                        {
                            new SpawnEntityAction("f608b57c-0847-442d-abwa4223-c57fbsdrsr7f395de") 
                            { 
                                DelayInSeconds = 0.1,
                               
                                    EntityData = new EntityData()
                                    { 
                                        EntityKey = "entity:binalRat", Name = "BinalRat(5280,5040)", 
                                        MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegiance#3" },
                                        Location = new Vector3(577,1299, 0), Bulk = 0.28f, BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult } 
                                    }, 
                                
                            },
                        }
                    },
                }
            });
            #endregion

            # region binal rats #2
            list.Add(new ActionSets()
            {
                FireMode = ActionSetsToFire.RandomValid,  // chooses randomly between the groups below  for continous spawning.
                KeyName = "continualSpawnBinalRats#2",
                SetsOfActions = new []
                { 
                    new ActionSetType("3e1fe44e-1e39-49cc-bb64-ffa6dba0832c")
                    {                                                       
                        Actions = new EventActionType[]
                        {   
                            new SpawnEntityAction("f39d3dff8-2268-4613-9477-1372b1871098") { DelayInSeconds = 0.1,
                           EntityData = new EntityData()
                                { EntityKey = "entity:binalRat", Name = "BinalRat1", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegiance#2" },
                                  Location = new Vector3(965,2069, 0), Bulk = 0.21f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult } }, },
                        }    
                    },
                    new ActionSetType ("687280f3-a63a-45d3-8cfd-76cadddbaffc")
                    {
                        Actions = new EventActionType[]
                        {
                            new SpawnEntityAction("f608b57c-0847-442d-ab93-c57fb7f395de") { DelayInSeconds = 0.1,
                           EntityData = new EntityData()
                                { EntityKey = "entity:binalRat", Name = "BinalRat2", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegiance#2" },
                                  Location = new Vector3(900,2000, 0), Bulk = 0.21f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult } }, },
                        }
                    },

                }
            });
            #endregion

            # region binal rats #1
            list.Add(new ActionSets()
            {
                FireMode = ActionSetsToFire.RandomValid,  // chooses randomly between the groups below  for continous spawning.
                KeyName = "continualSpawnBinalRats#1",
                SetsOfActions = new []
                { 
                    new ActionSetType("7f269bf2-a78c-45b5-a8f8-21bd269681ec")
                    {                                                       
                        Actions = new EventActionType[]
                        {   
                            new SpawnEntityAction("f38e9025-0b17-4940-aba0-13e0a22003c1") { DelayInSeconds = 0.1,
                           EntityData = new EntityData()
                                { EntityKey = "entity:binalRat", Name = "BinalRat1", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "binalRatAllegiance#1" },
                                  Location = new Vector3(2309,530, 0), Bulk = 0.21f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult } }, },
                        }    
                    },
                }
            });
            #endregion


            #region thunder chicken #2
            list.Add(new ActionSets()
            {
                FireMode = ActionSetsToFire.RandomValid,  // chooses randomly between the groups below  for continous spawning.
                KeyName = "continualSpawnThunderChicken#2",
                SetsOfActions = new []
                { 
                    new ActionSetType("8203c1d8-0esdf5d-44ee-9407-b374091ed5d5")
                    {                                                       
                        Actions = new EventActionType[]
                        {   
                            new SpawnEntityAction("91dfzsb2390e2-6a2425rfas3a8-b1f6-86a2f352d843") 
                            { 
                                DelayInSeconds = 0.1,
                               
                                    EntityData = new EntityData()
                                    { 
                                        EntityKey = "entity:studdedThunderChicken", Name = "ThunderChicken(3120,2688)", 
                                        MemberOf = new AllegianceAndExpedition() { AllegianceKey = "thunderChickenAllegiance#2" },
                                        Location = new Vector3(1490,1010, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult} 
                                    }, 
                                
                            },
                        }
                    },
                    new ActionSetType("8203c1d8-0esdf5d-44ee-9407-b374xdf091ed5d5")
                    {                                                       
                        Actions = new EventActionType[]
                        {   
                            new SpawnEntityAction("91dfzsb2390e2fawad2424aa8-bdse56-86a2f352d843") 
                            { 
                                DelayInSeconds = 0.1,
                               
                                    EntityData = new EntityData()
                                    { 
                                        EntityKey = "entity:studdedThunderChicken", Name = "ThunderChicken(1584,2736)", 
                                        MemberOf = new AllegianceAndExpedition() { AllegianceKey = "thunderChickenAllegiance#2" },
                                        Location = new Vector3(1384,906, 0), Bulk = 0.3f, BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult} 
                                    }, 
                                
                            },
                        }
                    },

                }
            });
            #endregion


            #region Snatcher
            list.Add(new ActionSets()
            {
                FireMode = ActionSetsToFire.RandomValid,  // chooses randomly between the groups below  for continous spawning.
                KeyName = "continualSpawnSnatchers#1",
                SetsOfActions = new []
                { 
                    new ActionSetType("3eae1eb9-fd76-42b5-9a12-52qtq32tegrhtyipe1a2")
                    {                                                       
                        Actions = new EventActionType[]
                        {   
                            new SpawnEntityAction("4906cf65-f9f8-4590-8dd9-3f0d0253egrwsj7e") { DelayInSeconds = 0.1,
                           EntityData = new EntityData()
                                { EntityKey = "entity:whipjaw", Name = "Whipjaw", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "snatcherAllegiance#1" },
                                  Location = new Vector3(1109,2406, 0), Bulk = 2f, BioEntity = new Maps.MapEditor.BiologicalEntity() //5725,3845
                                { AgeGroup = AIAgeGroup.Adult } }, },
                        }    
                    },
                    new ActionSetType ("8ed0a13c-8590-417e-a246-63d6c26bbe69")
                    {
                        Actions = new EventActionType[]
                        {
                            new SpawnEntityAction("a2dd9aef-2011-4673-a7ca-99cf7a72c7c3") { DelayInSeconds = 0.1,
                           EntityData = new EntityData()
                                { EntityKey = "entity:whipjaw", Name = "Whipjaw", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "snatcherAllegiance#1" },
                                  Location = new Vector3(1000, 2340, 0), Bulk = 2f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult,  } }, },
                        }
                    },
                }
            });

            #endregion
     

            /*
            #region swampDemonTree #2 north
            list.Add(new ActionSets()
            {
                FireMode = ActionSetsToFire.RandomValid,  // chooses randomly between the groups below  for continous spawning.
                KeyName = "continualSwampDemonTree#2",
                SetsOfActions = new []
                { 
                    new ActionSetType("3eaaga3265629-fd76-42b5-9a12-526zxzdb2250e1a2")
                    {                                                       
                        Actions = new EventActionType[]
                        {   
                            new SpawnEntityAction("4906cf65-f9f8-4590-8dd9-3agf3hd08zxczg1b487e") 
                            {
                                DelayInSeconds = 0.1,
                               
                                    EntityData = new EntityData()
                                    {
                                        EntityKey = "entity:swampDendront", Name = "Swamp demon tree south", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "swampDemonTreeAllegiance#2" },
                                        Location = new Vector3(2602,624, 0), Bulk = 1.1f, 
                                        BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult} 
                                    }, 
                                
                            },
                        }
                    },
                }
            });
            #endregion
            */

            /*
            #region swamp slugs (worms / megapods)
            list.Add(new ActionSets()
            {
                FireMode = ActionSetsToFire.RandomValid,  // chooses randomly between the groups below  for continous spawning.
                KeyName = "continualSpawnSlugs",
                SetsOfActions = new []
                {
                    new ActionSetType("c2321887-d2eawf356253a0f8-73d7d00efcd8")
                    {                                                       
                        Actions = new EventActionType[]
                        {   
                            new SpawnEntityAction("453500fb-e178-4b13a52t2ef-79a2571945eb") {
                           EntityData = new EntityData()
                                { EntityKey = "entity:megapod", Name = "Megapod1", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "slugAllegiance#1" },
                                  Location = new Vector3(2410,485, 0), Bulk = 1.1f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult } }, },
                        }    
                    },
                    new ActionSetType("1azw53-489f-9654-0aa5ba257958")
                    {                                                       
                        Actions = new EventActionType[]
                        {   
                            new SpawnEntityAction("1f2d70fawa2525a439-ae13-657b72031cba") {
                           EntityData = new EntityData()
                                { EntityKey = "entity:megapod", Name = "Megapod2", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "slugAllegiance#1" },
                                  Location = new Vector3(2300,456, 0), Bulk = 1.1f, BioEntity = new Maps.MapEditor.BiologicalEntity()
                                { AgeGroup = AIAgeGroup.Adult } }, }},
                            
                    },     
                }
            });
            #endregion*/

            #region leafcutter Quadite
            #region  #1
            list.Add(new ActionSets()
            {
                FireMode = ActionSetsToFire.RandomValid,  // chooses randomly between the groups below  for continous spawning.
                KeyName = "continualSpawnLeafcutter#1",
                SetsOfActions = new []
                {
                    #region spawns
                    new ActionSetType("d71132d7-1383-415d-92dba356t64366a2ae6a8c")
                    {                                                       
                        Actions = new EventActionType[]
                        {   
                            new SpawnEntityAction("99875cc0-30ae-2a4rfwf2fe-06cdc739ecfe")
                            {
                               
                                    EntityData = new EntityData()
                                    { 
                                        EntityKey = "entity:fieldQuadite", Name = "leafcutter(1728,1008)", MemberOf = new AllegianceAndExpedition() { AllegianceKey = "leafcutterAllegiance#1" },
                                        Location = new Vector3(1210,1162, 0), Bulk = 0.20f,
                                        BioEntity = new Maps.MapEditor.BiologicalEntity(){ AgeGroup = AIAgeGroup.Adult} 
                                    }, 
                                
                            },
                        }    
                    },
                    #endregion
                }
            });
            #endregion

            #endregion
            #endregion



            #region extrusionMachineScreen
            list.Add(new ActionSets()
            {
                KeyName = "extrusionMachineArrivalScreen",
                SetsOfActions = new []{ new ActionSetType("rt47yurtwy654wyurwtyuteyutyu99d88b")
                        {     
                             Condition = new PlayerAllegiancePersons(){ MinMembers = 2 }
                             , 
                        MaxFirings = 1,
                        Actions = new EventActionType[]{ //mp dont add banter first because it will often collide with immigration greetings

                        new EventActionDialog("eyutey678r67irtyutyutyueuif4e51") { DelayInSeconds = 5, //give time for immi greetings
                       //todo: spawn name of orig. townsperson
                         DisplayText = new DynamicText(){ Text = "-Aha. The extrusion machine. So that's what it looks like. Ok, let's get the polymer workshop set up then. And get the marshcot sap ready in the meantime. The chemist is probably keen to start working. Where is the chemist by the way?" ,  //  
                        },
                        DisplayImage = "GroupMeeting",
                         DialogOptions = new[] { new DialogOption()
                         {
                            Text = "GUIDE #2", Tooltip = "See how to build the polymer workshop (opens separate window.)", ActiveInArchive = true,
                            ActionSet = "showTutorialRubberScenario5_2" //
                         }}                  
                                                 
                        }
                    }
                    }
                  }
            });
            #endregion

            #region makeGasketsFinishedScreen
            list.Add(new ActionSets()
            {
                KeyName = "makeGasketsFinishedScreen", //

                SetsOfActions = new []{ new ActionSetType("dbrer2465462546254rgh56y5y56ererereret6c0")
                     {                     
                        MaxFirings = 1, 
                        Condition = new PlayerAllegiancePersons(){ MinMembers = 2 }
                        ,                        
                       Actions = new EventActionType[]{
                           

                       new EventActionDialog("t9999456775637567ggggggggggdfghgfdghdhfe4f4e51") { DelayInSeconds = 1, // 
                       
                         DisplayText = new DynamicText(){ Text = "-What's that smell? \n-Neson:  Just completed the first batch of rubber parts. The vulcanization process gives off a distinct odour. \n-Smells like money, right? \n-Neson: Something like that. These components are high quality. You got some high-grade sap in that swamp of yours. \n-Great! I'll tell the others so we can get those things on the market as soon as possible. \n-Neson: I suggest you sell to Zenig Station first. That way we can buy some of their sulfur at the same time. We wouldn't want to run out! \n-Right. Anyway, I think you should do a few more batches before we start trading. And then we'll be on our way to reaching our goal." ,  //  
                        },
                        DisplayImage = "GroupMeeting",
                         DialogOptions = new[] { new DialogOption()
                         {
                            Text = "GUIDE #3", Tooltip = "See how to sell rubber parts (opens separate window.)", ActiveInArchive = true,
                            ActionSet = "showTutorialRubberScenario5_3" 
                         }}                   
                               
          

                        }
                     }
                   }
                 }
            });

            #endregion

            #region //Tutorial windows (scenario 5)  REDIRECTION.

         /////   //tutorial texts are in BaseData\BaseDataLoader.cs///////

            list.Add(new ActionSets()
            {
                // opens the tut window when the dialog button is clicked
                KeyName = "showTutorialRubberScenario5_1",
                SetsOfActions = new []{ new ActionSetType("645teyujtey7teytjtjdgj82b")
                {                                 
                    Actions = new EventActionType[]{ new ShowTutorialAction("01f2dghjtytejtujktdejdtjtyjdgfgd7d2f30") 
                    {   
                        
                            TutorialPageKey = "tutorialRubberScenario5_1"
                                                                        
                    }
                }
                }
            }
            });

            list.Add(new ActionSets()
            {
                // opens the tut window when the dialog button is clicked
                KeyName = "showTutorialRubberScenario5_2",
                SetsOfActions = new []{ new ActionSetType("645t56756y7uhwrtsuhj5qaw644jw57juigj82b")
                {                                 
                    Actions = new EventActionType[]{ new ShowTutorialAction("01f2dghjtytejtujghjjdrwr56y4wyw545yd2f30") 
                    {   
                        
                            TutorialPageKey = "tutorialRubberScenario5_2"
                                                                        
                    }
                }
                }
            }
            });

            list.Add(new ActionSets()
            {
                // opens the tut window when the dialog button is clicked
                KeyName = "showTutorialRubberScenario5_3",
                SetsOfActions = new []{ new ActionSetType("645t56756y45647rtyrswyrtswysy7juigj82b")
                {                                 
                    Actions = new EventActionType[]{ new ShowTutorialAction("01f2dghjtytejt546rtyssy56wewsreyrew4wyw545yd2f30") 
                    {   
                        
                            TutorialPageKey = "tutorialRubberScenario5_3"
                                                                        
                    }
                }
                }
            }
            });
            #endregion


            #region nest special action and respawns 
            double nestSpawnInterval = 2400d; //mp july 2016 was 1600d   but i think it should take a bit longer before nest respawns. 
            list.Add(new ActionSets() 
            {
                FireMode = ActionSetsToFire.AllValid,
                KeyName = "bigBombActivated",
                ActionTargets = new TargetObject()
                {
                    TargetObjectType = TargetObjectType.TargetEntity
                },
                SetsOfActions = new []
                {
                    new ActionSetType("d92asad626701d-ab98-447d-9b02-7ccc426426dgsdbfbe39f5")
                    {                                                       
                        Actions = new EventActionType[]
                        {
                            new ParticleEffectAction("s91efsffe191-d051-4sxc3ce-3535859f-24axccx284ce1504")
                            {
                                DelayInSeconds = 5,                              
                                UseLocationOfEntity = new TargetObject(){ TargetObjectType = TargetObjectType.TargetEntity },
                                DurationInSeconds = 0.4d,
                                ParticleEmitters = new[]
                                {
                                    new ParticleEmitterEffect()
                                    {
                                        AttachToEntity = false,
                                        ParticleSystemKey = "explosionSmokeCloud",
                                    }
                                }                                
                            },
                            new ParticleEffectAction("xcc91efe1ccx91-d05xcxc1-43caae-859f-24asd284ce15-04")
                            {
                                DelayInSeconds = 5,                              
                                UseLocationOfEntity = new TargetObject(){ TargetObjectType = TargetObjectType.TargetEntity },
                                DurationInSeconds = 12d,
                                ParticleEmitters = new[]
                                {
                                    new ParticleEmitterEffect()
                                    {
                                        AttachToEntity = false,
                                        ParticleSystemKey = "explosionSmokeCloudLong",
                                    }
                                }                                
                            },
                            new ParticleEffectAction("91esddfe191-dcxc051-43crereyye-859dfff-24a284ce1504")
                            {
                                DelayInSeconds = 5,                               
                                UseLocationOfEntity = new TargetObject(){ TargetObjectType = TargetObjectType.TargetEntity },
                                DurationInSeconds = 0.4d,
                                ParticleEmitters = new[]
                                {
                                    new ParticleEmitterEffect()
                                    {
                                        AttachToEntity = false,
                                        ParticleSystemKey = "explosion",
                                    }
                                }                                
                            },
                            new ParticleEffectAction("91efsdsdawwqe191-d05rqxv1-43sdce-859f-24a284ce1504")
                            {
                                DelayInSeconds = 5,                               
                                UseLocationOfEntity = new TargetObject(){ TargetObjectType = TargetObjectType.TargetEntity },
                                DurationInSeconds = 0.4d,
                                ParticleEmitters = new[]
                                {
                                    new ParticleEmitterEffect()
                                    {
                                        AttachToEntity = false,
                                        ParticleSystemKey = "mineExplosion",
                                    }
                                }                               
                            },
                            new SoundEffectAction("91efe191-d051-43ce-859f-24ada-t35252sw-284ce1504")
                            {
                                DelayInSeconds = 5d,
                                KeyName ="traps/mineExplosionHardwDebris",
                                Sound = "traps/mineExplosionHardwDebris"                                
                            },
                            new DestroyEntityAction("edf63fa265263ea9-egdag624e2-462edc37-8312-7ce1944a9d9e")
                            {
                                DelayInSeconds = 5.5d, // make sure the particles have been started before destroying the target, otherwise the particles won't have a start location
                                
                                TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.TargetEntity }                                
                            },
                        }
                    },
                    #region respawn #1
                    new ActionSetType("d92a70sada25251d-afdsaf32523-47d-9b02-7cccbfbe39f5")
                    {
                        Condition = new CustomCondition()
                        {
                            TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.TargetEntity },
                            PropertyCondition = new PropertyCondition()
                            {
                                PropertyKey = "name",
                                ConstantStringEqual = "Field quadite nest 1" 
                            }                            
                        },
                        Actions = new EventActionType[]
                        {
                            new SpawnEntityAction("edf6saf3252533ea9-e4af3262e2-4c3sg257-8312-7ce1944a9d9e")
                            {
                                DelayInSeconds = nestSpawnInterval,                                   
                                EntityDataKey = new ValueNode(){ String = "fieldQuaditeNest1" }                                                           
                            }                           
                        }
                    },
                    #endregion                   
                 
                }
            });
            #endregion
            return list;
         
        }
    }
}
