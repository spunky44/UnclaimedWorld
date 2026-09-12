using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Entities;
using WindowSystem;

namespace UWGame.SimSide.Allegiances.Statistics
{

    public enum StatTypes { Security, Comfort, Food, Population }

   
    public class DataPoint<T> : ISnapshot, IComparable
    {
        public DateAndTime.TimeDateYear Time;
        public T Value;


        public DataPoint()
        {
        }

        public DataPoint(T value)
        {
            this.Value = value;
        }

        public DataPoint(T value, DateAndTime.TimeDateYear time)
        {
            this.Value = value;
            this.Time = time;
        }

        static DataPoint()
        {
            // optimization: compute and save the type info for later.
            Snapshotter.GetStaticTypeInfo<T>(out genericTypeInfo);
        }

        /// <summary>
        /// let's cache the type information instead of retrieving it for every element in a list.
        /// </summary>
        static Snapshotter.TypeInformation genericTypeInfo;

        public int CompareTo(object obj)
        {
            return Time.CompareTo(((DataPoint<T>)obj).Time);
        }


        #region ISnapshot

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


        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            //this.snapshotPollInterval = sn.DoDouble(snapshotPollInterval);

            Value = (T)sn.DoElement(genericTypeInfo, Value); // we cannot accomodate nested generic types (like collections)

            this.Time = sn.DoTimeDateYear(Time);

            sn.Ignore(genericTypeInfo);

            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            if (genericTypeInfo.IsSnapshot)
            {
                ((ISnapshot)Value).LoadPostProcess(sn);
            }
        }

        #endregion




    }


    /// <summary>
    /// base class for any sort of statistics that should be polled regularly
    /// 
    /// data is stored in derived classes...
    /// </summary>
    public abstract class Statistic : ISnapshot
    {


        public GroupStatistics Parent;


        // protected double lastRating;

        public StatTypes StatType;

        private Regulator pollRegulator;
        double snapshotPollInterval;

       // private bool isFirstUpdate = true;
        
        protected Statistic()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }

        public Statistic(double pollInterval)
        {
            this.snapshotPollInterval = pollInterval; // saved for after snapshot

            CreateRegulators();
        }

        private void CreateRegulators()
        {
            pollRegulator = new Regulator(The.Sim.GameplayRandomGenerator, snapshotPollInterval, "Statistics");
        }

        public virtual void Update(GameTime gameTime)
        {
            if (/*isFirstUpdate ||*/ pollRegulator.IsReady())
            {
                GatherPolledData();

                //isFirstUpdate = false;
            }
        }

        public virtual void ChangeAllegiance(Allegiance newAllegiance)
        {

        }

        protected int GetMembers()
        {
            int noOfMembers = 0;
            ICanIterateEntities group = LookUpICanIterateEntities.FindByID(Parent.CanIterateEntitiesID);

            group.IterateMembers(e =>
            {
                /* if (e.EntityType.IntelligenceType.CanUseWeapons == true)
                 {*/
                noOfMembers++;
                // }
            });

            return noOfMembers;
        }


        /// <summary>
        /// override to gather and store Sim data periodically
        /// </summary>
        public virtual void GatherPolledData()
        {

        }

       
       /* public static string StatTypeToString(StatTypes statType)
        {
            switch(statType)
            {
                case StatTypes.Comfort:
                    return "Comfort";
                case StatTypes.Food:
                    return "Food";
                case StatTypes.Security:
                    return "Security";

                default: return "";
            }

        }*/

        public static string RatingsTypeToString(RatingTypes rating)
        {
            switch (rating)
            {
                case RatingTypes.Comfort:
                    return "Comfort";
                case RatingTypes.Food:
                    return "Food";
                case RatingTypes.Security:
                    return "Security";

                default: return "";
            }

        }

        public static string RatingsTypeToDescription(RatingTypes rating)
        {
            switch (rating)
            {
                case RatingTypes.Comfort:
                    return "Represents basic needs such as shelter against the environment, prevention of disease as well as other needs like entertainment and luxury items.";
                case RatingTypes.Food:
                    return "This area covers food availability, stock size, hunger risk and variety.";
                case RatingTypes.Security:
                    return "The safety of the colony or individual against living threats, human or alien.";

                default: return "";
            }

        }

        public static void DiscardOldData<T>(List<DataPoint<T>> itemGroup, DateAndTime.TimeDateYear oldestDataToKeep)
        {
            if (itemGroup.Count > 0)
            {
                DataPoint<T> first = itemGroup[0];
                while (first.Time.CompareTo(oldestDataToKeep) < 0)
                {
                    itemGroup.RemoveAt(0);

                    if (itemGroup.Count > 0)
                    {
                        first = itemGroup[0];
                    }
                    else
                    {
                        break;
                    }
                }
            }

        }

        /// <summary>
        /// appends [icon] FOOD for dynamic text
        /// </summary>
        /// <param name="rating"></param>
        /// <returns></returns>
        public static void AppendRatingsTypeToStringAndIcon(StringBuilder text, RatingTypes rating)
        {           
            text.Append(Icon.ToIcon(Statistic.RatingsTypeToIcon(rating), RatingsTypeToColor(rating)));
            text.Append(Statistic.RatingsTypeToString(rating).ToUpper(Config.Culture));
        }

        /// <summary>
        /// appends [icon] FOOD  for dynamic text
        /// </summary>
        /// <param name="rating"></param>
        /// <returns></returns>
        public static string AppendRatingsTypeToStringAndIcon(RatingTypes rating)
        {
            StringBuilder text = new StringBuilder();
            AppendRatingsTypeToStringAndIcon(text, rating);

            return text.ToString();
        }

        public static string RatingsTypeToIcon(RatingTypes rating)
        {
            switch (rating)
            {
                case RatingTypes.Comfort:
                    return "lcd_icon_comfort";
                case RatingTypes.Food:
                    return "lcd_icon_nutrition";
                case RatingTypes.Security:
                    return "lcd_icon_security";

                default: return "";
            }

        }

        public static string RatingsTypeToColor(RatingTypes rating)
        {
            switch (rating)
            {
                case RatingTypes.Comfort:
                    return GameData.Instance.GUIConstants.ComfortColorConstant;
                case RatingTypes.Food:
                    return GameData.Instance.GUIConstants.FoodColorConstant;
                case RatingTypes.Security:
                    return GameData.Instance.GUIConstants.SecurityColorConstant;

                default: return "";
            }

        }

        /// <summary>
        /// for use by scripts
        /// </summary>
        /// <param name="rating"></param>
        /// <returns></returns>
        public static string RatingsTypeToKey(RatingTypes rating)
        {
            switch (rating)
            {
                case RatingTypes.Comfort:
                    return "comfort";
                case RatingTypes.Food:
                    return "food";
                case RatingTypes.Security:
                    return "security";

                default: return "";
            }

        }

       
        protected void AppendComponent(StringBuilder text, string caption, float value, string prefix = null, string suffix = null,
            bool indent = false, bool omitIfZero = true, bool formatAsPercentage = false, bool formatAsInteger = false, Common.ValueTint? valueTint = null)
        {
            if (omitIfZero && Common.IsZero(value))
            {
                return;
            }
            else
            {
                if (indent)
                {
                    text.Append(Common.indentString);
                }
                text.Append(caption);
                text.Append(": ");

                if (prefix != null)
                {
                    text.Append(prefix);
                }

                string valueAsString;
                if (formatAsPercentage)
                {
                    valueAsString = Common.PercentageToString(value, useColoring: true, valueTint: valueTint);
                }
                else if (formatAsInteger)
                {
                    valueAsString = Common.ValueToIntegerString(value, true, valueTint);
                }
                else
                {
                    valueAsString = FormatRatingComponent(value);
                }
                text.Append(valueAsString);

                if (suffix != null)
                {
                    text.Append(suffix);
                }

                Common.AppendLine(text);
            }
        }
       
        protected string FormatRatingComponent(float value)
        {
            return value.ToString("N2");
        }

        public static int SumDataPoints(Dictionary<EntityType, List<DataPoint<float>>> dict, EntityType entityType, DateAndTime.TimeDateYear from, DateAndTime.TimeDateYear to)
        {
            List<DataPoint<float>> data;

            if (dict.TryGetValue(entityType, out data))
            {

                List<DataPoint<float>> list = Statistic.GetDataPointsBetween(data, from, to);

                float sum = list.Sum(d => d.Value);

                return (int)sum;
            }

            return 0;
        }

        public static float GetMeanOfDataPoints(Dictionary<EntityType, List<DataPoint<float>>> dict, EntityType entityType, DateAndTime.TimeDateYear from, DateAndTime.TimeDateYear to)
        {
            List<DataPoint<float>> data;

            if (dict.TryGetValue(entityType, out data))
            {

                List<DataPoint<float>> list = Statistic.GetDataPointsBetween(data, from, to);

                float sum = list.Average(d => d.Value);

                return sum;
            }

            return 0f;
        }

        /// <summary>
        /// Returns a list of datapoints between 2 time points.
        /// </summary>
        public static List<DataPoint<T>> GetDataPointsBetween<T>(List<DataPoint<T>> dataPoints, DateAndTime.TimeDateYear from, DateAndTime.TimeDateYear to, bool extendLastValue = false)
        {

            List<DataPoint<T>> resultList = new List<DataPoint<T>>();

            if (dataPoints.Count == 0)
            {
                return resultList;
            }

            int firstRelevantIndex;
            int lastRelevantIndex;

            GetDataPointsBetween(dataPoints, from, to, out firstRelevantIndex, out lastRelevantIndex, extendLastValue);

            //Populates the list with the datapoints between the relevant indices.
            if (firstRelevantIndex >= 0 && lastRelevantIndex >= 0)
            {
                for (int i = firstRelevantIndex; i <= lastRelevantIndex; i++)
                {
                    resultList.Add(dataPoints[i]);
                }
            }

            return resultList;
        }


        /// <summary>
        /// returns the indexes for the data points in range.
        /// if there are no datapoints in range, returns false
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataPoints"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="indexFrom"></param>
        /// <param name="indexTo"></param>
        /// <returns></returns>
        public static bool GetDataPointsBetween<T>(List<DataPoint<T>> dataPoints, DateAndTime.TimeDateYear from, DateAndTime.TimeDateYear to, out int indexFrom, out int indexTo, bool extendLastValue)
        {

            indexFrom = dataPoints.BinarySearch(new DataPoint<T>() { Time = from });

            if (indexFrom < 0)
            {
                int indexOfNearest = ~indexFrom;

                if (indexOfNearest == dataPoints.Count)
                {
                    //from time is larger than all elements
                    if (extendLastValue)
                    {
                        indexFrom = dataPoints.Count - 1; // NEW: use the last datapoint
                    }
                    else
                    {
                        indexTo = -1;
                        return false;
                    }

                }
                else if (indexOfNearest == 0)
                {
                    // from time is less than first item
                    indexFrom = 0;
                }
                else
                {
                    // from time is between (indexOfNearest - 1) and indexOfNearest
                    indexFrom = indexOfNearest;
                }
            }

            indexTo = dataPoints.BinarySearch(new DataPoint<T>() { Time = to });

            if (indexTo < 0)
            {
                int indexOfNearest = ~indexTo;

                if (indexOfNearest == dataPoints.Count)
                {
                    //to time is larger than all elements
                    indexTo = dataPoints.Count - 1;
                }
                else if (indexOfNearest == 0)
                {
                    // to time is less than first item
                    //  firstRelevantIndex = -1;
                    //   lastRelevantIndex = -1;
                    return false; // null;
                }
                else
                {
                    // to time is between (indexOfNearest - 1) and indexOfNearest
                    indexTo = Math.Max(0, indexOfNearest - 1);
                }
            }

            return true;

            /*int length = indexTo - indexFrom + 1;
            DateTime[] result = new DateTime[length];
            if (length > 0)
            {
                Array.Copy(dataPoints, indexFrom, result, 0, length);
            }
            return result;*/

        }


        public abstract float GetLatestValue();


        public abstract float GetChange();


        protected static float GetChange(List<DataPoint<float>> ratings)
        {
            if (ratings.Count > 1)
            {
                return ratings[ratings.Count - 1].Value - ratings[ratings.Count - 2].Value;
            }

            return 0f;
            
        }


        /// <summary>
        /// TODO: For ratings, don't use this. we want a more 1-to-1 result...
        /// 
        /// Returns an average of datapoints from a list between 2 specified time points.
        /// 
        /// For optimization, can reuse a previously calculated average for part of the interval.
        /// </summary>
        public float GetAverage(
            List<DataPoint<float>> list, 
            //List<DataPoint<double>> averageList, 
            DataPoint<float> lastAveragePoint,
            DateAndTime.TimeDateYear from, DateAndTime.TimeDateYear to)
        {
            
            DateAndTime.TimeDateYear adjustedFrom = from;

            float overlapPercentage = 0f;

            float lastAverage = 0f;

            //For optimization,                    
            //only get the datapoints between when last average ended and 'to'           
            if (lastAveragePoint != null
                && lastAveragePoint.Time.CompareTo(from) >= 0) //averageList.Count > 0)
            {
                lastAverage = lastAveragePoint.Value; 
        
                adjustedFrom = lastAveragePoint.Time; 

               /* double intervalInDays = DateAndTime.GetTimeDifferenceInDays(from, to);

                adjustedFrom.AddTime(intervalInDays);*/

                float lastAverageTimeInDays = (float)lastAveragePoint.Time.TotalDays;
                float newIntervalFromInDays = (float)from.TotalDays;
                float newIntervalToInDays = (float)to.TotalDays;

                // compute the overlap between previously covered average and new interval:
                overlapPercentage = (lastAverageTimeInDays - newIntervalFromInDays) / (newIntervalToInDays - newIntervalFromInDays);

                overlapPercentage = Math.Min(1f, overlapPercentage); // clamp

                //overlapPercentage = (double)overlappingDataPoints.Count / (double)(newDataPoints.Count + overlappingDataPoints.Count);

            }


            List<DataPoint<float>> newDataPoints = GetDataPointsBetween<float>(list, adjustedFrom, to);

            float newDataPointAverage = 0;

            if (newDataPoints.Count > 0)
            {
                newDataPointAverage = newDataPoints.Select(d => d.Value).Average();
            }


            float average = (lastAverage * overlapPercentage) + (newDataPointAverage * (1 - overlapPercentage));

                       
            return average;
        }

        protected string RatingStatisticToString(double rating)
        {
            return Common.PercentageToString(rating);           
        }



        #region ISnapshot

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public virtual Snapshotter.Version DoVersion(Snapshotter sn)
        {
            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }


        public bool IsSnapshotted { get; set; }


        public virtual ISnapshot DoSnapshot(Snapshotter sn)
        {
            this.snapshotPollInterval = sn.DoDouble(snapshotPollInterval);
            this.StatType = sn.DoEnum(StatType);
            // this.lastRating = sn.DoDouble(lastRating);


            sn.Ignore(Parent);

            return this;
        }

        public virtual void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            CreateRegulators();
        }

        #endregion

    }
}
