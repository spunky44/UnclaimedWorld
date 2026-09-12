using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Snapshots;
using UWGame.Control.Replays;
using UWGame.SimSide.Allegiances;
using WindowSystem;
using UWGame.SimSide.SimEffects;

namespace UWGame.SimSide.Entities
{
    public class Personality : ISnapshot
    {
        public Entity Parent;

        public PersonalityType PersonalityType;

        public Dictionary<RatingTypes, float> Principles;

        public float Adaptability;

       
        /// <summary>
        /// can be set by the designer at start. 
        /// NEW: is removed after the agent chooses to emigrate away
        /// </summary>
        public Dictionary<AllegianceID, float> Attraction;


        public float Stability;

        private SimplexNoise stabilityNoise;

      
        float happiness;

        bool happinessIsDirty = true;

        string happinessBreakdown = null;

        RatingTypes? mostUnhappyRating = null;

        /// <summary>
        /// -1 - +1
        /// The difference between principles and Rating. Can be negative. unhappy-content-happy
        /// </summary>
        public float Happiness
        {
            get
            {
                if (happinessIsDirty)
                {
                    ComputeHappiness();
                    happinessIsDirty = false;

                    happinessBreakdown = null;
                }

                return happiness;
            }
        }

        public string HappinessBreakdown
        {
            get
            {
                if (happinessBreakdown == null)
                {
                    happinessBreakdown = GetHappinessBreakdown();
                }

                return happinessBreakdown;
            }
        }

        public RatingTypes? MostUnhappyRating
        {
            get
            {
                return mostUnhappyRating;
            }
        }

        public Personality()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor");
        }

        public Personality(Entity entity, PersonalityType type)
        {
            PersonalityType = type;
            this.Parent = entity;

            stabilityNoise = new SimplexNoise();

            /*
            stabilityRandomSeed = The.Sim.GameplayRandomGenerator.Next("volatilityRandomSeed");
            CreateStabilitySeedNumbers();*/

            PersonalityType.FillEntity(this);


        }

        public void Initialize()
        {
            Parent.Intelligence.Statistics.RatingsChanged += Statistics_RatingsChanged;

        }


        public RatingTypes GetHighestUnhappiness()
        {
            int noOfComponents = Principles.Count;

            float highestUnhappiness = 10f;
            RatingTypes? ratingType = null;

            foreach (var item in Principles)
            {
                float principle = item.Value;

                float rating = Parent.Intelligence.Statistics.GetRating(item.Key);

                float happiness = ComputeHappinessComponent(principle, rating);

                if (happiness < highestUnhappiness)
                {
                    highestUnhappiness = happiness;

                    ratingType = item.Key;
                }
            }

            return ratingType.Value;

        }

        public void SetPrinciplesToMinimum(RatingTypes rating, float minimum)
        {
            float current = Principles[rating];
            if (current < minimum)
            {
                Principles[rating] = minimum;
            }
        }

        private string GetHappinessBreakdown()
        {
            StringBuilder text = new StringBuilder();

            Common.AppendLine(text, "The person's satisfaction with their living situation.");
            text.Append("Happiness: ");
            text.Append(Common.PercentageToString(Happiness, useColoring: true));

            string term;
            if (Common.IsPositive(Common.ToPercent(Happiness))) // > 0)
            {
                term = "Happy";
            }
            else 
            {
                term = "Unhappy";
            }
           

            Common.AppendLine(text, string.Format(" ({0})", term));

            Common.AppendDivider(text);
            Common.AppendLine(text, "Based on:");
            Common.AppendLine(text);

            foreach (var item in Principles)
            {
                float rating;
                float principle = item.Value;

                Statistic.AppendRatingsTypeToStringAndIcon(text, item.Key);
             /*   text.Append(Icon.ToIcon(Statistic.RatingsTypeToIcon(item.Key), Statistic.RatingsTypeToColor(item.Key)));
                text.Append(Statistic.RatingsTypeToString(item.Key).ToUpper(Config.Culture));*/

                Common.AppendLine(text);

               // text.Append(Common.indentString);              
                text.Append("   Personal conditions: "); //was "Rating: "  
                rating = Parent.Intelligence.Statistics.GetRating(item.Key);
                text.Append(Common.PercentageToString(rating));
                Common.AppendLine(text);              
                text.Append("- Principles: ");
                // text.Append(Common.PercentageToString(foodPrinciple));
                Common.AppendLine(text, Common.PercentageToString(principle));
                text.Append("   Difference: ");
                Common.AppendLine(text, Common.PercentageToString(rating - principle, useColoring: true));
                Common.AppendLine(text);
            }

            text.Append("AVERAGE: ");
            Common.AppendLine(text, Common.PercentageToString(happiness, useColoring: true));


            return text.ToString();

        }

        /// <summary>
        /// controls participation in unhappiness group meetings
        /// leaders don't complain here...
        /// </summary>
        /// <param name="effectComponents"></param>
        /// <returns></returns>
        public bool CanComplainProperty(List<Tuple<string, bool>> effectComponents = null)
        {
            bool value = Parent.GetEffect(AffectsFlags.CanComplain, true, effectComponents: effectComponents);

            return value;
        }


        /// <summary>
        /// -1 - 1
        /// returns a person-specific number that fluctuates over time
        /// </summary>
        /// <returns></returns>
        public float GetCurrentStability()
        {
            float noise = stabilityNoise.Generate1D((float)The.Sim.TotalUnPausedGameTimeInSeconds, GameData.Instance.AIConstants.MigrateStabilityFrequency);
            /*
            SimplexNoiseGenerator.SeedNumbers = stabilitySeedNumbers;
            float noise = SimplexNoiseGenerator.Generate1D((float)The.Sim.TotalUnPausedGameTimeInSeconds, GameData.Instance.AIConstants.MigrateStabilityFrequency);*/

            return (1f - Stability) * noise; // scale

        }

        public static float ComputeHappinessComponent(float principle, float rating)
        {
            return rating - principle;
        }

        public float ComputeHappinessComponent(RatingTypes ratingType)
        {
            float principle = Principles[ratingType];

            float rating = Parent.Intelligence.Statistics.GetRating(ratingType);

            float difference = ComputeHappinessComponent(principle, rating);

            return difference;
        }

        void ComputeHappiness()
        {
            int noOfComponents = Principles.Count;

            float differenceSum = 0f;

            float mostUnhappyRating = 0f;
            RatingTypes? mostUnhappyRatingType = null;

            foreach (var item in Principles)
            {
                float principle = item.Value;

                float rating = Parent.Intelligence.Statistics.GetRating(item.Key);

                float difference = ComputeHappinessComponent(principle, rating);

                if (difference < mostUnhappyRating)
                {
                    mostUnhappyRating = difference;
                    mostUnhappyRatingType = item.Key;
                }

                differenceSum += difference;

            }

            happiness = differenceSum / noOfComponents;

            /*if (mostUnhappyRatingType.HasValue)
            {*/
                this.mostUnhappyRating = mostUnhappyRatingType;
           // }
        }


        public void Update(double? timeSinceLastUpdate)
        {
           
            UpdatePrinciples(timeSinceLastUpdate);

        }


        public /*DateAndTime.TimeDateYear*/ float GetTimeForPrincipleToReachValue(RatingTypes rating, float value)
        {
            float principle = Principles[rating];
            float principleDelta = value - principle;

            float timeInSeconds = principleDelta / (Adaptability * (float)(GameData.Instance.AIConstants.Ratings.PrinciplesAdaptationSpeedPerSecond));

            return timeInSeconds;

          //  float principleDelta = Adaptability * (float)(GameData.Instance.AIConstants.Ratings.PrinciplesAdaptationSpeedPerSecond * time);

           // return DateAndTime.GetSecondsToIngameDays(timeInSeconds);

           /* float timeInDays = (float)(timeInSeconds / DateAndTime.secondsPerDay);

            return timeInDays;*/

        }

        /// <summary>
        /// move principles towards a point above colony's rating, to simulate people's needs for more...
        ///  
        /// </summary>
        /// <param name="timeSinceLastUpdate"></param>
        void UpdatePrinciples(double? timeSinceLastUpdate)
        {
            Allegiance allegiance = Parent.Intelligence.Allegiance;

            List<RatingTypes> ratingTypes = Principles.Keys.ToList();
            foreach (var item in ratingTypes)
            {
                // float colonyRating = allegiance.Statistics.GetRating(item);
                float rating = Parent.Intelligence.Statistics.GetRating(item);

                float targetPoint = rating + GameData.Instance.AIConstants.Ratings.PrinciplesTargetDelta;

                float principle = Principles[item];
                float difference = ComputeHappinessComponent(principle, targetPoint); // rating);

                float principleDelta = 0f;
                if (!Common.IsZero(difference))
                {
                    principleDelta = Adaptability * (float)(GameData.Instance.AIConstants.Ratings.PrinciplesAdaptationSpeedPerSecond * timeSinceLastUpdate.Value);

                    principleDelta = Common.ClampTop(principleDelta, Math.Abs(difference));

                    if (Common.IsLessThanOrEqual(difference, 0f))
                    {
                        principleDelta *= -1f;
                    }

                    float newPrinciples = principle + principleDelta;
                    newPrinciples = Common.Clamp(newPrinciples, 0f, 1f);

                    Principles[item] = newPrinciples;
                }

            }
        }

       /* void CreateDecisionPoints(double? timeSinceLastUpdate)
        {
            Allegiance allegiance = Parent.Intelligence.Allegiance;

            foreach (var item in Principles)
            {
                float happiness = ComputeHappinessComponent(item.Key);

                if (happiness < 0f)
                {
                    float points = -(float)(GameData.Instance.AIConstants.Ratings.DecisionPointsPerHappinessPerSecond * timeSinceLastUpdate.Value * happiness);

                    allegiance.AddDecisionPoints(item.Key, points);
                }
            }


        }*/

        /*  private void CreateStabilitySeedNumbers()
          {
              RandomGenerator generator = new RandomGenerator(stabilityRandomSeed, RandomGenerator.GeneratorType.Sim);
              stabilitySeedNumbers = SimplexNoiseGenerator.CreateSeedNumbers(generator); // these numbers are the same before and after snapshot.
          }*/

        void Statistics_RatingsChanged()
        {
            happinessIsDirty = true;
        }


        #region ISnapshot

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public Snapshotter.Version DoVersion(Snapshotter sn)
        {
            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }

        public bool IsSnapshotted { get; set; }

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            this.PersonalityType = sn.DoGameData(PersonalityType);
            this.Principles = sn.DoDictionary(Principles);

            // this.Adventurousness = sn.DoFloat(Adventurousness);
            this.Stability = sn.DoFloat(Stability);
            this.stabilityNoise = (SimplexNoise)sn.DoISnapshot(stabilityNoise);
            this.Adaptability = sn.DoFloat(Adaptability);
            this.Attraction = sn.DoDictionary(Attraction);

         /*   this.happiness = sn.DoFloat(happiness);
            this.happinessIsDirty = sn.DoBool(happinessIsDirty);
          */

            sn.Ignore(Parent);

            // recompute all these on load:
            sn.Ignore(happinessIsDirty);
            sn.Ignore(happinessBreakdown);
            sn.Ignore(happiness);
            sn.Ignore(mostUnhappyRating);

            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            stabilityNoise.LoadPostProcess(sn);
            //CreateStabilitySeedNumbers();

            Parent.Intelligence.Statistics.RatingsChanged += Statistics_RatingsChanged;
        }




        #endregion


    }
}
