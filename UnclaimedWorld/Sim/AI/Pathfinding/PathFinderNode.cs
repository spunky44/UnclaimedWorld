using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Pathfinding
{
    [DebuggerDisplay("{AbsoluteX},{AbsoluteY}, Parent: ({ParentAbsoluteX},{ParentAbsoluteY}), Status: {Status}, F: {F}, G: {G}")]
    public class PathFinderNode : ISnapshot
    {
        /// <summary>
        /// ushort may be too small... has to hold the cost, not just the distance in subtiles.
        /// max: 10 * 512 * 3 = 15.000
        /// </summary>
        public int F; // f = gone + heuristic
        public int G;
        public ushort AbsoluteX; 
        public ushort AbsoluteY;

        /// <summary>
        /// Sector coordinates
        /// </summary>
        public /*ushort*/ byte SectorX; 
        public /*ushort*/ byte SectorY; 
        public ushort ParentAbsoluteX; // Parent
        public ushort ParentAbsoluteY; // Parent
        public byte Status;

        // TODO: later add a 'state' variable - telling which vehicle (type) we are onboard



        #region ISnapshot

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

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            this.F = sn.DoInt32(F);
            this.G = sn.DoInt32(G);
            this.SectorX = sn.DoByte(SectorX);
            this.SectorY = sn.DoByte(SectorY);
            this.AbsoluteX = sn.DoUInt16(AbsoluteX);
            this.AbsoluteY = sn.DoUInt16(AbsoluteY);
            this.ParentAbsoluteX = sn.DoUInt16(ParentAbsoluteX);
            this.ParentAbsoluteY = sn.DoUInt16(ParentAbsoluteY);
            this.Status = sn.DoByte(Status);

            return this;
        }


        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

        }

        #endregion
    }


    /*
    public class PathFinderNode: ISnapshot
    {       
        public int F; // f = gone + heuristic
        public int G;
        public ushort X; // NEW
        public ushort Y; // NEW
        public ushort PX; // Parent
        public ushort PY; // Parent
        public byte Status;
        // TODO: add a 'state' variable - telling which vehicle (type) we are onboard



        #region ISnapshot

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

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            this.F = sn.DoInt32(F);
            this.G = sn.DoInt32(G);
            this.X = sn.DoUInt16(X);
            this.Y = sn.DoUInt16(Y);
            this.PX = sn.DoUInt16(PX);
            this.PY = sn.DoUInt16(PY);
            this.Status = sn.DoByte(Status);
          
            return this;
        }


        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

        }

        #endregion
    }*/
}
