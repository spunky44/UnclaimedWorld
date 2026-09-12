using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Maps
{
    /// <summary>
    /// similar to SubtileSector
    ///    
    /// 
    /// The LOH is used for allocating memory for large objects (such as arrays) that require more than 85,000 bytes.
    /// </summary>
    public class TileSector: ISnapshot 
    {
        /// <summary>
        /// sector coords
        /// </summary>
        public Point Coords;

        public byte[][] Map; // { get { return map; } set { map = value; } }

        /// <summary>
        /// we use IsBlocked when adding up to the final movement map
        /// this could be packed within byte cost if we ever bother...
        /// </summary>
        public bool[][] IsBlocked; // { get { return isBlocked; } set { isBlocked = value; } }
        //private bool[][] isBlocked;


        /// <summary>
        /// Moved from InfluenceMap
        /// 
        /// we keep 2 maps in order to compare them after redrawing and detect changes to the sectors.
        /// then we switch them around.
        /// </summary>
        protected byte[][] backBufferMap1;
        protected byte[][] backBufferMap2;


        #region Moved from Sector


        /// <summary>
        /// this flag indicates if the tiles in the sector have changed during Redraw, and is used to determine if adding is needed higher up in the chain
        /// </summary>
        public bool IsDirty = true; // start with dirty sectors so we get a full redraw.


        public Rectangle TileArea;

        #endregion


        public TileSector()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor");
        }

        public TileSector(TileLayer parent, int sectorCoordsX, int sectorCoordsY) //, int mapWidth, int mapHeight)
        {
            Coords = new Point(sectorCoordsX, sectorCoordsY);

            TileArea = new Rectangle();

            int sectorSize = parent.SectorSize;

          /*  this.width = width;
            this.height = height;*/


            Common.InitJaggedArray(ref backBufferMap1, sectorSize, sectorSize);
            Common.InitJaggedArray(ref backBufferMap2, sectorSize, sectorSize);

            Map = backBufferMap1;

            Common.InitJaggedArray(ref IsBlocked, sectorSize, sectorSize);            

            TileArea.X = sectorCoordsX * sectorSize; 
            TileArea.Y = sectorCoordsY * sectorSize; 

            TileArea.Width = Common.ClampTop(sectorSize, sectorSize - (TileArea.X + sectorSize - parent.Width));
            TileArea.Height = Common.ClampTop(sectorSize, sectorSize - (TileArea.Y + sectorSize - parent.Height));
        }

        public void Initialize()
        {

        }


        public void SetValue(int relativeX, int relativeY, byte value, byte? blockingLimit)
        {
            Map[relativeX][relativeY] = value;

            if (blockingLimit.HasValue)
            {
                if (value > blockingLimit.Value)
                {
                    IsBlocked[relativeX][relativeY] = true;
                }
                else
                {
                    IsBlocked[relativeX][relativeY] = false;
                }

            }
        }

        public byte GetValue(ushort relativeX, ushort relativeY)
        {
            return Map[relativeX][relativeY];
        }

        public bool GetIsBlocked(ushort relativeX, ushort relativeY)
        {
            return IsBlocked[relativeX][relativeY];
        }

       
        public void ClearMap()
        {          
            Common.ClearJaggedArray(Map);
            Common.ClearJaggedArray(IsBlocked);

            // init...
            IsDirty = false;
          
        }


        public void AddSector(TileSector sectorToAdd, /* Rectangle sectorArea,*/ float weight/*, byte[][] map, bool[][] isBlocked, byte[][] mapToAdd, bool[][] isBlockedToAdd*/)
        {
            byte[] column, columnToAdd;
            bool[] isBlockedColumn, isBlockedToAddColumn;
            for (int x = 0; x < TileArea.Width; x++)
            {
                column = Map[x];
                isBlockedColumn = IsBlocked[x];

                columnToAdd = sectorToAdd.Map[x];
                isBlockedToAddColumn = sectorToAdd.IsBlocked[x];

                for (int y = 0; y < TileArea.Height; y++)
                {
                    column[y] = (byte)(Common.ClampTop(column[y] + weight * columnToAdd[y], 255));
                    isBlockedColumn[y] = isBlockedColumn[y] || isBlockedToAddColumn[y];
                }
            }

            /*
            byte[] column, columnToAdd;
            bool[] isBlockedColumn, isBlockedToAddColumn;
            for (int x = sectorArea.Left; x < sectorArea.Right; x++)
            {
                column = map[x];
                isBlockedColumn = isBlocked[x];

                columnToAdd = mapToAdd[x];
                isBlockedToAddColumn = isBlockedToAdd[x];

                for (int y = sectorArea.Top; y < sectorArea.Bottom; y++)
                {
                    column[y] = (byte)(Common.ClampTop(column[y] + weight * columnToAdd[y], 255));
                    isBlockedColumn[y] = isBlockedColumn[y] || isBlockedToAddColumn[y];
                }
            }*/
        }


        public void SwitchBuffers()
        {
            if (Map == backBufferMap1)
            {
                Map = backBufferMap2;
            }
            else
            {
                Map = backBufferMap1;
            }
        }

        /// <summary>
        /// moved from ThreatMap
        /// </summary>
        /// <param name="sector"></param>
        /// <returns></returns>
        public bool SectorHasChanged()
        {
            for (int x = 0; x < TileArea.Width; x++)
            {
                for (int y = 0; y < TileArea.Height; y++)
                {
                    // compare old and new values:
                    if (backBufferMap1[x][y] != backBufferMap2[x][y])
                    {
                        return true;
                    }
                }
            }
            /*
            for (int x = TileArea.Left; x < TileArea.Right; x++)
            {
                for (int y = TileArea.Top; y < TileArea.Bottom; y++)
                {
                    // compare old and new values:
                    if (backBufferMap1[x][y] != backBufferMap2[x][y])
                    {
                        return true;
                    }
                }
            }*/

            return false;
        }

        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            /*id = SnapshotID(sn, id);
            IDCounter = (SubtileLayerID)sn.DoEnum(IDCounter);
            */

            this.Map = sn.DoJaggedArray(Map);
            this.Coords = sn.DoPoint(Coords);
            this.TileArea = sn.DoRectangle(TileArea);
            this.IsBlocked = sn.DoJaggedArray(IsBlocked);
            this.IsDirty = sn.DoBool(IsDirty);

            if (sn.mode != Snapshotter.Mode.Load)
            {
                if (backBufferMap1 != Map)
                {
                    // save a null instead of a copy of Map:
                    sn.DoJaggedArray(backBufferMap1);
                    sn.DoJaggedArray((byte[][])null);

                    sn.Ignore(backBufferMap2);
                }

                if (backBufferMap2 != Map)
                {
                    // save a null instead of a copy of Map:
                    sn.DoJaggedArray((byte[][])null);
                    sn.DoJaggedArray(backBufferMap2);

                    sn.Ignore(backBufferMap1);
                }
            }
            else
            {
                // we will load a null for one field, and then replace with a reference to Map.
                backBufferMap1 = sn.DoJaggedArray(backBufferMap1);
                backBufferMap2 = sn.DoJaggedArray(backBufferMap2);
            }

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

            if (backBufferMap1 == null)
            {
                backBufferMap1 = Map;
            }

            if (backBufferMap2 == null)
            {
                backBufferMap2 = Map;
            }
        }

        #endregion
    }
}
