using System;
using System.Collections.Generic;

using System.Text;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Systems.TimeSlicing
{

    public enum CyclableID : ulong
    {
        First = 0L,
        Invalid = ulong.MaxValue,
        Max = Invalid
    }    

    /// <summary>
    /// All ICyclable classes will be snapshotted from the LookUp<ICyclable, CyclableID> collection.  
    /// 
    /// Some ICyclables are order dependent when loading, one example is RegionSearchPlanner that depends on DependentRegionMap having been loaded (LoadPostProcess'ed) first...
    /// </summary>
    public interface ICyclable : ILookUp<ICyclable, CyclableID>, ISnapshot
    {
        bool CycleOnce();

        /// <summary>
        /// implementors can use this to indicate if they want to pause, i.e. not receive timeslices.
        /// </summary>
        bool IsPaused { get; }

        /// <summary> 
        /// This flag will indicate to CycleManager if the Icyclable wants to stay in the list after snapshotting.
        /// 
        /// ICyclable decides in its implementation of Snapshot() whether it will be snapshotting its current progress (then return false), 
        /// or whether it wants to scrap the progress and start over (then return true).
        /// 
        /// </summary>
        bool UnregisterBeforeSnapshot { get; }


        double StartedOnTimeInSeconds { get; set; }
        double ComputationTimeSpentInSeconds { get; set; }

        double TotalComputationAllInstancesInSeconds { get; set; }

        double? UpdateInterval { get; }

        void PrintInfo(StringBuilder text);

    }
}
