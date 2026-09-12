using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.ClientSide.Interface;
using GameStateManagement;
using Microsoft.Xna.Framework;
using InputEventSystem;

namespace UWGame.Client.MainMenu.LoadSavedGame
{
    class LoadSavedGameInterface : CommonInterface
    {
                
        int loadPanelWidth = 600; 
        int totalHeight = 800;

        
        int left, top;


        public SaveLoadGamePanel loadPanel;

        public LoadSavedGameScreen loadGameScreen;

        public LoadSavedGameInterface(LoadSavedGameScreen loadGameScreen, UnclaimedWorld game)
            : base(game)
        {

            this.loadGameScreen = loadGameScreen;

            game.Controller.ValidateDrawAreaWidth(loadPanelWidth);
            /*
            if (game.GraphicsDeviceManager.PreferredBackBufferWidth < loadPanelWidth)
            {
                throw new Exception("Screen resolution is too low.");
            }*/

            // center the panels:
            left = (game.Controller.DrawArea.Width - loadPanelWidth) / 2;
            top = (game.Controller.DrawArea.Height - totalHeight) / 2;         
        }

        public override void LoadContent()
        {
            base.LoadContent();

            loadPanel = new SaveLoadGamePanel(SaveLoadGamePanel.SaveOrLoad.Load, this, new Point(left, top));         

            loadPanel.CancelClick += new EventHandler(loadPanel_CancelClick);
            loadPanel.SaveOrLoadClick += new EventHandler(loadPanel_LoadClick);

            loadPanel.ShowDialog(true);
          
            SetInterfaceCursor();
        }

        void loadPanel_LoadClick(object sender, EventArgs e)
        {
            loadGameScreen.LoadSavedGameWithLoadingScreen(loadPanel.SelectedSaveGamePath);
        }

        void loadPanel_CancelClick(object sender, EventArgs e)
        {
            loadGameScreen.ExitToMainMenu();
        }

        
    }
}
