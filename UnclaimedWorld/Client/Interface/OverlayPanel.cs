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
using UWGame.SimSide.Resources;

namespace UWGame.ClientSide.Interface
{
    /// <summary>
    /// this panel is placed at the bottom of the screen and has buttons that switch map overlays on and off
    /// </summary>
    public class OverlayPanel
    {
        public Window DisplayWindow;

        protected InGameInterface intf = The.InGameUI;

        const int height = 53;

        public ImageButton btOverlay;
        public ImageButton btResource;
        public ImageButton btAccessMinimap;

        InputData frameInput;

        public OverlayPanel(int xPos)
        {
            CreateButtonWindow(intf.gui, xPos, "scanpanel", 22, out DisplayWindow);
            frameInput = The.Client.Controller.InputData;

            int buttonLeft = 10;
            int buttonTop = 16;

            /*
            if (The.Sim.Mode == Sim.EngineMode.Game)
            {*/
                btAccessMinimap = new ImageButton(intf.gui);
                btAccessMinimap.Init(ImageButtonType.Minimap);
                btAccessMinimap.Position = new Point(buttonLeft, buttonTop);
                DisplayWindow.Add(btAccessMinimap);
                btAccessMinimap.Click += new ClickHandler(btAccessMinimap_Click);
                btAccessMinimap.ToolTip = "Toggle the mini-map on/off";

                btOverlay = new ImageButton(intf.gui);
                btOverlay.Init(ImageButtonType.ResourceSelectionArrow);
                btOverlay.Position = new Point(buttonLeft + btAccessMinimap.Width, buttonTop);
                DisplayWindow.Add(btOverlay);
                btOverlay.Click += new ClickHandler(tbOverlay_Click);
                // btOverlay.MouseOut += new MouseOutHandler(tbOverlay_MouseOut);
                btOverlay.ToolTip = "Select what to display on the mini-map and the terrain view";

                
                btResource = new ImageButton(intf.gui);
                btResource.Init(ImageButtonType.ScanButton);
                btResource.Position = new Point(buttonLeft + 2 * btAccessMinimap.Width, buttonTop);
                DisplayWindow.Add(btResource);
                btResource.Click += new ClickHandler(resourceTypeRadioButton_click);
                // btOverlay.MouseOut += new MouseOutHandler(tbOverlay_MouseOut);
                btResource.ToolTip = "Activate the SCAN button to highlight resources in the terrain view";

                btResource.IsChecked = The.InGameUI.OverlaySettings.ShowOverlaysOnGameArea; // true;
              //  resourceTypeRadioButton_click(btResource, null);
                intf.HUDOverlayPanel.Refresh();
           // }
        }


        public static void CreateButtonWindow(GUIManager gui, int xPos, string sprite, int cornerSize, out Window buttonWindow)
        {
            buttonWindow = new Window(gui);
            buttonWindow.Position = new Point(xPos, The.Client.Controller.DrawArea.Height - height); //GraphicsDevice.Viewport.Height - height);
            buttonWindow.WindowSize = new Vector2(124, height); //Interface.Instance.mainPanelHeight);
            buttonWindow.Level = Level.Bottom;

            buttonWindow.IsMovable = false;
            buttonWindow.Resizable = false;
            buttonWindow.Margin = 0;
            buttonWindow.HasCloseButton = false;

            buttonWindow.Skin = gui.GUISpriteSheet.GetSourceRectangle(sprite);
            buttonWindow.CornerSize = cornerSize;

            buttonWindow.Show(); //Make it visible
        }

        //just for testing purposes
        void btAccessMinimap_Click(UIComponent sender, EventArgs e)
        {
            if (The.InGameUI.Minimap.DisplayWindow.IsVisibleAndActive)
            {
                The.InGameUI.Minimap.Hide();
            }
            else
            {
                The.InGameUI.Minimap.Show();
            }
        }

        void resourceTypeRadioButton_click(UIComponent sender, EventArgs e)
        {
            //  DeselectOldRadioButton(sender);            
            // ImageButton button = sender as ImageButton;
            /*
            ResourceCategory category = (ResourceCategory)sender.Tag1;
            The.InGameUI.ResourceOutlinesToRender = category;*/

            The.InGameUI.OverlaySettings.ShowOverlaysOnGameArea = ((ImageButton)sender).IsChecked; 

            /*
            if (The.Client.Renderer.renderResourceOutlineState == Map.GameWorldRenderer.RenderResourceOutlinesState.None)
            {
                The.Client.Renderer.renderResourceOutlineState = Map.GameWorldRenderer.RenderResourceOutlinesState.Default;
            }
            else
            {
                The.Client.Renderer.renderResourceOutlineState = Map.GameWorldRenderer.RenderResourceOutlinesState.None;
            }*/
        }

        void tbOverlay_MouseOut(InputEventSystem.MouseEventArgs args)
        {
            if (!The.InGameUI.HUDOverlayPanel.DisplayWindow.CheckCoordinates(frameInput.mouseX, frameInput.mouseY))
            {
                //The.InGameUI.HUDResourceOverPanel.Hide();
            }
        }

        void tbOverlay_Click(UIComponent sender, EventArgs e)
        {

            if (!intf.HUDOverlayPanel.DisplayWindow.IsVisibleAndActive)
            {
                int screenWidth = (int)Common.Clamp(0.22 * The.MapUI.mapWindowWidth, InGameInterface.minimapMinWidth, InGameInterface.minimapMaxWidth);
                int screenX = (screenWidth == InGameInterface.minimapMaxWidth ? 15 : 0);


                intf.HUDOverlayPanel.ShowInScreenSpace(sender.AbsolutePosition.X - 45,
                   sender.AbsolutePosition.Y - intf.HUDOverlayPanel.DisplayWindow.Height - 7); // + 10);

                intf.HUDOverlayPanel.Refresh();
            }
            else
            {
                intf.HUDOverlayPanel.Hide();
            }

        }


    }
}
