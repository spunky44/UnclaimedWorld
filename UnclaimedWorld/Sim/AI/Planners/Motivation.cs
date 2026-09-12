using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Planners
{
    abstract class Motivation: ISnapshot
    {


        //Baseclass for our motivation classes.
        //The different motivation classes will determine what different actions we are motivated to do


        //protected abstract GetActionsWeAreMotivatedToDo();

        public bool IsFulfilled()
        {
            return false;
        }






        #region ISnapshot

        public virtual ISnapshot DoSnapshot(Snapshotter sn)
        {
            
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
