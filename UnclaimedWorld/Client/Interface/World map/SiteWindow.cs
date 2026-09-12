using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Items;
using UWGame.SimSide.Maps;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Goals;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Entities.Owners;
using UWGame.SimSide.Snapshots;
using UWGame.ClientSide.Interface.HUD_Windows;
using UWGame.SimSide.Overland;
using UWGame.SimSide.Allegiances;
using UWGame.ClientSide.Interface.LCD;
using UWGame.SimSide.Communication;
using UWGame.SimSide.Overland.Missions.Templates;
using UWGame.ClientSide.Interface.BuyAndSell;
using UWGame.SimSide;
using UWGame.ClientSide.Interface.Missions;
using UWGame.ClientSide.Interface.Personnel;
using UWGame.SimSide.Tiers;
using UWGame.SimSide.Policies;

namespace UWGame.ClientSide.Interface.World_map
{
    /// <summary>
    /// displays TravelLocations for a Site and allows the user to select one
    /// </summary>
    /// 

    public class SiteWindow : UIComponent
    {
        Grid grdOuter;

        Site site;

        /// <summary>
        /// This is not a buyer in case 1: Showing what the site is willing to buy.
        /// </summary>
        EntityGroupID? otherPartyID; 

       // bool isMissionAction;



        /// <summary>
        /// is null when viewing sites from World Map
        /// </summary>
     //   EntityGroupID? missionActionBuyerID;

        TextArea taDescription;

        public event Action<TravelLocation> TerminalSelected;

        // LCDInnerPanel panel;

        Label lblHeader;
        int titleY = 9 - 1;
        int sideMargin = 9 + 7;
        Label windowHeading;
        GUIManager gui;
        int captionX = 9 + 7, terminalX = 134 + 9;

        MovableArea movableArea;


        public event Action ChildDialogDisplayed;
        public event Action ChildDialogClosed;


        /// <summary>
        /// </summary>
        private TravelLocation? dialogSourceLocation;
      //  private /*static*/ MissionStopTemplate dialogSourceLocation;
    

        /// <summary>
        /// Don't check mouse status during animation.
        /// </summary>
        /// <param name="sender">Animating control.</param>
        private void OnStartAnimating(UIComponent sender)
        {
            IsAnimating = true;
        }

        /// <summary>
        /// TODO: find out if this is really needed!
        /// </summary>
        /// <param name="sender"></param>
        private void OnEndAnimating(UIComponent sender)
        {
            IsAnimating = false;
        }


        public SiteWindow(GUIManager gui, int width, int height)
            : base(gui)
        {

            this.gui = gui;

            Box background = new Box(gui);
            background.CornerSize = 20;
            background.SetSkinLocation(SkinState.Normal,gui.GUISpriteSheet.GetSourceRectangle("HUD_window_base"), null, null);
            background.Width = width;
            background.Height = height;
            Add(background);

            Width = width;
            Height = height;

            windowHeading = new Label(gui);
            Add(windowHeading);
            windowHeading.Init(Label.LabelType.HUDWindowHeader);
            windowHeading.X = sideMargin;
            windowHeading.Y = titleY;
            windowHeading.Text = "";
            windowHeading.FitToText();
            windowHeading.Visible = true;

            ImageButton closeButton = new ImageButton(gui);
            Add(closeButton);
            closeButton.Init(ImageButtonType.HUDClose);
            closeButton.X = this.Width - 25;
            closeButton.Y = titleY;
            closeButton.Visible = true;
            closeButton.Click += closeButton_Click;
            closeButton.ToolTip = "Close this window";
            closeButton.CheckedMode = CheckedModes.CannotBeChecked;
            closeButton.ZOrder = 1;

            taDescription = new TextArea(gui, ListBoxType.HUDAndLCD);
            taDescription.RenderType = RenderType.CRTAndLCD;
            taDescription.Init(Label.LabelType.HUDWindow);         
            taDescription.Width = width - 24;           
            Add(taDescription);
            taDescription.CanGrowInHeight = false;
            taDescription.ScrollBarEnabled = true;
            taDescription.Y = windowHeading.Bottom + 6;
            taDescription.X = sideMargin;
            taDescription.Height = 38;

            this.movableArea = new MovableArea(gui);
            base.Add(this.movableArea);
            this.movableArea.StartMoving += new StartMovingHandler(OnStartAnimating);
            this.movableArea.EndMoving += new EndMovingHandler(OnEndAnimating);
            this.movableArea.ZOrder = 0.1f;
            this.movableArea.Width = Width; // 330;
            this.movableArea.Height = Height;

            CreateGridHeader();

            grdOuter = new Grid(gui, ListBoxType.HUDAndLCD, WindowSystem.Label.LabelType.HUDWindow);//new Grid(gui, ListBoxType.HUDAndLCD, WindowSystem.Label.LabelType.HUDWindow);
            grdOuter.IsOuterGrid = true;
            grdOuter.Y = lblHeader.Bottom + 8;
            grdOuter.FixedItemHeights = true;
            grdOuter.Width = width;
            grdOuter.ItemHeight = 22;
            grdOuter.Font = GUIManager.LCDandHUDBodyFontPath;
            grdOuter.Height = 85;
            grdOuter.Visible = true;
            grdOuter.ZOrder = 1f;
            grdOuter.CanGrowInHeight = false;
            grdOuter.ScrollBar.ZOrder = 1f;
            grdOuter.ScrollBar.X = width - 20;

            Add(grdOuter);
        }



        private void CreateGridHeader()
        {
            int yPos = taDescription.Bottom + 3;

            lblHeader = new Label(gui);
            Add(lblHeader);
            lblHeader.Init(Label.LabelType.HUDWindow);
            lblHeader.Text = "EXPEDITION";
            lblHeader.FitToText();
            lblHeader.X = sideMargin;
            lblHeader.Y = yPos;
            lblHeader.Visible = true;

            lblHeader = new Label(gui);
            Add(lblHeader);
            lblHeader.Init(Label.LabelType.HUDWindow);
            lblHeader.Text = "TERMINAL";
            lblHeader.FitToText();
            lblHeader.X = terminalX;
            lblHeader.Y = yPos;
            lblHeader.Visible = true;
        }

        void closeButton_Click(UIComponent sender, EventArgs e)
        {
            Hide();
        }

        public void Fill(Site site, EntityGroupID? otherPartyID) //, bool isMissionAction)
        {
            this.site = site;
            this.otherPartyID = otherPartyID;
           // this.isMissionAction = isMissionAction;

            windowHeading.Text = "SITE: " + site.Name;

            taDescription.Text = site.Description;
        }

        /*  public void OpenNextToItemButton(UIComponent itemButton)
          {           
              Fill((Site)itemButton.Tag1);
           
              this.Position = itemButton.Parent.Position;
              this.X = this.X - this.Width;

              //Correct if its outside of the window
              if (this.X < 0)
              {
                  this.X = 0;
              }

              if (this.X + this.Width > The.InGameUI.WorldMapDialog.worldMap.Width)
              {
                  this.X = The.InGameUI.WorldMapDialog.worldMap.Width - this.Width;
              }

              if (this.Y < 0)
              {
                  this.Y = 0;
              }

              if (this.Y + this.Height >  The.InGameUI.WorldMapDialog.worldMap.Height)
              {
                  this.Y = The.InGameUI.WorldMapDialog.worldMap.Height - this.Height-60; 
              }

              Populate();
          }*/

        private void Populate()
        {
            grdOuter.Y = lblHeader.Bottom + 8;
            grdOuter.BeginAddingEntries();
            List<EntityID> terminals;

            List<object> keysToDelete = null;

            SharedKnowledge sharedKnowledge = The.InGameUI.UIAllegiance.SharedKnowledge;
            string travelLocationKey;
            foreach (var allegiance in site.Allegiances)
            {
                if (allegiance.RepresentativeEntityType.Person != null) // only list human allegiances...
                {
                    //  bool hasAddedAllegianceName = false;
                    foreach (var expedition in allegiance.Expeditions)
                    {

                        // terminals = expedition.OwnedEntities.Terminals;
                        //sharedKnowledge.AllKnownEntities.Terminals.TryGetValue(expedition.ID, out terminals); 

                        foreach (var list in expedition.OwnedEntities.Terminals)
                        {
                            // add terminal locations:
                            // TODO: only add personnel button once, not for each terminal

                            for (int i = list.Value.Count - 1; i >= 0; i--)
                            {
                                // TODO: the terminal type (helipad, pier...) should filter the transportation options
                                EntityID terminalID = list.Value[i];

                                travelLocationKey = TravelLocation.GetKey(site.ID, allegiance.ID, expedition.ID, terminalID);

                                IKnownEntityData terminalData;
                                if (GoalEvaluator.HandleOwnerDataResult(sharedKnowledge, terminalID,
                                    sharedKnowledge.AllKnownEntities, out terminalData))
                                {
                                    AddUpdateRow(travelLocationKey, allegiance, expedition, terminalData);
                                }
                                /* else
                                 {
                                     Common.AddToList(ref keysToDelete, travelLocationKey);
                                 }*/
                            }

                            // the other allegiance may not know its natural terminals..?
                            //add a personnel button


                        }


                        // only add "natural terminals" here... barge and harpy cannot use the same...
                        // add expedition location:
                        /*   travelLocationKey = TravelLocation.GetKey(site.ID, allegiance.ID, expedition.ID, null);
                           AddUpdateRow(travelLocationKey, allegiance, expedition, null);
                       */
                    }
                }
            }

            // here, we can add Sites without allegiances too (fishing? exploration?), but how/where is the vehicle going to arrive... i think we need natural terminals. but, we have no way of gaining knowledge about them... yet

            // delete 
            #region Delete

            foreach (var row in grdOuter.EntriesByKey)
            {
                TravelLocation loc = (TravelLocation)row.Value.FindChildById(UIComponent.DataControlID.Selector).Tag1;

                // check if this location still exists in the Sim, otherwise remove it from the grid:
                Site thisSite = LookUp<Site, SiteID>.FindByID((SiteID)loc.SiteID);

                if (thisSite == null)
                {
                    Common.AddToList(ref keysToDelete, row.Key);
                    continue;
                }

                if (loc.AllegianceID.HasValue)
                {
                    Allegiance thisAllegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)loc.AllegianceID.Value);

                    if (thisAllegiance == null ||
                        !site.Allegiances.Contains(thisAllegiance))
                    {
                        Common.AddToList(ref keysToDelete, row.Key);
                        continue;
                    }
                }

                if (loc.ExpeditionID.HasValue)
                {
                    Allegiance thisAllegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)loc.AllegianceID.Value);
                    Expedition thisExpedition = LookUp<Expedition, ExpeditionID>.FindByID((ExpeditionID)loc.ExpeditionID.Value);

                    if (thisExpedition == null ||
                        thisAllegiance == null ||
                        !thisAllegiance.Expeditions.Contains(thisExpedition))
                    {
                        Common.AddToList(ref keysToDelete, row.Key);
                        continue;
                    }
                }

                if (loc.TerminalEntityID.HasValue)
                {
                    Allegiance thisAllegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)loc.AllegianceID.Value);
                    Expedition thisExpedition = LookUp<Expedition, ExpeditionID>.FindByID((ExpeditionID)loc.ExpeditionID.Value);

                    IKnownEntityData terminalData;
                    if (!GoalEvaluator.HandleOwnerDataResult(sharedKnowledge, (EntityID)loc.TerminalEntityID.Value,
                        sharedKnowledge.AllKnownEntities, out terminalData))
                    {
                        keysToDelete.Add(row.Key);
                        continue;
                    }

                    /*  Entity terminal = Entity.FindByID((EntityID)loc.TerminalEntityID);

                      if (terminal == null ||
                          !thisExpedition.OwnedEntities.Contains(terminal))
                   
                          keysToDelete.Add(row.Key);
                          continue;
                      }*/
                }
            }

            if (keysToDelete != null)
            {
                foreach (object key in keysToDelete)
                {
                    grdOuter.RemoveEntry(key);
                }
            }

            #endregion

            // sorting by name:
            grdOuter.Sort(r => r.OrderByTag1, Grid.Sorting.Ascending, r => r.OrderByTag2, Grid.Sorting.Ascending);


            // final update to show/suppress repeated strings/labels:
            string prevExpeditionName = "";
            string thisExpeditionName = "";
            foreach (var row in grdOuter.Entries)
            {
                thisExpeditionName = (row.FindChildById(UIComponent.DataControlID.Caption) as Label).Text;

                if (thisExpeditionName.Equals(prevExpeditionName))
                {
                    (row.FindChildById(UIComponent.DataControlID.Caption) as Label).Text = "";
                }
                prevExpeditionName = thisExpeditionName;
            }

            grdOuter.EndAddingEntries();
        }

        private void AddUpdateRow(string travelLocationKey, Allegiance allegiance, Expedition expedition, IKnownEntityData terminal)
        {
            UIComponent itemRow;
            if (!grdOuter.TryGetEntry(travelLocationKey, out itemRow))
            {
                itemRow = AddItemRow(travelLocationKey, site, allegiance, expedition, terminal);

            }

            UpdateRow(itemRow, allegiance);
        }

        public void Refresh()
        {
            Populate();

        }


        private UIComponent AddItemRow(string key, Site site,
                                              SimSide.Allegiances.Allegiance allegiance,
                                              SimSide.Expeditions.Expedition expedition,
                                              IKnownEntityData terminal)
        {
            UIComponent itemRow = new UIComponent(gui);

            TravelLocation location;
            location = new TravelLocation(allegiance, (long)expedition.ID,
                terminal == null ? null : (long?)terminal.EntityID);
            /*  location.SetAllegiance(allegiance);
              location.ExpeditionID = (long)expedition.ID;
              location.SiteID = (long)site.ID;*/
            // location.TerminalEntityID = terminal == null ? null : (long?)terminal.ID;

            grdOuter.ItemHeight = 22;

            Label lblAllegianceName = new Label(gui);
            lblAllegianceName.Init(Label.LabelType.HUDWindow);
            lblAllegianceName.ID = UIComponent.DataControlID.Caption;
            lblAllegianceName.X = captionX;
            lblAllegianceName.Visible = true;
            lblAllegianceName.MaxWidth = 123;
            itemRow.Add(lblAllegianceName);

            Label lblterminal = new Label(gui);
            lblterminal.Init(Label.LabelType.HUDWindow);
            lblterminal.X = terminalX;
            lblterminal.Visible = true;
            lblterminal.ID = UIComponent.DataControlID.Transport;
            itemRow.Add(lblterminal);

            int buttonX = 308;

            TextButton tbSelect = new TextButton(gui);
            tbSelect.Init(TextButton.TextButtonType.HUD);
            tbSelect.Visible = true;           
            tbSelect.Text = "SELECT";
            tbSelect.Tag1 = location;
            tbSelect.ZOrder = 1;
            tbSelect.ScaleWidthToFitText();
            tbSelect.X = buttonX - tbSelect.Width;
            tbSelect.ID = UIComponent.DataControlID.Selector;
            itemRow.Add(tbSelect);
            tbSelect.Click += tbSelect_Click;


            ImageButton tbToBuy = new ImageButton(gui);
            tbToBuy.Init(ImageButtonType.HUDPrices);
            tbToBuy.ID = UIComponent.DataControlID.WillingToBuy;
            tbToBuy.X = buttonX;
            itemRow.Add(tbToBuy);
            tbToBuy.Click += tbToBuy_Click;
            tbToBuy.Tag1 = location;
            tbToBuy.NormalColor = GameData.Instance.GUIConstants.SellingButtonTint; // Color.Green;

            ImageButton tbForSale = new ImageButton(gui);
            tbForSale.Init(ImageButtonType.HUDPrices);
            tbForSale.ID = UIComponent.DataControlID.GoodsForSale;
            tbForSale.X = tbToBuy.Right + 0;  //331;
            itemRow.Add(tbForSale);
            tbForSale.Click += tbForSale_Click; //tbPrices_Click;
            tbForSale.Tag1 = location;
            tbForSale.NormalColor = GameData.Instance.GUIConstants.BuyingButtonTint;

          //  tbPrices.MaxHeight = tbPrices.Height - 2;
                      

            ImageButton tbPeople = new ImageButton(gui);
            tbPeople.Init(ImageButtonType.HUDPeople);
            tbPeople.ID = UIComponent.DataControlID.Personnel;
            tbPeople.X = tbForSale.Right + 0;
            itemRow.Add(tbPeople);
            tbPeople.Click += tbPeople_Click;
            tbPeople.Tag1 = location;
           // tbPrices.MaxHeight = tbPrices.Height - 2;
          

            /*
            TextButton tbPrices = new TextButton(gui);
            tbPrices.Init(TextButton.TextButtonType.HUD);
            tbPrices.X = 310;
            tbPrices.Text = "PRICES";          
            tbPrices.Tag1 = location;
            tbPrices.ZOrder = 1;
            tbPrices.ScaleWidthToFitText();
            tbPrices.ID = UIComponent.DataControlID.Price;
            itemRow.Add(tbPrices);
            tbPrices.Click += tbPrices_Click;
            */

            grdOuter.AddEntry(key, itemRow);

            return itemRow;
        }

        void tbForSale_Click(UIComponent sender, EventArgs e)
        {
          
            Site site;
            Allegiance allegiance;
            Expedition expedition;
            IKnownEntityData terminal;

            TravelLocation location = (TravelLocation)sender.Tag1;

            if (!location.ResolveLocation(The.InGameUI.UIAllegiance.SharedKnowledge, out site, out allegiance, out expedition, out terminal))
            {
                return;
            }

            EntityGroup buyer, seller;

            BuySellPanel.BuySellDialogMode mode;

            buyer = LookUp<EntityGroup, EntityGroupID>.FindByID(this.otherPartyID);
            seller = expedition.OwnedEntities;

           /* if (!isMissionAction)
            {*/
                 // case 2: Buyer is player, Seller is NPC              
                 mode = BuySellPanel.BuySellDialogMode.ViewBuyAtNPC;
            /* }
             else
             {
                 // case 4: Buyer is player, Seller is NPC
                 mode = BuySellPanel.BuySellDialogMode.ActionBuyAtNPC;
             }*/

            
            ShowDialog(sender, mode, buyer, seller);
        }


        void tbToBuy_Click(UIComponent sender, EventArgs e)
        {         
            // show what the site is willing to buy. 
            
            Site site;
            Allegiance allegiance;
            Expedition expedition;
            IKnownEntityData terminal;

            TravelLocation location = (TravelLocation)sender.Tag1;

            if (!location.ResolveLocation(The.InGameUI.UIAllegiance.SharedKnowledge, out site, out allegiance, out expedition, out terminal))
            {               
                return;
            }

            EntityGroup buyer, seller;

            BuySellPanel.BuySellDialogMode mode;
          /*  if (!isMissionAction) 
            {*/
                // case 1. NPC is buyer, no seller (Player selling??)
                buyer = expedition.OwnedEntities;
                seller = null;

                mode = BuySellPanel.BuySellDialogMode.ViewSellAtNPC;
          /*  }
            else
            {
                // case 3. NPC is buyer, Player is selling
                buyer = LookUp<EntityGroup, EntityGroupID>.FindByID(this.otherPartyID);
                seller = expedition.OwnedEntities;

                mode = BuySellPanel.BuySellDialogMode.ActionSellAtPlayer;
            }*/

            ShowDialog(sender, mode /*CargoActionTypes.Sell*/, buyer, seller);
        }



        void ShowDialog(UIComponent sender, BuySellPanel.BuySellDialogMode mode, /* CargoActionTypes action,*/ EntityGroup buyer, EntityGroup seller)
        {
            TravelLocation location = (TravelLocation)sender.Tag1;

          /*  Site site;
            Allegiance allegiance;
            Expedition expedition;
            IKnownEntityData terminal;

            if (!location.ResolveLocation(The.InGameUI.UIAllegiance.SharedKnowledge, out site, out allegiance, out expedition, out terminal))
            {
                //HandleDestroyedMissionStop();
                return;
            }
  
            EntityGroup buyer = LookUp<EntityGroup, EntityGroupID>.FindByID(this.missionActionBuyerID);
            */


            dialogSourceLocation = location; // save it for callbacks

            if (ChildDialogDisplayed != null)
            {
                ChildDialogDisplayed.Invoke();
                //The.InGameUI.WorldMapDialog.ShowModalOverlay();
            }

          
          /*  ShowBuySellDialog(sender.AbsolutePosition, action, null,
                allegiance, expedition, buyer);
            */

          /*  Allegiance allegiance = null;
            if (seller != null)
            {
                allegiance = seller.GetAllegiance();
            }*/

            BuySellPanel dialog = The.InGameUI.BuySellDialog;

            dialog.CancelClick += new EventHandler(BuySellDialog_CancelClick);
            dialog.Window.Close += ChildDialogWindow_Close;


            The.InGameUI.FillAndShowBuySellDialog(sender.AbsolutePosition, mode, //action, false,
                null, CanTrade, /*allegiance,*/ seller, buyer, GetItemsForSale);        
            
        }


        const string noCommTooltip = "No communication with this allegiance";

        private void UpdateRow(UIComponent item, Allegiance allegiance)
        {
            UIComponent itemComponent;

            itemComponent = item.FindChildById(UIComponent.DataControlID.Selector);
            TextButton tbSelect = (TextButton)itemComponent;
            
            TravelLocation travelLocation = (TravelLocation)tbSelect.Tag1;  //item.FindChildById(UIComponent.DataControlID.Selector).Tag1;

            itemComponent = item.FindChildById(UIComponent.DataControlID.Caption);
            if (itemComponent != null)
            {
                Label lblValue = (Label)itemComponent;
                lblValue.Text = allegiance.Name;

                if (lblValue.TextWidth > lblValue.MaxWidth)
                {
                    lblValue.ToolTip = allegiance.Name;
                }
                else
                {
                    lblValue.ToolTip = "";
                }
            }

            itemComponent = item.FindChildById(UIComponent.DataControlID.Transport);
            if (itemComponent != null)
            {
                Label lblterminal = (Label)itemComponent;


                if (travelLocation.TerminalEntityID == null)
                {
                    lblterminal.Text = "No terminal";
                }
                else
                {
                    Entity entity = Entity.FindByID((EntityID)travelLocation.TerminalEntityID);
                    lblterminal.Text = entity.GetDisplayName(); //.EntityType.Name;
                }
            }

          
            ImageButton tbToBuy = (ImageButton)item.FindChildById(UIComponent.DataControlID.WillingToBuy);        
            ImageButton tbForSale = (ImageButton)item.FindChildById(UIComponent.DataControlID.GoodsForSale);
            ImageButton tbPeople = (ImageButton)item.FindChildById(UIComponent.DataControlID.Personnel);


            CommunicationMethod? method;
            Allegiance fromAllegiance = The.InGameUI.UIAllegiance;
            bool isInCommRange = Communicates.IsInCommunicationRange(fromAllegiance, allegiance, out method);


            if (isInCommRange)
            {
                tbForSale.Enabled = true;
                tbForSale.ToolTip = "View prices and goods for sale at this location";

                tbToBuy.Enabled = true;
                tbToBuy.ToolTip = "View prices for goods this location is willing to buy";

                tbPeople.Enabled = true;
                tbPeople.ToolTip = "See people at this location who are interested in migrating"; //"View people at this location"
            }
            else
            {
                tbForSale.Enabled = false;
                tbForSale.ToolTip = noCommTooltip;

                tbToBuy.Enabled = false;
                tbToBuy.ToolTip = noCommTooltip;

                tbPeople.Enabled = false;
                tbPeople.ToolTip = noCommTooltip;
            }

            // don't show ToBuy for player's own sites:
            if (allegiance == The.InGameUI.UIAllegiance)
            {
                tbToBuy.Visible = false;
            }
            else
            {
                tbToBuy.Visible = true;
            }


            if (TerminalSelected != null) // only set from mission screen
            {
                tbSelect.Visible = true;

               // TravelLocation travelLocation = (TravelLocation)tbSelect.Tag1;
                bool isAlreadySelected;
                bool wrongTerminalType;
                bool canSelect = The.InGameUI.CreateMissionPanel.CanSelectLocation(travelLocation, out isAlreadySelected, out wrongTerminalType);

                if (canSelect)
                {                    
                    if (isInCommRange)
                    {

                        if (The.InGameUI.CreateMissionPanel.HasTransportationIfStart(travelLocation))
                        {
                            tbSelect.Enabled = true;
                            tbSelect.ToolTip = "Select this location";
                        }
                        else
                        {
                            tbSelect.Enabled = false;
                            tbSelect.ToolTip = "This cannot be selected as a starting location since there is no transportation currently available from this site.";
                        }
                        
                    }
                    else
                    {
                        tbSelect.Enabled = false;
                        tbSelect.ToolTip = noCommTooltip;
                    }
                }
                else
                {
                    tbSelect.Enabled = false;

                    if (isAlreadySelected)
                    {
                        tbSelect.ToolTip = "This location is already selected";
                    }
                    else if (wrongTerminalType)
                    {
                        IKnownEntityData terminalData;
                        allegiance.SharedKnowledge.GetKnownData((EntityID)travelLocation.TerminalEntityID, out terminalData);
                        if (terminalData != null)
                        {
                            tbSelect.ToolTip = TravelActionTemplate.GetWrongTerminalError(terminalData); // "This terminal is not accessible to the transport";
                        }
                    }
                    else
                    {
                        tbSelect.ToolTip = "";
                    }
                }              
            }
            else
            {
                tbSelect.Visible = false;
            }

            item.OrderByTag1 = Expedition.FindByID((ExpeditionID)travelLocation.ExpeditionID).Name;
            item.OrderByTag2 = travelLocation.TerminalEntityID == null ? "" : Entity.FindByID((EntityID)travelLocation.TerminalEntityID).Name;
        }

      

        void tbPeople_Click(UIComponent sender, EventArgs e)
        {
             TravelLocation location = (TravelLocation)sender.Tag1;
             dialogSourceLocation = location; // save it for callbacks

             ShowPersonnelDialog(sender.AbsolutePosition);

             if (ChildDialogDisplayed != null)
             {
                 ChildDialogDisplayed.Invoke();
                 //The.InGameUI.WorldMapDialog.ShowModalOverlay();
             }            

        }

        private void ShowPersonnelDialog(Point absolutePosition)
        {

            //ShowModalOverlay();

            PersonnelDialog dialog = The.InGameUI.PersonnelDialog;

            dialog.CancelClick += new EventHandler(PersonnelDialog_CancelClick);

            dialog.Window.Close += ChildDialogWindow_Close;

            // show the buy/load dialog
            dialog.FillAndShow(GetPeople, absolutePosition, false, false);

        }

        void ChildDialogWindow_Close(UIComponent sender)
        {
            ((Window)sender).Close -= ChildDialogWindow_Close;

            if (ChildDialogClosed != null)
            {
                ChildDialogClosed.Invoke();
            }
        }


        private void ShowBuySellDialog(Point absolutePosition, CargoActionTypes selectedAction,
            Dictionary<EntityType, List<EntityID>> currentOrders,  
            Allegiance allegiance, Expedition expedition, EntityGroup buyer)
        {

              
            
        }

        /// <summary>
        /// we can't show policies without a home expedition...
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="tierPolicy"></param>
        /// <returns></returns>
        private bool CanTrade(EntityType entityType, out TierOrAreaType tierPolicy)
        {
            //Expedition expedition = GetHomeExpedition();
            tierPolicy = null;
            return true;

            //return InGameInterface.CanTrade(expedition, entityType, out tierPolicy);
        }

        private List<IKnownEntityData> GetPeople()
        {           
            return CreateMissionPanel.GetPeople(dialogSourceLocation);
        }


        private List<EntityID> GetItemsForSale(EntityType type, out bool sourceIsInvalid)
        {
            return CreateMissionPanel.GetItemsForSale(type, dialogSourceLocation.Value, null, out sourceIsInvalid);
        }

        void BuySellDialog_CancelClick(object sender, EventArgs e)
        {
            ResetAfterBuySellDialog();
        }

        void PersonnelDialog_CancelClick(object sender, EventArgs e)
        {
            ResetAfterPersonnelDialog();

        }

        public void ResetAfterBuySellDialog()
        {
            BuySellPanel dialog = The.InGameUI.BuySellDialog;
                       
            dialog.CancelClick -= new EventHandler(BuySellDialog_CancelClick);            
            //dialog.Window.Close -= ChildDialogWindow_Close; // the close event fires a bit later, after fading.

            dialogSourceLocation = null;

          //  RemoveModalTintedOverlay();
        }

       
        public void ResetAfterPersonnelDialog()
        {
            PersonnelDialog dialog = The.InGameUI.PersonnelDialog;

            dialog.CancelClick -= new EventHandler(PersonnelDialog_CancelClick);
            //dialog.Window.Close -= ChildDialogWindow_Close; // the close event fires a bit later, after fading.

            dialogSourceLocation = null;

            //  RemoveModalTintedOverlay();
        }

        void tbSelect_Click(UIComponent sender, EventArgs e)
        {
            if (TerminalSelected != null)
            {
                TerminalSelected.Invoke((TravelLocation)sender.Tag1);
            }

            /*
            bool acceptLocation = The.InGameUI.CreateMissionPanel.AvoidStartEqualsDestination((TravelLocation)sender.Tag1);

            if (acceptLocation)
            {   
                Hide();

                The.InGameUI.WorldMapDialog.SelectedTravelLocation = (TravelLocation)sender.Tag1;
                The.InGameUI.WorldMapDialog.btOK_Click(sender, null);
            }   */
        }

        public void Hide()
        {
            this.Visible = false;
            this.Y = 1000;
        }

        public void Show()
        {
            this.Visible = true;

            Populate();
        }




        private static void AddTravelLocation(ComboBox cb,
         ref bool hasAddedSiteName, ref bool hasAddedAllegianceName, ref bool hasAddedExpeditionName,
         Site site, SimSide.Allegiances.Allegiance allegiance, SimSide.Expeditions.Expedition expedition, Entity terminal, TravelLocation? excludeLocation)
        {

            if (excludeLocation != null
                && (AllegianceID)excludeLocation.Value.AllegianceID == allegiance.ID
                && (ExpeditionID)excludeLocation.Value.ExpeditionID == expedition.ID
                && ((terminal == null && excludeLocation.Value.TerminalEntityID == null) || ((EntityID?)excludeLocation.Value.TerminalEntityID == terminal.ID)))
            {
                return;
            }

            TravelLocation location;

            long? terminalID = null;
            if (terminal != null)
            {
                terminalID = (long)terminal.ID;
            }

            location = new TravelLocation(allegiance, (long)expedition.ID, terminalID);
            /* location.SetAllegiance(allegiance);
             location.ExpeditionID = (long)expedition.ID;*/


            string displayName = "";

            /*
             if (!hasAddedSiteName)
             {
                 if (site.IsPlaySite)
                 {
                     displayName += "- HERE -";
                 }

                 //displayName.Append(site.Value.Name);
                 displayName += site.Name;
                 hasAddedSiteName = true;
             }
             else
             {
                 displayName = displayName.PadLeft(site.Name.Length + separatorLength);
             }

             if (!hasAddedAllegianceName)
             {
                 displayName += separator + allegiance.Name;
                 hasAddedAllegianceName = true;
             }
             else
             {
                 displayName = displayName.PadLeft(displayName.Length + separatorLength + allegiance.Name.Length);
             }

             if (!hasAddedExpeditionName)
             {
                 displayName += separator + expedition.Name;
                 hasAddedExpeditionName = true;
             }
             else
             {
                 displayName = displayName.PadLeft(displayName.Length + separatorLength + expedition.Name.Length);
             }

             if (terminal != null)
             {
                 displayName += separator + terminal.EntityType.Name;
             }
             else
             {
                 //??
             }
             */
            cb.AddEntry(location, displayName);
        }

    }

    #region As a window
    /* public class SiteWindow : HUDWindow
    {
        Grid grdOuter;

        Site site;

        int gridHeaderY = correctedTopMargin - 1; // topMargin;
        int sideMargin = correctedSideMargin + 7;
        Label windowHeading;

        int captionX = correctedSideMargin + 7, discardClaimX = 289, terminalX =/* 356*/
    //134 + correctedSideMargin, locationX = 464 + correctedSideMargin;
    /*
        public SiteWindow() //int screenWidth, int screenHeight, int screenX)
            : base(348, 192, true, true)
        {
            CreateGridHeader();

            grdOuter = new Grid(gui, ListBoxType.HUDAndLCD, WindowSystem.Label.LabelType.HUDWindow);
            grdOuter.IsOuterGrid = true; // false;

            // categoryGrid.Position = new Point(
            // categoryGrid.DebugTag = "categoryGrid";
            grdOuter.Y = windowHeading.Bottom + doubleSpacing + 16; // 48 + topMargin;
            grdOuter.FixedItemHeights = true; // false;
            grdOuter.Width = DisplayWindow.Width - 2 * correctedSideMargin + 9; // listSurface.Width; // make grid fill the panel           
            grdOuter.ScrollBarEnabled = true;
            grdOuter.ItemHeight = 22; // 22;
            grdOuter.CanGrowInHeight = false; // true;            
            grdOuter.Font = GUIManager.LCDandHUDFontPath;
            grdOuter.Height = 126; // 40; // 160
            grdOuter.Visible = true;

            Add(grdOuter);

            base.DisplayWindow.Level = Level.Dialogs;

        }

        private void CreateGridHeader()
        {
            windowHeading = new Label(gui);
            Add(windowHeading);
            windowHeading.Init(Label.LabelType.HUDWindowHeader);
            windowHeading.X = sideMargin;
            windowHeading.Y = gridHeaderY;
            windowHeading.Text = "";
            windowHeading.FitToText();
            windowHeading.Visible = true;
            Label lblHeader;

            lblHeader = new Label(gui);
            Add(lblHeader);
            lblHeader.Init(Label.LabelType.HUDWindow);
            lblHeader.Text = "EXPEDITION";
            lblHeader.FitToText();
            lblHeader.X = sideMargin;
            lblHeader.Y = windowHeading.Bottom + 3;
            lblHeader.Visible = true;

            lblHeader = new Label(gui);
            Add(lblHeader);
            lblHeader.Init(Label.LabelType.HUDWindow);
            lblHeader.Text = "TERMINAL";
            lblHeader.FitToText();
            lblHeader.X = terminalX;
            lblHeader.Y = windowHeading.Bottom + 3;
            lblHeader.Visible = true;

        }

        public void PopulateAndShowOnPlayfield(UIComponent spawnButton, EntityType entityType, EntityGroup owner)
        {
            //  The.InGameUI.EntityListWindow.SetDataSource(entityType, owner != null ? owner.ID : (EntityGroupID?)null, null);
            The.InGameUI.EntityListWindow.SetDataSource(entityType, The.InGameUI.GetExpedition().OwnedEntities.AllEntities[entityType]);

            int screenPosx = spawnButton.AbsolutePosition.X + 26;
            int screenPosY = spawnButton.AbsolutePosition.Y;

            ShowOnPlayfield(screenPosx, screenPosY);
        }

        public void Fill(Site site)
        {
            this.site = site;
            windowHeading.Text = "SITE: " + site.Name;
        }

        public override void Refresh()
        {

            Populate();
        }

        public override void ShowOnPlayfield(int screenPosX, int screenPosY, bool avoidRightInterfaceArea = false)
        {
            base.ShowOnPlayfield(screenPosX, screenPosY, avoidRightInterfaceArea);

            Populate();
        }

        public void OpenNextToItemButton(UIComponent itemButton)
        {
            this.DisplayWindow.ZOrder = 1f;
            Fill((Site)itemButton.Tag1);
            ShowOnPlayfield(itemButton.AbsolutePosition.X - 2* DisplayWindow.Width - 2, itemButton.AbsolutePosition.Y, false);
            //ShowOnPlayfield(itemButton.AbsolutePosition.X - DisplayWindow.Width - 2, itemButton.AbsolutePosition.Y, false);
        }

        private void Populate()
        {
            grdOuter.BeginAddingEntries();

            UIComponent item = null;

            List<Entity> terminals;

            string travelLocationKey;
            foreach (var allegiance in site.Allegiances)
            {
                if (allegiance.RepresentativeEntityType.Person != null) // only list human allegiances...
                {
                    //  bool hasAddedAllegianceName = false;
                    foreach (var expedition in allegiance.Expeditions)
                    {
                        terminals = expedition.GetTerminals(); // the terminal type (helipad, pier...) should filter the transportation options
                        
                        // add terminal locations:
                        if (terminals != null)
                        {
                            foreach (var terminal in terminals)
                            {
                                travelLocationKey = TravelLocation.GetKey(site.ID, allegiance.ID, expedition.ID, terminal.ID);

                                AddUpdateRow(travelLocationKey, allegiance, expedition, terminal);
                            }
                        }
                        else
                        {
                            //why would we add an empty if we have terminals ?? AO
                            // add expedition location:
                            travelLocationKey = TravelLocation.GetKey(site.ID, allegiance.ID, expedition.ID, null);
                            AddUpdateRow(travelLocationKey, allegiance, expedition, null);
                        }                       
                    }
                }
            }
            // TODO: here, we can add Sites without allegiances too (fishing? exploration?):

            // delete 
            #region Delete
            List<object> deletTheseKeys = new List<object>(); 
            
            foreach (var row in grdOuter.EntriesByKey)
            {
                TravelLocation loc = (TravelLocation)row.Value.FindChildById(UIComponent.DataControlID.Selector).Tag1;

                // check if this location still exists in the Sim, otherwise remove it from the grid:
                Site thisSite = LookUp<Site, SiteID>.FindByID((SiteID)loc.SiteID);

                if (thisSite == null)
                {
                    deletTheseKeys.Add(row.Key);
                    continue;
                }
                
                if (loc.AllegianceID.HasValue)
                {
                    Allegiance thisAllegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)loc.AllegianceID.Value);

                    if (thisAllegiance == null ||
                        !site.Allegiances.Contains(thisAllegiance))
                    {
                        deletTheseKeys.Add(row.Key);
                        continue;
                    }
                }

                if (loc.ExpeditionID.HasValue)
                {
                    Allegiance thisAllegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)loc.AllegianceID.Value);
                    Expedition thisExpedition = LookUp<Expedition, ExpeditionID>.FindByID((ExpeditionID)loc.ExpeditionID.Value);

                    if (thisExpedition == null ||
                        thisAllegiance == null ||
                        !thisAllegiance.Expeditions.Contains(thisExpedition))
                    {
                        deletTheseKeys.Add(row.Key);
                        continue;
                    }
                }

                if (loc.TerminalEntityID.HasValue)
                {
                    Allegiance thisAllegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)loc.AllegianceID.Value);
                    Expedition thisExpedition = LookUp<Expedition, ExpeditionID>.FindByID((ExpeditionID)loc.ExpeditionID.Value);
                    Entity terminal = Entity.FindByID((EntityID)loc.TerminalEntityID);

                    if (terminal == null ||
                        !thisExpedition.OwnedEntities.Contains(terminal))
                    {
                        deletTheseKeys.Add(row.Key);
                        continue;
                    }
                }  
            }

            foreach (object key in deletTheseKeys)
            {
                grdOuter.RemoveEntry(key);
            }
            #endregion

            // sorting by name:
            grdOuter.Sort(r => r.OrderByTag1, Grid.Sorting.Ascending, r => r.OrderByTag2, Grid.Sorting.Ascending);


            // final update to show/suppress repeated strings/labels:
            string prevExpeditionName = "";
            string thisExpeditionName = "";
            foreach (var row in grdOuter.EntriesByKey.Reverse())
            {
                thisExpeditionName = (row.Value.FindChildById(UIComponent.DataControlID.Caption) as Label).Text;

                if (thisExpeditionName.Equals(prevExpeditionName))
                {
                    (row.Value.FindChildById(UIComponent.DataControlID.Caption) as Label).Text = ""; 
                }
                prevExpeditionName = thisExpeditionName;
            }

            grdOuter.EndAddingEntries();
        }

        private void AddUpdateRow(string travelLocationKey, Allegiance allegiance, Expedition expedition, Entity terminal)
        {
            UIComponent itemRow;
            if (!grdOuter.TryGetEntry(travelLocationKey, out itemRow))
            {
                itemRow = AddItemRow(travelLocationKey, site, allegiance, expedition, terminal);

            }

            UpdateRow(itemRow, allegiance);
        }


        private UIComponent AddItemRow(string key, Site site,
                                              SimSide.Allegiances.Allegiance allegiance,
                                              SimSide.Expeditions.Expedition expedition,
                                              Entity terminal)
        {
            UIComponent itemRow = new UIComponent(gui);

            TravelLocation location;
            location = new TravelLocation();
            location.SetAllegiance(allegiance);
            location.ExpeditionID = (long)expedition.ID;
            location.SiteID = (long)site.ID;
            
            location.TerminalEntityID = terminal == null ? null : (long?)terminal.ID;

            grdOuter.ItemHeight = 22;

            Label lblAllegianceName = new Label(gui);
            lblAllegianceName.Init(Label.LabelType.HUDWindow);
            lblAllegianceName.ID = UIComponent.DataControlID.Caption;
            lblAllegianceName.X = captionX;
            lblAllegianceName.Visible = true;
            lblAllegianceName.MaxWidth = 123;
            itemRow.Add(lblAllegianceName);

            Label lblterminal = new Label(gui);
            lblterminal.Init(Label.LabelType.HUDWindow);
            lblterminal.X = terminalX;
            lblterminal.Visible = true;
            lblterminal.ID = UIComponent.DataControlID.Transport;
            itemRow.Add(lblterminal);

            TextButton tbSelect = new TextButton(gui);
            tbSelect.Init(TextButton.TextButtonType.HUD);
            tbSelect.Visible = true;
            tbSelect.X = 249;
            tbSelect.Text = "SELECT";
            tbSelect.Tag1 = location;
            tbSelect.ScaleWidthToFitText();
            tbSelect.ID = UIComponent.DataControlID.Selector;

            itemRow.Add(tbSelect);

            tbSelect.Click += tbSelect_Click;

            grdOuter.AddEntry(key, itemRow);

            return itemRow;
        }


        private void UpdateRow(UIComponent item, Allegiance allegiance)
        {
            UIComponent itemComponent;
            TravelLocation travelLocation = (TravelLocation)item.FindChildById(UIComponent.DataControlID.Selector).Tag1;

            itemComponent = item.FindChildById(UIComponent.DataControlID.Caption);
            if (itemComponent != null)
            {
                Label lblValue = (Label)itemComponent;
                lblValue.Text = allegiance.Name;

                if (lblValue.TextWidth > lblValue.MaxWidth)
                {
                    lblValue.ToolTip = allegiance.Name;
                }
                else
                {
                    lblValue.ToolTip = "";
                }
            }

            itemComponent = item.FindChildById(UIComponent.DataControlID.Transport);
            if (itemComponent != null)
            {
                Label lblterminal = (Label)itemComponent;


                if (travelLocation.TerminalEntityID == null)
                {
                    lblterminal.Text = "No terminal";
                }
                else
                {
                    Entity entity = Entity.FindByID((EntityID)travelLocation.TerminalEntityID);
                    lblterminal.Text = entity.EntityType.Name;
                }
            }

            item.OrderByTag1 = Expedition.FindByID((ExpeditionID)travelLocation.ExpeditionID).Name;
            item.OrderByTag2 = travelLocation.TerminalEntityID == null ? "" : Entity.FindByID((EntityID)travelLocation.TerminalEntityID).Name;
        }
        
        void tbSelect_Click(UIComponent sender, EventArgs e)
        {
            The.InGameUI.WorldMapDialog.SelectedTravelLocation = (TravelLocation)sender.Tag1;

            bool acceptLocation = The.InGameUI.CreateMissionPanel.AvoidStartEqualsDestination();


            if (acceptLocation)
            {
                The.InGameUI.WorldMapDialog.btOK.Enabled = true;
                The.InGameUI.WorldMapDialog.btOK.ToolTip = "Accepts the order and closes the dialog.";
                The.InGameUI.WorldMapDialog.btOK.Color = Color.Black;

                Hide();
            }
            else
            {
                The.InGameUI.WorldMapDialog.SelectedTravelLocation = null;
            }
        }



        private static void AddTravelLocation(ComboBox cb,
         ref bool hasAddedSiteName, ref bool hasAddedAllegianceName, ref bool hasAddedExpeditionName,
         Site site, SimSide.Allegiances.Allegiance allegiance, SimSide.Expeditions.Expedition expedition, Entity terminal, TravelLocation excludeLocation)
        {

            if (excludeLocation != null
                && (AllegianceID)excludeLocation.AllegianceID == allegiance.ID
                && (ExpeditionID)excludeLocation.ExpeditionID == expedition.ID
                && ((terminal == null && excludeLocation.TerminalEntityID == null) || ((EntityID?)excludeLocation.TerminalEntityID == terminal.ID)))
            {
                return;
            }

            TravelLocation location;

            location = new TravelLocation();
            location.SetAllegiance(allegiance);
            location.ExpeditionID = (long)expedition.ID;

            if (terminal != null)
            {
                location.TerminalEntityID = (long)terminal.ID;
            }

            string displayName = "";

            /*
             if (!hasAddedSiteName)
             {
                 if (site.IsPlaySite)
                 {
                     displayName += "- HERE -";
                 }

                 //displayName.Append(site.Value.Name);
                 displayName += site.Name;
                 hasAddedSiteName = true;
             }
             else
             {
                 displayName = displayName.PadLeft(site.Name.Length + separatorLength);
             }

             if (!hasAddedAllegianceName)
             {
                 displayName += separator + allegiance.Name;
                 hasAddedAllegianceName = true;
             }
             else
             {
                 displayName = displayName.PadLeft(displayName.Length + separatorLength + allegiance.Name.Length);
             }

             if (!hasAddedExpeditionName)
             {
                 displayName += separator + expedition.Name;
                 hasAddedExpeditionName = true;
             }
             else
             {
                 displayName = displayName.PadLeft(displayName.Length + separatorLength + expedition.Name.Length);
             }

             if (terminal != null)
             {
                 displayName += separator + terminal.EntityType.Name;
             }
             else
             {
                 //??
             }
             */
    /*
 cb.AddEntry(location, displayName);
}
               
}*/
    #endregion
}
