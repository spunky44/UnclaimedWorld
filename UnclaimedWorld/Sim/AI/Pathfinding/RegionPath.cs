using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Pathfinding
{
    public enum RegionPathID : long 
    {
        Invalid = long.MaxValue,
        Max = Invalid,
        First = 1
    }

    /// <summary>
    /// a high-level path that can be cached and passed to the pathfinder
    /// </summary>
    public class RegionPath : ILookUp<RegionPath, RegionPathID>, ISnapshot
    {
        /// <summary>
        /// 'Gone' = cost from the pathfinder
        /// </summary>
        public float Cost;

        public List<RegionPathNode> PathNodes = new List<RegionPathNode>();


        public RegionPath()
        {
           /* if (!Snapshotter.IsSnapshotting)
            {
                AddToLookup();

            }*/

            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor");
        }

        public RegionPath(float cost)
        {             
            AddToLookup();

            this.Cost = cost;           
        }


        public void Destroy()
        {
            RemoveIDEntry();
        }

        public void AddNode(RegionPathFinderNodeAStar pathNode)
        {
            RegionPathNode newNode = new RegionPathNode(pathNode);
            PathNodes.Add(newNode);
        }

        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            IDCounter = sn.DoEnum(IDCounter);
            id = sn.DoEnum(id);

            PathNodes = sn.DoList(PathNodes);
            Cost = sn.DoFloat(Cost);

            return this;
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

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            foreach (var item in PathNodes)
            {
                item.LoadPostProcess(sn);
            }
        }

        #endregion



        #region ILookup

        private RegionPathID id = RegionPathID.Invalid;
        static RegionPathID IDCounter = RegionPathID.First;

        public RegionPathID ID
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

        public RegionPathID GetUniqueID()
        {
            IDCounter++;
            if (IDCounter >= RegionPathID.Max)
            {
                throw new Exception("Astounding, RegionPathID just exceeded 64 bits. Something seriously wrong has happened.");
            }

            return IDCounter;
        }

        public RegionPathID SnapshotID(Snapshotter sn, RegionPathID id)
        {
            return (RegionPathID)sn.DoEnum(id);
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
            if (ID != RegionPathID.Invalid)
                LookUp<RegionPath, RegionPathID>.Add(ID, this);
        }

        public void SetInvalid()
        {
            id = RegionPathID.Invalid;
        }

        public void RemoveIDEntry()
        {
            LookUp<RegionPath, RegionPathID>.Remove(this);
        }

        void ILookUp<RegionPath, RegionPathID>.ResetIDCounter() // interface method - does nothing...
        {
        }

        public static void ResetIDCounter() // called by invoke, do not remove
        {
            IDCounter = RegionPathID.First;
        }

        void ILookUp<RegionPath, RegionPathID>.CreateLookupCollection() // interface method - does nothing...
        {
        }

        public static void CreateLookupCollection()
        {
            LookUp<RegionPath, RegionPathID>.Create();
        }

        #endregion
    }
}
