using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Trees
{
    /// <summary>
    /// Static class that contains the shared methods from the ILookup implementation
    /// </summary>
    public class HasCrops
    {
        static HasCropsID IDCounter = HasCropsID.First;

        public static HasCropsID GetUniqueID()
        {
            IDCounter++;
            if (IDCounter >= HasCropsID.Max)
            {
                throw new Exception("Astounding, HasCropsID just exceeded 64 bits. Something seriously wrong has happened.");
            }

            return IDCounter;
        }


        public static void DoSnapshot(Snapshotter sn)
        {
            IDCounter = (HasCropsID)sn.DoEnum(IDCounter);
        }


        public static void ResetIDCounterNoInvoke()
        {
            IDCounter = HasCropsID.First;
        }
    }
}
