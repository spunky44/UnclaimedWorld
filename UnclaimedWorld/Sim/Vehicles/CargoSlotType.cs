using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.SimSide.Vehicles
{
    public class CargoSlotType
    {
        public float Capacity;

        public CargoSlotType() 
        {
            Capacity = 2f;
        }

        public CargoSlotType(float capacity)
        {
            Capacity = capacity;
        }
    }
}
