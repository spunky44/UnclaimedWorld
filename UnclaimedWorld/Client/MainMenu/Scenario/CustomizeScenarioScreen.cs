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
using UWGame.SimSide.XmlCollections;
#endregion

namespace UWGame.ClientSide.MainMenu.Scenario
{
    public class CustomizeScenarioScreen : GameScreen
    {
        CustomizeScenarioInterface intf;

        public SimSide.Scenarios.Scenario Scenario;

        InputData frameInput;
        #region Initialization

        public CustomizeScenarioScreen(Controller screenManager, SimSide.Scenarios.Scenario scenario)
        {
            intf = new CustomizeScenarioInterface(this, screenManager.Game);
            this.Scenario = scenario;

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


        public void StartGame(SerializableDictionary<string, Option> options, Difficulty mainDifficulty)
        {
            // Play the game.       

            string difficultyKey = null;
            if (mainDifficulty != null)
            {
                difficultyKey = mainDifficulty.KeyName;
            }

            LoadingScreen.StartTransitioningToGame(Controller, 
                new StartGameParams(){ StartScenarioParams = new StartScenarioParams(){ 
                    Scenario = this.Scenario, 
                    Options = options,
                    MainDifficultyKey = difficultyKey, // used in some achievements
                    Source = this.Scenario.Source, ScenarioName = this.Scenario.Name // these are needed to load replays for instance
                }}, 
                true, true);

            //The.LoadScreen.QueueActive = true;
        }
    }
}
