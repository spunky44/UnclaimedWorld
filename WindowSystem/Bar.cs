#region File Description
//-----------------------------------------------------------------------------
// File:      Bar.cs
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
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System;
using InputEventSystem;
#endregion

namespace WindowSystem
{
    /// <summary>
    /// A graphical bar drawn with two end sprites, and a stretched middle
    /// sprite.
    /// </summary>
    /// <remarks>
    /// Inherited from SkinnedComponent to allow arbitrary number of skin
    /// states. For example, the Scrollbar class uses a Bar to draw the thumb
    /// with three states, normal, hover, and pressed.
    /// </remarks>
    public class Bar : SkinnedComponent
    {
        #region Default Properties
        private static int defaultEdgeSize = 5;

        /// <summary>
        /// Sets the default size of the edge of the bar skin.
        /// </summary>
        /// <value>Must be at least 0.</value>
        public static int DefaultEdgeSize
        {
            set
            {
                Debug.Assert(value >= 0);
                defaultEdgeSize = value;
            }
        }
        #endregion

        #region Fields
        private int edgeSize;
        private bool isVertical;
        #endregion

        public Rectangle? UnderSprite;
        public int UnderSpriteY = 0;

        #region Properties
        /// <summary>
        /// Get/Set whether the bar is vertical or horizontal.
        /// </summary>
        /// <value>TRUE if bar is vertical, otherwise FALSE.</value>
        public bool IsVertical
        {
            get { return isVertical; }
            set
            {
                this.isVertical = value;
                RefreshSkins();
            }
        }

        /// <summary>
        /// Get/Set the size of the edge of the bar skin.
        /// </summary>
        /// <value>Must be at least 0.</value>
        public int EdgeSize
        {
            get { return edgeSize; }
            set
            {
                Debug.Assert(value >= 0);

                this.edgeSize = value;
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
        public Bar(GUIManager guiManager)
            : base(guiManager)
        {
            this.isVertical = false;
            CanHaveFocus = false;

            #region Set Default Properties
            EdgeSize = defaultEdgeSize;
            #endregion
        }
        #endregion

        /// <summary>
        /// Update skin sizes.
        /// </summary>
        protected override void RefreshSkins()
        {
            // Get the current list of skins
            Dictionary<int, ComponentSkin> skins = this.Skins;

       
            foreach (KeyValuePair<int, ComponentSkin> skin in skins)
            {
                skin.Value.Rects = CreateBar(
                    GetSkinLocation(skin.Key),
                    new Rectangle(0, 0, Width, Height),
                    this.edgeSize,
                    this.isVertical,
                    UnderSprite,
                    UnderSpriteY,
                    skin.Value.EdgeColor //, skin.Value.CenterColor
                );
            }

            Redraw();
        }

        protected override void OnMouseOut(UIComponent sender, MouseEventArgs args)
        {
            base.OnMouseOut(sender, args);

            if (Enabled)
            {
                CurrentSkinState = SkinState.Normal;                
            }
        }

        protected override void OnMouseOver(UIComponent sender, MouseEventArgs args)
        {
            base.OnMouseOver(sender, args);

            if (Enabled)
            {
                if (IsPressed)
                {
                    CurrentSkinState = SkinState.CheckedPressed;
                }
                else
                {
                    CurrentSkinState = SkinState.Hover;                                       
                }
            }
        }

        protected static List<GUIRect> CreateBar(
            Rectangle source,
            Rectangle dimensions,
            int edgeSize,
            bool isVertical, Rectangle? underSprite, int underSpriteY, Color? color = null
            )
        {
            List<GUIRect> result = new List<GUIRect>();

            GUIRect[] sprites;
            int spriteIndex = 0;
            if (underSprite.HasValue)
            {
                sprites = new GUIRect[4];
            }
            else
            {
                sprites = new GUIRect[3];
            }

            GUIRect guiRect;

            //Color edgeColorToUse

            if (isVertical)
            {
                if (underSprite.HasValue)
                {
                    sprites[spriteIndex] = new GUIRect();
           
                    sprites[spriteIndex].Source = new Rectangle(underSprite.Value.X, underSprite.Value.Y, underSprite.Value.Width, dimensions.Height - (edgeSize)); //underSprite.Value.Height);
                    sprites[spriteIndex].Destination = new Rectangle(dimensions.X, dimensions.Y + underSpriteY, underSprite.Value.Width, dimensions.Height - (edgeSize)); //underSprite.Value.Height);
                    
                    spriteIndex++;
                }

                // Top
                guiRect = new GUIRect();
                guiRect.Source = new Rectangle(source.X, source.Y, source.Width, edgeSize);
                guiRect.Destination = new Rectangle(dimensions.X, dimensions.Y, dimensions.Width, edgeSize);
                if (color.HasValue)
                {
                    guiRect.Color = color.Value;
                }
                sprites[spriteIndex] = guiRect;

                // Middle
                guiRect = new GUIRect();
                guiRect.Source = new Rectangle(source.X, source.Y + edgeSize, source.Width, source.Height - (2 * edgeSize));
                guiRect.Destination = new Rectangle(dimensions.X, dimensions.Y + edgeSize, dimensions.Width, dimensions.Height - (2 * edgeSize));
                if (color.HasValue)
                {
                    guiRect.Color = color.Value;
                }
                sprites[spriteIndex + 1] = guiRect;

                // Bottom            
                guiRect = new GUIRect();
                guiRect.Source = new Rectangle(source.X, source.Bottom - edgeSize, source.Width, edgeSize);
                guiRect.Destination = new Rectangle(dimensions.X, dimensions.Y + dimensions.Height - edgeSize, dimensions.Width, edgeSize);
                if (color.HasValue)
                {
                    guiRect.Color = color.Value;
                }
                sprites[spriteIndex + 2] = guiRect;
            }
            else
            {
                // Left
                guiRect = new GUIRect();               
                guiRect.Source = new Rectangle(source.X, source.Y, edgeSize, source.Height);
                guiRect.Destination = new Rectangle(dimensions.X, dimensions.Y, edgeSize, dimensions.Height);
                if (color.HasValue)
                {
                    guiRect.Color = color.Value;
                }
                sprites[spriteIndex] = guiRect;

                // Middle
                int middlePiece = source.Width - (2 * edgeSize);
              /*  if (middlePiece < 1)
                    throw new Exception("Source rectangle is too small!");
                */

                guiRect = new GUIRect();
                guiRect.Source = new Rectangle(source.X + edgeSize, source.Y, middlePiece, source.Height);
                guiRect.Destination = new Rectangle(dimensions.X + edgeSize, dimensions.Y, dimensions.Width - (2 * edgeSize), dimensions.Height);
                if (color.HasValue)
                {
                    guiRect.Color = color.Value;
                }
                sprites[spriteIndex + 1] = guiRect;

                // Right
                guiRect = new GUIRect();
                guiRect.Source = new Rectangle(source.Right - edgeSize, source.Y, edgeSize, source.Height);
                guiRect.Destination = new Rectangle(dimensions.Right - edgeSize, dimensions.Y, edgeSize, dimensions.Height);
                if (color.HasValue)
                {
                    guiRect.Color = color.Value;
                }
                sprites[spriteIndex + 2] = guiRect;

            }

            // Copy over the results
            for (int i = 0; i < sprites.Length; i++)
                result.Add(sprites[i]);

            return result;
        }
    }
}