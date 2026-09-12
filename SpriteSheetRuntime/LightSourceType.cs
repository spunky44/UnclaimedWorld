#region File Description
//-----------------------------------------------------------------------------
// SpriteSheetContent.cs
//
// Microsoft Game Technology Group
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System.Collections.Generic;
using Microsoft.Xna.Framework;
#endregion

namespace SpriteSheetRuntime
{
    /// <summary>
    /// TODO: delete this - use LightingType
    /// </summary>
    public class LightSourceType
    {
        public string SpriteName;

      //  private Point offset, offsetFlipped;

        /// <summary>
        /// Call GetOffset!
        /// </summary>
        public Point Offset;
    /*    {
            set { offset = value; }
        }*/
        /// <summary>
        /// Call GetOffset!
        /// </summary>
        public Point OffsetFlipped;
     /*   {
            set { offsetFlipped = value; }
        }*/
               

        public Point GetOffset(bool flipHorizontally)
        {
            if (flipHorizontally)
            {
                return OffsetFlipped;
            }
            else return Offset;
        }

       // public Rectangle SpriteRectangle;
        public bool IsIndoor;
    }
}
