using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.InGameEvents.Expressions;

namespace UWGame.SimSide.Processes
{
    public class ProgressFactorProperty
    {
        public string InputType;
        public bool UseActingOnEntity;



        public string PropertyKey;
        /// <summary>
        /// substances can also be used to modify the process speed
        /// </summary>
        public string SubstanceKey;

        /// <summary>
        /// to be used in an itemized tooltip for how the final process speed is derived.
        /// "Humidity", "Wind"...
        /// </summary>
        public string TooltipCaption;

        /// <summary>
        /// makes it easier to create formulas...
        /// </summary>
        public float? ShiftByAmount;
        public float? ScaleByAmount;

        public EvalNode Factor;
    }
}
