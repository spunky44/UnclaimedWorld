using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace UWGame.SimSide.Entities.Containers
{
    class Garrison: ISnapshot
    {
        List<EntityID> AgentsInside = new List<EntityID>();

        private Entity parent;
        EntityID snapshotParent;


        public Garrison(Entity parent)
        {
            this.parent = parent;
        }

        public Garrison()         
        {
            Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");     
        }


        public bool Contains(EntityID entity)
        {
            return AgentsInside.Contains(entity);
        }


        public bool Add(EntityID entity)
        {
            if (!AgentsInside.Contains(entity))
            {
                AgentsInside.Add(entity);
            }

            return true;
        }

        public bool Remove(EntityID entity)
        {
            return AgentsInside.Remove(entity);
        }

        public static bool EntityBelongs(Entity entity)
        {
            return entity.EntityType.IntelligenceType != null;
        }


        public int GetNoOfAgentsInside()
        {
            return AgentsInside.Count;
        }


        public void IterateContained(Action<Entity> iterateMethod) //Container.IterateMethod iterateMethod)
        {
            IterateList(AgentsInside, iterateMethod);            
        }

        public void GetContainedItemsList(Predicate<Entity> rule, List<Entity> items)
        {
            EntityID entityID;
            Entity entityInside;
            for (int i = AgentsInside.Count - 1; i >= 0; i--)
            {
                entityID = AgentsInside[i];

                entityInside = Entity.FindByID(entityID);
                if (entityInside != null)
                {
                    if (rule == null || rule(entityInside))
                    {
                        items.Add(entityInside);
                    }
                }
                else
                {
                    Remove(entityID);
                }
            }

        }

        public static void IterateList(List<EntityID> containedItems, Action<Entity> iterateMethod) //Container.IterateMethod iterateMethod)
        {
            EntityID entityID;
            Entity entityInside;
            for (int i = containedItems.Count - 1; i >= 0; i--)
            {
                entityID = containedItems[i];

                entityInside = Entity.FindByID(entityID);
                if (entityInside != null)
                {
                    iterateMethod(entityInside);
                }
                else
                {
                    containedItems.Remove(entityID);
                }
            }
        }


        /// <summary>
        /// ejects all agents inside
        /// </summary>
        /// <param name="damageStandardDev"></param>
        /// <param name="damageSpread"></param>
        public void UncontainAllEntities() //float? damageStandardDev = null, float? damageSpread = null) //float? chanceToDestroy = null)
        {
            EntityID entityID;
            Entity entity;
            for (int i = AgentsInside.Count - 1; i >= 0; i--)
            {
                entityID = AgentsInside[i];

                entity = Entity.FindByID(entityID);

                if (entity != null)
                {
                    if (parent.Contains.Remove(entity))
                    {
                        parent.Contains.EjectEntity(entity);
                    }
                }
                else
                {
                    // remove outdated entity:
                    AgentsInside.RemoveAt(i);                    
                }
            }

        }

        /*
        public static bool GetDoorPosition(Entity container, Vector2[] doors, ref Vector3 position, ref ExitDoor door, out ExitDoor doorThatWasUsed)
        {
            position = container.PlaySiteLocation;

            if (door == ExitDoor.NextAvailable)
            {
                //TODO: iterate my doors, and return the first available one

                //  Vector2 doorPosition = new Vector2(0); //replace this with ContainerType data
                //  position += doorPosition.ToVector3();

                // Vector2[] doors = parent.EntityType.ContainerType.HomeContainerType.Doors;
                door = ExitDoor.Door1;

                position = GetDoorPosition(container, door, doors[(int)door]); // doors);


                //DrawAllPoints();

                doorThatWasUsed = door;

                return false;
            }

            if (door < ExitDoor.Max)//one of our indexed doors
            {
                //  Vector2[] doors = parent.EntityType.ContainerType.HomeContainerType.Doors;

                if (doors != null && doors.Length > (int)door)
                {
                    position = GetDoorPosition(container, door, doors[(int)door]); //doors);

                    // DrawAllPoints();

                    doorThatWasUsed = door;
                    return true;

                }

            }

            //   DrawAllPoints();

            doorThatWasUsed = door;
            return false;
        }

        public static Vector3 GetDoorPosition(Entity container, ExitDoor door, Vector2 doorOffset) // Vector2[] Doors)
        {
            //Vector2 doorOffset = Doors[(int)door];

            if (container.EntityType.RenderableType.RenderAsModelType != null)
            {
                return GetRelativePointRotated(container, doorOffset).ToVector3();
            }
            else
            {
                Vector3 doorPosition = container.PlaySiteLocation; // parent.Location;
          
                if (container.FlipHorizontally) // parent.FlipHorizontally)
                {
                    doorOffset.X *= -1f;
                }

                doorPosition += doorOffset.ToVector3();
                return doorPosition;
            }            
        }
        */


        


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
            snapshotParent = (EntityID)sn.SnapshotID<Entity, EntityID>(parent);

            this.AgentsInside = sn.DoList(AgentsInside);

            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            this.parent = Entity.FindByID(snapshotParent);
        }



        #endregion
    }
}
