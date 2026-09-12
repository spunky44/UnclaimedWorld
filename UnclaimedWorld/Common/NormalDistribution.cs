using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.Control.Replays;

namespace UWGame
{
    /// <summary>
    /// can fill in either mean + stdev, or min + max.
    /// If both sets are filled in, min/max are used for clamping the result from mean + stdev...
    /// </summary>
    public class NormalDistribution
    {
        public double? Mean;

        /// <summary>
        /// standard deviation is the deviation from the mean to each side - a max. spread divided by 3 gives the deviation value
        /// e.g. we want a max 'spread' of 0.10 to each side - divide by 3 and pass 0.03.     
        /// </summary>
        public double? StandardDeviation;

        public float? Max;
        public float? Min;


        /// <summary>
        /// standard deviation is the deviation from the mean to each side - a max. spread divided by 3 gives the deviation value
        /// e.g. we want a max 'spread' of 0.10 to each side - divide by 3 and pass 0.03.
        /// 
        /// One standard deviation away from the mean in either direction on the horizontal axis accounts for somewhere around 68 percent of the people in this group. 
        /// Two standard deviations away from the mean account for roughly 95 percent of the people. 
        /// And three standard deviations account for about 99 percent of the people.
        /// 
        /// For example, the average height for adult men in the United States is about 178 cm, with a standard deviation 
        /// of around 8 cm. This means that most men (about 68 percent, assuming a normal distribution) 
        /// have a height within 8 cm of the mean (170–185 cm), while almost all men (about 95%) have a height 
        /// within 15 cm of the mean (163–193 cm). If the standard deviation were zero, then all men would be 
        /// exactly 178 cm high. If the standard deviation were 51 cm, then men would have much more variable heights, 
        /// with a typical range of about 127 to 229 cm.
        /// </summary>
        /// <param name="generator"></param>
        /// <param name="mean"></param>
        /// <param name="stdDev"></param>
        /// <returns></returns>
        public double GetRandomValue(RandomGenerator generator, bool clampBetweenZeroAndOne = false)
        {
            double value;

            if (Mean.HasValue)
            {
                double stdDev = StandardDeviation ?? 0d;
                value = GetRandomValue(generator, Mean.Value, stdDev);
            }
            else if (Min.HasValue && Max.HasValue)
            {
                float? mean;
                float? stdDev;
                Common.GetNormalDistributionFromMinMaxValues(Min.Value, Max.Value, out mean, out stdDev);

                value = GetRandomValue(generator, mean.Value, stdDev.Value);
            }
            else return 0d; // error here.

            if (Max.HasValue)
            {
                value = Common.ClampTop(value, Max.Value);
            }

            if (Min.HasValue)
            {
                value = Common.ClampBottom(value, Min.Value);
            }

            if (clampBetweenZeroAndOne)
            {
                value = Common.Clamp(value, 0f, 1f);
            }

            return value;
        }


        public double GetMean()
        {
            if (!Mean.HasValue)
            {
                float? mean;
                float? stdDev;
                Common.GetNormalDistributionFromMinMaxValues(Min.Value, Max.Value, out mean, out stdDev);

                Mean = mean;
            }

            return Mean.Value;

        }

        public float GetMin()
        {
            if (!Min.HasValue)
            {
                Min = (float)(Mean.Value - 3 * StandardDeviation.Value);
            }

            return Min.Value;

        }

        public float GetMax()
        {
            if (!Max.HasValue)
            {
                Max = (float)(Mean.Value + 3 * StandardDeviation.Value);
            }

            return Max.Value;

        }

        public int GetRandomIntegerValue(RandomGenerator generator)
        {
            double value = GetRandomValue(generator);

            return (int)Math.Round(value);
        }



        /// <summary>
        /// standard deviation is the deviation from the mean to each side - a max. spread divided by 3 gives the deviation value
        /// e.g. we want a max 'spread' of 0.10 to each side - divide by 3 and pass 0.03.
        /// 
        /// One standard deviation away from the mean in either direction on the horizontal axis accounts for somewhere around 68 percent of the people in this group. 
        /// Two standard deviations away from the mean account for roughly 95 percent of the people. 
        /// And three standard deviations account for about 99 percent of the people.
        /// 
        /// For example, the average height for adult men in the United States is about 178 cm, with a standard deviation 
        /// of around 8 cm. This means that most men (about 68 percent, assuming a normal distribution) 
        /// have a height within 8 cm of the mean (170–185 cm), while almost all men (about 95%) have a height 
        /// within 15 cm of the mean (163–193 cm). If the standard deviation were zero, then all men would be 
        /// exactly 178 cm high. If the standard deviation were 51 cm, then men would have much more variable heights, 
        /// with a typical range of about 127 to 229 cm.
        /// </summary>
        /// <param name="generator"></param>
        /// <param name="mean"></param>
        /// <param name="stdDev"></param>
        /// <returns></returns>
        public static double GetRandomValue(RandomGenerator generator, double mean, double stdDev)
        {
            bool showMessage;
            if (generator.Type == RandomGenerator.GeneratorType.Sim)
            {
                showMessage = true;
            }
            else
            {
                showMessage = false;
            }
            double r1 = generator.NextDouble("Common - RandomNormalDistribution", showMessage);
            double r2 = generator.NextDouble("Common - RandomNormalDistribution", showMessage);

            return mean + (stdDev * (Math.Sqrt(-2 * Math.Log(r1)) * Math.Cos(6.28 * r2)));
        }
    }
}
