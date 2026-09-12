using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.AllGameData.EventHooks
{
    public class DetectionEventHooksLoader
    {
        public static List<DetectEntityTypeHook> InitDetectEntityTypeHooks()
        {
            List<DetectEntityTypeHook> list = new List<DetectEntityTypeHook>();

          /*  list.Add(new DetectEntityTypeHook() //hack!! 
            {
                KeyName = "detectBinalratcarcass",
                TypeKey = "entity:human",
                DetectedEntityKey = "item:binalRatCarcass",
                ActionSetsKey = "claimCarcassNearTrap"
            });
            list.Add(new DetectEntityTypeHook() //hack!! 
            {
                KeyName = "detectThunderChickenCarcass",
                TypeKey = "entity:human",
                DetectedEntityKey = "item:thunderChickenCarcass",
                ActionSetsKey = "claimCarcassNearTrap"
            });
            list.Add(new DetectEntityTypeHook() //hack!! 
            {
                KeyName = "detectBushDragonCarcass",
                TypeKey = "entity:human",
                DetectedEntityKey = "item:bushDragonCarcass",
                ActionSetsKey = "claimCarcassNearTrap"
            });
            list.Add(new DetectEntityTypeHook() //hack!! 
            {
                KeyName = "detectTurnipCarcass",
                TypeKey = "entity:human",
                DetectedEntityKey = "item:turnipCarcass",
                ActionSetsKey = "claimCarcassNearTrap"
            });
            list.Add(new DetectEntityTypeHook() //hack!! 
            {
                KeyName = "detectPatricianCarcass",
                TypeKey = "entity:human",
                DetectedEntityKey = "item:patricianCarcass",
                ActionSetsKey = "claimCarcassNearTrap"
            });
            list.Add(new DetectEntityTypeHook() //hack!! 
            {
                KeyName = "detectDemontreecarcass",
                TypeKey = "entity:human",
                DetectedEntityKey = "item:demontreeCarcass",
                ActionSetsKey = "claimCarcassNearTrap"
            });
            list.Add(new DetectEntityTypeHook() //hack!!
            {
                KeyName = "detectSwampDemonTreeCarcass",
                TypeKey = "entity:human",
                DetectedEntityKey = "item:swampDemonTreeCarcass",
                ActionSetsKey = "claimCarcassNearTrap"
            });
            list.Add(new DetectEntityTypeHook() //hack!! 
            {
                KeyName = "detectWormCarcass",
                TypeKey = "entity:human",
                DetectedEntityKey = "item:wormCarcass",
                ActionSetsKey = "claimCarcassNearTrap"
            });*/


            return list;
        }
       
    }
}
