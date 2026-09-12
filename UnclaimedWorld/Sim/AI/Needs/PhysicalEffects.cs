using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Items;
using UWGame.SimSide.Entities;
using System.Xml.Serialization;
using UWGame.SimSide.AllGameData;

namespace UWGame.SimSide.AI.Needs
{
    /// <summary>
    /// represents a physical need that can cause sickness and stunted growth when it is not met
    /// </summary>
    public class PhysicalEffects
    {
        public float LimitForIncreasedSickness; // = 0.3f;
        public float SicknessFactor = 1f;

        public float LimitForReducedGrowth; // = 0.1f;
        public float GrowthReductionFactor = 1f;

        //public bool IsEssential = true; 
        

        public float? LimitForWeightReduction = null;
        public bool ShouldSerializeLimitForWeightReduction()
        {
            return LimitForWeightReduction != null;
        }

        public float? LimitForWeightIncrease = null;
        public bool ShouldSerializeLimitForWeightIncrease()
        {
            return LimitForWeightIncrease != null;
        }

        public float? DaysAtZeroCausingDeath = null;
        public bool ShouldSerializeDaysAtZeroCausingDeath()
        {
            return DaysAtZeroCausingDeath != null;
        }

        public float? DaysAtZeroCausingCollapse = null;
        public bool ShouldSerializeDaysAtZeroCausingCollapse()
        {
            return DaysAtZeroCausingCollapse != null;
        }

        /// <summary>
        /// the factor that starvation days is decreased by when we are no longer starving
        /// </summary>
        public float? DaysAtZeroDecreaseFactor = 1f; //0.5f;
        public bool ShouldSerializeDaysAtZeroDecreaseFactor()
        {
            return DaysAtZeroDecreaseFactor != null;
        }

        public bool UseExertionFactorToDecrease = false;

        

    }
}
