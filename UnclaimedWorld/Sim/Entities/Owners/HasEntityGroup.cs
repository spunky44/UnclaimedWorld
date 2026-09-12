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
    public class HasEntityGroup
    {
        static HasEntityGroupID IDCounter = HasEntityGroupID.First;

        public static HasEntityGroupID GetUniqueID()
        {
            IDCounter++;
            if (IDCounter >= HasEntityGroupID.Max)
            {
                throw new Exception("Astounding, HasEntityGroupID just exceeded 64 bits. Something seriously wrong has happened.");
            }

            return IDCounter;
        }


        public static void DoSnapshot(Snapshotter sn)
        {
            IDCounter = (HasEntityGroupID)sn.DoEnum(IDCounter);
        }


        public static void ResetIDCounterNoInvoke() 
        {
            IDCounter = HasEntityGroupID.First;
        }


    }
}
