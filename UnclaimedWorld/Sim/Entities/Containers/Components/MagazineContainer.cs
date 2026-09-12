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
    /// the MagazineContainer should maybe not merge with ReplenishContainer. It contains code to deal with ammo items - these are kind of special because an ammo item can have more than one arrow/bullet in it...
    /// </summary>
    class MagazineContainer: Container, IReplenishes
    {
        
        /// <summary>
        /// unspent ammo - prevent other ágents from taking them again by setting the property Replenishes on the Item!
        /// each ammo item can hold several rounds!
        /// this dictionary maintains the list of ammo items as well as the total rounds.
        /// </summary>
        private Dictionary<EntityType, AmmoOfType> AmmoItems = new Dictionary<EntityType, AmmoOfType>();

        /// <summary>
        /// what to do with degraded ammunition? perhaps jam the weapon before they are removed... perhaps a reload should always clear this list
        /// </summary>
        private List<EntityID> DegradedItems = new List<EntityID>();


       

        public MagazineContainer(Entity parent)
            : base(parent)
        {

        }

        public MagazineContainer()         
        {
            Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");     
        }


        public override void Destroy()
        {
            // drop all items:
            /* for (int i = itemstor - 1; i >= 0; i--)
             {
                
             }*/

            // eject all items:
            UncontainAllEntities(0.4f, 0.3f);
            
        }

        public bool IsReplenishing(EntityID entityID)
        {
            return Contains(entityID) && !DegradedItems.Contains(entityID);
        }

        /// <summary>
        /// ejects all stored entities (items) and may apply condition damage to them
        /// </summary>
        /// <param name="damageStandardDev"></param>
        /// <param name="damageSpread"></param>
        public void UncontainAllEntities(float? damageStandardDev = null, float? damageSpread = null) //float? chanceToDestroy = null)
        {
            EntityID entityID;
            Entity entity;

            foreach (var item in AmmoItems)
            {
                for (int i = item.Value.Items.Count - 1; i >= 0; i--)
                {
                    entityID = item.Value.Items[i];

                    entity = Entity.FindByID(entityID);

                    if (entity != null)
                    {
                        if (Parent.Contains.Remove(entity))
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
                        RemoveOutdatedItem(item.Value, entityID);
                    }
                }
              
            }           
        }

        private void RemoveOutdatedItem(AmmoOfType ammoOfType, EntityID entityID)
        {
            ammoOfType.Items.Remove(entityID);

            ammoOfType.TotalIsDirty = true;

        }

        public override void IterateContained(Action<Entity> iterateMethod) // Container.IterateMethod iterateMethod)
        {
            EntityID entityID;
            Entity entityInside;
            foreach (var ammoItems in AmmoItems)
            {
                for (int i = ammoItems.Value.Items.Count - 1; i >= 0; i--)
                {
                    entityID = ammoItems.Value.Items[i];

                    entityInside = Entity.FindByID(entityID);
                    if (entityInside != null)
                    {
                        iterateMethod(entityInside);
                    }
                    else
                    {
                        RemoveOutdatedItem(ammoItems.Value, entityID);                       
                    }
                }
            }

            for (int i = DegradedItems.Count - 1; i >= 0; i--)
            {
                entityID = DegradedItems[i];

                entityInside = Entity.FindByID(entityID);
                if (entityInside != null)
                {
                    iterateMethod(entityInside);
                }
                else
                {
                    DegradedItems.RemoveAt(i);
                }
            }

        }

        public override List<Entity> GetContainedItemsList(Predicate<Entity> rule) 
        {
            List<Entity> items = new List<Entity>();

            EntityID entityID;
            Entity entityInside;
            foreach (var ammoItems in AmmoItems)
            {
                for (int i = ammoItems.Value.Items.Count - 1; i >= 0; i--)
                {
                    entityID = ammoItems.Value.Items[i];

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
                        RemoveOutdatedItem(ammoItems.Value, entityID);
                    }
                }
            }

            for (int i = DegradedItems.Count - 1; i >= 0; i--)
            {
                entityID = DegradedItems[i];

                entityInside = Entity.FindByID(entityID);
                if (entityInside != null)
                {
                    if (rule(entityInside))
                    {
                        items.Add(entityInside);
                    }
                }
                else
                {
                    DegradedItems.RemoveAt(i);
                }
            }

            return items;
        }

        protected override bool RemoveFromContain(Entity entity, List<PassengerOrCargoSlot> slots)
        {

            bool wasRemoved = false;

            AmmoOfType ammoOfType;

            if (AmmoItems.TryGetValue(entity.EntityType, out ammoOfType))
            {

                wasRemoved = ammoOfType.Items.Remove(entity.EntityID);
                ammoOfType.TotalIsDirty = true;

            }
            else
            {
                wasRemoved = DegradedItems.Remove(entity.EntityID);
            }

            return true;

        }


        public override bool Contains(EntityID entityID)
        {
            foreach (var ammo in AmmoItems)
            {
                foreach (var item in ammo.Value.Items)
                {
                    if (item == entityID)
                    {
                        return true;
                    }
                }
            }

            return DegradedItems.Contains(entityID);          
        }

        public override void SwitchEntities(Entity itemToRemove, Entity exchangeWithItem, bool ignoreCapacity = false)
        {
            AmmoOfType ammo;
            if (AmmoItems.TryGetValue(itemToRemove.EntityType, out ammo))
            {
                ammo.Items.Remove(itemToRemove.EntityID);
                //itemToRemove.ContainedBy = null;

                ammo.TotalIsDirty = true;
            }

            DegradedItems.Add(exchangeWithItem.EntityID); // ??

            exchangeWithItem.ContainedBy = Parent.EntityID;

        }

        /// <summary>
        /// see if we have the rounds needed. More than one round can be used, like for a burst shot
        /// </summary>
        /// <param name="ammoType"></param>
        /// <param name="noOfRounds"></param>
        /// <returns></returns>
        public bool HasAmmo(EntityType ammoType, int noOfRounds)
        {
            AmmoOfType items;
            if (AmmoItems.TryGetValue(ammoType, out items))
            {
                return items.TotalRounds >= noOfRounds;

            }

            return false;

        }

        public int GetTotalAmmo()
        {
            return AmmoItems.Sum(a => a.Value.TotalRounds);
        }


        public void GetAmmoStatus(ref Dictionary<EntityType, int> ammo)
        {
            ammo = new Dictionary<EntityType, int>();
            foreach (var item in AmmoItems)
            {
                ammo.Add(item.Key, item.Value.TotalRounds);
            }

        }

        /// <summary>
        /// Never implement this!
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="compartment"></param>
        /// <param name="placeInStorage"></param>
        /// <returns></returns>
        protected override bool AddToContainList(Entity entity, StorageCompartment? compartment = null, StorageCondition placeInStorage = null, List<PassengerOrCargoSlot> slotsToUse = null, bool ignoreCapacity = false,
            bool replenish = false,
            bool isProductionOutput = false, 
            UpgradeCategory upgradeCategory = null) //bool isUpgrade = false)
        {
            throw new NotImplementedException();
        }

        protected override bool AddToContainList(Entity entityToReplenishWith, out Entity exceedingAmmoItem, StorageCompartment? compartment = null, StorageCondition placeInStorage = null)
        {
            AmmoOfType items;
            if (!AmmoItems.TryGetValue(entityToReplenishWith.EntityType, out items))
            {
                items = new AmmoOfType() { Items = new List<EntityID>() };

                AmmoItems.Add(entityToReplenishWith.EntityType, items);
            }

            // check for exeeding max capacity:
            MagazineContainerType magazineContainerType = (MagazineContainerType)Parent.EntityType.ContainerType;
            exceedingAmmoItem = null;
            int surplus = items.TotalRounds + entityToReplenishWith.Item.Ammunition.NoOfRounds - magazineContainerType.MaxCapacity;
            if (surplus > 0)
            {
                // split the ammo item if necessary:
                entityToReplenishWith.Item.Ammunition.NoOfRounds = magazineContainerType.MaxCapacity - items.TotalRounds;

                // create a surplus item:
                exceedingAmmoItem = new Entity(entityToReplenishWith.EntityType);
                exceedingAmmoItem.Initialize(entityToReplenishWith.Site);  //The.Sim.PlaySite);
                exceedingAmmoItem.InitializeModelAndOnScreenFunctionality();
                exceedingAmmoItem.Item.Ammunition.NoOfRounds = surplus;
                // exceedingAmmoItem.ChangeOwnership(entityToReplenishWith.Owner); //cannot be done before we set MapPosition (until we implement containers?)

            }


            // see if we can merge ammo items...
            if (!MergeAmmoItems(entityToReplenishWith, items.Items))
            {
                entityToReplenishWith.Item.Replenishes = Parent.EntityID; // set a lock
                items.Items.Add(entityToReplenishWith.EntityID);
            }

            items.RecomputeTotalRounds();

            EntityID entityID;
            Entity degradedItem;

            // let's clear out any degraded bullets now that the agent is doing a reload anyway:
            for (int i = DegradedItems.Count - 1; i >= 0; i--)
            {
                entityID = DegradedItems[i];
                degradedItem = Entity.FindByID(entityID);
                if (degradedItem != null)
                {
                    Parent.Contains.Uncontain(degradedItem);                   
                }

               // DegradedItems.RemoveAt(i);
            }

            return true; 
        }

        /// <summary>
        /// returns the remaining ammo item if capacity would otherwise be exceeded (an ammo item can have multiple rounds)
        /// </summary>
        /// <param name="entityToReplenishWith"></param>
        /// <returns></returns>
   /*     public Entity Reload(Entity entityToReplenishWith)
        {

            AmmoOfType items;
            if (!AmmoItems.TryGetValue(entityToReplenishWith.EntityType, out items))
            {
                items = new AmmoOfType() { Items = new List<EntityID>() };

                AmmoItems.Add(entityToReplenishWith.EntityType, items);
            }

            // check for exeeding max capacity:
            Entity exceedingAmmoItem = null;
            int surplus = items.TotalRounds + entityToReplenishWith.Item.Ammunition.NoOfRounds - parent.EntityType.MagazineType.MaxCapacity;
            if (surplus > 0)
            {
                // split the ammo item if necessary:
                entityToReplenishWith.Item.Ammunition.NoOfRounds = parent.EntityType.MagazineType.MaxCapacity - items.TotalRounds;

                // create a surplus item:
                exceedingAmmoItem = Entity.Produce(entityToReplenishWith.EntityType);
                exceedingAmmoItem.Initialize(The.Sim.Site);
                exceedingAmmoItem.InitializeModelAndOnScreenFunctionality(The.Sim.ScreenManager.Game);
                exceedingAmmoItem.Item.Ammunition.NoOfRounds = surplus;
                // exceedingAmmoItem.ChangeOwnership(entityToReplenishWith.Owner); //cannot be done before we set MapPosition (until we implement containers?)

            }


            // see if we can merge ammo items...
            if (!MergeAmmoItems(entityToReplenishWith, items.Items))
            {
                entityToReplenishWith.Item.Replenishes = parent.ID; // set a lock
                items.Items.Add(entityToReplenishWith.ID);
            }

            items.RecomputeTotalRounds();


            return exceedingAmmoItem;
        }*/




        private bool MergeAmmoItems(Entity entityToReplenishWith, List<EntityID> existingItems)
        {
            Ammunition existingAmmo;
            Ammunition replenishAmmo = entityToReplenishWith.Item.Ammunition;
            int maxRoundsInItem = entityToReplenishWith.EntityType.ItemType.AmmunitionType.MaxNoOfRounds;

            for (int i = existingItems.Count - 1; i >= 0; i--)
            {
                Entity existingEntity = Entity.FindByID(existingItems[i]);

                if (existingEntity != null)
                {
                    Item existingItem = existingEntity.Item;
                    existingAmmo = existingEntity.Item.Ammunition;
                    if (existingAmmo.NoOfRounds + replenishAmmo.NoOfRounds <= maxRoundsInItem)
                    {
                        //let's merge the items:
                        existingAmmo.NoOfRounds += replenishAmmo.NoOfRounds;

                        existingItem.MergeItems(entityToReplenishWith);

                        return true;

                    }
                }
                else
                {
                    // remove the invalid item:
                    existingItems.RemoveAt(i);
                }

            }

            return false;

        }


        /// <summary>
        /// returns the number of rounds we actually were able to spend
        /// </summary>
        /// <param name="typeOfAmmo"></param>
        /// <param name="noOfRounds"></param>
        /// <returns></returns>
        public int SpendAmmo(EntityType typeOfAmmo, int noOfRounds, EntityGroup owner)
        {
            // bool success = false;
            AmmoOfType ammoOfType;
            int totalRoundsToSpend = noOfRounds;

            if (AmmoItems.TryGetValue(typeOfAmmo, out ammoOfType))
            {

                Entity ammoEntity;
                Item ammoItem;
                
                for (int i = ammoOfType.Items.Count - 1; i >= 0; i--)
                {
                    int noOfRoundsToSpendFromItem;

                    ammoEntity = Entity.FindByID(ammoOfType.Items[i]);

                    if (ammoEntity != null)
                    {
                        ammoItem = ammoEntity.Item;

                        // spend as much as possible from this item:
                        noOfRoundsToSpendFromItem = Math.Min(ammoItem.Ammunition.NoOfRounds, totalRoundsToSpend);

                        ammoItem.Ammunition.NoOfRounds = ammoItem.Ammunition.NoOfRounds - noOfRoundsToSpendFromItem;

                        totalRoundsToSpend -= noOfRoundsToSpendFromItem;

                        // delete the item if no rounds left:
                        if (ammoItem.Ammunition.NoOfRounds <= 0)
                        {
                            ammoEntity.Destroy();

                          //  ammoOfType.Items.RemoveAt(i);
                        }

                        if (totalRoundsToSpend <= 0)
                        {
                            // done.
                            // success = true;
                            break;
                        }
                    }
                    else
                    {
                        // invalid item found:
                        this.RemoveOutdatedItem(ammoOfType, ammoOfType.Items[i]);
                        //ammoOfType.Items.RemoveAt(i);
                    }
                }

            }

            if (ammoOfType != null)
            {
                ammoOfType.RecomputeTotalRounds();
            }

            owner.SetAmmoDirty(typeOfAmmo);


            return noOfRounds - totalRoundsToSpend;

            //return success;
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

            this.AmmoItems = sn.DoDictionary(AmmoItems);
            this.DegradedItems = sn.DoList(DegradedItems);
          
            return this;
        }

        public override void LoadPostProcess(Snapshotter sn)
        {
            base.LoadPostProcess(sn);


        }



        #endregion

    }


    public class AmmoOfType: ISnapshot
    {
        public List<EntityID> Items;


        private int totalRounds = 0;

        /// <summary>
        /// tells whether the total needs recomputing - may also be set from outside...
        /// </summary>
        public bool TotalIsDirty = true;


        /// <summary>
        /// a convenience sum of the total rounds contained in the list of items
        /// </summary>
        public int TotalRounds
        {
            get
            {
                if (TotalIsDirty)
                {
                    RecomputeTotalRounds();
                }

                return totalRounds;
            }
        }

      

        public void RecomputeTotalRounds()
        {
            int total = 0;
            for (int i = Items.Count - 1; i >= 0; i--)
            {
                Entity ammoEntity = Entity.FindByID(Items[i]);

                if (ammoEntity != null)
                {
                    total += ammoEntity.Item.Ammunition.NoOfRounds;
                }
                else
                {
                    Items.RemoveAt(i);
                }
            }

            totalRounds = total;
            TotalIsDirty = false;
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
            this.Items = sn.DoList(Items);
            this.totalRounds = sn.DoInt32(totalRounds);
            this.TotalIsDirty = sn.DoBool(TotalIsDirty);

            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            

        }



        #endregion


    }
}
