using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.SimSide.Buildings
{
    /// <summary>
    /// NEW: only use for paths and roads.
    /// </summary>
    public class DirectionalLayoutType
    {       

        public bool CornerPlacement = false;

        /// <summary>
        /// blocks movement by all transportation.
        /// STERAIN: no longer used...? or use this for fences?
        /// </summary>
        public bool IsObstacle = false;

        public bool ConnectsToNeighbours = false;

        public bool Fixed8DirPlacement = false;

    }
}
