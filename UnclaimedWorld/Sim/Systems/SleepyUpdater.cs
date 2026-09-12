using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.AI.Pathfinding;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.Systems
{
    public enum Module { Sim, Client }

    /// <summary>
    /// ISleepingUpdatable will hold this ID in order to noitfy the collection when their update interval changes
    /// 
    /// 
    /// </summary>
    public enum SleepyUpdaterID : ulong
    {
        Invalid = ulong.MaxValue,
        Max = Invalid,
        First = 1
    }

    /// <summary>
    /// an optimization that will update a list of objects on certain intervals, without polling each item every frame. It will only examine the minimum no of members every frame.
    /// 
    /// don't use this class if the items will mostly have updates every frame - the overhead will probably mean less performance.
    /// 
    /// Snapshot note: the class and the list members are not snapshotted automatically even though a SleepyUpdater has an ID reference and is maintained by a LookUp class.
    /// </summary>
    public class SleepyUpdater<T> where T : ISleepingUpdatable 
    {
        /// <summary>
        /// non-sleeping items are kept and sorted in this list.
        /// 
        /// where are the non-sleeping items kept..? with the client???
        /// 
        /// a sleeping ISleepingUpdatable must notify this class if it wants to receive updates again.
        /// </summary>
        private List<T> listOfNonSleepingItems = new List<T>();


        /// <summary>
        /// for performance... for checking to see if an item already exists in the list.
        /// It will rarely happen that an item goes to sleep or wakes up. Each time we check using this map if it is already in the updatable list
        /// </summary>
        private Dictionary<T, T> lookupMap = new Dictionary<T, T>();


        private bool listIsDirty = false;

        private bool staggerUpdates = false;

        /// <summary>
        /// used to select the random generator to use for staggered intervals
        /// </summary>
        Module belongsToModule;

        HighResolutionTime timer;


        public int Count
        {
            get
            {
                return listOfNonSleepingItems.Count;
            }
        }

        public SleepyUpdater()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor");
        }
      
        public SleepyUpdater(Module module, bool staggerUpdates = false)
        {
            this.staggerUpdates = staggerUpdates;
            belongsToModule = module;

            AddToLookup();

            timer = new HighResolutionTime();
        }

        private void SetDirty()
        {
            listIsDirty = true;
        }


        private void AddToUpdatables(T item)
        {
            if (!lookupMap.ContainsKey(item))
            {
                lookupMap.Add(item, item);
                listOfNonSleepingItems.Add(item);
                SetDirty();
            }
        }

        private void RemoveFromUpdatables(T item)
        {
            if (lookupMap.Remove(item))
            {
                listOfNonSleepingItems.Remove(item);
                // no need to re-sort
            }
        }

        /// <summary>
        /// in Sim, probably best to keep existing timepoint after load! and snapshot the timepoint too.
        /// </summary>
        /// <param name="itemToAdd"></param>
        /// <param name="staggerUpdate"></param>
        /// <param name="keepExistingTimepoint"></param>
        public void Add(T itemToAdd, bool? staggerUpdate = null, bool keepExistingTimepoint = false) //, bool setTimePoint = true)
        {
            // register the event listener to re-schedule or wake up sleepers
            // new: the client must call a method instead, using this id
            itemToAdd.SleepyUpdater = this.id;
          
            // set the update timepoint:
            bool staggerThisUpdate = false;
            if (this.staggerUpdates && staggerUpdate != false)
            {
                staggerThisUpdate = true;
            }

            SetNextTimePoint(itemToAdd, staggerThisUpdate, true, keepExistingTimepoint);
                                
        }

    
        public void NotifyUpdateIntervalChanged(ISleepingUpdatable item)
        {           
            // interval changed - set the new update timepoint:
            SetNextTimePoint((T)item, false, true);

        }

        public void Remove(T item)
        {
            
            RemoveFromUpdatables(item);
                      
        }

       

        /// <summary>
        /// handles continual updates of a list of objects, when updates are not needed every frame
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="listIsDirty"></param>
        public void Update(GameTime gameTime) 
        {
            if (listOfNonSleepingItems.Count == 0)
                return;

            bool wasDestroyed;
            // long currentTicks = The.Sim.TotalUnPausedGameTime.Ticks;
            // double currentSeconds = The.Sim.TotalUnPausedGameTimeInSeconds; 
            T item;

            
            if (listIsDirty)
            {
                // only sort if needed
                SortTimedUpdatableList<T>(listOfNonSleepingItems);
                listIsDirty = false;
            }
               

           
         
            int index = 0;
            while (listOfNonSleepingItems.Count > 0 && index < listOfNonSleepingItems.Count)
            {
                item = listOfNonSleepingItems[index];              

                if (item.TimePointInSeconds == null || !The.Sim.TimepointReached(item.TimePointInSeconds.Value)) // item.TimePointInSeconds > currentTicks)
                {
                    break; // done. skip the following items too
                }

             //   timer.Start();

                item.Update(gameTime, out wasDestroyed);

                /*
                double timeTaken = timer.GetTime();
                if (timeTaken > 0.002)
                {
                 
                }*/

                if (The.Sim == null) // the game can have ended here
                {
                    return;
                }

                if (!wasDestroyed)
                {
                    // the item is still in the list - update the counter to get the following item:
                    index++;

                             
                    bool intervalChanged;
                    // ask each subsystem when it requires the next update:
                    // perhaps fires event to set timepoint
                    item.RecomputeUpdateInterval(out intervalChanged);

                    // if the interval changed, then SetNextTimePoint will already have been called when the event fired.
                    if (!intervalChanged)
                    {
                        // find the next time point we want to update the item:
                        SetNextTimePoint(item, false, intervalChanged); //, timeBeforeNextUpdate);
                    }

                }

            }

        }



        public void IterateItems(Action<T> action)
        {
            foreach (var item in this.listOfNonSleepingItems)
            {
                action(item);  

            }
        }

        public T GetItem(Func<T, bool> matches)
        {
            return listOfNonSleepingItems.FirstOrDefault(matches);

        }

        private void SetNextTimePoint(T updatable, bool staggerUpdates, bool intervalChanged, bool keepExistingTimePoint = false) 
        {
            double? updateInterval = updatable.UpdateInterval;

            if (updateInterval == null) // updatable.UpdateInterval == null) 
            {
                // put to sleep...
                updatable.SetNextTimepoint(null);

                RemoveFromUpdatables(updatable); // happens rarely
           
            }
            else if (Common.IsZero(updateInterval)) // updatable.UpdateInterval.Value))
            {

                // use Sim or Client time???
                updatable.SetNextTimepoint(UpdateTimePoints.ComputeTimePointFromInterval(0));

                // see if we can avoid a lookup every frame for 0 interval items:
                if (intervalChanged) 
                {                    
                    AddToUpdatables(updatable);
                }
               
            }
            else
            {                     
                if (!keepExistingTimePoint) // true when loading
                {
                    double interval = updateInterval.Value; // updatable.UpdateInterval.Value;

                    if (staggerUpdates)
                    {
                        // add a small number to the timepoint to stagger this and the following updates:
                        double randomNumber;
                        if (belongsToModule == Module.Sim)
                        {
                            randomNumber = The.Sim.GameplayRandomGenerator.NextDouble("");
                        }
                        else
                        {
                            randomNumber = The.Client.ClientRandomGenerator.NextDouble("");
                        }

                        interval += (0.5 - randomNumber) * interval;

                        interval = Common.ClampBottom(interval, 0d);
                    }

                    // use Sim or Client time???
                    updatable.SetNextTimepoint(UpdateTimePoints.ComputeTimePointFromInterval(interval)); // The.Sim.TotalUnPausedGameTimeInSeconds + updatable.UpdateInterval.Value;

                }

                if (intervalChanged)
                {
                    AddToUpdatables(updatable);
                }

                SetDirty(); // always re-sort when setting less than every frame updates
                               
            }

        }

     

        /// <summary>
        /// sort a list of objects so the ones with the sooner timepoint for next update/expiry are at the top.
        /// 
        /// generic constraint are not part of the method signature, so overloading with the same name is impossible
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        private void SortTimedUpdatableList<U>(List<U> list) where U : ISleepingUpdatable
        {
            // sort by next update timepoint:

            list.Sort((e1, e2) =>
            {
                if (e1.TimePointInSeconds == null)
                {
                    if (e2.TimePointInSeconds == null)
                    {
                        return 0;
                    }
                    else return 1;
                }

                if (e2.TimePointInSeconds == null)
                {
                    return -1;
                }

                return e1.TimePointInSeconds.Value.CompareTo(e2.TimePointInSeconds.Value);
                //eventActionsAreDirty = false;
            });
          
        }


     /*   #region ISnapshot

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

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            id = SnapshotID(sn, id);
            IDCounter = (SleepyUpdaterID)sn.DoEnum(IDCounter);

            // don't snapshot items... repopulate post-load...
           // this.listOfNonSleepingItems = sn.DoList(listOfNonSleepingItems);
      
            this.belongsToModule = (Module)sn.DoEnum(belongsToModule);
            this.listIsDirty = sn.DoBool(listIsDirty);
            this.staggerUpdates = sn.DoBool(staggerUpdates);

            sn.Ignore(listOfNonSleepingItems);
            sn.Ignore(lookupMap); // rebuild on post-load

            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {

            foreach (var item in listOfNonSleepingItems)
            {
                lookupMap.Add(item, item);
            }


            //an iteration of the Collection, calling LoadPostProcess on every instance of T - needed????
            foreach (var il in lookupMap)
            {
                ISnapshot snap = il.Value as ISnapshot;
                if (snap != null)
                {
                    snap.LoadPostProcess(sn);

                    snap.IsSnapshotted = false; // reset
                }
            }           
        
        }

        #endregion*/


        #region ILookup - NOTE: NOT implementing the interface - only following the same pattern. ILookUp cannot handle generic types.
        // keep the ID counter in the item class as we usually do.
        private SleepyUpdaterID id = SleepyUpdaterID.Invalid;
        static SleepyUpdaterID IDCounter = SleepyUpdaterID.First;

        public SleepyUpdaterID ID
        {
            get
            {
                return id;
            }

            private set
            {
                id = value;
            }
        }


        public SleepyUpdaterID GetUniqueID()
        {
            IDCounter++;
            if (IDCounter >= SleepyUpdaterID.Max)
            {
                throw new Exception("Astounding, SleepyUpdaterID just exceeded 64 bits. Something seriously wrong has happened.");
            }

            return IDCounter;
        }

        public SleepyUpdaterID SnapshotID(Snapshotter sn, SleepyUpdaterID id)
        {
            return (SleepyUpdaterID)sn.DoEnum(id);
        }

        public void AddToLookup()
        {
            if (typeof(T) == typeof(Entity))
            {

            }

            ID = GetUniqueID();
            if (ID != SleepyUpdaterID.Invalid)
            {
                LookUpSleepyUpdater<T>.Add(ID, this);
            }
        }

        public void RemoveIDEntry()
        {
            LookUpSleepyUpdater<T>.Remove(this);
        }

       /* void ILookUp<SleepyUpdater<T>, SleepyUpdaterID>.ResetIDCounter()
        {
        }*/

        public static void ResetIDCounter() // called by invoke, do not remove
        {
            IDCounter = SleepyUpdaterID.First;
        }

        public void SetInvalid()
        {
            id = SleepyUpdaterID.Invalid;
        }

        #endregion

    }
        
    /// <summary>
    /// a static non-generic helper class to supplement the SleepyUpdatable interface for convenience
    /// </summary>
    public class UpdateTimePoints
    {

        /// <summary>
        /// sets the timepoint for when the object should have its next update called
        /// </summary>
        /// <param name="hasTimepoint"></param>
      /*  public static void SetTimePoint(ISleepingUpdatable hasTimepoint)
        {
            double current = The.Sim.TotalUnPausedGameTimeInSeconds;
            hasTimepoint.TimePointInSeconds = current + hasTimepoint.UpdateInterval; // TimeInSecondsBetweenUpdates can be 0 - this means update each frame
        }*/

        public static double ComputeTimePointFromInterval(double interval) 
        {
            return The.Sim.TotalUnPausedGameTimeInSeconds + interval;           
        }

        
        public static void GetSoonestInterval(double? tempInterval, ref double? currentInterval)
        {
            if (tempInterval.HasValue)
            {
                // sets the new time if we need a sooner update:
                if (currentInterval == null)
                {
                    currentInterval = tempInterval;
                }
                else if (tempInterval.Value < currentInterval.Value)
                {
                    currentInterval = tempInterval;
                }
            }
        }

        /// <summary>
        /// converts a time point into an interval by subtracting the current time...
        /// </summary>
        /// <param name="expiryTimePointInSeconds"></param>
        /// <returns></returns>
        public static double? ComputeIntervalFromTimepoint(double? expiryTimePointInSeconds)
        {
            double? timeBeforeNextUpdate = null;

            if (expiryTimePointInSeconds.HasValue)
            {
                // wake us up when it is time to die...
                // don't clamp to zero... better to overshoot a bit.
                timeBeforeNextUpdate = Common.ClampBottom(expiryTimePointInSeconds.Value - The.Sim.TotalUnPausedGameTimeInSeconds, 0.01d); // ok to use the time calculated by Sim..? 
            }

            return timeBeforeNextUpdate;
        }


    
    }
}
