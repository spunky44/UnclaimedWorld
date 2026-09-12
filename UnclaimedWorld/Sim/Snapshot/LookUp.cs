using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UWGame.SimSide.Systems.TimeSlicing;
using UWGame.SimSide.AI.Goals;

namespace UWGame.SimSide.Snapshots
{
    /// <summary>
    /// Don't change the casing! Lookup is a Linq method.
    /// 
    /// manages the collection of ILookup objects. Also handles snapshotting.
    /// 
    /// The counter for the IDs must reside in the ILookup class. So while this class will snapshot the collection itself, The ILookup class is responsible for snapshotting the counter!
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="Id"></typeparam>
    public class LookUp<T, Id> : ILookUpCollectible 
        where T : ILookUp<T, Id> 
        where Id: struct // cannot specify enum - this allows nullable operations
    {
        private static Dictionary<Id, T> collection; // = new Dictionary<Id, T>();
       
        private static LookUp<T, Id> instance;


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
       

        /// <summary>
        /// NEW: replaces static ctor
        /// </summary>
        public static void Create()
        {
            //instantiate a singleton-like instance of lookup. This ctor will only be called on program startup. instance will survive even when going to the main menu...
            if (instance == null) // permit any existing instance to exist... interface types may call this for each implementor class? it may have been used on the main menu screen???
            {
                bool typeIsExcluded = false;
                if (typeof(T) == typeof(Goal))
                {
                    //throw exception or handle appropriately.
                    typeIsExcluded = true;
                }


                if (typeIsExcluded)
                {
                    throw new Exception("Type is excluded from LookUp.");
                }

                instance = new LookUp<T, Id>();
                Sim.AddLookupCollectible(typeof(T), instance);

                collection = new Dictionary<Id, T>();
            }
        }

        //static ctor
       /* static LookUp()
        {
            //instantiate a singleton-like instance of lookup. This ctor will only be called on program startup. instance will survive even when going to the main menu...
            instance = new LookUp<T, Id>();
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
                collection = (Dictionary<Id, T>)sn.DoDictionary(collection);
               
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
