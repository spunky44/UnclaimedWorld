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
    public class LoseGamePanel : Panel
    {

       
        Box display; //, edges;
        LCDScreen lcdScreen;
        UIComponent lcdSurface;

        public event EventHandler CancelClick;


        int controlTop = 70;

       // string mapDataXmlPath = "";


      //  public event EventHandler SaveOrLoadClick;
      //  public event EventHandler CancelClick;

        //MainMenuInterface mainMenuIntf;

        TextArea area;
        Grid surfaceGrid;

        public static Vector2 Dimensions = new Vector2(440, 560);


        public string Text
        {
            set
            {
                area.Text = value;
            }
        }

        public LoseGamePanel(LoseGameInterface intf, Point position) :
            base(intf, "GAME END", position,
                 Dimensions, Level.Dialogs)
        {
                        
            FullLCDPanel.AddLCDPanelFitWindowWithBottomMargin(intf, Window, 
                BottomMarginForButtons, new Point(MarginX, MarginTop), out display, out lcdSurface, ref lcdScreen);

            CreateSurfaceWithScrollbar(out surfaceGrid, lcdSurface, false);

            //int sideMargin = 12;

            CreateTextArea(intf, ref area, surfaceGrid);


            TextButton btDone = new TextButton(Interface.gui);
            Window.Add(btDone); // add first!!! sets defaults!
            btDone.Init(TextButton.TextButtonType.White);
            PlaceLeftButtonUnderLCD(btDone);            
            btDone.Text = "DONE";
            btDone.Click += new ClickHandler(btDone_Click);
            btDone.ScaleWidthToFitText();
        
            AddDefaultDirt();


        }

       

                    
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

       

    }
}
