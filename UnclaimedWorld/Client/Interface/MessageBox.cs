using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;
using Microsoft.Xna.Framework;
using System.IO;
using UWGame.SimSide.Maps;
using System.Xml.Serialization;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.AllGameData;
using UWGame.SimSide;
using GameStateManagement;
using UWGame.ClientSide.Screens;
using UWGame.Control;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Xna.Framework.Graphics;



namespace UWGame.ClientSide.Interface
{
    /// <summary>
    /// a simple modal dialog
    /// </summary>
    public class MessageBox : Panel
    {

        protected Box display;
        protected LCDScreen lcdScreen;
        protected UIComponent lcdSurface;

        TextArea taContent;

        public event EventHandler CancelClick;
        public event EventHandler OKClick;

        TextButton btOK, btCancel;

        public MessageBox(CommonInterface intf, string title = "MESSAGE") :
            base(intf /*The.InGameUI*/, title, new Point(420, 300), 
                 new Vector2(330, 250), Level.MessageBox, PanelType.RegularEdges)
        {

            int xPos = MarginX + 6;
            int spacing = 6;

            int buttonX = MarginX + spacing;


            btOK = new TextButton(Interface.gui);
            Window.Add(btOK); // add first!!! sets defaults!
            btOK.Init(TextButton.TextButtonType.White);
            btOK.Text = "OK";
            PlaceLeftButtonUnderLCD(btOK);            
          //  btOK.Position = new Point(xPos, Form.Height - MarginBottom - btOK.Height - spacing);
            btOK.Click += new ClickHandler(btOK_Click);
            btOK.ScaleWidthToFitText();


            btCancel = new TextButton(Interface.gui);
            Window.Add(btCancel); // add first!!! sets defaults!
            btCancel.Init(TextButton.TextButtonType.White);   
            btCancel.Text = "CANCEL";
            btCancel.Click += new ClickHandler(btCancel_Click);
            btCancel.ScaleWidthToFitText();
            PlaceRightButtonUnderLCD(btCancel);

          //  Rectangle rect = Interface.gui.GUISpriteSheet.GetSourceRectangle("main_panel_dirt_center");
          //  Panel.AddImage(Interface.gui, Form, rect, new Point(40, 90));        


            FullLCDPanel.AddLCDPanelFitWindowWithBottomMargin(Interface, Window, BottomMarginForButtons,
                RosterMargin, out display, out lcdSurface, ref lcdScreen);


            taContent = new TextArea(Interface.gui, ListBoxType.LCD);

            taContent.RenderType = RenderType.CRTAndLCD;
            taContent.Init(Label.LabelType.LCDNormal);
            taContent.CanGrowInHeight = true;
            lcdSurface.Add(taContent);
            taContent.X = lcdSideMargin;
            taContent.Y = 0;
            taContent.Width = lcdSurface.Width - 2 * lcdSideMargin; 

            AddDirtOnStraightEdges();

            Hide();

        }

              
        public string Text
        {
            set
            {
                taContent.Text = value;
            }
        }
     


        void btCancel_Click(UIComponent sender, EventArgs e)
        {           
            Hide();

            if (CancelClick != null)
                CancelClick.Invoke(this, null);

        }

        void btOK_Click(UIComponent sender, EventArgs e)
        {
           
            Hide();

            if (OKClick != null)
                OKClick.Invoke(this, null);
            
        }

        public enum ButtonOptions { OK, OKAndCancel, None }
        public virtual void ShowMessage(string message, string title = null, bool modal = true, ButtonOptions buttonOptions = ButtonOptions.OK) //, UIComponent positionOverControl = null)
        {
            taContent.Text = message;

            switch (buttonOptions)
            {
                case ButtonOptions.OK:
                    btOK.Visible = true;
                    btCancel.Visible = false;
                    break;

                case ButtonOptions.OKAndCancel:
                    btOK.Visible = true;
                    btCancel.Visible = true;
                    break;

                case ButtonOptions.None:
                    btOK.Visible = false;
                    btCancel.Visible = false;
                    break;
            }

            lblTitle.Text = title ?? "MESSAGE";

            base.ShowDialog(modal);

        }

        public override void ShowDialog(bool modal)
        {
            base.ShowDialog(modal);
            
        }

        

    }
}
