using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;
using UWGame.ClientSide.Interface;
using Microsoft.Xna.Framework;
using UWGame.SimSide;
using Microsoft.Xna.Framework.Input;
using InputEventSystem;

namespace UWGame.ClientSide.Interface
{
    
    public class MinimapAccessPanel
    {
        public Window DisplayWindow;
       
        protected InGameInterface intf = The.InGameUI;

        const int height = 55;

       
        public ImageButton btAccessMinimap; 


        InputData frameInput;

        public MinimapAccessPanel(int screenX)
        {

           /* CreateButtonWindow(intf.gui, screenX, "minimap_button_base", 1, out DisplayWindow);
            frameInput = The.Client.ScreenManager.InputData;
       
            int buttonLeft = 0;
            int buttonTop = 18;

            int buttonXSpacing = 2, buttonYSpacing = 0;

            if (The.Sim.Mode == Sim.EngineMode.Game)
            {

                btAccessMinimap = new ImageButton(intf.gui);
                btAccessMinimap.Init(ImageButtonType.Minimap);
                btAccessMinimap.Position = new Point(buttonLeft, buttonTop);
                DisplayWindow.Add(btAccessMinimap);
                btAccessMinimap.Click += new ClickHandler(btAccessMinimap_Click);
                btAccessMinimap.ToolTip = "Toggle the minimap";

           // }   */        
        }

        void btAccessMinimap_Click(UIComponent sender, EventArgs e)
        {
            if (The.InGameUI.Minimap.DisplayWindow.Visible)
            {
                The.InGameUI.Minimap.Hide();
            }
            else
            {
                The.InGameUI.Minimap.Show();
            }
        }

        private void CreateButtonWindow(GUIManager gui, int xPos, string sprite, int cornerSize, out Window buttonWindow)
        {
            buttonWindow = new Window(gui);
            buttonWindow.Position = new Point(xPos, The.Client.Controller.DrawArea.Height - height); //GraphicsDevice.Viewport.Height - height);
            buttonWindow.WindowSize = new Vector2(38, height); //Interface.Instance.mainPanelHeight);
            buttonWindow.Level = Level.Bottom;

            buttonWindow.IsMovable = false;
            buttonWindow.Resizable = false;
            buttonWindow.Margin = 0;
            buttonWindow.HasCloseButton = false;

            buttonWindow.Skin = gui.GUISpriteSheet.GetSourceRectangle(sprite);
            buttonWindow.CornerSize = cornerSize;

            buttonWindow.Show(); //Make it visible
        }


               
        void tbOverlay_MouseOut(InputEventSystem.MouseEventArgs args)
        {
            if (!The.InGameUI.HUDOverlayPanel.DisplayWindow.CheckCoordinates(frameInput.mouseX, frameInput.mouseY))
            {               
                The.InGameUI.HUDOverlayPanel.Hide();
            }
        }

        void tbOverlay_Click(UIComponent sender, EventArgs e)
        {
            if (!intf.HUDOverlayPanel.DisplayWindow.Visible)
            {              
                intf.HUDOverlayPanel.ShowInScreenSpace(sender.AbsolutePosition.X,
                   sender.AbsolutePosition.Y - intf.HUDOverlayPanel.DisplayWindow.Height); // + 10);
            }
            else
            {
                intf.HUDOverlayPanel.Hide();
            }

        }

        
    }
}
