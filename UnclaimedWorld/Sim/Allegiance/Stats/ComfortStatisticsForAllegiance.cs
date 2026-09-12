using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.AI.Needs;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Items;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Allegiances.Statistics
{
    public class ComfortStatisticsForAllegiance : ComfortStatistics
    {

        /// <summary>
        /// for othersite entities, this will probably be the only component in member rating
        /// </summary>
        public List<DataPoint<float>> SharedRating = new List<DataPoint<float>>();



        public ComfortStatisticsForAllegiance()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");

        }

        public ComfortStatisticsForAllegiance(GroupStatistics parent)
            : base(parent)
        {

        }

        public override void AddSharedRating(float ratingValue)
        {
            base.AddSharedRating(ratingValue);

            DateAndTime.TimeDateYear now = The.Sim.DateAndTime.CurrentTimeDateYear;

            SharedRating.Add(new DataPoint<float>()
            {
                Time = now,
                Value = ratingValue
            });
        }

        public float GetSharedRatings()
        {
            if (SharedRating.Count > 0)
                return SharedRating.Last().Value;
            
            return 0f;
        }


      /*  protected override float ScoreRating()
        {           
            int currentNoOfMembers;
            float homeComfort;
            float needsRating;
            Dictionary<NeedTypeID, Tuple<float, float>> hasComfortNeedsMet;
            string homeName;
            GatherHousingComfort(out currentNoOfMembers, out homeComfort, out homeName);

            
            ScoreComfortNeeds(currentNoOfMembers, out hasComfortNeedsMet, out needsRating);
            
            float rating;
            CombineScores(homeComfort, needsRating, 0f, out rating);


            if (composeBreakdown)
            {
                ComposeRatingBreakdown(rating, homeComfort, hasComfortNeedsMet, needsRating);
            }

            return rating;

        }*/

        protected override float ScoreRating()
        {
            int currentNoOfMembers;
            float homeComfort;
            float needsRating;
            Dictionary<NeedTypeID, float> hasComfortNeedsMet;
            string homeName;
            GatherHousingComfort(out currentNoOfMembers, out homeComfort, out homeName);
            
            ScoreComfortNeeds(currentNoOfMembers, out hasComfortNeedsMet, out needsRating);

            float rating;
            CombineScores(homeComfort, needsRating, 0f, out rating);


            if (composeBreakdown)
            {
                ComposeRatingBreakdown(rating, homeComfort, hasComfortNeedsMet, needsRating);
            }

            return rating;

        }

        /// <summary>
        /// compose the text that explains the comfort rating at the specified timepoint.
        /// </summary>
        /// <param name="timepointIndex"></param>
        /// <param name="rating"></param>
        /// <param name="text"></param>
        private void ComposeRatingBreakdown(float rating, float homeComfort,
            Dictionary<NeedTypeID, float> hasComfortNeedsMet,  // Dictionary<NeedTypeID, Tuple<float, float>> hasComfortNeedsMet, 
            float needsRating)
        {           
            StringBuilder text = new StringBuilder();

            Common.AppendLine(text, "How well the colony provides comfort and luxuries:");
            AppendComponent(text, "COLONY COMFORT CONDITIONS", rating, omitIfZero: false, formatAsPercentage: true);

            Common.AppendDivider(text);
            Common.AppendLine(text, "Based on:");
            Common.AppendLine(text);

            Common.AppendLine(text, "HOUSING; AVG. COMFORT VALUE");
            text.Append("Subscore: ");
            Common.AppendLine(text, Common.PercentageToString(homeComfort, useColoring: true));

            ComposeRatingBreakdownForNeeds(hasComfortNeedsMet, needsRating, text);


            ratingsBreakdown = text.ToString();

        }


        private static bool AffectsHomeRating(EntityType entityType)
        {
            if (entityType.ContainerType != null && entityType.ContainerType.ResidenceType != null 
                && entityType.ContainerType.ResidenceType.ComfortLevel > 0f)
            {
                return true;
            }

            if (entityType.Upgrader != null)
            {
                if (entityType.Upgrader.EffectsFinal != null)
                {
                    foreach (var item in entityType.Upgrader.EffectsFinal)
                    {
                        if (/*item.Affects(SimEffects.AffectsNumbers.AgentComfort) ||*/ item.Affects(SimEffects.AffectsNumbers.OfferedComfort))                      
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private static bool AffectsNeedsRating(Allegiance allegiance, /*NeedType[] needs,*/ EntityType itemEntityType)
        {
            // similar to AffectsFoodRating, only considers representative type...
            if (allegiance.RepresentativeEntityType.BiologicalType.IsEatable(itemEntityType))
            {
                if (itemEntityType.ItemType.FoodType.EffectTypes != null)
                {
                    foreach (var item in itemEntityType.ItemType.FoodType.EffectTypes)
                    {
                        if (item.Affects(SimEffects.AffectsNumbers.AgentComfort) || item.Affects(SimEffects.AffectsNumbers.OfferedComfort))
                            //item.EffectTypes.Any(e => e.Affects == SimEffects.AffectsNumbers.AgentComfort || e.Affects == SimEffects.AffectsNumbers.OfferedComfort))
                        {
                            return true;
                        }                        
                    }
                }

                /*
                NeedType needType;
                foreach (var item in itemEntityType.ItemType.FoodType.FoodNutrientProfile.FoodNutrientTypes)
                {
                   if (item.SatisfiesComfortNeed(needs, out needType))
                   {
                       return true;
                   }
                }*/
            }
            

            return false;
        }

        public static bool AffectsComfortRating(Allegiance allegiance, EntityType entityType) //, NeedType[] needs)
        {
            return AffectsHomeRating(entityType)
                || AffectsNeedsRating(allegiance, /*needs,*/ entityType); 
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

            this.SharedRating = sn.DoList(SharedRating);


            return this;
        }

        public override void LoadPostProcess(Snapshotter sn)
        {
            base.LoadPostProcess(sn);

        }

        #endregion

    }
}
