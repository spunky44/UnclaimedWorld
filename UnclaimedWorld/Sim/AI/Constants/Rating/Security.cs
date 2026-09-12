using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.SimSide.AI.Constants.Rating
{
    public class Security
    {
        public float DaysForInjuriesToAffect = 3f;
        public float DaysForDeathsToAffect = 3f;

        public float DeathFactor = 1f;
        public float InjuryFactor = 0.1f;

        public float MaxHandWeaponsToScorePerMember = 2f;

        public float MinimumNoOfAttacksForWeaponToCount = 3f; //amount of shots stockpiled for the weapon to count

      //  public float DefenseRatingValue = 0.05f;

     

    }
}
