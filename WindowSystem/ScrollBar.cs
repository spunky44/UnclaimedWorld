#region File Description
//-----------------------------------------------------------------------------
// File:      ScrollBar.cs
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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using InputEventSystem;
#endregion

namespace WindowSystem
{
    #region Delegates
    /// <summary>
    /// When scrollbar's position changes.
    /// </summary>
    /// <param name="position">New scrollbar position.</param>
    public delegate void ScrollHandler(int position);
    #endregion


    
    /// <summary>
    /// A graphical vertical scrollbar. To use just set the maximum value, and
    /// register for the OnScroll event.
    /// </summary>
    /// <remarks>
    /// Even though only vertical scrollbars are currently implemented, it
    /// shouldn't be difficult to add horizontal functionality at some point in
    /// the future.
    /// </remarks>
    public class ScrollBar : Bar // Icon
    {
        public enum ScrollBarType { Default, CommRoller, MainRoller, HUD, LCD }

        #region Default Properties
        private static int defaultButtonSize = 17;
        private static Rectangle defaultBackgroundSkin = new Rectangle(66, 5, 17, 17);
        private static Rectangle defaultTopButtonSkin = new Rectangle(84, 23, 17, 17);
        private static Rectangle defaultTopButtonHoverSkin = new Rectangle(102, 23, 17, 17);
        private static Rectangle defaultTopButtonPressedSkin = new Rectangle(120, 23, 17, 17);
        private static Rectangle defaultBottomButtonSkin = new Rectangle(84, 5, 17, 17);
        private static Rectangle defaultBottomButtonHoverSkin = new Rectangle(102, 5, 17, 17);
        private static Rectangle defaultBottomButtonPressedSkin = new Rectangle(120, 5, 17, 17);
        private static Rectangle defaultThumbSkin = new Rectangle(66, 23, 17, 10);
        private static Rectangle defaultThumbHoverSkin = new Rectangle(66, 34, 17, 10);
        private static Rectangle defaultThumbPressedSkin = new Rectangle(66, 45, 17, 10);

        /// <summary>
        /// Sets the default button width and height.
        /// </summary>
        /// <value>Must be greater than 0.</value>
        public static int DefaultButtonSize
        {
            set
            {
                Debug.Assert(value > 0);
                defaultButtonSize = value;
            }
        }

        /// <summary>
        /// Sets the background skin.
        /// </summary>
        public static Rectangle DefaultBackgroundSkin
        {
            set { defaultBackgroundSkin = value; }
        }

        /// <summary>
        /// Sets the top button skin.
        /// </summary>
        public static Rectangle DefaultTopButtonSkin
        {
            set { defaultTopButtonSkin = value; }
        }

        /// <summary>
        /// Sets the top button hover skin.
        /// </summary>
        public static Rectangle DefaultTopButtonHoverSkin
        {
            set { defaultTopButtonHoverSkin = value; }
        }

        /// <summary>
        /// Sets the top button pressed skin.
        /// </summary>
        public static Rectangle DefaultTopButtonPressedSkin
        {
            set { defaultTopButtonPressedSkin = value; }
        }

        /// <summary>
        /// Sets the bottom button skin.
        /// </summary>
        public static Rectangle DefaultBottomButtonSkin
        {
            set { defaultBottomButtonSkin = value; }
        }

        /// <summary>
        /// Sets the bottom button hover skin.
        /// </summary>
        public static Rectangle DefaultBottomButtonHoverSkin
        {
            set { defaultBottomButtonHoverSkin = value; }
        }

        /// <summary>
        /// Sets the bottom button pressed skin.
        /// </summary>
        public static Rectangle DefaultBottomButtonPressedSkin
        {
            set { defaultBottomButtonPressedSkin = value; }
        }

        /// <summary>
        /// Sets the thumb skin.
        /// </summary>
        public static Rectangle DefaultThumbSkin
        {
            set { defaultThumbSkin = value; }
        }

        /// <summary>
        /// Sets the thumb hover skin.
        /// </summary>
        public static Rectangle DefaultThumbHoverSkin
        {
            set { defaultThumbHoverSkin = value; }
        }

        /// <summary>
        /// Sets the thumb pressed skin.
        /// </summary>
        public static Rectangle DefaultThumbPressedSkin
        {
            set { defaultThumbPressedSkin = value; }
        }
        #endregion

        #region Fields
        public ScrollBarType ScrollType;

        // Should be the same as the keyboard delay and repeat rates
        private const int RepeatDelay = 500;
        private const int RepeatRate = 50;

        private ImageButton topButton;
        private ImageButton bottomButton;
        private Bar thumb;
        private int value;
        private int viewable;
        private int maximumValue;
        private int scrollStep;
        private int countdown;
        private bool firstRepeat;
        private bool isTopPressed;
        private bool isBottomPressed;
        private bool isTopOver;
        private bool isBottomOver;
        private bool draggingThumb;
        private int dragPoint;
        private Point lastLocation;
        #endregion

        #region Properties
        /// <summary>
        /// Sets the button size, and the width of the scrollbar.
        /// </summary>
        public int ButtonSize
        {
            get { return this.topButton.Width; }
            set
            {
                MinWidth = value;
                MinHeight = 2 * value;
                this.thumb.Y = value;
                Width = value;
            }
        }

        /// <summary>
        /// Get/Set the amount size of the viewport.
        /// </summary>
        /// <value>Must be at leat 0.</value>
        public int Viewable
        {
            get { return this.viewable; }
            set
            {
                Debug.Assert(value >= 0);
                this.viewable = value;
                CalculateThumbSize();

                if (this.viewable > this.maximumValue)
                    ScrollTo(0);
            }
        }


        public override int Height
        {
            get
            {
                return base.Height;
            }
            set
            {
                base.Height = value;
            }
        }

        /// <summary>
        /// Get/Set the amount to scroll each time a button is pressed.
        /// </summary>
        public int ScrollStep
        {
            get { return this.scrollStep; }
            set { this.scrollStep = value; }
        }

        /// <summary>
        /// Get/Set the current scroll value.
        /// 
        /// 0 - Range
        /// 
        /// Range = Ratio * Gap
        /// </summary>
        public int Value
        {
            get { return value; }
            set { ScrollTo(value); }
        }

        /// <summary>
        /// Get/Set the maximum scroll value.
        /// </summary>
        /// <value>Must be at least 0.</value>
        public int MaximumValue
        {
            get { return this.maximumValue; }
            set
            {
                Debug.Assert(value >= 0);
                this.maximumValue = value;
                CalculateThumbSize();

                if (this.viewable > this.maximumValue)
                    ScrollTo(0);
            }
        }

        /// <summary>
        /// Sets the control background skin.
        /// </summary>
        public Rectangle BackgroundSkin
        {
            set { SetSkinLocation(0, value); }
        }

        /// <summary>
        /// Sets the top button skin.
        /// </summary>
        public Rectangle TopButtonSkin
        {
            set { this.topButton.SetSkinLocation(SkinState.Normal,value); }
        }

        /// <summary>
        /// Sets the top button hover skin.
        /// </summary>
        public Rectangle TopButtonHoverSkin
        {
            set { this.topButton.SetSkinLocation(1, value); }
        }

        /// <summary>
        /// Sets the top button pressed skin.
        /// </summary>
        public Rectangle TopButtonPressedSkin
        {
            set { this.topButton.SetSkinLocation(2, value); }
        }

        /// <summary>
        /// Sets the bottom button skin.
        /// </summary>
        public Rectangle BottomButtonSkin
        {
            set { this.bottomButton.SetSkinLocation(SkinState.Normal,value); }
        }

        /// <summary>
        /// Sets the bottom button hover skin.
        /// </summary>
        public Rectangle BottomButtonHoverSkin
        {
            set { this.bottomButton.SetSkinLocation(1, value); }
        }

        /// <summary>
        /// Sets the bottom button pressed skin.
        /// </summary>
        public Rectangle BottomButtonPressedSkin
        {
            set { this.bottomButton.SetSkinLocation(2, value); }
        }

        /// <summary>
        /// Sets the thumb skin.
        /// </summary>
        public Rectangle ThumbSkin
        {
            set { this.thumb.SetSkinLocation(SkinState.Normal,value); }
        }

        /// <summary>
        /// Sets the thumb hover skin.
        /// </summary>
        public Rectangle ThumbHoverSkin
        {
            set { this.thumb.SetSkinLocation(1, value); }
        }

        /// <summary>
        /// Sets the thumb pressed skin.
        /// </summary>
        public Rectangle ThumbPressedSkin
        {
            set { this.thumb.SetSkinLocation(2, value); }
        }

        /// <summary>
        /// Gets the range of scroll values.
        /// range is the part outsde the viewable area.
        /// when everyting is visible, range is 0
        /// 
        /// Range = Ratio * Gap
        /// </summary>
        private float Range
        {
            get { return (float)(this.maximumValue - this.viewable); }
        }

        /// <summary>
        /// Gets the size of the thumb.
        /// </summary>
        private float ShaftHeight
        {
            get 
            {
                if (ScrollType == ScrollBarType.Default
                    || ScrollType == ScrollBarType.HUD
                    || ScrollType == ScrollBarType.LCD)
                {
                    //return (float)(Height - (Width * 2));
                    return (float)(Height - 2 * topButton.Height);
                }
                else return (float)Height;
            }
        }
        #endregion

        #region Events
        public event ScrollHandler Scroll;
        #endregion

        #region Constructors

        public ScrollBar(GUIManager guiManager) : this(guiManager, ScrollBarType.Default) { }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game">The currently running Game object.</param>
        /// <param name="guiManager">GUIManager that this control is part of.</param>
        public ScrollBar(GUIManager guiManager, ScrollBarType type)
            : base(guiManager)
        {
            this.ScrollType = type;

            this.value = 0;
            this.viewable = 1;
            this.maximumValue = 1;
            this.scrollStep = 1;
            this.countdown = 0;
            this.firstRepeat = true;
            this.isTopPressed = false;
            this.isBottomPressed = false;
            this.isTopOver = false;
            this.isBottomOver = false;
            this.draggingThumb = false;
            this.lastLocation = Point.Zero;

           

            #region Set Default Properties
            switch (type)
            {
                case ScrollBarType.CommRoller:
                    this.thumb = new Bar(guiManager);

                    Add(this.thumb);

                    //this.thumb.DebugTag = "ThumbDebug";
            
                    MinHeight = 1; // 2 * defaultButtonSize;
                    //   Scale = true; OLD - Icon!
                    

                    Rectangle rect = guiManager.GUISpriteSheet.GetSourceRectangle("event_scroller_groove");
                    BackgroundSkin = rect;
                    MinWidth = rect.Width;
                    Width = rect.Width; // groove
                    EdgeSize = 10; // for the groove bar ends

                    
                    
                    // the long sprite with the ribs - not scaled:
                    thumb.UnderSprite = guiManager.GUISpriteSheet.GetSourceRectangle("event_scroller_knob_base");
                    thumb.UnderSpriteY = 7;
                    // the short sprite with endpoints and transparent middle with shading (bar):
                    rect = guiManager.GUISpriteSheet.GetSourceRectangle("event_scroller_knobwithshading");
                   // Width = rect.Width;
                    ThumbSkin = rect;
                    ThumbHoverSkin = rect;
                    ThumbPressedSkin = rect;
                    thumb.EdgeSize = 12;
                    thumb.Width = rect.Width;
                    
                    // the long sprite with the ribs - not scaled:
                /*    Image thumbImage = new Image(guiManager);
                    rect = guiManager.GUISpriteSheet.SourceRectangle("event_scroller_knob_base");

                    thumbImage.SetSkinLocation(SkinState.Normal,rect);
                    thumbImage.Position = new Vector2(0f, 12f);
                    thumbImage.ResizeToFit(); // don't scale this!!
                    thumbImage.CanHaveFocus = false; // important! don't take focus and intercept mouse press from the bar.
                    thumb.Add(thumbImage);*/

                    RenderType = RenderType.CRTAndLCD;

                    break;
                case ScrollBarType.MainRoller:
                    this.thumb = new Bar(guiManager);

                    Add(this.thumb);

                    MinHeight = 1; // 2 * defaultButtonSize;
                    //   Scale = true; OLD - Icon!                    

                    rect = guiManager.GUISpriteSheet.GetSourceRectangle("basic_scroller_groove");
                    BackgroundSkin = rect;
                    MinWidth = rect.Width;
                    Width = rect.Width; // groove
                    EdgeSize = 11; // for the groove bar ends - includes shadow!


                    // the long sprite with the ribs - not scaled:
                    thumb.UnderSprite = guiManager.GUISpriteSheet.GetSourceRectangle("basic_scroller_knob_base");
                    thumb.UnderSpriteY = 5;
                    // the short sprite with endpoints and transparent middle with shading (bar):
                    rect = guiManager.GUISpriteSheet.GetSourceRectangle("basic_scroller_knobwithshading");
                    // Width = rect.Width;
                    ThumbSkin = rect;
                    ThumbHoverSkin = rect;
                    ThumbPressedSkin = rect;
                    thumb.EdgeSize = 13; // 5; // 13;
                    thumb.Width = rect.Width;

                    thumb.DebugTag = "scrollerKnob";

                    RenderType = RenderType.Normal;

                    break;
                case ScrollBarType.Default:
                    this.topButton = new ImageButton(guiManager);
                    this.bottomButton = new ImageButton(guiManager);
                    this.thumb = new Bar(guiManager);

                    // Thumb added first so buttons drawn on top when really small
                    Add(this.thumb);
                    Add(this.topButton);
                    Add(this.bottomButton);

                    #region Set Properties
                    MinWidth = defaultButtonSize;
                    MinHeight = 2 * defaultButtonSize;
                    //   Scale = true; OLD - Icon!
                    IsVertical = true;
                    this.thumb.CanHaveFocus = true;
                    this.thumb.IsVertical = true;
                    this.thumb.Y = defaultButtonSize;
                    #endregion

                    Width = defaultButtonSize;

                    rect = defaultTopButtonSkin; //guiManager.GUISpriteSheet.SourceRectangle("blue");
                    BackgroundSkin = defaultBackgroundSkin;
                    TopButtonSkin = rect; //defaultTopButtonSkin;
                    TopButtonHoverSkin = rect; //defaultTopButtonHoverSkin;
                    TopButtonPressedSkin = rect; //defaultTopButtonPressedSkin;
                    BottomButtonSkin = rect; //defaultBottomButtonSkin;
                    BottomButtonHoverSkin = rect; //defaultBottomButtonHoverSkin;
                    BottomButtonPressedSkin = rect; //defaultBottomButtonPressedSkin;
                    ThumbSkin = rect; // defaultThumbSkin;
                    ThumbHoverSkin = rect; // defaultThumbHoverSkin;
                    ThumbPressedSkin = rect; // defaultThumbPressedSkin;

                    break;
                case ScrollBarType.HUD:
                    int widthToUse = 12;

                    this.topButton = new ImageButton(guiManager);
                    this.bottomButton = new ImageButton(guiManager);
                    this.thumb = new Bar(guiManager);

                    // Thumb added first so buttons drawn on top when really small
                    Add(this.thumb);
                    Add(this.topButton);
                    Add(this.bottomButton);

                    MinWidth = widthToUse;
                    MinHeight = 2 * widthToUse;
                    //   Scale = true; OLD - Icon!
                    IsVertical = true;
                    this.thumb.CanHaveFocus = true;
                    this.thumb.IsVertical = true;
                    this.thumb.Y = widthToUse;

                    CanHaveFocus = true; // to catch clicking in the trough

                    Width = widthToUse;

                    EdgeSize = 4; // for the trough
                                     
                    BackgroundSkin = guiManager.GUISpriteSheet.GetSourceRectangle("HUD_scrollbar_base");
                   // TopButtonSkin = rect; //defaultTopButtonSkin;
                    topButton.InitButton("HUD_uparrow", ImageButton.hudHoverTint, ImageButton.hudPressedTint);
                  //  topButton.SetSkinLocation(SkinState.Normal,value);
                  //  TopButtonHoverSkin = rect; //defaultTopButtonHoverSkin;
                  //  TopButtonPressedSkin = rect; //defaultTopButtonPressedSkin;
                    bottomButton.InitButton("HUD_downarrow", ImageButton.hudHoverTint, ImageButton.hudPressedTint);
                
                //    BottomButtonSkin = rect; //defaultBottomButtonSkin;
                 //   BottomButtonHoverSkin = rect; //defaultBottomButtonHoverSkin;
                 //   BottomButtonPressedSkin = rect; //defaultBottomButtonPressedSkin;

                    thumb.SetSkinLocation(SkinState.Normal, guiManager.GUISpriteSheet.GetSourceRectangle("HUD_scrollbar_knob"));
                    thumb.SetSkinLocation(SkinState.Hover, guiManager.GUISpriteSheet.GetSourceRectangle("HUD_scrollbar_knob"), ImageButton.hudHoverTint, ImageButton.hudHoverTint);
                    thumb.SetSkinLocation(SkinState.Pressed, guiManager.GUISpriteSheet.GetSourceRectangle("HUD_scrollbar_knob"), ImageButton.hudPressedTint, ImageButton.hudPressedTint);

                 //   ThumbSkin = rect; // defaultThumbSkin;
                 //   ThumbHoverSkin = rect; // defaultThumbHoverSkin;
                 //   ThumbPressedSkin = rect; // defaultThumbPressedSkin;

                    break;

                case ScrollBarType.LCD:
                    widthToUse = 15;

                    this.topButton = new ImageButton(guiManager);
                    this.bottomButton = new ImageButton(guiManager);
                    this.thumb = new Bar(guiManager);

                    // Thumb added first so buttons drawn on top when really small
                    Add(this.thumb);
                    Add(this.topButton);
                    Add(this.bottomButton);

                    MinWidth = widthToUse;
                    MinHeight = 2 * widthToUse;
                    //   Scale = true; OLD - Icon!
                    IsVertical = true;
                    this.thumb.CanHaveFocus = true;
                    this.thumb.IsVertical = true;
                    this.thumb.Y = widthToUse;

                    CanHaveFocus = true; // to catch clicking in the trough

                    Width = widthToUse;

                    EdgeSize = 10; // 4; // for the trough

                    BackgroundSkin = guiManager.GUISpriteSheet.GetSourceRectangle("basic_scrollbar_groove");

                    topButton.InitButton("basic_scrollbar_arrow_up", ImageButton.lcdHoverTint, ImageButton.lcdPressedTint);               
                    bottomButton.InitButton("basic_scrollbar_arrow_down", ImageButton.lcdHoverTint, ImageButton.lcdPressedTint); 

                  //  thumb.DebugTag = "lcdthumb";

                    thumb.SetSkinLocation(SkinState.Normal, guiManager.GUISpriteSheet.GetSourceRectangle("basic_scrollbar_knob"));
                    thumb.SetSkinLocation(SkinState.Hover, guiManager.GUISpriteSheet.GetSourceRectangle("basic_scrollbar_knob"), ImageButton.lcdHoverTint, ImageButton.lcdHoverTint);
                    thumb.SetSkinLocation(SkinState.Pressed, guiManager.GUISpriteSheet.GetSourceRectangle("basic_scrollbar_knob"), ImageButton.lcdPressedTint, ImageButton.lcdPressedTint);
                    thumb.EdgeSize = 4;

                    RenderType = WindowSystem.RenderType.CRTAndLCD;

                    break;
            }

            IsVertical = true;
            this.thumb.CanHaveFocus = true;
            this.thumb.IsVertical = true;
            this.thumb.Y = defaultButtonSize;
            this.thumb.ZOrder = 1f; // always on top

            #endregion


            
            


            #region Event Handlers
            if (topButton != null)
            {
                this.topButton.MouseDown += new MouseDownHandler(OnTopButtonDown);
                this.topButton.MouseUp += new MouseUpHandler(OnButtonUp);
                this.topButton.MouseOut += new MouseOutHandler(OnTopButtonOut);
                this.topButton.MouseOver += new MouseOverHandler(OnTopButtonOver);
            }
            if (bottomButton != null)
            {
                this.bottomButton.MouseDown += new MouseDownHandler(OnBottomButtonDown);
                this.bottomButton.MouseUp += new MouseUpHandler(OnButtonUp);
                this.bottomButton.MouseOver += new MouseOverHandler(OnBottomButtonOver);
                this.bottomButton.MouseOut += new MouseOutHandler(OnBottomButtonOut);
            }

            this.thumb.MouseOver += new MouseOverHandler(OnThumbMouseOver);
            this.thumb.MouseOut += new MouseOutHandler(OnThumbMouseOut);
            this.thumb.MouseDown += new MouseDownHandler(OnThumbDown);
            this.thumb.MouseUp += new MouseUpHandler(OnThumbUp);
            this.thumb.MouseMove += new MouseMoveHandler(OnThumbMove);
            #endregion
        }
        #endregion

        /// <summary>
        /// Handles the timing, and triggers scrolling when buttons are
        /// pressed.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // Trigger scroll if countdown is reached
            if (this.countdown <= 0)
            {

                if (this.isTopPressed && this.isTopOver)
                {
                    dragPoint = thumb.Height / 2;
                   // dragPoint = 0; // ??
                    ScrollTo(this.value - this.scrollStep);
                }
                else if (this.isBottomPressed && this.isBottomOver)
                {
                   // dragPoint = thumb.Height; // ??
                    dragPoint = thumb.Height / 2;
                    ScrollTo(this.value + this.scrollStep);
                }

                if (this.firstRepeat)
                {
                    this.countdown += RepeatDelay;
                    this.firstRepeat = false;
                }
                else
                    this.countdown = RepeatRate;
            }

            // Update timer
            if ((this.isTopPressed && this.isTopOver) ||
                (this.isBottomPressed && this.isBottomOver)
                )
                this.countdown -= gameTime.ElapsedGameTime.Milliseconds; //// XNA 3
            else
                this.countdown = 0;

            base.Update(gameTime);
        }


        public bool IsAtEnd()
        {
            return value == Range;
        }

        public bool IsAtTop()
        {
            return value == 0;
        }

        public override RenderType RenderType
        {
            get
            { // never CRT...
                //return RenderType.Normal;
                return base.RenderType;
            }
            set
            {
                base.RenderType = value;
            }
        }

        public bool ShowKnob
        {
            set 
            {
                if (value)
                {                    
                    Add(thumb);
                }
                else
                {
                    Remove(thumb);
                }
            }
        }

        /// <summary>
        /// Calculates the scroll thumb height, based on how much of the total
        /// scrollbar size can be fit in parent control.
        /// </summary>
        private void CalculateThumbSize()
        {
            int heightBefore = this.thumb.Height;

            try
            {
                // Calculate height
                float thumbHeight = ShaftHeight / ((float)this.maximumValue / this.viewable);
                this.thumb.Height = Convert.ToInt32(thumbHeight);
            }
            catch
            {
                this.thumb.Height = (int)ShaftHeight;
            }

            // Cap sizes to allowed values
            if (this.thumb.Height < 10)
                this.thumb.Height = 10;
            else if (this.thumb.Height > ShaftHeight)
                this.thumb.Height = (int)ShaftHeight;

            if (this.thumb.Height != heightBefore)
                Redraw();
        }

        /// <summary>
        /// Scrolls to the specified position, and invokes the Scroll event.
        /// 
        /// position min: 0, max: Range
        /// </summary>
        /// <param name="position">Position to scroll to.</param>
        private void ScrollTo(int position)
        {
            // Cap position to allowed values
            if (position > Range)
                position = (int)Range;

            if (position < 0)
                position = 0;

            this.value = position;

         
            // Calculate pixel position
            if (ScrollType == ScrollBarType.Default
                || ScrollType == ScrollBarType.HUD
                || ScrollType == ScrollBarType.LCD)
            {
                // leave room at top and bottom for buttons:

                float height = Gap; // ShaftHeight - this.thumb.Height; // Height - (topButton.Height * 2) - this.thumb.Height;
               // float height = ShaftHeight;

                if (Range > 0f)
                {
                    float percentage = (float)value / Range; 

                    float thumbY = topButton.Height + height * percentage; // if value = Range, thumbY should be at Gap

                    // adjust the thumb position so the drag point stays at the mouse position
                    // the drag point is the center when clicking in the trough.                  
                   // thumbY -= dragPoint;

                    if (thumbY < topButton.Height)
                    {
                        thumbY = topButton.Height;
                    }
                    else
                    {
                        float maxThumbY = Gap + topButton.Height;
                        if (thumbY > maxThumbY)
                        {
                            thumbY = maxThumbY;
                        }
                    }

                    this.thumb.Y = Convert.ToInt32(thumbY);

                }
                else
                {
                    this.thumb.Y = this.topButton.Height;
                }
            }
           

            if (Scroll != null)
                Scroll.Invoke(value);

            Redraw();
        }

        private float Gap
        {
            get
            {
                return ShaftHeight - (float)this.thumb.Height; 
            }
        }

        /// <summary>
        /// Range / Gap
        /// </summary>
        private float Ratio
        {
            get
            {
                return Range / Gap; // Range / ShaftHeight;
            }
        }

        /// <summary>
        /// Converts mouse y-coordinates to a scroll position.    
        /// </summary>
        /// <param name="mousePosition">Mouse y-position.</param>
        /// <returns>Scroll position.</returns>
        private int MouseToScrollPosition(int mousePosition) //, float? dragPoint = null)
        {
            // modify the result with the current drag point on the thumb. the default drag point is half the thumb height (used when clicking outside the thumb)
            // this only affects the result when near the endpoints.
            // In ScrollTo, the thumb is placed according to the drag point.

            float y = (float)mousePosition;

            
            // the upper-gap / trough ratio is the same as the value/range ratio!
            // we can determine the value from this:
            // value / Range = gap / ShaftHeight
            // value = Range * gap / ShaftHeight

            // gap is related to thumb length:
            // gap = ShaftHeight - thumb.Length

            float position;
            if (Gap > 0f)
            {
               // float ratio = Range / gap;
              /*  if (y > ShaftHeight - dragPoint)
                {  
                    // mouse in half of the thumb height distance from the bottom should give position = Range (max)
                    position = Range;
                }
                else if (y < dragPoint)
                {
                    // above upper half
                    position = 0;
                }
                else
                {*/
                   // float ratio = Range / ShaftHeight;

                    position = Ratio * y;
              //  }

                return Convert.ToInt32(position);               
            }
            else
            {
                return 0;
            }
        }

        #region Event Handlers
        /// <summary>
        /// Update thumb skin.
        /// </summary>
        /// <param name="args">Mouse event arguments.</param>
        protected void OnThumbMouseOver(UIComponent sender, MouseEventArgs args)
        {
            if (!this.draggingThumb)
                this.thumb.CurrentSkinState = SkinState.Hover;
        }

        /// <summary>
        /// Update thumb skin.
        /// </summary>
        /// <param name="args">Mouse event arguments.</param>
        protected void OnThumbMouseOut(UIComponent sender, MouseEventArgs args)
        {
            if (!this.draggingThumb)
                this.thumb.CurrentSkinState = SkinState.Normal;
        }

        /// <summary>
        /// Starts dragging the thumb.
        /// </summary>
        /// <param name="args">Mouse event arguments.</param>
        protected void OnThumbDown(MouseEventArgs args)
        {
            if (args.Button == MouseButtons.Left)
            {
                this.draggingThumb = true;
                this.lastLocation = args.Position;
                this.dragPoint = args.Position.Y - thumb.AbsolutePosition.Y;
                this.thumb.CurrentSkinState = SkinState.Pressed;
            }
        }

        /// <summary>
        /// Finishes dragging the thumb.
        /// </summary>
        /// <param name="args">Mouse event arguments.</param>
        protected void OnThumbUp(MouseEventArgs args)
        {
            if (args.Button == MouseButtons.Left)
            {
                this.draggingThumb = false;

                // Check if hover state should be shown
                if (this.thumb.CheckCoordinates(args.Position.X, args.Position.Y))
                    this.thumb.CurrentSkinState = SkinState.Hover;
                else
                    this.thumb.CurrentSkinState = SkinState.Normal;

                // Get focus back from thumb
                GUIManager.SetFocus(this);
            }
        }

        /// <summary>
        /// Updates scroll position when the thumb is dragged.
        /// </summary>
        /// <param name="args">Mouse event arguments.</param>
        protected void OnThumbMove(MouseEventArgs args)
        {
            if (this.draggingThumb)
            {
                int yPosition;
               
               /* int beforeY = this.thumb.Y;
                yPosition = this.thumb.Y + dragPoint - topButton.Height;
                int yDiff = args.Position.Y - this.lastLocation.Y;

                // Get the position from before move
                //int beforeY = yPosition;

                yPosition += yDiff;
                */
                                
                yPosition = args.Position.Y - AbsolutePosition.Y - topButton.Height; // Convert mouse location to trough location

                yPosition -= dragPoint;

                // scale the yposition, so when the thumb touches the endpoint the value will be at 0 or Range
          /*      int topOfYPos = dragPoint;
                int bottomOfYPos = (int)(ShaftHeight - (thumb.Height - dragPoint));

                // scale the middle:
                float middleRange = bottomOfYPos - topOfYPos;
                if (yPosition < topOfYPos)
                {
                    yPosition = 0;
                }
                else if (yPosition > bottomOfYPos)
                {
                    yPosition = (int)ShaftHeight;
                }
                else if (yPosition > topOfYPos && yPosition < bottomOfYPos)
                {
                    yPosition = (int)MathHelper.Lerp(0, ShaftHeight, (yPosition - topOfYPos) / middleRange);                   
                }
                     */         

                if (ScrollType == ScrollBarType.Default
                    || ScrollType == ScrollBarType.HUD
                    || ScrollType == ScrollBarType.LCD)
                {                  
                    ScrollTo(MouseToScrollPosition(yPosition));
                }              

                // set the new location after clamping:
               // this.lastLocation.Y += this.thumb.Y - beforeY;
            }
        }

        /// <summary>
        /// When the mouse is pressed inside the thumb trough, the thumb moves
        /// to that position. If above the thumb, the thumb top moves to that
        /// position, and the thumb bottom when clicking below.
        /// </summary>
        /// <param name="args">Mouse event arguments.</param>
        protected override void OnMouseDown(MouseEventArgs args)
        {
            base.OnMouseDown(args);

            if (args.Button == MouseButtons.Left)
            {
                dragPoint = thumb.Height / 2; // center the thumb under the mouse cursor

                // Convert mouse location to scrollbar location
                int yPosition = args.Position.Y - AbsolutePosition.Y - topButton.Height;

                yPosition -= dragPoint;

                if (ScrollType == ScrollBarType.Default
                    || ScrollType == ScrollBarType.HUD
                    || ScrollType == ScrollBarType.LCD)
                {
                   /* // clamp or scale:
                    int topLimit = dragPoint; // top of the trough, but lowest y position and value
                    int bottomLimit = (int)(ShaftHeight - dragPoint); // bottom of the through, but highest y position and value
                    if (yPosition > bottomLimit)
                    {
                        // clamp to bottom.
                        // mouse in half of the thumb height distance from the bottom should give position = Range (max)
                        yPosition = Convert.ToInt32(ShaftHeight);
                    }
                    else if (yPosition < topLimit)
                    {
                        // clamp to top
                        // above upper half 
                        yPosition = 0;
                    }
                    else
                    {
                        // scale, there will be a gap
                        // scale the click pos between the two endpoints:
                        int middleRange = bottomLimit - topLimit;
                       // yPosition = (int)MathHelper.Lerp(0, ShaftHeight, (float)(yPosition - topLimit) / (float)middleRange);
                        yPosition = (int)MathHelper.Lerp(topLimit, bottomLimit, (float)(yPosition - topLimit) / (float)middleRange);

                    }*/

                    ScrollTo(MouseToScrollPosition(yPosition));

                }               
            }
        }

        /// <summary>
        /// Top button is pressed.
        /// </summary>
        /// <param name="args">Mouse event arguments.</param>
        protected void OnTopButtonDown(MouseEventArgs args)
        {
            if (args.Button == MouseButtons.Left)
            {
                this.isTopPressed = true;
                this.isTopOver = true;
                this.firstRepeat = true;
            }
        }

        /// <summary>
        /// Bottom button is pressed.
        /// </summary>
        /// <param name="args">Mouse event arguments.</param>
        protected void OnBottomButtonDown(MouseEventArgs args)
        {
            if (args.Button == MouseButtons.Left)
            {
                this.isBottomPressed = true;
                this.isBottomOver = true;
                this.firstRepeat = true;
            }
        }

        /// <summary>
        /// Either button is released.
        /// </summary>
        /// <param name="args">Mouse event arguments.</param>
        protected void OnButtonUp(MouseEventArgs args)
        {
            if (args.Button == MouseButtons.Left)
            {
                this.isTopPressed = false;
                this.isBottomPressed = false;
                this.isTopOver = false;
                this.isBottomOver = false;

                // Get focus back
                GUIManager.SetFocus(this);
            }
        }

        /// <summary>
        /// Mouse hovering over top button. Only scrolls when mouse is held
        /// over button.
        /// </summary>
        /// <param name="args">Mouse event arguments.</param>
        protected void OnTopButtonOver(UIComponent sender, MouseEventArgs args)
        {
            this.isTopOver = true;
        }

        /// <summary>
        /// Mouse hovering over bottom button. Only scrolls when mouse is held
        /// over button.
        /// </summary>
        /// <param name="args">Mouse event arguments.</param>
        protected void OnBottomButtonOver(UIComponent sender, MouseEventArgs args)
        {
            this.isBottomOver = true;
        }

        /// <summary>
        /// Mouse outside top button area. Only scrolls when mouse is held over
        /// button.
        /// </summary>
        /// <param name="args">Mouse event arguments.</param>
        protected void OnTopButtonOut(UIComponent sender, MouseEventArgs args)
        {
            this.isTopOver = false;
        }

        /// <summary>
        /// Mouse outside bottom button area. Only scrolls when mouse is held
        /// over button.
        /// </summary>
        /// <param name="args">Mouse event arguments.</param>
        protected void OnBottomButtonOut(UIComponent sender, MouseEventArgs args)
        {
            this.isBottomOver = false;
        }

        /// <summary>
        /// Update child controls.
        /// </summary>
        /// <param name="sender">Resized control.</param>
        protected override void OnResize(UIComponent sender)
        {
            base.OnResize(sender);

            // Widths - make buttons square!
            if (topButton != null)
            {
                this.topButton.Width = Width;
                this.topButton.Height = Width;
            }

            if (bottomButton != null)
            {
                this.bottomButton.Width = Width;
                this.bottomButton.Height = Width;

                this.bottomButton.Y = Height - this.bottomButton.Height;
            }

            if (ScrollType == ScrollBarType.Default
                || ScrollType == ScrollBarType.HUD
                || ScrollType == ScrollBarType.LCD)
            {   // don't set thumb width for the comm panel. It is wider than the groove.
                this.thumb.Width = Width;
            }

            // Thumb size needs to be updated
            CalculateThumbSize();

            // Keep position up to date
            ScrollTo(this.value);
        }
        #endregion
    }
}