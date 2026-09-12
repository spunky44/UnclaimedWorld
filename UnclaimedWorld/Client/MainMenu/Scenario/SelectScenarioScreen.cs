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
using UWGame.SimSide.AllGameData.Scenarios;
#endregion

namespace UWGame.ClientSide.MainMenu.Scenario
{
    public class SelectScenarioScreen : GameScreen
    {
        SelectScenarioInterface intf;

        InputData frameInput;
        #region Initialization

        public SelectScenarioScreen(Controller screenManager)
        {
            intf = new SelectScenarioInterface(this, screenManager.Game);

            frameInput = screenManager.InputData;
        }
        #endregion

        public override void Draw(GameTime gameTime)
        {
            intf.Draw(gameTime);

        }

        public override void Destroy()
        {
            intf.Destroy();

            intf = null;
        }

        public override void HandleInput(/*InputState input*/)
        {
            if (frameInput.IsKeyDown(Keys.Escape))
            {
                ExitToMainMenu();
            }
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


        public void SelectScenario()
        {
            ExitScreen();
            Controller.AddScreen(new UWGame.ClientSide.MainMenu.Scenario.SelectScenarioScreen(Controller));

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

        public void SelectScenario(SimSide.Scenarios.Scenario scenario)
        {
            // load the data:
            scenario.ScenarioData = AllScenarioLoader.LoadScenarioData(scenario);

            ExitScreen();
            Controller.AddScreen(new UWGame.ClientSide.MainMenu.Scenario.CustomizeScenarioScreen(Controller, scenario));

        }
    }
}
