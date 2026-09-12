using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;
using UWGame.SimSide;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.HelpTopics;

namespace UWGame.ClientSide.Interface.HUD_Windows
{
    public class HUDHelpPanel: HUDWindow 
    {
       // TextButton tbSalvage, /*tbGather,*/ tbHunt, tbScout, tbExpedition;

        Grid grid;

        public HUDHelpPanel()
            : base(204, 230 , true, false, false, "HUD_window_base", true, Level.Menu)
        {
            CreateMenuGrid(out grid);
            grid.Selectability = Grid.SelectabilityOptions.Single;

            Populate();
          
            grid.SelectedChanged += new SelectionChangedHandler(grid_SelectedChanged);
        }

        void grid_SelectedChanged(UIComponent sender)
        {
            HelpTopic selectedTopic = null;

            Grid clickedGrid = sender as Grid;

            if (clickedGrid != null)
            {
                object key;
                if (clickedGrid.GetKey(clickedGrid.SelectedItem, out key))
                {
                    selectedTopic = (HelpTopic)key;
                }
            }


            HelpTopicDialog dialog = The.InGameUI.HelpTopicDialogs[selectedTopic];
            
          //  dialog.ShowText(selectedTopic.Text);

            dialog.ShowInScreenSpace(0, 200);
            dialog.DisplayWindow.CenterWindow();
        }

     
        public void Populate()
        {                     

            grid.BeginAddingEntries();


            UIComponent row;
            foreach (var item in GameData.Instance.AllHelpTopics)
            {
                row = grid.AddEntry(item.Value, item.Value.Name.ToUpper(Config.Culture));
                row.OrderByTag1 = item.Value.Name;
            }

            grid.Sort(t => t.OrderByTag1, Grid.Sorting.Ascending);

            grid.EndAddingEntries();
            
        }

        


        public override void Hide()
        {
            base.Hide();

            if (The.Sim.Mode != Sim.EngineMode.Edit)
            {
                The.InGameUI.MainPanel.btHelp.IsChecked = false;

            }
        }
       

    }
}
