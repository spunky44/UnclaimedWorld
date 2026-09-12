#region File Description
//-----------------------------------------------------------------------------
// File:      Window.cs
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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using InputEventSystem;
#endregion

namespace WindowSystem
{
    /// <summary>
    /// When the window is transitioning.
    /// </summary>   
    public delegate void TransitioningHandler(Window sender);

    public enum Level
    {
        Min = 0,
        FoggyBottom = Min, // i am sad that FOW is a dialog...
        RockBottom, // HUD windows on the map? Only some HUD windows?
        Bottom, // minimap + important HUD windows

        BelowBelowBelowMiddle, // roster access panel
        BelowBelowMiddle, // roster panel
        BelowMiddle,
        Middle,       
        Dialogs, // world map... event dialog
        StackedDialogs, //, personneldialog, buy/sell
        EntityTypeInfo, //entity type info windows
        EventDialog, // NEW: event dialog only
        Menu, // Pause menu, Save/load, Help
        MessageBox, // Confirmation box, error...
        ComboBoxList,
        Tooltip, // only yellow tooltip
        Max // never use Max
    }
    /// <summary>
    /// Represents a graphical window, with a title bar, a close button, and
    /// movable and resizable areas. Most aspects can be removed, and the
    /// movable area can cover the whole window, like WinAmp or iTunes.
    /// </summary>
    public class Window : UIComponent
    {
        public event TransitioningHandler Transitioning;

        #region Default Properties
        private static bool defaultHasCloseButton = true;
        private static bool defaultHasTitleBar = true;
        private static bool defaultFullWindowMovableArea = true;
        private static int defaultTitleBarHeight = 24;
        private static int defaultButtonSize = 20;
        private static int defaultMargin = 0; // 5;
        private static float defaultAnimationTransparency = 0.75f;
        private static string defaultTitleFont = "Content/Fonts/DefaultHeading";
        private static Rectangle defaultSkin = new Rectangle(15, 1, 15, 15); //new Rectangle(15, 1, 30, 30); 
        private static Rectangle defaultTitleBarSkin = new Rectangle(1, 1, 13, 25);
        private static Rectangle defaultCloseButtonSkin = new Rectangle(1, 168, 20, 20);
        private static Rectangle defaultCloseButtonHoverSkin = new Rectangle(22, 168, 20, 20);
        private static Rectangle defaultCloseButtonPressedSkin = new Rectangle(43, 168, 20, 20);

        private static float timeToTransitionIn = 0.2f;
        private static float timeToTransitionOut = 0.2f;

        private Level level = Level.Bottom;

        public bool HasCRTOrLCDComponents = false;
     //   public bool HasCRTComponents = false;
        public bool HasOverlayComponents = false;

        public bool IsBackgroundGraphics = false;

        public Level Level
        {
            get { return level; }
            set 
            {
                if (level != value)
                {
                    guiManager.Remove(this);
                    level = value;
                    guiManager.Add(this);
                }
                else
                {
                    guiManager.Add(this);
                }
                
                foreach (UIComponent control in this.Controls)
                {
                    control.Level = value;                    
                }
            
            }
        }

        /// <summary>
        /// Sets whether windows have close buttons by default.
        /// </summary>
        [SkinAttribute]
        public static bool DefaultHasCloseButton
        {
            set { defaultHasCloseButton = value; }
        }

        /// <summary>
        /// Sets whether the movable area covers the whole window by default.
        /// </summary>
        [SkinAttribute]
        public static bool DefaultHasFullWindowMovableArea
        {
            set { defaultFullWindowMovableArea = value; }
        }

        /// <summary>
        /// Sets the default title bar height.
        /// </summary>
        /// <value>Must be greater than 0.</value>
        [SkinAttribute]
        public static int DefaultTitleBarHeight
        {
            set
            {
                Debug.Assert(value > 0);
                defaultTitleBarHeight = value;
            }
        }

        /// <summary>
        /// Sets the default close button width and height.
        /// </summary>
        /// <value>Must be greater than 0.</value>
        [SkinAttribute]
        public static int DefaultButtonSize
        {
            set
            {
                Debug.Assert(value > 0);
                defaultButtonSize = value;
            }
        }

        /// <summary>
        /// Sets the default distance from the edge to display child controls.
        /// </summary>
        /// <value>Must be at least 0.</value>
        [SkinAttribute]
        public static int DefaultMargin
        {
            set
            {
                Debug.Assert(value >= 0);
                defaultMargin = value;
            }
        }

        /// <summary>
        /// Sets the default title textfont.
        /// </summary>
        /// <value>Must be a non-empty string.</value>
        [SkinAttribute]
        public static string DefaultTitleFont
        {
            set
            {
                Debug.Assert(value != null);
                Debug.Assert(value.Length > 0);
                defaultTitleFont = value;
            }
        }

        /// <summary>
        /// Sets the default window background skin.
        /// </summary>
        [SkinAttribute]
        public static Rectangle DefaultSkin
        {
            set { defaultSkin = value; }
        }

        /// <summary>
        /// Sets the default title bar skin.
        /// </summary>
        [SkinAttribute]
        public static Rectangle DefaultTitleBarSkin
        {
            set { defaultTitleBarSkin = value; }
        }

        /// <summary>
        /// Sets the default close button skin.
        /// </summary>
        [SkinAttribute]
        public static Rectangle DefaultCloseButtonSkin
        {
            set { defaultCloseButtonSkin = value; }
        }

        /// <summary>
        /// Sets the default hover close button skin.
        /// </summary>
        [SkinAttribute]
        public static Rectangle DefaultCloseButtonHoverSkin
        {
            set { defaultCloseButtonHoverSkin = value; }
        }

        /// <summary>
        /// Sets the default pressed close button skin.
        /// </summary>
        [SkinAttribute]
        public static Rectangle DefaultCloseButtonPressedSkin
        {
            set { defaultCloseButtonPressedSkin = value; }
        }
        #endregion

        #region Fields

        /// <summary>
        /// don't add controls to this!
        /// </summary>
        private Box box;

        /// <summary>
        /// it is ok to add controls to this surface!
        /// </summary>
        public UIComponent ViewPort;

        /// <summary>
        /// what is this used for?
        /// </summary>
        private MovableArea movableArea;
        private MovableArea backgroundMovableArea;
        private Label label;
        private ImageButton closeButton;
        private ResizableArea[] resizableAreas;
        private bool hasCloseButton;
      //  private bool hasTitleBar;
        private bool fullWindowMovableArea;
        private int margin;
        private int resizableBorder;
        private bool isResizable;
        private float transparency;

        private enum Transition { In, Out, None }
        private Transition transitioning = Transition.None;

        

        public bool HideThisNow = false;

        /// <summary>
        /// 0 - 1
        /// </summary>
        private float transitionValue;

        /// <summary>
        /// use this to make translucent windows
        /// </summary>
        public float Opacity = 1f;


        private float alpha = 1f;

        public string ID;
        #endregion


        #region Properties

        public float TransitionValue
        {
            get { return transitionValue; }
            set 
            { 
                transitionValue = value;
                if (Transitioning != null)
                {
                    Transitioning.Invoke(this);
                }
            }
        }

        /// <summary>
        /// WARNING! Stays true while transitioning out!
        /// </summary>
        public bool Visible; // { get; private set; }

        /// <summary>
        /// is also false while transitioning out after closing. Call this when doing data source refreshes etc.
        /// </summary>
        public bool IsVisibleAndActive
        {
            get
            {
                if (transitioning == Transition.Out || Visible == false)
                {
                    return false;
                }
                else return true;
            }
        }

        /// <summary>
        /// Get/Set whether window can be resized by the user.
        /// </summary>
        [SkinAttribute]
        public bool Resizable
        {
            get { return this.isResizable; }
            set
            {
                this.isResizable = value;

                // Simply change focus settings of resizable area, preventing
                // them from receiving focus when window is not resizable.
                if (this.isResizable)
                {
                    for (int i = 0; i < 8; i++)
                        this.resizableAreas[i].CanHaveFocus = true;
                }
                else
                {
                    for (int i = 0; i < 8; i++)
                        this.resizableAreas[i].CanHaveFocus = false;
                }
            }
        }

        public Vector2 WindowSize
        {
            get
            {
                return new Vector2(ClientWidth, ClientHeight);
            }
            set 
            { 
                ClientWidth = (int)value.X;
                ClientHeight = (int)value.Y;
            }
        }

        /// <summary>
        /// Sets the width of the window client area.
        /// </summary>
        public int ClientWidth
        {
            get { return this.ViewPort.Width; }
            set { Width = value + (this.margin * 2); }
        }

        /// <summary>
        /// Sets the height of the window client area.
        /// </summary>
        public int ClientHeight
        {
            get { return this.ViewPort.Height; }
            set { Height = value + this.margin; } //Height = value + (HasTitleBar? TitleBarHeight: 0) + this.margin; }
        }

        /// <summary>
        /// Get/Set whether the window has a close button.
        /// </summary>
        [SkinAttribute]
        public bool HasCloseButton
        {
            get { return this.hasCloseButton; }
            set
            {
                if (this.hasCloseButton && !value)
                    base.Remove(this.closeButton);
                else if (!this.hasCloseButton && value)
                    base.Add(this.closeButton);

                this.hasCloseButton = value;
            }
        }
/*
        /// <summary>
        /// Get/Set whether the window has a close button.
        /// </summary>
        [SkinAttribute]
        public bool HasTitleBar
        {
            get { return this.hasTitleBar; }
            set
            {
                if (this.hasTitleBar && !value)
                    base.Remove(this.titleBar);
                else if (!this.hasTitleBar && value)
                    base.Add(this.titleBar);

                this.hasTitleBar = value;
            }
        }
    */

        /// <summary>
        /// Get/Set whether the movable area covers the whole window.
        /// </summary>
        [SkinAttribute]
        public bool IsMovable
        {
            get { return this.fullWindowMovableArea; }
            set
            {
                this.fullWindowMovableArea = value;

                if (this.fullWindowMovableArea)
                {
                    this.ViewPort.Add(this.backgroundMovableArea);
                    // Set parent to this so window is moved instead of
                    // viewport.
                    this.backgroundMovableArea.Parent = this;
                }
                else
                    this.ViewPort.Remove(this.backgroundMovableArea);
            }
        }

        /// <summary>
        /// Get/Set the title bar height.
        /// </summary>
        /// <value>Must be greater than 0.</value>
    /*    [SkinAttribute]
        public int TitleBarHeight
        {
            get { return this.titleBar.Height; }
            set
            {
               // Debug.Assert(value > 0);

                if (value > 0)
                {
                    this.titleBar.Height = value;
                    this.movableArea.Height = value;

                    MinHeight = value;
                }
                                

                this.viewPort.Y = value;
                this.backgroundMovableArea.Y = this.viewPort.Y;

                // Ensure client size remains the same
                ClientHeight = ClientHeight;
            }
        }*/

        /// <summary>
        /// Get/Set the close button width and height.
        /// </summary>
        /// <value>Must be at least 0.</value>
        [SkinAttribute]
        public int ButtonSize
        {
            get { return this.closeButton.Width; }
            set
            {
                Debug.Assert(value > 0);

                this.closeButton.Width = value;
                this.closeButton.Height = value;
               // this.closeButton.Y = (TitleBarHeight - value) / 2;
                this.closeButton.X = Width - value - this.closeButton.Y;
            }
        }

        public int CornerSize
        {
          //  get { return cornerSize; }
            set
            {                
                this.box.CornerSize = value;                
            }
        }

        public int ResizableBorderSize
        {
            get { return resizableBorder; }
            set
            {
                resizableBorder = value;
                RefreshResizableAreas();
            }
        }

        /// <summary>
        /// hide/show the box panel with edges and middle
        /// </summary>
        public bool ShowPanel
        {
            set
            {
                if (value == false)
                {
                    box.DebugTag = "Removed";
                    //Remove(box);
                    if (base.Controls.Remove(box))
                    {
                        box.CleanUp();                        
                    }
                    //Controls.Remove(box);
                }
                else 
                {
                    Add(box);
                }
               /* else if (value == true && !Controls.Contains(box))
                {
                    Controls.Add(box);
                }*/
            }
        }

        public Image Background
        {
            set
            {
              /*  value.Parent = this;
                               
                value.Initialize();
                Controls.Insert(0, value);
                */
                base.Add(value);
                
            }
        }

        /// <summary>
        /// Get/Set the padding between the window edge and controls.
        /// </summary>
        /// <value>Must be at least 0.</value>
        [SkinAttribute]
        public int Margin
        {
            get { return margin; }
            set
            {
                Debug.Assert(value >= 0);
                
                this.margin = value;

                //Lars: I remove this, because I want corner sprites and window margin to be unrelated!
              //  this.box.CornerSize = value;

                // Align title bar text with the margin
                this.label.X = value;

                this.ViewPort.X = value;
                // added by Lars:
                this.ViewPort.Y = value;

                //this.backgroundMovableArea.X = value;
                this.ViewPort.Width = Width - (value * 2);
                this.ViewPort.Height = Height  - value; //Height - (HasTitleBar? TitleBarHeight: 0) - value; // Lars: No longer using titlebars...
                
                //Lars: I remove this, because I want the resizable border and window margin to be unrelated!
               // RefreshResizableAreas();

                // Save client height, because resetting width will cause a
                // resize.
                int clientHeight = ClientHeight;
                // Ensure client size remains the same
                ClientWidth = ClientWidth;
                ClientHeight = clientHeight;
            }
        }


      


        /// <summary>
        /// Get/Set the title text.
        /// </summary>
        /// <value>Must not be null.</value>
        public string TitleText
        {
            get { return this.label.Text; }
            set { this.label.Text = value; }
        }

     /*   /// <summary>
        /// Sets the font of the title text.
        /// </summary>
        /// <value>Must be a valid path.</value>
        [SkinAttribute]
        public SpriteFont TitleFont
        {
            set
            {
                this.label.Font = value;
                // Centre title vertically
                this.label.Height = this.label.TextHeight;
                this.label.Y = (TitleBarHeight - this.label.Height) / 2;
            }
        }*/

        /// <summary>
        /// Sets the background skin.
        /// </summary>
        [SkinAttribute]
        public Rectangle Skin
        {
            set { this.box.SetSkinLocation(SkinState.Normal,value); }
        }


        public void SetSkin(Rectangle source, Color? edgeColor = null, Color? centerColor = null)
        {
            this.box.SetSkinLocation(SkinState.Normal,source, edgeColor, centerColor);
        }

      /*  /// <summary>
        /// Gets the title bar skin.
        /// </summary>
        [SkinAttribute]
        public Rectangle TitleBarSkin
        {
            set { this.titleBar.SetSkinLocation(SkinState.Normal,value); }
        }*/

        /// <summary>
        /// Sets the close button skin.
        /// </summary>
        [SkinAttribute]
        public Rectangle CloseButtonSkin
        {
            set { this.closeButton.SetSkinLocation(SkinState.Normal,value); }
        }

        /// <summary>
        /// Sets the hover close button skin.
        /// </summary>
        [SkinAttribute]
        public Rectangle CloseButtonHoverSkin
        {
            set { this.closeButton.SetSkinLocation(1, value); }
        }

        /// <summary>
        /// Sets the pressed close button skin.
        /// </summary>
        [SkinAttribute]
        public Rectangle CloseButtonPressedSkin
        {
            set { this.closeButton.SetSkinLocation(2, value); }
        }
        #endregion

        #region Events
        public delegate void DrawContentDelegate(Window sender, SpriteBatch spriteBatch);
        public event DrawContentDelegate DrawContentEvent;
        public event CloseHandler Close;

        /// <summary>
        /// this is useful because a rexize is usually both a reposition and a dimension change. Only registering for one of them would miss/lag behind the other property change
        /// </summary>
       
        #endregion


        public void SetResizableArea(ResizeAreas area, bool isResizable)
        {
            foreach (var r in resizableAreas)
            {
                if (r.ResizeArea == area)
                {
                    r.CanHaveFocus = isResizable;
                   
                    return;
                }
            }

         /*   for (int i = 0; i < resizableAreas.Length; i++)
            {
                if (resizableAreas[i].ResizeArea == area)
                {
                    
                    this.resizableAreas[i].CanHaveFocus = isResizable;
                  
                    return;
                }
            }*/

        }

        #region Constructors
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game">The currently running Game object.</param>
        /// <param name="guiManager">GUIManager that this control is part of.</param>
        public Window(GUIManager guiManager)
            : base(guiManager)
        {
            this.isResizable = true;
            this.transparency = -1;

            #region Create Child Controls
            this.box = new Box(guiManager);
            box.DebugTag = "windowBox";
            this.ViewPort = new UIComponent(guiManager);
          //  this.titleBar = new Bar(game, guiManager);
            this.movableArea = new MovableArea(guiManager);
            this.backgroundMovableArea = new MovableArea(guiManager);
            this.label = new Label(guiManager);
            this.closeButton = new ImageButton(guiManager);
            #endregion

            #region Add Child Controls
            base.Add(this.box); // this is the draw order too! very important.
            base.Add(this.ViewPort);            
        //    base.Add(this.titleBar);
            base.Add(this.label);
            base.Add(this.movableArea);
            #endregion

            #region Add Resizable Areas
            this.resizableAreas = new ResizableArea[8];

            for (int i = 0; i < 8; i++)
            {
                this.resizableAreas[i] = new ResizableArea(guiManager);
                this.resizableAreas[i].ZOrder = 0.3f;
                this.resizableAreas[i].StartResizing += new StartResizingHandler(OnStartAnimating);
               // this.resizableAreas[i].EndResizing += new EndResizingHandler(OnEndAnimating);
                this.resizableAreas[i].EndResizing += Window_EndResizing;
                base.Add(this.resizableAreas[i]);
            }

            this.resizableAreas[0].ResizeArea = ResizeAreas.TopLeft;
            this.resizableAreas[1].ResizeArea = ResizeAreas.Top;
            this.resizableAreas[1].DebugTag = "resizeTop";
            this.resizableAreas[2].ResizeArea = ResizeAreas.TopRight;
            this.resizableAreas[3].ResizeArea = ResizeAreas.Left;
            this.resizableAreas[4].ResizeArea = ResizeAreas.Right;
            this.resizableAreas[5].ResizeArea = ResizeAreas.BottomLeft;
            this.resizableAreas[6].ResizeArea = ResizeAreas.Bottom;
            this.resizableAreas[7].ResizeArea = ResizeAreas.BottomRight;
            #endregion

            #region Set Non-Default Properties
            this.movableArea.ZOrder = 0.1f;
            this.closeButton.ZOrder = 0.4f;
            this.ViewPort.ZOrder = 0.2f;
            this.ViewPort.CanHaveFocus = true;
            this.ViewPort.CanReceiveMouseWheelEvents = true;
            this.CanReceiveMouseWheelEvents = true;
            MinWidth = 16;
            #endregion

            #region Set Default Properties
            Margin = defaultMargin;
            HasCloseButton = defaultHasCloseButton;
            //HasTitleBar = defaultHasTitleBar;
            IsMovable = defaultFullWindowMovableArea;
            //TitleBarHeight = defaultTitleBarHeight;
            Width = MinWidth;
            Height = MinHeight;
            ButtonSize = defaultButtonSize;
            Skin = defaultSkin;
            //TitleBarSkin = defaultTitleBarSkin;
            CloseButtonSkin = defaultCloseButtonSkin;
            CloseButtonHoverSkin = defaultCloseButtonHoverSkin;
            CloseButtonPressedSkin = defaultCloseButtonPressedSkin;
            #endregion

            #region Event Handlers
            this.closeButton.Click += new ClickHandler(OnClose);
            this.movableArea.StartMoving += new StartMovingHandler(OnStartAnimating);
            this.movableArea.EndMoving += new EndMovingHandler(OnEndAnimating);
            this.backgroundMovableArea.StartMoving += new StartMovingHandler(OnStartAnimating);
            this.backgroundMovableArea.EndMoving += new EndMovingHandler(OnEndAnimating);
            //this.backgroundMovableArea.Click += backgroundMovableArea_Click;
            #endregion


            guiManager.RegisterWindow(this);

        }

       /* void backgroundMovableArea_Click(UIComponent sender, EventArgs e)
        {
            
        }*/

        void Window_EndResizing(UIComponent sender)
        {
            OnEndAnimating(sender);
        }

        #endregion

        /// <summary>
        /// Tidy controls that might not have been added.
        /// </summary>
        /// <remarks>
        /// For some reason, the viewPort control needs to be cleaned up here,
        /// even though it is always added to Window. Will investigate in the
        /// future, but for now this allows Window controls to be garbage
        /// collected.
        /// </remarks>
        public override void CleanUp()
        {
            this.closeButton.CleanUp();
            this.ViewPort.CleanUp();
            this.backgroundMovableArea.CleanUp();

            base.CleanUp();
        }


       

        /// <summary>
        /// Loads default font.
        /// </summary>
        /// <param name="loadAllContent">Which type of content to load.</param>
        protected override void LoadGraphicsContent(bool loadAllContent)
        {
            /* OBSDEFAULT
            if (loadAllContent)
                TitleFont = defaultTitleFont;
            */
            base.LoadGraphicsContent(loadAllContent);
        }

        /// <summary>
        /// Add first! It will set default values
        /// Add child controls to viewport, so user doesn't have to worry about
        /// title bars etc.
        /// </summary>
        /// <param name="control">Control to add.</param>
        public override int Add(UIComponent control)
        {
            // Add to viewport
            return this.ViewPort.Add(control);
        }

        /// <summary>
        /// Remove child controls from viewport, so user doesn't have to worry
        /// about title bars etc.
        /// </summary>
        /// <param name="control">Control to remove.</param>
        /// <returns>Removal result.</returns>
        public override bool Remove(UIComponent control)
        {
            return this.ViewPort.Remove(control);
        }

        /// <summary>
        /// Centres the window on the screen.
        /// </summary>
        public void CenterWindow()
        {
            X = (guiManager.ScreenWidth / 2) - (Width / 2);
            Y = (guiManager.ScreenHeight / 2) - (Height / 2);
            /*X = (guiManager.Game.Window.ClientBounds.Width / 2) - (Width / 2);
            Y = (guiManager.Game.Window.ClientBounds.Height / 2) - (Height / 2);*/
        }

        /// <summary>
        /// Shows window in modal mode, no other controls can receive focus
        /// while window is open.
        /// </summary>
     /*   public void Show(bool modal)
        {
            GUIManager.Add(this);
            if (modal)
                GUIManager.SetModal(this);
        }*/

        public void ShowModal()
        {
            Show();
            GUIManager.SetModal(this);
        }

       
        public void Show()
        {            
            GUIManager.Add(this);

            transitioning = Transition.In;

            ResetAllScrollBars();

            Visible = true;
            //Enabled = true;
        }

        /// <summary>
        /// Closes window. Don't call this directly! Call Hide() instead.
        /// </summary>
        internal void CloseWindow()
        {
            Visible = false;

            guiManager.WindowWasClosed(this);

            if (Parent == null)
                GUIManager.Remove(this);
            else
                GUIManager.Remove(this);

            if (Close != null)
                Close.Invoke(this);
        }


        private void ResetAllScrollBars()
        {
            // set scrollbar at the top
            List<ScrollBar> scrollbars = null; 
            FindChildOfType<ScrollBar>(null, ref scrollbars);

            if (scrollbars != null)
            {
                foreach (var item in scrollbars)
                {
                    item.Value = 0;
                }
            }
        }

     /*   public int GetContentWidth()
        {
            return Width - 2 * margin;
        }
        */
        /// <summary>
        /// Sets the positions and sizes of resizable areas.
        /// </summary>
        private void RefreshResizableAreas()
        {
            // Top left
            this.resizableAreas[0].X = 0;
            this.resizableAreas[0].Y = 0;
            this.resizableAreas[0].Width = this.resizableBorder;
            this.resizableAreas[0].Height = this.resizableBorder;

            // Top
            this.resizableAreas[1].X = this.resizableBorder;
            this.resizableAreas[1].Y = 0;
            this.resizableAreas[1].Width = Width - (2 * this.resizableBorder);
            this.resizableAreas[1].Height = this.resizableBorder;

            // Top right
            this.resizableAreas[2].X = Width - this.resizableBorder;
            this.resizableAreas[2].Y = 0;
            this.resizableAreas[2].Width = this.resizableBorder;
            this.resizableAreas[2].Height = this.resizableBorder;

            // Left
            this.resizableAreas[3].X = 0;
            this.resizableAreas[3].Y = this.resizableBorder;
            this.resizableAreas[3].Width = this.resizableBorder;
            this.resizableAreas[3].Height = Height - (2 * this.resizableBorder);

            // Right
            this.resizableAreas[4].X = Width - this.resizableBorder;
            this.resizableAreas[4].Y = this.resizableBorder;
            this.resizableAreas[4].Width = this.resizableBorder;
            this.resizableAreas[4].Height = Height - (2 * this.resizableBorder);

            // Bottom left
            this.resizableAreas[5].X = 0;
            this.resizableAreas[5].Y = Height - this.resizableBorder;
            this.resizableAreas[5].Width = this.resizableBorder;
            this.resizableAreas[5].Height = this.resizableBorder;

            // Bottom
            this.resizableAreas[6].X = this.resizableBorder;
            this.resizableAreas[6].Y = Height - this.resizableBorder;
            this.resizableAreas[6].Width = Width - (2 * this.resizableBorder);
            this.resizableAreas[6].Height = this.resizableBorder;

            // Bottom right
            this.resizableAreas[7].X = Width - this.resizableBorder;
            this.resizableAreas[7].Y = Height - this.resizableBorder;
            this.resizableAreas[7].Width = this.resizableBorder;
            this.resizableAreas[7].Height = this.resizableBorder;
        }
        /*
        /// <summary>
        /// Sets the positions and sizes of resizable areas.
        /// </summary>
        private void RefreshResizableAreas()
        {
            // Top left
            this.resizableAreas[0].X = 0;
            this.resizableAreas[0].Y = 0;
            this.resizableAreas[0].Width = this.margin;
            this.resizableAreas[0].Height = this.margin;

            // Top
            this.resizableAreas[1].X = this.margin;
            this.resizableAreas[1].Y = 0;
            this.resizableAreas[1].Width = Width - (2 * this.margin);
            this.resizableAreas[1].Height = this.margin;

            // Top right
            this.resizableAreas[2].X = Width - this.margin;
            this.resizableAreas[2].Y = 0;
            this.resizableAreas[2].Width = this.margin;
            this.resizableAreas[2].Height = this.margin;

            // Left
            this.resizableAreas[3].X = 0;
            this.resizableAreas[3].Y = this.margin;
            this.resizableAreas[3].Width = this.margin;
            this.resizableAreas[3].Height = Height - (2 * this.margin);

            // Right
            this.resizableAreas[4].X = Width - this.margin;
            this.resizableAreas[4].Y = this.margin;
            this.resizableAreas[4].Width = this.margin;
            this.resizableAreas[4].Height = Height - (2 * this.margin);

            // Bottom left
            this.resizableAreas[5].X = 0;
            this.resizableAreas[5].Y = Height - this.margin;
            this.resizableAreas[5].Width = this.margin;
            this.resizableAreas[5].Height = this.margin;

            // Bottom
            this.resizableAreas[6].X = this.margin;
            this.resizableAreas[6].Y = Height - this.margin;
            this.resizableAreas[6].Width = Width - (2 * this.margin);
            this.resizableAreas[6].Height = this.margin;

            // Bottom right
            this.resizableAreas[7].X = Width - this.margin;
            this.resizableAreas[7].Y = Height - this.margin;
            this.resizableAreas[7].Width = this.margin;
            this.resizableAreas[7].Height = this.margin;
        }
        */
        #region Event Handlers
        /// <summary>
        /// Don't check mouse status during animation.
        /// </summary>
        /// <param name="sender">Animating control.</param>
        private void OnStartAnimating(UIComponent sender)
        {
            IsAnimating = true;
        }

        /// <summary>
        /// MY OWN!!!
        /// </summary>
        public void Hide()
        {
            ResetMouseOver();
            foreach (var control in Controls)
            {
                control.ResetMouseOver();
            }
            transitioning = Transition.Out;

            
            // moved:
            //Visible = false;
            //CloseWindow();
        }


        public override void Update(GameTime gameTime)
        {
            if (transitioning == Transition.In)
            {
                TransitionValue += (float)(gameTime.ElapsedGameTime.TotalSeconds / timeToTransitionIn);
                if (TransitionValue >= 1)
                {
                    TransitionValue = 1f;
                    transitioning = Transition.None;
                }
                ComputeAlpha();
            }
            else if (transitioning == Transition.Out)
            {
                TransitionValue -= (float)(gameTime.ElapsedGameTime.TotalSeconds / timeToTransitionOut);

                if (TransitionValue <= 0)
                {
                    TransitionValue = 0f;
                    transitioning = Transition.None;

                    HideThisNow = true;
                    
                    //CloseWindow();
                }
                ComputeAlpha();
            }



            base.Update(gameTime);
        }

        
        private void ComputeAlpha()
        {
            alpha = transitionValue * Opacity;            
        }

        protected override void OnMouseOver(UIComponent sender, MouseEventArgs args)
        {
            base.OnMouseOver(sender, args);
 
            if (/*toolTip != null
                &&*/ Visible == true)
            {
                guiManager.MouseIsOverWindow(this);
            }

        }

       

        protected override void OnMove(UIComponent sender)
        {
            base.OnMove(sender);

            // If moving, change transparency
            if (IsAnimating && this.transparency == -1)
            {
                //this.transparency = Transparency;
                //Transparency *= defaultAnimationTransparency;
            }
        }

        /// <summary>
        /// Don't check mouse status during animation.
        /// </summary>
        /// <param name="sender">Animating control.</param>
        private void OnEndAnimating(UIComponent sender)
        {
            IsAnimating = false;
            // Reset transparency
            if (this.transparency != -1)
            {
                //Transparency = this.transparency;
                //this.transparency = -1;
            }
        }

        /// <summary>
        /// Closes window, starting animation if necessary. Also invokes the
        /// window Close event.
        /// </summary>
        /// <param name="sender">Control invoking close.</param>
        protected void OnClose(UIComponent sender, EventArgs e)
        {
            Hide();
            //CloseWindow();
        }

        /// <summary>
        /// Update child controls.
        /// </summary>
        /// <param name="sender">Resizing control.</param>
        protected override void OnResize(UIComponent sender)
        {
            base.OnResize(sender);

            // Width
            this.box.Width = Width;
         //   this.titleBar.Width = Width;
            this.movableArea.Width = Width;
            this.closeButton.X = Width - this.closeButton.Width - this.closeButton.Y;
            // Resize label
            this.label.Width = Width - this.closeButton.Width - this.closeButton.Y - (this.margin * 2);
            this.ViewPort.Width = Width - (this.margin * 2);
            this.backgroundMovableArea.Width = this.ViewPort.Width;

            // Height
            this.box.Height = Height;
            this.ViewPort.Height = Height - this.margin; //Height - (HasTitleBar? TitleBarHeight : 0) - this.margin;
            this.backgroundMovableArea.Height = this.ViewPort.Height;

            RefreshResizableAreas();

            // If resizing, change transparency
            if (IsAnimating && this.transparency == -1)
            {
                //this.transparency = Transparency;
                //Transparency *= defaultAnimationTransparency;
            }
        }

        internal override void Draw(SpriteBatch spriteBatch, Rectangle parentScissor, RenderType typesToRender, float a)
        {
            // Makes sure that Windows draw their children with the window's alpha!
            base.Draw(spriteBatch, parentScissor, typesToRender, alpha);
        }

        protected override void DrawControl(SpriteBatch spriteBatch, Rectangle parentScissor, float a)
        {
            
            base.DrawControl(spriteBatch, parentScissor, a);

            if (DrawContentEvent != null)
            {
                DrawContentEvent(this, spriteBatch);
            }
        }

        /// <summary>
        /// Allows tabbing between child controls.
        /// </summary>
        /// <param name="args">Key event arguments.</param>
        /*protected override void KeyUpIntercept(KeyEventArgs args)
        {
            base.KeyUpIntercept(args);

            if (args.Key == Keys.Tab && IsChild(GUIManager.GetFocus()))
            {
                List<UIComponent> controls = this.ViewPort.Controls;
                bool backwards = args.Shift;
                int index = 0;

                foreach (UIComponent control in controls)
                {
                    if (control.IsChild(GUIManager.GetFocus()))
                    {
                        UIComponent nextControl;
                        int nextIndex = index;

                        while (true)
                        {
                            // Loop round the list if necessary
                            if (backwards)
                            {
                                if (nextIndex > 0)
                                    nextIndex--;
                                else
                                    nextIndex = controls.Count - 1;
                            }
                            else
                            {
                                if ((nextIndex + 1) < controls.Count)
                                    nextIndex++;
                                else
                                    nextIndex = 0;
                            }

                            nextControl = controls[nextIndex];

                            // Set focus to next control
                            if (nextControl.CanHaveFocus && nextControl != this.backgroundMovableArea)
                            {
                                GUIManager.SetFocus(nextControl);
                                break;
                            }
                            else if (nextIndex == index) // Exit loop if coming back to the same control
                                break;
                        }

                        break;
                    }

                    index++;
                }
            }
        }*/
        #endregion
    }
}