using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AllGameData;
using System.Diagnostics;

namespace UWGame.SimSide.Resources
{
    [DebuggerDisplay("{KeyName}")]
    public class ResourceType : IGameData, IHasCategory<ResourceCategory>, IXmlSerializable, IDetectableType
    {
        public string KeyName { get; set; }

        public string Name { get; set; }
        public bool DeleteRecord
        {
            get;
            set;
        }
       // public EntityType ResourceItem;

        /// <summary>
        /// TODO: should not be defined here. instead each process type should the resource as (one of) its inputs
        /// 
        /// for critter harvesting, define the extract processes manually...
        /// </summary>
        public string ResourceItem;

        [XmlIgnore]
        public EntityType ResourceItemType;

        public ResourceCategory Category { get; set; }

        /// <summary>
        /// only one of the below should be used:
        /// </summary>
        public TileResourceType TileResourceType;

        public CropType CropType;


        //public DateAndTime.TimeDateYear? DateToReplenish;

        /// <summary>
        /// the default is full replenish
        /// </summary>
        public float FractionOfMaximumToReplenishEachTime = 1f;

        /// <summary>
        /// for seasonal crops etc.
        ///       
        /// validate that intervals cannot overlap!
        /// </summary>
        public NormalDistribution[] DaysOfYearToReplenish;

        [XmlIgnore]
        public List<NormalDistribution> OrderedDaysOfYearToReplenish;

        public string DetectionTag;

        /*
        [XmlIgnore]
        public float MaximumReplenishRate;
        */

        
        #region Client properties

       // public bool IsRareMaterial = false;

        /// <summary>
        /// print info in this color on the editor map
        /// </summary>
        public Color? Color; // = Color.White;

       
        //  public float AvoidDetectionFactor;

        public float? DetectionFlashDuration; 

        #endregion

        



        public ResourceType(string keyName)
        {
            this.KeyName = keyName;

        }

        public ResourceType()
        {
        }

       
        public bool CanReplenish()
        {
            return DaysOfYearToReplenish != null;            
        }

        public double ComputeReplenishDaysFromNow(ref ushort? indexOfLastReplenishPoint)
        {
            DateAndTime.TimeDateYear nowTimeDateYear = The.Sim.DateAndTime.CurrentTimeDateYear;
            double nowDate = nowTimeDateYear.Day + nowTimeDateYear.TimeOfDay;

            bool addOneYear = false;
            NormalDistribution dateInfo;

            // find earliest date point:
            if (OrderedDaysOfYearToReplenish.Count > 1)
            {
                if (indexOfLastReplenishPoint == null)
                {
                    // int? lastIndexBefore
                   // double? lastDayBeforeNow = null;

                    int? nextIndex = null;
                    for (int i = 0; i < OrderedDaysOfYearToReplenish.Count; i++)
                    {
                        NormalDistribution normalDist = OrderedDaysOfYearToReplenish[i];

                        double mean = normalDist.GetMean();

                        double meanInDays = mean * DateAndTime.DaysPerYear;

                        if (meanInDays > nowDate)
                        {
                            nextIndex = i;
                            break;
                        }
                    }

                    if (nextIndex == null)
                    {
                        addOneYear = true; // we are one year ahead
                        nextIndex = 0; // OrderedDaysOfYearToReplenish.Count - 1;
                    }

                    indexOfLastReplenishPoint = (ushort)nextIndex.Value;
                }
                else
                {
                    indexOfLastReplenishPoint++;
                    if (indexOfLastReplenishPoint > DaysOfYearToReplenish.Length - 1)
                    {
                        indexOfLastReplenishPoint = 0;
                        dateInfo = OrderedDaysOfYearToReplenish[indexOfLastReplenishPoint.Value];
                        
                        if (dateInfo.GetMean() < nowDate)
                        {
                            addOneYear = true; // we are one year ahead
                        }
                    }
                }
            }
            else
            {
                indexOfLastReplenishPoint = 0;
                dateInfo = OrderedDaysOfYearToReplenish[indexOfLastReplenishPoint.Value];

                if (dateInfo.GetMean() < nowDate)
                {
                    addOneYear = true; // we are one year ahead
                }
            }


            dateInfo = OrderedDaysOfYearToReplenish[indexOfLastReplenishPoint.Value];
            
            // this can be negative...
            // allow the date to go over "New Year"
            double date = DateAndTime.DaysPerYear * dateInfo.GetRandomValue(The.Sim.GameplayRandomGenerator);           
            int year = nowTimeDateYear.Year;
            if (addOneYear)
            {
                year++;
            }

            DateAndTime.TimeDateYear nextTimeDateYear = new DateAndTime.TimeDateYear(date);
            nextTimeDateYear.AddTime(year * DateAndTime.DaysPerYear);

          /*  if (date > DateAndTime.DaysPerYear) // over New Year
            {
                // stay within a year
                nextTimeDateYear.AddTime(-DateAndTime.DaysPerYear);
                //date -= DateAndTime.DaysPerYear;
            }
            else if (date < 0)
            {
                nextTimeDateYear.AddTime(DateAndTime.DaysPerYear);
                //date += DateAndTime.DaysPerYear;
            }*/

            if (addOneYear == false)
            {
                // same year - clamp the computed date to not be before Now
                if (nextTimeDateYear.TotalDays < nowTimeDateYear.TotalDays)
                {
                    nextTimeDateYear = new DateAndTime.TimeDateYear(nowTimeDateYear.TotalDays);
                }
            }

            
            double days = nextTimeDateYear.TotalDays - nowTimeDateYear.TotalDays;

            System.Diagnostics.Debug.Assert(days >= 0, "Cannot be negative");

            return days;


            /* OLD
            // how many days from now:
            if (nowTimeDateYear.Year > 0)
            {
                // move to the year before...
                date += (nowTimeDateYear.Year - 1) * DateAndTime.DaysPerYear;
            }

            while(date < nowTotalDays)
            {
                // find the earliest date
                date += DateAndTime.DaysPerYear;
            }*/

           // return date - nowTotalDays;
            
        }


        public int GetHarvestableItemsFromBulk(float totalHarvestableBulk)
        {
            return (int)(totalHarvestableBulk / ResourceItemType.ItemType.MaximumBulk.Value);

        }
        /*
        public double ComputeReplenishDaysFromNow(ref ushort? indexOfLastReplenishPoint)
        {
            double days = DateAndTime.DaysPerYear * this.DayOfYearToReplenish.GetRandomValue(The.Sim.GameplayRandomGenerator);

            if (days > DateAndTime.DaysPerYear)
            {
                // stay within a year
                days -= DateAndTime.DaysPerYear;
            }

            // how many days from now:
            DateAndTime.TimeDateYear nowDate = The.Sim.DateAndTime.CurrentTimeDateYear;
            double now = nowDate.TotalDays;
            if (nowDate.Year > 0)
            {
                // move to the year before...
                days += (nowDate.Year - 1) * DateAndTime.DaysPerYear;
            }

            while (days < now)
            {
                // find the earliest date
                days += DateAndTime.DaysPerYear;
            }

            return days - now;

        }*/

        public void Initialize()
        {
            if (!string.IsNullOrEmpty(DetectionTag))
            {
                BaseDataLoader.AddToTagCollection(this,
                    DetectionTag, GameData.Instance.DetectableTypeByTag);
            }

            if (TileResourceType != null)
            {
                TileResourceType.Initialize();
            }

            if (DaysOfYearToReplenish != null)
            {
                OrderedDaysOfYearToReplenish = DaysOfYearToReplenish.OrderBy(d => d.GetMean()).ToList();
            }

        }


        public void PostLoadContentInitialize()
        {           
        }

        public bool ShouldSerializeColor() 
        {
            return Color != null;
        }

        public void PreInitValidate(ref List<string> errors)
        {
            EntityType.ValidateRequiredValue(ref errors, "Name", !string.IsNullOrEmpty(Name));
            EntityType.ValidateRequiredValue(ref errors, "Category", Category != null);
        
        }

        public void PostInitValidate(ref List<string> errors) 
        {
            if (OrderedDaysOfYearToReplenish != null && OrderedDaysOfYearToReplenish.Count > 1)
            {
                // validate no overlap..
                float? previousMax = null;
                foreach (var item in OrderedDaysOfYearToReplenish)
                {
                    float min = item.GetMin();
                    float max = item.GetMax();

                    if (previousMax.HasValue)
                    {
                        if (min < previousMax.Value)
                        {
                            EntityType.CreateValidationError(ref errors, "Resource replenish date intervals must not overlap.");
                        }
                    }

                    previousMax = max;
                }
            }
        }

        public void PostDataCompleteInitialize()
        {
            ResourceItemType = GameData.Instance.AllEntityTypes[ResourceItem];

            GameData.Instance.ItemHarvestSource.Add(ResourceItemType, this);

        }

        public void PreDataCompleteValidate(ref List<string> listOfErrors) { }
       
        public void PostDataCompleteValidate(ref List<string> listOfErrors)
        {
        }

        #region IXmlSerializable Members

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            CustomXmlSerializer.ReadXmlDeserialize(this, reader, _proxyData);
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            CustomXmlSerializer.WriteXmlSerialize(this, writer, _proxyData);
        }

        public static readonly CustomXmlSerializer.XmlProxyData _proxyData = new CustomXmlSerializer.XmlProxyData(typeof(ResourceType))
        {
            // use placeholders!
            TypeMappings = BaseDataLoader.GetListOfTypeMappings(true)
        };

        #endregion
    }


    public interface ResourceOrEntityType
    {

    }
}
