using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Systems
{
    /// <summary>
    /// special implementation of ILookUpCollectible because LookUp cannot be used with generic types.
    /// 
    /// There will be a collection for each type T each with their own ID counter.  
    /// 
    /// Since SleepyUpdater is used in both Sim and Client, the SleepyUpdater members will not be automatically snapshotted.
    /// </summary>
    public class LookUpSleepyUpdater<T> : ILookUpCollectible where T : ISleepingUpdatable
    {
        /// <summary>
        /// most of the time, this will only have 1 entry.
        /// </summary>
        private static Dictionary<SleepyUpdaterID, SleepyUpdater<T>> collection = new Dictionary<SleepyUpdaterID, SleepyUpdater<T>>();

        private static LookUpSleepyUpdater<T> instance;

        public static void Create()
        {          
            if (instance == null) // permit any existing instance to exist... interface types may call this for each implementor class? it may have been used on the main menu screen???
            {
                instance = new LookUpSleepyUpdater<T>();
                Sim.AddLookupCollectible(typeof(LookUpSleepyUpdater<T>), instance);
            }
        }

        //static ctor
      /*  static LookUpSleepyUpdater()
        {
            //instantiate a singleton-like instance of lookup
            instance = new LookUpSleepyUpdater<T>();
            Sim.AddLookupCollectible(typeof(LookUpSleepyUpdater<T>), instance);
        }*/

      
        public void ClearCollection()
        {
            // this method should look the same as LookUp.ClearCollection()

            collection.Clear();

            SleepyUpdater<T>.ResetIDCounter(); // NEW
        }

      
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

        public static SleepyUpdater<T> FindByID(SleepyUpdaterID id)
        {
            SleepyUpdater<T> iComposite;

            if (collection.TryGetValue(id, out iComposite))
            {
                return iComposite;
            }
           

            return null;
        }

     
        public static void Remove(SleepyUpdater<T> instance)
        {
            collection.Remove(instance.ID);           

            instance.SetInvalid();

        }


        public static void Add(SleepyUpdaterID id, SleepyUpdater<T> instance)
        {           
            collection.Add(id, instance);
           
        }

        #region ISnapshot


        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            // we don't snapshot Sleepy Updater class. Instead we recreate it post-load.
          //  collection = sn.DoDictionary(collection);            

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
}
