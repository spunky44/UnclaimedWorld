using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.SimSide.AI.Constants.Rating
{
    public class Food
    {
        public float BaseScore = 0.1f;

        public float DaysForHungerDeathsToAffect = 3;
        public float HungerDeathRatingPenaltyFactor = 4;  // if the number is 4, this means a food rating hit of 100 % if one man out of 4 dies of hunger

        public float StockpiledFoodRating = 0.05f;

        public float StarvationPenaltyFactor = 1f;


    }
}
