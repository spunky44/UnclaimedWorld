using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Maps
{
  
   
    /// <summary>
    /// immutable class!
    /// </summary>
    public class Region: ISnapshot 
    {
        public Vector2 CenterLocation { get; private set; }

        public Point CenterInSubtiles { get; private set; }

        /// <summary>
        /// zero is 'no region' on the map.
        /// </summary> 
        public ushort Color { get; private set; }

        public Color DebugColor;

       // public bool IsDirty = false;

        public Region(Point centerInSubTiles, ushort color)
        {
            if (color == 321 && centerInSubTiles.X == 118 && centerInSubTiles.Y == 61)
            {

            }
          
            this.CenterInSubtiles = centerInSubTiles;
            this.CenterLocation = MapManager.SubTileToWorldPos(centerInSubTiles);

            this.Color = color;

            DebugColor = Common.GetRandomColorFromSeed(color); 
        }


        public Region()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }

       
            

        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            this.CenterLocation = sn.DoVector2(CenterLocation);
            this.CenterInSubtiles = sn.DoPoint(CenterInSubtiles);
            this.Color = sn.DoUInt16(Color);
           // this.IsDirty = sn.DoBool(IsDirty);

            sn.Ignore(DebugColor);

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

            DebugColor = Common.GetRandomColorFromSeed(Color); 
        }

        #endregion


    }
}
