using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Resources;

namespace UWGame.SimSide.Resources
{


   /* public class ResourceContainerFactory
    {
        //public members
        public static bool isProducing()
        {
            return producing;
        }

        public static bool isRemoving()
        {
            return removing;
        }

        public ResourceContainer FindByID(ulong id)
        {
            return FindByID((Sim.ResourceID)id);
        }


        /// <summary>
        /// returns null if the entity has been destroyed.
        /// So, often the entity should be looked up in SharedKnowledge first. 
        /// That way, the agents won't become aware of an entity's destruction when they shouldn't be.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static ResourceContainer FindByID(ResourceID id)
        {
            if (id == Sim.ResourceID.Invalid)
                return null;

            ResourceContainer val;
            if (resourceContainerCollection.TryGetValue(id, out val))
                return val;

            return null;
        }

        public static void Remove(ResourceContainer container)
        {
            removing = true;
            resourceContainerCollection.Remove(container.ID);
            removing = false;

            producing = true; // not really, just permitting the setting of ID here
            container.ID = Sim.ResourceID.Invalid;  // .SetID(Entity.ID.Invalid);
            producing = false;
        }
        public static Crop Produce(Trees.Tree tree, ResourceType resourceType)
        {
            producing = true;
            Crop container = new Crop(tree, resourceType);
          
            AssignUniqueID(container);

            if (container.ID != Sim.ResourceID.Invalid)
                resourceContainerCollection.Add(container.ID, container); //<<== add this

            //  e.SetName(((uint)e.ID).ToString());// cast so we don't get the enum constant name as a string

            producing = false;
            return container;
        }
        public static TileResourceContainer Produce(Maps.TerrainTile terrainTile, ResourceType resourceType)
        {
            producing = true;
            TileResourceContainer container = new TileResourceContainer(terrainTile, resourceType);
            
          
            AssignUniqueID(container);

            if (container.ID != Sim.ResourceID.Invalid)
                resourceContainerCollection.Add(container.ID, container); //<<== add this

            //  e.SetName(((uint)e.ID).ToString());// cast so we don't get the enum constant name as a string

            producing = false;
            return container;
        }

        public static void ResetIDCounter()
        {
            idCounter = (Sim.ResourceID)1;
        }


        public static void Clear()
        {
            resourceContainerCollection.Clear();
            ResetIDCounter();
        }

        //private members
        private static bool producing = false;
        private static bool removing = false;
        private static Dictionary<Sim.ResourceID, ResourceContainer> resourceContainerCollection = new Dictionary<Sim.ResourceID, ResourceContainer>();
        private static Sim.ResourceID idCounter = (Sim.ResourceID)1;
        private static void AssignUniqueID(ResourceContainer container)
        {
            container.ID = idCounter;

            idCounter++;

            if (idCounter >= Sim.ResourceID.Max)
            {
                throw new Exception("Astounding, EntityID just exceeded 64 bits. Something seriously wrong has happened.");
            }
        }

        public static Sim.ResourceID LastUsedID
        {
            get
            {
                return idCounter;
            }
        }

    }*/

}
