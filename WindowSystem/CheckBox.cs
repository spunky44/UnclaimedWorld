#region File Description
//-----------------------------------------------------------------------------
// File:      CheckBox.cs
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
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace WindowSystem
{
    public enum CheckBoxType { Default, LED, LCD, LCDTinting, LCDRadioBanner, LCDRadio, HUDRadio, HUDCheckBox, LCDNoLabel }

    public enum CheckBoxFlavor { NA, Blue, Red, Green, Purple }

    /// <summary>
    /// A graphical checkbox button, with a description label.
    /// </summary>
    public class CheckBox : UIComponent, ICanBeChecked
    {



        #region Default Properties
        private static int defaultHeight = 15;
        private static int defaultHMargin = 5;
        private static string defaultFont = "Content/Fonts/DefaultFont";
        private static Rectangle defaultSkin = new Rectangle(1, 27, 15, 15);
        private static Rectangle defaultHoverSkin = new Rectangle(17, 27, 15, 15);
        private static Rectangle defaultPressedSkin = new Rectangle(33, 27, 15, 15);
        private static Rectangle defaultCheckedSkin = new Rectangle(1, 43, 15, 15);
        private static Rectangle defaultCheckedHoverSkin = new Rectangle(17, 43, 15, 15);
        private static Rectangle defaultCheckedPressedSkin = new Rectangle(33, 43, 15, 15);

        /// <summary>
        /// Sets the default control height, which is also the width and height
        /// of the button.
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
        /// Sets the default gap between the button and the label.
        /// </summary>
        /// <value>Must be at least 0.</value>
        public static int DefaultHMargin
        {
            set
            {
                Debug.Assert(value >= 0);
                defaultHMargin = value;
            }
        }

        /// <summary>
        /// Sets the default font for CheckBox labels.
        /// </summary>
        /// <value>Must not be null.</value>
        public static string DefaultFont
        {
            set
            {
                Debug.Assert(value != null);
                defaultFont = value;
            }
        }

        /// <summary>
        /// Sets the default skin.
        /// </summary>
        public static Rectangle DefaultSkin
        {
            set { defaultSkin = value; }
        }

        /// <summary>
        /// Sets the default hover skin.
        /// </summary>
        public static Rectangle DefaultHoverSkin
        {
            set { defaultHoverSkin = value; }
        }

        /// <summary>
        /// Sets the default pressed skin.
        /// </summary>
        public static Rectangle DefaultPressedSkin
        {
            set { defaultPressedSkin = value; }
        }

        /// <summary>
        /// Sets the default checked skin.
        /// </summary>
        public static Rectangle DefaultCheckedSkin
        {
            set { defaultCheckedSkin = value; }
        }

        /// <summary>
        /// Sets the default checked hover skin.
        /// </summary>
        public static Rectangle DefaultCheckedHoverSkin
        {
            set { defaultCheckedHoverSkin = value; }
        }

        /// <summary>
        /// Sets the default checked pressed skin.
        /// </summary>
        public static Rectangle DefaultCheckedPressedSkin
        {
            set { defaultCheckedPressedSkin = value; }
        }
        #endregion

        #region Fields
        public ImageButton button;
        private Label label;
        private int hMargin;
        private int vMargin;
        #endregion

        #region Properties
        /// <summary>
        /// Sets the gap between the button and the label.
        /// </summary>
        /// <value>Must be at least 0.</value>
        public int HMargin
        {
            get { return hMargin; }
            set
            {
               // Debug.Assert(value >= 0);

                hMargin = value;
                RefreshMargins();
            }
        }

        CheckedModes checkedMode = CheckedModes.SwitchCheckedStateOnClick;
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

        public override EventArgs EventArgs
        {
            get
            {
                return base.EventArgs;
            }
            set
            {
                base.EventArgs = value;
                button.EventArgs = value;
            }
        }
     
        /// <summary>
        /// this setting determines if the button can go from checked to not checked by clicking on it.
        /// for radiobuttons, we typically don't want this.
        /// </summary>
        public bool SwitchStateOnClick
        {
            get
            {
                return button.CheckedMode == CheckedModes.SwitchCheckedStateOnClick; // .SwitchStateOnClick;
            }
            set
            {
                if (value == true)
                {
                    button.CheckedMode = CheckedModes.SwitchCheckedStateOnClick;
                }
                else
                {
                    button.CheckedMode = CheckedModes.CanBeChecked;
                }
                //button.SwitchStateOnClick = value;
            }
        }

        /// <summary>
        /// sets the relative y position of the button and the label
        /// </summary>
        public int VMargin
        {
            get { return vMargin; }
            set
            {                
                vMargin = value;
                RefreshMargins();
            }
        }

        public Color BackColor
        {
            set
            {
                label.BackColor = value;
            }
        }      

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

                    button.Enabled = value;
                    label.Enabled = value;                   
                }
            }
        }

        public override string ToolTip
        {
            get
            {
                return button.ToolTip;
            }
            set
            {
                button.ToolTip = value;                
            }
        }

        /// <summary>
        /// Sets the label font.
        /// </summary>
        /// <value>Must not be a valid path.</value>
        public SpriteFont Font
        {
            set { label.Font = value; }
        }

        /// <summary>
        /// Get/Set the description text.
        /// </summary>
        /// <value>Must not be null.</value>
        public string Text
        {
            get { return label.Text; }
            set
            {
                label.Text = value;

                // Automatically set control width              
                SetWidth();
            }
        }

        private void SetWidth()
        {
            this.Width = label.X + label.Width;
        }

        public Label Label
        {
            get { return label; }
        }

        public void FitToText()
        {
            label.FitToText();

           // RefreshMargins(); // sets label width from Width !?!? truncates the label.

            SetWidth(); // sets Width from label width
        }


        public void SetNormalColor(Color? color)
        {
            button.SetSkinLocation(SkinState.Normal, null, color, color);
            button.SetSkinLocation(SkinState.Checked, null, color, color);
        }

        /// <summary>
        /// Sets the skin.
        /// </summary>
        public Rectangle Skin
        {
            set { this.button.SetSkinLocation(SkinState.Normal, value); }
        }

        /// <summary>
        /// Sets the hover skin.
        /// </summary>
        public Rectangle HoverSkin
        {
            set { this.button.SetSkinLocation(SkinState.Hover, value); }
        }

        /// <summary>
        /// Sets the pressed skin.
        /// </summary>
        public Rectangle PressedSkin
        {
            set { this.button.SetSkinLocation(/*SkinState.Pressed*/  2, value); } // 2???
        }

        /// <summary>
        /// Sets the checked skin.
        /// </summary>
        public Rectangle CheckedSkin
        {
            set { this.button.SetSkinLocation(3 /*SkinState.Checked*/, value); } // 3???
        }

        /// <summary>
        /// Sets the checked hover skin.
        /// </summary>
        public Rectangle CheckedHoverSkin
        {
            set { this.button.SetSkinLocation(SkinState.CheckedHover, value); }
        }

        /// <summary>
        /// Sets the checked pressed skin.
        /// </summary>
        public Rectangle CheckedPressedSkin
        {
            set { this.button.SetSkinLocation(/*SkinState.CheckedPressed*/ 5, value); } // 5????
        }

        /// <summary>
        /// Get/Set whether the button is currently checked.
        /// </summary>
        public bool IsChecked
        {
            get { return button.IsChecked; }
            set { button.IsChecked = value; }
        }

        /// <summary>
        /// Gets the checkbox button.
        /// </summary>
        protected ImageButton Button
        {
            get { return button; }
        }
        #endregion

        #region Events
        // Event is simply used to relay clicks on the button
        new public event ClickHandler Click;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game">The currently running Game object.</param>
        /// <param name="guiManager">GUIManager that this control is part of.
        /// </param>
        public CheckBox(GUIManager guiManager)
            : base(guiManager)
        {
            #region Create Child Controls
            this.button = new ImageButton(guiManager);
            this.label = new Label(guiManager);
            #endregion

            #region Add Child Controls
            Add(this.button);
            Add(this.label);
            #endregion

            #region Set Properties
            CanHaveFocus = false;
            #endregion

            #region Set Default Properties
            Width = defaultHeight;
            Height = defaultHeight;
            HMargin = defaultHMargin;
            Skin = defaultSkin;
            HoverSkin = defaultHoverSkin;
            PressedSkin = defaultPressedSkin;
            CheckedSkin = defaultCheckedSkin;
            CheckedHoverSkin = defaultCheckedHoverSkin;
            CheckedPressedSkin = defaultCheckedPressedSkin;
            #endregion

            #region Event Handlers
            this.button.Click += new ClickHandler(OnClick);
            #endregion
        }
        #endregion

        /// <summary>
        /// Load default font.
        /// </summary>
        /// <param name="loadAllContent">Which type of content to load.</param>
        protected override void LoadGraphicsContent(bool loadAllContent)
        {
            /* OBSDEFAULT
            if (loadAllContent)
                Font = defaultFont;
            */
            base.LoadGraphicsContent(loadAllContent);
        }

        

        public void Init(CheckBoxType type)
        {
            Init(type, CheckBoxFlavor.NA);
        }

        CheckBoxType type;

        public void Init(CheckBoxType type, CheckBoxFlavor flavor)
        {
           // button.DebugTag = "debugRadio";

            this.type = type;

            Rectangle rect;
            switch (type)
            {               
               
                case CheckBoxType.LCD:

                    button.Init(ImageButtonType.LCDCheckbox);

                    /*
                    button.InitButton("basic_checkbox_unselected", "basic_checkbox_selected", ImageButton.lcdHoverTint, ImageButton.lcdPressedTint);
                  
                    rect = GUIManager.GUISpriteSheet.GetSourceRectangle("basic_checkbox_disabled");
                    button.SetSkinLocation(SkinState.Disabled, rect);
               */

                    label.Init(Label.LabelType.LCDCheckbox);
                    
                    Height = (int)MathHelper.Max(button.Height, label.Height);
                    CenterChildVertically(label);

                    HMargin = 0;
                    VMargin = 1;

                    break;

                case CheckBoxType.LCDNoLabel:

                    button.Init(ImageButtonType.LCDCheckbox);

                    /*
                    button.InitButton("basic_checkbox_unselected", "basic_checkbox_selected", ImageButton.lcdHoverTint, ImageButton.lcdPressedTint);
                  
                    rect = GUIManager.GUISpriteSheet.GetSourceRectangle("basic_checkbox_disabled");
                    button.SetSkinLocation(SkinState.Disabled, rect);
               */

                  //  label.Init(Label.LabelType.LCDCheckbox);

                    Height = button.Height;
                   // CenterChildVertically(label);

                    HMargin = 0;
                    VMargin = 1;

                    break;

                case CheckBoxType.LCDTinting:

                    button.Init(ImageButtonType.LCDCheckbox);

                    label.Init(Label.LabelType.LCDCheckboxWhite);

                    Height = (int)MathHelper.Max(button.Height, label.Height);
                    CenterChildVertically(label);

                    HMargin = 0;
                    VMargin = 1;

                    break;
                
                case CheckBoxType.HUDCheckBox://-----------------------------------------------------------------------------

                    button.Init(ImageButtonType.HUDCheckbox);                                   
                     
                    label.Init(Label.LabelType.HUDWindow);
                    
                    Height = (int)MathHelper.Max(button.Height, label.Height);
                    CenterChildVertically(label);

                    
                    HMargin = 0;
                    VMargin = 1;

                    break;

                case CheckBoxType.LCDRadioBanner:
                   
                    button.InitButton("basic_radio_unselected", "basic_radio_selected", ImageButton.lcdHoverTint, ImageButton.lcdPressedTint, disabledSprite: "basic_radio_disabled");

                    label.Init(WindowSystem.Label.LabelType.LCDRadioBanner);
                  
                    Height = button.Height;
                    Width = button.Width;

                    VMargin = 2;
                    HMargin = -6;

                    break;

                case CheckBoxType.LCDRadio:

                    button.InitButton("basic_radio_unselected", "basic_radio_selected", ImageButton.lcdHoverTint, ImageButton.lcdPressedTint);

                    label.Init(WindowSystem.Label.LabelType.LCDRadio);
                 

                    Height = button.Height;
                    Width = button.Width;

                    VMargin = 2;
                    HMargin = 0;

                    break;

                case CheckBoxType.HUDRadio:
                   
                    button.InitButton("HUD_radio_empty", "HUD_radio_filled", ImageButton.hudHoverTint, ImageButton.hudPressedTint);
                    button.ScaleImageToSizeOfControl = false;

                    label.Font = GUIManager.LCDandHUDFont;
                    label.NormalColor = Color.White;

                 
                  //  HMargin = 0;

                    break;
                case CheckBoxType.LED:
                    // this type of button has a state!
                    switch(flavor)
                    {
                        case CheckBoxFlavor.Blue:
                            rect = guiManager.GUISpriteSheet.GetSourceRectangle("minimap_light_blue_off");
                            break;
                        case CheckBoxFlavor.Purple:
                            rect = guiManager.GUISpriteSheet.GetSourceRectangle("minimap_light_purple_off");
                            break;
                        case CheckBoxFlavor.Green:
                            rect = guiManager.GUISpriteSheet.GetSourceRectangle("minimap_light_green_off");
                            break;
                        default: // Red
                            rect = guiManager.GUISpriteSheet.GetSourceRectangle("minimap_light_red_off");
                            break;
                    }

                    Skin = rect;
                    HoverSkin = rect;
                    PressedSkin = rect;
                    
                    button.Width = rect.Width;
                    button.Height = rect.Height;
                    
                    Height = rect.Height;

                    switch (flavor)
                    {
                        case CheckBoxFlavor.Blue:
                            rect = guiManager.GUISpriteSheet.GetSourceRectangle("minimap_light_blue_on");
                            break;
                        case CheckBoxFlavor.Purple:
                            rect = guiManager.GUISpriteSheet.GetSourceRectangle("minimap_light_purple_on");
                            break;
                        case CheckBoxFlavor.Green:
                            rect = guiManager.GUISpriteSheet.GetSourceRectangle("minimap_light_green_on");
                            break;
                        default: // Red
                            rect = guiManager.GUISpriteSheet.GetSourceRectangle("minimap_light_red_on");
                            break;
                    }
                    
                    CheckedSkin = rect;
                    CheckedHoverSkin = rect;
                    CheckedPressedSkin = rect;

                    label.Font = GUIManager.MediumButtonInterfaceFont;
                    label.NormalColor = Color.White;

                    VMargin = 12;
                    HMargin = -4;

                    break;
            }

          //  RefreshMargins();
            SetWidth();


        /*    switch (type)
            {
                case ImageButtonType.Comm:
                    Rectangle rect = GUIManager.GUISpriteSheet.SourceRectangle("event_button_out");
                    Width = rect.Width;
                    Height = rect.Height;
                    EdgeSize = 4;
                    Skin = rect;
                    HoverSkin = rect;
                    rect = GUIManager.GUISpriteSheet.SourceRectangle("event_button_down");
                    PressedSkin = rect;
                    Font = GUIManager.ButtonFaceFontPath; // GUIManager.RegularInterfaceFontPath; // no antialiasing: GUIManager.ButtonFaceFontPath; // GUIManager.RegularInterfaceFontPath;
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

        public void InitHUD(Rectangle rect)
        {
            Skin = rect;
            // buttonBar.SetSkinLocation(SkinState.Normal, rect, ImageButton.hudHoverTint, ImageButton.hudHoverTint); // test only

            button.SetSkinLocation(SkinState.Hover, rect, ImageButton.hudHoverTint, ImageButton.hudHoverTint);
            button.SetSkinLocation(SkinState.Pressed, rect, ImageButton.hudPressedTint, ImageButton.hudPressedTint);
            button.SetSkinLocation(SkinState.Checked, rect, ImageButton.hudCheckedTint, ImageButton.hudCheckedTint);
            button.SetSkinLocation(SkinState.CheckedPressed, rect, ImageButton.hudCheckedPressedTint, ImageButton.hudCheckedPressedTint);
            button.SetSkinLocation(SkinState.CheckedHover, rect, ImageButton.hudHoverTint, ImageButton.hudHoverTint);
            //  HoverSkin = rect;


            Font = GUIManager.LCDandHUDFont;
           // Color = Color.White;

        }


        /// <summary>
        /// Update controls to respect new margins.
        /// </summary>
        protected void RefreshMargins()
        {
            // Update margin
            label.X = HMargin + button.Width;
            label.Width = Width - button.Width - HMargin;

            // Lars:
            if (type != CheckBoxType.LCD
                && type != CheckBoxType.LCDTinting)
            {
                label.Y = VMargin;
            }
            //label.Width = Width - button.Width - HMargin;

        }

        #region Event Handlers
        /// <summary>
        /// Update locations of child control.
        /// </summary>
        /// <param name="sender"></param>
        protected override void OnResize(UIComponent sender)
        {
            base.OnResize(sender);

            //Lars - is it necessary?

            // Update child sizes
         /*   button.Width = Height;*/ // Square only!!!

            if (type != CheckBoxType.LCD
             && type != CheckBoxType.LCDRadioBanner
             && type != CheckBoxType.LCDTinting)
            {
                button.Height = Height;
                label.Height = Height;
            }

            RefreshMargins();
        
        
        }
               

        /// <summary>
        /// This event handler is called when the button is clicked. It simply
        /// relays the event to the event handler of this control.
        /// </summary>
        /// <param name="sender">Clicked control.</param>
        public void OnClick(UIComponent sender, EventArgs e)
        {
            if (Click != null)
                Click.Invoke(this, e);
        }
        #endregion
    }
}