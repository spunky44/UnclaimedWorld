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
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using UWGame.Control;
using InputEventSystem;

//using UWGame.SimSide;
#endregion

namespace UWGame.ClientSide.MainMenu.Intro
{
   
    class IntroVideoScreen : GameScreen
    {
        Video video;
        VideoPlayer player;
        Texture2D videoTexture;

        SpriteBatch spriteBatch;

        /// <summary>
        /// load the movie into its own content manager
        /// </summary>
        ContentManager content;

        InputData frameInput;

        #region Initialization
        
        /// <summary>
        /// Constructor fills in the menu contents.
        /// </summary>
        public IntroVideoScreen(Controller screenManager)
        {
            frameInput = screenManager.InputData;
        }


        #endregion

        public override void LoadContent() //bool loadAllContent)
        {          
            base.LoadContent();

            content = new ContentManager(Controller.Game.Services);
            content.RootDirectory = "Content";

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(Controller.GraphicsDevice);
           // video = content.Load<Video>("MainMenu/TauCetiStartGame");
            video = content.Load<Video>("MainMenu/TauCetiMainMenu");
            player = new VideoPlayer();

            //intf.StartMovie();
        }
       
        public override void Draw(GameTime gameTime)
        {
            Controller.GraphicsDevice.Clear(Color.Black);

            // Only call GetTexture if a video is playing or paused
            if (player.State != MediaState.Stopped)
                videoTexture = player.GetTexture();

            // Drawing to the rectangle will stretch the 
            // video to fill the screen
            Rectangle screen = new Rectangle(Controller.GraphicsDevice.Viewport.X,
                Controller.GraphicsDevice.Viewport.Y,
                1280, //ScreenManager.GraphicsDevice.Viewport.Width,
                720); //ScreenManager.GraphicsDevice.Viewport.Height);

            // Draw the video, if we have a texture to draw.
            if (videoTexture != null)
            {
                spriteBatch.Begin();
                spriteBatch.Draw(videoTexture, screen, Color.White);
                spriteBatch.End();
            }

            //intf.Draw(gameTime);

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

                if (player.State == MediaState.Stopped)
                {
                    player.IsLooped = true;
                    player.Play(video);
                }


            }

        }

        public override void UnloadContent()
        {
            // unload the movie:
            content.Unload();

            base.UnloadContent(); 

           
        }

       
    }
}
