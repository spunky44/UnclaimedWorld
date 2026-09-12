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
using WindowSystem;
using UWGame.Control;
using InputEventSystem;

//using UWGame.SimSide;
#endregion

namespace UWGame.ClientSide.MainMenu.Intro
{
    /// <summary>
    /// Not used...
    /// </summary>
    class IntroScreen : GameScreen
    {
        IntroInterface intf;

        InputData frameInput;
        #region Initialization


        /// <summary>
        /// Constructor fills in the menu contents.
        /// </summary>
        public IntroScreen(Controller screenManager)
        {
            intf = new IntroInterface(screenManager.Game);

           // intf.framedCRT.AnimationMode = CRTTextAnimator.AnimationMode.Line;
            frameInput = screenManager.InputData;
        }


        #endregion

        public override void LoadContent() //bool loadAllContent)
        {
            intf.LoadContent();
            base.LoadContent(); //loadAllContent);


            // **** START!
            intf.framedCRT.Show();
            intf.framedCRT.TurnOn();

            intf.StartMovie();
        }
       
        public override void Draw(GameTime gameTime)
        {
            intf.Draw(gameTime);

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
                ExitScreen();
               // ScreenManager.AddScreen(new BackgroundScreen());
                Controller.AddScreen(new MainMenuScreen(Controller.Game));
            }

        }


        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            if (IsActive)
            {
                //this.GameTime = gameTime;                



                //  mouseState = Mouse.GetState();
                intf.Update(gameTime);


            }

        }

        public override void UnloadContent() //bool unloadAllContent)
        {
            base.UnloadContent(); //unloadAllContent);
            intf.UnloadContent();
        }

        #region Handle Input

        /*
        /// <summary>
        /// Loading screen callback for activating the gameplay screen.
        /// </summary>
        void LoadGameplayScreen(object sender, EventArgs e)
        {
            UWGame.SimSide.UWGame.SimSide game = new UWGame.SimSide.UWGame.SimSide(ScreenManager);
            ScreenManager.AddScreen(game);

            //ScreenManager.AddScreen(new GameplayScreen());
        }
        */

        #endregion
    }
}
