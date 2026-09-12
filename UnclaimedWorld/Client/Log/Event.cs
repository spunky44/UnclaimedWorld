using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;

namespace UWGame.ClientSide.Log
{
    public enum Priority { Low, Normal, High }
    public class Event
    {
        public EventType EventType;

        public EntityID? Entity;
        public string EntityName; // save the name for when the entity dies...
        public string EntityLink;

        public Priority Priority;
        
        //public int Time = 0;

        /// <summary>
        /// do we really want to use wall clock time here???
        /// what about using in game time? since they are game events
        /// </summary>
        public DateTime Time;

      

        public string Text;
       // public double Timestamp;

        public uint ID;

        private static uint idCounter = 0;

        public Event(Entity entity)
        {
           // Timestamp = The.Sim.TotalUnPausedGameTime.TotalMilliseconds; // DateTime.Now;
            if (entity != null)
            {
                this.Entity = entity.EntityID;

                this.EntityName = entity.ToString();
                this.EntityLink = entity.ToLink();
            }

           

            ID = idCounter;
            idCounter++;
        }

        public override string ToString()
        {
            if (Entity != null)
            {
                return string.Format("{0}: [{1}] {2}", Time, Entity, Text);
            }
            else
            {
                return string.Format("{0}: {1}", Time, Text);           
            }
        }


        public static void ResetOtherIDCounter()
        {
            idCounter = 0;
        }
    }
}
