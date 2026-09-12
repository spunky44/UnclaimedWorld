using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.Maps;
using UWGame.SimSide.InGameEvents;
using UWGame.SimSide.Resources;
using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.Systems.Triggers;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Systems;
using System.Diagnostics;
using UWGame.SimSide.Systems.TimeSlicing;
using UWGame.SimSide.Snapshots;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.AI.Pathfinding;
using UWGame.SimSide.AI;
using UWGame.SimSide.InGameEvents.SpecialEvents;
using UWGame.SimSide.Overland;

namespace UWGame.SimSide.IngameEvents
{
    
    public class EventManager : ICyclable, ISnapshot
    {
        //int spawnEntityCounter = 0;


        private enum Phase { /*SpawnEntities, TimeConditions, AreaConditions, PropertyConditions, AllegianceMemberConditions,*/ GlobalConditions, EventActions, PlayerEntityDeaths, GroupMeetings }
        private Phase phase = Phase.GlobalConditions; //.SpawnEntities;

        private Regulator regulator; 

        HighResolutionTime timer;

        const int eventActionsPerCycle = 5;

 
        // polled:
        private SleepyUpdater<PolledEvent> polledEvents = new SleepyUpdater<PolledEvent>(Module.Sim, true);
        List<PolledEvent> snapshotPolledEvents;

        private SleepyUpdater<EventAction> eventActions = new SleepyUpdater<EventAction>(Module.Sim);
        List<EventAction> snapshotEventActions;
       
        /// <summary>
        /// keeps track of the different event actions that have ever fired. 
        /// an action set is decoupled from conditions, so the total number of firings are counted here
        /// </summary>
        private Dictionary<ActionSetType, int> actionSetFirings = new Dictionary<ActionSetType, int>();
       // private Dictionary<ActionSetTypeID, int> actionSetFirings = new Dictionary<ActionSetTypeID, int>();

        /// <summary>
        /// contains the time since the last meeting
        /// </summary>
        UnhappinessGroupMeetingEvent unhappinessGroupMeetingEvent;     
        Funeral funeral;


        public bool IsPaused { get; set; }
        
        public double StartedOnTimeInSeconds { get; set; }

        public static double totalComputationAllInstancesInSeconds;
        public double TotalComputationAllInstancesInSeconds
        {
            get
            {
                return totalComputationAllInstancesInSeconds;
            }
            set
            {
                totalComputationAllInstancesInSeconds = value;
            }
        }
        public double ComputationTimeSpentInSeconds { get; set; }


        public double? UpdateInterval
        {
            get
            {
                return 0.08d;
               // return 0; // NEW: run every frame
                //return 2d;
            }
        }

        public EventManager()
        {

        }

        public EventManager(Site site)
        {
            if (!Snapshotter.IsSnapshotting)
            {
                AddToLookup();

                CreateRegulators();

                if (site.IsPlaySite)
                {
                    unhappinessGroupMeetingEvent = new UnhappinessGroupMeetingEvent();
                    //policyAdopted = new PolicyAdoptedByVoting();
                    funeral = new Funeral();
                }
            }           
        }

        void CreateRegulators()
        {            
            regulator = new Regulator(The.Sim.GameplayRandomGenerator, 1f / UpdateInterval.Value, "EventManager");


            timer = new HighResolutionTime();
        }

     
        public void RemovePolledEvent(PolledEventType type, EntityID? sourceEntity = null, ExpeditionID? sourceExpedition = null, AllegianceID? sourceAllegiance = null)
        {
            PolledEvent polledEvent = GetEvent(type, sourceEntity, sourceExpedition, sourceAllegiance);

            if (polledEvent != null)
            {
                RemovePolledEvent(polledEvent);
            }
        }

        private PolledEvent GetEvent(PolledEventType type,
            EntityID? sourceEntity = null, ExpeditionID? sourceExpedition = null, AllegianceID? sourceAllegiance = null)
        {
            PolledEvent polledEvent = null;

            if (sourceEntity.HasValue)
            {
                polledEvent = polledEvents.GetItem(p => p.PolledEventType == type && p.SourceEntity == sourceEntity);
            }
            else if (sourceExpedition.HasValue)
            {
                polledEvent = polledEvents.GetItem(p => p.PolledEventType == type && p.SourceExpedition == sourceExpedition);           
            }
            else if (sourceAllegiance.HasValue)
            {
                polledEvent = polledEvents.GetItem(p => p.PolledEventType == type && p.SourceAllegiance == sourceAllegiance);
           
            }

            return polledEvent;
        }

        /// <summary>
        /// the event will not be added if one already exists of the same type and with the same source...
        /// </summary>
        /// <param name="eventKey"></param>
        /// <param name="sourceEntity"></param>
        /// <param name="sourceExpedition"></param>
        /// <param name="sourceAllegiance"></param>
        /// <returns></returns>
        public PolledEvent AddPolledEvent(string eventKey, EntityID? sourceEntity = null, ExpeditionID? sourceExpedition = null, AllegianceID? sourceAllegiance = null)
        {           
            PolledEventType pt = GameData.Instance.AllPolledEvents[eventKey];
            PolledEvent polledEvent;

            if (GetEvent(pt, sourceEntity, sourceExpedition, sourceAllegiance) == null)
            {
                polledEvent = new PolledEvent(pt, sourceEntity, sourceExpedition);

                AddPolledEventToSleepyUpdater(polledEvent);

                return polledEvent;
            }

            return null;
        }

        private void AddPolledEventToSleepyUpdater(PolledEvent globalCondition, bool keepExistingTimepoint = false)
        {
            bool addRandomTimeOffset = false;
            if (globalCondition.PolledEventType.AllowRandomTimeOffset == true)
            {
                addRandomTimeOffset = true; // stagger regular updates
            }

            polledEvents.Add(globalCondition, addRandomTimeOffset, keepExistingTimepoint);
        }

      /*  private void AddPolledEventToSleepyUpdater(PolledEvent globalCondition, bool keepExistingTimepoint = false)
        {
            bool addRandomTimeOffset = false;
            if ((globalCondition.PolledEventType.ConditionSet == null || globalCondition.PolledEventType.ConditionSet.IsTimeCondition == false)
                && globalCondition.PolledEventType.AllowRandomTimeOffset == true)
            {                
                addRandomTimeOffset = true; // stagger regular updates
            }

            polledEvents.Add(globalCondition, addRandomTimeOffset, keepExistingTimepoint);
        }*/


        public bool IsExpended(ActionSetType actionSet)
        {
            if (actionSet.MaxFirings.HasValue)
            {
                int firings;
                if (!actionSetFirings.TryGetValue(actionSet, out firings))
                {
                    firings = 0;
                }

                if (firings >= actionSet.MaxFirings.Value)
                    return true;
            }

            return false;
        }

        public void IncreaseFirings(ActionSetType actionSet)
        {
            int firings;
            if (actionSetFirings.TryGetValue(actionSet, out firings))
            {
                firings++;
                actionSetFirings[actionSet] = firings;
            }
            else
            {
                firings = 1;
                actionSetFirings.Add(actionSet, firings);
            }

            
        }

        public void RemoveEventAction(EventAction eventAction)
        {
            eventActions.Remove(eventAction);
        }

        public void RemovePolledEvent(PolledEvent polledEvent)
        {
            polledEvents.Remove(polledEvent);
        }

        public void Update(GameTime gameTime)
        {
            if (!The.Sim.CycleManager.IsRegistered(this))
            {
                double milliSecondsSinceLastReady = 0;
                if (regulator.IsReady(ref milliSecondsSinceLastReady))
                {
                   // deltaTime = milliSecondsSinceLastReady; // get the precise time since we were last here! use this value to calculate degradation.

                    phase = Phase.GlobalConditions; // EventActions; //.SpawnEntities;
                    //spawnEntityCounter = 0;
                    The.Sim.CycleManager.Register(this, CycleManager.Priority.Medium);
                }
            }

        }


        public void AddEventForLaterExecution(EventAction eventAction)
        {
            eventActions.Add(eventAction);

        }

        public void ClearEvents()
        {
            //DetectEntityTypeEvents.Clear();
          //  DetectResourceTypeEvents.Clear();
         /*   TimeConditionGameEvents.Clear();
            AreaConditionGameEvents.Clear();
            AllegianceMemberConditionGameEvents.Clear();
           // TriggerEvents.Clear();
            PropertyGameEvents.Clear();*/
            polledEvents = new SleepyUpdater<PolledEvent>(Module.Sim, true);

        }

        
        
        


        /// <summary>
        /// triggers some burial dialog and destroys the corpse...
        /// </summary>
        public void PlayerEntityHasDied(Entity deadEntity, Entity carcassEntity, CauseOfDeath? causeOfDeath)
        {
            if (funeral != null)
            {
                funeral.PlayerEntityHasDied(deadEntity, carcassEntity, causeOfDeath);
            }

           
        }


      


        public void GetCurrentEvents(StringBuilder description)
        {
            description.AppendLine("Global (polled) conditions:");
            polledEvents.IterateItems(
                g => description.AppendLine(g.TimePointInSeconds.Value + ": " + g.PolledEventType.KeyName));

            description.AppendLine();
            description.AppendLine("Event actions:");

           eventActions.IterateItems(
                e => description.AppendLine(e.TimePointInSeconds.Value + ": " + e.EventActionType.ToString())); // .GlobalConditionalEvent.KeyName + ": " + g.TimePointInSeconds.Value));


        }

        #region ILookup

        private CyclableID id = CyclableID.Invalid;
      
        //=================== ILookup Methods =====================
        public CyclableID ID
        {
            get
            {
                return id;
            }

            private set
            {
                id = value;
            }
        }

        public CyclableID GetUniqueID()
        {
            return Cyclable.GetUniqueID();
        }

        public CyclableID SnapshotID(Snapshotter sn, CyclableID id)
        {
            return (CyclableID)sn.DoEnum(id);
        }


        public int LoadPostProcessOrder
        {
            get
            {
                return 0;
            }
        }


        public void AddToLookup()
        {
            ID = GetUniqueID();
            if (ID != CyclableID.Invalid)
                LookUp<ICyclable, CyclableID>.Add(ID, this);
        }

        public void RemoveIDEntry()
        {
            LookUp<ICyclable, CyclableID>.Remove(this);
        }

        public void SetInvalid()
        {
            id = CyclableID.Invalid;
        }

        public void ResetIDCounter() // interface method - does nothing... Sim will call Cyclable.ResetIDCounter.
        {
        }

        void ILookUp<ICyclable, CyclableID>.CreateLookupCollection() // interface method - does nothing...
        {
        }

        public static void CreateLookupCollection()
        {
            LookUp<ICyclable, CyclableID>.Create();
        }

        #endregion

        #region ICyclable Members

        public void PrintInfo(StringBuilder text)
        {
            text.Append(string.Format("EventManager {0}:", ID));

        }


        public bool CycleOnce()
        {
            bool result = false;

            Phase progressBeforeStart = phase;
           
            timer.Start();
           
                         
            switch (phase)
            {
               
                case Phase.GlobalConditions:
                    polledEvents.Update(The.Sim.GameTime);

                    if (The.Sim == null) // the game can have ended here
                    {
                        result = true;
                        break;
                        //return true;
                    }

                    phase = Phase.EventActions;
                    break;

                case Phase.EventActions:
                    {
                        // fire any event actions that have been saved for later execution
                                            
                        eventActions.Update(The.Sim.GameTime); 

                        phase = Phase.PlayerEntityDeaths;
                        break;
               
                    }
                case Phase.PlayerEntityDeaths:
                    {
                        if (funeral != null)
                        {
                            funeral.Update();
                        }
                       
                        phase = Phase.GroupMeetings;                        
                        break;
                       
                    }
                case Phase.GroupMeetings:
                    {
                        
                        // playsite only
                        if (unhappinessGroupMeetingEvent != null)
                        {
                            unhappinessGroupMeetingEvent.Update();
                        }

                        phase = Phase.GlobalConditions; 
                        result = true;
                        break;
                      
                    }

                default:
                    result = true;
                    break;
                    
            }


            double timeTaken = timer.GetTime();

            if (timeTaken > 0.002)
            {
                int fired = 0;
            }

            return result;

        }

     /*   private void TestPropertyConditions()
        {
            GlobalConditionalEvent e;
            int fired = 0;

            while (PropertyGameEvents.Count > 0 && fired < eventActionsPerCycle)
            {
                e = PropertyGameEvents[0];

                if (e.Condition.PollCondition.CustomCondition.IsFulfilled())
                {
                    e.Fire(null);

                    fired++;


                    TimeConditionGameEvents.RemoveAt(0);
                }
                else {

                }
            }

        }*/

        /*
        private void TestAreaConditions()
        {
            // fire events that depend on player agents entering an area
            // make these into triggers instead???
            GlobalConditionalEvent e;
            //   bool totalWasAdded = false;
            //   int fired = 0;
            Rectangle area;

            // perform all area tests in one go...
            for (int i = AreaConditionGameEvents.Count - 1; i >= 0; i--)
            {
                e = AreaConditionGameEvents[i];

                area = e.Condition.AreaCondition.Area;

                foreach (var entity in The.Sim.Site.PlayerAllegiance.AllegianceMembers)
                {
                    if (area.Contains(entity.Key.Location.ToPoint())) // better to use quad tree...?
                    {
                        bool wasAdded;
                        e.Fire(entity.Value);

                        //   fired++;

                        //    totalWasAdded = totalWasAdded || wasAdded;

                        AreaConditionGameEvents.RemoveAt(i);

                        break;
                    }
                }
            }
        }*/

      /*  private void TestAllegianceMemberConditions()
        {
            GlobalConditionalEvent e;

            int noOfMembersToTrigger;
            LimitCondition limitBehaviour;

            int siteMembers = The.Sim.Site.PlayerAllegiance.AllegianceMembers.Count;

            bool fire = false;

            // perform all tests in one go...
            for (int i = AllegianceMemberConditionGameEvents.Count - 1; i >= 0; i--)
            {
                e = AllegianceMemberConditionGameEvents[i];
                noOfMembersToTrigger = e.Condition.NoSiteAllegianceMembers.MemberLimitToTrigger;
                limitBehaviour = e.Condition.NoSiteAllegianceMembers.LimitConditionValue;


                switch (limitBehaviour)
                {
                    case LimitCondition.Equal:
                        if (siteMembers == noOfMembersToTrigger)
                        {
                            fire = true;
                        }
                        break;
                    case LimitCondition.AboveOrEqual:
                        if (siteMembers >= noOfMembersToTrigger)
                        {
                            fire = true;
                        }
                        break;
                    case LimitCondition.BelowOrEqual:
                        if (siteMembers <= noOfMembersToTrigger)
                        {
                            fire = true;
                        }
                        break;
                    default: fire = false;
                        break;
                }

                if (fire)
                {
                    bool wasAdded;
                    e.Fire(null);

                    AllegianceMemberConditionGameEvents.RemoveAt(i);
                }

            }
        }
        */

      


       


        public bool PlayerAllegianceIsSleepingOrCollapsed()
        {
            return The.Sim.PlaySite.PlayerAllegiance.Members.All(e => !e.IsAwakeAndActive());

        }

        public bool PlayerAllegianceIsAttacking()
        {
            return The.Sim.PlaySite.PlayerAllegiance.Members.Any(e => e.IsAttacking());

        }

        public bool PlayerAllegianceIsUnderThreat()
        {
            return The.Sim.PlaySite.PlayerAllegiance.IsUnderThreat();

        }


        public void PrintGlobalConditions(System.Text.StringBuilder description)
        {
            description.AppendLine("sleeping or collapsed: " + PlayerAllegianceIsSleepingOrCollapsed());
            description.AppendLine("attacking: " + PlayerAllegianceIsAttacking());
            description.AppendLine("under threat: " + PlayerAllegianceIsUnderThreat());

        }

      
      
      


        public bool UnregisterBeforeSnapshot
        {
            get
            {
                return false; 
            }
        }

        #endregion

        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            // snapshot the current progress.
            id = SnapshotID(sn, id);

            this.IsPaused = sn.DoBool(IsPaused);
            this.phase = sn.DoEnum(phase);

            this.actionSetFirings = (Dictionary<ActionSetType, int>)sn.DoDictionary(actionSetFirings);
          
            this.unhappinessGroupMeetingEvent = (UnhappinessGroupMeetingEvent)sn.DoISnapshot(unhappinessGroupMeetingEvent);
            this.funeral = (Funeral)sn.DoISnapshot(funeral);
           
          //  this.eventActions = (SleepyUpdater<EventAction>)sn.DoISnapshot(eventActions); 
         //   this.globalConditions = (SleepyUpdater<GlobalCondition>)sn.DoISnapshot(globalConditions); 
           
            if (sn.mode != Snapshotter.Mode.Load)
            {
                snapshotEventActions = new List<EventAction>();
                snapshotPolledEvents = new List<PolledEvent>();

                eventActions.IterateItems(e => snapshotEventActions.Add(e));
                polledEvents.IterateItems(e => snapshotPolledEvents.Add(e));
            }
 
            // we are responsible for snapshotting SleepyUpdater members:
            snapshotEventActions = sn.DoList(snapshotEventActions);
            snapshotPolledEvents = sn.DoList(snapshotPolledEvents);

            sn.Ignore(totalComputationAllInstancesInSeconds);
            sn.Ignore(ComputationTimeSpentInSeconds);
            sn.Ignore(StartedOnTimeInSeconds);

         
            sn.Ignore(regulator);
            sn.Ignore(polledEvents);
            sn.Ignore(eventActions);
           
            return this;
        }

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


        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

           // this.eventActions.LoadPostProcess(sn); // needed because the LookUp class won't call it...
           // this.globalConditions.LoadPostProcess(sn); 

            foreach (var polledEvent in snapshotPolledEvents)
            {
               
                polledEvent.LoadPostProcess(sn);
                AddPolledEventToSleepyUpdater(polledEvent, true);
            }
            snapshotPolledEvents.Clear(); // clear for next save

            foreach (var item in snapshotEventActions)
            {
                eventActions.Add(item, keepExistingTimepoint: true);
            }
            snapshotEventActions.Clear();  // clear for next save

            if (unhappinessGroupMeetingEvent != null)
            {
                unhappinessGroupMeetingEvent.LoadPostProcess(sn);
            }

            if (funeral != null)
            {
                funeral.LoadPostProcess(sn);
            }

            CreateRegulators();
           
        }

        #endregion
    }
}
