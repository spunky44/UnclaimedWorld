using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_6.Data
{
    public class DetectionEventHooksLoader
    {
        public static List<DetectEntityTypeHook> InitDetectEntityTypeHooks()
        {
            List<DetectEntityTypeHook> list = new List<DetectEntityTypeHook>();
            list.Add(new DetectEntityTypeHook()
            {
                KeyName = "detectWhiteThunderChicken",
                TypeKey = "entity:human",
                DetectedEntityKey = "entity:whiteThunderChicken",
                ActionSetsKey = "detectWhiteThunderChicken"
            });


            list.Add(new DetectEntityTypeHook()
            {
                KeyName = "detectPygmyThunderChicken",
                TypeKey = "entity:human",
                DetectedEntityKey = "entity:pygmyThunderChicken",
                ActionSetsKey = "detectPygmyThunderChicken"
            });

            list.Add(new DetectEntityTypeHook()
            {
                KeyName = "detectPatricianHook",
                TypeKey = "entity:human",
                DetectedEntityKey = "entity:patrician",
                ActionSetsKey = "detectPatrician"
            });

            list.Add(new DetectEntityTypeHook()
            {
                KeyName = "detectMudWormHook",
                TypeKey = "entity:human",
                DetectedEntityKey = "entity:mudWorm",
                ActionSetsKey = "detectMudWorm"
            });
            return list;
        }

        public static List<DetectResourceTypeHook> InitDetectResourceTypeHooks()
        {
            List<DetectResourceTypeHook> list = new List<DetectResourceTypeHook>();

            return list;  
        }
    }
}
