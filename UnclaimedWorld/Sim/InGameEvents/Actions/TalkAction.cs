using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.ClientSide.Interface.HUD_Windows;
using UWGame.SimSide.Entities;
using GameStateManagement;
using UWGame.SimSide.Allegiances;
using UWGame.Control;
using UWGame.SimSide;
using System.Xml.Serialization;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.InGameEvents.Expressions;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.InGameEvents.Conditions;


namespace UWGame.SimSide.InGameEvents.Actions
{
    public enum ActionByAgent { RandomInAllegiance, PreferSpecific, OnlySpecific, OnlyTriggeringEntity, PreferTriggeringEntity  }

    /// <summary>
    /// now belongs in Sim because talk actions can cause body turns
    /// </summary>
    public class TalkAction : EventActionType// : IGameData
    {
        /// <summary>
        /// if true, the speaker will turn towards any nearby listeners, but only if he is idle
        /// </summary>
        public bool TurnTowardsListeners = false;

        /// <summary>
        /// the long name is to prevent conflicts in XML with other 'Priority' types
        /// </summary>
        public enum TalkActionPriority { Low, Normal, High }

        /// <summary>
        /// Only a higher priority talk action can interrupt and cancel a current conversation/ talkaction.
        /// Normal is default
        /// </summary>
        public TalkActionPriority TalkPriority = TalkActionPriority.Normal;

        /// <summary>
        /// supports max 3 speakers in conversation
        /// </summary>
        public enum SpeakerInConversation { First, Second, Third }

        /// <summary>
        /// All talk actions in the set with the same number here will be said by the same person
        /// </summary>
        public SpeakerInConversation? SpeakerDenomination;

        /// <summary>
        /// For how long the speech bubble will stay.
        /// if a duration is not specified, one will be calculated based on the length of the text
        /// </summary>
        public float? Duration;
       
        public ActionByAgent ActionByAgent;

        /// <summary>
        /// optional, but required for PreferSpecific and OnlySpecific
        /// </summary>
        public string NameOfSpeaker;

        /// <summary>
        /// optional, default will be player allegiance
        /// </summary>
        public string Allegiance;

        public string LineKey;
        public string DefaultText;

        #region Filters - used to select a speaker

        public bool CanTalkWhileSleeping;
        public bool CanTalkWhileFighting;
        public bool CanTalkWhileThreatened;
        public bool CanTalkWhileEmigrating;
        public bool CanTalkWhileTravelling;

        public Condition CanTalk;
        //public EvalNode CanTalk;

        #endregion


        [XmlIgnore]
        public int LineNo
        {
            get;
            private set;
        }

       


        public TalkAction(string keyName): base(keyName)
        {

        }

        public TalkAction()          
        {

        }

        public void SetLineNo(int lineNo)
        {
            this.LineNo = lineNo;
        }

       
        public override bool UsesTriggeringEntity
        {
            get
            {
                return ActionByAgent == ActionByAgent.OnlyTriggeringEntity ||
                    ActionByAgent == ActionByAgent.PreferTriggeringEntity;
            }
        }

        public override bool Execute(EventAction eventAction, ref string failReason)
        {
           
            ActionSetData actionData = LookUp<ActionSetData, ActionSetDataID>.FindByID(eventAction.ParentID.Value);

            if (!actionData.IsConversationValid(this))
            {
                failReason = "Conversation not valid";
                actionData.Conversation.EndConversation();
                return false;
            }


            if (!actionData.ConversationHasStarted)
            {
                // the conversation has not started.
                // is this line the first to be spoken?
                if (LineNo > 0)
                {
                    // one or more lines before this one were not spoken; cancel the action.
                    return false;
                }

                // see if a new conversation (or monologue?) is allowed to start:
                if (IsMoreImportantConversationActive())
                {
                    failReason = "A conversation of same or higher prio is still active";
                    return false;
                }
            }
            else
            {
                if (!actionData.Conversation.IsTalkActionNextInLine(this))
                {
                    // one or more spoken lines before this were never spoken... end the conversation and cancel...
                    actionData.Conversation.EndConversation();
                    return false;
                }
            }
            

            // find out who should say the line:
            EntityID? speaker = FindSpeaker(eventAction /*eventAction.TriggeringEntity*/, actionData);

            if (speaker.HasValue)
            {
                Entity speakerEntity = Entity.FindByID(speaker.Value);
                if (speakerEntity != null)
                {
                 
                /*    if (SpeakerDenomination.HasValue)
                    {*/
                        actionData.Conversation.SpeakLine(this, speakerEntity); 

                  //  }

                    speakerEntity.Intelligence.SpeakLine(LineKey, DefaultText, Duration.Value, TurnTowardsListeners, actionData.Conversation);




                    return true;
                }
            }
            else
            {
                // if we failed to find someone to say the line, make sure the conversation ends:
                failReason = "No speaker found";
                  
                actionData.Conversation.EndConversation();
            }

            return false;
        }


        /// <summary>
        /// prevent interweaved conversations... no active conversations of same or higher importance.
        /// on screen? globally?
        /// </summary>
        /// <returns></returns>
        private bool IsMoreImportantConversationActive()
        {
            Allegiance allegiance = GetAllegianceIfItCanSpeak();
            if (allegiance != null)
            {
                return allegiance.Members.Any(
                    e => EntityTypeCanSpeak(e.EntityType) 
                      &&  IsHavingSameOrMoreImportantConversation(e.Intelligence)); // no one in allegiance must currently be having a conversation of the same or higher importance

            }
            else
            {
                return false;
            }
        }


        private EntityID? FindSpeaker(EventAction eventAction, ActionSetData actionData)
        {
            EntityID? triggeringEntityID = eventAction.TriggeringEntity;

           // is this talk action part of a conversation (at least one person has to have spoken)
            if (SpeakerDenomination.HasValue && actionData.ConversationHasStarted) // parent.Conversation != null)
            {               

                // see if we have already been assigned a speaker in the conversation:
                EntityID previouslyAssignedSpeaker;
                if (actionData.Conversation.AssignedSpeakers.TryGetValue(SpeakerDenomination.Value, out previouslyAssignedSpeaker))
                {
                    // the next line in a conversation will be allowed to interrupt the previous line spoken by the same person, even before it has run its duration!
                    // so set the delays carefully.
                    return previouslyAssignedSpeaker;
                }
            }

            // else find a speaker:

            EntityID? speakerID = null;
            Entity entity;

            Entity triggeringEntity = null;
            if (triggeringEntityID.HasValue)
            {
                // this can be null if the action had a delay and the entity died:
                triggeringEntity = Entity.FindByID(triggeringEntityID.Value);
            }

            switch (ActionByAgent)
            {
                case ActionByAgent.OnlySpecific:
                    entity = GetEntityByName(NameOfSpeaker);

                    if (entity != null)
                    {
                        speakerID = entity.EntityID;
                    }

                    break;

                case ActionByAgent.PreferSpecific:

                    entity = GetEntityByName(NameOfSpeaker);
                    if (entity == null)
                    {
                        // find someone else in the allegiance who can say the line:
                        speakerID = GetRandomInAllegianceWhoCanSpeak(GetAllegianceIfItCanSpeak(), actionData.Conversation, eventAction);
                        
                    }
                    else
                    {
                        if (EntityCanSpeak(entity, eventAction))
                        {
                            speakerID = entity.EntityID;
                        }
                        else
                        {
                            // get an entity from the same allegiance:
                            speakerID = GetRandomInAllegianceWhoCanSpeak(entity.Intelligence.Allegiance, actionData.Conversation, eventAction); //GetAllegianceIfItCanSpeak());
                        }                        
                    }
                    break;

                case ActionByAgent.RandomInAllegiance:
                    speakerID = GetRandomInAllegianceWhoCanSpeak(GetAllegianceIfItCanSpeak(), actionData.Conversation, eventAction);
                    break;

                case ActionByAgent.OnlyTriggeringEntity:
                    if (EntityCanSpeak(triggeringEntity, eventAction))
                    {
                        speakerID = triggeringEntity.EntityID;
                    }

                    break;

                case ActionByAgent.PreferTriggeringEntity:
                    if (EntityCanSpeak(triggeringEntity, eventAction))
                    {
                        speakerID = triggeringEntity.EntityID;
                    }
                    else
                    {
                        speakerID = GetRandomInAllegianceWhoCanSpeak(triggeringEntity.Intelligence.Allegiance, actionData.Conversation, eventAction);
                    }
                    break;

                default: 
                    speakerID = null;
                    break;
            }

            /*
            if (SpeakerDenomination.HasValue)
            {
                SaveSpeaker(speakerID, parent);
            }
            */
            return speakerID;

        }

        /*
        private void SaveSpeaker(EntityID? speaker, ActionSet actionSet)
        {
            if (speaker.HasValue)
            {
                actionSet.SpeakLine(speaker.Value, this);               
            }
        }*/

        private bool AllegianceCanSpeak(Allegiances.Allegiance allegiance)
        {
            if (!CanTalkWhileThreatened && allegiance.IsUnderThreat())
            {
                return false;
            }

            return true;
        }


        

        private bool IsHavingSameOrMoreImportantConversation(Intelligence intelligence)
        {
            if (intelligence.CurrentConversationID.HasValue)
            {
                Conversation currentConversation = LookUp<Conversation, ConversationID>.FindByID(intelligence.CurrentConversationID.Value);

                if (currentConversation != null
                    && currentConversation.TalkPriority.HasValue
                    && (int)currentConversation.TalkPriority >= (int)this.TalkPriority)
                {
                    return true;
                }
            }

            return false;

        }

        /// <summary>
        /// ???
        /// </summary>
        /// <param name="intelligence"></param>
        /// <returns></returns>
        private bool IsCurrentlySpeakingWithoutAConversation(Intelligence intelligence)
        {
            if (intelligence.CurrentConversationID == null)
            {
                if (!string.IsNullOrEmpty(intelligence.SpokenLine))
                {
                    return true;
                }
            }

            return false;

        }

        private bool EntityTypeCanSpeak(EntityType entityType)
        {
            return entityType.Person != null || 
                (entityType.IntelligenceType != null && entityType.IntelligenceType.CanSpeak == true);
        }


        public static bool TestAgentProperties(Entity entity, bool allowSleeping,
                                                              bool allowUnconscious,
                                                                bool allowFighting,
                                                                bool allowThreatened,
                                                                bool allowEmigrating,
                                                                bool allowTravelling)
        {           
            Intelligence intelligence;
            if (!entity.Find(out intelligence))
                return false;

            bool isSleeping = intelligence.IsSleeping();

            if (!allowUnconscious && (intelligence.IsAwakeAndActive == false && isSleeping == false))
            {
                return false;
            }

            if (!allowSleeping && isSleeping)
            {
                return false;
            }

            if (!allowFighting && intelligence.IsAttacking())
            {
                return false;
            }

            if (!allowThreatened && intelligence.Allegiance.IsUnderThreat())
            {
                return false;
            }

            if (!allowTravelling && entity.Location == null)
            {
                return false;
            }

            if (!allowEmigrating && intelligence.IsEmigrating())
            {
                return false;
            }

            return true;

        }


       
        /// <summary>
        /// also tests for destroyed entities...
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private bool EntityCanSpeak(Entity entity, EventAction eventAction)
        {
            if (entity == null)
                return false;

            if (!entity.IsOnPlaySite())
                return false;

            if (!EntityTypeCanSpeak(entity.EntityType))
                return false;

            Intelligence intelligence;
            if (!entity.Find(out intelligence))
                return false;


            if (!TestAgentProperties(entity, CanTalkWhileSleeping, false, CanTalkWhileFighting, CanTalkWhileThreatened, CanTalkWhileEmigrating, CanTalkWhileTravelling))
            {
                return false;
            }
                       

            if (IsHavingSameOrMoreImportantConversation(intelligence))
            {
                return false;
            }

            if (IsCurrentlySpeakingWithoutAConversation(intelligence))
            {
                return false;
            }

            if (CanTalk != null)
            {
                if (!CanTalk.IsFulfilled(ref entity /* eventAction.TriggeringEntity*/, eventAction.TargetEntity, 
                    eventAction.PolledEventSource, eventAction.DynamicTarget))
                {
                    return false;
                }

              //  PropertyResult? result = CanTalk.Evaluate(eventAction.TriggeringEntity, eventAction.TargetEntity, eventAction.PolledEventSource, eventAction.DynamicTarget);

               /* if (result == null || result.Value.BoolResult != true)
                {
                    return false;
                }*/
            }

            return true;

        }

        private Allegiance GetAllegianceIfItCanSpeak()
        {
            SimSide.Allegiances.Allegiance allegiance;
            if (Allegiance != null)
            {
                // if specified:
                allegiance = The.Sim.PlaySite.Allegiances.First(a => a.Name == Allegiance);
            }
            else
            {
                // get the player's allegiance:
                allegiance = The.Sim.PlaySite.PlayerAllegiance;
            }

            if (allegiance != null
                && allegiance.Members.Count > 0
                && AllegianceCanSpeak(allegiance))
            {
                return allegiance;
            }           
            

            return null;
        }

        /// <summary>
        /// finds a random entity in the allegiance who can speak and has not spoken before in this conversation
        /// </summary>
        /// <param name="allegiance"></param>
        /// <param name="conversation"></param>
        /// <returns></returns>
        private EntityID? GetRandomInAllegianceWhoCanSpeak(Allegiance allegiance, Conversation conversation, EventAction eventAction)
        {
            if (allegiance != null)
            {
                var shuffledList = Common.Randomize(allegiance.Members.ToList(), The.Sim.GameplayRandomGenerator);

                Entity randomMember = shuffledList.FirstOrDefault(
                    e => EntityCanSpeak(e, eventAction) // can speak
                        && (conversation == null || !conversation.AssignedSpeakers.Values.Contains(e.EntityID))); // has not spoken before

                if (randomMember != null)
                {
                    return randomMember.EntityID;
                }
            } 
               
            return null;
        }

        public static Entity GetEntityByName(string name)
        {
            Entity entity;
            EntityID entityID;
            if (The.Sim.PlaySite.EntitiesByName.TryGetValue(name, out entityID))
            {
                entity = Entity.FindByID(entityID);
                if (entity != null)
                {
                    return entity; // entityID;
                }

            }

            return null;

        }

      
    /*    public string KeyName
        {
            get;
            set;
        }
        public string Name
        {
            get;
            set;
        }*/

        public override void PreInitValidate(ref List<string> errors)
        {
            base.PreInitValidate(ref errors);

            if (DefaultText == null)
            {
                EntityType.CreateValidationError(ref errors, "Text was not filled out!");
                                //string.Format("Text was not filled out!", KeyName));
            }
        }

        public override void Initialize()
        {
            if (!Duration.HasValue)
            {
                Duration = DefaultText.Length / GameData.Instance.Constants.TalkSpeedInCharactersPerSecond;

                Duration = Common.Clamp(Duration.Value, GameData.Instance.Constants.MinimumTalkDurationInSeconds, GameData.Instance.Constants.MaximumTalkDurationInSeconds);
                
            }
        }

        public override string ToString()
        {
            return this.DefaultText;
        }
    }
}
