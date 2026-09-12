using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Entities;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.SimEffects
{
   
    /// <summary>
    /// a set of currently active effects for display in the side panel
    /// examples: Alcohol, Flute music, Broken arm
    /// </summary>
    public class SimEffectProfile : ISnapshot, IHasExposedProperties
    {
        public EffectProfileType EffectProfileType;

        public List<SimEffect> Effects;
        List<SimEffectID> snapshotEffects;


        /// <summary>
        /// in seconds - useful for effects that have falloff
        /// </summary>
      /*  public double StartedOn;

        /// <summary>
        /// food items will scale the effect type intensity by consumed bulk
        /// </summary>
        public float Intensity;


        public double? ExpiresOn;*/

        public SimEffectProfile()
        {

        }

        public SimEffectProfile(EffectProfileType effectType, Entity onEntity) //, float intensity)
        {
            this.EffectProfileType = effectType;
            Effects = new List<SimEffect>();

            foreach (var item in EffectProfileType.EffectTypes)
            {
                SimEffect effect = new SimEffect(item);
                Effects.Add(effect);
            }


            if (onEntity != null)
            {
                List<ActionSets> defaultActionSets;
                EffectProfileType.EventActions.TryGetValue(AgentActionHooks.StartedEffect, out defaultActionSets);
                Goal.FireEventActions(onEntity, null, defaultActionSets, null);

                /*
                Goal.FireEventActions(null, onEntity.EntityID,
                    ProcessType.GetStartHook(), ProcessType.EventActions, AgentActionHooks.StartProducing, null);

                Goal.FireEventActions(attacker, onEntity.EntityID,
                    hookToUse, defaultEventActions,
                    hookToUse, EventActions);*/
            }
        }

         // Exposed properties
        static SimEffectProfile()
        {
            exposedPropertyValueFunctions.Add("noOfEffects", GetNoOfEffects);
            exposedPropertyValueFunctions.Add("description", GetDescription);
            exposedPropertyValueFunctions.Add("name", GetName);
           
            
        }


        public void Destroy(Entity onEntity, bool wasReplaced)
        {
            foreach (var item in Effects)
            {
                item.Destroy();
            }


            if (wasReplaced == false && onEntity != null)
            {
                List<ActionSets> defaultActionSets;
                EffectProfileType.EventActions.TryGetValue(AgentActionHooks.EndedEffect, out defaultActionSets);
                Goal.FireEventActions(onEntity, null, defaultActionSets, null);
            }
        }

        #region ExposedProperties
        public static PropertyResult? GetNoOfEffects(IHasExposedProperties anoObjectToGetValueFrom, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((SimEffectProfile)anoObjectToGetValueFrom).GetNoOfEffects();
        }

        public PropertyResult? GetNoOfEffects()
        {
            PropertyResult result = new PropertyResult();
            result.NumberResult = this.EffectProfileType.EffectTypes.Count;
            return result;
        }

        public static PropertyResult? GetName(IHasExposedProperties anoObjectToGetValueFrom, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((SimEffectProfile)anoObjectToGetValueFrom).GetName();
        }

        public PropertyResult? GetName()
        {
            PropertyResult result = new PropertyResult();
            result.StringResult = this.EffectProfileType.Name;
            return result;
        }

        public static PropertyResult? GetDescription(IHasExposedProperties anoObjectToGetValueFrom, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((SimEffectProfile)anoObjectToGetValueFrom).GetDescription();
        }

        public PropertyResult? GetDescription()
        {
            PropertyResult result = new PropertyResult();

            StringBuilder text = new StringBuilder();
            Common.AppendHeaderOnLightBG(text, EffectProfileType.Name);
            Common.Append(text, EffectProfileType.Description);
            Common.AppendDividerOnOwnLine(text);
            foreach (var item in Effects)
            {
               /* Common.Append(text, item.EffectType.Name); // Name is not used...
                Common.Append(text, ": ");*/
                item.EffectType.AppendAsString(text, EffectType.Background.White);
                Common.AppendLine(text);
            }

            result.StringResult = text.ToString();
            return result;
        }

        #endregion


        #region IHasExposedProperties


        public void GetChildren(string keyToList, ref List<IHasExposedProperties> listToFillWithProperties, FilterCondition filter,
            EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget, SharedKnowledge getterKnowledge = null)
        {
        }

        private static Dictionary<string, GetPropertyValue> exposedPropertyValueFunctions = new Dictionary<string, GetPropertyValue>();

        public string GetCaption(string captionKey)
        {
            return null;
        }

        public PropertyResult? GetPropertyValue(string propertyKey, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
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
            return EffectProfileType.Name;
        }

        public void GetDefaultKey(out string key)
        {
            key = KeyName; // null;
        }

        public string KeyName
        {
            get { return EffectProfileType.KeyName; }
        }

        public EntityID? GetEntityID()
        {
            return null;
        }

        public bool GetIsSeenDirectly() //SharedKnowledge sharedKnowledge)
        {
            return true;
        }

        public void SetPropertyValue(string propertyKey, PropertyResult? value)
        {
            Entity.SetPropertyValue(ref customFields, propertyKey, value);
        }



        private Dictionary<string, PropertyResult> customFields;

        #endregion
         /* public void Start(EffectProfileType effectType, float intensity)
        {
            this.EffectProfileType = effectType;

           

            // find mutexed effects:

          
            StartedOn = The.Sim.TotalUnPausedGameTimeInSeconds;
            Intensity = intensity;

            if (EffectType.Duration.HasValue)
            {
                ExpiresOn = StartedOn + EffectType.Duration.Value;

            }
        }*/


      

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
            EffectProfileType = sn.DoGameData(EffectProfileType);
           /* StartedOn = sn.DoDouble(StartedOn);
            Intensity = sn.DoFloat(Intensity);*/


            if (Effects != null)
            {
                snapshotEffects = Effects.Select(e => e.ID).ToList();
            }

            snapshotEffects = sn.DoList(snapshotEffects);
            customFields = sn.DoDictionary(customFields);
       
            sn.Ignore(Effects);
            sn.Ignore(exposedPropertyValueFunctions);
         
            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            if (snapshotEffects != null)
            {
                Effects = snapshotEffects.Select(e => LookUp<SimEffect, SimEffectID>.FindByID(e)).ToList();   

            }
          
        }


        #endregion

    }
}
