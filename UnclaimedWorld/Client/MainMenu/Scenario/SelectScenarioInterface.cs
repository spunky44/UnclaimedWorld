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
    public class SelectScenarioInterface : CommonInterface
    {              

        private SelectScenarioPanel panel;

        public SelectScenarioScreen Screen;

        public Tooltip Tooltip;



        public SelectScenarioInterface(SelectScenarioScreen loadReplayScreen, UnclaimedWorld game)
            : base(game)
        {

            this.Screen = loadReplayScreen;
                             
        }

        public override void LoadContent()
        {
            base.LoadContent();

         //   DisplayPanelRenderer.LoadContent();

            panel = new SelectScenarioPanel(this, new Point(0, 0));

            panel.Window.CenterWindow();

            panel.CancelClick += new EventHandler(loadPanel_CancelClick);

            panel.ShowDialog(true); // panel.Show();

            Tooltip = new Tooltip(this);

            SetInterfaceCursor();
        }

        void loadPanel_CancelClick(object sender, EventArgs e)
        {
            Screen.ExitToMainMenu();
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
    }
}
