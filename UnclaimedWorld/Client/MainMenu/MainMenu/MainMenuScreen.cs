#region File Description
//-----------------------------------------------------------------------------
// MainMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.MainMenu.LoadMap;
using UWGame;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using UWGame.ClientSide;
using Microsoft.Xna.Framework.Input;
using UWGame.Client.Audio;
using UWGame.ClientSide.Screens;
using UWGame.Control;
using UWGame.Client.MainMenu.LoadReplay;
using UWGame.SimSide;
using UWGame.SimSide.Scenarios;
using UWGame.Client.MainMenu.LoadSavedGame;
 

#endregion

namespace GameStateManagement
{
    /// <summary>
    /// The main menu screen is the first thing displayed when the game starts up.
    /// </summary>
    public class MainMenuScreen: GameScreen //: MenuScreen
    {
        MainMenuInterface intf;

        UnclaimedWorld game;

        Song titleSong;

        #region Initialization


        /// <summary>
        /// Constructor fills in the menu contents.
        /// </summary>
        public MainMenuScreen(UnclaimedWorld game)
        {
            this.game = game;

            if (game.GraphicsDeviceManager.GraphicsDevice != null)
            {
                intf = new MainMenuInterface(this, game); // screenManager.Game);
            }

        }

        protected override float getStringEnlargementFactor()
        {
            return 1.4f;
        }



        #endregion

        public override void Draw(GameTime gameTime)
        { 
            intf.Draw(gameTime);

        }

        public override void LoadContent()
        {
            base.LoadContent();

            titleSong = Controller.Content.Load<Song>("Music\\Jesper Lundager - Prosperous Frontier_320"); // "Music\\Martin Hasseldam - Settle");

            //restart the song if it is playing:
            Controller.AudioManager.StopSong();
            Controller.AudioManager.PlaySong(titleSong, true);
            Controller.AudioManager.Resume();

            
            if (intf != null)
            {
                intf.LoadContent();
            }
        }

        public override void UnloadContent()
        {
            base.UnloadContent();

            intf.UnloadContent();
        }

        public override void Destroy()
        {
            intf.Destroy();

            intf = null;
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
           
            if (intf == null)
            {
                intf = new MainMenuInterface(this, game);

                intf.LoadContent();
            }

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            if (IsActive)
            {
                //this.GameTime = gameTime;                



                //  mouseState = Mouse.GetState();
                intf.Update(gameTime);


            }
        }

     

        public void Exit()
        {
            Controller.Game.Exit();
        }

        public void ShowIntro()
        {
            ExitScreen();
            Controller.AddScreen(new UWGame.ClientSide.MainMenu.Intro.IntroVideoScreen(Controller));

        }

        public void ShowCredits()
        {
            ExitScreen();
            Controller.AddScreen(new UWGame.ClientSide.MainMenu.Credits.CreditsScreen(Controller));

        }

        public void ShowScenarios()
        {
            ExitScreen();
            Controller.AddScreen(new UWGame.ClientSide.MainMenu.Scenario.SelectScenarioScreen(Controller));

        }

        

        public void TestMap()
        {
            LoadingScreen.StartTransition(Controller, ShowTestGameScreen, false);
        }

        public void EditMap()
        {
            LoadingScreen.StartTransition(Controller, ShowMapEditorScreen, false);
        }

        public void LoadReplay()
        {
            LoadingScreen.StartTransition(Controller, ShowLoadReplayScreen, false);
        }

        public void LoadGame()
        {
            LoadingScreen.StartTransition(Controller, ShowLoadGameScreen, false);
        }

       /* public void CreateGame()
        {
            LoadingScreen.Load(ScreenManager, ShowCreateGameScreen, true); 
        }*/

        public void StartTest()
        {
            // Play the game.

            LoadingScreen.StartTransitioningToGame(Controller,
               // new StartGameParams() { StartDebugScenarioParams =  new StartDebugScenarioParams() { ScenarioKey = PlaceGameEntities.GetDefaultScenario() } },
                new StartGameParams() { StartDebugScenarioParams = PlaceGameEntities.GetNewScenarioParams()  },
                true, true);

            //The.LoadScreen.QueueActive = true;
        }

        public void DummyEventHandler(object caller, EventArgs e)
        {
            //do nothing
        }



        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
        public override void HandleInput(/*InputState input*/)
        {
            /*if (input == null)
                throw new ArgumentNullException("input");*/
         
            /*
            if (keyboard.IsKeyTapped(Keys.Escape))
            {
                const string message = "Are you sure you want to exit the game?";

                MessageBoxScreen messageBox = new MessageBoxScreen(message);

                messageBox.Accepted += ExitMessageBoxAccepted;

                ScreenManager.AddScreen(messageBox);
            }
            */
        }


        /// <summary>
        /// When the user cancels the main menu, ask if they want to exit the sample.
        /// </summary>
    /*    protected override void OnCancel()
        {
            const string message = "Are you sure you want to exit this sample?";

            MessageBoxScreen messageBox = new MessageBoxScreen(message);

            messageBox.Accepted += ExitMessageBoxAccepted;

            ScreenManager.AddScreen(messageBox);
        }*/


        /// <summary>
        /// Event handler for when the user selects ok on the "are you sure
        /// you want to exit" message box.
        /// </summary>
        void ExitMessageBoxAccepted(object sender, EventArgs e)
        {
            Controller.Game.Exit();
        }



        


        //public void InitSimAndClient()
        //{

        //    //The Sim and the Client are decoupled, we need one of each
        //    //It should eventaully be possible for the sim to run in the
        //    //complete absence of a client (running headless)
        //    //or in the presence of any variation, subclass or state
        //    //of client... the client has no effect whatsoever on the sim
        //    //some day -- MLo

        //    bool headless = false;

        //    The.Sim = new UWGame.SimSide.Sim(ScreenManager, "", "", UWGame.SimSide.Sim.EngineMode.Game);

        //    if ( ! headless)
        //    {
        //        The.Client = new UWGame.ClientSide.Client(ScreenManager);
        //    }

        //    // add sim and client to the screen manager
        //    if (The.Sim != null)
        //    {
        //        ScreenManager.AddScreen(The.Sim);
        //    }
        //    if (The.Client != null)
        //    {
        //        ScreenManager.AddScreen(The.Client);
        //    }

        //    //Post-proceess both client and sim
        //    if (The.Sim != null)
        //    {
        //        The.Sim.Init();
        //    }
        //    if (The.Client != null)
        //    {
        //        The.Client.Init();
        //    }

        //    // begin run for sim and client 
        //    if (The.Sim != null)
        //    {
        //        The.Sim.BeginRun();
        //    }
        //    if (The.Client != null)
        //    {
        //        The.Client.BeginRun();
        //    }
            


             
        //}

        /// <summary>
        /// Loading screen callback for activating the gameplay screen.
        /// </summary>
      /*  void ShowCreateGameScreen(object sender, EventArgs e)
        {
            ScreenManager.AddScreen(new BackgroundScreen(BackgroundScreen.Background.BlueTint));
            CreateGameScreen create = new CreateGameScreen(ScreenManager);
            ScreenManager.AddScreen(create);

            //ScreenManager.AddScreen(new GameplayScreen());
        }*/

        void ShowMapEditorScreen(object sender, EventArgs e)
        {
            Controller.AddScreen(new BackgroundScreen(BackgroundScreen.Background.BlueTint));
            LoadMapScreen screen = new LoadMapScreen(Controller, UWGame.SimSide.Sim.EngineMode.Edit);
            Controller.AddScreen(screen);

            //ScreenManager.AddScreen(new GameplayScreen());
        }

        void ShowLoadReplayScreen(object sender, EventArgs e)
        {
            Controller.AddScreen(new BackgroundScreen(BackgroundScreen.Background.BlueTint));
            LoadReplayScreen screen = new LoadReplayScreen(Controller);
            Controller.AddScreen(screen);
        }

        void ShowLoadGameScreen(object sender, EventArgs e)
        {
            Controller.AddScreen(new BackgroundScreen(BackgroundScreen.Background.BlueTint));
            LoadSavedGameScreen screen = new LoadSavedGameScreen(Controller);
            Controller.AddScreen(screen);
        }

        void ShowTestGameScreen(object sender, EventArgs e)
        {
            Controller.AddScreen(new BackgroundScreen(BackgroundScreen.Background.BlueTint));
            LoadMapScreen screen = new LoadMapScreen(Controller, UWGame.SimSide.Sim.EngineMode.Game, true);
            Controller.AddScreen(screen);

            //ScreenManager.AddScreen(new GameplayScreen());
        }

       
    }
}
