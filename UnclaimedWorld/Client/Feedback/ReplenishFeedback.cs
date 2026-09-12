using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Systems;

namespace UWGame.ClientSide.Feedback
{
    public class ReplenishFeedback: ISleepingUpdatable
    {
        /// <summary>
        /// if after this time there has not been another agent update, the flags will be cleared.
        /// </summary>
        private const double timeInSecondsToRevert = 10;

        /// <summary>
        /// NEW: if bulk is not available, this property will show the minimum that is required.
        /// </summary>
        public float? MinimumAmountRequired
        {
            get;
            private set;
        }

        //not needed for just one binary value.

        /// <summary>
        /// only shows owned status, a quick first filter made in the evaluator
        /// </summary>
        public bool OwnsItem
        {
            get;
            private set;
        }


        /// <summary>
        /// most accurate, evaluates locks etc.
        /// </summary>
        public bool ItemIsAvailable
        {
            get;
            private set;
        }

        /// <summary>
        /// used in cleanup
        /// </summary>
        public ExpeditionID Expedition;
        public EntityType EntityType;


        /// <summary>
        /// expiry
        /// </summary>
        public double? TimePointInSeconds
        {
            get;
            private set;
        }



        public ReplenishFeedback(Expedition expedition, EntityType entityType)
        {
            this.Expedition = expedition.ID;
            this.EntityType = entityType;

            OwnsItem = true;
            ItemIsAvailable = true;
        }


        public void SetOwnsItem(bool value, float? requiredAmount)
        {
            RefreshExpiry(); // don't expire

            if (value)
            {                             
                OwnsItem = true;                
            }
            else
            {
                OwnsItem = false;
            }

            SetRequiredAmount(value, requiredAmount);
        }

        public void SetItemIsAvailable(bool value, float? requiredAmount)
        {
            RefreshExpiry(); // don't expire

            ItemIsAvailable = value;

            SetRequiredAmount(value, requiredAmount);
        }

        private void SetRequiredAmount(bool value, float? requiredAmount)
        {
            if (value == true)
            {
                MinimumAmountRequired = null; // reset
            }
            else
            {                
                if (MinimumAmountRequired.HasValue)
                {
                    if (requiredAmount < MinimumAmountRequired.Value)
                    {
                        MinimumAmountRequired = requiredAmount;
                    }
                }
                else
                {
                    MinimumAmountRequired = requiredAmount;
                }
            }
        }


        public void RefreshExpiry()
        {
            SleepyUpdater<ReplenishFeedback> updater = LookUpSleepyUpdater<ReplenishFeedback>.FindByID(SleepyUpdater);
            if (updater != null)
            {
                updater.NotifyUpdateIntervalChanged(this);  // this makes the sleepy updater compute a new expiry timepoint and resort the list:
            }
        }


        #region ISleepingUpdatable

        public void SetNextTimepoint(double? timepoint)
        {
            TimePointInSeconds = timepoint;
        }


        public double? UpdateInterval
        {
            get { return timeInSecondsToRevert; }
        }

        void ISleepingUpdatable.CreateSleepyLookupCollection()
        {

        }

        public static void CreateSleepyLookupCollection()
        {
            LookUpSleepyUpdater<ReplenishFeedback>.Create();
        }


        public SleepyUpdaterID SleepyUpdater { get; set; }

        public void Update(Microsoft.Xna.Framework.GameTime gameTime, out bool wasDestroyed)
        {
            UpdateExpiry(out wasDestroyed);
        }


        /// <summary>
        /// the object expires x seconds after the last agent update was received.
        /// </summary>
        /// <param name="wasDestroyed"></param>
        private void UpdateExpiry(out bool wasDestroyed)
        {
            wasDestroyed = false;

            if (The.Sim.TimepointReached(TimePointInSeconds.Value))
            {
                Destroy(); // destroy directly
                wasDestroyed = true;
            }
        }


        private void Destroy()
        {
            // The.Client.DestroyAccessibility(this);
            The.Client.Feedback.DestroyReplenishFeedback(this);
        }

        public void RecomputeUpdateInterval(out bool intervalChanged)
        {
            // never change the expiry interval.
            intervalChanged = false;

            /*
            if (!Common.IsEqual(UpdateInterval, currentInterval))
            {
                UpdateInterval = currentInterval; // if changed, will alert the sleepy updater to resort the list

                intervalChanged = true;
            }*/
        }

        #endregion

    }
}
