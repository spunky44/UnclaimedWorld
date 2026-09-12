using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.Control.Commands;
using UWGame.SimSide;
using UWGame.SimSide.Commands;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using WindowSystem;

namespace UWGame.ClientSide.Interface.HUD_Windows
{
    public class PatrolWindow : HUDWindow
    {
        MapArea mapArea;

        CheckBox cbAttackVermin;
        CheckBox cbAttackTargetsOutsideZone;

        TextButton btOK;

        Label lblName, lblHeader;
        Image headerIcon;

        FillableBar fbNoOfPatrollers;


        public PatrolWindow()
            : base(239, 240, true, level: Level.Bottom, isMovable: true) 
        {

            AddZoneNameAndHeader("", "PATROL", "HUD_icon_patrol", tripleSpacing, out lblName, out lblHeader, out headerIcon);

            Label lblPatrollers = new Label(gui);
            lblPatrollers.Init(Label.LabelType.HUDWindow);
            Add(lblPatrollers);
            lblPatrollers.Text = "No. of patrollers:";
            lblPatrollers.X = tripleSpacing;
            lblPatrollers.Y = 52;
            
            fbNoOfPatrollers = new FillableBar(gui, FillableBar.FillableBarType.HUDSlider, false, true, GameData.Instance.GUIConstants.TimeBetweenSliderButtonIncrements, GameData.Instance.GUIConstants.SliderButtonDelay);
            Add(fbNoOfPatrollers);
            fbNoOfPatrollers.X = 120;
            fbNoOfPatrollers.Y = lblPatrollers.Y;
            fbNoOfPatrollers.Width = 120;
            fbNoOfPatrollers.MaxValue = 10;
            fbNoOfPatrollers.Value = 1;
            fbNoOfPatrollers.UpdateSliderPosition();

            cbAttackVermin = new CheckBox(gui);
            Add(cbAttackVermin);
            cbAttackVermin.Init(CheckBoxType.HUDCheckBox);
            cbAttackVermin.Text = "Attack vermin";
            cbAttackVermin.ToolTip = "Select whether vermin should be attacked by the patroller in addition to dangerous animals."; //Select whether any nearby vermin should be attacked while on patrol
            cbAttackVermin.FitToText();
            cbAttackVermin.X = tripleSpacing;
            cbAttackVermin.Y = lblPatrollers.Bottom + doubleSpacing;

            cbAttackTargetsOutsideZone = new CheckBox(gui);
            Add(cbAttackTargetsOutsideZone);
            cbAttackTargetsOutsideZone.Init(CheckBoxType.HUDCheckBox);
            cbAttackTargetsOutsideZone.Text = "May leave zone in pursuit";
            cbAttackTargetsOutsideZone.ToolTip = "Select whether the patroller is permitted to pursue targets far outside the patrol zone."; //mp shorter than:  Select whether the patrolling character is permitted to pursue targets at long distances outside the patrol zone
            cbAttackTargetsOutsideZone.FitToText();
            cbAttackTargetsOutsideZone.X = tripleSpacing;
            cbAttackTargetsOutsideZone.Y = cbAttackVermin.Bottom + doubleSpacing;




            TextButton btCancel = new TextButton(gui);
            Add(btCancel);
            btCancel.Text = "CANCEL";
            btCancel.Init(TextButton.TextButtonType.HUD);
            btCancel.Click += btCancel_Click;
            btCancel.Width = 72;
            //   btCancel.Height = buttonHeight;
            btCancel.Y = DisplayWindow.Height - btCancel.Height - tripleSpacing;
            btCancel.X = DisplayWindow.Width - tripleSpacing - btCancel.Width;


            btOK = new TextButton(gui);
            Add(btOK);
            btOK.Text = "OK";
            btOK.Init(TextButton.TextButtonType.HUD);
            btOK.Click += btOK_Click;
            btOK.Width = 72;
            //    bt.Height = buttonHeight;
            btOK.Y = DisplayWindow.Height - btOK.Height - tripleSpacing;
            btOK.X = btCancel.X - 2 - btOK.Width;
                     

        }

        public override void ShowOnPlayfield(int screenPosX, int screenPosY, bool avoidRightInterfaceArea = true)
        {
            base.ShowOnPlayfield(screenPosX, screenPosY, avoidRightInterfaceArea);

           /* MapArea*/ mapArea = TileSelectionContextMenu.GetMapArea();

            string zoneName = "";
            if (mapArea.Zone != null)
            {
                zoneName = mapArea.Zone.GetDisplayName();
            }

            SetDisplayName(zoneName, lblName, lblHeader, headerIcon);


            if (mapArea.Zone != null && mapArea.Zone.PatrolJob != null)
            {
                cbAttackVermin.IsChecked = mapArea.Zone.PatrolJob.AttackVermin;
                cbAttackTargetsOutsideZone.IsChecked = mapArea.Zone.PatrolJob.AttackTargetsOutsideZone;
                
                fbNoOfPatrollers.Value = mapArea.Zone.PatrolJob.MaxJobPositions;
                fbNoOfPatrollers.UpdateSliderPosition();

                cbAttackVermin.Enabled = false; // #UPDATEATTACKJOBTAKERS
                cbAttackTargetsOutsideZone.Enabled = false;

               // btOK.Enabled = false; // cannot currently update a job - no command exists...
            }
            else
            {
                cbAttackVermin.IsChecked = false;
                cbAttackTargetsOutsideZone.IsChecked = false;
                // btOK.Enabled = true;

                cbAttackVermin.Enabled = true; // #UPDATEATTACKJOBTAKERS
                cbAttackTargetsOutsideZone.Enabled = true;
            }


        }

        public override void Hide()
        {
            base.Hide();
        }

        void btOK_Click(UIComponent sender, EventArgs e)
        {
            if (TileSelectionContextMenu.CanCreateAndSelectZone())
            {
                
                EntityGroupID groupID;
                if (!TileSelectionContextMenu.GetExpedition(out groupID))
                {
                    Hide();
                    return;
                }

                Command patrolCommand;
                bool attackVermin = cbAttackVermin.IsChecked;
                bool attackOutsideZone = cbAttackTargetsOutsideZone.IsChecked;

                int noOfPatrollers = fbNoOfPatrollers.Value;

               // noOfPatrollers = Common.ClampBottom(noOfPatrollers, 1);
                
                // same logic as in AttackArea
                if (mapArea.Zone != null && mapArea.Zone.PatrolJob != null)
                {
                    //#UPDATEATTACKJOBTAKERS
                    // NEW - update patrol job:                 
                    patrolCommand = new PatrolAreaUpdateJob(mapArea.Zone.PatrolJob.ID, true, noOfPatrollers);

                }
                else
                {
                    if (The.InGameUI.SelectedZone != null)
                    {
                        patrolCommand = new PatrolArea(The.InGameUI.SelectedZone.ID, true, groupID, attackVermin, attackOutsideZone, noOfPatrollers);
                    }
                    else
                    {
                        patrolCommand = new PatrolArea(The.InGameUI.SelectedTiles, true, groupID, attackVermin, attackOutsideZone, noOfPatrollers);
                    }
                }

                The.Client.Controller.StoreAndExecuteCommand(patrolCommand);                
                
            }
        }

        void btCancel_Click(UIComponent sender, EventArgs e)
        {
            Hide();
        }


    }
}
