#region File Description
//-----------------------------------------------------------------------------
// ScreenManager.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using UWGame.SimSide;
using UWGame;
using UWGame.Client.Audio;
using UWGame.SimSide.AllGameData;
using UWGame.ClientSide.Screens;
 
using GameStateManagement;

using UWGame.Control.Replays;
using UWGame.Control.Input;
using UWGame.ClientSide;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework.Input;
using InputEventSystem;
using UWGame.Control.Commands;
using UWGame.SimSide.Commands;
using UWGame.SimSide.Scenarios;
using UWGame.SimSide.Snapshots;
using Steamworks;
using System.Runtime;
using UWGame.Steam;
#endregion

namespace UWGame.Control // DONT change this namespace to Controller - VS cannot handle class names and namespaces that are the same...
{
    /// <summary>
    /// handles input to both Sim and Client - controls everything on the menu screens too - this object never dies.
    /// </summary>
    public class Controller : DrawableGameComponent
    {
        #region Fields

        List<GameScreen> screens = new List<GameScreen>();
        List<GameScreen> screensToUpdate = new List<GameScreen>();

        //InputState input = new InputState();
        private InputManager inputManager;

        IGraphicsDeviceService graphicsDeviceService;

        //NormalMouseStateWrapper normalMouseStateWrapper = new NormalMouseStateWrapper();

        ContentManager content;
        SpriteBatch spriteBatch;
        SpriteFont font;
        Texture2D blankTexture;

        //public Random Random = new Random();

        /// <summary>
        /// should not be accessible outside Controller
        /// </summary>
        private Replayer replayer;
        private Recorder recorder;
        private string replayDisplayedTime;

        private bool skipRendering = false;

        bool traceEnabled;
        bool replayPaused = false;


        public float ActiveZoomFactor
        {
            get;
            private set;
        }

        private CommandInvoker CommandInvoker;


        /// <summary>
        /// dra to this instead of to the back buffer when zoom is required. 
        /// As the last step, draw this with a scale factor into the back buffer
        /// </summary>
        public RenderTarget2D ZoomRenderTarget;
      

        #endregion



        #region Properties

        // We don't use a depth buffer at all...?
        // XNA 3
        //public DepthStencilBuffer NoMultiSamplingStencilBuffer;
        //public DepthStencilBuffer MultiSamplingStencilBuffer;

      //  public GameData GameData;

        public enum GraphicsLevel { Low, High } // handle texture detail/size and no. of render targets
     //   public int PixelShaderVersion;
        public GraphicsLevel GraphicsLevelSetting = GraphicsLevel.High; //GraphicsLevelSetting = GraphicsLevel.Low; // GraphicsLevel.Low;

        /// <summary>
        /// only for controlling replays:
        /// </summary>
        private InputData replayerInputData;

        public InputData InputData
        {
            get
            {
                return inputManager.InputData;
            }
        }

        private bool contentIsLoaded = false;

        /// <summary>
        /// use this for playing music everywhere. 
        /// also sound outside the game, like on menu screens?
        /// 
        /// since this survives save/load, it must be cleansed of events etc. that may hang on the renderables/entities and cause a memory leak.
        /// </summary>
        public AudioManager AudioManager;

        /// <summary>
        /// user client settings (music volume etc) used on both menus and in-game
        /// </summary>
      //  public UserSettings UserSettings;

        /// <summary>
        /// client options, some of which can be set/saved via UI
        /// </summary>
        public Options Options;

        /// <summary>
        /// contains sim and client stuff...
        /// </summary>
        public Progress Progress;

        /// <summary>
        /// Only for use on the menus etc. In-game, use either Sim or Client generators.
        /// </summary>
        public RandomGenerator RandomGenerator;


        public SteamManager SteamManager;
        public StatsAndAchievements StatsAndAchievements;

        /// <summary>
        /// Expose access to our Game instance (this is protected in the
        /// default GameComponent, but we want to make it public).
        /// </summary>
        new public UnclaimedWorld Game
        {
            get { return (UnclaimedWorld)base.Game; }
        }


        /// <summary>
        /// Expose access to our graphics device (this is protected in the
        /// default DrawableGameComponent, but we want to make it public).
        /// </summary>
        new public GraphicsDevice GraphicsDevice
        {
            get { return base.GraphicsDevice; }
        }

        /// <summary>
        /// this area may be smaller than the screen/window size, and will be scaled up to achieve a zoom effect
        /// </summary>
        public Dimension DrawArea // Point DrawArea
        {
            get;
            private set;
        }

       
        

        /// <summary>
        /// Lars: It confuses me that there are so many content managers... in Game, ScreenManager and Client...
        /// Can't we find out what is the responsibility of each...?
        /// 
        /// A content manager used to load data that is shared between multiple
        /// screens. This is never unloaded, so if a screen requires a large amount
        /// of temporary data, it should create a local content manager instead.
        /// </summary>
        public ContentManager Content
        {
            get { return content; }
        }


        /// <summary>
        /// A default SpriteBatch shared by all the screens. This saves
        /// each screen having to bother creating their own local instance.
        /// </summary>
        public SpriteBatch SpriteBatch
        {
            get { return spriteBatch; }
        }


        

        /// <summary>
        /// A default font shared by all the screens. This saves
        /// each screen having to bother loading their own local copy.
        /// </summary>
        public SpriteFont Font
        {
            get { return font; }
        }


        /// <summary>
        /// If true, the manager prints out a list of all the screens
        /// each time it is updated. This can be useful for making sure
        /// everything is being added and removed at the right times.
        /// </summary>
        public bool ScreenTraceEnabled
        {
            get { return traceEnabled; }
            set { traceEnabled = value; }
        }


        #endregion

        #region Initialization
        UnclaimedWorld game;
        /// <summary>
        /// Constructs a new screen manager component.
        /// </summary>
        public Controller(UnclaimedWorld game)//Game game)
            : base(game)
        {
            this.game = game;
            content = new ContentManager(game.Services);
            content.RootDirectory = "Content";

            graphicsDeviceService = (IGraphicsDeviceService)game.Services.GetService(typeof(IGraphicsDeviceService));

            AudioManager = new AudioManager(null, true); // Only use this manager to play music since it survives snapshots and this will sort of compensate for no cueing ability

            RandomGenerator = new RandomGenerator( RandomGenerator.GeneratorType.Client);

            if (graphicsDeviceService == null)
                throw new InvalidOperationException("No graphics device service.");

            //don't use GraphicsDevice here, it gives this error: "the graphics device cannot be used before initialize has been called"
            inputManager = new InputManager(this);

            replayer = new Replayer(inputManager, this, game);
            replayer.Initialize();

            inputManager.Initialize(replayer);

            recorder = new Recorder(this);
            recorder.Initialize();

         

            replayerInputData = new InputEventSystem.InputData(null);

            The.Snapshotter = new Snapshotter();
            CommandInvoker = new Commands.CommandInvoker();

            this.SteamManager = new Steam.SteamManager();
            this.StatsAndAchievements = new Steam.StatsAndAchievements(this);

            SteamManager.Initialize();
            StatsAndAchievements.Initialize();

            //int resolutionWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width; // DPI TEST
         

            LoadOptionSettings();

            LoadProgress();

            recorder.RecordDuringPlay = Options.RecordGame;
#if DEBUG
            recorder.RecordDuringPlay = true;
#endif
          
        }


        public void UpdateNewStateFromInputDevices()
        {
            inputManager.UpdateNewStateFromInputDevices();
        }

        /// <summary>
        /// we need the zoom globally, not just in-game
        /// </summary>
        public virtual void SetZoomRenderTaget()
        {
            if (ZoomIsActive())
            {
                GraphicsDevice.SetRenderTarget(ZoomRenderTarget);
            }
            else
            {
                GraphicsDevice.SetRenderTarget(null);
            }
        }

        /// <summary>
        /// necessary to call this to clean up between Start game, since Controller and InputData live on
        /// </summary>
        public void EndGameSession()
        {
           
        }


        public void ValidateDrawAreaWidth(int minWidth)
        {
            if (DrawArea.Width < minWidth)
            {
                throw new Exception("Not enough drawing space. Increase the screen resolution, or reduce the zoom factor.");
            }
        }

        public void Destroy()
        {
            SteamManager.Destroy();

            if (ZoomRenderTarget != null)
            {
                ZoomRenderTarget.Dispose();
            }
        }

        /// <summary>
        /// Load your graphics content.
        /// </summary>
       // protected override void LoadGraphicsContent(bool loadAllContent) // XNA 3
        protected override void LoadContent()
        {
            // Load content belonging to the screen manager.
            /* if (loadAllContent)
             {*/

            spriteBatch = new SpriteBatch(GraphicsDevice);
            //font = content.Load<SpriteFont>("Content/Arial"); // content.Load<SpriteFont>("Content/menufont");
            font = content.Load<SpriteFont>(WindowSystem.GUIManager.LCDandHUDBodyFontPath); // content.Load<SpriteFont>("Content/menufont");
            //MLo: changed WindowManager font to match the InGameUI, to match font style before Client gets constructed

            blankTexture = content.Load<Texture2D>("MainMenu/blank");
            
        

            // set a flag to make it ok to call LoadContent on newly added screens:
            contentIsLoaded = true;

            if (game.GraphicsDeviceManager.IsFullScreen == false)
            {
                var form = (System.Windows.Forms.Form)System.Windows.Forms.Control.FromHandle(game.Window.Handle);
                form.Location = new System.Drawing.Point(10, 10);
            }


            PresentationParameters pp = GraphicsDevice.PresentationParameters;

            if (ZoomIsActive())
            {
                int width = (int)(pp.BackBufferWidth / ActiveZoomFactor);
                int height = (int)(pp.BackBufferHeight / ActiveZoomFactor);

                DrawArea = new Dimension(width, height);

                // no depth buffer, no multisampling!
                ZoomRenderTarget = new RenderTarget2D(GraphicsDevice,
                    width, height, false, // TODO: should have smaller dimensions than the screen
                    pp.BackBufferFormat, DepthFormat.None, 0, RenderTargetUsage.PreserveContents); // preserve .DiscardContents);
            }
            else
            {
                DrawArea = new Dimension(pp.BackBufferWidth, pp.BackBufferHeight);
            }


            // Tell each of the screens to load their content.
            foreach (GameScreen screen in screens)
            {
                screen.LoadContent();
            }

            The.IngameLoadScreen.LoadContent();
            
        }

      /*  private void LoadUserSettings()
        {
            if (BaseDataLoader.CurrentSerializeMode != UWGame.SimSide.AllGameData.BaseDataLoader.SerializeMode.Read)
            {
                UserSettings = new UserSettings();
            }

            // TODO: save in documents folder
            BaseDataLoader.SerializeAndDeserializeObject(ref UserSettings, "", "userSettings.xml", Config.DataType.BaseData);

           
        }*/

        private void LoadProgress()
        {           
            string path = Config.GetDataFolderPath(Config.DataType.UserSettings, "", Progress.FileName);

            if (System.IO.File.Exists(path))
            {

                try
                {
                    BaseDataLoader.DeserializeObject("", Progress.FileName, out Progress, Config.DataType.UserSettings);
                }
                catch(IOException)
                {
                    Progress = new Progress();
                }
                    
               // Progress.Write();    // write the file to keep up to date with new settings
                
            }
            else // write the file if it does not exist:
            {
                CreateProgressFile();
            }

        }


        private void LoadOptionSettings()
        {
           // string path = Options.GetPathToFile();
            string path = Config.GetDataFolderPath(Config.DataType.UserSettings, "", Options.FileName);

            if (System.IO.File.Exists(path))
            {
               
                BaseDataLoader.DeserializeObject("", Options.FileName, out Options, Config.DataType.UserSettings);

                //****** PREVENT CRASHES BECAUSE OF INVALID DATA:
                bool optionsAreValid = Options.Validate();

                Debug.Assert(optionsAreValid);

                if (!optionsAreValid)
                {
                    // as a fallback for the invalid options file, use an instance instead:
                    CreateOptionsFileWithDefaultSettings();      
                    // delete this code when the error is hopefully solved...
                }
                //********
                else
                {

                    Options.Write();    // write the file to keep up to date with new settings
                }
            }
            else // write the file if it does not exist:
            {
                CreateOptionsFileWithDefaultSettings();          
            }

            ActiveZoomFactor = Options.ZoomFactor;
            ActiveZoomFactor = Common.ClampBottom(ActiveZoomFactor, 1f);

            AudioManager.Init(Options, false);
            
        }

        private void CreateOptionsFileWithDefaultSettings()
        {
            Options = new Options(); // default settings                

          //  Options.SetDefaultResolution();

            Options.Write();
        }

        private void CreateProgressFile()
        {
            Progress = new Progress();

            Progress.Write();
        }

        /// <summary>
        /// Unload your graphics content.
        /// </summary>
        //protected override void UnloadGraphicsContent(bool unloadAllContent) // XNA 3
        protected override void UnloadContent()
        {
            // only gets called when the app closes, if ever...


            // Unload content belonging to the screen manager.
            /*if (unloadAllContent)
            {*/
                content.Unload();
           // }

            // Tell each of the screens to unload their content.
            foreach (GameScreen screen in screens)
            {
                screen.UnloadContent();
            }


        }


        #endregion
      
        #region Update and Draw
        public void DummyEventHandler(object caller, EventArgs e)
        {
            //do nothing
        }

        public void PauseReplay()
        {
            replayPaused = true;
        }

        public ReplayVerificationData RetrieveVerificationData()
        {
            Vector3 representativeEntityLocation = new Vector3(0.0f, 0.0f, 0.0f);
            Vector2 mapWindowLocation;
            if (The.Sim != null)
            {
                Entity representativeAgent = The.Sim.GetRepresentativeEntity();
                if (representativeAgent != null)
                {
                    representativeEntityLocation = representativeAgent.PlaySiteLocation;
                }

                mapWindowLocation = The.MapUI.MapWindowWorldPosition;
                
            }
            else
            {
                /*representativeEntityLocation.X = 0.0f; All values should be zero by default
                representativeEntityLocation.Y = 0.0f;*/

                mapWindowLocation.X = 0.0f;
                mapWindowLocation.Y = 0.0f;
            }

            return new Replays.ReplayVerificationData()
            {
                MapWindowLocation = mapWindowLocation,
                RepresentativeEntityLocation = representativeEntityLocation
            };
            
        }

        public void Cleanup()
        {
            // we want to get rid of event delegates...
            inputManager.Reset();
           
        }


        private void VerifySimAndRecordedData()
        {
            if (replayer.IsPlaying)
            {
                ReplayVerificationData currentSimData = RetrieveVerificationData();
                ReplayVerificationData recordedData = replayer.CurrentReplay.GetCurrentFrame().RecordedVerificationData;
            
                bool diverged = false;
                if (!currentSimData.Verify(recordedData, replayer.Mode))
                {                   
                    diverged = true;
                }

#if DEBUG
                Console.WriteLine(replayer.CurrentReplay.currentFrameIndex + " SIM:" + currentSimData.RepresentativeEntityLocation.X.ToString() + " REC:" + recordedData.RepresentativeEntityLocation.X.ToString() + (diverged? "DIV" : ""));
#endif    

                if (diverged)
                {
                    throw new Exception("Sim and recorded data have diverged.");
                }
            }
        }

        public void SaveOrVerifyEntityAIState(string entityAIState)
        {
             #if !RELEASE

                if (recorder.isRecording)
                {
                    recorder.SaveEntityAIState(entityAIState + recorder.currentFrameIndex);
                }
                
                if(replayer.IsPlaying)
                {
                    entityAIState += replayer.CurrentReplay.currentFrameIndex; // +1;
                    string savedEntityAIState = replayer.GetCurrentSavedAIState() ;
                    if (savedEntityAIState != null)
                    {
                        if (savedEntityAIState != entityAIState)
                        {
                            throw new Exception("Replay out of sync!");
                        }
                    }
                }
                  
            #endif
        }
        
        public void SaveOrVerifyRandomGet(string getMessage)
        {
            #if !RELEASE
                if (recorder.isRecording)
                {
                    recorder.SaveRandomGet(getMessage);
                }

                if (replayer.IsPlaying)
                {
                    string savedGetMessage = replayer.GetCurrentSavedRandomGet();

                    if (savedGetMessage != null && getMessage != null)
                    {
                        if (savedGetMessage != getMessage)
                        {
                            throw new Exception("Replay out of sync!");
                        }
                    }
                }
                
            #endif
        }


        /// <summary>
        /// maybe place logic in CommandInvoker
        /// 
        /// see if we can give CommandInvoker knowledge without references to Replayer and Replayer (Source enum?)
        /// </summary>
        /// <param name="command"></param>
        public void StoreAndExecuteCommand(Command command)
        {
            if (recorder.isRecording)
            {
                recorder.RecordCommand(command);
            }

            CommandInvoker.Store(command);

            if (replayer.IsPlaying == false ||
                (replayer.Mode == Replayer.ReplayingMode.InterfaceMode))
            {
                CommandInvoker.Execute(command);// during replaying we only execute stored commands
            }
        }

        public bool SkipRendering()
        {
            //Skipping rendering Isn´t safe right now, testing shows that it creates diversions
            //Minimizing seems to create the same problem since it makes the game automatically skip rendering
            //The rendering should not be connected to the results we get in the sim in any way, so this is something that needs to be solved long term

            /*if(replayer.IsPlaying == true)
            {
                KeyboardState inputState = Keyboard.GetState();
                if (inputState.IsKeyDown(Keys.F))
                {
                    skipRendering = !skipRendering;
                }
                return skipRendering;
            }*/
            return false;
        }

        /// <summary>
        /// Allows each screen to run logic.
        /// </summary>
        public override void Update(GameTime gameTime)
        {

            if (System.Threading.Monitor.TryEnter(UpdateScreensLock)) // while loading (Sim = null), Update/Draw is skipped instead of blocked so the app remains responsive to Windows
            {
                try
                {

                    AudioManager.Update(gameTime);


                    HandleReplay(ref gameTime); // during replay, this will substitute the gameTime (frame duration) with the recorded value.

                    if (replayPaused)
                        return;

                    inputManager.Update(gameTime, Game.IsActive); // invokes Commands during replay in Interface Mode 

                    replayer.Update(); // invokes Commands during replay in Command Mode. when we replay a pause command the Sim has already computed for the next frame??



                    // Make a copy of the master screen list, to avoid confusion if
                    // the process of updating one screen adds or removes others.
                    screensToUpdate.Clear();
                    
                    foreach (GameScreen screen in screens)
                        screensToUpdate.Add(screen);
                   

                    // threadLocked = false;

                    bool otherScreenHasFocus = false;
                    bool coveredByOtherScreen = false;

                    // Loop as long as there are screens waiting to be updated.
                    while (screensToUpdate.Count > 0)
                    {
                        // Pop the topmost screen off the waiting list.
                        GameScreen screen = screensToUpdate[screensToUpdate.Count - 1];

                        screensToUpdate.RemoveAt(screensToUpdate.Count - 1);

                        // if the game ends, but the Client has not updated yet, the game will crash in Client.Update? 
                        // Client is still in screens list, but has Client.isExiting = true !!!



                        // Update the screen.
                        screen.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

                        if (screen.ScreenState == ScreenState.TransitionOn ||
                            screen.ScreenState == ScreenState.Active)
                        {

                            // If this is the first active screen we came across,
                            // give it a chance to handle input.
                            if (!otherScreenHasFocus)
                            {
                                // test whether the UWGame app is in focus, if not then don't handle input
                                //(!(Kensei.Dev.Options.Form != null && Kensei.Dev.Options.Form.ContainsFocus) || Replayer.IsActive))
                                //if (Game.IsActive || Replayer.IsActive) 
                                {
                                    screen.HandleInput(); // this records Commands
                                }

                                otherScreenHasFocus = true;
                            }

                            // If this is an active non-popup, inform any subsequent
                            // screens that they are covered by it.
                            if (!screen.IsPopup)
                                coveredByOtherScreen = true;
                        }
                    }

                    // Print debug trace?
                    if (traceEnabled)
                        TraceScreens();

                    StatsAndAchievements.Update(); 
                    SteamManager.Update();


                    VerifySimAndRecordedData(); // now that Sim has updated we will verify it against the recorded data.

                    recorder.Update(gameTime); // Records current input and Sim state as verification for replay. Advances frame when done.                


                    recorder.AdvanceFrame();   // advance after Sim has computed
                    replayer.AdvanceFrame();

                }
                finally
                {
                    System.Threading.Monitor.Exit(UpdateScreensLock);
                }
            }
            else
            {
                if (LoadingScreenIsActive())
                {
                    //The.LoadScreen.Update(gameTime);
                }
                else if (IngameLoadScreenIsActive())
                {
                    The.IngameLoadScreen.Update(gameTime, false, false);
                }
            }
             
        }

       


        /// <summary>
        /// the dumps never appeard on Steam. 
        /// in 64 bit, the code needs changes to work.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="ex"></param>
        void CreateSteamMiniDump(string msg, Exception ex) 
        {
         /*   try
            {
                if (SteamAPI.IsSteamRunning())
                {
                    // You can build and set an arbitrary comment to embed in the minidump here,
                    // maybe you want to put what level the user was playing, how many players on the server,
                    // how much memory is free, etc...
                    SteamAPI.SetMiniDumpComment(msg);

                    // The 0 here is a build ID, we don't set it...
                    SteamAPI.WriteMiniDump(ex, 0);
                }
            }
            catch(Exception ex2)
            {
                Console.WriteLine(ex2.Message);
            }*/
        }

        

        private void HandleReplay(ref GameTime gameTime)
        {
            if (replayer.IsPlaying)
            {
                if (Game.IsActive)
                {
                    replayerInputData.UpdateNewState(Keyboard.GetState(), Mouse.GetState());
                    
                    if (replayerInputData.IsKeyTapped(Keys.P) || replayerInputData.IsKeyTapped(Keys.Pause) || replayerInputData.IsKeyTapped(Keys.Space))
                    {
                        replayPaused = !replayPaused;
                    }
                }

                if (replayPaused == true)
                {
                    return;
                }

                replayDisplayedTime = GetDisplayedReplayTime().Value.ToString();
            
                gameTime = replayer.CurrentReplay.GetCurrentFrame().GameTime;
            }

        }

        public double? GetDisplayedReplayTime()
        {
            if (replayer != null && replayer.CurrentReplay != null)
            {
                return replayer.CurrentReplay.GetCurrentFrame().GameTime.TotalGameTime.TotalSeconds - replayer.CurrentReplay.GetStartingSeconds();
            }
            else return null;
        }

      

        public void SaveScenarioForReplay(StartGameParams startGameParams) //PlaceGameEntities.DebugScenarios scenario)
        {
            if (replayer.IsActive == false
                && startGameParams.StartGameEditorParams == null) 
            {
              /*  StartDebugScenarioParams debugScenario = startGameParams as StartDebugScenarioParams;
                if (debugScenario != null)
                {*/
                    recorder.SaveStartGameParams(startGameParams); 
               /* }
                else
                {
                    // TODO: save create game scenario startup options:

                }*/
            }
        }

        /*
        public PlaceGameEntities.DebugScenarios GetScenario()
        { 
            if(replayer.IsActive)
            {
                return replayer.CurrentReplay.scenario;
            }
            else
            {
                return PlaceGameEntities.GetDefaultScenario();
            }
        }
        */

        public int? GetRandomSeed()
        {
            if (replayer.IsActive)
            {
                // return the stored seed:
                return replayer.CurrentReplay.randomSeed;
            }
            else
            {
                // create a new random seed:
                Random seedRandomizer = new Random();
                return seedRandomizer.Next();
            }
        }

        public string GetReplayTime()
        {
            return replayDisplayedTime;
        }

        public void GameEnded()
        {
            recorder.StopRecording();

            Cleanup(); 
        }

        public void LoadReplay(string replayPath, float? timeToPauseReplay)
        {
            replayer.LoadReplay(replayPath, timeToPauseReplay);
        }

        public void LoadingFinished()
        {
            if (The.Sim != null
                    && The.Sim.Mode == Sim.EngineMode.Game)
            {               

                if (replayer.IsActive)
                {
                    replayer.StartReplay();
                  /*  if (recorder.recordDuringReplay) //???
                    {
                        recorder.StartRecording(The.Sim.GameplayRandomGenerator.RandomSeed);
                    }*/
                }
                else
                {
                    if (recorder.RecordDuringPlay)
                    {
                        recorder.CleanupOldRecordedFiles();

                        recorder.StartRecording(The.Sim.GameplayRandomGenerator.RandomSeed);
                    }
                }
            }
        }

        /// <summary>
        /// Prints a list of all the screens, for debugging.
        /// </summary>
        void TraceScreens()
        {
            List<string> screenNames = new List<string>();

            foreach (GameScreen screen in screens)
                screenNames.Add(screen.GetType().Name);

            Trace.WriteLine(string.Join(", ", screenNames.ToArray()));
        }

       


       


        private bool monkey = true;

        /// <summary>
        /// what does it do..?
        /// a monitor/lock
        /// </summary>
       // private bool threadLocked = false;

      //  private object screensLock = new object();

        public object UpdateScreensLock = new object();


        /// <summary>
        /// Tells each screen to draw itself.
        /// </summary>      
        public override void Draw(GameTime gameTime)
        {
            SetZoomRenderTaget();
          
            if (ZoomIsActive())
            {
                 GraphicsDevice.Clear(ClearOptions.Target, Color.Black, 1.0f, 0);
            }

            // skip section instead of waiting
            if (System.Threading.Monitor.TryEnter(UpdateScreensLock)) // while loading, Draw is skipped instead of blocked so the app remains responsive to Windows
            {
                try
                {
                    // load thread should not add/remove screens while the main thread is in this section:
                    foreach (GameScreen screen in screens)
                    {
                        if (screen.ScreenState == ScreenState.Hidden)
                            continue;


                        screen.Draw(gameTime); // cannot Draw client if Sim is gone... happens while loading.

                        if (screen is LoadingScreen)
                            break;// no screens get to draw after the LoadingScreen

                    }
                }
                finally
                {
                    System.Threading.Monitor.Exit(UpdateScreensLock);
                }
            }
            else
            {
                // load saved game is going on...
                if (LoadingScreenIsActive()) // !The.LoadScreen.IsLoadFinished)
                {
                    The.LoadScreen.Draw(gameTime);
                }
                else if (IngameLoadScreenIsActive())
                {
                    The.IngameLoadScreen.Draw(gameTime);
                }
            }

            // draw with zoom here:
            DrawFullscreenQuad();
        }
         
        RasterizerState rasterizerSampleClosest = new RasterizerState() { CullMode = CullMode.None, FillMode = FillMode.WireFrame,  };

       
        void DrawFullscreenQuad()
        {
            if (ZoomIsActive())
            {
                PresentationParameters pp = GraphicsDevice.PresentationParameters;

                GraphicsDevice.SetRenderTarget(null);

                //UWGame.ClientSide.Map.GameWorldRenderer.SaveRenderTargetToFile("zoomTarget", ZoomRenderTarget);

                //GraphicsDevice.RasterizerState = RasterizerState.
               
                if (ActiveZoomFactor % 1f == 0f)
                {
                    // avoid blurring by sampling points when zoom factor is whole multiples.
                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp);
                }
                else
                {
                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
                }

               // spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);


                // Draw the quad.
                spriteBatch.Draw(ZoomRenderTarget, new Rectangle(0, 0, pp.BackBufferWidth, pp.BackBufferHeight), Color.White);
                spriteBatch.End();

                
            }
        }

        #endregion

        bool LoadingScreenIsActive()
        {
            return !The.LoadScreen.IsLoadFinished;
        }

        bool IngameLoadScreenIsActive()
        {
            return The.LoadScreen.IsLoadFinished;
        }


        public bool ZoomIsActive()
        {
            return ActiveZoomFactor > 1f; // Options.ZoomFactor > 1f; 
        }


       // static AutoResetEvent drawScreensSignal;

        #region Public Methods


        /// <summary>
        /// Adds a new screen to the screen manager.
        /// </summary>
        public void AddScreen(GameScreen screen, int index = -1)
        {

          /*  while (threadLocked)
            {
            }

            threadLocked = true;*/

             screen.Controller = this;

            // doesn't work! Texture2d Disposed error when loading content outside LoadContent.
            // only load if we are past the constructor.
            // If we have a graphics device, tell the screen to load content.
            if (contentIsLoaded && 
                graphicsDeviceService != null &&
                graphicsDeviceService.GraphicsDevice != null)
            {
                screen.LoadContent(); //true);
            }

           /* lock (screensLock)
            {*/

                if (index != -1)
                {
                    screens.Insert(index, screen);
                }
                else
                {
                    screens.Add(screen);
                }
           // }

          //  threadLocked = false;

        }

        /// <summary>
        /// called when replacing Sim and Client during load operations
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="unloadContent"></param>
        public void RemoveFromList(GameScreen screen)
        {
           /* while (threadLocked)
            {
            }

            threadLocked = true;*/

           /* lock (screensLock)
            {*/
                screens.Remove(screen);
            //}

            screensToUpdate.Remove(screen);

           // threadLocked = false;
        }

        public int GetIndexOfScreen(GameScreen screen)
        {
            return screens.IndexOf(screen);

        }
     

        /// <summary>
        /// Removes a screen from the screen manager. You should normally
        /// use GameScreen.ExitScreen instead of calling this directly, so
        /// the screen can gradually transition off rather than just being
        /// instantly removed.
        /// </summary>
        public void RemoveScreenNow(GameScreen screen, bool unloadContent = true)
        {
            // If we have a graphics device, tell the screen to unload content.
            if (unloadContent == true &&
                (graphicsDeviceService != null) &&
                (graphicsDeviceService.GraphicsDevice != null))
            {
                screen.UnloadContent(); 
            }

            screen.Destroy();

           /* while (threadLocked)
            {
            }

            threadLocked = true;*/

           /* lock (screensLock)
            {*/
                screens.Remove(screen);
           // }

            screensToUpdate.Remove(screen);

           // threadLocked = false;
        
        }


        /// <summary>
        /// Expose an array holding all the screens. We return a copy rather
        /// than the real master list, because screens should only ever be added
        /// or removed using the AddScreen and RemoveScreen methods.
        /// </summary>
        public GameScreen[] GetScreens()
        {
            return screens.ToArray();
        }


        /// <summary>
        /// Helper draws a translucent black fullscreen sprite, used for fading
        /// screens in and out, and for darkening the background behind popups.
        /// </summary>
        public void FadeBackBufferToBlack(int alpha)
        {
            //Viewport viewport = GraphicsDevice.Viewport;
            Dimension dim = DrawArea;
            spriteBatch.Begin();

            spriteBatch.Draw(blankTexture,
                             new Rectangle(0, 0, dim.Width, dim.Height),
                             new Color((byte)0, (byte)0, (byte)0, (byte)alpha));
            
            spriteBatch.End();
        }


        /// <summary>
        /// we destroy the old client to get rid of any lingering references to Sim objects that no longer exist, such as UIAllegiance
        /// </summary>
        /// <param name="controller"></param>
        public void RecreateClientAfterLoad() 
        {
            ContentManager clientContent = The.Client.Content; // save this...

            RemoveScreenNow(The.Client, false); // keep the loaded content, but make sure we dispose rendertargets

            // NEW: Let's compact large object heap after destroying the client and its large vertex arrays:  
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect();   // takes 1 second

            //following same pattern as in StartGameQueueMain:

            The.Client = new ClientSide.Client(this, clientContent);

            AddScreen(The.Client); // calls LoadContent(). the content is already in the content manager's memory, so this is instant                       

            The.Client.Init(); // this takes a long time


            AudioManager.Resume(); // resume the paused music after in-game save only. Loading/saving a cue position won't work...

          //  The.Client.BeginRun(); // moved to first frame of Update for ingame save load
          
        }
       


        #endregion

        
    }
    /*
    public class Globals
    {
        private static Globals instance;

                      
        /// <summary>
        /// For use in client classes and methods such as Draw() and particles...
        /// </summary>
        public RandomGenerator ClientRandomGenerator;


        private Globals()
        {
            //before I can save the seed in a proper way I need to make sure the Randomizer is reset as soon as we start recording
            //, otherwise the replay randomizer and the regular randomizer will be out of sync

            //CreateNewRandomGenerator();
            ClientRandomGenerator = new RandomGenerator();
        }

        public static Globals Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Globals();
                }
                return instance;

            }

        }

    }*/
}
