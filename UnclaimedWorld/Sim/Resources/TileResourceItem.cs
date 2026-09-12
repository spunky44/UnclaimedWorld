using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems.TimeSlicing;

namespace UWGame.SimSide.Resources
{
    public class TileResourceItem : IResourceItem
    {
        public ProcessJob AssignedToJob { get; set; }
        JobID? snapshotJob;

        private TileResourceContainer container;
        ResourceID snapshotResourceContainer;

        public ResourceContainer Container
        {
            get { return container; }
        }

        public TileResourceItem()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor");       

        }

        public TileResourceItem(TileResourceContainer container)
        {
            AddToLookup();

            this.container = container;
        }


        public void Destroy()
        {
            RemoveIDEntry();
        }

        #region ILookup

        private ResourceItemID id = ResourceItemID.Invalid;

        //=================== ILookup Methods =====================
        public ResourceItemID ID
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

        public ResourceItemID GetUniqueID()
        {
            return ResourceItem.GetUniqueID();
        }

        public ResourceItemID SnapshotID(Snapshotter sn, ResourceItemID id)
        {
            return (ResourceItemID)sn.DoEnum(id);
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
            if (ID != ResourceItemID.Invalid)
                LookUp<IResourceItem, ResourceItemID>.Add(ID, this);
        }

        public void RemoveIDEntry()
        {
            LookUp<IResourceItem, ResourceItemID>.Remove(this);
        }

        public void SetInvalid()
        {
            id = ResourceItemID.Invalid;
        }

        public void ResetIDCounter() // interface method - does nothing... Sim will call Cyclable.ResetIDCounter.
        {
        }

        void ILookUp<IResourceItem, ResourceItemID>.CreateLookupCollection() // interface method - does nothing...
        {
        }

        public static void CreateLookupCollection()
        {
            LookUp<IResourceItem, ResourceItemID>.Create();
        }

        #endregion


        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            this.id = (ResourceItemID)sn.DoEnum(id);
           
            this.snapshotResourceContainer = (ResourceID)sn.SnapshotID<ResourceContainer, ResourceID>(container);
            this.snapshotJob = sn.SnapshotID<Job, JobID>(AssignedToJob);

            sn.Ignore(container);
            sn.Ignore(AssignedToJob);


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

            container = (TileResourceContainer)LookUp<ResourceContainer, ResourceID>.FindByID(snapshotResourceContainer);


            if (snapshotJob != null)
            {
                AssignedToJob = (ProcessJob)LookUp<Job, JobID>.FindByID(snapshotJob);
            }
        }

        #endregion
    }
}
