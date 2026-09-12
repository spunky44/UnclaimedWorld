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
    public enum AddonSize { Tiny, Small, Big }
    public class BuildingTypeData
    {
        public Point BaseCenter;

        public int Width;
        public int Height;

        public int WidthInTiles;
        public int HeightInTiles;

        // doesn't work with ? and null... use -1 instead
        /// <summary>
        /// in pixels
        /// </summary>
        public Point FrontDoorRelativeLocation = new Point(-1, -1); 
        public Point BackDoorRelativeLocation = new Point(-1, -1);

        public Dictionary<AddonSize, List<Point>> AddonSlots; // = new Dictionary<AddonSize, List<Point>>() { { AddonSize.Tiny, new List<Point>() }, { AddonSize.Small, new List<Point>() }, {AddonSize.Big, new List<Point>()} };
       

        public List<Point> WindowPositions;

        public byte[] DiscomfortValues;

        public Dictionary<string, Point> LightSourceOffsets;


        public void CreateAddonSlotsIfNotExist()
        {
            if (AddonSlots == null)
            {
                AddonSlots = new Dictionary<AddonSize, List<Point>>() { { AddonSize.Tiny, new List<Point>() }, { AddonSize.Small, new List<Point>() }, { AddonSize.Big, new List<Point>() } };
            }
       
        }
    }
}
