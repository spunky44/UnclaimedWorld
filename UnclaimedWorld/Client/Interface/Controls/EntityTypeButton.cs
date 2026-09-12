using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;
using UWGame.ClientSide.Interface.HUD_Windows;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Items;
using InputEventSystem;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using UWGame.Client.Interface;
using UWGame.SimSide.AI;
using UWGame.SimSide.Processes;
using UWGame.SimSide;


namespace UWGame.ClientSide.Interface.Controls
{
    /// <summary>
    /// this button type opens a DataTypeTooltip when hovered/clicked
    /// </summary>
    public class DataTypeButton: TextButton
    {
        
        public InGameInterface.AnchorSide? SideToAnchorOn;


        public DataSheet.InfoToShow InfoToShow;

        /// <summary>
        /// if true, clicking this button will close any other tool tips
        /// </summary>
        public bool IsRoot = true;

        /// <summary>
        /// fill in either process type or entity type + entity id
        /// </summary>
        public ProcessType processType
        {
            get;
            private set;
        }
       
        public EntityType entityType;
        private EntityID? EntityID;

       
        /// <summary>
        /// owner is used to populate the tool tip with availability info
        /// </summary>
        /// <param name="gui"></param>
        /// <param name="entityType"></param>
        /// <param name="owner"></param>
        public DataTypeButton(GUIManager gui, DataSheet.InfoToShow infoToShow, EntityType entityType, EntityGroupID? owner, bool useUIOwner, EntityID? entityID = null)
            : base(gui)
        {
            Click += new ClickHandler(EntityTypeButton_Click);
           // ToolTip = entityType;
            FillEntityType(infoToShow, entityType, owner, useUIOwner, entityType.PluralName /* null*/, entityID);            
        }


        public DataTypeButton(GUIManager gui, DataSheet.InfoToShow infoToShow, ProcessType processType, EntityGroupID? owner, bool useUIOwner)
            : base(gui)
        {
            Click += new ClickHandler(EntityTypeButton_Click);
        
            FillProcessType(infoToShow, processType, owner, useUIOwner, processType.Name /* null*/);
        }

       

        public void FillEntityType(DataSheet.InfoToShow infoToShow, EntityType entityType, EntityGroupID? owner, bool useUIOwner, string newText = null, EntityID? entityID = null)
        {
            if (entityType != this.entityType || EntityID != entityID)
            {
                this.entityType = entityType;
                EntityID = entityID;
                InfoToShow = infoToShow;

                EventArgs = new DataTypeButtonEventArgs(/*entityType,*/ owner, useUIOwner);
                if (newText != null)
                {
                    Text = newText;
                }
            }
        }

        private void FillProcessType(DataSheet.InfoToShow infoToShow, ProcessType processType, EntityGroupID? owner, bool useUIOwner, string newText = null)
        {
            if (processType != this.processType)
            {
                this.processType = processType;
               
                InfoToShow = infoToShow;

                EventArgs = new DataTypeButtonEventArgs(owner, useUIOwner);
                if (newText != null)
                {
                    Text = newText;
                }
            }
        }

        public bool ShowsData(DataSheet tooltip)
        {
            EntityDataSheet entityTypeTooltip = tooltip as EntityDataSheet;
            if (entityTypeTooltip != null)
            {
                return entityTypeTooltip.EntityType == this.entityType;
            }
            else
            {
                return tooltip.ProcessType != null && tooltip.ProcessType == this.processType;
            }


        }
      
        void EntityTypeButton_Click(UIComponent sender, EventArgs e)
        {
            HandleEntityTypeButtonClick(e);
        }

        protected override void OnMouseOut(UIComponent sender, MouseEventArgs args)
        {
            if (The.Sim.Mode != Sim.EngineMode.Game)
            {
                return; // many UIs in editor can use the button to show the type name, but should not allow clicks..
            }


            if (The.InGameUI.EntityTypeTooltipsStack.Count > 0)
            {
                // did this control spawn a tool tip? (not pinned)
                DataSheet spawnedTooltip = The.InGameUI.EntityTypeTooltipsStack.Find(tt => tt.SpawningControl == this);
                
                if (spawnedTooltip != null
                    && spawnedTooltip.CurrentState == DataSheet.State.Collapsed)
                {
                    // hide it if has not been expanded and the mouse has not moved onto it:

                    if (!spawnedTooltip.MouseIsOverTooltipOrChildTooltips(args.Position /*inputData*/, 0))
                    {
                        spawnedTooltip.Hide();
                    }
                }
            }

            base.OnMouseOut(sender, args);
        }

        protected override void OnMouseOver(UIComponent sender, MouseEventArgs args)
        {
            if (The.Sim.Mode != Sim.EngineMode.Game)
            {
                return; // many UIs in editor can use the button to show the type name, but should not allow clicks..
            }

            //DataTypeTooltip shownTooltip = The.InGameUI.EntityTypeTooltipsStack.Find(tt => tt.SpawningControl == this);

            DataSheet shownTooltip = The.InGameUI.GetAnySpawnedTooltip(this);
       
            // if not already shown (collapsed or expanded):
            if (shownTooltip == null)
            {
                DataSheet tooltip = GetAndFillTooltip();
                tooltip.StartCountdownToShow(this);
            }
                            
            base.OnMouseOver(sender, args);
        }

      

        /// <summary>
        /// the player clicked an entity type. make sure that its tooltip window is shown and expanded immediately
        /// 
        /// if already expanded, a click will close it
        /// </summary>
        /// <param name="button"></param>
        /// <param name="e"></param>
        public void HandleEntityTypeButtonClick(EventArgs e)
        {
            if (The.Sim.Mode != Sim.EngineMode.Game)
            {
                return; // many UIs in editor can use the button to show the type name, but should not allow clicks..
            }


            int x, y;

            DataSheet tooltip;

            DataTypeButtonEventArgs typeArgs = e as DataTypeButtonEventArgs;


            // is the window already shown for this spawning control?
            tooltip = The.InGameUI.GetAnySpawnedTooltip(this);
            //tooltip = The.InGameUI.EntityTypeTooltipsStack.Find(tt => tt.SpawningControl == this);

            if (tooltip != null)
            {
                // the tooltip is already shown

                // is the tooltip expanded?
                if (tooltip.CurrentState != DataSheet.State.Collapsed)
                {
                    // then close it:
                    tooltip.Hide();
                }
                else
                {
                    // expand it:

                    The.InGameUI.SelectAnchorPoint(this, tooltip.DisplayWindow, SideToAnchorOn, DataSheet.ExpandedHeight, true, DataSheet.GetAnchorPointYOffset(), out x, out y);

                    tooltip.InitAndShow(this, typeArgs.Owner, typeArgs.UseUIOwner, x, y);

                    if (entityType != null)
                    {
                        EntityDataSheet entityTypeTooltip = tooltip as EntityDataSheet;
                        entityTypeTooltip.Fill(entityType, EntityID);
                    }
                    else
                    {
                        ProcessTypeDataSheet processTypeTooltip = tooltip as ProcessTypeDataSheet;
                        processTypeTooltip.Fill(processType);
                    }

                    tooltip.Expand(InfoToShow);
                }
            }
            else
            {

                if (IsRoot)
                {
                    // close other line of tooltips:
                    The.InGameUI.CloseAllEntityTooltips();
                }

                // get another tooltip and expand it:             
                tooltip = GetAndFillTooltip();

                The.InGameUI.SelectAnchorPoint(this, tooltip.DisplayWindow, SideToAnchorOn, DataSheet.ExpandedHeight, true, DataSheet.GetAnchorPointYOffset(), out x, out y);

                tooltip.InitAndShow(this, typeArgs.Owner, typeArgs.UseUIOwner, x, y);
                //SetTooltipData(tooltip);
                tooltip.Expand(InfoToShow);
            }

        }

        private DataSheet GetAndFillTooltip()
        {
            //throw new Exception("34343");


            if (entityType != null)
            {

                EntityDataSheet tooltip = The.InGameUI.poolOfEntityTypeTooltips.Get();
                tooltip.Fill(entityType, EntityID);
                
                return tooltip;
            }
            else
            {
                ProcessTypeDataSheet tooltip = The.InGameUI.poolOfProcessTypeTooltips.Get();
                tooltip.Fill(processType);

                return tooltip;
            }
           
        }

        public void SetAvailableStatusColor(bool available)
        {
            // new: set color on structures too
           /* if (this.entityType.ItemType != null)
            {*/
                LabelColor = GetStockStatusColor(available, GetNormalColor(), base.Type);
           // }
        }

        public static Color notInStockColorDark = Common.ColorFromHex("#87824c"); // "#A0985B"); // "#cac172"); 
        public static Color notInStockColorLight = Common.ColorFromHex("#cac172"); //"#87824c"); // "#A0985B"); // "#cac172"); 
        public static Color GetStockStatusColor(bool ownsItem, Color normalColor, TextButtonType textButtonType)
        {

            Color color = Color.White; 

            if (ownsItem)
            {
                color = normalColor; // Color.White;
            }
            else
            {
                if (textButtonType == TextButtonType.LCDToolTipBlack)
                {
                    color = notInStockColorDark;
                }
                else if (textButtonType == TextButtonType.HUDToolTipWhite)
                {
                    color = notInStockColorLight;
                }
                else if (textButtonType == TextButtonType.LCDAmount)
                {
                    color = notInStockColorLight;
                }
            }

            return color;
        }
    }
}
