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
using UWGame.SimSide;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.Commands;
using UWGame.SimSide.Processes;


namespace UWGame.ClientSide.Interface.HUD_Windows
{
    public class EntityListWindow : HUDWindow
    {
        Grid grdEntities;

        EntityType entityType;

        private enum OwnerAction { Discard, Claim }

        List<EntityID> listOfEntities;

        //OLD:
        /// <summary>
        /// can be null, if we have populated from an area. we can populate the list from an owner or a zone
        /// </summary>
     //   EntityGroupID? ownerOfItems;

        /// <summary>
        /// can be null if we have have populated from an owner
        /// </summary>
   //     MapArea mapArea;

        /// <summary>
        /// the function to call to repopulate the list if no owner was supplied
        /// </summary>
     //   Func<EntityType, List<EntityID>> getListOfEntities;

        int gridHeaderY = correctedTopMargin; // topMargin;
        Label windowHeading;
        private static int WindowHeight = 20;
        private string hyperLinkToolTip = "LMB: Select the link target.\n RMB: Center on target.\n Double click: Select and center.";


        int gridY = 0;
        const int hzlStatusWidth = 60;
        private static int locationX = 2 * correctedSideMargin - 3;
        private static int statusX = locationX + 110 + sideMargin;
        private static int salvageX = statusX + hzlStatusWidth  + correctedSideMargin - 4;
        private static int discardClaimX = salvageX + 21 + correctedSideMargin;
        private static int captionX = discardClaimX + 20 + correctedSideMargin;
        
       //private static int thirdButtonX = discardClaimX + 21 + correctedSideMargin;
       //private static int captionX = thirdButtonX + 20 + correctedSideMargin;
        private static int spaceOnLeftAndBetween = correctedSideMargin;


        public EntityListWindow()
            : base(596, 180, true, true, true, level: Level.EntityTypeInfo)
        {
            DisplayWindow.SetResizableArea(ResizeAreas.Top, true);
            DisplayWindow.SetResizableArea(ResizeAreas.Bottom, true);

            DisplayWindow.MinHeight = 60;
            DisplayWindow.ResizableBorderSize = 6;
            DisplayWindow.Resize += DisplayWindow_Resize;

   
            windowHeading = new Label(gui);
            Add(windowHeading);
            windowHeading.Init(Label.LabelType.HUDWindowHeader);
            windowHeading.X = locationX;
            windowHeading.Y = gridHeaderY - 1;
            windowHeading.ID = UIComponent.DataControlID.Name;
            windowHeading.FitToText();

            CreateGridHeader();


            grdEntities = new Grid(gui, ListBoxType.HUDAndLCD, WindowSystem.Label.LabelType.HUDWindow);
            grdEntities.IsOuterGrid = true; // false;
            grdEntities.ScrollBarEnabled = true;
            // categoryGrid.Position = new Point(
            // categoryGrid.DebugTag = "categoryGrid";
            grdEntities.Y = gridY;// windowHeading.Bottom + doubleSpacing; // 48 + topMargin;
            grdEntities.FixedItemHeights = true; // false;
            grdEntities.Width = DisplayWindow.Width - 2 * correctedSideMargin + 9; // listSurface.Width; // make grid fill the panel   
            grdEntities.ItemHeight = 22; // 22;
            grdEntities.CanGrowInHeight = false; // true;            
            grdEntities.Font = GUIManager.LCDandHUDBodyFontPath;
            grdEntities.Height = DisplayWindow.Height - grdEntities.Y - 10; // 40; // 160

            Add(grdEntities);

            this.WorldPosition = null;
           // base.DisplayWindow.Level = Level.Dialogs;

            SetVerticalPositions();
        }

        void DisplayWindow_Resize(UIComponent sender)
        {
            SetVerticalPositions();
        }

        private void SetVerticalPositions()
        {
           
          //  listSurface.Height = btOK.Y - 13 - listSurface.Y;
            grdEntities.Height = DisplayWindow.Height - grdEntities.Y - 10; // 40; // 160

           // grdEntities.Height = listSurface.Height;


        }

        private void CreateGridHeader()
        {          

            Label lblHeader;
            lblHeader = new Label(gui);
            lblHeader.Init(Label.LabelType.HUDWindow);
            lblHeader.ID = UIComponent.DataControlID.Location;
            lblHeader.Text = "LOCATION";
            lblHeader.FitToText();
            lblHeader.X = locationX;
            lblHeader.Y = windowHeading.Bottom+3;
            Add(lblHeader);

            lblHeader = new Label(gui);
            lblHeader.Init(Label.LabelType.HUDWindow);
            lblHeader.ID = UIComponent.DataControlID.Status;
            lblHeader.Text = "STATUS";
            lblHeader.FitToText();
            lblHeader.X = statusX;
            lblHeader.Y = windowHeading.Bottom+3;
            Add(lblHeader);

            gridY = lblHeader.Bottom + 8;
        }

       
        public void SetDataSource(EntityType entityType, List<EntityID> listOfEntities)
        {
            this.entityType = entityType;
            this.listOfEntities = listOfEntities;          
        }
        
        private void FillHeader()
        {
            string text = "";
            //ITEM:Sticks
            if (entityType.ItemType != null)
            {
                text = "ITEM: ";
            }

            text += entityType.Name;
            windowHeading.Text = text;
        }

        public override void Refresh()
        {
            Populate();
        }

        public override void ShowOnPlayfield(int screenPosX, int screenPosY, bool avoidRightInterfaceArea = false)
        {
            base.ShowOnPlayfield(screenPosX, screenPosY, avoidRightInterfaceArea);

            DisplayWindow.BringToTop();


            this.WorldPosition = null;

            Fill();
        }

        public void PopulateAndShowOnPlayfield(UIComponent spawnButton, EntityType entityType, EntityGroup owner)
        {
            SetDataSource(entityType, The.InGameUI.GetExpedition().OwnedEntities.AllEntities[entityType]);

            int screenPosx = spawnButton.AbsolutePosition.X + 26;
            int screenPosY = spawnButton.AbsolutePosition.Y;

            ShowOnPlayfield(screenPosx, screenPosY);
        }

        public void OpenNextToStockButton(UIComponent itemButton)
        {
            ShowOnPlayfield(itemButton.AbsolutePosition.X - DisplayWindow.Width - 2, itemButton.AbsolutePosition.Y - gridY, false);
        }

        private void Populate()
        {
            grdEntities.BeginAddingEntries();

            UIComponent item = null;
          //  List<EntityID> listOfEntities = null;
            EntityGroup resolvedOwner = null;
            List<EntityID> invalidEntities = null;
            bool  canBeSalvagedNow, canBeDiscardedNow, canBeClaimed;
            IKnownEntityData entityData;
            EntityID entityID;

          /*  if (ownerOfItems != null)
            {
                resolvedOwner = LookUp<EntityGroup, EntityGroupID>.FindByID(ownerOfItems.Value);
                if (resolvedOwner != null)
                {
                    resolvedOwner.Items.TryGetValue(entityType, out listOfEntities);
                }
                // if we fail to resolve the owner, the list will be cleared.
            }
            else if (getListOfEntities != null) //mapArea != null)
            {
                // we won't repopulate from map area... why..?
                listOfEntities = getListOfEntities(entityType);
            }*/

            if (listOfEntities == null ||  listOfEntities.Count == 0)
            {
                this.grdEntities.Clear();
                return;
            }

            for (int i = listOfEntities.Count - 1; i >= 0; i--)
            {
                entityID = listOfEntities[i];
               /* if (resolvedOwner != null)
                {
                    if (!GoalEvaluator.HandleOwnerDataResult(The.InGameUI.UIAllegiance.SharedKnowledge, entityID, resolvedOwner, out entityData, entityType))
                    {
                        continue;
                    }
                }
                else
                {*/
                    if (GoalEvaluator.EntityDataResultCausesSkip(The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(entityID, out entityData)))
                    {
                        // mark as invalid:
                        if (invalidEntities == null)
                        {
                            invalidEntities = new List<EntityID>();
                        }
                        invalidEntities.Add(entityID);

                        continue;
                    }
               // }

                GetAllowedActions(entityData, out canBeSalvagedNow, out canBeDiscardedNow, out canBeClaimed);

                // see if the item is represented, the do an update:   
                if (!grdEntities.TryGetEntry(entityID, out item))
                {
                   
                    AddItemControls(entityID, entityData, canBeSalvagedNow, canBeDiscardedNow, canBeClaimed, out item);
                }
                Entity entitty = Entity.FindByID((EntityID)66);
                UpdateItemControls(entityData, item, canBeSalvagedNow, canBeDiscardedNow, canBeClaimed);
            }

            // remove rows that are no longer in the list of entities
            grdEntities.DeleteEntries<EntityID>(
                e => listOfEntities.Contains(e)
                    && (invalidEntities == null || !invalidEntities.Contains(e)));

            grdEntities.Sort(r => r.OrderByTag1, Grid.Sorting.Descending, r => r.OrderByTag2, Grid.Sorting.Descending);


            // FullLCDPanel.Cleanup<ResourceType, ResourcesAndJobs, ResourceCategory>(outerGrid, dictionary, categoryGrids);
            grdEntities.EndAddingEntries();

        }

        const string selectTooltip = "Click to select";
        const string selectTooltipOffsite = "Cannot be selected. The item is off site.";

        private void AddItemControls(EntityID key, IKnownEntityData entityData, bool canBeSalvagedNow, bool canBeDiscarded, bool canBeClaimed, out  UIComponent item) // string value1, string value2)
        {
            bool typeCanBeSalvaged = entityData.EntityType.CanBeSalvagedDirectly();

            //location  - status  -(salvage - discard       - Select) ->   dont need category headers
            //locationX - statusX - salvage - discardClaimX - captionX 

            int nextXLocation = locationX;
            item = new UIComponent(gui);

            Hyperlink hlLocation = new Hyperlink(gui, RenderType.Normal);
            hlLocation.ID = UIComponent.DataControlID.Location;
            hlLocation.X = nextXLocation;
            hlLocation.Tag1 = key;
            hlLocation.NormalColor = Color.White;
            hlLocation.DebugTag = "hlLocation" + grdEntities.Count;

            item.Add(hlLocation);

            nextXLocation = statusX;

            HorizontalList hzlStatus = new HorizontalList(The.InGameUI.gui, 7); //now that we don't support categories, let's allow 2 icons to be displayed, like the progress bar and a status icon // 1);
            hzlStatus.CenterItemsVertically = true;
            hzlStatus.ID = UIComponent.DataControlID.StatusIcon;
            hzlStatus.HorizontalSpacing = -2;
            hzlStatus.X = nextXLocation;
            hzlStatus.Visible = true;
            hzlStatus.Height = WindowHeight;
            hzlStatus.MinHeight = WindowHeight; // don't scale the height by its contents. this makes centering icons easier
            hzlStatus.MaxHeight = WindowHeight; // don't scale the height by its contents.                
            hzlStatus.Width = hzlStatusWidth;
            
            item.Add(hzlStatus);

            nextXLocation = salvageX;

            if (typeCanBeSalvaged)
            {
                ImageButton tbSalvage = new ImageButton(gui);
                tbSalvage.Init(ImageButtonType.HUDSalvage);
                tbSalvage.Enabled = canBeSalvagedNow;
                tbSalvage.ID = UIComponent.DataControlID.SalvageAction;
                tbSalvage.X = nextXLocation;
                tbSalvage.EventArgs = new EntityButtonEventArgs((EntityID)key);
                tbSalvage.ToolTip = "Salvage the object: When breaking this apart, some parts will be retrieved, some will be lost. See the process tooltip for more info.";
                tbSalvage.Click += new ClickHandler(salvage_Click);
                item.Add(tbSalvage);
                tbSalvage.Y = tbSalvage.Y - 1;
                tbSalvage.MaxHeight = tbSalvage.Height - 2;
               // nextXLocation = discardClaimX;

                ImageButton tbPackDown = new ImageButton(gui);
                tbPackDown.Init(ImageButtonType.HUDPackDown);
                tbPackDown.Enabled = canBeSalvagedNow;
                tbPackDown.ID = UIComponent.DataControlID.PackingDownAction;
                tbPackDown.X = nextXLocation;
                tbPackDown.EventArgs = new EntityButtonEventArgs((EntityID)key);
                tbPackDown.ToolTip = "Disassemble the object: All its parts will be retrieved. See the process tooltip for more info.";
                tbPackDown.Click += new ClickHandler(salvage_Click);
                item.Add(tbPackDown);
                tbPackDown.Y = tbPackDown.Y - 1;
                tbPackDown.MaxHeight = tbPackDown.Height - 2;
                nextXLocation = discardClaimX;
            }

            ImageButton tbDiscardClaim = new ImageButton(gui);
            tbDiscardClaim.Init((canBeClaimed) ? ImageButtonType.HUDClaim : ImageButtonType.HUDDiscard);
            tbDiscardClaim.ID = UIComponent.DataControlID.DiscardClaim;
            tbDiscardClaim.X = nextXLocation;
            tbDiscardClaim.ToolTip = (canBeClaimed) ? "Claim this object which is not owned by anyone" : "Discard the object";
            tbDiscardClaim.EventArgs = new EntityButtonEventArgs((EntityID)key);
            tbDiscardClaim.Click += new ClickHandler(owner_Click);
            tbDiscardClaim.MaxHeight = tbDiscardClaim.Height - 2;
            tbDiscardClaim.DebugTag = "ClaimButton" + grdEntities.Count;

            item.Add(tbDiscardClaim);
            tbDiscardClaim.Y = tbDiscardClaim.Y - 1;

            nextXLocation = (nextXLocation == salvageX) ? discardClaimX : captionX;

            //A third button for later usage
            // its not updated in update row 
            /*
             * nextXLocation = (nextXLocation == salvageX) ? discardClaimX : thirdButtonX;
             * 
            ImageButton tbThirdButton = new ImageButton(gui);
            tbThirdButton.Init(ImageButtonType.HUDClaim);
            tbThirdButton.ID = UIComponent.DataControlID.Action;
            tbThirdButton.X = nextXLocation;
          //  tbThirdButton.ToolTip = (canBeClaimed) ? "Claim the item not owned by anyone" : "Discard the item";
            tbThirdButton.EventArgs = new EntityButtonEventArgs((EntityID)key);
           // tbThirdButton.Click += new ClickHandler(owner_Click);            
            item.Add(tbThirdButton);
            tbThirdButton.Y = tbThirdButton.Y - 1;
            tbThirdButton.MaxHeight = tbThirdButton.Height - 2;


            nextXLocation = (nextXLocation == discardClaimX) ? thirdButtonX : captionX;
            */

            ItemTypeButtonEventArgs eventArgs = new ItemTypeButtonEventArgs(entityType);
            TextButton tbSelect = new TextButton(gui);
            tbSelect.Text = "SELECT";
            tbSelect.ID = UIComponent.DataControlID.Selector;
            tbSelect.Init(TextButton.TextButtonType.HUD);
            tbSelect.Tag1 = entityData.EntityID;
            tbSelect.X = nextXLocation;
            tbSelect.Y = tbDiscardClaim.Y;
           // tbSelect.ToolTip = 
            tbSelect.ScaleToFitText();
            tbSelect.Height = tbSelect.Height +2;
            tbSelect.Click += btName_Click;
            tbSelect.Visible = true;
            tbSelect.EventArgs = eventArgs;

            item.Add(tbSelect);

            grdEntities.AddEntry(key, item);

         //   ResizeWindow(tbSelect.Right + spaceOnLeftAndBetween);
        }



        void btName_Click(UIComponent sender, EventArgs e)
        {
            EntityID? entityID = sender.Tag1 as EntityID?;
            if (entityID.HasValue)
            {
                The.InGameUI.SelectEntity(entityID.Value);
            }
         //   The.InGameUI.SelectedEntity = sender.Tag1 as EntityID?;            
        }

        /// <summary>
        /// call from UpdateItemRow
        /// </summary>
        /// <param name="entityWithStatus"></param>
        private void UpdateIconList(IKnownEntityData entityWithStatus, HorizontalList hzlStatus)
        {
            IHasExposedProperties hasExposedProperties = null;
            hasExposedProperties = entityWithStatus as IHasExposedProperties;

            foreach (PresentationTypeCategory presentationTypeCategory in GameData.Instance.CustomStatusIconData.FinalPresentationTypeCategories)
            {
                int? numberOfItems = null;
                PresentationTypeCategoryProcessor.DisplayCategory(presentationTypeCategory, hasExposedProperties, hzlStatus, ref numberOfItems);
            }

            hzlStatus.Width = (hzlStatus.Width < hzlStatusWidth) ? hzlStatusWidth : hzlStatus.Width;           
        }

        private static void UpdateDiscardClaimButton(ImageButton bt, bool canBeDiscarded, bool canBeClaimed)
        {
            if (canBeDiscarded || canBeClaimed)
            {
                if (canBeDiscarded)
                {
                    bt.Init(ImageButtonType.HUDDiscard);
                    bt.Tag1 = OwnerAction.Discard;
                    bt.ToolTip = "Discard the item";
                }
                else
                {
                    bt.Init(ImageButtonType.HUDClaim);
                    bt.Tag1 = OwnerAction.Claim;
                    bt.ToolTip = "Claim the item not owned by anyone";
                }
                bt.Visible = true;
            }
            else
            {
                bt.Visible = false;
            }
        }

        private void UpdateItemControls(IKnownEntityData entityData, UIComponent item, bool canBeSalvagedNow, bool canBeDiscarded, bool canBeClaimed)
        {
            IKnownEntityData parentOrContainerData;
            Zone stockpile;
            int nextLocation;

            //Get the parent or container Entity and the zone 
            GetLocation(entityData, out parentOrContainerData, out stockpile);

            UIComponent itemComponent;
            itemComponent = item.FindChildById(UIComponent.DataControlID.Location);
            Hyperlink hlLocation = (Hyperlink)itemComponent;
            hlLocation.ToolTip = hyperLinkToolTip;
            if (parentOrContainerData != null)
            { 
                hlLocation.MaxWidth = 110;
                hlLocation.TargetEntityID = (uint)parentOrContainerData.EntityID;

                if (InGameInterface.CanSelectEntity(parentOrContainerData))
                {
                    hlLocation.Enabled = true;

                    if (entityData.ParentEntityID != null)
                    {
                        hlLocation.ToolTip = "*" + parentOrContainerData.GetDisplayName() + " \n*:part of this item. \n \n" + hlLocation.ToolTip;
                        hlLocation.Text = "*" + parentOrContainerData.GetDisplayName();
                    }
                    else
                    {
                        hlLocation.ToolTip = hyperLinkToolTip;
                        hlLocation.Text = parentOrContainerData.GetDisplayName();
                    }
                }
                else
                {
                    hlLocation.Text = parentOrContainerData.GetDisplayName();
                    hlLocation.Enabled = false;
                    hlLocation.ToolTip = selectTooltipOffsite;
                }
            }
            else
            {
                if (stockpile != null)
                {
                    hlLocation.Text = "Stockpile " + stockpile.GetDisplayName();
                }
                else
                {
                    hlLocation.Text = entityData.MapPosition.ToString();
                }
                hlLocation.TargetEntityID = (uint)entityData.EntityID;
                hlLocation.TargetMapPosition = entityData.MapPosition;
            }

            itemComponent = item.FindChildById(UIComponent.DataControlID.StatusIcon);
            HorizontalList hzlStatus = itemComponent as HorizontalList;
            UpdateIconList(entityData, hzlStatus);

            nextLocation = (hzlStatus.Width == hzlStatusWidth) ? hzlStatusWidth : hzlStatus.Width - hzlStatusWidth;

            ImageButton tbSalvage = item.FindChildById(UIComponent.DataControlID.SalvageAction) as ImageButton;
            ImageButton tbPackDown = item.FindChildById(UIComponent.DataControlID.PackingDownAction) as ImageButton;
            
            if (tbSalvage != null)
            {
                ImageButton salvageButtonToUse;
                if (entityData.EntityType.NonLivingType.SalvageProcessType.IsSalvageWithoutWaste)
                {
                    salvageButtonToUse = tbPackDown;
                    tbSalvage.Visible = false;
                }
                else
                {
                    salvageButtonToUse = tbSalvage;
                    tbPackDown.Visible = false;
                }

                salvageButtonToUse.Visible = true;
                if (!Salvage.SalvageJobExists(entityData))
                {
                    salvageButtonToUse.Enabled = canBeSalvagedNow;
                }
                else
                {
                    salvageButtonToUse.Enabled = false;
                }
                salvageButtonToUse.X = (nextLocation == hzlStatusWidth) ? salvageX : salvageX + nextLocation;
            }        



            itemComponent = item.FindChildById(UIComponent.DataControlID.DiscardClaim);
            ImageButton btDiscard = (ImageButton)itemComponent;
            UpdateDiscardClaimButton(btDiscard, canBeDiscarded, canBeClaimed);

            itemComponent = item.FindChildById(UIComponent.DataControlID.Selector);
            TextButton tbSelect = (TextButton)itemComponent;

            if (entityData.IsCompleted())
            {
                tbSelect.LabelColor = Color.White;
            }
            else
            {
                tbSelect.LabelColor = Color.Orange;
            }

            if (InGameInterface.CanSelectEntity(entityData))
            {
                tbSelect.Enabled = true;
                tbSelect.ToolTip = selectTooltip;
            }
            else
            {
                tbSelect.Enabled = false;
                tbSelect.ToolTip = selectTooltipOffsite;
               // tbName.ToolTip = ;
            }

            if (tbSalvage != null)
            {
                //we have a salvage button
                if (nextLocation == hzlStatusWidth)
                {
                    // horizontalList not wider then the preset one
                    btDiscard.X = discardClaimX;                  
                    tbSelect.X = captionX;
                }
                else
                {
                    //we need to shift a bit to the rigth
                    btDiscard.X = discardClaimX + nextLocation;                    
                    tbSelect.X = captionX + nextLocation;
                }
            }
            else
            {
                //we dont have a salvage button
                if (nextLocation == hzlStatusWidth)
                {
                    // horizontalList not wider then the preset one
                    btDiscard.X = salvageX;                    
                    tbSelect.X = discardClaimX-1;
                }
                else
                {
                    //we need to shift a bit to the rigth
                    btDiscard.X = salvageX + nextLocation;
                    tbSelect.X = discardClaimX + nextLocation-1;
                }
            }

            item.OrderByTag1 = entityData.IsCompleted() ? 1 : 0;
            item.OrderByTag2 = hlLocation.Text;

            ResizeWindow(tbSelect.Right + spaceOnLeftAndBetween);           
        }

        void Fill()
        {
            FillHeader();

            Populate();
        }

        void owner_Click(UIComponent sender, EventArgs e)
        {
            EntityButtonEventArgs args = e as EntityButtonEventArgs;
            EntityID entityID = args.Entity;

            IKnownEntityData entityData;
            EntityResult result = The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(entityID, out entityData);

            EntityGroup ownerOfItem;
            if (!LookUpOwners.ResolveEntityOwner(entityData, out ownerOfItem))
            {
                return;
            }

            if (ownerOfItem != null)
            {
                if (result == EntityResult.Destroyed || result == EntityResult.EntityStatusIsNowUnknown) // consider unknown entity locations as destroyed.
                {
                    ownerOfItem.DeleteEntity(entityID, entityType);
                    entityData.OwnedBy = null;
                    return;
                }
            }

            ImageButton btSender = sender as ImageButton;
            OwnerAction action = ((OwnerAction)btSender.Tag1);
            if (action == OwnerAction.Discard)
            {
                if (ownerOfItem != null)
                {
                    // jobs that use the unowned entity should be cancelled in the goal preconditions...
                    //ownerOfItem.InternalOwner.OwnerContent.HaulingJobs.FindAll(j => j is HaulingJob && ((HaulingJob)j).Item == entityID);

                    // even if the item no longer existed, we have now given up ownership of it. For reclaim, we must be able to to see it.
                    ownerOfItem.DeleteEntity(entityID, entityType);
                    entityData.OwnedBy = null;
                }
            }
            else
            {
                Entity itemEntity = entityData as Entity;
                // can only claim seen items!
                if (itemEntity != null)
                {
                    // claim the item for the main? expedition:
                    Expedition expedition = The.Sim.PlaySite.GetFirstPlayerExpedition();
                    itemEntity.ChangeOwnership(expedition);
                }
            }

            // just refresh whole list...
            Populate();
        }
        
        void salvage_Click(UIComponent sender, EventArgs e)
        {
            EntityButtonEventArgs args = e as EntityButtonEventArgs;
            EntityID entityID = args.Entity;
            ImageButton button = sender as ImageButton;

            if (!Salvage.SalvageJobExists(Entity.FindByID(entityID)))
            {
                HUDEntityContextMenu.TrySalvage(entityID);
                button.Enabled = false;
            }

        }

        private void GetLocation(IKnownEntityData entityData, out IKnownEntityData partOrContainerData, out Zone stockpile)
        {
            //Moved from populate          

            // part of                             
            // stored - carried             
            // in open
            stockpile = null;
            partOrContainerData = null;

            EntityID entityID = entityData.EntityID;
            EntityID? containedBy = entityData.ContainedBy;

            List<EntityID> invalidEntities = null;

            if (entityData.ParentEntityID != null)
            {
                if (GoalEvaluator.EntityDataResultCausesSkip(The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(entityData.ParentEntityID.Value,
                    out partOrContainerData)))
                {
                    // mark as invalid:
                    if (invalidEntities == null)
                    {
                        invalidEntities = new List<EntityID>();
                    }
                    invalidEntities.Add(entityID);
                }
            }
            else if (containedBy.HasValue)
            {
                if (GoalEvaluator.EntityDataResultCausesSkip(The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(containedBy.Value, out partOrContainerData)))
                {
                    // mark as invalid:
                    if (invalidEntities == null)
                    {
                        invalidEntities = new List<EntityID>();
                    }
                    invalidEntities.Add(entityID);
                }
            }
            else
            {
                partOrContainerData = null;
                stockpile = The.Map.GetTile(entityData.MapPosition.Value).GetStockpileZone(The.InGameUI.UIAllegiance);
            }
        }

        private void ResizeWindow(int width)
        {
            int spaceForScrollBar = 15;
            int maxX = this.DisplayWindow.Right; // this.DisplayWindow.X + this.DisplayWindow.Width;

            width = width + spaceForScrollBar;
            grdEntities.ScrollBar.X = width - spaceForScrollBar - 6;

            this.DisplayWindow.Width = width;
            this.DisplayWindow.X = maxX - width;
        }

        public static void GetAllowedActions(IKnownEntityData entityData, out bool canBeSalvagedNow, out bool canBeDiscardedNow, out bool canBeClaimed) //, out ProcessType salvageProcess)
        {
            CanBeSalvagedOrDiscarded(entityData, out canBeSalvagedNow, out canBeDiscardedNow);
            canBeClaimed = CanBeClaimed(entityData);
        }

        public static void CanBeSalvagedOrDiscarded(IKnownEntityData entityData, out bool canBeSalvagedNow, out bool canBeDiscardedNow)
        {
            canBeDiscardedNow = false;
            canBeSalvagedNow = false;

            // can only salvage and discard owned items:
            if (entityData.OwnedBy != null
                && The.InGameUI.UIAllegiance.HumanActivities != null)
            {
                EntityGroup resolvedOwner;
                if (LookUpOwners.ResolveEntityOwner(entityData, out resolvedOwner)
                    && resolvedOwner != null
                    && resolvedOwner.IsOwnedByAllegiance(The.InGameUI.UIAllegiance))
                {
                    if (entityData.UpgradeFor.HasValue) // don't allow upgrades to be discarded... if the upgraded item is discarded, then the upgrades should do the same...
                    {
                        canBeDiscardedNow = false;
                    }
                    else
                    {
                        canBeDiscardedNow = true;
                    }

                    if (entityData.EntityType.CanBeSalvagedDirectly()
                        && !Salvage.SalvageJobExists(entityData))
                    {
                        canBeSalvagedNow = true;
                    }
                }
            }
        }

        public static bool CanBeClaimed(IKnownEntityData entityData)
        {
            bool canBeClaimed;
            // we can only claim unowned items that are seen directly!!!
            if (entityData.OwnedBy == null
                && entityData is Entity)
            {
                if (entityData.UpgradeFor.HasValue) // don't allow upgrades to be claimed/discarded... if the upgraded item is claimed, then the upgrades should follow
                {
                    canBeClaimed = false;
                }
                else
                {
                    //Probably accurate at this point, perhaps there are plans for claiming animals in the future, in that case this should be changed//Finn
                    if (((Entity)entityData).NonLivingEntity != null)
                    {
                        canBeClaimed = true;
                    }
                    else
                    {
                        canBeClaimed = false;
                    }
                }
            }
            else
            {
                canBeClaimed = false;
            }

            return canBeClaimed;
        }
    }
}
