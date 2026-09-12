using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;

namespace UWGame.ClientSide.Interface.LCD
{
    /// <summary>
    /// contains code that displays messages or errors to the user in a specific spot on the panel.
    /// </summary>
    public class ErrorsAndMessages
    {
        Label lblMessages;
        Label lblErrors;

        UIComponent surface;

        public ErrorsAndMessages(UIComponent lcdSurface, GUIManager gui, int x, int y,Label.LabelType labelType = Label.LabelType.LCDNormal)
        {
            this.surface = lcdSurface;

            // create, don't add yet
            lblMessages = new Label(gui);
            lblMessages.Init(labelType);
            lblMessages.X = x;
            lblMessages.Y = y;

            lblErrors = new Label(gui);            
            lblErrors.Init(Label.LabelType.LCDError);
            lblErrors.X = lblMessages.X;
            lblErrors.Y = lblMessages.Y;   
        }

        public int Bottom
        {
            get
            {
                return lblMessages.Bottom;
            }
        }
        public int Right
        {
            get
            {
                return lblMessages.Right;
            }
        }
        public int Y
        {
            get
            {
                return lblMessages.Y;
            }
        }

        public bool IsShowingError
        {
            get
            {
                return surface.Contains(lblErrors) && !string.IsNullOrEmpty(lblErrors.Text);
            }
        }

        public void ShowError(string text, string tooltip = null)
        {
            surface.Add(lblErrors);
            surface.Remove(lblMessages);

            lblErrors.Text = text;
            lblErrors.ToolTip = tooltip;
            lblErrors.FitToText();
        }

        public void ShowMessage(string text, string tooltip = null)
        {
            surface.Add(lblMessages);
            surface.Remove(lblErrors);

            lblMessages.Text = text;
            lblMessages.ToolTip = tooltip;
            lblMessages.FitToText();
        }

        /// <summary>
        /// removes errors or messages - use Show to make them appear
        /// </summary>
        public void Hide()
        {
            surface.Remove(lblErrors);
            surface.Remove(lblMessages);
        }
    }
}
