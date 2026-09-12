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

namespace UWGame.SimSide.Entities.Owners
{
    /// <summary>
    /// the purpose of this class is to maintain a mapping from owner IDs and owner collections.
    /// 
    /// Owner IDs will be held by:
    /// 
    /// 1. Owners, like Expedition, Person, Household
    /// 2. Owned entities like items
    /// 3. AI Goals for assigning ownership to new entities
    /// 4. AI Evaluators for the same purpose
    /// 
    /// 
    /// when two owner collections are merged (because a person dies or an expedition is removed), we only need to update a few entries in the mapping.
    /// We do not have to go over all owned items and change their ownedBy IDs.
    /// 
    /// However, this happens infrequently. So perhaps we should update each item after all...?
    /// 
    /// OwnerContent has a parent (IOwner) as well. What should that point to?
    /// 
    /// can owner collections be split... what then... what is the use case for that situation?
    /// It would probably not make sense to keep the IDs. 
    /// 3. and 4. would have to reset
    /// since their owner IDs are invalid. 
    /// Needs to think about this more.
    /// </summary>
    public class LookUpOwners: ILookUpCollectible
    {

        private static Dictionary<OwnerID, EntityID> snapshotEntities = new Dictionary<OwnerID, EntityID>();
        private static Dictionary<OwnerID, ExpeditionID> snapshotExpeditions = new Dictionary<OwnerID, ExpeditionID>();
        private static Dictionary<OwnerID, HouseholdID> snapshotHouseholds = new Dictionary<OwnerID, HouseholdID>();

        private static Dictionary<OwnerID, IOwner> collection;
      
        private static LookUpOwners instance;


        public static void Create()
        {
            if (instance == null)
            {
                //instantiate a singleton-like instance of lookup
                instance = new LookUpOwners();
                Sim.AddLookupCollectible(typeof(IOwner), instance);

                collection = new Dictionary<OwnerID, IOwner>();
            }
        }

        /*
        //static ctor
        static LookUpOwners()
        {
            //instantiate a singleton-like instance of lookup
            instance = new LookUpOwners();
            Sim.AddLookupCollectible(typeof(IOwner), instance);
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

            collection.Clear();
        }

        public int LoadPostProcessOrder
        {
            get { return 0; } // do this collection first.
        }

        public bool PerformSnapshot
        {
            get { return true; }
            
        }

      
        public static IOwner FindByID(OwnerID? id)
        {
            if (id == null)
                return null;

            IOwner owner;
            collection.TryGetValue(id.Value, out owner);

            return owner;            
        }

        public static void Remove(IOwner owner)
        {
            collection.Remove(owner.ID);
         
            owner.SetInvalid();
        }


        public static void Add(OwnerID ownerID, IOwner owner)
        {
            collection.Add(ownerID, owner);
        }

       
              

        /// <summary>
        /// returns false and clears the OwnedBy property if the owner could not be found.
        /// </summary>
        /// <param name="entityData"></param>
        /// <param name="owner"></param>
        /// <returns></returns>
        public static bool ResolveEntityOwner(IKnownEntityData entityData, out IOwner owner)
        {
            owner = null;
            if (entityData.OwnedBy.HasValue)
            {
                owner = FindByID(entityData.OwnedBy);
                if (owner == null)
                {
                    entityData.OwnedBy = null;
                    return false;

                }
            }

            return true;
        }


        /// <summary>
        /// returns false and clears the OwnedBy property if the owner could not be found.
        /// 
        /// Returns True and a null output if not owned!
        /// </summary>
        /// <param name="entityData"></param>
        /// <param name="owner"></param>
        /// <returns></returns>
        public static bool ResolveEntityOwner(IKnownEntityData entityData, out EntityGroup ownedEntities)
        {
            IOwner iowner;
            ownedEntities = null;
            if (entityData.OwnedBy.HasValue)
            {
                iowner = FindByID(entityData.OwnedBy);
                if (iowner == null)
                {
                    entityData.OwnedBy = null;
                    return false;
                }

                ownedEntities = iowner.OwnedEntities;
            }

            return true;
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
                    else
                    {
                        System.Diagnostics.Debug.Assert(false, "type not implemented!");
                    }
                }
            }

            snapshotExpeditions = sn.DoDictionary(snapshotExpeditions);
            snapshotEntities = sn.DoDictionary(snapshotEntities);
            snapshotHouseholds = sn.DoDictionary(snapshotHouseholds);

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

            snapshotExpeditions.Clear();
            snapshotEntities.Clear();
            snapshotHouseholds.Clear();
        }


        #endregion
       
    }

}
