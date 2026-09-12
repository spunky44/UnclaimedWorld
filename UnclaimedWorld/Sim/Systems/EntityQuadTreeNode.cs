/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Collisions;
using System.Drawing;

namespace UWGame.SimSide.Systems
{
    public class EntityQuadTreeNode
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
        /// The maximum number of entities in this node before partitioning
        /// </summary>
        protected int maxNodeCollidablesBeforePartition;

        protected bool isPartitioned;

        protected EntityQuadTreeNode parentNode;

        protected EntityQuadTreeNode topLeftNode;
        protected EntityQuadTreeNode topRightNode;
        protected EntityQuadTreeNode bottomLeftNode;
        protected EntityQuadTreeNode bottomRightNode;

        protected List<Entity> collidableEntities;

        protected MapSizeChangeDelegate MapResize;


        //ctor
        public EntityQuadTreeNode(EntityQuadTreeNode parentNode, CollideShape2D rect, int maxCollidablesPerNode)
        {
            this.parentNode = parentNode;
            Bounds = rect;
            maxNodeCollidablesBeforePartition = maxCollidablesPerNode;
            isPartitioned = false;
            collidableEntities = new List<Entity>();
        }

        //ctor
        public EntityQuadTreeNode(CollideShape2D rect, int maxCollidablesPerNode, MapSizeChangeDelegate mapResize)
        {
            parentNode = null;
            Bounds = rect;
            maxNodeCollidablesBeforePartition = maxCollidablesPerNode;
            MapResize = mapResize;
            isPartitioned = false;
            collidableEntities = new List<Entity>();
        }


        public void Insert(Entity entityToInsert)
        {
            // If partitioned, try to find child node to add to
            if (!InsertInChild(entityToInsert))
            {
                collidableEntities.Add(entityToInsert);

                // Check if this node needs to be partitioned
                if (!isPartitioned && collidableEntities.Count >= maxNodeCollidablesBeforePartition)
                {
                    Partition();
                }
            }
        }

        /// <summary>
        /// Inserts an entity into one of this node's children
        /// </summary>
        protected bool InsertInChild(Entity entityToInsert)
        {
            if (!isPartitioned)
                return false;

            if (topLeftNode.EnvelopsEntity(entityToInsert))
                topLeftNode.Insert(entityToInsert);
            else if (topRightNode.EnvelopsEntity(entityToInsert))
                topRightNode.Insert(entityToInsert);
            else if (bottomLeftNode.EnvelopsEntity(entityToInsert))
                bottomLeftNode.Insert(entityToInsert);
            else if (bottomRightNode.EnvelopsEntity(entityToInsert))
                bottomRightNode.Insert(entityToInsert);

            else
                return false; // insert in child failed

            return true;
        }
        public bool EnvelopsEntity(Entity entityToCheck)
        {
            Vector2 positionToCheck = entityToCheck.Location.ToVector2();
            return bounds.ContainsPoint(positionToCheck);
        }
        /// <summary>
        /// Pushes an entity down to one of this node's children
        /// </summary>
        public bool PushEntityDown(int i)
        {
            if (InsertInChild(collidableEntities[i]))
            {
                RemoveEntityAtIndex(i);
                return true;
            }

            else
                return false;
        }

        /// <summary>
        /// Push an entity up to this node's parent
        /// </summary>
        public void PushEntityleUp(int i)
        {
            Entity m = collidableEntities[i];

            RemoveEntityAtIndex(i);
            parentNode.Insert(m);
        }

        /// <summary>
        /// Repartitions this node
        /// </summary>
        protected void Partition()
        {
            // Create the nodes
            Vector2 midPoint = Vector2.Divide(Vector2.Add(Bounds.BoundsUpperLeft, Bounds.BoundsLowerRight), 2.0f);

            topLeftNode = new EntityQuadTreeNode(this, new CollideShape2D(Bounds.BoundsUpperLeft, midPoint), maxNodeCollidablesBeforePartition);
            topRightNode = new EntityQuadTreeNode(this, new CollideShape2D(new Vector2(midPoint.X, Bounds.BoundsTop), new Vector2(Bounds.BoundsRight, midPoint.Y)), maxNodeCollidablesBeforePartition);
            bottomLeftNode = new EntityQuadTreeNode(this, new CollideShape2D(new Vector2(Bounds.BoundsLeft, midPoint.Y), new Vector2(midPoint.X, Bounds.BoundsBottom)), maxNodeCollidablesBeforePartition);
            bottomRightNode = new EntityQuadTreeNode(this, new CollideShape2D(midPoint, Bounds.BoundsLowerRight), maxNodeCollidablesBeforePartition);

            isPartitioned = true;

            // Try to push entities down to child nodes
            int i = 0;
            while (i < collidableEntities.Count)
            {
                if (!PushEntityDown(i))
                {
                    i++;
                }
            }
        }

        /// <summary>
        /// adds all entities intersecting a specified shapes' bounding rectangle to the list referenced
        /// </summary>
        public void GetEntitiesIntersectingBounds(CollideShape2D bounds, ref List<Entity> foundEntities)
        {
            if (bounds.isBoundsOverlap(bounds))
            {
                foreach (Entity collidableEntity in collidableEntities)
                {
                    if (bounds.ContainsPoint(collidableEntity.Location.ToVector2()))
                    {
                        if (foundEntities == null)
                        {
                            foundEntities = new List<Entity>();
                        }

                        foundEntities.Add(collidableEntity);
                    }
                }

                // query all subtrees
                if (isPartitioned)
                {
                    topLeftNode.GetEntitiesIntersectingBounds(bounds, ref foundEntities);
                    topRightNode.GetEntitiesIntersectingBounds(bounds, ref foundEntities);
                    bottomLeftNode.GetEntitiesIntersectingBounds(bounds, ref foundEntities);
                    bottomRightNode.GetEntitiesIntersectingBounds(bounds, ref foundEntities);
                }
            }
        }

        /// <summary>
        /// adds all entities in the node to the list referenced
        /// </summary>
        public void GetAllEntitiesInNode(ref List<Entity> foundEntities)
        {
            if (collidableEntities.Count > 0)
            {
                if (foundEntities == null && collidableEntities.Count > 0)
                {
                    foundEntities = new List<Entity>();
                }

                foundEntities.AddRange(collidableEntities);
            }

            // query all subtrees
            if (isPartitioned)
            {
                topLeftNode.GetAllEntitiesInNode(ref foundEntities);
                topRightNode.GetAllEntitiesInNode(ref foundEntities);
                bottomLeftNode.GetAllEntitiesInNode(ref foundEntities);
                bottomRightNode.GetAllEntitiesInNode(ref foundEntities);
            }
        }

        /// <summary>
        /// Finds the node containing a specified entity
        /// </summary>
        public EntityQuadTreeNode FindNodeContainingEntity(Entity entityToFindNodeWith)
        {
            if (collidableEntities.Contains(entityToFindNodeWith))
                return this;

            else if (isPartitioned)
            {
                EntityQuadTreeNode n = null;

                // Check the nodes that could contain the entity
                if (topLeftNode.EnvelopsEntity(entityToFindNodeWith))
                {
                    n = topLeftNode.FindNodeContainingEntity(entityToFindNodeWith);
                }
                if (n == null &&
                    topRightNode.EnvelopsEntity(entityToFindNodeWith))
                {
                    n = topRightNode.FindNodeContainingEntity(entityToFindNodeWith);
                }
                if (n == null &&
                    bottomLeftNode.EnvelopsEntity(entityToFindNodeWith))
                {
                    n = bottomLeftNode.FindNodeContainingEntity(entityToFindNodeWith);
                }
                if (n == null &&
                    bottomRightNode.EnvelopsEntity(entityToFindNodeWith))
                {
                    n = bottomRightNode.FindNodeContainingEntity(entityToFindNodeWith);
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
            while (collidableEntities.Count > 0)
            {
                RemoveEntityAtIndex(0);
            }
        }

        /// <summary>
        /// Removes an entity from this node
        /// </summary>
        public bool RemoveEntity(Entity entityToRemove)
        {
            if (collidableEntities.Contains(entityToRemove))
            {
                collidableEntities.Remove(entityToRemove);
                return true;
            }
            else
                if (isPartitioned)
                {
                    if (topLeftNode.RemoveEntity(entityToRemove))
                    {
                        return true;
                    }
                    else
                        if (topRightNode.RemoveEntity(entityToRemove))
                        {
                            return true;
                        }
                        else
                            if (bottomLeftNode.RemoveEntity(entityToRemove))
                            {
                                return true;
                            }
                            else
                                if (bottomRightNode.RemoveEntity(entityToRemove))
                                {
                                    return true;
                                }
                }
            return false;
        }

        /// <summary>
        /// Removes an entity from this node at a specific index
        /// </summary>
        protected void RemoveEntityAtIndex(int index)
        {
            if (index < collidableEntities.Count)
            {
                collidableEntities.RemoveAt(index);
            }
        }
    }
}*/
