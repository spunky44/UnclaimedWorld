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


namespace UWGame.ClientSide.Interface
{
    /// <summary>
    /// contains developer options.
    /// </summary>
    public class MainMenuDevPanel : Panel
    {
        
        int controlTop = 70;


        public MainMenuDevPanel(MainMenuInterface intf, Point position) :
            base(intf, "DEV OPTIONS", position,
                 new Vector2(360, MainMenuPanel.PanelHeight), Level.Dialogs, PanelType.MainMenu)
        {
            
            InitButtons();

        }

        


        private void InitButtons()
        {           

            Rectangle rect = Interface.gui.GUISpriteSheet.GetSourceRectangle("main_panel_dirt_center");
            Panel.AddImage(Interface.gui, Window, rect, new Point(40, 30));
            
            int columnWidth = 120;

            int blotY = 64;

            int blotHeight = 96;
            
            /////////////////////////////////
           /* Box gameBlot = new Box(Interface.gui);
            Form.Add(gameBlot);
            rect = Interface.gui.GUISpriteSheet.GetSourceRectangle("basic_darkblue"); 
            gameBlot.SetSkinLocation(SkinState.Normal,rect);
            gameBlot.CornerSize = 3;
            gameBlot.Position = new Point(MarginX, blotY);
            gameBlot.Width = 102;
            gameBlot.Height = blotHeight; 
            */

            int top = MainMenuPanel.ButtonTop; // blotY + 5;

            int xPos = MarginX + 26;
            int spacing = 7;

         

            TextButton btStartTest = new TextButton(Interface.gui);
            Window.Add(btStartTest); // add first!!! sets defaults!
            btStartTest.Init(TextButton.TextButtonType.White);
            btStartTest.Position = new Point(xPos, top);
            btStartTest.Text = "TEST";
            btStartTest.Click += new ClickHandler(btContinueGame_Click);
            btStartTest.ToolTip = "Start a test scenario";
            btStartTest.Width = MainMenuPanel.ButtonWidth;
      
            TextButton btLoadReplay = new TextButton(Interface.gui);
            Window.Add(btLoadReplay); // add first!!! sets defaults!
            btLoadReplay.Init(TextButton.TextButtonType.White);
            btLoadReplay.Position = new Point(xPos, MainMenuPanel.SecondButtonRowYPos);
            btLoadReplay.Text = "LOAD REPLAY";
            btLoadReplay.Click += new ClickHandler(btLoadReplay_Click);
            btLoadReplay.ToolTip = "Load replay";
            btLoadReplay.Width = MainMenuPanel.ButtonWidth;
           
        }

            
        void btLoadReplay_Click(UIComponent sender, EventArgs e)
        {
            ((MainMenuInterface)Interface).mainMenuScreen.LoadReplay();
        }
              

        void btContinueGame_Click(UIComponent sender, EventArgs e)
        {
            ((MainMenuInterface)Interface).mainMenuScreen.StartTest();
        }

        

        public override void ShowDialog(bool modal)
        {

            base.ShowDialog(modal);

        }

       
    }
}
