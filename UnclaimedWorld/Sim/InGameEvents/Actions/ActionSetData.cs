using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.ClientSide.GameEvents;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.InGameEvents.Actions
{
    public enum ActionSetDataID : ulong
    {
        Invalid = uint.MaxValue,
        Max = Invalid,
        First = 1
    }

    /// <summary>
    /// contains run-time data such as counters for a corresponding ActionSet type. We do not want to store this data in the Type objects since they should be readonly.
    /// 
    /// this class gets instantiated when one of its actions are fired, and manages any delays or coordination between the actions. 
    /// 
    /// TODO: is it possible to Destroy this object? when?
    /// </summary>
    public class ActionSetData: ISnapshot, ILookUp<ActionSetData, ActionSetDataID>
    {
       

        ActionSetType actionSetType;

      

        private ConversationID? conversationID;

        /// <summary>
        /// the conversation object coordinates the talk actions
        /// </summary>
        public Conversation Conversation
        {
            get
            {
                Conversation conversation;
                if (conversationID == null)
                {
                   // ActionSetType actionSetType = LookUp<ActionSetType, ActionSetTypeID>.FindByID(actionSetTypeID);

                    conversation = new Conversation(actionSetType.NoOfTalkActions);
                    conversationID = conversation.ID;
                }
                else
                {
                    conversation = LookUp<Conversation, ConversationID>.FindByID(conversationID.Value);
                }

                return conversation;
            }           
        }

     

        public ActionSetData(ActionSetType actionSetType)
        {
           // this.actionSetTypeID = actionSetType.ID;
            this.actionSetType = actionSetType;

            AddToLookup();
        }

        public ActionSetData()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");         
        }

        public bool ConversationHasStarted
        {
            get
            {
                if (conversationID.HasValue)
                {
                    Conversation conversation = LookUp<Conversation, ConversationID>.FindByID(conversationID.Value);
                    if (conversation != null)
                    {
                        return conversation.HasStarted;
                    }
                    
                }
                return false;               
            }
        }

        public bool IsConversationValid(TalkAction talkAction)
        {
            // is this talk action part of a conversation (at least one person has to have spoken)

            if (talkAction.SpeakerDenomination.HasValue && ConversationHasStarted) //  parent.Conversation != null)
            {
                if (Conversation.ConversationShouldEnd) //  parent.Conversation.ConversationShouldEnd)
                {
                    return false; // cancel the rest of the dialogue...
                }

                // if any previous speaker has died in the meantime, don't continue either:
                if (Conversation.AssignedSpeakers.Values.Any(e => Entity.FindByID(e) == null))
                {
                    return false;
                }

            }

            return true;

        }

        

        public void Fire(Entity triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
        {          
            //ActionSetType actionSetType = LookUp<ActionSetType, ActionSetTypeID>.FindByID(actionSetTypeID);
                      
            foreach (var eventActionType in actionSetType.Actions)
            {
                EventAction eventAction = new EventAction(eventActionType, this);
               // eventActions.Add(eventAction);

                eventAction.ExecuteNowOrLater(triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
            }

            if (actionSetType.MaxFirings.HasValue)
            {
                The.Sim.PlaySite.EventManager.IncreaseFirings(actionSetType);
            }

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
            id = sn.DoEnum(id);
            IDCounter = sn.DoEnum(IDCounter);

           // this.actionSetTypeID = (ActionSetTypeID)sn.DoEnum(actionSetTypeID);
            this.actionSetType = (ActionSetType)sn.DoGameData(actionSetType);
            this.conversationID = sn.DoEnumNullable(conversationID);                        

            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);


        }

        #endregion


        #region ILookup

        private ActionSetDataID id = ActionSetDataID.Invalid;
        static ActionSetDataID IDCounter = ActionSetDataID.First;

        public ActionSetDataID ID
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


        public ActionSetDataID GetUniqueID()
        {
            IDCounter++;
            if (IDCounter >= ActionSetDataID.Max)
            {
                throw new Exception("Astounding, ActionSetDataID just exceeded 64 bits. Something seriously wrong has happened.");
            }

            return IDCounter;
        }

        public ActionSetDataID SnapshotID(Snapshotter sn, ActionSetDataID id)
        {
            return (ActionSetDataID)sn.DoEnum(id);
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
            if (ID != ActionSetDataID.Invalid)
            {
                LookUp<ActionSetData, ActionSetDataID>.Add(ID, this); // this collection will snapshot all instances.
            }
        }

        public void RemoveIDEntry()
        {
            LookUp<ActionSetData, ActionSetDataID>.Remove(this);
        }

        void ILookUp<ActionSetData, ActionSetDataID>.ResetIDCounter()
        {
        }

        public static void ResetIDCounter() // called by invoke, do not remove
        {
            IDCounter = ActionSetDataID.First;
        }

        void ILookUp<ActionSetData, ActionSetDataID>.CreateLookupCollection() // interface method - does nothing...
        {
        }

        public static void CreateLookupCollection()
        {
            LookUp<ActionSetData, ActionSetDataID>.Create();
        }


        public void SetInvalid()
        {
            id = ActionSetDataID.Invalid;
        }

        #endregion
    }
}
