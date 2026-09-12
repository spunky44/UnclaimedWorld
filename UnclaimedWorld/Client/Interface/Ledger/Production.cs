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
using UWGame.SimSide.Processes;
using WindowSystem;

namespace UWGame.ClientSide.Interface.Ledger
{
    public class Production: LedgerSheet
    {
        public override string DisplayName
        {
            get { return "Production"; }
        }

        public override string Tooltip
        {
            get { return "Shows production details including productivity."; }
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
        int disappearedX = productionX + 3 * columnWidth;
        int productivityX = productionX + 4 * columnWidth;

      //  int topPartHeight = 100;
        int bottomPartHeight = 40;


        string wastedTooltip;
        string nameTooltip;
        string producedTooltip;
        string usedTooltip;
        string disappearedTooltip;
        string productivityTooltip;

        /// <summary>
        /// contains categories like Ingredients and Prepared food
        /// 
        /// NEW: make this scrollable, with a max hegiht. Then the sorting header can stay in place
        /// </summary>
        Grid outerGrid;

        public const int ItemHeight = 26; // 22;

        SortingButtons<ProductionSettings.SortColumns> sortingButtons;
      //  UIComponent sortingButtonsContainer;

        List<Grid> categoryGrids = new List<Grid>();

        List<EntityCategory> categoriesToShow;

        UIComponent bottomContainer;
        Label lblTotalProduced, lblTotalConsumed, lblTotalDegraded, lblTotalCritterEaten, lblTotalDisappeared;

        int gridYPos;

        public Production(GUIManager gui, int width, int height): base(gui, width, height)
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
           
            lblTotalDisappeared = new Label(gui);
            bottomContainer.Add(lblTotalDisappeared);
            lblTotalDisappeared.Init(Label.LabelType.LCDNormal);
            lblTotalDisappeared.X = disappearedX;
            bottomContainer.CenterChildVertically(lblTotalDisappeared);



            if (GameData.Instance.GUIConstants.ProductionCategories != null && GameData.Instance.GUIConstants.ProductionCategories.Length > 0)
            {                
                categoriesToShow = GameData.Instance.GUIConstants.ProductionCategories.Select(c => GameData.Instance.AllEntityCategories[c]).ToList();

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


            nameTooltip = CreateTooltip("Name", "Name of product");
            producedTooltip = CreateTooltip("Produced", "Produced by the colony");
            usedTooltip = CreateTooltip("Used", "Used in production");
            wastedTooltip = CreateTooltip("Spoiled", "Degraded into waste");
            disappearedTooltip = CreateTooltip("Disappeared", "Disappeared");
            productivityTooltip = CreateTooltip("Productivity", "Mean productivity");

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


        public void LoadUserSettings(SortingSettings<ProductionSettings.SortColumns> settings)
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
            sortingButtons = new SortingButtons<ProductionSettings.SortColumns>(GUIManager);
            sortingButtons.Width = Width;
            sortingButtons.Height = 30;
            sortingButtons.Position = new Point(0, 0);
            Add(sortingButtons);
            sortingButtons.SortClicked += tbSort_Click;

            sortingButtons.AddTextButton(nameWidth, "NAME", ProductionSettings.SortColumns.Name, nameTooltip);
            sortingButtons.AddTextButton(columnWidth, "PROD.", ProductionSettings.SortColumns.Produced, producedTooltip);
            sortingButtons.AddTextButton(columnWidth, "USED", ProductionSettings.SortColumns.UsedInProduction, usedTooltip);
            sortingButtons.AddTextButton(columnWidth, "DEGR.", ProductionSettings.SortColumns.Wasted, wastedTooltip);         
            sortingButtons.AddTextButton(columnWidth, "DISAPP.", ProductionSettings.SortColumns.Disappeared, disappearedTooltip);
            sortingButtons.AddTextButton(columnWidth, "PRDCTIV.", ProductionSettings.SortColumns.Productivity, productivityTooltip);

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
            Dictionary<EntityType, List<DataPoint<float>>> usedAsInputStats = stats.Stats[ProductionStatistics.StatTypes.UsedAsInput];
            Dictionary<EntityType, List<DataPoint<float>>> degradedStats = stats.Stats[ProductionStatistics.StatTypes.Degraded];
            Dictionary<EntityType, List<DataPoint<float>>> disappearedStats = stats.Stats[ProductionStatistics.StatTypes.Disappeared];
            Dictionary<EntityType, List<DataPoint<Productivity>>> productivityStats = stats.ProductivityStats;


            // summarize:
            DateAndTime.TimeDateYear toTime = The.Sim.DateAndTime.CurrentTimeDateYear; 

            DateAndTime.TimeDateYear fromTime = FromDate; // toTime;
          
            int producedTotal = 0;
            int usedTotal = 0;
            int degradedTotal = 0;           
            int disappearedTotal = 0;

            // Nested grids

            // outer level is an item "category"
            // add nested grids for each, containing item types...
                                    

            outerGrid.BeginAddingEntries();

            if (categoriesToShow != null)
            {
                
                foreach (var entityType in stats.TypesWithStats)
                {
                    if (//entityType.ItemType != null && entityType.ItemType.FoodType != null // include ingredients and waste as well.
                        //&& 
                        categoriesToShow.Contains(entityType.Category))
                    {
                        key = entityType.Category;

                        int availableAmount = owner.CountAvailableItems(entityType);

                        int produced = Statistic.SumDataPoints(producedStats, entityType, fromTime, toTime);
                        int used = Statistic.SumDataPoints(usedAsInputStats, entityType, fromTime, toTime);
                        int degraded = Statistic.SumDataPoints(degradedStats, entityType, fromTime, toTime);                      
                        int disappeared = Statistic.SumDataPoints(disappearedStats, entityType, fromTime, toTime);

                        float? productivity, skillProd, toolProd, energyProd;
                        ProductionStatistics.GetMeanOfDataPoints(productivityStats, entityType, fromTime, toTime, out productivity, out toolProd, out skillProd, out energyProd);


                        producedTotal += produced;
                        usedTotal += used;
                        degradedTotal += degraded;                      
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

                        UpdateItemRow(itemRow, entityType, availableAmount, produced, used, degraded, disappeared, productivity, toolProd, skillProd, energyProd);
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
            InventoryPanel.DoCategorySorting(outerGrid, categoryGrids, The.InGameUI.ProductionSettings.SortingSettings.SortOrder);

            outerGrid.EndAddingEntries();

            UpdateTotals(producedTotal, usedTotal, degradedTotal, disappearedTotal);

            ResizeHeight();
        }


        private void UpdateTotals(int producedTotal, int consumedTotal, int degradedTotal, int disappearedTotal)
        {
            SetLabelPositiveColorAndValue(lblTotalProduced, producedTotal);
            lblTotalConsumed.Text = consumedTotal.ToString();
            SetLabelNegativeColorAndValue(lblTotalDegraded, degradedTotal);      
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

       

        private void UpdateItemRow(UIComponent itemRow, EntityType entityType, int noOfAvailableItems, int produced, int used, int degraded, int disappeared, 
            float? productivity, float? toolProd, float? skillProd, float? energyProd)
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
            lblConsumed.Text = used.ToString();
            lblConsumed.FitToText();
            lblConsumed.ToolTip = usedTooltip;

            Label lblDegraded;
            itemRow.FindChildById(DataControlID.Degraded, out lblDegraded);
            SetLabelNegativeColorAndValue(lblDegraded, degraded);
            lblDegraded.FitToText();
            lblDegraded.ToolTip = wastedTooltip;

           
            Label lblDisappeared;
            itemRow.FindChildById(DataControlID.Disappeared, out lblDisappeared);
            SetLabelNegativeColorAndValue(lblDisappeared, disappeared);
            lblDisappeared.FitToText();
            lblDisappeared.ToolTip = disappearedTooltip;

            Label lblProductivity;
            itemRow.FindChildById(DataControlID.Productivity, out lblProductivity);
            if (productivity.HasValue)
            {
                lblProductivity.Visible = true;
                lblProductivity.Text = productivity.Value.ToString("N2");
                lblProductivity.FitToText();
                lblProductivity.ToolTip = productivityTooltip;

                StringBuilder prodTooltip = new StringBuilder();
                Common.AppendHeaderOnLightBG(prodTooltip, "Productivity");
                Common.Append(prodTooltip, "Mean (average) productivity in the selected timespan.");
                Common.AppendDividerOnOwnLine(prodTooltip);

                Common.Append(prodTooltip, "Tools: ");
                Common.AppendFormat(prodTooltip, "{0:N2}", true, toolProd);
                Common.AppendLine(prodTooltip);

                Common.Append(prodTooltip, "Skill: ");
                Common.AppendFormat(prodTooltip, "{0:N2}", true, skillProd);
                Common.AppendLine(prodTooltip);

                Common.Append(prodTooltip, "Worker energy: ");
                Common.AppendFormat(prodTooltip, "{0:N2}", true, energyProd);
                //   Common.AppendLine(prodTooltip);

                Common.AppendDividerOnOwnLine(prodTooltip);

                Common.Append(prodTooltip, "Total productivity: ");
                Common.AppendFormat(prodTooltip, "{0:N2}", true, productivity);
                //    Common.AppendLine(prodTooltip);
                lblProductivity.ToolTip = prodTooltip.ToString();

            }
            else
            {
                lblProductivity.Visible = false;
            }

            object orderBy = null;
            switch(The.InGameUI.ProductionSettings.SortingSettings.SortedBy)
            {
                case ProductionSettings.SortColumns.Name:
                    orderBy = entityType.Name;
                    break;
                case ProductionSettings.SortColumns.Consumed:
                    orderBy = used;
                    break;
                case ProductionSettings.SortColumns.Produced:
                    orderBy = produced;
                    break;
                case ProductionSettings.SortColumns.Disappeared:
                    orderBy = disappeared;
                    break;
                case ProductionSettings.SortColumns.Wasted:
                    orderBy = degraded;
                    break;
                case ProductionSettings.SortColumns.Productivity:
                    orderBy = productivity;
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
                   
            Label lblDisappeared = new Label(gui);
            item.Add(lblDisappeared);
            lblDisappeared.Init(Label.LabelType.LCDNormal);
            lblDisappeared.X = disappearedX;
            lblDisappeared.ID = DataControlID.Disappeared;
            item.CenterChildVertically(lblDisappeared);

            Label lblProductivity = new Label(gui);
            item.Add(lblProductivity);
            lblProductivity.Init(Label.LabelType.LCDNormal);
            lblProductivity.X = productivityX;
            lblProductivity.ID = DataControlID.Productivity;
            item.CenterChildVertically(lblProductivity);
            lblProductivity.TooltipExpires = false;
            lblProductivity.TooltipWidth = 260;

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
