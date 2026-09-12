using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Overland;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Expeditions;
using UWGame.ClientSide.Interface.LCD;
using UWGame.SimSide.Overland.Missions;
using UWGame.Control.Commands;
using UWGame.SimSide.Overland.Missions.Templates;
using UWGame.SimSide;
using UWGame.SimSide.Communication;
using UWGame.SimSide.Overland.Locations;
using UWGame.ClientSide.Interface.Personnel;
using UWGame.ClientSide.Interface.World_map;
using UWGame.ClientSide.Interface.Controls;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.AI;
using UWGame.ClientSide.Interface.Inventory;
using UWGame.SimSide.XmlCollections;
using UWGame.SimSide.AI.StrategicDecisions;
using UWGame.ClientSide.Interface.BuyAndSell;
using UWGame.SimSide.Tiers;
using UWGame.SimSide.Policies;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Buildings;

namespace UWGame.ClientSide.Interface.Missions
{
    /// <summary>
    /// for now only return trips are implemented. If later on we decide to enable more complex routes, instead of having 2 combos, each destination should be selected from a combobox
    /// 
    /// 
    /// </summary>
    public class CreateMissionPanel : RosterPanel
    {
        /// <summary>
        /// this outer surface grid has a scrollbar. it contains the location panels beneath the selection panel
        /// </summary>
        Grid surfaceGrid;

               
        public TravelLocation? start, destination;


        /// <summary>
        /// key is EntityType
        /// </summary>
        ComboBox cbTransportation;


        /// <summary>
        /// displays "cost to hire / no transport available"
        /// right under the combo box
        /// </summary>
        //Label lblTransportNotes;
        Label lblTransportNotesOrCost;


        /// <summary>
        /// displayed at the LCD bottom, beneath the scrollable area
        /// </summary>
       // ErrorsAndMessages errorsAndMessages;

        ErrorAndMessagePanel errorAndMessagePanel;


        Label lblDestination, lblStartingLocation;

        Image iconHomeStart, iconHomeDestination;
      
        ActionPicker ActionPicker;

        /// <summary>
        /// displayed at the LCD bottom, beneath the scrollable area
        /// </summary>
        Label lblTotalMissionCost;
        Label lblTotalCargoBulk;

        Label lblAvailableCargoBulk;
        Label lblSupplyNeedsHeader, lblCostHeader, lblSupplyNeeds, lblCost, lblTotalTradingCredits;
       
        TextButton tbTransportMore;

        int labelXPos = 0;
        int controlXPos = 118;
        int controlWidth = 200;

        int labelWidth = 145;

        int travelPointControlWidth = 430;

        LCDInnerPanel selectionPanel;
      //  LCDInnerPanel lcdErrorMessagePanel;

        const Label.LabelType topLeftCaptionLabelStyle = Label.LabelType.LCDHeadingSteelGrey; // 
        const Label.LabelType totalCaptionLabelStyle = Label.LabelType.LCDSmallHeadingBanner;

        enum Mode { Edit, Create }

        MissionTemplate missionTemplate;

        /// <summary>
        /// it's very confusing and buggy to have both this property with lazy init and the variable which can stay null!
        /// </summary>
        MissionTemplate Mission
        {
            get
            {
                if (missionTemplate == null)
                {
                    // creating a temporary Mission makes it possible to use the same code for both edit and create on this panel.
                    // however, we want to clean up the temporary object and not allow it to change ID sequences (for replay purposes)

                    Expedition expedition = LookUp<Expedition, ExpeditionID>.FindByID(The.InGameUI.UIExpedition.Value);
                    OwnerID ownerID = ((IOwner)expedition).ID;
                    missionTemplate = new MissionTemplate(The.InGameUI.UIAllegiance.ID, ownerID, false); //The.InGameUI.UIAllegiance, false);
                }

                return missionTemplate;
            }
            set
            {
                this.missionTemplate = value;
            }
        }

        
        ExclamationMarkInACircle exclamationMarkStart, exclamationMarkDestination, exclamationTransport;



        /// <summary>
        /// perhaps later, put each travel leg in a grid instead.
        /// </summary>
        //   List<Tuple<TravelLocation, LCDInnerPanel>> locations = new List<Tuple<TravelLocation,LCDInnerPanel>>();

        const int innerPanelVerticalPadding = 8;
        const int staticBottomHeight = 60;

        Point worldMapDialogPosition;

        ImageButton actionPickerSourceButton;

        #region popup state - to be used after the dialog closes

        private /*static*/ MissionStopTemplate dialogSourceLocation;
        CargoActionTypes? dialogSourceAction;

        public enum WorldMapDialogSource { Start, End }

        public WorldMapDialogSource worldMapDialogSource;


        /// <summary>
        /// is null when creating, but filled when editing
        /// </summary>
        MissionActionTemplate dialogSourceActionTemplate;

        #endregion

        public MissionTemplate getMissionTemplate()
        {
            return missionTemplate;
        }

        public CreateMissionPanel()
            : base("CREATE NEW RUN", 620, true)
        {

            // static top selection area (outside of scrollable area):
            AddSelectionPanel();

            // scrolls mission part:
            CreateSurfaceWithScrollbar(out surfaceGrid, lcdSurface, true, 184);
            //, //titleBottom + SingleSpacing, 
            // staticBottomHeight);
            lcdSurface.DebugTag = "createMissionSurface";
            surfaceGrid.DebugTag = "grdLocations";

            errorAndMessagePanel = new ErrorAndMessagePanel(lcdSurface);
            /*
            lcdErrorMessagePanel = new LCDInnerPanel(Interface.gui, lcdSurface.Width, true, 1f);
            lcdSurface.Add(lcdErrorMessagePanel.Panel);          
            lcdErrorMessagePanel.ContentHeight = 30;
            lcdErrorMessagePanel.Panel.Y = lcdSurface.Height - 35; // 24; 
            lcdErrorMessagePanel.VerticalContentPadding = 8;

            errorsAndMessages = new ErrorsAndMessages(lcdSurface, Window.guiManager, 10, lcdErrorMessagePanel.Panel.Y + 10); //below scrollable area
            */


            UpdateMissionStopGridYPosAndHeight();

            worldMapDialogPosition = new Point(base.Window.X - 75, base.Window.Y + 75);


            // selection panel is given a low number to stay at the top when the grid gets sorted:
            // selectionPanel.Panel.OrderByTag1 = -1000;

            //AddBottomStaticPart();

            ActionPicker = new Missions.ActionPicker(Interface.gui);
            ActionPicker.ActionSelected += new Action<ActionTypes>(ActionPicker_ActionSelected);
            ActionPicker.MouseOut += new MouseOutHandler(ActionPicker_MouseOut);
            ActionPicker.InvalidLocation += ActionPicker_InvalidLocation;
            lcdSurface.Add(ActionPicker);
            HideActionPicker();

            // 'Panel' buttons:
            TextButton btStartRun = AddLowerButton("START RUN", "Click to start this run", Align.Left);
            btStartRun.Click += new ClickHandler(btStartRun_Click);

            TextButton btCancelRun = AddLowerButton("CANCEL RUN", "Click to cancel this run", Align.Right);
            btCancelRun.Click += btCancelRun_Click;


        }

        void ActionPicker_InvalidLocation()
        {
            HideActionPicker();

            Revalidate();
            /*
            List<string> errors = null;
            FieldError? errorFieldCode = null;
            
            // now validate...    
            if (!ValidateMissionTemplate(ref errors, ref errorFieldCode))
            {
                if (errors != null && errors.Count > 0)
                {
                    ShowErrors(errors, errorFieldCode);
                }
            }*/
        }

        void btCancelRun_Click(UIComponent sender, EventArgs e)
        {
            OnCancel();

            surfaceGrid.Clear();

            Hide();

            The.InGameUI.ChangeRosterPanel(The.InGameUI.MissionsPanel);
        }

        private void UpdateMissionStopGridYPosAndHeight()
        {
            surfaceGrid.Y = selectionPanel.Panel.Bottom + 6;
            surfaceGrid.Height = lcdSurface.Height - surfaceGrid.Y - errorAndMessagePanel.ContentHeight - 13; // lcdErrorMessagePanel.ContentHeight - 13;
        }

        void ActionPicker_MouseOut(UIComponent sender, InputEventSystem.MouseEventArgs args)
        {
            // lcdSurface.Remove(ActionPicker); // makes GuiManager crash...
            HideActionPicker();

            actionPickerSourceButton = null;
        }



        const int totalColumnX = 435 + 43;
        const int totalCaptionColumnX = 370; // 335;

        /*  private void AddBottomStaticPart()
          {
           
          }*/


        private void UpdateTotalCost()
        {
            if (missionTemplate != null)
            {
                EntityGroup entityGroup = LookUp<EntityGroup, EntityGroupID>.FindByID((EntityGroupID)The.InGameUI.UIOwner.Value);

                lblTotalCargoBulk.Text = Entity.GetBulkAsString(missionTemplate.ComputeTotalCargoBulk());
                lblTotalCargoBulk.ToolTip = "Amount of cargo bulk packed into the vehicle.";

                lblAvailableCargoBulk.Text = "/ " + Entity.GetBulkAsString(missionTemplate.GetTotalCargoCapacity());
                lblAvailableCargoBulk.X = lblTotalCargoBulk.Right; // +SingleSpacing;
                lblAvailableCargoBulk.ToolTip = "Total amount of cargo bulk that the vehicle can transport.";

                lblTotalMissionCost.Visible = true;
                decimal totalBoughtCost, totalSoldCost, transportCost; // always positive

                // total is negative when we are earning money
                decimal total = missionTemplate.ComputeTotalCost(out transportCost, out totalBoughtCost, out totalSoldCost);
                lblTotalMissionCost.Text = Common.GetPriceAsString(total);
                ColorLabelByValue(lblTotalMissionCost, total);

                              
                StringBuilder text = new StringBuilder();
                Common.AppendLine(text, "The total cost of the mission.");             
                Common.AppendDivider(text);
                Common.Append(text, "Transport cost: ");
                Common.Append(text, Common.GetPriceAsString(transportCost, true, Common.ValueTint.Negative));
                Common.AppendLine(text);
                if (!Common.IsZero(totalBoughtCost))
                {
                    Common.Append(text, "Cost of bought items: ");
                    Common.Append(text, Common.GetPriceAsString(totalBoughtCost, true, Common.ValueTint.Negative));
                    Common.AppendLine(text);
                }
                if (!Common.IsZero(totalSoldCost))
                {
                    Common.Append(text, "Price of sold items: ");
                    Common.Append(text, Common.GetPriceAsString(totalSoldCost, true, Common.ValueTint.Positive));
                    Common.AppendLine(text);
                }
                Common.AppendLine(text);
                if (Common.IsGreaterThan(total, 0m))
                {
                    Common.Append(text, "Total cost: ");
                    Common.Append(text, Common.GetPriceAsString(total, true, Common.ValueTint.Negative));                   
                }
                else
                {
                    Common.Append(text, "Total earned: ");
                    Common.Append(text, Common.GetPriceAsString(total, true, Common.ValueTint.Positive));
                }

                //Common.AppendFormat(text, "Max. amount we can sell: {0}", true, noOfAvailableItems);
                lblTotalMissionCost.ToolTip = text.ToString();


                lblTotalTradingCredits.Text = " / " + Common.MoneyAsString(entityGroup.Parent.TradeCredits.Value, true);
                lblTotalTradingCredits.ToolTip = "Total amount of available Credits.";
                lblTotalTradingCredits.X = lblTotalMissionCost.Right; // +SingleSpacing;



                if (missionTemplate.StartMissionStopTemplate != null && missionTemplate.StartMissionStopTemplate.TravelAction != null
                    && missionTemplate.TransportationType != null)
                {
                    double distance = missionTemplate.GetTotalDistance();

                    decimal startFee, totalDistanceCost, costPerKilometer;
                    total = missionTemplate.ComputeTransportationCost(out startFee, out totalDistanceCost, out costPerKilometer);

                    lblCost.Text = Common.GetPriceAsString(costPerKilometer) + "/ KM";  //Math.Round((double.Parse(lblTotalMissionCost.Text) / missionTemplate.StartMissionStopTemplate.TravelAction.Distance), 1) + "/ KM";
                    lblCost.ToolTip = "The cost of transportation / km";
                }
            }
            //  lblTotalMissionCost.ToolTip = "Total expenses (in trade credits) for starting this run.";

        }

        private void ColorLabelByValue(Label lbl, decimal total)
        {
            if (Common.IsZero(total))
            {
                lbl.NormalColor = lbl.GetNormalColorForType();
            }
            else if (total < (decimal)0)
            {
                lbl.NormalColor = GameData.Instance.GUIConstants.PositiveColor;
            }
            else
            {
                lbl.NormalColor = GameData.Instance.GUIConstants.NegativeColor;
            }
        }

        /*  void btStartRun_Click(UIComponent sender, EventArgs e)
          {
              if (missionTemplate != null)
              {
                  if (missionTemplate.ID == MissionTemplateID.Invalid)
                  {
                      // only create the ID on commit:
                      missionTemplate.AddToLookup();
                  }

                  Mission mission = new Mission(this.missionTemplate); //, true);
              }
          }*/

        

        private void ShowError(string error, string errorTooltip = null, FieldError? errorField = null)
        {
            errorAndMessagePanel.ShowError(error, errorTooltip);

            if (errorField.HasValue)
            {
                string combinedError = error;
                if (errorTooltip != null)
                {
                    combinedError += " \n" + errorTooltip;
                }

                switch(errorField.Value)
                {
                  
                    case FieldError.Start://0
                        ShowLocationError(combinedError, exclamationMarkStart, lblStartingLocation, iconHomeStart);
                   
                        break;

                    case FieldError.Destination://1
                        ShowLocationError(combinedError, exclamationMarkDestination, lblDestination, iconHomeDestination);
                                                                   
                        break;
                    case FieldError.Transport://2
                    
                        lblTransportNotesOrCost.Visible = false;

                        exclamationTransport.Visible = true;
                        exclamationTransport.ToolTip = combinedError;

                        break;
                }
            }
           


        }

        private static void ShowLocationError(string tooltip, ExclamationMarkInACircle exclamation, Label location, Image homeIcon)
        {
            exclamation.Visible = true;
            exclamation.ToolTip = tooltip;

            if (homeIcon.Visible)
            {
                exclamation.X = homeIcon.Right;
            }
            else
            {
                if (location.Visible)
                {
                    exclamation.X = location.Right + 4;
                }
                else
                {
                    exclamation.X = location.X;
                }
            }
        }


        private void Revalidate()
        {
            if (errorAndMessagePanel.IsShowingError)
            {
                ClearErrors();

                List<string> errors = null;
                FieldError? errorFieldCode = null;

                if (!ValidateMissionTemplate(ref errors, ref errorFieldCode))
                {
                    if (errors != null && errors.Count > 0)
                    {
                        ShowErrors(errors, errorFieldCode);
                    }
                }
            }
        }


        private void ClearErrors()
        {
            errorAndMessagePanel.Clear();

            lblTransportNotesOrCost.Visible = true;                     

            exclamationMarkDestination.Visible = false;
            exclamationMarkStart.Visible = false;
            exclamationTransport.Visible = false;
        }

        void btStartRun_Click(UIComponent sender, EventArgs e)
        {
            // clear old errors before validating
            ClearErrors();

            List<string> errors = null;
            FieldError? errorFieldCode = null;

            // For now, we don't allow templates that can not be used for a mission to exist...
            // validate that a mission can actually be started from this template... do all allegiances, vehicles etc. still exist?
            // validate that cargo runs are possible with the limits...           
            if (ValidateMissionTemplate(ref errors, ref errorFieldCode))
            {
                Site fromSite;
                Allegiance fromAllegiance;
                Expedition fromExpedition;
                IKnownEntityData fromTerminal;

                Mission.StartMissionStopTemplate.TravelLocation.ResolveLocation(The.InGameUI.UIAllegiance.SharedKnowledge, out fromSite, out fromAllegiance, out fromExpedition, out fromTerminal);

                // get vehicles and items:
               

                Dictionary<EntityType, List<Entity>> vehicles = null;

                if (!GetVehiclesToAssign(fromExpedition, ref vehicles))
                {
                    ShowError("No vehicle available.", null, FieldError.Transport);// TODO: TEST THIS!!  

                    return;
                }

                if (VehiclesAreHired(fromExpedition)
                    && !CanCommunicateWithExpedition(fromExpedition))
                {
                    ShowError("No communication with Start allegiance.", noCommTooltip, FieldError.Start);
                   
                    return;
                }

                //It only checks the communication between the destinationExpedition and the players Expedition
                //if we have more travelLocations in the mission it doesnt check if we can communicate with those Expeditions
                if (!CanCommunicateWithExpedition(LookUp<Expedition, ExpeditionID>.FindByID((ExpeditionID)destination.Value.ExpeditionID)))
                {
                    ShowError("No communication with Destination allegiance.", noCommTooltip, FieldError.Destination);
                   
                    return;
                }


                // first finish creating the mission template:
                Command createTemplateCommand = new SimSide.Commands.CreateMissionTemplate(missionTemplate);

                // this will assign the missing template ID:
                The.Client.Controller.StoreAndExecuteCommand(createTemplateCommand);


              //  decimal totalMissionCost = missionTemplate.ComputeTotalCost(); // why..?

                // now create the mission itself 
                Command createMissionCommand = new SimSide.Commands.CreateMission(missionTemplate.ID, fromExpedition.OwnedEntities.ID, vehicles);

                // this creates a job too:
                The.Client.Controller.StoreAndExecuteCommand(createMissionCommand);

                Hide();

                The.InGameUI.RosterAccessPanel.ShowMissions();
            }
            else
            {
                if (errors != null && errors.Count > 0)
                {
                    ShowErrors(errors, errorFieldCode);                
                }
               /* else
                {
                    ClearErrors();                    
                }*/
            }

        }

        private void ShowErrors(List<string> errors, FieldError? errorField) // int? errorFieldCode)
        {
            ShowError(errors[0], null, errorField);

            /*
            if (errorField.HasValue)
            {
                switch (errorField.Value)
                {
                    case 0: exclamationMarkStart.Visible = true; lblStartingLocation.Visible = false; break;
                    case 1: exclamationMarkDestination.Visible = true; lblDestination.Visible = false; break;
                    case 2: exclamationTransport.Visible = true; lblTransportNotes.Visible = false; break;
                }
            }

            errorsAndMessages.ShowError(errors[0], null);
            TintErrorPanel(Color.LightSalmon);*/
        }


        private bool GetVehiclesToAssign(Expedition fromExpedition, ref Dictionary<EntityType, List<Entity>> vehicles)
        {
            // the vehicles should get a job lock set on them when Mission executes
            //EntityType vehicleType = (EntityType)cbTransportation.SelectedKey;
            // Mission.TransportationType.Vehicles;

            vehicles = new Dictionary<EntityType, List<Entity>>();

            if (Mission.TransportationType != null
                && Mission.TransportationType.Vehicles != null)
            {
                foreach (var vehicleType in Mission.TransportationType.Vehicles)
                {

                    fromExpedition.GetAvailableVehicles(GameData.Instance.AllEntityTypes[vehicleType.First], ref vehicles,                        
                        vehicleType.Second); // it would be easy to order additional transports of the same type...

                    if (vehicles.Count == 0)
                    {
                        return false;
                    }
                }
            }

            return true;

        }




        bool ValidateMissionTemplate(ref List<string> errors, ref FieldError? errorFieldCode)
        {
            if (Mission.StartMissionStopTemplate != null)
            {
                if (!Mission.ValidateMissionStops())
                {
                    HandleDestroyedMissionStop();
                    return false;
                }
            }

            return Mission.Validate(ref errors, ref errorFieldCode);
        }


        /*
        private bool IsStartLocation(TravelLocation location)
        {
            foreach (var item in surfaceGrid.EntriesByKey)
            {
                TravelLocation gridLocation = item.Key as TravelLocation;
                if (gridLocation != null)
                {
                    if (gridLocation == location)
                    {
                        return true;
                    }
                    else return false;
                }
            }

           
            return false;
        }*/


        private void HandleDestroyedMissionStop()
        {
            // show a popup message to the user.

            The.InGameUI.MessageBox.ShowMessage("One of the travel locations no longer exists. It is not possible to continue editing the mission.");

            The.InGameUI.MessageBox.OKClick += new EventHandler(MessageBoxDestroyedMission_OKClick);

            // leave the roster open beneath the modal dialog.
        }

        void MessageBoxDestroyedMission_OKClick(object sender, EventArgs e)
        {
            The.InGameUI.MessageBox.OKClick -= new EventHandler(MessageBoxDestroyedMission_OKClick);

            // destroy the temporary mission data:
            DestroyMission();

            Hide();

        }

        void btNewAction_Click(UIComponent sender)
        {


        }

        void btAddAction_Click(UIComponent sender, EventArgs e)
        {
            actionPickerSourceButton = (ImageButton)sender;



            //lcdSurface.Add(ActionPicker);
            ShowActionPicker();

            ActionPicker.Fill(missionTemplate, actionPickerSourceButton.Tag1 as MissionStopTemplate);
            // populate will resize:
            ActionPicker.Show();

            int xPos = sender.AbsolutePosition.X - base.display.AbsolutePosition.X;
            int yPos = sender.AbsolutePosition.Y - base.display.AbsolutePosition.Y;

            yPos = sender.AbsolutePosition.Y;
            //  yPos = sender.Parent.Parent.Parent.Parent.Parent.Parent.Parent.Parent.AbsolutePosition.Y;

            ActionPicker.CenterThisHorizontally(xPos);
            ActionPicker.CenterThisVertically(yPos);



            // clamp:
            InGameInterface.PlaceWindowInsideViewableArea(ActionPicker, lcdSurface);

        }

        private void ShowActionPicker()
        {
            ActionPicker.Visible = true;
        }

        void ActionPicker_ActionSelected(ActionTypes selectedAction)
        {
            HideActionPicker();

            MissionStopTemplate location = actionPickerSourceButton.Tag1 as MissionStopTemplate;
            Point dialogSourceAbsolutePosition = actionPickerSourceButton.AbsolutePosition;

            actionPickerSourceButton = null;

            /*  if (Mission.StartMissionStopTemplate == location)
              {*/
            Site site;
            Allegiance allegiance;
            Expedition expedition;
            IKnownEntityData terminal;

            if (!location.TravelLocation.ResolveLocation(The.InGameUI.UIAllegiance.SharedKnowledge, out site, out allegiance, out expedition, out terminal))
            {
                HandleDestroyedMissionStop();
                return;
            }


            switch (selectedAction)
            {
                case ActionTypes.Buy:

                    EntityGroup playerBuyer =  GetPlayerBuyer();

                    BuyOrLoadFromLocation(dialogSourceAbsolutePosition, CargoActionTypes.Buy, location, /*allegiance,*/ expedition, playerBuyer);
                    break;

                case ActionTypes.Load:
                    BuyOrLoadFromLocation(dialogSourceAbsolutePosition, CargoActionTypes.Load, location, /*allegiance,*/ expedition, null);
                    break;

                case ActionTypes.Sell:

                    EntityGroup npcBuyer = GetNPCBuyer();

                    BuyOrLoadFromLocation(dialogSourceAbsolutePosition, CargoActionTypes.Sell, location, /*allegiance,*/ expedition, npcBuyer);
                    break;

                case ActionTypes.Embark:
                    EmbarkFromLocation(dialogSourceAbsolutePosition, location, allegiance, expedition);
                    break;
            }

            /* }
             else //if (sender == cbAddActionDestination)
             {
                 // The.InGameUI.BuySellDialog.ShowDialog(true);  

                 // for now, we sell/unload all...
                 // perhaps later, we will implement more complex order queues.

             }*/

            UpdateTotalCost();
        }

        private void HideActionPicker()
        {
            ActionPicker.Visible = false;
            ActionPicker.X = 1000; // Visible false still intercepts mouse clicks...
        }


        /// <summary>
        /// when creating a new order...
        /// </summary>
        /// <param name="dialogSourceAbsolutePosition"></param>
        /// <param name="selectedAction"></param>
        /// <param name="location"></param>
        /// <param name="allegiance"></param>
        /// <param name="ownerOfItemsExpedition"></param>
        private void BuyOrLoadFromLocation(Point dialogSourceAbsolutePosition, CargoActionTypes selectedAction, MissionStopTemplate location, //Allegiance allegiance, 
            Expedition ownerOfItemsExpedition, EntityGroup buyer)
        {

            // save the action:
            dialogSourceAction = selectedAction;

            // save the key...
            dialogSourceLocation = location;


            ShowBuySellDialog(dialogSourceAbsolutePosition, selectedAction, null, /*allegiance,*/ ownerOfItemsExpedition, buyer); 

        }


        private void EmbarkFromLocation(Point dialogSourceAbsolutePosition, MissionStopTemplate location, Allegiance allegiance, Expedition expedition)
        {
            /* if (expedition != null)
             {*/
            // save the action:
            //  dialogSourceAction = selectedAction;

            // save the key...
            dialogSourceLocation = location;

           // List<EntityID> entities = expedition.GetMembersReadyToEmigrate();

            ShowPersonnelDialog(dialogSourceAbsolutePosition); //, entities); 
           
        }



        private void ShowPersonnelDialog(Point dialogSourceAbsolutePosition) //, List<EntityID> entities)
        {
            ShowModalOverlay();

            PersonnelDialog dialog = The.InGameUI.PersonnelDialog;

            dialog.OKClick += new EventHandler(personnelDialog_OKClick);
            dialog.CancelClick += new EventHandler(personnelDialog_CancelClick);

            // show the buy/load dialog
            // string headingText = GetLoadUnloadHeading(selectedAction, allegiance);

            dialog.FillAndShow(GetPeople, dialogSourceAbsolutePosition, true, false); //, entities);

        }

        void personnelDialog_CancelClick(object sender, EventArgs e)
        {

            //   UIComponent locationPanel = surfaceGrid.EntriesByKey[dialogSourceLocation];
            /*  ComboBox cbAddAction = (ComboBox)(locationPanel.FindChildById(UIComponent.DataControlID.AddAction));

              cbAddAction.SelectedIndex = -1;*/

            ResetAfterPersonnelDialog();

        }

      /*  public static bool CanBeSelectedForEmbark(Entity entity)
        {
            return entity.Intelligence.IsReadyForEmbark(The.InGameUI.UIAllegiance);            
        }*/

       

        void personnelDialog_OKClick(object sender, EventArgs e)
        {
            PersonnelDialog dialog = The.InGameUI.PersonnelDialog;

            List<EntityID> selection = dialog.GetSelectedEntities();

            /* if (selection != null && selection.Count > 0)
             {*/
            // create the MissionAction to embark:
            MissionActionTemplate newAction = null;

            // create or edit?
            bool isCreating = false;
            if (dialogSourceActionTemplate == null)
            {
                isCreating = true;
            }

            Site site;
            Allegiance allegiance;
            Expedition sellingExpedition;
            IKnownEntityData terminal;
            if (dialogSourceLocation.TravelLocation.ResolveLocation(The.InGameUI.UIAllegiance.SharedKnowledge, out site, out allegiance, out sellingExpedition, out terminal))
            {
                if (isCreating)
                {
                    if (selection != null && selection.Count > 0)
                    {
                        // create:
                        newAction = new EmbarkActionTemplate(dialogSourceLocation, selection, true);
                    }
                }
                else
                {
                    // update:
                    EmbarkActionTemplate existingAction = dialogSourceActionTemplate as EmbarkActionTemplate;
                    if (selection != null && selection.Count > 0)
                    {
                        // update existing order
                        existingAction.PassengerListTemplate.Passengers = selection.Select(p => (long)p).ToList();

                    }
                    else
                    {
                        // delete action if no orders
                        existingAction.MissionStopTemplate.RemoveAction(existingAction);
                    }

                }

            }
            else
            {
                ResetAfterPersonnelDialog();
                HandleDestroyedMissionStop(); // this closes the roster.
                return;
            }
            // }

            if (newAction != null)
            {
                Mission.AddAction(newAction, dialogSourceLocation);
            }

            // repopulate from the data source:
            RepopulateAfterAddedAction();

            // reset
            ResetAfterPersonnelDialog();

        }

        private void ShowWorldMapDialog(Point absolutePosition) //, CargoActionTypes selectedAction, Dictionary<EntityType, int> currentOrders, Allegiance allegiance, Expedition expedition)
        {

            ShowModalOverlay();

            WorldMapDialog dialog = The.InGameUI.WorldMapDialog;

            dialog.OKClick += new EventHandler(WorldMapDialog_OKClick);
            dialog.CancelClick += new EventHandler(WorldMapDialog_CancelClick);

            dialog.SelectedTravelLocation = null;

            EntityGroup playerBuyer = GetPlayerBuyer();
            EntityGroupID? buyerID = null;
            if (playerBuyer != null)
            {
                buyerID = playerBuyer.ID;
            }
            dialog.Fill(buyerID);

            // center dialog over the click source:
            dialog.ShowInScreenSpace(worldMapDialogPosition.X, worldMapDialogPosition.Y); // absolutePosition.X - dialog.Window.Width / 2, absolutePosition.Y - dialog.Window.Height / 2, false);

            //   dialog.ShowInScreenSpace(absolutePosition.X - dialog.Window.Width / 2, absolutePosition.Y /*- dialog.Window.Height*/ / 2, false);
        }


        private void ShowBuySellDialog(Point absolutePosition, CargoActionTypes selectedAction,
            Dictionary<EntityType, List<EntityID>> currentOrders,  
           // Allegiance allegiance, 
            Expedition ownerOfItemsExpedition, EntityGroup buyer)
        {

            ShowModalOverlay();

            BuySellPanel dialog = The.InGameUI.BuySellDialog;

            dialog.OKClick += new EventHandler(BuySellDialog_OKClick);
            dialog.CancelClick += new EventHandler(BuySellDialog_CancelClick);


            BuySellPanel.BuySellDialogMode mode; // = BuySellPanel.BuySellDialogMode.ActionBuyAtNPC;

            if (selectedAction == CargoActionTypes.Buy)
            {
                mode = BuySellPanel.BuySellDialogMode.ActionBuyAtNPC;
            }
            else //if (selectedAction == CargoActionTypes.Sell)
            {
                mode = BuySellPanel.BuySellDialogMode.ActionSellAtPlayer;
            }
 
            // show the buy/load dialog
            The.InGameUI.FillAndShowBuySellDialog(absolutePosition, mode, // selectedAction, true,
                currentOrders, CanTrade, /*allegiance,*/ ownerOfItemsExpedition.OwnedEntities, buyer, GetItemsForSale);           

        }

       

        private List<EntityID> GetItemsForSale(EntityType type, out bool sourceIsInvalid)
        {
            Expedition expedition = GetHomeExpedition();

            // crashes here?... null exception

            if (dialogSourceLocation == null)
            {               
#if !RELEASE
                throw new Exception("dialogSourceLocation is null"); // #TRADECRASH - examine why this happens
#endif

                sourceIsInvalid = true; // in release, let's handle this as an invalid source... remove this after fixing the error.
                return null;
            }

            return GetItemsForSale(type, dialogSourceLocation.TravelLocation, expedition, out sourceIsInvalid);
        }

        public static List<EntityID> GetItemsForSale(EntityType type, TravelLocation travelLocation, Expedition buyingExpedition, out bool sourceIsInvalid)
        {
          
            sourceIsInvalid = false;
            Site fromSite;
            Allegiance fromAllegiance;
            Expedition fromExpedition;
            IKnownEntityData fromTerminal;

            Dictionary<EntityType, List<EntityID>> wares = new Dictionary<EntityType, List<EntityID>>();

            OwnerID sellerID;

            SharedKnowledge sharedKnowledge = The.InGameUI.UIAllegiance.SharedKnowledge;
            if (travelLocation.ResolveLocation(sharedKnowledge, out fromSite, out fromAllegiance, out fromExpedition, out fromTerminal))
            {
                // retrieve from TerminalContainer, always visible
                if (fromTerminal != null)
                {
                    sellerID = ((IOwner)fromExpedition).ID;
                    List<EntityID> listOfEntities;
                      if (fromTerminal.OfferedEntitiesByType != null 
                        && fromTerminal.OfferedEntitiesByType.TryGetValue(type, out listOfEntities))
                    {
                        return GetValidItemsForSale(fromTerminal, sellerID, sharedKnowledge, listOfEntities, buyingExpedition);
                    }
                    else return null;
                }
                else
                {
                    sourceIsInvalid = true;
                    return null;
                }
            }
            else
            {
                sourceIsInvalid = true;
                return null;
            }
        }

      /*  public Dictionary<EntityType, List<EntityID>> GetItemsForSale()
        {
            Site fromSite;
            Allegiance fromAllegiance;
            Expedition fromExpedition;
            IKnownEntityData fromTerminal;

            Dictionary<EntityType, List<EntityID>> wares = new Dictionary<EntityType, List<EntityID>>();

            OwnerID sellerID;

            SharedKnowledge sharedKnowledge = The.InGameUI.UIAllegiance.SharedKnowledge;
            if (dialogSourceLocation.TravelLocation.ResolveLocation(sharedKnowledge, out fromSite, out fromAllegiance, out fromExpedition, out fromTerminal))
            {
                // retrieve from TerminalContainer, always visible
                if (fromTerminal != null)
                {
                    sellerID = ((IOwner)fromExpedition).ID;

                    // filter parts, unfinished items and so on...
                    foreach (var item in fromTerminal.OfferedEntitiesByType)
                    {
                        List<EntityID> listOfEntities = GetValidItemsForSale(fromTerminal, sellerID, sharedKnowledge, item.Value);


                        if (listOfEntities != null)
                        {
                            wares[item.Key] = listOfEntities;
                        }

                    }

                    return wares;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }*/

        private static List<EntityID> GetValidItemsForSale(IKnownEntityData fromTerminal, OwnerID sellerID, SharedKnowledge sharedKnowledge, List<EntityID> items, Expedition buyingExpedition)
        {
            List<EntityID> listOfEntities = null;
                       

            for (int i = items.Count - 1; i >= 0; i--)
            {
                EntityID entityID = items[i];

                if (BuySellActionTemplate.ItemIsValidForSale(sharedKnowledge, entityID, fromTerminal, sellerID, buyingExpedition))
                {
                    Common.AddToList(ref listOfEntities,
                        entityID);
                }
            }
            return listOfEntities;
        }

        private List<IKnownEntityData> GetPeople()
        {
            TravelLocation? travelLocation = null;
            if (dialogSourceLocation != null)
            {
                travelLocation = dialogSourceLocation.TravelLocation;
            }

            return GetPeople(travelLocation);
        }

        public static List<IKnownEntityData> GetPeople(TravelLocation? travelLocation)
        {
            Site fromSite;
            Allegiance fromAllegiance;
            Expedition fromExpedition;
            IKnownEntityData fromTerminal;

            List<IKnownEntityData> result = null;

            if (travelLocation != null
                && travelLocation.Value.ResolveLocation(The.InGameUI.UIAllegiance.SharedKnowledge, out fromSite, out fromAllegiance, out fromExpedition, out fromTerminal))
            {
                if (fromExpedition != null)
                {
                    IKnownEntityData entityData;
                    foreach (var item in fromExpedition.IndependentMembers)
                    {
                        if (!GoalEvaluator.EntityDataResultCausesSkip(The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(item, out entityData)))
                        {
                            Common.AddToList(ref result, entityData);
                        }
                    }

                    return result;
                    // return fromAllegiance.MembersList.Select(e => e.ID).ToList();   
                }
                else
                {
                    return null;
                }

                /*
                if (fromAllegiance != null)
                {
                    return fromAllegiance.MembersList.Select(e => (IKnownEntityData)e).ToList();
                    // return fromAllegiance.MembersList.Select(e => e.ID).ToList();   
                }
                else
                {
                    return null;
                }*/
            }
           
            return null;
        }


        void WorldMapDialog_OKClick(object sender, EventArgs e)
        {
            ResetAfterWorldMapDialog();

            if (start != null && destination != null)
            {
                surfaceGrid.Clear();

                if (worldMapDialogSource == WorldMapDialogSource.Start)
                {
                    start = The.InGameUI.WorldMapDialog.SelectedTravelLocation;
                }
                else
                {
                    destination = The.InGameUI.WorldMapDialog.SelectedTravelLocation;
                }

                if (!PopulateTransportation())
                    return;

                CreateMissionStartingLocation();

                CreateMissionDestinationAndReturnDestination();
            }
            else
            {
                if (worldMapDialogSource == WorldMapDialogSource.Start)
                {
                    start = The.InGameUI.WorldMapDialog.SelectedTravelLocation;

                    if (!PopulateTransportation())
                        return;

                    CreateMissionStartingLocation();

                }
                else
                {
                    destination = The.InGameUI.WorldMapDialog.SelectedTravelLocation;

                    CreateMissionDestinationAndReturnDestination();
                }
            }

            ShowHomeIcon();

            UpdateTotalCost();


            Revalidate();

        }


        private bool CanTrade(EntityType entityType, out TierOrAreaType tierPolicy)
        {
            Expedition expedition = GetHomeExpedition();

            return InGameInterface.CanTrade(expedition, entityType, out tierPolicy);
        }

        private Expedition GetHomeExpedition()
        {
            if ((AllegianceID)start.Value.AllegianceID == The.InGameUI.UIAllegiance.ID)
            {
                return LookUp<Expedition, ExpeditionID>.FindByID((ExpeditionID)start.Value.ExpeditionID);                

            }
            else if (destination != null
                && (AllegianceID)destination.Value.AllegianceID == The.InGameUI.UIAllegiance.ID)
            {
                return LookUp<Expedition, ExpeditionID>.FindByID((ExpeditionID)destination.Value.ExpeditionID); 
            }

            return null;
        }

        private void ShowHomeIcon()
        {
            if ((AllegianceID)start.Value.AllegianceID == The.InGameUI.UIAllegiance.ID)
            {
                iconHomeStart.Visible = true;
                iconHomeDestination.Visible = false;
                iconHomeStart.X = lblStartingLocation.Right;
               
            }
            else if (destination != null
                && (AllegianceID)destination.Value.AllegianceID == The.InGameUI.UIAllegiance.ID)
            {
                iconHomeDestination.Visible = true;
                iconHomeStart.Visible = false;
                iconHomeDestination.X = lblDestination.Right;               
            }
            else
            {
                iconHomeStart.Visible = false;
                iconHomeDestination.Visible = false;
            }
        }

        private void CreateMissionStartingLocation()
        {
            surfaceGrid.BeginAddingEntries();

            // update mission data:
            CreateStartingLocation();

            // next update the UI to show the new data:
            PopulateMissionLocations();

            surfaceGrid.EndAddingEntries();
        }

        private void CreateMissionDestinationAndReturnDestination()
        {
            // update destination data:
            CreateMissionDestination();

            CreateReturnDestination();

            // next update the UI to show the new data:
            PopulateMissionLocations();

            PopulateTransportation();

            Mission.SelectRoutes();

           
        }

        void WorldMapDialog_CancelClick(object sender, EventArgs e)
        {
            ResetAfterWorldMapDialog();
        }

        void BuySellDialog_CancelClick(object sender, EventArgs e)
        {

            ResetAfterBuySellDialog();
        }

        void BuySellDialog_OKClick(object sender, EventArgs e)
        {
            BuySellPanel dialog = The.InGameUI.BuySellDialog;

            if (dialog.Orders != null) // && dialog.Orders.Count > 0)
            {
                // create the MissionAction to buy or sell:
                MissionActionTemplate newAction = null;

                // create or edit?
                bool isCreating = false;
                if (dialogSourceActionTemplate == null)
                {
                    isCreating = true;
                }

                switch (dialogSourceAction)
                {
                    case CargoActionTypes.Buy:
                        {
                            // The Buyer (who will own the items) should be the UI Expedition, not the expedition that the vehicle returns to (because we can hire vehicles).
                            // make the mission owner the buyer instead???
                            Expedition buyingExpedition = LookUp<Expedition, ExpeditionID>.FindByID(The.InGameUI.UIExpedition);

                            // the Seller is the expedition at the MissionStop                    
                            Site site;
                            Allegiance allegiance;
                            Expedition sellingExpedition;
                            IKnownEntityData terminal;
                            if (dialogSourceLocation.TravelLocation.ResolveLocation(The.InGameUI.UIAllegiance.SharedKnowledge, out site, out allegiance, out sellingExpedition, out terminal))
                            {
                                newAction = CreateOrModifyBuySellActions(dialog, newAction, isCreating, buyingExpedition, sellingExpedition);
                            }
                            else
                            {
                                ResetAfterBuySellDialog();
                                HandleDestroyedMissionStop();
                                return;
                            }

                            break;
                        }
                    case CargoActionTypes.Sell:
                        {
                          
                            // NEW: The Seller is the expedition at this MissionStop.
                            // The Buyer is any different expedition on the route <- this will fail for more waypoints, then the buyer will have to be chosen explicitly

                            Site site;
                            Allegiance allegiance;
                           // Expedition expedition;
                            IKnownEntityData terminal;
                            Expedition buyingExpedition = null, sellingExpedition = null;


                            // the Seller is the expedition at the MissionStop     
                            if (!dialogSourceLocation.TravelLocation.ResolveLocation(The.InGameUI.UIAllegiance.SharedKnowledge, out site, out allegiance, out sellingExpedition, out terminal))
                            {                                  
                                ResetAfterBuySellDialog();
                                HandleDestroyedMissionStop();
                                return;
                            }

                            buyingExpedition = Mission.GetExpeditionMatchingPredicate(exp => exp != sellingExpedition);

                            if (buyingExpedition != null)
                            {
                                newAction = CreateOrModifyBuySellActions(dialog, newAction, isCreating, buyingExpedition, sellingExpedition);
                            }
                            else
                            {
                                ResetAfterBuySellDialog();
                                return;
                            }
                            
                            break;

                            /*
                            // OLD: The Buyer is the expedition the vehicle returns to <- This fails if the player expedition is the start!
                            // The Seller is the expedition owning the goods...

                            Site site;
                            Allegiance allegiance;
                            Expedition expedition;
                            IKnownEntityData terminal;
                            Expedition buyingExpedition = null, sellingExpedition = null;

                            TravelLocation? returnDestination = GetReturnDestination();
                            if (returnDestination != null &&
                                returnDestination.Value.ResolveLocation(The.InGameUI.UIAllegiance.SharedKnowledge, out site, out allegiance, out expedition, out terminal))
                            {
                                buyingExpedition = expedition;
                            }
                            else
                            {
                                ResetAfterBuySellDialog();
                                HandleDestroyedMissionStop();
                                return;
                            }

                            if (buyingExpedition == expedition)
                            {
                                throw new Exception();
                            }

                            // the Seller is the expedition at the MissionStop     
                            if (dialogSourceLocation.TravelLocation.ResolveLocation(The.InGameUI.UIAllegiance.SharedKnowledge, out site, out allegiance, out sellingExpedition, out terminal))
                            {
                                newAction = CreateOrModifyBuySellActions(dialog, newAction, isCreating, buyingExpedition, sellingExpedition);
                            }
                            else
                            {
                                ResetAfterBuySellDialog();
                                HandleDestroyedMissionStop();
                                return;
                            }

                            break;*/
                        }

                    case CargoActionTypes.Load:
                        {
                            newAction = new LoadActionTemplate(dialogSourceLocation, dialog.Orders, true);

                            break;
                        }

                }


                if (newAction != null)
                {
                    Mission.AddAction(newAction, dialogSourceLocation);
                }
            }
            else
            {
                // an empty order means we should remove the buy/sell action?


            }


            // repopulate from the data source:
            RepopulateAfterAddedAction();

            // reset
            ResetAfterBuySellDialog();

        }

        private MissionActionTemplate CreateOrModifyBuySellActions(BuySellPanel dialog, MissionActionTemplate newAction, bool isCreating, Expedition buyingExpedition, Expedition sellingExpedition)
        {
            if (isCreating)
            {
                if (dialog.Orders != null && dialog.Orders.Count > 0)
                {
                    // create:
                    newAction = new BuySellActionTemplate(Mission, dialogSourceLocation, dialog.Orders, true, null,
                                                        ((IOwner)buyingExpedition).ID,
                                                        ((IOwner)sellingExpedition).ID, true);
                }
            }
            else
            {
                // update:
                BuySellActionTemplate existingAction = dialogSourceActionTemplate as BuySellActionTemplate;
                if (dialog.Orders != null)
                {
                    if (dialog.Orders.Any(k => k.Value.Count > 0))
                    {
                        // update existing order
                        existingAction.ContractTemplate.Entities = new SimSide.XmlCollections.SerializableDictionary<string, List<long>>(dialog.Orders.ToDictionary(k => k.Key.KeyName, k => k.Value.Select(e => (long)e).ToList()));
                    }
                    else
                    {
                        // delete action if no orders
                        existingAction.MissionStopTemplate.RemoveAction(existingAction);
                    }

                    /*if (dialog.Orders.Any(k => k.Value > 0))
                    {
                        // update existing order
                        existingAction.ContractTemplate.Goods = new SimSide.XmlCollections.SerializableDictionary<string, int>(dialog.Orders.ToDictionary(k => k.Key.KeyName, k => k.Value));
                    }
                    else
                    {
                        // delete action if no orders
                        existingAction.MissionStopTemplate.RemoveAction(existingAction);
                    }*/
                }

            }
            return newAction;
        }

        private void RepopulateAfterAddedAction()
        {
            UIComponent itemRow;
            surfaceGrid.TryGetEntry(dialogSourceLocation, out itemRow);

           /* UpdateMissionLocationRow(itemRow, dialogSourceLocation);

            PopulateLocationActions(dialogSourceLocation);
            */

            // NEW populate travel action rows also:
            PopulateMissionLocations();

            UpdateTotalCost();

            Revalidate();
        }

        private void ResetAfterWorldMapDialog()
        {
            WorldMapDialog dialog = The.InGameUI.WorldMapDialog;

            dialog.OKClick -= new EventHandler(WorldMapDialog_OKClick);
            dialog.CancelClick -= new EventHandler(WorldMapDialog_CancelClick);

            RemoveModalOverlay();
            /*
            dialogSourceLocation = null;
            dialogSourceAction = null;
            dialogSourceActionTemplate = null;*/
        }

        private void ResetAfterBuySellDialog()
        {
            BuySellPanel dialog = The.InGameUI.BuySellDialog;

            dialog.OKClick -= new EventHandler(BuySellDialog_OKClick);
            dialog.CancelClick -= new EventHandler(BuySellDialog_CancelClick);

            dialogSourceLocation = null;
            dialogSourceAction = null;
            dialogSourceActionTemplate = null;

            RemoveModalOverlay();
        }

        private void ResetAfterPersonnelDialog()
        {
            PersonnelDialog dialog = The.InGameUI.PersonnelDialog;

            dialog.OKClick -= new EventHandler(personnelDialog_OKClick);
            dialog.CancelClick -= new EventHandler(personnelDialog_CancelClick);

            dialogSourceLocation = null;
            dialogSourceAction = null;
            dialogSourceActionTemplate = null;

            RemoveModalOverlay();
        }

       


        const int collapsedPanelContentHeight = 119;

        private void AddSelectionPanel()
        {
            int captionWidth = 109;

            selectionPanel = new LCDInnerPanel(Interface.gui, lcdSurface.Width, true, 1f);
            lcdSurface.Add(selectionPanel.Panel);
            // surfaceGrid.AddEntry(selectionPanel.Panel, selectionPanel.Panel);
            selectionPanel.ContentHeight = collapsedPanelContentHeight;
            selectionPanel.Panel.Y = 0; // 24; 
            selectionPanel.VerticalContentPadding = innerPanelVerticalPadding;
            // selection panel is given a low number to stay at the top when the grid gets sorted:
            // selectionPanel.Panel.OrderByTag1 = -1000;


            Label lbl;

            lbl = new Label(Interface.gui);
            selectionPanel.AddContent(lbl);
            lbl.Init(topLeftCaptionLabelStyle);
            lbl.Text = "START:"; // "FROM:";
            // lbl.Position = new Point(labelXPos, 0);
            lbl.Width = captionWidth;
            //lbl.FitToText();

            ImageButton btStart = new ImageButton(Interface.gui);
            btStart.InitWithIcon(ImageButtonType.LCD, "globe_icon", false);
            selectionPanel.AddContent(btStart);
            btStart.Width = 35;
            btStart.Height = 44;
            btStart.ToolTip = "Select the starting point for the mission";
            btStart.X = controlXPos; // cbTransportation.Right + 2;
            lbl.AlignVertically(btStart);
            btStart.Click += new ClickHandler(btStart_Click); //new ClickHandler(cbFrom_SelectionChanged);
            btStart.RecalculateIconPosition();

            lbl = new Label(Interface.gui);
            selectionPanel.AddContent(lbl);
            lbl.Init(topLeftCaptionLabelStyle);
            lbl.Text = "DESTINATION:"; // "TO:";            
            lbl.Y = btStart.Bottom + SingleSpacing;
            lbl.Width = captionWidth;
            //lbl.FitToText();

            ImageButton btDestination = new ImageButton(Interface.gui);
            btDestination.InitWithIcon(ImageButtonType.LCD, "globe_icon", false);
            selectionPanel.AddContent(btDestination);
            btDestination.Width = 35;
            btDestination.Height = 44;
            btDestination.ToolTip = "Select the destination point for the mission";
            btDestination.X = controlXPos; // cbTransportation.Right + 2;
            lbl.AlignVertically(btDestination);
            btDestination.Click += new ClickHandler(btDestination_Click); //new ClickHandler(cbFrom_SelectionChanged);
            btDestination.RecalculateIconPosition();

            lbl = new Label(Interface.gui);
            selectionPanel.AddContent(lbl);
            lbl.Init(topLeftCaptionLabelStyle);
            lbl.Text = "TRANSPORT:";
            lbl.Y = btDestination.Bottom + DoubleSpacing;
            lbl.Width = captionWidth;
            //lbl.FitToText();

            cbTransportation = new ComboBox(Interface.gui, ListBoxType.LCDCombo, false);
            selectionPanel.AddContent(cbTransportation);
            cbTransportation.Init(ComboBoxTypes.LCD);
            cbTransportation.X = controlXPos;
            lbl.AlignVertically(cbTransportation);
            cbTransportation.Width = controlWidth;
            cbTransportation.SelectionChanged += new SelectionChangedHandler(cbTransportation_SelectionChanged);



            tbTransportMore = new TextButton(Interface.gui);
            tbTransportMore.Init(TextButton.TextButtonType.LCD);
            selectionPanel.AddContent(tbTransportMore);
            tbTransportMore.Text = "MORE";
            tbTransportMore.Width = 70;
            tbTransportMore.X = cbTransportation.Right + 2;
            lbl.AlignVertically(tbTransportMore);
            tbTransportMore.Click += new ClickHandler(tbTransportMore_Click);

            // lblTransportNotes = new ErrorsAndMessages(selectionPanel.Panel /* lcdSurface*/, Window.guiManager, tbTransportMore.Right + 5, cbTransportation.Y+6); 

            lblTransportNotesOrCost = new Label(Window.guiManager);
            selectionPanel.AddContent(lblTransportNotesOrCost);
            lblTransportNotesOrCost.Init(Label.LabelType.LCDNormal);
            lblTransportNotesOrCost.X = tbTransportMore.Right + 5;
            lblTransportNotesOrCost.Y = cbTransportation.Y + 6;
            lblTransportNotesOrCost.Visible = true;
            lblTransportNotesOrCost.TooltipExpires = false;

            exclamationTransport = new ExclamationMarkInACircle(Window.guiManager);
            selectionPanel.AddContent(exclamationTransport);
            exclamationTransport.X = tbTransportMore.Right + 5;
            exclamationTransport.Y = cbTransportation.Y + 6;
            exclamationTransport.Visible = false;


            /*
            lblTransportNotesErrorMarker = new Label(Window.guiManager);
            selectionPanel.AddContent(lblTransportNotesErrorMarker);
            lblTransportNotesErrorMarker.Init(Label.LabelType.LCDNormal);
            lblTransportNotesErrorMarker.X = tbTransportMore.Right + 5;
            lblTransportNotesErrorMarker.Y = cbTransportation.Y + 6;
            lblTransportNotesErrorMarker.NormalColor = Color.Red;
            lblTransportNotesErrorMarker.Text = "!";
            lblTransportNotesErrorMarker.Visible = false;
            */



            Image columnDividerHorizontal = new Image(Interface.gui);
            selectionPanel.AddContent(columnDividerHorizontal);
            columnDividerHorizontal.SetSkinLocation(SkinState.Normal,Interface.gui.GUISpriteSheet.GetSourceRectangle("basic_line"));
            columnDividerHorizontal.X = 5;
            columnDividerHorizontal.Y = cbTransportation.Y - 7;
            columnDividerHorizontal.Width = 561;
            columnDividerHorizontal.Height = 2;
            columnDividerHorizontal.ScaleImageToSizeOfControl = true;


            lblSupplyNeedsHeader = new Label(Interface.gui);
            lblSupplyNeedsHeader.Init(topLeftCaptionLabelStyle);
            selectionPanel.AddContent(lblSupplyNeedsHeader);
            lblSupplyNeedsHeader.Text = "SUPPLY NEEDS";
            lblSupplyNeedsHeader.Width = labelWidth;
            lblSupplyNeedsHeader.X = 160;
            lblSupplyNeedsHeader.Y = cbTransportation.Bottom + 6;

            lblSupplyNeeds = new Label(Interface.gui);
            lblSupplyNeeds.Init(WindowSystem.Label.LabelType.LCDNormal);
            selectionPanel.AddContent(lblSupplyNeeds);
            lblSupplyNeeds.Text = "N/A";
            lblSupplyNeeds.Width = labelWidth;
            lblSupplyNeeds.X = lblSupplyNeedsHeader.X;
            lblSupplyNeeds.Y = lblSupplyNeedsHeader.Bottom + 2;

            lblCostHeader = new Label(Interface.gui);
            lblCostHeader.Init(topLeftCaptionLabelStyle);
            selectionPanel.AddContent(lblCostHeader);
            lblCostHeader.Width = labelWidth;

            lblCostHeader.X = lblSupplyNeedsHeader.Right + 14;
            lblCostHeader.Y = cbTransportation.Bottom + 6;
            lblCostHeader.Text = "COST / KM";
            lblCost = new Label(Interface.gui);
            lblCost.Init(WindowSystem.Label.LabelType.LCDNormal);
            selectionPanel.AddContent(lblCost);
            lblCost.Width = labelWidth;
            lblCost.X = lblCostHeader.X;
            lblCost.Y = lblCostHeader.Bottom + 2;


            
            //***** moved from bottom part:
            int topOfBottomPart = 9; // lcdSurface.Height - staticBottomHeight;

            // on the left side:
            //  errorAndMessageStart = new ErrorsAndMessages(lcdSurface, Window.guiManager, 161, topOfBottomPart, Label.LabelType.LCDHeadingBlue); //below scrollable area         

            lblStartingLocation = new Label(Window.guiManager);
            selectionPanel.AddContent(lblStartingLocation);
            lblStartingLocation.Init(Label.LabelType.LCDHeadingBlue);
            lblStartingLocation.X = 161;
            lblStartingLocation.Y = topOfBottomPart;
            lblStartingLocation.Visible = false;

            iconHomeStart = CreateHomeIcon();
            iconHomeStart.Y = lblStartingLocation.Y;
            /*
                        lblStartErrorMarker = new Label(Window.guiManager);
                        selectionPanel.AddContent(lblStartErrorMarker);
                        lblStartErrorMarker.Init(Label.LabelType.LCDNormal);
                        lblStartErrorMarker.X = 161;
                        lblStartErrorMarker.Y = topOfBottomPart;
                        lblStartErrorMarker.NormalColor = Color.Red;
                        lblStartErrorMarker.Text = "!";
                        lblStartErrorMarker.Visible = false;*/


            exclamationMarkStart = new ExclamationMarkInACircle(Window.guiManager);
            selectionPanel.AddContent(exclamationMarkStart);
            exclamationMarkStart.X = 161;
            exclamationMarkStart.Y = topOfBottomPart + 3;
            exclamationMarkStart.Visible = false;



            //  errorAndMessageDestination = new ErrorsAndMessages(lcdSurface, Window.guiManager, 161, btStart.Bottom + SingleSpacing, Label.LabelType.LCDHeadingBrown); //below scrollable area

            lblDestination = new Label(Window.guiManager);
            selectionPanel.AddContent(lblDestination);
            lblDestination.Init(Label.LabelType.LCDHeadingBrown);
            lblDestination.X = 161;
            lblDestination.Y = btStart.Bottom + SingleSpacing;
            lblDestination.Visible = false;

            exclamationMarkDestination = new ExclamationMarkInACircle(Window.guiManager);
            selectionPanel.AddContent(exclamationMarkDestination);
            exclamationMarkDestination.X = 161;
            exclamationMarkDestination.Y = btStart.Bottom + SingleSpacing + 4;
            exclamationMarkDestination.Visible = false;

            iconHomeDestination = CreateHomeIcon();
            iconHomeDestination.Y = lblDestination.Y;


            /*
            lblDestinationErrorMarker = new Label(Window.guiManager);
            selectionPanel.AddContent(lblDestinationErrorMarker);
            lblDestinationErrorMarker.Init(Label.LabelType.LCDNormal);
            lblDestinationErrorMarker.X = 161;
            lblDestinationErrorMarker.Y = btStart.Bottom + SingleSpacing;
            lblDestinationErrorMarker.NormalColor = Color.Red;
            lblDestinationErrorMarker.Text = "!";
            lblDestinationErrorMarker.Visible = false;*/





            // on the right side:
            Label lblCargoCaption = new Label(Window.guiManager);
            lcdSurface.Add(lblCargoCaption);
            lblCargoCaption.Init(totalCaptionLabelStyle);
            lblCargoCaption.Y = topOfBottomPart;
            lblCargoCaption.X = totalCaptionColumnX; // 0;
            lblCargoCaption.Text = "TOTAL CARGO:";
            lblCargoCaption.ToolTip = "Total bulk of the cargo";

            lblTotalCargoBulk = new Label(Window.guiManager);
            lcdSurface.Add(lblTotalCargoBulk);
            lblTotalCargoBulk.Init(Label.LabelType.LCDNormal);
            lblTotalCargoBulk.Y = lblCargoCaption.Y;
            lblTotalCargoBulk.X = totalColumnX; // 0;

            lblAvailableCargoBulk = new Label(Window.guiManager);
            lcdSurface.Add(lblAvailableCargoBulk);
            lblAvailableCargoBulk.Init(Label.LabelType.LCDNormal);
            lblAvailableCargoBulk.Y = lblCargoCaption.Y;
            lblAvailableCargoBulk.X = lblTotalCargoBulk.Right + SingleSpacing;

            Label lblCostCaption = new Label(Window.guiManager);
            lcdSurface.Add(lblCostCaption);
            lblCostCaption.Init(totalCaptionLabelStyle);
            lblCostCaption.Y = btStart.Bottom + SingleSpacing;
            lblCostCaption.X = totalCaptionColumnX; // 0;
            lblCostCaption.Text = "TOTAL COST:";
            lblCostCaption.ToolTip = "Total expenses (in trade credits) for starting this run. A negative number here means we will earn credits.";

            lblTotalMissionCost = new Label(Window.guiManager);
            lcdSurface.Add(lblTotalMissionCost);
            lblTotalMissionCost.Init(Label.LabelType.LCDNormal);
            lblTotalMissionCost.Y = lblCostCaption.Y;
            lblTotalMissionCost.X = totalColumnX; // 0;
            lblTotalMissionCost.Height = lblCostCaption.Height;
            lblTotalMissionCost.Visible = false;
            lblTotalMissionCost.TooltipExpires = false;

            lblTotalTradingCredits = new Label(Window.guiManager);
            lcdSurface.Add(lblTotalTradingCredits);
            lblTotalTradingCredits.Init(Label.LabelType.LCDNormal);
            lblTotalTradingCredits.Y = lblCostCaption.Y;
            lblTotalTradingCredits.X = lblTotalMissionCost.Right + SingleSpacing;



            Image columnDividerVertical = new Image(Interface.gui);
            selectionPanel.AddContent(columnDividerVertical);
            columnDividerVertical.SetSkinLocation(SkinState.Normal,Interface.gui.GUISpriteSheet.GetSourceRectangle("basic_line"));
            columnDividerVertical.X = lblCostCaption.X - 5;
            columnDividerVertical.Y = 5;
            columnDividerVertical.Width = 2;
            columnDividerVertical.Height = 72;
            columnDividerVertical.ScaleImageToSizeOfControl = true;
        }

        private Image CreateHomeIcon()
        {
            Image icon = new Image(Window.guiManager);
            icon.SetSkinLocation(SkinState.Normal,Window.guiManager.GUISpriteSheet.GetSourceRectangle("HUD_icon_structure"), Color.Green, Color.Green);
            icon.ResizeControlToFitImage();
            icon.ToolTip = "This is where we are"; //MP was confusing: "Home base"
            icon.Visible = false;
            lcdSurface.Add(icon);

            return icon;
        }


        void btStart_Click(UIComponent sender, EventArgs e)
        {
            worldMapDialogSource = WorldMapDialogSource.Start;
            ShowWorldMapDialog(sender.AbsolutePosition);
        }

        void btDestination_Click(UIComponent sender, EventArgs e)
        {
            if (Mission.StartMissionStopTemplate != null)
            {
                worldMapDialogSource = WorldMapDialogSource.End;
                ShowWorldMapDialog(sender.AbsolutePosition);
            }
            else
            {
                ShowError("Select a starting point first.", null, FieldError.Start);
            }
        }

        bool transportIsExpanded = false;
        void tbTransportMore_Click(UIComponent sender, EventArgs e)
        {
            if (transportIsExpanded)
            {
                // collapse
                transportIsExpanded = false;
                tbTransportMore.Text = "MORE";

                selectionPanel.ContentHeight = collapsedPanelContentHeight - 4; // 
            }
            else
            {
                // expand
                transportIsExpanded = true;
                tbTransportMore.Text = "LESS";

                selectionPanel.ContentHeight = 162;
            }


            UpdateMissionStopGridYPosAndHeight();
        }

        private class TransportationType
        {
            public EntityType VehicleType;
        }

        void cbTransportation_SelectionChanged(UIComponent sender)
        {
            //object from = start;

            TravelLocation fromLocation = start.Value; // from as TravelLocation;
            Site fromSite;
            Allegiance fromAllegiance;
            Expedition fromExpedition;
            IKnownEntityData fromTerminal;

            if (!fromLocation.ResolveLocation(The.InGameUI.UIAllegiance.SharedKnowledge, out fromSite, out fromAllegiance, out fromExpedition, out fromTerminal))
            {
                HandleDestroyedMissionStop();
                return;
            }


            // OutputTransportNotes(fromExpedition);

          
            Mission.TransportationType = new TransportationTemplate();

            EntityType chosenVehicleType = ((TransportationType)cbTransportation.SelectedKey).VehicleType;

            if (chosenVehicleType != null)
            {
                Mission.TransportationType.Vehicles = new List<Pair<string, int>>() { new Pair<string, int>(chosenVehicleType.KeyName, 1) };
            }
            

            // set routes (if possible!)
            if (!Mission.SelectRoutes())
            {
                HandleDestroyedMissionStop();
                return;
            }

            if (VehiclesAreHired(fromExpedition))
            {
                Mission.TransportationType.HiredFromOwner = (long)(((IOwner)fromExpedition).ID);

                // missionTemplate.TransportsAreHired = true;


                /* Expedition buyingExpedition = LookUp<Expedition, ExpeditionID>.FindByID(The.InGameUI.UIExpedition);
                 missionTemplate.Payer = (long)(((IOwner)buyingExpedition).ID); */
            }
            else
            {
                Mission.TransportationType.HiredFromOwner = null;
                //missionTemplate.TransportsAreHired = false;
            }

            Expedition expedition = LookUp<Expedition, ExpeditionID>.FindByID((ExpeditionID)destination.Value.ExpeditionID);
            EntityGroup owner = expedition.OwnedEntities;

            if (selectionPanel.Panel.FindChildById(UIComponent.DataControlID.Name) != null)
            {
                selectionPanel.Panel.Remove(selectionPanel.Panel.FindChildById(UIComponent.DataControlID.Name));
            }
            DataTypeButton dtCaption = new DataTypeButton(Interface.gui, HUD_Windows.DataSheet.InfoToShow.Production, chosenVehicleType, GoalEvaluator.GetOwnerID(owner),
                                                          false);

            dtCaption.Init(TextButton.TextButtonType.LCDToolTipBlack);
            dtCaption.ID = UIComponent.DataControlID.Name;
            selectionPanel.AddContent(dtCaption);
            dtCaption.IsRoot = true;
          //  dtCaption.Text = chosenVehicleType.PluralName;
            dtCaption.TextAlignment = TextButton.TextAlign.Left;
            dtCaption.X = 10;
            dtCaption.Width = lblSupplyNeedsHeader.X - dtCaption.X - 5;
            dtCaption.Y = lblSupplyNeedsHeader.Y + 2;

            OutputTransportNotes(fromExpedition);

            UpdateTotalCost();

            // next update the UI to show the new data:
            PopulateMissionLocations();


            Revalidate();
        }


        private bool VehiclesAreHired(Expedition fromExpedition)
        {
            return fromExpedition != null && fromExpedition.Allegiance != The.InGameUI.UIAllegiance;
        }


        /*  void cbFrom_SelectionChanged(UIComponent sender)
          {
              surfaceGrid.BeginAddingEntries();

              if (!PopulateTransportation())
                  return;

             // ClearForm();

              // update mission data:
              CreateStartingLocation();


              // next update the UI to show the new data:
              PopulateMissionLocations();

              surfaceGrid.EndAddingEntries();
          }*/

        /*
        void cbTo_SelectionChanged(UIComponent sender)
        {
          
            // update destination data:
            CreateMissionDestination();

            CreateReturnDestination();

            // next update the UI to show the new data:
            PopulateMissionLocations(); 

            PopulateTransportation();
        }*/


        /// <summary>
        /// the Create functions only add to the data source, not to the UI. that is done in a call to Populate
        /// </summary>
        private void CreateStartingLocation()
        {
            if (Mission.StartMissionStopTemplate != null)
            {
                Mission.StartMissionStopTemplate.Destroy();
                Mission.StartMissionStopTemplate = null;
            }

            TravelLocation? fromLocation = start; // cbStart.SelectedKey as TravelLocation;

            // add a start location to the mission:
            Mission.StartMissionStopTemplate = new MissionStopTemplate(false); //Mission);
            Mission.StartMissionStopTemplate.TravelLocation = fromLocation.Value;

        }

        private void CreateMissionDestination()
        {
            // destroy the old temporary action:
            if (Mission.StartMissionStopTemplate != null)
            {
                if (Mission.StartMissionStopTemplate.TravelAction != null)
                {
                    Mission.StartMissionStopTemplate.TravelAction.Destroy();
                    Mission.StartMissionStopTemplate.TravelAction = null;
                }
            }

            if (destination != null) // cbDestination.SelectedIndex > -1)
            {
                MissionStopTemplate destinationTemplate = new MissionStopTemplate(false); //Mission);
                destinationTemplate.TravelLocation = destination.Value; // (TravelLocation)cbDestination.SelectedKey;

                destinationTemplate.Actions.Enqueue(new UnloadActionTemplate(destinationTemplate, false)); // unload here by default
                destinationTemplate.Actions.Enqueue(new DisembarkActionTemplate(destinationTemplate, false)); // disembark here by default

                //  destinationTemplate.IsLocked = true;


                Mission.AddMissionLocation(destinationTemplate);
            }
        }


        private TravelLocation? GetReturnDestination()
        {
            if (Mission.StartMissionStopTemplate != null
                && Mission.StartMissionStopTemplate.TravelAction != null)
            {
                return Mission.StartMissionStopTemplate.TravelAction.ToMissionStop.TravelAction.ToMissionStop.TravelLocation;
            }

            return null;
        }

       

        private void CreateReturnDestination()
        {
            // this logic will have to be replaced when we implement longer routes
            if (Mission.StartMissionStopTemplate != null
                && Mission.StartMissionStopTemplate.TravelAction != null)
            {
                // delete existing return destination:
                if (Mission.StartMissionStopTemplate.TravelAction.ToMissionStop.TravelAction != null)
                {
                    Mission.StartMissionStopTemplate.TravelAction.ToMissionStop.TravelAction.Destroy();
                }

                if (start != null) // cbStart.SelectedIndex > -1)
                {
                    MissionStopTemplate destinationTemplate = new MissionStopTemplate(false);
                    destinationTemplate.TravelLocation = new TravelLocation(start.Value); // (TravelLocation)cbStart.SelectedKey); // get the start, make a copy, set it as the destination

                    destinationTemplate.IsLocked = true;// NEW
                    destinationTemplate.Actions.Enqueue(new UnloadActionTemplate(destinationTemplate, false)); // unload here by default NEW
                    destinationTemplate.Actions.Enqueue(new DisembarkActionTemplate(destinationTemplate, false)); // disembark here by default NEW


                    Mission.AddMissionLocation(destinationTemplate);
                }
            }

        }

        private UIComponent AddTravelActionRow(TravelActionTemplate travelAction)
        {
            UIComponent itemRow = new UIComponent(Interface.gui);
            surfaceGrid.AddEntry(travelAction, itemRow);
            itemRow.OrderByTag1 = travelAction.MissionStopTemplate.Number * 2 + 1;
            itemRow.Height = collapsedTravelActionHeight;

            TextButton tbMore = new TextButton(Interface.gui);
            tbMore.Init(TextButton.TextButtonType.LCD);
            itemRow.Add(tbMore);
            tbMore.CheckedMode = CheckedModes.SwitchCheckedStateOnClick;
            //tbMore.HasState = true;           
            tbMore.Text = "SHOW";
            tbMore.Width = 70;
            tbMore.X = 99;
            tbMore.Y = 2;
            tbMore.Click += new ClickHandler(tbTravelActionMore_Click);
            // tbMore.ToolTip = "Show more details";
            tbMore.Tag1 = travelAction;
            tbMore.ID = UIComponent.DataControlID.Expand;

            Image imArrow = new Image(Interface.gui);
            itemRow.Add(imArrow);
            // cp.AddContent(imArrow);
            imArrow.SetSkinLocation(SkinState.Normal,Interface.gui.GUISpriteSheet.GetSourceRectangle("mission_arrow_small"));
            imArrow.ResizeControlToFitImage();
            imArrow.ID = UIComponent.DataControlID.SmallArrow;
            imArrow.X = 13;

            Image imBigArrow = new Image(Interface.gui);
            itemRow.Add(imBigArrow);
            imBigArrow.Y = 6;
            imBigArrow.X = -2;
            imBigArrow.SetSkinLocation(SkinState.Normal,Interface.gui.GUISpriteSheet.GetSourceRectangle("mission_arrow_big"));
            imBigArrow.ResizeControlToFitImage();
            imBigArrow.Visible = false;
            imBigArrow.ID = UIComponent.DataControlID.BigArrow;

            int captionWidth = 176;
            Label lblCaption, lblValue;

            int captionValueYDistance = -2;
            int captionX = 190;
            int secondRowCaptionY = 43;

            lblCaption = new Label(Interface.gui);
            itemRow.Add(lblCaption);
            lblCaption.Init(Label.LabelType.LCDSmallHeadingBanner);
            lblCaption.Text = "DISTANCE:";
            lblCaption.X = captionX;
            lblCaption.Y = 8;
            lblCaption.Width = captionWidth;
            lblCaption.Visible = false; // show when expanded only
            lblCaption.ID = UIComponent.DataControlID.DistanceCaption;


            Label lblCaptionValue = new Label(Interface.gui);
            itemRow.Add(lblCaptionValue);
            lblCaptionValue.Init(Label.LabelType.LCDNormal);
            lblCaptionValue.X = lblCaption.X + 2;
            lblCaptionValue.Y = lblCaption.Bottom + captionValueYDistance;
            lblCaptionValue.Width = captionWidth;
            lblCaptionValue.Visible = true;
            lblCaptionValue.ID = UIComponent.DataControlID.Distance;


            Label lblCapacity = new Label(Interface.gui);
            itemRow.Add(lblCapacity);
            lblCapacity.Init(Label.LabelType.LCDSmallHeadingBanner);
            lblCapacity.Text = "CAPACITY:";
            lblCapacity.X = lblCaption.Right + 5;// captionX;
            lblCapacity.Y = 8;
            lblCapacity.Width = captionWidth;
            lblCapacity.Visible = false; // show when expanded only
            lblCapacity.ID = UIComponent.DataControlID.CapacityCaption;

            Label lblCapacityValue = new Label(Interface.gui);
            itemRow.Add(lblCapacityValue);
            lblCapacityValue.Init(Label.LabelType.LCDNormal);
            lblCapacityValue.X = lblCapacity.X + 2;
            lblCapacityValue.Y = lblCapacity.Bottom + captionValueYDistance;
            lblCapacityValue.Width = captionWidth;
            lblCapacityValue.Visible = true;
            lblCapacityValue.ID = UIComponent.DataControlID.Capacity;


            Label lblTravelTime = new Label(Interface.gui);
            itemRow.Add(lblTravelTime);
            lblTravelTime.Init(Label.LabelType.LCDSmallHeadingBanner);
            lblTravelTime.Text = "TRAVEL TIME:";
            lblTravelTime.X = captionX;
            lblTravelTime.Y = secondRowCaptionY;
            lblTravelTime.Width = captionWidth;
            lblTravelTime.Visible = true; // stays visible
            lblTravelTime.ID = UIComponent.DataControlID.TravelTimeCaption;

            Label lblTravelTimeValue = new Label(Interface.gui);
            itemRow.Add(lblTravelTimeValue);
            lblTravelTimeValue.Init(Label.LabelType.LCDNormal);
            lblTravelTimeValue.X = lblTravelTime.X + 2;
            lblTravelTimeValue.Y = lblTravelTime.Bottom + captionValueYDistance;
            lblTravelTimeValue.Width = captionWidth;
            lblTravelTimeValue.Visible = true;
            lblTravelTimeValue.ID = UIComponent.DataControlID.TravelTime;


            Label lblFuelConsumption = new Label(Interface.gui);
            itemRow.Add(lblFuelConsumption);
            lblFuelConsumption.Init(Label.LabelType.LCDSmallHeadingBanner);
            lblFuelConsumption.Text = "FUEL CONSUMPTION:";
            lblFuelConsumption.X = lblCaption.Right + 5;
            lblFuelConsumption.Y = secondRowCaptionY;
            lblFuelConsumption.Width = captionWidth;
            lblFuelConsumption.Visible = true; // stays visible
            lblFuelConsumption.ID = UIComponent.DataControlID.FuelConsumptionCaption;

            Label lblFuelConsumptionValue = new Label(Interface.gui);
            itemRow.Add(lblFuelConsumptionValue);
            lblFuelConsumptionValue.Init(Label.LabelType.LCDNormal);
            lblFuelConsumptionValue.X = lblCapacity.X;// lblCaption.X + 2;
            lblFuelConsumptionValue.Y = lblTravelTime.Bottom + captionValueYDistance;
            lblFuelConsumptionValue.Width = captionWidth;
            lblFuelConsumptionValue.Visible = true;
            lblFuelConsumptionValue.Text = "N/A";
            lblFuelConsumptionValue.ID = UIComponent.DataControlID.FuelConsumption;

            return itemRow;

        }

        const int collapsedTravelActionHeight = 30;

        void tbTravelActionMore_Click(UIComponent sender, EventArgs e)
        {
            TextButton btExpand = sender as TextButton;
            // Job job = (Job)sender.Tag1;

            UIComponent itemRow;
            surfaceGrid.TryGetEntry(sender.Tag1, out itemRow);

            if (btExpand.IsChecked)
            {
                // expand

                itemRow.Height = 87;

            }
            else
            {
                // collapse
                btExpand.Text = "SHOW";
                itemRow.Height = collapsedTravelActionHeight;
            }

            UpdateTravelActionRow(itemRow, (TravelActionTemplate)sender.Tag1);

        }

        private UIComponent AddMissionLocationRow(MissionStopTemplate missionLocation)
        {

            Label lblStart = null;
            Label lblAllegianceAndExpedition = null;

            TravelLocation location = missionLocation.TravelLocation;

            UIComponent panel = AddMissionLocationPanel(missionLocation, ref lblStart, ref lblAllegianceAndExpedition);

            //   panel.Panel.Visible = true;

            Site fromSite;
            Allegiance fromAllegiance;
            Expedition fromExpedition;
            IKnownEntityData fromTerminal;

            if (!location.ResolveLocation(The.InGameUI.UIAllegiance.SharedKnowledge, out fromSite, out fromAllegiance, out fromExpedition, out fromTerminal))
            {
                HandleDestroyedMissionStop();
                return panel;
            }




            lblStart.Init(location.ExpeditionID == start.Value.ExpeditionID ? Label.LabelType.LCDHeadingBlue : Label.LabelType.LCDHeadingBrown);
            lblStart.Text = fromSite.Name.ToUpper()/*+ ", "+fromAllegiance.Name + ", " + fromExpedition.Name*/;
            lblStart.FitToText();


            lblAllegianceAndExpedition.Init(Label.LabelType.LCDNormal);
            lblAllegianceAndExpedition.X = lblStart.Right + 5;
            lblAllegianceAndExpedition.Text = fromAllegiance.Name + ", " + fromExpedition.Name;
            lblAllegianceAndExpedition.FitToText();


            PopulateLocationActions(missionLocation);

            // PopulateMissionActionsCombo(number == 0, cbAddAction, missionLocation, fromAllegiance, fromExpedition); 


            return panel;
        }


        private void UpdateTravelActionRow(UIComponent itemRow, TravelActionTemplate travelAction)
        {

            Image imArrow = (Image)itemRow.FindChildById(UIComponent.DataControlID.SmallArrow);
            Image imBigArrow = (Image)itemRow.FindChildById(UIComponent.DataControlID.BigArrow);

            Label lblDistanceCaption = (Label)itemRow.FindChildById(UIComponent.DataControlID.DistanceCaption);
            Label lblDistance = (Label)itemRow.FindChildById(UIComponent.DataControlID.Distance);

            Label lblCapacity = (Label)itemRow.FindChildById(UIComponent.DataControlID.CapacityCaption);
            Label lblCapacityValue = (Label)itemRow.FindChildById(UIComponent.DataControlID.Capacity);


            TextButton tbShow = (TextButton)itemRow.FindChildById(UIComponent.DataControlID.Expand);
            if (tbShow.IsChecked)
            {
                imArrow.Visible = false;
                imBigArrow.Visible = true;

                lblDistanceCaption.Visible = true;
                lblDistance.Visible = true;


                lblCapacity.Visible = true;
                lblCapacityValue.Visible = true;


                lblDistance.Text = Units.GetKilometersAsString(travelAction.Distance);


                Label lblTravelTime = (Label)itemRow.FindChildById(UIComponent.DataControlID.TravelTime);

                if (missionTemplate.TransportationType != null)
                {
                    lblTravelTime.Text = travelAction.GetTravelTime(missionTemplate).ToIntervalString(); // Distance);

                    PassengerListTemplate passengerListTemplate = missionTemplate.GetPassengerListTemplate();

                    if (passengerListTemplate == null)
                    {
                        lblCapacityValue.Text = Entity.GetBulkAsString(missionTemplate.ComputeTotalCargoBulk()) + " BULK";
                    }
                    else
                    {
                        lblCapacityValue.Text = passengerListTemplate.Passengers.Count() + " Passengers | " + Entity.GetBulkAsString(missionTemplate.ComputeTotalCargoBulk()) + " BULK";
                    }
                }
                else
                {
                    lblTravelTime.Text = "UNKNOWN";
                }


                // lblTravelTimeCaption.Visible = true;


                tbShow.Text = "HIDE";
                tbShow.ToolTip = "Collapse this part";
            }
            else
            {
                imArrow.Visible = true;
                imBigArrow.Visible = false;

                lblDistanceCaption.Visible = false;
                lblDistance.Visible = false;
                lblCapacity.Visible = false;
                lblCapacityValue.Visible = false;
                tbShow.Text = "SHOW";
                tbShow.ToolTip = "Expand to show more information";
            }
        }



        private void UpdateMissionLocationRow(UIComponent itemRow, MissionStopTemplate missionLocation) //, int number)
        {
            itemRow.OrderByTag1 = missionLocation.Number * 2;

            // update the main fields:        
            Label lblAlert = (Label)itemRow.FindChildById(UIComponent.DataControlID.ErrorsAndMessages);

            // bool showAddAction = true;

            // hide the action selector until a transport has been selected.
            if (Mission.TransportationType == null
                && !missionLocation.IsLocked)
            {
                lblAlert.Visible = true; // prompt the user to select a transportation first
                lblAlert.Text = "First select a transportation, then add actions to each stop.";
                lblAlert.FitToText();

                //addActionLabel.Visible = false;
                //   cbAddAction.Visible = false;
            }
            else
            {
                lblAlert.Visible = false;

                // show/hide combo based on locked status:
                /* if (missionLocation.IsLocked)
                 {
                     showAddAction = false;
                   //  addActionLabel.Visible = false;
                    // cbAddAction.Visible = false;
                 }
                 else
                 {
                     showAddAction = true;
                   //  addActionLabel.Visible = true;
                  //   cbAddAction.Visible = true;
                 }*/
            }

            // redraw the action list:
            PopulateLocationActions(missionLocation);

        }

        private void PopulateMissionLocations()
        {
            surfaceGrid.BeginAddingEntries();

            // call this recursively along the linked list of destinations:
            PopulateMissionLocation(missionTemplate.StartMissionStopTemplate);

            // sort:
            // selection panel is given a low number to stay at the top
            surfaceGrid.Sort(i => (int)(i.OrderByTag1 ?? 1000), Grid.Sorting.Ascending);

            // Cleanup (keep the other panel elements):
            surfaceGrid.DeleteEntriesWithNullableKey<MissionStopTemplate>(m => m == null || missionTemplate.ContainsLocation(m));


            surfaceGrid.EndAddingEntries();

        }

        private void PopulateTravelAction(TravelActionTemplate travelAction)
        {
            UIComponent itemRow;

            if (!surfaceGrid.TryGetEntry(travelAction, out itemRow))
            {
                itemRow = AddTravelActionRow(travelAction);
            }

            UpdateTravelActionRow(itemRow, travelAction);

        }

        private void PopulateMissionLocation(MissionStopTemplate missionLocation) // bool isStart)
        {
            UIComponent itemRow;

            if (!surfaceGrid.TryGetEntry(missionLocation, out itemRow))
            {
                itemRow = AddMissionLocationRow(missionLocation);
            }

            UpdateMissionLocationRow(itemRow, missionLocation);


            if (missionLocation.TravelAction != null)
            {
                PopulateTravelAction(missionLocation.TravelAction);

                // call this method recursively along the linked list of destinations:
                PopulateMissionLocation(missionLocation.TravelAction.ToMissionStop);
            }


            if (start != null)
            {
                lblStartingLocation.Visible = true;
                exclamationMarkStart.Visible = false;
                lblStartingLocation.Text = The.InGameUI.WorldMapDialog.worldMap.GetSiteName((SiteID)start.Value.SiteID);
            }
            if (destination != null)
            {
                lblDestination.Visible = true;
                exclamationMarkDestination.Visible = false;
                lblDestination.Text = The.InGameUI.WorldMapDialog.worldMap.GetSiteName((SiteID)destination.Value.SiteID);
            }
        }

        private void PopulateLocationActions(MissionStopTemplate location) //, bool showAddAction)
        {
            // Grid grdActions = (Grid)surfaceGrid.EntriesByKey[location].FindChildById(UIComponent.DataControlID.CurrentOrders);
            HorizontalList hlActions = (HorizontalList)surfaceGrid.EntriesByKey[location].FindChildById(UIComponent.DataControlID.CurrentOrders);

            Queue<MissionActionTemplate> actions = Mission.GetActionsAtLocation(location);

            bool enabled = !location.IsLocked;

            //   hlActions.Clear();
            hlActions.BeginAddingEntries();

            UIComponent itemRow;
            ImageButton btAction;
            if (actions != null)
            {
                foreach (var item in actions)
                {
                    int key = (int)item.ActionType;


                    if (!hlActions.TryGetEntry(key, out itemRow))
                    {
                        ClickHandler deleteAction = null;
                        if (item.AllowDeleting)
                        {
                            deleteAction = ibDelete_Click;
                        }

                        itemRow = AddAction(hlActions, key, item, item.ActionType, "Click to edit", enabled, deleteAction, Interface.gui, out btAction);
                        btAction.Click += new ClickHandler(tbAction_Click);
                    }

                    UpdateAction(itemRow, item, enabled);

                }
            }

            int newButtonKey = -1;

            // remove & re-add the NEW button to make it appear last.
            if (hlActions.TryGetEntry(newButtonKey, out itemRow))
            {
                hlActions.TryRemoveEntry(newButtonKey);
            }

            if (enabled) //showAddAction)
            {
                if (itemRow == null) // !hlActions.TryGetEntry(addButtonKey, out itemRow))
                {
                    AddCreateActionButton(hlActions, newButtonKey, location, true, out btAction);
                }
                else
                {
                    hlActions.AddEntry(newButtonKey, itemRow);
                }
            }
            else
            {
                // hlActions.TryRemoveEntry(addButtonKey);
            }


            // clean up unused actions:
            //hlActions.DeleteEntries<MissionActionTemplate>(r => r == null || actions.Contains(r));
            hlActions.DeleteEntries<int>(r => r == newButtonKey || actions.FirstOrDefault(a => (int)a.ActionType == r) != null); // .Contains(.Contains(r));

            //hlActions.Sort(Grid.Sorting.Descending, true, a => a == );

            hlActions.EndAddingEntries();
        }

        private void AddCreateActionButton(HorizontalList hlActions, object key, MissionStopTemplate missionStopTemplate, bool enabled, out ImageButton btAction)
        {
            UIComponent item = new UIComponent(Interface.gui);
            hlActions.AddEntry(key, item);

            btAction = new ImageButton(Interface.gui);
            item.Add(btAction);
            btAction.Init(ImageButtonType.AddAction);
            btAction.Tag1 = missionStopTemplate; // key;           
            btAction.Enabled = enabled;
            btAction.ScaleImageToSizeOfControl = false;
            btAction.Click += new ClickHandler(btAddAction_Click);

            item.Width = btAction.Width;
            item.Height = btAction.Height;

            Label lblName = new Label(Interface.gui);
            item.Add(lblName);
            lblName.Init(Label.LabelType.LCDNormal);
            lblName.Text = "ADD ACTION";
            lblName.X = 16;
            lblName.Y = 14;

        }


        /// <summary>
        /// add a cube for each Action - Buy, Sell, Load etc.
        /// </summary>
        /// <param name="hlActions"></param>
        /// <param name="action"></param>
        /// <param name="enabled"></param>
        public static UIComponent AddAction(HorizontalList hlActions, object key, object tag, ActionTypes action, string tooltip,
            bool enabled, ClickHandler deleteAction /*  bool deleteButton*/, GUIManager gui, out ImageButton btAction)
        {
            UIComponent item = new UIComponent(gui);
            item.DebugTag = action.ToString();
            hlActions.AddEntry(key, item);

            btAction = new ImageButton(gui);
            item.Add(btAction);
            btAction.Init(GetActionButtonType(action));
            btAction.Tag1 = tag; // key;
            // btAction.Click += new ClickHandler(tbAction_Click);
            btAction.ID = UIComponent.DataControlID.Action;
            btAction.Enabled = enabled;
            btAction.ScaleImageToSizeOfControl = false;
            btAction.ToolTip = tooltip;
            btAction.DebugTag = action.ToString() + "Button";

            item.Width = btAction.Width;
            item.Height = btAction.Height;

            Label lblName = new Label(gui);
            item.Add(lblName);
            lblName.Init(Label.LabelType.LCDNormal);
            lblName.Text = MissionActionTemplate.GetName(action).ToUpper(Config.Culture);
            lblName.X = 16;
            lblName.Y = 14;

            if (deleteAction != null) // deleteButton)
            {
                Image ibDeleteAction = new Image(gui);
                item.Add(ibDeleteAction);
                ibDeleteAction.SetSkinLocation(SkinState.Normal,gui.GUISpriteSheet.GetSourceRectangle("HUD_icon_trash"));
                ibDeleteAction.SetSkinLocation(SkinState.Hover, gui.GUISpriteSheet.GetSourceRectangle("HUD_icon_trash"), Color.Gray, Color.Gray);
                //ibDeleteTracking.Click += tbStopTracking_Click;
                ibDeleteAction.ToolTip = "Delete this action";
                ibDeleteAction.X = 101;
                ibDeleteAction.Y = 14;
                ibDeleteAction.Color = GameData.Instance.GUIConstants.sidePanelTextColor;
                ibDeleteAction.ResizeControlToFitImage();
                ibDeleteAction.Click += deleteAction; // ibDelete_Click;
            }

            Grid ordersGrid = new Grid(gui, ListBoxType.LCD, Label.LabelType.LCDNormal);
            item.Add(ordersGrid);
            ordersGrid.FixedItemHeights = true;
            ordersGrid.ItemHeight = 16;
            ordersGrid.Width = 95;
            ordersGrid.Height = 48;
            ordersGrid.CanGrowInHeight = false;
            ordersGrid.ScrollBarEnabled = false;
            ordersGrid.X = btAction.X + 16;
            ordersGrid.Y = 29;
            ordersGrid.ID = UIComponent.DataControlID.Passengers;
            ordersGrid.CanHaveFocus = false;
            ordersGrid.DebugTag = "passengerGrid";
            
            HorizontalList hzlIcons = new HorizontalList(gui);          
            item.Add(hzlIcons);
            hzlIcons.HorizontalSpacing = 6;
            hzlIcons.Y = 31;
            hzlIcons.Color = new Color(0x1E, 0x52, 0x5C);
            hzlIcons.DebugTag = "goodsList";
            hzlIcons.MaxWidth = 94;// btAction.Width - 20;
            hzlIcons.MaxHeight = 48;// btAction.Width - 20;           
            hzlIcons.X = btAction.X + 12;
            hzlIcons.ID = UIComponent.DataControlID.Goods;
            hzlIcons.CanHaveFocus = false;


            return item;
           
        }





        /*
          [XmlInclude(typeof(BuyActionTemplate))]
    [XmlInclude(typeof(LoadActionTemplate))]
    [XmlInclude(typeof(UnloadActionTemplate))]
    [XmlInclude(typeof(TravelActionTemplate))]
    [XmlInclude(typeof(EmbarkActionTemplate))]
    [XmlInclude(typeof(DisembarkActionTemplate))]         
         */

        private static CargoActionTypes? GetCargoActionType(ActionTypes action) //MissionActionTemplate action)
        {
            switch (action)
            {
                case ActionTypes.Buy:
                    return CargoActionTypes.Buy;

                case ActionTypes.Sell:
                    return CargoActionTypes.Sell;

            }

            return null;
        }
        /// <summary>
        /// i did not want to place this data in the Sim class...
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        private static ImageButtonType GetActionButtonType(ActionTypes action) //MissionActionTemplate action)
        {
            switch (action)
            {
                case ActionTypes.Buy:
                    return ImageButtonType.BuyAction;

                case ActionTypes.Sell:
                    return ImageButtonType.SellAction;

                case ActionTypes.Load:
                    return ImageButtonType.LoadAction;

                case ActionTypes.Unload:
                    return ImageButtonType.UnloadAction;

                case ActionTypes.Embark:
                    return ImageButtonType.EmbarkAction;

                case ActionTypes.Disembark:
                    return ImageButtonType.DisembarkAction;

                default:
                    return ImageButtonType.BuyAction;
            }

            /* if (action is BuyActionTemplate)
             {
                 return ImageButtonType.BuyAction;
             }
             else if (action is LoadActionTemplate)
             {
                 return ImageButtonType.LoadAction;
             }
             else if (action is UnloadActionTemplate)
             {
                 return ImageButtonType.UnloadAction;
             }
             else if (action is EmbarkActionTemplate)
             {
                 return ImageButtonType.EmbarkAction;
             }
             else if (action is DisembarkActionTemplate)
             {
                 return ImageButtonType.DisembarkAction;
             }

             return ImageButtonType.BuyAction;*/
        }

        /// <summary>
        /// add a row for each Action - Buy, Sell, Load etc.
        /// </summary>
        /// <param name="grdActions"></param>
        /// <param name="action"></param>
        /// <param name="enabled"></param>
        /*   private void AddActionRow(Grid grdActions, MissionActionTemplate action, bool enabled)
           {
               UIComponent item = new UIComponent(Interface.gui);
               grdActions.AddEntry(action, item);

               TextButton tbAction = new TextButton(Interface.gui);
               item.Add(tbAction);
               tbAction.Init(TextButton.TextButtonType.LCD);
               tbAction.Text = action.Name;
               tbAction.ScaleWidthToFitText();
               tbAction.Tag1 = action;            
               tbAction.Position = new Point(0, 0); 
               //tbAction.EventArgs = eventArgs; // new ItemTypeButtonEventArgs(entityType);
               tbAction.Click += new ClickHandler(tbAction_Click);
               tbAction.ID = UIComponent.DataControlID.Action;
               tbAction.Enabled = enabled;

               ImageButton ibDelete = new ImageButton(Interface.gui);
               item.Add(ibDelete);
               ibDelete.InitWithIcon(ImageButtonType.LCD, "lcd_icon_trash", false); 
               ibDelete.ToolTip = "Delete this action";            
               ibDelete.Tag1 = action;
               ibDelete.Position = new Point(60, 0);
               //tbAction.EventArgs = eventArgs; // new ItemTypeButtonEventArgs(entityType);
               ibDelete.Click += new ClickHandler(ibDelete_Click);
               ibDelete.Enabled = enabled;
               ibDelete.ID = UIComponent.DataControlID.Cancel;
           }*/

        private void UpdateAction(UIComponent item, MissionActionTemplate action, bool enabled)
        {
            ImageButton tbAction = (ImageButton)item.FindChildById(UIComponent.DataControlID.Action);
            tbAction.Enabled = true;// enabled;

            SerializableDictionary<string, List<long>> goodsOrders = null;

            BuySellActionTemplate buyActionTemplate = action as BuySellActionTemplate;
            LoadActionTemplate loadActionTemplate = action as LoadActionTemplate;

            if (buyActionTemplate != null)
            {
                goodsOrders = buyActionTemplate.ContractTemplate.Entities;
            }
            else if (loadActionTemplate != null)
            {
                goodsOrders = loadActionTemplate.Orders;
            }

            UpdateGoodsIcons(item, goodsOrders);


            List<long> passengers = null;
            EmbarkActionTemplate embarkActionTemplate = action as EmbarkActionTemplate;
            // DisembarkActionTemplate disembarkActionTemplate = action as DisembarkActionTemplate;

            if (embarkActionTemplate != null)
            {
                passengers = embarkActionTemplate.PassengerListTemplate.Passengers;
            }

            UpdatePassengers(item, passengers);

            /*  if (buyActionTemplate != null)
              {
                  hzlIcons.ToolTip = "";
                  foreach (var item in buyActionTemplate.ContractTemplate.Entities) // Goods)
                  {
                      EntityType entityType = GameData.Instance.AllEntityTypes[item.Key];
                      UIComponent component;
                      if (!hzlIcons.TryGetEntry(entityType, out component) && item.Value.Count > 0)
                      {
                          Image icon;
                          IconInfo iconInfo;
                          Rectangle rect;
                          rect = entityType.GetIconSprite(out iconInfo);
                          icon = new Image(Interface.gui);
                          icon.SetSkinLocation(SkinState.Normal,rect);
                          icon.Texture = Interface.gui.GUISpriteSheet.Texture;
                          icon.ToolTip = entityType.Name + " : " + item.Value;
                          icon.ResizeControlToFitImage();
                          icon.OrderByTag1 = (float)icon.Width;
                          hzlIcons.AddEntry(entityType, icon);
                          icon.Visible = true;
                          icon.ResizeControlToFitImage();
                      }
                      else
                      {
                          if (component != null)
                          {
                              component.ToolTip = entityType.Name + " : " + item.Value;

                              hzlIcons.ToolTip = hzlIcons.ToolTip + component.ToolTip + "\n ";
                          }
                      }
                  }


                  // if we change the noOfOrders from 1 or more to 0 the item stays in the order list, Should be removed if its 0? 

                  hzlIcons.DeleteEntries<EntityType>(r => buyActionTemplate.ContractTemplate.Entities[r.KeyName].Count > 0);//  ContainsKey(entityType.KeyName)); // .Contains(.Contains(r));
                  hzlIcons.RefreshEntries();
              }*/



            /*  ImageButton ibDelete = (ImageButton)row.FindChildById(UIComponent.DataControlID.Cancel);
              ibDelete.Enabled = enabled;*/
        }

        private void UpdatePassengers(UIComponent item, List<long> passengers) // EmbarkActionTemplate embarkActionTemplate)
        {
            // why use a horiz list here? another hack it seems.
           // HorizontalList hzlIcons = (HorizontalList)item.FindChildById(UIComponent.DataControlID.Passengers);
            Grid grdPassengers = (Grid)item.FindChildById(UIComponent.DataControlID.Passengers);
            grdPassengers.BeginAddingEntries();

            if (passengers != null)
            {               
                foreach (var passenger in passengers)
                {
                    Entity entity = Entity.FindByID((EntityID)passenger);
                    UIComponent component;

                    if (!grdPassengers.TryGetEntry(entity.ID, out component))
                    {        
                        grdPassengers.AddEntry(entity.ID, entity.Name);                       
                    }                   
                }

                grdPassengers.DeleteEntries<EntityID>(r => passengers.Contains((long)r));

            }
            else
            {
                grdPassengers.Clear();
            }

            grdPassengers.EndAddingEntries();

            /*
            grdPassengers.BeginAddingEntries();

            if (passengers != null)
            {
                grdPassengers.ToolTip = "";
                grdPassengers.Y = 30;
                foreach (var passenger in passengers)
                {
                    Entity entity = Entity.FindByID((EntityID)passenger);
                    UIComponent component;

                    if (!grdPassengers.TryGetEntry(entity.ID, out component))
                    {
                        grdPassengers.HorizontalSpacing = 0;
                        Label lblPassenger = new Label(Interface.gui);
                        lblPassenger.Init(Label.LabelType.LCDNormal);
                        lblPassenger.Text = entity.Name;
                        lblPassenger.Visible = true;
                        lblPassenger.FitToText();
                        lblPassenger.MaxWidth = grdPassengers.MaxWidth;

                        if (grdPassengers.Width < lblPassenger.Width)
                        {
                            grdPassengers.Width = lblPassenger.Width;
                        }

                        grdPassengers.AddEntry(entity.ID, lblPassenger);
                    }                   
                }

                grdPassengers.DeleteEntries<EntityID>(r => passengers.Contains((long)r));

            }
            else
            {
                grdPassengers.Clear();
            }

            grdPassengers.EndAddingEntries();*/
        }

        const int goodsRowHeight = 26;


        private void UpdateGoodsIcons(UIComponent item, SerializableDictionary<string, List<long>> orders)
        {
            HorizontalList hzlIcons = (HorizontalList)item.FindChildById(UIComponent.DataControlID.Goods);
            hzlIcons.BeginAddingEntries();

            hzlIcons.ToolTip = "";
            if (orders != null)
            {
                foreach (var good in orders)
                {
                    EntityType entityType = GameData.Instance.AllEntityTypes[good.Key];
                    UIComponent component;
                    if (!hzlIcons.TryGetEntry(entityType, out component) && good.Value.Count > 0)
                    {
                        Image icon;
                        IconInfo iconInfo;
                        Rectangle rect;
                        rect = entityType.GetIconSprite(out iconInfo);
                        int yOffset = 0;
                        if (iconInfo != null)
                        {
                            yOffset = iconInfo.GetYPosAdjustment(rect.Height);
                        }

                        icon = new Image(Interface.gui);
                        icon.SetSkinLocation(SkinState.Normal,rect);
                        icon.Texture = Interface.gui.GUISpriteSheet.Texture;
                       // icon.ToolTip = entityType.Name + ": " + good.Value.Count;
                        icon.ResizeControlToFitImage();
                        // hzlIcons.CenterChildVertically(icon, iconInfo != null ? iconInfo.CenterYPos : null);        
                        icon.OrderByTag1 = (float)icon.Width;
                        icon.CanHaveFocus = false;

                        UIComponent iconItem = new UIComponent(Interface.gui); // use an item so cntering is easier
                        iconItem.Width = rect.Width;
                        iconItem.Height = goodsRowHeight;
                        iconItem.Add(icon);
                        iconItem.CenterChildVertically(icon, iconInfo != null ? iconInfo.CenterYPos : null);
                        iconItem.CanHaveFocus = false;

                        hzlIcons.AddEntry(entityType, iconItem);
                       // hzlIcons.AddEntry(entityType, icon);
                       // icon.Y += yOffset;
                        //icon.Visible = true;

                    }
                        /*
                    else
                    {
                        if (component != null)
                        {
                            component.ToolTip = entityType.Name + " : " + good.Value.Count;

                            hzlIcons.ToolTip = hzlIcons.ToolTip + component.ToolTip + " \n";
                        }
                    }*/

                    
                }

                hzlIcons.DeleteEntries<EntityType>(r => orders.ContainsKey(r.KeyName) && orders[r.KeyName].Count > 0);

            }
            else
            {
                hzlIcons.Clear();
            }
            // if we change the noOfOrders from 1 or more to 0 the item stays in the order list, Should be removed if its 0? 

            hzlIcons.EndAddingEntries();
            //hzlIcons.RefreshEntries();

        }



        void ibDelete_Click(UIComponent sender, EventArgs e)
        {
            MissionActionTemplate action = sender.Parent.FindChildById(UIComponent.DataControlID.Action).Tag1 as MissionActionTemplate;
            MissionStopTemplate missionStop = action.MissionStopTemplate;

            missionStop.RemoveAction(action);

            // repopulate from the data source:
            UIComponent itemRow;

            surfaceGrid.TryGetEntry(missionStop, out itemRow);

            UpdateMissionLocationRow(itemRow, missionStop);

            PopulateLocationActions(missionStop);
            UpdateTotalCost();


            Revalidate();
        }

        private EntityGroup GetPlayerBuyer()
        {
            EntityGroup playerOwner;
            if (this.Mission.GetOwner(The.InGameUI.UIAllegiance.SharedKnowledge, e => e.GetAllegiance() == The.InGameUI.UIAllegiance, out playerOwner))
            {
                return playerOwner;
            }
            else
            {
                HandleDestroyedMissionStop();

                return null;
            }

        }

        /// <summary>
        /// get an npc owner on the itinerary
        /// </summary>
        /// <returns></returns>
        private EntityGroup GetNPCBuyer()
        {
            EntityGroup npcOwner;
            if (this.Mission.GetOwner(The.InGameUI.UIAllegiance.SharedKnowledge, e => e.GetAllegiance() != The.InGameUI.UIAllegiance, out npcOwner))
            {
                return npcOwner;
            }
            else
            {
                HandleDestroyedMissionStop();

                return null;
            }
        }

        /// <summary>
        /// clicking the button allows editing the action
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void tbAction_Click(UIComponent sender, EventArgs e)
        {
            MissionActionTemplate action = sender.Tag1 as MissionActionTemplate;

            MissionStopTemplate missionStop = action.MissionStopTemplate;

            Site site;
            Allegiance allegiance;
            Expedition expedition;
            IKnownEntityData terminal;
            // TravelLocation location = (TravelLocation)cbStart.SelectedKey;
            if (!missionStop.TravelLocation.ResolveLocation(The.InGameUI.UIAllegiance.SharedKnowledge, out site, out allegiance, out expedition, out terminal))
            {
                HandleDestroyedMissionStop();
                return;
            }


            switch (action.ActionType)
            {
                case ActionTypes.Buy:
                    {
                        EditBuySellAction(CargoActionTypes.Buy, sender, action, missionStop, /*allegiance,*/ expedition, null);

                        break;
                    }
                case ActionTypes.Sell:
                    {
                        EntityGroup npcBuyer = GetNPCBuyer();
                        EditBuySellAction(CargoActionTypes.Sell, sender, action, missionStop, /*allegiance,*/ expedition, npcBuyer);

                        break;
                    }
                case ActionTypes.Embark:
                    {
                        EmbarkActionTemplate template = action as EmbarkActionTemplate;

                        // save the action:
                        //  dialogSourceAction = CargoActionTypes.Buy; // selectedAction;

                        // save the key...
                        dialogSourceLocation = missionStop;

                        // save the action that we are editing:
                        dialogSourceActionTemplate = action;

                        ShowPersonnelDialog(sender.AbsolutePosition); //, template.PassengerListTemplate.Passengers.Select(p => (EntityID)p).ToList());

                        break;
                    }
            }
        }

        private void EditBuySellAction(CargoActionTypes cargoAction, UIComponent sender, MissionActionTemplate action, MissionStopTemplate missionStop, //Allegiance allegiance, 
            Expedition expedition, EntityGroup buyer)
        {
            BuySellActionTemplate buyTemplate = action as BuySellActionTemplate;

            // save the action:
            dialogSourceAction = cargoAction; // CargoActionTypes.Buy; // selectedAction;

            // save the key...
            dialogSourceLocation = missionStop;

            // save the action that we are editing:
            dialogSourceActionTemplate = action;

            ShowBuySellDialog(sender.AbsolutePosition, cargoAction, // CargoActionTypes.Buy,
                buyTemplate.ContractTemplate.Entities.ToDictionary(k => GameData.Instance.AllEntityTypes[k.Key], k => k.Value.Select(e => (EntityID)e).ToList()),
                /*allegiance,*/ expedition, buyer);
        }


        public override void Refresh()
        {
            base.Refresh();

            // refresh "child dialogs" too:

            if (The.InGameUI.WorldMapDialog.Window.IsVisibleAndActive)
            {
                The.InGameUI.WorldMapDialog.Refresh();
            }

            if (The.InGameUI.PersonnelDialog.Window.IsVisibleAndActive)
            {
                The.InGameUI.PersonnelDialog.Refresh();
            }

            if (The.InGameUI.BuySellDialog.Window.IsVisibleAndActive)
            {
                The.InGameUI.BuySellDialog.Refresh();
            }

            if (ActionPicker.Visible)
            {
                ActionPicker.Refresh();
            }
        }

        private UIComponent AddMissionLocationPanel(MissionStopTemplate locationKey, ref Label heading, ref Label subheading)
        {
            LCDInnerPanel innerPanel = new LCDInnerPanel(Interface.gui, lcdSurface.Width, true, 1f);
            surfaceGrid.AddEntry(locationKey, innerPanel.Panel);
            innerPanel.ContentHeight = 144; // 134; // 120;
            innerPanel.Panel.Y = DoubleSpacing;
            innerPanel.VerticalContentPadding = innerPanelVerticalPadding;
            innerPanel.Panel.OrderByTag1 = locationKey.Number * 2; // make numbering room for the travelaction


            heading = new Label(Interface.gui);
            innerPanel.AddContent(heading, 2, -1);
            heading.Init(Label.LabelType.LCDHeadingGreen);
            //  lblStart.Text = "DESTINATION:"; // "TO:";        
            heading.FitToText();

            subheading = new Label(Interface.gui);
            innerPanel.AddContent(subheading, heading.Right, 0);
            subheading.Init(Label.LabelType.LCDNormal);
            //  lblStart.Text = "DESTINATION:"; // "TO:";        
            subheading.FitToText();

            Grid contentGrid = new Grid(Interface.gui, ListBoxType.LCD, Label.LabelType.LCDNormal);
            contentGrid.FixedItemHeights = false;
            contentGrid.RenderType = RenderType.CRTAndLCD;
            innerPanel.AddContent(contentGrid);
            contentGrid.Font = GUIManager.LCDandHUDBodyFontPath;
            contentGrid.Width = 510;//263;
            contentGrid.Height = 0; // 30; 
            contentGrid.Y = heading.Bottom + DoubleSpacing;
            contentGrid.CanGrowInHeight = true;
            contentGrid.ScrollBarEnabled = false;
            contentGrid.Selectability = Grid.SelectabilityOptions.None;
            contentGrid.DebugTag = "contentGrid";
            contentGrid.IsOuterGrid = false; // NEW
            contentGrid.BeginAddingEntries();

            // place this grid inside the content grid, so the combo box will be pushed down when more actions are added.
            /*  Grid grdActions = new Grid(Interface.gui, ListBoxType.LCD, Label.LabelType.LCDNormal);
              grdActions.FixedItemHeights = true;
              grdActions.RenderType = RenderType.CRTAndLCD;
          //    innerPanel.AddContent(grdActions);
              contentGrid.AddEntry(grdActions, grdActions);
              grdActions.Font = GUIManager.LCDandHUDFontPath; // Important! Must be set after Add! Add will reinitialize with defaults.
              grdActions.Width = 260; 
              grdActions.Height = 0; // 30; 
              grdActions.ItemHeight = 26;//22; 
            //  grdActions.Y = 50;
              grdActions.CanGrowInHeight = true;
              grdActions.ScrollBarEnabled = false; 
              grdActions.Selectability = Grid.SelectabilityOptions.None;
              grdActions.ID = UIComponent.DataControlID.CurrentOrders;
              */

            HorizontalList hlActions = new HorizontalList(Interface.gui); // ListBoxType.LCD, Label.LabelType.LCDNormal);
            // grdActions.FixedItemHeights = true;
            hlActions.RenderType = RenderType.CRTAndLCD;
            hlActions.X = 2;
            contentGrid.AddEntry(hlActions, hlActions);
            hlActions.Font = GUIManager.LCDandHUDBodyFontPath; // Important! Must be set after Add! Add will reinitialize with defaults.
            hlActions.Width = contentGrid.Width; // 260;
            hlActions.Height = 0; // 30; 
            hlActions.HorizontalSpacing = 3;
            /* grdActions.ItemHeight = 26;//22;            
             grdActions.CanGrowInHeight = true;
             grdActions.ScrollBarEnabled = false;
             grdActions.Selectability = Grid.SelectabilityOptions.None;*/
            hlActions.ID = UIComponent.DataControlID.CurrentOrders;


            UIComponent addActionItem = new UIComponent(Interface.gui);
            addActionItem.Height = 22;

            // ErrorsAndMessages errors = new ErrorsAndMessages( , , );
            Label lblError = new Label(Interface.gui);
            addActionItem.Add(lblError);
            //innerPanel.AddContent(lbl);
            lblError.Init(Label.LabelType.LCDError); //.LCDNormal);
            // lblError.Text = "First select a transportation, then add actions to each stop.";
            lblError.Y = hlActions.Bottom + SingleSpacing;
            //lblError.FitToText();
            lblError.ID = UIComponent.DataControlID.ErrorsAndMessages;


            contentGrid.AddEntry(addActionItem, addActionItem);

            contentGrid.EndAddingEntries();

            /*
            if (locationKey.IsLocked)
            {
                lblCaption.Visible = false;
                lblError.Visible = false;
                cbAddAction.Visible = false;
            }*/


            return innerPanel.Panel;
        }

        public override void Show()
        {
            base.Show();

            Populate();
        }

        private void Populate()
        {
            //  PopulateTravelPointsComboBox(cbStart, null);
            //  PopulateTravelPointsComboBox(cbDestination, null);

            if (missionTemplate == null)
            {
                //  SetDefaultStartExpedition();
            }
        }

        public override void Hide()
        {
            base.Hide();


            HideOpenDialogs();

            start = null;
            destination = null;

            // clear the dynamic parts:
            ClearForm();

        }

        private void HideOpenDialogs()
        {
            ResetAfterBuySellDialog();
            ResetAfterPersonnelDialog();
            ResetAfterWorldMapDialog();

            The.InGameUI.BuySellDialog.Hide();
            The.InGameUI.PersonnelDialog.Hide();
            The.InGameUI.WorldMapDialog.Hide();
        }


        protected override void OnCancel()
        {
            base.OnCancel();

            // destroy the temporary mission data:
            DestroyMission();
        }

        private void DestroyMission()
        {
            if (missionTemplate != null)
            {
                missionTemplate.Destroy();
                missionTemplate = null;
            }
        }

        private void ClearForm()
        {
            // clear form variables
            missionTemplate = null;
            dialogSourceLocation = null;
            dialogSourceAction = null;

            The.InGameUI.WorldMapDialog.UnCheckSiteMarkerButtons();

            lblTotalMissionCost.Text = "";
            lblAvailableCargoBulk.Text = "";
            lblTotalCargoBulk.Text = "";
            lblCost.Text = "";
            lblTotalTradingCredits.Text = "";

            iconHomeStart.Visible = false;
            iconHomeDestination.Visible = false;

            ClearErrors();
           // TintErrorPanel(new Color(255, 255, 255, 255));

            DataTypeButton dt = (DataTypeButton)selectionPanel.Panel.FindChildById(UIComponent.DataControlID.Name);
            if (dt != null)
            {
                selectionPanel.Panel.Remove(dt);
            }

            cbTransportation.Clear();

            exclamationMarkDestination.Visible = false;
            exclamationMarkStart.Visible = false;
            exclamationTransport.Visible = false;

            lblTransportNotesOrCost.Visible = false;
            lblDestination.Visible = false;
            lblStartingLocation.Visible = false; ;

            transportIsExpanded = false;
            tbTransportMore.Text = "MORE";

            selectionPanel.ContentHeight = collapsedPanelContentHeight - 4;

            errorAndMessagePanel.Clear();

            
            //clear GUI
            foreach (var item in surfaceGrid.EntriesByKey)
            {
                if (item.Key is MissionStopTemplate)
                {
                    surfaceGrid.TryRemoveEntry(item);
                }
            }

            surfaceGrid.Clear();
        }

        private bool CanCommunicateWithExpedition(Expedition expedition)
        {
            CommunicationMethod? method;

            return Communicates.IsInCommunicationRange(expedition.Allegiance, The.InGameUI.UIAllegiance, out method);


            //   return Communicates.IsInCommunicationRange(expedition.Allegiance,
            //       LookUp<Expedition,ExpeditionID>.FindByID((ExpeditionID)destination.ExpeditionID.Value).Allegiance, out method);
        }

        private bool PopulateTransportation()
        {
            // see if modes of transport exist between the start and destination

            // if multiple vehicles exist, only list one entry for each type

            Mission.TransportationType = null; // clear the mission data also.

            cbTransportation.BeginAddingEntries();

            cbTransportation.Clear();


            /*  object from = start; 
              object to = destination; 
              */

            if (start != null
                && destination != null)
            {
                TravelLocation fromLocation = start.Value; // from as TravelLocation;
                Site fromSite;
                Allegiance fromAllegiance;
                Expedition fromExpedition;
                IKnownEntityData fromTerminal;

                if (!fromLocation.ResolveLocation(The.InGameUI.UIAllegiance.SharedKnowledge, out fromSite, out fromAllegiance, out fromExpedition, out fromTerminal))
                {
                    HandleDestroyedMissionStop();
                    return false;
                }

                TravelLocation toLocation = destination.Value; // to as TravelLocation;
                Site toSite;
                Allegiance toAllegiance;
                Expedition toExpedition;
                IKnownEntityData toTerminal;
                if (!toLocation.ResolveLocation(The.InGameUI.UIAllegiance.SharedKnowledge, out toSite, out toAllegiance, out toExpedition, out toTerminal))
                {
                    HandleDestroyedMissionStop();
                    return false;
                }


                if (fromAllegiance != null
                    && fromExpedition != null
                    && fromAllegiance != toAllegiance)
                {

                    if (VehiclesAreHired(fromExpedition)
                        && !CanCommunicateWithExpedition(fromExpedition))
                    {
                        cbTransportation.Enabled = false;
                    }
                    else
                    {
                        cbTransportation.Enabled = true;

                        Dictionary<EntityType, List<Entity>> vehicles;
                        bool hasLandRoute;
                        GetVehicleTransportationOptions(fromSite, fromExpedition, toSite, out vehicles, out hasLandRoute);

                        if (vehicles != null)
                        {

                            string displayText;
                            foreach (var group in vehicles)
                            {
                                displayText = group.Key.Name;
                                if (fromExpedition.Allegiance != The.InGameUI.UIAllegiance)
                                {
                                    displayText += " (HIRED)"; // for hired vehicles
                                }

                                cbTransportation.AddEntry(new TransportationType() { VehicleType = group.Key }, displayText);
                            }
                        }

                        if (hasLandRoute)
                        {
                            // by land route, it should be possible to choose a foot expedition too (maximum range?)
                            // disabled for now.
                           // cbTransportation.AddEntry(new TransportationType(), "On foot");
                        }
                    }
                }

                OutputTransportNotes(fromExpedition);

            }
            else
            {
                OutputTransportNotes(null);
            }

            cbTransportation.EndAddingEntries();


            return true;
        }

        /// <summary>
        /// selects from and to targets according to available transport options.
        /// Let's keep this until we have the Template feature...
        /// </summary>
        private void SetDefaultStartExpedition()
        {
            List<TravelLocation> locations = GetAllTravelLocations();
            foreach (var item in locations) // cbStart.EntriesByKey)
            {
                TravelLocation startLocation = item;

                Site startSite;
                Allegiance startAllegiance;
                Expedition startExpedition;
                IKnownEntityData startTerminal;
                if (startLocation.ResolveLocation(The.InGameUI.UIAllegiance.SharedKnowledge, out startSite, out startAllegiance, out startExpedition, out startTerminal))
                {
                    Site toSite;
                    Allegiance toAllegiance;
                    Expedition toExpedition;
                    IKnownEntityData toTerminal;

                    foreach (var item2 in locations) // cbDestination.EntriesByKey)
                    {
                        if (!item.Equals(item2)) // item.Value.Text != item2.Value.Text) // use reference equality instead???
                        {
                            TravelLocation endLocation = item2; // as TravelLocation;

                            if (endLocation.ResolveLocation(The.InGameUI.UIAllegiance.SharedKnowledge, out toSite, out toAllegiance, out toExpedition, out toTerminal))
                            {
                                Dictionary<EntityType, List<Entity>> vehicles;
                                bool hasLandRoute;
                                GetVehicleTransportationOptions(startSite, startExpedition, toSite, out vehicles, out hasLandRoute);
                                if (vehicles != null &&
                                    vehicles.Any(k => k.Value.Count > 0))
                                {
                                    start = item;
                                    destination = item2;
                                    //cbStart.SelectedKey = item.Key;
                                    //cbDestination.SelectedKey = item2.Key;

                                    return;
                                }
                            }
                        }
                    }

                }
            }
        }



        private static void GetVehicleTransportationOptions(Site fromSite, Expedition fromExpedition, Site toSite, //TravelLocation fromLocation, Expedition fromExpedition, TravelLocation toLocation, 
            out Dictionary<EntityType, List<Entity>> vehicles, out bool hasLandRoute)
        {
            
            List<Tuple<Route, double>> allRoutes = The.Sim.World.GetRoutesAndDistances(fromSite, toSite);
           
            vehicles = new Dictionary<EntityType, List<Entity>>();
            hasLandRoute = false;
            foreach (var item in allRoutes)
            {
                bool isAirRoute = false;
                RouteType? routeType = null; 
                if (item.Item1 == null)
                {
                    isAirRoute = true;
                }
                else
                {
                    routeType = item.Item1.RouteType;
                }

                fromExpedition.GetAvailableVehicles(routeType, isAirRoute, item.Item2,
                    ref vehicles, null); // get all vehicles - or just one per type..?

                if (routeType == RouteType.Land)
                {
                    hasLandRoute = true;
                }
            }
        }

        const string noCommTooltip = "To hire transports from another allegiance, we need to establish communication first. A ground satellite station is a good option.";

        private void OutputTransportNotes(Expedition fromExpedition)
        {
            bool vehiclesAreHired = VehiclesAreHired(fromExpedition);


            // write helpful notes:
            if (vehiclesAreHired
                && !CanCommunicateWithExpedition(fromExpedition))
            {               
                ShowError("No communication with the Start allegiance.", noCommTooltip);
            }
            else if (cbTransportation.EntriesByKey.Count == 0 && start != null && destination != null)
            {
                ShowError("No modes of transport are available from Start to Destination.", null, FieldError.Transport);               
            }
            else
            {
                if (cbTransportation.SelectedKey != null)
                {
                    // show info about the selected transport:

                    EntityType transportType = ((TransportationType)cbTransportation.SelectedKey).VehicleType;

                    if (vehiclesAreHired && transportType != null) //fromAllegiance != The.InGameUI.UIAllegiance)
                    {
                        lblTransportNotesOrCost.Visible = true;
                        exclamationTransport.Visible = false;

                      /*  decimal? pricePerKilometer;
                        decimal? price = fromExpedition.OwnedEntities.GetVehicleForHirePrice(transportType, out pricePerKilometer);
                        */
                        decimal startFee, totalDistanceCost, costPerKilometer, total;

                        total = missionTemplate.ComputeTransportationCost(out startFee, out totalDistanceCost, out costPerKilometer);

                        lblTransportNotesOrCost.Text = "COST TO HIRE: " + Common.GetPriceAsString(total); // +"\n" + "The vehicle is not owned by us, but we can hire it for a price.";
                       // ColorLabelByValue(lblTransportNotesOrCost, total);

                        StringBuilder text = new StringBuilder();
                        Common.AppendLine(text, "Hired transport cost");
                        Common.AppendLine(text, "The vehicle is not owned by us, but we can hire it for a price.");
                        Common.AppendDivider(text);
                        Common.Append(text, "Starting fee: ");
                        Common.Append(text, Common.GetPriceAsString(startFee), true);
                        Common.AppendLine(text);
                        Common.Append(text, "Cost per kilometer: ");
                        Common.Append(text, Common.GetPriceAsString(costPerKilometer), true);
                        Common.AppendLine(text);
                        Common.Append(text, "Distance cost: ");
                        Common.Append(text, Common.GetPriceAsString(totalDistanceCost), true);
                        Common.AppendLine(text);
                        Common.AppendLine(text);
                        Common.Append(text, "Total cost: ");
                        Common.Append(text, Common.GetPriceAsString(total), true);
                        //Common.AppendFormat(text, "Max. amount we can sell: {0}", true, noOfAvailableItems);
                        lblTransportNotesOrCost.ToolTip = text.ToString();


                        //lblTransportNotes.ToolTip = "The vehicle is not owned by us, but we can hire it for a price.";
                    }
                    else
                    {
                        lblTransportNotesOrCost.Text = "";
                        lblTransportNotesOrCost.Visible = false;
                    }
                }
                else
                {
                    lblTransportNotesOrCost.Visible = false;
                }
            }

            // lblTransportNotes.FitToText();
        }





        const string separator = " | ";
        static int separatorLength = separator.Length;


        private List<TravelLocation> GetAllTravelLocations()
        {
            List<TravelLocation> locations = new List<TravelLocation>();
            Site site;
            List<Entity> terminals;

            /*  foreach (var kvp in The.Sim.World.AllSites)
              {
                  site = kvp.Value;

                  foreach (var allegiance in site.Allegiances)
                  {
                      if (allegiance.RepresentativeEntityType.Person != null) // only list human allegiances...
                      {
                       
                          foreach (var expedition in allegiance.Expeditions)
                          {
                           
                              terminals = expedition.GetTerminals(); // the terminal type (helipad, pier...) should filter the transportation options

                              TravelLocation location;

                              location = new TravelLocation(allegiance, (long)expedition.ID, null);
                         

                              if (terminals != null)
                              {
                                  foreach (var terminal in terminals)
                                  {
                                    
                                      location = new TravelLocation(allegiance, (long)expedition.ID, (long)terminal.ID);
                                  

                                      locations.Add(location);
                                  }
                              }
                              else
                              {
                                  // add an entry for no terminals
                               
                                  location = new TravelLocation(allegiance, (long)expedition.ID, null);
                              
                                  locations.Add(location);
                              }
                          }
                      }
                  }

                  // TODO: here, we can add Sites without allegiances too (fishing? exploration?):

              }*/

            return locations;

        }

        /*
        private void PopulateTravelPointsComboBox(ComboBox cb, TravelLocation excludeLocation)
        {
            List<Entity> terminals = null;
                       
            bool hasAddedSiteName = false;
            bool hasAddedAllegianceName = false;
            bool hasAddedExpeditionName = false;

            //StringBuilder displayName = new StringBuilder();
                     

            Site site;


            cb.BeginAddingEntries();

            cb.Clear();

            foreach (var kvp in The.Sim.World.AllSites)
            {
                site = kvp.Value;

                hasAddedSiteName = false;
                foreach (var allegiance in site.Allegiances)
                {
                    if (allegiance.RepresentativeEntityType.Person != null) // only list human allegiances...
                    {
                        hasAddedAllegianceName = false;
                        foreach (var expedition in allegiance.Expeditions)
                        {
                            hasAddedExpeditionName = false;
                            terminals = expedition.GetTerminals(); // the terminal type (helipad, pier...) should filter the transportation options

                            if (terminals != null)
                            {
                                foreach (var terminal in terminals)
                                {
                                    AddTravelLocationToComboBox(cb, ref hasAddedSiteName, ref hasAddedAllegianceName, ref hasAddedExpeditionName, site, allegiance, expedition, terminal, excludeLocation);
                                }
                            }
                            else
                            {
                                // add an entry for no terminals
                                AddTravelLocationToComboBox(cb, ref hasAddedSiteName, ref hasAddedAllegianceName, ref hasAddedExpeditionName, site, allegiance, expedition, null, excludeLocation);

                            }
                        }
                    }
                }

                // TODO: here, we can add Sites without allegiances too (fishing? exploration?):

            }

            cb.EndAddingEntries();

        }
        */
        /*
        private static void AddTravelLocationToComboBox(ComboBox cb, 
            ref bool hasAddedSiteName, ref bool hasAddedAllegianceName, ref bool hasAddedExpeditionName,
            Site site, SimSide.Allegiances.Allegiance allegiance, SimSide.Expeditions.Expedition expedition, Entity terminal, TravelLocation excludeLocation)
        {
            
            if (excludeLocation != null
                && (AllegianceID)excludeLocation.AllegianceID == allegiance.ID
                && (ExpeditionID)excludeLocation.ExpeditionID == expedition.ID
                && ((terminal == null && excludeLocation.TerminalEntityID == null) || ((EntityID?)excludeLocation.TerminalEntityID == terminal.ID)))
            {
                return;
            }

            TravelLocation location;

            location = new TravelLocation();
            location.SetAllegiance(allegiance);
            location.ExpeditionID = (long)expedition.ID;

            if (terminal != null)
            {
                location.TerminalEntityID = (long)terminal.ID;
            }

            string displayName = "";

            //displayName.Clear();
            if (!hasAddedSiteName)
            {
                if (site.IsPlaySite)
                {
                    displayName += "- HERE -";
                }

                //displayName.Append(site.Value.Name);
                displayName += site.Name;
                hasAddedSiteName = true;
            }
            else
            {
                displayName = displayName.PadLeft(site.Name.Length + separatorLength);
            }

            if (!hasAddedAllegianceName)
            {
                displayName += separator + allegiance.Name;
                hasAddedAllegianceName = true;
            }
            else
            {
                displayName = displayName.PadLeft(displayName.Length + separatorLength + allegiance.Name.Length);
            }

            if (!hasAddedExpeditionName)
            {
                displayName += separator + expedition.Name;
                hasAddedExpeditionName = true;
            }
            else
            {
                displayName = displayName.PadLeft(displayName.Length + separatorLength + expedition.Name.Length);
            }

            if (terminal != null)
            {
                displayName += separator + terminal.EntityType.Name;
            }
            else
            {
                //??
            }

            cb.AddEntry(location, displayName);
        }*/


        public bool HasTransportationIfStart(TravelLocation travelLocation)
        {
            if (worldMapDialogSource == WorldMapDialogSource.Start)
            {
              
                Site site;
                Allegiance allegiance;
                IKnownEntityData terminal;
                Expedition expedition;

                if (travelLocation.ResolveLocation(The.InGameUI.UIAllegiance.SharedKnowledge, out site, out allegiance, out expedition, out terminal))
                {                   
                    return expedition.HasAavailableVehicle();                     
                }
                else
                {
                    HandleDestroyedMissionStop();
                    return false;
                }
            }

            return true;
        }


        private bool TransportCanUseTerminal(TravelLocation toTravelLocation) //, EntityType transportType)
        {

            if (missionTemplate.TransportationType != null)
            {
                EntityType transportType = missionTemplate.TransportationType.GetMainTransportation();

                if (transportType != null)
                {
                    VehicleContainerType vehicle = transportType.ContainerType as VehicleContainerType;

                    // cannot test route before we pick one...?
                    /* if (!TravelActionTemplate.ValidateRoute( , , ,vehicle, ref errors))
                     {
                         return false;
                     }
                     else 
                     {     */

                    List<string> errors = null;
                    return TravelActionTemplate.CanUseTerminal(missionTemplate, toTravelLocation, vehicle, ref errors);

                    // }
                }
            }

            return true;

        }


        internal bool CanSelectLocation(TravelLocation travelLocation, out bool isAlreadySelected, out bool wrongTerminalType)
        {
            isAlreadySelected = false;
            wrongTerminalType = false;

            if (worldMapDialogSource == WorldMapDialogSource.End)
            {
                if (destination != null)
                {
                    if (destination.Value.AllegianceID == travelLocation.AllegianceID && destination.Value.ExpeditionID == travelLocation.ExpeditionID
                        && destination.Value.TerminalEntityID == travelLocation.TerminalEntityID)
                    {
                        isAlreadySelected = true;
                        return false;
                    }

                    if (start != null)
                    {
                        if (travelLocation.AllegianceID == start.Value.AllegianceID && travelLocation.ExpeditionID == start.Value.ExpeditionID)
                        {
                            isAlreadySelected = true;
                            return false;
                        }
                    }

                   // List<string> errors = null;
                    if (!TransportCanUseTerminal(travelLocation))
                    {
                        wrongTerminalType = true;
                        return false;
                    }

                }
                else
                {
                    if (travelLocation.AllegianceID == start.Value.AllegianceID && travelLocation.ExpeditionID == start.Value.ExpeditionID)
                    {
                        isAlreadySelected = true;
                        return false;
                    }
                }
            }
            else
            {
                if (start != null)
                {
                    if (start.Value.AllegianceID == travelLocation.AllegianceID && start.Value.ExpeditionID == travelLocation.ExpeditionID)
                    {
                        isAlreadySelected = true;
                        return false;
                    }
                }
                else
                {
                    if (destination != null)
                    {
                        if (travelLocation.AllegianceID == destination.Value.AllegianceID && travelLocation.ExpeditionID == destination.Value.ExpeditionID
                            || travelLocation.TerminalEntityID == destination.Value.TerminalEntityID)
                        {
                            isAlreadySelected = true;
                            return false;
                        }
                    }
                }
            }

            return true;
        }
    }

    /// <summary>
    /// this class is silly, should just have been an icon
    /// </summary>
    public class ExclamationMarkInACircle : UIComponent
    {
        public Icon circle;

        public Label exclamationMark;

        // public Image exclamationmark;

        public ExclamationMarkInACircle(GUIManager gui)
            : base(gui)
        {
            this.RenderType = WindowSystem.RenderType.CRTAndLCD;
            this.Width = 23;
            this.Height = 23;

            circle = new Icon(guiManager);
            circle.SetSkinLocation(SkinState.Normal,guiManager.GUISpriteSheet.GetSourceRectangle("lcd_icon_circleBG"), Color.Red, Color.Red);
            circle.Width = 22;
            circle.Height = 22;
            circle.ResizeControlToFitImage();
            Add(circle);
            circle.X = 0;
            circle.Y = 0;


            exclamationMark = new Label(guiManager);
            Add(exclamationMark);
            exclamationMark.Init(Label.LabelType.LCDError);
            exclamationMark.Text = "!";
            exclamationMark.X = 7;
            exclamationMark.Y = 1;
            exclamationMark.FitToText();

            /*
            exclamationmark = new Image(guiManager);
            exclamationmark.SetSkinLocation(SkinState.Normal,guiManager.GUISpriteSheet.GetSourceRectangle("HUD_icon_status_exclamation"), Color.Red, Color.Red);
            exclamationmark.ResizeControlToFitImage();
            Add(exclamationmark);
            exclamationmark.X = -1;
            exclamationmark.Y =-1;*/
        }

        public override string ToolTip
        {
            get
            {
                return circle.ToolTip;
            }
            set
            {
                circle.ToolTip = value;
            }
        }

        protected override void OnMouseOut(UIComponent sender, InputEventSystem.MouseEventArgs args)
        {
            circle.SetMouseOutState();

            base.OnMouseOut(sender, args);
        }

        protected override void OnMouseOver(UIComponent sender, InputEventSystem.MouseEventArgs args)
        {
            circle.SetMouseOverState();

            base.OnMouseOver(sender, args);


        }

    }

}
