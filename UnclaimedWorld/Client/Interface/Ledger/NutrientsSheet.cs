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
using UWGame.SimSide.AI.Needs;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Items;
using WindowSystem;

namespace UWGame.ClientSide.Interface.Ledger
{
    /// <summary>
    /// does not include the dog or other animals
    /// </summary>
    public class NutrientsSheet: LedgerSheet
    {
        public override string DisplayName
        {
            get { return "Nutrients"; }
        }

        public override string Tooltip
        {
            get { return "Shows food nutrients produced and consumed."; }
        }

        public override bool ShowRangeSelector
        {
            get { return true; }
        }

        const int columnWidth = 90;
        const int nameWidth = 150;
               
      //  const int nameX = 30;
        const int productionX = nameWidth;
        const int consumedX = productionX + columnWidth;
        const int overconsumedX = productionX + 2 * columnWidth;

        const int storedX = overconsumedX + (int)(1.5f * columnWidth);
        const int daysLeftX = storedX + (int)(columnWidth);


      //  int topPartHeight = 100;
        int bottomPartHeight = 40;



        string overconsumedTooltip;
        string nameTooltip;
        string producedTooltip;
        string consumedTooltip;
        string storedTooltip;
        string daysLeftTooltip;
      
        Grid outerGrid;

        public const int ItemHeight = 26; // 22;

        SortingButtons<NutrientSheetSettings.SortColumns> sortingButtons;

        int gridYPos;

       /* UIComponent bottomContainer;
        Label lblTotalProduced, lblTotalConsumed, lblTotalDegraded, lblTotalCritterEaten, lblTotalDisappeared;
        */

        public NutrientsSheet(GUIManager gui, int width, int height)
            : base(gui, width, height)
        {
            CreateTooltips();

            CreateGridHeaderButtons();

            gridYPos = sortingButtons.Bottom + 6; 

            // this should not grow in height:
            outerGrid = new Grid(gui, ListBoxType.LCD, Label.LabelType.LCDNormal);
            Add(outerGrid);          
            outerGrid.RenderType = RenderType.CRTAndLCD;
            outerGrid.Font = GUIManager.LCDandHUDBodyFontPath; 
            outerGrid.Width = Width;
            outerGrid.Height = GridMaxHeight;
            outerGrid.Position = new Point(0, gridYPos);
            outerGrid.RowSpacing = 1;
            outerGrid.FixedItemHeights = true;
            outerGrid.ItemHeight = ItemHeight;
            /*
            outerGrid.ScrollBarEnabled = false;
            outerGrid.CanGrowInHeight = true;
            outerGrid.HeightResize += outerGrid_HeightResize;
            outerGrid.FixedItemHeights = false;
            */
            outerGrid.CanGrowInHeight = false;            
            outerGrid.ScrollBarEnabled = true;            

                                          
        }

        private void CreateTooltips()
        {
           
            nameTooltip = CreateTooltip("Name", "Name of food nutrient type");           
            overconsumedTooltip = CreateTooltip("Overconsumed", "These nutrients were consumed but were not needed. To reduce this waste, make sure there is food with different nutrient profiles available.");
            consumedTooltip = CreateTooltip("Consumed", "Consumed by colony members");
            producedTooltip = CreateTooltip("Produced", "Produced by the colony");
            storedTooltip = CreateTooltip("Stored", "Currently stored");
            daysLeftTooltip = CreateTooltip("Days left", "How many days the current store will last");

        }

       

        private int GridMaxHeight
        {
            get
            {
                return Height - gridYPos - bottomPartHeight;
            }
        }

      /*  private void ResizeHeight()
        {
            // shrink the Grid if possible, move the totals up below it
            if (outerGrid.surface.Height < GridMaxHeight)
            {
                outerGrid.Height = outerGrid.surface.Height;
                
            }

        }*/


        public void LoadUserSettings(SortingSettings<NutrientSheetSettings.SortColumns> settings)
        {
            sortingButtons.Fill(
                settings);
        }

       /* public override void LoadUserSettings(FoodProductionSettings settings)
        {
            sortingButtons.Fill(
                settings);           
        }*/


        private void CreateGridHeaderButtons()
        {
         /*   sortingButtonsContainer = new UIComponent(GUIManager);
            Add(sortingButtonsContainer);
            sortingButtonsContainer.Width = Width;
            sortingButtonsContainer.Height = 50;
            sortingButtonsContainer.Position = new Point(0, 160);
            */

            sortingButtons = new SortingButtons<NutrientSheetSettings.SortColumns>(GUIManager);
            sortingButtons.Width = Width;
            sortingButtons.Height = 30;
            sortingButtons.Position = new Point(0, 0);
            Add(sortingButtons);
            sortingButtons.SortClicked += tbSort_Click;

            sortingButtons.AddTextButton(nameWidth, "NAME", NutrientSheetSettings.SortColumns.Name, nameTooltip); // "Name of food nutrient type");
            sortingButtons.AddTextButton(columnWidth, "PROD.", NutrientSheetSettings.SortColumns.Produced, producedTooltip);
            sortingButtons.AddTextButton(columnWidth, "CONS.", NutrientSheetSettings.SortColumns.Consumed, consumedTooltip);
                      
            sortingButtons.AddTextButton(columnWidth, "OVER.", NutrientSheetSettings.SortColumns.Overconsumed, overconsumedTooltip); //

            sortingButtons.CreateTextButton(storedX, columnWidth, "STORED", NutrientSheetSettings.SortColumns.Stored, storedTooltip);
            sortingButtons.CreateTextButton(daysLeftX, columnWidth, "DAYS", NutrientSheetSettings.SortColumns.DaysLeft, daysLeftTooltip);
         
           /* sortingButtons.AddTextButton(columnWidth, "STORED", NutrientSheetSettings.SortColumns.Stored, "Currently stored");
            sortingButtons.AddTextButton(columnWidth, "DAYS", NutrientSheetSettings.SortColumns.DaysLeft, "How many days the current store will last");
         */

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

            Expedition expedition = The.InGameUI.GetExpedition(); // The.Sim.PlaySite.GetFirstPlayerExpedition();
            if (expedition == null)
                return; // clear all? or does it only occur on startup...

                   
            UIComponent itemRow;
           
            // get the statistics data
 
            // get all the stats
            EntityGroup owner = expedition.OwnedEntities;
          
            NutrientStatistics stats = The.InGameUI.UIAllegiance.Statistics.NutrientStatistics;
            Dictionary<FoodNutrientType, List<DataPoint<float>>> producedStats = stats.Stats[NutrientStatistics.StatTypes.Produced];
            Dictionary<FoodNutrientType, List<DataPoint<float>>> consumedStats = stats.Stats[NutrientStatistics.StatTypes.Consumed];
            Dictionary<FoodNutrientType, List<DataPoint<float>>> overConsumedStats = stats.Stats[NutrientStatistics.StatTypes.Overconsumed];

            Dictionary<FoodNutrientType, float> stored = GetNutrients(expedition);
            Dictionary<FoodNutrientType, float> daysLeft = GetNutrientsDaysLeft(expedition, stored);
                       
            var neededNutrients = GetMemberNutrients(expedition);

           
            DateAndTime.TimeDateYear toTime = The.Sim.DateAndTime.CurrentTimeDateYear;
            DateAndTime.TimeDateYear fromTime = FromDate; 

            outerGrid.BeginAddingEntries();

            foreach (var nutrientType in neededNutrients)
            {
                // summarize:
                float produced = SumDataPoints(producedStats, nutrientType, fromTime, toTime);
                float consumed = SumDataPoints(consumedStats, nutrientType, fromTime, toTime);
                float overConsumed = SumDataPoints(overConsumedStats, nutrientType, fromTime, toTime);

                
                if (!outerGrid.TryGetEntry(nutrientType, out itemRow))
                {
                    itemRow = AddItemRow(nutrientType);
                }

                UpdateItemRow(itemRow, nutrientType, 
                    produced, consumed, overConsumed, stored[nutrientType], daysLeft[nutrientType]);

            }
                   

            // remove unused items  
            outerGrid.DeleteEntries<FoodNutrientType>(j => neededNutrients.Contains(j));
           
            outerGrid.Sort(u => u.OrderByTag1, The.InGameUI.NutrientSheetSettings.SortingSettings.SortOrder);

            outerGrid.EndAddingEntries();

        }

         public Dictionary<FoodNutrientType, float> GetNutrients(Expedition expedition)
        {
            Dictionary<FoodNutrientType, float> totalBulk = new Dictionary<FoodNutrientType, float>();                       

            foreach (var item in GameData.Instance.AllFoodNutrientTypes)
	        {
		        totalBulk.Add(item.Value, 0f);
	        }

            OwnerID? ownerID = expedition.OwnedEntities.GetOwnerID();

            List<Entity> foodItems = null;
            foreach (var item in expedition.OwnedEntities.Food) //  food)
            {
                if (expedition.IsEatableByIndependentMembers(item.Key))
                {
                    List<EntityID> availableEntities = null;
                    List<EntityID> unavailableEntities = null;

                    foreach (var entityID in item.Value)
                    {
                        Entity entity = Entity.FindByID(entityID);
                        if (entity != null)
                        {
                            int noOfIncompleteItems = 0, noOfAvailableItems = 0, noOfAvailableItemsIncludingIntrinsic = 0, noOfItemsUsedAsParts = 0, noOfItemsOffSite = 0, noOfItemsOwnedByOthers = 0;

                            EntityGroup.CountEntity(ownerID, entity, ref noOfIncompleteItems, ref noOfItemsUsedAsParts, ref noOfItemsOffSite, ref noOfItemsOwnedByOthers, 
                                ref noOfAvailableItems, ref noOfAvailableItemsIncludingIntrinsic,
                                ref availableEntities, ref unavailableEntities);


                            if (noOfAvailableItems > 0)
                            {
                                Common.AddToList(ref foodItems, entity);

                            }
                        }
                    }
                }
            }

            if (foodItems != null)
            {
                List<FoodNutrientType> keys = totalBulk.Keys.ToList();
                foreach (var item in keys)
	            {	 
                    float bulk = 0;
                    foreach (var entity in foodItems)
	                {                    
                        float thisBulk;
                        if (entity.Item.Food.NutrientBulkAmounts.TryGetValue(item, out thisBulk))
                        {
                            bulk += thisBulk;
                        }
	                }

                    totalBulk[item] = bulk;
                }           
            }

            return totalBulk;
            
        }

        


        private HashSet<FoodNutrientType> GetMemberNutrients(Expedition expedition)
        {
            HashSet<FoodNutrientType> nutrients = new HashSet<FoodNutrientType>();
            expedition.IterateMembers(e =>
                {
                    if (e.BiologicalEntity != null && ProductionStatistics.CountMemberConsumption(e))
                    {
                        foreach (var need in e.BiologicalEntity.Needs.NeedsList)
                        {
                            if (need.Value.FoodNeed != null)
                            {
                                nutrients.Add(need.Value.NeedType.FoodNeedType.FoodNutrientType);
                            }
                        }
                    }
                });

            return nutrients;

        }

        public Dictionary<FoodNutrientType, float> GetNutrientsDaysLeft(Expedition expedition, Dictionary<FoodNutrientType, float> stored)
        {
            Dictionary<FoodNutrientType, float> daysLeft = new Dictionary<FoodNutrientType, float>();

            foreach (var item in stored)
	        {
                float totalDailyNeed = 0f;

                expedition.IterateMembers(e => 
                    {
                        if (e.BiologicalEntity != null && ProductionStatistics.CountMemberConsumption(e)) // don't count the dog
                        {
                            Need need;
                            if (e.BiologicalEntity.Needs.NeedsList.TryGetValue(item.Key.KeyName, out need)
                                && need.FoodNeed != null)
                            {
                                totalDailyNeed += (need.FoodNeed.TotalNeededNutrientBulk * need.DecreasePerDay);
                            }
                        }
                    });

                float thisDaysLeft = item.Value / totalDailyNeed;

                daysLeft[item.Key] = thisDaysLeft;		 
	        }


            return daysLeft;
        }

        private void SetLabelNegativeColorAndValue(Label lbl, float value, int xPosRight) //, int decimals)
        {
            SetLabelValue(lbl, value, xPosRight); //, decimals);
            if (lbl.Text != "0") // value > 0)
            {             
                lbl.NormalColor = GameData.Instance.GUIConstants.NegativeColor;
            }
        }

        private static void SetLabelValue(Label lbl, float value, int xPosRight) //, int decimals)
        {
            lbl.Text = Common.DecimalToStringSignificant(value, 0.0001f, "0.0001", 0.000001f);

            //lbl.Text = Common.DecimalToString(value, decimals); // value.ToString();
            lbl.FitToText();
            lbl.AlignRight(xPosRight);

        }

        private void SetLabelPositiveColorAndValue(Label lbl, float value, int xPosRight) //, int decimals)
        {
            SetLabelValue(lbl, value, xPosRight); //, decimals);
            if (lbl.Text != "0") //if (value > 0)
            {             
                lbl.NormalColor = GameData.Instance.GUIConstants.PositiveColor;
            }
        }

        private float SumDataPoints(Dictionary<FoodNutrientType, List<DataPoint<float>>> dict, FoodNutrientType nutrientType, DateAndTime.TimeDateYear from, DateAndTime.TimeDateYear to)
        {
            List<DataPoint<float>> data;

            if (dict.TryGetValue(nutrientType, out data))
            {              
                List<DataPoint<float>> list = Statistic.GetDataPointsBetween(data, from, to);

                float sum = list.Sum(d => d.Value);

                return sum;
            }

            return 0;
        }

        private void UpdateItemRow(UIComponent itemRow, FoodNutrientType nutrientType, float produced, float consumed, float overConsumed, float stored, float daysLeft)
        {
           // int decimals = nutrientType.DecimalsToShowBulk;

            int rightPadding = 10;

            Label lblProduced;
            itemRow.FindChildById(DataControlID.Produced, out lblProduced);
            SetLabelPositiveColorAndValue(lblProduced, produced, productionX + columnWidth - rightPadding); //, decimals);
            lblProduced.ToolTip = producedTooltip;

            Label lblConsumed;
            itemRow.FindChildById(DataControlID.Consumed, out lblConsumed);
            SetLabelValue(lblConsumed, consumed, consumedX + columnWidth - rightPadding); //, decimals); //);
            lblConsumed.ToolTip = consumedTooltip;

            Label lblOverconsumed;
            itemRow.FindChildById(DataControlID.Overconsumed, out lblOverconsumed);
            SetLabelNegativeColorAndValue(lblOverconsumed, overConsumed, overconsumedX + columnWidth - rightPadding); //, decimals);
            lblOverconsumed.ToolTip = overconsumedTooltip;

            Label lblStored;
            itemRow.FindChildById(DataControlID.Stored, out lblStored);
            SetLabelPositiveColorAndValue(lblStored, stored, storedX + columnWidth - rightPadding); //, decimals);
            lblStored.ToolTip = storedTooltip;

            Label lblDaysLeft;
            itemRow.FindChildById(DataControlID.DaysLeft, out lblDaysLeft);
            SetLabelPositiveColorAndValue(lblDaysLeft, daysLeft, daysLeftX + columnWidth - rightPadding); //, decimals);
            lblDaysLeft.ToolTip = daysLeftTooltip;
            
            object orderBy = null;
            switch(The.InGameUI.NutrientSheetSettings.SortingSettings.SortedBy)
            {
                case NutrientSheetSettings.SortColumns.Name:
                    orderBy = nutrientType.Name;
                    break;
                case NutrientSheetSettings.SortColumns.Consumed:
                    orderBy = consumed;
                    break;
                case NutrientSheetSettings.SortColumns.Produced:
                    orderBy = produced;
                    break;
                case NutrientSheetSettings.SortColumns.Overconsumed:
                    orderBy = overConsumed;
                    break;
                case NutrientSheetSettings.SortColumns.Stored:
                    orderBy = stored;
                    break;
                case NutrientSheetSettings.SortColumns.DaysLeft:
                    orderBy = daysLeft;
                    break; 
            }

            itemRow.OrderByTag1 = orderBy;


        }

        private UIComponent AddItemRow(FoodNutrientType nutrientType)
        {
            GUIManager gui = base.GUIManager;

            //  grid.MouseMove += InventoryPanel_MouseOver;

            Image icon;
            UIComponent item;

            Grid grid = outerGrid;

            item = new UIComponent(gui);
            item.DebugTag = "stocksItem";

            grid.AddEntry(nutrientType, item);
          //  item.CanHaveFocus = true;                    

            Label lblCaption = new Label(gui);
            item.Add(lblCaption);
            lblCaption.Init(Label.LabelType.LCDNormal);
            lblCaption.X = 0;
            lblCaption.Text = nutrientType.Name;
            lblCaption.FitToText();
            item.CenterChildVertically(lblCaption);

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

            Label lblOverconsumed = new Label(gui);
            item.Add(lblOverconsumed);
            lblOverconsumed.Init(Label.LabelType.LCDNormal);
            lblOverconsumed.X = overconsumedX;
            lblOverconsumed.ID = DataControlID.Overconsumed;
            item.CenterChildVertically(lblOverconsumed);

            Label lblStored = new Label(gui);
            item.Add(lblStored);
            lblStored.Init(Label.LabelType.LCDNormal);
            lblStored.X = storedX;
            lblStored.ID = DataControlID.Stored;
            item.CenterChildVertically(lblStored);

            Label lblDaysLeft = new Label(gui);
            item.Add(lblDaysLeft);
            lblDaysLeft.Init(Label.LabelType.LCDNormal);
            lblDaysLeft.X = storedX;
            lblDaysLeft.ID = DataControlID.DaysLeft;
            item.CenterChildVertically(lblDaysLeft);


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
