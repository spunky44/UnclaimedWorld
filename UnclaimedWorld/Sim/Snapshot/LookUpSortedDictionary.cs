using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UWGame.SimSide.Systems.TimeSlicing;

namespace UWGame.SimSide.Snapshots
{
    /// <summary>
    /// Copy of LookUp that uses a SortedDictionary instead of a Dictionary to hold key/value pairs.
    /// 
    /// SortedDictionary does not end up on the Large Obejct Heap, so is less likely to cause out of memory crashes.
    /// Use this imp for large collections which don't need to look up the ID very often (preferably only during save/load)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="Id"></typeparam>
    public class LookUpSortedDictionary<T, Id> : ILookUpCollectible 
        where T : ILookUp<T, Id> 
        where Id: struct // cannot specify enum - this allows nullable operations
    {
       // private static Dictionary<Id, T> collection = new Dictionary<Id, T>();
        private static SortedDictionary<Id, T> collection = new SortedDictionary<Id, T>();

        private static LookUpSortedDictionary<T, Id> instance;


        private static bool performSnapshot = true;

      /*  public bool PerformSnapshot 
        {
            get { return performSnapshot; }
        }*/

        /// <summary>
        /// these statics are called from AddToLookup, not sure if there is a better place..?
        /// set this to false to make sure we don't snapshot gamedatatypes!
        /// </summary>
        /// <param name="value"></param>
        public static void SetPerformSnapshot(bool value)
        {
            performSnapshot = value;
        }

        /// <summary>
        /// we snapshot this value so it will also be there when loading fresh
        /// </summary>
        /// <param name="orderToUse"></param>
        public static void SetLoadPostProcessOrder(int orderToUse)
        {
            order = orderToUse;
        }


        public bool SnapshotThis
        {
            get
            {
                return performSnapshot;
            }
        }

        static int order = 100; // do these collections last (but before Goals?)
        public int LoadPostProcessOrder 
        {
            get { return order; }            
        }

        public static void Create()
        {
            //instantiate a singleton-like instance of lookup. This ctor will only be called on program startup. instance will survive even when going to the main menu...
            if (instance == null) // permit any existing instance to exist... interface types may call this for each implementor class? it may have been used on the main menu screen???
            {
                instance = new LookUpSortedDictionary<T, Id>();
                Sim.AddLookupCollectible(typeof(T), instance);
            }
        }

        /*
        //static ctor
        static LookUpSortedDictionary()
        {
            //instantiate a singleton-like instance of lookup. This ctor will only be called on program startup. instance will survive even when going to the main menu...
            instance = new LookUpSortedDictionary<T, Id>();
            Sim.AddLookupCollectible(typeof(T), instance);
        }*/

        #region ILookupCollectible

        public void ClearCollection()
        {
            collection.Clear();

            Type typeOfT = typeof(T);
            if (!typeOfT.IsInterface) // can't invoke on ICyclable and other interface types.
            {
                var methodInfo = typeOfT.GetMethod("ResetIDCounter");
              
                methodInfo.Invoke(null, null); //null - means calling static method
               
            }
        }


        #endregion

        public static T FindByID(Id id)  
        {
            T val;
            if (collection.TryGetValue(id, out val))
                return val;

            return default(T);
        }

        /// <summary>
        /// for easy post load linkup, this overload handles a null id by returning null.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static T FindByID(Id? id)
        {
            if (id.HasValue)
            {
                T val;
                if (collection.TryGetValue(id.Value, out val))
                    return val;
            }

            return default(T);
        }

        public static void Remove(ILookUp<T, Id> instance)
        {
            collection.Remove(instance.ID);

            instance.SetInvalid();

        }


        public static void Add(Id id, T instance)
        {
           /* if (!Snapshotter.IsSnapshotting) // precaution to prevent constructor calls from Load... 
            {*/
                collection.Add(id, instance);
           // }
        }


        public static void IterateMembers(Action<T> iterateMethod)
        {
            foreach (var item in collection)
            {
                iterateMethod(item.Value);
            }
        }


       
        #region ISnapshot


        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            if (performSnapshot)
            {
              
              //  collection = (Dictionary<Id, T>)sn.DoDictionary(collection);
                collection = (SortedDictionary<Id, T>)sn.DoSortedDictionary(collection);
                order = sn.DoInt32(order); // NEW
            }
            else
            {
                sn.Ignore(collection);
            }

            sn.Ignore(instance);
            sn.Ignore(performSnapshot);
            sn.Ignore(order);

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
            if (performSnapshot)
            {
              /*  Type type = collection.GetType().GetGenericArguments()[1];
                if (type.Name == "ICyclable")
                {

                }*/

                // CyclableID / ICyclable is such a diverse group of classes, some of them are order dependent.
                // for instance RegionSearchPlanner depends on DependenRegionMap having been processed first.
                // so we need to order the collection...              
                var sortedList = collection.OrderBy(l => l.Value.LoadPostProcessOrder);

                foreach (var il in sortedList) //collection)
                {
                    ISnapshot snap = il.Value as ISnapshot;
                    if (snap != null)
                    {
                        snap.LoadPostProcess(sn);

                        snap.IsSnapshotted = false; // reset
                    }
                }
            }
        }


        #endregion

       
    }
}
