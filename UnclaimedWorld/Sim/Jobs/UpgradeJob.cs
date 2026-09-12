using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Entities.Containers;

namespace UWGame.SimSide.Jobs
{
    public class UpgradeJob : ISnapshot
    {       
        public UpgradeCategory UpgradeCategory;


        public UpgradeJob(UpgradeCategory upgradeCategory) 
        {
            this.UpgradeCategory = upgradeCategory;
           
        }




        public UpgradeJob()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }

       

      
        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            UpgradeCategory = sn.DoGameData(UpgradeCategory);
            
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

        }

        public bool IsSnapshotted
        {
            get;
            set;
        }


        #endregion
    }
}
