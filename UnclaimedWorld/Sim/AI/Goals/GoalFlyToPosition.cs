using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Items;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Vehicles;
using Xclna.Xna.Animation;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Entities.Containers.Components;//TODO DECOUPLE
namespace UWGame.SimSide.AI.Goals
{
   
    public class GoalFlyToPosition: Goal
    {
        bool isAtTarget = false;

        public Entity Aircraft;
        Vehicle vehicleComponent;


        private bool hasLoweredLandingGear = false;

        public Point Destination
        {
            get { return destination; }
            set
            {
                destination = value;
                destinationPoint = new Vector2(MapManager.tileSize * (0.5f + destination.X),
                    MapManager.tileSize * (0.5f + destination.Y));
            }

        }
        private Point destination;
        private Vector2 destinationPoint;

        /// <summary>
        /// The "close enough" limit, if the tank is inside this many pixel 
        /// to it's destination it's considered at it's destination
        /// </summary>
        const float atDestinationLimit = 16f;

      
        public GoalFlyToPosition(Entity owner, Point destination)
            : base(owner)
        {
            Destination = destination;
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

            this.isAtTarget = sn.DoBool(isAtTarget);
            sn.DoUnknownObject(Aircraft); //TODO use lookup
            sn.DoUnknownObject(vehicleComponent);//TODO use lookup
            this.hasLoweredLandingGear = sn.DoBool(hasLoweredLandingGear);
            this.destination = sn.DoPoint(destination);
            this.destinationPoint = sn.DoVector2(destinationPoint);

            return this;
        }

        public GoalFlyToPosition()
        {
        }

        public override bool IsSame(Jobs.Job job)
        {
            return false;
        }
        protected override void Activate()
        {
            if (!entity.GetDrivenVehicle(out Aircraft))
            {
                Status = Goals.Status.Failed;
                return;
            }            
            
            Aircraft.Find(out vehicleComponent);


            SelectSteeringType();
            Status = Status.Active;

           


            Aircraft.Renderable.StartAdditionalAnimation("raiselandinggear", Playback.Forwards, StartingPoint.Current, BlendMode.Normal); //TODO DECOUPLE
                        
            // necessary?
            Aircraft.Renderable.RenderAsModel.AnimatedModel.ModelAnimator.ShowMesh("propeller_left", false);
            Aircraft.Renderable.RenderAsModel.AnimatedModel.ModelAnimator.ShowMesh("propeller_right", false);


       /*     if (UWGame.SimSide.Instance.map.TileMap[entity.MapPosition.X, entity.MapPosition.Y].ContainsItem(meal))
            {
                Status = Status.Active;
                secondsToEat = Common.RandomSpread(Globals.Instance.Random, baseSecondsToEat, secondsToEatSpread);
                
            }
            else
            {
                // cannot get the item. Fail.
                Status = Status.Failed;
            }
            */
        }

        protected override void ProcessWhileActive(GameTime elapsed)
        {
            //if status is inactive, call Activate()
            // May fail if no item:
         /*   ActivateIfInactive();

            if (Status == Status.Active)
            {*/
                float elapsedTime = (float)elapsed.ElapsedGameTime.TotalSeconds;

                if (!isAtTarget)
                {
                    
                    // This code causes the tank to change its speed gradually while it 
                    // moves toward the waypoint previousMoveSpeed tracks how fast the 
                    // tank was going, desiredMoveSpeed finds how fast the tank want to 
                    // go and Math.Clamp keeps the tank from accelerating or decelerating 
                    // too fast.
                    float previousMoveSpeed = Aircraft.Locomotor.MoveSpeed;

                    float desiredMoveSpeed = 0f;

                    float headingDifference = 0f;
                    if (vehicleComponent.Aircraft.SteeringType == SteeringType.HardTurn)
                    {
                        desiredMoveSpeed = FindMaxMoveSpeedForHardTurn(destinationPoint, elapsedTime, out headingDifference);
                    }
                    else if (vehicleComponent.Aircraft.SteeringType == SteeringType.Pivot)
                    {
                        desiredMoveSpeed = FindPivotSpeed(elapsedTime);
                    }
                    else if (vehicleComponent.Aircraft.SteeringType == SteeringType.SlowTurn)
                    {
                        desiredMoveSpeed = FindSlowCircleSpeed(elapsedTime, out headingDifference);
                    }


                    // do accel/decel:
                    // measure distance at aircraft height!
                    double distToTarget = Vector2.Distance(destinationPoint, new Vector2(Aircraft.Location.Value.X, Aircraft.Location.Value.Y));  //Vector3.Distance(new Vector3(destinationPoint, 0), Aircraft.Location);
                    if (distToTarget > 0)
                    {
                        VehicleContainerType vehicleContainerType = (VehicleContainerType)Aircraft.EntityType.ContainerType;

                        double speed = MathHelper.Clamp(desiredMoveSpeed,
                            previousMoveSpeed - vehicleContainerType.MaxAcceleration * elapsedTime,
                            previousMoveSpeed + vehicleContainerType.MaxAcceleration * elapsedTime);

                        // decelerate:
                        
                        // don't stop completely before we hit the destination:
                        speed = Math.Min((distToTarget + atDestinationLimit) / vehicleContainerType.Deceleration, speed);

                        Aircraft.Locomotor.MoveSpeed = (float)speed;

                    }

                /*    float deltaSpeed = Aircraft.Locomotor.MoveSpeed - previousMoveSpeed;
                    float acceleration = deltaSpeed / elapsedTime;*/

                    SetPitch(previousMoveSpeed, elapsedTime);

                    if (Aircraft.Renderable.RenderAsModel.ModelData.HasAircraftDucts)
                    {
                        SetFansAngle(previousMoveSpeed, headingDifference, elapsedTime);
                    }


                    // move the aircraft:
                    // this will update the aircraft locaion too
                  //  Aircraft.Location = Aircraft.Location + (new Vector3(Aircraft.Vehicle.Direction.X, Aircraft.Vehicle.Direction.Y, 0) * Aircraft.Locomotor.MoveSpeed * elapsedTime);
                    entity.Location = Aircraft.Location + (new Vector3(Aircraft.FacingNormal.X, Aircraft.FacingNormal.Y, 0) * Aircraft.Locomotor.MoveSpeed * elapsedTime);

               
                                                            
                    
                    if (distToTarget < 180 && !hasLoweredLandingGear) 
                    { // lower landing gear
                        hasLoweredLandingGear = true;
                        Aircraft.Renderable.StartAdditionalAnimation("raiselandinggear", Playback.Backwards, StartingPoint.Current, BlendMode.Normal);
                    }

                    float distanceSquaredToTarget = Vector2.DistanceSquared(new Vector2(Aircraft.PlaySiteLocation.X, Aircraft.PlaySiteLocation.Y), destinationPoint);
                    if (distanceSquaredToTarget < atDestinationLimit)
                    {
                        Aircraft.Locomotor.MoveSpeed = 0f;
                       // Aircraft.Renderable.RenderAsModel.RotationSpeed = 0f;
                        isAtTarget = true;

                    }
                }
                else
                {
                    if (Aircraft.Renderable.RenderAsModel.ModelData.HasAircraftDucts)
                    {
                        SetLeftDuctAngle(Aircraft, vehicleComponent, 0f, elapsedTime);
                        SetRightDuctAngle(Aircraft, vehicleComponent, 0f, elapsedTime);
                    }

                    bool rollIsZero = Common.IsEqual(vehicleComponent.Roll, 0f);
                    bool pitchIsZero = Common.IsEqual(vehicleComponent.Pitch, 0f);
                    if (rollIsZero && pitchIsZero)
                    {
                        vehicleComponent.Roll = 0f;
                        vehicleComponent.Pitch = 0f;
                        Status = Status.Completed;
                    }
                    else
                    { 
                        if (!rollIsZero)
                        {
                            // right the aircraft:
                            float rollDifference = 0f - vehicleComponent.Roll;
                            rollDifference = MathHelper.Clamp(rollDifference, -elapsedTime * 0.3f, elapsedTime * 0.3f);
                            vehicleComponent.Roll += rollDifference;
                        }

                        if (!pitchIsZero)
                        {
                            float pitchDifference = 0f - vehicleComponent.Pitch;
                            pitchDifference = MathHelper.Clamp(pitchDifference, -elapsedTime * 0.3f, elapsedTime * 0.3f);
                            vehicleComponent.Pitch += pitchDifference;
                        }
                    }
                }
                
           /* }

            ExitIfFailedOrCompleted();

            return Status;*/
        }

        private void SetPitch(float previousMoveSpeed, float elapsedTime)
        {
            VehicleContainerType vehicleContainerType = (VehicleContainerType)Aircraft.EntityType.ContainerType;

            float deltaSpeed = Aircraft.Locomotor.MoveSpeed - previousMoveSpeed;

            // add accel to overcome drag: 
            // oscillates...
           // float speedToCompensateForDrag = 0.5f * Aircraft.Locomotor.MoveSpeed / Aircraft.EntityType.VehicleType.Aircraft.MaxAirSpeed; 

            float rotateToCompensateForDrag = 0.5f * Aircraft.Locomotor.MoveSpeed / vehicleContainerType.Aircraft.MaxAirSpeed; 

            float acceleration = deltaSpeed / elapsedTime; //(deltaSpeed + speedToCompensateForDrag) / elapsedTime;

            float rotate;

            if (acceleration + rotateToCompensateForDrag > 0)
            {
                rotate = MathHelper.Lerp(0, vehicleContainerType.Aircraft.MaxPitchInRadians, acceleration / 50 + rotateToCompensateForDrag);
            }
            else
            {
                rotate = MathHelper.Lerp(-vehicleContainerType.Aircraft.MaxPitchInRadians, 0, -acceleration / 50 + rotateToCompensateForDrag);
            }

         /*   if (acceleration > 0)
            {
                rotate = MathHelper.Lerp(0, Aircraft.EntityType.VehicleType.Aircraft.MaxPitchInRadians, acceleration / 50); 
            }
            else
            {
                rotate = MathHelper.Lerp(-Aircraft.EntityType.VehicleType.Aircraft.MaxPitchInRadians, 0, -acceleration / 50);
            }*/

            float pitchDifference = rotate - vehicleComponent.Pitch;

            float maxPitchChange = elapsedTime * vehicleContainerType.Aircraft.PitchChangeSpeed;
            pitchDifference = MathHelper.Clamp(pitchDifference, -maxPitchChange, maxPitchChange);

         /*   if (Math.Abs(pitchDifference) > 0.015) // let's ignore small oscillations... 
            {*/
                float newRotate = vehicleComponent.Pitch + pitchDifference;

                vehicleComponent.Pitch = newRotate;
           // }
        }

        private void SetFansAngle(float previousMoveSpeed, float headingDifference, float elapsedTime)
        {
            VehicleContainerType vehicleContainerType = (VehicleContainerType)Aircraft.EntityType.ContainerType;

            float deltaSpeed = Aircraft.Locomotor.MoveSpeed - previousMoveSpeed;

       
            // add accel to overcome drag: - oscillates violently 
            //float speedToCompensateForDrag = 0.5f * Aircraft.Locomotor.MoveSpeed / Aircraft.EntityType.VehicleType.Aircraft.MaxAirSpeed; // 0.2f; // (float)Math.Pow(((double)Aircraft.Locomotor.MoveSpeed), 2.0) / 1000f;

            float rotateToCompensateForDrag = 0.5f * Aircraft.Locomotor.MoveSpeed / vehicleContainerType.Aircraft.MaxAirSpeed; 


            //float acceleration = (deltaSpeed + speedToCompensateForDrag) / elapsedTime;            
            float acceleration = deltaSpeed / elapsedTime;            


          //  float rotate = (acceleration / 50f) * MathHelper.PiOver4;

            float rotate;
            if (acceleration + rotateToCompensateForDrag > 0)
            {
                rotate = MathHelper.Lerp(0, MathHelper.PiOver4, acceleration / 50 + rotateToCompensateForDrag);
            }
            else
            {
                rotate = MathHelper.Lerp(-MathHelper.PiOver4, 0, -acceleration / 50 + rotateToCompensateForDrag);
            }

            /*
            if (acceleration > 0)
            {
                rotate = MathHelper.Lerp(0, MathHelper.PiOver4, acceleration / 50); 
            }
            else
            {
                rotate = MathHelper.Lerp(-MathHelper.PiOver4, 0, -acceleration / 50);
            }
               */

           
          /*  float leftRightRotateDifference = headingDifference
*/
            if (!Common.IsEqual(headingDifference, 0f))
            {
                //rollDifference = MathHelper.Clamp(rollDifference, -0.5f * elapsedTime, 0.5f * elapsedTime);
               
            }

            headingDifference = MathHelper.Clamp(headingDifference, -0.3f, 0.3f);

            SetLeftDuctAngle(Aircraft, vehicleComponent, rotate + headingDifference, elapsedTime);

            SetRightDuctAngle(Aircraft, vehicleComponent, rotate - headingDifference, elapsedTime);           
            
        }



        private static void SetPropellerAngle(Entity aircraft, ref Matrix rotateTransform, string boneName) // BonePose propeller)
        {
            aircraft.Renderable.SetModelBoneRotation(boneName, rotateTransform);
            /*
            propeller.MainAnimationTrack = null;
            propeller.CurrentBlendController = null;

            propeller.DefaultTransform = rotateTransform *
                Matrix.CreateTranslation(propeller.DefaultTransform.Translation);*/
        }

        public enum PropDirection { Start, Stop }
        public static void UpdatePropellers(Entity aircraft, Vehicle vehicleComponent, float elapsedTime, PropDirection direction)
        {
            VehicleContainerType vehicleContainerType = (VehicleContainerType)aircraft.EntityType.ContainerType;

            float newRotate = elapsedTime * vehicleComponent.Aircraft.PropellerSpeed; //

            float dir = (direction == PropDirection.Start ? 1f : -1f);
            vehicleComponent.Aircraft.PropellerSpeed = MathHelper.Clamp(
                    vehicleComponent.Aircraft.PropellerSpeed + dir * elapsedTime * vehicleContainerType.Aircraft.PropellerAcceleration,
                    0f,
                    vehicleContainerType.Aircraft.MaxPropellerSpeed);

            Matrix rotateTransform = Matrix.CreateFromYawPitchRoll(MathHelper.PiOver2, 0f, newRotate + MathHelper.PiOver2); 

           // BonePose propeller = aircraft.Renderable.RenderAsModel.AnimatedModel.ModelAnimator.BonePoses["propeller_joint_right"];
            SetPropellerAngle(aircraft, ref rotateTransform, "propeller_joint_right");
              
            //propeller = aircraft.Renderable.RenderAsModel.AnimatedModel.ModelAnimator.BonePoses["propeller_joint_left"];
            SetPropellerAngle(aircraft, ref rotateTransform, "propeller_joint_left");
        }

        public static void SetLeftDuctAngle(Entity aircraft, Vehicle vehicleComponent, float rotate, float elapsedTime)
        {
           // Vehicle vehicleComponent;
           // aircraft.Find(out vehicleComponent);

            rotate -= vehicleComponent.Pitch;

         //   BonePose leftDuct = aircraft.Renderable.RenderAsModel.AnimatedModel.ModelAnimator.BonePoses["jet_engine_joint"]; // wrong naming !! left!

            vehicleComponent.Aircraft.LeftDuctFanAngle = SetDuctAngle(aircraft, vehicleComponent, vehicleComponent.Aircraft.LeftDuctFanAngle, rotate, "jet_engine_joint", elapsedTime);
            
        }

        public static void SetRightDuctAngle(Entity Aircraft, Vehicle vehicleComponent, float rotate, float elapsedTime)
        {
            rotate -= vehicleComponent.Pitch;

          //  BonePose rightDuct = Aircraft.Renderable.RenderAsModel.AnimatedModel.ModelAnimator.BonePoses["jet_engine_joint_right"];

            vehicleComponent.Aircraft.RightDuctFanAngle = SetDuctAngle(Aircraft, vehicleComponent, vehicleComponent.Aircraft.RightDuctFanAngle, rotate, "jet_engine_joint_right", elapsedTime);
        }

        private static float SetDuctAngle(Entity aircraft, Vehicle vehicleComponent, float currentAngle, float rotate, /*BonePose*/ string duct, float elapsedTime)
        {
            VehicleContainerType vehicleContainerType = (VehicleContainerType)aircraft.EntityType.ContainerType;

            float rollDifference = rotate - currentAngle;


            float maxDuctAngleChange = elapsedTime * vehicleContainerType.Aircraft.DuctChangeAngleSpeed;
            rollDifference = MathHelper.Clamp(rollDifference, -maxDuctAngleChange, maxDuctAngleChange);

          /*  if (Math.Abs(rollDifference) > 0.03) // let's ignore small oscillations... 
            {*/

            float newRotate = currentAngle + rollDifference;

            aircraft.Renderable.SetModelBoneRotation(duct, Matrix.CreateFromYawPitchRoll(MathHelper.PiOver2, 0f, newRotate + MathHelper.PiOver2));
               
            /*
            duct.DefaultTransform =
                Matrix.CreateFromYawPitchRoll(MathHelper.PiOver2, 0f, newRotate + MathHelper.PiOver2) *
                Matrix.CreateTranslation(duct.DefaultTransform.Translation);
            */

            return newRotate;

           // }
          //  else return currentAngle;
        }


        public void SelectSteeringType()
        {

            if (Vector2.Dot(destinationPoint - new Vector2(Aircraft.PlaySiteLocation.X, Aircraft.PlaySiteLocation.Y), new Vector2(Aircraft.FacingNormal.X, Aircraft.FacingNormal.Y)) > 0)
                //Vector2.Dot(destinationPoint - new Vector2(Aircraft.Location.X, Aircraft.Location.Y), new Vector2(Aircraft.Vehicle.Direction.X, Aircraft.Vehicle.Direction.Y)) > 0)
            {
                // if the curve is long:
                if (IsWaypointWithinTurningRadiusAtMaxSpeed(destinationPoint))
                {
                    // make a long curve to the destination:
                    vehicleComponent.Aircraft.SteeringType = SteeringType.SlowTurn;
                }
                else
                {   // make a hard turn, slowing down to get there:
                    vehicleComponent.Aircraft.SteeringType = SteeringType.HardTurn;
                }

            }
            else
            {   // pivot in place, then move straight:
                vehicleComponent.Aircraft.SteeringType = SteeringType.Pivot;
            }

        }


      

        private float FindPivotSpeed(float elapsedTime)
        {
            Aircraft.Locomotor.MoveSpeed = 0;

            // This code causes the tank to turn towards the waypoint.
            float facingDirection; 
            float headingDifference;
            facingDirection = TurnToFace(Aircraft.PlaySiteLocation, destinationPoint,
                    Aircraft.Rotation, Aircraft.EntityType.LocomotorType.MaxAngularSpeed * elapsedTime, out headingDifference);

           // this.Aircraft.Renderable.RenderAsModel.SetRotationSpeed(this.Aircraft.Locomotor.Rotation, facingDirection, elapsedTime);
            // update rotation:
            this.Aircraft.Rotation = facingDirection;
          
            Aircraft.FacingNormal = new Vector3(
                (float)Math.Cos(facingDirection),
                (float)Math.Sin(facingDirection), 0);

            if (Math.Abs(headingDifference) < 0.2f)
            {
                VehicleContainerType vehicleContainerType = (VehicleContainerType)Aircraft.EntityType.ContainerType;

                return vehicleContainerType.Aircraft.MaxAirSpeed; // start...
            }
            else
            {
                return 0f; // stay put
            }
        }

        /// <summary>
        /// Estimate the Tank's best possible movement speed to it's destination
        /// </summary>
        /// <param name="newDestination">The Tank's target location</param>
        /// <returns>Maximum estimated movement speed for the Tank 
        /// up to Tank.MaxMoveSpeed</returns>
        private float FindMaxMoveSpeedForHardTurn(Vector2 waypoint, float elapsedTime, out float headingDifference)
        {
            //float r = FindRadius(waypoint);
            VehicleContainerType vehicleContainerType = (VehicleContainerType)Aircraft.EntityType.ContainerType;

            float finalSpeed = vehicleContainerType.Aircraft.MaxAirSpeed;
           
            // If closestDistance is less than turningRadius, then the waypoint is 
            // inside one of the 2 circles the Tank cannot turn into when moving at 
            // Tank.MaxMoveSpeed, instead we need to estimate a speed that the tank 
            // can move at.
           // if (closestDistance < turningRadius) 
            if (!IsWaypointWithinTurningRadiusAtMaxSpeed(waypoint))
            {
                // This finds the radius of a circle where the Tank's location and 
                // the waypoint are 2 points on opposite sides of the circle.
                float radius = Vector2.Distance(new Vector2(Aircraft.PlaySiteLocation.X, Aircraft.PlaySiteLocation.Y), waypoint) / 2;
                // Now we use the radius from above to and Tank.MaxAngularVelocity 
                // to find out how fast we can move towards the waypoint by taking 
                // r = v/w and turning it into v = r*w
                finalSpeed = Aircraft.EntityType.LocomotorType.MaxAngularSpeed * radius;
            }

            // This code causes the tank to turn towards the waypoint.  First we 
            // take the vector that represents the tanks’ current heading, 
            // Tank.Direction, and convert it into an angle in radians. Then we 
            // use TurnToFace to make the tank turn towards it’s waypoint based 
            // on it’s turning speed, Tank.MaxAngularVelocity. After we have the 
            // new direction in radian we convert it back into a vector.
            float facingDirection; // = (float)Math.Atan2(Aircraft.Direction.Y, Aircraft.Direction.X);


            facingDirection = TurnToFace(Aircraft.PlaySiteLocation, destinationPoint,
                    Aircraft.Rotation, Aircraft.EntityType.LocomotorType.MaxAngularSpeed * elapsedTime, out headingDifference);

          //  this.Aircraft.Renderable.RenderAsModel.SetRotationSpeed(this.Aircraft.Locomotor.Rotation, facingDirection, elapsedTime);
            // update rotation:
            this.Aircraft.Rotation = facingDirection;
          /*  Aircraft.Vehicle.Direction = new Vector3(
                (float)Math.Cos(facingDirection),
                (float)Math.Sin(facingDirection), 0);*/
            Aircraft.FacingNormal = new Vector3(
                (float)Math.Cos(facingDirection),
                (float)Math.Sin(facingDirection), 0);

            // update roll/banking:
            SetRoll(headingDifference, elapsedTime);

            return finalSpeed;
        }


        private bool IsWaypointWithinTurningRadiusAtMaxSpeed(Vector2 waypoint)
        {
            VehicleContainerType vehicleContainerType = (VehicleContainerType)Aircraft.EntityType.ContainerType;

            float finalSpeed = vehicleContainerType.Aircraft.MaxAirSpeed;
            // Given a velocity v (Tank.MaxMoveSpeed) and an angular velocity 
            // w(Tank.MaxAngularVelocity), the smallest turning radius 
            // r(turningRadius) ofthe tank is the velocity divided by the turning
            // speed: r = v/w 
            float turningRadius = vehicleContainerType.Aircraft.MaxAirSpeed / Aircraft.EntityType.LocomotorType.MaxAngularSpeed;

            // This code figures out if the tank can move to its waypoint from its 
            // current location based on its turning circle(turningRadius) when its 
            // moving as fast as possible(Tank.MaxMoveSpeed). For any given turning 
            // circle there is an area to either side of the tank that it cannot 
            // move into that can be represented by 2 circles of radius turningRadius 
            // on either side of the tank. If the waypoint is inside one of these 
            // 2 circles the tank will have to slow down before it can move to it

            // This creates a vector that’s orthogonal to the tank in the direction 
            // it's facing. This means that the vector is at a right angle to the 
            // direction the tank is pointing in.
            //Vector2 orth = new Vector2(Aircraft.Vehicle.Direction.Y, -Aircraft.Vehicle.Direction.X);
            Vector2 orth = new Vector2(Aircraft.FacingNormal.Y, -Aircraft.FacingNormal.X);

            // In this code we can combine the tanks’ location, the orthogonal 
            // vector and the tanks’ turning radius to find the 2 points that 
            // describe the centers of the circles the tanks cannot move into. 
            // Then we use Vector2.Distance to find the distances from each circle 
            // center to the waypoint. Afterwards Math.Min return the distance from 
            // the waypoint to whichever circle was closest.
            Vector2 loc = Aircraft.PlaySiteLocation.ToVector2();
            float closestDistance = Math.Min(
                Vector2.Distance(waypoint, loc + (orth * turningRadius)),
                Vector2.Distance(waypoint, loc - (orth * turningRadius)));

            return closestDistance >= turningRadius;
        }


        private float FindSlowCircleSpeed(float elapsedTime, out float headingDifference)
        {
            VehicleContainerType vehicleContainerType = (VehicleContainerType)Aircraft.EntityType.ContainerType;

            // if we are overshooting the target, drop this and make a hard turn:
            if (!IsWaypointWithinTurningRadiusAtMaxSpeed(destinationPoint))
            {
                headingDifference = 0f;
                vehicleComponent.Aircraft.SteeringType = SteeringType.HardTurn;
                return vehicleContainerType.Aircraft.MaxAirSpeed;
            }

            float circleRadius = FindSlowCircleRadius(destinationPoint);

            // Now we use the radius from above to and Tank.MaxAngularVelocity 
            // to find out how fast we can move towards the waypoint by taking 
            // r = v/w and turning it into v = r*w
            float angularSpeed = vehicleContainerType.Aircraft.MaxAirSpeed / circleRadius;

            float facingDirection;


            facingDirection = TurnToFace(Aircraft.PlaySiteLocation, destinationPoint,
                    Aircraft.Rotation, angularSpeed * elapsedTime, out headingDifference);

           // this.Aircraft.Renderable.RenderAsModel.SetRotationSpeed(this.Aircraft.Locomotor.Rotation, facingDirection, elapsedTime);
            // update rotation:
            this.Aircraft.Rotation = facingDirection;           
            Aircraft.FacingNormal = new Vector3(
                (float)Math.Cos(facingDirection),
                (float)Math.Sin(facingDirection), 0);

            // update roll/banking:
            SetRoll(headingDifference, elapsedTime);



            return vehicleContainerType.Aircraft.MaxAirSpeed; // move at full speed
        }

        private void SetRoll(float headingDifference, float elapsedTime)
        {
            VehicleContainerType vehicleContainerType = (VehicleContainerType)Aircraft.EntityType.ContainerType;

            float desiredRoll = 5f * (Aircraft.Locomotor.MoveSpeed / vehicleContainerType.Aircraft.MaxAirSpeed) * headingDifference; // Math.Sign(headingDifference);
            float rollDifference = desiredRoll - vehicleComponent.Roll;

            if (!Common.IsEqual(rollDifference, 0f))
            {
                rollDifference = MathHelper.Clamp(rollDifference, -0.5f * elapsedTime, 0.5f * elapsedTime);
                vehicleComponent.Roll += rollDifference;

                vehicleComponent.Roll = MathHelper.Clamp(vehicleComponent.Roll, -vehicleContainerType.Aircraft.MaxRollDegreeWhenTurning, 
                    vehicleContainerType.Aircraft.MaxRollDegreeWhenTurning);
                //Aircraft.Vehicle.Roll = MathHelper.Clamp(Aircraft.Vehicle.Roll, 0f, Aircraft.EntityType.VehicleType.Aircraft.MaxRollDegreeWhenTurning);
            }
        }

        private float FindSlowCircleRadius(Vector2 waypoint)
        {
            Vector2 A = new Vector2(Aircraft.PlaySiteLocation.X, Aircraft.PlaySiteLocation.Y);
            // the vector perpendicular to the tangent passes through the center C:
           // Vector2 AC = new Vector2(-Aircraft.Vehicle.Direction.Y, Aircraft.Vehicle.Direction.X);
            Vector2 AC = new Vector2(-Aircraft.FacingNormal.Y, Aircraft.FacingNormal.X);

            // circle chord:
            Vector2 AB = waypoint - A;
  
            // circle center C is perpendicular to the midpoint of the chord:
            Vector2 M = AB / 2f;
            Vector2 MC = new Vector2(-M.Y, M.X);

       //     float r = (AC.Y * AB.X - AC.X * AB.Y) / (AC.X * MC.Y - AC.Y * MC.X);
            Vector2 mPoint = A + M;

            Vector2 result = Vector2.Zero;
            // compute the intersection of the two lines:
            Common.IntersectionOfTwoLines(A, A + 10000f * AC, mPoint, mPoint + 10000f * MC, ref result);

            Vector2 vectorToIntersection = A - result;

            return vectorToIntersection.Length();

       //OLD:     return result.Length();

        }




     /*   public static float TurnToFace(Vector3 position, Vector2 faceThis,
            float currentAngle, float turnSpeed, out float headingDifference)
        {
            return TurnToFace(new Vector2(position.X, position.Y), faceThis, currentAngle, turnSpeed, out headingDifference);
        }
        */
        /// <summary>
        /// Calculates the angle that an object should face, given its position, its
        /// target's position, its current angle, and its maximum turning speed.
        /// </summary>
        public static float TurnToFace(Vector3 position, Vector2 faceThis,
            float currentAngle, float turnSpeed, out float headingDifference)
        {
            // consider this diagram:
            //         C 
            //        /|
            //      /  |
            //    /    | y
            //  / o    |
            // S--------
            //     x
            // 
            // where S is the position of the spot light, C is the position of the cat,
            // and "o" is the angle that the spot light should be facing in order to 
            // point at the cat. we need to know what o is. using trig, we know that
            //      tan(theta)       = opposite / adjacent
            //      tan(o)           = y / x
            // if we take the arctan of both sides of this equation...
            //      arctan( tan(o) ) = arctan( y / x )
            //      o                = arctan( y / x )
            // so, we can use x and y to find o, our "desiredAngle."
            // x and y are just the differences in position between the two objects.
            float x = faceThis.X - position.X;
            float y = faceThis.Y - position.Y;

            // we'll use the Atan2 function. Atan will calculates the arc tangent of 
            // y / x for us, and has the added benefit that it will use the signs of x
            // and y to determine what cartesian quadrant to put the result in.
            // http://msdn2.microsoft.com/en-us/library/system.math.atan2.aspx
            float desiredAngle = (float)Math.Atan2(y, x);

            // so now we know where we WANT to be facing, and where we ARE facing...
            // if we weren't constrained by turnSpeed, this would be easy: we'd just 
            // return desiredAngle.
            // instead, we have to calculate how much we WANT to turn, and then make
            // sure that's not more than turnSpeed.

            // first, figure out how much we want to turn, using WrapAngle to get our
            // result from -Pi to Pi ( -180 degrees to 180 degrees )
            float difference = Common.WrapAngleBetweenMinusPiAndPi(desiredAngle - currentAngle);
            headingDifference = difference;
          
            // clamp that between -turnSpeed and turnSpeed.
            difference = MathHelper.Clamp(difference, -turnSpeed, turnSpeed);

            // so, the closest we can get to our target is currentAngle + difference.
            // return that, using WrapAngle again.
            // try this instead so we are consistent with the rotation values that we use.
            return Common.WrapAngleBetweenZeroAndTwoPi(currentAngle + difference); //Common.WrapAngleBetweenMinusPiAndPi(currentAngle + difference);
        }

     
        /// <summary>
        /// True when the tank is "close enough" to it's destination
        /// </summary>
        public bool IsAtDestination()
        {
            float distanceToDestination = Vector2.DistanceSquared(new Vector2(Aircraft.PlaySiteLocation.X, Aircraft.PlaySiteLocation.Y), destinationPoint);
            return distanceToDestination < atDestinationLimit;
        }

      
    }
}
