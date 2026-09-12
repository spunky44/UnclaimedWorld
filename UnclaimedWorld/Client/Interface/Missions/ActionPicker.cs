using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;
using UWGame.ClientSide.Interface.LCD;
using UWGame.SimSide.Overland.Missions.Templates;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Snapshots;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Overland;
using UWGame.SimSide.AI;
using UWGame.SimSide.Entities.Containers;

namespace UWGame.ClientSide.Interface.Missions
{
    /// <summary>
    /// a panel that covers the window content.
    /// </summary>
    public class ActionPicker : UIComponent
    {
        LCDInnerPanel panel;

        //List<ImageButton

        HorizontalList list;

        int maxWidth = 409;//512;

        public event Action<ActionTypes> ActionSelected;

        ImageButton btSell, btBuy, btLoad, btUnload, btEmbark, btDisembark;


        MissionStopTemplate missionStopTemplate;
        MissionTemplate missionTemplate;

        public event Action InvalidLocation;


        public ActionPicker(GUIManager gui)
            : base(gui)
        {
            this.DebugTag = "ActionPicker";
            panel = new LCDInnerPanel(gui, maxWidth, true, 1f); // the image decor in the corners will intercept the mouse and prevent the panel from firing mouseOut! 
            Add(panel.Panel);
            panel.ContentHeight = 215;
            panel.Panel.Y = 0;
            panel.VerticalContentPadding = 6;
            panel.Panel.X = 0;

            Color backgroundColor = Common.ColorFromHex("#ccffcc");
            panel.Panel.SetSkinLocation(SkinState.Normal,gui.GUISpriteSheet.GetSourceRectangle("lcd_panel_background"), backgroundColor, backgroundColor);

            Width = panel.Panel.Width;
            Height = panel.Panel.Height;


            base.ZOrder = 1;


            list = new HorizontalList(gui);
            // grdActions.FixedItemHeights = true;
            list.RenderType = RenderType.CRTAndLCD;
            panel.AddContentSetFullWidth(list);
            list.Font = GUIManager.LCDandHUDBodyFontPath; // Important! Must be set after Add! Add will reinitialize with defaults.           
            list.MaxWidth = panel.ContentWidth; // wrap around
            list.Height = 0; // 30; 
            list.Y = 15;
            list.X = 15;
            /* grdActions.ItemHeight = 26;//22;            
             grdActions.CanGrowInHeight = true;
             grdActions.ScrollBarEnabled = false;
             grdActions.Selectability = Grid.SelectabilityOptions.None;*/


            PopulateAll();
        }

       
        public void Fill(MissionTemplate missionTemplate, MissionStopTemplate missionstopTemplate)
        {
            this.missionStopTemplate = missionstopTemplate;
            this.missionTemplate = missionTemplate;
        }

        public void Show()
        {
            //  PopulateAll();
            Populate();
        }


        public new void Refresh()
        {
            Populate();
        }

        private void PopulateAll()
        {
            
            list.BeginAddingEntries();

            btBuy = AddAction(ActionTypes.Buy);
            btEmbark = AddAction(ActionTypes.Embark);
          //  btLoad = AddAction(ActionTypes.Load);
            btSell = AddAction(ActionTypes.Sell);

            // done per auto:
          //  btDisembark = AddAction(ActionTypes.Disembark);
          //  btUnload = AddAction(ActionTypes.Unload);


            list.EndAddingEntries();
            
        }

        private ImageButton AddAction(ActionTypes action)
        {
            ImageButton btAction;
            CreateMissionPanel.AddAction(list, action, action, action, null, true, null, guiManager, out btAction);

            btAction.Click += new ClickHandler(bt_Click);

            return btAction;

        }

        private void Populate() //MissionTemplate Mission, MissionStopTemplate location, Allegiance allegiance, Expedition expedition) 
        {

            Allegiance thisAllegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)missionTemplate.Allegiance);

            // set enabled/disabled status
            Site site;
            Allegiance allegiance;
            Expedition expedition;
            IKnownEntityData terminal;
            if (!missionStopTemplate.TravelLocation.ResolveLocation(The.InGameUI.UIAllegiance.SharedKnowledge, out site, out allegiance, out expedition, out terminal ))
            {
                //this.Close();
                if (InvalidLocation != null)
                {
                    InvalidLocation.Invoke();
                }

                return;
            }

            // the Seller is the expedition at the MissionStop - don't allow selling     
           // btBuy.Enabled = CanBuyAtTerminal(terminal);

            UpdateSelling(terminal);
            UpdateBuying(terminal);
            UpdateEmbark(thisAllegiance, terminal, allegiance);
          //  UpdateDisembark(terminal);
            
            /*
            if (allegiance != null)
            {
                if (allegiance == The.InGameUI.UIAllegiance) // own allegiance
                {
                    if (location.IsStart())
                    {
                        AddActionOption(actions, ActionTypes.Load);
                    }
                    else
                    {
                        AddActionOption(actions, ActionTypes.Unload);
                        AddActionOption(actions, ActionTypes.Disembark);
                    }
                }
                else // other allegiance
                {
                    if (location.IsStart())
                    {
                        AddActionOption(actions, ActionTypes.Buy);
                        AddActionOption(actions, ActionTypes.Embark);

                        //  RemoveActionOption(ActionTypes.Unload);
                        //  RemoveActionOption(ActionTypes.Disembark);
                    }
                    else
                    {
                        // AddActionOption(cb, actions, ActionTypes.Sell);   TODO                               
                    }
                }
            }*/
        }

        private void UpdateEmbark(Allegiance ourAllegiance, IKnownEntityData terminalData, Allegiance siteAllegiance)
        {
            List<string> errors = null;
            if (!ActionExists(ActionTypes.Embark, ref errors) &&
                EmbarkActionTemplate.ValidateEmbark(ourAllegiance, terminalData, siteAllegiance, ref errors)) // BuySellActionTemplate.ValidateWorkingTerminalCanSell(missionTemplate, terminalData, ref errors))
            {
                btEmbark.Enabled = true;
                btEmbark.ToolTip = "Click to add an Embark action";

                return;
            }

            btEmbark.Enabled = false;
            btEmbark.ToolTip = "Embark is unavailable here: " + string.Join(" \n", errors);


        }

        private void UpdateSelling(IKnownEntityData terminalData)
        {
            List<string> errors = null;
            if (!ActionExists(ActionTypes.Sell, ref errors) && 
                BuySellActionTemplate.ValidateWorkingTerminalCanSell(missionTemplate, terminalData, ref errors))
            {
                btSell.Enabled = true;
                btSell.ToolTip = "Click to add a Sell action";

                return;
            }
           
            btSell.Enabled = false;
            btSell.ToolTip = "Sell is unavailable here: " + string.Join(" \n", errors);
            
        }

        private void UpdateEnabledStatus(ImageButton bt, IKnownEntityData terminalData)
        {

        }

        private bool ActionExists(ActionTypes action, ref List<string> errors)
        {
            if (missionStopTemplate.Actions.Any(a => a.ActionType == action))
            {
                Common.AddToList(ref errors, "Action already exists.");
                return true;
            }

            return false;
        }

        private void UpdateBuying(IKnownEntityData terminalData)
        {
            List<string> errors = null;

            if (!ActionExists(ActionTypes.Buy, ref errors) &&
                BuySellActionTemplate.ValidateWorkingTerminalCanBuy(missionTemplate, terminalData, ref errors))
            {
                btBuy.Enabled = true;
                btBuy.ToolTip = "Click to add a Buy action";

                return;
            }
            
            
            btBuy.Enabled = false;
            btBuy.ToolTip = "Buy is unavailable here: " + string.Join(" \n", errors);            

        }


       /* private void Populate(MissionTemplate Mission, MissionStopTemplate location, Allegiance allegiance, Expedition expedition) //bool isOwnAllegiance)
        {
            // populate with the allowed mission actions at this location
            // TODO: set enabled/disabled status
            // actions should be unique at a location
            // for now we can only load at start... this means the cargo hold will be empty.

            //bool fromAllegiance == The.InGameUI.UIAllegiance);
            Queue<MissionActionTemplate> actions = Mission.GetActionsAtLocation(location);

            //cb.Clear();

            list.BeginAddingEntries();

            if (allegiance != null)
            {
                if (allegiance == The.InGameUI.UIAllegiance) // own allegiance
                {
                    if (location.IsStart())
                    {
                        AddActionOption(actions, ActionTypes.Load);
                    }
                    else
                    {
                        AddActionOption(actions, ActionTypes.Unload);
                        AddActionOption(actions, ActionTypes.Disembark);
                    }
                }
                else // other allegiance
                {
                    if (location.IsStart())
                    {
                        AddActionOption(actions, ActionTypes.Buy);
                        AddActionOption(actions, ActionTypes.Embark);

                        //  RemoveActionOption(ActionTypes.Unload);
                        //  RemoveActionOption(ActionTypes.Disembark);
                    }
                    else
                    {
                        // AddActionOption(cb, actions, ActionTypes.Sell);   TODO                               
                    }
                }
            }

            list.EndAddingEntries();

        }*/

        private void UpdateAction(ImageButton bt)
        {


        }

        private void RemoveActionOption(ActionTypes action)
        {
            list.RemoveEntry(action);
        }

        /*
        private void AddActionOption(Queue<MissionActionTemplate> existingActions, ActionTypes action)
        {
            if (!existingActions.Any(a => a.ActionType == action))
            {
                ImageButton bt;
                CreateMissionPanel.AddAction(list, action, action, action, null, true, false, guiManager, out bt);
                bt.Click += new ClickHandler(bt_Click);
                //list.AddEntry(action, MissionActionTemplate.GetName(action));
            }
        }*/

        void bt_Click(UIComponent sender, EventArgs e)
        {
            if (ActionSelected != null)
            {
                ActionSelected.Invoke((ActionTypes)sender.Tag1);
            }
        }
    }
}
