using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.AI.Constants.Rating;
using UWGame.SimSide.AI.Needs;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Allegiances.Statistics
{
    public class FoodStatisticsForMembers: FoodStatistics
    {
        /// <summary>
        /// update when changing allegiance
        /// </summary>
        public Allegiance Allegiance;
        AllegianceID snapshotAllegiance;

        public FoodStatisticsForMembers()
        {
        }

        public FoodStatisticsForMembers(GroupStatistics parent, Allegiance parentAllegiance)
            : base(parent)
        {
            this.Allegiance = parentAllegiance;

        }

        

        /// <summary>       
        /// score hunger deaths and needs.
        /// Add ratings from colony to score, for communal food stockpiles (and hunger deaths)?
        /// </summary>      
        protected override float ScoreRating() 
        {
            Food food = GameData.Instance.AIConstants.Ratings.Food;

            float rating;

            int hungerDeaths;
            int noOfMembers = GetMembers();
            Dictionary<NeedTypeID, float> starvingMembers;
            float totalNeeds;
            float deathsContribution;
            ScoreNeedsAndDeaths(noOfMembers, out hungerDeaths, out starvingMembers, out totalNeeds, out deathsContribution);

            // get the parent allegiance, we need the stats as base:
            FoodStatisticsForAllegiance allegianceStats = (FoodStatisticsForAllegiance)Allegiance.Statistics.Ratings[RatingTypes.Food];

            // Base score is NOT in shared rating!
            float sharedRating = (float)allegianceStats.GetSharedRatings(); 

            float starvingScore;
            CombineScores(food.BaseScore, sharedRating, totalNeeds, deathsContribution, out rating, out starvingScore);
            
            if (composeBreakdown) 
            {
                ComposeRatingBreakdown(food, rating, hungerDeaths, deathsContribution, noOfMembers, starvingMembers, starvingScore, sharedRating);
            }

            return rating;
        }

        public override void ChangeAllegiance(Allegiance newAllegiance)
        {
            Allegiance = newAllegiance;
        }

        private void ComposeRatingBreakdown(Food food, float rating, int deaths, float deathsContribution, int noOfMembers, Dictionary<NeedTypeID, float> starvingMemberPercentages, float starvingScore, float sharedScore)
        {
            StringBuilder text = new StringBuilder();

            //  DateAndTime.TimeDateYear time = Rating[timepointIndex].Time;
            // float deathsContribution;
            // int deaths, noOfMembers;
            //  Dictionary<NeedTypeID, float> starvingMemberPercentages = new Dictionary<NeedTypeID,float>();
            // ScoreRating(timepointIndex, time, out rating, out noOfMembers, out deaths, ref starvingMemberPercentages);

            Common.AppendLine(text, "and their personal experience of food conditions"); 
            AppendComponent(text, "PERSONAL FOOD CONDITIONS:", rating, omitIfZero: false, formatAsPercentage: true);

            Common.AppendDivider(text);
            Common.AppendLine(text, "Based on:");
            Common.AppendLine(text);

            Common.AppendLine(text, "BASELINE");
            text.Append("Subscore: +");
            Common.AppendLine(text, Common.PercentageToString(food.BaseScore, useColoring: true));
            Common.AppendLine(text);

            AppendComponent(text, "COLONY FOOD CONDITIONS", sharedScore, omitIfZero: false, formatAsPercentage: true);


            ComposeRatingBreakdownForNeedsAndDeaths(food, deaths, deathsContribution, starvingMemberPercentages, starvingScore, text);


            ratingsBreakdown = text.ToString();

        }


        #region ISnapshot

        Snapshotter.Version version;
        public override Snapshotter.Version DoVersion(Snapshotter sn)
        {
            base.DoVersion(sn); // each class in the class hierarchy snapshots and maintains their own version.

            version = sn.DoVersion(Snapshotter.Version.Original);
            return version;
        }

        public override Snapshots.ISnapshot DoSnapshot(Snapshots.Snapshotter sn)
        {
            base.DoSnapshot(sn);

            snapshotAllegiance = (AllegianceID)sn.SnapshotID<Allegiance, AllegianceID>(Allegiance);



            return this;
        }

        public override void LoadPostProcess(Snapshotter sn)
        {
            base.LoadPostProcess(sn);

            Allegiance = LookUp<Allegiance, AllegianceID>.FindByID(snapshotAllegiance);
        }

        #endregion
    }
}
