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
using UWGame.SimSide.AI.Needs;
using UWGame.SimSide.Entities.Biological;

namespace UWGame.SimSide.Items
{
    public class FoodNutrientAmount: IXmlSerializable, IHasExposedProperties
    {
        public FoodNutrientType Nutrient;

        /// <summary>
        /// amount per bulk!!!
        /// I think it is alright to go above 1 for special items...
        /// </summary>
        public float Amount;


        static FoodNutrientAmount()
        {
            exposedPropertyValueFunctions.Add("nutrientLevel", GetNutrientLevel); // not used currently...
            exposedPropertyValueFunctions.Add("satisfiedDailyIntake", GetSatisfiedDailyIntake);
        }


       

        #region ExposedProperties


        public void GetChildren(string keyToList, ref List<IHasExposedProperties> listToFillWithProperties, FilterCondition filter,
            EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget, 
            SharedKnowledge getterKnowledge = null)
        {
        }


        public string GetDefaultCaption(string propertyKey)
        {
            return Nutrient.Name;
        }
        public void GetDefaultKey(out string PropertyKey)
        {
            PropertyKey = null;
        }
        public string KeyName
        {
            get { return Nutrient.KeyName; }
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

        private static Dictionary<string, GetPropertyValue> exposedPropertyValueFunctions = new Dictionary<string, GetPropertyValue>();


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


        public static PropertyResult? GetNutrientLevel(IHasExposedProperties anoObjectToGetValueFrom, SharedKnowledge getterKnowledge, IHasExposedProperties parent)
        {
            return ((FoodNutrientAmount)anoObjectToGetValueFrom).GetNutrientLevel();
        }

        public static PropertyResult? GetSatisfiedDailyIntake(IHasExposedProperties anoObjectToGetValueFrom, SharedKnowledge getterKnowledge, IHasExposedProperties parent)
        {
            return ((FoodNutrientAmount)anoObjectToGetValueFrom).GetSatisfiedDailyIntake(getterKnowledge, parent);
        }

        public PropertyResult? GetNutrientLevel()
        {
            PropertyResult result = new PropertyResult();

            result.NumberResult = this.Amount;


            return result;
        }

        public PropertyResult? GetSatisfiedDailyIntake(SharedKnowledge sharedKnowledge, IHasExposedProperties parent) //float itemBulk, EntityType consumer)
        {
            //Entity parentItem = (Entity)
            float itemBulk = ((IKnownEntityData)parent).Bulk;
            EntityType consumer = sharedKnowledge.Allegiance.RepresentativeEntityType;

            float consumerBulk;
            NeedType[] needs = consumer.BiologicalType.GetAdultNeedsAndWeight(out consumerBulk);

            if (needs != null)
            {
                string result = GetSatisfiedDailyIntake(itemBulk, needs, consumerBulk);

                return new PropertyResult()
                {
                    StringResult = result
                };
            }

            return null;
        }



        public string GetSatisfiedDailyIntake(float itemBulk, NeedType[] adultNeeds, float consumerWeight) //EntityType consumer)
        {
            float consumerBulk = BiologicalEntity.GetBulkFromWeight(consumerWeight); // casteType.WeightMean);

            NeedType needType = adultNeeds.FirstOrDefault(n => n.KeyName == Nutrient.KeyName);
            if (needType != null)
            {
                float totalAmount = Amount * itemBulk;

                float requiredAmountPerLevel = needType.FoodNeedType.RequiredNutrientsAsFractionOfEntityBulk * consumerBulk; //  // Parent.Parent.Parent.Parent.Bulk;

                float requiredAmountPerDay = (float)(requiredAmountPerLevel * needType.DecreasePerDay.Mean);
                
                float satisfiedDailyRequirements;

                if (requiredAmountPerDay > 0f)
                {
                    satisfiedDailyRequirements = totalAmount / requiredAmountPerDay;
                }
                else
                {
                    satisfiedDailyRequirements = 1f;
                }

                return Common.PercentageToString(satisfiedDailyRequirements);
            }

            /*
            CasteType casteType = consumer.BiologicalType.Castes.FirstOrDefault(c => c.Reproduction == Entities.Biological.Reproduction.Male);

            if (casteType == null)
            {
                casteType = consumer.BiologicalType.Castes[0];
            }

            if (casteType != null)
            {
                AgeGroupType ageGroup = casteType.AgeGroupTypes.FirstOrDefault(a => a.AIAgeGroup == AIAgeGroup.Adult);

                if (ageGroup != null)
                {
                    float consumerBulk = BiologicalEntity.GetBulkFromWeight(casteType.WeightMean);

                    NeedType needType = ageGroup.NeedTypes.FirstOrDefault(n => n.KeyName == Nutrient.KeyName);
                    if (needType != null)
                    {
                        float totalAmount = Amount * itemBulk;

                        float requiredAmountPerLevel = needType.FoodNeedType.RequiredNutrientsAsFractionOfEntityBulk * consumerBulk; //  // Parent.Parent.Parent.Parent.Bulk;

                        float requiredAmountPerDay = (float)(requiredAmountPerLevel * needType.DecreasePerDay.Mean);

                        float satisfiedDailyRequirements = totalAmount / requiredAmountPerDay;

                        return Common.PercentageToString(satisfiedDailyRequirements);
                    }
                }
            }*/

            return null;
        }


      /*  public bool SatisfiesComfortNeed(NeedType[] adultNeeds, out NeedType needType)
        {
            needType = adultNeeds.FirstOrDefault(n => n.KeyName == Nutrient.KeyName);
            if (needType != null && needType.ComfortEffects != null)
            {
                return true;
            }

            return false;
        }*/

        #endregion

        #region IXmlSerializable Members

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            CustomXmlSerializer.ReadXmlDeserialize(this, reader, _proxyData);
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            CustomXmlSerializer.WriteXmlSerialize(this, writer, _proxyData);
        }

        public static readonly CustomXmlSerializer.XmlProxyData _proxyData = new CustomXmlSerializer.XmlProxyData(typeof(FoodNutrientAmount))
        {
            TypeMappings = BaseDataLoader.GetListOfTypeMappings()
        };

        #endregion
    }
}
