using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameStateManagement;
using UWGame.SimSide.AI.Goals;
using UWGame.ClientSide.Renderables;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Items;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Combat;

namespace UWGame.SimSide.Entities.Locomotors
{
    /// <summary>
    /// gives the entity the ability to move through the air in a ballistic trajectory
    /// </summary>
    public class BallisticLocomotor : ISnapshot
    {
        private Vector3 direction;

        public Locomotor Parent;

        #region Launched missiles attack data

        /// <summary>
        /// mostly for logging of hit result
        /// </summary>
        public EntityID? LaunchedByEntity;


        /// <summary>
        /// to prevent friendly fire...
        /// </summary>
        public Allegiances.Allegiance LaunchedByAllegiance;

        /// <summary>
        /// has damage info
        /// </summary>
        public AttackType AttackType;

        public OwnerID? OwnerOfCarcass;
        public AttackJob AttackJob;

        #endregion

        private Goal.MovementSpeeds currentMovementSpeedType = Goal.MovementSpeeds.Normal;
        public Goal.MovementSpeeds CurrentMovementSpeedType
        {
            get
            {
                return currentMovementSpeedType;
            }
            set 
            {
                if (currentMovementSpeedType != value)
                {
                    currentMovementSpeedType = value;
                    Parent.CurrentMaximumSpeedIsDirty = true;
                    Parent.CurrentMaximumSpeedNotAffectedByTerrainIsDirty = true;
                    Parent.CurrentMaximumSpeedForEvaluatorIsDirty = true;
                }
            }
        }


        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public Snapshotter.Version DoVersion(Snapshotter sn)
        {
            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }

        public bool IsSnapshotted { get; set; }


        public ISnapshot DoSnapshot(Snapshotter sn)
        {
//            this.AttackJob;            //TODO
//            this.AttackType;           //TODO
//            this.currentMovementSpeedType;//TODO
//            this.direction;            //TODO  
            this.gravity = sn.DoDouble(gravity);
//            this.LaunchedByAllegiance;//TODO
            this.LaunchedByEntity = sn.DoEnumNullable(LaunchedByEntity);
//            this.OwnerOfCarcass;      //TODO
//            this.Parent;              //TODO 
            this.velocity = sn.DoVector3(velocity);

            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            //lookups and other fix-ups
        }

        public BallisticLocomotor()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }


        public BallisticLocomotor(Locomotor parent)
        {
            this.Parent = parent;

            
        }

        public void Launch(Vector3 from, Vector3 velocity) 
        {
            this.velocity = velocity;

            float headingRotation = Common.VectorToAngle(velocity.ToVector2());

            Parent.Parent.SetRotationAndDir(headingRotation);

            Parent.Parent.Location = from;
        }


        public void StartMoving(Vector3 target, float speed, float upwardsSpeed, EntityID? launchedByEntity, Allegiances.Allegiance launchedByAllegiance, AttackType attackType, OwnerID? ownerOfCarcass, AttackJob job)
        {
            this.LaunchedByEntity = launchedByEntity;
            this.LaunchedByAllegiance = launchedByAllegiance;
            this.AttackType = attackType;
            this.OwnerOfCarcass = ownerOfCarcass;
            this.AttackJob = job;

            direction = target - Parent.Parent.Location.Value;
            direction.Normalize();

            Vector3 startLocation = Parent.Parent.Location.Value;
            startLocation.Z = 20f;  // start above ground
            Parent.Parent.Location = startLocation;

            velocity = speed * direction;

            velocity.Z = upwardsSpeed; // upwards speed

            float headingRotation = Common.VectorToAngle(velocity.ToVector2());

            Parent.Parent.SetRotationAndDir(headingRotation);
            Parent.Parent.Renderable.SetToParentLocation();
            // can't set pitch... is in Vehicle :(

            if (Parent.Parent.Collidable == null)
            {
                // let's create a collision proxy on the fly if the entity doesn't have one:
                Parent.Parent.Collidable = The.CollisionManager.AddCollidable(Parent.Parent,
                    Parent.Parent.PlaySiteLocation.ToVector2(), new Vector2(2f, 2f));
            }
        }

        double gravity = 10f;

        Vector3 velocity;

        public void MoveTowardsLocation(GameTime elapsed)
        {
            // no lift... arrows also?

          
            //          Rotation += 0.01f;
            //          SetRotationAndDir(Rotation); 
            /*
            Vector3 pos = Parent.Location;

            pos.Z = (float)(30.0 + (Math.Sin(aa += 0.01) * 30.0));

            Parent.SetPosition(pos);

            */


            double deltaT = elapsed.ElapsedGameTime.TotalSeconds;
            velocity.Z = (float)(velocity.Z - deltaT * gravity);

          //  double zSpeed = zSpeed elapsed
          //  double heightOverGround =

            Vector3 location = Parent.Parent.PlaySiteLocation;

            location += velocity * (float)deltaT;

            bool destinationReached = false;
            if (location.Z <= 0f)
            {
                location.Z = 0f;

                destinationReached = true;
            }

            Parent.Parent.Location = location;

            if (destinationReached)
            {
                StopMoving();
            }
        }

        public void StopMoving()
        {
            Parent.CurrentMoveMode = Locomotor.Mode.None;
            Parent.OnDestinationReached();
        }
    }
}
