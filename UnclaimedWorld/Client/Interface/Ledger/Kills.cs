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
    public class Kills: LedgerSheet
    {
        public override string DisplayName
        {
            get { return "Kills"; }
        }

        public override string Tooltip
        {
            get { return "Shows the number of killed creatures."; }
        }

        public override bool ShowRangeSelector
        {
            get { return false; }
        }

        const int columnWidth = 90;
        const int nameWidth = 215;

        const int itemTypeIconColumnX = 12;
        const int nameX = 30;
        const int killedX = nameX + nameWidth;
      
      //  int topPartHeight = 100;
        int bottomPartHeight = 40;


        string nameTooltip;
        string killedTooltip;
      

        /// <summary>
        /// contains categories like Ingredients and Prepared food
        /// 
        /// NEW: make this scrollable, with a max hegiht. Then the sorting header can stay in place
        /// </summary>
        Grid outerGrid;

        public const int ItemHeight = 26; // 22;

        SortingButtons<KillsSettings.SortColumns> sortingButtons;
      //  UIComponent sortingButtonsContainer;

     
     
        UIComponent bottomContainer;
        Label lblTotalKilled;

        int gridYPos;

        public Kills(GUIManager gui, int width, int height): base(gui, width, height)
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

            lblTotalKilled = new Label(gui);
            bottomContainer.Add(lblTotalKilled);
            lblTotalKilled.Init(Label.LabelType.LCDNormal);
            lblTotalKilled.X = killedX;
            bottomContainer.CenterChildVertically(lblTotalKilled);
            
           
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
            killedTooltip = CreateTooltip("Killed", "Killed by the colony");
           

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


        public void LoadUserSettings(SortingSettings<KillsSettings.SortColumns> settings)
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
            sortingButtons = new SortingButtons<KillsSettings.SortColumns>(GUIManager);
            sortingButtons.Width = Width;
            sortingButtons.Height = 30;
            sortingButtons.Position = new Point(0, 0);
            Add(sortingButtons);
            sortingButtons.SortClicked += tbSort_Click;

            sortingButtons.AddTextButton(nameWidth, "NAME", KillsSettings.SortColumns.Name, nameTooltip);
            sortingButtons.AddTextButton(columnWidth, "KILLED.", KillsSettings.SortColumns.Kills, killedTooltip);
         
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
            
            UIComponent itemRow;


            // get the statistics data
 
            // get all the stats
            KillStatistics stats = The.InGameUI.UIAllegiance.Statistics.KillStatistics; 
           // Dictionary<EntityType, List<DataPoint<float>>> producedStats = stats..Stats[ProductionStatistics.StatTypes.Produced];
          


            // summarize:
            DateAndTime.TimeDateYear toTime = The.Sim.DateAndTime.CurrentTimeDateYear; 

            DateAndTime.TimeDateYear fromTime = FromDate; // toTime;
          
            int killedTotal = 0;
           
                                  

            outerGrid.BeginAddingEntries();


            foreach (var item in stats.Kills)
            {
                EntityType entityType = item.Key;
                             
                int killed = item.Value;
            
               
                killedTotal += killed;
               
              
                if (!outerGrid.TryGetEntry(entityType, out itemRow))
                {
                    itemRow = AddItemRow(entityType, owner, false);
                }

                UpdateItemRow(itemRow, entityType, killed);

            }

            

            // remove unused items   
          /*  foreach (CollapsablePanel colPanel in outerGrid.Entries)
            {
                Grid grd = (Grid)colPanel.ExpandedPanel.Controls[0];
                grd.DeleteEntries<EntityType>(j => stats.TypesWithStats.Contains(j));
            }*/

        

            //sort categories
         //   InventoryPanel.DoCategorySorting(outerGrid, categoryGrids, The.InGameUI.ProductionSettings.SortingSettings.SortOrder);

            outerGrid.Sort(u => u.OrderByTag1, The.InGameUI.KillsSettings.SortingSettings.SortOrder);

            outerGrid.EndAddingEntries();

            UpdateTotals(killedTotal);

            ResizeHeight();
        }


        private void UpdateTotals(int killedTotal)
        {
            lblTotalKilled.Text = killedTotal.ToString();
            lblTotalKilled.FitToText();

            lblTotalKilled.AlignRight(killedX);

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

       

        private void UpdateItemRow(UIComponent itemRow, EntityType entityType, int killed)
        {
                       
            // update production status:
            DataTypeButton tbCaption = (DataTypeButton)itemRow.FindChildById(UIComponent.DataControlID.Caption);
           // tbCaption.SetAvailableStatusColor(noOfAvailableItems > 0);

            Label lblKilled;
            itemRow.FindChildById(DataControlID.Killed, out lblKilled);
            lblKilled.Text = killed.ToString(); //SetLabelPositiveColorAndValue(lblKilled, produced);
            lblKilled.FitToText();
            lblKilled.ToolTip = killedTooltip;

            lblKilled.AlignRight(killedX);
            
            object orderBy = null;
            switch(The.InGameUI.KillsSettings.SortingSettings.SortedBy)
            {
                case KillsSettings.SortColumns.Name:
                    orderBy = entityType.Name;
                    break;
                case KillsSettings.SortColumns.Kills:
                    orderBy = killed;
                    break;
               

            }

            itemRow.OrderByTag1 = orderBy;

        }

        private UIComponent AddItemRow(EntityType entityType, EntityGroup owner, bool useCurrentUIOwner)
        {
            GUIManager gui = base.GUIManager;

            //  grid.MouseMove += InventoryPanel_MouseOver;

            Image icon;
            UIComponent item;

            item = new UIComponent(gui);
          
            outerGrid.AddEntry(entityType, item);
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

            Label lblKilled = new Label(gui);
            item.Add(lblKilled);
            lblKilled.Init(Label.LabelType.LCDNormal);
            lblKilled.X = killedX;
            lblKilled.ID = DataControlID.Killed;
            item.CenterChildVertically(lblKilled);
                     

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
