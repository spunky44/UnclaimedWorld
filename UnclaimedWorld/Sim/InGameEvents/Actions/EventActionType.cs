using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UWGame.ClientSide.GameEvents;
using Microsoft.Xna.Framework.Content;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.InGameEvents.PropertyObjects;


namespace UWGame.SimSide.InGameEvents.Actions
{
    public enum EventActionTypeID : ulong
    {
        Invalid = ulong.MaxValue,
        Max = Invalid,
        First = 1
    }


    /// <summary>
    /// an action that should be performed in response to an event firing
    /// 
    /// Important: Like all Game Data Types, this class is readonly!! These objects may be shared by mmultiple events/action sets. 
    /// If instance data, like progress or status variables are needed, then create special instance classes for those - don't put the data in here.
    /// 
    /// This type class is special, it is given an ID, however the collection is not snapshotted... instead we rely on game data loading in the same sequence and assigning the same IDs each time.  
    /// </summary>
    [XmlInclude(typeof(SpawnEntityAction))]
    [XmlInclude(typeof(DestroyEntityAction))] 
    [XmlInclude(typeof(SetPropertyAction))]
    [XmlInclude(typeof(CreateExpeditionAction))]
    [XmlInclude(typeof(ExploreAction))]
    [XmlInclude(typeof(ChangeResourcesAction))]
    [XmlInclude(typeof(SpawnWorldAction))]
    [XmlInclude(typeof(SpawnSiteAction))]
    [XmlInclude(typeof(SpawnRouteAction))]
    [XmlInclude(typeof(SpawnStockpileAction))]
    [XmlInclude(typeof(SpawnAllegianceAction))]
    [XmlInclude(typeof(SpawnAllegianceRelationAction))]
   // [XmlInclude(typeof(SpawnContractAction))]
    [XmlInclude(typeof(SpawnTriggerAction))]
    [XmlInclude(typeof(ChangeCreditsAction))]
    [XmlInclude(typeof(CreateJobAction))]
    [XmlInclude(typeof(CancelJobAction))]
    [XmlInclude(typeof(ClaimEntityAction))]
    [XmlInclude(typeof(AttackEntityAction))]
    [XmlInclude(typeof(ProcessAction))]
    [XmlInclude(typeof(DetectAction))]
    [XmlInclude(typeof(EventActionDialog))]
    [XmlInclude(typeof(TalkAction))]
    [XmlInclude(typeof(WinGameAction))]
    [XmlInclude(typeof(LoseGameAction))]
    [XmlInclude(typeof(SoundEffectAction))]
    [XmlInclude(typeof(MusicAction))]
    [XmlInclude(typeof(LogAction))]
    [XmlInclude(typeof(SetViewAction))]
    [XmlInclude(typeof(ParticleEffectAction))]
    [XmlInclude(typeof(ShowTutorialAction))]
    public abstract class EventActionType : IGameData //, ILookUp<EventActionType, EventActionTypeID>
    {
        /// <summary>
        /// place the description here, then it will be preserved for player modding when serialized to xml.
        /// </summary>
        public string Comments;


        // fill in only one of the below:
      /*  public SpawnEntityAction SpawnEntity;
        public DestroyEntityAction DestroyEntity;     
        public SetPropertyAction SetPropertyAction;
        public CreateExpeditionAction CreateExpedition;
        public ExploreAction ExploreAction;
        public ChangeResourcesAction ChangeResourcesAction;
        public SpawnWorldAction SpawnWorld;
        public SpawnSiteAction SpawnSite;
        public SpawnRouteAction SpawnRoute;
        public SpawnStockpileAction SpawnStockpile;
        public SpawnAllegianceAction SpawnAllegiance;
        public SpawnAllegianceRelationAction SpawnAllegianceRelation;
        public SpawnContractAction SpawnContract;
        public SpawnTriggerAction SpawnTriggerAction;
        public ChangeCreditsAction ChangeCreditsAction;
        public CreateJobAction CreateJobAction;
        public CancelJobAction CancelJobAction;
        public ClaimEntityAction ClaimEntityAction;
        public AttackEntityAction AttackEntityAction;
        public ProcessAction ProcessAction;
        public DetectAction DetectAction;

        // player/client only:
        public EventActionDialog EventActionDialog;
        public TalkAction TalkAction;
        public WinGameAction WinGameAction;
        public LoseGameAction LoseGameAction;
        public SoundEffectAction SoundEffectAction;
        public MusicAction MusicAction;
        public LogAction LogAction;
        public SetViewAction SetViewAction;      
        public ParticleEffectAction ParticleEffectAction;
        public ShowTutorialAction ShowTutorialAction;
      */

        /// <summary>
        /// a delay after condition triggering until the action is executed
        /// </summary>
        public double DelayInSeconds = 0;


        public string KeyName
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }
        public bool DeleteRecord
        {
            get;
            set;
        }

        public EventActionType(string keyName)
        {
            this.KeyName = keyName;
        }

        public EventActionType()
        {
           /* if (!Snapshotter.IsSnapshotting)
            {*/
                // auto-generate a keyname for use in snapshotting. This should be overwritten in the DataLoader by a better name if it is going to be referenced from other types.
              //  KeyName = GameData.CreateKeyName();
          //  }

           // if (!Snapshotter.IsSnapshotting) // done both when loading from xml and when instantiating directly in GameDataLoaders!
          //  {
                /*
                 * 1. load from xml/instantiate in GameDataLoader - creates IDs
                 * 2. load objects from save file with referencing IDs
                 * 
                 * Same???
                 * This will only work if the sequence is exactly the same, so the IDs will get assigned the same values each time the game data types are loaded. 
                // And of course it will break if there are changes to the data types.
                 * 
                 * // make sure we don't snapshot this collection - it is already stored as a gamedatatype
                 * 
                 */                             

           //     AddToLookup(); // this also sets a flag not to snapshot the collection.
                
           // }
        }

        /// <summary>
        /// returns false if the action was not performed (talk only?)
        /// </summary>
        /// <param name="eventAction"></param>
        /// <returns></returns>
        public abstract bool Execute(EventAction eventAction, ref string failReason); 
      /*  {
            if (KeyName == "startWetFirewood")
            {

            }

            if (SpawnEntity != null)
            {
                bool result = SpawnEntity.Execute(eventAction.TriggeringEntity, eventAction.TargetEntity, eventAction.PolledEventSource, eventAction.DynamicTarget, ref failReason);

                if (!result)
                {

                }

                return result;
            }
            else if (DestroyEntity != null)
            {
                DestroyEntity.Execute(eventAction.TriggeringEntity, eventAction.TargetEntity, eventAction.PolledEventSource, eventAction.DynamicTarget, ref failReason);
            }           
            else if (WinGameAction != null)
            {
                WinGameAction.Execute(eventAction, ref failReason);
            }
            else if (LoseGameAction != null)
            {
                LoseGameAction.Execute(eventAction, ref failReason);
            }
            else if (ExploreAction != null)
            {
                ExploreAction.Execute(eventAction.TriggeringEntity, eventAction.TargetEntity, eventAction.PolledEventSource, eventAction.DynamicTarget, ref failReason);
            }
            else if (SetPropertyAction != null)
            {
                SetPropertyAction.Execute(eventAction.TriggeringEntity, eventAction.TargetEntity, eventAction.PolledEventSource, eventAction.DynamicTarget);
            }
            else if (CreateExpedition != null)
            {
                CreateExpedition.Execute(eventAction.TriggeringEntity, eventAction.TargetEntity, eventAction.PolledEventSource, eventAction.DynamicTarget, ref failReason);
            }
            else if (ChangeResourcesAction != null)
            {
                ChangeResourcesAction.Execute(eventAction.TriggeringEntity, eventAction.TargetEntity, eventAction.PolledEventSource, eventAction.DynamicTarget, ref failReason);
            }
            else if (SpawnWorld != null)
            {
                return SpawnWorld.Execute(eventAction.TriggeringEntity, eventAction.TargetEntity, ref failReason);
            }
            else if (SpawnSite != null)
            {
                return SpawnSite.Execute(eventAction.TriggeringEntity, eventAction.TargetEntity, ref failReason);
            }
            else if (SpawnStockpile != null)
            {
                return SpawnStockpile.Execute(eventAction, ref failReason);
            }
            else if (SpawnRoute != null)
            {
                return SpawnRoute.Execute(eventAction.TriggeringEntity, eventAction.TargetEntity, ref failReason);
            }
            else if (SpawnAllegiance != null)
            {
                return SpawnAllegiance.Execute(eventAction.TriggeringEntity, eventAction.TargetEntity, eventAction.PolledEventSource, eventAction.DynamicTarget, ref failReason);
            }
            else if (SpawnAllegianceRelation != null)
            {
                return SpawnAllegianceRelation.Execute(eventAction.TriggeringEntity, eventAction.TargetEntity, ref failReason);
            }
            else if (SpawnContract != null)
            {
                return SpawnContract.Execute(eventAction.TriggeringEntity, eventAction.TargetEntity, ref failReason);
            }
            else if (ChangeCreditsAction != null)
            {
                return ChangeCreditsAction.Execute(eventAction.TriggeringEntity, eventAction.TargetEntity, eventAction.PolledEventSource, eventAction.DynamicTarget, ref failReason);
            }
            else if (SpawnTriggerAction != null)
            {
                return SpawnTriggerAction.Execute(eventAction.TriggeringEntity, eventAction.TargetEntity, eventAction.PolledEventSource, eventAction.DynamicTarget, ref failReason);
            }
            else if (CreateJobAction != null)
            {
                return CreateJobAction.Execute(eventAction.TriggeringEntity, eventAction.TargetEntity, eventAction.PolledEventSource, eventAction.DynamicTarget, ref failReason);
            }
            else if (CancelJobAction != null)
            {
                return CancelJobAction.Execute(eventAction.TriggeringEntity, eventAction.TargetEntity, eventAction.PolledEventSource, eventAction.DynamicTarget, ref failReason);
            }
            else if (ClaimEntityAction != null)
            {
                return ClaimEntityAction.Execute(eventAction.TriggeringEntity, eventAction.TargetEntity, eventAction.PolledEventSource, eventAction.DynamicTarget, ref failReason);
            }
            else if (ProcessAction != null)
            {
                return ProcessAction.Execute(eventAction.TriggeringEntity, eventAction.TargetEntity, eventAction.PolledEventSource, eventAction.DynamicTarget, ref failReason);
            }
            else if (DetectAction != null)
            {
                return DetectAction.Execute(eventAction, ref failReason);
            }
            else if (AttackEntityAction != null)
            {
                return AttackEntityAction.Execute(eventAction.TriggeringEntity, eventAction.TargetEntity, eventAction.PolledEventSource, eventAction.DynamicTarget, ref failReason);
            }
            else if (The.Client != null)
            {
                if (EventActionDialog != null) // don't show dialogs in headless mode
                {
                    EventActionDialog.Execute(eventAction, ref failReason);
                }
                else if (SoundEffectAction != null)
                {
                    SoundEffectAction.Execute();
                }
                else if (MusicAction != null)
                {
                    MusicAction.Execute();
                }
                else if (TalkAction != null)
                {
                    return TalkAction.Execute(eventAction, ref failReason);
                }
                else if (LogAction != null)
                {
                    LogAction.Execute(eventAction.TriggeringEntity);
                }
                else if (ParticleEffectAction != null)
                {
                    return ParticleEffectAction.Execute(eventAction.TriggeringEntity, eventAction.TargetEntity, eventAction.PolledEventSource, eventAction.DynamicTarget, ref failReason);
                }
                else if (SetViewAction != null)
                {
                    SetViewAction.Execute(eventAction.TriggeringEntity, eventAction.TargetEntity, eventAction.PolledEventSource, eventAction.DynamicTarget, ref failReason);
                }
                else if (ShowTutorialAction != null)
                {
                    ShowTutorialAction.Execute(eventAction.TriggeringEntity, eventAction.TargetEntity, ref failReason);
                }
            }

            return true;
        }*/

      /*  public override string ToString()
        {
            if (SpawnEntity != null)
            {
                return SpawnEntity.ToString();
            }
            else if (DestroyEntity != null)
            {
                return DestroyEntity.ToString();
            }
            else if (AttackEntityAction != null)
            {
                return AttackEntityAction.ToString();
            }
            else if (ProcessAction != null)
            {
                return ProcessAction.ToString();
            }
            else if (DetectAction != null)
            {
                return DetectAction.ToString();
            }
            else if (WinGameAction != null)
            {
                return WinGameAction.ToString();
            }
            else if (LoseGameAction != null)
            {
                return LoseGameAction.ToString();
            }
            else if (SetPropertyAction != null)
            {
                return SetPropertyAction.ToString();
            }
            else if (ExploreAction != null)
            {
                return ExploreAction.ToString();
            }
            else if (ChangeResourcesAction != null)
            {
                return ChangeResourcesAction.ToString();
            }
            else if (SpawnWorld != null)
            {
                return SpawnWorld.ToString();
            }
            else if (SpawnStockpile != null)
            {
                return SpawnStockpile.ToString();
            }
            else if (SpawnSite != null)
            {
                return SpawnSite.ToString();
            }
            else if (SpawnRoute != null)
            {
                return SpawnRoute.ToString();
            }
            else if (SpawnAllegiance != null)
            {
                return SpawnAllegiance.ToString();
            }
            else if (SpawnAllegianceRelation != null)
            {
                return SpawnAllegianceRelation.ToString();
            }
            else if (SpawnContract != null)
            {
                return SpawnContract.ToString();
            }
            else if (CreateExpedition != null)
            {
                return CreateExpedition.ToString();
            }
            else if (SpawnTriggerAction != null)
            {
                return SpawnTriggerAction.ToString();
            }
            else if (ChangeCreditsAction != null)
            {
                return ChangeCreditsAction.ToString();
            }
            else if (CreateJobAction != null)
            {
                return CreateJobAction.ToString();
            }
            else if (CancelJobAction != null)
            {
                return CancelJobAction.ToString();
            }
            else if (The.Client != null)
            {
                if (EventActionDialog != null)
                {
                    return EventActionDialog.ToString();
                }
                else if (SoundEffectAction != null)
                {
                    return SoundEffectAction.ToString();

                }
                else if (MusicAction != null)
                {
                    return MusicAction.ToString();

                }
                else if (TalkAction != null)
                {
                    return TalkAction.ToString();

                }
                else if (LogAction != null)
                {
                    return LogAction.ToString();

                }
                else if (ParticleEffectAction != null)
                {
                    return ParticleEffectAction.ToString();
                }
                else if (SetViewAction != null)
                {
                    return SetViewAction.ToString();
                }
                else if (ShowTutorialAction != null)
                {
                    return ShowTutorialAction.ToString();
                }
            }

            return base.ToString();
        }*/


        public virtual void ExtractNestedActionTypes(ref List<string> duplicateKeyErrors)
        {
            if (!GameData.Instance.AllEventActionTypes.ContainsKey(KeyName)) // some will already have been added. TODO: WARNING: duplicate keys!! set a flag on the non-nested ones first!
            {
                GameData.Instance.AllEventActionTypes.Add(KeyName, this); // add to dict so we can look it up                       
            }
            else
            {
                Common.AddToList(ref duplicateKeyErrors, "Duplicate key: " + KeyName);
            }
                      
        }

        public virtual void LoadContent(ContentManager content)
        {
           
        }

        public virtual bool UsesTriggeringEntity
        {
            get
            {               
                return false;
            }
        }


        public virtual void PreInitValidate(ref List<string> errors)
        {
          
            EntityType.ValidateRequiredValue(ref errors, "KeyName", !string.IsNullOrEmpty(KeyName));


        }

        public virtual void PostLoadContentValidate(ref List<string> listOfErrors)
        {
          
        }


        public virtual void Initialize()
        {
          
        }

        public virtual void PostInitValidate(ref List<string> listOfErrors)
        {
           /* if (SpawnEntity != null)
            {
                SpawnEntity.PostInitValidate(ref listOfErrors);
            }
            else if (DestroyEntity != null)
            {
                DestroyEntity.PostInitValidate(ref listOfErrors);
            }
            else if (EventActionDialog != null) // don't show dialogs in headless mode
            {
                EventActionDialog.PostInitValidate(ref listOfErrors);
            }
            else if (WinGameAction != null)
            {
                WinGameAction.PostInitValidate(ref listOfErrors);
            }
            else if (LoseGameAction != null)
            {
                LoseGameAction.PostInitValidate(ref listOfErrors);
            }
            else if (SoundEffectAction != null)
            {
                SoundEffectAction.PostInitValidate(ref listOfErrors);
            }
            else if (MusicAction != null)
            {
                MusicAction.PostInitValidate(ref listOfErrors);
            }
            else if (TalkAction != null)
            {
                TalkAction.PostInitValidate(ref listOfErrors);
            }
            else if (LogAction != null)
            {
                LogAction.PostInitValidate(ref listOfErrors);
            }
            else if (SetPropertyAction != null)
            {
                SetPropertyAction.PostInitValidate(ref listOfErrors);
            }*/
        }

        public void PostDataCompleteInitialize()
        {
        }

        public void PreDataCompleteValidate(ref List<string> listOfErrors)
        { 
        
        
        }
       
        public virtual void PostDataCompleteValidate(ref List<string> listOfErrors)
        {           
           
        }

        public static bool GetEntity(string entityName, TargetObject targetObject,
           EventAction action, out Entity entity, ref string failReason)
        {
            return GetEntity(entityName, targetObject, action.TriggeringEntity, action.TargetEntity, action.PolledEventSource, action.DynamicTarget, out entity, ref failReason);
        }

        public static bool GetEntity(string entityName, TargetObject targetObject,
           EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget, out Entity entity, ref string failReason)
        {
            entity = null;
            if (entityName != null)
            {
                entity = TalkAction.GetEntityByName(entityName);
                if (entity == null)
                {
                    failReason = "No entity with name '" + entityName + "' exists.";
                    return false;
                }
            }
            else
            {
                var result = targetObject.GetResult(triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
                if (result.Count == 0)
                {
                    failReason = "Entity lookup did not give any results.";
                    return false;
                }
                entity = (Entity)result[0];
            }

            return true;
        }
    }
}
