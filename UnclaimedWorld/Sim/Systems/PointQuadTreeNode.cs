using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Collisions;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots;
using System.Drawing;

namespace UWGame.SimSide.Systems
{
    public class PointQuadTreeNode<T>
    {
        public delegate void MapSizeChangeDelegate(CollideShape2D newSize);


      /*  protected CollideShape2D bounds;
        public CollideShape2D Bounds
        {
            get
            {
                return bounds;
            }

            protected set
            {
                bounds = value;
            }
        }*/

        protected RectangleF bounds;
        public RectangleF Bounds
        {
            get
            {
                return bounds;
            }

            protected set
            {
                bounds = value;
            }
        }

        /// <summary>
        /// The maximum number of entities in this node before partitioning
        /// </summary>
        protected int maxNodeCollidablesBeforePartition;

        protected bool isPartitioned;

        protected PointQuadTreeNode<T> parentNode;

        /// <summary>
        /// the 4 parts that this node can be subdivided in (thereby the name: quadtree)
        /// </summary>
        protected PointQuadTreeNode<T> topLeftNode;
        protected PointQuadTreeNode<T> topRightNode;
        protected PointQuadTreeNode<T> bottomLeftNode;
        protected PointQuadTreeNode<T> bottomRightNode;

        /// <summary>
        /// does NOT include objects in child nodes
        /// </summary>
        protected List<PointTreeDweller<T>> objectsInThisNode;

      //  protected MapSizeChangeDelegate MapResize;

        private int depth;
        private int maxDepth;

        //ctor
        public PointQuadTreeNode(PointQuadTreeNode<T> parentNode, RectangleF rect, int maxCollidablesPerNode, int depth, int maxDepth)
        {
            this.parentNode = parentNode;
            Bounds = rect;
            maxNodeCollidablesBeforePartition = maxCollidablesPerNode;
            isPartitioned = false;
            objectsInThisNode = new List<PointTreeDweller<T>>();
            this.depth = depth;
            this.maxDepth = maxDepth;
        }

        //ctor
        public PointQuadTreeNode(/*CollideShape2D*/ RectangleF rect, int maxCollidablesPerNode, /*MapSizeChangeDelegate mapResize,*/ int depth, int maxDepth)
        {
            parentNode = null;
            Bounds = rect;
            maxNodeCollidablesBeforePartition = maxCollidablesPerNode;
          //  MapResize = mapResize;
            isPartitioned = false;
            objectsInThisNode = new List<PointTreeDweller<T>>();
            this.depth = depth;
            this.maxDepth = maxDepth;
        }


        public void Insert(PointTreeDweller<T> objectToInsert)
        {
            // If partitioned, try to find child node to add to
            if (!InsertInChild(objectToInsert))
            {
                objectsInThisNode.Add(objectToInsert);
                objectToInsert.ContainingNode = this;

                // Check if this node needs to be partitioned
                if (!isPartitioned && objectsInThisNode.Count >= maxNodeCollidablesBeforePartition)
                {
                    Partition();
                }
            }
        }

        /// <summary>
        /// Inserts an entity into one of this node's children
        /// </summary>
        protected bool InsertInChild(PointTreeDweller<T> pointObject)
        {
            if (!isPartitioned)
                return false;

            if (topLeftNode.EnvelopsPoint(pointObject.ObjectAndPosition.Second))
                topLeftNode.Insert(pointObject);
            else if (topRightNode.EnvelopsPoint(pointObject.ObjectAndPosition.Second))
                topRightNode.Insert(pointObject);
            else if (bottomLeftNode.EnvelopsPoint(pointObject.ObjectAndPosition.Second))
                bottomLeftNode.Insert(pointObject);
            else if (bottomRightNode.EnvelopsPoint(pointObject.ObjectAndPosition.Second))
                bottomRightNode.Insert(pointObject);

            else
                return false; // insert in child failed

            return true;
        }

        public bool EnvelopsPoint(Vector2 point)
        {
            return bounds.Contains(point.X, point.Y);
           // return bounds.ContainsPoint(point);
        }

        /// <summary>
        /// Pushes an entity down to one of this node's children
        /// </summary>
        public bool PushEntityDown(int i)
        {
            if (InsertInChild(objectsInThisNode[i]))
            {
                RemovePointObjectAtIndex(i);
                return true;
            }

            else
                return false;
        }



        /// <summary>
        /// Pushes an object upwards until it finds a fitting Node
        /// </summary>
        public bool PushObjectUp(PointTreeDweller<T> objectToPushUp)
        {
            bool objectRemoved = RemoveObject(objectToPushUp);
            if (objectRemoved)
            {
                parentNode.InsertOrPassUp(objectToPushUp, this);
            }
            else
            {
                int i = 3;//This should never occur, this means that the object we wanted to push up is not in the node.
              
            }
            return objectRemoved;
        }

        private void InsertOrPassUp(PointTreeDweller<T> objectToInsert, PointQuadTreeNode<T> sender)
        {
            if (topLeftNode != sender)
            {
                if (topLeftNode.EnvelopsPoint(objectToInsert.ObjectAndPosition.Second))
                {
                    topLeftNode.Insert(objectToInsert);
                    return;
                }
            }

            if (topRightNode != sender)
            {
                if (topRightNode.EnvelopsPoint(objectToInsert.ObjectAndPosition.Second))
                {
                    topRightNode.Insert(objectToInsert);
                    return;
                }
            }

            if (bottomLeftNode != sender)
            {
                if (bottomLeftNode.EnvelopsPoint(objectToInsert.ObjectAndPosition.Second))
                {
                    bottomLeftNode.Insert(objectToInsert);
                    return;
                }
            }

            if (bottomRightNode != sender)
            {
                if (bottomRightNode.EnvelopsPoint(objectToInsert.ObjectAndPosition.Second))
                {
                    bottomRightNode.Insert(objectToInsert);
                    return;
                }
            }

            if (parentNode != null)
            {
                parentNode.InsertOrPassUp(objectToInsert, this);
            }
            else
            {
                throw new Exception("Object did not fit anywhere in the grid!");
            }

        }

        /// <summary>
        /// Repartitions this node
        /// </summary>
        protected void Partition()
        {
            if (depth >= maxDepth)
            {
                return;//Prevents infinite recursive resizing
            }
            // Create the nodes
          /*  Vector2 midPoint = Vector2.Divide(Vector2.Add(Bounds.BoundsUpperLeft, Bounds.BoundsLowerRight), 2.0f);

            topLeftNode = new PointQuadTreeNode<T>(this, new CollideShape2D(Bounds.BoundsUpperLeft, midPoint), maxNodeCollidablesBeforePartition, depth + 1, maxDepth);
            topRightNode = new PointQuadTreeNode<T>(this, new CollideShape2D(new Vector2(midPoint.X, Bounds.BoundsTop), new Vector2(Bounds.BoundsRight, midPoint.Y)), maxNodeCollidablesBeforePartition, depth + 1, maxDepth);
            bottomLeftNode = new PointQuadTreeNode<T>(this, new CollideShape2D(new Vector2(Bounds.BoundsLeft, midPoint.Y), new Vector2(midPoint.X, Bounds.BoundsBottom)), maxNodeCollidablesBeforePartition, depth + 1, maxDepth);
            bottomRightNode = new PointQuadTreeNode<T>(this, new CollideShape2D(midPoint, Bounds.BoundsLowerRight), maxNodeCollidablesBeforePartition, depth + 1, maxDepth);
            */

            Vector2 upperLeft = new Vector2(Bounds.Left, Bounds.Top);
            Vector2 lowerRight = new Vector2(Bounds.Right, Bounds.Bottom);
            
            Vector2 midPoint = Vector2.Divide(Vector2.Add(upperLeft, lowerRight), 2.0f);

            float width = bounds.Width / 2f;
            float height = bounds.Height / 2f;

            topLeftNode = new PointQuadTreeNode<T>(this, new RectangleF(Bounds.X, Bounds.Y, width, height), maxNodeCollidablesBeforePartition, depth + 1, maxDepth);
            topRightNode = new PointQuadTreeNode<T>(this, new RectangleF(midPoint.X, Bounds.Y, width, height), maxNodeCollidablesBeforePartition, depth + 1, maxDepth);
            bottomLeftNode = new PointQuadTreeNode<T>(this, new RectangleF(Bounds.X, midPoint.Y, width, height), maxNodeCollidablesBeforePartition, depth + 1, maxDepth);
            bottomRightNode = new PointQuadTreeNode<T>(this, new RectangleF(midPoint.X, midPoint.Y, width, height), maxNodeCollidablesBeforePartition, depth + 1, maxDepth);
    
           /* topLeftNode = new PointQuadTreeNode<T>(this, new CollideShape2D(upperLeft, midPoint), maxNodeCollidablesBeforePartition, depth + 1, maxDepth);
            topRightNode = new PointQuadTreeNode<T>(this, new CollideShape2D(new Vector2(midPoint.X, Bounds.Top), new Vector2(Bounds.Right, midPoint.Y)), maxNodeCollidablesBeforePartition, depth + 1, maxDepth);
            bottomLeftNode = new PointQuadTreeNode<T>(this, new CollideShape2D(new Vector2(Bounds.X, midPoint.Y), new Vector2(midPoint.X, Bounds.Bottom)), maxNodeCollidablesBeforePartition, depth + 1, maxDepth);
            bottomRightNode = new PointQuadTreeNode<T>(this, new CollideShape2D(midPoint, lowerRight), maxNodeCollidablesBeforePartition, depth + 1, maxDepth);
          */

            isPartitioned = true;

            // Try to push entities down to child nodes
            int i = 0;
            while (i < objectsInThisNode.Count)
            {
                if (!PushEntityDown(i))
                {
                    i++;
                }
            }
        }

        /// <summary>
        /// adds all objects intersecting a specified shapes' bounding rectangle to the list referenced
        /// </summary>
        public void GetObjectsIntersectingBounds(CollideShape2D bounds, ref List<Pair<T, Vector2>> foundObjects)
        {
            //NEW: compute a bounding rect from teh shape which may be a circle:
            RectangleF rectangleToTest = new RectangleF(bounds.BoundsLeft, bounds.BoundsTop, bounds.BoundsWidth, bounds.BoundsHeight);

            if (this.bounds.IntersectsWith(rectangleToTest))
            //if (this.bounds.isBoundsOverlap(bounds))
          //  if (bounds.isBoundsOverlap(bounds)) // Bug here??? testing bounds parameter against itself???
            {
                foreach (PointTreeDweller<T> collidableObject in objectsInThisNode)
                {                   
                  //  float xPos = collidableObject.ObjectAndPosition.Second.X;
                  //  float yPos = collidableObject.ObjectAndPosition.Second.Y;
                   //if (bounds.Contains(xPos, yPos))
                    if (bounds.ContainsPoint(collidableObject.ObjectAndPosition.Second))
                    {
                        if (foundObjects == null)
                        {
                            foundObjects = new List<Pair<T, Vector2>>();
                        }

                        foundObjects.Add(collidableObject.ObjectAndPosition);
                    }
                }

                // query all subtrees
                if (isPartitioned)
                {
                    topLeftNode.GetObjectsIntersectingBounds(bounds, ref foundObjects);
                    topRightNode.GetObjectsIntersectingBounds(bounds, ref foundObjects);
                    bottomLeftNode.GetObjectsIntersectingBounds(bounds, ref foundObjects);
                    bottomRightNode.GetObjectsIntersectingBounds(bounds, ref foundObjects);
                }
            }
        }

        /// <summary>
        /// adds all entities in the node to the list referenced
        /// </summary>
        public void GetAllObjectsInNode(ref List<PointTreeDweller<T>> foundObjects)
        {
            if (objectsInThisNode.Count > 0)
            {
                if (foundObjects == null && objectsInThisNode.Count > 0)
                {
                    foundObjects = new List<PointTreeDweller<T>>();
                }

                foundObjects.AddRange(objectsInThisNode);
            }

            // query all subtrees
            if (isPartitioned)
            {
                topLeftNode.GetAllObjectsInNode(ref foundObjects);
                topRightNode.GetAllObjectsInNode(ref foundObjects);
                bottomLeftNode.GetAllObjectsInNode(ref foundObjects);
                bottomRightNode.GetAllObjectsInNode(ref foundObjects);
            }
        }

        /// <summary>
        /// Finds the node containing a specified object
        /// </summary>
        public PointQuadTreeNode<T> FindNodeContainingEntity(PointTreeDweller<T> objectToFindNodeWith)
        {
            if (objectsInThisNode.Contains(objectToFindNodeWith))
                return this;

            else if (isPartitioned)
            {
                PointQuadTreeNode<T> n = null;

                // Check the nodes that could contain the entity
                if (topLeftNode.EnvelopsPoint(objectToFindNodeWith.ObjectAndPosition.Second))
                {
                    n = topLeftNode.FindNodeContainingEntity(objectToFindNodeWith);
                }
                if (n == null && topRightNode.EnvelopsPoint(objectToFindNodeWith.ObjectAndPosition.Second))
                {
                    n = topRightNode.FindNodeContainingEntity(objectToFindNodeWith);
                }
                if (n == null && bottomLeftNode.EnvelopsPoint(objectToFindNodeWith.ObjectAndPosition.Second))
                {
                    n = bottomLeftNode.FindNodeContainingEntity(objectToFindNodeWith);
                }
                if (n == null &&
                    bottomRightNode.EnvelopsPoint(objectToFindNodeWith.ObjectAndPosition.Second))
                {
                    n = bottomRightNode.FindNodeContainingEntity(objectToFindNodeWith);
                }

                return n;
            }

            else
                return null;
        }



        public void Destroy()
        {
            // Destroy all child nodes
            if (isPartitioned)
            {
                topLeftNode.Destroy();
                topRightNode.Destroy();
                bottomLeftNode.Destroy();
                bottomRightNode.Destroy();

                topLeftNode = null;
                topRightNode = null;
                bottomLeftNode = null;
                bottomRightNode = null;
            }

            // Remove all entities
            while (objectsInThisNode.Count > 0)
            {
                RemovePointObjectAtIndex(0);
            }
        }

        /// <summary>
        /// Removes an object from this node
        /// </summary>
        public bool RemoveObject(PointTreeDweller<T> objectToRemove)
        {
            if (objectsInThisNode.Contains(objectToRemove))
            {
                objectsInThisNode.Remove(objectToRemove);
                return true;
            }
            else
                if (isPartitioned)
                {
                    if (topLeftNode.RemoveObject(objectToRemove))
                    {
                        return true;
                    }
                    else
                        if (topRightNode.RemoveObject(objectToRemove))
                        {
                            return true;
                        }
                        else
                            if (bottomLeftNode.RemoveObject(objectToRemove))
                            {
                                return true;
                            }
                            else
                                if (bottomRightNode.RemoveObject(objectToRemove))
                                {
                                    return true;
                                }
                }
            return false;
        }

        /// <summary>
        /// Removes an entity from this node at a specific index
        /// </summary>
        protected void RemovePointObjectAtIndex(int index)
        {
            if (index < objectsInThisNode.Count)
            {
                objectsInThisNode.RemoveAt(index);
            }
        }


    }
}
