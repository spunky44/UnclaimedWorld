using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.GatheringSites
{
    public class Arc: ISnapshot
    {
        public float Radius;
        public double MinAngle;
        public double MaxAngle;

        public Arc(float rad, int min, int max)
        {
            Radius = rad;
            MinAngle = (Math.PI * min) / 180.0;
            MaxAngle = (Math.PI * max) / 180.0;
            //TODO enforce that min is in fact less than max
            //TODO also enforce the 360 degre wrapping problem for cases
            //where the arc encompasses zero degrees... min needs to be
            //both need to be withing the range of -180 and 180 degrees?
        }


        public Arc()
        {

        }


        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            this.Radius = sn.DoFloat(Radius);
            this.MinAngle = sn.DoDouble(MinAngle);
            this.MaxAngle = sn.DoDouble(MaxAngle);


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
