using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.SimSide.AI.Constants
{
    public class Migration
    {
        public float MinimumMigrateRisk = 0.05f;
        public float MaximumMigrateRisk = 0.7f;

        public float DesirabilityGivingMaximumMigrateRisk = 1f;

        public float WeightOfPersonalTotal = 0.25f;

        public float WeightOfPersonalAdventurousness = 0.4f;
        public float WeightOfPersonalRandom = 0.6f;

    }
}
