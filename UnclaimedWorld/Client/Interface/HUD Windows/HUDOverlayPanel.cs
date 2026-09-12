using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Entities;
using UWGame.SimSide;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Items;
using UWGame.SimSide.Collisions;
using System.Diagnostics;
using UWGame.ClientSide.Interface.Controls;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.AI;
using UWGame.SimSide.Processes;
using UWGame.ClientSide.Interface.Overlays;
using UWGame.ClientSide.Interface.Inventory;

namespace UWGame.ClientSide.Interface.HUD_Windows
{
    public class HUDOverlayPanel : HUDWindow
    {
        List<Grid> childGridsThatWereChanged = new List<Grid>();

        const int quantityX = 10;
        const int captionX = 25;//42;
        const int buildX = 120;//18;

        Grid outerGrid;


        Grid grdSingleItems;
        Grid categoryGrid;

        public HUDOverlayPanel()
            : base(260 - 16, 350)
        {
            UIComponent listSurface = new UIComponent(gui);
            Add(listSurface);
            listSurface.X = 7;
            listSurface.Y = 10;//50;

            listSurface.Width = DisplayWindow.ViewPort.Width - 2 * listSurface.X - 2; // - 3 + 5;            
            listSurface.Height = DisplayWindow.Height - 2 * listSurface.Y - 5;

            grdSingleItems = new Grid(gui, ListBoxType.HUDAndLCD, Label.LabelType.HUDWindow); // WindowSystem.Label.LabelType.EntityTypeTooltip);
            grdSingleItems.IsOuterGrid = false;
            // grdInputs.DebugTag = "grdInputs";
            grdSingleItems.X = sideMargin;
            grdSingleItems.FixedItemHeights = true;
            grdSingleItems.Width = listSurface.Width;
            grdSingleItems.ScrollBarEnabled = false;
            grdSingleItems.ItemHeight = 22;
            grdSingleItems.CanGrowInHeight = true;
            grdSingleItems.Font = GUIManager.LCDandHUDBodyFontPath;
            // grdSingleItems.Height = 160; // 40; // 160


            // categoryGrid = CreateOuterGridForCollapsableLists(The.InGameUI.gui, listSurface ); 
            /* categoryGrid = CreateOuterGridForCollapsableLists(The.InGameUI.gui, null, false); 
             categoryGrid.ItemHeight = 36;*/

            categoryGrid = new Grid(gui, ListBoxType.HUDAndLCD, Label.LabelType.HUDWindow);
            categoryGrid.FixedItemHeights = false;
            categoryGrid.CanGrowInHeight = true;
            categoryGrid.ScrollBarEnabled = false;
            categoryGrid.RenderType = RenderType.Normal; // RenderType.CRTAndLCD;          
            categoryGrid.HMargin = 0;
            categoryGrid.VMargin = 0;
            categoryGrid.Font = GUIManager.LCDandHUDBodyFontPath; // Important! Must be set after Add! Add will reinitialize with defaults.
            categoryGrid.Width = listSurface.Width;
            // outerGrid.Height = surface.Height;
            categoryGrid.ItemHeight = 26;
            categoryGrid.Position = new Point(0, 0);


            outerGrid = CreateOuterGridForCollapsableLists(The.InGameUI.gui, listSurface, true);
            outerGrid.IsOuterGrid = true;
            outerGrid.ScrollBarEnabled = true;

            outerGrid.BeginAddingEntries();
            outerGrid.AddEntry(grdSingleItems, grdSingleItems);
            outerGrid.AddEntry(categoryGrid, categoryGrid);
            outerGrid.EndAddingEntries();

        }


        public override void Refresh()
        {
            base.Refresh();

            Populate();
        }

        public override void Hide()
        {
            base.Hide();

            The.InGameUI.OverlayPanel.btOverlay.IsChecked = false;
        }

        public void Populate()
        {
            SharedKnowledge sharedKnowledge = The.InGameUI.UIAllegiance.SharedKnowledge;

            if (The.Sim.Mode == Sim.EngineMode.Game)
            {
                PopulateSingleItemGrid();
            }
            else
            {
                PopulateSingleItemGridEditor();
            }


            childGridsThatWereChanged.Clear();

            categoryGrid.BeginAddingEntries();

            // PopulateEmptyCategory(sharedKnowledge,"");   // Colony Members
            PopulateResources(sharedKnowledge, "Show resource");
            PopulateStructureTypes(sharedKnowledge, "Show structure");
            PopulateEntityTypes(sharedKnowledge, "");


            foreach (var item in childGridsThatWereChanged)
            {
                // refit rows
                item.EndAddingEntries();
            }
            categoryGrid.EndAddingEntries();
        }


        private void PopulateSingleItemGrid()
        {
            grdSingleItems.BeginAddingEntries();

            var types = Enum.GetValues(typeof(OverlayTypes));

            UIComponent row;
            foreach (var item in types)
            {
                if (!grdSingleItems.EntriesByKey.TryGetValue(item, out row))
                {
                    OverlayTypes type = (OverlayTypes)item;
                    row = AddSingleItemRow(type, 
                        OverlaySettings.GetTextFromOverlayType(type), 
                        null, 
                        OverlaySettings.GetTooltipFromOverlayType(type),
                        OverlaySettings.GetIconFromOverlayType(type), 
                        OverlaySettings.GetColorFromOverlayType(type), 
                        cbSelectOverlayType_Click);
                }

                UpdateSingleItemRow(row);
            }



            grdSingleItems.EndAddingEntries();

        }

        private void PopulateSingleItemGridEditor()
        {
            grdSingleItems.BeginAddingEntries();

            var types = Enum.GetValues(typeof(EditorOverlayTypes));

            UIComponent row;
            foreach (var item in types)
            {
                if (!grdSingleItems.EntriesByKey.TryGetValue(item, out row))
                {
                    EditorOverlayTypes type = (EditorOverlayTypes)item;

                    row = AddSingleItemRow(type,
                        OverlaySettings.GetTextFromOverlayType(type),
                        null,
                        OverlaySettings.GetTooltipFromOverlayType(type),
                        OverlaySettings.GetIconFromOverlayType(type),
                        OverlaySettings.GetColorFromOverlayType(type),
                        cbSelectEditorOverlayType_Click);
                }

                UpdateSingleItemRowEditor(row);
            }
            

            grdSingleItems.EndAddingEntries();

        }

        #region PopulateCategoriesAndGroupings

        private void PopulateStructureTypes(SharedKnowledge sharedKnowledge, string checkboxTooltip)
        {
            CollapsablePanel cpGrouping;
            Grid grdGrouping = null;
            UIComponent groupingRow;

            EntityGrouping grouping = EntityGrouping.Structures;

            foreach (var kvp in sharedKnowledge.AllKnownEntities.Structures)
            {

                if (kvp.Value.Count > 0)
                {
                    AddAndUpdateEntityGroupingAndRow(kvp.Key, checkboxTooltip, out grdGrouping, grouping);
                }

                #region Clean-up

                // cleanup/delete 
                // remove entity types that should no longer be displayed  
                List<EntityID> listOfEntities;
                if (!sharedKnowledge.AllKnownEntities.AllEntities.TryGetValue(kvp.Key, out listOfEntities) || listOfEntities.Count == 0)
                {
                    if (grdGrouping == null)
                    {
                        if (categoryGrid.TryGetEntry(grouping, out groupingRow))
                        {
                            cpGrouping = groupingRow as CollapsablePanel;
                            grdGrouping = (Grid)cpGrouping.ExpandedPanel.Controls[0];

                            grdGrouping.BeginAddingEntries();
                            childGridsThatWereChanged.Add(grdGrouping); // save the grid for resize at the end of update
                        }
                    }

                    if (grdGrouping != null)
                    {
                        grdGrouping.TryRemoveEntry(kvp.Key);

                        if (grdGrouping.Entries.Count == 0)
                        {
                            categoryGrid.TryRemoveEntry(grouping);

                            if (grdGrouping != null)
                            {
                                childGridsThatWereChanged.Remove(grdGrouping);
                            }
                        }
                    }
                }
                #endregion
            }
        }

        private void PopulateEntityTypes(SharedKnowledge sharedKnowledge, string checkboxTooltip)
        {
            CollapsablePanel cpGrouping;
            Grid grdGrouping = null;
            UIComponent categoryRow;

            foreach (var kvp in sharedKnowledge.AllKnownEntities.AllEntities) // iterate all known entities
            {
                EntityGrouping? grouping = OverlaySettings.GetGrouping(kvp.Key);

                if (grouping != null && grouping != EntityGrouping.Structures) // only add/update the entity if its a structure
                {
                    if (kvp.Value.Count > 0 && grouping != null)
                    {
                        AddAndUpdateEntityGroupingAndRow(kvp.Key, checkboxTooltip, out grdGrouping, (EntityGrouping)grouping);
                    }

                    #region Clean-up
                    // cleanup/delete 
                    // remove entity types that should no longer be displayed  
                    if (grouping != EntityGrouping.Structures)
                    {
                        List<EntityID> listOfEntities;
                        if (sharedKnowledge.AllKnownEntities.AllEntities.TryGetValue(kvp.Key, out listOfEntities) && listOfEntities.Count == 0)
                        {
                            if (categoryGrid.TryGetEntry(grouping, out categoryRow))
                            {
                                cpGrouping = categoryRow as CollapsablePanel;
                                grdGrouping = (Grid)cpGrouping.ExpandedPanel.Controls[0];

                                grdGrouping.BeginAddingEntries();
                                childGridsThatWereChanged.Add(grdGrouping); // save the grid for resize at the end of update
                            }

                            if (grdGrouping != null)
                            {
                                grdGrouping.TryRemoveEntry(kvp.Key);

                                if (grdGrouping.Entries.Count == 0)
                                {
                                    categoryGrid.TryRemoveEntry(grouping);
                                    if (grdGrouping != null)
                                    {
                                        childGridsThatWereChanged.Remove(grdGrouping);
                                    }
                                }
                            }
                        }
                    }
                    #endregion
                }
            }
        }

        private void PopulateResources(SharedKnowledge sharedKnowledge, string checkboxTooltip)
        {           
            if (The.Sim.Mode == Sim.EngineMode.Game)
            {
                foreach (var kvp in sharedKnowledge.PlaySiteKnowledge.AllKnownResourceContainers)
                {
                    ResourceType resourceType = kvp.Key;
                    int noOfResourceContainers = kvp.Value.Count;

                    HandleResource(checkboxTooltip, resourceType, noOfResourceContainers, r => ShouldDisplayResourceInGame(sharedKnowledge, r));
                }
            }
            else
            {
                foreach (var kvp in The.Sim.PlaySite.EditorResources) //.Resources)
                {
                    ResourceType resourceType = kvp; // kvp.Key;
                    int noOfResourceContainers = 1; // kvp.Value.Count;

                    HandleResource(checkboxTooltip, resourceType, noOfResourceContainers, ShouldDisplayResourceInEditor);
                }
            }
        }

        private bool ShouldDisplayResourceInGame(SharedKnowledge sharedKnowledge, ResourceType resourceType)
        {
            HashSet<ResourceID> listOfEntities;

            return sharedKnowledge.PlaySiteKnowledge.AllKnownResourceContainers.TryGetValue(resourceType, out listOfEntities) && listOfEntities.Count > 0;
        }

        private bool ShouldDisplayResourceInEditor(ResourceType resourceType)
        {
          //  ObservableList<ResourceContainer> listOfEntities;

            return The.Sim.PlaySite.EditorResources.Contains(resourceType); // The.Sim.PlaySite.Resources.TryGetValue(resourceType, out listOfEntities) && listOfEntities.Count > 0;
        }

        private void HandleResource(string checkboxTooltip, ResourceType resourceType, int noOfResourceContainers, Predicate<ResourceType> resourceExists)
        {

            CollapsablePanel cpCategory;
            Grid grdCategory = null;
            UIComponent categoryRow;
            UIComponent itemRow;

            ResourceCategory resourceCategory = resourceType.Category;

            if (noOfResourceContainers > 0)
            {
                cpCategory = null;
                grdCategory = null;

                EntityType entityType = resourceType.ResourceItemType;

                if (grdCategory == null)
                {
                    // get the category panel and grid, or add them if needed:
                    if (categoryGrid.TryGetEntry(resourceCategory, out categoryRow))
                    {

                        cpCategory = categoryRow as CollapsablePanel;
                        grdCategory = (Grid)cpCategory.ExpandedPanel.Controls[0];

                    }
                    else
                    {
                        // first time we encounter this category. Fill the buttons with defaults.
                        AddResourceCategoryRow(ref cpCategory, ref grdCategory, resourceCategory);

                        UpdateCategoryRow(cpCategory, The.InGameUI.OverlaySettings.ResourceCategoriesToDisplay[resourceCategory]);
                        // categoryIsJustAdded = true;
                        grdCategory.BeginAddingEntries();
                        childGridsThatWereChanged.Add(grdCategory); // save the grid for resize at the end of update
                    }
                }


                if (!grdCategory.TryGetEntry(resourceType, out itemRow))
                {
                    itemRow = AddResourceTypeRow(grdCategory, resourceType, resourceCategory, checkboxTooltip);
                }

                bool setting = The.InGameUI.OverlaySettings.DisplayResourceType(resourceType);
                UpdateResourceTypeRow(itemRow, setting, cpCategory);

            }

            #region Clean-up
            // cleanup/delete 
            // remove entity types that should no longer be displayed  

            HashSet<ResourceID> listOfEntities;

            if (!resourceExists(resourceType)) // sharedKnowledge.PlaySiteKnowledge.AllKnownResourceContainers.TryGetValue(resourceType, out listOfEntities) || listOfEntities.Count == 0)
            {
                if (grdCategory == null)
                {
                    if (categoryGrid.TryGetEntry(resourceCategory, out categoryRow))
                    {
                        cpCategory = categoryRow as CollapsablePanel;
                        grdCategory = (Grid)cpCategory.ExpandedPanel.Controls[0];

                        grdCategory.BeginAddingEntries();
                        childGridsThatWereChanged.Add(grdCategory); // save the grid for resize at the end of update
                    }
                }

                if (grdCategory != null)
                {
                    grdCategory.TryRemoveEntry(resourceType);

                    if (grdCategory.Entries.Count == 0)
                    {
                        categoryGrid.TryRemoveEntry(resourceCategory);
                        if (grdCategory != null)
                        {
                            childGridsThatWereChanged.Remove(grdCategory);
                        }
                    }
                }

            }
            #endregion
        }

        /*   private void PopulateEmptyCategory(SharedKnowledge sharedKnowledge, string checkboxTooltip)
           {
               //adds an emptyCategory, we use it for colonymembers because we dont populate that grid now
               UIComponent itemRow = null;
               CollapsablePanel cpCategory = null;
               Grid grdCategory = null;

               if (!categoryGrid.TryGetEntry(EntityGrouping.ColonyMembers, out itemRow))
               {              
                   AddEmptyCategoryRow(ref cpCategory, ref grdCategory, EntityGrouping.ColonyMembers, OverlaySettings.GetName(EntityGrouping.ColonyMembers));
                   UpdateCategoryRow(cpCategory, The.InGameUI.OverlaySettings.EntityTypeGroupingsToDisplay[EntityGrouping.ColonyMembers]);
               }
           }*/

        private void AddAndUpdateEntityGroupingAndRow(EntityType entityType, string checkboxTooltip, out Grid grdGrouping, EntityGrouping grouping)
        {
           
            CollapsablePanel cpGrouping;
            UIComponent groupingRow;

            cpGrouping = null;
            grdGrouping = null;

            if (grdGrouping == null)
            {
                // get the category panel and grid, or add them if needed:
                if (categoryGrid.TryGetEntry(grouping, out groupingRow))
                {
                    // not needed..?
                    cpGrouping = groupingRow as CollapsablePanel;
                    grdGrouping = (Grid)cpGrouping.ExpandedPanel.Controls[0];
                    /*
                                       if (!entityCategoryPanels.Exists(t => t.Item2 == cpCategory))
                                       {
                                           entityCategoryPanels.Add(new Tuple<EntityGrouping, CollapsablePanel>((EntityGrouping)grouping, cpCategory)); // save it for later..

                                           // we only have to update the category once:                        
                                           UpdateCategoryRow(cpCategory, The.InGameUI.OverlaySettings.EntityTypeGroupingsToDisplay[(EntityGrouping)grouping]);
                                       }*/
                }
                else
                {
                    // first time we encounter this category. Fill the buttons with defaults.
                    AddEntityGroupingRow(ref cpGrouping, ref grdGrouping, grouping, OverlaySettings.GetName(grouping));

                    UpdateCategoryRow(cpGrouping, The.InGameUI.OverlaySettings.EntityTypeGroupingsToDisplay[grouping]);

                    // categoryIsJustAdded = true;
                    grdGrouping.BeginAddingEntries();
                    childGridsThatWereChanged.Add(grdGrouping); // save the grid for resize at the end of update

                }
            }

            UIComponent itemRow;
            if (!grdGrouping.TryGetEntry(entityType, out itemRow))
            {
                if (checkboxTooltip == "")
                {
                    if (grouping.Equals(EntityGrouping.Animals))
                    {
                        checkboxTooltip = "Show animal";
                    }
                    else if (grouping.Equals(EntityGrouping.Interest))
                    {
                        checkboxTooltip = "Show places of interest";
                    }
                }
                itemRow = AddEntityTypeRow(grdGrouping, entityType, grouping, checkboxTooltip);
            }

            UpdateEntityTypeRow(itemRow, The.InGameUI.OverlaySettings.EntityTypesToDisplay[entityType], cpGrouping);

        }

        #endregion

        private int GetCollapsablePanelWidth()
        {
            return categoryGrid.Width - 16;
        }

        #region AddCategoryAndGroupingRows

        /// <summary>
        /// can be: EntityGrouping, ResourceCategory
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="grdChild"></param>
        /// <param name="entityType"></param>
        /// <param name="key"></param>
        private void CreateCategoryRow(ref CollapsablePanel cp, ref Grid grdChild, object key, string title, Color color, ClickHandler clickHandler)
        {
            /* outerCategoryGrid.ScrollBarEnabled = true;
             outerCategoryGrid.ScrollBar.X = 231 - 16;            
             */

            cp = new CollapsablePanel(gui, CollapsablePanel.PanelType.HUDSmall);
            cp.CollapsedHeight = categoryGrid.ItemHeight;
            cp.Init();
            cp.Title = title;
            //  cp.Width = GetCollapsablePanelWidth(); // width is set when RefreshMargins is called
            cp.TitleSummaryRightAlignXPos = quantityX;
            cp.TitlePositionX = quantityX;
            cp.ToolTip = "Click to see more detailed options";
            // cp.DebugTag = "cpOverlayCategory";
            cp.CollapsablePanelRightPadding = 1;

            categoryGrid.AddEntry(key, cp);

            grdChild = new Grid(gui, ListBoxType.HUDAndLCD, Label.LabelType.HUDWindow);
            grdChild.FixedItemHeights = true;
            grdChild.Width = 210; // cp.Width - 15; 
            grdChild.VMargin = singleSpacing - 2; // create space above and below the list of rows
            grdChild.ScrollBarEnabled = false;
            grdChild.ItemHeight = 25;
            grdChild.CanGrowInHeight = true;
            grdChild.Font = GUIManager.LCDandHUDBodyFontPath;
            grdChild.IsOuterGrid = false;
            grdChild.DebugTag = "grdChild";

            cp.AddContent(grdChild);

            if (key as ResourceCategory != null)
            {
                Image scanIcon = new Image(gui);
                scanIcon.ToolTip = "When activating the SCAN button, these resources will also be visible in the terrain view.";
                scanIcon.SetSkinLocation(SkinState.Normal, gui.GUISpriteSheet.GetSourceRectangle("HUD_icon_scan"));
                scanIcon.ResizeControlToFitImage();
                scanIcon.X = quantityX;
                scanIcon.ID = UIComponent.DataControlID.StatusIcon;
                scanIcon.Visible = true;

                cp.CenterOnHeader(scanIcon);
                cp.TitlePositionX = scanIcon.Right + singleSpacing - 2;
                cp.Add(scanIcon);
            }

            CheckBox cbSelect = new CheckBox(gui);
            cbSelect.Init(CheckBoxType.HUDCheckBox);
            cbSelect.ToolTip = "Select all / Deselect all";
            cbSelect.ID = UIComponent.DataControlID.Selector;
            cbSelect.X = 188; // cp.Width - cbSelectDeselect.Width;
            cbSelect.Y = 4;
            cbSelect.Visible = true;
            cbSelect.IsChecked = false;
            cbSelect.Tag1 = key;
            cbSelect.Click += clickHandler;
            cbSelect.SetNormalColor(color);
            cp.Add(cbSelect);
            cbSelect.DebugTag = "cbOverlaySelect";

            Image checkIcon = new Image(gui);
            checkIcon.ToolTip = "Some items in the category have overriding settings.";
            checkIcon.SetSkinLocation(SkinState.Normal, gui.GUISpriteSheet.GetSourceRectangle("HUD_exclamationmark_parenthesis"));
            checkIcon.ResizeControlToFitImage();
            checkIcon.X = cbSelect.X - 20;
            checkIcon.ID = UIComponent.DataControlID.HasOverridingItemIcon;
            checkIcon.Visible = false;
            checkIcon.Color = color;

            cp.CenterOnHeader(checkIcon);
            cp.Add(checkIcon);

            // cpCategory.CenterOnHeader(cbSelectDeselect);



        }

        private void AddResourceCategoryRow(ref CollapsablePanel cpCategory, ref Grid categoryGrid, ResourceCategory resourceCategory)
        {
            Color color = resourceCategory.Color.Value;

            CreateCategoryRow(ref cpCategory, ref categoryGrid, resourceCategory, resourceCategory.Name, color, cbSelectDeselectResourceCategory_Click);
        }

        private void AddEntityGroupingRow(ref CollapsablePanel cpGrouping, ref Grid groupingGrid, EntityGrouping entityGrouping, string title)
        {
            Color color = OverlaySettings.GetGroupingColor(entityGrouping, false).Value;

            CreateCategoryRow(ref cpGrouping, ref groupingGrid, entityGrouping, title, color, cbSelectDeselectEntityGrouping_Click);
        }

        /*  private void AddEmptyCategoryRow(ref CollapsablePanel cpCategory, ref Grid categoryGrid, EntityGrouping entityGrouping, string title)
          {
              Color color = OverlaySettings.GetGroupingColor(entityGrouping, false).Value;

              CreateChildGridRow(ref cpCategory, ref categoryGrid, entityGrouping, title, color, cbSelectDeselectEntityGrouping_Click);
          }*/

        #endregion

        #region AddGridRows

        private UIComponent AddResourceTypeRow(Grid grid, ResourceType resourceType, object resourceCategory, string checkboxTooltip)
        {
            ResourceTypeButtonEventArgs eventArgs = new ResourceTypeButtonEventArgs(resourceType);
            UIComponent item = CreateEntityTypeRow(resourceType, null, checkboxTooltip, eventArgs, DisplayWindow.Width, resourceCategory);

            grid.AddEntry(resourceType, item);

            return item;
        }

        private UIComponent AddEntityTypeRow(Grid grid, EntityType entityType, EntityGrouping grouping, string checkboxTooltip)
        {
            ItemTypeButtonEventArgs eventArgs = new ItemTypeButtonEventArgs(entityType);
            UIComponent item = CreateEntityTypeRow(null, entityType, checkboxTooltip, eventArgs, DisplayWindow.Width, grouping);

            grid.AddEntry(entityType, item);

            return item;
        }

        #endregion

        #region Update Category and Item rows

        private void UpdateEntityTypeRow(UIComponent itemRow, bool selected, CollapsablePanel cpCategory)
        {
            CheckBox cBox = (CheckBox)itemRow.FindChildById(UIComponent.DataControlID.Selector);
            EntityType entityType = (EntityType)cBox.Tag1;
            EntityGrouping grouping = OverlaySettings.GetGrouping(entityType).Value;

            cBox.IsChecked = selected;
            UpdateOverridingSettingIcon(cpCategory, grouping);
        }

        private void UpdateResourceTypeRow(UIComponent itemRow, bool selected, CollapsablePanel cpCategory)
        {
            CheckBox cBox = (CheckBox)itemRow.FindChildById(UIComponent.DataControlID.Selector);
            ResourceType resourceType = (ResourceType)cBox.Tag1;
            ResourceCategory category = resourceType.Category;

            cBox.IsChecked = selected;
            UpdateOverridingSettingIcon(cpCategory, category);
        }


        private void UpdateCategoryRow(CollapsablePanel cpCategory, bool selected)
        {
            CheckBox cBox = (CheckBox)cpCategory.FindChildById(UIComponent.DataControlID.Selector);

            cBox.IsChecked = selected;
            UpdateOverridingSettingIcon(cpCategory, cBox.Tag1);
        }

        #endregion

        private void UpdateSingleItemRowEditor(UIComponent row)
        {
            CheckBox cb;
            row.FindChildById(UIComponent.DataControlID.Selector, out cb);
            cb.IsChecked = The.InGameUI.OverlaySettings.EditorOverlayTypeSettings[(EditorOverlayTypes)row.Tag1];
        }
        
        private void UpdateSingleItemRow(UIComponent row)
        {
            CheckBox cb;
            row.FindChildById(UIComponent.DataControlID.Selector, out cb);
            cb.IsChecked = The.InGameUI.OverlaySettings.OverlayTypeSettings[(OverlayTypes)row.Tag1];
        }

       // private UIComponent AddSingleItemRow(OverlayTypes type, string tooltip) 
        private UIComponent AddSingleItemRow(object key, string text, string captionTooltip, string checkboxTooltip, string spriteName, Color? color, ClickHandler clickHandler) //Rectangle rect) 
        {

            UIComponent item = new UIComponent(gui);
            grdSingleItems.AddEntry(key, item);

            if (spriteName != null)
            {
                Rectangle rect = gui.GUISpriteSheet.GetSourceRectangle(spriteName);  //OverlaySettings.GetIconFromOverlayType(type));

                Image icon = new Image(gui);
                item.Add(icon);
                icon.Texture = gui.GUISpriteSheet.Texture;
                icon.SetSkinLocation(SkinState.Normal, rect);
                icon.ResizeControlToFitImage();
                icon.X = 2;
                item.AlignVertically(icon);
            }

            Label lblCaption = new Label(gui);
            item.Add(lblCaption);
            lblCaption.Init(Label.LabelType.HUDWindow);
            lblCaption.X = captionX;
            lblCaption.Text = text; // OverlaySettings.GetTextFromOverlayType(type);
            lblCaption.FitToText();
            item.AlignVertically(lblCaption);
            lblCaption.ToolTip = captionTooltip; // OverlaySettings.GetTooltipFromOverlayType(type);

            CheckBox cbSelect = new CheckBox(gui);
            cbSelect.Init(CheckBoxType.HUDCheckBox);
            cbSelect.ID = UIComponent.DataControlID.Selector;
            cbSelect.ToolTip = checkboxTooltip;
            //    cbSelect.EventArgs = eventArgs;
            item.AlignVertically(cbSelect);
            cbSelect.X = 182;
            cbSelect.ToolTip = checkboxTooltip; // OverlaySettings.GetTooltipFromOverlayType(type);
            // cbSelect.AlignRight(GetCollapsablePanelWidth());
            //cbSelect.X = lblCaption.Right;

            cbSelect.IsChecked = false;

           // Color? color = null;

            cbSelect.Tag1 = key; // type;
            cbSelect.Click += clickHandler;
         //   color = OverlaySettings.GetColorFromOverlayType(type);


            cbSelect.SetNormalColor(color);

            item.Add(cbSelect);

            //  grdSingleItems.AddEntry(type, item);

            return item;
        }

        

        /// <summary>
        /// uses either the resource item or the entity type
        /// </summary>
        /// <param name="resourceType"></param>
        /// <param name="entityType"></param>
        /// <param name="tooltip"></param>
        /// <param name="eventArgs"></param>
        /// <param name="menuWidth"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private UIComponent CreateEntityTypeRow(ResourceType resourceType, EntityType entityType, string tooltip, EventArgs eventArgs, int menuWidth, object key)//, bool selected)
        {

            UIComponent item = new UIComponent(gui);
            EntityType entityTypeForIcon = entityType ?? resourceType.ResourceItemType;
            IconInfo iconInfo;
            Rectangle rect = entityTypeForIcon.GetIconSprite(out iconInfo);


            Image icon = new Image(gui);
            item.Add(icon);
            icon.Texture = gui.GUISpriteSheet.Texture;
            icon.ResizeControlToFitImage();
            icon.X = 2;
            icon.SetSkinLocation(SkinState.Normal, rect);


            
                DataTypeButton tbCaption = new DataTypeButton(gui, DataSheet.InfoToShow.Production, entityTypeForIcon, null, true);
                tbCaption.Text = entityTypeForIcon.Name;
                tbCaption.Init(TextButton.TextButtonType.HUDToolTipWhite);
                tbCaption.ID = UIComponent.DataControlID.Caption;
                tbCaption.IsRoot = true;
                tbCaption.TextAlignment = TextButton.TextAlign.Left;
                tbCaption.X = 2 * doubleSpacing;//32 + icon.X;
                tbCaption.Width = 160; // inStockX - resourceTextX; // quantityX - captionX;
            

            CheckBox cbSelect = new CheckBox(gui);
            cbSelect.Init(CheckBoxType.HUDCheckBox);
            cbSelect.ID = UIComponent.DataControlID.Selector;
            cbSelect.ToolTip = tooltip;
            cbSelect.EventArgs = eventArgs;
            cbSelect.Y = icon.Y + 3;
            cbSelect.Height = 1;
            // cbSelect.Width = 40;
            cbSelect.X = tbCaption.X + tbCaption.Width;//tbCaption.X + tbCaption.Width + 10;
            // cbSelect.Init(CheckBoxType.HUDCheckBox);           
            cbSelect.IsChecked = false;

            Color? color = null;
            if (resourceType != null)
            {
                cbSelect.Tag1 = resourceType;
                cbSelect.Click += cbSelectResourceType_Click;
                color = resourceType.Category.Color;
            }
            else
            {
                cbSelect.Tag1 = entityType;
                cbSelect.Click += cbSelectEntityType_Click;
                color = OverlaySettings.GetGroupingColorFromEntityType(entityType); //, false);
            }

            cbSelect.SetNormalColor(color);

            item.Add(cbSelect);
            item.Add(tbCaption);

            return item;
        }



        #region Select-Deselect All

        //Selects - Deselects All entity in the cpGrouping
        private static void SelectDeselectEntityTypesInGrouping(CollapsablePanel cpGrouping, bool selectAll)
        {
            Grid itemGrid = ((Grid)cpGrouping.ExpandedPanel.Controls[0]);
            CheckBox cbSelectDeselect;
            The.InGameUI.OverlaySettings.EntityTypeGroupingsToDisplay[(EntityGrouping)cpGrouping.Tag1] = selectAll;

            foreach (var item in itemGrid.Entries)
            {
                EntityType entityType = (EntityType)item.Tag1;

                cbSelectDeselect = (CheckBox)item.FindChildById(UIComponent.DataControlID.Selector);
                cbSelectDeselect.IsChecked = selectAll;

                SelectDeselectEntityType(selectAll, entityType);
            }
        }

        //Selects - Deselects All resource in the cpCategory
        private static void SelectDeselectResourceTypes(CollapsablePanel cpCategory, bool selectAll)
        {
            Grid itemGrid = ((Grid)cpCategory.ExpandedPanel.Controls[0]);
            CheckBox cbSelectDeselect;
            The.InGameUI.OverlaySettings.ResourceCategoriesToDisplay[(ResourceCategory)cpCategory.Tag1] = selectAll;

            foreach (var item in itemGrid.Entries)
            {
                ResourceType resourceType = (ResourceType)item.Tag1;

                cbSelectDeselect = (CheckBox)item.FindChildById(UIComponent.DataControlID.Selector);
                cbSelectDeselect.IsChecked = selectAll;

                SelectDeselectResourceType(selectAll, resourceType);
            }
        }

        #endregion

        #region Select-Deselect Single

        //Selects - Deselects an entity in the datasource
        private static void SelectDeselectResourceType(bool select, ResourceType resourceType)
        {
            /* if (!The.InGameUI.OverlaySettings.ResourceTypesToDisplay.ContainsKey(resourceType))
             {
                 The.InGameUI.OverlaySettings.ResourceTypesToDisplay.Add(resourceType, select);
             }
             else
             {*/
            The.InGameUI.OverlaySettings.ResourceTypesToDisplay[resourceType] = select;
            //  }
        }

        //Selects - Deselects a resource in the datasource
        private static void SelectDeselectEntityType(bool select, EntityType entityType)
        {

            /* if (!The.InGameUI.OverlaySettings.EntityTypesToDisplay.ContainsKey(entityType))
             {
                 The.InGameUI.OverlaySettings.EntityTypesToDisplay.Add(entityType, select);
             }
             else
             {*/
            The.InGameUI.OverlaySettings.EntityTypesToDisplay[entityType] = select;
            //  }
        }

        #endregion

        #region Select-Deselect All Click event handlers

        //Header checkbox click event for entityGroupings
        void cbSelectDeselectEntityGrouping_Click(UIComponent sender, EventArgs e)
        {
            CheckBox cbSelectGrouping = sender as CheckBox;
            CollapsablePanel cpGrouping = (CollapsablePanel)cbSelectGrouping.Parent;

            SelectDeselectEntityTypesInGrouping(cpGrouping, cbSelectGrouping.IsChecked);
            UpdateOverridingSettingIcon(cpGrouping, cbSelectGrouping.Tag1);

            The.InGameUI.Minimap.SetSettingsDirty();
        }

        //Header checkbox click event for resourceCategories
        void cbSelectDeselectResourceCategory_Click(UIComponent sender, EventArgs e)
        {
            CheckBox cbSelectDeselect = sender as CheckBox;
            CollapsablePanel cpCategory = (CollapsablePanel)cbSelectDeselect.Parent;

            SelectDeselectResourceTypes(cpCategory, cbSelectDeselect.IsChecked);
            UpdateOverridingSettingIcon(cpCategory, cbSelectDeselect.Tag1);

            The.InGameUI.Minimap.SetSettingsDirty();
        }

        #endregion

        #region Select-Deselect Single Click event handlers

        //entityGroupingRow Checkbox click event for entyties
        void cbSelectResourceType_Click(UIComponent sender, EventArgs e)
        {
            CheckBox cBox = (CheckBox)sender;
            ResourceType resourceType = (ResourceType)(cBox.Parent).Tag1;
            ResourceCategory resourceCategory = resourceType.Category;
            CollapsablePanel cpCategory = (CollapsablePanel)categoryGrid.EntriesByKey[resourceCategory];

            SelectDeselectResourceType(cBox.IsChecked, resourceType);
            UpdateOverridingSettingIcon(cpCategory, resourceCategory);

            The.InGameUI.Minimap.SetSettingsDirty();
        }

        //resourceCategoryRow Checkbox click event for resources
        void cbSelectEntityType_Click(UIComponent sender, EventArgs e)
        {
            CheckBox cbSelectEntityType = (CheckBox)sender;
            EntityType entityType = (EntityType)cbSelectEntityType.Tag1;
            EntityGrouping grouping = OverlaySettings.GetGrouping(entityType).Value;
            CollapsablePanel cpGrouping = (CollapsablePanel)categoryGrid.EntriesByKey[grouping];

            SelectDeselectEntityType(cbSelectEntityType.IsChecked, entityType);
            UpdateOverridingSettingIcon(cpGrouping, grouping);

            The.InGameUI.Minimap.SetSettingsDirty();
        }

        void cbSelectEditorOverlayType_Click(UIComponent sender, EventArgs e)
        {
            CheckBox cbSelect = (CheckBox)sender;
            EditorOverlayTypes type = (EditorOverlayTypes)sender.Tag1;
            The.InGameUI.OverlaySettings.EditorOverlayTypeSettings[type] = cbSelect.IsChecked;       
            
            if (type == EditorOverlayTypes.TerrainDivision && cbSelect.IsChecked)
            {
                // redraw whole map:
                The.Client.Renderer.terrainSlicedMap.RedrawMap(The.Client.Renderer.DiffuseMSRenderTarget);
            }
        }

        void cbSelectOverlayType_Click(UIComponent sender, EventArgs e)
        {
            CheckBox cbSelect = (CheckBox)sender;
            OverlayTypes type = (OverlayTypes)sender.Tag1;
            The.InGameUI.OverlaySettings.OverlayTypeSettings[type] = cbSelect.IsChecked;

            if (type == OverlayTypes.ColonyMembers)
            {
                The.InGameUI.OverlaySettings.EntityTypeGroupingsToDisplay[EntityGrouping.ColonyMembers] = cbSelect.IsChecked;
                The.InGameUI.Minimap.SetSettingsDirty();
            }
        }

        #endregion

        /// <summary>
        /// check if there are visible or hidden item types with a different setting than the category setting
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="category"></param>
        /// <param name="stockpile"></param>
        /// <returns></returns>        
        private bool OneOrMoreItemsInCategoryAreDifferent(CollapsablePanel cp, object category, out bool hiddenItemsAreDifferent)
        {
            hiddenItemsAreDifferent = false;
            CheckBox cBox = (CheckBox)cp.FindChildById(UIComponent.DataControlID.Selector);

            // first check GUI items:
            Grid grid = (Grid)cp.ExpandedPanel.Controls[0];
            if (grid.Entries.Exists(c => ((CheckBox)c.FindChildById(UIComponent.DataControlID.Selector)).IsChecked != cBox.IsChecked)) // check GUI
            {
                return true;
            }
            else
            {
                ResourceCategory keyAsResourceCategory = category as ResourceCategory;
                if (keyAsResourceCategory != null)
                {
                    if (The.InGameUI.OverlaySettings.ResourceCategoryHasDifferentItemSetting(cBox.IsChecked, keyAsResourceCategory))
                    {
                        hiddenItemsAreDifferent = true;
                        return true;
                    }
                }
                else
                {
                    if (The.InGameUI.OverlaySettings.EntityCategoryHasDifferentItemSetting(cBox.IsChecked, (EntityGrouping)category))
                    {
                        hiddenItemsAreDifferent = true;
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// the icon gets shown when some items have a setting that differs from the category setting
        /// </summary>      
        private void UpdateOverridingSettingIcon(CollapsablePanel cpCategory, object category)
        {
            Image overridingIcon = (Image)cpCategory.FindChildById(UIComponent.DataControlID.HasOverridingItemIcon);
            bool hiddenItemsAreDifferent;

            if (OneOrMoreItemsInCategoryAreDifferent(cpCategory, category, out hiddenItemsAreDifferent))
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
    }
}
