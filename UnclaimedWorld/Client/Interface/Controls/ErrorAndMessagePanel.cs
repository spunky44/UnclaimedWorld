using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.ClientSide.Interface.LCD;
using WindowSystem;

namespace UWGame.ClientSide.Interface.Controls
{
    public class ErrorAndMessagePanel
    {
        LCDInnerPanel lcdErrorMessagePanel;

        ErrorsAndMessages errorsAndMessages;


        public int ContentHeight
        {
            get
            {
                return lcdErrorMessagePanel.ContentHeight;
            }
        }

        public int Height
        {
            get
            {
                return lcdErrorMessagePanel.Height;
            }
        }
        
        public ErrorAndMessagePanel(UIComponent lcdSurface)
        {
            lcdErrorMessagePanel = new LCDInnerPanel(lcdSurface.guiManager, lcdSurface.Width, true, 1f);
            lcdSurface.Add(lcdErrorMessagePanel.Panel);
            lcdErrorMessagePanel.ContentHeight = 30;
            lcdErrorMessagePanel.Panel.Y = lcdSurface.Height - 35; // 24; 
            lcdErrorMessagePanel.VerticalContentPadding = 8;

            errorsAndMessages = new ErrorsAndMessages(lcdSurface, lcdSurface.guiManager, 10, lcdErrorMessagePanel.Panel.Y + 10); //below scrollable area
            
        }

        public void ShowErrors(List<string> errors)
        {
            if (errors != null && errors.Count > 0)
            {
                // perhaps show the whole list in a text area...
                ShowError(errors[0]);
            }
        }

        public void ShowError(string error, string errorTooltip = null)
        {
            errorsAndMessages.ShowError(error, errorTooltip);

            TintErrorPanel(Color.LightSalmon);
        }

       /* public void Clear()
        {
            errorsAndMessages.Hide();
        }*/

        public void Clear()
        {
            TintErrorPanel(new Color(255, 255, 255, 255));
            
            errorsAndMessages.Hide();
        }

        public bool IsShowingError
        {
            get
            {
                return errorsAndMessages.IsShowingError;
            }
        }
      

        private void TintErrorPanel(Color color)
        {
            lcdErrorMessagePanel.TintPanel(color);
            //lcdErrorMessagePanel.Panel.SetSkinLocation(SkinState.Normal,lcdErrorMessagePanel.Panel.guiManager.GUISpriteSheet.GetSourceRectangle("lcd_panel_background"), color, color);
        }       


    }
}
