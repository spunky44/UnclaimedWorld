using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.AI.Needs;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.AI.Constants.Rating;

namespace UWGame.SimSide.Allegiances.Statistics
{
    /// <summary>
    /// I think this rating should be computed differently for persons and for animals...    
    /// Animals can have burrows too, that can suffer from overcrowding, parasites etc.
    /// 
    /// For people, compute this stat based on:
    /// housing quality
    /// people per home
    /// other facilities
    /// general living standards - luxuries?
    /// work amount
    /// </summary>
    public abstract class ComfortStatistics : Rating //Statistic, IHasRating
    {
        
        #region Raw data
             
        /// <summary>
        /// each datapoint is the average home (housing) comfort of all the group's members at that moment.
        /// </summary>
       // public List<DataPoint<float>> AverageHomeComfort = new List<DataPoint<float>>();

        /// <summary>
        /// for each comfort need: how large percentage of members have this need satisfied
        /// value 0.0 - 100.0
        /// 
        /// only the last value is used...
        /// </summary>
        public Dictionary<NeedTypeID, List<DataPoint<float>>> HasSatisifedComfortNeedMemberPercentage = new Dictionary<NeedTypeID, List<DataPoint<float>>>();
      
        #endregion

        #region Computed data sets

        /// <summary>
        /// we are no longer doing rolling averages!
        /// 
        /// stores a rolling average, for optimization   
        /// 
        /// value 0.0 - 1.0
        /// </summary>
       // DataPoint<float> homeComfortMovingAverage = null;

      
        #endregion

       

        public ComfortStatistics()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
    
        }

        public ComfortStatistics(GroupStatistics parent)
            : base(0.2)
        {
            Parent = parent;
            StatType = StatTypes.Comfort;
        }

       
      


        public override string ToString()
        {
            string result = RatingStatisticToString(GetLatestValue());

            return result;
        }

      /*  protected void ScoreComfortNeeds(int noOfMembers, out float totalNeeds)
        {
            DateAndTime.TimeDateYear now = The.Sim.DateAndTime.CurrentTimeDateYear;

            Comfort comfort = GameData.Instance.AIConstants.Ratings.Comfort;

            Dictionary<NeedTypeID, DataPoint<List<EntityID>>> currentDataPoints;
            int currentNoOfMembers;

            GatherComfortNeeds(out currentDataPoints, out currentNoOfMembers);

            AddToHasSatisifedComfortNeedDataLists(currentDataPoints, currentNoOfMembers);


            hasSatisifiedComfort = new Dictionary<NeedTypeID, Tuple<float, float>>();

            totalNeeds = 0;

            NeedType needType;


            // we only let needs that are critical for survival influence this rating...
            foreach (var needTypeData in HasSatisifedComfortNeedMemberPercentage)
            {
                needType = LookUp<NeedType, NeedTypeID>.FindByID(needTypeData.Key);

                float hasSatisifedComfortPercentage = needTypeData.Value.Last().Value;

                float rating = 0f;

                if (hasSatisifiedComfort != null)
                {
                    float percentage = 0.01f * hasSatisifedComfortPercentage;
                    rating = needType.ComfortEffects.ComfortWeight * percentage; // some needs can be more important than others...
                    hasSatisifiedComfort[needTypeData.Key] = new Tuple<float, float>(percentage, rating);
                }

                totalNeeds += rating; // hasSatisifedComfortPercentage;
            }
        }*/


        protected void ScoreComfortNeeds(int noOfMembers, out Dictionary<NeedTypeID, float> hasSatisifiedComfort, out float totalComfortEffects)
        {
            DateAndTime.TimeDateYear now = The.Sim.DateAndTime.CurrentTimeDateYear;

            Comfort comfort = GameData.Instance.AIConstants.Ratings.Comfort;

            Dictionary<NeedTypeID, DataPoint<List<EntityID>>> currentDataPoints;
            int currentNoOfMembers;

            // gather comfort need percentages - but NOT their effects
            GatherComfortNeeds(out currentDataPoints, out currentNoOfMembers);

            AddToHasSatisifedComfortNeedDataLists(currentDataPoints, currentNoOfMembers);


          
            totalComfortEffects = 0;



            ICanIterateEntities group = LookUpICanIterateEntities.FindByID(Parent.CanIterateEntitiesID);

            float tempTotal = 0f;
            int members = 0;
            // NEW: comfort % from Effects etc.
            group.IterateMembers(e =>
            {
                if (GroupStatistics.GatherStatisticsForEntity(e)) // Parent.GatherStatsForMember(e)) // don't count robots...
                {
                    tempTotal += e.Intelligence.Comfort;
                    members++;
                }
            });

            totalComfortEffects = tempTotal;
            if (members > 0)
            {
                totalComfortEffects /= members;
            }
            

            hasSatisifiedComfort = new Dictionary<NeedTypeID, float>(); 

            // OLD: static comfort % per need - NEW: just %, doesn't do much here...
             NeedType needType;
            foreach (var needTypeData in HasSatisifedComfortNeedMemberPercentage)
            {
                needType = LookUp<NeedType, NeedTypeID>.FindByID(needTypeData.Key);
                               
                float hasSatisifedComfortPercentage = needTypeData.Value.Last().Value;
                
                if (hasSatisifiedComfort != null)
                {
                    float percentage = 0.01f * hasSatisifedComfortPercentage;
                    hasSatisifiedComfort[needTypeData.Key] = percentage;

                    /* OLD: static comfort % per need 
                    rating = needType.ComfortEffects.ComfortWeight * percentage; // some needs can be more important than others...
                    hasSatisifiedComfort[needTypeData.Key] = new Tuple<float, float>(percentage, rating);*/
                }

                // OLD: static comfort % per need 
               // totalNeeds += rating; 
            }
        }

        protected void ComposeRatingBreakdownForNeeds(
           // Dictionary<NeedTypeID, Tuple<float, float>> hasComfortNeedsMet, 
            Dictionary<NeedTypeID, float> hasComfortNeedsMet, 
            float needsScore,
            StringBuilder text, bool includeMemberPercentage = true)
        {
          
            if (hasComfortNeedsMet != null)
            {
                bool hasAddedHeader = false;
                foreach (var item in hasComfortNeedsMet)
                {
                   /* if (!Common.IsZero(item.Value.Item2))
                    {*/
                        if (!hasAddedHeader)
                        {
                            Common.AppendLine(text);
                            Common.AppendLine(text, "COMFORT NEEDS MET");
                            hasAddedHeader = true;
                        }

                        NeedType needType = LookUp<NeedType, NeedTypeID>.FindByID(item.Key);
                        text.Append(needType.ToString());
                        text.Append(": ");
                        if (includeMemberPercentage)
                        {
                            text.Append("Members ");
                            text.Append(Common.PercentageToString(item.Value));
                           // text.Append(", ");
                        }
                      /*  text.Append("Contribution +");
                        text.Append(Common.PercentageToString(item.Value.Item2));
                        */
                  //  }
                }

                if (hasAddedHeader)
                {
                    Common.AppendLine(text);
                    text.Append("Subscore: +");
                    Common.AppendLine(text, Common.PercentageToString(needsScore, useColoring: true));
                }
            }
           
        }

      /*  protected void ComposeRatingBreakdownForNeeds(
          Dictionary<NeedTypeID, Tuple<float, float>> hasComfortNeedsMet, 
         // Dictionary<NeedTypeID, float> hasComfortNeedsMet,
          float needsScore,
          StringBuilder text, bool includeMemberPercentage = true)
        {

            if (hasComfortNeedsMet != null)
            {
                bool hasAddedHeader = false;
                foreach (var item in hasComfortNeedsMet)
                {
                    if (!Common.IsZero(item.Value.Item2))
                    {
                        if (!hasAddedHeader)
                        {
                            Common.AppendLine(text);
                            Common.AppendLine(text, "COMFORT NEEDS MET");
                            hasAddedHeader = true;
                        }

                        NeedType needType = LookUp<NeedType, NeedTypeID>.FindByID(item.Key);
                        text.Append(needType.ToString());
                        text.Append(": ");
                        if (includeMemberPercentage)
                        {
                            text.Append("Members ");
                            text.Append(Common.PercentageToString(item.Value.Item1));
                             text.Append(", ");
                        }
                          text.Append("Contribution +");
                          text.Append(Common.PercentageToString(item.Value.Item2));
                          Common.AppendLine(text);
                    }
                }

                if (hasAddedHeader)
                {
                    text.Append("Subscore: +");
                    Common.AppendLine(text, Common.PercentageToString(needsScore));
                }
            }

        }*/


        protected static void CombineScores(float homeComfort, float totalNeeds, float sharedRating, out float rating)
        {
           // float totalNeedsScore = totalNeeds / 100f;

            rating = homeComfort + totalNeeds + sharedRating; // totalNeedsScore;

            rating = Common.Clamp(rating, 0f, 1f);
        }

        /// <summary>
        /// poll the members - add datapoints if starving, both as absolute number and as percentage
        /// </summary>
    /*    protected void GatherComfortNeeds(out Dictionary<NeedTypeID, DataPoint<List<EntityID>>> currentDataPointsOut, out int currentNoOfMembers) 
        {
            // temp variables needed for lambda function
            Dictionary<NeedTypeID, DataPoint<List<EntityID>>> currentDataPoints = new Dictionary<NeedTypeID, DataPoint<List<EntityID>>>(); 
            int noOfMembers = 0;

            DataPoint<List<EntityID>> needDataPoint;


            ICanIterateEntities group = LookUpICanIterateEntities.FindByID(Parent.GroupID);

            // adds the ID for each member that has comfort needs satisfied to some degree
            group.IterateMembers(e =>
            {
                if (
                    GroupStatistics.GatherStatisticsForEntity(e))
                {
                    currentDataPoints.Add( e.Intelligence.Comfort


                    if (e.SimEffects != null)
                    {
                        if (e.SimEffects.Effects.TryGetValue(SimEffects.Affects.AgentComfort )
                        {

                        }

                    }

                    BiologicalEntity bioEntity;
                    if (e.Find(out bioEntity))
                    {
                        NeedTypeID? needType = null;

                        noOfMembers++;

                        foreach (var need in bioEntity.Needs.NeedsList)
                        {
                            if (need.Value.NeedType.ComfortEffects != null)
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

                                if (need.Value.CurrentLevel > 0f)
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
        }*/

        

        /// <summary>
        /// poll the members - add datapoints if starving, both as absolute number and as percentage
        /// </summary>
        protected void GatherComfortNeeds(out Dictionary<NeedTypeID, DataPoint<List<EntityID>>> currentDataPointsOut, out int currentNoOfMembers) //, out NeedType needTypeOut)
        {
            // temp variables needed for lambda function
            Dictionary<NeedTypeID, DataPoint<List<EntityID>>> currentDataPoints = new Dictionary<NeedTypeID, DataPoint<List<EntityID>>>(); 
            int noOfMembers = 0;

            DataPoint<List<EntityID>> needDataPoint;


            ICanIterateEntities group = LookUpICanIterateEntities.FindByID(Parent.CanIterateEntitiesID);

            // adds the ID for each member that has comfort needs satisfied to some degree
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
                            if (need.Value.NeedType.GivesComfortEffects()) // .ComfortEffects != null)                                 
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

                                if (need.Value.CurrentLevel > 0f) 
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


        private void AddToHasSatisifedComfortNeedDataLists(Dictionary<NeedTypeID, DataPoint<List<EntityID>>> hasSatisfiedComfortPerNeed, int currentNoOfMembers)
        {
            foreach (var item in hasSatisfiedComfortPerNeed)
            {
                /*
                //Adds each individual to list of starving individuals
               
                //Adds number of starving people to list
                Common.AddToMultiList(starvingMemberCount, item.Key, new DataPoint<float>()
                {
                    Time = The.Sim.DateAndTime.CurrentTimeDateYear,
                    Value = item.Value.Value.Count
                });
                */
                //Adds percentage of starving people to list

                float percentage = ((float)item.Value.Value.Count / currentNoOfMembers) * 100f;

                Common.AddToMultiList(HasSatisifedComfortNeedMemberPercentage, item.Key, new DataPoint<float>()
                {
                    Time = The.Sim.DateAndTime.CurrentTimeDateYear,
                    Value = percentage
                });
            }
        }

        /// <summary>
        /// poll the members 
        /// </summary>
        protected void GatherHousingComfort(out int currentNoOfMembers, out float homeComfort, out string homeName) //out Dictionary<NeedTypeID, DataPoint<List<EntityID>>> currentDataPointsOut, ) 
        {
            ICanIterateEntities group = LookUpICanIterateEntities.FindByID(Parent.CanIterateEntitiesID);
            Allegiance allegiance = group.GetAllegiance;

            DataPoint<float> newDataPoint = new DataPoint<float>();
            int noOfMembers = 0;

            homeName = null;
            string tempHomeName = null;

            float totalHousingComfort = 0f;

            group.IterateMembers(e =>
            {
                if (GroupStatistics.GatherStatisticsForEntity(e)) // Parent.GatherStatsForMember(e)) // don't count robots...
                {
                    if (e.EntityType.Person != null) // .BiologicalType != null)
                    {
                        Person person = e.PersonEntity;

                        float housingComfort;
                        if (person.Household != null && person.Household.Home != null)
                        {
                            IKnownEntityData homeData;
                            if (!GoalEvaluator.EntityDataResultCausesSkip(allegiance.SharedKnowledge.GetKnownData(person.Household.Home.Value, out homeData)))
                            {
                                tempHomeName = homeData.EntityType.Name;
                                housingComfort = homeData.ComfortLevel.Value; // (float)Residence.GetComfortRating(homeData); 
                            }
                            else
                            {
                                person.Household.Home = null;
                                housingComfort = 0f;
                            }
                        }
                        else
                        {
                            // Home? I have no home. I live like an animal...
                            housingComfort = 0f;
                        }

                        totalHousingComfort += housingComfort;

                        noOfMembers++;
                    }
                    else if (e.EntityType.BiologicalType != null)
                    {
                        // TODO: score animal dwellings too.
                        totalHousingComfort += 1f;
                        noOfMembers++;
                    }
                }
               

            });

            homeName = tempHomeName ?? "Open air";

            newDataPoint.Time = The.Sim.DateAndTime.CurrentTimeDateYear;
            homeComfort = totalHousingComfort;
            if (noOfMembers > 0)
            {
                homeComfort /= noOfMembers;
                newDataPoint.Value = homeComfort;
            }
            else
            {
                newDataPoint.Value = 0f;
            }

         //   AverageHomeComfort.Add(newDataPoint);

            currentNoOfMembers = noOfMembers;
        }



        #region ISnapshot

        Snapshotter.Version version;       
        public override Snapshotter.Version DoVersion(Snapshotter sn)
        {
            base.DoVersion(sn); // each class in the class hierarchy snapshots and maintains their own version.

            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }

        public override ISnapshot DoSnapshot(Snapshotter sn)
        {
            base.DoSnapshot(sn);

           // this.AverageHomeComfort = sn.DoList(AverageHomeComfort);
            this.HasSatisifedComfortNeedMemberPercentage = sn.DoMultiMap(HasSatisifedComfortNeedMemberPercentage);

            return this;
        }

      



        #endregion
    }
}
