using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Resources
{
    public enum ResourceItemID : ulong
    {
        First = 0L,
        Invalid = ulong.MaxValue,
        Max = Invalid
    }

    /// <summary>
    /// pseudo item that has not been created yet... used for coordinating agents
    /// </summary>
    public interface IResourceItem : ILookUp<IResourceItem, ResourceItemID>, ISnapshot
    {
        ResourceContainer Container { get; }

      //  Entity TargetedForHarvestingBy { get; set; }

        /// <summary>
        /// remember to de-assign when the job is destroyed...
        /// </summary>
        ProcessJob AssignedToJob { get; set; }


        void Destroy();

    }
}
