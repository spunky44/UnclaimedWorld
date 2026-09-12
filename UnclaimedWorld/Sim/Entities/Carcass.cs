using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities
{
   /* public class Carcass : IResourceItem // IHarvestItemContainer
    {
        ResourceContainer container;

        public ResourceContainer Container
        {
            get { return container; }
        }


        public ProcessJob AssignedToJob { get; set; }


        public Carcass(ResourceContainer container)
        {
            AddToLookup();
            this.container = container;
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

        #endregion
    }*/
}
