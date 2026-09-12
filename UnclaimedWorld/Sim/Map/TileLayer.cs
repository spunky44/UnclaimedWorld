using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Maps
{
    /// <summary>
    /// similar to SubtileLayer, but with 9x lower resolution
    /// </summary>
    public class TileLayer : Layer
    {
        public override int SectorSize
        {
            get { return MapManager.SectorSizeInTiles; }
        }

        public TileSector[][] Sectors;


        const byte defaultValue = 0;

        /// <summary>
        /// the limit for considering a tile blocked (cost = 0)
        /// </summary>
        public byte? BlockingLimit;


        /// <summary>
        /// during drawing and after clear, all sectors that get a value are gathered in this set.
        /// sectors that are not drawn into, and which are not added to by a child map, should be removed at the end.
        /// </summary>
        public HashSet<Point> AffectedSectors = new HashSet<Point>();

        /// <summary>
        /// these are the sectors that were drawn into last time. We compare the two sets for any changes in order to signal the dependent maps via the IsDirty flag.
        /// </summary>
        public HashSet<Point> PreviouslyAffectedSectors = new HashSet<Point>();
       

        public TileLayer()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor");
        }


        public TileLayer(ushort width, ushort height)
            : base(width, height)
        {
            Common.InitJaggedArray(ref Sectors, SectorsAcrossWidth, SectorsAcrossHeight);

        }

        /// <summary>
        /// optimized to avoid unnecessary operations
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="value"></param>
        /// <param name="operation"></param>
        /// <param name="affectedSector"></param>
        /// <returns></returns>
        public bool SetValue(int x, int y, double value, InfluenceMap.Operation operation, out TileSector affectedSector)
        {
            ushort relativeX, relativeY;
            ushort sectorX, sectorY;

            affectedSector = null;

            GetSectorAndRelativeCoords(x, y, out sectorX, out sectorY, out relativeX, out relativeY);
            TileSector sector = Sectors[sectorX][sectorY];

            if (sectorX > 0 && sectorY > 0)
            {

            }

            byte oldValue = defaultValue;
            if (sector == null)
            {
                if (operation == InfluenceMap.Operation.SetValue)
                {
                    if (value == defaultValue)
                    {
                        return false; // don't spawn an empty sector
                    }
                    else
                    {
                        sector = CreateSector(sectorX, sectorY);
                    }
                }
                else if (operation == InfluenceMap.Operation.AddToExisting)
                {
                    if (value <= 0)
                    {
                        // can't store a negative value.
                        return false; // don't spawn an empty sector
                    }
                    else
                    {
                        sector = CreateSector(sectorX, sectorY);
                    }
                }
            }
            else 
            {
                oldValue = sector.GetValue(relativeX, relativeY);
            }

            if (sector != null)
            {
                byte newValue; 
                if (operation == InfluenceMap.Operation.AddToExisting)
                {
                    newValue = (byte)Common.Clamp(value + oldValue, 0, 255);
                }
                else 
                {
                    if (value != oldValue)
                    {
                        newValue = (byte)Common.Clamp(value, 0, 255); 
                    }
                    else 
                    {
                        return false;
                    }

                }               
                

                affectedSector = sector;
                sector.SetValue(relativeX, relativeY, newValue, BlockingLimit);

                return true;               
              
            }

            return false;
        }


        public void IterateSectors(Action<TileSector> action)
        {
            for (int x = 0; x < SectorsAcrossWidth; x++)
            {
                for (int y = 0; y < SectorsAcrossHeight; y++)
                {
                    TileSector sector = Sectors[x][y];
                    if (sector != null)
                    {
                        action(sector);
                    }
                }
            }

        }

        public void ClearMaps()
        {
            IterateSectors(s => s.ClearMap());
        }

        public void SwitchBuffers()
        {
            IterateSectors(s => s.SwitchBuffers());
        }

        protected TileSector GetOrCreateSector(int x, int y, out ushort sectorX, out ushort sectorY, out ushort relativeX, out ushort relativeY)
        {
            GetSectorAndRelativeCoords(x, y, out sectorX, out sectorY, out relativeX, out relativeY);

            TileSector sector = GetOrCreateSector(sectorX, sectorY);

            return sector;

        }

        public TileSector GetOrCreateSector(int sectorX, int sectorY)
        {
            TileSector sector = Sectors[sectorX][sectorY];
            if (sector == null)
            {
                sector = CreateSector(sectorX, sectorY);
            }

            return sector;
        }

        private TileSector CreateSector(int sectorX, int sectorY)
        {
            TileSector sector = new TileSector(this, sectorX, sectorY);
            Sectors[sectorX][sectorY] = sector;
            return sector;
        }

        
        public void GetDirtySectors(List<TileSector> listToAddTo)
        {
            IterateSectors(s =>
                {
                    if (s.IsDirty)
                    {
                        listToAddTo.Add(s);
                    }
                });

        }

        public void GetAllSectors(ref List<Point> listToAddTo)
        {
            List<Point> placeHolderListToAddTo = listToAddTo; // not allowed to use ref param in lambda...
            IterateSectors(s =>
            {
                Common.AddToList(ref placeHolderListToAddTo, s.Coords);                
            });

            listToAddTo = placeHolderListToAddTo;
        }

        public HashSet<Point> GetAllSectors()
        {
            HashSet<Point> set = new HashSet<Point>();
            IterateSectors(s =>
            {
                Common.AddToList(ref set, s.Coords);
            });

            return set;
        }

        public void GetDirtySectors(ref HashSet<Point> listToAddTo)
        {
            HashSet<Point> placeHolderListToAddTo = listToAddTo; // not allowed to use ref param in lambda...
            IterateSectors(s =>
            {
                if (s.IsDirty)
                {
                    Common.AddToList(ref placeHolderListToAddTo /* listToAddTo*/, s.Coords);
                }
            });

            listToAddTo = placeHolderListToAddTo;
        }

        public void GetDirtySectors(ref List<Point> listToAddTo)
        {
            List<Point> placeHolderListToAddTo = listToAddTo; // not allowed to use ref param in lambda...
            IterateSectors(s =>
            {
                if (s.IsDirty)
                {
                    Common.AddToList(ref placeHolderListToAddTo, s.Coords);
                }
            });

            listToAddTo = placeHolderListToAddTo;
        }

        public byte GetValue(Point pos)
        {
            TileSector sector;
            return GetValue(pos.X, pos.Y, out sector);
        }

        public byte GetValue(int x, int y)
        {
            TileSector sector;
            return GetValue(x, y, out sector);
        }

        public TileSector GetSector(int sectorX, int sectorY)
        {
            return Sectors[sectorX][sectorY];
        }

        public byte GetValue(int x, int y, out TileSector sector)
        {
            ushort relativeX, relativeY;
            ushort sectorX, sectorY;

            GetSectorAndRelativeCoords(x, y, out sectorX, out sectorY, out relativeX, out relativeY);

            sector = Sectors[sectorX][sectorY];
            if (sector != null)
            {
                return sector.GetValue(relativeX, relativeY);
            }
            else return defaultValue;

            //return null;
        }

        public bool GetIsBlocked(Point tilePos)
        {
            return GetIsBlocked(tilePos.X, tilePos.Y);
        }

        public bool GetIsBlocked(int x, int y)
        {
            ushort relativeX, relativeY;
            ushort sectorX, sectorY;

            GetSectorAndRelativeCoords(x, y, out sectorX, out sectorY, out relativeX, out relativeY);

            TileSector sector = Sectors[sectorX][sectorY];
            if (sector != null)
            {
                return sector.GetIsBlocked(relativeX, relativeY);
            }
            else return false;

        }

        /// <summary>
        /// clears the previously drawn sector coords, stores them, in preparation for drawing
        /// </summary>
        public void BeginDrawing()
        {
            StoreSectorsBeforeClear();

            SwitchBuffers();
        }

        public void StoreSectorsBeforeClear()
        {
            PreviouslyAffectedSectors.Clear();

            foreach (var sector in AffectedSectors) // save these before clearing...
            {
                PreviouslyAffectedSectors.Add(sector);
            }

            AffectedSectors.Clear();
        }

       

        public HashSet<Point> GetSectorsThatHaveChanged(HashSet<Point> sectorsToCheck)
        {
            HashSet<Point> sectorsThatHaveChanged = new HashSet<Point>();

            foreach (Point sectorCoords in sectorsToCheck) // check them all...
            {
                TileSector sector = GetSector(sectorCoords.X, sectorCoords.Y);
                if (sector != null)
                {
                    //we must compare the old and the new version to see if anything changed...
                    if (sector.SectorHasChanged())
                    {
                        Common.AddToList(ref sectorsThatHaveChanged, sector.Coords);
                        /*
                        sector.IsDirty = true;
                        sector.SectorIsClear = false;*/
                    }

                }
            }

            return sectorsThatHaveChanged;

        }


        public void CompareOldAndNewSectors()
        {
            // find out if sectors have changed:

            // add the sectors that we touched last time, but didn't this time.
            HashSet<Point> currentAndPreviousAffectedSectors = new HashSet<Point>();
            currentAndPreviousAffectedSectors.UnionWith(PreviouslyAffectedSectors);
            currentAndPreviousAffectedSectors.UnionWith(AffectedSectors);

            //previouslyAffectedSectors.UnionWith(affectedSectors);

            HashSet<Point> sectorsThatHaveChanged = GetSectorsThatHaveChanged(currentAndPreviousAffectedSectors);// check them all...


            foreach (Point sectorCoords in sectorsThatHaveChanged)
            {
                TileSector sector = GetSector(sectorCoords.X, sectorCoords.Y);
                if (sector != null)
                {
                    sector.IsDirty = true;                   
                }
            }
        }



        public void RemoveSectors(HashSet<Point> sectorsToRemove)
        {
            // remove sectors that are empty:
            foreach (var item in sectorsToRemove)
            {
                Sectors[item.X][item.Y] = null; // release memory.
            }

            /*
            for (int x = 0; x < SectorsAcrossWidth; x++)
            {
                for (int y = 0; y < SectorsAcrossHeight; y++)
                {
                    TileSector sector = Sectors[x][y];
                    if (sector != null)
                    {
                        if (!sectorsToRemove.Contains(sector.Coords))
                        {
                            Sectors[x][y] = null; // release memory.
                        }
                    }
                }
            }*/
        }

        /// <summary>
        /// draws a circle with the specified fall-off or radius per tile.
        /// radius = 0 is a point. radius = 1 is a point with a 'cross' of half the center value.
        /// 
        /// also adds to affected sectors collection.
        /// </summary>
        /// <param name="map"></param>
        /// <param name="pos"></param>
        /// <param name="centerValue"></param>
        /// <param name="addToExistingValues"></param>
        public void DrawLinearInfluenceCircle(Point pos, int centerValue, InfluenceMap.Operation operation, InfluenceMap.Falloff falloffYesNo,
            InfluenceMap.CircleParameter circleParam, int paramValue, int? maxRadius = null)
        {
            bool isDrawingAPositiveCircle;
            double falloffEachTile;
            int minX;
            int minY;
            int maxX;
            int maxY;

            /* int width = Common.GetJaggedArrayWidth(map);
             int height = Common.GetJaggedArrayHeight(map);
             */

            pos = InfluenceMap.ComputeLinearCircle(Width, Height, pos, centerValue, falloffYesNo, circleParam, paramValue, maxRadius, out isDrawingAPositiveCircle, out falloffEachTile, out minX, out minY, out maxX, out maxY);

            float dist;
            double result = centerValue;

            TileSector changedSector;

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    if (falloffYesNo == InfluenceMap.Falloff.Yes)
                    {
                        dist = Common.DistanceOctile(new Point(x, y), pos);
                        result = centerValue + (dist * falloffEachTile);
                    }

                    if (!isDrawingAPositiveCircle)
                    {
                        result = Common.ClampTop(result, 0);
                    }
                    else
                    {
                        result = Common.ClampBottom(result, 0);
                    }

                    changedSector = null;

                   // byte operationValue = (byte)Common.Clamp(result, 0, 255);                   

                    SetValue(x, y, result/* operationValue*/, operation, 
                        out changedSector);                     
                       

                    if (changedSector != null)
                    {
                        AffectedSectors.Add(changedSector.Coords);
                    }
                }
            }

            // return list of affected sectors:
           /* int minSectorX = minX / SectorSize;
            int maxSectorX = maxX / SectorSize;
            int minSectorY = minY / SectorSize;
            int maxSectorY = maxY / SectorSize;

            for (int x = minSectorX; x <= maxSectorX; x++)
            {
                for (int y = minSectorY; y <= maxSectorY; y++)
                {
                    AffectedSectors.Add(new Point(x, y)); // new Point(x, y));
                }
            }*/

        }

        

        #region ISnapshot


        Snapshotter.Version version;
        public virtual Snapshotter.Version DoVersion(Snapshotter sn)
        {
            base.DoVersion(sn); // each class in the class hierarchy snapshots and maintains their own version.

            version = sn.DoVersion(Snapshotter.Version.Original);
            return version;
        }

        public bool IsSnapshotted { get; set; }

        public override ISnapshot DoSnapshot(Snapshotter sn)
        {
            base.DoSnapshot(sn);

            Sectors = sn.DoJaggedArray(Sectors);
            this.BlockingLimit = sn.DoByteNullable(BlockingLimit);
            this.AffectedSectors = sn.DoHashSet(AffectedSectors);
            this.PreviouslyAffectedSectors = sn.DoHashSet(PreviouslyAffectedSectors);
         

            return this;
        }


        public override void LoadPostProcess(Snapshotter sn)
        {
            base.LoadPostProcess(sn);

            sn.RegisterLoadPostProcessCall(this);


            IterateSectors(s => s.LoadPostProcess(sn));
        }

        #endregion
    }
}
