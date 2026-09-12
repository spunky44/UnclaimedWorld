using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework.Input;
using UWGame.SimSide;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Items;
using UWGame.SimSide.Expeditions;
using UWGame.ClientSide.Interface.Controls;
using UWGame.Client.Interface;

using InputEventSystem;
using UWGame.SimSide.AI;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Snapshots;
using UWGame.ClientSide.Interface.Inventory;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Tiers;
using UWGame.SimSide.Policies;

namespace UWGame.ClientSide.Interface.HUD_Windows
{
    /// <summary>
    /// since the entity type never changes, we can populate a window for each type and keep it around forever.
    /// 
    /// - the collapsed window closes on mouse-out.
    /// - the expanded window opens on a mouse click, and only closes via the close button.
    /// </summary>
    public abstract class DataSheet : HUDWindow
    {
        /// <summary>
        /// gets set to null when pinning
        /// </summary>
        public UIComponent SpawningControl;

        protected ImageButton btPin;

        ImageButton btShowProductionInfo, btShowGeneralInfo;

        Image noProduction;

        protected TextArea summaryDescription;
        protected Label lblName;

        //   private EntityTypeTooltip childWindow;

        public enum State { Collapsed, ExpandedProduction, ExpandedData }
        private State state = State.Collapsed;

        public State CurrentState
        {
            get { return state; }
        }

        //private bool isExpanded = false;

        private double timePassed = 0f;

        // private const double timeBeforeExpanding = 1.2;

        protected const int rightMargin = 15; // 11;

        const int collapsedWidth = 256; // 224;
        const int collapsedHeight = 104; // 96;

        // const int expandedWidth = 160;
        public const int ExpandedHeight = 320;

        //   const int secondColumnWidth = 220;

        const int collapsedContentLeft = 42;
        const int summaryDescripitonX = 34;
        //  const int expandedContentHeight = 204;
        //const int contentStartY = 20;

        int expandedContentStart;

        //const int maxExpandedContentHeightBeforeScrollAreasAppear = 180; // 204;
        int maxExpandedContentHeightBeforeScrollAreasAppear = 180;


        protected const int itemHeight = 22;


        public bool CanExpand = true;

        /// <summary>
        /// this is placed at the top of the expanded panel, outside the grid
        /// </summary>
        protected Label lblExpandedHeading;
        // Image imExpandedIcon;


        /// <summary>
        /// these are outer grids that are used to mimic flow layout in html. one of them is added to the content view port at a time. 
        /// 
        /// entries are sorted by Tag (integer)
        /// </summary>
        protected Grid grdProductionOuter, grdGeneralOuter;

        private Grid currentlyShownExpandedGrid;

        /// <summary>
        /// this control contains one of the two grids and shows a portion of it
        /// </summary>
        UIComponent expandedContentViewPort;

        ImageButton btScrollDown, btScrollUp;

      

        #region Data

        TextArea taDescription;


        #endregion


        #region Production

        const float policyIndex = 5f;
        const float skillIndex = 10f;
        const float actingOnIndex = 15f;
        const float gatheredFromIndex = 18f;
        const float inputIndex = 20f;
        const float toolsIndex = 50f;


        Grid grdInputs; //, grdTools;

        Label lblMadeFrom;
        Icon icMadeFrom;
        UIComponent madeFromHeader;
        UIComponent skillHeader;
        UIComponent actingOnHeader;
        UIComponent gatheredFromHeader;
       
        /// <summary>
        /// for now, there can be up to 3 grids
        /// </summary>
        //List<Tuple<ToolAlternatives, Grid>> toolGrids = new List<Tuple<ToolAlternatives, Grid>>();

        List<ToolGrid> allToolGrids = new List<ToolGrid>();

        Dictionary<ToolAlternatives, ToolGrid> currentToolGrids = new Dictionary<ToolAlternatives, ToolGrid>();

        //  Dictionary<Grid, int> toolGridsScrollPositions = new Dictionary<Grid, int>();
        // Dictionary<Grid, ImageButton> toolGridsUpButton = new Dictionary<Grid, ImageButton>();


        private bool showAllTools;

        Label lblSkill, lblGatheredFrom;

              

        protected UIComponent policyHeader;
        Icon policyIcon;

        /// <summary>
        /// contains the DataType button
        /// </summary>
        UIComponent actingOn;
       

        #endregion


        /// <summary>
        /// data that depends on the entity (instance), not the EntityType
        /// </summary>
        private EntityTypeTooltipInstanceData instanceData;

        #region Owner

        // either pouplate based on what the area of the map the player is over, or populate from a fixed owner
        // if none is valid, results are undefined...?

        /// <summary>
        /// can be null
        /// </summary>
        protected EntityGroupID? owner;


        // if true, the UIOwner will be used
        protected bool useCurrentUIOwner = false;

        /// <summary>
        /// don't use this when spawning more entity type buttons and tooltips!
        /// Only when calculating availability!
        /// 
        /// Returns false if there was a problem resolving the owner.
        /// </summary>
        /// <returns></returns>
        protected bool ResolveOwner(out EntityGroup resolvedOwner)
        {
            if (useCurrentUIOwner)
            {
                resolvedOwner = LookUp<EntityGroup, EntityGroupID>.FindByID(The.InGameUI.UIOwner);
            }
            else
            {
                resolvedOwner = LookUp<EntityGroup, EntityGroupID>.FindByID(owner);
            }

            return resolvedOwner != null; // not sure if the tooltip should work outside UIOwner areas???
        }

        #endregion

        /*{
            set
            {
                entityType = value;
                
            }
        }*/

        /// <summary>
        /// these panels can be taller than the viewport - they are moved when the user clicks on 'More'
        /// </summary>
        //  UIComponent pnProduction, pnGeneral;

        //   UIComponent viewport;




        protected Image icon;

        public const int expandButtonY = 44; // 36;
        public const int expandButtonHeight = 18;

        const int scrollAreaHeight = 15; // 18;


        public Color entityToolTipHeaderColor;
        //  ImageButton btEncyclopedia;

        protected int expandedPanelHeadingY = collapsedHeight + topMargin;


        protected static Color productionColor = Common.ColorFromHex("FFA83E");

        /// <summary>
        /// creates a panel with the relevant fields for this entity type
        /// </summary>
        /// <param name="entityType"></param>
        public DataSheet()
            : base(collapsedWidth, collapsedHeight, true, false, true /*false*/, "HUD_windowInfo_base", level: Level.EntityTypeInfo)
        {
            DisplayWindow.SetResizableArea(ResizeAreas.Top, true);
            DisplayWindow.SetResizableArea(ResizeAreas.Bottom, true);

            DisplayWindow.MinHeight = collapsedHeight; 
            DisplayWindow.ResizableBorderSize = 6;
            DisplayWindow.Resize += DisplayWindow_Resize;
            

            base.DisplayWindow.ViewPort.MouseOut += new MouseOutHandler(ViewPort_MouseOut);

            base.DisplayWindow.DebugTag = "DataTypeTooltip";

            // processTypeToShow = GetProcessToShow();
            showAllTools = The.Sim.Controller.Options.ShowAllTools;

            // to handle fades
            UpdateWhileHidden = true;


            icon = new Image(gui);
            icon.Texture = gui.GUISpriteSheet.Texture;
            Add(icon);
            icon.X = sideMargin;
            icon.Y = topMargin;


            lblName = new Label(gui);
            //  lblName.Text = EntityType.PluralName;
            lblName.X = collapsedContentLeft;
            lblName.Y = topMargin;
            Add(lblName);
            lblName.Width = DisplayWindow.Width - collapsedContentLeft - 6;
            lblName.Init(Label.LabelType.EntityTypeTooltipHeader);
            lblName.ID = UIComponent.DataControlID.Caption;

            summaryDescription = new TextArea(gui, ListBoxType.HUDAndLCD);
            Add(summaryDescription); // add calls initialize, that sets font...
            summaryDescription.Init(Label.LabelType.HUDWindow);
            summaryDescription.X = collapsedContentLeft; // summaryDescripitonX;
            summaryDescription.Y = lblName.Bottom + singleSpacing; // doubleSpacing;
            summaryDescription.Width = DisplayWindow.Width - collapsedContentLeft - 6; // 196; // triggers BreakText that requires font to be set
            summaryDescription.Height = DisplayWindow.Height - 2 * topMargin - 10;
            summaryDescription.HMargin = 0;
            summaryDescription.ZOrder = 1f;

            noProduction = new Image(gui);
            // Add(btShowProductionInfo);
            noProduction.Texture = gui.GUISpriteSheet.Texture;
            noProduction.SetSkinLocation(SkinState.Normal,gui.GUISpriteSheet.GetSourceRectangle("HUD_info_button_productionUnavailable"));
            noProduction.ResizeControlToFitImage();
            noProduction.X = sideMargin + 2;
            noProduction.Y = expandButtonY;
            noProduction.ToolTip = "No production info available for this";

            btShowProductionInfo = new ImageButton(gui);
            Add(btShowProductionInfo);
            btShowProductionInfo.Init(ImageButtonType.HUDShowProductionInfo);
            btShowProductionInfo.X = noProduction.X;
            btShowProductionInfo.Y = expandButtonY;
            btShowProductionInfo.Click += new ClickHandler(btShowProductionInfo_Click);
            btShowProductionInfo.ToolTip = "See production/process information about this"; //was: "See how this item can be produced"
            btShowProductionInfo.CheckedMode = CheckedModes.CanBeChecked;

            btShowGeneralInfo = new ImageButton(gui);
            Add(btShowGeneralInfo);
            btShowGeneralInfo.Init(ImageButtonType.HUDShowGeneralInfo);
            btShowGeneralInfo.X = noProduction.X;
            btShowGeneralInfo.Y = expandButtonY + expandButtonHeight + singleSpacing;
            btShowGeneralInfo.Click += new ClickHandler(btShowGeneralInfo_Click);
            btShowGeneralInfo.ToolTip = "See general data/info about this"; //was: see how this item can be produced
            btShowGeneralInfo.CheckedMode = CheckedModes.CanBeChecked;

            btPin = new ImageButton(gui);
            Add(btPin);
            btPin.Init(ImageButtonType.HUDPin);
            btPin.CheckedMode = CheckedModes.SwitchCheckedStateOnClick;
            btPin.X = DisplayWindow.Width - singleSpacing - btPin.Width;
            btPin.Y = 58;
            btPin.Click += btPin_Click;
            UpdatePinButton();


            /* viewport = new UIComponent(gui);
             viewport.Width = DisplayWindow.Width;
             viewport.Height = expandedHeight - collapsedHeight;
             Add(viewport);
             viewport.Y = collapsedHeight;
             */


            // expanded panel starts here
            //*******

           
            lblExpandedHeading = new Label(gui);
            Add(lblExpandedHeading);
            lblExpandedHeading.Init(Label.LabelType.EntityTypeTooltipSubHeading);
            lblExpandedHeading.X = SideMarginOutsideGrid();
            lblExpandedHeading.Y = expandedPanelHeadingY;


            /*btEncyclopedia = new ImageButton(gui);
            Add(btEncyclopedia);
            btEncyclopedia.Init(ImageButtonType.HUDShowGeneralInfo);
            btEncyclopedia.X = DisplayWindow.Width - 28;
            btEncyclopedia.Y = expandedPanelHeadingY;
            btEncyclopedia.Click += new ClickHandler(btEncyclopedia_Click);
            */

            /*  imExpandedIcon = new Image(gui);
              Add(imExpandedIcon);
              imExpandedIcon.Texture = gui.GUISpriteSheet.Texture;
              imExpandedIcon.SetSkinLocation(SkinState.Normal,gui.GUISpriteSheet.GetSourceRectangle("HUD_icon_changeLayout")); // placeholder image
              imExpandedIcon.ResizeControlToFitImage();
              imExpandedIcon.X = sideMargin;
              imExpandedIcon.Y = expandedPanelHeadingY;
              */

            btClose = new ImageButton(gui);
            Add(btClose);
            btClose.Init(ImageButtonType.HUDClose);
            btClose.X = DisplayWindow.Width - singleSpacing - btClose.Width;
            btClose.Y = 5;// expandedPanelHeadingY;
            btClose.Click += new ClickHandler(btClose_Click);

            lblName.MaxWidth = btClose.X - 25 - lblName.X;
            summaryDescription.MaxWidth = DisplayWindow.Width - 2 * summaryDescription.X;

            // need room for scroll arrow above!
            expandedContentStart = collapsedHeight + 32; 

            expandedContentViewPort = new UIComponent(gui);
            expandedContentViewPort.Y = expandedContentStart;
            expandedContentViewPort.Width = DisplayWindow.Width;
            expandedContentViewPort.Height = maxExpandedContentHeightBeforeScrollAreasAppear; // expandedHeight - expandedContentStart - expandButtonHeight - doubleSpacing; // expandedContentHeight;
            Add(expandedContentViewPort);

            btScrollDown = new ImageButton(gui);
            //  Add(btScrollDown);
            btScrollDown.Init(ImageButtonType.HUDInfoScrollDown);
            btScrollDown.X = (DisplayWindow.Width - btScrollDown.Width) / 2;
            btScrollDown.Y = expandedContentViewPort.Bottom + singleSpacing; // expandButtonY + expandButtonHeight + singleSpacing;
            //   btShowGeneralInfo.Click += new ClickHandler(btShowGeneralInfo_Click);
            btScrollDown.Height = scrollAreaHeight;
            //  btScrollDown.Width = DisplayWindow.Width - 2 * sideMargin;
            btScrollDown.MouseOver += new MouseOverHandler(btScrollDown_MouseOver);
            btScrollDown.MouseOut += new MouseOutHandler(btScrollDown_MouseOut);


            btScrollUp = new ImageButton(gui);
            //  Add(btScrollDown);
            btScrollUp.Init(ImageButtonType.HUDInfoScrollUp);
            btScrollUp.X = (DisplayWindow.Width - btScrollUp.Width) / 2;
            btScrollUp.Y = expandedContentStart - expandButtonHeight + 5; // expandedContentViewPort.Bottom + singleSpacing; // expandButtonY + expandButtonHeight + singleSpacing;
            //   btShowGeneralInfo.Click += new ClickHandler(btShowGeneralInfo_Click);
            btScrollUp.Height = scrollAreaHeight;
            // btScrollUp.Width = DisplayWindow.Width - 2 * sideMargin;
            btScrollUp.MouseOver += new MouseOverHandler(btScrollUp_MouseOver);
            btScrollUp.MouseOut += new MouseOutHandler(btScrollUp_MouseOut);


            // don't populate these yet... just create the controls
            CreateProductionPanel();

            CreateGeneralPanel();
        }

        private void SetVerticalPositions()
        {

           // maxExpandedContentHeightBeforeScrollAreasAppear

            AdaptContentToWindow();

        }

        void DisplayWindow_Resize(UIComponent sender)
        {
            SetVerticalPositions();
        }


        void btPin_Click(UIComponent sender, EventArgs e)
        {
            UpdatePinButton();
        }

        private void UpdatePinButton()
        {
            if (btPin.IsChecked)
            {
                btPin.ToolTip = "Unpin this window so it will be hidden as normal";
                The.InGameUI.PinnedDataTypeTooltips.Add(this);
                The.InGameUI.UnpinnedDataTypeTooltipsOutsideStack.Remove(this);

                SpawningControl = null;
            }
            else
            {
                btPin.ToolTip = "Pin this window so it stays open";
                The.InGameUI.PinnedDataTypeTooltips.Remove(this);

                if (this.DisplayWindow.IsVisibleAndActive)
                {
                    The.InGameUI.UnpinnedDataTypeTooltipsOutsideStack.Add(this);
                }
            }
        }




        /*
        public void SetTooltipData(EntityType entityType, EntityID? entityID)
        {
            if (entityID.HasValue)
            {
                SharedKnowledge sharedKnowledge = The.InGameUI.UIAllegiance.SharedKnowledge;
                IKnownEntityData knownEntity;
                sharedKnowledge.GetKnownData(entityID.Value, out knownEntity);
                if (knownEntity != null)
                {
                    instanceData = knownEntity.TooltipEntityData;
                }
                else
                {
                    instanceData = null;
                }
            }
            else
            {
                instanceData = null;
            }

            EntityType = entityType;
        }*/

        void btEncyclopedia_Click(UIComponent sender, EventArgs e)
        {

        }

        void btClose_Click(UIComponent sender, EventArgs e)
        {
            // unpin:
            btPin.IsChecked = false;

            Hide();
        }

        public virtual void OnSetProduction()
        {

        }

        public static int GetAnchorPointYOffset()
        {
            return -(int)(expandButtonY + 0.5 * expandButtonHeight);
        }

        void btScrollUp_MouseOut(UIComponent sender, InputEventSystem.MouseEventArgs args)
        {
            mouseOverScrollUpArea = false;
        }

        void btScrollUp_MouseOver(UIComponent sender, InputEventSystem.MouseEventArgs args)
        {
            mouseOverScrollUpArea = true;
        }

        void btScrollDown_MouseOut(UIComponent sender, InputEventSystem.MouseEventArgs args)
        {
            mouseOverScrollDownArea = false;
        }

        bool mouseOverScrollDownArea, mouseOverScrollUpArea = false;

        void btScrollDown_MouseOver(UIComponent sender, InputEventSystem.MouseEventArgs args)
        {
            mouseOverScrollDownArea = true;

        }

        public static float scrollSpeedPerSecond = 200f;

        private void UpdateScrolling(GameTime elapsed)
        {
            if (mouseOverScrollDownArea)
            {
                ScrollDownGrid(elapsed, currentlyShownExpandedGrid);
            }
            else if (mouseOverScrollUpArea)
            {
                ScrollUpGrid(elapsed, currentlyShownExpandedGrid);
            }


            /*
            switch (state)
            {
                case State.ExpandedData:
            
                    ScrollDownGrid(elapsed, grdGeneralOuter);

                    break;
                case State.ExpandedProduction:

                    ScrollDownGrid(elapsed, grdProductionOuter);

                    break;
            }*/
        }

        private void AdaptContentToWindow()
        {
            if (currentlyShownExpandedGrid != null)
            {
                int maxExpandedContentHeightBeforeScrollAreasAppear = DisplayWindow.Height - expandedContentViewPort.Y - 24;

                expandedContentViewPort.Height = maxExpandedContentHeightBeforeScrollAreasAppear;

                // if the grid is too large, show the scroll areas
                if (currentlyShownExpandedGrid.Height > maxExpandedContentHeightBeforeScrollAreasAppear)
                {
                   // expandedContentViewPort.Height = maxExpandedContentHeightBeforeScrollAreasAppear;



                  /*  currentlyShownExpandedGrid.Y -= (int)(elapsed.ElapsedGameTime.TotalSeconds * scrollSpeedPerSecond);

                    if (IsAtBottom(currentlyShownExpandedGrid))
                    {
                        // we are at the bottom now. scroll no further.                
                        currentlyShownExpandedGrid.Y = expandedContentViewPort.Height - grid.Height;

                        HideScrollDownArea();
                    }

                    */

                    if (!mouseOverScrollDownArea && !mouseOverScrollUpArea)
                    {
                        if (currentlyShownExpandedGrid.Y < 0 // currentlyShownExpandedGrid.Bottom == expandedContentViewPort.Height) // !IsAtBottom(currentlyShownExpandedGrid))
                            && currentlyShownExpandedGrid.Bottom < expandedContentViewPort.Height)
                        {
                           /* if (!IsAtTop(currentlyShownExpandedGrid))
                            // && currentlyShownExpandedGrid.Bottom < )
                            // && currentlyShownExpandedGrid.Height < expandedContentViewPort.Height) 
                            {*/
                                // show more of the content grid so we don't leave blank space beneath it:
                                int newGridBottomYPos = expandedContentViewPort.Height;

                                int newYPos = newGridBottomYPos - currentlyShownExpandedGrid.Height; // grid.Bottom <= expandedContentViewPort.Height
                                newYPos = Common.ClampTop(newYPos, 0);
                                currentlyShownExpandedGrid.Y = newYPos;
                           // }
                        }
                    }


                    if (!IsAtBottom(currentlyShownExpandedGrid))
                    {
                        
                        ShowScrollDownArea();

                        btScrollDown.Y = expandedContentViewPort.Bottom + singleSpacing; 
                    }
                    else
                    {
                        HideScrollDownArea();
                    }

                    // see if a scroll up area should be added:
                    if (!IsAtTop(currentlyShownExpandedGrid)) // only after we have scrolled down a bit
                    {
                        ShowScrollUpArea();
                    }
                    else
                    {
                        // hide the scoll up area
                        HideScrollUpArea();
                    }
                }
                else
                {
                    // the content grid is less than the max height. resize the viewport to fit:
                   // expandedContentViewPort.Height = currentlyShownExpandedGrid.Height;

                    HideScrollUpArea();
                    HideScrollDownArea();
                }
            }
        }

        /// <summary>
        /// resize the window to fit the content and show scroll areas if necessary
        /// </summary>
      /*  private void AdaptWindowToContent()
        {
           
            if (currentlyShownExpandedGrid != null)
            {
                // if the grid is too large, show the scroll areas
                if (currentlyShownExpandedGrid.Height > maxExpandedContentHeightBeforeScrollAreasAppear) 
                {
                    expandedContentViewPort.Height = maxExpandedContentHeightBeforeScrollAreasAppear;

                    // make room for scroll area:
                    //  DisplayWindow.Height = expandedContentViewPort.Bottom + singleSpacing + scrollAreaHeight + doubleSpacing;

                    if (!IsAtBottom(currentlyShownExpandedGrid))
                    {
                        ShowScrollDownArea();
                    }
                    else
                    {
                        HideScrollDownArea();
                    }

                    // see if a scroll up area should be added:
                    if (!IsAtTop(currentlyShownExpandedGrid)) // only after we have scrolled down a bit
                    {
                        ShowScrollUpArea();
                    }
                    else
                    {
                        // hide the scoll up area
                        HideScrollUpArea();
                    }
                }
                else
                {
                    // the content grid is less than the max height. resize the viewport to fit:
                    expandedContentViewPort.Height = currentlyShownExpandedGrid.Height;

                    HideScrollUpArea();
                    HideScrollDownArea();

                }

                DisplayWindow.Height = expandedContentViewPort.Bottom + singleSpacing + scrollAreaHeight + doubleSpacing;
            }
        }*/

        private void ShowScrollUpArea()
        {
            if (!DisplayWindow.Contains(btScrollUp))
            {
                Add(btScrollUp);
            }
        }

        private void ShowScrollDownArea()
        {
            if (!DisplayWindow.Contains(btScrollDown))
            {
                Add(btScrollDown);
            }
        }

        private void HideScrollUpArea()
        {
            Remove(btScrollUp);

            mouseOverScrollUpArea = false;
        }

        private void HideScrollDownArea()
        {
            Remove(btScrollDown);

            mouseOverScrollDownArea = false;
        }



        private void ScrollUpGrid(GameTime elapsed, Grid grid)
        {
            grid.Y += (int)(elapsed.ElapsedGameTime.TotalSeconds * scrollSpeedPerSecond);

            if (IsAtTop(grid))
            {
                // we reached the top. hide the control and resize
                grid.Y = 0;

                HideScrollUpArea();
            }


            ShowScrollDownArea();

        }

        private bool IsAtTop(Grid grid)
        {
            return grid.Y >= 0;
        }

        private bool IsAtBottom(Grid grid)
        {
            return grid.Bottom <= expandedContentViewPort.Height;
        }

        private void ScrollDownGrid(GameTime elapsed, Grid grid)
        {
            grid.Y -= (int)(elapsed.ElapsedGameTime.TotalSeconds * scrollSpeedPerSecond);

            if (IsAtBottom(grid))
            {
                // we are at the bottom now. scroll no further.                
                grid.Y = expandedContentViewPort.Height - grid.Height;

                HideScrollDownArea();
            }

            // make sure the scroll up area is displayed:
            ShowScrollUpArea();
        }

        /// <summary>
        /// DataType is always just 1 process
        /// </summary>
        public ProcessType ProcessType
        {
            get
            {
                return processTypeToShowProductionFor;
            }        
        }

        /// <summary>
        /// when more than one process can produce this type, we only show one...
        /// </summary>
        protected ProcessType processTypeToShowProductionFor;

        protected const int extraSideMargin = 5; // 10;

        const string madeFromTooltip = "We need all of the below materials, in the given amounts";
        const string gatheredTooltip = "This item is gathered from a resource";

        private void CreateProductionPanel()
        {
            //  if (processTypeToShow == null)
            //       return;                       

            grdProductionOuter = CreateOuterGridForCollapsableLists(The.InGameUI.gui, DisplayWindow.ViewPort, false);
            grdProductionOuter.ScrollBarEnabled = false;
            grdProductionOuter.CanGrowInHeight = true; // false; // true;
            grdProductionOuter.DebugTag = "outerProdGrid";
            grdProductionOuter.BeginAddingEntries();

            madeFromHeader = AddSubHeader(grdProductionOuter, "MADE FROM:", "HUD_icon_stockpile", 1, 5, out lblMadeFrom, out icMadeFrom, topPadding: 0, itemHeight: 9); // 0 padding at the very top
            madeFromHeader.OrderByTag1 = inputIndex; // for sorting

            grdInputs = CreateFixedItemHeightGrid();
            grdInputs.OrderByTag1 = inputIndex + 1; // for sorting

            grdProductionOuter.AddEntry(grdInputs, grdInputs);


            // pnProduction.Add(grdInputs);
            Icon icon;
            Label lbl;

            policyHeader = AddSubHeader(grdProductionOuter, "POLICY:", "lcd_icon_section", -2, 0, out lbl, out icon, topPadding: 0, itemHeight: 18);
            policyHeader.OrderByTag1 = policyIndex; // for sorting
            lbl.ToolTip = "Requires a policy to be enacted";
            policyIcon = new Icon(gui);
            policyHeader.Add(policyIcon);


          //  skillHeader = AddSubHeader(grdProductionOuter, "REQUIRED SKILL:", "HUD_icon_person", -7, 1, out lbl, out icon, topPadding: 0, itemHeight: 25); // icon is large, so set the item height
            skillHeader = AddSubHeader(grdProductionOuter, "REQUIRED SKILL:", "HUD_icon_person", -7, -4, out lbl, out icon, topPadding: 0, itemHeight: 18); // icon is large, so set the item height
            skillHeader.OrderByTag1 = skillIndex; // for sorting
            lbl.ToolTip = "Requires a character with sufficient level in this skill (more than 0.1)";  //was     only a character with the required skill can start this

            lblSkill = new Label(gui);
            lblSkill.Init(Label.LabelType.EntityTypeTooltip);
            lblSkill.X = 48; // sideMargin + extraSideMargin;
            grdProductionOuter.AddEntry(lblSkill, lblSkill);
            lblSkill.OrderByTag1 = skillIndex + 1;  // for sorting


            actingOnHeader = AddSubHeader(grdProductionOuter, "SPECIAL LOCATION:", "HUD_icon_star", -7, 1, out lbl, out icon, itemHeight: 25, addToGrid: false); 
            actingOnHeader.OrderByTag1 = actingOnIndex; // for sorting
            lbl.ToolTip = "Can only be built at a special location, by using its action menu";  

            // the data row is created late, we need the entity type...
            
            gatheredFromHeader = AddSubHeader(grdProductionOuter, "GATHERED FROM:", "HUD_icon_gather", -1, 11, out lbl, out icon, itemHeight: 25, addToGrid: false);
           // gatheredFromHeader = AddSubHeader(grdProductionOuter, "GATHERED FROM:", "HUD_icon_gather", 0, 11, out lbl, out icon, topPadding: 0, itemHeight: 18, addToGrid: false);
            gatheredFromHeader.OrderByTag1 = gatheredFromIndex; // for sorting
            lbl.ToolTip = "Needs to be gathered from a resource"; // "Can only be built at a special location";  

            lblGatheredFrom = new Label(gui);
            lblGatheredFrom.Init(Label.LabelType.EntityTypeTooltip);
            lblGatheredFrom.X = lblSkill.X; // sideMargin + extraSideMargin;
           // grdProductionOuter.AddEntry(lblGatheredFrom, lblSkill);
            lblGatheredFrom.OrderByTag1 = gatheredFromIndex + 1;  // for sorting

            /*
            policyHeader = AddSubHeader(grdProductionOuter, "POLICY:", "lcd_icon_section", -7, -4, out lbl, out icon, topPadding: 0, itemHeight: 18);
            policyHeader.OrderByTag1 = policyIndex; // for sorting
            lbl.ToolTip = "Requires a policy to be enacted";  
            */

            // create 3 tool grids:
            for (int i = 0; i < 3; i++)
            {
                CreateToolGrid(i);
            }


            CreateProductionPanelContents();

            grdProductionOuter.EndAddingEntries();

        }


        protected int SideMarginOutsideGrid()
        {
            return sideMargin + extraSideMargin;
        }

        /// <summary>
        /// not just for entity types
        /// </summary>
        /// <returns></returns>
        protected Grid CreateFixedItemHeightGrid(int? itemHeight = null)
        {
            Grid grid = new Grid(gui, ListBoxType.HUDAndLCD, WindowSystem.Label.LabelType.EntityTypeTooltip);
            grid.IsOuterGrid = false;
            // grdInputs.DebugTag = "grdInputs";
            grid.X = sideMargin;
            grid.FixedItemHeights = true; // false;
            grid.Width = grdProductionOuter.Width; // listSurface.Width; // make grid fill the panel           
            grid.ScrollBarEnabled = false;
            grid.ItemHeight = itemHeight ?? 22; 
            grid.CanGrowInHeight = true;
            grid.Font = GUIManager.LCDandHUDBodyFontPath;
            grid.Height = 160; // 40; // 160

            return grid;
        }


        protected virtual void CreateProductionPanelContents()
        { }

        /// <summary>
        ///  creates a label surrounded by a component for extra vertical padding within the grid
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="text"></param>
        /// <param name="addToGrid"></param>
        /// <returns></returns>
        protected UIComponent AddSubHeader(Grid grid, string text, out Label lbl, bool addToGrid = true, Color? labelColor = null)
        {
            Icon icon;
            return AddSubHeader(grid, text, null, 0, 0, out lbl, out icon, addToGrid, labelColor: labelColor);
        }

        /// <summary>
        /// creates a label surrounded by a component for extra vertical padding within the grid
        /// 
        /// 0 top padding always???
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="text"></param>
        /// <param name="lbl"></param>
        /// <param name="addToGrid"></param>
        /// <returns></returns>
        protected UIComponent AddSubHeader(Grid grid, string text, string sprite, int iconXPos, int iconYPos, out Label lbl, out Icon icon, bool addToGrid = true, int topPadding = singleSpacing, int? itemHeight = null, Color? labelColor = null)
        {
            icon = null;

            int margin = sideMargin + extraSideMargin;
            lbl = new Label(gui);
            lbl.Init(Label.LabelType.EntityTypeTooltip);
            lbl.Text = text; //;
            lbl.FitToText();
            lbl.X = margin;
            UIComponent paddingBox = new UIComponent(gui);
            paddingBox.Height = itemHeight ?? lbl.TextHeight + topPadding; // singleSpacing;
            paddingBox.Add(lbl);
            lbl.Y = topPadding; // singleSpacing;
            if (labelColor != null)
            {
                lbl.NormalColor = labelColor.Value;
            }


            if (sprite != null)
            {
                icon = new Icon(gui);
                icon.SetSkinLocation(SkinState.Normal,gui.GUISpriteSheet.GetSourceRectangle(sprite));              
                icon.ScaleImageToSizeOfControl = false;
                icon.ResizeControlToFitImage();
                // reduce the height if needed:
                if (itemHeight.HasValue && icon.Height > itemHeight.Value)
                {
                    icon.Height = itemHeight.Value;
                }

                paddingBox.Add(icon);

              //  paddingBox.CenterChildVertically(icon, lbl.Y + lbl.TextHeight / 2);
                
                icon.Y = iconYPos;

                icon.X = margin + iconXPos;

                lbl.X = margin + 14; // icon.Right;
            }

            if (addToGrid)
            {
                // this will set paddingbox height to the height of its contents, if larger...
                grid.AddEntry(paddingBox, paddingBox); 
            }

            return paddingBox;
        }




        private class ToolScrollEventArgs : EventArgs
        {
            // public Grid ToolGrid;
            public int Index;
            // public ToolAlternatives Tools;
        }

        static Color cyan = Common.ColorFromHex("0FF8FD");

        private void CreateToolGrid(int index) //, ToolAlternatives tools)
        {
            Label lbl;
            Icon icon;
            UIComponent header;// = AddSubHeader("TOOL OPTIONS #" + (index + 1), out lbl, false);
            switch (index)
            {
                case 0:
                    header = AddSubHeader(grdProductionOuter, GameData.Instance.GUIConstants.FirstToolOption, "HUD_icon_tool", 0, 8, out lbl, out icon, false);
                    break;
                case 1:
                    header = AddSubHeader(grdProductionOuter, GameData.Instance.GUIConstants.SecondToolOption, "HUD_icon_tool", 0, 8, out lbl, out icon, false);
                    break;
                case 2:
                    header = AddSubHeader(grdProductionOuter, GameData.Instance.GUIConstants.ThirdToolOption, "HUD_icon_tool", 0, 8, out lbl, out icon, false);
                    break;
                default:
                    header = AddSubHeader(grdProductionOuter, "More than 3 tool options." + (index + 1), out lbl, false);
                    icon = null;
                    break;

            }

            if (icon != null)
            {
                icon.Color = cyan;
            }
            lbl.NormalColor = cyan;
            lbl.ToolTip = "Needs one of the tools from the group below. Expand the list to see more tool options. NOTE: There can be even more options than the ones shown"; //was: "Needs one of the below tools"

            ImageButton btExpand = new ImageButton(The.InGameUI.gui);
            header.Add(btExpand);
            btExpand.Init(ImageButtonType.HUDExpandCollapseTinted); // HUDArrowUp);
            btExpand.ID = UIComponent.DataControlID.Expand;          
            btExpand.X = 220;
            btExpand.Y = lbl.Y;            
            // btUp.Y = expandButtonY + expandButtonHeight + singleSpacing;
            btExpand.Click += btExpandTools_Click;
            btExpand.ToolTip = "Expand the list of tools";
            btExpand.CheckedMode = CheckedModes.SwitchCheckedStateOnClick;
            btExpand.NormalColor = cyan;
            btExpand.DebugTag = "btExpand";


            // add the buttons to scroll the tool options:
            // DISABLED for now
            ImageButton btUp = null, btDown = null;

            if (showAllTools)
            {
                btUp = new ImageButton(The.InGameUI.gui);
                header.Add(btUp);
                btUp.Init(ImageButtonType.HUDArrowUp);
                btUp.ID = UIComponent.DataControlID.Up;
                btUp.X = /*lblHeader.Right*/ 110 + singleSpacing;
                // btUp.Y = expandButtonY + expandButtonHeight + singleSpacing;
                btUp.Click += new ClickHandler(btScrollToolsUp_Click);
                btUp.ToolTip = "Scroll up in the list of tools";


                btDown = new ImageButton(The.InGameUI.gui);
                header.Add(btDown);
                btDown.Init(ImageButtonType.HUDArrowDown);
                btDown.ID = UIComponent.DataControlID.Down;
                btDown.X = btUp.Right + doubleSpacing;
                // btUp.Y = expandButtonY + expandButtonHeight + singleSpacing;
                btDown.Click += new ClickHandler(btDown_Click);
                btDown.ToolTip = "Scroll down in the list of tools";
            }

            //  header.Height = lblHeader.Height;
            //  grdProductionOuter.AddEntry(tools, header); // use the tools set as key for the grid row


            Grid grdTools = new Grid(gui, ListBoxType.HUDAndLCD, WindowSystem.Label.LabelType.EntityTypeTooltip);
            grdTools.IsOuterGrid = false; // true; // false;
            grdTools.DebugTag = "grdTools";
            // categoryGrid.Position = new Point(
            // categoryGrid.DebugTag = "categoryGrid";
            grdTools.X = sideMargin;
            //  grdTools.Y = 48 + topMargin;
            grdTools.FixedItemHeights = true; // false;
            grdTools.Width = grdProductionOuter.Width; // listSurface.Width; // make grid fill the panel           
            grdTools.ScrollBarEnabled = false;
            grdTools.ItemHeight = 22; // 22;
            grdTools.CanGrowInHeight = true; // false; // true;            
            grdTools.Font = GUIManager.LCDandHUDBodyFontPath;
            grdTools.Height = 160; // 40; // 160

            if (showAllTools)
            {
                ToolScrollEventArgs eventArgs = new ToolScrollEventArgs() { Index = index }; 
                btDown.EventArgs = eventArgs;
                btUp.EventArgs = eventArgs;
            }

            // store the grid
            allToolGrids.Add(new ToolGrid() { Grid = grdTools, ScrollPosition = 0, btScrollDown = btDown, btScrollUp = btUp, HeaderRow = header });
            
        }

        void btExpandTools_Click(UIComponent sender, EventArgs e)
        {
           // ToolGrid toolGrid = allToolGrids[((ToolScrollEventArgs)e).Index];

            if (((ImageButton)sender).IsChecked)
            {
                sender.ToolTip = "Collapse the list of tools";
            }
            else
            {
                sender.ToolTip = "Expand the list of tools";          
            }

            PopulateProductionDataRefresh();

        }

        void btDown_Click(UIComponent sender, EventArgs e)
        {
            ScrollTools(e, 1);
        }

        void btScrollToolsUp_Click(UIComponent sender, EventArgs e)
        {
            ScrollTools(e, -1);

            //toolGrids.Remove(gridInfo);

            // cannot modify a tuple
            /* toolGrids.Add(new Tuple<ToolAlternatives, Grid, int>
                 (tools, gridInfo.Item2, newIndex));*/
            //  }
        }

        private void ScrollTools(EventArgs e, int increase)
        {
            //ToolAlternatives tools = ((ToolScrollEventArgs)e).Tools;
            //   ToolAlternatives tools = //allt ((ToolScrollEventArgs)e).Index;

            //   ToolGrid toolGrid = currentToolGrids[tools];

            ToolGrid toolGrid = allToolGrids[((ToolScrollEventArgs)e).Index]; // currentToolGrids[tools];


            int currentIndex = toolGrid.ScrollPosition; // gridInfo.Item3;

            int newIndex = currentIndex + increase;

            toolGrid.ScrollPosition = newIndex;
        }

        protected virtual void CreateGeneralPanelContents()
        {


        }

        private void CreateGeneralPanel()
        {
           
            grdGeneralOuter = CreateOuterGridForCollapsableLists(The.InGameUI.gui, DisplayWindow.ViewPort, false);
            grdGeneralOuter.ScrollBarEnabled = false;
            grdGeneralOuter.CanGrowInHeight = true; // false; // true;

            grdGeneralOuter.BeginAddingEntries();

            taDescription = new TextArea(gui, ListBoxType.HUDAndLCD); // text appears whiter - why?
            grdGeneralOuter.AddEntry(taDescription, taDescription);
            taDescription.Init(Label.LabelType.HUDWindow);         
            taDescription.CanGrowInHeight = true;
            taDescription.ZOrder = 1f;
            taDescription.X = SideMarginOutsideGrid(); // sideMargin + extraSideMargin - 3;
            taDescription.Width = collapsedWidth - 2 * taDescription.X;
            taDescription.OrderByTag1 = 0f;

            CreateGeneralPanelContents();


            grdGeneralOuter.EndAddingEntries();

        }

        void btShowGeneralInfo_Click(UIComponent sender, EventArgs e)
        {
            if (state != State.ExpandedData)
            {
                Expand(InfoToShow.Data);
            }
        }

        void btShowProductionInfo_Click(UIComponent sender, EventArgs e)
        {
            if (state != State.ExpandedProduction)
            {
                Expand(InfoToShow.Production);

            }
        }


        /// <summary>
        /// the tooltip is already filled/populated at this point - just show it
        /// </summary>
        /// <param name="spawningControl"></param>
        /// <param name="owner"></param>
        /// <param name="useUIOwner"></param>
        /// <param name="screenPosX"></param>
        /// <param name="screenPosY"></param>
        public void InitAndShow(UIComponent spawningControl, EntityGroupID? owner, bool useUIOwner, int screenPosX, int screenPosY)
        {
            // set the owner so item/skill availability will be correct:
            this.useCurrentUIOwner = useUIOwner;
            if (!useUIOwner)
            {
                this.owner = owner;
            }
            else
            {
                this.owner = null;
            }

            this.SpawningControl = spawningControl;

            TextButton textButton = spawningControl as TextButton;
            if (textButton != null)
            {
                // light up the spawning entity type button:
                textButton.IsChecked = true;
            }


            if (!The.InGameUI.EntityTypeTooltipsStack.Contains(this))
            {
                The.InGameUI.EntityTypeTooltipsStack.Add(this);
            }

            ShowInScreenSpace(screenPosX, screenPosY, false);
        }

        public override void ShowInScreenSpace(int screenPosX, int screenPosY, bool modal)
        {
            base.ShowInScreenSpace(screenPosX, screenPosY, modal);

            DisplayWindow.BringToTop();


            timePassed = 0f;

            The.InGameUI.InventorySettings.TrackTargetsChanged += InventorySettings_TrackTargetsChanged;
        }

        /// <summary>
        ///  only update areas related/affected by tracking
        /// </summary>
        protected virtual void RefreshTrackTargets()
        {

        }

        void InventorySettings_TrackTargetsChanged()
        {
            // only update areas related/affected by tracking
            RefreshTrackTargets();

            // PopulateCollapsedFieldsRefresh();

            if (state == State.ExpandedProduction)
            {
                PopulateProductionDataRefresh();
            }

        }


        /// <summary>
        /// only disappears on mouse out if state is collapsed
        /// </summary>
        /// <param name="args"></param>
        void ViewPort_MouseOut(UIComponent sender, InputEventSystem.MouseEventArgs args)
        {
            // if the mouse moves out of the panel, and is not on the child tooltip, close this tooltip and its children too.
            if (state != State.Collapsed)
                return;


            DataSheet childWindow = The.InGameUI.GetChildTooltip(this);
            /*  EntityTypeTooltip parentWindow = The.InGameUI.GetParentTooltip(this);

              bool wasHidden = false;*/

            if (childWindow == null) // || !this.childWindow.DisplayWindow.Visible)
            {
                Hide();
                // wasHidden = true;
            }
            /*    else
                {
                    if (!childWindow.DisplayWindow.CheckCoordinates(args.Position.X, args.Position.Y))
                    {
                        Hide();
                        wasHidden = true;
                    }
                }
                */

            /*
            // now see if we are within the parent window boundaries. if not, close that too:
            if (wasHidden && parentWindow != null)
            {
                if (!parentWindow.DisplayWindow.CheckCoordinates(args.Position.X, args.Position.Y))
                {
                    parentWindow.Hide();
                }
            }*/
        }

        protected void Fill()
        {
            processTypeToShowProductionFor = GetProcessToShow();

            PopulateCollapsedFields();


            // hide/show tool grids:
            UpdateToolGrids();
        }

      
        /// <summary>
        /// gathers tool data for the process, also hides/shows grids
        /// (why was this split into 2 methods? update frequency I guess)
        /// </summary>
        private void UpdateToolGrids()
        {
            grdProductionOuter.BeginAddingEntries();

            currentToolGrids.Clear();

            if (processTypeToShowProductionFor != null
                && processTypeToShowProductionFor.ProcessToolSet != null
                && processTypeToShowProductionFor.ProcessToolSet.Tools != null)
            {
                int gridNo = 0;
                // collect the grids we need:
                foreach (var toolSet in processTypeToShowProductionFor.ProcessToolSet.Tools)
                {
                    ToolGrid grid = allToolGrids[gridNo];

                    currentToolGrids.Add(toolSet, grid);
                    
                    gridNo++;
                }
            }


            // remove all grids:
            foreach (var toolGrid in allToolGrids)
            {
                // clear the grid itself
                toolGrid.Grid.Clear();

                // remove the grid and title header ("TOOL OPTIONS")
                grdProductionOuter.RemoveEntry(toolGrid.HeaderRow, toolGrid.HeaderRow);
                grdProductionOuter.RemoveEntry(toolGrid.Grid, toolGrid.Grid);
            }

            if (currentToolGrids.Count > 0)
            {
                float index = 0f; // GetToolPanelIndex();

                foreach (var item in currentToolGrids)
                {
                    grdProductionOuter.AddEntry(item.Value.HeaderRow, item.Value.HeaderRow); //, index); // use the tools set as key for the grid row
                    item.Value.HeaderRow.OrderByTag1 = toolsIndex + index;

                    index++;

                    grdProductionOuter.AddEntry(item.Value.Grid, item.Value.Grid); //, index);
                    item.Value.Grid.OrderByTag1 = toolsIndex + index;

                    index++;
                }
            }

            grdProductionOuter.Sort(Grid.Sorting.Ascending, true);
            grdProductionOuter.EndAddingEntries();
        }


      /*  private int GetToolPanelIndex()
        {
            // after skill...
            return grdProductionOuter.GetIndexByKey(lblSkill) + 1;

        }*/

        /// <summary>
        /// even when hidden, updates scrolling and fading. not content!
        /// </summary>
        /// <param name="elapsed"></param>
        public override void Update(GameTime elapsed)
        {
            base.Update(elapsed);

            if (DisplayWindow.Visible == false)
            {

                Tooltip.HandleUpdate(elapsed, ref timePassed, tooltipAnchor, this);
            }

            if (DisplayWindow.IsVisibleAndActive && currentlyShownExpandedGrid != null)
            {
                UpdateScrolling(elapsed);
            }
        }

        public override void Refresh()
        {
            base.Refresh();

            ProcessType previousProcess = processTypeToShowProductionFor;
            processTypeToShowProductionFor = GetProcessToShow();

            if (processTypeToShowProductionFor != previousProcess)
            {
                // hide/show tool grids:
                UpdateToolGrids();
            }

            RefreshCollapsedFields();


            if (state == State.ExpandedProduction)
            {
                PopulateProductionDataRefresh();
            }
            else if (state == State.ExpandedData)
            {
                PopulateGeneralDataRefresh();
            }

        }



        /*   public void RetireTooltip()
           {
               if (EntityType != null)
               {
                   List<EntityTypeTooltip> tooltips;
                   if (!The.InGameUI.poolOfTooltips.TryGetValue(EntityType, out tooltips)) // .ContainsKey(EntityType))
                   {
                       tooltips = new List<EntityTypeTooltip>();                  
                       The.InGameUI.poolOfTooltips.Add(EntityType, tooltips);
                   }

                   if (!tooltips.Contains(this))
                   {
                       tooltips.Add(this);
                   }
               }
           }*/

        public bool MouseIsOverTooltipOrChildTooltips(/*InputData inputData*/ Point mousePos, int callNo)
        {

            if (DisplayWindow.CheckCoordinates(mousePos.X, mousePos.Y)) // inputData.mouseX, inputData.mouseY))
            {
                return true;
            }
            else
            {
                DataSheet childWindow = The.InGameUI.GetChildTooltip(this);
                if (childWindow != null)
                {
                    return childWindow.MouseIsOverTooltipOrChildTooltips(mousePos /* inputData*/, callNo + 1);
                }
            }

            return false;
        }

        UIComponent tooltipAnchor;
        public void StartCountdownToShow(UIComponent sender)
        {
            this.tooltipAnchor = sender;

            timePassed = 0f;
        }


        /// <summary>
        /// populate the expanded panel at the time it is requested. Then keep the controls
        /// </summary>
        /// 
        public void UpdateProductionAndGeneralInfoButtons(InfoToShow infoToShow)
        {
            if (infoToShow == InfoToShow.Production && processTypeToShowProductionFor != null)
            {
                btShowProductionInfo.IsChecked = true;
                btShowGeneralInfo.IsChecked = false;
            }
            else
            {
                btShowGeneralInfo.IsChecked = true;
                btShowProductionInfo.IsChecked = false;
            }
        }
        public enum InfoToShow { Production, Data }
        public void Expand(InfoToShow infoToShow)
        {
            DisplayWindow.Height = ExpandedHeight;

            // add the grid with the data that we want to show

            UpdateProductionAndGeneralInfoButtons(infoToShow);

            if (infoToShow == InfoToShow.Production) 
            {
                btShowProductionInfo.IsChecked = true;
                btShowGeneralInfo.IsChecked = false;
                state = State.ExpandedProduction;
                
                expandedContentViewPort.Remove(grdGeneralOuter);
                expandedContentViewPort.Add(grdProductionOuter);

                currentlyShownExpandedGrid = grdProductionOuter;

                SetProductionHeading(lblExpandedHeading);
                lblExpandedHeading.FitToText();                           

                lblExpandedHeading.NormalColor = productionColor;

                grdProductionOuter.Y = 0; 

                PopulateProductionDataRefresh();

            }
            else
            {
                state = State.ExpandedData;

                expandedContentViewPort.Remove(grdProductionOuter);
                expandedContentViewPort.Add(grdGeneralOuter);

                currentlyShownExpandedGrid = grdGeneralOuter;

                lblExpandedHeading.Text = "DATA";
                lblExpandedHeading.FitToText();
                PadHeader(lblExpandedHeading);

                lblExpandedHeading.NormalColor = Color.White;

                grdGeneralOuter.Y = 0; 

                PopulateGeneralFields();                         
            }

            AdaptContentToWindow();   
        }

       /* protected static string PadHeader(string text)
        {
            int slashWidth = lbl.GetTextWidth("/");
            int slashes = (grdProductionOuter.Width - lbl.Right - 60) / slashWidth;
            string slashesString = new string('/', slashes);
            lbl.Text += slashesString;
            lbl.FitToText();
        }*/

        protected void PadHeader(Label lbl)
        {            
            int slashWidth = lbl.GetTextWidth("/");
            int slashes = (grdProductionOuter.Width - lbl.Right - 60) / slashWidth;
            string slashesString = new string('/', slashes);
            lbl.Text += slashesString;
            lbl.FitToText();
        }


        /*  override void Destroy()
          {
            
          }*/


        //protected abstract string GetProductionHeading();

        protected abstract void SetProductionHeading(Label label);

        protected abstract string GetInputHeading();

        private void PopulateCollapsedFields()
        {

            btShowProductionInfo.IsChecked = false;
            btShowGeneralInfo.IsChecked = false;

            btShowProductionInfo.Visible = true;

            Remove(noProduction);


            PopulateCollapsedFieldsContents();

        }

        /// <summary>
        /// refresh header fields that need to update regardless of expanded/collapsed state
        /// </summary>
        protected virtual void RefreshCollapsedFields()
        {

        }

        /// <summary>
        /// stuff that only needs to be populated once, like icon type and name
        /// </summary>
        protected virtual void PopulateCollapsedFieldsContents()
        {

        }

        private void PopulateProductionDataRefresh()
        {
            EntityGroup resolvedOwner;
            // resolve the owner - if this fails, availability will show unavailable...
            ResolveOwner(out resolvedOwner);


            //***  nest these calls:
            grdProductionOuter.BeginAddingEntries();

            PopulatePolicy();
            PopulateGatheredAt();
            PopulateInputList(resolvedOwner);
            PopulateSkill();
            PopulateActingOn();
            PopulateToolsList(resolvedOwner);

            PopulateProductionContentRefresh();

            grdProductionOuter.Sort(Grid.Sorting.Ascending, true);

            CollapseTopRowPadding();

            grdProductionOuter.EndAddingEntries();
            //***


            AdaptContentToWindow();
           // AdaptWindowToContent();

        }

        private void CollapseTopRowPadding()
        {
           // grdProductionOuter.Entries[0].

        }

        private void PopulateGeneralDataRefresh()
        {

            //***  nest these calls:
            grdGeneralOuter.BeginAddingEntries();


            PopulateGeneralDataContentRefresh(); // ammo...

            grdGeneralOuter.Sort(Grid.Sorting.Ascending, true); // NEW


            grdGeneralOuter.EndAddingEntries();
            
            AdaptContentToWindow();

        }

        /// <summary>
        /// "DATA"
        /// </summary>
        private void PopulateGeneralFields()
        {
            grdGeneralOuter.BeginAddingEntries();

            PopulateDescription();

            PopulateGeneralDataContent();

            grdGeneralOuter.Sort(Grid.Sorting.Ascending, true); // NEW

            grdGeneralOuter.EndAddingEntries(); // resizes?

        }

        protected virtual string GetDescription()
        {
            return "";
        }

        private void PopulateDescription()
        {
            grdGeneralOuter.TryRemoveEntry(taDescription);
            taDescription.Text = GetDescription();

            if (!string.IsNullOrEmpty(taDescription.Text))
            {
                grdGeneralOuter.AddEntry(taDescription, taDescription);
                //description.Height = 1;
            }

            /*
            taDescription.Text = GetDescription();

            if (string.IsNullOrEmpty(taDescription.Text))
            {
                grdGeneralOuter.TryRemoveEntry(taDescription);
                //description.Height = 1;
            }
            else if (!grdGeneralOuter.EntriesByKey.ContainsKey(taDescription))
            {
                
                grdGeneralOuter.Insert(taDescription, 0);
            }*/

        }



        protected abstract ProcessType GetProcessToShow();
               


        private void PopulateSkill()
        {
            // show/hide the heading:
            grdProductionOuter.TryRemoveEntry(skillHeader);
            grdProductionOuter.TryRemoveEntry(lblSkill);

            if (processTypeToShowProductionFor != null && processTypeToShowProductionFor.RequiredSkillType != null)
            {
                SkillType skillType = processTypeToShowProductionFor.RequiredSkillType;

                lblSkill.Text = skillType.Name;

                // show skill status for this expedition only..?
                Expedition expedition = The.InGameUI.GetExpedition();

                // see if anyone has the needed skill
                bool expeditionHasSkill = expedition.HasSkill(skillType);


                if (!expeditionHasSkill)
                {
                    lblSkill.NormalColor = DataTypeButton.notInStockColorLight;
                    lblSkill.ToolTip = "No one has sufficent level in this skill (more than 0.1)";//was  no one has this skill
                }
                else
                {
                    lblSkill.NormalColor = lblSkill.GetNormalColorForType();
                    lblSkill.ToolTip = "At least one character has the sufficent level in this skill (more than 0.1)";//was    At least one character has this skill.
                }

                grdProductionOuter.AddEntry(skillHeader, skillHeader);
                grdProductionOuter.AddEntry(lblSkill, lblSkill);
            }
            else
            {
                lblSkill.Text = "";
            }
        }

       
        protected virtual void PopulatePolicy()
        {
            grdProductionOuter.TryRemoveEntry(policyHeader);
           
            if (processTypeToShowProductionFor != null)
            {
                TierOrAreaType tierArea = processTypeToShowProductionFor.GetTierArea();

                ShowPolicyArea(tierArea);
            }            
        }

        protected void ShowPolicyArea(TierOrAreaType tierArea)
        {
            if (tierArea != null) // processTypeToShowProductionFor.TierAreaType != null) //  entityType.TierAreaType != null)
            {               
               // policyIcon.SetSkinLocations(gui.GUISpriteSheet.GetSourceRectangle(tierArea.Icon), Icon.UIType.HUD);
                policyIcon.SetSkinLocation(SkinState.Normal, gui.GUISpriteSheet.GetSourceRectangle(tierArea.Icon));
                policyIcon.SetSkinLocation(SkinState.Hover, gui.GUISpriteSheet.GetSourceRectangle(tierArea.Icon), Color.Gray, Color.Gray, false, true); // modulate the yellow tint

                policyIcon.ResizeControlToFitImage();
                policyIcon.ToolTip = Common.ComposeHeadingAndBlobText(tierArea.ToString(), tierArea.Description);
                policyIcon.AlignRight(policyHeader.Width - rightMargin);
                policyIcon.TooltipExpires = false;

                // show skill status for this expedition only..?
                Expedition expedition = The.InGameUI.GetExpedition();

                // see if anyone has the needed skill
                bool expeditionHasPolicy = expedition.Policy.CanProduceOrTrade(tierArea);

                if (!expeditionHasPolicy)
                {
                    policyIcon.Color = DataTypeButton.notInStockColorLight;
                    policyIcon.ToolTip = tierArea.GetNotAvailableTooltip();
                }
                else
                {
                    policyIcon.Color = Color.White;
                    policyIcon.ToolTip = "We have the needed policy: " + tierArea.ToString();
                }


                grdProductionOuter.AddEntry(policyHeader, policyHeader);
                // grdProductionOuter.AddEntry(lblSkill, lblSkill);
            }
        }

        private void PopulateGatheredAt()
        {
            grdProductionOuter.TryRemoveEntry(gatheredFromHeader);
            grdProductionOuter.TryRemoveEntry(lblGatheredFrom);

            if (processTypeToShowProductionFor != null && processTypeToShowProductionFor.IsGathering)
            {
                /*
                items that come from a tile resource should have the wording :
                   GATHERED AT:
                   FISHED AT:
                   HARVESTED AT:
                   CAUGHT AT:
                   the verb is datadriven, comes from the process in processloader, for example "gatherTreeScuttler" "fishStreakFin" "catchNeonHornets" 
                   (needs conjugation)
                */
               // string gatheredAt = processTypeToShowProductionFor.GatheredAtTerm ?? "GATHERED FROM"; //"GATHERED AT";

                ResourceType resource = processTypeToShowProductionFor.ResourceTypeInput;

                if (resource != null)
                {
                    lblGatheredFrom.Text = resource.Name; 
                   // lblGatheredFrom.Text = gatheredAt + ": " + resource.Name; // ": CONSULT DATASHEET"; // better to display resource name/icon
                }
                else
                {
                    lblGatheredFrom.Text = "CONSULT DATASHEET"; // ??
                    //lblGatheredFrom.Text = gatheredAt + ": CONSULT DATASHEET";
                }
               
                grdProductionOuter.AddEntry(gatheredFromHeader, gatheredFromHeader);
                grdProductionOuter.AddEntry(lblGatheredFrom, lblGatheredFrom);
            }
        }

        private void PopulateInputList(EntityGroup resolvedOwner)
        {
            grdInputs.BeginAddingEntries();

            // update the list to show availability

            grdProductionOuter.TryRemoveEntry(madeFromHeader);


            if (processTypeToShowProductionFor != null)
            {
                // show the header
               // int madeFromIndex = 0;
                //grdProductionOuter.AddEntry(madeFromHeader, madeFromHeader, madeFromIndex);
              
                if (processTypeToShowProductionFor.InputsByType != null
                   && processTypeToShowProductionFor.InputsByType.Count > 0
                   && !processTypeToShowProductionFor.IsGathering)
                {
                    if (processTypeToShowProductionFor.IsKilling) // || processTypeToShowProductionFor.IsHarvesting) // processTypeToShowProductionFor.IsPrimaryProcess)
                    {
                        // carcasses
                        lblMadeFrom.Text = "YIELDED FROM:";
                    }
                    else
                    {
                        lblMadeFrom.Text = GetInputHeading(); // "MADE FROM:";
                    }

                    lblMadeFrom.Visible = true;
                    icMadeFrom.Visible = true;
                }             
                else
                {
                    // special actions
                    lblMadeFrom.Text = "";
                    lblMadeFrom.Visible = false;
                    icMadeFrom.Visible = false;
                }

                

                if (lblMadeFrom.Visible)
                {
                    grdProductionOuter.AddEntry(madeFromHeader, madeFromHeader); //, madeFromIndex);
                    if (icMadeFrom.Visible)
                    {
                        lblMadeFrom.ToolTip = madeFromTooltip;
                        lblMadeFrom.X = 25; // make space for an icon
                    }
                    else
                    {
                        lblMadeFrom.ToolTip = gatheredTooltip;
                        lblMadeFrom.X = 10;
                    }
                }

                if (processTypeToShowProductionFor.InputsByType != null
                    && processTypeToShowProductionFor.InputsByType.Count > 0)
                {

                    UIComponent itemRow;
                    EntityType inputEntityType;

                    bool hasInputs, hasTools, ownsEnoughItemsOfThisType;
                   
                    if (processTypeToShowProductionFor.InputsByType != null)
                    {
                        foreach (var input in processTypeToShowProductionFor.InputsByType)
                        {
                            inputEntityType = input.Key;
                            int maxAmountThatCanBeProduced;
                            int? noOfMissingInputTypes, noOfAvailableInputTypes, noOfAvailableItems;

                            InventoryPanel.HasInputForProcess(processTypeToShowProductionFor, resolvedOwner, out hasInputs, out maxAmountThatCanBeProduced, 
                                out noOfMissingInputTypes, out noOfAvailableInputTypes, out noOfAvailableItems, input.Key);
                            // InventoryPanel.OwnsProductOrHasProcessInputsAndTools(inputEntityType, GetOwner(), out ownsEnoughItemsOfThisType, out hasInputs, out hasTools, null, false); // true); // !EntityTypeIsAvailableForProduction(entityType, owner, noOfAvailableItems))

                            ownsEnoughItemsOfThisType = hasInputs;
                            //   score = ScoreItem(inputEntityType, 0f, out hasInputs, out hasTools, out ownsItem);

                            hasTools = InventoryPanel.HasToolsForProcess(processTypeToShowProductionFor, resolvedOwner);

                            if (!grdInputs.TryGetEntry(inputEntityType, out itemRow))
                            {
                                itemRow = AddEntityAmountRow(grdInputs, inputEntityType, ownsEnoughItemsOfThisType, "The amount of materials that are needed"); //, input.Value.Amount.AmountToString );
                            }

                            UpdateRequiredItemRow(itemRow, inputEntityType, 0f, hasTools, hasInputs, ownsEnoughItemsOfThisType, input.Value.Amount.NoOfItems, noOfAvailableItems);
                        }
                    }

                    grdInputs.DeleteEntries<EntityType>(e => processTypeToShowProductionFor.InputsByType.ContainsKey(e));
                }
                else
                {
                    grdInputs.Clear();
                }

            }
            else
            {
                grdInputs.Clear();
            }

            grdInputs.EndAddingEntries();
        }


        private void PopulateActingOn()
        {
            if (actingOn != null)
            {   
                grdProductionOuter.TryRemoveEntry(actingOnHeader);            
                grdProductionOuter.TryRemoveEntry(actingOn);
            }
                     

            if (processTypeToShowProductionFor != null && processTypeToShowProductionFor.ActingOnType != null)
            { 
                EntityType actingOnType = processTypeToShowProductionFor.ActingOnType;

                if (actingOn == null)
                {
                    actingOn = new UIComponent(gui); // need an entitytype to create this...
                    DataTypeButton entityTypeButton;
                    StockpileWindow.CreateItemGridRow(actingOnType, owner, useCurrentUIOwner, actingOn, DataSheet.InfoToShow.Data, false, false, out entityTypeButton);
                    entityTypeButton.X = 42;
                    entityTypeButton.Width = 154;
                    actingOn.OrderByTag1 = actingOnIndex + 1f;
                    actingOn.Height = 23;
                }

                grdProductionOuter.AddEntry(actingOnHeader, actingOnHeader);
                grdProductionOuter.AddEntry(actingOn, actingOn);

               
                Icon icon = (Icon)actingOn.FindChildById(UIComponent.DataControlID.Icon, true);
                IconInfo iconInfo;
                Rectangle rect;
                rect = actingOnType.GetIconSprite(out iconInfo);
                icon.SetSkinLocation(SkinState.Normal,rect);
                icon.ResizeControlToFitImage();
                actingOn.CenterChildVertically(icon, iconInfo != null ? iconInfo.CenterYPos : null);
                actingOn.CenterHorizontally(24, icon);

                DataTypeButton dbButton = (DataTypeButton)actingOn.FindChildById(UIComponent.DataControlID.Caption, true);
                dbButton.FillEntityType(DataSheet.InfoToShow.Data, actingOnType, null, true, actingOnType.Name, null);
            }
           

        }

        /*  private static void SetEntityTypeColor(EntityType entityType, Owner owner, TextButton tbCaption)
          {
           


              bool hasTools, hasInputs;
              int productionLimit;
              int? numberOfMissingInputTypes;
              bool canProduce = InventoryPanel.HasProcessInputsAndToolsForProduct(entityType, owner, out hasInputs, out hasTools, out productionLimit, out numberOfMissingInputTypes);

              Color color = InventoryPanel.GetProductionStatusColor(hasTools, hasInputs, canProduce, tbCaption.GetNormalColor());

              tbCaption.Color = color;
          }*/

        protected UIComponent AddEntityAmountRow(Grid grid, EntityType inputEntityType, /*Input input,*/ bool ownsItem, string amountTooltip) //, int amount)
        {
            UIComponent itemRow = new UIComponent(gui);
            grid.AddEntry(inputEntityType, itemRow);


            DataTypeButton tbCaption;
            CreateItemGridRow(inputEntityType, itemRow, out tbCaption);

            tbCaption.SetAvailableStatusColor(ownsItem);
            itemRow.CenterChildVertically(tbCaption);

            Label lblAmount = new Label(gui);
            lblAmount.Init(Label.LabelType.HUDWindow);
           // lblAmount.Text = amount.ToString(); // input.Amount.AmountToString();
            lblAmount.X = tbCaption.Right + doubleSpacing;
            itemRow.Add(lblAmount);
            lblAmount.ToolTip = amountTooltip; // ;
            lblAmount.ID = UIComponent.DataControlID.Amount;

            RightJustify(lblAmount);

            // item row has a height now that its been added:
            itemRow.CenterChildVertically(lblAmount);

            //hacky           
            lblAmount.Y++;

            return itemRow;
        }

        private void RightJustify(Label lbl)
        {
            lbl.FitToText();

            lbl.X = collapsedWidth - lbl.TextWidth - sideMargin - extraSideMargin - 5;

        }

        /// <summary>
        /// update to show availability
        /// </summary>
        /// <param name="fillUserControls"></param>
        /// <param name="itemRow"></param>
        /// <param name="entityType"></param>
        /// <param name="noOfStockpiledItems"></param>
        /// <param name="stockpile"></param>
        protected void UpdateEntityAmountRow(UIComponent itemRow, EntityType entityType, bool isAvailable, int amount)
        {
            // update existing row:

            DataTypeButton tbCaption = (DataTypeButton)itemRow.FindChildById(UIComponent.DataControlID.Caption);

            tbCaption.SetAvailableStatusColor(isAvailable == true);
            
            Bar bar = (Bar)itemRow.FindChildById(UIComponent.DataControlID.Background);

            RefreshTrackedColorBar(entityType, bar);

            Label lblAmount = (Label)itemRow.FindChildById(UIComponent.DataControlID.Amount);
            if (lblAmount != null)
            {
                lblAmount.Text = amount.ToString();
                lblAmount.FitToText();
                RightJustify(lblAmount);               
            }
        }


        /// <summary>
        /// update to show availability
        /// </summary>
        /// <param name="fillUserControls"></param>
        /// <param name="itemRow"></param>
        /// <param name="entityType"></param>
        /// <param name="noOfStockpiledItems"></param>
        /// <param name="stockpile"></param>
        protected void UpdateRequiredItemRow(UIComponent itemRow, EntityType entityType, float score,
            bool? hasTools = null, bool? hasInputs = null, bool? isAvailable = null, int? neededAmount = null, int? availableAmount = null) 
        {
            // update existing row:

            DataTypeButton tbCaption = (DataTypeButton)itemRow.FindChildById(UIComponent.DataControlID.Caption);

            tbCaption.SetAvailableStatusColor(isAvailable == true);

            itemRow.OrderByTag1 = score;

            Bar bar = (Bar)itemRow.FindChildById(UIComponent.DataControlID.Background);

            RefreshTrackedColorBar(entityType, bar);

            Label lblAmount = (Label)itemRow.FindChildById(UIComponent.DataControlID.Amount);
            if (lblAmount != null)
            {
                if (neededAmount.HasValue)
                {
                    if (availableAmount.HasValue)
                    {
                        lblAmount.Text = availableAmount.Value + "/" + neededAmount.Value.ToString();

                        StringBuilder text = new StringBuilder();
                       
                       // string tooltip;
                        if (availableAmount.Value >= neededAmount.Value)
                        {
                            Common.AppendLine(text, "We have the needed amount of this input.");
                            Common.Append(text, "Available: ");
                            Common.Append(text, availableAmount.Value.ToString(), true);
                      
                           // tooltip = string.Format("Available: {0} / Needed: {1} \nWe have the needed amount of this input.", availableAmount.Value, neededAmount.Value);
                            //"Available / Needed \nWe have a sufficient amount of this input.";
                            lblAmount.NormalColor = Color.White; // DataTypeButton.notInStockColorLight;
                        }
                        else
                        {
                            Common.AppendLine(text, "We do not have the needed amount of this input.");
                            Common.Append(text, "Available: ");
                            Common.Append(text, availableAmount.Value.ToString(), Common.ValueTint.Negative);
                      
                          //  tooltip = string.Format("We do not have the needed amount of this input. \nAvailable: {0} / Needed: {1}", availableAmount.Value, neededAmount.Value);
                         
                            //lblAmount.ToolTip = "Available / Needed";
                            lblAmount.NormalColor = DataTypeButton.notInStockColorLight;
                        }
                       
                        Common.Append(text, " / Needed: ");
                        Common.Append(text, neededAmount.Value.ToString(), true);

                        lblAmount.ToolTip = text.ToString();
                    }
                    else
                    {
                        lblAmount.Text = neededAmount.Value.ToString();
                        lblAmount.NormalColor = Color.White;
                        lblAmount.ToolTip = "Needed amount";
                    }

                    lblAmount.FitToText();
                    RightJustify(lblAmount);
                }
                else
                {
                    lblAmount.Text = "0";
                    lblAmount.NormalColor = Color.White;
                }
            }          
        }

        protected static void RefreshTrackedColorBar(EntityType entityType, Bar bar)
        {
            string toolTip = null;
            Color? color;

            if (The.InGameUI.InventorySettings.GetTrackedColorAndTooltip(entityType, out color, out toolTip))
            {
                bar.Visible = true;

                bar.SetSkinLocation(SkinState.Normal, null, color, color);

                Color hoverTint = new Color(color.Value.R - 40, color.Value.G - 40, color.Value.B - 40);
                bar.SetSkinLocation(SkinState.Hover, null, hoverTint, hoverTint);
                bar.ToolTip = toolTip;
            }
            else
            {
                // hide the bar
                bar.Visible = false;
            }
        }



        private UIComponent AddToolRow(Grid grid, EntityType entityType, float productivity, float score, bool hasInputs, bool hasTools, bool ownsItem)
        {
            UIComponent itemRow = new UIComponent(gui);
            grid.AddEntry(entityType, itemRow);

            DataTypeButton tbCaption;

            CreateItemGridRow(entityType, itemRow, out tbCaption);

            itemRow.OrderByTag1 = score; // used for sorting!

            #region commented
            /* Image stageIcon = new Image(gui);
             rect = gui.GUISpriteSheet.GetSourceRectangle("HUD_icon_changeLayout"); // placeholder image
             stageIcon.SetSkinLocation(SkinState.Normal,rect);
             stageIcon.Texture = gui.GUISpriteSheet.Texture;
             itemRow.Add(stageIcon);
             stageIcon.X = sideMargin; // = new Point(itemTypeIconColumnX, 0);
             stageIcon.ResizeControlToFitImage();

             //     ItemTypeButtonEventArgs eventArgs = new ItemTypeButtonEventArgs(inputEntityType);
             EntityTypeButtonEventArgs typeArgs = new EntityTypeButtonEventArgs(inputEntityType, owner);

             tbCaption = new TextButton(gui);
             tbCaption.Init(TextButton.TextButtonType.LCDToolTipWhite);
             tbCaption.ID = UIComponent.DataControlID.Caption;
             tbCaption.Text = inputEntityType.PluralName;
             tbCaption.EventArgs = typeArgs;
             tbCaption.ToolTip = inputEntityType;
             tbCaption.TextAlignment = TextButton.TextAlign.Left;
             tbCaption.X = stageIcon.Right + buttonSpacing;
             itemRow.Add(tbCaption);
             tbCaption.Width = grdInputs.Width - tbCaption.X;
             //  tbCaption.X = itemTypeDescriptionColumnX;
             tbCaption.Click += new ClickHandler(The.InGameUI.HandleEntityTypeProductionClick);
             tbCaption.DebugTag = "inputEntity";
             */


            /*    Color color;

                if (!ownsItem)
                {
                    color = InventoryPanel.GetProductionStatusColor(hasTools, hasInputs, hasTools && hasInputs, tbCaption.GetNormalColor());
                }
                else
                {
                    color = tbCaption.GetNormalColor();
                }

                tbCaption.Color = color;
                */
            #endregion


            itemRow.CenterChildVertically(tbCaption);

            Label lblProductivity = new Label(gui);
            lblProductivity.Init(Label.LabelType.HUDWindow);
            lblProductivity.Text = productivity.ToString("N", Config.Culture); // "TEST"; // productivity.ToString("N");
            lblProductivity.X = tbCaption.Right + singleSpacing;
            itemRow.Add(lblProductivity);
            RightJustify(lblProductivity);
            lblProductivity.DebugTag = "toolProd";
            lblProductivity.ToolTip = "Productivity rating";

            itemRow.CenterChildVertically(lblProductivity);

            lblProductivity.Y++;

            return itemRow;
        }

        protected void CreateItemGridRow(EntityType entityType, UIComponent itemRow, out DataTypeButton entityTypeButton)
        {
            StockpileWindow.CreateItemGridRow(entityType, owner, useCurrentUIOwner, itemRow, DataSheet.InfoToShow.Production, false, false, out entityTypeButton);
            entityTypeButton.X = 36;
            entityTypeButton.Width = 154;

            Bar bar = CreateTrackedColorBar(entityTypeButton.Right);

            itemRow.Add(bar);
        }

        protected Bar CreateTrackedColorBar(int x)
        {
            //Add the colored Bar next to the DatatypeButton
            Bar bar = new Bar(Interface.gui);
            bar.ID = UIComponent.DataControlID.Background;
            bar.X = x;
            bar.Height = 23;// rectangle.Height;
            bar.Width = 15;

            Rectangle rectangle = Interface.gui.GUISpriteSheet.GetSourceRectangle("basic_highlightBar_white");
            bar.SetSkinLocation(SkinState.Normal, rectangle);
            bar.SetSkinLocation(SkinState.Hover, rectangle);

            bar.Visible = false;
            return bar;
        }



       

        private List<EntityType> toolsInGrid = new List<EntityType>();

        /// <summary>
        /// show each set of tools (up to 3) in its own list. show 3 alternatives at a time, filtered by stock availability (1 step back), sorted by desirability and with two buttons to 
        /// change the alternatives displayed in each list.
        /// </summary>
        /// <param name="processTypeToShow"></param>
        private void PopulateToolsList(EntityGroup resolvedOwner)
        {
            Grid grid;
            ToolAlternatives tools;
            int firstToolIndexToShow, clampedToolIndex;
            int maxToolIndex;

            UIComponent itemRow;

            // int addedTools = 0;

            EntityType entityType = null;
            float productivity;

            ToolGrid toolGrid;

            foreach (var value in currentToolGrids) // loop over the 3 grids
            {
                toolGrid = value.Value;
                grid = toolGrid.Grid;
                tools = value.Key;
                firstToolIndexToShow = toolGrid.ScrollPosition;

                ImageButton btExpand;
                toolGrid.HeaderRow.FindChildById(UIComponent.DataControlID.Expand, out btExpand);


                int noOfToolsToDisplay;
                if (btExpand.IsChecked)
                {
                    noOfToolsToDisplay = GameData.Instance.GUIConstants.NoOfToolsToDisplayWhenExpanded;
                }
                else
                {
                    noOfToolsToDisplay = GameData.Instance.GUIConstants.NoOfToolsToDisplayWhenCollapsed;  //noOfToolsToDisplayWhenCollapsed;
                }

                toolsInGrid.Clear();

                // score the list so the most relevant tools can be shown at the top    
                var toolScores = ScoreTools(resolvedOwner, tools);

                // examine user scroll setting, clamp if necessary:
                clampedToolIndex = Common.Clamp(firstToolIndexToShow, 0, toolScores.Count - noOfToolsToDisplay);

                if (clampedToolIndex != firstToolIndexToShow)
                {
                    value.Value.ScrollPosition = clampedToolIndex;

                    firstToolIndexToShow = clampedToolIndex;
                }

                maxToolIndex = Common.ClampTop(firstToolIndexToShow + noOfToolsToDisplay, toolScores.Count);

                if (!btExpand.IsChecked)
                {
                    if (maxToolIndex < toolScores.Count)
                    {
                        btExpand.Visible = true;
                    }
                    else
                    {
                        btExpand.Visible = false;
                    }
                }
                else
                {
                    btExpand.Visible = true;
                }


                grid.BeginAddingEntries();

                int thisIndex = -1;

                try
                {

                    // loop through the data source items, update the grid with new values
                    for (int i = firstToolIndexToShow; i < maxToolIndex; i++)
                    {
                        thisIndex = i; // for debug

                        var tool = toolScores[i]; // <- crash here? // http://steamcommunity.com/app/284100/discussions/2/405694115197789476/

                        entityType = tool.Item1;
                        productivity = tool.Item2;

                        toolsInGrid.Add(entityType);

                        if (!grid.TryGetEntry(entityType, out itemRow))
                        {
                            itemRow = AddToolRow(grid, entityType, productivity, tool.Item3, tool.Item4, tool.Item5, tool.Item6);
                        }

                        UpdateRequiredItemRow(itemRow, entityType, tool.Item3,
                               tool.Item4, tool.Item5, tool.Item6);
                    }
                }
                catch (Exception ex)
                {
                    // delete after  http://steamcommunity.com/app/284100/discussions/2/405694115197789476/ is corrected
                    string exceptionString = ex.Message;
                    exceptionString += " \n lblName.Text: " + lblName.Text;
                    exceptionString += " \n entityType: " + entityType != null ? entityType.KeyName : "null";
                    exceptionString += " \n thisIndex: " + thisIndex;
                    exceptionString += " \n maxToolIndex: " + maxToolIndex;
                    exceptionString += " \n firstToolIndexToShow: " + firstToolIndexToShow;
                    throw new Exception("#1 " + exceptionString);

                }


                // remove grid items not in the new set:
                grid.DeleteEntries<EntityType>(i => toolsInGrid.Exists(t => i == t));

                // sort the grid:              
                grid.Sort(i => (float)i.OrderByTag1);


                // show/hide scroll buttons:
                if (showAllTools)
                {
                    ShowOrHideToolGridScrollButtons(tools, toolGrid, toolScores.Count);
                }

                grid.EndAddingEntries();
            }

        }

        protected void CreateGridAndHeader(Grid outerGrid, out UIComponent header, string title, string tooltip, float sortIndex, out Grid grid, out Label lblHeader)
        {
            header = AddSubHeader(outerGrid, title, out lblHeader);
            header.OrderByTag1 = sortIndex;

            lblHeader.ToolTip = tooltip;

            grid = new Grid(gui, ListBoxType.HUDAndLCD, WindowSystem.Label.LabelType.EntityTypeTooltip);
            grid.IsOuterGrid = false;
            grid.X = sideMargin;
            grid.FixedItemHeights = true;
            grid.Width = outerGrid.Width;
            grid.ScrollBarEnabled = false;
            grid.ItemHeight = itemHeight;
            grid.CanGrowInHeight = true;
            grid.Font = GUIManager.LCDandHUDBodyFontPath;
            grid.Height = 160;
            grid.OrderByTag1 = sortIndex + 1f;

            outerGrid.AddEntry(grid, grid);
        }

        /// <summary>
        /// called continually
        /// </summary>
        protected virtual void PopulateProductionContentRefresh()
        {
           
        }

        /// <summary>
        /// called continually
        /// </summary>
        protected virtual void PopulateGeneralDataContentRefresh()
        { }

        /// <summary>
        /// only called when displaying/switching
        /// </summary>
        protected virtual void PopulateGeneralDataContent()
        { }


        private bool ProcessHasEntityTypeAsOutput(ProcessType processType, EntityType entityType)
        {
            return processType.Outputs.FirstOrDefault(o => o.FinalEntityTypeToCreate == entityType) != null;
        }

        /*   private bool ProcessHasOutput(Output output, EntityType entityType)
           {
               return output.FinalEntityType == entityType;
           }*/


        /// <summary>
        /// not currently used
        /// </summary>
        /// <param name="tools"></param>
        /// <param name="toolGrid"></param>
        /// <param name="toolScoresCount"></param>
        private void ShowOrHideToolGridScrollButtons(ToolAlternatives tools, ToolGrid toolGrid, int toolScoresCount)
        {
            UIComponent itemRow;
            if (toolScoresCount > GameData.Instance.GUIConstants.NoOfToolsToDisplayWhenCollapsed && !toolGrid.ScrollButtonsAreShown)
            {
                // show
                if (grdProductionOuter.TryGetEntry(tools, out itemRow))
                {
                    UIComponent btUp = itemRow.FindChildById(UIComponent.DataControlID.Up);
                    if (btUp == null)
                    {
                        itemRow.Add(toolGrid.btScrollUp);
                        itemRow.Add(toolGrid.btScrollDown);

                        toolGrid.ScrollButtonsAreShown = true;
                    }
                }
            }
            else if (toolScoresCount <= GameData.Instance.GUIConstants.NoOfToolsToDisplayWhenCollapsed && toolGrid.ScrollButtonsAreShown)
            {
                // hide
                if (grdProductionOuter.TryGetEntry(tools, out itemRow))
                {
                    itemRow.Remove(toolGrid.btScrollDown);
                    itemRow.Remove(toolGrid.btScrollUp);

                    toolGrid.ScrollButtonsAreShown = false;
                }
            }

        }

        /*  private bool OwnsItemOrHasInputs(Tuple<EntityType, float, float, bool, bool, bool> toolInfo)
          {
              return toolInfo.Item4 || toolInfo.Item6;
          }*/

        /// <summary>
        /// returns:
        /// type, productivity, score, has inputs, has tools, owns item
        /// 
        /// make the tuple into a class, too confusing
        /// </summary>
        /// <param name="tools"></param>
        /// <returns></returns>
        private List<Tuple<EntityType, float, float, bool, bool, bool>> ScoreTools(EntityGroup resolvedOwner, ToolAlternatives tools)
        {
            //tools.ToolsAndProductivity[0].Item3 = 0f;

            bool hasInputs, hasTools, ownsItem;
            float score;
            List<Tuple<EntityType, float, float, bool, bool, bool>> toolScores = new List<Tuple<EntityType, float, float, bool, bool, bool>>();

            foreach (var tool in tools.ToolsAndProductivity)
            {
                score = ScoreItem(resolvedOwner, tool.Item1, tool.Item2, out hasInputs, out hasTools, out ownsItem, true);

                // store the important stats about each tool in a tuple:
                toolScores.Add(
                    new Tuple<EntityType, float, float, bool, bool, bool>(tool.Item1, tool.Item2, score, hasInputs, hasTools, ownsItem));
            }


            // also show unavailable tools:
            //  toolScores.RemoveAll(s => Common.IsZero(s.Item3));

            toolScores = toolScores.OrderByDescending(s => s.Item3).ToList();

            return toolScores;

        }

        /// <summary>
        /// scores the tool or input based on availabitlity of inputs, tools or currently in stock
        /// 
        /// also scores Used In items
        /// </summary>
        /// <param name="item"></param>
        /// <param name="productivity"></param>
        /// <param name="hasInputs"></param>
        /// <param name="hasTools"></param>
        /// <param name="isAvailable"></param>
        /// <returns></returns>
        protected float ScoreItem(EntityGroup resolvedOwner, EntityType item, float productivity, out bool hasInputs, out bool hasTools, out bool isAvailable, bool isToolContext)
        {
            bool ownsItemIncludingIntrinsicParts;
            bool ownsItem;

            //InventoryPanel.OwnsProductOrHasProcessInputsAndTools(item, resolvedOwner, out isAvailable, out ownsItemIncludingIntrinsicParts, out hasInputs, out hasTools, null, false); 
            InventoryPanel.OwnsProductOrHasProcessInputsAndTools(item, resolvedOwner, out ownsItem, out ownsItemIncludingIntrinsicParts, out hasInputs, out hasTools, null, true); 

           
            float score = 0f;
            if (isToolContext)
            {
                score = ScoreItem(item, ownsItemIncludingIntrinsicParts);
                isAvailable = ownsItemIncludingIntrinsicParts;
            }
            else
            {
                score = ScoreItem(item, ownsItem);
                isAvailable = ownsItem;
            }

            score += MathHelper.Lerp(0f, 0.05f, productivity);

            return score;
            
        }

        private static float ScoreItem(EntityType item, bool isAvailable)
        {
            Dictionary<ProcessType, AttainableInfo> attainableInfo = null;
            float score = 0f;

            if (!isAvailable)
            {
                attainableInfo = The.InGameUI.InventorySettings.GetAttainableInfo(item);
            }

            if (isAvailable)
            {
                score = 100f;
            }
            else
            {
                if (attainableInfo != null && attainableInfo.Any(a => a.Value.IsProducable))
                {
                    score = 10f;
                }
            }

            return score;
        }

        private void Collapse()
        {
            state = State.Collapsed;

            //  base.DisplayWindow.ViewPort.MouseOut += new MouseOutHandler(ViewPort_MouseOut);

            currentlyShownExpandedGrid = null;

            DisplayWindow.Width = collapsedWidth;
            DisplayWindow.Height = collapsedHeight;
        }

        protected virtual void Retire()
        {

        }

        public override void Hide()
        {
            if (btPin.IsChecked) // is only true if closed from outside! 
            {
                // stay open

                The.InGameUI.EntityTypeTooltipsStack.Remove(this);

            }
            else
            {

                DisplayWindow.Hide();

                Collapse();

                Retire();

                // uncheck parent button:
                if (SpawningControl != null)
                {
                    TextButton textButton = SpawningControl as TextButton;
                    if (textButton != null)
                    {
                        textButton.IsChecked = false;
                    }

                    SpawningControl = null;
                }


                // close child windows:
                // EntityTypeTooltip childWindow = The.InGameUI.GetChildTooltip(this);
                DataSheet childWindow = The.InGameUI.GetChildTooltip(this);


                if (childWindow != null)
                {
                    childWindow.Hide();

                    childWindow = null;
                }

                The.InGameUI.EntityTypeTooltipsStack.Remove(this);
                The.InGameUI.PinnedDataTypeTooltips.Remove(this);

                tooltipAnchor = null;
                timePassed = 0f;

                The.InGameUI.InventorySettings.TrackTargetsChanged -= InventorySettings_TrackTargetsChanged;
            }

            // ZoneGatherResourcesWindow.Hide();

            // ZoneStockpileWindow.Hide();
        }
    }
}
