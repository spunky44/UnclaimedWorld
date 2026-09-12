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
    /// A push button comprised of a graphical Bar component, with an icon
    /// on top. Copy of TextButton.
    /// </summary>
    public class ImageBarButton : UIComponent, ICanBeChecked
    {
        public enum ImageBarButtonType { Brown }

        #region Default Properties
        private static int defaultWidth = 65;
        private static int defaultHeight = 25;
        private static int defaultEdgeSize = 12;
        private static Rectangle defaultSkin = new Rectangle(1, 142, 25, 25);
        private static Rectangle defaultHoverSkin = new Rectangle(27, 142, 25, 25);
        private static Rectangle defaultPressedSkin = new Rectangle(52, 142, 25, 25);

       // private static string commButtonFont = "Content/Fonts/Newtown";

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
        protected Bar buttonBar;
        private Icon icon;


        protected ImageBarButtonType type;

        #endregion

        private SoundEffect clickedSound;

        protected bool hasState = false;
        protected bool isChecked = false;

       // private int xLabelOffset = 0;

        /// <summary>
        /// include shadows in this value.
        /// </summary>
        private int rightButtonPadding = 2;       
        private int leftButtonPadding = 2;

        #region Properties
        /// <summary>
        /// Sets the control skin.
        /// </summary>
        public Rectangle Skin
        {
            set { this.buttonBar.SetSkinLocation(SkinState.Normal,value); }
        }

        /// <summary>
        /// Sets the control hover skin.
        /// </summary>
        public Rectangle HoverSkin
        {
            set { this.buttonBar.SetSkinLocation(1, value); }
        }

        /// <summary>
        /// Sets the control pressed skin.
        /// </summary>
        public Rectangle PressedSkin
        {
            set { this.buttonBar.SetSkinLocation(2, value); }
        }

        /// <summary>
        /// Get/Set size of left and right parts of the button.
        /// </summary>
        /// <value>Must be at least 0.</value>
        public int EdgeSize
        {
            get { return this.buttonBar.EdgeSize; }
            set { this.buttonBar.EdgeSize = value; }
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
        public ImageBarButton(GUIManager guiManager)
            : base(guiManager)
        {
            #region Create Child Controls
            this.buttonBar = new Bar(guiManager);
          
            #endregion

            #region Add Child Controls
            Add(this.buttonBar);
         
            #endregion

            #region Set Properties
            this.buttonBar.IsVertical = false;
            MinWidth = defaultHeight;
            MinHeight = defaultHeight;
            #endregion

        
            
        }
        #endregion

      /*  public void Init(ImageBarButtonType type)
        {
            Init(type, -1);
        }*/

        public void Init(ImageBarButtonType type, string iconSprite)
        {
            this.type = type;
            Rectangle rect;
            switch (type)
            {

                case ImageBarButtonType.Brown:
                    rect = GUIManager.GUISpriteSheet.GetSourceRectangle("basic_buttondarkblue_out"); //"basic_button_out");
                    Width = rect.Width;
                    Height = rect.Height;
                    EdgeSize = 14;
                    Skin = rect;
                    rect = GUIManager.GUISpriteSheet.GetSourceRectangle("basic_buttondarkblue_hover");
                    HoverSkin = rect;
                    rect = GUIManager.GUISpriteSheet.GetSourceRectangle("basic_buttondarkblue_in"); //"basic_button_in");
                    PressedSkin = rect;
                  
                    clickedSound = GUIManager.BeepBasicPanel;
                    break;
              
               
            }

            icon = new Icon(guiManager);
            Add(this.icon);
            icon.CanHaveFocus = false;
            rect = guiManager.GUISpriteSheet.GetSourceRectangle(iconSprite);
            icon.SetSkinLocation(SkinState.Normal,rect);
            icon.ResizeControlToFitImage();
            icon.X = (Width - icon.Width) / 2;
            icon.Y = (Height - icon.Height) / 2;

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

      

       

        #region Event Handlers
        /// <summary>
        /// Updates button skin to reflect current mouse state.
        /// </summary>
        /// <param name="args">Mouse event arguments.</param>
        protected override void OnMouseOver(UIComponent sender, MouseEventArgs args)
        {
            base.OnMouseOver(sender, args);

           
            if (IsPressed)
            {
                this.buttonBar.CurrentSkinState = SkinState.Pressed;

                if (hasState)
                {
                    if (this.isChecked)
                        this.buttonBar.CurrentSkinState = SkinState.CheckedPressed;
                }
            }
            else
            {
                this.buttonBar.CurrentSkinState = SkinState.Hover;

                if (hasState)
                {
                    if (this.isChecked)
                        this.buttonBar.CurrentSkinState = SkinState.CheckedHover;
                }
            }
        }

        /// <summary>
        /// Updates button skin to reflect current mouse state.
        /// </summary>
        /// <param name="args">Mouse event arguments.</param>
        protected override void OnMouseOut(UIComponent sender, MouseEventArgs args)
        {
            base.OnMouseOut(sender, args);
            this.buttonBar.CurrentSkinState = SkinState.Normal;

            if (hasState && this.isChecked)
                this.buttonBar.CurrentSkinState = SkinState.Checked;

          
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
                this.buttonBar.CurrentSkinState = SkinState.Pressed;
                             
                if (clickedSound != null)
                {
                    GUIManager.PlaySound(clickedSound);                          
                }
            }
        }

        /// <summary>
        /// Updates button skin to reflect current mouse state.
        /// </summary>
        /// <param name="args">Mouse event arguments.</param>
        protected override void OnMouseUp(MouseEventArgs args)
        {
            base.OnMouseUp(args);
            
            if (args.Button == MouseButtons.Left)
            {
                if (CheckCoordinates(args.Position.X, args.Position.Y))
                {
                    this.buttonBar.CurrentSkinState = SkinState.Hover;

                    if (hasState)
                    {
                        this.isChecked = !this.isChecked;

                        if (this.isChecked)
                            this.buttonBar.CurrentSkinState = SkinState.CheckedHover;
                    }
                }
                else
                {
                    this.buttonBar.CurrentSkinState = SkinState.Normal;

                    if (hasState)
                    {
                        if (this.isChecked)
                            this.buttonBar.CurrentSkinState = SkinState.Checked;
                    }
                }
                               
            }
        }

        /// <summary>
        /// Refresh child controls when control is resized.
        /// </summary>
        /// <param name="sender">Resized control.</param>
        protected override void OnResize(UIComponent sender)
        {
            base.OnResize(sender);

            if (this.buttonBar != null)
            {
                this.buttonBar.Width = Width;
                this.buttonBar.Height = Height;
            }

            if (this.icon != null)
            {
                icon.X = (Width - icon.Width) / 2;
                icon.Y = (Height - icon.Height) / 2;
            }

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
                isChecked = value;

                this.buttonBar.CurrentSkinState = SkinState.Normal;
                
                if (isChecked)
                    this.buttonBar.CurrentSkinState = SkinState.Checked;
            }
        }

        #endregion
    }
}