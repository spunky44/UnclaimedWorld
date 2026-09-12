using System;
using System.Collections.Generic;
using System.Text;
using WindowSystem;
using InputEventSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
//using Microsoft.Xna.Framework.Storage;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Items;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Vehicles;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Buildings;
namespace UWGame.ClientSide.Interface
{
    public class ExpandedPanel: Panel
    {
        protected bool hasBeenDrawn = false;

       // Image dirtRight;

        protected int TitleHeight;

        protected Window cablesWindow;

        public bool HasCRT = false;
        
      //  protected float topMargin = 20;

        public ExpandedPanel(string topTitle)
            : this(topTitle, The.InGameUI, new Point(The.InGameUI.expandedInterfaceLeft, InGameInterface.rosterPanelTop), new Vector2(The.InGameUI.expandedInterfaceWidth,
                        The.InGameUI.rosterPanelHeight), Level.Middle, true)
        {
                      

        }

        public ExpandedPanel(string topTitle, CommonInterface intf, Point pos, Vector2 dimensions, Level level, bool includeCables)
            : base(intf, null, pos, dimensions, level)
        {
            Window.HasCloseButton = false; // we use our own...
            Window.Level = Level.Middle;

            /* don't show the close button... we want a better design.
            TextButton btClose = AddCloseButton(Form);
            btClose.Click += new ClickHandler(Close_OnPress);
            */

          /*  Label title = new Label(intf.gui);
            Window.Add(title);
            title.Text = topTitle;
            title.X = MarginX;
            title.Y = MarginY;
            title.Init(Label.LabelType.PlainPanelHeader);
            */
         /*   if (includeCables)
            {
                Rectangle rect = intf.gui.GUISpriteSheet.GetSourceRectangle("event_cables");
                cablesWindow = InGameInterface.CreateBackgroundWindow(intf.gui, rect, 
                    new Point((int)(pos.X + dimensions.X), (int)(pos.Y + dimensions.Y - 136)));
                
                cablesWindow.Hide();
            }*/
            
        }

        public override void Hide()
        {
            if (cablesWindow != null)
            {
                cablesWindow.Hide();
            }

            base.Hide();
        }

        public override void Show()
        {
            if (cablesWindow != null)
            {
                cablesWindow.Show();
            }

            base.Show();
        }

      /*  public static TextButton AddCloseButton(Window Form)
        {
            TextButton btClose = new TextButton(Form.guiManager); //intf.gui);
            Form.Add(btClose); // add first!!! sets defaults!
            btClose.Init(TextButton.TextButtonType.White);
            btClose.Position = new Point(Form.Width - 36, 6);
            btClose.Text = "X";
            btClose.Width = 32;
            btClose.Height = 32;
            btClose.ToolTip = "[Esc] Close the panel.";
            return btClose;
        }*/

        protected void AddDirtOnEdges()
        {
            AddDirtOnEdges(Interface.gui, Window);
        }

        public static void AddDirtOnLeftEdge(GUIManager gui, Window Form)
        {
            Rectangle rect = gui.GUISpriteSheet.GetSourceRectangle("basic_dirt_left");
            Image dirtLeft = AddImage(gui, Form, rect, new Point(0, 0));
            dirtLeft = AddImage(gui, Form, rect, new Point(0, rect.Height));
            dirtLeft.RenderType = RenderType.Overlay;
        }

        public static Image AddDirtOnBottomEdge(GUIManager gui, Window Form)
        {
            Rectangle rect = gui.GUISpriteSheet.GetSourceRectangle("basic_dirt_bottom");
            Image dirtBottom = AddImage(gui, Form, rect, new Point(0, 0));
            dirtBottom = AddImage(gui, Form, rect, new Point(0, Form.Height - rect.Height));           
            dirtBottom.RenderType = RenderType.Overlay;

            return dirtBottom;
        }

        /// <summary>
        /// must be drawn after LCD. Is drawn with overlay in order to stay on top of recessed part of lcd panel.
        /// </summary>
        public static void AddDirtOnEdges(GUIManager gui, Window Form)
        {
            Rectangle rect = gui.GUISpriteSheet.GetSourceRectangle("basic_dirt_left");
            Image dirtLeft = AddImage(gui, Form, rect, new Point(0, 0));            
            dirtLeft = AddImage(gui, Form, rect, new Point(0, rect.Height));
            dirtLeft.RenderType = RenderType.Overlay;

            rect = gui.GUISpriteSheet.GetSourceRectangle("basic_dirt_bottom");
            AddImage(gui, Form, rect, new Point(0, Form.Height - rect.Height));

            rect = gui.GUISpriteSheet.GetSourceRectangle("basic_dirt_top");
            Image dirtTop = AddImage(gui, Form, rect, new Point(0, -9));
            dirtTop.RenderType = RenderType.Overlay;
            dirtTop = AddImage(gui, Form, rect, new Point(rect.Width, -9));
            dirtTop.RenderType = RenderType.Overlay;

            rect = gui.GUISpriteSheet.GetSourceRectangle("basic_dirt_right");
            Image dirtRight = AddImage(gui, Form, rect, new Point(Form.Width - rect.Width, 0));
            dirtRight.RenderType = RenderType.Overlay;
        }

            
        /// <summary>
        /// Close button handler!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Close_OnPress(object sender, EventArgs e)
        {
          
          //  The.InGameUI.CollapseExpandedPanel();
                     
        }
    }
}
