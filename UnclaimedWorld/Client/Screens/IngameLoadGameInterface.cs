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
    using UWGame.ClientSide.Interface.HUD_Windows;
    using UWGame.SimSide.Scenarios;
    
    public class IngameLoadGameInterface : CommonInterface
    {
        /*int loadPanelWidth = 600; //730;
        int totalHeight = 800;
        
        
        int left, top;*/

       // public IngameLoadGameScreen loadingScreen;

        public SaveLoadMessageBox SaveLoadMessageBox;

        public IngameLoadGameInterface(IngameLoadGameScreen screen, UnclaimedWorld game)
            : base(game)
        {

           // this.loadingScreen = screen;
           

            // center the panels:
         /*   left = (game.GraphicsDeviceManager.PreferredBackBufferWidth - loadPanelWidth) / 2;
            top = (game.GraphicsDeviceManager.PreferredBackBufferHeight - totalHeight) / 2;
               */      
        }

        public override void LoadContent()
        {
            base.LoadContent();

            SaveLoadMessageBox = new Interface.SaveLoadMessageBox(this);
            SaveLoadMessageBox.ShowMessage("Loading game, please wait...", null, true, Interface.MessageBox.ButtonOptions.None);

            SetInterfaceCursor();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (SaveLoadMessageBox.Window.IsVisibleAndActive)
            {
                SaveLoadMessageBox.Update(gameTime);              
            }
        }

       
        void Form_Close(UIComponent sender)
        {
        }

      

     /*   public override void Update(GameTime gameTime)
        {
            //input.Update(gameTime);

        }*/



    }
}


