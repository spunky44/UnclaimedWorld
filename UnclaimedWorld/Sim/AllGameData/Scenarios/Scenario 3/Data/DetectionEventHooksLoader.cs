using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_3.Data
{
    public class DetectionEventHooksLoader
    {
        public static List<DetectEntityTypeHook> InitDetectEntityTypeHooks()
        {
            List<DetectEntityTypeHook> list = new List<DetectEntityTypeHook>();


            list.Add(new DetectEntityTypeHook()
            {
                KeyName = "detectCrevice",
                TypeKey = "entity:human",
                DetectedEntityKey = "terrain:crevice",
                ActionSetsKey = "detectCreviceRemark"
            });


            list.Add(new DetectEntityTypeHook()
            {
                KeyName = "detectBushDragon",
                TypeKey = "entity:human",
                DetectedEntityKey = "entity:bushDragon",
                ActionSetsKey = "detectBushDragon"
            });

 

            return list;

            //BaseDataLoader.SerializeAndDeserializeTypeList(list, GameData.Instance.AllDetectedEntityEventHooks, "", "detectedEntityHooks.xml", Config.DataType.BaseData);

        }

        public static List<DetectResourceTypeHook> InitDetectResourceTypeHooks()
        {
            List<DetectResourceTypeHook> list = new List<DetectResourceTypeHook>();

            list.Add(new DetectResourceTypeHook()
            {
                KeyName = "detectcommonOilTubers",
                TypeKey = "entity:human",
                DetectedResourceKey = "commonOilTubers",
                ActionSetsKey = "detectcommonOilTubersRemark"

            });




            return list;

            //  BaseDataLoader.SerializeAndDeserializeTypeList(list, GameData.Instance.AllDetectedResourceEventHooks, "", "detectedResourceHooks.xml", Config.DataType.BaseData);

        }


    }
}
