using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.SimSide.Overland.Locations
{
    public static class Double
    {
        public static double ToDegrees(this double d)
        {
            return d * (180d / Math.PI);
        }

        public static double ToRadians(this double d)
        {
            return d * (Math.PI / 180d);
        }
    }
}
