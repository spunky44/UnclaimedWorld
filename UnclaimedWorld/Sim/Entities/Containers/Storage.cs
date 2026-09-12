using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Items;
using UWGame.SimSide.AI;
using UWGame.SimSide.Snapshots;
using System.Diagnostics;

namespace UWGame.SimSide.Entities.Containers
{
    /// <summary>
    /// not a global LookUp ID!
    /// </summary>
    public enum StorageID : ulong
    {
        Invalid = uint.MaxValue,
        Max = Invalid,
        First = 1
    }

    /// <summary>
    /// a specific storage compartment with temperature etc.
    /// 
    /// it has no knowledge of the ItemStorage instance it belongs to.
    /// </summary>
    public class Storage: ISnapshot
    {
     
        /// <summary>
        /// In Kelvin. 21 degree Celsius
        /// </summary>
        public const float AirConTemperature = 294f;


        /// <summary>
        /// In Kelvin. 5 degree Celsius
        /// </summary>
        public const float RefrigeratorTemperature = 278f;

        /// <summary>
        /// In Kelvin. 11 degree Celsius
        /// </summary>
        public const float EarthCooledTemperature = 284f;


        /// <summary>
        /// In Kelvin. -18 degree Celsius
        /// </summary>
        public const float FreezerTemperature = 255f;

        /// <summary>
        /// In Kelvin. 20 degree Celsius
        /// </summary>
        public const float RoomTemperature = 293f;
        
        /// <summary>
        /// In Kelvin. 40 degree Celsius
        /// </summary>
        public const float Hot = 313f;
        
        public float TotalCapacity;
        public StorageCondition StorageConditions;

        private bool totalStoredIsDirty = true;
        private float totalStored = 0f;

        /// <summary>
        /// don't call Add or Remove on this collection!!! use the methods instead
        /// </summary>
        public List<EntityID> StoredItems = new List<EntityID>();


        public bool IsPowered = true; // need more features... false;

      //  public ItemStorage Parent;
        /// <summary>
        /// we don't cache ItemStorage since Storage objects in MemoryFact don't have one...
        /// </summary>
        public EntityID Parent;

        public Storage(ItemStorage parent, StorageCondition conditions)
        {
            ItemStorage.IDCounter++;
            ID = ItemStorage.IDCounter;
          //  AddToLookup();

            Parent = parent.Parent.ID;
            StorageConditions = conditions;
        }

        /// <summary>
        /// copy constructor for MemoryFact - keep this up to date with changes to the class!!!
        /// </summary>
        /// <param name="original"></param>
        public Storage(Storage original)
        {
            id = original.ID; // copy

          //  AddToLookup(); // OLD: yes, even the memory fact copy gets an ID

            StorageConditions = original.StorageConditions;

            StoredItems = original.StoredItems.ToList();

            TotalCapacity = original.TotalCapacity;

            totalStored = original.TotalStored;

            IsPowered = original.IsPowered;

            Parent = original.Parent;
        }

        public Storage()         
        {
            Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");     
        }

        public void Destroy()
        {
            //RemoveIDEntry();
        }

        /// <summary>
        /// this doesn't check capacity or anything
        /// </summary>
        /// <param name="item"></param>
        public void Add(EntityID item)
        {
            StoredItems.Add(item);
            totalStoredIsDirty = true;
        }



        public bool Remove(EntityID item)
        {
            if (StoredItems.Remove(item))
            {
                totalStoredIsDirty = true;
                             

                return true;
            }

            return false;
        }

        /// <summary>
        /// used when cleaning up destroyed items from the inventory
        /// </summary>
        /// <param name="item"></param>
        /// <param name="removeFromParentItemStorage"></param>
        /// <returns></returns>
        public bool RemoveOutdatedItem(EntityID item)
        {
            if (StoredItems.Remove(item))
            {
                totalStoredIsDirty = true;
                    
                //item.Item.StoredIn = null;

                return true;
            }

            return false;
        }


        public void SwitchEntities(Entity itemToRemove, Entity exchangeWithItem)
        {
            // make the switch:
            Remove(itemToRemove.EntityID);
            Add(exchangeWithItem.EntityID);

            StoredItems.Remove(itemToRemove.EntityID);
            StoredItems.Add(exchangeWithItem.EntityID);
                        
        }


       

     /*   public static float GetMoisture(Storage.Conditions StorageConditions) //,float ambientTemperature)
        {
            if (StorageConditions == Conditions.Aquarium)
            {
                return 1f;
            }
            else if(StorageConditions == Conditions.Moist)
            {
                return 0.9f;
            }
            else return 0f;
        }*/

      /*  public static float GetTemperature(Storage.Conditions StorageConditions, bool isPowered, float ambientTemperature)
        {
            switch (StorageConditions)
            {
                case Conditions.Isolated:
                case Conditions.Aquarium:
                case Conditions.Moist:
                    return ComputeIsolatedTemperature(ambientTemperature); // ambientTemperature;
                case Conditions.Dryer:
                    return Hot;
                case Conditions.EarthCooled:
                    return EarthCooledTemperature;
                case Conditions.Refrigerator:
                    if (isPowered)
                    {
                        return RefrigeratorTemperature;
                    }
                    else return ambientTemperature;
                case Conditions.Freezer:
                    if (isPowered)
                    {
                        return FreezerTemperature;
                    }
                    else return ambientTemperature;
                case Conditions.Airconditioning:
                    if (isPowered)
                    {
                        return RoomTemperature;
                    }
                    else return ambientTemperature;
                default: return ambientTemperature;
            }

        }*/

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

        public float CalculateTotalStored()
        {
            float bulk = 0f;
            EntityID item;
            Entity itemEntity;
            for (int i = StoredItems.Count - 1; i >= 0; i--)
            {
                item = StoredItems[i];

                itemEntity = Entity.FindByID(item);

                if (itemEntity != null)
                {
                    bulk += itemEntity.Bulk;
                }
                else
                {
                    // clean up
                    StoredItems.RemoveAt(i);
                }
            }
            
            totalStored = bulk;
            return bulk;
        }

        public bool HasCapacityForItem(IKnownEntityData itemToPickUp)
        {
            return itemToPickUp.Bulk <= TotalCapacity - TotalStored;
        }

        public bool HasCapacityForItem(float itemToPickUpBulk)
        {
            return itemToPickUpBulk <= TotalCapacity - TotalStored;
        }

       // private static StorageID IDCounter;

        private StorageID id = StorageID.Invalid;

        public StorageID ID
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

        /*
        #region ILookup

       

        public StorageID GetUniqueID()
        {
            IDCounter++;
            if (IDCounter >= StorageID.Max)
            {
                throw new Exception("Astounding, StorageID just exceeded 64 bits. Something seriously wrong has happened.");
            }

            return IDCounter;
        }

        public StorageID SnapshotID(Snapshotter sn, StorageID id)
        {
            return (StorageID)sn.DoEnum(id);
        }



        public void AddToLookup()
        {
            ID = GetUniqueID();
            if (ID != StorageID.Invalid)
                LookUp<Storage, StorageID>.Add(ID, this);
        }

        public void SetInvalid()
        {
            id = StorageID.Invalid;
        }

        public void RemoveIDEntry()
        {
            LookUp<Storage, StorageID>.Remove(this);
        }

        void UWGame.SimSide.Snapshots.ILookUp<Storage, StorageID>.ResetIDCounter() // interface method - does nothing...
        {
        }

        public static void ResetIDCounter() // called by invoke, do not remove
        {
            IDCounter = StorageID.First;
        }

        #endregion
        */

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
            this.id = sn.DoEnum(id);
            //IDCounter = sn.DoEnum(IDCounter);

            this.Parent = sn.DoEnum(Parent);
            this.IsPowered = sn.DoBool(IsPowered);
            this.StoredItems = sn.DoList(StoredItems);
            this.StorageConditions = sn.DoGameData(StorageConditions); // sn.DoEnum(StorageConditions);
            this.TotalCapacity = sn.DoFloat(TotalCapacity);
            this.totalStored = sn.DoFloat(totalStored);
            this.totalStoredIsDirty = sn.DoBool(totalStoredIsDirty);
                     

            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

         
        }



        #endregion
    }
}
