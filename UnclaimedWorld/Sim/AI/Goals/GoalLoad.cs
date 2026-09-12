using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Items;
using UWGame.SimSide.Vehicles;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Entities.Owners;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Entities.Containers.Components;//TODO DECOUPLE
namespace UWGame.SimSide.AI.Goals
{
    class GoalLoad: CompositeGoal
    {
        private EntityID itemToLoad;
        private EntityID vehicleToLoad;

        /// <summary>
        /// if filled, if the owner cannot be resolved, the goal fails
        /// </summary>
        public OwnerID? NewOwner;

        List<PassengerOrCargoSlot> slots;
        List<PassengerOrCargoSlotID> snapshotSlots;


        /// <summary>
        /// loads ONE item by walking to it, picking it up, walking back to vehicle.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="vehicle"></param>
        /// <param name="item"></param>
        /// <param name="newOwner"></param>
        public GoalLoad(Entity entity, EntityID vehicle, EntityID item, OwnerID? newOwner)
            : base(entity)
        {
            itemToLoad = item;
            vehicleToLoad = vehicle;
            NewOwner = newOwner;
      
        }

      

        public GoalLoad()
        {
        }
        


        protected override void Activate()
        {
            //make sure the subgoal list is clear.
            RemoveAllSubgoals();

     /*  Vehicle and item do not have to be in the same spot...*/

            StorageCompartment compartmentToUse = StorageCompartment.Haul;

            IKnownEntityData itemData;
            if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(itemToLoad, out itemData)))
            {
                return;
            }

            IKnownEntityData vehicleData;
            if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(vehicleToLoad, out vehicleData)))
            {
                return;
            }

            bool isCarryingItemToLoad = entity.AgentStorage.Contains(itemToLoad);

            if ((isCarryingItemToLoad || // if we are not already carrying the item, we must have capacity to pick it up:
                itemData.Bulk <= entity.AgentStorage.GetCompartment(compartmentToUse).UnusedCapacity))
            {               

                // first make sure we are out of any vehicle:
                if (entity.DrivingVehicle != null)
                {
                    AddSubgoal(new GoalExitVehicle(entity, entity.DrivingVehicle.Value));
                }
                else if (entity.PassengerInVehicle != null)
                {
                    AddSubgoal(new GoalExitVehicle(entity, entity.PassengerInVehicle.Value));
                }

                if (!isCarryingItemToLoad)
                {
                    Vector3 itemToLoadKnownLocation = itemData.PlaySiteLocation;

                    AddSubgoal(new GoalMoveToPosition(entity, null, itemData, GoalMoveToPosition.VehicleUse.NoVehicle));

                    if (!PickupItemOrUnloadFirst(itemData, compartment: compartmentToUse))
                    {                       
                        return;
                    }

                  /*  AddSubgoal(new GoalMoveToPosition(entity, itemToLoadKnownLocation, null, GoalMoveToPosition.VehicleUse.NoVehicle));

                    AddSubgoal(new GoalPickup(entity, itemToLoad, NewOwner));*/


                }
         
          
                slots = vehicleData.GetCargoSlotsForLoading(itemData.Bulk);

                foreach (PassengerOrCargoSlot slotToLoad in slots)
                {
                    slotToLoad.CargoSlot.TargetedByHauler = entity;
                }

                PassengerOrCargoSlot slot = slots[0];                

                Vector3 entryLocation, cargoBayLocation;
                slot.GetEntryPoints(out entryLocation, out cargoBayLocation);

                AddSubgoal(new GoalMoveToPosition(entity, entryLocation, null, GoalMoveToPosition.VehicleUse.NoVehicle));

                // also turn...?
                AddSubgoal(new GoalTurnToFace(entity, cargoBayLocation.ToVector2()));

                //MLo should this be playing backwards? should goal unload play backwards?
                // this should be redone so the vehicle sends a message when it is ready...
                //AddSubgoal(new GoalWait(entity, 3000d, /*AnimState.Containing, AnimState.Reverse,*/ vehicleData.EntityID));  


                //AddSubgoal(new GoalStartAnimationAndWaitForEnd(entity, vehicleToLoad, "open_for_loading", Playback.Forwards, StartingPoint.Current, //TODO DECOUPLE
                //     BlendMode.Additive, false));
              

                Status = Status.Active;
            }
            else
            {
                // cannot get the item. Fail.
                Status = Status.Failed;
            }

        }


       
        


        protected override void ProcessWhileActive(GameTime elapsed)
        {
            //if status is inactive, call Activate()
            // May fail if no item:
         /*   ActivateIfInactive();

            if (Status == Goals.Status.Active)
            {*/
                Status = ProcessSubgoals(elapsed);

                if (Status == Status.Completed)
                {
                    Entity item;
                    if (EntityIsNotSeenDirectly(itemToLoad, out item))
                    {
                        return; // Status;
                    }

                    Entity vehicle;
                    if (EntityIsNotSeenDirectly(vehicleToLoad, out vehicle))
                    {
                        return; // Status;
                    }

                    if (entity.AgentStorage.Uncontain(item, placeInStorageEntity: vehicle, slotsToUse: slots)) // vehicle.AgentStorage.ItemStorage.LoadCarriedItem(entity, item, slots)) 
                    {
                        IOwner newOwner = LookUpOwners.FindByID(NewOwner);
                        item.ChangeOwnership(newOwner); 

                        Status = Status.Completed;
                    }
                    else
                    {
                        Status = Status.Failed;
                    }
                }
          /*  }

            ExitIfFailedOrCompleted();

            return Status;*/
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

            this.itemToLoad = sn.DoEntityID(itemToLoad);
            this.vehicleToLoad = sn.DoEntityID(vehicleToLoad);
            this.NewOwner = sn.DoEnumNullable(NewOwner);

            if (slots != null)
            {
                snapshotSlots = slots.Select(s => s.ID).ToList();
            }
            this.snapshotSlots = sn.DoList(snapshotSlots);

            return this;
        }


        public override void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            base.LoadPostProcess(sn);

            if (snapshotSlots != null)
            {
                slots = snapshotSlots.Select(s => LookUp<PassengerOrCargoSlot, PassengerOrCargoSlotID>.FindByID(s)).ToList();
            }
            snapshotSlots.Clear(); // remember to clear/set to null for the next save
        }


        #endregion
    }
}
