using System;
using System.Collections.Generic;
using System.Text;
using WindowSystem;
using InputEventSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
//using Microsoft.Xna.Framework.Storage;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Items;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Vehicles;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Resources;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Processes;
using UWGame.SimSide;
using UWGame.SimSide.AI;
using UWGame.ClientSide.Interface.Controls;
using UWGame.Client.Interface;
using UWGame.SimSide.Commands;
using UWGame.Control.Commands;
using UWGame.ClientSide.Interface.LCD;
using System.Collections;
using System.Linq;
using UWGame.ClientSide.Interface.HUD_Windows;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Tiers;
using UWGame.SimSide.Policies;



namespace UWGame.ClientSide.Interface.Inventory
{
    public enum ViewType { Categories, List }

    /// <summary>
    /// one panel per expedition
    /// </summary>
    public class InventoryPanel : RosterPanel
    {

        /// <summary>
        /// Basic disables future orders
        /// </summary>
        public enum ProductionMode { Basic, Advanced }


        /// <summary>
        /// set this whenever the dialog is shown
        /// </summary>
        Expedition expedition;

        const int topPanelExpandedNoTrackingHeight = 112;
        const int topPanelExpandedWithTrackingHeight = 150;
        const int topPanelCollapsedHeight = 34;


        public const int DefaultStocksMaxValue = 5;
        public const int MaxStockOrder = 99;
        private const int maxNumberOfMissingInputsToDisplayBuildingsWithout = 1;
        private bool haveActiveTracking = true;

        Rectangle screenDimensions = new Rectangle(40, 700, 400, 300);

        /// <summary>
        /// when switching, keep the other populated grid in memory
        /// </summary>
        Grid grdCategoryView;
        Grid grdListView;


        /*UIComponent*/
        SortingButtons<InventorySettings.SortColumns> sortingButtons;
        UIComponent sortingButtonsContainer;

        List<Grid> categoryGrids = new List<Grid>();


        
        ViewType viewType = ViewType.Categories;

        int itemTypeIconColumnX = 12; // 22; 
        int captionX = 30; // 40;
        int availableX = 204; //214 // make room for 3 digits
        int unavailableX = 243;    
        int itemTypeProductionTargetColumnX = 300;

        LCDInnerPanel filterAndTrackingPanel;

        FilterPropertiesPanel filterPropertiesPanel;

        ComboBox cbEntityTracking; //, cbDistance;

        RadioGroup rgDistance;
        ImageButton ibNormalDistance, ibAttainableDistance, ibCyclopedia;


        ImageButton btIncludeSalvage;
        
     
       // public const string tbTrackBeingTrackedTooltip = "This item/structure is being tracked.";
        const string tbTrackNotTrackedTooltip = "Track this item/structure";
        public const string TrackingLimitTooltip = "No more objects can be tracked, cancel some of the other tracked objects first.";

    
        public const string expandFilterTooltip = "Display search options";
        public const string collapseFilterTooltip = "Hide search options";


        /// <summary>
        /// use this to avoid changing the user's setting during update after he has grabbed the slider (but not released it yet)
        /// </summary>
      //  FillableBar sliderBeingDragged = null;

  
        private HashSet<EntityType> /*Dictionary<string, EntityType>*/ currentListData;
        private Dictionary<EntityType, Availability> allAvailableItems = new Dictionary<EntityType, Availability>();

        public new const int ItemHeight = 26; // 22;
      
        RadioButton rbOr, rbAnd;
        CheckBox cbInputChain, cbOutputChain, cbToolsOption;
        ImageButton ibCategory, ibList;
        Image rowDivider1, rowDivider2, columnDividerVertical1, columnDividerVertical2;

        TextButton tbExpand;
    
        Color encyclopediaTint;

       // bool isFirstUpdate;


        public InventoryPanel()
            : base("PRODUCTION", 555, false) 
        {
            expedition = The.Sim.PlaySite.GetFirstPlayerExpedition();

            encyclopediaTint = Common.ColorFromHex("#636D8C"); // "#1A3291");
            encyclopediaTint.A = 160; // 128;

            if (GameData.Instance.GUIConstants.EnableFilters)
            {
                CreateTopPanel();
            }

       
          /*  sortingButtonsContainer = new UIComponent(Interface.gui);
            sortingButtonsContainer.Width = lcdSurface.Width;
            sortingButtonsContainer.Height = 50;
            //sortingButtonsContainer.ItemHeight = ItemHeight; 
            sortingButtonsContainer.Position = new Point(0, 160);
            lcdSurface.Add(sortingButtonsContainer);
            */

          
            CreateGridHeaderButtons();

            grdCategoryView = CreateOuterGrid(false);
            grdListView = CreateOuterGrid(true);

          /*  if (!GameData.Instance.GUIConstants.EnableFilters)
            {                
                sortingButtons.Position = new Point(0, 10);
                grdCategoryView.Position = new Point(0, 45);
                grdListView.Position = new Point(0, 45);
            }*/
            

            LoadUserSettings();

            SetView(ViewType.Categories);


            ShowRelevantGrid();

           
        }

        private void CreateGridHeaderButtons()
        {
            sortingButtonsContainer = new UIComponent(Interface.gui);
            lcdSurface.Add(sortingButtonsContainer);
            sortingButtonsContainer.Width = lcdSurface.Width;
            sortingButtonsContainer.Height = 50;
            sortingButtonsContainer.Position = new Point(0, 160);

            sortingButtons = new SortingButtons<InventorySettings.SortColumns>(Interface.gui);
            sortingButtons.Width = lcdSurface.Width;
            sortingButtons.Height = 50;
            sortingButtons.Position = new Point(60, 0);
            sortingButtonsContainer.Add(sortingButtons);
            sortingButtons.SortClicked += tbSort_Click;

            ibList = new ImageButton(Interface.gui);           
            sortingButtonsContainer.Add(ibList);           
            ibList.InitWithIcon(ImageButtonType.LCD, "basic_icon_list", true);
            ibList.CheckedMode = CheckedModes.CanBeChecked;
            ibList.Click += tbListView_Click;
            ibList.Y = 0;
            ibList.X = 0;
            ibList.ToolTip = "List view";
            ibList.Width = 30;
            ibList.Height = 30;
            ibList.RecalculateIconPosition();
         //   ibList.MouseOut += ibList_MouseOut;


            ibCategory = new ImageButton(Interface.gui);           
            sortingButtonsContainer.Add(ibCategory);           
            ibCategory.InitWithIcon(ImageButtonType.LCD, "basic_icon_category", true);
            ibCategory.CheckedMode = CheckedModes.CanBeChecked;
            ibCategory.Click += tbCategoryView_Click;
            ibCategory.ToolTip = "Category view";
            ibCategory.Width = 30;
            ibCategory.Y = 0;
            ibCategory.X = 30;
            ibCategory.IsChecked = true;
           // ibCategory.Pressed = true;        
            ibCategory.Height = 30;
            ibCategory.RecalculateIconPosition();
           // ibCategory.MouseOut += ibList_MouseOut;

            // tbListView_Click(ibList, null);

          /*  sortingButtons.CreateTextButton(0, 150, "NAME", InventorySettings.SortColumns.Name);
            sortingButtons.CreateImageButton(146, 84, "IN STOCK", InventorySettings.SortColumns.InStock);
            sortingButtons.CreateImageButton(226, 156, "CAN PRODUCE", InventorySettings.SortColumns.CanProduce);
            sortingButtons.CreateImageButton(378, 60, "TRACKED", InventorySettings.SortColumns.Tracking);
            */

            sortingButtons.CreateTextButton(0, 138, "NAME", InventorySettings.SortColumns.Name);
            sortingButtons.CreateImageButton(134, 72, "IN STOCK", InventorySettings.SortColumns.InStock);
            sortingButtons.CreateImageButton(202, 168, "CAN PRODUCE", InventorySettings.SortColumns.CanProduce);
            sortingButtons.CreateImageButton(366, 72, "TRACKED", InventorySettings.SortColumns.Tracking);

            
        }

      /*  void ibList_MouseOut(MouseEventArgs args)
        {
            SetView(viewType);
        }*/

        private Grid CreateOuterGrid(bool fixedItemHeight)
        {
            Grid grd = new Grid(Interface.gui, ListBoxType.LCD, Label.LabelType.LCDNormal);
            grd.FixedItemHeights = fixedItemHeight; // false;
            grd.RenderType = RenderType.CRTAndLCD;
            grd.Font = GUIManager.LCDandHUDBodyFontPath; // Important! Must be set after Add! Add will reinitialize with defaults.
            grd.Width = lcdSurface.Width;
            grd.Height = lcdSurface.Height - lcdSurface.Controls[lcdSurface.Controls.Count - 1].Bottom + 15;
            grd.ItemHeight = ItemHeight; //18;
            grd.Position = new Point(0, 195);
            grd.ScrollBarEnabled = true;
            grd.RowSpacing = 1;

            return grd;
        }

        private void ShowRelevantGrid()
        {
            switch (viewType)
            {
                case ViewType.List:
                    lcdSurface.Remove(grdCategoryView);
                    lcdSurface.Add(grdListView);
                    break;

                case ViewType.Categories:
                    lcdSurface.Remove(grdListView);
                    lcdSurface.Add(grdCategoryView);
                    break;
            }
        }

        private void CreateTopPanel()
        {
            filterAndTrackingPanel = new LCDInnerPanel(Interface.gui, lcdSurface.Width, false);
            filterAndTrackingPanel.HorizontalContentPadding = 0;
            filterAndTrackingPanel.VerticalContentPadding = 5;
            filterAndTrackingPanel.ContentHeight = 150;
            lcdSurface.Add(filterAndTrackingPanel.Panel);

            tbExpand = CreateTextButton(Interface.gui, 4, 5, 30, "MORE", Expand_Click);
            tbExpand.ScaleWidthToFitText();
            filterAndTrackingPanel.AddContent(tbExpand);
            tbExpand.X = 4;
            tbExpand.Y = 5;
            tbExpand.ToolTip = collapseFilterTooltip;
            /*
            ibExpand = new ImageButton(Interface.gui);
            filterAndTrackingPanel.AddContent(ibExpand);
            ibExpand.Init(ImageButtonType.LCDExpandWithUpAndDownArrows);
            ibExpand.Click += Expand_Click;
            ibExpand.ToolTip = collapseFilterTooltip;
            ibExpand.X = 4;
            ibExpand.Y = 5;
            ibExpand.Height = 30;
            ibExpand.Width = 30;
            ibExpand.RecalculateIconPosition();
            ibExpand.OrderByTag1 = null;
            Icon icon = ibExpand.Controls[0] as Icon;
            icon.CurrentSkin = 1;
            icon.Visible = true;*/


            RadioGroup rgOrAnd = new RadioGroup(Interface.gui);
            rgOrAnd.Width = 500;
            rgOrAnd.Height = 300;
            rgOrAnd.Position = new Point(0, 0);

            rbOr = CreateRadioButton(60 /*44*/, 5, 50, "OR", "Display only the items that are in AT LEAST ONE of the selected filters", CheckBoxType.LCDRadioBanner, rgOrAnd_Click);//mp i don't mention / tracked types because too long. it's also a filter, right
            rbOr.IsChecked = true;
            rbOr.Tag1 = InventorySettings.AndOr.Or;

            rbAnd = CreateRadioButton(120 /*104*/, 5, 70, "AND", "Display only the items that are in ALL of the selected filters", CheckBoxType.LCDRadioBanner, rgOrAnd_Click);
            rbAnd.IsChecked = false;
            rbAnd.Tag1 = InventorySettings.AndOr.And;

            rgOrAnd.Add(rbOr);
            rgOrAnd.Add(rbAnd);

            filterAndTrackingPanel.AddContent(rgOrAnd);

            columnDividerVertical1 = new Image(Interface.gui);
            filterAndTrackingPanel.AddContent(columnDividerVertical1);
            columnDividerVertical1.SetSkinLocation(SkinState.Normal,Interface.gui.GUISpriteSheet.GetSourceRectangle("basic_line"));
            columnDividerVertical1.X = 204;//188;
            columnDividerVertical1.Y = 6;
            columnDividerVertical1.Width = 2;
            columnDividerVertical1.Height = tbExpand.Bottom - 10;
            columnDividerVertical1.ScaleImageToSizeOfControl = true;


            rgDistance = new RadioGroup(Interface.gui);             
            rgDistance.Height = 30;
            rgDistance.Width = 222;
            rgDistance.NewMemberChecked += rgDistance_NewMemberChecked;
            filterAndTrackingPanel.AddContent(rgDistance);
            rgDistance.Y = 5;
            rgDistance.X = 212; //196;       

            ibNormalDistance = new ImageButton(Interface.gui);
            rgDistance.Add(ibNormalDistance);
            ibNormalDistance.InitWithIcon(ImageButtonType.LCD, "basic_icon_branchShort", true);
            ibNormalDistance.CheckedMode = CheckedModes.CanBeChecked;
           // ibNormalDistance.Click += btCyc_Click;
            ibNormalDistance.Width = 80;
            ibNormalDistance.X = 0;
            ibNormalDistance.Y = 0;
            ibNormalDistance.ToolTip = "'AVAILABLE NOW': Lists only objects that are owned by the colony now or can be made in one step.";
            ibNormalDistance.EventArgs = new DistanceArg() { Availability = InventorySettings.Availability.AvailableNow };

            ibAttainableDistance = new ImageButton(Interface.gui);
            rgDistance.Add(ibAttainableDistance);
            ibAttainableDistance.InitWithIcon(ImageButtonType.LCD, "basic_icon_branchLong", true); //was: "ratings_arrow_horizontal"
            ibAttainableDistance.CheckedMode = CheckedModes.CanBeChecked;
          //  ibAttainableDistance.Click += btCyc_Click;
            ibAttainableDistance.Width = 80;
            ibAttainableDistance.X = ibNormalDistance.Right + 6;
            ibAttainableDistance.Y = ibNormalDistance.Y;
            ibAttainableDistance.ToolTip = "'ATTAINABLE': Also lists objects that can be made in the future, based on the resource types that have been discovered.";
            ibAttainableDistance.EventArgs = new DistanceArg() { Availability = InventorySettings.Availability.Attainable };

            ibCyclopedia = new ImageButton(Interface.gui);
            rgDistance.Add(ibCyclopedia);
            ibCyclopedia.InitWithIcon(ImageButtonType.LCD, "basic_icon_book", true);
            ibCyclopedia.CheckedMode = CheckedModes.CanBeChecked;
          //  ibCyclopedia.Click += btCyc_Click;
            ibCyclopedia.Width = 40;
            ibCyclopedia.X = ibAttainableDistance.Right + 6;
            ibCyclopedia.Y = ibAttainableDistance.Y;
            ibCyclopedia.ToolTip = "'ENCYCLOPEDIA': Shows all known blueprints of every item/process currently stored in the PPU";
            ibCyclopedia.EventArgs = new DistanceArg() { Availability = InventorySettings.Availability.AllKnownBlueprints };

            columnDividerVertical2 = new Image(Interface.gui);
            filterAndTrackingPanel.AddContent(columnDividerVertical2);
            columnDividerVertical2.SetSkinLocation(SkinState.Normal,Interface.gui.GUISpriteSheet.GetSourceRectangle("basic_line"));
            columnDividerVertical2.X = 438;
            columnDividerVertical2.Y = 6;
            columnDividerVertical2.Width = 2;
            columnDividerVertical2.Height = tbExpand.Bottom - 10;
            columnDividerVertical2.ScaleImageToSizeOfControl = true;


            /*
            cbDistance = new ComboBox(Interface.gui, ListBoxType.LCDCombo, false);
            filterAndTrackingPanel.AddContent(cbDistance);
            cbDistance.Init(ComboBoxTypes.LCD);
            cbDistance.Y = 5;
            cbDistance.X = 212; //196; 
            cbDistance.Width = 170; // 185;
            cbDistance.SelectedIndex = 0;
            cbDistance.AddEntry(InventorySettings.Availability.AvailableNow, "-> Available now");
            // cbDistance.AddEntry(InventorySettings.Availability.OneStepAway, "->-> One step away");
            cbDistance.AddEntry(InventorySettings.Availability.Attainable, "->->-> Attainable");
            // cbDistance.AddEntry(InventorySettings.Availability.AllKnownBlueprints, ">>>> All blueprints"); // maybe infinity symbol later
            cbDistance.SelectionChanged += cbAvailable_SelectionChanged;
            cbDistance.ToolTip = "Select availability: \n \n'Available now' lists only objects that are owned by the colony now or can be made in one step. \n \n'Attainable' also lists objects that can be made in the future, based on the resource types that have been discovered."; // \n //was: "Select availability"
            */
            
           
            btIncludeSalvage = new ImageButton(Interface.gui);
            filterAndTrackingPanel.AddContent(btIncludeSalvage);
            btIncludeSalvage.InitWithIcon(ImageButtonType.LCD, "HUD_icon_recycleArrows", true);
            btIncludeSalvage.Click += chIncludeSalvage_Click;
            btIncludeSalvage.Width = 30;
            btIncludeSalvage.X = 450;
            btIncludeSalvage.Y = rgDistance.Y;
            btIncludeSalvage.ToolTip = "Include salvageable materials from items and structures when determining what is ATTAINABLE / NOT ATTAINABLE to produce"; //mp was: Count salvaged materials ........what is attainable to produce
            btIncludeSalvage.SetIconTint(GameData.Instance.GUIConstants.sidePanelTextColor);          

            /*
            chIncludeSalvage = new CheckBox(Interface.gui);
            filterPanel.AddContent(chIncludeSalvage);
            chIncludeSalvage.Init(CheckBoxType.LCD);
            chIncludeSalvage.Click += chIncludeSalvage_Click;
            chIncludeSalvage.X = btCyc.Right + 6;
            chIncludeSalvage.Y = btCyc.Y;
            chIncludeSalvage.Text = "SALVAGE";
            chIncludeSalvage.ToolTip = "Count salvaged materials from current inventory items and structures when determining what is attainable to produce";
            */

            rowDivider1 = new Image(Interface.gui);
            filterAndTrackingPanel.AddContent(rowDivider1);
            rowDivider1.SetSkinLocation(SkinState.Normal,Interface.gui.GUISpriteSheet.GetSourceRectangle("basic_line"));
            rowDivider1.X = 6;
            rowDivider1.Y = tbExpand.Bottom;
            rowDivider1.Width = filterAndTrackingPanel.ContentWidth - 12;
            rowDivider1.Height = 2;
            rowDivider1.ScaleImageToSizeOfControl = true;

            filterPropertiesPanel = new FilterPropertiesPanel(Interface.gui, false); //, filterAndTrackingPanel.ContentWidth);
            filterPropertiesPanel.FiltersChanged += filterPropertiesPanel_FiltersChanged;
            filterAndTrackingPanel.AddContentSetFullWidth(filterPropertiesPanel);
            filterPropertiesPanel.Y = 36;

            /*
            cbFilter = new ComboBox(Interface.gui, ListBoxType.LCDCombo, false);
            filterPanel.AddContent(cbFilter);
            cbFilter.Init(ComboBoxTypes.LCD);
            cbFilter.X = 4;
            cbFilter.Y = ibExpand.Bottom + 6;
            cbFilter.Width = 185;
            PopulateFilterSettingsCombo();
            cbFilter.SelectedIndex = 0;
            cbFilter.SelectionChanged += cbFilter_SelectionChanged;
            cbFilter.ToolTip = "Select filter";
            cbFilter.DebugTag = "cbFilter";
           

            grdHrzFilterContainer = new Grid(Interface.gui, ListBoxType.LCD, Label.LabelType.LCDNormal);
            grdHrzFilterContainer.FixedItemHeights = false;
            grdHrzFilterContainer.RenderType = RenderType.Normal;
            filterPanel.AddContent(grdHrzFilterContainer);
            grdHrzFilterContainer.Font = GUIManager.LCDandHUDFontPath; // Important! Must be set after Add! Add will reinitialize with defaults.
            grdHrzFilterContainer.Width = 310;
            grdHrzFilterContainer.Height = 75;
            grdHrzFilterContainer.Position = new Point(190, ibExpand.Bottom + 5);
            grdHrzFilterContainer.CanHaveFocus = true;
            grdHrzFilterContainer.ScrollBarEnabled = true;
            grdHrzFilterContainer.CanGrowInHeight = false;
            grdHrzFilterContainer.DebugTag = "filterGrid";


            this.hzlFilters = new HorizontalList(Interface.gui);
            hzlFilters.CenterItemsVertically = true;
            grdHrzFilterContainer.AddEntry(hzlFilters, hzlFilters);
            hzlFilters.HorizontalSpacing = 6;
            hzlFilters.Y = 5;
            hzlFilters.MaxWidth = grdHrzFilterContainer.Width - 10;
            hzlFilters.X = 0;

            ibRemoveAllFilters = new ImageButton(Interface.gui);
            filterPanel.AddContent(ibRemoveAllFilters);
            ibRemoveAllFilters.InitWithIcon(ImageButtonType.LCD, "HUD_icon_trash", false);
            ibRemoveAllFilters.Click += tbRemoveAllFilters_Click;
            ibRemoveAllFilters.ToolTip = "Remove all filters";
            ibRemoveAllFilters.X = 158;
            ibRemoveAllFilters.Y = cbFilter.Bottom + 7;
            ibRemoveAllFilters.Height = 30;
            ibRemoveAllFilters.Width = 30;
            ibRemoveAllFilters.Visible = false;
            ibRemoveAllFilters.SetIconTint(GameData.Instance.GUIConstants.sidePanelTextColor);
            ibRemoveAllFilters.RecalculateIconPosition();
            */

            rowDivider2 = new Image(Interface.gui);
            filterAndTrackingPanel.AddContent(rowDivider2);
            rowDivider2.SetSkinLocation(SkinState.Normal,Interface.gui.GUISpriteSheet.GetSourceRectangle("basic_line"));
            rowDivider2.X = 6;
            rowDivider2.Y = filterPropertiesPanel.Bottom; // grdHrzFilterContainer.Bottom;
            rowDivider2.Width = filterAndTrackingPanel.ContentWidth - 12;
            rowDivider2.Height = 2;
            rowDivider2.ScaleImageToSizeOfControl = true;

            cbEntityTracking = new ComboBox(Interface.gui, ListBoxType.LCDCombo, false);
            filterAndTrackingPanel.AddContent(cbEntityTracking);
            cbEntityTracking.Init(ComboBoxTypes.LCD);
            cbEntityTracking.X = 4;
            cbEntityTracking.Y = 120;
            cbEntityTracking.Width = 185;
            PopulateEntityTrackingCombo();
            cbEntityTracking.SelectedIndex = 0;
            cbEntityTracking.SelectionChanged += cbTracking_SelectionChanged;
            cbEntityTracking.ToolTip = "Select a tracked object to view its options";

            The.InGameUI.InventorySettings.TrackTargetsChanged += InventorySettings_TrackTargetsChanged;

            cbInputChain = CreateCheckBox(190, 123, 80, "IN", "Track objects that are NEEDED TO CREATE this object", CheckBoxType.LCDTinting); //objects was: items.  changed it so that it includes structures. maybe also animals and trees?
            cbOutputChain = CreateCheckBox(280, 123, 80, "OUT", "Track objects that can be CREATED FROM this object", CheckBoxType.LCDTinting); //"Track objects that can be created using this object"
            cbToolsOption = CreateCheckBox(370, 123, 80, "TOOLS", "Track TOOLS that can be USED TO CREATE this object", CheckBoxType.LCDTinting);

            ImageButton ibDeleteTracking = new ImageButton(Interface.gui);
            filterAndTrackingPanel.AddContent(ibDeleteTracking);
            ibDeleteTracking.InitWithIcon(ImageButtonType.LCD, "HUD_icon_trash", false);
            ibDeleteTracking.Click += tbStopTracking_Click;
            ibDeleteTracking.ToolTip = "Stop tracking this object";
            ibDeleteTracking.X = 460;
            ibDeleteTracking.Y = 122;
            ibDeleteTracking.Height = 30;
            ibDeleteTracking.Width = 30;
            ibDeleteTracking.SetIconTint(GameData.Instance.GUIConstants.sidePanelTextColor);
            ibDeleteTracking.RecalculateIconPosition();
        }

        class DistanceArg : EventArgs
        {
            public InventorySettings.Availability Availability;
        }

        void rgDistance_NewMemberChecked(ICanBeChecked arg1, EventArgs arg2)
        {
            The.InGameUI.InventorySettings.AvailabilitySettings = ((DistanceArg)((ImageButton)arg1).EventArgs).Availability;
            //The.InGameUI.InventorySettings.AvailabilitySettings = (InventorySettings.Availability)(sender as ComboBox).SelectedKey;

            UpdateAllBlueprintsSetting();

            Populate();
        }

        private void UpdateAllBlueprintsSetting()
        {
            if (ibCyclopedia.IsChecked)
            {
                BackgroundTint = encyclopediaTint;

                btIncludeSalvage.IsChecked = false;
                btIncludeSalvage.Enabled = false;
            }
            else
            {
                BackgroundTint = null;
                btIncludeSalvage.Enabled = true;
            }
        }

        void filterPropertiesPanel_FiltersChanged()
        {
            Refresh();
        }

        void chIncludeSalvage_Click(UIComponent sender, EventArgs e)
        {
            The.InGameUI.InventorySettings.IncludeSalvageProcesses = btIncludeSalvage.IsChecked;

            Populate();
        }

      /*  void btCyc_Click(UIComponent sender, EventArgs e)
        {
            if (ibCyclopedia.IsChecked)
            {
                The.InGameUI.InventorySettings.AvailabilitySettings = InventorySettings.Availability.AllKnownBlueprints;
                BackgroundTint = encyclopediaTint;

                cbDistance.SelectedIndex = -1;
            }
            else
            {
                if (cbDistance.SelectedKey == null)
                {
                    cbDistance.SelectedIndex = 0;
                }

                The.InGameUI.InventorySettings.AvailabilitySettings = (InventorySettings.Availability)cbDistance.SelectedKey;
                BackgroundTint = null;
            }

            FillDistance(The.InGameUI.InventorySettings.AvailabilitySettings);

            Populate();
        }*/



        private RadioButton CreateRadioButton(int x, int y, int width, string text, string tooltip, CheckBoxType checkBoxType, ClickHandler Click)
        {
            RadioButton rb = new RadioButton(Interface.gui);
            rb.Init(checkBoxType);
            rb.Click += Click;
            rb.X = x;
            rb.Y = y;
            rb.Text = text;
            rb.ToolTip = tooltip;
            rb.Width = width;
            return rb;
        }

        private CheckBox CreateCheckBox(int x, int y, int width, string text, string tooltip, CheckBoxType checkBoxType)
        {
            CheckBox rb = new CheckBox(Interface.gui);
            filterAndTrackingPanel.AddContent(rb);
            rb.Init(checkBoxType);
            rb.X = x;
            rb.Y = y;
            rb.Text = text;
            rb.ToolTip = tooltip;
            rb.Width = width;
            rb.Click += chbEntityTracking_Click;
            return rb;
        }

        public static TextButton CreateTextButton(GUIManager gui, int x, int y, int width, string text, ClickHandler tbSwitch_Click)
        {
            TextButton tb = new TextButton(gui);
            tb.Width = width;
            tb.Init(TextButton.TextButtonType.LCD);
            tb.Click += tbSwitch_Click;
            tb.Y = y;
          //  tb.CheckedMode = CheckedModes.CanBeChecked;
            tb.Text = text;
            tb.X = x;
            tb.Width = width;
            tb.Height = 30;

            return tb;
        }


       

       

        public void PopulateEntityTrackingCombo()
        {
            TrackTarget selectedKey = (TrackTarget)cbEntityTracking.SelectedKey;

            cbEntityTracking.Clear();

            foreach (TrackTarget tracked in The.InGameUI.InventorySettings.TrackedTargets.Values)
            {
                if (!cbEntityTracking.EntriesByKey.ContainsKey(tracked))
                {
                    cbEntityTracking.AddEntry(tracked, tracked.EntityType.Name);
                  
                    cbEntityTracking.EntriesByKey[tracked].NormalColor = tracked.Color;
                }
            }

            if (cbEntityTracking.EntriesByKey.Count > 0)
            {
                // reselect:
                if (selectedKey != null
                    && cbEntityTracking.EntriesByKey.ContainsKey(selectedKey))
                {
                    cbEntityTracking.SelectedKey = selectedKey;
                }
                else
                {
                    cbEntityTracking.SelectedIndex = 0;
                }
            }
        }


        private void LoadUserSettings()
        {
            if (GameData.Instance.GUIConstants.EnableFilters)
            {
                rbOr.IsChecked = The.InGameUI.InventorySettings.AndOrSetting == InventorySettings.AndOr.Or;
                rbAnd.IsChecked = The.InGameUI.InventorySettings.AndOrSetting == InventorySettings.AndOr.And;

                filterPropertiesPanel.Fill(The.InGameUI.InventorySettings.FilterPropertySettings);
                
                PopulateEntityTrackingCombo();

                //Selected entity index is set to 0 
                cbEntityTracking.SelectedIndex = 0; // does this fire an event to fill the controls?

                TrackTarget firstTrackedTarget = The.InGameUI.InventorySettings.GetFirstTrackedTarget();

                if (firstTrackedTarget != null)
                {
                    /* var e = The.InGameUI.InventorySettings.TrackedTargets.GetEnumerator();
                     e.MoveNext();
                     var anElement = e.Current;*/

                    FillTrackedEntityTypeControls(firstTrackedTarget.ShowInputs,
                                  firstTrackedTarget.ShowOutputs,
                                  firstTrackedTarget.ShowTools, firstTrackedTarget.Color);
                }

                FillDistance(The.InGameUI.InventorySettings.AvailabilitySettings);

                btIncludeSalvage.IsChecked = The.InGameUI.InventorySettings.IncludeSalvageProcesses;

                //Set the top panel to expanded or minimized  
                haveActiveTracking = cbEntityTracking.Count != 0;

                //The.InGameUI.InventorySettings.SetSettingsAreDirty(true);

                ExpandOrCollapseTopPanel(The.InGameUI.InventorySettings.IsExpanded); 
            }
            else
            {
                ExpandOrCollapseTopPanel(false); 
            }

            sortingButtons.Fill(The.InGameUI.InventorySettings.SortingSettings); //(int)The.InGameUI.InventorySettings.SortingSettings.SortedBy, The.InGameUI.InventorySettings.SortOrder);
            
            /*
            switch (The.InGameUI.InventorySettings.SortedBy)
            {
                case InventorySettings.SortColumns.Name:
                    tbSortName.IsChecked = true;
                    SetSortOrderIcons(tbSortName, sortingButtons, The.InGameUI.InventorySettings.SortOrder);
                    break;

                case InventorySettings.SortColumns.InStock:
                    tbSortInStock.IsChecked = true;
                    SetSortOrderIcons(tbSortInStock, sortingButtons, The.InGameUI.InventorySettings.SortOrder);
                    break;

                case InventorySettings.SortColumns.CanProduce:
                    tbSortCanProduce.IsChecked = true;
                    SetSortOrderIcons(tbSortCanProduce, sortingButtons, The.InGameUI.InventorySettings.SortOrder);
                    break;

                case InventorySettings.SortColumns.Tracking:
                    tbSortTracked.IsChecked = true;
                    SetSortOrderIcons(tbSortTracked, sortingButtons, The.InGameUI.InventorySettings.SortOrder);
                    break;
            }*/
        }

        private void FillDistance(InventorySettings.Availability distance)
        {
            switch(distance)
            {
                case InventorySettings.Availability.AvailableNow:
                    rgDistance.SelectMember(ibNormalDistance);
                    break;

                case InventorySettings.Availability.Attainable:
                    rgDistance.SelectMember(ibAttainableDistance);
                    break;

                case InventorySettings.Availability.AllKnownBlueprints:
                    rgDistance.SelectMember(ibCyclopedia);                    
                    break;

            }

            UpdateAllBlueprintsSetting();

            /*
            if (distance != InventorySettings.Availability.AllKnownBlueprints)
            {
                ibCyclopedia.IsChecked = false;

                // don't want the event firing when setting:
                cbDistance.SelectionChanged -= cbAvailable_SelectionChanged;
                cbDistance.SelectedKey = distance;
                cbDistance.SelectionChanged += cbAvailable_SelectionChanged;

                cbDistance.Enabled = true;
            }
            else
            {
                ibCyclopedia.IsChecked = true;

                cbDistance.Enabled = false;
            }*/

        }

      /*  private void cbAvailable_SelectionChanged(UIComponent sender)
        {
            The.InGameUI.InventorySettings.AvailabilitySettings = (InventorySettings.Availability)(sender as ComboBox).SelectedKey;


            Populate();
       
        }*/

     /*   private void cbFilter_SelectionChanged(UIComponent sender)
        {
            ComboBox cb = (ComboBox)sender;

            if (!cb.SelectedKey.Equals(addFilterPromptKey))
            {
                FilterSetting filter = (FilterSetting)cb.SelectedKey;

                if (The.InGameUI.InventorySettings.AddFilterSetting(filter))
                {
                    AddFilterSettingToHzlList(filter);
                }

                cb.SelectedIndex = 0;

                hzlFilters.Sort(Grid.Sorting.Ascending, true);
                hzlFilters.RefreshEntries();

                Refresh();
            }
        }*/



        private void cbTracking_SelectionChanged(UIComponent sender)
        {
            TrackTarget selectedItem = (TrackTarget)cbEntityTracking.SelectedKey;

            FillTrackedEntityTypeControls(selectedItem.ShowInputs,
                          selectedItem.ShowOutputs,
                          selectedItem.ShowTools, selectedItem.Color);

           // TextButton tb = cbEntityTracking.Controls[0] as TextButton;
           // tb.Color = selectedItem.Color;
        }

        /*
        private void AddFilterSettingToHzlList(FilterSetting filter) // object key, string displayName)
        {
            string text = filter.GetDisplayString();
            TextButton btFilter = CreateTextButton(190, 5, 0, text + "  X ", filter_Click);
            btFilter.ScaleToFitText();
            btFilter.Height = 30;
            btFilter.ID = UIComponent.DataControlID.Filter;
            btFilter.Tag1 = filter; // "Filter tag used: " + filter.FilterSettingType.KeyName; // ???
            btFilter.ToolTip = "Click to remove"; // text;
            btFilter.OrderByTag1 = (float)btFilter.Width;
           
            hzlFilters.AddEntry(filter, btFilter);
        }*/

        private void FillTrackedEntityTypeControls(bool showinputs, bool showoutputs, bool showtools, Color backColor)
        {
            cbInputChain.IsChecked = showinputs;
            cbOutputChain.IsChecked = showoutputs;
            cbToolsOption.IsChecked = showtools;

            cbInputChain.BackColor = backColor;
            cbOutputChain.BackColor = backColor;
            cbToolsOption.BackColor = backColor;
        }



        private void RefreshTopPanelAfterTrackingChange()
        {
            haveActiveTracking = cbEntityTracking.Count != 0;
            ExpandOrCollapseTopPanel(The.InGameUI.InventorySettings.IsExpanded);

            // The.InGameUI.InventorySettings.SetSettingsAreDirty(true);

            Refresh();
        }

        private void HideOrDisplayTrackingOptions()
        {
            if (haveActiveTracking && cbEntityTracking.Count == 0)
            {
                haveActiveTracking = false;
                UpdateGridYPosition(topPanelExpandedNoTrackingHeight);

                rowDivider2.Visible = false;
            }
            if (haveActiveTracking == false && cbEntityTracking.Count > 0)
            {
                haveActiveTracking = true;
                UpdateGridYPosition(topPanelExpandedWithTrackingHeight);

                rowDivider2.Visible = true;
            }

            ExpandOrCollapseTopPanel(!The.InGameUI.InventorySettings.IsExpanded);
        }

        private void UpdateGridYPosition(int filterPanelHeight)
        {
            int sortingYPos;
            if (filterAndTrackingPanel != null)
            {
                filterAndTrackingPanel.ContentHeight = filterPanelHeight;//97
                sortingYPos = filterAndTrackingPanel.Panel.Bottom + 2;
            }
            else
            {
                sortingYPos = 6;
            }

            sortingButtonsContainer.Y = sortingYPos; // lcdSurface.Controls[1].Bottom + 2;
            grdListView.Y = sortingButtonsContainer.Bottom - 15;
            grdCategoryView.Y = sortingButtonsContainer.Bottom - 15;


            grdListView.Height = lcdSurface.Height - sortingButtonsContainer.Bottom + 15;
            grdCategoryView.Height = lcdSurface.Height - sortingButtonsContainer.Bottom + 15;
        }


     
        private void ExpandOrCollapseTopPanel(bool expand)
        {
            if (expand)
            {
                if (haveActiveTracking)
                {
                    SetMinimizedOrExpandedContentProperties(topPanelExpandedWithTrackingHeight, 1, true, "LESS", collapseFilterTooltip);
                }
                else
                {
                    SetMinimizedOrExpandedContentProperties(topPanelExpandedNoTrackingHeight, 1, true, "LESS", collapseFilterTooltip);
                }                
            }
            else
            {
                SetMinimizedOrExpandedContentProperties(topPanelCollapsedHeight, 0, false, "MORE", expandFilterTooltip);

            }

            The.InGameUI.InventorySettings.IsExpanded = expand;
        }

        private void SetMinimizedOrExpandedContentProperties(int panelheight, int currentSkin, bool visible, string text, string btToolTip)
        {
            UpdateGridYPosition(panelheight);

            if (GameData.Instance.GUIConstants.EnableFilters)
            {
                tbExpand.ToolTip = btToolTip;
                tbExpand.Text = text;
                /* Icon icon = ibExpand.Controls[0] as Icon;
                 icon.CurrentSkin = currentSkin;*/

                // show hide the controls on the part of the panel that is always visible:
                // it would have been better to add them to a panel control
                rowDivider1.Visible = visible;
                rbAnd.Visible = visible;
                rbOr.Visible = visible;
             //   filterPropertiesPanel.Visible = visible; // this sets all child controls visible too, which we don't want
                //cbDistance.Visible = visible;
                rgDistance.Visible = visible;
                columnDividerVertical1.Visible = visible;
                //ibCyclopedia.Visible = visible;
                btIncludeSalvage.Visible = visible;

                if (visible)
                {
                    filterAndTrackingPanel.AddContentSetFullWidth(filterPropertiesPanel);
          
                   // filterPropertiesPanel.Show(); // need to reset child control visibility
                }
                else
                {
                    filterAndTrackingPanel.RemoveContent(filterPropertiesPanel);
                }
            }
        }



        #region ClickEvent Handlers

     

        private void Expand_Click(UIComponent sender, EventArgs e)
        {
            ImageButton btExpand = sender as ImageButton;

            ExpandOrCollapseTopPanel(!The.InGameUI.InventorySettings.IsExpanded);

        }

        private void rgOrAnd_Click(UIComponent sender, EventArgs e)
        {
            RadioButton rb = (RadioButton)sender;
            if (The.InGameUI.InventorySettings.AndOrSetting != (InventorySettings.AndOr)rb.Tag1)
            {
                The.InGameUI.InventorySettings.AndOrSetting = (InventorySettings.AndOr)rb.Tag1;


                // The.InGameUI.InventorySettings.SetSettingsAreDirty(true);

                Refresh();
            }
        }

        /*
        private void filter_Click(UIComponent sender, EventArgs e)
        {
            //Removes the clicked filter from the active Filters/Categories            

            // string deleteFilter = ((TextButton)sender).Tag1.ToString();
            FilterSetting filter = (FilterSetting)(((TextButton)sender).Tag1);

            hzlFilters.RemoveEntry(filter);

            The.InGameUI.InventorySettings.RemoveFilterSetting(filter); //.ActiveFilterSettings.Remove(deleteFilter);


            hzlFilters.Sort(Grid.Sorting.Ascending, true);
            hzlFilters.RefreshEntries();

            Refresh();
        }*/

        private void chbEntityTracking_Click(UIComponent sender, EventArgs e)
        {
            CheckBox cb = (CheckBox)sender;

            TrackTarget selectedItem = (TrackTarget)cbEntityTracking.SelectedKey;

            if (selectedItem != null)
            {
                selectedItem.SetTrackingOptions(cbInputChain.IsChecked, cbOutputChain.IsChecked, cbToolsOption.IsChecked);
            }

            //Refresh();
        }

        public void btTrack_Click(UIComponent sender, EventArgs e)
        {
            ImageButton bt = (ImageButton)sender;
            EntityType entityType = (EntityType)bt.Parent.Parent.Tag1; // (EntityType)bt.Parent.Tag1;

            bool isTrackedNow = The.InGameUI.InventorySettings.ToggleTracking(entityType);

            UIComponent hotspot = bt.Parent;
            if (isTrackedNow)
            {
               // hotspot.Visible = false; 
            }
            else
            {
              //  hotspot.Visible = true;
            }
      
        }



        private void tbStopTracking_Click(UIComponent sender, EventArgs e)
        {
            TrackTarget trackedEntityType = (TrackTarget)cbEntityTracking.SelectedKey;


            bool isTrackedNow = The.InGameUI.InventorySettings.ToggleTracking(trackedEntityType.EntityType);

            if (!isTrackedNow) 
            { 
                // check both grids, if the item row exists, hide the tracking button
                ActOnRow(trackedEntityType.EntityType, HideTrackingButton);

                /*               
                UIComponent row;
                if (grdListView.TryGetEntry(trackedEntityType.EntityType, out row))
                {
                    HideTrackingButton(row);
                }

                object key = GetCategoryKey(trackedEntityType.EntityType);

                UIComponent categoryRow;
                if (grdCategoryView.TryGetEntry(key, out categoryRow))
                {
                    CollapsablePanel cpCategory = categoryRow as CollapsablePanel;
                    Grid grdCategory = cpCategory.ExpandedPanel.Controls[0] as Grid;

                    if (grdCategory.TryGetEntry(trackedEntityType.EntityType, out row))
                    {
                        HideTrackingButton(row);
                    }
                }*/
            }
        }


        private void ActOnRow(EntityType entityType, Action<UIComponent> action)
        {
            UIComponent row;
            if (grdListView.TryGetEntry(entityType, out row))
            {
                action(row);
            }

            object key = GetCategoryKey(entityType);

            UIComponent categoryRow;
            if (grdCategoryView.TryGetEntry(key, out categoryRow))
            {
                CollapsablePanel cpCategory = categoryRow as CollapsablePanel;
                Grid grdCategory = cpCategory.ExpandedPanel.Controls[0] as Grid;

                if (grdCategory.TryGetEntry(entityType, out row))
                {
                    action(row);
                }
            }
        }

        private static object GetCategoryKey(EntityType entityType)
        {
            object key;

            key = entityType.Category;

            /*
            if (entityType.ItemType != null)
            {
                key = entityType.Category;
            }
            else
            {
                key = entityType.StructureType.Category;
            }*/

            return key;
        }

        private static void HideTrackingButton(UIComponent row)
        {
            ImageButton ibTrack = (ImageButton)(row.FindChildById(UIComponent.DataControlID.Track).Controls[0]);
            ibTrack.Visible = false;
        }

        private void SetView(ViewType viewTypeToSet)
        {
            viewType = viewTypeToSet;

            ShowRelevantGrid();

            if (viewType == ViewType.List)
            {
                ibCategory.IsChecked = false;
                ibCategory.IsChecked = false;

               // ibCategory.CurrentSkinState = SkinState.Normal;               
            }
            else
            {
                ibCategory.IsChecked = true;             
                ibList.IsChecked = false;
               // ibList.CurrentSkinState = SkinState.Normal;
            }
        }

        private void tbListView_Click(UIComponent sender, EventArgs e)
        {
            if (viewType != ViewType.List)
            {
                SetView(ViewType.List);
            
                Populate();
            }
        }

        private void tbCategoryView_Click(UIComponent sender, EventArgs e)
        {
            if (viewType != ViewType.Categories)
            {
                SetView(ViewType.Categories);
            
                Populate();
            }
        }

        private void tbSort_Click() 
        {           
            Populate();
        }

        #endregion


      
        /// <summary>
        /// collapsable panels instead of grids??
        /// </summary>
        /// <param name="outerGrid"></param>
        /// <param name="categoryGrids"></param>
        /// <param name="itemSortOrder"></param>
        public static void DoCategorySorting(Grid outerGrid, List<Grid> categoryGrids, Grid.Sorting itemSortOrder)
        {
            //sort categories
          
            outerGrid.Sort(i => i.OrderByTag1, Grid.Sorting.Ascending); // itemSortOrder); // );           

            // sort item grids:   
            foreach (var grid in categoryGrids)
            {
                if (grid.Entries.Count > 0)
                {
                    grid.Sort(i => i.OrderByTag1, itemSortOrder);
                }
                grid.EndAddingEntries();
            }
        }

        


        public override void Refresh()
        {          
            Populate();
            
            base.Refresh();
        }

        /* this belongs in AddRow / UpdateRow, if needed
        private void MatchStockButtonWidths()
        {
            int size = 0;
            foreach (var row in grdCategoryView.Entries)
            {
                UIComponent itemComponent = row.FindChildById(UIComponent.DataControlID.Stock);
                if (itemComponent != null)
                {
                    TextButton tbStock = (TextButton)itemComponent;
                    if (tbStock.Width > size) size = tbStock.Width;
                }
            }
            foreach (var row in grdCategoryView.Entries)
            {
                UIComponent itemComponent = row.FindChildById(UIComponent.DataControlID.Stock);
                if (itemComponent != null)
                {
                    TextButton tbStock = (TextButton)itemComponent;
                    if (tbStock.Width > size) size = tbStock.Width;
                }
            }
        }*/

      
        public override void Show()
        {           
            //isFirstUpdate = true;

            base.Show(); // this calls Refresh
        }

        void InventorySettings_TrackTargetsChanged()
        {
            PopulateEntityTrackingCombo();

            RefreshTopPanelAfterTrackingChange();

            Refresh();
        }

        #region GetNoOfAvailableEntities

        public static int GetNoOfAvailableEntities(Dictionary<EntityType, List<EntityID>> allEntities,
                                                  EntityGroup owner,
                                                  EntityType entityType,
                                                  out int noOfIncompleteEntities,
                                                  out int noOfEntitiesUsedAsParts,
                                                  out int noOfItemsOnOtherSite,
                                                  out int noOfItemsOwnedByOthers,
                                                  out int noOfAvailableItemsIncludingIntrinsic,
                                                  Dictionary<EntityType, Availability> allAvailableEntities = null,
                                                  bool countPartsOfEntities = false,
                                                  bool ownedByOtherAllegiance = false,
                                                  bool doCacheLookup = true/*,
                                                  List<JobID> excludeAssignedToJob = null*/)
        {
            List<EntityID> listOfEntities;
            List<EntityID> listOfAvailableEntities, listOfUnavailableEntities;

            return GetNoOfAvailableEntities(allEntities,
                                                  owner, // optional
                                                  entityType,
                                                  out noOfIncompleteEntities,
                                                  out noOfEntitiesUsedAsParts,
                                                  out noOfItemsOnOtherSite,
                                                  out noOfItemsOwnedByOthers,
                                                  out noOfAvailableItemsIncludingIntrinsic,
                                                  out listOfEntities,
                                                  out listOfAvailableEntities, out listOfUnavailableEntities,
                                                  allAvailableEntities,
                                                  countPartsOfEntities,
                                                  ownedByOtherAllegiance,
                                                  doCacheLookup
                                                  /*,
                                                  excludeAssignedToJob*/);
        }

        public class Availability
        {           
            /// <summary>
            /// counts intrinsic tools which are parts as unavailable
            /// </summary>
            public int NoOfAvailableItems;

            /// <summary>
            /// NEW: counts intrinsic tools which are parts as available
            /// </summary>
            public int NoOfAvailableItemsIncludingIntrinsic;

            public int NoOfIncompleteEntities;
            public int NoOfEntitiesUsedAsParts;
            public int NoOfEntitiesOffSite; 
            public int NoOfEntitiesOwnedByOthers; 

            public int AvailableToBuy; 

            /// <summary>
            /// all entities - displayed in side panel
            /// </summary>
            public List<EntityID> AllEntities;

            public List<EntityID> AvailableEntities;

            public List<EntityID> UnavailableEntities;

            public Availability()
            {

            }

           /* public Availability(int noOfAvailableItems, int noOfIncompleteEntities, int noOfEntitiesUsedAsParts, int availableToBuy, List<EntityID> listOfEntities)
            {
                NoOfAvailableItems = noOfAvailableItems;            
                NoOfIncompleteEntities = noOfIncompleteEntities;            
                NoOfEntitiesUsedAsParts = noOfEntitiesUsedAsParts;
                AvailableToBuy = availableToBuy;
                ListOfEntities = listOfEntities;
            }*/
        }

        /// <summary>
        /// Returns 0 if owner/group is null.
        /// 
        /// we have to loop over all relevant items and determine if they are currently used as parts, in which case we do not want to show them in the 
        /// inventory
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public static int GetNoOfAvailableEntities(Dictionary<EntityType, List<EntityID>> allEntities,
                                                   EntityGroup owner,
                                                   EntityType entityType,
                                                   out int noOfIncompleteEntities,
                                                   out int noOfEntitiesUsedAsParts,
                                                   out int noOfItemsOnOtherSite,
                                                   out int noOfItemsOwnedByOthers,
                                                   out int noOfAvailableItemsIncludingIntrinsic,
                                                   out List<EntityID> listOfAllEntities,
                                                   out List<EntityID> listOfAvailableEntities,
                                                   out List<EntityID> listOfUnavailableEntities,
                                                   Dictionary<EntityType, Availability> allAvailableEntities = null,
                                                   bool countPartsOfEntities = false,
                                                   bool ownedByOtherAllegiance = false,
                                                   bool doCacheLookup = true)
        {
            noOfIncompleteEntities = 0;
            noOfEntitiesUsedAsParts = 0;
            noOfItemsOnOtherSite = 0;
            noOfItemsOwnedByOthers = 0;
            noOfAvailableItemsIncludingIntrinsic = 0;
            listOfAllEntities = null;
            listOfAvailableEntities = null;
            listOfUnavailableEntities = null;

            OwnerID? ownerID = owner.GetOwnerID();            

            if (allEntities == null) // owner == null)
                return 0;

         
            int noOfAvailableEntities = 0;
            noOfAvailableItemsIncludingIntrinsic = 0;

            // look up in cache first 
            if (doCacheLookup && allAvailableEntities != null)
            {
                Availability availability;
                if (allAvailableEntities.TryGetValue(entityType, out availability))
                {
                    noOfEntitiesUsedAsParts = availability.NoOfEntitiesUsedAsParts;
                    noOfIncompleteEntities = availability.NoOfIncompleteEntities;
                    noOfItemsOnOtherSite = availability.NoOfEntitiesOffSite;
                    noOfItemsOwnedByOthers = availability.NoOfEntitiesOwnedByOthers;

                    noOfAvailableItemsIncludingIntrinsic = availability.NoOfAvailableItemsIncludingIntrinsic;

                    listOfAllEntities = availability.AllEntities;
                    listOfAvailableEntities = availability.AvailableEntities;
                    listOfUnavailableEntities = availability.UnavailableEntities;

                    return availability.NoOfAvailableItems;
                }
            }


            if (!allEntities.TryGetValue(entityType, out listOfAllEntities))
            {
                noOfAvailableEntities = 0;
            }
            else
            {
                listOfAvailableEntities = new List<EntityID>();
                listOfUnavailableEntities = new List<EntityID>();

                EntityID entityID;
                IKnownEntityData itemData;
                SharedKnowledge sharedKnowledge = The.InGameUI.UIAllegiance.SharedKnowledge;
                for (int i = listOfAllEntities.Count - 1; i >= 0; i--)
                {
                    entityID = listOfAllEntities[i];

                    if (GoalEvaluator.HandleOwnerDataResult(sharedKnowledge, entityID, owner, out itemData))
                    {
                        EntityGroup.CountEntity(ownerID, itemData, ref noOfIncompleteEntities, ref noOfEntitiesUsedAsParts, ref noOfItemsOnOtherSite, ref noOfItemsOwnedByOthers,
                            ref noOfAvailableEntities, ref noOfAvailableItemsIncludingIntrinsic,
                            ref listOfAvailableEntities, ref listOfUnavailableEntities);

                    }
                }
            }


            if (allAvailableEntities != null)
            {
                Availability availability;
                if (!allAvailableEntities.TryGetValue(entityType, out availability))
                {
                    availability = new Availability(); 
                    allAvailableEntities[entityType] = availability;  // cache the result               
                }

                // add to existing sums:
                if (ownedByOtherAllegiance)
                {
                    // used in Attainability calculations:
                    availability.AvailableToBuy += noOfAvailableEntities;
                }
                else
                {                             
                    availability.NoOfAvailableItems += noOfAvailableEntities;

                    availability.NoOfAvailableItemsIncludingIntrinsic += noOfAvailableItemsIncludingIntrinsic;

                    availability.NoOfIncompleteEntities += noOfIncompleteEntities;
                    availability.NoOfEntitiesUsedAsParts += noOfEntitiesUsedAsParts;
                    availability.NoOfEntitiesOwnedByOthers += noOfItemsOwnedByOthers;
                    availability.NoOfEntitiesOffSite += noOfItemsOnOtherSite;
                }      
         
                if (listOfAllEntities != null)
                {
                    if (availability.AllEntities == null)
                    {
                        availability.AllEntities = new List<EntityID>();
                    }

                    availability.AllEntities.AddRange(listOfAllEntities);
                }

                if (listOfAvailableEntities != null)
                {
                    if (availability.AvailableEntities == null)
                    {
                        availability.AvailableEntities = new List<EntityID>();
                    }

                    availability.AvailableEntities.AddRange(listOfAvailableEntities);
                }

                if (listOfUnavailableEntities != null)
                {
                    if (availability.UnavailableEntities == null)
                    {
                        availability.UnavailableEntities = new List<EntityID>();
                    }

                    availability.UnavailableEntities.AddRange(listOfUnavailableEntities);
                }                
            }

            return noOfAvailableEntities;
        }

       

        #endregion

        private void Populate()
        {
            
            Expedition expedition = The.Sim.PlaySite.GetFirstPlayerExpedition();
            if (expedition == null)
                return; // clear all? or does it only occur on startup...

            EntityGroup owner = expedition.OwnedEntities;
           

            // clear caches before repopulating:            
            CountAllEntities(owner, allAvailableItems, false);
           
            // attainability runs timesliced, with its own update frequency.

            switch (viewType)
            {
                case ViewType.Categories:
                    PopulateWithCategories(owner);
                    if (grdCategoryView.Count > 0 && grdListView.Count == 0)
                    {
                        PopulateWithList(owner);
                    }
                    break;
                case ViewType.List:
                    PopulateWithList(owner);
                    if (grdListView.Count > 0 && grdCategoryView.Count == 0)
                    {
                        PopulateWithCategories(owner);
                    }
                    break;
            }

            //isFirstUpdate = false;
        }

        private void PopulateWithCategories(EntityGroup owner)
        {
           
            CollapsablePanel cpCategory;
            Grid categoryGrid = null;
            UIComponent categoryRow;
            UIComponent itemRow;

            categoryGrids = new List<Grid>();
          
            object key;

            currentListData = The.InGameUI.InventorySettings.GetData(allAvailableItems);
            
            
            // Nested grids

            // outer level is an item "category"
            // add nested grids for each, containing item types...


            grdCategoryView.BeginAddingEntries();

            foreach (var entityType in currentListData)//.AllItemTypes) //  only items for now...
            {
                if (entityType.StructureType != null || entityType.ItemType != null)
                {
                    key = GetCategoryKey(entityType); // #TRACKFIX

                    cpCategory = null;

                    if (grdCategoryView.TryGetEntry(key, out categoryRow))
                    {
                        cpCategory = categoryRow as CollapsablePanel;
                        categoryGrid = (Grid)cpCategory.ExpandedPanel.Controls[0];

                        if (!categoryGrids.Contains(categoryGrid))
                        {
                            categoryGrid.BeginAddingEntries();
                            categoryGrids.Add(categoryGrid); // for resize at the end of update
                        }
                    }

                    if (cpCategory == null) // add the category
                    {
                        AddCategoryRow(ref cpCategory, ref categoryGrid, key);

                        categoryGrid.BeginAddingEntries();
                        categoryGrids.Add(categoryGrid); // for resize at the end of update
                    }

                    if (!categoryGrid.TryGetEntry(entityType, out itemRow))
                    {
                        itemRow = AddItemRow(categoryGrid, entityType, owner, false, false);
                    }

                    UpdateItemRow(itemRow, entityType, owner);
                }
            }


            // why not use LCDPanel.Cleanup instead..?

            // remove unused item   
            foreach (CollapsablePanel colPanel in grdCategoryView.Entries)
            {
                Grid grd = (Grid)colPanel.ExpandedPanel.Controls[0];
                grd.DeleteEntries<EntityType>(j => currentListData.Contains(j));
            }

            //remove empty categories
            List<object> keysToRemove = new List<object>();
            foreach (var keys in grdCategoryView.EntriesByKey.Keys)
            {
                Grid grd = (Grid)(grdCategoryView.EntriesByKey[keys] as CollapsablePanel).ExpandedPanel.Controls[0];

                if (grd.Entries.Count == 0)
                {
                    keysToRemove.Add(keys);
                }
            }
            foreach (var deleteKey in keysToRemove)
            {
                grdCategoryView.RemoveEntry(deleteKey);
            }

            //sort categories
            DoCategorySorting(grdCategoryView, categoryGrids, The.InGameUI.InventorySettings.SortingSettings.SortOrder);

            grdCategoryView.EndAddingEntries();
        }

        private void PopulateWithList(EntityGroup owner)
        {
           /* Expedition expedition = The.Sim.PlaySite.GetFirstPlayerExpedition();
            if (expedition == null)
                return; // clear all? or does it only occur on startup...
          
            EntityGroup owner = expedition.OwnedEntities;
           */
  
            UIComponent itemRow;

            currentListData = The.InGameUI.InventorySettings.GetData(allAvailableItems);

            grdListView.BeginAddingEntries();

            foreach (var entityType in currentListData)
            {
                if (entityType.Name.Contains("Bellows"))
                {
                }
                if (!grdListView.TryGetEntry(entityType, out itemRow))
                {
                    itemRow = AddItemRow(grdListView, entityType, owner, false, true);
                }

                UpdateItemRow(itemRow, entityType, owner);
            }

            // remove unused rows           
            grdListView.DeleteEntries<EntityType>(j => currentListData.Contains(j));

            grdListView.Sort(i => i.OrderByTag1, The.InGameUI.InventorySettings.SortingSettings.SortOrder);

            grdListView.EndAddingEntries();
        }

        public static void CountAllEntities(EntityGroup owner, Dictionary<EntityType, Availability> allAvailableItems, bool countItemsToBuy)
        {
            // this is needed to compute attainability...
            allAvailableItems.Clear();

          
            // gather items available for trade here, loop over all allegiances that we can trade with
            // they can form the basis for attainability

            if (countItemsToBuy)
            {
                List<AllegianceRelation> relations;
                if (The.Sim.World.Relations.TryGetValue(The.InGameUI.UIAllegiance.ID, out relations))
                {
                    foreach (var item in relations)
                    {
                        if (item.AllowTrade)
                        {
                            Allegiance otherAllegiance = item.AllegianceB; // LookUp<Allegiance, AllegianceID>.FindByID(item.AllegianceB)

                            foreach (var expedition in otherAllegiance.Expeditions)
                            {
                                var terminals = expedition.GetWorkingTerminals(The.InGameUI.UIAllegiance.SharedKnowledge, null);
                                foreach (var terminal in terminals)
                                {
                                    // don't look up in cache...
                                    CountAllEntities(terminal.OfferedEntitiesByType, null, allAvailableItems, true);                                    
                                   // CountAllEntities(expedition.OwnedEntities.TradeManager.AllEntitiesAvailableForTrade, null, allAvailableItems, true);                                    
                                }                                
                            }
                        }
                    }
                }
            }

            // don't look up in cache...
            CountAllEntities(owner.AllEntities, owner, allAvailableItems, false);            

        }

        private static void CountAllEntities(Dictionary<EntityType, List<EntityID>> itemsToCount,  EntityGroup owner, Dictionary<EntityType, Availability> allAvailableItems, bool isOwnedByOtherAllegiance)
        {            
            foreach (var item in itemsToCount)
            {               
                EntityType entityType = item.Key;
                CountItems(itemsToCount, owner, allAvailableItems, isOwnedByOtherAllegiance, entityType);
            }
        }

        public static void CountItems(Dictionary<EntityType, List<EntityID>> itemsToCount, EntityGroup owner, Dictionary<EntityType, Availability> allAvailableItems, bool isOwnedByOtherAllegiance, EntityType entityType)
        {

            int noOfIncompleteEntities;
            int noOfEntitiesUsedAsParts;
            int noOfItemsOffSite;
            int noOfItemsOwnedByOthers;
            int noOfAvailableItemsIncludingIntrinsic;

            GetNoOfAvailableEntities(itemsToCount, owner, entityType, out noOfIncompleteEntities, out noOfEntitiesUsedAsParts, out noOfItemsOffSite, out noOfItemsOwnedByOthers, out noOfAvailableItemsIncludingIntrinsic,
                allAvailableItems, ownedByOtherAllegiance: isOwnedByOtherAllegiance, doCacheLookup: false);

        }

        private void UpdateItemRow(UIComponent itemRow, EntityType entityType, EntityGroup owner)
        {           
                       
            if (GameData.Instance.GUIConstants.EnableFilters)
            {
                UpdateItemRowTracking(itemRow, entityType);
            }

            ProductionOrderControl productionOrderControl = (ProductionOrderControl)itemRow.FindChildById(UIComponent.DataControlID.Orders);

            int noOfAvailableItems;
            productionOrderControl.UpdateOrders(owner, allAvailableItems, out noOfAvailableItems);


            // update production status:
            DataTypeButton tbCaption = (DataTypeButton)itemRow.FindChildById(UIComponent.DataControlID.Caption);
            tbCaption.SetAvailableStatusColor(noOfAvailableItems > 0);
        }

        Dictionary<EntityType, bool> standingOrderButtonWasChecked = new Dictionary<EntityType, bool>();


      /*  private void UpdateItemRowNormalProduction(UIComponent itemRow, Dictionary<ProcessType, AttainableInfo> attainableInfo, EntityType entityType, ProcessType processType,
            bool hasTools, bool hasInputs, bool hasSkills, bool hasResources, int productionLimit, int? outputBatchAmount, bool canOrderFromInventory)
        {

            // first update (Fill):
            // set the check button state according to orders

            // later updates:
            // set fillable bar according to check button


            // first see if button was checked/unchecked this session:
            // standing order mode:
            // has checked standing order in this session 
            


            bool canProduceNow = productionLimit > 0;

            if (entityType.KeyName.Contains("ironArrow"))
            {

            }

           // UIComponent hotspot = null;
            ImageButton btStandingOrder = null;
            if (GameData.Instance.GUIConstants.EnableStandingOrders)
            {
              
                btStandingOrder = (ImageButton)itemRow.FindChildById(UIComponent.DataControlID.StandingOrderModePadlock);
            }
          
            UWGame.SimSide.Expeditions.ProductionOrder stockTarget;
            if (expedition != null)
            {

                bool showWarning = false;
                          
                FillableBar fillableBar = itemRow.FindChildById(UIComponent.DataControlID.CurrentOrders) as FillableBar;

                if (canOrderFromInventory)
                {
                    if (GameData.Instance.GUIConstants.EnableStandingOrders)
                    {
                        btStandingOrder.Visible = true;

                        // make sure the hotspot is visible, but setting the state changes the padlock child button too.
                   
                        if (expedition.OwnedEntities.ProductionOrders.Orders.TryGetValue(entityType, out stockTarget)) // all Items have an entry.
                        {
                            if (isFirstUpdate)
                            {
                                SetPadlockButtonState(stockTarget.AmountToKeepInStore.HasValue, btStandingOrder);
                            }


                            if (btStandingOrder.IsChecked)
                            {
                                UpdateStandingOrderProduction(productionLimit, outputBatchAmount, canProduceNow, stockTarget, fillableBar, btStandingOrder);

                            }
                            else
                            {
                                UpdateDirectProduction(productionLimit, outputBatchAmount, canProduceNow, stockTarget, ref showWarning, fillableBar, btStandingOrder);
                            }

                        }
                        else
                        {
                            fillableBar.Visible = false; // process is gather or other which cannot yet be given from the inventory panel

                            btStandingOrder.Visible = false;
                        }
                    }
                    else
                    {
                        // tutorial etc.
                        //btStandingOrder.Visible = false; 

                        if (expedition.OwnedEntities.ProductionOrders.Orders.TryGetValue(entityType, out stockTarget)) // all Items have an entry.
                        {
                            UpdateDirectProduction(productionLimit, outputBatchAmount, canProduceNow, stockTarget, ref showWarning, fillableBar, null);
                        }
                    }
                }
                else
                {
                    fillableBar.Visible = false; // process is gather or other which cannot yet be given from the inventory panel

                    if (btStandingOrder != null)
                    {
                        btStandingOrder.Visible = false;
                    }
                }

                HorizontalList hzNotAttainable = itemRow.FindChildById(UIComponent.DataControlID.NotAttainableIcons) as HorizontalList;
                HorizontalList hzAttainable = itemRow.FindChildById(UIComponent.DataControlID.AttainableIcons) as HorizontalList;

                if (fillableBar.Visible == false)
                {
                    // the slider control is hidden for salvage, gather and special actions

                    if (entityType.KeyName.Contains("item:firewood"))
                    {

                    }
                
                    if (canOrderFromInventory == false && canProduceNow == true)
                    {
                        // salvage, gather, special action. processes that are initiated outside the inventory.
                        // give information about giving orders here
                        UpdateNonInventoryOrders(processType, hzAttainable, hzNotAttainable);                           
                    }
                    else 
                    { 
                        // cannot produce now. show either green or red icons:
                        UpdateAttainable(attainableInfo, hzAttainable, hzNotAttainable,
                            hasTools, hasInputs, hasSkills, hasResources, canProduceNow, processType);
                            
                    }
                }
                else
                {                      
                    hzAttainable.Visible = false;
                    hzNotAttainable.Visible = false;                
                }
                

                UIComponent warning = itemRow.FindChildById(UIComponent.DataControlID.Warning, true);
                if (warning != null)
                {
                    if (showWarning) //canOrderFromInventory && OrderedItemsRequireWarning(expedition, currentOrder))
                    {
                        warning.Visible = true;
                    }
                    else
                    {
                        warning.Visible = false;
                    }
                }
            }
        }*/

       

      //  public const int maxStandingOrder = 50;

       
     /*   private void UpdateStandingOrderProduction(int productionLimit, int? outputBatchAmount, bool canProduceNow, ProductionOrder stockTarget, FillableBar fillableBar, ImageButton btStandingOrder)
        {
            fillableBar.Visible = true; // can always set orders

            int currentOrder = stockTarget.AmountToKeepInStore ?? 0;

            fillableBar.ColorAllControls = GameData.Instance.GUIConstants.StandingOrderTint; // Color.Cornsilk;
            btStandingOrder.NormalColor = GameData.Instance.GUIConstants.StandingOrderTint;
 
            // don't limit the slider based on inputs. use a fixed maximum, like 50...

            bool sliderValuesWereChanged = false;

            if (!IsDraggingSlider(fillableBar)) // don't change the slider that the user is currently dragging
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

        private void UpdateDirectProduction(int productionLimit, int? outputBatchAmount, bool canProduceNow, UWGame.SimSide.Expeditions.ProductionOrder stockTarget, ref bool showWarning, FillableBar fillableBar, ImageButton btStandingOrder)
        {
            int currentOrder = stockTarget.ProductionJobsToComplete ?? 0;

            if (OrderedItemsRequireWarning(expedition, currentOrder))
            {
                showWarning = true;
                //warning.Visible = true;
            }
            
            fillableBar.ColorAllControls = UIComponent.LCDTint;
            if (btStandingOrder != null)
            {
                btStandingOrder.NormalColor = UIComponent.LCDTint;
            }

            // update the slider:  
            bool sliderValuesWereChanged = false;

            ((ProductionTargetEventArgs)fillableBar.EventArgs).OutputBatchAmount = outputBatchAmount; // for converting back to Jobs...

            int totalProductionLimit = productionLimit;
            int totalCurrentOrder = currentOrder;
            if (outputBatchAmount.HasValue)
            {
                totalProductionLimit *= outputBatchAmount.Value;
                totalCurrentOrder *= outputBatchAmount.Value;

                fillableBar.StepSize = outputBatchAmount.Value; // set the minimum slider increments to the same as the batch size
            }
            else
            {
                fillableBar.StepSize = 1;
            }

            if (!IsDraggingSlider(fillableBar)) // don't change the slider that the user is currently dragging
            {
                // show/limit the slider based on available inputs:
                if (canProduceNow == false) // productionLimit == 0)
                {
                    // cannot produce NOW
                    fillableBar.Visible = false;

                }
                else
                {
                    fillableBar.Visible = true;

                 
                    if (fillableBar.MaxValue != totalProductionLimit)
                    {
                        fillableBar.MaxValue = totalProductionLimit;
                        sliderValuesWereChanged = true;
                    }

                    if (fillableBar.Value != totalCurrentOrder)
                    {
                        fillableBar.Value = totalCurrentOrder;
                        sliderValuesWereChanged = true;
                    }
                }
            }
           

            if (sliderValuesWereChanged)
            {
                fillableBar.UpdateSliderPosition();
            }

        }

        private void UpdateNonInventoryOrders(ProcessType processType, HorizontalList hzAttainable, HorizontalList hzNotAttainable)
        {
            hzAttainable.Visible = true;
            hzNotAttainable.Visible = false;

            hzAttainable.BeginAddingEntries();

            hzAttainable.TryRemoveEntry(IconKeys.NoProcess);
            hzAttainable.TryRemoveEntry(IconKeys.NoSkill);
            hzAttainable.TryRemoveEntry(IconKeys.NoResource);
            hzAttainable.TryRemoveEntry(IconKeys.NoPolicy);
            hzAttainable.TryRemoveEntry(IconKeys.NoInput);
            hzAttainable.TryRemoveEntry(IconKeys.NoTool);
            hzAttainable.TryRemoveEntry(IconKeys.Upgrade);

            Color color = UIComponent.LCDNormal; // GameData.Instance.GUIConstants.AttainableColor;
            UIComponent icon;
            string text = "";

            if (processType.IsSalvageProcess)
            {
                // show the salvage icon
                
                icon = AddOrGetIcon(IconKeys.Salvage, hzAttainable, "lcd_icon_recycleArrows", color);

                string input = ".";

                if (processType.InputsByType != null && processType.InputsByType.Count > 0)
                {
                    EntityType inputType = processType.InputsByType.First().Key;

                    // only show the hint if we own the item/structure input..
                    Availability availability;
                    if (The.InGameUI.InventorySettings.AllAvailableItems.TryGetValue(inputType, out availability)
                        && availability.NoOfAvailableItems > 0)
                    {
                        input = ", for example: " + inputType.Name + ".";
                    }
                }

                text = "Attainable from salvaging items or structures" +  input; // the items or structures may not exist (loops are possible)

                icon.ToolTip = text;
            }
            else 
            {
                hzAttainable.TryRemoveEntry(IconKeys.Salvage);
            }

            if (processType.IsHarvesting)
            {
                // show the gather icon
                icon = AddOrGetIcon(IconKeys.NoResource, hzAttainable, "lcd_icon_gather", color);
                text = "Attainable, but needs to be harvested from a resource with the GATHER action. Examine the tooltip to see the resource.";

               
                icon.ToolTip = text;
            }
            else
            {
                hzAttainable.TryRemoveEntry(IconKeys.NoResource);
            }

                     
            if (processType.IsUpgrade)
            {               
                icon = AddOrGetIcon(IconKeys.Upgrade, hzAttainable, "lcd_icon_uparrow", color);
                text = "This is an upgrade and it is constructed with the UPGRADE action. Examine the tooltip to see the objects that can be upgraded.";

                icon.ToolTip = text;
            }
            else
            {
                hzAttainable.TryRemoveEntry(IconKeys.Upgrade);
            }


            if (processType.IsSpecialActionType) 
            {
                // show the special site (star) icon
             
                icon = AddOrGetIcon(IconKeys.NoSpecialEntity, hzAttainable, "lcd_icon_special", color);
                text = string.Format(lblImmovableToolTip, processType.ActingOnType.Name);  //"Attainable, but needs a special action to be performed. Examine the tooltip to determine how.";
                
                icon.ToolTip = text;
            }
            else
            {
                hzAttainable.TryRemoveEntry(IconKeys.NoSpecialEntity);
            }
            
            
            hzAttainable.EndAddingEntries();
        }


        public static bool OrderedItemsRequireWarning(Expedition expedition, int orderedJobs)
        {
            if (orderedJobs > GameData.Instance.GUIConstants.OrderedJobsWithSameOutputToTriggerWarning)
            {
                //int totalJobs = expedition.OwnedEntities.coun 0;
               // expedition.OwnedEntities.ProductionJobs 
                if (TotalJobsRequireWarning(expedition))
                {
                    return true;
                }

            }

            return false;
        }

        public static bool TotalJobsRequireWarning(Expedition expedition)
        {
            //if (expedition.OwnedEntities.TotalDirectOrderProductionJobs > GetMaximumJobsBeforeWarning(expedition))
            if (expedition.OwnedEntities.ProductionOrders.TotalDirectOrders > EntityGroup.GetMaximumJobsBeforeWarning(expedition))
            {
                return true;
            }

            return false;
        }*/

        

      

        

       


        //private static void UpdateAttainable(AttainableInfo attainableInfo, Label lblAttainable, HorizontalList hzNotAttainable)
        //{
        //    if (attainableInfo != null
        //        /*&& attainableInfo.IsOwned == false*/) // currently, we dont compute attainability of owned items... 
        //    {
        //        if (attainableInfo.IsAttainable)
        //        {
        //            lblAttainable.Visible = true;                
        //            hzNotAttainable.Visible = false;
        //        }
        //        else
        //        {
        //            lblAttainable.Visible = false;                    
        //            hzNotAttainable.Visible = true;

        //            attainableInfo.PopulateIcons(hzNotAttainable); // display list of red icons
        //        }
        //    }
        //    else
        //    {
        //        lblAttainable.Visible = false;
        //        hzNotAttainable.Visible = false;
        //    }
        //}

     

       

        private static void UpdateItemRowTracking(UIComponent itemRow, EntityType entityType)
        {
            TrackTarget trackTarget;
            Bar barBackground;

          
            The.InGameUI.InventorySettings.TrackedTargets.TryGetValue(entityType, out trackTarget);


            barBackground = (Bar)itemRow.FindChildById(UIComponent.DataControlID.Background);
            ImageButton btTracking = (ImageButton)(itemRow.FindChildById(UIComponent.DataControlID.Track).Controls[0]); //(ImageButton)itemRow.FindChildById(UIComponent.DataControlID.Track);

            Color? barColor = null;
            string toolTip = null;


            //Get the Color and the ToolTip for the Bar
            if (The.InGameUI.InventorySettings.GetTrackedColorAndTooltip(entityType, out barColor, out toolTip))
            {
                barBackground.Visible = true;
                barBackground.SetSkinLocation(SkinState.Normal, null, barColor, barColor);

                Color hoverTint = new Color(barColor.Value.R - 40, barColor.Value.G - 40, barColor.Value.B - 40);
                barBackground.SetSkinLocation(SkinState.Hover, null, hoverTint, hoverTint);
                barBackground.ToolTip = toolTip;

                if (trackTarget != null)
                {               
                    btTracking.IsChecked = true;
                        //btTracking.Pressed = true; // should be IsChecked
               
                    btTracking.Visible = true;
                }
            }
            else
            {
                // btTracking.IsChecked = false;
                btTracking.IsChecked = false;
                               
                // hide the bar
                barBackground.Visible = false;
            }

            if (The.InGameUI.InventorySettings.SortingSettings.SortedBy == InventorySettings.SortColumns.Tracking)
            {
                // TODO: make this readable, please:
                itemRow.OrderByTag1 = barBackground.Visible == false ? 2 : 1;
                itemRow.OrderByTag1 = trackTarget == null ? itemRow.OrderByTag1 : 0;
            }
        }

        private void InventoryPanel_MouseOver(UIComponent sender, MouseEventArgs args)
        {
            
            if (viewType == ViewType.List)
            {
                SetTrackingVisibility(grdListView, args);
            }
            else
            {
                foreach (var grid in categoryGrids)
                {
                    SetTrackingVisibility(grid, args);
                }
            }
        }

        /// <summary>
        /// why was it necessary to iterate all entries?
        /// Reason:_____________
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="args"></param>
        private void SetTrackingVisibility(Grid grid, MouseEventArgs args)
        {
            foreach (var row in grid.Entries)
            {
                ShowHideTrackingButton(args, row);
            }
        }


        /*
        public static void ShowHidePadlockButton(UIComponent sender, bool show)
        {
            UIComponent row = sender.Parent;

            ImageButton btPadlock = (ImageButton)sender.Controls[0]; 
            
            // if not already tracking (hotspot should be visible = false)
            btPadlock.Visible = show;

        }*/


        private static void ShowHideTrackingButton(UIComponent sender, bool show)
        {
            UIComponent row = sender.Parent;

            ImageButton btTrack = (ImageButton)sender.Controls[0]; // (ImageButton)row.FindChildById(UIComponent.DataControlID.Track);
            TrackTarget trackedEntityType;

            if (!The.InGameUI.InventorySettings.TrackedTargets.TryGetValue((EntityType)row.Tag1, out trackedEntityType))
            {
                // if not already tracking (hotspot should be visible = false)
                btTrack.Visible = show;

               // Console.WriteLine("show: " + show);


                // tracking.Visible = false;
                if (The.InGameUI.InventorySettings.HasAvailableTrackingSlots())
                {
                    btTrack.ToolTip = tbTrackNotTrackedTooltip;
                    btTrack.Enabled = true;
                }
                else
                {
                    btTrack.ToolTip = TrackingLimitTooltip;
                    btTrack.Enabled = false; // disabled state is not a skin, hmm..
                }
            }
           /* else
            {
                btTrack.Enabled = true;
                btTrack.Visible = true;
            }*/
        }


        private static void ShowHideTrackingButton(MouseEventArgs args, UIComponent row)
        {
            ImageButton btTrack = (ImageButton)row.FindChildById(UIComponent.DataControlID.Track);
            TrackTarget trackedEntityType;


            if (row.AbsolutePosition.Y < args.Position.Y && row.AbsolutePosition.Y + row.Height > args.Position.Y)
            {
                if (row.AbsolutePosition.X < args.Position.X && row.AbsolutePosition.X + row.Width > args.Position.X)
                {
                    btTrack.Visible = true;
                }
                else
                {
                    btTrack.Visible = false;
                }
            }
            else
            {
                btTrack.Visible = false;
            }



            if (!The.InGameUI.InventorySettings.TrackedTargets.TryGetValue((EntityType)row.Tag1, out trackedEntityType))
            {
                // tracking.Visible = false;
                if (The.InGameUI.InventorySettings.HasAvailableTrackingSlots())
                {
                    btTrack.ToolTip = tbTrackNotTrackedTooltip;
                    btTrack.Enabled = true;
                }
                else
                {
                    btTrack.ToolTip = TrackingLimitTooltip;
                    btTrack.Enabled = false; // disabled state is not a skin, hmm..
                }
            }
            else
            {
                btTrack.Enabled = true;
                btTrack.Visible = true;
            }
        }



        public static bool CanBuildNow(EntityType entityType, EntityGroup owner, bool includeSalvageProcesses)
        {
            // bool canProduce;
            bool hasInputs;
            bool hasTools;
            int maxAmountThatCanBeProduced;
            int? noOfMissingInputTypes;
            int? noOfAvailableInputTypes;
            int? outputBatchAmount;
            bool hasSkills;
            bool hasResources;
            bool hasSpecialSite;
            bool hasPolicy;
            bool needsImmovableInput;
            EntityType immovableInput;
            ProcessType process;
            /* canProduce =*/

            InventoryPanel.GetBestProcessForDisplay(
                entityType,
                owner,
                out hasInputs,
                out hasTools,
                out maxAmountThatCanBeProduced,
                out noOfMissingInputTypes,
                out noOfAvailableInputTypes,
                out hasSkills,
                out hasResources,
                out hasSpecialSite,
                out hasPolicy,
                out immovableInput, // needsImmovableInput,
                out outputBatchAmount, out process,
                includeSalvageProcesses,
                null);

            needsImmovableInput = immovableInput != null;

            if (maxAmountThatCanBeProduced > 0) 
            {
                return true;
            }
            else
            {
                return false;
            }

           // return GetProductionAvailability(hasInputs, hasTools, maxAmountThatCanBeProduced, noOfMissingInputTypes, noOfAvailableInputTypes, hasSkills, needsImmovableInput);

        }

        /*
        private static ProductionAvailability GetProductionAvailability(bool hasInputs, bool hasTools, int maxAmountThatCanBeProduced, int? noOfMissingInputTypes, int? noOfAvailableInputTypes, bool hasSkills, bool needsImmovableInput)
        {
            if (maxAmountThatCanBeProduced > 0) // && !needsImmovableInput) // && hasInputs && hasTools && hasSkills && !needsImmovableInput) // ???
            {
                return ProductionAvailability.CanBuildNow;
            }
            else if (noOfMissingInputTypes == maxNumberOfMissingInputsToDisplayBuildingsWithout
                && noOfAvailableInputTypes > 0 // don't include items with zero available inputs in the lookahead
                && !needsImmovableInput)
            {
                return ProductionAvailability.MissingOneInputType;
            }
            else
            {
                return ProductionAvailability.FurtherAway;
            }
        }
        */
        /*  private static BuildAvailability GetAvailability(bool hasInputs, bool hasTools, int? noOfMissingInputTypes, int? noOfAvailableInputTypes, bool hasSkills, bool needsImmovableInput)
          {
              if (hasInputs && hasTools && hasSkills && !needsImmovableInput)
              {
                  return BuildAvailability.CanBuildNow;
              }
              else if (noOfMissingInputTypes == maxNumberOfMissingInputsToDisplayBuildingsWithout
                  && noOfAvailableInputTypes > 0 // don't include items with zero available inputs in the lookahead
                  && !needsImmovableInput)
              {
                  return BuildAvailability.MissingOneInputType;
              }
              else
              {
                  return BuildAvailability.DoNotDisplay;
              }
          }*/




        #region Untouched

      /*  public static Color UpdateStockButton(int noOfAvailableItems, int noOfIncompleteItems, int noOfItemsUsedAsParts, UIComponent itemComponent, TextButton tbStock, EntityType entityType)
        {
            bool inStock = noOfAvailableItems > 0;
            int noOfcurrentOrder;

            if (entityType.StructureType == null)
            {
                noOfcurrentOrder = The.InGameUI.GetExpedition().Stocks.Targets[entityType].ProductionTarget;
            }
            else
            {
                noOfcurrentOrder = 0;//The.InGameUI.GetExpedition().Stocks.Targets[entityType].ProductionTarget;
            }
            Color color = DataTypeButton.GetStockStatusColor(inStock, tbStock.GetNormalColor(), tbStock.Type);

            if (noOfIncompleteItems != 0 && noOfAvailableItems == 0 || noOfItemsUsedAsParts != 0 && noOfAvailableItems == 0)
            {
                color = Common.ColorFromHex("#FFFFFF");
            }

            tbStock.Color = color;

            tbStock = InventoryPanel.SetStockButtonToolTipAndText(tbStock, noOfAvailableItems, noOfIncompleteItems, noOfItemsUsedAsParts, noOfcurrentOrder);

            if (inStock || noOfIncompleteItems != 0 || noOfItemsUsedAsParts != 0)
            {
                tbStock.Tag1 = true;
                tbStock.Enabled = true;
            }
            else
            {
                tbStock.Enabled = false;
            }

            //  itemComponent.OrderByTag1 = inStock ? 1 : 0;
            tbStock.ScaleWidthToFitText();

            return color;
        }*/

      /*  public static TextButton SetStockButtonToolTipAndText(TextButton tbStock, int noOfAvailableItems, int noOfIncompleteItems, int noOfItemsUsedAsParts, int noOfcurrentOrder)
        {
            bool inStock = noOfAvailableItems > 0;
            tbStock.Tag1 = inStock;

            //Order of Numbers on the StockButton -> In Stock / Part of / Being produced

            FillStockButton(tbStock, noOfAvailableItems, noOfIncompleteItems, noOfItemsUsedAsParts, true);

            if (noOfItemsUsedAsParts == 0 && noOfIncompleteItems == 0 && noOfAvailableItems == 0)
            {
                tbStock.Text = "0";
                tbStock.ToolTip = "No items in stock";
            }
            return tbStock;
        }*/

      /*  public static void FillStockButton(TextButton tbStock, int noOfAvailableItems, int noOfIncompleteItems, int noOfItemsUsedAsParts, bool displayParts)
        {
            //In Stock
            tbStock.Text = noOfAvailableItems.ToString();
            tbStock.ToolTip = noOfAvailableItems + " finished Items";

            if (displayParts)
            {
                // Part of
                tbStock.Text += "|" + noOfItemsUsedAsParts;
                tbStock.ToolTip = tbStock.ToolTip + " \n" + noOfItemsUsedAsParts + " Items are parts";
            }
            // Being produced            
            tbStock.Text += "|" + noOfIncompleteItems;
            tbStock.ToolTip = tbStock.ToolTip + " \n" + noOfIncompleteItems + " Items being produced";

            tbStock.ToolTip = tbStock.ToolTip + "\n \n Click to see the list";

            tbStock.ScaleWidthToFitText();
        }*/

        /// <summary>
        /// resources etc. can currently not be ordered from here.
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
       /* private bool CanGiveProductionOrdersFromThisPanel(EntityType entityType)
        {
            List<ProcessType> processes;
            if (GameData.Instance.ProcessYieldsThisOutput.TryGetValue(entityType, out processes))
            {
                foreach (var process in processes)
                {
                    if (process.IsAccessibleFromInventoryPanel()) 
                    {
                        bool processInputsAreAllItems = false;

                        if (process.InputsByType != null
                            && process.Inputs.Length > 0) // processes without inputs defined can not be ordered
                        {
                            processInputsAreAllItems = true;

                            foreach (var input in process.InputsByType)
                            {
                                if (input.Key.ItemType == null)
                                {
                                    // requires a non-item input...
                                    processInputsAreAllItems = false;
                                    break;
                                }
                            }
                        }

                        if (processInputsAreAllItems)
                        {
                            return true; // there is a process that does not require non-item inputs (like animals or trees)
                        }
                    }
                }
            }

            return false;
        }*/


        public static CollapsablePanel.SubType GetPanelSubType(EntityCategory category)
        {
            switch(category.CategoryColor)
            {
                case EntityCategory.CategoryColors.Blue:
                    return CollapsablePanel.SubType.Blue;

                case EntityCategory.CategoryColors.Green:
                    return CollapsablePanel.SubType.Green;

                case EntityCategory.CategoryColors.Red:
                    return CollapsablePanel.SubType.Red;

                case EntityCategory.CategoryColors.None:
                default:
                    return CollapsablePanel.SubType.Normal;

            }
        }


        private void AddCategoryRow(ref CollapsablePanel cpCategory, ref Grid categoryGrid, object key)
        {
            EntityCategory entityCategory = key as EntityCategory;
           // StructureCategory strCategory = key as StructureCategory;

            cpCategory = new CollapsablePanel(Interface.gui, CollapsablePanel.PanelType.DropDownBig);
            cpCategory.HeadingYPos = 4;
            cpCategory.CollapsedHeight = 28;// grdCategoryView.ItemHeight;

            grdCategoryView.AddEntry(key, cpCategory);
            cpCategory.OrderByTag1 = entityCategory.SortOrder; // Name;  //entityCategory == null ? strCategory.Name : entityCategory.Name;

            cpCategory.Init(GetPanelSubType(entityCategory));
            cpCategory.Title = entityCategory.Name;  //entityCategory == null ? strCategory.Name.ToUpper(Config.Culture) : entityCategory.Name;
            cpCategory.Width = grdCategoryView.Width;


            if (entityCategory.DisplayStructureIcon) // key as StructureCategory != null)
            {
                Image scanIcon = new Image(Interface.gui);
                scanIcon.SetSkinLocation(SkinState.Normal,Interface.gui.GUISpriteSheet.GetSourceRectangle("HUD_icon_structure"));
                scanIcon.ResizeControlToFitImage();
                scanIcon.X = 8;
                scanIcon.ID = UIComponent.DataControlID.StatusIcon;
                scanIcon.Visible = true;

                cpCategory.CenterOnHeader(scanIcon);
                scanIcon.Y = scanIcon.Y + 2;
                cpCategory.TitlePositionX = scanIcon.Right + 6 - 2;
                cpCategory.Add(scanIcon);
            }

            categoryGrid = new Grid(Interface.gui, ListBoxType.LCD, Label.LabelType.LCDNormal);
            categoryGrid.DebugTag = "categoryGrid";

            categoryGrid.FixedItemHeights = true; // false;
            categoryGrid.Width = cpCategory.Width; // make grid fill the collapsable panel
            cpCategory.AddContent(categoryGrid); // .ExpandedPanel.Add(categoryGrid);

            categoryGrid.ScrollBarEnabled = false;
            categoryGrid.ItemHeight = ItemHeight;
            categoryGrid.CanGrowInHeight = true;
            categoryGrid.Font = GUIManager.LCDandHUDBodyFontPath;
            categoryGrid.IsOuterGrid = false;
        }

        private const int orderedX = 200;

        private int GetMaxProduction(EntityType entityType)
        {
            return 5;
        }

        private UIComponent AddItemRow(Grid grid, EntityType entityType, EntityGroup owner, bool useCurrentUIOwner, bool gridList)
        {
          //  grid.MouseMove += InventoryPanel_MouseOver;

            Image icon;
            UIComponent item;

            item = new UIComponent(Interface.gui);
            item.DebugTag = "stocksItem";

            grid.AddEntry(entityType, item);
            item.CanHaveFocus = true;

            Bar bar = new Bar(Interface.gui);
            bar.ID = UIComponent.DataControlID.Background;
            bar.X = 0;
            bar.Width = 300;
            bar.EdgeSize = 23;
            Rectangle rectangle = Interface.gui.GUISpriteSheet.GetSourceRectangle("basic_highlightBar_white");
            bar.SetSkinLocation(SkinState.Normal, rectangle);
            bar.SetSkinLocation(SkinState.Hover, rectangle);
            bar.Height = rectangle.Height;
            bar.Visible = false;

            item.Add(bar);
            item.CenterChildVertically(bar);

            icon = AddEntityTypeIcon(entityType, item, itemTypeIconColumnX);


            if (entityType.StructureType != null)
            {
                icon.Color = GameData.Instance.GUIConstants.sidePanelTextColor;
            } 
            else if (entityType.TreeType != null)
            {
                icon.Color = GameData.Instance.GUIConstants.sidePanelTextColor;
            }             
            else if (entityType.BiologicalType != null)
            {
                icon.Color = GameData.Instance.GUIConstants.sidePanelTextColor; 
               
            }
            else if (entityType.TerrainType != null && entityType.TerrainType.IsSpecialInterestFeature)
            {
                icon.Color = GameData.Instance.GUIConstants.sidePanelTextColor;
            }
           


            DataTypeButton dtCaption = new DataTypeButton(Interface.gui, HUD_Windows.DataSheet.InfoToShow.Production, entityType,
                GoalEvaluator.GetOwnerID(owner),
                useCurrentUIOwner);
            dtCaption.Init(TextButton.TextButtonType.LCDToolTipBlack);
            dtCaption.ID = UIComponent.DataControlID.Caption;
            item.Add(dtCaption);
            dtCaption.IsRoot = true;
           // dtCaption.Text = entityType.PluralName;
            dtCaption.TextAlignment = TextButton.TextAlign.Left;
            dtCaption.Width = availableX - captionX - 6;
            dtCaption.X = captionX;
                       

            item.CenterChildVertically(dtCaption);

           ProductionTargetEventArgs eventArgs = new ProductionTargetEventArgs(entityType, null);
           /*  AddProductionControls(entityType, item, eventArgs, availableX, itemTypeProductionTargetColumnX,
                tbItems_Click, fillableBar_SliderMouseDown, InventoryPanelPadlock_Click);
            */

           ProductionOrderControl orders = new ProductionOrderControl(entityType, eventArgs, item.guiManager, ProductionOrderControl.UILayout.LCD, 83, // itemTypeProductionTargetColumnX, 
                tbItems_Click, //fillableBar_SliderMouseDown, 
                InventoryPanelPadlock_Click);

            item.Add(orders);
            orders.X = availableX;
            orders.ID = UIComponent.DataControlID.Orders;


            if (GameData.Instance.GUIConstants.EnableFilters)
            {
                item.DebugTag = "itemRow";

                // item.MouseOver += InventoryPanel_MouseOver;
                //  item.MouseOut += OLD: InventoryPanel_MouseOver;

                // the hotspot contains the tracking button.
                UIComponent trackingHotspot = new UIComponent(Interface.gui);
                item.Add(trackingHotspot);
                trackingHotspot.Position = new Point(itemTypeProductionTargetColumnX + 125, 0); // -5); //new Point(itemTypeProductionTargetColumnX + 105 + 20, -5);
                trackingHotspot.Width = 60;
                trackingHotspot.Height = item.Height;
                trackingHotspot.MouseOver += trackingHotspot_MouseOver;
                trackingHotspot.MouseOut += trackingHotspot_MouseOut;
                trackingHotspot.EventArgs = eventArgs;
                trackingHotspot.ID = UIComponent.DataControlID.Track;

                ImageButton tbTracking = new ImageButton(Interface.gui);
                trackingHotspot.Add(tbTracking); //  item.Add(tbTracking);
                tbTracking.InitWithIcon(ImageButtonType.LCD, "basic_icon_crosshairs", true);
                tbTracking.Position = new Point(20, 0); //tbTracking.Position = new Point(itemTypeProductionTargetColumnX + 125 + 20, -5); 
                tbTracking.EventArgs = eventArgs;
                //  tbTracking.ID = UIComponent.DataControlID.Track;
                tbTracking.Click += btTrack_Click;
                tbTracking.Visible = false;
                tbTracking.ToolTip = tbTrackNotTrackedTooltip;
                tbTracking.Width = 36;
                item.CenterChildVertically(tbTracking);
                tbTracking.MouseOut += tbTracking_MouseOut;

                //if its the categoryView we need move the trackButton a bit to the left 
                if (!gridList)
                {
                    tbTracking.X = tbTracking.X - 11;
                }
            }


            #region commented
            /*
            Spinner spKeepInStore, spProductionTarget;

            spKeepInStore = new Spinner(intf.gui);
            item.Add(spKeepInStore);
            spKeepInStore.Init(Spinner.SpinnerType.LCD);
            spKeepInStore.Count = expedition.Stocks.Targets[entityType].KeepInStore; // game.MinDesiredItemsInStock[itemTypeInCategory];
            spKeepInStore.CountChanged += new Spinner.CountChangedHandler(keepInStore_CountChanged);
            spKeepInStore.EventArgs = new ItemTypeButtonEventArgs(entityType);
            spKeepInStore.ColumnID = "keepInStore";
            // spKeepInStore.DebugTag = "keepInStore";
            spKeepInStore.Position = new Point(itemTypeKeepInStoreColumnX, 0);
            spKeepInStore.NoOfDigits = 4;
            spKeepInStore.ToolTip = "Change the amount of this item to keep in stock.";



            spProductionTarget = new Spinner(intf.gui);
            item.Add(spProductionTarget);
            spProductionTarget.Init(Spinner.SpinnerType.LCD);
            spProductionTarget.Count = expedition.Stocks.Targets[entityType].ProductionTarget; // game.MinDesiredItemsInStock[itemTypeInCategory];
            spProductionTarget.CountChanged += new Spinner.CountChangedHandler(productionTarget_CountChanged);
            spProductionTarget.EventArgs = new ItemTypeButtonEventArgs(entityType);
            spProductionTarget.ColumnID = "productionTarget";
            //  spProductionTarget.DebugTag = "productionTarget";
            spProductionTarget.Position = new Point(itemTypeProductionTargetColumnX, 0);
            spProductionTarget.NoOfDigits = 4;
            spProductionTarget.ToolTip = "Change the amount of this item to produce.";           
*/
            #endregion

            return item;
        }

      

        /*
        void standingHotspot_MouseOut(UIComponent sender, MouseEventArgs args)
        {
            ImageButton btPadlock = sender.Controls[0] as ImageButton;

            if (!btPadlock.IsChecked)
            {
                ShowHidePadlockButton(sender, false);
            }
        }

        void standingHotspot_MouseOver(UIComponent sender, MouseEventArgs args)
        {
            ShowHidePadlockButton(sender, true);
        }

        void tbStanding_MouseOut(UIComponent sender, MouseEventArgs args)
        {
            UIComponent hotspot = sender.Parent;
            if (!((ImageButton)sender).IsChecked
                && !hotspot.CheckCoordinates(args.Position.X, args.Position.Y))
            {
                ShowHidePadlockButton(hotspot, false);
            }
        }
        */

        void InventoryPanelPadlock_Click(UIComponent sender, EventArgs e)
        {
            Expedition expedition = The.InGameUI.GetExpedition(); // The.Sim.PlaySite.GetFirstPlayerExpedition();
            if (expedition == null)
                return; 

            ProductionOrderControl.Padlock_Click(sender, e);

            ProductionTargetEventArgs prodArgs = e as ProductionTargetEventArgs;          
            EntityGroup owner = expedition.OwnedEntities;
            UpdateItemRow(sender.Parent.Parent, prodArgs.Item, owner);
        }

       

        void tbTracking_MouseOut(UIComponent sender, MouseEventArgs args)
        {
            // after unchecking, it was often possible to move the moue away with the hotspot receiving its mouse out event.
            UIComponent hotspot = sender.Parent;
            if (!((ImageButton)sender).IsChecked
                && !hotspot.CheckCoordinates(args.Position.X, args.Position.Y))
            {
                ShowHideTrackingButton(hotspot, false);
            }
        }

        void trackingHotspot_MouseOut(UIComponent sender, MouseEventArgs args)
        {           
            ShowHideTrackingButton(sender, false);

        }

        void trackingHotspot_MouseOver(UIComponent sender, MouseEventArgs args)
        {          
            ShowHideTrackingButton(sender, true);
        }

       

        public static Image AddEntityTypeIcon(EntityType entityType, UIComponent item, int? xPosToCenterAbout = null)
        {
            Image icon = new Image(item.guiManager);
                         
            IconInfo iconInfo;
            Rectangle rect;
            rect = entityType.GetIconSprite(out iconInfo);           
            icon.SetSkinLocation(SkinState.Normal,rect);
            icon.Texture = item.guiManager.GUISpriteSheet.Texture;
            item.Add(icon);
            icon.ResizeControlToFitImage();
            item.CenterChildVertically(icon, iconInfo != null ? iconInfo.CenterYPos : null);
            if (xPosToCenterAbout.HasValue)
            {
                item.CenterHorizontally(xPosToCenterAbout.Value, icon);
            }

            return icon;
        }

            

       
      



        public void OnSetProduction(string entityTypeKey)
        {
            EntityType entityType = GameData.Instance.AllEntityTypes[entityTypeKey];
         
            ActOnRow(entityType, ResetSliderBeingDragged);

          //  sliderBeingDragged = null;
            Populate();
        }

        void ResetSliderBeingDragged(UIComponent row)
        {
            ProductionOrderControl order = (ProductionOrderControl)row.FindChildById(UIComponent.DataControlID.Orders);
            if (order != null)
            {
                order.ResetSliderBeingDragged();
            }
        }

        /*
        private static bool FilterItems(EntityType entityType, EntityGroup owner, Dictionary<EntityType, InventoryPanel.Availability> allAvailableItems = null, bool countPartsOfEntities = false)
        {
            bool ownsItem, hasInputs, hasTools;

            OwnsProductOrHasProcessInputsAndTools(entityType, owner, out ownsItem, out hasInputs, out hasTools, allAvailableItems, countPartsOfEntities);

            if (!ownsItem) // not in inventory
            {
                List<ProcessType> processesYieldingOutput;
                if (GameData.Instance.ProcessYieldsThisOutput.TryGetValue(entityType, out processesYieldingOutput))
                {
                    if (!processesYieldingOutput.Exists(p =>
                        (p.IsAccessibleFromInventoryPanel() // IsProduction() 
                        && !p.IsPrimaryProcess)))
                    {
                        return false; // only primary or salvage processes yield this item - don't show it.
                    }
                }
                else return false;
            }

            return ownsItem || hasInputs; // || hasTools;
        }
        */

        /// <summary>
        /// this function disregards items that are parts inside other items, if desired
        /// 
        /// returns true if we own the item or have one or more inputs for it
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="owner"></param>
        /// <param name="allAvailableItems"></param>
        /// <returns></returns>
        public static bool OwnsProductOrHasProcessInputsAndTools(EntityType entityType, EntityGroup owner, Dictionary<EntityType, InventoryPanel.Availability> allAvailableItems = null, 
            bool countPartsOfEntities = false)
        {
            bool ownsItem, ownsItemIncludingIntrinsicParts, hasInputs, hasTools;

            OwnsProductOrHasProcessInputsAndTools(entityType, owner, out ownsItem, out ownsItemIncludingIntrinsicParts, out hasInputs, out hasTools, allAvailableItems, countPartsOfEntities);

            return ownsItem || hasInputs; // || hasTools;

        }

        /// <summary>
        /// TODO: Should this method also return hasSkills?
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="owner"></param>
        /// <param name="ownsItem"></param>
        /// <param name="hasInputs"></param>
        /// <param name="hasTools"></param>
        /// <param name="allAvailableItems"></param>
        /// <param name="countPartsOfEntities"></param>
        public static void OwnsProductOrHasProcessInputsAndTools(EntityType entityType, EntityGroup owner, 
            out bool ownsItem, out bool ownsItemIncludingIntrinsicPart,
            out bool hasInputs, out bool hasTools, 
            Dictionary<EntityType, InventoryPanel.Availability> allAvailableItems = null, 
            bool countPartsOfEntities = false)
        {
            hasInputs = false;
            hasTools = false;
            int noOfIncompleteEntities;
            int noOfEntitiesUsedAsParts;
            int noOfItemsOffSite;
            int noOfItemsOwnedByOthers;
            int noOfAvailableItemsIncludingIntrinsic;

            if (owner == null)
            {
                ownsItem = false;
                ownsItemIncludingIntrinsicPart = false;
            }
            else
            {

                int available = GetNoOfAvailableEntities(owner.AllEntities, owner, entityType, out noOfIncompleteEntities,
                                                         out noOfEntitiesUsedAsParts, out noOfItemsOffSite, out noOfItemsOwnedByOthers, out noOfAvailableItemsIncludingIntrinsic,
                                                         allAvailableItems, countPartsOfEntities);

                if (available != 0 || noOfIncompleteEntities != 0) // inconsistency here...
                {
                    ownsItem = true;
                    ownsItemIncludingIntrinsicPart = true;
                }
                else
                {
                    if (noOfAvailableItemsIncludingIntrinsic > 0)
                    {
                        ownsItemIncludingIntrinsicPart = true;
                    }
                    else
                    {
                        ownsItemIncludingIntrinsicPart = false;
                    }

                    ownsItem = false;
                    int productionLimit;
                    int? noOfMissingInputTypes;
                    int? noOfAvailableInputTypes;
                    int? outputBatchAmount;
                    bool hasSkills;
                    bool hasResources;
                    bool hasSpecialSite;
                    bool hasPolicy;
                    EntityType immovableInput;
                    ProcessType process;

                    GetBestProcessForDisplay(entityType, owner, out hasInputs, out hasTools, out productionLimit,
                        out noOfMissingInputTypes, out noOfAvailableInputTypes, out hasSkills, out hasResources, out hasSpecialSite, out hasPolicy, out immovableInput, out outputBatchAmount, out process, The.InGameUI.InventorySettings.IncludeSalvageProcesses,
                        null,
                        allAvailableItems);
                }
            }
        }

        private static bool HandleProcess(EntityType entityType, ProcessType processType,
            EntityGroup owner,
            out bool hasInputs,
            out bool hasTools,
            out int maxAmountThatCanBeProduced,
            out int? noOfMissingInputTypes,
            out int? noOfAvailableInputTypes,
            out bool hasSkills,
            out bool hasResources,
            out bool hasSpecialSite,
            out bool hasPolicy,
            out EntityType needsImmovableInput,          
            out int? outputBatchAmount,
            Dictionary<EntityType, InventoryPanel.Availability> allAvailableItems = null)
        {
            outputBatchAmount = processType.GetOutputAmount(entityType);

            return HasAllInputsAndToolsForProcess(processType, owner, out hasInputs, out hasTools, out maxAmountThatCanBeProduced,
                out noOfMissingInputTypes, out noOfAvailableInputTypes, out hasSkills, out hasResources, out hasSpecialSite, out hasPolicy, out needsImmovableInput, allAvailableItems);
           
        }

        /// <summary>
        /// Scores the possible processes and returns the 'best' one. (Perhaps enable the user to scroll them all?)
        /// returns a process that can produce now if possible. Also accepts a predicate to return the second best option.
        /// 
        /// Ongoing inventory processes should score high since they cannot be cancelled from the task manager. But only on the Inventory panel
        /// 
        /// This is used on inventory panel to show relevant info.
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="owner"></param>
        /// <param name="hasInputs"></param>
        /// <param name="hasTools"></param>
        /// <param name="maxAmountThatCanBeProduced"></param>
        /// <param name="noOfMissingInputTypes"></param>
        /// <param name="noOfAvailableInputTypes"></param>
        /// <param name="hasSkills"></param>
        /// <param name="hasResources"></param>
        /// <param name="hasSpecialSite"></param>
        /// <param name="needsImmovableInput"></param>
        /// <param name="outputBatchAmount"></param>
        /// <param name="processType"></param>
        /// <param name="includeSalvageProcesses"></param>
        /// <param name="preferProcess"></param>
        /// <param name="allAvailableItems"></param>
        /// <returns></returns>
    /*    public static bool GetBestProcessForDisplay(EntityType entityType, EntityGroup owner, out bool hasInputs, out bool hasTools, out int maxAmountThatCanBeProduced,
            out int? noOfMissingInputTypes, out int? noOfAvailableInputTypes, out bool hasSkills, out bool hasResources, out bool hasSpecialSite, out bool hasPolicy, out EntityType needsImmovableInput, out int? outputBatchAmount, out ProcessType processType,
            bool includeSalvageProcesses, 
            Predicate<ProcessType> preferProcess,
            Dictionary<EntityType, InventoryPanel.Availability> allAvailableItems = null,
            Predicate<ProcessType> allowProcess = null)
        {
            List<ProcessType> listOfProcesses;
            GameData.Instance.ProcessYieldsThisOutput.TryGetValue(entityType, out listOfProcesses);

            if (includeSalvageProcesses)
            {
                List<ProcessType> listOfSalvageProcesses;
                if (GameData.Instance.SalvageProcessYieldsThisOutput.TryGetValue(entityType, out listOfSalvageProcesses))
                {
                    if (listOfProcesses != null)
                    {
                        listOfProcesses = new List<ProcessType>(listOfProcesses); // copy
                        listOfProcesses.AddRange(listOfSalvageProcesses);
                    }
                    else
                    {
                        listOfProcesses = listOfSalvageProcesses;
                    }
                }
            }

            if (listOfProcesses != null) 
            {
                // harvesting procs are allowed here!

                // iterate all processes, return one that works
                // prioritize inventory processes over gather processes - especially when current inventory orders are underway. It is the only way to cancel them
                foreach (var process in listOfProcesses)
                {
                    if (allowProcess == null || allowProcess(process))
                    {
                        processType = process;

                        bool canProduceNow = HandleProcess(entityType, processType, owner, out hasInputs, out hasTools, out maxAmountThatCanBeProduced,
                            out noOfMissingInputTypes, out noOfAvailableInputTypes, out hasSkills, out hasResources, out hasSpecialSite, out hasPolicy, out needsImmovableInput, out outputBatchAmount, allAvailableItems);

                        if (listOfProcesses.Count == 1 || canProduceNow == true)
                        {
                            return canProduceNow;
                        }
                    }
                }

                // if none was fulfilled, select any process, the missing prerequisites will be displayed in the form of green icons:
                // prefer processes that are available from the inventory panel, i.e. try to exclude salvage and harvest
                processType = listOfProcesses.FirstOrDefault(p => (allowProcess == null || allowProcess(p)) && (preferProcess == null || preferProcess(p))); 
                if (processType == null)
                {
                    if (allowProcess == null || allowProcess(listOfProcesses[0]))
                    {
                        processType = listOfProcesses[0];
                    }
                }

                if (processType != null)
                {
                    return HandleProcess(entityType, processType, owner, out hasInputs, out hasTools, out maxAmountThatCanBeProduced,
                            out noOfMissingInputTypes, out noOfAvailableInputTypes, out hasSkills, out hasResources, out hasSpecialSite, out hasPolicy, out needsImmovableInput, out outputBatchAmount, allAvailableItems);
                }

            }
           

            hasSkills = false;
            hasInputs = false;
            hasTools = false;
            hasResources = false;
            hasSpecialSite = false;
            hasPolicy = false;
            needsImmovableInput = null; // false;
            maxAmountThatCanBeProduced = 0;
            noOfMissingInputTypes = null;
            noOfAvailableInputTypes = null;
            outputBatchAmount = null;
            processType = null;

            return false;

        }*/



         /// <summary>
        /// Scores the possible processes and returns the 'best' one. (Perhaps enable the user to scroll them all?)
        /// returns a process that can produce now if possible. Also accepts a predicate to return the second best option.
        /// 
        /// Ongoing inventory processes should score high since they cannot be cancelled from the task manager. But only on the Inventory panel
        /// 
        /// This is used on inventory panel to show relevant info.
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="owner"></param>
        /// <param name="hasInputs"></param>
        /// <param name="hasTools"></param>
        /// <param name="maxAmountThatCanBeProduced"></param>
        /// <param name="noOfMissingInputTypes"></param>
        /// <param name="noOfAvailableInputTypes"></param>
        /// <param name="hasSkills"></param>
        /// <param name="hasResources"></param>
        /// <param name="hasSpecialSite"></param>
        /// <param name="needsImmovableInput"></param>
        /// <param name="outputBatchAmount"></param>
        /// <param name="processType"></param>
        /// <param name="includeSalvageProcesses"></param>
        /// <param name="guiPreferProcess"></param>
        /// <param name="allAvailableItems"></param>
        /// <returns></returns>
        public static bool GetBestProcessForDisplay(EntityType entityType, EntityGroup owner, out bool hasInputs, out bool hasTools, out int maxAmountThatCanBeProduced,
            out int? noOfMissingInputTypes, out int? noOfAvailableInputTypes, out bool hasSkills, out bool hasResources, out bool hasSpecialSite, out bool hasPolicy, out EntityType needsImmovableInput, out int? outputBatchAmount, out ProcessType processType,
            bool includeSalvageProcesses, 
            Predicate<ProcessType> guiPreferProcess,
            Dictionary<EntityType, InventoryPanel.Availability> allAvailableItems = null,
            Predicate<ProcessType> allowProcess = null)
        {
            List<ProcessType> listOfProcesses;
            GameData.Instance.ProcessYieldsThisOutput.TryGetValue(entityType, out listOfProcesses);

            if (includeSalvageProcesses)
            {
                List<ProcessType> listOfSalvageProcesses;
                if (GameData.Instance.SalvageProcessYieldsThisOutput.TryGetValue(entityType, out listOfSalvageProcesses))
                {
                    if (listOfProcesses != null)
                    {
                        listOfProcesses = new List<ProcessType>(listOfProcesses); // copy
                        listOfProcesses.AddRange(listOfSalvageProcesses);
                    }
                    else
                    {
                        listOfProcesses = listOfSalvageProcesses;
                    }
                }
            }

            if (listOfProcesses != null) 
            {
                processType = GetBestProcessForDisplay(entityType, listOfProcesses, 
                    guiPreferProcess, allowProcess, owner, allAvailableItems);

                if (processType != null)
                {
                    return HandleProcess(entityType, processType, owner, out hasInputs, out hasTools, out maxAmountThatCanBeProduced,
                            out noOfMissingInputTypes, out noOfAvailableInputTypes, out hasSkills, out hasResources, out hasSpecialSite, out hasPolicy, out needsImmovableInput, out outputBatchAmount, allAvailableItems);
                }

            }
           

            hasSkills = false;
            hasInputs = false;
            hasTools = false;
            hasResources = false;
            hasSpecialSite = false;
            hasPolicy = false;
            needsImmovableInput = null; // false;
            maxAmountThatCanBeProduced = 0;
            noOfMissingInputTypes = null;
            noOfAvailableInputTypes = null;
            outputBatchAmount = null;
            processType = null;

            return false;

        }
     
        /// <summary>
        /// we can only show one process (gather or inventory etc.) at a time
        /// score the possible processes and return the best one together with its flags
        /// </summary>
        /// <returns></returns>
        private static ProcessType GetBestProcessForDisplay(EntityType entityType, List<ProcessType> processes, Predicate<ProcessType> guiPrefersProcess, Predicate<ProcessType> allowProcess, EntityGroup owner,
            Dictionary<EntityType, InventoryPanel.Availability> allAvailableItems) 
        {

            float bestScore = 0f;
            ProcessType bestProcess = null;

            // iterate all processes, return one that works
            // prioritize inventory processes over gather processes - especially when current inventory orders are underway. It is the only way to cancel them
            foreach (var process in processes)
            {
                float score = 0f;
                if (allowProcess == null || allowProcess(process))
                {
                    if ((guiPrefersProcess == null || guiPrefersProcess(process))) // processes that give orders in this interface
                    {
                        score += 2f;

                        //score ongoing processes highest, whether or not they can produce
                        if (OrdersExist(owner, entityType, process))
                        {
                            score += 10f;
                        }
                    }

                    // score processes that can produce higher
                    bool canProduceNow = HasAllInputsAndToolsForProcess(process, owner, allAvailableItems);

                    if (canProduceNow)
                    {
                        score += 5f;
                    }

                    if (score >= bestScore)
                    {
                        bestProcess = process;
                        bestScore = score;
                    }
                }
            }

            return bestProcess;

        }


        private static bool OrdersExist(EntityGroup owner, EntityType entityType, ProcessType process)
        {
            if (!owner.ProductionOrders.OrdersExist(entityType))
            {
                List<ProcessJob> jobs;
                if (owner.ProductionJobs.TryGetValue(entityType, out jobs))
                {
                    return jobs.Any(p => p.ProcessType == process);
                }
            }

            return false;
        }

        private static ProcessType SelectProcessTypeForOutput(List<ProcessType> listOfProcesses)
        {
            if (listOfProcesses.Count == 1)
            {
                return listOfProcesses[0];
            }
            else
            {
                // prefer non harvest processes:
                ProcessType processType = listOfProcesses.FirstOrDefault(p => p.IsPartOfProductionChainButCannotOrderFromInventory());
                if (processType == null)
                {
                    return listOfProcesses[0];
                }
                else return processType;
            }
        }

        public override void Hide()
        {
            base.Hide();

            foreach (var item in grdCategoryView.Entries)
            {
                ResetSliderBeingDragged(item);
            }

            foreach (var item in grdListView.Entries)
            {
                ResetSliderBeingDragged(item);
            }
            //sliderBeingDragged = null;

        }

        /// <summary>
        /// if owner is null, 0 availability will be shown...
        /// 
        /// Uses the same functionallity as HasAllInputsAndToolsForProcess
        /// But it only checks for one entitytype
        /// 
        /// So you can check seperatly for each input for an process to see what you got enough of and what you do not have enough of.
        /// </summary>
        /// <param name="process"></param>
        /// <param name="owner"></param>
        /// <param name="hasInputs"></param>
        /// <param name="maxAmountThatCanBeProduced"></param>
        /// <param name="noOfMissingInputTypes"></param>
        /// <param name="typeToCheck"></param>
        /// <returns></returns>
        public static bool HasInputForProcess(
            ProcessType process,
            EntityGroup owner,
            out bool hasInputs,
            out int maxAmountThatCanBeProduced,
            out int? noOfMissingInputTypes,
            out int? noOfAvailableInputTypes, // only used for lookahead filtering in the build menu
            out int? noOfAvailableItems,
            EntityType typeToCheck)
        {
            maxAmountThatCanBeProduced = 0;
            hasInputs = true;
            int? currentProductionLimit = null;

            // see if all inputs are there - disregarding assignment etc.
            if (process.InputsByType != null)
            {
                noOfMissingInputTypes = 0;
                noOfAvailableInputTypes = 0;
                noOfAvailableItems = 0;

                HasInputForProcess(typeToCheck, process.InputsByType[typeToCheck], owner, process, ref hasInputs, ref noOfMissingInputTypes,
                           ref noOfAvailableInputTypes, ref currentProductionLimit, ref noOfAvailableItems, null);

                /*
                foreach (KeyValuePair<EntityType, Input> kvp in process.InputsByType)
                {
                    if (kvp.Key == typeToCheck)
                    {
                        HasInputForProcess(kvp.Key, kvp.Value, owner, process, ref hasInputs, ref noOfMissingInputTypes,
                            ref noOfAvailableInputTypes, ref currentProductionLimit, null);

                        break;
                    }
                }*/
            }
            else
            {
                noOfAvailableItems = null;
                noOfMissingInputTypes = null;
                noOfAvailableInputTypes = null;
            }


            if ((noOfMissingInputTypes.HasValue && noOfMissingInputTypes > 0))
            {
                maxAmountThatCanBeProduced = 0;
            }
            else if (currentProductionLimit.HasValue)
            {
                maxAmountThatCanBeProduced = currentProductionLimit.Value;
            }

            return hasInputs;
        }



        /// <summary>
        /// Returns 0 if owner/group is null.
        /// </summary>
        /// <param name="kvp"></param>
        /// <param name="owner"></param>
        /// <param name="process"></param>
        /// <param name="hasInputs"></param>
        /// <param name="noOfMissingInputTypes"></param>
        /// <param name="noOfAvailableInputTypes"></param>
        /// <param name="currentProductionLimit"></param>
        /// <param name="allAvailableItems"></param>
        private static void HasInputForProcess(
            EntityType inputType, Input input,
            EntityGroup owner,
            ProcessType process,
            ref bool hasInputs,
            ref int? noOfMissingInputTypes,
            ref int? noOfAvailableInputTypes,
            ref int? currentProductionLimit,
            ref int? noOfAvailableItems,
            Dictionary<EntityType, InventoryPanel.Availability> allAvailableItems = null)
        {
            int noOfItemsAvailableForProduction;
           // int noOfAvailableItems;
            int noOfItemsOrderedAsInput = 0;
            int noOfIncompleteEntities;
            int noOfEntitiesUsedAsParts;
            int noOfItemsOffSite;
            int noOfItemsOwnedByOthers;
            int noOfAvailableItemsIncludingIntrinsic;

            int productionLimitWithThisInput;


            noOfAvailableItems = GetNoOfAvailableEntities(owner.AllEntities, owner, inputType, out noOfIncompleteEntities,
                                                             out noOfEntitiesUsedAsParts, out noOfItemsOffSite, out noOfItemsOwnedByOthers, out noOfAvailableItemsIncludingIntrinsic, allAvailableItems);

            noOfItemsOrderedAsInput = GetNoOfItemsOrderedAsInput(owner, inputType, process);

            noOfItemsAvailableForProduction = noOfAvailableItems.Value - noOfItemsOrderedAsInput; // this can go under 0!?!?
            noOfItemsAvailableForProduction = Common.ClampBottom(noOfItemsAvailableForProduction, 0);


            //// how many products do we have materials for:
            productionLimitWithThisInput = noOfItemsAvailableForProduction / input.Amount.NoOfItems.Value; // todo: bulk input

            if (productionLimitWithThisInput == 0)
            {
                hasInputs = false;
                noOfMissingInputTypes++;

            }
            else
            {
                noOfAvailableInputTypes++;

                if (currentProductionLimit.HasValue)
                {
                    currentProductionLimit = Math.Min(currentProductionLimit.Value, productionLimitWithThisInput);
                }
                else
                {
                    currentProductionLimit = productionLimitWithThisInput;
                }
            }


        }

        public static bool HasAllInputsAndToolsForProcess(ProcessType processType, EntityGroup owner, Dictionary<EntityType, InventoryPanel.Availability> allAvailableItems)
        {
            bool hasInputs;
            bool hasTools;
            int maxAmountThatCanBeProduced;
            int? noOfMissingInputTypes;
            int? noOfAvailableInputTypes;
            int? outputBatchAmount;
            bool hasSkills;
            bool hasResources;
            bool hasSpecialSite;
            bool hasPolicy;
            EntityType immovableInput;

            bool canProduceNow = InventoryPanel.HasAllInputsAndToolsForProcess(processType, owner, out hasInputs, out hasTools, out maxAmountThatCanBeProduced,
                           out noOfMissingInputTypes, out noOfAvailableInputTypes, out hasSkills, out hasResources, out hasSpecialSite, out hasPolicy, out immovableInput, allAvailableItems);

            return canProduceNow;
        }

        /// <summary>
        /// Returns 0 if owner/group is null.
        /// </summary>
        /// <param name="process"></param>
        /// <param name="owner"></param>
        /// <param name="hasInputs"></param>
        /// <param name="hasTools"></param>
        /// <param name="maxAmountThatCanBeProduced"></param>
        /// <param name="noOfMissingInputTypes"></param>
        /// <param name="noOfAvailableInputTypes"></param>
        /// <param name="allAvailableItems"></param>
        /// <returns></returns>
        public static bool HasAllInputsAndToolsForProcess(
            ProcessType process,
            EntityGroup owner,
            out bool hasInputs,
            out bool hasTools,
            out int maxAmountThatCanBeProduced,
            out int? noOfMissingInputTypes,
            out int? noOfAvailableInputTypes,
            out bool hasSkills,
            out bool hasResource,
            out bool hasSpecialSite,
            out bool hasPolicy,
            out EntityType immovableInput,
            Dictionary<EntityType, InventoryPanel.Availability> allAvailableItems = null)
        {
            maxAmountThatCanBeProduced = 0;
            hasInputs = true;
            hasResource = true;
            hasSpecialSite = true;
            hasPolicy = true;

            int? currentProductionLimit = null;

            SkillType skillType = process.RequiredSkillType;
            hasSkills = owner.GetExpedition().HasSkill(skillType);

            immovableInput = process.ImmovableInput();

            // see if all inputs are there - disregarding assignment etc.
            if (process.InputsByType != null)
            {
                noOfMissingInputTypes = 0;
                noOfAvailableInputTypes = 0;

                int? noOfAvailableItems = null;
                foreach (KeyValuePair<EntityType, Input> kvp in process.InputsByType)
                {
                    if (kvp.Key.TreeType != null)
                    {
                        continue; // tree crops list the tree as input... just ignore it.
                    }

                    HasInputForProcess(kvp.Key, kvp.Value, owner, process, ref hasInputs, ref noOfMissingInputTypes,
                         ref noOfAvailableInputTypes, ref currentProductionLimit, ref noOfAvailableItems, allAvailableItems);
                }
            }
            else
            {
                noOfMissingInputTypes = null;
                noOfAvailableInputTypes = null;
            }

            TierOrAreaType policy;
             Expedition expedition = owner.Parent as Expedition;
             if (expedition != null && !expedition.Policy.CanUseProcess(process, out policy))
             {
                 hasPolicy = false;
             }

            if (process.ResourceTypeInput != null)
            {
                if (!InventorySettings.HasResources(process))
                {
                    hasResource = false;
                }
            }

            if (process.ActingOnType != null)
            {
                if (!InventorySettings.HasSpecialSite(process))
                {
                    hasSpecialSite = false;                    
                }
            }

            hasTools = HasToolsForProcess(process, owner);

            bool canProduce = hasSkills && hasInputs && hasTools && hasResource && hasSpecialSite && hasPolicy; 

            if (!canProduce) // //!hasSkills || !hasTools || (noOfMissingInputTypes.HasValue && noOfMissingInputTypes > 0)) 
            {
                maxAmountThatCanBeProduced = 0;
            }
            else if (currentProductionLimit.HasValue)
            {
                maxAmountThatCanBeProduced = currentProductionLimit.Value;
            }
            else //if (!process.IsPartOfProductionChainButCannotOrderFromInventory()) // NEW! #MULTIPLEPROCESSES
            {
                if (process.ActingOnType != null)
                {
                    maxAmountThatCanBeProduced = 1; // perhaps we could count the special sites/clay deposits/turnip shells?
                }
                else
                {
                    // we can produce with no limit (make clay from clay pit etc)                   
                    maxAmountThatCanBeProduced = 20; 
                }
            }           

            return canProduce;    
        }

        /// <summary>
        /// returns true if the expedition has all the needed tools for this process.
        /// NEW: checks if the tool is Completed!
        /// </summary>
        /// <param name="process"></param>
        /// <returns></returns>
        public static bool HasToolsForProcess(ProcessType process, EntityGroup owner) // Expedition expedition)
        {
            if (process.ProcessToolSet != null)
            {
                //List<Entity> items;
                bool toolWasFound = false;

                foreach (var toolAlternatives in process.ProcessToolSet.Tools)
                {
                    foreach (var tool in toolAlternatives.Tools)
                    {
                        foreach (var toolEntityType in tool.ToolEntityTypes)
                        {
                            if (HasToolOfType(owner, toolEntityType))
                            {
                                toolWasFound = true;
                                break;
                            }
                        }

                    }

                    if (!toolWasFound)
                    {
                        return false;
                    }
                    else
                    { // continue...
                        toolWasFound = false;
                    }

                }
            }

            return true;
        }

        /// <summary>
        /// if owner is null, will return false.
        /// 
        /// NEW: checks if the tool is Completed!
        /// 
        /// this does not check the state of the items - broken, used as parts etc....
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="toolType"></param>
        /// <returns></returns>
        private static bool HasToolOfType(EntityGroup owner, EntityType toolType)
        {
            if (owner == null)
                return false;

            List<EntityID> entities;



            SharedKnowledge sharedKnowledge = owner.GetAllegiance().SharedKnowledge;

            if (toolType.ItemType != null)
            {
                if (owner.Items.TryGetValue(toolType, out entities))
                {
                    return HasValidTool(owner, entities, sharedKnowledge);

                    /*if (entities.Count > 0)
                    {
                        return true;
                    }*/
                }
            }
            else if (toolType.StructureType != null)
            { // structure tools

                if (owner.Structures.TryGetValue(toolType, out entities))
                {
                    return HasValidTool(owner, entities, sharedKnowledge);

                    /*
                    if (entities.Count > 0)
                    {
                        return true;
                    }*/
                }
            }

            return false;
        }

        /// <summary>
        /// should resemble EvaluateJob.IsValidTool...
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="entities"></param>
        /// <param name="sharedKnowledge"></param>
        /// <returns></returns>
        private static bool HasValidTool(EntityGroup owner, List<EntityID> entities, SharedKnowledge sharedKnowledge)
        {
            for (int i = entities.Count - 1; i >= 0; i--)
            {
                IKnownEntityData entityData;
                if (GoalEvaluator.HandleOwnerDataResult(sharedKnowledge, entities[i], owner, out entityData))
                {
                    bool isIntrinsic = entityData.EntityType.IsIntrinsic();

                    if (entityData.IsCompleted() // OLD
                        && GoalEvaluator.IsOnPlaySite(entityData) // NEW
                        && (isIntrinsic || entityData.PartOfID == null)) // NEW
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static int GetNoOfItemsOrderedAsInput(EntityGroup owner, EntityType itemType, ProcessType currentProcess)
        {
            int total = 0;

          //  Expedition expedition = owner.Parent as Expedition;

            List<ProcessType> processes;
            if (GameData.Instance.ProcessesUsingThisInput.TryGetValue(itemType, out processes))
            {

                // find other processes that require this input:
                foreach (var process in processes)
                {
                    if (currentProcess != process)
                    {
                        if (process.HasOutput)
                        {
                            foreach (var output in process.Outputs)
                            {
                                ProductionOrder target;
                                if (output.IsWasteProduct == false // waste products are never ordered by the player.
                                    && owner.ProductionOrders.Orders.TryGetValue(output.FinalEntityTypeToCreate, out target)
                                    && target.ProductionJobsToComplete.HasValue)
                                {
                                    int orderedOutput = target.ProductionJobsToComplete.Value;
                                    if (orderedOutput > 0)
                                    {
                                        int? requiredInputs = process.InputsByType[itemType].Amount.NoOfItems;

                                        if (requiredInputs.HasValue)
                                        {
                                            total += orderedOutput * requiredInputs.Value;
                                        }
                                    }
                                }
                            }
                        }
                    }

                }
            }

            return total;
        }

        private TextButton latestCheckedTextButton = null;

        private void tbItems_Click(UIComponent sender, EventArgs e)
        {
            //????
            if (latestCheckedTextButton != null)
            {
                latestCheckedTextButton.IsChecked = false;
            }
            latestCheckedTextButton = (sender as TextButton);

            // open the items panel
            if ((bool)latestCheckedTextButton.Tag1 == true) // hack??? see StockButton
            {
                StockButton stockButton = sender as StockButton;

                The.InGameUI.EntityListWindow.SetDataSource(stockButton.EntityType, stockButton.EntityList); // expedition.OwnedEntities.AllEntities[entityType]);
                //expedition.OwnedEntities.ID, null);

                The.InGameUI.EntityListWindow.OpenNextToStockButton(sender);
                //The.InGameUI.EntityListWindow.ShowOnPlayfield(sender.AbsolutePosition.X + 26, sender.AbsolutePosition.Y);
            }

        }

        
        void salvage_Click(UIComponent sender, EventArgs e)
        {



        }

        void keepInStore_CountChanged(int newCount, EventArgs e)
        {
            expedition.OwnedEntities.ProductionOrders.Orders[((ItemTypeButtonEventArgs)e).Item].AmountToKeepInStore = newCount;
            //game.MinDesiredItemsInStock[((ItemTypeButtonEventArgs)e).Item] = newCount;
        }

      
        void ExpandStockItem_OnPress(object sender, EventArgs e)
        {
            EntityType itemType = (EntityType)((ItemTypeButtonEventArgs)e).Item;
        }

        #endregion

        #region commented methods


        /*   void fillableBar_SliderMouseDown(object sender, EventArgs e)
           {
               // don't refresh the slider posiiton once the user has touched it:
               //   SetUserChangedSliderState((FillableBar)sender); //e);

               SetUserChangedSliderState((FillableBar)sender); //e);

           }*/

        /*  private void SetUserChangedSliderState(FillableBar control) //EventArgs e)
          {
              if (!userChangedData.ContainsKey(control))
              {
                  userChangedData.Add(control, true);
              }
          }*/

        /// <summary>
        /// we must either own the item or have at least one input that we own before it appears in the list
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        /*  public static bool EntityTypeIsAvailableForProduction(EntityType entityType, Owner owner, int noOfOwnedItems)
          {
              if (noOfOwnedItems == 0)
              {
                  List<ProcessType> listOfProcesses;
                  if (GameData.Instance.ProcessYieldsThisOutput.TryGetValue(entityType, out listOfProcesses))
                  {
                      foreach (var process in listOfProcesses)
                      {
                          if (process.InputsByType != null)
                          {
                              foreach (var input in process.InputsByType)
                              {
                                  if (GetNoOfAvailableItems(
                                      owner, input.Key) > 0)
                                  {
                                      return true;
                                  }
                              }
                          }

                      }
                  }

                  return false;
              }
              else return true;

          }*/


        /*   private void SetListHeight()
           {
               grid.Height = display.Height - 20;
              // list.Height = display.Height - 20;
           }*/

        /*  private void UpdateStocks()
          {
              UIComponent category, item;

              CollapsablePanel cpCategory;
              Grid grdItems;

              foreach (KeyValuePair<EntityType, List<EntityID>> kvp in expedition.ExpeditionOwner.InternalOwner.OwnerContent.Items)
              {
                  if (grid.TryGetItem(kvp.Key.Category, out category))
                  {
                      cpCategory = category as CollapsablePanel;
                      grdItems = (Grid)cpCategory.ExpandedPanel.Controls[0];

                      if (grdItems.TryGetItem(kvp.Key, out item))
                      {
                          foreach (UIComponent child in item.Controls)
                          {
                              if (child.ColumnID == "inStore") // XXXXX!
                              {
                                  ((Label)child).Text = expedition.ExpeditionOwner.InternalOwner.OwnerContent.Items[kvp.Key].Count.ToString();
                              }
                              else if (child.ColumnID == "keepInStore") // XXXXX!
                              {
                                  ((Spinner)child).Count = expedition.Stocks.Targets[kvp.Key].KeepInStore;
                              }
                              else if (child.ColumnID == "productionTarget") // XXXXX!
                              {
                                  ((Spinner)child).Count = expedition.Stocks.Targets[kvp.Key].ProductionTarget;
                              }
                          }
                      }
                      else
                      {
                          // add a new item to the grid:

                      }
                  }

                  //stockLabels[kvp.Key].Text = kvp.Value.Count.ToString();
              }
          }*/

        /*   private Rectangle ClampItemRect(Rectangle rect)
           {
               Rectangle
           }*/



        /*  private static Color notInStockColor = Common.ColorFromHex("#cac172"); // new Color(90, 90, 90);
          public static Color GetStockStatusColor(bool ownsItem, Color normalColor)
          {
              Color color;
              if (ownsItem)
              {
                  color = normalColor; // Color.White;
              }
              else
              {
                  color = notInStockColor;
              }

              return color;
          }*/

        /*   public static Color GetProductionStatusColor(bool hasTools, bool hasInputs, bool canProduce, Color normalColor)
           {
               Color color;
               if (canProduce)
               {
                   color = normalColor; // Color.White;
               }
               else
               {
                   if (!hasInputs && !hasTools)
                   {
                       color = The.Sim.ScreenManager.UserSettings.ProductionStatusNoToolsAndNoInputsColor;
                   }
                   else
                   {
                       color = The.Sim.ScreenManager.UserSettings.ProductionStatusNoInputsOrNoToolsColor;
                   }
               }
               return color;
           }*/



        #endregion
    }
}
