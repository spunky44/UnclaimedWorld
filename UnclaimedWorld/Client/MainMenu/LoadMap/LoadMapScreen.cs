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
#endregion

namespace UWGame.ClientSide.MainMenu.LoadMap
{
    
    /// <summary>
    /// we get to this screen from Edit map and Test map on the Main Menu
    /// </summary>
    public class LoadMapScreen : GameScreen
    {
        LoadMapInterface intf;

        SimSide.Sim.EngineMode gameMode;
        bool isTestingGame;
       // string mapToLoad;

        InputData frameInput;
        #region Initialization



        public LoadMapScreen(Controller screenManager, SimSide.Sim.EngineMode gameMode, bool isTestingGame = false)
        {

            intf = new LoadMapInterface(this, screenManager.Game);

            this.gameMode = gameMode;
            this.isTestingGame = isTestingGame;

            //Interface.Interface.Instance = intf;

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

        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
        public override void HandleInput(/*InputState input*/)
        {
            /*if (input == null)
                throw new ArgumentNullException("input");*/

            /*    if (input.PauseGame)
                {
                    // If they pressed pause, bring up the pause menu screen.
                    ScreenManager.AddScreen(new PauseMenuScreen());
                }
                else
                {
                */




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


        public void StartMapEditor(DirectoryInfo mapToLoad) // string mapToLoad)
        {
         
            Random seedRandomizer = new Random();
            int randomSeed = seedRandomizer.Next();
                     

            LoadingScreen.StartTransitioningToGame(Controller,
                new StartGameParams() { StartGameEditorParams = new StartGameEditorParams() { MapToLoadPath = mapToLoad.FullName, EngineMode = gameMode } },
                true);

        }

       

        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            if (IsActive)
            {
               
                //  mouseState = Mouse.GetState();
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

        
    }
}
