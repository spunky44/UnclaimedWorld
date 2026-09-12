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
using UWGame.ClientSide.Interface.HUD_Windows;
using UWGame.Control.Replays;
using Steamworks;
using UWGame.Steam;


namespace UWGame.ClientSide.Interface
{
    public class MainMenuPanel : Panel
    {
        public const int ButtonWidth = 106;

        public const int PanelHeight = 177;

        const int blotTop = 67; // 21;
        const int blotX = 23;

        public const int ButtonTop = 76; // 21;

       // public const int ButtonTop = 81; // 21;

        OptionsDialog optionsDialog;

        public const int SecondButtonRowYPos = 116;


        public MainMenuPanel(MainMenuInterface intf, Point position) :
            base(intf, "MAIN MENU", position,
                 new Vector2(424, PanelHeight /* 180*/), Level.Dialogs, PanelType.MainMenu)
        {


            optionsDialog = new OptionsDialog(intf);
            optionsDialog.CancelClick += new EventHandler(optionsDialog_CancelClick);
            optionsDialog.OKClick += new EventHandler(optionsDialog_OKClick);


                      
            InitButtons();

         /*   RandomGenerator gen = new RandomGenerator(RandomGenerator.GeneratorType.Sim); 
            for (int i = 10 - 1; i >= 0; i--)
            {
                byte[] stabilitySeedNumbers;
                int stabilityRandomSeed = gen.Next("volatilityRandomSeed", false);

                RandomGenerator generator = new RandomGenerator(stabilityRandomSeed, RandomGenerator.GeneratorType.Sim);
                stabilitySeedNumbers = SimplexNoise.CreateSeedNumbers(generator); // these numbers are the same before and after snapshot.

                SimplexNoise.SeedNumbers = stabilitySeedNumbers;

                for (int j = 0; j < 10; j++)
                {                   
               
                    float noise = SimplexNoise.Generate1D(0.1f * j); // GameData.Instance.AIConstants.MigrateStabilityFrequency);
                   // float noise = SimplexNoise.Generate2D(0.1f * j, 0.1f * j); // GameData.Instance.AIConstants.MigrateStabilityFrequency);

                    Console.Write(noise);
                    Console.Write(" ");
                }

                Console.WriteLine();
            }*/
           
        }

        


        void optionsDialog_OKClick(object sender, EventArgs e)
        {
            //HandleChildWindowClose();
        }

        void optionsDialog_CancelClick(object sender, EventArgs e)
        {
            //HandleChildWindowClose();
        }
       
       

        private void InitButtons()
        {           

            // brown blot:
     /*       Box brown = new Box(intf.gui);
            Form.Add(brown);
            rect = intf.gui.GUISpriteSheet.SourceRectangle("basic_brown");
            brown.SetSkinLocation(SkinState.Normal,rect);
            brown.CornerSize = 3;
            brown.Position = new Point(edgesLeft.X + edgesLeft.Width + 10, edgesLeft.Y + 160);
            brown.Width = edgesRight.X - (edgesLeft.X + edgesLeft.Width) - 20;
            brown.Height = 95; 

            Image scratch1 = AddImage(intf.gui, Form, "basic_scratch1", new Point(brown.X - 4, brown.Y - 9));
            Image scratch2 = AddImage(intf.gui, Form, "basic_scratch2", new Point(brown.X - 10, brown.Y + 22));

            // overlay dirt:
            Image bigSplotch = AddImage(intf.gui, Form, "basic_dirt_bigsplotch", new Point(brown.X, brown.Y + 40));
            */

            int columnWidth = 120;

            
            int blotHeight = 84;

           
            Image gameBlot = new Image(Interface.gui);
            Window.Add(gameBlot);
            Rectangle rect = Interface.gui.GUISpriteSheet.GetSourceRectangle("darksquare"); 
            gameBlot.SetSkinLocation(SkinState.Normal,rect);           
            gameBlot.Position = new Point(blotX /*MarginX*/, blotTop); // Form.Height - 40 - MarginY);//edges.Y + edges.Height + 10 + 70);
            gameBlot.Width = 113; // Form.Width - 2 * MarginX;
            gameBlot.Height = blotHeight; // 200;
            gameBlot.ScaleImageToSizeOfControl = true;

            // this looks better underneath the buttons...
            rect = Interface.gui.GUISpriteSheet.GetSourceRectangle("main_panel_dirt_center");
            Panel.AddImage(Interface.gui, Window, rect, new Point(40, 30));
            


            int xPos = blotX + 3; // 6;
            int spacing = 7;

            int blackButtonHeight = 36; // 31;
        
            TextButton btNew = new TextButton(Interface.gui);
            Window.Add(btNew); // add first!!! sets defaults!
            btNew.Init(TextButton.TextButtonType.Black);
            btNew.Text = "NEW GAME";
            btNew.Position = new Point(xPos, gameBlot.Y + 6);
            btNew.Click += new ClickHandler(btNew_Click);
            btNew.Width = ButtonWidth;
            btNew.Height = blackButtonHeight;

            TextButton btLoadGame = new TextButton(Interface.gui);
            Window.Add(btLoadGame); // add first!!! sets defaults!
            btLoadGame.Init(TextButton.TextButtonType.Black);
            btLoadGame.Position = new Point(xPos, btNew.Bottom + 1 /* spacing*/);
            btLoadGame.Text = "LOAD GAME";
            btLoadGame.Click += new ClickHandler(btLoadGame_Click);
            btLoadGame.Width = ButtonWidth;
            btLoadGame.Height = blackButtonHeight;

            /*
            Box toolsBlot = new Box(Interface.gui);
            Form.Add(toolsBlot);
            rect = Interface.gui.GUISpriteSheet.GetSourceRectangle("basic_darkblue"); //"basic_brown");
            toolsBlot.SetSkinLocation(SkinState.Normal,rect);
            toolsBlot.CornerSize = 3;
            toolsBlot.Position = new Point(gameBlot.Right + 6, blotY); // Form.Height - 40 - MarginY);//edges.Y + edges.Height + 10 + 70);
            toolsBlot.Width = 102; // Form.Width - 2 * MarginX;
            toolsBlot.Height = blotHeight; // 200;
           

            xPos = toolsBlot.X + 6;

            Label lblTools = new Label(Interface.gui);
            Form.Add(lblTools);
            lblTools.Init(Label.LabelType.PlainPanelNormal);
            lblTools.Text = "TOOLS";
            lblTools.X = xPos;
            lblTools.Y = toolsBlot.Y - lblTools.TextHeight - 6;
            lblTools.Width = ButtonWidth;
           */

            xPos += columnWidth;

            #if !RELEASE // only in release week

            
            TextButton btEditMap = new TextButton(Interface.gui);
            Window.Add(btEditMap); // add first!!! sets defaults!
            btEditMap.Init(TextButton.TextButtonType.White);
            btEditMap.Position = new Point(xPos, ButtonTop);
            btEditMap.Text = "EDIT MAP";
            btEditMap.Click += new ClickHandler(btEditMap_Click);
            btEditMap.Width = ButtonWidth;

           
            TextButton btTestMap = new TextButton(Interface.gui);
          //  Window.Add(btTestMap); // add first!!! sets defaults!
            btTestMap.Init(TextButton.TextButtonType.White);
            btTestMap.Position = new Point(xPos, btEditMap.Bottom + spacing);
            btTestMap.Text = "TEST MAP";
            btTestMap.Click += new ClickHandler(btTestMap_Click);
            btTestMap.Width = ButtonWidth;

            
#endif            
            TextButton btOptions = new TextButton(Interface.gui);
            Window.Add(btOptions); // add first!!! sets defaults!
            btOptions.Init(TextButton.TextButtonType.White);
            btOptions.Position = new Point(xPos, SecondButtonRowYPos);
            btOptions.Text = "OPTIONS";
            btOptions.Click += btOptions_Click;
            btOptions.Width = ButtonWidth;

            xPos += 114;

            TextButton btCredits = new TextButton(Interface.gui);
            Window.Add(btCredits); // add first!!! sets defaults!
            btCredits.Init(TextButton.TextButtonType.White);
            btCredits.Position = new Point(xPos, ButtonTop);
            btCredits.Text = "CREDITS";
            btCredits.Click += new ClickHandler(btCredits_Click);
            btCredits.Width = ButtonWidth;
          
            TextButton btExit = new TextButton(Interface.gui);
            Window.Add(btExit); // add first!!! sets defaults!
            btExit.Init(TextButton.TextButtonType.White);
            btExit.Position = new Point(xPos, SecondButtonRowYPos);
            btExit.Text = "EXIT";
            btExit.Click += new ClickHandler(btExit_Click);
            btExit.Width = ButtonWidth;
             

           // AddDirtOnStraightEdges();

            /*
            rect = Interface.gui.GUISpriteSheet.GetSourceRectangle("main_panel_dirt_center");
            Panel.AddImage(Interface.gui, Form, rect, new Point(40, 30));
            */
        }

        void btNew_Click(UIComponent sender, EventArgs e)
        {
            ((MainMenuInterface)Interface).mainMenuScreen.ShowScenarios();
        }

        void btCredits_Click(UIComponent sender, EventArgs e)
        {
            ((MainMenuInterface)Interface).mainMenuScreen.ShowCredits();
        }

        void btLoad_Click(UIComponent sender, EventArgs e)
        {
            
        }

        void btExit_Click(UIComponent sender, EventArgs e)
        {
            ((MainMenuInterface)Interface).mainMenuScreen.Exit();
        }

        void btIntro_Click(UIComponent sender, EventArgs e)
        {
            ((MainMenuInterface)Interface).mainMenuScreen.ShowIntro();
        }

        void btOptions_Click(UIComponent sender, EventArgs e)
        {

           // Interface.Game.Controller.StatsAndAchievements.UnlockAchievement(StatsAndAchievements.AchievementID.tutorialCompleted);

            optionsDialog.ShowDialog(true);

            
            //((MainMenuInterface)Interface).mainMenuScreen.ShowOptions();
        }

        void btTestMap_Click(UIComponent sender, EventArgs e)
        {
            ((MainMenuInterface)Interface).mainMenuScreen.TestMap();
        }

        void btEditMap_Click(UIComponent sender, EventArgs e)
        {
            ((MainMenuInterface)Interface).mainMenuScreen.EditMap();
        }

        void btLoadReplay_Click(UIComponent sender, EventArgs e)
        {
            ((MainMenuInterface)Interface).mainMenuScreen.LoadReplay();
        }

        void btLoadGame_Click(UIComponent sender, EventArgs e)
        {
            ((MainMenuInterface)Interface).mainMenuScreen.LoadGame();
        }

       /* void btContinueGame_Click(UIComponent sender, EventArgs e)
        {
            ((MainMenuInterface)Interface).mainMenuScreen.ContinueGame();
        }*/

        

        public string MapDataXmlPath;


        public override void ShowDialog(bool modal)
        {

            base.ShowDialog(modal);

        }

       

        

    }
}
