using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Allegiances.Statistics
{
    public enum RatingTypes { Food, Security, Comfort }

    public abstract class Rating: Statistic
    {
       // public const string RatingsBreakdownTempMessage = "Computing, please wait...";
        public const string RatingsBreakdownTempMessage = "Computing a new rating. (If paused, unpause the game)";

       
        protected bool composeBreakdown = false;
        protected string ratingsBreakdown = FoodStatistics.RatingsBreakdownTempMessage;


        public List<DataPoint<float>> Ratings = new List<DataPoint<float>>();

        protected const int minimumDataPointsToStore = 1; // 50;


        protected Rating()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }

        public Rating(double pollInterval): base(pollInterval)
        {
           
        }


        public override void Update(GameTime gameTime)
        {
            if (Ratings.Count == 0)
            {
                GatherPolledData();
            }
            else
            {
                base.Update(gameTime);
            }
        }

        public override void GatherPolledData()
        {
            float rating = ScoreRating();

            DateAndTime.TimeDateYear now = The.Sim.DateAndTime.CurrentTimeDateYear;
            Ratings.Add(new DataPoint<float>()
            {
                Time = now,
                Value = rating
            });


            if (composeBreakdown == false)
            {
                ratingsBreakdown = RatingsBreakdownTempMessage; // don't let the player see outdated information.
            }
        }

        protected abstract float ScoreRating();

        public override float GetChange()
        {
            return Statistic.GetChange(Ratings);

        }


        public virtual void AddSharedRating(float ratingValue)
        {

        }

        protected bool IsPlaySite()
        {
            ICanIterateEntities group = LookUpICanIterateEntities.FindByID(Parent.CanIterateEntitiesID);
            return group.GetAllegiance.Site.IsPlaySite;
        }


        /// <summary>
        /// returns 0 - 1
        /// </summary>
        /// <returns></returns>       
        public override float GetLatestValue()
        {
            if (Ratings.Count > 0)
                return Ratings.Last().Value;

            return 0;
        }


        /// <summary>
        /// for client use in tooltips.
        /// only gets computed if the toggle has been set!
        /// </summary>
        /// <returns></returns>       
        public string GetRatingsBreakdown()
        {
           /* if (The.Sim.IsPaused && ratingsBreakdown == FoodStatistics.RatingsBreakdownTempMessage)
            {
                return "Unpause the game to compute a new rating.";
            }
            else
            {*/
                return ratingsBreakdown;
          //  }
        }
 
        /// <summary>
        /// toggle whether the tooltip should be composed when scoring
        /// </summary>
        /// <param name="compose"></param>
        public void ToggleComposeBreakdown(bool compose)
        { 
            composeBreakdown = compose;

            /*
            if (compose == false)
            {
                ratingsBreakdown = RatingsBreakdownTempMessage; // don't let the player see outdated information.
            }
            */
        }


        #region ISnapshot

        Snapshotter.Version version;
        public override Snapshotter.Version DoVersion(Snapshotter sn)
        {
            base.DoVersion(sn); // each class in the class hierarchy snapshots and maintains their own version.

            version = sn.DoVersion(Snapshotter.Version.Original);
            return version;
        }

        public override ISnapshot DoSnapshot(Snapshotter sn)
        {
            base.DoSnapshot(sn);

            this.Ratings = sn.DoList(Ratings);


            sn.Ignore(ratingsBreakdown);
            sn.Ignore(composeBreakdown);


            return this;
        }



        #endregion
    }
}
