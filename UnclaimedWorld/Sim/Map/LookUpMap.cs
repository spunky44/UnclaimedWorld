using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.AI;
using UWGame.SimSide.Items;
using UWGame.SimSide.Allegiances;

namespace UWGame.SimSide.Maps
{
    
  /*  public class LookUpMap: ILookUpCollectible
    {

        private static Dictionary<IMapID, EntityID> snapshotEntities = new Dictionary<IMapID, EntityID>();
        private static Dictionary<IMapID, ExpeditionID> snapshotExpeditions = new Dictionary<IMapID, ExpeditionID>();

        private static Dictionary<IMapID, IMap> collection = new Dictionary<IMapID, IMap>();

        private static LookUpMap instance;
        
        //static ctor
        static LookUpMap()
        {
            //instantiate a singleton-like instance of lookup
            instance = new LookUpMap();
            Sim.AddLookupCollectible(typeof(IHasEntityGroup), instance);
        }

      
        public void ClearCollection()
        {
            snapshotExpeditions.Clear();
            snapshotEntities.Clear();
           
            collection.Clear();
        }

        public bool PerformSnapshot
        {
            get { return true; }
            
        }

      

        public void DoSnapshot(Snapshotter sn)
        {
            // fill the snapshot collections with IDs:
            if (sn.mode != Snapshotter.Mode.Load)
            {
                foreach (var item in collection)
                {
                    if (item.Value is PersonEntity)
                    {
                        snapshotEntities.Add(item.Key, ((PersonEntity)item.Value).Parent.ID);
                    }
                    else if (item.Value is Expedition)
                    {
                        snapshotExpeditions.Add(item.Key, ((Expedition)item.Value).ID);
                    }
                   
                }
            }

            snapshotExpeditions = sn.DoDictionary(snapshotExpeditions);
            snapshotEntities = sn.DoDictionary(snapshotEntities);
           
            sn.Ignore(collection);
        }

        public void DoPostLoad(Snapshotter sn)
        {
            collection.Clear();

            
            foreach (var item in snapshotHouseholds)
            {
                collection.Add(item.Key, LookUp<Household, HouseholdID>.FindByID(item.Value));
            }

            foreach (var item in snapshotEntities)
            {
                collection.Add(item.Key, Entity.FindByID(item.Value).PersonEntity);
            }

           

            snapshotExpeditions.Clear();
            snapshotEntities.Clear();
          
        }

        public static IMap FindByID(IMapID? id)
        {
            if (id == null)
                return null;

            IMap item;
            collection.TryGetValue(id.Value, out item);

            return item;
        }

        public static void Add(IMapID id, IMap item)
        {
            collection.Add(id, item);
        }


        public static void Remove(IMap owner)
        {
            collection.Remove(owner.ID);
            owner.SetInvalid();
        }

       
    }*/

}
