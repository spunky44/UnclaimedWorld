#region File Description
//-----------------------------------------------------------------------------
// LoadingScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UWGame;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using UWGame.SimSide;
using GameStateManagement;
using UWGame.Control;
using UWGame.SimSide.Scenarios;
using System.Xml.Serialization;
using System.Threading;
 
#endregion

namespace UWGame.ClientSide.Screens
{
    /// <summary>
    /// The loading screen coordinates transitions between the menu system and the
    /// game itself. Normally one screen will transition off at the same time as
    /// the next screen is transitioning on, but for larger transitions that can
    /// take a longer time to load their data, we want the menu system to be entirely
    /// gone before we start loading the game. This is done as follows:
    /// 
    /// - Tell all the existing screens to transition off.
    /// - Activate a loading screen, which will transition on at the same time.
    /// - The loading screen watches the state of the previous screens.
    /// - When it sees they have finished transitioning off, it activates the real
    ///   next screen, which may take a long time to load its data. The loading
    ///   screen will be the only thing displayed while this load is taking place.
    /// </summary>
    /// 
    public class LoadingScreen : GameScreen
    {
        #region Fields

        bool loadingIsSlow;
        bool otherScreensAreGone;

        /// <summary>
        /// fill this when loading any other screen than the Sim/Client
        /// </summary>
        EventHandler<EventArgs> loadEventHandler;

        /// <summary>
        /// fill this when loading the game/scenario/editor
        /// </summary>
        StartGameParams startGameParams;


        Rectangle backgroundDest;
        Texture2D backgroundTexture;

        ContentManager content;

        LoadingScreenInterface intf;

        /// <summary>
        /// await input from the user before the game starts:
        /// </summary>
        public bool WaitForUser = true;

        /// <summary>
        /// did user continue?
        /// </summary>
        private bool userContinued = false;

       

        #endregion

        #region Initialization

        /// <summary>
        /// The constructor is private: loading screens should
        /// be activated via the static Load method instead.
        /// </summary>
        private LoadingScreen(Controller screenManager, bool showInterface, bool loadingIsSlow, StartGameParams startGameParams, EventHandler<EventArgs> loadNextScreen)
        {
            base.Controller = screenManager;

            TransitionOnTime = TimeSpan.FromSeconds(0.5);

            this.WaitForUser = false; // waitForUser;
            this.loadingIsSlow = loadingIsSlow;

            this.loadEventHandler = loadNextScreen;
            this.startGameParams = startGameParams;

            if (loadNextScreen == null)
            {
                startGame = true;
            }

            if (showInterface)
            {
                intf = new LoadingScreenInterface(this, startGameParams, Controller.Game);
            }

        }

        private static int imageToShow = DateTime.Now.Millisecond;


        /// <summary>
        /// Loads graphics content for this screen. The background texture is quite
        /// big, so we use our own local ContentManager to load it. This allows us
        /// to unload before going from the menus into the game itself, wheras if we
        /// used the shared ContentManager provided by the ScreenManager, the content
        /// would remain loaded forever.
        /// </summary>               
        public override void LoadContent()
        {
            if (content == null)
            {
                content = new ContentManager(Controller.Game.Services);
                content.RootDirectory = "Content";
            }

            

            InitBackgroundImage();

            if (intf != null)
            {
                intf.LoadContent();
            }
        }

        /// <summary>
        /// Unloads graphics content for this screen.
        /// </summary>
        public override void UnloadContent()
        {
            content.Unload();

            if (intf != null)
            {
                intf.UnloadContent();
            }
        }

        public static void StartTransitioningToGame(Controller controller, StartGameParams startGameParams, //Sim.EngineMode mode,
                               bool loadingIsSlow, bool showInterface = false)
        {
            // Tell all the current screens to transition off.
            foreach (GameScreen screen in controller.GetScreens())
                screen.ExitScreen();

            // Create and activate the loading screen.
            LoadingScreen loadingScreen = new LoadingScreen(controller, showInterface, loadingIsSlow, startGameParams, null); //mode);
            
            The.LoadScreen = loadingScreen;

            
            // loadingScreen.loadingIsSlow = loadingIsSlow;
            //  loadingScreen.loadEventHandler = loadNextScreen;

            controller.AddScreen(loadingScreen);
        }

        /// <summary>
        /// Activates the loading screen for transitioning to another screen that may take some time
        /// </summary>
        public static void StartTransition(Controller controller,
                                EventHandler<EventArgs> loadNextScreen,
                                bool loadingIsSlow, bool showInterface = false)
        {
            // Tell all the current screens to transition off.
            foreach (GameScreen screen in controller.GetScreens())
                screen.ExitScreen();

            // Create and activate the loading screen.
            LoadingScreen loadingScreen = new LoadingScreen(controller, showInterface, loadingIsSlow, null, loadNextScreen);

            The.LoadScreen = loadingScreen;


            controller.AddScreen(loadingScreen);
        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Updates the loading screen.
        /// </summary>
        private bool startGameQueueHasFinished;
       


        private bool startGame;
       /* {
            get;
            set;
        }*/

      

        // [EQATEC.Profiler.SkipInstrumentation]
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            if (otherScreensAreGone)
            {
                if (startGame)
                {
                    // start loading the game (for playing, editing or replaying)
                    if (StartGameQueueMain())
                    {
                        Controller.RemoveScreenNow(this);
                        startGame = false;
                    }
                    ScreenState = ScreenState.Active;//override hidden
                }
                else
                {
                    // show any other screen than the game:
                    loadEventHandler(this, EventArgs.Empty);
                    Controller.RemoveScreenNow(this);
                }

               /* if (QueueActive)
                {

                    if (LoadingQueueMain())
                    {
                       // ScreenManager.RemoveScreen(this);
                        QueueActive = false;

                    }
                                        

                    ScreenState = ScreenState.Active;//override hidden

                    intf.Update(gameTime);

                }
                else if (userContinued)
                {
                    loadEventHandler(this, EventArgs.Empty);
                    ScreenManager.RemoveScreen(this);

                    ScreenManager.AudioManager.StopSong();

                    IsLoadFinished = true;
                }    */          

            }

            if (intf != null)
            {
                intf.Update(gameTime);
            }
        }

       
        public bool IsLoadFinished
        {
            get;
            set;
        }

        private enum QueueState
        {
            ADD_SIM,
            ADD_CLIENT,
            INIT_SIM,
            GAMEDATA_LOADCONTENT,
            GAMEDATA_INIT,
            INIT_CLIENT,
            RUN_SIM,
            RUN_CLIENT,
            DONE
        };

        private QueueState queueState = QueueState.ADD_SIM;
        private System.Threading.Thread thread;
        private bool threadStarted = false;
        public bool StartGameQueueMain()
        {
            /*
             *

             *   if (The.Client.GraphicsDevice.GraphicsDeviceStatus == GraphicsDeviceStatus.Lost)
             *   {
             *       return false;
             *   }
             * 
             * This is placed in some of the cases due to a problem with the device being lost while trying to create rendertargets and models
             * What we currently do not know is how this should be solved
             * Ideas
             * Can we use the delegate The.Client.GraphicsDevice.DeviceReset to recreate the modells and rendertargets if they are lost?
             * 
             */

            // first create Sim and Client in a thread (why.. ?):
            if (threadStarted == false)
            {
                thread = new System.Threading.Thread(LoadDataInThread);
                thread.IsBackground = true;
                thread.Start(); // <- This loads GameData, such as EntityTypes
                threadStarted = true;
                IsLoadFinished = false;
            }

            // init the rest on the main thread:
            if (The.LoadScreen.startGameQueueHasFinished)
            {
                if (thread != null
                    && thread.IsAlive)
                {
                    if (Thread.CurrentThread != thread)
                    {
                        thread.Join();
                    }

                   // thread.Suspend(); // ?? OLD
                }

                switch (queueState)
                {
                    case QueueState.ADD_SIM:
                        {
                            if (The.Sim != null)
                            {
                                Controller.AddScreen(The.Sim); // calls LoadContent()
                            }

                            The.LoadScreen.Progress("Adding Client... ", 1937);
                            queueState = QueueState.ADD_CLIENT;
                            break;
                        }
                    case QueueState.ADD_CLIENT:
                        {
                            
                            if (The.Client != null)
                            {
                                if (The.Client.GraphicsDevice.GraphicsDeviceStatus == GraphicsDeviceStatus.Lost)
                                {
                                    return false;
                                }

                                Controller.AddScreen(The.Client); // calls LoadContent()
                            }

                            The.LoadScreen.Progress("Initializing Sim... ", 31);
                            queueState = QueueState.INIT_SIM;
                            break;
                        }
                    case QueueState.INIT_SIM:
                        {
                            if (The.Sim != null)
                            {
                                The.Sim.Init(); // does very little...
                            }

                            The.LoadScreen.Progress("Loading GameData content... ", 63);
                            queueState = QueueState.GAMEDATA_LOADCONTENT;
                            break;
                        }
                    case QueueState.GAMEDATA_LOADCONTENT:
                        {
                            if (The.Client != null && The.Client.GraphicsDevice.GraphicsDeviceStatus == GraphicsDeviceStatus.Lost)
                            {
                                return false;
                            }
                            //this method runs a special subqueue
                            if (!GameData.Instance.LoadContent(Controller.Game, The.Client.Content)) // ScreenManager.Content))
                                break;

                            The.LoadScreen.Progress("Init GameData... ", 141);
                            queueState = QueueState.GAMEDATA_INIT;
                            break;
                        }
                    case QueueState.GAMEDATA_INIT:
                        {
                            GameData.Instance.Initialize(); // calls PostLoadContentInitialize()

                            The.LoadScreen.Progress("Initializing Client... ", 265);
                            queueState = QueueState.INIT_CLIENT;
                            break;
                        }
                    case QueueState.INIT_CLIENT:
                        {
                            if (The.Client != null && The.Client.GraphicsDevice.GraphicsDeviceStatus == GraphicsDeviceStatus.Lost)
                            {
                                return false;
                            }

                            if (intf != null && this.PromptDialogIsDisplayed())
                            {
                                intf.PromptUserToContinue();

                                // wait until user has dismissed dialog before we can continue loading the in game interface
                                break;

                            }
                            else
                            {
                                if (The.Client != null)
                                {
                                    The.Client.Init();
                                }

                                The.LoadScreen.Progress("Sim.BeginRun... ", 67);
                                queueState = QueueState.RUN_SIM;
                                break;
                            }
                        }
                    case QueueState.RUN_SIM:
                        {
                            if (The.Sim != null)
                            {
                                if (!The.Sim.QueueBeginRun()) //this starts the game. If loading, The.Sim will have been replaced with a new instance when we return from this call...
                                    break;
                            } 

                            The.LoadScreen.Progress("Client.BeginRun... ", 2625);
                            queueState = QueueState.RUN_CLIENT;
                            break;
                        }
                    case QueueState.RUN_CLIENT:
                        {
                            // before we proceed, make sure that load went as it should:
                            VerifyLoadSaveGame();

                            if (The.Client != null)
                            {
                                if (The.Client.GraphicsDevice.GraphicsDeviceStatus == GraphicsDeviceStatus.Lost) // what does this do?
                                {
                                    return false;
                                }

                                if (!The.Client.BeginRunWasCalled)
                                {
                                    The.Client.BeginRun(); 
                                }
                            }

                            The.LoadScreen.Progress("Loading Queue Done... ", 1);
                            queueState = QueueState.DONE;
                            break;
                        }
                    case QueueState.DONE:
                    default:
                        {
                         
                            IsLoadFinished = true;
                            Controller.LoadingFinished(); // why isn't this called after in-game load also..?
                            return true;
                        }

                }//end switch
            }//end if loadqueuefinished

            return false;
        }


        private void VerifyLoadSaveGame()
        {
            if (Sim.LoadException != null)
            {
                throw Sim.LoadException;
            }
        }
       

        private bool PromptDialogIsDisplayed()
        {
            return WaitForUser && !userContinued;
        }


        /// <summary>
        /// this completes instantly!
        /// </summary>
        private void LoadDataInThread()
        {
          
            Controller.UpdateNewStateFromInputDevices(); //Keyboard.GetState(), UWGame.Control.Input.InputManager.GetZoomedMouseState()); // Mouse.GetState());
         
            while (true)
            {
                #if RELEASE
                    // handle and display exceptions that happen in this thread (they are not caught otherwise):
                    try{
                #endif

                if (!QueueSimAndClientCtors()) // almost no work in this.
                {                   
                    return; // end the thread
                }

                 #if RELEASE                   
                    }
                    catch (Exception e)
                    {
                        Controller.Game.HandleExceptionInReleaseMode(e, false);
                    }
                #endif
            }
        }




        private enum SimClientQueueState
        {
            BEGIN,
            SimConstructor,
            CLIENT_CTOR,
            //   SIM_ADD,
            //   CLIENT_ADD,
            //   SIM_INIT,
            //   CLIENT_INIT,
            //    SIM_BEGINRUN,
            //   CLIENT_BEGINRUN,
            DONE

        }

        private SimClientQueueState simClientQueueState = SimClientQueueState.BEGIN;
        private bool QueueSimAndClientCtors()
        {

            //The Sim and the Client are decoupled, we need one of each
            //It should eventaully be possible for the sim to run in the
            //complete absence of a client (running headless)
            //or in the presence of any variation, subclass or state
            //of client... the client has no effect whatsoever on the sim
            //some day -- MLo
            bool headless = false;

            switch (simClientQueueState)
            {
                case SimClientQueueState.BEGIN:
                    {
                        The.LoadScreen.Progress("Sim Constructor...", 62);
                        simClientQueueState = SimClientQueueState.SimConstructor;

                        break;
                    }
                case SimClientQueueState.SimConstructor:
                    {
                        if (The.Sim == null)
                        {
                            int? randomSeed = Controller.GetRandomSeed();
                                                        
                            // saves scenario replay data
                            
                            Controller.SaveScenarioForReplay(startGameParams); 

                            The.Sim = new UWGame.SimSide.Sim(Controller, startGameParams, randomSeed);
                             
                        }

                        //*****
                        // Loads GAME DATA (EntityTypes etc.) here:
                        //******
                        if (The.Sim.QueueGameDataAndSimInit()) //returns true when queue is finished
                        {
                            The.LoadScreen.Progress("ClientConstructor...", 140);
                            simClientQueueState = SimClientQueueState.CLIENT_CTOR;
                        }

                        break;
                    }
                case SimClientQueueState.CLIENT_CTOR: 
                    {
                        if (!headless && The.Client == null)
                        {
                            The.Client = new UWGame.ClientSide.Client(Controller);
                        }

                        The.LoadScreen.Progress("Everything Else...", 47);
                        The.LoadScreen.startGameQueueHasFinished = true;  // signal that the main program thread can continue

                        simClientQueueState = SimClientQueueState.DONE;

                        return false; // end the thread
                       // break;
                    }

            }

            return true;


        }


        public void OKToStartGame()
        {
            userContinued = true;
        }

        protected override float getStringEnlargementFactor()
        {
            return 1f;
        }

        private int progress = 0;
        private List<string> queueNamesTimes = new List<string>();

        private int prevTime = System.Environment.TickCount;


        /// <summary>
        /// The.LoadScreen.Progress("Sim Constructor...", 469);
        ///tells the loading screen that the next phase includes the construction of the Sim class instance, 
        ///and that this call is expected to take 469 milliseconds (about a half second). 
        /// </summary>
        /// <param name="newStatus"></param>
        /// <param name="points"></param>
        public void Progress(string newStatus, int points)
        {

            int time = System.Environment.TickCount;

            while (threadlocked)
            {
            }

            threadlocked = true;
            progress += points;

            //stamp the time since the last progress into the last entry in status list, before adding new(pending) status
            if (queueNamesTimes.Count >= 1)
                queueNamesTimes[queueNamesTimes.Count - 1] += " " + (time - prevTime) + " msec";

            queueNamesTimes.Add(newStatus);

            threadlocked = false;

            prevTime = time;
        }

        private bool threadlocked = false;


        /// <summary>
        /// Draws the loading screen.
        /// </summary>
        private string graph = "";
        private float angle = 0f;
        private double graphValue = 0d;
        public override void Draw(GameTime gameTime)
        {
            if ((ScreenState == ScreenState.Active) &&
                (Controller.GetScreens().Length == 1))
            {
                otherScreensAreGone = true;
            }
            
           
            if (loadingIsSlow)
            {
                Controller.SpriteBatch.GraphicsDevice.Clear(Color.Black);

                Color color = new Color((byte)255, (byte)255, (byte)255, TransitionAlpha);

                Controller.SpriteBatch.Begin();
                Controller.SpriteBatch.Draw(backgroundTexture, backgroundDest, color);
                Controller.SpriteBatch.End();

                UpdateAndDrawProgress();

                if (intf != null)
                {
                    intf.Draw(gameTime);
                }
            }
        }

        public override void Destroy()
        {
            loadEventHandler = null;
            /*while (true)
            {
            }*/

            if (thread != null)
            {
                thread.Join();
                thread = null;
            }

            if (intf != null)
            {
                intf.Destroy();

                intf = null;
            }
        }

        private void UpdateAndDrawProgress()
        {
            Vector2 textPosition = new Vector2(100, 100);


            while (threadlocked)
            {
            }

            threadlocked = true;

            if (graphValue < ((double)progress))
                graphValue++;

            //if we are getting lots of updates while the load is in the load thread, then lerp gently
            if (this.queueState < QueueState.ADD_CLIENT)
                graphValue = graphValue * 0.999d + ((double)progress) * 0.001d;
            else //otherwise, practically don't lerp at all
                graphValue = graphValue * 0.7d + ((double)progress) * 0.3d;

            threadlocked = false;
            //      //
            //     //
            //    / |  /|
            double totalTime = 32263; // 18973;//<(=======--
            //    \ |  \|
            //     \\   
            //      \\   

            double scalar = Math.Min(1d, graphValue / totalTime);

            double graphWidth = 400d;

            //current total points: 39765
            int graphLength = (int)(scalar * graphWidth);

            if (graph.Length > graphLength)
                graph = "";

            while (graph.Length < graphLength)
                graph += "|";


            //ScreenManager.SpriteBatch.Begin(SpriteSortMode.immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            Controller.SpriteBatch.Begin();

           
            int yPos = Controller.DrawArea.Height - 200; // .GraphicsDevice.Viewport.Height - 200;

            Color color = Color.DarkGray;
            if (The.LoadScreen.startGameQueueHasFinished)
            {
                color = Color.Gray; // singlethreaded now...
            }

            for (int offs = 0; offs < 3; ++offs)
                Controller.SpriteBatch.DrawString(Controller.Font, graph, new Vector2(100 + offs, yPos), color, 0f, new Vector2(0), new Vector2(1f, 2f), new SpriteEffects(), 0);

            
            //Then draw the optional progress log, only if spacebar is held
            if (Keyboard.GetState().IsKeyDown(Keys.Space))
            {
               float scale = getStringEnlargementFactor();

                textPosition.Y += 40;

                while (threadlocked)
                {
                }

                threadlocked = true;

                int count = queueNamesTimes.Count;
                string s;
                for (int i = 0; i < queueNamesTimes.Count; i++)
                {
                    s = queueNamesTimes[i] + " " + graphValue;

                    if (--count == 0)//make the last queue name big
                    {
                        scale *= 1.33f;
                    }

                    if (count < 20)//only draw the last twenty queue names
                    {
                        Controller.SpriteBatch.DrawString(Controller.Font, s, textPosition, Color.Black, 0f,
                                                new Vector2(0), scale, new SpriteEffects(), 0);
                        textPosition.Y += 20;
                    }
                }

                threadlocked = false;

            }
            else //no spacebar, so draw the backstory text instead
            {
                float scale = 1;

//                textPosition.Y += 40;

                while (threadlocked)
                {
                }

                threadlocked = true;

            /*    string s = LoadingScreenInterface.displayText;

                double length = (double)s.Length;
                double amtToDraw = length* Math.Min(1, scalar*2);
                int numChars = (int)amtToDraw;
                s = s.Substring(0, numChars);
                
                ScreenManager.SpriteBatch.DrawString(ScreenManager.Font, s, textPosition, Color.Black, 0f,
                                        new Vector2(0), scale, new SpriteEffects(), 0);                
                */

                threadlocked = false;      

            }

            Controller.SpriteBatch.End();
        }


        /// <summary>
        /// load and scale/crop the image that is shown in the background of the loading screen
        /// </summary>
        private void InitBackgroundImage()
        {         
          
            if (startGameParams != null && startGameParams.StartScenarioParams != null)
            {
                backgroundTexture = content.Load<Texture2D>(startGameParams.StartScenarioParams.Scenario.ScenarioData.LoadingBackgroundImage); 
            }
            else
            {
                // default:
                backgroundTexture = content.Load<Texture2D>("Scenarios/Default/Scenario Screen/TitleImgSurvival_1920px");
            }

            //    backgroundTexture = content.Load<Texture2D>("MainMenu/panorama_blue_1920px");        
            //    backgroundTexture = content.Load<Texture2D>("MainMenu/old_panorama_1920px");       
            //    backgroundTexture = content.Load<Texture2D>("MainMenu/TitleImgSurvival_1920px");
            //    backgroundTexture = content.Load<Texture2D>("MainMenu/panorama_1920px");
              

            int backgroundImageWidth = backgroundTexture.Width;
            int backgroundImageHeight = backgroundTexture.Height;

            //float scaleFactor = (float)screenSize.Y / (float)backgroundImageHeight;
            //// scale up as well??? factor > 1f

            //// scale height, and crop sides:
            //int height = screenSize.Y;
            //int width = (int)(scaleFactor * backgroundImageWidth); //(int)(((float)height) * (((float)screenSize.X) / ((float)screenSize.Y)));



            //LETTERBOX
            Dimension dim = Controller.DrawArea;
            //Viewport viewport = Controller.GraphicsDevice.Viewport;
            if (dim.Width < backgroundImageWidth) // viewport.Width < backgroundImageWidth)
            {
                float sc = (float)dim.Width / (float)backgroundImageWidth;
                backgroundImageWidth = dim.Width;
                backgroundImageHeight = (int)((float)backgroundImageHeight * sc);
            }

            backgroundDest = new Rectangle((dim.Width - backgroundImageWidth) / 2,
                                            (dim.Height - backgroundImageHeight) / 3,
                                            backgroundImageWidth,
                                            backgroundImageHeight);

        }






        #endregion
    }



}
