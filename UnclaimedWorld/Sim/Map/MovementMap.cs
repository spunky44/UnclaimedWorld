using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI.Pathfinding;
using System.Threading.Tasks;
using UWGame.SimSide.AI;
using UWGame.SimSide.Systems.TimeSlicing;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Maps.Regions;
namespace UWGame.SimSide.Maps
{

    /// <summary>
    /// A movement map is the sum of the terrain costs map and a discomfort map, relevant to the type of entity.
    /// it contains the most values, 8 per tile.
    /// 
    /// holds a Layers instance with 2 layers - one for the terrain map and one for the sum of terrain, discomfort and threats.
    /// When this class is destroyed, only destroy its own layer, not the shared terrain layer.
    /// </summary>
    public class MovementMap : ICyclable, ISnapshot
    {
        /// <summary>
        /// make these private. Contains holes.
        /// 
        /// should this include flags?? or only cost/is blocked status?
        /// </summary>
        private Dictionary<SurfaceType.TransportType, SubtileLayer> moveMapLayer;
        private Dictionary<SurfaceType.TransportType, SubtileLayerID> snapshotMoveMapLayer; // for snapshotting the above


        /// <summary>
        /// NEW - clients will use these for pathfinding etc.
        /// </summary>
        private Dictionary<SurfaceType.TransportType, SubtileLayers> layers;
        private Dictionary<SurfaceType.TransportType, SubtileLayersID> snapshotClientLayers; // for snapshotting the above


        /// <summary>
        /// Also for client use. The layers contain a reference to the terrain layer and an overriding MoveMap layer with sector holes.        
        /// Their cost have an added value from any threats or discomfort that the owning allegiance know about.  
        ///       
        /// </summary>
        public Dictionary<SurfaceType.TransportType, SubtileLayers> Layers
        {
            get
            {
                return layers; // subtileArrays;
            }
        }

        /// <summary>
        /// used to track changes to the shared terrain map with the IsDirty flag:
        /// only used for adding subtile sectors? not to recompute regions...?
        /// This object is unique to each move map, so they can track progress independently...
        /// </summary>
        private Dictionary<SurfaceType.TransportType, Sector[][]> terrainSectors;
        private bool terrainSectorsAreDirty = false;
        // int terrainSectorWidthInSubtiles;


        /// <summary>
        /// only has a discomfort map as child
        /// </summary>
        public List<Dependence> Children = new List<Dependence>();


        public string IDName;

        public bool IsPaused { get; set; }
        public double StartedOnTimeInSeconds { get; set; }
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

        /// <summary>
        /// NEVER USED???
        /// 
        /// this is set true when the map has finished updating itself (could take several cycles) and can be used in pathfinding.
        /// 
        /// </summary>
        public bool IsReady { get; set; }


        // public bool ComputeAll = false;

        /// <summary>
        /// only humans have more than one transport map
        /// </summary>
        private SurfaceType.TransportType[] transportsToInclude;

        private int cycleRegionMapTransportIndex = 0;

       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="weight"></param>
        /// <param name="childDMap"></param>
        /// <param name="childThreatMap"></param>
        /// <param name="terrainMap"></param>
        public MovementMap(float weight, DiscomfortMap childDMap, ThreatMap childThreatMap, string idName, float updateInterval, params SurfaceType.TransportType[] transportsToInclude)
        {
            AddToLookup();

            The.Map.AllMovementMaps.Add(this);

            this.IDName = idName;

            this.updateInterval = updateInterval;

            if (transportsToInclude.Length == 0)
            {
                transportsToInclude = new SurfaceType.TransportType[] { SurfaceType.TransportType.Foot, SurfaceType.TransportType.Car, SurfaceType.TransportType.OffRoad };
            }

            this.transportsToInclude = transportsToInclude;

            InitMap();

            InitTerrainSectors();

            // The.Map.MovementMaps.Add(this);

            Dependence dep1 = new Dependence(childDMap, weight); // ThreatMap.GetThreatApproachFactor(EntityApproach.Bold));//EntityApproach.Normal));  
            Children.Add(dep1);
            //childDMap.Parent = dep1;

           // shared.AddMovementMap(this, childDMap, childThreatMap);

           
            CreateRegulators();

            GetCurrent(); // init...

        }


        public MovementMap()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }

        /*
        public void BlockSubtile(SurfaceType.TransportType transport, int x, int y)
        {
            SubtileLayer thisMap;
            if (Layers.TryGetValue(transport, out thisMap))
            {
                // #SECTORS TODO - test why flags were previously removed here by using a 0? Maybe because the move map does not contain flags.
                // create a new sector if not filled???
                MapManager.SubtileValue newValue = MapManager.SetCost(thisMap.GetValue(x, y), MapManager.SubtileValue.Blocked); // 0);
             
                thisMap.SetValue(x, y, ); // = 0;

               // thisMap.Values[x][y] = 0; // OLD: removes flags??
            }
        }*/

        /// <summary>
        /// I believe it is called when not blocking? But terrain costs are not used much beyond block/unblock status...
        /// 
        /// called when blocking changes on the terrain map... which doesn't happen too often
        /// </summary>
        /// <param name="transport"></param>
        /// <param name="subtile"></param>
        public void SetTerrainSectorDirty(SurfaceType.TransportType transport, Point subtile)
        {
            Sector[][] transportSectors;

            if (terrainSectors.TryGetValue(transport, out transportSectors))
            {
                Sector sector = transportSectors[subtile.X / MapManager.SectorSizeInSubtiles][subtile.Y / MapManager.SectorSizeInSubtiles];

                sector.SubtilesAreDirty = true; // triggers re-adding of sector subtiles
             
                terrainSectorsAreDirty = true;

                // OLD:
                // blocked status has changed.
                // mark the region as dirty:                
                /*  RegionMap regionMap = RegionMap[transport];

                  // is the region map created yet?
                  if (regionMap.RegionGraph.Count > 0)
                  {
                      regionMap.MarkDirtyPoint(subtile.X, subtile.Y);
                  }*/
            }
        }


        private void InitMap()
        {
            Dictionary<SurfaceType.TransportType, SubtileLayers> terrainMap = The.Map.TerrainCosts;

            ushort width = (ushort)The.Map.mapSubtileWidth; // Common.GetJaggedArrayWidth(terrainMap[SurfaceType.TransportType.Foot]);
            ushort height = (ushort)The.Map.mapSubtileHeight; // Common.GetJaggedArrayHeight(terrainMap[SurfaceType.TransportType.Foot]);


            if (moveMapLayer == null)
            {
                moveMapLayer = new Dictionary<SurfaceType.TransportType, SubtileLayer>();
                snapshotMoveMapLayer = new Dictionary<SurfaceType.TransportType, SubtileLayerID>();
                layers = new Dictionary<SurfaceType.TransportType, SubtileLayers>();
                snapshotClientLayers = new Dictionary<SurfaceType.TransportType, SubtileLayersID>();

                foreach (KeyValuePair<SurfaceType.TransportType, SubtileLayers> kvp in terrainMap)
                {
                    // critter allegiances don't need all the terrain maps.
                    if (this.transportsToInclude.Length == 0 || Array.Exists(this.transportsToInclude, t => t == kvp.Key))
                    {
                        SubtileLayer layer = new SubtileLayer(width, height);
                        moveMapLayer.Add(kvp.Key, layer);
                        snapshotMoveMapLayer.Add(kvp.Key, layer.ID);

                        SubtileLayers terrainLayers = terrainMap[kvp.Key];
                        SubtileLayers allLayers = new SubtileLayers(this, this.IDName + kvp.Key.ToString(), terrainLayers.Layers[0], layer, (TerrainRegionMap)terrainLayers.RegionMap); // point to terrain map

                        layers.Add(kvp.Key, allLayers);
                        snapshotClientLayers.Add(kvp.Key, allLayers.ID);

                        /* RegionMap regionMap = new DependentRegionMap(Layers[transport]) { IDName = this.IDName + kvp.Key.ToString() };

                         RegionMap.Add(transport, regionMap);
                         snapshotRegionMaps.Add(transport, regionMap.ID);*/

                    }
                }
            }


            /*
            Dictionary<SurfaceType.TransportType, SubtileLayers> terrainMap = The.Map.TerrainCosts;

            int width = Common.GetJaggedArrayWidth(terrainMap[SurfaceType.TransportType.Foot]);
            int height = Common.GetJaggedArrayHeight(terrainMap[SurfaceType.TransportType.Foot]);
                                 

            if (subtileArrays == null)
            {
                subtileArrays = new Dictionary<SurfaceType.TransportType, SubtileLayer>();
                subtileArrayIDs = new Dictionary<SurfaceType.TransportType, SubtileLayerID>();

                // only for humans!!!
                // we want a new terrain type for large entities too!!!
                foreach (KeyValuePair<SurfaceType.TransportType, MapManager.SubtileValue[][]> kvp in terrainMap)
                {
                    if (this.transportsToInclude.Length == 0 || Array.Exists(this.transportsToInclude, t => t == kvp.Key))
                    {
                        SubtileLayer array = new SubtileLayer(width, height);                       
                        subtileArrays.Add(kvp.Key, array);

                        subtileArrayIDs.Add(kvp.Key, array.ID);
                    }
                }

            }*/
        }

        private void InitTerrainSectors()
        {
            //  terrainSectorWidthInSubtiles = MapManager.SubtilesPerTileLength * InfluenceMap.SectorSizeInTiles;

            terrainSectors = new Dictionary<SurfaceType.TransportType, Sector[][]>();

            //   foreach (var item in subtileValues)
            foreach (var item in moveMapLayer)
            {
                Sector[][] transportSectors = null;

                Common.InitJaggedArray(ref transportSectors, The.Map.NoOfSectorsAcrossWidth, The.Map.NoOfSectorsAcrossHeight); // (int)Math.Ceiling((double)mapWidth / (double)sectorWidth), (int)Math.Ceiling((double)mapHeight / (double)sectorWidth));

                for (int x = 0; x < Common.GetJaggedArrayWidth(transportSectors); x++)
                {
                    for (int y = 0; y < Common.GetJaggedArrayHeight(transportSectors); y++)
                    {
                        transportSectors[x][y] = new Sector(new Point(x, y), MapManager.SectorSizeInSubtiles, The.Map.mapSubtileWidth, The.Map.mapSubtileHeight);
                    }
                }

                terrainSectors.Add(item.Key, transportSectors);
            }
        }


        private enum Phase { ComputeChildMaps, GetSectorsToAdd, AddChildMaps, RemoveEmptySectors, ComputeRegionMap, Completed }
        const Phase StartPhase = Phase.ComputeChildMaps;
        
        private Phase phase = StartPhase; 
       

        /// <summary>
        /// is a list instead of a hashSet because we need indexing
        /// </summary>
        List<Point> listOfSectorsToAdd = new List<Point>();

        //Dictionary<SurfaceType.TransportType, List<SubtileSector>> removedSectors = new Dictionary<SurfaceType.TransportType, List<SubtileSector>>();
        bool sectorsWereRemoved = false;
        //List<Point> listOfRemovedSectors = new List<Point>();


        public DiscomfortMap GetDiscomfortMap()
        {
            return (DiscomfortMap)Children[0].Child;
        }

        public bool CycleOnce()
        {
            DiscomfortMap dMap = GetDiscomfortMap();


            if (IDName == "ExposedHumanNormal") // "ExposedHumanNormal")
            {

            }

            switch (phase)
            {
                case Phase.ComputeChildMaps:
                    // recompute the child maps, keeping track of which parts (sectors) actually change
                    if (dMap.DoCycle())
                    {
                        phase = Phase.RemoveEmptySectors;
                    }

                    return false;

                case Phase.RemoveEmptySectors:
                    RemoveEmptySectors();
                    

                    phase = Phase.GetSectorsToAdd;
                    return false;

                case Phase.GetSectorsToAdd:

                    GetSectorsToAdd();

                    phase = Phase.AddChildMaps; 

                    IsReady = false; // lock the map...
                    
                    return false;

                case Phase.AddChildMaps:
                    // here we start modifying the map while there may be ongoing searches.
                    // we allow them to continue but check each cycle if the endpoints are free...

                    if (AddChildMapSectors(dMap))
                    {
                        phase = Phase.ComputeRegionMap;

                        // restart searches???
                        // set IsReady = true
                      
                    }
                  
                    return false;
                    
                case Phase.ComputeRegionMap:
                    // compute the associated region maps directly instead of waiting for an update:
                    // if the region map is waiting for terrain regions to finish,
                    // Movement Map should be paused.
                    if (ComputeRegionMaps())
                    {
                        phase = Phase.Completed; // StartPhase;
                        //IsReady = true; never used...
                        return true;                       
                    }

                    return false;


            }

            return false;
            
        }

       


        bool ComputeRegionMaps()
        {
             SurfaceType.TransportType transport = transportsToInclude[cycleRegionMapTransportIndex];

             DependentRegionMap regionMap = (DependentRegionMap)Layers[transport].RegionMap;

            if (regionMap.IsPaused)
            {
                this.IsPaused = true; 

                return false; // NEW!
            }

            // before starting the region map, reset the progress point depending on whether it has dirty sectors or not.
            regionMap.ResetComputePointIfNeeded();

            if (regionMap.CycleOnce())
            {
                cycleRegionMapTransportIndex++;
            }

            if (cycleRegionMapTransportIndex == transportsToInclude.Length)
            {
                // finished
                cycleRegionMapTransportIndex = 0;
                return true;
            }

            return false;
        }

        void RemoveEmptySectors()
        {
            // gather destroyed regions, remove them in RegionMap.RemoveRegions()
            sectorsWereRemoved = false;
            //removedSectors.Clear();

            /// remove sectors 
            /// when no child sector exists.

            List<Point> allSectors = new List<Point>();
            GetAllSectors(allSectors);

            foreach (var sectorCoords in allSectors)
            {
                if (Children.TrueForAll(d => d.Child.Map.GetSector(sectorCoords.X, sectorCoords.Y) == null))
                {
                    foreach (var map in moveMapLayer)
                    {
                        SubtileSector sector = map.Value.Sectors[sectorCoords.X][sectorCoords.Y];
                        if (sector != null)
                        {
                            DestroySector(sector, map.Key, map.Value);

                            sectorsWereRemoved = true;
                            //Common.AddToMultiList(removedSectors, map.Key, sector);

                        }

                    }
                }
            }     
        }

        private void DestroySector(SubtileSector sector, SurfaceType.TransportType transport, SubtileLayer map)
        {
            
            RegionMap regionMap = Layers[transport].RegionMap;

            
            // sector.Destroy(regionMap);
            map.Sectors[sector.Coords.X][sector.Coords.Y] = null; // release the memory

            HashSet<ushort> sectorRegions = sector.GetRegions();          
            ((DependentRegionMap)regionMap).SetRegionsFromRemovedSectors(sectorRegions);


#if DEBUG


            if (regionMap != null)
            {
                regionMap.AddLog(string.Format("Sector removed, Coords: {0}, Created on: {1}", sector.Coords, sector.CreatedOn));
            }

#endif
        }


        private void GetSectorsToAdd()
        {
            listOfSectorsToAdd.Clear();

            // get the sectors in the child map that have changed
            List<Point> discomfortSectorsToAdd = null;
            List<Point> allDiscomfortSectors = null;
            GetDiscomfortSectorsToAdd(ref allDiscomfortSectors, ref discomfortSectorsToAdd);

            if (discomfortSectorsToAdd != null)
            {
                listOfSectorsToAdd.AddRange(discomfortSectorsToAdd);
            }

            if (allDiscomfortSectors != null)
            {
                HashSet<Point> setOfDirtyTerrainSectors = new HashSet<Point>();

                // examine the terrain map for change in sectors too:
                if (terrainSectorsAreDirty)
                {
                    GetSectorsToAddFromTerrain(setOfDirtyTerrainSectors);

                    if (setOfDirtyTerrainSectors.Count > 0)
                    {
                        List<Point> listOfDirtyTerrainSectors = setOfDirtyTerrainSectors.ToList();
                       
                        // don't add terrain sectors if the discomfort sector does not exist:
                        listOfDirtyTerrainSectors.RemoveAll(s => !allDiscomfortSectors.Contains(s));

                        listOfSectorsToAdd.AddRange(listOfDirtyTerrainSectors);
                    }
                }
            }
        }

      /*  public void NotifyRegionMapPaused()
        {
            if (phase == Phase.ComputeRegionMap && !IsPaused)
            {
                IsPaused = true;
            }
        }*/

        public void NotifyRegionMapUnpaused()
        {
            if (phase == Phase.ComputeRegionMap && IsPaused)
            {
                // we can move on now
                IsPaused = false;
            }
            else if (!The.Sim.CycleManager.IsRegistered(this) && this.phase == Phase.Completed) // .ComputeChildMaps)
            {                
                // if movemap is not computing, start it immediately, region maps need to rebuild their layer graphs

                phase = Phase.ComputeRegionMap;

                The.Sim.CycleManager.Register(this, CycleManager.Priority.High);    
            }
        }

        private void GetAllSectors(List<Point> listOfSectors)
        {
            int sectorWidth = The.Map.NoOfSectorsAcrossWidth; // Common.GetJaggedArrayWidth(Children[0].Child.Sectors); //width / InfluenceMap.SectorSizeInTiles;
            int sectorHeight = The.Map.NoOfSectorsAcrossHeight; // Common.GetJaggedArrayHeight(Children[0].Child.Sectors); //height / InfluenceMap.SectorSizeInTiles; //Common.GetJaggedArrayHeight(sectors);

            // compile the list of Sectors to clear and add:

            for (int x = 0; x < sectorWidth; x++)
            {
                for (int y = 0; y < sectorHeight; y++)
                {
                    listOfSectors.Add(new Point(x, y));

                }
            }

        }

        /// <summary>
        /// add maps by sector
        /// only clear and add sectors where something is dirty
        /// otherwise keep the sector as it is!
        /// </summary>
        /// <param name="mapToAdd"></param>
        /// <param name="weight"></param>
        private void GetDiscomfortSectorsToAdd(ref List<Point> listOfAllSectors, ref List<Point> listOfDirtySectors)
        {

            /* int sectorWidth = Common.GetJaggedArrayWidth(Children[0].Child.Sectors); //width / InfluenceMap.SectorSizeInTiles;
             int sectorHeight = Common.GetJaggedArrayHeight(Children[0].Child.Sectors); //height / InfluenceMap.SectorSizeInTiles; //Common.GetJaggedArrayHeight(sectors);
             */

            // compile the list of Sectors to clear and add:

            /*   for (int x = 0; x < sectorWidth; x++)
               {
                   for (int y = 0; y < sectorHeight; y++)
                   { */
            foreach (Dependence child in Children)
            {
                child.Child.Map.GetAllSectors(ref listOfAllSectors);

                child.Child.Map.GetDirtySectors(ref listOfDirtySectors);

            }

           /* if (listOfAllSectors != null)
            {
                listOfDirtySectors = listOfAllSectors.Select(s => s.)
            }*/
          
        }


        private int cycleSectorIndex = 0;

        private void GetSectorsToAddFromTerrain(HashSet<Point> listOfSectors)
        {
            // let's clear the flags as we see them
            if (terrainSectorsAreDirty)
            {
                Sector sector;

                foreach (var transportSectors in terrainSectors)
                {
                    int width = Common.GetJaggedArrayWidth(transportSectors.Value);
                    int height = Common.GetJaggedArrayHeight(transportSectors.Value);

                    for (int x = 0; x < width; x++)
                    {
                        for (int y = 0; y < height; y++)
                        {
                            sector = transportSectors.Value[x][y];
                            if (sector.SubtilesAreDirty)
                            {
                                listOfSectors.Add(new Point(x, y));

                                sector.SubtilesAreDirty = false;
                            }
                        }
                    }
                }

                terrainSectorsAreDirty = false;
            }
        }


        private bool AddChildMapSectors(DiscomfortMap dMap)
        {
            if (listOfSectorsToAdd.Count == 0)
            {
                if (!sectorsWereRemoved) // removedSectors.Count == 0)
                {
                    phase = Phase.ComputeChildMaps;
                    return true;
                }
                else
                {
                    // skip adding...
                    phase = Phase.ComputeRegionMap;
                    return false;
                }
            }


            // cancel current searches???
            // set IsReady = false


            // each sector comprises (3 * 16 = 48) * 48 subtiles = 2400 operations
            // do one sector each cycle:                      
            Point sectorCoords = listOfSectorsToAdd[cycleSectorIndex];
            TileSector sector = dMap.Map.Sectors[sectorCoords.X][sectorCoords.Y];

            Parallel.ForEach(transportsToInclude, transport =>
            {
                RegionMap regionMap = layers[transport].RegionMap; 

                // is the region map created yet?
             //   bool updateRegionsAsDirty = regionMap.RegionGraph.Count > 0;

                // vehicle maps should have lower weight since they offer protection:
                // XXX is hackish?
                float factor = 1f;
                if (transport == SurfaceType.TransportType.Foot)
                {
                    factor = 1f;
                }
                else
                {
                    factor = 0.25f;
                }

                
                SubtileSector transportTerrainSector = The.Map.TerrainCosts[transport].GetSector(sectorCoords.X, sectorCoords.Y);
                TileSector dMapSector = dMap.Map.GetSector(sectorCoords.X, sectorCoords.Y);

             
                // this is for the new values: 
                // if creating a new sector, mark regions as dirty too.
                // create new sector if it does not exist:
                bool isNewSector;
                SubtileSector sectorToAddInto = moveMapLayer[transport].GetOrCreateSector(sectorCoords.X, sectorCoords.Y, out isNewSector);
                MapManager.SubtileValue[][] transportAddToMap = sectorToAddInto.Values;
               
                if (isNewSector)
                {
                    regionMap.MarkDirtySector(sectorToAddInto);                    
                }

                byte value = 0, newValue, oldValue;
                byte terrainCost;

                bool isBlocked = false;

                int subtileStartX, subtileStartY;

                MapManager.SubtileValue[] subtileColumn; /*oldSubtileColumn,*/
                byte[] tileColumn = null;
                bool[] tileIsBlockedColumn = null;

                /*
                if (dMapSector == null)
                {
                    value = 0;
                    isBlocked = false;
                }*/

                for (int tileX = 0; tileX < sector.TileArea.Width; tileX++)
                {
                    if (dMapSector != null)
                    {
                        tileColumn = dMapSector.Map[tileX];
                        tileIsBlockedColumn = dMapSector.IsBlocked[tileX];
                    }

                    for (int tileY = 0; tileY < sector.TileArea.Height; tileY++)
                    {
                        subtileStartX = 3 * tileX;
                        subtileStartY = 3 * tileY;

                        if (dMapSector != null)
                        {
                            value = (byte)(factor * tileColumn[tileY]);
                            isBlocked = tileIsBlockedColumn[tileY];
                        }

                        for (int subtileX = subtileStartX; subtileX < subtileStartX + 3; subtileX++)
                        {
                            subtileColumn = transportAddToMap[subtileX];
                            //  oldSubtileColumn = oldTransportAddToMap[subtileX];

                            for (int subtileY = subtileStartY; subtileY < subtileStartY + 3; subtileY++)
                            {
                                // we want to check whether each subtile changes status from blocked/unblocked, so we compare with the old map

                                if (isBlocked == true)
                                {
                                    newValue = 0;
                                }
                                else
                                {
                                    terrainCost = MapManager.GetCost(transportTerrainSector.Values[subtileX][subtileY]);

                                    if (terrainCost > 0)
                                    {   // only if the discomfort map and the terrain map values are not blocked do we add the two (with clamping):
                                        newValue = (byte)Common.ClampTop(terrainCost + value, MapManager.MaxTerrainCost); // 255);
                                    }
                                    else
                                    {
                                        // blocked terrain map edges should stay blocked (=0):
                                        newValue = 0;
                                    }
                                }

                                //we don't need a double buffer if we compare each subtile...
                                oldValue = MapManager.GetCost(subtileColumn[subtileY]);//
                                subtileColumn[subtileY] = MapManager.SetCost(subtileColumn[subtileY], newValue);

                                if (!isNewSector &&
                                   // updateRegionsAsDirty &&
                                    ((newValue == 0 && oldValue != 0) ||
                                    (newValue != 0 && oldValue == 0)))
                                {
                                    // blocked status has changed.
                                    // mark the region as dirty:
                                    regionMap.MarkDirtySector(sectorToAddInto); //subtileX, subtileY);
                                }
                            }

                        }

                    }
                }               

            });


            /*
            Parallel.ForEach(transportsToInclude, transport =>
            {
                RegionMap regionMap = RegionMap[transport];
                
                // is the region map created yet?
                bool updateRegionsAsDirty = regionMap.RegionGraph.Count > 0;

                // vehicle maps should have lower weight since they offer protection:
                // XXX is hackish?
                float factor = 1f;
                if (transport == SurfaceType.TransportType.Foot)
                {
                    factor = 1f;
                }
                else
                {
                    factor = 0.25f;
                }

                // this is for the new values:
                MapManager.SubtileValue[][] transportAddToMap;//, oldTransportAddToMap; 

                MapManager.SubtileValue[][] transportTerrainMap; 

             //   transportAddToMap = subtileValues[transport];
                transportAddToMap = subtileArrays[transport].Values;

              

                transportTerrainMap = The.Map.TerrainCosts[transport];
                SubtileSector terrainSector = The.Map.TerrainCosts[transport].GetSector(sectorCoords.X, sectorCoords.Y);
           
                byte value, newValue, oldValue;
                byte terrainCost;

                bool isBlocked;

                int subtileStartX, subtileStartY;

                MapManager.SubtileValue[] subtileColumn; 
                byte[] tileColumn;
                bool[] tileIsBlockedColumn;

                for (int tileX = sector.TileArea.Left; tileX < sector.TileArea.Right; tileX++)
                {
                    tileColumn = dMap.Map[tileX];
                    tileIsBlockedColumn = dMap.IsBlocked[tileX];

                    for (int tileY = sector.TileArea.Top; tileY < sector.TileArea.Bottom; tileY++)
                    {
                        subtileStartX = 3 * tileX;
                        subtileStartY = 3 * tileY;

                        value = (byte)(factor *  tileColumn[tileY]);

                        isBlocked = tileIsBlockedColumn[tileY];

                        for (int subtileX = subtileStartX; subtileX < subtileStartX + 3; subtileX++)
                        {
                            subtileColumn = transportAddToMap[subtileX];
                          //  oldSubtileColumn = oldTransportAddToMap[subtileX];

                            for (int subtileY = subtileStartY; subtileY < subtileStartY + 3; subtileY++)
                            {
                                // we want to check whether each subtile changes status from blocked/unblocked, so we compare with the old map

                                if (isBlocked == true)
                                {
                                    newValue = 0;
                                }
                                else
                                {
                                    terrainCost = MapManager.GetCost(transportTerrainMap[subtileX][subtileY]);

                                    if (terrainCost > 0)
                                    {   // only if the discomfort map and the terrain map values are not blocked do we add the two (with clamping):
                                        newValue = (byte)Common.ClampTop(terrainCost + value, MapManager.MaxTerrainCost); // 255);
                                    }
                                    else
                                    {
                                        // blocked terrain map edges should stay blocked (=0):
                                        newValue = 0;
                                    }
                                }

                                //we don't need a double buffer if we compare each subtile...
                                oldValue = MapManager.GetCost(subtileColumn[subtileY]);//
                                subtileColumn[subtileY] = MapManager.SetCost(subtileColumn[subtileY], newValue);

                                if (updateRegionsAsDirty && 
                                    ((newValue == 0 && oldValue != 0) ||
                                    (newValue != 0 && oldValue == 0)))
                                {
                                    // blocked status has changed.
                                    // mark the region as dirty:
                                    regionMap.MarkDirtyPoint(subtileX, subtileY);                                    
                                }
                            }

                        }

                    }
                }

            });*/


            cycleSectorIndex++;

            if (cycleSectorIndex == listOfSectorsToAdd.Count)
            {
                // finished
                phase = Phase.ComputeChildMaps;
                cycleSectorIndex = 0;

                return true;
            }

            return false;
        }



        /*   private bool AddChildMaps(DiscomfortMap dMap)
           {
               float factor = 1f;
            
               // cycle once...
               // do one horizontal tile slice:

               // vehicle maps should have lower weight since they offer protection:
               // XXX is hackish?
               if (cycleAddTransport == TerrainType.TransportType.Foot)
               {
                   factor = 1f;
               }
               else
               {
                   factor = 0.25f;
               }


               //   int subTileCounter = 0;
               //    int tileX = 0;


               int mapWidth = cycleTerrainMap.GetLength(0);

               int subTileStartY = cycleTileY * 3;


           

               // do one horizontal tile slice (=3 subtile slices)
               // byte[] cycleAddToMapRow;
               Parallel.For(subTileStartY, subTileStartY + 3, (y) =>
               {
                   // very important - declare variables for each thread!
                   //  byte[] column, columnToAdd;
                   //  bool[] columnIsBlocked, columnToAddIsBlocked;

                   //            for (int y = subTileStartY; y < subTileStartY + 3; y++)
                   //          {
                   int subTileCounter = 0;
                   int tileX = 0;

                   byte value;
                   byte oldValue;

                   for (int x = 0; x < mapWidth; x++)
                   {

                       if (dMap.IsBlocked[tileX][cycleTileY] == true)
                       {
                           cycleAddToMap[x][y] = 0;
                       }
                       else
                       {

                           value = (byte)(factor * weightToAddWith * dMap.Map[tileX][cycleTileY]);

                           oldValue = cycleTerrainMap[x][y];


                           if (oldValue > 0)
                           {   // only if the discomfort map and the terrain map values are not blocked do we add the two (with clamping):
                               cycleAddToMap[x][y] = (byte)Common.ClampTop(oldValue + value, 255);
                           }
                           else
                           {
                               // blocked terrain map edges should stay blocked (=0):
                               cycleAddToMap[x][y] = 0;
                           }
                       
                       }

                       ++subTileCounter; // this seems to be a bit faster than doing divisions:
                       if (subTileCounter > 2)
                       {
                           subTileCounter = 0;
                           ++tileX;
                       }

                   }

               });

               cycleTileY++;

               int height = Common.GetJaggedArrayHeight(dMap.Map);

               if (cycleTileY == height)
               {
                   cycleTileY = 0;

                   if (CycleAddTransportIndex == MapManager.MapTransportTypeArray.Count - 1)
                   {
                       // finished!!!
                       // reset:
                       CycleAddTransportIndex = 0;

                       phase = Phase.ComputeChildMaps;

                       return true;
                   }
                   else
                   {
                       CycleAddTransportIndex++;
                   }
               }

               return false;
           }
           */

        /*  private void ClearTilesInSectors(List<Point> listOfSectors)
          {
              DiscomfortMap dMap = (DiscomfortMap)Children[0].Child;

              Parallel.ForEach(listOfSectors, sectorCoords =>
              {
                  Sector sector = dMap.sectors[sectorCoords.X][sectorCoords.Y];

                  // clear the tiles in this sector and add the children 
                  InfluenceMap.ClearTilesInSector(sector, Map);

                  foreach (Dependence child in Children)
                  {
                      InfluenceMap.AddTilesInSector(sector.TileArea, child.Weight, map, child.Child.Map, child.Child.IsBlocked);
                  }

              });
          }*/

        private static void ClearTilesInSector(Sector sector, byte[][] map) // Point sectorCoords)
        {
            byte[] column;

            for (int x = sector.TileArea.Left; x < sector.TileArea.Right; x++)
            {
                column = map[x];

                for (int y = sector.TileArea.Top; y < sector.TileArea.Bottom; y++)
                {
                    column[y] = 0;

                }
            }
        }


        /// <summary>
        /// resets the map to the values from the terrain map.
        /// </summary>
        /*  public void ClearMap()
          {

              // this takes quite a long time too!
              for (int transport = 0; transport < Map.GetLength(0); transport++)
              {
                  for (int x = 0; x < Map.GetLength(1); x++)
                  {
                      for (int y = 0; y < Map.GetLength(2); y++)
                      {
                          for (int i = 0; i < Map.GetLength(3); i++)
                          {
                              Map[transport, x, y, i] = terrainMap[transport, x, y, i];
                          }
                      }
                  }
              }

          }*/


        public MovementMap GetCurrent()
        {
            /* if (IsDirty() && !The.Sim.CycleManager.IsRegistered(this))
             {   // we need to recompute this map
               
                 Redraw();

                                            
              
                 The.Sim.CycleManager.Register(this, CycleManager.Priority.High);                

             }

             hasChanged = false; // ???
             */

            return this;
        }

        public override string ToString()
        {
            return "Move map: " + IDName;
        }

        public void ClearMap()
        {
            // never call this...
            throw new NotImplementedException();
        }

        private double updateInterval;
        public double? UpdateInterval
        {
            get
            {
                return updateInterval;
            }
        }

        private Regulator regulator;

        void CreateRegulators()
        {
            regulator = new Regulator(The.Sim.GameplayRandomGenerator, 1d / UpdateInterval.Value, "MovementMap");
        }

        public void Update(GameTime gameTime)
        {
           /* if (this.IDName != "ExposedHumanNormal")
            {
                return;
            }*/

            if (!The.Sim.CycleManager.IsRegistered(this))
            {
                double milliSecondsSinceLastReady = 0;
                if (regulator.IsReady(ref milliSecondsSinceLastReady))
                {

                    phase = StartPhase; // Phase.ComputeChildMaps;

                    The.Sim.CycleManager.Register(this, CycleManager.Priority.High);
                }
            }

        }

        public void PrintInfo(StringBuilder text)
        {           
            text.Append(string.Format("Movemap {0}, {1}", IDName, phase));
           
        }

        public void Destroy()
        {
            if (The.Sim.CycleManager.IsRegistered(this))
            {
                The.Sim.CycleManager.UnRegister(this);
            }

            The.Map.AllMovementMaps.Remove(this);

            RemoveIDEntry();

            foreach (var item in layers)
            {
                item.Value.Destroy(false);
            }

            /*
            for (int i = RegionMap.Keys.Count - 1; i >= 0; i--)
            {
                RegionMap regionMap = RegionMap[RegionMap.Keys.ElementAt(i)];

                regionMap.Destroy();
            }*/

        }



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

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            // snapshot the current progress of the time sliced process. This is necessary since we do not have a backup of the starting state to revert to!
            this.id = SnapshotID(sn, id);


            this.terrainSectorsAreDirty = sn.DoBool(terrainSectorsAreDirty);
            this.terrainSectors = sn.DoMultiArray(terrainSectors);
            this.phase = (Phase)sn.DoEnum(phase);
            this.listOfSectorsToAdd = sn.DoList(listOfSectorsToAdd);
            //   this.ComputeAll = sn.DoBool(ComputeAll);
            this.cycleSectorIndex = sn.DoInt32(cycleSectorIndex);
            this.IDName = sn.DoString(IDName);
            this.IsReady = sn.DoBool(IsReady);
            this.IsPaused = sn.DoBool(IsPaused);

            this.Children = sn.DoList(Children);
            this.transportsToInclude = sn.DoArray(transportsToInclude);
            this.cycleRegionMapTransportIndex = sn.DoInt32(cycleRegionMapTransportIndex);
            this.sectorsWereRemoved = sn.DoBool(sectorsWereRemoved);

           /* if (IDName.Contains("entity:human"))
            {
                this.updateInterval = GameData.Instance.AIConstants.PlayerMovementMapUpdateInterval;
            }
            else
            {
                this.updateInterval = GameData.Instance.AIConstants.OtherMovementMapUpdateInterval;
            }*/
            this.updateInterval = sn.DoDouble(updateInterval); // #MIGRATE


            snapshotMoveMapLayer = sn.DoDictionary(snapshotMoveMapLayer);
            snapshotClientLayers = sn.DoDictionary(snapshotClientLayers);

            //    this.subtileArrayIDs = sn.DoDictionary(subtileArrayIDs);
            //  this.snapshotRegionMaps = sn.DoDictionary(snapshotRegionMaps);


            //  sn.Ignore(RegionMap);
            sn.Ignore(totalComputationAllInstancesInSeconds);
            sn.Ignore(ComputationTimeSpentInSeconds);
            sn.Ignore(StartedOnTimeInSeconds);

            sn.Ignore(moveMapLayer);
            sn.Ignore(layers);
            // sn.Ignore(subtileValues);

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
            /*
            RegionMap.Clear();
            foreach (var item in snapshotRegionMaps)
            {
                RegionMap.Add(item.Key, (RegionMap)LookUp<ICyclable, CyclableID>.FindByID(snapshotRegionMaps[item.Key]));
                // RegionMap.Add(item.Key, Maps.RegionMap.AllRegionMaps[snapshotRegionMaps[item.Key]]);
            }*/

            moveMapLayer = new Dictionary<SurfaceType.TransportType, SubtileLayer>();
            foreach (var item in snapshotMoveMapLayer)
            {
                moveMapLayer.Add(item.Key, LookUp<SubtileLayer, SubtileLayerID>.FindByID(item.Value));
            }

            layers = new Dictionary<SurfaceType.TransportType, SubtileLayers>();
            foreach (var item in snapshotClientLayers)
            {
                layers.Add(item.Key, LookUp<SubtileLayers, SubtileLayersID>.FindByID(item.Value));
            }

            foreach (var item in Children)
            {
                item.LoadPostProcess(sn);
            }

            foreach (var sectors in terrainSectors)
            {
                foreach (var item in sectors.Value)
                {
                    foreach (var item2 in item)
                    {
                        item2.LoadPostProcess(sn);
                    }
                }
            }

            CreateRegulators();

        }

        #endregion

        public bool UnregisterBeforeSnapshot
        {
            get
            {
                return false;
            }
        }
    }
}
