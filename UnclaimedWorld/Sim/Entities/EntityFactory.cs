using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.SimSide.Entities
{

   /* public class EntityFactoryf 
    {
        //private members
        private static bool producing = false;
        private static bool removing = false;
        private static Dictionary<EntityID, Entity> entityCollection = new Dictionary<EntityID, Entity>();
        private static EntityID idCounter = (EntityID)1;

        //public properties
        public static bool isProducing()
        {
            return producing;
        }

        public static bool isRemoving()
        {
            return removing;
        }

        public static void DoSnapshot( Snapshot sn )
        {
            producing = sn.DoBool(producing);
            removing = sn.DoBool(removing);
            entityCollection = (Dictionary<EntityID,Entity>)sn.DoDictionary(entityCollection);
            idCounter = (EntityID)sn.DoEnum(idCounter); 
        }


        /// <summary>
        /// returns null if the entity has been destroyed.
        /// So, often the entity should be looked up in SharedKnowledge first. 
        /// That way, the agents won't become aware of an entity's destruction when they shouldn't be.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Entity FindByID(EntityID id)
        {
            if (id == EntityID.Invalid)
                return null;

            Entity val;
            if (entityCollection.TryGetValue(id, out val))
                return val;

            return null;
        }

        public static void Remove(Entity e)
        {
            removing = true;
            entityCollection.Remove(e.ID);
            removing = false;

            producing = true; // not really, just permitting the setting of ID here
            e.ID = EntityID.Invalid;  // .SetID(Entity.ID.Invalid);
            producing = false;
        }

        public static Entity Produce(EntityType type)
        {
            //producing = true;
            //Entity e = new Entity(type);// TODO Refactor screenmodels should be renderables only
            //// Lars: Parts are constructed recursively and will set produce to false. Compensate with this hack:
            //if (type.Parts != null)
            //{
            //    producing = true;
            //}

            //AssignUniqueID(e);

            //if (e.ID != EntityID.Invalid)
            //    entityCollection.Add(e.ID, e); //<<== add this

            ////  e.SetName(((uint)e.ID).ToString());// cast so we don't get the enum constant name as a string

            //producing = false;
            //return e;
        throw new NotImplementedException();
        }

        public static void ResetIDCounter()
        {
            idCounter = (EntityID)1;
        }


        public static void Clear()
        {
            entityCollection.Clear();
            ResetIDCounter();
        }

        public static Dictionary<EntityID, Entity> AllEntities
        {
            get { return entityCollection; }
        }

        private static void AssignUniqueID(Entity e)
        {
            e.ID = idCounter;

            idCounter++;

            if (idCounter >= EntityID.Max)
            {
                throw new Exception("Astounding, EntityID just exceeded 64 bits. Something seriously wrong has happened.");
            }
        }

        public static EntityID LastUsedID
        {
            get
            {
                return idCounter;
            }
        }

    }*/

}
