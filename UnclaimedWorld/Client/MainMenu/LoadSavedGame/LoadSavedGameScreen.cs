#region using statements
using Microsoft.Xna.Framework;
using GameStateManagement;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Items;
using UWGame.SimSide.Overland;
using UWGame.SimSide;
 
using UWGame.ClientSide.Screens;
using UWGame.Control;
using InputEventSystem;
using UWGame.SimSide.Scenarios;
using System.IO;
using UWGame.SimSide.Snapshots;
using System.IO.Compression;
#endregion

namespace UWGame.Client.MainMenu.LoadSavedGame
{
    /// <summary>
    /// we get to this screen from Load on the Main Menu - it uses LoadGameInterface to create an instance of SaveAndLoadPanel, which is alos accessible in-game
    /// </summary>
    class LoadSavedGameScreen : GameScreen
    {
        LoadSavedGameInterface intf;

        InputData inputData;

        #region Initialization

        public LoadSavedGameScreen(Controller screenManager)
        {
            intf = new LoadSavedGameInterface(this, screenManager.Game);

            inputData = screenManager.InputData;
        }
        #endregion

        public override void Draw(GameTime gameTime)
        {
            intf.Draw(gameTime);

        }

        public override void HandleInput(/*InputState input*/)
        {
            if (inputData.IsKeyDown(Keys.Escape))
            {
                ExitToMainMenu();
            }
        }

        public override void Destroy()
        {
            intf.Destroy();

            intf = null;
        }

        public void ExitToMainMenu()
        {
            ExitScreen();

            Controller.AddScreen(new MainMenuScreen(Controller.Game));
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            if (IsActive)
            {           
                intf.Update(gameTime);
            }
        }

        public override void LoadContent()
        {
            base.LoadContent();
            
            intf.LoadContent();
        }
       
        public override void UnloadContent()
        {
            base.UnloadContent();

            intf.UnloadContent();

        }

        public static void LoadSavedGameWithLoadingScreen(Controller controller, string savegamePath, SnapshotHeader header)
        {
            header.StartGameParams.SavedGameToLoad = savegamePath;


            if (header.StartGameParams.StartScenarioParams != null)
            {
                header.StartGameParams.StartScenarioParams.LoadScenarioFromName(); // now read the scenario data - this is needed by the loading screen to show the background image, and later on load the game data:                
            }

            

            LoadingScreen.StartTransitioningToGame(controller,
                header.StartGameParams,
                true, true);
        }


        public void LoadSavedGameWithLoadingScreen(string savegamePath)
        {
            SnapshotHeader header;

            // read in the save header with the name of the scenario etc.

            FileStream stream = File.Open(savegamePath, FileMode.Open);
            GZipStream cmp = new GZipStream(stream, CompressionMode.Decompress);
            BufferedStream buffStrm = new BufferedStream(cmp, 65536);

            using (BinaryReader reader = new BinaryReader(buffStrm))
            {
                header = The.Snapshotter.LoadHeader(reader);
            }

            LoadSavedGameWithLoadingScreen(Controller, savegamePath, header);

        }
    }
}
