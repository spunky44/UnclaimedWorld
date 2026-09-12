using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;
using UWGame.ClientSide.Interface;
using Microsoft.Xna.Framework;
using UWGame.SimSide;
using Microsoft.Xna.Framework.Input;
using UWGame.ClientSide.GameEvents;

namespace UWGame.ClientSide.Interface
{
    /// <summary>
    /// this panel is placed at the top right, below the CRT screen
    /// </summary>
    public class RosterAccessPanel
    {
        Window window;
     

        protected InGameInterface intf = The.InGameUI;

        const int height = 500; 
        public const int Width = 51;

        const int buttonLeft = 12;

        /// <summary>
        /// game mode
        /// </summary>
        public ImageButton btStock, btJobs, btEventDialogs, btGraphs, btLedger, btWorld, btDiplomacy, btMissions, btZone, btEntity, btPersonnel, btPolicy;

        /// <summary>
        /// editor mode
        /// </summary>
        public ImageButton btEditorPlaceEntity;
        public ImageButton btEditorPaintTile, btEditorTerrainHeight;

        public RosterAccessPanel()
        {

            window = new Window(intf.gui);
           // window.Position = new Point(The.Client.GraphicsDevice.Viewport.Width - Width, 254); 
            window.Position = new Point(The.Client.Controller.DrawArea.Width - Width, 254); 
            window.WindowSize = new Vector2(Width, height); //Interface.Instance.mainPanelHeight);
            window.Level = Level.BelowBelowBelowMiddle; // .Bottom;
            window.IsMovable = false;
            window.Resizable = false;
            window.Margin = 0;
            window.HasCloseButton = false;
            window.Skin = intf.gui.GUISpriteSheet.GetSourceRectangle("sidebar_base");
            window.CornerSize = 25; // 15;
            window.Show(); //Make it visible
            
            int buttonTop = 6;

            int buttonXSpacing = 6, buttonYSpacing = 4;

            if (The.Sim.Mode == Sim.EngineMode.Game)
            {
                
                Image bg = Panel.AddImage(The.InGameUI.gui, window, "infoswitch_bg", new Point(12, 20));

                btEntity = new ImageButton(The.InGameUI.gui);
                window.Add(btEntity); // add first!!! sets defaults!
                btEntity.Init(ImageButtonType.EntityInfo);
                btEntity.Position = new Point(bg.X + 7, bg.Y + 24);
                btEntity.ToolTip = "Show details about the selected entity";
                btEntity.Click += new ClickHandler(btEntity_Click);
                The.InGameUI.SidePanelEntity.AccessButton = btEntity;
                The.InGameUI.SelectedEntityChangedEvent += new InGameInterface.SelectedEntityChanged(InGameUI_SelectedEntityChangedEvent);
                btEntity.Enabled = false;

                btZone = new ImageButton(The.InGameUI.gui);
                window.Add(btZone); // add first!!! sets defaults!
                btZone.Init(ImageButtonType.MapAreaInfo);
                btZone.Position = new Point(btEntity.X, btEntity.Bottom + 2);
                btZone.ToolTip = "Show details about the selected zone";
                btZone.Click += new ClickHandler(btZone_Click);
                The.InGameUI.SidePanelMapArea.AccessButton = btZone;
                //The.InGameUI.SelectedZone

                btStock = new ImageButton(intf.gui);
                btStock.Init(ImageButtonType.InventoryButton);
                btStock.Position = new Point(buttonLeft, bg.Bottom + 28);
                window.Add(btStock);
                btStock.Click += new ClickHandler(stock_Click);
                The.InGameUI.InventoryPanel.AccessButton = btStock;
                btStock.ToolTip = "View the inventory and give production orders";
                             

                btJobs = new ImageButton(intf.gui);
                btJobs.Init(ImageButtonType.TasksButton);
                btJobs.Position = new Point(buttonLeft, btStock.Bottom + buttonYSpacing);
                window.Add(btJobs);
                btJobs.Click += new ClickHandler(tbJobs_Click);
                The.InGameUI.JobsPanel.AccessButton = btJobs;
                btJobs.ToolTip = "View current tasks and set priorities";

              

                btEventDialogs = new ImageButton(intf.gui);
                btEventDialogs.Init(ImageButtonType.EventDialogsButton);
                btEventDialogs.Position = new Point(buttonLeft, btJobs.Bottom + buttonYSpacing);
                window.Add(btEventDialogs);
                btEventDialogs.Click += new ClickHandler(tbEventDialogs_Click);
                The.InGameUI.EventArchivePanel.AccessButton = btEventDialogs;
                btEventDialogs.ToolTip = "Display event archive";

                btGraphs = new ImageButton(intf.gui);
                btGraphs.Init(ImageButtonType.GraphButton);
                btGraphs.Position = new Point(buttonLeft, btEventDialogs.Bottom + buttonYSpacing);
                window.Add(btGraphs);
                btGraphs.Click += new ClickHandler(tbGraphs_Click);
                The.InGameUI.GraphPanel.AccessButton = btGraphs;
                btGraphs.ToolTip = "Show graphs";

                btLedger = new ImageButton(intf.gui);
                btLedger.Init(ImageButtonType.LedgerButton);
                btLedger.Position = new Point(buttonLeft, btGraphs.Bottom + buttonYSpacing);
                window.Add(btLedger);
                btLedger.Click += btLedger_Click;
                The.InGameUI.LedgerPanel.AccessButton = btLedger;
                btLedger.ToolTip = "Show ledger";

                btWorld = new ImageButton(intf.gui);
                btWorld.Init(ImageButtonType.DiplomacyButton);
                btWorld.Position = new Point(buttonLeft, btLedger.Bottom + buttonYSpacing);
                window.Add(btWorld);
                btWorld.Click += new ClickHandler(btWorld_Click);
                The.InGameUI.WorldMapPanel.AccessButton = btWorld;
                btWorld.ToolTip = "View the world map";
                            

             /*   btDiplomacy = new ImageButton(intf.gui);
                btDiplomacy.Init(ImageButtonType.DiplomacyButton);
                btDiplomacy.Position = new Point(buttonLeft, btWorld.Bottom + buttonYSpacing);
                window.Add(btDiplomacy);
                btDiplomacy.Click += new ClickHandler(tbDiplomacy_Click);
                The.InGameUI.DiplomacyPanel.AccessButton = btDiplomacy;
                btDiplomacy.ToolTip = "View other allegiances and trade with them";*/
                                
                btMissions = new ImageButton(intf.gui);
                btMissions.Init(ImageButtonType.MissionsButton);
                btMissions.Position = new Point(buttonLeft,btWorld.Bottom + buttonYSpacing);// btDiplomacy.Bottom + buttonYSpacing);
                window.Add(btMissions);
                btMissions.Click += new ClickHandler(tbMissions_Click);
                The.InGameUI.MissionsPanel.AccessButton = btMissions;             
                btMissions.ToolTip = "Arrange an off-map trade mission and transport";

                btPersonnel = new ImageButton(intf.gui);
                btPersonnel.Init(ImageButtonType.PersonnelButton);
                btPersonnel.Position = new Point(buttonLeft, btMissions.Bottom + buttonYSpacing);
                window.Add(btPersonnel);
                btPersonnel.Click += new ClickHandler(btPersonnel_Click);
                The.InGameUI.PersonnelRosterPanel.AccessButton = btPersonnel;
                btPersonnel.ToolTip = "View the list of colony members";

                btPolicy = new ImageButton(intf.gui);
                btPolicy.Init(ImageButtonType.PolicyButton);
                btPolicy.Position = new Point(buttonLeft, btPersonnel.Bottom + buttonYSpacing);
                window.Add(btPolicy);
                btPolicy.Click += new ClickHandler(tbPolicy_Click);
                The.InGameUI.PolicyPanel.AccessButton = btPolicy;
                btPolicy.ToolTip = "View or change the colony's policies";

            }
            else
            {
              /*  tbEditor = AddLightBoxButton(buttonLeft, buttonTop, "EDITOR", 3);
                The.InGameUI.SidePanelEdit.MainControlButton = tbEditor;
                tbEditor.Click += new ClickHandler(editor_Click);*/
             //   buttonGroup.Add(tbEditor);

                btEditorPlaceEntity = new ImageButton(intf.gui);
                btEditorPlaceEntity.Init(ImageButtonType.InventoryButton);
                btEditorPlaceEntity.Position = new Point(buttonLeft, 20);
                window.Add(btEditorPlaceEntity);
                btEditorPlaceEntity.Click += new ClickHandler(editorPlaceEntity_Click);
                The.InGameUI.SidePanelEditorEntity.AccessButton = btEditorPlaceEntity;
                btEditorPlaceEntity.ToolTip = "Place map assets";


                btEditorPaintTile = new ImageButton(intf.gui);
                btEditorPaintTile.Init(ImageButtonType.DiplomacyButton);
                btEditorPaintTile.Position = new Point(buttonLeft, btEditorPlaceEntity.Bottom + buttonYSpacing);
                window.Add(btEditorPaintTile);
                btEditorPaintTile.Click += btEditorPaintTile_Click;
                The.InGameUI.SidePanelEditorSoil.AccessButton = btEditorPaintTile;
                btEditorPaintTile.ToolTip = "Paint terrain properties";

                btEditorTerrainHeight = new ImageButton(intf.gui);
                btEditorTerrainHeight.Init(ImageButtonType.PolicyButton);
                btEditorTerrainHeight.Position = new Point(buttonLeft, btEditorPaintTile.Bottom + buttonYSpacing);
                window.Add(btEditorTerrainHeight);
                btEditorTerrainHeight.Click += btEditorTerrainHeight_Click;
                The.InGameUI.SidePanelEditorTerrainHeight.AccessButton = btEditorTerrainHeight;
                btEditorTerrainHeight.ToolTip = "Change terrain height";
               
            }

            Rectangle rect = intf.gui.GUISpriteSheet.GetSourceRectangle("basic_dirt_smallsplotch");
            Image smallSplotch = Panel.AddImage(intf.gui, window, rect, new Point(window.Width - rect.Width + 20, 82));
        

            rect = intf.gui.GUISpriteSheet.GetSourceRectangle("basic_dirt_bottom");
            Image dirtBottom = Panel.AddImage(intf.gui, window, rect, new Point(0, window.Height - rect.Height));
           

        }

       
       

       


        void InGameUI_SelectedEntityChangedEvent(SimSide.Entities.EntityID? oldEntity, SimSide.Entities.EntityID? newEntity)
        {
            if (newEntity == null)
            {
                btEntity.Enabled = false;
            }
            else
            {
                btEntity.Enabled = true;
            }
        }

       
        static void btEntity_Click(UIComponent sender, EventArgs e)
        {
            The.InGameUI.ShowSelectedEntityPanel(false);
        }

        static void btZone_Click(UIComponent sender, EventArgs e)
        {
            The.InGameUI.ShowSelectedMapAreaPanel(false);
        }


        public void DisableMissions()
        {
            window.Remove(btMissions);
        }

        public void DisableGraphs()
        {
            window.Remove(btGraphs);
        }

        public void DisableContacts()
        {
            window.Remove(btDiplomacy);
        }
        public void DisablePersonell()
        {
            window.Remove(btPersonnel);
        }

        public void DisablePolicy()
        {
            window.Remove(btPolicy);
        }

        public void DisableWorldMap()
        {
            window.Remove(btWorld);
        }

        public void DisableLedger()
        {
            window.Remove(btLedger);
        }

        public void CheckAccessButton(ImageButton bt)
        {
            bt.IsChecked = true;

            DeselectOtherRadioButtons(bt);


        }

        private void DeselectOtherRadioButtons(ImageButton bt)
        {

            if (The.Sim.Mode == Sim.EngineMode.Game)
            {
                if (bt != btStock)
                {
                    btStock.IsChecked = false;
                }

                if (bt != btJobs)
                {
                    btJobs.IsChecked = false;
                }

               /* if (bt != btDiplomacy)
                {
                    btDiplomacy.IsChecked = false;
                }*/

                if (bt != btGraphs)
                {
                    btGraphs.IsChecked = false;
                }

                if (bt != btMissions)
                {
                    btMissions.IsChecked = false;
                }

                if (bt != btEventDialogs)
                {
                    btEventDialogs.IsChecked = false;
                }

                if (bt != btPersonnel)
                {
                    btPersonnel.IsChecked = false;
                }

                if (bt != btZone)
                {
                    btZone.IsChecked = false;
                }

                if (bt != btEntity)
                {
                    btEntity.IsChecked = false;
                }
            }
            else
            {

            }
        }

      
      /*  void editor_Click(UIComponent sender, EventArgs e)
        {
            intf.Change(The.InGameUI.SidePanelEdit);
        }

        void action_Click(UIComponent sender, EventArgs e)
        {
            intf.HUDActionPanel.ShowInScreenSpace(sender.AbsolutePosition.X,
                sender.AbsolutePosition.Y - intf.HUDActionPanel.DisplayWindow.Height);

            DeselectOtherRadioButtons(tbAction);
        }*/

        void editorPlaceEntity_Click(UIComponent sender, EventArgs e)
        {
            The.InGameUI.ChangeRosterPanel(The.InGameUI.SidePanelEditorEntity);

            DeselectOtherRadioButtons(btEditorPlaceEntity);
        }

        void btEditorPaintTile_Click(UIComponent sender, EventArgs e)
        {
            The.InGameUI.ChangeRosterPanel(The.InGameUI.SidePanelEditorSoil);

            DeselectOtherRadioButtons(btEditorPaintTile);
        }

        void btEditorTerrainHeight_Click(UIComponent sender, EventArgs e)
        {
            The.InGameUI.ChangeRosterPanel(The.InGameUI.SidePanelEditorTerrainHeight);

            DeselectOtherRadioButtons(btEditorTerrainHeight);
        }


        void stock_Click(UIComponent sender, EventArgs e)
        {
            The.InGameUI.ChangeRosterPanel(The.InGameUI.InventoryPanel);

            DeselectOtherRadioButtons(btStock);
        }


        void tbEventDialogs_Click(UIComponent sender, EventArgs e)
        {
            The.InGameUI.ChangeRosterPanel(The.InGameUI.EventArchivePanel);

            DeselectOtherRadioButtons(btEventDialogs);

          /*  if (The.Client.EventDialogsData.Count > 0)
            {                
                EventActionDialog.ShowEventDialog(mode: EventDialog.Mode.EventArchive);
            }*/
            
        }

        void tbGraphs_Click(UIComponent sender, EventArgs e)
        {
            The.InGameUI.ChangeRosterPanel(The.InGameUI.GraphPanel);

            DeselectOtherRadioButtons(btGraphs);
      
        }

        void btLedger_Click(UIComponent sender, EventArgs e)
        {
            The.InGameUI.ChangeRosterPanel(The.InGameUI.LedgerPanel);

            DeselectOtherRadioButtons(btLedger);
        }

        void btWorld_Click(UIComponent sender, EventArgs e)
        {
            The.InGameUI.ChangeRosterPanel(The.InGameUI.WorldMapPanel);

            DeselectOtherRadioButtons(btWorld);

        }

        void tbDiplomacy_Click(UIComponent sender, EventArgs e)
        {
           The.InGameUI.ChangeRosterPanel(The.InGameUI.DiplomacyPanel);
             
           DeselectOtherRadioButtons(btDiplomacy);

        }

        void tbJobs_Click(UIComponent sender, EventArgs e)
        {
            The.InGameUI.ChangeRosterPanel(The.InGameUI.JobsPanel);

            DeselectOtherRadioButtons(btJobs);
        }

        void tbPolicy_Click(UIComponent sender, EventArgs e)
        {
            The.InGameUI.ChangeRosterPanel(The.InGameUI.PolicyPanel);

            DeselectOtherRadioButtons(btPolicy);
        }

        public void ShowMissions()
        {
            The.InGameUI.ChangeRosterPanel(The.InGameUI.MissionsPanel);

            DeselectOtherRadioButtons(btMissions);
        }

        void tbMissions_Click(UIComponent sender, EventArgs e)
        {
            ShowMissions();
        }


        void btPersonnel_Click(UIComponent sender, EventArgs e)
        {
            The.InGameUI.ChangeRosterPanel(The.InGameUI.PersonnelRosterPanel);

            DeselectOtherRadioButtons(btPersonnel);
        }

    }
}
