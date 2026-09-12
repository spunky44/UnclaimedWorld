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
using UWGame.SimSide.Snapshots;
using Microsoft.Xna.Framework.Content;
using UWGame.Client.MainMenu.LoadSavedGame;
using System.IO.Compression;
using System.Threading;

namespace UWGame.ClientSide.Interface
{
    /// <summary>
    /// this in game dialog window contains functions such as Quit, Resume, Save, Load and Options
    /// </summary>
    public class InGameMenuDialog : Panel
    {

        OptionsDialog optionsDialog;

        SaveLoadGamePanel saveDialog, loadDialog;


        public InGameMenuDialog() :
            base(The.InGameUI, "MENU", new Point(400, 280), 
                 new Vector2(260, 290), Level.Menu, PanelType.RegularEdges)
        {

            int buttonWidth = 180;
            int xPos = MarginX + (Window.Width - 2 * MarginX - buttonWidth) / 2;
            int spacing = 6;

            TextButton btContinueGame = new TextButton(Interface.gui);
            Window.Add(btContinueGame); // add first!!! sets defaults!
            btContinueGame.Init(TextButton.TextButtonType.White);
            btContinueGame.Position = new Point(xPos, MarginTop + 5);
            btContinueGame.Text = "RESUME GAME";
            // btContinueGame.ToolTip = (saveOrLoad == SaveOrLoad.Save? "Saves the map data." : "Loads a new map.");
            btContinueGame.Click += new ClickHandler(btContinueGame_Click);
            btContinueGame.Width = buttonWidth;

            TextButton btQuitToMainMenu = new TextButton(Interface.gui);
            Window.Add(btQuitToMainMenu); // add first!!! sets defaults!
            btQuitToMainMenu.Init(TextButton.TextButtonType.White);
            btQuitToMainMenu.Position = new Point(xPos, btContinueGame.Bottom + spacing);
            btQuitToMainMenu.Text = "QUIT TO TITLE SCREEN"; //mp apr 2015 changed from "QUIT TO MENU"       ----mp shortened from "QUIT TO MAIN MENU" because text got cut off
            // btContinueGame.ToolTip = (saveOrLoad == SaveOrLoad.Save? "Saves the map data." : "Loads a new map.");
            btQuitToMainMenu.Click += new ClickHandler(btQuitToMainMenu_Click);
            btQuitToMainMenu.Width = buttonWidth;

            TextButton btQuitToDesktop = new TextButton(Interface.gui);
            Window.Add(btQuitToDesktop); // add first!!! sets defaults!
            btQuitToDesktop.Init(TextButton.TextButtonType.White);
            btQuitToDesktop.Position = new Point(xPos, btQuitToMainMenu.Bottom + spacing);
            btQuitToDesktop.Text = "QUIT TO DESKTOP";
            // btContinueGame.ToolTip = (saveOrLoad == SaveOrLoad.Save? "Saves the map data." : "Loads a new map.");
            btQuitToDesktop.Click += new ClickHandler(btQuitToDesktop_Click);
            btQuitToDesktop.Width = buttonWidth;

            TextButton btOptions = new TextButton(Interface.gui);
            Window.Add(btOptions); // add first!!! sets defaults!
            btOptions.Init(TextButton.TextButtonType.White);
            btOptions.Position = new Point(xPos, btQuitToDesktop.Bottom + spacing);
            btOptions.Text = "OPTIONS";
            // btContinueGame.ToolTip = (saveOrLoad == SaveOrLoad.Save? "Saves the map data." : "Loads a new map.");
            btOptions.Click += new ClickHandler(btOptions_Click);
            btOptions.Width = buttonWidth;

            TextButton btSaveGame = new TextButton(Interface.gui);
            Window.Add(btSaveGame); // add first!!! sets defaults!
            btSaveGame.Init(TextButton.TextButtonType.White);
            btSaveGame.Position = new Point(xPos, btOptions.Bottom + spacing + spacing);
            btSaveGame.Text = "SAVE GAME";
            btSaveGame.Click += new ClickHandler(btSaveGame_Click);
            btSaveGame.Width = buttonWidth;

            TextButton btLoadGame = new TextButton(Interface.gui);
            Window.Add(btLoadGame); // add first!!! sets defaults!
            btLoadGame.Init(TextButton.TextButtonType.White);
            btLoadGame.Position = new Point(xPos, btSaveGame.Bottom + spacing);
            btLoadGame.Text = "LOAD GAME";
            btLoadGame.Click += new ClickHandler(btLoadGame_Click);
            btLoadGame.Width = buttonWidth;


          /*  Rectangle rect = Interface.gui.GUISpriteSheet.GetSourceRectangle("main_panel_dirt_center");
            Panel.AddImage(Interface.gui, Form, rect, new Point(40, 120));
            */

            

            AddDefaultDirt();

            base.Window.Height = btLoadGame.Bottom + SingleSpacing + MarginBottom;



            optionsDialog = new OptionsDialog(The.InGameUI);
            optionsDialog.CancelClick += new EventHandler(optionsDialog_CancelClick);
            optionsDialog.OKClick += new EventHandler(optionsDialog_OKClick);

            saveDialog = new SaveLoadGamePanel(SaveLoadGamePanel.SaveOrLoad.Save, Interface, new Point(300,0) );
            saveDialog.SaveOrLoadClick += new EventHandler(saveDialog_SaveClick);
            saveDialog.CancelClick += new EventHandler(saveloadDialog_CancelClick);

            loadDialog = new SaveLoadGamePanel(SaveLoadGamePanel.SaveOrLoad.Load, Interface, Point.Zero);
            loadDialog.SaveOrLoadClick += new EventHandler(loadDialog_LoadClick);
            loadDialog.CancelClick += new EventHandler(saveloadDialog_CancelClick);

        }

        void optionsDialog_OKClick(object sender, EventArgs e)
        {
            HandleChildWindowClose();
        }

        void optionsDialog_CancelClick(object sender, EventArgs e)
        {
            HandleChildWindowClose();
        }

        void saveloadDialog_CancelClick(object sender, EventArgs e)
        {
            HandleChildWindowClose();

        }

        private static void HandleChildWindowClose()
        {
            The.InGameUI.ShowInGameMenu();// show this dialog again so we stay modal.
        }

        /// <summary>
        /// this can be accessed both from main menu and in-game
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void loadDialog_LoadClick(object sender, EventArgs e)
        {
            LoadSavedGameWithPossibleRedirect(loadDialog.SelectedSaveGamePath);

        }

        /// <summary>
        /// only called when loading without preceding save in-game
        /// </summary>
        /// <param name="savegamePath"></param>
        private void LoadSavedGameWithPossibleRedirect(string savegamePath)
        {
            SnapshotHeader header;

            // read in the save header with the name of the scenario etc.

            FileStream stream = File.Open(savegamePath, FileMode.Open);
            GZipStream cmp = new GZipStream(stream, CompressionMode.Decompress);
            BufferedStream buffStrm = new BufferedStream(cmp, 65536);

            using (BinaryReader reader = new BinaryReader(buffStrm))
            {
                header = The.Snapshotter.LoadHeader(reader);
            }


            if (The.Sim.StartGameParams.IsSameScenario(header.StartGameParams))
            {
                // we can keep the game data in memory.
                // this will speed up loading.

                SaveLoadMessageBox.LoadGameDirectlyInGame(savegamePath, false);


                // run this in a thread:
               // The.Sim.LoadSavedGame(savegamePath); 

               // LoadGameDirectly(savegamePath);

            }
            else
            {
                // go back via the transitioning screen to unload content and data and load in all the new game data that is needed
                LoadSavedGameScreen.LoadSavedGameWithLoadingScreen(The.Sim.Controller, savegamePath, header);
            }

        }

        /**
        Thread thread;
        bool isSaving = false; // true during save+load
        bool saveThreadFinished = false;
        string savePath;
        TimeSpan saveStartedTimeStamp;
        */

        void saveDialog_SaveClick(object sender, EventArgs e)
        {
            /*
             start saving in a background thread. 
             * show a modal message box with no buttons
             * in Update, do a busy wait until the thread has finished
             * then continue with load... 
             */
            The.InGameUI.SaveLoadMessageBox.StartSave(saveDialog.SelectedSaveGamePath);

         /*   savePath = saveDialog.SelectedSaveGamePath;
             
            if (isSaving == false)
            {
                saveStartedTimeStamp = The.Sim.GameTime.TotalGameTime; // .TotalUnPausedGameTime;

                saveThreadFinished = false;

                thread = new System.Threading.Thread(SaveInThread);
                thread.IsBackground = true;
                thread.Start();
                isSaving = true;


                The.InGameUI.MessageBox.ShowMessage("Saving. Please wait...", true, MessageBox.ButtonOptions.None); // gets removed after load when client is recreated

            }

            */
          //  LoadGameDirectly(path);

        }

        public override void DrawContent(Window sender, Microsoft.Xna.Framework.Graphics.SpriteBatch formSpriteBatch)
        {
            base.DrawContent(sender, formSpriteBatch);
        }

      /*  public override void Update(GameTime elapsed)
        {            
            base.Update(elapsed);

            if (isSaving)
            {
                TimeSpan timeSinceSaveStarted = elapsed.TotalGameTime - saveStartedTimeStamp;
                The.InGameUI.MessageBox.Text = "Saving. Please wait... " + timeSinceSaveStarted.TotalSeconds.ToString();

                if (saveThreadFinished)
                {
                    if (thread != null
                        && thread.IsAlive)
                    {
                        if (Thread.CurrentThread != thread)
                        {
                            thread.Join();
                        }
                    }

                    LoadGameDirectly(savePath);

                    isSaving = false;
                }               
            }
        }
        */
       

        /*
        void saveDialog_SaveClick(object sender, EventArgs e)
        {
           
         /*   string path = saveDialog.SelectedSaveGamePath;
            The.Sim.DoSave(path);

            LoadGameDirectly(path);
          
        }*/

    /*    private void LoadGameDirectly(string path)
        {
            // follow a save by a load to make the same outcome whether the game continues now or is loaded later

            // run this in a thread:
            The.Sim.LoadSavedGame(path); 

            // who can call this:
            //The.Client.BeginRun(); // only here we may pull data from Sim, such as TileMap etc!
        }*/

       /* void Snapshotter_PreLoadPostProcess()
        {
            The.Client.Controller.RecreateClientAfterLoad();

            The.Snapshotter.PreLoadPostProcess -= new Action(Snapshotter_PreLoadPostProcess); // de-register..

        }*/
               

        private void InitButtons()
        {           
                       
          /*  Box brown = new Box(Interface.gui);
            Form.Add(brown);
            rect = Interface.gui.GUISpriteSheet.GetSourceRectangle("basic_darkblue"); //"basic_brown");
            brown.SetSkinLocation(SkinState.Normal,rect);
            brown.CornerSize = 3;
            brown.Position = new Point(MarginX, MarginY); // Form.Height - 40 - MarginY);//edges.Y + edges.Height + 10 + 70);
            brown.Width = Form.Width - 2 * MarginX;
            brown.Height = Form.Height - 2 * MarginY; // 200;
            */           
        }

        
        void btQuitToDesktop_Click(UIComponent sender, EventArgs e)
        {
            The.Sim.Controller.Game.Exit();
        }

        void btOptions_Click(UIComponent sender, EventArgs e)
        {
            The.InGameUI.HideInGameMenu(); //hide the InGameMenu temporarily, we will re-show it after the dialog closes, to stay modal
            
            optionsDialog.ShowDialog(true);
        }

        void btSaveGame_Click(UIComponent sender, EventArgs e)
        {            
            The.InGameUI.HideInGameMenu(); //hide the InGameMenu temporarily, we will re-show it after the dialog closes, to stay modal
            
            saveDialog.ShowDialog(true);  
        }

        void btLoadGame_Click(UIComponent sender, EventArgs e)
        {
            The.InGameUI.HideInGameMenu(); //hide the InGameMenu temporarily, we will re-show it after the dialog closes, to stay modal

            loadDialog.ShowDialog(true);
        }
 

        /// <summary>
        /// This uses the loading screen to
        /// transition from the game back to the main menu screen.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btQuitToMainMenu_Click(UIComponent sender, EventArgs e)
        {

            QuitToMainMenu();
        }


        public void QuitToMainMenu()
        {
            LoadingScreen.StartTransition(The.Sim.Controller, LoadMainMenuScreen, false);
        }
       

        /// <summary>
        /// Loading screen callback for activating the main menu screen,
        /// used when quitting from the game.
        /// </summary>
        void LoadMainMenuScreen(object sender, EventArgs e)
        {
            LoadingScreen loadingScreen = sender as LoadingScreen;
          
            foreach (GameScreen screen in loadingScreen.Controller.GetScreens())
                screen.ExitScreen();

            loadingScreen.Controller.AddScreen(new BackgroundScreen(BackgroundScreen.Background.Normal));
            loadingScreen.Controller.AddScreen(new MainMenuScreen(loadingScreen.Controller.Game));
        }


        void btContinueGame_Click(UIComponent sender, EventArgs e)
        {
            The.InGameUI.HideInGameMenu();
        }

        public override void ShowDialog(bool modal)
        {
            base.ShowDialog(modal);

        }

    }
}
