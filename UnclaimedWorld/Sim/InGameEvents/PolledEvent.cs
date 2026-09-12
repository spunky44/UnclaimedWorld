using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Systems;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;
using System.Diagnostics;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Expeditions;

namespace UWGame.SimSide.InGameEvents
{
    /// <summary>
    /// the instance part of a global condition event. 
    /// These can be global, or can be attached to an Entity, Expedition or Allegiance
    /// </summary>
    [DebuggerDisplay("{PolledEventType.KeyName} time:{timePointInSeconds}, interval:{updateInterval}")]
    public class PolledEvent : ISleepingUpdatable, ISnapshot 
    {
     
        public PolledEventType PolledEventType;

        /// <summary>
        /// one of these may be filled, and will be passed along as TriggeringEntity...
        /// 
        /// if we implement IhasExposedPropertiesID, these could become one
        /// </summary>
        public EntityID? SourceEntity;
        public ExpeditionID? SourceExpedition;
        public AllegianceID? SourceAllegiance;

        private bool hasUpdatedOnce = false;

        double? timePointInSeconds;

        /// <summary>
        /// the time to execute this action
        /// </summary>
        public double? TimePointInSeconds
        {
            get
            {
                return timePointInSeconds;
            }
        }

        public void SetNextTimepoint(double? timepoint)
        {
            this.timePointInSeconds = timepoint;
        }


        void ISleepingUpdatable.CreateSleepyLookupCollection()
        {

        }

        public static void CreateSleepyLookupCollection()
        {
            LookUpSleepyUpdater<PolledEvent>.Create();
        }

       
        public SleepyUpdaterID SleepyUpdater { get; set; }

        private double? updateInterval;
        public double? UpdateInterval
        {
            get
            {
                return updateInterval;
            }

            private set
            {
                if (!Common.IsEqual(updateInterval, value))
                {
                    updateInterval = value;

                    SleepyUpdater<PolledEvent> updater = LookUpSleepyUpdater<PolledEvent>.FindByID(SleepyUpdater); // LookUpSleepyUpdater<GlobalCondition>.FindByID(SleepyUpdater);
                    if (updater != null)
                    {
                        updater.NotifyUpdateIntervalChanged(this);  // this makes the sleepy updater compute a new expiry timepoint and resort the list:
                    }
                }
            }
        }


        public PolledEvent(PolledEventType type, 
            EntityID? sourceEntity = null, ExpeditionID? sourceExpedition = null, AllegianceID? sourceAllegiance = null)
        {
            this.PolledEventType = type;
            this.SourceEntity = sourceEntity;
            this.SourceExpedition = sourceExpedition;
            this.SourceAllegiance = sourceAllegiance;

            bool intervalChanged = false;
            RecomputeUpdateInterval(out intervalChanged);
        }

        public PolledEvent()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");    
        }

        private void Destroy()
        {
            The.Sim.PlaySite.EventManager.RemovePolledEvent(this);
        }

        public void Update(Microsoft.Xna.Framework.GameTime gameTime, out bool wasDestroyed)
        {
            wasDestroyed = false;

            Entity triggeringEntity = null;

         
            IHasExposedProperties polledEventSource = null;
            if (SourceEntity.HasValue)
            {
                polledEventSource = Entity.FindByID(SourceEntity.Value);
            }
            else if (SourceExpedition.HasValue)
            {
                polledEventSource = Expedition.FindByID(SourceExpedition.Value);
            }
            else if (SourceAllegiance.HasValue)
            {
                polledEventSource = LookUp<Allegiance, AllegianceID>.FindByID(SourceAllegiance.Value);
            }

            if (PolledEventType.Condition == null || PolledEventType.Condition.IsFulfilled(ref triggeringEntity, null, polledEventSource, null))
            {   
               
                PolledEventType.Fire(triggeringEntity, polledEventSource, out wasDestroyed);
                /*
              //  if (PolledEventType.ConditionSet != null && PolledEventType.ConditionSet.IsTimeCondition) // #POLLCHANGE
                if (PolledEventType.PollInterval == null)
                {
                    wasDestroyed = true; // only allow time conditions one chance to fire after it is time
                }

                if (wasDestroyed
                    && The.Sim != null) // the game can have ended here
                {
                    // remove the condition object if the actions are expended, it is no longer needed:
                    Destroy();                  
                }*/
            }

           
            if (PolledEventType.PollInterval == null && PolledEventType.UseDefaultPollInterval == false)// #POLLCHANGE
            {
                wasDestroyed = true; // only allow time conditions one chance to fire after it is time
            }

            if (wasDestroyed
                && The.Sim != null) // the game can have ended here
            {
                // remove the condition object if the actions are expended, it is no longer needed:
                Destroy();
            }

        }

        public override string ToString()
        {
            return PolledEventType.KeyName;
        }



        public void RecomputeUpdateInterval(out bool intervalChanged)
        {
         
            intervalChanged = false;

            double? tempInterval = null, currentInterval = null;

            if (!hasUpdatedOnce)
            {
                if (PolledEventType.StartTimePoint != null)
                {                   
                    // only 1st time!
                    tempInterval = PolledEventType.StartTimePoint.GetUpdateInterval();
                    UpdateTimePoints.GetSoonestInterval(tempInterval, ref currentInterval);
                }
                else if (PolledEventType.StartAfterInterval == true)
                {
                    tempInterval = PolledEventType.GetPollIntervalUpdateInterval(); 
                    UpdateTimePoints.GetSoonestInterval(tempInterval, ref currentInterval);               
                }

                if (currentInterval == null)
                {
                    currentInterval = 0; // start at 0 seconds per default...
                }

                hasUpdatedOnce = true;
            }
            else
            {
                tempInterval = PolledEventType.GetPollIntervalUpdateInterval(); // #POLLCHANGE - only 2nd time
                UpdateTimePoints.GetSoonestInterval(tempInterval, ref currentInterval);
                
                if (currentInterval == null)
                {
                    currentInterval = PolledEventType.DefaultPollInterval;
                }
            }
           

            if (!Common.IsEqual(UpdateInterval, currentInterval))
            {
                UpdateInterval = currentInterval; // if changed, will alert the sleepy updater to resort the list

                intervalChanged = true;
            }
        }


        /*   public void RecomputeUpdateInterval(out bool intervalChanged)
           {
               if (this.PolledEventType.KeyName == "DEMOISLANDMAP_checkQuaditeCarcassDetected") //"SANDBOXNOMADMAP_musicTrackList") //"farmplotLoop")
               {

               }

               intervalChanged = false;

               double? tempInterval = null, currentInterval = null;

               if (PolledEventType.ConditionSet != null)
               {
                   // time conditions:
                   tempInterval = PolledEventType.ConditionSet.GetUpdateInterval();
                   UpdateTimePoints.GetSoonestInterval(tempInterval, ref currentInterval);
               }

               tempInterval = PolledEventType.GetUpdateInterval();
               UpdateTimePoints.GetSoonestInterval(tempInterval, ref currentInterval);
            

               if (currentInterval == null)
               {
                   currentInterval = PolledEventType.DefaultPollInterval;
               }

               if (!Common.IsEqual(UpdateInterval, currentInterval))
               {
                   UpdateInterval = currentInterval; // if changed, will alert the sleepy updater to resort the list

                   intervalChanged = true;
               }
           }*/


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
            this.PolledEventType = (PolledEventType)sn.DoGameData(PolledEventType);
            this.timePointInSeconds = sn.DoDoubleNullable(timePointInSeconds);
            this.updateInterval = sn.DoDoubleNullable(updateInterval);
            this.SleepyUpdater = sn.DoEnum(SleepyUpdater);

            this.hasUpdatedOnce = sn.DoBool(hasUpdatedOnce);

            this.SourceEntity = sn.DoEnumNullable(SourceEntity);
            this.SourceExpedition = sn.DoEnumNullable(SourceExpedition);
            this.SourceAllegiance = sn.DoEnumNullable(SourceAllegiance);


            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);


        }

        #endregion

    }
}
