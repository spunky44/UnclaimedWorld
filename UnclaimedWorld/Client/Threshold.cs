using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities.Biological;

namespace UWGame.ClientSide
{
    public class Threshold: IEdge
    {
        /// <summary>
        /// the upper edge 
        /// </summary>
        public float Edge { get; set; }

        /// <summary>
        /// a text string - can be null
        /// </summary>
        public string Term;

        /// <summary>
        /// a text string, will be displayed when Term is hovered. - can be null
        /// 
        /// Will be combined with TextFormatting, if that is also specified.
        /// 
        /// 2014-10-07:
        /// Set the default value to "No tooltip available." as it was requested that all sidepanel items should have tooltips   
        /// </summary>
        public string TermTooltip;


        /// <summary>
        /// an image - can be null
        /// The icon is placed before the caption!
        /// </summary>
        public string Icon;

        /// <summary>
        /// a color tint - can be null
        /// </summary>
        public Color? IconTint;


        public Color? TermTint;

        public bool UseValueTextFormatting = true;
        public bool UseValueTooltipFormatting = true;

    }
}
