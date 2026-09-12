using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities
{
    public enum ThreatGroupID : ulong
    {
        Invalid = uint.MaxValue,
        Max = Invalid,
        First = 1
    }

    /// <summary>
    /// TODO: the threat group should be destroyed and removed from the list when there are no more entities or memory facts pointing to it...
    /// it does not hurt if it stays in the list though.
    /// </summary>
    public class ThreatGroup : ISnapshot, ILookUp<ThreatGroup, ThreatGroupID>
    {
        public string Name;
               

        public ThreatGroup(string threatGroupName)
        {
            AddToLookup();

            this.Name = threatGroupName;

            if (!string.IsNullOrEmpty(Name))
            {
                // add to the list so it can reused by others:
                The.Sim.PlaySite.ThreatGroups.Add(this);
            }
        }

        public ThreatGroup()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor");       

        }


        public static ThreatGroup GetThreatGroup(string threatGroupName)
        {
            ThreatGroup threatGroup;
            if (!string.IsNullOrEmpty(threatGroupName))
            {
                threatGroup = The.Sim.PlaySite.ThreatGroups.FirstOrDefault(t => t.Name == threatGroupName);
                if (threatGroup == null)
                {
                    threatGroup = new ThreatGroup(threatGroupName);
                }
            }
            else
            {
                threatGroup = new ThreatGroup(null);
            }
            return threatGroup;
        }

        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            this.id = sn.DoEnum(id);
            IDCounter = sn.DoEnum(IDCounter);
            this.Name = sn.DoString(Name);
           
            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            //lookups and other fix-ups
        }

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public Snapshotter.Version DoVersion(Snapshotter sn)
        {
            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }

        public bool IsSnapshotted { get; set; }


        #endregion


        #region ILookup

        private ThreatGroupID id = ThreatGroupID.Invalid;
        static ThreatGroupID IDCounter = ThreatGroupID.First;

        public ThreatGroupID ID
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

        public ThreatGroupID GetUniqueID()
        {
            IDCounter++;
            if (IDCounter >= ThreatGroupID.Max)
            {
                throw new Exception("Astounding, ThreatGroupID just exceeded 64 bits. Something seriously wrong has happened.");
            }

            return IDCounter;
        }

        public ThreatGroupID SnapshotID(Snapshotter sn, ThreatGroupID id)
        {
            return (ThreatGroupID)sn.DoEnum(id);
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
            if (ID != ThreatGroupID.Invalid)
                LookUp<ThreatGroup, ThreatGroupID>.Add(ID, this);
        }

        public void SetInvalid()
        {
            id = ThreatGroupID.Invalid;
        }

        public void RemoveIDEntry()
        {
            LookUp<ThreatGroup, ThreatGroupID>.Remove(this);
        }

        void ILookUp<ThreatGroup, ThreatGroupID>.ResetIDCounter() // interface method - does nothing...
        {
        }

        public static void ResetIDCounter() // called by invoke, do not remove
        {
            IDCounter = ThreatGroupID.First;
        }

        void ILookUp<ThreatGroup, ThreatGroupID>.CreateLookupCollection() // interface method - does nothing...
        {
        }

        public static void CreateLookupCollection()
        {
            LookUp<ThreatGroup, ThreatGroupID>.Create();
        }

        #endregion

    }
}
