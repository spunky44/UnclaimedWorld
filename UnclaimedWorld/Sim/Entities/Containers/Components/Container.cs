using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Body;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Vehicles;
using UWGame.SimSide.Items;
using UWGame.ClientSide.Log;
using UWGame.SimSide.Snapshots;
using System.Diagnostics;
using UWGame.SimSide.Combat;
using UWGame.SimSide.Allegiances;

namespace UWGame.SimSide.Entities.Containers.Components
{

   
    [Flags]
    public enum ContainedEntityStatus
    {
        None = 0x0,
        Entering = 0x1,
        Exiting = 0x2,
        Contained = 0x4,
        Passenger = 0x8,
        GettingHealed = 0x16,
        EatingBirthdayCake = 0x32,
    }


    public enum EnterExitType
    {
        PickMeUp,
        GoToRendezvousPoint,
        HopOnBoard,
        OKImOn,
        GetOff
    };

    /*
IGarrison and IStorage interfaces, which the rest of the game classes use to relate to a Container component in different ways.
[02:29:41] Mark Lorenzen: Container as IGarrison
[02:29:52] Mark Lorenzen: Container as IEquipment
[02:30:05] Mark Lorenzen: Container as IStorage
[02:31:26] Mark Lorenzen: each of these interfaces would allow other classes to enter, exit, store, remember, inhabit, transport, crew... the container, if the interface is present.
[02:31:50] Mark Lorenzen: in the case of an encampment building...
[02:32:01] Mark Lorenzen: it would need to do at least two things:
[02:32:12] Mark Lorenzen: 1) be a place to leave your stuff
[02:32:34] Mark Lorenzen: 2) be a place to go into and do homey stuff like sleep
[02:32:59] Mark Lorenzen: so write a class ContainerHome
[02:33:19] Mark Lorenzen: inherit IStorage and IGarrison
[02:33:57] Mark Lorenzen: IStorage exposes the CombinedStorage style functions.
[02:34:19] Mark Lorenzen: IGarrison would be new, exposing the functions for people entering and Exiting
[02:34:42] Mark Lorenzen: also inherit IExit, which handles the physical transitioning into and out of the building.     
     
other buildings might be IGarrison but not IStorage... like a watchtower
[02:35:45] Mark Lorenzen: or the other way around... a pup tent is a home but not a storage
[02:36:16] Mark Lorenzen: a vehicle would inherit ITransport and ICrew
[02:36:27] Mark Lorenzen: the crew is the driver
[02:36:40] Mark Lorenzen: passengers and items are contained via ITransport
*/


    /// <summary>
    /// Why not a component..?
    /// 
    /// Containment is the basis for many complex systems, it helps us to have a formal
    /// place where we can monitor the outside world if we need to
    /// 
    /// The interface approach requires us to make a class for each permutation of modules...
    /// </summary>
    public abstract class Container: ISnapshot 
    {
        public Entity Parent;
        private EntityID parentID;

        protected Container()
        {
            Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");     
        }

        public Container(Entity parent)
        {
            this.Parent = parent;
        }


        // Containment Methods==============================================================

        // is this container an AI proxy for the entities it contains?
        public virtual bool isGarrison() { return false; }


        //some containers hide their contained Entities, others show them (vehicle, visitorplace, etc.)
        // contained Entities add/remove their renderable representations entirely from the client
        public virtual bool IsOpenContainer() { return false; }

        public virtual bool IsVisible(Entity entity)
        {
            return false;
        }

        // Parent Entity changed position, orientation... react as appropriate.
        public virtual void ReactToTransformChange() { }


        //this is used for containers that must do something to allow people to enter or exit...
        //eg, land (for aircraft), open door (whatever)... it's called with wants=PickMeUp
        //when something is in the enter state, and wants=OKImOn when the unit has
        //either entered, exited, or given up, etc
        public virtual bool OnEntityWantsToEnterOrExit(Entity entity, EnterExitType wants) { return false; }

        // returns true if there are entitys currently waiting to enter.
        public virtual bool hasentitysWantingToEnterOrExit() { return false; }


        // you will want to override onContaining() and onRemoving() if you need to
        // do special actions at those event times for your module
        public virtual void onContaining(Entity entity, bool wasSelected) { }		///< entity now contains 'entity'
        public virtual void onRemoving(Entity entity) { }			///< entity no longer contains 'entity'

        ///should probably be a damage type, instead, since some damage comes from decay, degrading, age, etc.
        public virtual void onPassengerDamage(Entity entity, Body.Body body, AttackType attack) { }

        // attachement, place finding, etc
      //  protected virtual void setUpRiderRenderable(Entity rider, bool hideRider) { }

        // returns max int to effectively mean, "all are welcome. no limits"
      //  public virtual int getContainMax() { return int.MaxValue; }

        // All the Logic for how to simulate an orderly exit should be in the various passengers' GoalExitVehicle or such
        // these methods are just a way to signal those systems to do their thing
        public virtual void orderAllPassengersToExit() { }
        public virtual void orderOnePassengersToExit(Entity passenger) { }

        /// <summary>
        /// this is called when the container is destroyed.  Override to destroy or eject the content entities. (later, if an entity is encountered which is contained in a destroyed container, the entity will be destroyed as cleanup.)
        /// </summary>
        public abstract void Destroy();
       
        //can this container contain this kind of entity? 
        //and, if checkCapacity is true, does this container have enough space left to hold the given unit?
       // public virtual bool isValidContainerFor(Entity entity, bool checkCapacity, bool checkPath) { return false; }


        /// <summary>
        /// all playsite regulators must be reset when leaving the playsite! 
        /// Otherwise elapsed time will be accrued while the entity is away, and can result in a huge time delta when it returns
        /// </summary>
        public virtual void ResetPlaySiteRegulators()
        {


        }
        
        /// <summary>
        /// add 'entity' to contain list    
        /// has to be the superset of all the paramerers the subclasses need...
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="compartment"></param>
        /// <param name="placeInStorage"></param>
        /// <param name="ignoreCapacity"></param>
        /// <returns></returns>
        public bool AddToContain(Entity entity, StorageCompartment? compartment = null, StorageCondition placeInStorage = null, bool ignoreCapacity = false,
            bool replenish = false, bool isProductionOutput = false, bool assertContainment = true, UpgradeCategory upgradeCategory = null) //bool isUpgrade = false) 
        {
            ValidateContainStatus(entity);


            bool wasAdded = AddToContainList(entity, compartment, placeInStorage, null, ignoreCapacity, replenish, isProductionOutput, upgradeCategory: upgradeCategory); // isUpgrade: isUpgrade);
            if (wasAdded)
            {
                entity.ContainedBy = Parent.EntityID;

                DoContainmentMonitoring(entity, "added to");

                if (entity.Renderable != null)
                {
                    entity.Renderable.UpdateIsDrawnStatus(); 
                                        
                }

                UpdateRenderFlags();

                // disable collisions:
                entity.DisableCollisions();
            }

            if (assertContainment)
            {
                ValidateContainStatus(entity);
            }


            return wasAdded;
        }

        /// <summary>
        /// magazine container only...
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="surplusEntity"></param>
        /// <param name="compartment"></param>
        /// <param name="placeInStorage"></param>
        /// <returns></returns>
        public bool AddToContain(Entity entity, out Entity surplusEntity, StorageCompartment? compartment = null, StorageCondition placeInStorage = null) 
        {
          //  ValidateContainStatus(entity);

            bool wasAdded = AddToContainList(entity, out surplusEntity, compartment, placeInStorage);
            if (wasAdded)
            {
                entity.ContainedBy = Parent.EntityID;

                DoContainmentMonitoring(entity, "added to");

                UpdateRenderFlags();

                // disable collisions??
                entity.DisableCollisions();
            }

            ValidateContainStatus(entity);


            return wasAdded;
        }

        public virtual double? GetUpdateInterval()
        {
            IHasReplenishItems hasReplenishItems = this as IHasReplenishItems;
            if (hasReplenishItems != null && hasReplenishItems.ReplenishItems != null)
            {
                return GameData.Instance.Constants.UpdateIntervalForEntityComponents;
            }

            return null;
        }

        private void UpdateRenderFlags()
        {
            float? percentageFull = null;
            IStorage storage = this as IStorage;
            if (storage != null)
            {
                percentageFull = storage.TotalStored / storage.TotalItemStorageCapacity;
            }
            else
            {
                IHoldsProductionOutput holdsOutput = this as IHoldsProductionOutput;
                if (holdsOutput != null && holdsOutput.TotalStoredOutput.HasValue)
                {
                    percentageFull = holdsOutput.TotalStoredOutput / holdsOutput.TotalOutputCapacity;          
                }
            }

            if (percentageFull.HasValue)
            {
                float fullLimit = Parent.EntityType.ContainerType.FullStatePercentage ?? 0.9f;
                float halfLimit = Parent.EntityType.ContainerType.HalfFullStatePercentage ?? 0.4f;

                if (Parent.Renderable != null)
                {
                    if (percentageFull > fullLimit)
                    {
                        Parent.SetSpriteStateFlag(ClientSide.Renderables.StateModifier.Full);

                    }
                    else if (percentageFull > halfLimit)
                    {
                        Parent.SetSpriteStateFlag(ClientSide.Renderables.StateModifier.HalfFull);

                    }
                    else
                    {

                        Parent.ClearSpriteStateFlag(ClientSide.Renderables.StateModifier.HalfFull);
                        Parent.ClearSpriteStateFlag(ClientSide.Renderables.StateModifier.Full);
                    }
                }

            }

        }

        private void DoContainmentMonitoring(Entity entity, string action)
        {
            // add instrumentation, debug tracking, etc. Will capture every containment that happens, everywhere


           /* The.Client.AddLogEvent(The.Client.Log.DebugEvent, entity, string.Format("was added to container {0}",
                                               parent));
            */

            entity.DebugLog.Add(string.Format("Containment: Was {1} container {0}", Parent.ID, action));

            Parent.DebugLog.Add(string.Format("Containment: {0} was {1} container", entity.ID, action));

        }

        // The part of AddToContain that inheritors can override
        protected abstract bool AddToContainList(Entity entity,
            StorageCompartment? compartment = null, StorageCondition placeInStorage = null, List<PassengerOrCargoSlot> slotsToUse = null,
            bool ignoreCapacity = false,
            bool replenish = false,
            bool isProductionOutput = false,
            UpgradeCategory upgradeCategory = null); // bool isUpgrade = false); 
     


        /// <summary>
        /// magazine container only...
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="surplusEntity"></param>
        /// <param name="compartment"></param>
        /// <param name="placeInStorage"></param>
        /// <returns></returns>
        protected virtual bool AddToContainList(Entity entity, out Entity surplusEntity, StorageCompartment? compartment = null, StorageCondition placeInStorage = null)
        {
            surplusEntity = null;
            return false;
        }


        /// <summary>
        /// the part of Remove that inheritors can override
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="slotsToUse"></param>
        /// <returns></returns>
        protected abstract bool RemoveFromContain(Entity entity, List<PassengerOrCargoSlot> slotsToUse = null);

        /// <summary>
        /// sets ContainedBy to null - inheritors should remove from collection(s).       
        /// 
        /// Warning: this leaves the entity in an illegal (crashy) state! neither contained nor placed in the game world...
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="slotsToUse"></param>
        /// <returns></returns>
        public bool Remove(Entity entity, List<PassengerOrCargoSlot> slotsToUse = null) 
        {
            ValidateContainStatus(entity);

            if (entity.ContainedBy == Parent.EntityID
                || (entity.ContainedBy == null && entity.PartOfID.HasValue)) // #KNIFEHACK - Oct. 11 '16: fixes a bug where a character was carrying a knife which was also a part of a spear in another location. Delete if the assert does not occur.
            {
                entity.ContainedBy = null;

                DoContainmentMonitoring(entity, "removed from");

               /* The.Client.AddLogEvent(The.Client.Log.DebugEvent, entity, string.Format("was removed from container {0}",
                                            parent));
                */

                Item item = entity.Item;
                if (item != null)
                {
                    item.OKToTakeThisItemFromCarrier = null;
                }

              
                if (entity.Renderable != null)
                {
                    entity.Renderable.IsDrawn = true;
                }

                /*if (entity.EntityType.CollidableType != null)
                {*/
                // enable collisions again:
                entity.EnableCollisions(); // PlaceInWorld will also call this. perhaps remove it here.
                

                RemoveFromContain(entity, slotsToUse);

                UpdateRenderFlags();

                HandleAllegiancesInCommRangeRemoveEntity(entity);

                ValidateContainStatus(entity);

                return true;
            }
            else
            {
                //ValidateContainStatus(entity);

                return false;
            }
        }


        /// <summary>
        /// debug/assert function
        /// </summary>
        /// <param name="entity"></param>
        protected void ValidateContainStatus(Entity entity)
        {
            //return; 

#if DEBUG || PROFILE

           
            if (entity.ContainedBy.HasValue && entity.PartOfID.HasValue)
            {
                throw new Exception("Containment/part status error!");  //#KNIFEHACK
            }


            if (entity.ContainedBy == null
                && this.Contains(entity.ID))
            {
                throw new Exception("Containment status error!");  //#CONTAINFIX // v.1.0.1.1
            }

            /*
            if (entity.ID == EntityID.Invalid)
                return;
 
            if (entity.ContainedBy.HasValue)
            {
                Entity container = Entity.FindByID(entity.ContainedBy.Value);
                if (!container.Contains.Contains(entity.ID))
                {
                    throw new Exception("Containment status error!"); // this error still appears (May 2016)                   
                }
            }

            if (entity.IsOnPlaySite())
            {
                if (entity.Location == null)
                {
                    throw new Exception("Containment/location error!");
                }
            }*/
#endif
        }

        // Does this container currently contain the given entity?      
        public abstract bool Contains(EntityID entityID);

        /// <summary>
        /// for switching items into degraded items (junk), entities into corpses etc.
        /// 
        /// the method is responsible for putting entities in the right compartments, perhaps ignoring capacity limits etc.
        /// </summary>
        /// <param name="entityToRemove"></param>
        /// <param name="exchangeWithEntity"></param>
        public abstract void SwitchEntities(Entity entityToRemove, Entity exchangeWithEntity, bool ignoreCapacity = false);

        // remove all entities in contain list
       // public virtual void removeAllContained() { }

        // remove entity from contain list, and from the world						
      //  public virtual void RemoveFromContainAndDestroy(ref Entity entityToDestroy) { }

        // Status bits that contained members will have while contained.	    
        public virtual ContainedEntityStatus getContainedStatusForEntity(Entity queryentity)
        {
            return ContainedEntityStatus.None;
        }

        // Hey, can I shoot out of this vehicle, structure?
        public virtual bool isPassengerAllowedToFire() { return false; }

        // does enquiring entity not have visibility into this container? true by default
      //  public virtual bool CanEntityViewContained(EntityType enquiringEntity) { return true; }

        // can enquiringEntity interact with this contained passenger/occupant
        // returns false if occupant not contained
        public virtual bool isOccupantBlockedByContainer(Entity entity, Entity containedTarget) { return false; }

        // Does this container display its occupants in the InGameUI?
        // A structure, vehicle, may when selected populate the UI with a list of qualified entities it contains
        // or some other description of its occupancy
        public virtual bool isDisplayedInUI() { return false; }

        // If I want to say, attack all the people inside vehicle, which one is closest to me right now?
        public virtual EntityID getClosestRiderToPosition(Vector3 position) { return EntityID.Invalid; }


        /// interface for dis/enabling entitys to enter us.
        public virtual void enableEnter(bool bEnable) { }

        //reset to the initial openness state in ContainerType
        public virtual void restoreDefaultOpenness() { }


        /// <summary>
        /// some contained items give effects to their container
        /// </summary>
        /// <param name="entity"></param>
        public virtual void NotifyBrokenContainedEntity(Entity entity)
        {

        }

        /// <summary>
        /// some contained items give effects to their container
        /// 
        /// should probably not remove the sprite because the upgrade broke down...
        /// </summary>
        /// <param name="entity"></param>
        public virtual void NotifyFunctionalContainedEntity(Entity entity)
        {

        }

        // iterate the contain list
        //	    virtual void iterateContained( delegate, filter ){}					
       // public delegate bool FilterMethod(Entity thisEntity);
        public abstract List<Entity> GetContainedItemsList(Predicate<Entity> rule);// FilterMethod filter);

        
       
        public delegate void IterateMethod(Entity thisEntity);
        public delegate bool IterateBoolMethod(Entity thisEntity);

      //  public abstract void IterateContained(IterateMethod iterateMethod);
        public abstract void IterateContained(Action<Entity> iterateMethod);


       /// <summary>
        /// atomic action that removes a contained entity and either destroys it or places it in a different container or in the open. (Later on the exit/queue mechanisms will be included here)       
       /// </summary>
       /// <param name="entity"></param>
       /// <param name="destroy"></param>
       /// <param name="shouldQueue"></param>
       /// <param name="storageTarget">overrides the next 3 args</param>
       /// <param name="placeInStorageEntity"></param>
       /// <param name="compartment"></param>
       /// <param name="placeInStorage"></param>
       /// <param name="slotsToUse"></param>
       /// <param name="placeOnGround">absolute location</param>
       /// <param name="isOfferedForSale"></param>
       /// <returns></returns>
        public bool Uncontain(Entity entity, bool destroy = false, bool shouldQueue = false, 
            StorageTarget? storageTarget = null, // 
            Entity placeInStorageEntity = null, StorageCompartment? compartment = null, StorageCondition placeInStorage = null, //StorageTarget should override these.
            List<PassengerOrCargoSlot> slotsToUse = null, Vector3? placeOnGround = null) //, bool isOfferedForSale = false)
        {
            ValidateContainStatus(entity);

            if (Remove(entity, slotsToUse))
            {
                bool isExit = false; 
                if (destroy)
                {
                    entity.Destroy();
                }
                else if (shouldQueue && isExit)
                {
                    // TODO
                    //Queue(entity);
                }
                else
                {
                    return EjectEntity(entity, 
                        storageTarget,
                        placeInStorageEntity, compartment, placeInStorage, 
                        placeOnGround);
                }

                ValidateContainStatus(entity);

                return true;
            }


            ValidateContainStatus(entity);

            return false;
        }

        /// <summary>
        /// Remove first! Then call this to place an ejected entity in the world or directly in the specified container/storage without using exits/queuing
        /// 
        /// TODO: don't set coords when ejecting at OtherSite
        /// </summary>
        /// <param name="entityToEject"></param>
        public bool EjectEntity(Entity entityToEject,
            StorageTarget? storageTarget = null,
            Entity placeInStorageEntity = null, StorageCompartment? compartment = null, StorageCondition placeInStorage = null, // storageTarget overrides these
            Vector3? placeOnGround = null, bool isOfferedForSale = false) 
        {

            return EjectEntity(entityToEject, Parent, storageTarget, placeInStorageEntity, compartment, placeInStorage, placeOnGround, isOfferedForSale);
            
        }

        /// <summary>
        /// Now handles "ejected" parts as well
        /// </summary>
        /// <param name="entityToEject"></param>
        /// <param name="parentOrPartOfEntity">Either the container or the entity that the part is inside.</param>
        /// <param name="storageTarget"></param>
        /// <param name="placeInStorageEntity"></param>
        /// <param name="compartment"></param>
        /// <param name="placeInStorage"></param>
        /// <param name="placeOnGround"></param>
        /// <param name="isOfferedForSale"></param>
        /// <returns></returns>
        public static bool EjectEntity(Entity entityToEject, Entity parentOrPartOfEntity,
           StorageTarget? storageTarget = null,
           Entity placeInStorageEntity = null, StorageCompartment? compartment = null, StorageCondition placeInStorage = null, // storageTarget overrides these
           Vector3? placeOnGround = null, bool isOfferedForSale = false)
        {
            Entity containerToUse = placeInStorageEntity;
            StorageCompartment? compartmentToUse = compartment;
            StorageCondition storageConditionToUse = placeInStorage;

            if (storageTarget != null)
            {
                containerToUse = Entity.FindByID(storageTarget.Value.StorageEntity);
                if (containerToUse != null)
                {
                    IStorage iStorage = containerToUse.Contains as IStorage;

                    Storage storage = iStorage.FindStorage(storageTarget.Value.StorageID);

                    // override..
                    // Storage storage = LookUp<Storage, StorageID>.FindByID(storageTarget.Value.StorageID);

                    System.Diagnostics.Debug.Assert(storage != null && iStorage != null, "Storage invalid...");

                    compartmentToUse = iStorage.GetCompartment(storageTarget.Value.StorageID);
                    storageConditionToUse = storage.StorageConditions;
                }
            }
            else if (placeInStorageEntity == null)
            {
                // is the parent entity itself contained?
                if (!parentOrPartOfEntity.GetContainedBy(out containerToUse))
                {
                    return false;
                }
            }

            //HandleAllegiancesInCommRangeRemoveEntity(entityToEject);

            if (containerToUse != null)
            {
                // place the dropped item in selected storage:    
                if (!containerToUse.Contains.AddToContain(entityToEject,
                    compartmentToUse, storageConditionToUse)) //, isOfferedForSale: isOfferedForSale))
                {
                    // no room... place on ground instead:
                    PlaceOnGround(entityToEject, parentOrPartOfEntity, placeOnGround);

                    return false; // we failed...
                }
            }
            else
            {
                // place on ground:
                // TODO: spread out items/entities over the structure area...
                PlaceOnGround(entityToEject, parentOrPartOfEntity, placeOnGround);
            }

            return true;
        }

        private void HandleAllegiancesInCommRangeRemoveEntity(Entity entityToEject)
        {
            // here, other allegiances should forget the entities, for instance if the entities were sold items onboard a vehicle that has now arrived back where it came from..
            // if the entity is placed into a TerminalContainer, it will be seen again.
            if (Parent.EntityType.CommunicatorType != null)
            {
                Allegiance owningAllegiance = Parent.GetAllegianceOrOwner();
                if (owningAllegiance != null)
                {
                    foreach (var allegianceID in owningAllegiance.AllegiancesWeAreInContactWith)
                    {
                        Allegiance otherAllegiance = LookUp<Allegiance, AllegianceID>.FindByID(allegianceID);

                        //Communication.CommunicationMethod? method;
                        if (otherAllegiance != null
                            && otherAllegiance.Site != owningAllegiance.Site)
                            //&& !Communication.Communicates.IsInCommunicationRange(owningAllegiance, otherAllegiance, out method))
                        {
                            entityToEject.HandleEntityMovingOutOfCommunicationRange(otherAllegiance);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityToBePlaced"></param>
        /// <param name="placeOnGround"></param>
        private void PlaceOnGround(Entity entityToBePlaced, Vector3? placeOnGround)
        {
            PlaceOnGround(entityToBePlaced, Parent, placeOnGround);
                      
        }


        private static void PlaceOnGround(Entity entityToBePlaced, Entity parentOrPartOfEntity, Vector3? placeOnGround)
        {
            // NEW: set Site to the container's Site when uncontained:
            entityToBePlaced.Site = parentOrPartOfEntity.Site;

            if (entityToBePlaced.IsOnPlaySite()) //NEW: don't set coords when ejecting at OtherSite
            {
                Vector3 groundLocationToUse = placeOnGround ?? parentOrPartOfEntity.AccessPoint.Value;

                // Point accessPointTilePos = Maps.MapManager.WorldPosToTile(groundLocationToUse);

                entityToBePlaced.PlaceEntityOnPlaySite(groundLocationToUse, Entity.AddRandomOffset.Yes, null, null, null);

                // we do not call PlaceOnPlaySite because some duplicate adds may appear...
                //   entity.SetGroundLocationWithSmallRandomOffset(groundLocationToUse); 

                if (entityToBePlaced.Renderable != null)
                {
                    entityToBePlaced.Renderable.SetToParentLocation();
                }
            }


        }

        public virtual bool IsDriver(Entity entity) { return false;}
        public virtual bool IsPassenger(Entity entity) { return false; }
        public virtual bool IsDriverOrPassenger(Entity entity) { return false; }

        //public virtual bool IsStored(Entity entity) { return false; }


        public static void ComputeSumsOfItems(Entity e, Dictionary<EntityType, int> containedEntities)
        {
            if (e.EntityType.NonLivingType != null) // ??? not agents..?
            {
                int sum = 0;
                if (containedEntities.TryGetValue(e.EntityType, out sum))
                {
                    sum++;
                    containedEntities[e.EntityType] = sum;
                }
                else
                {
                    containedEntities[e.EntityType] = 1;
                }
            }
        }

        #region ISnapshot

        
        public virtual ISnapshot DoSnapshot(Snapshotter sn)
        {
            
            parentID = (EntityID)sn.SnapshotID<Entity, EntityID>(Parent);


            return this;
        }

        public virtual void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            Parent = Entity.FindByID(parentID);
        }

        Snapshotter.Version version;
        public virtual Snapshotter.Version DoVersion(Snapshotter sn)
        {
            version = sn.DoVersion(Snapshotter.Version.Original);
            return version;
        }

        public bool IsSnapshotted { get; set; }

        #endregion
    }
}
