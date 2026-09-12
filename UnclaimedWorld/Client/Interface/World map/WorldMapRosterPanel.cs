using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;

namespace UWGame.ClientSide.Interface.World_map
{
    public class WorldMapRosterPanel : RosterPanel
    {
        const int windowWidth = 600;

        WorldMap worldMap;

        public WorldMapRosterPanel()
            : base("WORLD MAP",
            windowWidth, Math.Min(584, The.InGameUI.rosterPanelHeight), false,
            panelType: PanelType.RosterPanel) // to avoid using the event archive ctor...
        {

            worldMap = new WorldMap(lcdSurface,
              525, 440);// 562, 594);//646); // base.Form);

            worldMap.ChildDialogDisplayed += worldMap_ChildDialogDisplayed;
            worldMap.ChildDialogClosed += worldMap_ChildDialogClosed;
        }

        public override void Hide()
        {
            base.Hide();

            worldMap.HideOpenDialogs();          
        }

        void worldMap_ChildDialogClosed()
        {
            RemoveModalOverlay();           
        }

        void worldMap_ChildDialogDisplayed()
        {
            ShowModalOverlay();
        }

        public override void Refresh()
        {
            base.Refresh();

            worldMap.Update();

        }

        public override void Show()
        {
            EntityGroupID? otherParty = The.InGameUI.UIOwner;
            worldMap.Fill(The.Sim.World, otherParty, false); 

            base.Show();
  
        }
    }
}
