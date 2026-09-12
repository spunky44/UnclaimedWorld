using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Items
{
    /// <summary>
    /// Static class that contains the shared methods from the ILookup implementation in IComposite.
    /// </summary>
    public class Composite
    {
        static CompositeID IDCounter = CompositeID.First;

        public static CompositeID GetUniqueID()
        {
            IDCounter++;
            if (IDCounter >= CompositeID.Max)
            {
                throw new Exception("Astounding, CompositeID just exceeded 64 bits. Something seriously wrong has happened.");
            }

            return IDCounter;
        }


        public static void DoSnapshot(Snapshotter sn)
        {
            IDCounter = sn.DoEnum(IDCounter);
        }


        public static void ResetIDCounterNoInvoke() 
        {
            IDCounter = CompositeID.First;
        }

    }
}
