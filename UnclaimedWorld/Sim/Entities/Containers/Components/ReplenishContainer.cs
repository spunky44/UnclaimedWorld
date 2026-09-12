using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Items;
using UWGame.SimSide.Vehicles;
using GameStateManagement;
using UWGame.Control;
using UWGame.SimSide.Snapshots;
using System.Diagnostics;

namespace UWGame.SimSide.Entities.Containers.Components
{
    /// <summary>
    /// a container that holds the items that are currently replenishing the entity (such as firewood, batteries, bait...)
    /// 
    /// Because vehicles can have many types of containment (passengers, cargo, fuel), there is a separate class for replenishment items
    /// </summary>
    class ReplenishContainer : Container, IReplenishes, IHasReplenishItems
    {

        ReplenishItems replenishItems;
        public ReplenishItems ReplenishItems
        {
            get
            {
                return replenishItems;
            }
        }
       
       

        public ReplenishContainer(Entity parent)
            : base(parent)
        {
            replenishItems = new ReplenishItems(parent);

        }

        public ReplenishContainer()         
        {
            Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");     
        }


        public override void Destroy()
        {
           
            // eject all items:
            replenishItems.UncontainAllEntities(
                    GameData.Instance.Constants.ConditionDamageMeanToContentsOfDestroyedContainers,
                    GameData.Instance.Constants.ConditionDamageSpreadToContentsOfDestroyedContainers);        

            replenishItems.Destroy();
        }

        public bool IsReplenishing(EntityID entityID)
        {
            return Contains(entityID);
        }
       

        private void RemoveOutdatedItem(AmmoOfType ammoOfType, EntityID entityID)
        {
            ammoOfType.Items.Remove(entityID);

            ammoOfType.TotalIsDirty = true;

        }

        public override void IterateContained(Action<Entity> iterateMethod) //Container.IterateMethod iterateMethod)
        {
            replenishItems.IterateContained(iterateMethod);
        }


        public override void ResetPlaySiteRegulators()
        {
            replenishItems.ResetPlaySiteRegulators();
        }
       

        public override List<Entity> GetContainedItemsList(Predicate<Entity> rule)
        {
            List<Entity> items = new List<Entity>();

            replenishItems.GetContainedItemsList(rule, items);

            return items;
        }

       


        public override bool Contains(EntityID entityID)
        {
            return replenishItems.Contains(entityID);         
        }


        public override void SwitchEntities(Entity itemToRemove, Entity exchangeWithItem, bool ignoreCapacity = false)
        {            
            if (Remove(itemToRemove, null))
            {
                AddToContain(exchangeWithItem, null, ignoreCapacity: true);
            }
        }


        protected override bool RemoveFromContain(Entity entity, List<PassengerOrCargoSlot> slots)
        {

            bool wasRemoved = replenishItems.Remove(entity.EntityID);

            return true;

        }


        /// <summary>
        /// TODO: See that ignore capacity is supported.
        /// replenishItems.Add(entity.ID); seems to add to list without looking at capacity.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="compartment"></param>
        /// <param name="placeInStorage"></param>
        /// <param name="slotsToUse"></param>
        /// <param name="ignoreCapacity"></param>
        /// <returns></returns>
        protected override bool AddToContainList(Entity entity, StorageCompartment? compartment = null, StorageCondition placeInStorage = null, List<PassengerOrCargoSlot> slotsToUse = null,
            bool ignoreCapacity = false,
            bool replenish = false,
            bool isProductionOutput = false,
            UpgradeCategory upgradeCategory = null) //bool isUpgrade = false)
        {
            replenishItems.Add(entity.EntityID);
           
            return true;
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
          
            this.replenishItems = (ReplenishItems)sn.DoISnapshot(replenishItems);
                       

            return this;
        }

        public override void LoadPostProcess(Snapshotter sn)
        {
            base.LoadPostProcess(sn);

            replenishItems.LoadPostProcess(sn);
        }



        #endregion
    }


}
