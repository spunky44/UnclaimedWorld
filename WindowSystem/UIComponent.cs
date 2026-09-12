#region File Description
//-----------------------------------------------------------------------------
// File:      UIComponent.cs
// Namespace: WindowSystem
// Author:    Aaron MacDougall
//-----------------------------------------------------------------------------
#endregion

#region License
//-----------------------------------------------------------------------------
// Copyright (c) 2007, Aaron MacDougall
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice,
//   this list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of Aaron MacDougall nor the names of its contributors may
//   be used to endorse or promote products derived from this software without
//   specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
// POSSIBILITY OF SUCH DAMAGE.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using InputEventSystem;
#endregion

namespace WindowSystem
{
    #region Delegates
    /// <summary>
    /// When a control is moved.
    /// </summary>
    /// <param name="sender">Moved control.</param>
    public delegate void MoveHandler(UIComponent sender);

    /// <summary>
    /// When a control is resized.
    /// </summary>
    /// <param name="sender">Resized control.</param>
    public delegate void ResizeHandler(UIComponent sender);

    /// <summary>
    /// When the mouse enters a control's location rectangle.
    /// </summary>
    /// <param name="args">Mouse event arguments.</param>
    public delegate void MouseOverHandler(UIComponent sender, MouseEventArgs args);

    /// <summary>
    /// When the mouse leaves a control's location rectangle.
    /// </summary>
    /// <param name="args">Mouse event arguments.</param>
    public delegate void MouseOutHandler(UIComponent sender, MouseEventArgs args);

    /// <summary>
    /// When a control requires a redraw.
    /// </summary>
    /// <param name="sender">Control requiring redraw.</param>
    public delegate void RequiresRedrawHandler(UIComponent sender);

    /// <summary>
    /// When a control has been clicked (mouse pressed then released inside the
    /// area of a control).
    /// </summary>
    /// <param name="sender">Clicked control.</param>
    public delegate void ClickHandler(UIComponent sender, EventArgs e);

    /// <summary>
    /// When a control receives focus.
    /// </summary>
    public delegate void GetFocusHandler();

    /// <summary>
    /// When a control no longer has focus.
    /// </summary>
    public delegate void LoseFocusHandler();

    /// <summary>
    /// When a control such as a window is closed.
    /// </summary>
    /// <param name="sender">Closed control.</param>
    public delegate void CloseHandler(UIComponent sender);

    public delegate void DrawContentHandler();

    public delegate void UpdateHandler(GameTime gameTime);

   

    #endregion

    public enum RenderType { Normal, CRTAndLCD, Overlay }

   
   
    /// <summary>
    /// This is the base class for all GUI controls. Handles all the shared
    /// events.
    /// </summary>
    public partial class UIComponent 
    {
        #region Fields
       
        public GUIManager guiManager;       
      //  protected InputData inputData;
        private Rectangle location;
        private Point absolutePosition;
        private List<UIComponent> controls;
        private UIComponent parent;
        private int minWidth;
        private int minHeight;

        private int? maxWidth;
        private int? maxHeight;

        private float zOrder;
        private bool canHaveFocus;
        private bool isRedrawRequired;
        private bool isInitialized;
        private bool isAnimating;
        private bool isMouseOver;
        private bool isPressed;

        private bool rightIsPressed; // NEW


        private RenderType renderType = RenderType.Normal;

      //  public bool IsLCDSurface = false;


        private EventArgs eventArgs = null;

        protected string toolTip = null;

        bool visible = true;
        public virtual bool Visible
        {
            get { return visible; }
            set
            {

                bool oldValue = visible;
                visible = value;
                // new: set Visible on children too. It should affect mouseclicks and other events on the child controls (for example Combobox)
                if (controls != null)
                {
                    foreach (var item in controls)
                    {
                        item.Visible = value;
                    }
                }             
            }

        }

    //    private object toolTipObject = null;

        #endregion

        #region Colors

        public static Color lcdButtonHoverTint = Microsoft.Xna.Framework.Color.Azure;

        public static Color lcdHoverTint = Microsoft.Xna.Framework.Color.LightGray;
        public static Color lcdTooltipHoverTint = Microsoft.Xna.Framework.Color.LightGray;
        public static Color lcdTooltipPressedTint = Microsoft.Xna.Framework.Color.DarkGray;
        public static Color lcdPressedTint = Microsoft.Xna.Framework.Color.DarkGray;  
        public static Color lcdTooltipCheckedTint = Microsoft.Xna.Framework.Color.LightGray;
        public static Color lcdTooltipCheckedPressedTint = Microsoft.Xna.Framework.Color.DarkGray;
        public static Color lcdTooltipCheckedHoverTint = Microsoft.Xna.Framework.Color.LightGray;
                     
        public static Color panelHoverTint = Microsoft.Xna.Framework.Color.LightGray;
   

        public static Color hudHoverTint = Microsoft.Xna.Framework.Color.Yellow;
        public static Color hudHoverTintNonEnabled = Microsoft.Xna.Framework.Color.Gray;//Hover when button is not enabled and there is a skin
        public static Color hudPressedTint = new Color(205, 216, 52);
        public static Color hudCheckedTint = Microsoft.Xna.Framework.Color.Turquoise;
        public static Color hudCheckedPressedTint = Microsoft.Xna.Framework.Color.DarkTurquoise;
        public static Color hudCheckedHoverTint = Microsoft.Xna.Framework.Color.YellowGreen; 

       


        #endregion

        #region Events

        // the events are exposed as public events, but are also all hooked up to a protected method (prefixed with 'On') for internal use.

        public event MouseDownHandler MouseDown;
        public event MouseUpHandler MouseUp;
        public event MouseMoveHandler MouseMove;

        public event MouseWheelHandler MouseWheelChanged;

        public event KeyDownHandler KeyDown;
        public event KeyUpHandler KeyUp;
        public event ClickHandler Click;
        public event ClickHandler RightClick;
        public event MoveHandler Move;
        public event ResizeHandler Resize;
        public event ResizeHandler HeightResize;
        public event MouseOverHandler MouseOver;

        /// <summary>
        /// Note: is not invoked when moving to a child control! Difference from Windows Forms...
        /// 
        /// When a control has children with CanHaveFocus = true that form a bridge to the edge that the mouse can move over, then the control will never invoke this event.
        /// Set CanHaveFocus on those elements.
        /// </summary>
        public event MouseOutHandler MouseOut;
        public event RequiresRedrawHandler RequiresRedraw;
        public event GetFocusHandler GetFocus;
        public event LoseFocusHandler LoseFocus;        
        public event DrawContentHandler DrawContentEvent;        
        public event UpdateHandler UpdateEvent;

        /// <summary>
        /// Invoked when the Tooltip string is accessed. This happens periodically from the Tooltip class, while visible
        /// for lazy updates of the tooltip text. The tooltip is refreshed with the normal hud window refresh rate.
        /// </summary>
        public event Action<UIComponent> TooltipRequested;

        /// <summary>
        /// invoked when the tooltip gets shown or hidden. Use this to optimize generation of costly tooltips
        /// </summary>
        public event Action<UIComponent, bool> TooltipDisplayChange;
     //   public event Action<UIComponent> TooltipDisplayed;

        /// <summary>
        /// fires when the tooltip disappears again
        /// </summary>
      //  public event Action<UIComponent> TooltipHidden;


        public event Action Destroyed;

        #endregion

        public static Color lcdDisabledColor = Color.Gray;
        public static Color lcdHoverDisabledColor = Color.LightGray;

        public static Color HUDDisabledColor = Color.Gray;

        public static Color errorColor = new Color(0x95, 0x35, 0x40); // 953540 // 
        public static Color lcdLight = new Color(0xDF, 0xEE, 0xF0); //dfeef0 //new Color(0xD1, 0xE0, 0xE3); //D1E0E3
        public static Color lcdYellow = new Color(0xFF, 0xEB, 0x28);//FFEB28

        // 
        public static Color HUDLightTint = Util.ColorFromHex("#9FB5B2").Value; // "#BCD8D5").Value; // 

        public const string HUDTintHex = "#859997";
        public static Color HUDTint = Util.ColorFromHex(HUDTintHex).Value;

        public const string LCDTintHex = "#DFEDEE";
        public static Color LCDTint = Util.ColorFromHex(LCDTintHex).Value;

        public const string LCDNormalHex = "#1E525C";
        public static Color LCDNormal = Util.ColorFromHex(LCDNormalHex).Value; // new Color(0x1E, 0x52, 0x5C); // //#1e525c //475254 //  new Color(0x47, 0x52, 0x54);
        public static Color LCDDark = new Color(0x2D, 0x40, 0x46); // //#1e525c //475254 //  new Color(0x47, 0x52, 0x54);

        public static Color OffWhiteColor = new Color(0xE2, 0xE2, 0xE2);



        #region Properties

        public virtual string ToolTip
        {
            get 
            {
               
                if (TooltipRequested != null)
                {
                    TooltipRequested.Invoke(this);
                }

                return toolTip; 
            }

            set 
            {
               

                toolTip = value;
                if (!string.IsNullOrEmpty(toolTip))
                {
                    CanHaveFocus = true; // this is needed for labels to get the MouseOver event
                }
            }
        }

        

        bool tooltipExpires = true;
        public virtual bool TooltipExpires
        {
            get
            {
                return tooltipExpires;
            }

            set
            {
                tooltipExpires = value;
            }

        }

        int tooltipWidth = 200;
        public virtual int TooltipWidth
        {
            get
            {
                return tooltipWidth;
            }

            set
            {
                tooltipWidth = Math.Max(value, 50);
            }

        }

        /// <summary>
        /// this is used to find and update controls within a grid (because deleting and re-creating controls in real time impairs mouse clicks)
        /// </summary>
        public enum DataControlID
        {
            None, Caption, Stock, Available, Unavailable, CurrentOrders, MaxOrders, Build, Up, Down,
            Amount,
            Condition,
            Status, StatusIconBackground, StatusIcon,
            Background,
            Location,
            Immigration, Communication, Transport,
            SalvageAction, PackingDownAction, DiscardClaim,
            Filter,
            UpgradeAllow, UpgradeProhibit, UpgradeAllowItem, UpgradeProhibitIem, HasOverridingItemIcon,
            PriorityNormal, PriorityHigh, PriorityLow,
            Randomize, Option, Difficulty, Score, LeftGrid, Tools, ToolsError,
            JobType, JobsPanelAssignedWorker, JobStatus, Progress, Cancel, //AddAction, 
            Action,
            //AddActionLabel,
            Price,
            GoodsForSale,
            WillingToBuy,
            Connector,
            HorizontalConnector,
            ETA,
            NextStop,
            ErrorsAndMessages,
            Profession,
            Name,
            Selector,
            Age,
            Sex,
            Rating,
            DistanceCaption,
            Distance,
            CapacityCaption,
            Capacity,
            FuelConsumptionCaption,
            FuelConsumption,
            TravelTimeCaption,
            TravelTime,
            SmallArrow,
            BigArrow,
            Track,
            Expand,
            Attainable,
            Goods,
            MapTexture,
            PanelBox,
            GridInItemRow,
            RatingIcon,
            Bulk,
            Personnel,
            NotAttainableIcons,
            Warning,
            AttainableIcons,
            Priority,
            ApplyPriority,
            Skill,
            Passengers,
            CannotEmigrate,
            Icon,
            Actions,
            VotersFor,
            VotersAgainst,
            Prompt,
            VotersForIcon,
            VotersAgainstIcon,
            HasSelection,
            StandingOrderModeHotspot,
            StandingOrderModePadlock,
            TravelStatus,
            InputHeading,
            OfferDemand,
            BiggestConcernIcon,
            Orders,
            CurrentCategoryOrders,
            Habitat,
            Produced,
            Consumed,
            Degraded,
            CritterEaten,
            Disappeared,
            Overconsumed,
            Stored,
            DaysLeft,
            Value,
            Regrowth,
            Productivity,
            Killed

        }

        //public enum DataControlID { None, Caption, Stock, Available, CurrentOrders, Up, Down, Condition, Status, Location, SalvageAction, DiscardClaim, StockpileAllow, StockpileProhibit,PriorityNormal, PriorityHigh, PriorityUrgent, HasOverridingItemIcon, Randomize, Option, Difficulty, Score, Materials, Tools }
      
        /// <summary>
        /// this is used to identify controls within a row in a grid
        /// </summary>
        public DataControlID ID = DataControlID.None;

        /// <summary>
        /// this can be used for anything, but is most often used for storing the key for a grid item
        /// </summary>
        public object Tag1;

        /// <summary>
        /// NEW: custom presentation use this for storing the entityID for IHasExposedProperty
        /// </summary>
        public object Tag2;
        
        /// <summary>
        /// use this for a value to sort the grid by
        /// </summary>
        public object OrderByTag1;

        /// <summary>
        /// use this for a second value to sort the grid by
        /// </summary>
        public object OrderByTag2;

        public string Name = "";

        public string DebugTag = "";
      
        
        private bool enabled = true;
        public virtual bool Enabled
        {
            get
            {
                return enabled;
            }
            set
            {
                enabled = value;
            }

        }

        public virtual RenderType RenderType
        {
            get 
            {
                return renderType;
            }
            set 
            {
               
                renderType = value;
                foreach (UIComponent control in this.controls)
                {
                   /* if (!(control is ScrollBar) || value == RenderType.Normal)
                    {*/
                        control.RenderType = value;
                   /* }
                    else if ((control is ScrollBar && ((ScrollBar)control).ScrollType == ScrollBar.ScrollBarType.CommRoller))
                    {
                        control.RenderType = RenderType.CRTAndLCD;
                    }*/
                }
                
            }
            
        }

        public bool ClipThis = true;

        private Level level = Level.RockBottom;
        public Level Level
        {
            get
            {
                return level;
            }
            set
            {
                level = value;
                foreach (UIComponent control in this.controls)
                {
                    control.Level = value;                    
                }
            }

        }
           

        /// <summary>
        /// Gets the GUIManager object associated with this control.
        /// </summary>
        protected GUIManager GUIManager
        {
            get { return this.guiManager; }
        }

        /// <summary>
        /// Gets the IInputEventService object used for event handling.
        /// </summary>
        /*protected IInputEventsService InputEvents
        {
            get { return this.inputEvents; }
        }*/

        /// <summary>
        /// Gets the control location / bounds
        /// </summary>
        public Rectangle Location
        {
            get { return this.location; }
        }

        public virtual EventArgs EventArgs
        {
            get { return this.eventArgs; }
            set { this.eventArgs = value; }
        }

        /// <summary>
        /// Get/Set the x-position of this control in relation to the parent.
        /// </summary>
        public int X
        {
            get { return this.location.X; }
            set
            {
                if (this.location.X != value)
                {
                    if (value == 1581)
                    {

                    }

                    this.location.X = value;

                    if (Move != null)
                        Move.Invoke(this);
                }
            }
        }

        public int Right
        {
            get { return this.location.X + Width; }
        }

        public int MiddleVertical
        {
            get
            {
                return this.location.Y + this.Height / 2;
            }
        }

        /// <summary>
        /// Get/Set the y-position of this control in relation to the parent.
        /// </summary>
        public int Y
        {
            get { return this.location.Y; }
            set
            {
                if (this.location.Y != value)
                {
                    this.location.Y = value;

                    if (Move != null)
                        Move.Invoke(this);
                }
            }
        }

        public Point Position
        {
            set 
            { 
                X = value.X;
                Y = value.Y;
            }

            get { return location.Location; }
        }

        /// <summary>
        /// Get/Set the control width.
        /// </summary>
        public virtual int Width
        {
            get { return this.location.Width; }
            set
            {
                if (value != location.Width)
                {
                   
                    if (value < this.minWidth)
                        value = this.minWidth;

                    if (maxWidth.HasValue)
                    {
                        if (value > maxWidth.Value)
                        {
                            value = maxWidth.Value;
                        }
                    }

                    this.location.Width = value;

                    if (Resize != null)
                        Resize.Invoke(this);
                }
            }
        }

        protected bool drawChildrenFirst = false;
     
        public int Bottom
        {
            get
            {
                return Y + Height;
            }
        }

        /// <summary>
        /// Get/Set the control height.
        /// </summary>
        public virtual int Height
        {
            get { return this.location.Height; }
            set
            {
                if (this.location.Height != value)
                {
                   
                    if (value < this.minHeight)
                        value = this.minHeight;

                    if (maxHeight.HasValue)
                    {
                        if (value > maxHeight.Value)
                        {
                            value = maxHeight.Value;
                        }
                    }


                    this.location.Height = value;

                    if (Resize != null)
                        Resize.Invoke(this);

                    if (HeightResize != null)
                    {
                        HeightResize.Invoke(this);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the location on the screen of the control.
        /// </summary>
        public Point AbsolutePosition
        {
            get { return this.absolutePosition; }
        }

        /// <summary>
        /// Gets the list of child controls.
        /// </summary>
        public /*internal*/ List<UIComponent> Controls
        {
            get { return this.controls; }
        }

        /// <summary>
        /// Get/Set the direct parent.
        /// </summary>
        public virtual UIComponent Parent
        {
            get { return this.parent; }
            internal set
            {
                // Remove old event handlers
                if (this.parent != null)
                {
                    this.parent.Move -= OnParentMoved;
                    this.parent.Resize -= OnParentResized;
                }

                this.parent = value;

                // Add new event handlers
                if (this.parent != null)
                {
                    this.parent.Move += new MoveHandler(OnParentMoved);
                    this.parent.Resize += new ResizeHandler(OnParentResized);
                }

                // Update absolute position
                Refresh();
            }
        }

        /// <summary>
        /// Get/Set the minimum control width.
        /// </summary>
        /// <remarks>
        /// Control must always have a size of at least 1, otherwise
        /// calculations can become unpredictable, and DrawableUIComponent
        /// objects cannot create render targets.
        /// </remarks>
        /// <value>Must be at least 1.</value>
        public int MinWidth
        {
            get { return this.minWidth; }
            set
            {
                Debug.Assert(value > 0);
                this.minWidth = value;

                // If width is less than minimum width, resize
                if (Width < this.minWidth)
                    Width = this.minWidth;
            }
        }

        public int? MaxWidth
        {
            get { return this.maxWidth; }
            set
            {                
                this.maxWidth = value;

                // If width is less than minimum width, resize
                if (Width > this.maxWidth.Value)
                    Width = this.maxWidth.Value;
            }
        }

        public virtual int? MaxHeight
        {
            get { return this.maxHeight; }
            set
            {
                this.maxHeight = value;

                // If width is less than minimum width, resize
                if (Height > this.maxHeight.Value)
                    Height = this.maxHeight.Value;
            }
        }

        /// <summary>
        /// Get/Set the minimum control height.
        /// </summary>
        /// <remarks>
        /// Control must always have a size of at least 1, otherwise
        /// calculations can become unpredictable, and DrawableUIComponent
        /// objects cannot create render targets.
        /// </remarks>
        /// <value>Must be at least 1.</value>
        public virtual int MinHeight
        {
            get { return this.minHeight; }
            set
            {
                Debug.Assert(value > 0);
                this.minHeight = value;

                // If height is less than minimum height, resize
                if (Height < this.minHeight)
                    Height = this.minHeight;
            }
        }

        /// <summary>
        /// Get/Set the focusing z-order.
        /// </summary>
        public float ZOrder
        {
            get { return this.zOrder; }
            set
            {
                Debug.Assert(value >= 0.0f && value <= 1.0f);
                this.zOrder = value;
            }
        }

        public void CenterThisVertically(int YPosToCenterTo)
        {
            Y = YPosToCenterTo - Height / 2; 
        }

        /// <summary>
        /// will center a contained control within this control.
        /// </summary>
        /// <param name="childControl"></param>
      /*  public void CenterChildVertically(UIComponent childControl)
        {
            childControl.Y = (Height - childControl.Height) / 2;
        }*/



        /// <summary>
        /// will center a contained control within this control.
        /// 
        /// childCenter: optional vertical center coordinate for the child, if the child is not symmetrical.
        /// </summary>
        /// <param name="childControl"></param>
        public void CenterChildVertically(UIComponent childControl, int? childCenterYPos = null)
        {
            if (childCenterYPos.HasValue)
            {
                childControl.Y = (Height - 2 * childCenterYPos.Value) / 2;
            }
            else
            {
                childControl.Y = (Height - childControl.Height) / 2;
            }
        }

        


        /// <summary>
        /// will align the control parameter relative to this control
        /// </summary>
        /// <param name="controlToAlign"></param>
        public void AlignVertically(UIComponent controlToAlign)
        {
            controlToAlign.Y = Y + (Height - controlToAlign.Height) / 2;

        }

        /// <summary>
        /// aligns the right side of the component with the given position
        /// </summary>
        /// <param name="rightXPos"></param>
        public void AlignRight(int rightXPos)
        {
            X = rightXPos - Width;
        }

        public void CenterChildHorizontally(UIComponent childControl)
        {
            childControl.X = (Width - childControl.Width) / 2;
        }

        public void CenterHorizontally(int xPosToCenterAbout, UIComponent childControl)
        {
            childControl.X = xPosToCenterAbout - (childControl.Width / 2);
        }

        public void CenterThisHorizontally(int xPosToCenterAbout)
        {
            X = xPosToCenterAbout - Width / 2;
        }

        /// <summary>
        /// Get/Set whether this control receives focus. True by default.
        /// 
        /// Focus is needed for labels (and all other controls) to get the MouseOver event, that creates the hover effect
        /// Focus is also needed for the MouseUp event (Clicks).
        /// 
        /// Focus is evaluated from the parent down to its children. So the Grid cannot be allowed to take focus
        /// if it contains buttons. The grid cannot have a hover event while at the same time allow clicks to pass through to underlying controls.
        /// </summary>
        public bool CanHaveFocus
        {
            get { return this.canHaveFocus; }
            set 
            {                
                this.canHaveFocus = value; 
            }
        }

        /// <summary>
        /// False by default. Only outermost grids and listboxes should set this to true.
        /// </summary>
        public bool CanReceiveMouseWheelEvents
        {
            get;
            set;
        }


        /// <summary>
        /// Get/Set whether control needs to be redrawn.
        /// </summary>
        protected bool IsRedrawRequired
        {
            set { this.isRedrawRequired = value; }
            get { return this.isRedrawRequired; }
        }

        /// <summary>
        /// Get/Set whether this control has been initialised.
        /// </summary>
        internal protected bool IsInitialized
        {
            get { return this.isInitialized; }
            set { this.isInitialized = value; }
        }

        /// <summary>
        /// Get/Set whether the control is currently animating.
        /// </summary>
        internal protected bool IsAnimating
        {
            get { return this.isAnimating; }
            set { this.isAnimating = value; }
        }
        /// <summary>
        /// Gets whether the mouse is currently hovering over this control.
        /// </summary>
        internal protected bool IsMouseOver
        {
            get { return this.isMouseOver; }
        }

        /// <summary>
        /// Get/Set whether button is currently pressed.
        /// </summary>
        protected bool IsPressed
        {
            get { return this.isPressed; }
            set { this.isPressed = value; }
        }

        //protected bool RightI

       
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor sets up data and event handlers.
        /// </summary>
        /// <param name="game">The currently running Game object.</param>
        /// <param name="guiManager">GUIManager that this control is part of.</param>
        public UIComponent(GUIManager guiManager)
        {
            this.guiManager = guiManager;
           // this.inputData = null;
            this.absolutePosition = Point.Zero;
            this.controls = new List<UIComponent>();
            this.parent = null;

            // Minimum size of 1
            this.location = new Rectangle(0, 0, 1, 1);
            this.minWidth = 1;
            this.minHeight = 1;

            this.zOrder = 0.0f;
            this.canHaveFocus = true;
            this.isRedrawRequired = true;
            this.isInitialized = false;
            this.isAnimating = false;
            this.isMouseOver = false;
            this.isPressed = false;
            this.rightIsPressed = false;

            #region Event Handlers
            // the events are exposed as public events, but are also all hooked up to a protected method for internal use.

            this.MouseDown += new MouseDownHandler(OnMouseDown);
            this.MouseUp += new MouseUpHandler(OnMouseUp);
            this.MouseMove += new MouseMoveHandler(OnMouseMove);
            this.MouseOver += new MouseOverHandler(OnMouseOver);
            this.MouseOut += new MouseOutHandler(OnMouseOut);

            this.MouseWheelChanged += new MouseWheelHandler(OnMouseWheelChanged);

            // it seems strange and confusing that this class sends these events to itself and noone else...
            this.KeyDown += new KeyDownHandler(OnKeyDown);
            this.KeyUp += new KeyUpHandler(OnKeyUp);

            this.Move += new MoveHandler(OnMove);
            this.Resize += new ResizeHandler(OnResize);
            this.RequiresRedraw += new RequiresRedrawHandler(OnRequiresRedraw);
            this.GetFocus += new GetFocusHandler(OnGetFocus);
            this.LoseFocus += new LoseFocusHandler(OnLoseFocus);
            #endregion

          
        }

       

        /*
        /// <summary>
        /// Destructor.
        /// </summary>
        ~UIComponent()
        {
            instanceCount--;
        }*/

        #endregion


      /*  public void NotifyTooltipShown()
        {
            if (TooltipDisplayed != null)
            {
                TooltipDisplayed.Invoke(this);
            }
        }

        public void NotifyTooltipHidden()
        {
            if (TooltipHidden != null)
            {
                TooltipHidden.Invoke(this);
            }
        }*/

        public void NotifyTooltipShown()
        {
            if (TooltipDisplayChange != null)
            {
                TooltipDisplayChange.Invoke(this, true);
            }
        }

        public void NotifyTooltipHidden()
        {
            if (TooltipDisplayChange != null)
            {
                TooltipDisplayChange.Invoke(this, false);
            }
        }

        /// <summary>
        /// Registers event handlers with the input events system, and
        /// refreshes the control position.
        /// </summary>
        public virtual void Initialize()
        {
            foreach (UIComponent control in this.controls)
                control.Initialize();

            if (!this.isInitialized)
            {
                // Get input event system, and register event handlers
                /*inputData = guiManager.InputData; 

                if (inputData != null)
                {
                    // we want to de-register in Destroy().

                    // it seems wasteful to hook up all uicomponents to these events. A better design would be to let the GUIManager to only notify the focused control
                   // inputData.KeyDown += KeyDownIntercept;
                 //   inputData.KeyUp += KeyUpIntercept;

                    //inputData.MouseDown += MouseDownIntercept; // NEW #EVENTCHG
                    // inputData.MouseUp += MouseUpIntercept; // NEW #EVENTCHG
                    // inputData.MouseMove += MouseMoveIntercept; // NEW #EVENTCHG 
                }*/

                // Refreshing here allows controls to sort themselves out
                Refresh();
                
              
                this.isInitialized = true;

                // Important: Call LoadContent:
                LoadGraphicsContent(true);
            }
        }

      
      
        public virtual void UnloadGraphicsContent(bool unloadAllContent)
        {

        }

        protected virtual void LoadGraphicsContent(bool loadAllContent)
        {

        }

        /// <summary>
        /// sorts the stacking (drawing) order of child controls
        /// </summary>
        public void SortControls(Func<UIComponent, int> orderBy)
        {
            controls = controls.OrderBy(orderBy).ToList();

        }

        /// <summary>
        /// NOTE: There is no reliable way to destroy all UIComponents. Only the ones that are currently placed on a form can be reached.
        /// </summary>
        public virtual void Destroy()
        {

            if (controls != null)
            {
                foreach (var item in controls)
                {
                    item.Destroy();
                }
            }


            if (Destroyed != null)
            {
                Destroyed.Invoke();
            }           

        }

        /// <summary>
        /// Tidies up event handlers for this control, as well as all children.
        /// </summary>
        public virtual void CleanUp()
        {
            foreach (UIComponent control in this.controls)
                control.CleanUp();

            if (this.isInitialized)
            {
               // UnsubscribeEvents();

                if (this.guiManager.GetFocus() == this)
                {
                    
                    this.guiManager.SetFocus(null);
                }

                if (this.guiManager.GetModal() == this)
                    this.guiManager.SetModal(null);

                //base.Dispose(true);

                this.isInitialized = false;
            }
        }

      /*  private void UnsubscribeEvents()
        {
            if (this.inputData != null)
            {
                // Remove event handlers from input event system
                // performance will quickly degrade otherwise, as each new add/subscribe will take longer.
                //inputData.KeyDown -= KeyDownIntercept;
                //inputData.KeyUp -= KeyUpIntercept;

                // inputData.MouseDown -= MouseDownIntercept; // NEW #EVENTCHG
                //inputData.MouseUp -= MouseUpIntercept; // NEW #EVENTCHG
                //inputData.MouseMove -= MouseMoveIntercept;  // NEW #EVENTCHG               

            }
        }*/

        /// <summary>
        /// Update all child components.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public virtual void Update(GameTime gameTime)
        {
          /*  if (DebugTag == "statusSurface")
            {

            }*/

            foreach (UIComponent control in this.controls)
                control.Update(gameTime);

            if (UpdateEvent != null)
            {
                UpdateEvent.Invoke(gameTime);
            }

           // base.Update(gameTime);
        }

        /// <summary>
        /// Bring the control to the front. Currently only works with top-level
        /// controls.
        /// </summary>
        public virtual void BringToTop()
        {
            this.guiManager.BringToTop(this);
        }

        /// <summary>
        /// Is the specified control contained in this part of the GUI tree.
        /// </summary>
        /// <param name="control">Control to search for.</param>
        /// <returns>TRUE if control was found, otherwise FALSE.</returns>
        internal protected bool IsChild(UIComponent key)
        {
            if (key == this)
                return true;
            else
            {
                foreach (UIComponent control in this.controls)
                {
                    if (control.IsChild(key))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Adds a control as a child, if it has not already been added. Also
        /// initializes control.
        /// </summary>
        /// <param name="control">Control to add.</param>
        public virtual int Add(UIComponent control)
        {
            Insert(control, controls.Count);

            return controls.Count;

        }

        public bool Contains(UIComponent control)
        {
            return controls.Contains(control);
        }

        public void Insert(UIComponent control, int indexPosition)
        {
            if (!this.controls.Contains(control))
            {
                control.Parent = this;

                if (renderType != RenderType.Normal)
                {
                    control.RenderType = renderType;
                }

                control.ClipThis = ClipThis;
                control.Level = Level;

                control.Initialize();
                this.controls.Insert(indexPosition, control);
            }
        }

        /// <summary>
        /// Removes the child control.
        /// </summary>
        /// <param name="control">Control to remove.</param>
        /// <returns>TRUE if control existed, otherwise FALSE.</returns>
        public virtual bool Remove(UIComponent control)
        {
            bool result = false;

            if (this.controls.Remove(control))
            {
                control.Parent = null; // NEW!

                control.CleanUp();
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Checks a skin rectangle for valid dimensions.
        /// </summary>
        /// <param name="location">Skin rectangle to check.</param>
        /// <returns>true if rectangle is valid, otherwise false</returns>
        internal static bool CheckSkinLocation(Rectangle location)
        {
            bool result = false;

            if (
                location.X >= 0 &&
                location.Y >= 0 &&
                location.Width > 0 &&
                location.Height > 0
                )
                result = true;

            return result;
        }

        /// <summary>
        /// Invokes RequiresRedraw event.
        /// </summary>
        public void Redraw()
        {
            if (RequiresRedraw != null)
                RequiresRedraw.Invoke(this);
        }

        /// <summary>
        /// Refreshes control's position, as well as all children.
        /// </summary>
        protected void Refresh()
        {
           /* if (DebugTag == "displayPanel")
            {

            }*/

            if (this.parent != null)
            {
                this.absolutePosition.X = this.parent.AbsolutePosition.X + X;
                this.absolutePosition.Y = this.parent.AbsolutePosition.Y + Y;
            }
            else
            {
                this.absolutePosition.X = X;
                this.absolutePosition.Y = Y;
            }

            // Refresh children
            foreach (UIComponent control in this.controls)
                control.Refresh();
        }


        public Window GetParentWindow()
        {
            if (this is Window)
            {
                return (Window)this;
            }
            else if (parent != null)
            {
                return parent.GetParentWindow();
            }
            else return null;
        }

       
        /// <summary>
        /// Tells all children to draw themselves.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
       /* internal virtual void DrawControl(GameTime gameTime, SpriteBatch spriteBatch)
        {
            foreach (UIComponent control in this.controls)
                control.DrawControl(gameTime, spriteBatch);
        }*/

        /// <summary>
        /// Performs culling and clipping to inside the parent control, before
        /// calling DrawControl() which should be overridden to actually draw
        /// the control, and then it draws its children.
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch to draw with.</param>
        /// <param name="parentScissor">The scissor region of the parent control.</param>
        internal virtual void Draw(SpriteBatch spriteBatch, Rectangle parentScissor, RenderType typesToRender, float alpha)
        {            
            if (!Visible)
            {
                return;
            }
            
            // Create rectangle with the absolute dimensions of this control
            Rectangle thisScissor = location;
            thisScissor.X = absolutePosition.X;
            thisScissor.Y = absolutePosition.Y;

            // Cull this control if it isn't inside the parent
            bool result;
            if (ClipThis == false)
            {
                result = true;
            }
            else
            {
                parentScissor.Intersects(ref thisScissor, out result);
            }


            if (DebugTag == "rbDisabled") // "rgAccessButtons")
            {

            }

            if (result)
            {            

                if (ClipThis == true)
                {
                    ClipToParent(ref parentScissor, ref thisScissor);
                }
                              

                // Actually draw the control
                if (RenderType == typesToRender)
                {

                    if (drawChildrenFirst)
                    {
                        DrawChildren(spriteBatch, typesToRender, alpha, thisScissor);
                    }

                    DrawControl(spriteBatch, thisScissor, alpha);

                    if (DrawContentEvent != null)
                    {
                        DrawContentEvent.Invoke();
                    }
                }

                if (!drawChildrenFirst)
                {
                    DrawChildren(spriteBatch, typesToRender, alpha, thisScissor);
                }
            }
        }

        private void DrawChildren(SpriteBatch spriteBatch, RenderType typesToRender, float alpha, Rectangle thisScissor)
        {
            // Draw children
            foreach (UIComponent control in this.controls)
                control.Draw(spriteBatch, thisScissor, typesToRender, alpha);

            //return thisScissor;
        }

        private static void ClipToParent(ref Rectangle parentScissor, ref Rectangle thisScissor)
        {
            // Clip this control so it is inside the parent
            if (thisScissor.X < parentScissor.X)
            {
                thisScissor.Width -= parentScissor.X - thisScissor.X;
                thisScissor.X = parentScissor.X;
            }
            if (thisScissor.Right > parentScissor.Right)
                thisScissor.Width -= thisScissor.Right - parentScissor.Right;

            if (thisScissor.Y < parentScissor.Y)
            {
                thisScissor.Height -= parentScissor.Y - thisScissor.Y;
                thisScissor.Y = parentScissor.Y;
            }
            if (thisScissor.Bottom > parentScissor.Bottom)
                thisScissor.Height -= thisScissor.Bottom - parentScissor.Bottom;
        }

        /// <summary>
        /// Override to create a graphical control that draws itself.
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch to draw with.</param>
        /// <param name="parentScissor">The scissor region of the parent control.</param>
        protected virtual void DrawControl(SpriteBatch spriteBatch, Rectangle parentScissor, float alpha)
        {
        }

        #region Focusing

       
        /// <summary>
        /// Checks children if they can have focus, then checks itself. Uses
        /// children's z-order to determine which one should have focus.
        /// </summary>
        /// <param name="x">Mouse x-position.</param>
        /// <param name="y">Mouse y-position.</param>
        /// <returns>The object taking focus, otherwise null.</returns>
        internal UIComponent CheckFocus(int x, int y, TestMode mode)
        {
            UIComponent result = null;

            // Check if control is entitled to focus
            if (!this.isAnimating
                && this.Visible) // NEW: why would an invisible control or its children be able to take focus?
            {
               /* if (this.visible == false)
                {

                }*/

                // Check that the mouse position is inside this control
                if (CheckCoordinates(x, y))
                {
                    float zTemp = 0.0f;

                    // Go through each child asking them if they can have focus
                    foreach (UIComponent control in this.controls)
                    {
                        // Keep track of highest z-order
                        if (control.ZOrder >= zTemp)
                        {
                            UIComponent child = control.CheckFocus(x, y, mode);
                            if (child != null)
                            {
                                

                                // Set result and update highest z-order
                                result = child;
                                zTemp = control.ZOrder;
                            }
                        }
                        
                    }

                    // If no child took focus, see if this control can take it
                    if (result == null)
                    {
                        if (mode == TestMode.Focus && this.canHaveFocus)
                        {
                            result = this;
                        }
                        else if (mode == TestMode.MouseWheel && this.CanReceiveMouseWheelEvents)
                        {
                            result = this;
                        }
                    }
                    
                }
            }

            return result;
        }

        /// <summary>
        /// Asks children for their mouse status, and invokes MouseOut event
        /// for this control if necessary.
        /// </summary>
        /// <param name="args">Mouse event arguments.</param>
        /// <returns>Control asking for MouseOver event, otherwise null.</returns>
        internal UIComponent CheckMouseStatus(MouseEventArgs args, Rectangle parentScissor)
        {
            UIComponent result = null;

           /* if (DebugTag == "toolProd")
            {

            }*/

            // Check if control can receive MouseOver and MouseOut events
            if (!this.isAnimating && this.Visible) // NEW: hidden controls should not interfere                      
            {
                // Check if MouseOut event should be invoked
                // Create rectangle with the absolute dimensions of this control
                Rectangle thisScissor = location;
                thisScissor.X = absolutePosition.X;
                thisScissor.Y = absolutePosition.Y;

                // Cull this control if it isn't inside the parent
            /*    bool isInsideParent;
                parentScissor.Intersects(ref thisScissor, out isInsideParent);

                if (isInsideParent){*/

                ClipToParent(ref parentScissor, ref thisScissor);
                bool mouseOver = thisScissor.Contains(args.Position);

                //bool mouseOver = CheckCoordinates(args.Position.X, args.Position.Y);

              /*  bool shouldInvokeMouseOver = false;
                if (DebugTag == "ActionPicker")
                {
                    if (mouseOver)
                    {
                        if (this.isMouseOver)
                        {
                            Console.WriteLine("isMouseOver true, mouseOver true");
                        }
                        else
                        {
                            Console.WriteLine("isMouseOver false, mouseOver true -> Invoke mouseOver?");
                            shouldInvokeMouseOver = true;
                        }
                    }
                    else
                    {
                        if (this.isMouseOver)
                        {
                            Console.WriteLine("isMouseOver true, mouseOver false -> Invoke mouseOut?");
                        }
                        else
                        {
                            Console.WriteLine("isMouseOver false, mouseOver false");
                        }                       
                    }
                }*/

                if (this.isMouseOver)
                {
                   
                    if (!mouseOver)
                    {                      
                        InvokeMouseOut(args);
                    }
                }               
                 
                
                if (mouseOver)
                {
                   
                    float zTemp = 0.0f;
                    
                    // Ask each child to check its mouse status
                    foreach (UIComponent control in this.controls)
                    {
                        if (control.Visible) // NEW: hidden controls should not interfere
                        {
                            // Keep track of highest z-order
                            if (control.ZOrder >= zTemp)
                            {
                                UIComponent child = control.CheckMouseStatus(args, thisScissor);

                                if (child != null)
                                {
                                    // MouseOut last result
                                    if (result != null && result.IsMouseOver)
                                        result.InvokeMouseOut(args);

                                    // Set result and update highest z-order
                                    result = child;
                                    zTemp = control.ZOrder;
                                }
                            }
                        }
                    }

                   /* if (shouldInvokeMouseOver && result != null)
                    {
                        string tag;
                        if (!string.IsNullOrEmpty(result.DebugTag))
                        {
                            tag = result.DebugTag;
                        }
                        else
                        {
                            tag = result.GetType().Name;
                        }

                        Console.WriteLine("isMouseOver false, mouseOver true, Child result: " + tag );
                    }*/

                    // If no child requires the event, see if this control can
                    // take it.
                    if (result == null && this.canHaveFocus)
                        result = this;
                }
                else
                {
                    // Ensure each control can receive MouseOut event
                    // why is this needed..?
                    foreach (UIComponent control in this.controls)
                        control.CheckMouseStatus(args, thisScissor);
                }
            }

            return result;
        }

        //OLD:
        /*    internal UIComponent CheckMouseStatus(MouseEventArgs args)
        {
            UIComponent result = null;

#if DEBUG
            if (DebugTag == "scrollerKnob")
            {

            }
#endif

            // Check if control can receive MouseOver and MouseOut events
            if (!this.isAnimating)
            {
                // Check if MouseOut event should be invoked
               // bool mouseOver = CheckCoordinates(args.Position.X, args.Position.Y);
                bool mouseOver = CheckCoordinates(args.Position.X, args.Position.Y);
                if (this.isMouseOver)
                {
                    if (!mouseOver)
                    {
                        this.isMouseOver = false;
                        MouseOut.Invoke(args);
                    }
                }

                if (mouseOver)
                {
#if DEBUG
                    if (DebugTag == "countersRecess")
                    {

                    }
#endif
                    //inputEvents.MouseIsInInterface = true;
                    guiManager.mouseIsInInterface = true;

                    if (renderType == RenderType.CRTAndLCD)
                    {
                        if (IsLCDSurface) //DebugTag == "LCDSurface")
                        {
                            //inputEvents.MouseIsInLCDInterface = true;
                            if ((int)guiManager.LCDLevel <= (int)Level)
                            {
                                guiManager.MouseIsInLCDInterface = true;
                                guiManager.LCDLevel = this.Level;
                            }
                        }

                    }
                    else
                    {
                        if (!IsLCDSurface && (int)guiManager.LCDLevel < (int)Level)
                        {
                            guiManager.MouseIsInLCDInterface = false;
                            guiManager.LCDLevel = Level.RockBottom;
                        }
                    }
                  

                    float zTemp = 0.0f;

                    // Ask each child to check its mouse status
                    foreach (UIComponent control in this.controls)
                    {
                        // Keep track of highest z-order
                        if (control.ZOrder >= zTemp)
                        {
                            UIComponent child = control.CheckMouseStatus(args);

                            if (child != null)
                            {
                                // MouseOut last result
                                if (result != null && result.IsMouseOver)
                                    result.InvokeMouseOut(args);

                                // Set result and update highest z-order
                                result = child;
                                zTemp = control.ZOrder;
                            }
                        }
                    }

                    // If no child requires the event, see if this control can
                    // take it.
                    if (result == null && this.canHaveFocus)
                        result = this;
                }
                else
                {
                    // Ensure each control can receive MouseOut event
                    foreach (UIComponent control in this.controls)
                        control.CheckMouseStatus(args);
                }
            }

            return result;
        }*/

        /// <summary>
        /// Invokes MouseOver event.
        /// </summary>
        /// <param name="args">Event arguments.</param>
        internal void InvokeMouseOver(MouseEventArgs args)
        {
           
            this.isMouseOver = true;
            MouseOver.Invoke(this, args);
        }

        /// <summary>
        /// Invokes MouseOut event.
        /// </summary>
        /// <param name="args">Event arguments.</param>
        internal void InvokeMouseOut(MouseEventArgs args)
        {           
            this.isMouseOver = false;
            MouseOut.Invoke(this, args);
        }

        /// <summary>
        /// Invokes the GetFocus event.
        /// </summary>
        internal void GiveFocus()
        {
            if (GetFocus != null)
                GetFocus.Invoke();
        }

        /// <summary>
        /// Invokes the LoseFocus event.
        /// </summary>
        internal void TakeFocus()
        {
          /*  if (DebugTag == "combolist")
            {

            }*/

            if (LoseFocus != null)
                LoseFocus.Invoke();
        }

        /// <summary>
        /// Checks if the specified position is inside the control's absolute
        /// bounds.
        /// </summary>
        /// <param name="x">X-position.</param>
        /// <param name="y">Y-position.</param>
        /// <returns>true if inside, otherwise false.</returns>
        public bool CheckCoordinates(int x, int y)
        {
            bool result = false;
           
            if (x >= this.absolutePosition.X && (x < (this.absolutePosition.X + this.location.Width)))
            {
                if (y >= this.absolutePosition.Y && (y < (this.absolutePosition.Y + this.location.Height)))
                    result = true;
            }

            return result;
        }
        #endregion

        #region Event Handlers

        /// <summary>
        /// Receives KeyDown events when focused
        /// </summary>
        /// <param name="args">Key event arguments.</param>
        public virtual void KeyDownIntercept(KeyEventArgs args)
        {
            if (/*this.guiManager.GetFocus() == this
                &&*/ enabled)
            {
                KeyDown.Invoke(args);
            }
        }

        /// <summary>
        /// Receives KeyUp events when focused
        /// </summary>
        /// <param name="args">Key event arguments.</param>
        public virtual void KeyUpIntercept(KeyEventArgs args)
        {
            if (/*this.guiManager.GetFocus() == this
                && */ enabled)
            {
                KeyUp.Invoke(args);
            }
        }

        /// <summary>
        /// Receives all MouseDown events, and checks if control is focused and
        /// should receive event.
        /// </summary>
        /// <param name="args">Mouse event arguments.</param>
        //protected 
        public virtual void MouseDownIntercept(MouseEventArgs args)
        {


            // Check coordinates as well because modal mode messes up the
            // MouseDown event.
            if (/*this.guiManager.GetFocus() == this 
                &&*/ enabled 
                && CheckCoordinates(args.Position.X, args.Position.Y))
            {

                MouseDown.Invoke(args);
            }
        }

        /// <summary>
        /// Receives all MouseUp events, and checks if control is focused and
        /// should receive event.
        /// </summary>
        /// <param name="args">Mouse event arguments.</param>
        public virtual void MouseUpIntercept(MouseEventArgs args)
        {

            if (/*this.guiManager.GetFocus() == this 
                &&*/ enabled)
            {

                MouseUp.Invoke(args);
            }           
        }

        /// <summary>      
        /// Only called when the control has focus (= has been clicked on!)
        /// 
        /// </summary>
        /// <param name="args">Mouse event arguments.</param>
        public virtual void MouseMoveIntercept(MouseEventArgs args)
        {
            if (enabled)
            {
                 MouseMove.Invoke(args);
            }
        }

        /// <summary>
        /// Receives all MouseMove events, and checks if control is focused and
        /// should receive event.
        /// 
        /// Only called when the control has focus (= has been clicked on!)
        /// 
        /// </summary>
        /// <param name="args">Mouse event arguments.</param>
      /*  protected virtual void MouseMoveIntercept(MouseEventArgs args)
        {
            if (guiManager.GetFocus() == this
                && enabled)
            {
                MouseMove.Invoke(args);
            }
        }*/


        /// <summary>
        /// Different from the other intercept methods, GUIManager calls this directly. There's no reason why 1000s of UIComponents need a call.
        /// </summary>
        /// <param name="wheelChange"></param>
        public virtual void MouseWheelIntercept(int wheelChange) 
        {
            if (/*guiManager.GetFocus() == this
                &&*/ enabled)
            {
                MouseWheelChanged.Invoke(wheelChange); 
            }
        }


        /// <summary>
        /// Does nothing, override to handle event.
        /// </summary>
        /// <param name="args">Key event arguments.</param>
        protected virtual void OnKeyDown(KeyEventArgs args)
        {
        }

        /// <summary>
        /// Does nothing, override to handle event.
        /// </summary>
        /// <param name="args">Key event arguments.</param>
        protected virtual void OnKeyUp(KeyEventArgs args)
        {
        }

        /// <summary>
        /// Used for click event. Override to handle event.
        /// </summary>
        /// <param name="args">Mouse event arguments.</param>
        protected virtual void OnMouseDown(MouseEventArgs args)
        {
            if (args.Button == MouseButtons.Left)
            {

                this.isPressed = true;
            }
            else if (args.Button == MouseButtons.Right)
            {
                this.rightIsPressed = true;
            }
        }

        /// <summary>
        /// Used for click event. Override to handle event.
        /// </summary>
        /// <param name="args">Mouse event arguments.</param>
        protected virtual void OnMouseUp(MouseEventArgs args)
        {

            if (Enabled && Visible)
            {
                if (this.isPressed)
                {
                    if (args.Button == MouseButtons.Left)
                    {

                        this.isPressed = false;

                        if (Click != null)
                        {
                            // Check if button was clicked
                            if (CheckCoordinates(args.Position.X, args.Position.Y))
                            {

                                Click.Invoke(this, eventArgs);
                            }
                        }
                    }
                }
                else if (this.rightIsPressed)
                {
                    if (args.Button == MouseButtons.Right)
                    {
                        this.rightIsPressed = false;

                        if (RightClick != null)
                        {
                            // Check if button was clicked
                            if (CheckCoordinates(args.Position.X, args.Position.Y))
                            {
                                RightClick.Invoke(this, eventArgs);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Does nothing, override to handle event.
        /// 
        /// This only gets called for the control that has focus (= was last clicked on!)
        /// </summary>
        /// <param name="args">Mouse event arguments.</param>
        protected virtual void OnMouseMove(MouseEventArgs args)
        {
          
        }


        protected virtual void OnMouseOver(UIComponent sender, MouseEventArgs args)
        {           
            if (toolTip != null 
                && Visible == true)
            {
                guiManager.ShowToolTip(this);
            }
            
        }


        protected virtual void OnMouseOut(UIComponent sender, MouseEventArgs args)
        {
            guiManager.HideToolTip(this);
            
        }


        protected virtual void OnMouseWheelChanged(int wheelChange)
        {


        }

        /// <summary>
        /// Simply refreshes, override to handle event.
        /// </summary>
        /// <param name="args">Moved control.</param>
        protected virtual void OnMove(UIComponent sender)
        {
            Refresh();
        }

        /// <summary>
        /// Simply refreshes, override to handle event.
        /// </summary>
        /// <param name="args">Resized control.</param>
        protected virtual void OnResize(UIComponent sender)
        {
           /* if (DebugTag == "HelpWindow")
            {

            }*/

            Refresh();
        }

        /// <summary>
        /// Called when a parent control is moved. Invokes Move event.
        /// </summary>
        /// <param name="sender">Moved control.</param>
        protected virtual void OnParentMoved(UIComponent sender)
        {
            Move.Invoke(this);
        }

        /// <summary>
        /// Called when a parent controls is resized.
        /// </summary>
        /// <param name="sender">Resized control.</param>
        protected virtual void OnParentResized(UIComponent sender)
        {
        }

        /// <summary>
        /// Called when control needs to redraw it's texture. Events are used
        /// to filter redraw message to all parents.
        /// </summary>
        /// <param name="sender">Control requiring redraw.</param>
        protected virtual void OnRequiresRedraw(UIComponent sender)
        {
            this.isRedrawRequired = true;
            // Tell parent it also needs to redraw
            if (this.parent != null)
                this.parent.OnRequiresRedraw(sender);
        }

        public UIComponent FindParentOfType(Type componentType)
        {
            if (Parent == null)
            {
                return null;
            }
            else if (Parent != null && Parent.GetType() == componentType)
            {
                return Parent;
            }
            else
            {
                return Parent.FindParentOfType(componentType);
            }
        }

        public void FindChildById<T>(DataControlID id, out T child,  bool firstLevelOnly = false) where T : UIComponent
        {
            UIComponent childComponent = FindChildById(id, firstLevelOnly);
            if (childComponent != null)
            {
                child = (T)childComponent;
                return;
            }

            child = null;
        }

        /// <summary>
        /// recursively searches all child controls for the ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public UIComponent FindChildById(DataControlID id, bool firstLevelOnly = false)
        {
            foreach (UIComponent child in Controls)
            {
                if (child.ID == id)
                {
                    return child;
                }
            }

            if (!firstLevelOnly)
            {
                // go recursive too:
                UIComponent found = null;
                foreach (UIComponent child in Controls)
                {
                    found = child.FindChildById(id);
                    if (found != null)
                    {
                        return found;
                    }
                }
            }

            return null;
        }

        public void FindChildrenById(DataControlID id, ref List<UIComponent> foundChildren, bool firstLevelOnly = false)
        {
            foreach (UIComponent child in Controls)
            {
                if (child.ID == id)
                {
                    Util.AddToList(ref foundChildren, child);                    
                }
            }

            if (!firstLevelOnly)
            {
                // go recursive too:               
                foreach (UIComponent child in Controls)
                {
                    child.FindChildrenById(id, ref foundChildren, false);                    
                }
            }
                        
        }

        public UIComponent FindChildById(string id) 
        {
            foreach (UIComponent child in Controls)
            {
                if (/*child != ignoreThis &&*/ child.Name == id) 
                {
                    return child;
                }
            }

            UIComponent found = null;
            foreach (UIComponent child in Controls)
            {
                found = child.FindChildById(/*ignoreThis,*/ id);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }
        /// <summary>
        /// This is currently used in Window Hide()
        /// As this flag needs to be set for the window not to autoclose on the next Show..
        /// </summary>
        public void ResetMouseOver()
        {
            isMouseOver = false;
        }

        public void FindChildOfType<T>(UIComponent ignoreThis, ref List<T> foundChildren) where T: UIComponent
        {         
            foreach (UIComponent child in Controls)
            {
                if (child != ignoreThis && child is T) 
                {
                    if (foundChildren == null)
                        foundChildren = new List<T>();

                    foundChildren.Add((T)child);
                }		 
            }

            CollapsablePanel collapsablePanel = this as CollapsablePanel;
            if (collapsablePanel != null)
            {   // the panel does not belong to the controls collection when it is collapsed.
                collapsablePanel.ExpandedPanel.FindChildOfType(ignoreThis, ref foundChildren);
            }
            else
            {
                foreach (UIComponent child in Controls)
                {
                    child.FindChildOfType(ignoreThis, ref foundChildren);
                }
            }
            
        }

        /// <summary>
        /// Does nothing, override to handle event.
        /// </summary>
        /// <param name="args">Event arguments.</param>
        protected virtual void OnGetFocus()
        {

        }

        /// <summary>
        /// Happens when the modal dialog appears. Reset the control    
        /// </summary>
        /// <param name="args">Event arguments.</param>
        protected virtual void OnLoseFocus()
        {
            isPressed = false; // NEW
            rightIsPressed = false; // NEW
        }


        #endregion
    }
}