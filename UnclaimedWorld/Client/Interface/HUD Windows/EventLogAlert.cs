using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;
using Microsoft.Xna.Framework;

namespace UWGame.ClientSide.Interface.HUD_Windows
{
    /// <summary>
    /// this HUD window appears above the log for a few seconds as an alert to the player.
    /// </summary>
    public class LogAlert : HUDWindow
    {
        Label label;


        public string Text
        {
            get { return label.Text; }
        }

        public static Color AlertColor = Color.GreenYellow; //.Yellow is the link hover color

        private double timeElapsed = 0;

        public const int AlertWindowHeight = 28;

        public LogAlert()
            : base(HUDLog.DefaultWidth, AlertWindowHeight, true, true, false, "HUD_window_base", false)
        {
            base.HideOnRightClick = false;

            CloseButtonYPos = 2;
            //btClose.Y = 0; // 2;

            DisplayWindow.Level = Level.Bottom;

            label = new Label(DisplayWindow.guiManager);
            Add(label);
            label.Init(Label.LabelType.HUDWindow);
            label.NormalColor = AlertColor;
            label.X = 8;
            label.Y = 6;
            SetLabelSize();

            base.DisplayWindow.Resize += DisplayWindow_Resize;
        }

        private void SetLabelSize()
        {
            label.Width = btClose.X - 2 - label.X;
        }

        void DisplayWindow_Resize(UIComponent sender)
        {
            SetLabelSize();
        }

        /// <summary>
        /// don't include a timestamp in the text. that makes it hard to filter alert spam...
        /// </summary>
        /// <param name="text"></param>
        public void Show(string text)
        {
            label.Text = text;
           // label.FitToText();

            timeElapsed = 0d;

            DisplayWindow.X = The.InGameUI.LogPanel.HUDEventLog.DisplayWindow.X;
            DisplayWindow.Y = The.InGameUI.LogPanel.GetTopOfAlertWindows() - LogPanel.AlertWindowOverlap;
            DisplayWindow.Width = The.InGameUI.LogPanel.HUDEventLog.DisplayWindow.Width;

            DisplayWindow.Show();
            DisplayWindow.BringToTop();

        }

        

        public override void Update(GameTime gameTime)
        {
            timeElapsed += gameTime.ElapsedGameTime.TotalSeconds;

          /*  if (timeElapsed > The.Client.ScreenManager.UserSettings.AlertLifetime)
            {
                Hide();

            }*/
        }

        public bool IsExpired()
        {
            return timeElapsed > The.Client.Controller.Options.AlertLifetime;
        }

        public override void Hide()
        {
            base.Hide();

            The.InGameUI.LogPanel.Retire(this);
        }
    }
}
