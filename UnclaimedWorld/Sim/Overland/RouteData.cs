using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Overland.Locations;

namespace UWGame.SimSide.Overland
{
    /// <summary>
    /// this will create a 2-way route between sites
    /// </summary>
    public class RouteData
    {
        public string Name;

        public string FromSite;
        public string ToSite;

        public RouteType RouteType;

        public float Length;

    }
}
