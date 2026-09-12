using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.ClientSide.Interface;
using Microsoft.Xna.Framework;
using WindowSystem;
using UWGame.Client.Interface.MapGUI;
using UWGame.SimSide.AI;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.AI.Pathfinding;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Items;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Allegiances;
using System.Diagnostics;

namespace UWGame.SimSide.Maps 
{
    /// <summary>
    /// a selection of tiles and the code to render the selection status
    /// </summary>
    [DebuggerDisplay("{BoundingRectangle}")]
    public class MapArea: ISnapshot
    {
        /// <summary>
        /// made this private to monitor changes
        /// </summary>
        private List<TerrainTile> Coverage = new List<TerrainTile>();
        List<TerrainTileID> snapshotCoverage;

        public MapAreaRender MapAreaRender;

        /// <summary>
        /// saved, to compute connectivity via flood fill from this tile
        /// 
        /// used by AI also? then rename to non-client sounding name
        /// </summary>
        public Point? StartDragTile;

        /// <summary>
        /// 'Right' and 'Bottom' are outside the MapArea...
        /// </summary>
        public Rectangle? BoundingRectangle;

        /// <summary>
        /// sometimes null? causes crash...
        /// </summary>
        public TerrainTile BottomLeftTile;
        TerrainTileID snapshotBottomLeft;

        public TerrainTile UpperLeftTile;
        TerrainTileID snapshotUpperLeft;

        public TerrainTile BottomRightTile;
        TerrainTileID snapshotBottomRight;

        public TerrainTile UpperRightTile;
        TerrainTileID snapshotUpperRight;

        public Zone Zone;
        ZoneID? snapshotZone;

        public MapArea()
        {
            if (!Snapshotter.IsSnapshotting)
            {
                Initialize();
            }
        //    List<GUIRect> guiRects = Box.CreateBox(sourceRectangle, new Rectangle(0, 0, Width, Height), 10);

            //if ()
        }



        public List<TilePos> GetTileLocations()
        {
            List<TilePos> tileLocations = new List<TilePos>();
            foreach(var tile in Coverage)
            {
                tileLocations.Add(tile.TilePos);
            }
            return tileLocations;
        }

        public void Initialize()
        {
            MapAreaRender = new Client.Interface.MapGUI.MapAreaRender(this);
        }

        /// <summary>
        /// copy constructor called when a new Zone is created
        /// </summary>
        /// <param name="mapArea"></param>
        /// <param name="zone"></param>
        public MapArea(MapArea mapArea, Zone zone = null)
        {

            Coverage.AddRange(mapArea.Coverage);

            StartDragTile = mapArea.StartDragTile;
            
            Zone = zone;
            zone.MapArea = this;

            RecomputeBoundingRectangleAndEdges(false); // we cannot compute the edges before terrain tiles have ben assigned to the zone 

            Initialize();
        }

        public void Clear()
        {
            Coverage.Clear();

            MapAreaRender.SetIsDirty();
        }

        public void Add(TerrainTile tile)
        {
            Coverage.Add(tile);

            MapAreaRender.SetIsDirty();
        }

      
        /// <summary>
        /// how many tiles are in the area
        /// </summary>
        public int Count
        {
            get
            {
                return Coverage.Count;
            }
        }

        public Vector3? GetCenter()
        {
            if (BoundingRectangle.HasValue)
            {
                return MapManager.TileToWorldPos(BoundingRectangle.Value.Center);
            }

            return null;
        }

        /// <summary>
        /// Warning: this returns a rectangle where Bottom and Right are outside the covered area...
        /// </summary>
        /// <returns></returns>
        public Rectangle? GetBoundingBoxInTiles()
        {
            int minX, maxX, minY, maxY;

            if (Coverage.Count == 0)
                return null;

            minX = Coverage.Min(t => t.X);
            maxX = Coverage.Max(t => t.X);
            minY = Coverage.Min(t => t.Y);
            maxY = Coverage.Max(t => t.Y);

            return new Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1);

        }

        public delegate void IterateMethod(TerrainTile tile);

        public void IterateArea(IterateMethod iterateMethod)
        {
            foreach (var item in Coverage)
            {
                iterateMethod(item);
            }
        }

        public delegate bool IterateBooleanMethod(TerrainTile tile);

        public void IterateAreaBreakOnTrue(IterateBooleanMethod iterateMethod)
        {
            foreach (var item in Coverage)
            {
                if (iterateMethod(item))
                {
                    return;
                }
            }
        }

        public void HandleFirstTile(IterateMethod handleMethod)
        {
            handleMethod(Coverage[0]);
        }


        public void PostLoadContent()
        {
            MapAreaRender.PostLoadContent();

        }

        public TerrainTile GetFirst()
        {
            return Coverage[0];
        }

        public TerrainTile GetBottomLeft()
        {
            List<TerrainTile> leftTiles = Coverage.FindAll(tile => tile.X == BoundingRectangle.Value.Left);

            int maxY = leftTiles.Max(tile => tile.Y);

            return leftTiles.FirstOrDefault(tile => tile.Y == maxY);

        }

        public TerrainTile GetUpperLeft()
        {
            List<TerrainTile> leftTiles = Coverage.FindAll(tile => tile.X == BoundingRectangle.Value.Left);

            int maxY = leftTiles.Min(tile => tile.Y);

            return leftTiles.FirstOrDefault(tile => tile.Y == maxY);

        }

        public TerrainTile GetBottomRight()
        {
            List<TerrainTile> rightTiles = Coverage.FindAll(tile => tile.X == BoundingRectangle.Value.Right - 1);

            int maxY = rightTiles.Max(tile => tile.Y);

            return rightTiles.FirstOrDefault(tile => tile.Y == maxY);

        }

        public TerrainTile GetUpperRight()
        {
            List<TerrainTile> rightTiles = Coverage.FindAll(tile => tile.X == BoundingRectangle.Value.Right - 1);

            int maxY = rightTiles.Min(tile => tile.Y);

            return rightTiles.FirstOrDefault(tile => tile.Y == maxY);

        }


        /// <summary>
        /// should return the owner of the covered tiles - what about areas crossing owner boundaries..?
        /// </summary>
        /// <returns></returns>
        public EntityGroup GetOwner()
        {
            if (Zone != null)
            {
                EntityGroup owner = LookUp<EntityGroup, EntityGroupID>.FindByID(Zone.Owner);
                return owner;
            }
            else
            {
                // no zone has been defined - this means we are using SelectedTiles
                // SelectedTiles.Zone will always be null.

                // TODO when more expeditions - get closest to the area that we cover...
                EntityGroup expeditionOwner = The.Sim.PlaySite.GetFirstPlayerExpedition().OwnedEntities;

                return expeditionOwner;
            }

        }


        public void RecomputeBoundingRectangleAndEdges(bool computeEdges = true)
        {          
            
            if (Coverage.Count == 0)
                return; //?? prevents crash when zone is being deleted?

            int minX = Coverage.Min(t => t.X);
            int minY = Coverage.Min(t => t.Y);
            int maxX = Coverage.Max(t => t.X);
            int maxY = Coverage.Max(t => t.Y);

            //hmm, Right and Bottom are outside the area...
            BoundingRectangle = new Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1);

           // System.Diagnostics.Debug.Assert(BoundingRectangle.Value.Right < The.Map.mapTileWidth, "out of bounds?");

            BottomLeftTile = GetBottomLeft();
            UpperLeftTile = GetUpperLeft();
            BottomRightTile = GetBottomRight();
            UpperRightTile = GetUpperRight();

          
            if (!BoundingRectangle.Value.Contains(StartDragTile.Value))
            {
                // move start drag tile to closest tile within rectangle
                TilePos startPos = new TilePos(StartDragTile.Value);
                TerrainTile closest = Common.GetMinimum(Coverage, t => Common.DistanceOctile(t.TilePos, (startPos)));
                StartDragTile = new Point(closest.X, closest.Y);
            }


            if (computeEdges && Zone != null)
                Zone.ComputeEdges();
        }

        public TerrainTile GetCornerFromIndex(int index)
        {
            switch (index)
            {
                case 0:
                    {
                        return BottomLeftTile;
                    }
                case 1:
                    {
                        return BottomRightTile;
                    }
                case 2:
                    {
                        return UpperRightTile;
                    }
                case 3:
                    {
                        return UpperLeftTile;
                    }
                default:
                    {
                        return null;
                    }
            }
        }

        public bool CheckConnectivity(bool createList, ref List<TerrainTile> connectedTiles)
        {
            FloodFill floodFill = new FloodFill();

            SubtileLayers terrainCosts = The.Map.TerrainCosts[SurfaceType.TransportType.Foot];
            ushort[][] nodes = null;
            Common.InitJaggedArray(ref nodes, MapManager.SubtilesPerTileLength * BoundingRectangle.Value.Width, MapManager.SubtilesPerTileLength * BoundingRectangle.Value.Height);

            Point topLeftSubtile = MapManager.TileEdgeToSubtile(new Point(BoundingRectangle.Value.X, BoundingRectangle.Value.Y));
            Rectangle boundingRectangleSubtiles = new Rectangle(topLeftSubtile.X, topLeftSubtile.Y,
                MapManager.SubtilesPerTileLength * BoundingRectangle.Value.Width, MapManager.SubtilesPerTileLength * BoundingRectangle.Value.Height);

            
            floodFill.DoFloodFillOfArea(terrainCosts, nodes, MapManager.TileEdgeToSubtile(StartDragTile.Value), boundingRectangleSubtiles); // TileArea.Value);
            int width = Common.GetJaggedArrayWidth(nodes);
            int height = Common.GetJaggedArrayHeight(nodes);

          /*  ushort visited;
            ushort[] nodeColumn;

            int tileX, tileY;
            int subtileXCounter = 0, subtileYCounter = 0;

            //bool allSubtilesVisited;
            bool oneSubtileVisited;*/

            Point tilePos;

            Point subtileCorner;

            bool allTilesVisited = true;

            if (createList)
            {
                connectedTiles = new List<TerrainTile>();
            }


            // check for each tile that at least one subtile is visited:
            
            Point minTile = The.Map.ClampTileMapPosition(new Point(BoundingRectangle.Value.Left, BoundingRectangle.Value.Top));
            Point maxTile = The.Map.ClampTileMapPosition(new Point(BoundingRectangle.Value.Right, BoundingRectangle.Value.Bottom));

            
            //for (int x = 0; x < TileArea.Value.Width; x++)
            for (int x = minTile.X; x < maxTile.X; x++) // test these boundaries!!!
            {
                //for (int y = 0; y < TileArea.Value.Height; y++)
                for (int y = minTile.Y; y < maxTile.Y; y++)
                {
                    //allSubtilesVisited = true;
                    //oneSubtileVisited = false;

                    tilePos = new Point(x, y);
                    
                    subtileCorner = MapManager.TileEdgeToSubtile(x - BoundingRectangle.Value.Left, y - BoundingRectangle.Value.Top); // tilePos,

                    if (IsTileVisited(nodes, subtileCorner))
                    {
                        if (createList)
                        {
                            connectedTiles.Add(The.Map.GetTile(tilePos));
                        }
                    }
                    else
                    {
                        if (!The.Map.TileIsCompletelyBlocked(terrainCosts, tilePos))
                        {
                            allTilesVisited = false;

                            if (!createList)
                            {
                                return false;
                            }
                        }
                    }


                }
            }

            return allTilesVisited;
           
        }

        public void CropTilesToConnectedArea()
        {
            List<TerrainTile> newList = null;
            The.InGameUI.SelectedTiles.CheckConnectivity(true, ref newList);

            Clear();
            foreach (var tile in newList)
            {
                Add(tile);
            }

            RecomputeBoundingRectangleAndEdges();

            // delete harvest jobs ouside the new area:
            if (Zone != null && Zone.HarvestJobs != null)
            {
                foreach (var item in Zone.HarvestJobs)
                {
                    ProcessJob job;
                    Point jobPosition;
                    for (int i = item.Value.Count - 1; i >= 0; i--)
                    {
                        job = item.Value[i];
                        jobPosition = job.HarvestJob.Item.Container.MapPosition;
                        if (!Coverage.Exists(t => t.X == jobPosition.X && t.Y == jobPosition.Y))
                        {
                            job.Destroy(true);
                        }
                    }
                }
            }
        }

        public delegate bool CycleEntityPredicate(IKnownEntityData entityData);

        public EntityID? CycleKnownEntities(EntityID? previousID, SharedKnowledge sharedKnowledge, CycleEntityPredicate predicate, out bool foundEntities,bool returnFirstMatch = false)
        {
            List<EntityID> listOfPersons = new List<EntityID>();
            foundEntities = false;
           
            if (previousID == null)
            {
                returnFirstMatch = true;
            }

            IKnownEntityData entityData;
            foreach (var tile in Coverage)
            {
                if (tile.EntitiesOnTile != null)
                {
                    foreach (var entity in tile.EntitiesOnTile)
                    {
                        if (predicate(entity))
                        {
                            //This check was done in 
                            //else if (returnFirstMatch == true)
                            //before but this resulted in some problems due to foundEntities got set to true but it did not actually return an entity
                            //Due to this requirement being checked to late.
                            if (sharedKnowledge.GetKnownData(entity.EntityID, out entityData) == EntityResult.SeenDirectly) //Do not consider this entity if we can not see it directly.
                            {
                                foundEntities = true;
                                if (entity.EntityID == previousID)
                                {
                                    returnFirstMatch = true;
                                }
                                else if (returnFirstMatch == true)
                                {
                                    return entity.EntityID;
                                }
                            }
                        }
                    }

                }

                if (tile.RememberedRootEntitiesOnTile != null)
                {
                    List<MemoryFact> list;

                    if (tile.RememberedRootEntitiesOnTile.TryGetValue(sharedKnowledge, out list))
                    {
                        foreach (var memoryFact in list)
                        {
                            if (predicate(memoryFact))
                            {
                                foundEntities = true;
                                if (returnFirstMatch == true)
                                {
                                    return memoryFact.EntityID;
                                }
                                else if (memoryFact.EntityID == previousID)
                                {
                                    returnFirstMatch = true;
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }

        private static bool IsTileVisited(ushort[][] nodes, Point subtileCorner)
        {
            ushort visited;
            for (int sx = subtileCorner.X; sx < subtileCorner.X + MapManager.SubtilesPerTileLength; sx++)
            {
                for (int sy = subtileCorner.Y; sy < subtileCorner.Y + MapManager.SubtilesPerTileLength; sy++)
                {
                    visited = nodes[sx][sy];

                    if (visited == 1) // && !(sx == 1 && sy == 1)) // check that an edge subtile is visited?
                    {
                        return true;
                        //oneSubtileVisited = true;

                        //allSubtilesVisited = false;

                    }

                }

            }

            return false;
            //return visited;
        }

        public Vector3? SelectBestGroundLocationForStorage(IKnownEntityData item, Dictionary<Point, Tuple<List<Tuple<IKnownEntityData, Point>>, float[][]>> allTileData)
        {
            Point tilePos;
            Vector3? result;
            Tuple<List<Tuple<IKnownEntityData, Point>>, float[][]> tileData;
            foreach (var tile in Coverage)
            {
                tilePos = new Point(tile.X, tile.Y);

                tileData = HaulingJobManager.GetTileGroundStorageData(allTileData, tilePos);

                result = HaulingJobManager.FindSimilarItemToStackWithInTile(item, tileData, tile);

                if (result.HasValue)
                {
                    return result.Value;
                }
            }

            // no similar items were found. find an empty spot:
            foreach (var tile in Coverage)
            {
                tilePos = new Point(tile.X, tile.Y);

                tileData = HaulingJobManager.GetTileGroundStorageData(allTileData, tilePos);

                // leave space for more items of the same type to stack next to it:
                result = HaulingJobManager.FindEmptySubtileForStorage(tilePos, item, tileData, true);
                if (result.HasValue)
                {
                    return result.Value;
                }
            }

            foreach (var tile in Coverage)
            {
                tilePos = new Point(tile.X, tile.Y);

                tileData = HaulingJobManager.GetTileGroundStorageData(allTileData, tilePos);

                // else choose the first subtile that has room:
                result = HaulingJobManager.FindFirstSubtileWithRoomForStorage(item, tilePos, tileData);
                if (result.HasValue)
                {
                    return result.Value;
                }
            }

            // no free spot found...
            return null;

            // else dump it in the center of the first tile...
          /*  tilePos = new Point(0, 0);
            tileData = HaulingJobManager.GetTileGroundStorageData(allTileData, tilePos);

            Point relativeSubtilePos = new Point(1, 1);
            float totalBulkOnSubtile = tileData.Item2[relativeSubtilePos.X][relativeSubtilePos.Y];

            tileData.Item2[relativeSubtilePos.X][relativeSubtilePos.Y] = totalBulkOnSubtile + item.Bulk;
            tileData.Item1.Add(new Tuple<IKnownEntityData, Point>(item, relativeSubtilePos));

            result = MapManager.SubtileAndTilePosToWorldPos(relativeSubtilePos, tilePos);
            return result.Value;*/

        }
         

        public void GetNoOfItemsInAreaByType(Dictionary<EntityType, List<EntityID>> allItems, Predicate<IKnownEntityData> entitiesToCount, Allegiance allegianceViewpoint) 
        {            
            IterateArea(tile =>
            {
                if (tile.AllegiancesThatSeeThisTile.Contains(allegianceViewpoint))
                {
                    if (tile.EntitiesOnTile != null)
                    {
                        foreach (var entity in tile.EntitiesOnTile)
                        {
                           allItems = AddToAllItems(allItems, entity.EntityType,entity.EntityID);
                        }                  
                    }
                }
                else
                {
                    if (tile.RememberedRootEntitiesOnTile != null)
                    {
                        List<MemoryFact> rememberedEntities;
                        if (tile.RememberedRootEntitiesOnTile.TryGetValue(allegianceViewpoint.SharedKnowledge, out rememberedEntities))
                        {
                            if (rememberedEntities != null)
                            {
                                foreach (var memoryFact in rememberedEntities)
                                {
                                    allItems = AddToAllItems(allItems, memoryFact.EntityType,memoryFact.EntityID);
                                }
                            }
                        }
                    }
                }
            });
        }

        private static Dictionary<EntityType, List<EntityID>> AddToAllItems(Dictionary<EntityType, List<EntityID>> allItems, EntityType entityType, EntityID entityID)
        {
            if (entityType.ItemType != null)
            {
                List<EntityID> listOfIDs;
                if (!allItems.TryGetValue(entityType, out listOfIDs))
                {
                    allItems.Add(entityType, listOfIDs);

                    allItems.TryGetValue(entityType, out listOfIDs);
                }
                if (listOfIDs != null)
                {
                    if (!listOfIDs.Contains(entityID))
                    {
                        listOfIDs.Add(entityID);
                        allItems[entityType] = listOfIDs;
                    }
                }
                else
                {
                    listOfIDs = new List<EntityID>();
                    listOfIDs.Add(entityID);
                    allItems[entityType] = listOfIDs;
                }
            }
            return allItems;
        }     

     
        public void GetNoOfEntitiesInArea(Dictionary<EntityType, int> allItems, Predicate<IKnownEntityData> entitiesToCount, Allegiance allegianceViewpoint) 
        {
            //Predicate<EntityType> entityTypesToCount = (e => e.ItemType != null);

            IterateArea(tile =>
            {
                if (The.Sim.Mode == Sim.EngineMode.Edit
                    || tile.AllegiancesThatSeeThisTile.Contains(allegianceViewpoint))
                {
                    if (tile.EntitiesOnTile != null)
                    {
                        foreach (var entity in tile.EntitiesOnTile)
                        {
                            CountEntity(allItems, entity, entitiesToCount);
                        }                        
                    }
                }
                else
                {
                    if (tile.RememberedRootEntitiesOnTile != null)
                    {
                        List<MemoryFact> rememberedEntities;
                        if (tile.RememberedRootEntitiesOnTile.TryGetValue(allegianceViewpoint.SharedKnowledge, out rememberedEntities))
                        {
                            if (rememberedEntities != null)
                            {
                                foreach (var memoryFact in rememberedEntities)
                                {
                                    //We will not count parts or contained items in the zone...
                                    if (memoryFact.ContainedBy.HasValue == false && memoryFact.PartOfID == null)
                                    {
                                        CountEntity(allItems, memoryFact, entitiesToCount);
                                    }
                                }
                            }

                        }
                    }
                }
            });
        }

        public static void CountEntity(Dictionary<EntityType, int> allItems, IKnownEntityData entity, Predicate<IKnownEntityData> entitiesToCount)
        {
            int currentSum;
            if (entitiesToCount(entity)) // entityType.ItemType != null)
            {
                if (allItems.TryGetValue(entity.EntityType, out currentSum))
                {
                    currentSum++;
                }
                else
                {
                    currentSum = 1;
                }

                allItems[entity.EntityType] = currentSum;
            }

        }


        public void GetSumOfAllResourcesInArea(SharedKnowledge sharedKnowledge, Dictionary<ResourceType, ResourcesAndJobs> sum) // Dictionary<ResourceType, Tuple<int, int>> sum)
        {
            // only discovered containers
            // TODO later: use memory facts
            // TODO: only resources with process defined (for humans???)
            IterateArea(tile =>
            {
                if (tile.TreesOnTile != null)
                {
                    UWGame.SimSide.Trees.Tree treeComponent;
                    foreach (Entity tree in tile.TreesOnTile)
                    {
                        tree.Find(out treeComponent);
                        if (treeComponent.Crops != null)
                        {
                            
                            foreach (KeyValuePair<ResourceType, Crop> kvp in treeComponent.Crops)
                            {
                                if (JobManager.AllowGatherJobForResource(sharedKnowledge, kvp.Value))  //sharedKnowledge.AllDetectedEntities.Contains(kvp.Value.DetectableID))
                                {
                                    AddToResourceSum(kvp.Value, sum);
                                }
                                
                            }
                        }
                    }
                }

                // designer placed... for now
                if (tile.TileResources != null)
                {
                    foreach (var container in tile.TileResources)
                    {
                        if (JobManager.AllowGatherJobForResource(sharedKnowledge, container.Value))  //if (sharedKnowledge.AllDetectedEntities.Contains(container.Value.DetectableID))
                        {
                            AddToResourceSum(container.Value, sum);
                        }
                    }
                }
            });

        }

        private void AddToResourceSum(ResourceContainer container, Dictionary<ResourceType, ResourcesAndJobs> sum) // Dictionary<ResourceType, int> sum) //Dictionary<ResourceType, Tuple<int, int>> sum)
        {
            int oldSum; // = null;
            bool maximumReached = false;

            //  Tuple<int, int> tuple;
            ResourcesAndJobs oldValue;
            if (sum.TryGetValue(container.ResourceType, out oldValue)) //sum.TryGetValue(container.ResourceType, out tuple))
            {
                //oldSum = tuple.Item1;
                oldValue.NumberOfResources = oldValue.NumberOfResources + container.ResourceItems.Count;

                oldValue.MaximumRegrowth = oldValue.MaximumRegrowth + container.GetMaximumRegrowth();
                oldValue.CurrentRegrowth = oldValue.CurrentRegrowth + container.GetCurrentRegrowth(out maximumReached);

                if (oldValue.MaximumReached == true)
                {
                    if (maximumReached == false)
                    {
                        oldValue.MaximumReached = false;
                    }
                }
               /* else
                {
                    if (maximumReached == true)
                    {
                        oldValue.MaximumReached = true;
                    }
                }*/

                sum[container.ResourceType] = oldValue;
                //sum[container.ResourceType] = oldSum + container.ResourceItems.Count;
            }
            else
            {
                /* if (container.ResourceItems.Count > 0)
                 {*/
                ResourcesAndJobs entry = new ResourcesAndJobs()
                {
                    NumberOfResources = container.ResourceItems.Count,
                    NumberOfJobsInZone = GetNoOfJobs(container.ResourceType),
                    NumberOfJobsInOtherZones = 0,

                    MaximumRegrowth = container.GetMaximumRegrowth(),
                    UserChangedData = false
                };

              //  bool maximumReached;
                entry.CurrentRegrowth = container.GetCurrentRegrowth(out maximumReached);
                entry.MaximumReached = maximumReached;

                sum.Add(container.ResourceType, entry);

            }
        }

        public int GetNoOfJobs(ResourceType resourceType)
        {
            if (Zone != null)
            {
                List<ProcessJob> list;
                if (Zone.HarvestJobs.TryGetValue(resourceType, out list))
                {
                    return list.Count;
                }
            }

            return 0;
        }

        /// <summary>
        /// count the existing jobs and save them in the data structure
        /// </summary>
        /// <param name="mapArea"></param>
        /// <param name="data"></param>
        public void GetSumOfAllHarvestJobsInArea(Dictionary<ResourceType, ResourcesAndJobs> data) //, Dictionary<ResourceType), List<ProcessJob>> allJobs = null) // Dictionary<ResourceType, int> sum)
        {
            EntityGroup expeditionOwner = The.Sim.PlaySite.GetFirstPlayerExpedition().OwnedEntities;

            ResourcesAndJobs resourcesAndJobs;

            IterateArea(tile =>
            {
                if (tile.HarvestJobs != null)
                {
                    //List<HarvestJob> jobs;
                    Dictionary<ResourceType, List<ProcessJob>> jobs;
                    if (tile.HarvestJobs.TryGetValue(expeditionOwner.ID, out jobs))
                    {
                        foreach (var jobsOfType in jobs)
                        {
                            // int oldSum; // = null;

                            if (jobsOfType.Value.Count > 0)
                            {
                                int jobsInOtherZones;
                                jobsInOtherZones = jobsOfType.Value.Count(j => (Zone == null) || j.HarvestJob.Zone != Zone);
                                // could this number sometimes be higher than the number of resources???

                                if (data.TryGetValue(jobsOfType.Key, out resourcesAndJobs)) // out oldSum))
                                {
                                    // add to the sum:
                                    resourcesAndJobs.NumberOfJobsInOtherZones = resourcesAndJobs.NumberOfJobsInOtherZones + jobsInOtherZones;

                                    //  resourcesAndJobs.NumberOfJobsInZone = resourcesAndJobs.NumberOfJobsInZone + ; // .Count;
                                    data[jobsOfType.Key] = resourcesAndJobs;


                                }
                                else
                                {
                                    //create new entry:

                                    data.Add(jobsOfType.Key, new ResourcesAndJobs()
                                    {
                                        NumberOfResources = 0,
                                        NumberOfJobsInOtherZones = jobsInOtherZones,
                                        NumberOfJobsInZone = GetNoOfJobs(jobsOfType.Key), // jobsInZone, //jobsOfType.Value.Count, 
                                        UserChangedData = false
                                    });
                                }

                                /*
                                if (allJobs != null)
                                {
                                    List<ProcessJob> alreadyAddedJobs;
                                    // also collect the jobs while we're here:
                                    if (!allJobs.TryGetValue(jobsOfType.Key, out alreadyAddedJobs))
                                    {
                                        alreadyAddedJobs = new List<ProcessJob>();
                                        alreadyAddedJobs.AddRange(jobsOfType.Value);
                                        allJobs.Add(jobsOfType.Key, alreadyAddedJobs); //alreadyAddedJobs);
                                    }
                                    else
                                    {
                                        alreadyAddedJobs.AddRange(jobsOfType.Value);
                                    }


                                }*/
                            }


                        }

                    }
                }

            });

            /*  foreach (var tile in mapArea.Coverage)
              {
                  if (tile.HarvestJobs != null)
                  {
                      //List<HarvestJob> jobs;
                      Dictionary<ResourceType, List<ProcessJob>> jobs;
                      if (tile.HarvestJobs.TryGetValue(expeditionOwner, out jobs))
                      {
                          foreach (var jobsOfType in jobs)
                          {
                             // int oldSum; // = null;

                              if (jobsOfType.Value.Count > 0)
                              {
                                  if (data.TryGetValue(jobsOfType.Key, out resourcesAndJobs)) // out oldSum))
                                  {
                                      // add to the sum:
                                      resourcesAndJobs.NumberOfJobs = resourcesAndJobs.NumberOfJobs + jobsOfType.Value.Count;
                                      data[jobsOfType.Key] = resourcesAndJobs;

                                    
                                  }
                                  else
                                  {                                    
                                      //create new entry:
                                      data.Add(jobsOfType.Key, new ResourcesAndJobs() { NumberOfResources = 0, NumberOfJobs = jobsOfType.Value.Count, UserChangedData = false });
                                      //sum.Add(job.ResourceType, 1);
                                  }

                                  if (allJobs != null)
                                  {
                                      List<ProcessJob> alreadyAddedJobs;
                                      // also collect the jobs while we're here:
                                      if (!allJobs.TryGetValue(jobsOfType.Key, out alreadyAddedJobs))
                                      {
                                          alreadyAddedJobs = new List<ProcessJob>();
                                          alreadyAddedJobs.AddRange(jobsOfType.Value);
                                          allJobs.Add(jobsOfType.Key, alreadyAddedJobs); //alreadyAddedJobs);
                                      }
                                      else
                                      {
                                          alreadyAddedJobs.AddRange(jobsOfType.Value);
                                      }

                                    
                                  }
                              } 
                                            
                          }

                      }
                  }

              }*/


        }


        private void GetAllHarvestJobsInArea(Dictionary<ResourceType, List<ProcessJob>> allJobs) // Dictionary<ResourceType, int> sum)
        {
            EntityGroup expeditionOwner = The.Sim.PlaySite.GetFirstPlayerExpedition().OwnedEntities;

            IterateArea(tile =>
            {
                if (tile.HarvestJobs != null)
                {
                    //List<HarvestJob> jobs;
                    Dictionary<ResourceType, List<ProcessJob>> jobs;
                    if (tile.HarvestJobs.TryGetValue(expeditionOwner.ID, out jobs))
                    {

                        foreach (var jobsOfType in jobs)
                        {

                            if (jobsOfType.Value.Count > 0)
                            {
                                // int oldSum; // = null;
                                allJobs.Add(jobsOfType.Key, jobsOfType.Value);
                            }
                        }

                    }
                }

            });
        }

        /// <summary>
        /// can return the list as either entities or entity ids
        /// TODO: replace this method with calls to quad trees
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="entityIDs"></param>
        /// <param name="entitiesToInclude"></param>
        public void GetEntitiesInArea(List<IKnownEntityData> entities, List<EntityID> entityIDs, Predicate<IKnownEntityData> entitiesToInclude, Allegiance allegianceViewpoint) // Dictionary<ResourceType, ResourcesAndJobs> sum) // Dictionary<ResourceType, Tuple<int, int>> sum)
        {
            //Predicate<EntityType> entityTypesToCount = (e => e.ItemType != null);

            IKnownEntityData entityData = null;

            IterateArea(tile =>
            {
                if (tile.AllegiancesThatSeeThisTile.Contains(allegianceViewpoint)) //The.InGameUI.UIAllegiance))
                {
                    if (tile.EntitiesOnTile != null)
                    {
                        foreach (var entityOrContainer in tile.EntitiesOnTile)
                        {             
                            //-----------------------------
                         /*   if (entityOrContainer.ContainedEntities != null && entityOrContainer.ContainedEntities.Count > 0)
                            {
                                foreach (var entity in entityOrContainer.ContainedEntities)
                                {
                                    IKnownEntityData iKnowEntity = Entity.FindByID(entity);

                                    if (entitiesToInclude(iKnowEntity)
                                    && !GoalEvaluator.EntityDataResultCausesSkip(allegianceViewpoint.SharedKnowledge.GetKnownData(iKnowEntity.EntityID, out entityData))) // sharedKnowledge.AllDetectedEntities.ContainsKey(entity)) //!GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(entity.EntityID, out entityData)))
                                    {
                                        if (entities != null)
                                        {
                                            entities.Add(iKnowEntity);
                                        }

                                        if (entityIDs != null)
                                        {
                                            entityIDs.Add(iKnowEntity.EntityID);
                                        }
                                    }
                                }
                            }      */                     
                            //----------------------------------------------
                            if (entitiesToInclude(entityOrContainer)
                                && !GoalEvaluator.EntityDataResultCausesSkip(allegianceViewpoint.SharedKnowledge.GetKnownData(entityOrContainer.EntityID, out entityData))) // sharedKnowledge.AllDetectedEntities.ContainsKey(entity)) //!GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(entity.EntityID, out entityData)))
                            {
                                if (entities != null)
                                {
                                    entities.Add(entityOrContainer);
                                }

                                if (entityIDs != null)
                                {
                                    entityIDs.Add(entityOrContainer.EntityID);
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (tile.RememberedRootEntitiesOnTile != null)
                    {
                        List<MemoryFact> rememberedEntities;

                        if (tile.RememberedRootEntitiesOnTile.TryGetValue(allegianceViewpoint.SharedKnowledge, out rememberedEntities))
                        {
                            if (rememberedEntities != null)
                            {
                                foreach (var memoryFact in rememberedEntities)
                                {
                                    if (entitiesToInclude(memoryFact))
                                    {
                                        if (entities != null)
                                        {
                                            entities.Add(memoryFact);
                                        }

                                        if (entityIDs != null)
                                        {
                                            entityIDs.Add(memoryFact.EntityID);
                                        }
                                    }
                                }
                            }

                        }
                    }
                }
            });

            // remove duplicates:
            if (entities != null)
            {
                entities = entities.Distinct().ToList();
            }

            if (entityIDs != null)
            {
                entityIDs = entityIDs.Distinct().ToList();
            }
        }


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
            this.BoundingRectangle = sn.DoRectangleNullable(BoundingRectangle);
            this.snapshotBottomLeft = (TerrainTileID)sn.SnapshotID<TerrainTile, TerrainTileID>(BottomLeftTile);
            this.snapshotBottomRight = (TerrainTileID)sn.SnapshotID<TerrainTile, TerrainTileID>(BottomRightTile);
            this.snapshotUpperLeft = (TerrainTileID)sn.SnapshotID<TerrainTile, TerrainTileID>(UpperLeftTile);
            this.snapshotUpperRight = (TerrainTileID)sn.SnapshotID<TerrainTile, TerrainTileID>(UpperRightTile);
            this.StartDragTile = sn.DoPointNullable(StartDragTile);
            this.snapshotZone = sn.SnapshotID<Zone, ZoneID>(Zone);

            if (sn.mode != Snapshotter.Mode.Load)
            {
                this.snapshotCoverage = Coverage.Select(t => t.ID).ToList();
            }
            this.snapshotCoverage = sn.DoList(snapshotCoverage);


            sn.Ignore(BottomLeftTile);
            sn.Ignore(BottomRightTile);
            sn.Ignore(UpperLeftTile);
            sn.Ignore(UpperRightTile);
            sn.Ignore(Coverage);
            sn.Ignore(Zone);
            sn.Ignore(MapAreaRender);

            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            BottomLeftTile = LookUpSortedDictionary<TerrainTile, TerrainTileID>.FindByID(snapshotBottomLeft);
            BottomRightTile = LookUpSortedDictionary<TerrainTile, TerrainTileID>.FindByID(snapshotBottomRight);
            UpperLeftTile = LookUpSortedDictionary<TerrainTile, TerrainTileID>.FindByID(snapshotUpperLeft);
            UpperRightTile = LookUpSortedDictionary<TerrainTile, TerrainTileID>.FindByID(snapshotUpperRight);

            if (snapshotZone.HasValue)
            {
                Zone = (Zone)LookUp<Zone, ZoneID>.FindByID(snapshotZone.Value);
            }

            Coverage = snapshotCoverage.Select(t => LookUpSortedDictionary<TerrainTile, TerrainTileID>.FindByID(t)).ToList();

            Initialize();
        }

        #endregion

        public struct ResourcesAndJobs
        {
            public int NumberOfResources;
            public int NumberOfJobsInZone;          

            public int NumberOfJobsInOtherZones;

            public float CurrentRegrowth;
            public float MaximumRegrowth;
            public bool MaximumReached;

            public bool UserChangedData;
        }

        /// <summary>
        /// ??
        /// </summary>
        public struct FindPreyJobs
        {
            public int NumberOfJobsInZone;


            public bool UserChangedData;
        }
    }
}
