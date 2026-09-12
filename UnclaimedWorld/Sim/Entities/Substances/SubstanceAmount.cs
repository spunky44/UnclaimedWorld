using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UWGame.SimSide.Entities;
using UWGame.SimSide.AllGameData;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.AI;
using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities.Substances
{
    public class SubstanceAmount: ISnapshot, IHasExposedProperties
    {
        public SubstanceType SubstanceType;

        /// <summary>
        /// amount per bulk!!!
        /// I think it is alright to go above 1 for special items...
        /// </summary>
        public float Amount;

        public SubstanceAmount()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor");       

        }

        public SubstanceAmount(float amount, SubstanceType type)
        {
            SubstanceType = type;
            Amount = amount;
            //if (!exposedPropertyValueFunctions.ContainsKey("substanceLevel"))
            //{
            //    exposedPropertyValueFunctions.Add("substanceLevel", GetNutrientLevel);
            //}
        }

        /// <summary>
        /// copy ctor for memoryfact
        /// </summary>
        /// <param name="original"></param>
        public SubstanceAmount(SubstanceAmount original)
        {
            Amount = original.Amount;
            SubstanceType = original.SubstanceType;
        }

        static SubstanceAmount()
        {
            exposedPropertyValueFunctions.Add("substanceLevel", GetSubstanceAmount);
        }

        #region ExposedProperties


        public void GetChildren(string keyToList, ref List<IHasExposedProperties> listToFillWithProperties, FilterCondition filter,
            EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget, SharedKnowledge getterKnowledge = null)
        {
        }


        public string GetDefaultCaption(string propertyKey)
        {
            return SubstanceType.Name;
        }
        public void GetDefaultKey(out string PropertyKey)
        {
            PropertyKey = null;
        }
        public string KeyName
        {
            get { return SubstanceType.KeyName; }
        }
        public EntityID? GetEntityID()
        {
            return null;
        }
        public string GetCaption(string captionKey)
        {
            return null;
        }


        public bool GetIsSeenDirectly() //SharedKnowledge sharedKnowledge)
        {
            return true; // should maybe depend on parent status...?
        }

        private static Dictionary<string, GetPropertyValue> exposedPropertyValueFunctions = new Dictionary<string, GetPropertyValue>();



        //Copied customFields,GetPropertyValue,SetPropertyValue from the skills. 
        //TODO: See if theese should be here aswell or empty functions?
        private Dictionary<string, PropertyResult> customFields;

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

        public void SetPropertyValue(string propertyKey, PropertyResult? value)
        {
            Entity.SetPropertyValue(ref customFields, propertyKey, value);
            
        }


        public static PropertyResult? GetSubstanceAmount(IHasExposedProperties anoObjectToGetValueFrom, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((SubstanceAmount)anoObjectToGetValueFrom).GetSubstanceLevel();
        }
        public PropertyResult? GetSubstanceLevel()
        {
            PropertyResult skillResult = new PropertyResult();

            skillResult.NumberResult = this.Amount * 100;

            return skillResult;
        }
       
        #endregion


        #region ISnapshot

        public bool IsSnapshotted { get; set; }

        public ISnapshot DoSnapshot(Snapshotter sn)
        {

            this.Amount = sn.DoFloat(Amount);
            this.customFields = sn.DoDictionary(customFields);
            this.SubstanceType = sn.DoGameData(SubstanceType);


            sn.Ignore(exposedPropertyValueFunctions);

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

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);


            if (customFields != null)
            {
                foreach (var item in customFields)
                {
                    item.Value.LoadPostProcess(sn);
                }
            }
        }

        #endregion

    }
}
