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
 
using GameStateManagement;
using UWGame.Control;

#endregion

namespace UWGame.ClientSide.Screens
{
   
    public class IngameLoadGameScreen : GameScreen
    {
        UnclaimedWorld game;

        public IngameLoadGameInterface Interface;

       // string text;

        #region Initialization



        public IngameLoadGameScreen(UnclaimedWorld game) //Controller screenManager, string text)
        {
            this.game = game;

            if (game.GraphicsDeviceManager.GraphicsDevice != null)
            {
                Interface = new IngameLoadGameInterface(this, game);
            }

           // this.text = text;
          
        }

        


        #endregion

        public override void Draw(GameTime gameTime)
        {
            Interface.Draw(gameTime);

        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                      bool coveredByOtherScreen)
        {

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            if (IsActive)
            {
                //this.GameTime = gameTime;                



                //  mouseState = Mouse.GetState();
                Interface.Update(gameTime);


            }

        }

        
        public override void LoadContent() //bool loadAllContent)
        {
            base.LoadContent(); //loadAllContent);

            if (Interface == null)
            {
                Interface = new IngameLoadGameInterface(this, game);
            }

            Interface.LoadContent();

           // intf.Panel.Text = text;

        }




        public override void UnloadContent() //bool unloadAllContent)
        {
            base.UnloadContent(); //unloadAllContent);

            Interface.UnloadContent();

        }

        #region Handle Input


        

        public void DummyEventHandler(object caller, EventArgs e)
        {
            //do nothing
        }




        /// <summary>
        /// Event handler for when the user selects ok on the "are you sure
        /// you want to exit" message box.
        /// </summary>
        void ExitMessageBoxAccepted(object sender, EventArgs e)
        {
            Controller.Game.Exit();
        }


       

        void ShowMapEditorScreen(object sender, EventArgs e)
        {
            Controller.AddScreen(new BackgroundScreen(BackgroundScreen.Background.BlueTint));
            LoadMapScreen screen = new LoadMapScreen(Controller, UWGame.SimSide.Sim.EngineMode.Edit);
            Controller.AddScreen(screen);

            //ScreenManager.AddScreen(new GameplayScreen());
        }

        void ShowTestGameScreen(object sender, EventArgs e)
        {
            Controller.AddScreen(new BackgroundScreen(BackgroundScreen.Background.BlueTint));
            LoadMapScreen screen = new LoadMapScreen(Controller, UWGame.SimSide.Sim.EngineMode.Game, true);
            Controller.AddScreen(screen);

            //ScreenManager.AddScreen(new GameplayScreen());
        }

        #endregion
    }
}
