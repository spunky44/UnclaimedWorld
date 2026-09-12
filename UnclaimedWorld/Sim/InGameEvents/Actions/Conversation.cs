using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.InGameEvents.Actions
{
    public enum ConversationID : ulong
    {
        Invalid = ulong.MaxValue,
        Max = Invalid,
        First = 1
    }

    /// <summary>
    /// maps TalkActions to speaker entities
    /// </summary>
    public class Conversation: ILookUp<Conversation, ConversationID>, ISnapshot
    {
       
        
        /// <summary>
        /// keep a list of the entities that have spoken so far during this conversation
        /// </summary>
        public Dictionary<TalkAction.SpeakerInConversation, EntityID> AssignedSpeakers = new Dictionary<TalkAction.SpeakerInConversation, EntityID>();

        /// <summary>
        /// used to signal that the rest of the talk actions should not be carried out (for instance if one of the speakers died).
        /// The conversation object is kept around for this reason...
        /// </summary>
        public bool ConversationShouldEnd = false;

        /// <summary>
        /// will contain the highest priority we have encountered in the executed talk actions 
        /// </summary>
        public TalkAction.TalkActionPriority? TalkPriority;

        public int TotalNoOfTalkActions;

        /// <summary>
        /// how many lines have been spoken until now.
        /// </summary>
        private int noOfSpokenLines = 0;

               

        public bool HasStarted
        {
            get
            {
                return noOfSpokenLines > 0;
            }
        }

        public Conversation(int totalTalkActions)
        {
            AddToLookup();

            TotalNoOfTalkActions = totalTalkActions;            
        }

        public Conversation()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");         
        }

        public bool IsTalkActionNextInLine(TalkAction talkAction)
        {
            return noOfSpokenLines == talkAction.LineNo;
        }

        public void SpeakLine(TalkAction talkAction, Entity entity) // EntityID entityID)
        {
            if (talkAction.SpeakerDenomination.HasValue)
            {
                if (!AssignedSpeakers.ContainsKey(talkAction.SpeakerDenomination.Value))
                {
                    AssignedSpeakers.Add(talkAction.SpeakerDenomination.Value, entity.EntityID);
                }
            }

            TalkPriority = talkAction.TalkPriority; // this will not change...)
                        
            // get notified when the spoken line is over:
            // conversation no longer uses the event because it is a pain to snapshot
          //  entity.Intelligence.TalkActionEnded += new Action(Intelligence_TalkActionEnded);

            noOfSpokenLines++;
        }

        void Intelligence_TalkActionEnded()
        {
            TalkActionEnded();
        }

        public void TalkActionEnded()
        {
            if (noOfSpokenLines == TotalNoOfTalkActions)
            {
                EndConversation();
            }
        }

        /// <summary>
        /// ends the conversation, freeing the speakers to start a new conversation
        /// 
        /// it cannot be destroyed...
        /// </summary>
        public void EndConversation()
        {
            ConversationShouldEnd = true;

            if (AssignedSpeakers != null)
            {
                foreach (var item in AssignedSpeakers)
                {
                    Entity speaker = Entity.FindByID(item.Value);
                    if (speaker != null)
                    {
                        if (speaker.Intelligence.CurrentConversationID == this.id)
                        {
                            speaker.Intelligence.CurrentConversationID = null;
                        }
                    }
                }
            }

            RemoveIDEntry(); // this should be safe...
        }


        #region ILookUp

        //======ILookup===============
        private ConversationID id = ConversationID.Invalid;
        static ConversationID IDCounter = ConversationID.First;

        public ConversationID ID
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

        public ConversationID GetUniqueID()
        {
            IDCounter++;
            if (IDCounter >= ConversationID.Max)
            {
                throw new Exception("Astounding, ConversationID just exceeded 64 bits. Something seriously wrong has happened.");
            }

            return IDCounter;
        }
      
        
        public ConversationID SnapshotID(Snapshotter sn, ConversationID id)
        {
            return (ConversationID)sn.DoEnum(id);
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
            if (ID != ConversationID.Invalid)
                LookUp<Conversation, ConversationID>.Add(ID, this);
        }

        public void SetInvalid()
        {
            id = ConversationID.Invalid;
        }

        public void RemoveIDEntry()
        {
            LookUp<Conversation, ConversationID>.Remove(this);
        }

        void UWGame.SimSide.Snapshots.ILookUp<Conversation, ConversationID>.ResetIDCounter() // interface method - does nothing...
        {
        }

        public static void ResetIDCounter() // called by invoke, do not remove
        {
            IDCounter = ConversationID.First;
        }

        void ILookUp<Conversation, ConversationID>.CreateLookupCollection() // interface method - does nothing...
        {
        }

        public static void CreateLookupCollection()
        {
            LookUp<Conversation, ConversationID>.Create();
        }

        #endregion


        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            id = SnapshotID(sn, id);
            IDCounter = sn.DoEnum(IDCounter);

            this.AssignedSpeakers = sn.DoDictionary(AssignedSpeakers);
            this.ConversationShouldEnd = sn.DoBool(ConversationShouldEnd);
            this.noOfSpokenLines = sn.DoInt32(noOfSpokenLines);
            this.TalkPriority = sn.DoEnumNullable(TalkPriority);
            this.TotalNoOfTalkActions = sn.DoInt32(TotalNoOfTalkActions);

            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

          
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


        #endregion

    }
}
