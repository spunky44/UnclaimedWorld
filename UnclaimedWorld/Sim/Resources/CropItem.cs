using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameStateManagement;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Jobs;
using UWGame.Control;
using UWGame.SimSide.Snapshots;


namespace UWGame.SimSide.Resources
{
    /// <summary>
    /// a pseudo item that can grow and possibly ripen
    /// TODO: add a Destroy method and an ID field
    /// </summary>
    public class CropItem: IResourceItem
    {
        /// <summary>
        /// 0 - 1: 1 = fully ripe
        /// Grains are ready to harvest when they are dry... let's just call that ripe too.
        /// </summary>
        public float Ripeness;

        public float Bulk;

        /// <summary>
        /// 1 = normal potential.
        /// Simulates different placement of the crop item for instance... gives some variability.
        /// </summary>
        public float GrowthSpeedFactor;

        public Crop crop;
        ResourceID snapshotCrop;

        public bool IsFullyGrown = false;

        public ResourceContainer Container
        {
            get { return crop; }
        }

      
        public ProcessJob AssignedToJob { get; set; }
        JobID? snapshotJob;


        public CropItem()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor");       

        }

        public CropItem(Crop crop)
        {
            AddToLookup();

            // distribute from 0.75 to 1.25:
            GrowthSpeedFactor = 1.25f - 0.25f * (float)(The.Sim.GameplayRandomGenerator.NextDouble("CropItem") + The.Sim.GameplayRandomGenerator.NextDouble("CropItem"));

            this.crop = crop;
        }

      /*  public bool IsFullyGrown()
        {
            return Bulk >= crop.CropType.CropItem.Bulk;
        }*/

     /*   public float EstimateGrowth(double deltaTimeInSeconds)
        {
            float growth = (float)(GrowthSpeed * deltaTimeInSeconds);
            return crop.CropType.CropItem.Bulk;  
        }*/


        public bool Grow(ref float availableForGrowth, double deltaDays)
        {            
            float growth = (float)(GrowthSpeedFactor * deltaDays * crop.ResourceType.CropType.CropItemGrowthPerDay);

            growth = Math.Min(availableForGrowth, growth);

            float maxGrowth = crop.ResourceType.ResourceItemType.ItemType.MaximumBulk.Value - Bulk;

            growth = Math.Min(maxGrowth, growth);

            if (growth > 0f)
            {
                Bulk += growth;
                availableForGrowth -= growth;

                if (Bulk >= crop.ResourceType.ResourceItemType.ItemType.MaximumBulk.Value)
                {
                    IsFullyGrown = true;

                    Bulk = crop.ResourceType.ResourceItemType.ItemType.MaximumBulk.Value;
                }

                return true;
            }
            else
            {                
                return false;
            }
        }


        public void Ripen(double deltaDays)
        {
            Ripeness += (float)(deltaDays * crop.ResourceType.CropType.RipeSpeed.Value);

            Ripeness = Math.Min(1f, Ripeness);
        }

        public void UpdateSimulation(double deltaTimeInSeconds)
        {


        }

        public bool IsRipe()
        {
            return crop.ResourceType.CropType.RipeSpeed == null || Ripeness >= 1f;
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
           
            this.snapshotJob = sn.SnapshotID<Job, JobID>(AssignedToJob);

            this.Bulk = sn.DoFloat(Bulk);
            this.snapshotCrop = (ResourceID)sn.SnapshotID<ResourceContainer, ResourceID>(crop);
            this.GrowthSpeedFactor = sn.DoFloat(GrowthSpeedFactor);
            this.IsFullyGrown = sn.DoBool(IsFullyGrown);
            this.Ripeness = sn.DoFloat(Ripeness);
            

            sn.Ignore(crop);
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

            crop = (Crop)LookUp<ResourceContainer, ResourceID>.FindByID(snapshotCrop);

            if (snapshotJob != null)
            {
                AssignedToJob = (ProcessJob)LookUp<Job, JobID>.FindByID(snapshotJob);
            }
        }

        #endregion
    }
}
