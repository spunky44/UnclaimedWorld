using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.Systems;

namespace UWGame.SimSide.Trade
{
    /// <summary>
    /// NEW  
    /// Traders should buy and sell the same items, others should not
    /// But the price has to be different
    /// Items that are bought by a trader can be sold right after
    /// 
    /// the amounts/price should represent the scarcity of the item. amounts and price should be linearly dependent
    /// The amount numbers (buy and sell) will be modified by both the TradeProfile, and by the Size factor of the site
    /// 
    /// see also TradeAmount
    /// </summary>
    public class TradeAmountType
    {
        /// <summary>
        /// EITHER
        /// </summary>
        public string EntityType;
        /// <summary>
        /// OR:
        /// </summary>
        public string EntityDataKey;



        /// <summary>
        /// the maximum that will be available for sale. The items will be divided between the terminals.
        /// </summary>
        public int MaxAmountForSale;

        /// <summary>
        /// if zero, the good will never be bought here
        /// </summary>
        public int MaxAmountToBuy;

        /// <summary>
        /// The start amount to buy/demand in trade
        /// if not filled, a random value will be generated below MaxAmountToBuy
        /// </summary>
        public int? AmountToBuy;


        public float LinearIncreasePerDay;
        public float LinearConsumptionPerDay;

       /// <summary>
       /// optional. Global prices can be used instead
       /// </summary>
        public float? SellPrice;

        /// <summary>
        /// ditto
        /// </summary>
        public float? BuyPrice;


        /// <summary>
        /// for non-linear production or consumption (noise function)
        /// </summary>
        public string OfferDemandProfile;

        

        /// <summary>
        /// +/-
        /// the maximum that will be available for trade (buy or sell)
        /// </summary>
        public NormalDistribution SpecificMaxAmount;

        /// <summary>
        /// +/-
        /// </summary>
        public NormalDistribution SpecificIncreasePerDay;

        public NormalDistribution SpecificConsumptionPerDay;


        public NormalDistribution SpecificSellPrice;

        public NormalDistribution SpecificBuyPrice;

        /// <summary>
        /// if not filled, a random amount below max will be used
        /// </summary>
        public NormalDistribution StartAmount;
        
        
        public string GetEntityTypeKey()
        {

            if (EntityType != null && EntityType.Contains("robot"))
            {

            }

            if (EntityDataKey != null)
            {
                EntityData entityDataToUse = GameData.Instance.AllEntityData[EntityDataKey];
                return entityDataToUse.EntityKey;
            }
            else return EntityType;
        }

    }
}
