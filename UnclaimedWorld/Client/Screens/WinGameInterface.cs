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
    
    /// <summary>
    /// Interface for winning the game
    /// </summary>
    public class WinGameInterface : CommonInterface
    {

    //    public FramedCRT framedCRT;
    //    int framedCRTWidth = (int)((4f / 3f) * 300f);
   //     int framedCRTHeight = 300;

        int loadPanelWidth = 600; //730;
        int totalHeight = 800;

        
        int left, top;

        
        //   IntroPanel introPanel;

        
       // public Interface.EntityPanel.EntityPanel EntityPanel;
        public WinGamePanel Panel;


       // public Dictionary<string, ModelData> AllModels = new Dictionary<string, ModelData>();

        public WinGameScreen winGameScreen;

        public WinGameInterface(WinGameScreen winGameScreen, UnclaimedWorld game)
            : base(game)
        {
            
            this.winGameScreen = winGameScreen;

          /*  if (game.GraphicsDeviceManager.PreferredBackBufferWidth < loadPanelWidth)
            {
                throw new Exception("Screen resolution is too low.");
            }*/
            game.Controller.ValidateDrawAreaWidth(loadPanelWidth);
            
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

         //   DisplayPanelRenderer.LoadContent();

            // CRT must be created after crt effect textures (scanlines etc.) have loaded:
         //   Rectangle directSource = new Rectangle(entityPanelLeft, entityPanelTop, framedCRTWidth, framedCRTHeight);
            /*framedCRT = new FramedCRT(this, new Rectangle(entityPanelLeft, entityPanelTop, framedCRTWidth, framedCRTHeight),
                                                directSource, Level.Middle);
            framedCRT.ShowCables = false;*/
                      

            Panel = new WinGamePanel(this, new Point(left, top));

           
            Panel.CancelClick += new EventHandler(loadPanel_CancelClick);

            Panel.Show();
            

            SetInterfaceCursor();


          //  framedCRT.Show();

        }

        void loadPanel_CancelClick(object sender, EventArgs e)
        {
            winGameScreen.ExitToMainMenu();
        }

        void loadPanel_SaveOrLoadClick(object sender, EventArgs e)
        {
          //  winGameScreen.StartMapEditor(loadPanel.MapDataXmlPath);
        }


       

    }
}


