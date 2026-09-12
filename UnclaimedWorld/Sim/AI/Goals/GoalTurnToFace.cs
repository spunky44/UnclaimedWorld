using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Items;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Vehicles;
using GameStateManagement;
using UWGame.Control;
using UWGame.SimSide.Snapshots;
namespace UWGame.SimSide.AI.Goals
{
    /// <summary>
    /// with parameters, this goal can make the head turn while the body is turning
    /// 
    /// TODO: it cannot head turn beyond 90 degrees!
    /// 
    /// TODO: create proper animation for turning, that also turns the head!
    /// </summary>
    public class GoalTurnToFace: Goal
    {
        private Vector2 pointToFace;

        private EntityID? entityToFace;


        private bool setCenterOfAttentionToTurnTarget;
        private float interestLevelToSet;

        private bool turnCompletely;
        private float allowedRotationMargin;

        public GoalTurnToFace(Entity owner, Vector2? pointToFace, EntityID? entityToFace = null, bool turnCompletely = true, bool setCenterOfAttentionToTurnTarget = true, float? interestLevel = null)
            : base(owner)
        {
            if (pointToFace.HasValue)
            {
                this.pointToFace = pointToFace.Value;
            }

            this.entityToFace = entityToFace;
            this.turnCompletely = turnCompletely;


            if (interestLevel == null)
            {
                interestLevelToSet = GameData.Instance.Constants.InterestLevelForTurnToFace;
            }
            else
            {
                interestLevelToSet = interestLevel.Value;
            }

            this.setCenterOfAttentionToTurnTarget = setCenterOfAttentionToTurnTarget;
        }

        public override bool IsSame(Jobs.Job job)
        {
            return false;
        }
                    

        public GoalTurnToFace()
        {
        }


        protected override void Activate()
        {
            if (entityToFace.HasValue)
            {
                IKnownEntityData targetData;
                if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(entityToFace.Value, out targetData)))
                {
                    return;
                }

                pointToFace = targetData.PlaySiteLocation.ToVector2();
            }

            if (entity.EntityType.LocomotorType == null || Common.IsZero(entity.EntityType.LocomotorType.MaxAngularSpeed))
            {                
                Status = Goals.Status.Completed;

                // turning is disabled for this type (sentry)
                SetCenterOfAttention(); // do head turn

                return; 
            }

            Vector3 location = entity.PlaySiteLocation;
            float rotationOfClosestCorner;
            int cornerNo;

            if (turnCompletely)
            {
                allowedRotationMargin = 0.05f;
            }
            else
            {
                allowedRotationMargin = MathHelper.Lerp(0.2f, 0.75f, (float)The.Sim.GameplayRandomGenerator.NextDouble("GoalTurnToFace"));
            }

            if (IsFacing(entity, pointToFace, out rotationOfClosestCorner, out cornerNo, allowedRotationMargin)) //location, pointToFace, entity.Locomotor.Rotation))
            {
                Status = Status.Completed;
            }
            else
            {
                Status = Status.Active;
                                
                // look where we're going:
                SetCenterOfAttention();

                GoalTraverseEdgeBetweenWaypoints.SelectMoveSpeed(entity);
               
            }
        }

       


        private void SetCenterOfAttention()
        {
            if (setCenterOfAttentionToTurnTarget)
            {
                if (entityToFace.HasValue)
                {
                    entityIntelligence.SetNewCenterOfAttention(entityToFace.Value, null, interestLevelToSet);
                }
                else
                {
                    entityIntelligence.SetNewCenterOfAttention(null, pointToFace.ToVector3(), interestLevelToSet);
                }
            }
        }

        
     

        /// <summary>
        /// corner 0 = model rotation, 1 is closest clockwise etc.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="position"></param>
        /// <param name="faceThis"></param>
        /// <param name="currentRotation"></param>
        /// <param name="cornerNo"></param>
        /// <returns></returns>
        public static float FindClosestCorner(Entity entity, Vector3 position, Vector2 faceThis, float currentRotation, out int cornerNo) 
        {
            cornerNo = 0;
            float bestRotation = 0;
            if (entity.EntityType.LocomotorType.FourSidedSymmetry)
            {
                float minDistance = 10000f;
                
                float currentDistance;
                float cornerToTest = currentRotation;

                float desiredAngle = ComputeDesiredAngle(position, faceThis);

                for (int i = 0; i < 4; i++) // let's test four corners
                {
                    currentDistance = GetSmallestAngleDistance(desiredAngle, cornerToTest); //???!! // GetAngleDistance(desiredAngle, cornerToTest); 

                    if (currentDistance < minDistance - 0.01f)//0.006f) // prevents jitter?
                    {
                        minDistance = currentDistance;
                        bestRotation = cornerToTest;

                        cornerNo = i;
                    }

                    cornerToTest += MathHelper.PiOver2;
                    cornerToTest = Common.WrapAngleBetweenZeroAndTwoPi(cornerToTest);
                }

            }
            else
            {
                bestRotation = currentRotation;
            }

            return bestRotation;
        }

        public static bool IsOneCornerFacing(Entity entity, Vector3 location, Vector2 pointToFace, float currentRotation, out float rotationOfClosestCorner, out int cornerNo, float allowedRotationMargin = 0.05f)
        {

            rotationOfClosestCorner = FindClosestCorner(entity, location, pointToFace, currentRotation, out cornerNo);

            return IsFacing(location, pointToFace, rotationOfClosestCorner, allowedRotationMargin);
          
        }

        /// <summary>
        /// this method handles symmetric beings correctly
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="pointToFace"></param>
        /// <param name="rotationOfClosestCorner"></param>
        /// <param name="cornerNo"></param>
        /// <returns></returns>
        public static bool IsFacing(Entity entity, Vector2 pointToFace, out float rotationOfClosestCorner, out int cornerNo, float allowedRotationMargin = 0.05f, bool performSymmetricFlip = true)
        {
            Vector3 location = entity.PlaySiteLocation;

            if (entity.EntityType.LocomotorType.RotatorType != null)
            {
                rotationOfClosestCorner = entity.Locomotor.Rotator.AbsoluteRotation; // not the corner, the rotator!
                cornerNo = 0; // ?

                return IsFacing(location, pointToFace, entity.Locomotor.Rotator.AbsoluteRotation, allowedRotationMargin);
              
            }
            else if (entity.EntityType.LocomotorType.FourSidedSymmetry)
            {
                bool isOneCornerFacing = IsOneCornerFacing(entity, location, pointToFace, entity.Rotation, out rotationOfClosestCorner, out cornerNo);

                if (performSymmetricFlip)
                {
                    if (cornerNo != 0)
                    {
                        // do the flip!!! hope no-one notices...
                        entity.Locomotor.FlipFourSidedSymmetryCreature(rotationOfClosestCorner);

                        rotationOfClosestCorner = entity.Rotation;
                        cornerNo = 0;
                    }
                }

                return isOneCornerFacing;
            }
            else
            {
                rotationOfClosestCorner = entity.Rotation;
                cornerNo = 0;

               /* float desiredAngle = ComputeDesiredAngle(location, pointToFace);

                return GetSmallestAngleDistance(desiredAngle, entity.Rotation) < maxAngleDistance;
                */

                return IsFacing(location, pointToFace, entity.Rotation, allowedRotationMargin);
              
            }
        }

        private static bool IsFacing(Vector3 location, Vector2 faceThis, float currentRotation, float allowedRotationMargin)
        {
            float desiredAngle = ComputeDesiredAngle(location, faceThis);


            return GetSmallestAngleDistance(desiredAngle, currentRotation) < allowedRotationMargin; // 0.05f;
            //return GetAngleDistance(desiredAngle, currentRotation) < 0.05f;
           
        }

        public static float ComputeModelRotationFromCornerRotation(int cornerNo, float cornerRotation)
        {
            if (cornerNo == 0)
            {
                return cornerRotation;
            }
            else
            {
                float modelRotation = cornerRotation - cornerNo * MathHelper.PiOver2;
                return Common.WrapAngleBetweenZeroAndTwoPi(modelRotation);
            }
        }

        /// <summary>
        /// what is correct??!?!
        /// </summary>
        /// <param name="rotationToFace"></param>
        /// <param name="currentRotation"></param>
        /// <returns></returns>
        public static float GetSmallestAngleDistance(float rotationToFace, float currentRotation)
        {
            float difference = Math.Abs(rotationToFace - currentRotation);

          //  return difference;

            return Math.Min(difference, MathHelper.TwoPi - difference);
        }

        public static float GetAngleDistance(float rotationToFace, float currentRotation)
        {
            float difference = Math.Abs(rotationToFace - currentRotation);

            return difference;

          //  return Math.Min(difference, MathHelper.TwoPi - difference);
        }


      /*  public static float GetAngleDistance(/*Vector3 position, Vector2 faceThis,*/ 
        //float desiredAngle, float currentRotation)
        /*{
           
            return GetAngleDistance(desiredAngle, currentRotation);

           // return Math.Abs(currentRotation - desiredAngle);
        }*/

        private static float ComputeDesiredAngle(Vector3 position, Vector2 faceThis)
        {
            float x = faceThis.X - position.X;
            float y = faceThis.Y - position.Y;
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

            // we'll use the Atan2 function. Atan will calculates the arc tangent of 
            // y / x for us, and has the added benefit that it will use the signs of x
            // and y to determine what cartesian quadrant to put the result in.
            // http://msdn2.microsoft.com/en-us/library/system.math.atan2.aspx
            float desiredAngle = (float)Math.Atan2(y, x);

            // wrap the angle to stay consistent with our angle convention:
            desiredAngle = Common.WrapAngleBetweenZeroAndTwoPi(desiredAngle);
            return desiredAngle;
        }

        public static float TurnSpeedWhenTurningInPlace = 2.8f;  //8th jan 2013: 1.8f........ 0.8f   Turn Speed When Turning In Place  ..speed of rotation when turning..while standing still  (..rotation speed)

        protected override void ProcessWhileActive(GameTime elapsed)
        {
            //if status is inactive, call Activate()           
            /*  ActivateIfInactive();

              if (Status == Status.Active)
              {*/
            Vector3 location = entity.PlaySiteLocation;
            float rotationOfClosestCorner;
            int cornerNo;
            if (IsFacing(entity, pointToFace, out rotationOfClosestCorner, out cornerNo, allowedRotationMargin)) //location, pointToFace, entity.Locomotor.Rotation))
            {
                Status = Status.Completed;
            }
            else
            {
                // if (cornerNo != 0)
                //{
                //    // do the flip!!! hope no-one notices...
                //    entity.Renderable./*TODO DECOUPLE*/RenderAsModel.FlipFourSidedSymmetryCreature(rotationOfClosestCorner);

                //    rotationOfClosestCorner = entity.Locomotor.Rotation;
                //    cornerNo = 0;
                //} 

                // use a more relaxed speed when turning in place:
                float angularSpeed = Math.Min(TurnSpeedWhenTurningInPlace, entity.EntityType.LocomotorType.MaxAngularSpeed);

                float facingDirection;
                float headingDifference;
                facingDirection = GoalFlyToPosition.TurnToFace(location, pointToFace,
                        rotationOfClosestCorner, (float)(angularSpeed * elapsed.ElapsedGameTime.TotalSeconds),
                        out headingDifference);

                // facingDirection = ComputeModelRotationFromCornerRotation(cornerNo, facingDirection);


                // update rotation:               
                /*  entity.Locomotor.Rotation = facingDirection;

                  entity.Locomotor.NormalizedMoveDir = new Vector3(
                      (float)Math.Cos(facingDirection),
                      (float)Math.Sin(facingDirection), 0f);*/

                if (entity.EntityType.LocomotorType.RotatorType == null)
                {
                    entity.SetRotationAndDir(facingDirection);
                }
                else
                {
                    entity.Locomotor.Rotator.AbsoluteRotation = facingDirection;
                }

            }

        }


        public override void OnExit()
        {
            base.OnExit();

            if (entity.EntityType.LocomotorType.RotatorType != null)
            {
                entity.Locomotor.Rotator.DoneRotating();
            }
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

            this.pointToFace = sn.DoVector2(pointToFace);
            this.entityToFace = sn.DoEntityIDNullable(entityToFace);
            this.setCenterOfAttentionToTurnTarget = sn.DoBool(setCenterOfAttentionToTurnTarget);
            this.interestLevelToSet = sn.DoFloat(interestLevelToSet);
            this.turnCompletely = sn.DoBool(turnCompletely);
            this.allowedRotationMargin = sn.DoFloat(allowedRotationMargin);

            return this;
        }

        #endregion
       
    }
}
