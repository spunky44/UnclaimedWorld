using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.AI.Needs;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.AI.Constants.Rating;
using WindowSystem;

namespace UWGame.SimSide.Allegiances.Statistics
{

    /// <summary>
    /// compute as:
    /// total hunger days per time period
    /// each need type can produce hunger separately - add them together (variable: DaysAtZero)
    /// </summary>
    public abstract class FoodStatistics : Rating //Statistic, IHasRating
    {

        #region Raw data
        // Poll data:
      //  Dictionary<NeedTypeID, List<DataPoint<List<EntityID>>>> starvingMembers = new Dictionary<NeedTypeID, List<DataPoint<List<EntityID>>>>(); // cannot handle nested generic types in snapshot...
        
        /// <summary>
        /// delete this
        /// </summary>
      //  public List<DataPoint<float>> testData = new List<DataPoint<float>>();

        /// <summary>
        /// was never used...
        /// we use double for the count to simplify methods
        /// </summary>
     //   public Dictionary<NeedTypeID, List<DataPoint<float>>> starvingMemberCount = new Dictionary<NeedTypeID, List<DataPoint<float>>>();

        /// <summary>
        /// value 0.0 - 100.0
        /// 
        /// only the latest data point is ever used... the rest are for showing a graph (player only probably)
        /// </summary>
        public Dictionary<NeedTypeID, List<DataPoint<float>>> StarvingMemberPercentage = new Dictionary<NeedTypeID, List<DataPoint<float>>>();
       
    /*    Dictionary<NeedType, List<DataPoint<List<EntityID>>>> starvingMembers = new Dictionary<NeedType, List<DataPoint<List<EntityID>>>>();
        Dictionary<NeedType, List<DataPoint<double>>> starvingMemberCount = new Dictionary<NeedType, List<DataPoint<double>>>();
        Dictionary<NeedType, List<DataPoint<double>>> starvingMemberPercentage = new Dictionary<NeedType, List<DataPoint<double>>>();
        */
        // Event data:
        public List<DataPoint<EntityID>> starvingDeaths = new List<DataPoint<EntityID>>();

        #endregion

        #region Computed data sets

        
          
        /// <summary>
        /// stores a rolling average, for optimization   
        /// 
        /// value 0.0 - 100.0
        /// </summary>
      //  Dictionary<NeedTypeID, DataPoint<float>> starvingMemberPercentageAverage = new Dictionary<NeedTypeID, DataPoint<float>>();
       
        // not needed..?
       // Dictionary<NeedTypeID, List<DataPoint<float>>> starvingMemberCountAverage = new Dictionary<NeedTypeID, List<DataPoint<float>>>();
       // Dictionary<NeedTypeID, List<DataPoint<float>>> starvingDeathsAverage = new Dictionary<NeedTypeID, List<DataPoint<float>>>();


      //  List<DataPoint<float>> starvingDeathCounts = new List<DataPoint<float>>();

       

        public enum SetsOfData { StarvingAbsolute, StarvingPercentage, StarvingDeaths }


      

        #endregion

        //The regulator controlling how often we poll data about hunger.
     //   float pollsPerDay = 5;


        public FoodStatistics()         
        {

        }


        public FoodStatistics(GroupStatistics parent) : base(0.2)
        {
           
            Parent = parent;

            StatType = StatTypes.Food;            


        }

       

        /// <summary>
        /// add a datapoint for each need type that is currently in the group.
        /// the datapoint contains the number of entities that are suffering starvation
        /// </summary>
        /// <param name="needToCompute"></param>
    /*    protected override void GatherPolledData(GameTime gameTime)
        {            
            
            ScoreRating(); // new - score rating continuously

        }*/

        protected void AddToStarvingDataLists(Dictionary<NeedTypeID, DataPoint<List<EntityID>>> entitiesStarvingPerNeed, int currentNoOfMembers)
        {
            foreach (var item in entitiesStarvingPerNeed)
            {
                //Adds each individual to list of starving individuals
              
                //Adds number of starving people to list
             /*   Common.AddToMultiList(starvingMemberCount, item.Key, new DataPoint<float>()
                {
                    Time = The.Sim.DateAndTime.CurrentTimeDateYear,
                    Value = item.Value.Value.Count
                });*/

                //Adds percentage of starving people to list

                float percentage = ((float)item.Value.Value.Count / currentNoOfMembers) * 100f;

                Common.AddToMultiList(StarvingMemberPercentage, item.Key, new DataPoint<float>()
                {
                    Time = The.Sim.DateAndTime.CurrentTimeDateYear,
                    Value = percentage
                });
            }
        }

      
        protected void ScoreNeedsAndDeaths(int noOfMembers, out int hungerDeaths, out Dictionary<NeedTypeID, float> starvingMembers, out float totalNeeds, out float deathsContribution)
        {
            // Poll Data: Save absolute no, percentage, individuals starving
            // Event data: Starving deaths
                      
            // invert value
            // shift, so midpoint is 0.5
            // clamp if needed

            // score will increase when people die from hunger...
            // so add factor for starving deaths
            // moving average on starving deaths
            DateAndTime.TimeDateYear now = The.Sim.DateAndTime.CurrentTimeDateYear;

            Food food = GameData.Instance.AIConstants.Ratings.Food;

            Dictionary<NeedTypeID, DataPoint<List<EntityID>>> currentDataPoints;
            int currentNoOfMembers;

            GatherStarving(out currentDataPoints, out currentNoOfMembers);

            AddToStarvingDataLists(currentDataPoints, currentNoOfMembers);

            starvingMembers = new Dictionary<NeedTypeID, float>(); // null;
            //  ScoreRating(out rating, out noOfMembers, out deaths, ref starvingMembers);


            // starvingMembers = null;
            totalNeeds = 0;

            NeedType needType;

            int noOfCriticalNeeds = 0;
            
                      
            // we only let needs that are critical for survival influence this rating...
            foreach (var needTypeData in StarvingMemberPercentage)
            {
                needType = LookUp<NeedType, NeedTypeID>.FindByID(needTypeData.Key);

                if (needType.FoodNeedType == null || !needType.FoodNeedType.IsEssential) //DaysAtZeroCausingDeath.HasValue)
                {
                    continue;
                }

                // this need is critical.

                #region OLD: moving average
                /* 
                
                float movingAverageForNeed = 0; // this will hold the newest moving average value for this need type.

                DataPoint<float> lastNeedTypeAverage = null;
                starvingMemberPercentageAverage.TryGetValue(needTypeData.Key, out lastNeedTypeAverage);


                movingAverageForNeed = GetAverage(needTypeData.Value,
                    lastNeedTypeAverage, shortTermFromDate, date);


                //Log the average for reuse next time
                DataPoint<float> newNeedTypeAverage = new DataPoint<float>()
                {
                    Time = date,
                    Value = movingAverageForNeed
                };

                starvingMemberPercentageAverage[needTypeData.Key] = newNeedTypeAverage;
                */
                #endregion

                noOfCriticalNeeds++;

                // float starvingPercentage = needTypeData.Value[timepointIndex].Value;
                float starvingPercentage = needTypeData.Value.Last().Value;

                if (starvingMembers != null)
                {
                    starvingMembers[needTypeData.Key] = starvingPercentage;
                }

                //  totalMovingAverage += movingAverageForNeed;
                totalNeeds += starvingPercentage;

            }

            //  totalMovingAverage /= noOfCriticalNeeds;
            totalNeeds /= noOfCriticalNeeds;

            totalNeeds *= food.StarvationPenaltyFactor;

            //*************
            // LONG TERM: Impact from hunger deaths last a longer time:
            DateAndTime.TimeDateYear longTermFromDate = now; // date;
            longTermFromDate.AddTime(-food.DaysForHungerDeathsToAffect);

            List<DataPoint<EntityID>> starvingDeathsInRange = GetDataPointsBetween<EntityID>(starvingDeaths, longTermFromDate, now); // date);
            hungerDeaths = starvingDeathsInRange.Count;

            float longTermFromDateDays = (float)longTermFromDate.TotalDays;

            deathsContribution = 0;

            deathsContribution = hungerDeaths * food.HungerDeathRatingPenaltyFactor;

            if (noOfMembers > 0)
            {
                deathsContribution /= noOfMembers;
            }
            
            RemoveOldestData();


            // OLD:
            // give each death an impact depending on the total members and when the death happened.
            /*   for (int i = 0; i < starvingDeathsInRange.Count; i++)
               {
                   DataPoint<EntityID> dataPoint = starvingDeathsInRange[i];

                   // scale by members:
                   float thisDeathImpact = (float)food.HungerDeathRatingPenaltyFactor / ((float)currentNoOfMembers + 1f);

                   // scale by passed time:
                   thisDeathImpact = MathHelper.Lerp(0f, thisDeathImpact,
                       (float)((dataPoint.Time.TotalDays - longTermFromDateDays) / food.TimeInDaysForHungerDeathsToAffectFoodStatus));

                   deathsContribution += thisDeathImpact;
               }*/
        }

        
        /// <summary>
        /// needed to avoid save game bloat and running out of memory...
        /// </summary>
        private void RemoveOldestData()
        {
            ICanIterateEntities group = LookUpICanIterateEntities.FindByID(Parent.CanIterateEntitiesID);
            Allegiance allegiance = group.GetAllegiance;
            
            if (!GroupStatistics.GatherStatisticsForDisplayOnly(allegiance)) //  this.Parent.RepresentativeEntityType.Person == null) // collect all data for player, to display in graph
            {
                foreach (var item in StarvingMemberPercentage)
                {
                    while (item.Value.Count > minimumDataPointsToStore)
                    {
                        item.Value.RemoveAt(0);
                    }
                }
            }
        }

        protected static void CombineScores(float baseScore, float stockpileScore, float totalNeeds, float deathsContribution, out float rating, out float totalNeedsScore)
        {
            totalNeedsScore = totalNeeds / 100f;

            rating = baseScore + stockpileScore - totalNeedsScore;

            rating -= deathsContribution;
            rating = Common.Clamp(rating, 0f, 1f);
        }


        protected void ComposeRatingBreakdownForNeedsAndDeaths(Food food, int deaths, float deathsContribution, 
            Dictionary<NeedTypeID, float> starvingMemberPercentages, float starvingScore, 
            StringBuilder text)
        {
            // AppendComponent(text, "Hunger deaths", deaths, indent: true, formatAsPercentage: true);
            if (starvingMemberPercentages != null)
            {
                bool hasAddedHeader = false;
                foreach (var item in starvingMemberPercentages)
                {
                    if (!Common.IsZero(item.Value))
                    {
                        if (!hasAddedHeader)
                        {
                            Common.AppendLine(text);
                            Common.AppendLine(text, "STARVING");
                            hasAddedHeader = true;
                        }

                        NeedType needType = LookUp<NeedType, NeedTypeID>.FindByID(item.Key);
                        // numbers are in percent already
                        AppendComponent(text, needType.ToString(), item.Value, suffix: "%", indent: true, formatAsInteger: true, valueTint: Common.ValueTint.Negative);
                       // AppendComponent(text, needType.ToString(), item.Value, indent: true, formatAsPercentage: true, valueTint: Common.ValueTint.Negative);
                    }
                }

                if (hasAddedHeader)
                {
                    text.Append("Subscore: -");
                    Common.AppendLine(text, Common.PercentageToString(starvingScore, useColoring: true, valueTint: Common.ValueTint.Negative));
                }
            }

            if (deaths > 0)
            {
                Common.AppendLine(text);
                Common.AppendLine(text, "RECENT HUNGER DEATHS");          
                //Common.AppendLine(text, string.Format("Number of deaths: {0:N0} in last {1:N1} days", deaths, food.DaysForHungerDeathsToAffect));
                Common.Append(text, "Number of deaths: ");
                Common.Append(text, deaths.ToString(), true);
                Common.Append(text, " in last ");
                Common.AppendFormat(text, "{0:N1}", true, food.DaysForHungerDeathsToAffect);
                Common.AppendLine(text);
                text.Append("Subscore: -");
                Common.AppendLine(text, Common.PercentageToString(deathsContribution, useColoring: true, valueTint: Common.ValueTint.Negative));
                Common.AppendLine(text);
            }
        }

        /// <summary>
        /// poll the members - add datapoints if starving, both as absolute number and as percentage
        /// </summary>
        protected void GatherStarving(out Dictionary<NeedTypeID, DataPoint<List<EntityID>>> currentDataPointsOut, out int currentNoOfMembers) //, out NeedType needTypeOut)
        {
            // temp variables needed for lambda function
            Dictionary<NeedTypeID, DataPoint<List<EntityID>>> currentDataPoints = new Dictionary<NeedTypeID, DataPoint<List<EntityID>>>();;
            int noOfMembers = 0;

            DataPoint<List<EntityID>> needDataPoint;


            ICanIterateEntities group = LookUpICanIterateEntities.FindByID(Parent.CanIterateEntitiesID);

            // adds the ID for each member that is starving right now
            group.IterateMembers(e =>
            {                    
                if (e.EntityType.BiologicalType != null
                    && GroupStatistics.GatherStatisticsForEntity(e))
                {
                    BiologicalEntity bioEntity;
                    if (e.Find(out bioEntity))
                    {
                        NeedTypeID? needType = null;
                        
                        noOfMembers++;

                        foreach (var need in bioEntity.Needs.NeedsList)
                        {
                            if (need.Value.NeedType.FoodNeedType != null //NeedClass == AINeedClass.Food //) // also stimulants here???
                                && need.Value.NeedType.FoodNeedType.IsEssential) // NEW! exclude stimulants..
                            {
                                needType = need.Value.NeedType.ID;

                                if (!currentDataPoints.TryGetValue(needType.Value, out needDataPoint))
                                {
                                    needDataPoint = new DataPoint<List<EntityID>>()
                                    {
                                        Time = The.Sim.DateAndTime.CurrentTimeDateYear,
                                        Value = new List<EntityID>()
                                    };
                                    currentDataPoints.Add(needType.Value, needDataPoint);
                                }

                                if (Common.IsZero(need.Value.CurrentLevel)) // DaysAtZerogoes up slowly after eating.
                                {
                                    Common.AddToList(ref needDataPoint.Value, e.ID);
                                }
                            }
                        }
                    }
                }

            });

           
           // needTypeOut = needType;
            currentDataPointsOut = currentDataPoints;
            currentNoOfMembers = noOfMembers;
        }

            


       /*  private void ScoreRating(int timepointIndex, DateAndTime.TimeDateYear date, out float rating, out int noOfMembers, out int hungerDeaths , ref Dictionary<NeedTypeID, float> starvingMembers)
        {
           DateAndTime.TimeDateYear shortTermFromDate = date;
            shortTermFromDate.AddTime(-GameData.Instance.AIConstants.TimeInDaysForMovingAverageFoodStatus);
           
            float totalMovingAverage = 0; 

           

            //TimeInDaysForHungerDeathsToAffectFoodStatus = 6;
            //HungerDeathRatingPenaltyFactor

        }*/

       /* public int GetLatestDataIndex()
        {
            if (starvingMemberCount.Count > 0)
            {
                foreach (var item in starvingMemberCount)
                {
                    return item.Value.Count - 1;
                }

            }

            return 0;
        }*/

       


      
        public override string ToString()
        {
            string result = RatingStatisticToString(GetLatestValue());

            return result;
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

           // this.starvingDeathCounts = sn.DoList(starvingDeathCounts);
            this.starvingDeaths = sn.DoList(starvingDeaths);
           // this.starvingDeathsAverage = sn.DoMultiMap(starvingDeathsAverage);
          //  this.starvingMemberCount = sn.DoMultiMap(starvingMemberCount); 
          //  this.starvingMemberCountAverage = sn.DoMultiMap(starvingMemberCountAverage);
            this.StarvingMemberPercentage = sn.DoMultiMap(StarvingMemberPercentage);
          //  this.starvingMemberPercentageAverage = sn.DoDictionary(starvingMemberPercentageAverage);
           // this.starvingMembers = sn.DoMultiMap(starvingMembers);

           // this.Rating = sn.DoList(Rating);


            return this;
        }



        #endregion
    }
}
