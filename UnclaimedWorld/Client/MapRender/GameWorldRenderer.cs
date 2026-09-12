using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using SpriteSheetRuntime;
using UWGame.ClientSide.Interface;
using UWGame.SimSide.Items;
using Microsoft.Xna.Framework.Input;
using UWGame.SimSide.Soil;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using UWGame.SimSide.Vegetation;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using UWGame.SimSide.AI;
using UWGame.SimSide.Trees;
using UWGame.SimSide.Entities.Body;
using UWGame.SimSide.Resources;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide;
using UWGame.SimSide.Maps;
using UWGame.ClientSide.Map.Water;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.Entities.Containers;
using UWGame.Client.MapRender;
using UWGame.SimSide.Jobs;
using InputEventSystem;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Entities.Containers.Components;


namespace UWGame.ClientSide.Map
{
    public class GameWorldRenderer
    {
        // private MapManager map;
        private Sim sim;

        public Dictionary<string, LightSourceType> LightSourceTypes;

        public Water.Water Water;


        DayAndNightEffects DayAndNightEffects;

        public Effect TimeOfDayLightingEffect;

        //public Texture2D alphaTex;

        public Texture2D Scanlines;
        public Texture2D OverlayGradient;

        public Effect CloudShadowsEffect;

        //public Dictionary<EntityType, Entity> boxModels = new Dictionary<EntityType, Entity>();
        //  private Dictionary<string, Queue<Entity>> attachableEntities = new Dictionary<string, Queue<Entity>>();

        private Dictionary<string, Queue<Renderable>> attachableRenderables = new Dictionary<string, Queue<Renderable>>();



        // drawable objects sorted by Y world coordinate
        public List<List<ILocatable>> sortedObjectsToDraw = new List<List<ILocatable>>();
        private List<LightSource> lightSourcesToDraw = new List<LightSource>();

        private List<ILocatable> shadowsToDraw = new List<ILocatable>();
        private MapResourceRenderer mapResourceRenderer = new MapResourceRenderer();
        //  private List<ILocatable> groundSpritesToDraw = new List<ILocatable>();

        //private List<List<Entity>> // 
        // List<Entity> ghostedStructuresToDraw = new List<Entity>();

        // private VertexMultitextured[] terrainVertices;

        // use 16 bit indices to support older cards:
        //  private short[] terrainIndices;

        private Effect GroundFeatureEffect;
        public VertexGroundFeature[] groundFeatureVertices;
        private int groundFeatureQuadIndex = 0;

        private short[] groundFeatureIndices;
        // VertexDeclaration roadsAndPathsVertexDeclaration;

        /// <summary>
        /// sets index buffer size too (shared between vertex types)
        /// </summary>
        public const int noOfFeatureQuads = 15000; // 10000;

        /// <summary>
        /// they use the same index buffer:
        /// </summary>
        private const int noOfRoadQuads = noOfFeatureQuads; 

        private const int noOfLightSourceQuads = 200;
        private const int noOfOverlayQuads = 2000;

        /// <summary>
        /// cannot be greater than noOfFeatureQuads - this sets index buffer
        /// </summary>
        private const int noOfInfluenceQuads = 10000; 

        private List<Renderable> overlayModelEntities = new List<Renderable>();

        private List<Renderable> lightEmittingModels = new List<Renderable>();

        private VertexFeatureQuad[] featureVertices;

       // public VertexFeatureQuad[] outlineVertices;
     
        private VertexLightSourceQuad[] lightSourceVertices;
        private VertexOverlayQuad[] overlayVertices;
        private VertexOverlayGroundSpriteQuad[] overlayGroundSpriteVertices;

        private int influenceMapQuadIndex = 0;
        private VertexOverlayGroundSpriteQuad[] influenceMapVertices;

        //  private static Pool<TerrainTilePosition> freeTerrainTilePositions = new Pool<TerrainTilePosition>(800);

        // use 16 bit indices to support older cards:
        public /*int[]*/ short[] featureIndices;

        /* XNA 3
        private VertexDeclaration quadVertexDeclaration;
        private VertexDeclaration lightSourceQuadVertexDeclaration;
        private VertexDeclaration overlayQuadVertexDeclaration;
        private VertexDeclaration overlayGroundSpriteQuadVertexDeclaration;
        */
        private int overlayQuadIndex = 0;
        private int overlayGroundSpriteQuadIndex = 0;

        private short[] lightSourceIndices;


        private Plane noClippingPlane;

        //   VertexDeclaration terrainVertexDeclaration;

        const int terrainsPerBatch = 3; //4;


        //region /* TERRAIN*/
        public Matrix TerrainViewMatrix;
        public Vector3 TerrainCameraPosition;

        public int noOfVerticesHorizontal; // = noOfTilesToDisplayHorizontally + 2;
        public int noOfVerticesVertical;

        private const int xTilesToIncludeInDraw = 2;
        private const int yBottomTilesToIncludeInDraw = 4;
        private const int yTopTilesToIncludeInDraw = 2;

        //    public LightSourceSpriteSheet LightSourcesSpriteSheet;
        //     public ExtendedSpriteSheet BillboardSpriteSheet;
        public SpriteSheet GhostedStructuresSpriteSheet;

        // Custom rendertargets for edge detection on models.
        /// <summary>
        /// is now multisampled, SaveAsPng does not work, instead save the resolved rt
        /// 
        /// </summary>
        public RenderTarget2D DiffuseMSRenderTarget;
               
        /// <summary>
        /// used for lighting shader
        /// </summary>
        private RenderTarget2D diffuseRenderTarget;

        /// <summary>
        /// the final scene that will be blitted to the back buffer
        /// </summary>
        private RenderTarget2D diffuseFinalRenderTarget;

        private RenderTarget2D edgeDetectNormalDepthRenderTarget;

        private RenderTarget2D shadowRenderTarget;
        private RenderTarget2D emissiveModelLightRenderTarget;
        private RenderTarget2D emissiveModelLightDistanceRenderTarget;

      
        /// <summary>
        /// x: 10 bits - DistanceFromViewer - used in lighting (LightSourcesEffect) to determine if shapes are occluding the light sources
        /// y: 10 bits - Height over ground - not currently used
        /// z: 10 bits - Billboard Alpha - used in CloudShadows.fx to overlay shadows
        /// </summary>
        public RenderTarget2D DistanceHeightAndBillboardAlphaRenderTarget;

        //  public Effect BlendTerrainEffect;
        public Effect terrainEffect;
        public Effect billboardEffect;
        public Effect lightSourceEffect;
        private Effect overlayEffect;
        private Effect overlayGroundSpritesEffect;

        public double CameraViewingAngle;
        public double CosCameraViewingAngle;
        public float modelYCorrectionFactor; // = Math.Cos(MathHelper.PiOver2 - CameraViewingAngle);

        public Vector3 CameraTarget;
        public Vector3 CameraDirection = new Vector3(0f, -1200f, 1000f); //new Vector3(0f, -5f, 1000f); //
        public Vector3 CameraPosition;

        public Matrix View;

        private BlendState lightsBlendAdd = new BlendState()
        {
            ColorBlendFunction = BlendFunction.Add,
            ColorSourceBlend = Blend.One,
            ColorDestinationBlend = Blend.One
        };

        private BlendState overlayBlendState = new BlendState()
        {
            ColorSourceBlend = Blend.SourceAlpha,
            ColorDestinationBlend = Blend.One
        };

        private Dictionary<string, Texture2D> terrainTextures = new Dictionary<string, Texture2D>();

        List<string> textureParams = new List<string>();

        /// <summary>
        /// define the drawing area, gets assigned at start of each Draw
        /// </summary>
        public int TileStartX;
        public int TileEndX;
        public int TileStartY;
        public int TileEndY;


        /*   public Texture2D grassTexture;
           public Texture2D firegrassTexture;
           public Texture2D muckrootTexture;

           public Texture2D sandTexture;*/

        public Texture2D perlinTexture, perlinBigTexture;

        //public Texture2D SelectedTileTexture;
        private Texture2D cloudShadowTexture;

        private Vector4 TimeOfDayLightingFactor;

        BloomComponent bloom;

        private const float amountToLowerBloomThresholdAtDawn = 0.5f;
        private const float amountToRaiseBloomIntensityAtDawn = 0.8f;

        private const float amountToLowerBloomThresholdAtSunset = 0.4f;
        private const float amountToRaiseBloomIntensityAtSunset = 0.4f;

        private float windTime;

        public List<TerrainBatch> terrainBatches = new List<TerrainBatch>();

        // PerformanceCounter ramTest = new PerformanceCounter("Process", "Working Set", Process.GetCurrentProcess().ProcessName);


        public int TerrainSliceSize = 1024;
        public TerrainSlicedMap terrainSlicedMap = null;



      //  public RenderResourceOutlinesState renderResourceOutlineState = RenderResourceOutlinesState.None;//.Default;


        // To keep things efficient, the picking works by first applying a bounding
        // sphere test, and then only bothering to test each individual triangle
        // if the ray intersects the bounding sphere. This allows us to trivially
        // reject many models without even needing to bother looking at their triangle
        // data. This field keeps track of which models passed the bounding sphere
        // test, so you can see the difference between this approximation and the more
        // accurate triangle picking.
        //    List<string> insideBoundingSpheres = new List<string>();

        public EntityID? PickedModel;

        public RasterizerState rasterizerStateWireframe;

        public List<Renderable> renderablesFadingOut = new List<Renderable>();
        private HashSet<Renderable> previouslyDrawnRenderablesThatCanFade = new HashSet<Renderable>();
        private HashSet<Renderable> currentlyDrawnRenderablesThatCanFade = new HashSet<Renderable>();

        private HashSet<Renderable> previouslyDrawnRenderablesThatCanLerp = new HashSet<Renderable>();
        private HashSet<Renderable> currentlyDrawnRenderablesThatCanLerp = new HashSet<Renderable>();

        public List<RenderAsBillboard> OverlayBillboards = new List<RenderAsBillboard>();


        public GameWorldRenderer()
        {
            sim = The.Sim;

            Water = new Water.Water(this);

            DayAndNightEffects = new Map.DayAndNightEffects();

            textureParams.Add("texture0");
            textureParams.Add("texture1");
            textureParams.Add("texture2");
            textureParams.Add("texture3");

            CosCameraViewingAngle = Vector3.Dot(CameraDirection, Vector3.Down) / (CameraDirection.Length() * Vector3.Down.Length());
            CameraViewingAngle = Math.Acos(CosCameraViewingAngle);
            modelYCorrectionFactor = (float)Math.Cos(MathHelper.PiOver2 - CameraViewingAngle);

            UpdateTerrainViewMatrix();


            rasterizerStateWireframe = new RasterizerState() { CullMode = CullMode.None, FillMode = FillMode.WireFrame };

            Dimension drawArea = The.Client.Controller.DrawArea;
            Viewport deviceViewport = The.Client.GraphicsDevice.Viewport;
            DrawAreaViewport = new Viewport(0, 0, drawArea.Width, drawArea.Height, deviceViewport.MinDepth, deviceViewport.MaxDepth);
            
            The.Client.InitializeSpriteBatch();

            //freeTerrainTilePositions = new Pool<TerrainTilePosition>(100);
        }

        // for fix-up after other important classes like Client have constructed
        public void Init()
        {
            noClippingPlane = CreatePlane(4000f/*WaterHeight - 20f*/, new Vector3(0, 0, -1), true); //false);

        }



        public void LoadContent()
        {
           
            GraphicsDevice device = The.Client.GraphicsDevice;
            PresentationParameters pp = device.PresentationParameters;

            int width = The.Client.Controller.DrawArea.Width; // pp.BackBufferWidth
            int height = The.Client.Controller.DrawArea.Height; // pp.BackBufferHeight

            // #MONOCHANGE before: (with framework changes, now enables AA again)
            DiffuseMSRenderTarget = new RenderTarget2D(device,
                 width, height, false,
                  pp.BackBufferFormat, pp.DepthStencilFormat, pp.MultiSampleCount, RenderTargetUsage.PreserveContents);
                  

          /*  DiffuseMSRenderTarget = new RenderTarget2D(device,
                width, height, false,
                 pp.BackBufferFormat, pp.DepthStencilFormat, 1, RenderTargetUsage.PreserveContents);
                 */

            // resolve target - not needed?
            /*diffuseRenderTarget = new RenderTarget2D(device, width, height, false,
                pp.BackBufferFormat, pp.DepthStencilFormat, 0, RenderTargetUsage.PreserveContents);
            */
            

            edgeDetectNormalDepthRenderTarget = new RenderTarget2D(device,
                                                         width, height, false,
                                                         pp.BackBufferFormat, pp.DepthStencilFormat, 0, RenderTargetUsage.PreserveContents);


            // no depth buffer, no multisampling!
            shadowRenderTarget = new RenderTarget2D(device,
                width, height, false,
                pp.BackBufferFormat, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);

            // no depth buffer, no multisampling!
            diffuseRenderTarget = new RenderTarget2D(device,
                width, height, false,
                pp.BackBufferFormat, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);

            diffuseFinalRenderTarget = new RenderTarget2D(device,
               width, height, false,
               pp.BackBufferFormat, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);

            emissiveModelLightRenderTarget = new RenderTarget2D(device,
                width, height, false,
                pp.BackBufferFormat, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);

            emissiveModelLightDistanceRenderTarget = new RenderTarget2D(device,
                width, height, false, SurfaceFormat.Rg32
                , DepthFormat.None, 0, RenderTargetUsage.DiscardContents);

            DistanceHeightAndBillboardAlphaRenderTarget = new RenderTarget2D(device,
                // NEW: Use 3 components of 10 bits each. // does the reordering (?) of bytes mean anything???
                   width, height, false, SurfaceFormat.Rgba1010102, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);

            // This one is 2 components of 16 bits each.
            //   pp.BackBufferWidth, pp.BackBufferHeight, 1, SurfaceFormat.Rg32, MultiSampleType.None, 0);

            // INSTEAD OF THIS:
            /*DepthRenderTarget = new RenderTarget2D(device,
                pp.BackBufferWidth, pp.BackBufferHeight, 1, pp.BackBufferFormat, MultiSampleType.None, 0);
            */


            GhostedStructuresSpriteSheet = The.Client.Content.Load<SpriteSheet>("GhostedBuildings");
            //    SaveTextureToFile("GhostedStructures.png", GhostedStructuresSpriteSheet.Texture);
        //    SaveTextureToFile("BillboardSpriteSheet.png", GameData.Instance.BillboardSpriteSheet.Texture);

            //LightSourcesSpriteSheet = The.Client.Content.Load<LightSourceSpriteSheet>("LightSources");
            //GameData.Instance.AllLightSourceTypes = LightSourcesSpriteSheet.AllLightSourceData;

            Scanlines = The.Client.Content.Load<Texture2D>("GUI\\CRT_ScanLines");
            OverlayGradient = The.Client.Content.Load<Texture2D>("overlayGradient");

            //grassTexture = The.Client.Content.Load<Texture2D>("terrain\\t_greengrass_base");
            terrainTextures.Add("greengrass", The.Client.Content.Load<Texture2D>("terrain\\t_greengrass_base"));  
            terrainTextures.Add("earth", The.Client.Content.Load<Texture2D>("terrain\\t_earth_base")); //"terrain\\grid_test")); //
            //terrainTextures.Add("clay", The.Client.Content.Load<Texture2D>("terrain\\t_earth_base"));
            terrainTextures.Add("muckroot", The.Client.Content.Load<Texture2D>("terrain\\muckroot_base"));
            terrainTextures.Add("muckrootthin", The.Client.Content.Load<Texture2D>("terrain\\t_muckrootthin_base"));
            terrainTextures.Add("billowgrass", The.Client.Content.Load<Texture2D>("terrain\\t_billowgrass_base"));

            terrainTextures.Add("firegrass", The.Client.Content.Load<Texture2D>("terrain\\firegrass_base"));
            terrainTextures.Add("sand", The.Client.Content.Load<Texture2D>("terrain\\t_sand_base"));
            terrainTextures.Add("vulcanic", The.Client.Content.Load<Texture2D>("terrain\\t_vulcanic_base"));
            terrainTextures.Add("rocks", The.Client.Content.Load<Texture2D>("terrain\\t_rocks_base")); //grid_test")); //

            terrainTextures.Add("limestone", The.Client.Content.Load<Texture2D>("terrain\\t_limestone_base"));
            terrainTextures.Add("humus", The.Client.Content.Load<Texture2D>("terrain\\t_humus_base"));
            terrainTextures.Add("seabed", The.Client.Content.Load<Texture2D>("terrain\\t_seabed_base"));
            terrainTextures.Add("deepseabed", The.Client.Content.Load<Texture2D>("terrain\\t_deepseabed_base"));
            terrainTextures.Add("linear gradient normal map", The.Client.Content.Load<Texture2D>("terrain\\t_rocks_base_depthmap"));

            //   firegrassTexture = The.Client.Content.Load<Texture2D>("terrain\\firegrass_base");  //"terrain\\t_orangegrass_base"); 
            //   muckrootTexture = The.Client.Content.Load<Texture2D>("terrain\\muckroot_base");  //"terrain\\t_orangegrass_base");             
            //    sandTexture = The.Client.Content.Load<Texture2D>("terrain\\t_earth_base"); //sand_2");

            perlinTexture = The.Client.Content.Load<Texture2D>("perlin_2");
            perlinBigTexture = The.Client.Content.Load<Texture2D>("perlinMedium"); //"perlinBig");

            terrainEffect = The.Client.Content.Load<Effect>("multiTex");
            billboardEffect = The.Client.Content.Load<Effect>("billboard");
            lightSourceEffect = The.Client.Content.Load<Effect>("LightSourcesEffect");
            overlayEffect = The.Client.Content.Load<Effect>("OverlayEffect");
            overlayGroundSpritesEffect = The.Client.Content.Load<Effect>("OverlayGroundSpriteEffect");


            Water.LoadContent();

            DayAndNightEffects.LoadContent();

            TimeOfDayLightingEffect = The.Client.Content.Load<Effect>("TimeOfDayAndLightsources");

            //alphaTex = The.Client.Content.Load<Texture2D>("alphaTex");

            cloudShadowTexture = The.Client.Content.Load<Texture2D>("CloudShadowTexture5"); //"clouds");
            CloudShadowsEffect = The.Client.Content.Load<Effect>("CloudShadows");

            GroundFeatureEffect = The.Client.Content.Load<Effect>("RoadsAndPaths");


        }

        public void Destroy()
        {
            // avoid GPU memory leaks:
            edgeDetectNormalDepthRenderTarget.Dispose();
            emissiveModelLightDistanceRenderTarget.Dispose();
            emissiveModelLightRenderTarget.Dispose();
            DistanceHeightAndBillboardAlphaRenderTarget.Dispose();
            //diffuseRenderTarget.Dispose();
            DiffuseMSRenderTarget.Dispose();
            diffuseRenderTarget.Dispose();
            shadowRenderTarget.Dispose();

            if (bloom != null)
            {
                bloom.Destroy();
            }

            Water.Destroy();

            if (terrainSlicedMap != null)
            {
                terrainSlicedMap.Destroy();
            }
        }

        /// <summary>
        /// not called when saving/loading!
        /// </summary>
        public void UnloadContent()
        {
            // Very important to avoid getting more and more components when going to the start menu and back:
            /*  if (bloom != null)
              {
                  The.Sim.ScreenManager.Game.Components.Remove(bloom); BLOOMCHANGE
              }*/

            if (bloom != null)
            {
                bloom.UnloadContent();
            }

            Water.UnloadContent();

        }

        public void PostLoadContent()
        {
            if (The.Client.BloomEnabled)
            {
                bloom = new BloomComponent(The.Sim.Controller.Game);
                // bloom.Enabled = false; //BLOOMCHANGE
                // call bloom with Draw():
                //   bloom.Visible = false; // BLOOMCHANGE

                // we must remember to remove it again.
                //  game.ScreenManager.Game.Components.Add(bloom); BLOOMCHANGE

                // subtle bloom - copy the settings:
                //bloom.BaseSettings = BloomSettings.PresetSettings[5];
                bloom.BaseSettings = BloomSettings.PresetSettings[6]; // morten tweak bloom ....Select Bloom recipe to use (counts from 0)
                bloom.Settings = new BloomSettings(bloom.BaseSettings.Name, bloom.BaseSettings.BloomThreshold, bloom.BaseSettings.BlurAmount, bloom.BaseSettings.BloomIntensity,
                bloom.BaseSettings.BaseIntensity, bloom.BaseSettings.BloomSaturation, bloom.BaseSettings.BloomSaturation);
            }


            // terrainVertices = new VertexMultitextured[(noOfVerticesHorizontal) * (noOfVerticesVertical)];
            // terrainIndices = new short[(noOfVerticesHorizontal - 1) * (noOfVerticesVertical - 1) * 6];

            // terrainVertexDeclaration = new VertexDeclaration(The.Client.GraphicsDevice, VertexMultitextured.VertexElements);
            // we only have to do this once:
            // SetUpTerrainIndices(terrainIndices);

            // these vertex buffers use the same index buffer:
            AssertVertexbufferAndIndexBufferMatch(noOfRoadQuads, noOfFeatureQuads);
            AssertVertexbufferAndIndexBufferMatch(noOfInfluenceQuads, noOfFeatureQuads);
            AssertVertexbufferAndIndexBufferMatch(noOfOverlayQuads, noOfFeatureQuads);


            // roads and paths:
            groundFeatureVertices = new VertexGroundFeature[noOfRoadQuads * 4];
            groundFeatureIndices = new short[noOfRoadQuads * 6];
            // roadsAndPathsVertexDeclaration = new VertexDeclaration(The.Client.GraphicsDevice, VertexRoadAndPath.VertexElements);
            SetUpIndices(noOfRoadQuads, groundFeatureIndices);

            // features: buildings, trees etc:
            featureVertices = new VertexFeatureQuad[noOfFeatureQuads * 4];
            featureIndices = new short[noOfFeatureQuads * 6];
            SetUpIndices(noOfFeatureQuads, featureIndices);

            // light sources:
            lightSourceVertices = new VertexLightSourceQuad[noOfLightSourceQuads * 4];
            lightSourceIndices = new short[noOfLightSourceQuads * 6];
            SetUpIndices(noOfLightSourceQuads, lightSourceIndices);

            overlayVertices = new VertexOverlayQuad[noOfOverlayQuads * 4];
            overlayGroundSpriteVertices = new VertexOverlayGroundSpriteQuad[noOfOverlayQuads * 4];

            influenceMapVertices = new VertexOverlayGroundSpriteQuad[noOfInfluenceQuads * 4];

            //outlineVertices = new VertexFeatureQuad[noOfFeatureQuads * 4];
          

            mapResourceRenderer.PostLoadContent();


            if (terrainSlicedMap == null)
            {
                terrainSlicedMap = new TerrainSlicedMap();
                terrainSlicedMap.Init();
            }

        }

        int rightRenderEdge;
        int bottomRenderEdge;

        public void InitAfterMapLoad()
        {
            // build the terrain indices etc.           
            noOfVerticesHorizontal = The.MapUI.noOfTilesToDisplayHorizontally + 4; //2;
            noOfVerticesVertical = The.MapUI.noOfTilesToDisplayVertically + 4; // 2;            

            rightRenderEdge = The.Map.mapTileWidth + GutterSize;
            bottomRenderEdge = The.Map.mapTileHeight + GutterSize;

            sortedObjectsToDraw.Clear();
            for (int i = 0; i <= The.MapUI.noOfTilesToDisplayVertically + yBottomTilesToIncludeInDraw + yTopTilesToIncludeInDraw; i++)
            {
                sortedObjectsToDraw.Add(new List<ILocatable>());
            }

            Water.Initialize();

            CreateTerrainTilePositions();
            mapResourceRenderer.Init();
        }

        /*  private TerrainTile GetClosestTileOnActualMap(int tilePosX, int tilePosY)
          {
              int closestXOnMap = Common.Clamp(tilePosX, 0, map.mapWidth - 1);
              int closestYOnMap = Common.Clamp(tilePosY, 0, map.mapHeight - 1);
                       
              return map.TileMap[closestXOnMap][closestYOnMap];
          }*/

        /*  Dictionary<int, Dictionary<int, TerrainTilePosition>> gutterTiles = new Dictionary<int, Dictionary<int, TerrainTilePosition>>();
          private void CreateGutterTiles(int gutterSize)
          {
              // top edge:
              for (int x = 0; x < map.mapWidth; x++)
              {
                  for (int y = -gutterSize; y < 0; y++)
                  {
                    

                  }
              }
          }*/

        TerrainTilePosition[][] terrainTilePositions;
        Dictionary<TerrainTile, List<TerrainTilePosition>> tilesToPositions = new Dictionary<TerrainTile,List<TerrainTilePosition>>(); // one-to-many used by editor


        /// <summary>
        /// create wrapper objects for the tiles and terrain for use in rendering (optimization)
        /// </summary>
        /// <param name="gutterSize"></param>
        public void CreateTerrainTilePositions()
        {
            Common.InitJaggedArray(ref terrainTilePositions, The.Map.mapTileWidth + 2 * GutterSize, The.Map.mapTileHeight + 2 * GutterSize);

            for (int x = -GutterSize; x < The.Map.mapTileWidth + GutterSize; x++)
            {
                for (int y = -GutterSize; y < The.Map.mapTileHeight + GutterSize; y++)
                {
                    int closestXOnMap = Common.Clamp(x, 0, The.Map.mapTileWidth - 1);
                    int closestYOnMap = Common.Clamp(y, 0, The.Map.mapTileHeight - 1);

                    TerrainTile tile = The.Map.TileMap[closestXOnMap][closestYOnMap];


                    CreateTerrainTilePositions(x, y, tile);

                    //SetTerrainTilePosition(x, y, ttp);
                }
            }
        }

        /// <summary>
        /// pass coords as well as the tile because positions in the gutter will be pointing to the closest tile.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="tile"></param>
        public void CreateTerrainTilePositions(int x, int y, TerrainTile tile)
        {     
            TerrainTilePosition ttp = new TerrainTilePosition(x, y, tile);
            ttp.X = x;
            ttp.Y = y;

            ttp.RecomputeTerrainTile();

            terrainTilePositions[x + GutterSize][y + GutterSize] = ttp;
            Common.AddToMultiList(tilesToPositions, tile, ttp);   // several positions may point to the same tile! (gutter)
        }


        public void RecomputeTerrainTilePositions(TerrainTile tile)
        {
            List<TerrainTilePosition> positions;
            if (tilesToPositions.TryGetValue(tile, out positions))
            {
                foreach (var item in positions)
                {
                    item.RecomputeTerrainTile();
                }

            }
        }
       

        private TerrainTilePosition GetTerrainTilePosition(int x, int y)
        {
            return terrainTilePositions[x + GutterSize][y + GutterSize];
        }


        /*
        private void SetTerrainTilePosition(int x, int y, TerrainTilePosition tp)
        {
            terrainTilePositions[x + GutterSize][y + GutterSize] = tp;
         }*/

        /* private TerrainTilePosition GetClosestTileOnActualMap(int tilePosX, int tilePosY)
         {
             int closestXOnMap = Common.Clamp(tilePosX, 0, map.mapWidth - 1);
             int closestYOnMap = Common.Clamp(tilePosY, 0, map.mapHeight - 1);

             TerrainTilePosition tp = freeTerrainTilePositions.Get();

             tp.TerrainTile = map.TileMap[closestXOnMap][closestYOnMap];
             tp.X = tilePosX;
             tp.Y = tilePosY;

             return tp;
                       
         }*/

        class TerrainTilePosition
        {
            public TerrainTile TerrainTile;
            public int X;
            public int Y;

            //public Vector3 RenderPosition;
            //public Vector2 RenderTextureCoordinate;

            public TerrainPosition TerrainPosition;
            public TerrainPosition[][] TerrainSubtilePositions;

            public TerrainTilePosition(int x, int y, TerrainTile closestTile)
            {
                this.X = x;
                this.Y = y;

                this.TerrainTile = closestTile;
            }

            /// <summary>
            /// sets and computes the data for the terrain tile we are pointing at
            /// </summary>
            /// <param name="tile"></param>
            public void RecomputeTerrainTile() //TerrainTile tile)
            {
                if (TerrainTile.Terrain != null)
                {
                    TerrainPosition tp = new TerrainPosition(this, TerrainTile.Terrain);
                    TerrainPosition = tp;
                }
                else
                {
                    Common.InitJaggedArray(ref TerrainSubtilePositions, 3, 3);
                    for (int sx = 0; sx < 3; sx++)
                    {
                        for (int sy = 0; sy < 3; sy++)
                        {
                            TerrainPosition tp = new TerrainPosition(this, TerrainTile.TerrainSubtiles[sx][sy], sx, sy);
                            TerrainSubtilePositions[sx][sy] = tp;
                        }
                    }
                }
            }

        }

        private static void AssertVertexbufferAndIndexBufferMatch(int vertextBufferSizeInQuads, int indexBufferSizeInQuads)
        {
            if (vertextBufferSizeInQuads > indexBufferSizeInQuads)
            {
                throw new Exception("Vertex buffer should not be larger than the index buffer.");
            }
        }

        class TerrainPosition
        {
            public Terrain Terrain;
            // public int X;
            // public int Y;

            public Vector3 RenderPosition;
            public Vector2 RenderTextureCoordinate;

            public TerrainTilePosition Parent;

            private const float oneOverTextureSize = 1f / 512f;

            public TerrainPosition(/*int x, int y,*/ TerrainTilePosition parent, Terrain terrain, int? sx = null, int? sy = null)
            {
                /* this.X = x;
                 this.Y = y;
                 */
                this.Parent = parent;
                this.Terrain = terrain;

                ComputeTerrainPosition(sx, sy);
            }

            private void ComputeTerrainPosition(int? sx = null, int? sy = null)
            {
                int x = Parent.X;
                int y = Parent.Y;
                float xPos, yPos;

                // if (!terrain.IsSubtileTerrain())
                if (!Terrain.IsSubtileTerrain()) // tile.Value.TerrainTile.Terrain != null)
                {
                    // terrain = tile.Value.TerrainTile.Terrain;
                    MapManager.TileToWorldPos(x, y, out xPos, out yPos);
                }
                else
                {
                    // int? sx, sy;
                    //  GetSubtileCoords(out sx, out sy);

                    xPos = x * MapManager.tileSize + sx.Value * MapManager.subTileSize + MapManager.subTileSizeOver2;
                    yPos = y * MapManager.tileSize + sy.Value * MapManager.subTileSize + MapManager.subTileSizeOver2;

                }

                RenderPosition = new Vector3(xPos, yPos, MapManager.TerrainZLevel + Terrain.TerrainDepth);

                RenderTextureCoordinate.X = xPos * oneOverTextureSize;
                RenderTextureCoordinate.Y = yPos * oneOverTextureSize;
            }
        }

        private void CreateTerrainTrianglesToTheRightAndDown(int x, int y, int lastXToDraw, int lastYToDraw, TerrainBatch batch) //, List<VertexMultitextured> listOfTerrainVertices, List<ushort> listOfTerrainIndices)
        {
            // int closestXOnMap = Common.Clamp(tilePosX, 0, map.mapWidth - 1);
            //  int closestYOnMap = Common.Clamp(tilePosY, 0, map.mapHeight - 1);

            TerrainTilePosition tile, rightTile = null, downRightTile = null, downTile = null;

            // when we are drawing from outside the map (the gutter/border), this is the tile that we copy...
            tile = GetTerrainTilePosition(x, y); // GetClosestTileOnActualMap(tilePosX, tilePosY);

            //TerrainTile tile = map.TileMap[tilePosX][tilePosY];

            // TerrainTile rightTile = null, downRightTile = null, downTile = null;

            /*   if (x == 95 && y == 118)
               {
                   tile.TerrainTile.Moisture += 0.0001f;
               }*/

            if (x < lastXToDraw)
            {
                int rightX = x + 1;
                if (rightX < rightRenderEdge)
                {
                    rightTile = GetTerrainTilePosition(rightX, y); // GetClosestTileOnActualMap(tilePosX + 1, tilePosY);
                }
            }

            if (y < lastYToDraw)
            {
                int downY = y + 1;
                if (downY < bottomRenderEdge)
                {
                    downTile = GetTerrainTilePosition(x, downY); // GetClosestTileOnActualMap(tilePosX, tilePosY + 1);

                    if (rightTile != null)
                    {
                        downRightTile = GetTerrainTilePosition(x + 1, downY); // GetClosestTileOnActualMap(tilePosX + 1, tilePosY + 1);
                    }
                }
            }

            // The indices are specified in clockwise order 
            //- because XNA is a right-handed system, triangles drawn in counter-clockwise order are assumed to be facing away from the camera, and are automatically culled by default.
            // - specify vertices in clockwise order too!
            if (tile.TerrainTile.Terrain != null)
            {
                // single terrain
                if (rightTile != null)
                {
                    ConnectSingleTerrainToTheRight(batch, tile, rightTile, downRightTile);
                }

                if (downTile != null)
                {
                    ConnectSingleTerrainDown(batch, tile, downTile, downRightTile);
                }
            }
            else
            {
                CreateInnerSubtiles(batch, tile);

                if (rightTile != null)
                {
                    ConnectSubtileTerrainToTheRight(batch, tile, rightTile, downRightTile, downTile);
                }

                if (downTile != null)
                {
                    ConnectSubtileTerrainDown(batch, tile, downTile);
                }
            }
        }

        private static void ConnectSubtileTerrainToTheRight(TerrainBatch batch, TerrainTilePosition tile, TerrainTilePosition rightTile, TerrainTilePosition downRightTile, TerrainTilePosition downTile)
        {

            if (downRightTile == null || downTile == null)
                return;//nothing to do MLo

            if (rightTile.TerrainTile.Terrain != null)
            {
                // connect to single terrain:
                // create 2 triangles
                SetupTerrainVertex(tile, 2, 0, batch); // this tile, upper subtile
                SetupTerrainVertex(rightTile, null, null, batch); // right tile
                SetupTerrainVertex(tile, 2, 1, batch); // this tile, middle subtile

                SetupTerrainVertex(tile, 2, 1, batch); // this tile, middle subtile
                SetupTerrainVertex(rightTile, null, null, batch); // right tile
                SetupTerrainVertex(tile, 2, 2, batch); // this tile, lower subtile

                // connect down right - 1st triangle:
                if (downRightTile.TerrainTile.Terrain != null)
                {
                    // connect to single terrain:
                    SetupTerrainVertex(tile, 2, 2, batch); // this tile, lower subtile
                    SetupTerrainVertex(rightTile, null, null, batch); // right tile
                    SetupTerrainVertex(downRightTile, null, null, batch); // downRightTile

                }
                else
                {
                    // connect to upper left subtile of down right tile:
                    SetupTerrainVertex(tile, 2, 2, batch); // this tile, lower subtile
                    SetupTerrainVertex(rightTile, null, null, batch); // right tile
                    SetupTerrainVertex(downRightTile, 0, 0, batch); // downRightTile, upper left subtile    


                }

            }
            else
            {
                // connect to subtiles
                // create 4 triangles
                SetupTerrainVertex(tile, 2, 0, batch); // this tile, upper subtile
                SetupTerrainVertex(rightTile, 0, 0, batch); // right tile
                SetupTerrainVertex(rightTile, 0, 1, batch);

                SetupTerrainVertex(tile, 2, 0, batch); // this tile, upper subtile
                SetupTerrainVertex(rightTile, 0, 1, batch);
                SetupTerrainVertex(tile, 2, 1, batch);


                SetupTerrainVertex(tile, 2, 1, batch); // this tile, middle subtile
                SetupTerrainVertex(rightTile, 0, 1, batch); // right tile
                SetupTerrainVertex(rightTile, 0, 2, batch);

                SetupTerrainVertex(tile, 2, 1, batch); // this tile, middle subtile
                SetupTerrainVertex(rightTile, 0, 2, batch);
                SetupTerrainVertex(tile, 2, 2, batch);

                // connect down right:
                if (downRightTile.TerrainTile.Terrain != null)
                {
                    // connect to single terrain:
                    SetupTerrainVertex(tile, 2, 2, batch); // this tile, lower subtile
                    SetupTerrainVertex(rightTile, 0, 2, batch); // right tile
                    SetupTerrainVertex(downRightTile, null, null, batch); // downRightTile
                }
                else
                {
                    // connect to upper left subtile of down right tile:
                    SetupTerrainVertex(tile, 2, 2, batch); // this tile, lower subtile
                    SetupTerrainVertex(rightTile, 0, 2, batch); // right tile
                    SetupTerrainVertex(downRightTile, 0, 0, batch); // downRightTile, upper left subtile
                }
            }

            // connect down right - 2nd triangle:
            if (downRightTile.TerrainTile.Terrain != null)
            {

                if (downTile.TerrainTile.Terrain != null)
                {
                    SetupTerrainVertex(tile, 2, 2, batch); // this tile, lower subtile
                    SetupTerrainVertex(downRightTile, null, null, batch);
                    SetupTerrainVertex(downTile, null, null, batch);
                }
                else
                {
                    SetupTerrainVertex(tile, 2, 2, batch); // this tile, lower subtile
                    SetupTerrainVertex(downRightTile, null, null, batch);
                    SetupTerrainVertex(downTile, 2, 0, batch);
                }
            }
            else
            {
                if (downTile.TerrainTile.Terrain != null)
                {
                    SetupTerrainVertex(tile, 2, 2, batch); // this tile, lower subtile
                    SetupTerrainVertex(downRightTile, 0, 0, batch);
                    SetupTerrainVertex(downTile, null, null, batch);
                }
                else
                {
                    SetupTerrainVertex(tile, 2, 2, batch); // this tile, lower subtile
                    SetupTerrainVertex(downRightTile, 0, 0, batch);
                    SetupTerrainVertex(downTile, 2, 0, batch);
                }

            }

        }


        private static void CreateInnerSubtiles(TerrainBatch batch, TerrainTilePosition tile)
        {
            // create 8 triangles:
            SetupTerrainVertex(tile, 0, 0, batch);
            SetupTerrainVertex(tile, 1, 0, batch);
            SetupTerrainVertex(tile, 1, 1, batch);

            SetupTerrainVertex(tile, 0, 0, batch);
            SetupTerrainVertex(tile, 1, 1, batch);
            SetupTerrainVertex(tile, 0, 1, batch);

            SetupTerrainVertex(tile, 1, 0, batch);
            SetupTerrainVertex(tile, 2, 0, batch);
            SetupTerrainVertex(tile, 2, 1, batch);

            SetupTerrainVertex(tile, 1, 0, batch);
            SetupTerrainVertex(tile, 2, 1, batch);
            SetupTerrainVertex(tile, 1, 1, batch);

            SetupTerrainVertex(tile, 0, 1, batch);
            SetupTerrainVertex(tile, 1, 1, batch);
            SetupTerrainVertex(tile, 1, 2, batch);

            SetupTerrainVertex(tile, 0, 1, batch);
            SetupTerrainVertex(tile, 1, 2, batch);
            SetupTerrainVertex(tile, 0, 2, batch);

            SetupTerrainVertex(tile, 1, 1, batch);
            SetupTerrainVertex(tile, 2, 1, batch);
            SetupTerrainVertex(tile, 2, 2, batch);

            SetupTerrainVertex(tile, 1, 1, batch);
            SetupTerrainVertex(tile, 2, 2, batch);
            SetupTerrainVertex(tile, 1, 2, batch);

        }

        private static void ConnectSubtileTerrainDown(TerrainBatch batch, TerrainTilePosition tile, TerrainTilePosition downTile)
        {
            // connect down
            if (downTile.TerrainTile.Terrain != null)
            {
                // single terrain
                // draw 2 triangles

                SetupTerrainVertex(tile, 2, 2, batch); // this tile
                SetupTerrainVertex(downTile, null, null, batch); // down tile
                SetupTerrainVertex(tile, 1, 2, batch);

                SetupTerrainVertex(tile, 1, 2, batch); // this tile
                SetupTerrainVertex(downTile, null, null, batch); // down tile
                SetupTerrainVertex(tile, 0, 2, batch);

            }
            else
            {
                // draw 4 triangles down
                SetupTerrainVertex(tile, 2, 2, batch); // this tile
                SetupTerrainVertex(downTile, 2, 0, batch); // down tile
                SetupTerrainVertex(tile, 1, 2, batch);

                SetupTerrainVertex(tile, 1, 2, batch); // this tile
                SetupTerrainVertex(downTile, 2, 0, batch); // down tile
                SetupTerrainVertex(downTile, 1, 0, batch);

                SetupTerrainVertex(tile, 1, 2, batch); // this tile
                SetupTerrainVertex(downTile, 1, 0, batch); // down tile
                SetupTerrainVertex(tile, 0, 2, batch);

                SetupTerrainVertex(tile, 0, 2, batch); // this tile
                SetupTerrainVertex(downTile, 1, 0, batch); // down tile
                SetupTerrainVertex(downTile, 0, 0, batch);

            }

        }


        private static void ConnectSingleTerrainDown(TerrainBatch batch, TerrainTilePosition tile, TerrainTilePosition downTile, TerrainTilePosition downRightTile)
        {
            // connect down
            if (downTile.TerrainTile.Terrain != null)
            {
                // single terrain
                if (downRightTile == null)
                {
                    return; // nothing to draw.
                }
                //1 => 1
                /* x
                 * | \    
                 * |   \
                 * |     \
                 * |       \
                 * o---------o
                 */

                SetupTerrainVertex(tile, null, null, batch); // this tile

                if (downRightTile.TerrainTile.Terrain != null)
                {
                    SetupTerrainVertex(downRightTile, null, null, batch); // down right tile
                }
                else
                {
                    SetupTerrainVertex(downRightTile, 0, 0, batch); // down right tile
                }

                SetupTerrainVertex(downTile, null, null, batch); // down tile

            }
            else
            {
                // 1 => 3
                // draw 3 triangles all in all.
                // first the 2 straight down:
                SetupTerrainVertex(tile, null, null, batch); // this tile
                SetupTerrainVertex(downTile, 1, 0, batch); // down tile, middle subtile
                SetupTerrainVertex(downTile, 0, 0, batch); // down tile, left subtile

                SetupTerrainVertex(tile, null, null, batch); // this tile
                SetupTerrainVertex(downTile, 2, 0, batch); // down tile, right subtile
                SetupTerrainVertex(downTile, 1, 0, batch); // down tile, middle subtile

                if (downRightTile != null)
                {
                    // now the triangle to the down right tile:
                    if (downRightTile.TerrainTile.Terrain != null)
                    {
                        SetupTerrainVertex(tile, null, null, batch);        // this tile
                        SetupTerrainVertex(downRightTile, null, null, batch); // down right tile
                        SetupTerrainVertex(downTile, 2, 0, batch); // down tile, right subtile

                    }
                    else
                    {
                        SetupTerrainVertex(tile, null, null, batch);        // this tile
                        SetupTerrainVertex(downRightTile, 0, 0, batch);         // down right tile, left subtile
                        SetupTerrainVertex(downTile, 2, 0, batch); // down tile, right subtile
                    }
                }
            }

        }

        private static void ConnectSingleTerrainToTheRight(TerrainBatch batch, TerrainTilePosition tile, TerrainTilePosition rightTile, TerrainTilePosition downRightTile)
        {
            // single terrain
            if (downRightTile == null)
            {
                return; // nothing to draw.
            }

            // connect to the right
            if (rightTile.TerrainTile.Terrain != null)
            {

                //1 => 1
                /* x---------o
                 *  \        |
                 *    \      |
                 *      \    |
                 *        \  |
                 *           o
                 * 
                 */

                SetupTerrainVertex(tile, null, null, batch); // this tile
                SetupTerrainVertex(rightTile, null, null, batch); // right tile

                if (downRightTile.TerrainTile.Terrain != null)
                {
                    SetupTerrainVertex(downRightTile, null, null, batch); // down right tile
                }
                else
                {
                    SetupTerrainVertex(downRightTile, 0, 0, batch); // down right tile
                }

            }
            else
            {
                // 1 => 3
                // draw 3 triangles all in all.
                SetupTerrainVertex(tile, null, null, batch); // this tile
                SetupTerrainVertex(rightTile, 0, 0, batch); // right tile, upper subtile
                SetupTerrainVertex(rightTile, 0, 1, batch); // right tile, middle subtile

                SetupTerrainVertex(tile, null, null, batch); // this tile
                SetupTerrainVertex(rightTile, 0, 1, batch); // right tile, middle subtile
                SetupTerrainVertex(rightTile, 0, 2, batch); // right tile, lower subtile

                // connect to the down right tile:
                if (downRightTile.TerrainTile.Terrain != null) // Lars: got a crash here... downRightTile was null
                {
                    SetupTerrainVertex(tile, null, null, batch);        // this tile
                    SetupTerrainVertex(rightTile, 0, 2, batch);         // right tile, lower subtile
                    SetupTerrainVertex(downRightTile, null, null, batch); // down right tile

                }
                else
                {
                    SetupTerrainVertex(tile, null, null, batch);        // this tile
                    SetupTerrainVertex(rightTile, 0, 2, batch);         // right tile, lower subtile
                    SetupTerrainVertex(downRightTile, 0, 0, batch); // down right tile, upper subtile
                }
            }

        }

        private void SetUpTerrainIndices(short[] indices)
        {
            int counter = 0;
            for (int y = 0; y < noOfVerticesVertical - 1; y++)
            {
                for (int x = 0; x < noOfVerticesHorizontal - 1; x++)
                {
                    short topLeft = (short)(x + y * noOfVerticesHorizontal);
                    short topRight = (short)((x + 1) + y * noOfVerticesHorizontal);
                    short lowerRight = (short)((x + 1) + (y + 1) * noOfVerticesHorizontal);
                    short lowerLeft = (short)(x + (y + 1) * noOfVerticesHorizontal);

                    indices[counter++] = topLeft;
                    indices[counter++] = lowerRight;
                    indices[counter++] = lowerLeft;

                    indices[counter++] = topLeft;
                    indices[counter++] = topRight;
                    indices[counter++] = lowerRight;
                }
            }
        }

        public static void SetUpIndices(int noOfQuads, short[] indices)
        {

            int counter = 0;
            int quadOffset = 0;

            for (int i = 0; i < noOfQuads - 1; i++)
            {
                quadOffset = i * 4;

                short topLeft = (short)(quadOffset);
                short topRight = (short)(quadOffset + 1);
                short lowerRight = (short)(quadOffset + 2);
                short lowerLeft = (short)(quadOffset + 3);

                indices[counter++] = topLeft;
                indices[counter++] = lowerRight;
                indices[counter++] = lowerLeft;

                indices[counter++] = topLeft;
                indices[counter++] = topRight;
                indices[counter++] = lowerRight;

            }
        }

       

        private void SortObjectsForDrawingAndComputeMatrices(bool drawModels)
        {
            foreach (var sortedList in sortedObjectsToDraw)
            {
                sortedList.Clear();
            }



            shadowsToDraw.Clear();

            TerrainTile tileToDraw;

            ComputeDrawingArea();


            bool drawLightSources = true;
#if DEBUG || PROFILE
            drawLightSources = Kensei.Dev.Options.GetOption("Rendering.Render light sources");
#endif


            bool drawBillboards = true;
#if DEBUG || PROFILE
            drawBillboards = Kensei.Dev.Options.GetOption("Rendering.Render billboards");
#endif

            /**
            bool drawItems = true;
#if DEBUG || PROFILE
            drawItems = Kensei.Dev.Options.GetOption("Rendering.Render items");
#endif
*/
            bool isInGodMode = GetIsInGodMode();

            SharedKnowledge knowledgeToShow = The.InGameUI.UIAllegiance.SharedKnowledge;

            bool tileIsInFogOfWar;

            // Draw quads:
            int currentRow;

            for (int y = TileStartY; y <= TileEndY; y++)
            {
                //tileRow = 
                currentRow = y - TileStartY;
                for (int x = TileStartX; x <= TileEndX; x++)
                {
                    tileToDraw = The.Map.TileMap[x][y];



                    tileIsInFogOfWar = !tileToDraw.AllegiancesThatSeeThisTile.Contains(The.InGameUI.UIAllegiance); // Intelligence.AllegianceType.Player);

                    //draw buildings:
                    if (tileToDraw.BaseCenterForMultiTileEntities != null)
                    {

                        //add light sources on the building:
                        if (drawLightSources)
                        {
                            foreach (Entity entity in tileToDraw.BaseCenterForMultiTileEntities)
                            {
                                // Lighting lighting;
                                if (entity.Renderable != null
                                   && entity.Renderable.LightSources != null)
                                {
                                    lightSourcesToDraw.AddRange(entity.Renderable.LightSources);
                                }
                                /*
                                if (entity.Renderable != null
                                    && entity.Renderable.Lighting != null)
                                {
                                    entity.Renderable.Lighting.GetLightSourcesForDrawing(lightSourcesToDraw);
                                }*/

                            }
                        }

                    }

                    // draw the items and entities that were last seen by the player.
                    if (tileToDraw.RememberedRootEntitiesOnTile != null)
                    {

                        List<MemoryFact> memoryFacts;
                        if (tileToDraw.RememberedRootEntitiesOnTile.TryGetValue(knowledgeToShow, out memoryFacts))
                        {
                            foreach (MemoryFact memoryFact in memoryFacts)
                            {
                                if (!memoryFact.IsAlwaysShown() // trees were drawn already.
                                    && memoryFact.PartOfID == null && memoryFact.ContainedBy == null) // new...
                                {
                                    AddRenderableToRender(drawModels, drawBillboards, currentRow, memoryFact.Renderable, x, y);
                                }
                            }
                        }

                    }

                    if (!isInGodMode
                        && The.InGameUI.UIAllegiance.AllegianceType == SimSide.Allegiances.AllegianceType.Player
                        && !tileToDraw.HasEverBeenSeenByPlayer)
                    {
                        // cull entities in the shroud when in player mode
                        continue;
                    }

                    // NEW: handle multi-billboard structures:
                    // it would be better if the entity were added on the first tile we see it, and disregarded on the others...
                    // PROBLEM: we have to search previous row collections.
                    if (tileToDraw.GeoLayoutEntitiesOnTile != null)
                    {
                        /* if (x == 217)
                         {

                         }*/
                        for (int i = tileToDraw.GeoLayoutEntitiesOnTile.Count - 1; i >= 0; i--)
                        {
                            EntityID entityIDOnTile = tileToDraw.GeoLayoutEntitiesOnTile[i];
                            Entity entityOnTile = Entity.FindByID(entityIDOnTile);
                            if (entityOnTile != null)
                            {
                                AddRenderableToRender(drawModels, drawBillboards, currentRow, entityOnTile.Renderable, x, y);
                            }
                            else
                            {
                                tileToDraw.GeoLayoutEntitiesOnTile.RemoveAt(i);
                            }
                        }
                        /*
                        foreach (Entity entity in tileToDraw.TiledEntityOnTile)
                        {
                            AddRenderableToRender(drawModels, drawBillboards, currentRow, entity.Renderable, x, y);
                        }*/
                    }


                    // draw structure being placed
                    if ((The.InGameUI.InterfaceMode == InGameInterface.InterfaceState.Build || The.InGameUI.InterfaceMode == InGameInterface.InterfaceState.EditorPlaceEntity))
                    {

                        // test gets called for every tile, hmmm...
                        foreach (InGameInterface.EntityPosition ep in The.InGameUI.EntitiesBeingPlaced)
                        {
                            AddRenderableToRender(drawModels, drawBillboards, currentRow, ep.Entity.Renderable, x, y);
                        }
                    }



                    if (tileToDraw.EntitiesOnTile != null)
                    {
                        foreach (Entity entity in tileToDraw.EntitiesOnTile)
                        {
                          
                            if (isInGodMode
                                || (!tileIsInFogOfWar && !entity.RequiresRollToDetect()) // items are always 'detected'
                                || entity.EntityType.GetIsNeverInFogOfWar() // always render 'rocks' and trees                             
                                || (entity.EntityType.IntelligenceType != null && entity.Intelligence.Allegiance == The.InGameUI.UIAllegiance) // always render our agents
                                || knowledgeToShow.AllDetectedEntities.Contains(entity.DetectableID)) // don't render non-detected entities!
                            {
                                AddRenderableToRender(drawModels, drawBillboards, currentRow, entity.Renderable, x, y);
                            }
                        }
                    }



                    if (tileToDraw.TileResources != null)
                    {
                        foreach (var resource in tileToDraw.TileResources)
                        {
                            if (tileToDraw.X == 18 && tileToDraw.Y == 10)
                            {

                            }

                            if (isInGodMode || knowledgeToShow.AllDetectedEntities.Contains(resource.Value.DetectableID)) // don't render non-detected resources!
                            {
                                AddRenderableToRender(drawModels, drawBillboards, currentRow, resource.Value.Renderable, x, y);

                                //resource.Value.IsSeenByUIAllegiance = true; // is used in outline rendering
                            }
                          /*  else
                            {
                                resource.Value.IsSeenByUIAllegiance = false;
                            }*/

                        }
                    }

                    if (!tileIsInFogOfWar || isInGodMode)
                    {
                        // add free renderables such as particle emitters here:
                        if (tileToDraw.RenderablesOnTile != null)
                        {
                            foreach (var renderable in tileToDraw.RenderablesOnTile)
                            {
                                AddRenderableToRender(drawModels, drawBillboards, currentRow, renderable, x, y);
                            }
                        }
                    }

                }
            }

            // compute matrices once...
            foreach (var sortedList in sortedObjectsToDraw)
            {
                foreach (var gameObject in sortedList)
                {
                    Renderable renderable = gameObject.AsRenderable; 
                    if (renderable != null && renderable.RenderAsModel != null)
                    {
                        renderable.RenderAsModel.ComputeMatricesForDrawing(AnimatedModel.Transformations.All, renderable.RenderAsModel.FinalModelScale);

                    }
                }
            }

            // update fading based on what was rendered the last frame:
            if (previouslyDrawnRenderablesThatCanFade != null)
            {
                foreach (var item in previouslyDrawnRenderablesThatCanFade)
                {
                    // if (!item.IsRenderedThisFrame)
                    if (currentlyDrawnRenderablesThatCanFade == null || !currentlyDrawnRenderablesThatCanFade.Contains(item))
                    {
                        // start fading out:
                        renderablesFadingOut.Add(item);
                        item.FadeOut();
                    }
                }
            }

            if (previouslyDrawnRenderablesThatCanLerp != null)
            {
                foreach (var item in previouslyDrawnRenderablesThatCanLerp)
                {
                    // if (!item.IsRenderedThisFrame)
                    if (currentlyDrawnRenderablesThatCanLerp == null || !currentlyDrawnRenderablesThatCanLerp.Contains(item))
                    {
                        item.LerpableWasRenderedLastFrame = false;
                        
                      //  renderablesFadingOut.Add(item);                     
                    }
                }
            }

            previouslyDrawnRenderablesThatCanLerp = currentlyDrawnRenderablesThatCanLerp;
            currentlyDrawnRenderablesThatCanLerp = null;

            previouslyDrawnRenderablesThatCanFade = currentlyDrawnRenderablesThatCanFade;
            currentlyDrawnRenderablesThatCanFade = null;
            //previouslyDrawnRenderablesThatCanFade.Clear();
        }


        private void DrawInvisibleEntitiesForDebugOrEditor()
        {
#if DEBUG || PROFILE
            // draw invisible entities (ambients, fog emitters etc.)
           
            bool drawMarkers = Kensei.Dev.Options.GetOption("Overlays.Markers");

            if (drawMarkers)
            {

                int currentRow;
                TerrainTile tileToDraw;

                for (int y = TileStartY; y <= TileEndY; y++)
                {
                    //tileRow = 
                    currentRow = y - TileStartY;
                    for (int x = TileStartX; x <= TileEndX; x++)
                    {
                        tileToDraw = The.Map.TileMap[x][y];

                        if (tileToDraw.EntitiesOnTile != null)
                        {
                            Renderable renderable;
                            RenderableType renderableType;
                            foreach (Entity entity in tileToDraw.EntitiesOnTile)
                            {
                                renderable = entity.Renderable;
                                renderableType = renderable.RenderableType;

                                if (renderable.RenderAsBillboard == null
                                    && renderable.RenderAsGroundSprite == null
                                    && renderable.RenderAsModel == null
                                    && renderable.RenderAsIcon == null
                                    && renderable.RenderAsConnectedGroundSprite == null)
                                {
                                    if (renderableType.DefaultClientState != null && renderableType.DefaultClientState.Sounds != null)
                                    {
                                        The.MapUI.AddDebugMarker(entity.Location.Value, Color.DeepPink, entity, 6);

                                    }
                                    else if (renderable.ParticleEmitters != null)
                                    {
                                        The.MapUI.AddDebugMarker(entity.Location.Value, Color.Khaki, entity, 6);
                                    }

                                    /*RenderableType = new RenderableType()
                   {
                       Default = new SpriteConditionInfo()
                       {
                           Sounds*/
                                    // renderable.RenderableType.soun
                                }
                            }
                        }
                    }
                }
            }
#endif
        }

        public static bool GetIsInGodMode()
        {

            bool isInGodMode = false;

#if DEBUG || PROFILE
            isInGodMode = Kensei.Dev.Options.GetOption("Dev.God mode");
#endif
            if (The.Sim.Mode == SimSide.Sim.EngineMode.Edit) //The.InGameUI.IsShowingMapEditor())
            {
                isInGodMode = true;
            }
            return isInGodMode;
        }

        /*
        private Renderable GetRenderable(ILocatable iLocatable)
        {
           // Entity entity = iLocatable as Entity;
            if (iLocatable.AsEntity != null) // entity != null)
            {
                return iLocatable.AsEntity.Renderable;
            }

            //MemoryFact memoryFact = iLocatable as MemoryFact;
            if (iLocatable.AsMemoryFact != null) //memoryFact != null)
            {
                return iLocatable.AsMemoryFact.Renderable; // memoryFact.Renderable;
            }

            Renderable renderable = iLocatable as Renderable;
            return renderable;

        }
        */

        private void ComputeDrawingArea()
        {
            // add an n tile wide border for drawing large stuff:
            TileStartX = The.MapUI.mapWindowTileX - xTilesToIncludeInDraw;
            TileEndX = TileStartX + The.MapUI.noOfTilesToDisplayHorizontally + 2 * xTilesToIncludeInDraw;
            TileStartY = The.MapUI.mapWindowTileY - yTopTilesToIncludeInDraw;
            TileEndY = TileStartY + The.MapUI.noOfTilesToDisplayVertically + yTopTilesToIncludeInDraw + yBottomTilesToIncludeInDraw;

            TileStartX = The.Map.ClampTileMapXPosition(TileStartX);
            TileEndX = The.Map.ClampTileMapXPosition(TileEndX);
            TileStartY = The.Map.ClampTileMapYPosition(TileStartY);
            TileEndY = The.Map.ClampTileMapYPosition(TileEndY);

            tileStartShadowsX = The.MapUI.mapWindowTileX - 1;
            tileEndShadowsX = tileStartShadowsX + The.MapUI.noOfTilesToDisplayHorizontally + 2;
            tileStartShadowsY = The.MapUI.mapWindowTileY - 1;
            tileEndShadowsY = tileStartShadowsY + The.MapUI.noOfTilesToDisplayVertically + 2;

            // NEW: start drawing 'off-map' to get all shadows
            Vector3? shadowVector3D = DayAndNightEffects.GetDropShadowEstimate();
            int shadowTilesInX = 0;
            int shadowTilesInY = 0;
            if (shadowVector3D.HasValue)
            {
                shadowTilesInX = (int)((shadowVector3D.Value.X * 50f) / MapManager.tileSize);
                shadowTilesInY = (int)((shadowVector3D.Value.Y * 50f) / MapManager.tileSize);
            }

            if (shadowTilesInX > 0)
            {
                tileStartShadowsX = tileStartShadowsX - shadowTilesInX;
            }
            else if (shadowTilesInX < 0)
            {
                tileEndShadowsX = tileEndShadowsX + Math.Abs(shadowTilesInX);
            }
            if (shadowTilesInY > 0)
            {
                tileStartShadowsY = tileStartShadowsY - shadowTilesInY;
            }
            else if (shadowTilesInY < 0)
            {
                tileEndShadowsY = tileEndShadowsY + Math.Abs(shadowTilesInY);
            }

            tileStartShadowsX = The.Map.ClampTileMapXPosition(tileStartShadowsX);
            tileEndShadowsX = The.Map.ClampTileMapXPosition(tileEndShadowsX);
            tileStartShadowsY = The.Map.ClampTileMapYPosition(tileStartShadowsY);
            tileEndShadowsY = The.Map.ClampTileMapYPosition(tileEndShadowsY);
        }

        /// <summary>
        /// adds entity to be drawn. For multi-billboard entities, only adds the ones within the specified tile.
        /// </summary>
        /// <param name="drawModels"></param>
        /// <param name="drawBillboards"></param>
        /// <param name="currentRow"></param>
        /// <param name="renderable"></param>
        /// <param name="tileX"></param>
        /// <param name="tileY"></param>     
        private void AddRenderableToRender(bool drawModels, bool drawBillboards, int currentRow, Renderable renderable, int tileX, int tileY)
        {
            if (renderable != null)
            {
                renderable.IsOnScreen = true;
            }

            if (renderable != null && renderable.IsDrawn)
            {

                if (renderable.SelectedSpriteInfo != null // calling this will adopt new sprites if needed

                    //&& renderable.SelectedSpriteInfo.RenderAsBillboardType != null) //  OLD.. why test for billboards?
                    && renderable.RenderAsGroundSprite != null)
                {
                    if (PositionIsOnTile(renderable.MapPosition, tileX, tileY))
                    {

                        if (renderable.Parent != null
                            && renderable.Parent.EntityType.StructureType != null // keep this???
                            && renderable.Parent.EntityType.StructureType.IsAddon)
                        {
                            // TODO: let RenderAsGroundSpriteType define a sorting order

                            middleSprites.Add(renderable.RenderAsGroundSprite);

                        }
                        else
                        {
                            // TODO: let RenderAsGroundSpriteType define a sorting order

                            bottomSprites.Add(renderable.RenderAsGroundSprite);

                        }
                    }

                }

                bool renderableWasAdded = false;

                if (renderable.RenderAsBillboard != null)
                {
                    if (drawBillboards)
                    {

                        foreach (RenderAsBillboard billboard in renderable.RenderAsBillboard)
                        {

                            if (PositionIsOnTile(billboard.MapPosition.Value, tileX, tileY))
                            {
                                if (renderable.Entity != null && renderable.Entity.ToString().Contains("Tipi"))
                                {

                                }

                                if (TileEntitiesAreInSight(tileX, tileY))
                                {

                                    sortedObjectsToDraw[currentRow].Add(billboard);
                                    //  renderable.IsRenderedThisFrame = true;
                                }

                                if (!renderable.DrawAsNonPhysical)
                                {
                                    if (TileShadowsAreInSight(tileX, tileY))
                                    {
                                        shadowsToDraw.Add(billboard);
                                    }
                                }
                            }
                        }
                    }
                }
                else if (renderable.RenderAsModel != null)
                {
                    if (drawModels)
                    {
                        if (PositionIsOnTile(renderable.MapPosition, tileX, tileY))
                        {
                            if (TileEntitiesAreInSight(tileX, tileY))
                            {
                                sortedObjectsToDraw[currentRow].Add(renderable);


                                renderableWasAdded = true;

                                if (!renderable.DrawAsNonPhysical && renderable.Parent.EntityType.BiologicalType != null) // no light emitted from memory facts and from dead things
                                {
                                    if (renderable.RenderAsModel.ModelData.HasEmittingParts)
                                    {
                                        lightEmittingModels.Add(renderable);
                                    }
                                }
                            }

                            if (!renderable.DrawAsNonPhysical && renderable.MemoryFact == null) // no shadows on memory facts
                            {
                                if (TileShadowsAreInSight(tileX, tileY))
                                {
                                    shadowsToDraw.Add(renderable);
                                }
                            }
                        }
                    }
                }

                // for entity renderables that only have particle emitters:
                if (!renderableWasAdded && renderable.ParticleEmitters != null && renderable.MemoryFact == null) // memory facts don't draw particles...
                {
                    if (PositionIsOnTile(renderable.MapPosition, tileX, tileY))
                    {
                        if (TileEntitiesAreInSight(tileX, tileY))
                        {
                            sortedObjectsToDraw[currentRow].Add(renderable);
                            renderableWasAdded = true;
                        }
                    }
                }

              
                // for entity renderables that are only sound emitters
                if (!renderableWasAdded && renderable.StateSoundPlaying != null && renderable.MemoryFact == null) // memory facts don't emit sound...
                {
                    if (PositionIsOnTile(renderable.MapPosition, tileX, tileY))
                    {
                        if (TileEntitiesAreInSight(tileX, tileY))
                        {
                            sortedObjectsToDraw[currentRow].Add(renderable);
                            renderableWasAdded = true;

                            //  renderable.IsRenderedThisFrame = true;
                        }
                    }
                }

                // update collections to keep track of fading in/out
                if (renderableWasAdded)
                {
                    if (renderable.CanLerpLocation())
                    {
                        Common.AddToList(ref currentlyDrawnRenderablesThatCanLerp, renderable);                       
                    }

                    if (renderable.CanFade())
                    {                       
                        Common.AddToList(ref currentlyDrawnRenderablesThatCanFade, renderable);
                       
                        renderable.FadeIn();
                    }
                }
            }
        }

        public bool PositionIsOnTile(Point positionOfEntity, int tileX, int tileY)
        {
            return positionOfEntity.X == tileX && positionOfEntity.Y == tileY;
        }

        private bool TileShadowsAreInSight(int tileX, int tileY)
        {
            return tileX >= tileStartShadowsX && tileX <= tileEndShadowsX && tileY >= tileStartShadowsY && tileY <= tileEndShadowsY;
        }

        public bool TileEntitiesAreInSight(int tileX, int tileY)
        {
            return tileX >= TileStartX && tileX <= TileEndX && tileY >= TileStartY && tileY <= TileEndY;
        }

        /*  private void AddEntityToRender(bool drawModels, bool drawBillboards, int currentRow, MemoryFact memoryFact, int tileX, int tileY)
          {
              Entity entity = (Entity)memoryFact.PointsTo;

              // if ((entity.Renderable.RenderAsModel != null)) // && entity.DrawThis()) // always drawn if in this list.
              //{ 
                  if (drawModels 
                      && memoryFact.ContainedBy == null 
                      && memoryFact.MapPosition.X == tileX && memoryFact.MapPosition.Y == tileY)
                  {
                      sortedObjectsToDraw[currentRow].Add(memoryFact);
                  }
              // } 
          }*/


        //   Renderable renderable = RenderableFactory.Produce(null, emitterType);

        public Renderable GetFreeAttachableRenderable(string renderableTypeKey)
        {
            Renderable renderable;

            Queue<Renderable> renderables;
            if (!attachableRenderables.TryGetValue(renderableTypeKey, out renderables))
            {
                renderables = new Queue<Renderable>();
                attachableRenderables.Add(renderableTypeKey, renderables);
            }

            if (renderables.Count == 0)
            {
                // add some fresh boxes, we've run out:
                RenderableType renderableType = GameData.Instance.AttachableRenderableTypes[renderableTypeKey]; 
                for (int i = 0; i < 1 /* 10*/; i++)
                {
                    renderable = RenderableFactory.Produce(null, renderableType);
                  
                    renderable.Initialize(true);

                    renderables.Enqueue(renderable);
                }
            }

            renderable = renderables.Dequeue();


            return renderable;

        }

        public void RetireAttachableRenderable(Renderable renderable)
        {
            attachableRenderables[renderable.RenderableType.KeyName].Enqueue(renderable);
        }

        /*
        public Entity GetFreeAttachableEntity(string key)
        {
            Entity entity;

            Queue<Entity> entities;
            if (!attachableEntities.TryGetValue(key, out entities))
            {
                entities = new Queue<Entity>();
                attachableEntities.Add(key, entities);
            }

            if (entities.Count == 0)
            {
                // add some fresh boxes, we've run out:
                EntityType entityType = GameData.Instance.AllEntityTypes[key]; //"box"];
                for (int i = 0; i < 10; i++)
                {
                    entity = Entity.Produce(entityType, true);
                    entity.Initialize(null);
                    entity.InitializeModelAndOnScreenFunctionality(The.Sim.ScreenManager.Game);

                    entities.Enqueue(entity);
                }
            }

            entity = entities.Dequeue();


            return entity;

        }
       
        public void RetireAttachableEntity(Entity box)
        {
            attachableEntities[box.EntityType.KeyName].Enqueue(box);
        } */

        private void DrawBloomEffect(Texture2D sceneTexture)
        {

            if (The.Client.spriteBatch == null)
                return;

            if (The.Client.BloomEnabled)
            {
                // "Subtle",      0.92f,  3,   1.5f,     1,    1,       1),
                //               Thresh  Blur Bloom  Base  BloomSat BaseSat
                if (DayAndNightEffects.SunAnimation == DayAndNightEffects.SunAnimations.MorningAfterSunrise)
                {
                    float progress = The.Sim.DateAndTime.SunElevation / DateAndTime.dawnSunElevationEnd;

                    float newBloomThreshold;
                    float newBloomIntensity;
                    float targetBloomThreshold = bloom.BaseSettings.BloomThreshold - amountToLowerBloomThresholdAtDawn;
                    float targetBloomIntensity = bloom.BaseSettings.BloomIntensity + amountToRaiseBloomIntensityAtDawn;

                    if (progress < 0.1f)
                    {   // 'ramping up' quickly:
                        progress *= 10f; // normalize
                        newBloomThreshold = MathHelper.SmoothStep(bloom.BaseSettings.BloomThreshold, targetBloomThreshold, progress);
                        newBloomIntensity = MathHelper.SmoothStep(bloom.BaseSettings.BaseIntensity, targetBloomIntensity, progress);
                    }
                    else if (progress < 0.4f)
                    { // plateau:
                        newBloomThreshold = targetBloomThreshold;
                        newBloomIntensity = targetBloomIntensity;
                    }
                    else
                    {   // fade slowly to normal levels:
                        progress = (progress - 0.4f) / 0.6f; // normalize
                        newBloomThreshold = MathHelper.SmoothStep(targetBloomThreshold, bloom.BaseSettings.BloomThreshold, progress);
                        newBloomIntensity = MathHelper.SmoothStep(targetBloomIntensity, bloom.BaseSettings.BloomIntensity, progress);
                    }

                    bloom.Settings.BloomThreshold = newBloomThreshold;
                    bloom.Settings.BloomIntensity = newBloomIntensity;
                }
                else if (DayAndNightEffects.SunAnimation == DayAndNightEffects.SunAnimations.EveningBeforeSunset)
                {
                    float progress = 1f - The.Sim.DateAndTime.SunElevation / DateAndTime.sunsetElevationStart;

                    float newBloomThreshold;
                    float newBloomIntensity;
                    float targetBloomThreshold = bloom.BaseSettings.BloomThreshold - amountToLowerBloomThresholdAtSunset;
                    float targetBloomIntensity = bloom.BaseSettings.BloomIntensity + amountToRaiseBloomIntensityAtSunset;

                    if (progress < 0.5f)
                    {   // 'ramping up' slowly:
                        progress *= 10f; // normalize
                        newBloomThreshold = MathHelper.SmoothStep(bloom.BaseSettings.BloomThreshold, targetBloomThreshold, progress);
                        newBloomIntensity = MathHelper.SmoothStep(bloom.BaseSettings.BaseIntensity, targetBloomIntensity, progress);
                    }
                    else if (progress < 0.7f)
                    { // plateau:
                        newBloomThreshold = targetBloomThreshold;
                        newBloomIntensity = targetBloomIntensity;
                    }
                    else
                    {   // fade slowly to normal levels:
                        progress = (progress - 0.7f) / 0.3f; // normalize
                        newBloomThreshold = MathHelper.SmoothStep(targetBloomThreshold, bloom.BaseSettings.BloomThreshold, progress);
                        newBloomIntensity = MathHelper.SmoothStep(targetBloomIntensity, bloom.BaseSettings.BloomIntensity, progress);
                        // newBloomIntensity = targetBloomIntensity;
                    }

                    bloom.Settings.BloomThreshold = newBloomThreshold;
                    bloom.Settings.BloomIntensity = newBloomIntensity;

                }

                bloom.Draw(sceneTexture); //null, );
            }
            else
            {
                // no bloom. draw to back buffer:
                The.Client.Controller.SetZoomRenderTaget(); // The.Client.GraphicsDevice.SetRenderTarget(null);

                The.Client.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
                // Draw the quad.
                The.Client.spriteBatch.Draw(sceneTexture, new Rectangle(0, 0, sceneTexture.Width, sceneTexture.Height), Color.White);
                The.Client.spriteBatch.End();

            }
        }

        bool isInGodMode = false;

        /// <summary>
        /// must be done after all entities have been updated, but before drawing starts.
        /// </summary>
        public void UpdateModelMatricesWithNewPositions()
        {
            bool drawModels = true;
#if DEBUG
            drawModels = Kensei.Dev.Options.GetOption("Rendering.Render models");
#endif



#if DEBUG || PROFILE
            isInGodMode = Kensei.Dev.Options.GetOption("Dev.God mode");
#endif
            if (The.Sim.Mode == SimSide.Sim.EngineMode.Edit) //The.InGameUI.IsShowingMapEditor())
            {
                isInGodMode = true;
            }


            // recompute the camera and view matrix
            float cameraX = The.MapUI.MapWindowWorldPosition.X + The.MapUI.mapWindowWidth / 2f;
            float cameraY = The.MapUI.MapWindowWorldPosition.Y + The.MapUI.mapWindowHeight / 2f;

            CameraTarget = new Vector3(cameraX, cameraY, 0f);
            CameraPosition = CameraTarget - CameraDirection;

            Vector3 up = -Vector3.UnitZ;
            //up.Normalize();

            View = Matrix.CreateLookAt(CameraPosition, CameraTarget, up);


            SortObjectsForDrawingAndComputeMatrices(drawModels);

            //  ComputeModelMatricesForDrawing();

        }

        public void UpdateMousePicking()
        {


        }

        private bool IsThereWaterInCurrentView()
        {
            int lastXToDraw, lastYToDraw, firstXToDraw, firstYToDraw;
            GetEdgesOfTerrainToDraw(out lastXToDraw, out lastYToDraw, out firstXToDraw, out firstYToDraw);


            TerrainTile tileToDraw;
            for (int y = firstYToDraw; y < lastYToDraw; y++)
            {
                for (int x = firstXToDraw; x < lastXToDraw; x++)
                {

                    int closestXOnMap = Common.Clamp(x, 0, The.Map.mapTileWidth - 1);
                    int closestYOnMap = Common.Clamp(y, 0, The.Map.mapTileHeight - 1);

                    tileToDraw = The.Map.TileMap[closestXOnMap][closestYOnMap];
                    if (tileToDraw.IsPartlyUnderWater())
                    {
                        return true;
                    }
                }
            }

            return false;
        }

       
        
        /// <summary>
        /// Draws map and people
        /// </summary>       
        public void Draw()
        {           
            // used to cull water rendering:           
            bool isThereWaterInView = IsThereWaterInCurrentView();

            //   SaveTextureToFile("spritesheet", UWGame.SimSide.Instance.FlatSpriteSheet.Texture);
            //   SaveTextureToFile("billboards", GameData.Instance.BillboardSpriteSheet.Texture);

            // time is scaled down to make things wave in the wind more slowly.
            if (!sim.IsPaused)
            {
                windTime += (float)sim.GameTime.ElapsedGameTime.TotalSeconds * 0.333f;
            }
            // OLD:
            //windTime = (float)UWGame.SimSide.Instance.GameTime.TotalGameTime.TotalSeconds * 0.333f;

            DayAndNightEffects.Recompute();

            Color? tint = DayAndNightEffects.GetTimeOfDayColor();
            if (tint.HasValue)
            {
                TimeOfDayLightingFactor = ComputeTimeOfDayLightMultiplier(tint.Value);
            }
            else
            {
                TimeOfDayLightingFactor = Vector4.One; // *0.5f;
                TimeOfDayLightingFactor.W = 1f;
            }

            UpdateTerrainViewMatrix();

            // these vertices are needed in 2 places: Draw water refraction map and draw terrain.
            //if (firstFrame)
            //{
            //    SetUpTerrainVerticesAndIndicesInCurrentView();
            //    firstFrame = false;
            //}

            // The.Client.GraphicsDevice.DepthStencilBuffer = game.ScreenManager.NoMultiSamplingStencilBuffer; // XNA 3
            
            if (isThereWaterInView)
            {
                Water.UpdateReflectedViewMatrix(); // do this first!

                if (The.MapUI.IsScrolling == true)
                {
                    Water.DrawRefractionMap(terrainBatches);
                }

                Water.DrawReflectionMap();
            }

            // this is the main render target for all drawing, it is used as input to the edge detect effect along with a depth map:
            /*  if (game.EdgeDetectEnabled)
              {
                  game.graphics.GraphicsDevice.SetRenderTarget(0, EdgeDetectSceneRenderTarget);
              }
              else
              {
                  game.graphics.GraphicsDevice.SetRenderTarget(0, null);
              }*/




           The.Client.GraphicsDevice.SetRenderTarget(DiffuseMSRenderTarget);   //draw terrain into MS RT
          //  The.Client.GraphicsDevice.SetRenderTarget(null);


            The.Client.GraphicsDevice.Clear(Color.White);

         

#if DEBUG || PROFILE
            if (Kensei.Dev.Options.GetOption("Rendering.Render terrain"))
            {
               // terrainSlicedMap.Draw(null);
                terrainSlicedMap.Draw(DiffuseMSRenderTarget);
            }

            /*
            DiffuseMSRenderTarget.ResolveSubresource(diffuseRenderTarget);
            SaveRenderTargetToFile("diffuseRenderTarget", diffuseRenderTarget);
            */

#else
           
                terrainSlicedMap.Draw(DiffuseMSRenderTarget);

#endif
          /*  DrawMapEdges();

            return;*/
          //  goto skip;
          

            if (isThereWaterInView)
            {
#if DEBUG || PROFILE
                if (Kensei.Dev.Options.GetOption("Rendering.Render water"))
                {

                    Water.DrawWater(sim.GameTime);
                }
#else
                Water.DrawWater(sim.GameTime);
#endif
            }


            DrawGroundFeatureSprites();

            DrawMapEdges();

            //  DrawGroundSprites();

      

            bool drawModels = true;
#if DEBUG || PROFILE
            drawModels = Kensei.Dev.Options.GetOption("Rendering.Render models");
#endif
            
            bool drawShadows = true;
#if DEBUG || PROFILE
            drawShadows = Kensei.Dev.Options.GetOption("Rendering.Render shadows");
#endif

            if (drawShadows && The.Sim.DateAndTime.SunIsUp)
            {
                DrawShadows(drawModels);
            }

          

            // draw billboards, models, edges etc.           
            DrawSortedObjectsAndParticles();

           // goto skip;

            if (drawShadows && The.Sim.DateAndTime.SunIsUp)
            {               
                
               // The.Client.GraphicsDevice.SetRenderTarget(diffuseRenderTarget); // WHERE IS THIS USED??? the ms RT was cleared to black before this

                DrawCloudAndDropShadows();
            }

          //  skip:

            // draws the bloomed result of diffuseRenderTarget (EdgeDetectSceneRenderTarget) into the back buffer (=screen)
            DrawBloomEffect(diffuseFinalRenderTarget);  //DiffuseMSRenderTarget); // diffuseRenderTarget); 

            //********************
            // from this point on, we are drawing directly to back buffer, as all processing has finished.
            //*********************

            #region Draw debug markers

#if DEBUG || PROFILE
            The.MapUI.DrawDebugInfo(); 
#endif

            #endregion

            The.MapUI.DrawBullets();

            // we use the debugging system to draw fog of war in release mode too... perhaps this should be changed...
            Dimension dim = The.Client.Controller.DrawArea;
            Kensei.Dev.Manager.Draw(The.Client.GraphicsDevice, Matrix.Identity, dim.Width, dim.Height);
                //The.Client.GraphicsDevice.Viewport.Width, The.Client.GraphicsDevice.Viewport.Height);

            The.Client.GraphicsDevice.BlendState = BlendState.AlphaBlend;

            DrawOverlayGroundSprites();

            DrawOverlayBillboards();

            DrawOverlayModels();


            DrawInfluenceMapSprites();


            // print editor data for tiles here...
            if (sim.Mode == SimSide.Sim.EngineMode.Edit)
            {
                bool printCoords = The.InGameUI.OverlaySettings.EditorOverlayTypeSettings[Interface.Overlays.EditorOverlayTypes.Coords] == true;              
                                
                PrintEditorTileInfo(printCoords); //contained.Parent, renderIds); 
            }
        }       



        public static Plane CreatePlane(float height, Vector3 planeNormalDirection, bool clipSide)
        {
            planeNormalDirection.Normalize();
            Vector4 planeCoeffs = new Vector4(planeNormalDirection, height);

            if (clipSide)
                planeCoeffs *= -1;


            //Matrix worldViewProjection = currentViewMatrix * projection;
            //Matrix inverseWorldViewProjection = Matrix.Invert(worldViewProjection);
            //inverseWorldViewProjection = Matrix.Transpose(inverseWorldViewProjection);

            //planeCoeffs = Vector4.Transform(planeCoeffs, inverseWorldViewProjection);



            Plane finalPlane = new Plane(planeCoeffs);

            return finalPlane;
        }

        private void DrawLightsFromModelEmitters()
        {
            if (lightEmittingModels.Count > 0)
            {
                // EmissiveModelLightRenderTarget.clea
                The.Client.GraphicsDevice.SetRenderTargets(emissiveModelLightRenderTarget, emissiveModelLightDistanceRenderTarget);
                The.Client.GraphicsDevice.Clear(Color.Black);

                foreach (var renderable in lightEmittingModels)
                {
                    renderable.Draw(RenderTechnique.DrawModelEmitters, ref View, ref The.Client.Projection, 1f, 1f);

                    // draw emitters again to achieve a more blurred (anti-aliased?) look:
                    // doesn't work too good...
                    //   entity.Renderable./*TODO DECOUPLE*/RenderAsModel.ComputeMatricesForDrawing(AnimatedModel.Transformations.All, entity.Renderable./*TODO DECOUPLE*/RenderAsModel.FinalModelScale * 1.05f);

                    // entity.Draw(RenderTechnique.DrawModelEmitters, ref UWGame.SimSide.Instance.Map.Renderer.view, ref UWGame.SimSide.Instance.Projection, 0.15f, 1f) 

                    // entity.Renderable./*TODO DECOUPLE*/RenderAsModel.ComputeMatricesForDrawing(AnimatedModel.Transformations.All, entity.Renderable./*TODO DECOUPLE*/RenderAsModel.FinalModelScale * 3f);

                    //entity.Draw(RenderTechnique.DrawModelEmitters, ref UWGame.SimSide.Instance.Map.Renderer.view, ref UWGame.SimSide.Instance.Projection, 0.1f, 1f); 
                }
            }
        }

        private void DrawOverlayModels()
        {
            foreach (Renderable renderable in overlayModelEntities)
            {
                renderable.Draw(RenderTechnique.StandardOverlay,
                    ref View, ref The.Client.Projection, 0.25f, 1f);

                /*  renderable.RenderAsModel.ComputeMatricesForDrawing(AnimatedModel.Transformations.All, renderable.RenderAsModel.FinalModelScale * 1.05f);

                  // draw again to achieve a more blurred (anti-aliased?) look:
                  renderable.Draw(RenderTechnique.StandardOverlay, ref View, ref The.Client.Projection, 0.15f, 1f);*/
            }

        }


        private void CopyContainedBillboardShadowQuads(Renderable renderable, ref int featureQuadIndex)
        {
            foreach (RenderAsBillboard billboard in renderable.RenderAsBillboard)
            {
                billboard.CopyShadowQuadToVertexBuffer(featureVertices, ref featureQuadIndex);

            }
            /* foreach (ContainedBillboard billboard in entity.Renderable.RenderAsBillboard) 
             {
                 billboard.Renderable.RenderAsBillboard.CopyShadowQuadToVertexBuffer(featureVertices, ref featureQuadIndex);
                       
             }*/

        }

        /*  private void CopyContainedBillboardShadowQuads(Entity entity, ref int featureQuadIndex, int x, int y)
          {
              foreach (RenderAsBillboard billboard in entity.Renderable.RenderAsBillboard) //RenderAsBillboard billboard in entity.Renderable.RenderAsBillboard)
              {
                  if (billboard.MapPosition.X == x && billboard.MapPosition.Y == y)
                  {
                      billboard.CopyShadowQuadToVertexBuffer(featureVertices, ref featureQuadIndex);
                  }
              }
            
          }*/

        /*   private void CopyContainedBillboardQuads(Entity entity, ref int featureQuadIndex, int x, int y)
           {
               foreach (RenderAsBillboard billboard in entity.Renderable.RenderAsBillboard)
               {
                   if (billboard.MapPosition.X == x && billboard.MapPosition.Y == y)
                   {
                       billboard.CopyQuadToVertexBuffer(featureVertices, ref featureQuadIndex);

                   }
               }
           

           }*/

        int tileStartShadowsX;
        int tileEndShadowsX;
        int tileStartShadowsY;
        int tileEndShadowsY;

        private void DrawShadows(bool drawModels)
        {

            bool drawTrees = true;
#if DEBUG || PROFILE
            drawTrees = Kensei.Dev.Options.GetOption("Rendering.Render trees");
#endif
            bool drawItems = true;
#if DEBUG || PROFILE
            drawItems = Kensei.Dev.Options.GetOption("Rendering.Render items");
#endif



            SetShadowDrawing();

            int featureQuadIndex = 0;

            // shadows are unsorted...
            Renderable renderable;
            RenderAsBillboard renderAsBillboard;


            foreach (var drawObject in shadowsToDraw)
            {
                renderable = drawObject as Renderable;
                if (renderable != null)
                {
                    if (renderable.RenderAsModel != null)
                    {
                        //  if( ! entityToDraw.Stealth ) 

                        renderable.RenderAsModel.DrawShadow();
                    }

                    continue;
                }

                renderAsBillboard = drawObject as RenderAsBillboard;
                if (renderAsBillboard != null)
                {
                    CopyContainedBillboardShadowQuads(renderAsBillboard.Parent, ref featureQuadIndex);

                    continue;
                }
            }

            if (featureQuadIndex > 0)
            {
                DrawBillboardBatch(featureQuadIndex, RenderTechnique.NoLighting);
            }

            return;

        }

        private void SetupInterfaceOnMapQuads()
        {
            overlayGroundSpriteQuadIndex = 0;

            The.InGameUI.Selection.SetupQuad(overlayGroundSpriteVertices, ref overlayGroundSpriteQuadIndex);
            The.InGameUI.SelectedTiles.MapAreaRender.SetupQuad(overlayGroundSpriteVertices, ref overlayGroundSpriteQuadIndex);
            The.InGameUI.SelectedTilesPreview.MapAreaRender.SetupQuad(overlayGroundSpriteVertices, ref overlayGroundSpriteQuadIndex);
            The.InGameUI.SelectRectangle.SetupQuad(overlayGroundSpriteVertices, ref overlayGroundSpriteQuadIndex);

            if (The.InGameUI.ShowOverlaysAndMarkerWindows) // || The.InGameUI.Minimap.ShowZones)
            {
                if (The.InGameUI.UIExpedition.HasValue)
                {
                    Expedition expedition = Expedition.FindByID(The.InGameUI.UIExpedition.Value);
                    if (expedition != null)
                    {
                        foreach (var zone in expedition.OwnedEntities.Zones)
                        {
                            zone.MapArea.MapAreaRender.SetupQuad(overlayGroundSpriteVertices, ref overlayGroundSpriteQuadIndex);
                        }
                    }
                }

                /*
                foreach (var owner in The.Sim.PlaySite.AllOwners)
                {
                    if (owner.GetAllegiance() == The.InGameUI.UIAllegiance)
                    {
                        foreach (var zone in owner.Zones)
                        {
                           
                            zone.MapArea.MapAreaRender.SetupQuad(overlayGroundSpriteVertices, ref overlayGroundSpriteQuadIndex);
                           
                        }
                    }
                }*/
            }

            // add other selected/marked tiles here:
            // TODO: discomfort and user overlays:
            /*    if (map.Overlays.Count > 0)
                {
                    byte alpha;
                    Color c;
                    foreach (InfluenceMap iMap in map.Overlays)
                    {
                        InfluenceMap current = (InfluenceMap)iMap.GetCurrent();
                        if (current.Map[x + The.MapUI.mapX, y + The.MapUI.mapY] > 0)
                        {
                            if (current.IsBlocked[x + The.MapUI.mapX, y + The.MapUI.mapY])
                            {
                                alpha = 255;
                            }
                            else
                            {
                                alpha = (byte)(0.5f * current.Map[x + The.MapUI.mapX, y + The.MapUI.mapY]);

                                if (alpha > 0)
                                {
                                    alpha = (byte)Common.Clamp(alpha + 50, 0, 254);

                                }
                            }

                            c = new Color(current.Color.R, current.Color.G, current.Color.B, alpha);
                            UWGame.SimSide.Instance.spriteBatch.Draw(UWGame.SimSide.Instance.FlatSpriteSheet.Texture, destination,
                                UWGame.SimSide.Instance.FlatSpriteSheet.SourceRectangle("WhiteRectangle"), c);
                        }

                    }
                }*/


            // ??
            //The.InGameUI.Selection.CopyQuadToVertexBuffer(overlayGroundSpriteVertices, ref overlayGroundSpriteQuadIndex);
        }

        private void SetupInfluenceQuads()
        {
            influenceMapQuadIndex = 0;

          //  The.InGameUI.TerrainBlockingRender.SetupQuad(overlayGroundSpriteVertices, ref overlayGroundSpriteQuadIndex);
           // draw more overlays here...

            /*The.InGameUI.SelectedTiles.MapAreaRender.SetupQuad(overlayGroundSpriteVertices, ref overlayGroundSpriteQuadIndex);
            The.InGameUI.SelectedTilesPreview.MapAreaRender.SetupQuad(overlayGroundSpriteVertices, ref overlayGroundSpriteQuadIndex);
            The.InGameUI.SelectRectangle.SetupQuad(overlayGroundSpriteVertices, ref overlayGroundSpriteQuadIndex);
            */

            if (The.InGameUI.InterfaceMode == InGameInterface.InterfaceState.Build 
                || The.InGameUI.InterfaceMode == InGameInterface.InterfaceState.EditorPlaceEntity
                || The.InGameUI.OverlaySettings.OverlayTypeSettings[Interface.Overlays.OverlayTypes.BuildAreas] == true
                || The.InGameUI.OverlaySettings.EditorOverlayTypeSettings[Interface.Overlays.EditorOverlayTypes.BuildAreas] == true) 
            {
                The.InGameUI.TerrainBlockingRender.SetupQuad(influenceMapVertices, ref influenceMapQuadIndex);
            }


            if (The.InGameUI.OverlaySettings.OverlayTypeSettings[Interface.Overlays.OverlayTypes.Threats] == true)
            {
                The.InGameUI.ThreatRender.SetupQuad(influenceMapVertices, ref influenceMapQuadIndex);
            }
        
            // discomfort and user overlays:
          /*  if (map.Overlays.Count > 0)
            {
                byte alpha;
                Color c;
                foreach (InfluenceMap iMap in map.Overlays)
                {
                    InfluenceMap current = (InfluenceMap)iMap.GetCurrent();
                    if (current.Map[x + The.MapUI.mapX, y + The.MapUI.mapY] > 0)
                    {
                        if (current.IsBlocked[x + The.MapUI.mapX, y + The.MapUI.mapY])
                        {
                            alpha = 255;
                        }
                        else
                        {
                            alpha = (byte)(0.5f * current.Map[x + The.MapUI.mapX, y + The.MapUI.mapY]);

                            if (alpha > 0)
                            {
                                alpha = (byte)Common.Clamp(alpha + 50, 0, 254);

                            }
                        }

                        c = new Color(current.Color.R, current.Color.G, current.Color.B, alpha);
                        UWGame.SimSide.Instance.spriteBatch.Draw(UWGame.SimSide.Instance.FlatSpriteSheet.Texture, destination,
                            UWGame.SimSide.Instance.FlatSpriteSheet.SourceRectangle("WhiteRectangle"), c);
                    }

                }
            }*/

        }

        public float CorrectModelYPositionForDrawing(float yLocation)
        {

            float yDistanceFromCameraTarget = yLocation - CameraTarget.Y;
            return (float)(CameraTarget.Y + yDistanceFromCameraTarget / modelYCorrectionFactor);

        }

        /*  private void DrawRoadsPathsAndGroundSprites()
          {
              // first collect all the vertices in a batch:
              TerrainTile tileToDraw;
              roadsAndPathsQuadIndex = 0;
              for (int y = 0; y < The.MapUI.noOfTilesToDisplayVertically; y++)
              {
                  for (int x = 0; x < The.MapUI.noOfTilesToDisplayHorizontally; x++)
                  {
                      tileToDraw = map.TileMap[x + The.MapUI.mapX][y + The.MapUI.mapY];

                      tileToDraw.CopyRoadQuadsToVertexBuffer(roadsAndPathsVertices, ref roadsAndPathsQuadIndex);
                   
                  }
              }

              // NEW:
              DrawGroundSprites();

              // draw all of them:
              if (roadsAndPathsQuadIndex > 0)
              {
                  DrawRoadAndPathUserVertices(roadsAndPathsQuadIndex);
              }
          }

  */
        // draw order lists:
        List<IDrawnAsGroundSprite> bottomSprites = new List<IDrawnAsGroundSprite>(); // structure ground sprites such as torn up earth
        List<IDrawnAsGroundSprite> middleSprites = new List<IDrawnAsGroundSprite>(); // add-ons and roads
        List<IDrawnAsGroundSprite> topSprites = new List<IDrawnAsGroundSprite>(); // resources like firewood...
        List<IDrawnAsGroundSprite> outlineSprites = new List<IDrawnAsGroundSprite>();

        private void DrawGroundFeatureSprites()
        {

            // zones as well???

            Rectangle destination;
            TerrainTile tileToDraw;
            int xScreen, yScreen;

            groundFeatureQuadIndex = 0;

            bool drawGroundSprites = true;
#if DEBUG || PROFILE
            drawGroundSprites = Kensei.Dev.Options.GetOption("Rendering.Render ground sprites");
#endif


            // Draw the map
            TerrainTile[] tileColumn;

            SharedKnowledge knowledgeToShow = The.InGameUI.UIAllegiance.SharedKnowledge;

            /*  for (int x = 0; x < The.MapUI.noOfTilesToDisplayHorizontally; x++)
              {
                  tileColumn = The.Map.TileMap[x + The.MapUI.mapWindowTileX];

                  for (int y = 0; y < The.MapUI.noOfTilesToDisplayVertically; y++)
                  {
                        tileToDraw = tileColumn[y + The.MapUI.mapWindowTileY];
             */

            for (int x = TileStartX; x <= TileEndX; x++)
            {
                tileColumn = The.Map.TileMap[x];

                for (int y = TileStartY; y <= TileEndY; y++)
                {
                    tileToDraw = tileColumn[y];
                  
                    The.MapUI.TileToScreen(x, y, out xScreen, out yScreen);
                    destination = new Rectangle(xScreen, yScreen, MapManager.tileSize, MapManager.tileSize);

                  
                    // draw ground part of entities:
                    if (drawGroundSprites)
                    {
                       
                        if (tileToDraw.EdgeLayoutEntities != null)
                        {
                            // untested...
                            foreach (Entity feature in tileToDraw.EdgeLayoutEntities)
                            {
                                RenderAsGroundSprite groundSprite = feature.Renderable.RenderAsGroundSprite;
                                if (groundSprite != null) // feature.Renderable.Find(out groundSprite))
                                {
                                    middleSprites.Add(groundSprite);

                                    // groundSprite.CopyQuadToVertexBuffer(roadsAndPathsVertices, ref roadsAndPathsQuadIndex);
                                                                       
                                }
                            }
                        }

                        middleSprites.Add(tileToDraw); //TODO: move to Renderable/RenderAsGroundSprite
                        
                    }
                }
            }

            bottomSprites = bottomSprites.Distinct().ToList();
            middleSprites = middleSprites.Distinct().ToList();
            topSprites = topSprites.Distinct().ToList();

            DrawSetOfGroundSprites(bottomSprites);
            DrawSetOfGroundSprites(middleSprites);
            DrawSetOfGroundSprites(topSprites);

            // draw all of them:
            if (groundFeatureQuadIndex > 0)
            {
                DrawGroundFeatureUserVertices(groundFeatureQuadIndex);
            }


            bottomSprites.Clear();
            middleSprites.Clear();
            topSprites.Clear();
        }


        private void DrawSetOfGroundSprites(List<IDrawnAsGroundSprite> list)
        {
            foreach (IDrawnAsGroundSprite groundSprite in list)
            {
                groundSprite.CopyQuadToVertexBuffer(groundFeatureVertices, ref groundFeatureQuadIndex);
            }
        }


       

        public const int GutterSize = 2;
        public void GetEdgesOfTerrainToDraw(out int lastXToDraw, out int lastYToDraw, out int firstXToDraw, out int firstYToDraw)
        {
            lastXToDraw = The.MapUI.mapWindowTileX + The.MapUI.noOfTilesToDisplayHorizontally + GutterSize; // 1;  // add one for the edges! we are starting at -1 - that gives two extra tiles.
            lastYToDraw = The.MapUI.mapWindowTileY + The.MapUI.noOfTilesToDisplayVertically + GutterSize; // 1; // NEW: add one extra layer of vertices.

            firstXToDraw = The.MapUI.mapWindowTileX - GutterSize;
            firstYToDraw = The.MapUI.mapWindowTileY - GutterSize;
        }

        private void GetEdgesOfTerrainToDraw(float size, Vector2 position, out int lastXToDraw, out int lastYToDraw, out int firstXToDraw, out int firstYToDraw)
        {
            int noOfTilesToDisplayHorizontally = (int)Math.Ceiling((decimal)size / (decimal)MapManager.tileSize);
            int noOfTilesToDisplayVertically = (int)Math.Ceiling((decimal)size / (decimal)MapManager.tileSize);
            lastXToDraw = Common.Min(terrainTilePositions.Length - 2, MapManager.WorldPosToTile(position).X + noOfTilesToDisplayHorizontally + GutterSize); // 1;  // add one for the edges! we are starting at -1 - that gives two extra tiles.
            lastYToDraw = Common.Min(terrainTilePositions[0].Length - 2, MapManager.WorldPosToTile(position).Y + noOfTilesToDisplayVertically + GutterSize); // 1; // NEW: add one extra layer of vertices.

            firstXToDraw = MapManager.WorldPosToTile(position).X - GutterSize;
            firstYToDraw = MapManager.WorldPosToTile(position).Y - GutterSize;
        }

        /// <summary>
        /// Recompute visible vertices only when we are scrolling
        /// </summary>
        private bool first = false;
        public void SetUpTerrainVerticesAndIndicesInCurrentView()
        {

            if (!The.MapUI.IsScrolling && terrainBatches != null && terrainBatches.Count > 0 && The.MapUI.RenderedTerrainIsDirty == false)
            {
                return; // only recompute when something changed.
            }

            // poor fps when scrolling...

            The.MapUI.RenderedTerrainIsDirty = false;

            terrainBatches.Clear();

            int lastXToDraw, lastYToDraw, firstXToDraw, firstYToDraw;
            GetEdgesOfTerrainToDraw(out lastXToDraw, out lastYToDraw, out firstXToDraw, out firstYToDraw);


            // Vector4 deepColor = new Vector4(0.9f, 0.95f, 0.9f, 1f); //new Vector4(0.1f, 0.1f, 0.2f, 1f);


            CreateBatchesOfTerrainTypesToDraw(lastXToDraw, lastYToDraw, firstXToDraw, firstYToDraw, terrainBatches);


            // set up all the vertices we need:
            Parallel.ForEach(terrainBatches, batch =>
            //  foreach (TerrainBatch batch in batches)
            {
                // VertexMultitextured[] terrainVertices = batch.terrainVertices;
                // int terrainVertexIndex = 0;

                //    TerrainTile tileToDraw;
                //Terrain terrainToDraw;
                // float xPos, yPos;

                // int closestXOnMap, closestYOnMap;

                // we may start/end outside the actual map! uses the special gutter terrain objects for that...
                for (int y = firstYToDraw; y < lastYToDraw; y++)
                {
                    for (int x = firstXToDraw; x < lastXToDraw; x++)
                    {
                        // closestXOnMap = Common.Clamp(x, 0, map.mapWidth - 1);
                        //  closestYOnMap = Common.Clamp(y, 0, map.mapHeight - 1);

                        // tileToDraw = map.TileMap[closestXOnMap][closestYOnMap];



                        CreateTerrainTrianglesToTheRightAndDown(x, y, lastXToDraw, lastYToDraw, batch);

                    }
                }
            }); // P-For


            //return batches;

        }

        public void SetUpTerrainVerticesAndIndicesInCurrentView(float size, Vector2 position, out List<TerrainBatch> terrainBatchList)
        {
            terrainBatchList = new List<TerrainBatch>();
           

            The.MapUI.RenderedTerrainIsDirty = false;
            // terrainBatches.Clear();

            int lastXToDraw, lastYToDraw, firstXToDraw, firstYToDraw;
            GetEdgesOfTerrainToDraw(size, position, out lastXToDraw, out lastYToDraw, out firstXToDraw, out firstYToDraw);


            CreateBatchesOfTerrainTypesToDraw(lastXToDraw, lastYToDraw, firstXToDraw, firstYToDraw, terrainBatchList);            

            Parallel.ForEach(terrainBatchList, batch =>
            {
                for (int y = firstYToDraw; y < lastYToDraw; y++)
                {
                    for (int x = firstXToDraw; x < lastXToDraw; x++)
                    {
                        CreateTerrainTrianglesToTheRightAndDown(x, y, lastXToDraw, lastYToDraw, batch);
                    }
                }
            });

        }


        private void CreateBatchesOfTerrainTypesToDraw(int lastXToDraw, int lastYToDraw, int firstXToDraw, int firstYToDraw, List<TerrainBatch> batches)
        {
            List<TerrainBatch> renderAsRockBatches = new List<TerrainBatch>();


            // TerrainBatch terrainBatch = new TerrainBatch(noOfVerticesHorizontal, noOfVerticesVertical);
            TerrainBatch terrainBatch = new TerrainBatch();

            // draw the earth first... it can be batched with others. "Ground rock"?            
            terrainBatch.terrainInBatch.Add(GameData.Instance.AllSoilComponentTypes["soil:groundrock"]); // "earth"); // special case... otherwise we use keyname for vegetation.


            batches.Add(terrainBatch);


            //  List<TerrainTile> tilesToDraw = new List<TerrainTile>();

            List<RenderedTerrainType> terrainTypesToSortIntoBatches = new List<RenderedTerrainType>();

            TerrainTile tileToDraw;

            List<string> alreadyBatched = new List<string>();
            for (int y = firstYToDraw; y < lastYToDraw; y++)
            {
                for (int x = firstXToDraw; x < lastXToDraw; x++)
                {

                    int closestXOnMap = Common.Clamp(x, 0, The.Map.mapTileWidth - 1);
                    int closestYOnMap = Common.Clamp(y, 0, The.Map.mapTileHeight - 1);

                    tileToDraw = The.Map.TileMap[closestXOnMap][closestYOnMap];
                    //   tilesToDraw.Add(tileToDraw);

                    if (tileToDraw.Terrain != null)
                    {
                        if (tileToDraw.Terrain.SoilComponents != null)
                        {
                            AddSoilTypesToDraw(tileToDraw.Terrain, renderAsRockBatches, terrainTypesToSortIntoBatches, alreadyBatched);
                        }
                    }
                    else
                    {
                        for (int sx = 0; sx < 3; sx++)
                        {
                            for (int sy = 0; sy < 3; sy++)
                            {
                                AddSoilTypesToDraw(tileToDraw.TerrainSubtiles[sx][sy], renderAsRockBatches, terrainTypesToSortIntoBatches, alreadyBatched);
                            }
                        }

                    }
                }
            }


            for (int y = firstYToDraw; y < lastYToDraw; y++)
            {
                for (int x = firstXToDraw; x < lastXToDraw; x++)
                {

                    int closestXOnMap = Common.Clamp(x, 0, The.Map.mapTileWidth - 1);
                    int closestYOnMap = Common.Clamp(y, 0, The.Map.mapTileHeight - 1);

                    tileToDraw = The.Map.TileMap[closestXOnMap][closestYOnMap];
                    //   tilesToDraw.Add(tileToDraw);

                    if (tileToDraw.Terrain != null)
                    {
                        if (tileToDraw.Terrain.Vegetation != null)
                        {
                            AddVegetationTypesToDraw(tileToDraw.Terrain, terrainTypesToSortIntoBatches, alreadyBatched);
                        }
                    }
                    else if (tileToDraw.TerrainSubtiles != null)
                    {
                        for (int sx = 0; sx < 3; sx++)
                        {
                            for (int sy = 0; sy < 3; sy++)
                            {
                                AddVegetationTypesToDraw(tileToDraw.TerrainSubtiles[sx][sy], terrainTypesToSortIntoBatches, alreadyBatched);
                            }
                        }
                    }
                }
            }

            // sort the soil batches:
            terrainTypesToSortIntoBatches.Sort();

            foreach (RenderedTerrainType terrainType in terrainTypesToSortIntoBatches)
            {
                if (terrainBatch.terrainInBatch.Count == terrainsPerBatch)
                {
                    terrainBatch = new TerrainBatch(); //noOfVerticesHorizontal, noOfVerticesVertical);
                    batches.Add(terrainBatch);
                }
                terrainBatch.terrainInBatch.Add(terrainType);
                //alreadyBatched.Add(vegType.Value.LowVegetationType.KeyName);
            }

            renderAsRockBatches.Sort();

            // these need to be drawn last (transparency/ alpha blending issues):
            batches.AddRange(renderAsRockBatches);
        }

        private void AddSoilTypesToDraw(Terrain terrain, List<TerrainBatch> renderAsRockBatches, List<RenderedTerrainType> terrainTypesToSortIntoBatches, List<string> alreadyBatched)
        {
            if (terrain.SoilComponents != null)
            {
                foreach (var soilType in terrain.SoilComponents)
                {
                    if (soilType.Value.Amount > 0f)
                    {
                        if (!alreadyBatched.Contains(soilType.Value.SoilComponentType.KeyName))
                        {
                            if (soilType.Value.SoilComponentType.RenderAsRocksType != null) //.HasTransparency)
                            {
                                // this terrain will be drawn last, in its own batch:
                                TerrainBatch rockBatch = new TerrainBatch(); //noOfVerticesHorizontal, noOfVerticesVertical);
                                rockBatch.RenderAsRocks = true;
                                rockBatch.terrainInBatch.Add(soilType.Value.SoilComponentType); //.Value.SoilComponentType.KeyName);
                                renderAsRockBatches.Add(rockBatch);
                            }
                            else
                            {
                                /* if (terrainBatch.terrainInBatch.Count == terrainsPerBatch)
                                 {
                                     terrainBatch = new TerrainBatch(noOfVerticesHorizontal, noOfVerticesVertical);
                                     batches.Add(terrainBatch);
                                 }
                                 terrainBatch.terrainInBatch.Add(soilType.Value.SoilComponentType); //soilType.Value.SoilComponentType.KeyName);
                                 */
                                terrainTypesToSortIntoBatches.Add(soilType.Value.SoilComponentType);

                            }

                            alreadyBatched.Add(soilType.Value.SoilComponentType.KeyName);
                        }
                    }
                }
            }
        }

        private static double a = 0;//dummy var to modulate test tint color MLo

        /// <summary>
        /// we need to supply tile coords because they are not the same as the tile data when we are drawing the map gutter.
        /// </summary>
        /// <param name="tile"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="sx"></param>
        /// <param name="sy"></param>
        /// <param name="batch"></param>
        private static void SetupTerrainVertex(TerrainTilePosition tile, int? sx, int? sy, /*float xPos, float yPos,*/ TerrainBatch batch) //, List<VertexMultitextured> terrainVertices, List<ushort> terrainIndices) //, ref ushort terrainVertexIndex) //, /* VertexMultitextured[] terrainVertices,*/ 
        {
            VertexMultitextured vertex = new VertexMultitextured();

            //  float xPos, yPos;
            /* 
             int x = tile.Value.TerrainTile.X;
             int y = tile.Value.TerrainTile.Y;

          */
            Terrain terrain;
            TerrainPosition tp;
            if (tile.TerrainTile.Terrain != null)
            {
                //terrain = tile.TerrainTile.Terrain;
                tp = tile.TerrainPosition;
                terrain = tp.Terrain;
                // MapManager.TileToWorldPos(x, y, out xPos, out yPos);
            }
            else
            {
                //terrain = tile.TerrainTile.TerrainSubtiles[sx.Value][sy.Value];
                tp = tile.TerrainSubtilePositions[sx.Value][sy.Value];
                terrain = tp.Terrain;
                /*   xPos = x * MapManager.tileSize + sx.Value * MapManager.subTileSize + MapManager.subTileSizeOver2;
                   yPos = y * MapManager.tileSize + sy.Value * MapManager.subTileSize + MapManager.subTileSizeOver2;
                    */
            }

            vertex.Position = tp.RenderPosition;
            vertex.TextureCoordinate = tp.RenderTextureCoordinate;

            /*
            vertex.Position = new Vector3(xPos, yPos, MapManager.TerrainZLevel + terrain.TerrainDepth);

            // NEW: Use 512 instead for proper wrapping on old cards!
            vertex.TextureCoordinate.X = xPos / 512f;
            vertex.TextureCoordinate.Y = yPos / 512f;
            */


            // set vegetation data and everything else

            // only draw two biggest values of vegetation, rocks, sand etc.?
            UWGame.SimSide.Vegetation.LowVegetation vegetation;
            SoilComponent soil;

            //bool fog = ! tile.TerrainTile.AllegiancesThatSeeThisTile.Contains(The.Sim.Site.PlayerAllegiance); 

            for (int i = 0; i < terrainsPerBatch; i++)
            {
                float weight = 0f;
                Vector4 tintColor = Vector4.One; //Vector4.Zero;

                int noiseChannelToUse = 0;
                float noiseScaling = 1;

                //   RenderPerlinNoise renderPerlin = RenderPerlinNoise;
                float renderPerlinSharpness = 1f;

                if (i < batch.terrainInBatch.Count) // this loop operates on the SAME vertex! There is room in the struct for 3 sets of terrain info.
                {
                    //string terrainKeyName = batch.terrainInBatch[i].KeyName;
                    RenderedTerrainType terrainType = batch.terrainInBatch[i];

                   
                    if (terrainType.IsBaseTerrain) // terrainKeyName == "soil:groundrock")
                    {
                        weight = 1f;
                        tintColor = Vector4.One;

                        //if (fog)
                        //{
                        //    tintColor.X *= 0.33f;
                        //    tintColor.Y *= 0.33f;
                        //    tintColor.Z *= 0.33f;
                        //}

                    }
                    else
                    {
                        if (terrain.Vegetation != null && terrainType is LowVegetationType && terrain.Vegetation.TryGetValue((LowVegetationType)terrainType, out vegetation))
                        {
                            weight = vegetation.DisplayAmount;
                            tintColor = Vector4.One;

                            noiseChannelToUse = vegetation.LowVegetationType.GetPerlinNoiseChannel();

                            renderPerlinSharpness = vegetation.LowVegetationType.RenderPerlinNoiseSharpness;
                            noiseScaling = vegetation.LowVegetationType.NoiseScaling;

                        }
                        else if (terrain.SoilComponents != null && terrainType is SoilComponentType && terrain.SoilComponents.TryGetValue((SoilComponentType)terrainType, out soil))
                        {
                            weight = soil.DisplayAmount;
                            tintColor = Vector4.Lerp(soil.SoilComponentType.DryTintAsVector, soil.SoilComponentType.WetTintAsVector, terrain.Parent.Moisture);

                            noiseChannelToUse = soil.SoilComponentType.GetPerlinNoiseChannel();

                            renderPerlinSharpness = soil.SoilComponentType.RenderPerlinNoiseSharpness;
                            noiseScaling = soil.SoilComponentType.NoiseScaling;
                        }
                    }

                    /*   if (tileToDraw.IsUnderWater())
                       {
                           tintColor = tileToDraw.WaterBottomTint; // deepColor; // Vector4.Lerp(Vector4.One, deepColor, (tileToDraw.LevelBelowWater / global::UWGame.SimSide.Maps.Water.Water.WaterDepthForDeepestBlue));
                       }*/
                }

                // tintColor.Y *= (float)Math.Sin(a += 0.01d);

                switch (i)
                {
                    case 0:
                        vertex.TexWeights.X = weight;
                        vertex.TintColor0 = tintColor;
                        //  terrainVertices[terrainVertexIndex].NoiseChannelToUse.X = noiseChannelToUse; //GetPerlinToUse(renderPerlin); // (usePerlinNoise ? 12f : 0f);
                        vertex.AlphaSharpness.X = renderPerlinSharpness;
                        vertex.NoiseScaling.X = noiseScaling;
                        break;
                    case 1:
                        vertex.TexWeights.Y = weight;
                        vertex.TintColor1 = tintColor;
                        //   terrainVertices[terrainVertexIndex].NoiseChannelToUse.Y = noiseChannelToUse; //GetPerlinToUse(renderPerlin);
                        vertex.AlphaSharpness.Y = renderPerlinSharpness;
                        vertex.NoiseScaling.Y = noiseScaling;
                        break;
                    case 2:
                        vertex.TexWeights.Z = weight;
                        vertex.TintColor2 = tintColor;
                        //    terrainVertices[terrainVertexIndex].NoiseChannelToUse.Z = noiseChannelToUse; // GetPerlinToUse(renderPerlin);
                        vertex.AlphaSharpness.Z = renderPerlinSharpness;
                        vertex.NoiseScaling.Z = noiseScaling;
                        break;
                }

            }

            batch.terrainVerticesList.Add(vertex);

            //   terrainIndices.Add(terrainVertexIndex);
            batch.terrainIndicesList.Add((short)batch.terrainIndicesList.Count); // terrainVertexIndex);
            //terrainVertexIndex++;

            // terrainVertexIndex++;

        }


        /*
            foreach (TerrainBatch batch in batches)
            {
                VertexMultitextured[] terrainVertices = batch.terrainVertices;
                int terrainVertexIndex = 0;


                for (int y = firstYToDraw; y < lastYToDraw; y++)
                {
                    yPos = y * MapManager.tileSize + MapManager.tileSizeOver2;

                    for (int x = firstXToDraw; x < lastXToDraw; x++)
                    {
                        xPos = x * MapManager.tileSize + MapManager.tileSizeOver2;

                        int closestXOnMap = Common.Clamp(x, 0, map.mapWidth - 1);
                        int closestYOnMap = Common.Clamp(y, 0, map.mapHeight - 1);

                        tileToDraw = map.TileMap[closestXOnMap][closestYOnMap];


                        terrainVertices[terrainVertexIndex].Position = new Vector3(xPos, yPos, MapManager.TerrainZLevel + tileToDraw.TerrainDepth);
                        // NEW:
                        //   terrainVertices[terrainVertexIndex].WorldPosition = new Vector3(xPos, yPos, MapManager.TerrainZLevel + tileToDraw.TerrainDepth);

                        // NEW: Use 512 instead for proper wrapping on old cards!
                        terrainVertices[terrainVertexIndex].TextureCoordinate.X = xPos / 512f;
                        terrainVertices[terrainVertexIndex].TextureCoordinate.Y = yPos / 512f;

                        // set vegetation data and everything else

                        // only draw two biggest values of vegetation, rocks, sand etc.?
                        Vegetation.LowVegetation vegetation;
                        SoilComponent soil;
                        //  Short4 noiseChannelsToUse = new Microsoft.Xna.Framework.Graphics.PackedVector.Short4();
                        // Short4 noiseTexturesToUse = new Microsoft.Xna.Framework.Graphics.PackedVector.Short4();


                        for (int i = 0; i < terrainsPerBatch; i++)  // batch.terrainInBatch.Count; i++)
                        {
                            float weight = 0f;
                            Vector4 tintColor = Vector4.One; //Vector4.Zero;
                            //bool usePerlinNoise = true;
                           

                            int noiseChannelToUse = 0;
                            float noiseScaling = 1;

                            //   RenderPerlinNoise renderPerlin = RenderPerlinNoise;
                            float renderPerlinSharpness = 1f;

                            if (i < batch.terrainInBatch.Count)
                            {
                                string terrainKeyName = batch.terrainInBatch[i].KeyName;


                                if (terrainKeyName == "soil:groundrock")
                                {
                                    weight = 1f;
                                    tintColor = Vector4.One;
                                }
                                else
                                {
                                    if (tileToDraw.Vegetation != null && tileToDraw.Vegetation.TryGetValue(terrainKeyName, out vegetation))
                                    {
                                        weight = vegetation.DisplayAmount; //Growth;
                                        tintColor = Vector4.One;

                                        noiseChannelToUse = vegetation.LowVegetationType.GetPerlinNoiseChannel(); ;


                                        renderPerlinSharpness = vegetation.LowVegetationType.RenderPerlinNoiseSharpness;
                                        noiseScaling = vegetation.LowVegetationType.NoiseScaling;
                                    }
                                    else if (tileToDraw.SoilComponents != null && tileToDraw.SoilComponents.TryGetValue(terrainKeyName, out soil))
                                    {
                                        weight = soil.DisplayAmount;
                                        tintColor = Vector4.Lerp(soil.SoilComponentType.DryTintAsVector, soil.SoilComponentType.WetTintAsVector, tileToDraw.Moisture);
                                        //  renderPerlin = soil.SoilComponentType.RenderWithPerlinNoise;

                                        noiseChannelToUse = soil.SoilComponentType.GetPerlinNoiseChannel();


                                        renderPerlinSharpness = soil.SoilComponentType.RenderPerlinNoiseSharpness;
                                        noiseScaling = soil.SoilComponentType.NoiseScaling;
                                    }
                                }

                            }


                            switch (i)
                            {
                                case 0:
                                    terrainVertices[terrainVertexIndex].TexWeights.X = weight;
                                    terrainVertices[terrainVertexIndex].TintColor0 = tintColor;
                                    //  terrainVertices[terrainVertexIndex].NoiseChannelToUse.X = noiseChannelToUse; //GetPerlinToUse(renderPerlin); // (usePerlinNoise ? 12f : 0f);
                                    terrainVertices[terrainVertexIndex].AlphaSharpness.X = renderPerlinSharpness;
                                    terrainVertices[terrainVertexIndex].NoiseScaling.X = noiseScaling;
                                    break;
                                case 1:
                                    terrainVertices[terrainVertexIndex].TexWeights.Y = weight;
                                    terrainVertices[terrainVertexIndex].TintColor1 = tintColor;
                                    //   terrainVertices[terrainVertexIndex].NoiseChannelToUse.Y = noiseChannelToUse; //GetPerlinToUse(renderPerlin);
                                    terrainVertices[terrainVertexIndex].AlphaSharpness.Y = renderPerlinSharpness;
                                    terrainVertices[terrainVertexIndex].NoiseScaling.Y = noiseScaling;
                                    break;
                                case 2:
                                    terrainVertices[terrainVertexIndex].TexWeights.Z = weight;
                                    terrainVertices[terrainVertexIndex].TintColor2 = tintColor;
                                    //    terrainVertices[terrainVertexIndex].NoiseChannelToUse.Z = noiseChannelToUse; // GetPerlinToUse(renderPerlin);
                                    terrainVertices[terrainVertexIndex].AlphaSharpness.Z = renderPerlinSharpness;
                                    terrainVertices[terrainVertexIndex].NoiseScaling.Z = noiseScaling;
                                    break;
                              

                            }

                        }

                    
                        // Short4(1, 0, 0, 0); -> rgb = 001                       

                        terrainVertexIndex++;
                    }
                }
            }
         
         */

        private static void AddVegetationTypesToDraw(Terrain terrain, List<RenderedTerrainType> terrainTypesToSortIntoBatches, List<string> alreadyBatched)
        {
            if (terrain.Vegetation != null)
            {
                foreach (var vegType in terrain.Vegetation)
                {
                    if (vegType.Value.DisplayAmount > 0f)
                    {
                        if (!alreadyBatched.Contains(vegType.Value.LowVegetationType.KeyName))
                        {
                            terrainTypesToSortIntoBatches.Add(vegType.Value.LowVegetationType);
                            alreadyBatched.Add(vegType.Value.LowVegetationType.KeyName);
                        }
                    }
                }
            }
        }

        /*    private float GetPerlinToUse(RenderPerlinNoise renderPerlinNoise)
            {
                switch (renderPerlinNoise)
                {
                    case RenderPerlinNoise.None: return 0f;
                    case RenderPerlinNoise.Small: return 1f;
                    case RenderPerlinNoise.Big: return 2f;
                }
                return 0f;
            }*/

        public void SetShadowDrawing()
        {
            //The.Client.GraphicsDevice.SetRenderTarget(0, ShadowRenderTarget); // XNA 3
            The.Client.GraphicsDevice.SetRenderTarget(shadowRenderTarget);
            // The.Client.GraphicsDevice.DepthStencilBuffer = game.ScreenManager.NoMultiSamplingStencilBuffer; // XNA 3

           
            // The.Client.GraphicsDevice.RenderState.AlphaBlendEnable = true;
            The.Client.GraphicsDevice.BlendState = BlendState.AlphaBlend;

            Color clearColor = new Color(1f, 1f, 1f, 0f);
            The.Client.GraphicsDevice.Clear(clearColor); //Color.White);


            /*   graphics.GraphicsDevice.Clear(ClearOptions.Stencil, Color.Black, 0, 0);

            
               graphics.GraphicsDevice.RenderState.StencilEnable = true;
               graphics.GraphicsDevice.RenderState.ColorWriteChannels = ColorWriteChannels.None;
               graphics.GraphicsDevice.RenderState.ReferenceStencil = 0;
               graphics.GraphicsDevice.RenderState.StencilFunction = CompareFunction.Equal;
               graphics.GraphicsDevice.RenderState.StencilPass = StencilOperation.Increment;*/
        }

        /*    public void EndShadowDrawing()
            {
                game.graphics.GraphicsDevice.SetRenderTarget(0, EdgeDetectSceneRenderTarget);
          
                // draw the finished shadow map:
                //Texture2D shadowMap = UWGame.SimSide.Instance.ShadowRenderTarget.GetTexture();
                //shadowMap.Save("shadowMap.png", ImageFileFormat.Png);

                TimeOfDayLightingEffect.Parameters["baseTexture"].SetValue(ShadowRenderTarget.GetTexture());
                TimeOfDayLightingEffect.Parameters["alphaFactor"].SetValue(The.Sim.DateAndTime.GetDropShadowAlphaFactor());
                TimeOfDayLightingEffect.CurrentTechnique = TimeOfDayLightingEffect.Techniques["ApplyShadowMap"];
                TimeOfDayLightingEffect.Begin();
                foreach (EffectPass pass in TimeOfDayLightingEffect.CurrentTechnique.Passes)
                {
                    pass.Begin();
                    game.quadRenderer.Render(-Vector2.One, Vector2.One);
                    pass.End();
                }

                TimeOfDayLightingEffect.End();

            }*/

        public void DrawCloudAndDropShadows()
        {
            // uses DistanceHeightAndBillboardAlphaRenderTarget and ShadowRenderTarget as parameters
            The.Client.GraphicsDevice.BlendState = BlendState.NonPremultiplied; // overridden in shader..?

            // draw the finished shadow map:          

            //   SaveRenderTargetToFile("ShadowRenderTarget", ShadowRenderTarget); // OK

            CloudShadowsEffect.CurrentTechnique = CloudShadowsEffect.Techniques["RenderCloudShadows"];

            //   depthmap.Save("depthMap.png", ImageFileFormat.Png);
            CloudShadowsEffect.Parameters["BillboardDepthHeightMap"].SetValue(DistanceHeightAndBillboardAlphaRenderTarget);

            //  SaveRenderTargetToFile("DistanceHeightAndBillboardAlphaRenderTarget", DistanceHeightAndBillboardAlphaRenderTarget); // ok

            CloudShadowsEffect.Parameters["GroundDropShadowTexture"].SetValue(shadowRenderTarget);
            
            CloudShadowsEffect.Parameters["CloudTexture"].SetValue(cloudShadowTexture);
            CloudShadowsEffect.Parameters["CloudEdgeSharpness"].SetValue(The.MapUI.CloudSharpness);

           // Viewport viewport = The.Client.GraphicsDevice.Viewport;
            Dimension dim = The.Client.Controller.DrawArea;
            Vector2 viewportSize = new Vector2(dim.Width, dim.Height);

            //vieportSize is involved in offsetting the clouds to the current viewport scroll location

            CloudShadowsEffect.Parameters["ViewportSize"].SetValue(viewportSize);
            CloudShadowsEffect.Parameters["WindowPosition"].SetValue(The.MapUI.MapWindowWorldPosition);

            CloudShadowsEffect.Parameters["CloudCoverLimit"].SetValue(1f - The.Sim.PlaySite.PlaySite.Weather.CloudCover);
            CloudShadowsEffect.Parameters["CloudPosition"].SetValue(The.Sim.PlaySite.PlaySite.Weather.CloudPosition);
            CloudShadowsEffect.Parameters["ShadowAlpha"].SetValue(DayAndNightEffects.GetDropShadowAlphaFactor() * The.MapUI.CloudOpacity);


            // TODO: can we clamp drawing to stay within the map???

            foreach (EffectPass pass in CloudShadowsEffect.CurrentTechnique.Passes)
            {              
                pass.Apply();
                The.Client.quadRenderer.Render(The.Client.GraphicsDevice, - Vector2.One, Vector2.One);               
            }


        }




        /*   private void CopyToTerrainBuffers(VertexMultitextured[] vertices, int[] indices)
           {
               terrainVertexBuffer = new VertexBuffer(game.graphics.GraphicsDevice, vertices.Length * VertexMultitextured.SizeInBytes, BufferUsage.WriteOnly);
               terrainVertexBuffer.SetData(vertices);

               terrainIndexBuffer = new IndexBuffer(game.graphics.GraphicsDevice, typeof(int), indices.Length, BufferUsage.WriteOnly);
               terrainIndexBuffer.SetData(indices);
           }*/

        public void DrawTerrainUserVertices(/*Matrix currentViewMatrix,*/
            List<TerrainBatch> batches, Plane? clippingPlane, Vector2 position, Vector2 renderTargetSize, List<TerrainBatch> terrainSliceBatches = null)
        {           
            
            /*
             * SM - Shader Model (Pixel Shader version?)
             SM 1.1 - 1.3: 4 samplers one read per sampler.
            SM 1.4 : 6 samplers two reads per sampler.
            SM 2.0 : 16 samplers ; 32 reads
            SM 2.A : 16 samplers ; 512 reads
            SM 2.B : 16 samplers ; 512 reads
            SM 3.0 : 16 samplers ; >= 512 reads
            SM 4.0 : 16 samplers (can read from 128 diffrent resources) ; nearly unlimited reads.
             * 
             * game.graphics.GraphicsDevice.GraphicsDeviceCapabilities.MaxSimultaneousTextures = 8???
            */
            // batch at instruction limit or texture limit?
            // BlendState terrainState = new BlendState();

            //  The.Client.GraphicsDevice.RenderState.CullMode = CullMode.None; // XNA 3
            The.Client.GraphicsDevice.RasterizerState = RasterizerState.CullNone;

           
            //  The.Client.GraphicsDevice.RenderState.DepthBufferEnable = true; // XNA 3
            The.Client.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            // The.Client.GraphicsDevice.RenderState.AlphaBlendEnable = true; // XNA 3
            The.Client.GraphicsDevice.BlendState = BlendState.AlphaBlend;

            // pre multipied alpha belnding:
            // http://blogs.msdn.com/b/shawnhar/archive/2009/11/10/premultiplied-alpha-in-xna-game-studio.aspx
            // The.Client.GraphicsDevice.RenderState.SourceBlend = Blend.One; // XNA 3
            //  The.Client.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;

            // is now default with XNA 4:
            // http://blogs.msdn.com/b/shawnhar/archive/2010/04/08/premultiplied-alpha-in-xna-game-studio-4-0.aspx
            // The.Client.GraphicsDevice.BlendState = BlendState.AlphaBlend;
                      
            //  SaveTextureToFile("BillboardSpriteSheet", GameData.Instance.BillboardSpriteSheet.Texture);

            // int passNo = 0;

            terrainEffect.Parameters["UseIntegerPositions"].SetValue(!The.MapUI.IsScrolling);

            terrainEffect.Parameters["perlinTexture"].SetValue(perlinTexture);

          /*  Matrix worldMatrix = Matrix.Identity;
            terrainEffect.Parameters["xWorld"].SetValue(worldMatrix);
            terrainEffect.Parameters["xView"].SetValue(currentViewMatrix);
            terrainEffect.Parameters["xProjection"].SetValue(The.Client.Projection);
            */



            // NEW!
           // Viewport viewport = The.Client.GraphicsDevice.Viewport; // gets the size of the rendertarget!
            //Dimension dim = The.Client.Controller.DrawArea;
            //Vector2 viewportSize = new Vector2(dim.Width, dim.Height);
           // Vector2 viewportSize = new Vector2(dim.Width / 2f, dim.Height);
            terrainEffect.Parameters["ViewportSize"].SetValue(renderTargetSize);
            
           /* if (Position == null)
            {
                terrainEffect.Parameters["WindowPosition"].SetValue(The.MapUI.MapWindowWorldPosition);
            }
            else
            {*/
                terrainEffect.Parameters["WindowPosition"].SetValue(position);
           // }
            terrainEffect.Parameters["NearPlane"].SetValue(The.Client.NearPlane);
            terrainEffect.Parameters["FarPlane"].SetValue(The.Client.FarPlane);
            terrainEffect.Parameters["ZOffset"].SetValue(2500f); // where does this value come from...?

            if (clippingPlane.HasValue)
            {
                terrainEffect.Parameters["ClipPlane0"].SetValue(new Vector4(clippingPlane.Value.Normal, clippingPlane.Value.D));
                terrainEffect.Parameters["DoClipping"].SetValue(true);
            }
            else
            {
                terrainEffect.Parameters["DoClipping"].SetValue(false);
            }

            bool useWireframe = false;
            if (sim.Mode == SimSide.Sim.EngineMode.Edit)
            {
                useWireframe = The.InGameUI.OverlaySettings.EditorOverlayTypeSettings[Interface.Overlays.EditorOverlayTypes.TerrainDivision] == true;
            }
            else
            {

#if DEBUG || PROFILE
                if (Kensei.Dev.Options.GetOption("Overlays.Terrain division")) // only has an effect at startup because we cache the rsult
                {
                    useWireframe = true; // The.Client.GraphicsDevice.RasterizerState = rasterizerStateWireframe;
                }
                else
                {
                    useWireframe = false; // The.Client.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
                }
#endif
            }

            if (useWireframe)
            {
                The.Client.GraphicsDevice.RasterizerState = rasterizerStateWireframe;
            }
            else
            {
                The.Client.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            }


          //  The.Client.GraphicsDevice.RasterizerState = The.Client.Renderer.rasterizerStateWireframe;
        


            if (terrainSliceBatches == null)
            {
                DrawTerrainUsingBatchList(batches);
            }
            else
            {
                DrawTerrainUsingBatchList(terrainSliceBatches);
            }



            The.Client.GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            // pre multiplied alpha blending:
            // http://blogs.msdn.com/b/shawnhar/archive/2009/11/10/premultiplied-alpha-in-xna-game-studio.aspx
            // The.Client.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;  // XNA 3
            //  The.Client.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha; // XNA 3
            //
        }

        private void DrawTerrainUsingBatchList(List<TerrainBatch> batchlist)
        {
            VertexMultitextured[] terrainVertices;
            short[] terrainIndices;
            foreach (TerrainBatch batch in batchlist)
            {
                /* if (batch.HasTransparency)
                 {
                     // pre multiplied alpha blending:
                     // http://blogs.msdn.com/b/shawnhar/archive/2009/11/10/premultiplied-alpha-in-xna-game-studio.aspx
                     game.graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                     game.graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                     //
                 }*/

                if (batch.RenderAsRocks)
                {


#if DEBUG || PROFILE
                    if (Kensei.Dev.Options.GetOption("Rendering.Show light amount"))
                    {
                        terrainEffect.CurrentTechnique = terrainEffect.Techniques["DebugRenderRocksSingleLayer" + batch.terrainInBatch[0].GetPerlinNoiseChannel().ToString()];
                    }
                    else
                    {
                        terrainEffect.CurrentTechnique = terrainEffect.Techniques["RenderRocksSingleLayer" + batch.terrainInBatch[0].GetPerlinNoiseChannel().ToString()];
                    }
#else
                    terrainEffect.CurrentTechnique = terrainEffect.Techniques["RenderRocksSingleLayer" + batch.terrainInBatch[0].GetPerlinNoiseChannel().ToString()];
#endif


                    // for normal mapping light effect:
                    terrainEffect.Parameters["ShadowFactor"].SetValue(DayAndNightEffects.GetOwnShadowFactor());
                    // on the normal maps, z and y are exchanged.                   
                    terrainEffect.Parameters["LightPosition"].SetValue(new Vector3(
                                                                             The.Sim.DateAndTime.SunPosition.X,
                                                                             The.Sim.DateAndTime.SunPosition.Y,
                                                                             The.Sim.DateAndTime.SunPosition.Z
                                                                             ));
                }
                else
                {

                    StringBuilder technique = new StringBuilder("MultiTextured");

                    technique.Append(batch.terrainInBatch[0].GetPerlinNoiseChannel().ToString());

                    if (batch.terrainInBatch.Count > 1)
                    {
                        technique.Append(batch.terrainInBatch[1].GetPerlinNoiseChannel().ToString());
                    }
                    /*   else
                       {
                           technique.Append("0"); // don't care
                       }*/

                    terrainEffect.CurrentTechnique = terrainEffect.Techniques[technique.ToString()];

                    /*"MultiTextured" 
                    + noiseChannel1
                    + noiseChannel2*/
                }

                if (batch.terrainInBatch.Count > 2)
                {
                    terrainEffect.Parameters["terrain3Noise"].SetValue((float)batch.terrainInBatch[2].GetPerlinNoiseChannel());

                }


                //  Vegetation.LowVegetationType vegType;
                SoilComponentType soilType;
                RenderedTerrainType renderedTerrainType;

                int j = 0;
                for (; j < batch.terrainInBatch.Count; j++)
                {
                    renderedTerrainType = batch.terrainInBatch[j];

                    if (renderedTerrainType.TextureName == "greengrass")
                    {

                    }

                    terrainEffect.Parameters[textureParams[j]].SetValue(terrainTextures[renderedTerrainType.TextureName]);

                    soilType = renderedTerrainType as SoilComponentType;
                    if (soilType != null)
                    {
                        if (soilType.RenderAsRocksType != null && soilType.RenderAsRocksType.DepthMapTextureName != null)
                        {
                            terrainEffect.Parameters["NormalMap"].SetValue(terrainTextures[soilType.RenderAsRocksType.DepthMapTextureName]);
                        }
                    }
                }


                foreach (EffectPass pass in terrainEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();

                    terrainVertices = batch.TerrainVerticesArray;
                    terrainIndices = batch.TerrainIndicesArray;
                    The.Client.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, terrainVertices, 0, terrainVertices.Length, terrainIndices, 0, terrainIndices.Length / 3);
                    //  The.Client.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, terrainVertices, 0, terrainVertices.Length, terrainIndices, 0, terrainIndices.Length / 3);

                }

                //passNo++;
            }

        }

        public void DrawGroundOutlineUserVertices(int numberOfQuadsToDraw, bool doubleSpeed = false)
        {

            //The.Client.GraphicsDevice.RasterizerState.CullMode = CullMode.None;
            The.Client.GraphicsDevice.RasterizerState = RasterizerState.CullNone;


            // The.Client.GraphicsDevice.RenderState.DepthBufferEnable = false; // XNA 3
            The.Client.GraphicsDevice.DepthStencilState = DepthStencilState.None;

            //  The.Client.GraphicsDevice.RenderState.AlphaBlendEnable = true; // XNA 3
            // The.Client.GraphicsDevice.RenderState.AlphaTestEnable = false; // XNA 3

            // xna 3 - set in shader!?
            //   The.Client.GraphicsDevice.SamplerStates[0].AddressU = TextureAddressMode.Wrap;
            //    The.Client.GraphicsDevice.SamplerStates[0].AddressV = TextureAddressMode.Wrap;

            float alpha = 0;
            //if (doubleSpeed == true)
            //{
            //    alpha = The.InGameUI.SelectedCyclePlayerQuick.GetCurrentColor(Color.White).A;
            //    alpha /= 255.0f;
            //}
            //else
            //{
            //    alpha = The.InGameUI.SelectedCyclePlayer.GetCurrentColor(Color.White).A;
            //    alpha /= 255.0f;
            //}

            GroundFeatureEffect.Parameters["AlphaAdjustment"].SetValue(1f);

            GroundFeatureEffect.CurrentTechnique = GroundFeatureEffect.Techniques["RenderOutlineGroundSprites"]; //0]; //"RoadsAndPaths"];

            GroundFeatureEffect.Parameters["NormalMap"].SetValue(terrainTextures["linear gradient normal map"]);


            GroundFeatureEffect.Parameters["UseIntegerPositions"].SetValue(!The.MapUI.IsScrolling);
            GroundFeatureEffect.Parameters["spriteSheetTexture"].SetValue(The.Client.FlatSpriteSheet.Texture);


         /*   Viewport viewport = The.Client.GraphicsDevice.Viewport;
            Vector2 viewportSize = new Vector2(viewport.Width, viewport.Height);*/
            Dimension dim = The.Client.Controller.DrawArea;
            Vector2 viewportSize = new Vector2(dim.Width, dim.Height);
            GroundFeatureEffect.Parameters["ViewportSize"].SetValue(viewportSize);
            GroundFeatureEffect.Parameters["WindowPosition"].SetValue(The.MapUI.MapWindowWorldPosition);



            foreach (EffectPass pass in GroundFeatureEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                The.Client.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, groundFeatureVertices, 0, 4 * numberOfQuadsToDraw, groundFeatureIndices, 0, 2 * numberOfQuadsToDraw);

            }

        }



        public void DrawGroundFeatureUserVertices(int numberOfQuadsToDraw)
        {

            //The.Client.GraphicsDevice.RasterizerState.CullMode = CullMode.None;
            The.Client.GraphicsDevice.RasterizerState = RasterizerState.CullNone;


            // The.Client.GraphicsDevice.RenderState.DepthBufferEnable = false; // XNA 3
            The.Client.GraphicsDevice.DepthStencilState = DepthStencilState.None;

            //  The.Client.GraphicsDevice.RenderState.AlphaBlendEnable = true; // XNA 3
            // The.Client.GraphicsDevice.RenderState.AlphaTestEnable = false; // XNA 3
            The.Client.GraphicsDevice.BlendState = BlendState.AlphaBlend;

            // xna 3 - set in shader!?
            //   The.Client.GraphicsDevice.SamplerStates[0].AddressU = TextureAddressMode.Wrap;
            //    The.Client.GraphicsDevice.SamplerStates[0].AddressV = TextureAddressMode.Wrap;

#if DEBUG || PROFILE
            if (Kensei.Dev.Options.GetOption("Rendering.Show light amount"))
            {
                GroundFeatureEffect.CurrentTechnique = GroundFeatureEffect.Techniques["RenderGroundSpritesDebugLighting"];
            }
            else
            {
#endif
                GroundFeatureEffect.CurrentTechnique = GroundFeatureEffect.Techniques["RenderGroundSprites"]; //0]; //"RoadsAndPaths"];
#if DEBUG || PROFILE
            }
#endif
            // for normal mapping light effect:
            GroundFeatureEffect.Parameters["ShadowFactor"].SetValue(DayAndNightEffects.GetOwnShadowFactor());
            // on the normal maps, z and y are exchanged.                   
            GroundFeatureEffect.Parameters["LightPosition"].SetValue(new Vector3(
                                                                     The.Sim.DateAndTime.SunPosition.X,
                                                                     The.Sim.DateAndTime.SunPosition.Y,
                                                                     The.Sim.DateAndTime.SunPosition.Z
                                                                     ));

            GroundFeatureEffect.Parameters["NormalMap"].SetValue(terrainTextures["linear gradient normal map"]);


            GroundFeatureEffect.Parameters["UseIntegerPositions"].SetValue(!The.MapUI.IsScrolling);
            GroundFeatureEffect.Parameters["spriteSheetTexture"].SetValue(The.Client.FlatSpriteSheet.Texture);

            // RoadsAndPathsEffect.Parameters["perlinTexture"].SetValue(perlinTexture);


            /*    Matrix worldMatrix = Matrix.Identity;
                RoadsAndPathsEffect.Parameters["World"].SetValue(worldMatrix);
                RoadsAndPathsEffect.Parameters["View"].SetValue(TerrainViewMatrix);
                RoadsAndPathsEffect.Parameters["Projection"].SetValue(game.Projection); // projectionMatrix);
                */

            Dimension dim = The.Client.Controller.DrawArea;
            Vector2 viewportSize = new Vector2(dim.Width, dim.Height);
            /*
            Viewport viewport = The.Client.GraphicsDevice.Viewport;
            Vector2 viewportSize = new Vector2(viewport.Width, viewport.Height);*/
            GroundFeatureEffect.Parameters["ViewportSize"].SetValue(viewportSize);
            GroundFeatureEffect.Parameters["WindowPosition"].SetValue(The.MapUI.MapWindowWorldPosition);



            foreach (EffectPass pass in GroundFeatureEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                The.Client.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, groundFeatureVertices, 0, 4 * numberOfQuadsToDraw, groundFeatureIndices, 0, 2 * numberOfQuadsToDraw);

            }

        }

        private void SetLightSourceDrawing()
        {
            /*
           Shawn sez: If you have a lot of overlapping lights in your scene, an interesting extension of this technique is to accumulate all the lights separately before combining them with your main scene:

           Create a RenderTarget2D the same size as your screen 
           Draw the regular scene as normal 
           GraphicsDevice.SetRenderTarget(0, lightRenderTarget) 
           GraphicsDevice.Clear(Color.Black) 
           Draw all the light shapes using SpriteBlendMode.Additive 
           GraphicsDevice.ResolveRenderTarget(0) 
           GraphicsDevice.SetRenderTarget(0, null) 
           Using SpriteSortMode.Immediate, set renderstates for multiply blend mode 
           SpriteBatch.Draw(lightRenderTarget.GetTexture()), covering the entire screen
           */


            /*
            The.Client.GraphicsDevice.DepthStencilBuffer = game.ScreenManager.NoMultiSamplingStencilBuffer;
            // occluded pixel - render ambient light:

            The.Client.GraphicsDevice.RenderState.AlphaBlendEnable = true;

            // additive blending. light sources will be rendered with alpha = 0.
            The.Client.GraphicsDevice.RenderState.BlendFunction = BlendFunction.Add;
            The.Client.GraphicsDevice.RenderState.SourceBlend = Blend.One;
            The.Client.GraphicsDevice.RenderState.DestinationBlend = Blend.One;
            */
        }

        /*
         * OLD:
         * private void SetLightSourceDrawing()
           {           

               game.graphics.GraphicsDevice.SetRenderTarget(0, LightRenderTarget);
               game.graphics.GraphicsDevice.DepthStencilBuffer = UWGame.SimSide.Instance.NoMultiSamplingStencilBuffer;
               // occluded pixel - render ambient light:
                    
                
            //   Color clearColor = new Color(0f, 0f, 0f, 1f); 
            //   game.graphics.GraphicsDevice.Clear(new Color(TimeOfDayLightingFactor)); // clearColor);
               game.graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;

               // additive blending. light sources will be rendered with alpha = 0.
               game.graphics.GraphicsDevice.RenderState.BlendFunction = BlendFunction.Add;
               game.graphics.GraphicsDevice.RenderState.SourceBlend = Blend.One;
               game.graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.One;

               game.graphics.GraphicsDevice.RenderState.SeparateAlphaBlendEnabled = true;
               game.graphics.GraphicsDevice.RenderState.AlphaBlendOperation = BlendFunction.Add;
               game.graphics.GraphicsDevice.RenderState.AlphaSourceBlend = Blend.One;
               game.graphics.GraphicsDevice.RenderState.AlphaDestinationBlend = Blend.Zero; // overwrite the alpha value!
           }*/



        /*  private bool IsPhysical(GameObject drawObject)
          {            
              Entity entity = drawObject as Entity;
              if (entity != null && entity.IsNotStarted())
              {
                  return false;
              }

              return true;
          }*/

        public static void InsertionSort<T>(IList<T> list) where T : ILocatable // GameObject //, Comparison<T> comparison)
        {
            /*  if (list == null)
                 throw new ArgumentNullException("list");
            if (comparison == null)
                 throw new ArgumentNullException("comparison");
 */
            int count = list.Count;
            for (int j = 1; j < count; j++)
            {
                T key = list[j];

                int i = j - 1;
                for (; i >= 0 && list[i].CompareTo(key) > 0; i--)
                //   for (; i >= 0 && (int)(list[i].Location.Y - key.Location.Y) > 0; i--)
                //   for (; i >= 0 && (int)(key.Location.Y - list[i].Location.Y) > 0; i--)
                {
                    list[i + 1] = list[i];
                }
                list[i + 1] = key;
            }

            /*  for (int j = 1; j < count; j++)
              {
                  T key = list[j];

                  int i = j - 1;
                  for (; i >= 0 && comparison(list[i], key) > 0; i--)
                  {
                      list[i + 1] = list[i];
                  }
                  list[i + 1] = key;
              }*/
        }

        public static void SaveRenderTargetToFile(string name, RenderTarget2D renderTarget)
        {
            using (Stream stream = File.Create(name + ".png")) // "EdgeDetectSceneRenderTarget.png"))
            {
                renderTarget.SaveAsPng(stream, renderTarget.Width, renderTarget.Height);
            }
        }

        public static void SaveTextureToFile(string name, Texture2D texture2D)
        {
            using (Stream stream = File.Create(name + ".png")) // "EdgeDetectSceneRenderTarget.png"))
            {
                texture2D.SaveAsPng(stream, texture2D.Width, texture2D.Height);
            }
        }



        public enum RenderTechnique { Standard, StandardMonochrome, StandardOverlay, NoLighting, NormalsAndDepth, LightSources, DepthHeightBillboardAlpha, DrawModelEmitters, Outline }

        /// <summary>
        /// 
        /// </summary>
        private void DrawSortedObjectsAndParticles()
        {
           /* if (The.Client.spriteBatch == null)
                The.Client.InitializeSpriteBatch();*/
            
            int featureQuadIndex = 0;
            overlayQuadIndex = 0;
            overlayModelEntities.Clear();
            
            GraphicsDevice device = The.Client.GraphicsDevice;

            // set the states for model rendering:
            //The.Client.GraphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;
            device.RasterizerState = RasterizerState.CullCounterClockwise;


            device.DepthStencilState = DepthStencilState.None; // disable depth buffering
            
            device.BlendState = BlendState.AlphaBlend; 
            //device.BlendState = BlendState.NonPremultiplied;


            // If we are doing edge detection, first off we need to render the
            // normals and depth of our model into a special rendertarget.  

            // WHEN DEBUGGING - WAIT FOR A FEW SECS BEFORE SAVING THE RESULTING TEXTURE! I think the fade in is the problem.

            device.SetRenderTarget(edgeDetectNormalDepthRenderTarget);
            device.Clear(Color.Black);

            featureQuadIndex = DrawNormalDepthMapForEdgeEnhancement(featureQuadIndex);

            // got the special depth map drawn. Now reset to draw as standard:


            device.SetRenderTarget(DistanceHeightAndBillboardAlphaRenderTarget);

            // SaveRenderTargetToFile("EdgeDetectNormalDepthRenderTarget", EdgeDetectNormalDepthRenderTarget);

            //NEW

            featureQuadIndex = DrawDepthMapForLighting(featureQuadIndex, device);

            // NOW we can do our main render pass!!!    
            device.BlendState = BlendState.AlphaBlend;

          //  SaveRenderTargetToFile("EdgeDetectSceneRenderTarget", EdgeDetectSceneRenderTarget); // Here, texture contains terrain and ground sprites. With MS, the texture is white.

            device.SetRenderTarget(DiffuseMSRenderTarget); //draw model polygons into our MS target to get antialiasing

            // draw models with depth test.
            // what about billboards?
            // what happens when they overlap?
            // set depth (z) on FeatureQuads...
            //device.DepthStencilState = DepthStencilState.None;
            
            featureQuadIndex = DrawSortedObjectsMain(featureQuadIndex);


            DrawInvisibleEntitiesForDebugOrEditor();
            
            DrawMapResourceOverlays();

            // draw particles here... does this also work when drawing additively?
            // The.Client.ParticleManager.Draw(The.Sim.GameTime);
            //UWGame.SimSide.Instance.particleManager.Draw(UWGame.SimSide.Instance.GameTime);


            // after resolving, don't use the MS rendertarget anymore
            // DiffuseMSRenderTarget.ResolveSubresource(diffuseRenderTarget); // #MONOUPDATE

            // SaveRenderTargetToFile("EdgeDetectSceneRenderTargetResolve", EdgeDetectSceneRenderTargetResolve);

            // device.SetRenderTarget(DiffuseMSRenderTarget); // sample this when drawing lights!

            //the result is in diffuseFinalRenderTarget
            device.SetRenderTarget(diffuseRenderTarget); // sample this when drawing lights!
           // device.SetRenderTarget(null); 

            //  SaveRenderTargetToFile("EdgeDetectSceneRenderTarget", EdgeDetectSceneRenderTarget); // Here, texture contains models and billboards also

            // uses EdgeDetectNormalDepthRenderTarget as parameter
            bool drawOutlines = true;
#if DEBUG || PROFILE
            drawOutlines = Kensei.Dev.Options.GetOption("Rendering.Draw outlines");
#endif
            if (drawOutlines)
            {
                DrawOutlines();
            }
            else
            {
                // don't apply the edge enhancement effect:
                The.Client.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
                // The.Client.spriteBatch.Draw(diffuseRenderTarget /*EdgeDetectSceneRenderTarget*/, Vector2.Zero, Color.White);
                The.Client.spriteBatch.Draw(DiffuseMSRenderTarget, Vector2.Zero, Color.White);

                The.Client.spriteBatch.End();
            }


            DrawLightsFromModelEmitters(); // draws into special RTs


            // the RT lights will be drawn into this
            device.SetRenderTarget(diffuseFinalRenderTarget);

           


            //device.SetRenderTarget(diffuseRenderTarget); // ??? clear and reuse this render target??? will be used as bloom parameter. Current output is in diffuseFinalRT
           // device.Clear(Color.Black);

            
            // SaveRenderTargetToFile("emitter", EmissiveModelLightRenderTarget);
            //    SaveRenderTargetToFile("emitterDistance", EmissiveModelLightDistanceRenderTarget);

            //  SaveRenderTargetToFile("diffuseSceneRenderTarget" , diffuseSceneRenderTarget);

            // uses diffuseSceneRenderTarget as parameter         
            DrawTimeOfDayOverlay();


            // start drawing light sources additively:

            // uses diffuseSceneRenderTarget and DistanceHeightAndBillboardAlphaRenderTarget as parameters
            GetLightsToDrawAndDrawThem();
            
        }

        private void DrawTopAndBottomEdges(Rectangle rect, Rectangle sourcerect)
        {

            ////Can this be done in a smoother way? Currently casting a float to int to get the minimum
            ////ammount of images needed extra to cover the corrners and not breaking the tiling
            int halfResWidth = DiffuseMSRenderTarget.Width / 2;
            float extraSize = ((DiffuseMSRenderTarget.Width) / rect.Width) + 1;
            int extraSizeInt = (int)extraSize;
            ////

            //Loop trough the number of tiles to stack on height, minimum 1 tile
            for (int j = 0; j < Common.Max(1, ((DiffuseMSRenderTarget.Height) / rect.Height)); j++)
            {//Loop trough the width and add extra tiles to the right and left that wont break the tiling but will fill in the corners
                for (int i = 0; i < ((The.Map.MapWorldWidth) / rect.Width) + extraSizeInt; i++)
                {

                    //Top border far left to far right
                    //

                    Vector2 pos = new Vector2(rect.Width * i - (extraSizeInt * rect.Width) / 2, -rect.Height * (1 + j));
                    Point screenPos = The.MapUI.WorldPosToScreenPoint(new Vector2(pos.X, pos.Y));
                    rect.X = screenPos.X - 1;
                    rect.Y = screenPos.Y - 1;
                    The.Client.spriteBatch.Draw(The.Client.FlatSpriteSheet.Texture, rect, sourcerect, Color.White);
                    //

                    //Bottom border far left to far right
                    //
                    pos.Y = rect.Height * j + (int)The.Map.MapWorldWidth;
                    screenPos = The.MapUI.WorldPosToScreenPoint(new Vector2(pos.X, pos.Y));
                    rect.X = screenPos.X - 1;
                    rect.Y = screenPos.Y - 1;
                    The.Client.spriteBatch.Draw(The.Client.FlatSpriteSheet.Texture, rect, sourcerect, Color.White);



                    //

                }
            }
        }

        private void DrawLeftAndRightEdges(Rectangle rect, Rectangle sourcerect)
        {
            //Loop trough the width needed with a minimum of 1 sprite X
            for (int j = 0; j < Common.Max(1, ((DiffuseMSRenderTarget.Width) / rect.Width)); j++)
            {//loop trough the ammount of tiles needed on the height part Y 
                for (int i = 0; i < (The.Map.MapWorldHeight) / rect.Height; i++)
                {

                    //Top To bottom, corrner top to corrner bottom. Left side
                    //
                    Vector2 pos = new Vector2(-rect.Width * (1 + j), rect.Height * i);
                    Point screenPos = The.MapUI.WorldPosToScreenPoint(new Vector2(pos.X, pos.Y));
                    rect.X = screenPos.X - 1;
                    rect.Y = screenPos.Y - 1;
                    The.Client.spriteBatch.Draw(The.Client.FlatSpriteSheet.Texture, rect, sourcerect, Color.White);
                    //

                    //Top To bottom, corrner top to corrner bottom. Right side
                    //
                    pos.X = rect.Width * j + (int)The.Map.MapWorldWidth;
                    screenPos = The.MapUI.WorldPosToScreenPoint(new Vector2(pos.X, pos.Y));
                    rect.X = screenPos.X - 1;
                    rect.Y = screenPos.Y - 1;
                    The.Client.spriteBatch.Draw(The.Client.FlatSpriteSheet.Texture, rect, sourcerect, Color.White);
                    //
                }
            }
        }




        private void DrawMapResourceOverlays()
        {
            if (The.Sim.Mode == Sim.EngineMode.Edit // the overlay panel is not shown in the editor.
                 || The.InGameUI.OverlaySettings.ShowOverlaysOnGameArea == false) // renderResourceOutlineState == RenderResourceOutlinesState.None)
            {
                return;
            }

            mapResourceRenderer.Render(The.Map, this);
        }


        private void DrawMapEdges()
        {

            if (The.Client.spriteBatch != null)
            {


                //TODO: See if this rect can be created outside of loop and not every frame
                Rectangle rect = new Rectangle();
                Rectangle sourcerect = The.Client.FlatSpriteSheet.GetSourceRectangle("mapedge_base");
                rect.Height = sourcerect.Height;
                rect.Width = sourcerect.Width;

                //Draw all edges 
                The.Client.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
                DrawTopAndBottomEdges(rect, sourcerect);
                DrawLeftAndRightEdges(rect, sourcerect);
                The.Client.spriteBatch.End();




            }

        }



        private int DrawSortedObjectsMain(int featureQuadIndex, RenderTechnique renderTechnique = RenderTechnique.Standard)
        {

            //NEW
            //   device.SetRenderTarget(1, DistanceHeightAndBillboardAlphaRenderTarget);
            // NEW END


            //    Texture2D normalDepthTexture = EdgeDetectNormalDepthRenderTarget.GetTexture();
            //    normalDepthTexture.Save("normalDepthTexture.jpg", ImageFileFormat.Jpg);

            Entity objectAsEntity;
            Renderable renderable;

            bool renderIds = false;

            if (sim.Mode == SimSide.Sim.EngineMode.Edit)
            {
                renderIds = The.InGameUI.OverlaySettings.EditorOverlayTypeSettings[Interface.Overlays.EditorOverlayTypes.EntityIDs] == true;  //The.InGameUI.SidePanelEditorPlace.RenderIds;

            }

            bool drawInfo = false;
            bool drawMarkers = false;

#if DEBUG || PROFILE

            drawInfo = Kensei.Dev.Options.GetOption("Dev.Show hitpoints");
            drawMarkers = Kensei.Dev.Options.GetOption("Overlays.Markers");

#endif

            foreach (var sortedList in sortedObjectsToDraw)
            {
                foreach (var drawObject in sortedList)
                {

#if DEBUG || PROFILE
                    // TODO: also draw invisible enitites (fog emitter)
                    if (drawMarkers)
                    {
                        The.MapUI.AddDebugMarker(drawObject.Location, Color.White, drawObject);
                    }
#endif

                    objectAsEntity = null;

                    if (drawObject is RenderAsBillboard)
                    {
                        RenderAsBillboard contained = (RenderAsBillboard)drawObject;

                        objectAsEntity = contained.Parent.Entity;

                        if (contained.Parent != null && contained.Parent.DrawAsOverlay) // contained.Parent.DrawAsNonPhysical)
                        {
                            // draw structures and features being placed as overlays after all is done.
                            contained.CopyOverlayQuadToVertexBuffer(overlayVertices, ref overlayQuadIndex);

                        }
                        else
                        {
                            contained.CopyQuadToVertexBuffer(featureVertices, ref featureQuadIndex);

                        }

                    }
                    else
                    {
                        renderable = drawObject.AsRenderable;
                        if (renderable != null)
                        {
                            objectAsEntity = renderable.Entity;

                            if (renderable.DrawAsOverlay)
                            {
                                // draw this later...                        
                            }
                            else
                            {
                                if (featureQuadIndex > 0)
                                {
                                    DrawBillboardBatch(featureQuadIndex, RenderTechnique.Standard);
                                    featureQuadIndex = 0;
                                }

                                //draw memory facts with special shading/color...
                                renderable.Draw(RenderTechnique.Standard,
                                    ref View, ref The.Client.Projection);

                                /* if (objectAsEntity != null) //drawObject is Entity)
                                 {
                                     objectAsEntity.Draw(RenderTechnique.Standard, ref View, ref The.Client.Projection);
                                 }
                                 else if (drawObject is MemoryFact &&  ((MemoryFact)drawObject).Renderable != null) //.PointsTo != null) // 'remembered' entity
                                 {
                                     (((MemoryFact)drawObject).Renderable).Draw(RenderTechnique.StandardMonochrome, ref View, ref The.Client.Projection);
                                     //((Entity)(((MemoryFact)drawObject).PointsTo)).Draw(RenderTechnique.StandardMonochrome, ref View, ref The.Client.Projection);
                                 }*/

                            }
                        }
                    }

                    if (objectAsEntity != null)
                    {
                        if (sim.Mode == SimSide.Sim.EngineMode.Edit)
                        {
                            // TODO: also draw invisible enitites (fog emitter)
                            PrintEditorData(objectAsEntity, renderIds); //, The.InGameUI.SidePanelEdit.PrintResources);
                        }

#if DEBUG || PROFILE
                        // TODO: also draw invisible enitites (fog emitter)
                        DrawEntityDebugText(objectAsEntity, drawInfo);

                        if (drawMarkers)
                        {
                            DrawAccessPointMarkers(objectAsEntity);

                            DrawAgentMarker(objectAsEntity);

                            
                            if (objectAsEntity.EntityType.ContainerType != null
                               && objectAsEntity.Contains is IExit) // .EntityType.ContainerType is HomeContainerType)
                            {
                                //get  doors, rally points etc:
                                ((IExit)objectAsEntity.Contains).GetDebugMarkers();
                            }
                           /* if (objectAsEntity.EntityType.ContainerType != null
                                && objectAsEntity.EntityType.ContainerType is HomeContainerType)
                            {
                                //get  doors, rally points etc:
                                ((HomeContainer)objectAsEntity.Contains).GetDebugMarkers();
                            }*/

                           
                        }
#endif
                    }

                }
            }

            if (featureQuadIndex > 0)
            {   // draw the rest of the quad batch
                DrawBillboardBatch(featureQuadIndex, renderTechnique);
                featureQuadIndex = 0;
            }

            return featureQuadIndex;
        }

        private static void DrawAccessPointMarkers(Entity objectAsEntity)
        {
            //  Vector3 center;
            if (objectAsEntity.Contains != null)
            {
                // the access point(s) assigned by an IExit always take precedence over "natural" access points
                IExit exit = objectAsEntity.Contains as IExit;
                if (exit != null)
                {
                    //  Kensei.Dev.Shape.Circle(center, 2, Color.Azure);

                    The.MapUI.AddDebugMarker(exit.ComputeAccessPoint(), Color.Azure, objectAsEntity, 3);

                    return;
                }
            }


            The.MapUI.AddDebugMarker(objectAsEntity.AccessPoint.Value, Color.LightYellow, objectAsEntity, 3);

            //            Kensei.Dev.Shape.Circle(center, 2, Color.Yellow, 3);
        }

        private static void DrawAgentMarker(Entity objectAsEntity)
        {
            if (objectAsEntity.EntityType.IntelligenceType != null)
            {
                The.MapUI.AddDebugMarker(objectAsEntity.PlaySiteLocation, Color.Orange, objectAsEntity, 5);
            }

            //            Kensei.Dev.Shape.Circle(center, 2, Color.Yellow, 3);
        }

        /// <summary>
        /// lags behind when scrolling!
        /// tree resources do not!
        /// why???
        /// </summary>
        /// <param name="printCoords"></param>
        private void PrintEditorTileInfo(bool printCoords) //, bool printResources)
        {
            TerrainTile tileToDraw;
            TerrainTile[] tileColumn;
            Vector2 screenPosition;
            for (int x = TileStartX; x <= TileEndX; x++)
            {
                tileColumn = The.Map.TileMap[x];
                for (int y = TileStartY; y <= TileEndY; y++)
                {

                   /* if (printResources)
                    {*/
                    tileToDraw = tileColumn[y];

                    bool resourcesWerePrinted = false;
                    if (tileToDraw.DesignerPlacedResources != null)
                    {

                        screenPosition = The.MapUI.TileEdgeToScreen(tileToDraw.X, tileToDraw.Y);

                        foreach (var resource in tileToDraw.DesignerPlacedResources)
                        {
                            bool werePrinted;
                            screenPosition = PrintEditorResource(screenPosition, resource, out werePrinted); // lags behind...

                            if (werePrinted)
                            {
                                resourcesWerePrinted = werePrinted;
                            }
                        }
                    }

                    if (!resourcesWerePrinted && printCoords)
                    {
                        PrintCoords(x, y);
                    }

                   /* }
                    else if (printCoords)
                    {
                        PrintCoords(x, y);

                    }*/
                }
            }

        }

        private void PrintEditorData(Entity objectAsEntity, bool renderIds) //, bool renderResources)
        {
            if (objectAsEntity != null)
            {
                Vector2 screenPosition = The.MapUI.WorldPosToScreen(objectAsEntity.PlaySiteLocation);
                Vector2 printPos = screenPosition;
                printPos.X -= 26f;

                if (renderIds)
                {
                    Kensei.Dev.DevText.Print(printPos, objectAsEntity.EntityID.ToString(), Color.White);
                }

                EditorData editorData;
                if (objectAsEntity.Find(out editorData))
                {
                  
                    if (editorData.Resources != null) // && renderResources)
                    {
                        printPos.Y -= 40f;
                        foreach (var resource in editorData.Resources)
                        {
                            bool wasPrinted;
                            printPos = PrintEditorResource(printPos, resource, out wasPrinted);
                                                       
                        }
                    }
                }
            }
        }

        private static void PrintCoords(int tileX, int tileY)
        {
            // print the tile pos in the center. print world coords in the corners.
            Vector2 upperLeftScreen = The.MapUI.TileEdgeToScreen(tileX, tileY);

            Vector2 center = upperLeftScreen;
            center.X += 8;
            center.Y += 20;
            Kensei.Dev.DevText.Print(center, tileX + "," + tileY, Color.White);

            Vector3 upperLeftWorld = MapManager.TileEdgeToWorldPos(new Point(tileX, tileY));
            Kensei.Dev.DevText.Print(upperLeftScreen, upperLeftWorld.X.ToString(), Color.Yellow); // print world coords on two lines
            Kensei.Dev.DevText.Print(upperLeftScreen + new Vector2(0f, 12f), upperLeftWorld.Y.ToString(), Color.Yellow);


        }

        private static Vector2 PrintEditorResource(Vector2 printPos, Resource resource, out bool wasPrinted)
        {
            wasPrinted = false;

            if (!The.InGameUI.OverlaySettings.DisplayResourceType(resource.ResourceType))
            {
                return printPos;
            }

            ResourceType resourceType = resource.ResourceType; // GameData.Instance.AllResourceTypes[resource.KeyName];
            Color color = resourceType.Color ?? resourceType.Category.Color ?? Color.White;

            // Kensei.Dev.DevText.Print(printPos, resource.KeyName, color);
            // printPos.Y += 12f;

            StringBuilder stringToPrint = new StringBuilder();
            string delimiter = "";
            if (resource.MinResourceItems.HasValue)
            {
                stringToPrint.Append(resource.MinResourceItems.Value.ToString() + "-" + resource.MaxResourceItems.Value.ToString());
                //Kensei.Dev.DevText.Print(printPos, "Abs.: " + resource.Min.Value.ToString() + " - " + resource.Max.Value.ToString(), color);

                delimiter = "|";
                //printPos.Y += 12f;
            }
            
            if (resource.Modifier.HasValue && resource.Modifier.Value != 100) // don't print unity modifier
            {
                stringToPrint.Append(delimiter);
                stringToPrint.Append(resource.Modifier.Value.ToString() + " %"); 
            }

            string printString = stringToPrint.ToString();
            if (!string.IsNullOrEmpty(printString))
            {
                Kensei.Dev.DevText.Print(printPos, printString, color);

                wasPrinted = true;
                printPos.Y += 12f;
            }

            return printPos;
        }

        private void DrawEntityDebugText(Entity objectAsEntity, bool drawInfo)
        {
            if (drawInfo)
            {
                if (objectAsEntity != null)
                {
                    BodyComponent body;
                    if (objectAsEntity.Find(out body))
                    {
                        Vector2 pos = The.MapUI.WorldPosToScreen(objectAsEntity.PlaySiteLocation);
                        pos.Y -= 32f;
                        pos.X -= 14f;
                        Kensei.Dev.DevText.Print(pos, ((int)(body.Body.GlobalHitpoints)).ToString(), Color.LightGreen);
                    }

                    Intelligence intelligence;
                    if (objectAsEntity.Find(out intelligence))
                    {
                        Vector2 pos = The.MapUI.WorldPosToScreen(objectAsEntity.PlaySiteLocation);
                        pos.Y -= 22f;
                        pos.X -= 14f;
                        Kensei.Dev.DevText.Print(pos, ((int)(100f * intelligence.Morale)).ToString(), Color.LightBlue);
                    }

                    NonLivingEntity nonLiving;
                    if (objectAsEntity.Find(out nonLiving))
                    {
                        Vector2 pos = The.MapUI.WorldPosToScreen(objectAsEntity.PlaySiteLocation);
                        pos.Y -= 32f;
                        pos.X -= 14f;
                        Kensei.Dev.DevText.Print(pos, ((int)(100f * nonLiving.Condition)).ToString(), Color.LightCyan);
                    }
                }
            }
        }

        private int DrawDepthMapForLighting(int featureQuadIndex, GraphicsDevice device)
        {
            // Draw depth maps in a separate pass - MRT doesn't work with multisampling in DirectX 9. 
            // We also render their distance to the viewer at the same time in a second render target (DepthRenderTarget) - MRT.
            // But first we clear the Depth render target with a high value (1) for DistanceFromViewer. This will be the ground value. 
            // Height is unused for now...
            // Billboard Alpha is Blue and is used when drawing cloud shadows.
            // When we are clearing the target, only the first three components, Red, Green and Blue matter (Alpha is Don't Care):
            Color colorToClearWith = new Color(1f, 0f, 0f, 0f);
            device.Clear(colorToClearWith);

            device.BlendState = BlendState.NonPremultiplied; // xna 4

            foreach (var sortedList in sortedObjectsToDraw)
            {
                foreach (var drawObject in sortedList)
                {

                    /*  if (drawObject is MemoryFact && ((MemoryFact)drawObject).MemoryItemRenderData != null) // 'remembered' item
                      {
                          ((MemoryFact)drawObject).MemoryItemRenderData.CopyQuadToVertexBuffer(featureVertices, ref featureQuadIndex);
                      }
                      else*/
                    if (drawObject is RenderAsBillboard)
                    {
                        RenderAsBillboard contained = (RenderAsBillboard)drawObject;
                        if (contained.Parent != null && !contained.Parent.DrawAsNonPhysical)
                        {
                            contained.CopyQuadToVertexBuffer(featureVertices, ref featureQuadIndex);
                        }
                    }
                    else
                    {
                        Renderable renderable = drawObject.AsRenderable;

                        if (renderable.DrawAsNonPhysical) // !IsPhysical(drawObject))
                        {
                            // draw this later...
                            //overlayModelEntities.Add((Entity)drawObject);
                        }
                        else
                        {
                            if (featureQuadIndex > 0)
                            {
                                DrawBillboardBatch(featureQuadIndex, RenderTechnique.DepthHeightBillboardAlpha);
                                featureQuadIndex = 0;
                            }


                            if (renderable != null)
                            {
                                renderable.Draw(RenderTechnique.DepthHeightBillboardAlpha, ref View, ref The.Client.Projection);
                            }

                            /*  if (drawObject is Entity)
                              {
                                  ((Entity)drawObject).Draw(RenderTechnique.DepthHeightBillboardAlpha, ref View, ref The.Client.Projection);
                              }
                              else if (drawObject is MemoryFact && ((MemoryFact)drawObject).PointsTo != null) //.MemoryModelRenderData != null) // 'remembered' entity
                              {
                                  ((Entity)(((MemoryFact)drawObject).PointsTo)).Draw(RenderTechnique.DepthHeightBillboardAlpha, ref View, ref The.Client.Projection);
                              }*/
                        }
                    }
                }
            }

            if (featureQuadIndex > 0)
            {   // draw the rest of the quad batch
                DrawBillboardBatch(featureQuadIndex, RenderTechnique.DepthHeightBillboardAlpha);
                featureQuadIndex = 0;
            }

            return featureQuadIndex;
        }

        private int DrawNormalDepthMapForEdgeEnhancement(int featureQuadIndex)
        {
            /*  using (Stream stream = File.Create("EdgeDetectSceneRenderTarget.png"))
              {
                  EdgeDetectSceneRenderTarget.SaveAsPng(stream, EdgeDetectSceneRenderTarget.Width, EdgeDetectSceneRenderTarget.Height);
              }*/

            Renderable renderable;
            //  RenderAsBillboard renderAsBillboard;

            foreach (var sortedList in sortedObjectsToDraw)
            {
                //sortedList.Sort(); // "unstable sort" (quicksort, O(n log(n)) - causes flickering billboards because left-to-right order is not preserved when y is the same value!

                //this method call seems to have a high perf cost in the profiler... and commenting it out makes little difference visually as well as perf. wise...
                InsertionSort(sortedList); // "stable sort" O(n^2) - no flicker!

                foreach (var drawObject in sortedList)
                {
                    if (!(drawObject is LightSource)) // lights don't affect edges
                    {

                        if (drawObject.AsRenderAsBillboard != null)
                        {

                            if (drawObject.AsRenderAsBillboard.Parent != null)
                            {
                                if (!drawObject.AsRenderAsBillboard.Parent.DrawAsNonPhysical) // IsPhysical(contained.Parent))
                                {
                                    drawObject.AsRenderAsBillboard.CopyQuadToVertexBuffer(featureVertices, ref featureQuadIndex);
                                }
                            }
                        }
                        else
                        {
                            renderable = drawObject.AsRenderable; // GetRenderable(drawObject);
                            if (renderable != null)
                            {
                                if (renderable.DrawAsNonPhysical) // !IsPhysical(drawObject))
                                {
                                    // draw this later...
                                    overlayModelEntities.Add(renderable);
                                }
                                else
                                {   // other entity
                                    if (featureQuadIndex > 0)
                                    {
                                        DrawBillboardBatch(featureQuadIndex, RenderTechnique.NormalsAndDepth);
                                        featureQuadIndex = 0;
                                    }


                                    renderable.Draw(RenderTechnique.NormalsAndDepth, ref View, ref The.Client.Projection);


                                    /*
                                    if (drawObject is Entity)
                                    {
                                        ((Entity)drawObject).Draw(RenderTechnique.NormalsAndDepth, ref View, ref The.Client.Projection);

                                    }
                                    else
                                    {
                                        MemoryFact mf = drawObject as MemoryFact;
                                        if (mf != null && mf.PointsTo != null && mf.MemoryModelRenderData != null) // 'remembered' entity
                                        {
                                            mf.PointsTo.Draw(RenderTechnique.NormalsAndDepth, ref View, ref The.Client.Projection);
                                        }
                                    }*/

                                }
                            }
                        }
                    }
                }
                //  sortedList.Clear();
            }
            if (featureQuadIndex > 0)
            {   // draw the rest of the quad batch
                DrawBillboardBatch(featureQuadIndex, RenderTechnique.NormalsAndDepth);
                featureQuadIndex = 0;
            }
            return featureQuadIndex;
        }



        private void DrawOverlayBillboards()
        {
            if (overlayQuadIndex == 0)
                return;

            // RenderState renderState = SetOverlayRenderState();
            // SetOverlayRenderState();
            The.Client.GraphicsDevice.BlendState = overlayBlendState;

            overlayEffect.CurrentTechnique = overlayEffect.Techniques["DrawOverlay"];

            Dimension dim = The.Client.Controller.DrawArea;
            Vector2 viewportSize = new Vector2(dim.Width, dim.Height);
            /*
            Viewport viewport = The.Client.GraphicsDevice.Viewport;
            Vector2 viewportSize = new Vector2(viewport.Width, viewport.Height);*/
            overlayEffect.Parameters["ViewportSize"].SetValue(viewportSize);
            overlayEffect.Parameters["WindowPosition"].SetValue(The.MapUI.MapWindowWorldPosition);

            overlayEffect.Parameters["ScanlinesTexture"].SetValue(Scanlines);
           // overlayEffect.Parameters["OverlayGradient"].SetValue(OverlayGradient);

            overlayEffect.Parameters["OverlayTexture"].SetValue(GhostedStructuresSpriteSheet.Texture);

            Texture2D depthmap = DistanceHeightAndBillboardAlphaRenderTarget;
            // depthmap.Save("depthmap.png", ImageFileFormat.Png);

            overlayEffect.Parameters["DistanceHeightAndBillboardAlpha"].SetValue(depthmap);

            /*  Texture2D diffuseScene = diffuseSceneRenderTarget.GetTexture();
              overlayEffect.Parameters["DiffuseSceneTexture"].SetValue(diffuseScene);
              */

            //  UWGame.SimSide.Instance.GraphicsDevice.VertexDeclaration = overlayQuadVertexDeclaration;

            //  overlayEffect.Begin();
            foreach (EffectPass pass in overlayEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                The.Client.GraphicsDevice.DrawUserIndexedPrimitives( // OK TO REUSE INDICES
                    PrimitiveType.TriangleList, overlayVertices, 0, overlayQuadIndex * 4, featureIndices, 0, overlayQuadIndex * 2);

            }


            //  ResetOverlayRenderState(); // XNA 3
            The.Client.GraphicsDevice.BlendState = BlendState.AlphaBlend;
        }

        /*  private static void ResetOverlayRenderState() 
          {
               // XNA 3
              renderState.SourceBlend = Blend.SourceAlpha;
              renderState.DestinationBlend = Blend.InverseSourceAlpha;
             

          }*/

        /*   private void SetOverlayRenderState()
          {           

               // XNA 3
             RenderState renderState = UWGame.SimSide.Instance.GraphicsDevice.RenderState;
              renderState.SourceBlend = Blend.SourceAlpha;
              renderState.DestinationBlend = Blend.One;
              return renderState;
                   
           
          }*/

        private void DrawOverlayGroundSprites()
        {
            SetupInterfaceOnMapQuads();

            if (overlayGroundSpriteQuadIndex == 0)
                return;

            //  SetOverlayRenderState();
            The.Client.GraphicsDevice.BlendState = overlayBlendState;

            overlayGroundSpritesEffect.CurrentTechnique = overlayGroundSpritesEffect.Techniques["DrawOverlayGroundSprite"];

            Dimension dim = The.Client.Controller.DrawArea;
            Vector2 viewportSize = new Vector2(dim.Width, dim.Height);
            /*
            Viewport viewport = The.Client.GraphicsDevice.Viewport;
            Vector2 viewportSize = new Vector2(viewport.Width, viewport.Height);*/
            overlayGroundSpritesEffect.Parameters["ViewportSize"].SetValue(viewportSize);
            overlayGroundSpritesEffect.Parameters["WindowPosition"].SetValue(The.MapUI.MapWindowWorldPosition);

            overlayGroundSpritesEffect.Parameters["ScanlinesTexture"].SetValue(Scanlines);

            overlayGroundSpritesEffect.Parameters["OverlayTexture"].SetValue(The.Client.FlatSpriteSheet.Texture);

            // depthmap.Save("depthmap.png", ImageFileFormat.Png);

            overlayGroundSpritesEffect.Parameters["DistanceHeightAndBillboardAlpha"].SetValue(DistanceHeightAndBillboardAlphaRenderTarget);


            //UWGame.SimSide.Instance.GraphicsDevice.VertexDeclaration = overlayGroundSpriteQuadVertexDeclaration;

            // overlayGroundSpritesEffect.Begin();
            foreach (EffectPass pass in overlayGroundSpritesEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                The.Client.GraphicsDevice.DrawUserIndexedPrimitives( // OK TO REUSE INDICES.
                    PrimitiveType.TriangleList, overlayGroundSpriteVertices, 0, overlayGroundSpriteQuadIndex * 4, featureIndices, 0, overlayGroundSpriteQuadIndex * 2);

            }

            // ResetOverlayRenderState(); // XNA 3
            The.Client.GraphicsDevice.BlendState = BlendState.AlphaBlend;

        }


        private void DrawInfluenceMapSprites()
        {
            SetupInfluenceQuads();

            if (influenceMapQuadIndex == 0)
                return;

            The.Client.GraphicsDevice.BlendState = overlayBlendState;

            overlayGroundSpritesEffect.CurrentTechnique = overlayGroundSpritesEffect.Techniques["DrawInfluenceOverlay"];

            Dimension dim = The.Client.Controller.DrawArea;
            Vector2 viewportSize = new Vector2(dim.Width, dim.Height);
            /*
            Viewport viewport = The.Client.GraphicsDevice.Viewport;
            Vector2 viewportSize = new Vector2(viewport.Width, viewport.Height);*/
            overlayGroundSpritesEffect.Parameters["ViewportSize"].SetValue(viewportSize);
            overlayGroundSpritesEffect.Parameters["WindowPosition"].SetValue(The.MapUI.MapWindowWorldPosition);
            overlayGroundSpritesEffect.Parameters["ScanlinesTexture"].SetValue(Scanlines);
            overlayGroundSpritesEffect.Parameters["OverlayTexture"].SetValue(The.Client.FlatSpriteSheet.Texture);

            // depthmap.Save("depthmap.png", ImageFileFormat.Png);

          //  overlayGroundSpritesEffect.Parameters["DistanceHeightAndBillboardAlpha"].SetValue(DistanceHeightAndBillboardAlphaRenderTarget);


            // overlayGroundSpritesEffect.Begin();
            foreach (EffectPass pass in overlayGroundSpritesEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                The.Client.GraphicsDevice.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList, influenceMapVertices, 0, influenceMapQuadIndex * 4, featureIndices, 0, influenceMapQuadIndex * 2);

            }

            The.Client.GraphicsDevice.BlendState = BlendState.AlphaBlend;
        }

        private void GetLightsToDrawAndDrawThem()
        {
            int lightSourceIndex = 0;

            // NEW: draw only lights:
            foreach (LightSource lightSource in lightSourcesToDraw)
            {
                lightSource.CopyQuadToVertexBuffer(lightSourceVertices, lightSourceIndex);
                lightSourceIndex++;
            }

            bool lightsToDraw = lightSourceIndex > 0 || lightEmittingModels.Count > 0;

            if (lightsToDraw)
            {
                The.Client.GraphicsDevice.BlendState = lightsBlendAdd;
            }

            if (lightSourceIndex > 0)
            {

                DrawLightSources(lightSourceIndex, DrawLightsTechnique.TwoDeeLightSources);

                lightSourceIndex = 0;

                // normal blending:
                /* The.Client.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha; // XNA 3
                 The.Client.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;*/

            }

            if (lightEmittingModels.Count > 0)
            {
                // create one light source quad containing the full screen render from the emitting models.
                LightSource lightSource = new LightSource(Vector3.Zero);
                lightSource.SetupQuadVertices(Vector3.Zero, Vector2.Zero, 0f, emissiveModelLightRenderTarget.Bounds, emissiveModelLightRenderTarget);

                lightSourceIndex = 0;
                lightSource.CopyQuadToVertexBuffer(lightSourceVertices, lightSourceIndex);
                lightSourceIndex = 1;

                DrawLightSources(lightSourceIndex, DrawLightsTechnique.ModelEmittedLight);
            }

            if (lightsToDraw)
            {
                The.Client.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            }

            lightEmittingModels.Clear();

            lightSourcesToDraw.Clear();

        }

        /// <summary>
        /// Helper applies the edge detection effect.
        /// </summary>
        void DrawOutlines()
        {
            if (The.Client.spriteBatch == null)
                return;

            Effect edgeDetect = The.Client.EdgeDetectEffect;
            EffectParameterCollection parameters = edgeDetect.Parameters;


           //      SaveRenderTargetToFile("EdgeDetectNormalDepthRenderTarget", edgeDetectNormalDepthRenderTarget);
            //     SaveRenderTargetToFile("EdgeDetectSceneRenderTarget", EdgeDetectSceneRenderTarget);

          //  SaveRenderTargetToFile("DiffuseMSRenderTarget", DiffuseMSRenderTarget); // crashes... 



            
            parameters["EdgeWidth"].SetValue(0.4f); //0.3f); 
            parameters["EdgeIntensity"].SetValue(0.4f); //0.5f);

            Vector2 resolution = new Vector2(DiffuseMSRenderTarget.Width, DiffuseMSRenderTarget.Height);
            parameters["ScreenResolution"].SetValue(resolution);
            parameters["NormalDepthTexture"].SetValue(edgeDetectNormalDepthRenderTarget);
            


            
            // Activate the appropriate effect technique.
            edgeDetect.CurrentTechnique = edgeDetect.Techniques["EdgeDetect"];

            
            The.Client.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, effect: edgeDetect);

           // edgeDetect.CurrentTechnique.Passes[0].Apply();

           // The.Client.spriteBatch.Draw(diffuseRenderTarget, Vector2.Zero, Color.White);
            The.Client.spriteBatch.Draw(DiffuseMSRenderTarget, Vector2.Zero, Color.White);

            The.Client.spriteBatch.End();
                        
        }

        private enum DrawLightsTechnique { TwoDeeLightSources, ModelEmittedLight }
        /// <summary>
        /// draws 2d lights by combining them with the scene and extra depth data (DistanceHeightAndBillboardAlpha)
        /// </summary>
        /// <param name="lightSourceIndex"></param>
        private void DrawLightSources(int lightSourceIndex, DrawLightsTechnique tech)
        {
            Vector3 multiplier = new Vector3(1f - TimeOfDayLightingFactor.X, 1f - TimeOfDayLightingFactor.Y, 1f - TimeOfDayLightingFactor.Z);
            float darknessLevel = multiplier.Length(); //  night: 0.76, day: 0, evening: 0.3
            // normalize:
            darknessLevel = MathHelper.Clamp(darknessLevel / 0.7f, 0f, 1f);

            if (tech == DrawLightsTechnique.TwoDeeLightSources)
            {
                lightSourceEffect.CurrentTechnique = lightSourceEffect.Techniques["DrawLightSources"];

                lightSourceEffect.Parameters["LightSourceTexture"].SetValue(GameData.Instance.LightSourcesSpriteSheet.Texture);

            }
            else
            {
                lightSourceEffect.CurrentTechnique = lightSourceEffect.Techniques["DrawModelEmitterLights"];

                lightSourceEffect.Parameters["EmitterLightSourceDistance"].SetValue(emissiveModelLightDistanceRenderTarget);

                lightSourceEffect.Parameters["LightSourceTexture"].SetValue(emissiveModelLightRenderTarget);

            }

            lightSourceEffect.Parameters["UseIntegerPositions"].SetValue(!The.MapUI.IsScrolling);

            lightSourceEffect.Parameters["DarknessLevel"].SetValue(darknessLevel);

            Dimension dim = The.Client.Controller.DrawArea;
            Vector2 viewportSize = new Vector2(dim.Width, dim.Height);
            /*
            Viewport viewport = The.Client.GraphicsDevice.Viewport;
            Vector2 viewportSize = new Vector2(viewport.Width, viewport.Height);*/
            lightSourceEffect.Parameters["ViewportSize"].SetValue(viewportSize);
            lightSourceEffect.Parameters["WindowPosition"].SetValue(The.MapUI.MapWindowWorldPosition);



            //depthmap.Save("depthmap.jpg", ImageFileFormat.Jpg);
            lightSourceEffect.Parameters["DistanceHeightAndBillboardAlpha"].SetValue(DistanceHeightAndBillboardAlphaRenderTarget);


            lightSourceEffect.Parameters["DiffuseSceneTexture"].SetValue(diffuseRenderTarget);
          //  lightSourceEffect.Parameters["DiffuseSceneTexture"].SetValue(DiffuseMSRenderTarget);

            //UWGame.SimSide.Instance.GraphicsDevice.VertexDeclaration = lightSourceQuadVertexDeclaration;
            
            foreach (EffectPass pass in lightSourceEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                The.Client.GraphicsDevice.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList, lightSourceVertices, 0, lightSourceIndex * 4, lightSourceIndices, 0, lightSourceIndex * 2);

            }

        }



        private void DrawBillboardBatch(int featureQuadIndex, RenderTechnique technique)
        {

            billboardEffect.Parameters["UseIntegerPositions"].SetValue(!The.MapUI.IsScrolling);

            // draw the quads now:
            if (technique == RenderTechnique.Standard)
            {
                // Important - don't write to the depth buffer. Only the 3d models use it for sorting their meshes. Everything else is sorted 'manually'.
                The.Client.GraphicsDevice.DepthStencilState = DepthStencilState.None;


                if (DayAndNightEffects.SunAnimation != DayAndNightEffects.SunAnimations.Night)
                {   // render in daylight
#if DEBUG || PROFILE
                    if (Kensei.Dev.Options.GetOption("Rendering.Show light amount"))
                    {
                        billboardEffect.CurrentTechnique = billboardEffect.Techniques["StandardDebugLighting"];
                    }
                    else
                    {
                        billboardEffect.CurrentTechnique = billboardEffect.Techniques["Standard"];
                    }
#else
                    
                    billboardEffect.CurrentTechnique = billboardEffect.Techniques["Standard"];
                    
#endif
                    // for normal mapping light effect:
                    billboardEffect.Parameters["ShadowFactor"].SetValue(DayAndNightEffects.GetOwnShadowFactor());
                }
                else
                {   // render at night. No normal map shading.
                    billboardEffect.CurrentTechnique = billboardEffect.Techniques["StandardAtNight"];
                }

              // SaveTextureToFile("BillboardSpriteSheetNormalTexture", GameData.Instance.BillboardSpriteSheet.NormalTexture);

                Dimension dim = The.Client.Controller.DrawArea;
                Vector2 viewportSize = new Vector2(dim.Width, dim.Height);
                /*
                Viewport viewport = The.Client.GraphicsDevice.Viewport;
                Vector2 viewportSize = new Vector2(viewport.Width, viewport.Height);*/
                billboardEffect.Parameters["ViewportSize"].SetValue(viewportSize);
                billboardEffect.Parameters["WindowPosition"].SetValue(The.MapUI.MapWindowWorldPosition);
                billboardEffect.Parameters["DiffuseTexture"].SetValue(GameData.Instance.BillboardSpriteSheet.Texture);
                billboardEffect.Parameters["NormalTexture"].SetValue(GameData.Instance.BillboardSpriteSheet.NormalTexture);
                // on the normal maps, z and y are exchanged.                   
                billboardEffect.Parameters["LightPosition"].SetValue(new Vector3(
                                                                         The.Sim.DateAndTime.SunPosition.X,
                                                                         The.Sim.DateAndTime.SunPosition.Y,
                                                                         The.Sim.DateAndTime.SunPosition.Z
                                                                         ));

                billboardEffect.Parameters["WindTime"].SetValue(windTime);
                billboardEffect.Parameters["ShadowXAlignment"].SetValue(DayAndNightEffects.ShadowXAlignment);


                foreach (EffectPass pass in billboardEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    // IMPORTANT: No state changes here without CommitChanges!
                    The.Client.GraphicsDevice.DrawUserIndexedPrimitives(
                        PrimitiveType.TriangleList, featureVertices, 0, featureQuadIndex * 4, featureIndices, 0, featureQuadIndex * 2);

                }
            }
            else
            {

                if (technique == RenderTechnique.NormalsAndDepth)
                {   // render normals and depth map for edge processing
                    //Everything else is sorted 'manually'!!!
                    The.Client.GraphicsDevice.DepthStencilState = DepthStencilState.None;

                    The.Client.GraphicsDevice.BlendState = BlendState.NonPremultiplied;
                    billboardEffect.CurrentTechnique = billboardEffect.Techniques["NormalsAndDepthMap"];
                }
                else if (technique == RenderTechnique.DepthHeightBillboardAlpha)
                {
                    //The.Client.GraphicsDevice.DepthStencilState = DepthStencilState.None; // ADD THIS HERE???

                    billboardEffect.CurrentTechnique = billboardEffect.Techniques["DepthHeightBillboardAlpha"];
                }
                else if (technique == RenderTechnique.NoLighting)
                {   // render fake drop shadows
                    billboardEffect.CurrentTechnique = billboardEffect.Techniques["Shadow"];
                    billboardEffect.Parameters["Rotation"].SetValue(DayAndNightEffects.SunShadowRotationMatrix);
                    billboardEffect.Parameters["ShadowScaling"].SetValue(DayAndNightEffects.ShadowScaling);
                }

                Dimension dim = The.Client.Controller.DrawArea;
                Vector2 viewportSize = new Vector2(dim.Width, dim.Height);
                /*
                Viewport viewport = The.Client.GraphicsDevice.Viewport;
                Vector2 viewportSize = new Vector2(viewport.Width, viewport.Height);
                */
                billboardEffect.Parameters["ViewportSize"].SetValue(viewportSize);
                billboardEffect.Parameters["WindowPosition"].SetValue(The.MapUI.MapWindowWorldPosition);
                billboardEffect.Parameters["DiffuseTexture"].SetValue(GameData.Instance.BillboardSpriteSheet.Texture);
                billboardEffect.Parameters["WindTime"].SetValue(windTime);
                billboardEffect.Parameters["ShadowXAlignment"].SetValue(DayAndNightEffects.ShadowXAlignment);


                foreach (EffectPass pass in billboardEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    // IMPORTANT: No state changes here without CommitChanges!

                    The.Client.GraphicsDevice.DrawUserIndexedPrimitives(
                        PrimitiveType.TriangleList, featureVertices, 0, featureQuadIndex * 4, featureIndices, 0, featureQuadIndex * 2);

                }
            }

        }

        public void UpdateTerrainViewMatrix()
        {
            TerrainCameraPosition = CameraTarget; // -UWGame.SimSide.Instance.CameraDirection;
            Vector3 terrainCameraTarget = CameraTarget;

            TerrainCameraPosition.Z = -1000f; 

            // WHY is this offset necessary?
            float offset = 150f;
           /* if (The.Client.Controller.DrawWithZoom())
            {
                offset /= The.Client.Controller.Options.ZoomFactor; // makes no difference..?
            }*/
            TerrainCameraPosition.X += offset; 
            // we should be looking straight down - change the target also
            terrainCameraTarget.X = TerrainCameraPosition.X;

            Vector3 cameraUpVector = Vector3.Cross(CameraTarget - TerrainCameraPosition, Vector3.Left); //Vector3.Left); //new Vector3(-1, 0, 0);           
            cameraUpVector.Normalize();
            TerrainViewMatrix = Matrix.CreateLookAt(TerrainCameraPosition, terrainCameraTarget /*UWGame.SimSide.Instance.CameraTarget*/, cameraUpVector);

        }

        private Vector4 ComputeTimeOfDayLightMultiplier(Color tint)
        {
            Vector4 ambientColor = tint.ToVector4();
            ambientColor *= ambientColor.W;

            float deltaRed = ambientColor.X;
            float deltaGreen = ambientColor.Y;
            float deltaBlue = ambientColor.Z;

            ambientColor.X = 1f - deltaBlue - deltaGreen;
            ambientColor.Y = 1f - deltaRed - deltaBlue;
            ambientColor.Z = 1f - deltaRed - deltaGreen;
            ambientColor.W = 1f;

            return ambientColor;
        }

        /* OLD:
        private Vector4 ComputeTimeOfDayLightMultiplier(Color tint)
        {            
            Vector4 ambientColor = tint.ToVector4();
            ambientColor *= ambientColor.W;

            float deltaRed = 0.5f - (1f - ambientColor.X) / 2f;
            float deltaGreen = 0.5f - (1f - ambientColor.Y) / 2f;
            float deltaBlue = 0.5f - (1f - ambientColor.Z) / 2f;

            ambientColor.X = 0.5f - deltaBlue - deltaGreen;
            ambientColor.Y = 0.5f - deltaRed - deltaBlue;
            ambientColor.Z = 0.5f - deltaRed - deltaGreen;
            ambientColor.W = 1f;

            return ambientColor;
        }*/

        private void DrawTimeOfDayOverlay()
        {
            /*    Color? tint = The.Sim.DateAndTime.GetTimeOfDayColor();
                // is null during the day. todo: render lights when it is overcast?
            
                if (!tint.HasValue)
                {
                    tint = Color.White;
                }*/

            GraphicsDevice device = The.Client.GraphicsDevice;

            //device.SetRenderTarget(null);// xna 3?
            //device.SetRenderTarget(EdgeDetectSceneRenderTarget);

            //device.DepthStencilBuffer = UWGame.SimSide.Instance.MultiSamplingStencilBuffer;

            TimeOfDayLightingEffect.Parameters["baseTexture"].SetValue(diffuseRenderTarget);
          //  TimeOfDayLightingEffect.Parameters["baseTexture"].SetValue(DiffuseMSRenderTarget);

            TimeOfDayLightingEffect.Parameters["AmbientColorForLightSources"].SetValue(TimeOfDayLightingFactor);

            TimeOfDayLightingEffect.CurrentTechnique = TimeOfDayLightingEffect.Techniques["AmbientLight"];
            // TimeOfDayLightingEffect.Begin();
            foreach (EffectPass pass in TimeOfDayLightingEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                The.Client.quadRenderer.Render(device, - Vector2.One, Vector2.One);
            }

        }


        /* OLD:
        * private void DrawTimeOfDayOverlay()
           {
            
            
               Color? tint = The.Sim.DateAndTime.GetTimeOfDayColor();
               // is null during the day. todo: render lights when it is overcast?
               if (tint.HasValue)
               {
                   // multiplicative blending
                   // value 0.5 = no effect
                   // < 0.5: darkening
                   // > 0.5: lightening
                   // http://blogs.msdn.com/shawnhar/archive/2007/01/02/spritebatch-and-custom-blend-modes.aspx
                   GraphicsDevice device = game.graphics.GraphicsDevice;

                   //device.RenderState.BlendFunction = BlendFunction.m
                   device.RenderState.AlphaBlendEnable = true;
                   device.RenderState.SourceBlend = Blend.DestinationColor;
                   device.RenderState.DestinationBlend = Blend.SourceColor;

                   device.SetRenderTarget(0, null);//UWGame.SimSide.Instance.EdgeDetectSceneRenderTarget);
                   device.DepthStencilBuffer = UWGame.SimSide.Instance.MultiSamplingStencilBuffer;

                   TimeOfDayLightingEffect.Parameters["baseTexture"].SetValue(LightRenderTarget.GetTexture());

                   TimeOfDayLightingEffect.Parameters["AmbientColorForLightSources"].SetValue(TimeOfDayLightingFactor);
                

                   TimeOfDayLightingEffect.CurrentTechnique = TimeOfDayLightingEffect.Techniques["ApplyLightSourcesMap"];
                   TimeOfDayLightingEffect.Begin();
                   foreach (EffectPass pass in TimeOfDayLightingEffect.CurrentTechnique.Passes)
                   {
                       pass.Begin();
                       game.quadRenderer.Render(-Vector2.One, Vector2.One);
                       pass.End();
                   }
                   TimeOfDayLightingEffect.End();

                   // re-enable normal alpha blending:
                   device.RenderState.SourceBlend = Blend.SourceAlpha;
                   device.RenderState.DestinationBlend = Blend.InverseSourceAlpha;

                   //   Rectangle drawnRectangle = new Rectangle(0, 0, this.mapWindowWidth, mapWindowHeight);
                   //   UWGame.SimSide.Instance.spriteBatch.Draw(UWGame.SimSide.Instance.interfaceArt, drawnRectangle, new Rectangle(0, 0, 4, 10), tint);
               }

           }*/

        /// <summary>
        /// Computes the world matrices for all the models we are going to draw.
        /// the matrices can be used both when drawing as normal, to normalDepth map, and drop shadows.
        /// </summary>
        /*   private void ComputeModelMatricesForDrawing()
           {
               TerrainTile tileToDraw;

               for (int y = 0; y < The.MapUI.noOfTilesToDisplayVertically; y++)
               {
                   for (int x = 0; x < The.MapUI.noOfTilesToDisplayHorizontally; x++)
                   {
                       tileToDraw = map.TileMap[x + The.MapUI.mapX, y + The.MapUI.mapY];

                       // draw people:
                       if (tileToDraw.EntitiesOnTile != null)
                       {
                           foreach (Entity entity in tileToDraw.EntitiesOnTile)
                           {
                               // move to base class? no, call IAnimatedModel...
                               if (entity.Renderable.RenderAsModel != null)
                               {
                                   entity.Renderable.RenderAsModel.ComputeMatricesForDrawing(AnimatedModel.Transformations.All, entity.EntityType.Renderable.RenderAsModelType.ModelScale);
                               }
                            
                           }
                       }
                 
                   }
               }
           }*/



        /// <summary>
        /// Runs a per-triangle picking algorithm over all the models in the scene,
        /// storing which triangle is currently under the cursor.
        /// </summary>
        public void UpdatePicking()
        {
            // Look up a collision ray based on the current cursor position. See the
            // Picking Sample documentation for a detailed explanation of this.
            Ray cursorRay = CalculateCursorRay(The.Client.Projection, View);

            //  Kensei.Dev.DevText.Print(cursorRay.ToString());

            // Clear the previous picking results.
            //insideBoundingSpheres.Clear();

            PickedModel = null;

            // Keep track of the closest object we have seen so far, so we can
            // choose the closest one if there are several models under the cursor.
            float closestIntersection = float.MaxValue;

            // Entity entity;
            Renderable renderable;

            // we can pick memory facts too:
            IKnownEntityData entityData;

            // Loop over all our models.
            foreach (var sortedList in sortedObjectsToDraw)
            {
                foreach (var drawObject in sortedList)
                {
                    renderable = drawObject.AsRenderable;

                    //   renderable
                    if (renderable != null && renderable.RenderAsModel != null) // entity != null && entity.Renderable.RenderAsModel != null)
                    {
                        entityData = renderable.Parent;

                        if (entityData != null)
                        {
                            //((IAnimatedModel)drawObject).Draw(RenderTechnique.Standard);

                            bool insideBoundingSphere;
                            Vector3 vertex1, vertex2, vertex3;

                            // Perform the ray to model intersection test.
                            float? intersection = RayIntersectsModel(cursorRay, renderable.RenderAsModel.AnimatedModel.ModelAnimator.Model,
                                                                     renderable.RenderAsModel.AnimatedModel.StandardDrawingWorldTransformation,
                                                                     out insideBoundingSphere,
                                                                     out vertex1, out vertex2,
                                                                     out vertex3);

                            // If this model passed the initial bounding sphere test, remember
                            // that so we can display it at the top of the screen.
                            /*       if (insideBoundingSphere)
                                   {
                                        Kensei.Dev.DevText.Print("Is inside BoundingSphere of " + drawObject.ToString(), Color.White);
                                       //          
                                
                                   }*/

                            // Do we have a per-triangle intersection with this model?
                            if (intersection != null)
                            {
                                // If so, is it closer than any other model we might have
                                // previously intersected?
                                if (intersection < closestIntersection)
                                {

                                    // Store information about this model.
                                    closestIntersection = intersection.Value;
                                    PickedModel = entityData.EntityID;


                                }
                            }
                            else if (insideBoundingSphere && PickedModel == null && drawObject != null)
                            {
                                // - avoid this? makes it harder to select a structure behind a model!
                              //  PickedModel = entityData;// we may not be exactly pickworthy, but we'll catch anyone nearby if none better
                            }
                        }
                    }
                }
            }
        }


        Viewport DrawAreaViewport;

        // CalculateCursorRay Calculates a world space ray starting at the camera's
        // "eye" and pointing in the direction of the cursor. Viewport.Unproject is used
        // to accomplish this. see the accompanying documentation for more explanation
        // of the math behind this function.
        public Ray CalculateCursorRay(Matrix projectionMatrix, Matrix viewMatrix)
        {
            InputData inputData = The.Client.Controller.InputData;
            // create 2 positions in screenspace using the cursor position. 0 is as
            // close as possible to the camera, 1 is as far away as possible.
            Vector3 nearSource = new Vector3(inputData.mouseX, inputData.mouseY, 0f);
            Vector3 farSource = new Vector3(inputData.mouseX, inputData.mouseY, 1f);

            // use Viewport.Unproject to tell what those two screen space positions
            // would be in world space. we'll need the projection matrix and view
            // matrix, which we have saved as member variables. We also need a world
            // matrix, which can just be identity.
            Vector3 nearPoint = DrawAreaViewport.Unproject(nearSource,
                projectionMatrix, viewMatrix, Matrix.Identity);

            Vector3 farPoint = DrawAreaViewport.Unproject(farSource,
                projectionMatrix, viewMatrix, Matrix.Identity);

          /*  Vector3 nearPoint = The.Client.GraphicsDevice.Viewport.Unproject(nearSource,
                projectionMatrix, viewMatrix, Matrix.Identity);

            Vector3 farPoint = The.Client.GraphicsDevice.Viewport.Unproject(farSource,
                projectionMatrix, viewMatrix, Matrix.Identity);
            */

            // find the direction vector that goes from the nearPoint to the farPoint
            // and normalize it....
            Vector3 direction = farPoint - nearPoint;
            direction.Normalize();

            // and then create a new ray using nearPoint as the source.
            return new Ray(nearPoint, direction);
        }

        /// <summary>
        /// Checks whether a ray intersects a model. This method needs to access
        /// the model vertex data, so the model must have been built using the
        /// custom TrianglePickingProcessor provided as part of this sample.
        /// Returns the distance along the ray to the point of intersection, or null
        /// if there is no intersection.
        /// </summary>
        static float? RayIntersectsModel(Ray ray, Model model, Matrix modelTransform,
                                         out bool insideBoundingSphere,
                                         out Vector3 vertex1, out Vector3 vertex2,
                                         out Vector3 vertex3)
        {
            vertex1 = vertex2 = vertex3 = Vector3.Zero;

            // The input ray is in world space, but our model data is stored in object
            // space. We would normally have to transform all the model data by the
            // modelTransform matrix, moving it into world space before we test it
            // against the ray. That transform can be slow if there are a lot of
            // triangles in the model, however, so instead we do the opposite.
            // Transforming our ray by the inverse modelTransform moves it into object
            // space, where we can test it directly against our model data. Since there
            // is only one ray but typically many triangles, doing things this way
            // around can be much faster.

            Matrix inverseTransform = Matrix.Invert(modelTransform);



            // Look up our custom collision data from the Tag property of the model.
            Dictionary<string, object> tagData = (Dictionary<string, object>)model.Tag;

            if (tagData == null)
            {
                throw new InvalidOperationException(
                    "Model.Tag is not set correctly. Make sure your model " +
                    "was built using the custom TrianglePickingProcessor.");
            }

            // Start off with a fast bounding sphere test.
            BoundingSphere boundingSphere = (BoundingSphere)tagData["BoundingSphere"];

            BoundingSphere boundingSphereWorld = boundingSphere.Transform(modelTransform);

            float? worldlIntersect = boundingSphereWorld.Intersects(ray);

            ray.Position = Vector3.Transform(ray.Position, inverseTransform);
            ray.Direction = Vector3.TransformNormal(ray.Direction, inverseTransform);
            if (worldlIntersect == null) // boundingSphere.Intersects(ray) == null) //inverse transform is not working it seems
            {
                // If the ray does not intersect the bounding sphere, we cannot
                // possibly have picked this model, so there is no need to even
                // bother looking at the individual triangle data.
                insideBoundingSphere = false;

                return null;
            }
            else
            {
                // The bounding sphere test passed, so we need to do a full
                // triangle picking test.
                insideBoundingSphere = true;

                // return 1;

                // Keep track of the closest triangle we found so far,
                // so we can always return the closest one.
                float? closestIntersection = null;

                // Loop over the vertex data, 3 at a time (3 vertices = 1 triangle).
                Vector3[] vertices = (Vector3[])tagData["Vertices"];

                for (int i = 0; i < vertices.Length; i += 3)
                {
                    // Perform a ray to triangle intersection test.
                    float? intersection;

                    RayIntersectsTriangle(ref ray,
                                          ref vertices[i],
                                          ref vertices[i + 1],
                                          ref vertices[i + 2],
                                          out intersection);

                    // Does the ray intersect this triangle?
                    if (intersection != null)
                    {
                        return intersection; // don't test anymore...

                        // If so, is it closer than any other previous triangle?
                        if ((closestIntersection == null) ||
                            (intersection < closestIntersection))
                        {
                            // Store the distance to this triangle.
                            closestIntersection = intersection;

                            // Transform the three vertex positions into world space,
                            // and store them into the output vertex parameters.
                            Vector3.Transform(ref vertices[i],
                                              ref modelTransform, out vertex1);

                            Vector3.Transform(ref vertices[i + 1],
                                              ref modelTransform, out vertex2);

                            Vector3.Transform(ref vertices[i + 2],
                                              ref modelTransform, out vertex3);
                        }
                    }
                }

                return closestIntersection;
            }
        }


        /// <summary>
        /// Checks whether a ray intersects a triangle. This uses the algorithm
        /// developed by Tomas Moller and Ben Trumbore, which was published in the
        /// Journal of Graphics Tools, volume 2, "Fast, Minimum Storage Ray-Triangle
        /// Intersection".
        /// 
        /// This method is implemented using the pass-by-reference versions of the
        /// XNA math functions. Using these overloads is generally not recommended,
        /// because they make the code less readable than the normal pass-by-value
        /// versions. This method can be called very frequently in a tight inner loop,
        /// however, so in this particular case the performance benefits from passing
        /// everything by reference outweigh the loss of readability.
        /// </summary>
        static void RayIntersectsTriangle(ref Ray ray,
                                          ref Vector3 vertex1,
                                          ref Vector3 vertex2,
                                          ref Vector3 vertex3, out float? result)
        {
            // Compute vectors along two edges of the triangle.
            Vector3 edge1, edge2;

            Vector3.Subtract(ref vertex2, ref vertex1, out edge1);
            Vector3.Subtract(ref vertex3, ref vertex1, out edge2);

            // Compute the determinant.
            Vector3 directionCrossEdge2;
            Vector3.Cross(ref ray.Direction, ref edge2, out directionCrossEdge2);

            float determinant;
            Vector3.Dot(ref edge1, ref directionCrossEdge2, out determinant);

            // If the ray is parallel to the triangle plane, there is no collision.
            if (determinant > -float.Epsilon && determinant < float.Epsilon)
            {
                result = null;
                return;
            }

            float inverseDeterminant = 1.0f / determinant;

            // Calculate the U parameter of the intersection point.
            Vector3 distanceVector;
            Vector3.Subtract(ref ray.Position, ref vertex1, out distanceVector);

            float triangleU;
            Vector3.Dot(ref distanceVector, ref directionCrossEdge2, out triangleU);
            triangleU *= inverseDeterminant;

            // Make sure it is inside the triangle.
            if (triangleU < 0 || triangleU > 1)
            {
                result = null;
                return;
            }

            // Calculate the V parameter of the intersection point.
            Vector3 distanceCrossEdge1;
            Vector3.Cross(ref distanceVector, ref edge1, out distanceCrossEdge1);

            float triangleV;
            Vector3.Dot(ref ray.Direction, ref distanceCrossEdge1, out triangleV);
            triangleV *= inverseDeterminant;

            // Make sure it is inside the triangle.
            if (triangleV < 0 || triangleU + triangleV > 1)
            {
                result = null;
                return;
            }

            // Compute the distance along the ray to the triangle.
            float rayDistance;
            Vector3.Dot(ref edge2, ref distanceCrossEdge1, out rayDistance);
            rayDistance *= inverseDeterminant;

            // Is the triangle behind the ray origin?
            if (rayDistance < 0)
            {
                result = null;
                return;
            }

            result = rayDistance;
        }


    }

    public struct VertexMultitextured : IVertexType
    {
        public Vector3 Position;
        public /*Vector4*/ Vector2 TextureCoordinate;
        public Vector4 TexWeights;
        public Vector4 TintColor0;
        public Vector4 TintColor1;
        public Vector4 TintColor2;
        // public Vector4 TintColor3;

        //  public Vector4 NoiseChannelToUse;
        public Vector4 AlphaSharpness;
        public Vector4 NoiseScaling;


        // public int
        //  public Vector3 WorldPosition; // NEW

        public static int SizeInBytes = (3 + 2 + 4 + 4 + 4 + 4 + 4 + 4) * sizeof(float);
        /*public static VertexElement[] VertexElements = new VertexElement[]
         {
             new VertexElement( 0, 0, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Position, 0 ),
             new VertexElement( 0, sizeof(float) * 3, VertexElementFormat.Vector4, VertexElementMethod.Default, VertexElementUsage.TextureCoordinate, 0 ),             
             new VertexElement( 0, sizeof(float) * 7, VertexElementFormat.Vector4, VertexElementMethod.Default, VertexElementUsage.TextureCoordinate, 1 ),
             new VertexElement( 0, sizeof(float) * 11, VertexElementFormat.Vector4, VertexElementMethod.Default, VertexElementUsage.Color, 0 )
         };
        */
        public static VertexElement[] VertexElements = new VertexElement[]
         {
             new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0 ),          
             new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0 ),
             new VertexElement(sizeof(float) * 5, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 1 ),
             new VertexElement(sizeof(float) * 9, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 2 ),
             new VertexElement(sizeof(float) * 13, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 3 ),
            // new VertexElement( 0, sizeof(float) * 17, VertexElementFormat.Vector4, VertexElementMethod.Default, VertexElementUsage.TextureCoordinate, 4 ),
             new VertexElement(sizeof(float) * 17, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 4 ),
             new VertexElement(sizeof(float) * 21, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 5 ),
             new VertexElement(sizeof(float) * 25, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 6 )//,
          //   new VertexElement( 0, sizeof(float) * 29, VertexElementFormat.Vector4, VertexElementMethod.Default, VertexElementUsage.TextureCoordinate, 7 ) 
            // new VertexElement( 0, sizeof(float) * 25, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Position, 1 ) // NEW: WroldPosition
         };

        private readonly static VertexDeclaration vertexDeclaration = new VertexDeclaration(VertexElements);

        public VertexDeclaration VertexDeclaration
        {
            get { return vertexDeclaration; }
        }
    }

    public struct VertexGroundFeature : IVertexType
    {
        public Vector3 Position;
        public Vector2 TextureCoordinate;
        public Vector3 WorldPosition;
        public Vector4 TintColor;

        public static int SizeInBytes = (3 + 2 + 3 + 4) * sizeof(float);
        public static VertexElement[] VertexElements = new VertexElement[]
         {
             new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0 ),
             new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0 ),
             new VertexElement(sizeof(float) * 5, VertexElementFormat.Vector3, VertexElementUsage.Position, 1 ),
             new VertexElement(sizeof(float) * 8, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 1 ),
         };

        private readonly static VertexDeclaration vertexDeclaration = new VertexDeclaration(VertexElements);

        public VertexDeclaration VertexDeclaration
        {
            get { return vertexDeclaration; }
        }
    }



}
