using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using WindowSystem;
using UWGame.SimSide.Allegiances;
using UWGame.ClientSide.Interface.LCD;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Communication;

namespace UWGame.ClientSide.Interface
{
    public class DiplomacyPanel: RosterPanel
    {      
        Grid grid;


        const int itemHeight = 36;
        const int horizPadding = 6;
        const int vertPadding = 4;

        List<AllegianceRelation> allAllegiancesToShow = new List<AllegianceRelation>();

        public DiplomacyPanel()
            : base("CONTACTS", 600,false)
        {

            CreateGridWithColumnHeadings(lcdSurface, headingsY, itemHeight, out grid, 0,
                new Tuple<string, int>("NAME", 0),
                new Tuple<string, int>("LOCATION", locationColumnX),
                new Tuple<string, int>("MIGRTE FACTOR", immigrationColumnX),
                new Tuple<string, int>("COMMUNICATION", communicationColumnX));

            /*
            grid = FullLCDPanel.AddGridWithFixedItemHeights(Interface.gui, lcdSurface, 50);
            grid.ItemHeight = itemHeight;
            grid.Selectability = Grid.SelectabilityOptions.None;
            

             CreateColumnHeadings();*/

        }

        private void Populate()
        {
            UIComponent itemRow;

            GetAllAllegiancesToShow();

            double currentRating = The.InGameUI.UIAllegiance.Statistics.GetOverallRating();

            grid.BeginAddingEntries();


            foreach (var item in allAllegiancesToShow)
            {              

                if (!grid.TryGetEntry(item, out itemRow))
                {
                    itemRow = AddItemRow(item, item, currentRating);
                    
                }

                UpdateRow(item, itemRow, currentRating);

            }

            // remove unused rows           
            grid.DeleteEntries<AllegianceRelation>(j => allAllegiancesToShow.Contains(j));

            grid.EndAddingEntries();
        }



        public override void Refresh()
        {
            Populate();

            base.Refresh();
        }

        public override void Show()
        {


            base.Show(); // calls Refresh
        }

        const int headingYPos = 30;
     /*   private void CreateColumnHeadings()
        {
            Label lblHeading = new Label(Interface.gui);
            lblHeading.Init(Label.LabelType.LCDSmallHeadingBanner);
            lblHeading.Text = "NAME";
            lcdSurface.Add(lblHeading);
            lblHeading.Y = headingYPos;
            lblHeading.FitToText();

            lblHeading = new Label(Interface.gui);
            lblHeading.Init(Label.LabelType.LCDSmallHeadingBanner);
            lblHeading.Text = "LOCATION";
            lcdSurface.Add(lblHeading);
            lblHeading.Y = headingYPos;
            lblHeading.X = locationColumnX;
            lblHeading.FitToText();

            lblHeading = new Label(Interface.gui);
            lblHeading.Init(Label.LabelType.LCDSmallHeadingBanner);
            lblHeading.Text = "MIGRTE FACTOR";
            lcdSurface.Add(lblHeading);
            lblHeading.Y = headingYPos;
            lblHeading.X = immigrationColumnX;
            lblHeading.FitToText();

            lblHeading = new Label(Interface.gui);
            lblHeading.Init(Label.LabelType.LCDSmallHeadingBanner);
            lblHeading.Text = "COMMUNICATION";
            lcdSurface.Add(lblHeading);
            lblHeading.Y = headingYPos;
            lblHeading.X = communicationColumnX;
            lblHeading.FitToText();


        }*/
       
        private void GetAllAllegiancesToShow()
        {
            allAllegiancesToShow.Clear();

            // add the allegiances we know about (or still think exist?):
            List<AllegianceRelation> allegianceRelations;
            if (The.Sim.World.Relations.TryGetValue(The.InGameUI.UIAllegiance.ID, out allegianceRelations))
            {
                allAllegiancesToShow.AddRange(allegianceRelations);

             /*   foreach (var item in allegianceRelations)
                {
                    allAllegiancesToShow.Add(item.

                }*/
            }            

            // sort:
            allAllegiancesToShow = allAllegiancesToShow.OrderBy(j => j.AllegianceB.Site.Name).ToList();
        }

        const int locationColumnX = 140;
        const int immigrationColumnX = 260;
        const int communicationColumnX = 380;

        private UIComponent AddItemRow(object key, AllegianceRelation relation, double currentAllegianceRating)
        {
            Image icon;
            UIComponent item;

            item = new UIComponent(Interface.gui);
            item.Height = itemHeight;

            LCDInnerPanel rowPanel = new LCDInnerPanel(Interface.gui, grid.Width, false); // lcdSurfaceOptions.Width);
            item.Add(rowPanel.Panel);
            rowPanel.HorizontalContentPadding = horizPadding;
            rowPanel.VerticalContentPadding = vertPadding;


            TextButton tbView;

            Label lblName = new Label(Interface.gui);
            rowPanel.AddContent(lblName);
         //   lblName.Text = relation.AllegianceB.Name;
            lblName.Width = 200;
            lblName.Init(Label.LabelType.LCDNormal);
            lblName.ID = UIComponent.DataControlID.Caption;           
            item.CenterChildVertically(lblName);
          
          //  label.ToolTip = entityType;          


            Label lblSite = new Label(Interface.gui);
            rowPanel.AddContent(lblSite, locationColumnX - 7); // lblName.Right + 6);
            lblSite.Text = relation.AllegianceB.Site.Name;
            lblSite.Width = 200;
            lblSite.Init(Label.LabelType.LCDNormal);
            lblSite.ID = UIComponent.DataControlID.Location;
            item.CenterChildVertically(lblSite);


         /*   Label lblRelation = new Label(Interface.gui);
            item.Add(lblRelation);
            lblRelation.Text = relation.Relation;
            lblRelation.Width = 200;
            lblRelation.Init(Label.LabelType.LCDNormal);
            lblRelation.ID = UIComponent.DataControlID.Location;
            lblRelation.X = lblSite.Right + 6;
            item.CenterChildVertically(lblRelation);
            */

            Label lblImmigrationPull = new Label(Interface.gui);
            rowPanel.AddContent(lblImmigrationPull, immigrationColumnX - 7); // lblSite.Right + 6);            
          //  SetMigrationFactor(relation, lblImmigrationPull, currentAllegianceRating);
            lblImmigrationPull.Width = 20;
            lblImmigrationPull.Init(Label.LabelType.LCDNormal);
            lblImmigrationPull.ID = UIComponent.DataControlID.Immigration;
            item.CenterChildVertically(lblImmigrationPull);
            
            Label lblCommunication = new Label(Interface.gui);
            rowPanel.AddContent(lblCommunication, communicationColumnX - 7); // lblImmigrationPull.Right + 6);
          //  DisplayCommunication(relation, lblCommunication);
            lblCommunication.Width = 120;
            lblCommunication.Init(Label.LabelType.LCDNormal);
            lblCommunication.ID = UIComponent.DataControlID.Communication;
            item.CenterChildVertically(lblCommunication);


            grid.AddEntry(relation, item);

            return item;

          /*  CollapsablePanel collapsablePanel = null;

            AddCollapsablePanel(ref collapsablePanel, key);

            // common to all jobs:
            AddControlsToCollapsablePanelTitleBar(collapsablePanel, owner, key, name, status, job, canCancel);

            // TODO: should only be added when the job is expanded (will create a short delay!):

            int headerHeight = 28;

            AddProcessJobHeader(collapsablePanel, headerHeight);

            Grid grdMaterials = null;
            Grid grdTools = null;
            Grid gridInCollapsablePanelRight = null;

            // perhaps reuse this for other job types??
            AddColumnGrids(collapsablePanel, ref grdMaterials, ref grdTools, ref gridInCollapsablePanelRight, headerHeight);

            grdMaterials.ID = UIComponent.DataControlID.Materials;
            grdTools.ID = UIComponent.DataControlID.Tools;

            AddControlsToMaterialsGrid(grdMaterials, key, job, owner);
            */
        }

      
        private static void SetMigrationFactor(AllegianceRelation relation, Label lblMigration, double thisAllegianceRating)
        {
            Allegiance allegiance = relation.AllegianceB;
            GroupStatistics allegianceStats = allegiance.Statistics;

            double otherAllegianceRating = allegianceStats.GetOverallRating();

            double factor = thisAllegianceRating - otherAllegianceRating;

            lblMigration.Text = factor.ToString("F2");
            lblMigration.ToolTip = "Shows the overall migration pull factor between this allegiance and our own. If the number is positive, we could receive more migrants.";

        }




        public static void DisplayCommunication(/*AllegianceRelation relation,*/ ICommunicates commA, ICommunicates commB, Label lblCommunication, string inCommTooltip, string noCommTooltip, out bool inCommRange)
        {
            CommunicationMethod? workingMethod;
            if (Communicates.IsInCommunicationRange(commA, commB, out workingMethod)) //  commA.IsInCommunicationRange(commB, out workingMethod))//The.InGameUI.UIAllegiance.IsInCommunicationRange(relation.AllegianceB, out workingMethod))
            {
                lblCommunication.Text = "IN COMM RANGE";
                lblCommunication.ToolTip = string.Format(inCommTooltip, // "The allegiance can be reached with the communication equipment ({0}) that is currently deployed", 
                    CommunicatorType.GetName(workingMethod.Value));

                lblCommunication.NormalColor = lblCommunication.GetNormalColorForType();

                inCommRange = true;
            }
            else
            {
                lblCommunication.Text = "NO COMM";
                lblCommunication.ToolTip = noCommTooltip; // "We currently have no way to communicate with the allegiance.";
                lblCommunication.NormalColor = Label.LCDErrorColor;

                inCommRange = false;
            }
            
        }

        private void UpdateRow(AllegianceRelation relation, UIComponent itemRow, double currentAllegianceRating)
        {
            Label lblName = itemRow.FindChildById(UIComponent.DataControlID.Caption) as Label;
            lblName.Text = relation.AllegianceB.Name;

            Label lblSite = itemRow.FindChildById(UIComponent.DataControlID.Location) as Label;
            lblSite.Text = relation.AllegianceB.Site.Name;

            bool inCommRange;
            Label lblComm = itemRow.FindChildById(UIComponent.DataControlID.Communication) as Label;
            DisplayCommunication(The.InGameUI.UIAllegiance, relation.AllegianceB, lblComm, "The allegiance can be reached with the communication equipment ({0}) that is currently deployed", 
                                                                                           "We currently have no way to communicate with the allegiance.", out inCommRange);

            Label lblImmigrationPull = itemRow.FindChildById(UIComponent.DataControlID.Immigration) as Label;   
            SetMigrationFactor(relation, lblImmigrationPull, currentAllegianceRating);

          /*  bool hasLeftRowItemsToUpdate;
            bool hasRightRowItemsToUpdate;

            ProcessJob pJob = job as ProcessJob;
            if (pJob != null)
            {
                hasToolsForProcessJob = JobManager.HasToolsForProcess(pJob.ProcessType, owner);
            }
            UpdateTitleBar(itemRow, priority, job, owner, status);
            UpdateMaterialAndHarvestResourcesCollumn(itemRow, job, owner, out hasLeftRowItemsToUpdate);
            UpdateToolsColumn(itemRow, priority, job, owner, out hasRightRowItemsToUpdate);

            cpJob = itemRow as CollapsablePanel;
            if (hasLeftRowItemsToUpdate || hasRightRowItemsToUpdate)
            {
                cpJob.ShowAndEnableExpandButton();
            }
            else
            {
                cpJob.HideAndDisableExpandButton();
            }*/
           
        }

    }
}
