using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using WindowSystem;
using UWGame.SimSide.Overland;
using UWGame.ClientSide.Interface.LCD;
using UWGame.ClientSide.Interface.Controls;
using UWGame.SimSide.Entities;

namespace UWGame.ClientSide.Interface.World_map
{
    public class WorldMapDialog : Panel
    {
        LCDScreen lcdScreen;
        UIComponent lcdSurface;
        Box display;
        Label lblInfo;
       public WorldMap worldMap;
      
        public event EventHandler OKClick;
        public event EventHandler CancelClick;

        //public Site SelectedSite;

        public TravelLocation? SelectedTravelLocation;

        ModalOverlay modalOverlay;

        public WorldMapDialog(CommonInterface intf, Point position) :
            base(intf, "WORLD MAP", position, new Vector2(590, Math.Min(617/*590*/, The.InGameUI.rosterPanelHeight)), Level.Dialogs)//new Vector2(625, Math.Min(793, The.InGameUI.rosterPanelHeight)), Level.Dialogs)
        {
            RosterPanel.CreateRosterStyleLCDPanel(intf, Window,
                out display, out lcdSurface, ref lcdScreen);

            lcdSurface.DebugTag = "worldmapDlgLcdSurface";

            modalOverlay = new ModalOverlay(Window);

            worldMap = new WorldMap(lcdSurface,
               525, 440);// 562, 594);//646); // base.Form);

            worldMap.TerminalSelected += worldMap_TerminalSelected;

            worldMap.ChildDialogDisplayed += worldMap_ChildDialogDisplayed;
            worldMap.ChildDialogClosed += worldMap_ChildDialogClosed;
            /*
            siteWindow = new World_map.SiteWindow(Interface.gui,330,150);
            lcdSurface.Add(siteWindow);
            siteWindow.Hide();
          */

            LCDInnerPanel lcdErrorMessagePanel = new LCDInnerPanel(Interface.gui, lcdSurface.Width, true, 1f);
            lcdSurface.Add(lcdErrorMessagePanel.Panel);
            // surfaceGrid.AddEntry(selectionPanel.Panel, selectionPanel.Panel);
            lcdErrorMessagePanel.ContentHeight = 30;
            lcdErrorMessagePanel.Panel.Y = lcdSurface.Height - 35; // 24; 
            lcdErrorMessagePanel.VerticalContentPadding = 8;

            lblInfo = new Label(Interface.gui);
            lcdSurface.Add(lblInfo);
            lblInfo.Init(Label.LabelType.LCDNormal);
            lblInfo.Y = lcdErrorMessagePanel.Panel.Y + 10;
            lblInfo.X = 10;

            TextButton btCancel = AddLowerButton("CANCEL", "Cancels and closes the dialog.", Align.Right);
            btCancel.Click += new ClickHandler(btCancel_Click);
        }

        public void ShowModalOverlay()
        {
            modalOverlay.Show(Window, lcdSurface, display);
        }

        public void RemoveModalOverlay()
        {
            modalOverlay.Remove(Window, lcdSurface, display);
        }

        void worldMap_ChildDialogClosed()
        {
            RemoveModalOverlay();
        }

        void worldMap_ChildDialogDisplayed()
        {
            ShowModalOverlay();
        }

        void worldMap_TerminalSelected(TravelLocation travelLocation)
        {
            bool alreadySelected;
            bool wrongTerminalType;
            bool acceptLocation = The.InGameUI.CreateMissionPanel.CanSelectLocation(travelLocation, out alreadySelected, out wrongTerminalType);

            if (acceptLocation)
            {
                //Hide();
                SelectedTravelLocation = travelLocation;

                AcceptAndClose();
            }   
        }

       

        void btCancel_Click(UIComponent sender, EventArgs e)
        {
            Window.Hide();

            if (CancelClick != null)
                CancelClick.Invoke(this, null);
        }

        public void btOK_Click(UIComponent sender, EventArgs e)
        {
            AcceptAndClose();
        }

        private void AcceptAndClose()
        {
            if (this.OKClick != null)
                OKClick.Invoke(this, null);

            Hide();
        }

        public void UnCheckSiteMarkerButtons()
        {
            worldMap.UnCheckSiteMarkerButtons();
        }

        public override void Refresh()
        {
            base.Refresh();

           worldMap.Update();

        }

        public void Fill(EntityGroupID? buyer)
        {
            if (The.InGameUI.CreateMissionPanel.worldMapDialogSource == UWGame.ClientSide.Interface.Missions.CreateMissionPanel.WorldMapDialogSource.Start)
            {
                lblInfo.Text = "Select a starting location for the mission!";
            }
            else
            {
                lblInfo.Text = "Select a destination for the mission!";
            }

            worldMap.Fill(The.Sim.World, buyer, true);          
        }


    }
}
