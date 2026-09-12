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
#endregion

namespace UWGame.Client.MainMenu.LoadReplay
{
    class LoadReplayScreen : GameScreen
    {
        LoadReplayInterface intf;

        InputData frameInput;
        #region Initialization

        public LoadReplayScreen(Controller screenManager)
        {
            intf = new LoadReplayInterface(this, screenManager.Game);

            frameInput = screenManager.InputData;
        }
        #endregion

        public override void Draw(GameTime gameTime)
        {
            intf.Draw(gameTime);

        }

        public override void HandleInput(/*InputState input*/)
        {
            if (frameInput.IsKeyDown(Keys.Escape))
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

        internal void LoadReplay(string path, float? timeToPauseReplay)
        {
            base.Controller.LoadReplay(
                path,
                timeToPauseReplay);
        }
    }
}
