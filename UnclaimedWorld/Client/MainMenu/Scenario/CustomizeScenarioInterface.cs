using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.ClientSide.Interface;
using GameStateManagement;
using Microsoft.Xna.Framework;
using InputEventSystem;
using UWGame.ClientSide.Interface.HUD_Windows;

namespace UWGame.ClientSide.MainMenu.Scenario
{
    class CustomizeScenarioInterface : CommonInterface
    {
               

        private CustomizeScenarioPanel panel;

        public CustomizeScenarioScreen Screen;

        public Tooltip Tooltip;

        public CustomizeScenarioInterface(CustomizeScenarioScreen screen, UnclaimedWorld game)
            : base(game)
        {
            this.Screen = screen;

           

        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime); 

            Tooltip.Update(gameTime);

        }

        public override void Destroy()
        {
            base.Destroy();

            Tooltip.Destroy();
        }

        public override void LoadContent()
        {
            base.LoadContent();

         //   DisplayPanelRenderer.LoadContent();

            panel = new CustomizeScenarioPanel(this, new Point(0, 0), Screen);
            panel.Window.CenterWindow();
            panel.CancelClick += new EventHandler(loadPanel_CancelClick);
            panel.Show();

            SetInterfaceCursor();

            Tooltip = new Tooltip(this);
        }

        void loadPanel_CancelClick(object sender, EventArgs e)
        {
            Screen.ExitToMainMenu();
        }

       
    }
}
