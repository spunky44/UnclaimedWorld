using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Maps;

namespace UWGame.SimSide.AI.Planners
{
    /// <summary>
    /// store what we know about a region here
    /// 
    /// how do we save knowledge when regions are destroyed?
    /// 
    /// Tie to terrain instead of threat maps since it will be less volatile.
    /// 
    /// </summary>
    public class RegionKnowledge
    {
     //   public RegionID RegionID;

        public float Threat;

        public float Allies;

        public float Resources;

    }
}
