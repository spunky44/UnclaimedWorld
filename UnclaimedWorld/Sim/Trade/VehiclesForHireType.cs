using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWGame.SimSide.Trade
{
    public class VehiclesForHireType
    {
        public int MaxAmount; 

        public int StartAmount; 

        public float Price;

        public float PricePerKilometer;


        public NormalDistribution SpecificPrice;

        public NormalDistribution SpecificStartAmount;

        public NormalDistribution SpecificMaxAmount;

    }
}
