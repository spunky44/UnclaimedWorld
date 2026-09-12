using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.AI.Pathfinding;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Maps
{
    
    public class Sector: ISnapshot
    {
       
        /// <summary>
        /// set to true during Clear(). Set to false if anything was drawn into the sector during Redraw(). 
        /// this flag tells us that nothing was drawn in the sector, and it doesn't need to be compared to anything for changes.
        /// </summary>
        public bool SectorIsClear = true;

        /// <summary>
        /// this flag indicates if the tiles in the sector have changed during Redraw, and the subtiels should be re-added
        /// </summary>
        public bool SubtilesAreDirty = false; // no redraw is needed to start with.
        

        public Rectangle TileArea;

        public Sector(Point sectorCoords, int sectorSizeInTiles, int mapWidth, int mapHeight)
        {
            TileArea = new Rectangle();

            TileArea.X = sectorCoords.X * sectorSizeInTiles;
            TileArea.Y = sectorCoords.Y * sectorSizeInTiles;

            TileArea.Width = Common.ClampTop(sectorSizeInTiles, sectorSizeInTiles - (TileArea.X + sectorSizeInTiles - mapWidth));
            TileArea.Height = Common.ClampTop(sectorSizeInTiles, sectorSizeInTiles - (TileArea.Y + sectorSizeInTiles - mapHeight));
        }


        public Sector()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }

        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            SubtilesAreDirty = sn.DoBool(SubtilesAreDirty);
            SectorIsClear = sn.DoBool(SectorIsClear);
            TileArea = sn.DoRectangle(TileArea);

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
