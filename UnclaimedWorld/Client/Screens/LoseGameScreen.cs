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
   
    public class LoseGameScreen : GameScreen
    {
       
        LoseGameInterface intf;

        string text;

        #region Initialization


        
        public LoseGameScreen(Controller screenManager, string text)
        {
            intf = new LoseGameInterface(this, screenManager.Game);


            this.text = text;
          
        }

        


        #endregion

        public override void Draw(GameTime gameTime)
        {
            intf.Draw(gameTime);

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


        public void ExitToMainMenu()
        { 
            ExitScreen(); // important to call this first!

            //   ScreenManager.AddScreen(new BackgroundScreen());
            Controller.AddScreen(new MainMenuScreen(Controller.Game));

           
        }

        public override void LoadContent() //bool loadAllContent)
        {
            base.LoadContent(); //loadAllContent);


            intf.LoadContent();

            intf.Panel.Text = text;

        }




        public override void UnloadContent() //bool unloadAllContent)
        {
            base.UnloadContent(); //unloadAllContent);

            intf.UnloadContent();

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
