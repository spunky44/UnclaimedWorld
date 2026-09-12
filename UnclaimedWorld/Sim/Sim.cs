
#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;//TODO DECOUPLE don't use namespace in sim
using Microsoft.Xna.Framework.Content;//TODO DECOUPLE don't use namespace in sim
using Microsoft.Xna.Framework.Graphics;//TODO DECOUPLE don't use namespace in sim
using Microsoft.Xna.Framework.Input;//TODO DECOUPLE don't use namespace in sim
////using Microsoft.Xna.Framework.Storage;
using System.Xml;
using UWGame.ClientSide.Interface;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Items;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Vehicles;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Buildings;
using UWGame.ClientSide.Particles;
using UWGame.SimSide.AI.Activities;
using SpriteSheetRuntime;
using WindowSystem;
using InputEventSystem;
using Xclna.Xna.Animation;
using UWGame.SimSide.Trees;
using System.Threading;
using System.Xml.Serialization;
using System.IO;
using System.Diagnostics;
using System.Linq;
using UWGame.SimSide.Vegetation;
using UWGame.SimSide.Soil;
using GameStateManagement;
using UWGame.SimSide.Overland;
using UWGame.SimSide.AI;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.IngameEvents;
using UWGame.SimSide.Entities.Body;
using UWGame.SimSide.AI.Needs;
using UWGame.SimSide.AllGameData;
using UWGame.ClientSide.Renderables;//TODO DECOUPLE
using UWGame.SimSide.Collisions;
using UWGame.SimSide.Systems;
using UWGame.SimSide.InGameEvents;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Systems.Triggers;
using UWGame.ClientSide;
using UWGame.ClientSide.Log;
using UWGame.Control;
using UWGame.Control.Replays;
using UWGame.SimSide.AI.Goals;
using UWGame.ClientSide.Interface.HUD_Windows;
using UWGame.SimSide.Scenarios;
using UWGame.ClientSide.Screens;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.AllGameData.Scenarios;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Processes;
using System.Reflection;
using UWGame.SimSide.Systems.TimeSlicing;
using UWGame.SimSide.Snapshots;
using UWGame.Client.MainMenu.LoadSavedGame;
using UWGame.SimSide.Entities.Owners;
using System.IO.Compression;
using UWGame.SimSide.Overland.Locations;
using UWGame.ClientSide.Interface.Overlays;
using UWGame.ClientSide.Interface.Inventory;
using UWGame.ClientSide.Interface.BuyAndSell;
using UWGame.SimSide.Entities.Containers;
using UWGame.ClientSide.Interface.Tasks;
using UWGame.SimSide.SimEffects;
using UWGame.SimSide.Overland.Missions;
using UWGame.SimSide.Overland.Missions.Templates;
using UWGame.SimSide.GatheringSites;
using UWGame.SimSide.Entities.Substances;
using UWGame.SimSide.AI.Pathfinding;
using UWGame.ClientSide.Interface.Ledger;
using UWGame.ClientSide.Feedback;


#endregion


namespace UWGame.SimSide
{

    public enum Speeds { Pause, Normal, TwiceNormal, FourTimesNormal }

    /// <summary>should someday encompass all the Sim side classes and manages thier updates and such
    /// It includes the Map 
    /// The entities
    /// anything that evaluates
    /// jobs
    /// AI
    /// anything that thinks
    /// Client side objects are freely accessible and writable from classes on SimSide. SimSide classes are responsible for constructing and monitoring client classes, and ensuring they really exist. Client class is still responsible for giving client classes an update timeslice.    
    /// 
    ///if the gameclient is missing or disabled, the simulation
    // will operate unaltered. This is called playing "headless"
    // Renderable gets updated in a separate block in the game main loop, after the simulation tick
    // also, the refresh rate of the client (Renderables...) may be different from the simulation
    // Entities and their components are considered SIMULATION side, and Renderable types are CLIENT side
    // These should always stay decoupled. Simulation is read-only to the client, and client is write-only to simulation
    // MLo
    /// </summary>
    public class Sim : GameScreen, ISnapshot //TODO  MLo: Make Sim something apart from GameScreen. 
    //Client should be of type ScreenManager, and the UI should consiste of GameScreens, 
    //but SIm should be something different, something not client dependent.
    {



        //public bool IsPaused { get; private set; }
        private Speeds speed = Speeds.Normal;

        public Speeds Speed
        {
            get
            {
                return speed;
            }
        }


        public bool IsPaused
        {
            get
            {
                return speed == Speeds.Pause;
            }
        }

        private float GameSpeed
        {
            get
            {
                switch (speed)
                {

                    case Speeds.Normal:
                        return 1f;

                    case Speeds.TwiceNormal:
                        return 2f;

                    case Speeds.FourTimesNormal:
                        return 4f;

                    default:
                        return 1f;
                }
            }
        }


        public bool IsGameOver = false;

        public enum SerializeMode { WriteAndRead, NoSerialize, Read }
        public static SerializeMode CurrentSerializeMode = SerializeMode.NoSerialize; // .NoSerialize; //SerializeMode.NoSerialize; // SerializeMode.Read; // SerializeMode.NoSerialize; // SerializeMode.NoSerialize; ////  // SerializeMode.NoSerialize; // SerializeMode.WriteAndRead; //SerializeMode.NoSerialize; //  SerializeMode.WriteAndRead; // SerializeMode.NoSerialize; // SerializeMode.WriteAndRead; // SerializeMode.NoSerialize; // should be 'Read' when shipping!


        //  private int mapRefCount = 0;//MLo prevent more than one map at a time
        // public MapManager Map;

        public TriggerSystem TriggerSystem;

        public enum PersonSex // TODO Refactor Move to EntityType or PersonType
        {
            Male,
            Female
        }


        public Dictionary<EntityID, Tuple<WaitingFor, double>> WaitingAgents = new Dictionary<EntityID, Tuple<WaitingFor, double>>(); // HashSet<EntityID>();

        public DateAndTime DateAndTime;

        /// <summary>
        /// let this symbol be independent of the slider max value.
        /// </summary>
        public const int HasNoLimitValue = -1;


        /// <summary>
        /// move these to Site???
        /// </summary>      
        public List<EntityID> AllStructures = new List<EntityID>();
        public List<EntityID> AllTerrainEntities = new List<EntityID>();

        public List<string> DebugAttackLog = new List<string>();

        /// <summary>
        /// these values are saved for debugging purposes, nothing else...
        /// </summary>
        public Dictionary<ResourceType, Tuple<NoiseParams, SimplexNoise>> ResourceNoiseSeeds = null;
        //  public Dictionary<ResourceType, Tuple<ChangeResourcesAction.NoiseParams, byte[]>> ResourceNoiseSeeds = null; 


        /// <summary>
        /// The site for the local map. a cached reference to the single Site in World that has IsPlaySite set to true
        /// </summary>
        public Site PlaySite;
        SiteID snapshotPlaySite;



        /// <summary>
        /// The currently simulated world.
        /// </summary>
        public World World;


        public Dictionary<Jobs.ProcessJob, List<AI.Goals.GoalDoProduceAtomic>> ProductionJobsRequiringEnergy =
                    new Dictionary<ProcessJob, List<global::UWGame.SimSide.AI.Goals.GoalDoProduceAtomic>>();

        public CycleManager CycleManager;

        /// <summary>
        /// this should do everything, playsite and othersite, agents, degrading etc.
        /// </summary>
        SleepyUpdater<Entity> entities;

        /// <summary>
        /// holds non-sleeping entities when snapshotting
        /// </summary>
        List<EntityID> snapshotEntities;

        private static Collections CollectionOfCollections = new Collections();
        public static void AddLookupCollectible(Type t, ILookUpCollectible collection)
        {
            // don't call this from a static ctor.           
            // add all static col instances explicitly when starting a new game instead of lazily via static ctors
            // still snapshot the col? yes. then the col of cols will always be complete


            if (!CollectionOfCollections.ContainsKey(t))
                CollectionOfCollections.Add(t, collection);
        }

        public static void RemoveLookupCollectible(Type t)
        {
            CollectionOfCollections.Remove(t);
        }

        //     public Dictionary<IDiscoverable, ConditionalEvent> DetectEvents = new Dictionary<IDiscoverable, ConditionalEvent>();


        private enum SeasonPrefix : ulong { Early, Mid, Late };
        private enum Seasons : ulong { Spring, Summer, Autumn, Winter };

        public enum DayPhases : ulong { Work, Leisure, Sleep };
        public const double SleepPhaseStarts = 0.923;
        public const double WorkPhaseStarts = 0.23;
        public const double LeisurePhaseStarts = 0.615;


        /// <summary>
        /// Who are currently attacking this entity?
        /// use this list to disperse/find spots for attackers...
        /// 
        /// This list does not belong in Entity, because it does not depend on the entity being still alive.
        /// It also does not belong in MemoryFact, since it is shared by all allegiances.
        /// </summary>      
        public Dictionary<EntityID, Dictionary<EntityID, Vector3>> MeleeAttackers = new Dictionary<EntityID, Dictionary<EntityID, Vector3>>();

        public EngineMode Mode = EngineMode.Game;//TODO Move to common and rename

        /// <summary>
        /// Somteimes null, causing a crash after load???
        /// 
        /// these are saved in the snapshot header part
        /// </summary>
        public StartGameParams StartGameParams;

        public StartGameMode startGameMode; // temporary, used during start/load only

        // List<Tuple<EntityID, Vector2, Vector2>> snapshotCollisionManagerList;
        List<EntityID> snapshotCollisionManagerList;
        List<Pair<EntityID, Vector2>> snapshotAgentQuadTree;



        public const int FoodStockSize = 2;


        Regulator performanceRegulator;

        public GameTime GameTime = new GameTime();

        TimeSpan totalUnPausedGameTime;

        /// <summary>
        /// keeps track of the total time we have been paused
        /// see http://social.msdn.microsoft.com/Forums/en/xnagamestudioexpress/thread/640ca42d-1809-4fb9-af53-bc11592c1538
        /// 
        /// I saw a savegame where this time, when converted to the date format, was different from the DateAndTime.Current value after 2 years...
        /// </summary>
        public TimeSpan TotalUnPausedGameTime
        {
            get
            {
                return totalUnPausedGameTime;
            }
            private set
            {
                totalUnPausedGameTime = value;
                TotalUnPausedGameTimeInSeconds = totalUnPausedGameTime.TotalSeconds;
            }
        }

        /// <summary>
        /// a cached conversion from ticks to seconds
        /// gets persisted in save games
        /// 
        /// does it cause problems to use this for Client stuff too..???
        /// </summary>
        public double TotalUnPausedGameTimeInSeconds
        {
            get;
            private set;
        }


        TimeSpan elapsedTime = TimeSpan.Zero;

        int entityCounter = 0;

        public enum EngineMode { Game, Edit }
        //public 
        private RandomGenerator gameplayRandomGenerator;
        public RandomGenerator GameplayRandomGenerator
        {
            get
            {
                return gameplayRandomGenerator;
            }
        }

        #region Snapshotted client data

        /// <summary>
        /// client value to snapshot
        /// </summary>
        Vector2? snapshotMapWindowWorldPosition;

        List<EventDialogData> snapshotEvents;

        OverlaySettings snapshotOverlaySettings;
        InventorySettings snapshotInventorySettings;
        TaskSettings snapshotTaskSettings;
        BuySellPanelSettings snapshotBuySettings;
        BuySellPanelSettings snapshotSellSettings;
        FoodProductionSettings snapshotFoodProductionSettings;
        ProductionSettings snapshotProductionSettings;
        KillsSettings snapshotKillsSettings;
        NutrientSheetSettings snapshotNutrientSheetSettings;

        //   TrackTarget snapshotTrackedEntityType;      

        #endregion

        /// <summary>
        /// create game
        /// </summary>
        /// <param name="screenManager"></param>
        /// <param name="scenarioSettings"></param>
        /// <param name="randomSeed"></param>
        /*    public Sim(Controller screenManager,
               ScenarioSettings? scenarioSettings,          
               int randomSeed)
            {
                this.ScreenManager = screenManager;
                this.scenarioSettings = scenarioSettings;
                this.Mode = EngineMode.Game;
          
                CommonInit(randomSeed);
            }
            */

        /// <summary>
        /// debug/test
        /// </summary>
        /// <param name="screenManager"></param>
        /// <param name="scenario"></param>
        /// <param name="randomSeed"></param>
        /*  public Sim(Controller screenManager,
             PlaceGameEntities.DebugScenarios scenario,
             int randomSeed)
          {
              this.ScreenManager = screenManager;          
              this.debugScenarioToLoad = scenario;          
              this.Mode = EngineMode.Game;
           
              CommonInit(randomSeed);

          }*/


        /*  public Sim(Controller screenManager, 
              string mapToLoad, 
              string modToLoad, 
              PlaceGameEntities.DebugScenarios scenario, ScenarioSettings? scenarioSettings, 
              EngineMode mode, 
              int randomSeed)
          {

              this.ScreenManager = screenManager;
              this.mapToLoad = mapToLoad;
              this.modToLoad = modToLoad;
              this.debugScenarioToLoad = scenario;
              this.scenarioSettings = scenarioSettings;
              this.Mode = mode;
           
              CommonInit(randomSeed);
          }//ctor
          */

        public Sim(Controller screenManager,
            StartGameParams startGameParams, int? randomSeed)
        {
            this.Controller = screenManager;
            this.StartGameParams = startGameParams;

            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5); // if transition time is 0, The.Sim is destroyed immediately when clicking OK on the last modal dialog. Then Client must check Sim to avoid crashing while finishing its update.

            The.Sim = this;


            // IMPORTANT!
            // all the rest of the construction is now done in QueueSimInit, below
            // -MLo
            if (randomSeed.HasValue)
            {
                gameplayRandomGenerator = new RandomGenerator(randomSeed.Value, RandomGenerator.GeneratorType.Sim);
            }
            else
            {
                gameplayRandomGenerator = new RandomGenerator(RandomGenerator.GeneratorType.Sim);
            }

            CreateLookupCollections(); // do this before loading game data types
        }



        private void CreateRegulators()
        {
            performanceRegulator = new Regulator(gameplayRandomGenerator, 0.3d, "Sim", Regulator.Modes.Sim);

        }



        private enum QUEUESTATE
        {
            BEGIN,
            LOAD_BASE_DATA, LOAD_SCENARIO_DATA, LOAD_MAP_DATA, LOAD_MOD_DATA,
            Initialize,
            PostDataCompleteInitialize,
            CREATE_META_DATA,
            INIT_CONSTANTS,
            EVERYTHING_ELSE,
            DONE,
            WRITESCENARIOS
        }


        BaseDataLoader baseDataLoader;
        DataLoader scenarioDataLoader;

        /// <summary>       
        /// data priority: base data - mods - map - scenario 
        /// load base data first, then mods, then map, then scenario - overwrite keys/delete them
        /// </summary>
        /// <returns></returns>
        private QUEUESTATE queueState = QUEUESTATE.BEGIN;
        public bool QueueGameDataAndSimInit()
        {
            Scenario scenario = StartGameParams?.StartScenarioParams?.Scenario; // can be null

            switch (queueState)
            {
                case QUEUESTATE.BEGIN:
                    {
                        baseDataLoader = new BaseDataLoader();

                        //StartScenarioParams scenarioParams = this.StartGameParams.StartScenarioParams; 
                        if (scenario != null) // scenarioParams != null)
                        {
                            scenarioDataLoader = AllScenarioLoader.GetScenarioDataLoader(scenario); // used to read in scenario-specific GameData
                        }

                        queueState = QUEUESTATE.LOAD_BASE_DATA;
                        break;
                    }
                case QUEUESTATE.LOAD_BASE_DATA:
                    {

                        //here we spin on QueueInitGameTypeLists() until it returns true(done)
                        if (baseDataLoader.QueueInitGameData(scenario))
                        {

                            The.LoadScreen.Progress("Load scenario data...", 200); //??
                            queueState = QUEUESTATE.LOAD_SCENARIO_DATA;
                        }

                        break;
                    }
                // add MODS here

                case QUEUESTATE.LOAD_SCENARIO_DATA: // load any scenario-specific entity types, events etc. In case of conflicts, they will override the previously defined types.
                    {
                        bool goToNextQueueState = false;
                        if (scenarioDataLoader != null)
                        {
                            if (scenarioDataLoader.QueueInitGameData(scenario))
                            {
                                goToNextQueueState = true;
                            }
                        }
                        else
                        {
                            goToNextQueueState = true;
                        }

                        if (goToNextQueueState)
                        {
                            The.LoadScreen.Progress("PostDataCompleteInitialize...", 1015);
                            queueState = QUEUESTATE.PostDataCompleteInitialize;
                        }

                        break;
                    }


                case QUEUESTATE.PostDataCompleteInitialize:
                    {
                        // here we want to do a second pass and call Initialize + PostInitializeValidate after all collections are complete.

                        GameData.Instance.PostDataCompleteInitialize();

                        The.LoadScreen.Progress("Replace data placeholders...", 1015);

                        if (CurrentSerializeMode == SerializeMode.WriteAndRead)
                        {
                            The.LoadScreen.Progress("Write scenarios...", 3015);
                            queueState = QUEUESTATE.WRITESCENARIOS;
                        }
                        else
                        {
                            The.LoadScreen.Progress("More SimInit stuff...", 50);
                            queueState = QUEUESTATE.EVERYTHING_ELSE;

                        }
                        break;
                    }

                case QUEUESTATE.WRITESCENARIOS: // we only perform this step when serializing vanilla scenarios to xml before deploying!
                    {
                        // we already load the scenario headers on the create game screen... is this still needed?
                        // these data are special - they are only loaded on demand                     
                        if (CurrentSerializeMode == SerializeMode.WriteAndRead)
                        {
                            RGScenarioLoader.Serialize();
                        }

                        The.LoadScreen.Progress("More SimInit stuff...", 50);
                        queueState = QUEUESTATE.EVERYTHING_ELSE;

                        break;
                    }
                case QUEUESTATE.EVERYTHING_ELSE:
                    {
                        DateAndTime = new SimSide.DateAndTime();

                        // these values are reset if loading.
                        /*
                        TransitionOnTime = TimeSpan.FromSeconds(1.5);
                        TransitionOffTime = TimeSpan.FromSeconds(0.5); // Set during load also??? // Lars: I believe this should be the same value as for Client - otherwise things screw up when exiting...  TimeSpan.FromSeconds(0.5);
                        */

                        TriggerSystem = new TriggerSystem();


                        Locale.Init();

                        The.Map = new MapManager();

                        CycleManager = new CycleManager();

                        InitEntityUpdater();
                        //EntityUpdater = new SleepyUpdater<Entity>(Systems.Module.Sim, true);

                        CreateRegulators();

                        //////////////////////////////
                        // The.Snapshotter = new Snapshotter();
                        //////////////////////////////

                        queueState = QUEUESTATE.DONE;
                        break;
                    }
                case QUEUESTATE.DONE:
                    {
                        // set progress variable back so it will be ready for the next time we load/restart:
                        queueState = QUEUESTATE.BEGIN;

                        return true;
                    }

            }

            return false;
        }


        public override void Init()
        {
            base.Init();
            The.Map.Init();

        }

        public class FileOpenException: Exception
        {
            public FileOpenException(string message, Exception innerException): base(message, innerException)
            {

            }

        }

        public void DoSave(string fullFilePath)
        {           
            SnapshotHeader header = new SnapshotHeader(StartGameParams);

            // this can throw a variety of exceptions, no access etc.:
            // lets' catch them all, and displat them to the user:
            FileStream stream;
            try
            {
                stream = File.Open(fullFilePath, FileMode.Create);  // colon gives: System.NotSupportedException
            }
            catch(Exception e)
            {
                throw new FileOpenException(e.Message, e);
            }

            GZipStream cmp = new GZipStream(stream, CompressionMode.Compress);
            BufferedStream buffStrm = new BufferedStream(cmp, 65536);

            using (BinaryWriter writer = new BinaryWriter(buffStrm))
            {
                The.Snapshotter.Save(writer, header, true, true, true);
            }
        }

       
        /// <summary>
        /// Remeber that the Sim instance with this reference is replaced during load
        /// </summary>
        Thread loadSaveGameThread;
        enum LoadSaveGameProgressFlag { NotStarted, Ongoing, Done }

        LoadSaveGameProgressFlag loadSaveGameProgressFlag = LoadSaveGameProgressFlag.NotStarted;
       // bool isLoadingSavedGame = false;

        /// <summary>
        /// needs to be static since Sim is exchanged
        /// 
        /// is set true in case of exceptions too.
        /// </summary>
        public static bool LoadIsFinished = false;
        public static Exception LoadException = null;


        class LoadSavedGameParams
        {
            public string LoadFilePath;
           // public UnclaimedWorld Game;
        }


        /// <summary>
        /// can be polled by main thread to see if the worker is finished.
        /// 
        /// Sim progress variables are replaced together with the Sim instance!
        /// 
        /// Client will never see the thread finish..? Since the dialog will have been replaced, it will not receive any Updates while hidden
        /// </summary>
        /// <param name="fullFilePath"></param>
        /// <returns></returns>
        public bool LoadSavedGame(string fullFilePath) 
        {            
            if (loadSaveGameProgressFlag == LoadSaveGameProgressFlag.NotStarted) /*  isLoadingSavedGame == false
                && loadSaveGameThread == null)*/
            {
                
                // catch any load exceptions and prsetn them in the main thread?
                loadSaveGameThread = new System.Threading.Thread(LoadSavedGameInThread);
                loadSaveGameThread.IsBackground = true;
                loadSaveGameProgressFlag = LoadSaveGameProgressFlag.Ongoing;
              //  isLoadingSavedGame = true;
                LoadIsFinished = false;
                loadSaveGameThread.Priority = ThreadPriority.AboveNormal;
                loadSaveGameThread.Start(new LoadSavedGameParams() { LoadFilePath = fullFilePath }); 
             
               // The.InGameUI.MessageBox.ShowMessage("Saving. Please wait...", true, MessageBox.ButtonOptions.None); // gets removed after load when client is recreated

            }
            else
            {              
                // only the progress screen will enter this code, not the save/load dialog

                if (LoadIsFinished)
                {
                    if (loadSaveGameThread != null
                        && loadSaveGameThread.IsAlive)
                    {
                        if (Thread.CurrentThread != loadSaveGameThread)
                        {
                            loadSaveGameThread.Join();
                          //  loadSaveGameThread = null;
                        }
                    }

                    loadSaveGameProgressFlag = LoadSaveGameProgressFlag.Done;
                    //isLoadingSavedGame = false;

                    if (LoadException != null)
                    {
                        // Note: if Sim fails to load, this code will likely not be entered by the main thread.
                        throw LoadException; // throw this from the main thread.
                    }
                }  
            }

            return false;
        }

      
        private static void LoadSavedGameInThread(object parms)  //fullFilePath)
        {
            LoadSavedGameParams loadParms = parms as LoadSavedGameParams;

            string fullFilePath = loadParms.LoadFilePath;
            
             #if RELEASE  
            try 
            {
            #endif
            

                GZipStream cmp = new GZipStream(File.Open((string)fullFilePath, FileMode.Open), CompressionMode.Decompress);
                BufferedStream buffStrm = new BufferedStream(cmp, 65536);

                using (BinaryReader reader = new BinaryReader(buffStrm))
                {
                    The.Snapshotter.Load(reader);
                }


                
             #if RELEASE                   
                }
                catch (Exception e)
                {
                    LoadIsFinished = true;

                    LoadException = new UWException("This Save game failed to load. This can be because the file is out of date. " + Environment.NewLine + Environment.NewLine + "SOLUTION: If your current version of the game is different than the version number on the save file, you can revert to an earlier version of the game. Steam makes it possible to select a different branch/version of the game, and on the Unclaimed World forum on Steam there are instructions on how to do this. " + Environment.NewLine + Environment.NewLine + "If this does not work, you can make a post in the Unclaimed World forum on Steam and paste the error report." + Environment.NewLine + Environment.NewLine, e) { IncludePasteInstructions = false };
 
                    // store the exception so the main thread can handle it. Only the main thread can show a dialog etc.

                    // The.Sim may be null during load.
                    //loadParms.Game.HandleExceptionInReleaseMode(loadException, false);
                }
#endif

        }

        /*
        /// <summary>
        /// performs a load preserving the current GameData (EntityTypes etc.)
        /// 
        /// only clear collections if the Client will be recreated during load!
        /// </summary>
        /// <param name="fullFilePath"></param>
        /// <param name="verifyCompleteness"></param>
        public static bool LoadSavedGame(string fullFilePath) //, bool clearCollections)
        {
            // run this in a worker thread??

            GZipStream cmp = new GZipStream(File.Open(fullFilePath, FileMode.Open), CompressionMode.Decompress);
            BufferedStream buffStrm = new BufferedStream(cmp, 65536);

            using (BinaryReader reader = new BinaryReader(buffStrm))
            {
                The.Snapshotter.Load(reader); //, clearCollections);
            }

            return true;
        }*/

        /// <summary>
        /// NEW: always called to reset all ID counters
        /// OLD: only do this for in-game load. 
        /// this clears client collections as well, such as SleepyUpdaterLookUp.Renderable. This requires a new Client().
        /// </summary>
        public void FreeMemoryBeforeLoad()
        {
            ClearData(false);

            /*
            foreach (ILookUpCollectible lc in CollectionOfCollections.Values) // 
            {
                if (lc.SnapshotThis) // collections of gamedatatypes must survive.
                {
                    lc.ClearCollection(); // unless the type is an interface, this will dynamic invoke ResetIDCounter too (interfaces cannot implement methods, that's why we cannot invoke them)
                }
            }
            RegionMap.AllRegionMaps.Clear();*/

        }


        private void ClearData(bool clearBaseData)
        {

            // Important: prevent memory leaks - clear all static collections!!!        

            #region Reset static ID counters

            foreach (ILookUpCollectible lc in CollectionOfCollections.Values)
            {
                if (clearBaseData || lc.SnapshotThis) // collections of gamedatatypes must survive.
                {
                    lc.ClearCollection(); // unless the type is an interface, this will dynamic invoke ResetIDCounter too (interfaces cannot implement methods, that's why we cannot invoke them)
                }
            }

            //*** Reset ID Counters for interface LookUps
            // for interface types, LookupCollectible can only clear the collection. It cannot reset the counter. so do it here.

            Cyclable.ResetIDCounterNoInvoke();
            ResourceItem.ResetIDCounterNoInvoke();
            Composite.ResetIDCounterNoInvoke();
            HasMembers.ResetIDCounterNoInvoke();
            Detectable.ResetIDCounterNoInvoke();
            MethodCounter.ResetIDCounterNoInvoke();
            IMapCounter.ResetIDCounterNoInvoke();
            Owner.ResetIDCounterNoInvoke();
            HasEntityGroup.ResetIDCounterNoInvoke();
            HasCrops.ResetIDCounterNoInvoke();

            //*******

            //****** Reset non-LookUp id counters

            Event.ResetOtherIDCounter();
            TalkEvent.ResetOtherIDCounter();
            ItemStorage.ResetIDCounterNoInvoke();

            //****



            #endregion


            //OLD           
            //  RenderableFactory.Clear();

            GoalDoProduceAtomic.ClearPool();
            GoalTraverseEdgeBetweenWaypointsAtomic.ClearPool();
            GoalDoTakeFiveAtomic.ClearPool();


            #region Clear other static collections outside the Lookup class

            //  MovementMap.AllMovementMaps.Clear();

            BodyPart.ResetBodyPartCounter();


            #endregion




            The.AgentQuadTree = null;
            The.CollisionManager = null;
            The.Map = null;
            The.Sim = null;
        }


        /// <summary>
        /// is this called during Save/Load..? no.
        /// </summary>
        public override void Destroy()
        {
            base.Destroy();

            ClearData(true);

            GameData.UnloadAllData();


            Kensei.Dev.Options.RemoveAllOptions();

        }

        public void DestroyAllEntities()
        {

            for (int i = The.Sim.PlaySite.Entities.Count - 1; i > 0; i--)
            {
                The.Sim.PlaySite.Entities[i].Destroy(false);
            }


        }

        /// <summary>
        /// Lars added this
        /// </summary>
        public override void ExitScreen()
        {
            base.ExitScreen();
            Controller.GameEnded();

        }
 
      
        public string GetDifficultyKey() 
        {
            StartScenarioParams parms = The.Sim.StartGameParams.StartScenarioParams;
            if (parms != null)
            {
                return parms.MainDifficultyKey;
            }

            return null;

        }

       

      

        /// <summary>
        /// Save-or-Load-or_crc every object whose state affect the simulation
        /// </summary>
        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            /*  const Snapshotter.Version CurrentVersion = (Snapshotter.Version)1;
              Snapshotter.Version version = CurrentVersion;
              version = sn.DoVersion(Snapshotter.Version.Original); 
              */

            Stopwatch watch = Stopwatch.StartNew();

            #region client values - these should be few...

            if (sn.mode != Snapshotter.Mode.Load)
            {
                if (The.Client != null)
                {
                    snapshotMapWindowWorldPosition = The.MapUI.MapWindowWorldPosition;
                    snapshotEvents = The.Client.EventDialogsData;

                    if (The.InGameUI != null)
                    {
                        snapshotOverlaySettings = The.InGameUI.OverlaySettings; // ugly, but necessary..  
                        snapshotInventorySettings = The.InGameUI.InventorySettings;
                        snapshotBuySettings = The.InGameUI.BuySettings;
                        snapshotSellSettings = The.InGameUI.SellSettings;
                        snapshotTaskSettings = The.InGameUI.TaskSettings;
                        snapshotFoodProductionSettings = The.InGameUI.FoodProductionSettings;
                        snapshotKillsSettings = The.InGameUI.KillsSettings;
                        snapshotProductionSettings = The.InGameUI.ProductionSettings;
                        snapshotNutrientSheetSettings = The.InGameUI.NutrientSheetSettings;
                    }
                    else
                    {
                        snapshotOverlaySettings = null;
                        snapshotInventorySettings = null;
                        snapshotBuySettings = null;
                        snapshotSellSettings = null;
                        snapshotTaskSettings = null;
                        snapshotFoodProductionSettings = null;
                        snapshotProductionSettings = null;
                        snapshotNutrientSheetSettings = null;
                    }
                }
                else
                {
                    snapshotMapWindowWorldPosition = null;
                    snapshotEvents = null;
                    snapshotOverlaySettings = null;
                    snapshotInventorySettings = null;

                }
            }

            snapshotMapWindowWorldPosition = sn.DoVector2Nullable(snapshotMapWindowWorldPosition);
            snapshotEvents = sn.DoList(snapshotEvents);
            snapshotOverlaySettings = (OverlaySettings)sn.DoISnapshot(snapshotOverlaySettings);
            snapshotInventorySettings = (InventorySettings)sn.DoISnapshot(snapshotInventorySettings);

            snapshotBuySettings = (BuySellPanelSettings)sn.DoISnapshot(snapshotBuySettings);
            snapshotSellSettings = (BuySellPanelSettings)sn.DoISnapshot(snapshotSellSettings);
            snapshotTaskSettings = (TaskSettings)sn.DoISnapshot(snapshotTaskSettings);
            snapshotFoodProductionSettings = (FoodProductionSettings)sn.DoISnapshot(snapshotFoodProductionSettings);
            snapshotProductionSettings = (ProductionSettings)sn.DoISnapshot(snapshotProductionSettings);
            snapshotKillsSettings = (KillsSettings)sn.DoISnapshot(snapshotKillsSettings);          
            snapshotNutrientSheetSettings = (NutrientSheetSettings)sn.DoISnapshot(snapshotNutrientSheetSettings);

            #endregion

            if (sn.mode != Snapshotter.Mode.Load)
            {
                snapshotEntities = new List<EntityID>();
                entities.IterateItems(e => snapshotEntities.Add(e.ID));
            }
            // we are responsible for snapshotting SleepyUpdater members:
            snapshotEntities = sn.DoList(snapshotEntities);


            CollectionOfCollections = (Collections)sn.DoISnapshot(CollectionOfCollections);   // entities, cyclables are snapshotted here      


            AllStructures = sn.DoList(AllStructures);
            AllTerrainEntities = sn.DoList(AllTerrainEntities);
            elapsedTime = sn.DoTimeSpan(elapsedTime);
            //EntitiesToSpawn = sn.DoList(EntitiesToSpawn) as List<EntityData>;
            entityCounter = sn.DoInt32(entityCounter);
            MeleeAttackers = sn.DoNestedDictionary(MeleeAttackers);
            Mode = sn.DoEnum(Mode);
            ProductionJobsRequiringEnergy = sn.DoMultiMap(ProductionJobsRequiringEnergy);
            startGameActionLog = sn.DoList(startGameActionLog);
            startGamePopulationSpawnLog = sn.DoList(startGamePopulationSpawnLog);
            IsGameOver = sn.DoBool(IsGameOver);
            TotalUnPausedGameTime = sn.DoTimeSpan(TotalUnPausedGameTime);
            TotalUnPausedGameTimeInSeconds = sn.DoDouble(TotalUnPausedGameTimeInSeconds);
            World = (World)sn.DoISnapshot(World);
            DateAndTime = (DateAndTime)sn.DoISnapshot(DateAndTime);
            //  speed = sn.DoEnum(speed); // start at 1x speed always


            /* MIGRATION EXAMPLE:
            //AllMonkeys was added in version 2 of Sim
            if ((uint)version >= 2)
            {
                AllMonkeys = (List<EntityID>)sn.DoCollection(AllMonkeys);
            }
            else 
            {
                AllMonkeys = MockAMonkeyCollectionOrSomething();//or just null, probably
            }
            */

            #region ID counters
            // for interface types.
            // saves an idcounter only. 
            // Remember to make a call to ResetIDCounterNoInvoke also!

            Cyclable.DoSnapshot(sn);
            Composite.DoSnapshot(sn);
            ResourceItem.DoSnapshot(sn);
            HasMembers.DoSnapshot(sn);
            Detectable.DoSnapshot(sn);
            MethodCounter.DoSnapshot(sn);
            IMapCounter.DoSnapshot(sn);
            HasEntityGroup.DoSnapshot(sn);
            Owner.DoSnapshot(sn);
            HasCrops.DoSnapshot(sn);

            #endregion

            // other classes depend on this:
            gameplayRandomGenerator = (RandomGenerator)sn.DoISnapshot(gameplayRandomGenerator);

            #region The - static globals:
            The.Map = (MapManager)sn.DoISnapshot(The.Map);

            if (sn.mode != Snapshotter.Mode.Load)
            {
                this.snapshotCollisionManagerList = The.CollisionManager.GetAllObjects().Select(e => e.ID).ToList(); // .GetAllObjectsAndPositions();

                this.snapshotAgentQuadTree = The.AgentQuadTree.GetAllObjectsAndPositions().Select(p => new Pair<EntityID, Vector2>(p.First.ID, p.Second)).ToList();

            }

            The.CollisionManager = (CollisionManager<Entity>)sn.DoISnapshot(The.CollisionManager);
            this.snapshotCollisionManagerList = sn.DoList(snapshotCollisionManagerList);
            
            The.AgentQuadTree = (PointQuadTree<Entity>)sn.DoISnapshot(The.AgentQuadTree);
            this.snapshotAgentQuadTree = sn.DoList(snapshotAgentQuadTree);

            #endregion

          

            CycleManager = (CycleManager)sn.DoISnapshot(CycleManager); // does not snapshot the registered clients - but these are all ICyclables, and are snapshotted from their LookUp collection.                      
            snapshotPlaySite = (SiteID)sn.SnapshotID<Site, SiteID>(PlaySite);
            TriggerSystem = (TriggerSystem)sn.DoISnapshot(TriggerSystem);



            sn.Ignore(WaitingAgents);
            sn.Ignore(GameTime);
            sn.Ignore(baseDataLoader);
            //  sn.Ignore(mapRefCount);          
            sn.Ignore(queueState);
            sn.Ignore(scenarioDataLoader);
            sn.Ignore(StartGameParams); // saved in the header part
            sn.Ignore(ResourceNoiseSeeds);
            sn.Ignore(IsPaused);
            sn.Ignore(DebugAttackLog);
            sn.Ignore(CurrentSerializeMode); // not needed at this stage after data has been loaded
            sn.Ignore(Controller);
            sn.Ignore(IsInNormalGameLoop);
            sn.Ignore(beginRunProgress);
            sn.Ignore(entities);
            sn.Ignore(speed);
            sn.Ignore(sortedStartGameEventActions);
            sn.Ignore(startGameEventActionsProgress);
            sn.Ignore(LoadException);
            sn.Ignore(loadSaveGameThread);
            sn.Ignore(loadSaveGameProgressFlag);
            sn.Ignore(startGameMode);
            sn.Ignore(placeGeoLayoutProgress);
            sn.Ignore(geoLayoutEntityProgress);


            base.IgnoreSnapshotFields(sn);

            watch.Stop();
            //   Snapshotter.Log("EntityFactory.Collection done in " + watch.Elapsed, Snapshotter.LogPriority.infinity);


            return this;
        }


        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            //lookups and other fix-ups
            //Since this is the root object, here  
            //there should be a huge block not unlike that in DoSnapshot,
            //but which calls LoadPostProcess on every object of type ISnapshot.      


            CycleManager.LoadPostProcess(sn);
            TriggerSystem.LoadPostProcess(sn);
            World.LoadPostProcess(sn);
            DateAndTime.LoadPostProcess(sn);
            //NutrientSheetSettings.loadpos

            // the collections need this value:
            PlaySite = LookUp<Site, SiteID>.FindByID(snapshotPlaySite);

            // Entity.LoadPostProcess() depends on this:


            /*   List<Tuple<Entity, Vector2, Vector2>> listToRebuildFrom = 
                snapshotCollisionManagerList.Select(t => new Tuple<Entity, Vector2, Vector2>(Entity.FindByID(t.Item1), t.Item2, t.Item3)).ToList();*/

            List<Pair<Entity, Vector2>> listToRebuildAgentQuadTreeFrom =
              snapshotAgentQuadTree.Select(t => new Pair<Entity, Vector2>(Entity.FindByID(t.First), t.Second)).ToList();

            The.AgentQuadTree.SetPreLoadPostProcess(listToRebuildAgentQuadTreeFrom);
            The.AgentQuadTree.LoadPostProcess(sn); // this will fix the quad tree


            The.Map.LoadPostProcess(sn);


            CollectionOfCollections.LoadPostProcess(sn);



            // this depends on Entity.Collidable:
            List<Collidable<Entity>> listToRebuildCollisionManagerFrom =
             snapshotCollisionManagerList.Select(t => Entity.FindByID(t).Collidable).ToList();

            The.CollisionManager.SetPreLoadPostProcess(listToRebuildCollisionManagerFrom);
            The.CollisionManager.LoadPostProcess(sn); // this will fix the quad tree



            if (snapshotOverlaySettings != null)
            {
                snapshotOverlaySettings.LoadPostProcess(sn);
            }

            if (snapshotInventorySettings != null)
            {
                snapshotInventorySettings.LoadPostProcess(sn);
            }

            if (snapshotBuySettings != null)
            {
                snapshotBuySettings.LoadPostProcess(sn);
            }
            if (snapshotSellSettings != null)
            {
                snapshotSellSettings.LoadPostProcess(sn);
            }

            InitEntityUpdater();
            if (snapshotEntities != null)
            {
                foreach (var item in snapshotEntities)
                {
                    entities.Add(LookUp<Entity, EntityID>.FindByID(item), keepExistingTimepoint: true);
                }
                snapshotEntities.Clear();  // clear for next save
            }




            CreateRegulators();

            // it is still too early to do client stuff now... use the method below.
            RecreateClientAfterInGameSave(sn);

            speed = Speeds.Normal;

        }




        public void RecreateClientAfterInGameSave(Snapshotter sn)
        {
            if (snapshotMapWindowWorldPosition.HasValue)
            {
                The.MapUI.MapWindowWorldPosition = snapshotMapWindowWorldPosition.Value;
            }

            if (snapshotEvents != null)
            {
                foreach (var item in snapshotEvents)
                {
                    item.LoadPostProcess(sn);
                }

                The.Client.EventDialogsData = snapshotEvents;
            }

            The.InGameUI.OverlaySettings = snapshotOverlaySettings;
            The.InGameUI.InventorySettings = snapshotInventorySettings;
            The.InGameUI.BuySettings = snapshotBuySettings;
            The.InGameUI.SellSettings = snapshotSellSettings;
            The.InGameUI.TaskSettings = snapshotTaskSettings;
            The.InGameUI.FoodProductionSettings = snapshotFoodProductionSettings;
            The.InGameUI.ProductionSettings = snapshotProductionSettings;
            The.InGameUI.KillsSettings = snapshotKillsSettings;           
            The.InGameUI.NutrientSheetSettings = snapshotNutrientSheetSettings;


            The.InGameUI.OverlaySettings.LoadPostProcess(sn);
            The.InGameUI.InventorySettings.LoadPostProcess(sn);
            The.InGameUI.BuySettings.LoadPostProcess(sn);
            The.InGameUI.SellSettings.LoadPostProcess(sn);
            The.InGameUI.TaskSettings.LoadPostProcess(sn);
            The.InGameUI.FoodProductionSettings.LoadPostProcess(sn);
            The.InGameUI.ProductionSettings.LoadPostProcess(sn);
            The.InGameUI.KillsSettings.LoadPostProcess(sn);
            The.InGameUI.NutrientSheetSettings.LoadPostProcess(sn);


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

        /* Monkey save version example
        Snapshotter.Version version = Snapshotter.Version.Original;
        public Snapshotter.Version DoVersion(Snapshotter sn)
        {
            version = sn.DoVersion((Snapshotter.Version)2);  // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }*/

        public bool IsSnapshotted { get; set; }

        public Sim()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }

        void Entities_ListMemberRemoved(object sender, int indexOfRemovedMember)
        {
            ObservableList<Entity>.UpdateCounterWhenItemIsRemoved(ref entityCounter, indexOfRemovedMember);
        }

        public bool TimepointReached(long TimepointInTicks)
        {
            return TotalUnPausedGameTime.Ticks >= TimepointInTicks;
        }

        public bool TimepointReached(double TimepointInSeconds)
        {
            return Common.IsGreaterThanOrEqual(TotalUnPausedGameTimeInSeconds, TimepointInSeconds);
        }

        public bool TimepointReached(double? TimepointInSeconds)
        {
            return TimepointInSeconds.HasValue && Common.IsGreaterThanOrEqual(TotalUnPausedGameTimeInSeconds, TimepointInSeconds.Value);
        }

        public bool TimepointReached(double? lastTimepointInSeconds, double firstTimepoint, double minimumTimeInSeconds)
        {
            if (lastTimepointInSeconds == null)
            {
                if (Common.IsGreaterThanOrEqual(TotalUnPausedGameTimeInSeconds, firstTimepoint))
                {
                    return true;
                }
            }
            else
            {
                return Common.IsGreaterThanOrEqual(TotalUnPausedGameTimeInSeconds, lastTimepointInSeconds.Value + minimumTimeInSeconds);
            }

            return false;
        }


        /// <summary>
        /// returns a value from 0-1 indicating the progress in the indicated phase. (0 if outside phase)
        /// </summary>
        /// <param name="phase"></param>
        /// <returns></returns>
        public double GetTimePhaseProgress(DayPhases phase)
        {
            switch (phase)
            {
                case DayPhases.Work:
                    if (DateAndTime.TimeOfDay > Sim.WorkPhaseStarts && DateAndTime.TimeOfDay < Sim.LeisurePhaseStarts)
                    {
                        return (DateAndTime.TimeOfDay - Sim.WorkPhaseStarts) / (Sim.LeisurePhaseStarts - Sim.WorkPhaseStarts);

                    }
                    else return 0;
                case DayPhases.Leisure:
                    if (DateAndTime.TimeOfDay > Sim.LeisurePhaseStarts && DateAndTime.TimeOfDay < Sim.SleepPhaseStarts)
                    {
                        return (DateAndTime.TimeOfDay - Sim.LeisurePhaseStarts) / (Sim.SleepPhaseStarts - Sim.LeisurePhaseStarts);

                    }
                    else return 0;
                case DayPhases.Sleep:
                    double sleepPhaseTillMidnight = 1d - Sim.SleepPhaseStarts;

                    if (DateAndTime.TimeOfDay > Sim.SleepPhaseStarts)
                    {
                        return (DateAndTime.TimeOfDay - Sim.SleepPhaseStarts) / (sleepPhaseTillMidnight + Sim.WorkPhaseStarts);
                    }
                    else if (DateAndTime.TimeOfDay < Sim.WorkPhaseStarts)
                    {
                        return (sleepPhaseTillMidnight + DateAndTime.TimeOfDay) / (sleepPhaseTillMidnight + Sim.WorkPhaseStarts);
                    }
                    else return 0;
            }
            return 0;

        }




        

        public void SetGameSpeed(Speeds speed)
        {
            this.speed = speed;
            /*
            switch (speed)
            {
                case Speeds.Normal:
                    GameSpeed = 1f;
                    break;

                case Speeds.TwiceNormal:
                    GameSpeed = 2f;
                    break;

                case Speeds.FourTimesNormal:
                    GameSpeed = 4f;
                    break;
            }*/

        }



        /// <summary>
        /// call The.Client.Pause also
        /// </summary>
        public void PauseGame()
        {
            speed = Speeds.Pause;
            //IsPaused = true;

            System.Diagnostics.Trace.WriteLine("PauseGame()");
            System.Diagnostics.Trace.WriteLine(Environment.StackTrace);

        }

        public void ResumeGame()
        {
            speed = Speeds.Normal;
            //IsPaused = false;
        }
        /*
        public void LoadEditorMap(string map)
        {
            if (PlaySite != null)
            {
                LoadMap(map);
            }
        }*/

        /*  private void LoadTestGameMap(string map)
          {
              if (PlaySite != null)
              {
                  LoadMap(map);

                  Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 5)));
              }
          }*/


        /*  private void LoadScenarioMap(string map)
          {
              LoadMap(map);

          
          }*/


        /* public bool LoadMap(string mapToLoad)
         {          
             The.Map.LoadMap(mapToLoad); // takes 2-3 s

             PostLoadMap();
            
             return true;
         }*/

        public void PostLoadMap()
        {
            The.Map.InitPostLoadMap();

            // new func:
            if (Mode == EngineMode.Game)
            {
                // in editor mode we only use terrain maps to check accessibility for instance.
                foreach (Allegiances.Allegiance allegiance in PlaySite.Allegiances)
                {
                    allegiance.SharedKnowledge.PlaySiteKnowledge.InitAuxiliaryMaps();
                }
            }


            //TODO this should be pulled by client, not pushed to it
            if (The.InGameUI != null && The.InGameUI.Minimap != null)
            {
                // update the minimap if we are reloading
                The.InGameUI.Minimap.CreateMap();
            }

            //TODO this should be pulled by client, not pushed to it
            if (The.InGameUI != null && The.InGameUI.FogMap != null)
            {
                // update the minimap if we are reloading
                The.InGameUI.FogMap.CreateMap();
            }
        }

        private void AdvancePlaceGeoLayout(int progress)
        {
            The.LoadScreen.Progress("Sim PlaceGeoLayout stage " + (placeGeoLayoutProgress + 1), progress);
            placeGeoLayoutProgress++;
        }

        private void Advance(int progress)
        {
            The.LoadScreen.Progress("Sim BeginRun stage " + (beginRunProgress + 1), progress); // 100);
            ++beginRunProgress;
           
        }

        #region Init Progress

        /// <summary>
        /// The new Sim instance gets this value set after Load as well
        /// </summary>
        public int beginRunProgress = 0;
        int geoLayoutEntityProgress = 0;
        int placeGeoLayoutProgress = 0;

        const int entitiesPerCycle = 100;

        #endregion

        public const int ProgressAfterSaveLoad = 4;

        /// <summary>
        /// Timesliced.
        /// AFTER Initialize AND LoadContent.
        /// </summary>
        public bool QueueBeginRun()
        {
            switch (beginRunProgress)
            {
                case 0:
                    // let's check that all assets referenced in GameData is there:
                    // Decouple..?
                    GameData.Instance.PostLoadContentValidate();
                    Advance(60); //Advance(4687);
                    return false;
                case 1:
                    StartGamePreLoadMap();

                    Advance(10);
                    return false;

                case 2:
                    InitRenderablesForEditorOrGame();

                    Advance(10);
                    return false;

                case 3://--------------------------------------------------------------------

                    // use a subqueue here - this takes too long for one step
                    if (StartGameLoadMapOrSavedGame()) // this loads the scenario map. if loading, it replaces this Sim instance with a new one in The.Sim.    takes 8 sec.                                  
                    {
                        Advance(10);  //Advance(12047);

                     //   The.Sim.beginRunProgress = beginRunProgress; // when loading: transfer the state to the new Sim instance in The.Sim !

                        return false;
                    }
                    else
                    {
                        return false;
                    }
              
                case ProgressAfterSaveLoad: // 4:

                    StartGamePostLoadMap();
                    Advance(10);

                    return false; 
                case 5:
                    if (StartGamePostLoadMapPrepareEventActions())
                    {
                        Advance(10);
                        return false;
                    }
                    else
                    {
                        return false;
                    }
                case 6:
                    if (StartGamePostLoadMapExecuteEvents())
                    {
                        Advance(10);
                        return false;
                    }
                    else
                    {
                        return false;
                    }
                case 7:
                    if (StartGamePostLoadMapRegisterEvents())
                    {
                        Advance(10);
                        return false;
                    }
                    else
                    {
                        return false;
                    }
                case 8://--------------------------------------------------------------------
                    // max advance: 12074 ms, 24000 entities = 240 cycles. Advance by 50 each cycle...
                    if (PlaySite.PlaceAllGeometry(ref geoLayoutEntityProgress, entitiesPerCycle)) //takes 10 secs, so split it up (does it prevent hangs??)...
                    {
                        // done.
                        Advance(67); // InitAuxiliaryMaps
                        return false;
                    }
                    else
                    {
                        AdvancePlaceGeoLayout(50);
                        return false; // still in this stage
                    }
               
                case 9:
                    PlaySite.InitAuxiliaryMaps();

                    beginRunProgress = 0;//reset for next time?
                    return true;
            }

            beginRunProgress++;//catches gaps in the case sequence ?!?!?!

            return false;

        }



        private void InitRenderablesForEditorOrGame()
        {
            foreach (var item in GameData.Instance.AllEntityTypes)
            {
                item.Value.InitRenderableTypeMode();
            }
        }


        /// <summary>
        /// AFTER Initialize AND LoadContent.
        /// </summary>
        /*    public bool BeginRun()
            {

                switch(state)
                {
                    case 0:
                        // let's check that all assets referenced in GameData is there:
                        GameData.Instance.PostLoadContentValidate();

                        return Advance();
                    case 1://--------------------------------------------------------------------
                  
                        StartGame(); // this loads the scenario map

                        state = 0;//reset for next time?
                        return true;
                
                }

                state++;//catches gaps in the case sequence
                return false;

             }*/

        List<string> startGameActionLog = new List<string>();

        List<string> startGamePopulationSpawnLog = new List<string>();

        /// <summary>
        /// only need to call this when starting a new game for the first time in the program session. 
        /// The static instances can survive going to the main menu and starting again... but perhapps we don't want that.
        /// </summary>
        private void CreateLookupCollections()
        {
            //CollectionOfCollections.Clear(); // NEW...

            // call all ILookup implementors, this replaces previous lazy init:
            // if some collections already exist, this should have no effect
            Allegiance.CreateLookupCollection();
            AllegianceRelation.CreateLookupCollection();


            Household.CreateLookupCollection();
            Expedition.CreateLookupCollection();
            Terrain.CreateLookupCollection();
            TerrainTile.CreateLookupCollection();

            SubtileLayer.CreateLookupCollection();
            SubtileLayers.CreateLookupCollection();

            Goal.CreateLookupCollection();

            SimProcess.CreateLookupCollection();
            ProcessMemory.CreateLookupCollection();
            SimEffect.CreateLookupCollection();
            SubstancePool.CreateLookupCollection();

            UWGame.SimSide.Trees.Tree.CreateLookupCollection();

            DiscomfortMap.CreateLookupCollection();

            Site.CreateLookupCollection();
            Route.CreateLookupCollection();

            PassengerOrCargoSlot.CreateLookupCollection();

            Trigger.CreateLookupCollection();

            TileResourceItem.CreateLookupCollection();

            GatheringSite.CreateLookupCollection();
            GatheringSiteType.CreateLookupCollection();

            Mission.CreateLookupCollection();
            MissionStop.CreateLookupCollection();
            MissionStopTemplate.CreateLookupCollection();
            MissionTemplate.CreateLookupCollection();
            MissionActionTemplate.CreateLookupCollection();
            ContractTemplate.CreateLookupCollection();

            //EventActionType.crea
            ReplenishActionType.CreateLookupCollection();
            ActionSetData.CreateLookupCollection();
            Conversation.CreateLookupCollection();

            ThreatGroup.CreateLookupCollection();

            ToolTypeCombination.CreateLookupCollection();

            LowVegetation.CreateLookupCollection();

            ResourceContainer.CreateLookupCollection();

            Job.CreateLookupCollection();

            NeedType.CreateLookupCollection();

            EntityGroup.CreateLookupCollection();

            Zone.CreateLookupCollection();

            Entity.CreateLookupCollection();
            MemoryFact.CreateLookupCollection();

            InfluenceMap.CreateLookupCollection();

            RegionSearchRequest.CreateLookupCollection();
            RegionPath.CreateLookupCollection();


            ActionLookup<IKnownProcess>.Create();
            ActionLookup<float>.Create();
            ActionLookup<List<EntityID>>.Create();
            ActionLookup<SimProcess>.Create();
            ActionLookup<int>.Create();

            ActionLookup.Create();


            Entity.CreateSleepyLookupCollection();
            Renderable.CreateSleepyLookupCollection();
            ParticleEmitter.CreateSleepyLookupCollection();
            Trigger.CreateSleepyLookupCollection();
            PolledEvent.CreateSleepyLookupCollection();
            SimProcess.CreateSleepyLookupCollection();
            EventAction.CreateSleepyLookupCollection();
            AccessibilityFeedback.CreateSleepyLookupCollection();
            ReplenishFeedback.CreateSleepyLookupCollection();
            TileResourceContainer.CreateSleepyLookupCollection();


            LookUp<ICyclable, CyclableID>.Create();
            LookUpICanIterateEntities.Create();
            LookUpHasEntityGroup.Create();
            LookUpIComposites.Create();
            LookUpIHasCrops.Create();
            LookUpOwners.Create();
            LookUpIDetectables.Create();

        }

        private bool StartGamePreLoadMap()
        {
            IsInNormalGameLoop = false;

            startGameMode = GetStartGameMode();

            switch(startGameMode)
            {
                case StartGameMode.Edit:
                    {
                        Mode = this.StartGameParams.StartGameEditorParams.EngineMode;

                        The.Sim.World = World.CreateFromWorldData(new WorldData() { WorldRadius = GameData.Instance.Constants.DefaultWorldRadius });

                        Site site = new Site("editorSite", true)
                        {
                            PlayerAllegiance = new Allegiance(AllegianceType.Player, GameData.Instance.AllEntityTypes["entity:human"], "editorAllegiance")
                        };

                        The.Map.StartLoadMap(new DirectoryInfo(this.StartGameParams.StartGameEditorParams.MapToLoadPath));


                       
                        /*  if (Mode == EngineMode.Edit)
                          {
                              LoadEditorMap(editorParams.MapToLoad);
                          }
                          else
                          {
                              LoadTestGameMap(editorParams.MapToLoad);
                          }*/
                        break;
                    }
                case StartGameMode.DebugNewGame:
                    {
                        StartDebugScenarioParams debugParams = this.StartGameParams.StartDebugScenarioParams;
                        The.Sim.World = World.CreateFromWorldData(new WorldData() { WorldRadius = GameData.Instance.Constants.DefaultWorldRadius });

                        Site site = new Site("debugSite", true)
                        {
                            Coords = new GeodeticCoordinate(10, 60),
                            Name = "Debug Site"
                        };

                        The.Map.StartLoadMap(debugParams.MapKey);

                        break;
                    }

                case StartGameMode.ScenarioNewGame:
                    {
                        // create a new game:
                        StartScenarioParams scenarioParams = this.StartGameParams.StartScenarioParams;
         
                        // set the time - must be done BEFORE the events are registered - TimeCondition needs the date to init correctly
                        DateAndTime.ResetTimeOfYearAndTimeOfDay(scenarioParams.Scenario.TimeDateYear);

                        ExecuteStartAction(scenarioParams.Scenario.ScenarioData.SpawnWorldAction);
                        ExecuteStartAction(scenarioParams.Scenario.ScenarioData.SpawnSiteAction);

                        The.Map.StartLoadMap(scenarioParams.Scenario.MapKey);

                        break;
                    }
            }
            
            return true;
        }

        public enum StartGameMode
        {
            Edit, 
            DebugNewGame,
            DebugLoadSaved,
            ScenarioNewGame,
            ScenarioLoadSaved           
        }

        StartGameMode GetStartGameMode()
        {
            try // crash here: http://steamcommunity.com/app/284100/discussions/2/333656722975368918/
            {
                StartGameEditorParams editorParams = this.StartGameParams.StartGameEditorParams;
                if (editorParams != null)
                {
                    return StartGameMode.Edit;
                }

                StartDebugScenarioParams debugParams = this.StartGameParams.StartDebugScenarioParams;
                if (debugParams != null)
                {
                    if (string.IsNullOrEmpty(StartGameParams.SavedGameToLoad))
                    {
                        return StartGameMode.DebugNewGame;
                    }
                    else
                    {
                        return StartGameMode.DebugLoadSaved;
                    }
                }

                StartScenarioParams scenarioParams = this.StartGameParams.StartScenarioParams;
                if (scenarioParams != null)
                {
                    if (string.IsNullOrEmpty(StartGameParams.SavedGameToLoad))
                    {
                        return StartGameMode.ScenarioNewGame;
                    }
                    else
                    {
                        return StartGameMode.ScenarioLoadSaved;
                    }
                }
 
            }
            catch(Exception e)
            {
                string text = "";
                if (this == null)
                {
                    text = "Sim is null";
                }
                else
                {
                    if (StartGameParams == null)
                    {
                        text = "StartGameParams is null";
                    }
                    else
                    {
                        if (this.StartGameParams.StartDebugScenarioParams == null)
                        {
                            text = "StartDebugScenarioParams is null";
                        }

                        if (this.StartGameParams.StartGameEditorParams == null)
                        {
                            text = "StartGameEditorParams is null";
                        }

                        if (this.StartGameParams.StartScenarioParams == null)
                        {
                            text = "StartScenarioParams is null";
                        }

                        if (StartGameParams.SavedGameToLoad == null)
                        {
                            text = "SavedGameToLoad is null";
                        }

                    }
                }

                throw new Exception("GetStartGameMode " + text, e);
            }
                
            return StartGameMode.Edit;
           
        }

        private bool StartGamePostLoadMap()
        {
            switch (startGameMode)
            {
                case StartGameMode.Edit:
                    {
                        PostLoadMap();

                        if (Mode == EngineMode.Edit)
                        {
                            //??
                        }
                        else
                        {
                            if (PlaySite != null)
                            {
                                Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 5)));
                            }
                        }

                        break;
                    }

                case StartGameMode.DebugNewGame:
                    {
                        PostLoadMap();

                        The.Sim.World.GetPlaySite().PlayerAllegiance = new Allegiance(AllegianceType.Player, GameData.Instance.AllEntityTypes["entity:human"], "debugAllegiance");

                        StartDebugScenarioParams debugParams = this.StartGameParams.StartDebugScenarioParams;
                        if (debugParams.ScenarioKey != PlaceGameEntities.DebugScenarios.None)
                        {
                            PlaceGameEntities.BeginRun(debugParams.ScenarioKey); // places entities
                        }

                        break;
                    }

                case StartGameMode.ScenarioNewGame:
                    {
                        PostLoadMap();

                        break;
                    }
            }


            return true;
            

            /*
            StartGameEditorParams editorParams = this.StartGameParams.StartGameEditorParams;
            if (editorParams != null)
            {
                PostLoadMap();

                if (Mode == EngineMode.Edit)
                {

                }
                else
                {
                    if (PlaySite != null)
                    {
                        Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 5)));
                    }
                }

            }


            StartDebugScenarioParams debugParams = this.StartGameParams.StartDebugScenarioParams;
            if (debugParams != null)
            {
                if (string.IsNullOrEmpty(StartGameParams.SavedGameToLoad))
                {
                    PostLoadMap();

                    The.Sim.World.GetPlaySite().PlayerAllegiance = new Allegiance(AllegianceType.Player, GameData.Instance.AllEntityTypes["entity:human"], "debugAllegiance");

                    if (debugParams.ScenarioKey != PlaceGameEntities.DebugScenarios.None)
                    {
                        PlaceGameEntities.BeginRun(debugParams.ScenarioKey); // places entities
                    }

                }
            }

            StartScenarioParams scenarioParams = this.StartGameParams.StartScenarioParams;
            if (scenarioParams != null)
            {
                if (string.IsNullOrEmpty(StartGameParams.SavedGameToLoad))
                {
                    PostLoadMap();

                    // StartGamePostLoadMapEventActions(scenarioParams);
                }

            }

            return true;*/
        }

        private bool StartGamePostLoadMapPrepareEventActions()
        {
            switch(startGameMode)
            {
                case StartGameMode.ScenarioNewGame:
                    {
                        StartScenarioParams scenarioParams = this.StartGameParams.StartScenarioParams;
                        List<EventActionType> actions = new List<EventActionType>();
                        if (scenarioParams.Scenario.ScenarioData.Actions != null)
                        {
                            // look up keys and append to list of actions:   
                            foreach (var item in scenarioParams.Scenario.ScenarioData.Actions)
                            {
                                EventActionType action = GameData.Instance.AllEventActionTypes[item];
                                actions.Add(action);
                            }
                        }

                        foreach (var item in scenarioParams.Options)
                        {
                            actions.AddRange(item.Value.GetStartActions());
                        }

                        sortedStartGameEventActions = actions.OrderBy(e => e.DelayInSeconds).ToList();


                        break;
                    }


            }

          /*  StartScenarioParams scenarioParams = this.StartGameParams.StartScenarioParams;
            if (scenarioParams != null)
            {
                if (string.IsNullOrEmpty(StartGameParams.SavedGameToLoad))
                {
                    List<EventActionType> actions = new List<EventActionType>();
                    if (scenarioParams.Scenario.ScenarioData.Actions != null)
                    {
                        // look up keys and append to list of actions:   
                        foreach (var item in scenarioParams.Scenario.ScenarioData.Actions)
                        {
                            EventActionType action = GameData.Instance.AllEventActionTypes[item];
                            actions.Add(action);
                        }
                    }

                    foreach (var item in scenarioParams.Options)
                    {
                        actions.AddRange(item.Value.GetStartActions());
                    }

                    sortedStartGameEventActions = actions.OrderBy(e => e.DelayInSeconds).ToList();
                                       
                }
            }*/

            return true;
        }

        List<EventActionType> sortedStartGameEventActions;
        int startGameEventActionsProgress = 0;

        private bool StartGamePostLoadMapExecuteEvents()
        {
            if (sortedStartGameEventActions != null)
            {
                int maxEvent = startGameEventActionsProgress + 15;
                maxEvent = Common.ClampTop(maxEvent, sortedStartGameEventActions.Count);

                EventActionType eventType;

                //execute scenario startup events:
                for (int i = startGameEventActionsProgress; i < maxEvent; i++)
                {
                    eventType = sortedStartGameEventActions[i];
                    EventAction eventAction = new EventAction(eventType, null);

                    // allow delays???

                    string logMessage = eventAction.Execute();
                }

                if (maxEvent == sortedStartGameEventActions.Count)
                {
                    sortedStartGameEventActions = null;
                    startGameEventActionsProgress = 0;
                    return true;
                }
                else
                {
                    startGameEventActionsProgress = maxEvent;
                    return false;
                }
                /*
                 foreach (var item in sortedStartGameEventActions) // takes 1s
                 {
                     EventAction eventAction = new EventAction(item, null);

                     // allow delays???

                     string logMessage = eventAction.Execute();
                     // startGameActionLog.Add(logMessage);

                 }*/
            }
            else return true;
        }

        private bool StartGamePostLoadMapRegisterEvents()
        {
            switch(startGameMode)
            {
                case StartGameMode.ScenarioNewGame:
                    {
                        StartScenarioParams scenarioParams = this.StartGameParams.StartScenarioParams;
          
                        // register events:
                        scenarioParams.Scenario.RegisterEvents();

                        // register the custom events:
                        foreach (var option in scenarioParams.Options)
                        {
                            if (option.Value.ConditionalEvents != null)
                            {
                                foreach (var item in option.Value.ConditionalEvents)
                                {
                                    PlaySite.EventManager.AddPolledEvent(item);
                                }
                            }
                        }

                        break;
                    }


            }
            /*
            StartScenarioParams scenarioParams = this.StartGameParams.StartScenarioParams;
            if (scenarioParams != null)
            {
                if (string.IsNullOrEmpty(StartGameParams.SavedGameToLoad))
                {
                    // register events:
                    scenarioParams.Scenario.RegisterEvents();

                    // register the custom events:
                    foreach (var option in scenarioParams.Options)
                    {
                        if (option.Value.ConditionalEvents != null)
                        {
                            foreach (var item in option.Value.ConditionalEvents)
                            {
                                PlaySite.EventManager.AddPolledEvent(item);
                            }
                        }
                    }
                }
            }*/

            return true;
        }

        /// <summary>
        /// is timesliced
        /// </summary>
        /// <returns></returns>
        private bool StartGameLoadMapOrSavedGame()
        {
            IsInNormalGameLoop = false;

            switch(startGameMode)
            {
                case StartGameMode.Edit:
                    {
                        StartGameEditorParams editorParams = this.StartGameParams.StartGameEditorParams;
         
                        return The.Map.LoadMapQueued(); //editorParams.MapToLoad);

                    }
                case StartGameMode.DebugNewGame:
                    {
                        StartDebugScenarioParams debugParams = this.StartGameParams.StartDebugScenarioParams;
          
                        return The.Map.LoadMapQueued(); //debugParams.MapKey);                        
                    }
                case StartGameMode.DebugLoadSaved:
                    {
                        // load a saved game in a new thread, also creates a client and calls BeginRun on it:
                        return LoadSavedGame(StartGameParams.SavedGameToLoad);  
                     
                    }
                case StartGameMode.ScenarioNewGame:
                    {
                        StartScenarioParams scenarioParams = this.StartGameParams.StartScenarioParams;
           
                        // create a new game:
                        return The.Map.LoadMapQueued(); //scenarioParams.Scenario.MapKey);
                    }
                case StartGameMode.ScenarioLoadSaved:
                    {
                        // load a saved game:
                        return LoadSavedGame(StartGameParams.SavedGameToLoad); // some IDs have already been assigned even though we come from the titlem screen 
                    }

            }

            return true;

            /*
            StartGameEditorParams editorParams = this.StartGameParams.StartGameEditorParams;
            if (editorParams != null)
            {
                return The.Map.LoadMap(editorParams.MapToLoad);
                               
            }

            StartDebugScenarioParams debugParams = this.StartGameParams.StartDebugScenarioParams;
            if (debugParams != null)
            {
                if (string.IsNullOrEmpty(StartGameParams.SavedGameToLoad))
                {
                    return The.Map.LoadMap(debugParams.MapKey);
                }
                else
                {
                    // load a saved game in a new thread, also creates a client and calls BeginRun on it:
                    return LoadSavedGame(StartGameParams.SavedGameToLoad);  
                }

            }

            StartScenarioParams scenarioParams = this.StartGameParams.StartScenarioParams;
            if (scenarioParams != null)
            {
                if (string.IsNullOrEmpty(StartGameParams.SavedGameToLoad))
                {
                    // create a new game:
                    return The.Map.LoadMap(scenarioParams.Scenario.MapKey);
                }
                else
                {
                    // load a saved game:
                    return LoadSavedGame(StartGameParams.SavedGameToLoad); // some IDs have already been assigned even though we come from the titlem screen 
                }

                // return; // done
            }

            return true;
            */
        }

        /*

        /// <summary>
        /// starts the game, either a scenario or a debug scenario, or a map editor. Also performs a load of Sim data if needed.
        /// </summary>
        private bool StartGame()
        {
            IsInNormalGameLoop = false;


            StartGameEditorParams editorParams = this.StartGameParams.StartGameEditorParams;
            if (editorParams != null)
            {
                Mode = editorParams.EngineMode;

                The.Sim.World = World.CreateFromWorldData(new WorldData() { WorldRadius = GameData.Instance.Constants.DefaultWorldRadius });

                Site site = new Site("editorSite", true){
                    PlayerAllegiance = new Allegiance(AllegianceType.Player, GameData.Instance.AllEntityTypes["entity:human"], "editorAllegiance")         
                };

                if (Mode == EngineMode.Edit)
                {
                    LoadEditorMap(editorParams.MapToLoad); 
                }
                else
                {
                    LoadTestGameMap(editorParams.MapToLoad); 
                }

                return; // done
            }

            StartDebugScenarioParams debugParams = this.StartGameParams.StartDebugScenarioParams; 
            if (debugParams != null)
            {                              
                if (string.IsNullOrEmpty(StartGameParams.SavedGameToLoad))
                {
                    CreateDebugGame(debugParams);
                }
                else
                {
                    // load a saved game:
                    LoadSavedGame(StartGameParams.SavedGameToLoad);  // some IDs have already been assigned even though we come from the titlem screen 
                }

                return; // done
            }

            StartScenarioParams scenarioParams = this.StartGameParams.StartScenarioParams; 
            if (scenarioParams != null)
            {
                if (string.IsNullOrEmpty(StartGameParams.SavedGameToLoad))
                {
                    // create a new game:
                    CreateNewGame(scenarioParams); // takes 4s
                }
                else
                {
                    // load a saved game:
                    LoadSavedGame(StartGameParams.SavedGameToLoad); // some IDs have already been assigned even though we come from the titlem screen 
                }

                return; // done
            }

        }
        */

        /// <summary>
        /// the sequence is:
        /// 1. create site
        /// 2. load map
        /// 3. create allegiances and the rest
        /// </summary>
        /// <param name="scenarioParams"></param>
        /* private void CreateDebugGame(StartDebugScenarioParams debugParams)
         {
             The.Sim.World = World.CreateFromWorldData(new WorldData() { WorldRadius = GameData.Instance.Constants.DefaultWorldRadius });

             Site site = new Site("debugSite", true)
             {
                 Coords = new GeodeticCoordinate(10, 60),
                 Name = "Debug Site"
             };

             LoadMap(debugParams.MapKey); 

             site.PlayerAllegiance = new Allegiance(AllegianceType.Player, GameData.Instance.AllEntityTypes["entity:human"], "debugAllegiance");


             if (debugParams.ScenarioKey != PlaceGameEntities.DebugScenarios.None) 
             {
                 PlaceGameEntities.BeginRun(debugParams.ScenarioKey); // places entities
             }
         }*/


        /// <summary>
        /// the sequence is:
        /// 1. create site
        /// 2. load map
        /// 3. create allegiances and the rest
        /// </summary>
        /// <param name="scenarioParams"></param>
        /*   private void CreateNewGame(StartScenarioParams scenarioParams)
           {
               // set the time - must be done BEFORE the events are registered - TimeCondition needs the date to init correctly
               DateAndTime.ResetTimeOfYearAndTimeOfDay(scenarioParams.Scenario.TimeDateYear);

               ExecuteStartAction(scenarioParams.Scenario.ScenarioData.SpawnWorldAction);
               ExecuteStartAction(scenarioParams.Scenario.ScenarioData.SpawnSiteAction);


               LoadMap(scenarioParams.Scenario.MapKey); // takes 4s

               List<EventActionType> actions = new List<EventActionType>();
               if (scenarioParams.Scenario.ScenarioData.Actions != null)
               {
                   // look up keys and append to list of actions:   
                   foreach (var item in scenarioParams.Scenario.ScenarioData.Actions)
                   {
                        EventActionType action = GameData.Instance.AllEventActionTypes[item];
                       actions.Add(action);
                   }
               }

               foreach (var item in scenarioParams.Options)
               {
                   actions.AddRange(item.Value.GetStartActions());
               }



               // sort by delay and execute immediately???
               IOrderedEnumerable<EventActionType> sortResult = actions.OrderBy(e => e.DelayInSeconds);// .Sort(



               //execute scenario startup events:
               foreach (var item in sortResult) // takes 1s
               {
                   EventAction eventAction = new EventAction(item, null);

                   // allow delays???
                
                   string logMessage = eventAction.Execute();
                  // startGameActionLog.Add(logMessage);
                
               }

            
               // register events:
               scenarioParams.Scenario.RegisterEvents();

               // register the custom events:
               foreach (var option in scenarioParams.Options)
               {
                   if (option.Value.ConditionalEvents != null)
                   {
                       foreach (var item in option.Value.ConditionalEvents)
                       {
                           PlaySite.EventManager.AddPolledEvent(item);
                       }
                   }
               }
           }*/

        private void ExecuteStartAction(string actionKey)
        {
            EventActionType actionType = GameData.Instance.AllEventActionTypes[actionKey];
            EventAction spawnAction = new EventAction(actionType, null);
            string logSpawnActionMessage = spawnAction.Execute();

            //startGameActionLog.Add(logSpawnActionMessage);
        }

        public void AddStartLogMessage(string message)
        {
            startGameActionLog.Add(message);
        }

        public void AddStartPopulationSpawnMessage(string message)
        {
            startGamePopulationSpawnLog.Add(message);
        }

        public List<string> GetStartGameLog()
        {
            return startGameActionLog;
        }

        public List<string> GetStartPopulationSpawnLog()
        {
            return startGamePopulationSpawnLog;
        }


        public void InitializeBioEntityToPlace(PersonSex sex, float? age, UWGame.SimSide.Entities.Biological.AIAgeGroup? ageGroup, Allegiances.Allegiance allegiance, Entity entity)
        {
            entity.BiologicalEntity.CasteType = entity.EntityType.BiologicalType.Castes[sex == PersonSex.Female ? 1 : 0];

            /*  if (raceIndex.HasValue)
              {
                  entity.BiologicalEntity.RaceType = entity.EntityType.BiologicalType.RaceTypes[raceIndex.Value];
              }*/



            // must be set before Initialize:
            // entity.BiologicalEntity.SetRandomEntityStats(age);

            if (allegiance == null && Mode == EngineMode.Game) // don't set allegiances in edit mode.
            {
                allegiance = new Allegiances.Allegiance(Allegiances.AllegianceType.Other, entity.EntityType, null);
            }

            // must be set before Initialize:          
            entity.BiologicalEntity.SetAgePreInit(age, ageGroup);

            entity.Initialize(PlaySite, allegiance);
            entity.InitializeModelAndOnScreenFunctionality();

            //   entity.BiologicalEntity.SetRandomEntityStats();

        }



        public void ExploreShroud(TilePos from, TilePos to, int startRadiusInTiles, int endRadiusInTiles, Entity byEntity)
        {
            WorldLocation fromLocation = MapManager.TilePosToWorldLocation(from);
            WorldLocation toLocation = MapManager.TilePosToWorldLocation(to);

            float startRadius = startRadiusInTiles * MapManager.tileSize;
            float endRadius = endRadiusInTiles * MapManager.tileSize;


            ExploreShroud(fromLocation, toLocation, startRadius, endRadius, byEntity, DetectMode.RollToDetectHiddenEntities);
        }

        /// <summary>
        /// explore the whole map - does not detect entities?!?!
        /// </summary>
        /// <param name="location"></param>
        /// <param name="toLocation"></param>
        /// <param name="radiusInPixels"></param>
        /// <param name="endRadiusInPixels"></param>
        /// <param name="byEntity"></param>
        public void ExploreShroud(Entity byEntity, DetectMode detectMode)
        {
            TerrainTile tile;
            TerrainTile[] column;
            TerrainTile[][] map = The.Map.TileMap;

            Sensor sensor = null;
            SharedKnowledge sharedKnowledge = null;
            if (byEntity != null)
            {
                byEntity.Find(out sensor);
                sharedKnowledge = byEntity.Intelligence.Allegiance.SharedKnowledge;
            }

            List<IDetectable> allDetectables = new List<IDetectable>();

            for (int x = 0; x < The.Map.mapTileWidth; x++)
            {
                column = map[x];
                for (int y = 0; y < The.Map.mapTileHeight; y++)
                {
                    tile = column[y];

                    ExploreTile(byEntity, sharedKnowledge, detectMode, sensor, tile, allDetectables);
                }
            }
        }

        /// <summary>
        /// explore the shroud along a line between two points, varying the radius in between
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="startRadius"></param>
        /// <param name="endRadius"></param>
        public void ExploreShroud(WorldLocation fromLocation, WorldLocation toLocation, float startRadiusInPixels, float endRadiusInPixels, Entity byEntity, DetectMode rollToDetect)
        {
            // WorldLocation fromLocation = MapManager.TilePosToWorldLocation(from);
            // WorldLocation toLocation = MapManager.TilePosToWorldLocation(to);

            int startRadius = (int)(startRadiusInPixels / (float)MapManager.tileSize);
            int endRadius = (int)(endRadiusInPixels / (float)MapManager.tileSize);

            Vector2 dir = (toLocation - fromLocation).ToVector3().ToVector2();
            float length = dir.Length();
            dir.Normalize();

            float travelled = 0f;

            float stepSize = 30f;

            WorldLocation currentSpot = fromLocation;


            //   float startRadiusInTiles = startRadius * MapManager.tileSize;
            //  float endRadiusInTiles = endRadius * MapManager.tileSize;

            // int radiusInTiles;
            int radiusInTiles;



            while (travelled < length)
            {
                currentSpot = new WorldLocation(fromLocation.X + travelled * dir.X, fromLocation.Y + travelled * dir.Y, 0f);

                radiusInTiles = (int)MathHelper.Lerp(startRadius, endRadius, travelled / length);

                ExploreCircularSpot(byEntity, currentSpot, rollToDetect, radiusInTiles);

                travelled += stepSize;
            }

        }

        public void ExploreCircularSpot(Entity byEntity, WorldLocation location, DetectMode rollToDetect, float radiusInPixels)
        {
            int startRadius = (int)(radiusInPixels / (float)MapManager.tileSize);

            The.Sim.ExploreCircularSpot(byEntity, location, rollToDetect, startRadius);

        }

        public void ExploreCircularSpot(Entity byEntity, WorldLocation location, DetectMode rollToDetect, int radiusInTiles)
        {
            Sensor sensor;
            if (byEntity.Find(out sensor))
            {

            }

            TerrainTile[][] map = The.Map.TileMap;
            int width = Common.GetJaggedArrayWidth(map);
            int height = Common.GetJaggedArrayHeight(map);


            TerrainTile[] column;
            TilePos currentTile = MapManager.WorldPosToTilePos(location);

            int minX, minY, maxX, maxY;
            TerrainTile tile;

            List<IDetectable> allDetectables = new List<IDetectable>();


            minX = Math.Max(0, currentTile.X - radiusInTiles);
            minY = Math.Max(0, currentTile.Y - radiusInTiles);

            maxX = Math.Min(width - 1, currentTile.X + radiusInTiles);
            maxY = Math.Min(height - 1, currentTile.Y + radiusInTiles);

            SharedKnowledge sharedKnowledge = null;

            if (byEntity != null)
            {
                sharedKnowledge = byEntity.Intelligence.Allegiance.SharedKnowledge;
            }

            for (int x = minX; x <= maxX; x++)
            {
                column = map[x];
                for (int y = minY; y <= maxY; y++)
                {
                    if (Common.DistanceOctile(currentTile, new TilePos(x, y)) <= radiusInTiles)
                    {
                        tile = column[y];
                        ExploreTile(byEntity, sharedKnowledge, rollToDetect, sensor, tile, allDetectables);
                    }
                }
            }

        }

        private static void ExploreTile(Entity byEntity, SharedKnowledge sharedKnowledge, DetectMode detectMode, Sensor sensor, TerrainTile tile, List<IDetectable> allDetectables)
        {
            if (tile.HasEverBeenSeenByPlayer == false)
            {
                tile.HasEverBeenSeenByPlayer = true;

                if (detectMode != DetectMode.NoEntityDetection && sensor != null)
                {
                    allDetectables.Clear();
                    tile.GetDetectablesOnTile(allDetectables);

                    if (allDetectables.Count > 0)
                    {
                        bool rollTodetectHiddenEntities = detectMode == DetectMode.RollToDetectHiddenEntities;

                        sensor.RollToDetect(byEntity, sharedKnowledge, allDetectables, true, true, unseeAfterDetecting: true, rollToDetectHiddenEntities: rollTodetectHiddenEntities, doAssert: false);
                    }
                }
            }
        }



        /*private Entity GetRandomPerson()
        {
            return Site.Persons[ScreenManager.Random.Next(Site.Persons.Count)];
        }*/





        /// <summary>
        /// AFTER Init()
        /// Load your graphics content.  If loadAllContent is true, you should
        /// load content from both ResourceManagementMode pools.  Otherwise, just
        /// load ResourceManagementMode.Manual content.
        /// </summary>
        public override void LoadContent()
        {

        }


        public enum WaitingFor { Regions, Path }
        public void AddWaitingAgent(Entity entity, WaitingFor waitingFor)
        {

#if DEBUG || PROFILE
            WaitingAgents[entity.ID] = new Tuple<WaitingFor, double>(waitingFor, TotalUnPausedGameTimeInSeconds);

#endif

        }



        /*   private void LoadStructureBlockedData(StructureType currentStructure, UtilityMap state, byte[] contentDiscomfortValues)
           {
               int subTileHeight = currentStructure.HeightInTiles * 3;
               int subTileWidth = currentStructure.WidthInTiles * 3;

               currentStructure.TileLayoutType.BlockedEdges.Add(state, new byte[subTileWidth, subTileHeight]);


               byte[,] blockedEdges = currentStructure.TileLayoutType.BlockedEdges[state];
               for (int x = 0; x < currentStructure.WidthInTiles * 3; x++)
               {
                   for (int y = 0; y < currentStructure.HeightInTiles * 3; y++)
                   {
                       //invert y:
                       blockedEdges[x, subTileHeight - y - 1] = contentDiscomfortValues[x + subTileWidth * y];
                       // currentStructure.BlockedEdges[x, subTileHeight - y - 1] = discomfortValues[x + subTileWidth * y]; 
                   }
               }

               byte[,] discomfortValues = new byte[currentStructure.WidthInTiles, currentStructure.HeightInTiles];
               currentStructure.TileLayoutType.DiscomfortValues.Add(state, discomfortValues);

               byte cost;
               for (int y = 0; y < currentStructure.HeightInTiles; y++)
               {
                   for (int x = 0; x < currentStructure.WidthInTiles; x++)
                   {
                       // sample the center of each 9-group of subtiles:
                       cost = blockedEdges[x * 3 + 1, y * 3 + 1];
                       if (cost < 255 && cost > 0)
                       {
                           // scale the cost to within reasonable levels defined in AIConstants:
                           cost = (byte)MathHelper.Lerp((float)AIConstants.HighestDiscomfortLevelForLeisureActivityToStart,
                                                   (float)AIConstants.HighestDiscomfortLevelForLeisureActivityToContinue, ((float)cost) / 255f);
                           discomfortValues[x, y] = cost; // discomfortValues[x, currentStructure.HeightInTiles - 1 - y] = cost; // invert y!
                       }
                       else if (cost == 255) // the center is a blocked subtile... look around at the other subtiles for a proper discomfort value:
                       {
                           discomfortValues[x, y] = GetAnyUnblockedSubtileCost(x, y, blockedEdges);
                       }
                       else
                       {
                           discomfortValues[x, y] = 0; // discomfortValues[x, currentStructure.HeightInTiles - 1 - y] = 0; // invert y!
                       }
                   }
               }
           }*/

        /// <summary>
        /// gets set true after the first Update()
        /// </summary>
        public bool IsInNormalGameLoop = false;

        private void CoordsTest()
        {
            /*   GeodeticCoordinate from = new GeodeticCoordinate(50, 40);
         GeodeticCoordinate to = new GeodeticCoordinate(60, 40);
         */
            GeodeticCoordinate from = new GeodeticCoordinate(50, 0); // lat 0: equator, 90: north pole
            GeodeticCoordinate to = new GeodeticCoordinate(60, 0);

            double bearing = DistanceCalculator.GetBearing(from, to);

            double pis = bearing / Math.PI;
        }

        private void InitEntityUpdater()
        {
            entities = new SleepyUpdater<Entity>(UWGame.SimSide.Systems.Module.Sim, true);
        }

        public void AddEntity(Entity entity)
        {
            entities.Add(entity);
        }

        public void RemoveEntity(Entity entity)
        {
            entities.Remove(entity);

        }


        public void AdvanceTime(double seconds)
        {
            TotalUnPausedGameTime = TotalUnPausedGameTime.Add(new TimeSpan(0, 0, 0, 0, (int)(seconds * 1000)));

            DateAndTime.AdvanceTime(seconds);

            foreach (var item in PlaySite.Entities.GetAsList())
            {
                item.CreateBioSystemsRegulator(); // avoid starving/dying
            }

        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {

            if (GameTime == null)
            {
                gameTime = new GameTime(new TimeSpan((long)(GameSpeed * gameTime.TotalGameTime.Ticks)), new TimeSpan((long)(GameSpeed * gameTime.ElapsedGameTime.Ticks)));
            }
            else
            {
                TimeSpan elapsedTime = new TimeSpan((long)(GameSpeed * gameTime.ElapsedGameTime.Ticks));
                TimeSpan totalTime = GameTime.TotalGameTime;
                totalTime = totalTime.Add(elapsedTime);

                gameTime = new GameTime(totalTime, elapsedTime);
            }



            //This is a hacky way to make Sim update, even when the
            //Client is sonsidered in focuse and in front
            //there is similar code in Client.Update()
            base.Update(gameTime, false, false);


            if (IsActive && The.LoadScreen.IsLoadFinished)
            {

                // Thread.Sleep(20);
               // System.Diagnostics.Trace.WriteLine("Sim.Update #1 " + TotalUnPausedGameTimeInSeconds);


                //******* IMPORTANT - about GameTime **********************
                // GameTime.ElapsedGameTime - time passed since last XNA Update
                // always OK to use.

                // GameTime.TotalGameTime - total time passed since the start of the game
                // BEWARE - this includes the time where the player paused the game. So for many simulation tasks this is wrong to use.
                // use the global variable TotalUnPausedGameTime instead! This number is not increased when the game is paused.

                IsInNormalGameLoop = true; // create move maps immediately for new allegiances

                // use this for interface animations which never pause
                this.GameTime = gameTime;

                if (!IsPaused)
                {

                    // use this in regulators, so they don't count the time the game was paused.
                    TotalUnPausedGameTime = TotalUnPausedGameTime.Add(new TimeSpan(gameTime.ElapsedGameTime.Ticks));

                    // let's cache this conversion:
                   // TotalUnPausedGameTimeInSeconds = TotalUnPausedGameTime.TotalSeconds;

                    System.Diagnostics.Trace.WriteLine("Sim.Update #2 " + TotalUnPausedGameTimeInSeconds);


                    if (Mode == EngineMode.Game)
                    {
                        The.Client.MarkPerformanceTime("before sim update", Color.Yellow);

                        // I think it's OK to use the ordinary game time here, because we only use the elapsed time since the last update...
                       // DateAndTime.UpdateDayAndYear(gameTime);
                        DateAndTime.Update(gameTime);


                        //  The.Client.MarkPerformanceTime("Entities.Update", Color.Blue);


                        TriggerSystem.Update(gameTime);

                        this.World.Update(gameTime);

                        The.Client.MarkPerformanceTime("World.Update", Color.Blue);

                        if (entities.GetItem(e => e.EntityID == (EntityID)14180) != null)
                        {

                        } 

                        entities.Update(gameTime);

                        The.Client.MarkPerformanceTime("Entities.Update", Color.Beige);

                        //    Site.Update(gameTime);


                        // call this from the Parent objects instead?
                        LookUp<EntityGroup, EntityGroupID>.IterateMembers(
                            g =>
                            {
                                g.Update(gameTime);        // calls OtherManager.Update, HaulingJobManager.Update                         
                            });



                        The.Client.MarkPerformanceTime("HaulingJobManager.Update", Color.Purple);

                        // may cause game end, so we place it last (The.Map = null etc.):
                        CycleManager.Update();

                        //Possible that The.Sim = null etc.
                        if (IsGameOver)
                        {
                            IsInNormalGameLoop = true;
                            return;
                        }
                        else
                        {
                            The.Client.MarkPerformanceTime("CycleManager.Update", Color.Red);

                            CheckPerformance();
                        }
                    }


                    The.Map.Update(gameTime);

                }
                else
                {
                   
                   // Thread.Sleep(50); // saves on CPU?
                    
                }
            }
        }


        private void CheckPerformance()
        {
            if (performanceRegulator.IsReady())
            {
                if (WaitingAgents.Count >= GameData.Instance.Constants.NumberOfAgentsWatingToTriggerAlert)
                {
                    double averageWaitingTime = The.Sim.WaitingAgents.Average(a => TotalUnPausedGameTimeInSeconds - a.Value.Item2);

                    if (averageWaitingTime > GameData.Instance.Constants.AverageAgentWatingTimeToTriggerAlert)
                    {
                        if (Controller.Options.ShowPerformanceWarning)
                        {
                            The.Client.AddLogEvent(The.Client.Log.GeneralEvent, null,
                                 string.Format("Warning: Slowdown detected. Average agent waiting time: {0:N1}s", averageWaitingTime), ClientSide.Log.Priority.High); 

                        }
                    }
                }
            }

        }



        public Entity GetRepresentativeEntity()
        {
            Entity entity;
            if (PlaySite.PlayerAllegiance.Persons.Count != 0)
            {
                entity = PlaySite.PlayerAllegiance.Persons.FirstOrDefault(p => p.IsOnPlaySite());
                //entity = PlaySite.PlayerAllegiance.Persons[0];
            }
            else
            {
                List<PointTreeDweller<Entity>> allAgents = null;
                The.AgentQuadTree.GetAllObjects(ref allAgents);
                if (allAgents != null && allAgents.Count > 0)
                {
                    entity = allAgents[0].ObjectAndPosition.First;
                }
                else
                {
                    entity = null;
                }
            }
            return entity;
        }


        /* private void ProduceEnergy(GameTime elapsed)
         {
             double total = 0f;
             foreach (EntityID id in EnergyProducers)
             {
                 Entity ep = Entity.FindByID(id);
                 if (ep == null)
                     continue;

               
                 Power power;
                 ep.Find(out power);
                 total += elapsed.ElapsedGameTime.TotalSeconds * power.PowerOutput;
                 // }
             }

             EnergyProductionLastCycle = total;

             AvailableEnergy += total;
         }*/

        /*
        // TODO
        private double AssignEnergyToProducers(double totalEnergyNeeded)
        {
            double energySpent = 0;
            bool energyShortfall = totalEnergyNeeded > AvailableEnergy;

            foreach (KeyValuePair<ProcessJob, List<AI.Goals.GoalDoProduceAtomic>> kvp in ProductionJobsRequiringEnergy)
            {
                AI.Goals.GoalDoProduceAtomic producer;
                double currentEnergySpent;
                for (int i = 0; i < kvp.Value.Count; i++)
                {
                    producer = kvp.Value[i];

                    if (energySpent < AvailableEnergy)
                    {

                        if (producer.Produce(
                            (energyShortfall ? AvailableEnergy * (producer.NeededEnergy / totalEnergyNeeded) : producer.NeededEnergy), out currentEnergySpent))
                        {
                            energySpent += currentEnergySpent;
                        }
                    }
                    else
                    {
                        // produce with zero energy to complete other production tasks...
                        producer.Produce(0, out currentEnergySpent);
                    }
                }

            }

            return energySpent;
        }
        */

        /// <summary>
        /// // TODO
        /// Do production with the available energy.
        /// Add more energy requiring tasks here (recharging etc).
        /// </summary>
        /*  private void Produce()
          {
              // calculate energy needs from production goals
              double totalEnergyNeeded = 0;
              foreach (KeyValuePair<ProcessJob, List<AI.Goals.GoalDoProduceAtomic>> kvp in ProductionJobsRequiringEnergy)
              {
                  for (int i = 0; i < kvp.Value.Count; i++)
                  {
                      totalEnergyNeeded += kvp.Value[i].NeededEnergy;
                  }

              }
              // Add more energy requiring tasks here (recharging etc).

              // store the number for estimators:
              TotalEnergyNeedsLastCycle = totalEnergyNeeded;

              // if there is a shortfall, allocate available energy proportionally to needs:
              AvailableEnergy -= AssignEnergyToProducers(totalEnergyNeeded);

              // TotalEnergyNeeded = 0;
              ProductionJobsRequiringEnergy.Clear();

          }*/



        /// <summary>
        /// The Sim should NEVER EVER EVER draw, so this fn should be empty.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            if (!The.LoadScreen.IsLoadFinished)
                return;

        }





    } // end class Sim



}



