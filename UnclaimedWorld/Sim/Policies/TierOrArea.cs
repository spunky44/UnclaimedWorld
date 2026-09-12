using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Allegiances.Statistics;

namespace UWGame.SimSide.Policies
{
    public class TierOrArea
    {
        /// <summary>
        /// required
        /// </summary>
        public string Tier;

        /// <summary>
        /// optional
        /// </summary>
        public RatingTypes? /*string*/ Area;

    }
}
