using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using UWGame.SimSide.Maps;

namespace UWGame.SimSide.Collisions
{

    /// <summary>
    /// Assign Center, Size etc. when properties change. This will invoke the manager to move the collidable
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DebuggerDisplay("Parent={parent}")]
    public class Collidable<T>
  //  public class Collidable
    {
        #region Properties

        private Vector2 center;
        public Vector2 Center
        {
            get
            {
                return center;
            }
            set
            {
                if (center != value)
                {
                    center = value;
                    OnChange();
                }
            }
        }

        private Vector2 size;

        /// <summary>
        ///  the offset from center to corners
        /// </summary>
        public Vector2 Size
        {
            get
            {
                return size;
            }
            set
            {
                if (size != value)
                {
                    size = value;
                    OnChange();
                }
            }
        }

        private bool enabled = false;

        /// <summary>
        /// returns true if the collidable is currently registered with Collision Manager and generating collisions
        /// </summary>
        public bool Enabled
        {
            get
            {
                return enabled;
            }
        }

        private CollideShape2D bounds;

        /// <summary>
        /// The shape associated with this collidable Entity, Renderable, etc.
        /// </summary>
        public CollideShape2D Bounds
        {
            get
            {
                return bounds;
            }
            set
            {
                if (bounds != value)
                {
                    bounds = value;
                    OnChange();
                }
            }
        }

        private List<CollideShape2D> childShapes;
        public void AddChildShape(CollideShape2D child)
        {
            CollideShape2D toAdd = new CollideShape2D(child);

            if (childShapes == null)
                childShapes = new List<CollideShape2D>();

            childShapes.Add(toAdd);
            toAdd.Parent = this as Collidable<Entity>;

            OnChange();
        }


        public bool IsComposite
        {
            get
            {
                return childShapes != null && childShapes.Count > 0;
            }
        }


        private bool flipHorizontally = false;
        public bool FlipHorizontally
        {
            get
            {
                return flipHorizontally;
            }
            set
            {
                if (flipHorizontally != value)
                {
                    flipHorizontally = value;

                    //iterate child shapes and flip
                    if (childShapes != null)
                    {
                        foreach (CollideShape2D child in childShapes)
                            child.FlipHorizontally();

                        OnChange();
                    }

                }
            }
        }


        private T parent;

        /// <summary>
        /// The parent of this object (entity, renderable, etc.)
        /// </summary>
        /// <remarks>used to access the Entity, Renderable, etc. controlling this proxy</remarks>
        public T Parent
        {
            get
            {
                return parent;
            }
        }

        #endregion

        #region Events and Event Handlers

        public delegate void MoveHandlerDelegate(Collidable<T> collidableMoving);
        public event MoveHandlerDelegate Move;

        public delegate void DestroyHandlerDelegate(Collidable<T> collidableToDestroy);
        public event DestroyHandlerDelegate Destroy;

        protected void OnChange()
        {

            if (childShapes != null)
            {
                Vector2 ul = Vector2.Zero;
                Vector2 lr = Vector2.Zero;

                // Update boundary geometries
                foreach (CollideShape2D child in childShapes)
                {
                    if (ul.X > child.BoundsUpperLeft.X + child.Offset.X)
                        ul.X = child.BoundsUpperLeft.X + child.Offset.X;

                    if (ul.Y > child.BoundsUpperLeft.Y + child.Offset.Y)
                        ul.Y = child.BoundsUpperLeft.Y + child.Offset.Y;

                    if (lr.X < child.BoundsLowerRight.X + child.Offset.X)
                        lr.X = child.BoundsLowerRight.X + child.Offset.X;

                    if (lr.Y < child.BoundsLowerRight.Y + child.Offset.Y)
                        lr.Y = child.BoundsLowerRight.Y + child.Offset.Y;
                }

                size.X = Math.Max(Math.Abs(ul.X), Math.Abs(lr.X));
                size.Y = Math.Max(Math.Abs(ul.Y), Math.Abs(lr.Y));

            }


            // Refresh bounds geometry
            bounds.BoundsUpperLeft = center - size;
            bounds.BoundsLowerRight = center + size;

            // Call event handler
            if (Move != null)
                Move(this);

            
            Entity parentEntity = parent as Entity;
            if (parentEntity != null && parentEntity.GeometryLayout != null)
            {
                parentEntity.FootprintIsDirty = true;
            }
        }

        public void IterateChildShapes(Action<CollideShape2D, int> iterateMethod)
        {
            if (childShapes == null || childShapes.Count == 0)
                return;

            for (int i = 0; i < childShapes.Count; i++)
            {
                iterateMethod(childShapes[i], i);
            }

        }

        public bool IterateChildShapes(IterateCollidableMethod iterateMethod, Color color, bool isSelected = false)
        {
            if (childShapes == null || childShapes.Count == 0)
                return false;

            bool retval = false;

            foreach (CollideShape2D child in childShapes)
                retval |= iterateMethod(child, color, isSelected);

            return retval;

            // OnChange();//this assumes that the iterate method did something
        }

        public bool IterateChildShapes(IterateCollidableMethod iterateMethod, ref CollideShape2D other)
        {
            if (childShapes == null || childShapes.Count == 0)
                return false;

            bool retval = false;

            foreach (CollideShape2D child in childShapes)
                retval |= iterateMethod(child, Color.Black, false);

            return retval;

            // OnChange();//this assumes that the iterate method did something
        }




        public void NudgeChildShape(int idx, int x, int y, int? r = null, int? w = null)
        {
            if (childShapes == null || idx >= childShapes.Count)
                return;

            CollideShape2D child = childShapes[idx];

            Vector2 newOffset = child.Offset + new Vector2(x, y);
            float? newRadius = null;
            float? newHeight = null;
            float? newWidth = null;

            if (child.PrimitiveType == CollidePrim.Circle)
            {
                newRadius = child.Radius + r;

                if (newRadius <= 4)
                    newRadius = 4;//clamp to avoid many math problems later on

            }

            if (child.PrimitiveType == CollidePrim.Rectangle)
            {
                if (r.HasValue)
                {
                    int h = 2 * r.Value;

                    newHeight = child.GetHeight() + h;

                    if (newHeight <= 4)
                        newHeight = 4;//clamp to avoid many math problems later on
                }

                if (w.HasValue)
                {
                    newWidth = child.GetWidth() + w.Value;

                    if (newWidth <= 8)
                        newWidth = 8;//clamp to avoid many math problems later on

                }
            }


            child.AdjustShape(newOffset, newRadius, newWidth, newHeight);

            OnChange();

            return; // to confirm valid idx
        }



        public void BeCircle()
        {
            bounds.BeCircle();
        }

        protected void OnDestroy()
        {
            if (Destroy != null)
                Destroy(this);
        }

        #endregion

     

        #region Initialization

        /// <summary>
        /// bast ctor, handles parent, center size and bounds
        /// 
        /// NEW: allow null for position, will be set later...
        /// </summary>
        public Collidable(T parent, Vector2? position, Vector2 size)
        {
            this.bounds = new CollideShape2D(0f, 0f, 1f, 1f);
            this.parent = parent;           
            this.size = size;

            if (position.HasValue)
            {
                this.center = position.Value;
                OnChange();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Destroys this collidable and removes it from the collisionmanager
        /// </summary>
        public void Delete()
        {
            OnDestroy();
        }

        public void SetEnabled()
        {
            enabled = true;

            // cannot call AddCollidable from this generic T class...

        }

        public void SetDisabled()
        {
            enabled = false;
        }

        static Vector2 spread1 = new Vector2(-MapManager.subTileSizeOver2, -MapManager.subTileSizeOver2);
        static Vector2 spread2 = new Vector2(0, -MapManager.subTileSizeOver2);
        static Vector2 spread3 = new Vector2(MapManager.subTileSizeOver2, -MapManager.subTileSizeOver2);
        static Vector2 spread4 = new Vector2(-MapManager.subTileSizeOver2, 0);
        static Vector2 spread5 = new Vector2(MapManager.subTileSizeOver2, 0);
        static Vector2 spread6 = new Vector2(-MapManager.subTileSizeOver2, MapManager.subTileSizeOver2);
        static Vector2 spread7 = new Vector2(0, MapManager.subTileSizeOver2);
        static Vector2 spread8 = new Vector2(MapManager.subTileSizeOver2, MapManager.subTileSizeOver2);


        public bool IsWithinShapes(Vector2 worldPos, bool testIfAnyPartOfSubtileIsInBounds)
        {
            bool isWithinBounds;
            if (testIfAnyPartOfSubtileIsInBounds)
            {
                // see if any part of the surrounding subtile is within shape bounds:
                isWithinBounds =
                                ContainsPoint(worldPos + spread1) ||
                                ContainsPoint(worldPos + spread2) ||
                                ContainsPoint(worldPos + spread3) ||
                                ContainsPoint(worldPos + spread4) ||
                                ContainsPoint(worldPos + spread5) ||
                                ContainsPoint(worldPos + spread6) ||
                                ContainsPoint(worldPos + spread7) ||
                                ContainsPoint(worldPos + spread8) ||
                                ContainsPoint(worldPos);
            }
            else
            {
                // only test the single point (center of a subtile, mostly)
                isWithinBounds = ContainsPoint(worldPos);
            }

            return isWithinBounds;
        }


        public void GetBoundsWithPadding(float pad, out Vector2 from, out Vector2 to)
        {
           // GeometryLayoutType geoType = parent.CurrentSimState.GeometryLayoutType; // parent.EntityType.GeometryLayoutType;

            Vector2 padVec = new Vector2(pad); // geoType.Pad);

            //iterate all the tiles inside the shape bounds rectangle 
            from = Bounds.BoundsUpperLeft - padVec;
            to = Bounds.BoundsLowerRight + padVec;

            // clamp first:
            from = The.Map.ClampWorldPosition(from);
            to = The.Map.ClampWorldPosition(to);

            // convert to center of subtiles instead of corners:
            from = MapManager.SubTileToWorldPos(MapManager.WorldPosToSubtile(from));
            to = MapManager.SubTileToWorldPos(MapManager.WorldPosToSubtile(to));
        }

        public bool ContainsPoint(Vector2 point)
        {
            if (!Bounds.ContainsPoint(point))
                return false;

            if (childShapes != null)
            {
                //iterate childshapes after bounds check
                //return true on first success
               
                foreach (CollideShape2D child in childShapes)
                {
                    if (child.ContainsPoint(point))
                        return true;
                }
            }

            return false;

        }

        public bool ShapesContainEntities(float pad, Predicate<Entity> countEntity)
        {
            return IterateSubtilesBreakOnTrue(pad, s => GeoLayoutSubtileContainsEntities(s, countEntity));

        }

        public bool IterateSubtilesBreakOnTrue(float pad, Predicate<Vector2> iterateMethod) //, Predicate<Entity> countEntity)
        {
            Vector2 from;
            Vector2 to;

            GetBoundsWithPadding(pad, out from, out to);

            return MapManager.IterateSubtilesBreakOnTrue(from, to, iterateMethod);
        }

        private bool GeoLayoutSubtileContainsEntities(Vector2 worldPos, Predicate<Entity> countEntity)
        {
            bool isWithinShapes = IsWithinShapes(worldPos, false);

            if (isWithinShapes)
            {
                Point subtilePos = MapManager.WorldPosToSubtile(worldPos);
                if (The.Map.SubtileContainsEntities(subtilePos, countEntity)) // parent.ID))
                {
                    return true;
                }
            }

            return false;
        }


        public bool TestCollision(Collidable<T> other, out float overlap, out Vector2 ctr)
        {
            //to begin with
            overlap = 0;
            ctr = other.Center;

            if (IsComposite) //I have multiple shapes
            {
                if (other.IsComposite) //we both have multiple shapes
                {
                    //this should be very unusual, since composite shapes tend to be stationary
                    //I iterate my child shapes, and report the deepest overlap
                    //TODO this has redundancy, since every one of my children tests every one of his and vice versa
                    foreach (CollideShape2D child in childShapes)
                    {
                        other.TestCollision(child, out overlap, out ctr);
                    }
                }
                else //but he has only his bounding shape
                {
                    //I iterate my child shapes, and report the deepest overlap
                    float ol = 0;
                    foreach (CollideShape2D child in childShapes)
                    {
                        ol = child.GetShapeOverlapAmount(other.Bounds);
                        if (ol > overlap)
                        {
                            overlap = 0l;
                            ctr = other.Center;
                        }
                    }
                }

            }
            else // I have only my bounding shape
            {
                if (other.IsComposite) //he has multiple shapes
                {
                    return other.TestCollision(this.Bounds, out overlap, out ctr);
                }
                else //we both have only our bounding shapes
                {
                    overlap = Bounds.GetShapeOverlapAmount(other.Bounds);
                    ctr = other.Center;
                }
            }

            return overlap > 0;
        }


        public bool TestCollision(CollideShape2D other, out float overlap, out Vector2 ctr)
        {
            overlap = 0;
            ctr = Center;

            //I iterate my child shapes, and report the deepest overlap
            foreach (CollideShape2D child in childShapes)
            {

                Vector2 ul = child.BoundsUpperLeft + child.Offset + Center;
                Vector2 lr = child.BoundsLowerRight + child.Offset + Center;

                CollideShape2D worldChild = new CollideShape2D(ul, lr);
                if (child.PrimitiveType == CollidePrim.Circle)
                    worldChild.BeCircle();

                float ol = worldChild.GetShapeOverlapAmount(other);
                if (ol > overlap)
                {
                    overlap = ol;
                    ctr = worldChild.Center;
                }
            }

            return overlap > 0;
        }


        #endregion
    }

}
