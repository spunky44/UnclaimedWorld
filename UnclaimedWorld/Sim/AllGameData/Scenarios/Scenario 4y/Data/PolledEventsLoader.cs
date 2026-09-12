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

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_4y.Data
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
                StartTimePoint = new TimePoint() { RelativeTimeInSeconds = new ValueNode() { PropertyKey = "lesserWhipjawMigrationInterval" } },
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

            return list;
        }
    }
}
