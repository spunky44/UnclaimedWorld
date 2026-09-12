using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using UWGame.SimSide.Items;
using UWGame.SimSide.Vehicles;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Jobs;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Snapshots; //TODO DECOUPLE

namespace UWGame.SimSide.AI.Goals
{
    /// <summary>
    /// we use this goal to take items out of a container that we can't go into... and then pick them up with GoalPickup
    /// </summary>
    class GoalUnload: CompositeGoal
    {
        private EntityID itemToUnload;
        private EntityID containerToUnload;
        
        // use this to mark the item with the job that needs the item after dropping it off:
        private Jobs.Job assignToJob;
        JobID? snapshotJob;

        List<PassengerOrCargoSlot> slots;
        List<PassengerOrCargoSlotID> snapshotSlots;

        public GoalUnload(Entity owner, EntityID container, EntityID item)
            : base(owner)
        {
            itemToUnload = item;
            containerToUnload = container;
            /*
#if !RELEASE
            if (entity.ID == (EntityID)26381 && itemToUnload == (EntityID)26391)
            {
                throw new Exception();
            }
#endif*/
        }

        /// <summary>
        /// only use this if we are unloading on top of the job that needs the item - not if the item must be carried somewhere else...
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="container"></param>
        /// <param name="item"></param>
        /// <param name="assignToJob"></param>
        public GoalUnload(Entity owner, EntityID container, EntityID item, Jobs.Job assignToJob)
            : base(owner)
        {
            itemToUnload = item;
            containerToUnload = container;
            this.assignToJob = assignToJob;
/*
#if !RELEASE
            if (entity.ID == (EntityID)26381 && itemToUnload == (EntityID)26391)
            {
                throw new Exception();
            }
#endif*/
        }

     

        public GoalUnload()
        {
        }


        protected override void Activate()
        {
            IKnownEntityData itemData;
            if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(itemToUnload, out itemData)))
            {
                return;
            }

            IKnownEntityData containerData;
            if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(containerToUnload, out containerData)))
            {
                return;
            }

            /*
#if !RELEASE
            if (entity.ID == (EntityID)26381)
            {
                throw new Exception();
            }
#endif
*/

            if (
                containerData.ContainsEntity(itemToUnload) &&
                itemData.Bulk <= entity.AgentStorage.ItemStorage.TotalCapacity - entity.AgentStorage.ItemStorage.TotalStored)
            {
                Status = Status.Active;

                // first make sure we are out of any vehicle:
                if (entity.DrivingVehicle != null)
                {
                    AddSubgoal(new GoalExitVehicle(entity, entity.DrivingVehicle.Value));
                }
                else if (entity.PassengerInVehicle != null)
                {
                    AddSubgoal(new GoalExitVehicle(entity, entity.PassengerInVehicle.Value));
                }

            
               // Vector3 loadingLocation;

                Vector3 entryLocation;
                Vector3 cargoBayLocation;
                slots = containerData.GetCargoSlotsForUnloading(itemData.Bulk);
                if (slots != null)
                {
                    PassengerOrCargoSlot slot = slots[0];

                    slot.CargoSlot.TargetedByHauler = entity;

                    
                    slot.GetEntryPoints(out entryLocation, out cargoBayLocation);
                }
                else
                {
                    entryLocation = containerData.AccessPoint.Value;

                    cargoBayLocation = containerData.Location.Value;                   
                }

               // vehicleToLoad.Vehicle.GetLoadingLocation(out loadingLocation); 
                // walk to vehicle:
                AddSubgoal(new GoalMoveToPosition(entity, entryLocation, null, GoalMoveToPosition.VehicleUse.NoVehicle));

               // Vector2 cargoBay = vehicleToLoad.Vehicle.GetRelativePointRotated(vehicleToLoad.EntityType.VehicleType.CargoHold);
                if (cargoBayLocation != entryLocation)
                {
                    AddSubgoal(new GoalTurnToFace(entity, cargoBayLocation.ToVector2()));
                }

                // this should be redone so the vehicle sends a message when it is ready...
                //AddSubgoal(new GoalWait(entity, 3000d, /* AnimState.Containing, AnimState.Reverse,*/ containerData.EntityID));  

           
            }
            else
            {
                // cannot get the item. Fail.
                Status = Status.Failed;
            }

        }

        protected override void ProcessWhileActive(GameTime elapsed)
        {
           
            Status = ProcessSubgoals(elapsed);

            if (Status == Status.Completed)
            {
                Entity itemEntity;
                if (!entityIntelligence.GetEntitySeenDirectly(itemToUnload, out itemEntity))
                {
                    Status = Goals.Status.Failed;
                    return; // Status;
                }

                Entity containerToUnloadEntity;
                if (!entityIntelligence.GetEntitySeenDirectly(containerToUnload, out containerToUnloadEntity))
                {
                    Status = Goals.Status.Failed;
                    return; // Status;
                }


                if (containerToUnloadEntity.Contains.Uncontain(itemEntity, slotsToUse: slots, placeOnGround: entity.Location))
                {
                    Status = Status.Completed;

                    // "Give" the item to the job that needs it:
                    if (assignToJob != null)
                    {
                        if (assignToJob is ProcessJob)
                        {
                            ((ProcessJob)assignToJob).AssignInput(itemEntity);
                        }
                    }
                }
                else
                {
                    Status = Status.Failed;
                }
            }

        }


        public override string ToString()
        {
            return string.Format("{0} {1}", base.ToString(), itemToUnload);
          
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

            this.snapshotJob = sn.SnapshotID<Job, JobID>(assignToJob);

            this.containerToUnload = sn.DoEntityID(containerToUnload);
            this.itemToUnload = sn.DoEntityID(itemToUnload);
          
            if (slots != null)
            {
                snapshotSlots = slots.Select(s => s.ID).ToList();
            }

            this.snapshotSlots = sn.DoList(snapshotSlots);

            return this;
        }

        public override void LoadPostProcess(Snapshotter sn)
        {
            base.LoadPostProcess(sn);

            assignToJob = LookUp<Job, JobID>.FindByID(snapshotJob);

            if (snapshotSlots != null)
            {
                slots = snapshotSlots.Select(s => LookUp<PassengerOrCargoSlot, PassengerOrCargoSlotID>.FindByID(s)).ToList();
                snapshotSlots.Clear(); // remember to clear/set to null for the next save
            }
            //Moved clear into nullcheck. 2014-10-23 - Andreas
            
        }

        #endregion
    }
}
