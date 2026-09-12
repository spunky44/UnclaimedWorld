using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.AI.Pathfinding;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems.TimeSlicing;

namespace UWGame.SimSide.Maps.Regions
{
   
    public class DependentRegionMap : RegionMap, IIDEventSubscriber
    {
        /// <summary>
        /// we share regions and graph with this map:
        /// </summary>
        TerrainRegionMap terrainRegionMap;
        CyclableID snapshotTerrainRegionMap;

        MethodID onTerrainRegionsAreInvalidID, onTerrainRegionsAreDoneID;

        CyclableID parentMovementMap;

        /// <summary>     
        /// the edges are added to the graph itself, this is only needed for faster removal of those edges...
        /// </summary>
        private Dictionary<ushort, List<ushort>> bottomToUpperLayerConnectors = new Dictionary<ushort,List<ushort>>(); // connections from the bottom graph to the overriding sectors.

        /// <summary>
        /// blocked edges on the bottom graph when there are overriding sectors
        ///         
        /// </summary>
        private Dictionary<ushort, HashSet<ushort>> bottomBlockedEdges = new Dictionary<ushort,HashSet<ushort>>();
        private Dictionary<ushort, HashSet<ushort>> newBottomBlockedEdges = new Dictionary<ushort, HashSet<ushort>>();  
     

        private HashSet<ushort> regionColorsToRemove = new HashSet<ushort>();
        private HashSet<ushort> regionColorsToRemoveRollbackAfterSave = new HashSet<ushort>();


        public override Dictionary<ushort, HashSet<ushort>> BottomBlockedEdges
        {
            get
            {
                return bottomBlockedEdges;
            }
        }

        public override Dictionary<ushort, Region> BottomRegions
        {
            get { return terrainRegionMap.BottomRegions; }
        }

        public override Dictionary<ushort, Dictionary<ushort, RegionEdge>> BottomRegionGraph
        {
            get
            {
                return terrainRegionMap.RegionGraph;
            }
        }

        /// <summary>
        /// Total Colors: 65.536
        /// its region colors are in a specific range from 40.001 to 65.536
        /// </summary>
        protected override ushort ColorStartOfRange
        {
            get
            {
                return RegionMap.BaseLayerRegionColors;
            }
        }

        public DependentRegionMap()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor");
        }

        public DependentRegionMap(MovementMap moveMap, SubtileLayers layers, TerrainRegionMap terrainRegionMap)
            : base(layers, layers.Layers[1])
        {
            this.parentMovementMap = moveMap.ID;
            this.terrainRegionMap = terrainRegionMap;

            this.terrainRegionMap.RegionsAreInvalid.AddAndRegister(OnTerrainRegionsAreInvalid, this, out onTerrainRegionsAreInvalidID);
            this.terrainRegionMap.RegionsFinished.AddAndRegister(OnTerrainRegionsAreDone, this, out onTerrainRegionsAreDoneID);

        }


        public void SetRegionsFromRemovedSectors(HashSet<ushort> regionColorsToRemove)
        {
           
            // append:
            foreach (var item in regionColorsToRemove)
            {
                this.regionColorsToRemove.Add(item);
            }

            this.regionColorsToRemoveRollbackAfterSave = new HashSet<ushort>(this.regionColorsToRemove); // save a copy for possible rollback after save

            // we have to cancel all searches now. removing a sector means we lose both current and saved region subtiles, and we need them for when the search finishes
            CancelAllSearches();

            cachedClosestRegionToBlockedSubtile.Clear();

            // Now we cannot accept searches until the graph has been rebuilt. The removed sector can open up connections in the graph that were previously hidden. 
            // The system of blocked bottom edges is not complete. Two overridden sectors do not get blocked edges between them, for example.
            waitForRebuildAfterRemovedSector = true;
            

            // rebuild the map:
            sectorsAreDirty = true;
        }

        protected override void RemoveRegions()
        {
            base.RemoveRegions();

            // remove any regions that were inside sectors discarded by MovementMap:
            if (regionColorsToRemove != null && regionColorsToRemove.Count > 0)
            {               
                foreach (var item in regionColorsToRemove)
                {
                    RemoveRegion(newRegions[item]);
                }

                AddLog("Removed discarded sector regions: " + regionColorsToRemove.Count);

              //  AssertRegionGraph();

                regionColorsToRemove.Clear();
            }

          
            
        }

        public void OnTerrainRegionsAreInvalid()
        {
            
            // don't compute the graph while the terrain map is busy           
            if (IsComputingLayerEdges()) 
            {
                AddLog("OnTerrainRegionsAreInvalid while computing edges, IsPaused = true, isWaitingForTerrainRegions = true");

                // reset and wait...
                ResetEdgeBuilding();

                IsPaused = true;
            }
            else
            {
                AddLog("OnTerrainRegionsAreInvalid before computing edges, isWaitingForTerrainRegions = true");

            }
            // else pause when we get to the critical section

            // cancel all searches. they have to wait until the graph is rebuilt.
            CancelAllSearches(); 

            // clear cache here!?!?!?
            cachedClosestRegionToBlockedSubtile.Clear();


            //The graph is now invalid until rebuilt...           
            isWaitingForTerrainRegions = true;
        }


        /// <summary>
        /// sent from terrain region map!
        /// </summary>
        public void OnTerrainRegionsAreDone()
        {

            // terrain region is done, but we are NOT ready to service requests yet!
            bool wasPaused = IsPaused;
            IsPaused = false;

            MovementMap moveMap = (MovementMap)LookUp<ICyclable, CyclableID>.FindByID(parentMovementMap);
            moveMap.NotifyRegionMapUnpaused();// notify Movement Map so computation can restart

            // restart repair procedure with the new terrain data:
            // if sectors are dirty, start from the top instead. Maybe while we were stalled, some sectors changed?
            
            // cases: 
            // 1. some sectors have changed (sectorsAreDirty = true)
            //      - move back to the top
            // 2. not currently processing
            //      - move to RemoveEdgesBetweenSectors
            // 3. computing before RemoveEdgesBetweenLayers and no sectors have changed
            //      - continue
            // 4. waiting at RemoveEdgesBetweenLayers
            //      - continue
            // 5. building graph between sectors
            //      - move back to RemoveEdgesBetweenSectors
          

          
            if (sectorsAreDirty /*|| dirtySectors.Count > 0*/)   // case 1
            {
                AddLog(string.Format("OnTerrainRegionsAreDone, Case #1 ({0}), new progress = InitCleanupUnneededSectors", progress));

                // reset for repair:
                // redo in-progress sectors... this will not always be necessary for the completed ones, but this will ensure that they all the Finished call at the end.
             
                foreach (var item in dirtySectors)
                {
                    targetLayer.Sectors[item.X][item.Y].BlockedStatusHasChanged = true;
                }
               
                SetProgressAtRepairStart();  // move back to the top
            }
            else if (//isComputing == false &&  // case 2
                    progress == Progress.InitCleanupUnneededSectors)
            {
                AddLog(string.Format("OnTerrainRegionsAreDone, Case #2 ({0}), new progress = RemoveEdgesBetweenLayers", progress));

                progress = Progress.RemoveEdgesBetweenLayers; // move forward to RemoveEdgesBetweenSectors

                currentSectorIndex = 0;
            }
            else if (ProgressIsBeforeRemoveEdgesBetweenLayers()) // case 3
            {
                AddLog(string.Format("OnTerrainRegionsAreDone, Case #3 ({0})", progress));

                //  - continue
            }
            else if (progress == Progress.RemoveEdgesBetweenLayers) // case 4
            {
                AddLog(string.Format("OnTerrainRegionsAreDone, Case #4 (RemoveEdgesBetweenLayers), WasPaused: {0}, IsPaused = false", wasPaused));

                //   - continue
            }
            else if (progress == Progress.BuildRegionGraphBetweenSectors) // case 5
            {
                AddLog("OnTerrainRegionsAreDone, Case #5 (BuildRegionGraphBetweenSectors), new progress = RemoveEdgesBetweenLayers");

                progress = Progress.RemoveEdgesBetweenLayers; // move back to RemoveEdgesBetweenSectors

                currentSectorIndex = 0;

            }
            else
            {
                AddLog(string.Format("OnTerrainRegionsAreDone, Case??? ({0})", progress));

            }
           
        /*    else
            {
                if (progress != Progress.InitFromScratch) // Progress.BuildRegionGraphBetweenSectors)
                {
                    // if not currently processing, also set to removeedges.
                    // start over...
                    AddLog("OnTerrainRegionsAreDone, IsPaused = false, new progress = RemoveEdgesBetweenLayers");

                    progress = Progress.RemoveEdgesBetweenLayers;

                    currentSectorIndex = 0;
                }          

            }*/
           
        }

        private bool ProgressIsBeforeRemoveEdgesBetweenLayers()
        {
            return (int)progress < (int)Progress.RemoveEdgesBetweenLayers;

        }

        public override RegionPathID? GetRegionPath(Point startSubtile, Point destinationSubtile)
        {
            if (SameDataAsTerrain())
            {
                return terrainRegionMap.GetRegionPath(startSubtile, destinationSubtile);
            }
            else
            {
                return base.GetRegionPath(startSubtile, destinationSubtile);
            }
        }


       /* public override List<RegionPathFinderNodeAStar> GetRegionPath(Point startSubtile, Point destinationSubtile)
        {
            if (SameDataAsTerrain())
            {
                return terrainRegionMap.GetRegionPath(startSubtile, destinationSubtile);
            }
            else
            {
                return base.GetRegionPath(startSubtile, destinationSubtile);
            }
        }*/

        bool SameDataAsTerrain()
        {
            if (regions.Count == 0 && RegionGraph.Count == 0 && AllRegionCosts.Count == 0)
            {
                return true;
            }

            return false;
        }

        public override RegionMap.Result GetDistance(Entities.Entity entity, Point fromSubtile, Point toSubtile, ref float distance, bool sendMessageToEntity = true, MethodID? notifyWhenFinished = null, bool registerIfNotReady = true) //, bool doRegionSearchNow = false)
        {
            if (SameDataAsTerrain())
            {
                return terrainRegionMap.GetDistance(entity, fromSubtile, toSubtile, ref distance, sendMessageToEntity, notifyWhenFinished, registerIfNotReady);
            }
            else 
            {
                return base.GetDistance(entity, fromSubtile, toSubtile, ref distance, sendMessageToEntity, notifyWhenFinished, registerIfNotReady);
            }
        }

        private void ResetEdgeBuilding()
        {
            progress = Progress.RemoveEdgesBetweenLayers;
            currentSectorIndex = 0;
        }

        bool waitForRebuildAfterRemovedSector = false;
        bool isWaitingForTerrainRegions = false;
        protected override bool CanServiceRequests
        {
            get
            {
                return !isWaitingForTerrainRegions && !waitForRebuildAfterRemovedSector;
            }
        }

     
       


        public void ResetComputePointIfNeeded()
        {
            if (progress == Progress.RemoveEdgesBetweenLayers && currentSectorIndex == 0 && sectorsAreDirty)
            {
                SetProgressAtRepairStart();
            }
        }
             

        public override void Destroy()
        {
            terrainRegionMap.RegionsFinished.Remove(this.onTerrainRegionsAreDoneID);
            terrainRegionMap.RegionsAreInvalid.Remove(this.onTerrainRegionsAreInvalidID);

            base.Destroy();
        }

      /*  protected override bool IsSameOrNeighbour(ushort fromRegion, ushort toRegion)
        {
            var regionGraph = RegionGraph;
            if (fromRegion >= RegionMap.BaseLayerRegionColors)
            {
                regionGraph = terrainRegionMap.RegionGraph;                
            }


            Dictionary<ushort, RegionEdge> connections;
            if (regionGraph.TryGetValue(fromRegion, out connections) && connections.ContainsKey(toRegion))
            {
                return true;
            }

            return false;
        }*/

        protected override void RecomputeFinished()
        {
            //AssertRegionGraph();
            base.RecomputeFinished();


            bottomBlockedEdges = new Dictionary<ushort, HashSet<ushort>>(newBottomBlockedEdges);

            if (regionColorsToRemoveRollbackAfterSave != null)
            {
                regionColorsToRemoveRollbackAfterSave.Clear();
            }

           // isComputing = false;

            AddLog("Recompute ended, isWaitingForTerrainRegions = false, progress = " + progress);

            isWaitingForTerrainRegions = false; // ready to service requests.
            waitForRebuildAfterRemovedSector = false;
        }

     

    

        /// <summary>
        /// validate overridden sectors, neighbouring regions at the bottom must have blockers.
        /// </summary>
        /// <param name="regionGraphToUse"></param>
        /// <param name="regionsToUse"></param>
        protected override void AssertRegionGraph(bool useFieldsInProgress = true)
        {
            return;

#if DEBUG || PROFILE
            Dictionary<ushort, Dictionary<ushort, RegionEdge>> terrainGraphToUse;
            Dictionary<ushort, Dictionary<ushort, RegionEdge>> regionGraphToUse;
            Dictionary<ushort, Region> regionsToUse;
            Dictionary<ushort, HashSet<ushort>> bottomBlockedEdgesToUse;

            terrainGraphToUse = terrainRegionMap.RegionGraph;

            if (useFieldsInProgress)
            {
                bottomBlockedEdgesToUse = newBottomBlockedEdges;
                regionsToUse = newRegions;
                regionGraphToUse = newRegionGraph;
            }
            else
            {
                bottomBlockedEdgesToUse = bottomBlockedEdges;
                regionsToUse = regions;
                regionGraphToUse = RegionGraph;
            }


            // assert that all regions in the graph also exist in their sectors:
          /*  foreach (var item in regionsToUse)
            {
               
                ushort regionInSector = layers.GetRegion(item.Value.CenterInSubtiles.X, item.Value.CenterInSubtiles.Y, useFieldsInProgress);

                System.Diagnostics.Debug.Assert(item.Key == regionInSector, "Region graph and sectors out of sync!");

            }*/



            // assert no connections from upper layer sectors to overridden sectors exist:
            /*
            bool isLastSector;
            SubtileSector sector; //, overriddenSector;

            do
            {
                sector = GetCurrentSector(out isLastSector);
                if (sector == null)
                    return;
               
                HashSet<ushort> sectorRegions;

                if (useFieldsInProgress)
                {
                    sectorRegions = sector.GetRegionsInProgress();
                }
                else
                {
                    sectorRegions = sector.GetRegions();
                }

                Dictionary<ushort, RegionEdge> edges;
                foreach (var item in sectorRegions)
                {
                    if (item == 40105)
                    {

                    }

                    if (regionGraphToUse.TryGetValue(item, out edges))
                    {
                        foreach (var edge in edges)
                        {
                            // is it an edge to a lower sector?
                            if (edge.Value.ToRegion < ColorStartOfRange)
                            {
                                Region otherRegion = BottomRegions[edge.Value.ToRegion];
                                int topLayerIndex;
                                if (useFieldsInProgress)
                                {
                                    layers.GetUnfinishedSectorFromSubtile(otherRegion.CenterInSubtiles, out topLayerIndex);
                                }
                                else
                                {
                                    layers.GetSectorFromSubtile(otherRegion.CenterInSubtiles, out topLayerIndex);
                                }

                                if (topLayerIndex == 1)
                                {                                                                            
                              //      System.Diagnostics.Debug.Assert(false, "Upper layer sector has connection to overridden sector.");
                                    
                                }
                            }
                        }

                    }
                }

            }
            while (isLastSector == false);



            */

            /* assert blockers from lower layer sectors to overridden sectors exist:
             */
            /*
            SubtileSector overriddenSector;

            do 
            {
                sector = GetCurrentSector(out isLastSector);
                if (sector == null)
                    return;

                overriddenSector = layers.GetOverriddenSector(sector.Coords.X, sector.Coords.Y);

                HashSet<ushort> regions;

                if (useFieldsInProgress)
                {
                    regions = overriddenSector.GetRegionsInProgress();
                }
                else
                {
                    regions = overriddenSector.GetRegions();
                }

                Dictionary<ushort, RegionEdge> edges;
                foreach (var item in regions)
                {
                    if (item == 199)
                    {

                    }

                    if (terrainGraphToUse.TryGetValue(item, out edges))
                    {
                        foreach (var edge in edges)
                        {
                            if (edge.Value.ToRegion < ColorStartOfRange 
                                && !regions.Contains(edge.Value.ToRegion))
                            {
                                Region otherRegion = BottomRegions[edge.Value.ToRegion];
                                int topLayerIndex;
                                if (useFieldsInProgress)
                                {
                                    layers.GetUnfinishedSectorFromSubtile(otherRegion.CenterInSubtiles, out topLayerIndex);
                                }
                                else
                                {
                                    layers.GetSectorFromSubtile(otherRegion.CenterInSubtiles, out topLayerIndex);
                                }

                                if (topLayerIndex == 0)
                                {
                                    // points to bottom outside sector, with no upper sector
                                    // must have a blocker!
                                    HashSet<ushort> blockedEdges;
                                    if (bottomBlockedEdgesToUse.TryGetValue(edge.Value.ToRegion, out blockedEdges))
                                    {
                                        System.Diagnostics.Debug.Assert(blockedEdges.Contains(item), "Missing blocker from bottom layer sector.");
                                    }
                                    else
                                    {
                                        System.Diagnostics.Debug.Assert(false, "Missing blocker from bottom layer sector.");
                                    }
                                }
                            }

                        }

                    }

                }


            }
            while(isLastSector == false);
            */

            /*
              foreach (var item in regionGraphToUse)  // bottomRegions are invalid right after terrain repaint..?
            {
                bool startRegionIsOnTopSector = item.Key >= ColorStartOfRange;

                if (startRegionIsOnTopSector)
                {
                    Region startRegion = regionsToUse[item.Key]; 

                    Point startCenter = startRegion.CenterInSubtiles;

                    int startLayerIndex;
                    SubtileSector startSector = layers.GetSectorFromSubtile(startCenter, out startLayerIndex);
                           
                    foreach (var item2 in item.Value)
                    {
                        bool endRegionIsOnTopSector = item2.Key >= ColorStartOfRange;
                        if (!endRegionIsOnTopSector)
                        {
                            Region endRegion = BottomRegions[item2.Key]; // invalid right after terrain repaint..?

                            Point center = endRegion.CenterInSubtiles;

                            int layerIndex;
                            SubtileSector endSector = layers.GetSectorFromSubtile(center, out layerIndex);
                            System.Diagnostics.Debug.Assert(layerIndex == 0, "Error in region graph. A region connects to another region that is overridden by a top sector.");

                        }
                    }
                }
            }
             */

            // assert all connectors are to valid regions: (#CRASH1)
            Dictionary<ushort, RegionEdge> connections;

            foreach (var fromRegion in regionsToUse)
	        {
                if (regionGraphToUse.TryGetValue(fromRegion.Key, out connections))
                {               
                    foreach (KeyValuePair<ushort, RegionEdge> kvp in connections)
                    {
                        Region region;
                        if (useFieldsInProgress)
                        {
                            if (GetRegionInProgress(kvp.Key) == null) // , out region)) //  GetRegion(kvp.Key) == null)
                            {
                                throw new Exception("Region does not exist?"); //Lars: comment out this line if there is no time to investigate it
                            }
                        }
                        else
                        {
                            if (GetRegion(kvp.Key) == null) //  !regionsToUse.TryGetValue(kvp.Key, out region)) //  GetRegion(kvp.Key) == null)
                            {
                                throw new Exception("Region does not exist?"); //Lars: comment out this line if there is no time to investigate it
                            }
                        }
                        
                    }
                }
	        }
            
                
            /*
            foreach (var item in RegionGraph)
            {
                foreach (var region in item.Value)
                {
                    

                }
            }*/

#endif
        }


        protected override void StartBuildingGraphBetweenSectors()
        {
            if (terrainRegionMap.IsComputing)
            {
                AddLog("StartBuildingGraph, IsPaused = true");

                IsPaused = true; // halt!
            }
        }

        private bool IsComputingLayerEdges()
        {
            switch(progress)
            {
                case Progress.BuildRegionGraphBetweenSectors:
                case Progress.RemoveEdgesBetweenLayers:
              //  case Progress.BuildInternalRegionGraph:
                    return true;
            }

            return false;
        }

       

        public override Region GetRegion(ushort color)
        {
            if (color >= RegionMap.BaseLayerRegionColors )
            {
                return regions[color]; // Regions[color - 1]; // no region has the color 0 - this value is for 'no region'...
            }
            else if (color > 0)
            {
                return terrainRegionMap.GetRegion(color);
            }
            else return null;
        }

        public override Region GetRegionInProgress(ushort color)
        {
            if (color >= RegionMap.BaseLayerRegionColors)
            {
                return newRegions[color]; // Regions[color - 1]; // no region has the color 0 - this value is for 'no region'...
            }
            else if (color > 0)
            {
                return terrainRegionMap.GetRegionInProgress(color);
            }
            else return null;
        }

      /*  protected override void ScanLeftSideOfSector(SubtileSector sector, Point otherSectorCoords)
        {
            int layerIndex;
            SubtileSector leftSector = layers.GetSector(otherSectorCoords.X, otherSectorCoords.Y, out layerIndex);

            if (layerIndex == 0)
            {
                // connecting to terrain map:
                // add connectors:
                bottomToUpperLayerConnectors.Add();
            }
            else
            {
                // connecting to same layer sector:

            }

        }*/

        private void AddConnectors(/*SubtileSector overriddenSector,*/ ushort fromRegion, ushort toRegion)
        {
            ushort bottomLayerRegion = Math.Min(fromRegion, toRegion);
            ushort topLayerRegion = Math.Max(fromRegion, toRegion);

            if (bottomLayerRegion != 0 && topLayerRegion != 0)
            {
                AddLayerConnector(bottomLayerRegion, topLayerRegion);
            }

          //  BlockBottomConnection(overriddenSector, bottomLayerRegion);
        }


        private void AddLayerConnector(ushort bottomRegionColor, ushort topRegionColor)
        {
            // add 2-way connectors to our own region graph, so we don't interfere with the base terrain graph

        /*    ushort bottomRegionColor = Math.Min(region1, region2);
            ushort topRegionColor = Math.Max(region1, region2);*/

            Dictionary<ushort, RegionEdge> setOfEdges;

            Region bottomRegion = terrainRegionMap.BottomRegions[bottomRegionColor];// <- looks up in terrain map... //  terrainRegionMap.newRegions[bottomRegionColor];
            Region topRegion = newRegions[topRegionColor];
            float distanceBetweenRegionCenters = Common.DistanceOctile(bottomRegion.CenterLocation, topRegion.CenterLocation);             

            if (!newRegionGraph.TryGetValue(bottomRegionColor, out setOfEdges))
            {               
                AddEdge(newRegionGraph, bottomRegionColor, topRegionColor, distanceBetweenRegionCenters);
            }
            else if (!setOfEdges.ContainsKey(topRegionColor))
            {
                setOfEdges.Add(topRegionColor, new RegionEdge(bottomRegionColor, topRegionColor, distanceBetweenRegionCenters, false));         
            }
            else
            {
                return;
            }


            // save for easy cleanup...
            Common.AddToMultiList(bottomToUpperLayerConnectors, bottomRegionColor, topRegionColor);

            // now the other direction:
            if (!newRegionGraph.TryGetValue(topRegionColor, out setOfEdges))
            {
                setOfEdges = new Dictionary<ushort, RegionEdge>();
                newRegionGraph.Add(topRegionColor, setOfEdges);
            }

            // the key is the region the edge points to!
            setOfEdges.Add(bottomRegionColor, new RegionEdge(topRegionColor, bottomRegionColor, distanceBetweenRegionCenters, false));
                         
        }

    /*    private void AddTopToBottomLayerConnector(ushort fromRegionColor, ushort toRegionColor)
        {
            // add a normal edge to the top graph
            // this has to be removed when rebuilding the graph...
            Region fromRegion = newRegions[fromRegionColor];
            Region toRegion = newRegions[toRegionColor];
            float distanceBetweenRegionCenters = Common.DistanceOctile(fromRegion.CenterLocation, toRegion.CenterLocation);

            // !! the key is the region the edge points to!
            Dictionary<ushort, RegionEdge> setOfEdges;

            // get the set of edges leading from this region:
            if (!newRegionGraph.TryGetValue(fromRegionColor, out setOfEdges))
            {
                AddEdge(newRegionGraph, fromRegionColor, toRegionColor, distanceBetweenRegionCenters);
            }
            else if (!setOfEdges.ContainsKey(toRegionColor))
            {
                setOfEdges.Add(toRegionColor, new RegionEdge(fromRegionColor, toRegionColor, distanceBetweenRegionCenters, false));         
         
            }
        }*/

        protected override bool RemoveAllEdgesBetweenLayers()
        {
           
            foreach (var item in bottomToUpperLayerConnectors)
            {
                Dictionary<ushort, RegionEdge> edges;

                if (newRegionGraph.TryGetValue(item.Key, out edges))
                {
                    foreach (var destination in item.Value)
                    {
                        edges.Remove(destination);   // the key is the region the edge is going to!


                        Dictionary<ushort, RegionEdge> otherDirectionEdges;
                        // remove the edge in reverse direction
                        if (newRegionGraph.TryGetValue(destination, out otherDirectionEdges))
                        {
                            otherDirectionEdges.Remove(item.Key);
                        }
                    }

                }
            }

            bottomToUpperLayerConnectors.Clear();
            newBottomBlockedEdges.Clear(); // clear blockers too

            return true;

        }

        protected override bool BuildRegionGraphBetweenSectors()
        {
            if (ID == (CyclableID)202
                && The.Sim.TotalUnPausedGameTimeInSeconds > 15)
            {

            }


            bool isLastSector;

            SubtileSector sector = GetCurrentSector(out isLastSector); // scan all sectors, not just dirty ones.

            if (sector != null)
            {
                SubtileSector overiddenSector = layers.GetOverriddenSector(sector.Coords.X, sector.Coords.Y);
                ScanSectorEdges(sector, overiddenSector);
            }

            return isLastSector;
        }


        protected override void BlockBottomConnection(ushort overriddenRegion, ushort otherRegion)
        {
            if (overriddenRegion != 0 && otherRegion != 0)
            {               
                Common.AddToMultiList(newBottomBlockedEdges, overriddenRegion, otherRegion);
                Common.AddToMultiList(newBottomBlockedEdges, otherRegion, overriddenRegion); // both directions!
            }
        }

        /*
        private void BlockBottomConnection(SubtileSector overriddenSector, ushort bottomRegion)
        {
            Dictionary<ushort, RegionEdge> setOfEdges;

            if (BottomRegionGraph.TryGetValue(bottomRegion, out setOfEdges))
            {
                foreach (var item in setOfEdges)
	            {   
                    // look up in the overridden sector's regions. block all edges going there
		            if (overriddenSector.Regions.Contains(item.Key))
                    {
                        Common.AddToMultiList(bottomBlockedEdges, bottomRegion, item.Key);
                    }
                }               
            }
        }*/

        protected override void AddEdgesOrConnectors(ushort fromRegion, ushort toRegion)
        {

            if (fromRegion >= ColorStartOfRange && toRegion >= ColorStartOfRange)
            {
                // 1. both regions are on the top layer
                AddEdgesIfNotExists(fromRegion, toRegion);
            }
            else
            {
                // 2. regions are on two different layers
                // (the last case 3. is handled by TerrainRegionMap.)

                AddConnectors(fromRegion, toRegion);
            }
        }


        public override ISnapshot DoSnapshot(Snapshotter sn)
        {
            base.DoSnapshot(sn);

            parentMovementMap = sn.DoEnum(parentMovementMap);
            onTerrainRegionsAreInvalidID = sn.DoEnum(onTerrainRegionsAreInvalidID);
            onTerrainRegionsAreDoneID = sn.DoEnum(onTerrainRegionsAreDoneID);
            bottomBlockedEdges = sn.DoMultiMapHashSet(bottomBlockedEdges);
            bottomToUpperLayerConnectors = sn.DoMultiMap(bottomToUpperLayerConnectors);
            isWaitingForTerrainRegions = sn.DoBool(isWaitingForTerrainRegions); // if the terrain map is busy during save, it will still be busy after load.
            regionColorsToRemoveRollbackAfterSave = sn.DoHashSet(regionColorsToRemoveRollbackAfterSave); // our roll back point.
            waitForRebuildAfterRemovedSector = sn.DoBool(waitForRebuildAfterRemovedSector);

            snapshotTerrainRegionMap = (CyclableID)sn.SnapshotID<ICyclable, CyclableID>(terrainRegionMap);

            sn.Ignore(terrainRegionMap);
            sn.Ignore(regionColorsToRemove);
            sn.Ignore(newBottomBlockedEdges);
            sn.Ignore(totalComputationAllInstancesInSeconds);
            sn.Ignore(StartedOnTimeInSeconds);

            return this;
        }

        public override void LoadPostProcess(Snapshotter sn)
        {
            base.LoadPostProcess(sn);
            sn.RegisterLoadPostProcessCall(this);

            terrainRegionMap = (TerrainRegionMap)LookUp<ICyclable, CyclableID>.FindByID(snapshotTerrainRegionMap);

            if (regionColorsToRemoveRollbackAfterSave != null 
                && regionColorsToRemoveRollbackAfterSave.Count > 0)
            {
                sectorsAreDirty = true;
                regionColorsToRemove = new HashSet<ushort>(regionColorsToRemoveRollbackAfterSave); // roll back
            }


            LoadPostProcessRegisterMethodIDs();
        } 


        public void LoadPostProcessRegisterMethodIDs()
        {
            ActionLookup.Add(onTerrainRegionsAreInvalidID, OnTerrainRegionsAreInvalid);
            ActionLookup.Add(onTerrainRegionsAreDoneID, OnTerrainRegionsAreDone);
            
        }

    }
}
