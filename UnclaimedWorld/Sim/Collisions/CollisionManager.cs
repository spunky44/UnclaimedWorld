using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using UWGame.SimSide.Snapshots;
using System.Drawing;

namespace UWGame.SimSide.Collisions
{
    //give any caller the ability to run code on all my children
    public delegate bool IterateCollidableMethod(CollideShape2D child, Microsoft.Xna.Framework.Color color, bool sel);


 
    /// <summary>
    /// A collision manager for partitioning a space into rectangles
    /// 
    /// Objects inside are updted when they move through their Collidable.Move event
    /// </summary>
    /// <typeparam name="T">The type of the collisionmanager's collidables' parents, like Entity, Drawable, Trigger, etc.</typeparam>
    public class CollisionManager<T> : ISnapshot
    {
 
        /// <summary>
        /// The head node of the collisionmanager
        /// </summary>
        protected QTNode<T> headNode;

        /// <summary>
        /// Gets the map rectangle - never used it seems
        /// </summary>
        public CollideShape2D MapRect
        {
            get
            {
                return headNode.Bounds;
            }
        }

        private Vector2 snapshotBounds; // only the size is needed to recreate the head node - it has no parent.


        /// <summary>
        /// The maximum number of collidables in any node before partitioning
        /// </summary>
        protected int maxNodeCollidablesBeforePartition;

        
        // entity, position, size
        private List<Collidable<T>> listToRebuildFromPostLoad;
        //private List<Tuple<T, Vector2, Vector2>> listToRebuildFromPostLoad = new List<Tuple<T, Vector2, Vector2>>(); // this must be set prior to LoadPostProcess to rebuild the tree

        public CollisionManager()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor");
        }
 
        //ctor
        public CollisionManager(CollideShape2D worldRect, int maxCollidablesPerNode)
        {
            this.headNode = new QTNode<T>(worldRect, maxCollidablesPerNode, Resize);
            this.maxNodeCollidablesBeforePartition = maxCollidablesPerNode;
        }

        //ctor
        public CollisionManager(Vector2 size, int maxCollidablesPerNode)
            : this(new CollideShape2D(Vector2.Zero, size), maxCollidablesPerNode)
        {
            // Nothing extra to initialize
            
        }

 
 
       

         public void RemoveCollidable(Collidable<T> collidable)
         {
             headNode.RemoveCollidable(collidable);

             collidable.SetDisabled();
         }

        /// <summary>
        /// Inserts an existing collidable (which can have a complex shape) into the collisionmanager -- will resize if neede to encompas new position
        /// </summary>
         public void AddCollidable(Collidable<T> collidable)
        {             
            // check if the world needs resizing
            if (!headNode.ContainsRect(collidable.Bounds))
            {
                Resize(new CollideShape2D(
                    Vector2.Min(headNode.Bounds.BoundsUpperLeft, collidable.Bounds.BoundsUpperLeft) * 2,
                    Vector2.Max(headNode.Bounds.BoundsLowerRight, collidable.Bounds.BoundsLowerRight) * 2));
            }

            headNode.Insert(collidable);

            collidable.SetEnabled();
        }

        /// <summary>
        /// constructs, adds and returns a new circular collidable instance, also resizes as needed
        /// </summary>
         public Collidable<T> AddCollidable(T parent, Vector2 position, Vector2 size)
        {
            Collidable<T> newCol = new Collidable<T>(parent, position, size);

            // check if the world needs resizing
            if (!headNode.ContainsRect(newCol.Bounds))
            {
                Resize(new CollideShape2D(
                    Vector2.Min(headNode.Bounds.BoundsUpperLeft, newCol.Bounds.BoundsUpperLeft) * 2,
                    Vector2.Max(headNode.Bounds.BoundsLowerRight, newCol.Bounds.BoundsLowerRight) * 2));
            }

            headNode.Insert(newCol);

            newCol.SetEnabled();

            return newCol;
        }

        /// <summary>
        /// Resizes the collisionmanager map world size to the bounds of shape passed in
        /// </summary>
        public void Resize(CollideShape2D newMapSize)
        {
            // Get all of the collidables in the tree
            List<Collidable<T>> CollidableBucket = new List<Collidable<T>>();
            GetAllCollidables(ref CollidableBucket);

            // Destroy the head node
            headNode.Destroy();
            headNode = null;

            // Create a new head
            headNode = new QTNode<T>(newMapSize, maxNodeCollidablesBeforePartition, Resize);

            // Reinsert all the collidables
            foreach (Collidable<T> m in CollidableBucket)
            {
                headNode.Insert(m);
            }
        }

 
 
        /// <summary>
        /// adds collidables containing a specified point to the list referenced
        /// </summary>
        public void GetCollidablesContainingPoint(Vector2 location, ICollection<Collidable<T>> resultsList)
        {
            headNode.GetCollidablesContainingPoint(location, resultsList);
        }
      /*  public void GetCollidablesContainingPoint(Vector2 Point, ref List<Collidable<T>> CollidablesList)
        {
            headNode.GetCollidablesContainingPoint(Point, ref CollidablesList);
        }*/

        /// <summary>
        /// adds collidables intersecting a specified rectangle to the list referenced
        /// </summary>
        public void GetCollidablesIntersectingBounds(CollideShape2D bounds, ref List<Collidable<T>> collidablesList)
        {
            headNode.GetCollidablesIntersectingBounds(bounds, ref collidablesList);
        }

        /// <summary>
         /// adds all collidables in the collisionmanager to a list 
        /// </summary>
        public void GetAllCollidables(ref List<Collidable<T>> collidablesList)
        {           
            headNode.GetAllICollidablesInNode(ref collidablesList);            
        }

        
        // public void GetEntitiesInRange(Vector2 center, float radius, Predicate<Entity> filter, ref List<Collidable<Entity>> resultsList)
        public void GetEntitiesInRange(Vector2 center, float radius, Predicate<T> filter, ref List<Collidable<T>> resultsList)
         {
             //List<Collidable<Entity>> collidables = new List<Collidable<Entity>>();
             CollideShape2D bounds = new CollideShape2D(center, radius);

             GetCollidablesIntersectingBounds(bounds, ref resultsList);

             //The.CollisionManager.GetCollidablesIntersectingBounds(bounds, ref resultsList);

             if (resultsList != null)
             {
                // Collidable<Entity> collidable;
                 Collidable<T> collidable;
                 for (int i = resultsList.Count - 1; i >= 0; i--)
                 {
                     collidable = resultsList[i];
                     if (collidable.Parent == null || !filter(collidable.Parent))
                     {
                         resultsList.RemoveAt(i);
                     }
                 }
             }

         }

         #region ISnapshot


         public ISnapshot DoSnapshot(Snapshotter sn)
         {
             // snapshot strategy - (same as for PointQuadTree!):
             // instead of dealing with the graph of nodes, let's snapshot the list of members, then rebuild the tree after load.
             // but since the members can be a reference type like Entity, let the client handle them...

         //    this.maxDepth = sn.DoInt32(maxDepth);
             this.maxNodeCollidablesBeforePartition = sn.DoInt32(maxNodeCollidablesBeforePartition);

             if (sn.mode == Snapshotter.Mode.Save)
             {
                 this.snapshotBounds = MapRect.BoundsLowerRight - MapRect.BoundsUpperLeft; // luckily, we only need the dimensions to recreate the head node - it has no parent.
             }

             this.snapshotBounds = sn.DoVector2(snapshotBounds);

             sn.Ignore(listToRebuildFromPostLoad); // the client should provide this list post-load.        
             sn.Ignore(headNode); // ignored, will be recreated post load

             return this;
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

         public void LoadPostProcess(Snapshotter sn)
         {
             sn.RegisterLoadPostProcessCall(this);

             CollideShape2D bounds = new CollideShape2D(Vector2.Zero, snapshotBounds);
             RecreateFromList(bounds, listToRebuildFromPostLoad);
         }


         #endregion

         private void RecreateFromList(CollideShape2D/* RectangleF*/ bounds, List<Collidable<T>> /* List<Tuple<T, Vector2, Vector2>>*/ listOfObjects) 
         {
             // Destroy the head node
             if (headNode != null)
             {
                 headNode.Destroy();
                 headNode = null;
             }

            // containedObjects.Clear();

             // Create a new head
             headNode = new QTNode<T>(bounds, maxNodeCollidablesBeforePartition, Resize); // 0, maxDepth);

             // reinsert all objects
             foreach (var item in listOfObjects)
             {
                 AddCollidable(item);
                 //AddCollidable(item.Item1, item.Item2, item.Item3);
                // AddObject(item.First, item.Second);
             }
         }

         /// <summary>
         /// must be called prior to LoadPostProcess!!!
         /// </summary>
         /// <param name="listToRebuildFrom"></param>
         public void SetPreLoadPostProcess(List<Collidable<T>> listToRebuildFrom)
             //List<Tuple<T, Vector2, Vector2>> listToRebuildFrom) 
         {
             listToRebuildFromPostLoad = listToRebuildFrom;
         }

         /// <summary>
         /// to be called prior to snapshotting
         /// </summary>
         /// <returns></returns>
         //public List<Tuple<T, Vector2, Vector2>> GetAllObjectsAndPositions()
         public List<T> GetAllObjects()
         {
             List<Collidable<T>> collidables = new List<Collidable<T>>();
             GetAllCollidables(ref collidables);

              return collidables.Select(o => o.Parent).ToList();
            // return collidables.Select(o => new Tuple<T, Vector2, Vector2>(o.Parent, o. .Key, o.Value.ObjectAndPosition.Second)).ToList();

         }

     }




 }


