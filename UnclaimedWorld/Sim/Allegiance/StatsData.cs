using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.InGameEvents.Expressions;

namespace UWGame.SimSide.Allegiances
{
    public class StatsData
    {
        public float? Security = 1;
        public float? Comfort = 1;
        public float? FoodSupply = 1;

        public NormalDistribution RandomSecurity;
        public NormalDistribution RandomComfort;
        public NormalDistribution RandomFood;


        public EvalNode DynamicFood;
        public EvalNode DynamicComfort;
        public EvalNode DynamicSecurity;

        public float GetRating(RatingTypes rating)
        {
            switch(rating)
            {
                case RatingTypes.Comfort:
                    return GetValue(Comfort, RandomComfort, DynamicComfort);
                  
                case RatingTypes.Food:
                    return GetValue(FoodSupply, RandomFood, DynamicFood);
                   
                case RatingTypes.Security:
                    return GetValue(Security, RandomSecurity, DynamicSecurity);
                   
            }

            return 0f;
        }

        private static float GetValue(float? staticValue, NormalDistribution randomValue, EvalNode dynamicValue)
        {
            if (dynamicValue != null)
            {
                PropertyResult? result = dynamicValue.Evaluate(null, null, null, null);
                if (result.HasValue)
                {
                    return result.Value.NumberResult.Value;
                }

                return 0f;
            }
            else if (randomValue != null)
            {
                return (float)randomValue.GetRandomValue(The.Sim.GameplayRandomGenerator, true);
            }
            else return staticValue ?? 0f;
        }

       
    }
}
