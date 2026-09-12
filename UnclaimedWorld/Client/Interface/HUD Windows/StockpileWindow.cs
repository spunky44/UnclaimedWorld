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
using UWGame.SimSide.AI;
using UWGame.ClientSide.Interface.Controls;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Entities.Owners;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.XmlCollections;
using UWGame.ClientSide.Interface.Inventory;
using UWGame.SimSide.Commands;

namespace UWGame.ClientSide.Interface.HUD_Windows
{
    public class StockpileWindow: HUDWindow
    {
        Stockpile.TypesOfStockpiles typeOfStockpile;

        /// <summary>
        /// The source can be either an owned structure or a map area/zone
        /// If this is null, the currently selected map area/zone will be the source.
        /// </summary>
        EntityID? structure;
    
      
        protected int itemHeight = 18;

        const int quantityX = 238;
        const int itemTypeIconColumnX = 18; // sideMargin;
        const int captionX = 38;
        //const int allowItemX = 252;
        const int stageIconX = 41;
        const int allowX = 251;
        const int blockX = 337;
               
        Grid outerGrid;


        /// <summary>
        /// cache for each update
        /// </summary>
        private Dictionary<EntityType, InventoryPanel.Availability> allAvailableItems = new Dictionary<EntityType, InventoryPanel.Availability>();


        TextButton tbRemove;
        TextButton btOK;
        TextButton btCancel;

        Label lblName, lblHeader;

        Image headerIcon;

        UIComponent listSurface;

        const int rowHeight = 36;

        public StockpileWindow() 
            :base(448, 500, true, level: Level.Bottom, isMovable: true)
        {
            DisplayWindow.SetResizableArea(ResizeAreas.Top, true);
            DisplayWindow.SetResizableArea(ResizeAreas.Bottom, true);

            DisplayWindow.MinHeight = 200;
            DisplayWindow.ResizableBorderSize = 6;
            DisplayWindow.Resize += DisplayWindow_Resize;

            AddZoneNameAndHeader("", "STOCKPILE", "HUD_icon_stockpile", tripleSpacing, out lblName, out lblHeader, out headerIcon);


            listSurface = new UIComponent(gui);
            Add(listSurface);
            listSurface.X = tripleSpacing;
            listSurface.Y = 50;
            listSurface.Width = DisplayWindow.ViewPort.Width - 2 * tripleSpacing;
            listSurface.Height = 354; 

            outerGrid = CreateOuterGridForCollapsableLists(The.InGameUI.gui, listSurface); 
            outerGrid.ItemHeight = 36;
                      
            
            btCancel = new TextButton(gui);
            Add(btCancel);
            btCancel.Text = "CANCEL";
            btCancel.Init(TextButton.TextButtonType.HUD);
            btCancel.Click += new ClickHandler(btCancel_Click);
            btCancel.Width = 72;
         //   btCancel.Height = buttonHeight;
            btCancel.Y = DisplayWindow.Height - btCancel.Height - tripleSpacing;
            btCancel.X = DisplayWindow.Width - tripleSpacing - btCancel.Width;
            
            btOK = new TextButton(gui);
            Add(btOK);
            btOK.Text = "OK";
            btOK.Init(TextButton.TextButtonType.HUD);
            btOK.Click += new ClickHandler(btOk_Click);
            btOK.Width = 72;
        //    bt.Height = buttonHeight;
            btOK.Y = DisplayWindow.Height - btOK.Height - tripleSpacing;
            btOK.X = btCancel.X - 2 - btOK.Width;
                     
            
            tbRemove = new TextButton(gui);
            Add(tbRemove);
            tbRemove.Text = "Delete";
            tbRemove.ToolTip = "Remove the stockpile";
            tbRemove.Init(TextButton.TextButtonType.HUD);
            tbRemove.Click += new ClickHandler(bt_RemoveClick);
            tbRemove.Y = doubleSpacing;
            tbRemove.X = DisplayWindow.Width - tbRemove.Width - doubleSpacing;
            tbRemove.ScaleWidthToFitText();
         //   tbRemove.Height = buttonHeight;

            int yPos = DisplayWindow.Height - 80;

            SetVerticalPositions();
        }

        

        private void SetVerticalPositions()
        {
            btCancel.Y = DisplayWindow.Height - btCancel.Height - tripleSpacing;
            btOK.Y = btCancel.Y;

            listSurface.Height = btOK.Y - 13 - listSurface.Y;
            outerGrid.Height = listSurface.Height;
         
           // grid.Height = btOK.Y - 13 - grid.Y;

        }

        void DisplayWindow_Resize(UIComponent sender)
        {
            SetVerticalPositions();
        }

        public override void Refresh()
        {
            Fill();           
        }


        private void UpdateCategoryRow(CollapsablePanel cpCategory, Grid categoryGrid, EntityCategory entityCategory, Stockpile stockpile, bool fillUserControls)
        {
            if (fillUserControls)
            {
                bool allSetting;
              
                GetCategorySetting(entityCategory, stockpile, out allSetting); //, out noneSetting);


                ImageButton cbCategory;
                cpCategory.FindChildById(UIComponent.DataControlID.CurrentCategoryOrders, out cbCategory);

                cbCategory.IsChecked = allSetting;

              /*  TextButton tbAll = (TextButton)cpCategory.FindChildById(UIComponent.DataControlID.UpgradeAllow);
                TextButton tbNone = (TextButton)cpCategory.FindChildById(UIComponent.DataControlID.UpgradeProhibit);

                tbAll.IsChecked = allSetting;
                tbNone.IsChecked = noneSetting;*/
            }
        }

        const int expandedPanelMargin = 3;

        private void AddCategoryRow(ref CollapsablePanel cpCategory, ref Grid categoryGrid, EntityCategory entityCategory, Stockpile stockpile)
        {
            bool allowSetting;
           // bool prohibitSetting;
            GetCategorySetting(entityCategory, stockpile, out allowSetting); //, out prohibitSetting);


            cpCategory = new CollapsablePanel(gui, CollapsablePanel.PanelType.StockpileHUD);
           // cpCategory.HeadingYPos = 4;
            cpCategory.CollapsedHeight = outerGrid.ItemHeight;
            outerGrid.AddEntry(entityCategory, cpCategory);
            cpCategory.Init(); //CollapsablePanel.PanelType.Node);
            cpCategory.Title = entityCategory.Name.ToUpper(Config.Culture);
            cpCategory.Width = outerGrid.Width;
            cpCategory.TitleSummaryRightAlignXPos = quantityX + expandedPanelMargin;  
            cpCategory.TitlePositionX = captionX + expandedPanelMargin;        


            Image checkIcon = new Image(gui);
            checkIcon.SetSkinLocation(SkinState.Normal,gui.GUISpriteSheet.GetSourceRectangle("HUD_exclamationmark_parenthesis"));
            checkIcon.ResizeControlToFitImage();
            checkIcon.ToolTip = "Some items in the category have overriding settings.";
            cpCategory.Add(checkIcon);
            checkIcon.X = 256; // allowItemX;
            cpCategory.CenterOnHeader(checkIcon);           
            checkIcon.ID = UIComponent.DataControlID.HasOverridingItemIcon;
            checkIcon.Visible = false;
            

            // add header controls:
            ItemCategoryButtonEventArgs eventArgs = new ItemCategoryButtonEventArgs(entityCategory);

            ImageButton checkbox = new ImageButton(gui);
            cpCategory.Add(checkbox);
            checkbox.Init(ImageButtonType.HUDCheckbox);
            checkbox.CheckedMode = CheckedModes.SwitchCheckedStateOnClick;
            checkbox.Click += cbCategory_Click;
            checkbox.X = 318;
            checkbox.EventArgs = eventArgs;
            checkbox.ToolTip = "Stockpile everything in this category";
            checkbox.ID = UIComponent.DataControlID.CurrentCategoryOrders;
            cpCategory.CenterOnHeader(checkbox);


          /*  RadioGroup radioGroup = new RadioGroup(gui);
            cpCategory.Add(radioGroup);
            radioGroup.Width = 140;// optionsSurfaceGrid.Width;
            radioGroup.Height = rowHeight;
            radioGroup.Position = new Point(allowX + expandedPanelMargin, 0);
         

            TextButton tbAllow = new TextButton(gui);
            tbAllow.Init(TextButton.TextButtonType.HUDHasState);
            tbAllow.CheckedMode = CheckedModes.CanBeChecked;
            //tbAllow.SwitchStateOnClick = false;
            tbAllow.Text = "Allow";
            tbAllow.ToolTip = "Stockpile everything in this category";
            //tbAllow.Width = buttonWidth;
            tbAllow.ScaleWidthToFitText();
            tbAllow.ID = UIComponent.DataControlID.UpgradeAllow;
            tbAllow.EventArgs = eventArgs;
            tbAllow.Click += new ClickHandler(tbCategoryAllow_Click);
            radioGroup.Add(tbAllow);
            tbAllow.X = 0; // allX;            
        //    tbAllow.Height = buttonHeight;
            tbAllow.IsChecked = allowSetting;
            cpCategory.CenterOnHeader(tbAllow);

            TextButton tbProhibit = new TextButton(gui);
            tbProhibit.Init(TextButton.TextButtonType.HUDHasState);
            tbProhibit.CheckedMode = CheckedModes.CanBeChecked;
            //tbProhibit.SwitchStateOnClick = false;
            tbProhibit.Text = "Prohibit";
            tbProhibit.ToolTip = "Prevent stockpiling of anything in this category";
            tbProhibit.ScaleWidthToFitText();
            tbProhibit.ID = UIComponent.DataControlID.UpgradeProhibit;
            tbProhibit.Click += new ClickHandler(tbCategoryProhibit_Click);
            tbProhibit.EventArgs = eventArgs;
            radioGroup.Add(tbProhibit);
            tbProhibit.X = tbAllow.Right;           
         //   tbProhibit.Height = buttonHeight;
            tbProhibit.IsChecked = prohibitSetting;
            cpCategory.CenterOnHeader(tbProhibit);
            */

            categoryGrid = new Grid(gui, ListBoxType.HUDAndLCD, Label.LabelType.HUDWindow);
            // categoryGrid.Position = new Point(
            categoryGrid.DebugTag = "categoryGrid";

            categoryGrid.FixedItemHeights = true; // false;
            categoryGrid.Width = cpCategory.Width; // make grid fill the collapsable panel
            cpCategory.AddContent(categoryGrid); // .ExpandedPanel.Add(categoryGrid);
            
           // categoryGrid.Y = doubleSpacing;
            categoryGrid.VMargin = doubleSpacing - 4; // create space above and below the list of rows

            categoryGrid.ScrollBarEnabled = false;
            categoryGrid.ItemHeight = 22;
            categoryGrid.CanGrowInHeight = true;
            categoryGrid.Font = GUIManager.LCDandHUDBodyFontPath;
            categoryGrid.IsOuterGrid = false;

            // a newly added category gets its user controls filled with the stockpile settings:
            //FillCategoryRow(cpCategory, entityCategory, stockpile);
        }

        void cbItem_Click(UIComponent sender, EventArgs e)
        {
            ImageButton checkbox = sender as ImageButton;

            FillableBar slider;
            sender.Parent.FindChildById(UIComponent.DataControlID.MaxOrders, out slider);
              
            if (checkbox.IsChecked)
            {
                slider.Value = GameData.Instance.GUIConstants.UnlimitedStockpileValue;
                slider.Visible = true;
                slider.UpdateSliderPosition();
            }
            else
            {
                slider.Value = 0;
                slider.UpdateSliderPosition();
                slider.Visible = false;
            }

        }

        void cbCategory_Click(UIComponent sender, EventArgs e)
        {
            ImageButton checkbox = sender as ImageButton;

            if (checkbox.IsChecked)
            {
                // select others:

                EntityCategory category = ((ItemCategoryButtonEventArgs)e).Category;
                CollapsablePanel cpCategory = (CollapsablePanel)outerGrid.EntriesByKey[category];
                
                AllowItems(cpCategory);

                UpdateOverridingSettingIcon(cpCategory, category);

            }
            else
            {
                // deselect others:

                EntityCategory category = ((ItemCategoryButtonEventArgs)e).Category;
                CollapsablePanel cpCategory = (CollapsablePanel)outerGrid.EntriesByKey[category];
                             

                ProhibitItems(cpCategory);


                UpdateOverridingSettingIcon(cpCategory, category);       
            }
        }

       /* void tbCategoryProhibit_Click(UIComponent sender, EventArgs e)
        {
            TextButton tbProhibit = sender as TextButton;
            
            if (tbProhibit.IsChecked)
            {
                // deselect others:
              
                EntityCategory category = ((ItemCategoryButtonEventArgs)e).Category;
                CollapsablePanel cpCategory = (CollapsablePanel)outerGrid.EntriesByKey[category];

                TextButton tbAll = (TextButton)cpCategory.FindChildById(UIComponent.DataControlID.UpgradeAllow);
                tbAll.IsChecked = false;

                ProhibitItems(cpCategory);


                UpdateOverridingSettingIcon(cpCategory, category);                

            }
        }

        void tbCategoryAllow_Click(UIComponent sender, EventArgs e)
        {
            TextButton tbAllow = sender as TextButton;
            
            if (tbAllow.IsChecked)
            { 
                // select others:
                               
                EntityCategory category = ((ItemCategoryButtonEventArgs)e).Category;
                CollapsablePanel cpCategory = (CollapsablePanel)outerGrid.EntriesByKey[category];

                TextButton tbNone = (TextButton)cpCategory.FindChildById(UIComponent.DataControlID.UpgradeProhibit);
                tbNone.IsChecked = false;

                AllowItems(cpCategory);

                UpdateOverridingSettingIcon(cpCategory, category);
                        
            }
        }**/

       
        private static void AllowItems(CollapsablePanel cpCategory)
        {
            // deselect items:
            Grid itemGrid = ((Grid)cpCategory.ExpandedPanel.Controls[0]);

          /*  TextButton tbAllow, tbProhibit;
            foreach (var item in itemGrid.Entries)
            {               
                tbAllow = (TextButton)item.FindChildById(UIComponent.DataControlID.StockpileAllowItem);
                tbAllow.IsChecked = true;

                tbProhibit = (TextButton)item.FindChildById(UIComponent.DataControlID.StockpileProhibitIem);
                tbProhibit.IsChecked = false;
            }*/

            ImageButton cbItem;
            FillableBar slider;
            foreach (var item in itemGrid.Entries)
            {
                cbItem = (ImageButton)item.FindChildById(UIComponent.DataControlID.CurrentOrders);
                cbItem.IsChecked = true;

                item.FindChildById(UIComponent.DataControlID.MaxOrders, out slider);
                slider.Value = GameData.Instance.GUIConstants.UnlimitedStockpileValue;
                slider.Visible = true;
                slider.UpdateSliderPosition();
            }
        }


        private static void ProhibitItems(CollapsablePanel cpCategory)
        {
            // deselect items:
            Grid itemGrid = ((Grid)cpCategory.ExpandedPanel.Controls[0]);

           // ImageButton cbItem;
           // TextButton tbAllow, tbProhibit;
            FillableBar slider;
            foreach (var item in itemGrid.Entries)
            {
                ImageButton cbItem = (ImageButton)item.FindChildById(UIComponent.DataControlID.CurrentOrders);

                cbItem.IsChecked = false;

                item.FindChildById(UIComponent.DataControlID.MaxOrders, out slider);
                slider.Value = 0;
                slider.Visible = false;
                slider.UpdateSliderPosition();

              /*  tbAllow = (TextButton)item.FindChildById(UIComponent.DataControlID.StockpileAllowItem);
                tbAllow.IsChecked = false;

                tbProhibit = (TextButton)item.FindChildById(UIComponent.DataControlID.StockpileProhibitIem);
                tbProhibit.IsChecked = true;*/
            }
        }

        private void DeselectTopLevel()
        {
            /*//TODO Readd none and all button
            btNone.IsChecked = false;
            btAll.IsChecked = false;
            */
        }

        private UIComponent AddItemRow(Grid categoryGrid, /*int inStock, bool isChecked, int max,*/ EntityType entityType, EntityGroup owner)
        {

            UIComponent item;

            item = new UIComponent(gui);
            categoryGrid.AddEntry(entityType, item);

            //  item.DebugTag = "";
            DataTypeButton tbCaption;
            CreateItemGridRow(entityType, GoalEvaluator.GetOwnerID(owner), false, item, DataSheet.InfoToShow.Data, true, true, out tbCaption);

            Label label = new Label(gui);
            item.Add(label);
            label.Init(Label.LabelType.HUDWindow);
            label.ID = UIComponent.DataControlID.Stock;                 
           /* label.Text = inStock.ToString();
            label.FitToText();*/
            item.CenterChildVertically(label);
            label.Y += 2;
           // label.AlignRight(quantityX);

            ItemTypeButtonEventArgs eventArgs = new ItemTypeButtonEventArgs(entityType);

            ImageButton checkbox = new ImageButton(gui);
            item.Add(checkbox);
            checkbox.Init(ImageButtonType.HUDCheckbox);
            checkbox.CheckedMode = CheckedModes.SwitchCheckedStateOnClick;
            checkbox.X = allowX;
            item.CenterChildVertically(checkbox);
            checkbox.EventArgs = eventArgs;
            checkbox.ToolTip = "Check the box to allow the item to be stockpiled. This will override the category setting.";
            checkbox.ID = UIComponent.DataControlID.CurrentOrders;
            checkbox.Click += cbItem_Click;

            FillableBar slider = new FillableBar(gui, FillableBar.FillableBarType.HUDSlider, false, true,
                GameData.Instance.GUIConstants.TimeBetweenSliderButtonIncrements, GameData.Instance.GUIConstants.SliderButtonDelay);
            item.Add(slider);
            slider.X = checkbox.Right + singleSpacing;
            item.CenterChildVertically(slider);
            slider.ID = UIComponent.DataControlID.MaxOrders;
            slider.ShowMaxValueLabelAtEnd = false;
            slider.Width = 140;
           // slider.Value = max;
            slider.MaxValue = GameData.Instance.GUIConstants.UnlimitedStockpileValue; // 99;
            slider.SliderTooltip = "Set the maximum that can be stockpiled";
            slider.ButtonTooltip = "Set the maximum that can be stockpiled";
            slider.MaxSliderValueSymbol = "...";
            slider.MaxSliderValueTooltip = "No limit";

            return item;  
        }

        /*
       
        private void AddItemRow(Grid categoryGrid, int inStock, bool isChecked, EntityType entityType, EntityGroup owner) 
        {
            
            UIComponent item;

            item = new UIComponent(gui);
            categoryGrid.AddEntry(entityType, item);

          //  item.DebugTag = "";
            DataTypeButton tbCaption;
            CreateItemGridRow(entityType, GoalEvaluator.GetOwnerID(owner), false, item, DataTypeTooltip.InfoToShow.Data, true, true, out tbCaption);

            Label label = new Label(gui);
            item.Add(label);
            label.Init(Label.LabelType.HUDWindow);
            label.ID = UIComponent.DataControlID.Stock;
            //label.X = quantityX;          
            label.Text = inStock.ToString();
            label.FitToText();
            item.CenterChildVertically(label);
            label.Y += 2;
            label.AlignRight(quantityX);

            ItemTypeButtonEventArgs eventArgs = new ItemTypeButtonEventArgs(entityType);


            RadioGroup radioGroup = new RadioGroup(gui);
            item.Add(radioGroup);
            radioGroup.Width = 140;// optionsSurfaceGrid.Width;
            radioGroup.Height = rowHeight;
            radioGroup.Position = new Point(allowX, 0);
            radioGroup.NewMemberChecked += new Action<ICanBeChecked, EventArgs>(itemRadioGroup_NewMemberChecked);
            radioGroup.EventArgs = eventArgs;

            TextButton tbAllow = new TextButton(gui);
            tbAllow.Init(TextButton.TextButtonType.HUDHasState);
            tbAllow.CheckedMode = CheckedModes.CanBeChecked;
            //tbAllow.SwitchStateOnClick = false;
            tbAllow.Text = "Allow";
            tbAllow.ToolTip = "Click to allow this item type to be stockpiled. This will override the category setting";
           // tbAllow.Width = buttonWidth;
            tbAllow.ScaleWidthToFitText();
            tbAllow.ID = UIComponent.DataControlID.StockpileAllowItem;
            radioGroup.Add(tbAllow);
            tbAllow.X = 0;          
           // tbAllow.Height = buttonHeight;
            tbAllow.IsChecked = isChecked; // allowSetting;

            TextButton tbProhibit = new TextButton(gui);
            tbProhibit.Init(TextButton.TextButtonType.HUDHasState);
            tbProhibit.CheckedMode = CheckedModes.CanBeChecked;
           // tbProhibit.SwitchStateOnClick = false;
            tbProhibit.Text = "Prohibit";
            tbProhibit.ToolTip = "Click to prevent stockpiling of this item type. This will override the category setting";
            tbProhibit.ScaleWidthToFitText();
            tbProhibit.ID = UIComponent.DataControlID.StockpileProhibitIem;
            radioGroup.Add(tbProhibit);
            tbProhibit.X = tbAllow.Right;        
         //   tbProhibit.Height = buttonHeight;
            tbProhibit.IsChecked = !isChecked; // prohibitSetting;


            categoryGrid.DebugTag = "categoryGrid";
        }*/

        void itemRadioGroup_NewMemberChecked(ICanBeChecked obj, EventArgs e)
        {
            EntityType entityType = ((ItemTypeButtonEventArgs)e).Item;
            CollapsablePanel cpCategory = (CollapsablePanel)outerGrid.EntriesByKey[entityType.Category];

            UpdateOverridingSettingIcon(cpCategory, entityType.Category);
        }

    /*    void tbProhibitItem_Click(UIComponent sender, EventArgs e)
        {
            UpdateOverridingSettingIcon(sender, e);
        }

        void tbAllowItem_Click(UIComponent sender, EventArgs e)
        {
            UpdateOverridingSettingIcon(sender, e);
        }*/


        /// <summary>
        /// the icon gets shown when some items have a setting that differs from the category setting
        /// </summary>      
        private void UpdateOverridingSettingIcon(CollapsablePanel cpCategory, EntityCategory category, Stockpile stockpile = null)
        {
            if (stockpile == null)
            {
                MapArea mapArea = null;
                EntityGroup owner;
                IKnownEntityData structureData;

                if (!GetData(out owner, out structureData, out mapArea, out stockpile))
                {
                    Hide();
                    return;
                }
            }

            Image overridingIcon = (Image)cpCategory.FindChildById(UIComponent.DataControlID.HasOverridingItemIcon);
            bool hiddenItemsAreDifferent;
            if (OneOrMoreItemsInCategoryAreDifferent(cpCategory, category, stockpile, out hiddenItemsAreDifferent))
            {
                overridingIcon.Visible = true;
                if (hiddenItemsAreDifferent)
                {
                    overridingIcon.ToolTip = "Some hidden item types have overriding settings that are different.";
                }
                else
                {
                    overridingIcon.ToolTip = "Some item types have overriding settings that are different.";
                }
            }
            else
            {
                overridingIcon.Visible = false;
            }
        }

      
     
        public enum InfoClickResult { Production, Data }
        public static void CreateItemGridRow(EntityType entityType, EntityGroupID? owner, bool useUIOwner, UIComponent item, DataSheet.InfoToShow infoClickResult, bool usePluralName, bool isRoot, out DataTypeButton entityTypeButton, bool includeIcon = true)
        {
            /*if (includeIcon)
            { */
            Image icon = InventoryPanel.AddEntityTypeIcon(entityType, item, itemTypeIconColumnX);
            icon.ID = UIComponent.DataControlID.Icon;
         
            entityTypeButton = new DataTypeButton(The.InGameUI.gui, infoClickResult, entityType, owner, useUIOwner);
            entityTypeButton.Init(TextButton.TextButtonType.HUDToolTipWhite);
            entityTypeButton.ID = UIComponent.DataControlID.Caption;
            entityTypeButton.IsRoot = isRoot;
            entityTypeButton.Text = usePluralName? entityType.PluralName : entityType.Name;
            item.Add(entityTypeButton);
            entityTypeButton.TextAlignment = TextButton.TextAlign.Left;
            entityTypeButton.Width = 172; // quantityX - captionX;
            entityTypeButton.X = captionX;            
        }

    
        void tbItems_Click(UIComponent sender, EventArgs e)
        {
            // open the items panel

            EntityGroup owner; // = TileSelectionContextMenu.GetMapAreaOwner();
            IKnownEntityData structureData;
            MapArea mapArea;
            Stockpile stockpile;
          
            if (!GetData(out owner, out structureData, out mapArea, out stockpile))
            {
                // the entity/zone/area tiles got deleted in the meantime...
                Hide();
                return;
            }

            The.InGameUI.EntityListWindow.PopulateAndShowOnPlayfield(sender, ((ItemTypeButtonEventArgs)e).Item, owner);

        }


        void bt_RemoveClick(UIComponent sender, EventArgs e)
        {
            if (The.InGameUI.SelectedZone != null)
            {
                The.InGameUI.SelectedZone.Stockpile = null;
                Hide();
            }
        }

        void btTopAll_Click(UIComponent sender, EventArgs e)
        {
            //if (btAll.IsChecked)
            //{
            //    btNone.IsChecked = false;
            //}

            /*Owner owner = TileSelectionContextMenu.GetMapAreaOwner();

            TileSelectionContextMenu.CreateAndSelectZone(owner);

            if (The.InGameUI.SelectedZone.Stockpile == null)
            {
                The.InGameUI.SelectedZone.Stockpile = new Stockpile();
            }

            The.InGameUI.SelectedZone.Stockpile.Setting = Stockpile.StockpileSetting.All;
            */
         //   Hide();
        }

        private bool GetData(out EntityGroup owner, out IKnownEntityData structureData, out MapArea mapArea, out Stockpile stockpile)
        {
            stockpile = null;

            if (structure == null) //mapArea != null)
            {
                mapArea = TileSelectionContextMenu.GetMapArea();
                structureData = null;
                owner = mapArea.GetOwner(); // TileSelectionContextMenu.GetMapAreaOwner(mapArea);

                if (mapArea.Zone != null)
                {
                    stockpile = mapArea.Zone.Stockpile;
                }

                return true;
            }
            else
            {
                mapArea = null;

                if (GoalEvaluator.EntityDataResultCausesSkip(The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(structure.Value, out structureData)))
                {
                    owner = null;
                    return false;
                }


                IOwner iowner;
                LookUpOwners.ResolveEntityOwner(structureData, out iowner);
                if (iowner == null)
                {
                    // owner is needed.
                    owner = null;
                    return false;
                }

                owner = iowner.OwnedEntities;

                if (typeOfStockpile == Stockpile.TypesOfStockpiles.Normal)
                {
                    owner.StructureStockpiles.TryGetValue(structure.Value, out stockpile);
                }
                else
                {
                    owner.TerminalTradeOffers.TryGetValue(structure.Value, out stockpile);            
                }

                return true;
            }
        }

        void btCancel_Click(UIComponent sender, EventArgs e)
        {
            Hide();
        }

        void btOk_Click(UIComponent sender, EventArgs e)
        {
            // always creates a new stockpile...

            EntityGroup owner;
            IKnownEntityData structureData;
            MapArea mapArea;
            Stockpile stockpile;

            if (!GetData(out owner, out structureData, out mapArea, out stockpile))
            {
                // the entity/zone/area tiles got deleted in the meantime...
                Hide();
                return;
            }


            //clear and extract new user settings - we will need to merge with defaults later to keep 'future categories/items':           
           // stockpile.Clear();            
                        
            // examine custom settings:
            SerializableDictionary<string, bool> mayStockpileCategory;
            //SerializableDictionary<string, bool> mayStockpileItem;
            SerializableDictionary<string, int> mayStockpileItem;
            GetUserSettings(out mayStockpileCategory, out mayStockpileItem);


            DefaultStorageSettings defaultSettings = null;

            // perhaps later, ther will be default settings for zones also, like garbage dumps...
            if (structureData != null)
            {
                defaultSettings = structureData.EntityType.ContainerType.GetDefaultStorageSettings();
            }


            UWGame.SimSide.Commands.CreateStockpile command;
            if (structureData != null)
            {
                command = new SimSide.Commands.CreateStockpile(mayStockpileCategory, mayStockpileItem, owner.ID, defaultSettings, structure.Value, typeOfStockpile, true);
            }
            else if (The.InGameUI.SelectedZone != null)
            {
                command = new SimSide.Commands.CreateStockpile(mayStockpileCategory, mayStockpileItem, owner.ID, defaultSettings, The.InGameUI.SelectedZone.ID, true);
            }
            else
            {
                command = new SimSide.Commands.CreateStockpile(mayStockpileCategory, mayStockpileItem, owner.ID, defaultSettings, The.InGameUI.SelectedTiles, true);      
            }
            
            
            The.Client.Controller.StoreAndExecuteCommand(command);
            

            Hide();
        }

       // private void GetUserSettings(out SerializableDictionary<string, bool> mayStockpileCategory, out SerializableDictionary<string, bool> mayStockpileItem)
         private void GetUserSettings(out SerializableDictionary<string, bool> mayStockpileCategory, out SerializableDictionary<string, int> mayStockpileItem)
        {
            EntityCategory category;
            EntityType entityType;
            CollapsablePanel categoryPanel;
            Grid categoryGrid;
          
            TextButton tbAllowItem, tbProhibitItem;


            mayStockpileCategory = new SerializableDictionary<string, bool>(); 
            mayStockpileItem = new SerializableDictionary<string, int>(); 

            foreach (var item in outerGrid.EntriesByKey)
            {
                category = item.Key as EntityCategory;

                categoryPanel = item.Value as CollapsablePanel;

                ImageButton cbAllowCategory = (ImageButton)categoryPanel.FindChildById(UIComponent.DataControlID.CurrentCategoryOrders);

                if (cbAllowCategory.IsChecked)
                {                    
                    mayStockpileCategory.Add(category.KeyName, true);
                }
                else 
                {                    
                    mayStockpileCategory.Add(category.KeyName, false);
                }

              /*  TextButton tbAllowCategory = (TextButton)categoryPanel.FindChildById(UIComponent.DataControlID.UpgradeAllow);
                TextButton tbProhibitCategory = (TextButton)categoryPanel.FindChildById(UIComponent.DataControlID.UpgradeProhibit);
                
                if (tbAllowCategory.IsChecked)
                {
                    //stockpile.AddCategory(category, true);

                    mayStockpileCategory.Add(category.KeyName, true);

                }
                else if (tbProhibitCategory.IsChecked)
                {
                    //stockpile.AddCategory(category, false);

                    mayStockpileCategory.Add(category.KeyName, false);
                }*/


                // item settings will override the category settings 
                // examine entity types:
                categoryGrid = ((Grid)categoryPanel.ExpandedPanel.Controls[0]);

                foreach (var entityTypeRow in categoryGrid.EntriesByKey)
                {
                    entityType = (EntityType)entityTypeRow.Key;

                   // tbAllowItem = (TextButton)entityTypeRow.Value.FindChildById(UIComponent.DataControlID.UpgradeAllowItem);
                    ImageButton cbEntity = (ImageButton)entityTypeRow.Value.FindChildById(UIComponent.DataControlID.CurrentOrders);

                    if (cbEntity.IsChecked)
                    {
                        // mayStockpileItem.Add(entityType.KeyName, cbEntity.IsChecked);

                        FillableBar slider;
                        entityTypeRow.Value.FindChildById(UIComponent.DataControlID.MaxOrders, out slider);

                        int value;
                        if (slider.Value == GameData.Instance.GUIConstants.UnlimitedStockpileValue)
                        {
                            value = Sim.HasNoLimitValue;
                        }
                        else
                        {
                            value = slider.Value;
                        }

                        mayStockpileItem.Add(entityType.KeyName, value);
                    }
                    else
                    {
                        mayStockpileItem.Add(entityType.KeyName, 0);
                    }                  
                }
            }
        }                          
  

        void btTopNone_Click(UIComponent sender, EventArgs e)
        {
            //if (btAll.IsChecked)
            //{
            //    btNone.IsChecked = false;
            //}

          /*  Owner expeditionOwner = The.Sim.Site.GetMainExpedition().ExpeditionOwner;

            CreateZoneWithStockpile(expeditionOwner);

            The.InGameUI.SelectedZone.Stockpile.Setting = Stockpile.StockpileSetting.None;
            */

        }
     
        public void Hide()
        {          
            DisplayWindow.Hide();          
        }


        Dictionary<EntityType, int> allItems = new Dictionary<EntityType, int>();

        /// <summary>
        /// temporary list.
        /// while Filling, we save all the category panels that we touch in this collection
        /// </summary>
        List<Tuple<EntityCategory, CollapsablePanel>> categoryPanels = new List<Tuple<EntityCategory, CollapsablePanel>>();

        /// <summary>
        /// temporary list.
        /// while Filling, we save all the grids that had entries added to them here:
        /// </summary>
        List<Grid> categoryGridsThatWereAddedTo = new List<Grid>();

        public override void ShowOnPlayfield(int screenPosX, int screenPosY, bool avoidRightInterfaceArea = true)
        {
            base.ShowOnPlayfield(screenPosX, screenPosY, avoidRightInterfaceArea);


          //  tbRemove.X = lblHeader.Right + doubleSpacing;
            tbRemove.X = lblHeader.Right + 60;

          //  Fill(true);


        }

        /// <summary>
        /// the area will be the currently selected zone or tiles
        /// </summary>
        public void FillFromArea()
        {
            // only zone stockpiles can be removed.
            tbRemove.Visible = true;

            this.structure = null;
            typeOfStockpile = Stockpile.TypesOfStockpiles.Normal;

            Fill(true);

        }


        /// <summary>
        /// can be filled from either a map area or an owned structure with item storage
        /// </summary>
        /// <param name="structure"></param>
        public void FillFromStructure(EntityID? structure, Stockpile.TypesOfStockpiles typeOfStockpile)
        {
           // this.mapArea = mapArea;

            // only zone stockpiles can be removed...
            tbRemove.Visible = false;

            this.structure = structure;
            this.typeOfStockpile = typeOfStockpile;

            Fill(true);

        }

        public static bool ItemIsOwnedByAllegiance(IKnownEntityData e)
        {
            if (e.EntityType.ItemType != null
                        && e.OwnedBy != null)
            {
                return The.InGameUI.UIAllegiance.IsOwnedByAllegiance(e);
            }

            return false;
        }

        /// <summary>
        /// update the grid to show new items or categories, and the current no of items.       
        /// 
        /// NOTE: there is no cleanup of categories/items in this list. Once the rows have been added, they are never removed...
        /// </summary>
        private void Fill(bool fillUserControls = false)
        {
            MapArea mapArea = null;
            EntityGroup owner;
            IKnownEntityData structureData;
            Stockpile stockpile;
            if (!GetData(out owner, out structureData, out mapArea, out stockpile))
            {
                Hide();
                return;
            }

            if (typeOfStockpile == Stockpile.TypesOfStockpiles.Normal)
            {
                lblHeader.Text = "STOCKPILE";
            }
            else
            {
                lblHeader.Text = "TRADE";
            }
            
            string displayName = "";
            if (mapArea != null && mapArea.Zone != null)
            {
                displayName = mapArea.Zone.GetDisplayName();
            }
            else if (structureData != null)
            {
                displayName = structureData.EntityType.Name.ToUpper(Config.Culture);
            }

            // unnecessary to set this on refresh...
            SetDisplayName(displayName, lblName, lblHeader, headerIcon);


            Populate(fillUserControls, mapArea, owner, structureData, stockpile);

        }

        private void Populate(bool fillUserControls, MapArea mapArea, EntityGroup owner, IKnownEntityData structureData, Stockpile stockpile)
        {
            allItems.Clear();

            // get items owned by us
            CountItemsThatAreAlreadyHere(mapArea, structureData);

            CollapsablePanel cpCategory;
            Grid grdCategory = null;
            UIComponent categoryRow;
            UIComponent itemRow;

            allAvailableItems.Clear();

            categoryGridsThatWereAddedTo.Clear();
            categoryPanels.Clear();

            // Nested grids

            // outer level is an item "category"
            // add nested grids for each, containing item types...

            EntityType entityType;
            EntityCategory entityCategory;

            int noOfStockpiledItems;

            /*
            if (fillUserControls && stockpile != null)
            {
                FillToplevelControls(stockpile);
            }*/

            outerGrid.BeginAddingEntries();

            bool categoryIsJustAdded = false;

            foreach (var kvp in owner.Items) // only items for now...
            {
                entityType = kvp.Key;
                entityCategory = entityType.Category;
                // noOfOwnedItems = kvp.Value.Count;

                if (GameData.Instance.GUIConstants.StockpileExcludesCategoriesFinal.Contains(entityCategory))
                {
                    continue;
                }

                if (!allItems.TryGetValue(entityType, out noOfStockpiledItems))
                {
                    noOfStockpiledItems = 0;
                }


                categoryIsJustAdded = false;
                cpCategory = null;
                grdCategory = null;

                // get the category panel and grid, or add them if needed:
                if (outerGrid.TryGetEntry(entityCategory, out categoryRow))
                {

                    cpCategory = categoryRow as CollapsablePanel;
                    grdCategory = (Grid)cpCategory.ExpandedPanel.Controls[0];

                    if (!categoryPanels.Exists(t => t.Item2 == cpCategory))
                    {
                        categoryPanels.Add(new Tuple<EntityCategory, CollapsablePanel>(entityCategory, cpCategory)); // save it for later..

                        // we only have to update the category once:
                        UpdateCategoryRow(cpCategory, grdCategory, entityCategory, stockpile, fillUserControls);
                    }

                }
                else
                {
                    // don't add the category until we have (had) access to this item:
                    if (!InventoryPanel.OwnsProductOrHasProcessInputsAndTools(entityType, owner, allAvailableItems))
                    {
                        continue;
                    }

                    // first time we encounter this category. Fill the buttons with defaults.
                    AddCategoryRow(ref cpCategory, ref grdCategory, entityCategory, stockpile);

                    UpdateCategoryRow(cpCategory, grdCategory, entityCategory, stockpile, fillUserControls);

                    categoryPanels.Add(new Tuple<EntityCategory, CollapsablePanel>(entityCategory, cpCategory)); // save it for later..

                    categoryIsJustAdded = true;


                    grdCategory.BeginAddingEntries();
                    categoryGridsThatWereAddedTo.Add(grdCategory); // save the grid for resize at the end of update
                }


                // update/add item
                if (categoryIsJustAdded == false // no need to look up
                    && grdCategory.TryGetEntry(entityType, out itemRow))
                {
                    UpdateItemRow(fillUserControls, itemRow, entityType, noOfStockpiledItems, stockpile);
                }
                else
                {
                    // don't add the grid item row until we have (had) access to this item (ok to update it, it will get set to zero then):
                    if (!InventoryPanel.OwnsProductOrHasProcessInputsAndTools(entityType, owner, allAvailableItems))
                    {
                        continue;
                    }


                    grdCategory.BeginAddingEntries(); // it does not hurt to call this more than once.


                    itemRow = AddItemRow(grdCategory, entityType, owner);

                    UpdateItemRow(fillUserControls, itemRow, entityType, noOfStockpiledItems, stockpile);

                    if (!categoryGridsThatWereAddedTo.Contains(grdCategory))
                    {
                        categoryGridsThatWereAddedTo.Add(grdCategory); // save the grid for resize at the end of update
                    }
                }
            }


            foreach (var item in categoryPanels)
            {
                // update stock total for each category that was touched:
                item.Item2.Summary = allItems.Sum(e => e.Key.Category == item.Item1 ? e.Value : 0).ToString();

                // TODO: checkmark should be an 'has overriding item settings' icon
                // show/hide the checkmark icon - it will be shown if at least one item is checked:
                if (fillUserControls)
                {
                    // Image checkIcon = (Image)item.Item2.FindChildById(UIComponent.DataControlID.HasOverridingItemIcon);
                    //   checkIcon.Visible = OneOrMoreItemsInCategoryAreDifferent(item.Item2, item.Item1, stockpile);

                    UpdateOverridingSettingIcon(item.Item2, item.Item1, stockpile);
                }

            }

            foreach (var item in categoryGridsThatWereAddedTo)
            {
                // refit rows
                item.EndAddingEntries();
            }

            outerGrid.EndAddingEntries();
        }

        private void CountItemsThatAreAlreadyHere(MapArea mapArea, IKnownEntityData structureData)
        {
            if (mapArea != null)
            {
                mapArea.GetNoOfEntitiesInArea(allItems,
                    e => ItemIsOwnedByAllegiance(e),
                    The.InGameUI.UIAllegiance);
            }
            else
            {
                // get contained items owned by us
                var offeredItems = structureData.OfferedEntitiesByType;

                if (typeOfStockpile == Stockpile.TypesOfStockpiles.OfferedForTrade)
                {
                    Entity terminalEntity = structureData as Entity;
                    if (terminalEntity != null) // never in FOW
                    {
                                              
                        foreach (var list in offeredItems)
                        {
                            IKnownEntityData itemData;
                            foreach (var item in list.Value)
                            {
                                if (!GoalEvaluator.EntityDataResultCausesSkip(The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(item, out itemData)))
                                {
                                    MapArea.CountEntity(allItems, itemData, ItemIsOwnedByAllegiance);
                                }
                            }
                        }
                    }

                }
                else 
                {
                    if (structureData.ContainedEntities != null)
                    {
                        IKnownEntityData itemData;
                        foreach (var item in structureData.ContainedEntities)
                        {
                            if (!GoalEvaluator.EntityDataResultCausesSkip(The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(item, out itemData)))
                            {
                                List<EntityID> offeredItemsOfType;
                                // exclude traded items when counting:
                                if (offeredItems == null || !offeredItems.TryGetValue(itemData.EntityType, out offeredItemsOfType) || !offeredItemsOfType.Contains(itemData.EntityID))
                                {
                                    MapArea.CountEntity(allItems, itemData, ItemIsOwnedByAllegiance);
                                }
                            }
                        }

                    }
                }
            }
        }
       

        private void FillToplevelControls(Stockpile stockpile)
        {
            //if (stockpile.Setting == Stockpile.StockpileSetting.All)
            //{
            //    btAll.IsChecked = true;
            //    btNone.IsChecked = false;
            //}
            //else if (stockpile.Setting == Stockpile.StockpileSetting.None)
            //{
            //    btAll.IsChecked = false;
            //    btNone.IsChecked = true;
            //}
            //else
            //{
            //    btAll.IsChecked = false;
            //    btNone.IsChecked = false;
            //}
        }

        /// <summary>
        /// the check mark in the category row will be shown if an item in the list is checked 
        /// OR if the stockpile has an item type which is not in the list (because the type is not in the game (yet)) set to Allow.
        /// 
        /// </summary>
        /// <param name="cp"></param>
        /// <returns></returns>
     /*   private bool OneOrMoreItemsInCategoryAreAllowed(CollapsablePanel cp, EntityCategory category, Stockpile stockpile)
        {
            Grid grid = (Grid)cp.ExpandedPanel.Controls[0];
            if (grid.Entries.Exists(c => ((ImageButton)c.FindChildById(UIComponent.DataControlID.CurrentOrders)).IsChecked)) // check GUI
            {
                return true;
            }
            else
            {
                // check stockpile settings:
                if (stockpile.CategoryHasItemSetting(category))
                {
                    return true;
                }
            }

            return false;
        }*/

        /// <summary>
        /// check if there are visible or hidden item types with a different setting than the category setting
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="category"></param>
        /// <param name="stockpile"></param>
        /// <returns></returns>
        private bool OneOrMoreItemsInCategoryAreDifferent(CollapsablePanel cp, EntityCategory category, Stockpile stockpile, out bool hiddenItemsAreDifferent)
        {
            hiddenItemsAreDifferent = false;
           // TextButton tbAll = (TextButton)cp.FindChildById(UIComponent.DataControlID.CurrentCategoryOrders);
            ImageButton cbCategory;
            cp.FindChildById(UIComponent.DataControlID.CurrentCategoryOrders, out cbCategory);

            // first check GUI items:
            Grid grid = (Grid)cp.ExpandedPanel.Controls[0];

            if (grid.Entries.Exists(c => ((ICanBeChecked)c.FindChildById(UIComponent.DataControlID.CurrentOrders)).IsChecked != cbCategory.IsChecked)) // check GUI
            {
                return true;
            }
            else
            {
                // then check default stockpile settings ("future items") - perhaps explain this case in a tooltip?:
                // should only test items not in the grid!
                if (stockpile != null)
                {
                    if (stockpile.CategoryHasDifferentItemSetting(cbCategory.IsChecked, category, 
                        grid.EntriesByKey.Keys.Select(o => (EntityType)o).ToList()))
                    {
                        hiddenItemsAreDifferent = true;
                        return true;
                    }
                }
            }

            return false;
        }


        private void UpdateItemRow(bool fillUserControls, UIComponent itemRow, EntityType entityType, int noOfStockpiledItems, Stockpile stockpile)
        {
           
            // update existing row:
            UIComponent itemComponent = itemRow.FindChildById(UIComponent.DataControlID.Stock);
            if (itemComponent != null)
            {              
                Label lblValue = (Label)itemComponent;
                lblValue.Text = noOfStockpiledItems.ToString();
                lblValue.FitToText();

                lblValue.AlignRight(quantityX);
            }

            if (fillUserControls)
            {
                // update the checkbox:
                ImageButton checkbox = (ImageButton)itemRow.FindChildById(UIComponent.DataControlID.CurrentOrders);
               /* TextButton tbAllow = (TextButton)itemRow.FindChildById(UIComponent.DataControlID.StockpileAllowItem);
                TextButton tbProhibit = (TextButton)itemRow.FindChildById(UIComponent.DataControlID.StockpileProhibitIem);
                */

                bool isChecked = false;
                int maxOrders;
                GetItemSetting(stockpile, entityType, out isChecked, out maxOrders);

                checkbox.IsChecked = isChecked;
                /*
                tbAllow.IsChecked = isChecked;
                tbProhibit.IsChecked = !isChecked; // needed when radio..?
                 * */

                FillableBar slider = (FillableBar)itemRow.FindChildById(UIComponent.DataControlID.MaxOrders);

                int value;
                if (maxOrders == Sim.HasNoLimitValue)
                {
                    value = GameData.Instance.GUIConstants.UnlimitedStockpileValue;
                }
                else
                {
                    value = maxOrders;
                }

                slider.Value = value;
                slider.UpdateSliderPosition();

                if (isChecked)
                {
                    slider.Visible = true;
                }
                else
                {
                    slider.Visible = false;
                }
            }
        }

       /* private static void FillCategoryRow(CollapsablePanel cpCategory, EntityCategory entityCategory, Stockpile stockpile)
        {
            // fill buttons on the category level:
            TextButton tbAll = (TextButton)cpCategory.FindChildById(UIComponent.DataControlID.StockpileAll);
            TextButton tbNone = (TextButton)cpCategory.FindChildById(UIComponent.DataControlID.StockpileNone);

            if (stockpile != null) // fill in from stockpile settings
            {
                bool categorySetting;
                if (stockpile.mayStockpileCategory.TryGetValue(entityCategory, out categorySetting))
                {
                    if (categorySetting == true)
                    {
                        tbAll.IsChecked = true;
                        tbNone.IsChecked = false;
                    }
                    else
                    {
                        tbAll.IsChecked = false;
                        tbNone.IsChecked = true;
                    }
                }
                else
                {
                    tbAll.IsChecked = false;
                    tbNone.IsChecked = false;
                }
            }
            else
            {
                // default settings:
                tbAll.IsChecked = false;
                tbNone.IsChecked = false;
            }
        }*/

        private void GetCategorySetting(EntityCategory entityCategory, Stockpile stockpile, out bool allowSetting)
        {
            // fill buttons on the category level:

            if (stockpile != null) // fill in from stockpile settings
            {
                bool categorySetting;
                if (stockpile.TryGetCategory(entityCategory, out categorySetting))
                {
                    if (categorySetting == true)
                    {
                        allowSetting = true;                       
                    }
                    else
                    {
                        allowSetting = false;                     
                    }
                }
                else
                {
                    int limit;
                    allowSetting = stockpile.GetAllowBaseSetting(out limit);             
                }
            }
            else // default settings!
            {
                int limit;
                allowSetting = Stockpile.GetAllowBaseSetting(this.typeOfStockpile, out limit);              
            }
        }

        /*
        /// <summary>
        /// if the category setting has been filled, but one or more of the item types have an overriding setting that is different, 
        /// then neither Allow or Prohibt on the category will be checked.
        /// </summary>
        /// <param name="entityCategory"></param>
        /// <param name="stockpile"></param>
        /// <param name="allowSetting"></param>
        /// <param name="prohibitSetting"></param>
        private void GetCategorySetting(EntityCategory entityCategory, Stockpile stockpile, out bool allowSetting, out bool prohibitSetting)
        {
            // fill buttons on the category level:
          
            if (stockpile != null) // fill in from stockpile settings
            {
                bool categorySetting;
                if (stockpile.TryGetCategory(entityCategory, out categorySetting))
                {
                    if (categorySetting == true)
                    {
                        allowSetting = true;
                        prohibitSetting = false;
                    }
                    else
                    {
                        allowSetting = false;
                        prohibitSetting = true;
                    }
                }
                else
                {
                    allowSetting = stockpile.GetAllowBaseSetting();
                    prohibitSetting = !allowSetting;
                    
                }
            }
            else // default settings!
            {
                allowSetting = Stockpile.GetAllowBaseSetting(this.typeOfStockpile);
                prohibitSetting = !allowSetting;
            }
        }*/


        /// <summary>
        /// returns -1 as no limit
        /// </summary>
        /// <param name="stockpile"></param>
        /// <param name="entityType"></param>
        /// <param name="isChecked"></param>
        /// <param name="maxOrders"></param>
        private void GetItemSetting(Stockpile stockpile, EntityType entityType, out bool isChecked, out int maxOrders)
        {           
            if (stockpile != null)
            {               
                // item setting can override category setting, both prohibit and allow!
                isChecked = stockpile.MayStockpile(entityType, out maxOrders);
                
            }
            else
            {
                // default:              
                isChecked = Stockpile.GetAllowBaseSetting(this.typeOfStockpile, out maxOrders);              
            }       

        }

        public static bool OverlapsWithOtherStockpiles(MapArea mapArea)
        {
            bool otherStockpilesOnTile = false;
            mapArea.IterateAreaBreakOnTrue(tile =>
            {
                List<Zone> zonesOnTile = tile.GetListOfZones(The.InGameUI.UIAllegiance);
                if (zonesOnTile != null && zonesOnTile.Count > 0)
                {
                    if (zonesOnTile.Exists(z => z != The.InGameUI.SelectedZone && z.Stockpile != null))
                    {
                        otherStockpilesOnTile = true;
                        return true;
                    }
                }

                return false;
            });
            return otherStockpilesOnTile;
        }

        /*
        private static void PopulateGatherResourcesGrid(GUIManager gui, Grid outerGrid,            
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
                    categoryGrid.ResizeToFit = true;
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

                       

            FullLCDPanel.Cleanup<ResourceType, ResourcesAndJobs, ResourceCategory>(outerGrid, dictionary, categoryGrids);

            outerGrid.EndAddingEntries();
        }*/

        /*
        private class HarvestJobsButtonEventArgs : EventArgs
        {
            public HarvestJobsButtonEventArgs( object item, ResourcesAndJobs resourcesAndJobs)
            {
                this.Item = item;
                this.ResourcesAndJobs = resourcesAndJobs;
            }


            // public IGameData Item;
            public object Item;

            public ResourcesAndJobs ResourcesAndJobs;


        }
*/


        public void ResourceClicked(UIComponent sender, EventArgs eventArgs)
        {
            /*HarvestJobsButtonEventArgs iEventArgs = (HarvestJobsButtonEventArgs)eventArgs;

            string resourceKey = ((IGameData)iEventArgs.Item).KeyName;
            */

       /*     popup.DisplayWindow.Y = sender.AbsolutePosition.Y;
            popup.DisplayWindow.X = sender.AbsolutePosition.X + 16;

           //   Resource resourceData = null;

          //  GetDesignerResourceDataInArea(The.InGameUI.SelectedTiles, resourceKey, out resourceData);

            popup.Fill(GameData.Instance.AllResourceTypes[resourceKey], iEventArgs.ResourcesAndJobs.NumberOfJobsInZone,
                iEventArgs.ResourcesAndJobs.NumberOfResources - iEventArgs.ResourcesAndJobs.NumberOfJobsInOtherZones);

            popup.DisplayWindow.Show();
            */

        }

        public void SaveJobChanges(ResourceType resourceType, int noOfJobs)
        {
          /*  ResourcesAndJobs dataItem = userSelection[resourceType];
            dataItem.NumberOfJobsInZone = noOfJobs;
            dataItem.UserChangedData = true;

            userSelection[resourceType] = dataItem;
            */

            // show the changes:
           /* PopulateGatherResourcesGrid(gui, outerGrid,
                SetSummaryDelegate, itemClickHandler,
                userSelection);*/
        }

           

      
    }
}
