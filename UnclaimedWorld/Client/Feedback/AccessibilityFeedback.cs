using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide;
using UWGame.SimSide.Systems;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Jobs;

namespace UWGame.ClientSide.Feedback
{
    /// <summary>
    /// only used for giving player feedback
    /// </summary>
    public class AccessibilityFeedback : ISleepingUpdatable
    {

        public EntityID? EntityID;
        public JobID? JobID;

        /// <summary>
        /// if after this time there has not been another agent update, the flags will be cleared.
        /// </summary>
        private const double timeInSecondsToRevert = 5;

        /// <summary>
        /// expiry
        /// </summary>
        public double? TimePointInSeconds
        {
            get;
            private set;
        }

        /// <summary>
        /// perhaps display in the tooltip who the agent is that is trying to reach the entity/job
        /// </summary>
       // EntityID? lastUpdatingAgent;

        /// <summary>
        /// This is set for jobs that require bold stance. If no-one can take a bold stance 
        /// then the player will get feedback for this.
        /// </summary>
        public bool IsBlockedDueToBoldStanceRequired
        {
            get;
            private set;
        }

        public bool IsBlockedByThreat
        {
            get;
            private set;
        }

        /// <summary>
        /// is true if innaccessible due to terrain or threat, or any other reason
        /// </summary>
        public bool IsInaccessible
        {
            get;
            private set;
        }

        /// <summary>
        /// prey has a max distance it will get hunted
        /// </summary>
        public bool TooFarFromExpedition
        {
            get;
            private set;
        }

        public bool HuntingJobNotFeasible
        {
            get;
            private set;
        }

        public bool AreaNotCleared
        {
            get;
            private set;
        }
        

        private int noOfTimesWasInaccessible = 0;
        private int noOfTimesWasBlockedByThreat = 0;
        private int noOfTimesWasBlockedByBoldStance = 0;
        private int noOfTimesHuntingJobNotFeasible = 0;

        public AccessibilityFeedback(JobID jobID)
        {
            this.JobID = jobID;
        }

        public AccessibilityFeedback(EntityID entityID)
        {
            this.EntityID = entityID;
        }

        public void SetTooFarFromExpedition(bool value)
        {
            TooFarFromExpedition = value;
        }

        public void SetAreaNotCleared(bool value)
        {
            AreaNotCleared = value;
        }

        public void SetHuntingJobNotFeasible(IHasEntityGroup owner, bool value)
        {
            if (value)
            {
                noOfTimesHuntingJobNotFeasible++;
                if (SetClientFeedbackProperty(noOfTimesHuntingJobNotFeasible, owner))
                {
                    noOfTimesHuntingJobNotFeasible = 0;
                    HuntingJobNotFeasible = true;
                }
            }
            else
            {
                HuntingJobNotFeasible = false;
            }
        }

        public void SetBlockedByBoldStance(IHasEntityGroup owner, bool value)
        {
            if (value)
            {
                noOfTimesWasBlockedByBoldStance++;
                if (SetClientFeedbackProperty(noOfTimesWasBlockedByBoldStance, owner))
                {
                    noOfTimesWasBlockedByBoldStance = 0;
                    IsBlockedDueToBoldStanceRequired = true;
                }
            }
            else
            {
                IsBlockedDueToBoldStanceRequired = false;
            }
        }

        public void SetBlockedByThreat(IHasEntityGroup owner, bool value)
        {
            if (value)
            {
                noOfTimesWasBlockedByThreat++;
                if (SetClientFeedbackProperty(noOfTimesWasBlockedByThreat, owner))
                {
                    noOfTimesWasBlockedByThreat = 0;
                    IsBlockedByThreat = true;
                }               
            }
            else
            {
                IsBlockedByThreat = false;
            }
        }

        public void SetIsInaccessible(IHasEntityGroup owner, bool value)
        {
            if (value)
            {
                noOfTimesWasInaccessible++;
                if (SetClientFeedbackProperty(noOfTimesWasInaccessible, owner))
                {
                    noOfTimesWasInaccessible = 0;
                    IsInaccessible = true;
                }
            }
            else
            {
                IsInaccessible = false;
            }
        }

      

        /// <summary>
        /// to prevent dithering, require the value to have been set true a certain number of times relative to the number of agents that are evaluating
        //  should this go the other way also?
        /// </summary>
        /// <param name="timesSetTrue"></param>
        /// <param name="owner"></param>
        /// <returns></returns>
        private bool SetClientFeedbackProperty(int timesSetTrue, IHasEntityGroup owner)
        {
            SleepyUpdater<AccessibilityFeedback> updater = LookUpSleepyUpdater<AccessibilityFeedback>.FindByID(SleepyUpdater);
            if (updater != null)
            {
                updater.NotifyUpdateIntervalChanged(this);  // this makes the sleepy updater compute a new expiry timepoint and resort the list:
            }


            if (timesSetTrue >= owner.NoOfWorkers * 2)
            {
                return true;
            }

            return false;
        }



       

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
            LookUpSleepyUpdater<AccessibilityFeedback>.Create();
        }


        public SleepyUpdaterID SleepyUpdater { get; set; }

        public void Update(Microsoft.Xna.Framework.GameTime gameTime, out bool wasDestroyed)
        {
            UpdateExpiry(out wasDestroyed);
        }


        /// <summary>
        /// the object expires x seconds after the last agent updarte was received.
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
            The.Client.Feedback.DestroyAccessibility(this);
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
    }
}
