using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Collisions;
using Microsoft.Xna.Framework;
using UWGame.Control;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities.Locomotors
{
    public class AgentCollisionResponder : ICollisionResponder, ISnapshot
    {
        public CollisionResponder Parent;
        Entity parentEntity;

        Vector2 pushDirection = Vector2.Zero;

        /// <summary>
        /// this info is shared with other agents...
        /// </summary>
        public List<EntityID> LatestResolvedMovingCollisions = new List<EntityID>();

      //  private Dictionary<Collidable<Entity>, int> dislodgeCounters = new Dictionary<Collidable<Entity>, int>();
        private Dictionary<EntityID, int> dislodgeCounters = new Dictionary<EntityID, int>();

        Entity otherEntity = null;
        EntityID? snapshotOtherEntity;                

        public AgentCollisionResponder(CollisionResponder parent)
        {
            this.Parent = parent;

            parentEntity = this.Parent.Parent.Parent;
        }

        public AgentCollisionResponder() 
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");     
        }

        public void BeginCollisionHandling()
        {
            pushDirection = Vector2.Zero;

            
            LatestResolvedMovingCollisions.Clear();

            otherEntity = null;

        }

        public void HandleSingleCollision(Collidable<Entity> collidee)
        {
            otherEntity = collidee.Parent;

            if (otherEntity.EntityType.IntelligenceType == null
                 || (otherEntity.EntityType.IntelligenceType != null
                                && otherEntity.Intelligence.Allegiance != parentEntity.Intelligence.Allegiance))
            {
                // don't make room for critters/ other allegiances
                return; // skips all reaction behaviours below...        
            }

            // RESOLVE LOCKS/TRAFFIC JAMS:
            /*
             CASE 1:
             * I am sitting/standing, and another entity wants to walk past me
             */
            if (WeAreStaticAndNotReadyToDislodge(otherEntity)) // collidee))//See if someone is colliding with us and we are standing still, 
            {                                    // returns true if that is happening and if he has only done that for a short ammount of time or if we are waiting for something
                
                return; // prevents the other cases from being considered, and prevents a push vector from being applied.                
            }

            /*
             CASE 2:
             * If something has collided with us we should get pushed out from the center of that object.
             * if we are standing still, this means we will get out of the way of the object
             * if we are moving, it means we won't move right through the object
             */
            
            pushDirection += GetCollisionOffset(Parent.Parent.Parent.Collidable.Center - otherEntity.Collidable.Center);//Add a direction to the pushvector
           


            /*
             CASE 3:
             * I am either walking walking to or standing on a spot someone else wants aswell
             */
            if (otherEntity.Locomotor != null && otherEntity.Intelligence != null)//Is the collision with a movable objcet
            {
                ShouldSomeoneWait(otherEntity);//Checks if we should start a queue
            }

            /*
             CASE 4:
             * I am moving into or getting moved into by movable objects that wants to get past.
             */
            if (otherEntity.Locomotor != null && otherEntity.Locomotor.CollisionResponder != null)//Is the collision with a movable object
            {
                if (CheckIfCollisionAlreadySolved(otherEntity) == false)//Did the other guy already resolve the avoidance
                {
                    if (Parent.Parent.IsMoving() == true)//I am moving right now, else the moving object will handle this
                    {
                        pushDirection += SidestepOtherMovingEntity(otherEntity);//Check if we are colliding with a moving object and if we have to avoid it
                    }
                }
            }

            /*
            CASE 5:
            * <Add Next Case Here>
            */


            /**/


        }

        private bool CheckIfCollisionAlreadySolved(Entity other)
        {
            for (int i = 0; i < other.Locomotor.CollisionResponder.AgentCollisionResponder.LatestResolvedMovingCollisions.Count; i++)
            {
                if (other.Locomotor.CollisionResponder.AgentCollisionResponder.LatestResolvedMovingCollisions[i] == parentEntity.EntityID)
                {
                    return true;
                }
            }

            LatestResolvedMovingCollisions.Add(other.EntityID);

            return false;
        }

        private void ShouldSomeoneWait(Entity other)
        {
            bool isOtherIdle = other.Intelligence.IsIdle(); // IsEntityIdle(other);
            bool isEntityNearMyDestination = IsEntityNearMyDestination(other);
            bool isEntityHavingSameDestinationAsMe = IsEntityHavingSameDestionationAsMe(other);

            if (isOtherIdle == false &&
                isEntityHavingSameDestinationAsMe == true &&
                isEntityNearMyDestination == true)
            {
                Entity entityToStop = GetFurthestToDestination(other); //Find the entity furthest away

                if (entityToStop.Locomotor.CollisionResponder.WaitsToGiveRoomToOtherAgent == false)
                {
                    entityToStop.Intelligence.Brain.SendMessage(new Message(Message.MessageTypes.WaitAndMakeRoom));//Tell the other one to wait for you to be done
                }
            }
        }

        private Entity GetFurthestToDestination(Entity other)
        {
            float otherLengthToDestination = Common.DistanceOctile(other.PlaySiteLocation, Parent.Parent.CurrentMoveTarget);
            float parentLengthToDestination = Common.DistanceOctile(parentEntity.PlaySiteLocation, Parent.Parent.CurrentMoveTarget);

            if (parentLengthToDestination > otherLengthToDestination)
            {
                return parentEntity;
            }
            return other;
        }

        private bool WeAreStaticAndNotReadyToDislodge(/*Collidable<Entity>*/ Entity collidee)
        {
            
            int dislodgeResetvalue = 50;
            int dislodgeCounter;

            if (!dislodgeCounters.TryGetValue(collidee.ID, out dislodgeCounter))
            {
                dislodgeCounter = dislodgeResetvalue;
                dislodgeCounters.Add(collidee.ID, dislodgeCounter);
            }

            if ((!Parent.Parent.IsMoving() && collidee.Locomotor != null && collidee.Locomotor.IsMoving()))
            {
                --dislodgeCounter;

                dislodgeCounter = Common.ClampBottom(dislodgeCounter, 0);

                dislodgeCounters[collidee.ID] = dislodgeCounter;

                if (dislodgeCounter > 0)
                {
                    //we have not yet been pushed enough. if the pusher persists then I will move, but not yet.
                    return true;
                }
            }
            else if (dislodgeCounter < dislodgeResetvalue)
            {
                dislodgeCounter++; // recover after being pushed

                dislodgeCounters[collidee.ID] = dislodgeCounter;
            }
            else if (dislodgeCounter >= dislodgeResetvalue)
            {
                // remove the counter when it is fully increased
                dislodgeCounters.Remove(collidee.ID);
            }

            return false;
        }

        private bool IsEntityHavingSameDestionationAsMe(Entity other)
        {

            float collidingObjectPathX = other.Locomotor.CurrentMoveTarget.X;
            float collidingObjectPathY = other.Locomotor.CurrentMoveTarget.Y;

            float parentPathX = Parent.Parent.CurrentMoveTarget.X;
            float parentPathY = Parent.Parent.CurrentMoveTarget.Y;

            bool gotSamePathTarget = 
                Common.IsEqual(collidingObjectPathX, parentPathX) 
                && Common.IsEqual(collidingObjectPathY, parentPathY);

            return gotSamePathTarget;

        }

        private bool IsEntityNearMyDestination(Entity other)
        {



            bool entityCloseToMyTarget = (Common.IsEqual(other.PlaySiteLocation.X, Parent.Parent.CurrentMoveTarget.X, 10) && Common.IsEqual(other.PlaySiteLocation.Y, Parent.Parent.CurrentMoveTarget.Y, 10));

            return entityCloseToMyTarget;

        }

        private bool GetIsPositionLeftOfParent(Vector2 position)
        {
            Vector2 parentPosition = Parent.Parent.Parent.PlaySiteLocation.ToVector2();
            Vector2 parentDirection = Parent.Parent.direction.ToVector2();
            bool isObjectOnTheLeftSideOfParent = (((parentDirection.X - parentPosition.X) *
                                                   (position.Y - parentPosition.Y) -
                                                   (parentDirection.Y - position.Y) *
                                                   (position.X - parentPosition.X)) > 0);
            return isObjectOnTheLeftSideOfParent;
        }



        private Vector2 GetCollisionOffset(Vector2 offsetBetweenOwnerAndOther)
        {
            //TODO instead of offsetting from other.CollisionProxy.Center
            //which could be in the middle of a composite shape
            //lets use the middle of the child shape that we have actually collided with the deepest

            Vector2 offset = offsetBetweenOwnerAndOther;//to begin with...

            if (offset == Vector2.Zero) //oops
            {
                offset.X += (float)The.Sim.GameplayRandomGenerator.NextDouble("AgentCollisionResponder") - .5f;
                offset.Y += (float)The.Sim.GameplayRandomGenerator.NextDouble("AgentCollisionResponder") - .5f;
                if (offset == Vector2.Zero) //double oops
                    offset.Y = 1;
            }
            offset.Normalize();
            return offset;
        }

        private Vector2 SidestepOtherMovingEntity(Entity other)
        {
            Vector2 pushModifier = Vector2.Zero;
            Vector3 otherDirection = other.Locomotor.direction;
            float facingDotProduct = Vector3.Dot(Parent.Parent.direction, otherDirection);

            bool facingMovingAgent = (Common.IsEqual(facingDotProduct, -1, 0.1f) && other.IsMoving == true);//We will head on collisions with moving objects
            bool walkingIntoNonMovingAgent = (other.IsMoving == false);//We will avoid collisions with non moving objects
            if (facingMovingAgent == true || walkingIntoNonMovingAgent == true)//Are we facing eachother
            {
                if (GetIsPositionLeftOfParent(other.PlaySiteLocation.ToVector2()))
                {
                    pushModifier = new Vector2(Parent.Parent.direction.Y, -Parent.Parent.direction.X);//Make our agent sidestep right
                }
                else
                {
                    pushModifier = new Vector2(-Parent.Parent.direction.Y, Parent.Parent.direction.X);//Make our agent sidestep left
                }
            }
            return pushModifier;
        }

        public void SetInterestInCollidedEntity(List<Collidable<Entity>> collidees)
        {
            if (collidees.Count > 1)//more than just self
            {
                Entity otherEntity = null;
                float closestAgentDistance = -1;
                Entity closestEntity = null;

                foreach (Collidable<Entity> collidee in collidees)//Loop trough all collisions
                {
                    if (otherEntity == Parent.Parent.Parent || otherEntity == null)
                        continue;

                    if (CheckIfEntityIsClosestCollidingEntitySoFar(otherEntity, closestAgentDistance))//True if new object is closer than old
                    {
                        closestEntity = otherEntity;//Sets the new object
                    }

                }
                if (closestEntity != null)
                {
                    Parent.Parent.Parent.SetInterestInCollidedEntity(closestEntity);//look at the closest object 
                }
            }
        }

        private bool CheckIfEntityIsClosestCollidingEntitySoFar(Entity otherEntity, float closestAgentDistance)
        {

            float lengthBetweenParentAndOtherAgent = Common.DistanceOctile(Parent.Parent.Parent.Collidable.Center, otherEntity.Collidable.Center);

            if (lengthBetweenParentAndOtherAgent < closestAgentDistance || closestAgentDistance == -1)//We will be looking at the closest entity 
            {                                                                                         //so we check for every entity that we are colliding with what one is closest
                closestAgentDistance = lengthBetweenParentAndOtherAgent;
                return true;
            }
            return false;

        }

        public void EndCollisionHandling(List<Collidable<Entity>> collidees)
        {
            
            if (pushDirection != Vector2.Zero)
            {
                // hmm. here the last colllided entity is good enough:
                Parent.Parent.ApplyPushVector(otherEntity, pushDirection);//Apply the pushvector to move our Parent

                // make sure we are awake if we are getting pushed around:
                parentEntity.SendMessage(new Message(Message.MessageTypes.WakeUpCombatAlert));
            }

            // but here we want the closest...?
            SetInterestInCollidedEntity(collidees); //Lets only look at a object we actually are in contact with
            
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
            this.LatestResolvedMovingCollisions = sn.DoList(LatestResolvedMovingCollisions);
            this.pushDirection = sn.DoVector2(pushDirection);
            this.snapshotOtherEntity = sn.SnapshotID<Entity, EntityID>(otherEntity);
            this.dislodgeCounters = sn.DoDictionary(dislodgeCounters);

            sn.Ignore(Parent); // is pushed from Col Responder
            sn.Ignore(parentEntity);

            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            parentEntity = this.Parent.Parent.Parent;

            otherEntity = Entity.FindByID(snapshotOtherEntity);
        }

        #endregion
    }
}
