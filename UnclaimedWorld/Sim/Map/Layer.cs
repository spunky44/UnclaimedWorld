using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Maps
{
    public abstract class Layer: ISnapshot
    {
        public ushort Width { get; private set; }
        public ushort Height { get; private set; }

        public ushort SectorsAcrossWidth { get; private set; }
        public ushort SectorsAcrossHeight { get; private set; }

        public abstract int SectorSize { get; }


        public Layer(ushort width, ushort height)
        {
            this.Width = width;
            this.Height = height;

            SectorsAcrossWidth = (ushort)Math.Ceiling((double)Width / SectorSize); // round up
            SectorsAcrossHeight = (ushort)Math.Ceiling((double)Height / SectorSize);
        }

        public Layer()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }

        public void GetSectorFromAbsoluteCoords(int x, int y, out ushort sectorX, out ushort sectorY)
        {
            ushort relativeX, relativeY;
            GetSectorAndRelativeCoords(x, y, out sectorX, out sectorY, out relativeX, out relativeY);

        }

        public void GetSectorAndRelativeCoords(int x, int y, out ushort sectorX, out ushort sectorY, out ushort relativeX, out ushort relativeY)
        {
            int relX, relY;
            sectorX = (ushort)Math.DivRem(x, SectorSize, out relX);
            sectorY = (ushort)Math.DivRem(y, SectorSize, out relY);

            relativeX = (ushort)relX;
            relativeY = (ushort)relY;
        }



        #region ISnapshot


        Snapshotter.Version version;
        public virtual Snapshotter.Version DoVersion(Snapshotter sn)
        {
            version = sn.DoVersion(Snapshotter.Version.Original);
            return version;
        }

        public bool IsSnapshotted { get; set; }

        public virtual ISnapshot DoSnapshot(Snapshotter sn)
        {
            this.Width = sn.DoUInt16(Width);
            this.Height = sn.DoUInt16(Height);
            this.SectorsAcrossWidth = sn.DoUInt16(SectorsAcrossWidth);
            this.SectorsAcrossHeight = sn.DoUInt16(SectorsAcrossHeight);


           
            return this;
        }


        public virtual void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

        }

        #endregion
    }
}
