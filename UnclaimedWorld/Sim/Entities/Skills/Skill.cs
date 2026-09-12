using System;
using System.Collections.Generic;
using System.Text;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.AI;
using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities
{
    public class Skill : IHasExposedProperties, ISnapshot
    {
        float value;
        float alltimeMaxValue;
        float potential;
        public SkillType SkillType;

        /// <summary>
        /// 0 - 1
        /// </summary>
        public float Value
        {
            get { return value; }
            set
            {
                this.value = value;

                // y = -0.65x² + 1.3x + 0.35
                // 0 skill gives 0.35
                productionFactor = GetSkillProductionFactor(value);
            }
        }


        public static float GetSkillProductionFactor(float value)
        {
            // y = -0.65x² + 1.3x + 0.35
            // 0 skill gives 0.35
            return Common.Clamp(-0.65f * (value * value) + 1.3f * value + 0.35f, 0f, 1f);
        }


        /// <summary>
        /// 0 - ?
        /// can go higher than 1...?
        /// </summary>
        /// <param name="value"></param>
        /// <param name="skillType"></param>
        public Skill(float value, SkillType skillType) 
        {
            this.SkillType = skillType;
            this.Value = value;

           /* if (value > 1f)
            {
                throw new Exception("")
            }*/
        }

        public Skill()
        {
        }

       

        public override string ToString()
        {
            return ((int)(100 * value)).ToString();
        }

        public float CalculateProgress(double secondsElapsed, float manSecondsOfWorkNeeded, float? upperSkillBound)
        {
            return (float)((secondsElapsed / manSecondsOfWorkNeeded) * (upperSkillBound == null ? ProductionFactor : Common.ClampTop(upperSkillBound.Value, ProductionFactor))); 

        }

        private float productionFactor;
        /// <summary>
        ///  y = -0.65x² + 1.3x + 0.35
        ///  0 skill gives 0.35
        ///  1 skill gives 1
        ///  Theres a steep slope with diminishing return towards 1.
        /// </summary>
        public float ProductionFactor
        {
            get
            {
                return productionFactor;
            }
        }
      
        
        // Exposed properties
        static Skill()
        {
            exposedPropertyValueFunctions.Add("skillLevel", GetSkillLevel);
            exposedPropertyValueFunctions.Add("skillDescription", GetSkillDescription);

            
        }

        #region IHasExposedProperties

        public void GetChildren(string keyToList, ref List<IHasExposedProperties> listToFillWithProperties, FilterCondition filter,
            EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget, SharedKnowledge getterKnowledge = null)
        {
        }

        private static Dictionary<string, GetPropertyValue> exposedPropertyValueFunctions = new Dictionary<string,GetPropertyValue>();

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
            return SkillType.Name;
        }
        public void GetDefaultKey(out string PropertyKey)
        {
            PropertyKey = null;
        }
        public string KeyName
        {
            get { return SkillType.KeyName; }
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

        #region ExposedProperties
        public static PropertyResult? GetSkillLevel(IHasExposedProperties anoObjectToGetValueFrom, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Skill)anoObjectToGetValueFrom).GetSkillLevel();
        }

        public PropertyResult? GetSkillLevel()
        {
            PropertyResult skillResult = new PropertyResult();
            skillResult.NumberResult = value;
            return skillResult;
        }

        public static PropertyResult? GetSkillDescription(IHasExposedProperties anoObjectToGetValueFrom, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Skill)anoObjectToGetValueFrom).GetSkillDescription();
        }

        public PropertyResult? GetSkillDescription()
        {
            PropertyResult skillResult = new PropertyResult();
            skillResult.StringResult = SkillType.Description;
            return skillResult;
        }
        #endregion


        #region ISnapshot
        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            this.alltimeMaxValue = sn.DoFloat(alltimeMaxValue);
            this.customFields = sn.DoDictionary(customFields);
            this.potential = sn.DoFloat(potential);
            this.productionFactor = sn.DoFloat(productionFactor);
            this.SkillType = sn.DoGameData(SkillType);
            this.value = sn.DoFloat(value);


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

        public bool IsSnapshotted { get; set; }


        public void LoadPostProcess(Snapshotter sn)
        {

            sn.RegisterLoadPostProcessCall(this);

        }

        #endregion
    }
}
