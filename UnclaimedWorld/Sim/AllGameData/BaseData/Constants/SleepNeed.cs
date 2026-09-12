using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.SimSide.AllGameData.Constants
{
    public class SleepNeed // see also Constants.cs
    {
        /// <summary>
        /// physical need gain modifiers
        /// </summary>
        public float GainPerDayWhenSleeping = 12f;

        public float MaxLimitForPeopleSleepingInOpen = 0.94f;
        public float GainFactorForPeopleSleepingInOpen = 0.8f;

        public float MaxLimitForPeopleSleepingNearCampfire = 0.97f;
        public float GainFactorForPeopleSleepingNearCampfire = 0.9f;

        public float MaximumSleepNeedGainFactorForPeople = 1.1f;


    }
}
