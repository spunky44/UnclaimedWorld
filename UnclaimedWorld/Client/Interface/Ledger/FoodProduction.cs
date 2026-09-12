using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWGame.ClientSide.Interface.Controls;
using UWGame.ClientSide.Interface.Inventory;
using UWGame.SimSide;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Expeditions;
using WindowSystem;

namespace UWGame.ClientSide.Interface.Ledger
{
    public class FoodProduction: LedgerSheet
    {
        public override string DisplayName
        {
            get { return "Food production"; }
        }

        public override string Tooltip
        {
            get { return "Shows food produced, consumed and wasted. Listed by food types."; }
        }

        public override bool ShowRangeSelector
        {
            get { return true; }
        }

        const int columnWidth = 90;
        const int nameWidth = 215;

        const int itemTypeIconColumnX = 12;
        const int nameX = 30;
        const int productionX = nameX + nameWidth;
        const int consumedX = productionX + columnWidth;
        int degradedX = productionX + 2 * columnWidth;
        int critterEatenX = productionX + 3 * columnWidth;
        int disappearedX = productionX + 4 * columnWidth;

      //  int topPartHeight = 100;
        int bottomPartHeight = 40;


        string wastedTooltip;
        string nameTooltip;
        string producedTooltip;
        string consumedTooltip;
        string verminTooltip;
        string disappearedTooltip;

        /// <summary>
        /// contains categories like Ingredients and Prepared food
        /// 
        /// NEW: make this scrollable, with a max hegiht. Then the sorting header can stay in place
        /// </summary>
        Grid outerGrid;

        public const int ItemHeight = 26; // 22;

        SortingButtons<FoodProductionSettings.SortColumns> sortingButtons;
      //  UIComponent sortingButtonsContainer;

        List<Grid> categoryGrids = new List<Grid>();

        List<EntityCategory> categoriesToShow;

        UIComponent bottomContainer;
        Label lblTotalProduced, lblTotalConsumed, lblTotalDegraded, lblTotalCritterEaten, lblTotalDisappeared;

        int gridYPos;

        public FoodProduction(GUIManager gui, int width, int height): base(gui, width, height)
        {
            CreateTooltips();
           
            CreateGridHeaderButtons();

            gridYPos = sortingButtons.Bottom + 6; 

            // this should grow in height:
            // LedgerPanel will scroll
            outerGrid = new Grid(gui, ListBoxType.LCD, Label.LabelType.LCDNormal);
            Add(outerGrid);
            outerGrid.FixedItemHeights = false; 
            outerGrid.RenderType = RenderType.CRTAndLCD;
            outerGrid.Font = GUIManager.LCDandHUDBodyFontPath; 
            outerGrid.Width = Width;
            outerGrid.Height = Height - gridYPos - bottomPartHeight;
            outerGrid.Position = new Point(0, gridYPos);
            outerGrid.RowSpacing = 1;
            /*
            outerGrid.ScrollBarEnabled = false;
            outerGrid.CanGrowInHeight = true;
            outerGrid.HeightResize += outerGrid_HeightResize;
            */
            outerGrid.CanGrowInHeight = false;            
            outerGrid.ScrollBarEnabled = true;
             
            outerGrid.SurfaceHeightResize +=outerGrid_HeightResize;

           

            bottomContainer = new UIComponent(gui);
            bottomContainer.Height = bottomPartHeight;
            bottomContainer.Width = Width;
            Add(bottomContainer);
            bottomContainer.Y = outerGrid.Bottom;
            bottomContainer.X = 6;

            Label lblTotalCaption = new Label(gui);
            bottomContainer.Add(lblTotalCaption);
            lblTotalCaption.Init(Label.LabelType.LCDNormal);
            lblTotalCaption.X = nameX;
            lblTotalCaption.Text = "TOTAL:";
            lblTotalCaption.FitToText();
            bottomContainer.CenterChildVertically(lblTotalCaption);

            lblTotalProduced = new Label(gui);
            bottomContainer.Add(lblTotalProduced);
            lblTotalProduced.Init(Label.LabelType.LCDNormal);
            lblTotalProduced.X = productionX;
            bottomContainer.CenterChildVertically(lblTotalProduced);
            
            lblTotalConsumed = new Label(gui);
            bottomContainer.Add(lblTotalConsumed);
            lblTotalConsumed.Init(Label.LabelType.LCDNormal);
            lblTotalConsumed.X = consumedX;
            bottomContainer.CenterChildVertically(lblTotalConsumed);
          
            lblTotalDegraded = new Label(gui);
            bottomContainer.Add(lblTotalDegraded);
            lblTotalDegraded.Init(Label.LabelType.LCDNormal);
            lblTotalDegraded.X = degradedX;
            bottomContainer.CenterChildVertically(lblTotalDegraded);
           
            lblTotalCritterEaten = new Label(gui);
            bottomContainer.Add(lblTotalCritterEaten);
            lblTotalCritterEaten.Init(Label.LabelType.LCDNormal);
            lblTotalCritterEaten.X = critterEatenX;
            bottomContainer.CenterChildVertically(lblTotalCritterEaten);

            lblTotalDisappeared = new Label(gui);
            bottomContainer.Add(lblTotalDisappeared);
            lblTotalDisappeared.Init(Label.LabelType.LCDNormal);
            lblTotalDisappeared.X = disappearedX;
            bottomContainer.CenterChildVertically(lblTotalDisappeared);



            if (GameData.Instance.GUIConstants.FoodProductionCategories != null && GameData.Instance.GUIConstants.FoodProductionCategories.Length > 0)
            {                
                categoriesToShow = GameData.Instance.GUIConstants.FoodProductionCategories.Select(c => GameData.Instance.AllEntityCategories[c]).ToList();

            }
        }


        private void CreateTooltips()
        {
            /* sortingButtons.AddTextButton(nameWidth, "NAME", FoodProductionSettings.SortColumns.Name, "Name of food type");
            sortingButtons.AddTextButton(columnWidth, "PROD.", FoodProductionSettings.SortColumns.Produced, "Produced by the colony");
            sortingButtons.AddTextButton(columnWidth, "CONS.", FoodProductionSettings.SortColumns.Consumed, "Consumed by colony members");
            sortingButtons.AddTextButton(columnWidth, "SPOIL.", FoodProductionSettings.SortColumns.Wasted, "Spoiled/degraded into waste");  //mp was "WASTED"  but i think this word is too broad
            sortingButtons.AddTextButton(columnWidth, "VERMIN", FoodProductionSettings.SortColumns.EatenByCreatures, "Eaten by creatures");
            sortingButtons.AddTextButton(columnWidth, "DISAPP.", FoodProductionSettings.SortColumns.Disappeared, "Disappeared");
*/


            nameTooltip = CreateTooltip("Name", "Name of food type");
            producedTooltip = CreateTooltip("Produced", "Produced by the colony");
            consumedTooltip = CreateTooltip("Consumed", "Consumed by colony members");

            wastedTooltip = CreateTooltip("Spoiled", "Spoiled/degraded into waste");

            verminTooltip = CreateTooltip("Vermin", "Eaten by creatures");
            disappearedTooltip = CreateTooltip("Disappeared", "Disappeared");

        }

        private int GridMaxHeight
        {
            get
            {
                return Height - gridYPos - bottomPartHeight;
            }
        }

        private void ResizeHeight()
        {
            // shrink the Grid if possible, move the totals up below it
            if (outerGrid.surface.Height < GridMaxHeight)
            {
                outerGrid.Height = outerGrid.surface.Height;
            }
            else
            {
                outerGrid.Height = GridMaxHeight;
            }


            bottomContainer.Y = outerGrid.Bottom;
        }


        public void LoadUserSettings(SortingSettings<FoodProductionSettings.SortColumns> /*FoodProductionSettings*/ settings)
        {
            sortingButtons.Fill(
                settings);
        }

       /* public override void LoadUserSettings(FoodProductionSettings settings)
        {
            sortingButtons.Fill(
                settings);           
        }*/

        void outerGrid_HeightResize(UIComponent sender)
        {
            this.ResizeHeight();


           /* bottomContainer.Y = outerGrid.Bottom + 12;

            this.Height = bottomContainer.Bottom;*/
        }

        private void CreateGridHeaderButtons()
        {
            
            sortingButtons = new SortingButtons<FoodProductionSettings.SortColumns>(GUIManager);
            sortingButtons.Width = Width;
            sortingButtons.Height = 30;
            sortingButtons.Position = new Point(0, 0);
            Add(sortingButtons);
            sortingButtons.SortClicked += tbSort_Click;


            sortingButtons.AddTextButton(nameWidth, "NAME", FoodProductionSettings.SortColumns.Name, nameTooltip);
            sortingButtons.AddTextButton(columnWidth, "PROD.", FoodProductionSettings.SortColumns.Produced, producedTooltip);
            sortingButtons.AddTextButton(columnWidth, "CONS.", FoodProductionSettings.SortColumns.Consumed, consumedTooltip);
            sortingButtons.AddTextButton(columnWidth, "SPOIL.", FoodProductionSettings.SortColumns.Wasted, wastedTooltip);  //mp was "WASTED"  but i think this word is too broad
            sortingButtons.AddTextButton(columnWidth, "VERMIN", FoodProductionSettings.SortColumns.EatenByCreatures, verminTooltip);
            sortingButtons.AddTextButton(columnWidth, "DISAPP.", FoodProductionSettings.SortColumns.Disappeared, disappearedTooltip);


        }

        private void tbSort_Click()
        {
            Populate();
        }


        public override void RefreshData()
        {
            Populate();

            base.RefreshData();
        }

       // private double timeInDays = DateAndTime.DaysPerYear;

        private void Populate()
        {

            Expedition expedition = The.Sim.PlaySite.GetFirstPlayerExpedition();
            if (expedition == null)
                return; // clear all? or does it only occur on startup...

            EntityGroup owner = expedition.OwnedEntities;
            
                       
            CollapsablePanel cpCategory;
            Grid categoryGrid = null;
            UIComponent categoryRow;
            UIComponent itemRow;

            categoryGrids = new List<Grid>();

            object key;

          
           

            // get the statistics data
 
            // get all the stats
            ProductionStatistics stats = The.InGameUI.UIAllegiance.Statistics.ProductionStatistics;
            Dictionary<EntityType, List<DataPoint<float>>> producedStats = stats.Stats[ProductionStatistics.StatTypes.Produced];
            Dictionary<EntityType, List<DataPoint<float>>> consumedStats = stats.Stats[ProductionStatistics.StatTypes.ConsumedFood];
            Dictionary<EntityType, List<DataPoint<float>>> degradedStats = stats.Stats[ProductionStatistics.StatTypes.Degraded];
            Dictionary<EntityType, List<DataPoint<float>>> critterEatenStats = stats.Stats[ProductionStatistics.StatTypes.EatenByCreatures];
            Dictionary<EntityType, List<DataPoint<float>>> disappearedStats = stats.Stats[ProductionStatistics.StatTypes.Disappeared];

           /* HashSet<EntityType> typesWithStats = new HashSet<EntityType>();
            Common.AddRangeToSet(ref typesWithStats, producedStats.Keys.ToList());
            Common.AddRangeToSet(ref typesWithStats, consumedStats.Keys.ToList());
            Common.AddRangeToSet(ref typesWithStats, degradedStats.Keys.ToList());
            Common.AddRangeToSet(ref typesWithStats, critterEatenStats.Keys.ToList());
            */

            // summarize:
            DateAndTime.TimeDateYear toTime = The.Sim.DateAndTime.CurrentTimeDateYear; 

            DateAndTime.TimeDateYear fromTime = FromDate; // toTime;
          
            int producedTotal = 0;
            int consumedTotal = 0;
            int degradedTotal = 0;
            int critterEatenTotal = 0;
            int disappearedTotal = 0;

            // Nested grids

            // outer level is an item "category"
            // add nested grids for each, containing item types...
                                    

            outerGrid.BeginAddingEntries();

            if (categoriesToShow != null)
            {
                
                foreach (var entityType in stats.TypesWithStats)
                {
                    if (entityType.ItemType != null && entityType.ItemType.FoodType != null // include ingredients and waste as well.
                        && categoriesToShow.Contains(entityType.Category)
                        /*
                        && expedition.IsEatableByIndependentMembers(entityType)*/)
                    {
                        key = entityType.Category;

                        int availableAmount = owner.CountAvailableItems(entityType);

                        int produced = Statistic.SumDataPoints(producedStats, entityType, fromTime, toTime);
                        int consumed = Statistic.SumDataPoints(consumedStats, entityType, fromTime, toTime);
                        int degraded = Statistic.SumDataPoints(degradedStats, entityType, fromTime, toTime);
                        int critterEaten = Statistic.SumDataPoints(critterEatenStats, entityType, fromTime, toTime);
                        int disappeared = Statistic.SumDataPoints(disappearedStats, entityType, fromTime, toTime);

                        producedTotal += produced;
                        consumedTotal += consumed;
                        degradedTotal += degraded;
                        critterEatenTotal += critterEaten;
                        disappearedTotal += disappeared;

                        cpCategory = null;

                        if (outerGrid.TryGetEntry(key, out categoryRow))
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
                            itemRow = AddItemRow(categoryGrid, entityType, owner, false);
                        }

                        UpdateItemRow(itemRow, entityType, availableAmount, produced, consumed, degraded, critterEaten, disappeared);
                    }
                }

            } 

            // remove unused items   
            foreach (CollapsablePanel colPanel in outerGrid.Entries)
            {
                Grid grd = (Grid)colPanel.ExpandedPanel.Controls[0];
                grd.DeleteEntries<EntityType>(j => stats.TypesWithStats.Contains(j));
            }

            //remove empty categories
            List<object> keysToRemove = new List<object>();
            foreach (var keys in outerGrid.EntriesByKey.Keys)
            {
                Grid grd = (Grid)(outerGrid.EntriesByKey[keys] as CollapsablePanel).ExpandedPanel.Controls[0];

                if (grd.Entries.Count == 0)
                {
                    keysToRemove.Add(keys);
                }
            }
            foreach (var deleteKey in keysToRemove)
            {
                outerGrid.RemoveEntry(deleteKey);
            }

            //sort categories
            InventoryPanel.DoCategorySorting(outerGrid, categoryGrids, The.InGameUI.FoodProductionSettings.SortingSettings.SortOrder);

            outerGrid.EndAddingEntries();

            UpdateTotals(producedTotal, consumedTotal, degradedTotal, critterEatenTotal, disappearedTotal);

            ResizeHeight();
        }


        private void UpdateTotals(int producedTotal, int consumedTotal, int degradedTotal, int critterEatenTotal, int disappearedTotal)
        {
            SetLabelPositiveColorAndValue(lblTotalProduced, producedTotal);
            lblTotalConsumed.Text = consumedTotal.ToString();
            SetLabelNegativeColorAndValue(lblTotalDegraded, degradedTotal);
            SetLabelNegativeColorAndValue(lblTotalCritterEaten, critterEatenTotal);
            SetLabelNegativeColorAndValue(lblTotalDisappeared, disappearedTotal);

        }

        private void SetLabelNegativeColorAndValue(Label lbl, int value)
        {
            lbl.Text = value.ToString();
            if (value > 0)
            {             
                lbl.NormalColor = GameData.Instance.GUIConstants.NegativeColor;
            }
        }

        private void SetLabelPositiveColorAndValue(Label lbl, int value)
        {
            lbl.Text = value.ToString();
            if (value > 0)
            {             
                lbl.NormalColor = GameData.Instance.GUIConstants.PositiveColor;
            }
        }

       

        private void UpdateItemRow(UIComponent itemRow, EntityType entityType, int noOfAvailableItems, int produced, int consumed, int degraded, int critterEaten, int disappeared)
        {
                       
            // update production status:
            DataTypeButton tbCaption = (DataTypeButton)itemRow.FindChildById(UIComponent.DataControlID.Caption);
            tbCaption.SetAvailableStatusColor(noOfAvailableItems > 0);

            Label lblProduced;
            itemRow.FindChildById(DataControlID.Produced, out lblProduced);
            SetLabelPositiveColorAndValue(lblProduced, produced);
            lblProduced.FitToText();
            lblProduced.ToolTip = producedTooltip;

            Label lblConsumed;
            itemRow.FindChildById(DataControlID.Consumed, out lblConsumed);
            lblConsumed.Text = consumed.ToString();
            lblConsumed.FitToText();
            lblConsumed.ToolTip = consumedTooltip;

            Label lblDegraded;
            itemRow.FindChildById(DataControlID.Degraded, out lblDegraded);
            SetLabelNegativeColorAndValue(lblDegraded, degraded);
            lblDegraded.FitToText();
            lblDegraded.ToolTip = wastedTooltip;

            Label lblCritterEaten;
            itemRow.FindChildById(DataControlID.CritterEaten, out lblCritterEaten);
            SetLabelNegativeColorAndValue(lblCritterEaten, critterEaten);
            lblCritterEaten.FitToText();
            lblCritterEaten.ToolTip = verminTooltip;

            Label lblDisappeared;
            itemRow.FindChildById(DataControlID.Disappeared, out lblDisappeared);
            SetLabelNegativeColorAndValue(lblDisappeared, disappeared);
            lblDisappeared.FitToText();
            lblDisappeared.ToolTip = disappearedTooltip;

            object orderBy = null;
            switch(The.InGameUI.FoodProductionSettings.SortingSettings.SortedBy)
            {
                case FoodProductionSettings.SortColumns.Name:
                    orderBy = entityType.Name;
                    break;
                case FoodProductionSettings.SortColumns.Consumed:
                    orderBy = consumed;
                    break;
                case FoodProductionSettings.SortColumns.Produced:
                    orderBy = produced;
                    break;
                case FoodProductionSettings.SortColumns.Disappeared:
                    orderBy = disappeared;
                    break;
                case FoodProductionSettings.SortColumns.Wasted:
                    orderBy = degraded;
                    break;
                case FoodProductionSettings.SortColumns.EatenByCreatures:
                    orderBy = critterEaten;
                    break;

            }

            itemRow.OrderByTag1 = orderBy;

        }

        private UIComponent AddItemRow(Grid grid, EntityType entityType, EntityGroup owner, bool useCurrentUIOwner)
        {
            GUIManager gui = base.GUIManager;

            //  grid.MouseMove += InventoryPanel_MouseOver;

            Image icon;
            UIComponent item;

            item = new UIComponent(gui);
            item.DebugTag = "stocksItem";

            grid.AddEntry(entityType, item);
          //  item.CanHaveFocus = true;                    

            icon = InventoryPanel.AddEntityTypeIcon(entityType, item, itemTypeIconColumnX);
            
            DataTypeButton dtCaption = new DataTypeButton(gui, HUD_Windows.DataSheet.InfoToShow.Production, entityType,
                GoalEvaluator.GetOwnerID(owner),
                useCurrentUIOwner);
            dtCaption.Init(TextButton.TextButtonType.LCDToolTipBlack);
            dtCaption.ID = UIComponent.DataControlID.Caption;
            item.Add(dtCaption);
            dtCaption.IsRoot = true;
            // dtCaption.Text = entityType.PluralName;
            dtCaption.TextAlignment = TextButton.TextAlign.Left;
            dtCaption.Width = 175; 
            dtCaption.X = nameX;
            item.CenterChildVertically(dtCaption);

            Label lblProduced = new Label(gui);
            item.Add(lblProduced);
            lblProduced.Init(Label.LabelType.LCDNormal);
            lblProduced.X = productionX;
            lblProduced.ID = DataControlID.Produced;
            item.CenterChildVertically(lblProduced);

            Label lblConsumed = new Label(gui);
            item.Add(lblConsumed);
            lblConsumed.Init(Label.LabelType.LCDNormal);
            lblConsumed.X = consumedX;
            lblConsumed.ID = DataControlID.Consumed;
            item.CenterChildVertically(lblConsumed);

            Label lblDegraded = new Label(gui);
            item.Add(lblDegraded);
            lblDegraded.Init(Label.LabelType.LCDNormal);
            lblDegraded.X = degradedX;
            lblDegraded.ID = DataControlID.Degraded;
            item.CenterChildVertically(lblDegraded);

            Label lblCritterEaten = new Label(gui);
            item.Add(lblCritterEaten);
            lblCritterEaten.Init(Label.LabelType.LCDNormal);
            lblCritterEaten.X = critterEatenX;
            lblCritterEaten.ID = DataControlID.CritterEaten;
            item.CenterChildVertically(lblCritterEaten);

            Label lblDisappeared = new Label(gui);
            item.Add(lblDisappeared);
            lblDisappeared.Init(Label.LabelType.LCDNormal);
            lblDisappeared.X = disappearedX;
            lblDisappeared.ID = DataControlID.Disappeared;
            item.CenterChildVertically(lblDisappeared);


            return item;
        }

        private void AddCategoryRow(ref CollapsablePanel cpCategory, ref Grid categoryGrid, object key)
        {
            GUIManager gui = this.GUIManager;

            EntityCategory entityCategory = key as EntityCategory;
         
            cpCategory = new CollapsablePanel(gui, CollapsablePanel.PanelType.DropDownBig);
            cpCategory.HeadingYPos = 4;
            cpCategory.CollapsedHeight = 28;// grdCategoryView.ItemHeight;

            outerGrid.AddEntry(key, cpCategory);
            cpCategory.OrderByTag1 = entityCategory.Name;

            cpCategory.Init();
            cpCategory.Title = entityCategory.Name;
            cpCategory.Width = outerGrid.Width;
                                   

            categoryGrid = new Grid(gui, ListBoxType.LCD, Label.LabelType.LCDNormal);
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

    }
}
