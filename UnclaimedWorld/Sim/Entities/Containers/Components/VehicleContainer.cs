using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameEngine.Sim.Sim.Entities.Container;
using UWGame.SimSide.Vehicles;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Snapshots;
using System.Diagnostics;

namespace UWGame.SimSide.Entities.Containers.Components
{
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
    /// TODO: merge with Vehicle!
    /// </summary>
    class VehicleContainer : Container, ICrew, ITransport, IGarrison, IStorage, IExit, IHasReplenishItems // IReplenishes
    {
        ItemStorage storage;

        Residence residence;


        /// <summary>
        /// contains agents - crew + passengers... it doesn't seem we need a new class..?
        /// </summary>
        Garrison garrison;


        /// <summary>
        /// fuel, power etc.
        /// </summary>
        ReplenishItems replenishItems;
        public ReplenishItems ReplenishItems
        {
            get
            {
                return replenishItems;
            }
        }


        public VehicleContainer(Entity parent)
            : base(parent)
        {
            VehicleContainerType vehicleType = (VehicleContainerType)parent.EntityType.ContainerType;

            storage = new ItemStorage(parent, true, vehicleType.ItemStorageType);

            if (vehicleType.MaxPassengers > 0)
            {
                garrison = new Garrison(parent); // unmanned vehicles..?
            }

            if (parent.EntityType.ContainerType.ResidenceType != null)
            {
                residence = new Residence(parent);
            }
        }

        public VehicleContainer()         
        {
            Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");     
        }

        public override void Destroy()
        {
            // eject all items:
            storage.UncontainAllEntities(
                GameData.Instance.Constants.ConditionDamageMeanToContentsOfDestroyedContainers,
                GameData.Instance.Constants.ConditionDamageSpreadToContentsOfDestroyedContainers);

            replenishItems.UncontainAllEntities(
                GameData.Instance.Constants.ConditionDamageMeanToContentsOfDestroyedContainers,
                GameData.Instance.Constants.ConditionDamageSpreadToContentsOfDestroyedContainers);

            replenishItems.Destroy();

            if (garrison != null)
            {
                garrison.UncontainAllEntities();
            }

        }


        public Storage FindStorage(StorageID storageID)
        {
            Storage foundStorage = storage.FindStorage(storageID);
            
            return foundStorage;
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

            return items;
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

        public override bool Contains(EntityID entityID)
        {            
            // TODO
            return storage.Contains(entityID) || (garrison != null && garrison.Contains(entityID));
        }

        public Dictionary<StorageCondition, Storage> GetStorageSpaces()
        {
            return storage.StorageSpaces;
        }

        /// <summary>
        /// TODO
        /// </summary>
        public EntityID? Driver
        {
            // TODO
            get
            {
                return null;
            }
            set
            {

            }
        }

        public StorageCompartment GetCompartment(StorageID storageID)
        {
            return StorageCompartment.NormalStorage;
        }

        protected override bool RemoveFromContain(Entity entity, List<PassengerOrCargoSlot> slots)
        {

            if (slots != null)
            {
                foreach (PassengerOrCargoSlot slot in slots)
                {
                    slot.CargoSlot.TargetedByHauler = null;
                }
            }

            bool wasRemoved = storage.Remove(entity, true);

            if (!wasRemoved && garrison != null)
            {
                wasRemoved = garrison.Remove(entity.EntityID);
            }

            if (wasRemoved)
            {


                // handle the slots (if present):
                float storedInSlot;
                float bulkStillToUnload = entity.Bulk;

                if (slots != null)
                {
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
            }




            if (wasRemoved)
            {
                if (Parent.Locomotor != null)
                {
                    Parent.Locomotor.CurrentMaximumSpeedIsDirty = true;
                    Parent.Locomotor.CurrentMaximumSpeedNotAffectedByTerrainIsDirty = true;
                    // must not affect evaluator speed.
                }

                //RecalculateAndSetCurrentSpeed();

                //HandleAttachedBoxAnimation(parent);
            }

            //   return wasRemoved;

            return true;

        }


        public int GetNoOfAgentsInside()
        {
            if (garrison != null)
            {
                return garrison.GetNoOfAgentsInside();
            }
            else return 0;
        }

        /// <summary>
        /// TODO: clean this up
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="compartment"></param>
        /// <param name="placeInStorage"></param>
        /// <param name="slotsToUse"></param>
        /// <returns></returns>
        protected override bool AddToContainList(Entity entity, StorageCompartment? compartment = null, StorageCondition placeInStorage = null, List<PassengerOrCargoSlot> slotsToUse = null, 
            bool ignoreCapacity = false,
            bool replenish = false,
            bool isProductionOutput = false,
            UpgradeCategory upgradeCategory = null) //bool isUpgrade = false)
        {
            if (Garrison.EntityBelongs(entity))
            {
                if (garrison != null)
                {
                    return garrison.Add(entity.EntityID);
                }
                else return false;
            }
            else
            {
                return storage.Add(entity, placeInStorage, ignoreCapacity);
            }

           /* if (!storage.Add(entity, null, ignoreCapacity))
            {
                return false;
            }*/

            return true;

            // TODO: enable this code again:

            foreach (PassengerOrCargoSlot slotToLoad in slotsToUse)
            {
                slotToLoad.CargoSlot.TargetedByHauler = null;
            }
                      

            entity.Item.OKToTakeThisItemFromCarrier = null;


            Parent.Locomotor.CurrentMaximumSpeedIsDirty = true;
            Parent.Locomotor.CurrentMaximumSpeedNotAffectedByTerrainIsDirty = true;
            // must not affect evaluator speed.

            float roomInSlot;
            float bulkStillToLoad = entity.Bulk;
            //float bulkToLoadInThisSlot;

            // fill 'em up:
            foreach (PassengerOrCargoSlot slotToLoad in slotsToUse)
            {
                roomInSlot = slotToLoad.CargoSlot.GetRemainingRoom();

                if (roomInSlot >= bulkStillToLoad)
                {
                    slotToLoad.CargoSlot.BulkCarried += bulkStillToLoad;

                    break;
                }
                else
                {
                    slotToLoad.CargoSlot.BulkCarried += roomInSlot;

                    bulkStillToLoad -= roomInSlot;
                }
            }



            if (Parent.Vehicle != null)
            {
                Parent.Vehicle.UpdateCargoSlotsWithAttachedModels();
            }


            return true;
           
        }

        public override void SwitchEntities(Entity itemToRemove, Entity exchangeWithItem, bool ignoreCapacity = false)
        {
            // TODO: add crew...
            StorageCondition conditions = storage.GetStorageConditions(itemToRemove.EntityID);

            if (Remove(itemToRemove, null))
            {
                AddToContain(exchangeWithItem, null, conditions, ignoreCapacity: true);
            }

          /*  if (storage.Contains(itemToRemove))
            {
                storage.SwitchEntities(itemToRemove, exchangeWithItem);
            }*/

          /*  if (garrison.Contains(itemToRemove.ID))
            {
                garrison.Remove(itemToRemove.ID);

                storage.Add(exchangeWithItem, null);

                exchangeWithItem.ContainedBy = parent.ID;

            }*/
        }

      /*  public void UnloadFromVehicle(Entity vehicle, Vector3 location)// Point tilePos, Vector2 tileCenterOffset)
        {
            //OnBoard = null;
            vehicle.AgentStorage.ItemStorage.Remove(parent, true);

            parent.Location = location;
            parent.MapPosition = MapManager.WorldPosToTile(location); // tilePos;
            The.Map.TileMap[parent.MapPosition.X][parent.MapPosition.Y].AddEntity(parent);

        }*/

/*
        public bool LoadCarriedItem(Entity bearerOfItem, Entity item, List<PassengerOrCargoSlot> slots)
        {
            foreach (PassengerOrCargoSlot slotToLoad in slots)
            {
                slotToLoad.CargoSlot.TargetedByHauler = null;
            }

            if (item.Bulk + TotalStored <= TotalCapacity)
            {
                //StoredItems.Add(item);
                if (!Add(item, null))
                {
                    return false;
                }

                bearerOfItem.AgentStorage.ItemStorage.Remove(item, true);

               
                bearerOfItem.Locomotor.currentMaximumSpeedIsDirty = true;

                item.Item.EquippedBy = null;
                item.Item.OKToTakeThisItemFromCarrier = null;

                //item.OnBoard = Parent;

                CalculateTotalStored();
                Parent.Locomotor.currentMaximumSpeedIsDirty = true;


                float roomInSlot;
                float bulkStillToLoad = item.Bulk;
                //float bulkToLoadInThisSlot;

                // fill 'em up:
                foreach (PassengerOrCargoSlot slotToLoad in slots)
                {
                    roomInSlot = slotToLoad.CargoSlot.GetRemainingRoom();

                    if (roomInSlot >= bulkStillToLoad)
                    {
                        slotToLoad.CargoSlot.BulkCarried += bulkStillToLoad;

                        break;
                    }
                    else
                    {
                        slotToLoad.CargoSlot.BulkCarried += roomInSlot;

                        bulkStillToLoad -= roomInSlot;
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


        #region IExit Methods
        
        private bool preventRecursion = false;
        public void GetDebugMarkers()
        {
            ExitAndEntrance.GetDebugMarkers(this, ref preventRecursion);
        }

        public bool IsDoorAvailable()
        {
            return false;
        }
         public ExitDoor ReserveDoorForEntryOrExit(Entity entity, bool exiting)
        {
            return ExitDoor.NoneNeeded;
        }
        public void UseDoor(Entity entity, ExitDoor door, bool exiting)
        {
        }
        public void UnreserveDoor(ExitDoor door)
        {
        }
        public void SetRallyPoint(Vector3 pos, ExitDoor door)
        {
        }
        public Vector3 GetRallyPoint(ExitDoor door = ExitDoor.NextAvailable)
        {
            return Parent.PlaySiteLocation;
        }
        public bool GetNaturalRallyPoint(ref Vector3 rallyPoint, bool offset = true)
        {
            rallyPoint.X = rallyPoint.X = rallyPoint.X = 0f;
            return false;
        }

        public bool GetDoorPosition(ref Vector3 position, bool exiting, out ExitDoor doorThatWasUsed, ExitDoor door = ExitDoor.NextAvailable)
        {
            // TODO: use the passenger's slot...
            PassengerOrCargoSlotType passengerSlotType =
                ((VehicleContainerType)Parent.EntityType.ContainerType).PassengerOrCargoSlotTypes.FirstOrDefault(s => s.PassengerSlotType != null && s.PassengerSlotType.IsDriversSeat == false);

            // no queuing..?
            position = ExitAndEntrance.GetDoorPosition(this, 
                /*passengerSlotType.Entrance.ExitDoor,*/ passengerSlotType.Entrance.Offset ?? Vector2.Zero);

            doorThatWasUsed = passengerSlotType.Entrance.ExitDoor;

            return true;

           // return Garrison.GetDoorPosition(parent, parent.EntityType.ContainerType.VehicleContainerType.Doors, ref position, ref door, out doorThatWasUsed);
        }


        public Vector3 ComputeAccessPoint()
        {
            //since this is a vehicle, it should be one of the several doors, but which one? the dirver door or the main hatch?
            return GetRallyPoint();
        }


        public bool IsClearToApproach(Entity docker)
        {
            return IsDoorAvailable();
        }

        public bool ReserveApproachPosition(ref Entity docker, ref Vector3 position, out int index)
        {
            ExitDoor door = ReserveDoorForEntryOrExit(docker, false);

            if (door != ExitDoor.NoneAvailable) //valid door reserved
            {
                index = (int)door;
                ExitDoor doorThatWasUsed;
                GetDoorPosition(ref position, false, out doorThatWasUsed, door);
            }

            index = -1;
            return false;
        }


        public bool AdvanceApproachPosition(ref Entity docker, ref Vector3 position, out int index)
        {
            /// Give Entity the next Queue point to move to, and record that that point is taken.
            //In impl, this will consult with IExit ReserveDoor()

            index = -1;
            return false; //TODO impl
        }

        public bool IsClearToEnter(Entity docker)
        {
            return IsDoorAvailable();
        }

        public bool IsClearToAdvance(Entity docker, int dockerIndex)
        {
            ///this fn should examine the area between the door at dockerIndex and its rally point
            ///to make sure it is clear of interlopers, rapscallions or other ne'erdowells

            return true;//for now, everyone is clear to advance all the time, oops
        }

        public void GetEnterPosition(ref Entity docker, ref Vector3 position)
        {
            /// Give Entity the point that is the start of his docking path
            /// Returning null means there is none free
            /// All functions take docker as arg so we could have multiple docks on a building.  
            /// Docker is not assumed, it is recorded and checked.    

            ///In impl, this will consult with IExit.GetRallyPoint()
        }

        public void GetDockPosition(ref Entity docker, ref Vector3 position)
        {
            /// Give Entity the middle point of the dock process where the action() happens 	
            ///In impl, this will consult with IExit.GetDoorPosition()
        }

        public void GetDeparturePosition(ref Entity docker, ref Vector3 position)
        {
            /// Give Entity the point to move to when he is done  
            ///In impl, this will consult with IExit.GetRallyPoint()
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
            this.replenishItems = (ReplenishItems)sn.DoISnapshot(replenishItems);
            this.residence = (Residence)sn.DoISnapshot(residence);
            this.garrison = (Garrison)sn.DoISnapshot(garrison);

            return this;
        }

        public override void LoadPostProcess(Snapshotter sn)
        {
            base.LoadPostProcess(sn);

            if (storage != null)
            {
                storage.LoadPostProcess(sn);
            }

            if (residence != null)
            {
                residence.LoadPostProcess(sn);
            }

            if (garrison != null)
            {
                garrison.LoadPostProcess(sn);
            }

            if (replenishItems != null)
            {
                replenishItems.LoadPostProcess(sn);
            }
        }



        #endregion
    }
}
