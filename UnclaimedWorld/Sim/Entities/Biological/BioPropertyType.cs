using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.SimSide.Entities.Biological
{
    /// <summary>
    /// defines how different bioproperty values should be combined into one result.
    /// </summary>
    public class BioPropertyType
    {
        public string KeyName;

        /// <summary>
        /// DefaultPriority means age overrides race, which again overrides caste
        /// </summary>
        public enum Interpolate { DefaultPriority, Average, Max, Min }

        /// <summary>
        /// DefaultPriority means age overrides race, which again overrides caste
        /// </summary>
        public Interpolate InterpolateSetting = Interpolate.DefaultPriority;

    }
}
