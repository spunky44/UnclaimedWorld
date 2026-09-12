using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.SimSide.AI.Constants
{
    public class Combat
    {

        // Combat
        public float DistanceToleranceInMeleeCombat = 10;

        public int MaxTakersForVerminThreatJob = 2;

        public int MaxTakersForThreatJob = 8;

        public float JobTakersPerStrengthRatio = 3f;


        /// <summary>
        /// The range used for waking agents up that are sleeping nearby a fight.
        /// </summary>
        public float CombatWakeUpRange = 1000f;

    }
}
