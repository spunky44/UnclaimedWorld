using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Items;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Vehicles;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Entities.Containers.Components;//TODO DECOUPLE
namespace UWGame.SimSide.AI.Goals
{
    public class GoalTakeoff : CompositeGoal
    {
        public Entity Aircraft;
        Vehicle vehicleComponent;

        /*  public Point Destination
          {
              get { return destination; }
              set
              {
                  destination = value;
                  destinationPoint = new Vector2(UWGame.SimSide.Instance.map.tileWidth * (0.5f + destination.X),
                      UWGame.SimSide.Instance.map.tileHeight * (0.5f + destination.Y));
              }

          }
          private Point destination;*/
        private Vector3 destinationPoint;

        /// <summary>
        /// The "close enough" limit, if the tank is inside this many pixel 
        /// to it's destination it's considered at it's destination
        /// // Remember: distance is squared!
        /// </summary>
        const float atDestinationLimit = 16f;

        // TEASER XXX:
       // public const float TargetAltitude = 300f; //200f;



        public GoalTakeoff(Entity owner)
            : base(owner)
        {
           
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

            sn.DoUnknownObject(this.Aircraft);
            this.destinationPoint = sn.DoVector3(destinationPoint);
            sn.DoUnknownObject(this.vehicleComponent);

            return this;
        }

        public GoalTakeoff()
        {
        }



        protected override void Activate()
        {
            //make sure the subgoal list is clear.
            RemoveAllSubgoals();

            if (!entity.GetDrivenVehicle(out Aircraft))
            {
                Status = Goals.Status.Failed;
                return;
            }
            Aircraft.Find(out vehicleComponent);


            Status = Status.Active;
            /*
            destinationPoint = new Vector3(MapManager.tileSize * (0.5f + entity.MapPosition.X),
                    MapManager.tileSize * (0.5f + entity.MapPosition.Y), TargetAltitude);
            */ // NEW: use the aircraft position instead of the pilot's:
            destinationPoint = new Vector3(Aircraft.PlaySiteLocation.X, Aircraft.PlaySiteLocation.Y, ((VehicleContainerType)Aircraft.EntityType.ContainerType).Aircraft.CruiseAltitude);
                   

            /**
            Aircraft.Renderable.RenderAsModel.TryStartAnimation("driver_entry", Playback.Backwards, StartingPoint.Current, RenderAsModel.Mode.Additive);
            Aircraft.Renderable.RenderAsModel.TryStartAnimation("passenger_entry", Playback.Backwards, StartingPoint.Current, RenderAsModel.Mode.Additive);
            Aircraft.Renderable.RenderAsModel.TryStartAnimation("open_for_loading", Playback.Backwards, StartingPoint.Current, RenderAsModel.Mode.Additive);
            */

            /*  AddSubgoal(new GoalStartAnimationAndWaitForEnd(entity, Aircraft, "propeller_right_start", Playback.Forwards, StartingPoint.Current,
                   RenderAsModel.Mode.Additive, false));
              */


            // TODO: rework this... perhaps send a message when the aircraft is ready to take off.
            AddSubgoal(new GoalWait(entity, 3d, null, AnimModifier.Pre, AnimModifier.Right, AnimModifier.Slow)); //, Aircraft.ID));
            AddSubgoal(new GoalWait(entity, 3d, null, AnimModifier.Pre, AnimModifier.Left, AnimModifier.Slow)); //, Aircraft.ID));
            AddSubgoal(new GoalWait(entity, 3d, AnimAction.Containing, AnimModifier.Crew)); //, Aircraft.ID));
            AddSubgoal(new GoalWait(entity, 3d, AnimAction.Containing, AnimModifier.Passenger)); //, Aircraft.ID));
            AddSubgoal(new GoalWait(entity, 3d, AnimAction.Containing, AnimModifier.Open)); //, Aircraft.ID));  


            


            //// close all doors and start the engines:
            //AddSubgoal(new GoalStartAnimationAndWaitForEnd(entity, new List<AnimationToRun>
            //{
            //    new AnimationToRun(){ 
            //         targetEntity = Aircraft,
            //         animationKey = "propeller_right_start", 
            //        AnimationStates = new AnimState[] { AnimState.Pre, AnimState.Right, AnimState.Slow },

            //         playback = Playback.Forwards, //TODO DECOUPLE
            //         startingPoint = StartingPoint.FromBeginning,
            //         mode = BlendMode.Additive, runToEndIfInterrupted = false
            //    },
            //    new AnimationToRun(){ 
            //         targetEntity = Aircraft,
            //         animationKey = "propeller_left_start", 
            //        AnimationStates = new AnimState[] { AnimState.Pre, AnimState.Left, AnimState.Slow },
            //         playback = Playback.Forwards, 
            //         startingPoint = StartingPoint.FromBeginning,
            //         mode = BlendMode.Additive, runToEndIfInterrupted = false
            //    },
            //    new AnimationToRun(){ 
            //         targetEntity = Aircraft,
            //         animationKey = "driver_entry", 
            //        AnimationStates = new AnimState[] { AnimState.Containing, AnimState.Crew },
            //         playback = Playback.Backwards, //TODO not sure why this is backwards
            //         startingPoint = StartingPoint.Current,
            //         mode = BlendMode.Additive, runToEndIfInterrupted = false
            //    },
            //    new AnimationToRun(){ 
            //         targetEntity = Aircraft,
            //         animationKey = "passenger_entry", 
            //        AnimationStates = new AnimState[] { AnimState.Containing, AnimState.Passenger },
            //         playback = Playback.Backwards, //TODO not sure why this is backwards
            //         startingPoint = StartingPoint.Current,
            //         mode = BlendMode.Additive, runToEndIfInterrupted = false
            //    },
            //    new AnimationToRun(){ 
            //         targetEntity = Aircraft,
            //         animationKey = "open_for_loading", 
            //        AnimationStates = new AnimState[] { AnimState.Open },
            //         playback = Playback.Backwards, //TODO not sure why this is backwards
            //         startingPoint = StartingPoint.Current,
            //         mode = BlendMode.Additive, runToEndIfInterrupted = false
            //    }
            //}
            //));
        }

        public static bool UpdateAircraftUpDown(float desiredMoveSpeed, Microsoft.Xna.Framework.GameTime elapsed, Entity pilot, Entity aircraft, Vehicle vehicleComponent, Vector3 destinationPoint)
        {
            float elapsedTime = (float)elapsed.ElapsedGameTime.TotalSeconds;

            Vector2 planeAdjustment = Vector2.Zero;
            float previousMoveSpeed = vehicleComponent.Aircraft.VerticalSpeed;

            // do vertical accel/decel:
            double distToTarget = Vector3.Distance(destinationPoint, aircraft.PlaySiteLocation);
            if (distToTarget > 0)
            {
                VehicleContainerType vehicleContainerType = (VehicleContainerType)aircraft.EntityType.ContainerType;
                double speed = MathHelper.Clamp(desiredMoveSpeed,
                    previousMoveSpeed - vehicleContainerType.Aircraft.MaxVerticalAcceleration * elapsedTime,
                    previousMoveSpeed + vehicleContainerType.Aircraft.MaxVerticalAcceleration * elapsedTime);

                // decelerate:
                speed = (Math.Sign(speed)) * Math.Min(distToTarget / vehicleContainerType.Deceleration, Math.Abs(speed));

                vehicleComponent.Aircraft.VerticalSpeed = (float)speed;

            }

            // make a small adjustment to x and y pos if needed:
            float maxAdjustment = 10f * elapsedTime;
            planeAdjustment.X = MathHelper.Clamp(destinationPoint.X - aircraft.PlaySiteLocation.X, -maxAdjustment, maxAdjustment);
            planeAdjustment.Y = MathHelper.Clamp(destinationPoint.Y - aircraft.PlaySiteLocation.Y, -maxAdjustment, maxAdjustment);

            // move the aircraft:  
            pilot.Location = aircraft.Location + new Vector3(planeAdjustment, vehicleComponent.Aircraft.VerticalSpeed * elapsedTime);
           // aircraft.Location = aircraft.Location + new Vector3(planeAdjustment, vehicleComponent.Aircraft.VerticalSpeed * elapsedTime);
                        
            
            if (aircraft.Renderable.RenderAsModel.ModelData.HasAircraftDucts)
            {
                GoalFlyToPosition.SetLeftDuctAngle(aircraft, vehicleComponent, 0f, elapsedTime);
                GoalFlyToPosition.SetRightDuctAngle(aircraft, vehicleComponent, 0f, elapsedTime);

                // use anims instead...
                //GoalFlyToPosition.UpdatePropellers(Aircraft, elapsedTime, GoalFlyToPosition.PropDirection.Start);
            }

            if (Vector3.DistanceSquared(aircraft.PlaySiteLocation, destinationPoint)
                      < atDestinationLimit)
            {
                return true;     // target reached           
            }

            return false;

        }

        protected override void ProcessWhileActive(GameTime elapsed)
        {
            //if status is inactive, call Activate()           
         /*   ActivateIfInactive();

            if (Status == Goals.Status.Active)
            {*/
                Status = ProcessSubgoals(elapsed);

                if (Status == Status.Completed)
                {
                    Aircraft.Renderable.RenderAsModel.AnimatedModel.ModelAnimator.ShowMesh("propeller_left", false);
                    Aircraft.Renderable.RenderAsModel.AnimatedModel.ModelAnimator.ShowMesh("propeller_right", false);

                    float desiredMoveSpeed = ((VehicleContainerType)Aircraft.EntityType.ContainerType).Aircraft.MaxVerticalMoveSpeed;


                    if (UpdateAircraftUpDown(desiredMoveSpeed, elapsed, entity, Aircraft, vehicleComponent, destinationPoint))
                    {
                        Aircraft.Locomotor.MoveSpeed = 0f;
                        vehicleComponent.Aircraft.VerticalSpeed = 0f;
                        Status = Status.Completed;
                    }
                    else
                    {
                        AddSkimmerDust(Aircraft);

                        Status = Status.Active;
                    }
                }
           /* }

            ExitIfFailedOrCompleted();

            return Status;*/
        }

        public static void AddSkimmerDust(Entity aircraft)
        {
            float altitudeForDust = ((VehicleContainerType)aircraft.EntityType.ContainerType).Aircraft.AltitudeForDust;
            float dustIntensity = Math.Max((altitudeForDust - aircraft.PlaySiteLocation.Z) / altitudeForDust, 0f);
            dustIntensity *= The.Map.GetTile(aircraft.MapPosition.Value).GetDustFactor();

            The.Client.ParticleManager.AddDust(aircraft, aircraft.PlaySiteLocation, dustIntensity, GameData.Instance.Constants.SkimmerDustScale);
        }
    }
}
