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
using UWGame.SimSide.Entities.Biological;
//using UWGame.SimSide.InGameEvents.Expressions;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_4x.Data
{
    public class PolledEventsLoader
    {

        public static List<InGameEvents.PolledEventType> Init()
        {
            List<PolledEventType> list = new List<PolledEventType>();


            //TODO: finish migrate populations for bajingan and field quads nests

            #region Migration Spawns (moving from one end of map to the other etc.)

            #region migration Lesser Whipjaw East to West
            list.Add(new PolledEventType()
            {
                KeyName = "SANDBOXNOMADMAP_migrationLesserWhipjawEast",
                PollInterval = new ValueNode() { PropertyKey = "lesserWhipjawMigrationInterval" }, //(there's 1600 sec / day)  
                StartTimePoint = new TimePoint() { RelativeTimeInSeconds = new ValueNode() { PropertyKey = "lesserWhipjawMigrationInterval" }},
                AllowRandomTimeOffset = false,
                Condition = new CustomCondition()
                    {
                        // don't exceed max number:
                        TargetObject = new TargetObject()
                        {
                            GetList = new GetList()
                            {
                                HasPropertiesListKey = "allegiances",
                                FilterCondition = new PropertyCondition()
                                {
                                    PropertyKey = "keyName",
                                    ConstantStringEqual = "lesserWhipjawAllegianceWest"
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
                                PropertyKey = "maxLesserWhipjaw"
                            }
                        }
                },

                ActionSetsKey = "migrationLesserWhipjawEast"
            });
            #endregion

            #region migration Bajingan North to West
            list.Add(new PolledEventType()
            {
                KeyName = "SANDBOXNOMADMAP_migrationBajinganNorth",
                PollInterval = new ValueNode() { PropertyKey = "bajinganMigrationInterval" }, //(there's 1600 sec / day)  
                StartTimePoint = new TimePoint() { RelativeTimeInSeconds = new ValueNode() { PropertyKey = "bajinganMigrationInterval" } },             
                AllowRandomTimeOffset = false,
                Condition = new CustomCondition()
                    {
                        // don't exceed max number:
                        TargetObject = new TargetObject()
                        {
                            GetList = new GetList()
                            {
                                HasPropertiesListKey = "allegiances",
                                FilterCondition = new PropertyCondition()
                                {
                                    PropertyKey = "keyName",
                                    ConstantStringEqual = "bajinganAllegianceWest"
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
                                PropertyKey = "maxBajingan"
                            }
                        }
                },

                ActionSetsKey = "migrationBajinganNorth"
            });
            #endregion


            #endregion


            #region leafCutterNest
            list.Add(new PolledEventType()
            {
                KeyName = "SANDBOXNOMADMAP_spawnLeafcutterNests",               
                ActionSets = new ActionSets()
                {
                    SetsOfActions = new []
                    {
                        new ActionSetType("79dfgshsfghfsgzjshgfjs346x34fhjhs6e20")
                        {
                            Actions = new EventActionType[]
                            {
                                new SpawnEntityAction("4e8dghzjdgjdghjd56666665gjdgxjdg503c1")
                                {
                                   
                                        EntityData = new EntityData()
                                        {
                                            EntityKey = "terrain:fieldQuaditeNest",
                                            Name = "Leafcutter nest (coord. 33;27)", // search on this term and chane the coords in all cases
                                            Location = new Vector3(1584,1296,0),//1584,1296,0),//384,912(2208f, 1248f, 0), for testing at grassland
                                            Threat = new Threat() { ThreatGroupName = "leafcutterAllegiance#1" }
                                        }
                                    
                                },
                                new SpawnEntityAction("4e8dghjzdgjdghjd56x666665gjdgjdg503c3")
                                {
                                   
                                        EntityData = new EntityData()
                                        {
                                            EntityKey = "terrain:fieldQuaditeNest",
                                            Name = "Leafcutter nest (coord. 52;5)",
                                            Location = new Vector3(2496,240,0),//1536,1536, 0),
                                            Threat = new Threat() { ThreatGroupName = "leafcutterAllegiance#2" }
                                        }
                                    
                                },
                                new SpawnEntityAction("4e8dghjdgjdghjd5xz6666665gjdgjdg503c4")
                                {
                                   
                                        EntityData = new EntityData()
                                        {
                                            EntityKey = "terrain:fieldQuaditeNest",
                                            Name = "Leafcutter nest (coord. 75;26)",
                                            Location = new Vector3(3600,1248,0),//2496,288, 0),
                                            Threat = new Threat() { ThreatGroupName = "leafcutterAllegiance#3" }
                                        }
                                    
                                },
                                #region properties
                                new SetPropertyAction("4e8dasfa3wf33afaffwsfzjd56666665gjdgjdg503c7")
                                {
                                   
                                        TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.Root },
                                        PropertyKey = "nest1",Value = new ValueNode(){Bool = true }
                                    
                                },
                                new SetPropertyAction("4e8dghjdgjaws2524266665gjdgjdg503c7")
                                {
                                   
                                        TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.Root },
                                        PropertyKey = "nest2",Value = new ValueNode(){Bool = true }
                                    
                                },
                                new SetPropertyAction("4e8dghjdgjdghzjd5666awsfraw253afsf6665gjdgjdg503c7")
                                {
                                   
                                        TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.Root },
                                        PropertyKey = "nest3",Value = new ValueNode(){Bool = true }
                                    
                                },
                                new SetPropertyAction("4e8dghjdgjdghzjd56af2542a52agjdgjdg503c7")
                                {
                                   
                                        TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.Root },
                                        PropertyKey = "nest4",Value = new ValueNode(){Bool = true }
                                    
                                },
                                new SetPropertyAction("4e8dghjdgawfzjd56fawafwafafaawfwa66665gjdgjdg503c7")
                                {
                                   
                                        TargetObject = new TargetObject(){ TargetObjectType = TargetObjectType.Root },
                                        PropertyKey = "nest5",Value = new ValueNode(){Bool = true }
                                    
                                },
                                #endregion
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
