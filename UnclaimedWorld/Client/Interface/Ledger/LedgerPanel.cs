using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using WindowSystem;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide;
using Microsoft.Xna.Framework.Graphics;
using UWGame.ClientSide.Interface.LCD;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.AI.Needs;
using UWGame.SimSide.Snapshots;
using RoundLineCode;

namespace UWGame.ClientSide.Interface.Ledger
{
    public class LedgerPanel: RosterPanel
    {
        
        /// <summary>
        /// this outer surface grid has a scrollbar. 
        /// NEW: content handles scrolling parts, since some headers or totals should stay in place.
        /// </summary>
      //  Grid surfaceGrid;


        ComboBox cbSource, cbRange; // don't scroll these??
        Label lblRange;

        LedgerSheet currentSheet;

        TextArea taHelp;

      //  List<GroupStatistics> statisticsToShow = new List<GroupStatistics>();

        const int headerHeight = 80;        
        
        const int windowWidth = 740;


        public LedgerPanel()        
            : base("LEDGER",           
            windowWidth, The.InGameUI.rosterPanelHeight/*)*/,false, 
            panelType: PanelType.RosterPanel) // to avoid using the event archive ctor...
        {
           
           /* CreateSurfaceWithScrollbar(out surfaceGrid, lcdSurface, true,
             headerHeight);*/
                        
            int yPos = 12;

            Label lblSource = new Label(Interface.gui);
            lcdSurface.Add(lblSource);  //surfaceGridItem.Add(lblSource); // 
            lblSource.Init(Label.LabelType.LCDHeadingBlue); 
            lblSource.Text = "DATA:";
            lblSource.Position = new Point(0, 0);         
            lblSource.FitToText();          
            lblSource.CenterThisVertically(yPos);
            lblSource.Y += 1; 
            
            cbSource = new ComboBox(Interface.gui, ListBoxType.LCDCombo, false);
            lcdSurface.Add(cbSource);  //surfaceGridItem.Add(cbSource); // 
            cbSource.Init(ComboBoxTypes.LCD);
            cbSource.X = lblSource.Right + SingleSpacing;
            //cbDifficulty.Y = itemPadding;
            //cbDifficulty.IsEditable = false;
            cbSource.Width = 180;
            cbSource.CenterThisVertically(yPos);
            cbSource.SelectionChanged += new SelectionChangedHandler(cbSource_SelectionChanged);


            lblRange = new Label(Interface.gui);
            lcdSurface.Add(lblRange);  //surfaceGridItem.Add(lblRange); // 
            lblRange.Init(Label.LabelType.LCDHeadingRed);
            lblRange.Text = "RANGE:";
            lblRange.Position = new Point(280, 0);
            // lblName.Width = difficultyColumnX - lblName.X - SingleSpacing; 
            lblRange.FitToText();
            //lblSource.ToolTip = optionSet.Description;
            lblRange.CenterThisVertically(yPos);
            lblRange.Y += 1; 

            cbRange = new ComboBox(Interface.gui, ListBoxType.LCDCombo, false);
            lcdSurface.Add(cbRange);  //surfaceGridItem.Add(cbRange); // 
            cbRange.Init(ComboBoxTypes.LCD);
            cbRange.X = lblRange.Right + SingleSpacing;
            cbRange.Width = 105;
            cbRange.CenterThisVertically(yPos);
            

            taHelp = new TextArea(Interface.gui, ListBoxType.LCD);
            taHelp.RenderType = RenderType.CRTAndLCD;
            taHelp.Init(Label.LabelType.LCDNormal);
            taHelp.CanGrowInHeight = true;
            lcdSurface.Add(taHelp);  //surfaceGridItem.Add(taHelp); // 
            taHelp.Y = lblSource.Bottom + 2;
            taHelp.Width = lcdSurface.Width;   


            // place the graph inside a bordered panel:
          /*  graphPanel = new LCDInnerPanel(Interface.gui, canvasWidth, true);
            surfaceGridItem.Add(graphPanel.Panel);   //lcdSurface.Add(graphPanel.Panel);
            graphPanel.ContentHeight = canvasHeight;
            graphPanel.Panel.Y = 66; // 94; // 60;
            */
           
            /*surfaceGridItem.Height = 575; // 600;

            surfaceGrid.AddEntry(surfaceGridItem, surfaceGridItem);
            */

            CreateSheets();
            PopulateRangesCombo();

            ShowSheet();

        }

        string sheetKey = "sheetKey";
      
        void cbSource_SelectionChanged(UIComponent sender)
        {
            ShowSheet();


           /*  surfaceGrid.RemoveEntry(sheetKey);
             surfaceGrid.AddEntry(sheetKey, sheet);
            */


             Refresh();
        }

        private void ShowSheet()
        {
            LedgerSheet sheet = cbSource.SelectedKey as LedgerSheet; 

            taHelp.Text = sheet.Tooltip;

            if (currentSheet != null)
            {
                lcdSurface.Remove(currentSheet);
                //surfaceGrid.RemoveEntry(currentSheet);
              
            }

            currentSheet = sheet;
            lcdSurface.Add(currentSheet);
            currentSheet.Y = headerHeight;
            //surfaceGrid.AddEntry(currentSheet, currentSheet);

            if (sheet.ShowRangeSelector)
            {
                cbRange.Visible = true;
                lblRange.Visible = true;

                SetFromDateOnSheet();
            }
            else
            {
                cbRange.Visible = false;
                lblRange.Visible = false;
            }
        }


        int sheetHeight = 600;
       
        private void CreateSheets()
        {
            //FoodProduction foodProd = new FoodProduction(Interface.gui, surfaceGrid.surface.Width, surfaceGrid.Height); 
            FoodProduction foodProd = new FoodProduction(Interface.gui, lcdSurface.Width, lcdSurface.Height - headerHeight); 
            cbSource.AddEntry(foodProd, foodProd.DisplayName);
            foodProd.LoadUserSettings(The.InGameUI.FoodProductionSettings.SortingSettings);

            NutrientsSheet nutrients = new NutrientsSheet(Interface.gui, lcdSurface.Width, lcdSurface.Height - headerHeight);
            cbSource.AddEntry(nutrients, nutrients.DisplayName);
            nutrients.LoadUserSettings(The.InGameUI.NutrientSheetSettings.SortingSettings);

            Production production = new Production(Interface.gui, lcdSurface.Width, lcdSurface.Height - headerHeight);
            cbSource.AddEntry(production, production.DisplayName);
            production.LoadUserSettings(The.InGameUI.ProductionSettings.SortingSettings);

            Kills kills = new Kills(Interface.gui, lcdSurface.Width, lcdSurface.Height - headerHeight);
            cbSource.AddEntry(kills, kills.DisplayName);
            kills.LoadUserSettings(The.InGameUI.KillsSettings.SortingSettings);


            /*
            foreach (var item in AllGraphTypes)
            {
                cbSource.AddEntry(item, item.DisplayName);
            }*/

            cbSource.SelectionChanged -= new SelectionChangedHandler(cbSource_SelectionChanged);   
            cbSource.SelectedIndex = 0;
            cbSource.SelectionChanged += new SelectionChangedHandler(cbSource_SelectionChanged);


        }

       
        private void PopulateRangesCombo()
        {
            cbRange.AddEntry(GraphPanel.Ranges.OneDay, "One day");
            cbRange.AddEntry(GraphPanel.Ranges.OneSeason, "One season");
            cbRange.AddEntry(GraphPanel.Ranges.OneYear, "One year");
            cbRange.AddEntry(GraphPanel.Ranges.TenYears, "Ten years");

            cbRange.SelectionChanged -= new SelectionChangedHandler(cbRange_SelectionChanged);
            cbRange.SelectedKey = GraphPanel.Ranges.OneYear; // SelectedIndex = 0;
            cbRange.SelectionChanged += new SelectionChangedHandler(cbRange_SelectionChanged);

        }

        void cbRange_SelectionChanged(UIComponent sender)
        {
            // keep the current plot data sets, but redraw the ranges
            SetFromDateOnSheet();

            Refresh();
        }

        private void SetFromDateOnSheet()
        {
            string fromLabelText;
            GraphPanel.Ranges rangeKey = (GraphPanel.Ranges)cbRange.SelectedKey;
            DateAndTime.TimeDateYear fromDate = GraphPanel.GetStartPoint(rangeKey, out fromLabelText);

            currentSheet.FromDate = fromDate;
        }

       /* private DateAndTime.TimeDateYear GetStartPoint(out string fromLabelText)
        {
            DateAndTime.TimeDateYear now = The.Sim.DateAndTime.CurrentTimeDateYear;
            fromLabelText = "";

            object rangeKey = cbRange.SelectedKey;
           
            // subtract the desired interval
            switch ((Ranges)rangeKey)
            {
                case Ranges.OneDay:
                    fromLabelText = "One day ago";
                    now.AddTime(-1); 
                    break;

                case Ranges.OneSeason:
                    fromLabelText = "One season ago";
                    now.AddTime(-DateAndTime.DaysPerSeason); 
                    break;

                case Ranges.OneYear:
                    fromLabelText = "One year ago";
                    now.AddTime(-DateAndTime.DaysPerYear);
                    break;

                case Ranges.TenYears:
                    fromLabelText = "Ten years ago";
                    now.AddTime(-10d * DateAndTime.DaysPerYear); 
                    break;
            }

            

            return now;
        }*/

        public override void Refresh()
        {
            base.Refresh();

            currentSheet.RefreshData();
                      
        }




        public override void Show()
        {
            // TODO: select from multilist
           /* statisticsToShow.Clear();
            statisticsToShow.Add(The.InGameUI.UIAllegiance.Statistics);
            */
           
            base.Show(); // calls Refresh


            return;
            
        }
             


    }


   
    
}
