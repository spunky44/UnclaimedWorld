using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;
using Microsoft.Xna.Framework;

namespace UWGame.ClientSide.Interface.DateAndWeatherPanel
{
    /// <summary>
    /// contains a plastic part and a hud part, which are 2 windows.
    /// </summary>
    public class DateAndWeather
    {
        public HUDDateAndWeather HUDPanel;

        public Window PlasticPanel;
        Bar frame;

        private const int hudWidth = 385;//368;
        public const int Width = hudWidth + 2 * hudWindowXPos;

        private const int frameEdgeWidth = 33;
        private const int hudWindowXPos = 30;

        public DateAndWeather(int xPos)
        {

            HUDPanel = new HUDDateAndWeather(xPos + hudWindowXPos, hudWidth);
            HUDPanel.DisplayWindow.Y = 14;

            HUDPanel.DisplayWindow.Resize += new ResizeHandler(DisplayWindow_Resize);


            PlasticPanel = new Window(The.InGameUI.gui);
         //   plasticPanel.Skin = The.InGameUI.gui.GUISpriteSheet.GetSourceRectangle("date_weather_panel_frame"); //"event_base_small2");
         //   plasticPanel.CornerSize = frameEdgeWidth;
            PlasticPanel.Margin = 0;
            PlasticPanel.IsMovable = false;
            PlasticPanel.Resizable = false;
            PlasticPanel.HasCloseButton = false;
            PlasticPanel.Position = new Point(xPos, 0); 
            PlasticPanel.WindowSize = new Vector2(30, 43);
            PlasticPanel.Level = Level.Bottom; 
            PlasticPanel.Show();

            frame = new Bar(The.InGameUI.gui);
            frame.SetSkinLocation(SkinState.Normal,The.InGameUI.gui.GUISpriteSheet.GetSourceRectangle("date_weather_panel_frame"));
            frame.EdgeSize = frameEdgeWidth;
            PlasticPanel.Add(frame);
            frame.Height = PlasticPanel.Height;
            
            UpdateFrameSize();

        }

        public void SetPosition(int xPos)
        {
            PlasticPanel.X = xPos;
            HUDPanel.DisplayWindow.X = xPos + hudWindowXPos;

        }

        void DisplayWindow_Resize(UIComponent sender)
        {
            UpdateFrameSize();

        }

        private void UpdateFrameSize()
        {
            PlasticPanel.Width = HUDPanel.DisplayWindow.Width + 2 * hudWindowXPos;

            frame.Width = PlasticPanel.Width;
        }
    }
}
