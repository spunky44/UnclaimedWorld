using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Snapshots;
using Microsoft.Xna.Framework;

namespace UWGame.SimSide.Overland.Missions
{
    /// <summary>
    /// needed to recreate the mission actions from xml... perhaps also for use in scripting?
    /// </summary>
   // public enum MissionActionTypes { Buy, Load, Sell, Unload, Travel }

    public abstract class MissionAction: ISnapshot
    {
       
        public Mission parent;




        public MissionAction(Mission parent)
        {
            this.parent = parent;
        }

        public MissionAction()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
      
        }


      


       /* public abstract MissionActionTypes Type
        {
            get;
        }*/


        public virtual void Destroy()
        {
            
        }


        public virtual bool Update(GameTime elapsed)
        {
            return false;
        }

        public virtual void StartMission()
        {

        }

        protected void HandleFailedAction()
        {
            // for AI and player missions, call Abort() here.
            // later, we could allow the player some action, selected in a dialog of sorts...
            parent.Abort();
        }

        #region ISnapshot

        public virtual ISnapshot DoSnapshot(Snapshotter sn)
        {


            sn.Ignore(parent); // gets pushed post-load

            return this;

        }

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public virtual Snapshotter.Version DoVersion(Snapshotter sn)
        {
            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }

        public bool IsSnapshotted { get; set; }

        public virtual void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

        }

        #endregion
    }
}
