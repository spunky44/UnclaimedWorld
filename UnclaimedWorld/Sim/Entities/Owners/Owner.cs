using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities.Owners
{
    /// <summary>
    /// Static class that contains the shared methods from the ILookup implementation in IOwner.
    /// </summary>
    public class Owner
    {
        static OwnerID IDCounter = OwnerID.First;

        public static OwnerID GetUniqueID()
        {
            IDCounter++;
            if (IDCounter >= OwnerID.Max)
            {
                throw new Exception("Astounding, OwnerID just exceeded 64 bits. Something seriously wrong has happened.");
            }

            return IDCounter;
        }


        public static void DoSnapshot(Snapshotter sn)
        {
            IDCounter = (OwnerID)sn.DoEnum(IDCounter);
        }


        public static void ResetIDCounterNoInvoke() 
        {
            IDCounter = OwnerID.First;
        }


    }
}
