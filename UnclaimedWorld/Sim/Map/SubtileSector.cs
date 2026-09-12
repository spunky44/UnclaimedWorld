using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.AI.Pathfinding;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Maps
{
    /// <summary>
    /// movemap and terrain map are built up of square arrays of these
    /// 
    /// a note on size:
    /// The LOH is used for allocating memory for large objects (such as arrays) that require more than 85,000 bytes.
    /// see if we can stay below this limit... 291 * 291 subtiles
    /// </summary>
    public class SubtileSector : ISnapshot 
    {
        // LOG: FOR DEBUG ONLY
        public List<Tuple<double, string>> Log = new List<Tuple<double, string>>();

        /// <summary>
        /// debug only..
        /// </summary>
        public double? CreatedOn;

        public Rectangle SubtileArea;

        /// <summary>
        /// sector coords
        /// </summary>
        public Point Coords
        {
            get;
            private set;
        }

        /// <summary>
        /// Edge sectors will have uneven size...
        /// </summary>
        public MapManager.SubtileValue[][] Values;

        private bool blockedStatusHasChanged = true; // true to start with

        /// <summary>     
        /// this should trigger redrawing of regions.
        /// </summary>
        public bool BlockedStatusHasChanged
        {
            get
            {
                return blockedStatusHasChanged;
            }

            set
            {
                if (blockedStatusHasChanged != value)
                {
                    blockedStatusHasChanged = value;
                    AddLog("Blocked status set to: " + value);
                }
            }
        }


        #region Regions

        /// <summary>
        /// the sector should be ignored in searches until it has been fully computed once (FinishSector gets called)
        /// this is similar to how newRegions are ignored by searches until fully computed.
        /// 
        /// this takes care of the ADD case in ADD UPDATE DELETE for sectors.
        /// </summary>
        public bool HasFinishedFirstRun
        {
            get;
            private set;
        }

        public const int RegionRadius = 8;
        public const int RegionInterval = 16;

        /// <summary>
        /// moved from RegionMap
        /// the coordinate map that tells what region(color!) a subtile belongs to
        /// </summary>
        private ushort[][] regionsOnSubtiles;

        // 
        /// <summary>
        /// only used during flood fill operation.
        /// </summary>
        private byte[][] allNodeDistances;


        private List<SubtilePos> listOfUnassignedPoints = new List<SubtilePos>(200);

        private HashSet<ushort> regions = new HashSet<ushort>();

        #region progress variables and collections
        // *******************


        /// <summary>
        /// contains region id for each subtile
        /// </summary>
        private ushort[][] newRegionsOnSubtiles;
        private HashSet<ushort> newRegions = new HashSet<ushort>();


        private Point currentCenterRelative = new Point(-RegionRadius, RegionRadius);

        #endregion  //*****************
        #endregion

        public SubtileSector()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor");
        }

        public SubtileSector(SubtileLayer parent, int sectorCoordsX, int sectorCoordsY, bool setHasFinished = false) //, int mapWidth, int mapHeight)
        {
            int sectorSize = parent.SectorSize;

            HasFinishedFirstRun = setHasFinished;

            Coords = new Point(sectorCoordsX, sectorCoordsY);

            SubtileArea = new Rectangle();
            SubtileArea.X = sectorCoordsX * sectorSize;
            SubtileArea.Y = sectorCoordsY * sectorSize;

            SubtileArea.Width = Common.ClampTop(sectorSize, sectorSize - (SubtileArea.X + sectorSize - parent.Width));
            SubtileArea.Height = Common.ClampTop(sectorSize, sectorSize - (SubtileArea.Y + sectorSize - parent.Height));

            // cut off edge sectors: 
            Common.InitJaggedArray(ref Values, SubtileArea.Width, SubtileArea.Height);
            Common.InitJaggedArray(ref regionsOnSubtiles, SubtileArea.Width, SubtileArea.Height);
           
           /* Common.InitJaggedArray(ref Values, sectorSize, sectorSize);
            Common.InitJaggedArray(ref AllSubtiles, sectorSize, sectorSize);
            */


            Init();

            CreatedOn = The.Sim.TotalUnPausedGameTimeInSeconds;
            AddLog("Created");

        }

        public void AddLog(string text)
        {
#if DEBUG || PROFILE

            Log.Add(new Tuple<double, string>(The.Sim.TotalUnPausedGameTimeInSeconds, text));
#endif
        }


       /* public void Destroy(RegionMap regionMap)
        {
            // destroy/remove all regions:
            foreach (var item in regions)
            {
                regionMap.RemoveRegion(item);                
            }
        }*/

        public HashSet<ushort> GetRegions()
        {
            return regions;
        }

        public HashSet<ushort> GetRegionsInProgress()
        {
            return newRegions;
        }

        #region Region Map methods

        public void InitCopyVariablesSubtiles()
        {          
            Common.CopyJaggedArray(regionsOnSubtiles, newRegionsOnSubtiles);

            //newRegions = new HashSet<ushort>(regions);
            newRegions.Clear();
            foreach (var item in regions)
            {
                newRegions.Add(item);
            }

        }

        public void InitCopyVariablesClearDistances()
        {
            Common.ClearJaggedArray(allNodeDistances);
        }

        public void FinishRegions()
        {
            // it should be sufficient to copy once, when starting...
            Common.CopyJaggedArray(newRegionsOnSubtiles, regionsOnSubtiles);

          //  AllSubtiles = newAllSubtiles;
          
            regions.Clear();
            foreach (var item in newRegions)
            {
                regions.Add(item);
            }

            if (HasFinishedFirstRun == false)
            {
                HasFinishedFirstRun = true; // now we are ready
            }

            AddLog("FinishRegions");

            //AllSubtiles 
           // Common.CopyJaggedArray(AllSubtiles, newAllSubtiles);
        }

        public ushort GetRegion(Point subtile)
        {
            return GetRegion(subtile.X, subtile.Y);
        }

        public ushort GetRegion(int relativeX, int relativeY)
        {
            return regionsOnSubtiles[relativeX][relativeY];
        }

        public ushort GetRegionInProgress(Point subtile)
        {
            return GetRegionInProgress(subtile.X, subtile.Y);
        }

        public ushort GetRegionInProgress(int relativeX, int relativeY)
        {
            return newRegionsOnSubtiles[relativeX][relativeY];
        }

        public void ClearRegions(List<ushort> clearedRegions)
        {
            ushort subtileValue;
           // Region region;


            Common.ClearJaggedArray(newRegionsOnSubtiles); // mark as 0 - 'no region'

            /*  for (int x = SubtileArea.Left; x <= SubtileArea.Right; x++)
            {
                for (int y = SubtileArea.Top; y <= SubtileArea.Bottom; y++)
                {

                    subtileValue = newAllSubtiles[x][y];
                    if (subtileValue != 0)
                    {
                      region = GetRegion(subtileValue);

                        if (region != null
                         && region.CenterInSubtiles.X == x // only do this when we hit the center point
                         && region.CenterInSubtiles.Y == y)
                        {
                            regionsToRemove.Add(region);
                        }

                        newAllSubtiles[x][y] = 0; // mark as 'no region'
                    }
                }
            }*/

            clearedRegions.AddRange(newRegions);

            newRegions.Clear();

            // moved from RegionMap:
            currentCenterRelative = new Point(-RegionRadius, RegionRadius); // the procedure to find the next center starts by adding regionInterval = 2 * regionRadius

        }

        /// <summary>
        /// returns true when the sector is done
        /// </summary>
        /// <returns></returns>
        public bool FloodFill(RegionMap parent)
        {           
            if (!ScanRectangleWithIntervals()) // GetNextCenter())
            {
                if (Coords.X == 0 && Coords.Y == 1)
                {

                }

                // no more regularly spaced centers...
                // test the map for holes:
                if (!GetUnassignedPoint()) // scan whole sector in one cycle...
                {
                    // flood fill is done. We have all the regions. 

                    // all done.
                    return true;
                }
            }

            
            // else create a new region by flood filling out from the center:
            CreateRegionUsingFloodfill(parent);

            return false;

        }

        /// <summary>
        /// build the region graph connecting all internal regions
        /// </summary>
        /// <returns></returns>
        public void BuildRegionGraph(RegionMap regionMap)
        {
            int minX = 0;
            int minY = 0;

            int maxX = SubtileArea.Width - 1;
            int maxY = SubtileArea.Height - 1;

           
            for (int x = minX; x < maxX; x++)
            {
                for (int y = minY; y < maxY; y++)
                {
                   /* if (this.SubtileArea.X == 48 && this.SubtileArea.Y == 144)
                    {
                        if (x == maxX - 1)
                        {

                        }
                    } */

                    ExamineQuadForEdges(regionMap, x, y, x + 1, y + 1);
                }
            }
        }

        /// <summary>
        /// finds internal edges inside a sector
        /// </summary>
        /// <param name="fromX"></param>
        /// <param name="fromY"></param>
        /// <param name="toX"></param>
        /// <param name="toY"></param>
        private void ExamineQuadForEdges(RegionMap regionMap, int fromX, int fromY, int toX, int toY) // ushort from, ushort to)
        {
         /*   if (this.SubtileArea.X == 48 && this.SubtileArea.Y == 144)
            {
                if (fromX == 46 && toX == 47 && fromY == 13 && toY == 14)
                {

                }
            }*/

            ushort[] fromXColumn = newRegionsOnSubtiles[fromX];
            ushort topLeftRegion = fromXColumn[fromY]; // top left of subtile quad
            //  ushort toRegion = allNodes[toX, toY];

            

            ushort[] toXColumn = newRegionsOnSubtiles[toX];
            ushort topRightRegion = toXColumn[fromY]; // top right of quad   
            regionMap.AddEdgesIfNotExists(topLeftRegion, topRightRegion);
               

            ushort bottomRightRegion = toXColumn[toY];
            regionMap.AddEdgesIfNotExists(topLeftRegion, bottomRightRegion);
               

            ushort bottomLeftRegion = fromXColumn[toY];
            regionMap.AddEdgesIfNotExists(topLeftRegion, bottomLeftRegion);

            regionMap.AddEdgesIfNotExists(bottomLeftRegion, topRightRegion);                  

            
        }


        private void CreateRegionUsingFloodfill(RegionMap parent)
        {
            Region region = parent.CreateRegion(new Point(SubtileArea.X + currentCenterRelative.X, SubtileArea.Y + currentCenterRelative.Y));

            parent.FloodFill.DoFloodFill(Values, newRegionsOnSubtiles, allNodeDistances, currentCenterRelative, region.Color, RegionRadius + 1);

            newRegions.Add(region.Color);

           // CreateRegionUsingFloodfill(parent.floodFill, allNodeDistances, currentCenter, region.Color, regionRadius + 1);
            //floodFill.DoFloodFill(layers, newAllSubtiles, allNodeDistances, currentCenter, region.Color, regionRadius + 1);
        }


      /*  private bool GetNextCenter()
        {
            previousCenter = currentCenter; // debug only

            if (rectanglesToClearAndRedraw.Count > 0)
            {
                //  int rectangleIndex = 0;
                Rectangle scanRectangle;

                do
                {
                    if (cycleScanRectangleIndex == rectanglesToClearAndRedraw.Count)
                    {
                        return false;
                    }

                    scanRectangle = rectanglesToClearAndRedraw[cycleScanRectangleIndex];

                    if (!ScanRectangleWithIntervals(scanRectangle)) // failed to find next center in the rect - go to next rect
                    {
                        cycleScanRectangleIndex++;

                        if (cycleScanRectangleIndex == rectanglesToClearAndRedraw.Count)
                        {
                            return false;
                        }
                    }
                    else //- we found a center. stay in the same rectangle next time.
                    {
                        return true;
                    }
                }
                while (true);

            }
            else
            {
                // upper range on no of cycles???
                do
                {
                    currentCenter.X += regionInterval;
                    if (currentCenter.X >= subtileMapWidth)
                    {
                        currentCenter.X = regionRadius;

                        currentCenter.Y += regionInterval;

                        if (currentCenter.Y >= subtileMapHeight)
                        {
                            return false;
                        }
                    }
                } // skip an interval if this tile is already colored, or if it is a blocked tile:
                while (newAllSubtiles[currentCenter.X][currentCenter.Y] > 0 ||
                    MapManager.IsBlocked(layers[currentCenter.X][currentCenter.Y]));

                return true;
            }

        }*/

        private bool ScanRectangleWithIntervals() //Rectangle r)
        {
            Rectangle r = SubtileArea;

          /*  if (currentCenterRelative.X >= r.Left && currentCenterRelative.X <= r.Right
                && currentCenterRelative.Y >= r.Top && currentCenterRelative.Y <= r.Bottom)*/
            if (currentCenterRelative.X >= 0 && currentCenterRelative.X < r.Width
                && currentCenterRelative.Y >= 0 && currentCenterRelative.Y < r.Height)
            {
                // continue looking in this rectangle...
                //currentCenter.X += regionInterval;
            }
            else
            {
                // find a starting point for the scan
             /*   if (r.Width > regionRadius)
                {
                  //  currentCenterRelative.X = r.Left + regionRadius;
                    currentCenterRelative.X = regionRadius;
                }
                else
                {
                    currentCenterRelative.X = r.Width / 2;
                }*/

                currentCenterRelative.X = GetStartXPosForScan();

                currentCenterRelative.X -= RegionInterval; // compensate for loop increment...

                if (r.Height > RegionRadius)
                {
                   // currentCenterRelative.Y = r.Top + regionRadius;
                    currentCenterRelative.Y = RegionRadius;
                }
                else
                {
                    currentCenterRelative.Y = /*r.Top +*/ r.Height / 2;
                }
            }

            do
            {
                currentCenterRelative.X += RegionInterval;
               
              //  if (currentCenterRelative.X > r.Right) // boundaries are inclusive...
                if (currentCenterRelative.X >= r.Width) 
                {
                    //currentCenterRelative.X = r.Left + regionRadius;
                   // currentCenterRelative.X = regionRadius;
                    currentCenterRelative.X = GetStartXPosForScan();

                    currentCenterRelative.Y += RegionInterval;

                   
                  //  if (currentCenterRelative.Y > r.Bottom)
                    if (currentCenterRelative.Y >= r.Height)
                    {
                        return false;
                    }
                }

            } // skip an interval if this tile is already colored, or if it is a blocked tile:
            while (newRegionsOnSubtiles[currentCenterRelative.X][currentCenterRelative.Y] > 0 ||
                    MapManager.IsBlocked(Values[currentCenterRelative.X][currentCenterRelative.Y]));

            return true;
        }

        private int GetStartXPosForScan()
        {
            if (SubtileArea.Width > RegionRadius)
            {
                //  currentCenterRelative.X = r.Left + regionRadius;
                //currentCenterRelative.X = 
                    return RegionRadius;
            }
            else
            {
                //currentCenterRelative.X =
                return SubtileArea.Width / 2;
            }
        }

        private bool GetUnassignedPoint()
        {
            // brute force: get a point that has not been assigned a region yet:
            if (listOfUnassignedPoints.Count == 0)
            {
                //listOfDirtyPointsOutsideRegions.Clear();

                ScanAreaForUnassignedSubtiles(); //new Rectangle(0, 0, subtileMapWidth - 1, subtileMapHeight - 1));

                if (listOfUnassignedPoints.Count > 0)
                {
                    currentCenterRelative = listOfUnassignedPoints.ElementAt(0).ToPoint(); //[0]; // ???
                    return true;
                }
                else return false; // we are done with flood filling the map.
            }
            else
            {
                return ExamineUnassignedPoints();
            }

        }

        private bool ExamineUnassignedPoints()
        {
            SubtilePos pointToCheck;

            int countAtStart = listOfUnassignedPoints.Count;
            int i = countAtStart - 1;
            for (; i >= 0; i--) //iterate in reverse (unneccessary, now!)
            {
                pointToCheck = listOfUnassignedPoints.ElementAt(i);

                if (newRegionsOnSubtiles[pointToCheck.X][pointToCheck.Y] == 0)
                {
                    currentCenterRelative = pointToCheck.ToPoint();
                    listOfUnassignedPoints.RemoveRange(i, countAtStart - i);
                    //remove all the unqualified points at once
                    return true;
                }
            }

            listOfUnassignedPoints.Clear();

            return false; // no more unassigned points. we are done with flood filling the map.
        }


        private void ScanAreaForUnassignedSubtiles() //Rectangle area)
        {
            ushort[] newAllNodesColumn;
            MapManager.SubtileValue[] moveCostsColumn;
           // for (int x = SubtileArea.Left; x < SubtileArea.Right; x++)
            for (int x = 0; x < SubtileArea.Width; x++)
            {
                newAllNodesColumn = newRegionsOnSubtiles[x];
                moveCostsColumn = Values[x];

               // for (int y = SubtileArea.Top; y < SubtileArea.Bottom; y++)
                for (int y = 0; y < SubtileArea.Height; y++)
                {
                    if (newAllNodesColumn[y] == 0 &&
                        !MapManager.IsBlocked(moveCostsColumn[y]))
                    {
                        listOfUnassignedPoints.Add(new SubtilePos((ushort)x, (ushort)y));
                    }
                }
            }
        }

        public void CreateRegionUsingFloodfill(FloodFill floodFill, byte[][] allNodeDistances, Point from, ushort regionColor, int radius)
        {
            
           
        }

      
        #endregion

        private void Init()
        {
            // init fields that are not snapshotted here

           /* floodFill = new FloodFill();*/

            Common.InitJaggedArray(ref newRegionsOnSubtiles, SubtileArea.Width, SubtileArea.Height);
            Common.InitJaggedArray(ref allNodeDistances, SubtileArea.Width, SubtileArea.Height);
      
         /*   Common.InitJaggedArray(ref allNodeDistances, subtileMapWidth, subtileMapHeight);

            CreateRegulators();*/
        }

        public MapManager.SubtileValue? GetValue(int relativeX, int relativeY)
        {
            if (relativeX < SubtileArea.Width // Common.GetJaggedArrayWidth(Values)
                && relativeY < SubtileArea.Height) // Common.GetJaggedArrayHeight(Values))
            {
                return Values[relativeX][relativeY];
            }
            else return null;

            // .Values[relativeX][relativeY];
        }

        /*
        /// <summary>
        /// should the value override or add to base layer???
        /// add in pathfinder or before?
        /// 
        /// Before: store subtiles here. saves divide and add ops in the pathfinder... uses existing update code
        /// 
        /// zero should mean no added cost
        /// </summary>
        protected byte[][] values;

        /// <summary>
        /// pack into values instead? Perhpas later create an enum like SubtileValue, but only with a blocked flag and a cost (0 - 128)?
        /// </summary>
        protected bool[][] isBlocked;
        */


        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            /*id = SnapshotID(sn, id);
            IDCounter = (SubtileLayerID)sn.DoEnum(IDCounter);
            */
            this.blockedStatusHasChanged = sn.DoBool(blockedStatusHasChanged);
            this.Values = sn.DoJaggedArray(Values);
            this.Coords = sn.DoPoint(Coords);
            this.SubtileArea = sn.DoRectangle(SubtileArea);
            this.regionsOnSubtiles = sn.DoJaggedArray(regionsOnSubtiles);
            this.regions = sn.DoHashSet(regions);
            this.HasFinishedFirstRun = sn.DoBool(HasFinishedFirstRun);

            sn.Ignore(listOfUnassignedPoints); // start repair over after load
            sn.Ignore(newRegionsOnSubtiles);
            sn.Ignore(newRegions);
            sn.Ignore(allNodeDistances);
            sn.Ignore(currentCenterRelative);
            sn.Ignore(CreatedOn);
            sn.Ignore(Log);

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

            
            Init();


            // make sure that newSubtiles is same or newer than allsubtiles:
            Common.CopyJaggedArray(regionsOnSubtiles, newRegionsOnSubtiles);
            newRegions = new HashSet<ushort>(regions);
        }

        #endregion

        
    }
}
