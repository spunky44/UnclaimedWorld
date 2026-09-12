using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Snapshots;
using System.Diagnostics;
using UWGame.SimSide.Vehicles;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Allegiances;

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
    public class TerminalContainer : Container, IStorage
    {
        /// <summary>
        /// contains stored items. must be filled!
        /// </summary>
        ItemStorage storage;


        /// <summary>
        /// contains items offered for sale, perhaps simple terminals will have none?
        /// </summary>
        ItemStorage offeredItems;


        // pricing info should be in TradeManager...


        public TerminalContainer(Entity parent)
            : base(parent)
        {

            TerminalContainerType terminalType = (TerminalContainerType)parent.EntityType.ContainerType;

            offeredItems = new ItemStorage(parent, true, terminalType.OfferedForTradeStorageType); // mandatory


            if (terminalType.ItemStorageType != null)
            {
                storage = new ItemStorage(parent, true, terminalType.ItemStorageType);
            }
        }

        public TerminalContainer()         
        {
            Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");     
        }

       /* public Storage GetStoredAsTradeOffer(Entity entity)
        {
            return offeredItems.GetStoredIn(entity.EntityID);
        }*/


        public Storage FindStorage(StorageID storageID)
        {
            Storage foundStorage = storage.FindStorage(storageID);

            if (foundStorage == null)
            {
                foundStorage = offeredItems.FindStorage(storageID);
            }

            return foundStorage;
        }


        #region Container members


        public override void IterateContained(Action<Entity> del) //Container.IterateMethod del)
        {
            if (storage != null)
            {
                storage.IterateContained(del);
            }

            if (offeredItems != null)
            {
                offeredItems.IterateContained(del);
            }


        }

        public StorageCompartment GetCompartment(StorageID storageID)
        {
            if (storage != null && storage.HasStorage(storageID))
            {
                return StorageCompartment.NormalStorage;
            }
            else return StorageCompartment.OfferedForTrade;
        }

        public override List<Entity> GetContainedItemsList(Predicate<Entity> rule)
        {
            List<Entity> items = new List<Entity>();

            if (storage != null)
            {
                storage.GetContainedItemsList(rule, items);
            }

            if (offeredItems != null)
            {
                offeredItems.GetContainedItemsList(rule, items);
            }


            return items;
        }

        public Dictionary<EntityType, List<EntityID>> GetOfferedItems()
        {
            Dictionary<Entities.EntityType, List<Entities.EntityID>> offeredEntitiesByType = new Dictionary<EntityType, List<EntityID>>(); // null;
                
            if (offeredItems != null)
            {
                    
                offeredItems.IterateContained(e =>
                {
                    Common.AddToMultiList(offeredEntitiesByType, e.EntityType, e.ID);
                });
            }            

            return offeredEntitiesByType;
        }

        /// <summary>
        /// announce ourselves to allegiances in contact
        /// </summary>
        public void ComeOnline()
        {
            SeeTerminalByOtherAllegiances(false);

        }

        private void SeeOfferedItemByOtherAllegiances(Entity item, bool isAdded)
        {
             Allegiance thisAllegiance = Parent.GetAllegianceOrOwner();

             if (thisAllegiance != null)
             {
                 thisAllegiance.LetOtherAllegiancesSeeEntity(item, isAdded);
             }

        }

        private void SeeTerminalByOtherAllegiances(bool isDestroyed)
        {
            Allegiance thisAllegiance = Parent.GetAllegianceOrOwner();

            if (thisAllegiance != null)
            {
                thisAllegiance.LetOtherAllegiancesSeeEntity(Parent, !isDestroyed);           
            }
        }





        public override bool Contains(EntityID entityID)
        {
            return storage.Contains(entityID)
                || (offeredItems != null && offeredItems.Contains(entityID));
        }

        /// <summary>
        /// NOT trade storage
        /// </summary>
        public float TotalStored
        {
            get
            {
                return storage.TotalStored;
            }
        }


        /// <summary>
        /// NOT trade storage
        /// </summary>
        public float TotalItemStorageCapacity
        {
            get
            {
                return storage.TotalCapacity;
            }
        }

        public float TotalTradeItemStorageCapacity
        {
            get
            {
                return offeredItems.TotalCapacity;
            }
        }

        public float TotalTradeItemsStored
        {
            get
            {
                return offeredItems.TotalStored;
            }
        }

        public Storage GetStoredIn(Entity entity)
        {
            Storage storedIn = storage.GetStoredIn(entity.EntityID); 

            if (storedIn == null)
            {
                storedIn = offeredItems.GetStoredIn(entity.EntityID); 
            }

            return storedIn;
        }

        public Dictionary<StorageCondition, Storage> GetStorageSpaces()
        {
            return storage.StorageSpaces;

        }

        public Dictionary<StorageCondition, Storage> GetTradeOffersStorageSpaces()
        {
            return offeredItems.StorageSpaces;

        }

        protected override bool AddToContainList(Entity entity, StorageCompartment? compartment = null, StorageCondition placeInStorage = null, 
            List<PassengerOrCargoSlot> slotsToUse = null, bool ignoreCapacity = false,
            bool isReplenish = false,
            bool isProductionOutput = false,
            UpgradeCategory upgradeCategory = null) //bool isUpgrade = false)
        {
            // let's make normal storage the default:
            StorageCompartment compartmentToUse = compartment ?? StorageCompartment.NormalStorage;

            if (compartmentToUse == StorageCompartment.OfferedForTrade) //  isOfferedForSale)
            {
                bool wasAdded = offeredItems.Add(entity, placeInStorage, ignoreCapacity);

                if (wasAdded)
                {
                    // othersite allegiances can't see the tile itself, so we have to see/unsee explicitly
                    SeeOfferedItemByOtherAllegiances(entity, true);
                }

                return wasAdded;

            }
            else if (compartmentToUse == StorageCompartment.NormalStorage)
            {
                return storage.Add(entity, placeInStorage, ignoreCapacity);               
            }

            return false;
        }

        public override void SwitchEntities(Entity itemToRemove, Entity exchangeWithItem, bool ignoreCapacity = false)
        {
            if (offeredItems != null && offeredItems.Contains(itemToRemove))
            {
                StorageCondition conditions = offeredItems.GetStorageConditions(itemToRemove.EntityID);

                if (Remove(itemToRemove, null))
                {
                    AddToContain(exchangeWithItem, null, conditions, ignoreCapacity: true);
                }
            }     
            else if (storage != null && storage.Contains(itemToRemove))
            {
                StorageCondition conditions = storage.GetStorageConditions(itemToRemove.EntityID);

                if (Remove(itemToRemove, null))
                {
                    AddToContain(exchangeWithItem, null, conditions, ignoreCapacity: true);
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

            if (!wasRemoved && offeredItems != null)
            {
                wasRemoved = offeredItems.Remove(entity, true);

                if (wasRemoved)
                {
                    SeeOfferedItemByOtherAllegiances(entity, false);
                }
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

            if (offeredItems != null)
            {
                offeredItems.UncontainAllEntities(
                    GameData.Instance.Constants.ConditionDamageMeanToContentsOfDestroyedContainers,
                    GameData.Instance.Constants.ConditionDamageSpreadToContentsOfDestroyedContainers);

                offeredItems.Destroy();
            }

            SeeTerminalByOtherAllegiances(true);

        }

        #endregion




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
            this.offeredItems = (ItemStorage)sn.DoISnapshot(offeredItems);
          

            return this;
        }

        public override void LoadPostProcess(Snapshotter sn)
        {
            base.LoadPostProcess(sn);

            if (storage != null)
            {
                storage.LoadPostProcess(sn);
            }

            if (offeredItems != null)
            {
                offeredItems.LoadPostProcess(sn);
            }

        }



        #endregion
    }
}
