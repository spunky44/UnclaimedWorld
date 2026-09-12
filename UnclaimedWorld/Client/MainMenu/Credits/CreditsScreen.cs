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
using UWGame.Control;
using InputEventSystem;
 
#endregion

namespace UWGame.ClientSide.MainMenu.Credits
{
    
   
    public class CreditsScreen : GameScreen
    {
        CreditsInterface intf;

        InputData frameInput;
        #region Initialization



        public CreditsScreen(Controller screenManager)
        {

            intf = new CreditsInterface(this, screenManager.Game);

            frameInput = screenManager.InputData;

            //Interface.Interface.Instance = intf;

            
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

            //   ScreenManager.AddScreen(new BackgroundScreen());
            Controller.AddScreen(new MainMenuScreen(Controller.Game));

          //  ExitScreen();
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

        


        public override void LoadContent() //bool loadAllContent)
        {
            base.LoadContent(); //loadAllContent);

            
            intf.LoadContent();
        }
       



        public override void UnloadContent() //bool unloadAllContent)
        {
            base.UnloadContent(); //unloadAllContent);

            intf.UnloadContent();

        }

        
    }
}
