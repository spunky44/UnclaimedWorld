using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Xclna.Xna.Animation;
using UWGame.ClientSide;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Vehicles;
using System.Diagnostics;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Combat;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.AI;

namespace UWGame.SimSide.Entities.Containers.Components
{
    /// <summary>
    /// used to identify different ItemStorage instances in a container,
    /// The ItemStorage containers can be further broken down by Condition
    /// </summary>
    public enum StorageCompartment { Haul, Equipment, Stomach, NormalStorage, ProductionOutput, OfferedForTrade }

    /// <summary>
    /// for inventories on intelligent agents like people and robots that manage their own inventories (so not pack animals)
    /// </summary>
    public class AgentStorage : Container, IStorage, IIDEventSubscriber //, IReplenishes - make Robot class with IReplenishes??? then factor out MountedWeaponOrTool
    {
       
        // these two compartments have independent capacity. This is so that the AI code can still hope to be understood by mortals...

      //  public Dictionary<PlaceInCompartment, ItemStorage> Compartments;

        /// <summary>
        /// only for tools or hauled items, not for extra equipment like weapons and gadgets (except if they're hauled of course)
        /// tools go in the hauling storage because an agent will never haul and go to a process job at the same time.
        /// 
        /// Also for replenish items.
        /// 
        /// NEW: total bulk in this compartment triggers the hauling anim. 
        /// 
        /// I imagine critters will use this storage too when carrying items
        /// </summary>
        public ItemStorage ItemStorage;
      

      
        /// <summary>
        /// this is for weapons and equipment that the agent is allowed to have without affecting his tool or hauling capacity.
        /// 
        /// Can be null.
        /// </summary>
        public ItemStorage Equipment; 

        /// <summary>
        /// the stomach works differently than the other storage types. Items are destroyed right after they are placed inside, and the nutrients absorbed. 
        /// Then BioEntity has a counter StomachContents that simulates the stomach slowly becoming empty...
        /// 
        /// </summary>
        public ItemStorage Stomach; // new..


        MethodID? parentBulkChangedID;

        private EntityID? mountedToolOrWeapon;

        /// <summary>
        /// The item is in my hand, so I can use it -- this will be one of the items in the ItemStorage -- it does not come out of item storage 
        /// </summary>
        public EntityID? MountedToolOrWeapon
        {
            get
            {
                return mountedToolOrWeapon;
            }
            set
            {
                if (!Parent.EntityType.IntelligenceType.CanMountToolsOrWeapons())
                {
                    return;
                }

                if (mountedToolOrWeapon != value)
                {

                    Debug.Assert(value == null || Contains(value.Value), "Mounted items must be carried first!");
                    if (value != null && !Contains(value.Value)) // NEW: don't mount if not carried (should check for this first)...
                    {
                        
                        return;
                    }

                    Entity entityToMount = null;
                    if (value.HasValue)
                    {
                        entityToMount = Entity.FindByID(value.Value);                        
                    }

                    Entity entityMountedNow = null;
                    if (mountedToolOrWeapon.HasValue)
                    {
                        entityMountedNow = Entity.FindByID(mountedToolOrWeapon.Value);
                    }

                    // the attached renderables should be independent of anim states.
                    
                    // set or switch attached object:
                    if (Parent.Renderable != null)
                    {
                        if (entityMountedNow != null)
                        {
                            // clear previous animstates
                            Parent.Renderable.ClearAnimationStateFlags(entityMountedNow.EntityType.ItemType.AnimStatesWhenAttached);
                            
                            // TODO: select left or right hand (bow)

                            // de-attach:
                            Parent.Renderable.RemoveAttachedMountedRenderables(entityMountedNow.EntityType.ItemType.AttachedObjectRenderableType);
                        }

                        if (entityToMount != null)
                        {
                            // attach the model we want to use (we don't use the actual mounted entity, we use another renderable):
                            if (entityToMount.EntityType.ItemType.AttachedObjectRenderableType != null)
                            {
                                Parent.Renderable.AttachPooledObjectIfPossible(
                                    entityToMount.EntityType.ItemType.AttachedObjectRenderableType,
                                    entityToMount.EntityType.ItemType.AttachorTagToMountOn,
                                    AttacheePoint.RightHand,
                                    true);
                            }

                            // set any anim states on the holder of the item:
                            Parent.Renderable.SetAnimationStateFlags(entityToMount.EntityType.ItemType.AnimStatesWhenAttached);

                        }
                    }

                    mountedToolOrWeapon = value;

                    // end effects
                    if (entityMountedNow != null)
                    {
                        EndEffects(entityMountedNow);
                    }

                    // start effects
                    if (entityToMount != null)
                    {
                        StartEffects(entityToMount);
                    }

                }
            }
        }

        public bool isHaulingIsDirty = true;


        public AgentStorage()         
        {
            Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
     
        }

        public AgentStorage(Entity parent): base(parent)
        {
            AgentStorageType agentStorageType = (AgentStorageType)parent.EntityType.ContainerType;
            ItemStorage = new ItemStorage(parent, true, agentStorageType.ItemStorageType);


            if (agentStorageType.EquipmentStorageType != null)
            {
                Equipment = new ItemStorage(parent, true, agentStorageType.EquipmentStorageType);              
            }

            if (agentStorageType.StomachStorageType != null)
            {
                // stomach capacity depends on creature's bulk:
                Stomach = new ItemStorage(parent, false, agentStorageType.StomachStorageType);   
            
               // parent.BulkChanged += new Entity.BulkChangedHandler(parent_BulkChanged);

                parent.BulkChangedEvent.AddAndRegister(parent_BulkChanged,
                    this, out parentBulkChangedID);
            }
            
            // not sure if this should be a validation...
            Debug.Assert(parent.EntityType.BiologicalType == null
                || parent.EntityType.BiologicalType.StomachSizeFractionOfEntityBulk.HasValue, "Forgot to specify StomachStorageType??");
            

         /*   if (parent.EntityType.BiologicalType != null
                && parent.EntityType.BiologicalType.StomachSizeFractionOfEntityBulk.HasValue) // .ContainerType.AgentStorageType.StomachStorageType != null)
            {
                // stomach capacity depends on creature's bulk:
                Stomach = new Entities.ItemStorage(parent, false, null);

                parent.BulkChanged += new Entity.BulkChangedHandler(parent_BulkChanged);
            }*/

        }


        public Storage FindStorage(StorageID storageID)
        {
            Storage foundStorage = ItemStorage.FindStorage(storageID);

            if (foundStorage == null)
            {
                foundStorage = Equipment.FindStorage(storageID);
            }

            if (foundStorage == null)
            {
                foundStorage = Stomach.FindStorage(storageID);
            }

            return foundStorage;
        }



        public void NotifyStomachFractionChanged()//Bso
        {
            Stomach.TotalCapacity = Parent.BiologicalEntity.GetTotalStomachCapacity();
            Stomach.StorageSpaces[GameData.Instance.AllStorageConditions["isolated"]].TotalCapacity = Stomach.TotalCapacity;
        }

        void parent_BulkChanged(float oldValue)
        {
            NotifyStomachFractionChanged();
        }

        private bool isHauling = false;
        public bool IsHauling
        {
            get
            {
                if (isHaulingIsDirty)
                {
                    isHauling = RecomputeIsHauling();

                    isHaulingIsDirty = false;
                }

                return isHauling;
            }
        }


        public static AgentStorage.BurdenState GetHaulBurdenState(float newStoredPercentage)
        {
            if (newStoredPercentage > GameData.Instance.Constants.StoredPercentageMeansHeavyHaul)
            {
                return AgentStorage.BurdenState.HaulHeavy;
            }
            else
            {
                return AgentStorage.BurdenState.HaulLight;
            }
        }


        /// <summary>     
        /// If true, sets a haul anim flag and the target speed to HaulSpeed. NOTE: the speed is further reduced according to burden in CalculateSpeed... 
        /// 
        /// only some carried items should set the IsHauling flag...
        ///  
        /// recompute when:
        /// Carried items change
        /// AssignedToJob changes on any carried item
        /// </summary>
        /// <returns></returns>
        public bool RecomputeIsHauling(IKnownEntityData exceptItem = null, IKnownEntityData withItem = null) // EntityID? exceptItem = null, EntityID? withItem = null)
        {
           
            isHauling = false;

            float bulkToUse = TotalStored;
            if (exceptItem != null) // this is used to get the status after dropping an item
            {               
                bulkToUse -= exceptItem.Bulk;               
            }
            else if (withItem != null) // this is used to get the status after adding an item
            {
                bulkToUse += withItem.Bulk;
            }

            // NEW: only true if carrying some percentage.
            // this should make characters move normally more often, and speed up the game
            float newStoredPercentage = GetHaulingPercentageOfCapacity(bulkToUse);

            if (newStoredPercentage < GameData.Instance.Constants.StoredPercentageMeansHauling)
            {
                return isHauling;
            }

            EntityID entityID;
            for (int i = ItemStorage.StoredItems.Count - 1; i >= 0; i--)
            {
                entityID = ItemStorage.StoredItems[i];

                if (exceptItem == null || entityID != exceptItem.EntityID) 
                {
                    Entity itemEntity = Entity.FindByID(entityID);

                    if (itemEntity != null)
                    {
                        if (ItemTriggersHauling(itemEntity))
                        {
                            isHauling = true;
                            break;
                        }

                     /*   Job assignedToJob = EvaluateJob.ResolveAssignedToJob(itemEntity);

                        if (assignedToJob != null
                            && (assignedToJob is HaulingJob  // HaulingJob does not match replenish items (firewood)!
                              || (assignedToJob is ProcessJob && !itemEntity.EntityType.IsMountable())) // not hand tools for gathering, find prey job, patrol etc.
                            && assignedToJob.TakenBy.Contains(parent))
                        {

                            isHauling = true;
                            break;
                        }*/
                    }
                    else
                    {
                        ItemStorage.RemoveOutdatedItem(entityID);

                        /*
                        ItemStorage.StoredItems.RemoveAt(i);
                        totalStoredIsDirty = true;*/
                    }
                }
            }


            if (withItem != null)
            {
                if (ItemTriggersHauling(withItem))
                {
                    isHauling = true;                   
                }
            }

            return isHauling;
        }

        /// <summary>
        /// only some carried items should set the IsHauling flag...
        /// </summary>
        /// <returns></returns>
        private bool ItemTriggersHauling(IKnownEntityData item)
        {
            Job assignedToJob = EvaluateJob.ResolveAssignedToJob(item);

            if (assignedToJob != null
                && (assignedToJob is HaulingJob  // HaulingJob does not match replenish items (firewood)!
                  || (assignedToJob is ProcessJob && !item.EntityType.IsMountable())) // not hand tools for gathering, find prey job, patrol etc.
                && assignedToJob.TakenBy.Contains(Parent))
            {
                return true;

                //isHauling = true;
               // break;

            }

            return false;
        }

        public StorageCompartment GetCompartment(StorageID storageID)
        {
            if (ItemStorage != null && ItemStorage.HasStorage(storageID))
            {
                return StorageCompartment.Haul;
            }
            else if (Equipment != null && Equipment.HasStorage(storageID))
            {
                return StorageCompartment.Equipment;
            }
            else
            {
                return StorageCompartment.Stomach;
            }
        }


        public enum BurdenState { None, Mounted, Equipped, HaulLight, HaulHeavy }

        public BurdenState GetCurrentBurdenState()
        {
            if (IsHauling && MountedToolOrWeapon == null)
            {
                return GetHaulBurdenState(GetHaulingPercentageOfCapacity()); 

               /* if (GetHaulingPercentageOfCapacity() < GameData.Instance.Constants.StoredPercentageMeansHeavyHaul)
                {
                    return BurdenState.HaulLight;
                }
                else
                {
                    return BurdenState.HaulHeavy;
                }*/
            }
            else
            {
                if (MountedToolOrWeapon.HasValue)
                {
                    return BurdenState.Mounted;
                }
                else if (Equipment != null && Equipment.TotalStored > 0f)
                {
                    return BurdenState.Equipped; //?
                }

                return BurdenState.None;
            }

        }

        public float GetMountedWeaponRange(out float? coneWidth, out float? coneLength)
        {
            coneWidth = null;
            coneLength = null;
            if (MountedToolOrWeapon.HasValue)
            {
                Entity weapon = Entity.FindByID(MountedToolOrWeapon.Value);
                if (weapon != null)
                {
                    float maxRange = weapon.GetWeaponRange(ref coneWidth, ref coneLength);

                    return maxRange;
                }               
            }

            return 0f;
        }



        public void IterateContained(StorageCompartment compartment, Action<Entity> del) //Container.IterateMethod del)
        {
            switch (compartment)
            {
                case StorageCompartment.Haul:
                    ItemStorage.IterateContained(del);
                    break;
                case StorageCompartment.Equipment:
                    Equipment.IterateContained(del);
                    break;
                case StorageCompartment.Stomach:
                    Stomach.IterateContained(del);
                    break;
            }
        }

        public override void IterateContained(Action<Entity> del) // Container.IterateMethod del)
        {
            ItemStorage.IterateContained(del);

            if (Equipment != null)
            {
                Equipment.IterateContained(del);
            }

            if (Stomach != null)
            {
                Stomach.IterateContained(del);
            }
        }

        public void IterateContainedBreakOnTrue(IterateBoolMethod iterateMethod)
        {
            if (!ItemStorage.IterateContainedBreakOnTrue(iterateMethod))
            {
                if (Equipment != null)
                {
                    Equipment.IterateContainedBreakOnTrue(iterateMethod);
                }

                if (Stomach != null)
                {
                    Stomach.IterateContainedBreakOnTrue(iterateMethod);
                }
            }

        }

        public override List<Entity> GetContainedItemsList(Predicate<Entity> rule) 
        {
            List<Entity> items = new List<Entity>();

            ItemStorage.GetContainedItemsList(rule, items);

            if (Equipment != null)
            {
                Equipment.GetContainedItemsList(rule, items);
            }

            if (Stomach != null)
            {
                Stomach.GetContainedItemsList(rule, items);
            }

            return items;
        }

        public Storage GetStoredIn(Entity entity)
        {
            Storage storage = ItemStorage.GetStoredIn(entity.EntityID);

            if (storage == null && Equipment != null)
            {
                storage = Equipment.GetStoredIn(entity.EntityID);
            }

            if (storage == null && Stomach != null)
            {
                storage = Stomach.GetStoredIn(entity.EntityID);
            }

            return storage;
        }


        public Dictionary<StorageCondition, Storage> GetStorageSpaces()
        {
            return ItemStorage.StorageSpaces;

        }

        /// <summary>
        /// bulk of currently stored items. equipment items do not affect this
        /// </summary>
        public float TotalStored
        {
            get
            {
                return ItemStorage.TotalStored;
            }
        }

        /// <summary>
        /// a constant value
        /// </summary>
        public float TotalItemStorageCapacity 
        {
            get
            {
                return ItemStorage.TotalCapacity;
            }
        }

       

        public float GetHaulingPercentageOfCapacity(float stored)
        {
            return stored / ItemStorage.TotalCapacity;
        }

        public float GetHaulingPercentageOfCapacity()
        {
            return GetHaulingPercentageOfCapacity(ItemStorage.TotalStored);         
        }

        public ItemStorage GetCompartment(StorageCompartment compartment)
        {
            if (compartment == StorageCompartment.Equipment)
            {
                return Equipment;
            }
            else if (compartment == StorageCompartment.Stomach)
            {
                return Stomach;
            }
            else return ItemStorage; 
        }

        public bool Contains(Entity item)
        {
            return ItemStorage.Contains(item) 
                || (Equipment != null && Equipment.Contains(item))
                || (Stomach != null && Stomach.Contains(item)) 
                || MountedToolOrWeapon == item.EntityID;
        }

        public override bool Contains(EntityID item)
        {
            return ItemStorage.Contains(item) 
                || (Equipment != null && Equipment.Contains(item))
                || (Stomach != null && Stomach.Contains(item))
                || MountedToolOrWeapon == item;
        }


        public override void Destroy()
        {
            // eject all items:
            ItemStorage.UncontainAllEntities();
            ItemStorage.Destroy();

            if (Equipment != null)//Only remove items from an equipment that exists.
            {
                Equipment.UncontainAllEntities();
                Equipment.Destroy();
            }

            if (Stomach != null)
            {
                Stomach.UncontainAllEntities();
                Stomach.Destroy();
            }
        }

        protected override bool RemoveFromContain(Entity entity, List<PassengerOrCargoSlot> slots)
        {
            if (entity.ID == (EntityID)18)
            {

            }


            bool wasRemoved = false;

            wasRemoved = ItemStorage.Remove(entity, true);
            if (!wasRemoved && Equipment != null)
            {
                wasRemoved = Equipment.Remove(entity, true);

                if (wasRemoved)
                {
                    EndEffects(entity); 
                }
            }

            if (!wasRemoved && Stomach != null)
            {
                wasRemoved = Stomach.Remove(entity, true);
            }

            if (MountedToolOrWeapon == entity.EntityID)
            {
                MountedToolOrWeapon = null;
            }

            //  base.Remove(entity);

            if (wasRemoved)
            {
                if (Parent.Locomotor != null)
                {
                    Parent.Locomotor.CurrentMaximumSpeedIsDirty = true;
                    Parent.Locomotor.CurrentMaximumSpeedNotAffectedByTerrainIsDirty = true;
                    // must not affect evaluator speed.
                }

                isHaulingIsDirty = true;

                //RecalculateAndSetCurrentSpeed();

                HandleAttachedBoxAnimation();
            }

            return true;

        }


        private void HandleAttachedBoxAnimation()
        {

            AgentStorage.BurdenState currentBurdenState = GetCurrentBurdenState();

          /*  if (currentBurdenState != BurdenState.HaulHeavy)
            {*/
                Parent.Renderable.UpdateBurdenState(currentBurdenState); // RemoveHauledObjects();

           // }
        }



        /// <summary>
        /// Haul is the default compartment
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
            if (entity.ID == (EntityID)18)
            {

            }

            if (compartment == null || compartment.Value == StorageCompartment.Haul)
            {
                bool wasAdded = ItemStorage.Add(entity, placeInStorage, ignoreCapacity);

                if (wasAdded)
                {
                    isHaulingIsDirty = true;

                    if (Parent.Locomotor != null)
                    {
                        Parent.Locomotor.CurrentMaximumSpeedIsDirty = true;
                        Parent.Locomotor.CurrentMaximumSpeedNotAffectedByTerrainIsDirty = true;
                        // must not affect evaluator speed.
                    }
                }

                return wasAdded;
            }
            else if (compartment.Value == StorageCompartment.Equipment)
            {
                if (Equipment.Add(entity, placeInStorage, ignoreCapacity))
                {
                    // start effects
                    StartEffects(entity);

                    return true;
                }
                else return false;
            }
            else if (Stomach != null && compartment.Value == StorageCompartment.Stomach)
            {
                if (Stomach.Add(entity, placeInStorage, ignoreCapacity))
                {
                    Parent.BiologicalEntity.AddToStomachContents(entity.Bulk);
                   
                    return true;
                }
                else return false;
            }

            return false;
        }

        private void StartEffects(Entity item)
        {
            if (item.EntityType.ItemType.FinalEffectsWhenEquipped != null)
            {
                foreach (var effect in item.EntityType.ItemType.FinalEffectsWhenEquipped)
                {
                    this.Parent.SimEffects.Start(effect);
                }
            }
           
        }

        private void EndEffects(Entity item)
        {
            if (item.EntityType.ItemType.FinalEffectsWhenEquipped != null)
            {
                foreach (var effect in item.EntityType.ItemType.FinalEffectsWhenEquipped)
                {
                    this.Parent.SimEffects.Remove(effect);
                }
            }
        }

        public void EndEffectsFromEquippedItem(Entity entity)
        {
            if (Equipment != null && Equipment.Contains(entity.EntityID))
            {
                EndEffects(entity);
            }
        }


        public override void NotifyBrokenContainedEntity(Entity entity)
        {
            EndEffectsFromEquippedItem(entity);
        }
    

        public override void SwitchEntities(Entity itemToRemove, Entity exchangeWithItem, bool ignoreCapacity = false)
        {
            if (MountedToolOrWeapon == itemToRemove.EntityID)
            {
                MountedToolOrWeapon = null; // exchangeWithItem.ID; ??
            }

            if (Equipment != null && Equipment.Contains(itemToRemove.EntityID))
            {
                StorageCondition conditions = Equipment.GetStorageConditions(itemToRemove.EntityID);

                if (Remove(itemToRemove, null))
                {
                    EndEffects(itemToRemove);
 
                    AddToContain(exchangeWithItem, StorageCompartment.Equipment, conditions, ignoreCapacity: true);
                }

               // Equipment.SwitchEntities(itemToRemove, exchangeWithItem);
            }
            else if (ItemStorage.Contains(itemToRemove.EntityID))
            {
                StorageCondition conditions = ItemStorage.GetStorageConditions(itemToRemove.EntityID);

                if (Remove(itemToRemove, null))
                {
                    AddToContain(exchangeWithItem, StorageCompartment.Haul, conditions, ignoreCapacity: true);
                }

                //ItemStorage.SwitchEntities(itemToRemove, exchangeWithItem);
            }
            else if (Stomach != null && Stomach.Contains(itemToRemove.EntityID))
            {
                StorageCondition conditions = Stomach.GetStorageConditions(itemToRemove.EntityID);

                if (Remove(itemToRemove, null))
                {
                    AddToContain(exchangeWithItem, StorageCompartment.Stomach, conditions, ignoreCapacity: true);
                }

                //ItemStorage.SwitchEntities(itemToRemove, exchangeWithItem);
            }  
        }

       

    /*    public override void EjectEntity(Entity item, Entity placeInStorageEntity, Storage.Conditions? placeInStorage)
        {
          //  Remove(item);

            if (MountedToolOrWeapon == item.ID)
            {
                Entity mountedEntity = Entity.FindByID(MountedToolOrWeapon.Value);
                if (mountedEntity != null)
                {
                    mountedEntity.Item.DropByEntity(parent, placeInStorageEntity, placeInStorage);
                }

                MountedToolOrWeapon = null;
            }
            else if (!ItemStorage.DropCarriedItem(item, placeInStorageEntity, placeInStorage))
            {
                Equipment.DropCarriedItem(item, placeInStorageEntity, placeInStorage);
            }
        }*/

        /// <summary>
        /// use this to shift items from equipment storage to hauled/tools storage and vice versa
        /// </summary>
        /// <param name="item"></param>
        /// <param name="toCompartment"></param>
        public void MoveCarriedItemToCompartment(Entity item, StorageCompartment toCompartment)
        {
            Debug.Assert(item.ContainedBy == Parent.EntityID, "Illegal move, item must be carried.");
           

            if (toCompartment == StorageCompartment.Haul)
            {
                if (Equipment.Contains(item))
                {
                    Equipment.Remove(item, true);
                }

                if (!ItemStorage.Contains(item))
                {
                    ItemStorage.Add(item, null);
                }
            }
            else 
            {
                if (ItemStorage.Contains(item))
                {
                    ItemStorage.Remove(item, true);
                }

                if (!Equipment.Contains(item))
                {
                    Equipment.Add(item, null);
                }
            }           

        }


        public float GetFreeStomachCapacity()
        {
            if (Stomach != null)
            {
                return (1f - Parent.BiologicalEntity.StomachContents) * Stomach.TotalCapacity; // GetTotalStomachCapacity();
            }

            return 0f;
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

            this.Equipment = (ItemStorage)sn.DoISnapshot(Equipment);
            this.isHauling = sn.DoBool(isHauling);
            this.isHaulingIsDirty = sn.DoBool(isHaulingIsDirty);
            this.ItemStorage = (ItemStorage)sn.DoISnapshot(ItemStorage);
            this.mountedToolOrWeapon = sn.DoEntityIDNullable(mountedToolOrWeapon);
            this.parentBulkChangedID = sn.DoMethodIDNullable(parentBulkChangedID);
            this.Stomach = (ItemStorage)sn.DoISnapshot(Stomach);
           
            return this;
        }

        public override void LoadPostProcess(Snapshotter sn)
        {
            base.LoadPostProcess(sn);

            LoadPostProcessRegisterMethodIDs();

            this.ItemStorage.LoadPostProcess(sn);

            if (this.Stomach != null)
            {
                this.Stomach.LoadPostProcess(sn);
            }

            if (this.Equipment != null)
            {
                this.Equipment.LoadPostProcess(sn);
            }

        }

        public void LoadPostProcessRegisterMethodIDs()
        {
            if (parentBulkChangedID.HasValue)
            {
                ActionLookup<float>.Add(parentBulkChangedID.Value, parent_BulkChanged);
            }
            
        }

        #endregion

        
    }
}
