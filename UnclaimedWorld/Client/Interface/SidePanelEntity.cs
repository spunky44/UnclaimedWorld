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
using UWGame.SimSide.AI;

using UWGame.SimSide;
using UWGame.SimSide.Expeditions;
using UWGame.Client;

using UWGame.ClientSide.Interface.Controls;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.ClientSide.Interface.HUD_Windows;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Entities.Owners;
using UWGame.ClientSide.Interface.Inventory;

namespace UWGame.ClientSide.Interface
{
    public class SidePanelEntity : RosterPanel //SidePanel
    {
        public Grid outerGrid;

        CollapsablePanel cpCarrying;
        Grid grdCarrying;

        List<CategoryPanelAndData> customPanels = new List<CategoryPanelAndData>();
        Label lblStatusInfo1, lblStatusInfo2, lblStatusInfo3, lblStatusInfo4;
        DataTypeButton selectedEntityTypeDataButton;


        const int containsPanelSortOrder = 17; // after Status with the capacity bar...

        public SidePanelEntity()
            : base(The.InGameUI.sidePanelHeight, true)
        {
            /* move this to counter panel
            ImageButton btNext = new ImageButton(The.InGameUI.gui);
            centerWindow.Add(btNext);
            btNext.Position = new Point(3, 1);
            btNext.InitWithIcon(ImageButtonType.Black, "cycleCharacter_icon", false);  
            btNext.Click += new ClickHandler(next_Click);
           // btNext.RenderType = RenderType.Overlay; //??
            btNext.ZOrder = 1f;
            btNext.ToolTip = "Select the next camp member";
            */
            InitStatusContentPanel();

            outerGrid = CreateOuterGridForCollapsableLists(The.InGameUI.gui, lcdSurface);

            UIComponent item;

            int itemHeight = ItemHeight;

            item = new UIComponent(The.InGameUI.gui);

            AddCollapsablePanelAndGrid(The.InGameUI.gui, outerGrid, "ITEMS",
                itemHeight, out cpCarrying, out grdCarrying, Grid.SelectabilityOptions.None, containsPanelSortOrder);

            AddPresentationPanel(outerGrid, customPanels, GameData.Instance.CustomSidePanelData);

            // AddCollapsablePanelAndGrid(The.InGameUI.gui, outerGrid, "Occupants", itemHeight, out cpOccupants, out grdOccupants);
        }

        public static void AddPresentationPanel(Grid outerGrid, List<CategoryPanelAndData> customPanels, CustomDataPresentation presentationToUse)
        {
            // the set of categories never changes, so construct an entry component for each
            foreach (PresentationTypeCategory presentationTypeCategory in presentationToUse.FinalPresentationTypeCategories) //GameData.Instance.CustomSidePanelData.FinalPresentationTypeCategories) 
            {
                CollapsablePanel newCollapsablePanel;
                Grid newGrid;

                AddCollapsablePanelAndGridNotFixed(The.InGameUI.gui, outerGrid, presentationTypeCategory.Name, out newCollapsablePanel, out newGrid, Grid.SelectabilityOptions.None);

                customPanels.Add(new CategoryPanelAndData(newCollapsablePanel, newGrid, presentationTypeCategory));

                newCollapsablePanel.IsExpanded = presentationTypeCategory.StartsAsExpanded;
                newCollapsablePanel.OrderByTag1 = presentationTypeCategory.PanelSortOrder;
            }
        }

       /* public static void AddPresentationPanelOtherSite(Grid outerGrid, List<CategoryPanelAndData> customPanels)
        {
            foreach (PresentationTypeCategory presentationTypeCategory in GameData.Instance.CustomOtherSiteSidePanelData.FinalPresentationTypeCategories) // the set of categories never changes, so construct an entry component for each
            {
                CollapsablePanel newCollapsablePanel;
                Grid newGrid;

                AddCollapsablePanelAndGridNotFixed(The.InGameUI.gui, outerGrid, presentationTypeCategory.Name, out newCollapsablePanel, out newGrid);

                customPanels.Add(new CategoryPanelAndData(newCollapsablePanel, newGrid, presentationTypeCategory));

                newCollapsablePanel.IsExpanded = presentationTypeCategory.StartsAsExpanded;
                newCollapsablePanel.OrderByTag1 = presentationTypeCategory.PanelSortOrder;
            }
        }*/

        private void InitStatusContentPanel()
        {
            statusContent = The.InGameUI.StatusScreen.GetNewSurfaceContent();
            statusContent.DebugTag = "entityStatus";

            Game game = The.Sim.Controller.Game;
            GUIManager gui = The.InGameUI.gui;

            imStatusBackground = new Image(gui);
            statusContent.Add(imStatusBackground);

            InitBillboardPanel();

            //statusBillboardPanelCenter = new Vector2(statusContent.Width * 2f / 3f, statusContent.Height / 2f);

            Rectangle rect = gui.GUI_CRT_SpriteSheet.GetSourceRectangle("human_b_m_adult_1"); //"Construction");
            imStatusBackground.Texture = gui.GUI_CRT_SpriteSheet.Texture;
            imStatusBackground.SetSkinLocation(SkinState.Normal,rect);
            imStatusBackground.Position = new Point(statusContent.Width - rect.Width, 2); // imStatusBackground.Position = new Point(0, 2);            
            imStatusBackground.ScaleImageToSizeOfControl = false;
            imStatusBackground.ResizeControlToFitImage();
            imStatusBackground.RenderType = RenderType.CRTAndLCD;
            imStatusBackground.Alpha = 1f; // 0.35f;// use for background for text!

            InitStatusCRTHeader(gui, statusContent, statusTextX, out lblStatusHeading, out crtUnderline);

            lblStatusInfo1 = new Label(gui);
            statusContent.Add(lblStatusInfo1);
            lblStatusInfo1.Position = new Point(statusTextX, 110);
            lblStatusInfo1.Text = "MALE";
            lblStatusInfo1.Init(Label.LabelType.CRTSmall);
            lblStatusInfo1.Width = statusTextWidth;

            lblStatusInfo2 = new Label(gui);
            statusContent.Add(lblStatusInfo2);
            lblStatusInfo2.Position = new Point(statusTextX, 130);
            lblStatusInfo2.Text = "AGE: 34";
            lblStatusInfo2.Init(Label.LabelType.CRTSmall);
            lblStatusInfo2.Width = statusTextWidth;

            lblStatusInfo3 = new Label(gui);
            statusContent.Add(lblStatusInfo3);
            lblStatusInfo3.Position = new Point(statusTextX, 165);
            lblStatusInfo3.Text = "MANUAL LABORER";
            lblStatusInfo3.Init(Label.LabelType.CRTSmall);
            lblStatusInfo3.NormalColor = Color.PowderBlue;
            lblStatusInfo3.Width = statusTextWidth;

            lblStatusInfo4 = new Label(gui);
            statusContent.Add(lblStatusInfo4);
            lblStatusInfo4.Position = new Point(statusTextX, 185);
            lblStatusInfo4.Text = "WALKING";
            lblStatusInfo4.Init(Label.LabelType.CRTSmall);
            lblStatusInfo4.NormalColor = Color.PowderBlue;
            lblStatusInfo4.Width = statusTextWidth;
        }

        public override void Show()
        {
            The.InGameUI.StatusScreen.ShowCenterButton(true); //center_Click, track_Click); ///*"Selects the next entity.",*/ ); ////MP 30 oct 2013  changed it to next camp member because this is the functionality at the time...later we can get the intended purpose which is cycling through entities of that category or whatevs

            base.Show();
        }

        public override void Hide()
        {
            The.InGameUI.StatusScreen.ShowCenterButton(false);

            base.Hide();
        }

        private void ShowBackgroundImage()
        {
            // important! add on top of noise background, but behind text labels!
            statusContent.Insert(imStatusBackground, 1);
        }

        private void RefreshStatusScreen()
        {
            lblStatusInfo1.Text = "";
            lblStatusInfo2.Text = "";
            lblStatusInfo3.Text = "";
            lblStatusInfo4.Text = "";
            lblStatusHeading.Text = "";

            lblStatusHeading.DebugTag = "missingHeader";
            statusContent.Remove(imStatusBackground);
            statusContent.Remove(pnBillboards);

            RefreshEntityStatus();

            The.InGameUI.StatusScreen.Refresh();
        }

        protected void RefreshEntityStatus()
        {
            if (intface.SelectedEntity == null)
            {
                return;
            }
            IKnownEntityData data;
            The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(The.InGameUI.SelectedEntity.Value, out data);
            Entity entity = data as Entity;
            if (data == null)
            {
                return;
            }

            RefreshEntityName(data);

            if (entity != null)
            {
                if (data.EntityType.Person != null) // entity.BiologicalEntity != null) // && intface.SelectedEntity.EntityType.BiologicalType.Castes != null) 
                {
                    RefreshPersonSpecificData(data, entity);
                }
                else
                {
                    if (entity.Structure != null)
                    {
                        RefreshStructureData(entity);
                    }
                    #region vehicle code
                    /*  else if (entity.Vehicle != null)
                      {
                          Vehicle vehicle = entity.Vehicle;
                          if (vehicle.DrivenBy != null)
                          {
                              // TEASER HACK:
                            //  lblStatusInfo1.Text = "SEATS: 3/3";
                              lblStatusInfo1.Text = "SEATS: " + (1 + entity.Vehicle.Passengers.Count).ToString() + "/" + (data.EntityType.ContainerType.VehicleContainerType.MaxPassengers + 1).ToString();
                          }
                          else
                          {
                              lblStatusInfo1.Text = "UNOCCUPIED";
                          }                    
                    
                      }

                      Power power;
                      if (entity.Find(out power)) //.Power != null)
                      {
                          lblStatusInfo2.Text = "ENERGY: " + power.EnergyLevelToString(); // 81 %";
                      }

                      lblStatusInfo3.Text = "CONDITION: GOOD";*/
                    #endregion
                }

                if (entity.Intelligence != null
                    && entity.Intelligence.Brain != null) // is null while being built
                {
                    RefreshTask(entity);
                }
            }
            else
            {
                RefreshMemoryFactExplanation();
            }

            #region commented condition code
            /*if (data.Condition.HasValue) // entity.NonLivingEntity != null && entity.NonLivingEntity.Progress < 1f)
            {
                StringBuilder description = new StringBuilder();
                description.Append("CONDITION: ");
                description.Append((int)(100 * data.Condition)); // entity.NonLivingEntity.Progress));
                description.Append(" %");

                lblStatusInfo1.Text = description.ToString();
            }*/
            #endregion

            // image
            if (data.EntityType.Person != null && entity != null)
            {
                RefreshPortraitImage(entity);
            }
            else if (data.EntityType.RenderableTypeMode != null && data.EntityType.RenderableTypeMode.DefaultClientState != null && data.EntityType.RenderableTypeMode.DefaultClientState.RenderAsBillboardType != null)
            {
                // important! add on top of noise background, but behind text labels!
                //ShowBackgroundImage();
                //statusContent.Remove(imStatusBackground);
                ShowBillboardPanel();

                //SidePanel.DisplayScaledBillboardImage(statusContent, imStatusBackground, intface.SelectedEntity.EntityType, StatusScreen.HeightOfStatusImage, true);

                /*  float factor = (float)intface.SelectedEntity.EntityType.TileLayoutType.WidthInTiles / 
                      (float)intface.SelectedEntity.EntityType.TileLayoutType.HeightInTiles;
                          */

                CreateAndPlaceBillboards(intface.gui, pnBillboards, data.EntityType, statusBillboardPanelCenter,
                    StatusScreen.HeightOfStatusImage, true); //true);

            }
            else if (!string.IsNullOrEmpty(data.EntityType.ThumbnailBig))
            {
                ShowBackgroundImage();
                //statusContent.Remove(pnBillboards);

                Rectangle rect = intface.gui.GUI_CRT_SpriteSheet.GetSourceRectangle(data.EntityType.ThumbnailBig);
                imStatusBackground.Texture = The.InGameUI.gui.GUI_CRT_SpriteSheet.Texture;
                imStatusBackground.SetSkinLocation(SkinState.Normal,rect);
                imStatusBackground.Position = new Point(statusContent.Width - rect.Width, 2); // new Point(0, 2);
                imStatusBackground.ScaleImageToSizeOfControl = false;
                imStatusBackground.ResizeControlToFitImage();
                imStatusBackground.Alpha = 1f;
            }
            else
            {
                // clear it... TODO: models? or thumbnails
                //statusContent.Remove(imStatusBackground);
                //statusContent.Remove(pnBillboards);
            }
        }

        public void RefreshPortraitImage(Entity entity)
        {
            ShowBackgroundImage();
            //statusContent.Remove(pnBillboards);
            //statusContent.Remove(imStatusBackground);

            Rectangle rect = entity.PersonEntity.GetPortraitForStatusDisplay(The.InGameUI.gui); // .PortraitRectStatus; //"Construction");
            imStatusBackground.Texture = Interface.gui.GUI_CRT_SpriteSheet.Texture;
            imStatusBackground.SetSkinLocation(SkinState.Normal,rect);
            imStatusBackground.Position = new Point(statusContent.Width - rect.Width, 2); // new Point(0, 2);
            imStatusBackground.ScaleImageToSizeOfControl = false;
            imStatusBackground.ResizeControlToFitImage();
            imStatusBackground.Alpha = 1f; // 0.35f;// use for background for text!
        }

        private void RefreshMemoryFactExplanation()
        {
            string memoryFactExplanation = "Last seen here";//entity.Intelligence.Brain.GetStatus().ToUpper();
            memoryFactExplanation = string.Format("-{0}-", memoryFactExplanation);

            lblStatusInfo4.Text = memoryFactExplanation;
        }

        private void RefreshTask(Entity entity)
        {
            // current task:
            /*  if (entity.EntityType.BiologicalType != null)
              {*/
            string task = entity.Intelligence.Brain.GetStatus().ToUpper(Config.Culture);
            if (task != "")
            {
                task = string.Format("-{0}-", task);
            }

            lblStatusInfo3.Text = task;
            // }
        }

        private void RefreshStructureData(Entity entity)
        {
            lblStatusInfo4.Text = entity.Structure.GetStateDescription();
            if (lblStatusInfo4.Text != "")
            {
                lblStatusInfo4.Text = string.Format("-{0}-", lblStatusInfo4.Text);
            }
        }

        private void RefreshPersonSpecificData(IKnownEntityData data, Entity entity)
        {
            SetHeaderText(data.GetDisplayName().ToUpper(Config.Culture));

            lblStatusInfo1.Text = entity.BiologicalEntity.CasteType.Reproduction.ToString().ToUpper(Config.Culture);
            lblStatusInfo2.Text = "AGE: " + ((int)entity.BiologicalEntity.AgeGroup.Age).ToString();

            float exertionLevel = entity.Intelligence.Brain.GetExertionLevelOfActivity();
            float exertionLevelFactor = exertionLevel / GameData.Instance.Constants.PhysicalWork.MaxPhysicalWorkCost;
            string exertionDescription = GameData.Instance.AllPresentationTypes["energyUsePresentation"].GetValueTerm(exertionLevelFactor, null, "");
            //GameData.Instance.AllPresentationTypes[presentationTypeKey]
            lblStatusInfo4.Text += "EXERTION LEVEL: " + exertionDescription;
            //lblStatusInfo4.Text += "Productivity: " + entity.Intelligence.Brain.getp();
        }

        private void RefreshEntityName(IKnownEntityData data)
        {
            SetHeaderText(data.GetDisplayName().ToUpper(Config.Culture));            
        }

        public override void Refresh()
        {
            RefreshStatusScreen();           

            IKnownEntityData entityData;
            if (intface.SelectedEntity == null)
            {
                entityData = null;
            }
            else
            {
                The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(intface.SelectedEntity.Value, out entityData);
            }

            Populate(entityData);

        }

        public void Populate(IKnownEntityData entityData)
        {           
            outerGrid.BeginAddingEntries();

            // TODO: Migrate: expose as Entity property and populate from custom data            
            PopulateContains(entityData);

            PopulateCustomPanels(entityData, customPanels, outerGrid); // Needs, parts, skills etc.
                      
            RefreshEntityTypeButton(entityData);

            // sort panels here
            outerGrid.Sort(u => u.OrderByTag1, Grid.Sorting.Ascending);
            

            outerGrid.EndAddingEntries(); 
        }

        private void RefreshEntityTypeButton(IKnownEntityData entityData)
        {
            if (entityData != null)
            {
                UpdateEntityTypeInfo(outerGrid, ref selectedEntityTypeDataButton, entityData);
            }
            else
            {
                if (selectedEntityTypeDataButton != null)
                {
                    outerGrid.TryRemoveEntry(selectedEntityTypeDataButton);
                }
            }
        }


        

        private bool EntityCanBeSalvaged(IKnownEntityData entity)
        {
            if (entity != null)
            {
                return entity.EntityType.CanBeSalvagedDirectly();
            }

            return false;
        }

        private void ShowNextOrPrevious(int currentIndex)
        {
            int count = The.Sim.PlaySite.PlayerAllegiance.Persons.Count;
            if (count > 0)
            {
                if (currentIndex > count - 1)
                {
                    currentIndex = currentIndex - count;
                }
                else if (currentIndex < 0)
                {
                    currentIndex = currentIndex + count;
                }

                Entity entity = The.Sim.PlaySite.PlayerAllegiance.Persons[currentIndex];
                if (entity.IsDead == false && entity.IsOnPlaySite())
                {
                    intface.SelectEntity(entity);
                }
            }
        }



        private void PopulateContains(IKnownEntityData entityData)
        {
            if (AddPanelIfNotPresent(entityData != null && entityData.ContainedEntitiesByType != null, outerGrid, cpCarrying))
            {
                PopulateContainedItems(entityData);
            }
            else
            {
                AddPanelIfNotPresent(false, outerGrid, cpCarrying);// remove panel
            }
        }

        public static void PopulateCustomPanels(IKnownEntityData entityData, List<CategoryPanelAndData> customPanels, Grid outerGrid)
        {
            bool entityExists = true;
            IHasExposedProperties hasExposedProperties = entityData as Entity;

            if (hasExposedProperties == null)
            {
                hasExposedProperties = entityData as MemoryFact;
                if (hasExposedProperties == null)
                {
                    entityExists = false;
                }
            }

            foreach (CategoryPanelAndData customPanel in customPanels)
            {
                int? numberOfItems = null;
                bool panelHasData = false;
                if (entityExists == true)
                {

                    if (customPanel.Category.Name == "SKILLS")
                    {

                    }

                    panelHasData = PresentationTypeCategoryProcessor.DisplayCategory(
                            customPanel.Category,
                            hasExposedProperties,
                            customPanel.Grid,
                            ref numberOfItems
                            );

                    customPanel.Panel.Summary = "";
                    if (customPanel.Category.SetCountAsSummary == true)
                    {
                        if (customPanel.Grid.Count > 0)
                        {
                            customPanel.Panel.Summary = numberOfItems.ToString();
                        }
                    }
                    else if (numberOfItems.HasValue)
                    {
                        customPanel.Panel.Summary = numberOfItems.ToString();
                    }
                }

                try
                {                   
                    // crash here? http://steamcommunity.com/app/284100/discussions/2/343788552548402954/
                    // + http://steamcommunity.com/app/284100/discussions/2/152392786899383029/
                    AddPanelIfNotPresent(panelHasData && entityExists, outerGrid, customPanel.Panel);

                }
                catch(NullReferenceException ex)
                {
                    string msg = "Custom panel error #1 \n";
                    if (customPanel != null)
                    {
                        if (customPanel.Category != null)
                        {
                            msg += "customPanel.Category name: " + customPanel.Category.Name;
                        }
                        else
                        {
                            msg += ", customPanel.Category: null";
                        }
                    }
                    else
                    {
                        msg += ", customPanel: null";
                    }
                   
                    msg += ", panelHasData: " + panelHasData;
                    msg += ", entityExists: " + entityExists;
                    if (outerGrid == null)
                    {
                        msg += ", outerGrid: null";
                    }
                    else
                    {
                        msg += ", outerGrid: not null";
                    }

                    throw new Exception(msg);
                }
            }
        }


        private void PopulateContainedItems(IKnownEntityData entityData)
        {

            if (entityData.EntityType.BiologicalType != null)
            {
                if (cpCarrying.Title != "CARRYING")
                {
                    cpCarrying.Title = "CARRYING";
                }
            }
            else
            {
                //STORED ???
                if (cpCarrying.Title != "CONTAINS")
                {
                    cpCarrying.Title = "CONTAINS";
                }
                // horses etc.
                /*  */
            }

            #region commented
            // EntityID item;
            /*  List<EntityID> storedItemIds = entity.AgentStorage.ItemStorage.StoredItems;
              for (int i = storedItemIds.Count - 1; i >= 0; i--)
              {
                  item = storedItemIds[i];
                  The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(item, out knownData);
                  if (knownData != null)
                  {
                      storedItems.Add(knownData);
                  }
                  else
                  {
                      storedItemIds.RemoveAt(i);
                  }
              }*/
            #endregion

            if (AddPanelIfNotPresent(entityData.ContainedEntitiesByType.Count > 0, outerGrid, cpCarrying))
            {
                UIComponent itemRow;

                int total = 0;

                grdCarrying.BeginAddingEntries();

                EntityType entityType;

                OwnerID? ownerID = The.InGameUI.GetUIOwnerID();
 
                foreach (var item in entityData.ContainedEntitiesByType)
                {
                    if (entityData.TotalStored != null)
                    {
                    }
                    entityType = item.Key;

                    if (entityType.ItemType != null)
                    {
                        //We are counting every entity that this entity contains and displayiing it on the grid.Title as Summary
                        total += item.Value.Count;

                        if (!grdCarrying.TryGetEntry(entityType, out itemRow))
                        {
                            // add the item row and its category if needed:
                            itemRow = SidePanelMapArea.AddItemRow(grdCarrying, entityType, null, true, tbItems_Click);

                            //grdCarrying.TryGetEntry(entityType, out itemRow);
                        }

                        List<EntityID> listOfIDs = item.Value;
                        SidePanelMapArea.UpdateItemRow(ownerID, itemRow, entityType, listOfIDs);//entityData.ContainedEntities);
                    }
                }

                // remove unused items 
                grdCarrying.DeleteEntries<object>(
                    e => !(e is EntityType) || entityData.ContainedEntitiesByType.ContainsKey((EntityType)e));

                // and categories:
                grdCarrying.DeleteEntries<object>(
                    e => !(e is EntityCategory) || SidePanelMapArea.CategoryIsRepresented((EntityCategory)e, entityData.ContainedEntitiesByType));

                grdCarrying.EndAddingEntries();

                cpCarrying.Summary = total.ToString();
            }
        }

        private static void UpdateEntityTypeInfo(Grid outerGrid, ref DataTypeButton btEntityType, IKnownEntityData entityData) // EntityType entityType, EntityID? entityID)
        {
            Image icon;
            UIComponent item;
            EntityType entityType = entityData.EntityType;
            EntityID entityID = entityData.EntityID;
            //  object key = "key";
            // for populating entity type info in the side panel, always use the UI allegiance owner - regardless of who owns the entity...
            // the player wants to know if he can build the entity.
            // Owner owner = The.InGameUI.UIOwner; // NEW!

            if (btEntityType == null)
            {
                btEntityType = new DataTypeButton(The.InGameUI.gui, DataSheet.InfoToShow.Data, entityType, null, true, entityID);
            }
            else
            {
                btEntityType.FillEntityType(DataSheet.InfoToShow.Data, entityType, null, true, entityType.Name, entityID);
            }


            if (outerGrid.TryGetEntry(btEntityType, out item) == false)
            {
                UIComponent itemRow = new UIComponent(The.InGameUI.gui);

                // #ICONTEST
                icon = InventoryPanel.AddEntityTypeIcon(entityType, itemRow);
                icon.ID = UIComponent.DataControlID.StatusIcon;
                /*
                icon = new Image(The.InGameUI.gui);
                itemRow.Add(icon);
                icon.ID = UIComponent.DataControlID.StatusIcon;
                icon.Texture = The.InGameUI.gui.GUISpriteSheet.Texture;
                Rectangle rect = entityType.GetIconSprite();
                icon.SetSkinLocation(SkinState.Normal,rect);
                icon.ResizeControlToFitImage();*/
                icon.X = SidePanelMapArea.GridMargin;
                icon.Y = -3;

                btEntityType.Init(TextButton.TextButtonType.LCDToolTipBlack);
                btEntityType.ID = UIComponent.DataControlID.Caption;
                btEntityType.IsRoot = true;
                btEntityType.Text = entityType.Name;
                btEntityType.TextAlignment = TextButton.TextAlign.Left;
                btEntityType.Width = 160;
                btEntityType.X = icon.Width + 5;
                btEntityType.DebugTag = "sidePanelTypeInfo";

                itemRow.Add(btEntityType);
                outerGrid.AddEntry(btEntityType, itemRow, 0);
            }
            else
            {
                icon = (Image)outerGrid.FindChildById(UIComponent.DataControlID.StatusIcon);
                icon.Texture = The.InGameUI.gui.GUISpriteSheet.Texture;
                IconInfo iconInfo;
                Rectangle rect = entityType.GetIconSprite(out iconInfo);
                icon.SetSkinLocation(SkinState.Normal,rect);
                icon.ResizeControlToFitImage();
            }

            if (entityData.EntityType.StructureType != null || entityData.EntityType.Person != null || entityData.EntityType.BiologicalType != null || entityData.EntityType.TerrainType != null && entityData.EntityType.TerrainType.IsSpecialInterestFeature)
            {
                // We are using the normalStatusColorSidepanel's value from BaseDataLoader.InitPresentationTypes()
                icon.Color = GameData.Instance.GUIConstants.sidePanelTextColor;
            }
            else
            {
                icon.Color = null;
            }
        }

      
        void tbItems_Click(UIComponent sender, EventArgs e)
        {
            StockButton st = sender as StockButton;
           // EntityType entityType = ((StockButton)sender).EntityType;

            // open the entity window
            The.InGameUI.EntityListWindow.SetDataSource(st.EntityType, st.EntityList); // GetListOfItems(entityType));
            /* The.InGameUI.GetExpedition().OwnedEntities.AllEntities[entityType]*/

            The.InGameUI.EntityListWindow.OpenNextToStockButton(sender);
            // The.InGameUI.EntityListWindow.ShowOnPlayfield(sender.AbsolutePosition.X + 26, sender.AbsolutePosition.Y);

        }

        void previous_Click(UIComponent sender, EventArgs e)
        {
            if (intface.SelectedEntity == null)
                return;

            Entity entity = Entity.FindByID(intface.SelectedEntity.Value);
            if (entity == null)
                return;

            int currentIndex = The.InGameUI.UIAllegiance.MembersList.IndexOf(entity);

            if (currentIndex != -1)
            {
                currentIndex--;
            }
            else
            {
                currentIndex = 0;
            }

            ShowNextOrPrevious(currentIndex);
        }

        void next_Click(UIComponent sender, EventArgs e)
        {
            if (intface.SelectedEntity == null)
                return;

            Entity entity = Entity.FindByID(intface.SelectedEntity.Value);
            if (entity == null)
                return;

            // iterate through the Allegiance's members - this makes the most sense I think.
            int currentIndex = The.InGameUI.UIAllegiance.MembersList.IndexOf(entity);

            if (currentIndex != -1)
            {
                currentIndex++;
            }
            else
            {
                currentIndex = 0;
            }

            ShowNextOrPrevious(currentIndex);
        }


        public static void SetSummaryAsTotal(CollapsablePanel cpCategory, object o)
        {
            // sum the items in each category            
            string sumOfItemsValue = o.ToString();

            string totalInCategory = cpCategory.Summary;
            if (totalInCategory == "")
            {
                cpCategory.Summary = sumOfItemsValue;
            }
            else
            {
                cpCategory.Summary = (int.Parse(totalInCategory) + int.Parse(sumOfItemsValue)).ToString();
            }
        }

        public List<EntityID> GetListOfItems(EntityType entityType)
        {
            if (!intface.SelectedEntity.HasValue)
                return null;

            IKnownEntityData entityData;

            if (Entity.FindByID(intface.SelectedEntity.Value).ContainedBy.HasValue)
            {
                The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(Entity.FindByID(intface.SelectedEntity.Value).ContainedBy.Value, out entityData);
            }
            else
            {
                The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(intface.SelectedEntity.Value, out entityData);
            }


            // if the entity is gone...
            if (entityData == null)
                return null;

            List<EntityID> listOfItemsOfType = new List<EntityID>();
            Dictionary<EntityType, List<EntityID>> listOfItems = entityData.ContainedEntitiesByType;
            //  List<EntityID> listOfItems = entityData.ContainedEntities;

            if (listOfItems != null)
            {
                EntityID itemID;
                IKnownEntityData itemData;
                foreach (var items in listOfItems)
                {
                    if (items.Key == entityType)
                    {
                        for (int i = listOfItems[entityType].Count - 1; i >= 0; i--)
                        {
                            itemID = listOfItems[entityType][i];

                            if (!GoalEvaluator.EntityDataResultCausesSkip(The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(itemID, out itemData)))
                            {
                                if (itemData.EntityType == entityType)
                                {
                                    listOfItemsOfType.Add(itemData.EntityID);
                                }
                            }
                        }
                    }
                }
            }

            return listOfItemsOfType;
        }


        /*  void ExpandEntityPanel_OnPress(object sender, EventArgs e)
          {
              if (intf.displayedExpandedPanel != null)
              {
                  intf.displayedExpandedPanel.Hide();
                  intf.displayedExpandedPanel = null;
              }
              else
              {
                  intf.ShowExpandedGUIPanel(Interface.GUIPanels.Entity);
              }
          }*/

        /*   public override void Update(GameTime elapsed)
           {
               if (updateRegulator.IsReady())
               {
                   Refresh();
               }
               base.Update(elapsed);
           }*/

        /*
        void previous_Click(UIComponent sender, EventArgs e)
        {
            if (intface.SelectedEntity == null)
                return;

            Entity entity = Entity.FindByID(intface.SelectedEntity.Value);
            if (entity == null)
                return;

           // int currentIndex = UWGame.SimSide.Instance.Site.Entities.IndexOf(intface.SelectedEntity);
            int currentIndex = The.Sim.Site.Persons.IndexOf(entity);

            if (currentIndex != -1)
            {
                currentIndex--;
            }
            else
            {
                currentIndex = 0;
            }

            ShowNextOrPrevious(currentIndex);
        }*/

        /*
        void next_Click(UIComponent sender, EventArgs e)
        {
            if (intface.SelectedEntity == null)
                return;

            Entity entity = Entity.FindByID(intface.SelectedEntity.Value);
            if (entity == null)
                return;
          
            int currentIndex = The.Sim.PlaySite.Persons.IndexOf(entity);

            if (currentIndex != -1)
            {
                currentIndex++;
            }
            else
            {
                currentIndex = 0;
            }

            ShowNextOrPrevious(currentIndex);

        }*/

        /*private void RefreshActionButtons()
      {
          actionButtons.ClearCommands();

          if (intface.SelectedEntity != null)
          {
              IKnownEntityData data;
              The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(intface.SelectedEntity.Value, out data);
              if (EntityCanBeSalvaged(data))
              {
                  actionButtons.SetCommands("Salvage", "Salvage the structure and reclaim its resources.");

              }
          }
      }*/

        /*
        void produce_OnPress(object sender, EventArgs e)
        {
            if (intface.SelectedEntity != null && intface.SelectedEntity.EntityType.StructureType.ProducerType != null) // .IsProductionBuilding) 
            {
                //game.StartProductionJob((Buildings.IProductionBuilding)intf.SelectedBuilding, game.ColonyOwner, game.ColonyOwner);
                UWGame.SimSide.Instance.StartProductionJob(intface.SelectedEntity, UWGame.SimSide.Instance.ColonyOwner, UWGame.SimSide.Instance.ColonyOwner, UWGame.SimSide.Instance.ColonyOwner);
            }
        }*/

        /*
       private void PopulateOccupants(IKnownEntityData entityData)
       {
           List<IKnownEntityData> containedAgents = null;
           bool containsIntelligence = entityData != null && Entity.ContainsIntelligence(entityData, The.InGameUI.UIAllegiance.SharedKnowledge, ref containedAgents);
           if (AddPanelIfNotPresent(containsIntelligence, outerGrid, cpOccupants))           
           {
               PopulateOccupantsPanel(entityData, containedAgents);
           }
           else
           {
               AddPanelIfNotPresent(false, outerGrid, cpOccupants);
           }
       }

       private void PopulateOccupantsPanel(IKnownEntityData entityData, List<IKnownEntityData> containedAgents)
       {
            
           grdOccupants.BeginAddingEntries();

           if (containedAgents != null)
           {
               foreach (var item in containedAgents)
               {
                   UIComponent entry;
                   if (!grdOccupants.TryGetEntry(item.EntityID, out entry))
                   {
                       grdOccupants.AddHyperLinkEntry(item.EntityID, Entity.GetDisplayName(item), (uint)item.EntityID, hyperLinkMargin);
                   }
               }
           }

           grdOccupants.DeleteEntries<EntityID>(e => containedAgents != null && containedAgents.Exists(c => c.EntityID == e));

           grdOccupants.EndAddingEntries();
           cpOccupants.Summary = grdOccupants.Entries.Count.ToString();

           
       }*/

        /*  public static void PopulateTreePanel(GUIManager gui, FullLCDPanel.SetCollapsedSummary setSummaryDelegate, CollapsablePanel cp, Grid grd, List<Entity> listOfItems)
          {
              Dictionary<EntityType, int> numberOfItems;
            //  Dictionary<EntiType, int> numberOfItems;

              numberOfItems = Common.GroupItemsByType(listOfItems);

        
              FullLCDPanel.PopulateCategoryGrid<ItemType, int, ItemCategory>(gui, grd, CollapsablePanel.PanelType.Node,
                  null, setSummaryDelegate,
                  numberOfItems);

              cp.Summary = listOfItems.Count.ToString();
          }*/

        /*  public static void PopulateTreePanel(GUIManager gui, FullLCDPanel.SetCollapsedSummary setSummaryDelegate, CollapsablePanel cp, Grid grd, List<IKnownEntityData> listOfItems)
          {
              Dictionary<EntityType, int> numberOfItems;
              //  Dictionary<EntiType, int> numberOfItems;

              numberOfItems = Common.GroupItemsByType(listOfItems);
                       
              FullLCDPanel.PopulateCategoryGrid<EntityType, int, EntityCategory>(gui, grd, CollapsablePanel.PanelType.Node, Label.LabelType.LCDNormal,
                  setSummaryDelegate, null,
                  numberOfItems);

              cp.Summary = listOfItems.Count.ToString();
          }*/

        /*  private void PopulateStocks()
          {
              PopulateTreePanel(intf.gui, SetSummaryDelegate, cpStock, grdStock, The.Map.TileMap[
                          intface.SelectedEntity.MapPosition.X][
                          intface.SelectedEntity.MapPosition.Y].EntitiesOnTile.FindAll(e => e.Item != null)); 

         
          }*/
    }
}
