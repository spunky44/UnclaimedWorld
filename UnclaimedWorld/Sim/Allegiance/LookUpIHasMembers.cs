using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Expeditions;

namespace UWGame.SimSide.Allegiances
{
    /// <summary>  
    /// a special implementation of ILookUpCollectible.   
    /// 
    /// only store agents, expeditions, allegiances, households
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="Id"></typeparam>
    public class LookUpICanIterateEntities: ILookUpCollectible
    {

        // add collections and Add methods when there are more implementers..        
        private static Dictionary<CanIterateEntitiesID, AllegianceID> snapshotAllegiances = new Dictionary<CanIterateEntitiesID, AllegianceID>();
        private static Dictionary<CanIterateEntitiesID, HouseholdID> snapshotHouseholds = new Dictionary<CanIterateEntitiesID, HouseholdID>();
        private static Dictionary<CanIterateEntitiesID, ExpeditionID> snapshotExpeditions = new Dictionary<CanIterateEntitiesID, ExpeditionID>();
        private static Dictionary<CanIterateEntitiesID, EntityID> snapshotAgents = new Dictionary<CanIterateEntitiesID, EntityID>();

        private static Dictionary<CanIterateEntitiesID, ICanIterateEntities> collection; // = new Dictionary<CanIterateEntitiesID, ICanIterateEntities>();
      

        private static LookUpICanIterateEntities instance;
        

        public static void Create()
        {
            if (instance == null)
            {
                //instantiate a singleton-like instance of lookup
                instance = new LookUpICanIterateEntities();
                Sim.AddLookupCollectible(typeof(ICanIterateEntities), instance);

                collection = new Dictionary<CanIterateEntitiesID, ICanIterateEntities>();
            }
        }

        /*
        //static ctor
        static LookUpICanIterateEntities()
        {
            //instantiate a singleton-like instance of lookup
            instance = new LookUpICanIterateEntities();
            Sim.AddLookupCollectible(typeof(ICanIterateEntities), instance);
        }*/

      
        public void ClearCollection()
        {
            snapshotHouseholds.Clear();
            snapshotAllegiances.Clear();
            snapshotExpeditions.Clear();
            snapshotAgents.Clear();

            collection.Clear();
        }

       
        public bool SnapshotThis
        {
            get
            {
                return true;
            }
        }

        public int LoadPostProcessOrder
        {
            get { return 0; } // do this collection first.
        }

    

        public static ICanIterateEntities FindByID(CanIterateEntitiesID id)
        {
            ICanIterateEntities canIterate;
            collection.TryGetValue(id, out canIterate);

            return canIterate;
        }


        public static void Remove(ICanIterateEntities hasMembers)
        {
            collection.Remove(hasMembers.ID);
          
            hasMembers.SetInvalid();
        }


        public static void Add(CanIterateEntitiesID hasMembersID, ICanIterateEntities canIterate)
        {
            collection.Add(hasMembersID, canIterate);
        }

        #region ISnapshot


        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            // fill the snapshot collections with IDs:
            if (sn.mode != Snapshotter.Mode.Load)
            {
                foreach (var item in collection)
                {
                    if (item.Value is Household)
                    {
                        snapshotHouseholds.Add(item.Key, ((Household)item.Value).ID);
                    }
                    else if (item.Value is Entity)
                    {
                        snapshotAgents.Add(item.Key, ((Entity)item.Value).ID);
                    }
                    else if (item.Value is Expedition)
                    {
                        snapshotExpeditions.Add(item.Key, ((Expedition)item.Value).ID);
                    }
                    else if (item.Value is Allegiance)
                    {
                        snapshotAllegiances.Add(item.Key, ((Allegiance)item.Value).ID);
                    }
                }
            }

            snapshotExpeditions = sn.DoDictionary(snapshotExpeditions);
            snapshotAgents = sn.DoDictionary(snapshotAgents);
            snapshotHouseholds = sn.DoDictionary(snapshotHouseholds);
            snapshotAllegiances = sn.DoDictionary(snapshotAllegiances);

            sn.Ignore(collection);
            sn.Ignore(instance);

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

            collection.Clear();

            foreach (var item in snapshotAllegiances)
            {
                collection.Add(item.Key, LookUp<Allegiance, AllegianceID>.FindByID(item.Value));
            }

            foreach (var item in snapshotExpeditions)
            {
                collection.Add(item.Key, LookUp<Expedition, ExpeditionID>.FindByID(item.Value));
            }

            foreach (var item in snapshotHouseholds)
            {
                collection.Add(item.Key, LookUp<Household, HouseholdID>.FindByID(item.Value));
            }

            foreach (var item in snapshotAgents)
            {
                collection.Add(item.Key, Entity.FindByID(item.Value));
            }

            snapshotAllegiances.Clear();
            snapshotExpeditions.Clear();
            snapshotAgents.Clear();
            snapshotHouseholds.Clear();
        }


        #endregion

    }

}
