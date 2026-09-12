using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace UWGame.SimSide.Overland
{
    /// <summary>
    /// defines the state of the world on game startup (if not playing a campaign)
    /// </summary>
    public class WorldData
    {
      
        public double WorldRadius;

        /// <summary>
        /// 
        /// these are used to limit the view of the world in the interface. Longitude lines  (Width) are vertical and latitude lines (Height) are horizontal:
        /// </summary>
        public float ViewLatitudeStart, ViewLatitudeEnd, ViewLongitudeStart, ViewLongitudeEnd;
    }
}
