using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.AI.Needs;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Allegiances.Statistics
{
    public class ComfortStatisticsForMembers : ComfortStatistics
    {
        /// <summary>
        /// update when changing allegiance
        /// </summary>
        public Allegiance Allegiance;
        AllegianceID snapshotAllegiance;



        public ComfortStatisticsForMembers()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
    
        }

        public ComfortStatisticsForMembers(GroupStatistics parent, Allegiance parentAllegiance)
             : base(parent)
        {
            this.Allegiance = parentAllegiance;

        }

       


        protected override float ScoreRating() 
        {
            // get the parent allegiance, we need the stats as base:
            ComfortStatisticsForAllegiance allegianceStats = (ComfortStatisticsForAllegiance)Allegiance.Statistics.Ratings[RatingTypes.Comfort];

            float sharedRating = (float)allegianceStats.GetSharedRatings();
            float rating;

           
            if (IsPlaySite())
            {

                int currentNoOfMembers;
                float homeComfort, needsRating;
                string homeName;
                GatherHousingComfort(out currentNoOfMembers, out homeComfort, out homeName);

               // Dictionary<NeedTypeID, Tuple<float, float>> hasComfortNeedsMet;  
                // ScoreComfortNeeds(currentNoOfMembers, out needsRating);

                Dictionary<NeedTypeID, float> hasComfortNeedsMet;
                ScoreComfortNeeds(currentNoOfMembers, out hasComfortNeedsMet, out needsRating);
             
                
                CombineScores(homeComfort, needsRating, sharedRating, out rating);


                if (composeBreakdown)
                {
                    ComposeRatingBreakdown(rating, sharedRating, homeComfort, hasComfortNeedsMet, needsRating, homeName);
                }
            }
            else
            {
                // for othersite entities, use the shared rating (constant) as the total...
                rating = sharedRating;

                if (composeBreakdown)
                {
                    ComposeRatingBreakdownOtherSite(rating, sharedRating);
                }
            }

            return rating;

        }

        private void ComposeRatingBreakdownOtherSite(float rating, float sharedScore)
        {
            StringBuilder text = ComposeSharedRatingBreakdown(rating, sharedScore);

            ratingsBreakdown = text.ToString();
        }
      
        /// <summary>
        /// compose the text that explains the comfort rating at the specified timepoint.
        /// </summary>
        /// <param name="timepointIndex"></param>
        /// <param name="rating"></param>
        /// <param name="text"></param>
        private void ComposeRatingBreakdown(float rating, float sharedScore, float homeComfort,
            Dictionary<NeedTypeID, float> hasComfortNeedsMet, // Dictionary<NeedTypeID, Tuple<float, float>> hasComfortNeedsMet, 
            float needsRating, string homeName)
        {
            // rating = Rating[timepointIndex].Value;

            // DateAndTime.TimeDateYear now = The.Sim.DateAndTime.CurrentTimeDateYear;
            // ScoreRating(timepointIndex, /*now,*/ out rating, out homeComfort);

            StringBuilder text = ComposeSharedRatingBreakdown(rating, sharedScore);

            Common.AppendLine(text);

            text.Append("HOUSING: ");
            Common.AppendLine(text, homeName);
            text.Append("Subscore: +");
            Common.AppendLine(text, Common.PercentageToString(homeComfort, useColoring: true));

            ComposeRatingBreakdownForNeeds(hasComfortNeedsMet, needsRating, text, false);

            ratingsBreakdown = text.ToString();

        }

        private StringBuilder ComposeSharedRatingBreakdown(float rating, float sharedScore)
        {
            StringBuilder text = new StringBuilder();

            Common.AppendLine(text, "and their personal experience of comfort"); //mp this text comes in continuation of the line above about principles.     was "Rating of the character's comfort level"
            AppendComponent(text, "PERSONAL COMFORT CONDITIONS:", rating, omitIfZero: false, formatAsPercentage: true); //was "PERSONAL COMFORT RATING:"

            Common.AppendDivider(text);
            Common.AppendLine(text, "Based on:");
            Common.AppendLine(text);

            AppendComponent(text, "COLONY COMFORT CONDITIONS", sharedScore, omitIfZero: false, formatAsPercentage: true);
            return text;
        }

        public override void ChangeAllegiance(Allegiance newAllegiance)
        {
            Allegiance = newAllegiance;
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
