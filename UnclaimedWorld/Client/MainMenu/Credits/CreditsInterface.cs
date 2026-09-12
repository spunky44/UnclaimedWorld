using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;
using InputEventSystem;
using UWGame.ClientSide.Interface;
using GameStateManagement;

namespace UWGame.ClientSide.MainMenu.Credits
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Control;
    using Microsoft.Xna.Framework;
    using WindowSystem;
    using ClientSide.Interface.EntityPanel;
    using UWGame.ClientSide.Interface.HUD_Windows;
    
    public class CreditsInterface : CommonInterface
    {

    //    public FramedCRT framedCRT;
    //    int framedCRTWidth = (int)((4f / 3f) * 300f);
   //     int framedCRTHeight = 300;

        int loadPanelWidth = 600; //730;
        int totalHeight = 800;

        
        int left, top;

        

        
//        public MapEditorSaveLoadPanel loadPanel;

        CreditsPanel hud;

       // public Dictionary<string, ModelData> AllModels = new Dictionary<string, ModelData>();

        public CreditsScreen creditsScreen;

        public CreditsInterface(CreditsScreen createGameScreen, UnclaimedWorld game)
            : base(game)
        {

            this.creditsScreen = createGameScreen;

            game.Controller.ValidateDrawAreaWidth(loadPanelWidth);
            /*
            if (game.Controller.DrawArea.Width < loadPanelWidth) // game.GraphicsDeviceManager.PreferredBackBufferWidth < loadPanelWidth)
            {
                throw new Exception("Screen resolution is too low.");
            }*/

            // center the panels:
            left = (game.Controller.DrawArea.Width - loadPanelWidth) / 2;
            top = (game.Controller.DrawArea.Height - totalHeight) / 2;


            //frameLeft = left + loadPanelWidth + EntityPanelWidth - framedCRTWidth; //left + 320; 
            //frameTop = top;

           // entityPanelTop = top + 8;
           // entityPanelLeft = left + loadPanelWidth + 6;

        /*    Rectangle directSource = new Rectangle(frameLeft, frameTop, framedCRTWidth, framedCRTHeight);

            framedCRT = new FramedCRT(this, new Rectangle(frameLeft, frameTop, framedCRTWidth, framedCRTHeight),
                                                directSource, Level.Middle);

               */ 
         
        }

        public override void LoadContent()
        {
            base.LoadContent();

          //  DisplayPanelRenderer.LoadContent();

            // CRT must be created after crt effect textures (scanlines etc.) have loaded:
         //   Rectangle directSource = new Rectangle(entityPanelLeft, entityPanelTop, framedCRTWidth, framedCRTHeight);
            /*framedCRT = new FramedCRT(this, new Rectangle(entityPanelLeft, entityPanelTop, framedCRTWidth, framedCRTHeight),
                                                directSource, Level.Middle);
            framedCRT.ShowCables = false;*/

            hud = new CreditsPanel(this);

            hud.DisplayWindow.Close += new CloseHandler(DisplayWindow_Close);

           
          
            SetInterfaceCursor();

          //  framedCRT.Show();

        }

        void DisplayWindow_Close(UIComponent sender)
        {
            creditsScreen.ExitToMainMenu();
        }

      
      

       

    }
}


