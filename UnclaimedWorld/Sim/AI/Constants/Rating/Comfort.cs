using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.SimSide.AI.Constants.Rating
{
    public class Comfort
    {
        public float MinimumHomeConditionToConsiderPerfect = 0.9f; // 0f; //was 0.9f  but we removed the decay effect on house comfort until we have the repair system.

    }
}
