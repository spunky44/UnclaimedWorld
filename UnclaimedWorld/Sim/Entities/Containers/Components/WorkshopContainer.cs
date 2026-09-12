using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Snapshots;
using System.Diagnostics;
using UWGame.SimSide.Vehicles;

namespace UWGame.SimSide.Entities.Containers.Components
{
    /// <summary>
    /// can contain: 
    /// 
    /// Replenish items
    /// Stored items
    /// Production output
    /// 
    /// </summary>
    public class WorkshopContainer : Container, IStorage, IReplenishes, IHasReplenishItems, IHoldsProductionOutput
    {
        /// <summary>
        /// contains stored items - always filled
        /// </summary>
        ItemStorage storage;


        /// <summary>
        /// optional.
        /// contains process output. Not meant for other storage.
        /// </summary>
        ItemStorage productionOutput;


        ReplenishItems replenishItems;
        public ReplenishItems ReplenishItems
        {
            get
            {
                return replenishItems;
            }
        }



        public WorkshopContainer(Entity parent)
            : base(parent)
        {

            WorkshopContainerType toolType = (WorkshopContainerType)parent.EntityType.ContainerType;
            /* if (toolType.ItemStorageType != null)
             {*/
                 storage = new ItemStorage(parent, true, toolType.ItemStorageType); // mandatory
            // }

             if (toolType.RequiresReplenishType != null)
             {
                 replenishItems = new ReplenishItems(parent); 
             }

             if (toolType.ProductionOutputStorageType != null)
             {
                 productionOutput = new ItemStorage(parent, true, toolType.ProductionOutputStorageType);
             }
        }

        public WorkshopContainer()         
        {
            Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");     
        }

        public bool HasCapacityForOutput(Entity item)
        {
            if (productionOutput != null)
            {
                return productionOutput.HasCapacityForItem(item);

            }

            return false;
        }

        public float? TotalStoredOutput
        {
            get
            {
                if (productionOutput != null) // is optional..
                {
                    return productionOutput.TotalStored;
                }
                else return null;
            }
        }

        public float? TotalOutputCapacity
        {
            get
            {
                if (productionOutput != null) // is optional..
                {
                    return productionOutput.TotalCapacity;
                }
                else return null;
            }
        }

        public Storage FindStorage(StorageID storageID)
        {
            Storage foundStorage = storage.FindStorage(storageID);

            if (foundStorage == null)
            {
                foundStorage = productionOutput.FindStorage(storageID);
            }

            return foundStorage;
        }

        #region IReplenishes

        public bool IsReplenishing(EntityID entityID)
        {
            if (replenishItems != null)
            {
                return replenishItems.Contains(entityID);
            }

            return false;
        }
       

        #endregion

        #region Container members


        public StorageCompartment GetCompartment(StorageID storageID)
        {
            if (this.storage != null && storage.HasStorage(storageID))
            {
                return StorageCompartment.NormalStorage;
            }
            else if (productionOutput != null && productionOutput.HasStorage(storageID))
            {
                return StorageCompartment.ProductionOutput;
            }
            else
            {
                return StorageCompartment.Stomach;
            }
        }

        public override void IterateContained(Action<Entity> del) //Container.IterateMethod del)
        {
            if (storage != null)
            {
                storage.IterateContained(del);
            }

            if (productionOutput != null)
            {
                productionOutput.IterateContained(del);
            }

            if (replenishItems != null)
            {
                replenishItems.IterateContained(del);
            }

        }

        public override List<Entity> GetContainedItemsList(Predicate<Entity> rule)
        {
            List<Entity> items = new List<Entity>();

            if (storage != null)
            {
                storage.GetContainedItemsList(rule, items);
            }

            if (productionOutput != null)
            {
                productionOutput.GetContainedItemsList(rule, items);
            }

            if (replenishItems != null)
            {
                replenishItems.GetContainedItemsList(rule, items);
            }

            return items;
        }


        public override bool Contains(EntityID entityID)
        {
            return storage.Contains(entityID) 
                || (productionOutput != null && productionOutput.Contains(entityID)) 
                || (replenishItems != null && replenishItems.Contains(entityID));
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

        public Storage GetStoredIn(Entity entity)
        {
            Storage storedIn = null;

            if (storage != null)
            {
                storedIn = storage.GetStoredIn(entity.EntityID);
            }

            if (storedIn == null && productionOutput != null)
            {
                storedIn = this.productionOutput.GetStoredIn(entity.EntityID);
            }

            return storedIn;
        }

        public Dictionary<StorageCondition, Storage> GetStorageSpaces()
        {
            return storage.StorageSpaces;

        }


        protected override bool AddToContainList(Entity entity, StorageCompartment? compartment = null, StorageCondition placeInStorage = null, List<PassengerOrCargoSlot> slotsToUse = null, bool ignoreCapacity = false,
            bool replenish = false,
            bool isProductionOutput = false,
            UpgradeCategory upgradeCategory = null) //bool isUpgrade = false)
        {
            if (replenish)
            {
                return replenishItems.Add(entity.ID);
            }
            else if (isProductionOutput)
            {
                return productionOutput.Add(entity, placeInStorage, ignoreCapacity);
            }
            else
            {
                return storage.Add(entity, placeInStorage, ignoreCapacity);               
            }
        }

        public override void SwitchEntities(Entity itemToRemove, Entity exchangeWithItem, bool ignoreCapacity = false)
        {
            if (storage != null && storage.Contains(itemToRemove))
            {
                StorageCondition conditions = storage.GetStorageConditions(itemToRemove.EntityID);

                if (Remove(itemToRemove, null))
                {
                    AddToContain(exchangeWithItem, null, conditions, ignoreCapacity: true);
                }
            }
            else if (replenishItems != null && replenishItems.Contains(itemToRemove.ID))
            {
                if (Remove(itemToRemove, null))
                {
                    AddToContain(exchangeWithItem, replenish: true, ignoreCapacity: true);
                }
            }
            else if (productionOutput != null && productionOutput.Contains(itemToRemove))
            {
                if (Remove(itemToRemove, null))
                {
                    AddToContain(exchangeWithItem, isProductionOutput: true, ignoreCapacity: true);
                }

            }
        }

        protected override bool RemoveFromContain(Entity entity, List<PassengerOrCargoSlot> slots)
        {

            bool wasRemoved = false;

            if (storage != null)
            {
                wasRemoved = storage.Remove(entity, true);
            }

            if (!wasRemoved && productionOutput != null)
            {
                wasRemoved = productionOutput.Remove(entity, true);
            }

            if (!wasRemoved && replenishItems != null)
            {
                wasRemoved = replenishItems.Remove(entity.ID);
            }

            return wasRemoved;


        }

        public override void Destroy()
        {

            // eject all items:
            if (storage != null)
            {
                storage.UncontainAllEntities(
                    GameData.Instance.Constants.ConditionDamageMeanToContentsOfDestroyedContainers,
                    GameData.Instance.Constants.ConditionDamageSpreadToContentsOfDestroyedContainers);

                storage.Destroy();
            }

            if (productionOutput != null)
            {
                productionOutput.UncontainAllEntities(
                    GameData.Instance.Constants.ConditionDamageMeanToContentsOfDestroyedContainers,
                    GameData.Instance.Constants.ConditionDamageSpreadToContentsOfDestroyedContainers);

                productionOutput.Destroy();
            }

            if (replenishItems != null)
            {
                replenishItems.UncontainAllEntities(
                    GameData.Instance.Constants.ConditionDamageMeanToContentsOfDestroyedContainers,
                    GameData.Instance.Constants.ConditionDamageSpreadToContentsOfDestroyedContainers);

                replenishItems.Destroy();
            }


        }

        #endregion


        public void UncontainAllProductionOutput() 
        {
            if (productionOutput != null)
            {
                productionOutput.UncontainAllEntities(0f, 0f);
            }
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
            this.productionOutput = (ItemStorage)sn.DoISnapshot(productionOutput);
            this.replenishItems = (ReplenishItems)sn.DoISnapshot(replenishItems);


            return this;
        }

        public override void LoadPostProcess(Snapshotter sn)
        {
            base.LoadPostProcess(sn);

            if (storage != null)
            {
                storage.LoadPostProcess(sn);
            }

            if (productionOutput != null)
            {
                productionOutput.LoadPostProcess(sn);
            }

            if (replenishItems != null)
            {
                replenishItems.LoadPostProcess(sn);
            }
        }



        #endregion
    }
}
