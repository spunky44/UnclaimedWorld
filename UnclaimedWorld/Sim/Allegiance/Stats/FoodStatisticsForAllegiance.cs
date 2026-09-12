using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Constants.Rating;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.AI.Needs;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Snapshots;
using WindowSystem;

namespace UWGame.SimSide.Allegiances.Statistics
{
    public class FoodStatisticsForAllegiance : FoodStatistics
    {
        public List<DataPoint<float>> SharedRating = new List<DataPoint<float>>();

        public FoodStatisticsForAllegiance()
        {

        }

        public FoodStatisticsForAllegiance(GroupStatistics parent)
            : base(parent)
        {

        }

        public float GetSharedRatings()
        {
            if (SharedRating.Count > 0)
                return SharedRating.Last().Value;

            return 0f;
        }
        
      
        

        /// <summary>
        /// food for sale should not count.
        /// score ingredients too? hard to measure final nutrient output...
        /// </summary>
        /// <param name="stockpileScore"></param>
        private void ScoreStockpiledFood(int noOfMembers, out int foodItems, out float stockpileScore)
        {
            Food foodConstants = GameData.Instance.AIConstants.Ratings.Food;

            ICanIterateEntities group = LookUpICanIterateEntities.FindByID(Parent.CanIterateEntitiesID);
            Allegiance allegiance = group.GetAllegiance;

            // Dictionary<EntityType, int> ammoRounds = new Dictionary<EntityType, int>();

            int usableFoodItems = 0;
            group.IterateOwnedItems(e =>
            {
                foreach (var item in e.Food)
                {
                    if (AffectsFoodRating(allegiance, item.Key)) 
                    {
                        foreach (var food in item.Value)
                        {
                            IKnownEntityData foodData;
                            if (!GoalEvaluator.EntityDataResultCausesSkip(allegiance.SharedKnowledge.GetKnownData(food, out foodData)))
                            {
                                if (IsAvailableFood(allegiance, foodData))
                                {
                                    usableFoodItems++;
                                    //weaponsData.Add(new Tuple<IKnownEntityData, DefenseRatings>(foodData, defenseRating));
                                }
                            }
                        }
                    }
                }
            });

            foodItems = usableFoodItems;
            stockpileScore = foodItems * foodConstants.StockpiledFoodRating / noOfMembers;

            stockpileScore = Common.ClampTop(stockpileScore, 1f);

           
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

        public static bool AffectsFoodRating(Allegiance allegiance, EntityType item)
        {
            // don't count food for non-independet members like dogs.
            return allegiance.RepresentativeEntityType.BiologicalType.IsEatable(item);
        }


        /// <summary>
        /// don't count carcasses...
        /// </summary>
        /// <returns></returns>
        private bool IsAvailableFood(Allegiance allegiance, IKnownEntityData foodData)
        {
            if (!foodData.IsCompleted())
            {
                return false;
            }

            if (foodData.StoredPermanentlyIn == null)
            {
                return true;
            }
            else
            {
                // don't count food stored for trading either
                 IKnownEntityData containerData;
                 if (!GoalEvaluator.EntityDataResultCausesSkip(allegiance.SharedKnowledge.GetKnownData(foodData.StoredPermanentlyIn.Value.StorageEntity, out containerData)))
                 {
                     if (containerData.IsTradeOfferStorage(foodData.StoredPermanentlyIn.Value.StorageID))
                     {
                         return false;
                     }

                     return true;

                     /* Storage storage = storedIn.GetCompartment(foodData.StoredPermanentlyIn.Value.StorageID);
                      if (storage.StorageConditions.)
                              */
                     

                     //  LookUp<Storage, Stora foodData.StoredPermanentlyIn.Value. 

                 }

                 return false;
            }
        }

        /// <summary>
        /// returns 0 - 1
        ///       
        /// Add penalties for any hunger deaths over the last long period - this is the long range component.
        ///       
        /// </summary>
        protected override float ScoreRating()
        {
            Food food = GameData.Instance.AIConstants.Ratings.Food;

            // score stockpiles:
            float stockpileScore;
            int foodItems;
            float rating;

            int hungerDeaths;
            int noOfMembers = GetMembers(); 
            
            ScoreStockpiledFood(noOfMembers, out foodItems, out stockpileScore);

            float sharedRating = stockpileScore;
            sharedRating = Common.Clamp(sharedRating, 0f, 1f);
            AddSharedRating(sharedRating);


            Dictionary<NeedTypeID, float> starvingMembers;
            float totalNeeds;
            float deathsContribution;
            ScoreNeedsAndDeaths(noOfMembers, out hungerDeaths, out starvingMembers, out totalNeeds, out deathsContribution);

                      
          //  float totalNeedsScore = (100f - totalNeeds) / 100f;
            float starvingScore;
            CombineScores(food.BaseScore, stockpileScore, totalNeeds, deathsContribution, out rating, out starvingScore);


            if (composeBreakdown) 
            {
                ComposeRatingBreakdown(food, rating, hungerDeaths, deathsContribution, noOfMembers, starvingMembers, starvingScore, foodItems, stockpileScore);
            }

            return rating;
        }       




        private void ComposeRatingBreakdown(Food food, float rating, int deaths, float deathsContribution, int noOfMembers, Dictionary<NeedTypeID, 
            float> starvingMemberPercentages, float starvingScore,
            int foodItems, float stockpileScore)
        {
            StringBuilder text = new StringBuilder();

            //  DateAndTime.TimeDateYear time = Rating[timepointIndex].Time;
            // float deathsContribution;
            // int deaths, noOfMembers;
            //  Dictionary<NeedTypeID, float> starvingMemberPercentages = new Dictionary<NeedTypeID,float>();
            // ScoreRating(timepointIndex, time, out rating, out noOfMembers, out deaths, ref starvingMemberPercentages);

            Common.AppendLine(text, "How well the colony provides food:");
            AppendComponent(text, "COLONY FOOD CONDITIONS", rating, omitIfZero: false, formatAsPercentage: true); //was "COLONY NUTRITION RATING"

            Common.AppendDivider(text);
            Common.AppendLine(text, "Based on:");
            Common.AppendLine(text);

            Common.AppendLine(text, "BASELINE");
            text.Append("Subscore: +");
            Common.AppendLine(text, Common.PercentageToString(food.BaseScore, useColoring: true));

            Common.AppendLine(text);

            Common.AppendLine(text, "STOCKPILED FOOD");
            Common.Append(text, "Prepared food items ");
            Common.AppendLine(text, Label.ToLabel(foodItems.ToString(), GameData.Instance.GUIConstants.ValueTintHex));
          
            text.Append("Subscore: +");
            Common.AppendLine(text, Common.PercentageToString(stockpileScore, useColoring: true));

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
