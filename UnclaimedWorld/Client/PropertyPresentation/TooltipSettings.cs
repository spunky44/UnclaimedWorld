using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.ClientSide.PropertyPresentation
{
    public class TooltipSettings
    {
        /// <summary>
        /// can be used to define a property to set when the tooltip is (de)activated.
        /// </summary>
        public string TooltipActivationProperty;

        public bool? DisableExpiry;

        public int? TooltipWidth;


    }
}
