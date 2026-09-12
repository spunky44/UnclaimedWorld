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

namespace UWGame.ClientSide.Interface.HUD_Windows
{
    public class GatherResourcesWindow: HUDWindow
    {
        MapArea mapArea;
        Expedition expedition;

        //TODO: Check if categoryGridKeys should be a tuple instead containing key and object instead of getting the object from the outer grid.
        List<ResourceCategory> categoryGridKeys = new List<ResourceCategory>();

        /// <summary>
        /// contains groups/category grids and headers
        /// </summary>
        Grid outerGrid;


        Label lblName, lblHeader;
        Image headerIcon;

        FullLCDPanel.SetCollapsedSummary SetSummaryDelegate;

       // ClickHandler resourceClickHandler;

       // GatherResourcePopup popup;


        /// <summary>
        /// we store the user changes here before he commits them with 'Ok'
        /// </summary>
        Dictionary<ResourceType, MapArea.ResourcesAndJobs> data = new Dictionary<ResourceType, MapArea.ResourcesAndJobs>();

       // Dictionary<ResourceType, bool> userChangedData = new Dictionary<ResourceType, bool>();
        Dictionary<FillableBar, bool> userChangedData = new Dictionary<FillableBar, bool>();


        int itemTypeIconColumnX = 10; //18;

        bool isFirstUpdate;

        TextButton btAll;
        TextButton btNone;
        TextButton btOK;
        TextButton btCancel;

        public GatherResourcesWindow() 
            :base(364 /*334 */, 275, level: Level.Bottom, isMovable: true)
        {
            
            //popup = new GatherResourcePopup();

            DisplayWindow.SetResizableArea(ResizeAreas.Top, true);
            DisplayWindow.SetResizableArea(ResizeAreas.Bottom, true);        

            DisplayWindow.MinHeight = 200;
            DisplayWindow.ResizableBorderSize = 6;
            DisplayWindow.Resize += DisplayWindow_Resize;

            AddZoneNameAndHeader("", "GATHER", "HUD_icon_gather", doubleSpacing, out lblName, out lblHeader, out headerIcon);

            SetSummaryDelegate = new FullLCDPanel.SetCollapsedSummary(SidePanelEntity.SetSummaryAsTotal);

            CreateGridHeader();
 

            outerGrid = new Grid(gui, ListBoxType.HUDAndLCD, WindowSystem.Label.LabelType.HUDWindow);
            outerGrid.IsOuterGrid = true; // false;
           
            // categoryGrid.Position = new Point(
            // categoryGrid.DebugTag = "categoryGrid";
            outerGrid.X = doubleSpacing;
            outerGrid.Y = 48 + topMargin;
            outerGrid.FixedItemHeights = false; // false;
            outerGrid.Width = DisplayWindow.ViewPort.Width - 2 * doubleSpacing; // listSurface.Width; // make grid fill the panel           
            outerGrid.ScrollBarEnabled = true;
            outerGrid.ItemHeight = 27; // 22;
            outerGrid.CanGrowInHeight = false; // true; // false; // true;            
            outerGrid.Font = GUIManager.LCDandHUDBodyFontPath;
            outerGrid.Height = 136; // 40; // 160


            
            Add(outerGrid);
            
            //listSurface.Add(grdResources);
            
            

            int yPos = DisplayWindow.Height - 22 - bottomMargin; // 162;
            int xPos = sideMargin;


           
            btCancel = new TextButton(gui);
            Add(btCancel);
            btCancel.Text = "CANCEL";
            btCancel.Init(TextButton.TextButtonType.HUD);
            btCancel.Click += new ClickHandler(btCancel_Click);
            btCancel.Width = 72;
          //  btCancel.Y = DisplayWindow.Height - btCancel.Height - doubleSpacing;
            btCancel.X = DisplayWindow.Width - doubleSpacing - btCancel.Width;


            btOK = new TextButton(gui);
            Add(btOK);
            btOK.Text = "OK";
            btOK.Init(TextButton.TextButtonType.HUD);
            btOK.Click += new ClickHandler(btOk_Click);
            btOK.Width = 72;
         //   btOK.Y = DisplayWindow.Height - btOK.Height - doubleSpacing;
            btOK.X = btCancel.X - 2 - btOK.Width;
                     
            
            btNone = new TextButton(gui);
            Add(btNone);
            btNone.Text = "None";
            btNone.Init(TextButton.TextButtonType.HUDHasState);
            btNone.Click += new ClickHandler(btGatherNone_Click);
         //   btNone.Y = 195; // btAll.Y;
            btNone.X = 127; // btAll.Right + 2;
            btNone.ToolTip = "Cancel all gathering";
            //btNone.ScaleToFitText();
            btNone.Width = buttonWidth;
            btNone.CheckedMode = CheckedModes.CannotBeChecked;

            btAll = new TextButton(gui);
            Add(btAll);
            btAll.Text = "All";          
            btAll.Init(TextButton.TextButtonType.HUDHasState);
            btAll.Click += new ClickHandler(btGatherAll_Click);
        //    btAll.Y = btNone.Y; // 195;
            btAll.X = btNone.Right + 2; //103;
            btAll.ToolTip = "Gather all resources";
            //  btAll.ScaleToFitText();
            btAll.Width = buttonWidth;
            btAll.CheckedMode = CheckedModes.CannotBeChecked;


            SetVerticalPositions();
        }

        private void SetVerticalPositions()
        {            
            btCancel.Y = DisplayWindow.Height - btCancel.Height - doubleSpacing;        
            btOK.Y = DisplayWindow.Height - btOK.Height - doubleSpacing;
            btNone.Y = btOK.Y - 43; // 195; //
            btAll.Y = btNone.Y; // 195;

            outerGrid.Height = btNone.Y - 13 - outerGrid.Y; 

        }

        void DisplayWindow_Resize(UIComponent sender)
        {
            SetVerticalPositions();
        }

       // private const int resourceTextX = 32 + sideMargin;
       // private const int inStockX = 101 + sideMargin;
        private const int orderedX = 175; // 175;
        private const int regrowthX = 290;
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
            lblSliderHeading.Text = "Ordered / available";
            lblSliderHeading.FitToText();
            lblSliderHeading.X = orderedX;
            lblSliderHeading.Y = gridHeaderY;

            Label lblRegrowthHeading = new Label(gui);
            Add(lblRegrowthHeading);
            lblRegrowthHeading.Init(Label.LabelType.HUDWindow);
            lblRegrowthHeading.Text = "Regrowth";
            lblRegrowthHeading.FitToText();
            lblRegrowthHeading.ToolTip = "The yearly regrowth of each resource in the zone";
            lblRegrowthHeading.X = regrowthX;
            lblRegrowthHeading.Y = gridHeaderY;
        }

        void btGatherAll_Click(UIComponent sender, EventArgs e)
        {
            // set all jobs to their maximum:

            UIComponent itemComponent;
            UIComponent item;
            Grid grid;

            
            foreach (var key in categoryGridKeys)
            {
                if (outerGrid.TryGetEntry(key, out item))
                {
                    
                    (item as Grid).TryGetEntry(gridKey, out item);
                    grid = item as Grid;

                    foreach (var row in grid.EntriesByKey)
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
                }
                
            }         
        }

        void btCancel_Click(UIComponent sender, EventArgs e)
        {
            Hide();
        }

        void btOk_Click(UIComponent sender, EventArgs e)
        {           
            bool userChangedData = false;

            EntityGroupID groupID = expedition.OwnedEntities.ID;
            Zone selectedZone = mapArea.Zone; // The.InGameUI.SelectedZone;

            //EntityGroup owner = mapArea.GetOwner(); 

            // we don't really need the data dictionary now that the sliders are all on the main form instead of in a popup... but let's keep it for now.
            // retrieve the data:

            Grid grid;
            UIComponent item;
         //   List<Command> standingOrders = null;

            foreach (var key in categoryGridKeys)
            {
                if (outerGrid.TryGetEntry(key, out item))
                {
                   
                    (item as Grid).TryGetEntry(gridKey, out item);
                    grid = item as Grid;

                    foreach (var row in grid.EntriesByKey)
                    {
                        FillableBar slider = row.Value.FindChildById(UIComponent.DataControlID.CurrentOrders) as FillableBar;
                        ImageButton btStanding = null;
                        if (GameData.Instance.GUIConstants.EnableStandingOrders)
                        {
                           // btStanding = row.Value.FindChildById(UIComponent.DataControlID.StandingOrderModeHotspot).Controls[0] as ImageButton;
                            btStanding = row.Value.FindChildById(UIComponent.DataControlID.StandingOrderModePadlock) as ImageButton;
                        }

                        ResourceType resourceType = (ResourceType)row.Key;
                        EntityType entityType = resourceType.ResourceItemType;

                        if (GameData.Instance.GUIConstants.EnableStandingOrders && btStanding.IsChecked)
                        {
                            if (expedition.OwnedEntities.ProductionOrders.Orders[entityType].AmountToKeepInStore != slider.Value)
                            { 
                                SetStandingOrder newOrder = new SetStandingOrder(expedition.ID, entityType.KeyName, slider.Value, true);
                                The.Client.Controller.StoreAndExecuteCommand(newOrder);       
                             
                            }

                            // TODO: create/use the same zone..execute right away (see below)

                            if (mapArea.Zone == null || !mapArea.Zone.AllowStandingOrderHarvest.Contains(resourceType))
                            {
                                if (slider.Value > 0)
                                {
                                    // enable
                                    SetStandingOrderGatherInZone enableCommand;
                                    if (selectedZone != null)
                                    {
                                        enableCommand = new SetStandingOrderGatherInZone(selectedZone.ID, true, resourceType.KeyName,  true, groupID);
                                    }
                                    else
                                    {
                                        enableCommand = new SetStandingOrderGatherInZone(mapArea, true, resourceType.KeyName, true, groupID);
                                    }
                                    The.Client.Controller.StoreAndExecuteCommand(enableCommand);        
                                }
                                else
                                {
                                    // disable
                                    SetStandingOrderGatherInZone disableCommand;
                                    if (selectedZone != null)
                                    {
                                        disableCommand = new SetStandingOrderGatherInZone(selectedZone.ID, true, resourceType.KeyName, false, groupID);
                                    }
                                    else
                                    {
                                        disableCommand = new SetStandingOrderGatherInZone(mapArea, true, resourceType.KeyName, false, groupID);
                                    }
                                 
                                    The.Client.Controller.StoreAndExecuteCommand(disableCommand);           
                                }
                            }
                        }
                        else
                        {
                            MapArea.ResourcesAndJobs rowData = data[(ResourceType)row.Key];

                            if (rowData.NumberOfJobsInZone != slider.Value)
                            {
                                userChangedData = true;

                                rowData.NumberOfJobsInZone = slider.Value;
                                rowData.UserChangedData = true;

                                data[resourceType] = rowData;
                            }

                            if (mapArea.Zone != null && mapArea.Zone.AllowStandingOrderHarvest.Contains(resourceType))
                            {
                                SetStandingOrderGatherInZone disableCommand;
                                if (selectedZone != null)
                                {
                                    disableCommand = new SetStandingOrderGatherInZone(selectedZone.ID, true, resourceType.KeyName, false, groupID);
                                }
                                else
                                {
                                    disableCommand = new SetStandingOrderGatherInZone(mapArea, true, resourceType.KeyName, false, groupID);
                                }
                             
                                The.Client.Controller.StoreAndExecuteCommand(disableCommand);                       
                

                              //  Common.AddToList(ref standingOrders, disableCommand);
                            }
                        }

                        // make sure we are up to date:
                        selectedZone = The.InGameUI.SelectedZone;                        
                    }
                }
            }


           /* if (standingOrders != null)
            {
                foreach (var order in standingOrders)
                {
                    The.Client.Controller.StoreAndExecuteCommand(order);
                }
            }*/

           

            // if the data has been changed, create commands as necessary:
            if (userChangedData) // data.Any(r => r.Value.UserChangedData == true))
            {
                selectedZone = The.InGameUI.SelectedZone;

                int currentNoOfJobs;
                int jobDifference;

                
                ProcessType processType;
                
                // for each resource type:
                foreach (var dataItem in data)
                {
                    if (dataItem.Value.UserChangedData)
                    {
                        if (selectedZone != null)
                        {
                            currentNoOfJobs = selectedZone.MapArea.GetNoOfJobs(dataItem.Key); // jobsInZone[dataItem.Key].Count; //.NumberOfJobsInZone;
                        }
                        else
                        {
                            currentNoOfJobs = 0;
                        }

                        jobDifference = dataItem.Value.NumberOfJobsInZone - currentNoOfJobs;


                        processType = GameData.Instance.ProcessYieldsThisOutput[dataItem.Key.ResourceItemType].FirstOrDefault(p => p.IsGathering); //[0];

                        Command gather;
                        if (selectedZone != null)
                        {
                            gather = new Gather(selectedZone.ID, true, jobDifference, dataItem.Key.ResourceItemType.KeyName, processType.KeyName,
                                dataItem.Key.KeyName, groupID);
                        }
                        else
                        {
                            gather = new Gather(The.InGameUI.SelectedTiles, true, jobDifference, dataItem.Key.ResourceItemType.KeyName, processType.KeyName,
                                dataItem.Key.KeyName, groupID);
                        }

                        The.Client.Controller.StoreAndExecuteCommand(gather); // will select/create the zone also.

                        // make sure we are up to date:
                        selectedZone = The.InGameUI.SelectedZone;

                    }
                }

            }

            Hide();
        }
            

       

        void btGatherNone_Click(UIComponent sender, EventArgs e)
        {
            // clear all jobs:
            UIComponent itemComponent;

            Grid grid;
            UIComponent item;
            foreach (var key in categoryGridKeys)
            {
                if (outerGrid.TryGetEntry(key, out item))
                {
                    
                    (item as Grid).TryGetEntry(gridKey, out item);
                    grid = item as Grid;

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
                }

            }

            

            /*
           
            ResourcesAndJobs dataItem;

            List<ResourceType> keys = new List<ResourceType>(data.Keys);
            for (int i = 0; i < keys.Count; i++)
            {
                ResourceType key = keys[i];

                dataItem = data[key];
                dataItem.NumberOfJobsInZone = 0;
                dataItem.UserChangedData = true;

                data[key] = dataItem;
               
            }
            */
          /*  foreach (var item in data)
            {
                if (data.TryGetValue(item.Key, out dataItem))
                {
                    dataItem.NumberOfJobs = 0;
                    dataItem.UserChangedData = true;

                    data[item.Key] = dataItem;
                }
            }

            // show the changes:
            PopulateGatherResourcesGridNew(//gui, outerGrid,
                //SetSummaryDelegate, 
                resourceClickHandler,
                data);*/
        }

        public void Hide()
        {
           
            DisplayWindow.Hide();
           // popup.DisplayWindow.Hide();
        }


        

        public override void Refresh()
        {         
            data.Clear();

          //  MapArea mapArea = TileSelectionContextMenu.GetMapArea();            

            // if in a zone, only count jobs in our zone.
            // resources taken by jobs claimed by other zones are not available  
            // jobs in our zone: 0, jobs in other zones: 5, total: 8

            mapArea.GetSumOfAllResourcesInArea(The.InGameUI.UIAllegiance.SharedKnowledge, data);

            mapArea.GetSumOfAllHarvestJobsInArea(data);
                        
            // NEW: add entries for standing orders for depleted resources. they might spawn again, so keep showing them
            GetStandingOrdersForDepletedResources();

            Populate(); //mapArea);

          /*  FullLCDPanel.PopulateGatherResourcesGrid<ResourceType, int, ResourceCategory>(gui, outerGrid, 
                null, SetSummaryDelegate, resourceClickHandler, 
                numberOfResources);

          */
        }

        private void GetStandingOrdersForDepletedResources()
        {
            if (mapArea.Zone != null)
            {
                foreach (var item in mapArea.Zone.AllowStandingOrderHarvest)
                {
                    if (!data.ContainsKey(item))
                    {
                        data.Add(item, new MapArea.ResourcesAndJobs()); // add an empty item
                    }
                }

            }

        }

        const string gridKey = "Grid";

        /// <summary>
        /// grid without categories
        /// </summary>
        /// <param name="gui"></param>
        /// <param name="outerGrid"></param>
        /// <param name="setCollapsedSummary"></param>
        /// <param name="clickHandler"></param>
        /// <param name="dictionary"></param>
        private void Populate() //MapArea mapArea)
        {

            UIComponent item = null;
         
            int available, currentOrder, noOfAvailableItems;

            EntityGroup owner = mapArea.GetOwner(); // TileSelectionContextMenu.GetMapAreaOwner(mapArea);

            if (owner == null)
            {
                // oops, what should happen here..? clear the grid?
                outerGrid.Clear();
                return; 
            }

            Grid grid = null;

            int currentNoOfCategories = outerGrid.Entries.Count;

            foreach (var kvp in data)
            {
                /*
                 * If we do not already have a grid with this category we want to create one
                 */
                if (!outerGrid.TryGetEntry(kvp.Key.Category, out item))
                {
                    /*
                     * The Inner grid will contain
                     * A label that is the category header header 
                     * A grid containing the resources that has this category 
                    */
                    CreateInnerGrid(kvp.Key.Category);
                    
                }
                else
                {
                    outerGrid.TryGetEntry(kvp.Key.Category, out item);
                    (item as Grid).TryGetEntry(gridKey, out item);
                    grid = item as Grid;
                }



                available = GetAvailableResources(kvp.Value);

                currentOrder = kvp.Value.NumberOfJobsInZone;
              /*  noOfAvailableItems = InventoryPanel.GetNoOfAvailableEntities(owner.AllEntities, owner, kvp.Key.ResourceItemType, out noOfIncompleteEntities,
                                                                                                              out noOfEntitiesUsedAsParts, out noOfItemsOffSite, out noOfItemsOwnedByOthers); 
                */
                // see if the item is represented:   

                outerGrid.TryGetEntry(kvp.Key.Category, out item);
                (item as Grid).TryGetEntry(gridKey, out item);
                grid = item as Grid;
                grid.BeginAddingEntries();
                

                if (!grid.TryGetEntry(kvp.Key, out item)) 
                {
                    item = AddRow(kvp.Key, kvp.Key.Name, /*noOfAvailableItems, available,*/ currentOrder, kvp.Value, owner, grid);
                    
                }

                UpdateRow(item, available, currentOrder, owner, kvp.Key, kvp.Value.MaximumRegrowth, kvp.Value.CurrentRegrowth, kvp.Value.MaximumReached);

                grid.EndAddingEntries();
            }

            
            object key;
            for (int index = categoryGridKeys.Count - 1; index >= 0; index--)
            {
                key = categoryGridKeys[index];
                                
                if (outerGrid.TryGetEntry(key, out item))
                {
                    (item as Grid).TryGetEntry(gridKey, out item);
                    grid = item as Grid;
                    grid.Sort(i => i.OrderByTag2, Grid.Sorting.Ascending);
                    CleanUpGrid(grid, key);
                }
                
            }
            

            if (currentNoOfCategories != outerGrid.Entries.Count)
            {
                outerGrid.Sort(i => i.OrderByTag1, Grid.Sorting.Ascending);
            }

            isFirstUpdate = false;
        }

        private static int GetAvailableResources(MapArea.ResourcesAndJobs kvp)
        {
            int available;
            available = (kvp.NumberOfResources - kvp.NumberOfJobsInOtherZones);
            available = Common.ClampBottom(available, 0);
            return available;
        }


        private void CreateInnerGrid(ResourceCategory category)
        {
            Grid innerGrid = new Grid(gui, ListBoxType.HUDAndLCD, WindowSystem.Label.LabelType.HUDWindow);
            innerGrid.Initialize();
            innerGrid.FixedItemHeights = false;
            innerGrid.CanGrowInHeight = true;
            innerGrid.Width = DisplayWindow.ViewPort.Width - 2 * doubleSpacing;
            innerGrid.ScrollBarEnabled = false;
            innerGrid.Font = GUIManager.LCDandHUDBodyFontPath;

            Grid grdResourceCategory = new Grid(gui, ListBoxType.HUDAndLCD, WindowSystem.Label.LabelType.HUDWindow);
            grdResourceCategory.IsOuterGrid = true; // false;
            grdResourceCategory.FixedItemHeights = true; // false;
            grdResourceCategory.Width = DisplayWindow.ViewPort.Width - 2 * doubleSpacing; // listSurface.Width; // make grid fill the panel           
            grdResourceCategory.ScrollBarEnabled = false;
            grdResourceCategory.ItemHeight = 27; // 22;
            grdResourceCategory.CanGrowInHeight = true; // true; // false; // true;            
            grdResourceCategory.Font = GUIManager.LCDandHUDBodyFontPath;
            grdResourceCategory.Height = 136; // 40; // 160

            ResourceCategory categoryKey = category;
            categoryGridKeys.Add(categoryKey);

            AddTextRow(categoryKey.Name, innerGrid);
            innerGrid.OrderByTag1 = category.Name; //We want to sort the grids by the category name
            innerGrid.AddEntry(gridKey, grdResourceCategory);
            outerGrid.AddEntry(categoryKey, innerGrid);
        }


        private void CleanUpGrid(Grid grid, Object key)
        {   
            // remove resource rows that no longer appear in the data source:
            grid.DeleteEntries<ResourceType>(e => data.ContainsKey(e));

            if (grid.Count == 0)
            {
                // remove empty category grids:
                outerGrid.RemoveEntry(key);
                categoryGridKeys.Remove(key as ResourceCategory);
            }
        }


        private void UpdateRow(UIComponent item, int available, int currentOrder, EntityGroup owner, ResourceType resourceType, float? maxRegrowth, float? currentRegrowth, bool maxReached) //, MapArea.ResourcesAndJobs Value)
        {
            EntityType entityType = resourceType.ResourceItemType;

            int noOfIncompleteEntities;
            int noOfEntitiesUsedAsParts;
            int noOfItemsOffSite;
            int noOfItemsOwnedByOthers;
            int noOfAvailableItemsIncludingIntrinsic;

            int noOfAvailableItems = InventoryPanel.GetNoOfAvailableEntities(owner.AllEntities, owner, entityType, out noOfIncompleteEntities,
                                                                                                              out noOfEntitiesUsedAsParts, out noOfItemsOffSite, out noOfItemsOwnedByOthers, out noOfAvailableItemsIncludingIntrinsic); 
                
            UIComponent itemComponent;
            itemComponent = item.FindChildById(UIComponent.DataControlID.Stock);
            if (itemComponent != null)
            {
                TextButton tbItems = (TextButton)itemComponent;
                tbItems.Text = noOfAvailableItems.ToString();
                tbItems.Enabled = noOfAvailableItems > 0;
                // cpCategory.RightJustifyLabel(lblValue);
            }

            // update prod. ability
            bool hasTools, hasInputs, hasSkills, hasResources, hasSpecialSite, hasPolicy;
            EntityType immovableInput;
            int maxAmountThatCanBeProduced;
            int? noOfMissingInputTypes, noOfAvailableInputTypes, outputBatchAmount;
            ProcessType processType;

            
            bool canProduce = InventoryPanel.GetBestProcessForDisplay(entityType, owner, out hasInputs, out hasTools,
                out maxAmountThatCanBeProduced, out noOfMissingInputTypes, out noOfAvailableInputTypes, out hasSkills, out hasResources, out hasSpecialSite, out hasPolicy, out immovableInput, out outputBatchAmount, out processType, false,
                null,
                null,
                p => p.IsGathering == true); // only allow gather processes

            DataTypeButton tbCaption = (DataTypeButton)item.FindChildById(UIComponent.DataControlID.Caption);

            tbCaption.SetAvailableStatusColor(noOfAvailableItems > 0);
            

            // update the slider:
                      
            FillableBar fillableBar = item.FindChildById(UIComponent.DataControlID.CurrentOrders) as FillableBar;        

          //  UIComponent hotspot = null;
            ImageButton btStandingOrder = null;
            if (GameData.Instance.GUIConstants.EnableStandingOrders)
            {
              //  hotspot = item.FindChildById(UIComponent.DataControlID.StandingOrderModeHotspot);
              //  btStandingOrder = hotspot.Controls[0] as ImageButton;

                btStandingOrder = item.FindChildById(UIComponent.DataControlID.StandingOrderModePadlock) as ImageButton;
            }

            ProductionOrder order;
            if (owner.ProductionOrders.Orders.TryGetValue(entityType, out order)) // all Items have an entry.
            {
                if (GameData.Instance.GUIConstants.EnableStandingOrders)
                {
                    if (isFirstUpdate)
                    {
                        if (mapArea.Zone != null)
                        {
                            if (isFirstUpdate)
                            {
                                ProductionOrderControl.SetPadlockButtonState(mapArea.Zone.AllowStandingOrderHarvest.Contains(resourceType) && order.AmountToKeepInStore.HasValue,
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
                        UpdateDirectOrders(available, currentOrder, canProduce, fillableBar, btStandingOrder);
                    }
                }
                else
                {
                    UpdateDirectOrders(available, currentOrder, canProduce, fillableBar, null);
                }

            }


            HorizontalList hzNotAttainable = item.FindChildById(UIComponent.DataControlID.NotAttainableIcons) as HorizontalList;
            HorizontalList hzAttainable = item.FindChildById(UIComponent.DataControlID.AttainableIcons) as HorizontalList;

            if (fillableBar.Visible == false)
            {
                Dictionary<ProcessType, AttainableInfo> attainableInfo = null;
                if (!canProduce)
                {
                    attainableInfo = The.InGameUI.InventorySettings.GetAttainableInfo(resourceType.ResourceItemType);
                }

                ProductionOrderControl.UpdateAttainable(attainableInfo, hzAttainable, hzNotAttainable,
                    hasTools, hasInputs, hasSkills, hasResources, canProduce, processType);
            }
            else
            {
                hzAttainable.Visible = false;
                hzNotAttainable.Visible = false;
            }


            Label lblRegrowth = item.FindChildById(UIComponent.DataControlID.Regrowth) as Label;
            if (resourceType.CanReplenish())
            {
                lblRegrowth.Visible = true;

                if (currentRegrowth.HasValue)
                {
                    string currentRegrowthText;
                   
                    string tt;
                    if (maxReached)
                    {
                        currentRegrowthText = "max."; // currentRegrowth.Value + "/" + maxRegrowth.Value;
                        tt = "0 (at max.)";
                    }
                    else
                    {
                        currentRegrowthText = string.Format("+{0:N1}", currentRegrowth.Value); // currentRegrowth.Value + "/" + maxRegrowth.Value;
                        tt = string.Format("{0:N1}", currentRegrowth.Value);
                    }

                    lblRegrowth.Text = currentRegrowthText;
                    lblRegrowth.FitToText();

                    StringBuilder tooltip = new StringBuilder();
                    Common.AppendHeaderOnLightBG(tooltip, "Regrowth rate");
                    Common.AppendFormat(tooltip, "Shows the expected yearly regrowth of {0} in the zone.", false, resourceType.Name);
                    Common.AppendDividerOnOwnLine(tooltip);
                    //Common.AppendLine(tooltip);
                    Common.Append(tooltip, "Maximum regrowth per year: ");
                    Common.AppendFormat(tooltip, "{0:N1}", true, maxRegrowth.Value);
                    Common.AppendLine(tooltip);
                    Common.Append(tooltip, "Current regrowth per year: ");
                    Common.Append(tooltip, tt, true);
                    //Common.AppendFormat(tooltip, "{0:N1}", true, currentRegrowth.Value);

                    lblRegrowth.ToolTip = tooltip.ToString();                    
                }
            }
            else
            {
                lblRegrowth.Visible = false;
            }

        }



        private void UpdateStandingOrderProduction(ProductionOrder stockTarget, FillableBar fillableBar, ImageButton btStandingOrder)
        {
            fillableBar.Visible = true; // can always set orders

            int currentOrder = stockTarget.AmountToKeepInStore ?? 0;

            //fillableBar.Color = Color.Cornsilk;
            fillableBar.ColorAllControls = GameData.Instance.GUIConstants.StandingOrderTint;
            btStandingOrder.NormalColor = GameData.Instance.GUIConstants.StandingOrderTint;
 
            // don't limit the slider based on inputs. use a fixed maximum, like 50...

            bool sliderValuesWereChanged = false;

            fillableBar.StepSize = 1;

            // need to update maxValue first: #SLIDERFIX
            if (fillableBar.MaxValue != GameData.Instance.GUIConstants.MaxStandingOrder) //.max InventoryPanel.maxStandingOrder)
            {
                fillableBar.MaxValue = GameData.Instance.GUIConstants.MaxStandingOrder;
                sliderValuesWereChanged = true;
            }

            if (!userChangedData.ContainsKey(fillableBar)) //  don't overwrite user changes
            {
                if (fillableBar.Value != currentOrder)
                {
                    fillableBar.Value = currentOrder;
                    sliderValuesWereChanged = true;
                }
            }           


            if (sliderValuesWereChanged)
            {
                fillableBar.UpdateSliderPosition();
            }
        }

        private void UpdateDirectOrders(int available, int currentOrder, bool canProduce, FillableBar fillableBar, ImageButton btStandingOrder)
        {
            HuntWindow.SetNormalTint(fillableBar, btStandingOrder);
            /*
            fillableBar.ColorAllControls = UIComponent.HUDTint;

            if (btStandingOrder != null)
            {
                btStandingOrder.NormalColor = UIComponent.HUDTint;
            }*/

          
            bool sliderValuesWereChanged = false;
            if (fillableBar.MaxValue != available)
            {
                sliderValuesWereChanged = true;
                fillableBar.MaxValue = available;
            }

            if (!userChangedData.ContainsKey(fillableBar)) //  don't overwrite user changes
            {
                if (fillableBar.Value != currentOrder)
                {
                    fillableBar.Value = currentOrder;
                    sliderValuesWereChanged = true;
                }
            }

          

            if (sliderValuesWereChanged)
            {
                fillableBar.UpdateSliderPosition();
            }


            if (!canProduce)
            {
                fillableBar.Visible = false;
            }
            else
            {
                fillableBar.Visible = true;
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

        private void AddTextRow(string textRow, Grid gridToAddTo) // string value1, string value2)
        {

            //UIComponent item = new UIComponent(gui);


            Label lblText = new Label(gui);
            //item.Add(lblText);
            lblText.Init(Label.LabelType.HUDWindow);
            lblText.Text = textRow; // "GATHER";
            lblText.FitToText();
            lblText.X = 10; //(int)(0.5f * (DisplayWindow.Width -lblHeader.Width - 2 * sideMargin));
            lblText.Y = 0;
            string key = textRow;

            
            gridToAddTo.AddEntry(key, lblText);
        }

        private UIComponent AddRow(object key, string caption, /*int inStock, int available,*/ int currentOrder,
            MapArea.ResourcesAndJobs resourcesAndJobs, EntityGroup owner, Grid grid) 
        {
            EntityType entityType = ((ResourceType)key).ResourceItemType;
            EventArgs eventArgs = new HarvestJobsButtonEventArgs(key, resourcesAndJobs);

            UIComponent item = new UIComponent(gui);
            grid.AddEntry(key, item);

            int xPos;
            // int width;

            Image icon = InventoryPanel.AddEntityTypeIcon(entityType, item, itemTypeIconColumnX);

            DataTypeButton tbCaption = new DataTypeButton(gui, DataSheet.InfoToShow.Data, entityType, owner.ID, false);
            tbCaption.Init(TextButton.TextButtonType.HUDToolTipWhite);
            tbCaption.ID = UIComponent.DataControlID.Caption;
            tbCaption.IsRoot = true;
            tbCaption.Text = caption;
            item.Add(tbCaption);
            tbCaption.TextAlignment = TextButton.TextAlign.Left;
            tbCaption.Width = 125; // 105; 
            tbCaption.X = 22;
            tbCaption.DebugTag = "entityTypeButton";

            int produceColumnX = orderedX - 11;

            if (GameData.Instance.GUIConstants.EnableStandingOrders)
            {               
                ImageButton btPadlock = new ImageButton(Interface.gui);
                item.Add(btPadlock); //  item.Add(tbTracking);
                btPadlock.Init(ImageButtonType.HUDPadlock);
                btPadlock.Position = new Point(produceColumnX - 13, 0);      
                btPadlock.Click += btPadlock_Click;
              //  btPadlock.Visible = false;
                btPadlock.ToolTip = ProductionOrderControl.btPadlockTooltip;
                btPadlock.ID = UIComponent.DataControlID.StandingOrderModePadlock;               
                item.CenterChildVertically(btPadlock);
              //  btPadlock.MouseOut += btPadlock_MouseOut;

                /*
                UIComponent standingHotspot = new UIComponent(Interface.gui);
                item.Add(standingHotspot);
                standingHotspot.Position = new Point(produceColumnX - 16, 0); // -5); //new Point(itemTypeProductionTargetColumnX + 105 + 20, -5);
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
              //  tbStanding.Tag1 = key;
                //tbStanding.EventArgs = eventArgs;             
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
            fillableBar.EventArgs = eventArgs;
           // fillableBar.MaxValue = available;
            fillableBar.Value = currentOrder;
            fillableBar.SliderMouseDown += new EventHandler(fillableBar_SliderMouseDown);
            fillableBar.Tag1 = key; // ResourceType
            fillableBar.Y = 4;
            fillableBar.UpdateSliderPosition();
          //  fillableBar.SetTags();

            HorizontalList hzNotAttainable = new HorizontalList(Interface.gui);
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


            Label lblRegrowth = new Label(gui);
            item.Add(lblRegrowth);
            lblRegrowth.ID = UIComponent.DataControlID.Regrowth;
            lblRegrowth.Init(Label.LabelType.HUDWindow);         
            lblRegrowth.X = regrowthX;
            item.CenterChildVertically(lblRegrowth);
            lblRegrowth.Y -= 1;

            lblRegrowth.TooltipWidth = 260;

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

            ResourceType resourceType = (ResourceType)row.Tag1;
            int available = GetAvailableResources(data[resourceType]);

            UpdateRow(row, available, 0, owner, resourceType, null, null, false);
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


               
      
        /// <summary>
        /// not currently used - use this for popup slider action
        /// </summary>
        /// <param name="resourceType"></param>
        /// <param name="noOfJobs"></param>
        public void SaveJobChanges(ResourceType resourceType, int noOfJobs)
        {
            MapArea.ResourcesAndJobs dataItem = data[resourceType];
            dataItem.NumberOfJobsInZone = noOfJobs;
            dataItem.UserChangedData = true;

            data[resourceType] = dataItem;

           // MapArea mapArea = TileSelectionContextMenu.GetMapArea();

            // show the changes:
            Populate(); //mapArea);
        }

    }
}
