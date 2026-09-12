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
    /// can contain items spawned by the fish trap... 
    ///    
    /// 
    /// </summary>
    public class OtherContainer : Container
    {
       
        /// <summary>
        /// contains process output. Not meant for other storage.
        /// </summary>
        ItemStorage storage;



        public OtherContainer(Entity parent)
            : base(parent)
        {

            OtherContainerType toolType = (OtherContainerType)parent.EntityType.ContainerType;
                        
            storage = new ItemStorage(parent, true, toolType.StorageType);
             
        }

        public OtherContainer()         
        {
            Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");     
        }

        public float? TotalStoredOutput
        {
            get
            {
                return storage.TotalStored;
                
            }
        }

        public float? TotalOutputCapacity
        {
            get
            {
                return storage.TotalCapacity;               
            }
        }

       

        public bool HasCapacityForOutput(Entity item)
        {
            return storage.HasCapacityForItem(item);
                        
        }

        #region Container members



        public override void IterateContained(Action<Entity> del) //Container.IterateMethod del)
        {           
            storage.IterateContained(del);
            
        }

        public override List<Entity> GetContainedItemsList(Predicate<Entity> rule)
        {
            List<Entity> items = new List<Entity>();
                      
            storage.GetContainedItemsList(rule, items);
            

            return items;
        }


        public override bool Contains(EntityID entityID)
        {
            return (storage != null && storage.Contains(entityID));
        }

       

        protected override bool AddToContainList(Entity entity, StorageCompartment? compartment = null, StorageCondition placeInStorage = null, List<PassengerOrCargoSlot> slotsToUse = null, 
            bool ignoreCapacity = false,
            bool replenish = false,
            bool isProductionOutput = false,
            UpgradeCategory upgradeCategory = null) //bool isUpgrade = false)
        {
            return storage.Add(entity, placeInStorage, ignoreCapacity);
            
                       
        }

        public override void SwitchEntities(Entity itemToRemove, Entity exchangeWithItem, bool ignoreCapacity = false)
        {
            if (storage.Contains(itemToRemove))
            {
                if (Remove(itemToRemove, null))
                {
                    AddToContain(exchangeWithItem, isProductionOutput: true, ignoreCapacity: true);
                }

            }
        }

        protected override bool RemoveFromContain(Entity entity, List<PassengerOrCargoSlot> slots)
        {
            return storage.Remove(entity, true);
        }

        public override void Destroy()
        {
            // eject all items:           
            storage.UncontainAllEntities(
                GameData.Instance.Constants.ConditionDamageMeanToContentsOfDestroyedContainers,
                GameData.Instance.Constants.ConditionDamageSpreadToContentsOfDestroyedContainers);

            storage.Destroy();            
        }

        #endregion


        public void UncontainAllProductionOutput() 
        {
            storage.UncontainAllEntities(0f, 0f);
            
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

           
            if (storage != null)
            {
                storage.LoadPostProcess(sn);
            }
          
        }



        #endregion
    }
}
