using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Collisions;
using Microsoft.Xna.Framework;
using GameStateManagement;
using UWGame.Control;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities.Locomotors
{
    /// <summary>
    /// performs an entity's reactions to colliding with other entities - I wanted to separate this from movement. 
    /// The response behavour could then perhaps be modified by the locomotion mode (wheeled/legged?)
    /// </summary>
    public class CollisionResponder : ISnapshot
    {
        public Locomotor Parent;

        /// <summary>
        /// The idea is that only one responder will be active at a time.
        /// </summary>
        private ICollisionResponder activeResponder;

        /// <summary>
        /// simulates a walking agent
        /// </summary>
        public AgentCollisionResponder AgentCollisionResponder;

        /// <summary>
        /// simulates an object in flight
        /// </summary>
        BallisticCollisionResponder BallisticCollisionResponder;


        private List<Collidable<Entity>> collidees = new List<Collidable<Entity>>();
      
        /// <summary>
        /// TODO: move to AgentColResponder
        /// 
        /// Added this variable as the IsMoving variable does not do what was needed
        /// as when the object gets pushed etc their IsMoving flag is changed, this one is currently set to false after waitgoal, need to sort that better.
        /// </summary>
        public bool WaitsToGiveRoomToOtherAgent = false;
        
        private enum ActiveResponder { Agent, Ballistic }
        ActiveResponder snapshotActiveResponder;
      

        public CollisionResponder()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }

        public CollisionResponder(Locomotor parent)
        {
            this.Parent = parent;

            CollisionResponderType responderType = parent.Parent.EntityType.LocomotorType.CollisionResponderType;
            if (responderType.AgentCollisionResponderType != null)
            {
                AgentCollisionResponder = new AgentCollisionResponder(this);

            }

            if (responderType.BallisticResponderType != null)
            {
                BallisticCollisionResponder = new Locomotors.BallisticCollisionResponder(this);

            }
        }

        public void ReactToCollisions()
        {
            if (activeResponder == null)
                return; // temporarily disabled..

            collidees.Clear(); //clean slate

           
            // i think agent responder shares info with other agents... watch out.
            activeResponder.BeginCollisionHandling();
               

            The.CollisionManager.GetCollidablesIntersectingBounds(Parent.Parent.Collidable.Bounds, ref collidees);// not actual collision test, only bounds check

            Parent.SetIsColliding(false);//until possibly set true below...

            if (collidees.Count > 1)//more than just self
            {
                Entity otherEntity = null;
                
                float overlap = 0;

                //bool collidingWithOtherAllegiance = false;

                

                foreach (Collidable<Entity> collidee in collidees)
                {
                    if (collidee.Parent == Parent.Parent || collidee.Parent == null)
                        continue;

                    otherEntity = collidee.Parent;

                    if (otherEntity.Collidable == null || otherEntity.Collidable.Enabled == false)
                    {   // SLEEP HACK - remove this - for some reason no collision proxy is set, but the entity still causes collisions
                        continue;
                    }

                    Vector2 otherCenter = otherEntity.Collidable.Center;
                    if (Parent.Parent.Collidable.TestCollision(otherEntity.Collidable, out overlap, out otherCenter) == false) //the actual collision test
                    {
                        continue;
                    }

                    Parent.SetIsColliding(true);//in case anyone asks

                    activeResponder.HandleSingleCollision(collidee);                        
                    
                }


                if (Parent.IsColliding == true && activeResponder != null) // are we still 
                {
                    activeResponder.EndCollisionHandling(collidees);                   
                    
                }

            }

        }



        public void SetCollisionResponse(Locomotor.Mode CurrentMoveMode)
        {
            switch (CurrentMoveMode)
            {
                case Locomotor.Mode.Legged:
                    activeResponder = AgentCollisionResponder;
                    break;
                case Locomotor.Mode.Ballistic:
                    activeResponder = BallisticCollisionResponder;
                    break;
                case Locomotor.Mode.None:
                    activeResponder = null; //?? 
                    break;
            }
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
            this.WaitsToGiveRoomToOtherAgent = sn.DoBool(WaitsToGiveRoomToOtherAgent);

            this.AgentCollisionResponder = (AgentCollisionResponder)sn.DoISnapshot(AgentCollisionResponder);
            this.BallisticCollisionResponder = (BallisticCollisionResponder)sn.DoISnapshot(BallisticCollisionResponder);

            if (sn.mode != Snapshotter.Mode.Load)
            {
                if (activeResponder is AgentCollisionResponder)
                {
                    snapshotActiveResponder = ActiveResponder.Agent;
                }
                else
                {
                    snapshotActiveResponder = ActiveResponder.Ballistic;
                }
                // add more here...
            }

            snapshotActiveResponder = sn.DoEnum(snapshotActiveResponder);


            sn.Ignore(Parent);
            sn.Ignore(collidees);
            sn.Ignore(activeResponder);

            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            if (AgentCollisionResponder != null)
            {
                AgentCollisionResponder.Parent = this;
                AgentCollisionResponder.LoadPostProcess(sn);
            }

            if (BallisticCollisionResponder != null)
            {
                BallisticCollisionResponder.LoadPostProcess(sn);
            }

            switch (snapshotActiveResponder)
            {
                case ActiveResponder.Agent:
                    activeResponder = AgentCollisionResponder;
                    break;
                case ActiveResponder.Ballistic:
                    activeResponder = BallisticCollisionResponder;
                    break;
                default:
                    activeResponder = AgentCollisionResponder;
                    break;
            }
        }

        #endregion
    }
}
