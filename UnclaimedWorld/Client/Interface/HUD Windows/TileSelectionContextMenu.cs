using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Items;
using Microsoft.Xna.Framework;
using UWGame.SimSide;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Expeditions;
using UWGame.Control.Commands;
using UWGame.SimSide.Commands;
using UWGame.SimSide.Entities;
using UWGame.ClientSide.Interface.Inventory;

namespace UWGame.ClientSide.Interface.HUD_Windows
{
    public class TileSelectionContextMenu : HUDWindow
    {
        private const int collapsedHeight = 16;
        int fullHeight;

        ImageButton btDelete;
       
        TextButton tbStockpile, tbGather, tbHunt, tbScout, tbForage, tbPatrol, tbAttack;

        TextButton tbConnect;
        
        ImageButton btCycle, btModify;

        //Commented out all lblZone code as it was requested that it should not be displayed in this window.
        //Label lblZone;


        public const string overlapsWarning = "The area overlaps with an existing stockpile";

        public const string notConnectedWarning = "Unable to do this, because the zone is not continuous";

        private const string stockpileTooltip = "Stockpile items in this zone";

        public Window OpeningWindow;


        # region  Child windows

        public GatherResourcesWindow ZoneGatherResourcesWindow;
        private StockpileWindow zoneStockpileWindow;
        private PatrolWindow patrolWindow;
        private HuntWindow huntWindow;
        private AttackWindow attackWindow;

        #endregion

        public TileSelectionContextMenu()
            : base(209, collapsedHeight, true, level: Level.Bottom) // false)
        {
            //base.DisplayWindow

            ZoneGatherResourcesWindow = new GatherResourcesWindow();
            zoneStockpileWindow = new StockpileWindow();
            patrolWindow = new PatrolWindow();
            huntWindow = new HuntWindow();
            attackWindow = new AttackWindow(); 

            base.DisplayWindow.ViewPort.MouseOut += new MouseOutHandler(DisplayWindow_MouseOut);

                        
            //lblZone = new Label(gui);
            //Add(lblZone);
            //lblZone.Init(Label.LabelType.HUDWindowHeader);
            //lblZone.X = sideMargin;
            //lblZone.Y = 5; // topMargin; // to align with ZoneMarker label
          

         /*   lblStockpileOverlaps = new Label(gui);
            lblStockpileOverlaps.Init(Label.LabelType.HUDWindow);
            lblStockpileOverlaps.Text = overlapsWarning;
            lblStockpileOverlaps.FitToText();

            lblNotConnected = new Label(gui);
            lblNotConnected.Init(Label.LabelType.HUDWindow);
            lblNotConnected.Text = notConnectedWarning;
            lblNotConnected.FitToText();*/

            TileSelectionContextMenu.CreateCycleButton(ref btCycle, gui, The.InGameUI.ContextMenuOpener.CycleButtonXPos);
            btCycle.DebugTag = "cycleButton";
            Add(btCycle);
            btCycle.Click += new ClickHandler(btCycle_Click);
         //   Add(btModify);
         //   btModify.Click += new ClickHandler(btModify_Click);




            tbConnect = new TextButton(gui);
            tbConnect.Text = "Crop";
            tbConnect.ToolTip = "Remove the areas that are not connected to the start point";
            tbConnect.Init(TextButton.TextButtonType.HUD);
            tbConnect.Click += new ClickHandler(bt_ConnectClick);


            int yPos = 2; // DisplayWindow.Height - 25; // 162;
            int xPos = 5;
            int ySpacing = 0; // 2;

          //  TextButton bt;

        /*    bt = new TextButton(gui);
            Add(bt);
            bt.Text = "DEL";
            bt.ToolTip = "Delete the zone";
            bt.Init(TextButton.TextButtonType.HUD);
            bt.Click += new ClickHandler(bt_DeleteClick);
            bt.Y = topMargin;
            bt.X = 60;
            bt.ScaleToFitText();
            */
                    
           

             btDelete = new ImageButton(gui);
             Add(btDelete);         
             btDelete.Init(ImageButtonType.HUDDelete);
             btDelete.ToolTip = "Delete the zone";
             btDelete.Y = topMargin;
             btDelete.X = btCycle.Right + singleSpacing; // 60;
             btDelete.Click += new ClickHandler(bt_DeleteClick);
            // btDelete.Visible = mapArea.Zone != null;


            yPos = 43;


         
            tbGather = new TextButton(gui);
            Add(tbGather);
            tbGather.Text = "GATHER";
            tbGather.ToolTip = "Select resources to harvest in the zone";
            tbGather.Init(TextButton.TextButtonType.HUDGather);
            tbGather.Click += new ClickHandler(btGather_Click);
            tbGather.Y = yPos;
            tbGather.X = xPos;
            tbGather.ScaleWidthToFitText();
            tbGather.CheckedMode = CheckedModes.CanBeChecked;

            int yDistance = tbGather.Height + ySpacing;

            yPos += yDistance;

            tbStockpile = new TextButton(gui);
            Add(tbStockpile);
            tbStockpile.Text = "STOCKPILE";
            tbStockpile.ToolTip = stockpileTooltip;
            tbStockpile.Init(TextButton.TextButtonType.HUDStockpile);
            tbStockpile.Click += new ClickHandler(btStockpile_Click);
            tbStockpile.Y = yPos;
            tbStockpile.X = xPos;
            tbStockpile.ScaleWidthToFitText();
            tbStockpile.CheckedMode = CheckedModes.CanBeChecked;

            yPos += yDistance;

            tbScout = new TextButton(gui);
            Add(tbScout);
            tbScout.Text = "SCOUT";
            tbScout.ToolTip = "Explore the zone briefly";
            tbScout.Init(TextButton.TextButtonType.HUDScout);
            tbScout.Click += new ClickHandler(btScout_Click);
            tbScout.Y = yPos;
            tbScout.X = xPos;
            tbScout.ScaleWidthToFitText();
            tbScout.CheckedMode = CheckedModes.CanBeChecked;

            yPos += yDistance;

            tbForage = new TextButton(gui);
            Add(tbForage);
            tbForage.Text = "EXAMINE"; // this is for zones. once we do the research mechanic on entities, we will for that use the word Analyze  
            tbForage.ToolTip = "Make a thorough examination of the area to uncover hidden resources";
            tbForage.Init(TextButton.TextButtonType.HUDForage);
            tbForage.Click += new ClickHandler(btExamine_Click);
            tbForage.Y = yPos;
            tbForage.X = xPos;
            tbForage.ScaleWidthToFitText();
            tbForage.CheckedMode = CheckedModes.CanBeChecked;

            yPos += yDistance;

            tbPatrol = new TextButton(gui);
            Add(tbPatrol);
            tbPatrol.Text = "PATROL";
            tbPatrol.ToolTip = "Patrol the zone continously and engage any threats that appear (Required stance: Fearless)";
            tbPatrol.Init(TextButton.TextButtonType.HUDPatrol);
            tbPatrol.Click += new ClickHandler(tbPatrol_Click);
            tbPatrol.Y = yPos;
            tbPatrol.X = xPos;
            tbPatrol.ScaleWidthToFitText();
            tbPatrol.CheckedMode = CheckedModes.CanBeChecked;

            yPos += yDistance;

            tbAttack = new TextButton(gui);
            Add(tbAttack);
            tbAttack.Text = "ATTACK";
            tbAttack.ToolTip = "Do a combat sweep of the area, attacking any entities of the specified type (Required stance: Fearless)";
            tbAttack.Init(TextButton.TextButtonType.HUDAttack);
            tbAttack.Click += new ClickHandler(tbAttack_Click);
            tbAttack.Y = yPos;
            tbAttack.X = xPos;
            tbAttack.ScaleWidthToFitText();
            tbAttack.CheckedMode = CheckedModes.CanBeChecked;

            yPos += yDistance;

            tbHunt = new TextButton(gui);
            Add(tbHunt);
            tbHunt.Text = "HUNT";
            tbHunt.ToolTip = "Locate and hunt prey in the zone.";
            tbHunt.Init(TextButton.TextButtonType.HUDHunt);
            tbHunt.Click += new ClickHandler(tbHunt_Click);
            tbHunt.Y = yPos;
            tbHunt.X = xPos;
            tbHunt.ScaleWidthToFitText();
            tbHunt.CheckedMode = CheckedModes.CanBeChecked;

          

            //yPos += tbForage.Bottom + ySpacing;

           
            // these icons are added as needed
            for (int i = 0; i < 3; i++)
            {
                Image icon = new Image(gui);           
                icon.Texture = GameData.Instance.BillboardSpriteSheet.Texture;               
                icon.X = 0;
                harvestResourceImages[i] = icon;
            }

            for (int i = 0; i < 3; i++)
            {
                Image icon = new Image(gui);
                icon.Texture = GameData.Instance.BillboardSpriteSheet.Texture;
                icon.X = 0;
                stockpileImages[i] = icon;
            }

            fullHeight = tbHunt.Bottom + sideMargin;

            base.DisplayWindow.Height = fullHeight; // yPos;

        //    base.DisplayWindow.UpdateEvent += new UpdateHandler(DisplayWindow_UpdateEvent);

        //    base.windowHeight
            //xPos += bt.Width + 10;

        }

        public static void CreateCycleButton(ref ImageButton btCycle, /*ref ImageButton btModify,*/ GUIManager gui, int xPos)
        {
           /* btModify = new ImageButton(gui);
            btModify.Init(ImageButtonType.HUDModifyZone);
            btModify.ToolTip = "Modify the area";
            btModify.Y = topMargin;
            btModify.X = rightEdge - btModify.Width; // btCycle.X + btCycle.Width + buttonSpacing;
            */            

            btCycle = new ImageButton(gui);         
            btCycle.Init(ImageButtonType.HUDCycleEntity);           
            btCycle.ToolTip = "Cycle through entities in the zone";
            btCycle.Y = topMargin;
            btCycle.X = xPos; // btModify.X - btCycle.Width; // xPos; // sideMargin;


        }

    //    private const float timeToWaitForMouseToEnter = 1f;

       /* void DisplayWindow_UpdateEvent(Microsoft.Xna.Framework.GameTime gameTime)
        {
            

        }*/


        private bool ChildWindowIsVisible()
        {
            return ZoneGatherResourcesWindow.DisplayWindow.IsVisibleAndActive
                || zoneStockpileWindow.DisplayWindow.IsVisibleAndActive
                || patrolWindow.DisplayWindow.IsVisibleAndActive
                || huntWindow.DisplayWindow.IsVisibleAndActive
                || attackWindow.DisplayWindow.IsVisibleAndActive;
        }


        void DisplayWindow_MouseOut(UIComponent sender, InputEventSystem.MouseEventArgs args)
        {
            //The.InGameUI.Tooltip.SpawningWindow == DisplayWindow; // .IsShowingTooltipForComponent()

            if (!TooltipIsShownForThisOrOpeningWindow()
                && !ChildWindowIsVisible())
            {
                Hide();
            }
        }

        /// <summary>
        /// used to prevent the window from closing because the mouse is moved over a tooltip that belongs to this winodw or its parent (the opener)
        /// </summary>
        /// <returns></returns>
        bool TooltipIsShownForThisOrOpeningWindow()
        {
            if (The.InGameUI.Tooltip.DisplayWindow.Visible == true)
            {
                if (The.InGameUI.Tooltip.SpawningWindow == DisplayWindow)
                {
                    return true;
                }

                if (The.InGameUI.Tooltip.SpawningWindow == OpeningWindow) // The.InGameUI.ContextMenuOpener.DisplayWindow)
                {
                     return true;
                }
            }            
 
            return false;
        }

        void bt_ConnectClick(UIComponent sender, EventArgs e)
        {
            MapArea mapArea;
            mapArea = TileSelectionContextMenu.GetMapArea();


            List<TerrainTile> newList = null;
            bool isConnected = mapArea.CheckConnectivity(false, ref newList);
            // stockpile demands a connected area
            if (!isConnected)
            {
               // TODOCOMMAND
                mapArea.CropTilesToConnectedArea();
                 
             //   mapArea.

            }
        }

        void bt_DeleteClick(UIComponent sender, EventArgs e)
        {
            if (The.InGameUI.SelectedZone != null)
            {
               // The.InGameUI.SelectedZone.Remove();

                Command destroyZone = new DeleteZone(The.InGameUI.SelectedZone.ID);

                The.Client.Controller.StoreAndExecuteCommand(destroyZone);

                //The.InGameUI.SelectedZone.marker.Hide();
                //The.InGameUI.hide
                The.InGameUI.SelectedZone = null;

                this.Hide();
            }

        }

        void bt_MouseOver(InputEventSystem.MouseEventArgs args)
        {
        //    base.DisplayWindow.Height = fullHeight;
        }


        public static bool CreateAndSelectZone(EntityGroup expeditionOwner)
        {
            if (CanCreateAndSelectZone())
            {
                if (The.InGameUI.SelectedZone != null)
                {
                    //We got an selected zone.
                    //Lets not create a new one?
                }
                else
                {
                    Zone newZone = new Zone(expeditionOwner, The.InGameUI.SelectedTiles);

                    The.InGameUI.SelectedZone = newZone;
                    newZone.MapArea.MapAreaRender.IsSelected = true;
                }
                

                return true;
                //return newZone;
            }

            return false;

        }

        public static bool CanCreateAndSelectZone()
        {
            return The.InGameUI.SelectedZone != null //If we already have a selected zone we do not need to create a new one to use
                || The.InGameUI.SelectedTiles.Count > 0; //Else we check if we have tiles to create a new zone with.
        }

        private Image[] harvestResourceImages = new Image[3];
        private Image[] stockpileImages = new Image[3];



        public override void Refresh()
        {
            base.Refresh();

            RefreshThisPanel();

            if (this.ZoneGatherResourcesWindow.DisplayWindow.IsVisibleAndActive)
            {
                this.ZoneGatherResourcesWindow.Refresh();
            }

            if (this.huntWindow.DisplayWindow.IsVisibleAndActive)
            {
                this.huntWindow.Refresh();
            }
        }

        /// <summary>
        /// test the selection area and set buttons appropriately
        /// 
        /// </summary>
        public void RefreshThisPanel()
        {
            MapArea mapArea;
            mapArea = TileSelectionContextMenu.GetMapArea();

            if (mapArea.Count == 0 // this has happened once. the selected tiles were gone, not sure how. let's close the window if it happens again.
                || mapArea.BoundingRectangle == null) // new... check this value also
            {
                Hide();

                return;
            }

            if (mapArea.Zone != null)
            {               
                tbHunt.IsChecked = mapArea.Zone.ZoneHunt.HasHuntOrders() || mapArea.Zone.ZoneHunt.HasFindPreyJobs(); 
                tbScout.IsChecked = mapArea.Zone.ScoutingJob != null;
                tbForage.IsChecked = mapArea.Zone.ExamineJob != null;
                tbStockpile.IsChecked = mapArea.Zone.Stockpile != null;
                tbPatrol.IsChecked = mapArea.Zone.PatrolJob != null;
                tbAttack.IsChecked = mapArea.Zone.AttackAreaJob != null;

                RemoveEntityIcons();

                if (mapArea.Zone.HarvestJobs.Count > 0)
                {

                    int noOfResourcesDisplayed = 0;
                    int nextXPos = tbGather.Right + 2; // singleSpacing;

                    foreach (var resource in mapArea.Zone.HarvestJobs)
                    {
                        if (resource.Value.Count > 0)
                        {
                            tbGather.IsChecked = true;

                            Image icon = harvestResourceImages[noOfResourcesDisplayed];
                            //Rectangle rect = GameData.Instance.BillboardSpriteSheet.SourceRectangle(resource.Key.ResourceItem.RenderAsBillboardType[0].AssetName); // The.Sim.GUISpriteSheet.SourceRectangle(itemTypeInCategory.IconSpriteName); //SpriteName);
                            IconInfo iconInfo;
                            Rectangle rect = resource.Key.ResourceItemType.GetIconSprite(out iconInfo);
                            icon.SetSkinLocation(SkinState.Normal,rect);
                            icon.Texture = gui.GUISpriteSheet.Texture; 
                            icon.ResizeControlToFitImage();

                            Add(icon);

                            icon.X = nextXPos;
                            int yOffset = 0;
                            if (iconInfo != null)
                            {
                                yOffset = iconInfo.GetYPosAdjustment(rect.Height);
                            }
                         //   icon.Y = tbGather.Y + 2 + yOffset; 
                            icon.Y = tbGather.Y + tbGather.Height / 2 - rect.Height / 2 + yOffset;

                            nextXPos = icon.Right + 4; 

                          //  DisplayWindow.CenterChildVertically(icon, tbGather.Y + tbGather.Height / 2); // iconInfo != null ? iconInfo.CenterYPos : null);
                            /*
                            if (xPosToCenterAbout.HasValue)
                            {
                                item.CenterHorizontally(xPosToCenterAbout.Value, icon);
                            }*/


                            noOfResourcesDisplayed++;

                            if (noOfResourcesDisplayed >= 3)
                            {
                                break;
                            }
                        }

                    }
                }

                /* // Show icons for stockpile settings...
                 * 
                Stockpile stockpile = mapArea.Zone.Stockpile;
                if (stockpile != null)
                {
                    // show up to 3 stockpiled items icons
                    // this will also show icons for undiscovered items...
                    // show categories also..? how?
                    if (stockpile.HasItemSettings()) // .mayStockpileItem.Count > 0)
                    {
                        int noOfIcons = 0;
                        int nextXPos = tbStockpile.Right + 2; // singleSpacing;

                        foreach (var item in stockpile.mayStockpileItem)
                        {
                            bool showItem = InventoryPanel.OwnsProductOrHasProcessInputsAndTools(entityType, owner, allAvailableItems);

                            if (item.Value == true)
                            {
                                Image icon = stockpileImages[noOfIcons];
                                Rectangle rect = InventoryPanel.GetItemSprite(item.Key);
                                icon.Texture = gui.GUISpriteSheet.Texture;
                                icon.SetSkinLocation(SkinState.Normal,rect);

                                icon.ResizeControlToFitImage();

                                Add(icon);

                                icon.X = nextXPos;
                                icon.Y = tbStockpile.Y + 2;

                                nextXPos = icon.Right + 4; // singleSpacing;

                                noOfIcons++;

                                if (noOfIcons >= 3)
                                {
                                    break;
                                }
                            }
                        }
                    }

                }*/

            }
            else
            {
                tbPatrol.IsChecked = tbHunt.IsChecked = tbForage.IsChecked = tbScout.IsChecked = tbStockpile.IsChecked = tbGather.IsChecked = tbAttack.IsChecked = false;

                RemoveEntityIcons();


                //lblZone.Text = "";
                //lblZone.FitToText();
            }

            btDelete.Visible = mapArea.Zone != null;

            bool overlaps = StockpileWindow.OverlapsWithOtherStockpiles(mapArea);

            if (overlaps)
            {
                tbStockpile.Enabled = false;
                tbStockpile.ToolTip = overlapsWarning;

                //Remove(tbStockpile);

                // Add(lblStockpileOverlaps);
                // lblStockpileOverlaps.X = tbStockpile.X;
                //  lblStockpileOverlaps.Y = tbStockpile.Y;
            }


            // Remove(lblNotConnected);

            List<TerrainTile> newList = null;
            bool isConnected = mapArea.CheckConnectivity(false, ref newList);
            // stockpile demands a connected area
            if (!isConnected)
            {
                // there is a crash bug so we are not showing this button now...
              //  Add(tbConnect);

                if (!overlaps)
                {
                    tbStockpile.Enabled = false;
                    tbStockpile.ToolTip = notConnectedWarning;
                }
            }



            if (!overlaps && isConnected)
            {
                tbStockpile.Enabled = true;

                tbStockpile.ToolTip = stockpileTooltip;

                /*if (!DisplayWindow.Controls.Contains(tbStockpile))
                {
                    Add(tbStockpile);
                }*/
            }
        }

        private void RemoveEntityIcons()
        {
            for (int i = 0; i < harvestResourceImages.Length; i++)
            {
                Remove(harvestResourceImages[i]);
            }

            for (int i = 0; i < stockpileImages.Length; i++)
            {
                Remove(stockpileImages[i]);
            }
        }

     /*   public static Owner GetMapAreaOwner()
        {
           
            if (The.InGameUI.SelectedZone != null)
            {
                return The.InGameUI.SelectedZone.Owner;                
            }
            else
            {
                // TODO when more expeditions - get closest...
                Owner expeditionOwner = The.Sim.Site.GetFirstPlayerExpedition().ExpeditionOwner;

                return expeditionOwner;
            }
          
        }*/

     

        public static MapArea GetMapArea()
        {
            MapArea mapArea;
            if (The.InGameUI.SelectedZone != null)
            {
                mapArea = The.InGameUI.SelectedZone.MapArea;
            }
            else
            {
                mapArea = The.InGameUI.SelectedTiles;
            }
            return mapArea;
        }


        public override void Hide()
        {
            base.Hide();
            //DisplayWindow.Hide();

            OpeningWindow = null;

            HideChildWindows();
        }

        void btModify_Click(UIComponent sender, EventArgs e)
        {

        }


        public static int GetXPositionOfChildWindow(Window window)
        {
            return window.X + window.Width - 4;
        }

        void btGather_Click(UIComponent sender, EventArgs e)
        {
            HideChildWindows();

            ZoneGatherResourcesWindow.ShowOnPlayfield(GetXPositionOfChildWindow(base.DisplayWindow), base.DisplayWindow.Y);

            //TODO: only populate on SCAN click when the selection is large
            ZoneGatherResourcesWindow.Refresh();
        }

        void btStockpile_Click(UIComponent sender, EventArgs e)
        {
            HideChildWindows();

            MapArea mapArea = TileSelectionContextMenu.GetMapArea();
            if (mapArea != null)
            {            
                zoneStockpileWindow.ShowOnPlayfield(GetXPositionOfChildWindow(base.DisplayWindow), base.DisplayWindow.Y);

                zoneStockpileWindow.FillFromArea();
            }
        }


        void btCycle_Click(UIComponent sender, EventArgs e)
        {
            ContextMenuOpener.CycleEntities(GetMapArea());
        }

        private void HideChildWindows()
        {
            ZoneGatherResourcesWindow.Hide();
            zoneStockpileWindow.Hide();
            patrolWindow.Hide();
            huntWindow.Hide();
            attackWindow.Hide();
        }

        public static bool GetExpedition(out EntityGroupID expeditionGroupID)
        {
            Expedition expedition = The.InGameUI.GetExpedition();

            if (expedition != null)
            {
                expeditionGroupID = expedition.OwnedEntities.ID;

                return true;
            }

            expeditionGroupID = EntityGroupID.Invalid;

            return false;
        }

        void btScout_Click(UIComponent sender, EventArgs e)
        {

            //This would never let us use an already existing zone? So we cannot add several jobs to same zone.
            if (CanCreateAndSelectZone())
            {
                if (tbScout.IsChecked)
                {
                    Command scoutCommand; 
                    EntityGroupID groupID;

                    if (GetExpedition(out groupID))
                    {

                        if (The.InGameUI.SelectedZone == null)
                        {
                            scoutCommand = new Scout(The.InGameUI.SelectedTiles, true, groupID);
                        }
                        else
                        {
                            scoutCommand = new Scout(The.InGameUI.SelectedZone.ID, true, groupID);
                        }

                        The.Client.Controller.StoreAndExecuteCommand(scoutCommand);
                    }
                }
                else
                {
                    //Cancel job????
                }
            }
            
            //    {
            //Expedition expedition = The.Sim.Site.GetMainExpedition();
            //Owner expeditionOwner = expedition.ExpeditionOwner;

            //if (CreateAndSelectZone(expeditionOwner))
            //{
            //    if (tbScout.IsChecked)
            //    {

            //        // create scout job for area:

            //        The.InGameUI.SelectedZone.ScoutingJob = new ScoutingJob(The.InGameUI.SelectedZone, expeditionOwner.InternalOwner.OwnerContent.ScoutingJobs, expedition.PlayerSetWorkPriority); //The.Sim.Site.PlayerAllegiance.ScoutingJobs);
            //    }
            //    else
            //    {
            //        // cancel job:
            //        The.InGameUI.SelectedZone.ScoutingJob.Destroy(true);
            //    }

            //    //ScoutingJob scoutingJob = new ScoutingJob();
            //    //ScoutingJob job = new ScoutingJob(location, The.Sim.Site.PlayerAllegiance.ScoutingJobs);

            //    //pJob.HarvestJob = new HarvestJob(pJob, items[items.Count - 1] /*itemToGather*/, expeditionOwner);

            //}

            //Hide();
        }

        

        private void SelectZone(Zone zoneToSelect)
        {
            The.InGameUI.SelectedZone = zoneToSelect;
            zoneToSelect.MapArea.MapAreaRender.IsSelected = true;
        }

        public void OnScoutArea(Zone zoneToScout)
        {
            SelectZone(zoneToScout);

            Hide();
        }

        public void OnCreateStockpile(Zone zone)
        {
            if (zone != null)
            {
                SelectZone(zone);
            }

            Hide();
        }


        void tbPatrol_Click(UIComponent sender, EventArgs e)
        {
            if (CanCreateAndSelectZone())
            {
                HideChildWindows();

                MapArea mapArea = TileSelectionContextMenu.GetMapArea();
                if (mapArea != null)
                {
                    patrolWindow.ShowOnPlayfield(GetXPositionOfChildWindow(base.DisplayWindow), base.DisplayWindow.Y);

                    //  patrolWindow.FillFromArea();
                }
            }
        }

        void tbAttack_Click(UIComponent sender, EventArgs e)
        {
            if (CanCreateAndSelectZone())
            {
                HideChildWindows();

                MapArea mapArea = TileSelectionContextMenu.GetMapArea();
                if (mapArea != null)
                {
                    attackWindow.ShowOnPlayfield(GetXPositionOfChildWindow(base.DisplayWindow), base.DisplayWindow.Y);
                }
            }
        }

        void tbHunt_Click(UIComponent sender, EventArgs e)
        {
            if (CanCreateAndSelectZone())
            {
                HideChildWindows();

                huntWindow.ShowOnPlayfield(GetXPositionOfChildWindow(base.DisplayWindow), base.DisplayWindow.Y);

                huntWindow.Refresh();
            }
        }

        /*
        void btHunt_Click(UIComponent sender, EventArgs e)
        {
            if (CanCreateAndSelectZone())
            {
                if (tbFindPrey.IsChecked)
                {
                    EntityGroupID groupID;
                    if (GetExpedition(out groupID))
                    {
                        Command huntCommand;
                        
                        if (The.InGameUI.SelectedZone != null)
                        {
                            huntCommand = new HuntArea(The.InGameUI.SelectedZone.ID, true, groupID);

                        }
                        else
                        {
                            huntCommand = new HuntArea(The.InGameUI.SelectedTiles, true, groupID);

                        }
                        The.Client.Controller.StoreAndExecuteCommand(huntCommand);
                    }
                }
                else
                {
                    //Cancel job????
                }
            }          
        }*/
              

        

        public void OnHuntArea(Zone zoneToHuntIn)
        {
            SelectZone(zoneToHuntIn);

            Hide();
        }

        void btExamine_Click(UIComponent sender, EventArgs e)
        {
            if (CanCreateAndSelectZone())
            {
                if (tbForage.IsChecked)
                {
                    EntityGroupID groupID;
                    if (GetExpedition(out groupID))
                    {
                        Command forageCommand;
                        if (The.InGameUI.SelectedZone != null)
                        {
                            forageCommand = new Examine(The.InGameUI.SelectedZone.ID, true, groupID);
                        }
                        else
                        {
                            forageCommand = new Examine(The.InGameUI.SelectedTiles, true, groupID);
                        }

                        The.Client.Controller.StoreAndExecuteCommand(forageCommand);
                    }

                }
                else
                {
                    //Cancel job????
                }
            }
            //Expedition expedition = The.Sim.Site.GetMainExpedition();
            //Owner expeditionOwner = expedition.ExpeditionOwner;

            //if (CreateAndSelectZone(expeditionOwner))
            //{
            //    //  The.InGameUI.SelectedZone.Forage = tbForage.IsChecked;

            //    if (tbForage.IsChecked)
            //    {
            //        The.InGameUI.SelectedZone.ForageJob = new ScoutingJob(The.InGameUI.SelectedZone, expeditionOwner.InternalOwner.OwnerContent.ScoutingJobs, expedition.PlayerSetWorkPriority); // The.Sim.Site.PlayerAllegiance.ScoutingJobs);
            //        The.InGameUI.SelectedZone.ForageJob.ForageForResources = true;
            //    }
            //    else
            //    {
            //        // cancel job:
            //        The.InGameUI.SelectedZone.ForageJob.Destroy(true);
            //    }

            //}

            //Hide();
        }

        

        public void OnForageArea(Zone zoneToForage)
        {
            SelectZone(zoneToForage);

            Hide();
        }

        public void OnSetStandingGatherOrder(Zone zone)
        {
            // select the zone, but do not hide the window. there may be more commands in the batch
            SelectZone(zone);
         
        }

        public void OnGather(Zone zone)
        {
            // select the zone, but do not hide the window. there may be more gather commands in the batch
            SelectZone(zone);

          //  Hide();
        }
    }
}
