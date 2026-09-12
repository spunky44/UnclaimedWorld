using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Resources
{
    /// <summary>
    /// Static class that contains the shared methods from the ILookup implementation in IResourceItem.
    /// The counter has to be shared to provide unique ResourceItemIDs across the classes. 
    /// But the code cannot be placed in the generic Lookup class, since it does not know about the ResourceItemID type...
    /// </summary>
    public static class ResourceItem
    {
        static ResourceItemID IDCounter = ResourceItemID.First;

        public static ResourceItemID GetUniqueID()
        {
            IDCounter++;
            if (IDCounter >= ResourceItemID.Max)
            {
                throw new Exception("Astounding, ResourceItemID just exceeded 64 bits. Something seriously wrong has happened.");
            }

            return IDCounter;
        }
        

        public static void DoSnapshot(Snapshotter sn)
        {
            IDCounter = (ResourceItemID)sn.DoEnum(IDCounter);
        }


        public static void ResetIDCounterNoInvoke() 
        {
            IDCounter = ResourceItemID.First;
        }

    }
}
