using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Vehicles;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using Xclna.Xna.Animation;
using UWGame.ClientSide.Renderables;
using UWGame.ClientSide;
using UWGame.SimSide.Systems;
using UWGame.SimSide.Snapshots;//TODO DECOUPLE
namespace UWGame.SimSide.AI.Goals
{
    /// <summary>
    /// exit goal for both driver and passengers.
    /// </summary>
    class GoalExitVehicle: CompositeGoal
    {
        EntityID vehicle;
       // Vehicle vehicleComponent;

      //  DrivingVehicle drivingVehicleTrigger;
        public GoalExitVehicle(Entity owner, EntityID vehicle) //, DrivingVehicle trigger)
            : base(owner)
        {      
            this.vehicle = vehicle;
           
        //    this.drivingVehicleTrigger = trigger;
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

            this.vehicle = (EntityID)sn.DoEnum(vehicle);

            return this;
        }

        public GoalExitVehicle()
        {
        }

        protected override void Activate()
        {         
            Status = Status.Active;
            Entity vehicleEntity;

            if (!entityIntelligence.GetEntitySeenDirectly(vehicle, out vehicleEntity))
            {
                // something wrong has happened!
                Status = Goals.Status.Failed;
                return;
            }

            if (entity.DrivingVehicle == vehicle || entity.PassengerInVehicle == vehicle)
            {

                // TODO: change this:
                AddSubgoal(new GoalWait(entity, 3, AnimAction.Containing, AnimModifier.Crew)); //, vehicle));


                //AddSubgoal(new GoalStartAnimationAndWaitForEnd(entity, vehicleEntity.EntityID, "driver_entry", Playback.Forwards, StartingPoint.Current, //TODO DECOUPLE
                //     BlendMode.Additive, false));

              /*  AnimationController anim = vehicle.Renderable.RenderAsModel.TryStartAnimation("driver_entry", Playback.Forwards, StartingPoint.Current, RenderAsModel.Mode.Additive, 1f);
                timeToWait = anim.Duration - anim.ElapsedTime;
                StartDelay(anim.Duration - anim.ElapsedTime);*/
            }
            else
            {
                
                Status = Status.Completed;
            }

          
        }



        protected override void ProcessWhileActive(GameTime elapsed)
        {
            //if status is inactive, call Activate()
           /* ActivateIfInactive();

            if (Status == Goals.Status.Active)
            {*/
                if (!IsDelayed(elapsed))
                {
                    Status = ProcessSubgoals(elapsed);

                    if (Status == Status.Completed)
                    {
                        Entity vehicleEntity;


                        // code this defensively, so an additional GoalExitVehicle doesn't throw an error.
                        if (entity.DrivingVehicle != null || entity.PassengerInVehicle != null)
                        {
                            if (!entityIntelligence.GetEntitySeenDirectly(vehicle, out vehicleEntity))
                            {
                                // something wrong has happened!
                                Status = Goals.Status.Failed;

                               // ExitIfFailedOrCompleted();

                                return; // Status;
                            }


                            bool isDriving = entity.DrivingVehicle == vehicle;

                            if (isDriving)
                            {
                                entity.DeleteTriggerOfType(GameData.Instance.AllTriggerTypes["drivenVehicle"]);
                            }


                            Vector3 exitLocation, seatLocation;
                            Vehicle vehicleComponent = vehicleEntity.Vehicle;

                            PassengerOrCargoSlot place = vehicleComponent.GetEntityPlaceInVehicle(entity);
                            // Place? place = vehicleComponent.GetEntityPlaceInVehicle(entity);

                            place.GetEntryPoints(out exitLocation, out seatLocation);
                            //vehicleComponent.GetEntryPoint(place, out exitLocation, out seatLocation); 

                            if (vehicleComponent.ExitVehicle(entity))
                            {
                                entity.Location = exitLocation;

                                entity.FacingNormal = vehicleEntity.FacingNormal;
                                entity.Rotation = vehicleEntity.Rotation;

                                AttachPoint attachPoint;
                                if (isDriving)
                                {
                                    // attachPoint = vehicle.EntityType.Renderable.RenderAsModelType.ModelData.DriverAttachPoint;
                                    attachPoint = vehicleEntity.Renderable.RenderAsModel.ModelData.DriverAttachor;
                                }
                                else
                                {
                                    // attachPoint = vehicle.EntityType.Renderable.RenderAsModelType.ModelData.GetAttachPointFromKeyName(place.PassengerOrCargoSlotType.AttachPointName); //.Value);
                                    attachPoint = vehicleEntity.Renderable.RenderAsModel.ModelData.GetAttachPointFromKeyName(place.PassengerOrCargoSlotType.AttachPointName); //.Value);
                                }

                                if (attachPoint != null) // !string.IsNullOrEmpty(vehicle.EntityType.Renderable.RenderAsModelType.ModelData.DriverAttachPoint.AttachBoneName))
                                {
                                    vehicleEntity.Renderable.RenderAsModel.AnimatedModel.ModelAnimator.DeattachObject(entity.Renderable.RenderAsModel, attachPoint.BoneName);
                                }

                            }

                            // tell all passengers to Get Off:
                            if (isDriving && vehicleComponent.Passengers.Count > 0)
                            {
                                for (int i = 0; i < vehicleComponent.Passengers.Count; i++)
                                {

                                    vehicleComponent.TellPassengerToGetOff(entity, vehicleComponent.Passengers[i]);

                                }
                            }

                            // close the door...
                            vehicleEntity.Renderable.StartAdditionalAnimation("driver_entry", Playback.Backwards, StartingPoint.Current, BlendMode.Normal);


                            Status = Status.Completed;
                            // it is OK if we are already out of the vehicle...


                        }
                    }
                }
           /* }

            ExitIfFailedOrCompleted();

            return Status;*/
                
        }
    }
}
