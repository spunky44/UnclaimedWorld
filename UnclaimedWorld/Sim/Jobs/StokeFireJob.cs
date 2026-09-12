using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Items;
using UWGame.SimSide.Snapshots;
namespace UWGame.SimSide.Jobs
{
    /// <summary>
    /// NOT CURRENTLY USED 
    /// </summary>
    public class StokeFireJob: Job
    {
        
        public float ManSecondsOfWorkNeeded = 0f;

        /// <summary>
        /// also a tile?
        /// </summary>
        public Entity FireSite;
       

        /// <summary>
        /// Value between 0 - 1
        /// </summary>
        public float Progress;

        

        public StokeFireJob()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }

        public StokeFireJob(Entity fireSite, EntityGroup entityGroup) //, Priority priority)
            : base(entityGroup) //, priority) 
        {           
            FireSite = fireSite;

            ComputeJobType();
            SetDefaultPriority(entityGroup);
        }

        public bool IsInProgress()
        {
            return Progress > 0f;
        }


        public override Vector3? GetCircaLocation()
        {
            return null;
        }
    }
}
