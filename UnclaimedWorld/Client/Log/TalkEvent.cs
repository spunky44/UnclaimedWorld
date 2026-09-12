using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.InGameEvents.Actions;

namespace UWGame.ClientSide.Log
{
    /// <summary>
    /// after we log the last talk event, we can destroy the conversation object.
    /// </summary>
    public class TalkEvent
    {
        public uint ID;

        public EntityID SpokenBy;

        /// <summary>
        /// save the name for when the entity is destroyed
        /// </summary>
        public string Name;

        public string Line;

        /// <summary>
        /// for coloring
        /// </summary>
        public ulong? MessageGroupNo;
        //public Conversation Conversation;

        private static uint idCounter = 0;

        public TalkEvent()
        {           
            ID = idCounter;
            idCounter++;
        }

        public static void ResetOtherIDCounter()
        {
            idCounter = 0;
        }
    }
}
