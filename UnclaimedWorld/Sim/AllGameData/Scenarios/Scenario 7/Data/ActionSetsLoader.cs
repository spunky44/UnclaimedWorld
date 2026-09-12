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

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_7.Data
{
    public class ActionSetsLoader
    {

        public static List<ActionSets> Init()
        {
            List<ActionSets> list = new List<ActionSets>();
                     
           
            #region nest special action and respawns
            double nestSpawnInterval = 2400d; //mp july 2016 was 1600d   but i think it should take a bit longer before nest respawns. // was 2400d
            list.Add(new ActionSets()
            {
                FireMode = ActionSetsToFire.AllValid,
                KeyName = "bigBombActivated",
                ActionTargets = new TargetObject()
                {
                    TargetObjectType = TargetObjectType.TargetEntity
                },
                SetsOfActions = new[]
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
                    #region respawn field quadite nest #1
                    new ActionSetType("101cc4ee-a07e-4c97-94e2-9c5281791412")
                    {
                        Condition = new CustomCondition()
                        {
                            TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.TargetEntity },
                            PropertyCondition = new PropertyCondition()
                            {
                                PropertyKey = "name",
                                ConstantStringEqual = "Field Quadite Nest 1" 
                            }                            
                        },
                        Actions = new EventActionType[]
                        {
                            new SpawnEntityAction("f295b796-1987-405a-80c5-552b1ab5f29d")
                            {
                                DelayInSeconds = nestSpawnInterval,                                   
                                EntityDataKey = new ValueNode(){ String = "fieldQuaditeNest1" }                                                           
                            }                           
                        }
                    },
                    #endregion  
                    #region respawn #1
                    new ActionSetType("d92a70sada25251d-afdsaf32523-47d-9b02-7cccbfbe39f5")
                    {
                        Condition = new CustomCondition()
                        {
                            TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.TargetEntity },
                            PropertyCondition = new PropertyCondition()
                            {
                                PropertyKey = "name",
                                ConstantStringEqual = "Swarmer nest 1" 
                            }                            
                        },
                        Actions = new EventActionType[]
                        {
                            new SpawnEntityAction("edf6saf3252533ea9-e4af3262e2-4c3sg257-8312-7ce1944a9d9e")
                            {
                                DelayInSeconds = nestSpawnInterval,                                   
                                EntityDataKey = new ValueNode(){ String = "swarmerNest1" /*"fieldQuaditeNest1"*/ }                                                           
                            }                           
                        }
                    },
                    #endregion  
                    #region respawn #2
                    new ActionSetType("d92a70sada25251awfasdd-afdsaf3ddwa2523-47d-9b02-7cccbfbe39f5")
                    {
                        Condition = new CustomCondition()
                        {
                            TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.TargetEntity },
                            PropertyCondition = new PropertyCondition()
                            {
                                PropertyKey = "name",
                                ConstantStringEqual = "Swarmer nest 2" 
                            }                            
                        },
                        Actions = new EventActionType[]
                        {
                            new SpawnEntityAction("edf6saf3252533sadaea9-e4af3262e2-4c3sg257-8312-7ce1944a9d9e")
                            {
                                DelayInSeconds = nestSpawnInterval,                                   
                                EntityDataKey = new ValueNode(){ String = "swarmerNest2" /* "fieldQuaditeNest1"*/ }                                                           
                            }                           
                        }
                    },
                    #endregion 
                    #region respawn #3
                    new ActionSetType("d92a70sada252afa3f3ff51d-afdsaf32523-47d-9b02-7cccbfbe39f5")
                    {
                        Condition = new CustomCondition()
                        {
                            TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.TargetEntity },
                            PropertyCondition = new PropertyCondition()
                            {
                                PropertyKey = "name",
                                ConstantStringEqual = "Swarmer nest 3" 
                            }                            
                        },
                        Actions = new EventActionType[]
                        {
                            new SpawnEntityAction("edf6saf32525egtytrtews33ea9-e4af3262e2-4c3sg257-8312-7ce1944a9d9e")
                            {
                                DelayInSeconds = nestSpawnInterval,                                   
                                EntityDataKey = new ValueNode(){ String = "swarmerNest3" }                                                           
                            }                           
                        }
                    },
                    #endregion 
                    #region respawn #4
                    new ActionSetType("d92a70sada252afa3f3ffesju6bv51d-afdsaf32523-47d-9b02-7cetccbfbe39f5")
                    {
                        Condition = new CustomCondition()
                        {
                            TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.TargetEntity },
                            PropertyCondition = new PropertyCondition()
                            {
                                PropertyKey = "name",
                                ConstantStringEqual = "Swarmer nest 4" 
                            }                            
                        },
                        Actions = new EventActionType[]
                        {
                            new SpawnEntityAction("edf6saf3252hkslnbv5egtytrtews33ea9-e4af3262e2-4c3sg257-8312-7ce1944a9d9e")
                            {
                                DelayInSeconds = nestSpawnInterval,                                   
                                EntityDataKey = new ValueNode(){ String = "swarmerNest4" }                                                           
                            }                           
                        }
                    },
                    #endregion 
                    #region respawn #5
                    new ActionSetType("d92a70sada252afa3fjkiaww3ff51d-afdsaf32523-47d-9b02-7cccbfbe39f5")
                    {
                        Condition = new CustomCondition()
                        {
                            TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.TargetEntity },
                            PropertyCondition = new PropertyCondition()
                            {
                                PropertyKey = "name",
                                ConstantStringEqual = "Swarmer nest 5" 
                            }                            
                        },
                        Actions = new EventActionType[]
                        {
                            new SpawnEntityAction("edf6saf32sadwaghjj525egtytrtews33ea9-e4af3262e2-4c3sg257-8312-7ce1944a9d9e")
                            {
                                DelayInSeconds = nestSpawnInterval,                                   
                                EntityDataKey = new ValueNode(){ String = "swarmerNest5" }                                                           
                            }                           
                        }
                    },
                    #endregion 
                    #region respawn #6
                    new ActionSetType("d92a70sada252afa3f53533fds3ff51d-afdsaf32523-47d-9b02-7cccbfbe39f5")
                    {
                        Condition = new CustomCondition()
                        {
                            TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.TargetEntity },
                            PropertyCondition = new PropertyCondition()
                            {
                                PropertyKey = "name",
                                ConstantStringEqual = "Swarmer nest 6" 
                            }                            
                        },
                        Actions = new EventActionType[]
                        {
                            new SpawnEntityAction("edf6saf32525egtytrte51ws33ea9-e4af3262e2-4c3sg257-8312-7ce1944saavwha9d9e")
                            {
                                DelayInSeconds = nestSpawnInterval,                                   
                                EntityDataKey = new ValueNode(){ String = "swarmerNest6" }                                                           
                            }                           
                        }
                    },
                    #endregion 
                 
                }
            });
            #endregion

            #region //Tutorial GUIDE window (scenario 7)  REDIRECTION.

            /////   //tutorial texts are in BaseData\BaseDataLoader.cs///////

            list.Add(new ActionSets()
            {
                // opens the tut window when the dialog button is clicked
                KeyName = "showTutorialMiningScenario7",
                SetsOfActions = new[]{ new ActionSetType("645tdghjhdgkgdjkfjkdjhj82b")
                {                                 
                    Actions = new EventActionType[]{ new ShowTutorialAction("01f2dfhkjfljfhfhklfhljfhfgd7d2f30") 
                    {   
                        
                            TutorialPageKey = "tutorialMiningScenario7"
                                                                        
                    }
                }
                }
            }
            });


            #endregion

            return list;
         
        }
    }
}
