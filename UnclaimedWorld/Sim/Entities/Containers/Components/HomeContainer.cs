using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Vehicles;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Maps;
using Microsoft.Xna.Framework.Input;
using UWGame.ClientSide;
using GameStateManagement;
using UWGame.Control;
using UWGame.SimSide.Snapshots;
using System.Diagnostics;


namespace UWGame.SimSide.Entities.Containers.Components
{
 

    /// <summary>
    /// used for structures where entities can sleep/make a home
    /// </summary>
    class HomeContainer : Container, IGarrison, IStorage, IResidence, IExit, IUpgrades
    {
        /// <summary>
        /// contains items
        /// </summary>
        ItemStorage storage;

        /// <summary>
        /// contains agents
        /// </summary>
        Garrison garrison;

        Residence residence;

        /// <summary>
        /// can be null!!!
        /// </summary>
        UpgradeItems upgradeItems;


        public HomeContainer(Entity parent)
            : base(parent)
        {
            storage = new ItemStorage(parent, true, ((HomeContainerType)parent.EntityType.ContainerType).ItemStorageType);

            garrison = new Garrison(parent);

            if (parent.EntityType.ContainerType.CanBeUpgraded)
            {
                upgradeItems = new UpgradeItems(parent);
            }

            if (parent.EntityType.ContainerType.ResidenceType != null)
            {
                residence = new Residence(parent);
            }


        }

        public HomeContainer()         
        {
            Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");     
        }


        public Residence Residence
        {
            get
            {
                return residence;
            }
        }

        public override void IterateContained(Action<Entity> del) //Container.IterateMethod del)
        {
            if (storage != null)
            {
                storage.IterateContained(del);
            }

            if (garrison != null)
            {
                garrison.IterateContained(del);
            }

            if (upgradeItems != null)
            {
                upgradeItems.IterateContained(del);
            }

        }

        public override List<Entity> GetContainedItemsList(Predicate<Entity> rule)
        {
            List<Entity> items = new List<Entity>();

            if (storage != null)
            {
                storage.GetContainedItemsList(rule, items);
            }

            if (garrison != null)
            {
                garrison.GetContainedItemsList(rule, items);
            }

            if (upgradeItems != null)
            {
                upgradeItems.GetContainedItemsList(rule, items);
            }

            return items;
        }

        public Storage FindStorage(StorageID storageID)
        {
            Storage foundStorage = storage.FindStorage(storageID);

            return foundStorage;
        }
        
        public override bool Contains(EntityID entityID)
        {
            return storage.Contains(entityID) 
                || (garrison != null && garrison.Contains(entityID))
                || (upgradeItems != null && upgradeItems.Contains(entityID));
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
            return storage.GetStoredIn(entity.EntityID);
        }

        public Dictionary<StorageCondition, Storage> GetStorageSpaces()
        {
            return storage.StorageSpaces;

        }

        public int GetNoOfAgentsInside()
        {
            return garrison.GetNoOfAgentsInside();
        }

        public void FixContainmentBug(EntityID entityID)
        {
            garrison.Remove(entityID);
        }

        protected override bool AddToContainList(Entity entity, StorageCompartment? compartment = null, StorageCondition placeInStorage = null, List<PassengerOrCargoSlot> slotsToUse = null, 
            bool ignoreCapacity = false,
            bool replenish = false,
            bool isProductionOutput = false, 
            UpgradeCategory upgradeCategory = null) //bool isUpgrade = false)
        {
            if (upgradeCategory != null) // isUpgrade)
            {
                return upgradeItems.Add(upgradeCategory, entity);
            }
            else if (Garrison.EntityBelongs(entity))
            {
                return garrison.Add(entity.EntityID);
            }
            else
            {
                return storage.Add(entity, placeInStorage,ignoreCapacity);
            }
        }

        public override void NotifyBrokenContainedEntity(Entity entity)
        {
            EndEffectsFromUpgraderItem(entity);
        }

        public override void NotifyFunctionalContainedEntity(Entity entity)
        {
            StartEffectsAndSpriteFromUpgraderItem(entity);
        }

        public void EndEffectsFromUpgraderItem(Entity entity)
        {
            if (upgradeItems != null && upgradeItems.Contains(entity.EntityID))
            {
                upgradeItems.EndEffects(entity);
            }
        }

        public void StartEffectsAndSpriteFromUpgraderItem(Entity entity)
        {
            if (upgradeItems != null && upgradeItems.Contains(entity.EntityID))
            {
                upgradeItems.ApplyUpgradeEffects(entity);
            }
        }

        /*
        public override void NotifyBrokenContainedEntity(Entity entity)
        {
            EndEffectsFromUpgraderItem(entity);
        }*/

        public override void SwitchEntities(Entity itemToRemove, Entity exchangeWithItem, bool ignoreCapacity = false)
        {
            if (upgradeItems != null && upgradeItems.Contains(itemToRemove.ID))
            {
                UpgradeCategory upgradeCategory = upgradeItems.GetUpgradeCategory(itemToRemove.ID);
                if (Remove(itemToRemove, null))
                {
                    AddToContain(exchangeWithItem, upgradeCategory: upgradeCategory, ignoreCapacity: true);
                }
            }
            else
            {
                StorageCondition conditions = storage.GetStorageConditions(itemToRemove.EntityID);

                if (Remove(itemToRemove, null))
                {
                    AddToContain(exchangeWithItem, null, conditions, ignoreCapacity: true);
                }
            }

            /*if (garrison.Contains(itemToRemove.ID))
            {
                garrison.Remove(itemToRemove.ID);

                storage.Add(exchangeWithItem, null);

                exchangeWithItem.ContainedBy = parent.ID;

            }*/
        }

        protected override bool RemoveFromContain(Entity entity, List<PassengerOrCargoSlot> slots)
        {

            bool wasRemoved = storage.Remove(entity, true);

            if (!wasRemoved)
            {
                wasRemoved = garrison.Remove(entity.EntityID);
            }

            if (!wasRemoved && upgradeItems != null)
            {
                wasRemoved = upgradeItems.Remove(entity.EntityID);
            }

            //return wasRemoved;

            return true;

        }


        public StorageCompartment GetCompartment(StorageID storageID)
        {
            return StorageCompartment.NormalStorage;            
        }

        #region IUpgrades

        public bool IsUpgrade(EntityID entityID)
        {
            if (upgradeItems != null)
            {
                return upgradeItems.Contains(entityID);
            }

            return false;
        }

        public Dictionary<UpgradeCategory, EntityID> ContainedUpgrades
        {
            get
            {
                if (upgradeItems != null)
                {
                    return upgradeItems.ContainedUpgrades;
                }

                return null;
            }
        }

        #endregion

        #region IExit Methods
        public bool IsDoorAvailable()
        {
            //Lookup flags iterating doors[]; return true on first unreserved door 

            return false;
        }

        ExitDoor simplifiedDoorToUseIndex = ExitDoor.Door1;

        public ExitDoor ReserveDoorForEntryOrExit(Entity entity, bool exiting)
        {
            return ExitAndEntrance.ReserveDoorForEntryOrExit(Parent, entity, exiting, ref simplifiedDoorToUseIndex);
        }

        public void UseDoor(Entity entity, ExitDoor door, bool exiting)
        {
            //if exiting
            //consult the containertype for constants...
            //compute exit position and orentation using method getExitPosition()
            //dequeue the exiting entity
            //ask the locomotor of entity to teleport to that position/orientation
            //getRallyPoint()
            //push a movetoposition subgoal to the rally point
            //after that subgoal, goalthink takes over
            //else entering
            //suspend the ai (goalthink)
            //push a movetoposition subgoal to the enteroffset
            //set a collision callback to parent container, which will contain entity instantly
        }

        public void UnreserveDoor(ExitDoor door)
        {
            //clear the flag in doors[]  

        }

        public void SetRallyPoint(Vector3 pos, ExitDoor door)
        {
            //dynamically adjust or define a rally point for this door
            //these are assigned in worldspace
            //store new rally point in member doors[], keyed to door ExitDoor value
        }

        public Vector3 GetRallyPoint(ExitDoor door = ExitDoor.NextAvailable)
        {
            HomeContainerType containerType = (HomeContainerType)Parent.EntityType.ContainerType;

            return ExitAndEntrance.GetRallyPoint(this, door);           
        }

        public bool GetNaturalRallyPoint(ref Vector3 rallyPoint, bool offset = true)
        {
            return ExitAndEntrance.GetNaturalRallyPoint(this, ref rallyPoint, offset);
            
          /*  rallyPoint = parent.PlaySiteLocation;
            if (((HomeContainerType)parent.EntityType.ContainerType).HasRallyPointInCourtyard)
                return true;//because the middle of the building is the middle of the courtyard

            //just whip up a point in "front" of the building
            float rallyPointDistance = Math.Min(2, parent.EntityType.StructureType.HeightInTiles) * MapManager.tileSize;
            rallyPoint.Y += rallyPointDistance;
            return true;*/
        }

        public bool GetDoorPosition(ref Vector3 position, bool exiting, out ExitDoor doorThatWasUsed, ExitDoor door = ExitDoor.NextAvailable)
        {
            return ExitAndEntrance.GetDoorPosition(this, ref position, ref door, out doorThatWasUsed);
        }
              


        public Vector3 ComputeAccessPoint()
        {
            return GetRallyPoint();
        }

        public bool IsClearToApproach(Entity docker)
        {
            return IsDoorAvailable();
        }

       

      
        public bool AdvanceApproachPosition(ref Entity docker, ref Vector3 position, out int index)
        {
            
            /// Give Entity the next Queue point to move to, and record that that point is taken.
            //In impl, this will consult with IExit ReserveDoor()

            index = -1; // this should look for the next best unreserved door
            return false; //TODO impl
        }


        public bool IsClearToEnter(Entity docker)
        {
            return IsDoorAvailable();
        }

        public bool IsClearToAdvance(Entity docker, int dockerIndex)
        {
            //this fn should examine the area between the door at dockerIndex and its rally point
            //to make sure it is clear of interlopers, rapscallions or other ne'erdowells

            return true;//for now, everyone is clear to advance all the time, oops
        }

        public void GetEnterPosition(ref Entity docker, ref Vector3 position)
        {
            /// Give Entity the point that is the start of his docking path
            /// Returning null means there is none free
            /// All functions take docker as arg so we could have multiple docks on a building.  
            /// Docker is not assumed, it is recorded and checked.    

            //In impl, this will consult with IExit.GetRallyPoint()
        }

        public void GetDockPosition(ref Entity docker, ref Vector3 position)
        {
            /// Give Entity the middle point of the dock process where the action() happens 	
            //In impl, this will consult with IExit.GetDoorPosition()
        }

        public void GetDeparturePosition(ref Entity docker, ref Vector3 position)
        {
            /// Give Entity the point to move to when he is done  
            //In impl, this will consult with IExit.GetRallyPoint()
        }

        public void OnApproachRallyReached(ref Entity docker)
        {
            /// Entity has reached the Enter rally Point.
        }

        public void OnDockReached(ref Entity docker)
        {
            /// Entity has reached the Dock point        
        }

        public void OnDepartureRallyReached(ref Entity docker)
        {
            /// Entity has reached the rally point on his way out.  He is no longer busy
        }

        public bool Action(ref Entity docker)
        {
            /// Perform our specific action on visiting entity.
            /// examples, fill his basket with apples, his tank with fuel, his belly with food...
            /// Returning FALSE means there is nothing for you to do so entity should leave
            return true;
        }


        public void CancelDock(ref Entity docker)
        {
            /// Clear entity from any reserved points, and if entity was the reason we were Busy, we aren't anymore.
        }

        public bool DockOpen
        {
            /// Is the dock open to accepting dockers?
            get
            {
                return true;
            }
        }

        public bool IsAllowedtoDock(ref Entity dockingEntity)
        {
            ///can entity dock here?
            ///this should be a combination of DockOpen, and also applying an entityFilter on dockingEntity
            ///this filter should be defined in COntainerTYpe, but might be hard coded in the interim
            return true;
        }

        public bool UsesRallyPointAfterUndock
        {
            /// A minority of docks want to give you a final command to their rally point. 
            /// this should refer to data in ContainType in many cases
            get
            {
                return true; // for now
            }
        }

        #endregion


        public override void Destroy()
        {

            // eject all items:
            storage.UncontainAllEntities(GameData.Instance.Constants.ConditionDamageMeanToContentsOfDestroyedContainers,
                GameData.Instance.Constants.ConditionDamageSpreadToContentsOfDestroyedContainers);

            storage.Destroy();

            garrison.UncontainAllEntities();

            if (upgradeItems != null)
            {
                upgradeItems.DestroyAllEntities();
            }
        }


        private bool preventRecursion = false;
        public void GetDebugMarkers()
        {
            ExitAndEntrance.GetDebugMarkers(this, ref preventRecursion);

            /*
#if (DEBUG || PROFILE)


            if (Kensei.Dev.Options.GetOption("Overlays.Markers") == false)
                return;

            if (preventRecursion) // so we can call DrawAllPoints from GetDoor, GetRally, etc.
                return;
            preventRecursion = true;

            // MapClient.ClearAllVisitorMarkers(this);

            ExitDoor doorThatWasUsed;
            for (ExitDoor t = ExitDoor.Door1; t < ExitDoor.Max; t++)
            {
                Vector3 door = Vector3.Zero;
                if (GetDoorPosition(ref door, false, out doorThatWasUsed, t))
                {
                    The.MapUI.AddDebugMarker(door, Color.Chartreuse, this);

                    Vector3 rally = GetRallyPoint(t);
                    The.MapUI.AddDebugMarker(rally, Color.Green, this);                    
                }

            }

            Vector3 naturalRallyPoint = parent.PlaySiteLocation;
            if (GetNaturalRallyPoint(ref naturalRallyPoint))
            {
                The.MapUI.AddDebugMarker(naturalRallyPoint, Color.Gray, this);

            }
            preventRecursion = false;
#endif**/

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

            this.garrison = (Garrison)sn.DoISnapshot(garrison);
            this.storage = (ItemStorage)sn.DoISnapshot(storage);
            this.residence = (Residence)sn.DoISnapshot(residence);
            this.upgradeItems = (UpgradeItems)sn.DoISnapshot(upgradeItems);

            this.simplifiedDoorToUseIndex = sn.DoEnum(simplifiedDoorToUseIndex);


            sn.Ignore(preventRecursion);

            return this;
        }

        public override void LoadPostProcess(Snapshotter sn)
        {
            base.LoadPostProcess(sn);

            garrison.LoadPostProcess(sn);
            storage.LoadPostProcess(sn);

            if (residence != null)
            {
                residence.LoadPostProcess(sn);
            }

            if (upgradeItems != null)
            {
                upgradeItems.LoadPostProcess(sn);
            }
        }

       

        #endregion

    }
}
