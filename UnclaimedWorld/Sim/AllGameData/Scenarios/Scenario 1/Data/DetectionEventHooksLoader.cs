using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_1.Data
{
    public class DetectionEventHooksLoader
    {
        public static List<DetectEntityTypeHook> InitDetectEntityTypeHooks()
        {
            List<DetectEntityTypeHook> list = new List<DetectEntityTypeHook>();

            list.Add(new DetectEntityTypeHook()
            {
                KeyName = "detectThunderChickenCarcass",
                TypeKey = "entity:human",
                DetectedEntityKey = "item:thunderChickenCarcass",
                ActionSetsKey = "detectThunderChickenCarcass"
            });
/* mp 2015 not used currently
            list.Add(new DetectEntityTypeHook()
            {
                KeyName = "detectBushdragonCarcass",
                TypeKey = "entity:human",
                DetectedEntityKey = "item:bushDragonCarcass",
                ActionSetsKey = "detectBushdragonCarcass"
            });
*/
            list.Add(new DetectEntityTypeHook()
            {
                KeyName = "detectQuaditeCarcass",
                TypeKey = "entity:human",
                DetectedEntityKey = "item:quaditeCarcass",
                ActionSetsKey = "detectQuaditeCarcass"
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

            list.Add(new DetectEntityTypeHook()
            {
                KeyName = "detectTwinkler",
                TypeKey = "entity:human",
                DetectedEntityKey = "entity:twinkler",
                ActionSetsKey = "detectTwinkler"
            });

            list.Add(new DetectEntityTypeHook()
            {
                KeyName = "detectBushDragonRemark",
                TypeKey = "entity:human",
                DetectedEntityKey = "entity:bushDragon",
                ActionSetsKey = "detectBushDragonRemark"
            });

            list.Add(new DetectEntityTypeHook()
            {
                KeyName = "detectBushDragonSetDelay",
                TypeKey = "entity:human",
                DetectedEntityKey = "entity:bushDragon",
                ActionSetsKey = "detectBushDragonSetDelay"
            });

            list.Add(new DetectEntityTypeHook()
            {
                KeyName = "detectQuaditeNestRemark",
                TypeKey = "entity:human",
                DetectedEntityKey = "terrain:quaditeNest",
                ActionSetsKey = "detectQuaditeNestRemark"
            });

            list.Add(new DetectEntityTypeHook()
            {
                KeyName = "detectQuaditeNestSetProperty",
                TypeKey = "entity:human",
                DetectedEntityKey = "terrain:quaditeNest",
                ActionSetsKey = "detectQuaditeNestSetProperty"
            });


            list.Add(new DetectEntityTypeHook()
            {
                KeyName = "detectCratesRemark",
                TypeKey = "entity:human",
                DetectedEntityKey = "terrain:crates",
                ActionSetsKey = "detectCratesRemark"
            });

            list.Add(new DetectEntityTypeHook()
            {
                KeyName = "detectCrystalBerries",
                TypeKey = "entity:human",
                DetectedEntityKey = "item:crystalBerries",
                ActionSetsKey = "talkCrystalBerries"
            });
/*            list.Add(new DetectEntityTypeHook()
            {
                KeyName = "detectCratesSetProperty",
                TypeKey = "entity:human",
                DetectedEntityKey = "terrain:crates",
                ActionSetsKey = "detectCratesSetProperty"
            });
*/

            list.Add(new DetectEntityTypeHook()
            {
                KeyName = "detectSkimmerTailRemark",
                TypeKey = "entity:human",                 
                DetectedEntityKey = "structure:skimmerTail",                 
                ActionSetsKey = "detectSkimmerTailRemark"
            });
            #region Rat nest detection
            /*
            list.Add(new DetectEntityTypeHook()
            {
                KeyName = "detectRatNest",
                TypeKey = "entity:human",
                DetectedEntityKey = "terrain:ratNest",
                ActionSetsKey = "detectRatNestRemark"
            });*/
            #endregion

            #region fish trap spot detection
            list.Add(new DetectEntityTypeHook()
            {
                KeyName = "detectFishTrapCoastRemark",
                TypeKey = "entity:human",
                DetectedEntityKey = "terrain:fishTrapSpotCoast",
                ActionSetsKey = "detectFishTrapCoastRemark"
            });
            list.Add(new DetectEntityTypeHook()
            {
                KeyName = "detectFishTrapShoreRemark",
                TypeKey = "entity:human",
                DetectedEntityKey = "terrain:fishTrapSpotShore",
                ActionSetsKey = "detectFishTrapShoreRemark"
            });
            #endregion

            #region Farmplot detection //MP nov 2015 not used. was buggy
 /*           list.Add(new DetectEntityTypeHook()
            {
                KeyName = "detectSmallFarmPlot",
                TypeKey = "entity:human",
                DetectedEntityKey = "terrain:smallPlotSpot",
                ActionSetsKey = "detectSmallFarmPlot"
            });
            list.Add(new DetectEntityTypeHook()
            {
                KeyName = "detectLargFarmPlot",
                TypeKey = "entity:human",
                DetectedEntityKey = "terrain:largePlotSpot",
                ActionSetsKey = "detectLargFarmPlot"
            });*/
            #endregion

            return list;

            //BaseDataLoader.SerializeAndDeserializeTypeList(list, GameData.Instance.AllDetectedEntityEventHooks, "", "detectedEntityHooks.xml", Config.DataType.BaseData);

        }

        public static List<DetectResourceTypeHook> InitDetectResourceTypeHooks()
        {
            List<DetectResourceTypeHook> list = new List<DetectResourceTypeHook>();


            list.Add(new DetectResourceTypeHook()
            {
                KeyName = "detectShadeleafCanes",
                TypeKey = "entity:human",
                DetectedResourceKey = "crop:shadeleafCanes",
                ActionSetsKey = "detectShadeleafCanes"

            });

            list.Add(new DetectResourceTypeHook()
            {
                KeyName = "detectShadeleafBowStave",
                TypeKey = "entity:human",
                DetectedResourceKey = "crop:shadeleafBowStave",
                ActionSetsKey = "detectShadeleafBowStave"

            });

            list.Add(new DetectResourceTypeHook()
            {
                KeyName = "detectShadeleafResin",
                TypeKey = "entity:human",
                DetectedResourceKey = "crop:shadeleafResin",
                ActionSetsKey = "detectShadeleafResin"

            });

            list.Add(new DetectResourceTypeHook()
            {
                KeyName = "detectSulfur",
                TypeKey = "entity:human",
                DetectedResourceKey = "sulfurDeposit",
                ActionSetsKey = "detectSulfur"

            });
/*
            list.Add(new DetectResourceTypeHook()
            {
                KeyName = "detectMarshcotSap",
                TypeKey = "entity:human",
                DetectedResourceKey = "crop:marshcotSap",
                ActionSetsKey = "detectMarshcotSap"

            });
*/

            return list;

            //  BaseDataLoader.SerializeAndDeserializeTypeList(list, GameData.Instance.AllDetectedResourceEventHooks, "", "detectedResourceHooks.xml", Config.DataType.BaseData);

        }


    }
}
