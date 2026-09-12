using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Collisions;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Entities;
using System.Drawing;

namespace UWGame.SimSide.Systems
{
    /// <summary>
    /// A version of CollisionManager which is simpler, because it considers objects points instead of variable sized.
    /// A quad tree for storing Point objects (as opposed to objects with a radius and/or shape)
    /// T is the type we are storing in the tree (Entity or EntityID)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PointQuadTree<T> : ISnapshot
    {
        /// <summary>
        /// The head node of the quad tree
        /// </summary>
        protected PointQuadTreeNode<T> headNode;


        /// <summary>
        /// Gets the map rectangle - never used it seems
        /// </summary>
        public RectangleF MapRect
        {
            get
            {
                return headNode.Bounds;
            }
        }

        private RectangleF snapshotBounds;


        /// <summary>
        /// The maximum number of entities in any node before partitioning
        /// </summary>
        protected int maxNodeEntitiesBeforePartition;
        private int maxDepth;

        /// <summary>
        /// keep the full list of node dwellers to easily update and return them
        /// </summary>
        private Dictionary<T, PointTreeDweller<T>> containedObjects = new Dictionary<T, PointTreeDweller<T>>();
        private List<Pair<T, Vector2>> listToRebuildFromPostLoad = new List<Pair<T, Vector2>>(); // this must be set prior to LoadPostProcess to rebuild the tree


        public PointQuadTree()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor");
        }

        //ctor
        public PointQuadTree(RectangleF worldRect, int maxCollidablesPerNode, int maxDepth)
        {
            this.headNode = new PointQuadTreeNode<T>(worldRect, maxCollidablesPerNode, /*Resize,*/ 0, maxDepth);
            this.maxNodeEntitiesBeforePartition = maxCollidablesPerNode;
            this.maxDepth = maxDepth;
        }

        //ctor
        public PointQuadTree(Vector2 size, int maxCollidablesPerNode, int maxDepth)
            : this(new RectangleF(0f, 0f, size.X, size.Y), maxCollidablesPerNode, maxDepth)        
        {
            // Nothing extra to initialize

        }

        public bool RemoveObject(T objectToRemove)
        {
            if (containedObjects.ContainsKey(objectToRemove))
            {
                headNode.RemoveObject(containedObjects[objectToRemove]);
                containedObjects.Remove(objectToRemove);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool Contains(T objectToCheck)
        {
            return containedObjects.ContainsKey(objectToCheck);
        }

        /// <summary>
        /// Inserts an existing object into the quad tree
        /// </summary>
        public void AddObject(T objectToAdd, Vector2 point)
        {
            //if (containedObjects.ContainsKey(objectToAdd))
            //    return;

            // check if the world needs resizing
            if (!headNode.EnvelopsPoint(point))
            {
                throw new Exception("pointObject wanted to be placed outside pointObject tree, currently this case is not handled!");
            }

            PointTreeDweller<T> newObject = new PointTreeDweller<T>();

            Pair<T, Vector2> objectAndPosition = new Pair<T, Vector2>(objectToAdd, point);
            newObject.ObjectAndPosition = objectAndPosition;

            containedObjects.Add(objectToAdd, newObject);
          //  containedObjects.Add(objectToAdd, point);

            headNode.Insert(newObject); // this sets Parent
        }

        public bool UpdateObject(T objectToUpdate, Vector2 point)
        {
            PointTreeDweller<T> dwellerToUpdate;
            if (containedObjects.TryGetValue(objectToUpdate, out dwellerToUpdate))
            {
               // PointTreeDweller<T> dwellerToUpdate = containedObjects[objectToUpdate];
                if (!Common.IsLocationEqual(dwellerToUpdate.ObjectAndPosition.Second, point)) // dwellerToUpdate.ObjectAndPosition.Second != point)
                {
                    dwellerToUpdate.ObjectAndPosition.Second = point;

                    if (dwellerToUpdate.ContainingNode.EnvelopsPoint(dwellerToUpdate.ObjectAndPosition.Second) == false)
                    {
                        dwellerToUpdate.ContainingNode.PushObjectUp(dwellerToUpdate);
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Resizes the quad tree map world size to the bounds of shape passed in - 
        /// never used... test if needed...
        /// </summary>
        public void Resize(/*CollideShape2D*/ RectangleF newMapSize)
        {
            // Get all of the collidables in the tree
           /* List<PointTreeDweller<T>> pointObjectBucket = new List<PointTreeDweller<T>>();
            GetAllObjects(ref pointObjectBucket);
            */

            List<Pair<T, Vector2>> listOfObjects = GetAllObjectsAndPositions();

            RecreateFromList(newMapSize, listOfObjects); //pointObjectBucket);
        }



        private void RecreateFromList(RectangleF newMapSize, List<Pair<T, Vector2>> listOfObjects) // List<PointTreeDweller<T>> pointObjectBucket)
        {
            // Destroy the head node
            if (headNode != null)
            {
                headNode.Destroy();
                headNode = null;
            }

            containedObjects.Clear();

            // Create a new head
            headNode = new PointQuadTreeNode<T>(newMapSize, maxNodeEntitiesBeforePartition, /*Resize,*/ 0, maxDepth);

            // reinsert all objects
            foreach (var item in listOfObjects)
            {
                AddObject(item.First, item.Second);
            }
        }

        /// <summary>
        /// adds entities intersecting a specified rectangle to the list referenced
        /// </summary>
        public void GetObjectsIntersectingBounds(CollideShape2D bounds, Predicate<T> filter, ref List<Pair<T, Vector2>> resultsList)
        {
            headNode.GetObjectsIntersectingBounds(bounds, ref resultsList);

            if (resultsList != null)
            {
                T collidableObject;
                for (int i = resultsList.Count - 1; i >= 0; i--)
                {
                    collidableObject = resultsList[i].First;

                    // filter result based on predicate:
                    if (collidableObject == null || (filter != null && !filter(collidableObject)))
                    {
                        resultsList.RemoveAt(i);
                    }
                }
            }
        }

        /// <summary>
        /// adds all entities in the quad tree to a list         
        /// why not just return containedObjects??? please don't call this method
        /// </summary>
        public void GetAllObjects(ref List<PointTreeDweller<T>> pointObjectList)
        {
            // why not just return containedObjects??? please don't call this method

            headNode.GetAllObjectsInNode(ref pointObjectList);
        }


        /// <summary>
        /// to be called prior to snapshotting
        /// </summary>
        /// <returns></returns>
        public List<Pair<T, Vector2>> GetAllObjectsAndPositions()
        {
            return containedObjects.Select(o => new Pair<T, Vector2>(o.Key, o.Value.ObjectAndPosition.Second)).ToList();
           
        }

        /// <summary>
        /// when getting other agents around an agent, remember to filter that agent away!
        /// 
        /// searches inside containers, parts etc.
        /// 
        /// NOTE: list will be null if not entities are found!
        /// </summary>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        /// <param name="filter"></param>
        /// <param name="resultsList"></param>
        public void GetEntitiesInRange(Vector2 center, float radius, Predicate<T> filter, ref List<Pair<T, Vector2>> resultsList)
        {
            CollideShape2D bounds = new CollideShape2D(center, radius);

            GetObjectsIntersectingBounds(bounds, filter, ref resultsList);
        }


        #region ISnapshot


        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            // snapshot strategy:
            // instead of dealing with the graph of nodes, let's snapshot the list of members, then rebuild the tree after load.
            // but since the members can be a reference type like Entity, let the client handle them...

            this.maxDepth = sn.DoInt32(maxDepth);
            this.maxNodeEntitiesBeforePartition = sn.DoInt32(maxNodeEntitiesBeforePartition);

            if (sn.mode == Snapshotter.Mode.Save)
            {
                this.snapshotBounds = MapRect;               
            }

            this.snapshotBounds = sn.DoRectangleF(snapshotBounds);

            sn.Ignore(listToRebuildFromPostLoad); // the client should provide this list.
            sn.Ignore(containedObjects); // ignored because we will recreate the structure after load
            sn.Ignore(headNode); // ignored for same reason

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

            RecreateFromList(snapshotBounds, listToRebuildFromPostLoad);
        }


        #endregion

        /// <summary>
        /// must be called prior to LoadPostProcess!!!
        /// </summary>
        /// <param name="listToRebuildFrom"></param>
        public void SetPreLoadPostProcess(List<Pair<T, Vector2>> listToRebuildFrom) //List<T> listToRebuildFrom)
        {
            listToRebuildFromPostLoad = listToRebuildFrom;
        }
    }
}
