using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Vehicles;
using UWGame.SimSide.Snapshots;
using System.Diagnostics;

namespace UWGame.SimSide.Entities.Containers.Components
{
 

    /// <summary>
    /// this is for the storage hole...
    /// </summary>
    class StorageContainer: Container, IStorage
    {

        ItemStorage storage;


        public StorageContainer(Entity parent)
            : base(parent)
        {
            storage = new ItemStorage(parent, true, ((StorageContainerType)parent.EntityType.ContainerType).ItemStorageType);

          
        }

        public StorageContainer()         
        {
            Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");     
        }

        public override void IterateContained(Action<Entity> del) // Container.IterateMethod del)
        {
            if (storage != null)
            {
                storage.IterateContained(del);
            }         

        }

        public override List<Entity> GetContainedItemsList(Predicate<Entity> rule)
        {
            List<Entity> items = new List<Entity>();

            if (storage != null)
            {
                storage.GetContainedItemsList(rule, items);
            }
            
            return items;
        }

        public StorageCompartment GetCompartment(StorageID storageID)
        {
            return StorageCompartment.NormalStorage;
        }


        public override void Destroy()
        {
            // eject all items:
            storage.UncontainAllEntities(
                GameData.Instance.Constants.ConditionDamageMeanToContentsOfDestroyedContainers,
                GameData.Instance.Constants.ConditionDamageSpreadToContentsOfDestroyedContainers);

            storage.Destroy();
            
        }

        public float TotalStored
        {
            get
            {
                return storage.TotalStored;
            }
        }
        public float TotalItemStorageCapacity
        {
            get
            {
                return storage.TotalCapacity;
            }
        }


        public override bool Contains(EntityID item)
        {
            return storage.Contains(item);
        }

        public Dictionary<StorageCondition, Storage> GetStorageSpaces()
        {
            return storage.StorageSpaces;

        }

        public Storage FindStorage(StorageID storageID)
        {
            return storage.FindStorage(storageID);
        }


        public Storage GetStoredIn(Entity entity)
        {
            return storage.GetStoredIn(entity.EntityID);
        }

        protected override bool AddToContainList(Entity entity, StorageCompartment? compartment = null, StorageCondition placeInStorage = null, List<PassengerOrCargoSlot> slotsToUse = null,
            bool ignoreCapacity = false,
            bool replenish = false,
            bool isProductionOutput = false, 
            UpgradeCategory upgradeCategory = null) //)bool isUpgrade = false,
            
        {
            return storage.Add(entity, placeInStorage, ignoreCapacity);
        }

        public override void SwitchEntities(Entity itemToRemove, Entity exchangeWithItem, bool ignoreCapacity = false)
        {
            StorageCondition conditions = storage.GetStorageConditions(itemToRemove.EntityID);

            if (Remove(itemToRemove, null))
            {
                AddToContain(exchangeWithItem, null, conditions, ignoreCapacity: true);
            }

           /* if (storage.Contains(itemToRemove))
            {
                storage.SwitchEntities(itemToRemove, exchangeWithItem);
            }  */          
        }


        protected override bool RemoveFromContain(Entity entity, List<PassengerOrCargoSlot> slots)
        {
            bool wasRemoved = storage.Remove(entity, true);

            return wasRemoved;
            
        }


        #region ISnapshot

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public override Snapshotter.Version DoVersion(Snapshotter sn)
        {
            base.DoVersion(sn); // each class in the class hierarchy snapshots and maintains their own version.

            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }


        public override ISnapshot DoSnapshot(Snapshotter sn)
        {
            base.DoSnapshot(sn);

            this.storage = (ItemStorage)sn.DoISnapshot(storage);


            return this;
        }

        public override void LoadPostProcess(Snapshotter sn)
        {
            base.LoadPostProcess(sn);

            storage.LoadPostProcess(sn);
        }
        

        #endregion
    }
}
