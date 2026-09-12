using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;
using InputEventSystem;
using UWGame.ClientSide.Interface;
using GameStateManagement;

namespace UWGame.ClientSide.Screens
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Control;
    using Microsoft.Xna.Framework;
    using WindowSystem;
    using UWGame.ClientSide.Screens;
    
    /// <summary>
    /// Interface for losing the game
    /// </summary>
    public class LoseGameInterface : CommonInterface
    {


        int loadPanelWidth = 600; 
        int totalHeight = 800;

        
        int left, top;

        public LoseGamePanel Panel;

        public LoseGameScreen loseGameScreen;

        public LoseGameInterface(LoseGameScreen loseGameScreen,  UnclaimedWorld game)
            : base(game)
        {

            this.loseGameScreen = loseGameScreen;

            game.Controller.ValidateDrawAreaWidth(loadPanelWidth);
            /*
            if (game.GraphicsDeviceManager.PreferredBackBufferWidth < loadPanelWidth)
            {
                throw new Exception("Screen resolution is too low.");
            }*/

            // center the panels:
            left = (game.Controller.DrawArea.Width - loadPanelWidth) / 2;
            top = (game.Controller.DrawArea.Height - totalHeight) / 2;


            SetInterfaceCursor();
        }

        public override void LoadContent()
        {
            base.LoadContent();

         //   DisplayPanelRenderer.LoadContent();


            Panel = new LoseGamePanel(this, new Point(left, top));


            Panel.CancelClick += new EventHandler(loadPanel_CancelClick);

            Panel.Show();

            SetInterfaceCursor();


        }

        void loadPanel_CancelClick(object sender, EventArgs e)
        {
            loseGameScreen.ExitToMainMenu();
        }

       
      
    }
}


