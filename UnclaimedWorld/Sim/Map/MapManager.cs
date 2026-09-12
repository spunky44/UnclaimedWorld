#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
////using Microsoft.Xna.Framework.Storage;
using System.Xml;
using UWGame.ClientSide.Interface;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Trees;
using SpriteSheetRuntime;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Vegetation;
using System.IO;
using System.Xml.Serialization;
using UWGame.SimSide.Vehicles;
using UWGame.SimSide.Soil;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using WindowSystem;
using GameStateManagement;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.AI;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Items;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Maps.MapEditor;
using System.Threading.Tasks;
using UWGame.SimSide.Processes;
using UWGame.ClientSide.Interface.HUD_Windows;
using UWGame.SimSide.AllGameData;
using UWGame;
using UWGame.SimSide.Overland;
using UWGame.SimSide.Collisions;
using UWGame.SimSide.Systems;
using System.Diagnostics;
using UWGame.Control;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems.TimeSlicing;
using UWGame.SimSide.Maps.Regions;
#endregion

namespace UWGame.SimSide.Maps 
{
   

    public class MapManager: ISnapshot
    {
        private Vector2[] eightDirsAsVectors = new Vector2[8];
        private Vector2[] normalizedEightDirsAsVectors = new Vector2[8];
        private float abDiagonalLength;
        private float abCartesianLength;

                    //public GameWorldRenderer Renderer;
        public const string MapsFolder = "Maps/";

       
        /// <summary>
        /// This bit field packs cost together with other flags into a single byte.
        /// perhaps later we will extend this to a short or int to have more flags, like for surface types (water, ford) etc.
        /// or use the BitMask64 class
        /// </summary>
        [Flags]
        public enum SubtileValue : byte
        {
            Blocked = 0,
            Cost = 1 | 2 | 4 | 8 | 16, // the maximum terrain cost is 31 - this must also have room for threat/discomfort
            Pad = 32,
            Reserved = 64
        }

        public const int MaxTerrainCost = (int)SubtileValue.Cost; // 31;

        /// <summary>
        /// This is the terrain costs map, with values for each subtile. There is an array of values for each transport type.
        /// Contains Pad and Reserved flags as well.
        /// 
        /// The values consist of a bit pattern and their numeric values are meaningless. Parse with GetCost or TestFlagIsSet!
        /// 
        /// The maximum terrain cost is 31, with some flags we fill a whole byte.
        /// 
        /// this is Layers so it can be used in place of MoveMap.
        /// </summary> 
        public Dictionary<SurfaceType.TransportType, SubtileLayers> TerrainCosts;
        public Dictionary<SurfaceType.TransportType, SubtileLayersID> snapshotTerrainCosts;

    //    public Dictionary<SurfaceType.TransportType, SubtileValue[][]> TerrainCosts;

        /// <summary>
        /// only used for snapshotting references to the above. 
        /// </summary>
      //  private Dictionary<SurfaceType.TransportType, SubtileLayerID> TerrainCostIDs;



        /// <summary>
        /// #SECTORS: moved to SubtileLayers
        /// 
        /// For general terrain inquiries, this is the high level representation of the base (foot) terrain map (TerrainCosts).
        /// 
        /// 
        /// </summary>
     /*   public RegionMap TerrainRegionMap;
        CyclableID snapshotTerrainRegionMap;
        */

        public const float DiagonalFactor = 1.41f;

        /// <summary>
        /// really needed???
        /// </summary>
        public List<string> AllMaps;

       
        private Queue<NoParkingSpotTimeStamp> NoParkingSpotsTimeStamps = new Queue<NoParkingSpotTimeStamp>();
        private Dictionary<NoParkingSpot, NoParkingSpot> NoParkingSpots = new Dictionary<NoParkingSpot, NoParkingSpot>();
        private Regulator noParkingSpotsRegulator;
                     


        /// <summary>
        /// When the Camera angle becomes closer to the ground, the terrain must be moved further 'inward' (z+) to prevent overlapping with the models.
        /// </summary>
        public const float TerrainZLevel = 1400f; //1000f;



        public const int SectorSizeInTiles = 16; // 26;

        /// <summary>
        /// 
        /// </summary>
        public const int SectorSizeInSubtiles = MapManager.SubtilesPerTileLength * SectorSizeInTiles; // 3 * 16 tiles


        // The size of an individual tile in pixels
        public const int tileSize = 48;
        public const int tileSizeOver2 = 24;
        public const float oneOverTileSize = 0.020833334f; // 1f/48f	

        public const int SubtilesPerTileLength = 3;

        /// <summary>
        /// tile size / 3
        /// </summary>
        public const int subTileSize = 16;
        public const int subTileSizeOver2 = 8;
        public const float oneOverSubtileSize = 0.0625f;

        public const int tileSizeOver4 = 12;
        public const int tileWidthSquared = 2304;

        /// <summary>
        /// need to subtract this to stay within tile/subtile right edge...
        /// </summary>
        const float locationEpsilon = 0.01f;

        public Vector3 MaxWorldPos;

       
        /* 7 0 4
         * 3 X 1
         * 6 2 5
         * */
        public static sbyte[,] direction = new sbyte[8, 2] { { 0, -1 }, { 1, 0 }, { 0, 1 }, { -1, 0 }, { 1, -1 }, { 1, 1 }, { -1, 1 }, { -1, -1 } };
        public static int[] oppositeDirection = new int[8] { 2, 3, 0, 1, 6, 7, 4, 5 };


        /*    Common.Direction[] edgeIndexToDirection = new Common.Direction[8]{Common.Direction.North, Common.Direction.East, Common.Direction.South, Common.Direction.West, 
                                                                            Common.Direction.NorthEast, Common.Direction.SouthEast, Common.Direction.SouthWest, Common.Direction.NorthWest};
            int[] directionToEdgeIndex = new int[8]{Common.Direction.North, Common.Direction.East, Common.Direction.South, Common.Direction.West, 
                                                                            Common.Direction.NorthEast, Common.Direction.SouthEast, Common.Direction.SouthWest, Common.Direction.NorthWest};
    */
        /// <summary>
        /// no of tile/subtile sectors across the width. the last sector may only be partially filled...
        /// </summary>
        public int NoOfSectorsAcrossWidth;
        public int NoOfSectorsAcrossHeight;

        /// <summary>
        /// no of tile sectors across the width.
        /// </summary>
      /*  public int MapTileSectorWidth;
        public int MapTileSectorHeight;*/

        /// <summary>
        /// Width in tiles
        /// </summary>
        public int mapTileWidth;

        /// <summary>
        /// Height in tiles
        /// </summary>
        public int mapTileHeight;

        public int mapSubtileWidth;
        public int mapSubtileHeight;

        public float MapWorldWidth;
        public float MapWorldHeight;

        /// <summary>
        /// clamp the entity positions to these values:
        /// </summary>
      //  public float MaxWorldPosX;
      //  public float MaxWorldPosY;

               
       
        public TerrainTile[][] TileMap;
        TerrainTileID[][] snapshotTileMap;

       
        //   public MovementMap ColonistMoveSafelyMap;
        //  public MovementMap IndigenousHerbivoreMoveSafelyMap;

        public System.Reflection.MethodInfo InfluenceMapTileIsFreeInfo;

        public System.Reflection.MethodInfo InfluenceMapTileIsComfortableInfo;

        /// <summary>
        /// this list is used to propagate terrain changes efficiently to the various influence-mapped movement maps.
        /// </summary>
    //    public List<MovementMap> MovementMaps = new List<MovementMap>();

      
        public static List<SurfaceType.TransportType> MapTransportTypeArray;

       

        //public enum Direction { North = 0, East = 1, South = 2, West = 3, NorthEast = 4, SouthEast = 5, SouthWest = 6, NorthWest = 7 }
        private static int[] subtileIndexToDirectionMappings = new int[9] { 7, 0, 4, 3, -1, 1, 6, 2, 5 };
      

        /// <summary>
        /// this dict is used for iterating and quickly propagating changes to all movement maps
        /// </summary>
        public List<MovementMap> AllMovementMaps = new List<MovementMap>();
        private List<CyclableID> snapshotAllMovementMaps = new List<CyclableID>();
     //   public Dictionary<CyclableID, MovementMap> AllMovementMaps = new Dictionary<CyclableID, MovementMap>();
        

        
        //  public RegionMapManager RegionMapManager;

     //   public MapLoader MapLoader; demoted to local variable

        public MapManager()
        {           

            MapTransportTypeArray = new List<SurfaceType.TransportType>(); //(TerrainType.TransportType[])Enum.GetValues(typeof(TerrainType.TransportType));
            foreach (var item in Enum.GetValues(typeof(SurfaceType.TransportType)))
            {
                if ((SurfaceType.TransportType)item != SurfaceType.TransportType.Air)
                {
                    MapTransportTypeArray.Add((SurfaceType.TransportType)item);
                }
            }


            float halfTileWidth = tileSize / 2f;
            float halfTileHeight = tileSize / 2f;
            for (int i = 0; i < 8; i++)
            {   // compute the vectors we need for detecting movement along an 8-dir:
                eightDirsAsVectors[i] = new Vector2(direction[i, 0] * halfTileWidth, direction[i, 1] * halfTileHeight);

                // save a unit vector as well.
                normalizedEightDirsAsVectors[i] = eightDirsAsVectors[i];
                normalizedEightDirsAsVectors[i].Normalize();
            }
            abDiagonalLength = (new Vector2(halfTileWidth, halfTileHeight)).Length();
            abCartesianLength = halfTileHeight;

           // BlockedPaths = new Dictionary<MovementMap, Queue<BlockedPath>>();
           // BlockedPathMaps = new Dictionary<MovementMap, Dictionary<FromTo, FromTo>>();

            //RegionMapManager = new RegionMapManager();

            if (!Snapshotter.IsSnapshotting) 
            {
                InitDataNotSnapshotted();
            }
        }


        void InitDataNotSnapshotted()
        {
            noParkingSpotsRegulator = new Regulator(The.Sim.GameplayRandomGenerator, 1, "MapManagerNoParkingSpots");

            // Reflection is used here...
            // test the next node for passability:
            // used in call back from Dijkstra:
            // FOUND IN InfluenceMap!!!!
            InfluenceMapTileIsFreeInfo = typeof(InfluenceMap).GetMethod("DiscomfortTileIsFree",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

            InfluenceMapTileIsComfortableInfo = typeof(InfluenceMap).GetMethod("DiscomfortTileIsBelowValue",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                
        }

        public void LoadContent()
        {
            
        }


       /* public void CreateSubtileArrays()
        {
            TerrainCosts = new Dictionary<SurfaceType.TransportType, MapManager.SubtileValue[][]>();

            MapManager.SubtileValue[][] newmap = null;
            foreach (var item in Enum.GetValues(typeof(SurfaceType.TransportType)))
            {
                if ((SurfaceType.TransportType)item != SurfaceType.TransportType.Air)
                {
                    Common.InitJaggedArray(ref newmap, subTileWidth, subTileHeight);
                    TerrainCosts.Add((SurfaceType.TransportType)item, newmap); //new byte[subTileWidth, subTileHeight]); 
                }
            }

        }*/

        public TerrainTile GetTile(int x, int y)
        {
            return TileMap[x][y];
        }

      /*  public TerrainTile GetTile(Point? pos)
        {
            return TileMap[pos.Value.X][pos.Value.Y];
        }*/

        public TerrainTile GetTile(Point pos)
        {
            return TileMap[pos.X][pos.Y];
        }
        public TerrainTile GetTile(TilePos pos)
        {
            return TileMap[pos.X][pos.Y];
        }

       
        /// <summary>
        /// store the path in both collections; the purpose of the queue is to manage cleanup easily.
        /// Lars: I removed this since it seems the functionality is covered with region maps which are always up to date.
        /// </summary>
        /// <param name="onMap"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
    /*    public void RegisterBlockedPath(MovementMap onMap, SurfaceType.TransportType transportType, Vector3 from, Vector3 to) //Point from, Point to)
        {
            Queue<BlockedPath> blockedPaths; 
            if (!BlockedPaths.ContainsKey(onMap))
            {
                blockedPaths = new Queue<BlockedPath>();
                BlockedPaths.Add(onMap, blockedPaths);
            }
            else
            {
                blockedPaths = BlockedPaths[onMap];
            }

            Dictionary<FromTo, FromTo> blockedMap;
            if (!BlockedPathMaps.ContainsKey(onMap))
            {
                blockedMap = new Dictionary<FromTo, FromTo>();
                BlockedPathMaps.Add(onMap, blockedMap);
            }
            else
            {
                blockedMap = BlockedPathMaps[onMap];
            }

            blockedPaths.Enqueue(new BlockedPath(transportType, from, to, DateTime.Now));
            FromTo fromTo = new FromTo(transportType, from, to);
            if (!blockedMap.ContainsKey(fromTo))
            {
                blockedMap.Add(fromTo, fromTo);
            }
        }*/

        public static bool IsPointReachableInStraightLine(Vector3 from, Vector3 to)
        {
            Point fromSubtilePos = MapManager.WorldPosToSubtile(from); //.MapPosition; 
            Point toSubtilePos = MapManager.WorldPosToSubtile(to);

            // find out if the point is reachable from the location (terrain foot map)
            if (fromSubtilePos != toSubtilePos)
            {
                // get the tiles              
                List<Point> subtiles = MapManager.GetSubtilesTouchedByLine(from.ToVector2(), to.ToVector2());
                Point previousTile = subtiles[0];

                SubtileLayers map = The.Map.TerrainCosts[SurfaceType.TransportType.Foot];

                Point subtile;
                for (int i = 1; i < subtiles.Count; i++)
                {
                    subtile = subtiles[i];
                    // we don't test edges within the same tile:
                    if (subtile != previousTile)
                    {
                        // go from tile to tile, examine all edges to see that they are not blocked                        
                        // tests both connecting edges:
                        if (IsBlocked(map.GetValue(subtile)))
                        {
                            // unreachable in a straight line at least. Return the vehicle's location instead.                         
                            return false;
                            // transformedLocation = Parent.Location;
                            //  return;
                        }
                    }

                    previousTile = subtiles[i];
                }
            }

            return true;
        }

        /* no longer needed with the region maps..?
        public bool IsPathBlocked(MovementMap onMap, SurfaceType.TransportType transportType, Vector3 from, Vector3 to)
        {
            if (BlockedPathMaps.ContainsKey(onMap))
            {
                return BlockedPathMaps[onMap].ContainsKey(new FromTo(transportType, from, to));
            }
            else return false;
        }

        private void CleanupBlockedPaths()
        {
            int maxAgeInSeconds = 5;
            foreach (KeyValuePair<MovementMap, Queue<BlockedPath>> kvp in BlockedPaths)
            {
                while (kvp.Value.Count > 0 && kvp.Value.Peek().Timestamp.AddSeconds(maxAgeInSeconds) < DateTime.Now)
                {
                    // remove from both collections:
                    BlockedPath path = kvp.Value.Dequeue();
                    BlockedPathMaps[kvp.Key].Remove(new FromTo(path.Transport, path.From, path.To));
                }
            }

        }
        */

        public bool EntityIsInFogOfWar(Entity gameEntity, Allegiances.Allegiance allegiance)
        {
            return GetTile(gameEntity.MapPosition.Value).TileIsInFogOfWar(allegiance);

        }

        public bool ProcessIsInFogOfWar(SimProcess process, Allegiances.Allegiance allegiance)
        {
            if (process.MapPosition.HasValue)
            {
                return TileMap[process.MapPosition.Value.X][process.MapPosition.Value.Y].TileIsInFogOfWar(allegiance);
            }
            else return false;
        }

        /// <summary>
        /// store the path in both collections; the purpose of the queue is to manage cleanup easily and to keep track of outdated entries.
        /// </summary>
        /// <param name="onMap"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public void RegisterNoParkingSpot(EntityType vehicleType, ProtectionLevel protectionLevel, EntityType driverType, ThreatStance approach, Point destination) // Rectangle area)
        {
            NoParkingSpot noParkingSpot = new NoParkingSpot(/*transportType,*/vehicleType, protectionLevel, driverType, approach, destination/*, vehicle*/); //area);
            NoParkingSpotTimeStamp timestamp = new NoParkingSpotTimeStamp(noParkingSpot, DateTime.Now);

            if (!NoParkingSpots.ContainsKey(noParkingSpot))
            {
                NoParkingSpotsTimeStamps.Enqueue(timestamp);
                NoParkingSpots.Add(noParkingSpot, noParkingSpot);
            }
        }

        public bool IsNoParkingSpot(EntityType vehicleType, ProtectionLevel protectionLevel, EntityType driverEntityType, ThreatStance approach, Point destination/*, Entity vehicle*/) //Rectangle area)
        {
            NoParkingSpot noParkingSpot = new NoParkingSpot(vehicleType, protectionLevel, driverEntityType, approach, destination);//, vehicle); //area);
            return NoParkingSpots.ContainsKey(noParkingSpot);

        }

        private void CleanupNoParkingSpots()
        {
            int maxAgeInSeconds = 5;

            while (NoParkingSpotsTimeStamps.Count > 0 && NoParkingSpotsTimeStamps.Peek().Timestamp.AddSeconds(maxAgeInSeconds) < DateTime.Now)
            {
                // remove from both collections:
                NoParkingSpotTimeStamp timestamp = NoParkingSpotsTimeStamps.Dequeue();
                NoParkingSpots.Remove(timestamp.NoParkingSpot);
            }

        }
                
        
        public void Update(GameTime time)
        {          
            if (!The.Sim.IsPaused)
            {
              
                // Update


                if (noParkingSpotsRegulator.IsReady())
                {
                    CleanupNoParkingSpots();
                }


                /* pull Move regions from Move map instead
                 */
                foreach (var item in TerrainCosts)
                {
                    TerrainRegionMap regionMap = (TerrainRegionMap)item.Value.RegionMap; // (RegionMap)LookUp<ICyclable, CyclableID>.FindByID(item);
                    regionMap.Update(time);
                }    
                  /*
                foreach (var item in RegionMap.AllRegionMaps)
                {
                    RegionMap regionMap = (RegionMap)LookUp<ICyclable, CyclableID>.FindByID(item);                    
                    regionMap.Update(time);
                }        */        
            }
        }


        public void UpdateWhoCanSeeEntityMovingBetweenTiles(Entity entity, Intelligence entityIntelligence, Point? from, Point? to)
        {
            TerrainTile fromTile = null;
            if (from.HasValue)
            {
                fromTile = GetTile(from.Value);
            }

            
         //   TerrainTile toTile = TileMap[to.X][to.Y];
            TerrainTile toTile = null;

            if (to.HasValue)
            {
                toTile = GetTile(to.Value);
            }

            if (fromTile != null
                && toTile != null)  // There is now code in ContainedBy that handles the case of moving into containers (toTile = null).
                                   
            {
                foreach (Allegiances.Allegiance allegiance in fromTile.AllegiancesThatSeeThisTile)
                {
                    if (!(entityIntelligence != null && allegiance == entityIntelligence.Allegiance) // if the entity is intelligent, stay seen by its own allegiance
                        && (toTile == null || !toTile.AllegiancesThatSeeThisTile.Contains(allegiance)))  
                    {
                        // moving out of view:
                        allegiance.SharedKnowledge.UnSeeEntity(entity);  // checks UsesMemory, creates MF on toTile (in FOW)
                    }
                }
            }

            if (toTile != null)
            {
                foreach (Allegiances.Allegiance allegiance in toTile.AllegiancesThatSeeThisTile)
                {
                    if (!(entityIntelligence != null && allegiance == entityIntelligence.Allegiance) // if the entity is intelligent, stay seen by its own allegiance
                        && (fromTile == null || !fromTile.AllegiancesThatSeeThisTile.Contains(allegiance)))
                    {
                        if (allegiance.SharedKnowledge != null)
                        {
                            // moving into view:
                            if (!entity.RequiresRollToDetect())  // see items directly
                            {
                                allegiance.SharedKnowledge.SeeDetectableIfRelevant(entity); 
                               // allegiance.SharedKnowledge.SeeDetectable(entity); 
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Similar to when entities move into FOW, we must handle the case where entities are contained and dissappear from view that way.
        /// 
        /// allegiances that see the item being contained 
        /// if cannot see inside: remove knowledge about the item?
        /// if cannot see the container: add a memoryfact?
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="entityIntelligence"></param>
        public void UpdateWhoCanSeeEntityBeingContained(Entity entity, Intelligence entityIntelligence, EntityID? newContainerID) //, Point? oldTilePos)
        {
            Entity container = Entity.FindByID(newContainerID);

            if (container == null)
                return;

            TerrainTile startTile = null;
            if (entity.MapPosition.HasValue)
            {
                startTile = GetTile(entity.MapPosition.Value);
            }
            
           
            if (startTile != null) 
            {
                // create memory facts if the container is not seen
                foreach (Allegiances.Allegiance allegiance in startTile.AllegiancesThatSeeThisTile)
                {
                    if (entityIntelligence == null || allegiance != entityIntelligence.Allegiance) // if the entity is intelligent, stay seen by its own allegiance                   
                    {
                        IKnownEntityData entityData;
                        if (allegiance.SharedKnowledge.GetKnownData(entity.ID, out entityData) == EntityResult.SeenDirectly) // only if we see the entity directly
                        {

                            IKnownEntityData containerData;

                            // MemoryFact should be outside the container - so we have to call this before setting containedBy.
                            if (!allegiance.SharedKnowledge.CanSeeInsideContainer(container)
                                || allegiance.SharedKnowledge.GetKnownData(container.ID, out containerData) != EntityResult.SeenDirectly)
                            {
                                allegiance.SharedKnowledge.UnSeeEntity(entity); // when binal rat is unseeing Bob, what happens to MFs of the rifle and clip he is carrying..? Nothing. We cannot see inside Bob.
                            }

                            // continue;

                            /* do we need this code also, for watchers that can see the container, and see inside it?
                            // moving into view:
                            if (!entity.RequiresRollToDetect())  // see items directly
                            {
                                allegiance.SharedKnowledge.SeeDetectableIfRelevant(entity);
                                // allegiance.SharedKnowledge.SeeDetectable(entity); 
                            }
                               

                            if (!allegiance.SharedKnowledge.CanSeeInsideContainer(container))
                            {
                                allegiance.SharedKnowledge.RemoveFromCollectionsOfKnownEntities(entity.EntityType, entity.ID); // is it better to UnSee?? No. where would the memory fact go?
                            }
                            else
                            {
                                    
                                if (allegiance.SharedKnowledge.GetKnownData(container.ID, out containerData) != EntityResult.SeenDirectly)
                                {
                                    // building is in FOW. Unsee the entity.
                                      
                                    // Also, we can only have MemoryFacts in FOW, or..? If outside FOW, it would just be deprecated right after...
                                    allegiance.SharedKnowledge.UnSeeEntity(entity);
                                }
                                   
                            } */
                        }
                    }
                }
            }

            /* foreach (Allegiances.Allegiance allegiance in containerTile.AllegiancesThatSeeThisTile)
             {
                 if (!(entityIntelligence != null && allegiance == entityIntelligence.Allegiance) // if the entity is intelligent, stay seen by its own allegiance
                     && (containerTile.AllegiancesThatSeeThisTile.Contains(allegiance)))
                 {
                     if (allegiance.SharedKnowledge != null)
                     {
                         if (!allegiance.SharedKnowledge.CanSeeInsideContainer(container))
                         {
                             allegiance.SharedKnowledge.RemoveFromCollectionsOfKnownEntities(entity.EntityType, entity.ID);
                         }
                     }
                 }
             }*/
            // }
        }

                

     /*   public void TileToScreenOneRange(int relativeTileX, int relativeTileY, out float xScreen, out float yScreen)
        {
            xScreen = ((float)((relativeTileX * tileSize) - The.InGameUI.MapUI.dx) / (float)mapWindowWidth) * 2f - 1f;
            yScreen = ((float)(relativeTileY * tileSize - The.InGameUI.MapUI.dy) / (float)mapWindowHeight) * 2f - 1f;
        }
        */


        /// <summary>
        /// converts a tile to screen coords using absolute tile position in map
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="xScreen"></param>
        /// <param name="yScreen"></param>
      /*  public void AbsoluteTileToScreen(int x, int y, out int xScreen, out int yScreen)
        {
            xScreen = ((x - The.InGameUI.MapUI.mapX) * tileSize) - (int)The.InGameUI.MapUI.dx;
            yScreen = (y - The.InGameUI.MapUI.mapY) * tileSize - (int)The.InGameUI.MapUI.dy;
        }*/

        



      /*  public Vector2 AbsoluteTileToScreen(int x, int y)
        {
            return new Vector2((float)(((x - The.InGameUI.MapUI.mapX) * tileSize) - The.InGameUI.MapUI.dx + tileSizeOver2),
                (float)((y - The.InGameUI.MapUI.mapY) * tileSize - The.InGameUI.MapUI.dy + tileSizeOver2));
        }*/

       
        /*
        public void AbsoluteTileToScreenNormalized(int x, int y, out float xScreen, out float yScreen)
        {
            
            xScreen = (float)(((x - mapX) * tileSize) - dx) / (float)The.Client.GraphicsDevice.Viewport.Width * 2f - 1f;
            yScreen = (float)(((y - mapY) * tileSize) - dy) / (float)The.Client.GraphicsDevice.Viewport.Height * -2f + 1f;
        }
        */

        /*   public Point GetMapWindowPosition()
           {
               return new Point(mapX * tileWidth + dx, mapY * tileHeight + dy);
           }*/
     /*   public Vector2 GetMapWindowPosition()
        {
            return MapWindowWorldPosition;
            //return new Vector2(mapX * tileWidth + dx, mapY * tileHeight + dy);
        }*/

       

        public int ClampTileMapXPosition(int x)
        {
            return Common.Clamp(x, 0, mapTileWidth - 1);
        }
        public int ClampTileMapYPosition(int y)
        {
            return Common.Clamp(y, 0, mapTileHeight - 1);
        }

        public Point ClampTileMapPosition(Point pos)
        {
            pos.X = Common.Clamp(pos.X, 0, mapTileWidth - 1);
            pos.Y = Common.Clamp(pos.Y, 0, mapTileHeight - 1);

            return pos;
        }

        public Point ClampSubtileMapPosition(Point pos)
        {
            pos.X = Common.Clamp(pos.X, 0, mapSubtileWidth - 1);
            pos.Y = Common.Clamp(pos.Y, 0, mapSubtileHeight - 1);

            return pos;
        }

        public Vector2 ClampWorldPosition(Vector2 pos)
        {
            pos.X = MathHelper.Clamp(pos.X, 0.01f, MaxWorldPos.X);
            pos.Y = MathHelper.Clamp(pos.Y, 0.01f, MaxWorldPos.Y);

            return pos;
        }

        public WorldLocation ClampWorldPosition(WorldLocation pos)
        {
            pos.X = MathHelper.Clamp(pos.X, 0.01f, MaxWorldPos.X);
            pos.Y = MathHelper.Clamp(pos.Y, 0.01f, MaxWorldPos.Y);

            return pos;
        }

        public Vector3 ClampWorldPosition(Vector3 position)
        {
            position.X = MathHelper.Clamp(position.X, 0.01f, MaxWorldPos.X);
            position.Y = MathHelper.Clamp(position.Y, 0.01f, MaxWorldPos.Y);

            return position;

           // return Vector3.Clamp(position, Vector3.Zero, MaxWorldPos);
        }

       
        public static Vector3 ClampWorldPositionToTile(Point tilePos, Vector3 position)
        {
            Vector2 topLeftOfTile = new Vector2(tilePos.X * tileSize, tilePos.Y * tileSize);
            position.X = MathHelper.Clamp(position.X, topLeftOfTile.X, topLeftOfTile.X + tileSize - locationEpsilon);
            position.Y = MathHelper.Clamp(position.Y, topLeftOfTile.Y, topLeftOfTile.Y + tileSize - locationEpsilon);
            return position;
        }

        public static Vector3 ClampWorldPositionToSubTile(Point subTilePos, Vector3 position)
        {
            Vector2 topLeftOfTile = new Vector2(subTilePos.X * subTileSize, subTilePos.Y * subTileSize);
            position.X = MathHelper.Clamp(position.X, topLeftOfTile.X, topLeftOfTile.X + subTileSize - locationEpsilon);
            position.Y = MathHelper.Clamp(position.Y, topLeftOfTile.Y, topLeftOfTile.Y + subTileSize - locationEpsilon);
            return position;
        }

        public static Point ClampSubTileToTile(Point subtilePos, Point tile)
        {
            //Point tile = new Point(subtilePos.X / 3, subtilePos.Y / 3);
            Point tileCenterSubtile = TileCenterToSubTile(tile);

            return new Point(
                Common.Clamp(subtilePos.X, tileCenterSubtile.X - 1, tileCenterSubtile.X + 1),
                Common.Clamp(subtilePos.Y, tileCenterSubtile.Y - 1, tileCenterSubtile.Y + 1));
        }

        public bool TileIsOnMap(int x, int y)
        {
            return x >= 0 && x < mapTileWidth
                && y >= 0 && y < mapTileHeight;
        }
        public bool TileIsOnMap(Point tile)
        {
            return tile.X >= 0 && tile.X < mapTileWidth
                && tile.Y >= 0 && tile.Y < mapTileHeight;
        }
        public bool TileIsOnMap(TilePos tile)
        {
            return tile.X >= 0 && tile.X < mapTileWidth
                && tile.Y >= 0 && tile.Y < mapTileHeight;
        }

        public bool SubtileIsOnMap(Point subtile)
        {
            return subtile.X >= 0 && subtile.X < mapSubtileWidth
                && subtile.Y >= 0 && subtile.Y < mapSubtileHeight;
            
        }

        public bool WorldLocationIsOnMap(Vector2 location)
        {
            return location.X >= 0 && location.X < MapWorldWidth
                && location.Y >= 0 && location.Y < MapWorldHeight;
        }

        public static Vector3 EdgeOfTileToWorldPos(int tileX, int tileY)
        {
            return new Vector3(tileSize * tileX, tileSize * tileY, 0f);
        }

        public static Vector2 EdgeOfTileToWorldPos(Point tile)
        {
            return new Vector2(tileSize * tile.X, tileSize * tile.Y);
        }

        public static Vector3 EdgeOfTileToWorldPosV3(Point tile)
        {
            return new Vector3(tileSize * tile.X, tileSize * tile.Y, 0f);
        }

        public static Vector3 TileEdgeToWorldPos(Point tile)
        {
            return new Vector3(tileSize * tile.X, tileSize * tile.Y, 0f); 
        }
        public static Vector3 TileToWorldPos(Point tile)
        {
            return new Vector3(tileSize * tile.X + tileSizeOver2, tileSize * tile.Y + tileSizeOver2, 0f); 
        }
        public static Vector3 TilePosToWorldPos(TilePos tile)
        {
            return new Vector3(tileSize * tile.X + tileSizeOver2, tileSize * tile.Y + tileSizeOver2, 0f); 
        }
        public static Vector3 TileToWorldPos(TerrainTile tile)
        {
            return new Vector3(tileSize * tile.X + tileSizeOver2, tileSize * tile.Y + tileSizeOver2, 0f); 
        }
        public static WorldLocation TilePosToWorldLocation(TilePos tile)
        {
            return new WorldLocation(tileSize * tile.X + tileSizeOver2, tileSize * tile.Y + tileSizeOver2, 0f); 
        }

        public static Vector2 TileToWorldPosVector2(Point tile)
        {
            return new Vector2(tileSize * tile.X + tileSizeOver2, tileSize * tile.Y + tileSizeOver2);  //Vector2(tileSize * (tile.X + 0.5f), tileSize * (tile.Y + 0.5f));
        }
        public static void TileToWorldPos(int tileX, int tileY, out float xPos, out float yPos)
        {
            xPos = tileSize * tileX + tileSizeOver2;
            yPos = tileSize * tileY + tileSizeOver2;
        }
        public static Point TileToCenterSubtile(Point tile)
        {
            return new Point((int)(3f * (tile.X + 0.5f)), (int)(3f * (tile.Y + 0.5f)));
        }
        public static Point TileToUpperLeftSubtile(Point tile)
        {
            return new Point(SubtilesPerTileLength * tile.X, SubtilesPerTileLength * tile.Y);
        }
        public static WorldLocation VaryLocationWithinSubtile(WorldLocation location)
        {
            return new WorldLocation(VaryLocationWithinSubtile(location.ToVector3()));
        }

        public static Vector3 VaryLocationWithinSubtile(Vector3 location)
        {
            Point subTilePos = MapManager.WorldPosToSubtile(location);

            // add a small offset to avoid 'Point' piles or stacks
            location.X += The.Sim.GameplayRandomGenerator.RandomBetween(-8, 8);
            location.Y += The.Sim.GameplayRandomGenerator.RandomBetween(-8, 8);

            // clamp to stay within tile:
            location = MapManager.ClampWorldPositionToSubTile(subTilePos, location);

            return location;
        }

        public static Vector3 SubTileToWorldPos(AI.Pathfinding.PathFinderNode tile)
        {
            return new Vector3(subTileSize * (tile.AbsoluteX + 0.5f), subTileSize * (tile.AbsoluteY + 0.5f), 0f);
        }

        public static Vector2 SubTileToWorldPos(Point subtile)
        {
            return new Vector2(subTileSize * (subtile.X + 0.5f), subTileSize * (subtile.Y + 0.5f));
        }

        public static Vector3 SubTileToWorldPos3(SubtilePos subtile)
        {
            return new Vector3(subTileSize * (subtile.X + 0.5f), subTileSize * (subtile.Y + 0.5f), 0f);
        }

        public static Vector3 SubTileToWorldPos3(Point subtile)
        {
            return new Vector3(subTileSize * (subtile.X + 0.5f), subTileSize * (subtile.Y + 0.5f), 0f);
        }

        public static Vector3 SubTileEdgeToWorldPos3(Point tile)
        {
            return new Vector3(subTileSize * tile.X, subTileSize * tile.Y, 0f);
        }

      /*  public static Vector2 SubTileCenterToWorldPos(Point subtile)
        {
            return new Vector2(subTileSize * subtile.X + subTileSizeOver2, subTileSize * subtile.Y + subTileSizeOver2);
        }*/

        public static Point SubTileToTilePos(Point subtile)
        {
            return new Point(subtile.X / 3, subtile.Y / 3);
        }

        public static Point WorldPosToTile(Vector2 pos)
        {
            return new Point((int)(oneOverTileSize * pos.X), (int)(oneOverTileSize * pos.Y));
        }
        public static Point WorldPosToTile(Vector3 pos)
        {
            return new Point((int)(oneOverTileSize * pos.X), (int)(oneOverTileSize * pos.Y));
        }

        public static TilePos WorldPosToTilePos(Vector3 pos)
        {
            return new TilePos((int)(oneOverTileSize * pos.X), (int)(oneOverTileSize * pos.Y));
        }
        public static TilePos WorldPosToTilePos(WorldLocation pos)
        {
            return new TilePos((int)(oneOverTileSize * pos.X), (int)(oneOverTileSize * pos.Y));
        }
        public static Point WorldPosToSubtile(Vector3 pos)
        {
            return new Point((int)(oneOverSubtileSize * pos.X), (int)(oneOverSubtileSize * pos.Y));
        }
        public static Point WorldPosToSubtile(Vector2 pos)
        {
            return new Point((int)(oneOverSubtileSize * pos.X), (int)(oneOverSubtileSize * pos.Y));
        }
        public static Point WorldPosToSubtile(WorldLocation pos)
        {
            return new Point((int)(oneOverSubtileSize * pos.X), (int)(oneOverSubtileSize * pos.Y));
        }
        public static SubtilePos WorldPosToSubtilePos(WorldLocation pos)
        {
            return new SubtilePos((ushort)(oneOverSubtileSize * pos.X), (ushort)(oneOverSubtileSize * pos.Y));
        }
        public static SubtilePos WorldPosToSubtilePos(Vector3 pos)
        {
            return new SubtilePos((ushort)(oneOverSubtileSize * pos.X), (ushort)(oneOverSubtileSize * pos.Y));
        }
        public static Point TileEdgeToSubtile(Point tile)
        {
            return new Point(tile.X * SubtilesPerTileLength, tile.Y * SubtilesPerTileLength);
        }

        public static SubtilePos TileEdgeToSubtile(TilePos tile)
        {
            return new SubtilePos((ushort)(tile.X * SubtilesPerTileLength), (ushort)(tile.Y * SubtilesPerTileLength));
        }

        public static Point TileEdgeToSubtile(int x, int y)
        {
            return new Point(x * SubtilesPerTileLength, y * SubtilesPerTileLength);
        }

        public static Point TileAndRelativeSubtileToAbsoluteSubtile(int x, int y, int relSubtileX, int relSubtileY)
        {
            return new Point(x * SubtilesPerTileLength + relSubtileX, y * SubtilesPerTileLength + relSubtileY);
        }

        public bool WorldLocationIsInsideMap(Vector3 location)
        {            
            return location.X >= 0.01f && location.X <= MaxWorldPos.X
                && location.Y >= 0.01f && location.Y <= MaxWorldPos.Y;
        }

        /// <summary>
        /// Returns position relative to the CORNER! of the tile
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static Vector2 WorldPosToPositionWithinTileFromCorner(Vector3 pos)
        {
            Vector2 relativePos;
            relativePos.X = pos.X % tileSize;
            relativePos.Y = pos.Y % tileSize;
            return relativePos;
        }

        /// <summary>
        /// Returns position relative to the center of the tile
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        /*    public static Vector2 WorldPosToPositionWithinTile(Vector2 pos)
            {
                pos.X = pos.X % tileSize - tileSizeOver2;
                pos.Y = pos.Y % tileSize - tileSizeOver2;
                return pos;
            }*/

        /// <summary>
        /// Returns position relative to the center of the tile
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static Vector2 WorldPosToPositionWithinTile(Vector3 pos)
        {
            Vector2 relativePos;
            relativePos.X = pos.X % tileSize - tileSizeOver2;
            relativePos.Y = pos.Y % tileSize - tileSizeOver2;
            return relativePos;
        }


        /// <summary>
        /// Returns position relative to the center of the tile
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        /*  public static void WorldPosToTilePosAndPositionWithinTile(Point relpos, out Point tilePos, out Vector2 relativePosToCenter)
          {
              Vector3 pos = new Vector3(relpos.X, relpos.Y, 0f);
              relativePosToCenter = WorldPosToPositionWithinTile(pos);
              tilePos = WorldPosToTile(pos);
          }*/

        /*  public static void WorldPosToTilePosAndPositionWithinTile(Vector3 location, out Point tilePos, out Vector2 relativePosToCenter)
          {
              relativePosToCenter = WorldPosToPositionWithinTile(location);
              tilePos = WorldPosToTile(location);
          }*/

        

       


        public bool TileRectangleIsInsideMap(int x, int y, int widthInTiles, int heightInTiles)
        {
            return (x >= 0 && x + widthInTiles <= mapTileWidth
                && y >= 0 && y + heightInTiles <= mapTileHeight);

            /*  return (x >= 0 && x + widthInTiles <= TileMap.GetUpperBound(0)
                  && y >= 0 && y + heightInTiles <= TileMap.GetUpperBound(1));*/

        }


        public static bool PointIsWithinArea<T>(T[][] map, Point point)
        {
            return point.X >= 0 && point.X < Common.GetJaggedArrayWidth(map)
                && point.Y >= 0 && point.Y < Common.GetJaggedArrayHeight(map);
        }







        public bool GetClosestAccessiblePoint(SubtileLayers map, Vector3? fromLocation, Vector3 toLocation, bool stayInsideTile, out Point? closestSubtile)
        {
            Point toSubtile = WorldPosToSubtile(toLocation);


            if (!MapManager.IsBlocked(map.GetValue(toSubtile)))
            {
                closestSubtile = toSubtile;
                return true;
            }

            Point? fromSubtile = null;
            if (fromLocation.HasValue)
            {
                fromSubtile = WorldPosToSubtile(fromLocation.Value);
            }

            Point start, end;

            if (stayInsideTile)
            {
                // clamp to the tile boundaries that we are in
                Point tile = new Point(toSubtile.X / 3, toSubtile.Y / 3);
                Point tileCenterSubtile = TileCenterToSubTile(tile);

                start = ClampSubTileToTile(new Point(toSubtile.X - 1, toSubtile.Y - 1), tile);
                end = ClampSubTileToTile(new Point(toSubtile.X + 1, toSubtile.Y + 1), tile);

            }
            else
            {
                start = ClampSubtileMapPosition(new Point(toSubtile.X - 1, toSubtile.Y - 1));
                end = ClampSubtileMapPosition(new Point(toSubtile.X + 1, toSubtile.Y + 1));
            }

            Point? bestPoint = null;
            float bestDistance = 100000000f;

            float thisDistance;
            Point thisPoint;
            for (int x = start.X; x < end.X; x++)
            {
                for (int y = start.Y; y < end.Y; y++)
                {
                    thisPoint = new Point(x, y);
                    if (!MapManager.IsBlocked(map.GetValue(x, y)))
                    {
                        if (fromSubtile.HasValue)
                        {
                            thisDistance = Common.DistanceOctile(thisPoint, fromSubtile.Value);

                            if (thisDistance < bestDistance)
                            {
                                bestPoint = thisPoint;
                                bestDistance = thisDistance;
                            }
                        }
                        else
                        {
                            closestSubtile = thisPoint;
                            return true;
                        }
                    }
                }
            }

            if (bestPoint.HasValue)
            {
                closestSubtile = bestPoint.Value;
                return true;
            }

            closestSubtile = null;
            return false;
        }

        public Expedition GetClosestExpedition(Vector3 location)
        {
            Point tilePos = WorldPosToTile(location);
            return GetClosestExpedition(tilePos);
        }

        public Expedition GetClosestExpedition(Point tilePos)
        {
            return TileMap[tilePos.X][tilePos.Y].OperatingAreaOf;
        }

       

     /*   private bool AddHarvestJobsForTile(Point tile)
        {
            TerrainTile clickedTile = TileMap[tile.X][tile.Y];

            Expedition expedition = GetClosestExpedition(tile);

            // create a job for every crop item on the tile!

            bool jobsAdded = false;

            if (clickedTile.TreesOnTile != null)
            {
                Trees.Tree treeComponent;
                foreach (Entity tree in clickedTile.TreesOnTile)
                {
                    tree.Find(out treeComponent);
                    if (treeComponent.Crops != null)
                    {
                        foreach (KeyValuePair<ResourceType, Crop> kvp in treeComponent.Crops)
                        {
                            jobsAdded = AddHarvestJobs(expedition, kvp.Value) | jobsAdded;
                        }
                    }
                }
            }

            // designer placed... for now
            if (clickedTile.TileResources != null)
            {
                foreach (var container in clickedTile.TileResources)
                {
                    jobsAdded = AddHarvestJobs(expedition, container.Value) | jobsAdded;
                }
            }

          

            // vegetation resources - these won't get implemented for the demo..
            Terrain terrain;
            if (clickedTile.Terrain != null)
            {
                terrain = clickedTile.Terrain;

                jobsAdded = AddHarvestJobs(expedition, terrain) | jobsAdded;
            }
            else
            {
                for (int sx = 0; sx < 3; sx++)
                {
                    for (int sy = 0; sy < 3; sy++)
                    {
                        terrain = clickedTile.TerrainSubtiles[sx][sy];
                        AddHarvestJobs(expedition, terrain);
                    }
                }
            }            

            return jobsAdded; // tile;
        }
        */

      


        public bool IsHuntingZone(Allegiances.Allegiance allegiance, Point mapPosition)
        {
            TerrainTile tile = GetTile(mapPosition);

            List<Zone> listOfZones = tile.GetListOfZones(allegiance);

            if (listOfZones != null)
            {
                return listOfZones.Exists(z => z.ZoneHunt.HasFindPreyJobs()); // z.FindPreyJob != null);
            }

            return false;
            
        }
       

      /*  private static bool AddHarvestJobs(Expedition expedition, Terrain terrain)
        {
            bool jobsAdded = false;

            if (terrain.Vegetation != null)
            {
                foreach (KeyValuePair<string, LowVegetation> kvp in terrain.Vegetation)
                {
                    if (kvp.Value.Crop != null)
                    {
                        jobsAdded = AddHarvestJobs(expedition, kvp.Value.Crop) | jobsAdded;
                    }
                }
            }

            return jobsAdded;
        }

        private static bool AddHarvestJobs(Expedition expedition,  IResourceItemContainer container)
        {
            ResourceType resourceType = container.ResourceType;
            List<Job> jobs = JobManager.GetProductionJobs(expedition.ExpeditionOwner, resourceType.ResourceItem);

            bool jobsAdded = false;
            foreach (IResourceItem item in container.ResourceItems)
            {
                jobsAdded = AddHarvestJob(expedition, resourceType, jobs, item) | jobsAdded;
            }

            return jobsAdded;
        }

        private static bool AddHarvestJob(Expedition expedition, ResourceType resourceType, List<Job> jobs, IResourceItem item)
        {
            if (!jobs.Exists(j => j is ProcessJob && ((ProcessJob)j).HarvestJob != null && ((ProcessJob)j).HarvestJob.Item == item)) // see that this item was not added already
            {
                ProcessType processType = GameData.Instance.ProcessYieldsThisOutput[resourceType.ResourceItem][0];

                ProcessJob pJob = new ProcessJob(null, processType, jobs);
                pJob.HarvestJob = new HarvestJob(pJob, item, expedition.ExpeditionOwner);

                return true;
            }

            return false;
        }*/

        public static bool IsStandingOnNonMovingEntity(Entity entity)
        {
            TerrainTile tile = The.Map.GetTile(entity.MapPosition.Value);

            Point entitySubtile = MapManager.WorldPosToSubtile(entity.PlaySiteLocation);

            return SubtileHasNonMovingIntelligentEntity(entity, tile, entitySubtile);

        }

        public static bool SubtileHasNonMovingIntelligentEntity(Entity entity, TerrainTile tile, Point entitySubtile)
        {
            if (tile.EntitiesOnTile != null)
            {
                foreach (Entity otherEntity in tile.EntitiesOnTile)
                {
                    if (otherEntity != entity && otherEntity.IsIntelligentAndNonMoving())
                    {
                        // see if we are standing on the same subtile as this non-moving entity:
                        if (entitySubtile == MapManager.WorldPosToSubtile(otherEntity.PlaySiteLocation))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// find a spot to stand where there are no other entities...
        /// return null if there are no available spots
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="tile"></param>
        /// <returns></returns>
        public Point? FindUnoccupiedSubtileInsideTile(Entity entity, Point tilePos)
        {
            TerrainTile tile = The.Map.TileMap[tilePos.X][tilePos.Y];

            SubtileLayers terrainCosts = TerrainCosts[SurfaceType.TransportType.Foot];

            
            Point currentSubtile;

            // let's test the center first:
            currentSubtile = TileCenterToSubTile(tilePos); 
            if (!SubtileIsCompletelyBlocked(terrainCosts, currentSubtile))
            {
                if (!SubtileHasNonMovingIntelligentEntity(entity, tile, currentSubtile))
                {
                    // subtile is clear
                    return currentSubtile;
                }
            }

            Point upperLeft = TileToUpperLeftSubtile(tilePos);

            for (int sx = 0; sx < 3; sx++)
			{
			    for (int sy = 0; sy < 3; sy++)
			    {
                    currentSubtile = new Point(upperLeft.X + sx, upperLeft.Y + sy);
                    if (!SubtileIsCompletelyBlocked(terrainCosts, currentSubtile))
                    {
                        if (!SubtileHasNonMovingIntelligentEntity(entity, tile, currentSubtile))
                        {
                            // subtile is clear
                            return currentSubtile;
                        }

                       /* if (tile.EntitiesOnTile != null)
                        {
                            foreach (Entity otherEntity in tile.EntitiesOnTile)
                            {
                                if (otherEntity != entity && otherEntity.IsIntelligentAndNonMoving())
                                {
                                    // see if we are standing on the same subtile as this non-moving entity:
                                    if (currentSubtile == MapManager.WorldPosToSubtile(otherEntity.Location))
                                    {
                                        continue;
                                    }
                                }
                            }
                        }*/

                    }			 
			    }
			}

            return null;
        }

        



        /// <summary>
        /// Init stuff that needs the dimensions of the map.
        /// </summary>
        public void InitPostLoadMap()
        {
            
            foreach (var item in TerrainCosts)
            {
                while (item.Value.RegionMap.CycleOnce() == false); // how long does this take..?   should it be done on the loading screen?            
            }

           // The.MapUI.InitPostLoadMap();
          
        }


        public void Init()
        {
          
            InitMaps();           
        }

        public static string ComposeMapDataXmlFilePathFromFolderPath(string folderPath)
        {
            return System.IO.Path.Combine(folderPath, Config.MapDataName);

            //return Config.GetDataFolderPath(Config.DataType.RGMap, name, "MapData.xml");
        }

        public static string ComposeMapDataXmlFilePath(string name)
        {
            return Config.GetDataFolderPath(Config.DataType.RGMap, name, Config.MapDataName); // "MapData.xml");         
        }

        public static string ComposeMapDataFolderPath(string folderName, string fileOrFolderName = "")
        {          
            string mapDataFolderPath = Config.GetDataFolderPath(Config.DataType.RGMap); 

            return System.IO.Path.Combine(mapDataFolderPath, folderName, fileOrFolderName);
        }

        private void InitMaps()
        {            
            var mapDirs = MapEditorSaveLoadPanel.GetListOfMapFolders(); //new List<string>(); // new List<MapData>();

            AllMaps = mapDirs.Select(d => d.Name).ToList();

            /*
            //Create our own namespaces for the output
           XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            //Add an empty namespace and empty value
            ns.Add("", "");
            XmlSerializer s = new XmlSerializer(typeof(MapData));


           
            // Deserialization
            MapData mapData;

            string[] mapFolders = System.IO.Directory.GetDirectories(GameDataLoader.DataFolder + MapsFolder, "*", System.IO.SearchOption.TopDirectoryOnly);

            foreach (string folder in mapFolders)
            {
                string[] mapDataFiles = System.IO.Directory.GetFiles(folder, "MapData.xml");

                if (mapDataFiles.Length > 0)
                {
                    mapData = MapEditorSaveLoadPanel.LoadMapData(mapDataFiles[0]);

                                

                    AllMaps.Add(mapData);
                }
                
            }
            */
        }

        /*  private void InitMap(MapData map1)
          {
              map1.TileMap = new MapDataTile[map1.Dimensions.X][];
              for (int x = 0; x < map1.Dimensions.X; x++)
              {
                  MapDataTile[] verticalTiles = new MapDataTile[map1.Dimensions.Y];
                  map1.TileMap[x] = verticalTiles;

                  for (int y = 0; y < map1.Dimensions.Y; y++)
                  {
                      verticalTiles[y] = new MapDataTile() { };
                  }
              }
          }*/

        public void SaveMap(string fullFolderPath, MapData mapData, bool createNewFolders)
        {
            mapData.Dimensions.X = mapTileWidth;
            mapData.Dimensions.Y = mapTileHeight;


            // InitMap(mapData);

            //important! clear old lists:
            if (mapData.SavedMapEntities != null)
            {
                mapData.SavedMapEntities.Clear();
            }
            if (mapData.Trees != null)
            {
                mapData.Trees.Clear();
            }
            if (mapData.Tiles != null)
            {
                mapData.Tiles.Clear();
            }

            Trees.Tree tree;
         /*   Entities.Rock rock;
            Entities.BiologicalEntity bioEntity;
            */

            TerrainTile currentTile;
            EditorData editorData;
            for (int x = 0; x < mapTileWidth; x++)
            {
                for (int y = 0; y < mapTileHeight; y++)
                {
                    currentTile = TileMap[x][y];

                    if (currentTile.EntitiesOnTile != null)
                    {
                        foreach (Entity entity in currentTile.EntitiesOnTile)
                        {
                            if (entity.Find(out editorData)) // entity.IsEditorPlaced)
                            {
                                /* if (entity.EntityType.TerrainType != null && entity.EntityType.TerrainType.CanBeMapEditorPlaced)
                                 {*/
                                if (!entity.Find(out tree)) // save trees separately...
                                {
                                    if (mapData.SavedMapEntities == null)
                                    {
                                        mapData.SavedMapEntities = new List<EntityData>();
                                    }

                                    EntityData savedEntity = CreateEntityDataFromEntity(entity);

                                    mapData.SavedMapEntities.Add(savedEntity);
                                }
                            }
                        }
                    }
                    
                    if (currentTile.TreesOnTile != null)
                    {
                        foreach (Entity entity in currentTile.TreesOnTile)
                        {
                            if (entity.Find(out editorData)) // if (entity.IsEditorPlaced)
                            {
                                //Trees.Tree tree;
                                if (entity.Find(out tree))
                                {
                                    if (mapData.Trees == null)
                                    {
                                        mapData.Trees = new List<EntityData>();
                                    }

                                    EntityData savedEntity = CreateEntityDataFromEntity(entity);

                                    mapData.Trees.Add(savedEntity);
                                }
                            }
                        }
                    }

                    if (currentTile.DesignerPlacedResources != null)
                    {
                        if (mapData.Tiles == null)
                        {
                            mapData.Tiles = new List<Tile>();
                        }

                        Tile tile = new Tile() 
                        { 
                            Resources = currentTile.DesignerPlacedResources, 
                           // Resources = currentTile.DesignerPlacedResources.ToArray(), 
                            Position = new Point(x, y) 
                        };
                        mapData.Tiles.Add(tile);
                    }                
                }
            }

           /* string mapFolder = ComposeMapDataFolderPath(mapData.FolderName);
            if (!System.IO.Directory.Exists(mapFolder))
            {
                System.IO.Directory.CreateDirectory(mapFolder);
            }*/

            if (!System.IO.Directory.Exists(fullFolderPath))
            {
                System.IO.Directory.CreateDirectory(fullFolderPath);
            }

            // save to disk
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            //Add an empty namespace and empty value
            ns.Add("", "");
            XmlSerializer s = new XmlSerializer(typeof(MapData));

          //  using (TextWriter w = new StreamWriter(ComposeMapDataXmlFilePath(mapData.FolderName)))
            using (TextWriter w = new StreamWriter(ComposeMapDataXmlFilePathFromFolderPath(fullFolderPath)))
            {
                s.Serialize(w, mapData, ns);
            }

            if (createNewFolders)
            {
                // TODO: copy over texture pngs also

                System.IO.Directory.CreateDirectory(System.IO.Path.Combine(fullFolderPath, "Trees"));
                System.IO.Directory.CreateDirectory(System.IO.Path.Combine(fullFolderPath, "Vegetation"));
                System.IO.Directory.CreateDirectory(System.IO.Path.Combine(fullFolderPath, "Soil"));
            }
        }

        /// <summary>
        /// gets called when saving a map in the editor.
        /// extract the needed data from the entity and the temporary EditorData component
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private static EntityData CreateEntityDataFromEntity(Entity entity)
        {
            Trees.Tree tree;
            Entities.Rock rock;
            UWGame.SimSide.Entities.Biological.BiologicalEntity bioEntity;
            EditorData editorData;
            EntityData savedEntity;

            entity.Find(out editorData);

            savedEntity = new EntityData() 
            { 
                Name = entity.Name, 
                EntityKey = entity.EntityType.KeyName, 
                Location = entity.Location,                 
                FlipHorizontally = entity.FlipHorizontally              
                
            };

            if (!entity.Find(out tree)) // !editorData.TreeAgeGroup.HasValue)
            {
                // only save bulk for non-trees, for trees we want bulk to be defined from age, and setting the bulk here will init Size in turn when loading...
                savedEntity.Bulk = entity.Bulk;
            }


            if (entity.Renderable./*TODO DECOUPLE*/RenderAsModel != null)
            {
                savedEntity.Rotation = MathHelper.ToDegrees(entity.Rotation); // MathHelper.ToRadians( entity.Locomotor.Rotation;
            }

            if (tree != null)
            {
                savedEntity.Tree = new MapEditor.Tree();

                // MIGRATE ONLY: set age group from Size: TODO: delete this!!
               /* if (tree.Size >= 1)
                {
                    savedEntity.Tree.AgeGroup = AgeGroup.Grown;
                }
                else
                {
                    savedEntity.Tree.AgeGroup = AgeGroup.Young;
                }*/
                               
                if (editorData.TreeAgeGroup.HasValue)
                {
                    savedEntity.Tree.AgeGroup = editorData.TreeAgeGroup.Value;
                }
                else
                {   
                    savedEntity.Tree.AgeInYears = tree.AgeInYears;                           
                }

                savedEntity.Tree.ShapeFactor = tree.ShapeFactor; 
                savedEntity.Tree.InSeason = tree.InSeason;
                savedEntity.Tree.Flavour = tree.Flavour;
            }

            if (entity.Find(out rock))
            {
                savedEntity.Rock = new MapEditor.Rock()
                {
                };
            }

            if (entity.Find(out bioEntity))
            {
                savedEntity.BioEntity = new MapEditor.BiologicalEntity()
                {
                    AgeInYears = new NormalDistribution() { Mean = bioEntity.AgeGroup.Age },
                    CasteKey = bioEntity.CasteType.Name,
                    RaceKey = bioEntity.RaceType.Name
                };

              
                if (editorData != null)
                {
                    savedEntity.MemberOf = new AllegianceAndExpedition()
                    {
                        AllegianceKey = editorData.AllegianceKey,
                        ExpeditionKey = editorData.ExpeditionName
                    };                    
                }
            }

            if (editorData != null)
            {
               // savedEntity.SpawnDate = editorData.SpawnDate;
                savedEntity.Resources = editorData.Resources;

            }

            return savedEntity;
        }

        /*     private void InitFeatureConnections()
             {
                 TerrainTile.AllRenderedRoadConnections = new Rectangle?[8, 8];
            
                 // double pieces:
                 TerrainTile.AllRenderedRoadConnections[(int)Common.Direction.East, (int)Common.Direction.South] = UWGame.SimSide.Instance.FlatSpriteSheet.SourceRectangle("gravel_E_S");

                 TerrainTile.AllRenderedRoadConnections[(int)Common.Direction.North, (int)Common.Direction.East] = UWGame.SimSide.Instance.FlatSpriteSheet.SourceRectangle("gravel_N_E");
                 TerrainTile.AllRenderedRoadConnections[(int)Common.Direction.North, (int)Common.Direction.SouthEast] = UWGame.SimSide.Instance.FlatSpriteSheet.SourceRectangle("gravel_N_SE");
                 TerrainTile.AllRenderedRoadConnections[(int)Common.Direction.North, (int)Common.Direction.West] = UWGame.SimSide.Instance.FlatSpriteSheet.SourceRectangle("gravel_N_W");

                 TerrainTile.AllRenderedRoadConnections[(int)Common.Direction.NorthWest, (int)Common.Direction.South] = UWGame.SimSide.Instance.FlatSpriteSheet.SourceRectangle("gravel_NW_S");

                 TerrainTile.AllRenderedRoadConnections[(int)Common.Direction.South, (int)Common.Direction.NorthEast] = UWGame.SimSide.Instance.FlatSpriteSheet.SourceRectangle("gravel_S_NE");
                 TerrainTile.AllRenderedRoadConnections[(int)Common.Direction.SouthWest, (int)Common.Direction.East] = UWGame.SimSide.Instance.FlatSpriteSheet.SourceRectangle("gravel_SW_E");
                 TerrainTile.AllRenderedRoadConnections[(int)Common.Direction.SouthWest, (int)Common.Direction.North] = UWGame.SimSide.Instance.FlatSpriteSheet.SourceRectangle("gravel_SW_N");

                 TerrainTile.AllRenderedRoadConnections[(int)Common.Direction.West, (int)Common.Direction.NorthEast] = UWGame.SimSide.Instance.FlatSpriteSheet.SourceRectangle("gravel_W_NE");

                 TerrainTile.AllRenderedRoadConnections[(int)Common.Direction.West, (int)Common.Direction.South] = UWGame.SimSide.Instance.FlatSpriteSheet.SourceRectangle("gravel_W_S");
                 TerrainTile.AllRenderedRoadConnections[(int)Common.Direction.West, (int)Common.Direction.SouthEast] = UWGame.SimSide.Instance.FlatSpriteSheet.SourceRectangle("gravel_W_SE");

                 // single pieces:
                 TerrainTile.AllRenderedRoadConnections[(int)Common.Direction.West, (int)Common.Direction.West] = UWGame.SimSide.Instance.FlatSpriteSheet.SourceRectangle("gravel_W");
                 TerrainTile.AllRenderedRoadConnections[(int)Common.Direction.NorthWest, (int)Common.Direction.NorthWest] = UWGame.SimSide.Instance.FlatSpriteSheet.SourceRectangle("gravel_NW");
                 TerrainTile.AllRenderedRoadConnections[(int)Common.Direction.North, (int)Common.Direction.North] = UWGame.SimSide.Instance.FlatSpriteSheet.SourceRectangle("gravel_N");
                 TerrainTile.AllRenderedRoadConnections[(int)Common.Direction.NorthEast, (int)Common.Direction.NorthEast] = UWGame.SimSide.Instance.FlatSpriteSheet.SourceRectangle("gravel_NE");
                 TerrainTile.AllRenderedRoadConnections[(int)Common.Direction.East, (int)Common.Direction.East] = UWGame.SimSide.Instance.FlatSpriteSheet.SourceRectangle("gravel_E");
                 TerrainTile.AllRenderedRoadConnections[(int)Common.Direction.SouthEast, (int)Common.Direction.SouthEast] = UWGame.SimSide.Instance.FlatSpriteSheet.SourceRectangle("gravel_SE");
                 TerrainTile.AllRenderedRoadConnections[(int)Common.Direction.South, (int)Common.Direction.South] = UWGame.SimSide.Instance.FlatSpriteSheet.SourceRectangle("gravel_S");
                 TerrainTile.AllRenderedRoadConnections[(int)Common.Direction.SouthWest, (int)Common.Direction.SouthWest] = UWGame.SimSide.Instance.FlatSpriteSheet.SourceRectangle("gravel_SW");


             }

             private void InitTrees()
             {
                 foreach (KeyValuePair<string, TreeTypeData> kvp in GameData.Instance.BillboardSpriteSheet.TreeTypeData)
                 {
                     TreeType treeType = GameData.Instance.AllTreeTypes["tree:" + kvp.Key];
                     treeType.HasSummerWinterCycle = kvp.Value.HasSummerWinterCycle;
                     treeType.HasDayNightCycle = kvp.Value.HasDayNightCycle;
                     treeType.MaxFlavours = kvp.Value.MaxFlavours;
                 }
             }*/

      /*  public static void CleanMap(MapData mapData)
        {
            mapData.SavedMapEntities = mapData.SavedMapEntities.Distinct(new EntityTypeLocationComparer()).ToList();

            //  mapData.Trees = mapData.Trees.Distinct(new TreeLocationComparer()).ToList();

        }*/

        MapLoader mapLoader;
        public void StartLoadMap(DirectoryInfo mapFolder)
        {
            mapLoader = new MapEditor.MapLoader(mapFolder);
        }

        public void StartLoadMap(string mapFolderName)
        {
            mapLoader = new MapEditor.MapLoader(mapFolderName);
        }

       // [EQATEC.Profiler.SkipInstrumentation]
        public bool LoadMapQueued() //string mapFolderName)
        {           
            if (mapLoader.QueueLoad()) // mapLoader.Load(mapToLoad);
            {
                mapLoader = null;
                return true;
            }

            return false;

          //  InitPostLoadMap();
        }

        public void SetDimensions(Point dimensions)
        {

            mapTileWidth = dimensions.X;
            mapTileHeight = dimensions.Y;


            mapSubtileWidth = SubtilesPerTileLength * mapTileWidth;
            mapSubtileHeight = SubtilesPerTileLength * mapTileHeight;

            MapWorldWidth = mapTileWidth * tileSize;
            MapWorldHeight = mapTileHeight * tileSize;

            NoOfSectorsAcrossWidth = (int)Math.Ceiling((double)mapSubtileWidth / (double)SectorSizeInSubtiles);
            NoOfSectorsAcrossHeight = (int)Math.Ceiling((double)mapSubtileHeight / (double)SectorSizeInSubtiles);

            float maxWorldPosX = MapWorldWidth - locationEpsilon;
            float maxWorldPosY = MapWorldHeight - locationEpsilon;
            MaxWorldPos = new Vector3(maxWorldPosX, maxWorldPosY, 10000f);
        }


        public bool InitCollisionTrees()
        {
            if (The.Sim.Mode == Sim.EngineMode.Game)
            {
                //Now that the map is all loaded, we can initialize the collision manager
                int maxCollidableEntities = 999;
                int livingEntityCap = 9;
                The.CollisionManager = new CollisionManager<Entity>(new Vector2(The.Map.MapWorldWidth, The.Map.MapWorldHeight), maxCollidableEntities);
                The.AgentQuadTree = new PointQuadTree<Entity>(new Vector2(The.Map.MapWorldWidth, The.Map.MapWorldHeight), livingEntityCap,7);
            }

            return true;
        }


        /// <summary>
        /// unused???
        /// </summary>
        /// <param name="location"></param>
        /// <param name="transport"></param>
        /// <param name="blockingAction"></param>
        public void BlockSubtile(Vector3 location, SurfaceType.TransportType transport, BlockingAction blockingAction)
        {
            Point subtile = WorldPosToSubtile(location);
            BlockTerrainSubtile(subtile, transport, blockingAction);
        }

      
        public void BlockTerrainSubtile(Point subtile, SurfaceType.TransportType transport, BlockingAction blockingAction)
        {
            //MLo added to avoid crash when placing terrain entities from map
            if (!The.Map.SubtileIsOnMap(subtile))
                return;


            //we are blocking. propagate the change - block all discomfort maps too.
          
            if (blockingAction == BlockingAction.Block)
            {                               
                // apply the new cost:
                SubtileValue newValue = SetCost(TerrainCosts[transport].GetValue(subtile), 0);
                TerrainCosts[transport].SetValueOnBottomLayer(subtile, newValue);
            }
            else
            {
                // remove the blocking - to what value?  what about roads!!!!
                SubtileValue newValue = SetCost(TerrainCosts[transport].GetValue(subtile), PlainsType.Instance.Cost((SurfaceType.TransportType)transport, SurfaceType.TerrainFeatures.None));
                TerrainCosts[transport].SetValueOnBottomLayer(subtile, newValue);
                    
            }


            PropagateChangeToDependentMaps(subtile, transport, blockingAction == BlockingAction.Block);

        }


        /// <summary>
        /// updates/marks as dirty any maps that depend on the data that was just changed.
        /// </summary>
        /// <param name="subtile"></param>
        /// <param name="transport"></param>
        /// <param name="isBlocking"></param>
        private void PropagateChangeToDependentMaps(Point subtile, SurfaceType.TransportType transport, bool isBlocking) // BlockingAction blockingAction)
        {
            /*  1. Mark the terrain sector regions as dirty. 
                2. Mark the movemap sectors as dirty for re-adding
            */
            // change the base terrain map and region map too:
            // this should now trigger recomputing the move map region graphs, since they are dependent on the terrain graph...
            MarkDirtyTerrainMapRegion(transport, subtile);


            ChangeMovementMaps(subtile, transport, isBlocking); // this should trigger re-adding the subtile sectors

        }



        public void IterateTileArea(Rectangle tileArea, Action<TerrainTile> iterateMethod) 
        {          
           
            for (int x = tileArea.Left; x < tileArea.Right; x++)
            {
                TerrainTile tile;
               
                for (int y = tileArea.Top; y < tileArea.Bottom; y++)
                {
                    tile = TileMap[x][y];

                    iterateMethod(tile);
                }
            }
        }


        public static void IterateSubtiles(Vector2 from, Vector2 to, Action<Vector2> iterateMethod)
        {
            float sts = subTileSize;
            
            for (float x = from.X; x <= to.X; x += sts)
            {
                for (float y = from.Y; y <= to.Y; y += sts)
                {
                    Vector2 subTile = new Vector2(x, y);

                    iterateMethod(subTile);
                }
            }
        }

        public static bool IterateSubtilesBreakOnTrue(Vector2 from, Vector2 to, Predicate<Vector2> iterateMethod)
        {
            float sts = subTileSize;

            for (float x = from.X; x <= to.X; x += sts)
            {
                for (float y = from.Y; y <= to.Y; y += sts)
                {
                    Vector2 subTile = new Vector2(x, y);

                    if (iterateMethod(subTile))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        
        private void ChangeMovementMaps(Point subtile, SurfaceType.TransportType transport, bool isBlocking)
        {          
            if (AllMovementMaps != null)
            {
                foreach (var moveMap in AllMovementMaps)
                {
                    // propagate the change:

                    /* #SECTORS: I commented out this code. I think it would interfere with logic in MovementMap.AddChildMapSectors where the terrain and the current move map values are compared...
                    if (isBlocking)
                    {  // this is an optimization that makes the blocked subtile take effect immediately rather that waiting for the next update cycle:
                        // I disabled this since it breaks the logic inside MovementMap.AddChildMapSectors where the terrain and the current move map values are compared...
                        // perhaps 
                        moveMap.BlockSubtile(transport, subtile.X, subtile.Y);
                    }*/
        
                    // mark as dirty:
                    moveMap.SetTerrainSectorDirty(transport, subtile);

                }
            }
        }

        private void MarkDirtyTerrainMapRegion(SurfaceType.TransportType transport, Point subtile)
        {
          
            RegionMap regionMap = TerrainCosts[transport].RegionMap;
            if (regionMap != null)
            {             
                // is the region map created yet?
                if (regionMap.RegionGraph.Count > 0)
                {
                    regionMap.MarkDirtySector(subtile.X, subtile.Y);
                }
            }
        }
        

        /// <summary>
        /// does not affect flags!!
        /// </summary>
        /// <param name="location"></param>
        /// <param name="newCost"></param>
        public void SetSubtileTerrainCost(Vector3 location, byte newCost)
        {
            foreach (SurfaceType.TransportType transport in MapTransportTypeArray) // Enum.GetValues(typeof(TerrainType.TransportType))) //int transport = 0; transport < TransportIndices.Length; transport++)
            {
                SetSubtileCost(location, transport, newCost);
            }
        }

        /// <summary>
        /// does not affect flags!!
        /// </summary>
        /// <param name="location"></param>
        /// <param name="newCost"></param>
        public void SetSubtileTerrainCost(Point subtilePos, byte newCost)
        {
            foreach (SurfaceType.TransportType transport in MapTransportTypeArray) 
            {
                SetSubtileCost(subtilePos, transport, newCost);
            }
        }

        public void SetSubtileTerrainValueFlag(Point subtilePos, SubtileValue flagToSet)
        {
            foreach (SurfaceType.TransportType transport in MapTransportTypeArray) 
            {
                SubtileLayers map = TerrainCosts[transport];

                map.SetValueOnBottomLayer(subtilePos, map.GetValue(subtilePos) | flagToSet);
            }
        }

        public void ClearSubtileTerrainValueFlag(Point subtilePos, SubtileValue flagToSet)
        {
            foreach (SurfaceType.TransportType transport in MapTransportTypeArray)
            {
                SubtileLayers map = TerrainCosts[transport];

                map.SetValueOnBottomLayer(subtilePos, map.GetValue(subtilePos) & ~flagToSet);
            }
        }

        public bool FlagIsSet(SubtilePos subtilePos, SurfaceType.TransportType transport, SubtileValue flagToTest)
        {
            SubtileLayers map = TerrainCosts[transport];

            return TestForFlag(map.GetValue(subtilePos), flagToTest);

        }
        public bool FlagIsSet(Point subtilePos, SurfaceType.TransportType transport, SubtileValue flagToTest)
        {
            SubtileLayers map = TerrainCosts[transport];

            return TestForFlag(map.GetValue(subtilePos), flagToTest);

        }

        public static bool TestForFlag(SubtileValue valueToTest, SubtileValue flagToTest)
        {
            return (valueToTest & flagToTest) != 0;
        }

        /*  public void ResetEdgeCost(Point mapPosition, Common.Direction edge, TerrainType.TransportType transport)
          {
              SetEdgeCost(mapPosition, edge, (int)transport, PlainsType.Instance.Cost(transport, TerrainType.TerrainFeatures.None));  
          }*/

       

        public void SetSubtileTerrainCostToSurfaceType(Vector3 location)
        {
            SetSubtileCostToSurfaceType(WorldPosToSubtile(location));           
        }

        /// <summary>
        /// sets the cost of the subtile to that which is determined by its surface type (land/water)
        /// </summary>
        /// <param name="subTilePos"></param>
        public void SetSubtileCostToSurfaceType(Point subTilePos)
        {
            SurfaceType surface = GetSurfaceType(subTilePos);

            foreach (SurfaceType.TransportType transport in MapTransportTypeArray)
            {                
                SetSubtileCost(subTilePos, transport, surface.Cost(transport, SurfaceType.TerrainFeatures.None));
            }

        }


        public SurfaceType GetSurfaceType(Point subtilePos)
        {
            return GetTerrain(subtilePos).SurfaceType;
            
        }

        /// <summary>
        /// returns 0 - 1
        /// </summary>
        /// <param name="subtilePos"></param>
        /// <returns></returns>
        public float GetRoughness(Point subtilePos)
        {
            TerrainTile tile = GetTile(SubTileToTilePos(subtilePos));

            Terrain terrain = tile.GetTerrain(subtilePos);

            float surfaceRoughness = terrain.GetRoughness();

            float treeRoughness = 0f;
            if (tile.TreesOnTile != null)
            {
                // non-blocking trees impede movement (wingweed):
                if (tile.TreesOnTile.Any(t => MapManager.WorldPosToSubtile(t.PlaySiteLocation) == subtilePos))
                {
                    treeRoughness = 0.9f;
                }
            }

            return Common.ClampTop(surfaceRoughness + treeRoughness, 1f);
        }

        public Terrain GetTerrain(Point subtilePos)
        {
            TerrainTile tile = GetTile(SubTileToTilePos(subtilePos));

            return tile.GetTerrain(subtilePos);

        }

        /*
        public void ResetEdgeCost(Point mapPosition, Common.Direction edge)
        {
            for (int transport = 0; transport < TransportIndices.Length; transport++)
            {
                SetEdgeCost(mapPosition, edge, PlainsType.Instance.Cost((TerrainType.TransportType)transport, TerrainType.TerrainFeatures.None));
            }

        }*/

        public enum BlockingAction { Block, Unblock }
        /// <summary>
        /// unblock assigns the Plain terrain cost value for that transport!
        /// STERAIN: delete this
        /// </summary>
        /// <param name="blockingAction"></param>
        /// <param name="mapPosition"></param>
        /// <param name="dir"></param>
        /// <param name="transport"></param>
        /*  public void BlockEdgeSafely(BlockingAction blockingAction, Point mapPosition, Common.Direction dir, int transport)
          {
              // TODO: if blocking horiz or vert edges, block the diagonal edges in a diamond shape too!!! to avoid trapping agents.
              if (dir == Common.Direction.East)
              {
                  BlockJustThisEdge(blockingAction, mapPosition, Common.Direction.East, transport);
                  BlockJustThisEdge(blockingAction, mapPosition, Common.Direction.NorthEast, transport);
                  BlockJustThisEdge(blockingAction, mapPosition, Common.Direction.SouthEast, transport);

                  Point nextTile = ClampTileMapPosition(new Point(mapPosition.X + 1, mapPosition.Y));
                  if (nextTile != mapPosition)
                  {
                      BlockJustThisEdge(blockingAction, nextTile, Common.Direction.West, transport);
                      BlockJustThisEdge(blockingAction, nextTile, Common.Direction.NorthWest, transport);
                      BlockJustThisEdge(blockingAction, nextTile, Common.Direction.SouthWest, transport);
                  }
              }
              else if (dir == Common.Direction.West)
              {
                  BlockJustThisEdge(blockingAction, mapPosition, Common.Direction.West, transport);
                  BlockJustThisEdge(blockingAction, mapPosition, Common.Direction.NorthWest, transport);
                  BlockJustThisEdge(blockingAction, mapPosition, Common.Direction.SouthWest, transport);

                  Point nextTile = ClampTileMapPosition(new Point(mapPosition.X - 1, mapPosition.Y));
                  if (nextTile != mapPosition)
                  {
                      BlockJustThisEdge(blockingAction, nextTile, Common.Direction.East, transport);
                      BlockJustThisEdge(blockingAction, nextTile, Common.Direction.NorthEast, transport);
                      BlockJustThisEdge(blockingAction, nextTile, Common.Direction.SouthEast, transport);
                  }
              }
              else if (dir == Common.Direction.North)
              {
                  BlockJustThisEdge(blockingAction, mapPosition, Common.Direction.North, transport);
                  BlockJustThisEdge(blockingAction, mapPosition, Common.Direction.NorthWest, transport);
                  BlockJustThisEdge(blockingAction, mapPosition, Common.Direction.NorthEast, transport);

                  Point nextTile = ClampTileMapPosition(new Point(mapPosition.X, mapPosition.Y - 1));
                  if (nextTile != mapPosition)
                  {
                      BlockJustThisEdge(blockingAction, nextTile, Common.Direction.South, transport);
                      BlockJustThisEdge(blockingAction, nextTile, Common.Direction.SouthWest, transport);
                      BlockJustThisEdge(blockingAction, nextTile, Common.Direction.SouthEast, transport);
                  }
              }
              else if (dir == Common.Direction.South)
              {
                  BlockJustThisEdge(blockingAction, mapPosition, Common.Direction.South, transport);
                  BlockJustThisEdge(blockingAction, mapPosition, Common.Direction.SouthWest, transport);
                  BlockJustThisEdge(blockingAction, mapPosition, Common.Direction.SouthEast, transport);

                  Point nextTile = ClampTileMapPosition(new Point(mapPosition.X, mapPosition.Y + 1));
                  if (nextTile != mapPosition)
                  {
                      BlockJustThisEdge(blockingAction, nextTile, Common.Direction.North, transport);
                      BlockJustThisEdge(blockingAction, nextTile, Common.Direction.NorthWest, transport);
                      BlockJustThisEdge(blockingAction, nextTile, Common.Direction.NorthEast, transport);
                  }
              }
              else
              {   // diagonals... no extra edges are touched.
                  BlockJustThisEdge(blockingAction, mapPosition, dir, transport);
              }

          }
          */
        /* STERAIN: delete this
        private void BlockJustThisEdge(BlockingAction blockingAction, Point mapPosition, Common.Direction dir, int transport)
        {
            //we are blocking. propagate the change - block all discomfort maps too.
            if (blockingAction == BlockingAction.Block)
            {
                foreach (MovementMap moveMap in MovementMaps)
                { // propagate the change:
                    if (transport <= moveMap.Map.GetLength(0))
                    {
                        moveMap.Map[transport, mapPosition.X, mapPosition.Y, (int)dir] = 0;

                    }
                }

                // apply the new cost:
                TerrainCosts[transport, mapPosition.X, mapPosition.Y, (int)dir] = 0;
            }
            else
            {
                // remove the blocking - to what value?  what about roads!!!!
                TerrainCosts[transport, mapPosition.X, mapPosition.Y, (int)dir] = PlainsType.Instance.Cost((TerrainType.TransportType)transport, TerrainType.TerrainFeatures.None);
            }
        }*/

       

        public void SetSubtileCost(Point subtilePosition, SurfaceType.TransportType transport, byte newCost)
        {
            if (subtilePosition.X < 0)
                subtilePosition.X = 0;
            if (subtilePosition.Y < 0)
                subtilePosition.Y = 0;

            // if we are blocking, we can update the MovementMaps right away.
            // but if we are unblocking we can't do that since the movementmap is a sum of terrain cost and various discomfort values.
            // The movement maps have to be re-generated before the change takes effect (1 second or so?).       
            if (newCost == 0)
            {
                // BlockEdgeSafely(BlockingAction.Block, mapPosition, dir, transport);
                BlockTerrainSubtile(subtilePosition, transport, BlockingAction.Block);
            }
            else
            {

                // not blocking.
                byte previousCost;
                int edgeValueDifference;

                previousCost = GetCost(TerrainCosts[transport].GetValue(subtilePosition));

                if (previousCost > 0) // previously, the edge was not blocked. 
                {    // optimization...? This optimization is not that important, I feel...

                    // #SECTORS: I removed this optimization, it only sets a value from unblocked to unblocked, so is not so important...
                    // compute difference
                  /*  edgeValueDifference = newCost - previousEdgeCost;
                    

                    SubtileLayers map = null;
                    SubtileValue currentValue;
                    byte costToSet;
                    foreach (var moveMap in AllMovementMaps) // MovementMaps)
                    { // propagate the change:
                        if (moveMap.Layers.TryGetValue(transport, out map))
                        {
                            currentValue = map.Values[subtilePosition.X][subtilePosition.Y];// #SECTORS TODO - modify holes layer?
                            // clamp new cost to 1 if the cost is decreasing:
                            costToSet = Common.ClampBottom((byte)(GetCost(currentValue) + edgeValueDifference), (byte)1);

                            map.Values[subtilePosition.X][subtilePosition.Y] = SetCost(currentValue, costToSet);                                

                        }
                    }*/

                }
                else
                {
                    //  BlockEdgeSafely(BlockingAction.Unblock, mapPosition, dir, transport);
                    BlockTerrainSubtile(subtilePosition, transport, BlockingAction.Unblock);
                    
                    // (don't do the movemap optimization, it is getting too complicated.)
                }

                // apply the new cost:
                SubtileValue newValue = SetCost(TerrainCosts[transport].GetValue(subtilePosition.X, subtilePosition.Y), newCost);
                TerrainCosts[transport].SetValueOnBottomLayer(subtilePosition.X, subtilePosition.Y, newValue);

            }

            PropagateChangeToDependentMaps(subtilePosition, transport, newCost == 0);

        }

        public void SetSubtileCost(/*Point mapPosition, Common.Direction dir,*/ Vector3 location, /*int*/ SurfaceType.TransportType transport, byte newCost)
        {
            Point mapPosition = WorldPosToSubtile(location);
            SetSubtileCost(mapPosition, transport, newCost);


            /* if ((int)dir > 3)
             {   // diagonals - modify cost:                
                 newCost = (byte)(MapManager.DiagonalFactor * newCost);
             }*/


        }


        /// <summary>
        /// this wil safely block/unblock the extra diagonal edges that navigation requires.
        /// </summary>
        /// <param name="mapPosition"></param>
        /// <param name="dir"></param>
        /// <param name="transport"></param>
        /// <param name="newCost"></param>
        /*  public void SetEdgeCost(Point mapPosition, Common.Direction dir, int transport, byte newCost)
          {            
              // STERAIN: only need this:
              if ((int)dir > 3)
              {   // diagonals - modify cost:                
                  newCost = (byte)(MapManager.DiagonalFactor * newCost);
              }

              // if we are blocking, we can update the MovementMaps right away.
              // but if we are unblocking we can't do that since the movementmap is a sum of terrain cost and various discomfort values.
              // The movement maps have to be re-generated before the change takes effect (1 second or so?).       
              if (newCost == 0)
              {
                  BlockEdgeSafely(BlockingAction.Block, mapPosition, dir, transport);
              }
              else 
              {
                  // not blocking.
                  byte previousEdgeCost;
                  int edgeValueDifference;
                                
                  previousEdgeCost = TerrainCosts[transport, mapPosition.X, mapPosition.Y, (int)dir];

                  if (previousEdgeCost > 0) // previously, the edge was not blocked. 
                  {    // optimization...? This optimization is not that important, I feel...

                      // compute difference
                      edgeValueDifference = newCost - previousEdgeCost;

                      foreach (MovementMap moveMap in MovementMaps)
                      { // propagate the change:
                          if (transport <= moveMap.Map.GetLength(0))
                          {
                              // clamp new cost to 1 if the cost is decreasing:
                              moveMap.Map[transport, mapPosition.X, mapPosition.Y, (int)dir] =
                                  Common.ClampBottom((byte)(moveMap.Map[transport, mapPosition.X, mapPosition.Y, (int)dir] + edgeValueDifference), (byte)1);

                          }
                      }
                  }
                  else
                  {   // previously blocked, now unblocking. Revert all the "special" edges, they get set to the neutral terrain cost...
                      BlockEdgeSafely(BlockingAction.Unblock, mapPosition, dir, transport);

                      // (don't do the movemap optimization, it is getting too complicated.)
                  }

                  // apply the new cost:
                  TerrainCosts[transport, mapPosition.X, mapPosition.Y, (int)dir] = newCost;        

              }

          }     
          */


        public int GetDirectionIndex(Point from, Point to)
        {
            int dx = to.X - from.X;
            int dy = to.Y - from.Y;

            for (int i = 0; i < 8; i++)
            {
                if (direction[i, 0] == dx && direction[i, 1] == dy)
                {
                    return i;
                }
            }

            return -1;

        }

        // STERAIN: Don't need these anymore...
        public static bool DetectIsAtCornerOfTile(float xRelative, float yRelative)
        {
            if (xRelative + yRelative < GameData.Instance.AIConstants.DistanceToConsiderOnRoad)
            {// top left corner
                return true;
            }

            if (tileSize - xRelative + yRelative < GameData.Instance.AIConstants.DistanceToConsiderOnRoad)
            { // top right corner
                return true;
            }

            if (xRelative + (tileSize - yRelative) < GameData.Instance.AIConstants.DistanceToConsiderOnRoad)
            {
                // bottom left
                return true;
            }

            if ((tileSize - xRelative) + (tileSize - yRelative) < GameData.Instance.AIConstants.DistanceToConsiderOnRoad)
            {
                return true;
            }

            return false;
        }


        public static bool DetectIsAtCenterOfTile(float xRelative, float yRelative)
        {

            if (Math.Abs(xRelative - tileSizeOver2) < 4f && Math.Abs(yRelative - tileSizeOver2) < 4f)
            {
                return true;
            }

            return false;
        }
        public static bool DetectIsAtCenterOfTile(Vector3 pos)
        {
            float xRelative = pos.X % tileSize;
            float yRelative = pos.Y % tileSize;

            if (Math.Abs(xRelative - tileSizeOver2) < 4f && Math.Abs(yRelative - tileSizeOver2) < 4f)
            {
                return true;
            }
            return false;
        }

        /*   public Common.Direction WorldLocationToDirectionWithinTile(Vector3 location)
           {
               Vector2 intraTilePos = new Vector2((int)location.X % tileSize, (int)location.Y % tileSize);
            
           }*/

        public Common.Direction WorldLocationToDirectionWithinTile(Vector3 location)
        {
            Vector2 vectorFromCenter = WorldPosToPositionWithinTile(location);

            if (Math.Abs(vectorFromCenter.X - tileSizeOver2) < 2f && Math.Abs(vectorFromCenter.Y - tileSizeOver2) < 2f) //DetectIsAtCenterOfTile(vectorFromCenter.X, vectorFromCenter.Y))
            {
                return (Common.Direction)The.Sim.GameplayRandomGenerator.Next(8, "MapManager");
            }
            else
            {
                float bestScore = 0f;
                float score;
                Common.Direction bestDir = Common.Direction.North;
                for (int i = 0; i < 8; i++)
                {
                    score = Vector2.Dot(normalizedEightDirsAsVectors[i], vectorFromCenter);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestDir = (Common.Direction)i;
                    }
                }
                return bestDir;
            }
        }


        /// <summary>
        /// Returns true if the starting point and direction are along one of the 8 directions.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public bool DetectMovementAlong8Dir(Vector3 location, Vector2 vectorToWaypoint, ref Common.Direction direction)
        {
            //STERAIN: keep this to form footpaths.

            /*
             - Find the point on the line closest to the point. In 2D that's quite easy; the distance along the line is 
                d = (p.v) - (A.v)
                ... where p is the point, A is a reference point on the line (probably one end), and v is the (normalised) direction of the line.

                If d <= 0, your point is off the beginning, and the closest point is A.
                If d > (distance from A to B), the point is off the end, and the closest point is B.
                Otherwise, the point is on the line segment, at A+d(B-A).                             
             */

            //   float xRelative = from.X % tileWidth;
            //   float yRelative = from.Y % tileHeight;
            Vector2 from;
            from.X = location.X % (float)tileSize;
            from.Y = location.Y % (float)tileSize;

            // the tile center:
            Vector2 A = new Vector2(tileSizeOver2, tileSizeOver2);

            Vector2 closestPointOnLine;
            // Find out if the From point is on a 8-dir line:

            if (DetectIsAtCenterOfTile(from.X, from.Y))
            {   // if we are at the center, displace a certain amount along movement dir so we don't touch more than one line.
                from += vectorToWaypoint * tileSizeOver4; // 5f;
            }
            else if (DetectIsAtCornerOfTile(from.X, from.Y))
            {
                // if we are at the corner, displace a certain amount along movement dir so we get into a proper tile.
                from += vectorToWaypoint * tileSizeOver4;
                // recompute if we end up in a new tile:
                from.X = from.X % (float)tileSize;
                from.Y = from.Y % (float)tileSize;
            }

            float distanceAlongLine;
            Vector2 directionAsVector;
            // allow 3% deviance from 8 dir:
            float allowedDirectionsDeviance = 0.1f;
            for (int i = 0; i < 8; i++)
            {
                directionAsVector = normalizedEightDirsAsVectors[i];
                distanceAlongLine = Vector2.Dot(from, directionAsVector) - Vector2.Dot(A, directionAsVector);

                /* if (distanceAlongLine <= 0f)
                 {
                 }
                 else */
                if (distanceAlongLine >= 0 && distanceAlongLine <= (i < 4 ? abCartesianLength : abDiagonalLength))
                {
                    // we are perpendicular to the line.
                    closestPointOnLine = A + directionAsVector * distanceAlongLine;

                    if (Vector2.DistanceSquared(from, closestPointOnLine) < GameData.Instance.AIConstants.DistanceSquaredToConsiderOnRoad)
                    { // we are on the line, at least within 2? pixels from it.

                        // see if the movement direction is along the line, within some tolerance:
                        // test both directions:
                        if ((Math.Abs(vectorToWaypoint.X - normalizedEightDirsAsVectors[i].X) < allowedDirectionsDeviance &&
                            Math.Abs(vectorToWaypoint.Y - normalizedEightDirsAsVectors[i].Y) < allowedDirectionsDeviance)
                            ||
                            (Math.Abs(vectorToWaypoint.X + normalizedEightDirsAsVectors[i].X) < allowedDirectionsDeviance &&
                            Math.Abs(vectorToWaypoint.Y + normalizedEightDirsAsVectors[i].Y) < allowedDirectionsDeviance))
                        {
                            direction = (Common.Direction)i;
                            return true;
                        }
                    }

                }
                /*   else
                   {
                   }*/
            }

            return false;

        }

        /// <summary>
        /// get the cost of passing from one tile to the next via the two adjoining edge parts over the TERRAIN map - not including discomfort. If one edge part is blocked (=0) the returned cost is 0.
        /// </summary>
        /// <param name="transport"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        /*   public byte GetTerrainCost(TerrainType.TransportType transport, Point from, Point to)
           {            
               int i = GetDirectionIndex(from, to);        

               byte costFrom = TerrainCosts[(int)transport, from.X, from.Y, i];
               byte costTo = TerrainCosts[(int)transport, to.X, to.Y, oppositeDirection[i]];
            
               if (costFrom == 0 || costTo == 0)
               {
                   return 0;
               }
               else
               {
                   return (byte)(costFrom + costTo);
               }
           }*/

        /// <summary>
        /// see if the to points are connected over the supplied MovementMap, and that 'to' is inside the world.
        /// </summary>
        /// <param name="moveMap"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
     /*   public bool LocationIsAccessible(MovementMap moveMap, Vector3 from, Vector3 to) // Point from, Point to)
        {
            if (!TileIsOnMap(MapManager.WorldPosToTile(to)))
            {
                return false;
            }
            else
            {
                byte[][] map = moveMap.Map[TerrainType.TransportType.Foot];
                Entity structure = null; // = TileMap[to.X, to.Y].StructureOnTile;
                if (IsBaseCenterOfWorkingBuilding(MapManager.WorldPosToTile(to), ref structure)) // (structure != null && structure.BaseCenterTile == to)
                {
                    // if we are testing the path to the base center of a building, test the entrances:
                    // test all entrances:

                    if (The.Map.TileIsCompletelyBlocked(map, structure.TileLayout.Building.FrontDoorTilePos) ||
                        The.Map.IsPathBlocked(moveMap, TerrainType.TransportType.Foot, from, structure.TileLayout.Building.FrontDoorLocation)) //FrontDoorTilePos))
                    {
                        if (structure.TileLayout.Building.BackDoorTilePos.HasValue)
                        {
                            if (The.Map.TileIsCompletelyBlocked(map, structure.TileLayout.Building.BackDoorTilePos.Value) ||
                                The.Map.IsPathBlocked(moveMap, TerrainType.TransportType.Foot, from, structure.TileLayout.Building.BackDoorLocation.Value)) //BackDoorTilePos.Value))
                            {
                                return false;
                            }
                            else
                            {
                                return true;
                            }
                        }
                        return false;
                    }

                }
                else if (The.Map.IsPathBlocked(moveMap, TerrainType.TransportType.Foot, from, to))
                {
                    return false;
                }
                else if (The.Map.TileIsCompletelyBlocked(map, MapManager.WorldPosToTile(to)))
                {
                    return false;
                }
            }
            return true;
        }*/

        /// <summary>
        /// this version doesn't use the stored, blocked paths. It can  be used with the TerrainCosts map, however.
        /// </summary>
        /// <param name="map"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public bool TileIsAccessible(SubtileLayers map, Point from, Point to)
        {
            if (!TileIsOnMap(to))
            {
                return false;
            }
            /*  else if (UWGame.SimSide.Instance.map.IsPathBlocked(moveMap, TerrainType.TransportType.Foot, from, to))
              {
                  return false;
              }*/
            else if (The.Map.TileIsCompletelyBlocked(map, to))
            {
                return false;
            }
            return true;
        }

        public bool SubtileIsCompletelyBlocked(SubtileLayers /* SubtileValue[][]*/ mapCosts, Point p) 
        {            
             return IsBlocked(mapCosts.GetValue(p));
        }

        public bool SubtileIsCompletelyBlocked(SubtileLayers /*SubtileValue[][]*/ mapCosts, SubtilePos p) 
        {
            return IsBlocked(mapCosts.GetValue(p));
        }

        public bool SubtileIsOrAdjacentToBlockedSubtile(SubtileLayers mapCosts, Point p)
        {

            if (p.Y < 1 || p.X < 1)
                return true; // any point on the top or left or off the map in that direction is "blocked"

            return
                IsBlocked(mapCosts.GetValue(--p.X, p.Y - 1)) ||
                IsBlocked(mapCosts.GetValue(p.X, p.Y)) ||
                IsBlocked(mapCosts.GetValue(p.X, p.Y + 1)) ||

                IsBlocked(mapCosts.GetValue(++p.X, p.Y -1)) ||
                IsBlocked(mapCosts.GetValue(p.X, p.Y)) ||
                IsBlocked(mapCosts.GetValue(p.X, p.Y + 1)) ||

                IsBlocked(mapCosts.GetValue(++p.X, p.Y -1)) ||
                IsBlocked(mapCosts.GetValue(p.X, p.Y)) ||
                IsBlocked(mapCosts.GetValue(p.X, p.Y + 1));
        }



        public static bool IsBlocked(SubtileValue value)
        {
            /*if (value.HasValue)
            {*/
                return (value & SubtileValue.Cost) == SubtileValue.Blocked;
           /* }

            return true;*/
        }

        /// <summary>
        /// returns the cost part of the bit field
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte GetCost(SubtileValue value)
        {
            return (byte)(value & SubtileValue.Cost);
        }


        public static string TilePosToString(Point tile)
        {
            return "[" + tile.X + "," + tile.Y + "]";
        }

        /// <summary>
        /// sets only the cost part of the terrain value, leaving the other flag bits as they are
        /// Cost = 0: blocked
        /// </summary>
        /// <param name="value"></param>
        /// <param name="cost"></param>
        public static SubtileValue SetCost(SubtileValue value, byte cost)
        {
            value = (value & ~(SubtileValue.Cost)) | (SubtileValue)cost;

            return value;
        }

        /// <summary>
        /// Seems obsolete...
        /// 
        /// Examine all subtiles inside the tile. Return true if they are all blocked.
        /// It is possible to use either the Terrain costs or the Movement map (includes discomfort.)
        /// </summary>
        /// <param name="mapCosts"></param>
        /// <param name="transport"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool TileIsCompletelyBlocked(SubtileLayers /* MapManager.SubtileValue[][]*/ mapCosts, Point p) 
        {
            // ushort fromX;
            //  ushort fromY;

            Point subtile = MapManager.TileToUpperLeftSubtile(p);
            int subTileX = subtile.X; // p.X * 3;
            int subTileY = subtile.Y; // p.Y * 3;

            return
                IsBlocked(mapCosts.GetValue(subTileX, subTileY)) &&
                IsBlocked(mapCosts.GetValue(subTileX, subTileY + 1)) &&
                IsBlocked(mapCosts.GetValue(subTileX, subTileY + 2)) &&

                IsBlocked(mapCosts.GetValue(++subTileX, subTileY)) &&
                IsBlocked(mapCosts.GetValue(subTileX, subTileY + 1)) &&
                IsBlocked(mapCosts.GetValue(subTileX, subTileY + 2)) &&

                IsBlocked(mapCosts.GetValue(++subTileX, subTileY)) &&
                IsBlocked(mapCosts.GetValue(subTileX, subTileY + 1)) &&
                IsBlocked(mapCosts.GetValue(subTileX, subTileY + 2));

            //tileto

            /*  for (int i = 0; i < 8; i++)
              {
                  fromX = (ushort)(p.X + direction[i, 0]);
                  fromY = (ushort)(p.Y + direction[i, 1]);

                  if (fromX >= 0 && fromX < mapWidth && fromY >= 0 && fromY < mapHeight)
                  {   // check both edge parts:
                      if (mapCosts[(int)transport, fromX, fromY, oppositeDirection[i]] != 0
                          && mapCosts[(int)transport, p.X, p.Y, i] != 0)
                      {
                          return false;
                      }
                  }
              }

              return true;*/
        }

     

      
        /// <summary>
        /// circular area
        /// </summary>
        /// <param name="tilePosition"></param>
        /// <param name="radius"></param>
        /// <param name="minX"></param>
        /// <param name="maxX"></param>
        /// <param name="minY"></param>
        /// <param name="maxY"></param>
        /// <returns></returns>
        public static Rectangle GetClampedMapAreaUsingTiles(TilePos tilePosition, int radius, out int minX, out int maxX, out int minY, out int maxY)
        {
            minX = Math.Max(0, tilePosition.X - radius);
            minY = Math.Max(0, tilePosition.Y - radius);


            maxX = Math.Min(The.Map.mapTileWidth - 1, tilePosition.X + radius);
            maxY = Math.Min(The.Map.mapTileHeight - 1, tilePosition.Y + radius);

            return new Rectangle(minX, minY, maxX - minX, maxY - minY);
        }

        /// <summary>
        /// rect area
        /// </summary>
        /// <param name="tilePosition"></param>
        /// <param name="widthInTiles"></param>
        /// <param name="heightInTiles"></param>
        /// <returns></returns>
        public Rectangle GetClampedMapAreaUsingTiles(Point tilePosition, int widthInTiles, int heightInTiles)
        {
            int minX, maxX, minY, maxY;

            minX = Math.Max(0, tilePosition.X);
            minY = Math.Max(0, tilePosition.Y);


            maxX = Math.Min(mapTileWidth - 1, tilePosition.X + widthInTiles);
            maxY = Math.Min(mapTileHeight - 1, tilePosition.Y + heightInTiles);

            return new Rectangle(minX, minY, maxX - minX, maxY - minY);
        }

        public Rectangle GetClampedMapAreaUsingSubTiles(Point subtilePosition, int widthInSubTiles, int heightInSubTiles)
        {
            int minX, maxX, minY, maxY;

            minX = Math.Max(0, subtilePosition.X);
            minY = Math.Max(0, subtilePosition.Y);


            maxX = Math.Min(mapSubtileWidth - 1, subtilePosition.X + widthInSubTiles);
            maxY = Math.Min(mapSubtileHeight - 1, subtilePosition.Y + heightInSubTiles);

            return new Rectangle(minX, minY, maxX - minX, maxY - minY);
        }

     /*   public Rectangle GetClampedMapAreaUsingWorldCoords(Vector3 location, float width, float height)
        {
            float minX, maxX, minY, maxY;

            minX = Math.Max(0, location.X);
            minY = Math.Max(0, location.Y);

            maxX = Math.Min(MapWorldWidth, location.X + width);
            maxY = Math.Min(MapWorldHeight, location.Y + height);

            return new Rectangle(minX, minY, maxX - minX, maxY - minY);
        }*/

        public Rectangle GetClampedMapAreaUsingTiles(Rectangle area)
        {
            return GetClampedMapAreaUsingTiles(new Point(area.X, area.Y), area.Width, area.Height);
        }

        public static Rectangle GetClampedRectangularMapAreaUsingSubtiles(Point topLeftSubtilePosition, int widthInSubtiles, int heightInSubtiles, out int minX, out int maxX, out int minY, out int maxY)
        {
            minX = Math.Max(0, topLeftSubtilePosition.X);
            minY = Math.Max(0, topLeftSubtilePosition.Y);

            //  int width = Common.GetJaggedArrayWidth(UWGame.SimSide.Instance.Map.TileMap);
            //    int height = Common.GetJaggedArrayHeight(UWGame.SimSide.Instance.Map.TileMap);

            maxX = Math.Min(The.Map.mapSubtileWidth - 1, topLeftSubtilePosition.X + widthInSubtiles);
            maxY = Math.Min(The.Map.mapSubtileHeight - 1, topLeftSubtilePosition.Y + heightInSubtiles);

            return new Rectangle(minX, minY, maxX - minX, maxY - minY);
        }
        public static Rectangle GetClampedRectangularMapAreaUsingSubtiles(Point topLeftSubtilePosition, int widthInSubtiles, int heightInSubtiles)
        {
            int minX, maxX, minY, maxY;

            minX = Math.Max(0, topLeftSubtilePosition.X);
            minY = Math.Max(0, topLeftSubtilePosition.Y);

            //  int width = Common.GetJaggedArrayWidth(UWGame.SimSide.Instance.Map.TileMap);
            //    int height = Common.GetJaggedArrayHeight(UWGame.SimSide.Instance.Map.TileMap);

            maxX = Math.Min(The.Map.mapSubtileWidth - 1, topLeftSubtilePosition.X + widthInSubtiles);
            maxY = Math.Min(The.Map.mapSubtileHeight - 1, topLeftSubtilePosition.Y + heightInSubtiles);

            return new Rectangle(minX, minY, maxX - minX, maxY - minY);
        }

        


        

        

       /* public static Common.Direction GetDirectionFromSubTile(int subTileIndex)
        {
            return (Common.Direction)subtileIndexToDirectionMappings[subTileIndex];
        }*/

        public static Vector3 GetWorldCoordsFromDirection(Point mapPosition, Common.Direction dir)
        {
            Point dirCoords = DirectionToRelativeSubtile(dir);
            Vector3 worldPos = EdgeOfTileToWorldPos(mapPosition.X, mapPosition.Y);

            worldPos.X += (float)((float)dirCoords.X + 0.5) * subTileSize;
            worldPos.Y += (float)((float)dirCoords.Y + 0.5) * subTileSize;

            return worldPos;

        }

        public static Vector3 TileAndDirectionToWorldPos(Point tilePos, Common.Direction edge)
        {
            Vector3 worldPos = TileToWorldPos(tilePos);
            Point subTile = DirectionToRelativeSubtile(edge);

            worldPos.X += subTile.X * subTileSize + subTileSizeOver2;
            worldPos.Y += subTile.Y * subTileSize + subTileSizeOver2;

            return worldPos;
        }

        public static Point DirectionToRelativeSubtile(Common.Direction dir)
        {
            switch (dir)
            {
                case Common.Direction.East:
                    return new Point(2, 1);
                case Common.Direction.North:
                    return new Point(1, 0);
                case Common.Direction.West:
                    return new Point(0, 1);
                case Common.Direction.South:
                    return new Point(1, 2);

                case Common.Direction.NorthEast:
                    return new Point(2, 0);
                case Common.Direction.SouthEast:
                    return new Point(2, 2);
                case Common.Direction.SouthWest:
                    return new Point(0, 2);
                case Common.Direction.NorthWest:
                    return new Point(0, 0);
                                 
                default:
                    return new Point(1, 0);

            }
        }

        /*  public static Common.Direction GetDirectionFromSubtileCoords(Common.Direction dir)
          {
              switch (dir)
              {
                  case Common.Direction.East:
                      return new Point(2, 1);
                  case Common.Direction.North:
                      return new Point(1, 0);
                  case Common.Direction.West:
                      return new Point(0, 1);
                  case Common.Direction.South:
                      return new Point(1, 2);
                  case Common.Direction.NorthEast:
                      return new Point(2, 0);
                  case Common.Direction.SouthEast:
                      return new Point(2, 2);
                  case Common.Direction.SouthWest:
                      return new Point(0, 2);
                  case Common.Direction.NorthWest:
                      return new Point(0, 0);

              }
          }*/
        /*
        public static Vector2 SubtileIndexToTileCenterOffset(int subtileIndex)
        {
            return (tileSize / 3f) * subtileIndexToCenterOffsetMappings[subtileIndex];
        }
        */
        /// <summary>
        /// compute the tile and offset from the given subtile coordinates in a grid starting at the specified tile position.
        /// </summary>
        /// <param name="bestSubtilePoint"></param>
        /// <param name="relativeToTile"></param>
        /// <param name="foundPoint"></param>
        /// <param name="foundTileCenterOffset"></param>
        public static void SubtileAndTilePosToWorldPos(Point bestSubtilePoint, Point relativeToTile, out Vector3 foundLocation) // out Point foundPoint, out Vector2 foundTileCenterOffset)
        {
            foundLocation = SubTileToWorldPos3(bestSubtilePoint) + EdgeOfTileToWorldPosV3(relativeToTile);


            /*
            Point relativeTilePos = new Point(bestSubtilePoint.X / 3, bestSubtilePoint.Y / 3);
            foundPoint = Common.AddPoints(relativeTilePos, relativeToTile); 

            int subTileIndex = GetLocalSubtileIndex(bestSubtilePoint.X, bestSubtilePoint.Y);

            foundTileCenterOffset = MapManager.SubtileIndexToTileCenterOffset(subTileIndex);*/
        }

        public static Vector3 SubtileAndTilePosToWorldPos(Point bestSubtilePoint, Point relativeToTile) 
        {
            return SubTileToWorldPos3(bestSubtilePoint) + EdgeOfTileToWorldPosV3(relativeToTile);
        }

        public static Point TileCenterToSubTile(Point tile)
        {
            return new Point(tile.X * 3 + 1, tile.Y * 3 + 1);
        }

        public static SubtilePos TileCenterToSubTile(TilePos tile)
        {
            return new SubtilePos((ushort)(tile.X * 3 + 1), (ushort)(tile.Y * 3 + 1));
        }

        public static int GetLocalSubtileIndex(int subtileX, int subtileY)
        {
            return subtileX % 3 + 3 * (subtileY % 3);
        }

        public static Point WorldPosToRelativeSubtile(Vector3 location, Vector3 relativeTo)
        {
            return new Point((int)((location.X - relativeTo.X) / subTileSize),
                             (int)((location.Y - relativeTo.Y) / subTileSize));

        }

        public static Point WorldPosToRelativeSubtile(Vector3 location)
        {
            return new Point((int)((location.X) * oneOverSubtileSize) % 3,
                             (int)((location.Y) * oneOverSubtileSize) % 3);

        }

        public static Point RelativePosToRelativeSubtile(Vector2 location)
        {
            return new Point((int)((float)location.X / subTileSize),
                             (int)((float)location.Y / subTileSize));

        }


        public static List<Point> GetSubtilesTouchedByLine(Vector2 from, Vector2 to)
        {
            List<Point> list = new List<Point>();

            float tileWidth = MapManager.subTileSize;
            float tileHeight = MapManager.subTileSize;

            return GetSquaresTouchedByLine(ref from, ref to, list, tileWidth, tileHeight);
        }


        public static List<Point> GetTilesTouchedByLine(Vector2 from, Vector2 to)
        {
            List<Point> list = new List<Point>();

            float tileWidth = MapManager.tileSize;
            float tileHeight = MapManager.tileSize;

            return GetSquaresTouchedByLine(ref from, ref to, list, tileWidth, tileHeight);
        }

        private static List<Point> GetSquaresTouchedByLine(ref Vector2 from, ref Vector2 to, List<Point> list, float tileWidth, float tileHeight)
        {
            Vector2 v = (to - from);
            int stepX = Math.Sign(v.X); //to.X - from.X);
            int stepY = Math.Sign(v.Y); // to.Y - from.Y);
            v.Normalize();

            int x = (int)(from.X / tileWidth);
            int y = (int)(from.Y / tileHeight);

            // NEW: Add the tile we're starting in:
            list.Add(new Point(x, y));

            int endX = (int)(to.X / tileWidth);
            int endY = (int)(to.Y / tileHeight);

            float tMaxX, tMaxY;

            float tDeltaX, tDeltaY;
            /*
             TDeltaX indicates how far along the ray we must move
            (in units of t) for the horizontal component of such a movement to equal the width of a voxel.
             */
            tDeltaX = Math.Abs(tileWidth / v.X);
            tDeltaY = Math.Abs(tileHeight / v.Y);

            /*
             we determine the value of t at which the ray crosses the first vertical voxel boundary and
                store it in variable tMaxX. We perform a similar computation in y and store the result in tMaxY. The
                minimum of these two values will indicate how much we can travel along the ray and still remain in the
                current voxel */
            if (v.X < 0f)
            {
                tMaxX = Math.Abs((from.X % tileWidth) / v.X);
            }
            else
            {
                tMaxX = Math.Abs((tileWidth - (from.X % tileWidth)) / v.X);
            }

            if (v.Y < 0f)
            {
                tMaxY = Math.Abs((from.Y % tileHeight) / v.Y);
            }
            else
            {
                tMaxY = Math.Abs((tileHeight - (from.Y % tileHeight)) / v.Y);
            }
            /*
                        tMaxY = Math.Abs((tileHeight - (from.Y % tileHeight)) / v.Y);
            
                        tMaxX = Math.Abs((tileWidth - (from.X % tileWidth)) / v.X);
                        tMaxY = Math.Abs((tileHeight - (from.Y % tileHeight)) / v.Y);
                        */
            if (endX != x || endY != y)
            {

                do
                {
                    if (tMaxX < tMaxY)
                    {
                        tMaxX = tMaxX + tDeltaX;
                        x = x + stepX;
                        list.Add(new Point(x, y));
                    }
                    else if (tMaxX == tMaxY)
                    {
                        tMaxX = tMaxX + tDeltaX;
                        x = x + stepX;
                        tMaxY = tMaxY + tDeltaY;
                        y = y + stepY;
                        list.Add(new Point(x, y));
                    }
                    else
                    {
                        tMaxY = tMaxY + tDeltaY;
                        y = y + stepY;
                        list.Add(new Point(x, y));
                    }

                    if (list.Count() > 20000)
                    {
//                        throw new Exception("The path smoothing bug -- inner loop makes infinite points");
                        break;
                    }
                }
                while (!(x == endX && y == endY));
            }

            return list;
        }



        public static bool IsPathClearToPoint(Vector3 from, Vector3 to, MovementMap moveMap, SurfaceType.TransportType transport)
        {
            List<Point> tiles = MapManager.GetSubtilesTouchedByLine(from.ToVector2(), to.ToVector2());
            Point previousTile = tiles[0];

            SubtileLayers map = moveMap.Layers[transport];

            Point subtile;
            for (int i = 1; i < tiles.Count; i++)
            {
                subtile = tiles[i];
                // we don't test edges within the same tile:
                if (subtile != previousTile)
                {
                    if (MapManager.IsBlocked(map.GetValue(subtile)))
                        return false;                  
                }
            }

            return true;
        }

        public enum ScanMethod { Fan, HalfCircle /*, FullCircle*/ }


        /// <summary>
        /// for items, this will modify the location to find a free area
        /// </summary>
        /// <param name="location"></param>
        /// <param name="addRandomOffset"></param>
        /// <param name="creatorOfItem"></param>
        /// <returns></returns>
        public static Vector3 FindFreeLocation(Vector3 location, bool avoidBlockedAreas, Entity.AddRandomOffset addRandomOffset, Entity creatorOfItem)
        {
            if (addRandomOffset == Entity.AddRandomOffset.Yes)
            {
                location = MapManager.VaryLocationWithinSubtile(location);
            }

            // placement of items - avoid obstacles:
            if (avoidBlockedAreas)
            {
                if (addRandomOffset == Entity.AddRandomOffset.No)
                {
                    if (creatorOfItem != null)
                    {
                        if (creatorOfItem.Location == location)// if the item location is the same as the creators we want to move it
                        {
                            float angle = (float)(MathHelper.PiOver2 + creatorOfItem.Rotation);// try to put the items beside the creator
                            Vector3 creatorDirection = Common.AngleToVector(angle).ToVector3();
                            location += creatorDirection * 30f;
                        }
                    }
                }

                location = The.Map.ClampWorldPosition(location); 


                Point subTile = MapManager.WorldPosToSubtile(location);
                SubtileLayers map = The.Map.TerrainCosts[SurfaceType.TransportType.Foot];

                // when outputting entities as a by product from a structure, the structure footprint does not exist yet. So avoid placing on the reserved subtiles where it will appear the next frame.
                if (The.Map.SubtileIsCompletelyBlocked(map, subTile)
                    || The.Map.FlagIsSet(subTile, SurfaceType.TransportType.Foot, MapManager.SubtileValue.Reserved)
                    /*|| The.Map.FlagIsSet(subTile, SurfaceType.TransportType.Foot, MapManager.TerrainValue.Pad)*/) // OK to place items on pad I think
                {
                    if (creatorOfItem != null)
                    {
                        location = MapManager.FindUnblockedLocation(location, 50f, MapManager.ScanMethod.HalfCircle, creatorOfItem.Location, true);//Try to find a location for the items close to the creator
                    }
                    else
                    {
                        location = MapManager.FindUnblockedLocation(location, 50f, MapManager.ScanMethod.Fan, null, true);//Try to find an unblocked location
                    }
                }
            }

            return location;
        }

        public RegionMap FootTerrainRegionMap
        {
            get
            {
                return TerrainCosts[SurfaceType.TransportType.Foot].RegionMap;
            }
        }
       
        /// <summary>
        /// this is used to compute an access point for non-moving entities without an exit, and for placing spawned items
        /// </summary>
        public static Vector3 FindUnblockedLocation(Vector3 locationToComputeFrom, float maxRadius, ScanMethod scanMethod = ScanMethod.Fan,
            Vector3? locationToStriveTowards = null, bool avoidReservedSubtiles = false, Predicate<SubtilePos> subtileIsValid = null, float? radiusIncrements = null)
        {

            Vector3 directionToLookForUnblockedLocationIn;
            if (locationToStriveTowards == null)
            {
                directionToLookForUnblockedLocationIn = new Vector3(0f, 1f, 0f); // south (towards player)
            }
            else
            {
                directionToLookForUnblockedLocationIn = locationToStriveTowards.Value - locationToComputeFrom;
                directionToLookForUnblockedLocationIn.Normalize();
            }

            SubtileLayers terrain = The.Map.TerrainCosts[SurfaceType.TransportType.Foot];

            // float minDistance = 1000f;
            // float? currentDistance = null; // Removed; unneeded

            Vector3? foundPoint = null;
            foundPoint = ScanDirectionFromCenter(directionToLookForUnblockedLocationIn, terrain, locationToComputeFrom, avoidReservedSubtiles, maxRadius /* 50f*/, subtileIsValid);

            if (foundPoint == null)
            {
                if (scanMethod == ScanMethod.Fan)
                {
                    FanScan(ref locationToComputeFrom, ref directionToLookForUnblockedLocationIn, terrain, ref foundPoint, avoidReservedSubtiles, maxRadius, subtileIsValid, radiusIncrements);
                }
                else if (scanMethod == ScanMethod.HalfCircle)
                {                   
                    HalfCircleScan(ref locationToComputeFrom, ref directionToLookForUnblockedLocationIn, terrain, ref foundPoint, avoidReservedSubtiles, maxRadius, subtileIsValid);
                }
                else
                { 
                    // TODO: add full circle scan option (2 half circle scans
                }
            }

            if (foundPoint.HasValue)
            {
                return foundPoint.Value;
            }
            else
            {
                return locationToComputeFrom; // location; // probably inaccessible.
            }
        }

        private static void HalfCircleScan(ref Vector3 locationToComputeFrom, ref Vector3 directionToLookForUnblockedLocationIn, SubtileLayers terrain, ref Vector3? foundPoint,
            bool avoidReservedSubtiles, float maxRadius, Predicate<SubtilePos> subtileIsValid = null)
        {
            Vector3 originalDirection = directionToLookForUnblockedLocationIn;
            float fourthOfACircleInRadians = (float)(Math.PI / 2);
            int numberOfSteps = 5;
            float radianStep = fourthOfACircleInRadians / (float)numberOfSteps;
            float originalRotation = (float)Math.Acos((double)originalDirection.X);
            float currentRadianDifference = 0.0f;
            int numberOfStepsTaken = 0;

            do
            {
                currentRadianDifference += radianStep;

                
                ScanWithRotation(ref locationToComputeFrom, ref directionToLookForUnblockedLocationIn, terrain, ref foundPoint, originalRotation, currentRadianDifference, avoidReservedSubtiles, 
                    maxRadius, subtileIsValid);

                if (foundPoint != null)
                {
                    break;
                }

                // toggle angle sign:
                ScanWithRotation(ref locationToComputeFrom, ref directionToLookForUnblockedLocationIn, terrain, ref foundPoint, originalRotation, -currentRadianDifference, avoidReservedSubtiles, maxRadius, 
                    subtileIsValid);

                if (foundPoint != null)
                {
                    break;
                }

                numberOfStepsTaken++;
            }
            while (numberOfStepsTaken < numberOfSteps);

            //we found no unblocked point
        }

        private static void ScanWithRotation(ref Vector3 locationToComputeFrom, ref Vector3 directionToLookForUnblockedLocationIn, SubtileLayers terrain, ref Vector3? foundPoint, float originalRotation,
            float currentRadianDifference, bool avoidReservedSubtiles, float maxRadius, Predicate<SubtilePos> subtileIsValid = null)
        {
            directionToLookForUnblockedLocationIn = Common.AngleToVector(originalRotation + currentRadianDifference).ToVector3();

            foundPoint = ScanDirectionFromCenter(directionToLookForUnblockedLocationIn, terrain, locationToComputeFrom, avoidReservedSubtiles, maxRadius, subtileIsValid);
        }

        private static void FanScan(ref Vector3 locationToComputeFrom, ref Vector3 directionToLookForUnblockedLocationIn, SubtileLayers terrain, ref Vector3? foundPoint,
            bool avoidReservedSubtiles, float maxRadius, Predicate<SubtilePos> subtileIsValid = null, 
            float? radiusIncrements = null) // if filled, will scan the full diameter first at small, then larger radius afterwards
        {
           
            
            float radiusToUse = 0f;
                       

            do
            {
                // scan in a fan out from the south direction to east/west, alternating left and right until a free subtile is found.
                float x = 0.1f;
                float y = 1f;

                float xSign = 1f;
                float xValue = 0f;

                if (radiusIncrements.HasValue)
                {
                    radiusToUse += radiusIncrements.Value;
                }
                else
                {
                    radiusToUse = maxRadius;
                }

                while (foundPoint == null)
                {
                    if (xValue > 1f)
                    {
                        if (y <= 0f)
                        {
                            // no success.
                            break;
                        }
                        else
                        {
                            y -= 0.1f;

                            if (xSign > 0f)
                            {
                                xSign = -1f; // switch between left and right
                            }
                            else
                            {
                                xSign = 1f;
                            }
                        }
                    }
                    else
                    {
                        xValue += 0.1f;

                        if (xSign > 0f)
                        {
                            xSign = -1f; // switch between left and right
                        }
                        else
                        {
                            xSign = 1f;
                        }
                    }

                    directionToLookForUnblockedLocationIn = new Vector3(xValue * xSign, y, 0f);

                    foundPoint = ScanDirectionFromCenter(directionToLookForUnblockedLocationIn, terrain, locationToComputeFrom, avoidReservedSubtiles, radiusToUse, subtileIsValid);

                }
            }
            while (radiusToUse < maxRadius);
        }

        /// <summary>
        /// returns the first non-blocked subtile in the given direction that we come across. 
        /// </summary>
        /// <param name="directionFromBaseCenter"></param>
        /// <param name="terrain"></param>
        /// <param name="distance"></param>
        /// <param name="locationToScanFrom"></param>
        /// <returns></returns>
        private static Vector3? ScanDirectionFromCenter(Vector3 directionFromBaseCenter, SubtileLayers terrain, Vector3 locationToScanFrom, bool avoidReservedSubtiles, float maxDistance,
            Predicate<SubtilePos> subtileIsValid = null)
        {
            float dotGap = MapManager.subTileSizeOver2; // 0.1f;
            SubtilePos subtile;
            // distance = null;

            WorldLocation scanLocation;

            for (float incr = 0; incr < maxDistance; incr += dotGap)
            {
                scanLocation = new WorldLocation(locationToScanFrom + directionFromBaseCenter * incr);

                if (The.Map.ClampWorldPosition(scanLocation) != scanLocation)
                {
                    // we are off map... try another direction
                    return null;
                }

                subtile = WorldPosToSubtilePos(scanLocation);

                SubtileValue terrainValue = terrain.GetValue(subtile);

                if (!MapManager.IsBlocked(terrainValue)
                    && (!avoidReservedSubtiles || !MapManager.TestForFlag(terrainValue, MapManager.SubtileValue.Reserved))
                    && (subtileIsValid == null || subtileIsValid(subtile)))
                {
                    // distance = (scanLocation - location).Length();
                    return scanLocation.ToVector3();
                }
            }

            return null;
        }

        private bool TerrainDepthIsUnderWaterLevel(float terrainDepth, float waterLevelBelowTerrain)
        {
            return terrainDepth - waterLevelBelowTerrain > 0f;
        }

        /// <summary>
        /// update tile map based on current water level and depth map data
        /// </summary>
      /*  private void UpdateTileMapWithWaterLevel()
        {
            TerrainTile tileToDraw;

            // how far below terrain is the water level?
            float waterLevelBelowTerrain = Renderer.Water.WaterHeight - TerrainZLevel;

            // how deep are we under current water height?
            float vertexDepthUnderWater;
            // how deep are we under 'ground level'?
            float vertexTerrainDepth;

            Terrain terrain;

            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    tileToDraw = TileMap[x][y];

                    if (tileToDraw.Terrain != null)
                    {
                        terrain = tileToDraw.Terrain;
                    }
                    else
                    {
                        for (int sx = 0; sx < 3; sx++)
                        {
                            for (int sy = 0; sy < 3; sy++)
                            {
                                

                            }
                        }

                    }

                    vertexTerrainDepth = terrain.TerrainDepth;

                    SetTerrainCostAccordingToWaterLevel(tileToDraw, waterLevelBelowTerrain, vertexTerrainDepth, terrain, y, x);

                    SetMoistureOnTile(terrain);
                }
            }
        }*/

      /*  private void SetTerrainCostAccordingToWaterLevel(TerrainTile tileToDraw, float waterLevelBelowTerrain, float vertexTerrainDepth, Terrain terrain, int y, int x)
        {
            float vertexDepthUnderWater;
            // don't clamp. We need negative water levels for the water shader
            vertexDepthUnderWater = vertexTerrainDepth - waterLevelBelowTerrain;

            if (vertexDepthUnderWater > 0f)
            {
                if (!tileToDraw.IsUnderWater())
                {
                    // tile is now under water.

                    // block movement:
                    SetTileCost(x, y, 0);
                }
            }
            else if (tileToDraw.IsUnderWater())
            {
                // tile is above water again

                tileToDraw.TerrainType = PlainsType.Instance;

                foreach (TerrainType.TransportType transport in MapTransportTypeArray)
                {
                    SetTileCost(x, y, transport, PlainsType.Instance.Cost(transport, TerrainType.TerrainFeatures.None));
                }
            }

            terrain.LevelBelowWater = vertexDepthUnderWater;
           
        }*/

        /// <summary>
        /// we want higher resolution near the water edge...
        /// </summary>
        /// <param name="tile"></param>
    /*    private void SplitMapInSubtiles()
        {
            TerrainTile tile;
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    tile = TileMap[x][y];

                    if (tile.LevelBelowWater > 0f)
                    {
                        Point from = ClampTileMapPosition(new Point(x - 1, y - 1));
                        Point to = ClampTileMapPosition(new Point(x + 1, y + 1));

                        for (int fx = from.X; fx <= to.X; fx++)
                        {
                            

                        }
                    }
                }
            }

        }*/

        


      

       

        

        /*  public bool IsBaseCenter(Point tile)
          {
              Entity structure = TileMap[tile.X, tile.Y].TiledEntityOnTile;
              if (structure != null && structure.MapPosition == tile)
              {                
                  return true;
              }
              else
              {
                  return false;
              }
          }*/

        public bool IsBaseCenterOfWorkingBuilding(Point pos, ref Entity structure) //, ref Point frontDoorTile, ref Point? backDoorTile)
        {
          /*  TerrainTile tile = TileMap[pos.X][pos.Y];
            if (tile.TiledEntityOnTile != null)
            {
                foreach (Entity entity in tile.TiledEntityOnTile)
                {
                    if (entity.TileLayout.Building != null &&
                        entity.IsCompleted() && 
                        entity.MapPosition == pos)
                    {
                        structure = entity;
                        return true;
                    }
                }
            }*/

            return false;

        }


        public bool SubtileContainsEntities(Point subtile, Predicate<Entity> countEntity) // EntityID? exceptEntity)
        {
            Point tilePos = SubTileToTilePos(subtile);

            TerrainTile tile = GetTile(tilePos);

           /* if (tile.RememberedRootEntitiesOnTile != null) // ??
            {


            }*/

            if (tile.EntitiesOnTile != null)
            {
                foreach (var item in tile.EntitiesOnTile)
                {
                    if ((countEntity == null || countEntity(item)) //item.ID != exceptEntity
                        && WorldPosToSubtile(item.PlaySiteLocation) == subtile)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
       

        public void ClearTile(TerrainTile selTile)
        {
            // destroy everything you touch
            //TerrainTile selTile = Interface.Interface.Instance.SelectedTile;
            if (selTile != null)
            {
               /* if (selTile.TiledEntityOnTile != null)
                {
                    for (int i = selTile.TiledEntityOnTile.Count - 1; i >= 0; i--)
                    {
                        selTile.TiledEntityOnTile[i].Destroy();
                    }                
                }*/

               
                if (selTile.EdgeLayoutEntities != null)
                {
                    for (int i = 0; i < selTile.EdgeLayoutEntities.Count; i++)
                    {
                        selTile.EdgeLayoutEntities[i].Destroy();
                        //selTile.RemoveEdgeStructure(selTile.EdgeLayoutEntities[i]);
                        i--;
                    }
                }

                if (selTile.EntitiesOnTile != null)
                {
                    for (int i = 0; i < selTile.EntitiesOnTile.Count; i++)
                    {
                        selTile.EntitiesOnTile[i].Destroy();
                      //  selTile.RemoveEntity(selTile.EntitiesOnTile[i]);
                        i--;
                    }
                }

                if (selTile.TreesOnTile != null)
                {
                    for (int i = 0; i < selTile.TreesOnTile.Count; i++)
                    {
                        Entity tree = selTile.TreesOnTile[i];
                        if (tree != null)
                        {
                            tree.Destroy();
                            //selTile.RemoveTree(tree, tree.EdgePosition);
                        }
                    }
                }
            }         
        }






        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            this.AllMaps = sn.DoList(AllMaps);

            this.abCartesianLength = sn.DoFloat(abCartesianLength);
            this.abDiagonalLength = sn.DoFloat(abDiagonalLength);

            this.eightDirsAsVectors = sn.DoArray(eightDirsAsVectors);
            this.normalizedEightDirsAsVectors = sn.DoArray(normalizedEightDirsAsVectors);

            // these values could also be derived from the loaded map:
            this.mapSubtileHeight = sn.DoInt32(mapSubtileHeight);
            this.mapSubtileWidth = sn.DoInt32(mapSubtileWidth);

            this.mapTileHeight = sn.DoInt32(mapTileHeight);
            this.mapTileWidth = sn.DoInt32(mapTileWidth);

            this.MapWorldHeight = sn.DoFloat(MapWorldHeight);
            this.MapWorldWidth = sn.DoFloat(MapWorldWidth);

            this.MaxWorldPos = sn.DoVector3(MaxWorldPos);
            this.NoParkingSpots = sn.DoDictionary(NoParkingSpots);

            this.NoParkingSpotsTimeStamps = sn.DoQueue(NoParkingSpotsTimeStamps);

            this.NoOfSectorsAcrossHeight = sn.DoInt32(NoOfSectorsAcrossHeight);
            this.NoOfSectorsAcrossWidth = sn.DoInt32(NoOfSectorsAcrossWidth);

          /*  this.TerrainCosts = sn.DoDictionary(TerrainCosts);
            this.TerrainCostIDs = sn.DoDictionary(TerrainCostIDs);
            */

            if (sn.mode != Snapshotter.Mode.Load)
            {
                snapshotAllMovementMaps = AllMovementMaps.Select(m => m.ID).ToList();

                snapshotTileMap = new TerrainTileID[Common.GetJaggedArrayWidth(TileMap)][]; 

                snapshotTerrainCosts = new Dictionary<SurfaceType.TransportType,SubtileLayersID>();                
                foreach (var item in TerrainCosts)
	            {
		             snapshotTerrainCosts.Add(item.Key, item.Value.ID);
	            }
                

                for (int x = 0; x < TileMap.Length; x++)
                {
                    TerrainTile[] column = TileMap[x];
                    TerrainTileID[] idColumn = new TerrainTileID[column.Length];
                    snapshotTileMap[x] = idColumn;
                    for (int y = 0; y < column.Length; y++)
                    {
                        idColumn[y] = column[y].ID;
                    }
                }


                
            }

            snapshotAllMovementMaps = sn.DoList(snapshotAllMovementMaps);
            snapshotTileMap = sn.DoJaggedArray(snapshotTileMap);
            snapshotTerrainCosts = sn.DoDictionary(snapshotTerrainCosts);

        //    snapshotTerrainRegionMap = (CyclableID)sn.SnapshotID<ICyclable, CyclableID>(TerrainRegionMap);


            sn.Ignore(TileMap);
            sn.Ignore(subtileIndexToDirectionMappings);
            sn.Ignore(direction);
            sn.Ignore(oppositeDirection);
            sn.Ignore(MapTransportTypeArray);
            sn.Ignore(AllMovementMaps);
            sn.Ignore(InfluenceMapTileIsComfortableInfo); // recreated post-load
            sn.Ignore(InfluenceMapTileIsFreeInfo); // recreated post-load
            sn.Ignore(TerrainCosts);
        //    sn.Ignore(TerrainRegionMap);

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

            AllMovementMaps = snapshotAllMovementMaps.Select(i => (MovementMap)LookUp<ICyclable, CyclableID>.FindByID(i)).ToList();

            // rebuild the tile map
            TileMap = new TerrainTile[Common.GetJaggedArrayWidth(snapshotTileMap)][]; 
            for (int x = 0; x < snapshotTileMap.Length; x++)
            {
                TerrainTileID[] idColumn = snapshotTileMap[x];
                TerrainTile[] tilemapColumn = new TerrainTile[idColumn.Length];
                TileMap[x] = tilemapColumn;
                for (int y = 0; y < idColumn.Length; y++)
                {
                    tilemapColumn[y] = LookUpSortedDictionary<TerrainTile, TerrainTileID>.FindByID(idColumn[y]);
                }
            }

            TerrainCosts = new Dictionary<SurfaceType.TransportType, SubtileLayers>();
            foreach (var item in snapshotTerrainCosts)
            {
                TerrainCosts.Add(item.Key, LookUp<SubtileLayers, SubtileLayersID>.FindByID(item.Value));
            }

            foreach (var item in TerrainCosts)
            {
                item.Value.LoadPostProcess(sn);
            }

         //   InitPostLoadMap();

            InitDataNotSnapshotted();

        }




    }


    /// <summary>
    /// we use this struct to store a timestamp for the blocked path.
    /// </summary>
    public struct BlockedPath
    {
        public Vector3 From; //Point From;
        public Vector3 To; //Point To;
        public SurfaceType.TransportType Transport;
        public DateTime Timestamp;

        public BlockedPath(SurfaceType.TransportType transportType, Vector3 f, Vector3 t, DateTime time)
        {
            this.From = f;
            this.To = t;
            this.Timestamp = time;
            this.Transport = transportType;
        }
    }
    public struct FromTo
    {
        Vector3 From;
        Vector3 To;
        SurfaceType.TransportType Transport;

        public FromTo(SurfaceType.TransportType transportType, Vector3 f, Vector3 t)
        {
            this.From = f;
            this.To = t;
            this.Transport = transportType;
        }
    }

    /// <summary>
    /// stores the time stamp associated to the no parking spot struct.
    /// </summary>
    public struct NoParkingSpotTimeStamp
    {
        public NoParkingSpot NoParkingSpot;
        public DateTime Timestamp;

        public NoParkingSpotTimeStamp(NoParkingSpot noParkingSpot, DateTime time)
        {
            this.NoParkingSpot = noParkingSpot;
            this.Timestamp = time;
        }
    }


    // Lars: I made these 3 structs to make calculations on the three coordinate systems less error-prone and easier to work with...

    [DebuggerDisplay("{X},{Y}")]
    public struct SubtilePos
    {
        public /*readonly*/ ushort X;
        public /*readonly*/ ushort Y;

        public SubtilePos(ushort x, ushort y)
        {
            this.X = x;
            this.Y = y;
        }

        public SubtilePos(Point pos)
        {
            this.X = (ushort)pos.X;
            this.Y = (ushort)pos.Y;
        }

        public static SubtilePos operator +(SubtilePos t1, SubtilePos t2)
        {
            return new SubtilePos((ushort)(t1.X + t2.X), (ushort)(t1.Y + t2.Y));
        }

        public static SubtilePos operator -(SubtilePos t1, SubtilePos t2)
        {
            return new SubtilePos((ushort)(t1.X - t2.X), (ushort)(t1.Y - t2.Y));
        }

        public bool Equals(SubtilePos p)
        {
            return (X == p.X) && (Y == p.Y);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is SubtilePos))
                return false;

            SubtilePos p = (SubtilePos)obj;
            return Equals(p);           

        }

        public override int GetHashCode()
        {
            return X.GetHashCode() + Y.GetHashCode(); // does not have to be unique...
        }

        public static bool operator ==(SubtilePos c1, SubtilePos c2)
        {
            return c1.Equals(c2);
        }

        public static bool operator !=(SubtilePos c1, SubtilePos c2)
        {
            return !c1.Equals(c2);
        }


        /// <summary>
        /// while migrating...
        /// </summary>
        /// <returns></returns>
        public Point ToPoint()
        {
            return new Point(X, Y);
        }
    }

    [DebuggerDisplay("{X},{Y}")]
    public struct TilePos
    {
        public int X;
        public int Y;

        public TilePos(Point pos)
        {
            this.X = pos.X;
            this.Y = pos.Y;
        }


        public TilePos(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public static TilePos operator +(TilePos t1, TilePos t2)
        {
            return new TilePos(t1.X + t2.X, t1.Y + t2.Y);
        }

        public static TilePos operator -(TilePos t1, TilePos t2)
        {
            return new TilePos(t1.X - t2.X, t1.Y - t2.Y);
        }

        /// <summary>
        /// while migrating...
        /// </summary>
        /// <returns></returns>
        public Point ToPoint()
        {
            return new Point(X, Y);
        }

        public string ToLink()
        {
            return string.Format("P{0},{1}", X, Y);
        }

        public override string ToString()
        {
            return string.Format("(X:{0},Y:{1})", X, Y);
        }

        public bool Equals(TilePos p)
        {
            return (X == p.X) && (Y == p.Y);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is TilePos))
                return false;

            TilePos p = (TilePos)obj;
            return Equals(p);

        }

        public override int GetHashCode()
        {
            return X.GetHashCode() + Y.GetHashCode(); // does not have to be unique...
        }

        public static bool operator ==(TilePos c1, TilePos c2)
        {
            return c1.Equals(c2);
        }

        public static bool operator !=(TilePos c1, TilePos c2)
        {
            return !c1.Equals(c2);
        }
    }

    [DebuggerDisplay("{X},{Y},{Z}")]
    public struct WorldLocation
    {
        public float X;
        public float Y;
        public float Z;

        public WorldLocation(float x, float y, float z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public WorldLocation(Vector3 location)
        {
            this.X = location.X;
            this.Y = location.Y;
            this.Z = location.Z;
        }

        public static WorldLocation operator +(WorldLocation t1, WorldLocation t2)
        {
            return new WorldLocation(t1.X + t2.X, t1.Y + t2.Y, t1.Z + t2.Z);
        }

        public static Vector3 operator +(WorldLocation t1, Vector3 t2)
        {
            return new Vector3(t1.X + t2.X, t1.Y + t2.Y, t1.Z + t2.Z);
        }

        public static WorldLocation operator -(WorldLocation t1, WorldLocation t2)
        {
            return new WorldLocation(t1.X - t2.X, t1.Y - t2.Y, t1.Z - t2.Z);
        }

        public static Vector3 operator -(WorldLocation t1, Vector3 t2)
        {
            return new Vector3(t1.X - t2.X, t1.Y - t2.Y, t1.Z - t2.Z);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is WorldLocation))
                return false;

            WorldLocation p = (WorldLocation)obj;
            return Equals(p);

        }

        public bool Equals(WorldLocation p)
        {
            return (X == p.X) && (Y == p.Y) && (Z == p.Z);
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() + Y.GetHashCode() + Z.GetHashCode(); // does not have to be unique...
        }

        public static bool operator ==(WorldLocation c1, WorldLocation c2)
        {
            return c1.Equals(c2);
        }

        public static bool operator !=(WorldLocation c1, WorldLocation c2)
        {
            return !c1.Equals(c2);
        }

        public Vector3 ToVector3()
        {
            return new Vector3(X, Y, Z);
        }
    }

    /// <summary>
    /// stores information about a destination where it is impossible to find a parking spot
    /// </summary>
    public struct NoParkingSpot
    {
        Point Destination;
        // Rectangle Area;
        //   Entity Vehicle;
        //   TerrainType.TransportType Transport;
        EntityType VehicleType;
        ProtectionLevel ProtectionLevel;
       // ThreatCategory Threat;
        EntityType DriverType;
        ThreatStance Approach;


        public NoParkingSpot(/*TerrainType.TransportType transportType,*/ EntityType vehicleType, ProtectionLevel protectionLevel, EntityType driverType, ThreatStance approach, Point destination) //, Entity vehicle) //Rectangle area)
        {
            this.Destination = destination;
            /*this.Vehicle = vehicle;
            this.Transport = transportType;*/
            this.VehicleType = vehicleType;
            this.ProtectionLevel = protectionLevel;
            //this.Threat = threat;
            this.DriverType = driverType;
            this.Approach = approach;
        }
    }
   
}
