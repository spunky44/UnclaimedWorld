using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Vegetation;

namespace UWGame.SimSide.Trees
{
    
    public class LookUpIHasCrops: ILookUpCollectible
    {
        // NEW: use SortedDictionary to avoid ending up on the Large Object Heap and causing out of memory crash
        private static SortedDictionary<HasCropsID, LowVegetationID> snapshotLowVegetation = new SortedDictionary<HasCropsID, LowVegetationID>();
        private static SortedDictionary<HasCropsID, EntityID> snapshotTreeCollection = new SortedDictionary<HasCropsID, EntityID>();

        private static SortedDictionary<HasCropsID, IHasCrops> collection = new SortedDictionary<HasCropsID, IHasCrops>();

        /*
        private static Dictionary<HasCropsID, LowVegetationID> snapshotLowVegetation = new Dictionary<HasCropsID, LowVegetationID>();
        private static Dictionary<HasCropsID, EntityID> snapshotTreeCollection = new Dictionary<HasCropsID, EntityID>();
             
        private static Dictionary<HasCropsID, IHasCrops> collection = new Dictionary<HasCropsID, IHasCrops>();
        */


        private static LookUpIHasCrops instance;
        
        public static void Create()
        {
            if (instance == null)
            {
                //instantiate a singleton-like instance of lookup
                instance = new LookUpIHasCrops();
                Sim.AddLookupCollectible(typeof(IHasCrops), instance);
            }
        }


        //static ctor
       /* static LookUpIHasCrops()
        {
            //instantiate a singleton-like instance of lookup
            instance = new LookUpIHasCrops();
            Sim.AddLookupCollectible(typeof(IHasCrops), instance);
        }*/

        public int LoadPostProcessOrder
        {
            get { return 0; } // do this collection first.
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
            snapshotTreeCollection.Clear();
            snapshotLowVegetation.Clear();
            collection.Clear();
        }

       /* public bool PerformSnapshot
        {
            get { return true; }
            
        }*/

              
       

        public static IHasCrops FindByID(HasCropsID id)
        {
            IHasCrops detectable;
            collection.TryGetValue(id, out detectable);

            return detectable;
        }

             

        public static void Remove(IHasCrops hasCrops)
        {
            collection.Remove(hasCrops.ID);
            hasCrops.SetInvalid();
        }


        public static void Add(HasCropsID hasCropsID, IHasCrops hasCrops)
        {
            collection.Add(hasCropsID, hasCrops);
        }


        #region ISnapshot


        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            // fill the snapshot collections with IDs:
            if (sn.mode != Snapshotter.Mode.Load)
            {
                foreach (var item in collection)
                {
                    if (item.Value is Tree)
                    {
                        snapshotTreeCollection.Add(item.Key, ((Tree)item.Value).Parent.ID);
                    }
                    else if (item.Value is LowVegetation)
                    {
                        snapshotLowVegetation.Add(item.Key, ((LowVegetation)item.Value).ID);
                    }
                }
            }

           /* snapshotTreeCollection = sn.DoDictionary(snapshotTreeCollection);
            snapshotLowVegetation = sn.DoDictionary(snapshotLowVegetation);
            */
            snapshotTreeCollection = sn.DoSortedDictionary(snapshotTreeCollection);
            snapshotLowVegetation = sn.DoSortedDictionary(snapshotLowVegetation);
           

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

            foreach (var item in snapshotTreeCollection)
            {
                Tree tree;
                Entity treeEntity = Entity.FindByID(item.Value);
                treeEntity.Find(out tree); // icky to save component reference..?
                collection.Add(item.Key, (IHasCrops)tree); // LookUp<ResourceContainer, ResourceID>.FindByID(item.Value));
            }

            foreach (var item in snapshotLowVegetation)
            {
                collection.Add(item.Key, (IHasCrops)LookUpSortedDictionary<LowVegetation, LowVegetationID>.
                    FindByID(item.Value));
            }

            snapshotLowVegetation.Clear();
            snapshotTreeCollection.Clear();            
        }


        #endregion
    }

}
