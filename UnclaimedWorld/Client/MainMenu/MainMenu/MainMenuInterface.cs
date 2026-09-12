using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;
using InputEventSystem;
using UWGame.ClientSide.Interface;
using GameStateManagement;

namespace GameStateManagement
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using GameStateManagement;
    using Microsoft.Xna.Framework;
    using WindowSystem;
    using System.Reflection;
    using UWGame;
    using UWGame.ClientSide.Interface.HUD_Windows;
   
    /// <summary>
    /// Interface for the main menu
    /// </summary>
    public class MainMenuInterface : CommonInterface
    {
                
        int left, top;

        Tooltip Tooltip;

        MainMenuPanel panel;
        MainMenuDevPanel devPanel;

        private TimelinePanel timeline;

        public MainMenuScreen mainMenuScreen;

        public MainMenuInterface(MainMenuScreen mainMenuScreen, UnclaimedWorld game)
            : base(game)
        {

            this.mainMenuScreen = mainMenuScreen;
                       
           // left =  (game.GraphicsDeviceManager.PreferredBackBufferWidth) / 2 - 180;
            left = game.Controller.DrawArea.Width / 2 - 180;
           // top = (game.GraphicsDeviceManager.PreferredBackBufferHeight - 240);
            top = (game.Controller.DrawArea.Height - 240);

            Tooltip = new Tooltip(this);
        }

        public override void LoadContent()
        {
            base.LoadContent();

          //  DisplayPanelRenderer.LoadContent();

            // CRT must be created after crt effect textures (scanlines etc.) have loaded:
         //   Rectangle directSource = new Rectangle(entityPanelLeft, entityPanelTop, framedCRTWidth, framedCRTHeight);
            /*framedCRT = new FramedCRT(this, new Rectangle(entityPanelLeft, entityPanelTop, framedCRTWidth, framedCRTHeight),
                                                directSource, Level.Middle);
            framedCRT.ShowCables = false;*/

             

            panel = new MainMenuPanel(this, new Point(left, top));           
            panel.Show();
                        

#if DEBUG || PROFILE

            devPanel = new MainMenuDevPanel(this, new Point(panel.Window.Right + 48, top));
            devPanel.Show();
#endif
          
            Window titleWindow = new Window(gui);
            titleWindow.X = 100;
            titleWindow.Y = 100;
            Image titleLogo = new Image(gui);
            titleWindow.Add(titleLogo);
            titleLogo.SetSkinLocation(SkinState.Normal,gui.GUISpriteSheet.GetSourceRectangle("Main_logo"));
            titleLogo.ResizeControlToFitImage();
            titleWindow.Width = titleLogo.Width;
            titleWindow.Height = titleLogo.Height;
            titleWindow.Show();

          /*  TextArea taNoVideo = new TextArea(gui, ListBoxType.HUDAndLCD);           
            taNoVideo.Font = GUIManager.LCDandHUDFont;
            taNoVideo.Color = Color.Orange; 
            titleWindow.Add(taNoVideo); // add calls initialize, that sets font...
            taNoVideo.Y = titleLogo.Bottom; 
            taNoVideo.Width = 240;
            taNoVideo.Height = 60;
            taNoVideo.Text = "Note: Background animation on title screen is temporarily disabled in version 0.9.x.x";
            titleWindow.Height = taNoVideo.Bottom;
            */
           

            timeline = new TimelinePanel(this);
            int timeLineX = panel.Window.Right + 280;
            timeLineX = Common.ClampTop(timeLineX, gui.ScreenWidth - TimelinePanel.Width); // don't go past the right edge of the screen 
            timeline.DisplayWindow.X = timeLineX;
            timeline.DisplayWindow.Y = titleWindow.Y + 100;



            // logo bottom right:
            Window bottomWindow = new Window(gui);
            bottomWindow.IsMovable = false;

            Image nordenLogo = new Image(gui);
            bottomWindow.Add(nordenLogo);
            nordenLogo.SetSkinLocation(SkinState.Normal,gui.GUISpriteSheet.GetSourceRectangle("Norden"));
            nordenLogo.ResizeControlToFitImage();

            Label supportedBy = new Label(gui);
            bottomWindow.Add(supportedBy);
            supportedBy.Init(Label.LabelType.HUDWindow);
            supportedBy.Text = "© 2012-16 Refactored Games OÜ, all rights reserved. Supported by: ";
            supportedBy.FitToText();
           
            bottomWindow.Width = gui.ScreenWidth; // nordenLogo.Right;
            bottomWindow.Height = nordenLogo.Height;

            bottomWindow.X = 0; // gui.ScreenWidth - bottomWindow.Width - 12;
            bottomWindow.Y = gui.ScreenHeight - bottomWindow.Height - 12;

            nordenLogo.X = bottomWindow.Width - nordenLogo.Width - 12;
            supportedBy.X = nordenLogo.X - supportedBy.Width - 12;
            supportedBy.Y = bottomWindow.Height - supportedBy.Height - 4;

            Label version = new Label(gui);
            bottomWindow.Add(version);
            version.Init(Label.LabelType.HUDWindow);
            version.Text = UnclaimedWorld.GetVersionAsString();
            version.FitToText();
            bottomWindow.CenterChildHorizontally(version);
            version.Y = bottomWindow.Height - version.Height - 4;


            Label steamWarning = new Label(gui);
            bottomWindow.Add(steamWarning);
            steamWarning.Init(Label.LabelType.HUDWindow);
            steamWarning.NormalColor = UIComponent.errorColor;
            steamWarning.X = 40;
            steamWarning.Y = bottomWindow.Height - steamWarning.Height - 4;
            if (!mainMenuScreen.Controller.SteamManager.IsInitialized)
            {
                /*Warning - Steam has not initialized correctly: This means that if you qualify for any new Steam achievements during play, you will NOT receive those achievements at any time!*/
                steamWarning.Text = "Warning: Steam has not initialized correctly. Achievements cannot be unlocked in this session!";
                steamWarning.FitToText();
                steamWarning.Visible = true;
            }
            else
            {
                steamWarning.Visible = false;
            }



            bottomWindow.Show();




            SetInterfaceCursor();

          //  framedCRT.Show();

        }


        public void ShowSteamWarning()
        {


        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            Tooltip.Update(gameTime);

        }

      
        public override void Destroy()
        {
            base.Destroy();

            Tooltip.Destroy();
        }


        // TODO: delete these 4 methods - not needed
        public static int GetBuild()
        { 
            Assembly assembly = Assembly.GetExecutingAssembly();
            return AssemblyName.GetAssemblyName(assembly.Location).Version.Build;
        }
        public static int GetRevision()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            return AssemblyName.GetAssemblyName(assembly.Location).Version.Revision;
        }
        public static int GetMajor()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            return AssemblyName.GetAssemblyName(assembly.Location).Version.Major;
        }
        public static int GetMinor()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            return AssemblyName.GetAssemblyName(assembly.Location).Version.Minor;
        }
      



    }
}


