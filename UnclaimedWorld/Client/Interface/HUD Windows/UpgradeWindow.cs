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
using UWGame.Control.Commands;

namespace UWGame.ClientSide.Interface.HUD_Windows
{
    public class UpgradeWindow: HUDWindow
    {
               
        EntityID entityID;

        Image headerIcon;

        protected int itemHeight = 18;

        const int produceColumnX = 226;
       
        const int maxOrdersX = 238;
        const int itemTypeIconColumnX = 18; // sideMargin;
        const int captionX = 38;
        const int categoryCheckedX = 330; // 252;
        const int stageIconX = 41;
        const int allowX = 267;
        const int blockX = 337;
               
        Grid outerGrid;


        /// <summary>
        /// cache for each update
        /// </summary>
        private Dictionary<EntityType, InventoryPanel.Availability> allAvailableItems = new Dictionary<EntityType, InventoryPanel.Availability>();

        UIComponent listSurface;

        Label lblName, lblHeader;
        TextButton btCancel;

        const int rowHeight = 36;

        public UpgradeWindow() 
            :base(448, 500, true, level: Level.Bottom, isMovable: true)
        {
            DisplayWindow.SetResizableArea(ResizeAreas.Top, true);
            DisplayWindow.SetResizableArea(ResizeAreas.Bottom, true);

            DisplayWindow.MinHeight = 100;
            DisplayWindow.ResizableBorderSize = 6;
            DisplayWindow.Resize += DisplayWindow_Resize;


            AddZoneNameAndHeader("", "UPGRADE", "HUD_icon_uparrow", tripleSpacing, out lblName, out lblHeader, out headerIcon);


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
            btCancel.Text = "CLOSE";
            btCancel.Init(TextButton.TextButtonType.HUD);
            btCancel.Click += new ClickHandler(btCancel_Click);
            btCancel.Width = 72;
         //   btCancel.Height = buttonHeight;
            btCancel.Y = DisplayWindow.Height - btCancel.Height - tripleSpacing;
            btCancel.X = DisplayWindow.Width - tripleSpacing - btCancel.Width;


            SetVerticalPositions();
        }

        private void SetVerticalPositions()
        {
            btCancel.Y = DisplayWindow.Height - btCancel.Height - tripleSpacing;

            listSurface.Height = btCancel.Y - 13 - listSurface.Y;
            outerGrid.Height = listSurface.Height;

            // grid.Height = btOK.Y - 13 - grid.Y;

        }

        void DisplayWindow_Resize(UIComponent sender)
        {
            SetVerticalPositions();
        }

        public override void Refresh()
        {
            Populate(false);           
        }


        private void UpdateCategoryRow(EntityGroup owner, CollapsablePanel cpCategory, Grid categoryGrid, UpgradeCategory upgradeCategory)
        {
            Icon icHasSelection = cpCategory.FindChildById(UIComponent.DataControlID.HasSelection, true) as Icon;

            EntityType entityType = owner.GetOrderedUpgrade(entityID, upgradeCategory);

            if (entityType != null)
            {
                icHasSelection.Visible = true;
            }
            else
            {
                icHasSelection.Visible = false;
            }
            


          /*  if (fillUserControls)
            {
                bool allSetting;
                bool noneSetting;
                
                TextButton tbAll = (TextButton)cpCategory.FindChildById(UIComponent.DataControlID.StockpileAllow);
                TextButton tbNone = (TextButton)cpCategory.FindChildById(UIComponent.DataControlID.StockpileProhibit);

                tbAll.IsChecked = allSetting;
                tbNone.IsChecked = noneSetting;
            }*/
        }

        const int expandedPanelMargin = 3;

        private void AddCategoryRow(out CollapsablePanel cpCategory, out Grid categoryGrid, UpgradeCategory upgradeCategory) //, Stockpile stockpile)
        {
           
            cpCategory = new CollapsablePanel(gui, CollapsablePanel.PanelType.StockpileHUD);
            cpCategory.CollapsedHeight = outerGrid.ItemHeight;
            outerGrid.AddEntry(upgradeCategory, cpCategory);
            cpCategory.Init(); //CollapsablePanel.PanelType.Node);
            cpCategory.Title = upgradeCategory.Name.ToUpper(Config.Culture);
            cpCategory.Width = outerGrid.Width;
            cpCategory.TitleSummaryRightAlignXPos = maxOrdersX + expandedPanelMargin;  
            cpCategory.TitlePositionX = captionX + expandedPanelMargin;
            cpCategory.TitleTooltip = upgradeCategory.Description;
            cpCategory.OrderByTag1 = upgradeCategory.SortOrder;
            
            Image checkIcon = new Image(gui);
            checkIcon.SetSkinLocation(SkinState.Normal,gui.GUISpriteSheet.GetSourceRectangle("HUD_checkmark"));
            checkIcon.ResizeControlToFitImage();
            checkIcon.ToolTip = "An upgrade is selected";
            cpCategory.Add(checkIcon);
            checkIcon.X = categoryCheckedX;
            cpCategory.CenterOnHeader(checkIcon);           
            checkIcon.ID = UIComponent.DataControlID.HasSelection;
            checkIcon.Visible = false;
            

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

                //UpdateOverridingSettingIcon(cpCategory, category);
                        
            }
        }

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
            foreach (var item in itemGrid.Entries)
            {
                cbItem = (ImageButton)item.FindChildById(UIComponent.DataControlID.CurrentOrders);
                cbItem.IsChecked = true;
            }
        }


        private static void ProhibitItems(CollapsablePanel cpCategory)
        {
            // deselect items:
            Grid itemGrid = ((Grid)cpCategory.ExpandedPanel.Controls[0]);

           // ImageButton cbItem;
            TextButton tbAllow, tbProhibit;
            foreach (var item in itemGrid.Entries)
            {
              /*  cbItem = (ImageButton)item.FindChildById(UIComponent.DataControlID.CurrentOrders);

                cbItem.IsChecked = false;*/

                tbAllow = (TextButton)item.FindChildById(UIComponent.DataControlID.UpgradeAllowItem);
                tbAllow.IsChecked = false;

                tbProhibit = (TextButton)item.FindChildById(UIComponent.DataControlID.UpgradeProhibitIem);
                tbProhibit.IsChecked = true;
            }
        }

        class UpgradeEventArgs : EventArgs
        {
            public EntityType EntityType;
            public UpgradeCategory UpgradeCategory;
        }

        
        private UIComponent AddItemRow(Grid categoryGrid, UpgradeCategory upgradeCategory, EntityType entityType, EntityGroup owner) 
        {
            
            UIComponent item;

            item = new UIComponent(gui);
            categoryGrid.AddEntry(entityType, item);


            //item.OrderByTag1 = entityType.PluralName; 
            item.OrderByTag1 = (entityType.TierOrAreaType != null? entityType.TierOrAreaType.GetTier().Index : 0); // order by tier 1st, then by name
            item.OrderByTag2 = entityType.PluralName; 

          //  item.DebugTag = "";
            DataTypeButton tbCaption;
            CreateItemGridRow(entityType, GoalEvaluator.GetOwnerID(owner), false, item, DataSheet.InfoToShow.Production, true, true, out tbCaption);

            Label label = new Label(gui);
            item.Add(label);
            label.Init(Label.LabelType.HUDWindow);
            label.ID = UIComponent.DataControlID.MaxOrders;          
           // label.X = produceColumnX;
            item.CenterChildVertically(label);
            label.Y += 2;
            label.X = produceColumnX;

            // for showing max producable:
            /*
            label.AlignRight(maxOrdersX);
            label.ToolTip = "The number of upgrades that can be fulfilled currently";
             */

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

            CheckBox checkBox = new CheckBox(Interface.gui);
            checkBox.Init(CheckBoxType.HUDCheckBox); // Radio buttons are meant to be required options... If you want them to be unchecked, use a checkbox //.HUDRadio); // .HUDCheckBox); 
            item.Add(checkBox);
            checkBox.FitToText();
            checkBox.X = produceColumnX + 100;
            checkBox.Click += cbSelect_Click;
            item.CenterChildVertically(checkBox);
            checkBox.Y += 2;
            checkBox.EventArgs = new UpgradeEventArgs() { EntityType = entityType, UpgradeCategory = upgradeCategory };
            checkBox.ID = UIComponent.DataControlID.Selector;
            checkBox.ToolTip = "When selected, the upgrade will be installed as soon as possible. If it breaks, a new upgrade will be installed. If unselected, the upgrade will be removed again.";


            /*
            ImageButton btBuild = new ImageButton(Interface.gui);
            btBuild.Init(ImageButtonType.HUDBuild);
            item.Add(btBuild);
            btBuild.X = produceColumnX;
            item.CenterChildVertically(btBuild);
            btBuild.Click += btBuild_Click;
            btBuild.EventArgs = new UpgradeEventArgs() { EntityType = entityType, UpgradeCategory = upgradeCategory };
            btBuild.Tag1 = entityType;
            btBuild.ID = UIComponent.DataControlID.Build;
            */

            /*
            ImageButton btCancel = new ImageButton(Interface.gui);
            btCancel.Init(ImageButtonType.HUDDelete);
            item.Add(btCancel);
            btCancel.X = btBuild.Right + 6; // produceColumnX;
            item.CenterChildVertically(btCancel);
            btCancel.Click += btCancel_Click;
            btCancel.Tag1 = entityType;
            btCancel.ID = UIComponent.DataControlID.Cancel;
            */

          //  item.OrderByTag2 = entityType.PluralName; 

/* 
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
            */

            categoryGrid.DebugTag = "categoryGrid";

            return item;
        }

        void cbSelect_Click(UIComponent sender, EventArgs e)
        {
            // let job be created by jobManager isntead. when the upgrade rots away, a new job will be created

            IKnownEntityData entityData;
            EntityGroup owner;
            if (!ResolveEntity(out entityData, out owner))
            {
                Hide();
                return;
            }

            //   EntityType upgradeType = (EntityType)sender.Tag1;

            UpgradeEventArgs args = e as UpgradeEventArgs;

            /*if (!SpecialAction.ActionJobExists(entityData, processType, jobs)) // see that this entity was not added already
            {*/

            string upgradeType = null; // is null to clear the order
            bool enable = ((ICanBeChecked)sender).IsChecked;
            if (enable)
            {
                upgradeType = args.EntityType.KeyName;
            }

            SetUpgrade setUpgradeCommand = new SetUpgrade(entityData.EntityID, owner.GetAllegiance().ID, owner.ID, true, args.UpgradeCategory, upgradeType);
           // setUpgradeCommand.Execute(true); // false); 
            The.Client.Controller.StoreAndExecuteCommand(setUpgradeCommand);

            if (enable)
            {
                // apply stockpile settings immediately
                CreateStockpile createStockpileCommand;
                
                // set stockpile settings immediately:
                if (args.EntityType.Upgrader.StorageSettingsFinal != null)
                {
                    createStockpileCommand = 
                        new SimSide.Commands.CreateStockpile(null, null, owner.ID, args.EntityType.Upgrader.StorageSettingsFinal, entityData.EntityID, Stockpile.TypesOfStockpiles.Normal, true);

                    The.Client.Controller.StoreAndExecuteCommand(createStockpileCommand);

                }               
            }

            Populate(true);
           
            List<Job> jobs = GetJobs(owner);

          
        }

        private static List<Job> GetJobs(EntityGroup owner)
        {
            List<Job> jobs = owner.OtherJobs;
            return jobs;
        }

      /*  private static ProcessType GetUpgradeProcess(EntityType upgradeType)
        {
            List<ProcessType> processes = GameData.Instance.ProcessYieldsThisOutput[upgradeType];
            ProcessType processType = processes.FirstOrDefault(p => p.IsUpgrade);
            return processType;
        }
        */
       
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
     /*   private void UpdateOverridingSettingIcon(CollapsablePanel cpCategory, EntityCategory category, Stockpile stockpile = null)
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
        }*/

      
     
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
        
    
       /* void tbItems_Click(UIComponent sender, EventArgs e)
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
        }*/


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

     /*   private bool GetData(out EntityGroup owner, out IKnownEntityData structureData)
        {

            if (GoalEvaluator.EntityDataResultCausesSkip(The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(entityID.Value, out structureData)))
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

            owner.StructureStockpiles.TryGetValue(entityID.Value, out stockpile);


            return true;

        }*/


        void btCancel_Click(UIComponent sender, EventArgs e)
        {
            Hide();
        }

        /*
        void btOk_Click(UIComponent sender, EventArgs e)
        {
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
            SerializableDictionary<string, bool> mayStockpileItem;
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
                command = new SimSide.Commands.CreateStockpile(mayStockpileCategory, mayStockpileItem, owner.ID, defaultSettings, entityID.Value, typeOfStockpile);
            }
            else if (The.InGameUI.SelectedZone != null)
            {
                command = new SimSide.Commands.CreateStockpile(mayStockpileCategory, mayStockpileItem, owner.ID, defaultSettings, The.InGameUI.SelectedZone.ID);
            }
            else
            {
                command = new SimSide.Commands.CreateStockpile(mayStockpileCategory, mayStockpileItem, owner.ID, defaultSettings, The.InGameUI.SelectedTiles);      
            }
            
            
            The.Client.Controller.StoreAndExecuteCommand(command);
            

            Hide();
        }*/

        private void GetUserSettings(out SerializableDictionary<string, bool> mayStockpileCategory, out SerializableDictionary<string, bool> mayStockpileItem)
        {
            EntityCategory category;
            EntityType entityType;
            CollapsablePanel categoryPanel;
            Grid categoryGrid;
          
            TextButton tbAllowItem, tbProhibitItem;


            mayStockpileCategory = new SerializableDictionary<string, bool>(); // = new SerializableDictionary<EntityCategory, bool>
            mayStockpileItem = new SerializableDictionary<string, bool>(); // = new Dictionary<EntityType, bool>();

            foreach (var item in outerGrid.EntriesByKey)
            {
                category = item.Key as EntityCategory;

                categoryPanel = item.Value as CollapsablePanel;

                TextButton tbAllowCategory = (TextButton)categoryPanel.FindChildById(UIComponent.DataControlID.UpgradeAllow);
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
                }


                // item settings will override the category settings 
                // examine entity types:
                categoryGrid = ((Grid)categoryPanel.ExpandedPanel.Controls[0]);

                foreach (var entityTypeRow in categoryGrid.EntriesByKey)
                {
                    entityType = (EntityType)entityTypeRow.Key;

                   tbAllowItem = (TextButton)entityTypeRow.Value.FindChildById(UIComponent.DataControlID.UpgradeAllowItem);
                   // ImageButton cbEntity = (ImageButton)entityTypeRow.Value.FindChildById(UIComponent.DataControlID.CurrentOrders);

                    // only store the setting if it differs from the category: - whoops, hard to merge with default settings then..
                    // not doing this will gradually grow the number of item settings
                    /*if (tbAllowItem.IsChecked != tbAllowCategory.IsChecked)
                    {*/

                  //  mayStockpileItem.Add(entityType.KeyName, cbEntity.IsChecked);


                    //OLD:
                    //stockpile.AddItem(entityType, tbAllowItem.IsChecked);
                    mayStockpileItem.Add(entityType.KeyName, tbAllowItem.IsChecked);


                    /* 
                     stockpile.AddItem(entityType, cbEntity.IsChecked);*/
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
           // tbRemove.X = lblHeader.Right + 60;

           // cleanup for categories that are not in use? should not be done here. At end of Fill is the correct place.
         /*   foreach (var item in categoryPanels)
            {
                Image checkIcon = (Image)item.Item2.FindChildById(UIComponent.DataControlID.ChildRowIsSelected);
                checkIcon.Visible = false;// OneOrMoreItemsInCategoryAreChecked(item.Item2);    
            }*/

        }
              

             
        public static bool ItemIsOwnedByAllegiance(IKnownEntityData e)
        {
            if (e.EntityType.ItemType != null
                        && e.OwnedBy != null)
            {
                IOwner owner;

                if (LookUpOwners.ResolveEntityOwner(e, out owner))
                {
                    if (owner != null && owner.Allegiance == The.InGameUI.UIAllegiance)
                        return true;
                }
            }

            return false;
        }


        private bool ResolveEntity(out IKnownEntityData entityData, out EntityGroup owner)
        {
            owner = null;

            The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(entityID, out entityData);

            if (entityData == null) // !GetData(out owner, out structureData, out mapArea, out stockpile))
            {
                Hide();
                return false;
            }

            IOwner iowner;
            LookUpOwners.ResolveEntityOwner(entityData, out iowner);
            if (iowner == null)
            {
                // owner is needed.               
                return false;
            }

            owner = iowner.OwnedEntities;
            if (owner.GetAllegiance() != The.InGameUI.UIAllegiance)
            {
                Hide();
                return false;
            }

            return true;
        }

        private void Populate(bool fillUserControls)
        {
            IKnownEntityData entityData;
            EntityGroup owner;
            if (!ResolveEntity(out entityData, out owner))
            {
                Hide();
                return;
            }
                     
            allAvailableItems.Clear();

            categoryGridsThatWereAddedTo.Clear();
            categoryPanels.Clear();


            outerGrid.BeginAddingEntries();

            bool categoryIsJustAdded = false;

            HashSet<EntityType> includedEntityTypes = null;

            UIComponent categoryRow, itemRow;
            CollapsablePanel cpCategory;
            Grid grdCategory;
            var upgradeCategories = entityData.EntityType.ContainerType.GetUpgradeOptions();
            foreach (var upgradeCategory in upgradeCategories)
            {
              
                // get the category panel and grid, or add them if needed:
                if (!outerGrid.TryGetEntry(upgradeCategory, out categoryRow))
                {
                    AddCategoryRow(out cpCategory, out grdCategory, upgradeCategory);
                                        
                    if (!categoryPanels.Exists(t => t.Item2 == cpCategory))
                    {
                       // categoryPanels.Add(new Tuple<EntityCategory, CollapsablePanel>(upgradeCategory, cpCategory)); // save it for later..

                        // we only have to update the category once:
                        
                    }
                }
                else
                {
                    cpCategory = categoryRow as CollapsablePanel;
                    grdCategory = (Grid)cpCategory.ExpandedPanel.Controls[0];
                }
               
                UpdateCategoryRow(owner, cpCategory, grdCategory, upgradeCategory);

                List<EntityType> itemsInCategory;
                if (GameData.Instance.UpgraderEntityTypesByUpgradeCategory.TryGetValue(upgradeCategory, out itemsInCategory))
                {
                    foreach (var entityType in itemsInCategory) 
                    {
                        // update/add item
                        if (/*categoryIsJustAdded == false // no need to look up
                        &&*/
                            !grdCategory.TryGetEntry(entityType, out itemRow))
                        {                           
                            grdCategory.BeginAddingEntries(); // it does not hurt to call this more than once.

                            itemRow = AddItemRow(grdCategory, upgradeCategory, entityType, owner);

                            if (!categoryGridsThatWereAddedTo.Contains(grdCategory))
                            {
                                categoryGridsThatWereAddedTo.Add(grdCategory); // save the grid for resize at the end of update
                            }
                        }

                        UpdateItemRow(itemRow, upgradeCategory, entityType, entityData, owner, fillUserControls);

                        Common.AddToList(ref includedEntityTypes, entityType);
                    }
                }
            }


            // remove unused item   
            foreach (CollapsablePanel colPanel in outerGrid.Entries)
            {
                Grid grd = (Grid)colPanel.ExpandedPanel.Controls[0];
                grd.DeleteEntries<EntityType>(e => includedEntityTypes != null && includedEntityTypes.Contains(e)); // currentListData.Contains(j));
            }

            //remove empty categories
            List<object> keysToRemove = null; // = new List<object>();
            foreach (var key in outerGrid.EntriesByKey.Keys)
            {
                Grid grd = (Grid)(outerGrid.EntriesByKey[key] as CollapsablePanel).ExpandedPanel.Controls[0];

                if (grd.Entries.Count == 0
                    || !upgradeCategories.Contains(key))
                {
                    Common.AddToList(ref keysToRemove, key);
                }
            }

            if (keysToRemove != null)
            {
                foreach (var deleteKey in keysToRemove)
                {
                    outerGrid.RemoveEntry(deleteKey);
                }
            }

            /*
            // remove unused item and category rows. 
             * // can't use since we need UpgradeCategory instead of EntityCategory
            FullLCDPanel.Cleanup<EntityType, int, EntityCategory>(outerGrid, null, null, 
                e => includedEntityTypes != null && includedEntityTypes.Contains(e),
                c => upgradeCategories.Contains(c));
            */

            // sort categories if changes were made:
            InventoryPanel.DoCategorySorting(outerGrid, categoryGridsThatWereAddedTo, Grid.Sorting.Ascending);

            
            foreach (var item in categoryGridsThatWereAddedTo)
            {
                // refit rows
                item.EndAddingEntries();
            }

            outerGrid.EndAddingEntries();

        }
      

        public void Fill(EntityID entityID, bool fillUserControls = false)
        {
            this.entityID = entityID;
            
            IKnownEntityData entityData;
            The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(entityID, out entityData);

            if (entityData == null) // !GetData(out owner, out structureData, out mapArea, out stockpile))
            {
                Hide();
                return;
            }
                                   
            string displayName = "";
            if (entityData != null)
            {
                displayName = entityData.GetDisplayName().ToUpper(Config.Culture);
            }

        
            SetDisplayName(displayName, lblName, lblHeader, headerIcon);
                 
            Populate(fillUserControls);
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
     /*   private bool OneOrMoreItemsInCategoryAreDifferent(CollapsablePanel cp, EntityCategory category, Stockpile stockpile, out bool hiddenItemsAreDifferent)
        {
            hiddenItemsAreDifferent = false;
            TextButton tbAll = (TextButton)cp.FindChildById(UIComponent.DataControlID.UpgradeAllow);

            // first check GUI items:
            Grid grid = (Grid)cp.ExpandedPanel.Controls[0];
            if (grid.Entries.Exists(c => ((TextButton)c.FindChildById(UIComponent.DataControlID.UpgradeAllowItem)).IsChecked != tbAll.IsChecked)) // check GUI
            {
                return true;
            }
            else
            {
                // then check default stockpile settings ("future items") - perhaps explain this case in a tooltip?:
                if (stockpile != null 
                    && stockpile.CategoryHasDifferentItemSetting(tbAll.IsChecked, category))
                {
                    hiddenItemsAreDifferent = true;
                    return true;
                }
            }

            return false;
        }*/


        private void UpdateItemRow(UIComponent itemRow, UpgradeCategory category, EntityType entityType, IKnownEntityData entityData, EntityGroup owner, bool fillUserControls) //, int noOfStockpiledItems, Stockpile stockpile)
        {

            bool hasTools, hasInputs, hasSkills, hasResources, hasSpecialSite, hasPolicy;
            EntityType immovableInput;
            int maxAmountThatCanBeProduced;
            int? noOfMissingInputTypes, noOfAvailableInputTypes, outputBatchAmount;
            ProcessType processType;

            bool canProduce = InventoryPanel.GetBestProcessForDisplay(entityType, owner, out hasInputs, out hasTools,
              out maxAmountThatCanBeProduced, out noOfMissingInputTypes, out noOfAvailableInputTypes, out hasSkills, out hasResources, out hasSpecialSite, out hasPolicy, out immovableInput, out outputBatchAmount, out processType, false,
              p => p.IsUpgrade);

            DataTypeButton tbCaption = (DataTypeButton)itemRow.FindChildById(UIComponent.DataControlID.Caption);

           // tbCaption.SetStockStatusColor(noOfAvailableItems > 0);

            HorizontalList hzNotAttainable = itemRow.FindChildById(UIComponent.DataControlID.NotAttainableIcons) as HorizontalList;
            HorizontalList hzAttainable = itemRow.FindChildById(UIComponent.DataControlID.AttainableIcons) as HorizontalList;

            Label lblMaxOrders = itemRow.FindChildById(UIComponent.DataControlID.MaxOrders) as Label;

            CheckBox cbSelect = itemRow.FindChildById(UIComponent.DataControlID.Selector) as CheckBox;

            if (fillUserControls)
            {
                // update the checkbox:
               
                bool isChecked = false;
                GetItemSetting(owner, category, entityType, out isChecked);

                cbSelect.IsChecked = isChecked;               
            }

            bool hasUpgradeInstalled = false;

            EntityID containedUpgrade;
            if (entityData.ContainedUpgrades != null && entityData.ContainedUpgrades.TryGetValue(category, out containedUpgrade)) // .ContainsKey(category))
            {
                IKnownEntityData containedUpgradeData;
                if (!GoalEvaluator.EntityDataResultCausesSkip(The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(containedUpgrade, out containedUpgradeData)))
                {
                    if (containedUpgradeData.EntityType == entityType)
                    {
                        hasUpgradeInstalled = true;
                    }
                }
            }  


            Dictionary<ProcessType, AttainableInfo> attainableInfo = null;
            if (!canProduce && !hasUpgradeInstalled)
            {
                attainableInfo = The.InGameUI.InventorySettings.GetAttainableInfo(entityType);

                ProductionOrderControl.UpdateAttainable(attainableInfo, hzAttainable, hzNotAttainable,
                    hasTools, hasInputs, hasSkills, hasResources, canProduce, processType);

                lblMaxOrders.Visible = false;
            }
            else
            {
                
                lblMaxOrders.Visible = true;
                              

                if (hasUpgradeInstalled)
                {
                    lblMaxOrders.Text = "INSTALLED"; 
                    lblMaxOrders.ToolTip = "The upgrade is installed.";            
                }
                else
                {
                    lblMaxOrders.Text = "AVAILABLE"; // Morten thought the number was too confusing: maxAmountThatCanBeProduced.ToString();
                    lblMaxOrders.ToolTip = "The upgrade can be installed now.";
                }

                lblMaxOrders.FitToText();
               // lblMaxOrders.AlignRight(maxOrdersX);

                hzAttainable.Visible = false;
                hzNotAttainable.Visible = false;
            }

           


          /*  ImageButton btBuild = itemRow.FindChildById(UIComponent.DataControlID.Build) as ImageButton;
            ImageButton btCancel = itemRow.FindChildById(UIComponent.DataControlID.Cancel) as ImageButton;

            btCancel.Visible = false; // TODO
           


           // ProcessType processType = GetUpgradeProcess(entityType);
            List<Job> jobs = GetJobs(owner);
            
            if (canProduce)
            {
                btBuild.Visible = true;
            }
            else
            {
                btBuild.Visible = false;
            }

            if (SpecialAction.ActionJobExists(entityData, processType, jobs)) // see that this job was not added already
            {
                btBuild.ToolTip = "This task is ongoing. Use the task panel to view or cancel it";
                btBuild.Enabled = false;
               // return;
            }
            else
            {
                btBuild.ToolTip = "Click to start this upgrade";
                btBuild.Enabled = true;
            }       

           * 

            if (btBuild.Visible == false)
            {
                Dictionary<ProcessType, AttainableInfo> attainableInfo = null;
                if (!canProduce)
                {
                    attainableInfo = The.InGameUI.InventorySettings.GetAttainableInfo(entityType);
                }

                InventoryPanel.UpdateAttainable(attainableInfo, hzAttainable, hzNotAttainable,
                    hasTools, hasInputs, hasSkills, hasResources, canProduce, processType);
            }
            else
            {
                hzAttainable.Visible = false;
                hzNotAttainable.Visible = false;
            }*/


            // update existing row:
          /*  UIComponent itemComponent = itemRow.FindChildById(UIComponent.DataControlID.Stock);
            if (itemComponent != null)
            {
                // TextButton lblValue = (TextButton)itemComponent;
                Label lblValue = (Label)itemComponent;
                lblValue.Text = noOfStockpiledItems.ToString();

                lblValue.AlignRight(quantityX);
            }*/

            /*
           */
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


     /*
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

                    //allowSetting = true; 
                    
                }
            }
            else // default settings!
            {
                allowSetting = Stockpile.GetAllowBaseSetting(this.typeOfStockpile);
                prohibitSetting = !allowSetting;

            }
        }
      */

        /*
        private void GetCategorySetting(EntityGroup owner, UpgradeCategory category, out bool isChecked)
        {
            isChecked = owner.GetOrderedUpgrade(entityID, category, entityType);

        }*/
        
        private void GetItemSetting(EntityGroup owner, UpgradeCategory category, EntityType entityType, out bool isChecked)
        {
            isChecked = owner.GetIsUpgrade(entityID, category, entityType);

            /*
            if (stockpile != null)
            {               
                // item setting can override category setting, both prohibit and allow!
                isChecked = stockpile.MayStockpile(entityType);
                
            }
            else
            {
                // default:
                isChecked = Stockpile.GetAllowBaseSetting(this.typeOfStockpile);
                //isChecked = true; 
            }  */     

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
