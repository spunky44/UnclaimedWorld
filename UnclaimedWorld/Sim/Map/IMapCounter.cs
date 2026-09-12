using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Maps
{
   
    public static class IMapCounter
    {
        static IMapID IDCounter = IMapID.First;

        public static IMapID GetUniqueID()
        {
            IDCounter++;
            if (IDCounter >= IMapID.Max)
            {
                throw new Exception("Astounding, IMapID just exceeded 64 bits. Something seriously wrong has happened.");
            }

            return IDCounter;
        }


        public static void DoSnapshot(Snapshotter sn)
        {
            IDCounter = (IMapID)sn.DoEnum(IDCounter);
        }


        public static void ResetIDCounterNoInvoke()
        {
            IDCounter = IMapID.First;
        }

    }
}
