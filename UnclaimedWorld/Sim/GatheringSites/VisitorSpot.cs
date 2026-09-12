using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Snapshots;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.GatheringSites
{
    public class VisitorSpot : IComparable<VisitorSpot>, ISnapshot
    {
        public EntityID EntityID;
        public Vector2 WorldLocation;
        public double DistanceSquared;
        public bool Arrived = false;

        public VisitorSpot(EntityID id, Vector2 pos, bool arrivedIn = false)
        {
            EntityID = id;
            WorldLocation = pos;
            DistanceSquared = 0f;
            Arrived = arrivedIn;
        }

        public VisitorSpot()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");

        }

        public int CompareTo(VisitorSpot other)
        {
            // If other is not a valid object reference, this instance is greater. 
            if (other == null)
                return 1;

            return DistanceSquared.CompareTo(other.DistanceSquared);
        }


        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            this.EntityID = sn.DoEntityID(EntityID);
            this.WorldLocation = sn.DoVector2(WorldLocation);
            this.DistanceSquared = sn.DoDouble(DistanceSquared);
            this.Arrived = sn.DoBool(Arrived);


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
