using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework.Input;
using UWGame.SimSide;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Items;
using UWGame.SimSide.Expeditions;
using UWGame.ClientSide.Interface.Controls;
using UWGame.Client.Interface;

using InputEventSystem;
using UWGame.SimSide.AI;
using UWGame.SimSide.Entities.Biological;
using UWGame.ClientSide.Interface.Inventory;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.AI.Needs;
using UWGame.SimSide.Combat;
using UWGame.SimSide.AI.Constants.Rating;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.SimEffects;
using UWGame.SimSide.AllGameData;

namespace UWGame.ClientSide.Interface.HUD_Windows
{
    /// <summary>
    /// this data type tooltip shows info about an entity type.
    /// </summary>
    public class EntityDataSheet : DataSheet
    {
        /// <summary>
        /// placed at the top, next to the header to indicate tracked status/color
        /// 
        /// these can change state by clicking in the inventory panel, the base class will receive an event if that happens.
        /// </summary>
        protected Bar coloredBar;
        protected ImageButton btTrack;

      //  protected ImageButton btOpenManager;

        const string trackingButtonTooltip = "Toggle tracking this item";

       
        #region Data - the UIcomponents are to give padding above and below the labels

        const float edibleIndex = 10;
        Label lblHumanEdible;
        UIComponent humanEdibleHeader;

        const float nutritionIndex = 15;
        Grid grdNutrition;
        Label lblNutrition;
        UIComponent nutritionHeader;

        const float comfortIndex = 20;
        Label lblComfort;
        UIComponent comfortHeader;

        const float replenishIndex = 30;
        UIComponent replenishHeader;
        Grid grdRequiresReplenish;

        const float upgradeForIndex = 40;
        UIComponent upgradeForHeader;
        Grid grdUpgradeFor;

        const float upgradesIndex = 50;
        UIComponent possibleUpgradesHeader;
        Grid grdPossibleUpgrades;

        const float effectsIndex = 60;
        UIComponent effectsHeader;
        Grid grdEffects;
             

        const float storageIndex = 80;
        UIComponent storageHeader;    
        Grid grdStorage;

       // Label lblAmmoHeader;
        const float ammoIndex = 90;
        UIComponent ammoHeader;
        Grid grdAmmo;

        const float weaponIndex = 100;
        Label lblWeapon;
        UIComponent weaponHeader;

        Label lblWeaponDefenseRating;
      //  TextArea taAttacks;

        Label lblWeaponNotWieldable;

        TextArea taHighlyEffective;
       // TextArea taNotEffective;


        const float durabilityIndex = 120;
        UIComponent durabilityHeader;
        Label lblDegradeType;
        Grid grdDurability;

        #endregion


        #region Production

        const float ordersIndex = 0f;

        const float usedInIndex = 80f;

        const float toolUsedForIndex = 100f;
        const float toolUsedForCapMessageIndex = 105f;


        int processIndex = 0;
        int noOfProcesses;
        
        /// <summary>
        /// arrow button
        /// </summary>
        protected ImageButton btSelectProcess;
        protected Label lblProcessIndex;

      //  protected Label lblProcessName;


        UIComponent settingsHeader;
        protected Label lblSettings;

     
        /// <summary>
        /// contains the slider etc.
        /// </summary>
        ProductionOrderControl productionOrderControl;
     


        Grid grdUsedIn;
        UIComponent usedInHeader;
        Label lblUsedIn;


        Grid grdToolUsedFor;
        UIComponent toolUsedForHeader;
        Label lbltoolUsedFor;
        Label lbltoolUsedForCapNotice;
       
        #endregion

        public EntityType EntityType
        {
            get
            {
                return entityType;
            }
        }

        private EntityType entityType;
      /*  public EntityType EntityType
        {
            get { return entityType; }
            set
            {
                if (value != entityType)
                {
                    entityType = value;

                    FillEntityTypeDependentInfo();
                }
            }
        }*/

        /// <summary>
        /// data that depends on the entity (instance), not the EntityType
        /// </summary>
        private EntityTypeTooltipInstanceData instanceData;


        /// <summary>
        /// creates a panel with the relevant fields for this entity type
        /// </summary>
        /// <param name="entityType"></param>
        public EntityDataSheet() : base()
        {
         //   UpdateHeader();
            if (GameData.Instance.GUIConstants.EnableFilters)
            {
                coloredBar = CreateTrackedColorBar(145);
                Add(coloredBar);
                coloredBar.Y = 4;
                // coloredBar.X = lblName.Right + 5;


                btTrack = new ImageButton(gui);
                Add(btTrack);
                btTrack.Init(ImageButtonType.HUDCrosshair);
                btTrack.CheckedMode = CheckedModes.SwitchCheckedStateOnClick;
                // btTrack.X = btClose.X - btTrack.Width - 2;
                // btTrack.Y = 5;// expandedPanelHeadingY;   
                btTrack.X = btClose.X;
                btTrack.Y = btClose.Bottom + 5;
                btTrack.Click += btTrack_Click;
                btTrack.ToolTip = trackingButtonTooltip; 
            }
           // lblName.MaxWidth = btTrack.X - 2 - lblName.X;

           /* btOpenManager = new ImageButton(gui);
            Add(btOpenManager);
            btOpenManager.Init(ImageButtonType.HUDCrosshair);         
            btOpenManager.X = btClose.X;
            btOpenManager.Y = btPin.Bottom + 5;
            btOpenManager.Click += btOpenManager_Click;
            btOpenManager.ToolTip = "Open the production manager and show this item"; 
            */

        /*    policyHeader = AddSubHeader(grdProductionOuter, "POLICY:", "lcd_icon_section", -2, 0, out lbl, out icon, topPadding: 0, itemHeight: 18);
            policyHeader.OrderByTag1 = policyIndex; // for sorting
            lbl.ToolTip = "Requires a policy to be enacted";
            policyIcon = new Icon(gui);
            policyHeader.Add(policyIcon);
           */

            btSelectProcess = new ImageButton(gui);
            Add(btSelectProcess);
            btSelectProcess.Init(ImageButtonType.HUDArrowRight);
            btSelectProcess.CheckedMode = CheckedModes.CannotBeChecked;
            btSelectProcess.X = DisplayWindow.Width - doubleSpacing - btSelectProcess.Width;
            btSelectProcess.Y = expandedPanelHeadingY;
            btSelectProcess.Click += btProcess_Click;
            btSelectProcess.ToolTip = "Click to view the next production process";
            btSelectProcess.DebugTag = "btProcess";
            btSelectProcess.NormalColor = productionColor;

            lblProcessIndex = new Label(gui);
            lblProcessIndex.Init(Label.LabelType.HUDWindow);
            Add(lblProcessIndex);
            lblProcessIndex.Y = expandedPanelHeadingY;
            lblProcessIndex.Text = "1/1";
            lblProcessIndex.FitToText();
            lblProcessIndex.X = btSelectProcess.X - lblProcessIndex.Width - singleSpacing;
            lblProcessIndex.NormalColor = productionColor;

          /*  lblProcessName = new Label(gui);
            lblProcessName.Init(Label.LabelType.HUDWindow);
            Add(lblProcessName);
            lblProcessName.Y = lblProcessIndex.Bottom;
            lblProcessName.Text = "NAME";
            lblProcessName.FitToText();
            lblProcessName.AlignRight(DisplayWindow.Width - doubleSpacing); // .X = SideMarginOutsideGrid();
            lblProcessName.ToolTip = "The name of the selected process";
            lblProcessName.NormalColor = productionColor;
            */
        }

        void btOpenManager_Click(UIComponent sender, EventArgs e)
        {
            The.InGameUI.ShowInventoryPanel();
            
        }


     
        void btProcess_Click(UIComponent sender, EventArgs e)
        {
            int newIndex = processIndex + 1;
            if (newIndex >= noOfProcesses)
            {
                newIndex = 0;
            }

            SelectProcess(newIndex);

            Refresh();
        }

        

        public void Fill(EntityType entityType, EntityID? entityID)
        {
            if (entityID.HasValue)
            {
                SharedKnowledge sharedKnowledge = The.InGameUI.UIAllegiance.SharedKnowledge;
                IKnownEntityData knownEntity;
                sharedKnowledge.GetKnownData(entityID.Value, out knownEntity);
                if (knownEntity != null)
                {
                    instanceData = knownEntity.TooltipEntityData;
                }
                else
                {
                    instanceData = null;
                }
            }
            else
            {
                instanceData = null;
            }

           
            if (this.entityType != entityType)
            {
                this.entityType = entityType;

                List<ProcessType> listOfProcesses;
                if (GameData.Instance.ProcessYieldsThisOutput.TryGetValue(entityType, out listOfProcesses))
                {
                    noOfProcesses = listOfProcesses.Count;
                    //processTypeToShow = listOfProcesses[processIndex];
                }
                else
                {
                    noOfProcesses = 0;
                }

                SelectProcess(0);

                // depends on EntityType:
                if (productionOrderControl != null)
                {
                    grdProductionOuter.RemoveEntry(productionOrderControl);
                    productionOrderControl = null;
                }

                CreateOrders();

                Fill();
            }
        }


      /*  private void CreateOrdersControls()
        {
            ProductionTargetEventArgs eventArgs = new ProductionTargetEventArgs(entityType, null);
            InventoryPanel.AddProductionControls(entityType, item, eventArgs, 0, 83,
                tbItems_Click, fillableBar_SliderMouseDown, InventoryPanelPadlock_Click);

        }*/
      
        private void SelectProcess(int index)
        {
            processIndex = index;

            PopulateProcessSelector();
        }

        private void PopulateProcessSelector()
        {
            if (noOfProcesses > 1)
            {
                lblProcessIndex.Text = string.Format("{0}/{1}", processIndex + 1, noOfProcesses);
                               
                btSelectProcess.Visible = true;
                lblProcessIndex.Visible = true;
               // lblProcessName.Visible = true;
            }
            else
            {
                btSelectProcess.Visible = false;
                lblProcessIndex.Visible = false;
               // lblProcessName.Visible = false;
            }
        }

        protected override void PopulatePolicy()
        {
            base.PopulatePolicy();

            if (entityType.TierOrAreaType != null)
            {
                grdProductionOuter.TryRemoveEntry(policyHeader); // overrides process?
          
                ShowPolicyArea(entityType.TierOrAreaType);
            }
        }
       
        private void SetMarginAndWidth(UIComponent component)
        {
            component.X = SideMarginOutsideGrid();
            component.Width = grdProductionOuter.Width - component.X - rightMargin;  // component.Width = grdProductionOuter.Width - grdNutrition.X - rightMargin; 
        }

        static Color blue = Common.ColorFromHex("0FF8FD");
        static Color brown = Common.ColorFromHex("CEB57D");       
        static Color green = Common.ColorFromHex("02CC6D");
        static Color lightgreen = Common.ColorFromHex("70CC7D");

        static Color foodColorGreen = Common.ColorFromHex("59C24E");
        static Color securityColorBlue = Common.ColorFromHex("69C2E5");
        static Color comfortColorPink = Common.ColorFromHex("CC82B7");

        const int fixedGridItemHeight = 18;

        protected override void CreateGeneralPanelContents()
        {

            humanEdibleHeader = AddSubHeader(grdGeneralOuter, "EDIBLE BY HUMANS:", out lblHumanEdible, labelColor: foodColorGreen);
            humanEdibleHeader.OrderByTag1 = edibleIndex;

            nutritionHeader = AddSubHeader(grdGeneralOuter, "POTENTIAL NUTRITIONAL CONTENT:", out lblNutrition, labelColor: foodColorGreen); //was "NUTRITION:" but now has to be used for indeible items and since content is dispalyed in % of Daily recommended intake for human..
            nutritionHeader.OrderByTag1 = nutritionIndex;
            grdNutrition = CreateSubGrid();
            grdNutrition.OrderByTag1 = nutritionIndex + 1;


            weaponHeader = AddSubHeader(grdGeneralOuter, "WEAPON DATA:", out lblWeapon, labelColor: securityColorBlue);
            weaponHeader.OrderByTag1 = weaponIndex;

            lblWeaponNotWieldable = new Label(gui);
            lblWeaponNotWieldable.Init(Label.LabelType.EntityTypeTooltip);
            lblWeaponNotWieldable.Text = "NOT WIELDABLE"; //;
            lblWeaponNotWieldable.ToolTip = "The weapon is a part of a structure/robot and cannot be used directly by characters.";
            lblWeaponNotWieldable.FitToText();
            lblWeaponNotWieldable.X = SideMarginOutsideGrid();
            lblWeaponNotWieldable.OrderByTag1 = weaponIndex + 1;
            grdGeneralOuter.AddEntry(lblWeaponNotWieldable, lblWeaponNotWieldable); 
         

            lblWeaponDefenseRating = new Label(gui);
            lblWeaponDefenseRating.Init(Label.LabelType.EntityTypeTooltip);
          //  lblWeaponDefenseRating.Text = text; //;
            lblWeaponDefenseRating.FitToText();
            lblWeaponDefenseRating.X = SideMarginOutsideGrid();
            lblWeaponDefenseRating.OrderByTag1 = weaponIndex + 2;
            grdGeneralOuter.AddEntry(lblWeaponDefenseRating, lblWeaponDefenseRating); 
         

            taHighlyEffective = new TextArea(gui, ListBoxType.HUDAndLCD);
            grdGeneralOuter.AddEntry(taHighlyEffective, taHighlyEffective);
            taHighlyEffective.Init(Label.LabelType.HUDWindow);
            taHighlyEffective.CanGrowInHeight = true;
            taHighlyEffective.ScrollBarEnabled = false;
            SetMarginAndWidth(taHighlyEffective);
            taHighlyEffective.OrderByTag1 = weaponIndex + 3;

            Label lbl;
            ammoHeader = AddSubHeader(grdGeneralOuter, "AMMUNITION:", out lbl, labelColor: securityColorBlue);
            ammoHeader.OrderByTag1 = ammoIndex;
            lbl.ToolTip = "The following ammunition type is required. \nNOTE: A ranged weapon will not count in the colony Security rating unless it has ammunition available.";
            grdAmmo = CreateFixedItemHeightGrid(); // CreateSubGrid();
            grdAmmo.OrderByTag1 = ammoIndex + 1;
            //grdGeneralOuter.Add(grdAmmo, grdAmmo);

            storageHeader = AddSubHeader(grdGeneralOuter, "STORAGE:", out lbl, false, labelColor: brown);
            storageHeader.OrderByTag1 = storageIndex;
            lbl.ToolTip = "The storage conditions and capacity offered by this object";
            grdStorage = CreateFixedItemHeightGrid(fixedGridItemHeight);
            grdStorage.OrderByTag1 = storageIndex + 1;

            replenishHeader = AddSubHeader(grdGeneralOuter, "FUEL/ENERGY:", out lbl, labelColor: brown);
            replenishHeader.OrderByTag1 = replenishIndex;
            lbl.ToolTip = "One of the following fuel or energy types is required";
            grdRequiresReplenish = CreateFixedItemHeightGrid();
            grdRequiresReplenish.OrderByTag1 = replenishIndex + 1;

            upgradeForHeader = AddSubHeader(grdGeneralOuter, "UPGRADE FOR:", out lbl, false);
            upgradeForHeader.OrderByTag1 = upgradeForIndex;
            lbl.ToolTip = "The following structures can be upgraded with this item (using the UPGRADE action)";
            grdUpgradeFor = CreateFixedItemHeightGrid();
            grdUpgradeFor.OrderByTag1 = upgradeForIndex + 1;

            possibleUpgradesHeader = AddSubHeader(grdGeneralOuter, "UPGRADE OPTIONS:", out lbl, false, labelColor: lightgreen);
            possibleUpgradesHeader.OrderByTag1 = upgradesIndex;
            lbl.ToolTip = "The structure has these optional upgrades (using the UPGRADE action)";
            grdPossibleUpgrades = CreateFixedItemHeightGrid();
            grdPossibleUpgrades.OrderByTag1 = upgradesIndex + 1;

            durabilityHeader = AddSubHeader(grdGeneralOuter, "DURABILITY:" /*"DURATION:"*/, out lbl, false, labelColor: blue);
            durabilityHeader.OrderByTag1 = durabilityIndex;
            lbl.ToolTip = "Shows how long the object will last under different conditions. \nStructures can have their lifespan extended with regular maintenance. Items CANNOT."; //was: "Shows how long the item will last when stored under different conditions"
           
            lblDegradeType = new Label(gui);
            lblDegradeType.Init(Label.LabelType.EntityTypeTooltip);        
            lblDegradeType.FitToText();
            lblDegradeType.X = SideMarginOutsideGrid();
            lblDegradeType.OrderByTag1 = durabilityIndex + 1;

            grdDurability = CreateFixedItemHeightGrid(fixedGridItemHeight);
            grdDurability.OrderByTag1 = durabilityIndex + 2;

            effectsHeader = AddSubHeader(grdGeneralOuter, "EFFECTS:", out lbl, false, labelColor: green);
            effectsHeader.OrderByTag1 = effectsIndex;
            lbl.ToolTip = "The resulting effects";
            grdEffects = CreateFixedItemHeightGrid(); // won't show entitytype buttons
            grdEffects.ItemHeight = 18;
            grdEffects.OrderByTag1 = effectsIndex + 1;

            comfortHeader = AddSubHeader(grdGeneralOuter, "COMFORT:", out lblComfort, labelColor: comfortColorPink);  // use comfort tint        
            lblComfort.ToolTip = "The base comfort level when in perfect condition, and without upgrades";
            comfortHeader.OrderByTag1 = comfortIndex;
        }

        private Grid CreateSubGrid()
        {
            int gridLineHeight = 18;

            Grid grid = new Grid(gui, ListBoxType.HUDAndLCD, WindowSystem.Label.LabelType.EntityTypeTooltip);
            grid.IsOuterGrid = false;
           // grdNutrition.DebugTag = "grdNutrition";
            grid.FixedItemHeights = true;
            grid.ScrollBarEnabled = false;
            grid.ItemHeight = gridLineHeight; //22; 
            grid.CanGrowInHeight = true;         
            grid.Font = GUIManager.LCDandHUDBodyFontPath;
            grid.Height = 160; // 40; // 160
            SetMarginAndWidth(grid);
            grdGeneralOuter.AddEntry(grid, grid);

            return grid;
        }

        void TooltipPadlock_Click(UIComponent sender, EventArgs e)
        {
            Expedition expedition = The.InGameUI.GetExpedition(); // The.Sim.PlaySite.GetFirstPlayerExpedition();
            if (expedition == null)
                return;

            ProductionOrderControl.Padlock_Click(sender, e);

            ProductionTargetEventArgs prodArgs = e as ProductionTargetEventArgs;
            EntityGroup owner = expedition.OwnedEntities;

            PopulateOrdersRefresh(owner);
            //UpdateItemRow(sender.Parent, prodArgs.Item, owner);
        }

        Dictionary<EntityType, InventoryPanel.Availability> allAvailableItems = new Dictionary<EntityType,InventoryPanel.Availability>();

        void PopulateOrdersRefresh(EntityGroup owner)
        {
            int noOfAvailableItems;

            allAvailableItems.Clear();
            InventoryPanel.CountItems(owner.AllEntities, 
                owner, allAvailableItems, false, entityType);
       

            productionOrderControl.UpdateOrders(owner, allAvailableItems, out noOfAvailableItems);


        }

        public override void OnSetProduction()
        {
            productionOrderControl.ResetSliderBeingDragged();

            //sliderIsBeingDragged = false;
            //sliderBeingDragged = null;

            EntityGroup resolvedOwner;
            // resolve the owner - if this fails, availabilitiy will show unavailable...
            ResolveOwner(out resolvedOwner);

            PopulateOrdersRefresh(resolvedOwner);           
        }

      
        private void tbItems_Click(UIComponent sender, EventArgs e)
        {          
            // open the items panel
           
            StockButton stockButton = sender as StockButton;

            The.InGameUI.EntityListWindow.SetDataSource(stockButton.EntityType, stockButton.EntityList); // expedition.OwnedEntities.AllEntities[entityType]);
            //expedition.OwnedEntities.ID, null);

            The.InGameUI.EntityListWindow.OpenNextToStockButton(sender);
            //The.InGameUI.EntityListWindow.ShowOnPlayfield(sender.AbsolutePosition.X + 26, sender.AbsolutePosition.Y);            

        }

       
        protected override void CreateProductionPanelContents()
        {
            settingsHeader = AddSubHeader(grdProductionOuter, "IN STOCK / ORDERS:", out lblSettings); //was: "ORDERS:"
            settingsHeader.OrderByTag1 = ordersIndex;


            /*
            usedInHeader = AddSubHeader(grdProductionOuter, "USED IN:", out lblUsedIn);      
            usedInHeader.OrderByTag1 = usedInIndex;

            grdUsedIn = new Grid(gui, ListBoxType.HUDAndLCD, WindowSystem.Label.LabelType.EntityTypeTooltip);
            grdUsedIn.IsOuterGrid = false; // true; // false;
            grdUsedIn.DebugTag = "grdOutputs";
            grdUsedIn.X = sideMargin;
            grdUsedIn.FixedItemHeights = true; 
            grdUsedIn.Width = grdProductionOuter.Width;  
            grdUsedIn.ScrollBarEnabled = false;
            grdUsedIn.ItemHeight = itemHeight; 
            grdUsedIn.CanGrowInHeight = true;     
            grdUsedIn.Font = GUIManager.LCDandHUDBodyFontPath;
            grdUsedIn.Height = 160; 
            grdUsedIn.OrderByTag1 = usedInIndex + 1f;
            grdProductionOuter.AddEntry(grdUsedIn, grdUsedIn);*/

            CreateGridAndHeader(grdProductionOuter, out usedInHeader, "USED IN:", "Used as a material in these objects", // "Used as a tool for making these objects",
             usedInIndex, out grdUsedIn, out lblUsedIn);


            CreateGridAndHeader(grdProductionOuter, out toolUsedForHeader, "USED FOR:", "Used as a tool for making these objects", 
                toolUsedForIndex, out grdToolUsedFor, out lbltoolUsedFor);

         //   grdProductionOuter.AddEntry(lbltoolUsedForCapNotice, lbltoolUsedForCapNotice);

            lbltoolUsedForCapNotice = new Label(gui);
            lbltoolUsedForCapNotice.Init(Label.LabelType.HUDWindow);
            lbltoolUsedForCapNotice.Text = "...Used for more objects than shown!";
            lbltoolUsedForCapNotice.FitToText();
            lbltoolUsedForCapNotice.OrderByTag1 = toolUsedForCapMessageIndex;
            lbltoolUsedForCapNotice.X = 35;
        }

      

        private void CreateOrders()
        {
            ProductionTargetEventArgs eventArgs = new ProductionTargetEventArgs(entityType, null);
            productionOrderControl = new ProductionOrderControl(entityType, eventArgs, DisplayWindow.guiManager, ProductionOrderControl.UILayout.HUD, 83, 
                tbItems_Click, TooltipPadlock_Click);


         /*   productionSettings = new UIComponent(gui);
           
            InventoryPanel.AddProductionControls(entityType, productionSettings, eventArgs, 0, 83, //out fbOrders, 
                tbItems_Click, fillableBar_SliderMouseDown, TooltipPadlock_Click);
            */
            productionOrderControl.X = sideMargin + 5;
            grdProductionOuter.AddEntry(productionOrderControl, productionOrderControl);
            productionOrderControl.OrderByTag1 = ordersIndex + 1;
        }

        public override void Hide()
        {
            base.Hide();

            productionOrderControl.ResetSliderBeingDragged(); 
        }

        void btTrack_Click(UIComponent sender, EventArgs e)
        {
            ImageButton tb = (ImageButton)sender;
            EntityType entityType = (EntityType)tb.Tag1;

            bool isTrackedNow = The.InGameUI.InventorySettings.ToggleTracking(entityType);
        }


        private Color GetStockButtonColor(EntityType entityType, bool isToolContext)
        {
            EntityGroup resolvedOwner;
            // resolve the owner - if this fails, availabilitiy will show unavailable...
            ResolveOwner(out resolvedOwner);

            if(resolvedOwner == null)
            resolvedOwner = UWGame.SimSide.Snapshots.LookUp<EntityGroup, EntityGroupID>.FindByID(The.InGameUI.UIOwner);

            bool available = GetIsAvailable(entityType, isToolContext, resolvedOwner);


            return DataTypeButton.GetStockStatusColor(available, Color.White, WindowSystem.TextButton.TextButtonType.LCDToolTipBlack);
        }

        private static bool GetIsAvailable(EntityType entityType, bool isToolContext, EntityGroup resolvedOwner)
        {
            int noOfAvailableItemsIncludingIntrinsic;
            int noOfAvailableItems = GetNoOfAvailableEntities(entityType, resolvedOwner, out noOfAvailableItemsIncludingIntrinsic);

            bool available;
            if (isToolContext)
            {
                available = noOfAvailableItemsIncludingIntrinsic > 0;
            }
            else
            {
                available = noOfAvailableItems > 0;
            }
            return available;
        }

        private static int GetNoOfAvailableEntities(EntityType entityType, EntityGroup resolvedOwner, out int noOfAvailableItemsIncludingIntrinsic)
        {
            int noOfIncompleteEntities;
            int noOfEntitiesUsedAsParts;
            int noOfItemsOffSite;
            int noOfItemsOwnedByOthers;
           // int noOfAvailableItemsIncludingIntrinsic;
            int noOfAvailableItems = InventoryPanel.GetNoOfAvailableEntities(resolvedOwner.AllEntities, resolvedOwner, entityType, out noOfIncompleteEntities,
                                                                                       out noOfEntitiesUsedAsParts, out noOfItemsOffSite, out noOfItemsOwnedByOthers, out noOfAvailableItemsIncludingIntrinsic,
                                                                                       null);
            return noOfAvailableItems;
        }

        

        /// <summary>
        /// tweak  the out of stock color to make it visible on the HUD background
        /// </summary>
        private readonly Color outOfStockColorForToolTip = Common.ColorFromHex("CCC86E");

        /// <summary>
        /// called by Refresh
        /// </summary>
        protected override void RefreshCollapsedFields()
        {
            base.RefreshCollapsedFields();

            RefreshHeader();
            
            /*
            RefreshTrackButton();

            RefreshTrackedColorBar(entityType, coloredBar);*/
        }

        protected override void RefreshTrackTargets()
        {
            base.RefreshTrackTargets();

            RefreshTrackButton();
            RefreshTrackedColorBar(entityType, coloredBar);
        }

        private void RefreshTrackButton()
        {
            TrackTarget trackTarget;
            if (The.InGameUI.InventorySettings.TrackedTargets.TryGetValue(entityType, out trackTarget))
            {
                btTrack.IsChecked = true;
            }
            else
            {
                btTrack.IsChecked = false;

                if (!The.InGameUI.InventorySettings.HasAvailableTrackingSlots())
                {
                    btTrack.Enabled = false;
                    btTrack.ToolTip = InventoryPanel.TrackingLimitTooltip;
                }
                else
                {
                    btTrack.Enabled = true;
                    btTrack.ToolTip = trackingButtonTooltip;
                }

            }

        }

       
        protected override void PopulateCollapsedFieldsContents()
        {
            lblName.Text = entityType.Name;
            lblName.ToolTip = entityType.Name;
            
           // lblName.MaxWidth = 145;
            
            RefreshHeader();

            if (GameData.Instance.GUIConstants.EnableFilters)
            {
                RefreshTrackTargets();
                coloredBar.X = lblName.Right + 5;

                btTrack.Tag1 = entityType;
            }
            summaryDescription.Text = entityType.SummaryDescription;
           

            if (entityType.ItemType != null)
            {
                IconInfo iconInfo;
                Rectangle rect = entityType.GetIconSprite(out iconInfo);
                this.icon.SetSkinLocation(SkinState.Normal,rect);
                this.icon.ResizeControlToFitImage();
                this.icon.Visible = true;
            }
            else if (entityType.ThumbnailSmall != null)
            {
                // use special 'thumbnails' for non-item types:
                Rectangle rect = gui.GUISpriteSheet.GetSourceRectangle(entityType.ThumbnailSmall);
                this.icon.SetSkinLocation(SkinState.Normal,rect);
                this.icon.ResizeControlToFitImage();
                this.icon.Visible = true;
            }
            
        }

        private void RefreshHeader()
        {
          //  lblName.Text = entityType.Name;
          //  lblName.MaxWidth = 145;

            lblName.NormalColor = GetStockButtonColor(entityType, entityType.IsIntrinsic()); // false);

            if (lblName.NormalColor != Color.White)
            {
                lblName.NormalColor = outOfStockColorForToolTip;
            }
        }


        protected override void PopulateGeneralDataContent()
        {
            btSelectProcess.Visible = false;
            lblProcessIndex.Visible = false;
            //lblProcessName.Visible = false;

            PopulateWeaponData();

            PopulateNutritionData();

            PopulateComfortData();

            PopulateReplenishTypeData();

            PopulateUpgradeForData();

            PopulatePossibleUpgradesData();

            PopulateDurationData();

            PopulateStorageData();

            PopulateEffectsData();

        }

        protected override void PopulateGeneralDataContentRefresh()
        {
            PopulateAmmo();

            PopulateUpgradeForDataRefresh();

            PopulatePossibleUpgradesDataRefresh();

            PopulateReplenishTypeDataRefresh();


        }

        protected override string GetDescription()
        {
            if (instanceData != null)
            {

                if (instanceData.RaceTypeDescription != null)
                {
                    return instanceData.Description + instanceData.RaceTypeDescription;
                }
                else
                {
                    return instanceData.Description + entityType.Description;
                }
            }
            else
            {
                return entityType.Description;
            }

        }

        private void PopulateComfortData()
        {
            grdGeneralOuter.TryRemoveEntry(comfortHeader);
          //  grdGeneralOuter.TryRemoveEntry(lblComfort);
           
            if (entityType.ContainerType != null)
            {
                HomeContainerType home = entityType.ContainerType as HomeContainerType;
                if (home != null && home.ResidenceType != null)
                {
                    grdGeneralOuter.AddEntry(comfortHeader, comfortHeader);
                   // grdGeneralOuter.AddEntry(lblComfort, lblComfort);
                   
                    lblComfort.Text = "COMFORT LEVEL: " + Common.PercentageToString(home.ResidenceType.ComfortLevel); 
                }
            }
        }

        private void PopulateWeaponData()
        {
            grdGeneralOuter.TryRemoveEntry(weaponHeader);
          //  grdGeneralOuter.TryRemoveEntry(lblWeapon);
            grdGeneralOuter.TryRemoveEntry(taHighlyEffective);
            grdGeneralOuter.TryRemoveEntry(lblWeaponDefenseRating);            
            grdGeneralOuter.TryRemoveEntry(grdAmmo);
            grdGeneralOuter.TryRemoveEntry(ammoHeader);
            grdGeneralOuter.TryRemoveEntry(lblWeaponNotWieldable);

            WeaponType weaponType = GetWeaponType(); // item or intrinsic

            if (weaponType != null)
            {
                Security security = GameData.Instance.AIConstants.Ratings.Security;

                grdGeneralOuter.AddEntry(weaponHeader, weaponHeader);
              //  grdGeneralOuter.AddEntry(lblWeapon, lblWeapon);                
                grdGeneralOuter.AddEntry(lblWeaponDefenseRating, lblWeaponDefenseRating);
                
                if (weaponType.IsIntrinsic == true) // no wielding by characters      
                {
                    grdGeneralOuter.AddEntry(lblWeaponNotWieldable, lblWeaponNotWieldable);      
                }                

                float defenseRating = weaponType.GetHighestDefenseRating();
                float weaponsScore = (float)defenseRating; // *security.DefenseRatingValue;

                lblWeaponDefenseRating.Text = string.Format("SECURITY RATING: {0:N2} ({1})", weaponsScore, DefenseRatingToString(defenseRating));
                lblWeaponDefenseRating.ToolTip = "This number is used in the colony's Security rating. The more highly rated weapons the colony has (up to 2 per person), the higher the colony's Security rating";


                MagazineContainerType magazineType = GetMagazineType();
                if (magazineType != null)
                {
                    grdGeneralOuter.AddEntry(ammoHeader, ammoHeader);
                    grdGeneralOuter.AddEntry(grdAmmo, grdAmmo);

                    PopulateAmmo(); // also called during refresh
                }

                grdGeneralOuter.AddEntry(taHighlyEffective, taHighlyEffective);

                taHighlyEffective.Text = "EFFECTIVE AGAINST: " + weaponType.HighlyEffectiveAgainst; //was: Highly effective against:

            }
        }

        private WeaponType GetWeaponType()
        {
            if (entityType.ItemType != null && entityType.ItemType.WeaponType != null)
            {
                return entityType.ItemType.WeaponType;
            }

            if (entityType.IntelligenceType != null
                && entityType.IntelligenceType.IntrinsicWeaponTypes != null
                && entityType.IntelligenceType.IntrinsicWeaponTypes.Count > 0)
            {
                return entityType.IntelligenceType.IntrinsicWeaponTypes[0].ItemType.WeaponType;
            }

            return null;
        }


        private void PopulateStorageData(ItemStorageType itemStorageType, bool isTrade, ref List<string> keys)
        {
            UIComponent itemRow;
            string key;
            foreach (var item in itemStorageType.StorageSpaces)
            {
                key = CreateStorageKey(isTrade, item.Key);

                Common.AddToList(ref keys, (string)key);

                StorageCondition condition = GameData.Instance.AllStorageConditions[item.Key];
                if (!grdStorage.TryGetEntry(key, out itemRow))
                {
                    if (Common.IsGreaterThan(item.Value.Capacity, 0f)) // skip Isolated with 0 capcity
                    {
                        itemRow = AddStorageRow(key, condition, item.Value, isTrade);
                    }
                    else
                    {
                        continue;
                    }
                }

                UpdateStorageRow(itemRow, item.Value);
            }


        }

        private static string CreateStorageKey(bool isTrade, string storageTypeKey)
        {
            string key;
            if (isTrade)
            {
                key = "trade" + storageTypeKey;
            }
            else
            {
                key = storageTypeKey;
            }
            return key;
        }

        /// <summary>
        /// don't refresh
        /// </summary>
        private void PopulateStorageDataOnce(IHasItemStorageType hasItemStorage, TerminalContainerType terminalContainerType)
        {
            grdStorage.BeginAddingEntries();

            List<string> keys = null;

            if (hasItemStorage != null)
            {
                PopulateStorageData(hasItemStorage.ItemStorageType, false, ref keys);               
            }

            if (terminalContainerType != null)
            {
                PopulateStorageData(terminalContainerType.OfferedForTradeStorageType, true, ref keys);
               
            }

            bool itemExists;
            grdStorage.DeleteEntries<object>(e => keys.Contains((string)e)); 

            /*
            grdStorage.DeleteEntries<StorageCondition>(e => hasItemStorage != null 
                && hasItemStorage.ItemStorageType.StorageSpaces.ContainsKey(e.KeyName)
                && Common.IsGreaterThan(hasItemStorage.ItemStorageType.StorageSpaces[e.KeyName].Capacity, 0f)); 
            */

            grdStorage.EndAddingEntries();
        }


       /* private bool StorageItemExists(string key, IHasItemStorageType hasItemStorage, TerminalContainerType terminalContainerType)
        {
           
            if (hasItemStorage != null)
            {
                 object itemKey = CreateStorageKey(false, key);

                hasItemStorage.ItemStorageType.StorageSpaces.ContainsKey(e.KeyName)
                && Common.IsGreaterThan(hasItemStorage.ItemStorageType.StorageSpaces[e.KeyName].Capacity, 0f)

            }
        }*/

        /// <summary>
        /// gets refreshed to show availability...
        /// </summary>
        private void PopulateAmmo()
        {
            grdAmmo.BeginAddingEntries();
         
            MagazineContainerType magazineType = GetMagazineType();

            if (magazineType != null)
            {
                EntityGroup resolvedOwner;
                // resolve the owner - if this fails, availabilitiy will show unavailable...
                ResolveOwner(out resolvedOwner);

                UIComponent itemRow;
                foreach (var item in magazineType.AmmoEntityTypes)
                {
                    
                    if (!grdAmmo.TryGetEntry(item, out itemRow))
                    {
                        itemRow = AddEntityTypeItemRow(grdAmmo, item); // AddAmmoItemRow(item);
                    }

                    UpdateItemRow(item, itemRow, resolvedOwner);
                }
            }

            grdAmmo.DeleteEntries<EntityType>(e => magazineType != null && magazineType.AmmoEntityTypes.Contains(e));

            grdAmmo.EndAddingEntries();
        }

        private MagazineContainerType GetMagazineType()
        {
            MagazineContainerType magazineType = null;

            // the sentry has ammo in its intrinsic weapon
            if (entityType.IntelligenceType != null
                && entityType.IntelligenceType.IntrinsicWeaponTypes != null)
            {
                foreach (var item in entityType.IntelligenceType.IntrinsicWeaponTypes)
                {
                    if (item.ContainerType != null && item.ContainerType is MagazineContainerType)
                    {
                        magazineType = item.ContainerType as MagazineContainerType;
                        break;
                    }
                }
            }

            if (magazineType == null)
            {
                magazineType = entityType.ContainerType as MagazineContainerType;
            }
            return magazineType;
        }

        private UIComponent AddEntityTypeItemRow(Grid grid, EntityType inputEntityType)
        {
            UIComponent itemRow = new UIComponent(gui);
            grid.AddEntry(inputEntityType, itemRow);

            DataTypeButton tbCaption;
            CreateItemGridRow(inputEntityType, itemRow, out tbCaption);

            itemRow.CenterChildVertically(tbCaption);

            return itemRow;
        }

        private UIComponent AddEffectRow(Grid grid, EffectType effectType, out string text)
        {
          /*  UIComponent itemRow = new UIComponent(gui);
             grid.AddEntry(effectType, itemRow);
            */


            StringBuilder stringBuilder = new StringBuilder();
            effectType.AppendAsString(stringBuilder, EffectType.Background.EntityTypeTooltipGreen);

            text = stringBuilder.ToString();
            UIComponent itemRow = grid.AddEntry(effectType, text, leftMargin: 5 /* 36 */ );
          

          /*  DataTypeButton tbCaption;
            CreateItemGridRow(effectType, itemRow, out tbCaption);

            itemRow.CenterChildVertically(tbCaption);
            */

            return itemRow;
        }

        private UIComponent AddAmmoItemRow(EntityType inputEntityType)
        {
            UIComponent itemRow = new UIComponent(gui);
            grdAmmo.AddEntry(inputEntityType, itemRow);


            DataTypeButton tbCaption;
            CreateItemGridRow(inputEntityType, itemRow, out tbCaption);

           // tbCaption.SetStockStatusColor(ownsItem);
            itemRow.CenterChildVertically(tbCaption);

          /*  Label lblAmount = new Label(gui);
            lblAmount.Init(Label.LabelType.HUDWindow);
            lblAmount.Text = input.Amount.AmountToString();
            lblAmount.X = tbCaption.Right + doubleSpacing;
            itemRow.Add(lblAmount);
            lblAmount.ToolTip = "The amount of materials that are needed";
           
            RightJustify(lblAmount);

            // item row has a height now that its been added:
            itemRow.CenterChildVertically(lblAmount); 
            //hacky           
            lblAmount.Y++;*/

            return itemRow;
        }

        private void UpdateItemRow(EntityType item, UIComponent itemRow, EntityGroup resolvedOwner, bool isToolContext = false) 
        {
            DataTypeButton tbCaption = (DataTypeButton)itemRow.FindChildById(UIComponent.DataControlID.Caption);

            bool available = GetIsAvailable(entityType, isToolContext, resolvedOwner);

            /*
            int noOfAvailableItemsIncludingIntrinsic;
            int noOfAvailableItems = GetNoOfAvailableEntities(item, resolvedOwner, out noOfAvailableItemsIncludingIntrinsic);
            */
          
            tbCaption.SetAvailableStatusColor(available); // noOfAvailableItems > 0);
        }

        private string DefenseRatingToString(float rating)
        {
            if (rating == 0f )
            {
                return "None";
            }
            else if (rating <= BaseDataLoader.VeryLowDefenseRating) // hmmmm
            {
                return "Very low";
            }
            else if (rating <= BaseDataLoader.LowDefenseRating) // hmmmm
            {
                return "Low";
            }
            else if (rating <= BaseDataLoader.MiddleDefenseRating) // hmmmm
            {
                return "Middle";
            }
            else if (rating <= BaseDataLoader.HighDefenseRating) // hmmmm
            {
                return "High";
            }
            else return "Highest";
           

           /* switch(rating)
            {
                case DefenseRatings.None:
                    return "None";

                case DefenseRatings.VeryLow:
                    return "Very low";

                case DefenseRatings.Low:
                    return "Low";

                case DefenseRatings.Middle:
                    return "Middle";

                case DefenseRatings.High:
                    return "High";

                case DefenseRatings.Highest:
                    return "Highest";

                default: return "";
            }*/
        }

        private void PopulateDurationData()
        {
            List<StorageDuration> storageDurations = null;

            if (entityType.NonLivingType != null)
            {
                storageDurations = entityType.NonLivingType.GetRepresentativeStorageConditions(entityType);
            }

            if (storageDurations != null && storageDurations.Count > 0)
            {
                UIComponent header;
                if (!grdGeneralOuter.TryGetEntry(durabilityHeader, out header))
                {
                    grdGeneralOuter.AddEntry(durabilityHeader, durabilityHeader);
                    grdGeneralOuter.AddEntry(lblDegradeType, lblDegradeType);
                    grdGeneralOuter.AddEntry(grdDurability, grdDurability);
                }

                PopulateDurationDataOnce(storageDurations);
            }
            else
            {
                grdGeneralOuter.TryRemoveEntry(durabilityHeader);
                grdGeneralOuter.TryRemoveEntry(lblDegradeType);
                grdGeneralOuter.TryRemoveEntry(grdDurability);
            }
        }

        private void PopulateStorageData()
        {
            IHasItemStorageType hasItemStorage = null;
            TerminalContainerType terminalContainerType = null;

            if (entityType.ContainerType != null)
            {
                hasItemStorage = entityType.ContainerType as IHasItemStorageType;
                terminalContainerType = entityType.ContainerType as TerminalContainerType;
            }

            

            if (hasItemStorage != null || terminalContainerType != null)
            {
                UIComponent header;
                if (!grdGeneralOuter.TryGetEntry(storageHeader, out header))
                {
                    grdGeneralOuter.AddEntry(storageHeader, storageHeader);
                    grdGeneralOuter.AddEntry(grdStorage, grdStorage);
                }

                PopulateStorageDataOnce(hasItemStorage, terminalContainerType);
            }
            else
            {
                grdGeneralOuter.TryRemoveEntry(storageHeader);             
                grdGeneralOuter.TryRemoveEntry(grdStorage);
            }
        }

        private void PopulateEffectsData()
        {
            /* attacks, food, items, upgraders */

            List<EffectProfileType> effects = entityType.GetEffectProfiles();

            if (effects != null && effects.Count > 0)
            {
                UIComponent header;
                if (!grdGeneralOuter.TryGetEntry(effectsHeader, out header))
                {
                    grdGeneralOuter.AddEntry(effectsHeader, effectsHeader);
                    grdGeneralOuter.AddEntry(grdEffects, grdEffects);
                }

                PopulateEffectsDataOnce(effects);
            }
            else
            {
                grdGeneralOuter.TryRemoveEntry(effectsHeader);
                grdGeneralOuter.TryRemoveEntry(grdEffects);
            }
        }

        private void PopulatePossibleUpgradesData()
        {

            if (entityType.ContainerType != null && entityType.ContainerType.CanBeUpgraded)
            {
                UIComponent header;
                if (!grdGeneralOuter.TryGetEntry(possibleUpgradesHeader, out header))
                {
                    grdGeneralOuter.AddEntry(possibleUpgradesHeader, possibleUpgradesHeader);
                    grdGeneralOuter.AddEntry(grdPossibleUpgrades, grdPossibleUpgrades);
                }

                PopulatePossibleUpgradesDataRefresh();
            }
            else
            {
                grdGeneralOuter.TryRemoveEntry(possibleUpgradesHeader);
                grdGeneralOuter.TryRemoveEntry(grdPossibleUpgrades);
            }

            /*
            grdGeneralOuter.TryRemoveEntry(upgradeForHeader);
            grdGeneralOuter.TryRemoveEntry(grdUpgradeFor);

            Upgrader upgrader = entityType.Upgrader;

            if (upgrader != null)
            {
                grdGeneralOuter.AddEntry(upgradeForHeader, upgradeForHeader);
                grdGeneralOuter.AddEntry(grdUpgradeFor, grdUpgradeFor);

                PopulateUpgradeForDataRefresh();
            }
            */
        }


        private void PopulateUpgradeForData()
        {            
            Upgrader upgrader = entityType.Upgrader;

            if (upgrader != null)
            {
                UIComponent header;
                if (!grdGeneralOuter.TryGetEntry(upgradeForHeader, out header))
                {
                    grdGeneralOuter.AddEntry(upgradeForHeader, upgradeForHeader);
                    grdGeneralOuter.AddEntry(grdUpgradeFor, grdUpgradeFor);
                }

                PopulateUpgradeForDataRefresh();
            }
            else
            {
                grdGeneralOuter.TryRemoveEntry(upgradeForHeader);
                grdGeneralOuter.TryRemoveEntry(grdUpgradeFor);
            }

            /*
            grdGeneralOuter.TryRemoveEntry(upgradeForHeader);
            grdGeneralOuter.TryRemoveEntry(grdUpgradeFor);

            Upgrader upgrader = entityType.Upgrader;

            if (upgrader != null)
            {
                grdGeneralOuter.AddEntry(upgradeForHeader, upgradeForHeader);
                grdGeneralOuter.AddEntry(grdUpgradeFor, grdUpgradeFor);

                PopulateUpgradeForDataRefresh();
            }
            */
        }


        private UIComponent AddStorageRow(object key, StorageCondition storageCondition, StorageType storageType, bool isTrade)
        {
            UIComponent itemRow;

            string name = storageCondition.Name;
            if (isTrade)
            {
                name += " (trading)";
            }

            itemRow = grdStorage.AddEntryRightJustifyValue(key, null, null, 5, name, 18,
               Common.DecimalToStringSignificant(storageType.Capacity),
               "The storage capacity in BLK",
               storageCondition.Description);

            itemRow.OrderByTag1 = storageCondition.Name; 


            return itemRow;
        }

        private void UpdateStorageRow(UIComponent itemRow, StorageType storageType)
        {
            Label lblValue;
            itemRow.FindChildById(UIComponent.DataControlID.Value, out lblValue);

            lblValue.Text = Common.DecimalToStringSignificant(storageType.Capacity);


        }

        private UIComponent AddDurationRow(StorageDuration duration)
        {
            UIComponent itemRow; 

            itemRow = grdDurability.AddEntryRightJustifyValue(duration, null, null, 5, duration.StorageDurationToDisplay.DisplayName, 18, 
                Common.DecimalToStringSignificant(duration.Duration),
                "The number of days the object will last when exposed to this condition. \nStructures can often have their lifespan extended with regular maintenance. Items CANNOT.", //was: "The number of days this item will last when stored under this condition"
                duration.StorageDurationToDisplay.Tooltip ?? duration.StorageCondition.Description);
           
            itemRow.OrderByTag1 = duration.SortOrder; 

            return itemRow;
        }

        /// <summary>
        /// we don't ned to refresh this grid, the data is static.
        /// </summary>
        private void PopulateDurationDataOnce(List<StorageDuration> storageDurations)
        {
            if (storageDurations != null)
            {

                lblDegradeType.Text = "'" + entityType.NonLivingType.FinalDegradeType.Name + "'";
                lblDegradeType.FitToText();
                lblDegradeType.ToolTip = entityType.NonLivingType.FinalDegradeType.Description; // "The name of the degrade profile for the object";
           
                EntityGroup resolvedOwner;
                ResolveOwner(out resolvedOwner);

                UIComponent itemRow;

                grdDurability.BeginAddingEntries();

              
                foreach (var storageDuration in storageDurations)
                {
                   // profileSortingNumber = effectProfile.SortOrder * 500f; // make room for items between categories
                   // UIComponent item;
                    if (!grdDurability.TryGetEntry(storageDuration, out itemRow))
                    {
                        itemRow = AddDurationRow(storageDuration);
                       // header.OrderByTag1 = profileSortingNumber;
                    }
                }

                // delete items only:              
                grdDurability.DeleteEntries<StorageDuration>(e => storageDurations.Contains(e));

              
                grdDurability.Sort(Grid.Sorting.Descending, true);
                
                grdDurability.EndAddingEntries();
            }
            else
            {
                lblDegradeType.Text = "";
                grdDurability.Clear();
            }
        }

        /// <summary>
        /// we don't ned to refresh this grid, the data is static.
        /// </summary>
        private void PopulateEffectsDataOnce(List<EffectProfileType> effectProfileTypes)
        {
            if (effectProfileTypes != null)
            {
                // for headers, EffectProfileType is used as key
                // for items, EffectType


                EntityGroup resolvedOwner;
                ResolveOwner(out resolvedOwner);

                UIComponent itemRow;

                grdEffects.BeginAddingEntries();

                float profileSortingNumber;

                HashSet<EffectType> encounteredEffects = null;

                // put everything in one grid.
                // loop over categories,
                // add a header item for each category

                foreach (var effectProfile in effectProfileTypes)
                {
                    profileSortingNumber = effectProfile.SortOrder * 500f; // make room for items between categories
                    UIComponent header;
                    if (!grdEffects.TryGetEntry(effectProfile, out header))
                    {
                        header = grdEffects.AddEntry(effectProfile, effectProfile.Name.ToUpper(Config.Culture), leftMargin: 5);
                        header.OrderByTag1 = profileSortingNumber;
                    }


                    foreach (var item in effectProfile.EffectTypes)
                    {
                        if (!grdEffects.TryGetEntry(item, out itemRow))
                        {
                            string text;
                            itemRow = AddEffectRow(grdEffects, item, out text);
                            itemRow.OrderByTag1 = profileSortingNumber + (float)text[0]; // compose a number to sort by from the first letter and the category
                        }

                        //  UpdateItemRow(item, itemRow, resolvedOwner);

                        Common.AddToList(ref encounteredEffects, item);
                    }

                }

                // delete items only:              
                Grid.DeleteEntriesWithMixedKeyTypes(grdEffects, e => !(e is EffectType) || (encounteredEffects != null && encounteredEffects.Contains(e)));

                // delete the header row if no effect types were encountered in that category.
                // sort, to get headers in the right order:
                grdEffects.Sort(Grid.Sorting.Ascending, true);

                List<object> keysToRemove = null; // = new List<object>();
                EffectProfileType lastHeader = null;
                foreach (var entry in grdEffects.Entries)
                {
                    if (!(entry.Tag1 is EffectType))
                    {
                        if (lastHeader != null)
                        {
                            // remove the last header, as it has no items under it
                            Common.AddToList(ref keysToRemove, lastHeader);
                        }

                        lastHeader = entry.Tag1 as EffectProfileType;
                    }
                    else
                    {
                        lastHeader = null;
                    }
                }

                // remove the final item too if it is a header:
                if (grdEffects.Count > 0)
                {
                    UIComponent lastEntry = grdEffects.Entries[grdEffects.Entries.Count - 1];
                    if (lastEntry.Tag1 is EffectProfileType)
                    {
                        Common.AddToList(ref keysToRemove, lastEntry.Tag1); // lastEntry);
                    }
                }

                if (keysToRemove != null)
                {
                    foreach (var deleteKey in keysToRemove)
                    {
                        grdEffects.RemoveEntry(deleteKey);
                    }
                }

                grdEffects.EndAddingEntries();
            }
            else
            {
                grdEffects.Clear();
            }
        }


        /// <summary>
        /// refresh to show availability
        /// </summary>
        private void PopulatePossibleUpgradesDataRefresh()
        {
            if (entityType.ContainerType != null && entityType.ContainerType.CanBeUpgraded)
            {
                // for headers, UpgradeCategory is used as key
                // for items, EntityType


                EntityGroup resolvedOwner;
                ResolveOwner(out resolvedOwner);

                UIComponent itemRow;

                grdPossibleUpgrades.BeginAddingEntries();
                
                float categorySortingNumber;

                HashSet<EntityType> encounteredUpgrades = null;

                // put everything in one grid.
                // loop over categories,
                // add a header item for each category
                var categories = entityType.ContainerType.GetUpgradeOptions();
                foreach (var category in categories)
                {
                    categorySortingNumber = category.SortOrder * 500f; // make room for items between categories
                    UIComponent header;
                    if (!grdPossibleUpgrades.TryGetEntry(category, out header))
                    {                        
                        header = grdPossibleUpgrades.AddEntry(category, category.Name.ToUpper(Config.Culture), leftMargin: 5);
                        header.OrderByTag1 = categorySortingNumber;
                    }

                    List<EntityType> upgrades;
                    if (GameData.Instance.UpgraderEntityTypesByUpgradeCategory.TryGetValue(category, out upgrades))
                    {
                        foreach (var item in upgrades)
                        {
                            if (!grdPossibleUpgrades.TryGetEntry(item, out itemRow))
                            {
                                itemRow = AddEntityTypeItemRow(grdPossibleUpgrades, item);
                                itemRow.OrderByTag1 = categorySortingNumber + (float)item.Name[0]; // compose a number to sort by from the first letter and the category
                            }

                            UpdateItemRow(item, itemRow, resolvedOwner);

                            Common.AddToList(ref encounteredUpgrades, item);
                        }
                    }
                }

                // delete items only:              
                Grid.DeleteEntriesWithMixedKeyTypes(grdPossibleUpgrades, e => !(e is EntityType) || (encounteredUpgrades != null && encounteredUpgrades.Contains(e)));

                // delete the header row if no entity types were encountered in that category.
                // sort, to get headers in the right order:
                grdPossibleUpgrades.Sort(Grid.Sorting.Ascending, true);

                List<object> keysToRemove = null; // = new List<object>();
                UpgradeCategory lastHeader = null;
                foreach (var entry in grdPossibleUpgrades.Entries)
                {
                    if (!(entry.Tag1 is EntityType))
                    {
                        if (lastHeader != null)
                        {
                            // remove the last header, as it has no items under it
                            Common.AddToList(ref keysToRemove, lastHeader);
                        }
                        
                        lastHeader = entry.Tag1 as UpgradeCategory;                        
                    }
                    else
                    {
                        lastHeader = null;
                    }                    
                }

                // remove the final item too if it is a header:
                if (grdPossibleUpgrades.Count > 0)
                {
                    UIComponent lastEntry = grdPossibleUpgrades.Entries[grdPossibleUpgrades.Entries.Count - 1];
                    if (lastEntry.Tag1 is UpgradeCategory)
                    {
                        Common.AddToList(ref keysToRemove, lastEntry.Tag1); // lastEntry);
                    }
                }

                if (keysToRemove != null)
                {
                    foreach (var deleteKey in keysToRemove)
                    {
                        grdPossibleUpgrades.RemoveEntry(deleteKey);
                    }
                } 
                
                grdPossibleUpgrades.EndAddingEntries();
            }
            else
            {
                grdPossibleUpgrades.Clear();
            }
        }

        /// <summary>
        /// refresh to show availability
        /// </summary>
        private void PopulateUpgradeForDataRefresh()
        {
            Upgrader upgrader = entityType.Upgrader;

            if (upgrader != null)
            {
                EntityGroup resolvedOwner;
                ResolveOwner(out resolvedOwner);

                UIComponent itemRow;

                grdUpgradeFor.BeginAddingEntries();              

                HashSet<EntityType> canUpgrade = new HashSet<EntityType>();
                foreach (var item in upgrader.UpgradeCategoryFinal)
                {
                    var types = GameData.Instance.EntityTypesToUpgradeByUpgradeCategory[item];
                    foreach (var canUpgradeType in types)
                    {
                        canUpgrade.Add(canUpgradeType);
                    }
                }

               // var canUpgrade = GameData.Instance.EntityTypesToUpgradeByUpgradeCategory[upgrader.UpgradeCategoryFinal];

                foreach (var item in canUpgrade)
                {
                    if (!grdUpgradeFor.TryGetEntry(item, out itemRow))
                    {
                        itemRow = AddEntityTypeItemRow(grdUpgradeFor, item);
                    }

                    UpdateItemRow(item, itemRow, resolvedOwner);
                }


                grdUpgradeFor.DeleteEntries<EntityType>(e => upgrader != null && e.CanBeUpgradedBy(entityType));

                grdUpgradeFor.EndAddingEntries();

            }
            else
            {
                grdUpgradeFor.Clear();
            }
        }



        private RequiresReplenishType GetRequiresReplenishType()
        {
            if (entityType.ContainerType != null)
            {
                RequiresReplenishType requiresReplenishType = entityType.ContainerType.GetRequiresReplenishType();
                if (requiresReplenishType != null)
                {
                    return requiresReplenishType;
                }
            }

            return null;
        }

        private void PopulateReplenishTypeData()
        {
            grdGeneralOuter.TryRemoveEntry(replenishHeader);
            grdGeneralOuter.TryRemoveEntry(grdRequiresReplenish);

            RequiresReplenishType requiresReplenishType = GetRequiresReplenishType();

            if (requiresReplenishType != null 
                && requiresReplenishType.RequiresFuelType != null)
            {
                grdGeneralOuter.AddEntry(replenishHeader, replenishHeader);
                grdGeneralOuter.AddEntry(grdRequiresReplenish, grdRequiresReplenish);

                PopulateReplenishTypeDataRefresh();
            }

        }

        private void PopulateReplenishTypeDataRefresh()
        {
            RequiresReplenishType requiresReplenishType = GetRequiresReplenishType();

            if (requiresReplenishType != null)
            {
                grdRequiresReplenish.BeginAddingEntries();

                EntityGroup resolvedOwner;
                ResolveOwner(out resolvedOwner);

                if (requiresReplenishType.RequiresFuelType != null)
                {
                    UIComponent itemRow;
                    foreach (var item in requiresReplenishType.RequiresFuelType.FuelEntityTypes)
                    {
                        if (!grdRequiresReplenish.TryGetEntry(item, out itemRow))
                        {
                            itemRow = AddEntityTypeItemRow(grdRequiresReplenish, item);
                        }

                        UpdateItemRow(item, itemRow, resolvedOwner);
                    }
                }

                grdRequiresReplenish.DeleteEntries<EntityType>(e => requiresReplenishType.RequiresFuelType != null && requiresReplenishType.RequiresFuelType.FuelEntityTypes.Contains(e));

                grdRequiresReplenish.EndAddingEntries();

            }
            else
            {
                grdRequiresReplenish.Clear();
            }           
        }

        private void PopulateNutritionData()
        {
            grdGeneralOuter.TryRemoveEntry(humanEdibleHeader);
            grdGeneralOuter.TryRemoveEntry(nutritionHeader);
            grdGeneralOuter.TryRemoveEntry(grdNutrition);

            if (entityType.ItemType != null && entityType.ItemType.FoodType != null)
            {
                grdGeneralOuter.AddEntry(humanEdibleHeader, humanEdibleHeader);
                grdGeneralOuter.AddEntry(nutritionHeader, nutritionHeader);

                BiologicalType bioType = The.InGameUI.UIAllegiance.RepresentativeEntityType.BiologicalType;
                if (bioType != null)
                {
                    string speciesName = bioType.SpeciesPlural.ToUpper(Config.Culture);
                    if (bioType.ConsumeProcesses.ContainsKey(entityType))
                    {
                        lblHumanEdible.Text = "EDIBLE TO " + speciesName;
                    }
                    else
                    {
                        lblHumanEdible.Text = "INEDIBLE TO " + speciesName + " IN THIS CONDITION";
                    }
                }

                grdGeneralOuter.AddEntry(grdNutrition, grdNutrition);

                grdNutrition.BeginAddingEntries();
                grdNutrition.Clear();

                if (entityType.ItemType.MaximumBulk.HasValue)
                {
                    EntityType consumer = The.InGameUI.UIAllegiance.RepresentativeEntityType;
                    float weight;
                    NeedType[] needs = consumer.BiologicalType.GetAdultNeedsAndWeight(out weight);

                    
                    foreach (var item in entityType.ItemType.FoodType.FoodNutrientProfile.FoodNutrientTypes)
                    {
                        float amount = item.Amount;

                        string percentage = item.GetSatisfiedDailyIntake(entityType.ItemType.MaximumBulk.Value, needs, weight);
                                               

                        if (percentage != null)
                        {
                            grdNutrition.AddEntryRightJustifyValue(item.Nutrient.KeyName, null, null, null, item.Nutrient.Name, 0, percentage, "Of recommended daily intake for an adult human"); //or "Satisfied daily needs for an adult human"

                            /* TODO: make an Effects list instead
                            // show other effects than comfort..?
                            string comfortRating = null;
                            NeedType needType;
                            if (item.SatisfiesComfortNeed(needs, out needType))
                            {
                                comfortRating = Common.PercentageToString(needType.ComfortEffects.ComfortWeight);
                            }

                            if (comfortRating != null)
                            {
                                // CONTRIBUTES TO: COMFORT (5%)    
                                 grdNutrition.AddEntry(item.Nutrient.KeyName + "C", string.Format("{0}CONTRIBUTES TO: COMFORT ({1})", Common.indentString, comfortRating));                               
                            }*/
                        }
                       
                    }
                }

                grdNutrition.EndAddingEntries();
            }
        }

       
        protected override ProcessType GetProcessToShow()
        {
            ProcessType processTypeToShow = null;
            List<ProcessType> listOfProcesses;
            if (GameData.Instance.ProcessYieldsThisOutput.TryGetValue(entityType, out listOfProcesses))
            {
                var sortedProcesses = listOfProcesses.OrderByDescending(p => (p.IsGathering ? 1 : 0)).ToList();

                processTypeToShow = sortedProcesses[processIndex]; // listOfProcesses[processIndex];
            }

            return processTypeToShow;
        }



        private UIComponent AddUsedForItemRow(EntityType outputEntityType, float score)
        {
            UIComponent itemRow = new UIComponent(gui);
            grdToolUsedFor.AddEntry(outputEntityType, itemRow);

            DataTypeButton tbCaption;
            CreateItemGridRow(outputEntityType, itemRow, out tbCaption);
                        
            itemRow.OrderByTag1 = score; 
            itemRow.OrderByTag2 = outputEntityType.Name;

            return itemRow;
        }

        private UIComponent AddUsedInItemRow(EntityType outputEntityType, float score,  bool hasInputs, bool hasTools, bool ownsItem)
        {
            UIComponent itemRow = new UIComponent(gui);
            grdUsedIn.AddEntry(outputEntityType, itemRow);

            //bool hasInputs, hasTools, ownsItem;
                    
            DataTypeButton tbCaption;
            CreateItemGridRow(outputEntityType, itemRow, out tbCaption);

           
         /*   Bar coloredBar = The.InGameUI.InventoryPanel.GetColoredBar(outputEntityType, tbCaption.Right, 15);           
            itemRow.Add(coloredBar);*/
            
            itemRow.OrderByTag1 = score; // ScoreItem(outputEntityType, 0f, out hasInputs, out hasTools, out ownsItem);
            itemRow.OrderByTag2 = outputEntityType.Name;

            return itemRow;
        }
        /*

        /// <summary>
        /// belongs in EntityTypeTooltip since processes don't have policy requirements
        /// </summary>
         private void PopulatePolicy()
        {
            grdProductionOuter.TryRemoveEntry(policyHeader);
            //grdProductionOuter.TryRemoveEntry(lblSkill);

            if (entityType.TierAreaType != null)
            {
               
                policyIcon.SetSkinLocation(SkinState.Normal, gui.GUISpriteSheet.GetSourceRectangle(entityType.TierAreaType.Icon));
                policyIcon.ResizeControlToFitImage();               
                policyIcon.ToolTip = Common.ComposeHeadingAndBlobText(entityType.TierAreaType.ToString(), entityType.TierAreaType.Description);
                policyIcon.AlignRight(policyHeader.Width - rightMargin);

                grdProductionOuter.AddEntry(policyHeader, policyHeader);
               // grdProductionOuter.AddEntry(lblSkill, lblSkill);
            }

        }
        */

        protected override void PopulateProductionContentRefresh()
        {
            PopulateProcessSelector(); 

           
            

            EntityGroup resolvedOwner;
            // resolve the owner - if this fails, availabilitiy will show unavailable...
            ResolveOwner(out resolvedOwner);

            PopulateProcessName();

           // PopulatePolicy();
            PopulateUsedInList(resolvedOwner);

            PopulateToolUsedForList(resolvedOwner);

            PopulateOrdersRefresh(resolvedOwner);
            //PopulateProductionControls();

        }

/*
        private void ShowProcessSelector()
        {
            btProcess.Visible = true;
            lblProcess.Visible = true;

        }*/

        private void PopulateProcessName()
        {
            if (processTypeToShowProductionFor != null)
            {
                lblProcessIndex.Text = string.Format("{0}/{1} {2}", processIndex + 1, noOfProcesses, processTypeToShowProductionFor.Name);
                //lblProcessIndex.FitToText();

                lblProcessIndex.FitToText();

                int right = btSelectProcess.X - 2;
                int width = Math.Min(lblProcessIndex.Width, right - base.lblExpandedHeading.Right - 6);

                lblProcessIndex.Width = width;
                lblProcessIndex.AlignRight(right); //DisplayWindow.Width - doubleSpacing); 
                lblProcessIndex.ToolTip = processTypeToShowProductionFor.Name;

                /*
                lblProcessName.Text = processTypeToShowProductionFor.Name;
                lblProcessName.FitToText();
                lblProcessName.AlignRight(DisplayWindow.Width - doubleSpacing); */
            }
        }

        /// <summary>
        /// this list has the items that uses this type as input.
        /// the list is sorted by availability i.e. at the top are products that have all their material requirements met, 
        /// at the bottom are those with fewest requirements met. 
        /// products that have the same number of material requirements met are sorted alphabetically.
        /// </summary>
        private void PopulateUsedInList(EntityGroup resolvedOwner)
        {
            grdUsedIn.BeginAddingEntries();

            // Owner owner = expedition.ExpeditionOwner;


            // update the list to show availability

            List<ProcessType> processesUsingThisInput;

            if (GameData.Instance.ProcessesUsingThisInput.TryGetValue(entityType, out processesUsingThisInput))
            {
                UIComponent itemRow;
                EntityType outputEntityType;

                bool hasInputs, hasTools, ownsItem;
                float score;

                foreach (var process in processesUsingThisInput)
                {
                    if (process.Outputs != null) 
                    {
                        foreach (var output in process.Outputs) // can outputs overlap..?
                        { 
                            
                            outputEntityType = output.FinalEntityTypeToCreate;

                            if (!output.IsWasteProduct)// omit waste products
                            {

                                score = ScoreItem(resolvedOwner, outputEntityType, 0f, out hasInputs, out hasTools, out ownsItem, false);

                                if (!grdUsedIn.TryGetEntry(outputEntityType, out itemRow))
                                {
                                    itemRow = AddUsedInItemRow(outputEntityType, score, hasTools, hasInputs, ownsItem);
                                }

                                UpdateRequiredItemRow(itemRow, outputEntityType, score, hasTools, hasInputs, ownsItem);
                            }
                        }
                    }
                }

                grdUsedIn.DeleteEntries<EntityType>(e => processesUsingThisInput.Exists(p => ProcessHasEntityTypeAsOutput(p, e)));
         
                // sort the grid, first by score, then by name:              
                grdUsedIn.Sort(i => (float)i.OrderByTag1, Grid.Sorting.Descending, i => i.OrderByTag2, Grid.Sorting.Ascending);
                
            }
            else
            {
                grdUsedIn.Clear();
            }
            


            grdUsedIn.EndAddingEntries();


            // show/hide the heading:
            grdProductionOuter.TryRemoveEntry(usedInHeader);

            if (grdUsedIn.Entries.Count > 0)
            {
               // int usedInHeaderIndex = grdProductionOuter.GetIndex(grdOutputs);
                               
                grdProductionOuter.AddEntry(usedInHeader, usedInHeader); //, usedInHeaderIndex);

                if (processTypeToShowProductionFor != null && processTypeToShowProductionFor.IsKilling)
                {
                    // carcasses
                    lblUsedIn.Text = "YIELDS:";
                    lblUsedIn.ToolTip = "Yields these products";
                }
                else
                {
                    lblUsedIn.Text = "USED IN:";
                    lblUsedIn.ToolTip = string.Format("Used as a material input for making these objects (max {0} items are shown)", GameData.Instance.GUIConstants.MaxToolsToShowInUsedForList);
                }
            }         

        }


        private void PopulateToolUsedForList(EntityGroup resolvedOwner)
        {
            grdToolUsedFor.BeginAddingEntries();

            // update the list to show availability
            bool wasCapped = false;

            if (entityType.ToolType != null)
            {

                HashSet<ProcessType> processesUsingTool;

                if (GameData.Instance.ToolsUsedFor.TryGetValue(entityType, out processesUsingTool))
                {
                    UIComponent itemRow;
                    EntityType outputEntityType;

                    bool hasInputs, hasTools, ownsItem;
                    float score;

                    // show processes as well??? farming...
                    
                    // copies output/usedIn grid code:
                    foreach (var process in processesUsingTool)
                    {
                        if (process.Outputs != null)
                        {
                            foreach (var output in process.Outputs) 
                            {
                                outputEntityType = output.FinalEntityTypeToCreate;

                                if (!output.IsWasteProduct)// omit waste products
                                {

                                    score = ScoreItem(resolvedOwner, outputEntityType, 0f, out hasInputs, out hasTools, out ownsItem, false);

                                    if (!grdToolUsedFor.TryGetEntry(outputEntityType, out itemRow))
                                    {
                                        itemRow = AddUsedForItemRow(outputEntityType, score);
                                    }

                                    UpdateRequiredItemRow(itemRow, outputEntityType, score, hasTools, hasInputs, ownsItem);
                                }
                            }
                        } 
                    }

                    grdToolUsedFor.DeleteEntries<EntityType>(e => processesUsingTool.Any(p => ProcessHasEntityTypeAsOutput(p, e)));

                    // cap the number of items:
                    wasCapped = grdToolUsedFor.CapNoOfEntries(GameData.Instance.GUIConstants.MaxToolsToShowInUsedForList);


                    // sort the grid, first by score, then by name:              
                    grdToolUsedFor.Sort(i => (float)i.OrderByTag1, Grid.Sorting.Descending, i => i.OrderByTag2, Grid.Sorting.Ascending);

                }
                else
                {
                    grdToolUsedFor.Clear();
                }
            }
            else
            {
                // not a tool
                grdToolUsedFor.Clear();
            }


            grdToolUsedFor.EndAddingEntries();


            // show/hide the heading:
            grdProductionOuter.TryRemoveEntry(toolUsedForHeader);
            grdProductionOuter.TryRemoveEntry(lbltoolUsedForCapNotice);

            if (grdToolUsedFor.Entries.Count > 0)
            {
                grdProductionOuter.AddEntry(toolUsedForHeader, toolUsedForHeader);

                if (wasCapped)
                {
                    grdProductionOuter.AddEntry(lbltoolUsedForCapNotice, lbltoolUsedForCapNotice);
                }
               
            }
        }

        protected override void Retire()
        {
            The.InGameUI.poolOfEntityTypeTooltips.Retire(this);
        }

       
        private bool ProcessHasEntityTypeAsOutput(ProcessType processType, EntityType entityType)
        {
            if (processType.Outputs != null)
            {
                return processType.Outputs.FirstOrDefault(o => o.FinalEntityTypeToCreate == entityType) != null;
            }
            else
            {
                return false; // We got no outputs at all.
            }
        }

        protected override void SetProductionHeading(Label lbl)
        {
            lbl.Text = "PRODUCTION";
            if (noOfProcesses <= 1)
            {
                PadHeader(lbl);
            }
        }

        /*
        protected override string GetProductionHeading()
        {
            if (noOfProcesses <= 1)
            {
                return "PRODUCTION";
            }
            else
            {

            }
        }*/

        protected override string GetInputHeading()
        {
            return "MADE FROM:";
        }
    }
}
