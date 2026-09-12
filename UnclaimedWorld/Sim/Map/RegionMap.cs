using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI.Pathfinding;
using UWGame.SimSide.Entities;
using System.Diagnostics;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Systems.TimeSlicing;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Items;
using UWGame.SimSide.Maps.Regions;

namespace UWGame.SimSide.Maps
{
    
    /// <summary>
    /// The terrain region map is a fully computed map.
    /// 
    /// The movement region maps are built from a movement map, consisting of a full terrain layer and an incomplete overriding layer.
    /// The region map mirrors this structure. regions are computed for the overriding sectors and kept as a layer. The terrain region layer is shared between region maps.
    /// Each region map has a full graph and costs table. 
    /// 
    /// when the bottom layer regions are redrawn, all the dependent region maps must redraw their graphs before they can be used. This means restarting their ongoing process...
    /// </summary>
    public abstract class RegionMap : ISnapshot, ICyclable
    {
        // LOG: FOR DEBUG ONLY
        public List<Tuple<double, string>> Log = new List<Tuple<double, string>>();

        
        public string IDName;

        public const ushort BaseLayerRegionColors = 40000;
       
      //  private Point previousCenter;

        public double StartedOnTimeInSeconds { get; set; }

        protected int currentSectorIndex;
        //private Point currentSectorCoords;

        public static double totalComputationAllInstancesInSeconds;
        public double TotalComputationAllInstancesInSeconds
        {
            get
            {
                return totalComputationAllInstancesInSeconds;
            }
            set
            {
                totalComputationAllInstancesInSeconds = value;
            }
        }

        public double ComputationTimeSpentInSeconds { get; set; }

        public double? UpdateInterval
        {
            get
            {
                return null;
            }
        }

        /// <summary>        
        /// the next region color that is available. (zero is 'no region' on the map.)
        /// unused region colors get re-used.
        /// </summary>
        private ushort regionColorCounter = 1;

        /// <summary>      
        /// The terrain cost array that this region map is a high-level representation of. 
        /// This can either be the raw terrain cost array from MapManager, or it can be the movement map arrays which include threats.
        /// </summary>
        protected SubtileLayers layers;
        private SubtileLayersID snapshotLayers;

        /// <summary>
        /// this is the layer we are keeping up to date with regions and connectors to lower layers
        /// </summary>
        protected SubtileLayer targetLayer;
        private SubtileLayerID snapshotTargetLayer;

        /// <summary>
        /// set true when the regions require refilling in one or more sectors
        /// NEW: also set to true if sectors have been removed by MovementMap
        /// </summary>
        protected bool sectorsAreDirty;

        protected abstract ushort ColorStartOfRange
        {
            get;
        }

        public abstract Dictionary<ushort, Dictionary<ushort, RegionEdge>> BottomRegionGraph
        {
            get;
        }

     /*   public abstract Dictionary<ushort, Dictionary<ushort, RegionEdge>> BottomToUpperLayerConnectors // connections from the bottom graph to the overriding sectors.
        {
            get;
        }*/

        public abstract Dictionary<ushort, HashSet<ushort>> BottomBlockedEdges
        {
            get;
        }


        public abstract Dictionary<ushort, Region> BottomRegions
        {
            get;
        }

        public void AssertSearch(RegionSearchPlanner planner, RegionSearchRequest searchRequest)
        {
            /*Searches are allowed to progress on the old Regions and RegionGraph, while new data is computed.
             * The same assert is done on search complete.
             * 
             *  Assert(regions.ContainsKey(regionSearchPlanner.Start) || BottomRegions.ContainsKey(regionSearchPlanner.Start), "unknown region?");
             * before:
             * AllRegionCosts.Add(regionSearchPlanner.Start, regionSearchPlanner.GetBFSResult());

               the client will look up the result from the position, and key into the AllRegionCosts collection. 
             * therefore the result is useless if the start region has changed.
             * 
             * 
             * update sector: ok, we still have the old data
             * add sector: ok, just ignore sectors that have not finished a first run
             * remove sector: error, both the new and old subtiles will be gone. therefore the assert fails.
             */
            // assert that the regions/sectors don't change while we are searching:
            if (planner.Start != layers.GetRegion(searchRequest.FromSubtile.X, searchRequest.FromSubtile.Y))
            {
                System.Diagnostics.Debug.Assert(false, "regions changed during search!"); // sector was removed?
            }

            System.Diagnostics.Debug.Assert(regions.ContainsKey(planner.Start) || BottomRegions.ContainsKey(planner.Start), "unknown region?");
        }

        /*
        // 
        /// <summary>
        /// only used during flood fill operation.
        /// </summary>
        public byte[][] AllNodeDistances;
    */

        int subtileMapWidth, subtileMapHeight;

       
        //*********       
        // data available!
        // protected. Clients should call GetRegion
        //
        protected Dictionary<ushort, Region> regions = new Dictionary<ushort, Region>();
     
       
       
        protected List<Point> dirtySectors = new List<Point>();

        private List<Point> dirtySectorsRollbackAfterSave;


        /// <summary>
        /// used to optimize sector edge region scanning
        /// </summary>
        private HashSet<Point> scannedSectors = new HashSet<Point>();



        /// <summary>
        /// keep a unique graph for every region map
        /// if the dependent map contains no sectors, we can default all searches to the terrain map instead!
        /// 
        /// the dictionary of edges leading out from a region of the specified color.
        /// the dictionary in the value part has as its key the region the edge is going to!
        /// NOTE that if the region is an island it will not have an entry in this collection!
        /// </summary>
        public Dictionary<ushort, Dictionary<ushort, RegionEdge>> RegionGraph = new Dictionary<ushort, Dictionary<ushort, RegionEdge>>();

        /// <summary>
        /// also unique for every region map
        /// gives the distance from each region to any other region - -1 for no access!
        /// store the node instead of its cost (G) because it seems faster.... though it does take up some more memory.
        /// 
        /// Note: This is not the distance that we give to clients! The real distance is modulated by the precise end point locations!
        /// </summary>
        public Dictionary<ushort, Dictionary<ushort, RegionPathFinderNodeBFS>> AllRegionCosts;

        /// <summary>
        /// some paths are stored here... give them to the pathfinder to speed up search.
        /// </summary>
        public Dictionary<ushort, Dictionary<ushort, RegionPathID>> RegionPaths; 


        /// <summary>
        /// clear this whenever the bottom or dependent map changes!
        /// </summary>
        protected Dictionary<Point, Tuple<ushort, double, int>> cachedClosestRegionToBlockedSubtile;
     
       

        #region progress variables and collections
        // *******************
        /// <summary>
        /// the data we are working on! don't look up in these!
        /// </summary>
        protected Dictionary<ushort, Region> newRegions = new Dictionary<ushort, Region>();
     
   

        public Dictionary<ushort, Dictionary<ushort, RegionEdge>> newRegionGraph = new Dictionary<ushort, Dictionary<ushort, RegionEdge>>();
        private List<ushort> regionGraphKeyListForCopying;
        private int regionGraphCopyProgressIndex;

      
        //********************
        #endregion



        protected enum Progress { InitFromScratch = 0, InitCleanupUnneededSectors = 1,
                                    InitCopyVariablesSubtiles = 2, InitCopyVariablesRegions = 3, 
                                    InitCopyVariablesRegionGraph = 4, InitCopyVariablesClearDistances = 5,        
                                    InitClearDirtySectors = 6,         
                                    InitRemoveRegionsAndLinks = 7, 
                                    FillRegions = 8, RemoveEdgesBetweenLayers = 9, 
                                    BuildInternalRegionGraph = 10, BuildRegionGraphBetweenSectors = 11 }

        protected Progress progress = Progress.InitFromScratch;

        protected bool isComputing;

        public FloodFill FloodFill;
        


        // client requests       
        /// <summary>
        /// dict of RegionSearchPlanners
        /// a planner groups several requests from the same region.
        /// When the region map is redrawn, these are all cancelled. The planners are then created again (in StartPendingSearchRequests) from the list of searches...
        /// </summary>
        private Dictionary<ushort, CyclableID> regionBFSPlanners = new Dictionary<ushort, CyclableID>();

        private Dictionary<ushort, Dictionary<ushort, CyclableID>> regionAStarPlanners = new Dictionary<ushort, Dictionary<ushort, CyclableID>>();
     

        /// <summary>
        /// it is necessary to store the searches here as well as in the planner...
        /// they get stored here if they come in before the region map is ready.
        /// </summary>
        private List<RegionSearchRequestID> regionSearchRequests = new List<RegionSearchRequestID>();
     
        public bool IsPaused { get; set; }


        public RegionMap()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor");
           
        }

        public RegionMap(SubtileLayers layers, SubtileLayer targetLayer) 
        {
           // manager.RegionMaps.Add(this);
           
            AddToLookup();
                      
            this.layers = layers;
            this.targetLayer = targetLayer; // layers.Layers[0];

            subtileMapWidth = The.Map.mapSubtileWidth; // Common.GetJaggedArrayWidth(this.layers);
            subtileMapHeight = The.Map.mapSubtileHeight; // Common.GetJaggedArrayHeight(this.layers);


            AllRegionCosts = new Dictionary<ushort, Dictionary<ushort, RegionPathFinderNodeBFS>>();
            RegionPaths = new Dictionary<ushort, Dictionary<ushort, RegionPathID>>(); // RegionPathFinderNodeAStar>>>();

            //OutOfBandCosts = new Dictionary<ushort, Dictionary<ushort, float>>();

            cachedClosestRegionToBlockedSubtile = new Dictionary<Point, Tuple<ushort, double, int>>(); // new Dictionary<Point, ushort>();
            

            Init();

           // InitVariablesForRecalc();

            regionColorCounter = ColorStartOfRange; 

            timer = new HighResolutionTime();
        }


        public virtual void Destroy()
        {
            if (The.Sim.CycleManager.IsRegistered(this))
            {
                The.Sim.CycleManager.UnRegister(this);
            }

            AddLog("Destroyed, ID = " + id);

            CancelAllSearches();

            RemoveIDEntry();

        }

        private void Init()
        {
            // init fields that are not snapshotted here

            FloodFill = new FloodFill();
            
          //  Common.InitJaggedArray(ref AllNodeDistances, subtileMapWidth, subtileMapHeight);

            CreateRegulators();
        }


        protected virtual void CreateRegulators()
        {
           
        }

       /* private void InitVariablesForRecalc()
        {
            progress = Progress.InitFromScratch;           
         }*/

        public abstract Region GetRegion(ushort color);
       

        public abstract Region GetRegionInProgress(ushort color);
       

        public Region GetRegion(int subtileX, int subtileY)
        {
            return GetRegion(layers.GetRegion(subtileX, subtileY));
        }

        public SubtileSector GetSector(Point subtile)
        {
            return layers.GetSectorFromSubtile(subtile);
        }

        public SubtileSector GetSector(Point subtile, out int layerIndex)
        {
            return layers.GetSectorFromSubtile(subtile, out layerIndex);
        }
     /*   public Region GetRegion(int subtileX, int subtileY)
        {
            return GetRegion(this.AllSubtiles[subtileX][subtileY]);
        }
        */

       
        public void MarkDirtySector(int subtileX, int subtileY)
        {
            SubtileSector sector = targetLayer.GetSectorFromSubtiles(subtileX, subtileY);
            if (sector != null)
            {
                MarkDirtySector(sector);
            }
        }

        public void MarkDirtySector(SubtileSector sector)
        {
            sector.BlockedStatusHasChanged = true;

            sectorsAreDirty = true; // this triggers recompute in Update for the Terrain map
        }

       
        


        public override string ToString()
        {
            return "Region map: " + IDName;
        }

      /*  private void GatherSectorsMarkedAsDirty()
        {
            AddLog("GatherRegionsMarkedAsDirty " + dirtyPoints.Count);

            dirtySectors.Clear();
            
            foreach (var item in dirtyPoints)
            {
                layers.GetSector()
                Region region = GetRegion(item.X, item.Y);
                if (region != null)
                {
                    regionsToRemove.Add(region);
                }
            }

        }*/

        /*
        private void GatherRegionsMarkedAsDirty()
        {
            AddLog("GatherRegionsMarkedAsDirty " + dirtyPoints.Count);
            
            regionsToRemove.Clear();

            foreach (var item in dirtyPoints)
            {
                Region region = GetRegion(item.X, item.Y);
                if (region != null)
                {
                    regionsToRemove.Add(region);
                }
            }
            
        }*/

       /* private void MarkConnectedRegionsAsDirty(Region region, float distance)
        {
            Dictionary<ushort, RegionEdge> connectedRegionEdges;

            if (newRegionGraph.TryGetValue(region.Color, out connectedRegionEdges)) // ignore islands
            {
                float newDistance;
                Region connectedRegion;

                foreach (KeyValuePair<ushort, RegionEdge> kvp in connectedRegionEdges)
                {
                    connectedRegion = GetRegion(kvp.Key);
                 
                    if (!regionsToRemove.Contains(connectedRegion))
                    {
                       // connectedRegion.IsDirty = true;

                        regionsToRemove.Add(connectedRegion);

                        newDistance = Common.DistanceOctile(region.CenterInSubtiles, connectedRegion.CenterInSubtiles) + distance;
                        if (newDistance < 3 * regionRadius)
                        {
                            MarkConnectedRegionsAsDirty(connectedRegion, newDistance);
                        }
                    }
                }
            }
        }*/



     /*   private bool RegionIsEntirelyWithinClearedRectangles(Rectangle regionBoundingRectangle) // Region region)
        {
            foreach (var rectangleToClear in rectanglesToClearAndRedraw)
            {
                if (rectangleToClear.Contains(regionBoundingRectangle))
                    return true;
            }

            return false;
        }*/

        public ushort GetRegionColor(int subtileX, int subtileY)
        {
            return layers.GetRegion(subtileX, subtileY);

        }

        public ushort GetRegionColorInProgress(int subtileX, int subtileY)
        {
            return layers.GetRegion(subtileX, subtileY, true);

        }
        
       

     /*   private void InitFromScratch()
        {
           
            // init the data structures while preserving the ones in use.
            //method: if it is easy, we clear & copy. otherwise, we allocate with new...
            newRegions = new Dictionary<ushort, Region>(); // new List<Region>();
            newRegionGraph = new Dictionary<ushort, Dictionary<ushort, RegionEdge>>();

            Common.ClearJaggedArray(AllNodeDistances);

        }*/


        private Queue<ushort> UnusedRegionColors = new Queue<ushort>();

     
      //  private List<Rectangle> rectanglesToClearAndRedraw = new List<Rectangle>();

        /// <summary>       
        /// the regions and their connections that we want to remove as part of the repair procedure
        /// </summary>
        private HashSet<Region> regionsToRemove = new HashSet<Region>();




        private static bool InitCopyVariablesRegionGraph(
            Dictionary<ushort, Dictionary<ushort, RegionEdge>> RegionGraph, 
            ref Dictionary<ushort, Dictionary<ushort, RegionEdge>> newRegionGraph, 
            ref int regionGraphCopyProgressIndex,
            ref List<ushort> regionGraphKeyListForCopying)
        { 
            // create a deep copy of the region graph. this way, the old data will not be changed while we are updating the new data

            // this appears to be the costliest operation. (still true..?)
          
            if (regionGraphKeyListForCopying == null)
            {
                newRegionGraph = new Dictionary<ushort, Dictionary<ushort, RegionEdge>>();

                regionGraphKeyListForCopying = RegionGraph.Keys.ToList();
                regionGraphCopyProgressIndex = 0;

                return false;
            }

            int maxIndex = Math.Min(regionGraphCopyProgressIndex + 50, regionGraphKeyListForCopying.Count);

            ushort regionColor;
            for (int i = regionGraphCopyProgressIndex; i < maxIndex; i++)
            {
                regionColor = regionGraphKeyListForCopying[i];
                Dictionary<ushort, RegionEdge> newEdges = new Dictionary<ushort, RegionEdge>();
                newRegionGraph.Add(regionColor, newEdges);

                var connections = RegionGraph[regionColor];
                foreach (var item in connections)
                {
                    newEdges.Add(item.Key, new RegionEdge(item.Value.FromRegion, item.Value.ToRegion, item.Value.Length, item.Value.HasRoad));
                }
            }

            regionGraphCopyProgressIndex = maxIndex; // store the progress

            if (regionGraphCopyProgressIndex == regionGraphKeyListForCopying.Count)
            {
                
                regionGraphKeyListForCopying = null;
                return true;
            }
            else return false;
        }

        /*

        private void InitCopyVariablesRegionGraph()
        {
            newRegionGraph = new Dictionary<ushort, Dictionary<ushort, RegionEdge>>();

            // create a deep copy of the region graph. this way, the old data will not be changed while we are updating the new data
            // this appears to be the costliest operation.
            foreach (KeyValuePair<ushort, Dictionary<ushort, RegionEdge>> kvp in RegionGraph)
            {
                Dictionary<ushort, RegionEdge> newEdges = new Dictionary<ushort, RegionEdge>();
                newRegionGraph.Add(kvp.Key, newEdges);

                foreach (var item in kvp.Value)
                {
                    newEdges.Add(item.Key, new RegionEdge(item.Value.FromRegion, item.Value.ToRegion, item.Value.Length, item.Value.HasRoad));
                }
            }            
        }*/

       

        /// <summary>
        /// gets all sectors where blocked status has changed, also resets dirty flags
        /// </summary>
        private void GetDirtySectors()
        {

            dirtySectors.Clear();
            targetLayer.IterateDirtySectors(s =>
                {
                    dirtySectors.Add(s.Coords);
                    s.BlockedStatusHasChanged = false; // reset, and gather while repair is ongoing
                });

#if DEBUG
            AddLog("GetDirtySectors: " + string.Join(",", dirtySectors));
#endif

            sectorsAreDirty = false;

        }

        private void InitCopyVariablesRegions()
        {
            // shallow copies of regions should be ok. the class is immutable.
            newRegions = new Dictionary<ushort, Region>(regions);

            AddLog("InitCopyVariablesRegions done.");
          
        }

    

        private bool ClearDirtySectorSubtiles()
        {
            if (dirtySectors.Count > 0)
            {
                bool isLastSector;
                SubtileSector sector = GetCurrentDirtySectorAndIncrement(out isLastSector);

                List<ushort> clearedRegions = new List<ushort>();
                sector.ClearRegions(clearedRegions);
                

                foreach (var item in clearedRegions)
                {
                    //  Debug.Assert(!regionsToRemove.Any(r => r.Color == item), "stored duplicate region...");

                    regionsToRemove.Add(newRegions[item]);
                }


                return isLastSector;
            }

            return true;
        }

        /*
        private void ClearDirtySectorSubtiles()
        {
            //regionsToTestForRemoval.Clear();
            edgeRegions.Clear();
            secondRectangleMergeIsNeeded = false;

            ushort subtileValue;
            Region region;

            // NEW NEW:
            // any regions that we touch along the edges are marked to be filled in.
            // clear them in a second pass.
            foreach (Rectangle rectangle in rectanglesToClearAndRedraw)
            {                             
                GetRegionsAlongHorizontalEdge(rectangle, rectangle.Top);                
                GetRegionsAlongHorizontalEdge(rectangle, rectangle.Bottom);

                GetRegionsAlongVerticalEdge(rectangle, rectangle.Left);
                GetRegionsAlongVerticalEdge(rectangle, rectangle.Right);
            }


            // iterate over the rectangles, clear everything inside
            foreach (Rectangle rectangle in rectanglesToClearAndRedraw)
            {              
                for (int x = rectangle.Left; x <= rectangle.Right; x++)
                {                  
                    for (int y = rectangle.Top; y <= rectangle.Bottom; y++)
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
                }

            }

            // now clear the regions along the edges thay may extend beyond the original rectangle to be cleared.
            bool wasAdded = false;
            foreach (var color in edgeRegions)
            {
                region = GetRegion(color);

                Rectangle rectangle = GetBoundingRectangle(0, region);
                wasAdded = false;

                for (int x = rectangle.Left; x <= rectangle.Right; x++)
                {
                    for (int y = rectangle.Top; y <= rectangle.Bottom; y++)
                    {
                        subtileValue = newAllSubtiles[x][y];
                        if (subtileValue == color) // this makes sure we don't touch new regions!
                        {
                            newAllSubtiles[x][y] = 0; // mark as 'no region'
                                                       
                            if (!wasAdded)
                            {
                                // add this as a new rectangle. We will need to perform an extra cleanup step to remove the overlap and merge the touching rectangles.
                                rectanglesToClearAndRedraw.Add(rectangle);

                                regionsToRemove.Add(region);

                                wasAdded = true;
                                secondRectangleMergeIsNeeded = true;
                            }

                        }
                    }
                }
            }

        }*/

     /*   private void GetRegionsAlongHorizontalEdge(Rectangle rectangle, int y)
        {
            ushort subtileValue;
            
            for (int x = rectangle.Left; x <= rectangle.Right; x++)
            {
                subtileValue = newAllSubtiles[x][y];
                if (subtileValue != 0)
                {
                    edgeRegions.Add(subtileValue);
                }
            }
            
        }*/

      /*  private void GetRegionsAlongVerticalEdge(Rectangle rectangle, int x)
        {
            ushort subtileValue;

            for (int y = rectangle.Top; y <= rectangle.Bottom; y++)
            {
                subtileValue = newAllSubtiles[x][y];
                if (subtileValue != 0)
                {
                    edgeRegions.Add(subtileValue);
                }
            }

        }*/

     /*   private void GetRectanglesAroundRegions()
        {           
            rectanglesToClearAndRedraw.Clear();

            // include an extra area to make the regions more evenly spaced and avoid long linear regions if possible
            int buffer = regionRadius; // 1;

            foreach (Region region in regionsToRemove)
            {
                Rectangle rectangle = GetBoundingRectangle(buffer, region);

                rectanglesToClearAndRedraw.Add(rectangle);

            }
        }*/

     /*   private static Rectangle GetBoundingRectangle(int buffer, Region region)
        {
            // NEW: since the "center" can now occur at the edge, we have to use a longer radius than before for the bounding rectangle.
           // int radiusToUse = 2 * regionRadius;

            int radiusToUse = regionRadius;

            Rectangle rectangle = new Rectangle();
            rectangle.X = region.CenterInSubtiles.X - radiusToUse - buffer;
            rectangle.Y = region.CenterInSubtiles.Y - radiusToUse - buffer;

            rectangle.Width = 2 * (radiusToUse + buffer);
            rectangle.Height = rectangle.Width;

            int minX, maxX, minY, maxY;

            // rectangle boundaries are guaranteed to be within map boundaries
            rectangle = MapManager.GetClampedRectangularMapAreaUsingSubtiles(new Point(rectangle.X, rectangle.Y), rectangle.Width, rectangle.Height, out minX, out maxX, out minY, out maxY);
            return rectangle;
        }*/

        /*
        private bool GetUnassignedPoint()
        {
            // scan rectangles only TODO: iterate sectors instead of rectangles
            if (rectanglesToClearAndRedraw.Count > 0) // || listOfDirtyPointsOutsideRegions.Count > 0)
            {
                bool scanRectangles = listOfUnassignedPoints.Count == 0;
                              

                if (scanRectangles) // listOfUnassignedPoints.Count == 0)
                {
                    foreach (Rectangle scanRectangle in rectanglesToClearAndRedraw)
                    {
                        ScanAreaForUnassignedSubtiles(scanRectangle);
                    }

                    if (listOfUnassignedPoints.Count > 0)
                    {
                        currentCenter = listOfUnassignedPoints.ElementAt(0).ToPoint(); //[0]; // ???
                        return true;
                    }
                    else return false;

                }
                else
                {
                    return ExamineUnassignedPoints();
                }

            }
            else // scan whole map
            {
                // brute force: get a point that has not been assigned a region yet:
                if (listOfUnassignedPoints.Count == 0)
                {
                    listOfDirtyPointsOutsideRegions.Clear();

                    ScanAreaForUnassignedSubtiles(new Rectangle(0, 0, subtileMapWidth - 1, subtileMapHeight - 1));

                    if (listOfUnassignedPoints.Count > 0)
                    {
                        currentCenter = listOfUnassignedPoints.ElementAt(0).ToPoint(); //[0]; // ???
                        return true;
                    }
                    else return false; // we are done with flood filling the map.
                }
                else
                {
                    return ExamineUnassignedPoints();
                }
            }
        }*/

        /* moved to SubtileSector
        private void ScanAreaForUnassignedSubtiles(Rectangle area)
        {
            ushort[] newAllNodesColumn;
            MapManager.SubtileValue[] moveCostsColumn;
            for (int x = area.Left; x <= area.Right; x++)
            {
                newAllNodesColumn = newAllSubtiles[x];
                moveCostsColumn = layers[x];

                for (int y = area.Top; y <= area.Bottom; y++)
                {
                    if (newAllNodesColumn[y] == 0 &&
                        !MapManager.IsBlocked(moveCostsColumn[y]))
                    {
                        listOfUnassignedPoints.Add(new SubtilePos((ushort)x, (ushort)y));
                    }
                }
            }
        }*/

       
        /// <summary>
        /// not currently being called... perhaps it should?
        /// </summary>
      /*  private void AddDirtyPointsOutsideRegionsToListOfUnassignedPoints()
        {
            foreach (var item in listOfDirtyPointsOutsideRegions)
            {
                if (newAllSubtiles[item.X][item.Y] == 0 &&
                    !MapManager.IsBlocked(layers[item.X][item.Y]))
                {
                    listOfUnassignedPoints.Add(item);
                }
            }
        }*/

         

        
      

        #region ICyclable Members

        private int cyclesToFillRegions = 0, cyclesToBuildGraph = 0;

        double timeTaken;

        HighResolutionTime timer;

        protected virtual void RecomputeStarted()
        {           
            if (sectorsAreDirty)
            {
                AddLog("Recompute started, isComputing = true");

                isComputing = true;               
            }
        }

      
        protected virtual void RecomputeFinished()
        {
            isComputing = false;

           /* if (isComputing)
            {
                AddLog("Recompute ended, isComputing = false");

                isComputing = false;
                RegionsFinished.Invoke();
            }*/
        }


        protected virtual void RecomputeAborted()
        {
            AddLog("Recompute aborted.");

            isComputing = false;
        }

       /* protected virtual void RecomputeStarted() { }
        protected virtual void RecomputeEnded() { }
        */

        protected virtual void StartBuildingGraphBetweenSectors() { }

        public virtual void PrintInfo(StringBuilder text)
        {
            text.Append(string.Format("RegionMap {0}:", ID));

        }


        public bool CycleOnce()
        {
            bool result = false;

            timer.Start();

            System.Diagnostics.Debug.Assert(regions != null, "Regions is null?");

            if (/*this is DependentRegionMap &&*/ IDName == "ExposedIndigHerbivoreCautiousFoot") // "Foot")
            {

            }

            switch (progress)
            {
                case Progress.InitFromScratch:

                   // InitFromScratch();

                    RecomputeStarted();

                    GetDirtySectors();                  

                    if (dirtySectors.Count == 0)
                    {
                        RecomputeAborted();
                        //RecomputeFinished();
                        return true; // abort
                    }

                    progress = Progress.FillRegions;
                    break;

                // 
                /*
                 Region colors - 256^2 colors are available.
                 * The dependent map can not use the same region colors as the base terrain. It would not be possible to create the graph.
                 * Terrain region map has its own range of colors.
                 * Each dependent can use the same region colors.              
                 */


                /* The terrain region maps should refill all dirty sectors. 
                 * The dependent maps each have their own graphs... but they point to shared regions which may no longer exist. 
                 * These graphs are now invalid when regions are removed... the costs tables also.
                 * 
                 * Before starting, notify dependents with the RegionsAreInvalid event. They will set their IsReady flag to false and not process any requests.
                 * 
                 * What can dependents do while terrain is recomputing?? Anything that does not collide with the terrain map. Repairing sectors is OK.
                 * 
                 * After the regions are done, the region graph is rebuilt. 
                 * After that, the event RegionsFinished should notify the dependent movement region maps. 
                 * The dependents then have to repair their region graphs. Then they can set IsReady true and restart all searches.
                 *
                 * 
                 */
                    

                // the repair procedure starts here:
                // repair works by getting the bounding rectangles around dirty regions, then painting everything inside the area with a 0.
                // also removing the regions and their connections.
                // then drawing new regions via floodfill inside the whole area.
                // colors will be reused in the new regions.
                #region Init for repair
                case Progress.InitCleanupUnneededSectors:

                    RecomputeStarted();

                    if (!sectorsAreDirty)
                    {
                        RecomputeAborted();
                     
                        return true; // abort
                    }
                    
                    //CleanupUneededSectors();
                    GetDirtySectors();

                    progress = Progress.InitCopyVariablesSubtiles;
                    break;

                case Progress.InitCopyVariablesSubtiles:
                                               
                    if (InitCopyVariablesSubtiles())
                    {
                        AddLog("InitCopyVariablesSubtiles done.");
         
                        progress = Progress.InitCopyVariablesRegions;
                    }          
                   
                    break;

                case Progress.InitCopyVariablesRegions:
                    InitCopyVariablesRegions();
                    progress = Progress.InitCopyVariablesRegionGraph;
                    break;

                case Progress.InitCopyVariablesRegionGraph:
                    if (InitCopyVariablesRegionGraph(RegionGraph, ref newRegionGraph, ref regionGraphCopyProgressIndex, ref regionGraphKeyListForCopying))
                    {
                        AddLog("InitCopyVariablesRegionGraph done.");
          
                        progress = Progress.InitCopyVariablesClearDistances;
                    }

                    break;

                case Progress.InitCopyVariablesClearDistances:
                   
                    if (InitCopyVariablesClearDistances())
                    {
                        AddLog("InitCopyVariablesClearDistances done.");
          
                        progress = Progress.InitClearDirtySectors;
                    }

                    break;

                #endregion

                // clear whole sectors.
                case Progress.InitClearDirtySectors:
                    if (ClearDirtySectorSubtiles())
                    {
                        AddLog("ClearDirtySectorSubtiles done.");
          
                        progress = Progress.InitRemoveRegionsAndLinks; 
                    }

                    break;
                    
                case Progress.InitRemoveRegionsAndLinks:
                                       
                    RemoveRegions();
                    progress = Progress.FillRegions; // done with init.
                                      

                    break;

                case Progress.FillRegions:

                    cyclesToFillRegions++;
                    
                    if (CreateRegions())
                    {
                        AddLog("FillRegions done");
                        // now connect them:
                        progress = Progress.BuildInternalRegionGraph;
        
                    }

                    break;
                    
             
               /* case Progress.RemoveEdgesBetweenLayers:

                    if (RemoveEdgesBetweenLayers())
                    {
                        AddLog("RemoveEdgesBetweenLayers done");

                        progress = Progress.BuildInternalRegionGraph;
                        cyclesToBuildGraph = 0;
                    }
                    break;
                    */
                case Progress.BuildInternalRegionGraph:

                    cyclesToBuildGraph++;

                    if (BuildInternalEdgesForDirtySectors())
                    {
                        AddLog("BuildRegionGraph done");

                        progress = Progress.RemoveEdgesBetweenLayers;


                        StartBuildingGraphBetweenSectors();  // dependent map may pause here while the terrain map computes.  
                    }

                    break;

                /*
              * 
              *  PAUSE the dependent map here, if terrain map is being recomputed. It must be finished before we can connect the layers.
                
                  ///scan one sector at a time.
                 /// scan the edges between sectors in a separate method.
                 /// 
                 * the dependent graph needs its own Region Graph as well as some additional collections. We have to rebuild it even if the dependents sectors' have not been changed. 
              * 
              *  /// 1. Remove edges from overridden sectors to the bottom layer
                 /// 2. Remove edges from the bottom layer to overridden sectors - these are called "connectors" and "blockers"              
                 /// 3. Scan the edges
              */
                case Progress.RemoveEdgesBetweenLayers:
                    //AssertRegionGraph();

                    if (RemoveAllEdgesBetweenLayers())
                    {
                        AssertRegionGraph();

                        AddLog("RemoveEdgesBetweenLayers done");

                        progress = Progress.BuildRegionGraphBetweenSectors;
                        cyclesToBuildGraph = 0;


                    }
                    break;
                                   
                case Progress.BuildRegionGraphBetweenSectors:

                    cyclesToBuildGraph++;

                    if (BuildRegionGraphBetweenSectors())
                    {                      
 
                        //Done! 

                        AddLog("BuildRegionGraphBetweenSectors done");


                        SetProgressAtRepairStart();
                       
                        
                        // cancel all ongoing searches on the old data:
                        CancelAllSearches();


                        AssertRegionGraph();

                        //now save the finished data structures ready to use!
                        regions = newRegions;
                        RegionGraph = newRegionGraph;

                        FinishSectors();
                                          

                        scannedSectors.Clear();

                        // unfortunately we can't reuse this data because the graph has changed...
                        AllRegionCosts.Clear();      
                        foreach (var item in RegionPaths)
                        {
                            foreach (var pathID in item.Value)
                            {
                                RegionPath path = LookUp<RegionPath, RegionPathID>.FindByID(pathID.Value);
                                if (path != null)
                                {
                                    path.Destroy(); // any ongoing AStar searches will be permitted to continue with invalid IDs, point search often uses outdated maps anyway.
                                }
                            }
                        }
                        RegionPaths.Clear();


                        cachedClosestRegionToBlockedSubtile.Clear();

                     
                        RecomputeFinished();

                        // we are ready to receive search requests from agents now!                                              
                        // (re)start all search requests with the new data:
                        StartPendingSearchRequests();


                        AssertRegionGraph(false);

                        dirtySectors.Clear();

                        cyclesToFillRegions = 0; // 1990
                        cyclesToBuildGraph = 0; // 780

                        result = true;
                        break;

                        //return true;
                    }
                    break;
            }

            timeTaken = timer.GetTime();

            if (timeTaken > 0.002)
            {

            }

            return result; // false; // not done yet...

        }

        protected void SetProgressAtRepairStart()
        {
            progress = Progress.InitCleanupUnneededSectors;
            currentSectorIndex = 0;

            AddLog("Progress set to repair start (InitCleanupUnneededSectors)");

        }

        protected virtual void AssertRegionGraph(bool useFieldsInProgress = true)
        {

        }
        

        public Region CreateRegion(Point center)
        {
            ushort newColor;

            // compute a new region
            if (UnusedRegionColors.Count > 0)
            {
                newColor = UnusedRegionColors.Dequeue();

              //  AddLog("Creating region with reused color: " + newColor);
            }
            else
            {
                newColor = regionColorCounter;
                regionColorCounter++;

              //  AddLog("Creating region with new color: " + newColor);
            }

            Region region = new Region(center, newColor);
            newRegions.Add(region.Color, region);

            return region;
        }

        private bool CreateRegions()
        {
            if (dirtySectors.Count > 0)
            {
                SubtileSector sector = GetCurrentDirtySector();

                if (sector.FloodFill(this))
                {
                    return IncrementSectorIndex();
                }
                else return false;
            }

            return true;
        }


      /*  private bool GetNextCenter()
        {
            bool isLastSector;
            SubtileSector sector = GetCurrentSector(out isLastSector);

            if (sector.)
            // no more regularly spaced centers...
            // test the map for holes:
            if (!GetUnassignedPoint()) // scan whole map in one cycle!!!???
            {
                // flood fill is done. We have all the regions. 

                AddLog("FillRegions done");

                // now connect them:
                progress = Progress.BuildGraph;

                // return false;
                break;
            }
            

        }*/

        private bool InitCopyVariablesSubtiles()
        {            

            if (dirtySectors.Count > 0)
            {
                bool isLastSector;
                SubtileSector sector = GetCurrentDirtySectorAndIncrement(out isLastSector);

                sector.InitCopyVariablesSubtiles();

                return isLastSector;
            }

              

            return true;            
        }

        private bool InitCopyVariablesClearDistances()
        {
            if (dirtySectors.Count > 0)
            {
                bool isLastSector;
                SubtileSector sector = GetCurrentDirtySectorAndIncrement(out isLastSector);

                sector.InitCopyVariablesClearDistances();

                return isLastSector;
            }

            return true;
        }


        private void FinishSectors()
        {
            // we copy & clear this:                       
            //Common.CopyJaggedArray(newAllSubtiles, AllSubtiles);
            SubtileSector sector = null;

            foreach (var item in dirtySectors)
            {               
                sector = targetLayer.GetSector(item.X, item.Y);

                sector.FinishRegions();

            }
        }


        /// <summary>
        /// terrain regions are never removed...
        /// </summary>
      /*  protected virtual void CleanupUneededSectors()  
        {
          

        }*/

       /* protected SubtileSector GetCurrentSector(out bool isLastSector)
        {
            SubtileSector overiddenSector;
            return GetCurrentSector(false, out isLastSector, out overiddenSector);
        }*/

        /// <summary>
        /// also resets the sector index if the last sector is reached
        /// </summary>
        /// <param name="isLastSector"></param>
        /// <returns></returns>
        protected SubtileSector GetCurrentDirtySectorAndIncrement(out bool isLastSector)
        {          

           // overiddenSector = null;
            SubtileSector sector = GetCurrentDirtySector();

            isLastSector = IncrementSectorIndex();    

            return sector;
        }

        private bool IncrementSectorIndex()
        {
            bool isLastSector;
            if (currentSectorIndex == dirtySectors.Count - 1)
            {
                isLastSector = true;
                currentSectorIndex = 0;
            }
            else
            {
                isLastSector = false;
                currentSectorIndex++;
            }
            return isLastSector;
        }

        private SubtileSector GetCurrentDirtySector()
        {
            SubtileSector sector = null;

            Point coords = dirtySectors[currentSectorIndex];
            sector = targetLayer.GetSector(coords.X, coords.Y);

            /* if (getOverriddenSector)
             {
                overiddenSector = layers.GetOverriddenSector(coords.X, coords.Y);
             }*/

            // we should not remove sectors while repair is ongoing...
            // so add a cleanup at the end...
            Debug.Assert(sector != null, "sector was removed during repair... this should not happen");
            //currentSectorIndex++;
            return sector;
        }

        /// <summary>
        /// used when iterating all sectors. for the dependent map, this will only iterate filled sectors
        /// </summary>
        /// <param name="isLastSector"></param>
        /// <returns></returns>
        protected SubtileSector GetCurrentSector(out bool isLastSector)
        {
            // overiddenSector = null;
            SubtileSector sector = null;

            do
            {
                int remainder;
                int quotient = Math.DivRem(currentSectorIndex, targetLayer.SectorsAcrossWidth, out remainder);
                Point coords = new Point(quotient, remainder);
                sector = targetLayer.GetSector(coords.X, coords.Y);

                if (coords.X == targetLayer.SectorsAcrossWidth - 1 
                 && coords.Y == targetLayer.SectorsAcrossHeight - 1)
                {
                    isLastSector = true; // done
                    currentSectorIndex = 0;
                }
                else
                {
                    isLastSector = false;
                    currentSectorIndex++; // reuse the index...
                }
            }
            while (sector == null && isLastSector == false);

            return sector;
        }

     /*   private void StartPendingSearchRequests()
        {
            if (this.regionSearchRequests.Count > 0)
            {
                for (int i = regionSearchRequests.Count - 1; i >= 0; i--)
                {
                    RegionSearchRequestID searchRequestID = regionSearchRequests[i];
                    RegionSearchRequest searchRequest = LookUp<RegionSearchRequest, RegionSearchRequestID>.FindByID(searchRequestID);

                    RegisterSearch(searchRequest);

                    AddLog("(Re)started search from " + searchRequest.FromSubtile);
                }

                             
            }
        }*/


        private void StartPendingSearchRequests()
        {
            if (this.regionSearchRequests.Count > 0)
            {
                for (int i = regionSearchRequests.Count - 1; i >= 0; i--)
                {
                    RegionSearchRequestID searchRequestID = regionSearchRequests[i];
                    RegionSearchRequest searchRequest = LookUp<RegionSearchRequest, RegionSearchRequestID>.FindByID(searchRequestID);

                    // start a planner to avoid spikes...
                    RegisterSearch(searchRequest);

                    AddLog("(Re)started search from " + searchRequest.FromSubtile);
                }


            }
        }


        /// <summary>
        /// to be called when the region map is redrawn. Restart all current searches on the new map?
        /// </summary>
        protected void CancelAllSearches()
        {
            // clear all ongoing searches! They will be restarted with the new data!
            RegionSearchPlanner regionSearchPlanner;

           
            foreach (var kvp in this.regionBFSPlanners)
            {
                regionSearchPlanner = (RegionSearchPlanner)LookUp<ICyclable, CyclableID>.FindByID(kvp.Value);
                if (regionSearchPlanner != null)
                {
/*
                    foreach (var item in regionSearchPlanner.SearchRequests)
	                {
                        searchIDS += item + ", ";

	                }

                    AddLog(string.Format("Cancelled searches from: {0}, IDs: {1}", regionSearchPlanner.Start, searchIDS));                    
                    */                 
                  
                    regionSearchPlanner.Destroy(); 
                  
                }              
            }

            foreach (var item in regionAStarPlanners)
            {
                foreach (var item2 in item.Value)
                {
                    regionSearchPlanner = (RegionSearchPlanner)LookUp<ICyclable, CyclableID>.FindByID(item2.Value);
                    if (regionSearchPlanner != null)
                    {
                        regionSearchPlanner.Destroy();
                    }
                }               
            }


            regionBFSPlanners.Clear();
            regionAStarPlanners.Clear();
        }

        public enum Result { OK, Wait, NoAccess }


       // public delegate void NotifyWhenFinished();


        /// <summary>
        /// we use this method when we do not have an active agent to check for access to containers etc.
        /// if an allegiance manager like HaulingJobManager calls this, items contained by agents in the same allegiance will be seen as accessible.
        /// intelligentEntityType is used to determine if it can be reached in case it is inside a building.
        /// 
        /// fromEntity can be a non-intelligent entity like an item. 
        /// fill in either fromEntity or fromSubtile
        /// and either toEntity or toSubtile
        /// </summary>
        /// <param name="fromEntityID"></param>
        /// <param name="toEntityID"></param>
        /// <param name="sharedKnowledge"></param>
        /// <param name="distance"></param>
        /// <param name="intelligentEntityType"></param>
        /// <param name="startingSubtile"></param>
        /// <param name="sendMessageToEntity"></param>
        /// <param name="notifyWhenFinished"></param>
        /// <param name="doRegionSearchNow"></param>
        /// <returns></returns>
        public Result GetDistanceToEntityUsingEntityType(EntityID? fromEntityID, EntityID? toEntityID, SharedKnowledge sharedKnowledge, ref float distance, 
            Allegiance allegiance,
            EntityType intelligentEntityType,
            out EntityResult fromEntityKnowledgeResult, out EntityResult toEntityKnowledgeResult,
            Point? startingSubtile = null, Point? endingSubtile = null,
            bool sendMessageToEntity = true, MethodID? notifyWhenFinished = null) //, bool doRegionSearchNow = false) 
        {
            Point? fromSubtile, toSubtile;

            fromEntityKnowledgeResult = EntityResult.SeenDirectly;
            toEntityKnowledgeResult = EntityResult.SeenDirectly;

           
            if (fromEntityID != null)
            {               
                if (!GetEntityPosition(fromEntityID.Value, intelligentEntityType, allegiance, null,
                    sharedKnowledge, out fromSubtile, out fromEntityKnowledgeResult))
                {
                    return Result.NoAccess;
                }
              
            }
            else
            {
                fromSubtile = startingSubtile.Value;
            }

            if (toEntityID != null)
            {              
                if (!GetEntityPosition(toEntityID.Value, intelligentEntityType, allegiance, null, 
                    sharedKnowledge, out toSubtile, out toEntityKnowledgeResult))
                {
                    return Result.NoAccess;
                }              
            }
            else
            {
                toSubtile = endingSubtile.Value;
            }


            if (fromSubtile.HasValue && toSubtile.HasValue)
            {
                return GetDistance(null, fromSubtile.Value, toSubtile.Value, ref distance, sendMessageToEntity, notifyWhenFinished); //, doRegionSearchNow);
            }
            else return Result.NoAccess;

        }

        public Result GetDistanceToEntityUsingWorldLocation(Entity activeEntity, IKnownEntityData fromEntity, IKnownEntityData toEntity, /*EntityGroup toEntityGroup,*/ ref float distance,
            Vector3? fromLocation = null, Vector3? toLocation = null,
            bool sendMessageToEntity = true, MethodID? notifyWhenFinished = null, bool doRegionSearchNow = false)
        {
            Point? fromSubtile = null, toSubtile = null;

            if (fromLocation.HasValue)
            {
                fromSubtile = MapManager.WorldPosToSubtile(fromLocation.Value);
            }

            if (toLocation.HasValue)
            {
                toSubtile = MapManager.WorldPosToSubtile(toLocation.Value);
            }

            return GetDistanceToEntity(activeEntity, fromEntity, toEntity, //toEntityGroup,
                ref distance,
                fromSubtile, toSubtile,
                sendMessageToEntity, notifyWhenFinished, doRegionSearchNow);

        }

        /// <summary>
        /// this overload can be used instead of the one with EntityIDs, in case a lookup has already been made. activeEntity is used for determining access to containers/buildings.
        /// Active entity and From entity will often be the same agent.
        /// 
        /// NEW: will also set a flag on the destnation entity to indicate to the player if it was inaccessible.
        /// </summary>
        /// <param name="activeEntity"></param>
        /// <param name="fromEntity"></param>
        /// <param name="toEntity"></param>
        /// <param name="distance"></param>
        /// <param name="intelligentEntityType"></param>
        /// <param name="startingSubtile"></param>
        /// <param name="endingSubtile"></param>
        /// <param name="sendMessageToEntity"></param>
        /// <param name="notifyWhenFinished"></param>
        /// <param name="doRegionSearchNow"></param>
        /// <returns></returns>
        public Result GetDistanceToEntity(
           Entity activeEntity,
           IKnownEntityData fromEntity,
           IKnownEntityData toEntity,
           ref float distance,
           Point? startingSubtile = null, Point? endingSubtile = null,
           bool sendMessageToEntity = true, MethodID? notifyWhenFinished = null, bool giveClientFeedback = true,
           Allegiance allegiance = null, bool allowTransactingWithAgentsInAllegiance = false)
        {
            Point? fromSubtile, toSubtile;
            
            return GetDistanceToEntity(
                activeEntity, 
                fromEntity, 
                toEntity, 
                ref distance,
                out fromSubtile, out toSubtile,
                startingSubtile, endingSubtile,
                sendMessageToEntity, notifyWhenFinished, giveClientFeedback,
                allegiance, allowTransactingWithAgentsInAllegiance);

        }

        /// <summary>
        /// this overload can be used instead of the one with EntityIDs, in case a lookup has already been made. activeEntity is used for determining access to containers/buildings.
        /// Active entity and From entity will often be the same agent.
        /// 
        /// NEW: will also set a flag on the destnation entity to indicate to the player if it was inaccessible.
        /// </summary>
        /// <param name="activeEntity"></param>
        /// <param name="fromEntity"></param>
        /// <param name="toEntity"></param>
        /// <param name="distance"></param>
        /// <param name="intelligentEntityType"></param>
        /// <param name="startingSubtile"></param>
        /// <param name="endingSubtile"></param>
        /// <param name="sendMessageToEntity"></param>
        /// <param name="notifyWhenFinished"></param>
        /// <param name="doRegionSearchNow"></param>
        /// <returns></returns>
        public Result GetDistanceToEntity(
            Entity activeEntity, 
            IKnownEntityData fromEntity, 
            IKnownEntityData toEntity, 
            ref float distance,
            out Point? fromSubtile, out Point? toSubtile,
            Point? startingSubtile = null, Point? endingSubtile = null,
            bool sendMessageToEntity = true, MethodID? notifyWhenFinished = null, bool giveClientFeedback = true,
            Allegiance allegiance = null, bool allowTransactingWithAgentsInAllegiance = false)
        {
            fromSubtile = null;
            toSubtile = null;

           
            try
            {
                // crash here: http://steamcommunity.com/app/284100/discussions/4/1290691308567487147/
                // Nullable object must have a value.


                if (fromEntity != null)
                {
                    if (!GetEntityPosition(fromEntity, activeEntity, activeEntity.EntityType, allegiance, allowTransactingWithAgentsInAllegiance, out fromSubtile))
                    {
                        return Result.NoAccess;
                    }

                }
                else
                {
                    fromSubtile = startingSubtile.Value;
                }

                if (toEntity != null)
                {
                    if (!GetEntityPosition(toEntity, activeEntity, activeEntity.EntityType, allegiance, allowTransactingWithAgentsInAllegiance, out toSubtile))
                    {
                        return Result.NoAccess;
                    }
                }
                else
                {
                    toSubtile = endingSubtile.Value;
                }


                if (fromSubtile.HasValue && toSubtile.HasValue)
                {
                    Result result = GetDistance(activeEntity, fromSubtile.Value, toSubtile.Value, ref distance, sendMessageToEntity, notifyWhenFinished); //, doRegionSearchNow);

                    // give client feedback:
                    if (giveClientFeedback
                        && toEntity != null)
                    {
                        if (result == Result.NoAccess)
                        {
                            The.Client.SetEntityInaccessible(activeEntity.Intelligence.Allegiance, toEntity, true);

                            // test terrain accessibility - this does a recursive call! - skip if Wait:
                            bool? isBlockedByThreat = GoalEvaluator.ComputeIsBlockedByThreat(activeEntity, toEntity);

                            // now set the blocked by threat flag via deduction:
                            if (isBlockedByThreat.HasValue)
                            {

                                The.Client.SetEntityBlockedByThreat(activeEntity.Intelligence.Allegiance, toEntity, isBlockedByThreat.Value);
                            }
                        }
                        else
                        {
                            The.Client.SetEntityInaccessible(activeEntity.Intelligence.Allegiance, toEntity, false);
                        }
                    }


                    return result;
                }
                else return Result.NoAccess;

            }
            catch(InvalidOperationException e)
            {
                /*
                    out Point? fromSubtile, out Point? toSubtile,
                    Point? startingSubtile = null, Point? endingSubtile = null,
                 */

                string info = string.Format("fromSubtile: {0}, toSubtile: {1}, startingSubtile: {2}, endingSubtile: {3}", fromSubtile, toSubtile, startingSubtile, endingSubtile);

                throw new Exception(e.Message + " " + info, e);
            }

        }


        /// <summary>
        /// returns either the entitys last known position or the entrance to its outermost container
        /// job manager doesn't have an active entity, it supplies an entity type instead.
        /// </summary>
        /// <param name="entityToGetPositionFor"></param>
        /// <param name="intelligentEntityType"></param>
        /// <param name="activeEntityToGoToEntrance"></param>
        /// <param name="sharedKnowledge"></param>
        /// <param name="subtilePosition"></param>
        /// <param name="entityKnowledgeResult"></param>
        /// <returns></returns>
        private bool GetEntityPosition(EntityID entityToGetPositionFor, EntityType intelligentEntityType, Allegiance allegiance, Entity activeEntityToGoToEntrance, SharedKnowledge sharedKnowledge, out Point? subtilePosition, out EntityResult entityKnowledgeResult)
        {
            subtilePosition = null;
            
            IKnownEntityData entityData;
            entityKnowledgeResult = sharedKnowledge.GetKnownData(entityToGetPositionFor, out entityData);

            if (entityKnowledgeResult == EntityResult.Remembered || entityKnowledgeResult == EntityResult.SeenDirectly)
            {
                return GetEntityPosition(entityData, activeEntityToGoToEntrance, intelligentEntityType, allegiance, true, out subtilePosition);                
            }
            else
            {               
                return false;
            }
        }


        /// <summary>
        /// job manager doesn't have an active entity, it supplies an entity type instead.
        /// </summary>
        /// <param name="entityData"></param>
        /// <param name="activeEntityToGoToEntrance"></param>
        /// <param name="intelligentEntityType"></param>
        /// <param name="subtilePosition"></param>
        /// <returns></returns>
        private bool GetEntityPosition(IKnownEntityData entityData, Entity activeEntityToGoToEntrance, EntityType intelligentEntityType, Allegiance allegiance, bool allowTransactingWithAgentsInAllegiance, out Point? subtilePosition)
        {
            subtilePosition = null;
                                             

            if (entityData.ContainedBy != null)  // target entity is inside a container               
            {
                if (activeEntityToGoToEntrance != null && entityData.ContainedBy == activeEntityToGoToEntrance.EntityID) 
                {
                    // carried by self
                    subtilePosition = MapManager.WorldPosToSubtile(activeEntityToGoToEntrance.AccessPoint.Value); // NEW! test it... //Location);

                    return true;
                }

                Entity container = Entity.FindByID(entityData.ContainedBy.Value);


                if (container != null)
                {
                    if (container == activeEntityToGoToEntrance)
                    {
                        // the container is the agent
                        subtilePosition = MapManager.WorldPosToSubtile(activeEntityToGoToEntrance.PlaySiteLocation);
                    }
                    else if ( // can we enter or transact with the container?                        
                           container.EntityType.ContainerType.AllowedInContainer(intelligentEntityType)
                        || container.EntityType.ContainerType.CanTransactWithContainer(intelligentEntityType)
                        || (allowTransactingWithAgentsInAllegiance == true 
                            && container.EntityType.IntelligenceType != null 
                            && container.Intelligence.Allegiance == allegiance))  // the hauling job manager must be allowed to 'transact' with items carried by agents in its allegiance.
                    {
                        subtilePosition = MapManager.WorldPosToSubtile(container.AccessPoint.Value);

                    }
                    else return false; //  can't access the container
                }
                else return false; // container is destroyed??

            }
            else if (entityData.Location == null)
            {
                return false; // off site...
            }
            else
            {
                subtilePosition = MapManager.WorldPosToSubtile(entityData.AccessPoint.Value);

            }
         

            return true;
            
        }

        /// <summary>
        /// parameter entity can be null here.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="frontEntrance"></param>
        /// <param name="backEntrance"></param>
        /// <returns></returns>
     /*   private Point? GetClosestBuildingEntranceToEntity(Entity entity, Vector3 frontEntrance, Vector3? backEntrance) //Buildings.Building building) //Entity toEntity)
        {
            Point? entranceSubtile = null;


            // target entity is inside a building. find the entrance to use:
          //  Buildings.Building building = toEntity.InsideBuilding.TileLayout.Building;
            Point frontDoorSubtile;
            Point? backDoorSubtile = null;

            Point? entitySubtile = null;
            if (entity != null)
            {
                entitySubtile = MapManager.WorldPosToSubtile(entity.Location);
            }


            frontDoorSubtile = MapManager.WorldPosToSubtile(frontEntrance); //building.FrontDoorLocation);

            if (backEntrance.HasValue) // building.BackDoorTilePos.HasValue)
            {
                backDoorSubtile = MapManager.WorldPosToSubtile(backEntrance.Value); // building.BackDoorLocation.Value);
            }

            // get closest entrance (that isn't blocked?)
            if (RegionGraph != null && RegionGraph.Count > 0)
            {
                // we have regions.
                ushort frontDoorRegion = this.AllSubtiles[frontDoorSubtile.X][frontDoorSubtile.Y];
                ushort backDoorRegion = 0;

                if (backDoorSubtile.HasValue)
                {
                    backDoorRegion = this.AllSubtiles[backDoorSubtile.Value.X][backDoorSubtile.Value.Y];
                }

                if (frontDoorRegion == 0 && backDoorRegion == 0)
                {
                    return null; // Result.NoAccess;
                }
                else if (frontDoorRegion != 0 && backDoorRegion != 0)
                {
                    // use air distance to pick an entrance
                    if (entity != null)
                    {
                        entranceSubtile = Building.GetClosestEntrance(entitySubtile.Value, frontDoorSubtile, backDoorSubtile);
                    }
                    else
                    {
                        entranceSubtile = frontDoorSubtile;
                    }
                }
                else if (frontDoorRegion == 0 && backDoorSubtile.HasValue)
                {
                    entranceSubtile = backDoorSubtile.Value;
                }
                else if (backDoorRegion == 0)
                {
                    entranceSubtile = frontDoorSubtile;
                }
               
            }
            else
            {
                // use air distance to pick an entrance
                if (entity != null)
                {
                    entranceSubtile = Building.GetClosestEntrance(entitySubtile.Value, frontDoorSubtile, backDoorSubtile);
                }
                else
                {
                    entranceSubtile = frontDoorSubtile;
                }
            }


            return entranceSubtile;

        }*/

       /* private static Point GetClosestEntrance(Entity entity, Buildings.Building building, Point? frontDoorSubtile, Point? backDoorSubtile)
        {
            Point toSubtile;
            EntranceToUse entranceToUse;
            building.GetClosestEntrance(entity.MapPosition, out entranceToUse);

            if (entranceToUse == EntranceToUse.Front)
            {
                toSubtile = frontDoorSubtile.Value;
            }
            else
            {
                toSubtile = backDoorSubtile.Value;
            }
            return toSubtile;
        }*/

     
        protected virtual bool CanServiceRequests
        {
            get { return true; }
        }

      /*  protected virtual Dictionary<ushort, Dictionary<ushort, RegionPathFinderNodeBFS>> GetCostResults()
        {
            return AllRegionCosts;
        }*/


       /* protected void GetCost(ushort fromRegion, ushort toRegion, out float? distance, out bool fromRegionEntryExists, out bool noAccess)
        {
            Dictionary<ushort, RegionPathFinderNodeBFS> costs;

            if (!allRegionCosts.TryGetValue(fromRegion, out costs))
            {

            }
            else
            {
                RegionPathFinderNodeBFS pathfinderNode;

                if (costs.TryGetValue(toRegion, out pathfinderNode))
                {
                    float cost = pathfinderNode.G;

                    return GetDistanceFromCost(ref fromSubtile, ref toSubtile, ref distance, fromRegion, toRegion, cost);
                }
                else
                {
                    // no path between the two regions:
                    return Result.NoAccess;
                }
            }
        }*/


        /// <summary>
        /// pass the search to the terrain region map if the move map has no sectors.
        /// 
        /// TODO: take an EntityID as arg instead of Entity
        /// 
        /// to be called by agents... they may have to wait for the result.
        /// Result is in pixels!
        /// 
        /// The other method GetDistanceToEntity handles entities inside containers correctly.
        /// 
        /// WARNING! will return OK when entity location and destination is on same subtile - even if blocked! Therefore, always call another method to see if work can actually be performed at the destination!
        /// </summary>
        /// <param name="fromRegion"></param>
        /// <param name="toRegion"></param>
        /// <returns></returns>
        public virtual Result GetDistance(Entity entity, Point fromSubtile, Point toSubtile, ref float distance, bool sendMessageToEntity = true, 
            MethodID? notifyWhenFinished = null, bool registerIfNotReady = true) 
        {

            if (fromSubtile == toSubtile) // this is true for items carried by an entity
            {
                // will be considered reachable even if on a blocked subtile...
                distance = 0;
                return Result.OK;
            }

            Dictionary<ushort, RegionPathFinderNodeBFS> costs;

            // cannot service requests if base terrain regions have been recomputed, but dependents are still pending...           

            // are we ready to answer requests? (we can service requests with the old data even if we are currently computing a new region map!)
            if (CanServiceRequests)
            {
                // we have regions.
                ushort fromRegion = layers.GetRegion(fromSubtile.X, fromSubtile.Y); // this.AllSubtiles[fromSubtile.X][fromSubtile.Y];
                ushort toRegion = layers.GetRegion(toSubtile.X, toSubtile.Y);


                if (toRegion == 0)
                {   // the target is unreachable!!!
                    return Result.NoAccess;
                }
                else if (fromRegion == 0)
                {
                    // slide out/escape from a blocked area:
                    // find the closest region to slide to and get distance from that:
                    ushort closestRegion;
                    closestRegion = GetClosestRegionToBlockedSubtile(ref fromSubtile, toRegion);

                    fromRegion = closestRegion;
                    // change the psotion also, it will perhaps be used later in a stored search
                    fromSubtile = GetRegion(fromRegion).CenterInSubtiles; // regions[fromRegion].CenterInSubtiles; 
                }

                if (fromRegion == toRegion)
                {
                    // same region - use air distance:
                    distance = MapManager.subTileSize * Common.DistanceOctile(fromSubtile, toSubtile);

                    return Result.OK;
                }

               

                if (!AllRegionCosts.TryGetValue(fromRegion, out costs)) 
                {                 
                    return SearchDirectly(fromSubtile, toSubtile, ref distance);

                }
                else
                {
                    RegionPathFinderNodeBFS pathfinderNode;

                    if (costs.TryGetValue(toRegion, out pathfinderNode))
                    {
                        // cost is negative for no path...
                        float cost = pathfinderNode.G;

                        return GetApproximateDistanceFromCost(fromSubtile, toSubtile, ref distance, fromRegion, toRegion, cost);
                    }
                    else 
                    {                       
                        return SearchDirectly(fromSubtile, toSubtile, ref distance);
                    }                   
                }

                // store the original request so it can survive cleanses and redrawing of regions:
               /* RegionSearchRequest searchRequest = StoreSearchRequest(entity, ref fromSubtile, ref toSubtile, sendMessageToEntity, notifyWhenFinished);
                RegisterSearch(searchRequest);*/

            }
            else if (registerIfNotReady)
            {
                // we are not ready to start searches yet, we don't have the regions.
                // store the original request so it can survive cleanses and redrawing of regions:
                StoreSearchRequest(entity, ref fromSubtile, ref toSubtile, sendMessageToEntity, notifyWhenFinished);
            }

            return Result.Wait; // tell the agent to wait...

        }

     /*   public virtual Result GetDistance(Entity entity, Point fromSubtile, Point toSubtile, ref float distance, bool sendMessageToEntity = true, MethodID? notifyWhenFinished = null, bool doRegionSearchNow = false) //ushort fromRegion, ushort toRegion)
        {

            if (fromSubtile == toSubtile) // this is true for items carried by an entity
            { 
                // will be considered reachable even if on a blocked subtile...
                distance = 0;
                return Result.OK;
            }

            
            Dictionary<ushort, RegionPathFinderNodeBFS> costs;

            // cannot service requests if base terrain regions have been recomputed, but dependents are still pending...           

            // are we ready to answer requests? (we can service requests with the old data even if we are currently computing a new region map!)
            if (CanServiceRequests) 
            {
                // we have regions.
                ushort fromRegion = layers.GetRegion(fromSubtile.X, fromSubtile.Y); // this.AllSubtiles[fromSubtile.X][fromSubtile.Y];
                ushort toRegion = layers.GetRegion(toSubtile.X, toSubtile.Y);
                               

                if (toRegion == 0)
                {   // the target is unreachable!!!
                    return Result.NoAccess;
                }
                else if (fromRegion == 0)
                {
                    // slide out/escape from a blocked area:
                    // find the closest region to slide to and get distance from that:
                    ushort closestRegion;
                    closestRegion = GetClosestRegionToBlockedSubtile(ref fromSubtile, toRegion);

                    fromRegion = closestRegion;
                    // change the psotion also, it will perhaps be used later in a stored search
                    fromSubtile = GetRegion(fromRegion).CenterInSubtiles; // regions[fromRegion].CenterInSubtiles; 
                }

                if (fromRegion == toRegion)
                {
                    // same region - use air distance:
                    distance = MapManager.subTileSize * Common.DistanceOctile(fromSubtile, toSubtile);

                    return Result.OK;
                }

                if (!AllRegionCosts.TryGetValue(fromRegion, out costs)) //allRegionCosts.TryGetValue(fromRegion, out costs))
                {
                    Dictionary<ushort, float> outOfBandCosts;

                    if (!OutOfBandCosts.TryGetValue(fromRegion, out outOfBandCosts))
                    {
                        // TODO: use direct search by default. modify the code so the same code path is followed whether CanServiceRequests is true or not.
                        // TODO: cache the path. Later, we should give it to the AStar pathfinder.
                        if (doRegionSearchNow)
                        {
                            
                            float cost = FindPathCost(fromRegion, toRegion);

                            // cache the result:
                            Dictionary<ushort, float> toOutOfBandCosts = new Dictionary<ushort, float>();
                            toOutOfBandCosts.Add(toRegion, cost);
                            OutOfBandCosts.Add(fromRegion, toOutOfBandCosts);

                            return GetDistanceFromCost(fromSubtile, toSubtile, ref distance, fromRegion, toRegion, cost);
                        }
                    }
                    else
                    {
                        float cost;
                        if (outOfBandCosts.TryGetValue(toRegion, out cost))
                        {
                            return GetDistanceFromCost(fromSubtile, toSubtile, ref distance, fromRegion, toRegion, cost);
                        }
                        else
                        {
                            cost = FindPathCost(fromRegion, toRegion);

                            // cache the result:
                            outOfBandCosts.Add(toRegion, cost);

                            return GetDistanceFromCost(fromSubtile, toSubtile, ref distance, fromRegion, toRegion, cost);
                        }
                    }
                }
                else
                {
                    RegionPathFinderNodeBFS pathfinderNode;

                    if (costs.TryGetValue(toRegion, out pathfinderNode))
                    {
                        float cost = pathfinderNode.G;

                        return GetDistanceFromCost(fromSubtile, toSubtile, ref distance, fromRegion, toRegion, cost);
                    }
                    else
                    {
                        // no path between the two regions:
                        return Result.NoAccess;
                    }
                }

                // store the original request so it can survive cleanses and redrawing of regions:
                RegionSearchRequest searchRequest = StoreSearchRequest(entity, ref fromSubtile, ref toSubtile, sendMessageToEntity, notifyWhenFinished);

                RegisterSearch(searchRequest);

            }
            else
            {
                // we are not ready to start searches yet, we don't have the regions.
                // store the original request so it can survive cleanses and redrawing of regions:
                StoreSearchRequest(entity, ref fromSubtile, ref toSubtile, sendMessageToEntity, notifyWhenFinished);              

            }

            return Result.Wait; // tell the agent to wait...

        }*/

        private RegionSearchRequest StoreSearchRequest(Entity entity, ref Point fromSubtile, ref Point toSubtile, bool sendMessageToEntity, MethodID? notifyWhenFinished)
        {
            EntityID? entityID = (entity != null ? (EntityID?)entity.EntityID : null);
            RegionSearchRequest searchRequest = new RegionSearchRequest(entityID, fromSubtile, toSubtile, sendMessageToEntity, notifyWhenFinished);
            regionSearchRequests.Add(searchRequest.ID);
            return searchRequest;
        }

        private ushort GetClosestRegionToBlockedSubtile(ref Point fromSubtile, ushort toRegion)
        {
            Tuple<ushort, double, int> closestRegionTuple;  
            ushort closestRegion;
            if (!cachedClosestRegionToBlockedSubtile.TryGetValue(fromSubtile, out closestRegionTuple))
            {                
                closestRegion = RegionSearchPlanner.GetClosestRegionToDestination(RegionSearcher.PathInfo.CostOnly,
                    this, toRegion, 100, MapManager.SubTileToWorldPos3(fromSubtile));

                Region region = GetRegion(closestRegion);
                // Note that this distance can be quite great if there are canyons of blocked areas between the start and end.
                // the search starts at the destination, which may be on an island.
                int distance = (int)Common.DistanceOctile(fromSubtile, region.CenterInSubtiles); 


                //cachedClosestRegionToBlockedSubtile.Add(fromSubtile, closestRegion);
                cachedClosestRegionToBlockedSubtile.Add(fromSubtile, new Tuple<ushort, double, int>(closestRegion, The.Sim.TotalUnPausedGameTimeInSeconds, distance));
            }
            else
            {
                closestRegion = closestRegionTuple.Item1;

                // May 2016: there was an error with an outdated region in the cache. so do a sanity check here... if it fails, throw in test mode
#if DEBUG || PROFILE
                Region region = GetRegion(closestRegion);
                if (region == null || (int)Common.DistanceOctile(fromSubtile, region.CenterInSubtiles) != closestRegionTuple.Item3) // Common.DistanceOctile(fromSubtile, region.CenterInSubtiles) > 46)
                {
                    throw new Exception("Cache error? The distance should not change, the cache should be cleared on every redraw.");
                }
#endif
            }

            return closestRegion;
        } 

      /*  private float FindPathCost(ushort from, ushort to)
        {            
            List<RegionPathFinderNodeAStar> path = null;
            float cost = 0f;
            RegionSearchPlanner.GetPathDirectly(RegionSearcher.PathInfo.CostOnly, this, from, to, ref path, ref cost);


            return cost;
        }*/


       /* private float FindPathCost(ushort from, ushort to)
        {
            RegionSearchPlanner newPlanner = new RegionSearchPlanner(this, from);

            List<RegionPathFinderNodeAStar> path = null;
            float cost = 0f;
            newPlanner.GetPathDirectly(RegionSearch.PathInfo.CostOnly, this, from, to, ref path, ref cost);

            newPlanner.Destroy();

            return cost;
        }*/

        /// <summary>
        /// call this instead of RegionGraph in order to navigate the layers correctly.
        /// 
        /// 
        /// </summary>
        /// <param name="fromRegion"></param>
        /// <param name="connections"></param>
        /// <returns></returns>
        public bool GetConnectors(ushort fromRegion, out Dictionary<ushort, RegionEdge> connections)
        {
            if (fromRegion < RegionMap.BaseLayerRegionColors)
            {

                if (BottomBlockedEdges != null)
                {
                    // there are connections to upper layer. copy if needed 

                    Dictionary<ushort, RegionEdge> copiedConnections = null;

                    Dictionary<ushort, RegionEdge> baseConnections;
                    Dictionary<ushort, RegionEdge> bottomConnections;

                    // overrrding region graph contains bottom region keys too, for the edge regions!
                    RegionGraph.TryGetValue(fromRegion, out baseConnections); // bottom to upper layer edges
                   

                    BottomRegionGraph.TryGetValue(fromRegion, out bottomConnections); // look up in bottom layer too
                    
                    // combine them:
                    if (baseConnections != null && bottomConnections != null)
                    {
                        copiedConnections = new Dictionary<ushort, RegionEdge>(baseConnections);
                            
                        foreach (var item in bottomConnections)
                        {
                            copiedConnections.Add(item.Key, item.Value);
                        }
                    }
                    else 
                    {
                        baseConnections = baseConnections ?? bottomConnections;
                    }


                    // append:
                    /*   if (regionMap.BottomToUpperLayerConnectors != null)
                     {
                         Dictionary<ushort, RegionEdge> layerConnectors;
                         if (regionMap.BottomToUpperLayerConnectors.TryGetValue(fromRegion, out layerConnectors) && layerConnectors.Count > 0)
                         {
                             foreach (var item in layerConnectors)
                             {		 	
                                 if (copiedConnections == null)
                                 {
                                     copiedConnections = new Dictionary<ushort, RegionEdge>(baseConnections);
                                 }
                            
                               
                                 copiedConnections.Add(item.Key, item.Value);
                             }                            
                         }
                     }*/

                    // check blockers:
                    if (BottomBlockedEdges != null)
                    {
                        HashSet<ushort> blockedDestinations;
                        if (BottomBlockedEdges.TryGetValue(fromRegion, out blockedDestinations) && blockedDestinations.Count > 0)
                        {
                            foreach (var item in blockedDestinations)
                            {
                                if (copiedConnections == null)
                                {
                                    copiedConnections = new Dictionary<ushort, RegionEdge>(baseConnections);
                                }

                                copiedConnections.Remove(item);
                            }
                        }
                    }

                    connections = copiedConnections ?? baseConnections;

                    return connections != null && connections.Count > 0;

                }
                else
                {
                    return BottomRegionGraph.TryGetValue(fromRegion, out connections); // look up in bottom layer
                }
            }
            else
            {
                // look up in overriding layer
                return RegionGraph.TryGetValue(fromRegion, out connections);
            }
        }


        protected bool IsSameOrNeighbour(ushort fromRegion, ushort toRegion)
        {
            Dictionary<ushort, RegionEdge> connections;
            if (GetConnectors(fromRegion, out connections))
            {
                if (connections.ContainsKey(toRegion))
                {
                    return true;
                }

            }

            return false;

            /*
            //Dictionary<ushort, RegionEdge> connections;
            if (RegionGraph.TryGetValue(fromRegion, out connections) && connections.ContainsKey(toRegion))
            {
                return true;
            }

            return false;*/
        }

        /// <summary>
        /// examines the endpoints and approximates the distance
        /// special cases when in same region, or neighbouring regions: use air distance between points.
        /// </summary>
        /// <param name="fromSubtile"></param>
        /// <param name="toSubtile"></param>
        /// <param name="distance"></param>
        /// <param name="fromRegion"></param>
        /// <param name="toRegion"></param>
        /// <param name="cost"></param>
        /// <returns></returns>
        private Result GetApproximateDistanceFromCost(Point fromSubtile, Point toSubtile, ref float distance, ushort fromRegion, ushort toRegion, float cost)
        {
            if (Common.IsGreaterThanOrEqual(cost, 0d)) // cost >= 0)
            {
                // special cases when in same region, or neighbouring regions: use air distance between points.

                if (IsSameOrNeighbour(fromRegion, toRegion))
                {
                    distance = MapManager.subTileSize * Common.DistanceOctile(fromSubtile, toSubtile);
                    return Result.OK;
                }
                else
                {
                    
                    distance = cost;
                    return Result.OK;
                }
            }
            else
            {
                // no path between the two regions:
                return Result.NoAccess;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchRequest"></param>
        private void RegisterSearch(RegionSearchRequest searchRequest, bool doBFS = false)
        {
            bool searchEndpointsMoved;
            ushort fromRegion, toRegion;
            Point fromSubtile = searchRequest.FromSubtile; 
            Point toSubtile = searchRequest.ToSubtile;

            if (!PrepareSearch(/*searchRequest.Entity,*/ ref fromSubtile, ref toSubtile, out fromRegion, out toRegion, out searchEndpointsMoved))
            {
                Entity entity = null;
                if (searchRequest.Entity.HasValue)
                {
                    entity = Entity.FindByID(searchRequest.Entity.Value);
                }

                RemoveSearchAndNotify(entity, searchRequest, 0f, Result.NoAccess);
                return;
            }

            if (searchEndpointsMoved)
            {
                searchRequest.AddLog(string.Format("Start and destinations moved, from {0} - {1} to {2} - {3}", searchRequest.FromSubtile, searchRequest.ToSubtile, fromSubtile, toSubtile));

                searchRequest.FromSubtile = fromSubtile;
                searchRequest.ToSubtile = toSubtile;              
            }

            RegionSearchPlanner regionSearchPlanner = null;
            CyclableID regionSearchPlannerID;

            if (doBFS == false)
            {
                // AStar
                // did we already start this search?
                Dictionary<ushort, CyclableID> planners;
                if (regionAStarPlanners.TryGetValue(fromRegion, out planners))
                {
                    if (planners.TryGetValue(toRegion, out regionSearchPlannerID))
                    {
                        regionSearchPlanner = (RegionSearchPlanner)LookUp<ICyclable, CyclableID>.FindByID(regionSearchPlannerID);
                    }
                }

                if (regionSearchPlanner == null)
                {
                    regionSearchPlanner = new RegionSearchPlanner(this, fromRegion, toRegion);
                }
            }
            else
            {
                // searches all region nodes in BFS.

                // did we already start searches from this region?
                if (regionBFSPlanners.TryGetValue(fromRegion, out regionSearchPlannerID))
                {
                    regionSearchPlanner = (RegionSearchPlanner)LookUp<ICyclable, CyclableID>.FindByID(regionSearchPlannerID);
                }

                if (regionSearchPlanner == null)
                {
                    regionSearchPlanner = new RegionSearchPlanner(this, fromRegion);
                }
            }

            regionSearchPlanner.AddSearchRequest(searchRequest);
        }

        /*

        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchRequest"></param>
        private void RegisterSearch(RegionSearchRequest searchRequest)
        {
            ushort toRegion = layers.GetRegion(searchRequest.ToSubtile);
            if (toRegion == 0)
            {   // the target is unreachable!!!
                // end here:
                Entity entity = null;
                if (searchRequest.Entity.HasValue)
                {
                    entity = Entity.FindByID(searchRequest.Entity.Value);
                }
                RemoveSearchAndNotify(entity, searchRequest, 0f, Result.NoAccess);
                return;
            }

            ushort fromRegion = layers.GetRegion(searchRequest.FromSubtile);

       
            if (fromRegion == 0)
            {
                // slide out/escape from a blocked area:
                // find the closest region to slide to and get distance from that:
                AssertRegionGraph(false);

                ushort closestRegion;
                closestRegion = GetClosestRegionToBlockedSubtile(ref searchRequest.FromSubtile, toRegion);

               
                //fromSubtile = Regions[fromRegion].CenterInSubtiles; // change the psotion also, it will perhaps be used later in a stored search

                Point newFrom = GetRegion(closestRegion).CenterInSubtiles; // change the psotion also, it will perhaps be used later in a stored search
                Point newTo = GetRegion(toRegion).CenterInSubtiles;

                Debug.Assert(newFrom != searchRequest.FromSubtile, "Search for closest region failed...");// ERROR: returns the bottom region even if it is blocked by an upper sector

                Debug.Assert(GetRegion(newFrom.X, newFrom.Y) != null, "No region at center?");

                searchRequest.AddLog(string.Format("Start and destinations moved, from {0} - {1} to {2} - {3}", searchRequest.FromSubtile, searchRequest.ToSubtile, newFrom, newTo));
                
                fromRegion = closestRegion;
                searchRequest.FromSubtile = newFrom;
                searchRequest.ToSubtile = newTo;
                
            }

            if (ID == (CyclableID)170 //&& fromRegion == 523
                && The.Sim.TotalUnPausedGameTimeInSeconds > 35)
            {

            }

            RegionSearchPlanner regionSearchPlanner = null;
            CyclableID regionSearchPlannerID;
            // did we already start searches from this region?
            if (regionSearchPlanners.TryGetValue(fromRegion, out regionSearchPlannerID))
            {
                regionSearchPlanner = (RegionSearchPlanner)LookUp<ICyclable, CyclableID>.FindByID(regionSearchPlannerID);
            }

            if (regionSearchPlanner == null)
            {
                regionSearchPlanner = new RegionSearchPlanner(this, fromRegion);             
            }

            regionSearchPlanner.AddSearchRequest(searchRequest);
        }*/

        /// <summary>
        /// exxamine if the search is valid,  and / or modify the endpoints
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="FromSubtile"></param>
        /// <param name="ToSubtile"></param>
        /// <param name="searchRequest"></param>
        /// <returns></returns>
        private bool PrepareSearch(ref Point FromSubtile, ref Point ToSubtile, out ushort fromRegion, out ushort toRegion, out bool searchEndpointsMoved)
        {
            searchEndpointsMoved = false;

            toRegion = layers.GetRegion(ToSubtile);
            fromRegion = layers.GetRegion(FromSubtile);

            if (toRegion == 0)
            {   
                // the target is unreachable!!!
                // end here               

               // return Result.NoAccess;
                return false;
            }        
            else if (fromRegion == 0)
            {
                // slide out/escape from a blocked area:
                // find the closest region to slide to and get distance from that:
                AssertRegionGraph(false);

                ushort closestRegion;
                closestRegion = GetClosestRegionToBlockedSubtile(ref FromSubtile, toRegion);

                Point newFrom = GetRegion(closestRegion).CenterInSubtiles; // change the psotion also, it will perhaps be used later in a stored search
                Point newTo = GetRegion(toRegion).CenterInSubtiles;


/*
                int layerIndex;
                SubtileSector fromSector = GetSector(FromSubtile, out layerIndex);

                // assert that the closestRegion is not on the overidden sector if fromRegion is on the upper layer:
                SubtileSector overriddenSector = layers.GetOverriddenSector(fromSector.Coords.X, fromSector.Coords.Y);
                if (layerIndex == 1 && overriddenSector != fromSector)
                {
                    if (overriddenSector.GetRegions().Contains(closestRegion))
                    {
                        Debug.Assert(false, "Search for closest region returned a region that should be overridden... ");// ERROR: returns the bottom region even if it is blocked by an upper sector
                    }
                }              
                */


                Debug.Assert(newFrom != FromSubtile, "Search for closest region failed...");// ERROR: returns the bottom region even if it is blocked by an upper sector

                Debug.Assert(GetRegion(newFrom.X, newFrom.Y) != null, "No region at center?");

                searchEndpointsMoved = true;

              //  searchRequest.AddLog(string.Format("Start and destinations moved, from {0} - {1} to {2} - {3}", FromSubtile, ToSubtile, newFrom, newTo));

                fromRegion = closestRegion;
              /*  searchRequest.FromSubtile = newFrom;
                searchRequest.ToSubtile = newTo;
                */

                FromSubtile = newFrom;
                ToSubtile = newTo;

            }

            return true;

        }

        public virtual RegionPathID? GetRegionPath(Point startSubtile, Point destinationSubtile)
        {
            RegionPathID? highLevelPath = null;

            ushort startRegion = GetRegionColor(startSubtile.X, startSubtile.Y);
            ushort destinationRegion = GetRegionColor(destinationSubtile.X, destinationSubtile.Y);

            Dictionary<ushort, RegionPathID> paths; //RegionPathFinderNodeAStar>> paths;
            if (RegionPaths.TryGetValue(startRegion, out paths))
            {
                RegionPathID pathID;
                if (paths.TryGetValue(destinationRegion, out pathID))
                {
                    highLevelPath = pathID;
                }
            }

            return highLevelPath;

        }

        /*
        public virtual List<RegionPathFinderNodeAStar> GetRegionPath(Point startSubtile, Point destinationSubtile)
        {
            List<RegionPathFinderNodeAStar> highLevelPath = null;

            ushort startRegion = GetRegionColor(startSubtile.X, startSubtile.Y);
            ushort destinationRegion = GetRegionColor(destinationSubtile.X, destinationSubtile.Y);

            Dictionary<ushort, List<RegionPathNode>> paths; //RegionPathFinderNodeAStar>> paths;
            if (RegionPaths.TryGetValue(startRegion, out paths))
            {
                paths.TryGetValue(destinationRegion, out highLevelPath);
            }

            return highLevelPath;

        }*/

        private Result SearchDirectly(Point FromSubtile, Point ToSubtile, ref float distance)
        {
            bool searchEndpointsMoved;
            ushort fromRegion, toRegion;
            if (!PrepareSearch(ref FromSubtile, ref ToSubtile, out fromRegion, out toRegion, out searchEndpointsMoved))
            {
                return Result.NoAccess;
            }
                      

            RegionPath path = null;
            float cost = 0f;
            RegionSearchPlanner.GetPathDirectly(RegionSearcher.PathInfo.FullPath, this, fromRegion, toRegion, ref path, ref cost);

            Result result = CachePathResult(FromSubtile, ToSubtile, fromRegion, toRegion, path);

            if (result == Result.OK)
            {
                GetApproximateDistanceFromCost(FromSubtile, ToSubtile, ref distance,
                       fromRegion, toRegion, path.Cost);
            }

            return result;

        }

        private Result CachePathResult(Point fromSubtile, Point toSubtile, ushort fromRegion, ushort toRegion, RegionPath path)
        {
            float cost;
            if (path != null
                && Common.IsGreaterThanOrEqual(path.Cost, 0))
            {
                cost = path.Cost; 

                // cache the results:               
                Common.AddToNestedDictionary(AllRegionCosts, fromRegion, toRegion, new RegionPathFinderNodeBFS(cost, toRegion));
                Common.AddToNestedDictionary(RegionPaths, fromRegion, toRegion, path.ID);
                              

                return Result.OK;
            }
            else
            {
                // store no path info
                cost = -1f;
                Common.AddToNestedDictionary(AllRegionCosts, fromRegion, toRegion, new RegionPathFinderNodeBFS(cost, toRegion));

                return Result.NoAccess;
            }
        }

      /*  private float GetPathCost(List<RegionPathFinderNodeAStar> path)
        {
            return path[path.Count - 1].G;
        }*/


        /// <summary>
        /// 
        /// </summary>
        /// <param name="regionSearchPlanner"></param>
      /*  public void PlannerSearchFinished(RegionSearchPlanner regionSearchPlanner)
        {           
            // store the result:
          
            // if this assertion fails, could it be because of a bug where the search does not get restarted after the regions have changed in either the overridden or the bottom layer?
            System.Diagnostics.Debug.Assert(regions.ContainsKey(regionSearchPlanner.Start) || BottomRegions.ContainsKey(regionSearchPlanner.Start), "unknown region?");

            if (regionSearchPlanner.IsBFS)
            {
                // replace any single path results if needed:  
                // currently not used... we are doing AStar path searches instead.
                AllRegionCosts[regionSearchPlanner.Start] = regionSearchPlanner.GetBFSResult();
                //AllRegionCosts.Add(regionSearchPlanner.Start, regionSearchPlanner.GetBFSResult());


                // delete the planner... it will not be used again before the regions are redrawn anyway.
                regionBFSPlanners.Remove(regionSearchPlanner.Start);

                // send the result back to each waiting agent:       
                // there are seldom more than 1 or 2 agents...
                foreach (RegionSearchRequestID searchRequestID in regionSearchPlanner.SearchRequests)
                {
                    RegionSearchRequest searchRequest = LookUp<RegionSearchRequest, RegionSearchRequestID>.FindByID(searchRequestID);
                    SearchFinished(searchRequest);
                }
            }
            else
            {
                regionAStarPlanners.Remove(regionSearchPlanner.ID);

                // send the result back to each waiting agent:       
                // there are seldom more than 1 or 2 agents...
                foreach (RegionSearchRequestID searchRequestID in regionSearchPlanner.SearchRequests)
                {
                    RegionSearchRequest searchRequest = LookUp<RegionSearchRequest, RegionSearchRequestID>.FindByID(searchRequestID);

                    // return CachePathResult(ref FromSubtile, ref ToSubtile, ref distance, fromRegion, toRegion, path, cost);
                    CachePathResult(ref searchRequest.FromSubtile, ref searchRequest.ToSubtile, ref distance, regionSearchPlanner.Start, 
                        regionSearchPlanner.Destination, regionSearchPlanner. path, cost);

                    SearchFinished(searchRequest);
                }              
            }            
            
        }*/


        public void PlannerBFSFinished(RegionSearchPlanner regionSearchPlanner, Dictionary<ushort, RegionPathFinderNodeBFS> result)
        {
            // store the result:

            // if this assertion fails, could it be because of a bug where the search does not get restarted after the regions have changed in either the overridden or the bottom layer?
            System.Diagnostics.Debug.Assert(regions.ContainsKey(regionSearchPlanner.Start) || BottomRegions.ContainsKey(regionSearchPlanner.Start), "unknown region?");

            // replace any single path results if needed:  
            // currently not used... we are doing AStar path searches instead.
            AllRegionCosts[regionSearchPlanner.Start] = result;
            //AllRegionCosts.Add(regionSearchPlanner.Start, regionSearchPlanner.GetBFSResult());


            // delete the planner... it will not be used again before the regions are redrawn anyway.
            regionBFSPlanners.Remove(regionSearchPlanner.Start);

            // send the result back to each waiting agent:       
            // there are seldom more than 1 or 2 agents...
            foreach (RegionSearchRequestID searchRequestID in regionSearchPlanner.SearchRequests)
            {
                RegionSearchRequest searchRequest = LookUp<RegionSearchRequest, RegionSearchRequestID>.FindByID(searchRequestID);
                SearchFinished(searchRequest);
            }

        }

        public void PlannerAStarFinished(RegionSearchPlanner regionSearchPlanner, RegionPath result)
        {
           
            // if this assertion fails, could it be because of a bug where the search does not get restarted after the regions have changed in either the overridden or the bottom layer?
            System.Diagnostics.Debug.Assert(regions.ContainsKey(regionSearchPlanner.Start) || BottomRegions.ContainsKey(regionSearchPlanner.Start), "unknown region?");

            Common.RemoveFromNestedDictionary(regionAStarPlanners, regionSearchPlanner.Start, regionSearchPlanner.Destination);


            // send the result back to each waiting agent:       
            // there are seldom more than 1 or 2 agents...
            foreach (RegionSearchRequestID searchRequestID in regionSearchPlanner.SearchRequests)
            {
                RegionSearchRequest searchRequest = LookUp<RegionSearchRequest, RegionSearchRequestID>.FindByID(searchRequestID);
 
                // store the result:
                // return CachePathResult(ref FromSubtile, ref ToSubtile, ref distance, fromRegion, toRegion, path, cost);
                CachePathResult(searchRequest.FromSubtile, searchRequest.ToSubtile, regionSearchPlanner.Start,
                    regionSearchPlanner.Destination, result);

                SearchFinished(searchRequest);
            }

        }


        public void AddSearchPlanner(RegionSearchPlanner planner)
        {
            if (planner.IsBFS)
            {
                regionBFSPlanners.Add(planner.Start, planner.ID);
            }
            else
            {
                Common.AddToNestedDictionary(regionAStarPlanners, planner.Start, planner.Destination, planner.ID);
               // regionAStarPlanners.Add(planner.ID);
            }
        }

        private void SearchFinished(RegionSearchRequest searchRequest)
        {
            float distance = -1f;
            Result result = Result.NoAccess; 
          
            Entity entity = null;
            if (searchRequest.Entity.HasValue)
            {
                entity = Entity.FindByID(searchRequest.Entity.Value);
            }

            if (entity != null)
            {               
                result = GetDistance(entity, searchRequest.FromSubtile, searchRequest.ToSubtile, ref distance);
                Debug.Assert(result != Result.Wait);
            }

            RemoveSearchAndNotify(entity, searchRequest, distance, result);
        }

        private void RemoveSearchAndNotify(Entity entity, RegionSearchRequest searchRequest, float distance, Result result)
        {
            this.regionSearchRequests.Remove(searchRequest.ID);

            searchRequest.Notify(entity, result, distance);

            searchRequest.Destroy();

        }

        /// <summary>
        /// remove the regions in the area we want to repaint. all connections are removed here, the color is recycled.
        /// color references on the subtile map should already have been painted over with a 0 at this point!
        /// </summary>
        /// <param name="regionsToRemove"></param>
        protected virtual void RemoveRegions() //HashSet<Region> regionsToRemove)
        {         

            AddLog("RemoveRegions " + regionsToRemove.Count);

            foreach (Region region in regionsToRemove)
            {
                RemoveRegion(region);                         
            }

            regionsToRemove.Clear();
        }

      /*  protected void RemoveRegion(ushort region)
        {
            RemoveRegion(regions[region]);
        }*/

        protected void RemoveRegion(Region region)
        {
           // AddLog("Removed region " + region.Color);

            newRegions.Remove(region.Color);

            Debug.Assert(!UnusedRegionColors.Contains(region.Color), "stored duplicate color...");

            UnusedRegionColors.Enqueue(region.Color); // store the color so we don't run out...

            // remove the region graph connections:
            Dictionary<ushort, RegionEdge> connectionsFromDirtyRegion;

            if (newRegionGraph.TryGetValue(region.Color, out connectionsFromDirtyRegion)) // beware of unconnected region islands
            {
                Dictionary<ushort, RegionEdge> secondLinkConnections;

                foreach (KeyValuePair<ushort, RegionEdge> kvp in connectionsFromDirtyRegion)
                {
                    // the key is the region the edge is going to!

                    // remove the edge in reverse direction
                    if (newRegionGraph.TryGetValue(kvp.Value.ToRegion, out secondLinkConnections))
                    {
                        secondLinkConnections.Remove(region.Color);
                    }
                }

                newRegionGraph.Remove(region.Color);
            }
        }

        


        private void ScanTopLeftCornerOfSector(SubtileSector thisSector, SubtileSector thisOveriddenSector, Point otherSectorCoords)
        {
            SubtileSector otherSector = layers.GetUnfinishedSector(otherSectorCoords);

            Point thisRegionPoint = new Point(0, 0);
            ushort centerRegion = thisSector.GetRegionInProgress(thisRegionPoint);

          /*  if (centerRegion > 0)
            {*/
                int sectorEnd = targetLayer.SectorSize - 1;

                ushort otherRegion = otherSector.GetRegionInProgress(sectorEnd, sectorEnd);
                if (otherRegion > 0)
                {
                    AddEdgesOrConnectors(otherRegion, centerRegion);

                    if (thisOveriddenSector != null)
                    {
                        ushort thisOverriddenRegion = thisOveriddenSector.GetRegion(thisRegionPoint);

                        BlockBottomConnection(thisOverriddenRegion, otherRegion);
                    }
                }

           // }
        }

        private void ScanTopRightCornerOfSector(SubtileSector thisSector, SubtileSector thisOveriddenSector, Point otherSectorCoords)
        {
            SubtileSector otherSector = layers.GetUnfinishedSector(otherSectorCoords);

            int sectorEnd = targetLayer.SectorSize - 1;
            Point thisRegionPoint = new Point(sectorEnd, 0);
            ushort centerRegion = thisSector.GetRegionInProgress(thisRegionPoint);

           /* if (centerRegion > 0)
            {*/
                ushort otherRegion = otherSector.GetRegionInProgress(0, sectorEnd);
                if (otherRegion > 0)
                {
                    AddEdgesOrConnectors(otherRegion, centerRegion);

                    if (thisOveriddenSector != null)
                    {
                        ushort thisOverriddenRegion = thisOveriddenSector.GetRegion(thisRegionPoint);

                        BlockBottomConnection(thisOverriddenRegion, otherRegion);
                    }
                }
           // }
        }

        private void ScanBottomRightCornerOfSector(SubtileSector thisSector, SubtileSector thisOveriddenSector, Point otherSectorCoords)
        {
            SubtileSector otherSector = layers.GetUnfinishedSector(otherSectorCoords);

            int sectorEnd = targetLayer.SectorSize - 1;
            Point thisRegionPoint = new Point(sectorEnd, sectorEnd);
            ushort centerRegion = thisSector.GetRegionInProgress(thisRegionPoint);

           /* if (centerRegion > 0)
            {*/
                ushort otherRegion = otherSector.GetRegionInProgress(0, 0);
                if (otherRegion > 0)
                {
                    AddEdgesOrConnectors(otherRegion, centerRegion);

                    if (thisOveriddenSector != null)
                    {
                        ushort thisOverriddenRegion = thisOveriddenSector.GetRegion(thisRegionPoint);

                        BlockBottomConnection(thisOverriddenRegion, otherRegion);
                    }
                }
          //  }
        }

        private void ScanBottomLeftCornerOfSector(SubtileSector thisSector, SubtileSector thisOveriddenSector, Point otherSectorCoords)
        {
            SubtileSector otherSector = layers.GetUnfinishedSector(otherSectorCoords);

            int sectorEnd = targetLayer.SectorSize - 1;
            Point thisRegionPoint = new Point(0, sectorEnd);
            ushort centerRegion = thisSector.GetRegionInProgress(thisRegionPoint);

           /* if (centerRegion > 0)
            {*/
                ushort otherRegion = otherSector.GetRegionInProgress(sectorEnd, 0);
                if (otherRegion > 0)
                {
                    AddEdgesOrConnectors(otherRegion, centerRegion);

                    if (thisOveriddenSector != null)
                    {
                        ushort thisOverriddenRegion = thisOveriddenSector.GetRegion(thisRegionPoint);

                        BlockBottomConnection(thisOverriddenRegion, otherRegion);
                    }
                }
         //   }
        }


        enum AdjacentSector {Left, Right, Over, Under }
        private void ScanVerticalSideForEdges(SubtileSector thisSector, SubtileSector thisOveriddenSector, AdjacentSector adjacentSectorDir) 
        {
            
             int thisSectorEdge;
             int otherSectorEdge;             
             SubtileSector otherSector;
            
             if (adjacentSectorDir == AdjacentSector.Right)
             {
                 thisSectorEdge = thisSector.SubtileArea.Width - 1; // targetLayer.SectorSize - 1;                 
                 Point otherSectorCoords = new Point(thisSector.Coords.X + 1, thisSector.Coords.Y);
                 otherSector = layers.GetUnfinishedSector(otherSectorCoords);
                 otherSectorEdge = 0;
             }
             else
             {
                 thisSectorEdge = 0;                 
                 Point otherSectorCoords = new Point(thisSector.Coords.X - 1, thisSector.Coords.Y);
                 otherSector = layers.GetUnfinishedSector(otherSectorCoords);
                 otherSectorEdge = otherSector.SubtileArea.Width - 1; // targetLayer.SectorSize - 1;
             }

             

             int maxY = thisSector.SubtileArea.Height - 1;
             for (int y = 0; y < maxY /* targetLayer.SectorSize - 1*/; y++)
             {
                 if (y == maxY - 1)
                 {

                 }

                 ushort thisRegionUpper = thisSector.GetRegionInProgress(thisSectorEdge, y);
                 ushort thisRegionLower = thisSector.GetRegionInProgress(thisSectorEdge, y + 1);

                 ushort otherRegionUpper = otherSector.GetRegionInProgress(otherSectorEdge, y);
                 ushort otherRegionLower = otherSector.GetRegionInProgress(otherSectorEdge, y + 1);

                
                 if (thisOveriddenSector != null)
                 {                    
                     ushort thisOverriddenRegionUpper = thisOveriddenSector.GetRegion(thisSectorEdge, y);
                     ushort thisOverriddenRegionLower = thisOveriddenSector.GetRegion(thisSectorEdge, y + 1);
 
                     // create blockers, if needed:                     
                     BlockBottomConnection(thisOverriddenRegionUpper, otherRegionUpper);
                     BlockBottomConnection(thisOverriddenRegionUpper, otherRegionLower);
                     BlockBottomConnection(thisOverriddenRegionLower, otherRegionUpper);
                 }
                
                 // connect 3 lines (one horizontal, 2 diagonal):
                 AddEdgesOrConnectors(thisRegionUpper, otherRegionUpper); // horizontal
                 AddEdgesOrConnectors(thisRegionUpper, otherRegionLower); // diagonal
                 AddEdgesOrConnectors(thisRegionLower, otherRegionUpper); // diagonal                
             }

            /*
            ushort[] fromXColumn = newAllSubtiles[fromX];
            ushort topLeftRegion = fromXColumn[fromY]; // top left of subtile quad
            //  ushort toRegion = allNodes[toX, toY];

            if (topLeftRegion > 0)
            {

                ushort[] toXColumn = newAllSubtiles[toX];
                ushort topRightRegion = toXColumn[fromY]; // top right of quad   
                if (topRightRegion > 0)
                {
                    regionMap.AddEdgesIfNotExists(topLeftRegion, topRightRegion);
                }

                ushort bottomRightRegion = toXColumn[toY];
                if (bottomRightRegion > 0)
                {
                    regionMap.AddEdgesIfNotExists(topLeftRegion, bottomRightRegion);
                }

                ushort bottomLeftRegion = fromXColumn[toY];
                if (bottomLeftRegion > 0)
                {
                   // regionMap.AddEdgesIfNotExists(topLeftRegion, bottomLeftRegion);

                    if (topRightRegion > 0)
                    {
                        AddEdgesOrConnectors(sector, overiddenSector, bottomLeftRegion, topRightRegion);
                    }
                }

            }*/
        }

        private void ScanHorizontalSideForEdges(SubtileSector thisSector, SubtileSector thisOveriddenSector, AdjacentSector adjacentSectorDir)
        {

            int thisSectorEdge;
            int otherSectorEdge;
           // Point otherSectorCoords;
            SubtileSector otherSector;
            if (adjacentSectorDir == AdjacentSector.Under)
            {
                thisSectorEdge = thisSector.SubtileArea.Height - 1; // targetLayer.SectorSize - 1;               
                Point otherSectorCoords = new Point(thisSector.Coords.X, thisSector.Coords.Y + 1);
                otherSector = layers.GetUnfinishedSector(otherSectorCoords);
                otherSectorEdge = 0;
            }
            else
            {
                thisSectorEdge = 0;                
                Point otherSectorCoords = new Point(thisSector.Coords.X, thisSector.Coords.Y - 1);
                otherSector = layers.GetUnfinishedSector(otherSectorCoords);
                otherSectorEdge = otherSector.SubtileArea.Height - 1; // targetLayer.SectorSize - 1; 
            }

          
            int maxX = thisSector.SubtileArea.Width - 1; /* targetLayer.SectorSize - 1*/
            for (int x = 0; x < maxX ; x++)
            {
                ushort thisRegionLeft = thisSector.GetRegionInProgress(x, thisSectorEdge);
                ushort thisRegionRight = thisSector.GetRegionInProgress(x + 1, thisSectorEdge);

                ushort otherRegionLeft = otherSector.GetRegionInProgress(x, otherSectorEdge);
                ushort otherRegionRight = otherSector.GetRegionInProgress(x + 1, otherSectorEdge);

                ushort? thisOverriddenRegionLeft = null;
                ushort? thisOverriddenRegionRight = null;
                if (thisOveriddenSector != null)
                {
                    thisOverriddenRegionLeft = thisOveriddenSector.GetRegion(x, thisSectorEdge);
                    thisOverriddenRegionRight = thisOveriddenSector.GetRegion(x + 1, thisSectorEdge);

                    // create blockers, if needed:
                    BlockBottomConnection(thisOverriddenRegionLeft.Value, otherRegionLeft); 
                    BlockBottomConnection(thisOverriddenRegionLeft.Value, otherRegionRight);
                    BlockBottomConnection(thisOverriddenRegionRight.Value, otherRegionLeft);
                }

                // connect 3 lines (one vertical, 2 diagonal):
                AddEdgesOrConnectors(thisRegionLeft, otherRegionLeft); // vertical
                AddEdgesOrConnectors(thisRegionLeft, otherRegionRight); // diagonal
                AddEdgesOrConnectors(thisRegionRight, otherRegionLeft); // diagonal                
            }
        }

        protected virtual void BlockBottomConnection(ushort overriddenRegion, ushort otherRegion)
        {


        }

        protected virtual void AddEdgesOrConnectors(ushort region1, ushort region2)
        {


        }

        /// <summary>
        /// add connectors between layers.
        /// don't modify the terrain graph. instead, make a separate connector collection that belongs to the dependent map.
        /// the dependent map graph is OK to modify. Add edges, marked with a flag...
        /// </summary>
    /*    protected virtual void AddEdgesOrConnectors(SubtileSector sector, SubtileSector overriddenSector, ushort topLayerRegion, ushort otherRegion)
        {

        }*/

    /*    protected void ScanLeftSideOfSector(SubtileSector thisSector, SubtileSector thisOveriddenSector, Point otherSectorCoords)
        {

            SubtileSector leftSector = layers.GetSector(otherSectorCoords);

            int rightEdge = targetLayer.SectorSize - 1;

            // connect regions in different sectors only:
            for (int y = 0; y < targetLayer.SectorSize; y++)
            {
                ushort rightRegion = thisSector.GetRegionInProgress(0, y);
               
                if (rightRegion > 0)
                {
                    ushort leftRegion = leftSector.GetRegionInProgress(rightEdge, y);
                    if (leftRegion > 0)
                    {
                        AddEdgesOrConnectors(rightRegion, leftRegion);

                      //  AddEdgesIfNotExists(leftRegion, rightRegion);
                    }
                }
            }
        }

        private void ScanRightSideOfSector(SubtileSector sector, SubtileSector overiddenSector, Point otherSectorCoords)
        {
            SubtileSector rightSector = layers.GetSector(otherSectorCoords);

            int rightEdge = targetLayer.SectorSize - 1;

            // connect regions in different sectors only:
            for (int y = 0; y < targetLayer.SectorSize; y++)
            {
                ushort leftRegion = sector.GetRegionInProgress(rightEdge, y);

                if (leftRegion > 0)
                {
                    ushort rightRegion = rightSector.GetRegionInProgress(0, y);
                    if (rightRegion > 0)
                    {
                        AddEdgesOrConnectors(leftRegion, rightRegion);
                    }
                }
            }

        }*/

    /*    private void ScanTopSideOfSector(SubtileSector sector, SubtileSector overiddenSector, Point otherSectorCoords)
        {
            SubtileSector topSector = layers.GetSector(otherSectorCoords);

            int bottomEdge = targetLayer.SectorSize - 1;

            // connect regions in different sectors only:
            for (int x = 0; x < targetLayer.SectorSize; x++)
            {
                ushort bottomRegion = sector.GetRegionInProgress(x, 0);

                if (bottomRegion > 0)
                {
                    ushort topRegion = topSector.GetRegionInProgress(x, bottomEdge);
                    if (topRegion > 0)
                    {
                        AddEdgesOrConnectors(bottomRegion, topRegion);
                    }
                }
            }
        }

        private void ScanBottomSideOfSector(SubtileSector sector, SubtileSector overiddenSector, Point otherSectorCoords)
        {
            SubtileSector bottomSector = layers.GetSector(otherSectorCoords);

            int bottomEdge = targetLayer.SectorSize - 1;

            // connect regions in different sectors only:
            for (int x = 0; x < targetLayer.SectorSize; x++)
            {
                ushort bottomRegion = sector.GetRegionInProgress(x, bottomEdge);

                if (bottomRegion > 0)
                {
                    ushort topRegion = bottomSector.GetRegionInProgress(x, 0);
                    if (topRegion > 0)
                    {
                        AddEdgesOrConnectors(bottomRegion, topRegion);
                    }
                }
            }

        }*/

        protected bool SectorHasBeenScanned(Point sectorCoords)
        {
            if (sectorCoords.X < 0 || sectorCoords.Y < 0 || sectorCoords.X >= targetLayer.SectorsAcrossWidth || sectorCoords.Y >= targetLayer.SectorsAcrossHeight)
                return true;

            return scannedSectors.Contains(sectorCoords);

        }

       
        /// <summary>
        /// if on the dependent map, we should remove edges between the layers before rebuilding the graph.
        /// </summary>
        /// <returns></returns>
        protected virtual bool RemoveAllEdgesBetweenLayers()
        {
            return true;          
        }
       
        /// <summary>
        /// only dirty/cleared sectors require this step.
        /// </summary>
        /// <returns></returns>
        protected bool BuildInternalEdgesForDirtySectors()
        {
            if (dirtySectors.Count > 0)
            {
                bool isLastSector;
                SubtileSector sector = GetCurrentDirtySectorAndIncrement(out isLastSector); // only dirty sectors

                sector.BuildRegionGraph(this);

                return isLastSector;
            }

            return true;

            #region OLD
            /*
            if (rectanglesToClearAndRedraw.Count > 0)
            {
                // scan dirty rectangles only - one per cycle:
                Rectangle scanRectangle = rectanglesToClearAndRedraw[buildGraphCurrentScanRectangle];

                // scan starts 1 subtile away
                int minX = Math.Max(scanRectangle.Left - 1, 0);
                int minY = Math.Max(scanRectangle.Top - 1, 0);

                int maxX = Math.Min(scanRectangle.Right + 1, subtileMapWidth - 1);
                int maxY = Math.Min(scanRectangle.Bottom + 1, subtileMapHeight - 1);

                for (int x = minX; x < maxX; x++)
                {
                    for (int y = minY; y < maxY; y++)
                    {
                        ExamineQuadForEdges(x, y, x + 1, y + 1);
                    }
                }

                buildGraphCurrentScanRectangle++;
                if (buildGraphCurrentScanRectangle == rectanglesToClearAndRedraw.Count)
                {
                    buildGraphCurrentScanRectangle = 0;
                    return true;
                }

                return false;
            }
            else
            {
                // scan the whole map
                // scan the map by pairs of tiles, first horizontally, then vertically:

                for (int x = 0; x < subtileMapWidth - 1; x++)
                {   // can this be parallelized?
                    ExamineQuadForEdges(x, buildGraphCurrentRow, x + 1, buildGraphCurrentRow + 1);
                }

                buildGraphCurrentRow++;

                if (buildGraphCurrentRow >= subtileMapHeight - 1)
                {
                    buildGraphCurrentRow = 0; // NEW

                    return true;
                }

                return false;
            }*/
            #endregion
        }


        /// <summary>
        /// for the dependent map, we need to rebuild all sector edges... for the terrain map, only the sectors that have changed.
        /// </summary>
        /// <returns></returns>
        protected abstract bool BuildRegionGraphBetweenSectors();

        protected void ScanSectorEdges(SubtileSector thisSector, SubtileSector thisOveriddenSector)
        {

            // edges
            Point otherSectorCoords = new Point(thisSector.Coords.X - 1, thisSector.Coords.Y);
            if (!SectorHasBeenScanned(otherSectorCoords))
            {
                ScanVerticalSideForEdges(thisSector, thisOveriddenSector, AdjacentSector.Left);                
            }

          //  AssertRegionGraph();


            otherSectorCoords = new Point(thisSector.Coords.X + 1, thisSector.Coords.Y);
            if (!SectorHasBeenScanned(otherSectorCoords))
            {
                ScanVerticalSideForEdges(thisSector, thisOveriddenSector, AdjacentSector.Right);                           
            }

            //AssertRegionGraph();


            otherSectorCoords = new Point(thisSector.Coords.X, thisSector.Coords.Y - 1);
            if (!SectorHasBeenScanned(otherSectorCoords))
            {
                ScanHorizontalSideForEdges(thisSector, thisOveriddenSector, AdjacentSector.Over);            
            }

         //   AssertRegionGraph();


            otherSectorCoords = new Point(thisSector.Coords.X, thisSector.Coords.Y + 1);
            if (!SectorHasBeenScanned(otherSectorCoords))
            {
                ScanHorizontalSideForEdges(thisSector, thisOveriddenSector, AdjacentSector.Under);
               
            }

        //    AssertRegionGraph();


            // corners
            otherSectorCoords = new Point(thisSector.Coords.X - 1, thisSector.Coords.Y - 1);
            if (!SectorHasBeenScanned(otherSectorCoords))
            {
                ScanTopLeftCornerOfSector(thisSector, thisOveriddenSector, otherSectorCoords);
            }

       //     AssertRegionGraph();

            otherSectorCoords = new Point(thisSector.Coords.X + 1, thisSector.Coords.Y - 1);
            if (!SectorHasBeenScanned(otherSectorCoords))
            {
                ScanTopRightCornerOfSector(thisSector, thisOveriddenSector, otherSectorCoords);
            }

          //  AssertRegionGraph();

            otherSectorCoords = new Point(thisSector.Coords.X - 1, thisSector.Coords.Y + 1);
            if (!SectorHasBeenScanned(otherSectorCoords))
            {
                ScanBottomLeftCornerOfSector(thisSector, thisOveriddenSector, otherSectorCoords);
            }

         //   AssertRegionGraph();

            otherSectorCoords = new Point(thisSector.Coords.X + 1, thisSector.Coords.Y + 1);
            if (!SectorHasBeenScanned(otherSectorCoords))
            {
                ScanBottomRightCornerOfSector(thisSector, thisOveriddenSector, otherSectorCoords);
            }

           // AssertRegionGraph();

            scannedSectors.Add(thisSector.Coords);
        }
     /*   protected virtual bool BuildInternalRegionGraph()
        {           
            if (rectanglesToClearAndRedraw.Count > 0)
            {
                // scan dirty rectangles only - one per cycle:
                Rectangle scanRectangle = rectanglesToClearAndRedraw[buildGraphCurrentScanRectangle];
                              
                // scan starts 1 subtile away
                int minX = Math.Max(scanRectangle.Left - 1, 0);
                int minY = Math.Max(scanRectangle.Top - 1, 0);

                int maxX = Math.Min(scanRectangle.Right + 1, subtileMapWidth - 1);
                int maxY = Math.Min(scanRectangle.Bottom + 1, subtileMapHeight - 1);
               
                for (int x = minX; x < maxX; x++)
                {
                    for (int y = minY; y < maxY; y++)
                    {
                        ExamineQuadForEdges(x, y, x + 1, y + 1);
                    }
                }

                buildGraphCurrentScanRectangle++;
                if (buildGraphCurrentScanRectangle == rectanglesToClearAndRedraw.Count)
                {
                    buildGraphCurrentScanRectangle = 0;
                    return true;
                }

                return false;
            }
            else
            {
                // scan the whole map
                // scan the map by pairs of tiles, first horizontally, then vertically:

                for (int x = 0; x < subtileMapWidth - 1; x++)
                {   // can this be parallelized?
                    ExamineQuadForEdges(x, buildGraphCurrentRow, x + 1, buildGraphCurrentRow + 1);
                }

                buildGraphCurrentRow++;

                if (buildGraphCurrentRow >= subtileMapHeight - 1)
                {
                    buildGraphCurrentRow = 0; // NEW

                    return true;
                }

                return false;
            }
        }*/

        /*  protected virtual bool BuildRegionGraph()
        {           
            if (rectanglesToClearAndRedraw.Count > 0)
            {
                // scan dirty rectangles only - one per cycle:
                Rectangle scanRectangle = rectanglesToClearAndRedraw[buildGraphCurrentScanRectangle];
                              
                // scan starts 1 subtile away
                int minX = Math.Max(scanRectangle.Left - 1, 0);
                int minY = Math.Max(scanRectangle.Top - 1, 0);

                int maxX = Math.Min(scanRectangle.Right + 1, subtileMapWidth - 1);
                int maxY = Math.Min(scanRectangle.Bottom + 1, subtileMapHeight - 1);
               
                for (int x = minX; x < maxX; x++)
                {
                    for (int y = minY; y < maxY; y++)
                    {
                        ExamineQuadForEdges(x, y, x + 1, y + 1);
                    }
                }

                buildGraphCurrentScanRectangle++;
                if (buildGraphCurrentScanRectangle == rectanglesToClearAndRedraw.Count)
                {
                    buildGraphCurrentScanRectangle = 0;
                    return true;
                }

                return false;
            }
            else
            {
                // scan the whole map
                // scan the map by pairs of tiles, first horizontally, then vertically:

                for (int x = 0; x < subtileMapWidth - 1; x++)
                {   // can this be parallelized?
                    ExamineQuadForEdges(x, buildGraphCurrentRow, x + 1, buildGraphCurrentRow + 1);
                }

                buildGraphCurrentRow++;

                if (buildGraphCurrentRow >= subtileMapHeight - 1)
                {
                    buildGraphCurrentRow = 0; // NEW

                    return true;
                }

                return false;
            }
        }*/

      

        bool IsDependentRegion(ushort region)
        {
            return region > ColorStartOfRange;
        }

        /// <summary>
        /// add connectors between layers.
        /// don't modify the terrain graph.
        /// </summary>
        /// <param name="fromRegionColor"></param>
        /// <param name="toRegionColor"></param>
        public void AddEdgesIfNotExists(ushort fromRegionColor, ushort toRegionColor)
        {
           
            if (fromRegionColor != 0 && toRegionColor != 0
                && fromRegionColor != toRegionColor)
            {
               
                // !! the key is the region the edge points to!
                Dictionary<ushort, RegionEdge> setOfEdges;

                float distanceBetweenRegionCenters;

                // add an edge in both directions... their distance is the same
                Region fromRegion = newRegions[fromRegionColor];
                Region toRegion = newRegions[toRegionColor];
                distanceBetweenRegionCenters = Common.DistanceOctile(fromRegion.CenterLocation, toRegion.CenterLocation);
                      
                // get the set of edges leading from this region:
                if (!newRegionGraph.TryGetValue(fromRegionColor, out setOfEdges))
                {
                    AddEdge(newRegionGraph, fromRegionColor, toRegionColor, distanceBetweenRegionCenters);

                }
                else
                {   // see if this edge exists already:
                    if (!setOfEdges.ContainsKey(toRegionColor))
                    {
                         setOfEdges.Add(toRegionColor, new RegionEdge(fromRegionColor, toRegionColor, distanceBetweenRegionCenters, false));
                    }
                    else
                    {
                        // if the edge exists in one direction, it will also exist in the other direction, so bail out here:
                        return;
                    }
                }

                // the other direction:
                if (!newRegionGraph.TryGetValue(toRegionColor, out setOfEdges))
                {
                    setOfEdges = new Dictionary<ushort, RegionEdge>();
                    newRegionGraph.Add(toRegionColor, setOfEdges);
                }

                // !! the key is the region the edge points to!
                setOfEdges.Add(fromRegionColor, new RegionEdge(toRegionColor, fromRegionColor, distanceBetweenRegionCenters, false));
                
            }
        }

        protected static float AddEdge(Dictionary<ushort, Dictionary<ushort, RegionEdge>> regionGraph, ushort fromRegion, ushort toRegion, float distanceBetweenRegionCenters)
        {
            Dictionary<ushort, RegionEdge> setOfEdges;
          
            setOfEdges = new Dictionary<ushort, RegionEdge>();
            regionGraph.Add(fromRegion, setOfEdges);

         //   distanceBetweenRegionCenters = Common.DistanceOctile(newRegions[fromRegion].CenterLocation, newRegions[toRegion].CenterLocation);
          
            // !! the key is the region the edge points to!
            setOfEdges.Add(toRegion, new RegionEdge(fromRegion, toRegion, distanceBetweenRegionCenters, false));
            return distanceBetweenRegionCenters;
        }


        /// <summary>
        /// finds region edges on a quad that straddles two sectors
        /// </summary>
        /// <param name="fromX"></param>
        /// <param name="fromY"></param>
        /// <param name="toX"></param>
        /// <param name="toY"></param>
    /*    protected void ExamineSectorBorderQuadForEdges(int fromX, int fromY, int toX, int toY) 
        {
            ushort region = layers.GetRegion(fromX, fromY);
            GetRegion(sectorX, sectorY, relativeX, relativeY);



            ushort[] fromXColumn = newAllSubtiles[fromX];
            ushort topLeftRegion = fromXColumn[fromY]; // top left of subtile quad
            //  ushort toRegion = allNodes[toX, toY];

            if (topLeftRegion > 0)
            {

                ushort[] toXColumn = newAllSubtiles[toX];
                ushort topRightRegion = toXColumn[fromY]; // top right of quad   
                if (topRightRegion > 0)
                {
                    AddEdgesIfNotExists(topLeftRegion, topRightRegion);
                }

                ushort bottomRightRegion = toXColumn[toY];
                if (bottomRightRegion > 0)
                {
                    AddEdgesIfNotExists(topLeftRegion, bottomRightRegion);
                }

                ushort bottomLeftRegion = fromXColumn[toY];
                if (bottomLeftRegion > 0)
                {
                    AddEdgesIfNotExists(topLeftRegion, bottomLeftRegion);

                    if (topRightRegion > 0)
                    {
                        AddEdgesIfNotExists(bottomLeftRegion, topRightRegion);
                    }
                }
            }
        }*/

        /// <summary>
        /// finds region edges on the corner between 4 sectors
        /// </summary>
        /// <param name="fromX"></param>
        /// <param name="fromY"></param>
        /// <param name="toX"></param>
        /// <param name="toY"></param>
     /*   protected void ExamineSectorCornerQuadForEdges(int fromX, int fromY, int toX, int toY) // ushort from, ushort to)
        {
            ushort[] fromXColumn = newAllSubtiles[fromX];
            ushort topLeftRegion = fromXColumn[fromY]; // top left of subtile quad
            //  ushort toRegion = allNodes[toX, toY];

            if (topLeftRegion > 0)
            {

                ushort[] toXColumn = newAllSubtiles[toX];
                ushort topRightRegion = toXColumn[fromY]; // top right of quad   
                if (topRightRegion > 0)
                {
                    AddEdgesIfNotExists(topLeftRegion, topRightRegion);
                }

                ushort bottomRightRegion = toXColumn[toY];
                if (bottomRightRegion > 0)
                {
                    AddEdgesIfNotExists(topLeftRegion, bottomRightRegion);
                }

                ushort bottomLeftRegion = fromXColumn[toY];
                if (bottomLeftRegion > 0)
                {
                    AddEdgesIfNotExists(topLeftRegion, bottomLeftRegion);

                    if (topRightRegion > 0)
                    {
                        AddEdgesIfNotExists(bottomLeftRegion, topRightRegion);
                    }
                }
            }
        }*/

        /// <summary>
        /// finds internal edges inside a sector
        /// </summary>
        /// <param name="fromX"></param>
        /// <param name="fromY"></param>
        /// <param name="toX"></param>
        /// <param name="toY"></param>
   /*     protected void ExamineQuadForEdges(int fromX, int fromY, int toX, int toY) // ushort from, ushort to)
        {
           
            ushort[] fromXColumn = newAllSubtiles[fromX];
            ushort topLeftRegion = fromXColumn[fromY]; // top left of subtile quad
            //  ushort toRegion = allNodes[toX, toY];

            if (topLeftRegion > 0)
            {
                
                ushort[] toXColumn = newAllSubtiles[toX];
                ushort topRightRegion = toXColumn[fromY]; // top right of quad   
                if (topRightRegion > 0)
                {
                    AddEdgesIfNotExists(topLeftRegion, topRightRegion);
                }

                ushort bottomRightRegion = toXColumn[toY];
                if (bottomRightRegion > 0)
                {
                    AddEdgesIfNotExists(topLeftRegion, bottomRightRegion);
                }

                ushort bottomLeftRegion = fromXColumn[toY];
                if (bottomLeftRegion > 0)
                {
                    AddEdgesIfNotExists(topLeftRegion, bottomLeftRegion);

                    if (topRightRegion > 0)
                    {
                        AddEdgesIfNotExists(bottomLeftRegion, topRightRegion);
                    }
                }

            }
        }*/



        /* private bool GetEdges(ushort from, ref List<ushort> edges)
         {
             return RegionGraph.TryGetValue(from, out edges);

         }

         public bool GetEdges(ushort from, ref HashSet<ushort> edges)
         {
             return RegionGraph.TryGetValue(from, out edges);

         }*/

       /* private void MergeScanRectangles()
        {
            // clean up the rectangles. otherwise we will end up with very small regions.
            // this will detect overlapping rectangles and replace them with a single bounding rectangle.

            int i = 0, j = 0;

            bool hasMerged = false;

            Rectangle r1, r2, rNew;
            while (i < rectanglesToClearAndRedraw.Count)
            {
                r1 = rectanglesToClearAndRedraw[i];

                j = 0;

                while (j < rectanglesToClearAndRedraw.Count)
                {
                    if (i != j)
                    {
                        r2 = rectanglesToClearAndRedraw[j];

                        if (Intersects(r1, r2))
                        {
                            rNew = Rectangle.Union(r1, r2);

                            rectanglesToClearAndRedraw.Remove(r1);
                            rectanglesToClearAndRedraw.Remove(r2);

                            rectanglesToClearAndRedraw.Add(rNew);

                            // start over...
                            i = 0;
                            j = 0;

                            hasMerged = true;

                            break;
                        }
                    }

                    j++;
                }

                if (!hasMerged)
                {
                    i++;
                }
                else
                {
                    hasMerged = false;
                }
            }


           
            //  return scanRectanglesAroundDirtySectors;
            // Rectangle.Intersect
        }*/


        public void AddLog(string text)
        {
#if DEBUG || PROFILE
           
            Log.Add(new Tuple<double, string>(The.Sim.TotalUnPausedGameTimeInSeconds, text));
#endif
        }

        private bool Intersects(Rectangle r1, Rectangle r2)
        {
            return !(r2.Left > r1.Left + r1.Width
                   || r2.Left + r2.Width < r1.Left
                   || r2.Top > r1.Top + r1.Height
                   || r2.Top + r2.Height < r1.Top
                   );
        }

      
        // moved to SubtileSector
    /*    private bool GetNextCenter()
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

        }

        private bool ScanRectangleWithIntervals(Rectangle r)
        {
            if (currentCenter.X >= r.Left && currentCenter.X <= r.Right
                && currentCenter.Y >= r.Top && currentCenter.Y <= r.Bottom)
            {
                // continue looking in this rectangle...
                //currentCenter.X += regionInterval;
            }
            else
            {
                // find a starting point for the scan
                if (r.Width > regionRadius)
                {
                    currentCenter.X = r.Left + regionRadius;
                }
                else
                {
                    currentCenter.X = r.Left + r.Width / 2;
                }

                currentCenter.X -= regionInterval; // compensate for loop increment...

                if (r.Height > regionRadius)
                {
                    currentCenter.Y = r.Top + regionRadius;
                }
                else
                {
                    currentCenter.Y = r.Top + r.Height / 2;
                }
            }

            do
            {
                currentCenter.X += regionInterval;

                //  if (currentCenter.X >= r.Right)
                if (currentCenter.X > r.Right) // boundaries are inclusive...
                {
                    currentCenter.X = r.Left + regionRadius;

                    currentCenter.Y += regionInterval;

                    //  if (currentCenter.Y >= r.Bottom)
                    if (currentCenter.Y > r.Bottom)
                    {
                        return false;
                    }
                }

            } // skip an interval if this tile is already colored, or if it is a blocked tile:
            while (newAllSubtiles[currentCenter.X][currentCenter.Y] > 0 ||
                    MapManager.IsBlocked(layers[currentCenter.X][currentCenter.Y]));

            return true;
        }*/

        /*  private bool GetNextCenterInScanRectangles()
          {
              int rectangleIndex = 0;
              Rectangle scanRectangle;

              do
              {
                  if (rectangleIndex == scanRectanglesAroundDirtySectors.Count)
                  {
                      return false;
                  }

                  scanRectangle = scanRectanglesAroundDirtySectors[rectangleIndex];


                  currentCenter.X = scanRectangle.Left + scanRectangle.Width / 2;
                  currentCenter.Y = scanRectangle.Top + scanRectangle.Height / 2;
                              

                  rectangleIndex++;

                
              } // skip an interval if this tile is already colored, or if it is a blocked tile:
              while (newAllSubtiles[currentCenter.X][currentCenter.Y] > 0 ||
                  moveCostsSubtileMap[currentCenter.X][currentCenter.Y] == 0);

              return true;

          }*/

        #endregion

        

       

       
        #region ILookup

        private CyclableID id = CyclableID.Invalid;
       
        //=================== ILookup Methods =====================
        public CyclableID ID
        {
            get
            {
                return id;
            }

            private set
            {
                id = value;
            }
        }

        public CyclableID GetUniqueID()
        {
            return Cyclable.GetUniqueID();
        }

        public CyclableID SnapshotID(Snapshotter sn, CyclableID id)
        {
            return (CyclableID)sn.DoEnum(id);
        }


        public int LoadPostProcessOrder
        {
            get
            {
                return 0;
            }
        }


        public void AddToLookup()
        {
            ID = GetUniqueID();
            if (ID != CyclableID.Invalid)
                LookUp<ICyclable, CyclableID>.Add(ID, this);
        }

        public void RemoveIDEntry()
        {
            LookUp<ICyclable, CyclableID>.Remove(this);
        }

        public void SetInvalid()
        {
            id = CyclableID.Invalid;
        }

        public void ResetIDCounter() // interface method - does nothing... Sim will call Cyclable.ResetIDCounter.
        {
        }

        void ILookUp<ICyclable, CyclableID>.CreateLookupCollection() // interface method - does nothing...
        {
        }

        public static void CreateLookupCollection()
        {
            LookUp<ICyclable, CyclableID>.Create();
        }


        #endregion





        #region ISnapshot

           

        public virtual ISnapshot DoSnapshot(Snapshotter sn)
        {
            // when snapshotting, the map will always have a graph (this is done on startup)

            // if repairing is underway, we should scrap the progress and start over when the instance loads.
            // so: don't snapshot the data structures that are prefixed with 'new'
            // don't save unusedColors either, or the region color counter. reset them after load.
            // On Save:
            // - Unregister the RegionMap - after load, let Update determine if there are dirty regions.
            // - Make sure that CycleManager did not save us already. Use UnregisterOnSave
            // - Dirty Regions must be restored to what it was before repairing started, PLUS the dirty regions that were added since... 
            // - Region search requests will be snapshotted and restarted after load.
            //                  

            id = SnapshotID(sn, id);
            regions = sn.DoDictionary(regions);

           
            this.IsPaused = sn.DoBool(IsPaused);
            this.IDName = sn.DoString(IDName);
            this.RegionGraph = sn.DoNestedDictionary(RegionGraph);
            this.AllRegionCosts = sn.DoNestedDictionary(AllRegionCosts);      
            this.cachedClosestRegionToBlockedSubtile = sn.DoDictionary(cachedClosestRegionToBlockedSubtile);

            this.RegionPaths = sn.DoNestedDictionary(RegionPaths);

         //   this.dirtySectors = sn.DoList(dirtySectors); 
            if (sn.mode != Snapshotter.Mode.Load)
            {
                dirtySectorsRollbackAfterSave = new List<Point>(dirtySectors); // save the set of sectors we were handling
            }
            this.dirtySectorsRollbackAfterSave = sn.DoList(dirtySectorsRollbackAfterSave);
            this.sectorsAreDirty = sn.DoBool(sectorsAreDirty); // this flag has been set if more regions are marked dirty while we were handling the set of dirty sectors

        //    this.dirtySectorsBeforeRepairStarted = sn.DoHashSet(dirtySectorsBeforeRepairStarted);// this has the sectors that were dirty before repair started

            this.regionSearchRequests = sn.DoList(regionSearchRequests);
            this.regionBFSPlanners = sn.DoDictionary(regionBFSPlanners);
            this.regionAStarPlanners = sn.DoNestedDictionary(regionAStarPlanners);
           // this.OutOfBandCosts = sn.DoNestedDictionary(OutOfBandCosts);

                      

            this.snapshotTargetLayer = (SubtileLayerID)sn.SnapshotID<SubtileLayer, SubtileLayerID>(targetLayer);

            this.subtileMapWidth = sn.DoInt32(subtileMapWidth);
            this.subtileMapHeight = sn.DoInt32(subtileMapHeight);
            this.snapshotLayers = (SubtileLayersID)sn.SnapshotID<SubtileLayers, SubtileLayersID>(layers); // (SubtileLayerID)sn.DoEnum(snapshotLayers);
          

            sn.Ignore(dirtySectors);
            sn.Ignore(layers);
            sn.Ignore(progress);
            sn.Ignore(isComputing);
           // sn.Ignore(sectorsAreDirty);
            sn.Ignore(totalComputationAllInstancesInSeconds);
            sn.Ignore(ComputationTimeSpentInSeconds);
            sn.Ignore(StartedOnTimeInSeconds);
            sn.Ignore(newRegionGraph);
            sn.Ignore(regionGraphKeyListForCopying);
            sn.Ignore(regionGraphCopyProgressIndex);
            sn.Ignore(newRegions);       

            sn.Ignore(timeTaken);
            sn.Ignore(scannedSectors);
       
            sn.Ignore(UnusedRegionColors);
            sn.Ignore(regionColorCounter);

        
            sn.Ignore(cyclesToBuildGraph);
            sn.Ignore(cyclesToFillRegions);    
            sn.Ignore(FloodFill);

            sn.Ignore(currentSectorIndex);
            sn.Ignore(regionsToRemove);          
            sn.Ignore(Log);
            sn.Ignore(timer);
          

            return this;
        }

        private void ResetColorCounter()
        {
            if (regions.Count > 0)
            {
                ushort maxColor = regions.Max(k => k.Key);

                regionColorCounter = (ushort)(maxColor + (ushort)1);
            }
            else
            {
                regionColorCounter = ColorStartOfRange;
            }

            AddLog("Color counter was reset to: " + regionColorCounter);
        }

        /// <summary>
        /// examine the regions dict to find unused colors...
        /// </summary>
        private void GetUnusedColors()
        {
            List<ushort> usedColors = regions.Keys.ToList();

            List<ushort> ordered = usedColors.OrderBy(c => c).ToList();

            ushort? lastColor = null;
            ushort currentColor;
            for (int i = 0; i < ordered.Count; i++)
            {
                currentColor = ordered[i];

                if (lastColor.HasValue &&
                    currentColor > lastColor + 1)
                {
                    // gap detected.
                    // add the gap:
                    for (int j = lastColor.Value + 1; j < currentColor; j++)
                    {
                        UnusedRegionColors.Enqueue((ushort)j);    
                    }       
                }

                lastColor = currentColor;
            }

            AddLog("Gathered unused colors, no.: " + UnusedRegionColors.Count);
        }

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public virtual Snapshotter.Version DoVersion(Snapshotter sn)
        {
            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }

        public bool IsSnapshotted { get; set; }

        public virtual void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);


         
            targetLayer = LookUp<SubtileLayer, SubtileLayerID>.FindByID(snapshotTargetLayer);
            layers = LookUp<SubtileLayers, SubtileLayersID>.FindByID(snapshotLayers);
           
            // reset for repair:
            // sectors marked dirty after repair started are preserved.
            foreach (var item in dirtySectorsRollbackAfterSave)
            {
                targetLayer.Sectors[item.X][item.Y].BlockedStatusHasChanged = true;
            }
            if (dirtySectorsRollbackAfterSave.Count > 0)
            {
                sectorsAreDirty = true;
            }


            // repair will start over after loading, so make sure we have all the dirty points that were marked before and while repair was ongoing
            //  this.dirtyPoints.UnionWith(dirtyPointsBeforeRepairStarted);          
            //this.dirtySectors = dirtySectorsBeforeRepairStarted.ToList();
           /* foreach (var item in dirtySectorsBeforeRepairStarted)
            {
                targetLayer.Sectors[item.X][item.Y].BlockedStatusHasChanged = true; 
            }
            sectorsAreDirty = dirtySectorsBeforeRepairStarted.Count > 0;
            dirtySectorsBeforeRepairStarted.Clear();
            */
            

            Init();

            foreach (var item in regions)
            {
                item.Value.LoadPostProcess(sn);
            }

            foreach (var item in RegionGraph)
            {
                foreach (var item2 in item.Value)
                {
                    item2.Value.LoadPostProcess(sn);
                }
            }

            ResetColorCounter();

            GetUnusedColors();

            // newRegions are not saved, but have to be at least as new as regions. They can be accessed from neighbouring sectors before they have been repaired.
            InitCopyVariablesRegions();

            // let's make sure the unsaved newRegions collection contains meaningful data too:
            bool isDone = false;
            int regionGraphProgress = 0;
            List<ushort> currentRegionGraphKeyListForCopying = null;
            do 
            {
                isDone = InitCopyVariablesRegionGraph(RegionGraph, ref newRegionGraph, ref regionGraphProgress, ref currentRegionGraphKeyListForCopying);
            }
            while (!isDone);

                       
            if (/*this is TerrainRegionMap*/ RegionGraph.Count == 0 && regions.Count == 0)
            {
                // create from bottom up:
                this.progress = Progress.InitFromScratch;
            }
            else
            { 
                // set progress to start of repair:
                SetProgressAtRepairStart();
            }
            

            timer = new HighResolutionTime();
        }

        #endregion


        public bool UnregisterBeforeSnapshot
        {
            get { return true; }
        }
    }

   
   
    

    
}
