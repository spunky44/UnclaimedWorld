using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace UWGame.SimSide.Snapshots
{
    public enum MethodID : ulong
    {
        First = 0L, // UNASSIGNED - NOT IN USE
        Invalid = uint.MaxValue,
        Max = Invalid
    }

 

    /// <summary>
    /// manages methodIDs and delegates. This class will not be snapshotted - there is no way to snapshot a method pointer.
    /// Instead, it requires clients to re-register their methods (using the same ID!) after load.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ActionLookup<T> : ILookUpCollectible
    {

        private static Dictionary<MethodID, Action<T>> collection; // = new Dictionary<MethodID, Action<T>>();

        private static ActionLookup<T> instance;

        public static void Create()
        {
            if (instance == null)
            {
                //instantiate a singleton-like instance of lookup
                instance = new ActionLookup<T>();
                Sim.AddLookupCollectible(typeof(ActionLookup<T>), instance);

                collection = new Dictionary<MethodID, Action<T>>(); // ensures Create must be called
            }
        }

        /*
        //static ctor
        static ActionLookup()
        {
            //instantiate a singleton-like instance of lookup
            instance = new ActionLookup<T>();
            Sim.AddLookupCollectible(typeof(ActionLookup<T>), instance);
        }*/




        public void ClearCollection()
        {
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

        /// <summary>
        /// if method is null, returns null.
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public static MethodID? GetID(Action<T> method)
        {
            if (method == null)
                return null;

            return collection.First(k => k.Value == method).Key;
        }

        public static Action<T> FindByID(MethodID id)
        {
            Action<T> handler;
            collection.TryGetValue(id, out handler);

            return handler;   


           // return collection[id];
        }

        public static void Remove(MethodID id)
        {
            collection.Remove(id);

        }


        public static void Add(MethodID id, Action<T> instance)
        {
            if (id == MethodID.First
                || id == MethodID.Invalid)
                throw new Exception("Invalid MethodID");

            if (typeof(T) == typeof(UWGame.SimSide.Processes.IKnownProcess))
            {
                // see if the class exists in coll-of-colls!
            }

            collection.Add(id, instance);
        }

        /// <summary>
        /// creates and returns a new ID that the method is registered under.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static MethodID AddWithNewID(Action<T> instance)
        {
            if (typeof(T) == typeof(UWGame.SimSide.Processes.IKnownProcess))
            {
                // see if the class exists in coll-of-colls!
            }

            MethodID id = MethodCounter.GetUniqueID(); 
            collection.Add(id, instance);

            return id;
        }


        
        #region ISnapshot


        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            // don't snapshot this collection. in fact, we want to clear it:
            if (sn.mode == Snapshotter.Mode.Save
                || sn.mode == Snapshotter.Mode.Load)
            {
                collection.Clear();
            }


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

        }


        #endregion

    }



    /// <summary>
    /// another lookup class corresponding to events without invocation arguments
    /// </summary>  
    public class ActionLookup : ILookUpCollectible
    {
        private static Dictionary<MethodID, Action> collection = new Dictionary<MethodID, Action>();

        private static ActionLookup instance;

        public static void Create()
        {
            if (instance == null)
            {
                //instantiate a singleton-like instance of lookup
                instance = new ActionLookup();
                Sim.AddLookupCollectible(typeof(ActionLookup), instance);
            }
        }


        //static ctor
       /* static ActionLookup()
        {
            //instantiate a singleton-like instance of lookup
            instance = new ActionLookup();
            Sim.AddLookupCollectible(typeof(ActionLookup), instance);
        }*/


        public void ClearCollection()
        {
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

        /// <summary>
        /// if method is null, returns null.
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public static MethodID? GetID(Action method)
        {
            if (method == null)
                return null;

            return collection.First(k => k.Value == method).Key;
        }

        public static Action FindByID(MethodID id)
        {
            Action handler;
            collection.TryGetValue(id, out handler);

            return handler;            
        }

        public static void Remove(MethodID id)
        {
            collection.Remove(id);

        }

        
        public static void Add(MethodID id, Action instance)
        {
            if (id == MethodID.First
               || id == MethodID.Invalid)
                throw new Exception("Invalid MethodID");

            collection.Add(id, instance);
        }

        /// <summary>
        /// creates and returns a new ID that the method is registered under.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static MethodID AddWithNewID(Action instance)
        {            
            MethodID id = MethodCounter.GetUniqueID();
            collection.Add(id, instance);

            return id;
        }
        

        #region ISnapshot


        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            // don't snapshot this collection. in fact, we want to clear it:
            if (sn.mode == Snapshotter.Mode.Save
                || sn.mode == Snapshotter.Mode.Load)
            {
                collection.Clear();
            }

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

        }


        #endregion

    }





    /*  public class DelegateLookup // : LookupCollectible where T : ILookUp<T, Id>
   {
       /// <summary>
       /// Delegates can only be called using DynamicInvoke, which is 200 times slower than normal invoke.
       /// http://stackoverflow.com/questions/12858340/difference-between-invoke-and-dynamicinvoke
       /// </summary>
       private static Dictionary<MethodID, Delegate> collection = new Dictionary<MethodID, Delegate>();

       private static MethodID idCounter = MethodID.First;

       public static DelegateLookup Instance;

       //static ctor
       static DelegateLookup()
       {
           //instantiate a singleton-like instance of lookup
           Instance = new DelegateLookup();

          // Sim.Add(typeof(T), Instance);
       }

      
       public void ClearCollection()
       {
           collection.Clear();

           idCounter = MethodID.First;
       }

          

       /// <summary>
       /// if method is null, returns null.
       /// </summary>
       /// <param name="method"></param>
       /// <returns></returns>
       public static MethodID? GetID(Delegate method)
       {
           if (method == null)
               return null;

           return collection.First(k => k.Value == method).Key;
       }

       public static Delegate FindByID(MethodID id)
       {
           return collection[id];
       }

       public static void Remove(MethodID id)
       {
           collection.Remove(id);
            
       }


       public static MethodID GetUniqueID()
       {
           idCounter++;
           if (idCounter >= MethodID.Max)
           {
               throw new Exception("Astounding, MethodID just exceeded 64 bits. Something seriously wrong has happened.");
           }

           return idCounter;
       }

       public static void Add(MethodID id, Delegate instance)
       {
          
           collection.Add(id, instance);
       }

       /// <summary>
       /// creates and returns a new ID that the method is registered under.
       /// </summary>
       /// <param name="instance"></param>
       /// <returns></returns>
       public static MethodID AddWithNewID(Delegate instance)
       {
           MethodID id = GetUniqueID();
           collection.Add(id, instance);

           return id;
       }

   }*/

}
