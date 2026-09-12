using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.AI.Goals;

namespace UWGame.SimSide.Jobs
{
    public class ReplenishJob : ISnapshot
    {
        public GoalReplenish.ReplenishAction Action;

        /// <summary>
        /// why not use ActingOnEntity?
        /// </summary>
        public EntityID EntityToReplenish;


        public ReplenishJob(EntityID entityToReplenish, GoalReplenish.ReplenishAction action) //, EntityGroup belongsTo, Priority priority)
            //: base(belongsTo, priority)
        {
            this.EntityToReplenish = entityToReplenish;
            this.Action = action;

        }

      
       

        public ReplenishJob()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }

       

      
        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
          //  this.snapshotJob = (JobID)sn.SnapshotID<Job, JobID>(ProcessJob);

            EntityToReplenish = sn.DoEnum(EntityToReplenish);
            Action = sn.DoEnum(Action);
            
            return this;
        }

        Snapshotter.Version version;
        public Snapshotter.Version DoVersion(Snapshotter sn)
        {
            version = sn.DoVersion(Snapshotter.Version.Original);
            return version;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            //ProcessJob = (ProcessJob)LookUp<Job, JobID>.FindByID(snapshotJob);

        }

        public bool IsSnapshotted
        {
            get;
            set;
        }


        #endregion
    }
}
