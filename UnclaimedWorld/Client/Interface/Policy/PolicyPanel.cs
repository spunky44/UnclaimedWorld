using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;
using UWGame.ClientSide.Interface.LCD;
using UWGame.SimSide.Overland.Missions.Templates;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Snapshots;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Overland;
using UWGame.SimSide.AI;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide;
using UWGame.SimSide.Tiers;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Commands;
using UWGame.SimSide.InGameEvents.SpecialEvents;

namespace UWGame.ClientSide.Interface.Policy
{
   
    public class PolicyPanel: RosterPanel
    {
      //  Grid surfaceGrid; // make this the part of the Tab

       
       // UIComponent tierPanel; // make this a Tab Item
        TiersPage tiersPage;

        WeaponsPage weaponsPage;
       

        TabControl tab;

        public PolicyPanel()
            : base("POLICY", 622, false)
        {
            GUIManager gui = Interface.gui;

            tab = new TabControl(gui, lcdSurface);

            tiersPage = new TiersPage(tab);
            weaponsPage = new WeaponsPage(tab);

            tab.NewPageSelected += tab_NewPageSelected;
        }

        void tab_NewPageSelected(TabPage obj)
        {
            ((TabPagePanel)obj).Refresh();
        }

        /*
        public PolicyPanel()
            : base("POLICY", 622, false)
        {
            GUIManager gui = Interface.gui;
            CreateSurfaceWithScrollbar(out surfaceGrid, lcdSurface, false);

            tierPanel = new UIComponent(gui);
            tierPanel.Width = surfaceGrid.Width;
           
            surfaceGrid.AddEntry(tierPanel, tierPanel);

            tierHeaderContainer = new UIComponent(gui);
            tierHeaderContainer.Width = tierPanel.Width; // -tierStartX;
            tierHeaderContainer.Height = 42;

            tierPanel.Add(tierHeaderContainer);
           // tierHeaderContainer.X = tierStartX;

         
            grdTiers = FullLCDPanel.AddGridWithFixedItemHeights(lcdSurface.guiManager, tierPanel, tierHeaderContainer.Height, 0);
            grdTiers.ItemHeight = tierHeight;
            grdTiers.Selectability = Grid.SelectabilityOptions.None;
            grdTiers.CanGrowInHeight = true;
            grdTiers.ScrollBarEnabled = false;
            grdTiers.RowSpacing = 6;

            PopulateStartingTiers();

        }*/


      
               
      
        public void Show()
        {
            ((TabPagePanel)tab.DisplayedTabPage).Refresh();

           // Populate();
        }


        public override void Refresh()
        {
            ((TabPagePanel)tab.DisplayedTabPage).Refresh();

           // Populate();

            base.Refresh();
        }

    }
}
