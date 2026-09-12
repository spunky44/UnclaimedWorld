using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Allegiances;

namespace UWGame.SimSide.AI.StrategicDecisions
{
    /// <summary>
    /// Shows the result of comparing two allegiances by an agent
    /// 
    /// stores the calculation (including weights) for client display
    /// </summary>
    public class AllegianceRatings : ISnapshot, IScore, IEdge
    {
        public float Score { get; set; }
        public float Edge { get; set; }

        public AllegianceID AllegianceID;


        /// <summary>
        /// sum of stat scores, not including the other factors
        /// </summary>
        public float TotalStatScore;


        /// <summary>
        /// +/- in %
        /// </summary>
        public float Desirability;

        public float Personal;

        public float Attraction;

        /// <summary>
        /// own condition, other condition
        /// </summary>
        public Dictionary<RatingTypes, Pair<float, float>> Conditions = new Dictionary<RatingTypes, Pair<float, float>>(); 
       // public Dictionary<StatTypes, Pair<float, float>> Conditions = new Dictionary<StatTypes, Pair<float, float>>(); 


        /// <summary>
        /// diff, weighted result (+/- in %)
        /// </summary>
        public Dictionary<RatingTypes, Pair<float, float>> ResultComponents = new Dictionary<RatingTypes, Pair<float, float>>(); 
       // public Dictionary<StatTypes, Pair<float, float>> ResultComponents = new Dictionary<StatTypes, Pair<float, float>>(); 

        // other factors:

        string breakdown = null;
        public string Breakdown
        {
            get
            {
                if (breakdown == null)
                {
                    breakdown = GetBreakdown();
                }

                return breakdown;
            }

        }

       // public void SetConditions(StatTypes stat, float ownCondition, float otherCondition)
        public void SetConditions(RatingTypes stat, float ownCondition, float otherCondition)
        {
            Pair<float, float> rating;
            if (!Conditions.TryGetValue(stat, out rating))
            {
                rating = new Pair<float, float>();
                Conditions.Add(stat, rating);
            }

            rating.First = ownCondition;
            rating.Second = otherCondition;

            breakdown = null;

        }

       // public void SetResults(StatTypes stat, float diff, float score)
        public void SetResults(RatingTypes stat, float diff, float score)
        {
            Pair<float, float> rating;
            if (!ResultComponents.TryGetValue(stat, out rating))
            {
                rating = new Pair<float, float>();
                ResultComponents.Add(stat, rating);
            }

            rating.First = diff;
            rating.Second = score;

            breakdown = null;
        }

        private string GetBreakdown()
        {
            StringBuilder text = new StringBuilder();

          //  Common.AppendLine(text, "The person's willingness to join our colony.");
            text.Append("ATTRACTION: "); //mp was "DESIRABILITY: "  could also be called Attraction
            Common.AppendLine(text, Common.PercentageToString(Desirability, useColoring:true));

            Common.AppendDivider(text);
            Common.AppendLine(text, "Based on:");
            Common.AppendLine(text);

            //  Common.AppendLine(text, Statistic.StatTypeToString(item.Key).ToUpper(Config.Culture));
            Common.AppendLine(text, "COMPARISON OF COLONY CONDITIONS: "); // NOTE: us/them gets inverted depending on where the person is - us refers to his current home. //mp was "COLONY RATING COMPARISON: "  but not clear enough.

            foreach (var item in ResultComponents)
            {
               // 4 % -41 %= -37 %
                Pair<float, float> conditions = Conditions[item.Key];
                text.Append(Common.indentString);
                Statistic.AppendRatingsTypeToStringAndIcon(text, 
                    item.Key);
               // text.Append(Statistic.RatingsTypeToString(item.Key).ToUpper(Config.Culture));
                text.Append(": ");
                text.Append(Common.PercentageToString(conditions.Second)); 
                text.Append(" - ");
                text.Append(Common.PercentageToString(conditions.First));
                text.Append(" = ");
                Common.AppendLine(text, Common.PercentageToString(item.Value.First, useColoring: true)); // .Second));

            }

            Common.AppendLine(text);
            //  Common.AppendLine(text, Statistic.StatTypeToString(item.Key).ToUpper(Config.Culture));
            text.Append("WEIGHTED TOTAL: ");
            Common.AppendLine(text, Common.PercentageToString(TotalStatScore, true, useColoring: true));


            Common.AppendLine(text);
            //  Common.AppendLine(text, Statistic.StatTypeToString(item.Key).ToUpper(Config.Culture));
            text.Append("PERSONAL CIRCUMSTANCES: "); // text.Append("Personal inertia (adventurousness): ");
            Common.AppendLine(text, Common.PercentageToString(Personal, true, useColoring: true));

          /*  if (!Common.IsZero(Attraction))
            {*/
                Common.AppendLine(text);
                text.Append("PERSONAL INTEREST: ");
                Common.AppendLine(text, Common.PercentageToString(Attraction, true, useColoring: true));

          //  }

            // AppendLine(text, "Rating of the colony's nutrition situation.");
            // AppendComponent(text, "Nutrition rating", rating, omitIfZero: false, formatAsPercentage: true);

            return text.ToString();

        }


        #region ISnapshot


        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            ResultComponents = sn.DoDictionary(ResultComponents);
            Conditions = sn.DoDictionary(Conditions);           
            Desirability = sn.DoFloat(Desirability);
            TotalStatScore = sn.DoFloat(TotalStatScore);
            Personal = sn.DoFloat(Personal);
            Score = sn.DoFloat(Score);
            Edge = sn.DoFloat(Edge);
            AllegianceID = sn.DoEnum(AllegianceID);
            Attraction = sn.DoFloat(Attraction);

            sn.Ignore(breakdown);

            return this;
        }

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

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

        }

        #endregion

    }
}
