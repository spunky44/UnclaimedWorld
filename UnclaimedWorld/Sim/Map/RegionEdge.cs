using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Maps
{
    public class RegionEdge: ISnapshot
    {
        public bool HasRoad; // ??

        public ushort FromRegion;
        public ushort ToRegion;

        public float Length;

        // perhaps mark the edge as going between layers?



        public RegionEdge(ushort from, ushort to, float length, bool hasRoad)
        {
            this.FromRegion = from;
            ToRegion = to;
            Length = length;
            HasRoad = hasRoad;
        }

        public RegionEdge()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            HasRoad = sn.DoBool(HasRoad);
            FromRegion = sn.DoUInt16(FromRegion);
            ToRegion = sn.DoUInt16(ToRegion);
            Length = sn.DoFloat(Length);

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
    }
}
