/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using UWGame.SimSide.Collisions;
namespace UWGame.SimSide.Systems
{
   

    /// <summary>
    /// A quad tree for partitioning a space into rectangles, holds only entities
    /// </summary>

     public class EntityQuadTree
    {
 
        /// <summary>
        /// The head node of the quad tree
        /// </summary>
        protected EntityQuadTreeNode headNode;

        /// <summary>
        /// Gets the map rectangle
        /// </summary>
        public CollideShape2D MapRect
        {
            get
            {
                return headNode.Bounds;
            }
        }

        /// <summary>
        /// The maximum number of entities in any node before partitioning
        /// </summary>
        protected int maxNodeEntitiesBeforeParition;

 
 
        //ctor
        public EntityQuadTree(CollideShape2D worldRect, int maxCollidablesPerNode)
        {
            this.headNode = new EntityQuadTreeNode(worldRect, maxCollidablesPerNode, Resize);
            this.maxNodeEntitiesBeforeParition = maxCollidablesPerNode;
        }

        //ctor
        public EntityQuadTree(Vector2 size, int maxCollidablesPerNode)
            : this(new CollideShape2D(Vector2.Zero, size), maxCollidablesPerNode)
        {
            // Nothing extra to initialize
            
        }

        public void RemoveEntity(Entity anEntityToRemove)
        {
            headNode.RemoveEntity(anEntityToRemove);
        }
 
        /// <summary>
        /// Inserts an existing entity into the quad tree -- will resize if neede to encompas new position
        /// </summary>
         public void AddEntity(Entity collidableEntityToAdd)
        {             
            // check if the world needs resizing
            if (!headNode.EnvelopsEntity(collidableEntityToAdd))
            {
                throw new Exception("Entity wanted to be placed outside entity tree, currently this case is not handled!");
            }

            headNode.Insert(collidableEntityToAdd);
        }

        /// <summary>
        /// Resizes the quad tree map world size to the bounds of shape passed in
        /// </summary>
        public void Resize(CollideShape2D newMapSize)
        {
            // Get all of the collidables in the tree
            List<Entity> CollidableEntityBucket = new List<Entity>();
            GetAllEntities(ref CollidableEntityBucket);

            // Destroy the head node
            headNode.Destroy();
            headNode = null;

            // Create a new head
            headNode = new EntityQuadTreeNode(newMapSize, maxNodeEntitiesBeforeParition, Resize);

            // Reinsert all the collidables
            foreach (Entity collidableEntity in CollidableEntityBucket)
            {
                headNode.Insert(collidableEntity);
            }
        }

        /// <summary>
        /// adds entities intersecting a specified rectangle to the list referenced
        /// </summary>
        public void GetEntitiesIntersectingBounds(CollideShape2D bounds, ref List<Entity> collidablesList)
        {
             headNode.GetEntitiesIntersectingBounds(bounds, ref collidablesList); 
        }

        /// <summary>
         /// adds all entities in the quad tree to a list 
        /// </summary>
        public void GetAllEntities(ref List<Entity> collidablesList)
        {
            headNode.GetAllEntitiesInNode(ref collidablesList);
        }

       
         /// <summary>
         /// when getting other agents around an agent, remember to filter that agent away!
         /// </summary>
         /// <param name="center"></param>
         /// <param name="radius"></param>
         /// <param name="filter"></param>
         /// <param name="resultsList"></param>
         public void GetEntitiesInRange(Vector2 center, float radius, Predicate<Entity> filter, ref List<Entity> resultsList)
         {
             CollideShape2D bounds = new CollideShape2D(center, radius);

             GetEntitiesIntersectingBounds(bounds, ref resultsList);

             if (resultsList != null)
             {
                 Entity collidableEntity;
                 for (int i = resultsList.Count - 1; i >= 0; i--)
                 {
                     collidableEntity = resultsList[i]; 

                     // filter result based on predicate:
                     if (collidableEntity == null || (filter != null && !filter(collidableEntity)))
                     {
                         resultsList.RemoveAt(i);
                     }
                 }
             }

         }

     }


 }*/
