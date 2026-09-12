using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.AI;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.Entities
{

  /*
    public class LookUpEntityGroups: ILookUpCollectible
    {
      
        private static Dictionary<EntityGroupID, EntityGroup> collection  = new Dictionary<EntityGroupID, EntityGroup>();

        private static LookUpEntityGroups instance;

       
        public bool PerformSnapshot 
        {
            get { return true; }
          
        }

        public bool ClearCollectionBeforePostLoad
        {
            get { return false; }
        }

        //static ctor
        static LookUpEntityGroups()
        {
            //instantiate a singleton-like instance of lookup
            instance = new LookUpEntityGroups();
            Sim.AddLookupCollectible(typeof(LookUpEntityGroups), 
                instance);
        }

        // LookupCollectibe=======================||       
        public void ClearCollection()
        {
            collection.Clear();

            EntityGroup.ResetIDCounter();

        }

       
        public void DoSnapshot(Snapshotter sn)
        {            
            collection = sn.DoDictionary(collection);            
        }

        public void DoPostLoad(Snapshotter sn)
        {
            //an iteration of the Collection, calling LoadPostProcess on every instance of T

            foreach (var il in collection)
            {
                EntityGroup owner = il.Value;

                owner.LoadPostProcess(sn);

                owner.IsSnapshotted = false; // reset

            }

        }

        public static EntityGroup FindByID(EntityGroupID id)
        {
            EntityGroup val;
            if (collection.TryGetValue(id, out val))
                return val;

            return null;
        }

        public static EntityGroup FindByID(EntityGroupID? id)
        {
            if (id.HasValue)
            {
                EntityGroup val;
                if (collection.TryGetValue(id.Value, out val))
                    return val;

            }

            return null;
        }
            


        public static void Remove(EntityGroup instance)
        {
            collection.Remove(instance.ID);

            instance.SetInvalid();
        }


        public static void Add(EntityGroupID id, EntityGroup instance)
        {
            collection.Add(id, instance);
        }

        

        public static void IterateGroups(Action<EntityGroup> iterateMethod)
        {
            foreach (var item in collection)
            {
                iterateMethod(item.Value);
            }


        }

    }*/
}
