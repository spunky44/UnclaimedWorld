using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems;

namespace UWGame.SimSide.Resources
{
    /// <summary>
    /// takes up a lot of memory because there is a lot of these objects...
    /// </summary>
    public class ResourceReplenish: ISnapshot
    {
        private ushort maxResourceItemsEverSet;

        private double? replenishTimePointInSeconds;

        private ushort? indexOfLastReplenishPoint;


      /*  public float MaxYearlyReplenishRate
        {
            get;
            private set;
        }*/


        public void SetNextReplenishTimepoint(ResourceContainer parent)
        {
            //DateAndTime.TimeDateYear date = resourceType.ComputeReplenishDate();
            double daysFromNow = parent.ResourceType.ComputeReplenishDaysFromNow(ref indexOfLastReplenishPoint);

            replenishTimePointInSeconds = The.Sim.TotalUnPausedGameTimeInSeconds + daysFromNow * DateAndTime.secondsPerDay;

            if (Common.IsZero(daysFromNow))
            {

            }

#if DEBUG
          //  Console.WriteLine("SetNextReplenishTimepoint " + parent + " " + daysFromNow);
#endif
            // replenishTimePointInSeconds = date.ToSeconds();
        }

        public void UpdateMaxItemsEverSet(int noOfItems)
        {
            maxResourceItemsEverSet = (ushort)Common.Max(maxResourceItemsEverSet, noOfItems);
        }

        public void Update(Microsoft.Xna.Framework.GameTime gameTime, ResourceContainer parent)
        {
            if (The.Sim.TimepointReached(replenishTimePointInSeconds))
            {
                ReplenishResourceItems(parent);
            }
        }

        /// <summary>
        /// gives the rate that can be replenished with the current resource state
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public float GetCurrentReplenishRate(ResourceContainer parent, out bool maximumReached)
        {
            ResourceType resourceType = parent.ResourceType;

            float maxReplenish = GetMaximumReplenishRate(parent);

            if (maxResourceItemsEverSet <= parent.NoOfHarvestableItems)
            {
                maximumReached = true;
                return 0f; // Common.Clamp(maxReplenish, 0f, maxResourceItemsEverSet - parent.NoOfHarvestableItems);

            }
            else
            {
                maximumReached = false;
                return Common.ClampBottom(maxReplenish, 0f);
            }

          //  return Common.Clamp(maxReplenish, 0f, maxResourceItemsEverSet - parent.NoOfHarvestableItems);
            
        }

        public float GetMaximumReplenishRate(ResourceContainer parent)
        {
            ResourceType resourceType = parent.ResourceType;

            float maxReplenish = 0f;
            if (resourceType.DaysOfYearToReplenish != null)
            {

                maxReplenish = resourceType.OrderedDaysOfYearToReplenish.Count * resourceType.FractionOfMaximumToReplenishEachTime * maxResourceItemsEverSet;

                /*
                for (int i = 0; i < resourceType.OrderedDaysOfYearToReplenish.Count; i++)
			    {
                    maxReplenish += resourceType.FractionOfMaximumToReplenishEachTime * maxResourceItemsEverSet;
			    }*/                
            }

            return maxReplenish;
        }

        private void ReplenishResourceItems(ResourceContainer parent)
        {
            if (parent.NoOfHarvestableItems < maxResourceItemsEverSet)
            {
                int replenishAmount = (int)Math.Round(parent.ResourceType.FractionOfMaximumToReplenishEachTime * maxResourceItemsEverSet);

                Common.Clamp(replenishAmount, 1, maxResourceItemsEverSet - parent.NoOfHarvestableItems);

                parent.AddResourceItems(replenishAmount);
              //  parent.AddResourceItems(maxResourceItemsEverSet - parent.NoOfHarvestableItems);
            }

            SetNextReplenishTimepoint(parent);
        }

        public double? GetUpdateInterval()
        {
            double? tempInterval = null, currentInterval = null;
            
            tempInterval = GetIntervalForReplenish();
            UpdateTimePoints.GetSoonestInterval(tempInterval, ref currentInterval);

            return currentInterval;
        }

        private double? GetIntervalForReplenish()
        {
            if (replenishTimePointInSeconds.HasValue)
            {
                return UpdateTimePoints.ComputeIntervalFromTimepoint(this.replenishTimePointInSeconds);
            }

            return null;
        }

        #region ISnapshot

        public virtual ISnapshot DoSnapshot(Snapshotter sn)
        {
          
            maxResourceItemsEverSet = sn.DoUInt16(maxResourceItemsEverSet);
            replenishTimePointInSeconds = sn.DoDoubleNullable(replenishTimePointInSeconds);
            indexOfLastReplenishPoint = sn.DoUInt16Nullable(indexOfLastReplenishPoint);

         //   MaxYearlyReplenishRate = sn.DoFloat(MaxYearlyReplenishRate);

            return this;
        }

        public virtual void LoadPostProcess(Snapshotter sn)
        {

            sn.RegisterLoadPostProcessCall(this);

        }

        Snapshotter.Version version;
        public virtual Snapshotter.Version DoVersion(Snapshotter sn)
        {
            version = sn.DoVersion(Snapshotter.Version.Original);
            return version;
        }

        public bool IsSnapshotted { get; set; }

        #endregion



       
    }
}
