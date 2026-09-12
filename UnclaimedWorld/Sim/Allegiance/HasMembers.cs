using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Allegiances
{
    /// <summary>
    /// Static class that contains the shared methods from the ILookup implementation in IHasMembers.
    /// </summary>
    public class HasMembers
    {
        static CanIterateEntitiesID IDCounter = CanIterateEntitiesID.First;

        public static CanIterateEntitiesID GetUniqueID()
        {
            IDCounter++;
            if (IDCounter >= CanIterateEntitiesID.Max)
            {
                throw new Exception("Astounding, HasMembersID just exceeded 64 bits. Something seriously wrong has happened.");
            }

            return IDCounter;
        }


        public static void DoSnapshot(Snapshotter sn)
        {
            IDCounter = (CanIterateEntitiesID)sn.DoEnum(IDCounter);
        }


        public static void ResetIDCounterNoInvoke() 
        {
            IDCounter = CanIterateEntitiesID.First;
        }

    }
}
