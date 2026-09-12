using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Vehicles;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using Xclna.Xna.Animation;
using UWGame.ClientSide.Renderables;
using UWGame.ClientSide;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Systems;
using UWGame.SimSide.Systems.Triggers;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Entities.Containers.Components;//TODO DECOUPLE
namespace UWGame.SimSide.AI.Goals
{
    class GoalEnterVehicleAsDriver: CompositeGoal
    {
        EntityID vehicle;
       // Vehicle vehicleComponent;
        Trigger trigger;

        PassengerOrCargoSlot driversSlot;

        public GoalEnterVehicleAsDriver(Entity owner, EntityID vehicle, Trigger trigger)
            : base(owner)
        {      
            this.vehicle = vehicle;
         //   vehicle.Find(out vehicleComponent);

            this.trigger = trigger;

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
            sn.DoUnknownObject(this.trigger);
            sn.DoUnknownObject(this.driversSlot);

            return this;
        }

        public GoalEnterVehicleAsDriver()
        {
        }

        protected override void Activate()
        {
            //make sure the subgoal list is clear.
            RemoveAllSubgoals();
                        

            IKnownEntityData vehicleData;
            EntityResult result = entityIntelligence.GetKnownData(vehicle, out vehicleData);

            bool hasDriver = false;

            if (result == EntityResult.SeenDirectly) //entityIntelligence.GetEntitySeenDirectly(this.vehicle, out vehicleEntity))
            {
                Entity vehicleEntity = (Entity)vehicleData;
                Container vehicleContains = vehicleEntity.Contains;

                if (vehicleContains != null)
                {
                    ICrew crewedVehicle = vehicleContains as ICrew;

                    if (crewedVehicle != null)
                    {
                        // are we already driving?
                        if (crewedVehicle.IsDriver(entity))
                        {
                            Status = Status.Completed;
                            return;
                        }
                        else if (crewedVehicle.Driver != null)
                        {
                            hasDriver = true;
                        }
                    }                    
                }
            }
            


            if (!hasDriver) // vehicleComponent.DrivenBy == null)
            {
                Status = Status.Active;
            
                // walk to the vehicle if necessary
            
                Vector3 entryLocation, seatLocation;
                driversSlot = vehicleData.GetFreeDriversSlot();
                driversSlot.GetEntryPoints(out entryLocation, out seatLocation); 

                //vehicleComponent.GetEntryPoint(Place.Driver, out entryLocation, out seatLocation); 

                AddSubgoal(new GoalMoveToPosition(entity, entryLocation, null, GoalMoveToPosition.VehicleUse.NoVehicle));

                AddSubgoal(new GoalTurnToFace(entity, seatLocation.ToVector2()));

                // TODO: change this:
                AddSubgoal(new GoalWait(entity, 3, AnimAction.Containing, AnimModifier.Crew)); //, ((Entity)vehicleData).ID));


                //AddSubgoal(new GoalStartAnimationAndWaitForEnd(entity, vehicle, "driver_entry", Playback.Forwards, StartingPoint.Current, //TODO DECOUPLE
                //     BlendMode.Additive, false));
              
            }
            else
            {
                // cannot enter vehicle - it is already taken. Fail.
                Status = Status.Failed;
            }

        }

        protected override void ProcessWhileActive(GameTime elapsed)
        {
            //if status is inactive, call Activate()
            // May fail if no item:
          /*  ActivateIfInactive();

            if (Status == Status.Active)
            {*/

                Status = ProcessSubgoals(elapsed);

                if (Status == Status.Completed)
                {                    
                    Entity vehicleEntity;
                    if (!entityIntelligence.GetEntitySeenDirectly(vehicle, out vehicleEntity))
                    {
                        Status = Goals.Status.Failed;
                        return; // Status;
                    }

                    Vehicle vehicleComponent = vehicleEntity.Vehicle;
                    
                    if (vehicleComponent.DrivenBy == null && driversSlot.DriversSlot.Driver == null) // just to make sure...
                    {
                        vehicleComponent.EnterVehicleAsDriver(entity, driversSlot);
                        

                        // attach to drivers seat, if open vehicle:
                        //BonePose attachTo;
                        //attachTo = parent.Renderable.RenderAsModel.AnimatedModel.ModelAnimator.BonePoses[""];

                        // Morten will create a special marker bone on the seat!!! No he won't, Maya problems. Hack it instead.  
                      
                        
                      /*  if (vehicle.EntityType.Renderable.RenderAsModelType.ModelData.DriversSeatScaling.HasValue)
                        {
                            scale = Matrix.CreateScale(vehicle.EntityType.Renderable.RenderAsModelType.ModelData.DriversSeatScaling.Value); // 1.5f); // WHY is this necessary?
                        }
                        else
                        {
                            scale = Matrix.Identity;
                        }*/

                        if (vehicleEntity.Renderable.RenderAsModel.ModelData.DriverAttachor != null)//vehicle.EntityType.Renderable.RenderAsModelType.ModelData.DriverAttachPoint != null)
                        {
                           // AttachPoint attachPoint = vehicle.EntityType.Renderable.RenderAsModelType.ModelData.DriverAttachPoint;
                            AttachPoint attachPoint = vehicleEntity.Renderable.RenderAsModel.ModelData.DriverAttachor;
                            Vector3? translation = attachPoint.Translation;
                            string attachBoneName = attachPoint.BoneName;
                           // float scaling = entity.EntityType.Renderable.RenderAsModelType.ModelScale;
                            float scaling = entity.Renderable./*TODO DECOUPLE*/RenderAsModel.FinalModelScale;
                            
                            // TODO: attach the entity
                            AttachPoint attachee;
                            Vector3 translationToUse, rotationToUse;

                            RenderAsModel.GetAttachTransformations(entity.Renderable, attachPoint, AttacheePoint.Bottom, null, out attachee, out translationToUse, out rotationToUse);
                        
                            vehicleEntity.Renderable.AttachEntityAndCreateLocalTransform(entity.Renderable, scaling, attachPoint, 
                                entity.Renderable./*TODO DECOUPLE*/RenderAsModel.ModelData.BottomAttachee, AttacheePoint.Bottom, translationToUse, rotationToUse);

                            

                        }
                        
                      //  entity.EntityType.VehicleType.DriversEntrance

                        // play a door closing anim on the vehicle                                        //Note backwards                      
                        vehicleEntity.Renderable.StartAdditionalAnimation("driver_entry", Playback.Backwards, StartingPoint.Current, BlendMode.Normal);

                        // get in the seat
                        entity.Renderable.SetAnimationStateFlag(AnimModifier.Crew);
                       // entity.Renderable./*TODO DECOUPLE*/RenderAsModel.StartAnimation("driving", Playback.Forwards, StartingPoint.Current, BlendMode.NoBlending, 1f, Looping.Yes);

                        
                        if (trigger != null)
                        {
                            entity.AttachTrigger(trigger);
                        }

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

        //public static void AttachEntityAndCreateLocalTransform(Entity entity, Entity attachToEntity, float scaling, Vector3? translation, string attachBoneName)
        //{
        //    Matrix translate;
        //    if (translation.HasValue)
        //    {
        //        translate = Matrix.CreateTranslation(translation.Value); 
        //    }
        //    else
        //    {
        //        translate = Matrix.Identity;
        //    }

        //    Matrix scale;
        //    scale = Matrix.CreateScale(scaling); 

        //    Matrix localTransform = scale *  translate;
        //    entity.Renderable./*TODO DECOUPLE*/RenderAsModel.LocalTransform = localTransform;

        //    if (!string.IsNullOrEmpty(attachBoneName))
        //    {
        //        attachToEntity.Renderable.RenderAsModel.AnimatedModel.ModelAnimator.AttachObject(entity.Renderable./*TODO DECOUPLE*/RenderAsModel, attachBoneName);
        //    }
           
        //}
    }
}
