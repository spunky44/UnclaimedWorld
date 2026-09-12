using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;
using UWGame.SimSide.Maps;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.AI;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Resources;
using UWGame.SimSide.AI.Goals;

namespace UWGame.ClientSide.Interface.HUD_Windows
{
    /// <summary>
    /// a little icon that marks a zone,entity or expedition center
    /// </summary>
    public class MarkerWindow : HUDWindow
    {
        public enum MarkerType
        { 
            Zone,
            Expedition,
            Entity,
            None
        }

        private static int WindowHeight = 24;
        private Label markerLabel;
        private ImageButton buttonExpand;
        private Point? screenPosition;

             

        private bool showWhileLineIsSpoken;

        /// <summary>
        /// only set true while a line is being spoken by the entity - this means the window must not be hidden
        /// </summary>
        public bool ShowWhileLineIsSpoken
        {
            get
            {
                return showWhileLineIsSpoken;
            }
            set
            {
                showWhileLineIsSpoken = value;

                if (showWhileLineIsSpoken == true)
                {
                    ShowWindow();
                }
                else
                {
                    if (DisplayWindow.IsVisibleAndActive == false)  // don't hide without retiring!
                    {
                        HideWindow();
                    }
                }
            }
        }

        /// <summary>
        /// only filled for Zone and Expedition... why not use HUDWindow.WorldPosition???
        /// </summary>
        public Vector2 MarkerWorldPosition
        { 
            get
            {
                return markerWorldPosition;
            }
        }
        private Vector2 markerWorldPosition;

        private MarkerType currentType = MarkerType.None;

        UIComponent zonePanel, entityPanel;//, expeditionPanel;
        
        // entityPanel contains:
        // HorizontalList statusIcons - don't clear when the window goes back into the pool. the content in the list will get updated when the window is in use again for a dfferent entity.
        // this is cheaper than creating the status icons again.

        private Image imGather, imStockpile, imScout, imPatrol, imHunt, imForage, imAttack;
        public Zone Zone;
        private const int iconSpacing = 2;
        //

        //Entity 
        private EntityID? entityID = null;
        /// <summary>
        /// We use this property when we want to set the entityID, it will do all the things necessary after the change
        /// </summary>
        public EntityID? EntityID
        {
            set
            {
                if (entityID != value)
                {
                    entityID = value;
                    activityWindow.owner = value;
                    Refresh();
                }
            }
        }
        private HorizontalList entityHorizontalList;
        private EntityActivityHUDWindow activityWindow;

        private const string personBackgroundSprite = "HUD_windowCharacter_base_small";
        private const string otherAllegianceBackgroundSprite = "HUD_windowRed_base_small";
        private const string specialSiteBackgroundSprite = "HUD_windowOrange_base_small";
        private const string defaultBackgroundSprite = "HUD_window_base_small";
       
        private Point offsetFromEntity = new Point(0, 90);
        
        /// <summary>
        /// progress flag. could be avoided with better code structure in InGameInterface 
        /// </summary>
        public bool IsRenewedThisFrame; //??

        //
        //Expedition
        public Expedition Expedition;
        

        public MarkerWindow()
            : base(100, WindowHeight, true)
        {
            markerLabel = new Label(gui);
            Add(markerLabel);

            buttonExpand = new ImageButton(gui);
            buttonExpand.Init(ImageButtonType.HUDArrowRight);
           // buttonExpand.MouseOver += new MouseOverHandler(bt_MouseOver);
            buttonExpand.Click += new ClickHandler(expandButton_Click);
            buttonExpand.X = base.DisplayWindow.Width - buttonExpand.Width - sideMargin;
            buttonExpand.ToolTip = "Click to see the actions that can be taken";
            buttonExpand.ZOrder = 1f;
            DisplayWindow.CenterChildVertically(buttonExpand);


           // DisplayWindow.MouseOver += new MouseOverHandler(DisplayWindow_MouseOver);
            DisplayWindow.ViewPort.MouseOver += new MouseOverHandler(ViewPort_MouseOver);
            DisplayWindow.ViewPort.MouseOut += new MouseOutHandler(ViewPort_MouseOut);

        }

        void ViewPort_MouseOut(UIComponent sender, InputEventSystem.MouseEventArgs args)
        {
            The.InGameUI.SetHoverEntity(null); // MarkerWindowHoverEntity = null;

          /*  if (entityID.HasValue)
            {                
                if (The.InGameUI.HoverEntity.EntityID == entityID.Value)
                {
                    The.InGameUI.HoverEntity = null;
                }
            }*/

        }

        void ViewPort_MouseOver(UIComponent sender, InputEventSystem.MouseEventArgs args)
        {
            if (entityID.HasValue)
            {
                IKnownEntityData entityData;
                The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(entityID.Value, out entityData);
                if (entityData != null)
                {
                    The.InGameUI.SetHoverEntity(entityData.EntityID); //MarkerWindowHoverEntity = entityData;
                }
            }
        }

       
        
        public void Reset()
        {
            ChangeTypeToNone();

            showWhileLineIsSpoken = false; // NEW

            EntityID = null;
            Zone = null;
            Expedition = null;
            WorldPosition = null;
        }

        public void ChangeTypeToNone()
        {
            ChangeType(MarkerType.None);
        }

        public void FillWithZoneInfo(Zone zone)
        {
            ChangeType(MarkerType.Zone);
            
            FillFromZone(zone);

            ChangeBackground();

            Add(zonePanel);
            Refresh();
        }

        public void FillWithEntityInfo(EntityID entityID)
        {
            ChangeType(MarkerType.Entity);

            FillFromEntity(entityID);

            //HUD_windowCharacter_base	
            ChangeBackground();

            Add(entityPanel);
            Refresh();
        }


        public bool EntityIsValid() // OffSite()
        {
            IKnownEntityData entityData;
            The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(entityID.Value, out entityData);
            if (entityData != null)
            {
                if (GoalEvaluator.IsOnPlaySite(entityData))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return false;
        }


        private void ChangeBackground()//sets the background to match the marker data
        {
            string surfaceSprite = defaultBackgroundSprite;
            switch (currentType)
            {
                case MarkerType.Entity:
                    {
                        IKnownEntityData entityData;
                        The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(entityID.Value, out entityData);

                        //Entity entityWithStatus = The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownDataAsEntity(entityID.Value); // Entity.Value);
                        if (entityData != null)
                        {
                            if (entityData.AllegianceID == The.InGameUI.UIAllegiance.ID)
                            {
                                surfaceSprite = personBackgroundSprite;
                            }
                            else if (entityData.EntityType.IntelligenceType != null
                                || entityData.EntityType.ThreatType != null)
                            //entityWithStatus.EntityType.ThreatCategory == ThreatCategory.Predator)
                            {
                                surfaceSprite = otherAllegianceBackgroundSprite;
                            }
                            else if (entityData.EntityType.TerrainType != null
                                /* && entityData.EntityType.SharedSpecialActionTypes != null && entityData.EntityType.SharedSpecialActionTypes.Count > 0*/)
                            {
                                 surfaceSprite = specialSiteBackgroundSprite;
                            }
                            else
                            {
                                surfaceSprite = defaultBackgroundSprite;
                            }
                        }
                        break;
                    }
                case MarkerType.Zone:
                    {
                        surfaceSprite = defaultBackgroundSprite;
                        break;
                    }
                case MarkerType.Expedition:
                    {
                        surfaceSprite = defaultBackgroundSprite;
                        break;
                    }
            }

            ChangeSurface(surfaceSprite);
        }

        public void FillWithExpeditionInfo(Expedition expedition)
        {
            ChangeType(MarkerType.Expedition);

            FillFromExpedition(expedition);

            ChangeBackground();
            Refresh();
        }

        private void ChangeType(MarkerType typeToChangeTo)
        {
            if(currentType == MarkerType.Entity)
            {
                Remove(entityPanel);
                Remove(buttonExpand);
                if (The.InGameUI.HUDActionPanel.IsShowingEntity(entityID.Value))
                {
                    // close when re-clicked:
                    The.InGameUI.HUDActionPanel.Hide();
                }

                activityWindow.Hide();                
            }
            else if(currentType == MarkerType.Expedition)
            {
                Remove(buttonExpand);
            }
            else if (currentType == MarkerType.Zone)
            {
                Remove(zonePanel);
                Remove(buttonExpand);
            }

            currentType = typeToChangeTo;
        }

        private void FillFromZone(Zone zone)
        {
            this.Zone = zone;
            
            int xPos = sideMargin;

            Zone.ZoneOrdersChanged += new EventHandler(Zone_ZoneOrdersChanged);

            //markerLabel.Init(Label.LabelType.HUDWindowHeader);
            //markerLabel.X = xPos;
            if (markerLabel.Text != "")
            {
                //Cleanup markerLabel text if this window gets reused. We do not want to have a label for zones.
                markerLabel.Text = "";
            }
            //markerLabel.Text = "";// Zone.GetDisplayName();
            //markerLabel.FitToText();
            //DisplayWindow.CenterVertically(markerLabel);

            TerrainTile tile = Zone.MapArea.BottomLeftTile;

            markerWorldPosition = MapManager.TilePosToWorldPos(tile.TilePos).ToVector2();
            markerWorldPosition.X -= MapManager.tileSizeOver2;
            markerWorldPosition.Y += MapManager.tileSizeOver2;
            
            Vector2 screenPos = The.MapUI.TilePosToScreen(new Point(tile.X, tile.Y));
            screenPos.X -= MapManager.tileSizeOver2;
            screenPos.Y += MapManager.tileSizeOver2;
            
            screenPosition = screenPos.ToPoint();

            if (zonePanel == null)
            {
                zonePanel = new UIComponent(gui);
                zonePanel.DebugTag = "zonePanel";

                Rectangle rect;

                imGather = new Image(gui);
                imGather.Texture = gui.GUISpriteSheet.Texture;
                rect = gui.GUISpriteSheet.GetSourceRectangle("HUD_icon_gather");
                imGather.SetSkinLocation(SkinState.Normal,rect);
                imGather.ResizeControlToFitImage();
                //  imGather.Y = iconY;
                DisplayWindow.CenterChildVertically(imGather);

                imStockpile = new Image(gui);
                imStockpile.Texture = gui.GUISpriteSheet.Texture;
                rect = gui.GUISpriteSheet.GetSourceRectangle("HUD_icon_stockpile");
                imStockpile.SetSkinLocation(SkinState.Normal,rect);
                imStockpile.ResizeControlToFitImage();

                DisplayWindow.CenterChildVertically(imStockpile);


                imHunt = new Image(gui);
                imHunt.Texture = gui.GUISpriteSheet.Texture;
                rect = gui.GUISpriteSheet.GetSourceRectangle("HUD_icon_hunt");
                imHunt.SetSkinLocation(SkinState.Normal,rect);
                imHunt.ResizeControlToFitImage();

                DisplayWindow.CenterChildVertically(imHunt);

                imScout = new Image(gui);
                imScout.Texture = gui.GUISpriteSheet.Texture;
                rect = gui.GUISpriteSheet.GetSourceRectangle("HUD_icon_scout");
                imScout.SetSkinLocation(SkinState.Normal,rect);
                imScout.ResizeControlToFitImage();

                DisplayWindow.CenterChildVertically(imScout);

                imForage = new Image(gui);
                imForage.Texture = gui.GUISpriteSheet.Texture;
                rect = gui.GUISpriteSheet.GetSourceRectangle("HUD_icon_forage");
                imForage.SetSkinLocation(SkinState.Normal,rect);
                imForage.ResizeControlToFitImage();

                DisplayWindow.CenterChildVertically(imForage);

                imPatrol = new Image(gui);
                imPatrol.Texture = gui.GUISpriteSheet.Texture;
                rect = gui.GUISpriteSheet.GetSourceRectangle("HUD_icon_patrol");
                imPatrol.SetSkinLocation(SkinState.Normal,rect);
                imPatrol.ResizeControlToFitImage();

                DisplayWindow.CenterChildVertically(imPatrol);

                imAttack = new Image(gui);
                imAttack.Texture = gui.GUISpriteSheet.Texture;
                rect = gui.GUISpriteSheet.GetSourceRectangle("HUD_icon_sword");
                imAttack.SetSkinLocation(SkinState.Normal, rect);
                imAttack.ResizeControlToFitImage();

                DisplayWindow.CenterChildVertically(imAttack);
            }
            Refresh();
        }
        
        private void FillFromEntity(EntityID entityID)
        {
            if (entityPanel == null)
            {
                entityPanel = new UIComponent(gui);
                
                /* // TODO: add a horizontal list for each category, needs an outer container too...
                // the set of categories never changes, so construct an entry component for each
                foreach (PresentationTypeCategory presentationTypeCategory in GameData.Instance.CustomStatusIconData.PresentationTypeCategories) 
                {
                    CollapsablePanel newCollapsablePanel;
                    Grid newGrid;

                    iconList = new HorizontalList(The.InGameUI.gui, 1);

                    customPanels.Add(new CategoryPanelAndData(newCollapsablePanel, newGrid, presentationTypeCategory));
                }*/

                // 
                entityHorizontalList = new HorizontalList(The.InGameUI.gui, 2); //now that we don't support categories, let's allow 2 icons to be displayed, like the progress bar and a status icon // 1);
                entityHorizontalList.CenterItemsVertically = true;
              //  entityHorizontalList.Height = 2;
               // entityHorizontalList.Y = 2;
                entityHorizontalList.HorizontalSpacing = -2;

                entityHorizontalList.Y = 0;
              
                entityPanel.Add(entityHorizontalList);
                entityPanel.Height = WindowHeight;
                entityHorizontalList.Height = WindowHeight;
                entityHorizontalList.MinHeight = WindowHeight; // don't scale the height by its contents. this makes centering icons easier
                entityHorizontalList.MaxHeight = WindowHeight; // don't scale the height by its contents.
                entityHorizontalList.DebugTag = "statusIcons";

                activityWindow = new EntityActivityHUDWindow(new Point(0, 20), 3);
               
            }

            EntityID = entityID;
        }
        
        private void FillFromExpedition(Expedition expedition)
        {
            Expedition = expedition;
            markerWorldPosition = Expedition.Location.Value.ToVector2();
            screenPosition = The.MapUI.WorldPosToScreenPoint(markerWorldPosition);

            int xPos = sideMargin;
            Add(markerLabel);
            markerLabel.Init(Label.LabelType.HUDWindow);
            markerLabel.X = xPos;
            markerLabel.Text = expedition.Name; // "Camp";
            markerLabel.FitToText();

            Add(buttonExpand);//always present for the expedition
            buttonExpand.X = markerLabel.Right;

            DisplayWindow.Width = markerLabel.Width + 2 * sideMargin + buttonExpand.Width;
            DisplayWindow.CenterChildVertically(markerLabel);
        }
        
        void Zone_ZoneOrdersChanged(object sender, EventArgs e)
        {
            Refresh();
        }

        public void Show()
        {
            ShowWindow();
            //isHiddenByDefault = false;
        }

        private void ShowWindow()
        {
            switch (currentType)
            {
                case MarkerType.Entity:
                    {
                        base.DisplayWindow.ViewPort.Click += new ClickHandler(OnMarkerClick);
                        
                        IKnownEntityData entityData;
                        UpdatePosition(out entityData);                        
                        //base.ShowInScreenSpace(screenPosition.Value.X, screenPosition.Value.Y);
                        activityWindow.Show();

                        break;
                    }
                case MarkerType.Expedition:
                    {
                        markerWorldPosition = Expedition.Location.Value.ToVector2();
                        screenPosition = The.MapUI.WorldPosToScreenPoint(markerWorldPosition);

                        break;
                    }
                case MarkerType.Zone:
                    {
                        TerrainTile tile = Zone.MapArea.BottomLeftTile;

                        Vector2 screenPos = The.MapUI.TilePosToScreen(new Point(tile.X, tile.Y));
                        screenPos.X -= MapManager.tileSizeOver2;
                        screenPos.Y += MapManager.tileSizeOver2;
                        screenPosition = screenPos.ToPoint();
                        break;
                    }
            }

            base.ShowOnPlayfield(screenPosition.Value.X, screenPosition.Value.Y);
        }
        
        private void UpdatePosition(out IKnownEntityData entityData)
        {
            if (entityID.HasValue == true)
            {
                The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(entityID.Value, out entityData);

                if (entityData != null)
                {
                    markerWorldPosition = entityData.RenderedLocation.ToVector2();
                    markerWorldPosition.X -= offsetFromEntity.X;
                    markerWorldPosition.Y -= offsetFromEntity.Y;

                    Point pos = The.MapUI.WorldPosToScreenPoint(markerWorldPosition);

                    /*pos.Y -= offsetFromeEntity.Y;
                    pos.X -= offsetFromeEntity.X;*/

                    screenPosition = pos;                    
                }
            }
            else
            {
                entityData = null;
            }
        }

        public override void Refresh()
        {
            if (currentType == MarkerType.Zone)
            {
                RefreshZoneMarker();
            }
            else if (currentType == MarkerType.Entity)
            {
                RefreshEntityMarker();
            }

            //The information for the expedition doesn´t change unlees we move camp
        }

        private void RefreshEntityMarker()
        {
            activityWindow.Refresh();

            entityPanel.X = sideMargin;
            entityPanel.Y = sideMargin;

            markerLabel.X = 0;
            markerLabel.Y = 0;

            IKnownEntityData entityWithStatus;
            The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(entityID.Value, out entityWithStatus);


            if (entityWithStatus != null)
            {              
                RefreshIconList(entityWithStatus);
               
                entityPanel.Width = entityHorizontalList.Width;
                DisplayWindow.CenterChildVertically(entityPanel);

                RefreshLabelWithEntity(entityWithStatus);
              
                bool canBeSalvaged;
                bool canBeHunted;
                bool canBeDiscarded;
                bool canBeClaimed;
                bool canBeUpgraded;
                bool hasSpecialActions;
             //   bool hasEnabledSpecialActions;
                bool setStockpile;
                bool setTradeOffers;
                IKnownEntityData knownData;

                if (HUDEntityContextMenu.EntityHasActions(out canBeSalvaged, out canBeHunted, out canBeDiscarded, out canBeClaimed, out hasSpecialActions, /*out hasEnabledSpecialActions,*/ out setStockpile, out setTradeOffers, out canBeUpgraded, out knownData, entityID))
                {
                    AddExpandButtonForEntityMarker();
                }
                else
                {
                    Remove(buttonExpand);
                    base.DisplayWindow.Width = markerLabel.Right + sideMargin;
                }

                offsetFromEntity.X = DisplayWindow.Width / 2;
            }
            else
            {
                Remove(buttonExpand);
                base.DisplayWindow.ViewPort.Click -= new ClickHandler(OnMarkerClick);
            }
        }

        private void AddExpandButtonForEntityMarker()
        {
            Add(buttonExpand);
            DisplayWindow.CenterChildVertically(buttonExpand);
            buttonExpand.X = markerLabel.X + markerLabel.TextWidth;

            base.DisplayWindow.Width = buttonExpand.Right + sideMargin;
        }

        public void RefreshIconList(IKnownEntityData entityWithStatus)
        {
            IHasExposedProperties hasExposedProperties = null;

            hasExposedProperties = entityWithStatus as IHasExposedProperties;

            foreach (PresentationTypeCategory presentationTypeCategory in GameData.Instance.CustomStatusIconData.FinalPresentationTypeCategories)
            { 
                int? numberOfItems = null;
                PresentationTypeCategoryProcessor.DisplayCategory(presentationTypeCategory, hasExposedProperties, entityHorizontalList, ref numberOfItems);
            }
        }

        /// <summary>
        /// checks if there would be any status icons to show for this entity/memoryfact (so the marker winodw should appear)
        /// </summary>
        /// <param name="hasExposedProperties"></param>
        /// <returns></returns>
        public static bool HasStatusIconsToShow(IHasExposedProperties hasExposedProperties)
        {
            foreach (PresentationTypeCategory presentationTypeCategory in GameData.Instance.CustomStatusIconData.FinalPresentationTypeCategories)
            {                
                if (PresentationTypeCategoryProcessor.KeyedEntryComponentHasDataToShow(presentationTypeCategory, hasExposedProperties, HorizontalList.CanProcessEntryDataStatic))
                {
                    return true;
                }
            }
            return false;
        }
       
        public void RefreshLabelWithEntity(IKnownEntityData entityWithStatus)
        {
            markerLabel.X += entityPanel.Position.X + entityPanel.Width;

            if (The.InGameUI.SelectedEntity.HasValue)
            {
                if (The.InGameUI.SelectedEntity.Value == entityWithStatus.EntityID)
                {
                    markerLabel.Init(Label.LabelType.HUDWindowHeader);
                }
                else
                {
                    markerLabel.Init(Label.LabelType.HUDWindow);
                }
            }
            else
            {
                markerLabel.Init(Label.LabelType.HUDWindow);
            }

            string name = entityWithStatus.GetDisplayName(); // entityWithStatus.EntityType.Name; 
            if (entityWithStatus.EntityType.IntelligenceType != null)
            {
                Entity entity = entityWithStatus as Entity;
                if (entity != null && entity.Intelligence.LastName != null)
                {
                    name = entity.Intelligence.LastName;
                }
                // for people from other allegiances - save the last name in memory fact
            }
           

            markerLabel.Text = name;

          /*  if (entityWithStatus is Entity)
            {
                Entity entity = entityWithStatus as Entity;
                if (entity.EntityType.Person == null)
                {
                    markerLabel.Text = entity.EntityType.Name; //entity.GetName();
                }
                else
                {
                    markerLabel.Text = entity.PersonEntity.LastName;
                }
            }
            else if (entityWithStatus is MemoryFact)
            {
                MemoryFact memory = entityWithStatus as MemoryFact;

                markerLabel.Text = memory.GetName();
            }*/
            
            markerLabel.FitToText();
            DisplayWindow.CenterChildVertically(markerLabel);
        }

        void OnMarkerClick(UIComponent sender, EventArgs e)
        {
            if(currentType == MarkerType.Entity)
            {
                The.InGameUI.SelectEntity(entityID.Value);
            }
            //this.DisplayWindow.ZOrder = 0.1f;
            this.DisplayWindow.BringToTop();
        }

        public void RefreshZoneMarker()
        {
            //Perhaps use the HorizontalList to handle this functionallity?
            int xPos = sideMargin + iconSpacing + 4;
            zonePanel.X = xPos;
            xPos = 0;
            zonePanel.Remove(imStockpile);
            zonePanel.Remove(imGather);
            zonePanel.Remove(imHunt);
            zonePanel.Remove(imScout);
            zonePanel.Remove(imForage);
            zonePanel.Remove(imPatrol);
            zonePanel.Remove(imAttack);
            zonePanel.Width = 0;

            if (Zone.Stockpile != null)
            {
                zonePanel.Add(imStockpile);
                imStockpile.X = xPos;
                xPos = imStockpile.Right + iconSpacing;

                zonePanel.Width += imStockpile.Width+iconSpacing;
            }

            if (Zone.HasHarvestJobs()
                || Zone.AllowStandingOrderHarvest.Count > 0)
            {
                zonePanel.Add(imGather);

                if (Zone.HasHarvestJobs())
                {
                    SetJobIconColor(Zone.HarvestJobs, imGather);
                }

                imGather.X = xPos;
                xPos = imGather.Right + iconSpacing;

                zonePanel.Width += imGather.Width + iconSpacing;
            }

            if (Zone.ZoneHunt.HasFindPreyJobs()
                || Zone.ZoneHunt.HasHuntOrders())
            {
                zonePanel.Add(imHunt);

                SetJobIconColor(Zone.ZoneHunt.FindPreyJobs, imHunt);
                imHunt.X = xPos;
                xPos = imHunt.Right + iconSpacing;

                zonePanel.Width += imHunt.Width + iconSpacing;
            }

            if (Zone.PatrolJob != null)
            {
                zonePanel.Add(imPatrol);
                SetJobIconColor(Zone.PatrolJob.ID, imPatrol);
                imPatrol.X = xPos;
                xPos = imPatrol.Right + iconSpacing;

                zonePanel.Width += imPatrol.Width + iconSpacing;
            }

            if (Zone.AttackAreaJob != null)
            {
                zonePanel.Add(imAttack);
                SetJobIconColor(Zone.AttackAreaJob.ID, imAttack);
                imAttack.X = xPos;
                xPos = imAttack.Right + iconSpacing;

                zonePanel.Width += imAttack.Width + iconSpacing;
            }

            if (Zone.ExamineJob != null)
            {
                zonePanel.Add(imForage);
                SetJobIconColor(Zone.ExamineJob.ID, imForage);
                imForage.X = xPos;
                xPos = imForage.Right + iconSpacing;

                zonePanel.Width += imForage.Width + iconSpacing;
            }

            if (Zone.ScoutingJob != null)
            {
                zonePanel.Add(imScout);

                SetJobIconColor(Zone.ScoutingJob.ID, imScout);
                imScout.X = xPos;
                xPos = imScout.Right + iconSpacing;

                zonePanel.Width += imScout.Width + iconSpacing;
            }
            zonePanel.Height = WindowHeight;
            
            
            xPos = zonePanel.Right + iconSpacing;
            buttonExpand.X = xPos; // base.DisplayWindow.Width - btExpand.Width - sideMargin;
            Add(buttonExpand);
            base.DisplayWindow.Width = buttonExpand.Right + 1; // +sideMargin; // 3 pixels margin??

        }
        //private void RefreshZoneMarker()
        //{           
        //    int xPos = markerLabel.Right + iconSpacing + 4;

        //    zonePanel.Remove(imStockpile);
        //    zonePanel.Remove(imGather);
        //    zonePanel.Remove(imHunt);
        //    zonePanel.Remove(imScout);
        //    zonePanel.Remove(imForage);
        //    zonePanel.Width = 0;

        //    if (Zone.Stockpile != null)
        //    {
        //        zonePanel.Add(imStockpile);

        //        zonePanel.Width += imStockpile.Width;


        //    }

        //    if (Zone.HasHarvestJobs())
        //    {
        //        zonePanel.Add(imGather);
                
        //        zonePanel.Width += imGather.Width;


        //    }

        //    if (Zone.PatrolJob != null)
        //    {
        //        zonePanel.Add(imPatrol);
        //        SetJobIconColor(Zone.PatrolJob.ID, imPatrol);
        //        zonePanel.Width += imPatrol.Width;


        //    }
        //    if (Zone.FindPreyJob != null)
        //    {
        //        zonePanel.Add(imHunt);
        //        SetJobIconColor(Zone.FindPreyJob.ID, imHunt);
        //        zonePanel.Width += imHunt.Width;

        //    }

        //    if (Zone.ForageJob != null)
        //    {
        //        zonePanel.Add(imForage);
        //        SetJobIconColor(Zone.ForageJob.ID, imForage);
        //        zonePanel.Width += imForage.Width;

        //    }

        //    if (Zone.ScoutingJob != null)
        //    {
        //        zonePanel.Add(imScout);               
        //        SetJobIconColor(Zone.ScoutingJob.ID, imScout);
        //        zonePanel.Width += imScout.Width;

        //    }

        //    zonePanel.Height = WindowHeight;
        //    zonePanel.X = xPos;

        //    xPos = zonePanel.Right + iconSpacing;

        //    buttonExpand.X = xPos; // base.DisplayWindow.Width - btExpand.Width - sideMargin;
        //    Add(buttonExpand);//for zones the expand button is always present

        //    base.DisplayWindow.Width = buttonExpand.Right + 1; // +sideMargin; // 3 pixels margin??
        //}

        void SetJobIconColor(Dictionary<ResourceType, List<ProcessJob>> jobs, Image image/*, bool blockedByThreat*/)
        {
            bool aJobWasBlockedByThreat = false;
            bool aJobWasBlockedByStance = false;
            bool jobIsInaccessible = false, blockedByThreat = false, blockedByStance = false, toofar = false, huntingNotFeasible = false;
            foreach (var kvp in jobs)
            {
                foreach (var job in kvp.Value)
                {
                    if (!GetJobStatus(job, ref aJobWasBlockedByThreat, ref aJobWasBlockedByStance))
                    {
                        image.ToolTip = null;
                        image.Color = Color.White;
                        return;
                    }  
                    /*
                    The.Client.GetFeedback(job.ID, out jobIsInaccessible, out blockedByThreat, out blockedByStance, out toofar, out huntingNotFeasible);
                    if (jobIsInaccessible == false && blockedByThreat == false && blockedByStance == false)
                    {
                        image.ToolTip = null;
                        image.Color = Color.White;
                        return;
                    }

                    if (blockedByStance)
                    {
                        aJobWasBlockedByStance = true;
                    }

                    if (blockedByThreat)
                    {
                        aJobWasBlockedByThreat = true;
                    }*/
                }
            }

            SetJobIconColor(jobIsInaccessible, aJobWasBlockedByStance, aJobWasBlockedByThreat, image);
        }

        void SetJobIconColor(List<FindPreyJob> jobs, Image image)
        {
            if (jobs != null)
            {
                bool aJobWasBlockedByThreat = false;
                bool aJobWasBlockedByStance = false;
                bool jobIsInaccessible = false;
                foreach (var job in jobs)
                {
                    if (!GetJobStatus(job, ref aJobWasBlockedByThreat, ref aJobWasBlockedByStance))
                    {
                        image.ToolTip = null;
                        image.Color = Color.White;
                        return;
                    }
                }

                SetJobIconColor(jobIsInaccessible, aJobWasBlockedByStance, aJobWasBlockedByThreat, image);
            }
            else
            {
                image.ToolTip = null;
                image.Color = Color.White;
            }
        }

        private bool GetJobStatus(Job job, ref bool aJobWasBlockedByThreat, ref bool aJobWasBlockedByStance)
        {
            bool jobIsInaccessible = false, blockedByThreat = false, blockedByStance = false, toofar = false, huntingNotFeasible = false, areaIsCleared = false;

            The.Client.GetFeedback(job.ID, out jobIsInaccessible, out blockedByThreat, out blockedByStance, out toofar, out huntingNotFeasible, out areaIsCleared);

            if (jobIsInaccessible == false && blockedByThreat == false && blockedByStance == false)
            {              
                return false;
            }

            if (blockedByStance)
            {
                aJobWasBlockedByStance = true;
            }

            if (blockedByThreat)
            {
                aJobWasBlockedByThreat = true;
            }

            return true;
        }

        void SetJobIconColor(bool jobIsInaccessible, bool blockedByThreat, bool blockedByStance, Image image)
        {
            if (jobIsInaccessible || blockedByStance || blockedByThreat)
            {
                if (blockedByThreat)
                {
                    image.ToolTip = "Dangerous area. To enter the area, a person must have Fearless stance. (Use a PATROL zone to clear the area of threats.)";//mp was: Dangerous area (To enter the area, a person must have Fearless stance)
                }
                else if (jobIsInaccessible)
                {
                    image.ToolTip = "Task cannot be completed because area is inaccessible - due to terrain or obstacles blocking the way"; //"Area not accessible because of terrain, structures or obstacles blocking the way"
                }
                else
                {
                    image.ToolTip = "No camp members can do this dangerous task (Required stance: Fearless)"; //  MP was: "Characters not in the mood to do an dangerous job."
                }
                image.Color = Color.Red;
            }
            else
            {
                image.ToolTip = null;
                image.Color = Color.White;
            }
        }

        void SetJobIconColor(JobID jobID, Image image)
        {
            bool jobIsInaccessible, blockedByThreat, blockedByStance, tooFarAwayFromExpedition, huntingNotFeasible, areaNotCleared;
            The.Client.GetFeedback(jobID, out jobIsInaccessible, out blockedByThreat, out blockedByStance, out tooFarAwayFromExpedition, out huntingNotFeasible, out areaNotCleared);

            SetJobIconColor(jobIsInaccessible, blockedByThreat, blockedByStance, image);
            
        }

        void expandButton_Click(UIComponent sender, EventArgs e)
        {
            if(currentType == MarkerType.Zone)
            {
                The.InGameUI.SelectedZone = Zone;

                The.InGameUI.ShowContextMenu(DisplayWindow.X, DisplayWindow.Y, this.DisplayWindow);

                The.InGameUI.ShowSelectedMapAreaPanel();
            }
            else if(currentType == MarkerType.Entity)
            {
                if (The.InGameUI.HUDActionPanel.IsShowingEntity(entityID.Value))
                {
                    // close when re-clicked:
                    The.InGameUI.HUDActionPanel.Hide();
                }
                else
                {
                    
                    The.InGameUI.HUDActionPanel.ShowOnPlayfield(DisplayWindow.AbsolutePosition.X + DisplayWindow.Width - 4,
                        DisplayWindow.AbsolutePosition.Y, false, entityID.Value);
                }
            }
            else if (currentType == MarkerType.Expedition)
            {
                if (The.InGameUI.HUDActionPanel.IsShowingExpedition(Expedition))
                {
                    // close when re-clicked:
                    The.InGameUI.HUDActionPanel.Hide();
                }
                else
                {
                    The.InGameUI.HUDActionPanel.ShowOnPlayfield(DisplayWindow.AbsolutePosition.X + DisplayWindow.Width - 4,
                           DisplayWindow.AbsolutePosition.Y, false, Expedition);
                }
            }
        }

        void bt_MouseOver(UIComponent sender, InputEventSystem.MouseEventArgs args)
        {
            // show the context menu??? without selecting?
        }

        /// <summary>
        /// only updates the position, and only for entity markers
        /// </summary>
        public void Update()
        {
            if (DisplayWindow.IsVisibleAndActive || showWhileLineIsSpoken == true)//Only update if the window is being shown
            {                   
                if (currentType == MarkerType.Entity)
                {                   
                    UpdateEntityMarkerPosition();
                }
            }
        }

     

        private void UpdateEntityMarkerPosition()
        {
            if (entityID.HasValue)
            {
                IKnownEntityData entityWithStatus;
                UpdatePosition(out entityWithStatus);

                if (entityWithStatus != null)
                {
                    SetScreenPosition(new Point(screenPosition.Value.X, screenPosition.Value.Y));
                    SetWorldPosition(new Point(screenPosition.Value.X, screenPosition.Value.Y));
                    activityWindow.SetPosition(screenPosition.Value);
                    activityWindow.Update();

                    return;
                }
            }           
        }

        public override void Hide()
        {
            //isHiddenByDefault = true;
            HideWindow();
        }

        private void HideWindow()
        {
            base.DisplayWindow.ViewPort.Click -= new ClickHandler(OnMarkerClick);

            if(showWhileLineIsSpoken == false)
            {
              /*  if (currentType == MarkerType.Entity)
                {
                    activityWindow.Hide();
                }*/

                base.Hide();
            }
        }

        public MarkerType GetCurrentType()
        {
            return currentType;
        }
    }
}
