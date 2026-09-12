using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities.Substances
{
    public enum SubstancePoolID : long
    {
        First = 0L,
        Invalid = long.MaxValue,
        Max = Invalid
    }


    /// <summary>
    /// Tile, connected power grid...
    /// requests should come from: tools, structures (infinite process?), processes...
    /// </summary>
    public class SubstancePool : ILookUp<SubstancePool, SubstancePoolID>
    {

        public SubstanceType SubstanceType;

        /// <summary>
        /// TODO
        /// </summary>
        public void RequestSubstance() //float amount)
        {


        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="amount"></param>
        public void Consume(float amount)
        {
            
        }



        #region ILookup

        private SubstancePoolID id = SubstancePoolID.Invalid;
        static SubstancePoolID IDCounter = SubstancePoolID.First;

        public SubstancePoolID ID
        {
            get
            {
                return id;
            }

            private set
            {
                id = value;
            }
        }

        public SubstancePoolID GetUniqueID()
        {
            IDCounter++;
            if (IDCounter >= SubstancePoolID.Max)
            {
                throw new Exception("Astounding, SubstancePoolID just exceeded 64 bits. Something seriously wrong has happened.");
            }

            return IDCounter;
        }

        public SubstancePoolID SnapshotID(Snapshotter sn, SubstancePoolID id)
        {
            return sn.DoEnum(id);
        }



        public int LoadPostProcessOrder
        {
            get
            {
                return 0;
            }
        }

        public void AddToLookup()
        {
            ID = GetUniqueID();
            if (ID != SubstancePoolID.Invalid)
                LookUp<SubstancePool, SubstancePoolID>.Add(ID, this);
        }

        public void SetInvalid()
        {
            id = SubstancePoolID.Invalid;
        }

        public void RemoveIDEntry()
        {
            LookUp<SubstancePool, SubstancePoolID>.Remove(this);
        }

        void ILookUp<SubstancePool, SubstancePoolID>.ResetIDCounter() // interface method - does nothing...
        {
        }

        public static void ResetIDCounter() // called by invoke, do not remove
        {
            IDCounter = SubstancePoolID.First;
        }

        void ILookUp<SubstancePool, SubstancePoolID>.CreateLookupCollection() // interface method - does nothing...
        {
        }

        public static void CreateLookupCollection()
        {
            LookUp<SubstancePool, SubstancePoolID>.Create();
        }


        #endregion
    }
}
