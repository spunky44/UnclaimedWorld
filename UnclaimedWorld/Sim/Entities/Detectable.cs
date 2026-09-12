using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities
{
    /// <summary>
    /// Static class that contains the shared methods from the ILookup implementation in IDetectable.
    /// </summary>
    public class Detectable
    {
        static DetectableID IDCounter = DetectableID.First;

        public static DetectableID GetUniqueID()
        {
            IDCounter++;
            if (IDCounter >= DetectableID.Max)
            {
                throw new Exception("Astounding, DetectableID just exceeded 64 bits. Something seriously wrong has happened.");
            }

            return IDCounter;
        }


        public static void DoSnapshot(Snapshotter sn)
        {
            IDCounter = sn.DoEnum(IDCounter);
        }


        public static void ResetIDCounterNoInvoke() 
        {
            IDCounter = DetectableID.First;
        }

    }
}
