using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Trade
{
    /// <summary>
    /// see also TradeAmountType
    /// </summary>
    public class TradeAmount: ISnapshot
    {
        /// <summary>
        /// we might keep a reference to TradeAmountType instead...
        /// </summary>
        public EntityData EntityData;


        public int? StartAmount;

        /// <summary>
        /// both can be defined for the Trade outpost
        ///      
        /// </summary>
        public int? MaxAmountForSale;

        public int? MaxAmountToBuy;


        


       // public float TimeInDaysElapsedSinceItemSpawn;
        public float Progress;


        /// <summary>
        /// +/-
        /// </summary>
        public float IncreasePerDay;

        public float ConsumptionPerDay;


        /// <summary>
        /// instead of the above
        /// </summary>
        public OfferDemandProfile OfferDemandProfile;

        public float OfferDemandProfileScaleFactor;

     
        public SimplexNoise OfferedForTradeNoise;



        /// <summary>   
        /// if we ever want moving prices, one way to do it could be to change the price when min or max amount is hit, within a range of 30% or so..
        /// </summary>
        public float? SellPrice;


        public float? BuyPrice;


        public float TimeInDaysElapsedSinceItemConsumed;

        /// <summary>
        /// this counter goes up regularly, to a maximum of MaxAmountToBuy. 
        /// Is reduced when the player sells something here, to a minimum of 0
        /// </summary>
        public int AmountToBuy;
        
        /// <summary>
        /// instead of spawning directly?? saves memory perhaps
        /// </summary>
       // public int AmountForSale;

      
      

        public TradeAmount()
        {

        }

        public TradeAmount(string entityType, TradeAmountType amountType, float scaleAmounts, OfferDemandProfile offerDemandProfile, PricesProfile pricesProfile, bool allowDemand, bool allowProduction)
        {
            if (amountType.EntityDataKey != null)
            {
                EntityData = GameData.Instance.AllEntityData[amountType.EntityDataKey];
            }


            if (allowProduction)
            {
                IncreasePerDay = amountType.LinearIncreasePerDay * scaleAmounts; 
                MaxAmountForSale = (int)(amountType.MaxAmountForSale * scaleAmounts); 
            }

            if (allowDemand)
            {
                ConsumptionPerDay = amountType.LinearConsumptionPerDay * scaleAmounts;            
                MaxAmountToBuy = (int)(amountType.MaxAmountToBuy * scaleAmounts); 
            }

            if (offerDemandProfile != null)
            {
                OfferDemandProfile = offerDemandProfile;
                OfferDemandProfileScaleFactor = scaleAmounts;

                if (offerDemandProfile.NonlinearAmountForSale != null)
                {
                    this.OfferedForTradeNoise = new SimplexNoise(); // uses 512 bytes of memory... too much for this feature?
                }
            }


            // allow global prices to be overridden:
            SellPrice = amountType.SellPrice;
            BuyPrice = amountType.BuyPrice;

            if (amountType.SpecificSellPrice != null)
            {
                SellPrice = (float)amountType.SpecificSellPrice.GetRandomValue(The.Sim.GameplayRandomGenerator);
            }

            if (amountType.SpecificBuyPrice != null)
            {
                BuyPrice = (float)amountType.SpecificBuyPrice.GetRandomValue(The.Sim.GameplayRandomGenerator);
            }

            if (pricesProfile != null)
            {
                float profilePrice;
                if (MaxAmountForSale > 0 && SellPrice == null)
                {
                    if (pricesProfile.Prices.TryGetValue(entityType, out profilePrice))
                    {
                        SellPrice = GameData.Instance.Constants.SellPriceModifier * profilePrice;
                    }
                }

                if (MaxAmountToBuy > 0 && BuyPrice == null)
                {
                    if (pricesProfile.Prices.TryGetValue(entityType, out profilePrice))
                    {
                        BuyPrice = profilePrice;
                    }
                    else
                    {
                        throw new Exception("Price not found: " + entityType);
                    }
                }
            }

            if (amountType.SpecificIncreasePerDay != null)
            {
                IncreasePerDay = amountType.SpecificIncreasePerDay.GetRandomIntegerValue(The.Sim.GameplayRandomGenerator);
            }

            if (amountType.SpecificMaxAmount != null)
            {
                MaxAmountForSale = amountType.SpecificMaxAmount.GetRandomIntegerValue(The.Sim.GameplayRandomGenerator);
            }        
  
            if (amountType.StartAmount != null)
            {
                StartAmount = amountType.StartAmount.GetRandomIntegerValue(The.Sim.GameplayRandomGenerator);
            }

            if (amountType.AmountToBuy.HasValue)
            {
                AmountToBuy = amountType.AmountToBuy.Value;
            }
            else if (MaxAmountToBuy > 0)
            {
                AmountToBuy = Common.RandomBetween(The.Sim.GameplayRandomGenerator, 0, MaxAmountToBuy.Value + 1);
            }
            else
            {
                AmountToBuy = 0;
            }
        }


        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            StartAmount = sn.DoInt32Nullable(StartAmount);
            MaxAmountForSale = sn.DoInt32Nullable(MaxAmountForSale);
            MaxAmountToBuy = sn.DoInt32Nullable(MaxAmountToBuy);
           
            IncreasePerDay = sn.DoFloat(IncreasePerDay);
            ConsumptionPerDay = sn.DoFloat(ConsumptionPerDay);

            SellPrice = sn.DoFloatNullable(SellPrice);
            BuyPrice = sn.DoFloatNullable(BuyPrice);

            Progress = sn.DoFloat(Progress);
            TimeInDaysElapsedSinceItemConsumed = sn.DoFloat(TimeInDaysElapsedSinceItemConsumed);

            AmountToBuy = sn.DoInt32(AmountToBuy);

            OfferDemandProfile = sn.DoGameData(OfferDemandProfile);
            OfferDemandProfileScaleFactor = sn.DoFloat(OfferDemandProfileScaleFactor);

            OfferedForTradeNoise = (SimplexNoise)sn.DoISnapshot(OfferedForTradeNoise);

            EntityData = sn.DoGameData(EntityData); // #LOAD35


            return this;
        }



        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            if (OfferedForTradeNoise != null)
            {
                OfferedForTradeNoise.LoadPostProcess(sn);
            }
        }


        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public Snapshotter.Version DoVersion(Snapshotter sn)
        {
            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }

        public bool IsSnapshotted { get; set; }

        #endregion
    }
}
