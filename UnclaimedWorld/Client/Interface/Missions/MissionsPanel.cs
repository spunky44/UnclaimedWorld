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
using UWGame.SimSide.Overland;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Overland.Missions;
using UWGame.SimSide;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.AI;

namespace UWGame.ClientSide.Interface.Missions
{
    /// <summary>
    /// has the list of trade runs and missions
    /// 
    /// IDEA: keep expired trade runs in the list. Then a new run can easily be set up from it (by using its mission template)
    /// </summary>
    public class MissionsPanel : RosterPanel
    {
        Grid grdMissions;
        Grid outerGrid;

        const int itemHeight = 36;
        const int horizPadding = 6;
        const int vertPadding = 4;

        List<Mission> allMissionsToShow = new List<Mission>();

        const int headingYPos = 45;

        LCDInnerPanel addPanel;
        const int vehicleColumnX = 7;
        const int communicationColumnX = 140; // 260;
        const int locationColumnX = 270; // 140;        
        const int etaColumnX = 390;

        const string addPanelKey = "Add Panel";
     

        public MissionsPanel()
            : base("MISSIONS", 600, false)
        {
          //  lcdSurface.Width = 529;

            CreateColumnHeadingsWithFixedLength(lcdSurface, 0,
                  new Tuple<string, int, int>("TRANSPORT", 0, 136),
                 new Tuple<string, int, int>("COMMUNICATION", communicationColumnX, 128),
                 new Tuple<string, int, int>("NEXT STOP", locationColumnX, 118),
                 new Tuple<string, int, int>("ETA", etaColumnX, 143));

            int yPos = 21;
            outerGrid = new Grid(Interface.gui, ListBoxType.LCD, Label.LabelType.CRTBigGlow); // contains list + Create Run button panel
            outerGrid.FixedItemHeights = false;
            outerGrid.RenderType = RenderType.CRTAndLCD;
            lcdSurface.Add(outerGrid);
            outerGrid.HMargin = 0; // 5; 
            outerGrid.VMargin = 0; // 5; // 
            outerGrid.Font = GUIManager.LCDandHUDBodyFontPath; // Important! Must be set after Add! Add will reinitialize with defaults.
            outerGrid.Width = lcdSurface.Width;
            outerGrid.Height = lcdSurface.Height - yPos; // -gridTopMargin;
            outerGrid.CanGrowInHeight = false; // shows the scrollbar
            outerGrid.ScrollBarEnabled = true;
            outerGrid.Position = new Point(0, yPos);
            outerGrid.RowSpacing = -1;


            grdMissions = new Grid(Interface.gui, ListBoxType.LCD, Label.LabelType.LCDNormal);
            grdMissions.FixedItemHeights = true;
            grdMissions.HMargin = 0; // 5; 
            grdMissions.VMargin = 0; // 5; // 
            grdMissions.RenderType = RenderType.CRTAndLCD;
            outerGrid.AddEntry(grdMissions, grdMissions);
            grdMissions.Font = GUIManager.LCDandHUDBodyFontPath; // Important! Must be set after Add! Add will reinitialize with defaults.
            // grdMissions.Width = lcdSurface.Width;
            // grid.Height = lcdSurface.Height - gridTopMargin - gridBottomMargin;
            grdMissions.ItemHeight = itemHeight;  
            grdMissions.Position = new Point(0, 0);
            grdMissions.CanGrowInHeight = true;
            grdMissions.ScrollBarEnabled = false;
            grdMissions.Selectability = Grid.SelectabilityOptions.None;


            /*
            CreateGridWithColumnHeadingsWithFixedLength(lcdSurface, SingleSpacing, itemHeight, out grid, 40,
                   new Tuple<string, int, int>("TRANSPORT", 0, 136),
                   new Tuple<string, int, int>("COMMUNICATION", communicationColumnX, 128),
                   new Tuple<string, int, int>("NEXT STOP", locationColumnX, 118),
                   new Tuple<string, int, int>("ETA", etaColumnX, 143));
            */



            addPanel = new LCDInnerPanel(Interface.gui, lcdSurface.Width - 4, true, 1f);
            outerGrid.AddEntry(addPanelKey, addPanel.Panel);
            addPanel.ContentHeight = 102;
            addPanel.Panel.Y = 0;
            addPanel.Panel.X = 0;
            addPanel.Panel.Add(AddCreateActionButton());

           // grdMissions.ItemHeight = addPanel.Panel.Height; 


        }



        private UIComponent AddCreateActionButton()
        {
            UIComponent item = new UIComponent(Interface.gui);
            ImageButton btAction = new ImageButton(Interface.gui);
            item.Add(btAction);
            btAction.Init(ImageButtonType.AddAction);
            btAction.ScaleImageToSizeOfControl = false;
            btAction.Click += new ClickHandler(tbNew_Click);
            btAction.X = 5;
            btAction.Y = 5;
            btAction.DebugTag = "newRun";

            item.Width = btAction.Width;
            item.Height = btAction.Height;

            Label lblName = new Label(Interface.gui);
            item.Add(lblName);
            lblName.Init(Label.LabelType.LCDNormal);
            lblName.Text = "NEW RUN";
            lblName.X = 16;
            lblName.Y = 14;         
            return item;
        }

        void tbNew_Click(UIComponent sender, EventArgs e)
        {
            
            The.InGameUI.ChangeRosterPanel(The.InGameUI.CreateMissionPanel);

        }

        private void Populate()
        {
            UIComponent itemRow;

            GetAllMissionsToShow();

            outerGrid.BeginAddingEntries();
            grdMissions.BeginAddingEntries();


            foreach (var item in allMissionsToShow)
            {

                if (!grdMissions.TryGetEntry(item, out itemRow))
                {
                    itemRow = AddItemRow(item, item);

                }

                // update existing row:                   
                UpdateRow(item, itemRow);
            }

            //hacky but the add Panel either has a dedicated screen space or
            //if it is the part of the grid we need to set the itemHeight each time we add or update something  AO    
            /*grid.TryRemoveEntry(addPanelKey);
            grid.ItemHeight = addPanel.Panel.Height;
            */

            // remove unused rows           
            grdMissions.DeleteEntries<Mission>(j => allMissionsToShow.Contains(j));

            // grid.AddEntry(addPanelKey, addPanel.Panel);            

            grdMissions.EndAddingEntries();
            outerGrid.EndAddingEntries();
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


        private void CreateColumnHeadings()
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
            lblHeading.Text = "COMMUNICATION";
            lcdSurface.Add(lblHeading);
            lblHeading.Y = headingYPos;
            lblHeading.X = communicationColumnX;
            lblHeading.FitToText();


        }

        private void GetAllMissionsToShow()
        {
            allMissionsToShow.Clear();

            if (The.InGameUI.UIAllegiance.Missions != null)
            {
                allMissionsToShow.AddRange(The.InGameUI.UIAllegiance.Missions);
            }

            // sort:
            //  allTransportsToShow = allTransportsToShow.OrderBy(j => j.AllegianceB.Site.Name).ToList();
        }


        /* TODO
        private static void SetCommunication(Transport transport, Label lblCommunication)
        {
            CommunicationMethod? workingMethod;
            if (The.InGameUI.UIAllegiance.IsInCommunicationRange(relation.AllegianceB, out workingMethod))
            {
                lblCommunication.Text = "IN COMM RANGE";
                lblCommunication.ToolTip = string.Format("The transport can be reached with the communication equipment ({0}) that is currently deployed", 
                    CommunicatorType.GetName(workingMethod.Value));

                lblCommunication.ToolTip = string.Format("The transport can be reached with the communication equipment ({0}) that is currently deployed",
                        CommunicatorType.GetName(workingMethod.Value));

                lblCommunication.Color = lblCommunication.GetNormalColor();
            }
            else
            {
                lblCommunication.Text = "NO COMM";
                lblCommunication.ToolTip = "We currently have no way to communicate with the transport.";
                lblCommunication.Color = Color.Red; // does not work so well with tooltip hover
            }
            
        }*/

        private UIComponent AddItemRow(object key, Mission transport)
        {          
            UIComponent item;

            item = new UIComponent(Interface.gui);
            item.Height = itemHeight;
           /* grdMissions.ItemHeight = itemHeight;*/

            LCDInnerPanel rowPanel = new LCDInnerPanel(Interface.gui, grdMissions.SurfaceWidth /* lcdSurface.Width*/, false); // lcdSurfaceOptions.Width);
            item.Add(rowPanel.Panel);
            rowPanel.HorizontalContentPadding = horizPadding;
            rowPanel.VerticalContentPadding = vertPadding;


            Label lblVehicle = new Label(Interface.gui);
            rowPanel.AddContent(lblVehicle, GetItemColumnFromHeader(vehicleColumnX)); // lblImmigrationPull.Right + 6);           
            lblVehicle.Width = 120;
            lblVehicle.Init(Label.LabelType.LCDNormal);
            lblVehicle.ID = UIComponent.DataControlID.Transport;
            item.CenterChildVertically(lblVehicle);


            Label lblCommunication = new Label(Interface.gui);
            rowPanel.AddContent(lblCommunication, GetItemColumnFromHeader(communicationColumnX)); // lblImmigrationPull.Right + 6);            
            lblCommunication.Width = 120;
            lblCommunication.Init(Label.LabelType.LCDNormal);
            lblCommunication.ID = UIComponent.DataControlID.Communication;
            item.CenterChildVertically(lblCommunication);

            Label lblNextStop = new Label(Interface.gui);
            rowPanel.AddContent(lblNextStop, GetItemColumnFromHeader(locationColumnX));
            lblNextStop.Width = 120;
            lblNextStop.Init(Label.LabelType.LCDNormal);
            lblNextStop.ID = UIComponent.DataControlID.NextStop;
            item.CenterChildVertically(lblNextStop);

            Label lblNextStopETA = new Label(Interface.gui);
            rowPanel.AddContent(lblNextStopETA, GetItemColumnFromHeader(etaColumnX));
            lblNextStopETA.Width = 120;
            lblNextStopETA.Init(Label.LabelType.LCDNormal);
            lblNextStopETA.ID = UIComponent.DataControlID.ETA;
            item.CenterChildVertically(lblNextStopETA);

            grdMissions.AddEntry(transport, item);

            return item;
        }

        private void UpdateRow(Mission mission, UIComponent itemRow)
        {
            itemRow.Height = itemHeight;
            Label lblTransport = itemRow.FindChildById(UIComponent.DataControlID.Transport) as Label;
            EntityType vehicleType = mission.GetMainTransportation();
            if (vehicleType != null)
            {
                lblTransport.Text = vehicleType.Name;
            }
            else
            {
                lblTransport.Text = "On foot";
            }

            bool inCommRange;
            Label lblComm = itemRow.FindChildById(UIComponent.DataControlID.Communication) as Label;
            DiplomacyPanel.DisplayCommunication(The.InGameUI.UIAllegiance, mission, lblComm, "The mission can be reached with the communication equipment ({0}) that is currently deployed",
                                                                                             "We currently have no way to communicate with the mission. ETA and location are not up to date.", out inCommRange);
            
            Label lblNextStop = itemRow.FindChildById(UIComponent.DataControlID.NextStop) as Label;
            Label lblETA = itemRow.FindChildById(UIComponent.DataControlID.ETA) as Label;

          
            if (inCommRange)
            {
                lblETA.NormalColor = lblETA.GetNormalColorForType();

                /*DateAndTime.TimeDateYear?*/
               // string eta;
                DateAndTime.TimeDateYear? eta; 
                TravelLocation? nextStop;
                if (mission.GetNextStopAndETA(out nextStop, out eta))
                {
                    Expedition expedition;
                    IKnownEntityData terminal;
                    Allegiance allegiance;
                    Site site;
                    nextStop.Value.ResolveLocation(The.InGameUI.UIAllegiance.SharedKnowledge, out site, out allegiance, out expedition, out terminal);
                    if (site != null)
                    {
                        lblNextStop.Text = site.Name; 
                        lblETA.Text = eta.ToString(); //.ToString();
                        lblETA.ToolTip = "The estimated time of arrival"; // "The time it will take for the mission to arrive";
                    }
                    else
                    {
                        lblNextStop.Text = "";
                        lblETA.Text = "";
                    }
                }
                else
                {
                    lblNextStop.Text = "";
                    lblETA.Text = "";
                }
            }
            else
            {
                lblETA.NormalColor = Label.LCDErrorColor;
                lblETA.ToolTip = "We are not in communication with the mission and the information is out of date.";
            }

        }

    }
}
