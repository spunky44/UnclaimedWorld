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
    /// <summary>
    /// this panel is placed at the bottom of the screen and has buttons that perform actions on the map, such as build
    /// </summary>
    public class ActionPanel
    {
        public Window DisplayWindow;
       
        protected InGameInterface intf = The.InGameUI;

        const int height = 49;

        //const int x = 760;

        public ImageButton tbBuild;

       
        InputData frameInput;

        public ActionPanel(int posX)
        {
            CreateButtonWindow(posX, out DisplayWindow);
         
            frameInput = The.Client.Controller.InputData;
       
            int buttonLeft = 27;
            int buttonTop = 12;

            int buttonXSpacing = 2, buttonYSpacing = 0;

            if (The.Sim.Mode == Sim.EngineMode.Game)
            {

           /*     tbBuild = new ImageButton(intf.gui);
                tbBuild.Init(ImageButtonType.BuildButton);
                tbBuild.Position = new Point(buttonLeft, buttonTop);
                DisplayWindow.Add(tbBuild);
                tbBuild.Click += new ClickHandler(build_Click);
              //  tbBuild.MouseOut += new MouseOutHandler(tbBuild_MouseOut);
                tbBuild.ToolTip = "Order building of structures";
                */

            }           
        }

        private void CreateButtonWindow(int xPos, out Window buttonWindow)
        {
            buttonWindow = new Window(intf.gui);
            buttonWindow.Position = new Point(xPos, The.Client.GraphicsDevice.Viewport.Height - height);
            buttonWindow.WindowSize = new Vector2(90, height); //Interface.Instance.mainPanelHeight);
            buttonWindow.Level = Level.Bottom;

            buttonWindow.IsMovable = false;
            buttonWindow.Resizable = false;
            buttonWindow.Margin = 0;
            buttonWindow.HasCloseButton = false;

            buttonWindow.Skin = intf.gui.GUISpriteSheet.GetSourceRectangle("buildpanel");
            buttonWindow.CornerSize = 1; // change this to scale the size...

            buttonWindow.Show(); //Make it visible
        }
     

        private void DeselectOtherRadioButtons(ImageButton tb)
        {
            if (tb != tbBuild)
            {
                tbBuild.IsChecked = false;
            }

           /* if (tb != tbAction)
            {
                tbAction.IsChecked = false;
            }*/
           
        }

        
        /*
        void tbBuild_MouseOut(InputEventSystem.MouseEventArgs args)
        {
            if (!The.InGameUI.HUDBuildPanel.DisplayWindow.CheckCoordinates(frameInput.mouseX, frameInput.mouseY))
            {               
              //  The.InGameUI.HUDBuildPanel.Hide();
            }
        }

       
       
        void build_Click(UIComponent sender, EventArgs e)
        {
            if (!intf.HUDBuildPanel.DisplayWindow.Visible)
            {               
                intf.HUDBuildPanel.ShowInScreenSpace(sender.AbsolutePosition.X - 105,
                   sender.AbsolutePosition.Y - intf.HUDBuildPanel.DisplayWindow.Height - 7); 
            }
            else
            {
                intf.HUDBuildPanel.Hide();
            }

          //  DeselectOtherRadioButtons(tbBuild);
        }*/

    }
}
