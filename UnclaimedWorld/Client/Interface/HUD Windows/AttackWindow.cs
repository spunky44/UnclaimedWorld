using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UWGame.SimSide.Maps;
using InputEventSystem;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Items;
using UWGame.SimSide.Processes;
using UWGame.SimSide;
using UWGame.SimSide.Trees;
using UWGame.SimSide.Expeditions;
using UWGame.ClientSide.Interface.Controls;
using UWGame.Control.Commands;
using UWGame.SimSide.Commands;
using UWGame.ClientSide.Interface.Inventory;
using UWGame.SimSide.Collisions;

namespace UWGame.ClientSide.Interface.HUD_Windows
{
    public class AttackWindow: HUDWindow
    {
        MapArea mapArea;
        Expedition expedition;

        CheckBox cbAttackVermin;

        Grid grid;

        FillableBar fbNoOfAttackers;

        Label lblName, lblHeader;
        Image headerIcon;

              
        TextButton btCancel, btOK;

        int itemTypeIconColumnX = 10; //18;

        bool isFirstUpdate;

        public AttackWindow() //int screenWidth, int screenHeight, int screenX)
            : base(239, 240, level: Level.Bottom, isMovable: true)
        {

           /* DisplayWindow.SetResizableArea(ResizeAreas.Top, true);
            DisplayWindow.SetResizableArea(ResizeAreas.Bottom, true);

            DisplayWindow.MinHeight = 200;
            DisplayWindow.ResizableBorderSize = 6;
            DisplayWindow.Resize += DisplayWindow_Resize;
            */

            AddZoneNameAndHeader("", "ATTACK", "HUD_icon_sword", doubleSpacing, out lblName, out lblHeader, out headerIcon);

            Label lblAttackers = new Label(gui);
            lblAttackers.Init(Label.LabelType.HUDWindow);
            Add(lblAttackers);
            lblAttackers.Text = "No. of attackers:";
            lblAttackers.ToolTip = "Select how many armed colony members should participate in the attack. \nWhen all threats are eliminated, the task will be canceled automatically"; 
            lblAttackers.X = tripleSpacing;
            lblAttackers.Y = 52;

            fbNoOfAttackers = new FillableBar(gui, FillableBar.FillableBarType.HUDSlider, false, true, GameData.Instance.GUIConstants.TimeBetweenSliderButtonIncrements, GameData.Instance.GUIConstants.SliderButtonDelay);
            Add(fbNoOfAttackers);
            fbNoOfAttackers.X = 120;
            fbNoOfAttackers.Y = lblAttackers.Y;
            fbNoOfAttackers.Width = 120;
            fbNoOfAttackers.MaxValue = 10;
            fbNoOfAttackers.Value = 1;
            fbNoOfAttackers.UpdateSliderPosition();

          //  SetSummaryDelegate = new FullLCDPanel.SetCollapsedSummary(SidePanelEntity.SetSummaryAsTotal);

           // CreateGridHeader();
       

            /*
            grid = new Grid(gui, ListBoxType.HUDAndLCD, WindowSystem.Label.LabelType.HUDWindow);
            grid.IsOuterGrid = true; // false;
           
            // categoryGrid.Position = new Point(
            // categoryGrid.DebugTag = "categoryGrid";
            grid.X = doubleSpacing;
            grid.Y = fbNoOfAttackers.Bottom + doubleSpacing; 
            grid.FixedItemHeights = true; // false;
            grid.Width = DisplayWindow.ViewPort.Width - 2 * doubleSpacing; // listSurface.Width; // make grid fill the panel           
            grid.ScrollBarEnabled = true;
            grid.ItemHeight = 27; // 22;
            grid.CanGrowInHeight = false; // true; // false; // true;            
            grid.Font = GUIManager.LCDandHUDBodyFontPath;
            grid.Height = 136; // 40; // 160
            Add(grid);*/

            cbAttackVermin = new CheckBox(gui);
            Add(cbAttackVermin);
            cbAttackVermin.Init(CheckBoxType.HUDCheckBox);
            cbAttackVermin.Text = "Also attack vermin";
            cbAttackVermin.ToolTip = "Select whether vermin should be attacked by the patrollers in addition to dangerous animals."; //Select whether any nearby vermin should be attacked while on patrol
            cbAttackVermin.FitToText();
            cbAttackVermin.X = tripleSpacing;
            cbAttackVermin.Y = fbNoOfAttackers.Bottom + doubleSpacing;            
            cbAttackVermin.button.DebugTag = "cbAttackVermin";

            btCancel = new TextButton(gui);
            Add(btCancel);
            btCancel.Text = "CANCEL";
            btCancel.Init(TextButton.TextButtonType.HUD);
            btCancel.Click += new ClickHandler(btCancel_Click);
            btCancel.Width = 72;
           // btCancel.Y = DisplayWindow.Height - btCancel.Height - doubleSpacing;
            btCancel.X = DisplayWindow.Width - doubleSpacing - btCancel.Width;


            btOK = new TextButton(gui);
            Add(btOK);
            btOK.Text = "OK";
            btOK.Init(TextButton.TextButtonType.HUD);
            btOK.Click += new ClickHandler(btOk_Click);
            btOK.Width = 72;
           // btOK.Y = DisplayWindow.Height - btOK.Height - doubleSpacing;
            btOK.X = btCancel.X - 2 - btOK.Width;


            SetVerticalPositions();
        }


        private void SetVerticalPositions()
        {
            btCancel.Y = DisplayWindow.Height - btCancel.Height - doubleSpacing;
            btOK.Y = btCancel.Y; 

           // grid.Height = btOK.Y - 13 - grid.Y;

        }

        void DisplayWindow_Resize(UIComponent sender)
        {
           // SetVerticalPositions();
        }


       
     
        void btCancel_Click(UIComponent sender, EventArgs e)
        {
            Hide();
        }


        /// <summary>
        /// copied from GatherWindow
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btOk_Click(UIComponent sender, EventArgs e)
        {           
          
            EntityGroupID groupID = expedition.OwnedEntities.ID;
            Zone selectedZone = mapArea.Zone; 

           
            int noOfAttackers = fbNoOfAttackers.Value;

           // noOfAttackers = Common.ClampBottom(noOfAttackers, 1);

            bool attackVermin = cbAttackVermin.IsChecked;

            Command command;
            if (selectedZone != null && selectedZone.AttackAreaJob != null)
            {
                //#UPDATEATTACKJOBTAKERS
                // NEW - update attack job:
                // the evaluator won't cancel takers if there are too many - put this in one of the managers..?
                command = new AttackAreaUpdateJob(selectedZone.AttackAreaJob.ID, true, noOfAttackers);

            }
            else
            {

                //AttackArea command;
                if (The.InGameUI.SelectedZone != null)
                {
                    command = new AttackArea(The.InGameUI.SelectedZone.ID, true, groupID, attackVermin, true, noOfAttackers);
                }
                else
                {
                    command = new AttackArea(The.InGameUI.SelectedTiles, true, groupID, attackVermin, true, noOfAttackers);
                }
            }

            The.Client.Controller.StoreAndExecuteCommand(command);       


  
            if (selectedZone != null) 
            {
                selectedZone.RemoveZoneOrFireOrdersChangedEvent();
            }

            Hide();
        }
            
               

    /*    void btGatherNone_Click(UIComponent sender, EventArgs e)
        {
            // clear all jobs:
            UIComponent itemComponent;

          
            foreach (var row in grid.EntriesByKey)
            {
                itemComponent = row.Value.FindChildById(UIComponent.DataControlID.CurrentOrders);
                if (itemComponent != null)
                {
                    FillableBar slider = (FillableBar)itemComponent;

                    slider.Value = 0;

                    SetUserChangedSliderState(slider); // slider.EventArgs);

                    slider.UpdateSliderPosition();
                }
            }
                

        }*/

            
        

        public override void Hide()
        {           
            DisplayWindow.Hide();          
        }

                

        public override void Refresh()
        {                   
            //Populate(); 
        }


        private List<Tuple<EntityType, bool>> GetPreyToDisplay()
        {
            
            List<Tuple<EntityType, bool>> result = new List<Tuple<EntityType,bool>>(); 
            HashSet<EntityType> habitats = GetHabitats();
            if (habitats != null)
            {
                foreach (var item in habitats)
                {
                    if (The.InGameUI.UIAllegiance.RepresentativeEntityType.IntelligenceType.PreyTypes.Contains(item))
                    {
                        result.Add(new Tuple<EntityType, bool>(item, true));
                    }
                }
            }

            foreach (var item in The.InGameUI.UIAllegiance.SharedKnowledge.PlaySiteKnowledge.SpottedPrey)
	        {
                if (habitats == null || !habitats.Contains(item))
                {
                    result.Add(new Tuple<EntityType, bool>(item, false));
                }		 
            }

            return result;
        }

       /* private bool ShowPreyInList(EntityType entityType)
        {
            return PreyHasBeenSeen(entityType) || IsPreyHabitat(entityType);
        }

        private bool PreyHasBeenSeen(EntityType entityType)
        {
            return The.InGameUI.UIAllegiance.SharedKnowledge.PlaySiteKnowledge.SpottedPrey.Contains(entityType);
        }*/

        private HashSet<EntityType> GetHabitats()
        {
            // test the map area, if inside a critter expedition, return true
            // quad tree for expeditions?
            // skip shrouded tiles
            // indicate these with an icon, like a paw perhaps

            //The.Sim.PlaySite.PlaySite.ExpeditionRadiusQuadTree.GetCollidablesContainingPoint()

            HashSet<Collidable<Expedition>> expeditions = new HashSet<Collidable<Expedition>>();
            mapArea.IterateArea(tile => GetPreyHabitatsOnTile(tile, expeditions));


            HashSet<EntityType> result = null;

            foreach (var item in expeditions)
            {
                Common.AddToSet(ref result, item.Parent.Allegiance.RepresentativeEntityType);
            }

            return result;

            /*
            HashSet<Collidable<Expedition>> expeditions = new HashSet<Collidable<Expedition>>();
            mapArea.IterateArea(tile 
                =>  The.Sim.PlaySite.PlaySite.ExpeditionRadiusQuadTree.GetCollidablesContainingPoint(MapManager.TileToWorldPosVector2(tile.TilePos.ToPoint()),
                expeditions));*/

        }

        private void GetPreyHabitatsOnTile(TerrainTile tile, HashSet<Collidable<Expedition>> expeditions)
        {
            if (tile.HasEverBeenSeenByPlayer)
            {
                Vector2 pos = MapManager.TileToWorldPosVector2(tile.TilePos.ToPoint());
                
                The.Sim.PlaySite.PlaySite.ExpeditionRadiusQuadTree.GetCollidablesContainingPoint(pos,
                    expeditions);
            }
        }

       
       
        /// <summary>
        /// grid without categories
        /// </summary>
        /// <param name="gui"></param>
        /// <param name="outerGrid"></param>
        /// <param name="setCollapsedSummary"></param>
        /// <param name="clickHandler"></param>
        /// <param name="dictionary"></param>
        private void Populate()
        {
            UIComponent item = null;
         

            EntityGroup owner = mapArea.GetOwner(); // TileSelectionContextMenu.GetMapAreaOwner(mapArea);

            if (owner == null)
            {
                // oops, what should happen here..? clear the grid?
                grid.Clear();
                return; 
            }


            var data = GetPreyToDisplay();

            int currentNoOfCategories = grid.Entries.Count;

            grid.BeginAddingEntries();

           /* if (The.InGameUI.UIAllegiance.RepresentativeEntityType.IntelligenceType.PreyTypes != null)
            {*/

                foreach (var prey in data) //  var entityType in The.InGameUI.UIAllegiance.RepresentativeEntityType.IntelligenceType.PreyTypes)
                {
                    if (!grid.TryGetEntry(prey.Item1, out item))
                    {
                        item = AddRow(prey.Item1, owner);
                    }

                   // UpdateRow(item, prey.Item1, prey.Item2, owner);
                }
           // }

           // grid.DeleteEntries<EntityType>(e => e.CanBeHuntedBy(The.InGameUI.UIAllegiance));
                
            grid.DeleteEntries<EntityType>(e => data.Any(t => t.Item1 == e));
            grid.Sort(i => i.OrderByTag1, Grid.Sorting.Ascending);            

            grid.EndAddingEntries();

            isFirstUpdate = false;
        }


       
        /*
        private void CleanUpGrid(Grid grid, Object key)
        {   
            // remove resource rows that no longer appear in the data source:
            grid.DeleteEntries<ResourceType>(e => data.ContainsKey(e));

            if (grid.Count == 0)
            {
                // remove empty category grids:
                outerGrid.RemoveEntry(key);                
            }
        }*/


       /* private void UpdateRow(UIComponent item, EntityType entityType, bool isInHabitat, EntityGroup owner) 
        {            
          
            if (entityType.KeyName.Contains("ranches"))
            {

            }

            item.Tag2 = isInHabitat;

            // update the slider:          
            FillableBar fillableBar = item.FindChildById(UIComponent.DataControlID.CurrentOrders) as FillableBar;
        
        
            ProductionOrder order;
            if (owner.ProductionOrders.Orders.TryGetValue(entityType.BiologicalType.CarcassType, out order)) // all Items have an entry.
            {
                int currentDirectOrder = 0;
                if (mapArea.Zone != null)
                {
                    mapArea.Zone.ZoneHunt.CreaturesToHunt.TryGetValue(entityType, out currentDirectOrder);
                }
              
                UpdateDirectOrders(currentDirectOrder, fillableBar, null);                
            }            
        }*/

                      

        public override void ShowOnPlayfield(int screenPosX, int screenPosY, bool avoidRightInterfaceArea = true)
        {
            isFirstUpdate = true;

            base.ShowOnPlayfield(screenPosX, screenPosY, avoidRightInterfaceArea);

            
            mapArea = TileSelectionContextMenu.GetMapArea();
            expedition = mapArea.GetOwner().Parent as Expedition; 

            string zoneName = "";
            if (mapArea.Zone != null)
            {
                zoneName = mapArea.Zone.GetDisplayName();
            }

            if (mapArea.Zone != null && mapArea.Zone.AttackAreaJob != null)
            {
                cbAttackVermin.IsChecked = mapArea.Zone.AttackAreaJob.AttackVermin;

                fbNoOfAttackers.Value = mapArea.Zone.AttackAreaJob.MaxJobPositions;
                fbNoOfAttackers.UpdateSliderPosition();

                cbAttackVermin.Enabled = false; // NEW //#UPDATEATTACKJOBTAKERS
              //  btOK.Enabled = true; // NEW //#UPDATEATTACKJOBTAKERS
                
            }
            else
            {
                cbAttackVermin.IsChecked = false;

                cbAttackVermin.Enabled = true;
                
            }

            SetDisplayName(zoneName, lblName, lblHeader, headerIcon);

        }


        private UIComponent AddRow(EntityType entityType, EntityGroup owner) 
        {

           // EventArgs eventArgs = new HarvestJobsButtonEventArgs(entityType, resourcesAndJobs);

            UIComponent item = new UIComponent(gui);
            grid.AddEntry(entityType, item);

            item.OrderByTag1 = entityType.PluralName;

            int xPos;
            // int width;

            Image icon = InventoryPanel.AddEntityTypeIcon(entityType, item, itemTypeIconColumnX);

            DataTypeButton tbCaption = new DataTypeButton(gui, DataSheet.InfoToShow.Data, entityType, owner.ID, false);
            tbCaption.Init(TextButton.TextButtonType.HUDToolTipWhite);
            tbCaption.ID = UIComponent.DataControlID.Caption;
            tbCaption.IsRoot = true;
            tbCaption.Text = entityType.PluralName;
            item.Add(tbCaption);
            tbCaption.TextAlignment = TextButton.TextAlign.Left;
            tbCaption.Width = 125; // 105; 
            tbCaption.X = 22;
            tbCaption.DebugTag = "entityTypeButton";

           // int produceColumnX = orderedX - 11;

                       
            CheckBox cbSelect = new CheckBox(gui);
            cbSelect.Init(CheckBoxType.HUDCheckBox);
            cbSelect.ID = UIComponent.DataControlID.Selector;
            cbSelect.ToolTip = "If checked, any entities of this type will be attacked"; // tooltip;
            //    cbSelect.EventArgs = eventArgs;
            item.AlignVertically(cbSelect);
           // cbSelect.X = produceColumnX;          
            cbSelect.IsChecked = false;
            cbSelect.Tag1 = entityType;
                    
          
            item.OrderByTag2 = entityType.PluralName; //We want to order the items by name
            
            return item;
        }
      
           
    }
}
