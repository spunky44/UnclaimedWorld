using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.XmlCollections;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Entities.Templates;
using UWGame.SimSide.Overland.Templates;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.Trade;

namespace UWGame.SimSide.AllGameData
{
    public class TradeProfileLoader
    {
        public static List<TradeProfile> Init()
        {
            List<TradeProfile> list = new List<TradeProfile>();

            #region "farmingTradeProfile"
            list.Add(new TradeProfile()
            {
                KeyName = "farmingTradeProfile",
                Comments = "has 1 random cash crop and 1 products export",

                HighExport = new[] { "foodCropsTrade" }, //
                RandomHighExport = new RandomTrade() { NoOfGroups = 1, Options = new[] { "cottonTrade", "firewoodTrade", "peatTrade" } }, // 1 random cash crop
                RandomMediumExport = new RandomTrade() { NoOfGroups = 1, Options = new[] {  "thunderChickenTrade", "bakedFoodTrade", "comfortTrade", "animalsTrade" } },

                MediumImport = new[] { "farmToolsTrade" },
                LowImport = new[] { "toolsTrade", "textileTrade" },
                RandomLowImport = new RandomTrade() { NoOfGroups = 1, Options = new[] { "hidesTrade", "turnipTrade" } }, 
            });
            #endregion                       

            #region "miningTradeProfile"
            list.Add(new TradeProfile()
            {
                KeyName = "miningTradeProfile", 
               
                HighExport = new[] {"mineralsTrade"},
                MediumExport = new[] { "metalsTrade" },
                LowExport = new[] { "toolsTrade" },
                MediumImport = new[] { "foodTrade", "comfortTrade", "textileTrade" },   
                //RandomHighExport = new RandomTrade() { NoOfGroups = 1, Options = new[] { "toolsExport", "farmToolsExport" } }, 
                       
            });
            #endregion

            #region "fishingTradeProfile"
            list.Add(new TradeProfile()
            {
                KeyName = "fishingTradeProfile",

                HighExport = new[] { "fishTrade" },
                MediumImport = new[] { "fishingEquipmentTrade", "comfortTrade" },
                LowImport = new[] { "toolsTrade" }
            });
            #endregion

            #region "advancedFarmingTradeProfile" 
            list.Add(new TradeProfile()
            {
                KeyName = "advancedFarmingTradeProfile",
                Comments = "exports food, has 1 random cash crop and 1 products export. Also trades advanced goods, since these are supposed to be more common",

                HighExport = new[] { "foodCropsTrade" }, //
                MediumExport = new[] { "mediumWeaponsTrade" },

                RandomHighExport = new RandomTrade() { NoOfGroups = 1, Options = new[] { "cottonTrade", "firewoodTrade", "peatTrade" } }, // 1 random cash crop
                RandomMediumExport = new RandomTrade() { NoOfGroups = 1, Options = new[] { "thunderChickenTrade", "bakedFoodTrade", "comfortTrade", "animalsTrade" } },

                MediumImport = new[] { "farmToolsTrade" },
                LowImport = new[] { "toolsTrade", "textileTrade" },
                RandomLowImport = new RandomTrade() { NoOfGroups = 1, Options = new[] { "hidesTrade", "turnipTrade" } },

                LowTrade = new[] { "advancedToolsTrade", "advancedHandWeaponsTrade", "advancedFoodTrade", "advancedEquipmentTrade", "advancedMaterialsTrade", "occasionalEquipmentTradeItems", "occasionallyOfferedSeeds" },

                TradeGroupPriority = new SerializableDictionary<string, int>()
                {
                    {"occasionalEquipmentTradeItems", 10 },
                    {"foodCropsTrade", 10 }, // overrides occasional crops
                    {"cottonTrade", 10 } // overrides occasional crops
                }
            });
            #endregion

            #region "advancedFishingTradeProfile" 
            list.Add(new TradeProfile()
            {
                KeyName = "advancedFishingTradeProfile",
               
                HighExport = new[] { "fishTrade" },
                MediumExport = new[] { "mediumWeaponsTrade" },

                MediumImport = new[] { "fishingEquipmentTrade", "comfortTrade" },
                LowImport = new[] { "toolsTrade" },

                MediumTrade = new[] { "occasionallyOfferedSeeds" },
                LowTrade = new[] { "advancedToolsTrade", "advancedHandWeaponsTrade", "advancedFoodTrade", "advancedEquipmentTrade", "advancedMaterialsTrade", "occasionalEquipmentTradeItems" },

                TradeGroupPriority = new SerializableDictionary<string, int>()
                {
                    {"occasionalEquipmentTradeItems", 10}
                }
            });
            #endregion

            #region "advancedMiningTradeProfile" 
            list.Add(new TradeProfile()
            {
                KeyName = "advancedMiningTradeProfile",
              
                HighExport = new[] { "mineralsTrade" },
                MediumExport = new[] { "metalsTrade", "mediumWeaponsTrade" },
                LowExport = new[] { "toolsTrade" },
                MediumImport = new[] { "foodTrade", "comfortTrade", "textileTrade" },

                MediumTrade = new[] { "occasionallyOfferedSeeds" },
                LowTrade = new[] { "advancedToolsTrade", "advancedHandWeaponsTrade", "advancedFoodTrade", "advancedEquipmentTrade", "advancedMaterialsTrade", "occasionalEquipmentTradeItems" },

                TradeGroupPriority = new SerializableDictionary<string,int>()
                {
                    {"occasionalEquipmentTradeItems", 10}
                }
            });
            #endregion



            #region "industryTradeProfile"
            list.Add(new TradeProfile()
            {
                KeyName = "industryTradeProfile",

                HighExport = new[] { "mineralsTrade",  },
                MediumExport = new[] { "metalsTrade", "toolsTrade", "weaponsTrade",  "industrialComponentsTrade", "textileTrade" },

                MediumImport = new[] { "foodTrade", "comfortTrade", "plasticsTrade" },//
                RandomLowImport = new RandomTrade() { NoOfGroups = 1, Options = new[] { "hidesTrade", "fuelTrade" } }, // 1 random low import. low because on Headway I dont want the player to make too much money from just making peat.
                
            });
            #endregion


            #region "bigTradingProfile"
            list.Add(new TradeProfile()
            {
                KeyName = "bigTradingProfile",

                MediumTrade = new[] { "mineralsTrade", "metalsTrade", "foodTrade", "toolsTrade", "weaponsTrade", "hidesTrade", "cropsTrade", "fuelTrade", "textileTrade", 
                    "industrialComponentsTrade", "occasionalAdvancedTradeItems", "occasionalAdvancedAmmoTradeItems", "fishingEquipmentTrade", "trapsTrade", "electronicsTrade", "containersTrade", "plasticsTrade", "animalsTrade" },
              
                       
            });
            #endregion

            #region "advancedTradingProfile"  
            list.Add(new TradeProfile()
            {
                KeyName = "advancedTradingProfile",
                       
                HighExport = new[] { "advancedHandWeaponsTrade", "advancedFoodTrade", "advancedToolsTrade", "advancedEquipmentTrade", "advancedMaterialsTrade" }

            });
            #endregion
            

            return list;

        }

    }
}
