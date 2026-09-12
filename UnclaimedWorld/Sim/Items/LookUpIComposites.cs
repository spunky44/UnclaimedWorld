using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Entities.Body;
using UWGame.SimSide.AI;

namespace UWGame.SimSide.Items
{
     /// <summary>  
     /// a special implementation of ILookUpCollectible which handles CompositeIDs and IComposite instances.
     /// 
     /// it maintains 2 collections instead of 1.
     /// when snapshotting, we don't want to snapshot Entity objects again (the Entity collection already does that.) 
     /// So we snapshot their EntityID instead, and link them up post-load.
     /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="Id"></typeparam>
    public class LookUpIComposites: ILookUpCollectible
    {     
    
        //private static Dictionary<CompositeID, EntityID> entityCollection = new Dictionary<CompositeID, EntityID>();
        private static SortedDictionary<CompositeID, EntityID> entityCollection; // = new SortedDictionary<CompositeID, EntityID>(); // prevents LOH allocation!
        private static Dictionary<CompositeID, Tuple<EntityID, MemoryFactID, BodyPartID>> machineBodyPartCollection; // = new Dictionary<CompositeID, Tuple<EntityID, MemoryFactID, BodyPartID>>();


        private static LookUpIComposites instance;
        
        //static ctor
       /* static LookUpIComposites()
        {
            //instantiate a singleton-like instance of lookup
            instance = new LookUpIComposites();
            Sim.AddLookupCollectible(typeof(IComposite), instance);
        }*/

        /// <summary>
        /// NEW: replaces static ctor
        /// </summary>
        public static void Create()
        {
            //instantiate a singleton-like instance of lookup. This ctor will only be called on program startup. instance will survive even when going to the main menu...
            if (instance == null) // permit any existing instance to exist... it may have been used on the main menu screen?
            {
                //instantiate a singleton-like instance of lookup
                instance = new LookUpIComposites();
                Sim.AddLookupCollectible(typeof(IComposite), instance);

                machineBodyPartCollection = new Dictionary<CompositeID, Tuple<EntityID, MemoryFactID, BodyPartID>>();
                entityCollection = new SortedDictionary<CompositeID, EntityID>(); // prevents LOH allocation!
            }
        }

        public bool SnapshotThis
        {
            get
            {
                return true;
            }
        }

        public void ClearCollection()
        {
            //collection.Clear();
            machineBodyPartCollection.Clear();
            entityCollection.Clear();
        }

        public int LoadPostProcessOrder
        {
            get { return 0; } // do this collection first.
        }

    

        public static IComposite FindByID(CompositeID id)
        {
          
            EntityID entityID;         
            Tuple<EntityID, MemoryFactID, BodyPartID> ids;

            if (machineBodyPartCollection.TryGetValue(id, out ids)) // collection.TryGetValue(id, out iComposite))
            {
              
                // BodyPartID is not global... so we need an EntityID or a MemoryFactID, then look up in its Body with the ID...
                Body body = null;
                Entity entity = Entity.FindByID(ids.Item1);
                if (entity != null)
                {
                    body = entity.Body;
                }
                else
                {
                    MemoryFact memoryFact = LookUp<MemoryFact, MemoryFactID>.FindByID(ids.Item2);
                    if (memoryFact != null)
                    {
                        body = memoryFact.Body;
                    }
                }

                if (body != null)
                {
                    return (MachineBodyPart)body.FindBodyPart(ids.Item3);
                }
                else return null;

               // return (MachineBodyPart)LookUp<BodyPart, BodyPartID>.FindByID(bodyPartID); // now, look up its value (this could be optimized with an extra dictionary if there is ever a need for it)     
            }
            else if (entityCollection.TryGetValue(id, out entityID)) // see if the key is an entity
            {
                return Entity.FindByID(entityID); // now, look up its value (this could be optimized with an extra dictionary if there is ever a need for it)              
            }

            return null;
        }

        public static void Remove(ILookUp<IComposite, CompositeID> instance)
        {
            if (!machineBodyPartCollection.Remove(instance.ID)) // collection.Remove(instance.ID))
            {
                entityCollection.Remove(instance.ID);
            }

            instance.SetInvalid();

        }

        /*public static void Remove(Id key)
        {
            collection.Remove(key);
        }*/

        public static void Add(CompositeID id, IComposite instance)
        {
            Entity entity = instance as Entity;
            // place in the correct collection
            if (entity != null)
            {
                entityCollection.Add(id, entity.EntityID);
            }
            else 
            {
                // store all the IDs we need:
                MachineBodyPart machineBodyPart = instance as MachineBodyPart;
                EntityID parent = EntityID.Invalid;
                if (machineBodyPart.Body.Parent != null)
                {
                    parent = machineBodyPart.Body.Parent.ID;
                }
                MemoryFactID parentMemoryFact = MemoryFactID.Invalid;
                if (machineBodyPart.Body.ParentMemoryFact != null)
                {
                    parentMemoryFact = machineBodyPart.Body.ParentMemoryFact.ID;
                }

                machineBodyPartCollection.Add(id, new Tuple<EntityID, MemoryFactID, BodyPartID>(parent, parentMemoryFact, machineBodyPart.BodyPartID));
               // collection.Add(id, instance);
            }
        }


        #region ISnapshot


        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            //objects like Entity that reside under different interface types as keys (Entity + IComposite), we only want to snapshot one instance.
          
            machineBodyPartCollection = /*(Dictionary<CompositeID, Tuple<EntityID, MemoryFactID, BodyPartID>>)*/sn.DoDictionary(machineBodyPartCollection);
            entityCollection = /*(SortedDictionary<CompositeID, EntityID>)*/sn.DoSortedDictionary(entityCollection);

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

            //an iteration of the Collection, calling LoadPostProcess on every instance of T

          /*  foreach (var il in collection)
            {
                ISnapshot snap = il.Value as ISnapshot;
                if (snap != null)
                {
                    snap.LoadPostProcess(sn);

                    snap.IsSnapshotted = false; // reset
                }
            }*/

            // don't postload Entity objects.
        }


        #endregion


    }

}
