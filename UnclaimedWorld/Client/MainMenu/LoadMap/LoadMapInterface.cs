using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;
using InputEventSystem;
using UWGame.ClientSide.Interface;
using GameStateManagement;

namespace UWGame.ClientSide.MainMenu.LoadMap
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
    
    /// <summary>
    /// Interface for loading a map
    /// </summary>
    public class LoadMapInterface : CommonInterface
    {
        
        int loadPanelWidth = 600; 
        int totalHeight = 800;

        
       // int left, top;

       // private Tooltip tooltip;

        public MapEditorSaveLoadPanel loadPanel;

        public LoadMapScreen loadMapScreen;

        public LoadMapInterface(LoadMapScreen createGameScreen, UnclaimedWorld game)
            : base(game, addTooltip: true)
        {

            this.loadMapScreen = createGameScreen;

            game.Controller.ValidateDrawAreaWidth(loadPanelWidth);
            /*
            if (game.Controller.DrawArea.Width < loadPanelWidth) //game.GraphicsDeviceManager.PreferredBackBufferWidth < loadPanelWidth)
            {
                throw new Exception("Screen resolution is too low, or the zoom factor is too high.");
            }*/

            // center the panels:
           /* left = (game.GraphicsDeviceManager.PreferredBackBufferWidth - loadPanelWidth) / 2;
            top = (game.GraphicsDeviceManager.PreferredBackBufferHeight - totalHeight) / 2;
            */

            //frameLeft = left + loadPanelWidth + EntityPanelWidth - framedCRTWidth; //left + 320; 
            //frameTop = top;

           // entityPanelTop = top + 8;
           // entityPanelLeft = left + loadPanelWidth + 6;

        /*    Rectangle directSource = new Rectangle(frameLeft, frameTop, framedCRTWidth, framedCRTHeight);

            framedCRT = new FramedCRT(this, new Rectangle(frameLeft, frameTop, framedCRTWidth, framedCRTHeight),
                                                directSource, Level.Middle);

               */ 
         
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            //tooltip.Update(gameTime);

        }

        public override void Destroy()
        {
            base.Destroy();

           // tooltip.Destroy();
        }

        public override void LoadContent()
        {
            base.LoadContent();

        //    DisplayPanelRenderer.LoadContent();

            // CRT must be created after crt effect textures (scanlines etc.) have loaded:
         //   Rectangle directSource = new Rectangle(entityPanelLeft, entityPanelTop, framedCRTWidth, framedCRTHeight);
            /*framedCRT = new FramedCRT(this, new Rectangle(entityPanelLeft, entityPanelTop, framedCRTWidth, framedCRTHeight),
                                                directSource, Level.Middle);
            framedCRT.ShowCables = false;*/
                      

            loadPanel = new MapEditorSaveLoadPanel(MapEditorSaveLoadPanel.SaveOrLoad.Load, this, new Point(0, 0));

            // center the panels:
            loadPanel.Window.X = (base.Game.Controller.DrawArea.Width - loadPanel.Window.Width) / 2;
            loadPanel.Window.Y = (base.Game.Controller.DrawArea.Height - loadPanel.Window.Height) / 2;
          
         /*   loadPanel.Window.X = (base.Game.GraphicsDeviceManager.PreferredBackBufferWidth - loadPanel.Window.Width) / 2;
            loadPanel.Window.Y = (base.Game.GraphicsDeviceManager.PreferredBackBufferHeight - loadPanel.Window.Height) / 2;
            */


            loadPanel.SaveOrLoadClick += new EventHandler(loadPanel_SaveOrLoadClick);

            loadPanel.CancelClick += new EventHandler(loadPanel_CancelClick);

            loadPanel.ShowDialog(true);
            //loadPanel.Show();

            SetInterfaceCursor();

          //  framedCRT.Show();

        }

        void loadPanel_CancelClick(object sender, EventArgs e)
        {
            loadMapScreen.ExitToMainMenu();
        }

        void loadPanel_SaveOrLoadClick(object sender, EventArgs e)
        {
            loadMapScreen.StartMapEditor(loadPanel.SelectedMapFolder); // MapDataXmlPath);

        }


       



    }
}


