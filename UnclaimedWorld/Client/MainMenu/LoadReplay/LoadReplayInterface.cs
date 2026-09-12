using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.ClientSide.Interface;
using GameStateManagement;
using Microsoft.Xna.Framework;
using InputEventSystem;

namespace UWGame.Client.MainMenu.LoadReplay
{
    class LoadReplayInterface : CommonInterface
    {
                
        int loadPanelWidth = 600; 
        int totalHeight = 800;

        
        int left, top;


        public LoadReplayPanel loadPanel;

        public LoadReplayScreen loadReplayScreen;

        public LoadReplayInterface(LoadReplayScreen loadReplayScreen,  UnclaimedWorld game)
            : base(game)
        {

            this.loadReplayScreen = loadReplayScreen;

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

           // DisplayPanelRenderer.LoadContent();

            loadPanel = new LoadReplayPanel(this , new Point(left, top));

            loadPanel.CancelClick += new EventHandler(loadPanel_CancelClick);
            loadPanel.LoadClick += new EventHandler(loadPanel_LoadClick);

            loadPanel.Show();

            SetInterfaceCursor();
        }

        void loadPanel_LoadClick(object sender, EventArgs e)
        {
            loadReplayScreen.LoadReplay(
                loadPanel.SelectedLoadReplayPath,
                loadPanel.TimeToPauseReplay);

            
        }

        void loadPanel_CancelClick(object sender, EventArgs e)
        {
            loadReplayScreen.ExitToMainMenu();
        }

        
    }
}
