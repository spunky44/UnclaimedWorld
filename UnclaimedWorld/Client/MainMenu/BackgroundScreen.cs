#region File Description
//-----------------------------------------------------------------------------
// BackgroundScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using UWGame.SimSide;
 
#endregion

namespace UWGame.Control
{
    /// <summary>
    /// The background screen sits behind all the other menu screens.
    /// It draws a background image that remains fixed in place regardless
    /// of whatever transitions the screens on top of it may be doing.
    /// </summary>
    class BackgroundScreen : GameScreen
    {
        #region Fields

        Video video;
        //VideoPlayer player;
        /// <summary>
        /// had to make this static beccause of MG bug: Cannot use a second instance of VideoPlayer
        /// </summary>
        static VideoPlayer player = new VideoPlayer();

        Texture2D videoTexture;


        /// <summary>
        /// load the movie into its own content manager
        /// TODO: Perhaps share this among main menu screens, to avoid having to load the video every time we go to another menu screen?
        /// 
        /// share this, because unloading videos causes a memory leak.
        /// </summary>
        ContentManager content;

        Texture2D backgroundTexture; 

        Rectangle backgroundDest, titleDest;

        #endregion

        #region Initialization

        public enum Background { Normal, BlueTint}

        private Background backgroundType;

        /// <summary>
        /// Constructor.
        /// </summary>
        public BackgroundScreen(Background background)
        {
            this.backgroundType = background;

            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            
        }

        Regulator videoRestartRegulator;

        /// <summary>
        /// Loads graphics content for this screen. The background texture is quite
        /// big, so we use our own local ContentManager to load it. This allows us
        /// to unload before going from the menus into the game itself, wheras if we
        /// used the shared ContentManager provided by the ScreenManager, the content
        /// would remain loaded forever.
        /// </summary>               
        public override void LoadContent() 
        {

            if (content == null)
            {
                content = new ContentManager(Controller.Game.Services);
                content.RootDirectory = "Content";
            }

            if (Controller.Options.PlayVideo)
            {
                //video = content.Load<Video>("MainMenu/TauCetiMainMenu");
                video = Controller.Content.Load<Video>("MainMenu/TauCetiMainMenu"); // use the permanent content manager to load video because unloading video leaks memory.

              //  player = new VideoPlayer();

              //  player.IsLooped = true; #MONOUPDATE

                player.Play(video); // NEW

                /*
                player.Stop();
                player.Dispose();

                player = new VideoPlayer();
                player.Play(video); // crashes
                */


                videoRestartRegulator = new Regulator(null, 1d / video.Duration.TotalSeconds, "BacgroundScreen");
            }
            else
            {
                // load a static image:
                backgroundTexture = content.Load<Texture2D>("MainMenu/TauCetiMainMenuBGOnly_1280px");
            }

         /*   if (backgroundType == Background.Normal)
            {
                backgroundTexture = content.Load<Texture2D>("Content/MainMenu/panorama_1920px"); //"Content/MainMenu/background");
            }
            else
            {
                backgroundTexture = content.Load<Texture2D>("Content/MainMenu/panorama_blue_1920px");
                // backgroundTextureBlue = content.Load<Texture2D>("Content/MainMenu/panorama_blue_1920px");
            }*/

          //  titleTexture = content.Load<Texture2D>("Content/MainMenu/title-text"); //I have merged the title logo unto the title screen image, because I didn´t know how to move the title into place. title-text now consists of a blank png -Morten
            //  }

            InitSize();
        }


        /// <summary>
        /// Unloads graphics content for this screen.
        /// </summary>
        public override void UnloadContent() //bool unloadAllContent)
        {
            //  video.Dispose();
            //   player.Dispose();
            if (Controller.Options.PlayVideo)
            {
                player.Stop();
               // player.Dispose(); // NEW - #MONOUPDATE
            }

             content.Unload();
        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Updates the background screen. Unlike most screens, this should not
        /// transition off even if it has been covered by another screen: it is
        /// supposed to be covered, after all! This overload forces the
        /// coveredByOtherScreen parameter to false in order to stop the base
        /// Update method wanting to transition off.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, false);

            if (Controller.Options.PlayVideo &&
                   player.State == MediaState.Stopped)
            {
              //  player.IsLooped = true; #MONOUPDATE
                player.Play(video);
            }


            /*
            // if the video has ended, start it again (no looping implemented)
            double lastReady = 0;
            if (player != null && videoRestartRegulator.IsReady(gameTime, ref lastReady))
            {
                try
                {
                    //player.Stop(); // throws this: http://community.monogame.net/t/videoplayer-stop-gives-object-not-set-to-an-instance-exception/7993
                
                    player.Play(video); // sometimes times out and throws: http://community.monogame.net/t/cannot-start-video-intermittent-crash/7991
                }
                catch(InvalidOperationException e)
                {

                }
            }
            */


          /*  if (Controller.Options.PlayVideo &&
                player.State == MediaState.Stopped)
            {
                //player.IsLooped = true; // #MONOCHANGE - not implemented? http://community.monogame.net/t/how-to-loop-video-using-videoplayer-note-islooped-is-not-implemented/1437
                //player.Play(video);
            }*/
          
        }


        private void InitSize()
        {
            Dimension /* Point*/ screenSize = Controller.DrawArea; // .Game.GetScreenResolution();

      

            int backgroundImageWidth = 1280; // backgroundTexture.Width;
            int backgroundImageHeight = 720; // backgroundTexture.Height;

            float scaleFactor = (float)screenSize.Height / (float)backgroundImageHeight;
            // scale up as well??? factor > 1f

            // scale height, and crop sides:
            int height = screenSize.Height; // .Y;
            int width = (int)(scaleFactor * backgroundImageWidth); //(int)(((float)height) * (((float)screenSize.X) / ((float)screenSize.Y)));

            backgroundDest = new Rectangle((screenSize.Width - width) / 2, (screenSize.Height - height) / 2, width, height);

        }

        // OLD: images
     /*   private void InitBackgroundImages()
        {
            Point screenSize = ScreenManager.Game.GetScreenResolution();

                 int titleX = (screenSize.X - titleTexture.Width) / 8;
                 int titleY = (screenSize.Y - titleTexture.Height) / 3;
            
                 titleDest = new Rectangle(titleX, titleY, titleTexture.Width, titleTexture.Height);

            int backgroundImageWidth = backgroundTexture.Width;
            int backgroundImageHeight = backgroundTexture.Height;

            float scaleFactor = (float)screenSize.Y / (float)backgroundImageHeight;
            // scale up as well??? factor > 1f

            // scale height, and crop sides:
            int height = screenSize.Y;
            int width = (int)(scaleFactor * backgroundImageWidth); //(int)(((float)height) * (((float)screenSize.X) / ((float)screenSize.Y)));

            backgroundDest = new Rectangle((screenSize.X - width) / 2, (screenSize.Y - height) / 2, width, height);

        }*/


        /// <summary>
        /// Draws the background screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {            
            byte fade = TransitionAlpha;


            Controller.GraphicsDevice.Clear(Color.Black);
         //   ScreenManager.GraphicsDevice.Clear(Color.Red);

            // Only call GetTexture if a video is playing or paused
            if (Controller.Options.PlayVideo 
                && player != null && player.State != MediaState.Stopped
                && player.Video != null) // Monogame: Video can sometimes be null?
            {
                videoTexture = player.GetTexture();
            }

            // Drawing to the rectangle will stretch the 
            // video to fill the screen
         /*   Rectangle screen = new Rectangle(ScreenManager.GraphicsDevice.Viewport.X,
                ScreenManager.GraphicsDevice.Viewport.Y,
                1280, 
                720); 
            */

            // Draw the video, if we have a texture to draw.
            if (videoTexture != null)
            {
                Controller.SpriteBatch.Begin();
                Controller.SpriteBatch.Draw(videoTexture, backgroundDest, new Color(fade, fade, fade));
                Controller.SpriteBatch.End();
            }

            if (backgroundTexture != null)
            {
                Controller.SpriteBatch.Begin(0, BlendState.AlphaBlend);

                Controller.SpriteBatch.Draw(backgroundTexture, backgroundDest,
                                               new Color(fade, fade, fade));

                Controller.SpriteBatch.End();

            }
           
        /*    ScreenManager.SpriteBatch.Begin(0, BlendState.AlphaBlend);

            ScreenManager.SpriteBatch.Draw(backgroundTexture, backgroundDest,
                                           new Color(fade, fade, fade));
            */
           // ScreenManager.SpriteBatch.End();
        }


        #endregion
    }
}
