using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Systems.TimeSlicing
{   

    /// <summary>
    /// Static class that contains the shared methods from the ILookup implementation in ICyclable.
    /// The counter has to be shared to provide unique CyclableIDs across the classes. 
    /// But the code cannot be placed in the generic Lookup class, since it does not know about the CyclableID type...
    /// </summary>
    public static class Cyclable
    {        
        static CyclableID IDCounter = CyclableID.First;

        public static CyclableID GetUniqueID()
        {
            IDCounter++;
            if (IDCounter >= CyclableID.Max)
            {
                throw new Exception("Astounding, CyclableID just exceeded 64 bits. Something seriously wrong has happened.");
            }

            return IDCounter;
        }

        /// <summary>
        /// the ID must have been set BEFORE calling this. ID has a private setter, that's why.
        /// </summary>
        /// <param name="cyclable"></param>
      /*  public static void AddToLookup(ICyclable cyclable)
        {
           // ID = GetUniqueID();
            if (cyclable.ID != CyclableID.Invalid)
                LookUp<ICyclable, CyclableID>.Add(cyclable.ID, cyclable);
        }*/


        public static void DoSnapshot(Snapshotter sn)
        {
            IDCounter = (CyclableID)sn.DoEnum(IDCounter);
        }
       

        public static void ResetIDCounterNoInvoke() 
        {
            IDCounter = CyclableID.First;
        }


        /// <summary>
        /// reuse this...
        /// </summary>
        /// <param name="cyclableID"></param>
        /// <param name="cyclable"></param>
        /// <param name="sn"></param>
   /*     public static void SnapshotICyclable(ref CyclableID? cyclableID, ICyclable cyclable, Snapshotter sn)
        {
            if (cyclable != null)
            {
                cyclableID = cyclable.ID;
            }
            else
            {
                cyclableID = null;
            }

            cyclableID = sn.DoEnumNullable(cyclableID);

        }*/

    }
}
