using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.XmlCollections;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Entities.Templates;
using UWGame.SimSide.Overland.Templates;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Trade;
using UWGame.SimSide.InGameEvents.Expressions;
using Microsoft.Xna.Framework;

namespace UWGame.SimSide.AllGameData
{
    public class ExpeditionDataLoader
    {
        public static List<ExpeditionData> Init()
        {
            List<ExpeditionData> list = new List<ExpeditionData>();
            
            #region "farming site"
            list.Add(new ExpeditionData()
            {
                KeyName = "farmingProfileExpedition",
                Name = "The Wharf",
                TradeProfile = "farmingTradeProfile",
                StructuresProfile = "mediumPierProfile", //
                VehiclesProfile = "bargeProfile",
                PricesProfile = "descentEraPrices" 
            });
            #endregion

            
            #region "mining site"
            list.Add(new ExpeditionData()
            {
                KeyName = "miningProfileExpedition",
                Name = "The Wharf",
                TradeProfile = "miningTradeProfile",
                StructuresProfile = "mediumPierProfile", // 
                VehiclesProfile = "bargeProfile",
                PricesProfile = "descentEraPrices" 
            });
            #endregion

            #region "fishing site"
            list.Add(new ExpeditionData()
            {
                KeyName = "fishingProfileExpedition",
                Name = "The Wharf",
                TradeProfile = "fishingTradeProfile",
                StructuresProfile = "mediumPierProfile", //
                VehiclesProfile = "bargeProfile",
                PricesProfile = "descentEraPrices"
            });
            #endregion


            #region "advanced farming site"
            list.Add(new ExpeditionData()
            {
                KeyName = "advancedFarmingProfileExpedition",
                Name = "The Wharf",
                TradeProfile = "advancedFarmingTradeProfile",
                StructuresProfile = "mediumPierSatelliteProfile", //
                VehiclesProfile = "advancedBargeProfile",
                PricesProfile = "planetFallEraPrices"
            });
            #endregion


            #region "advanced mining site"
            list.Add(new ExpeditionData()
            {
                KeyName = "advancedMiningProfileExpedition",
                Name = "The Wharf",
                TradeProfile = "advancedMiningTradeProfile",
                StructuresProfile = "mediumPierSatelliteProfile", // 
                VehiclesProfile = "advancedBargeProfile",
                PricesProfile = "planetFallEraPrices",
               /* AvailableForTrade = new SerializableDictionary<string, TradeAmountType>()
                {
                    // sell ammo at all sites


                }*/
            });
            #endregion

            #region "advanced fishing site"
            list.Add(new ExpeditionData()
            {
                KeyName = "advancedFishingProfileExpedition",
                Name = "The Wharf",
                TradeProfile = "advancedFishingTradeProfile",
                StructuresProfile = "mediumPierSatelliteProfile", //
                VehiclesProfile = "advancedBargeProfile",
                PricesProfile = "planetFallEraPrices"
            });
            #endregion


            #region "big trading hub"
            list.Add(new ExpeditionData()
            {
                KeyName = "destinyRiverDeltaDescentExpedition", //"bigTradingHubProfileExpedition",
                Name = "The Wharf",
                SizeFactor = 2.5f,
                TradeProfile = "bigTradingProfile",
                StructuresProfile = "largePierProfile",
                VehiclesProfile = "bargeProfile",
                PricesProfile = "descentEraPrices"
            });
            #endregion

          /*  #region Dukes Landing Planet Fall
            list.Add(new ExpeditionData()
            {
                KeyName = "dukesLandingPlanetFallExpedition", //"bigTradingHubProfileExpedition",
                Name = "Duke's Landing", // the wharf
                SizeFactor = 2.5f,
                AvailableForTrade = new SerializableDictionary<string,TradeAmountType>()
                {
                      { "item:scandium", new TradeAmountType(){ LinearIncreasePerDay = 0f, LinearConsumptionPerDay = 0f, MaxAmountToBuy = 40  }},
                      { "item:terbium", new TradeAmountType(){ LinearIncreasePerDay = 0f, LinearConsumptionPerDay = 0f, MaxAmountToBuy = 20  }},
                      { "item:sentry", new TradeAmountType(){ LinearIncreasePerDay = 6f, MaxAmountForSale = 12  }},
                      { "item:sentryGunAmmo", new TradeAmountType(){ LinearIncreasePerDay = 8f, LinearConsumptionPerDay = 8f, MaxAmountForSale = 20, MaxAmountToBuy = 20  }}
                },
                TradeProfile = "advancedTradingProfile",
                StructuresProfile = "largePierProfile",
                VehiclesProfile = "airliftProfile",
                PricesProfile = "planetFallEraPrices"
            });
            #endregion*/

            return list;

        }

    }
}
