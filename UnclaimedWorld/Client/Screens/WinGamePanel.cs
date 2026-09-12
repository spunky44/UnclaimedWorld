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
using UWGame.ClientSide.Interface;


namespace UWGame.ClientSide.Screens
{

    public class WinGamePanel : Panel
    {

       
        Box display; //, edges;
        LCDScreen lcdScreen;
        UIComponent lcdSurface;

        TextArea area;
        Grid surfaceGrid;

        public event EventHandler CancelClick;

        public string Text
        {
            set
            {
                area.Text = value;
            }
        }


        int controlTop = 70;

       // string mapDataXmlPath = "";


      //  public event EventHandler SaveOrLoadClick;
      //  public event EventHandler CancelClick;

        //MainMenuInterface mainMenuIntf;

        public WinGamePanel(WinGameInterface intf, Point position) :
            base(intf, "GAME WON", position,
                 LoseGamePanel.Dimensions, Level.Dialogs)
        {            
            
            FullLCDPanel.AddLCDPanelFitWindowWithBottomMargin(intf, Window, 
                BottomMarginForButtons, new Point(MarginX, MarginTop), out display, out lcdSurface, ref lcdScreen);


            CreateSurfaceWithScrollbar(out surfaceGrid, lcdSurface, false);

            CreateTextArea(intf, ref area, surfaceGrid);

            int sideMargin = 12;

            area = new TextArea(intf.gui, ListBoxType.LCD); 
            area.Init(Label.LabelType.LCDNormal);      
            area.CanGrowInHeight = true;
            surfaceGrid.AddEntry(area, area);
            area.X = sideMargin;
            area.Y = 45;
            area.Width = lcdSurface.Width - 2 * sideMargin; // 196; // triggers BreakText that requires font to be set        



           /* Rectangle rect = Interface.gui.GUISpriteSheet.GetSourceRectangle("main_panel_dirt_center");
            Panel.AddImage(Interface.gui, Form, rect, new Point(40, 30));
            */
            

            TextButton btDone = new TextButton(Interface.gui);
            Window.Add(btDone); // add first!!! sets defaults!
            btDone.Init(TextButton.TextButtonType.White);
            PlaceLeftButtonUnderLCD(btDone);
            //btDone.Position = new Point(12, brown.Y + 5);
            btDone.Text = "DONE";         
            btDone.Click += new ClickHandler(btDone_Click);
            btDone.ScaleWidthToFitText();
        

            AddDefaultDirt();

        }

        

       
       /* void btClose_Click(UIComponent sender, EventArgs e)
        {
            loseGameScreen.ExitToMainMenu();
        */
            //((MainMenuInterface)intf).mainMenuScreen.Exit();

            /*
            Form.Hide();

            if (CancelClick != null)
            {
                CancelClick.Invoke(sender, e);
            }*/
       // }


      
        void btDone_Click(UIComponent sender, EventArgs e)
        {
            if (CancelClick != null)
            {
                CancelClick.Invoke(sender, e);
            }
        }

        

        public string MapDataXmlPath;


        public override void ShowDialog(bool modal)
        {

            base.ShowDialog(modal);

        }


       


      /*  void btCancel_Click(UIComponent sender, EventArgs e)
        {
            Form.Hide();

            if (CancelClick != null)
            {
                CancelClick.Invoke(sender, e);
            }
        }*/

        

    }
}
