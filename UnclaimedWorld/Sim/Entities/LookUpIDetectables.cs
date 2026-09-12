using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Resources;

namespace UWGame.SimSide.Entities
{
     /// <summary>  
     /// a special implementation of ILookUpCollectible which handles DetectableIDs and IDetectable instances.
     /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="Id"></typeparam>
    public class LookUpIDetectables: ILookUpCollectible
    {         
        private static Dictionary<DetectableID, EntityID> snapshotEntityCollection = new Dictionary<DetectableID, EntityID>();
        private static Dictionary<DetectableID, ResourceID> snapshotResourceCollection = new Dictionary<DetectableID, ResourceID>();
             
       // private static Dictionary<CanIterateEntitiesID, EntityID> snapshotEntities = new Dictionary<CanIterateEntitiesID, EntityID>();

        private static Dictionary<DetectableID, IDetectable> collection = new Dictionary<DetectableID, IDetectable>();
      


        private static LookUpIDetectables instance;
        
        public static void Create()
        {
            if (instance == null)
            {
                //instantiate a singleton-like instance of lookup
                instance = new LookUpIDetectables();
                Sim.AddLookupCollectible(typeof(IDetectable), instance);
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
            snapshotResourceCollection.Clear();
            snapshotEntityCollection.Clear();

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

      

        public static IDetectable FindByID(DetectableID id)
        {
            IDetectable detectable;
            collection.TryGetValue(id, out detectable);

            return detectable;
        }

             

        public static void Remove(IDetectable detectable)
        {
            collection.Remove(detectable.ID);
            detectable.SetInvalid();
        }


        public static void Add(DetectableID detectableID, IDetectable iDetectable)
        {
            collection.Add(detectableID, iDetectable);
        }



        #region ISnapshot


        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            // fill the snapshot collections with IDs:
            if (sn.mode != Snapshotter.Mode.Load)
            {
                foreach (var item in collection)
                {
                    if (item.Value is ResourceContainer)
                    {
                        snapshotResourceCollection.Add(item.Key, ((ResourceContainer)item.Value).ID);
                    }
                    else if (item.Value is Entity)
                    {
                        snapshotEntityCollection.Add(item.Key, ((Entity)item.Value).ID);
                    }
                }
            }

            snapshotResourceCollection = (Dictionary<DetectableID, ResourceID>)sn.DoDictionary(snapshotResourceCollection);
            snapshotEntityCollection = (Dictionary<DetectableID, EntityID>)sn.DoDictionary(snapshotEntityCollection);

            sn.Ignore(instance);
            sn.Ignore(collection);

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

            foreach (var item in snapshotResourceCollection)
            {
                collection.Add(item.Key, (IDetectable)LookUp<ResourceContainer, ResourceID>.FindByID(item.Value));
            }

            foreach (var item in snapshotEntityCollection)
            {
                collection.Add(item.Key, Entity.FindByID(item.Value));
            }

            snapshotEntityCollection.Clear();
            snapshotResourceCollection.Clear();       
        }


        #endregion


    }

}
