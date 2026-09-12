using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.AI.Constants.Rating;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;
using WindowSystem;

namespace UWGame.SimSide.Allegiances.Statistics
{
    public abstract class SecurityStatistics : Rating // Statistic,  IHasRating
    {

        #region Raw Data
        #region events

        protected List<DataPoint<ViolentEvent>> violentDeaths = new List<DataPoint<ViolentEvent>>();
        protected List<DataPoint<ViolentEvent>> violentInjuries = new List<DataPoint<ViolentEvent>>();

        #endregion
        #region Polled data

        // List<DataPoint<float>> weaponStocks = new List<DataPoint<float>>();

        #endregion
        #endregion

        #region Computed Data

        protected List<DataPoint<float>> violentDeathCounts = new List<DataPoint<float>>();
        protected List<DataPoint<float>> violentInjuriesCounts = new List<DataPoint<float>>();
     /*   protected Dictionary<DateAndTime.TimeDateYear, float> violentDeathAverages = new Dictionary<DateAndTime.TimeDateYear, float>();
        protected Dictionary<DateAndTime.TimeDateYear, float> violentInjuriesAverages = new Dictionary<DateAndTime.TimeDateYear, float>();
        */


        #endregion


      
        public SecurityStatistics()         
        {

        }

        public SecurityStatistics(GroupStatistics parent)
            : base(0.2)
        { 
            Parent = parent;
            StatType = StatTypes.Security;
        }

       



      

        /// <summary>
        /// Adds either a death or injury to the appropriate list.
        /// </summary>
        public void AddViolentEvent(Entity victim, string description, ViolentEventType type)
        {
            if (GroupStatistics.GatherStatisticsForEntity(victim))
            {
                ViolentEvent eventToAdd = new ViolentEvent()
                {
                    EventType = type, //ViolentEventType.Injury,
                    Victim = victim.EntityID,
                    Name = victim.ToString(),
                    Description = description,
                    Time = The.Sim.DateAndTime.CurrentTimeDateYear
                };

                DataPoint<ViolentEvent> dataPoint = new DataPoint<ViolentEvent>()
                {
                    Value = eventToAdd,
                    Time = The.Sim.DateAndTime.CurrentTimeDateYear
                };

                if (eventToAdd.EventType == ViolentEventType.Death)
                {
                    Common.AddToList(ref violentDeaths, dataPoint);
                }
                else if (eventToAdd.EventType == ViolentEventType.Injury)
                {
                    Common.AddToList(ref violentInjuries, dataPoint);
                }
            }
        }

        public override string ToString()
        {
            string result = RatingStatisticToString(GetLatestValue());

            return result;
        }

        protected void ComposeDeathsBreakdown(int noOfMembers, int injuries, int deaths, float injuryRating, float deathRating, StringBuilder text, Security sec)
        {
            if (injuries > 0)
            {
                Common.AppendLine(text);
                Common.AppendLine(text, "RECENT INJURIES");               
                //text.AppendFormat("Number of injuries: {0:N0} in last {1:N1} days", injuries, sec.DaysForInjuriesToAffect);
                text.Append("Number of injuries: ");
                Common.Append(text, injuries.ToString(), true);
                Common.Append(text, " in last ");
                Common.AppendFormat(text, "{0:N1}", true, sec.DaysForInjuriesToAffect);
                Common.Append(text, " days");
                Common.AppendLine(text);
                Common.Append(text, "Subscore: -");
                Common.AppendPercentage(text, injuryRating, true, Common.ValueTint.Negative);
                //text.Append(Common.PercentageToString(injuryRating, useColoring: true, valueTint: Common.ValueTint.Negative));
                Common.AppendLine(text);
            }

            if (deaths > 0)
            {
                Common.AppendLine(text);
                Common.AppendLine(text, "RECENT DEATHS");              
               // text.AppendFormat("Number of deaths: {0:N0} in last {1:N1} days", deaths, sec.DaysForDeathsToAffect);
                Common.Append(text, "Number of deaths: ");
                Common.Append(text, deaths.ToString(), true);
                Common.Append(text, " in last ");
                Common.AppendFormat(text, "{0:N1}", true, sec.DaysForDeathsToAffect);
                Common.Append(text, " days");
                Common.AppendLine(text);
                Common.Append(text, "Subscore: -");
                Common.AppendPercentage(text, deathRating, true, Common.ValueTint.Negative);
               // text.Append(Common.PercentageToString(deathRating, useColoring: true, valueTint: Common.ValueTint.Negative));
                Common.AppendLine(text);
            }
        }

        /// <summary>
        /// let this be 'damage to assets'
        /// </summary>
        /// <param name="noOfMembers"></param>
        /// <param name="relevantInjuries"></param>
        /// <param name="relevantDeaths"></param>
        /// <param name="totalInjuryContribution"></param>
        /// <param name="totalDeathsContribution"></param>
        protected void ComputeInjuriesAndDeaths(float assets /* int noOfMembers*/, out int relevantInjuries, out int relevantDeaths,
            out float totalInjuryContribution, out float totalDeathsContribution, out float finalInjuryContribution, out float finalDeathsContribution)
        {
            //Creates the TimeDateYear from which to score by taking the current time and subtracting the scoring interval.
            
            DateAndTime.TimeDateYear atTime = The.Sim.DateAndTime.CurrentTimeDateYear; // GetTimeDateYear(timePointIndex);


            Security sec = GameData.Instance.AIConstants.Ratings.Security;
            //double relevantIntervalInDays = sec.DaysForDeathsToAffect; // 3;
            DateAndTime.TimeDateYear relevantDeathsFromTime = atTime;
            relevantDeathsFromTime.AddTime(-sec.DaysForDeathsToAffect);

            DateAndTime.TimeDateYear relevantInjuriesFromTime = atTime;
            relevantInjuriesFromTime.AddTime(-sec.DaysForInjuriesToAffect);

            //The amount of injuries and deaths within the time limits we are scoring.
            relevantInjuries = GetDataPointsBetween<ViolentEvent>(violentInjuries, relevantInjuriesFromTime, atTime).Count;
            relevantDeaths = GetDataPointsBetween<ViolentEvent>(violentDeaths, relevantDeathsFromTime, atTime).Count;



            // perhaps use rolling average instead of this
            //Calculates the score. Subtracting .5 per death and .05 per injury.
            //rating = (1f - relevantDeaths * .5f - relevantInjuries * .05f) / 1f; // why divide by 1??
            totalDeathsContribution = relevantDeaths * sec.DeathFactor; // .5f;

            finalDeathsContribution = GetFinal(assets, totalDeathsContribution);        
            finalDeathsContribution = Common.Clamp(finalDeathsContribution, 0f, 1f);

            totalInjuryContribution = relevantInjuries * sec.InjuryFactor;

            finalInjuryContribution = GetFinal(assets, totalInjuryContribution);      
            totalInjuryContribution = Common.Clamp(totalInjuryContribution, 0f, 0.5f);

           /* if (noOfMembers > 0)
            {
                totalDeathsContribution /= noOfMembers;
            }

            totalDeathsContribution = Common.Clamp(totalDeathsContribution, 0f, 1f);

            totalInjuryContribution = relevantInjuries * sec.InjuryFactor; // .05f;
            if (noOfMembers > 0)
            {
                totalInjuryContribution /= noOfMembers;
            }

            totalInjuryContribution = Common.Clamp(totalInjuryContribution, 0f, 0.5f);*/
        }

        protected float GetAssets()
        {
            int noOfMembers = GetMembers();

            return noOfMembers;
        }


        private float GetFinal(float assets, float total)
        {
            if (!Common.IsZero(assets)) 
            {
                return total / assets;
            }
            else
            {
                return total;
            }

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

           // this.violentDeathAverages = sn.DoDictionary(violentDeathAverages);
            this.violentDeathCounts = sn.DoList(violentDeathCounts);
            this.violentDeaths = sn.DoList(violentDeaths);

            this.violentInjuries = sn.DoList(violentInjuries);
          //  this.violentInjuriesAverages = sn.DoDictionary(violentInjuriesAverages);
            this.violentInjuriesCounts = sn.DoList(violentInjuriesCounts);

            return this;
        }



        #endregion

    }
}
