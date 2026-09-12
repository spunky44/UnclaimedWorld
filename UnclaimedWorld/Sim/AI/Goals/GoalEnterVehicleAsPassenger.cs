using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Vehicles;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Items;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Renderables;
using UWGame.ClientSide;
using Xclna.Xna.Animation;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Entities.Containers.Components;//TODO DECOUPLE
namespace UWGame.SimSide.AI.Goals
{
    class GoalEnterVehicleAsPassenger: CompositeGoal
    {
        Entity vehicle;
        Vehicle vehicleComponent;

        GoalFollowPath parentGoal;
        // bool okToWait;
        // Place? place;
        PassengerOrCargoSlot slot;

        float? previousDistanceToVehicle = null;
       
        public GoalEnterVehicleAsPassenger(Entity owner, Entity vehicle, GoalFollowPath parentGoal, float? previousDistanceToVehicle, /*bool okToWait,*/ List<EntityGroupID> ownersOfVehicles)
            : base(owner)
        {      
            this.vehicle = vehicle;
            vehicle.Find(out vehicleComponent);

            this.previousDistanceToVehicle = previousDistanceToVehicle;

            this.parentGoal = parentGoal;
          //  this.okToWait = okToWait;
            this.ownersOfVehicles = ownersOfVehicles;

        }


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

            sn.DoUnknownObject(vehicle);//TODO lookup
            sn.DoUnknownObject(vehicleComponent);//TODO lookup
            sn.DoUnknownObject(parentGoal);//TODO summarily assign in parent snapshot
           
            sn.DoUnknownObject(this.slot);
            this.previousDistanceToVehicle = sn.DoFloatNullable(previousDistanceToVehicle);

            return this;
        }

        public GoalEnterVehicleAsPassenger()
        {
        }


        protected override void Activate()
        {
            float distanceToVehicle = Common.DistanceOctile(entity.PlaySiteLocation, vehicle.Location.Value);
           //float rotationOfVehicle = vehicle.Locomotor.Rotation;

            bool okToWait = false;

            if (distanceToVehicle < 90f) 
            {
                if (!vehicle.Locomotor.IsMovingOrRotating) // let's start walking to the entrance only when the vehicle is standing still and not turning.
                {
                    if (vehicleComponent.Passengers.Count < ((VehicleContainerType)vehicle.EntityType.ContainerType).MaxPassengers)
                    {
                        Status = Status.Active;

                        Vector3 entryLocation, seatLocation;

                        slot = vehicleComponent.GetPlaceForNewPassenger();

                        if (slot != null)
                        {
                            slot.PassengerSlot.TargetedBy = entity;
                            slot.GetEntryPoints(out entryLocation, out seatLocation);
                            //vehicleComponent.GetEntryPoint(place, out entryLocation, out seatLocation);

                            // walk to vehicle:
                            AddSubgoal(new GoalMoveToPosition(entity, entryLocation, null, GoalMoveToPosition.VehicleUse.NoVehicle));

                            AddSubgoal(new GoalTurnToFace(entity, seatLocation.ToVector2()));

                            return;
                        }
                        else
                        {
                            // vehicle is full. Fail.
                            Status = Status.Failed;
                            return;
                        }
                    }
                    else
                    {
                        // vehicle is full. Fail.
                        Status = Status.Failed;
                        return;
                    }
                }
                else
                {
                    okToWait = true; // the vehicle is near, but still moving. Let's wait a bit more.
                }
            }
            else
            {
                // the vehicle is far away. Let's see if it is coming closer:
                if (!previousDistanceToVehicle.HasValue || previousDistanceToVehicle.Value > distanceToVehicle)
                {
                    previousDistanceToVehicle = distanceToVehicle;
                    okToWait = true;
                }
                else
                {
                    okToWait = false;
                }
            }

            
            if (okToWait)
            {
                // wait for the vehicle for a while:
                parentGoal.AddSubgoal(new GoalWaitForRide(entity, 1));
                // then try again
                parentGoal.AddSubgoal(new GoalEnterVehicleAsPassenger(entity, vehicle, parentGoal, previousDistanceToVehicle, ownersOfVehicles));

                // THIS goal is now complete. We will start waiting now...
                Status = Status.Completed;
            }
            else
            {
                // vehicle never showed up. Fail.
                Status = Status.Failed;
            }

            /* if (okToWait)
            {
                // wait for the vehicle for a while:
                parentGoal.AddSubgoal(new GoalWaitForRide(entity, 2));
                // then try again, this time without waiting:
                parentGoal.AddSubgoal(new GoalEnterVehicleAsPassenger(entity, vehicle, parentGoal, false, ownersOfVehicles));

                // THIS goal is now complete. We will start waiting now...
                Status = Status.Completed;
            }
            else
            {
                // vehicle never showed up. Fail.
                Status = Status.Failed;
            }*/
            

        }

        protected override void ProcessWhileActive(GameTime elapsed)
        {
            //if status is inactive, call Activate()
            // May fail if no item:
        /*    ActivateIfInactive();

            if (Status == Status.Active)
            {*/
                Status = ProcessSubgoals(elapsed);

                if (Status == Status.Completed)
                {

                    vehicleComponent.EnterVehicleAsPassenger(entity, slot);
                   

                   // AttachPoint attachPoint = vehicle.EntityType.Renderable.RenderAsModelType.ModelData.GetAttachPointFromKeyName(slot.PassengerOrCargoSlotType.AttachPointName); //.Value);                    
                    AttachPoint attachPoint = vehicle.Renderable.RenderAsModel.ModelData.GetAttachPointFromKeyName(slot.PassengerOrCargoSlotType.AttachPointName); //.Value);                    

                    // if no attach point exists, the model will simply be invisible.
                    if (attachPoint != null)
                    {
                        Vector3? translation = attachPoint.Translation;                       
                       // float scaling = entity.EntityType.Renderable.RenderAsModelType.ModelScale;
                        float scaling = entity.Renderable./*TODO DECOUPLE*/RenderAsModel.FinalModelScale;
                        string attachBoneName = attachPoint.BoneName;                        
                        string animationToUse = attachPoint.AttachedAnimationName;

                        entity.Renderable.SetAnimationStateFlag(AnimModifier.Passenger);

                        //entity.Renderable./*TODO DECOUPLE*/RenderAsModel.StartAnimation(animationToUse, Playback.Forwards, StartingPoint.Current, BlendMode.NoBlending, 1f, Looping.Yes); // TODO DECOUPLE

                        AttachPoint attachee;
                        Vector3 translationToUse, rotationToUse;

                        RenderAsModel.GetAttachTransformations(entity.Renderable, attachPoint, AttacheePoint.Bottom, null, out attachee, out translationToUse, out rotationToUse);
                               
                        vehicle.Renderable.AttachEntityAndCreateLocalTransform(entity.Renderable, scaling, attachPoint, 
                            entity.Renderable./*TODO DECOUPLE*/RenderAsModel.ModelData.BottomAttachee, AttacheePoint.Bottom, translationToUse, rotationToUse);

                     //   string attacheeBoneName = "AttacheeBottom";
                      //  vehicle.Renderable.RenderAsModel.AttachEntityAndCreateLocalTransform(entity, scaling, attachPoint, null, attacheeBoneName);
                            
                    }


                    // after we're on:
                    parentGoal.AddSubgoal(new GoalWaitAsPassenger(entity));
                    parentGoal.AddSubgoal(new GoalExitVehicle(entity, vehicle.EntityID));

                    // delete the rest of the path now. we are going some other route.
                    parentGoal.Path.Clear();
                    parentGoal.WaypointPath.Clear();

                    // instead, request a new path from where we get out to the destination:
                    parentGoal.AddSubgoal(new GoalMoveToPosition(entity, parentGoal.parentGoal.Destination.Value.ToVector3(), ownersOfVehicles, GoalMoveToPosition.VehicleUse.FreeUpAfterUse));

                    Status = Status.Completed;

                    // driver can start now:
                    vehicleComponent.DrivenBy.SendMessage(new Message(entity, Message.MessageTypes.OKImOn, null));
                }
          /*  }

            ExitIfFailedOrCompleted();

            return Status;*/
        
        }
    }
}
