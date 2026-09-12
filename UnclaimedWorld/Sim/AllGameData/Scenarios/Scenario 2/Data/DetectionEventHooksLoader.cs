using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_2.Data
{
    public class DetectionEventHooksLoader
    {
        public static List<DetectEntityTypeHook> InitDetectEntityTypeHooks()
        {
            List<DetectEntityTypeHook> list = new List<DetectEntityTypeHook>();






            list.Add(new DetectEntityTypeHook()
            {
                KeyName = "detectDemonTreeHook",
                TypeKey = "entity:human",
                DetectedEntityKey = "entity:spoakDendront",
                ActionSetsKey = "detectDemonTreeTalk"
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

          
            return list;

            //BaseDataLoader.SerializeAndDeserializeTypeList(list, GameData.Instance.AllDetectedEntityEventHooks, "", "detectedEntityHooks.xml", Config.DataType.BaseData);

        }

        public static List<DetectResourceTypeHook> InitDetectResourceTypeHooks()
        {
            List<DetectResourceTypeHook> list = new List<DetectResourceTypeHook>();

            return list;

            //  BaseDataLoader.SerializeAndDeserializeTypeList(list, GameData.Instance.AllDetectedResourceEventHooks, "", "detectedResourceHooks.xml", Config.DataType.BaseData);

        }


    }
}
