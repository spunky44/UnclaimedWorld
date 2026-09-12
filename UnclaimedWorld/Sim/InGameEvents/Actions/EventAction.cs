using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Systems;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.AI.Pathfinding;

namespace UWGame.SimSide.InGameEvents.Actions
{
    /// <summary>
    /// the runtime part of an event action type - is used to manage the lifetime of the event
    /// </summary>
    public class EventAction : ISleepingUpdatable, ISnapshot 
    {
        /// <summary>
        /// many EventActions can hold a reference to the same parent - so save its ID
        /// </summary>
        public ActionSetDataID? ParentID;

        public EventActionType EventActionType;
    //    EventActionTypeID snapshotEventActionType;

        /// <summary>
        /// store an id because the event may not execute immediately. if it is after a delay, the entity may be destroyed.
        /// in that case, cancel the action... (could this skip win/lose actions???)
        /// </summary>     
        public EntityID? TriggeringEntity;

        /// <summary>
        /// is filled for Process actions - the entity is the one that is being worked on
        /// </summary>
        public EntityID? TargetEntity;

        public IHasExposedProperties PolledEventSource;
        // for snapshotting:
        EntityID? polledEventSourceEntity;
        ExpeditionID? polledEventSourceExpedition;
        AllegianceID? polledEventSourceAllegiance;

        
        public IHasExposedProperties DynamicTarget;
        // for snapshotting:
        EntityID? dynamicTargetEntity;
        ExpeditionID? dynamicTargetExpedition;
        AllegianceID? dynamicTargetAllegiance;

        static HighResolutionTime timer = new HighResolutionTime();

        /// <summary>
        /// TODO: make it an ID, if we ever implements ILookup on IHasExposedProperties.
        /// We won't do it now because there are many implementors... and only a few are used as PolledEventSource.
        /// </summary>
       /* public IHasExposedProperties PolledEventSource
        {
            get
            {
                if (polledEventSourceEntity.HasValue)
                {
                    return Entity.FindByID(polledEventSourceEntity);
                }
                else if (polledEventSourceExpedition.HasValue)
                {
                    return LookUp<Expedition, ExpeditionID>.FindByID(polledEventSourceExpedition);
                }
                else if (polledEventSourceAllegiance.HasValue)
                {
                    return LookUp<Allegiance, AllegianceID>.FindByID(polledEventSourceAllegiance);
                }

                return null;
            }

            private set
            {
               

            }
        }*/




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

        public SleepyUpdaterID SleepyUpdater { get; set; }


        void ISleepingUpdatable.CreateSleepyLookupCollection()
        {

        }

        public static void CreateSleepyLookupCollection()
        {
            LookUpSleepyUpdater<EventAction>.Create();
        }

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

                    SleepyUpdater<EventAction> updater = LookUpSleepyUpdater<EventAction>.FindByID(SleepyUpdater);
                    if (updater != null)
                    {
                        updater.NotifyUpdateIntervalChanged(this);
                    }
                }
            }
        }

     
        public EventAction(EventActionType eventActionType, ActionSetData parent)
        {
            this.EventActionType = eventActionType;

            if (parent != null)
            {
                this.ParentID = parent.ID;
            }
        }

        public EventAction()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
    
        }

        public void ExecuteNowOrLater(Entity triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
        {
            TriggeringEntity = (triggeringEntity != null ? triggeringEntity.EntityID : (EntityID?)null);
            TargetEntity = targetEntity;
            PolledEventSource = polledEventSource;
            DynamicTarget = dynamicTarget;

            if (this.EventActionType.DelayInSeconds > 0f)
            {
                // execute this later...
                UpdateInterval = EventActionType.DelayInSeconds;


                The.Sim.PlaySite.EventManager.AddEventForLaterExecution(this);

            }
            else
            {
                Execute(); 
            }

        }

        

        public string Execute()
        {

            timer.Start();

            string failReason = "";
            bool success = EventActionType.Execute(this, ref failReason);

            StringBuilder logMessage = null;
            string logString = null;
            if (The.Sim != null) // the game can have ended here
            {
                logMessage = new StringBuilder(The.Sim.TotalUnPausedGameTimeInSeconds.ToString());
                logMessage.Append(" ");
                logMessage.Append(this.ToString());


                logMessage.Append((success == false ? "(NO EXEC)" + failReason : ""));
                logString = logMessage.ToString();

#if DEBUG || PROFILE


                // the dev dialog is created after some events.
                if (!Kensei.Dev.Options.AppendEventsLogText(logString))
                {
                    The.Sim.AddStartLogMessage(logString); 
                }

              //  Console.WriteLine(logString);
#endif
            }


            double timeTaken = timer.GetTime();
            if (timeTaken > 0.002)
            {

            }

            return logString;
            
            
        }


        public void Update(GameTime gameTime, out bool wasDestroyed)
        {
            UpdateDelayedFiring(out wasDestroyed);

        }

      /*  private void RecomputeUpdateInterval()
        {
            bool intervalChanged;
            RecomputeUpdateInterval(out intervalChanged);
        }*/

        public void RecomputeUpdateInterval(out bool intervalChanged)
        {
            intervalChanged = false;

            double? tempInterval = null, currentInterval = null;
            
            tempInterval = UpdateTimePoints.ComputeIntervalFromTimepoint(this.TimePointInSeconds);
            UpdateTimePoints.GetSoonestInterval(tempInterval, ref currentInterval);


            if (!Common.IsEqual(UpdateInterval, currentInterval))
            {
                UpdateInterval = currentInterval; // if changed, will alert the sleepy updater to resort the list

                intervalChanged = true;
            }
        }

        private void UpdateDelayedFiring(out bool wasDestroyed)
        {
            wasDestroyed = false;

            if (The.Sim.TimepointReached(TimePointInSeconds))
            {
                wasDestroyed = true;

                Execute();

                Destroy();

                //fired++;

                //eventActions.RemoveAt(0);
            }
        }


        private void Destroy()
        {
            The.Sim.PlaySite.EventManager.RemoveEventAction(this);
        }


        public override string ToString()
        {
            return this.EventActionType.ToString();
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
          //  EventActionType test = GameData.Instance.AllEventActionTypes[EventActionType.KeyName];

            this.EventActionType = (EventActionType)sn.DoGameData(EventActionType);
         //   this.snapshotEventActionType = (EventActionTypeID)sn.SnapshotID<EventActionType, EventActionTypeID>(EventActionType);
            this.ParentID = sn.DoEnumNullable(ParentID); 
            this.TargetEntity = sn.DoEnumNullable(TargetEntity);
            this.TriggeringEntity = sn.DoEnumNullable(TriggeringEntity);           
            this.timePointInSeconds = sn.DoDoubleNullable(timePointInSeconds);
            this.updateInterval = sn.DoDoubleNullable(updateInterval);
            this.SleepyUpdater = sn.DoEnum(SleepyUpdater);

            if (sn.mode != Snapshotter.Mode.Load)
            {
                Entity polledEventSourceAsEntity = PolledEventSource as Entity;
                if (polledEventSourceAsEntity != null)
                {
                    polledEventSourceEntity = polledEventSourceAsEntity.ID;
                }
                Expedition polledEventSourceAsExpedition = PolledEventSource as Expedition;
                if (polledEventSourceAsExpedition != null)
                {
                    polledEventSourceExpedition = polledEventSourceAsExpedition.ID;
                }
                Allegiance polledEventSourceAsAllegiance = PolledEventSource as Allegiance;
                if (polledEventSourceAsAllegiance != null)
                {
                    polledEventSourceAllegiance = polledEventSourceAsAllegiance.ID;
                }


                Entity dynamicTargetAsEntity = PolledEventSource as Entity;
                if (dynamicTargetAsEntity != null)
                {
                    dynamicTargetEntity = dynamicTargetAsEntity.ID;
                }
                Expedition dynamicTargetAsExpedition = PolledEventSource as Expedition;
                if (dynamicTargetAsExpedition != null)
                {
                    dynamicTargetExpedition = dynamicTargetAsExpedition.ID;
                }
                Allegiance dynamicTargetAsAllegiance = PolledEventSource as Allegiance;
                if (dynamicTargetAsAllegiance != null)
                {
                    dynamicTargetAllegiance = dynamicTargetAsAllegiance.ID;
                }
            }

            polledEventSourceEntity = sn.DoEnumNullable(polledEventSourceEntity);
            polledEventSourceExpedition = sn.DoEnumNullable(polledEventSourceExpedition);
            polledEventSourceAllegiance = sn.DoEnumNullable(polledEventSourceAllegiance);

            dynamicTargetEntity = sn.DoEnumNullable(dynamicTargetEntity);
            dynamicTargetExpedition = sn.DoEnumNullable(dynamicTargetExpedition);
            dynamicTargetAllegiance = sn.DoEnumNullable(dynamicTargetAllegiance);

            sn.Ignore(EventActionType);
            sn.Ignore(DynamicTarget);
            sn.Ignore(PolledEventSource);


            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

          //  EventActionType = LookUp<EventActionType, EventActionTypeID>.FindByID(snapshotEventActionType);


            if (polledEventSourceEntity.HasValue)
            {
                PolledEventSource = Entity.FindByID(polledEventSourceEntity);
            }
            else if (polledEventSourceExpedition.HasValue)
            {
                PolledEventSource = LookUp<Expedition, ExpeditionID>.FindByID(polledEventSourceExpedition);
            }
            else if (polledEventSourceAllegiance.HasValue)
            {
                PolledEventSource = LookUp<Allegiance, AllegianceID>.FindByID(polledEventSourceAllegiance);
            }

            polledEventSourceEntity = null;
            polledEventSourceExpedition = null;
            polledEventSourceAllegiance = null;

        }

        #endregion
    }
}
