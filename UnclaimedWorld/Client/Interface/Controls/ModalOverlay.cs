using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;

namespace UWGame.ClientSide.Interface.Controls
{
    public class ModalOverlay
    {
        Icon modalTintOverLCDSurface;
        Icon modalTintOverWindow;
        Icon modalTintOverDisplay;

        Color color = new Color(0, 0, 0, 0.6f);

        public ModalOverlay(Window Window)
        {
            modalTintOverLCDSurface = new Icon(Window.guiManager);
            modalTintOverLCDSurface.SetSkinLocation(SkinState.Normal,Window.guiManager.GUISpriteSheet.GetSourceRectangle("whiteSquare"), color, color);
            modalTintOverLCDSurface.ScaleImageToSizeOfControl = true;
            //  modalTintInnerPanel.ID = UIComponent.DataControlID.InnerOverlay;
            modalTintOverLCDSurface.X = 0;
            modalTintOverLCDSurface.Y = 0;
            modalTintOverLCDSurface.Width = Window.Width;
            modalTintOverLCDSurface.Height = Window.Height;
            modalTintOverLCDSurface.Visible = true;

            modalTintOverDisplay = new Icon(Window.guiManager);
            modalTintOverDisplay.SetSkinLocation(SkinState.Normal,Window.guiManager.GUISpriteSheet.GetSourceRectangle("whiteSquare"), color, color);
            modalTintOverDisplay.ScaleImageToSizeOfControl = true;
            //  tintedMiddlePanel.ID = UIComponent.DataControlID.MiddleOverlay;
            modalTintOverDisplay.X = 0;
            modalTintOverDisplay.Y = 0;
            modalTintOverDisplay.Width = Window.Width;
            modalTintOverDisplay.Height = Window.Height;
            modalTintOverDisplay.Visible = true;

            modalTintOverWindow = new Icon(Window.guiManager);
            modalTintOverWindow.SetSkinLocation(SkinState.Normal,Window.guiManager.GUISpriteSheet.GetSourceRectangle("whiteSquare"), color, color);
            modalTintOverWindow.ScaleImageToSizeOfControl = true;
            // tintedOuterPanel.ID = UIComponent.DataControlID.OuterOverlay;
            modalTintOverWindow.X = 0;
            modalTintOverWindow.Y = 0;
            modalTintOverWindow.Width = Window.Width - 6;
            modalTintOverWindow.Height = Window.Height;
            modalTintOverWindow.Visible = true;

        }

        /// <summary>
        /// don't confuse this with the gui manager modal mode...
        /// </summary>
        public void Show(Window Window, UIComponent lcdSurface, UIComponent display)
        {
            
        /*    modalTintOverLCDSurface = new Icon(Interface.gui);
            modalTintOverLCDSurface.SetSkinLocation(SkinState.Normal,Interface.gui.GUISpriteSheet.GetSourceRectangle("whiteSquare"), color, color);
            modalTintOverLCDSurface.ScaleImageToSizeOfControl = true;
            //  modalTintInnerPanel.ID = UIComponent.DataControlID.InnerOverlay;
            modalTintOverLCDSurface.X = 0;
            modalTintOverLCDSurface.Y = 0;
            modalTintOverLCDSurface.Width = Window.Width;
            modalTintOverLCDSurface.Height = Window.Height;
            modalTintOverLCDSurface.Visible = true;

            modalTintOverDisplay = new Icon(Interface.gui);
            modalTintOverDisplay.SetSkinLocation(SkinState.Normal,Interface.gui.GUISpriteSheet.GetSourceRectangle("whiteSquare"), color, color);
            modalTintOverDisplay.ScaleImageToSizeOfControl = true;
            //  tintedMiddlePanel.ID = UIComponent.DataControlID.MiddleOverlay;
            modalTintOverDisplay.X = 0;
            modalTintOverDisplay.Y = 0;
            modalTintOverDisplay.Width = Window.Width;
            modalTintOverDisplay.Height = Window.Height;
            modalTintOverDisplay.Visible = true;

            modalTintOverWindow = new Icon(Interface.gui);
            modalTintOverWindow.SetSkinLocation(SkinState.Normal,Interface.gui.GUISpriteSheet.GetSourceRectangle("whiteSquare"), color, color);
            modalTintOverWindow.ScaleImageToSizeOfControl = true;
            // tintedOuterPanel.ID = UIComponent.DataControlID.OuterOverlay;
            modalTintOverWindow.X = 0;
            modalTintOverWindow.Y = 0;
            modalTintOverWindow.Width = Window.Width - 6;
            modalTintOverWindow.Height = Window.Height;
            modalTintOverWindow.Visible = true;
            */

            lcdSurface.Add(modalTintOverLCDSurface);
            display.Add(modalTintOverDisplay);
            Window.Add(modalTintOverWindow);

        }

        public void Remove(Window Window, UIComponent lcdSurface, UIComponent display)
        {
            lcdSurface.Remove(modalTintOverLCDSurface);
            display.Remove(modalTintOverDisplay);
            Window.Remove(modalTintOverWindow);
        }

    }
}
