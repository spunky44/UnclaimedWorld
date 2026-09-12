using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Items;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Jobs;
using Xclna.Xna.Animation;
using UWGame.SimSide.Vehicles;
using UWGame.ClientSide.Renderables;
using UWGame.ClientSide;
using GameStateManagement;
using UWGame.SimSide.Entities.Containers;
using UWGame.Control;
using UWGame.SimSide.Snapshots;
using System.Diagnostics;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Entities.Containers.Components;//TODO DECOUPLE

namespace UWGame.SimSide.Entities.Containers
{
    /// <summary>
    /// This class organizes stored items in different storage compartments, with different conditions.
    /// </summary>
    public class ItemStorage: ISnapshot
    {
        private bool capacityIsFixed;

        /// <summary>
        /// This should probably remain constant for agents' item storage. The HaulingJobManager (and JobManager) works best when all persons have a certain capacity (Bulk = 1)
        /// 
        /// For Stomach, this gets set from the entity's Bulk whenever it changes.
        /// </summary>
        public float TotalCapacity;

        /// <summary>
        /// not used by Stomach
        /// </summary>
        private float? storageTypeFixedCapacity;

        /// <summary>
        /// static (global), because each entity can have multiple ItemStorage instances.
        /// </summary>
        public static StorageID IDCounter;




        /// <summary>
        /// keep this? as optimization for fast checks? we need it for iterating...
        /// </summary>
        public List<EntityID> StoredItems = new List<EntityID>();

        /// <summary>
        /// StorageCondition.Keyname is key
        /// </summary>
        public Dictionary<StorageCondition, Storage> StorageSpaces;
       // private Dictionary<string, StorageID> snapshotStorageSpaces;     


        /// <summary>
        /// for quick lookup to give the current storage
        /// </summary>
        private Dictionary<EntityID, Storage> StorageLookup = new Dictionary<EntityID, Storage>();
        private Dictionary<EntityID, StorageID> snapshotStorageLookup = new Dictionary<EntityID, StorageID>();

       

        private bool totalStoredIsDirty = true;
        private float totalStored = 0f;

        public Entity Parent;
        private EntityID snapshotParent;

      //  private ItemStorageType storageType;
       
      /*  public Dictionary<Storage.Conditions, Storage> StorageSpaces;
        private Dictionary<Storage.Conditions, StorageID> snapshotStorageSpaces;*/



        /// <summary>
        /// keep this total...?
        /// </summary>
        public float TotalStored
        {
            get
            {
                if (totalStoredIsDirty)
                {
                    CalculateTotalStored();
                    totalStoredIsDirty = false;
                }
                return totalStored;
            }
        }


        public float UnusedCapacity
        {
            get
            {
                return TotalCapacity - TotalStored;
            }
        }

        public bool HasStorage(StorageID storageID)
        {
            return StorageSpaces.Any(s => s.Value.ID == storageID);
        }


        public Storage GetStoredIn(EntityID entity)
        {
            Storage storage = null;
            StorageLookup.TryGetValue(entity, out storage);
            
            return storage;
        }


        public StorageCondition GetStorageConditions(EntityID entity)
        {
            Storage storage = GetStoredIn(entity);

            if (storage != null)
            {
                return storage.StorageConditions;
            }

            return null;
        }

        public void RecalculateTotalCapacity()
        {
            if (!capacityIsFixed)
            {
                // this seems to let cargo capacity depend on the people onboard... not sure this is a good idea...

                TotalCapacity = storageTypeFixedCapacity.Value; // storageType.GetTotalCapacity();

                if (Parent.Vehicle != null && Parent.Vehicle.Slots != null)
                {
                    TotalCapacity -= Parent.Vehicle.CargoCapacityTakenUpByPassengers;

                }
            }
        }

       

        /// <summary>
        /// all calls to this must go through the Container!!!
        /// 
        /// storageCondition can be null. Then the best storage is selected.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="storageCondition"></param>
        /// <returns></returns>
        public bool Add(Entity item, StorageCondition storageCondition, bool ignoreCapacity = false)
        {
            if (ignoreCapacity == true || HasCapacityForItem(item))
            {
                if (storageCondition == null)
                {
                    if (item.EntityType.NonLivingType != null // store living things too??
                        && StorageSpaces.Count > 1)
                    {
                        // select the best storage for this item:

                        /*if (item.EntityType.NonLivingType != null) //MLo crash fix
                        {*/
                            // step through the best options first:
                            foreach (DegradeType.StorageDamageEstimation storageDamage in item.EntityType.NonLivingType.FinalDegradeType.ConditionDamages)
                            {
                                if (StorageSpaces.ContainsKey(storageDamage.Condition))
                                {
                                    if (AddToStorage(item, storageDamage.Condition))
                                    {
                                        return true;
                                    }
                                }
                            }
                       // }
                    }
                    else
                    {
                        // isolated is default:
                        if (AddToStorage(item, GameData.Instance.AllStorageConditions["isolated"] /* Storage.Conditions.Isolated*/))
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    if (StorageSpaces.ContainsKey(storageCondition))
                    {
                        if (AddToStorage(item, storageCondition))
                        {
                           
                            return true;
                        }
                    }
                }

                // we cheat here... if there was no room in the compartments, but the total capacity is there,
                // we place it in the default storage which may exceed its capacity a little.
               
                AddToStorage(item, GameData.Instance.AllStorageConditions["isolated"] /* Storage.Conditions.Isolated*/, false); // don't test capacity

               
                return true;
            }

            return false;
        }

        private bool AddToStorage(Entity item, StorageCondition condition, bool testCapacity = true)
        {
           
            Storage addToStorage = StorageSpaces[condition];
            if (!testCapacity || addToStorage.HasCapacityForItem(item))
            {
                Item itemComponent = item.Item;
                addToStorage.Add(item.EntityID);
                StoredItems.Add(item.EntityID);

                StorageLookup.Add(item.EntityID, addToStorage);


                item.ContainedBy = Parent.EntityID;

             
                totalStoredIsDirty = true;

                return true;
            }
            else return false;
        }

        public void IterateContained(Action<Entity> iterateMethod) //Container.IterateMethod iterateMethod)
        {
            EntityID entityID;
            Entity entityInside;
            for (int i = StoredItems.Count - 1; i >= 0; i--)
            {
                entityID = StoredItems[i];

                entityInside = Entity.FindByID(entityID);
                if (entityInside != null)
                {
                    iterateMethod(entityInside);
                }
                else
                {
                    RemoveOutdatedItem(entityID);
                }
            }
           
        }

        public bool IterateContainedBreakOnTrue(Container.IterateBoolMethod iterateMethod)
        {
            EntityID entityID;
            Entity entityInside;
            for (int i = StoredItems.Count - 1; i >= 0; i--)
            {
                entityID = StoredItems[i];

                entityInside = Entity.FindByID(entityID);
                if (entityInside != null)
                {
                    if (iterateMethod(entityInside))
                    {
                        return true;
                    }
                }
                else
                {
                    RemoveOutdatedItem(entityID);
                }
            }

            return false;
        }

        public void GetContainedItemsList(Predicate<Entity> rule, List<Entity> items)
        {
            EntityID entityID;
            Entity entityInside;
            for (int i = StoredItems.Count - 1; i >= 0; i--)
            {
                entityID = StoredItems[i];

                entityInside = Entity.FindByID(entityID);
                if (entityInside != null)
                {
                    if (rule == null || rule(entityInside))
                    {
                        items.Add(entityInside);
                    }
                }
                else
                {
                    RemoveOutdatedItem(entityID);
                }
            }

        }

        public bool Contains(EntityID item)
        {
            return StoredItems.Contains(item);
        }

        public bool Contains(Entity item)
        {
            return StoredItems.Contains(item.EntityID);
        }

     /*   public bool Remove(EntityID item, bool removeFromChildStorage)
        {
            Entity itemEntity = Entity.FindByID(item);


        }*/


        /// <summary>
        /// ejects all stored entities (items) and may apply condition damage to them
        /// </summary>
        /// <param name="damageStandardDev"></param>
        /// <param name="damageSpread"></param>
        public void UncontainAllEntities(float? damageStandardDev = null, float? damageSpread = null) //float? chanceToDestroy = null)
        {
            EntityID entityID;
            Entity entity;
            for (int i = StoredItems.Count - 1; i >= 0; i--)
            {
                entityID = StoredItems[i];

                entity = Entity.FindByID(entityID);

                if (entity != null)
                {
                    if (Parent.Contains.Remove(entity)) // WHY does this call Parent??? all other classes call this.Remove() ???
                    {
                        Parent.Contains.EjectEntity(entity);

                        if (damageStandardDev.HasValue && damageSpread.HasValue)
                        {
                            float damage = (float)The.Sim.GameplayRandomGenerator.RandomNormalDistribution(damageStandardDev.Value, damageSpread.Value);

                            entity.DoDamage(damage); //, false);
                        }
                        
                    }
                }
                else
                {
                    RemoveOutdatedItem(entityID);
                }
            }

        }

        public bool Remove(Entity item, bool removeFromChildStorage)
        {
          
            return Remove(item.EntityID, removeFromChildStorage);
        }

        /// <summary>
        /// does not set ContainedBy to null on the Entity/Item
        /// </summary>
        /// <param name="item"></param>
        /// <param name="removeFromChildStorage"></param>
        /// <returns></returns>
        public bool Remove(EntityID item, bool removeFromChildStorage)
        {
          
            if (removeFromChildStorage)
            {
                Storage storage;
                if (StorageLookup.TryGetValue(item, out storage))
                {
                    storage.Remove(item);
                    RemoveFromAuxiliaryCollections(item);
                                       

                    return true;

                }
                
                return false;
            }
            else
            {
                RemoveFromAuxiliaryCollections(item);

                return true;
            }
        }

        private void RemoveFromAuxiliaryCollections(EntityID item)
        {
            StorageLookup.Remove(item);

            StoredItems.Remove(item);

            totalStoredIsDirty = true;
        }

       /* public bool Remove(EntityID item, bool removeFromChildStorage)
        {
           
            if (removeFromChildStorage)
            {
                foreach (KeyValuePair<Storage.Conditions, Storage> kvp in StorageSpaces)
                {
                    if (kvp.Value.Remove(item, false))
                    {
                        totalStoredIsDirty = true;
                        StoredItems.Remove(item);
                        return true;
                    }
                }

                return false;
            }
            else
            {
                totalStoredIsDirty = true;
                StoredItems.Remove(item);
                return true;
            }
        }*/


        /// <summary>
        /// used to clean up outdated (destroyed) items
        /// </summary>
        /// <param name="item"></param>
        /// <param name="removeFromChildStorage"></param>
        /// <returns></returns>
        public bool RemoveOutdatedItem(EntityID item)
        {            
            
            foreach (var kvp in StorageSpaces)
            {
                if (kvp.Value.RemoveOutdatedItem(item))
                {
                    RemoveFromAuxiliaryCollections(item);

                    return true;
                }
            }

            return false;
            
        }


        public void Destroy()
        {
            foreach (var item in StorageLookup)
            {
                item.Value.Destroy();
            }
        }

        public ItemStorage()         
        {
            Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");     
        }


        public ItemStorage(Entity entity, bool hasFixedCapacity, ItemStorageType parentStorageType)
        {
            this.Parent = entity;

            //storageType = parentStorageType;

            if (hasFixedCapacity)
            {
                // this capacity will never change, so let's compute it now:
                TotalCapacity = parentStorageType.GetTotalCapacity(); // TotalCapacity;
                storageTypeFixedCapacity = TotalCapacity;
            }

            StorageSpaces = new Dictionary<StorageCondition, Storage>();
            
            capacityIsFixed = hasFixedCapacity;

            foreach (var kvp in parentStorageType.StorageSpaces)
            {
                StorageSpaces.Add(GameData.Instance.AllStorageConditions[kvp.Key], new Storage(this, GameData.Instance.AllStorageConditions[kvp.Key]) { TotalCapacity = kvp.Value.Capacity });
            }


            // add default storage:
          /*  StorageSpaces.Add(Storage.Conditions.Isolated, 
                new Storage(this) { StorageType = Storage.Conditions.Isolated, TotalCapacity = this.TotalCapacity });
            */
        }

        

        private float CalculateTotalStored()
        {
            float bulk = 0f;

            for (int i = StoredItems.Count - 1; i >= 0; i--)
            {
                EntityID item = StoredItems[i];
                Entity itemEntity = Entity.FindByID(item);
                if (itemEntity != null)
                {
                    bulk += itemEntity.Bulk;
                }
                else
                {
                    StoredItems.Remove(item);                   
                }
            }           

            totalStored = bulk;
            return bulk;
        }


      /*  public void SwitchEntities(Entity itemToRemove, Entity exchangeWithItem)
        {            
            // make the switch:
            Storage storage = StorageLookup[itemToRemove.ID];

            storage.SwitchEntities(itemToRemove, exchangeWithItem);

            StoredItems.Remove(itemToRemove.ID);
            StoredItems.Add(exchangeWithItem.ID);

            StorageLookup.Remove(itemToRemove.ID);
            StorageLookup.Add(exchangeWithItem.ID, storage);



            totalStoredIsDirty = true; // if they have different bulk...
                     
        }*/


      /*  public bool PickUpItem(Entity item)
        {
            if (Add(item, null))
            {
               
                return true;
            }
            else
            {
                return false;
            }
        }*/

       
        /*
        public bool LoadItem(Item item)
        {
            if (item.Bulk + TotalStored <= TotalCapacity)
            {
                //StoredItems.Add(item);
                if (!Add(item, null))
                {
                    return false;
                }

                if (item.MapPosition.X > -1 && item.MapPosition.Y > -1)
                {
                    UWGame.SimSide.Instance.Map.TileMap[item.MapPosition.X][item.MapPosition.Y].RemoveItem(item);
                }

                item.InUseBy = null;
                //item.OnBoard = Parent;

                totalStoredIsDirty = true;
                Parent.Renderable.RenderAsModel.currentMaximumSpeedIsDirty = true;

                // NEW: set  the real position instead - is safer...
                item.MapPosition = Parent.MapPosition;

                return true;
            }
            else
            {
                return false;
            }
        }
        */
      
      /*  public bool DropCarriedItem(Entity item, Entity placeInStorageEntity, Storage.Conditions? placeInStorage)
        {
            if (Contains(item)) //StoredItems.Contains(item))
            {
                //StoredItems.Remove(item);
                Remove(item, true);

                item.Item.DropByEntity(Parent, placeInStorageEntity, placeInStorage);

                if (Parent.Locomotor != null)
                {
                    Parent.Locomotor.currentMaximumSpeedIsDirty = true;
                }

                //RecalculateAndSetCurrentSpeed();

                HandleAttachedBoxAnimation(Parent);

                return true;
            }

            return false;
        }*/









       
       

       

      /*  public bool UnloadItem(Entity item, Vector3 location, List<PassengerOrCargoSlot> slots)
        {
            foreach (PassengerOrCargoSlot slot in slots)
            {
                slot.CargoSlot.TargetedByHauler = null;
            } 
           

            if (StoredItems.Contains(item.ID))
            {
                item.Item.UnloadFromVehicle(Parent, location);
                totalStoredIsDirty = true;
                Parent.Locomotor.currentMaximumSpeedIsDirty = true;

              //  slot.CargoSlot.BulkCarried = Common.ClampBottom(slot.CargoSlot.BulkCarried - item.Bulk, 0f);

                
                // handle the slots (if present):
                float storedInSlot;
                float bulkStillToUnload = item.Bulk;
                              
                foreach (PassengerOrCargoSlot slot in slots)
                {
                    storedInSlot = slot.CargoSlot.BulkCarried;

                   // roomInSlot = slot.CargoSlot.GetRemainingRoom();

                    if (storedInSlot >= bulkStillToUnload)
                    {
                        slot.CargoSlot.BulkCarried -= bulkStillToUnload;

                        break;
                    }
                    else
                    {
                        slot.CargoSlot.BulkCarried -= storedInSlot;

                        bulkStillToUnload -= storedInSlot;
                    }

                }


                if (TotalStored == 0f)
                {
                    // clean up!!!
                    if (Parent.Vehicle != null)
                    {
                        Parent.Vehicle.EmptyAllCargoSlots();
                    }
                }

                if (Parent.Vehicle != null)
                {
                    Parent.Vehicle.UpdateCargoSlotsWithAttachedModels();
                }

                return true;
            }
            else
            {
                return false;
            }

        }*/

        public bool HasCapacityForItem(Entity itemToPickUp)
        {
            /* this doesn't match what the HaulEvaluator is doing:
            foreach (KeyValuePair<Storage.Conditions, Storage> kvp in StorageSpaces)
            {
                if (kvp.Value.HasCapacityForItem(itemToPickUp))
                {
                    return true;
                }
            }

            return false;
            */

            return HasCapacityForItem(itemToPickUp.Bulk);
        }

        public bool HasCapacityForItem(float itemBulk)
        {

            return Common.IsLessThanOrEqual(itemBulk, TotalCapacity - TotalStored);
        }

        public bool HasCapacityForItemWhenEmpty(Entity itemToPickUp)
        {

            return itemToPickUp.Bulk <= TotalCapacity; // HasCapacityForItem(itemToPickUp.Bulk);
        }

        public bool HasCapacityForItemWhenEmpty(float bulk)
        {
            return bulk <= TotalCapacity; 
        }

        public Storage FindStorage(StorageID storageID)
        {           
            foreach (var item in StorageSpaces)
            {
                if (item.Value.ID == storageID)
                {
                    return item.Value;
                }

            }   

            return null;
        }

        public static void ResetIDCounterNoInvoke() 
        {
            IDCounter = StorageID.First;
        }


        #region ISnapshot

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

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            this.capacityIsFixed = sn.DoBool(capacityIsFixed);
            
            //snapshotParent = Parent.EntityID;
            snapshotParent = (EntityID)sn.SnapshotID<Entity, EntityID>(Parent); // sn.DoEntityID(snapshotParent);

            IDCounter = sn.DoEnum(IDCounter);

           
            if (sn.mode != Snapshotter.Mode.Load)
            {  
                snapshotStorageLookup = StorageLookup.ToDictionary(s => s.Key, s => s.Value.ID);
                //snapshotStorageSpaces = StorageSpaces.ToDictionary(s => s.Key.KeyName, s => s.Value.ID);
            } 
            snapshotStorageLookup = sn.DoDictionary(snapshotStorageLookup);

            StorageSpaces = sn.DoDictionary(StorageSpaces);

            //snapshotStorageSpaces = sn.DoDictionary(snapshotStorageSpaces);

            this.storageTypeFixedCapacity = sn.DoFloatNullable(storageTypeFixedCapacity);
            this.StoredItems = sn.DoList(StoredItems);
            this.TotalCapacity = sn.DoFloat(TotalCapacity);
            this.totalStored = sn.DoFloat(totalStored);
            this.totalStoredIsDirty = sn.DoBool(totalStoredIsDirty);
            
            sn.Ignore(StorageLookup);
     
            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            this.Parent = Entity.FindByID(snapshotParent);

            StorageLookup = snapshotStorageLookup.ToDictionary(s => s.Key, s => FindStorage(s.Value));

            if (StorageLookup != null)
            {
                foreach (var item in StorageLookup)
                {
                    item.Value.LoadPostProcess(sn);
                }
            }

           // StorageLookup = snapshotStorageLookup.ToDictionary(s => s.Key, s => LookUp<Storage, StorageID>.FindByID(s.Value));
           // StorageSpaces = snapshotStorageSpaces.ToDictionary(s => GameData.Instance.AllStorageConditions[s.Key], s => LookUp<Storage, StorageID>.FindByID(s.Value));


        }

       

        #endregion
    }
}
