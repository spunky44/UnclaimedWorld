using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Trade
{
    /// <summary>
    /// 
    /// </summary>
    public class VehiclesForHireAmount: ISnapshot
    {
        public int StartAmount;

        public float IncreasePerDay;

        /// <summary>
        /// keep this. for long play, we may want growth in number of vehicles for hire...
        /// </summary>
        public int MaxAmount;

      
        public decimal Price;

        public decimal PricePerKilometer;


        public VehiclesForHireAmount()
        {

        }


        public VehiclesForHireAmount(VehiclesForHireType amountType, float sizeFactor)
        {
            if (amountType.SpecificStartAmount != null)
            {
                StartAmount = amountType.SpecificStartAmount.GetRandomIntegerValue(The.Sim.GameplayRandomGenerator);
            }
            else
            {
                StartAmount = (int)(amountType.StartAmount * sizeFactor);

                if (amountType.StartAmount == 1)
                {
                    StartAmount = Common.ClampBottom(StartAmount, 1); // at least one...
                }                
            }

            if (amountType.SpecificStartAmount != null)
            {
                MaxAmount = amountType.SpecificMaxAmount.GetRandomIntegerValue(The.Sim.GameplayRandomGenerator);
            }
            else
            {
                MaxAmount = (int)(amountType.MaxAmount * sizeFactor);
            }

            if (amountType.SpecificPrice != null)
            {
                Price = (decimal)amountType.SpecificPrice.GetRandomValue(The.Sim.GameplayRandomGenerator);
            }
            else
            {
                Price = (decimal)amountType.Price;
            }

            PricePerKilometer = (decimal)amountType.PricePerKilometer;
        }


        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            MaxAmount = sn.DoInt32(MaxAmount);
            StartAmount = sn.DoInt32(StartAmount);

            IncreasePerDay = sn.DoFloat(IncreasePerDay);

            Price = sn.DoDecimal(Price);
            PricePerKilometer = sn.DoDecimal(PricePerKilometer);

            return this;
        }



        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);


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
