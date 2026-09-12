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
    public class HuntWindow: HUDWindow
    {
        MapArea mapArea;
        Expedition expedition;

       
        Grid grid;


        Label lblName, lblHeader;
        Image headerIcon;

        FullLCDPanel.SetCollapsedSummary SetSummaryDelegate;


        /// <summary>
        /// we store the user changes here before he commits them with 'Ok'
        /// </summary>
      //  Dictionary<EntityType, MapArea.FindPreyJobs> data = new Dictionary<EntityType, MapArea.FindPreyJobs>();
       // Dictionary<ResourceType, MapArea.ResourcesAndJobs> data = new Dictionary<ResourceType, MapArea.ResourcesAndJobs>();


        Dictionary<FillableBar, bool> userChangedData = new Dictionary<FillableBar, bool>();

        TextButton btCancel, btOK;

        int itemTypeIconColumnX = 10; //18;

        bool isFirstUpdate;

        public HuntWindow() //int screenWidth, int screenHeight, int screenX)
            :base(334 /*320*/, 275, level: Level.Bottom, isMovable: true)
        {

            DisplayWindow.SetResizableArea(ResizeAreas.Top, true);
            DisplayWindow.SetResizableArea(ResizeAreas.Bottom, true);

            DisplayWindow.MinHeight = 200;
            DisplayWindow.ResizableBorderSize = 6;
            DisplayWindow.Resize += DisplayWindow_Resize;


            AddZoneNameAndHeader("", "HUNT", "HUD_icon_hunt", doubleSpacing, out lblName, out lblHeader, out headerIcon);

          //  resourceClickHandler = new ClickHandler(ResourceClicked);

            SetSummaryDelegate = new FullLCDPanel.SetCollapsedSummary(SidePanelEntity.SetSummaryAsTotal);

            CreateGridHeader();


          /*  UIComponent listSurface = new UIComponent(gui);
            Add(listSurface);
            listSurface.Y = 48 + topMargin;
            listSurface.Width = DisplayWindow.ViewPort.Width;
            listSurface.Height = 40; // DisplayWindow.ViewPort.Height - 40;
            */            

            grid = new Grid(gui, ListBoxType.HUDAndLCD, WindowSystem.Label.LabelType.HUDWindow);
            grid.IsOuterGrid = true; // false;
           
            // categoryGrid.Position = new Point(
            // categoryGrid.DebugTag = "categoryGrid";
            grid.X = doubleSpacing;
            grid.Y = 48 + topMargin;
            grid.FixedItemHeights = true; // false;
            grid.Width = DisplayWindow.ViewPort.Width - 2 * doubleSpacing; // listSurface.Width; // make grid fill the panel           
            grid.ScrollBarEnabled = true;
            grid.ItemHeight = 27; // 22;
            grid.CanGrowInHeight = false; // true; // false; // true;            
            grid.Font = GUIManager.LCDandHUDBodyFontPath;
            grid.Height = 136; // 40; // 160
            Add(grid);
                                   
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
            btOK.Y = btCancel.Y; // DisplayWindow.Height - btOK.Height - doubleSpacing;

            grid.Height = btOK.Y - 13 - grid.Y;

        }

        void DisplayWindow_Resize(UIComponent sender)
        {
            SetVerticalPositions();
        }


       // private const int resourceTextX = 32 + sideMargin;
       // private const int inStockX = 101 + sideMargin;
        private const int orderedX = 195; // 175; // 175;
        private const int gridHeaderY = 22 + topMargin;

        private void CreateGridHeader()
        {
          /*  Label lblHeader = new Label(gui);
            Add(lblHeader);
            lblHeader.Init(Label.LabelType.HUDWindow);
            lblHeader.Text = "Resource"; 
            lblHeader.FitToText();
            lblHeader.X = resourceTextX;
            lblHeader.Y = gridHeaderY;
            */

         /*   lblHeader = new Label(gui);
            Add(lblHeader);
            lblHeader.Init(Label.LabelType.HUDWindow);
            lblHeader.Text = "In stock";
            lblHeader.FitToText();
            lblHeader.X = inStockX;
            lblHeader.Y = gridHeaderY;
            */

            Label lblSliderHeading = new Label(gui);
            Add(lblSliderHeading);
            lblSliderHeading.Init(Label.LabelType.HUDWindow);
            lblSliderHeading.Text = "Ordered"; // "Ordered / available";
            lblSliderHeading.FitToText();
            lblSliderHeading.X = orderedX;
            lblSliderHeading.Y = gridHeaderY;
        }

      /*  void btGatherAll_Click(UIComponent sender, EventArgs e)
        {
            // set all jobs to their maximum:

            UIComponent itemComponent;
            UIComponent item;
                      
          
            foreach (var row in outerGrid.EntriesByKey)
            {
                itemComponent = row.Value.FindChildById(UIComponent.DataControlID.CurrentOrders);
                if (itemComponent != null)
                {
                    FillableBar slider = (FillableBar)itemComponent;

                    if (slider.Visible)
                    {
                        slider.Value = slider.MaxValue;

                        SetUserChangedSliderState(slider); // slider.EventArgs);

                        slider.UpdateSliderPosition();
                    }
                }
            }
                   
        }*/

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
            bool userChangedData = false;

            EntityGroupID groupID = expedition.OwnedEntities.ID;
            Zone selectedZone = mapArea.Zone; // The.InGameUI.SelectedZone;

            //EntityGroup owner = mapArea.GetOwner(); 

            // we don't really need the data dictionary now that the sliders are all on the main form instead of in a popup... but let's keep it for now.
            // retrieve the data:

            UIComponent item;
      

            foreach (var row in grid.EntriesByKey)
            {
                FillableBar slider = row.Value.FindChildById(UIComponent.DataControlID.CurrentOrders) as FillableBar;
                ImageButton btStanding = null;
                if (GameData.Instance.GUIConstants.EnableStandingOrders)
                {
                   // btStanding = row.Value.FindChildById(UIComponent.DataControlID.StandingOrderModeHotspot).Controls[0] as ImageButton;
                    btStanding = row.Value.FindChildById(UIComponent.DataControlID.StandingOrderModePadlock) as ImageButton;
                }

                EntityType entityType = (EntityType)row.Key; 

                if (GameData.Instance.GUIConstants.EnableStandingOrders && btStanding.IsChecked)
                {
                    // set the number of carcasses to keep in store. Copied from Gather
                    if (expedition.OwnedEntities.ProductionOrders.Orders[entityType.BiologicalType.CarcassType].AmountToKeepInStore != slider.Value)
                    {
                        SetStandingOrder newOrder = new SetStandingOrder(expedition.ID, entityType.BiologicalType.CarcassType.KeyName, slider.Value, true);
                        The.Client.Controller.StoreAndExecuteCommand(newOrder);                       
                    }

                    
                    if (mapArea.Zone == null || !mapArea.Zone.ZoneHunt.AllowStandingOrderHunt.Contains(entityType))
                    {
                        if (slider.Value > 0)
                        {
                            // enable
                            SetStandingOrderHuntInZone enableCommand;
                            if (selectedZone != null)
                            {
                                enableCommand = new SetStandingOrderHuntInZone(selectedZone.ID, true, entityType.KeyName, true, groupID);
                            }
                            else
                            {
                                enableCommand = new SetStandingOrderHuntInZone(mapArea, true, entityType.KeyName, true, groupID);
                            }
                           
                            The.Client.Controller.StoreAndExecuteCommand(enableCommand);
                        }
                        else
                        {
                            // disable
                            SetStandingOrderHuntInZone disableCommand;
                            if (selectedZone != null)
                            {
                                disableCommand = new SetStandingOrderHuntInZone(selectedZone.ID, true, entityType.KeyName, false, groupID);
                            }
                            else
                            {
                                disableCommand = new SetStandingOrderHuntInZone(mapArea, true, entityType.KeyName, false, groupID);
                            }
                          
                            The.Client.Controller.StoreAndExecuteCommand(disableCommand);
                        }
                    }
                }
                else
                {
                    // direct order. sets the number of critters to hunt in the zone.
                    int oldOrder = 0;
                    if (selectedZone != null)
                    {
                        selectedZone.ZoneHunt.CreaturesToHunt.TryGetValue((EntityType)row.Key, out oldOrder);
                    }

                    if (oldOrder != slider.Value)
                    {
                        bool removeAfterFirstHunt = false;
                        Command huntCommand;
                        if (selectedZone != null)
                        {
                            huntCommand = new HuntArea(The.InGameUI.SelectedZone.ID, true, entityType, slider.Value, removeAfterFirstHunt, groupID);
                        }
                        else
                        {
                            huntCommand = new HuntArea(The.InGameUI.SelectedTiles, true, entityType, slider.Value, removeAfterFirstHunt, groupID);
                        }

                        The.Client.Controller.StoreAndExecuteCommand(huntCommand);
                    }

                    /*
                    MapArea.FindPreyJobs rowData = data[(EntityType)row.Key];

                    if (rowData.NumberOfJobsInZone != slider.Value)
                    {

                        userChangedData = true;

                        rowData.NumberOfJobsInZone = slider.Value;
                        rowData.UserChangedData = true;

                        data[entityType] = rowData;
                    }*/

                    if (selectedZone != null && selectedZone.ZoneHunt.AllowStandingOrderHunt.Contains(entityType))
                        //mapArea.Zone != null && mapArea.Zone.ZoneHunt.AllowStandingOrderHunt.Contains(entityType))
                    {
                        SetStandingOrderHuntInZone disableCommand;
                       /* if (selectedZone != null)
                        {*/
                            disableCommand = new SetStandingOrderHuntInZone(selectedZone.ID, true, entityType.KeyName, false, groupID);
                       /* }
                        else
                        {
                            disableCommand = new SetStandingOrderHuntInZone(mapArea, true, entityType.KeyName, false, groupID);
                        }*/
                      
                        The.Client.Controller.StoreAndExecuteCommand(disableCommand);
                    }
                }

                // make sure we are up to date:
                selectedZone = The.InGameUI.SelectedZone;
            }
                          
  
            if (selectedZone != null) // && !selectedZone.ZoneHunt.HasHuntOrders())
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
            Populate(); 
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

                    UpdateRow(item, prey.Item1, prey.Item2, owner);
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


        private void UpdateRow(UIComponent item, EntityType entityType, bool isInHabitat, EntityGroup owner) 
        {            
          
            item.Tag2 = isInHabitat;

          /*
            DataTypeButton tbCaption = (DataTypeButton)item.FindChildById(UIComponent.DataControlID.Caption);

            tbCaption.SetStockStatusColor(noOfAvailableItems > 0);
            */

            // update the slider:          
            FillableBar fillableBar = item.FindChildById(UIComponent.DataControlID.CurrentOrders) as FillableBar;
        
          //  UIComponent hotspot = null;
            ImageButton btStandingOrder = null;
            if (GameData.Instance.GUIConstants.EnableStandingOrders)
            {
               /* hotspot = item.FindChildById(UIComponent.DataControlID.StandingOrderModeHotspot);
                btStandingOrder = hotspot.Controls[0] as ImageButton;*/

                btStandingOrder = item.FindChildById(UIComponent.DataControlID.StandingOrderModePadlock) as ImageButton;
            }

            Image imHabitat;
            item.FindChildById(UIComponent.DataControlID.Habitat, out imHabitat);
            imHabitat.Visible = isInHabitat;

            ProductionOrder order;
            if (owner.ProductionOrders.Orders.TryGetValue(entityType.BiologicalType.CarcassType, out order)) // all Items have an entry.
            {
                int currentDirectOrder = 0;
                if (mapArea.Zone != null)
                {
                    mapArea.Zone.ZoneHunt.CreaturesToHunt.TryGetValue(entityType, out currentDirectOrder);
                }

                if (GameData.Instance.GUIConstants.EnableStandingOrders)
                {
                    if (isFirstUpdate)
                    {
                        if (mapArea.Zone != null)
                        {
                            if (isFirstUpdate)
                            {
                                ProductionOrderControl.SetPadlockButtonState(mapArea.Zone.ZoneHunt.AllowStandingOrderHunt.Contains(entityType) && order.AmountToKeepInStore.HasValue,
                                    btStandingOrder);
                            }

                        }
                        else
                        {
                            btStandingOrder.IsChecked = false;
                           // btStandingOrder.Visible = false;
                        }

                    }

                    if (btStandingOrder.IsChecked)
                    {
                        UpdateStandingOrderProduction(order, fillableBar, btStandingOrder);

                    }
                    else
                    {
                        UpdateDirectOrders(currentDirectOrder, fillableBar, btStandingOrder);
                    }
                }
                else
                {
                    UpdateDirectOrders(currentDirectOrder, fillableBar, null);
                }
            }
            
        }



        private void UpdateStandingOrderProduction(ProductionOrder stockTarget, FillableBar fillableBar, ImageButton btStandingOrder)
        {
            int currentOrder = stockTarget.AmountToKeepInStore ?? 0;

            //fillableBar.Color = Color.Cornsilk;
            fillableBar.ColorAllControls = GameData.Instance.GUIConstants.StandingOrderTint;
            btStandingOrder.NormalColor = GameData.Instance.GUIConstants.StandingOrderTint;
 
            // don't limit the slider based on inputs. use a fixed maximum, like 50...

            bool sliderValuesWereChanged = false;

            if (!userChangedData.ContainsKey(fillableBar)) //  don't overwrite user changes
            {
                if (fillableBar.Value != currentOrder)
                {
                    fillableBar.Value = currentOrder;
                    sliderValuesWereChanged = true;
                }
            }


            fillableBar.StepSize = 1;

            if (fillableBar.MaxValue != GameData.Instance.GUIConstants.MaxStandingOrder)
            {
                fillableBar.MaxValue = GameData.Instance.GUIConstants.MaxStandingOrder;
                sliderValuesWereChanged = true;
            }


            if (sliderValuesWereChanged)
            {
                fillableBar.UpdateSliderPosition();
            }
        }

        const int maxPreyToHunt = 10;

        private void UpdateDirectOrders(int currentOrder, FillableBar fillableBar, ImageButton btStandingOrder)
        {
            SetNormalTint(fillableBar, btStandingOrder);

            bool sliderValuesWereChanged = false;
            if (!userChangedData.ContainsKey(fillableBar)) //  don't overwrite user changes
            {
                if (fillableBar.Value != currentOrder)
                {
                    fillableBar.Value = currentOrder;
                    sliderValuesWereChanged = true;
                }
            }

            if (fillableBar.MaxValue != maxPreyToHunt)
            {
                sliderValuesWereChanged = true;
                fillableBar.MaxValue = maxPreyToHunt;
            }

            if (sliderValuesWereChanged)
            {
                fillableBar.UpdateSliderPosition();
            }

            /*
            if (!canProduce)
            {
                fillableBar.Visible = false;
            }
            else
            {
                fillableBar.Visible = true;
            }*/
        }

        public static void SetNormalTint(FillableBar fillableBar, ImageButton btStandingOrder)
        {
            fillableBar.ColorAllControls = UIComponent.HUDTint;

            if (btStandingOrder != null)
            {
                btStandingOrder.NormalColor = UIComponent.HUDLightTint; // HUDTint;
            }
        }


        public override void ShowOnPlayfield(int screenPosX, int screenPosY, bool avoidRightInterfaceArea = true)
        {
            isFirstUpdate = true;

            base.ShowOnPlayfield(screenPosX, screenPosY, avoidRightInterfaceArea);

            userChangedData.Clear();

           

           // MapArea mapArea;
            mapArea = TileSelectionContextMenu.GetMapArea();
            expedition = mapArea.GetOwner().Parent as Expedition; 

            string zoneName = "";
            if (mapArea.Zone != null)
            {
                zoneName = mapArea.Zone.GetDisplayName();
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

            int produceColumnX = orderedX - 11;

            Image imHabitat = new Image(gui);
            imHabitat.SetSkinLocations(gui.GUISpriteSheet.GetSourceRectangle("HUD_icon_track"), Icon.UIType.HUD);
            item.Add(imHabitat);
            imHabitat.ResizeControlToFitImage();
            imHabitat.X = tbCaption.Right + 6;
            imHabitat.ID = UIComponent.DataControlID.Habitat;
            imHabitat.ToolTip = "The zone is in this creature's habitat";

            if (GameData.Instance.GUIConstants.EnableStandingOrders)
            {               
                ImageButton btPadlock = new ImageButton(Interface.gui);
                item.Add(btPadlock);
                btPadlock.Init(ImageButtonType.HUDPadlock);
                btPadlock.Position = new Point(produceColumnX - 13, 0);
                btPadlock.Click += btPadlock_Click;
              //  btPadlock.Visible = false;
                btPadlock.ToolTip = ProductionOrderControl.btPadlockTooltip;
                item.CenterChildVertically(btPadlock);
             //   btPadlock.MouseOut += btPadlock_MouseOut;
                btPadlock.ID = UIComponent.DataControlID.StandingOrderModePadlock;

                /*
                UIComponent standingHotspot = new UIComponent(Interface.gui);
                item.Add(standingHotspot);
                standingHotspot.Position = new Point(produceColumnX - 16, 0); 
                standingHotspot.Width = 16;
                standingHotspot.Height = item.Height;
                standingHotspot.MouseOver += standingHotspot_MouseOver;
                standingHotspot.MouseOut += standingHotspot_MouseOut;
               // standingHotspot.EventArgs = eventArgs;
                standingHotspot.ID = UIComponent.DataControlID.StandingOrderModeHotspot;

                ImageButton btPadlock = new ImageButton(Interface.gui);
                standingHotspot.Add(btPadlock); //  item.Add(tbTracking);
                btPadlock.Init(ImageButtonType.HUDPadlock);
                btPadlock.Position = new Point(3, 0);          
                btPadlock.Click += btPadlock_Click;
                btPadlock.Visible = false;
                btPadlock.ToolTip = InventoryPanel.btPadlockTooltip;              
                item.CenterChildVertically(btPadlock);
                btPadlock.MouseOut += btPadlock_MouseOut;*/
            }
         
            FillableBar fillableBar = new FillableBar(gui, FillableBar.FillableBarType.HUDSliderWhite, false, true, GameData.Instance.GUIConstants.TimeBetweenSliderButtonIncrements, GameData.Instance.GUIConstants.SliderButtonDelay) { };
            item.Add(fillableBar);
            fillableBar.ID = UIComponent.DataControlID.CurrentOrders;
            fillableBar.Width = 138; // 110;          
            fillableBar.X = produceColumnX;
          //  fillableBar.EventArgs = eventArgs;
           // fillableBar.MaxValue = available;
           // fillableBar.Value = currentOrder;
            fillableBar.SliderMouseDown += new EventHandler(fillableBar_SliderMouseDown);
            fillableBar.Tag1 = entityType; 
            fillableBar.Y = 4;
            fillableBar.UpdateSliderPosition();
           // fillableBar.SetTags();

          /*  HorizontalList hzNotAttainable = new HorizontalList(Interface.gui);
            item.Add(hzNotAttainable);
            hzNotAttainable.ID = UIComponent.DataControlID.NotAttainableIcons;
            hzNotAttainable.X = produceColumnX;
            hzNotAttainable.Height = 21;
            item.CenterChildVertically(hzNotAttainable);

            HorizontalList hzAttainable = new HorizontalList(Interface.gui);
            item.Add(hzAttainable);
            hzAttainable.ID = UIComponent.DataControlID.AttainableIcons;
            hzAttainable.X = produceColumnX;
            hzAttainable.Height = 21;
            item.CenterChildVertically(hzAttainable);
            */

            item.OrderByTag2 = entityType.PluralName; //We want to order the items by name


            return item;

        }
        /*
        void standingHotspot_MouseOut(UIComponent sender, MouseEventArgs args)
        {
            ImageButton btPadlock = sender.Controls[0] as ImageButton;

            if (!btPadlock.IsChecked)
            {
                InventoryPanel.ShowHidePadlockButton(sender, false);
            }
        }

        void standingHotspot_MouseOver(UIComponent sender, MouseEventArgs args)
        {
            InventoryPanel.ShowHidePadlockButton(sender, true);
        }

        void btPadlock_MouseOut(UIComponent sender, MouseEventArgs args)
        {
            UIComponent hotspot = sender.Parent;
            if (!((ImageButton)sender).IsChecked
                && !hotspot.CheckCoordinates(args.Position.X, args.Position.Y))
            {
                InventoryPanel.ShowHidePadlockButton(hotspot, false);
            }
        }
        */

        void btPadlock_Click(UIComponent sender, EventArgs e)
        {
            Expedition expedition = The.Sim.PlaySite.GetFirstPlayerExpedition();
            if (expedition == null)
                return;

            EntityGroup owner = expedition.OwnedEntities;
            ProductionTargetEventArgs prodArgs = e as ProductionTargetEventArgs;

            ImageButton btPadlock = sender as ImageButton;
            if (btPadlock.IsChecked)
            {
                btPadlock.ToolTip = ProductionOrderControl.btPadlockEnabledTooltip;
            }
            else
            {
                btPadlock.ToolTip = ProductionOrderControl.btPadlockTooltip;
            }

            UIComponent row = sender.Parent; //sender.Parent.Parent;

            UpdateRow(row, (EntityType)row.Tag1, (bool)row.Tag2, owner);
        }


        void tbItems_Click(UIComponent sender, EventArgs e)
        {
            // open the items panel
            EntityGroup expeditionOwner = The.Sim.PlaySite.GetFirstPlayerExpedition().OwnedEntities;

            The.InGameUI.EntityListWindow.PopulateAndShowOnPlayfield(sender, ((ItemTypeButtonEventArgs)e).Item, expeditionOwner); //, null);

            //The.InGameUI.EntityListWindow.ShowOnPlayfield(sender.AbsolutePosition.X + 26, sender.AbsolutePosition.Y);

        }

      
        void fillableBar_SliderMouseDown(object sender, EventArgs e)
        {
            // don't refresh the slider posiiton once the user has touched it:
            SetUserChangedSliderState((FillableBar)sender); //e);


        }

        void fillableBar_SliderMouseUp(object sender, EventArgs e)
        {
            // register the fact that the user pulled the slider, so we don't overwrite his changes during refresh
            SetUserChangedSliderState((FillableBar)sender); //e);


            // save the changes
            //SaveJobChanges(resourceType, args.Button fillableBar.Value);
        }

        private void SetUserChangedSliderState(FillableBar control) //EventArgs e)
        {
            if (!userChangedData.ContainsKey(control))
            {
                userChangedData.Add(control, true);
            }

        }

        /// <summary>
        /// grid with categories
        /// </summary>
        /// <param name="gui"></param>
        /// <param name="outerGrid"></param>
        /// <param name="setCollapsedSummary"></param>
        /// <param name="clickHandler"></param>
        /// <param name="dictionary"></param>
     /**   private static void PopulateGatherResourcesGrid(GUIManager gui, Grid outerGrid,            
            Interface.FullLCDPanel.SetCollapsedSummary setCollapsedSummary,
            ClickHandler clickHandler,
            Dictionary<ResourceType, ResourcesAndJobs> dictionary)
           //  Dictionary<ResourceType, Tuple<int, int>> dictionary)          
        {
            outerGrid.BeginAddingEntries();


            CollapsablePanel cpCategory;
            Grid categoryGrid = null;
            UIComponent item = null;

            List<Grid> categoryGrids = new List<Grid>();

            UIComponent itemComponent;
            int xValueColumn = 195;

            string value1, value2;

            // this stores the panels when Summary has been reset.
            List<CollapsablePanel> initializedPanels = new List<CollapsablePanel>();

            // outer level is an item "category"
            // add nested grids for each, containing item types...
            // foreach (KeyValuePair<T, List<TE>> kvp in dictionary)
            foreach (var kvp in dictionary)
            {
                // see if the category is represented:
                if (outerGrid.TryGetItem(kvp.Key.Category, out item)) // cpCategory))
                {
                    cpCategory = (CollapsablePanel)item;
                    if (!initializedPanels.Contains(cpCategory))
                    {
                        cpCategory.Summary = ""; // clear the summary of the data of the previous entity!
                        initializedPanels.Add(cpCategory);
                    }

                    categoryGrids.Clear();
                    cpCategory.ExpandedPanel.FindChildOfType<Grid>(null, categoryGrids);
                    categoryGrid = categoryGrids[0];
                }
                else
                {
                    // create the category node
                    
                    cpCategory = new CollapsablePanel(gui, CollapsablePanel.PanelType.HUD); // CollapsablePanel.PanelType.Node);
                    cpCategory.HeadingYPos = 4;
                    cpCategory.CollapsedHeight = outerGrid.ItemHeight;
                    outerGrid.AddEntry(kvp.Key.Category, cpCategory);
                    cpCategory.Init(); //CollapsablePanel.PanelType.Node);
                                                
                    

                    cpCategory.Title = kvp.Key.Category.Name;

                    cpCategory.Width = outerGrid.Width;


                    categoryGrid = new Grid(gui, ListBoxType.Main, WindowSystem.Label.LabelType.HUDWindow);
                    categoryGrid.IsOuterGrid = false;

                    // categoryGrid.Position = new Point(
                    // categoryGrid.DebugTag = "categoryGrid";

                    categoryGrid.FixedItemHeights = true; // false;
                    categoryGrid.Width = cpCategory.Width; // make grid fill the collapsable panel
                    cpCategory.AddContent(categoryGrid); // .ExpandedPanel.Add(categoryGrid);

                    categoryGrid.ScrollBarEnabled = false;
                    categoryGrid.ItemHeight = 22;
                    categoryGrid.CanGrowInHeight = true;
                    categoryGrid.Font = GUIManager.LCDInterfaceFontPath;


                    // cpCategory.IsExpanded = true; // DEBUGGING!

                    // categoryGrid.BeginAddingEntries();
                    // touchedCategoryGrids.Add(categoryGrid);
                }


                categoryGrid.BeginAddingEntries();

                value1 = (kvp.Value.NumberOfResources - kvp.Value.NumberOfJobsInOtherZones).ToString();
                value2 = kvp.Value.NumberOfJobsInZone.ToString();

                // see if the item is represented:   
                if (categoryGrid.TryGetItem(kvp.Key, out item)) // grdSkills.TryGetItem(kvp.Key.SkillCategory, out item)) // cpCategory))
                {
                    // update the resource value label
                    itemComponent = item.FindChildById("value1");
                    if (itemComponent != null)
                    {                        
                        Label lblValue = (Label)itemComponent;
                        lblValue.Text = value1;
                       // cpCategory.RightJustifyLabel(lblValue);
                    }

                    // update the jobs label
                    itemComponent = item.FindChildById("value2");
                    if (itemComponent != null)
                    {
                        
                        Label lblValue = (Label)itemComponent;
                        lblValue.Text = value2;
                       // cpCategory.RightJustifyLabel(lblValue);
                    }

                    // update the button event argument:
                    itemComponent = item.FindChildById("button");
                    if (itemComponent != null)
                    {
                        ImageButton button = (ImageButton)itemComponent;
                        ((HarvestJobsButtonEventArgs)itemComponent.EventArgs).Item = kvp.Key;
                        ((HarvestJobsButtonEventArgs)itemComponent.EventArgs).ResourcesAndJobs = kvp.Value;
                    }

                }
                else
                {
                    // add the item and the controls                  
                    if (clickHandler != null)
                    {
                        categoryGrid.AddEntryWithCaptionTwoValuesAndButton(kvp.Key, null, kvp.Key.Name, cpCategory.GetPaddingRight(), value1, value2, ImageButtonType.HUDArrowRight, clickHandler, 
                            new HarvestJobsButtonEventArgs(kvp.Key, kvp.Value));
                        //categoryGrid.AddEntryAndButton(kvp.Key, null, kvp.Key.Name, cpCategory.GetPaddingRight(), value, clickHandler, new IGameDataButtonEventArgs(kvp.Key));
                    }                                   

                }

                //**********
                if (setCollapsedSummary != null)
                {
                    setCollapsedSummary(cpCategory, value1);
                }
                //***********
            }

                       

            FullLCDPanel.Cleanup<ResourceType, ResourcesAndJobs, ResourceCategory>(outerGrid, dictionary);

            outerGrid.EndAddingEntries();
        }*/

        private class HarvestJobsButtonEventArgs : EventArgs
        {
            public HarvestJobsButtonEventArgs(/*IGameData*/ object item, MapArea.ResourcesAndJobs resourcesAndJobs)
            {
                this.Item = item;
                this.ResourcesAndJobs = resourcesAndJobs;
            }


            // public IGameData Item;
            public object Item;

            public MapArea.ResourcesAndJobs ResourcesAndJobs;


        }
           
    }
}
