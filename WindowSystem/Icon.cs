#region File Description
//-----------------------------------------------------------------------------
// File:      Icon.cs
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
using Microsoft.Xna.Framework.Graphics;
using InputEventSystem;
using System.Text;
#endregion

namespace WindowSystem
{
    /// <summary>
    /// A static image from the GUI texture. Can have different skin states. Can be scaled to fit the control
    /// size.
    /// </summary>
    public class Icon : SkinnedComponent
    {
        #region Fields
        private bool scaleImageToSizeOfControl;
        private float alpha = 1f;

     //   private float rotation = 0f;
     //   private Vector2 rotationOrigin = Vector2.Zero;

        private Color? color = null; // Color.White;
        #endregion

        #region Properties
        /// <summary>
        /// Get/Set whether the image be scaled to control size.
        /// </summary>
        public bool ScaleImageToSizeOfControl
        {
            get { return this.scaleImageToSizeOfControl; }
            set
            {
                this.scaleImageToSizeOfControl = value;
                RefreshSkins();
            }
        }

        public float Alpha
        {
            get { return alpha; }
            set 
            { 
                alpha = value;
                // update skin rects with new alpha value:
                RefreshSkins();

            }
        }

     /*   public float Rotation
        {
            get { return rotation; }
            set
            {
                rotation = value;
                // update skin rects with value:
                RefreshSkins();

            }
        }

        public Vector2 Origin
        {
            get { return rotationOrigin; }
            set
            {
                rotationOrigin = value;
                // update skin rects with value:
                RefreshSkins();

            }
        }
        */

        /// <summary>
        /// Modulating color! (Why not let all skinned components have this?)
        /// </summary>
        public Color? Color
        {
            get { return color; }
            set
            {
                color = value;
                // update skin rects with new color value:
                RefreshSkins();

            }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game">The currently running Game object.</param>
        /// <param name="guiManager">GUIManager that this control is part of.</param>
        public Icon(GUIManager guiManager)
            : base(guiManager)
        {
            this.scaleImageToSizeOfControl = false;
        }
        #endregion

        #region Event Handlers

        /// <summary>
        /// NEW: see if these break anything...
        /// 
        /// Updates button skin to reflect current mouse state.
        /// </summary>
        /// <param name="args">Mouse event arguments.</param>
        protected override void OnMouseOver(UIComponent sender, MouseEventArgs args)
        {
            if (SetMouseOverState())
            {
                base.OnMouseOver(sender, args);
            }           
        }

        public bool SetMouseOverState()
        {
            if (Enabled == false && string.IsNullOrEmpty(ToolTip))
            {
                return false;
            }
            else if (string.IsNullOrEmpty(ToolTip))
            {
                return false; // NEW: don't set the default hover state on bg images without a tooltip...
            }
            else if (Enabled == false)
            {
                CurrentSkinState = SkinState.HoverDisabled;
            }
            else
            {
                CurrentSkinState = SkinState.Hover;
            }

            return true;
        }

        /// <summary>
        /// Updates button skin to reflect current mouse state.
        /// </summary>
        /// <param name="args">Mouse event arguments.</param>
        protected override void OnMouseOut(UIComponent sender, MouseEventArgs args)
        {            
            if (SetMouseOutState())
            {
                base.OnMouseOut(sender, args);
            }
        }

        public bool SetMouseOutState()
        {
            if (Enabled == false && string.IsNullOrEmpty(ToolTip))
            {
                return false;
            }

            if (Enabled == false)
            {
                CurrentSkinState = SkinState.Disabled;
            }
            else
            {
                CurrentSkinState = SkinState.Normal;
            }

            return true;

        }

        #endregion


        public enum UIType { HUD, LCD }
        public void SetSkinLocations(Rectangle location, UIType uiType)
        {
            Color hoverColor;
            if (uiType == UIType.HUD)
            {
                hoverColor = Microsoft.Xna.Framework.Color.Gray; // ImageButton.hudHoverTint;
            }
            else
            {
                hoverColor = ImageButton.lcdHoverTint;
            }

            SetSkinLocation(SkinState.Normal, location, null /* normalColor*/, null /* normalColor*/, false, false);
            SetSkinLocation(SkinState.Hover, location, hoverColor, hoverColor, false, true); // false);
        }

        /// <summary>
        /// disables hover color if hover param is null!
        /// </summary>
        /// <param name="location"></param>
        /// <param name="normalColor"></param>
        /// <param name="hoverColor"></param>
        public void SetSkinLocations(Rectangle location, Color? normalColor = null, Color? hoverColor = null)
        {
            SetSkinLocation(SkinState.Normal, location, normalColor, normalColor, false, false);
            SetSkinLocation(SkinState.Hover, location, hoverColor, hoverColor, false, false);
        }

        public override void SetSkinLocation(int index, Rectangle? location, Color? edgeColor = null, Color? centerColor = null, bool flipHorizontally = false, bool modulateColor = false)
        {
            base.SetSkinLocation(index, location, edgeColor, centerColor, flipHorizontally, modulateColor);

            // sets a default hover state that modulates the current color and darkens it. 
            // if other colors are wanted, this can be overridden
            if (index == 0 && !Skins.ContainsKey((int)SkinState.Hover))
            {
                SetSkinLocation(SkinState.Hover, location, Microsoft.Xna.Framework.Color.Gray, Microsoft.Xna.Framework.Color.Gray, flipHorizontally, true);
            }
        }



        /// <summary>
        /// Resizes control to fit icon skin, as long as icon isn't scaling.
        /// </summary>
        public void ResizeControlToFitImage()
        {
            if (!this.scaleImageToSizeOfControl)
            {
                int currentSkin = CurrentSkin;

                if (currentSkin != -1)
                {
                    Rectangle source = GetSkinLocation(currentSkin);

                    if (source.Width > 0 && source.Height > 0)
                    {
                        Width = source.Width;
                        Height = source.Height;
                    }
                }
            }
        }

        /// <summary>
        /// Update skin sizes.
        /// </summary>
        protected override void RefreshSkins()
        {
            // Get the current list of skins
            Dictionary<int, ComponentSkin> skins = Skins;

            foreach (KeyValuePair<int, ComponentSkin> skin in skins)
            {
                GUIRect rect = new GUIRect();
                rect.Source = GetSkinLocation(skin.Key);
                rect.Alpha = Alpha;
                rect.FlipHorizontally = skin.Value.FlipHorizontally;

                if (skin.Value.ModulateColor)
                {
                    Color? colorToModulateWith = skin.Value.CenterColor ?? skin.Value.EdgeColor;
                    if (colorToModulateWith.HasValue && Color.HasValue)
                    {
                        // make sure that the hover tint is not overwritten:
                        rect.Color = new Microsoft.Xna.Framework.Color(colorToModulateWith.Value.ToVector4() * Color.Value.ToVector4());
                    }
                    else 
                    {
                        rect.Color = Color ?? skin.Value.CenterColor ?? skin.Value.EdgeColor ?? Microsoft.Xna.Framework.Color.White;                        
                    }
           
                }
                else
                {
                    rect.Color = Color ?? skin.Value.CenterColor ?? skin.Value.EdgeColor ?? Microsoft.Xna.Framework.Color.White;
                }    

              /*  rect.Rotation = Rotation;
                rect.Origin = Origin;*/

                if (this.scaleImageToSizeOfControl)
                {
                    if (Rotate90Degrees)
                    {
                        rect.Destination = new Rectangle(0, 0, Height, Width);
                    }
                    else
                    {
                        rect.Destination = new Rectangle(0, 0, Width, Height);
                    }
                }
                else
                {
                    if (Rotate90Degrees)
                    {
                        rect.Destination = new Rectangle(0, 0, rect.Source.Height, rect.Source.Width);
                    }
                    else
                    {
                        rect.Destination = new Rectangle(0, 0, rect.Source.Width, rect.Source.Height);
                    }
                }

                skin.Value.Rects.Clear();
                skin.Value.Rects.Add(rect);
            }

            Redraw();
        }

        public static string ToIcon(string sprite, string color)
        {
            // §C#00FF00¤True§
            StringBuilder text = new StringBuilder();
            text.Append("§I"); // means 'Icon'
            text.Append(color); // .ToString());
            text.Append("¤");

            text.Append(sprite);

            text.Append("§");

            return text.ToString();
        }
    }
}