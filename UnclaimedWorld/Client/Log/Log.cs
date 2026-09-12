using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide;
using UWGame.SimSide.AI;
using UWGame.SimSide.InGameEvents.Actions;

namespace UWGame.ClientSide.Log
{
    public delegate void NewEventHandler(Event newEvent); //MouseEventArgs args);

    public class Log
    {
        /// <summary>
        /// TODO: replace these with data driven categories
        /// </summary>
        public EventType DebugEvent = new EventType() { Name = "Debug", DefaultPriority = Priority.Normal };
        public EventType EconomicEvent = new EventType() { Name = "Economic", DefaultPriority = Priority.Normal };
        public EventType GeneralEvent = new EventType() { Name = "General", DefaultPriority = Priority.Normal };
        public EventType CombatEvent = new EventType() { Name = "Combat", DefaultPriority = Priority.Normal };
        
        public List<Event> Events = new List<Event>();
      //  public Dictionary<Entity, List<Event>> EntityEvents = new Dictionary<Entity, List<Event>>();

        public List<TalkEvent> TalkEvents = new List<TalkEvent>();

        public event NewEventHandler NewEventAlert;



        public void AddTalk(string line, EntityID speaker, Conversation conversation)
        {
            TalkEvent talkEvent = new TalkEvent() { Line = line, SpokenBy = speaker};

            if (conversation != null)
            {
                talkEvent.MessageGroupNo = (ulong) conversation.ID;
            }

            TalkEvents.Add(talkEvent);

        }


        /// <summary>
        /// TODO: add filtering to incoming messages - Group detection messages by resource type...
        /// allows some high prio messages to become alerts. 
        /// prevent alert spam!
        /// examples:
        /// X is under attack! (what about big battles?)
        /// Y is attacking Z!
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="concernedEntity"></param>
        /// <param name="eventText"></param>
        /// <param name="priority"></param>
        public void AddLogEvent(EventType eventType, Entity concernedEntity, string eventText, Priority? priority = null)
        {
            List<Event> entityEvents = null;
            if (eventType == DebugEvent)
            {
#if DEBUG || PROFILE

               /* if (!Kensei.Dev.Options.GetOption("Dev.Emit debug output in log"))
                {
                    return;
                }      */          
#else
                return;
#endif

            }


           
            /*
            if (concernedEntity != null)
            {   
                // filter events when thrashing AI occurs... Still necessary????? Missing messages can cause confusion...
                if (EntityEvents.TryGetValue(concernedEntity, out entityEvents))
                {
                    Event lastEventFromThisEntity = entityEvents[entityEvents.Count - 1];
                    // 2 secs...
                    if (The.Sim.TotalUnPausedGameTime.TotalMilliseconds - lastEventFromThisEntity.RealTimestamp < 2000 
                        && lastEventFromThisEntity.Text == eventText)
                    {
                        return;
                    }
                }                
            }*/
                                    

            /**If concerned Entity is not visible we should make sure it does not send messages to our log**/

            /**If it is concerned about a entity that we cant see then for example cE is fighting X and we cant see X do we type something else
             * Like we could replace fighting X to fighting an unseen entity - Lars: this will never happen. direct attacks are always detected.
             **/
            if (concernedEntity != null)
            {
                IKnownEntityData data;
                if (The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(concernedEntity.EntityID, out data) != EntityResult.SeenDirectly)
                {
                    return;
                }
            }

            if (priority == null)
            {
                priority = eventType.DefaultPriority;
            }

            Event evt = new Event(concernedEntity)
            {
                EventType = eventType,
                Text = eventText,
                Priority = priority.Value,
                Time = (The.Sim.DateAndTime.GetTime()) 
            };

            Events.Add(evt);

            if (Events.Count > GameData.Instance.GUIConstants.MaxLogEventsToKeep)
            {
                Events.RemoveAt(Events.Count - 1);
            }
            

           /* if (concernedEntity != null)
            {
                if (entityEvents == null)
                {
                    if (!EntityEvents.TryGetValue(concernedEntity, out entityEvents))
                    {
                        entityEvents = new List<Event>();
                        EntityEvents.Add(concernedEntity, entityEvents);
                    }
                }

                entityEvents.Add(evt);
            }*/

            // this will trigger an alert window:
            if (evt.Priority == Priority.High)
            {
                if (NewEventAlert != null)
                    NewEventAlert.Invoke(evt);
            }

        }

        
    }
}
