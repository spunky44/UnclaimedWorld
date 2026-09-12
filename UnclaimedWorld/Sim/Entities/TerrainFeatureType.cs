using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.SimSide.Entities
{
    public class TerrainFeatureType
    {
        //public bool IsPath = false;

        public PathType PathType;

        public bool CanBeMapEditorPlaced = false;

        /// <summary>
        /// True makes it show up in the overlay panel
        /// </summary>
        public bool IsSpecialInterestFeature = false;
    }
}
