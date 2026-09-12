using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame
{
    public static class Units
    {
        public static string GetKilometersAsString(double distance)
        {           
            return distance.ToString("F2") + " KM";

        }
    }
}
