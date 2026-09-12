#region File Description
//-----------------------------------------------------------------------------
// File:      ImageButton.cs
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
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using InputEventSystem;
using Microsoft.Xna.Framework.Audio;
#endregion

namespace WindowSystem
{
    public enum ImageButtonType
    {
        Default, Comm, CommSlim, MinimapRubber, Help, LCD, LCDIncrease, LCDDecrease, LCDExpand, LCDCancel, //LCDHigh, LCDUrgent, LCDNormal, 
        LCDCollapse, LCDArrowRight, LCDArrowDown, LCDArrowDownNew, MetalPanel, MetalPanelHorizontal, White, Black, HUDIncrease, HUDDecrease, HUDArrowRight, HUDArrowUp, HUDArrowDown, HUDCycleEntity,
        HUDModifyZone, HUDSalvage, HUDPackDown, HUDBuild, HUDClaim, HUDDiscard, HUDExpandCollapseTinted, HUDExpandArrowDown, HUDCheckbox, HUDShowProductionInfo, HUDShowGeneralInfo,
        HUDClose, HUDRadioButton, HUDInfoScrollUp, HUDInfoScrollDown, BuildButton,
        BuyAction, SellAction, LoadAction, UnloadAction, EmbarkAction, DisembarkAction, AddAction,
        InventoryButton, TasksButton, EventDialogsButton, DiplomacyButton, GraphButton, LedgerButton, MissionsButton, PersonnelButton, PolicyButton,
        EntityInfo, MapAreaInfo,
        Minimap,
        ScanButton, LCDCheckbox, ResourceSelectionArrow,
        HUDDelete, //ArrowBlue_Left, ArrowBlue_Right,
        CenterOnEntity,
        Counter,
        FoodRating, SecurityRating, ComfortRating,
        SiteMarker, SiteMarkerTallPin, SiteMarkerShortPin,
        LCDTracking, LCDSortingArrows, LCDList, LCDExpandWithUpAndDownArrows,
        HUDCrosshair,
        HUDPrices,
        HUDPeople,
        LCDPadlock,
        ComfortPolicy,
        SecurityPolicy,
        FoodPolicy,
        HUDPadlock,
        LCDPadlockWhite,
        HUDPin,
        HUD
    }



    /// <summary>
    /// A button with state checked/unchecked, comprised of seperate images for each state.
    /// </summary>
    public class ImageButton : Icon, ICanBeChecked
    {


        #region Fields
        protected bool isChecked = false;
      //  private bool switchStateOnClick = true;

        protected SoundEffect clickedSound;

        protected Icon icon;

        /// <summary>
        /// if not set, the icon will be centered
        /// </summary>
        private int? iconOffSetYPos;
        private int? iconOffSetXPos;

     //   protected Bar buttonBar;

      
        public bool RightClickEnabled = false;

        #endregion

        #region Properties

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


        public Color NormalColor
        {
            set
            {
                if (Color != value)
                {
                    base.Color = value;
                    SetSkinLocation(SkinState.Normal, null, value, value, modulateColor: true); // better to use Modulate, so final hover color is affected too
                }
            }
        }

        public override int Width
        {
            get
            {
                return base.Width;
            }
            set
            {
                base.Width = value;

                RecalculateIconPosition();
            }
        }

        /// <summary>
        /// this setting determines if the button can go from checked to not checked by clicking on it.
        /// for radiobuttons, we typically don't want this.
        /// (Also in TextButton!)
        /// </summary>
      /*  public bool SwitchStateOnClick
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

        /// <summary>
        /// Sets the skin.
        /// </summary>
        public Rectangle Skin
        {
            set { SetSkinLocation(SkinState.Normal, value); }
        }

        /// <summary>
        /// Sets the hover skin.
        /// </summary>
        public Rectangle HoverSkin
        {
            set { SetSkinLocation(SkinState.Hover, value); }
        }

        /// <summary>
        /// Sets the pressed skin.
        /// </summary>
        public Rectangle PressedSkin
        {
            set { SetSkinLocation(SkinState.Pressed, value); }
        }

        public Color DisabledColor = Microsoft.Xna.Framework.Color.LightGray;

        /// <summary>
        /// TODO: scrap these... should not be needed when there is a Disabled skin state and a Normal skin state
        /// </summary>
        private Color? enabledColor = null;
        private string enabledNormalSkin = null;

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

                    if (base.Enabled)
                    {
                        // TODO: delete this...
                        if (enabledNormalSkin != null && enabledColor.HasValue)
                        {
                            // change skin to show as enabled:                           
                            SetSkinLocation(SkinState.Normal, null, enabledColor.Value, enabledColor.Value);
                        }

                        if (IsChecked)
                        {
                            CurrentSkinState = SkinState.Checked;
                        }
                        else if (IsPressed)
                        {
                            CurrentSkinState = SkinState.Pressed;
                        }
                        else
                        {
                            CurrentSkinState = SkinState.Normal;
                        }

                        
                    }
                    else
                    {
                        // TODO: delete this...
                        if (enabledNormalSkin != null && enabledColor.HasValue)
                        {
                            // change skin to show as disabled:        
                            SetSkinLocation(SkinState.Normal, null, DisabledColor, DisabledColor); 
                        }

                        if (IsChecked)
                        {
                            CurrentSkinState = SkinState.CheckedDisabled;
                        }                        
                        else
                        {
                            CurrentSkinState = SkinState.Disabled;
                        }                       

                    }
                }

            }
        }


        void UpdateSkinState(bool hover = false)
        {
            if (base.Enabled)
            {
                if (hover)
                {
                    if (IsChecked)
                    {
                        CurrentSkinState = SkinState.CheckedHover;
                    }
                    else if (IsPressed)
                    {
                        CurrentSkinState = SkinState.Pressed;
                    }
                    else
                    {
                        CurrentSkinState = SkinState.Hover;
                    }
                }
                else
                {
                    if (IsChecked)
                    {
                        CurrentSkinState = SkinState.Checked;
                    }
                    else if (IsPressed)
                    {
                        CurrentSkinState = SkinState.Pressed;
                    }
                    else
                    {
                        CurrentSkinState = SkinState.Normal;
                    }                 
                }

            }
            else
            {
                if (IsChecked)
                {
                    if (hover)
                    {
                        CurrentSkinState = SkinState.CheckedDisabledHover;
                    }
                    else
                    {
                        CurrentSkinState = SkinState.CheckedDisabled;
                    }
                }                
                else
                {
                    if (hover)
                    {
                        CurrentSkinState = SkinState.HoverDisabled;
                    }
                    else
                    {
                        CurrentSkinState = SkinState.Disabled;
                    }
                }
            }
        }

        /// <summary>
        /// Get/Set whether the button is checked.
        /// </summary>
        public bool IsChecked
        {
            get { return isChecked; }
            set
            {
                if (isChecked != value)
                {
                    this.isChecked = value;

                    if (IsChecked)
                    {
                        if (!Enabled)
                        {
                            CurrentSkinState = SkinState.CheckedDisabled;
                        }
                        else if (IsPressed)
                        {
                            CurrentSkinState = SkinState.CheckedPressed;
                        }
                        else 
                        {
                            CurrentSkinState = SkinState.Checked;
                        }                       
                    }
                    else
                    {
                        if (!Enabled)
                        {
                            CurrentSkinState = SkinState.Disabled;
                        }
                        else if (IsPressed)
                        {
                            CurrentSkinState = SkinState.Pressed;
                        }                       
                        else
                        {
                            CurrentSkinState = SkinState.Normal;
                        }
                    }
                    
                }
            }
        }
        #endregion

        #region Constructor
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game">The currently running Game object.</param>
        /// <param name="guiManager">GUIManager that this control is part of.</param>
        public ImageButton(GUIManager guiManager)
            : base(guiManager)
        {
            ScaleImageToSizeOfControl = true;
                       

        }
        #endregion


        /// <summary>
        /// delete this
        /// </summary>
     /*   public bool Pressed
        {
            get { return this.IsPressed; }
            set
            {
                this.IsPressed = value;
                if (value)
                {
                    this.CurrentSkinState = SkinState.Pressed;                    
                }  
            }
        }*/

      
        public void Init(ImageButtonType type)
        {
            Init(type, -1);
        }

        public void Init(ImageButtonType type, int flavour)
        {
            Rectangle rect;
            switch (type)
            {
                case ImageButtonType.Comm:
                    rect = guiManager.GUISpriteSheet.GetSourceRectangle("event_button_out");
                    Skin = rect;
                    Width = rect.Width;
                    Height = rect.Height;

                    rect = guiManager.GUISpriteSheet.GetSourceRectangle("event_button_hover");
                    SetSkinLocation(SkinState.Hover, rect);

                    //Skin = new Rectangle(84, 23, 17, 17);
                    // set 'in' here - check box? it has a state...
                    rect = GUIManager.GUISpriteSheet.GetSourceRectangle("event_button_down");
                    PressedSkin = rect;

                    clickedSound = GUIManager.Click1;
                    break;

                case ImageButtonType.EventDialogsButton:
                    InitButton("dialog_event_button_out", "dialog_event_button_in", "dialog_event_button_hoover", "dialog_event_button_in_hover");
                    clickedSound = GUIManager.Click1;

                    break;

                case ImageButtonType.DiplomacyButton:
                    InitButton("diplomacy_button_out", "diplomacy_button_in", "diplomacy_button_hover", "diplomacy_button_in_hover");
                    clickedSound = GUIManager.Click1;

                    break;

                case ImageButtonType.PolicyButton:
                    InitButton("policy_button_out", "policy_button_in", "policy_button_hover", "policy_button_in_hover");
                    clickedSound = GUIManager.Click1;

                    break;

                case ImageButtonType.GraphButton:
                    InitButton("graph_button_out", "graph_button_in", "graph_button_hoover", "graph_button_in_hover");
                    clickedSound = GUIManager.Click1;

                    break;

                case ImageButtonType.LedgerButton:
                    InitButton("ledger_button_out", "ledger_button_in", "ledger_button_hoover", "ledger_button_in_hover");
                    clickedSound = GUIManager.Click1;

                    break;

                case ImageButtonType.TasksButton:
                    InitButton("tasks_button_out", "tasks_button_in", "tasks_button_hoover", "tasks_button_in_hover");
                    clickedSound = GUIManager.Click1;

                    break;

                case ImageButtonType.InventoryButton:
                    InitButton("inventory_button_out", "inventory_button_in", "inventory_button_hoover", "inventory_button_in_hover");
                    clickedSound = GUIManager.Click1;

                    break;

                case ImageButtonType.MissionsButton:
                    InitButton("comm_button_out", "comm_button_in", "comm_button_hover", "comm_button_in_hover");
                    clickedSound = GUIManager.Click1;

                    break;

                case ImageButtonType.PersonnelButton:
                    InitButton("personnel_button_out", "personnel_button_in", "personnel_button_hover", "personnel_button_in_hover");
                    clickedSound = GUIManager.Click1;

                    break;

                case ImageButtonType.EntityInfo:
                    InitButton("infoswitch_entitybutton_out", "infoswitch_entitybutton_in", lcdHoverTint, lcdPressedTint);
                    clickedSound = GUIManager.Click1;

                    rect = GUIManager.GUISpriteSheet.GetSourceRectangle("infoswitch_entitybutton_disabled");
                    SetSkinLocation(SkinState.Disabled, rect);

                    break;

                case ImageButtonType.MapAreaInfo:
                    InitButton("infoswitch_zonebutton_out", "infoswitch_zonebutton_in", lcdHoverTint, lcdPressedTint);
                    clickedSound = GUIManager.Click1;

                    rect = GUIManager.GUISpriteSheet.GetSourceRectangle("infoswitch_zonebutton_disabled");
                    SetSkinLocation(SkinState.Disabled, rect);

                    break;

                case ImageButtonType.CenterOnEntity:
                    InitButton("centerentity_button_out", "centerentity_button_in", "centerentity_button_hover");
                    clickedSound = GUIManager.Click1;

                    break;

                case ImageButtonType.Counter:
                    InitButton("counter_button_out", "counter_button_in", panelHoverTint); // "counter_button_hover");

                    break;

                case ImageButtonType.FoodRating:
                    InitButton("nutritionRating_button_out", "nutritionRating_button_in", panelHoverTint); // "counter_button_hover");
                    checkedMode = CheckedModes.CannotBeChecked;
                    break;

                case ImageButtonType.SecurityRating:
                    InitButton("securityRating_button_out", "securityRating_button_in", panelHoverTint); // "counter_button_hover");
                    checkedMode = CheckedModes.CannotBeChecked; 
                    break;

                case ImageButtonType.ComfortRating:
                    InitButton("comfortRating_button_out", "comfortRating_button_in", panelHoverTint); // "counter_button_hover");
                    checkedMode = CheckedModes.CannotBeChecked;
                    break;
                case ImageButtonType.BuyAction:
                    InitButton("mission_button_crate_green", lcdHoverTint, lcdPressedTint); 

                    break;
                case ImageButtonType.SellAction:
                    InitButton("mission_button_crate_red", lcdHoverTint, lcdPressedTint);

                    break;
                case ImageButtonType.LoadAction:
                    InitButton("mission_button_crate_green", lcdHoverTint, lcdPressedTint);

                    break;
                case ImageButtonType.UnloadAction:
                    InitButton("mission_button_crate_red", lcdHoverTint, lcdPressedTint);

                    break;
                case ImageButtonType.EmbarkAction:
                    InitButton("mission_button_people_green", lcdHoverTint, lcdPressedTint);

                    break;
                case ImageButtonType.DisembarkAction:
                    InitButton("mission_button_people_red", lcdHoverTint, lcdPressedTint);

                    break;
                case ImageButtonType.AddAction:
                    InitButton("mission_button_plus", lcdHoverTint, lcdPressedTint);

                    break;
                case ImageButtonType.ComfortPolicy:
                    InitButton("policy_button_comfort", lcdHoverTint, lcdPressedTint);
                   // SetSkinLocation(SkinState.Disabled, null, null, null);

                    break;
                case ImageButtonType.SecurityPolicy:
                    InitButton("policy_button_security", lcdHoverTint, lcdPressedTint);
                 //   SetSkinLocation(SkinState.Disabled, null, null, null);

                    break;
                case ImageButtonType.FoodPolicy:
                    InitButton("policy_button_food", lcdHoverTint, lcdPressedTint);
                  //  SetSkinLocation(SkinState.Disabled, null, null, null);

                    break;
              /*  case ImageButtonType.ComfortPolicy:
                    InitButton("policy_button_comfort", lcdHoverTint, lcdPressedTint, hasDisabledState: false);
                    SetSkinLocation(SkinState.Disabled, null, null, null);            

                    break;
                case ImageButtonType.SecurityPolicy:
                    InitButton("policy_button_security", lcdHoverTint, lcdPressedTint, hasDisabledState: false);
                    SetSkinLocation(SkinState.Disabled, null, null, null);            

                    break;
                case ImageButtonType.FoodPolicy:
                    InitButton("policy_button_food", lcdHoverTint, lcdPressedTint, hasDisabledState: false);
                    SetSkinLocation(SkinState.Disabled, null, null, null);            

                    break;*/
                case ImageButtonType.BuildButton:
                    rect = guiManager.GUISpriteSheet.GetSourceRectangle("build_button_out");
                    Skin = rect;
                    
                    Width = rect.Width;
                    Height = rect.Height;

                    rect = guiManager.GUISpriteSheet.GetSourceRectangle("build_button_in");                   
                    SetSkinLocation(SkinState.Checked, rect);
                    PressedSkin = rect;

                    rect = guiManager.GUISpriteSheet.GetSourceRectangle("build_button_in_hover");                   
                    SetSkinLocation(SkinState.CheckedHover, rect);

                    rect = guiManager.GUISpriteSheet.GetSourceRectangle("build_button_hoover");
                    SetSkinLocation(SkinState.Hover, rect);                    

                    clickedSound = GUIManager.Click1;
                    break;
                                        

                 case ImageButtonType.ResourceSelectionArrow:
                    rect = guiManager.GUISpriteSheet.GetSourceRectangle("arrow_button_out");
                    Skin = rect;

                    Width = rect.Width;
                    Height = rect.Height;

                    rect = guiManager.GUISpriteSheet.GetSourceRectangle("arrow_button_in");     
                    
                    SetSkinLocation(SkinState.Checked, rect);
                    PressedSkin = rect;

                    rect = guiManager.GUISpriteSheet.GetSourceRectangle("arrow_button_in_hover");      
                    SetSkinLocation(SkinState.CheckedHover, rect);

                    rect = guiManager.GUISpriteSheet.GetSourceRectangle("arrow_button_hover");
                    SetSkinLocation(SkinState.Hover, rect);
                
                    
                    clickedSound = GUIManager.Click1;
                    break;


                case ImageButtonType.ScanButton:
                    rect = guiManager.GUISpriteSheet.GetSourceRectangle("scan_button_out");
                    Skin = rect;

                    Width = rect.Width;
                    Height = rect.Height;

                    rect = guiManager.GUISpriteSheet.GetSourceRectangle("scan_button_in");      
                    SetSkinLocation(SkinState.Checked, rect);
                    PressedSkin = rect;

                    rect = guiManager.GUISpriteSheet.GetSourceRectangle("scan_button_in_hover");      
                    SetSkinLocation(SkinState.CheckedHover, rect);

                    rect = guiManager.GUISpriteSheet.GetSourceRectangle("scan_button_hover");
                    SetSkinLocation(SkinState.Hover, rect);
                
                    
                    clickedSound = GUIManager.Click1;
                    break;

                case ImageButtonType.Minimap:
                    InitButton("minimap_button_out", "minimap_button_in", "minimap_button_hover", "minimap_button_in_hover"); 

                    break;

                case ImageButtonType.CommSlim:
                    rect = guiManager.GUISpriteSheet.GetSourceRectangle("event_slim_button_out");
                    Skin = rect;
                    SetSkinLocation(SkinState.Checked, rect);
                    SetSkinLocation(SkinState.CheckedHover, rect);

                    rect = guiManager.GUISpriteSheet.GetSourceRectangle("event_slim_button_hover");
                    SetSkinLocation(SkinState.Hover, rect);


                    Width = rect.Width;
                    Height = rect.Height;

                    rect = GUIManager.GUISpriteSheet.GetSourceRectangle("event_slim_button_down");
                    PressedSkin = rect;

                    clickedSound = GUIManager.Click1;
                    break;                

                case ImageButtonType.MinimapRubber:
                    rect = guiManager.GUISpriteSheet.GetSourceRectangle("minimap_button_out");
                    Skin = rect;
                    SetSkinLocation(SkinState.Checked, rect);

                    rect = GUIManager.GUISpriteSheet.GetSourceRectangle("minimap_button_hover");
                    SetSkinLocation(SkinState.Hover, rect);
                    SetSkinLocation(SkinState.CheckedHover, rect);

                    Width = rect.Width;
                    Height = rect.Height;

                    rect = GUIManager.GUISpriteSheet.GetSourceRectangle("minimap_button_in");
                    PressedSkin = rect;
                    SetSkinLocation(SkinState.CheckedPressed, rect);

                    clickedSound = GUIManager.Click1;
                    break;
                case ImageButtonType.Help:
                    rect = guiManager.GUISpriteSheet.GetSourceRectangle("help_button_out");
                    Skin = rect;
                    SetSkinLocation(SkinState.Checked, rect);
                    SetSkinLocation(SkinState.CheckedHover, rect);
                    SetSkinLocation(SkinState.Hover, rect);
                    Width = rect.Width;
                    Height = rect.Height;

                    rect = GUIManager.GUISpriteSheet.GetSourceRectangle("help_button_in");
                    PressedSkin = rect;
                    SetSkinLocation(SkinState.CheckedPressed, rect);

                    clickedSound = GUIManager.Click1;
                    break;
                case ImageButtonType.LCDIncrease:
                    InitButton("spinner_increase", "spinner_increase_in", "spinner_increase_hover");
                    clickedSound = GUIManager.BeepLCD; // GUIManager.Bip1;
                    break;
                case ImageButtonType.LCDDecrease:
                    InitButton("spinner_decrease", "spinner_decrease_in", "spinner_decrease_hover");
                    clickedSound = GUIManager.BeepLCD; // GUIManager.Bip1;
                    break;
                case ImageButtonType.LCDCancel:
                    InitButton("spinner_decrease", "spinner_decrease_in", "spinner_decrease_hover"); // placeholder graphics!
                    clickedSound = GUIManager.BeepLCD;
                    break;
                case ImageButtonType.LCDExpand:
                    InitButton("lcd_expandbutton", "lcd_expandbutton_in", "lcd_expandbutton_hover");
                    clickedSound = GUIManager.BeepLCD; 
                    break;
                case ImageButtonType.LCDCollapse:
                    InitButton("lcd_collapsebutton", "lcd_collapsebutton_in", "lcd_collapsebutton_hover");
                    clickedSound = GUIManager.BeepLCD;
                    break;
                case ImageButtonType.LCDArrowRight:
                    InitButton("lcd_rightarrow", "lcd_rightarrow_in", "lcd_rightarrow_hover");
                    clickedSound = GUIManager.BeepLCD;
                    break;
                case ImageButtonType.LCDPadlock:
                    InitButton("basic_padlock", lcdHoverTint, lcdPressedTint, lcdPressedTint, lcdHoverTint, lcdPressedTint);
                    CheckedMode = CheckedModes.SwitchCheckedStateOnClick;
                    clickedSound = GUIManager.BeepLCD; 
                    break;
                case ImageButtonType.LCDPadlockWhite:
                    InitButton("basic_padlock_white", lcdHoverTint, lcdPressedTint, lcdPressedTint, lcdHoverTint, lcdPressedTint);
                    CheckedMode = CheckedModes.SwitchCheckedStateOnClick;
                    clickedSound = GUIManager.BeepLCD; 
                    break;
                   
                /*  case ImageButtonType.LCDUrgent:
                      InitButton("lcd_rightarrow", "lcd_rightarrow_in", "lcd_rightarrow_hover"); // placeholder
                      //clickedSound = GUIManager.BeepLCD; // GUIManager.Bip1;
                      break;
                  case ImageButtonType.LCDNormal:
                      InitButton("lcd_rightarrow", "lcd_rightarrow_in", "lcd_rightarrow_hover"); // placeholder
                      //clickedSound = GUIManager.BeepLCD; // GUIManager.Bip1;
                      break;
                  case ImageButtonType.LCDHigh:
                      InitButton("lcd_rightarrow", "lcd_rightarrow_in", "lcd_rightarrow_hover"); // placeholder
                      //clickedSound = GUIManager.BeepLCD; // GUIManager.Bip1;
                      break;*/
                case ImageButtonType.LCDArrowDown:
                    InitButton("lcd_downarrow", "lcd_downarrow_in", "lcd_downarrow_hover");
                    clickedSound = GUIManager.BeepLCD; // GUIManager.Bip1;
                    break;

                case ImageButtonType.SiteMarker:
                    InitButton("map_button_roundSmall", "map_button_roundSmall", ImageButton.lcdHoverTint, ImageButton.lcdPressedTint);

                    break;
                case ImageButtonType.SiteMarkerTallPin:
                    InitButton("map_button_roundSmall_pin", "map_button_roundSmall_pin", ImageButton.lcdHoverTint, ImageButton.lcdPressedTint);

                    break;
                case ImageButtonType.SiteMarkerShortPin:
                    InitButton("map_button_roundSmall_pinShort", "map_button_roundSmall_pinShort", ImageButton.lcdHoverTint, ImageButton.lcdPressedTint); //was   InitButton("map_button_roundSmall_pinShort", "map_button_roundSmall_pinShort"

                    break;
               
                case ImageButtonType.LCDCheckbox:

                    InitButton("basic_checkbox_unselected", "basic_checkbox_selected", ImageButton.lcdHoverTint, ImageButton.lcdPressedTint, disabledSprite: "basic_checkbox_disabled");

                  /*  rect = GUIManager.GUISpriteSheet.GetSourceRectangle("basic_checkbox_disabled");
                    SetSkinLocation(SkinState.Disabled, rect);*/

                    break;
                case ImageButtonType.LCDArrowDownNew:
                    InitButton("basic_dropdown_arrow_down", lcdHoverTint, lcdPressedTint);
                    break;
               
                //LCD Tracking
                case ImageButtonType.LCDTracking:

                    rect = GUIManager.GUISpriteSheet.GetSourceRectangle("basic_button_light");
                    Width = rect.Width;
                    Height = rect.Height;
                    Skin = rect;

                   
                    SetSkinLocation(1, rect, TextButton.lcdTooltipHoverTint, TextButton.lcdTooltipHoverTint);

                    rect = GUIManager.GUISpriteSheet.GetSourceRectangle("basic_button_light_in");
                    PressedSkin = rect;

                    Rectangle iconRect = guiManager.GUISpriteSheet.GetSourceRectangle("basic_icon_crosshairs");

                    icon = new Icon(guiManager);

                    //We are using the normalStatusColorSidepanel's value from BaseDataLoader.InitPresentationTypes()
                    Microsoft.Xna.Framework.Color color = new Color(58, 113, 119);
                    icon.SetSkinLocation(SkinState.Normal,iconRect, color, color);                   
                    icon.CurrentSkin = 0;
                    icon.Visible = false;                 
                    Add(icon);                  
                    icon.X = (Width - icon.Width) / 2;
                    icon.Y = (Height - icon.Height) / 2;
                    
                    icon.CanHaveFocus = false;
                  
                    clickedSound = GUIManager.BeepLCD;
                    break;                
                    
                case ImageButtonType.LCDList:
                    InitButton("basic_icon_list", hudHoverTint, hudPressedTint);
                    clickedSound = GUIManager.BeepLCD;
                    break;

                case ImageButtonType.HUDIncrease:
                    InitButton("HUD_spinner_increase", hudHoverTint, hudPressedTint);
                    clickedSound = GUIManager.BeepLCD;
                    break;
                case ImageButtonType.HUDDecrease:
                    InitButton("HUD_spinner_decrease", hudHoverTint, hudPressedTint);
                    clickedSound = GUIManager.BeepLCD;
                    break;
                case ImageButtonType.HUDCycleEntity:
                    InitButton("HUD_button_cycle", hudHoverTint, hudPressedTint);
                    clickedSound = GUIManager.BeepLCD;
                    break;
                case ImageButtonType.HUDModifyZone:
                    InitButton("HUD_button_changeLayout", hudHoverTint, hudPressedTint);
                    clickedSound = GUIManager.BeepLCD;
                    break;
                case ImageButtonType.HUDDelete:
                    InitButton("HUD_button_trash", hudHoverTint, hudPressedTint);
                    clickedSound = GUIManager.BeepLCD;
                    break;
                case ImageButtonType.HUDBuild:
                    InitButton("HUD_button_hammer", hudHoverTint, hudPressedTint);
                    clickedSound = GUIManager.BeepLCD;
                    break;
                case ImageButtonType.HUDSalvage:
                    InitButton("HUD_button_recycleArrows", hudHoverTint, hudPressedTint);                    
                    clickedSound = GUIManager.BeepLCD;
                    break;
                case ImageButtonType.HUDPackDown:
                    InitButton("HUD_button_closedBox", hudHoverTint, hudPressedTint);
                    clickedSound = GUIManager.BeepLCD;
                    break;
                case ImageButtonType.HUDDiscard:
                    InitButton("HUD_button_palmSlanted", hudHoverTint, hudPressedTint);
                    clickedSound = GUIManager.BeepLCD;
                    break;
                case ImageButtonType.HUDClaim:
                    InitButton("HUD_button_handGrabbing", hudHoverTint, hudPressedTint);
                    clickedSound = GUIManager.BeepLCD;
                    break;
                case ImageButtonType.HUDPrices:
                    InitButton("HUD_button_coins", hudHoverTint, hudPressedTint);
                    break;
                case ImageButtonType.HUDPeople:
                    InitButton("HUD_button_people", hudHoverTint, hudPressedTint);
                    break;
                case ImageButtonType.HUDShowProductionInfo:
                   // InitButton("HUD_info_button_production", "HUD_info_button_production_hover",  hudHoverTint, hudCheckedTint);
                    InitButton("HUD_info_button_production", "HUD_info_button_production_in", "HUD_info_button_production_hover", checkedHoverTint: hudHoverTint);
                   
                    clickedSound = null; // GUIManager.BeepLCD;
                    break;
                case ImageButtonType.HUDShowGeneralInfo:
                    InitButton("HUD_info_button_data", "HUD_info_button_data_hover", hudHoverTint, hudCheckedTint);
                    // InitButton("HUD_button_cycle", hudHoverTint, hudPressedTint, hudCheckedTint, hudCheckedHoverTint, hudCheckedPressedTint);

                    clickedSound = null; // GUIManager.BeepLCD;
                    break;
                case ImageButtonType.HUDClose:
                    InitButton("HUD_button_close", hudHoverTint, hudPressedTint, hudCheckedTint, hudCheckedHoverTint, hudCheckedPressedTint);
                    clickedSound = null; // GUIManager.BeepLCD;
                    checkedMode = CheckedModes.CannotBeChecked;
                    break;
                case ImageButtonType.HUDCrosshair:
                    InitButton("HUD_button_crosshairs", hudHoverTint, hudPressedTint, hudCheckedTint, hudCheckedHoverTint, hudCheckedPressedTint);                 
                    clickedSound = null;
                    break;
                case ImageButtonType.HUDPin:
                    InitButton("HUD_button_pushPin", hudHoverTint, hudPressedTint, hudCheckedTint, hudCheckedHoverTint, hudCheckedPressedTint);
                    clickedSound = null;
                    break;
                case ImageButtonType.HUDPadlock:
                    InitButton("HUD_padlock", hudHoverTint, hudPressedTint, hudCheckedTint, hudCheckedHoverTint, hudCheckedPressedTint);                 
                //    InitButton("HUD_padlock_grey", hudHoverTint, hudPressedTint, hudCheckedTint, hudCheckedHoverTint, hudCheckedPressedTint);                 
                    CheckedMode = CheckedModes.SwitchCheckedStateOnClick;
                    clickedSound = null;
                    break;

                case ImageButtonType.HUDInfoScrollUp:
                    InitButton("HUD_lineScroller_up", hudHoverTint, hudPressedTint, hudCheckedTint, hudCheckedHoverTint, hudCheckedPressedTint);
                    clickedSound = null;
                    break;
                case ImageButtonType.HUDInfoScrollDown:
                    InitButton("HUD_lineScroller_down", hudHoverTint, hudPressedTint, hudCheckedTint, hudCheckedHoverTint, hudCheckedPressedTint);
                    clickedSound = null;
                    break;
                case ImageButtonType.HUDCheckbox:
                    InitButton("HUD_checkbox_empty", "HUD_checkbox_filled", hudHoverTint, hudPressedTint, hasDisabledState: true);
                    ScaleImageToSizeOfControl = false;
                    break;
                case ImageButtonType.HUDRadioButton:
                    InitButton("HUD_radio_empty", "HUD_radio_filled", hudHoverTint, hudPressedTint, hasDisabledState: true);
                    ScaleImageToSizeOfControl = false;
                    break;
                case ImageButtonType.HUDExpandCollapseTinted:
                    // works best when NormalColor is set also                 
                    InitButton("HUD_rightarrow", "HUD_downarrow", Microsoft.Xna.Framework.Color.Gray /* hudHoverTint*/, pressedTint: hudPressedTint, modulateHoverColor: true);

                    ScaleImageToSizeOfControl = false;
                    clickedSound = GUIManager.BeepLCD;
                    DebugTag = "HUD_rightarrow";
                    break;
                case ImageButtonType.HUDExpandArrowDown:
                    InitButton("HUD_downarrow", hudHoverTint, hudPressedTint);
                    clickedSound = GUIManager.BeepLCD;
                    break;
                case ImageButtonType.HUDArrowRight:
                    InitButton("HUD_rightarrow", hudHoverTint, hudPressedTint);

                    //  rect = guiManager.GUISpriteSheet.SourceRectangle("HUD_rightarrow"); // test only
                    //  SetSkinLocation(SkinState.Normal, rect, hudHoverTint, hudHoverTint);

                    clickedSound = GUIManager.BeepLCD;
                    DebugTag = "HUD_rightarrow";
                    break;
                case ImageButtonType.HUDArrowDown:
                    InitButton("HUD_downarrow", hudHoverTint, hudPressedTint);
                    //clickedSound = GUIManager.BeepLCD;
                    break;
                case ImageButtonType.HUDArrowUp:
                    InitButton("HUD_uparrow", hudHoverTint, hudPressedTint);
                    //clickedSound = GUIManager.BeepLCD;
                    break;

                case ImageButtonType.MetalPanel:
                    rect = guiManager.GUISpriteSheet.GetSourceRectangle(string.Format("metal_button_{0}_out", flavour));
                    Skin = rect;

                    rect = guiManager.GUISpriteSheet.GetSourceRectangle(string.Format("metal_button_{0}_hover", flavour));
                    SetSkinLocation(SkinState.Hover, rect);

                    Width = rect.Width;
                    Height = rect.Height;

                    rect = GUIManager.GUISpriteSheet.GetSourceRectangle(string.Format("metal_button_{0}_in", flavour));
                    PressedSkin = rect;
                    SetSkinLocation(SkinState.Checked, rect);
                    SetSkinLocation(SkinState.CheckedHover, rect);
                    SetSkinLocation(SkinState.CheckedPressed, rect);

                    clickedSound = GUIManager.BeepMetalPanel; // GUIManager.Clack;
                    break;
                case ImageButtonType.MetalPanelHorizontal:
                    // same as above:
                    rect = guiManager.GUISpriteSheet.GetSourceRectangle(string.Format("metal_button_{0}horiz_out", flavour));
                    Skin = rect;

                    rect = guiManager.GUISpriteSheet.GetSourceRectangle(string.Format("metal_button_{0}horiz_hover", flavour));
                    SetSkinLocation(SkinState.Hover, rect);

                    Width = rect.Width;
                    Height = rect.Height;

                    rect = GUIManager.GUISpriteSheet.GetSourceRectangle(string.Format("metal_button_{0}horiz_in", flavour));
                    PressedSkin = rect;
                    SetSkinLocation(SkinState.Checked, rect);
                    SetSkinLocation(SkinState.CheckedHover, rect);
                    SetSkinLocation(SkinState.CheckedPressed, rect);

                    clickedSound = GUIManager.BeepMetalPanel; // GUIManager.Clack;
                    break;
                //arrowblue_down
                case ImageButtonType.LCDExpandWithUpAndDownArrows:
                    rect = GUIManager.GUISpriteSheet.GetSourceRectangle("basic_button_light");
                    Width = rect.Width;
                    Height = rect.Height;
                    Skin = rect;

                   
                    SetSkinLocation(1, rect, TextButton.lcdTooltipHoverTint, TextButton.lcdTooltipHoverTint);

                    rect = GUIManager.GUISpriteSheet.GetSourceRectangle("basic_button_light_in");
                    SetSkinLocation(SkinState.CheckedPressed, rect, TextButton.lcdTooltipHoverTint, TextButton.lcdTooltipHoverTint);
                    PressedSkin = rect;
                   
                    
                    iconRect = guiManager.GUISpriteSheet.GetSourceRectangle("arrowblue_down");

                    icon = new Icon(guiManager);

                    //We are using the normalStatusColorSidepanel's value from BaseDataLoader.InitPresentationTypes()
                    color = new Color(58, 113, 119);

                    icon.SetSkinLocation(SkinState.Normal,iconRect, color, color);
                    iconRect = guiManager.GUISpriteSheet.GetSourceRectangle("arrowblue_up");
                    icon.SetSkinLocation(1, iconRect, color, color);
                    icon.ResizeControlToFitImage();
                    icon.CurrentSkin = 0;
                  //  icon.Visible = false;
                 
                    Add(icon);
                   
                    icon.X = (Width - icon.Width) / 2;
                    icon.Y = (Height - icon.Height) / 2;
                    //icon.Y = icon.Y +1;
                    icon.CanHaveFocus = false;
                   
                    clickedSound = GUIManager.BeepBasicPanel;

                    break;  
                case ImageButtonType.LCDSortingArrows:
                    rect = GUIManager.GUISpriteSheet.GetSourceRectangle("basic_button_light");
                    Width = rect.Width;
                    Height = rect.Height;
                    Skin = rect;
                  
                    SetSkinLocation(1, rect, TextButton.lcdTooltipHoverTint, TextButton.lcdTooltipHoverTint);                   

                    rect = GUIManager.GUISpriteSheet.GetSourceRectangle("basic_button_light_in"); 
                    PressedSkin = rect;
                    SetSkinLocation(SkinState.Pressed, rect);
                    SetSkinLocation(SkinState.CheckedPressed, rect, TextButton.lcdTooltipHoverTint, TextButton.lcdTooltipHoverTint);
                   
                    iconRect = guiManager.GUISpriteSheet.GetSourceRectangle("basic_icon_sortingArrow_down");

                    icon = new Icon(guiManager);
                    icon.SetSkinLocation(SkinState.Normal,iconRect);
                    iconRect = guiManager.GUISpriteSheet.GetSourceRectangle("basic_icon_sortingArrow_up");
                    icon.SetSkinLocation(1, iconRect);
                    icon.ResizeControlToFitImage();
                    icon.CurrentSkin = 0;
                    icon.Visible = false;
                    Add(icon);                

                    icon.X = (Width - icon.Width) / 2;
                    icon.Y = (Height - icon.Height) / 2;
                    
                    icon.CanHaveFocus = false;
                  

                    clickedSound = GUIManager.BeepBasicPanel;

                    break;               
               /* case ImageButtonType.ArrowBlue_Right:
                    InitButton("arrowblue_right", hudHoverTint, hudPressedTint, new Color(0, 0, 0));
                    //clickedSound = GUIManager.BeepLCD;
                    break;
                case ImageButtonType.ArrowBlue_Left:
                    InitButton("arrowblue_left", hudHoverTint, hudPressedTint, new Color(0, 0, 0));
                    //clickedSound = GUIManager.BeepLCD;
                    break;*/




                /*    rect = guiManager.GUISpriteSheet.SourceRectangle(string.Format("metal_button_{0}_out", flavour));
                    Skin = rect;

                    rect = guiManager.GUISpriteSheet.SourceRectangle(string.Format("metal_button_{0}_hover", flavour));
                    SetSkinLocation(SkinState.Hover, rect);

                    Width = rect.Height; // !!
                    Height = rect.Width; // !!
                    ScaleImageToSizeOfControl = false;

                    rect = GUIManager.GUISpriteSheet.SourceRectangle(string.Format("metal_button_{0}_in", flavour));
                    PressedSkin = rect;
                    SetSkinLocation(SkinState.Checked, rect);
                    SetSkinLocation(SkinState.CheckedHover, rect);
                    SetSkinLocation(SkinState.CheckedPressed, rect);

                    clickedSound = GUIManager.BeepMetalPanel; // GUIManager.Clack;

                    // rotate:
                   // Rotation = MathHelper.PiOver2;
                    Rotate90Degrees = true;
                    //Origin = new Vector2(Width / 2f, Height / 2f);
                    */


            }

        }

        /// <summary>
        /// type should be the base graphic, the icon graphic is iconSprite
        /// </summary>
        /// <param name="type"></param>
        /// <param name="iconSprite"></param>
        /// <param name="hasCheckedState"></param>
        public void InitWithIcon(ImageButtonType type, string iconSprite, bool hasCheckedState, Color? iconTint = null)
        {
            if (!hasCheckedState)
            {
                CheckedMode = CheckedModes.CannotBeChecked;
            }

            Rectangle rect;
            switch (type)
            {
                case ImageButtonType.Counter:
                    Init(ImageButtonType.Counter);

                    break;

                case ImageButtonType.White:
                    rect = GUIManager.GUISpriteSheet.GetSourceRectangle("basic_buttonwhite_out");
                    Width = rect.Width;
                    Height = rect.Height;
                    Skin = rect;
                    rect = GUIManager.GUISpriteSheet.GetSourceRectangle("basic_buttonwhite_hover");
                    HoverSkin = rect;
                    rect = GUIManager.GUISpriteSheet.GetSourceRectangle("basic_buttonwhite_in");
                    PressedSkin = rect;

                    clickedSound = GUIManager.BeepBasicPanel;

                    break;

                case ImageButtonType.Black:
                    rect = GUIManager.GUISpriteSheet.GetSourceRectangle("smallblackbutton_out");
                    Width = rect.Width;
                    Height = rect.Height;
                    Skin = rect;
                    rect = GUIManager.GUISpriteSheet.GetSourceRectangle("smallblackbutton_hover");
                    HoverSkin = rect;
                    rect = GUIManager.GUISpriteSheet.GetSourceRectangle("smallblackbutton_in");
                    PressedSkin = rect;

                    clickedSound = GUIManager.BeepBasicPanel;

                    break;               
                                   
                case ImageButtonType.SiteMarker:
                    InitButton("map_button_roundSmall", "map_button_roundSmall", ImageButton.lcdHoverTint, ImageButton.lcdPressedTint);                  
                    break;

                case ImageButtonType.SiteMarkerTallPin:
                    InitButton("map_button_roundSmall_pin", "map_button_roundSmall_pin", ImageButton.lcdHoverTint, ImageButton.lcdPressedTint);
                    iconOffSetYPos = -1; //mp negative is up

                    break;
                case ImageButtonType.SiteMarkerShortPin:
                    InitButton("map_button_roundSmall_pinShort", "map_button_roundSmall_pinShort", ImageButton.lcdHoverTint, ImageButton.lcdPressedTint);
                    iconOffSetYPos = -1; // 2;

                    break;

                case ImageButtonType.LCD:
                    if (iconSprite == "basic_icon_crosshairs")
                    {
                    }

                    rect = GUIManager.GUISpriteSheet.GetSourceRectangle("basic_button_light"); 
                    Width = rect.Width;
                    Height = rect.Height;
                    Skin = rect;

                    SetSkinLocation(1, rect, TextButton.lcdTooltipHoverTint, TextButton.lcdTooltipHoverTint);

                    if (!hasCheckedState)
                    {
                        SetSkinLocation(SkinState.Checked, rect);
                        SetSkinLocation(SkinState.CheckedPressed, rect, TextButton.lcdTooltipHoverTint, TextButton.lcdTooltipHoverTint);
                        SetSkinLocation(SkinState.CheckedHover, rect, TextButton.lcdTooltipCheckedHoverTint, TextButton.lcdTooltipCheckedHoverTint);         
                    }

                    rect = GUIManager.GUISpriteSheet.GetSourceRectangle("basic_button_light_in"); 
                    PressedSkin = rect;
                    SetSkinLocation(SkinState.Pressed, rect);

                    if (hasCheckedState)
                    {
                        SetSkinLocation(SkinState.Checked, rect);
                        SetSkinLocation(SkinState.CheckedPressed, rect, TextButton.lcdTooltipHoverTint, TextButton.lcdTooltipHoverTint);
                        SetSkinLocation(SkinState.CheckedHover, rect, TextButton.lcdTooltipCheckedHoverTint, TextButton.lcdTooltipCheckedHoverTint);                   
                    }

                    rect = GUIManager.GUISpriteSheet.GetSourceRectangle("basic_button_light_disabled"); 
                    SetSkinLocation(SkinState.Disabled, rect); // NEW
           
                    clickedSound = GUIManager.BeepBasicPanel;                   

                    break;

                case ImageButtonType.HUD:

                    rect = GUIManager.GUISpriteSheet.GetSourceRectangle("HUD_button_wide");

                    Width = rect.Width;
                    Height = rect.Height;
                    Skin = rect;
                   
                    SetSkinLocation(SkinState.Hover, rect, hudHoverTint, hudHoverTint);
                    SetSkinLocation(SkinState.Pressed, rect, hudPressedTint, hudPressedTint);
                    SetSkinLocation(SkinState.Checked, rect, hudCheckedTint, hudCheckedTint);
                    SetSkinLocation(SkinState.CheckedPressed, rect, hudCheckedPressedTint, hudCheckedPressedTint);
                    SetSkinLocation(SkinState.CheckedHover, rect, hudHoverTint, hudHoverTint);
                    SetSkinLocation(SkinState.HoverDisabled, rect, hudHoverTintNonEnabled, hudHoverTintNonEnabled);
                    rect = GUIManager.GUISpriteSheet.GetSourceRectangle("HUD_button_wide_disabled");
                    SetSkinLocation(SkinState.Disabled, rect); // NEW
                    
                    clickedSound = GUIManager.BeepBasicPanel;

                    break;            
            }

            icon = new Icon(guiManager);
            Add(this.icon);
            icon.CanHaveFocus = false;
            rect = guiManager.GUISpriteSheet.GetSourceRectangle(iconSprite);
            icon.SetSkinLocation(SkinState.Normal,rect, iconTint, iconTint);
            icon.ResizeControlToFitImage();

            RecalculateIconPosition();
        }

        public void RecalculateIconPosition()
        {
            if (icon != null)
            {
                if (iconOffSetXPos.HasValue)
                {
                    icon.X = iconOffSetXPos.Value;
                }
                else
                {
                    icon.X = (Width - icon.Width) / 2;
                }

                if (iconOffSetYPos.HasValue)
                {
                    icon.Y = iconOffSetYPos.Value;
                }
                else
                {
                    icon.Y = (Height - icon.Height) / 2;
                }
            }
        }

        public void SetIconSkinState(int stateIndex)
        {
            icon.CurrentSkin = stateIndex;
        }

        public void SetIconTint(Color color)
        {
            icon.Color = color;
            icon.Visible = true;
        }

        public void SetIconTooltip(string toolTip)
        {
            icon.ToolTip = toolTip;
            icon.Visible = true;
        }
        public void SetIconClick(ClickHandler clickEvent)
        {
            icon.Click += clickEvent;
            icon.Tag1 = Tag1;
        }

              

      /*  public void InitHUD(Rectangle rect)
        {
            Skin = rect;
            // buttonBar.SetSkinLocation(SkinState.Normal, rect, ImageButton.hudHoverTint, ImageButton.hudHoverTint); // test only

            SetSkinLocation(SkinState.Hover, rect, ImageButton.hudHoverTint, ImageButton.hudHoverTint);
            SetSkinLocation(SkinState.Pressed, rect, ImageButton.hudPressedTint, ImageButton.hudPressedTint);
            SetSkinLocation(SkinState.Checked, rect, ImageButton.hudCheckedTint, ImageButton.hudCheckedTint);
            SetSkinLocation(SkinState.CheckedPressed, rect, ImageButton.hudCheckedPressedTint, ImageButton.hudCheckedPressedTint);
            SetSkinLocation(SkinState.CheckedHover, rect, ImageButton.hudHoverTint, ImageButton.hudHoverTint);
            //  HoverSkin = rect;


            // Color = Color.White;

        }*/


        private void InitButton(string normalSprite, string inSprite, string hoverSprite, string checkedHoverSprite = null, Color? checkedHoverTint = null)
        {
            enabledNormalSkin = normalSprite;

            Rectangle rect = guiManager.GUISpriteSheet.GetSourceRectangle(normalSprite);
            Skin = rect;
           
            Width = rect.Width;
            Height = rect.Height;

            if (hoverSprite != null)
            {
                rect = guiManager.GUISpriteSheet.GetSourceRectangle(hoverSprite);
            }
         
            SetSkinLocation(SkinState.Hover, rect);

            if (checkedHoverSprite != null)
            {
                rect = guiManager.GUISpriteSheet.GetSourceRectangle(checkedHoverSprite);
            }

           // SetSkinLocation(SkinState.CheckedHover, rect);         
            SetSkinLocation(SkinState.CheckedHover, rect, checkedHoverTint, checkedHoverTint);         

            rect = GUIManager.GUISpriteSheet.GetSourceRectangle(inSprite);
            PressedSkin = rect;
           
            SetSkinLocation(SkinState.Checked, rect);
            SetSkinLocation(SkinState.CheckedPressed, rect);
        }

        public void InitButton(string normalSprite, string checkedSprite, Color hoverTint, Color pressedTint, Color? normalTint = null, bool modulateHoverColor = false, string disabledSprite = null, bool hasDisabledState = false) 
        {
            enabledNormalSkin = normalSprite;
            enabledColor = normalTint; 

            Rectangle rect = guiManager.GUISpriteSheet.GetSourceRectangle(normalSprite);
            Rectangle checkedRect = guiManager.GUISpriteSheet.GetSourceRectangle(checkedSprite);
            if (normalTint == null)
            {
                Skin = rect;
                SetSkinLocation(SkinState.Checked, checkedRect);
            }
            else
            {
                SetSkinLocation(SkinState.Normal, rect, normalTint.Value, normalTint.Value);
                SetSkinLocation(SkinState.Checked, checkedRect, normalTint.Value, normalTint.Value);
            }

            SetSkinLocation(SkinState.Hover, rect, hoverTint, hoverTint, modulateColor: modulateHoverColor);
            SetSkinLocation(SkinState.Pressed, rect, pressedTint, pressedTint);

            if (disabledSprite != null) // cannot be combined with checked sprite...
            {
                Rectangle disabledRect = guiManager.GUISpriteSheet.GetSourceRectangle(disabledSprite);
                SetSkinLocation(SkinState.Disabled, disabledRect);
                SetSkinLocation(SkinState.HoverDisabled, disabledRect, hoverTint, hoverTint, modulateColor: modulateHoverColor);

                // Checked: Since we don't have sprites for these, re-use the disabled ones:
                SetSkinLocation(SkinState.CheckedDisabled, disabledRect); // NEW 
                SetSkinLocation(SkinState.CheckedDisabledHover, disabledRect, hoverTint, hoverTint, modulateColor: modulateHoverColor); // NEW             


            }
            else if (hasDisabledState)
            {
                SetSkinLocation(SkinState.Disabled, rect, lcdDisabledColor, lcdDisabledColor);
                SetSkinLocation(SkinState.HoverDisabled, rect, lcdHoverDisabledColor, lcdHoverDisabledColor);                 

                SetSkinLocation(SkinState.CheckedDisabled, checkedRect, lcdDisabledColor, lcdDisabledColor);
                SetSkinLocation(SkinState.CheckedDisabledHover, checkedRect, lcdHoverDisabledColor, lcdHoverDisabledColor);                 
            }
            
            SetSkinLocation(SkinState.CheckedHover, checkedRect, hoverTint, hoverTint, modulateColor: modulateHoverColor);
            SetSkinLocation(SkinState.CheckedPressed, checkedRect, pressedTint, pressedTint);
            
                       
            Width = rect.Width;
            Height = rect.Height;           
        }

        public void InitButton(string normalSprite, string checkedSprite, Color hoverTint, Color? normalTint = null)
        {
            enabledNormalSkin = normalSprite;
            enabledColor = normalTint; // store this...

            Rectangle rect = guiManager.GUISpriteSheet.GetSourceRectangle(normalSprite);
            Rectangle checkedRect = guiManager.GUISpriteSheet.GetSourceRectangle(checkedSprite);
            if (normalTint == null)
            {
                Skin = rect;
                SetSkinLocation(SkinState.Checked, checkedRect);
            }
            else
            {
                SetSkinLocation(SkinState.Normal, rect, normalTint.Value, normalTint.Value);
                SetSkinLocation(SkinState.Checked, checkedRect, normalTint.Value, normalTint.Value);
            }

            SetSkinLocation(SkinState.Hover, rect, hoverTint, hoverTint);
            SetSkinLocation(SkinState.Pressed, checkedRect);


            SetSkinLocation(SkinState.CheckedHover, checkedRect, hoverTint, hoverTint);
            SetSkinLocation(SkinState.CheckedPressed, checkedRect);
            //  SetSkinLocation(SkinState.CheckedHover, checkedRect); //??


            Width = rect.Width;
            Height = rect.Height;
        }

        public void InitButton(string normalSprite, Color hoverTint, Color pressedTint, Color? normalTint = null, bool hasDisabledState = true) //, Color? disabledTint = null) //, string barSprite = null) // string inSprite, string hoverSprite)
        {
            enabledNormalSkin = normalSprite;
            enabledColor = normalTint; // store this...

            Rectangle rect = guiManager.GUISpriteSheet.GetSourceRectangle(normalSprite);

            if (normalTint == null)
            {
                SetSkinLocation(SkinState.Normal, rect);
                SetSkinLocation(SkinState.Checked, rect);
            }
            else
            {
                SetSkinLocation(SkinState.Normal, rect, normalTint.Value, normalTint.Value);
                SetSkinLocation(SkinState.Checked, rect, normalTint.Value, normalTint.Value);              
            }
               
            
            SetSkinLocation(SkinState.CheckedHover, rect, hoverTint, hoverTint);
            SetSkinLocation(SkinState.Hover, rect, hoverTint, hoverTint);                       

            SetSkinLocation(SkinState.Pressed, rect, pressedTint, pressedTint);
            SetSkinLocation(SkinState.CheckedPressed, rect, pressedTint, pressedTint);

            if (hasDisabledState)
            {
                SetSkinLocation(SkinState.Disabled, rect, lcdDisabledColor, lcdDisabledColor);
                SetSkinLocation(SkinState.HoverDisabled, rect, lcdHoverDisabledColor, lcdHoverDisabledColor); // NEW                 
            }
       

            Width = rect.Width;
            Height = rect.Height;
        }


        public void InitButton(string normalSprite, Color hoverTint, Color pressedTint, Color checkedTint, Color checkedHoverTint, Color checkedPressedTint) // string inSprite, string hoverSprite)
        {
            enabledNormalSkin = normalSprite;

            Rectangle rect = guiManager.GUISpriteSheet.GetSourceRectangle(normalSprite);
            Skin = rect;
            SetSkinLocation(SkinState.Hover, rect, hoverTint, hoverTint);
            SetSkinLocation(SkinState.Pressed, rect, pressedTint, pressedTint);

            SetSkinLocation(SkinState.Checked, rect, checkedTint, null);
            SetSkinLocation(SkinState.CheckedHover, rect, checkedHoverTint, checkedHoverTint);
            SetSkinLocation(SkinState.CheckedPressed, rect, checkedPressedTint, checkedPressedTint);


            Width = rect.Width;
            Height = rect.Height;
        }

        #region Event Handlers
        /// <summary>
        /// Updates button skin to reflect current mouse state.
        /// </summary>
        /// <param name="args">Mouse event arguments.</param>
        protected override void OnMouseOver(UIComponent sender, MouseEventArgs args)
        {
            base.OnMouseOver(sender, args);

            if (DebugTag == "cbStockpile")
            {

            }

            if (Enabled == false)
            {
                if (string.IsNullOrEmpty(ToolTip))
                {
                    return;
                }
                else
                {
                    if (CheckedMode != CheckedModes.CannotBeChecked)
                    {
                        if (IsChecked)
                        {
                            CurrentSkinState = SkinState.CheckedDisabledHover;
                        }
                        else
                        {
                            CurrentSkinState = SkinState.HoverDisabled;
                        }
                    }
                    else
                    {
                        CurrentSkinState = SkinState.HoverDisabled;
                    }
                    
                    base.OnMouseOver(sender, args);
                    return;
                }
            }
            else 
            {
                if (IsPressed)
                {
                    CurrentSkinState = SkinState.CheckedPressed;                   
                }
                else
                {
                    CurrentSkinState = SkinState.Hover;

                    if (this.isChecked)
                        CurrentSkinState = SkinState.CheckedHover;                   
                }
            }
        }

        /// <summary>
        /// Updates button skin to reflect current mouse state.
        /// </summary>
        /// <param name="args">Mouse event arguments.</param>
        protected override void OnMouseOut(UIComponent sender, MouseEventArgs args)
        {
            // base.OnMouseOut(sender, args); // don't call Icon... we account for all cases here.
            guiManager.HideToolTip(this); // call this directly - HACK?


            if (Enabled)
            {
                if (CurrentSkinState == SkinState.Hover || CurrentSkinState == SkinState.CheckedHover) // allow click-disabled controls to get their normal state
                {
                    if (IsPressed)
                    {
                        CurrentSkinState = SkinState.Pressed;
                    }
                    else if (IsChecked)
                    {
                        CurrentSkinState = SkinState.Checked;
                    }
                    else
                    {
                        CurrentSkinState = SkinState.Normal;
                    }

                    /*
                    CurrentSkinState = SkinState.Normal;

                    if (isChecked)
                    {
                        CurrentSkinState = SkinState.Checked;
                    }

                    if (IsPressed)
                    {
                        CurrentSkinState = SkinState.Pressed;
                    }*/
                }
            }
            else 
            {
                if (IsChecked)
                {
                    CurrentSkinState = SkinState.CheckedDisabled;
                }
                else
                {
                    CurrentSkinState = SkinState.Disabled;
                }

                /*
                if (CurrentSkinState == SkinState.HoverDisabled)
                {
                    CurrentSkinState = SkinState.Disabled;
                }
                else if (CurrentSkinState == SkinState.CheckedDisabledHover)
                {
                    CurrentSkinState = SkinState.CheckedDisabled;
                }*/
            }
        }

        /// <summary>
        /// Updates button skin to reflect current mouse state.
        /// </summary>
        /// <param name="args">Mouse event arguments.</param>
        protected override void OnMouseDown(MouseEventArgs args)
        {

            base.OnMouseDown(args);

            if (args.Button == MouseButtons.Left)
            {
                CurrentSkinState = SkinState.Pressed;

                if (this.isChecked)
                    CurrentSkinState = SkinState.CheckedPressed;

                if (clickedSound != null)
                {
                    GUIManager.PlaySound(clickedSound);
                }
            }

            if (RightClickEnabled && args.Button == MouseButtons.Right)
            {
                CurrentSkinState = SkinState.Pressed;

                if (this.isChecked)
                    CurrentSkinState = SkinState.CheckedPressed;

                if (clickedSound != null)
                {
                    GUIManager.PlaySound(clickedSound);
                }
            }

        }

        protected override void OnLoseFocus()
        {
            base.OnLoseFocus();

            // when going modal, try to reset the state
            // (copied in TextButton)
           /* if (Enabled)
            {
                if (this.isChecked)
                {
                    CurrentSkinState = SkinState.Checked;
                }
                else
                {
                    CurrentSkinState = SkinState.Normal;
                }
            }
            else
            {
                CurrentSkinState = SkinState.Disabled; b
            }*/


            UpdateSkinState(false);

        }

        /// <summary>
        /// Updates button skin to reflect current mouse state.
        /// </summary>
        /// <param name="args">Mouse event arguments.</param>
        protected override void OnMouseUp(MouseEventArgs args)
        {
            if (args.Button == MouseButtons.Left)
            {
                if (CheckCoordinates(args.Position.X, args.Position.Y))
                {
                    if (checkedMode == CheckedModes.CanBeChecked && isChecked == false)
                    {
                        this.isChecked = true;
                    }
                    else if (checkedMode == CheckedModes.SwitchCheckedStateOnClick)
                    {
                        this.isChecked = !this.isChecked;
                    }

                  /*  if (SwitchStateOnClick || isChecked == false)
                    {
                        this.isChecked = !this.isChecked;
                    }*/
                }
            }

            base.OnMouseUp(args);

            if (args.Button == MouseButtons.Left)
            {
                // Check that the mouse was depressed inside the button
                if (CheckCoordinates(args.Position.X, args.Position.Y))
                {
                   /* if (SwitchStateOnClick || isChecked == false)
                    {
                        this.isChecked = !this.isChecked;
                    }*/

                    CurrentSkinState = SkinState.Hover;

                    if (IsPressed)
                    {
                        CurrentSkinState = SkinState.Pressed;
                    }

                    if (this.isChecked)
                        CurrentSkinState = SkinState.CheckedHover;                   
                }
                else
                {
                    CurrentSkinState = SkinState.Normal;                    

                    if (IsPressed)
                    {
                        CurrentSkinState = SkinState.Pressed;
                    }

                    if (this.isChecked)
                        CurrentSkinState = SkinState.Checked;
                }
               
            }

            

        }

       /* protected override void OnResize(UIComponent sender)
        {
            base.OnResize(sender);

            if (this.buttonBar != null)
            {
                this.buttonBar.Width = Width;
                this.buttonBar.Height = Height;
            }

            if (this.label != null && type != TextButtonType.Hyperlink)
                RefreshLabelPosition();
        }*/


        #endregion
    }
}