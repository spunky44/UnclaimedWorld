
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

using Xclna.Xna.Animation;
using GameStateManagement;
using SpriteSheetRuntime;
using InputEventSystem;

using UWGame.ClientSide.Interface;
using UWGame.SimSide.AI.Needs;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using UWGame.SimSide;

using UWGame.ClientSide.Renderables;
using UWGame.ClientSide.Particles;
using UWGame.ClientSide.Map;
using UWGame.Client.Interface.MapGUI;
using UWGame.ClientSide.Interface.HUD_Windows;
using UWGame.SimSide.AI.Goals;
using UWGame.ClientSide.Log;
using WindowSystem;
using UWGame.Client.Audio;
using UWGame.Client.Interface;
using UWGame.Control;
using UWGame.SimSide.Entities.Locomotors;
using UWGame.Control.Replays;
using UWGame.SimSide.Systems;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.AI;
using UWGame.SimSide.Items;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.Entities.Owners;
using RoundLineCode;
using UWGame.Control.Commands;
using UWGame.SimSide.Commands;
using UWGame.SimSide.Scenarios;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.IngameEvents;
using UWGame.SimSide.AI.Pathfinding;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.AI.StrategicDecisions;
using UWGame.SimSide.InGameEvents;
using UWGame.ClientSide.GameEvents;
using UWGame.ClientSide.Feedback;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Snapshots;
using UWGame.ClientSide.Interface.Tasks;

namespace UWGame.ClientSide
{





    /// <summary>
    /// This class will someday encompass all the client side classes and manages thier updates and such
    /// It includes the UI
    /// The display
    /// anything that renders
    /// plays sound
    /// receives human input
    /// provides feedback to a human
    /// the simluation can run without in a headless mode. 
    /// Client side objects are freely accessible and writable from classes on SimSide. SimSide classes are responsible for constructing and monitoring client classes, and ensuring they really exist. Client class is still responsible for giving client classes an update timeslice.    
    /// 
    /// when headless, the Client should not be null, instead it should be an instance of a derived class that does not react to method calls (same as Renderable)!
    /// 
    /// Don´t use Game.Window.Client bounds for getting the screen boundaries, it changes if we minimize the game. Use ScreenHeight and ScreenWidth instead.
    /// </summary>
    public class Client : GameScreen//never inherit any base or interface that saves, loads, or must be instanced 
    {
        /// <summary>
        /// Client is singleton, but sometimes does not exist, headless means it doesn't
        /// The game Simulation should be able to run without client, unaffected
        /// </summary>
        /// <returns></returns>
        public static bool isHeadless()//TODO Move to common
        {
            return The.Client == null; // but not for long, headless mode means that the client does not exist
        }


        public GraphicsDevice GraphicsDevice
        {
            get { return Controller.GraphicsDevice; }
        }

        public int ScreenWidth
        {
            get
            {
                return Controller.DrawArea.Width; // .GraphicsDevice.Viewport.Width;
            }
        }

        public int ScreenHeight
        {
            get
            {
                return Controller.DrawArea.Height; // .GraphicsDevice.Viewport.Height;
            }
        }

        public ContentManager Content;
        public SpriteBatch spriteBatch;
        public SpriteSheet FlatSpriteSheet;
        // Effect used to apply the edge detection postprocessing.
        public Effect EdgeDetectEffect;
        PrimitiveBatch primitiveBatch;
        public QuadRendererComponent quadRenderer;


       

        /// <summary>
        /// used for in-game sound effects (not music)
        /// </summary>
        public AudioManager AudioManager;

        public FeedbackManager Feedback;

        public float FarPlane = 3000f;//TODO move into a View class
        public float NearPlane = -1500f;

        /// <summary>
        /// orthographic projection for terrain etc.
        /// </summary>
        public Matrix Projection;

        /// <summary>
        /// not currently used
        /// </summary>
        public Matrix PerspectiveProjection;
       // public Matrix PerspectiveView;

        public GameWorldRenderer Renderer;

        public RoundLineManager RoundLineManager;
       
        public delegate void ButtonClick(object sender);

        public Texture2D interfaceArt;


        public ParticleManager ParticleManager;

        public RandomGenerator ClientRandomGenerator;

        /// <summary>
        /// Collection of event dialogs that the player can cycle trough when inside an EventDialog.
        /// </summary>
        public List<EventDialogData> EventDialogsData = new List<EventDialogData>();
        public int CurrentEventDialogIndex = 0;


        public bool EnableKeyboardShortcuts = true;

        public Log.Log Log;

        public bool BloomEnabled = true; // true;  //true; // false; //true;
        public bool EdgeDetectEnabled = true;

        SleepyUpdater<Renderable> renderables = new SleepyUpdater<Renderable>(Module.Client);

        /// <summary>
        /// this is only for free renderables, like particle emitters. other renderables are updated by their respective entity
        /// </summary>
      //  private List<Renderable> renderables = new List<Renderable>();
     

        int frameRate = 0;
        int frameCounter = 0;

        

        //public bool IsPaused = false;

        /* private float masterSFXVolume = 0.5f;
         public float MasterSFXVolume
         {
             get
             {
                 return masterSFXVolume;
             }
             set
             {
                 this.masterSFXVolume = value;
                
                 //The.InGameUI.gui.MasterSFXVolume = value;

                 // this affects all playing sound effects:
                 SoundEffect.MasterVolume = value; 
             }
         }
         */

   //     public bool IsRecreated = false;

        public TimeSpan ElapsedTimeBetweenDraws;

        /// <summary>
        /// when true, no mouse clicks or key presses will have an effect other than to the modal dialog
        /// 
        /// don't assign this - call SetModal!!!
        /// </summary>
        public bool IsModal
        {
            get;
            private set;
        }

        TimeSpan frameRateElapsedTime = TimeSpan.Zero;

        public GameTime GameTime = new GameTime();

        InputData inputData;

       
        /// <summary>
        /// ctor the client should really be the screenmanager, not a screen in the manager... but for now this works fine.
        /// </summary>
        /// <param name="controller"></param>
        public Client(Controller controller, ContentManager clientContent = null) 
        {
            The.Client = this;// make sure to null references in "The" class in destructors, throughout

            ClientRandomGenerator = new RandomGenerator(RandomGenerator.GeneratorType.Client);
            printAllegiancesRegulator = new Regulator(ClientRandomGenerator, 1, "Client");
            printPerformanceRegulator = new Regulator(ClientRandomGenerator, 1, "Client");
            printEmigrateRollsRegulator = new Regulator(ClientRandomGenerator, 0.25f, "Client");
           // replenishAlertRegulator = new Regulator(ClientRandomGenerator /*The.Sim.GameplayRandomGenerator*/, 1d / GameData.Instance.GUIConstants.TimeBetweenReplenishAlerts, "Client"); // "Expedition replenish");
      
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            this.Controller = controller;
            inputData = Controller.InputData;

            The.InGameUI = new InGameInterface(this.Controller.Game, GameData.Instance.GUIConstants.CustomColors);

            The.MapUI = new MapClient();

            quadRenderer = new QuadRendererComponent(); //Controller.Game);
         //   Controller.Game.Components.Add(quadRenderer);

            AudioManager = new UWGame.Client.Audio.AudioManager(controller.Options, false);

            if (clientContent == null)
            {
                Content = new ContentManager(Controller.Game.Services);
                Content.RootDirectory = "Content";
            }
            else
            {
                Content = clientContent;
            }

            Feedback = new FeedbackManager();

            Projection = Matrix.CreateOrthographic(Controller.DrawArea.Width, //GraphicsDevice.Viewport.Width,
                            Controller.DrawArea.Height, //GraphicsDevice.Viewport.Height, 
                            NearPlane, FarPlane);

            float aspectRatio = (float)Controller.DrawArea.Width / //GraphicsDevice.Viewport.Width /
                    (float)Controller.DrawArea.Height; // .GraphicsDevice.Viewport.Height;

            PerspectiveProjection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(40.0f),
                aspectRatio,
                1.0f, 10000.0f);
          //  PerspectiveView = Matrix.CreateLookAt(new Vector3(0, -420, -420f), new Vector3(0, 0, 0f), Vector3.UnitY);

            ParticleManager = new ParticleManager();

            Log = new Log.Log();


            Renderer = new GameWorldRenderer();

            RoundLineManager = new RoundLineManager();

        }//ctor




        public override void Init()
        {
            base.Init();

            //this has to be called after both client and sim are cted
            The.InGameUI.Initialize(); // the pools of EntityTypeTooltip windows take several seconds... :(

            Renderer.Init();

            ParticleManager.Init();
        }


        #region Replenish feeback

        /// <summary>
        /// call this from AI evaluators to give player feedback without extra overhead
        /// </summary>
        /// <param name="toolType"></param>
        /// <param name="status"></param>
       /* public virtual void SetToolReplenishStatus(Expedition expedition, EntityType toolType, bool ownsItem) // ReplenishStatus status)
        {
            Feedback.SetToolReplenishStatus(expedition, toolType, ownsItem);
        }*/

        public virtual void SetToolReplenishStatusItemOwned(Expedition expedition, EntityType toolType, bool ownsItem, float? requiredAmount) // ReplenishStatus status)
        {
            Feedback.SetToolReplenishStatusOwnsItem(expedition, toolType, ownsItem, requiredAmount);
        }

        public virtual void SetToolReplenishStatusItemAvailable(Expedition expedition, EntityType toolType, bool itemAvailable, float? requiredAmount) // ReplenishStatus status)
        {
            Feedback.SetToolReplenishStatusItemIsAvailable(expedition, toolType, itemAvailable, requiredAmount);
        }


        public bool GetToolReplenishStatus(Expedition expedition, EntityType toolType, out float? minimumRequiredAmount)
        {
            return Feedback.GetToolReplenishStatus(expedition, toolType, out minimumRequiredAmount);
        }
           

        #endregion



        #region Accessibility feedback

        public void GetFeedback(JobID jobID, out bool isInAccessible, out bool isBlockedByThreat, out bool isBlockedDueToBoldStanceRequired, out bool tooFarFromExpedition, out bool huntingNotFeasible, out bool areaNotCleared)
        {
            Feedback.GetFeedback(jobID, out isInAccessible, out isBlockedByThreat, out isBlockedDueToBoldStanceRequired, out tooFarFromExpedition, out huntingNotFeasible, out areaNotCleared);
        
        }

        public virtual void GetFeedback(EntityID entityID, out bool isInAccessible, out bool isBlockedByThreat, out bool isBlockedByBoldStance)
        {
            Feedback.GetFeedback(entityID, out isInAccessible, out isBlockedByThreat, out isBlockedByBoldStance);
        }

        public virtual bool GetIsInaccessible(JobID jobID)
        {
            return Feedback.GetIsInaccessible(jobID);
        }

        public virtual bool GetIsBlockedByThreat(JobID jobID)
        {
            return Feedback.GetIsBlockedByThreat(jobID);
        }

        public virtual void SetJobInaccessible(Job job, IHasEntityGroup ownerOfJob, bool isInaccessible)
        {
            Feedback.SetJobInaccessible(job, ownerOfJob, isInaccessible);
        }

        public virtual void SetHuntingJobNotFeasible(Job job, IHasEntityGroup ownerOfJob, bool value)
        {
            Feedback.SetHuntingJobNotFeasible(job, ownerOfJob, value);
        }

        public virtual void SetAreaNotCleared(Job job, IHasEntityGroup ownerOfJob, bool value)
        {
            Feedback.SetAreaNotCleared(job, ownerOfJob, value);
            
        }

        public virtual void SetJobTooFarFromExpedition(Job job, IHasEntityGroup ownerOfJob, bool value)
        {
            Feedback.SetJobTooFarFromExpedition(job, ownerOfJob, value);
        }

        public virtual void SetJobBlockedByBoldStance(Job job, IHasEntityGroup ownerOfJob, bool isBlocked)//TEMP Name
        {
            Feedback.SetJobBlockedByBoldStance(job, ownerOfJob, isBlocked);
        }



        public virtual void SetJobBlockedByThreat(Job job, IHasEntityGroup ownerOfJob, bool isBlocked)
        {
            Feedback.SetJobBlockedByThreat(job, ownerOfJob, isBlocked);
            
        }

        public virtual void DestroyAccessibility(AccessibilityFeedback accessibility)
        {
            Feedback.DestroyAccessibility(accessibility);
            
        }

        public virtual void DestroyAccessibility(JobID jobID)
        {
            Feedback.DestroyAccessibility(jobID);
        }

        public virtual void DestroyAccessibility(EntityID entityID)
        {
            Feedback.DestroyAccessibility(entityID);
        }

        public virtual void HandleDestroyedEntity(EntityID entityID)
        {
            Feedback.HandleDestroyedEntity(entityID);
        }       
      
        public virtual void SetEntityInaccessible(Allegiance agentAllegiance, IKnownEntityData entityData, bool isInaccessible)
        {
            Feedback.SetEntityInaccessible(agentAllegiance, entityData, isInaccessible);
        }

        public virtual void SetEntityBlockedByThreat(Allegiance agentAllegiance, IKnownEntityData entityData, bool blockedByThreat)
        {
            Feedback.SetEntityBlockedByThreat(agentAllegiance, entityData, blockedByThreat);
        }

        #endregion

        public void UpdateModelMatricesWithNewPositions()
        {
            Renderer.UpdateModelMatricesWithNewPositions();
        }


        public virtual void LogIsBroken(Entity entity)
        {
            if (entity.OwnedBy.HasValue)
            {
                 EntityGroup resolvedOwner;
                 if (LookUpOwners.ResolveEntityOwner(entity, out resolvedOwner)
                     && resolvedOwner.IsOwnedByAllegiance(The.InGameUI.UIAllegiance))
                 {
                     
                    Allegiance ownerAllegiance = resolvedOwner.GetAllegiance();

                    Point? mapPos = entity.MapPosition;
                    if (mapPos.HasValue && The.Map.TileIsOnMap(mapPos.Value)) // items being moved between containers have an illegal position...
                    {
                        TerrainTile tile = The.Map.GetTile(mapPos.Value);
                        if (tile.AllegiancesThatSeeThisTile.Contains(ownerAllegiance))
                        {
                            Log.AddLogEvent(The.Client.Log.EconomicEvent, entity, " is no longer functional. It has a broken part.");
                        }
                    }
                     
                 }
            }
        }

     
        public virtual void LogOutOfFuel(EntityType tool, float? requiredAmount) //Entity entity)
        {
         //   Log.AddLogEvent(The.Client.Log.EconomicEvent, null, string.Format("No fuel in inventory. Production orders using {0} cannot be completed before we have a supply of fuel", tool.Name), UWGame.ClientSide.Log.Priority.High);

            string logText;
            if (requiredAmount.HasValue)
            {
                logText = string.Format("Not enough suitable fuel in range. Production orders using {0} cannot be completed before we have at least {1:N2} BLK of suitable fuel available",
                            tool.Name, requiredAmount.Value);
            }
            else
            {
                // when does this occur?
                logText = string.Format("Not enough suitable fuel in range. Production orders using {0} cannot be completed before fuel is available",
                            tool.Name);
            }
            
            Log.AddLogEvent(The.Client.Log.EconomicEvent, null, 
                logText, UWGame.ClientSide.Log.Priority.High);

        }

        public virtual void LogKilledTarget(Entity targetAsEntity, Entity attacker)
        {
            Log.Priority priority = ClientSide.Log.Priority.Normal;
            if (targetAsEntity.Intelligence.Allegiance == The.InGameUI.UIAllegiance) 
            {
                priority = ClientSide.Log.Priority.High;
            }

            AddLogEvent(The.Client.Log.GeneralEvent, targetAsEntity, "was killed by " + attacker.ToLink() + ".", priority);
        }

        long previousDrawElapsedTotalGameTime;

        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            if (Controller.SkipRendering() == true)
            {
                return;
            }
            // it is possbile to have two Draws with same Ticks!?!?
            ElapsedTimeBetweenDraws = new TimeSpan(gameTime.TotalGameTime.Ticks - previousDrawElapsedTotalGameTime);

            previousDrawElapsedTotalGameTime = gameTime.TotalGameTime.Ticks;


            if (!The.LoadScreen.IsLoadFinished)
                return;

            if (!BeginRunWasCalled)
                return;


            MarkPerformanceTime("before client draw", Color.Wheat);

            //ScreenManager.GraphicsDevice.Clear(Color.White);
            //    DrawFire();

            if (The.InGameUI == null)
                throw new Exception("Where is our In Game UI, eh?");


          //  System.Diagnostics.Trace.WriteLine("Client.Draw()");

            UpdateModelMatricesWithNewPositions();
            MarkPerformanceTime("updateModelMatrices", Color.Tomato);

            // Lars: It tried to Draw the map before it had been loaded (TileMap == null)
            if (The.Map.TileMap == null)
            {
                return; // map not loaded yet?!?!?
            }

            The.MapUI.Draw();
            MarkPerformanceTime("MapClient.Draw", Color.Thistle);


            Renderer.Draw();
            MarkPerformanceTime("Renderer.Draw", Color.Teal);

           /* GraphicsDevice.SetRenderTarget(null);

            return;*/

            //Draw the GUI
#if DEBUG || PROFILE
            if (Kensei.Dev.Options.GetOption("Rendering.Render GUI"))
            {
                The.InGameUI.Draw(gameTime);
                MarkPerformanceTime("InGameUI.Draw", Color.Tan);

            }

#else
            The.InGameUI.Draw(gameTime);
#endif



#if DEBUG || PROFILE
            /* System.Text.StringBuilder description = new System.Text.StringBuilder("");

           description.Append("Tile Position: ");
           description.Append(entity.MapPosition.X);
           description.Append(", ");
           description.Append(entity.MapPosition.Y);
           description.Append("\n");
           description.Append("World Position: ");
           */

            PrintDebugPanelInfo();

#endif




            //if (Map.IsScrolling)
            //{
            //    spriteBatch.Begin();
            //    string fpsString = "IS SCROLLING";
            //    spriteBatch.DrawString(The.InGameUI.InterfaceFont, fpsString, new Vector2(333, 263), Color.Black);
            //    spriteBatch.DrawString(The.InGameUI.InterfaceFont, fpsString, new Vector2(332, 262), Color.White);
            //    spriteBatch.End();
            //}

            // Fire.Camera.myView = view;
            // Fire.Camera.myProjection = projection;
            // Fire.Camera.myPosition = new Vector3(cameraX, cameraY, -1000);
            // DrawFire();

            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0)
                Controller.FadeBackBufferToBlack(255 - TransitionAlpha);

            //base.Draw(gameTime);

            MarkPerformanceTime("Client.DrawDone", Color.Turquoise);


            //Sim flag? Seems to be the one that is currently used to check 
            //this on both client and sim side even tho client is the side that pauses.
            The.InGameUI.DrawPauseIcon(The.Sim.IsPaused);


#if DEBUG || PROFILE

            UpdateFrameRate();

            DrawPerformanceGraph();
#endif
        }



#if DEBUG || PROFILE

        private class TimePeriod
        {
            public string name;
            public int msec;
            public int max;
            public Color colr;
            public static int last;
        }
        private List<TimePeriod> PPList;

        private Dictionary<string, TimePeriod> PPDict;

        //private Timer timer;
        public void MarkPerformanceTime(string periodName, Color color)
        {
            
#if DEBUG || PROFILE

            if (PPList == null)
            {
                PPList = new List<TimePeriod>();
                TimePeriod.last = System.Environment.TickCount;
            }

            int now = System.Environment.TickCount;

            if (now - TimePeriod.last > 200)
            {

            }

            PPList.Add(new TimePeriod()
            {
                name = periodName,
                msec = now - TimePeriod.last,
                colr = color,
                max = 0
            });

            TimePeriod.last = System.Environment.TickCount;

#endif
        }

        private void DrawPerformanceGraph()
        {

            if (PPDict == null)
            {
                PPDict = new Dictionary<string, TimePeriod>();
            }


            if (spriteBatch == null)
                return;//must still be in loading screen

            spriteBatch.Begin();

            if (inputData != null)
            {
                Keys[] keys = inputData.GetPressedKeys();
                if (keys.Length > 0)
                {
                    spriteBatch.DrawString(The.InGameUI.InterfaceFont, keys[0].ToString(), new Vector2(The.MapUI.mapWindowWidth - 20, 120), Color.White);
                }
            }

            if (Kensei.Dev.Options.GetOption("Dev.Show FPS"))
            {
                frameCounter++;
                string fpsString = string.Format("FPS: {0}", frameRate);
               
                spriteBatch.DrawString(The.InGameUI.InterfaceFont, fpsString, new Vector2(5, 121), Color.Black);
                spriteBatch.DrawString(The.InGameUI.InterfaceFont, fpsString, new Vector2(4, 120), Color.White);

                if (The.Sim.IsPaused)
                {
                    string timeToSleepString = string.Format("Sleep(ms): {0}", (int)timeToSleep);
                    string desiredTimeToSleepString = string.Format("Target sleep(ms): {0}", (int)desiredSleepAmount);

                    spriteBatch.DrawString(The.InGameUI.InterfaceFont, timeToSleepString, new Vector2(55, 121), Color.Black);
                    spriteBatch.DrawString(The.InGameUI.InterfaceFont, timeToSleepString, new Vector2(54, 120), Color.White);

                    spriteBatch.DrawString(The.InGameUI.InterfaceFont, desiredTimeToSleepString, new Vector2(135, 121), Color.Black);
                    spriteBatch.DrawString(The.InGameUI.InterfaceFont, desiredTimeToSleepString, new Vector2(134, 120), Color.White);
                }

                foreach (KeyValuePair<string, TimePeriod> kvp in PPDict)
                {
                    TimePeriod pp = kvp.Value;
                    PPDict[kvp.Key].max = 0;
                }



                foreach (TimePeriod pp in PPList)
                {
                    //if this key already exists then lerp the current entry in the dictionary
                    if (PPDict.ContainsKey(pp.name))
                    {
                        if (pp.msec >= PPDict[pp.name].msec)
                            PPDict[pp.name].msec = pp.msec;
                        else
                            PPDict[pp.name].msec = (int)((float)PPDict[pp.name].msec * 0.98f + (float)pp.msec * 0.02f); // weak avg

                        if (PPDict[pp.name].msec > PPDict[pp.name].max)
                            PPDict[pp.name].max = PPDict[pp.name].msec;
                    }
                    else //or else append it to the dictionary
                    {
                        PPDict.Add(pp.name, pp);
                    }
                }

                //   PPList.Clear(); // moved this to prevent OutOfMemoryException


                int posY = 140;//top of graph

                foreach (KeyValuePair<string, TimePeriod> kvp in PPDict)
                {
                    TimePeriod pp = kvp.Value;

                    int size = pp.msec * 8;

                    Kensei.Dev.Shape.Box(new Vector2(0, posY += 15), new Vector2(size, posY += 20), Color.Black, pp.colr, Color.Black, pp.colr, true);
                    Kensei.Dev.Shape.Box(new Vector2(pp.max * 8, posY), new Vector2(pp.max * 8 + 4, posY - 20), Color.White, true);


                    spriteBatch.DrawString(The.InGameUI.InterfaceFont, pp.name, new Vector2(0, posY - 15), Color.White);

                    if (PPDict[kvp.Key].msec < PPDict[kvp.Key].max / 2)
                        PPDict[kvp.Key].max--;
                    if (PPDict[kvp.Key].msec > PPDict[kvp.Key].max)
                        PPDict[kvp.Key].msec--;
                }


            }

            PPList.Clear(); // moved this to prevent OutOfMemoryException

            spriteBatch.End();

        }
#else
        public void MarkPerformanceTime(string periodName, Color color)
       {
           // do nothing
       }
#endif


        public void InitializeSpriteBatch()
        {
            spriteBatch = new SpriteBatch(Controller.GraphicsDevice);
        }


        
        /// <summary>
        /// pauses all systems
        /// </summary>
        public void PauseGame()
        {
            if (The.Sim.Mode == Sim.EngineMode.Game)
            {
                Command pause = new Pause();
                Controller.StoreAndExecuteCommand(pause);
            }
        }

        public void ResumeGame()
        {
            //If sim has been destroyed we do no longer need to handle resuming it when exiting the Lose or Win screen event dialogs.
            if (The.Sim != null)
            {
                if (The.Sim.Mode == Sim.EngineMode.Game)
                {
                    Command resume = new Resume();
                    Controller.StoreAndExecuteCommand(resume);

                    /*
                    The.InGameUI.MainPanel.pauseButton.IsChecked = false;

                    AudioManager.Resume();

                    The.Sim.ResumeGame();*/
                }
            }
        }

        /// <summary>
        /// stores and executes a comman to set the speed. The command in turn calls client to show ui effects
        /// </summary>
        /// <param name="speed"></param>       
        public void SetGameSpeed(Speeds speed)
        {
            if (The.Sim.Mode == Sim.EngineMode.Game)
            {
                Command setSpeed = new SetGameSpeed(speed);
                Controller.StoreAndExecuteCommand(setSpeed);
            }
        }


        public void OnSetSpeed(Speeds speed)
        {
            The.InGameUI.MainPanel.OnSetSpeed(speed);
        }
       
        public void OnPause()
        {
            The.InGameUI.MainPanel.OnPause();
            
            Controller.AudioManager.Pause();
            AudioManager.Pause();
        }


        public void OnResume()
        {
            The.InGameUI.MainPanel.OnResume();

            Controller.AudioManager.Resume();
            AudioManager.Resume();
        }
        /*
        public void OnPause()
        {
            The.InGameUI.MainPanel.btPause.IsChecked = true;
            AudioManager.Pause();
        }


        public void OnResume()
        {
            The.InGameUI.MainPanel.btPause.IsChecked = false;
            AudioManager.Resume();
        }*/

        System.Text.StringBuilder description = new System.Text.StringBuilder("");
        private void PrintDebugPanelInfo()
        {
            description.Clear();
            //System.Text.StringBuilder description = new System.Text.StringBuilder("");

            description.Append("Time: ");
            description.Append(The.Sim.DateAndTime.TimeOfDay);
            description.Append("\n");
            description.AppendLine("TotalUnPausedGameTimeInSeconds: " + The.Sim.TotalUnPausedGameTimeInSeconds);
            description.AppendLine("Total days since start (1): " + The.Sim.TotalUnPausedGameTimeInSeconds / DateAndTime.secondsPerDay);
            description.AppendLine("Total days since start (2): " + (The.Sim.DateAndTime.CurrentTimeDateYear.TotalDays - The.Sim.DateAndTime.StartTimeDateYear.TotalDays));
            description.Append("Mouse world location: ");
            description.Append(The.MapUI.MouseWorldLocation);
            description.Append("\n");
            description.Append("Mouse tile position: ");
            description.Append(The.MapUI.MouseTilePosition);
            description.Append("\n");
            description.Append("Mouse subtile position: ");
            description.Append(The.MapUI.MouseSubtilePosition); // MapManager.WorldPosToSubtile(The.MapUI.MouseWorldLocation));
            description.Append("\n");
            description.Append("\n");

           /* if (The.InGameUI.SelectedEntity == null)
            {
                Kensei.Dev.Options.SetThreatLabelText("No selected entity");
            }*/

            if (The.InGameUI.SelectedEntity == null
                && The.InGameUI.SelectedTiles.Count > 0)
            {
                //Kensei.Dev.Options.SetEntityInfo("No Entity Selected");

                //TerrainTile firstTile = The.InGameUI.SelectedTiles.Coverage[0];
                The.InGameUI.SelectedTiles.HandleFirstTile(firstTile =>
                {
                    description.Append("Tile Position: ");
                    description.Append(firstTile.X);
                    description.Append(", ");
                    description.Append(firstTile.Y);
                    description.Append(" (");
                    description.Append(firstTile.X * 48);
                    description.Append(", ");
                    description.Append(firstTile.Y * 48);
                    description.Append(")");
                    description.Append("\n");
                });


                Kensei.Dev.Options.SetEntityInfoText(description.ToString());
            }
            else if (The.InGameUI.SelectedEntity != null)
            {
                //Entity entity = The.InGameUI.SelectedEntity as Entity;
                Entity entity = Entity.FindByID(The.InGameUI.SelectedEntity.Value); // as Entity;

                if (entity == null)
                {
                    string entityInFOW = "Entity is in FOW";
                   // Kensei.Dev.Options.SetThreatLabelText(entityInFOW);
                    Kensei.Dev.Options.SetEntityInfoText(entityInFOW);
                    return;
                }

            /*    string threatStance = "";
                if (entity.Intelligence != null)
                {
                     threatStance = "Current Threat Stance: " + entity.Intelligence.ThreatStance.ToString();
                }
                Kensei.Dev.Options.SetThreatLabelText(threatStance);
                */
                PrintEntityInfo(description, entity);
                /*
                Intelligence intelligenceComponent;
                if (entity.Find(out intelligenceComponent))
                {
                    if (entity.EntityType.BiologicalType != null)
                    {
                        PrintEntityGoals(intelligenceComponent);
                    }

                }
                */
                if (entity.Intelligence != null)
                {
                    description.Append("\nEntity\n");
                    description.Append(entity.Intelligence.Statistics.ToString() + "\n");
                }

                if (entity.AllegianceID != null)
                {
                    if (UWGame.SimSide.Snapshots.LookUp<Allegiance, AllegianceID>.FindByID(entity.AllegianceID).Statistics != null)
                    {
                        description.Append("Allegiance\n");
                        description.Append(UWGame.SimSide.Snapshots.LookUp<Allegiance, AllegianceID>.FindByID(entity.AllegianceID).Statistics.ToString() + "\n");
                    }
                }

                if (entity.EntityType.BiologicalType != null)
                {
                    UWGame.SimSide.Entities.Biological.BiologicalEntity bioEntity = entity.BiologicalEntity;

                    if (bioEntity.RaceType != null)
                    {
                        description.Append("Race: ");
                        description.Append(bioEntity.RaceType.KeyName);
                        description.AppendLine("");
                    }

                    description.Append("Caste: ");
                    description.Append(bioEntity.CasteType.KeyName);
                    description.AppendLine("");

                    description.Append("Age: ");
                     // use 1 decimals:
                    description.Append(bioEntity.AgeGroup.Age.ToString("N1"));
                    description.Append(" - ");
                    description.Append(bioEntity.AgeGroup.AgeGroupType.Name);
                    description.AppendLine("");

                }

             //   System.Text.StringBuilder states = new System.Text.StringBuilder("");

                Locomotor locomotor;
                if (entity.Find(out locomotor))
                {
                    if (locomotor.Stance != null)
                    {
                        description.Append("\nSim stance: ");
                        description.Append(locomotor.Stance.CurrentStance.ToString() + "\n\n");

                    }
                }

               
                if (entity.Intelligence != null)
                {
                    description.Append("Threat stance");
                    description.Append(": " + entity.Intelligence.ThreatStance.ToString() + "\n");
                }

                if (entity.Renderable != null)
                {
                    Renderable renderable = entity.Renderable;

                    if (renderable.RenderAsModel != null)
                    {
                        PrintAnimStates(description, entity, renderable);
                    }
                    else
                    {
                        renderable.PrintStaticStates(description);
                    }

                    //description.Append(states.ToString());
                }

                entity.PrintScriptVariables(description);


                Intelligence intelligenceComponent;
                if (entity.Find(out intelligenceComponent))
                {
                    if (entity.EntityType.BiologicalType != null)
                    {
                        PrintEntityGoals(intelligenceComponent, description);
                    }

                }
            }
            
            Kensei.Dev.Options.SetEntityInfoText(description.ToString());

            description.Clear();

            PrintGlobalScriptVariables(description);

            PrintGlobalConditions(description);

            PrintEventsInfo(description);

            string currentTab = Kensei.Dev.Options.ShownTab();

            switch(currentTab) // TODO - unfinished
            {
                case "Performance":
                    PrintPerformance();
                    break;
                case "Jobs":
                    PrintJobs();
                    break;
                case "Sounds":
                    PrintSounds();
                    break;

            }

            Kensei.Dev.Options.SetEventsText(description.ToString());

            PopulateAllegiances();            

          //  PrintEmigrateRolls();
        }

        Regulator printAllegiancesRegulator;

        Regulator printPerformanceRegulator;
        Regulator printEmigrateRollsRegulator;

        private void PrintSounds()
        {
            Kensei.Dev.Options.SetSoundsText(AudioManager.PrintSoundsForDebug());

        }

        private void PrintEmigrateRolls()
        {
            if (printEmigrateRollsRegulator.IsReady())
            {
                StringBuilder text = new StringBuilder();

                int total = EmigrateDecider.EmigrateRollFrequency.Sum();
                text.AppendLine("Total rolls: " + total);
                text.AppendLine("Members: " + The.InGameUI.UIAllegiance.Persons.Count);

                text.AppendLine("Target rolls per day, per person:" + DateAndTime.secondsPerDay / GameData.Instance.AIConstants.EmigrateDeciderUpdateIntervalInSeconds);
                text.AppendLine("Target rolls per day, for " + The.InGameUI.UIAllegiance.Persons.Count + " persons:" + The.InGameUI.UIAllegiance.Persons.Count * DateAndTime.secondsPerDay / GameData.Instance.AIConstants.EmigrateDeciderUpdateIntervalInSeconds);


                text.AppendLine("Distribution of emigrate random numbers:");

                double interval = 1d / EmigrateDecider.EmigrateRollFrequency.Length;
                for (int ctr = EmigrateDecider.EmigrateRollFrequency.GetLowerBound(0); ctr <= EmigrateDecider.EmigrateRollFrequency.GetUpperBound(0); ctr++)
                {
                    double from = ctr * interval;
                    double to = (ctr + 1) * interval - Common.epsilon;
                    text.AppendFormat("{0:N3} - {1:N3}       {2}", from, to, EmigrateDecider.EmigrateRollFrequency[ctr]);
                   // text.AppendFormat("0.{0}0-0.{0}9       {1}", ctr, EmigrateDecider.EmigrateRollFrequency[ctr]);
                    text.AppendLine();
                }

                Kensei.Dev.Options.SetEmigrateRollText(text.ToString());
            }
        }

        private void PrintPerformance()
        {
            //Kensei.Dev.Options.RemoveOption()

            if (printPerformanceRegulator.IsReady())
            {
                StringBuilder description = new StringBuilder();

                The.Sim.CycleManager.PrintPerformance(description);


                Kensei.Dev.Options.SetPerformanceText(description.ToString());
            }
        }

        int jobWidth = 10;
        int outputWidth = 14;
        int impWidth = 7;
        int scoreWidth = 7;
        int scoreNoToolsWidth = 7;
        int activeWidth = 8;
        int takerScoreWidth = 8;

        private void PrintJobs()
        {
            /* if (printJobsRegulator.IsReady())
             {*/
            StringBuilder description = new StringBuilder();

            //The.Sim.CycleManager.PrintPerformance(description);

           

            EntityGroup owner = LookUp<EntityGroup, EntityGroupID>.FindByID(The.InGameUI.UIOwner);
            if (owner != null)
            {
                description.Append("Job".PadRight(jobWidth));
                description.Append("Output".PadRight(outputWidth));
                description.Append("Imp.".PadRight(impWidth));
                description.Append("Sco.".PadRight(scoreWidth));
                description.Append("No tls".PadRight(scoreNoToolsWidth));
                description.Append("Active".PadRight(activeWidth));
                description.Append("Taker sc.".PadRight(takerScoreWidth));
             
                description.AppendLine();

                List<ProcessJob> pJobs = new List<ProcessJob>();
                foreach (var item in owner.ProductionJobs)
                {
                    foreach (var job in item.Value)
                    {
                        pJobs.Add(job);
                    }
                }

                var processJobs = pJobs.OrderByDescending(j => j.DebugScore);


                description.AppendLine("PRODUCTION/GATHER");
                foreach (var job in processJobs)
                {
                    AppendProcessJob(description, owner, job);
                }

                description.AppendLine();
                description.AppendLine("HAULING");

                var haulJobs = owner.HaulingJobs.OrderByDescending(j => j.DebugScore);
                foreach (HaulingJob job in haulJobs)
	            {
                    HaulingJobAnyItemOfType anyJob = job as HaulingJobAnyItemOfType;
                    HaulingJobSpecificItem specJob = job as HaulingJobSpecificItem;

                    string name;
                    if (anyJob != null)
                    {
                        name = "Haul any" ;
                    }
                    else
                    {
                        name = "Haul";
                    }
                    description.Append(name.Truncate(jobWidth).PadRight(jobWidth));

                    string item = "";
                    if (job.Item.HasValue)
                    {
                        IKnownEntityData itemData;
                        The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(job.Item.Value, out itemData);
                        if (itemData != null)
                        {
                            item = itemData.EntityType.Name;
                        }
                    }
                    else if (anyJob != null)
                    {
                        item = anyJob.RequiredItemType.Name;
                    }

                    description.Append(item.Truncate(outputWidth - 1).PadRight(outputWidth));

                    double materialUrgencyScore = EvaluateHaulingJobs.ScoreJobMaterialUrgency(job, owner);
                    description.Append(materialUrgencyScore.ToString("N2").PadRight(impWidth));      
                    description.Append(job.DebugScore.ToString("N3").PadRight(scoreWidth));
                    description.Append(job.DebugScoreNoTools.ToString("N3").PadRight(scoreNoToolsWidth));                      
                    
                    bool isActive = job.TakenBy.Count > 0;
                    description.Append(isActive.ToString().PadRight(activeWidth));

                    AppendTakerScore(description, takerScoreWidth, job);

                    description.AppendLine();
                }

                description.AppendLine();
                description.AppendLine("OTHER");

                var otherJobs = owner.OtherJobs.OrderByDescending(j => j.DebugScore);
                foreach (Job job in otherJobs)
                {
                    AppendJob(description, job);

                }

                List<ProcessJob> repairJobs = new List<ProcessJob>();
                foreach (var item in owner.RepairJobs)
                {
                    foreach (var job in item.Value)
                    {
                        repairJobs.Add(job);
                    }
                }

                if (repairJobs.Count > 0)
                {
                    var orderedRepairJobs = repairJobs.OrderByDescending(j => j.DebugScore);

                    description.AppendLine();
                    description.AppendLine("REPAIR");
                    foreach (var job in orderedRepairJobs)
                    {
                        AppendProcessJob(description, owner, job);
                    }
                }

                if (owner.FindPreyJobs.Count > 0)
                {
                    description.AppendLine();
                    description.AppendLine("FIND PREY");
                    var orderedFindPreyJobs = owner.FindPreyJobs.OrderByDescending(j => j.DebugScore);
                    foreach (FindPreyJob job in orderedFindPreyJobs)
                    {
                        double materialUrgencyScore = job.ScorePreyImportance(owner); // EvaluateHaulingJobs.ScoreJobMaterialUrgency(job, owner);
                      
                        AppendJob(description, job, materialUrgencyScore);
                    }
                }

                if (owner.ScoutingJobs.Count > 0)
                {
                    description.AppendLine();
                    description.AppendLine("SCOUT");
                    var orderedJobs = owner.ScoutingJobs.OrderByDescending(j => j.DebugScore);
                    foreach (var job in orderedJobs)
                    {
                        AppendJob(description, job);
                    }
                }

                if (owner.PatrolJobs.Count > 0)
                {
                    description.AppendLine();
                    description.AppendLine("PATROL");
                    var orderedJobs = owner.PatrolJobs.OrderByDescending(j => j.DebugScore);
                    foreach (var job in orderedJobs)
                    {
                        AppendJob(description, job);
                    }
                }

                if (owner.CheckProcessJobs.Count > 0)
                {
                    description.AppendLine();
                    description.AppendLine("CHECK PROCESS");
                    var orderedJobs = owner.CheckProcessJobs.OrderByDescending(j => j.DebugScore);
                    foreach (var job in orderedJobs)
                    {
                        AppendJob(description, job);
                    }
                }

                if (owner.AttackAreaJobs.Count > 0)
                {
                    description.AppendLine();
                    description.AppendLine("ATTACK");
                    var orderedJobs = owner.AttackAreaJobs.OrderByDescending(j => j.DebugScore);
                    foreach (var job in orderedJobs)
                    {
                        AppendJob(description, job);
                    }
                }
            }

            Kensei.Dev.Options.SetJobsText(description.ToString());
            // }

        }

        private void AppendJob(StringBuilder description, Job job, double? importance = null)
        {
            string name = job.GetName();

            description.Append(name.Truncate(jobWidth).PadRight(jobWidth));

            description.Append("".PadRight(outputWidth));

            //  double materialUrgencyScore = EvaluateHaulingJobs.ScoreJobMaterialUrgency(job, owner);
            //  description.Append(materialUrgencyScore.ToString("N2").PadRight(impWidth));
            if (importance.HasValue)
            {
                description.Append(importance.Value.ToString("N2").PadRight(impWidth));   
            }
            else
            {
                description.Append("".PadRight(impWidth));
            }
            description.Append(job.DebugScore.ToString("N3").PadRight(scoreWidth));
            description.Append(job.DebugScoreNoTools.ToString("N3").PadRight(scoreNoToolsWidth));

            bool isActive = job.TakenBy.Count > 0;
            description.Append(isActive.ToString().PadRight(activeWidth));

            AppendTakerScore(description, takerScoreWidth, job);

            description.AppendLine();
        }

        private void AppendProcessJob(StringBuilder description, EntityGroup owner, ProcessJob job)
        {
            description.Append(job.GetName().Truncate(jobWidth).PadRight(jobWidth));

            string name;
            if (job.ProcessType.HasOutput)
            {
                name = job.ProcessType.Outputs[0].FinalEntityTypeToCreate.Name;
            }
            else
            {
                name = job.ProcessType.Name;
            }
            description.Append(name.Truncate(outputWidth - 1).PadRight(outputWidth));

            description.Append(job.GetImportance(owner).ToString("N2").PadRight(impWidth));
            description.Append(job.DebugScore.ToString("N3").PadRight(scoreWidth));
            description.Append(job.DebugScoreNoTools.ToString("N3").PadRight(scoreNoToolsWidth));

            bool isActive;
            JobsPanel.GetProgressTint(job, out isActive);
            description.Append(isActive.ToString().PadRight(activeWidth));

            AppendTakerScore(description, takerScoreWidth, job);

            description.AppendLine();
        }

        private static void AppendTakerScore(StringBuilder description, int takerScoreWidth, Job job)
        {
            double? takerScore = job.GetTakerScore();
            if (takerScore.HasValue)
            {
                description.Append(takerScore.Value.ToString("N3").PadRight(takerScoreWidth));
            }
        }

        private void ResetPerformanceCounters(string option, bool? newBool, float? newFloat)
        {
            EventManager.totalComputationAllInstancesInSeconds = 0;
            MovementMap.totalComputationAllInstancesInSeconds = 0;
            PathPlanner.totalComputationAllInstancesInSeconds = 0;
            RegionSearchPlanner.totalComputationAllInstancesInSeconds = 0;
            HaulingJobManager.totalComputationAllInstancesInSeconds = 0;
            RegionMap.totalComputationAllInstancesInSeconds = 0;           

        }
       
        private static void PrintGlobalScriptVariables(System.Text.StringBuilder description)
        {
            description.AppendLine("Global (site) variables:");
            The.Sim.PlaySite.PrintGlobalScriptVariables(description);

        }

        private static void PrintGlobalConditions(System.Text.StringBuilder description)
        {
            description.AppendLine();
            description.AppendLine("Allegiance conditions:");
            The.Sim.PlaySite.EventManager.PrintGlobalConditions(description);

        }

        private static void PrintEventsInfo(System.Text.StringBuilder description)
        {
            description.AppendLine();
            description.AppendLine("Time: " + The.Sim.TotalUnPausedGameTimeInSeconds);
            The.Sim.PlaySite.EventManager.GetCurrentEvents(description);

        }

        private static void PrintAnimStates(System.Text.StringBuilder states, Entity entity, Renderable renderable)
        {
            states.Append("\nActual AnimationStateFlags:\n");
            states.Append(entity.Renderable.AnimConditions.ToString());

            if (renderable.RenderAsModel.SelectedAnimInfo != null)
            {
                states.Append("\nBest Match Conditions: ");
                if (entity.Renderable.AnimConditions.Equals(renderable.RenderAsModel.SelectedAnimInfo.ConditionSet))
                {
                    states.Append("PERFECT");
                }
                states.Append("\n");

                if (renderable.RenderAsModel.SelectedAnimInfo.ConditionSet != null)
                {
                    states.Append(renderable.RenderAsModel.SelectedAnimInfo.ConditionSet.ToString());
                }

                if (renderable.RenderAsModel.SelectedAnimInfo.Forbiddens != null)
                {
                    if (renderable.RenderAsModel.SelectedAnimInfo.Forbiddens.Any())
                    {
                        states.Append("\nBest Match Forbiddens:\n     ");
                        states.Append(renderable.RenderAsModel.SelectedAnimInfo.Forbiddens.StateNames);
                    }
                }
               
                states.Append("\n");

                renderable.RenderAsModel.AppendAnimDebugInfo(/*keyname,*/ states);

            }
            else
                states.Append("\n\n   MATCH FAILED -- curAnimInfo is null -- this is bad.\n\n");
        }

        

        private static void PrintEntityGoals(Intelligence intelligenceComponent, StringBuilder goals)
        {
            //System.Text.StringBuilder goals = new System.Text.StringBuilder("");
            goals.Append("\n");
            goals.Append(intelligenceComponent.Brain.ComposeIndentedString(""));

            goals.Append("Current goal score: ");
            goals.Append(intelligenceComponent.GetCurrentGoalUtility());
            goals.Append("\n");
            goals.Append("Score | Goal");
            goals.Append("\n");
            for (int i = 0; i < intelligenceComponent.TopScoringJobs.Count; i++)
            {
                goals.Append(intelligenceComponent.TopScoringJobs[i].Score.ToString("{0.0000}"));
                goals.Append(" ");
                goals.Append(intelligenceComponent.TopScoringJobs[i].Goal);
                goals.Append("\n");
            }
            goals.Append("\n");

           // Kensei.Dev.Options.SetEntityGoalsText(goals.ToString());
        }

        private static void PrintEntityInfo(System.Text.StringBuilder description, Entity entity)
        {
            if (entity.PersonEntity != null)
            {
                description.Append(entity.Name);
                description.Append(", Household: ");
                description.Append(entity.PersonEntity.Household.ID.ToString());
                description.Append(", Home: ");
                description.Append(entity.PersonEntity.Household.Home != null ?
                    entity.PersonEntity.Household.Home.Value.ToString() : "None");
                description.Append("\r\n\r\n");
            }
            description.Append("ID: ");
            description.Append(entity.EntityID);
            description.Append("\n");
            description.Append("Name: ");
            description.Append(entity.Name);
            description.Append("\n");
            description.Append("Assigned to job: ");
            description.Append(entity.AssignedToJob);
            description.Append("\n");
            EntityID? inuseBy = The.InGameUI.UIAllegiance.SharedKnowledge.GetInUseBy(entity.ID);
            if (inuseBy.HasValue)
            {
                description.Append("In use by: ");
                description.Append(inuseBy.Value);
                description.Append("\n");
            }
            if (entity.ContainedBy.HasValue)
            {
                description.Append("Contained by: ");
                description.Append(entity.ContainedBy.Value);
                description.Append("\n");
            }
            if (entity.IsOnPlaySite())
            {
                description.Append("Tile Position: ");
                description.Append(entity.MapPosition.Value);
                description.Append("\n");
                description.Append("World Position: ");
                description.Append(entity.PlaySiteLocation.X);
                description.Append(", ");
                description.Append(entity.PlaySiteLocation.Y);
                description.Append(", ");
                description.Append(entity.PlaySiteLocation.Z);
                Point subtile = MapManager.WorldPosToSubtile(entity.PlaySiteLocation);
                if (The.Map.SubtileIsCompletelyBlocked(The.Map.TerrainCosts[SurfaceType.TransportType.Foot], subtile))
                {
                    description.Append("\n");
                    description.Append("\n");
                    description.Append("Subtile " + subtile.ToString() + " is blocked!");
                    description.Append("\n");
                    description.Append("\n");
                }

                description.Append("Screen Position: ");
                Vector2 screenPos = The.MapUI.WorldPosToScreen(entity.PlaySiteLocation);
                description.Append(screenPos.X);
                description.Append(", ");
                description.Append(screenPos.Y);
                description.Append("\n");
            }           

            description.Append("\n");
            description.Append("Time of day: ");
            description.Append(The.Sim.DateAndTime.TimeOfDay);
           /* description.Append(", Day: ");
            description.Append(The.Sim.DateAndTime.GetDayNo());*/
            description.Append(", Time of year: ");
            description.Append(The.Sim.DateAndTime.TimeOfYear);
            description.Append("\n");          

            NonLivingEntity nonLiving;
            if (entity.Find(out nonLiving))
            {
                description.Append("\n");
                description.Append("Condition: ");
                description.Append(nonLiving.Condition);
                description.Append("\n");
                description.Append("Max condition: ");
                description.Append(nonLiving.MaxCondition);
                description.Append("\n");
            }

            if (entity.Intelligence != null && entity.Intelligence.Brain != null)
            {
                description.Append("Detect agents factor: ");
                description.Append(entity.Intelligence.Brain.GetDetectAgentsFactor(null, false));
                description.Append("\n");
                description.Append("Detect resources factor: ");
                description.Append(entity.Intelligence.Brain.GetDetectResourcesFactor(null, false));
                description.Append("\n");

                if (entity.BiologicalEntity != null) // ??
                {
                    description.Append("Exertion level: ");
                    description.Append(entity.Intelligence.Brain.GetExertionLevelOfActivity());
                    description.Append("\n");
                    description.Append("Stealth factor: ");
                    description.Append(entity.Intelligence.Brain.GetStealthFactor());
                    description.Append("\n");               
                }
            }

            if (entity.BiologicalEntity != null)
            {
                description.Append("Energy level: ");
                description.Append(entity.BiologicalEntity.EnergyLevel);
                description.Append("\n");               
                description.Append("Stomach content: ");
                description.Append(entity.BiologicalEntity.StomachContents);
                description.Append("\n");
                foreach (var need in entity.BiologicalEntity.Needs.NeedsList)
                {
                    description.Append(need.Value.NeedType.KeyName); // System.Enum.GetName(typeof(AINeedClass), need.Value.NeedType.NeedClass));
                    if (need.Value.NeedType.FoodNeedType != null) //PhysicalNeed != null)
                    {
                        description.Append("(" + need.Value.NeedType.FoodNeedType.FoodNutrient + ")");
                    }
                    description.Append(": " + need.Value.CurrentLevel);

                    if (need.Value.PhysicalNeed != null)
                    {
                        if (need.Value.PhysicalNeed.DaysAtZero > 0f)
                        {
                            description.Append(", days at zero: ");
                            description.Append(need.Value.PhysicalNeed.DaysAtZero);
                        }
                    }

                    description.Append("\n");

                }

                description.Append("\n");
            }
        }

       // bool dlgIsDisplayed = false;
      
        /// <summary>
        /// is only called ingame and on loading screen, not on the main menu
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="otherScreenHasFocus"></param>
        /// <param name="coveredByOtherScreen"></param>
        public override void Update(Microsoft.Xna.Framework.GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {

            // cheesy way of keeping focus for client
            //pass false in for both params, here
            base.Update(gameTime, false, false);

            if (!The.LoadScreen.IsLoadFinished)
                return;

            bool limitFPSWhenPaused = Controller.Options.LimitFramerateWhenPaused;

#if DEBUG || PROFILE
            if (Kensei.Dev.Options.GetOption("Dev.Show FPS"))
            {
               // UpdateFrameRate();
            }

            limitFPSWhenPaused = Kensei.Dev.Options.GetOption("Dev.Limit FPS when paused");
#endif

          //  return;

#if PROFILE //TODO need to send message to sim to pause also, MLo
           // test for CPU or GPU BOUND!!! (shawn hargreaves)
           // we are cpu bound it seems...
           // http://blogs.msdn.com/b/shawnhar/archive/2008/04/07/how-to-tell-if-you-are-cpu-or-gpu-bound.aspx

           if (inputData.IsKeyDown(Keys.PageUp)) // TODO Send message to Sim to go slow-mo?
               Thread.Sleep(2);

           if (inputData.IsKeyDown(Keys.PageDown)) // Send Message to Sim to suspend its update?
               return;
#endif


            // cheesy way of keeping focus for client
            //pass false in for both params, here
            //   base.Update(gameTime, false, false);

           // System.Diagnostics.Trace.WriteLine("Client.Update() #1");

            if (IsActive)
            {
                //******* IMPORTANT - about GameTime **********************
                // GameTime.ElapsedGameTime - time passed since last XNA Update
                // always OK to use.

                // GameTime.TotalGameTime - total time passed since the start of the game
                // BEWARE - this includes the time where the player paused the game. So for many simulation tasks this is wrong to use.
                // use the global variable TotalUnPausedGameTime instead! This number is not increased when the game is paused.

                // use this for interface animations which never pause
                // TODO, need to separate Sim time from Client Time
                //Client time is constant with real time, while
                //Sim time can be stopped, sped-up rewound, etc.

              //  System.Diagnostics.Trace.WriteLine("Client.Update() #2");

                frameCount++;
                this.GameTime = gameTime;

                if (!BeginRunWasCalled)
                {
                    BeginRun(); // runs on first frame when doing ingame save/load
                }

                The.InGameUI.Update(gameTime); // <- game can have ended here, in the last modal dialog. If Sim has 0 TransitionOff time, it will be destroyed.

                if (The.Sim == null)
                {
                    return;
                }

               /* if (!dlgIsDisplayed && The.Sim.TotalUnPausedGameTimeInSeconds > 5)
                {
                    dlgIsDisplayed = true;
                    EventActionDialog.ShowAndSaveEventDialog(null, "sdfsd", "sdfsdfsd", true);
                }*/

                The.MapUI.Update(gameTime);


                if (!The.Sim.IsPaused)
                {
                    ParticleManager.Update();

                    UpdateRenderables(gameTime);

                    Feedback.Update(gameTime);
                   // accessibilityFeedback.Update(gameTime);
                }
                else
                {
                    LimitFPS(limitFPSWhenPaused);
                }

                Kensei.Dev.Manager.Update();

                // call this after Renderable.Update:
                AudioManager.Update(gameTime);
                

                The.MapUI.UpdateMouseInMap();

                if (The.InGameUI.gui.IsMouseInInterface(inputData.mouseX, inputData.mouseY))
                {
                    The.MapUI.TryMapScrolling(); // why only if mouse is over gui interface???
                }
                
            } //endif isactive

            The.Client.MarkPerformanceTime("Client update", Color.CadetBlue);

        }//end update


        float timeToSleep = 0;
        float desiredSleepAmount;
        private int frameCount = 0;

       // const float frameTimeAtTargetFPS = 1f / targetFrameRateWhenPaused; // 0.0333f;
       
        /// <summary>
        /// saves on CPU while paused
        /// </summary>
        /// <param name="limitFPSWhenPaused"></param>
        private void LimitFPS(bool limitFPSWhenPaused)
        {
            if (limitFPSWhenPaused && frameRate > 0)
            {
                int excessFramerate = frameRate - Controller.Options.TargetFramerateWhenPaused;

                if (Math.Abs(excessFramerate) > 0) // excessFramerate > 0)
                {
                    float currentFrameTime = 1f / frameRate;

                    float frameTimeAtTargetFPS = 1f / Controller.Options.TargetFramerateWhenPaused;

                    desiredSleepAmount = (frameTimeAtTargetFPS - currentFrameTime) * 1000 * 2f; // fudge factor

                    float sleepAmountDifference = desiredSleepAmount - timeToSleep;

                    if (Math.Abs(sleepAmountDifference) > 0)
                    {
                        timeToSleep = timeToSleep + 0.04f * sleepAmountDifference; // lerp towards desired sleep amount

                        timeToSleep = Common.Clamp(timeToSleep, 0f, 50f);
                    }

                    Thread.Sleep((int)timeToSleep); //50); 
                }
                else
                {
                    timeToSleep = 0;
                }
            }
            else
            {
                timeToSleep = 0;
            }
        }


        /// <summary>
        /// client
        /// </summary>
        /// <param name="detectable"></param>
        /// <param name="resourceDetectionFactor"></param>
        /// <param name="detectingEntity"></param>
        public virtual void GiveDetectionFeedback(IDetectable detectable, DetectionFactor resourceDetectionFactor, Entity detectingEntity, Allegiance allegiance)
        {
            Entity asEntity = detectable as Entity;

            bool requiresDetection = detectable.RequiresRollToDetect(); // don't log auto-spotted entites (structures, items...)


            bool addResourceLogMessage = false;
            if (resourceDetectionFactor != null)
            {
                addResourceLogMessage = resourceDetectionFactor.AddLogMessageWhenDetected;
            }


            SetFlashing(detectable);


            if (detectingEntity != null
                && (asEntity != null || addResourceLogMessage)
                && requiresDetection
                && (asEntity == null || LogEntity(asEntity, allegiance)))
            {
                // TODO: don't log when picking up a harvested item...
                AddLogEvent(detectingEntity.Intelligence.Allegiance, The.Client.Log.GeneralEvent, detectingEntity, string.Format("has spotted {0}", detectable.ToLink()));
            }

            /* else // for debugging...
             {
                 if (detectingEntity != null
                     && asEntity != null
                     && requiresDetection)
                 {
                     //The.Client.AddLogEvent(The.Client.Log.DebugEvent, detectingEntity, string.Format("has spotted {0} (DEBUG)", detectable));

                 }
             }*/
        }

        private bool LogEntity(Entity entity, Allegiance allegiance)
        {
            if (!allegiance.SharedKnowledge.PlaySiteKnowledge.SpottedAnimals.Contains(entity.EntityType) // always log animals when the species is first spotted
                 || (entity.EntityType.IntelligenceType != null && entity.EntityType.IntelligenceType.IsPredator)) // always log predators
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// client
        /// </summary>
        /// <param name="detectable"></param>
        private void SetFlashing(IDetectable detectable)
        {
            ResourceContainer resource = detectable as ResourceContainer;

            if (resource != null)
            {
                resource.FlashAsDetected();
            }

            Entity asEntity = detectable as Entity;

            if (asEntity != null && asEntity.Renderable != null && asEntity.IsOnPlaySite())
            {
                asEntity.Renderable.SetToParentLocation();
                asEntity.Renderable.FlashAsDetected();
            }

        }


        /// <summary>
        /// will only log if the allegiance is the UIAllegiance
        /// </summary>
        /// <param name="loggingAllegiance"></param>
        /// <param name="eventType"></param>
        /// <param name="concernedEntity"></param>
        /// <param name="eventText"></param>
        /// <param name="priority"></param>
        public virtual void AddLogEvent(Allegiance loggingAllegiance, EventType eventType, Entity concernedEntity, string eventText, UWGame.ClientSide.Log.Priority? priority = null) 
        {
            if (The.InGameUI.UIAllegiance == loggingAllegiance)
            {
                Log.AddLogEvent(eventType, concernedEntity, eventText, priority);
            }
        }

        public virtual void AddLogEvent(EventType eventType, Entity concernedEntity, string eventText, UWGame.ClientSide.Log.Priority? priority = null)
        {
            Log.AddLogEvent(eventType, concernedEntity, eventText, priority);            
        }

        public virtual void AddRenderable(Renderable renderable)
        {
            renderables.Add(renderable);           
        }

        public virtual void RemoveRenderable(Renderable renderable)
        {
            renderables.Remove(renderable);

        }

        public bool MapHasMoved = true;


        private void UpdateRenderables(GameTime gameTime)
        {
            /*  if (MapHasMoved)
            {
                // wake up on-screen renderables:
               int lastXToDraw, lastYToDraw, firstXToDraw, firstYToDraw;
                 Renderer.GetEdgesOfTerrainToDraw(out lastXToDraw, out lastYToDraw, out firstXToDraw, out firstYToDraw);


                TerrainTile tileToDraw;
                for (int y = firstYToDraw; y < lastYToDraw; y++)
                {
                    for (int x = firstXToDraw; x < lastXToDraw; x++)
                    {

                    }
                }


                //re-sort renderables:


                MapHasMoved = false;
            }*/

            renderables.Update(gameTime);

        }

        /// <summary>
        /// TODO: do a proper update???
        /// </summary>
      /*  private void UpdateRenderablesExpiry()
        {
            SleepyUpdater.UpdateExpiring(renderablesWithoutEntity, ref renderablesAreDirty,
                r => { r.StartFadingOut(true);    // destroy after fading out first..           
                return false; // not removed yet... we want to fade out first.
                }); 
        }*/

        //public static void DestroyRenderable

        /// <summary>
        /// AFTER Initialize
        /// Load your graphics content.  
        /// </summary> 
        public override void LoadContent()
        {
            FlatSpriteSheet = Content.Load<SpriteSheet>("FlatSprites");

            SetupRoadPieceSprites();


            The.InGameUI.LoadContent();

            //spriteBatch = new SpriteBatch(ScreenManager.GraphicsDevice);//constructed just-in-time, now, in Draw()

            // Copy the absolute transformation of each node
            //   PersonEntityType.Instance.AbsoluteBoneTransforms = new Matrix[PersonEntityType.Instance.SkinnedModel.Model.Bones.Count];
            //   PersonEntityType.Instance.SkinnedModel.Model.CopyBoneTransformsTo(PersonEntityType.Instance.AbsoluteBoneTransforms);

            EdgeDetectEffect = Content.Load<Effect>("EdgeDetect");
            //ReplaceColorEffect = Content.Load<Effect>("colorreplace");

            //    primitiveBatch = new PrimitiveBatch(ScreenManager.GraphicsDevice);//constructed just-in-time, now

            //           this.BeginRun(); //MLo, not here, BeginRun must happen after both client and Sim have finished Init()
            
            Renderer.LoadContent();
            
            RoundLineManager.LoadContent(GraphicsDevice, Content);

            quadRenderer.LoadContent();


         
        }

        public bool BeginRunWasCalled
        {
            get;
            private set;
        }

        /// <summary>
        /// AFTER Initialize AND LoadContent.
        /// Call only by main thread!
        /// </summary>
        public void BeginRun()
        {

            // this debug system is only needed in Release mode also
            //because we use the rendering methods for fog of war...
            Dimension dim = Controller.DrawArea;
            Kensei.Dev.Manager.Initialise(The.Client.Content, The.Client.GraphicsDevice, 0, 0, dim.Width, dim.Height);

#if DEBUG || PROFILE
            InitDeveloperDialog(); // we need some placed entities for some of the options...           
#endif

            The.InGameUI.SetInterfaceCursor();

            The.InGameUI.PostLoadContent();

            Renderer.PostLoadContent();

            Renderer.InitAfterMapLoad();

            BeginRunWasCalled = true;
        }



        /// <summary>
        /// Unload your graphics content.  If unloadAllContent is true, you should
        /// unload content from both ResourceManagementMode pools.  Otherwise, just
        /// unload ResourceManagementMode.Manual content.  Manual content will get
        /// Disposed by the GraphicsDevice during a Reset.
        /// </summary>
        /// <param name="unloadAllContent">Which type of content to unload.</param>
        public override void UnloadContent() //bool unloadAllContent)
        {
           // ScreenManager.Game.Components.Remove(quadRenderer);

            Content.Unload();

            // clean up the animation stuff:
          /*  for (int i = 0; i < ScreenManager.Game.Components.Count; i++)
            {
                IGameComponent component = ScreenManager.Game.Components[i];
                if (component is ModelAnimator || component is AnimationController)
                {
                    ScreenManager.Game.Components.RemoveAt(i);
                    i--;
                }
            }*/

            Renderer.UnloadContent();


            The.InGameUI.UnloadContent();

         /*   The.InGameUI.Destroy();
            The.InGameUI = null;
                        
            The.MapUI = null;

          
       
            GameData.UnloadAllData();


            // make sure that we are destroyed completely:
            The.Sim = null;

            // Lars addded this:
            The.Client = null;*/
        }


       
        public override void Destroy()
        {
            //Controller.Game.Components.Remove(quadRenderer);
            // there is still an event link (GraphicsDeviceManager.deviceDisposing)?

         //   ZoomRenderTarget.Dispose();
        

            The.InGameUI.Destroy();
            The.InGameUI = null;

            The.MapUI = null;


            Kensei.Dev.Options.Destroy();

            AudioManager.Destroy();

            Renderer.Destroy();


            // Lars addded this:
            The.Client = null;

        }


        /// <summary>
        /// static collection for optimization... keep all the road piece sprites organized by sprite name and directions.
        /// </summary>
        public static Dictionary<string, Rectangle?[,]> AllConnectedGroundSprites = new Dictionary<string, Rectangle?[,]>();
      

        private static void SetupRoadPieceSprites()
        {
            foreach (var kvp in GameData.Instance.AllEntityTypes) //AllStructureTypes)
            {
                if (kvp.Value.RenderableTypeMode != null && kvp.Value.RenderableTypeMode.RenderAsConnectedGroundSpriteType != null) // . //StructureType.IsRoad)
                {
                    string spriteName = kvp.Value.RenderableTypeMode.RenderAsConnectedGroundSpriteType.AssetName;

                    if (!AllConnectedGroundSprites.ContainsKey(spriteName))
                    {
                        try
                        {
                            Rectangle?[,] connectionSprites = new Rectangle?[8, 8];

                            // double pieces:
                            connectionSprites[(int)Common.Direction.East, (int)Common.Direction.South] = The.Client.FlatSpriteSheet.GetSourceRectangle(string.Format("{0}_E_S", spriteName));

                            connectionSprites[(int)Common.Direction.North, (int)Common.Direction.East] = The.Client.FlatSpriteSheet.GetSourceRectangle(string.Format("{0}_N_E", spriteName));
                            connectionSprites[(int)Common.Direction.North, (int)Common.Direction.SouthEast] = The.Client.FlatSpriteSheet.GetSourceRectangle(string.Format("{0}_N_SE", spriteName));
                            connectionSprites[(int)Common.Direction.North, (int)Common.Direction.West] = The.Client.FlatSpriteSheet.GetSourceRectangle(string.Format("{0}_N_W", spriteName));

                            connectionSprites[(int)Common.Direction.NorthWest, (int)Common.Direction.South] = The.Client.FlatSpriteSheet.GetSourceRectangle(string.Format("{0}_NW_S", spriteName));

                            connectionSprites[(int)Common.Direction.South, (int)Common.Direction.NorthEast] = The.Client.FlatSpriteSheet.GetSourceRectangle(string.Format("{0}_S_NE", spriteName));
                            connectionSprites[(int)Common.Direction.SouthWest, (int)Common.Direction.East] = The.Client.FlatSpriteSheet.GetSourceRectangle(string.Format("{0}_SW_E", spriteName));
                            connectionSprites[(int)Common.Direction.SouthWest, (int)Common.Direction.North] = The.Client.FlatSpriteSheet.GetSourceRectangle(string.Format("{0}_SW_N", spriteName));

                            connectionSprites[(int)Common.Direction.West, (int)Common.Direction.NorthEast] = The.Client.FlatSpriteSheet.GetSourceRectangle(string.Format("{0}_W_NE", spriteName));

                            connectionSprites[(int)Common.Direction.West, (int)Common.Direction.South] = The.Client.FlatSpriteSheet.GetSourceRectangle(string.Format("{0}_W_S", spriteName));
                            connectionSprites[(int)Common.Direction.West, (int)Common.Direction.SouthEast] = The.Client.FlatSpriteSheet.GetSourceRectangle(string.Format("{0}_W_SE", spriteName));

                            // single pieces:
                            connectionSprites[(int)Common.Direction.West, (int)Common.Direction.West] = The.Client.FlatSpriteSheet.GetSourceRectangle(string.Format("{0}_W", spriteName));
                            connectionSprites[(int)Common.Direction.NorthWest, (int)Common.Direction.NorthWest] = The.Client.FlatSpriteSheet.GetSourceRectangle(string.Format("{0}_NW", spriteName));
                            connectionSprites[(int)Common.Direction.North, (int)Common.Direction.North] = The.Client.FlatSpriteSheet.GetSourceRectangle(string.Format("{0}_N", spriteName));
                            connectionSprites[(int)Common.Direction.NorthEast, (int)Common.Direction.NorthEast] = The.Client.FlatSpriteSheet.GetSourceRectangle(string.Format("{0}_NE", spriteName));
                            connectionSprites[(int)Common.Direction.East, (int)Common.Direction.East] = The.Client.FlatSpriteSheet.GetSourceRectangle(string.Format("{0}_E", spriteName));
                            connectionSprites[(int)Common.Direction.SouthEast, (int)Common.Direction.SouthEast] = The.Client.FlatSpriteSheet.GetSourceRectangle(string.Format("{0}_SE", spriteName));
                            connectionSprites[(int)Common.Direction.South, (int)Common.Direction.South] = The.Client.FlatSpriteSheet.GetSourceRectangle(string.Format("{0}_S", spriteName));
                            connectionSprites[(int)Common.Direction.SouthWest, (int)Common.Direction.SouthWest] = The.Client.FlatSpriteSheet.GetSourceRectangle(string.Format("{0}_SW", spriteName));

                            AllConnectedGroundSprites.Add(spriteName, connectionSprites);
                        }
                        catch (Exception e)
                        {
                            throw new Exception(string.Format("Error setting up connected ground sprites '{0}': {1}", spriteName, e.Message));
                        }
                    }
                }
            }
        }


        public void DrawPoint(Vector2 where, Color color)
        {

            if (primitiveBatch == null)
                primitiveBatch = new PrimitiveBatch(Controller.DrawArea.Width, Controller.DrawArea.Height, Controller.GraphicsDevice);


            // primitiveBatch.Begin(PrimitiveType.PointList); // XNA 3
            primitiveBatch.Begin(PrimitiveType.LineList);

            primitiveBatch.AddVertex(where, color);

            where.X--;
            primitiveBatch.AddVertex(where, color);
            where.X += 2f;
            primitiveBatch.AddVertex(where, color);
            where.X--;
            where.Y--;
            primitiveBatch.AddVertex(where, color);
            where.Y += 2f;
            primitiveBatch.AddVertex(where, color);

            // and we're done.
            primitiveBatch.End();
        }


        /// <summary>
        /// called when the selected entity changes
        /// </summary>
        private void SetDebugAttachorOptions(Entity entity)
        {
            string option;
            attachorOptions.Clear();

            if (entity.Renderable.RenderAsModel.ModelData.RightHandAttachor != null)
            {
                option = SetDebugAttachorOption(entity.Renderable.RenderAsModel.ModelData.RightHandAttachor);
            }

            if (entity.Renderable.RenderAsModel.ModelData.LeftHandAttachor != null)
            {

                option = SetDebugAttachorOption(entity.Renderable.RenderAsModel.ModelData.LeftHandAttachor);
            }

            if (entity.Renderable.RenderAsModel.ModelData.BackAttachor != null)
            {
                option = SetDebugAttachorOption(entity.Renderable.RenderAsModel.ModelData.BackAttachor);
            }


            if (entity.Renderable.RenderAsModel.ModelData.HelmetAttachor != null)
            {
                option = SetDebugAttachorOption(entity.Renderable.RenderAsModel.ModelData.HelmetAttachor);
            }



        }

        private string SetDebugAttachorOption(AttachPoint attachor) // string boneName)
        {
            string option;
            //option = "Anim." + attachor.AttachBoneName;
            option = "Attach.Attachor " + attachor.BoneName;

            attachorOptions.Add(option, attachor); // boneName);

            Kensei.Dev.Options.SetOption(option, false);
            Kensei.Dev.Options.SetOptionCallback(option, TestAttachor);

            return option;
        }



        private Vector3 testRotation = new Vector3(0);
        private Vector3 testTranslation = new Vector3(0);
        public void OrientAttachedModel(string option, bool? newBool, float? newFloat)
        {
            if (!newFloat.HasValue)
                return;


            if (option.Contains("Rotate"))
            {
                if (option.Contains("X"))
                    testRotation.X = newFloat.Value;
                else if (option.Contains("Y"))
                    testRotation.Y = newFloat.Value;
                else if (option.Contains("Z"))
                    testRotation.Z = newFloat.Value;
            }
            else if (option.Contains("Translate"))
            {
                if (option.Contains("X"))
                    testTranslation.X = newFloat.Value;
                else if (option.Contains("Y"))
                    testTranslation.Y = newFloat.Value;
                else if (option.Contains("Z"))
                    testTranslation.Z = newFloat.Value;
            }

            if (The.InGameUI.SelectedEntity != null)
            {
                //Entity entity = The.InGameUI.SelectedEntity as Entity;
                Entity entity = Entity.FindByID(The.InGameUI.SelectedEntity.Value); // as Entity;

                if (entity != null)
                {

                    //I'm going to write a new static method for rotating attached object

                    int idx = option.LastIndexOf('#');
                    string attachorBoneName = option.Substring(idx + 1);

                    AnimatedModel.OrientAttachedModel(entity, attachorBoneName, testRotation, testTranslation);

                }
            }

        }
        
        private Dictionary<string, Tuple<string, AttachPoint, AttacheePoint>> attacheeOptions; // = new Dictionary<string, Tuple<string, AttachPoint, AttacheePoint>>();
        private Dictionary<string, AttachPoint> attachorOptions = new Dictionary<string, AttachPoint>();

        /// <summary>
        /// called once
        /// </summary>
        /// <param name="renderableKey"></param>
        private void SetDebugAttacheeOptions(RenderableType renderableType) //string renderableKey)
        {

            // EntityType entityType = GameData.Instance.AllEntityTypes[modelName];

            string modelName = renderableType.RenderAsModelType.AssetName;
            // AttachPoint attacheePoint = EntityType.Renderable.RenderAsModelType.ModelData.BackAttacheePoint;
            AttachPoint attacheePoint = GameData.Instance.AllModels[modelName].BackAttachee;
            if (attacheePoint != null)
            {
                SetDebugAttacheeOption(renderableType.KeyName, attacheePoint, AttacheePoint.Back);
            }

            attacheePoint = GameData.Instance.AllModels[modelName].RightHandAttachee;
            if (attacheePoint != null)
            {
                SetDebugAttacheeOption(renderableType.KeyName, attacheePoint, AttacheePoint.RightHand);
            }

            attacheePoint = GameData.Instance.AllModels[modelName].LeftHandAttachee;
            if (attacheePoint != null)
            {
                SetDebugAttacheeOption(renderableType.KeyName, attacheePoint, AttacheePoint.LeftHand);
            }

            attacheePoint = GameData.Instance.AllModels[modelName].BottomAttachee;
            if (attacheePoint != null)
            {
                SetDebugAttacheeOption(renderableType.KeyName, attacheePoint, AttacheePoint.Bottom);
            }


        }

        private string SetDebugAttacheeOption(string renderableTypeKey, AttachPoint attacheePoint, AttacheePoint attacheePointName)
        {
            string option;
            option = "Attach." + renderableTypeKey + attacheePoint.BoneName;
            Kensei.Dev.Options.SetOption(option, false);
            Kensei.Dev.Options.SetOptionCallback(option, TestAttachee);

            attacheeOptions.Add(option, new Tuple<string, AttachPoint, AttacheePoint>(renderableTypeKey, attacheePoint, attacheePointName));
            return option;
        }

        private void TestAttachee(string option, bool? newBool, float? newFloat)
        {

            if (!newBool.HasValue)
                return;

            if (The.InGameUI.SelectedEntity != null)
            {
                //Entity entity = The.InGameUI.SelectedEntity as Entity;
                Entity entity = Entity.FindByID(The.InGameUI.SelectedEntity.Value); // as Entity;

                if (entity != null)
                {
                    if (newBool == true)
                    {
                        var attacheeData = attacheeOptions[option];
                        string objectKey = attacheeData.Item1;
                        string attacheeBoneName = attacheeData.Item2.BoneName;
                        AttachPoint attacheePointToUse = attacheeOptions[option].Item2;

                        AttacheePoint attacheePoint = attacheeData.Item3;

                        AnimConditionInfo animCondition = FindMatchingAnimCondition(entity, entity.Renderable.RenderAsModel.GetCurrentMainAnimation(), attacheeOptions[option].Item1, attacheePoint);



                        // clear other checkboxes: - not working.
                        /* foreach (KeyValuePair<string, Tuple<string, string>> kvp in attacheeOptions)
                         {
                             Kensei.Dev.Options.SetOption(kvp.Key, false, false);
                         }

                         Kensei.Dev.Options.SetOption(option, true, false);*/

                        // Kensei.Dev.Options.SetOptionsStartingWith("Anim.Attachee", false, false);


                        foreach (KeyValuePair<string, AttachPoint> kvp in attachorOptions)
                        {
                            if (Kensei.Dev.Options.GetOption(kvp.Key) == true)
                            {
                                AttachPoint attachor = kvp.Value;

                                AttachPoint attachee;

                                Renderable renderableToAttach = Renderer.GetFreeAttachableRenderable(objectKey); //.RenderAsModel

                                // see if the current anim specifies a transform/attach point:
                                Vector3 translationToUse, rotationToUse;
                               
                                RenderAsModel.GetAttachTransformations(renderableToAttach, attachor, attacheePoint, animCondition, out attachee, out translationToUse, out rotationToUse);


                                entity.Renderable.AttachObject(renderableToAttach.RenderAsModel,
                                    attachor, attacheePointToUse, null, translationToUse, rotationToUse);

                                break;
                            }
                        }
                    }
                    else
                    {
                        // deattach:
                        // AnimatedModel.RemoveAndRetireAllAttachedModels(The.InGameUI.SelectedEntity, attachorBoneName);

                    }
                }
            }
        }


        /// <summary>
        /// an attachment can be done here or in TestAttachee
        /// </summary>
        /// <param name="option"></param>
        /// <param name="newBool"></param>
        /// <param name="newFloat"></param>
        private void TestAttachor(string option, bool? newBool, float? newFloat)
        {
            if (The.InGameUI.SelectedEntity != null)
            {
                Entity entity = Entity.FindByID(The.InGameUI.SelectedEntity.Value); // as Entity;
                // Entity entity = The.InGameUI.SelectedEntity as Entity;

                if (entity != null)
                {
                    AttachPoint attachor = attachorOptions[option];
                    string attachorBoneName = attachor.BoneName;

                    if (newBool == true)
                    {
                        //TODO add a mutex for all the Attachor buttons, only one checkbox active at a time
                        foreach (KeyValuePair<string, AttachPoint> kvp in attachorOptions)
                        {
                            if (kvp.Key != option)
                            {
                                Kensei.Dev.Options.SetOption(kvp.Key, false);
                                entity.Renderable.RemoveAndRetireAllAttachedModels(attachorOptions[kvp.Key], null);
                            }
                        }


                        //remove the sliders
                        Kensei.Dev.Options.RemoveOptionsStartingWith("Attach.Rotate");
                        Kensei.Dev.Options.RemoveOptionsStartingWith("Attach.Translate");

                        //and if this is setting true, then add the axis sliders only under the active checkbox
                        // find object to attach:

                        foreach (var /*KeyValuePair<string, Tuple<string, AttachPoint>>*/ kvp in attacheeOptions)
                        {
                            if (Kensei.Dev.Options.GetOption(kvp.Key) == true)//is this attachee attached?
                            {
                                AttachPoint attacheePointToUse = kvp.Value.Item2;


                                AttacheePoint attacheeName = kvp.Value.Item3;
                                AnimConditionInfo animCondition = FindMatchingAnimCondition(entity, entity.Renderable.RenderAsModel.GetCurrentMainAnimation(), kvp.Value.Item1, attacheeName); //attacheePoint);



                                Renderable renderableToAttach = Renderer.GetFreeAttachableRenderable(kvp.Value.Item1);

                                // see if the current anim specifies a transform/attach point:
                                AttachPoint attachee;
                                Vector3 translationToUse, rotationToUse;

                                RenderAsModel.AppliedAttachableTransforms appliedRotationTransform;
                                RenderAsModel.AppliedAttachableTransforms appliedTranslationTransform;
                                RenderAsModel.GetAttachTransformations(renderableToAttach, attachor, attacheeName, animCondition, out attachee, out translationToUse, out rotationToUse,
                                    out appliedRotationTransform,
                                    out appliedTranslationTransform);


                                entity.Renderable.AttachObject(renderableToAttach.RenderAsModel, attachor, attacheePointToUse, null, translationToUse, rotationToUse);


                                //add them back, mapped to this option

                                // will be applied to the object!
                                testRotation = Common.WrapVectorBetweenMinusNAndN(rotationToUse /*attacheePointToUse.Rotation*/, 180);

                                string boneName = "";
                                string attachorBoneNameTag = "#"; // needed for decoding in the callback!!! don't change
                                string source = "";
                                boneName = ", " + attachorBoneNameTag + attachor.BoneName;

                                // shorten the captions so there is room for the number value!
                                switch (appliedRotationTransform)
                                {
                                    case RenderAsModel.AppliedAttachableTransforms.Attachee:                                        
                                        //source = ", Source: Attachee " + attachee.BoneName;
                                        source = ", Source: " + attachee.BoneName;
                                        break;
                                    case RenderAsModel.AppliedAttachableTransforms.Attachor:                                       
                                        source = ", Source: Attachor/default";
                                        break;
                                    case RenderAsModel.AppliedAttachableTransforms.Animation:
                                        source = ", Source: Anim condition " + animCondition.ConditionSet.ToString();
                                        break;
                                }
                                 // attacheeName; // attachor.BoneName;

                                string sliderOption = "Attach.Rotate X" + source + boneName;
                                Kensei.Dev.Options.SetOption(sliderOption, testRotation.X, -180, 180);
                                Kensei.Dev.Options.SetOptionCallback(sliderOption, OrientAttachedModel);
                                sliderOption = "Attach.Rotate Y" + source + boneName; 
                                Kensei.Dev.Options.SetOption(sliderOption, testRotation.Y, -180, 180);
                                Kensei.Dev.Options.SetOptionCallback(sliderOption, OrientAttachedModel);
                                sliderOption = "Attach.Rotate Z" + source + boneName; 
                                Kensei.Dev.Options.SetOption(sliderOption, testRotation.Z, -180, 180);
                                Kensei.Dev.Options.SetOptionCallback(sliderOption, OrientAttachedModel);

                                // will be applied to the object!
                                testTranslation = translationToUse; // attacheePointToUse.Translation.HasValue ? attacheePointToUse.Translation.Value : Vector3.Zero;
                                testTranslation = Common.WrapVectorBetweenMinusNAndN(testTranslation, 10);

                            
                                source = "";
                                switch (appliedTranslationTransform)
                                {
                                    case RenderAsModel.AppliedAttachableTransforms.Attachee:
                                        source = ", Source: " + attachee.BoneName;
                                        //source = ", Source: Attachee " + attachee.BoneName;
                                        break;
                                    case RenderAsModel.AppliedAttachableTransforms.Attachor:                                      
                                        source = ", Source: Attachor/default";
                                        break;
                                    case RenderAsModel.AppliedAttachableTransforms.Animation:
                                        source = ", Source: Anim condition " + animCondition.ConditionSet.ToString();
                                        break;

                                }

                                sliderOption = "Attach.Translate X" + source + boneName; 
                               // Kensei.Dev.Options.SetOption(sliderOption, testTranslation.X, -10, 10);
                                Kensei.Dev.Options.SetOption(sliderOption, testTranslation.X, -20, 20);
                                Kensei.Dev.Options.SetOptionCallback(sliderOption, OrientAttachedModel);
                                sliderOption = "Attach.Translate Y" + source + boneName; 
                                Kensei.Dev.Options.SetOption(sliderOption, testTranslation.Y, -20, 20);
                                Kensei.Dev.Options.SetOptionCallback(sliderOption, OrientAttachedModel);
                                sliderOption = "Attach.Translate Z" + source + boneName; 
                                Kensei.Dev.Options.SetOption(sliderOption, testTranslation.Z, -20, 20);
                                Kensei.Dev.Options.SetOptionCallback(sliderOption, OrientAttachedModel);
                                

                                break;//we only attach the first attachee option selected

                            }
                        }


                    }
                    else
                    {
                        //remove the sliders
                        Kensei.Dev.Options.RemoveOptionsStartingWith("Attach.Rotate");
                        Kensei.Dev.Options.RemoveOptionsStartingWith("Attach.Translate");

                        // deattach all:
                        entity.Renderable.RemoveAndRetireAllAttachedModels(attachor, null);

                    }
                }
            }

        }

        private AnimConditionInfo FindMatchingAnimCondition(Entity entity, string animKey, string attachedRenderableTypeKey, AttacheePoint? attacheePoint) //, out Vector3? translation, out Vector3? rotation)
        {
            if (entity.EntityType.RenderableTypeMode.RenderAsModelType.AnimConditions != null)
            {
                foreach (var animCondition in entity.EntityType.RenderableTypeMode.RenderAsModelType.AnimConditions)
                {
                    if (animCondition.AttachPoints != null)
                    {
                        // pick the first match we see...
                        if (animCondition.SoundAndAnimationSet != null && animCondition.SoundAndAnimationSet.BaseAnimations != null &&
                            animCondition.SoundAndAnimationSet.BaseAnimations.Contains(animKey))
                        {
                            // find the best match...
                            foreach (var item in animCondition.AttachPoints)
                            {
                                if (item.AttacheePoint == attacheePoint && item.RenderableTypeKey == attachedRenderableTypeKey)
                                {
                                    return animCondition; // item.AttachPoint;
                                }
                            }
                        }
                    }
                }
            }

            return null;


        }

        private void TestAnims(string option, bool? newBool, float? newFloat)
        {


            if (The.InGameUI.SelectedEntity != null)
            {
                Entity entity = Entity.FindByID(The.InGameUI.SelectedEntity.Value);

                if (entity != null && entity.Intelligence.DisableAI && entity.Renderable != null && entity.Renderable.RenderAsModel != null)
                {
                    if (newBool == true)
                    {
                        Looping doLooping = Looping.Yes;
                        //  Kensei.Dev.Options.GetOption(animLabel2 + "Looping", true);

                        if (option.StartsWith("Anim.Test anim 1"))
                        {
                            doLooping = (Kensei.Dev.Options.GetOption("Anim.Test anim 1: Looping") == true ? Looping.Yes : Looping.No);

                            //Kensei.Dev.Options.SetOptionsStartingWith("Anim.Test anim 1", false);
                        }
                        else if (option.StartsWith("Anim.Test anim 2"))
                        {
                            doLooping = (Kensei.Dev.Options.GetOption("Anim.Test anim 2: Looping") == true ? Looping.Yes : Looping.No);

                        }
                        else if (option.StartsWith("Anim.Test anim 3"))
                        {
                            doLooping = (Kensei.Dev.Options.GetOption("Anim.Test anim 3: Looping") == true ? Looping.Yes : Looping.No);

                        }


                        //  entity.Renderable.StartAdditionalAnimation(animationOptions[option], Playback.Forwards, StartingPoint.FromBeginning, BlendMode.NoBlending /* BlendMode.Additive*/, 1f, doLooping);
                        entity.Renderable.RenderAsModel.StartMainAnimation(animationOptions[option], Playback.Forwards, StartingPoint.FromBeginning, BlendMode.NoBlending /* BlendMode.Additive*/, 1f, doLooping);
                        //  The.InGameUI.SelectedEntity.Renderable.RenderAsModel.TryStartAnimation(animationOptions[option], Playback.Forwards, StartingPoint.FromBeginning, RenderAsModel.Mode.Normal, 1f, doLooping);
                    }
                    else
                    {
                        entity.Renderable.RenderAsModel.StopMainAnimation(); //animationOptions[option]);
                    }
                }
            }
        }


        

        private void MovemapClick(string option, bool? newBool, float? newFloat)
        {
            MovementMap map = overlayOptions[option]; // UWGame.SimSide.Instance.Map.GetMovementMap(ProtectionLevel.Exposed, ThreatCategory.Human, EntityApproach.Bold);

            if (newBool == true && !The.MapUI.Overlays.Contains(map))
            {
                The.MapUI.Overlays.Add(map);
            }
            else
            {
                The.MapUI.Overlays.Remove(map);
            }

        }

        private void RegionMapClick(string option, bool? newBool, float? newFloat)
        {
            RegionMap regionMap = regionMapOverlayOptions[option]; // UWGame.SimSide.Instance.Map.GetMovementMap(ProtectionLevel.Exposed, ThreatCategory.Human, EntityApproach.Bold);

            if (newBool == true && !The.MapUI.Overlays.Contains(regionMap))
            {
                The.MapUI.Overlays.Add(regionMap);
            }
            else
            {
                The.MapUI.Overlays.Remove(regionMap);
            }

        }

        private void ThreatMapClick(string option, bool? newBool, float? newFloat)
        {
            ThreatMap map = threatMapOverlayOptions[option]; 

            if (newBool == true && !The.MapUI.Overlays.Contains(map))
            {
                The.MapUI.Overlays.Add(map);
            }
            else
            {
                The.MapUI.Overlays.Remove(map);
            }

        }

        private void DiscomfortMapClick(string option, bool? newBool, float? newFloat)
        {
            DiscomfortMap map = discomfortMapOverlayOptions[option];

            if (newBool == true && !The.MapUI.Overlays.Contains(map))
            {
                The.MapUI.Overlays.Add(map);
            }
            else
            {
                The.MapUI.Overlays.Remove(map);
            }
        }

        #region these pairs are used to map from the data driven check boxes on the dev panel to some other object
        /// <summary>
        /// 
        /// </summary>
        private Dictionary<string, string> animationOptions = new Dictionary<string, string>();
        private Dictionary<string, MovementMap> overlayOptions = new Dictionary<string, MovementMap>();
        private Dictionary<string, RegionMap> regionMapOverlayOptions = new Dictionary<string, RegionMap>();
        private Dictionary<string, ThreatMap> threatMapOverlayOptions = new Dictionary<string, ThreatMap>();
        private Dictionary<string, DiscomfortMap> discomfortMapOverlayOptions = new Dictionary<string, DiscomfortMap>();      
        private Dictionary<string, ResourceType> resourceTypeOverlayOptions = new Dictionary<string, ResourceType>();

        #endregion

        private void InitDeveloperDialog()
        {
            // why init twice??
            Dimension dim = Controller.DrawArea;
            Kensei.Dev.Manager.Initialise(The.Client.Content, The.Client.GraphicsDevice, 0, 0, dim.Width, dim.Height);
            Kensei.Dev.Options.CreateDialog();

            Kensei.Dev.Options.SetOption("Dev.Debug selected entity", false);
            Kensei.Dev.Options.SetOption("Dev.Destroy selected entity", Kensei.Dev.Options.DebugButton.MakeAButton);
            Kensei.Dev.Options.SetOptionCallback("Dev.Destroy selected entity", Kill_OnPress);

            Kensei.Dev.Options.SetOption("Dev.Cancel selected entity's job", Kensei.Dev.Options.DebugButton.MakeAButton);
            Kensei.Dev.Options.SetOptionCallback("Dev.Cancel selected entity's job", CancelEntityJob_OnPress);

            Kensei.Dev.Options.SetOption("Dev.Destroy selected tile contents", false);
            Kensei.Dev.Options.SetOptionCallback("Dev.Destroy selected tile contents", ClearTile_OnPress);

            Kensei.Dev.Options.SetOption("Dev.Block tile", Kensei.Dev.Options.DebugButton.MakeAButton);
            Kensei.Dev.Options.SetOptionCallback("Dev.Block tile", BlockTile_OnPress);

            Kensei.Dev.Options.SetOption("Dev.Unblock tile", Kensei.Dev.Options.DebugButton.MakeAButton);
            Kensei.Dev.Options.SetOptionCallback("Dev.Unblock tile", UnblockTile_OnPress);

            Kensei.Dev.Options.SetOption("Performance.Reset", Kensei.Dev.Options.DebugButton.MakeAButton);
            Kensei.Dev.Options.SetOptionCallback("Performance.Reset", ResetPerformanceCounters);

           /* Kensei.Dev.Options.SetOption("Dev.Block edge", false);
            Kensei.Dev.Options.SetOptionCallback("Dev.Block edge", BlockEdge_OnPress);
            */

            Kensei.Dev.Options.SetOption("Dev.Merge households", Kensei.Dev.Options.DebugButton.MakeAButton);
            Kensei.Dev.Options.SetOptionCallback("Dev.Merge households", MergeHouseholds_OnPress);

          /*  Kensei.Dev.Options.SetOption("Dev.Custom action #1", Kensei.Dev.Options.DebugButton.MakeAButton);
            Kensei.Dev.Options.SetOptionCallback("Dev.Custom action #1", Action_OnPress);

            Kensei.Dev.Options.SetOption("Dev.Custom action #2", Kensei.Dev.Options.DebugButton.MakeAButton);
            Kensei.Dev.Options.SetOptionCallback("Dev.Custom action #2", Action2_OnPress);
            */

            Kensei.Dev.Options.SetOption("Dev.Test emigrate", Kensei.Dev.Options.DebugButton.MakeAButton);
            Kensei.Dev.Options.SetOptionCallback("Dev.Test emigrate", Emigrate_OnPress);

            Kensei.Dev.Options.SetOption("Dev.Test sleep", Kensei.Dev.Options.DebugButton.MakeAButton);
            Kensei.Dev.Options.SetOptionCallback("Dev.Test sleep", Sleep_OnPress);

            Kensei.Dev.Options.SetOption("Dev.Test starve", Kensei.Dev.Options.DebugButton.MakeAButton);
            Kensei.Dev.Options.SetOptionCallback("Dev.Test starve", Starve_OnPress);

            Kensei.Dev.Options.SetOption("Dev.Test starve near death", Kensei.Dev.Options.DebugButton.MakeAButton);
            Kensei.Dev.Options.SetOptionCallback("Dev.Test starve near death", StarveNearDeath_OnPress);

            Kensei.Dev.Options.SetOption("Dev.Test injury", Kensei.Dev.Options.DebugButton.MakeAButton);
            Kensei.Dev.Options.SetOptionCallback("Dev.Test injury", Injure_OnPress);

            Kensei.Dev.Options.SetOption("Dev.Immobilize", Kensei.Dev.Options.DebugButton.MakeAButton);
            Kensei.Dev.Options.SetOptionCallback("Dev.Immobilize", Immobilize_OnPress);

            Kensei.Dev.Options.SetOption("Dev.Test integrity damage", Kensei.Dev.Options.DebugButton.MakeAButton);
            Kensei.Dev.Options.SetOptionCallback("Dev.Test integrity damage", IntegrityDamage_OnPress);

            Kensei.Dev.Options.SetOption("Dev.Test damage", Kensei.Dev.Options.DebugButton.MakeAButton);
            Kensei.Dev.Options.SetOptionCallback("Dev.Test damage", Damage_OnPress);

            Kensei.Dev.Options.SetOption("Dev.Add credits", Kensei.Dev.Options.DebugButton.MakeAButton);
            Kensei.Dev.Options.SetOptionCallback("Dev.Add credits", AddCredits);

            Kensei.Dev.Options.SetOption("Dev.Raise comfort principles", Kensei.Dev.Options.DebugButton.MakeAButton);
            Kensei.Dev.Options.SetOptionCallback("Dev.Raise comfort principles", RaiseComfortPrinciples);

            Kensei.Dev.Options.SetOption("Dev.Raise food principles", Kensei.Dev.Options.DebugButton.MakeAButton);
            Kensei.Dev.Options.SetOptionCallback("Dev.Raise food principles", RaiseFoodPrinciples);

            Kensei.Dev.Options.SetOption("Dev.Raise security principles", Kensei.Dev.Options.DebugButton.MakeAButton);
            Kensei.Dev.Options.SetOptionCallback("Dev.Raise security principles", RaiseSecurityPrinciples);

            Kensei.Dev.Options.SetOption("Dev.Add log message", Kensei.Dev.Options.DebugButton.MakeAButton);
            Kensei.Dev.Options.SetOptionCallback("Dev.Add log message", AddLogMessage_OnPress);


            //Kensei.Dev.Options.SetOption("Dev.Place tree", false);
            //Kensei.Dev.Options.SetOptionCallback("Dev.Place tree", PlaceTree_OnPress);

            //Kensei.Dev.Options.SetOption("Dev.Place meat", false);
            //Kensei.Dev.Options.SetOptionCallback("Dev.Place meat", PlaceItem_OnPress);

            Kensei.Dev.Options.SetOption("Dev.Show FPS", false);
            Kensei.Dev.Options.SetOption("Dev.Limit FPS when paused", true);
          
            Kensei.Dev.Options.SetOption("Dev.Emit debug output in log", false);
            Kensei.Dev.Options.SetOption("Dev.Show task importance", false);

            Kensei.Dev.Options.SetOption("Dev.Show property values", false);

            Kensei.Dev.Options.SetOption("Dev.God mode", false); 
            Kensei.Dev.Options.SetOptionCallback("Dev.God mode", GodMode_OnPress);

            Kensei.Dev.Options.SetOption("Dev.Show fog of war", true);
            Kensei.Dev.Options.SetOptionCallback("Dev.Show fog of war", FogOfWar_OnPress);


            Kensei.Dev.Options.SetOption("Dev.Show hitpoints", false);

            Kensei.Dev.Options.SetOption("Dev.Detect all", false); 

            Kensei.Dev.Options.SetOption("Dev.Launch selected entity", false);
            Kensei.Dev.Options.SetOptionCallback("Dev.Launch selected entity", LaunchSelected);

            Kensei.Dev.Options.SetOption("Dev.Set GUI to allegiance of selected entity", false);
            Kensei.Dev.Options.SetOptionCallback("Dev.Set GUI to allegiance of selected entity", SetUIToSelectedEntityAllegiance);

            // Kensei.Dev.Options.SetOption("Dev.Test anims", false);
            // Kensei.Dev.Options.SetOptionCallback("Dev.Test anims", TestAnims);

            Kensei.Dev.Options.SetOption("Anim.Disable AI", false);
            Kensei.Dev.Options.SetOptionCallback("Anim.Disable AI", DisableAI_OnPress);

            Kensei.Dev.Options.SetOption("Anim.Rotate slowly", false);
            Kensei.Dev.Options.SetOptionCallback("Anim.Rotate slowly", RotateSlowly_OnPress);

            //RenderAsModel.RotateSlowly

            Kensei.Dev.Options.SetOption("Anim.Wander", false);
            Kensei.Dev.Options.SetOptionCallback("Anim.Wander", Wander_OnPress);


            // sets up checkbox states and animation buttons for selected entity:
            The.InGameUI.SelectedEntityChangedEvent += new UWGame.ClientSide.Interface.InGameInterface.SelectedEntityChanged(DeveloperDialogSelectedEntityChangedEvent);

            /* Kensei.Dev.Options.SetOption("Anim.Attachor PalmRight", false);
             Kensei.Dev.Options.SetOptionCallback("Anim.Attachor PalmRight", AttachorBoxRightHand_OnPress);

             Kensei.Dev.Options.SetOption("Anim.Attachee Hammer", false);
             Kensei.Dev.Options.SetOptionCallback("Anim.Attachee Hammer", AttachorBoxRightHand_OnPress);


             Kensei.Dev.Options.SetOption("Anim.Attachee box rightHand", false);
             Kensei.Dev.Options.SetOptionCallback("Anim.Attachee box rightHand", AttachorBoxRightHand_OnPress);
             */

            attacheeOptions = new Dictionary<string, Tuple<string, AttachPoint, AttacheePoint>>();
            foreach (var item in GameData.Instance.AttachableRenderableTypes)
            {
                SetDebugAttacheeOptions(item.Value);
            }

            /*
            SetDebugAttacheeOptions("box");
            SetDebugAttacheeOptions("hammer");
            SetDebugAttacheeOptions("backpack");
            SetDebugAttacheeOptions("rifle");
            SetDebugAttacheeOptions("spear");
            SetDebugAttacheeOptions("armsling");
            SetDebugAttacheeOptions("bush");
            */




            /*  // box:
            // attachment_rightHand
            private const string attachHandBoneName = "man:attach_leftHand"; // "man:man_RightHand"; // "man:attach_rightHand"
            */

            //RENDERING PANEL
            Kensei.Dev.Options.SetOption("Rendering.Render GUI", true); // MLo HACK
            Kensei.Dev.Options.SetOption("Rendering.Render models", true);
            Kensei.Dev.Options.SetOption("Rendering.Render terrain", true);
            Kensei.Dev.Options.SetOption("Rendering.Render water", true);
            Kensei.Dev.Options.SetOption("Rendering.Render trees", true);
            Kensei.Dev.Options.SetOption("Rendering.Render billboards", true);
            Kensei.Dev.Options.SetOption("Rendering.Render ground sprites", true);
            Kensei.Dev.Options.SetOption("Rendering.Render shadows", true);
            Kensei.Dev.Options.SetOption("Rendering.Render light sources", true);
            Kensei.Dev.Options.SetOption("Rendering.Pixel shader 2 or lower", true);
            Kensei.Dev.Options.SetOption("Rendering.Draw outlines", true);
            Kensei.Dev.Options.SetOption("Rendering.Show light amount", false);
            Kensei.Dev.Options.SetOption("Rendering.OLD Fog of war", false);
            Kensei.Dev.Options.SetOption("Rendering.Spoken lines", true);


            //TUNING PANEL / TWEAKING
            Kensei.Dev.Options.SetOption("Tuning.Fog of War tint", The.MapUI.FogOfWarTint, 0f, 1f);
            Kensei.Dev.Options.SetOptionCallback("Tuning.Fog of War tint", The.MapUI.SetFogOfWarTint);
            Kensei.Dev.Options.SetOption("Tuning.Fog of War fade rate", The.MapUI.FogOfWarFadeRate, 0.01f, 0.1f);
            Kensei.Dev.Options.SetOptionCallback("Tuning.Fog of War fade rate", The.MapUI.SetFogOfWarFadeRate);
            Kensei.Dev.Options.SetOption("Tuning.Shadow opacity", The.MapUI.CloudOpacity, 0f, 1f);
            Kensei.Dev.Options.SetOptionCallback("Tuning.Shadow opacity", The.MapUI.SetCloudOpacity);
            Kensei.Dev.Options.SetOption("Tuning.Cloud edge sharpness", The.MapUI.CloudSharpness, 0f, 10f);
            Kensei.Dev.Options.SetOptionCallback("Tuning.Cloud edge sharpness", The.MapUI.SetCloudEdgeHardness);
            /*   Kensei.Dev.Options.SetOption("Tuning.Collision push strength", Entity.pushStrength.X, 0f, 10f);
               Kensei.Dev.Options.SetOptionCallback("Tuning.Collision push strength", 
                                                (s, b, f) =>  Entity.pushStrength = new Vector2(f.Value));*/
            Kensei.Dev.Options.SetOption("Tuning.Selection cycle speed", The.InGameUI.SelectedCycleAnimation.FrameTime, 0.1f, 2f);
            Kensei.Dev.Options.SetOptionCallback("Tuning.Selection cycle speed",
                                             (s, b, f) => The.InGameUI.SelectedCycleAnimation.FrameTime = f.Value);
            Kensei.Dev.Options.SetOption("Tuning.Zone opacity", MapAreaRender.Opacity, 0f, 1f);
            Kensei.Dev.Options.SetOptionCallback("Tuning.Zone opacity", (s, b, f) => MapAreaRender.Opacity = f.Value);
            Kensei.Dev.Options.SetOption("Tuning.Tooltip scroll speed", DataSheet.scrollSpeedPerSecond, 20f, 200f);
            Kensei.Dev.Options.SetOptionCallback("Tuning.Tooltip scroll speed", (s, b, f) => DataSheet.scrollSpeedPerSecond = f.Value);

            string key;

            Kensei.Dev.Options.SetOption("Anim tuning.Model lerp factor", RenderAsModel.LerpFactor, 0f, 1f);
            Kensei.Dev.Options.SetOptionCallback("Anim tuning.Model lerp factor", (s, b, f) => RenderAsModel.LerpFactor = f.Value);

            key = "Anim tuning.Model location lerp limit";
            Kensei.Dev.Options.SetOption(key, RenderAsModel.LocationLerpLimit, 0f, 2f);
            Kensei.Dev.Options.SetOptionCallback(key, (s, b, f) => RenderAsModel.LocationLerpLimit = f.Value);

            key = "Anim tuning.Model rotation lerp limit";
            Kensei.Dev.Options.SetOption(key, RenderAsModel.RotationLerpLimit, 0f, 0.1f);
            Kensei.Dev.Options.SetOptionCallback(key, (s, b, f) => RenderAsModel.RotationLerpLimit = f.Value);

            key = "Anim tuning.Distance to stop lerping";
            Kensei.Dev.Options.SetOption(key, RenderAsModel.DistanceToStopLerping, 0f, 200f);
            Kensei.Dev.Options.SetOptionCallback(key, (s, b, f) => RenderAsModel.DistanceToStopLerping = f.Value);


            Kensei.Dev.Options.SetOption("Anim tuning.Collided entity interest level mean", GameData.Instance.Constants.InterestLevelForCollidedEntityMean, 0f, 200f);
            Kensei.Dev.Options.SetOptionCallback("Anim tuning.Collided entity interest level mean", (s, b, f) => GameData.Instance.Constants.InterestLevelForCollidedEntityMean = f.Value);

            Kensei.Dev.Options.SetOption("Anim tuning.Collided entity interest level std dev", GameData.Instance.Constants.InterestLevelForCollidedEntityStdDeviation, 0f, 50f);
            Kensei.Dev.Options.SetOptionCallback("Anim tuning.Collided entity interest std dev", (s, b, f) => GameData.Instance.Constants.InterestLevelForCollidedEntityStdDeviation = f.Value);

            key = "Anim tuning.Conversation interest level";
            Kensei.Dev.Options.SetOption(key, GameData.Instance.Constants.InterestLevelForConversation, 0f, 200f);
            Kensei.Dev.Options.SetOptionCallback(key, (s, b, f) => GameData.Instance.Constants.InterestLevelForConversation = f.Value);


            key = "Anim tuning.Spotted entity interest level mean";
            Kensei.Dev.Options.SetOption(key, GameData.Instance.Constants.InterestLevelForSpottedEntityMean, 0f, 200f);
            Kensei.Dev.Options.SetOptionCallback(key, (s, b, f) => GameData.Instance.Constants.InterestLevelForSpottedEntityMean = f.Value);

            key = "Anim tuning.Spotted entity interest level std dev";
            Kensei.Dev.Options.SetOption(key, GameData.Instance.Constants.InterestLevelForSpottedEntityStdDeviation, 0f, 50f);
            Kensei.Dev.Options.SetOptionCallback(key, (s, b, f) => GameData.Instance.Constants.InterestLevelForSpottedEntityStdDeviation = f.Value);

            key = "Anim tuning.Spotted resource interest level mean";
            Kensei.Dev.Options.SetOption(key, GameData.Instance.Constants.InterestLevelForSpottedResourceMean, 0f, 200f);
            Kensei.Dev.Options.SetOptionCallback(key, (s, b, f) => GameData.Instance.Constants.InterestLevelForSpottedResourceMean = f.Value);

            key = "Anim tuning.Spotted resource interest level std dev";
            Kensei.Dev.Options.SetOption(key, GameData.Instance.Constants.InterestLevelForSpottedResourceStdDeviation, 0f, 50f);
            Kensei.Dev.Options.SetOptionCallback(key, (s, b, f) => GameData.Instance.Constants.InterestLevelForSpottedResourceStdDeviation = f.Value);

            
          

            key = "Anim tuning.Time to wait before turning to listen";
            Kensei.Dev.Options.SetOption(key, GoalDoTakeFive.TimeToWaitBeforeTurningBodyToListen, 0f, 3f);
            Kensei.Dev.Options.SetOptionCallback(key, (s, b, f) => GoalDoTakeFive.TimeToWaitBeforeTurningBodyToListen = f.Value);

            /*    key = "Anim tuning.Idle chance to play long idle anim"; 
                Kensei.Dev.Options.SetOption(key, GoalDoTakeFive.IdleChanceToPlayLongIdle, 0f, 1f);
                Kensei.Dev.Options.SetOptionCallback(key, (s, b, f) => GoalDoTakeFive.IdleChanceToPlayLongIdle = f.Value);
               */

            key = "Anim tuning.Blend time factor"; // 1 = no blending at all
            Kensei.Dev.Options.SetOption(key, AnimationTrack.BlendPeriodInMilliseconds, 1f, 5000f);
            Kensei.Dev.Options.SetOptionCallback(key, (s, b, f) => AnimationTrack.BlendPeriodInMilliseconds = f.Value);

            

            /*
            key = "Anim tuning.Person walk speed"; 
            Kensei.Dev.Options.SetOption(key,
                GameData.Instance.AllEntityTypes["entity:human"].RenderableType.RenderAsModelType.AnimConditions.First(c => c.AnimationSet.BaseAnimations.Contains("gaitWalk")).SpeedFactor, 0.1f, 3f);
            Kensei.Dev.Options.SetOptionCallback(key, (s, b, f) =>
                GameData.Instance.AllEntityTypes["entity:human"].RenderableType.RenderAsModelType.AnimConditions.First(c => c.AnimationSet.BaseAnimations.Contains("gaitWalk")).SpeedFactor = f.Value);

            key = "Anim tuning.Person haul speed";
            Kensei.Dev.Options.SetOption(key,
                GameData.Instance.AllEntityTypes["entity:human"].RenderableType.RenderAsModelType.AnimConditions.First(c => c.AnimationSet.BaseAnimations.Contains("haulHeavy")).SpeedFactor, 0.1f, 3f);
            Kensei.Dev.Options.SetOptionCallback(key, (s, b, f) =>
                GameData.Instance.AllEntityTypes["entity:human"].RenderableType.RenderAsModelType.AnimConditions.First(c => c.AnimationSet.BaseAnimations.Contains("haulHeavy")).SpeedFactor = f.Value);
          
           */

            //OVERLAYS PANEL
            Kensei.Dev.Options.SetOption("Overlays.Terrain costs (Foot)", false);
            Kensei.Dev.Options.SetOptionCallback("Overlays.Terrain costs (Foot)", The.MapUI.ShowFoot_OnPress);
            Kensei.Dev.Options.SetOption("Overlays.Terrain costs (ATV)", false);
            Kensei.Dev.Options.SetOptionCallback("Overlays.Terrain costs (ATV)", The.MapUI.ShowATV_OnPress);
            Kensei.Dev.Options.SetOption("Overlays.Terrain costs (Car)", false);
            Kensei.Dev.Options.SetOptionCallback("Overlays.Terrain costs (Car)", The.MapUI.ShowCar_OnPress);
            Kensei.Dev.Options.SetOption("Overlays.Region map (Terrain/Foot)", false);
            Kensei.Dev.Options.SetOptionCallback("Overlays.Region map (Terrain/Foot)", The.MapUI.ShowFootRegionMap_OnPress);
            Kensei.Dev.Options.SetOption("Overlays.Crops", false);
            Kensei.Dev.Options.SetOption("Overlays.Allegiances", false);
            Kensei.Dev.Options.SetOption("Overlays.Jobs", false);
            Kensei.Dev.Options.SetOption("Overlays.Path search", false);
            Kensei.Dev.Options.SetOption("Overlays.Show entity waypoints", false);
            Kensei.Dev.Options.SetOption("Overlays.Ranges", false);
            Kensei.Dev.Options.SetOption("Overlays.Interest", false);
            Kensei.Dev.Options.SetOption("Overlays.Markers", false);
            Kensei.Dev.Options.SetOption("Overlays.TerrainGeometries", false);
            Kensei.Dev.Options.SetOption("Overlays.CollisionGeometries", false);
            Kensei.Dev.Options.SetOption("Overlays.SelectionShapes", false); // geo layouts only
            //  Kensei.Dev.Options.SetOption("Overlays.AccessPointsAndExits", false);
            Kensei.Dev.Options.SetOption("Dev.Place Threat", Kensei.Dev.Options.DebugButton.MakeAButton);
            Kensei.Dev.Options.SetOptionCallback("Dev.Place Threat", PlaceThreat_OnPress);
            Kensei.Dev.Options.SetOption("Overlays.Terrain division", false);                       

            Kensei.Dev.Options.SetOption("Overlays.Render region maps in progress", false);

         	//Temp Name. Did not figure out what to name it
			//Displays an circle from the center of an expedition to the radius set in the 
			//Representative entity.
			Kensei.Dev.Options.SetOption("Overlays.Expedition Scouting Radius", false);
            Kensei.Dev.Options.SetOption("Overlays.Sensor tiles", false); 

            // resource panel:
            foreach (var item in The.Sim.PlaySite.Resources)
            {
                string resourceKey = "Resources." + item.Key.KeyName;
                resourceTypeOverlayOptions.Add(resourceKey, item.Key);
                Kensei.Dev.Options.SetOption(resourceKey, false);
                Kensei.Dev.Options.SetOptionCallback(resourceKey, Resource_OnPress);
          
            }


            //add a tab in dialog for all the entity types
            var sortedTypes = GameData.Instance.AllEntityTypes.OrderBy(e => e.Key).ToList();
            foreach (var kvp in sortedTypes) // KeyValuePair<string, EntityType> kvp in GameData.Instance.AllEntityTypes)
            {
                EntityType type = kvp.Value;
                if (type != null)
                {
                    string tabPageName;
                    if (type.TerrainType != null)
                    {
                        tabPageName = "TerrainTypes";              
                    }
                    else if (type.StructureType != null)
                    {
                        tabPageName = "StructureTypes";                        
                    }
                    else if (type.ItemType != null)
                    {
                        tabPageName = "ItemTypes";                     
                    }
                    else
                    {
                        tabPageName = "EntityTypes";
                    }

                    Kensei.Dev.Options.SetOption(tabPageName + "." + type.KeyName, type);
                }
            }


            List<string> startLog = The.Sim.GetStartGameLog();
            foreach (var item in startLog)
            {
                Kensei.Dev.Options.AppendEventsLogText(item);    
            }

            List<string> startPopSpawnLog = The.Sim.GetStartPopulationSpawnLog();
            foreach (var item in startPopSpawnLog)
            {
                Kensei.Dev.Options.AppendPopSpawnText(item);
            }
            
            PopulateTestEvents(GameData.Instance.AllActionSets.Values.ToList(), GameData.Instance.AllPolledEvents.Values.ToList());


            // achievement test
            key = "Achievements.Advance time";
            Kensei.Dev.Options.SetOption(key, Kensei.Dev.Options.DebugButton.MakeAButton);
            Kensei.Dev.Options.SetOptionCallback(key, TestAdvanceTime);

           
            key = "Achievements.Set guns produced count";
            Kensei.Dev.Options.SetOption(key, Kensei.Dev.Options.DebugButton.MakeAButton);
            Kensei.Dev.Options.SetOptionCallback(key, TestSetGunsProduced);

            key = "Achievements.Set hides produced count";
            Kensei.Dev.Options.SetOption(key, Kensei.Dev.Options.DebugButton.MakeAButton);
            Kensei.Dev.Options.SetOptionCallback(key, TestSetHidesProduced);

            key = "Achievements.Test ratings and population";
            Kensei.Dev.Options.SetOption(key, Kensei.Dev.Options.DebugButton.MakeAButton);
            Kensei.Dev.Options.SetOptionCallback(key, TestRatingsAndPopulation);

            key = "Achievements.Spawn swarmers";
            Kensei.Dev.Options.SetOption(key, Kensei.Dev.Options.DebugButton.MakeAButton);
            Kensei.Dev.Options.SetOptionCallback(key, TestSwarmers);

            Kensei.Dev.Options.SetOption("Achievements.Spawn twinkler", Kensei.Dev.Options.DebugButton.MakeAButton);
            Kensei.Dev.Options.SetOptionCallback("Achievements.Spawn twinkler", TestTwinkler);

            Kensei.Dev.Options.SetOption("Achievements.Spawn sentries", Kensei.Dev.Options.DebugButton.MakeAButton);
            Kensei.Dev.Options.SetOptionCallback("Achievements.Spawn sentries", TestSpraySentries);

            key = "Achievements.Spawn food";
            Kensei.Dev.Options.SetOption(key, Kensei.Dev.Options.DebugButton.MakeAButton);
            Kensei.Dev.Options.SetOptionCallback(key, TestFood);

            key = "Achievements.Spawn guns and ammo";
            Kensei.Dev.Options.SetOption(key, Kensei.Dev.Options.DebugButton.MakeAButton);
            Kensei.Dev.Options.SetOptionCallback(key, TestCoilRiflesAndAmmo);

            key = "Achievements.Spawn upgraded huts";
            Kensei.Dev.Options.SetOption(key, Kensei.Dev.Options.DebugButton.MakeAButton);
            Kensei.Dev.Options.SetOptionCallback(key, TestUpgradedHuts);

            key = "Achievements.Spawn comfort items";
            Kensei.Dev.Options.SetOption(key, Kensei.Dev.Options.DebugButton.MakeAButton);
            Kensei.Dev.Options.SetOptionCallback(key, TestSpawnComfortItems);

            key = "Achievements.Spawn new member";
            Kensei.Dev.Options.SetOption(key, Kensei.Dev.Options.DebugButton.MakeAButton);
            Kensei.Dev.Options.SetOptionCallback(key, TestMember);

            key = "Achievements.Spawn bush dragon carcass";
            Kensei.Dev.Options.SetOption(key, Kensei.Dev.Options.DebugButton.MakeAButton);
            Kensei.Dev.Options.SetOptionCallback(key, TestBushDragonCarcass);


            // Kensei.Dev.Options.SetOption("Job scores", type);

        }


        #region Achievement tests

        private void TestSwarmers(string option, bool? newBool, float? newFloat)
        {
            UWGame.SimSide.Allegiances.Allegiance all1 = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:swarmer"]);

            Expedition exp = The.InGameUI.UIAllegiance.GetFirstExpedition();

            for (int x = 0; x < 10; x++)
			{		 			
                for (int y = 0; y < 5; y++)
                {
                    Entity e1 = PlaceGameEntities.PlaceAnimal("entity:swarmer", Reproduction.Male,
                        MapManager.WorldPosToTile(new Vector3(exp.Location.Value.X + x * 5f, exp.Location.Value.Y + y * 5f, 0f)), 
                        20, all1);

                    PlaceGameEntities.ImmobilizeEntity(e1);

                }
            }           
        }

        private void TestTwinkler(string option, bool? newBool, float? newFloat)
        {
            UWGame.SimSide.Allegiances.Allegiance all1 = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:twinkler"]);

            Expedition exp = The.InGameUI.UIAllegiance.GetFirstExpedition();

            Entity e1 = PlaceGameEntities.PlaceAnimal("entity:twinkler", Reproduction.Male,
                MapManager.WorldPosToTile(new Vector3(exp.Location.Value.X, exp.Location.Value.Y, 0f)),
                20, all1);

            PlaceGameEntities.ImmobilizeEntity(e1);

        }        

        private void TestAdvanceTime(string option, bool? newBool, float? newFloat)
        {
            The.Sim.AdvanceTime(DateAndTime.secondsPerDay); // DateAndTime.DaysPerYear);
           // The.Sim.DateAndTime.UpdateDayAndYear( )
        }

        private void TestSetGunsProduced(string option, bool? newBool, float? newFloat)
        {
            /*
             entityType.KeyName == "item:gunpowderRifle"
                                    || entityType.KeyName == "item:blackPowderRifleAmmo")             
             */

          //  Expedition exp = The.InGameUI.UIAllegiance.GetFirstExpedition();
            Allegiance al = The.InGameUI.UIAllegiance;
            EntityType gunType = GameData.Instance.AllEntityTypes["item:gunpowderRifle"];
            al.Statistics.AddProductionEvent(gunType, SimSide.Allegiances.Statistics.ProductionStatistics.StatTypes.Produced, 10);

            EntityType ammoType = GameData.Instance.AllEntityTypes["item:blackPowderRifleAmmo"];
            al.Statistics.AddProductionEvent(ammoType, SimSide.Allegiances.Statistics.ProductionStatistics.StatTypes.Produced, 20);
           
        }

        private void TestSetHidesProduced(string option, bool? newBool, float? newFloat)
        {
            
            EntityType hideType;

            Allegiance al = The.InGameUI.UIAllegiance;
           

          //  Expedition exp = The.InGameUI.UIAllegiance.GetFirstExpedition();
            hideType = GameData.Instance.AllEntityTypes["item:whipjawTannedHide"];
          //  exp.Statistics.AddProductionEvent(hideType, SimSide.Allegiances.Statistics.ProductionStatistics.StatTypes.Produced, 40);
            al.Statistics.AddProductionEvent(hideType, SimSide.Allegiances.Statistics.ProductionStatistics.StatTypes.Produced, 40);

            hideType = GameData.Instance.AllEntityTypes["item:megapodTannedHide"];
           // exp.Statistics.AddProductionEvent(hideType, SimSide.Allegiances.Statistics.ProductionStatistics.StatTypes.Produced, 20);
            al.Statistics.AddProductionEvent(hideType, SimSide.Allegiances.Statistics.ProductionStatistics.StatTypes.Produced, 20);

            hideType = GameData.Instance.AllEntityTypes["item:thunderChickenTannedHide"];
          //  exp.Statistics.AddProductionEvent(hideType, SimSide.Allegiances.Statistics.ProductionStatistics.StatTypes.Produced, 50);
            al.Statistics.AddProductionEvent(hideType, SimSide.Allegiances.Statistics.ProductionStatistics.StatTypes.Produced, 50);

        }


        private void TestRatingsAndPopulation(string option, bool? newBool, float? newFloat)
        {
            SpawnFood();
            SpawnGunsAndAmmo();
            SpawnUpgradedHuts();

            SpawnComfortItems();

            for (int i = 0; i < 20; i++)
            {
                SpawnNewMember();
            }               

        }

        private void TestFood(string option, bool? newBool, float? newFloat)
        {
            SpawnFood();
        }

        private static void SpawnFood()
        {
            Expedition exp = The.InGameUI.UIAllegiance.GetFirstExpedition();

            // spawn food
            for (int i = 0; i < 100; i++)
            {
                Entity type = new Entity(GameData.Instance.AllEntityTypes["item:smokedCarbonTail"]);
                PlaceGameEntities.AddColonyItem(type, exp.Location.Value);
            }
        }

        private void TestCoilRiflesAndAmmo(string option, bool? newBool, float? newFloat)
        {
            SpawnGunsAndAmmo();
        }

        private static void SpawnGunsAndAmmo()
        {
            Expedition exp = The.InGameUI.UIAllegiance.GetFirstExpedition();

            // spawn guns + ammo
            for (int i = 0; i < 20; i++)
            {
                Entity type;

                type = new Entity(GameData.Instance.AllEntityTypes["item:coilRifle"]);
                PlaceGameEntities.AddColonyItem(type, exp.Location.Value);

                type = new Entity(GameData.Instance.AllEntityTypes["item:coilRifleAmmo"]);
                PlaceGameEntities.AddColonyItem(type, exp.Location.Value);
                PlaceGameEntities.AddColonyItem(type, exp.Location.Value);
            }
        }

        private void TestBushDragonCarcass(string option, bool? newBool, float? newFloat)
        {
            SpawnBushDragonCarcass();
        }
        

        private void TestUpgradedHuts(string option, bool? newBool, float? newFloat)
        {
            SpawnUpgradedHuts();
        }

        private static void SpawnComfortItems()
        {
            Expedition exp = The.InGameUI.UIAllegiance.GetFirstExpedition();

            for (int i = 0; i < 20; i++)
            {
                Entity ammo = new Entity(GameData.Instance.AllEntityTypes["item:crystalBrandy"]);
                PlaceGameEntities.AddColonyItem(ammo, exp.Location.Value);
            }       
        }

        private static void SpawnBushDragonCarcass()
        {
            Expedition exp = The.InGameUI.UIAllegiance.GetFirstExpedition();
            Entity e = new Entity(GameData.Instance.AllEntityTypes["item:bushDragonCarcass"]);
            e.Bulk = 1f;
            PlaceGameEntities.AddColonyItem(e, exp.Location.Value);

        }

        private static void SpawnUpgradedHuts()
        {
            Expedition exp = The.InGameUI.UIAllegiance.GetFirstExpedition();
            Point expPos = MapManager.WorldPosToTile(exp.Location.Value);

            // spawn huts + upgrades
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    Entity hut = PlaceGameEntities.AddFinishedStructure("structure:clayHut", new Point(expPos.X + x * 2 + 2, expPos.Y + y * 2 + 2), exp);

                    UpgradeCategory cat;
                   
                    cat = GameData.Instance.AllUpgradeCategories["furniture4People"];
                    PlaceGameEntities.AddColonyItemUpgrade("item:furniture4People", hut, cat);

                    exp.OwnedEntities.SetUpgrade(hut.EntityID, cat, GameData.Instance.AllEntityTypes["item:furniture4People"]);

                    cat = GameData.Instance.AllUpgradeCategories["bedsOrMats4People"];
                    PlaceGameEntities.AddColonyItemUpgrade("item:beds4People", hut, cat);

                    exp.OwnedEntities.SetUpgrade(hut.EntityID, cat, GameData.Instance.AllEntityTypes["item:beds4People"]);
           
                }
            }
        }

        private void TestMember(string option, bool? newBool, float? newFloat)
        {
            SpawnNewMember();
        }

        private static void SpawnNewMember()
        {
            Expedition exp = The.InGameUI.UIAllegiance.GetFirstExpedition();
            Point expPos = MapManager.WorldPosToTile(exp.Location.Value);

            PlaceGameEntities.GetBob(expPos, exp);
        }


        private void TestSpraySentries(string option, bool? newBool, float? newFloat)
        {           
            Expedition exp = The.InGameUI.UIAllegiance.GetFirstExpedition();

            for (int x = 0; x < 5; x++)
            {
                Vector3 location = exp.Location.Value + new Vector3(x * 48f - 48f, -48f, 0f);

                SpawnSentry(exp, location);

            }

            for (int y = 0; y < 5; y++)
            {
                Vector3 location = exp.Location.Value + new Vector3(-48f, y * 48f - 48f, 0f);

                SpawnSentry(exp, location);
            }

            for (int i = 0; i < 20; i++)
            {
                Entity ammo = new Entity(GameData.Instance.AllEntityTypes["item:bushDragonCartridge"]);
                PlaceGameEntities.AddColonyItem(ammo, exp.Location.Value);         
            }          

        }

        private static void TestSpawnComfortItems(string option, bool? newBool, float? newFloat)
        {
            SpawnComfortItems();
        }

        private static Vector3 SpawnSentry(Expedition exp, Vector3 location)
        {
            Entity sentry = PlaceGameEntities.AddFinishedStructure("structure:sprayGunSentry",
                null, exp, false, location);

            Entity ammo = new Entity(GameData.Instance.AllEntityTypes["item:bushDragonCartridge"]);
            PlaceGameEntities.AddColonyItem(ammo, new Vector3(50f, 50f, 0f));
            Container magazine = sentry.Parts[0].Parts.FirstOrDefault(p => p.EntityType.KeyName == "item:sentrySprayGun").Contains;
            Entity surplusAmmo;
            magazine.AddToContain(ammo, out surplusAmmo);

            /* Entity sentry = PlaceGameEntities.AddFinishedStructure("structure:sentry", 
                 null, exp, false, exp.Location);

             Entity ammo = new Entity(GameData.Instance.AllEntityTypes["item:sentryGunAmmo"]);
             PlaceGameEntities.AddColonyItem(ammo, new Vector3(50f, 50f, 0f));
             Container magazine = sentry.Parts[0].Parts.FirstOrDefault(p => p.EntityType.KeyName == "item:sentryGun").Contains;
             Entity surplusAmmo;
             magazine.AddToContain(ammo, out surplusAmmo);*/
            return location;
        }

        #endregion


        private void PopulateAllegiances()
        {
            //Kensei.Dev.Options.RemoveOption()
            if (printAllegiancesRegulator.IsReady())
            {
                StringBuilder description = new StringBuilder();
                foreach (var item in The.Sim.PlaySite.Allegiances)
                {
                 //   Kensei.Dev.Options.SetOption("Allegiances." + item.Name, type);
                    string name = item.Name ?? item.KeyName ?? item.RepresentativeEntityType.PluralName;
                    description.Append(name + ": " + item.Members.Count);
                    int? max = item.GetMaxPopulationMembers();
                    if (max.HasValue)
                    {
                        description.Append("/" + max.Value);
                    }
                    description.AppendLine();
                }

                Kensei.Dev.Options.SetAllegiancesText(description.ToString());
            }

        }

        private void PopulateTestEvents(List<ActionSets> actions, List<PolledEventType> polled)
        {           
            Kensei.Dev.Options.PopulateTestEvents(actions);
            Kensei.Dev.Options.PopulatePolledTestEvents(polled);         

        }

        public void ScaleSelectedRenderable(string option, bool? newBool, float? newFloat)
        {
            if (!newFloat.HasValue)
                return;

            if (The.InGameUI.SelectedEntity == null)
                return;

            // Entity selected = The.InGameUI.SelectedEntity as Entity;
            Entity selected = Entity.FindByID(The.InGameUI.SelectedEntity.Value); // as Entity;

            if (selected == null)
                return;

            Renderable ren = selected.Renderable;
            if (ren == null)
                return;

            RenderAsModel ram = ren.RenderAsModel;
            if (ram == null)
                return;

            ram.FinalModelScale = newFloat.Value;

        }

        public void SetGaitMinimumSpeed(string option, bool? newBool, float? newFloat)
        {
            if (!newFloat.HasValue)
                return;

            if (The.InGameUI.SelectedEntity == null)
                return;

            Entity selected = Entity.FindByID(The.InGameUI.SelectedEntity.Value); // as Entity;

            if (selected == null)
                return;

            string[] parts = option.Split(new string[] { " - " }, StringSplitOptions.RemoveEmptyEntries);


        }

        public void YawSelectedRenderable(string option, bool? newBool, float? newFloat)
        {
            if (!newFloat.HasValue)
                return;

            if (The.InGameUI.SelectedEntity == null)
                return;

            Entity selected = Entity.FindByID(The.InGameUI.SelectedEntity.Value); // as Entity;

            if (selected == null)
                return;

            selected.SetRotationAndDir(newFloat.Value);

            // set the renderable properties too, so it will work while paused:           
            selected.Renderable.SetToParentLocation();

        }

        public const float MaxSpeed = 180f;

        /// <summary>
        /// this code causes a fps spike when selecting an entity!
        /// </summary>
        /// <param name="oldEntityID"></param>
        /// <param name="newEntity"></param>
        void DeveloperDialogSelectedEntityChangedEvent(EntityID? oldEntityID, EntityID? newEntity)
        {
            Kensei.Dev.Options.RemoveOptionsStartingWith("Attach.Attachor");
            Kensei.Dev.Options.RemoveOptionsStartingWith("Attach.Scale");
            Kensei.Dev.Options.RemoveOptionsStartingWith("Attach.Rotate");
            Kensei.Dev.Options.RemoveOptionsStartingWith("Attach.Translate");
            Kensei.Dev.Options.RemoveOptionsStartingWith("Attach.Pivot");

            string gaitOptionsKey = "Gait tuning.";
            string entitySpeedKey = "Gait tuning.Entity speed";

            Kensei.Dev.Options.RemoveOptionsStartingWith(gaitOptionsKey); // "Anim tuning.Gait");
            Kensei.Dev.Options.RemoveOptionsStartingWith(entitySpeedKey);

            regionMapOverlayOptions.Clear();
            overlayOptions.Clear();
            discomfortMapOverlayOptions.Clear();
            threatMapOverlayOptions.Clear();

            Kensei.Dev.Options.RemoveOptionsStartingWith("Overlays.Move map ");
            Kensei.Dev.Options.RemoveOptionsStartingWith("Overlays.Region map ");
            Kensei.Dev.Options.RemoveOptionsStartingWith("Overlays.Threat map ");
            Kensei.Dev.Options.RemoveOptionsStartingWith("Overlays.Discomfort map ");

            Kensei.Dev.Options.RemoveOptionsStartingWith("Anim tuning.Idle");

            The.MapUI.Overlays.Clear();

            // here we can reset the previously selected entity:
            if (oldEntityID != null)
            {
                Entity oldEntity = Entity.FindByID(oldEntityID.Value);

                if (oldEntity != null)
                {
                    if (oldEntity.Renderable.RenderAsModel != null && oldEntity.Locomotor != null)
                    {
                        oldEntity.Locomotor.MaximumSpeedDebugOnly = null;
                    }
                }
            }

            if (The.InGameUI.SelectedEntity != null)
            {
                Entity entity = Entity.FindByID(The.InGameUI.SelectedEntity.Value); // as Entity;

                if (entity == null || entity.Renderable == null)
                {
                    return;
                }

                if (entity.Renderable.RenderAsModel != null && entity.Locomotor != null)
                {

                    //having the scale bar above the attach options makes a nice visual separator
                    Kensei.Dev.Options.SetOption("Attach.Scale Selected Renderable",
                                                     entity.Renderable.RenderAsModel.FinalModelScale, 0.1f, 16.0f);
                    Kensei.Dev.Options.SetOptionCallback("Attach.Scale Selected Renderable", ScaleSelectedRenderable);


                    float rotation = Common.WrapAngleBetweenMinusPiAndPi(entity.Rotation);

                    Kensei.Dev.Options.SetOption("Attach.Pivot Selected Renderable", rotation, (float)(-Math.PI), (float)Math.PI);
                    Kensei.Dev.Options.SetOptionCallback("Attach.Pivot Selected Renderable", YawSelectedRenderable);


                    SetDebugAttachorOptions(entity);

                    // do attach options
                    foreach (KeyValuePair<string, AttachPoint> attachorOption in attachorOptions)
                    {
                        foreach (var attacheeOption in attacheeOptions)
                        {
                            if (AnimatedModel.HasAttachedModel(entity, attacheeOption.Value.Item1, attachorOption.Value.BoneName, attacheeOption.Value.Item2.BoneName))
                            {
                                Kensei.Dev.Options.SetOption(attachorOption.Key, true);
                            }

                        }

                    }

                    if (entity.Locomotor.LeggedLocomotor != null)
                    {
                        LeggedLocomotorType legType = entity.EntityType.LocomotorType.LeggedLocomotorType;

                       /* string key = "Anim tuning.Idle chance to talk";
                        Kensei.Dev.Options.SetOption(key, GoalDoTakeFive.IdleChanceToTalk, 0f, 1f);
                        Kensei.Dev.Options.SetOptionCallback(key, (s, b, f) => GoalDoTakeFive.IdleChanceToTalk = f.Value);
                        */
                        /*
                        string key = key = "Anim tuning.Idle chance to sit factor";
                        Kensei.Dev.Options.SetOption(key, legType.IdleChanceToSitFactor, 0f, 3f);
                        Kensei.Dev.Options.SetOptionCallback(key, (s, b, f) => legType.IdleChanceToSitFactor = f.Value);
                        key = "Anim tuning.Idle chance to kneel factor";
                        Kensei.Dev.Options.SetOption(key, legType.IdleChanceToKneelFactor, 0f, 3f);
                        Kensei.Dev.Options.SetOptionCallback(key, (s, b, f) => legType.IdleChanceToKneelFactor = f.Value);
                        key = "Anim tuning.Idle chance to stand factor";
                        Kensei.Dev.Options.SetOption(key, legType.IdleChanceToStandFactor, 0f, 3f);
                        Kensei.Dev.Options.SetOptionCallback(key, (s, b, f) => legType.IdleChanceToStandFactor = f.Value);
                        key = "Anim tuning.Idle chance to lay down factor";
                        Kensei.Dev.Options.SetOption(key, legType.IdleChanceToLayDownFactor, 0f, 3f);
                        Kensei.Dev.Options.SetOptionCallback(key, (s, b, f) => legType.IdleChanceToLayDownFactor = f.Value);*/

                        /*  key = "Anim tuning.Idle chance to smoke factor";
                          Kensei.Dev.Options.SetOption(key, GoalDoTakeFive.IdleChanceToSmokeFactor, 0f, 3f);
                          Kensei.Dev.Options.SetOptionCallback(key, (s, b, f) => GoalDoTakeFive.IdleChanceToSmokeFactor = f.Value);*/
                        //IdleRemainInCurrentSitOrStandStanceAddend
                        
                        /*key = "Anim tuning.Idle remain in current sit or stand stance addend"; // a number that creates inertia to stay sitting or standing
                        Kensei.Dev.Options.SetOption(key, legType.IdleRemainInCurrentSitOrStandStanceAddend, 0f, 3f);
                        Kensei.Dev.Options.SetOptionCallback(key, (s, b, f) => legType.IdleRemainInCurrentSitOrStandStanceAddend = f.Value);*/
                    }

                }

                if (entity.Intelligence != null)
                {
                    Kensei.Dev.Options.SetOption("Anim.Disable AI", entity.Intelligence.DisableAI);

                    SetDebugOverlayOptions(entity);
                }
                else
                {
                    Kensei.Dev.Options.RemoveOptionsStartingWith("Anim.Disable AI");
                }

                if (entity.Renderable.RenderAsModel != null && entity.Locomotor != null)
                {
                    Kensei.Dev.Options.SetOption("Anim.Rotate slowly", entity.Locomotor.RotateSlowly);

                }
                else
                {
                    Kensei.Dev.Options.RemoveOptionsStartingWith("Anim.Rotate slowly");
                }

                if (entity.Renderable.RenderAsModel != null && entity.Locomotor != null && entity.Locomotor.LeggedLocomotor != null)
                {
                    Kensei.Dev.Options.SetOption("Anim.Wander", entity.Locomotor.LeggedLocomotor.TestWander);
                }
                else
                {
                    Kensei.Dev.Options.RemoveOptionsStartingWith("Anim.Wander");
                }

                //    Kensei.Dev.Options.SetOption("Dev.Stealth selected entity <-- NEW --", entity.Stealth);




                if (entity.Renderable.RenderAsModel != null && entity.Locomotor != null)
                {

                    // we set this to null when entity gets deselected - this makes him go back to normal speed control
                    Kensei.Dev.Options.SetOption(entitySpeedKey, entity.Locomotor.MaximumSpeedDebugOnly.HasValue ? entity.Locomotor.MaximumSpeedDebugOnly.Value : 5f, 5f, MaxSpeed);
                    Kensei.Dev.Options.SetOptionCallback(entitySpeedKey, (s, b, f) => entity.Locomotor.MaximumSpeedDebugOnly = f.Value);

                    if (entity.EntityType.RenderableTypeMode.RenderAsModelType.GaitAnimations != null)
                    {
                        foreach (var item in entity.EntityType.RenderableTypeMode.RenderAsModelType.GaitAnimations)
                        {

                            foreach (var anim in item.Value)
                            {
                                var thisAnim = anim; // necessary for correct lambda expressions (callbacks): http://blogs.msdn.com/b/ericlippert/archive/2009/11/12/closing-over-the-loop-variable-considered-harmful.aspx

                                string thisKey = gaitOptionsKey + " - " + item.Key + " - " + anim.AnimationKey + " - minimum speed";

                                Kensei.Dev.Options.SetOption(thisKey, anim.MinimumSpeed, 0f, MaxSpeed);
                                //GameData.Instance.AllEntityTypes["entity:human"].RenderableType.RenderAsModelType.AnimConditions.First(c => c.AnimationSet.BaseAnimations.Contains("gaitWalk")).SpeedFactor, 0.1f, 3f);
                                Kensei.Dev.Options.SetOptionCallback(thisKey, (s, b, f) => thisAnim.MinimumSpeed = f.Value); //SetGaitMinimumSpeed); // 
                                //     GameData.Instance.AllEntityTypes["entity:human"].RenderableType.RenderAsModelType.AnimConditions.First(c => c.AnimationSet.BaseAnimations.Contains("gaitWalk")).SpeedFactor = f.Value);

                                thisKey = gaitOptionsKey + " - " + item.Key + " - " + anim.AnimationKey + " - maximum speed";

                                Kensei.Dev.Options.SetOption(thisKey, Common.ClampTop(anim.MaximumSpeed, MaxSpeed), 0f, MaxSpeed);
                                //GameData.Instance.AllEntityTypes["entity:human"].RenderableType.RenderAsModelType.AnimConditions.First(c => c.AnimationSet.BaseAnimations.Contains("gaitWalk")).SpeedFactor, 0.1f, 3f);
                                Kensei.Dev.Options.SetOptionCallback(thisKey, (s, b, f) =>
                                    thisAnim.MaximumSpeed = f.Value);
                                //     GameData.Instance.AllEntityTypes["entity:human"].RenderableType.RenderAsModelType.AnimConditions.First(c => c.AnimationSet.BaseAnimations.Contains("gaitWalk")).SpeedFactor = f.Value);

                            }

                        }
                    }
                    else
                    {
                        //Perhaps there should be an assert here
                    }

                }
                /* else
                 {
                     Kensei.Dev.Options.RemoveOptionsStartingWith(gaitOptionsKey); // "Anim tuning.Gait");
                     Kensei.Dev.Options.RemoveOptionsStartingWith(entitySpeedKey);
                 }*/

            }


            string animLabel1 = "Anim.Test anim 1: ";
            string animLabel2 = "Anim.Test anim 2: ";
            string animLabel3 = "Anim.Test anim 3: ";

            animationOptions.Clear();
            Kensei.Dev.Options.RemoveOptionsStartingWith(animLabel1);
            Kensei.Dev.Options.RemoveOptionsStartingWith(animLabel2);
            Kensei.Dev.Options.RemoveOptionsStartingWith(animLabel3);


            if (The.InGameUI.SelectedEntity != null)
            {
                Entity entity = Entity.FindByID(The.InGameUI.SelectedEntity.Value); // as Entity;
                if (entity == null)
                {
                    return;
                }

                if (entity.Renderable.RenderAsModel != null)
                {
                    // this call takes a long time:
                    SetDebugAnimOptions(entity, animLabel1, animLabel2, animLabel3);
                }
            }
        }

        private void SetDebugOverlayOptions(Entity entity)
        {
            if (The.Sim.Mode == UWGame.SimSide.Sim.EngineMode.Game)
            {


                if (entity.Intelligence.Allegiance != null &&
                    entity.Intelligence.Allegiance.SharedKnowledge.PlaySiteKnowledge.AllMovementMaps != null)
                {
                    foreach (var kvp1 in entity.Intelligence.Allegiance.SharedKnowledge.PlaySiteKnowledge.AllMovementMaps)
                    {
                        foreach (var kvp2 in kvp1.Value)
                        {
                            foreach (var kvp3 in kvp2.Value)
                            {
                                string option = "Overlays.Move map " + kvp3.Value.IDName;
                                overlayOptions.Add(option, kvp3.Value);
                                Kensei.Dev.Options.SetOption(option, false);
                                Kensei.Dev.Options.SetOptionCallback(option, MovemapClick);
                            }
                        }
                    }

                    foreach (var kvp1 in entity.Intelligence.Allegiance.SharedKnowledge.PlaySiteKnowledge.AllMovementMaps)
                    {
                        foreach (var kvp2 in kvp1.Value)
                        {
                            foreach (var kvp3 in kvp2.Value)
                            {
                                SetDebugOverlayRegionMapOption(kvp3, SurfaceType.TransportType.Foot);
                                SetDebugOverlayRegionMapOption(kvp3, SurfaceType.TransportType.OffRoad);
                            }
                        }
                    }

                    foreach (var kvp1 in entity.Intelligence.Allegiance.SharedKnowledge.PlaySiteKnowledge.AllMovementMaps)
                    {
                        foreach (var kvp2 in kvp1.Value)
                        {
                            foreach (var kvp3 in kvp2.Value)
                            {
                                DiscomfortMap dMap = kvp3.Value.GetDiscomfortMap();
                                string option = "Overlays.Discomfort map " + dMap.IDName;
                                discomfortMapOverlayOptions.Add(option, dMap);
                                Kensei.Dev.Options.SetOption(option, false);
                                Kensei.Dev.Options.SetOptionCallback(option, DiscomfortMapClick);
                            }
                        }
                    }

                    foreach (var kvp1 in entity.Intelligence.Allegiance.SharedKnowledge.PlaySiteKnowledge.ThreatMaps)
                    {
                        foreach (var kvp2 in kvp1.Value)
                        {
                            string option = "Overlays.Threat map " + kvp2.Value.IDName;
                            threatMapOverlayOptions.Add(option, kvp2.Value);
                            Kensei.Dev.Options.SetOption(option, false);
                            Kensei.Dev.Options.SetOptionCallback(option, ThreatMapClick);
                           
                        }
                    }
                }

            }
        }

        /*  public void LoadLoseGameScreen()
          {
           
              foreach (GameScreen screen in ScreenManager.GetScreens())
                  screen.ExitScreen();

              ScreenManager.AddScreen(new BackgroundScreen(BackgroundScreen.Background.Normal));
              ScreenManager.AddScreen(new LoseGameScreen(The.Sim.ScreenManager));
          }*/





        private void SetDebugOverlayRegionMapOption(KeyValuePair<ThreatStance, MovementMap> kvp3, SurfaceType.TransportType transport)
        {
            SubtileLayers layers;
            if (kvp3.Value.Layers.TryGetValue(transport, out layers))
            {
                string option = "Overlays.Region map " + layers.RegionMap.IDName; // +transport.ToString();
                regionMapOverlayOptions.Add(option, layers.RegionMap);
                Kensei.Dev.Options.SetOption(option, false);
                Kensei.Dev.Options.SetOptionCallback(option, RegionMapClick);
            }
        }

        private void SetDebugAnimOptions(Entity entity, string animLabel1, string animLabel2, string animLabel3)
        {
            Kensei.Dev.Options.SetOption(animLabel1 + "Looping", true);
            string option;

            foreach (KeyValuePair<string, AnimationController> kvp in entity.Renderable.RenderAsModel.AnimatedModel.ModelAnimator.AnimationControllers)
            {
                option = animLabel1 + kvp.Key;
                animationOptions.Add(option, kvp.Key);
                Kensei.Dev.Options.SetOption(option, false);
                Kensei.Dev.Options.SetOptionCallback(option, TestAnims);
            }

            // string option = animLabel2 + "walk";
            //  animationOptions.Add(option, "walk");
            //  Kensei.Dev.Options.SetOption(option, false);
            //  Kensei.Dev.Options.SetOptionCallback(option, TestAnims);

            Kensei.Dev.Options.SetOption(animLabel2 + "Looping", true);
            foreach (KeyValuePair<string, AnimationController> kvp in entity.Renderable.RenderAsModel.AnimatedModel.ModelAnimator.AnimationControllers)
            {
                option = animLabel2 + kvp.Key;
                animationOptions.Add(option, kvp.Key);
                Kensei.Dev.Options.SetOption(option, false);
                Kensei.Dev.Options.SetOptionCallback(option, TestAnims);
            }

            Kensei.Dev.Options.SetOption(animLabel3 + "Looping", true);
            foreach (KeyValuePair<string, AnimationController> kvp in entity.Renderable.RenderAsModel.AnimatedModel.ModelAnimator.AnimationControllers)
            {
                option = animLabel3 + kvp.Key;
                animationOptions.Add(option, kvp.Key);
                Kensei.Dev.Options.SetOption(option, false);
                Kensei.Dev.Options.SetOptionCallback(option, TestAnims);
            }
        }
        public void BuildClicked(object sender)
        {

        }

        #region DevDebugMethods

        // Test Threat!
        private void PlaceThreat_OnPress(string option, bool? newBool, float? newFloat)
        {
            The.InGameUI.InterfaceMode = UWGame.ClientSide.Interface.InGameInterface.InterfaceState.Threat;
        }

        
        /// <summary>
        /// shows the resource noise maps
        /// </summary>
        /// <param name="option"></param>
        /// <param name="newBool"></param>
        /// <param name="newFloat"></param>
        private void Resource_OnPress(string option, bool? newBool, float? newFloat)
        {
            ResourceType resourceType = resourceTypeOverlayOptions[option];

            if (newBool == true && !The.MapUI.Overlays.Contains(resourceType))
            {
                The.MapUI.ResourceOverlays.Add(resourceType);
            }
            else
            {
                The.MapUI.ResourceOverlays.Remove(resourceType);
            }

        }


        private void UnblockTile_OnPress(string option, bool? newBool, float? newFloat)
        {
            The.InGameUI.InterfaceMode = UWGame.ClientSide.Interface.InGameInterface.InterfaceState.UnblockSubtile;

            /*The.InGameUI.SelectedTiles.IterateArea(selTile =>
                {
                    if (selTile != null)
                    {
                        Vector2 from, to;
                        from = MapManager.SubTileToWorldPos(
                            MapManager.TileEdgeToSubtile(selTile.TilePos).ToPoint());

                        to = from + new Vector2(MapManager.subTileSize * 2, MapManager.subTileSize * 2);

                        MapManager.IterateSubtiles(from, to,
                            s =>
                            {
                                The.Map.SetSubtileTerrainCost(s.ToVector3(), 2); // 0 = blocked!
                               // The.Map.SetSubtileTerrainCostToSurfaceType(s.ToVector3());
                            });

                    }

                });*/
        }


        private void BlockTile_OnPress(string option, bool? newBool, float? newFloat)
        {

            The.InGameUI.InterfaceMode = UWGame.ClientSide.Interface.InGameInterface.InterfaceState.BlockSubtile;
            
            The.InGameUI.SelectedTiles.IterateArea(selTile =>
                {
                    if (selTile != null)
                    {
                        Vector2 from, to;
                        from = MapManager.SubTileToWorldPos(
                            MapManager.TileEdgeToSubtile(selTile.TilePos).ToPoint());

                        to = from + new Vector2(MapManager.subTileSize * 2, MapManager.subTileSize * 2);

                        MapManager.IterateSubtiles(from, to,
                            s =>
                            {
                                EntityData entityData = new EntityData()
                                {
                                    EntityKey = "structure:abatis",
                                    Location = s.ToVector3(),

                                };
                                bool failed;
                                MapLoader.CreateAndPlaceEntityFromEntityData(entityData, out failed);
                            });

                       

                        //  The.Map.SetTileCost(selTile.X, selTile.Y, 0);
                    }
                });

            /* foreach (var selTile in The.InGameUI.SelectedTiles.Coverage)
             {
                 if (selTile != null)
                 {
                     The.Map.SetTileCost(selTile.X, selTile.Y, 0);
                 }
             }*/
        }

      /*  private void BlockEdge_OnPress(string option, bool? newBool, float? newFloat)
        {
            The.InGameUI.InterfaceMode = UWGame.ClientSide.Interface.InGameInterface.InterfaceState.BlockSubtile;
        
        }*/


        private void SetEmigrateDecision(Entity man)
        {
            if (man.Intelligence.EmigrateDecider.PreferredMigrationTarget.HasValue)
            {
                man.Intelligence.Memory.SetEmigrateDecision(man.Intelligence.EmigrateDecider.PreferredMigrationTarget.Value, man);
            }
        }



        private void StarveNearDeath_OnPress(string option, bool? newBool, float? newFloat)
        {
            Entity man = GetSelectedAgentOrFirstPerson();

            if (man != null)
            {
                SetStarving(man, true);   
            }
        }

        private void Starve_OnPress(string option, bool? newBool, float? newFloat)
        {
            Entity man = GetSelectedAgentOrFirstPerson();

            if (man != null)
            {
                SetStarving(man);   
            }
        }


        private void AddCredits(string option, bool? newBool, float? newFloat)
        {
            The.InGameUI.UIAllegiance.TradeCredits += 500;
              
        }

        private void IntegrityDamage_OnPress(string option, bool? newBool, float? newFloat)
        {
            if (The.InGameUI.SelectedEntity.HasValue)
            {
                Entity entity = Entity.FindByID(The.InGameUI.SelectedEntity.Value);
                if (entity != null)
                {
                    if (entity.EntityType.NonLivingType != null)
                    {
                        entity.NonLivingEntity.DoIntegrityDamage(0.6f);
                    }
                }
            }
        }

        private void AddLogMessage_OnPress(string option, bool? newBool, float? newFloat)
        {           
            The.Client.Log.AddLogEvent(The.Client.Log.EconomicEvent, null, "Test log event"); 
        }

        private void Damage_OnPress(string option, bool? newBool, float? newFloat)
        {
            if (The.InGameUI.SelectedEntity.HasValue)
            {
                Entity entity = Entity.FindByID(The.InGameUI.SelectedEntity.Value);
                if (entity != null)
                {
                    if (entity.EntityType.NonLivingType != null)
                    {
                        entity.DoDamage(0.6f);
                    }
                }
            }
        }
        

        private void Immobilize_OnPress(string option, bool? newBool, float? newFloat)
        {
            Entity man = GetSelectedAgentOrFirstPerson();

            if (man != null && man.Locomotor != null)
            {
                man.Locomotor.ToggleImmobilize();               
            }
        }

        private void RaiseComfortPrinciples(string option, bool? newBool, float? newFloat)
        {
            Entity man = GetSelectedAgentOrFirstPerson();

            if (man != null && man.PersonEntity != null)
            {
                man.PersonEntity.Personality.Principles[SimSide.Allegiances.Statistics.RatingTypes.Comfort] += 0.1f;
            }
        }

        private void RaiseFoodPrinciples(string option, bool? newBool, float? newFloat)
        {
            Entity man = GetSelectedAgentOrFirstPerson();

            if (man != null && man.PersonEntity != null)
            {
                man.PersonEntity.Personality.Principles[SimSide.Allegiances.Statistics.RatingTypes.Food] += 0.1f;
            }
        }
        private void RaiseSecurityPrinciples(string option, bool? newBool, float? newFloat)
        {
            Entity man = GetSelectedAgentOrFirstPerson();

            if (man != null && man.PersonEntity != null)
            {
                man.PersonEntity.Personality.Principles[SimSide.Allegiances.Statistics.RatingTypes.Security] += 0.1f;
            }
        }
        private void Injure_OnPress(string option, bool? newBool, float? newFloat)
        {
            Entity man = GetSelectedAgentOrFirstPerson();

            if (man != null)
            {
                UWGame.SimSide.Entities.Body.BodyComponent body;

                man.Find(out body);
               // body.Body.GlobalHitpoints = 20;

                man.LogInjuryDescription("Test injury");

            }
        }

        private void Sleep_OnPress(string option, bool? newBool, float? newFloat)
        {
            Entity man = GetSelectedAgentOrFirstPerson();

            if (man != null)
            {
               
               man.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0f;
               man.BiologicalEntity.Needs.NeedsList["sleep"].PhysicalNeed.DaysAtZero = 1f;
               
                //  SetStarving(man);   

            }

        }

        private void Emigrate_OnPress(string option, bool? newBool, float? newFloat)
        {
          /*  UWGame.SimSide.Allegiances.Allegiance all2 = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:binalRat"], "all2");
            Expedition exp1 = new Expedition(new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:binalRat"]), "Start2", "Start2", MapManager.TileToWorldPos(new Point(12, 5)));
            Entity e2 = PlaceGameEntities.PlaceAnimal("entity:binalRat", Reproduction.Female, new Point(12, 5), 18, all2, null, exp1);
            */
            Entity man = GetSelectedAgentOrFirstPerson();

            if (man != null)
            {
                man.Intelligence.Memory.ResetTimepointForJoiningExpedition();
                SetEmigrateDecision(man);

                /*  man.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0f;
               man.BiologicalEntity.Needs.NeedsList["sleep"].PhysicalNeed.DaysAtZero = 1f;
               */

                //  SetStarving(man);   

            }

        }

        private static Entity GetSelectedAgentOrFirstPerson()
        {
            Entity man = null;
            if (The.InGameUI.SelectedEntity.HasValue)
            {
                Entity entity = Entity.FindByID(The.InGameUI.SelectedEntity.Value);
                if (entity.Intelligence != null)
                {
                    man = entity;
                }
            }

            if (man == null && The.Sim.PlaySite.PlayerAllegiance.Persons.Count > 0)
            {
                man = The.Sim.PlaySite.PlayerAllegiance.Persons[0];
            }
            return man;
        }

        private static void SetStarving(Entity man, bool nearDeath = false)
        {
            Need need;
            if (man.BiologicalEntity.Needs.NeedsList.TryGetValue("protein", out need))
            {
                need.CurrentLevel = 0f;
                if (nearDeath)
                {
                    need.PhysicalNeed.DaysAtZero = 2f;
                }
            }
            if (man.BiologicalEntity.Needs.NeedsList.TryGetValue("foodEnergy", out need))
            {
                need.CurrentLevel = 0f;
                if (nearDeath)
                {
                    need.PhysicalNeed.DaysAtZero = 2f;
                }
            }
            if (man.BiologicalEntity.Needs.NeedsList.TryGetValue("micronutrients", out need))
            {
                need.CurrentLevel = 0f;
                if (nearDeath)
                {
                    need.PhysicalNeed.DaysAtZero = 2f;
                }
            }

           /* man.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0f; //0.2f       
            man.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0f; //0.2f       
            man.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 0f; //0.2f */      
            man.BiologicalEntity.AddToStomachContents(-1f);

          /*  if (nearDeath)
            {
                man.BiologicalEntity.Needs.NeedsList["protein"].PhysicalNeed.DaysAtZero = 2f;
                man.BiologicalEntity.Needs.NeedsList["foodEnergy"].PhysicalNeed.DaysAtZero = 2f;
                man.BiologicalEntity.Needs.NeedsList["micronutrients"].PhysicalNeed.DaysAtZero = 2f;
            }*/
        }

        private void MergeHouseholds_OnPress(string option, bool? newBool, float? newFloat)
        {
            if (!newBool.HasValue)
                return;

            if (The.Sim.PlaySite.PlayerAllegiance.Persons.Count > 1 && The.Sim.PlaySite.GetFirstPlayerExpedition().Households.Count > 1)
            {   // merge 1st and last households:
                The.Sim.PlaySite.GetFirstPlayerExpedition().Households[0].MergeHouseholds(The.Sim.PlaySite.GetFirstPlayerExpedition().Households[The.Sim.PlaySite.GetFirstPlayerExpedition().Households.Count - 1]);
                // merge households 0 and 1:
                //((PersonEntity)Entities[0]).Household.MergeHouseholds(((PersonEntity)Entities[1]).Household);
            }

            /*Jobs.HaulingJob job = new Jobs.HaulingJob(new Point(1,1), ColonyJobs);
            job.Item = ColonyOwner.OwningBody.Items[Items.IronOreType.Instance][0];           
            */
        }

        private void ClearTile_OnPress(string option, bool? newBool, float? newFloat)
        {
            if (The.InGameUI.SelectedEntity != null)
            {
                //Entity entity = The.InGameUI.SelectedEntity as Entity;
                Entity entity = Entity.FindByID(The.InGameUI.SelectedEntity.Value); // as Entity;

                if (entity == null)
                    return;

                entity.Destroy();

                The.InGameUI.SelectEntity(null);              
            }
            else
            {

                The.InGameUI.SelectedTiles.IterateArea(tile =>
                {
                    The.Map.ClearTile(tile);
                });
            }
        }

        private void CancelEntityJob_OnPress(string option, bool? newBool, float? newFloat)
        {
            if (The.InGameUI.SelectedEntity != null)
            {
                Entity entity = Entity.FindByID(The.InGameUI.SelectedEntity.Value); // as Entity;          
                if (entity == null)
                    return;

                entity.SendMessage(new Message(null, Message.MessageTypes.CancelJobOrItemInUse, Message.CancelJobKeepVehicle.KeepVehicle));
            }
        }

        private void Kill_OnPress(string option, bool? newBool, float? newFloat)
        {
            // calling from  outside the main thread??? this can cause a crash if the main thread is iterating???
            if (The.InGameUI.SelectedEntity != null)
            {
                Entity entity = Entity.FindByID(The.InGameUI.SelectedEntity.Value); // as Entity;          
                if (entity == null)
                    return;


                if (entity.EntityType.BiologicalType != null)
                {
                    entity.Kill(null);
                }
                else
                {
                    entity.Destroy();
                }

            }
        }
 

        private void GodMode_OnPress(string option, bool? newBool, float? newFloat)
        {
            if (newBool == false)
            {
                The.InGameUI.FogMap.DisplayWindow.Show();
            }
            else
            {
                The.InGameUI.FogMap.DisplayWindow.Hide();
            }
        }

        private void FogOfWar_OnPress(string option, bool? newBool, float? newFloat)
        {
            if (newBool == true)
            {
                The.InGameUI.FogMap.DisplayWindow.Show();
            }
            else
            {
                The.InGameUI.FogMap.DisplayWindow.Hide();
            }
        }

        /*  private void Stealth_OnPress(string option, bool? newBool, float? newFloat)
          {
              if (!newBool.HasValue)
                  return;

               if (The.InGameUI.SelectedEntity != null)
               {
                   Entity entity = The.InGameUI.SelectedEntity as Entity;
                   if (entity == null)
                       return;

                   entity.Stealth = newBool.Value;
               }
          }*/

        private void PlaceTree_OnPress(string option, bool? newBool, float? newFloat)
        {
            /*   TerrainTile selTile = The.InGameUI.SelectedTiles;
               if (selTile != null)
               {
                   //Tree treeToPlant = new Tree(GameData.Instance.AllTreeTypes["tree:spoak"]);
                   Entity treeToPlant = new Entity(GameData.Instance.AllTreeTypes["tree:spoak"]);
                   Point pos = new Point(selTile.X, selTile.Y);
                   treeToPlant.PlaceGroundFeature(pos, Common.Direction.North, MapManager.TileAndDirectionToWorldPos(pos, Common.Direction.North));

                   //treeToPlant.Tree.Place(MapManager.TileToWorldPos(selTile), Common.Direction.North);

               }*/

        }



        private void DeattachObject(Entity entity, IAttachable objectToAttach, string attachorBoneName)
        {
            if (!entity.Renderable./*TODO DECOUPLE*/RenderAsModel.AnimatedModel.ModelAnimator.HasAttachedObject(attachorBoneName))
            {
                // hmmm...

                //  BonePose attachor;

                //  attachor = entity.Renderable./*TODO DECOUPLE*/RenderAsModel.AnimatedModel.ModelAnimator.BonePoses[attachorBoneName];

                entity.Renderable./*TODO DECOUPLE*/RenderAsModel.AnimatedModel.ModelAnimator.DeattachObject(objectToAttach, attachorBoneName);

            }
        }





        private void Wander_OnPress(string option, bool? newBool, float? newFloat)
        {

            if (The.InGameUI.SelectedEntity != null)
            {
                Entity entity = Entity.FindByID(The.InGameUI.SelectedEntity.Value); // as Entity;          
                if (entity != null && entity.Renderable.RenderAsModel != null && entity.Locomotor.LeggedLocomotor != null) // && The.InGameUI.SelectedEntity.PersonEntity == null)
                {
                    if (newBool == true)
                    {
                        entity.SendMessage(new Message(null, Message.MessageTypes.CancelJobOrItemInUse, Message.CancelJobKeepVehicle.KeepVehicle));
                    }

                    entity.Locomotor.LeggedLocomotor.TestWander = newBool.Value;
                }
            }
        }

        private void SetUIToSelectedEntityAllegiance(string option, bool? newBool, float? newFloat)
        {
            if (newBool == true)
            {
                The.InGameUI.SetUIAllegianceToSelectedEntity();
                
            }
            else
            {
                The.InGameUI.ResetUIAllegiance();
            }

        }


        private void LaunchSelected(string option, bool? newBool, float? newFloat)
        {
            if (newBool == true)
            {
                The.InGameUI.InterfaceMode = InGameInterface.InterfaceState.Launch;
            }
            else
            {
                The.InGameUI.InterfaceMode = InGameInterface.InterfaceState.None;
            }

        }

        private void DisableAI_OnPress(string option, bool? newBool, float? newFloat)
        {
            if (The.InGameUI.SelectedEntity != null)
            {
                Entity entity = Entity.FindByID(The.InGameUI.SelectedEntity.Value); // as Entity;          
                if (entity != null && entity.Intelligence != null)
                {
                    if (newBool == true)
                    {
                        entity.SendMessage(new Message(null, Message.MessageTypes.CancelJobOrItemInUse, Message.CancelJobKeepVehicle.KeepVehicle));
                    }

                    entity.Intelligence.DisableAI = newBool.Value;
                }
            }
        }

        private void RotateSlowly_OnPress(string option, bool? newBool, float? newFloat)
        {
            if (The.InGameUI.SelectedEntity != null)
            {
                Entity entity = Entity.FindByID(The.InGameUI.SelectedEntity.Value); // as Entity;          
                if (entity != null && entity.Locomotor != null)
                {
                    entity.Locomotor.RotateSlowly = newBool.Value;
                }
            }
        }

        private void PlaceItem_OnPress(string option, bool? newBool, float? newFloat)
        {
            /*  TerrainTile selTile = The.InGameUI.SelectedTiles;
              if (selTile != null)
              {
                    Item item = new Item(GameData.Instance.AllItemTypes["item:meat"]);
                    AddColonyItem(item, new Point(selTile.X, selTile.Y));
              }
  */
        }



        /* private void SetPixelShaderVersion_OnPress(string option, bool? newBool, float? newFloat)
         {
             ScreenManager.PixelShaderVersion = (newValue ? 2 : 3);
         }*/


        private void SetResolutionDelegate(string[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                default:
                    Kensei.Dev.Command.Print("Sets display resolution. Usage: SetResolution <width> <height>");
                    break;
                case 3:
                    try
                    {
                        int width = int.Parse(parameters[1]);
                        int height = int.Parse(parameters[2]);
                        Controller.Game.GraphicsDeviceManager.PreferredBackBufferWidth = width;
                        Controller.Game.GraphicsDeviceManager.PreferredBackBufferHeight = height;
                        Controller.Game.GraphicsDeviceManager.ApplyChanges();
                    }
                    catch (System.Exception)
                    {
                        Kensei.Dev.Command.Print("Invalid resolution.");
                    }
                    break;
            }
        }

        #endregion





        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
        public override void HandleInput(/*InputState input*/)
        {
            
                     
            if (The.LoadScreen.IsLoadFinished == false)
            {
                return;
            }

            Options options = Controller.Options;

            bool toggleIngameMenu = false;

            if (EnableKeyboardShortcuts)
            {
                toggleIngameMenu = inputData.IsKeyTapped(options.ToggleIngameMenu);
            }

            bool wasHidden = false;
            if (toggleIngameMenu)
            {                
                // allowed while modal:
                if (The.InGameUI.MenuDialog.Window.IsVisibleAndActive)
                {
                    wasHidden = true;
                    The.InGameUI.HideInGameMenu();
                }               
            }


            if (IsModal)
            {
                return;
            }

            // not allowed while modal:
            if (toggleIngameMenu)
            {
                if (wasHidden == false && !The.InGameUI.MenuDialog.Window.IsVisibleAndActive)
                {                       
                    The.InGameUI.ShowInGameMenu();
                }                              
            }

            

           /* if (inputData.IsKeyTapped(Keys.B))
            {
                BloomEnabled = !BloomEnabled;
            }

            if (inputData.IsKeyTapped(Keys.E))
            {
                EdgeDetectEnabled = !EdgeDetectEnabled;
            }*/

            /*   if (keyboard.IsKeyTapped(Keys.F1))
               {
                   The.InGameUI.Help.Show();
               }*/

            if (The.Sim.Mode == Sim.EngineMode.Game)
            {
                

                if (!The.Sim.IsGameOver)
                {
                    if (EnableKeyboardShortcuts)
                    {
                        // prevent key clashes with if else if:
                        if (toggleIngameMenu)
                        {
                            return;
                        }
                        else if (inputData.IsKeyTapped(Keys.F2))// hide radio event screen communicator panel morten collapse                
                        {
                            The.InGameUI.LogPanel.Hide();
                        }
                        if (inputData.IsKeyTapped(options.KeyPause1) || inputData.IsKeyTapped(options.KeyPause2) || inputData.IsKeyTapped(options.KeyPause3))
                        {
                            if (The.Sim.IsPaused)
                            {
                                ResumeGame();
                            }
                            else
                            {
                                PauseGame();
                            }
                        }
                        else if (inputData.IsKeyTapped(options.KeyGameSpeed1))
                        {
                            SetGameSpeed(Speeds.Normal);
                        }
                        else if (inputData.IsKeyTapped(options.KeyGameSpeed2))
                        {
                            SetGameSpeed(Speeds.TwiceNormal);
                        }
                        else if (inputData.IsKeyTapped(options.KeyGameSpeed3))
                        {
                            SetGameSpeed(Speeds.FourTimesNormal);
                        }
                        else if (inputData.IsKeyTapped(options.CloseRosterPanel))
                        {
                            The.InGameUI.CloseRosterPanel();
                        }
                    }

                }
            }
        }

        public void UpdateFrameRate()
        {
            frameRateElapsedTime += The.Sim.GameTime.ElapsedGameTime;
            

            if (frameRateElapsedTime > TimeSpan.FromSeconds(1))
            {
                frameRateElapsedTime -= TimeSpan.FromSeconds(1);
                frameRate = frameCounter;
                frameCounter = 0;
            }

        }

        #region Commands

        public virtual void StartBuild()
        {
            The.InGameUI.EntitiesBeingPlaced.Clear();
            The.InGameUI.InterfaceMode = InGameInterface.InterfaceState.None;//Perhaps things like this should be handled only on click?

            // play a sound:
            The.InGameUI.gui.PlaySound(GUIManager.PlaceBuildingBeep);            
        }

        public virtual void SetProduction(string entityTypeKey)
        {
            The.InGameUI.OnSetProduction(entityTypeKey);

        }

        public virtual void OnHuntCreature()
        {
            The.InGameUI.HUDActionPanel.OnHuntCreature();
        }
        public virtual void OnSalvageEntity()
        {
            The.InGameUI.HUDActionPanel.OnSalvageEntity();
        }

        public virtual void OnSpecialAction()
        {
            // play a sound:
            The.InGameUI.gui.PlaySound(GUIManager.PlaceBuildingBeep);
        }

        public virtual void OnPlaceExpedition()
        {
            The.MapUI.OnPlaceExpedition();
        }

        public virtual void OnDiscardEntity()
        {
            The.InGameUI.HUDActionPanel.OnDiscardEntity();
        }

        public virtual void OnClaimEntity()
        {
            The.InGameUI.HUDActionPanel.OnClaimEntity();
        }

        public virtual void OnScoutArea(Zone scoutArea)
        {
            The.InGameUI.ContextMenu.OnScoutArea(scoutArea);
        }

        public virtual void OnCreateStockpile(Zone zone)
        {
            The.InGameUI.ContextMenu.OnCreateStockpile(zone);
        }

        public virtual void OnForageArea(Zone forageArea)
        {
            The.InGameUI.ContextMenu.OnForageArea(forageArea);
        }

        public virtual void OnHuntArea(Zone huntArea)
        {
            The.InGameUI.ContextMenu.OnForageArea(huntArea);
        }

        public virtual void OnPatrolOrAttackArea(Zone patrolArea)
        {
            The.InGameUI.ContextMenu.OnForageArea(patrolArea);
        }

        #endregion

        /*   private void DrawFireOrtho()
           {
               ScreenManager.GraphicsDevice.SetRenderTarget(0, null);
           
               float cameraX = (Map.mapX * MapManager.tileSize) + Map.dx + Map.mapWindowWidth / 2f;
               float cameraY = (Map.mapY * MapManager.tileSize) + Map.dy + Map.mapWindowHeight / 2f;
               Matrix view = Matrix.CreateLookAt(new Vector3(cameraX, cameraY, -1000),
                                                 new Vector3(cameraX, cameraY, 0),
                                                 Vector3.Down);
               // 1.4f*

               Matrix projection = Matrix.CreateOrthographic(ScreenManager.GraphicsDevice.Viewport.Width, ScreenManager.GraphicsDevice.Viewport.Height, -10000f, 10000f);

               Fire.Camera.myView = view;
               Fire.Camera.myProjection = projection;
               Fire.Camera.myPosition = new Vector3(cameraX, cameraY, -1000);

           }*/

        /* private void PlaceFire()
         {
             Fire.FireEmitterBase fire = new FireEmitterBase(ScreenManager.Game, 1, Color.White); //new FireEmitterBase(this, 100, Color.White);
             fire.particleScale = 1.5f;
             //    fire.Translate(new Vector3(0, 0, -20));

             //     fire.Rotate( Vector3.UnitY, MathHelper.PiOver2);

             fire.myScale = new Vector3(20f, 20f, 20f); // new Vector3(10f, 10f, 10f);
            
             // We don't want game components, the cleanup is a hassle...
             ScreenManager.Game.Components.Add(fire);
         }*/

        /*  private void DrawFire()
          {
              //    ScreenManager.GraphicsDevice.SetRenderTarget(0, null);

           
              float cameraX = (Map.mapX * MapManager.tileSize) + Map.dx + Map.mapWindowWidth / 2f;
              float cameraY = (Map.mapY * MapManager.tileSize) + Map.dy + Map.mapWindowHeight / 2f;
              Matrix view = Matrix.CreateLookAt(new Vector3(cameraX, cameraY, -1000),
                                                    new Vector3(cameraX, cameraY, 0),
                                                    Vector3.Down);
              Fire.Camera.myView = view;

              Vector3 myPosition = new Vector3(0, 15, 25);//new Vector3(0, 5, 25);
              Quaternion myRotation = new Quaternion(0, 0, 0, 1);
              Matrix myView = Matrix.Invert(Matrix.CreateFromQuaternion(myRotation) *
                                     Matrix.CreateTranslation(myPosition));


              // 1.4f*
              Viewport myViewport = ScreenManager.GraphicsDevice.Viewport;
              float aspectRatio = (float)myViewport.Width / (float)myViewport.Height;


              //  Matrix projection = Matrix.CreatePerspectiveFieldOfView(1, aspectRatio, 1, 1000);//myViewport.MinDepth, myViewport.MaxDepth);
              Matrix projection = Matrix.CreateOrthographic(ScreenManager.GraphicsDevice.Viewport.Width, ScreenManager.GraphicsDevice.Viewport.Height, -10000f, 10000f);

              //  Fire.Camera.myView = myView;//view;
              Fire.Camera.myProjection = projection;
              //  Fire.Camera.myPosition = new Vector3(cameraX, cameraY, -1000);

          }*/


        /// <summary>
        /// a modal dialog should ALWAYS pause the game. And the player should not be able to unpause while modal.
        /// </summary>
        /// <param name="value"></param>
        public void SetModal(bool value)
        {
            if (value == true)
            {
                IsModal = true;
                // always show the cursor (it might be hidden because the user was dragging a slider)
                Controller.Game.IsMouseVisible = true;
                PauseGame();
            }
            else
            {
                IsModal = false;
                ResumeGame();
            }
        }
         

        public virtual void SetRenderableOnMemoryFact(MemoryFact memoryFact, Entity entity, Allegiance allegiance)
        {
            if (allegiance == The.InGameUI.UIAllegiance
                && entity.Renderable != null) // othersite entities have no renderable
            {
                memoryFact.Renderable = RenderableFactory.Produce(entity.Renderable, memoryFact); 
            }
        }


        public virtual void AddTalk(string line, EntityID speaker, Conversation conversation)
        {
            Log.AddTalk(line, speaker, conversation);           

        }


        public virtual void SpeakLine(Entity entity, float durationInSeconds, Conversation conversation, string personalityInfusedLine)
        {
            IKnownEntityData data;
            if (The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(entity.EntityID, out data) == EntityResult.SeenDirectly)
            {
                AddTalk(personalityInfusedLine, entity.EntityID, conversation);

               /* SpokenLine = personalityInfusedLine;
                spokenLineDuration = durationInSeconds;  // we need to keep track of elapsed time for when we want to signal the conversation that is has ended...
                spokenLineElapsedTime = 0f;

                // save this so we won't interrupt ourselves:
                CurrentConversationID = conversation.ID;*/

                The.InGameUI.ShowSpokenLine(entity, personalityInfusedLine, durationInSeconds); // 4f);
            }
        }
       
    }//class client

}//namespace
