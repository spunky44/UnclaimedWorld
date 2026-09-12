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

namespace UWGame.SimSide.Entities
{
    
    public class LookUpHasEntityGroup: ILookUpCollectible
    {

        private static Dictionary<HasEntityGroupID, EntityID> snapshotEntities = new Dictionary<HasEntityGroupID, EntityID>();
        private static Dictionary<HasEntityGroupID, ExpeditionID> snapshotExpeditions = new Dictionary<HasEntityGroupID, ExpeditionID>();
        private static Dictionary<HasEntityGroupID, HouseholdID> snapshotHouseholds = new Dictionary<HasEntityGroupID, HouseholdID>();
        private static Dictionary<HasEntityGroupID, AllegianceID> snapshotAllegiances = new Dictionary<HasEntityGroupID, AllegianceID>();

        private static Dictionary<HasEntityGroupID, IHasEntityGroup> collection; // = new Dictionary<HasEntityGroupID, IHasEntityGroup>();
        
        private static LookUpHasEntityGroup instance;
        

        public static void Create()
        {
            if (instance == null)
            {
                //instantiate a singleton-like instance of lookup
                instance = new LookUpHasEntityGroup();
                Sim.AddLookupCollectible(typeof(IHasEntityGroup), instance);

                collection = new Dictionary<HasEntityGroupID, IHasEntityGroup>();
            }
        }

        //static ctor
      /*  static LookUpHasEntityGroup()
        {
            //instantiate a singleton-like instance of lookup
            instance = new LookUpHasEntityGroup();
            Sim.AddLookupCollectible(typeof(IHasEntityGroup), instance);
        }*/


        public bool SnapshotThis
        {
            get
            {
                return true;
            }
        }
      
        public void ClearCollection()
        {
            snapshotExpeditions.Clear();
            snapshotEntities.Clear();
            snapshotHouseholds.Clear();
            snapshotAllegiances.Clear();

            collection.Clear();
        }

       /* public bool PerformSnapshot
        {
            get { return true; }
            
        }*/

        public int LoadPostProcessOrder
        {
            get { return 0; } // do this collection first.
        }

     

        public static IHasEntityGroup FindByID(HasEntityGroupID? id)
        {
            if (id == null)
                return null;

            IHasEntityGroup owner;
            collection.TryGetValue(id.Value, out owner);

            return owner;
        }

        public static void Add(HasEntityGroupID ownerID, IHasEntityGroup owner)
        {
            collection.Add(ownerID, owner);
        }

        /// <summary>
        /// For add/remove, I decided to make overloads instead of casting.
        /// The second parameter is to guard against copy/paste errors...
        /// </summary>
        /// <param name="hasMembers"></param>
        /// <param name="allegiance"></param>
        public static void Remove(IHasEntityGroup owner)
        {
            collection.Remove(owner.ID);
            owner.SetInvalid();
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
                    else if (item.Value is Person)
                    {
                        snapshotEntities.Add(item.Key, ((Person)item.Value).Parent.ID);
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
            snapshotEntities = sn.DoDictionary(snapshotEntities);
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

            foreach (var item in snapshotExpeditions)
            {
                collection.Add(item.Key, LookUp<Expedition, ExpeditionID>.FindByID(item.Value));
            }

            foreach (var item in snapshotHouseholds)
            {
                collection.Add(item.Key, LookUp<Household, HouseholdID>.FindByID(item.Value));
            }

            foreach (var item in snapshotEntities)
            {
                collection.Add(item.Key, Entity.FindByID(item.Value).PersonEntity);
            }

            foreach (var item in snapshotAllegiances)
            {
                collection.Add(item.Key, LookUp<Allegiance, AllegianceID>.FindByID(item.Value));
            }

            snapshotExpeditions.Clear();
            snapshotEntities.Clear();
            snapshotHouseholds.Clear();
            snapshotAllegiances.Clear();
        }


        #endregion
    }

}
