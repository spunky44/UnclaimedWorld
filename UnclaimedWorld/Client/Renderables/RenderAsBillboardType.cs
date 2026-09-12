using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace UWGame.ClientSide.Renderables
{
    public class RenderAsBillboardType 
    {
        public string AssetName
        {
            get;
            set;
        }

        public string AnimationAssetName;
        
        // could these be left out in a StaticConditionInfo..?
        /// <summary>
        /// This is mostly used to move the sprite relative to the ground sprite, in case of asymmetrical structures
        /// </summary>
        public Vector2 Offset;

        public float Bendyness;
        
        /// <summary>
        /// if no base center has been entered, the default is the center of the sprite.
        /// 
        /// Base center affects sorting and shadows, so should be as accurate as possible in order to improve visuals.
        /// 
        /// Changing this seems to affect placement of the sprite... it would be better if only Offset did that...
        /// </summary>
        public Vector2 BaseCenter;

        /// <summary>
        /// Used when rendering drop shadows. Make it a bit smaller rather than larger...
        /// </summary>
        public float AspectRatio = 1f;

        /// <summary>
        /// NEW: should be optional... could point to user bitmaps also?
        /// </summary>
        public string SpriteSheet;
    }
}
