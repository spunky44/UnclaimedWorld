using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using WindowSystem;
using UWGame.SimSide.Entities;
using UWGame.ClientSide.Interface.Controls;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Snapshots;
using UWGame.ClientSide.Interface.Missions;
using UWGame.SimSide.Overland.Missions.Templates;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Allegiances;
using UWGame.ClientSide.Interface.Inventory;
using UWGame.ClientSide.Interface.LCD;
using UWGame.SimSide.AI;
using UWGame.SimSide.Tiers;
using UWGame.SimSide.Policies;
using UWGame.SimSide.Trade;

namespace UWGame.ClientSide.Interface.BuyAndSell
{
    /// <summary>
    /// meant to be a popup window / dialog... 
    /// 
    /// Buy from: expedition (terminal), private person...
    /// Prices are defined in a TradeManager in the EntityGroup. Wares are retrieved via callback (to faciliate terminals)
    /// 
    /// Don't reference The.IngameUI or UIAllegiance, since we want to use the class on the scenario screen also.
    /// 
    /// creates its own LCDScreen
    /// 
    /// to promote reuse (like for a loadout screen), keep context-specific details out of this class, please
    /// 
    /// </summary>
    public class BuySellPanel : Panel
    {

        public enum BuySellDialogMode { 
            ViewSellAtNPC, // world map viewing what the NPC is willing to buy
            ViewBuyAtNPC,  // world map viewing what the NPC has for sale
            ActionSellAtPlayer, // mission action sell at player expedition
            ActionBuyAtNPC // mission action buy at NPC
        } 

        BuySellDialogMode mode;

        Grid grdCategoryView, grdListView;
        LCDInnerPanel lcdMessagePanel;
        LCDScreen lcdScreen;
        UIComponent lcdSurface;
        Box display; //, edges;
        //const int ItemHeight = Inventorypa 22;

        public event EventHandler OKClick;
        public event EventHandler CancelClick;


        int itemTypeIconColumnX = 40;
        int captionX = 70;
        int quantityX = 235;
        int sliderX = 242;
        int offerDemandX = 384;
        int priceX = 443; // 397; 
        int bulkX = 512; // 462; 

        private Dictionary<EntityType, InventoryPanel.Availability> allAvailableItems = new Dictionary<EntityType, InventoryPanel.Availability>();

        ImageButton ibCategory, ibList;

        Color unattainableColor = GameData.Instance.GUIConstants.UnattainableColor; // decouple? Common.ColorFromHex("#c6000e"); // 

        Icon backgroundTint;

        #region Data sources

        Dictionary<EntityType, List<EntityID>> currentOrders;

        /// <summary>
        /// Action is from the player's viewpoint
        /// </summary>
       // CargoActionTypes cargoActionType;
        CargoActionTypes CargoActionType
        {
            get
            {
                switch(mode)
                {
                    case BuySellDialogMode.ActionBuyAtNPC:
                    case BuySellDialogMode.ViewBuyAtNPC:
                        return CargoActionTypes.Buy;
                    case BuySellDialogMode.ActionSellAtPlayer:
                    case BuySellDialogMode.ViewSellAtNPC:
                        return CargoActionTypes.Sell;

                    default: return CargoActionTypes.Buy;
                }
            }
        }

        BuySellPanelSettings settings;

        bool AllowOrders
        {
            get
            {
                if (mode == BuySellDialogMode.ActionBuyAtNPC || mode == BuySellDialogMode.ActionSellAtPlayer)
                {
                    return true;
                }

                return false;
            }
        }

      
        EntityGroupID? ownerOfItemsID;

        /// <summary>
        /// cane be the player or an npc
        /// </summary>
        EntityGroupID? buyerID;

        /// <summary>
        /// to get NPC buy prices when player is selling
        /// </summary>
      //  EntityGroupID? npcBuyerID;

      
        /// <summary>
        /// callback delegate for GetItems!
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        public delegate V Func<T, U, V>(T input, out U output);
  // delegate List<EntityID> GetItems<EntityType,bool,List<EntityID>>(EntityType input, out bool output);

        /// <summary>
        /// to get items, decouple from SharedKnowledge for main menu use
        /// </summary>
        Func<EntityType, bool, List<EntityID>> getItems;


        public delegate bool CanTradeDelegate(EntityType entityType, out TierOrAreaType unavailablePolicy);
        //   private bool CanTrade(EntityType entityType, out TierArea unavailablePolicy)

        private CanTradeDelegate CanTrade;

        #endregion

        UIComponent sortingButtonsContainer;

        //  public Dictionary<EntityType, int> Orders
        public Dictionary<EntityType, List<EntityID>> Orders
        {
            get
            {
                return currentOrders;
            }
        }

        /// <summary>
        /// use this to avoid changing the user's setting during update after he has grabbed the slider (but not released it yet)
        /// </summary>
        FillableBar sliderBeingDragged = null;

        const int gridTopMargin = 50;
        const int gridBottomMargin = 50;

        Label lblNoWaresNote;

        Label lblTotalItemCost;
        Label lblTotalBulk;

        FilterPropertiesPanel filterPropertiesPanel;
        LCDInnerPanel filterPanelContainer;

        LCDInnerPanel totalsPanel;

        SortingButtons<BuySellPanelSettings.SortColumns> sortingButtons;
        //UIComponent sortingButtonsContainer;

        TextButton btOK, btCancel;

        TextButton tbExpand;

       
        public BuySellPanel(CommonInterface intf, Point position) :
            base(intf, "", position, new Vector2(614 /* 564*/, 580 /* 560*/), Level.StackedDialogs)
        {

            /*  FullLCDPanel.AddLCDPanelFitWindowWithBottomMargin(The.InGameUI, Form, 35, 
                  new Point(MarginX, MarginX), out display, out lcdSurface, ref lcdScreen);
              */

            RosterPanel.CreateRosterStyleLCDPanel(intf, Window, out display, out lcdSurface, ref lcdScreen, addBottomEdgeDirt: false);

            /*lblHeader = new Label(Interface.gui) { Position = new Point(0, 0) };
            lcdSurface.Add(lblHeader);
            lblHeader.Init(Label.LabelType.LCDBigHeaderBanner);
            lblHeader.Width = lcdSurface.Width;
            */


            filterPanelContainer = new LCDInnerPanel(Interface.gui, lcdSurface.Width, false);
            filterPanelContainer.HorizontalContentPadding = 0;
            filterPanelContainer.VerticalContentPadding = 5;
            filterPanelContainer.ContentHeight = 150;
            lcdSurface.Add(filterPanelContainer.Panel);

            filterPropertiesPanel = new FilterPropertiesPanel(Interface.gui, true); //, filterAndTrackingPanel.ContentWidth);          
            filterPropertiesPanel.FiltersChanged += filterPropertiesPanel_FiltersChanged;
            filterPanelContainer.AddContentSetFullWidth(filterPropertiesPanel);
            filterPropertiesPanel.Y = 2;

            tbExpand = new TextButton(Interface.gui);
            lcdSurface.Add(tbExpand);
            tbExpand.Init(TextButton.TextButtonType.LCD);
            tbExpand.Text = "MORE";
            tbExpand.X = 2;
            tbExpand.Y = 2;
            tbExpand.Click += Expand_Click;
            tbExpand.ScaleToFitText();
            // tbExpand.MinHeight = 25;
            tbExpand.Height = 30;

            sortingButtonsContainer = new UIComponent(Interface.gui);
            lcdSurface.Add(sortingButtonsContainer);
            sortingButtonsContainer.Width = lcdSurface.Width;
            sortingButtonsContainer.Height = 50;
            sortingButtonsContainer.Position = new Point(0, 160);

            sortingButtons = new SortingButtons<BuySellPanelSettings.SortColumns>(Interface.gui);
            sortingButtons.SortClicked += sortingButtons_SortClicked;
            sortingButtons.Position = new Point(56, 0);
            sortingButtons.Width = lcdSurface.Width - sortingButtons.X;
            sortingButtonsContainer.Add(sortingButtons);


            /*  CreateColumnHeadings(lcdSurface, 0, new Tuple<string, int>("NAME", captionX),
                                                         new Tuple<string, int>("ORDERED/AVAILABLE", sliderX - 30),
                                                         new Tuple<string, int>("PRICE", priceX),
                                                         new Tuple<string, int>("WEIGHT", bulkX));
              */

            /* lcdMessagePanel = new LCDInnerPanel(Interface.gui, 214, true, 1f);
             lcdSurface.Add(lcdMessagePanel.Panel);
             // surfaceGrid.AddEntry(selectionPanel.Panel, selectionPanel.Panel);
             lcdMessagePanel.ContentHeight = 30;
             lcdMessagePanel.Panel.Y = lcdSurface.Height - 35; // 24; 
             lcdMessagePanel.Panel.X = 270;
             lcdMessagePanel.VerticalContentPadding = 8;*/

            totalsPanel = new LCDInnerPanel(Interface.gui, lcdSurface.Width, false);
            totalsPanel.HorizontalContentPadding = 0;
            totalsPanel.VerticalContentPadding = 5;
            totalsPanel.ContentHeight = 26;
            lcdSurface.Add(totalsPanel.Panel);
            totalsPanel.Panel.Y = lcdSurface.Height - totalsPanel.Height;
            totalsPanel.TintPanel(Color.Cornsilk);


            grdCategoryView = new Grid(Interface.gui, ListBoxType.LCD, Label.LabelType.LCDNormal);
            grdCategoryView.FixedItemHeights = false;
            grdCategoryView.RenderType = RenderType.CRTAndLCD;
            lcdSurface.Add(grdCategoryView);
            grdCategoryView.Font = GUIManager.LCDandHUDBodyFontPath; // Important! Must be set after Add! Add will reinitialize with defaults.
            grdCategoryView.Width = lcdSurface.Width;
            grdCategoryView.Height = lcdSurface.Height - gridTopMargin - gridBottomMargin - 30 /*- lcdMessagePanel.Panel.Height*/;
            grdCategoryView.ItemHeight = InventoryPanel.ItemHeight;
            grdCategoryView.Position = new Point(0, gridTopMargin);


            grdListView = new Grid(Interface.gui, ListBoxType.LCD, Label.LabelType.LCDNormal);
            grdListView.FixedItemHeights = true;
            grdListView.RenderType = RenderType.CRTAndLCD;
            lcdSurface.Add(grdListView);
            grdListView.Font = GUIManager.LCDandHUDBodyFontPath; // Important! Must be set after Add! Add will reinitialize with defaults.
            grdListView.Width = lcdSurface.Width;
            grdListView.Height = lcdSurface.Height - gridTopMargin - gridBottomMargin - 30 /*- lcdMessagePanel.Panel.Height*/;
            grdListView.ItemHeight = InventoryPanel.ItemHeight;
            grdListView.Position = new Point(0, gridTopMargin);


            lblNoWaresNote = new Label(Interface.gui);
            lblNoWaresNote.Init(Label.LabelType.LCDNormal);
            //lblNoWaresNote.Text = "Nothing is available to buy.";
            lcdSurface.Add(lblNoWaresNote);
            lblNoWaresNote.Y = gridTopMargin; // hidden under the grid until it is needed.
            lblNoWaresNote.X = lcdSideMargin;
            // lblNoWaresNote.FitToText();
            lblNoWaresNote.Visible = false;





            Label lblTotalHeading = new Label(Interface.gui);
            totalsPanel.AddContent(lblTotalHeading);
            lblTotalHeading.Init(Label.LabelType.LCDSmallHeadingBanner);
            lblTotalHeading.Text = "TOTAL:";
            lblTotalHeading.Y = 8; // lcdSurface.Bottom - gridBottomMargin - 30;
            lblTotalHeading.X = 280;
            lblTotalHeading.FitToText();


            lblTotalBulk = new Label(Interface.gui);
            totalsPanel.AddContent(lblTotalBulk);
            lblTotalBulk.Init(Label.LabelType.LCDNormal);
            // lblTotalBulk.Text = entityType.ItemType.MaximumBulk.HasValue ? entityType.ItemType.MaximumBulk.Value.ToString("N") : "";
            lblTotalBulk.X = bulkX;
            lblTotalBulk.Y = lblTotalHeading.Y;


            lblTotalItemCost = new Label(Interface.gui);
            totalsPanel.AddContent(lblTotalItemCost);
            lblTotalItemCost.Init(Label.LabelType.LCDNormal);
            // lbl.Text = entityType.ItemType.MaximumBulk.HasValue ? entityType.ItemType.MaximumBulk.Value.ToString("N") : "";
            lblTotalItemCost.AlignRight(priceX);
            lblTotalItemCost.Y = lblTotalBulk.Y;

            Icon imCoin = Panel.AddImage(Interface.gui, totalsPanel.Panel, "lcd_icon_credits_oneCoin", new Point(lblTotalItemCost.Right + 6, lblTotalItemCost.Y + 2));
            imCoin.SetSkinLocation(SkinState.Normal,null, Label.LCDNormal, Label.LCDNormal);

            btOK = AddLowerButton("OK", "Accepts the order and closes the dialog.", Align.Left);
            btOK.Click += new ClickHandler(btOK_Click);

            btCancel = AddLowerButton("CANCEL", "Cancels and closes the dialog.", Align.Right);
            btCancel.Click += new ClickHandler(btCancel_Click);

            CreateGridHeaderButtons();

            RosterPanel.AddTintedBackground(ref backgroundTint, display);

            //  LoadUserSettings();

            // SetView(ViewType.Categories);

            //ShowRelevantGrid();
        }

        private void LoadUserSettings()
        {
            //BuySellPanelSettings settings = GetSettings();

            filterPropertiesPanel.Fill(settings.FilterPropertySettings);

            sortingButtons.Fill(settings.SortingSettings);

            SetView(settings.viewType);

            ExpandOrCollapseTopPanel(settings.IsExpanded);

        }


        private void Expand_Click(UIComponent sender, EventArgs e)
        {
            //  ImageButton btExpand = sender as ImageButton;
            TextButton tbExpand = sender as TextButton;

           // BuySellPanelSettings settings = GetSettings();

            bool expand = !settings.IsExpanded;
            ExpandOrCollapseTopPanel(expand);
        }

        void sortingButtons_SortClicked()
        {
            Populate();
        }

        void filterPropertiesPanel_FiltersChanged()
        {
            Refresh();
        }


        private void ExpandOrCollapseTopPanel(bool expand)
        {

            if (expand)
            {
                SetMinimizedOrExpandedContentProperties(85 /* 76*/, true, "LESS", InventoryPanel.collapseFilterTooltip);
            }
            else
            {
                SetMinimizedOrExpandedContentProperties(26, false, "MORE", InventoryPanel.expandFilterTooltip);
            }

           // BuySellPanelSettings settings = GetSettings();
            settings.IsExpanded = expand;
        }

        private void SetMinimizedOrExpandedContentProperties(int panelheight, bool visible, string text, string btToolTip)
        {
            tbExpand.ToolTip = btToolTip;
            tbExpand.Text = text;

            UpdateGridYPosition(panelheight);


            if (visible)
            {
                filterPanelContainer.AddContentSetFullWidth(filterPropertiesPanel);
            }
            else
            {
                filterPanelContainer.RemoveContent(filterPropertiesPanel);
            }
        }

        private void CreateGridHeaderButtons()
        {
            ibList = new ImageButton(Interface.gui);
            ibList.Width = 30;
            sortingButtonsContainer.Add(ibList);
            ibList.Width = 30;
            ibList.InitWithIcon(ImageButtonType.LCD, "basic_icon_list", true);
            ibList.Click += tbListView_Click;
            ibList.Y = 0;
            ibList.X = 0;
            ibList.ToolTip = "List view";
            ibList.Width = 30;
            ibList.Height = 30;
            ibList.RecalculateIconPosition();
            // ibList.MouseOut += ibList_MouseOut;


            ibCategory = new ImageButton(Interface.gui);
            ibCategory.Width = 30;
            sortingButtonsContainer.Add(ibCategory);
            ibCategory.Width = 30;
            ibCategory.InitWithIcon(ImageButtonType.LCD, "basic_icon_category", true);
            ibCategory.Click += tbCategoryView_Click;
            ibCategory.ToolTip = "Category view";
            ibCategory.Y = 0;
            ibCategory.X = 30;
            ibCategory.IsChecked = true;
            ibCategory.Width = 30;
            ibCategory.Height = 30;
            ibCategory.RecalculateIconPosition();
            // ibCategory.MouseOut += ibList_MouseOut;

            // tbListView_Click(ibList, null);

            sortingButtons.CreateTextButton(0, 185, "NAME", BuySellPanelSettings.SortColumns.Name);
            sortingButtons.CreateTextButton(179, 99, "AMOUNT", BuySellPanelSettings.SortColumns.Amount);
            sortingButtons.CreateTextButton(277, 62, "", BuySellPanelSettings.SortColumns.OfferDemand, "OFFER/DEMAND");
            sortingButtons.CreateTextButton(336, 62, "", BuySellPanelSettings.SortColumns.Price, "PRICE");
            sortingButtons.CreateTextButton(397, 90, "BULK", BuySellPanelSettings.SortColumns.Bulk);

            /*sortingButtons.CreateImageButton(282, 62, "PRICE", BuySellPanelSettings.SortColumns.Price);
            sortingButtons.CreateTextButton(347, 90, "BULK", BuySellPanelSettings.SortColumns.Bulk);
            */
        }


        private void UpdateGridYPosition(int filterPanelHeight)
        {
            filterPanelContainer.ContentHeight = filterPanelHeight;//97

            int grdSortingButtonsY = filterPanelContainer.Panel.Bottom + 2;
            sortingButtonsContainer.Y = grdSortingButtonsY;

            grdListView.Y = sortingButtonsContainer.Bottom - 15;
            grdCategoryView.Y = sortingButtonsContainer.Bottom - 15;

            int gridHeight = lcdSurface.Height - grdCategoryView.Y - totalsPanel.Height;
            //  grdListView.Height = lcdSurface.Height - grdSortingButtons.Bottom + 15;
            grdCategoryView.Height = gridHeight; // lcdSurface.Height - grdCategoryView.Y; // sortingButtonsContainer.Bottom + 15;
            grdListView.Height = gridHeight;
        }



        private void SetView(ViewType viewTypeToSet)
        {
           // var settings = GetSettings();
            settings.viewType = viewTypeToSet;


            if (settings.viewType == ViewType.List)
            {
                lcdSurface.Remove(grdCategoryView);
                lcdSurface.Add(grdListView);

                ibCategory.IsChecked = false;
                ibList.IsChecked = true;
            }
            else
            {
                lcdSurface.Remove(grdListView);
                lcdSurface.Add(grdCategoryView);

                ibCategory.IsChecked = true;
                ibList.IsChecked = false;
            }
        }

        private void tbListView_Click(UIComponent sender, EventArgs e)
        {
            SetView(ViewType.List);

            /*
            if (viewType != ViewType.List)
            {*/
            Populate();
            // }
        }

        private void tbCategoryView_Click(UIComponent sender, EventArgs e)
        {
            SetView(ViewType.Categories);

            /* if (viewType != ViewType.Categories)
             {*/
            Populate();
            // }
        }

        void btOK_Click(UIComponent sender, EventArgs e)
        {
            List<Grid> categoryGrid = new List<Grid>();

            /* Dictionary<EntityType, List<EntityID>> data;
             if (!GetData(out data))
             {
                 return;
             }*/

            EntityGroup ownerOfItems, npcBuyer;
            Expedition expeditionOwner;

            if (!ResolveEntityGroup(out ownerOfItems, out expeditionOwner, out npcBuyer))
            {
                return;
            }

            bool isUIOwned = GetIsPlayerOwnedLocation(ownerOfItems);


            List<EntityID> availableItems;
            int available, demandedItems;
            bool sourceIsInvalid;
            decimal? price;
            if (currentOrders != null)
            {
                // validate...
                foreach (var item in currentOrders)
                {
                    price = GetAgreedPrice(ownerOfItems, npcBuyer, item.Key); 

                    // #TRADECRASH - this call crashes sometimes?
                    if (GetTradeItems(ownerOfItems, npcBuyer, isUIOwned, item.Key, out availableItems, price, out available, out demandedItems, out sourceIsInvalid))
                    {
                        ValidateAndCountOrders(item.Key, availableItems);
                    }
                    else
                    {
                        if (sourceIsInvalid)
                        {
                            return;
                        }
                    }
                }

                // cleanup:
                List<EntityType> keysToRemove = null;
                foreach (var item in currentOrders)
                {
                    if (item.Value.Count == 0)
                    {
                        Common.AddToList(ref keysToRemove, item.Key);
                    }
                }
                if (keysToRemove != null)
                {
                    foreach (var item in keysToRemove)
                    {
                        currentOrders.Remove(item);
                    }
                }

            }

            /*
            foreach (var category in outerGrid.EntriesByKey)
            {
                categoryGrid.Clear();
                category.Value.FindChildOfType<Grid>(null, ref categoryGrid); // there is only one

                foreach (var item in categoryGrid[0].EntriesByKey)
                {
                    FillableBar fillableBar = (FillableBar)item.Value.FindChildById(UIComponent.DataControlID.CurrentOrders);

                    if (fillableBar.Value != 0)
                    {                        
                        Common.AddToDictionary(ref currentOrders, (EntityType)item.Key, fillableBar.Value);
                    }
                }
            }*/

            if (this.OKClick != null)
                OKClick.Invoke(this, null);

            Hide();

        }



        public static string GetHeading(BuySellDialogMode mode, EntityGroup buyer, EntityGroup siteOwner) //  Allegiance allegiance)
        {
            string text = "";

            switch(mode)
            {
                case BuySellDialogMode.ViewSellAtNPC:
                    text = "Willing to buy at ";
                    text += buyer.GetAllegiance().Name;

                    break;
                case BuySellDialogMode.ViewBuyAtNPC:                   
                    text = "For sale at ";
                    text += siteOwner.GetAllegiance().Name;

                    break;

                case BuySellDialogMode.ActionSellAtPlayer:
                    text = "Sell at ";
                    text += siteOwner.GetAllegiance().Name;

                    break;
                case BuySellDialogMode.ActionBuyAtNPC:
                    text = "For sale at ";
                    text += siteOwner.GetAllegiance().Name;


                    break;
            }

            return text;

        }

        /*
        public static string GetLoadUnloadHeading(CargoActionTypes action, Allegiance allegiance)
        {
            string text = "";
            switch (action)
            {
                case CargoActionTypes.Buy:
                    text = "For sale at "; //"Buy at ";
                    break;
                case CargoActionTypes.Sell:
                    if (allegiance == The.InGameUI.UIAllegiance)
                    {
                        text = "Sell at ";
                    }
                    else
                    {
                        text = "Willing to buy at "; 
                    }
                    break;
                case CargoActionTypes.Load:
                    text = "Load at ";
                    break;
            }

            text += allegiance.Name;

            return text;

        }*/

      /*  public void FillAndShowDialog(Point absolutePosition, CargoActionTypes selectedAction, bool allowOrders,
            Dictionary<EntityType, List<EntityID>> currentOrders,
            Allegiance allegiance, Expedition expedition, Func<EntityType, bool, List<EntityID>> getItems)
        {
            // show the buy/load dialog
            string headingText = GetLoadUnloadHeading(selectedAction, allegiance);


            BuySellPanelSettings settings;
            if (selectedAction == CargoActionTypes.Buy)
            {
                settings = The.InGameUI.BuySettings;
            }
            else
            {
                settings = The.InGameUI.SellSettings;
            }

            Fill(headingText, getItems, expedition.OwnedEntities.ID, currentOrders, selectedAction, allowOrders, settings);

            // center dialog over the click source:
            ShowInScreenSpace(absolutePosition.X - Window.Width / 2, absolutePosition.Y / 2, false);


        }*/


        /// <summary>
        /// don't reference InGamUI here! We want to Fill from the main menu too...
        /// 
        /// Cases:
        /// 1. 
        /// View Willing to buy from World Map
        /// Action = Sell
        /// Buyer = NPC
        /// Owner = null
        /// 
        /// 2.
        /// View For Sale from World map
        /// Action = Buy
        /// Buyer = Player
        /// Owner = NPC
        /// 
        /// 3. 
        /// Sell action
        /// Action = Sell
        /// Buyer = NPC
        /// Owner = Player
        /// 
        /// 4.
        /// Buy action
        /// Action = Buy
        /// Buyer = Player
        /// Owner = NPC
        /// 
        /// 
        /// </summary>
        /// <param name="heading"></param>
        /// <param name="getItems"></param>
        /// <param name="ownerID"></param>
        /// <param name="currentOrders"></param>
        /// <param name="cargoActionType"></param>
        /// <param name="allowOrders"></param>
        /// <param name="settings"></param>
        public void Fill(//string heading,
            BuySellDialogMode mode,
            Func<EntityType, bool, List<EntityID>> getItems,
            CanTradeDelegate canTrade,
            EntityGroupID? ownerID,
            EntityGroupID? buyerID, // EntityGroupID? npcBuyerID,
            Dictionary<EntityType, List<EntityID>> currentOrders,
           /* CargoActionTypes cargoActionType,
            bool allowOrders,*/
            BuySellPanelSettings settings)
        {
            this.mode = mode;

           
            this.getItems = getItems;
            this.CanTrade = canTrade;

            //this.orderedItemIsValid = orderedItemIsValid;
            this.ownerOfItemsID = ownerID;
            this.buyerID = buyerID;

            this.currentOrders = currentOrders;


           /* this.cargoActionType = cargoActionType;
            this.allowOrders = allowOrders;*/

            if (this.AllowOrders)
            {
                lcdSurface.Add(totalsPanel.Panel);
                btOK.Visible = true;
            }
            else
            {
                lcdSurface.Remove(totalsPanel.Panel);
                btOK.Visible = false;
            }

            this.settings = settings;

            LoadUserSettings();

            EntityGroup owner, buyer;
            Expedition expeditionOwner;
            if (!ResolveEntityGroup(out owner, out expeditionOwner, out buyer))
            {
                return;
            }
            bool isPlayerOwned = GetIsPlayerOwnedLocation(owner);

            // show the buy/load dialog
            string heading = GetHeading(mode, buyer, owner); // selectedAction, allegiance);
            lblTitle.Text = heading;


            if (CargoActionType == CargoActionTypes.Sell)
            {               
                sortingButtons.EnableButton(BuySellPanelSettings.SortColumns.OfferDemand, isPlayerOwned);

                Color tint = GameData.Instance.GUIConstants.SellingTint;
                tint.A = 160; 

                RosterPanel.SetBackgroundTint(tint, backgroundTint);
            }
            else if (CargoActionType == CargoActionTypes.Buy)
            {
                sortingButtons.EnableButton(BuySellPanelSettings.SortColumns.OfferDemand, false);

                Color tint = GameData.Instance.GUIConstants.BuyingTint;
                tint.A = 160;

                RosterPanel.SetBackgroundTint(tint, backgroundTint);
            }
            else
            {
                sortingButtons.EnableButton(BuySellPanelSettings.SortColumns.OfferDemand, false);

                RosterPanel.SetBackgroundTint(null, backgroundTint);       
            }
        }




        public override void Refresh()
        {
            Populate();

            base.Refresh();
        }

        /*  public override void Show()
          {
              if (typ)
              filterPropertiesPanel.Fill(The.InGameUI.InventorySettings.FilterPropertySettings);


              base.Show();
          }*/


        private void HandleDestroyedEntityGroupOrDataSource()
        {
            // TODO: show a popup message to the user.
            The.InGameUI.MessageBox.ShowMessage("The terminal no longer exists. It is not possible to continue browsing the items.");

            The.InGameUI.MessageBox.OKClick += new EventHandler(MessageBoxDestroyedMission_OKClick);

            // leave the roster open beneath the modal dialog.


            // Hide();
        }

        void MessageBoxDestroyedMission_OKClick(object sender, EventArgs e)
        {
            The.InGameUI.MessageBox.OKClick -= new EventHandler(MessageBoxDestroyedMission_OKClick);


            grdCategoryView.Clear();// clear all...
            grdListView.Clear();

            CancelDialog();
        }

        /// <summary>
        /// get all priced items?
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private bool GetData(out HashSet<EntityType> data)
        {
           
           // BuySellPanelSettings settings = GetSettings();
            data = settings.GetData();

            return true;

            /*  data = this.getItems.Invoke();

              if (data == null)
              {
                  HandleDestroyedEntityGroupOrDataSource();
                  //HandleDestroyedDataSource();

                  return false;
              }

              return true;*/
        }

        /*   private bool GetData(out Dictionary<EntityType, List<EntityID>> data)
           {
               //currentListData = The.InGameUI.InventorySettings.GetData(allAvailableItems);

               data = this.getItems.Invoke();

               if (data == null)
               {
                   HandleDestroyedEntityGroupOrDataSource();
                   //HandleDestroyedDataSource();

                   return false;
               }

               return true;
           }*/

        private bool ResolveEntityGroup(out EntityGroup ownerOfItems, out Expedition ownerOfItemsExpedition, out EntityGroup buyer) 
        {
            ownerOfItems = null;
            ownerOfItemsExpedition = null;
            buyer = null;

            if (mode != BuySellDialogMode.ViewSellAtNPC)
            {
                ownerOfItems = LookUp<EntityGroup, EntityGroupID>.FindByID(ownerOfItemsID);

                if (ownerOfItems == null)
                {
                    HandleDestroyedEntityGroupOrDataSource();

                    //Hide(); // ??

                    return false;
                }

                ownerOfItemsExpedition = ownerOfItems.Parent as Expedition;

            }

            if (buyerID.HasValue)
            {
                buyer = LookUp<EntityGroup, EntityGroupID>.FindByID(buyerID);
                if (buyer == null)
                {
                    HandleDestroyedEntityGroupOrDataSource();

                    return false;
                }
            }

         
            return true;
        }



        Dictionary<EntityType, EntityType> presentItemTypes = new Dictionary<EntityType, EntityType>();
        List<Grid> categoryGrids = new List<Grid>();


        private void PopulateWithCategories(EntityGroup ownerOfItems, Expedition expedition, EntityGroup npcBuyer)
        {

            //Dictionary<EntityType, List<EntityID>> data;
            HashSet<EntityType> data;
            if (!GetData(out data))
            {
                return;
            }

            CollapsablePanel cpCategory;
            Grid categoryGrid = null;
            UIComponent categoryRow;
            UIComponent itemRow;

            presentItemTypes.Clear();
            categoryGrids.Clear();

            allAvailableItems.Clear();

            // Nested grids

            // outer level is an item "category"
            // add nested grids for each, containing item types...

            // EntityType entityType;
            EntityCategory entityCategory;


            int /*noOfAvailableItems,*/ currentOrder;
            decimal? price;

            int currentNoOfCategories = grdCategoryView.Entries.Count;

            // don't use InGameUI! Decouple!!
            // SharedKnowledge sharedKnowledge = The.InGameUI.UIAllegiance.SharedKnowledge;

            //   List<JobID> listOfActiveMissions = GetActiveMissions();

            grdCategoryView.BeginAddingEntries();

            // what should be displayed:
            /*
               1. Buy - items for sale
             * 2. Sell - items that can be bought
             * 3. Load - owned items
             * 4. Unload - ?
             */
            object key;
            List<EntityID> availableItems;

            bool isUIOwned = GetIsPlayerOwnedLocation(ownerOfItems);

            foreach (var entityType in data)
            {
                /*if (entityType.ItemType != null)
                {*/
                entityCategory = entityType.Category;
                key = entityType.Category;
                // }

                price = GetAgreedPrice(ownerOfItems, /*isUIOwned,*/ npcBuyer, entityType);
               

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

              //  price = GetNPCPrice(owner, entityType);

                int noOfAvailableItems, demandedItems;
                bool itemSourceIsInvalid = false;
                if (cpCategory == null) // add the category
                {
                    // don't add the category until we have (had) access to this item:
                    if (!GetTradeItems(ownerOfItems, npcBuyer, isUIOwned, entityType, out availableItems, price, out noOfAvailableItems, out demandedItems, out itemSourceIsInvalid))
                    {
                        if (itemSourceIsInvalid)
                        {
                            return;
                        }
                        continue;
                    }

                    AddCategoryRow(ref cpCategory, ref categoryGrid, entityCategory);

                    categoryGrid.BeginAddingEntries();
                    categoryGrids.Add(categoryGrid); // for resize at the end of update
                }

                if (GetTradeItems(ownerOfItems, npcBuyer, isUIOwned, entityType, out availableItems, price, out noOfAvailableItems, out demandedItems, out itemSourceIsInvalid))
                {
                    presentItemTypes.Add(entityType, entityType);

                    if (!categoryGrid.TryGetEntry(entityType, out itemRow))
                    {
                        itemRow = AddItemRow(categoryGrid, entityType, ownerOfItems, true, true);
                    }

                    UpdateItemRow(itemRow, entityType, ownerOfItems, isUIOwned, availableItems, noOfAvailableItems, demandedItems, price, true);

                }
                else
                {
                    if (itemSourceIsInvalid)
                    {
                        return;
                    }
                }

            }

            // remove unused item and category rows. could be difficult?              
            FullLCDPanel.Cleanup<EntityType, int, EntityCategory>(grdCategoryView, null, presentItemTypes, null);

            Grid.Sorting sortOrder = GetSortOrder();
            // sort categories if changes were made:
            InventoryPanel.DoCategorySorting(grdCategoryView, categoryGrids, sortOrder);

            grdCategoryView.EndAddingEntries();

        }


     /*   private decimal? GetAgreedPrice(EntityGroup seller, EntityGroup buyer, EntityType entityType)
        {

            price = BuySellActionTemplate.GetTradePrice(entityType, buyer.OwnedEntities, seller.OwnedEntities);



            if (!isUIOwned)
            {
                // displaying an NPC collection
                if (owner != null)
                {
                    if (cargoActionType == CargoActionTypes.Buy)
                    {
                        return (decimal?)owner.GetSellPrice(entityType);
                    }
                    else
                    {
                        return (decimal?)owner.GetBuyPrice(entityType);
                    }
                }

                return null;

            }
            else
            {
                // displaying own collection.
                // get the buyer price:
                if (npcBuyer != null)
                {
                    return (decimal?)npcBuyer.GetBuyPrice(entityType);
                }
            }

            return null;
        }*/

        /// <summary>
        /// gives the price the two parties can agree on
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="isUIOwned"></param>
        /// <param name="npcBuyer"></param>
        /// <param name="entityType"></param>
        /// <returns></returns>
        private decimal? GetAgreedPrice(EntityGroup owner, /*bool isUIOwned,*/ EntityGroup buyer, EntityType entityType)
        {
           /* if (cargoActionType == CargoActionTypes.Sell)
            {*/
                return BuySellActionTemplate.GetTradePrice(entityType, buyer, owner);
          //  }


           /* if (!isUIOwned)
            {
                // displaying an NPC collection
                if (owner != null)
                {
                    if (cargoActionType == CargoActionTypes.Buy)
                    {
                        return (decimal?)owner.GetSellPrice(entityType);
                    }
                    else
                    {
                        return (decimal?)owner.GetBuyPrice(entityType);
                    }
                }

                return null;

            }
            else
            {
                // displaying own collection.
                // get the buyer price:
                if (npcBuyer != null)
                {
                    return (decimal?)npcBuyer.GetBuyPrice(entityType);
                }
            }

            return null;*/
        }

        /// <summary>
        /// Cases:
        /// 1. Buy from NPC expedition
        /// Price is determined by NPC sell price
        /// Show items for sale only
        /// 
        /// 2. Sell from player expedition
        /// Price is determined by NPC buy price
        /// Show items: 
        /// NPC is willing to buy
        /// + items stored for trade by player
        /// </summary>
        /// <param name="ownerOfItems"></param>
        /// <param name="expeditionOwner"></param>
        private void PopulateWithList(EntityGroup ownerOfItems, Expedition expeditionOwner, EntityGroup npcBuyer)
        {
            HashSet<EntityType> data;
            if (!GetData(out data))
            {
                return;
            }

            presentItemTypes.Clear();

            UIComponent itemRow;
            // currentListData = The.InGameUI.InventorySettings.GetData(allAvailableItems);

            bool isUIOwned = GetIsPlayerOwnedLocation(ownerOfItems);

            grdListView.BeginAddingEntries();
            decimal? price = null;
            List<EntityID> availableItems;
            foreach (var entityType in data)
            {
                if (entityType.KeyName.Contains("hauling"))
                {

                }

                price = GetAgreedPrice(ownerOfItems, /*isUIOwned,*/ npcBuyer, entityType);

               
                int noOfAvailableItems, demandedItems;
                bool itemSourceIsInvalid = false;
                if (!GetTradeItems(ownerOfItems, npcBuyer, isUIOwned, entityType, out availableItems, price, out noOfAvailableItems, out demandedItems, out itemSourceIsInvalid))
                {
                    if (itemSourceIsInvalid)
                    {
                        return;
                    }

                    continue;
                }

                presentItemTypes.Add(entityType, entityType);

                if (!grdListView.TryGetEntry(entityType, out itemRow))
                {
                    itemRow = AddItemRow(grdListView, entityType, ownerOfItems, true, false);
                }

                UpdateItemRow(itemRow, entityType, ownerOfItems, isUIOwned, availableItems, noOfAvailableItems, demandedItems, price, false);
            }

            // remove unused rows           
            grdListView.DeleteEntries<EntityType>(j => presentItemTypes.ContainsKey(j));

            Grid.Sorting sortOrder = GetSortOrder();

            grdListView.Sort(i => i.OrderByTag1, sortOrder);

            grdListView.EndAddingEntries();
        }

      /*  private BuySellPanelSettings GetSettings()
        {
            if (cargoActionType == CargoActionTypes.Buy)
            {
                return The.InGameUI.BuySettings;
            }
            else if (cargoActionType == CargoActionTypes.Sell)
            {
                return The.InGameUI.SellSettings;
            }

            return null;
        }*/

        private Grid.Sorting GetSortOrder()
        {
            Grid.Sorting sortOrder = Grid.Sorting.Ascending;

          //  BuySellPanelSettings settings = GetSettings();
            if (settings != null)
            {
                sortOrder = settings.SortingSettings.SortOrder;
            }

            return sortOrder;
        }


        private void Populate()
        {
            EntityGroup ownerOfItems, npcBuyer;
            Expedition expeditionOwner;

            if (!ResolveEntityGroup(out ownerOfItems, out expeditionOwner, out npcBuyer))
            {
                return;
            }

           // var settings = GetSettings();

            switch (settings.viewType)
            {
                case ViewType.Categories:
                    PopulateWithCategories(ownerOfItems, expeditionOwner, npcBuyer);
                    if (grdCategoryView.Count > 0 && grdListView.Count == 0)
                    {
                        PopulateWithList(ownerOfItems, expeditionOwner, npcBuyer);
                    }
                    break;
                case ViewType.List:
                    PopulateWithList(ownerOfItems, expeditionOwner, npcBuyer);
                    if (grdListView.Count > 0 && grdCategoryView.Count == 0)
                    {
                        PopulateWithCategories(ownerOfItems, expeditionOwner, npcBuyer);
                    }
                    break;
            }


            if (((settings.viewType == ViewType.Categories && grdCategoryView.Entries.Count == 0)
                || (settings.viewType == ViewType.List && grdListView.Entries.Count == 0))
                && !settings.FilterPropertySettings.HasActiveFilters())
            {
                lblNoWaresNote.ToolTip = "";
                if (CargoActionType == CargoActionTypes.Buy)
                {
                    bool isUIOwned = GetIsPlayerOwnedLocation(ownerOfItems);

                    if (isUIOwned)
                    {
                        if (settings.FilterPropertySettings.HasActiveFilters())
                        {
                            lblNoWaresNote.Text = "We are not offering anything for trade matching the filters.";
                        }
                        else
                        {
                            lblNoWaresNote.Text = "We are not offering anything for trade.";
                            lblNoWaresNote.ToolTip = "Items have to be placed inside a terminal and be included in the Trade settings to appear here.";
                        }
                    }
                    else
                    {
                        lblNoWaresNote.Text = "Nothing is available to buy.";
                    }
                }
                else
                {
                    lblNoWaresNote.Text = "Nothing can be sold here.";
                }

                lblNoWaresNote.FitToText();
                lblNoWaresNote.Y = sortingButtonsContainer.Bottom + 6;
                lblNoWaresNote.Visible = true;
            }
            else
            {
                lblNoWaresNote.Visible = false;
            }

            UpdateTotals(ownerOfItems, npcBuyer);

        }

        private int ValidateAndCountOrders(EntityType entityType, List<EntityID> availableItems)
        {
            int currentOrder = 0;
            List<EntityID> list;
            if (currentOrders.TryGetValue(entityType, out list)) // currentOrder);
            {
                // validate the ordered items against the data:
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    EntityID entityID = list[i];

                    if (availableItems == null || !availableItems.Contains(entityID)) //!orderedItemIsValid(entityID)) //BuySellActionTemplate.ItemIsValid(sharedKnowledge, entityID, terminal))
                    {
                        // the ordered item is no longer valid. remove it (replace it with another?)
                        list.RemoveAt(i);
                    }

                }

                currentOrder = list.Count;
            }
            return currentOrder;
        }


      

        /// <summary>            
        /// returns false if the item type is not for sale/purchase
        /// also false if the item source is invalid.
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="itemsOwner"></param>
        /// <param name="price"></param>
        /// <returns></returns>
        private bool GetTradeItems(EntityGroup itemsOwner, EntityGroup npcBuyer, bool isPlayerOwned, EntityType entityType, out List<EntityID> items, decimal? price, 
            out int available, out int demandedItems, out bool itemSourceIsInvalid)
        {
            itemSourceIsInvalid = false;
            items = null;
            available = 0;
            demandedItems = 0;
                      
          
            if (CargoActionType == CargoActionTypes.Buy)
            {
                if (!isPlayerOwned && price == null)
                {
                    // not for sale                  
                    return false;
                }


                items = this.getItems.Invoke(entityType,
                    out itemSourceIsInvalid);

                if (itemSourceIsInvalid) //??
                {
                    HandleDestroyedEntityGroupOrDataSource();

                    itemSourceIsInvalid = true;
                    available = 0;
                    return false;
                }

                if (items == null)
                {
                    if (price == null && isPlayerOwned)
                    {
                        // player looking at own site on world map
                        return false;
                    }
                    else
                    {
                        available = 0;
                    }
                }
                else
                {
                    available = items.Count;
                }
            }
            else if (CargoActionType == CargoActionTypes.Sell)
            {
                if (isPlayerOwned)
                {
                    items = this.getItems.Invoke(entityType,
                                   out itemSourceIsInvalid);

                    if (itemSourceIsInvalid)
                    {
                        HandleDestroyedEntityGroupOrDataSource();

                        itemSourceIsInvalid = true;
                        available = 0;
                        return false;
                    }

                    // player is selling
                    if (price.HasValue)
                    {
                        // the npc wants to buy this
                        demandedItems = npcBuyer.GetBuyAmount(entityType);

                        if (items != null)
                        {
                            available = Math.Min(items.Count, demandedItems);
                        }
                        else
                        {
                            available = 0;
                        }
                    }
                    else
                    {
                        if (items != null)
                        {
                            // the player has goods the npc doesn't want - show them anyways
                            available = 0;
                            return true;
                        }
                        else
                        {
                            // the item is neither wanted nor offered - hide it
                            return false;
                        }
                    }
                }
                else
                {
                    // invalid case...
                    if (price == null)
                    {
                        // not for sale
                        items = null;
                        available = 0;
                        return false;
                    }

                    // the NPC wants to buy this

                    items = null;

                    available = npcBuyer.GetBuyAmount(entityType); 
                }
            }
           
            return true;
        }


        public static int GetItemColumn(int headerXPos, bool isInCategoryPanel)
        {
            int xPos = headerXPos; // GetItemColumnFromHeader(headerXPos);
            if (isInCategoryPanel)
            {
                xPos -= 7;
            }
            return xPos;
        }


        /// <summary>
        /// copied from Inventory Panel
        /// </summary>
        /// <param name="categoryGrid"></param>
        /// <param name="noOfAvailableItems"></param>
        /// <param name="currentOrder"></param>
        /// <param name="entityType"></param>
        /// <param name="owner"></param>
        /// <param name="useCurrentUIOwner"></param>
        private UIComponent AddItemRow(Grid categoryGrid, EntityType entityType, EntityGroup owner, bool useCurrentUIOwner, bool isInCategoryPanel)
        {
            UIComponent item;

            item = new UIComponent(Interface.gui);
            categoryGrid.AddEntry(entityType, item);

            InventoryPanel.AddEntityTypeIcon(entityType, item, itemTypeIconColumnX);


            ItemTypeButtonEventArgs eventArgs = new ItemTypeButtonEventArgs(entityType);

            DataTypeButton tbCaption = new DataTypeButton(Interface.gui, HUD_Windows.DataSheet.InfoToShow.Data, entityType,
                GoalEvaluator.GetOwnerID(owner),
                useCurrentUIOwner);

            int xPos = GetItemColumn(captionX, isInCategoryPanel);
            tbCaption.Init(TextButton.TextButtonType.LCDToolTipBlack);
            tbCaption.ID = UIComponent.DataControlID.Caption;
            tbCaption.IsRoot = true;
            tbCaption.Text = entityType.PluralName;
            item.Add(tbCaption);
            tbCaption.TextAlignment = TextButton.TextAlign.Left;
            tbCaption.Width = 170; // 176; // quantityX - xPos;
            tbCaption.X = xPos;

            int productionLimit;

            HorizontalList hzNotAttainable = new HorizontalList(Interface.gui);
            item.Add(hzNotAttainable);
            hzNotAttainable.ID = UIComponent.DataControlID.NotAttainableIcons;
            hzNotAttainable.X = GetItemColumn(sliderX, isInCategoryPanel);
            hzNotAttainable.Height = 21;
            hzNotAttainable.Y = -2;
          //  item.CenterChildVertically(hzNotAttainable);


            FillableBar fillableBar = new FillableBar(Interface.gui, FillableBar.FillableBarType.LCDSlider, false, true, GameData.Instance.GUIConstants.TimeBetweenSliderButtonIncrements, GameData.Instance.GUIConstants.SliderButtonDelay); // productionMode == ProductionMode.Advanced);
            item.Add(fillableBar);
            fillableBar.ID = UIComponent.DataControlID.CurrentOrders;
            fillableBar.Width = 105;
            //   fillableBar.SetBarWidth(fillableBar.Width - 40); // !!?!?!?
            fillableBar.X = GetItemColumn(sliderX, isInCategoryPanel);  //sliderX;
            fillableBar.Y = 1;
            fillableBar.Tag1 = entityType;
            fillableBar.EventArgs = eventArgs;
            fillableBar.SliderMouseUp += new EventHandler(fillableBar_SliderMouseUp);
            fillableBar.SliderMouseDown += new EventHandler(fillableBar_SliderMouseDown);

            Label lbl;

            lbl = new Label(Interface.gui);
            item.Add(lbl);
            lbl.Init(Label.LabelType.LCDNormal);
            lbl.X = GetItemColumn(sliderX + 65, isInCategoryPanel);
            lbl.ID = UIComponent.DataControlID.Amount;

            lbl = new Label(Interface.gui);
            item.Add(lbl);
            lbl.Init(Label.LabelType.LCDNormal);      
            lbl.X = GetItemColumn(offerDemandX, isInCategoryPanel);
            lbl.ID = UIComponent.DataControlID.OfferDemand;
            lbl.TooltipExpires = false;
            lbl.TooltipWidth = 240;

            lbl = new Label(Interface.gui);
            item.Add(lbl);
            lbl.Init(Label.LabelType.LCDNormal);
            // lbl.Text = GetPriceAsString(price);         
            lbl.X = GetItemColumn(priceX, isInCategoryPanel);
            lbl.ID = UIComponent.DataControlID.Price;

            lbl = new Label(Interface.gui);
            item.Add(lbl);
            lbl.Init(Label.LabelType.LCDNormal);
            //  lbl.Text = entityType.ItemType.MaximumBulk.HasValue ? Entity.GetBulkAsString(entityType.ItemType.MaximumBulk.Value) : "";
            lbl.X = GetItemColumn(bulkX, isInCategoryPanel);
            lbl.ID = UIComponent.DataControlID.Bulk;

            item.OrderByTag1 = entityType.PluralName;

            return item;
        }

       

       

        private void AddCategoryRow(ref CollapsablePanel cpCategory, ref Grid categoryGrid, EntityCategory entityCategory)
        {
            cpCategory = new CollapsablePanel(Interface.gui, CollapsablePanel.PanelType.DropDownBig); //CollapsablePanel.PanelType.Node);
            cpCategory.HeadingYPos = 4;
            cpCategory.CollapsedHeight = grdCategoryView.ItemHeight;
            grdCategoryView.AddEntry(entityCategory, cpCategory);
            cpCategory.OrderByTag1 = entityCategory.SortOrder;
            cpCategory.Init(InventoryPanel.GetPanelSubType(entityCategory));
           // cpCategory.Init(); 
            cpCategory.Title = entityCategory.Name;
            cpCategory.Width = grdCategoryView.Width;

            //cpCategory.SetIcon(entityCategory.IconSpriteName);


            categoryGrid = new Grid(Interface.gui, ListBoxType.LCD /*ListBoxType.Main*/, Label.LabelType.LCDNormal);
            // categoryGrid.Position = new Point(
            categoryGrid.DebugTag = "categoryGrid";

            categoryGrid.FixedItemHeights = true; // false;
            categoryGrid.Width = cpCategory.Width; // make grid fill the collapsable panel
            cpCategory.AddContent(categoryGrid); // .ExpandedPanel.Add(categoryGrid);

            categoryGrid.ScrollBarEnabled = false;
            categoryGrid.ItemHeight = InventoryPanel.ItemHeight;
            categoryGrid.CanGrowInHeight = true;
            categoryGrid.Font = GUIManager.LCDandHUDBodyFontPath;
            categoryGrid.IsOuterGrid = false;
        }

        private void UpdateItemRow(UIComponent itemRow, EntityType entityType, EntityGroup owner, bool isOwnedByUI, List<EntityID> items,
            int noOfAvailableItems, int demandedItems, decimal? price, bool isInCategoryPanel)
        {
            int currentOrder = 0;
            if (currentOrders != null)
            {
                currentOrder = ValidateAndCountOrders(entityType, items);
            }

            int offeredItems = 0;
            if (items != null)
            {
                offeredItems = items.Count;
            }

            UIComponent amountComponent = itemRow.FindChildById(UIComponent.DataControlID.Amount);

            HorizontalList hzNotAttainable = itemRow.FindChildById(UIComponent.DataControlID.NotAttainableIcons) as HorizontalList;

            // update the slider:
            FillableBar fillableBar = itemRow.FindChildById(UIComponent.DataControlID.CurrentOrders) as FillableBar;

            bool sliderValuesWereChanged = false;

            bool isUIOwned = GetIsPlayerOwnedLocation(owner);

            if (!AllowOrders)
            {
                fillableBar.Visible = false;
                amountComponent.Visible = true;
                Label lblAmount = (Label)amountComponent;
                lblAmount.Text = noOfAvailableItems.ToString();
                lblAmount.FitToText();

                if (CargoActionType == CargoActionTypes.Sell)
                {
                    lblAmount.ToolTip = "Current demand: The amount of goods this site is willing to buy right now. This number may gradually rise."; //todo: dynamic detailed tooltip for all situations
                }
                else if (CargoActionType == CargoActionTypes.Buy)
                {
                    if (isUIOwned)
                    {
                        lblAmount.ToolTip = "For sale: The amount of goods we can trade right now. Items have to be inside a terminal and included in the Trade settings to appear here."; 
                
                    }
                    else
                    {
                        lblAmount.ToolTip = "For sale: The amount of goods this site has for sale right now. This number may gradually rise."; //todo: dynamic detailed tooltip for all situations
                    }
                }
                else
                {
                    lblAmount.ToolTip = null;
                }
            }
            else
            {
                amountComponent.Visible = false;

                TierOrAreaType unavailablePolicy;
                if (CanTrade(entityType, out unavailablePolicy))
                {
                    fillableBar.Visible = true;                   


                    if (sliderBeingDragged != fillableBar) // don't change the slider that the user is currently dragging
                    {

                        if (fillableBar.MaxValue != noOfAvailableItems)
                        {
                            fillableBar.MaxValue = noOfAvailableItems;
                            sliderValuesWereChanged = true;
                        }

                        if (fillableBar.Value != currentOrder)
                        {
                            fillableBar.Value = currentOrder;
                            sliderValuesWereChanged = true;
                        }

                       
                        if (sliderValuesWereChanged)
                        {
                            fillableBar.UpdateSliderPosition();
                        }

                        fillableBar.Visible = true;
                        //  fillableBar.MaxValue = noOfAvailableItems;

                        if (noOfAvailableItems > 0)
                        {
                            fillableBar.Enabled = true;
                            fillableBar.ToolTip = "Drag slider to specify amount to order.";
                        }
                        else
                        {
                            if (fillableBar.Enabled == true)
                            {
                                fillableBar.UpdateSliderPosition(); // we need to call this once
                                fillableBar.Enabled = false;
                            } 

                            if (CargoActionType == CargoActionTypes.Buy)
                            {
                                fillableBar.ToolTip = "This item is currently not available to buy.";
                            }
                            else
                            {
                                if (demandedItems == 0)
                                {
                                    fillableBar.ToolTip = "There is no demand for this item.";
                               
                                }
                                else
                                {
                                    fillableBar.ToolTip = "There are no items stored and offered for trade in the terminal building.";
                                }
                            }
                            //fillableBar.Visible = false;
                        }
                    }

                }
                else
                {
                    fillableBar.Visible = false;                   
                    hzNotAttainable.Visible = true;

                    PopulateNotAttainableIcons(hzNotAttainable, unavailablePolicy);
                }
            }


            // update production status:
            DataTypeButton tbCaption = (DataTypeButton)itemRow.FindChildById(UIComponent.DataControlID.Caption);
            tbCaption.SetAvailableStatusColor(noOfAvailableItems > 0);


            Label lblOfferDemand = (Label)itemRow.FindChildById(UIComponent.DataControlID.OfferDemand);
            
            if (CargoActionType == CargoActionTypes.Sell
                && isOwnedByUI)
            {
                lblOfferDemand.Text = offeredItems + "|" + demandedItems;

                StringBuilder text = new StringBuilder();
                Common.AppendLine(text, "Offer | Demand");
                Common.AppendDivider(text);
                Common.Append(text, "Amount we are offering for sale: ");
                Common.Append(text, offeredItems.ToString(), true);
                Common.AppendLine(text);
                Common.Append(text, "Max. amount buyer wants: ");
                Common.Append(text, demandedItems.ToString(), true);
                Common.AppendLine(text);
                Common.AppendLine(text);
                Common.Append(text, "Max. amount we can sell: ");
                Common.Append(text, noOfAvailableItems.ToString(), true);
                //Common.AppendFormat(text, "Max. amount we can sell: {0}", true, noOfAvailableItems);
                lblOfferDemand.ToolTip = text.ToString();
                
            }
            else 
            {
                lblOfferDemand.Text = "";
                lblOfferDemand.ToolTip = "";
              /*  lblOfferDemand.Text = offeredItems.ToString();
                lblOfferDemand.ToolTip = "The number of items available to buy right now";*/
            }

            lblOfferDemand.FitToText();
            lblOfferDemand.AlignRight(GetItemColumn(offerDemandX, isInCategoryPanel));
                                

            UIComponent priceComponent = itemRow.FindChildById(UIComponent.DataControlID.Price);
            if (priceComponent != null)
            {
                Label lblPrice = priceComponent as Label;
                lblPrice.Text = Common.GetPriceAsString(price);
                lblPrice.FitToText();
                lblPrice.AlignRight(GetItemColumn(priceX, isInCategoryPanel));

                if (isOwnedByUI)
                {
                    lblPrice.ToolTip = "The price we can sell 1 of these items for";
                }
                else
                {
                    if (CargoActionType == CargoActionTypes.Sell)
                    {
                        lblPrice.ToolTip = "The price we can sell 1 of these items for";
                    }
                    else
                    {
                        lblPrice.ToolTip = "The price we can buy 1 of these items for";
                    }
                }
            }

            float? bulk = TradeManager.GetBulkOfTradeItem(entityType);

            //float? bulk = entityType.ItemType.MaximumBulk;

            UIComponent bulkComponent = itemRow.FindChildById(UIComponent.DataControlID.Bulk);
            if (bulkComponent != null)
            {
                Label lbl = bulkComponent as Label;
                lbl.Text = bulk.HasValue ? Entity.GetBulkAsString(bulk.Value) : "";
                lbl.FitToText();
                lbl.AlignRight(GetItemColumn(bulkX, isInCategoryPanel));
            }

           // BuySellPanelSettings settings = GetSettings();

            //Set OrderByTag values:
            switch (settings.SortingSettings.SortedBy)
            {
                case BuyAndSell.BuySellPanelSettings.SortColumns.Name:
                    itemRow.OrderByTag1 = (itemRow.Tag1 as EntityType).PluralName;
                    break;

                case BuySellPanelSettings.SortColumns.Amount:
                    itemRow.OrderByTag1 = 100 * noOfAvailableItems + currentOrder;
                    break;

                case BuySellPanelSettings.SortColumns.Bulk:
                    itemRow.OrderByTag1 = bulk ?? 0f;
                    break;

                case BuySellPanelSettings.SortColumns.Price:
                    itemRow.OrderByTag1 = price;
                    break;

                case BuySellPanelSettings.SortColumns.OfferDemand:
                    itemRow.OrderByTag1 = 100 * offeredItems + demandedItems;
                    break;
            }
        }




        private void PopulateNotAttainableIcons(HorizontalList list, TierOrAreaType unavailablePolicy) // bool hasPolicy) 
        {
            list.BeginAddingEntries();

            string text = "";
            string delim = "";

            UIComponent icon;

            if (unavailablePolicy != null)
            {
                icon = ProductionOrderControl.AddOrGetIcon(unavailablePolicy, list, unattainableColor);
                text = "Not available. The following policy needs to be enacted first: " + unavailablePolicy.ToString();
                icon.ToolTip = text;
            }
            else
            {
                list.TryRemoveEntry(IconKeys.NoPolicy);
            }

            list.EndAddingEntries();
        }

        public override void Hide()
        {
            base.Hide();

            sliderBeingDragged = null;
        }

        void fillableBar_SliderMouseDown(object sender, EventArgs e)
        {
            // don't refresh the slider posiiton once the user has touched it:
            SetUserChangedSliderState((FillableBar)sender); //e);

        }

        private void SetUserChangedSliderState(FillableBar control) //EventArgs e)
        {
            sliderBeingDragged = control;

            /* if (!userChangedData.ContainsKey(control))
             {
                 userChangedData.Add(control, true);
             }*/

        }

        private bool GetIsPlayerOwnedLocation(EntityGroup owner)
        {
            if (owner != null && owner.GetAllegiance() == The.InGameUI.UIAllegiance)
            {
                return true;
            }

            return false;

            //return owner.GetAllegiance() == The.InGameUI.UIAllegiance;
        }

        void fillableBar_SliderMouseUp(object sender, EventArgs e)
        {
            sliderBeingDragged = null;

            EntityGroup owner, buyer;
            Expedition expeditionOwner;

            HashSet<EntityType> data;
            if (!GetData(out data))
            {
                return;
            }


            if (!ResolveEntityGroup(out owner, out expeditionOwner, out buyer))
            {
                return;
            }

            FillableBar slider = sender as FillableBar;
            EntityType entityType = (EntityType)slider.Tag1;

            int noOfCurrentlyOrdered = 0;

            int newOrder = slider.Value;

            // add/remove items:
            if (currentOrders == null)
            {
                currentOrders = new Dictionary<EntityType, List<EntityID>>();
            }

            List<EntityID> availableItems;
            List<EntityID> currentlyOrderedItems;

            int noOfAvailableItems, demandedItems;
            bool sourceIsInvalid;
            decimal? price;

            bool isUIOwned = GetIsPlayerOwnedLocation(owner);

            if (data.Contains(entityType)) // .TryGetValue(entityType, out availableItems))
            {
                price = GetAgreedPrice(owner, /*isUIOwned,*/ buyer, entityType); // GetNPCPrice(owner, entityType);

                if (GetTradeItems(owner, buyer, isUIOwned, entityType, out availableItems, price, out noOfAvailableItems, out demandedItems, out sourceIsInvalid))
                {
                    noOfCurrentlyOrdered = ValidateAndCountOrders(entityType, availableItems);

                    if (!currentOrders.TryGetValue(entityType, out currentlyOrderedItems))
                    {
                        currentlyOrderedItems = new List<EntityID>();
                        currentOrders.Add(entityType, currentlyOrderedItems);
                    }

                    // currentlyOrderedItems = currentOrders[entityType];

                    int difference = newOrder - noOfCurrentlyOrdered;

                    if (difference > 0)
                    {
                        // add
                        foreach (var item in availableItems)
                        {
                            if (!currentlyOrderedItems.Contains(item))
                            {
                                currentlyOrderedItems.Add(item);
                                difference--;

                                if (difference == 0)
                                    break;
                            }
                        }


                    }
                    else if (difference < 0)
                    {
                        // remove
                        while (difference != 0 && currentlyOrderedItems.Count > 0)
                        {
                            currentlyOrderedItems.RemoveAt(0);
                            difference++;
                        }
                    }
                }
                else
                {
                    if (sourceIsInvalid)
                    {
                        return;
                    }
                }


            }


            // TODO:
            // Common.AddOrUpdateDictionary(ref currentOrders, entityType, slider.Value);

            UpdateTotals(owner, buyer);

        }

        private void UpdateTotals(EntityGroup owner, EntityGroup npcBuyer)
        {
            float totalBulk = 0f;
            decimal totalCost = 0m;

           // bool isPlayerOwned = GetIsPlayerOwnedLocation(owner);

            if (currentOrders != null)
            {
                foreach (var item in currentOrders)
                {
                    if (item.Value.Count > 0)
                    {
                        switch (CargoActionType)
                        {
                            case CargoActionTypes.Buy:
                            case CargoActionTypes.Sell: //??

                                float? bulk = TradeManager.GetBulkOfTradeItem(item.Key);

                                if (bulk.HasValue) // item.Key.ItemType.MaximumBulk.HasValue)
                                {
                                    totalBulk += bulk.Value * item.Value.Count; // item.Key.ItemType.MaximumBulk.Value * item.Value.Count;
                                }

                                decimal? price = GetAgreedPrice(owner, /*isPlayerOwned,*/ npcBuyer, item.Key); // GetNPCPrice(owner, item.Key);
                                if (price.HasValue)
                                {
                                    totalCost += price.Value * item.Value.Count;

                                }

                                break;

                            /* case CargoActionTypes.Sell:

                                 break;*/
                        }
                    }
                }
            }

            lblTotalBulk.Text = Entity.GetBulkAsString(totalBulk);
            lblTotalBulk.FitToText();
            lblTotalBulk.AlignRight(bulkX);
            lblTotalBulk.Text += " BLK";

            lblTotalItemCost.Text = Common.MoneyAsString(totalCost, false); // totalCost.ToString("N");
            lblTotalItemCost.AlignRight(priceX);

        }

        void btCancel_Click(UIComponent sender, EventArgs e)
        {
            CancelDialog();
        }

        private void CancelDialog()
        {
            Window.Hide();

            if (CancelClick != null)
                CancelClick.Invoke(this, null);
        }

    }
}
