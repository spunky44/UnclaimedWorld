using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.AI.Constants.Rating;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Allegiances.Statistics
{
    public class SecurityStatisticsForMembers : SecurityStatistics 
    {
        /// <summary>
        /// update when changing allegiance
        /// </summary>
        public Allegiance Allegiance;
        AllegianceID snapshotAllegiance;

        public SecurityStatisticsForMembers()
        {

        }


        public SecurityStatisticsForMembers(GroupStatistics parent, Allegiance parentAllegiance)
            : base(parent)
        {
            this.Allegiance = parentAllegiance;

        }

        public override void ChangeAllegiance(Allegiance newAllegiance)
        {
            Allegiance = newAllegiance;
        }

        /// <summary>       
        /// score violent events as usual. 
        /// Add ratings from colony to score, for communal security like weapon stockpiles
        /// </summary>      
        protected override float ScoreRating() //out float injuryContribution, out float deathsContribution)
        {
            int noOfMembers = GetMembers();

            float assets = GetAssets();

            float rating;
            int injuries, deaths;

            float totalInjuryContribution, finalInjuryContribution;
            float totalDeathsContribution, finalDeathsContribution;
            ComputeInjuriesAndDeaths(assets, out injuries, out deaths, out totalInjuryContribution, out totalDeathsContribution, out finalInjuryContribution, out finalDeathsContribution);
            
            // get the parent allegiance, we need the stats as base:
            SecurityStatisticsForAllegiance allegianceStats = (SecurityStatisticsForAllegiance)Allegiance.Statistics.Ratings[RatingTypes.Security];

            float sharedRating = allegianceStats.GetSharedRatings(); // (float)allegianceStats.GetLatestValue(); 

           
           // rating = sharedRating - injuryContribution - deathsContribution;
            rating = sharedRating - finalInjuryContribution - finalDeathsContribution;

            rating = Common.Clamp(rating, 0f, 1f);


            if (composeBreakdown) // || ratingsBreakdown == null) // make sure we always have something for the tooltip to show
            {
                ComposeRatingBreakdown(rating, noOfMembers, injuries, deaths, totalInjuryContribution, totalDeathsContribution, finalInjuryContribution, finalDeathsContribution, sharedRating);
            }

            return rating;

            /*

            DateAndTime.TimeDateYear now = The.Sim.DateAndTime.CurrentTimeDateYear;
            Rating.Add(new DataPoint<float>()
            {
                Time = now,
                Value = rating
            });*/
        }

        private void ComposeRatingBreakdown(float rating, int noOfMembers, int injuries, int deaths, //float injuryRating, float deathRating, 
            float totalInjuryContribution, float totalDeathsContribution, float finalInjuryContribution, float finalDeathsContribution,
            float sharedSecurity) 
        {
          
            StringBuilder text = new StringBuilder();

            Security sec = GameData.Instance.AIConstants.Ratings.Security;

            Common.AppendLine(text, "and their personal experience of security conditions"); //was "Rating of the character's security situation."
            AppendComponent(text, "PERSONAL SECURITY CONDITIONS:", rating, omitIfZero: false, formatAsPercentage: true);

            Common.AppendDivider(text);
            Common.AppendLine(text, "Based on:");
            Common.AppendLine(text);

            AppendComponent(text, "COLONY SECURITY CONDITIONS", sharedSecurity, omitIfZero: false, formatAsPercentage: true);

            ComposeDeathsBreakdown(noOfMembers, injuries, deaths, finalInjuryContribution, finalDeathsContribution, text, sec);

            /* if (deaths > 0 || injuries > 0)
             {
                 text.AppendFormat("Per member: /{0:N0} ", sec.InjuryFactor);
                 AppendLine(text);
             }*/

           /* AppendLine(text);
            AppendLine(text, "As percentage: *100 ");*/
            // AppendDivider(text);

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
