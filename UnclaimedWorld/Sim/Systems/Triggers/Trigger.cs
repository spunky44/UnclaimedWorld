using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Entities.Body;
using GameStateManagement;
using UWGame.SimSide.AI;
using UWGame.Control;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.XmlCollections;
using UWGame.SimSide.Entities.Containers;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.InGameEvents.Conditions;


namespace UWGame.SimSide.Systems.Triggers
{
    public enum TriggerID : ulong
    {
        Invalid = ulong.MaxValue,
        Max = Invalid,
        First = 1
    }

    /// <summary>
    /// a trigger will send a message to all listeners within range
    /// and fire off events
    /// </summary>
    public class Trigger : ISleepingUpdatable, IHasExposedProperties, ISnapshot, ILookUp<Trigger, TriggerID>
    {
        public Entity Parent;
        EntityID? snapshotParent;

        public TriggerType TriggerType;
        public float? DurationInSeconds;

        Collisions.CollideShape2D area;


        /// <summary>
        /// either circular area
        /// </summary>
        float? range;

        /// <summary>
        /// or rectangle area
        /// </summary>
        Vector2? areaDimensions;



        private Vector3? fixedLocation;
        private Vector3 Location
        {
            get
            {
                if (Parent != null)
                {
                    return Parent.PlaySiteLocation;
                }
                else return fixedLocation.Value;
            }
        }

     
        private TimeSpan timeActivated;

        private int? timesTriggered;

        public enum TriggerMovement { Static, Attached }

        public object messageInfo;

       

       // private long? expiresAtTimepointInTicks;
        private double? expiryTimepointInSeconds;

        /// <summary>
        /// next update
        /// </summary>
        private double? timePointInSeconds;
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
            LookUpSleepyUpdater<Trigger>.Create();
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

                    SleepyUpdater<Trigger> updater = LookUpSleepyUpdater<Trigger>.FindByID(SleepyUpdater);

                    if (updater != null)
                    {
                        updater.NotifyUpdateIntervalChanged(this);  // this makes the sleepy updater compute a new expiry timepoint and resort the list:
                    }                  
                }
            }
        }

        public Trigger(Entity attachedTo, Vector3? fixedLocation, TriggerType TriggerType, object messageInfo = null, float? range = null, Vector2? area = null)
        {
            AddToLookup();

            this.Parent = attachedTo;
            this.messageInfo = messageInfo;
            this.TriggerType = TriggerType;

            this.fixedLocation = fixedLocation;

            this.range = range ?? TriggerType.Range;
            this.areaDimensions = area ?? TriggerType.AreaDimensions;

           // this.DurationInSeconds = DurationInSeconds;
           
           // this.MoveTrigger = triggerMovement;

            timeActivated = The.Sim.TotalUnPausedGameTime; // UWGame.SimSide.Instance.GetTotalGameTime();

            // stagger the trigger updates:
         //   TimePointInSeconds = 0.5 * UpdateInterval * The.Sim.GameplayRandomGenerator.NextDouble("Trigger");

            if (DurationInSeconds.HasValue)
            {
                expiryTimepointInSeconds = UpdateTimePoints.ComputeTimePointFromInterval(DurationInSeconds.Value);

               // expiresAtTimepointInTicks = TimeSpan.FromSeconds(DurationInSeconds.Value).Ticks + The.Sim.TotalUnPausedGameTime.Ticks;
            }

            InitArea();

            RecomputeUpdateInterval();
        }

        private void InitArea()
        {
            if (areaDimensions != null) // TriggerType.AreaDimensions != null)
            {
                float width = areaDimensions.Value.X;
                float height = areaDimensions.Value.Y;

                float left = Location.X - width / 2f;
                float top = Location.Y - height / 2f;
                // don't clamp the area to the map... it does not matter..

                area = new Collisions.CollideShape2D(top, left, top + height, left + width);
            }
        }

        static Trigger()
        {
            exposedPropertyValueFunctions.Add("triggeringsLeft", GetTriggeringsLeft); // returns null if no maximum

        }

        public Trigger() 
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
   
        }
       
      /*  public Trigger(
            Entity Source,
            TriggerPriority Priority,
            TriggerType TriggerType,
            float? DurationInSeconds,
            int RadiusManhattan,
            TriggerMovement triggerMovement,
            Point MapPosition)
        {
            this.Source = Source;
            this.Priority = Priority;
            this.TriggerType = TriggerType;
            this.DurationInSeconds = DurationInSeconds;
            this.RadiusManhattan = RadiusManhattan;
            this.MoveTrigger = triggerMovement;
            this.MapPosition = MapPosition;

            timeActivated = The.Sim.TotalUnPausedGameTime; // UWGame.SimSide.Instance.GetTotalGameTime();
        }*/

       

      //  private List<Entity> entitiesInRangeOfCurrentTrigger = new List<Entity>();
        public void Update(GameTime gameTime, out bool wasDestroyed)
        {
         
            UpdateTrigger(out wasDestroyed);

            if (!wasDestroyed)
            {
                UpdateExpiry(out wasDestroyed);

            }
        }

        private List<Pair<Entity, Vector2>> entitiesInRangeOfCurrentTrigger = new List<Pair<Entity, Vector2>>();
        public void UpdateTrigger(out bool wasDestroyed)
        {
           // bool wasDestroyed;
            wasDestroyed = false;

           
            Predicate<Entity> entityFilter = null;
            entitiesInRangeOfCurrentTrigger.Clear();

            if (area == null) // TriggerType.AreaDimensions == null)
            {
                float triggerRadius = range.Value; // (float)TriggerType.Range;

                The.AgentQuadTree.GetEntitiesInRange(Location.ToVector2(), triggerRadius, entityFilter, ref entitiesInRangeOfCurrentTrigger);
            }
            else
            {
              //  area.Center = Location.ToVector2(); // we cannot update the position.. only Collidable allows that...
                The.AgentQuadTree.GetObjectsIntersectingBounds(area, entityFilter, 
                    // e => The.Sim.PlaySite.PlayerAllegiance.Members.ContainsKey(e), //why was this used..?
                    ref entitiesInRangeOfCurrentTrigger);

                if (entitiesInRangeOfCurrentTrigger.Count > 0)
                {

                }
            }

            foreach (var entityWithinRange in entitiesInRangeOfCurrentTrigger)
            {
                if (entityWithinRange.First == Parent // NEW: don't trigger self
                   || !entityWithinRange.First.EntityType.IntelligenceType.HasInterestInTriggers.ContainsKey(TriggerType)
                   || entityWithinRange.First.Intelligence.IsReadyToHandleTrigger(this) == false)
                {
                    continue;
                }

                // test visibility
                if (Parent != null && !TriggerType.CanTriggerWhenUndetected)
                {
                    // only send the message if the entity can see us 
                    IKnownEntityData entityData;
                    if (entityWithinRange.First.Intelligence.GetKnownData(Parent.EntityID, out entityData) != EntityResult.SeenDirectly)
                    {
                        continue;
                    }
                }

                FireTrigger(entityWithinRange.First, out wasDestroyed);
            
            }      

            
        }

        private void UpdateExpiry(out bool wasDestroyed)
        {
            // timeBeforeNextUpdate = null;
            wasDestroyed = false;

            if (this.expiryTimepointInSeconds.HasValue)
            {
                if (The.Sim.TimepointReached(expiryTimepointInSeconds.Value))
                {
                    Destroy(); // destroy directly
                    wasDestroyed = true;

                    expiryTimepointInSeconds = null;
                }               
            }
        }

        private void RecomputeUpdateInterval()
        {
            bool intervalChanged;
            RecomputeUpdateInterval(out intervalChanged);
        }

        /// <summary>
        /// Remember to keep this up-to-date when more functionality is added to the Update method!
        /// Otherwise performance will suffer because of unnecessary updates, or the object may not receive any Update calls when it needs it.
        /// </summary>
        public void RecomputeUpdateInterval(out bool intervalWasChanged)
        {
            intervalWasChanged = false;

            double? tempInterval = null, currentInterval = null;


            tempInterval = UpdateTimePoints.ComputeIntervalFromTimepoint(this.expiryTimepointInSeconds);
            UpdateTimePoints.GetSoonestInterval(tempInterval, ref currentInterval);

            tempInterval = TriggerType.DurationBetweenTriggerUpdatesInSeconds;
            UpdateTimePoints.GetSoonestInterval(tempInterval, ref currentInterval);

         

            if (!Common.IsEqual(UpdateInterval, currentInterval))
            {
                UpdateInterval = currentInterval; // if changed, will alert the sleepy updater to resort the list

                intervalWasChanged = true;
            }
        }


        public void Destroy()
        {
            if (Parent != null)
            {
                Parent.DeleteTrigger(this); // expiredTriggers[i]);
            }
            else
            {
                The.Sim.TriggerSystem.DeleteTrigger(this);
            }

            RemoveIDEntry();
        }

        public void FireTrigger(Entity entity, out bool wasDestroyed)
        {
            wasDestroyed = false;
            
           /* if (TriggerType.DamageToTriggeringEntities != null)
            {
                DoDamage(entity);

            }*/


            SendMessages(entity);


            FireEvents(entity);


            if (TriggerType.MaxTimesToTriggerBeforeExpiring.HasValue)
            {
                if (timesTriggered == null)
                {
                    timesTriggered = 1;
                }
                else
                {
                    timesTriggered++;
                }

                if (timesTriggered >= TriggerType.MaxTimesToTriggerBeforeExpiring.Value)
                {
                    Destroy();
                    wasDestroyed = true;
                }
            }

        }

        public PropertyResult? GetTriggeringsLeft()
        {
            if (TriggerType.MaxTimesToTriggerBeforeExpiring.HasValue)
            {
                int value = TriggerType.MaxTimesToTriggerBeforeExpiring.Value - (timesTriggered ?? 0);
                return new PropertyResult() { NumberResult = value};
            }

            return null;
        }

        private OwnerID? GetOwnerOfCarcass()
        {
            // for traps, the owner is the owner of the trap. for agents, the owner is the agent...

            if (Parent.EntityType.IntelligenceType != null && Parent.Intelligence.CurrentExpedition != null)
            {
                return ((IOwner)Parent.Intelligence.CurrentExpedition).ID;
            }
            else if (Parent.OwnedBy.HasValue)
            {
                return Parent.OwnedBy;
            }

            return null;
        }

        private void SendMessages(Entity entity)
        {
            bool messageWasSent = false;

            if (TriggerType.IsPrey)
            {
                entity.SendMessage(new Message(Parent, Message.MessageTypes.PreyIsNear, this));
                messageWasSent = true;
            }


            if (TriggerType.IsEntityDied)
            {
                entity.SendMessage(new Message(Parent, Message.MessageTypes.EntityDied, this));
                messageWasSent = true;
            }

            if (!messageWasSent && TriggerType.Interest != null) // && TriggerType.Interest.InterestLevelMean.HasValue)
            {
                Message message = CreateInterestMessage(Parent, this, Parent.EntityID, null, null);
                entity.SendMessage(message); // this)); // a struct with a tuple member, hmmm...
            }
           
        }

       
        /// <summary>
        /// fire any events that show tutorial dialogs etc. to the player
        /// 
        /// triggering entity: if the trigger is attached to an entity (like a trap), that entity is used (names are a bit confusing perhaps..)
        /// targetentity: the listening entity that activates the trigger
        /// </summary>
        /// <param name="detectedEntity"></param>
        private void FireEvents(Entity detectingEntity)
        {
            
            if (TriggerType.ActionSetsKey == null)
                return;

            ActionSets actionsToFire = GameData.Instance.AllActionSets[TriggerType.ActionSetsKey]; 
            
            Entity triggeringEntity = null;
            if (Parent != null)
            {
                triggeringEntity = Parent;
            }

            bool isExpired;
            actionsToFire.Fire(triggeringEntity, detectingEntity.ID, null, out isExpired);

            // SerializableDictionary<string, ActionSets> detectionEvents = detectingEntity.EntityType.IntelligenceType.TriggerEventActions;          
           /* if (detectionEvents != null
                && detectionEvents.Count > 0)
            {
               
                ActionSets actionSets;
                if (detectionEvents.TryGetValue(TriggerType.KeyName, out actionSets))
                {
                    
                    actionSets.Fire(detectingEntity, null, out isExpired);

                    if (isExpired)
                    {   // no longer needed:
                        detectionEvents.Remove(TriggerType.KeyName);
                    }
                }
            }*/
        }

        public static Message CreateInterestMessage(Entity sender, Trigger trigger, EntityID? entity, Vector3? location, float? interest)
        {
            // perhaps it would be better to make Message into a class to avoid this kludge...
            return new Message(sender, Message.MessageTypes.Interest,
                    new Tuple<Trigger, EntityID?, Vector3?, float?>(trigger, entity, location, interest));
        }


        private static Dictionary<string, GetPropertyValue> exposedPropertyValueFunctions = new Dictionary<string, GetPropertyValue>();

        #region IHasExposedProperties

        public string KeyName { get; set; }

        //All IHasExposedProperties are casted to be able to call the class specific function for a certain property, 
        //this is safe because GetPropertyValue uses the global functions defined for the class in which it is used
        // However if we by mistake added a global function from another class to our dictionary then it might cause problems, but that should never happen
        public PropertyResult? GetPropertyValue(string propertyKey, SharedKnowledge getterKnowledge, IHasExposedProperties parent)
        {
            PropertyResult? result = null;
            if (exposedPropertyValueFunctions.ContainsKey(propertyKey))
            {
                result = exposedPropertyValueFunctions[propertyKey].Invoke(this, getterKnowledge, parent);
            }
            else
            {
                PropertyResult customResult;
                if (customFields != null && customFields.TryGetValue(propertyKey, out customResult))
                {
                    result = customResult;
                }
            }

            return result;
        }

        public string GetDefaultCaption(string propertyKey)
        {
            return TriggerType.Name;
        }

        public void GetDefaultKey(out string PropertyKey)
        {
            PropertyKey = null;
        }

        public void GetChildren(string keyToList, ref List<IHasExposedProperties> listToFillWithProperties, FilterCondition filter,
            EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget, 
            SharedKnowledge getterKnowledge = null)
        {
        }

        public EntityID? GetEntityID()
        {
            return null;
        }

        public bool GetIsSeenDirectly() //SharedKnowledge sharedKnowledge)
        {
            return true;
        }

        public string GetCaption(string captionKey)
        {
            return null;
        }

        public void SetPropertyValue(string propertyKey, PropertyResult? value)
        {
            Entity.SetPropertyValue(ref customFields, propertyKey, value);

        }

        private Dictionary<string, PropertyResult> customFields;

        #endregion

        #region Exposed properties


        public static PropertyResult? GetTriggeringsLeft(IHasExposedProperties anObjectToGetValueFrom, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Trigger)anObjectToGetValueFrom).GetTriggeringsLeft();
        }

        #endregion

        #region ISnapshot

        public bool IsSnapshotted { get; set; }

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public Snapshotter.Version DoVersion(Snapshotter sn)
        {
            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }


        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            this.id = SnapshotID(sn, id);
            IDCounter = (TriggerID)sn.DoEnum(IDCounter);

            this.snapshotParent = sn.SnapshotID<Entity, EntityID>(Parent);
            this.TriggerType = sn.DoGameData(TriggerType);
            this.DurationInSeconds = sn.DoFloatNullable(DurationInSeconds);
            this.expiryTimepointInSeconds = sn.DoDoubleNullable(expiryTimepointInSeconds);
            this.fixedLocation = sn.DoVector3Nullable(fixedLocation);
            this.messageInfo = sn.DoObject(messageInfo);
            this.timeActivated = sn.DoTimeSpan(timeActivated);
            this.timePointInSeconds = sn.DoDoubleNullable(timePointInSeconds);
            this.timesTriggered = sn.DoInt32Nullable(timesTriggered);
            this.updateInterval = sn.DoDoubleNullable(updateInterval);
            this.SleepyUpdater = sn.DoEnum(SleepyUpdater);
            this.customFields = sn.DoDictionary(customFields);
            this.KeyName = sn.DoString(KeyName);
            this.areaDimensions = sn.DoVector2Nullable(areaDimensions);
            this.range = sn.DoFloatNullable(range);


            sn.Ignore(entitiesInRangeOfCurrentTrigger);
            sn.Ignore(exposedPropertyValueFunctions);
            sn.Ignore(area);

            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            InitArea();

            Parent = Entity.FindByID(snapshotParent);
        }

        #endregion

        #region ILookup

        private TriggerID id = TriggerID.Invalid;
        static TriggerID IDCounter = TriggerID.First;

        public TriggerID ID
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

        public TriggerID GetUniqueID()
        {
            IDCounter++;
            if (IDCounter >= TriggerID.Max)
            {
                throw new Exception("Astounding, TriggerID just exceeded 64 bits. Something seriously wrong has happened.");
            }

            return IDCounter;
        }

        public TriggerID SnapshotID(Snapshotter sn, TriggerID id)
        {
            return (TriggerID)sn.DoEnum(id);
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
            if (ID != TriggerID.Invalid)
                LookUp<Trigger, TriggerID>.Add(ID, this);
        }

        public void SetInvalid()
        {
            id = TriggerID.Invalid;
        }

        public void RemoveIDEntry()
        {
            LookUp<Trigger, TriggerID>.Remove(this);
        }

        void ILookUp<Trigger, TriggerID>.ResetIDCounter() // interface method - does nothing...
        {
        }

        public static void ResetIDCounter() // called by invoke, do not remove
        {
            IDCounter = TriggerID.First;
        }

        void ILookUp<Trigger, TriggerID>.CreateLookupCollection() // interface method - does nothing...
        {
        }

        public static void CreateLookupCollection()
        {
            LookUp<Trigger, TriggerID>.Create();
        }


        #endregion
       
    }
}
