using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace UWGame.SimSide.Systems
{
  
    /// <summary>
    /// implementing this interface will make the object able to 'go to sleep' or request an update with much lower frequency from the SleepyUpdater class
    /// </summary>
    public interface ISleepingUpdatable
    {
        /// <summary>
        /// the list will be sorted by this value.
        /// 
        /// As it is designed, the SleepyUpdater that manages the list will set this value based on what the item itself places in UpdateInterval.        
        /// </summary>
        double? TimePointInSeconds { get; }


        /// <summary>
        /// only the SleepyUpdater should call this
        /// </summary>
        /// <param name="timepoint"></param>
        void SetNextTimepoint(double? timepoint);
        
        /// <summary>
        /// this property is used to detect if there were changes in the interval
        /// this gets set to null to signal that the renderable requires no updates (is sleeping)
        /// 
        /// It is OK for the item to set/recompute this
        /// </summary>
        double? UpdateInterval { get; }


        /// <summary>
        /// the sleepy updater class will be listening for this event - very important!
        /// 
        /// use this to notify the collection of interval changes!!!
        /// </summary>     
        //SleepyUpdater<T> SleepyUpdater { get; set; }
        SleepyUpdaterID SleepyUpdater { get; set; }
     

        void Update(GameTime gameTime, out bool wasDestroyed);

        /// <summary>
        /// SleepyUpdater will call this at the end of each Update to make sure that the interval is up-to-date. Then, it will set the next Update timepoint.
        /// 
        /// Should also be called by the item when:
        /// 1. Constructed
        /// 2. its state changes - this is the only way to wake up a sleeping item!
        /// </summary>
        /// <param name="intervalChanged"></param>
        void RecomputeUpdateInterval(out bool intervalChanged);


        /// <summary>
        /// NEW: static col instance is no longer lazily inited in the static ctor, but must be created explicitly on startup
        /// </summary>
        void CreateSleepyLookupCollection();

    }


}
