#region File Description
//-----------------------------------------------------------------------------
// File:      TextButton.cs
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
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using InputEventSystem;
using Microsoft.Xna.Framework.Audio;
#endregion

namespace WindowSystem
{

    /// <summary>
    /// A push button comprised of a graphical Box component, with a text label
    /// on top.
    /// </summary>
    public class TextButton : UIComponent, ICanBeChecked
    {
        public enum TextButtonType
        {
            LCD, LCDSliderButton, LCDSliderButtonWhite, LCDToolTipBlack, HUDSliderButton, HUDToolTipWhite, LCDCombo, LCDAmount, White, Black, BlackSlim, /*SidePanelTabButton,*/
            Hyperlink, HyperlinkBlack, HUD, HUDHasState, HUDGather, HUDStockpile,
            HUDItemQuantity, HUDScout, HUDHunt, HUDPatrol, HUDForage, LCDCollapsableHeaderBig, LCDCollapsableHeaderBigGreen, LCDCollapsableHeaderBigBlue, LCDCollapsableHeaderBigRed,
            LCDCollapsableHeaderSmall,
            LCDSortingArrows,
            HUDDiscard,
            HUDClaim,
            HUDSalvage,
            HUDPackingDown,
            HUDSliderButtonWhite,
            HUDAttack,
            HUDUpgrade
        }

        #region Default Properties
        private static int defaultWidth = 65;
        private static int defaultHeight = 12; // 25;
        private static int defaultEdgeSize = 12;
      
        private static Rectangle defaultSkin = new Rectangle(1, 142, 25, 25);
        private static Rectangle defaultHoverSkin = new Rectangle(27, 142, 25, 25);
        private static Rectangle defaultPressedSkin = new Rectangle(52, 142, 25, 25);
        private static Rectangle defaultHoverSkinNonEnabled = new Rectangle(1, 142, 25, 25);
      

        const int hudButtonHeight = 25;

        /// <summary>
        /// Sets the default button width.
        /// </summary>
        /// <value>Must be greater than 0.</value>
        public static int DefaultWidth
        {
            set
            {
                Debug.Assert(value > 0);
                defaultWidth = value;
            }
        }

        /// <summary>
        /// Sets the default button height.
        /// </summary>
        /// <value>Must be greater than 0.</value>
        public static int DefaultHeight
        {
            set
            {
                Debug.Assert(value > 0);
                defaultHeight = value;
            }
        }

        /// <summary>
        /// Sets the default size of left and right sides of button.
        /// </summary>
        /// <value>Must be greater than 0.</value>
        public static int DefaultEdgeSize
        {
            set
            {
                Debug.Assert(value > 0);
                defaultEdgeSize = value;
            }
        }

      
        /// <summary>
        /// Sets the default control skin.
        /// </summary>
        public static Rectangle DefaultSkin
        {
            set { defaultSkin = value; }
        }

        /// <summary>
        /// Sets the default control hover skin.
        /// </summary>
        public static Rectangle DefaultHoverSkin
        {
            set { defaultHoverSkin = value; }
        }

        /// <summary>
        /// Sets the default control pressed skin.
        /// </summary>
        public static Rectangle DefaultPressedSkin
        {
            set { defaultPressedSkin = value; }
        }
        #endregion

        #region Fields
        //  protected Bar buttonBar;
        protected Box buttonBox;
        protected Label label;

        protected int yLabelOffset = 0;


        public TextButtonType Type
        {
            get;
            protected set;
        }

        #endregion


       


        private SoundEffect clickedSound;

        private Icon icon;

        public bool HasState// = false;
        {
            get
            {
                return checkedMode != CheckedModes.CannotBeChecked;
            }
        }

        protected bool isChecked = false;

      /*  private bool switchStateOnClick = true;

        /// <summary>
        /// if the button has state, this setting determines if the button can go from checked to not checked by clicking on it.
        /// for radiobuttons, we typically don't want this.
        /// </summary>
        public bool SwitchStateOnClick
        {
            get
            {
                return switchStateOnClick;
            }
            set
            {
                switchStateOnClick = value;
            }
        }*/

        CheckedModes checkedMode = CheckedModes.CannotBeChecked;
        public CheckedModes CheckedMode
        {
            get
            {
                return checkedMode;
            }
            set
            {
                checkedMode = value;
            }
        }


        // private int xLabelOffset = 0;

        /// <summary>
        /// include shadows in this value.
        /// </summary>
        private int rightButtonPadding = 2;
        private int leftButtonPadding = 2;

        public enum TextAlign { Center, Left };

        private TextAlign textAlignment;
        public TextAlign TextAlignment //= TextAlign.Center;
        {
            get
            {
                return textAlignment;
            }
            set
            {
                if (textAlignment != value)
                {
                    textAlignment = value;
                    RefreshLabelPosition();
                }
            }
        }



        #region Properties
        /// <summary>
        /// Sets the control skin.
        /// </summary>
        public Rectangle Skin
        {
            set { this.buttonBox.SetSkinLocation(SkinState.Normal /*0*/, value); }
        }

        /// <summary>
        /// Sets the control hover skin.
        /// </summary>
        public Rectangle HoverSkin
        {
            set { this.buttonBox.SetSkinLocation(1, value); } // bug-prone code
        }
        public Rectangle HoverSkinNonEnabled
        {
            set { this.buttonBox.SetSkinLocation(1, value); } // bug-prone code
        }
        /// <summary>
        /// Sets the control pressed skin.
        /// </summary>
        public Rectangle PressedSkin
        {
            set { this.buttonBox.SetSkinLocation(SkinState.Pressed, value); }
        }

        public Rectangle DisabledSkin
        {
            set { this.buttonBox.SetSkinLocation(SkinState.Disabled, value, lcdDisabledColor, lcdDisabledColor); }
        }

        /// <summary>
        /// Get/Set size of left and right parts of the button.
        /// </summary>
        /// <value>Must be at least 0.</value>
        public int CornerSize
        {
            get { return this.buttonBox.CornerSize; } // this.buttonBar.EdgeSize; }
            set { this.buttonBox.CornerSize = value; } //EdgeSize = value; }
        }

        /// <summary>
        /// Get/Set the button text.
        /// </summary>
        /// <value>Must not be null.</value>
        public string Text
        {
            get { return this.label.Text; }
            set
            {
                this.label.Text = value;

                if (Type == TextButtonType.Hyperlink)
                {
                    this.Width = this.label.TextWidth;
                    this.Height = this.label.TextHeight; // this calls OnResize, which calls RefreshLabelPosition, which sets label size.
                }
                else
                {
                    RefreshLabelPosition();
                }
            }
        }

        /// <summary>
        /// The font used to draw button text.
        /// </summary>
        /// <value>Must not be a valid path.</value>
        public SpriteFont Font
        {
            set
            {
                label.Font = value;
                RefreshLabelPosition();
            }
        }


        public Color LabelColor
        {
            get
            {
                return label.NormalColor; // ?? label.Color;
            }
            set
            {
                label.NormalColor = value;
            }
        }


        public Color NormalColor
        {           
            set
            {
                buttonBox.SetSkinLocation(SkinState.Normal, null, value, value); // better to use Modulate, so final hover color is affected too
            }
        }

        #endregion

        #region Constructors

        /*   public TextButton(Game game, GUIManager guiManager)//: this(ImageButtonType.Default, game, guiManager)
        {            
        }*/

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game">The currently running Game object.</param>
        /// <param name="guiManager">GUIManager that this control is part of.</param>
        public TextButton(GUIManager guiManager)
            : base(guiManager)
        {
            #region Create Child Controls
            // this.buttonBar = new Bar(guiManager);
            this.buttonBox = new Box(guiManager);
            this.label = new Label(guiManager);
            #endregion

            #region Add Child Controls
            Add(this.buttonBox);
            Add(this.label);
            #endregion

            #region Set Properties
            //  this.buttonBar.IsVertical = false;
            MinWidth = defaultHeight;
            MinHeight = defaultHeight;

            CanHaveFocus = true;
            #endregion

            /*    switch (type)
            {
                case ImageButtonType.Comm:
                    Rectangle rect = guiManager.GUISpriteSheet.SourceRectangle("event_button_out");
                    Width = rect.Width;
                    Height = rect.Height;                    
                    EdgeSize = 4;
                    Skin = rect;
                    HoverSkin = rect;
                    PressedSkin = rect;
                    Font = GUIManager.RegularInterfaceFontPath; // no antialiasing: GUIManager.ButtonFaceFontPath; // GUIManager.RegularInterfaceFontPath;
                    Color = Color.White;
                    break;
                case ImageButtonType.Default:
                    #region Set Default Properties
                    Width = defaultWidth;
                    Height = defaultHeight;
                    EdgeSize = defaultEdgeSize;
                    Skin = defaultSkin;
                    HoverSkin = defaultHoverSkin;
                    PressedSkin = defaultPressedSkin;
                    #endregion
                    break;
            }*/

        }
        #endregion


        /// <summary>
        /// if the label should appear in a different color when disabled, call this.
        /// </summary>
        /// <param name="disabledColor"></param>
        /// <param name="enabledColor"></param>
        /*  private void InitDisabledColor(Color disabledColor, Color enabledColor)
          {
              this.DisabledColor = disabledColor;
              this.enabledColor = enabledColor;
          }*/


        public override bool Enabled
        {
            get
            {
                return base.Enabled;
            }
            set
            {
                if (base.Enabled != value)
                {
                    base.Enabled = value;

                    label.Enabled = value;

                    if (base.Enabled)
                    {
                        /*   if (enabledColor.HasValue)
                           {
                               Color = enabledColor.Value;
                           }*/

                        buttonBox.CurrentSkinState = SkinState.Normal;
                    }
                    else
                    {
                        /* if (DisabledColor.HasValue)
                         {
                             Color = DisabledColor.Value;
                         }*/

                        buttonBox.CurrentSkinState = SkinState.Disabled;

                    }
                }
            }
        }

        public void Init(TextButtonType type)
        {
            Init(type, -1);
        }

        public void Init(TextButtonType type, int flavour)
        {
            this.Type = type;
            Rectangle rect;
            switch (type)
            {
               
                case TextButtonType.White:
                    InitMainButton("basic_buttonwhite_out", "basic_buttonwhite_hover", "basic_buttonwhite_in", Color.Black, false);
                    break;

                case TextButtonType.Black:
                    InitMainButton("smallblackbutton_out", "smallblackbutton_hover", "smallblackbutton_in", Color.WhiteSmoke, false);
                    break;

                case TextButtonType.BlackSlim:
                    InitMainButton("smallblackbutton_out", "smallblackbutton_hover", "smallblackbutton_in", Color.WhiteSmoke, true);
                    break;

                case TextButtonType.Hyperlink:
                    throw new Exception("Not Used!");
                    
                    break;

                case TextButtonType.LCD:
                    label.Init(Label.LabelType.LCDNormal);
                    leftButtonPadding = 6;
                    rightButtonPadding = 8;

                    rect = GUIManager.GUISpriteSheet.GetSourceRectangle("basic_button_light");

                    Width = rect.Width;
                    Height = rect.Height;

                    CornerSize = 11;
                    
                    Skin = rect;
                    //  HoverSkin = rect;
                    buttonBox.SetSkinLocation(SkinState.Hover, rect, lcdTooltipHoverTint, lcdTooltipHoverTint);
                                                         
                    rect = GUIManager.GUISpriteSheet.GetSourceRectangle("basic_button_light_in");
                    PressedSkin = rect;

                    label.InitDisabledColor(lcdDisabledColor, LabelColor);

                    break;

                case TextButtonType.LCDSliderButton: // +/-

                    rect = InitLCDSliderButton("lcd_bar_knob");

                    break;

                case TextButtonType.LCDSliderButtonWhite: // +/-

                    rect = InitLCDSliderButton("lcd_bar_knob_white");

                    break;

                case TextButtonType.LCDSortingArrows:

                    Rectangle iconRect = guiManager.GUISpriteSheet.GetSourceRectangle("basic_icon_sortingArrow_down");

                   icon = new Icon(guiManager);
                    icon.SetSkinLocation(SkinState.Normal,iconRect);
                    iconRect = guiManager.GUISpriteSheet.GetSourceRectangle("basic_icon_sortingArrow_up");
                    icon.SetSkinLocation(1, iconRect);
                    icon.ResizeControlToFitImage();
                    icon.CurrentSkin = 0;
                    icon.Visible = false;

                    Add(icon);
                   icon.X = Width - icon.Width - 12;
                    icon.Y = 8;

                    // icon.X = (Width - icon.Width) / 2;
                   //  icon.Y = (Height - icon.Height) / 2;
                    //icon.Y = icon.Y +1;
                    icon.CanHaveFocus = false;
                    // Width = iconRect.Width;

                    InitInventory();
                    break;


                case TextButtonType.LCDAmount:
                    label.Init(Label.LabelType.LCDWhite);
                    leftButtonPadding = 6;
                    rightButtonPadding = 8;

                    rect = GUIManager.GUISpriteSheet.GetSourceRectangle("lcd_amountbutton");

                    Width = rect.Width;
                    Height = rect.Height;

                    CornerSize = 9;

                    Skin = rect;

                    buttonBox.SetSkinLocation(SkinState.Hover, rect, lcdTooltipHoverTint, lcdTooltipHoverTint);

                    rect = GUIManager.GUISpriteSheet.GetSourceRectangle("lcd_zeroamount_bg");
                    buttonBox.SetSkinLocation(SkinState.Disabled, rect);


                    /* TODO:
                    rect = GUIManager.GUISpriteSheet.GetSourceRectangle("basic_button_light_in");
                    PressedSkin = rect;*/

                    break;
                case TextButtonType.LCDToolTipBlack:
                    rect = GUIManager.GUISpriteSheet.GetSourceRectangle("lcd_infobutton");

                    Width = rect.Width;
                    Height = rect.Height;

                    CornerSize = 5;

                    leftButtonPadding = 6;
                    rightButtonPadding = 6;

                    // yLabelOffset = 1;

                    // rect = GUIManager.GUISpriteSheet.SourceRectangle("lcd_textbutton_in");
                    Skin = rect;

                    buttonBox.SetSkinLocation(SkinState.Hover, rect, lcdTooltipHoverTint, lcdTooltipHoverTint);
                    buttonBox.SetSkinLocation(SkinState.Pressed, rect, lcdTooltipPressedTint, lcdTooltipPressedTint);
                    buttonBox.SetSkinLocation(SkinState.Checked, rect, lcdTooltipCheckedTint, lcdTooltipCheckedTint);
                    buttonBox.SetSkinLocation(SkinState.CheckedPressed, rect, lcdTooltipCheckedPressedTint, lcdTooltipCheckedPressedTint);
                    buttonBox.SetSkinLocation(SkinState.CheckedHover, rect, lcdTooltipHoverTint, lcdTooltipHoverTint);
                    buttonBox.SetSkinLocation(SkinState.HoverDisabled, rect, ImageButton.hudHoverTintNonEnabled, ImageButton.hudHoverTintNonEnabled);
                    //  HoverSkin = rect;

                    CheckedMode = CheckedModes.CanBeChecked;
                   // HasState = true;

                    Font = GUIManager.LCDandHUDFont;
                    LabelColor = Label.LCDDark; // Color.Black;

                    label.InitDisabledColor(lcdDisabledColor, LabelColor);

                    break;

                case TextButtonType.HUDToolTipWhite:
                    //  rect = GUIManager.GUISpriteSheet.GetSourceRectangle("HUD_button");
                    rect = GUIManager.GUISpriteSheet.GetSourceRectangle("HUD_info_button_lessTransparent");

                    Width = rect.Width;
                    Height = 25; // rect.Height;

                    CornerSize = 5;

                    leftButtonPadding = 6;
                    rightButtonPadding = 6;

                    //  yLabelOffset = 1;

                    // rect = GUIManager.GUISpriteSheet.SourceRectangle("lcd_textbutton_in");
                    Skin = rect;

                    buttonBox.SetSkinLocation(SkinState.Hover, rect, lcdTooltipHoverTint, lcdTooltipHoverTint);
                    buttonBox.SetSkinLocation(SkinState.Pressed, rect, lcdTooltipPressedTint, lcdTooltipPressedTint);
                    buttonBox.SetSkinLocation(SkinState.Checked, rect, lcdTooltipCheckedTint, lcdTooltipCheckedTint);
                    buttonBox.SetSkinLocation(SkinState.CheckedPressed, rect, lcdTooltipCheckedPressedTint, lcdTooltipCheckedPressedTint);
                    buttonBox.SetSkinLocation(SkinState.CheckedHover, rect, lcdTooltipHoverTint, lcdTooltipHoverTint);
                    buttonBox.SetSkinLocation(SkinState.HoverDisabled, rect, ImageButton.hudHoverTintNonEnabled, ImageButton.hudHoverTintNonEnabled);
                    buttonBox.SetSkinLocation(SkinState.Disabled, rect, lcdTooltipPressedTint, lcdTooltipPressedTint);
                    //    rect = GUIManager.GUISpriteSheet.GetSourceRectangle("HUD_info_button_in");
                    //  PressedSkin = rect;

                    CheckedMode = CheckedModes.CanBeChecked;
                    //HasState = true;

                    Font = GUIManager.LCDandHUDFont;
                    LabelColor = Color.White;

                    label.InitDisabledColor(lcdDisabledColor, LabelColor);

                    break;

                case TextButtonType.LCDCombo:
                    label.Init(Label.LabelType.LCDNormal);
                    label.InitDisabledColor(lcdDisabledColor, label.NormalColor);

                    // enabledColor = Color.White;

                    rect = guiManager.GUISpriteSheet.GetSourceRectangle("basic_dropdown_light");

                    Skin = rect;
                    buttonBox.SetSkinLocation(SkinState.Hover, rect, ImageButton.lcdHoverTint, ImageButton.lcdHoverTint);

                    rect = guiManager.GUISpriteSheet.GetSourceRectangle("basic_dropdown_light_in");
                    buttonBox.SetSkinLocation(SkinState.Pressed, rect);
                    buttonBox.SetSkinLocation(SkinState.Checked, rect);
                    buttonBox.SetSkinLocation(SkinState.CheckedPressed, rect, ImageButton.lcdPressedTint, ImageButton.lcdPressedTint);
                    buttonBox.SetSkinLocation(SkinState.CheckedHover, rect, ImageButton.lcdHoverTint, ImageButton.lcdHoverTint);

                    rect = guiManager.GUISpriteSheet.GetSourceRectangle("basic_dropdown_light_disabled");
                    buttonBox.SetSkinLocation(SkinState.Disabled, rect);


                    leftButtonPadding = 17;
                    rightButtonPadding = 25; // NEW

                    TextAlignment = TextAlign.Left;

                    CheckedMode = CheckedModes.CanBeChecked;
                    //HasState = true;

                    buttonBox.CornerSize = 26; // 15;
                    Height = rect.Height;


                    RenderType = WindowSystem.RenderType.CRTAndLCD;
                    break;

                case TextButtonType.LCDCollapsableHeaderBig:
                    InitLCDCollapsableHeaderBig("basic_collapsable_header_medium");
                    break;
                case TextButtonType.LCDCollapsableHeaderBigBlue:
                    InitLCDCollapsableHeaderBig("basic_collapsable_header_medium_blue");
                    break;
                case TextButtonType.LCDCollapsableHeaderBigGreen:
                    InitLCDCollapsableHeaderBig("basic_collapsable_header_medium_green");
                    break;
                case TextButtonType.LCDCollapsableHeaderBigRed:
                    InitLCDCollapsableHeaderBig("basic_collapsable_header_medium_red");
                    break;

                case TextButtonType.LCDCollapsableHeaderSmall:
                    rect = guiManager.GUISpriteSheet.GetSourceRectangle("basic_collapsable_header_small");
                    Skin = rect;

                    // use same effects as combobox:
                    buttonBox.SetSkinLocation(SkinState.Hover, rect, ImageButton.lcdHoverTint, ImageButton.lcdHoverTint);

                    // TODO!  rect = guiManager.GUISpriteSheet.GetSourceRectangle("basic_collapsable_header_in"); 
                    buttonBox.SetSkinLocation(SkinState.Pressed, rect);
                    buttonBox.SetSkinLocation(SkinState.Checked, rect);
                    buttonBox.SetSkinLocation(SkinState.CheckedPressed, rect, ImageButton.lcdPressedTint, ImageButton.lcdPressedTint);
                    buttonBox.SetSkinLocation(SkinState.CheckedHover, rect, ImageButton.lcdHoverTint, ImageButton.lcdHoverTint);

                    buttonBox.CornerSize = 18;
                    Height = rect.Height;

                    break;
                case TextButtonType.HUDItemQuantity:
                    rect = GUIManager.GUISpriteSheet.GetSourceRectangle("HUD_button");

                    Width = rect.Width;
                    Height = rect.Height;

                    CornerSize = 8; // 3;


                    // rect = GUIManager.GUISpriteSheet.SourceRectangle("lcd_textbutton_in");
                    Skin = rect;

                    buttonBox.SetSkinLocation(SkinState.Hover, rect, lcdTooltipHoverTint, lcdTooltipHoverTint);
                    buttonBox.SetSkinLocation(SkinState.Pressed, rect, lcdTooltipPressedTint, lcdTooltipPressedTint);
                    buttonBox.SetSkinLocation(SkinState.Checked, rect, lcdTooltipCheckedTint, lcdTooltipCheckedTint);
                    buttonBox.SetSkinLocation(SkinState.CheckedPressed, rect, lcdTooltipCheckedPressedTint, lcdTooltipCheckedPressedTint);
                    buttonBox.SetSkinLocation(SkinState.CheckedHover, rect, lcdTooltipHoverTint, lcdTooltipHoverTint);
                    buttonBox.SetSkinLocation(SkinState.HoverDisabled, rect, ImageButton.hudHoverTintNonEnabled, ImageButton.hudHoverTintNonEnabled);
                    //  HoverSkin = rect;

                    CheckedMode = CheckedModes.CanBeChecked;
                    //HasState = true;

                    Font = GUIManager.LCDandHUDFont;
                    LabelColor = Color.White;

                    break;
                case TextButtonType.HUDGather:
                    rect = GUIManager.GUISpriteSheet.GetSourceRectangle("HUD_icon_gather"); //"HUD_button_base_gather"); //"HUD_button_gather");

                    InitZoneHUD(rect);

                    break;
                case TextButtonType.HUDStockpile:
                    //  rect = GUIManager.GUISpriteSheet.GetSourceRectangle("HUD_button_base_stockpile"); 

                    rect = GUIManager.GUISpriteSheet.GetSourceRectangle("HUD_icon_stockpile");

                    InitZoneHUD(rect);

                    break;

                case TextButtonType.HUDUpgrade:
                    
                    rect = GUIManager.GUISpriteSheet.GetSourceRectangle("HUD_icon_uparrow"); 
                    InitZoneHUD(rect);

                    break;

                case TextButtonType.HUDPatrol:
                    rect = GUIManager.GUISpriteSheet.GetSourceRectangle("HUD_icon_patrol");

                    InitZoneHUD(rect);
                    break;
                case TextButtonType.HUDAttack:
                    rect = GUIManager.GUISpriteSheet.GetSourceRectangle("HUD_icon_sword");

                    InitZoneHUD(rect);
                    break;
                case TextButtonType.HUDClaim:
                    rect = GUIManager.GUISpriteSheet.GetSourceRectangle("HUD_icon_handGrabbing");

                    InitZoneHUD(rect);
                    break;
                case TextButtonType.HUDDiscard:
                    rect = GUIManager.GUISpriteSheet.GetSourceRectangle("HUD_icon_palmSlanted");

                    InitZoneHUD(rect);
                    break;
                case TextButtonType.HUDSalvage:
                    rect = GUIManager.GUISpriteSheet.GetSourceRectangle("HUD_icon_recycleArrows");

                    InitZoneHUD(rect);
                    break;

                case TextButtonType.HUDPackingDown:
                    rect = GUIManager.GUISpriteSheet.GetSourceRectangle("HUD_icon_closedBox");

                    InitZoneHUD(rect);
                    break;                    

                case TextButtonType.HUDHunt:
                    //  rect = GUIManager.GUISpriteSheet.GetSourceRectangle("HUD_button_base_hunt"); 

                    rect = GUIManager.GUISpriteSheet.GetSourceRectangle("HUD_icon_hunt");

                    InitZoneHUD(rect);

                    break;
                case TextButtonType.HUDScout:
                    //  rect = GUIManager.GUISpriteSheet.GetSourceRectangle("HUD_button_base_scout"); 

                    rect = GUIManager.GUISpriteSheet.GetSourceRectangle("HUD_icon_scout");

                    InitZoneHUD(rect);

                    break;
                case TextButtonType.HUDForage:
                    //   rect = GUIManager.GUISpriteSheet.GetSourceRectangle("HUD_button_base_forage");

                    rect = GUIManager.GUISpriteSheet.GetSourceRectangle("HUD_icon_forage");

                    InitZoneHUD(rect);

                    break;

                case TextButtonType.HUD:
                case TextButtonType.HUDHasState:

                    rect = GUIManager.GUISpriteSheet.GetSourceRectangle("HUD_button");

                    Width = rect.Width;
                    Height = hudButtonHeight; // rect.Height;

                    leftButtonPadding = 6; // 4;
                    rightButtonPadding = 6; // 4; 

                    CornerSize = CollapsablePanel.HUD_button_CornerSize; //HUD_textbox_CornerSize; // 6;

                    InitHUD(rect);

                    if (type == TextButtonType.HUDHasState)
                    {
                        CheckedMode = CheckedModes.CanBeChecked;
                        //HasState = true;
                    }

                    break;

                case TextButtonType.HUDSliderButton:

                   // InitSliderKnobSingleLine(labelType, "HUD_slider_knob", "HUD_slider_knob", 4, 3, 0, 0, true, showValueLabel);

                    InitHUDSliderButton("HUD_slider_knob");
                    
                    break;
                case TextButtonType.HUDSliderButtonWhite: // +/-

                    InitHUDSliderButton("HUD_slider_knob_white");
                   
                    break;
            }


        }

        private void InitLCDCollapsableHeaderBig(string sprite)
        {
            Rectangle rect;
            rect = guiManager.GUISpriteSheet.GetSourceRectangle(sprite); //"basic_collapsable_header_medium");
            Skin = rect;

            // use same effects as combobox:
            buttonBox.SetSkinLocation(SkinState.Hover, rect, ImageButton.lcdHoverTint, ImageButton.lcdHoverTint);

            // TODO!  rect = guiManager.GUISpriteSheet.GetSourceRectangle("basic_collapsable_header_in"); 
            buttonBox.SetSkinLocation(SkinState.Pressed, rect);
            buttonBox.SetSkinLocation(SkinState.Checked, rect);
            buttonBox.SetSkinLocation(SkinState.CheckedPressed, rect, ImageButton.lcdPressedTint, ImageButton.lcdPressedTint);
            buttonBox.SetSkinLocation(SkinState.CheckedHover, rect, ImageButton.lcdHoverTint, ImageButton.lcdHoverTint);

            buttonBox.CornerSize = 15; //18; //7;
            Height = rect.Height;
           // return rect;
        }

        private Rectangle InitHUDSliderButton(string sprite)
        {
            Rectangle rect;
            rect = GUIManager.GUISpriteSheet.GetSourceRectangle(sprite);

            Width = rect.Width;
            Height = rect.Height;

            leftButtonPadding = 3;
            rightButtonPadding = 2; // 3;  

            CornerSize = 4;

            InitHUD(rect);
            return rect;
        }

        private Rectangle InitLCDSliderButton(string sprite)
        {
            Rectangle rect;
            DebugTag = "LCDSliderButton";

            label.Init(Label.LabelType.LCDNormal);
            leftButtonPadding = 4;
            rightButtonPadding = 4;

            rect = GUIManager.GUISpriteSheet.GetSourceRectangle(sprite);

            Width = rect.Width;
            Height = rect.Height;

            CornerSize = 6;

            Skin = rect;
            //  HoverSkin = rect;
            buttonBox.SetSkinLocation(SkinState.Hover, rect, lcdTooltipHoverTint, lcdTooltipHoverTint);

            rect = GUIManager.GUISpriteSheet.GetSourceRectangle(sprite); // TODO?
            buttonBox.SetSkinLocation(SkinState.Pressed, rect, lcdTooltipPressedTint, lcdTooltipPressedTint);

            //  PressedSkin = rect;

            rect = GUIManager.GUISpriteSheet.GetSourceRectangle("lcd_bar_knob_disabled");
            buttonBox.SetSkinLocation(SkinState.Disabled, rect, lcdTooltipHoverTint, lcdTooltipHoverTint);

            label.InitDisabledColor(lcdDisabledColor, LabelColor);
            return rect;
        }

      
        private Rectangle InitMainButton(string outSprite, string hoverSprite, string inSprite, Color color, bool slim = false)
        {
            Rectangle rect;
            rect = GUIManager.GUISpriteSheet.GetSourceRectangle(outSprite);
            Width = rect.Width;
            Height = rect.Height;
            CornerSize = 10;
            Skin = rect;          

            rect = GUIManager.GUISpriteSheet.GetSourceRectangle(hoverSprite);
            HoverSkin = rect;
            // buttonBox.SetSkinLocation(SkinState.Hover, rect, Color.LightGray, Color.LightGray);

            rect = GUIManager.GUISpriteSheet.GetSourceRectangle(inSprite);
            PressedSkin = rect;
            
            buttonBox.SetSkinLocation(SkinState.Checked, rect, lcdTooltipCheckedTint, lcdTooltipCheckedTint);
            buttonBox.SetSkinLocation(SkinState.CheckedPressed, rect, lcdTooltipCheckedPressedTint, lcdTooltipCheckedPressedTint);
            buttonBox.SetSkinLocation(SkinState.CheckedHover, rect, lcdTooltipHoverTint, lcdTooltipHoverTint);
                            

            Font = GUIManager.LCDandHUDFont;  
            LabelColor = color;

            if (slim)
            {
                leftButtonPadding = 8;
                rightButtonPadding = 8;

            }
            else
            {
                leftButtonPadding = 14;
                rightButtonPadding = 14;
            }

            clickedSound = GUIManager.BeepBasicPanel;

            return rect;
        }

        /// <summary>
        /// ???
        /// </summary>
        /// <returns></returns>
        public Color GetNormalColor()
        {
            switch (Type)
            {
                case TextButtonType.HUDToolTipWhite:
                    return Color.White;
                case TextButtonType.LCDToolTipBlack:
                    return Color.Black;
                case TextButtonType.LCDAmount:
                    return Color.White;

                default: return Color.White;
            }
        }

        private void InitInventory()
        {
            Height = hudButtonHeight;
            leftButtonPadding = 26;
            rightButtonPadding = 4;

            CornerSize = 8; // 24;

         //   HasState = true;

            label.Init(Label.LabelType.LCDNormal);

            Height = hudButtonHeight; // rect.Height;

            leftButtonPadding = 6;
            rightButtonPadding = 8;

            Rectangle rect = GUIManager.GUISpriteSheet.GetSourceRectangle("basic_button_light");

           // Width = rect.Width;
            Width = Width + icon.Width;
            Height = rect.Height;

            CornerSize = 11;

            Skin = rect;

            buttonBox.SetSkinLocation(SkinState.Hover, rect, lcdTooltipHoverTint, lcdTooltipHoverTint);

            rect = GUIManager.GUISpriteSheet.GetSourceRectangle("basic_button_light_in");
            PressedSkin = rect;
            buttonBox.SetSkinLocation(SkinState.Pressed, rect);
            buttonBox.SetSkinLocation(SkinState.CheckedPressed, rect, TextButton.lcdTooltipHoverTint, TextButton.lcdTooltipHoverTint);

            label.InitDisabledColor(lcdDisabledColor, LabelColor);
        }

        private void InitZoneHUD(Rectangle iconRect)
        {
            Height = hudButtonHeight; // rect.Height;

            Icon icon = new Icon(guiManager);
            icon.SetSkinLocation(SkinState.Normal,iconRect);
            icon.ResizeControlToFitImage();
            Add(icon);
            icon.X = 9;
            CenterChildVertically(icon);
            icon.CanHaveFocus = false;

            Width = iconRect.Width;

            leftButtonPadding = 26;
            rightButtonPadding = 4;

            CornerSize = 8; // 24;

            CheckedMode = CheckedModes.CannotBeChecked;
            //HasState = true;

            Rectangle borderRect = guiManager.GUISpriteSheet.GetSourceRectangle("HUD_button");

            InitHUD(borderRect);
        }

        private void InitHUD(Rectangle rect)
        {
            Skin = rect;
            // buttonBar.SetSkinLocation(SkinState.Normal, rect, ImageButton.hudHoverTint, ImageButton.hudHoverTint); // test only

            buttonBox.SetSkinLocation(SkinState.Hover, rect, ImageButton.hudHoverTint, ImageButton.hudHoverTint);
            buttonBox.SetSkinLocation(SkinState.Pressed, rect, ImageButton.hudPressedTint, ImageButton.hudPressedTint);
            buttonBox.SetSkinLocation(SkinState.Checked, rect, ImageButton.hudCheckedTint, ImageButton.hudCheckedTint);
            buttonBox.SetSkinLocation(SkinState.CheckedPressed, rect, ImageButton.hudCheckedPressedTint, ImageButton.hudCheckedPressedTint);
            buttonBox.SetSkinLocation(SkinState.CheckedHover, rect, ImageButton.hudHoverTint, ImageButton.hudHoverTint);
            buttonBox.SetSkinLocation(SkinState.HoverDisabled, rect, ImageButton.hudHoverTintNonEnabled, ImageButton.hudHoverTintNonEnabled);
            //  HoverSkin = rect;


            Font = GUIManager.LCDandHUDFont;
            LabelColor = Color.White;

            label.InitDisabledColor(HUDDisabledColor, LabelColor);

        }

        /// <summary>
        /// Loads the default font.
        /// </summary>
        /// <param name="loadAllContent">Which type of content to load.</param>
        protected override void LoadGraphicsContent(bool loadAllContent)
        {
            /* OBSDEFAULT*/
            /*if (loadAllContent)
                Font = GUIManager.ContentManager.Load<SpriteFont>(GUIManager.LCDInterfaceFontPath); // defaultFont;
            */
            base.LoadGraphicsContent(loadAllContent);
        }

        public void ScaleToFitText()
        {
            ScaleWidthToFitText();

            Height = label.TextHeight;
        }

        public void ScaleWidthToFitText()
        {
            Width = label.TextWidth + leftButtonPadding + rightButtonPadding; // 2 * EdgeSize + 8; // 14; //9;

        }

        public int TextWidth
        {
            get { return label.TextWidth; }
        }

        public int TextHeight
        {
            get { return label.TextHeight; }
        }

        public bool Pressed
        {
            get { return this.IsPressed; }
            set
            {
                this.IsPressed = value;
                if (value)
                {
                    this.buttonBox.CurrentSkinState = SkinState.Pressed;
                }
                else 
                {
                      this.buttonBox.CurrentSkinState = SkinState.Normal;                    
                }
            }
        }

        /// <summary>
        /// Sets the label sizes, and centres it on the button.
        /// </summary>
        private void RefreshLabelPosition()
        {
            if (Type != TextButtonType.Hyperlink)
            {
                // truncate the label if the text is too long:
                this.label.Width = Math.Min(Width - leftButtonPadding - rightButtonPadding, this.label.TextWidth); // = this.label.TextWidth

                this.label.Height = this.label.TextHeight;

                if (this.TextAlignment == TextButton.TextAlign.Center)
                {
                    // Centre
                    this.label.X = (this.Width - rightButtonPadding - leftButtonPadding - this.label.Width) / 2 + leftButtonPadding; // + xLabelOffset;
                }
                else
                {
                    this.label.X = leftButtonPadding;
                }

                // this.label.X = (this.Width - this.label.Width) / 2 - rightButtonPadding; // + xLabelOffset;
                this.label.Y = (this.Height - this.label.Height) / 2 + yLabelOffset;
            }
            else // hyperlinks:
            {
                this.label.Width = this.label.TextWidth;
                this.label.Height = this.label.TextHeight;

                if (this.Width != this.label.Width)
                {
                    Width = label.Width;
                }

                this.label.X = 0; // (this.Width - this.label.Width) / 2;
                this.label.Y = 0; // (this.Height - this.label.Height) / 2;
            }
        }

        public void ChangeSkinState(SkinState skinState)
        {
            this.buttonBox.CurrentSkinState = skinState;
        }

        #region Event Handlers

        /// <summary>
        /// Updates button skin to reflect current mouse state.
        /// </summary>
        /// <param name="args">Mouse event arguments.</param>
        protected override void OnMouseOver(UIComponent sender, MouseEventArgs args)
        {
            if (Enabled == false && string.IsNullOrEmpty(ToolTip))
            {
                return;
            }
            else if (Enabled == false)
            {
                ChangeSkinState(SkinState.HoverDisabled);
                base.OnMouseOver(sender, args);
                return;
            }
            
            if (Type != TextButtonType.Hyperlink)
            {
                if (IsPressed)
                {
                    ChangeSkinState(SkinState.CheckedPressed);                  
                }
                else
                {
                    ChangeSkinState(SkinState.Hover);

                    if (HasState)
                    {
                        if (this.isChecked)
                            ChangeSkinState(SkinState.CheckedHover);
                    }
                }
            }

            base.OnMouseOver(sender, args);

        }

        /// <summary>
        /// Updates button skin to reflect current mouse state.
        /// </summary>
        /// <param name="args">Mouse event arguments.</param>
        protected override void OnMouseOut(UIComponent sender, MouseEventArgs args)
        {
            if (Type != TextButtonType.Hyperlink)
            {
                if (Enabled == false && string.IsNullOrEmpty(ToolTip))
                {
                    return;
                }

                this.buttonBox.CurrentSkinState = SkinState.Normal;

                if (HasState && this.isChecked)
                    this.buttonBox.CurrentSkinState = SkinState.Checked;

                if (Enabled == false)
                    this.buttonBox.CurrentSkinState = SkinState.Disabled;

                if (IsPressed)
                {
                    this.buttonBox.CurrentSkinState = SkinState.Pressed;
                }
            }

            base.OnMouseOut(sender, args);

        }

        /// <summary>
        /// Updates button skin to reflect current mouse state.
        /// </summary>
        /// <param name="args">Mouse event arguments.</param>
        protected override void OnMouseDown(MouseEventArgs args)
        {
            if (Enabled == false)
            {
                return;
            }
            if (Type != TextButtonType.Hyperlink) // a bit hacky...
            {
                if (args.Button == MouseButtons.Left)
                {
                    this.buttonBox.CurrentSkinState = SkinState.Pressed;

                    /* if (Type == TextButtonType.White)
                     {
                         Color = brownPressedTextColor;
                     }*/

                    if (clickedSound != null)
                    {
                        GUIManager.PlaySound(clickedSound);
                    }
                }
            }

            base.OnMouseDown(args);

            //HandleMouseDownSkinChange(args);
        }


        protected override void OnLoseFocus()
        {
            base.OnLoseFocus();

            if (Type != TextButtonType.Hyperlink) // a bit hacky...
            {
                // set back the pressed state when going modal:
                // (copied in ImageButton)
                if (Enabled)
                {
                    if (this.isChecked)
                    {
                        this.buttonBox.CurrentSkinState = SkinState.Checked;
                    }
                    else
                    {
                        this.buttonBox.CurrentSkinState = SkinState.Normal;
                    }
                }
                else
                {
                    this.buttonBox.CurrentSkinState = SkinState.Disabled; 
                }                
            }
        }

      

        /// <summary>
        /// Updates button skin to reflect current mouse state.
        /// 
        /// this method should look similar to ImageButton's...
        /// </summary>
        /// <param name="args">Mouse event arguments.</param>
        protected override void OnMouseUp(MouseEventArgs args)
        {
            if (Enabled == false)
            {
                return;
            }
            if (Type != TextButtonType.Hyperlink) // a bit hacky...
            {
                if (args.Button == MouseButtons.Left)
                {
                    if (CheckCoordinates(args.Position.X, args.Position.Y))
                    {
                        this.buttonBox.CurrentSkinState = SkinState.Hover;

                        if (checkedMode == CheckedModes.CanBeChecked && isChecked == false)
                        {
                            this.isChecked = true;
                        }
                        else if (checkedMode == CheckedModes.SwitchCheckedStateOnClick)
                        {
                            this.isChecked = !this.isChecked;
                        }

                        if (this.isChecked)
                            this.buttonBox.CurrentSkinState = SkinState.CheckedHover;

                      /*  if (HasState)
                        {
                            if (SwitchStateOnClick == true || this.isChecked == false)
                            {
                                this.isChecked = !this.isChecked;
                            }

                            if (this.isChecked)
                                this.buttonBox.CurrentSkinState = SkinState.CheckedHover;
                        }*/
                    }
                    else
                    {
                        this.buttonBox.CurrentSkinState = SkinState.Normal;

                        if (HasState)
                        {
                            if (this.isChecked)
                                this.buttonBox.CurrentSkinState = SkinState.Checked;
                        }
                    }                   
                 
                }
                
            }

            // this wil set checked state(?) and invoke click handler:
            base.OnMouseUp(args);

        }

        /// <summary>
        /// Refresh child controls when control is resized.
        /// </summary>
        /// <param name="sender">Resized control.</param>
        protected override void OnResize(UIComponent sender)
        {
            base.OnResize(sender);

            if (this.buttonBox != null)
            {
                this.buttonBox.Width = Width;
                if (Type != TextButtonType.Hyperlink)
                {
                    this.buttonBox.Height = Height;
                }
                else
                {
                    this.buttonBox.Height = label.TextHeight;
                }
            }

            /*  if (this.buttonBox != null)
              {
                  this.buttonBox.Width = Width;
                  this.buttonBox.Height = Height;
              }
              */

            if (this.label != null && Type != TextButtonType.Hyperlink)
                RefreshLabelPosition();
        }
        #endregion

        #region ICanBeChecked Members



        public bool IsChecked
        {
            get
            {
                return isChecked;
            }
            set
            {
                if (isChecked != value)
                {
                    isChecked = value;

                    this.buttonBox.CurrentSkinState = SkinState.Normal;

                    if (isChecked)
                        this.buttonBox.CurrentSkinState = SkinState.Checked;
                }
            }
        }

        #endregion
    }
}