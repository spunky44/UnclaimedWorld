using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace UWGame.SimSide.Collisions
{

    /// <summary>
    /// A node in a quad tree
    /// </summary>
    public class QTNode<T>
    {
        public delegate void MapSizeChangeDelegate(CollideShape2D newSize);

        protected CollideShape2D bounds;
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
        }

        /// <summary>
        /// The maximum number of collidables in this node before partitioning
        /// </summary>
        protected int maxNodeCollidablesBeforePartition;

        protected bool isPartitioned;

        protected QTNode<T> parentNode;

        protected QTNode<T> topLeftNode;
        protected QTNode<T> topRightNode;
        protected QTNode<T> bottomLeftNode;
        protected QTNode<T> bottomRightNode;

        protected List<Collidable<T>> collidables;

        protected MapSizeChangeDelegate MapResize;


        //ctor
        public QTNode(QTNode<T> parentNode, CollideShape2D rect, int maxCollidablesPerNode)
        {
            this.parentNode = parentNode;
            Bounds = rect;
            maxNodeCollidablesBeforePartition = maxCollidablesPerNode;
            isPartitioned = false;
            collidables = new List<Collidable<T>>();
        }

        //ctor
        public QTNode(CollideShape2D rect, int maxCollidablesPerNode, MapSizeChangeDelegate mapResize)
        {
            parentNode = null;
            Bounds = rect;
            maxNodeCollidablesBeforePartition = maxCollidablesPerNode;
            MapResize = mapResize;
            isPartitioned = false;
            collidables = new List<Collidable<T>>();
        }


        public void Insert(Collidable<T> collidable)
        {
            // If partitioned, try to find child node to add to
            if (!InsertInChild(collidable))
            {
                collidable.Destroy += new Collidable<T>.DestroyHandlerDelegate(CollidableDestroy);
                collidable.Move += new Collidable<T>.MoveHandlerDelegate(CollidableMove);
                collidables.Add(collidable);

                // Check if this node needs to be partitioned
                if (!isPartitioned && collidables.Count >= maxNodeCollidablesBeforePartition)
                {
                    Partition();
                }
            }
        }

        /// <summary>
        /// Inserts an collidable into one of this node's children
        /// </summary>
        protected bool InsertInChild(Collidable<T> collidable)
        {
            if (!isPartitioned)
                return false;

            if (topLeftNode.ContainsRect(collidable.Bounds))
                topLeftNode.Insert(collidable);
            else if (topRightNode.ContainsRect(collidable.Bounds))
                topRightNode.Insert(collidable);
            else if (bottomLeftNode.ContainsRect(collidable.Bounds))
                bottomLeftNode.Insert(collidable);
            else if (bottomRightNode.ContainsRect(collidable.Bounds))
                bottomRightNode.Insert(collidable);

            else
                return false; // insert in child failed

            return true;
        }

        /// <summary>
        /// Pushes a collidable down to one of this node's children
        /// </summary>
        public bool PushCollidableDown(int i)
        {
            if (InsertInChild(collidables[i]))
            {
                RemoveCollidable(i);
                return true;
            }

            else
                return false;
        }

        /// <summary>
        /// Push a collidable up to this node's parent
        /// </summary>
        public void PushCollidableUp(int i)
        {
            Collidable<T> m = collidables[i];

            RemoveCollidable(i);
            parentNode.Insert(m);
        }

        /// <summary>
        /// Repartitions this node
        /// </summary>
        protected void Partition()
        {
            // Create the nodes
            Vector2 midPoint = Vector2.Divide(Vector2.Add(Bounds.BoundsUpperLeft, Bounds.BoundsLowerRight), 2.0f);

            topLeftNode = new QTNode<T>(this, new CollideShape2D(Bounds.BoundsUpperLeft, midPoint), maxNodeCollidablesBeforePartition);
            topRightNode = new QTNode<T>(this, new CollideShape2D(new Vector2(midPoint.X, Bounds.BoundsTop), new Vector2(Bounds.BoundsRight, midPoint.Y)), maxNodeCollidablesBeforePartition);
            bottomLeftNode = new QTNode<T>(this, new CollideShape2D(new Vector2(Bounds.BoundsLeft, midPoint.Y), new Vector2(midPoint.X, Bounds.BoundsBottom)), maxNodeCollidablesBeforePartition);
            bottomRightNode = new QTNode<T>(this, new CollideShape2D(midPoint, Bounds.BoundsLowerRight), maxNodeCollidablesBeforePartition);

            isPartitioned = true;

            // Try to push collidables down to child nodes
            int i = 0;
            while (i < collidables.Count)
            {
                if (!PushCollidableDown(i))
                {
                    i++;
                }
            }
        }



        /// <summary>
        /// adds all collidables containing specified point to the list referenced
        /// </summary>
        public void GetCollidablesContainingPoint(Vector2 Point, ICollection<Collidable<T>> collidablesFound) // ref List<Collidable<T>> collidablesFound)
        {
            // test the point against this node
            if (Bounds.ContainsPoint(Point))
            {
                // test the point in each collidable
                foreach (Collidable<T> col in collidables)
                {
                    if (col.Bounds.ContainsPoint(Point))
                        collidablesFound.Add(col);
                }

                // query all subtrees
                if (isPartitioned)
                {
                    topLeftNode.GetCollidablesContainingPoint(Point, collidablesFound);
                    topRightNode.GetCollidablesContainingPoint(Point, collidablesFound);
                    bottomLeftNode.GetCollidablesContainingPoint(Point, collidablesFound);
                    bottomRightNode.GetCollidablesContainingPoint(Point, collidablesFound);
                }
            }
        }
      /*  public void GetCollidablesContainingPoint(Vector2 Point, ref List<Collidable<T>> collidablesFound) //ICollection<Collidable<T>> collidablesFound) // ref List<Collidable<T>> collidablesFound)
        {

           // HashSet<T> set
            // test the point against this node
            if (Bounds.ContainsPoint(Point))
            {
                // test the point in each collidable
                foreach (Collidable<T> col in collidables)
                {
                    if (col.Bounds.ContainsPoint(Point))
                        collidablesFound.Add(col);
                }

                // query all subtrees
                if (isPartitioned)
                {
                    topLeftNode.GetCollidablesContainingPoint(Point, ref collidablesFound);
                    topRightNode.GetCollidablesContainingPoint(Point, ref collidablesFound);
                    bottomLeftNode.GetCollidablesContainingPoint(Point, ref collidablesFound);
                    bottomRightNode.GetCollidablesContainingPoint(Point, ref collidablesFound);
                }
            }
        }*/

        /// <summary>
        /// adds all collidables intersecting a specified shapes' bounding rectangle to the list referenced
        /// </summary>
        public void GetCollidablesIntersectingBounds(CollideShape2D bounds, ref List<Collidable<T>> collidablesFound)
        {
            // test the point against this node
            if (this.bounds.isBoundsOverlap(bounds))
            //if (bounds.isBoundsOverlap(bounds)) // Bug here??? testing bounds parameter against itself???
            {
                // test the point in each collidable
                foreach (Collidable<T> col in collidables)
                {
                    //Here, we could first test whether col == bounds, which means it collides with itself
                    //Skip testing collisions with yourself, right?
                    //if (bounds.Parent == col.Bounds.Parent)
                    //    continue;
                    //the parent doesn't work, parent is always null... why?

                    if (col.Bounds.isBoundsOverlap(bounds))
                    {
                        if (collidablesFound == null)
                        {
                            collidablesFound = new List<Collidable<T>>();
                        }

                        collidablesFound.Add(col);
                    }
                }

                // query all subtrees
                if (isPartitioned)
                {
                    topLeftNode.GetCollidablesIntersectingBounds(bounds, ref collidablesFound);
                    topRightNode.GetCollidablesIntersectingBounds(bounds, ref collidablesFound);
                    bottomLeftNode.GetCollidablesIntersectingBounds(bounds, ref collidablesFound);
                    bottomRightNode.GetCollidablesIntersectingBounds(bounds, ref collidablesFound);
                }
            }
        }

        /// <summary>
        /// adds all collidables in thie node to the list referenced
        /// </summary>
        public void GetAllICollidablesInNode(ref List<Collidable<T>> collidablesFound)
        {
            if (collidablesFound == null && collidables.Count > 0)
            {
                collidablesFound = new List<Collidable<T>>();
            }

            collidablesFound.AddRange(collidables);

            // query all subtrees
            if (isPartitioned)
            {
                topLeftNode.GetAllICollidablesInNode(ref collidablesFound);
                topRightNode.GetAllICollidablesInNode(ref collidablesFound);
                bottomLeftNode.GetAllICollidablesInNode(ref collidablesFound);
                bottomRightNode.GetAllICollidablesInNode(ref collidablesFound);
            }
        }

        /// <summary>
        /// Finds the node containing a specified collidable
        /// </summary>
        public QTNode<T> FindNodeContainingCollidable(Collidable<T> collidable)
        {
            if (collidables.Contains(collidable))
                return this;

            else if (isPartitioned)
            {
                QTNode<T> n = null;

                // Check the nodes that could contain the collidable
                if (topLeftNode.ContainsRect(collidable.Bounds))
                {
                    n = topLeftNode.FindNodeContainingCollidable(collidable);
                }
                if (n == null &&
                    topRightNode.ContainsRect(collidable.Bounds))
                {
                    n = topRightNode.FindNodeContainingCollidable(collidable);
                }
                if (n == null &&
                    bottomLeftNode.ContainsRect(collidable.Bounds))
                {
                    n = bottomLeftNode.FindNodeContainingCollidable(collidable);
                }
                if (n == null &&
                    bottomRightNode.ContainsRect(collidable.Bounds))
                {
                    n = bottomRightNode.FindNodeContainingCollidable(collidable);
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

            // Remove all collidables
            while (collidables.Count > 0)
            {
                RemoveCollidable(0);
            }
        }

        /// <summary>
        /// Removes an collidables from this node
        /// </summary>
        public void RemoveCollidable(Collidable<T> collidable)
        {
            // Find and remove the collidable
            if (collidables.Contains(collidable))
            {
                collidable.Move -= new Collidable<T>.MoveHandlerDelegate(CollidableMove);
                collidable.Destroy -= new Collidable<T>.DestroyHandlerDelegate(CollidableDestroy);
                collidables.Remove(collidable);
            }
        }

        /// <summary>
        /// Removes a collidable from this node at a specific index
        /// </summary>
        protected void RemoveCollidable(int idx)
        {
            if (idx < collidables.Count)
            {
                collidables[idx].Move -= new Collidable<T>.MoveHandlerDelegate(CollidableMove);
                collidables[idx].Destroy -= new Collidable<T>.DestroyHandlerDelegate(CollidableDestroy);
                collidables.RemoveAt(idx);
            }
        }



        public void CollidableMove(Collidable<T> collidable)
        {
            // Find the collidable
            if (collidables.Contains(collidable))
            {
                int i = collidables.IndexOf(collidable);

                // Try to push the collidable down to the child
                if (!PushCollidableDown(i))
                {
                    // otherwise, if not root, push up
                    if (parentNode != null)
                    {
                        PushCollidableUp(i);
                    }
                    else if (!ContainsRect(collidable.Bounds))
                    {
                        MapResize(new CollideShape2D(
                             Vector2.Min(Bounds.BoundsUpperLeft, collidable.Bounds.BoundsUpperLeft) * 2,
                             Vector2.Max(Bounds.BoundsLowerRight, collidable.Bounds.BoundsLowerRight) * 2));
                    }

                }
            }
            else
            {
                // this node doesn't contain that collidable, stop notifying it about it
                collidable.Move -= new Collidable<T>.MoveHandlerDelegate(CollidableMove);
            }
        }

        public void CollidableDestroy(Collidable<T> collidable)
        {
            RemoveCollidable(collidable);
        }



        /// <summary>
        /// Tests whether this node contains a shapes bounds
        /// </summary>
        public bool ContainsRect(CollideShape2D bnds)
        {
            return (bnds.BoundsUpperLeft.X >= Bounds.BoundsUpperLeft.X &&
                    bnds.BoundsUpperLeft.Y >= Bounds.BoundsUpperLeft.Y &&
                    bnds.BoundsLowerRight.X <= Bounds.BoundsLowerRight.X &&
                    bnds.BoundsLowerRight.Y <= Bounds.BoundsLowerRight.Y);
        }

    }
}

