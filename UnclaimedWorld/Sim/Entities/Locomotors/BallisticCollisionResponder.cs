using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Collisions;
using UWGame.SimSide.AI.Goals;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities.Body;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Combat;

namespace UWGame.SimSide.Entities.Locomotors
{
    public class BallisticCollisionResponder : ICollisionResponder, ISnapshot
    {
        CollisionResponder parent;
        Entity parentEntity;

        public BallisticCollisionResponder(CollisionResponder parent)
        {
            this.parent = parent;

            parentEntity = this.parent.Parent.Parent;

        }

        public BallisticCollisionResponder()
        { }

        public void HandleSingleCollision(Collidable<Entity> collidee)
        {
            Entity otherEntity = collidee.Parent;

            BallisticLocomotor ballisticLocomotor = parent.Parent.BallisticLocomotor;

            // no friendly fire...
            if (otherEntity.EntityType.IntelligenceType != null
                                && otherEntity.Intelligence.Allegiance != ballisticLocomotor.LaunchedByAllegiance) // parentEntity.Intelligence.Allegiance)
            {
                // do damage here...   
                // find the body part we hit..
                BodyPart.AttackDirection attackDirection = GoalDoAttack.GetAttackDirection(parentEntity, otherEntity.Location.Value, otherEntity.Rotation);

                BodyPart bodypart = otherEntity.Body.GetRandomBodyPartToHit(attackDirection);

                //This function used to destroy the job when the target died. It no longer does this
                //TODO: Needs to be tested again before this function is safe to use here.
                ballisticLocomotor.AttackType.HitTargets(Entity.FindByID(ballisticLocomotor.LaunchedByEntity), ballisticLocomotor.AttackJob, ballisticLocomotor.OwnerOfCarcass, bodypart.BodyPartID, otherEntity);

                Vector3 location = parentEntity.Location.Value;
                location.Z = 0f; // lay on ground
                parentEntity.Location = location;
                parent.Parent.BallisticLocomotor.StopMoving();

                //parentEntity.Destroy();

            }


        }

        public void BeginCollisionHandling()
        {
            
        }

        public void EndCollisionHandling(List<Collidable<Entity>> collidees)
        {
            
        }


        #region ISnapshot

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
            // TODO

            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

        }

        #endregion
    }
}
