using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.ClientSide.Interface.HUD_Windows;
using WindowSystem;
using Microsoft.Xna.Framework;
using UWGame.SimSide;

namespace UWGame.ClientSide.Interface
{
    /// <summary>
    /// has a plastic panel part and an expandable HUD panel part
    /// </summary>
    public class LogPanel
    {
        public HUDLog HUDEventLog;

        public Window PlasticPanel;
        
        List<LogAlert> activeAlerts = new List<LogAlert>();

       // EventLogAlert[] alertWindows;
        List<LogAlert> inactiveAlerts = new List<LogAlert>();
        
        public const int AlertWindowOverlap = 4;

        const int plasticEdgeWidth = 26;

        private int maxWidth = 100;

        public LogPanel(int xPos,int width)
        {
            int height = 43;

            PlasticPanel = new Window(The.InGameUI.gui);
            PlasticPanel.Skin = The.InGameUI.gui.GUISpriteSheet.GetSourceRectangle("log_panel_frame"); //"event_base_small2");
            PlasticPanel.CornerSize = 36;
            PlasticPanel.Margin = 0;
            PlasticPanel.IsMovable = false;
            PlasticPanel.Resizable = false;
            PlasticPanel.HasCloseButton = false;
            PlasticPanel.Position = new Point(xPos, The.Client.Controller.DrawArea.Height - height); // .GraphicsDevice.Viewport.Height - height); // dimensions.Top);
            PlasticPanel.WindowSize = new Vector2(width+55, height);
            PlasticPanel.Level = Level.Bottom; // .RockBottom;
            PlasticPanel.Show();
           
            maxWidth = width + 100;

            HUDEventLog = new HUDLog(xPos + plasticEdgeWidth,width);
            HUDEventLog.DisplayWindow.Resize += HUDEventLog_Resize;
            HUDEventLog.DisplayWindow.Move += HUDEventLog_Move; // increasing the height will trigger this

            ResizePlasticPart();
           
            for (int i = 0; i < The.Client.Controller.Options.MaxAlerts; i++)
            {
                inactiveAlerts.Add(new LogAlert());
            }

           
            The.Client.Log.NewEventAlert += new Log.NewEventHandler(Log_NewEventAlert);

        }

        void HUDEventLog_Move(UIComponent sender)
        {
            ArrangeAlertWindows();

        }

        /// <summary>
        /// reposition alert windows above the log
        /// </summary>
        /// <param name="sender"></param>
        void HUDEventLog_Resize(UIComponent sender)
        {
            ArrangeAlertWindows();

            ResizePlasticPart();
        }

        void ResizePlasticPart()
        {
            PlasticPanel.Width = HUDEventLog.DisplayWindow.Width + 2 * plasticEdgeWidth;


            //To prevent the window to be able to be dragged outside of the screen we set a maxvalue
            if(PlasticPanel.Width > maxWidth) 
            {
                PlasticPanel.Width = maxWidth;                
            }

            if (HUDEventLog.DisplayWindow.Width > maxWidth-58)
            {
                HUDEventLog.DisplayWindow.Width = maxWidth-58;
            }
        }


       

        /// <summary>
        /// shows an important log event in a window above the log
        /// </summary>
        /// <param name="newEvent"></param>
        void Log_NewEventAlert(Log.Event newEvent)
        {            
            string newText = HUDLog.FormatLogEntryText(newEvent, false);
            string plainText = Label.GetPlainText(newText);

            // filter alerts. only add a new alert if the same alert message is not already in the active stack...
            if (activeAlerts.Exists(a => a.Text == plainText))
                return;

            LogAlert newAlert;
            if (inactiveAlerts.Count > 0)
            {
                newAlert = inactiveAlerts[0];
                inactiveAlerts.RemoveAt(0);
            }
            else
            {
                // take the oldest from the active alerts:
                //newAlert = activeAlerts.Dequeue();
                newAlert = activeAlerts[0];
                activeAlerts.RemoveAt(0);
            }

            newAlert.Show(newText);

            // place on top:
            activeAlerts.Add(newAlert);
            //activeAlerts.Enqueue(newAlert);

            ArrangeAlertWindows();
            
        }

        public int GetTopOfAlertWindows()
        {
            if (activeAlerts.Count > 0)
            {
                return activeAlerts[activeAlerts.Count - 1].DisplayWindow.Y;
            }
            else return GetFirstAlertPosition();
        }


        private int GetFirstAlertPosition()
        {
            return HUDEventLog.DisplayWindow.Y - LogAlert.AlertWindowHeight + 5;
        }

        public void Update(GameTime gameTime)
        {
            LogAlert alert;
            int noActive = activeAlerts.Count;

            for (int i = activeAlerts.Count - 1; i >= 0; i--)
            {
              
                alert = activeAlerts[i];

                if (alert.IsExpired())
                {
                    alert.Hide();
                }
            }

            if (noActive != activeAlerts.Count)
            {
                ArrangeAlertWindows();
            }
        }

        private void ArrangeAlertWindows()
        {
            LogAlert alert;

            int lastYPos = GetFirstAlertPosition();

            // we assume the windows are ordered in the array...
            for (int i = 0; i < activeAlerts.Count; i++)
            {
                alert = activeAlerts[i];

                alert.DisplayWindow.Y = lastYPos;

                // same width also:
                alert.DisplayWindow.Width = HUDEventLog.DisplayWindow.Width;

                lastYPos -= AlertWindowOverlap;
            }


        }

        public void Retire(LogAlert alert)
        {
            activeAlerts.Remove(alert);
            inactiveAlerts.Add(alert);
        }

        public void Hide()
        {
            PlasticPanel.Hide();
            HUDEventLog.Hide();
        }
    }
}
