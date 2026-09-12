using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.Vehicles
{
    public class CargoSlot
    {
        PassengerOrCargoSlot parent;

        private int sequence;

        public float BulkCarried;

        public Entity TargetedByHauler; 

        public bool IsFull()
        {
           // if (parent.Parent.ItemStorage.TotalStored)

            return BulkCarried >= parent.PassengerOrCargoSlotType.CargoSlotType.Capacity;
        }


        public float GetRemainingRoom()
        {            
            return Common.ClampBottom(parent.PassengerOrCargoSlotType.CargoSlotType.Capacity - BulkCarried, 0f);
        }

        public CargoSlot(PassengerOrCargoSlot parent)
        {
            this.parent = parent;
        }
    }
}
