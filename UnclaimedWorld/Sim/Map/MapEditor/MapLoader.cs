using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.IO;
using System.Threading.Tasks;
using UWGame.SimSide.Entities;
using GameStateManagement;
using UWGame.SimSide.Soil;
using UWGame.SimSide.Vegetation;
using UWGame.SimSide.Resources;
using UWGame.SimSide.InGameEvents;
using UWGame.Control;
using UWGame.SimSide.Items;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.AI.Needs;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.SimEffects;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Entities.Biological;
using System.Diagnostics;
using System.Xml.Serialization;

namespace UWGame.SimSide.Maps.MapEditor //TODO DECOUPLE -- move this to client???? don;'t know
{
    /// <summary>
    /// this class now manages more timesliced stuff related to loading a map
    /// </summary>
    public class MapLoader
    {
        //MapManager map;

        private const float minimumHeightForTreesAndGrass = -30f;
        private const float minimumHeightForMoistureFromWaterSurface = -80f;


        #region Load map progress

        string mapFolderName;

        /// <summary>
        /// full path to the map folder
        /// used by map editor only
        /// - OR - gets assigned at start?
        /// </summary>
        string mapFolderPath;

        private QueueStateLoad queueState = QueueStateLoad.LoadMapData;

        MapData mapData;

        int transportProgress = 0;
        int tileProgressX = 0;

        int entityIndex = 0;

        #endregion

        Array transports;

        HashSet<TerrainTile> tilesContainingWaterEdge = new HashSet<TerrainTile>();
        HashSet<TerrainTile> grownTilesFromWaterEdge;

        public MapLoader(string mapFolderName)
        {
            Debug.Assert(The.Map.AllMaps.Contains(mapFolderName));

            this.mapFolderName = mapFolderName;
            
            transports = Enum.GetValues(typeof(SurfaceType.TransportType));
        }

        public MapLoader(DirectoryInfo mapFolder) //  string mapFolderName)
        {
            Debug.Assert(The.Map.AllMaps.Contains(mapFolder.Name));
            //Debug.Assert(The.Map.AllMaps.Contains(mapFolderName));

            //this.mapFolderName = mapFolderName;
            this.mapFolderPath = mapFolder.FullName;
                       
            transports = Enum.GetValues(typeof(SurfaceType.TransportType));
        }

        private enum QueueStateLoad
        {
            BuildMap, GenerateMapFromTextureData,
            ReduceMapSubdivision,
            LoadSavedMapEntities,
            PlaceFirewoodFromTrees,
            LoadTiles,
            InitCollisionTrees,
            LoadMapData,
            BuildMapTerrainSubtiles,
            BuildMapCosts,
            LoadWaterBottomTintTexture,
            LoadWaterColorTexture,
            LoadSoilTexture,
            LoadVegetationTexture,
            LoadTerrainHeightsTexture,
            LoadMoistureTexture,
            LoadSavedMapTrees,
            ReduceMapSubdivisionScanHoriz,
            ReduceMapSubdivisionScanVert,
            ReduceSubdivisionCombineTiles,
            ReduceSubdivisionGrowArea,
            InitializeEntityData
        }

        public static MapData LoadMapData(string mapDataXmlPath, string folderName)
        {
            // XXX HACK: Before we do this, we set the dimensions of the map
            // The trees (and entity types?) are clamped by these dimensions in set Location, so they must not be 0*0
            Vector3 maxWorldPos = The.Map.MaxWorldPos;
            The.Map.MaxWorldPos = new Vector3(500000, 500000, 500000);

            //Create our own namespaces for the output
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            //Add an empty namespace and empty value
            ns.Add("", "");
            XmlSerializer s = new XmlSerializer(typeof(MapData));

            MapData mapData;
            using (TextReader r = new StreamReader(mapDataXmlPath))
            {
                mapData = (MapData)s.Deserialize(r);
            }

            mapData.FolderName = folderName;

            // reset... should be set from the file info now.
            The.Map.MaxWorldPos = maxWorldPos;

            return mapData;
        }

        /// <summary>
        /// loads a new map in timeslices, to prevent not responding status assigned by Windows
        /// </summary>
        /// <returns></returns>
         public bool QueueLoad()
         {
             Rectangle tileArea = new Rectangle(0, 0, The.Map.mapTileWidth, The.Map.mapTileHeight);

             switch (queueState)
             {
                 case QueueStateLoad.LoadMapData:
                     {
                        
                         string mapDataXmlPath;

                         if (mapFolderPath == null)
                         {
                             mapFolderPath = MapManager.ComposeMapDataFolderPath(mapFolderName); // get the path to the RG maps
                            // mapDataXmlPath = MapManager.ComposeMapDataXmlFilePath(mapFolderName);
                         }
                         /*else
                         {
                            // mapDataXmlPath = MapManager.ComposeMapDataXmlFilePathFromFolderPath(mapFolderPath);
                         }*/

                         mapDataXmlPath = MapManager.ComposeMapDataXmlFilePathFromFolderPath(mapFolderPath);

                         mapData = LoadMapData(mapDataXmlPath, mapFolderName);

                         The.Map.SetDimensions(mapData.Dimensions);
                         The.MapUI.SetSize();

                         queueState = QueueStateLoad.InitCollisionTrees;

                         break;
                     }
                 case QueueStateLoad.InitCollisionTrees:
                     {
                         if (The.Map.InitCollisionTrees())
                         {
                             Common.InitJaggedArray(ref The.Map.TileMap, The.Map.mapTileWidth, The.Map.mapTileHeight);

                             queueState = QueueStateLoad.BuildMapTerrainSubtiles;
                         }

                         break;
                     }
                 case QueueStateLoad.BuildMapTerrainSubtiles:
                     {
                         if (BuildMapTerrainSubtiles())
                         {
                             
                             queueState = QueueStateLoad.BuildMap;                             
                         }

                         break;
                     }
                 case QueueStateLoad.BuildMap:
                     {
                         if (BuildMap())
                         {
                             queueState = QueueStateLoad.BuildMapCosts;                             
                         }

                         break;
                     }
                 case QueueStateLoad.BuildMapCosts:
                     {
                         if (BuildMapCosts())
                         {
                             if (The.Client != null) // WHY - OLD: move this to client?
                             {
                                 queueState = QueueStateLoad.LoadMoistureTexture;
                             }
                             else
                             {
                                 queueState = QueueStateLoad.ReduceMapSubdivision;
                             }                           
                         }

                         break;
                     }
                     
                 case QueueStateLoad.LoadMoistureTexture:
                     {
                         if (LoadMoistureTexture())
                         {
                             queueState = QueueStateLoad.LoadTerrainHeightsTexture;
                         }

                         break;
                     }
                 case QueueStateLoad.LoadTerrainHeightsTexture:
                     {
                         if (LoadTerrainHeightsTexture())
                         {
                             queueState = QueueStateLoad.LoadVegetationTexture;
                         }

                         break;
                     }
                 case QueueStateLoad.LoadVegetationTexture:
                     {
                         if (LoadVegetationTexture())
                         {
                             queueState = QueueStateLoad.LoadSoilTexture;
                         }

                         break;
                     }
                 case QueueStateLoad.LoadSoilTexture:
                     {
                         if (LoadSoilTexture())
                         {
                             queueState = QueueStateLoad.LoadWaterColorTexture;
                         }

                         break;
                     }
                 case QueueStateLoad.LoadWaterColorTexture:
                     {
                         if (LoadWaterColorTexture())
                         {
                             queueState = QueueStateLoad.LoadWaterBottomTintTexture;
                         }

                         break;
                     }
                 case QueueStateLoad.LoadWaterBottomTintTexture:
                     {
                         if (LoadWaterBottomTintTexture())
                         {
                             queueState = QueueStateLoad.ReduceMapSubdivisionScanHoriz;
                         }

                         break;
                     }
                 case QueueStateLoad.ReduceMapSubdivisionScanHoriz:
                     {
                         if (ReduceSubdivisionScanHorizontally(tileArea, tilesContainingWaterEdge))
                         {
                             queueState = QueueStateLoad.ReduceMapSubdivisionScanVert;
                         }

                         break;
                     }
                 case QueueStateLoad.ReduceMapSubdivisionScanVert:
                     {
                         if (ReduceSubdivisionScanVertically(tileArea, tilesContainingWaterEdge))
                         {
                            // grownTilesFromWaterEdge = new HashSet<TerrainTile>(tilesContainingWaterEdge);

                             queueState = QueueStateLoad.ReduceSubdivisionGrowArea;
                         }

                         break;
                     }
                 case QueueStateLoad.ReduceSubdivisionGrowArea:
                     {
                         if (ReduceSubdivisionGrowArea(tileArea, tilesContainingWaterEdge, ref grownTilesFromWaterEdge))
                         {                             
                             queueState = QueueStateLoad.ReduceSubdivisionCombineTiles;
                         }

                         break;
                     }
                 case QueueStateLoad.ReduceSubdivisionCombineTiles:
                     {
                         if (ReduceSubdivisionCombineTiles(tileArea, grownTilesFromWaterEdge))
                         {
                             queueState = QueueStateLoad.InitializeEntityData; // QueueStateLoad.LoadSavedMapEntities;
                         }

                         break;
                     }
                 case QueueStateLoad.InitializeEntityData:
                     {
                         InitializeEntityData();
                         queueState = QueueStateLoad.LoadSavedMapEntities;
                         break;
                     }
                 case QueueStateLoad.LoadSavedMapEntities:
                     {
                         if (LoadSavedMapEntities())
                         {
                             queueState = QueueStateLoad.LoadSavedMapTrees;
                         }

                         break;
                     }
                 case QueueStateLoad.LoadSavedMapTrees:
                     {
                         if (LoadSavedMapTrees())
                         {
                             if (The.Sim.Mode == Sim.EngineMode.Game)
                             {
                                 queueState = QueueStateLoad.PlaceFirewoodFromTrees;
                             }
                             else
                             {
                                 queueState = QueueStateLoad.LoadTiles;
                             }
                         }

                         break;
                     }
                 case QueueStateLoad.PlaceFirewoodFromTrees:
                     {
                         if (PlaceFirewoodFromTrees())
                         {
                             queueState = QueueStateLoad.LoadTiles;
                         }

                         break;
                     }
                 case QueueStateLoad.LoadTiles:
                     {
                         if (LoadTiles(mapData))
                         {
                             queueState = QueueStateLoad.LoadMapData; 
                             return true;
                         }

                         break;
                     }
             }

             return false;
         }

      /*  public void Load(MapData mapToLoad) 
        {
            // these calls take 1s each for the large maps. cycle them

            BuildMap(mapToLoad);

            if (The.Client != null) // TODO move this to client?
                GenerateMapFromTextureData(mapToLoad);

            ReduceMapSubdivision();

            //LoadEvents(mapToLoad);

            LoadSavedMapEntities(mapToLoad); //, timeOfDay, day);


            if (The.Sim.Mode == Sim.EngineMode.Game)
            {
                PlaceFirewoodFromTrees();
            }

            LoadTiles(mapToLoad);
        }*/


       
        private bool BuildMap()
        {         
            MapManager map = The.Map;

           
            ushort subTileWidth = (ushort)map.mapSubtileWidth; 
            ushort subTileHeight = (ushort)map.mapSubtileHeight; 

            map.TerrainCosts = new Dictionary<SurfaceType.TransportType, SubtileLayers>();


          
           foreach (var item in Enum.GetValues(typeof(SurfaceType.TransportType)))
           {
               if ((SurfaceType.TransportType)item != SurfaceType.TransportType.Air)
               {
                   SubtileLayer layer = new SubtileLayer(subTileWidth, subTileHeight);
                   SubtileLayers layers = new SubtileLayers(item.ToString(), layer);

                   layer.CreateAllSectors();

                   map.TerrainCosts.Add((SurfaceType.TransportType)item, layers);
                   
               }
           }

           //BuildMapCosts();

            return true;
        }

        private bool BuildMapCosts()
        {
            MapManager map = The.Map;

            ushort subTileWidth = (ushort)map.mapSubtileWidth;
            ushort subTileHeight = (ushort)map.mapSubtileHeight;

            TerrainTile tile;

            SubtileLayers currentMap;


            // initialize map costs

            int maxX = tileProgressX + 3;
            maxX = Common.ClampTop(maxX, subTileWidth);



            currentMap = map.TerrainCosts[(SurfaceType.TransportType)transportProgress];

            TerrainTile[] column;
            for (int x = tileProgressX; x < maxX; x++)
            {
                column = map.TileMap[x / 3];

                for (int y = 0; y < subTileHeight; y++)
                {
                    tile = column[y / 3];

                    currentMap.SetValueOnBottomLayer(x, y, (MapManager.SubtileValue)(tile.GetCost((SurfaceType.TransportType)transportProgress, SurfaceType.TerrainFeatures.None)));

                }
            }

            if (maxX < subTileWidth)
            {
                tileProgressX = maxX;
            }
            else
            {
                tileProgressX = 0;

                do
                {
                    transportProgress++;
                }
                while (transportProgress < transports.Length && (SurfaceType.TransportType)transportProgress == SurfaceType.TransportType.Air);

                if (transportProgress == transports.Length)
                {
                    // done
                    transportProgress = 0;
                    return true;
                }
            }

            return false;

            /*
        Type transports = typeof(SurfaceType.TransportType);

        // initialize map costs
        foreach (var transport in Enum.GetValues(transports))
        {
            if ((SurfaceType.TransportType)transport == SurfaceType.TransportType.Air)
            {
                continue;
            }

            currentMap = map.TerrainCosts[(SurfaceType.TransportType)transport];

            TerrainTile[] column;
            for (int x = 0; x < subTileWidth; x++)
            {
                column = map.TileMap[x / 3];

                for (int y = 0; y < subTileHeight; y++)
                {
                    tile = column[y / 3];

                    currentMap.SetValueOnBottomLayer(x, y, (MapManager.SubtileValue)(tile.GetCost((SurfaceType.TransportType)transport, SurfaceType.TerrainFeatures.None)));

                }
            }
        }*/
        }

      
        private bool BuildMapTerrainSubtiles()
        {
            TerrainTile mapTile;

            MapManager map = The.Map;

            // create terrain tiles and terrainsubtiles:

           /* for (int x = 0; x < map.mapTileWidth; x++)
            {*/
                for (int y = 0; y < map.mapTileHeight; y++)
                {
                    mapTile = new TerrainTile(tileProgressX, y);

                    Common.InitJaggedArray(ref mapTile.TerrainSubtiles, 3, 3); // create these for all tiles, but clean up later...
                    for (int sx = 0; sx < 3; sx++)
                    {
                        for (int sy = 0; sy < 3; sy++)
                        {
                            mapTile.TerrainSubtiles[sx][sy] = new Terrain(mapTile);
                        }
                    }

                    map.TileMap[tileProgressX][y] = mapTile;
                }
           // }

            tileProgressX++;
            if (tileProgressX == map.mapTileWidth)
            {
                tileProgressX = 0;
                return true;
            }

            return false;

        }


        private string ComposePath(string filename)
        {
            return System.IO.Path.Combine(mapFolderPath, filename);
        }

        private bool LoadMoistureTexture()
        {

            Texture2D texture = null;
            Color[] colors = null;

            string waterFilePath = ComposePath("moisture.png"); // MapManager.ComposeMapDataFolderPath(mapData.FolderName, "moisture.png");
            if (System.IO.File.Exists(waterFilePath))
            {
                using (Stream stream = File.OpenRead(waterFilePath))
                {
                    texture = Texture2D.FromStream(The.Client.GraphicsDevice, stream);
                    colors = new Color[texture.Width * texture.Height];
                    texture.GetData(colors);

                    Parallel.For(0, The.Map.mapTileWidth, (x) =>
                    {
                        Terrain terrain;

                        TerrainTile currentTile;
                        float xCoord, yCoord;

                        for (int y = 0; y < The.Map.mapTileHeight; y++)
                        {

                            currentTile = The.Map.TileMap[x][y];
                            Vector4 moisture = ReadColor(x, y, colors, texture).ToVector4() / 9f;

                            currentTile.Moisture = moisture.X;

                            /*for (int sx = 0; sx < 3; sx++)
                            {
                                for (int sy = 0; sy < 3; sy++)
                                {

                                    xCoord = x + sx * 0.33f; // use fractions for subtiles when doing texture lookup...
                                    yCoord = y + sy * 0.33f;

                                    terrain = currentTile.TerrainSubtiles[sx][sy];

                                    Vector4 moisture;
                                    moisture = ReadColor(xCoord, yCoord, colors, texture).ToVector4() / 9f;

                                    terrain.Moisture = moisture.X;
                                }
                            }*/

                        }
                    });
                }
            }

            return true;
        }


        /// <summary>
        /// Keep this???
        /// This should be data driven...
        /// </summary>
        private bool PlaceFirewoodFromTrees()
        {
            TerrainTile currentTile;
            float totalFibrousTreeMass = 0f;

            ResourceType firewoodType = GameData.Instance.AllResourceTypes["firewood"];

            Trees.Tree treeComponent;
            for (int x = 0; x < The.Map.mapTileWidth; x++)
            {
                for (int y = 0; y < The.Map.mapTileHeight; y++)
                {
                    currentTile = The.Map.TileMap[x][y];
                    totalFibrousTreeMass = 0f;

                    if (currentTile.TreesOnTile != null)
                    {
                        foreach (Entity tree in currentTile.TreesOnTile)
                        {
                            tree.Find(out treeComponent);
                            /*if (tree.EntityType.TreeType.FibrousPercentageOfTotalMass > 0.1)
                            {*/
                            totalFibrousTreeMass += tree.EntityType.TreeType.FibrousPercentageOfTotalMass * tree.Bulk;
                            //}
                        }

                        float totalMass = 0.05f * totalFibrousTreeMass;
                        
                        if (Common.IsGreaterThan(totalMass, 0f)
                            && firewoodType.GetHarvestableItemsFromBulk(totalMass) > 0)
                        {
                            currentTile.AddResource(firewoodType, totalMass);
                        }
                       // currentTile.TileResources["firewood"].SetTotalHarvestableBulk(0.05f * totalFibrousTreeMass);
                    }

                }

            }

            return true;
        }

        private bool LoadSoilTexture()
        {
            MapManager map = The.Map;

            string folderPath = ComposePath("Soil"); // MapManager.ComposeMapDataFolderPath(mapData.FolderName, "Soil");

            Dictionary<string, Color[]> soilTextures;
            Dictionary<string, Texture2D> soilTexture2ds;
            LoadBitmapsInFolder(folderPath, out soilTextures, out soilTexture2ds);


            foreach (KeyValuePair<string, Color[]> kvp in soilTextures)
            {
                SoilComponentType soilType;
                SoilComponent tileSoil;
                if (GameData.Instance.AllSoilComponentTypes.TryGetValue("soil:" + kvp.Key, out soilType))
                {
                    Parallel.For(0, map.mapTileWidth, (x) => // this will give bugs if SoilComponent gets an ID
                    {
                        Terrain terrain;

                        TerrainTile currentTile;
                        float xCoord, yCoord;

                        for (int y = 0; y < map.mapTileHeight; y++)
                        {

                            currentTile = map.TileMap[x][y];

                            for (int sx = 0; sx < 3; sx++)
                            {
                                for (int sy = 0; sy < 3; sy++)
                                {

                                    xCoord = x + sx * 0.33f; // use fractions for subtiles when doing texture lookup...
                                    yCoord = y + sy * 0.33f;

                                    terrain = currentTile.TerrainSubtiles[sx][sy];
                                    // float readValue = ReadColor(xCoord, yCoord, kvp.Value, soilTexture2ds[kvp.Key]).R;


                                    byte readValue = ReadColor(xCoord, yCoord, kvp.Value, soilTexture2ds[kvp.Key]).R;

                                    /*    float dummy = 0f;
                                        if (x == 95 && y == 118)
                                        {
                                            dummy = 0.00001f;
                                        }**/

                                    if (readValue > 1) // .png will report 0 as 1!
                                    {
                                        float amount = readValue * bitmapToSubtileAmountConversionFactor; // +dummy;

                                        if (amount > 0f)
                                        {
                                            /*  if (x == 184 && y == 78)
                                              {
                                                  amount += 0.000001f;
                                                 // throw new Exception();
                                              }*/
                                            //  tileSoil = new SoilComponent(terrain, soilType);
                                            //   tileSoil.Amount = amount;
                                            terrain.AddSoilComponent(soilType, amount);
                                        }
                                    }
                                }
                            }
                        }
                    });
                }
            }


            // normalize to sum = maxGrowthPerSubtile if greater than maxGrowthPerSubtile:
            Parallel.For(0, The.Map.mapTileWidth, (x) =>
            {
                Terrain terrain;
                TerrainTile currentTile;

                for (int y = 0; y < map.mapTileHeight; y++)
                {
                    currentTile = map.TileMap[x][y];

                    for (int sx = 0; sx < 3; sx++)
                    {
                        for (int sy = 0; sy < 3; sy++)
                        {
                            terrain = currentTile.TerrainSubtiles[sx][sy];

                            terrain.NormalizeSoil();
                        }
                    }

                }
            });

            return true;
        }

        private static void LoadBitmapsInFolder(string folderPath, out Dictionary<string, Color[]> vegetationTextures, out Dictionary<string, Texture2D> vegetationTexture2ds)
        {
            vegetationTextures = new Dictionary<string, Color[]>();
            vegetationTexture2ds = new Dictionary<string, Texture2D>();

            if (System.IO.Directory.Exists(folderPath))
            {
                string[] vegetationBitmaps = System.IO.Directory.GetFiles(folderPath, "*.png");
                foreach (string file in vegetationBitmaps)
                {
                    using (Stream stream = File.OpenRead(file))
                    {
                        Texture2D texture = Texture2D.FromStream(The.Client.GraphicsDevice, stream);
                        Color[] colors = new Color[texture.Width * texture.Height];
                        texture.GetData(colors);
                        string key = System.IO.Path.GetFileNameWithoutExtension(file);
                        vegetationTextures.Add(key, colors);
                        vegetationTexture2ds.Add(key, texture);
                    }
                }
            }
        }

        private float bitmapToSubtileAmountConversionFactor = 1f / (9f * 255f);
        private bool LoadVegetationTexture()
        {

            string folderPath = ComposePath("Vegetation"); // MapManager.ComposeMapDataFolderPath(mapData.FolderName, "Vegetation");

            Dictionary<string, Color[]> vegetationTextures;
            Dictionary<string, Texture2D> vegetationTexture2ds;
            LoadBitmapsInFolder(folderPath, out vegetationTextures, out vegetationTexture2ds);

            MapManager map = The.Map;

            foreach (KeyValuePair<string, Color[]> kvp in vegetationTextures)
            {
                LowVegetationType vegType;

                if (GameData.Instance.AllLowVegetationTypes.TryGetValue("veg:" + kvp.Key, out vegType))
                {
                    /* Parallel.For(0, map.mapTileWidth, (x) => // cannot use parallelism - ID gen/lookup is not threadsafe
                     {*/
                    for (int x = 0; x < map.mapTileWidth; x++)
                    {
                        LowVegetation tileVeg;
                        Terrain terrain;


                        TerrainTile currentTile;
                        float xCoord, yCoord;

                        for (int y = 0; y < map.mapTileHeight; y++)
                        {
                            currentTile = map.TileMap[x][y];



                            for (int sx = 0; sx < 3; sx++)
                            {
                                for (int sy = 0; sy < 3; sy++)
                                {
                                    xCoord = x + sx * 0.33f; // use fractions for subtiles when doing texture lookup...
                                    yCoord = y + sy * 0.33f;

                                    /*   float dummy = 0f;
                                       if (x == 95 && y == 118)
                                       {
                                           dummy = 0.00001f;
                                       }*/

                                    terrain = currentTile.TerrainSubtiles[sx][sy];
                                    float growth = ReadColor(xCoord, yCoord, kvp.Value, vegetationTexture2ds[kvp.Key]).R * bitmapToSubtileAmountConversionFactor; // +dummy;


                                    if (!vegType.CanGrowUnderWater)
                                    {
                                        // scale the grass towards the water surface:
                                        /* if (terrain.LevelBelowWater < 0 && terrain.LevelBelowWater > minimumHeightForTreesAndGrass)
                                         {
                                             growth = MathHelper.Lerp(0f, growth, terrain.LevelBelowWater / minimumHeightForTreesAndGrass);
                                         }
                                         else*/
                                        if (terrain.LevelBelowWater > 0)
                                        {
                                            growth = 0f;
                                        }
                                    }

                                    if (growth > 0f)
                                    {
                                        //  tileVeg = new LowVegetation(terrain, vegType);
                                        //   tileVeg.Amount = growth;
                                        //   terrain.AddVegetation(tileVeg);
                                        terrain.AddVegetation(vegType, growth);
                                    }
                                }
                            }
                        }
                    }
                }
            }


            // normalize to sum = 1 if greater than 1:
            Parallel.For(0, map.mapTileWidth, (x) =>
            {
                Terrain terrain;
                // Point subtilePos;

                TerrainTile currentTile;
                // float xCoord, yCoord;


                for (int y = 0; y < map.mapTileHeight; y++)
                {
                    currentTile = map.TileMap[x][y];

                    for (int sx = 0; sx < 3; sx++)
                    {
                        for (int sy = 0; sy < 3; sy++)
                        {
                            //subtilePos = TileAndRelativeSubtileToAbsoluteSubtile(x, y, sx, sy);

                            terrain = currentTile.TerrainSubtiles[sx][sy];

                            terrain.NormalizeVegetation();
                        }
                    }
                }
            });

            return true;
        }

        private bool LoadWaterColorTexture()
        {
            TerrainTile currentTile;
            Texture2D texture = null;
            Color[] colors = null;

            MapManager map = The.Map;

            string waterFilePath = ComposePath("waterColors.png"); // MapManager.ComposeMapDataFolderPath(mapData.FolderName, "waterColors.png");
            if (System.IO.File.Exists(waterFilePath))
            {
                using (Stream stream = File.OpenRead(waterFilePath))
                {
                    texture = Texture2D.FromStream(The.Client.GraphicsDevice, stream);
                    colors = new Color[texture.Width * texture.Height];
                    texture.GetData(colors);


                    for (int x = 0; x < map.mapTileWidth; x++)
                    {
                        for (int y = 0; y < map.mapTileHeight; y++)
                        {

                            currentTile = map.TileMap[x][y];

                            Vector4 waterColor;
                            /* if (colors != null)
                             {*/
                            waterColor = ReadColor(x, y, colors, texture).ToVector4();
                            /*  }
                              else
                              {
                                  terrainDepth = 0;
                              }*/


                            currentTile.ColorOfWater = waterColor; //colors[x + y * texture.Width].R;

                        }
                    }
                }
            }

            return true;
        }

        private bool LoadWaterBottomTintTexture()
        {
            TerrainTile currentTile;
            Texture2D texture = null;
            Color[] colors = null;

            MapManager map = The.Map;

            string waterFilePath = ComposePath("waterBottomTint.png"); // MapManager.ComposeMapDataFolderPath(mapData.FolderName, "waterBottomTint.png");
            if (System.IO.File.Exists(waterFilePath))
            {
                using (Stream stream = File.OpenRead(waterFilePath))
                {
                    texture = Texture2D.FromStream(The.Client.GraphicsDevice, stream);
                    colors = new Color[texture.Width * texture.Height];
                    texture.GetData(colors);


                    for (int x = 0; x < map.mapTileWidth; x++)
                    {
                        for (int y = 0; y < map.mapTileHeight; y++)
                        {

                            currentTile = map.TileMap[x][y];

                            Vector4 waterColor;
                            waterColor = ReadColor(x, y, colors, texture).ToVector4();

                            currentTile.WaterBottomTint = waterColor;

                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Loads values into the Grass property of the Terrain Tiles from a .png image.
        /// </summary>
        /// <param name="dataTexture"></param>
        private bool GenerateMapFromTextureData() 
        {
                       
            // we probably won't be using this in the final game.
            // load moisture, but override water tiles in LoadMapTerrainHeights:
            LoadMoistureTexture();

            LoadTerrainHeightsTexture();
     
            LoadVegetationTexture();
            LoadSoilTexture();


            LoadWaterColorTexture();
            LoadWaterBottomTintTexture();


            return true;

        }


        /*   private void ComputeMapTerrainData()
           {
               Parallel.For(0, map.mapWidth, (x) =>
                   {
                       TerrainTile tile;
                       for (int y = 0; y < map.mapHeight; y++)
                       {
                           tile = map.TileMap[x][y];
                           if (tile.Terrain != null)
                           {
                               tile.Terrain.ComputeTerrainPosition();
                           }
                           else
                           {
                               for (int sx = 0; sx < 3; sx++)
                               {
                                   for (int sy = 0; sy < 3; sy++)
                                   {
                                       tile.TerrainSubtiles[sx][sy].ComputeTerrainPosition(sx, sy);
                                   }
                               }
                           }
                       }
                   });

           }*/

        /// <summary>
        /// Resamples the value, so the bitmap size can be independent of the map.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="colors"></param>
        /// <param name="texture"></param>
        /// <returns></returns>
        private Color ReadColor(float x, float y, Color[] colors, Texture2D texture)
        {
            MapManager map = The.Map;

            float relx = x / (float)map.mapTileWidth;
            float rely = y / (float)map.mapTileHeight;

            // the texture is now independent of the terrain width and height!                   
            return colors[(int)(relx * texture.Width) + (int)(rely * (texture.Height - 1)) * texture.Width];

        }

        private bool LoadTerrainHeightsTexture()
        {
            MapManager map = The.Map;

            Texture2D texture = null;
            Color[] colors = null;
            //string[] terrainHeightBitmaps = System.IO.File.OpenRead(mapData.Folder + ""); // System.IO.Directory.GetFiles(mapData.Folder + "/Vegetation", "*.png");
            string terrainFilePath = ComposePath("terrainHeights.png"); // MapManager.ComposeMapDataFolderPath(mapData.FolderName, "terrainHeights.png");


            if (System.IO.File.Exists(terrainFilePath))
            {
                using (Stream stream = File.OpenRead(terrainFilePath))
                {
                    //texture = Texture2D.FromFile(UWGame.SimSide.Instance.GraphicsDevice, terrainFilePath);
                    texture = Texture2D.FromStream(The.Client.GraphicsDevice, stream);

                    colors = new Color[texture.Width * texture.Height];
                    texture.GetData(colors);

                }
            }
            else throw new Exception("File not found: " + terrainFilePath);

            // how far below terrain is the water level? 
            float waterLevelBelowTerrain = The.Client.Renderer.Water.WaterHeight - MapManager.TerrainZLevel;


            Parallel.For(0, map.mapTileWidth, (x) =>
            //for (int x = 0; x < mapWidth; x++)
            {
                Terrain terrain;
                Point subtilePos;
                float terrainDepth;
                TerrainTile currentTile;
                float xCoord, yCoord;

                for (int y = 0; y < map.mapTileHeight; y++)
                {
                    currentTile = map.TileMap[x][y];

                    for (int sx = 0; sx < 3; sx++)
                    {
                        for (int sy = 0; sy < 3; sy++)
                        {
                            subtilePos = MapManager.TileAndRelativeSubtileToAbsoluteSubtile(x, y, sx, sy);

                            xCoord = x + sx * 0.33f; // use fractions for subtiles...
                            yCoord = y + sy * 0.33f;

                            terrain = currentTile.TerrainSubtiles[sx][sy];


                            terrainDepth = ReadColor(xCoord, yCoord, colors, texture).R;

                            SetTerrainDepth(waterLevelBelowTerrain, terrain, terrainDepth);

                            The.Map.SetSubtileCostToSurfaceType(subtilePos);

                        }
                    }

                    SetMoistureOnTile(currentTile);
                }
            });

            // compute the water level on every tile
            // UpdateTileMapWithWaterLevel();

            return true;
        }

        public static void SetTerrainDepth(float waterLevelBelowTerrain, Terrain terrain, float terrainDepth)
        {
            float vertexDepthUnderWater;

            terrain.TerrainDepth = terrainDepth;

            // don't clamp. We need negative water levels for the water shader
            vertexDepthUnderWater = terrainDepth - waterLevelBelowTerrain;


            if (vertexDepthUnderWater > 0f)
            {
                // below water. block movement:
                terrain.SurfaceType = WaterType.Instance;
            }
            else
            {
                // tile is above water
                terrain.SurfaceType = PlainsType.Instance;
            }

           
            terrain.LevelBelowWater = vertexDepthUnderWater;          
        }


        /// <summary>
        /// consider storing this in the map???? if the waterline never changes anyways???
        /// </summary>
    /*    private bool ReduceMapSubdivision()
        {          
           
           // HashSet<TerrainTile> tilesContainingWaterEdge = new HashSet<TerrainTile>();

            ReduceSubdivisionScanHorizontally();
           
            ReduceSubdivisionScanVertically();          
           
           
            ReduceSubdivisionGrowArea(tilesContainingWaterEdge, out grownTilesFromWaterEdge);


            ReduceSubdivisionCombineTiles(grownTilesFromWaterEdge);
        }*/

        private static bool ReduceSubdivisionCombineTiles(Rectangle tileArea, HashSet<TerrainTile> grownTilesFromWaterEdge)
        {
            MapManager map = The.Map;

            // now reduce subdivison of terrain on all the tiles that we didn't mark:

            The.Map.IterateTileArea(tileArea, tile =>
                {
                    if (!grownTilesFromWaterEdge.Contains(tile)) // O(1) operation with HashSet
                    {                       
                        CombineTerrainSubtiles(tile);
                    }
                });


            /*
            for (int x = tileArea.Left; x < tileArea.Width; x++)          
            {
                TerrainTile tile;
               // for (int y = 0; y < map.mapTileHeight; y++)
                for (int y = tileArea.Top; y < tileArea.Height; y++)
                {
                    tile = map.TileMap[x][y];
                    if (!grownTilesFromWaterEdge.Contains(tile)) // O(1) operation with HashSet
                    {
                       
                        CombineTerrainSubtiles(tile);
                    }
                }
            }*/

            return true;
        }


        /// <summary>
        /// adds more tiles to the list of tiles that should not be merged
        /// </summary>
        /// <param name="tileArea"></param>
        /// <returns></returns>
        private static bool ReduceSubdivisionGrowArea(Rectangle tileArea, HashSet<TerrainTile> tilesContainingWaterEdge, ref HashSet<TerrainTile> grownTilesFromWaterEdge)
        {
            MapManager map = The.Map;
            TerrainTile currentTile;

            // grow the area:
            int radius = 1; //2    ...number of tiles from water's edge where we have HiRes ground texture blending
            Point pos;

            grownTilesFromWaterEdge = new HashSet<TerrainTile>(tilesContainingWaterEdge);


            //  TerrainTile currentTile;
            foreach (TerrainTile tile in tilesContainingWaterEdge)
            {
                pos = new Point(tile.X, tile.Y);

                int minX = Math.Max(tileArea.Left, pos.X - radius);
                int minY = Math.Max(tileArea.Top, pos.Y - radius); // should be 0 with whole map!
                int maxX = Math.Min(tileArea.Width - 1, pos.X + radius);
                int maxY = Math.Min(tileArea.Height - 1, pos.Y + radius);
               

               /* int minX = Math.Max(0, pos.X - radius);
                int minY = Math.Max(0, pos.Y - radius);
                int maxX = Math.Min(map.mapTileWidth - 1, pos.X + radius);
                int maxY = Math.Min(map.mapTileHeight - 1, pos.Y + radius);
                */

                for (int x = minX; x <= maxX; x++)
                {
                    for (int y = minY; y <= maxY; y++)
                    {
                        currentTile = map.TileMap[x][y];

                        if (!currentTile.IsFullyUnderWater())
                        {
                            grownTilesFromWaterEdge.Add(currentTile);
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// call this from Editor
        /// </summary>
        /// <param name="tileArea"></param>
        public static void RecomputeSubdivision(Rectangle tileArea)
        {            
            // start by subdividing all tiles (we have to keep existing terrain values!):
           // CreateTerrainSubtiles(tileArea);

            HashSet<TerrainTile> tilesContainingWaterEdge = new HashSet<TerrainTile>();

            // scan for water...
            ReduceSubdivisionScanHorizontally(tileArea, tilesContainingWaterEdge);
            ReduceSubdivisionScanVertically(tileArea, tilesContainingWaterEdge);

            HashSet<TerrainTile> grownTilesFromWaterEdge = null; // = new HashSet<TerrainTile>(tilesContainingWaterEdge);

            // grow the area
            ReduceSubdivisionGrowArea(tileArea, tilesContainingWaterEdge, ref grownTilesFromWaterEdge);

            // combine some subtiles
            ReduceSubdivisionCombineTiles(tileArea, grownTilesFromWaterEdge);

        }

        public static void CreateTerrainSubtiles(Rectangle tileArea)
        {
           // TerrainTile mapTile;
           // MapManager map = The.Map;

            // create terrainsubtiles:

            The.Map.IterateTileArea(tileArea,
                mapTile =>
                {
                    if (mapTile.Terrain != null)
                    {
                        Common.InitJaggedArray(ref mapTile.TerrainSubtiles, 3, 3);
                        for (int sx = 0; sx < 3; sx++)
                        {
                            for (int sy = 0; sy < 3; sy++)
                            {
                                Terrain subTerrain = new Terrain(mapTile);
                                mapTile.TerrainSubtiles[sx][sy] = subTerrain;

                                if (mapTile.Terrain.SoilComponents != null)
                                {
                                    foreach (var item in mapTile.Terrain.SoilComponents)
                                    {
                                        subTerrain.AddSoilComponent(item.Key, item.Value.Amount / 9f);
                                    }
                                }

                                if (mapTile.Terrain.Vegetation != null)
                                {
                                    foreach (var item in mapTile.Terrain.Vegetation)
                                    {
                                        subTerrain.AddVegetation(item.Key, item.Value.Amount / 9f);
                                    }
                                }

                                // more??
                                subTerrain.LevelBelowWater = mapTile.Terrain.LevelBelowWater;
                                subTerrain.TerrainDepth = mapTile.Terrain.TerrainDepth;
                                subTerrain.SurfaceType = mapTile.Terrain.SurfaceType;

                            }
                        }

                        mapTile.Terrain = null;
                    }
                });


         /*   for (int x = tileArea.Left; x < tileArea.Right; x++)
            {
                for (int y = tileArea.Top; y < tileArea.Bottom; y++)
                {
                    mapTile = map.TileMap[x][y];

                    if (mapTile.Terrain != null)
                    {
                        Common.InitJaggedArray(ref mapTile.TerrainSubtiles, 3, 3);
                        for (int sx = 0; sx < 3; sx++)
                        {
                            for (int sy = 0; sy < 3; sy++)
                            {
                                Terrain subTerrain = new Terrain(mapTile);
                                mapTile.TerrainSubtiles[sx][sy] = subTerrain;

                                if (mapTile.Terrain.SoilComponents != null)
                                {
                                    foreach (var item in mapTile.Terrain.SoilComponents)
                                    {
                                        subTerrain.AddSoilComponent(item.Key, item.Value.Amount / 9f);
                                    }
                                }

                                if (mapTile.Terrain.Vegetation != null)
                                {
                                    foreach (var item in mapTile.Terrain.Vegetation)
                                    {
                                        subTerrain.AddVegetation(item.Key, item.Value.Amount / 9f);
                                    }
                                }

                                // more??
                                subTerrain.LevelBelowWater = mapTile.Terrain.LevelBelowWater;
                                subTerrain.TerrainDepth = mapTile.Terrain.TerrainDepth;
                                subTerrain.SurfaceType = mapTile.Terrain.SurfaceType;

                            }
                        }

                        mapTile.Terrain = null;
                    }

                }
            }*/

           /* tileProgressX++;
            if (tileProgressX == map.mapTileWidth)
            {
                tileProgressX = 0;
                return true;
            }

            return false;*/
        }

        private static bool ReduceSubdivisionScanVertically(Rectangle tileArea, HashSet<TerrainTile> tilesContainingWaterEdge)
        {
            MapManager map;
            TerrainTile currentTile;
            map = The.Map;

            Terrain terrain;
            //  Point subtilePos;
            // float vertexDepthUnderWater, vertexTerrainDepth;

            //   float xCoord, yCoord;

            // parms:
            int widthInTiles = tileArea.Width; // map.mapTileWidth;
            int heightInTiles = tileArea.Height; // map.mapTileHeight;

            int startTileX = tileArea.Left; // 0;
            int startTileY = tileArea.Top; // 0;
            // parms


            bool isUnderWater;

            int tileY = 0, tileX = 0;
            int subtileYCounter = 0, subtileXCounter = 0;

            int sw = widthInTiles * MapManager.SubtilesPerTileLength; // 3; // map.mapTileWidth * 3;
            int sh = heightInTiles * MapManager.SubtilesPerTileLength; //* 3; // map.mapTileHeight * 3;

            int subtileStartX = startTileX * MapManager.SubtilesPerTileLength;
            int subtileStartY = startTileY * MapManager.SubtilesPerTileLength;

            int subtileEndX = subtileStartX + sw;
            int subtileEndY = subtileStartY + sh;


            // now do a vertical pass:
            tileX = startTileX;
            tileY = startTileY;

            currentTile = map.TileMap[startTileX][startTileY];  //map.TileMap[0][0]; // NEW: forgot to reset after horiz scan..?


            subtileYCounter = 0; // 0 - 2
            subtileXCounter = 0; // 0 - 2
           
            terrain = currentTile.TerrainSubtiles[0][0];
            isUnderWater = terrain.LevelBelowWater > 0f;

            bool currentTerrainIsUnderWater = isUnderWater;


            for (int x = subtileStartX; x < subtileEndX; x++)
            {
                for (int y = subtileStartY; y < subtileEndY; y++)
                {

                    terrain = currentTile.TerrainSubtiles[subtileXCounter][subtileYCounter];

                    currentTerrainIsUnderWater = terrain.LevelBelowWater > 0f;

                    if (y == subtileStartY) // 0)
                    {
                        // reset this variable each time we start at the top:
                        isUnderWater = currentTerrainIsUnderWater;
                    }

                    if (currentTerrainIsUnderWater != isUnderWater)
                    {
                        isUnderWater = currentTerrainIsUnderWater;

                        // mark this tile as containing a water edge:
                        tilesContainingWaterEdge.Add(currentTile);
                    }

                    subtileYCounter++;
                    if (subtileYCounter == 3)
                    {
                        subtileYCounter = 0;
                        tileY++;

                        if (tileY == heightInTiles) // reached the bottom
                        {
                            // start from the top again:
                            tileY = startTileY; // 0;
                        }
                        currentTile = map.TileMap[tileX][tileY];
                    }
                }

                subtileXCounter++;
                if (subtileXCounter == 3)
                {
                    subtileXCounter = 0;
                    tileX++;

                    if (tileX < widthInTiles)
                    {
                        currentTile = map.TileMap[tileX][tileY];
                    }
                }
            }

            return true;
        }

        private static bool ReduceSubdivisionScanHorizontally(Rectangle tileArea, HashSet<TerrainTile> tilesContainingWaterEdge)
        {
            Terrain terrain;
            TerrainTile currentTile;
            MapManager map = The.Map;

            int tileY, tileX;
            int subtileYCounter = 0, subtileXCounter = 0;

            // parms:
            int widthInTiles = tileArea.Width; // map.mapTileWidth;
            int heightInTiles = tileArea.Height; // map.mapTileHeight;

            int startTileX = tileArea.Left; // 0;
            int startTileY = tileArea.Top; // 0;
            // parms

            tileX = startTileX;
            tileY = startTileY;


            int sw = widthInTiles * MapManager.SubtilesPerTileLength; //  map.mapTileWidth * 3;
            int sh = heightInTiles * MapManager.SubtilesPerTileLength; // map.mapTileHeight * 3;

            int subtileStartX = startTileX * MapManager.SubtilesPerTileLength;
            int subtileStartY = startTileY * MapManager.SubtilesPerTileLength;

            int subtileEndX = subtileStartX + sw;
            int subtileEndY = subtileStartY + sh;

            currentTile = map.TileMap[startTileX][startTileY];  //map.TileMap[0][0]; // NEW: forgot to reset after horiz scan..?
          
            terrain = currentTile.TerrainSubtiles[0][0];

            bool isUnderWater = terrain.LevelBelowWater > 0f;

            bool currentTerrainIsUnderWater = isUnderWater;


            // scan subtiles horizontally
            for (int y = subtileStartY; y < subtileEndY; y++)  //for (int y = 0; y < sh; y++)
            {
                for (int x = subtileStartX; x < subtileEndX; x++)  //for (int x = 0; x < sw; x++)
                {

                    terrain = currentTile.TerrainSubtiles[subtileXCounter][subtileYCounter];

                    currentTerrainIsUnderWater = terrain.LevelBelowWater > 0f;

                    if (x == subtileStartX) // 0)
                    {
                        // reset this variable each time we start at the left edge:
                        isUnderWater = currentTerrainIsUnderWater;
                    }

                    if (currentTerrainIsUnderWater != isUnderWater)
                    {
                        isUnderWater = currentTerrainIsUnderWater;

                        // mark this tile as containing a water edge:
                        tilesContainingWaterEdge.Add(currentTile);
                    }

                    subtileXCounter++;
                    if (subtileXCounter == 3)
                    {
                        subtileXCounter = 0;
                        tileX++;

                        if (tileX == tileArea.Right) // widthInTiles) // map.mapTileWidth) // reached the right end tile
                        {
                            // start from the left again:
                            tileX = startTileX; // 0;
                        }
                        currentTile = map.TileMap[tileX][tileY];
                    }
                }

                subtileYCounter++;
                if (subtileYCounter == 3)
                {
                    subtileYCounter = 0;
                    tileY++;

                    if (tileY < tileArea.Bottom) // heightInTiles) 
                    {
                        currentTile = map.TileMap[tileX][tileY];
                    }
                }
            }

            return true;
        }


        /// <summary>
        /// reduce the terrain subdivision where it's not needed (away from coasts)
        /// </summary>
        /// <param name="tile"></param>
        private static void CombineTerrainSubtiles(TerrainTile tile)
        {
            Terrain terrain;

            Terrain combinedTerrain = new Terrain(tile);

            for (int sx = 0; sx < 3; sx++)
            {
                for (int sy = 0; sy < 3; sy++)
                {
                    terrain = tile.TerrainSubtiles[sx][sy];

                    if (terrain.Vegetation != null)
                    {
                        foreach (var kvp in terrain.Vegetation)
                        {
                            if (kvp.Value.Amount > 0f)
                            {
                                // combinedTerrain.AddVegetation(kvp.Value);
                                combinedTerrain.AddVegetation(kvp.Value.LowVegetationType, kvp.Value.Amount);
                            }
                        }
                    }

                    if (terrain.SoilComponents != null)
                    {
                        foreach (var kvp in terrain.SoilComponents)
                        {
                            if (kvp.Value.Amount > 0f)
                            {
                                combinedTerrain.AddSoilComponent(kvp.Value.SoilComponentType, kvp.Value.Amount);
                            }
                        }
                    }

                    // more...?
                }
            }

            CombineTerrain(tile, combinedTerrain);

            // combinedTerrain.Moisture = centerTerrain.Moisture;

            tile.TerrainSubtiles = null;
            tile.Terrain = combinedTerrain;



            tile.Terrain.NormalizeVegetation();
            tile.Terrain.NormalizeSoil();

            if (tile.X == 208 && tile.Y == 38)
            {
                //      throw new Exception();
            }

            tile.Terrain.RecomputeDisplayAmounts();
        }

        private static void CombineTerrain(TerrainTile tile, Terrain combinedTerrain)
        {
            Terrain centerTerrain = tile.GetCenterTerrain();
            combinedTerrain.TerrainDepth = centerTerrain.TerrainDepth; // take the center as height.
            combinedTerrain.LevelBelowWater = centerTerrain.LevelBelowWater;
            combinedTerrain.SurfaceType = centerTerrain.SurfaceType;
        }

        /* private void SplitTerrainTileInSubtiles(TerrainTile tile) // int x, int y)
         {
             //TerrainTile
             if (tile.TerrainSubtiles == null)
             {
                 Common.InitJaggedArray(ref tile.TerrainSubtiles, 3, 3);
             }
         }*/

        private void SetMoistureOnTile(TerrainTile tile)
        {
            Terrain centerTerrain = tile.GetCenterTerrain();

            // scale the moisture towards the water surface:
            if (centerTerrain.LevelBelowWater < 0 && centerTerrain.LevelBelowWater > minimumHeightForMoistureFromWaterSurface)
            {
                tile.Moisture = MathHelper.Lerp(1f, tile.Moisture, centerTerrain.LevelBelowWater / minimumHeightForMoistureFromWaterSurface);
            }
            else if (centerTerrain.LevelBelowWater > 0)
            {
                tile.Moisture = 1f;
            }

        }

        /// <summary>
        /// events should probably belong to a 'scenario', not a map...
        /// </summary>
        /// <param name="mapToLoad"></param>
        /*   private void LoadEvents(MapData mapToLoad) //, float timeOfDay, int day)
           {
               // clear these here..?
               The.Sim.EventManager.ClearEvents();

               if (mapToLoad.PredefinedEvents != null)
               {
                   GlobalConditionalEvent evt;
                   foreach (var e in mapToLoad.PredefinedEvents)
                   {
                       evt = GameData.Instance.AllGlobalEvents[e.Trim()];

                       The.Sim.EventManager.AddGlobalEvent(evt, mapToLoad.StartDate);
                   }

               }

               if (mapToLoad.Events != null)
               {
                   foreach (var e in mapToLoad.Events)
                   {
                       The.Sim.EventManager.AddGlobalEvent(e, mapToLoad.StartDate);

                   }
               }

               The.Sim.EventManager.TimeConditionGameEvents.Sort((e1, e2) => DateAndTime.CompareDates(e1.Condition.TimeCondition.Date.Value, e2.Condition.TimeCondition.Date.Value));

           }*/


        private void InitializeEntityData()
        {
            // only needed in edit mode...
            
                InitializeEntityData(mapData.SavedMapEntities);
                InitializeEntityData(mapData.Trees);
            
        }


        private void InitializeEntityData(List<EntityData> list)
        {
            if (list != null)
            {
                foreach (var item in list)
                {
                    item.PostDataCompleteInitialize();
                }
            }

        }

        private bool LoadSavedMapEntities() 
        {
           
            if (mapData.SavedMapEntities != null)
            {
                 int maxIndex = entityIndex + 100;
                 maxIndex = Common.ClampTop(maxIndex, mapData.SavedMapEntities.Count);

                EntityData entity;
                for (int i = entityIndex; i < maxIndex; i++)
                {
                    entity = mapData.SavedMapEntities[i];

                    bool placementFailed;
                    CreateAndPlaceEntityFromEntityData(entity, out placementFailed);                  
             
                }

                if (maxIndex == mapData.SavedMapEntities.Count)
                {
                    entityIndex = 0;
                    return true;
                }
                else
                {
                    entityIndex = maxIndex;
                    return false;
                }

                /*
                foreach (EntityData savedEntity in mapData.SavedMapEntities)
                {
                    bool placementFailed;
                    CreateAndPlaceEntityFromEntityData(savedEntity, out placementFailed);                  
                }*/
            }

            return true;
        }

        private bool LoadSavedMapTrees()
        {
            if (mapData.Trees != null)
            {
                int maxIndex = entityIndex + 100;
                maxIndex = Common.ClampTop(maxIndex, mapData.Trees.Count);

                EntityData tree;
                for (int i = entityIndex; i < maxIndex; i++)
                {
                    tree = mapData.Trees[i];

                   
                    bool placementFailed;
                    CreateAndPlaceEntityFromEntityData(tree, out placementFailed);
                }

                if (maxIndex == mapData.Trees.Count)
                {
                    entityIndex = 0;
                    return true;
                }
                else
                {
                    entityIndex = maxIndex;
                    return false;
                }

                /*foreach (EntityData tree in mapData.Trees)
                {
                    bool placementFailed;
                    CreateAndPlaceEntityFromEntityData(tree, out placementFailed);
                    //CreateOrAddToSpawnList(timeOfDay, day, tree);
                }*/
            }

            return true;
        }

        private bool LoadTiles(MapData mapToLoad) //, float timeOfDay, int day)
        {
            MapManager map = The.Map;

            if (mapToLoad.Tiles != null)
            {
                Sim.EngineMode mode = The.Sim.Mode;
                TerrainTile terrainTile;

                HashSet<string> designerResources = new HashSet<string>();

                foreach (var tile in mapToLoad.Tiles)
                {
                    if (tile.Resources != null)
                    {
                        terrainTile = map.TileMap[tile.Position.X][tile.Position.Y];

                        foreach (var resource in tile.Resources)
                        {
                            resource.PostDataCompleteInitialize();

                            designerResources.Add(resource.KeyName);
                        }

                        // only save this data in Editor mode...?
                        if (mode == Sim.EngineMode.Edit)
                        {
                            terrainTile.DesignerPlacedResources = tile.Resources;
                        }

                        if (mode == Sim.EngineMode.Game)
                        {
                            // create random tile resources:
                            SetRandomTileResourcesFromProbabilities(tile.Resources, terrainTile);
                        }

                        //  map.TileMap[tile.Position.X][tile.Position.Y].DesignerPlacedResources = tile.Resources.ToList();
                    }
                }

                foreach (var item in designerResources)
                {
                    The.Sim.PlaySite.EditorResources.Add(GameData.Instance.AllResourceTypes[item]);
                }

            }

           

            return true;
        }

       /* private void CreateOrAddToSpawnList(double timeOfDay, int day, EntityData savedEntity)
        {
            // if in game mode, check the spawn time before creating the entity:
            if (The.Sim.Mode == Sim.EngineMode.Game)
            {
                if (savedEntity.SpawnDate.HasValue) 
                {
                    if (DateAndTime.DateIsAfter(savedEntity.SpawnDate.Value.TimeOfDay, savedEntity.SpawnDate.Value.Day, timeOfDay, day))
                    {
                        The.Sim.EntitiesToSpawn.Add(savedEntity); // save for later...

                        return;
                    }
                }
            }

            bool failedToPlace;
            CreateAndPlaceEntityFromEntityData(savedEntity, out failedToPlace);
        }*/

        /*
        /// <summary>
        /// TODO: add Site as param
        /// </summary>
        /// <param name="entityData"></param>
        /// <param name="addToContainer"></param>
        /// <param name="locationToUse"></param>
        public static EntityID? CreateAndPlaceEntityFromEntityData(EntityData entityData, Entity addToContainer = null, Vector3? locationToUse = null)
        {
          
            EntityType entityType;
            if (!GameData.Instance.AllEntityTypes.TryGetValue(entityData.EntityKey, out entityType))
                return null;

            Entity entity = Entity.Produce(entityType);

            SetEntityDataBeforeInitialize(entityData, entity);

           
            // NEW: get expedition too - take AllegianceAndExpedition as argument
            Allegiances.Allegiance allegiance;
            Expedition expedition;
            GetOrCreateAllegianceAndExpedition(entityData, entity, out allegiance, out expedition);


            ThreatGroup threatGroup = GetThreatGroup(entityData, entity);
            Overland.Site site = The.Sim.PlaySite;

            if (allegiance != null)
            {
                site = allegiance.Site;
            }

            entity.Initialize(site, allegiance, threatGroup); // this adds to allegiance

            if (entity.IsOnPlaySite())
            {
                entity.InitializeModelAndOnScreenFunctionality(The.Sim.ScreenManager.Game);
            }

            IOwner owner = null;

            // we can either be a member or be owned by an expedtion
            if (entityData.MemberOf != null)
            {
                System.Diagnostics.Debug.Assert(entity.EntityType.IntelligenceType != null);

                expedition.AddMember(entity);
            }
            else if (entityData.OwnedBy != null)
            {
                if (entityData.Person == null)  // only set an owner on non-persons:
                {                   
                    owner = expedition;              
                }
            }


            // place the finished entity (must be done before setting ownership):     
            if (entity.IsOnPlaySite()) // .Site == The.Sim.PlaySite)
            {
                entity.PlaceNewEntityInWorld(locationToUse ?? entityData.Location,
                       addToContainer, true, owner);
            }
            else if (entityData.OwnedBy != null)
            {
                // for items owned by othersite entities and allegiances:
                entity.ChangeOwnership(owner);       
            }

            entity.ComeOnline(); // start the AI we need

            // also sets ownership:
            SetEntityDataAfterInitialize(entityData, entity);

            return entity.EntityID;
           
                       
        }
        */

        /// <summary>
        /// also calls ComeOnline()
        /// 
        /// called when loading a map in editor or starting a new game, and when firing spawn events.
        /// 
        /// location, owning, member keys etc. are separate parameters because EntityData is supposed to be immutable...
        /// </summary>
        /// <param name="entityData"></param>
        /// <param name="addToContainer"></param>
        /// <param name="locationToUse"></param>
        /// <param name="owningAllegianceKey"></param>
        /// <param name="owningExpeditionKey"></param>
        /// <param name="memberOfAllegianceKey"></param>
        /// <param name="memberOfExpeditionKey"></param>
        /// <returns></returns>
        public static Entity CreateAndPlaceEntityFromEntityData(EntityData entityData, out bool placementFailed, 
            Entity addToContainer = null, // overrides entityData
            StorageCondition placeInStorage = null,
            bool offerForSale = false,
            bool isProductionOutput = false,
            Vector3? locationToUse = null,
            string owningAllegianceKey = null, string owningExpeditionKey = null,
            string memberOfAllegianceKey = null, string memberOfExpeditionKey = null, 
            string siteKey = null,
            EntityID? anchorID = null,
            bool assertContainment = true,
            UpgradeCategory upgradeCategory = null,
            float? age = null,
            string caste = null,
            bool logProductionStatistics = false,
            bool suppressSpawningEvents = false)
        {
            placementFailed = false;

            EntityType entityType;
            if (!GameData.Instance.AllEntityTypes.TryGetValue(entityData.EntityKey, out entityType))
                return null;
            

            Entity entity;
            entity = new Entity(entityType);

           
            SetEntityDataBeforeInitialize(entityData, entity, age, caste);

            string owningAllegianceKeyToUse = null, owningExpeditionKeyToUse = null, memberOfAllegianceKeyToUse = null, memberOfExpeditionKeyToUse = null;                      
            
            // these can be overridden
            if (entityData.OwnedBy != null)
            {
                owningAllegianceKeyToUse = entityData.OwnedBy.AllegianceKey;
                owningExpeditionKeyToUse = entityData.OwnedBy.ExpeditionKey;
            }            
            
            if (entityData.MemberOf != null)
            {
                memberOfAllegianceKeyToUse = entityData.MemberOf.AllegianceKey;
                memberOfExpeditionKeyToUse = entityData.MemberOf.ExpeditionKey;
            }

            // overriding params:
            if (memberOfExpeditionKey != null)
            {
                memberOfExpeditionKeyToUse = memberOfExpeditionKey;
            }
            if (memberOfAllegianceKey != null)
            {
                memberOfAllegianceKeyToUse = memberOfAllegianceKey;
            }
            if (owningExpeditionKey != null)
            {
                owningExpeditionKeyToUse = owningExpeditionKey;
            }
            if (owningAllegianceKey != null)
            {
                owningAllegianceKeyToUse = owningAllegianceKey;
            }


            // NEW: get expedition too - take keys as argument instead of EntityData
            Allegiances.Allegiance allegiance = null;
            Expedition expedition = null;

            if (The.Sim.Mode == Sim.EngineMode.Game)
            {
                // don't create allegiances in Edit mode
                GetOrCreateAllegianceAndExpedition(entity.EntityType, out allegiance, out expedition,
                    entityData.Location.GetValueOrDefault(),
                    owningAllegianceKeyToUse, owningExpeditionKeyToUse,
                    memberOfAllegianceKeyToUse, memberOfExpeditionKeyToUse);

            }
            else if (The.Sim.Mode == Sim.EngineMode.Edit)
            {
                // just save the name...
                EditorData editorData;
                entity.Find(out editorData);
                /*
                if (entityData.MemberOf != null)
                {
                    editorData.AllegianceKey = entityData.MemberOf.AllegianceKey;
                    editorData.ExpeditionName = entityData.MemberOf.ExpeditionKey;
                }
                */
                if (memberOfAllegianceKey != null)
                {
                    editorData.AllegianceKey = memberOfAllegianceKey;
                }
                if (memberOfExpeditionKey != null)
                {
                    editorData.ExpeditionName = memberOfExpeditionKey;
                }
            }

            ThreatGroup threatGroup = GetThreatGroup(entityData, entity);

            Overland.Site site;
            if (siteKey != null)
            {
                site = The.Sim.World.AllSites[siteKey];
            }
            else if (allegiance != null)
            {
                site = allegiance.Site;
            }
            else
            {
                site = The.Sim.PlaySite;
            }
            

            entity.Initialize(site, allegiance, expedition, threatGroup); // this adds to allegiance

            if (entity.IsOnPlaySite())
            {
                entity.InitializeModelAndOnScreenFunctionality();
            }

            IOwner owner = null;

            //OLD? we can either be a member or be owned by an expedtion
            // robots and dogs are both members and owned
            if (entityData.MemberOf != null || memberOfExpeditionKey != null)
            {
                /* System.Diagnostics.Debug.Assert(entity.EntityType.IntelligenceType != null);

                 expedition.AddMember(entity);*/
            }
            /*else*/
            
            if (entityData.OwnedBy != null || owningExpeditionKey != null)
            {
                if (entityData.Person == null)  // only set an owner on non-persons:
                {
                    owner = expedition;
                }
            }

            StorageCompartment? compartment = null;
            if (offerForSale)
            {
                compartment = StorageCompartment.OfferedForTrade;
            }

            Entity.SetOwnerInfo setOwnerInfo = new Entity.SetOwnerInfo(owner);

            // place the finished entity (must be done before setting ownership):     
            if (entity.IsOnPlaySite()) 
            {
                bool simulateJoinedNow = false;
                if (entityData.Person != null)
                {
                    simulateJoinedNow = entityData.Person.SimulateJoinedExpeditionNow;
                }

                if (logProductionStatistics && owner != null)
                {
                    // log for the product owner
                    entity.SpawnedByOwner = owner.ID; 

                }
               
                if (!entity.PlaceEntityOnPlaySite(locationToUse ?? entityData.Location,
                    addToContainer, Entity.StructureState.Finished, setOwnerInfo, expedition, placeProductsInCompartment: compartment, placeInStorage: placeInStorage, anchorID: anchorID, 
                    assertContainment: assertContainment, simulateJoinedExpeditionNow: simulateJoinedNow, upgradeCategory: upgradeCategory, isProductionOutput: isProductionOutput))
                {
                    placementFailed = true;
                    return entity;
                }
            }
            else //if (entityData.OwnedBy != null || owningExpeditionKey != null)
            {
                // for items owned by othersite entities and allegiances:              
                bool simulateJoinedNow = false;
                if (entityData.Person != null)
                {
                    simulateJoinedNow = entityData.Person.SimulateJoinedExpeditionNow;
                }

                entity.PlaceEntityOnOtherSite(site, addToContainer, true, setOwnerInfo, expedition, compartment: compartment, placeInStorage: placeInStorage,
                    simulateJoinedExpeditionNow: simulateJoinedNow);
            }

            entity.ComeOnline(suppressSpawningEvents); // start the AI we need

           
            SetEntityDataAfterInitialize(entityData, entity);

            return entity;
        }

        public static void SetEntityDataAfterInitialize(EntityData entityData, Entity entity)
        {
            if (entityData.Person != null)
            {
                Entities.Person personComponent = entity.PersonEntity;
                
                // set portrait after initialize:
                if (!string.IsNullOrEmpty(entityData.Person.Portrait))
                {
                    personComponent.UpdatePortrait(The.InGameUI.gui, entityData.Person.Portrait);
                }

            }


            if (entityData.Resources != null
                || (entity.EntityType.TreeType != null && entity.EntityType.TreeType.DefaultCrops != null)) // resources set on this instance
            {
                DefaultCrops[] defaultResources = null;
                if (entity.EntityType.TreeType != null && entity.EntityType.TreeType.DefaultCrops != null)
                {
                    defaultResources = entity.EntityType.TreeType.DefaultCrops;
                }

               /* if (The.Sim.Mode == Sim.EngineMode.Game)
                { */  // we only set crops in the game, not in the editor.
                    SetRandomCropsFromResourceProbabilities(entityData.Resources, defaultResources, entity);
               // }

            }

            if (entityData.NeedLevels != null)
            {
                Needs needs = entity.BiologicalEntity.Needs;
                foreach (var item in entityData.NeedLevels)
                {
                    if (item.Value.Level != null)
                    {
                        needs.NeedsList[item.Key].CurrentLevel = (float)item.Value.Level.GetRandomValue(The.Sim.GameplayRandomGenerator);
                    }

                    if (item.Value.DaysAtZero != null)
                    {
                        needs.NeedsList[item.Key].PhysicalNeed.DaysAtZero = (float)item.Value.DaysAtZero.GetRandomValue(The.Sim.GameplayRandomGenerator);
                    }
                }
            }
            else
            {
                if (entity.BiologicalEntity != null)
                {
                    // NEW: random needs are default:
                    foreach (var need in entity.BiologicalEntity.Needs.NeedsList)
                    {
                        need.Value.CurrentLevel = (float)NormalDistribution.GetRandomValue(The.Sim.GameplayRandomGenerator, 0.6f, 0.1f);
                    }
                }
            }

            if (entityData.Properties != null)
            {
                foreach (var item in entityData.Properties)
                {
                    entity.SetPropertyValue(item.Key, item.Value);
                }
            }

            if (entityData.EffectProfiles != null)
            {
                foreach (var item in entityData.EffectProfiles)
                {
                    EffectProfileType effect = GameData.Instance.AllEffectProfileTypes[item];
                    entity.SimEffects.Start(effect);
                }
            }

            if (entityData.BioEntity != null)
            {
                UWGame.SimSide.Entities.Biological.BiologicalEntity bioComponent;
                entity.Find(out bioComponent);

                if (entityData.BioEntity.StomachContent != null)
                {
                    bioComponent.SetStomachContents((float)entityData.BioEntity.StomachContent.GetRandomValue(The.Sim.GameplayRandomGenerator));
                }
            }
        }


        /// <summary>
        /// fills the Entity with data from the Entity Data object.
        /// Also creates or retrieves allegiance/threat group for the entity
        /// </summary>
        /// <param name="entityData"></param>
        /// <param name="entity"></param>
        /// <param name="allegiance"></param>
        public static void SetEntityDataBeforeInitialize(EntityData entityData, Entity entity, float? age = null, string caste = null)
        {
            // allegiance = null;

            entity.FlipHorizontally = entityData.FlipHorizontally;

            if (/*entity.EntityType.TreeType == null &&*/ entityData.Bulk.HasValue) 
            {
                entity.Bulk = entityData.Bulk.Value;
            }

            entity.Name = entityData.Name;


            entity.Add(new EditorData(entity)
            {               
                Resources = entityData.Resources
            });

            if(The.Sim.Mode == Sim.EngineMode.Edit
                && entityData.Resources != null)
            {
                foreach (var item in entityData.Resources)
                {
                    The.Sim.PlaySite.EditorResources.Add(item.ResourceType);
                }
            }



            if (entity.Locomotor != null && entityData.Rotation.HasValue)
            {
                entity.SetRotationAndDir(MathHelper.ToRadians(entityData.Rotation.Value));
            }

            if (entityData.Threat != null)
            {

            }


            if (entityData.Tree != null)
            {
                Trees.Tree treeComponent;
                entity.Find(out treeComponent);

                entityData.Tree.SetTreeComponentPreInit(treeComponent);
               
            }
            else if (entity.EntityType.BiologicalType != null) // entityData.BioEntity != null)
            {
                Entities.Biological.BiologicalEntity bioComponent;
                entity.Find(out bioComponent);
                                
                if (entityData.BioEntity != null) 
                {
                    entityData.BioEntity.FillEntity(entity, age, caste);                   
                }
                else
                {
                    // caste must be set..
                    if (caste != null)
                    {
                        bioComponent.SetCasteOnNewEntity(caste); 
                    }
                    else
                    {
                        bioComponent.SetRandomCaste(); 
                    }

                    // race can be null.

                    bioComponent.SetAgePreInit(age); // set random age or defined age            
                }               
            }


            if (entityData.Person != null)
            {               

                entityData.Person.FillEntity(entity);
            }

        }


        /// <summary>
        /// gets or creates an allegiance
        /// </summary>
        /// <param name="entityData"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        private static void GetOrCreateAllegianceAndExpedition(EntityType entityType,
            out Allegiance allegiance, out Expedition expedition,
            Vector3 location,
            string owningAllegianceKey = null, string owningExpeditionKey = null,
            string memberOfAllegianceKey = null, string memberOfExpeditionKey = null)
        {
            allegiance = null;
            expedition = null;

            /*  if (The.Sim.Mode == Sim.EngineMode.Game)
              {   // don't create allegiances in Edit mode.
              */
            string allegianceKey = null;
            string expeditionKey = null;
            /*
            if (entityData.MemberOf != null)
            {
                allegianceKey = entityData.MemberOf.AllegianceKey;
                expeditionKey = entityData.MemberOf.ExpeditionKey;
            }
            else if (entityData.OwnedBy != null)
            {
                allegianceKey = entityData.OwnedBy.AllegianceKey;
                expeditionKey = entityData.OwnedBy.ExpeditionKey;
            }
            */

            if (owningAllegianceKey != null)
            {
                allegianceKey = owningAllegianceKey;
            }
            else if (memberOfAllegianceKey != null)
            {
                allegianceKey = memberOfAllegianceKey;
            }

            if (owningExpeditionKey != null)
            {
                expeditionKey = owningExpeditionKey;
            }
            else if (memberOfExpeditionKey != null)
            {
                expeditionKey = memberOfExpeditionKey;
            }

            // NEW: only get an allegiance if the key was specified
            if (!string.IsNullOrEmpty(allegianceKey))
            {
                allegiance = FindOrCreateAllegiance(entityType, allegianceKey);
            }


            if (allegiance != null)
            {
                //NEW: if allegiance is defined, we require an expedition too.

                expedition = FindOrCreateExpedition(allegiance, location, expeditionKey);

            }

            // }
            //else
            //{
            //    // just save the name...
            //    EditorData editorData;
            //    entity.Find(out editorData);
            //    /*
            //    if (entityData.MemberOf != null)
            //    {
            //        editorData.AllegianceKey = entityData.MemberOf.AllegianceKey;
            //        editorData.ExpeditionName = entityData.MemberOf.ExpeditionKey;
            //    }
            //    */if (memberOfAllegianceKey != null)
            //    {
            //        editorData.AllegianceKey = memberOfAllegianceKey;
            //    }
            //    if (memberOfExpeditionKey != null)
            //    {
            //        editorData.ExpeditionName = memberOfExpeditionKey;
            //    }
            //}

        }

        private static Expedition FindOrCreateExpedition(Allegiance allegiance, Vector3 location, string expeditionKey) // expeditionName)
        {
            Expedition expedition = null;

            // currently the AI evaluators need an expedition to have been set in order to Initialize:
            if (!string.IsNullOrEmpty(expeditionKey))
            {
                expedition = allegiance.GetExpedition(expeditionKey);

            }
            else
            {
                // if no expedition name is given, pick the frist one as default:
                expedition = allegiance.GetFirstExpedition();
            }

            if (expedition == null)
            {
                // if none exist, create one.
                expedition = new Expedition(allegiance,
                                            expeditionKey ?? allegiance.KeyName,
                                            expeditionKey ?? allegiance.KeyName,
                                            location);
            }

            return expedition;
        }

        private static Allegiances.Allegiance FindOrCreateAllegiance(EntityType entityType, string allegianceKey, string allegianceName = null, Allegiances.AllegianceType allegianceType = Allegiances.AllegianceType.Other)
        {

            Allegiances.Allegiance allegiance = null;

            if (allegianceKey != null)
            {
                allegiance = The.Sim.World.GetAllegianceFromKey(allegianceKey);
            }

            if (allegiance == null)
            {
                allegiance = new Allegiances.Allegiance(allegianceType, entityType, allegianceKey, allegianceName);


                // human allegiance?
                if (entityType.Person != null)
                {
                    allegiance.HumanActivities = new Allegiances.HumanActivities();
                }

            }

            return allegiance;
        }



        private static ThreatGroup GetThreatGroup(EntityData saved, Entity entity)
        {
            ThreatGroup threatGroup = null;

            if (The.Sim.Mode == Sim.EngineMode.Game)
            {   // don't create allegiances in Edit mode.


                if (saved.Threat != null)
                {

                    threatGroup = ThreatGroup.GetThreatGroup(saved.Threat.ThreatGroupName);
                }
            }
            else
            {
                // just save the name...
                EditorData editorData;
                entity.Find(out editorData);

                if (saved.Threat != null)
                {
                    editorData.ThreatGroupName = saved.Threat.ThreatGroupName;
                }
            }

            return threatGroup;
        }

        private static void SetRandomCropsFromResourceProbabilities(Resource[] resources, DefaultCrops[] defaultCrops, Entity entity)
        {
            Trees.Tree treeComponent;
            entity.Find(out treeComponent);

          
            if (treeComponent != null)
            {
                if (treeComponent.Crops != null)
                {
                    Resource resource = null;
                    DefaultCrops defaultCrop = null;
                    foreach (KeyValuePair<ResourceType, Crop> kvp in treeComponent.Crops)
                    {
                        resource = null;
                        defaultCrop = null;

                        if (resources != null) // first see if a resource has been specified on the instance level
                        {
                            resource = resources.FirstOrDefault(r => r.KeyName == kvp.Key.KeyName);
                        }

                        if (defaultCrops != null)// see if a default resource has been set
                        {
                            defaultCrop = defaultCrops.FirstOrDefault(r => r.KeyName == kvp.Key.KeyName);
                        }

                        if (resource != null && resource.AbsoluteMeanItems.HasValue)
                        {
                            kvp.Value.SetRandomCrops(entity.Bulk / entity.EntityType.TreeType.BulkPerSize, resource.AbsoluteMeanItems.Value, resource.AbsoluteStandardDeviation.Value);
                        }
                        else if (defaultCrop != null)
                        {
                            float? mean;
                            float? stdDev;

                            if (resource != null && resource.Modifier.HasValue)
                            {
                                // apply the modifier:
                                Common.GetNormalDistributionFromMinMaxValues(
                                    (0.01f * resource.Modifier.Value) * defaultCrop.MinItemsForFullGrownPlant.Value,
                                    (0.01f * resource.Modifier.Value) * defaultCrop.MaxItemsForFullGrownPlant.Value,
                                    out mean, out stdDev);
                            }
                            else
                            {
                                mean = defaultCrop.AbsoluteMeanItems;
                                stdDev = defaultCrop.AbsoluteStandardDeviationItems;
                            }

                            kvp.Value.SetRandomCrops(entity.Bulk / entity.EntityType.TreeType.BulkPerSize, mean.Value, stdDev.Value);
                        }
                    }
                }
            }
        }

        private static void SetRandomTileResourcesFromProbabilities(Resource[] resources, TerrainTile tile)
        {
            foreach (var resource in resources)
            {
                if (resource.AbsoluteMeanItems.HasValue)
                {
                    int noOfResourceItems = (int)Math.Round(
                       (The.Sim.GameplayRandomGenerator.RandomNormalDistribution((double)resource.AbsoluteMeanItems, (double)resource.AbsoluteStandardDeviation)),
                       MidpointRounding.ToEven);

                    tile.AddResource(resource.KeyName, noOfResourceItems);


                }
                else if (resource.Modifier.HasValue && resource.Modifier.Value != 100)
                {
                    // modify existing resource amount (such as firewood)
                    if (tile.TileResources != null)
                    {
                        // TileResourceContainer container = tile.TileResources.FirstOrDefault(r => r.ResourceType.KeyName == resource.KeyName);
                        TileResourceContainer container;
                        ResourceType resourceType = GameData.Instance.AllResourceTypes[resource.KeyName];
                        if (tile.TileResources.TryGetValue(resourceType, out container))
                        {
                            container.SetTotalHarvestableBulk(0.01f * (float)resource.Modifier.Value * container.TotalHarvestableBulk);
                        }
                    }
                }
            }

            /*  if (treeComponent.Crops != null)
              {
                  Resource resource;
                  DefaultCrops defaultCrop;
                  foreach (KeyValuePair<ResourceType, Crop> kvp in treeComponent.Crops)
                  {
                      resource = resources.FirstOrDefault(r => r.KeyName == kvp.Key.KeyName);
                      if (resource != null && resource.AbsoluteMeanItems.HasValue) // first see if a resource has been specified on the instance level
                      {
                          kvp.Value.SetRandomCrops(entity.Bulk / entity.EntityType.TreeType.BulkPerSize, resource.AbsoluteMeanItems.Value, resource.AbsoluteStandardDeviation.Value);
                      }
                      else if (defaultCrops != null)
                      {
                          defaultCrop = defaultCrops.FirstOrDefault(r => r.KeyName == kvp.Key.KeyName);
                          if (defaultCrop != null) // if not, then see if a default resource has been set
                          {
                              float? mean;
                              float? stdDev;

                              if (resource != null && resource.Modifier.HasValue)
                              {
                                  // apply the modifier:
                                  Common.GetNormalDistributionFromMinMaxValues(
                                      (0.01f * resource.Modifier.Value) * defaultCrop.MinItemsForFullGrownPlant.Value,
                                      (0.01f * resource.Modifier.Value) * defaultCrop.MaxItemsForFullGrownPlant.Value,
                                      out mean, out stdDev);
                              }
                              else
                              {
                                  mean = defaultCrop.AbsoluteMeanItems;
                                  stdDev = defaultCrop.AbsoluteStandardDeviationItems;
                              }

                              kvp.Value.SetRandomCrops(entity.Bulk / entity.EntityType.TreeType.BulkPerSize, mean.Value, stdDev.Value);

                           
                          }
                      }
                  }
              }*/

        }

        /// <summary>
        /// no longer used...?
        /// </summary>
        /// <param name="mapData"></param>
        private void LoadTreeBitmapsAndPlaceTrees(MapData mapData) //, int terrainWidth, int terrainLength, Color[] treeMapColors, Color[] treeSpeciesColors)
        {
            MapManager map = The.Map;

            string folderPath = ComposePath("Trees"); // MapManager.ComposeMapDataFolderPath(mapData.FolderName, "Trees");

            Dictionary<string, Color[]> treeTextures;
            Dictionary<string, Texture2D> treeTexture2ds;
            LoadBitmapsInFolder(folderPath, out treeTextures, out treeTexture2ds);

            TerrainTile currentTile;
            int noOfSpecies = 4;
            float speciesRange = 256f / (float)noOfSpecies;
            float currentTreeSizeDensity = 0f;

            List<Point> tileOffsets = new List<Point>{new Point(0, 0), new Point(0, 1), new Point(0, 2), new Point(1, 0), new Point(1, 1), new Point(1, 2), 
                                              new Point(2, 0), new Point(2, 1), new Point(2, 2)};
            int noOfValidTiles;

            int treesPlanted = 0;

            foreach (KeyValuePair<string, Color[]> kvp in treeTextures)
            {
                EntityType treeType;
                treesPlanted = 0;

                if (GameData.Instance.AllTreeTypes.TryGetValue("tree:" + kvp.Key, out treeType))
                {
                    //OK, Tree data was found. Plant it at random places...

                    for (int y = 0; y < map.mapTileHeight; y = y + 3)
                    {
                        for (int x = 0; x < map.mapTileWidth; x = x + 3) // move by threes
                        {

                            currentTreeSizeDensity = 0f;
                            noOfValidTiles = 0;

                            // count valid tiles:
                            Point tileToLookAt;
                            for (int i = 0; i < tileOffsets.Count; i++)
                            {
                                tileToLookAt = tileOffsets[i];
                                tileToLookAt.X += x;
                                tileToLookAt.Y += y;

                                if (tileToLookAt.X < 0 || tileToLookAt.X > map.mapTileWidth - 1
                                    || tileToLookAt.Y < 0 || tileToLookAt.Y > map.mapTileHeight - 1)
                                {
                                    continue;
                                }

                                currentTile = map.TileMap[tileToLookAt.X][tileToLookAt.Y];
                                if (currentTile.TerrainSubtiles[1][1].LevelBelowWater < minimumHeightForTreesAndGrass)
                                {
                                    noOfValidTiles++;
                                }
                            }


                            //randomize positions:
                            tileOffsets = Common.Randomize<Point>(tileOffsets, The.Sim.GameplayRandomGenerator);


                            // the tree map texture is independent of the terrain width and height!                   
                            int treeMapValueAtCurrentPosition = ReadColor(x, y, kvp.Value, treeTexture2ds[kvp.Key]).R; //kvp.Value[(int)(relx * treeMapTexture.Width) + (int)(rely * (treeMapTexture.Height - 1)) * treeMapTexture.Width].R;

                            float treeDensity;
                            if (treeMapValueAtCurrentPosition > 200)
                                treeDensity = 12;
                            else if (treeMapValueAtCurrentPosition > 150)
                                treeDensity = 8;
                            else if (treeMapValueAtCurrentPosition > 50)
                                treeDensity = 4;
                            else
                                treeDensity = 0;

                            if (treeDensity > 0)
                            {

                                /*   int species;
                                   if (Globals.Instance.Random.Next(10) < 8)
                                   {
                                       // int treeSpeciesValueAtCurrentPosition = treeSpeciesColors[(int)(relx * treeSpeciesMapTexture.Width) + (int)(rely * treeSpeciesMapTexture.Height) * terrainWidth].R;
                                       int treeSpeciesValueAtCurrentPosition = treeSpeciesColors[(int)(relx * treeSpeciesMapTexture.Width) + (int)(rely * (treeSpeciesMapTexture.Height - 1)) * treeSpeciesMapTexture.Width].R;
                                       treeSpeciesValueAtCurrentPosition += Globals.Instance.Random.Next(-5, 5);

                                       species = (int)((float)treeSpeciesValueAtCurrentPosition / speciesRange);
                                   }
                                   else
                                   {   // random tree...
                                       species = Globals.Instance.Random.Next(noOfSpecies);
                                   }
                                   */
                                Entity treeToPlant;

                                // go to max 6 trees per tile...
                                for (int j = 0; j < /*noOfValidTiles **/ 6; j++)
                                {

                                    if (currentTreeSizeDensity > treeDensity)
                                    {
                                        break;
                                    }

                                    for (int i = 0; i < tileOffsets.Count; i++)
                                    {
                                        tileToLookAt = tileOffsets[i];
                                        tileToLookAt.X += x;
                                        tileToLookAt.Y += y;

                                        if (tileToLookAt.X < 0 || tileToLookAt.X > map.mapTileWidth - 1
                                            || tileToLookAt.Y < 0 || tileToLookAt.Y > map.mapTileHeight - 1)
                                        {
                                            continue;
                                        }

                                        currentTile = map.TileMap[tileToLookAt.X][tileToLookAt.Y];
                                        if (currentTile.GetCenterTerrain().LevelBelowWater < minimumHeightForTreesAndGrass) // poll the center of the tile
                                        {


                                            Point position = new Point(currentTile.X, currentTile.Y);
                                            Vector3 location = MapManager.TileToWorldPos(position);

                                            location = location + new Vector3(MapManager.tileSizeOver2 - The.Sim.GameplayRandomGenerator.Next(MapManager.tileSize, "MapLoader"),
                                                                        MapManager.tileSizeOver2 - The.Sim.GameplayRandomGenerator.Next(MapManager.tileSize, "MapLoader"), 0f);
                                            //}
                                            location = map.ClampWorldPosition(location);

                                            Common.Direction edge = The.Map.WorldLocationToDirectionWithinTile(location);

                                            // we may only have one tree per edge/subtile...
                                          
                                            if (!currentTile.ContainsTreeAtSubtile(MapManager.WorldPosToSubtile(location))) 
                                            {
                                                treeToPlant = new Entity(treeType); 
                                                
                                                treeToPlant.Initialize(The.Sim.PlaySite);

                                                treeToPlant.PlaceEntityOnPlaySite(location, null, null, null);

                                                treeToPlant.ComeOnline();

                                                Trees.Tree treeComp;
                                                treeToPlant.Find(out treeComp);


                                                currentTreeSizeDensity += treeToPlant.EntityType.TreeType.SizeImpact;

                                                treesPlanted++;
                                            }

                                            if (currentTreeSizeDensity > treeDensity)
                                            {
                                                break;
                                            }
                                        }
                                    }
                                }
                            }

                        }
                    }
                }

            }


            return;

            /*  TerrainTile currentTile;
              int noOfSpecies = 4;
              float speciesRange = 256f / (float)noOfSpecies;
              float currentTreeSizeDensity = 0f;

              List<Point> tileOffsets = new List<Point>{new Point(0, 0), new Point(0, 1), new Point(0, 2), new Point(1, 0), new Point(1, 1), new Point(1, 2), 
                                                new Point(2, 0), new Point(2, 1), new Point(2, 2)};
              int noOfValidTiles;
              for (int y = 0; y < terrainLength; y = y + 3)
              {
                  for (int x = 0; x < terrainWidth; x = x + 3) // move by threes
                  {

                      currentTreeSizeDensity = 0f;
                      noOfValidTiles = 0;

                      // count valid tiles:
                      Point tileToLookAt;
                      for (int i = 0; i < tileOffsets.Count; i++)
                      {
                          tileToLookAt = tileOffsets[i];
                          tileToLookAt.X += x;
                          tileToLookAt.Y += y;

                          if (tileToLookAt.X < 0 || tileToLookAt.X > mapWidth - 1
                              || tileToLookAt.Y < 0 || tileToLookAt.Y > mapHeight - 1)
                          {
                              continue;
                          }

                          currentTile = TileMap[tileToLookAt.X, tileToLookAt.Y];
                          if (currentTile.LevelBelowWater < minimumHeightForTreesAndGrass)
                          {
                              noOfValidTiles++;
                          }
                      }


                      //randomize positions:
                      tileOffsets = Common.Randomize<Point>(tileOffsets, Globals.Instance.Random);

                      float relx = (float)x / (float)mapWidth;
                      float rely = (float)y / (float)mapHeight;

                      // the tree map texture is independent of the terrain width and height!                   
                      int treeMapValueAtCurrentPosition = treeMapColors[(int)(relx * treeMapTexture.Width) + (int)(rely * (treeMapTexture.Height - 1)) * treeMapTexture.Width].R;
                      float treeDensity;
                      if (treeMapValueAtCurrentPosition > 200)
                          treeDensity = 12;
                      else if (treeMapValueAtCurrentPosition > 150)
                          treeDensity = 8;
                      else if (treeMapValueAtCurrentPosition > 50)
                          treeDensity = 4;
                      else
                          treeDensity = 0;

                      if (treeDensity > 0)
                      {

                          int species;
                          if (Globals.Instance.Random.Next(10) < 8)
                          {
                              // int treeSpeciesValueAtCurrentPosition = treeSpeciesColors[(int)(relx * treeSpeciesMapTexture.Width) + (int)(rely * treeSpeciesMapTexture.Height) * terrainWidth].R;
                              int treeSpeciesValueAtCurrentPosition = treeSpeciesColors[(int)(relx * treeSpeciesMapTexture.Width) + (int)(rely * (treeSpeciesMapTexture.Height - 1)) * treeSpeciesMapTexture.Width].R;
                              treeSpeciesValueAtCurrentPosition += Globals.Instance.Random.Next(-5, 5);

                              species = (int)((float)treeSpeciesValueAtCurrentPosition / speciesRange);
                          }
                          else
                          {   // random tree...
                              species = Globals.Instance.Random.Next(noOfSpecies);
                          }

                          Tree treeToPlant;

                          for (int i = 0; i < tileOffsets.Count; i++)
                          {
                              tileToLookAt = tileOffsets[i];
                              tileToLookAt.X += x;
                              tileToLookAt.Y += y;

                              if (tileToLookAt.X < 0 || tileToLookAt.X > mapWidth - 1
                                  || tileToLookAt.Y < 0 || tileToLookAt.Y > mapHeight - 1)
                              {
                                  continue;
                              }

                              currentTile = TileMap[tileToLookAt.X, tileToLookAt.Y];
                              if (currentTile.LevelBelowWater < minimumHeightForTreesAndGrass)
                              {
                                  noOfValidTiles++;

                                  switch (species)
                                  {
                                      case 0:
                                          treeToPlant = new Tree(GameData.Instance.AllTreeTypes["tree:shadeleaf"]);
                                          break;
                                      case 1:
                                          treeToPlant = new Tree(GameData.Instance.AllTreeTypes["tree:spoak"]); 
                                          //treeToPlant.Flavour = 2;
                                          break;
                                      case 2:
                                          treeToPlant = new Tree(GameData.Instance.AllTreeTypes["tree:gianthollow"]); 
                                          break;
                                      case 3:
                                          treeToPlant = new Tree(GameData.Instance.AllTreeTypes["tree:daysheen"]); 
                                          break;
                                      default:
                                          treeToPlant = new Tree(GameData.Instance.AllTreeTypes["tree:shadeleaf"]); 
                                          break;
                                  }

                                  Point position = new Point(currentTile.X, currentTile.Y);
                                  Vector3 pos = TileToWorldPos(position);
                               
                                  pos = pos + new Vector3(tileSizeOver2 - Globals.Instance.Random.Next(tileSize),
                                                              tileSizeOver2 - Globals.Instance.Random.Next(tileSize), 0f);
                                  //}
                                  pos = ClampWorldPosition(pos);

                                  Common.Direction edge = UWGame.SimSide.Instance.Map.WorldLocationToDirectionWithinTile(pos);

                                  // we may only have one tree per edge/subtile...
                                  if (!currentTile.ContainsTree(edge))
                                  {
                                      treeToPlant.PlaceTree(pos, edge);
                                      currentTile.AddTree(treeToPlant, edge);

                                      currentTreeSizeDensity += treeToPlant.TreeType.SizeImpact;
                                  }

                                  if (currentTreeSizeDensity > treeDensity)
                                  {
                                      break;
                                  }
                              }
                          }
                      }

                  }
              }*/

        }
    }
}
