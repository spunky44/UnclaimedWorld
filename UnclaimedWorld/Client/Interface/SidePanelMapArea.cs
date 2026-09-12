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
using UWGame.ClientSide.Interface.HUD_Windows;
using UWGame.SimSide.Resources;
using UWGame.SimSide.AI.Goals;
using UWGame.Client;
using UWGame.ClientSide.Interface.Controls;
using UWGame.Client.Interface;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Allegiances;
using UWGame.ClientSide.Interface.Inventory;
namespace UWGame.ClientSide.Interface
{
    public class SidePanelMapArea : RosterPanel //SidePanel
    {
        /* Label lblInfo;
         Label lblGoal;
         */
        FullLCDPanel.SetCollapsedSummary SetSummaryDelegate;

        Grid outerGrid;

        CollapsablePanel cpPersons, cpRobots, cpAnimals, cpResources, cpItems, cpTrees, cpStructures, cpTerrain /*editor only*/;
            //cpEnvironment;
        Grid grdPersons, grdRobots, grdAnimals, grdResources, grdItems, grdTrees, grdStructures, grdTerrain;
           // grdEnvironment;


        public const int GridMargin = 2;
        

        private const bool useUIOWner = true;

        public SidePanelMapArea()
            : base(The.InGameUI.sidePanelHeight, true)
        {
            SetSummaryDelegate = new FullLCDPanel.SetCollapsedSummary(SetSummaryAsTotal);

            InitStatusContentPanel();

            #region commented
            /*  ExpandedPanel = new EntityPanel.EntityPanel(The.InGameUI, The.InGameUI.framedCRT,
                new Point(The.InGameUI.expandedInterfaceLeft, InGameInterface.expandedInterfaceTop),
                new Vector2(The.InGameUI.expandedInterfaceWidth,
                        The.InGameUI.expandedInterfaceHeight), Level.Middle, true, TabButtonPanel.Layout.Vertical, null, null); //, true);
            */



            //  centerClickLeft = new ClickHandler(center_Click);
            //  track_Click = new ClickHandler(center_RightClick);



            /*  Window window = new Window(intf.gui);
              Initialize(window);
              window.ID = "SmallEntityPanel";
              InitSmallPanel();*/
            //, "Entity Form", Color.White, Color.Black, "tahoma", 1f, false, false, false, false, Form.BorderStyle.None, Form.Style.Default));

            //  Form.Add(new TextButton(intf.gui) { Text = "View history", Position = new Vector2(intf.leftMargin, 300)}); //, "View log", 80, Color.White, intf.InterfaceFont, Form.Style.Default));


            /*TextButton expand = new TextButton(intf.gui) { Text = "[+]", Position = new Point(intf.leftMargin, 180) }; // "Expand", new Vector2(intf.leftMargin, 180), "[+]", 40, Color.White, intf.InterfaceFont, Form.Style.Default);
            Form.Add(expand);
            expand.Click += new ClickHandler(ExpandEntityPanel_OnPress);
            */


            // Maybe display these labels somewhere on the lcd?
            /*   lblInfo = new Label(intf.gui);
               lblInfo.X = (int)intf.leftMargin;
               lblInfo.Y = 20;
               lblInfo.Text = "";
               lblInfo.Width = Interface.InterfaceWidth - (int)intf.leftMargin;
               lblInfo.Height = 400;//lbl.TextHeight;
               Form.Add(lblInfo);

               lblGoal = new Label(intf.gui);
               lblGoal.X = (int)intf.leftMargin;
               lblGoal.Y = 240;
               lblGoal.Text = "";
               lblGoal.Width = Interface.InterfaceWidth - (int)intf.leftMargin;
               lblGoal.Height = 480;//lbl.TextHeight;
               Form.Add(lblGoal); */

            //***********************************

            // AddLCD();

            /*
            FullLCDPanel.AddLCDPanelFitWindowWithBottomMargin(The.InGameUI, Form, 34, new Point(MarginX, MarginY), out display,  out lcdSurface, ref lcdScreen);

            AddOverlayDetails(display);
            */
            #endregion

            int itemHeight = ItemHeight;

            outerGrid = CreateOuterGridForCollapsableLists(The.InGameUI.gui, lcdSurface);
            //  AddCollapsablePanelAndGrid(The.InGameUI.gui, outerGrid, "Transporting", itemHeight, out cpOnboard, out onboardGrid);

            AddCollapsablePanelAndGrid(The.InGameUI.gui, outerGrid, "PERSONS", itemHeight, out cpPersons, out grdPersons, Grid.SelectabilityOptions.None, 0);
            AddCollapsablePanelAndGrid(The.InGameUI.gui, outerGrid, "ROBOTS", itemHeight, out cpRobots, out grdRobots, Grid.SelectabilityOptions.None, 5);        
            AddCollapsablePanelAndGrid(The.InGameUI.gui, outerGrid, "ANIMALS", itemHeight, out cpAnimals, out grdAnimals, Grid.SelectabilityOptions.None, 10);
            AddCollapsablePanelAndGrid(The.InGameUI.gui, outerGrid, "STRUCTURES", itemHeight, out cpStructures, out grdStructures, Grid.SelectabilityOptions.None, 15);
            AddCollapsablePanelAndGrid(The.InGameUI.gui, outerGrid, "ITEMS", itemHeight, out cpItems, out grdItems, Grid.SelectabilityOptions.None, 20);
            AddCollapsablePanelAndGrid(The.InGameUI.gui, outerGrid, "RESOURCES", itemHeight, out cpResources, out grdResources, Grid.SelectabilityOptions.None, 25);
            AddCollapsablePanelAndGrid(The.InGameUI.gui, outerGrid, "PLANTS", itemHeight, out cpTrees, out grdTrees, Grid.SelectabilityOptions.None, 35);
          
            if (The.Sim.Mode == Sim.EngineMode.Edit)
            {
                AddCollapsablePanelAndGrid(The.InGameUI.gui, outerGrid, "TERRAIN", itemHeight, out cpTerrain, out grdTerrain, Grid.SelectabilityOptions.None, 45);
            }
        }

        private void InitStatusContentPanel()
        {
            statusContent = The.InGameUI.StatusScreen.GetNewSurfaceContent();

            Game game = The.Sim.Controller.Game;
            GUIManager gui = The.InGameUI.gui;

            imStatusBackground = new Image(gui);
            statusContent.Add(imStatusBackground);

            InitStatusCRTHeader(gui, statusContent, statusTextX, out lblStatusHeading, out crtUnderline);

            #region commented
            //  InitBillboardPanel(); // needed??

            // TODO: insert landscape picture here:
            /*   Rectangle rect = gui.GUI_CRT_SpriteSheet.GetSourceRectangle("human_b_m_adult_1"); 
               imStatusBackground.Texture = gui.GUI_CRT_SpriteSheet.Texture;
               imStatusBackground.SetSkinLocation(SkinState.Normal,rect);
               imStatusBackground.Position = new Point(0, 2);
               imStatusBackground.ScaleImageToSizeOfControl = false;
               imStatusBackground.ResizeControlToFitImage();
               imStatusBackground.RenderType = RenderType.CRTAndLCD;
               imStatusBackground.Alpha = 1f; // 0.35f;// use for background for text!
               */



            /*   lblStatusInfo1 = new Label(gui);
               statusContent.Add(lblStatusInfo1);
               lblStatusInfo1.Position = new Point(statusTextX, 80);
               lblStatusInfo1.Text = "MALE";
               lblStatusInfo1.Init(Label.LabelType.CRTSmall);
               lblStatusInfo1.Width = statusTextWidth;

               lblStatusInfo2 = new Label(gui);
               statusContent.Add(lblStatusInfo2);
               lblStatusInfo2.Position = new Point(statusTextX, 100);
               lblStatusInfo2.Text = "AGE: 34";
               lblStatusInfo2.Init(Label.LabelType.CRTSmall);
               lblStatusInfo2.Width = statusTextWidth;

               lblStatusInfo3 = new Label(gui);
               statusContent.Add(lblStatusInfo3);
               lblStatusInfo3.Position = new Point(statusTextX, 135);
               lblStatusInfo3.Text = "MANUAL LABORER";
               lblStatusInfo3.Init(Label.LabelType.CRTSmall);
               lblStatusInfo3.Color = Color.PowderBlue;
               lblStatusInfo3.Width = statusTextWidth;

               lblStatusInfo4 = new Label(gui);
               statusContent.Add(lblStatusInfo4);
               lblStatusInfo4.Position = new Point(statusTextX, 155);
               lblStatusInfo4.Text = "WALKING";
               lblStatusInfo4.Init(Label.LabelType.CRTSmall);
               lblStatusInfo4.Color = Color.PowderBlue;
               lblStatusInfo4.Width = statusTextWidth;*/
            #endregion
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

        private void RefreshStatusScreen()
        {
            /*
            lblStatusInfo1.Text = "";
            lblStatusInfo2.Text = "";
            lblStatusInfo3.Text = "";
            lblStatusInfo4.Text = "";
            */

            lblStatusHeading.Text = "";

            //  lblStatusHeading.DebugTag = "missingHeader";

            MapArea mapArea;
            mapArea = TileSelectionContextMenu.GetMapArea();

            if (mapArea == null) // no selection
            {
                return;
            }

            SetHeaderText("LAND");

            /*
            if (!string.IsNullOrEmpty(data.EntityType.Thumbnail))
            {
                ShowBackgroundImage();
                statusContent.Remove(pnBillboards);

                Rectangle rect = intface.gui.GUI_CRT_SpriteSheet.GetSourceRectangle(data.EntityType.Thumbnail);
                imStatusBackground.Texture = intf.gui.GUI_CRT_SpriteSheet.Texture;
                imStatusBackground.SetSkinLocation(SkinState.Normal,rect);
                imStatusBackground.Position = new Point(0, 2);
                imStatusBackground.ScaleImageToSizeOfControl = false;
                imStatusBackground.ResizeControlToFitImage();
                imStatusBackground.Alpha = 1f;
            }
            else
            {
                // clear it... TODO: models? or thumbnails
                statusContent.Remove(imStatusBackground);
                statusContent.Remove(pnBillboards);
            }*/
        }

        private void ShowBackgroundImage()
        {
            // important! add on top of noise background, but behind text labels!
            statusContent.Insert(imStatusBackground, 1);
        }

        public override void Show()
        {
            //The.InGameUI.StatusScreen.ShowCenterButton();            
            base.Show();
        }

        public override void Hide()
        {

            base.Hide();
        }

        /*   void center_Click(UIComponent sender, EventArgs e)
           {
               //((ImageButton)sender).IsChecked = false;
               intface.EnableTracking(false);
               if (The.InGameUI.SelectedEntity != null)
               {
                   IKnownEntityData data;
                   The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(The.InGameUI.SelectedEntity.Value, out data);
                   if (data != null)
                   {
                       intface.ZoomToEntity(data);
                   }
               }
           }

           void center_RightClick(UIComponent sender, EventArgs e)
           {
               if (The.InGameUI.SelectedEntity != null)
               {
                   IKnownEntityData data;
                   The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(The.InGameUI.SelectedEntity.Value, out data);
                   if (data != null)
                   {
                       intface.ZoomToEntity(data);
                       intface.EnableTracking(!intface.TrackSelectedEntity);
                   }
               }
               //intf.TrackSelectedEntity = !intf.TrackSelectedEntity;
           }
           */
      

        public override void Refresh()
        {
            RefreshStatusScreen();

            Populate();
        }

        private void Populate()
        {

            outerGrid.BeginAddingEntries();

            PopulatePersons();

            PopulateRobots();

            PopulateAnimals();

            PopulateItems();

            PopulateStructures();

            PopulateResources();

            PopulateTrees();

            if (The.Sim.Mode == Sim.EngineMode.Edit)
            {
                PopulateTerrain();
            }

            // sort panels here
            outerGrid.Sort(u => u.OrderByTag1, Grid.Sorting.Ascending);
           
            outerGrid.EndAddingEntries();
        }

        Dictionary<ResourceType, MapArea.ResourcesAndJobs> data = new Dictionary<ResourceType, MapArea.ResourcesAndJobs>();

        private void PopulateResources()
        {
            data.Clear();

            MapArea mapArea;
            mapArea = TileSelectionContextMenu.GetMapArea();

            mapArea.GetSumOfAllResourcesInArea(The.InGameUI.UIAllegiance.SharedKnowledge, data);

            int resourceTotal = 0;
            grdResources.BeginAddingEntries();

            if (AddPanelIfNotPresent(data.Count > 0, outerGrid, cpResources))
            {
                UIComponent item, itemComponent;

                int noOfResources;

                foreach (var kvp in data)
                {
                    noOfResources = kvp.Value.NumberOfResources;

                    resourceTotal += noOfResources;

                    // see if the item is represented:   
                    if (grdResources.TryGetEntry(kvp.Key, out item)) // grdSkills.TryGetItem(kvp.Key.SkillCategory, out item)) // cpCategory))
                    {
                        itemComponent = item.FindChildById(UIComponent.DataControlID.Stock);
                        if (itemComponent != null)
                        {
                            Label lblAmount = (Label)itemComponent;
                            lblAmount.Text = noOfResources.ToString();
                        }
                    }
                    else
                    {
                        AddResourceRow(kvp.Key, noOfResources);
                    }
                }
            }

            cpResources.Summary = resourceTotal.ToString();

            grdResources.DeleteEntries<ResourceType>(e => data.ContainsKey(e));

            grdResources.EndAddingEntries();
        }

        private void PopulateAnimals()
        {
            MapArea mapArea;
            mapArea = TileSelectionContextMenu.GetMapArea();

            entities.Clear();

            mapArea.GetEntitiesInArea(entities, null,
                e => e.EntityType.Person == null
                    && e.EntityType.IntelligenceType != null
                    && e.EntityType.BiologicalType != null, The.InGameUI.UIAllegiance
                    );

            grdAnimals.BeginAddingEntries();

            if (AddPanelIfNotPresent(entities.Count > 0, outerGrid, cpAnimals))
            {

                //int noOfPersons = 0;
                foreach (IKnownEntityData e in entities)
                {
                    if (!grdAnimals.EntriesByKey.ContainsKey(e.EntityID))
                    {
                        grdAnimals.AddHyperLinkEntry(e.EntityID, e.EntityType.Name, (uint)e.EntityID, hyperLinkMargin);
                    }
                }

                cpAnimals.Summary = entities.Count.ToString();
            }

            // remove grid entries not present:
            grdAnimals.DeleteEntries<EntityID>(
               e => entities.Exists(e1 => e1.EntityID == e));


            grdAnimals.EndAddingEntries();
        }


        private void PopulateStructures()
        {
            MapArea mapArea;
            mapArea = TileSelectionContextMenu.GetMapArea();

            entities.Clear();

            mapArea.GetEntitiesInArea(entities, null,
                e => e.EntityType.StructureType != null, The.InGameUI.UIAllegiance
                    );

            grdStructures.BeginAddingEntries();

            if (AddPanelIfNotPresent(entities.Count > 0, outerGrid, cpStructures))
            {

                //int noOfPersons = 0;
                foreach (IKnownEntityData e in entities)
                {
                    if (!grdStructures.EntriesByKey.ContainsKey(e.EntityID))
                    {
                        grdStructures.AddHyperLinkEntry(e.EntityID, e.EntityType.Name, (uint)e.EntityID, hyperLinkMargin);
                    }
                }

                cpStructures.Summary = entities.Count.ToString();
            }

            // remove grid entries not present:
            grdStructures.DeleteEntries<EntityID>(
               e => entities.Exists(e1 => e1.EntityID == e));


            grdStructures.EndAddingEntries();
        }
        Dictionary<EntityType, int> allItems = new Dictionary<EntityType, int>();
        List<IKnownEntityData> entities = new List<IKnownEntityData>();


        Dictionary<EntityType, List<EntityID>> allItemByType = new Dictionary<EntityType, List<EntityID>>();

        private void PopulateItems()
        {
            MapArea mapArea;
            mapArea = TileSelectionContextMenu.GetMapArea();

            allItemByType.Clear();

            mapArea.GetNoOfItemsInAreaByType(allItemByType,
                e => e.EntityType.ItemType != null, The.InGameUI.UIAllegiance);


            if (AddPanelIfNotPresent(allItemByType.Count > 0, outerGrid, cpItems))
            {
                OwnerID? ownerID = The.InGameUI.GetUIOwnerID();
 
                UIComponent itemRow;

                int total = 0;

                grdItems.BeginAddingEntries();

                EntityType entityType;

                foreach (var item in allItemByType)
                {
                    entityType = item.Key;

                    if (entityType.ItemType != null)
                    {
                        total += item.Value.Count;

                        if (!grdItems.TryGetEntry(entityType, out itemRow))
                        {
                            // add the item row and its category if needed:
                            AddItemRow(grdItems, entityType, null, useUIOWner, tbItems_Click);

                            grdItems.TryGetEntry(entityType, out itemRow);
                        }

                        List<EntityID> listOfItemsIntheArea = GetListOfItems(entityType);

                        UpdateItemRow(ownerID, itemRow, entityType, /*item.Value,*/ listOfItemsIntheArea);
                    }

                    // remove unused items 
                    grdItems.DeleteEntries<object>(
                        e => !(e is EntityType) || allItemByType.ContainsKey((EntityType)e));

                    // and categories:
                    grdItems.DeleteEntries<object>(
                        e => !(e is EntityCategory) || CategoryIsRepresented((EntityCategory)e, allItemByType));

                    grdItems.EndAddingEntries();

                    cpItems.Summary = total.ToString();
                }
            }
        }

        public static bool CategoryIsRepresented(EntityCategory category, Dictionary<EntityType, List<EntityID>> allEntities)
        {
            foreach (var item in allEntities)
            {
                if (item.Key.Category == category && item.Value.Count > 0)
                {
                    return true;
                }
            }

            return false;
        }

        /*   public static void AddEntityTypeInfo(Grid gridToAddInfoTo, EntityType entityType)
           {
           
               DataTypeButton tbCaption = new DataTypeButton(The.InGameUI.gui, DataTypeTooltip.InfoToShow.Data, entityType, null, useUIOWner);
               tbCaption.Init(TextButton.TextButtonType.LCDToolTipWhite);
               tbCaption.ID = UIComponent.DataControlID.Caption;
               tbCaption.IsRoot = true;
               tbCaption.Text = entityType.Name;
               //item.Add(tbCaption);
               tbCaption.TextAlignment = TextButton.TextAlign.Left;
               tbCaption.Width = 160; 
               tbCaption.X = GridMargin;

               gridToAddInfoTo.AddEntry(entityType, tbCaption);
           }*/

        /*public static void AddMemoryTypeInfo(Grid outerGrid, ref Label memoryFactLabel, string memoryFactInfo)
        {
            UIComponent item;
            if (memoryFactLabel == null)
            {
                memoryFactLabel = new Label(The.InGameUI.gui);
            }

            if (outerGrid.TryGetItem(memoryFactLabel, out item) == false)
            {
                memoryFactLabel.Init(Label.LabelType.LCDNormal);
                memoryFactLabel.ID = UIComponent.DataControlID.Caption;

                memoryFactLabel.Text = memoryFactInfo;
                memoryFactLabel.Width = 160;
                memoryFactLabel.X = gridMargin;

                memoryFactLabel.DebugTag = "sidePanelTypeInfo";

                outerGrid.AddEntry(memoryFactLabel, memoryFactLabel);
            }
        }*/



        public static UIComponent AddItemRow(Grid grdItems, EntityType entityType, EntityGroup owner, bool useCurrentUIOwner, Action<UIComponent, EventArgs> clickMethodToSeeListOfItems) //, EventArgs eventArgs)
        {
            Image icon;
            UIComponent item;

            item = new UIComponent(The.InGameUI.gui);

            //  item.DebugTag = "";

            if (entityType.Name == "Firewood")
            {
            }
            DataTypeButton tbCaption = new DataTypeButton(The.InGameUI.gui, DataSheet.InfoToShow.Data, entityType, GoalEvaluator.GetOwnerID(owner), useCurrentUIOwner);
            tbCaption.Init(TextButton.TextButtonType.LCDToolTipBlack);
            tbCaption.ID = UIComponent.DataControlID.Caption;
            tbCaption.IsRoot = true;
          //  tbCaption.Text = entityType.PluralName; // .Name;
            item.Add(tbCaption);
            tbCaption.TextAlignment = TextButton.TextAlign.Left;
            tbCaption.Width = 160; // DisplayWindow.Width - tbCaption.X - 2 * doubleSpacing;
            tbCaption.X = GridMargin;

          //  ItemTypeButtonEventArgs eventArgs = new ItemTypeButtonEventArgs(entityType);

            // TODO: disable clicking when the container is a memory fact!
            StockButton tbItems = new StockButton(The.InGameUI.gui, entityType);
            item.Add(tbItems);
            tbItems.ScaleWidthToFitText();
            tbItems.Position = new Point(tbCaption.Right + 6, 0);           
         //   tbItems.EventArgs = eventArgs; // new ItemTypeButtonEventArgs(entityType);
            tbItems.Click += new ClickHandler(clickMethodToSeeListOfItems); // tbItems_Click);
            tbItems.ID = UIComponent.DataControlID.Stock;

            InsertItemAndCategory(grdItems,
                entityType,
                entityType.Category,
                entityType.Category.Name.ToUpper(Config.Culture),
                null, //entityType.Category.SortOrder,
                item,
                null,
                CreateCategoryHeaderRow); //FindIndexOfNextCategoryRow);

            return item;
        }

        public static void InsertItemAndCategory(Grid grid, EntityType entityType, object categoryRowKey, string categoryName, int? categorySortOrder, /*int? categoryTotal,*/ UIComponent item,
            Func<Grid, int, int> findIndexOfNextCategoryRow,
            Func<string, int?, UIComponent> createCategoryHeaderRow)
        {
            UIComponent categoryRow;
            if (grid.TryGetEntry(categoryRowKey, out categoryRow))
            {
                // the category row exists already. get its index:
                int index = grid.Entries.IndexOf(categoryRow);

                UIComponent row;

                EntityType itemType;

                // look through the items under it to find the correct place
                do
                {
                    index++;

                    if (index < grid.Entries.Count)
                    {
                        row = grid.Entries[index];

                        itemType = row.Tag1 as EntityType;
                        if (itemType != null)
                        {
                            if (string.CompareOrdinal(itemType.Name, entityType.Name) > 0) // if this row comes after, then insert the item just before it.
                            {
                                //grid.Entries.Insert(index - 1, item);
                                grid.AddEntry(entityType, item, index);
                                return;
                            }
                        }
                        else
                        {
                            // we reached the next categroy row. insert item just before it:
                            grid.AddEntry(entityType, item, index);
                            return;
                            //itemCategory = row.Tag as EntityCategory;
                        }
                    }
                    else
                    {
                        // at the end.
                        grid.AddEntry(entityType, item);
                        return;
                    }


                } while (true);

            }
            else
            {
                // the category does not exist. add it at its correct place:

                int categoryIndex;
                UIComponent newCategoryRow = createCategoryHeaderRow(categoryName, null); // CreateCategoryHeaderRow(categoryName);

                if (findIndexOfNextCategoryRow != null && categorySortOrder != null)
                {
                    // we require sorted categories:
                    //AddCategoryHeaderRow(grid, categoryRowKey, categoryName, categorySortOrder.Value, out categoryIndex, findIndexOfNextCategoryRow);

                    categoryIndex = findIndexOfNextCategoryRow(grid, categorySortOrder.Value);

                    if (categoryIndex == grid.Entries.Count)
                    {
                        // add to the end of the list:
                        grid.AddEntry(categoryRowKey, newCategoryRow); //, index);

                        return;
                    }
                    else
                    {
                        // add just before the next category starts:
                        grid.AddEntry(categoryRowKey, newCategoryRow, categoryIndex);

                    }
                }
                else
                {
                    //The categories are not sorted.  just add the new category at the end:
                    // add to the end of the list:
                    grid.AddEntry(categoryRowKey, newCategoryRow);
                    categoryIndex = grid.Entries.Count - 1;
                }

                // add the item after it:
                grid.AddEntry(entityType, item, categoryIndex + 1);
            }

        }


        private static UIComponent AddEntityTypeRow(Grid grid, EntityType entityType, int amount) //, Action<UIComponent, EventArgs> clickMethodToSeeListOfItems) //, EventArgs eventArgs)
        {
            Image icon;
            UIComponent item;

            item = new UIComponent(The.InGameUI.gui);

            //  item.DebugTag = "";


            DataTypeButton tbCaption = new DataTypeButton(The.InGameUI.gui, DataSheet.InfoToShow.Data, entityType, null, true);
            tbCaption.Init(TextButton.TextButtonType.LCDToolTipBlack);
            tbCaption.ID = UIComponent.DataControlID.Caption;
            tbCaption.IsRoot = true;
            tbCaption.Text = entityType.Name;
            item.Add(tbCaption);
            tbCaption.TextAlignment = TextButton.TextAlign.Left;
            tbCaption.Width = 160; // DisplayWindow.Width - tbCaption.X - 2 * doubleSpacing;
            tbCaption.X = GridMargin;

            // ItemTypeButtonEventArgs eventArgs = new ItemTypeButtonEventArgs(entityType);

            Label lblAmount = new Label(The.InGameUI.gui);
            item.Add(lblAmount);
            lblAmount.Init(Label.LabelType.LCDNormal);
            lblAmount.Text = amount.ToString();
            lblAmount.Position = new Point(tbCaption.Right + 6, 0);
            lblAmount.ID = UIComponent.DataControlID.Stock;
            
            grid.AddEntry(entityType, item);

            return item;
        }

        private void PopulateTrees()
        {
            MapArea mapArea;
            mapArea = TileSelectionContextMenu.GetMapArea();

            allItems.Clear();

            mapArea.GetNoOfEntitiesInArea(allItems,
                e => e.EntityType.TreeType != null,
                The.InGameUI.UIAllegiance);

            if (AddPanelIfNotPresent(allItems.Count > 0, outerGrid, cpTrees))
            {
                // UIComponent categoryRow;
                UIComponent itemRow;

                int total = 0;

                grdTrees.BeginAddingEntries();

                EntityType entityType;

                foreach (var item in allItems)
                {
                    entityType = item.Key;

                    total += item.Value;

                    if (!grdTrees.TryGetEntry(entityType, out itemRow))
                    {
                        // add the item row and its category if needed:
                        AddEntityTypeRow(grdTrees, entityType, item.Value); //, tbItems_Click);
                        grdTrees.TryGetEntry(entityType, out itemRow);
                    }
                    UpdateEntityTypeRow(itemRow, entityType, item.Value);
                }

                // remove unused items 
                grdTrees.DeleteEntries<object>(
                    e => !(e is EntityType) || allItems.ContainsKey((EntityType)e));

                grdTrees.EndAddingEntries();

                cpTrees.Summary = total.ToString();
            }
        }

        private void PopulateTerrain()
        {
            MapArea mapArea;
            mapArea = TileSelectionContextMenu.GetMapArea();

            allItems.Clear();

            mapArea.GetNoOfEntitiesInArea(allItems,
                e => e.EntityType.TerrainType != null,
                The.InGameUI.UIAllegiance);

            if (AddPanelIfNotPresent(allItems.Count > 0, outerGrid, cpTerrain))
            {               
                UIComponent itemRow;

                int total = 0;

                grdTerrain.BeginAddingEntries();

                EntityType entityType;

                foreach (var item in allItems)
                {
                    entityType = item.Key;

                    total += item.Value;

                    if (!grdTerrain.TryGetEntry(entityType, out itemRow))
                    {                       
                        itemRow = AddEntityTypeRow(grdTerrain, entityType, item.Value); 
                        //grdTerrain.TryGetEntry(entityType, out itemRow);
                    }
                    UpdateEntityTypeRow(itemRow, entityType, item.Value);
                }

                // remove unused items 
                grdTerrain.DeleteEntries<object>(
                    e => !(e is EntityType) || allItems.ContainsKey((EntityType)e));

                grdTerrain.EndAddingEntries();

                cpTerrain.Summary = total.ToString();
            }
        }


        private void AddResourceRow(ResourceType resourceType, int amount) //, EventArgs eventArgs)
        {
            Image icon;
            UIComponent item;

            item = new UIComponent(The.InGameUI.gui);


            Label lblCaption = new Label(Interface.gui);
            item.Add(lblCaption);
            lblCaption.Init(Label.LabelType.LCDNormal);
            lblCaption.Text = resourceType.Name;
            lblCaption.X = GridMargin;
            lblCaption.ID = UIComponent.DataControlID.Caption;
            /**EntityTypeButtonEventArgs typeArgs = new EntityTypeButtonEventArgs(resourceType, owner);

            TextButton tbCaption = new TextButton(The.InGameUI.gui);
            tbCaption.Init(TextButton.TextButtonType.LCDToolTipWhite);
            tbCaption.ID = UIComponent.DataControlID.Caption;
            tbCaption.Text = resourceType.Name;
            tbCaption.EventArgs = typeArgs;
            tbCaption.ToolTip = resourceType;
            item.Add(tbCaption);
            tbCaption.TextAlignment = TextButton.TextAlign.Left;
            tbCaption.Width = 160; // DisplayWindow.Width - tbCaption.X - 2 * doubleSpacing;
            tbCaption.X = gridMargin;
            tbCaption.Click += new ClickHandler(The.InGameUI.HandleEntityTypeInfoClick);
           */

            Label lblAmount = new Label(Interface.gui);
            item.Add(lblAmount);
            lblAmount.Init(Label.LabelType.LCDNormal);
            lblAmount.Text = amount.ToString();
            lblAmount.Position = new Point(lblCaption.Right + 6, 0);
            lblAmount.ID = UIComponent.DataControlID.Stock;

            grdResources.AddEntry(resourceType, item);

        }

        private static UIComponent CreateCategoryHeaderRow(string categoryName, int? total)
        {
            UIComponent item;
            item = new UIComponent(The.InGameUI.gui);

            Label label = new Label(The.InGameUI.gui);
            item.Add(label);
            label.Init(Label.LabelType.LCDNormalDark);
            label.Y = 5;
            label.X = 7;
            label.Text = categoryName;
            label.FitToText();


            return item;
        }

       
        public static void UpdateItemRow(OwnerID? thisOwner, UIComponent itemRow, EntityType entityType, List<EntityID> itemsOfType) //, int amount)
        {
            int noOfIncompleteItems = 0, noOfAvailableItems = 0, noOfAvailableItemsIncludingIntrinsic = 0, noOfItemsUsedAsParts = 0, noOfItemsOffSite = 0, noOfItemsOwnedByOthers = 0;
            List<EntityID> availableEntities = null;
            List<EntityID> unavailableEntities = null;


            foreach (var entityID in itemsOfType)
            {
                Entity entity = Entity.FindByID(entityID);
                if (entity != null)
                {
                    EntityGroup.CountEntity(thisOwner, entity, ref noOfIncompleteItems, ref noOfItemsUsedAsParts, ref noOfItemsOffSite, ref noOfItemsOwnedByOthers, ref noOfAvailableItems, ref noOfAvailableItemsIncludingIntrinsic, 
                        ref availableEntities, ref unavailableEntities);
                }
            }

            StockButton tbItems = (StockButton)itemRow.FindChildById(UIComponent.DataControlID.Stock);
            tbItems.UpdateStockButton(noOfAvailableItems, noOfIncompleteItems, noOfItemsUsedAsParts, noOfItemsOffSite, noOfItemsOwnedByOthers, itemsOfType, false);
        }

        private static void UpdateEntityTypeRow(UIComponent itemRow, EntityType entityType, int amount)
        {
            // update existing row:           
            Label lblTrees = (Label)itemRow.FindChildById(UIComponent.DataControlID.Stock);
            lblTrees.Text = amount.ToString();
        }

        void tbItems_Click(UIComponent sender, EventArgs e)
        {
            StockButton st = sender as StockButton;
         
           
            // open the entity window, populate from area
            The.InGameUI.EntityListWindow.SetDataSource(st.EntityType, st.EntityList); // The.InGameUI.GetExpedition().OwnedEntities.AllEntities[entityType]);

            The.InGameUI.EntityListWindow.OpenNextToStockButton(sender);

        }

        private EntityType entityType;

        private static List<EntityID> GetListOfItems(EntityType entityType)
        {
            MapArea mapArea;
            mapArea = TileSelectionContextMenu.GetMapArea(); // make it a static area instead...

            SharedKnowledge sharedKnowledge = The.InGameUI.UIAllegiance.SharedKnowledge;


            List<EntityID> entityIDs = new List<EntityID>();

            mapArea.GetEntitiesInArea(null, entityIDs,
                e => e.EntityType == entityType, The.InGameUI.UIAllegiance
                // OLD:  e => e.EntityType == entityTypeToShowInEntityWindow 

                    /*&& e.Owner != null
                    && e.Owner.IsOwnedByAllegiance(The.InGameUI.UIAllegiance)*/);

            return entityIDs;

        }

        private void PopulatePersons()
        {
            MapArea mapArea;
            mapArea = TileSelectionContextMenu.GetMapArea();

            entities.Clear();

            mapArea.GetEntitiesInArea(entities, null, e => e.EntityType.Person != null, The.InGameUI.UIAllegiance);

            grdPersons.BeginAddingEntries();

            if (AddPanelIfNotPresent(entities.Count > 0, outerGrid, cpPersons))
            {

                //int noOfPersons = 0;
                foreach (IKnownEntityData e in entities)
                {
                    if (!grdPersons.EntriesByKey.ContainsKey(e.EntityID))
                    {
                        grdPersons.AddHyperLinkEntry(e.EntityID, e.Name, (uint)e.EntityID, hyperLinkMargin);
                    }
                }

                cpPersons.Summary = entities.Count.ToString();
            }

            // remove grid entries not present:
            grdPersons.DeleteEntries<EntityID>(
               e => entities.Exists(e1 => e1.EntityID == e));


            grdPersons.EndAddingEntries();

        }

        private void PopulateRobots()
        {
            MapArea mapArea;
            mapArea = TileSelectionContextMenu.GetMapArea();

            entities.Clear();

            mapArea.GetEntitiesInArea(entities, null, e => e.EntityType.IntelligenceType != null && e.EntityType.StructureType == null && e.EntityType.BiologicalType == null, The.InGameUI.UIAllegiance);

            grdRobots.BeginAddingEntries();

            if (AddPanelIfNotPresent(entities.Count > 0, outerGrid, cpRobots))
            {

                //int noOfPersons = 0;
                foreach (IKnownEntityData e in entities)
                {
                    if (!grdRobots.EntriesByKey.ContainsKey(e.EntityID))
                    {
                        grdRobots.AddHyperLinkEntry(e.EntityID, e.GetDisplayName() /* .Name*/, (uint)e.EntityID, hyperLinkMargin);
                    }
                }

                cpRobots.Summary = entities.Count.ToString();
            }

            // remove grid entries not present:
            grdRobots.DeleteEntries<EntityID>(
               e => entities.Exists(e1 => e1.EntityID == e));


            grdRobots.EndAddingEntries();

        }

        private void AddEntityHyperlink(Entity entity, Grid grid)
        {
            if (entity.PersonEntity != null)
            {
                grid.AddHyperLinkEntry(null, entity.Name, (uint)entity.EntityID, hyperLinkMargin);
                // onboardGrid.AddEntry(entity.PersonEntity.Name, entity.PersonEntity.Name);
            }
            else
            {
                grid.AddHyperLinkEntry(null, entity.EntityType.Name, (uint)entity.EntityID, hyperLinkMargin);
                //onboardGrid.AddEntry(entity.EntityType.Name, entity.EntityType.Name);                 
            }
        }

        /*
        void produce_OnPress(object sender, EventArgs e)
        {
            if (intface.SelectedEntity != null && intface.SelectedEntity.EntityType.StructureType.ProducerType != null) // .IsProductionBuilding) 
            {
                //game.StartProductionJob((Buildings.IProductionBuilding)intf.SelectedBuilding, game.ColonyOwner, game.ColonyOwner);
                UWGame.SimSide.Instance.StartProductionJob(intface.SelectedEntity, UWGame.SimSide.Instance.ColonyOwner, UWGame.SimSide.Instance.ColonyOwner, UWGame.SimSide.Instance.ColonyOwner);
            }
        }*/

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

        /*  private void PopulateStocks()
          {
              PopulateTreePanel(intf.gui, SetSummaryDelegate, cpStock, grdStock, The.Map.TileMap[
                          intface.SelectedEntity.MapPosition.X][
                          intface.SelectedEntity.MapPosition.Y].EntitiesOnTile.FindAll(e => e.Item != null)); 

         
          }*/
    }
}
