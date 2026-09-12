using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_5.Data
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
                KeyName = "detectSwampDemonTreeHook",
                TypeKey = "entity:human",
                DetectedEntityKey = "entity:swampDendront",
                ActionSetsKey = "detectSwampDemonTreeTalk" //
            });

            list.Add(new DetectEntityTypeHook()
            {
                KeyName = "detectWhipjawHook",
                TypeKey = "entity:human",
                DetectedEntityKey = "entity:whipjaw",
                ActionSetsKey = "detectWhipjawTalk"
            });

            list.Add(new DetectEntityTypeHook()
            {
                KeyName = "detectTwinklerHook",
                TypeKey = "entity:human",
                DetectedEntityKey = "entity:twinkler",
                ActionSetsKey = "detectTwinklerTalk"
            });

            list.Add(new DetectEntityTypeHook()
            {
                KeyName = "detectPygmyThunderChicken",
                TypeKey = "entity:human",
                DetectedEntityKey = "entity:pygmyThunderChicken",
                ActionSetsKey = "detectPygmyThunderChicken"
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
