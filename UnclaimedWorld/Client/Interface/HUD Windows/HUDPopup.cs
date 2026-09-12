using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;
using Microsoft.Xna.Framework;

namespace UWGame.ClientSide.Interface.HUD_Windows // TODO DECOUPLE this goes in client
{
    /// <summary>
    /// base class for popups appearing over the playfield
    /// </summary>
    public abstract class HUDPopup
    {
        public Window DisplayWindow;

        // dimensions of the entire window:         
        protected int windowHeight = 80; // 400 screenHeight + 33;
        protected int windowWidth = 200; //screenWidth + 130;

        protected GUIManager gui;
        protected Game game;

        public HUDPopup(int width, int height)
        {
            windowWidth = width;
            windowHeight = height;


            gui = The.InGameUI.gui;
            game = The.Sim.Controller.Game;


            DisplayWindow = new Window(gui);
            // Sequence matters for skins!!!
            DisplayWindow.Skin = gui.GUISpriteSheet.GetSourceRectangle("HUD_window_base");
            DisplayWindow.Opacity = 0.75f;
            DisplayWindow.CornerSize = 47;
            DisplayWindow.Margin = 0; // 7;
            DisplayWindow.Level = Level.Bottom; // Level.RockBottom; // Level.Bottom;
            DisplayWindow.Resizable = false;
            DisplayWindow.IsMovable = true; // true; // 
            DisplayWindow.Position = new Point(0, The.Client.Controller.DrawArea.Height /* GraphicsDevice.Viewport.Height*/ - windowHeight);
            DisplayWindow.WindowSize = new Vector2(windowWidth, windowHeight);  //new Vector2(screenDimensions.Width + 90, screenDimensions.Height + 40);
            DisplayWindow.HasCloseButton = true; // false;
            DisplayWindow.HasCRTOrLCDComponents = false; // true;
            DisplayWindow.HasOverlayComponents = false; // true;
            //DisplayWindow.IsLCDSurface = true;
            DisplayWindow.Hide();

        }


        protected void Add(UIComponent control)
        {
            DisplayWindow.Add(control);
        }
       
    }
}
