#region File Description
//-----------------------------------------------------------------------------
// SpriteSheet.cs
//
// Microsoft Game Technology Group
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

namespace SpriteSheetRuntime
{
    /// <summary>
    /// A sprite sheet contains many individual sprite images, packed into different
    /// areas of a single larger texture, along with information describing where in
    /// that texture each sprite is located. Sprite sheets can make your game drawing
    /// more efficient, because they reduce the number of times the graphics hardware
    /// needs to switch from one texture to another.
    /// </summary>
    public class ExtendedSpriteSheet: SpriteSheet
    {
        public Texture2D NormalTexture;

    
      //  public Dictionary<string, TreeTypeData> TreeTypeData;

      //  public Dictionary<string, BuildingTypeData> BuildingTypeData = new Dictionary<string, BuildingTypeData>();
       

        /// <summary>
        /// The constructor is internal: this should only be
        /// called by the SpriteSheetReader support class.
        /// </summary>
     /*   internal ExtendedSpriteSheet(ContentReader input)
        {
            texture = input.ReadObject<Texture2D>();
            
            spriteRectangles = input.ReadObject<Rectangle[]>();
            spriteNames = input.ReadObject<Dictionary<string, int>>();
        }*/

        /* NOT USED???
        protected virtual internal void ReadContent(ContentReader input)
        {
            base.ReadContent(input);

            NormalTexture = input.ReadObject<Texture2D>();
            UtilityTexture = input.ReadObject<Texture2D>();
        }
         * 
         * */

     /*   /// <summary>
        /// Gets the single large texture used by this sprite sheet.
        /// </summary>
        public Texture2D NormalTexture
        {
            get { return normalTexture; }
        }

        /// <summary>
        /// Gets the single large texture used by this sprite sheet.
        /// </summary>
        public Texture2D UtilityTexture
        {
            get { return utilityTexture; }
        }*/


    }
}
