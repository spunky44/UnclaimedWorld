using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide;
using System.Xml.Serialization;
using UWGame.SimSide.Resources;
using UWGame.SimSide.AllGameData;
using System.Diagnostics;

namespace UWGame.SimSide.Entities
{
    /// <summary>
    /// a profile for how well the sensor can detect different types of resources or entities
    /// </summary>
    [DebuggerDisplay("{KeyName}")]
    public class DetectionType: IGameData //, IXmlSerializable
    {
        public string Comments;

        public string KeyName { get; set; }

        public string Name { get; set; }
        public bool DeleteRecord
        {
            get;
            set;
        }
        /// <summary>
        /// the default detection factor is 1.
        /// the factor given here can be 0 = cannot detect or a number higher than 1
        /// 
        /// these arrays are not used in game
        /// </summary>
        public DetectionFactor[] DetectionFactors;

        public bool DetectionDisabled;

        /// <summary>
        /// helper dictionaries - these get used in the game
        /// </summary>
        [XmlIgnore]
        public Dictionary<IDetectableType, DetectionFactor> DetectFactors = new Dictionary<IDetectableType, DetectionFactor>();

      //  [XmlIgnore]
     //   public Dictionary<IDetectableType /* EntityType*/, CommonDetectionFactor> EntityTypeDetectFactors = new Dictionary<IDetectableType /*EntityType*/, CommonDetectionFactor>();
      
      /*  [XmlIgnore]
        public Dictionary<EntityType, EntityTypeDetectionFactor> EntityTypeDetectFactors = new Dictionary<EntityType, EntityTypeDetectionFactor>();
        */

       
        public void Initialize()
        {
            
        }

        public void PreInitValidate(ref List<string> errors) { }
        public void PostInitValidate(ref List<string> errors) 
        {
          /*  if (this.listOfErrors != null && this.listOfErrors.Count > 0)
            {
                errors.AddRange(this.listOfErrors);
            }  */      
        }


        public void PostLoadContentInitialize(ref List<string> errors)
        {
           
            if (DetectionFactors != null)
            {
                foreach (var factor in DetectionFactors)
                {
                    // we don't save the factors in their own list (yet), so init them here...
                    factor.PostLoadContentInitialize(ref errors);

                    foreach (var detectableType in factor.DetectableTypes)
                    {
                        if (!DetectFactors.ContainsKey(detectableType))
                        {
                            DetectFactors.Add(detectableType, factor);
                        }
                        else
                        {                            
                            EntityType.CreateValidationError(ref errors,
                                string.Format("An entry for the detectableType {0} already exists in the list of detect factors.", detectableType.KeyName));
                        }
                    }
                }
            }

         /*   if (EntityTypeDetectionFactors != null)
            {
                foreach (var factor in EntityTypeDetectionFactors)
                {
                    factor.PostLoadContentInitialize();

                    foreach (var entityType in factor.EntityTypes)
                    {
                        if (!EntityTypeDetectFactors.ContainsKey(entityType))
                        {
                            EntityTypeDetectFactors.Add(entityType, factor);
                        }
                        else
                        {
                            // save this validation error for a moment...
                            EntityType.CreateValidationError(ref listOfErrors,
                                string.Format("An entry for the entity type {0} already exists in the list of detect factors.", KeyName));
                        }
                    }
                }
            }*/


        }

        public void PostDataCompleteInitialize()
        {
        }
        public void PreDataCompleteValidate(ref List<string> listOfErrors) { }
       
        public void PostDataCompleteValidate(ref List<string> listOfErrors)
        {
        }

    }

    /// <summary>
    /// was: ResourceDetectionFactor
    /// </summary>
    public class DetectionFactor: IXmlSerializable
    {
        /// <summary>
        /// resource types with this key will be detected using the factors defined here
        /// 
        /// WAS ResourceTypeKeyName
        /// </summary>
        public string TypeKey;

        /// <summary>
        /// resource types with this tag will be detected using the factors defined here
        /// 
        /// WAS ResourceTypeTag
        /// </summary>
        public string TypeTag;

        
        /// <summary>
        /// the list of resource types that have specific detection factors defined
        /// </summary>
      /*  [XmlIgnore]
        public List<ResourceType> ResourceTypes = new List<ResourceType>();
        */

        /// <summary>
        /// WAS ResourceTypes
        /// </summary>
        [XmlIgnore]
        public List<IDetectableType> DetectableTypes = new List<IDetectableType>();
        

        /// <summary>
        /// entities within this distance are immediately detected (set to 0 to disable)
        /// </summary>
        public float? DistanceToAlwaysDetect;

        public bool ShouldSerializeDistanceToAlwaysDetect()
        {
            return DistanceToAlwaysDetect != null;
        }

        /// <summary>
        /// if true, the resource can only be detected when the entity is actively searching (for it???)
        /// </summary>
        public bool RequiresExamineAction = false;

        public bool AddLogMessageWhenDetected = true;

        public float Value;

        /// <summary>
        /// an optional skill to modify the standard detection ability
        /// </summary>
        public SkillType SkillToUse;
        
       
        //public float? InterestWhenDetected;

        /// <summary>
        /// when set, overrides the default interest number in constants
        /// </summary>
        public float? InterestLevelForSpottedResourceMean;
        public float? InterestLevelForSpottedResourceStdDeviation;

        public void PostLoadContentInitialize(ref List<string> errors)
        {
            if (this.TypeTag == "inDeeperWaterFishingSpot")
            {

            }

            // requires all types to have been loaded.
            if (!string.IsNullOrEmpty(TypeTag))
            {
                DetectableTypes = GameData.Instance.DetectableTypeByTag[TypeTag];
            }

            if (!string.IsNullOrEmpty(TypeKey))
            {
                IDetectableType detectableType = null;
                ResourceType r;
                EntityType e; 
                if (GameData.Instance.AllResourceTypes.TryGetValue(TypeKey, out r))
                {
                    detectableType = r;
                }
                else if (GameData.Instance.AllEntityTypes.TryGetValue(TypeKey, out e))
                {
                    detectableType = e;
                }
                else 
                {
                    EntityType.CreateValidationError(ref errors,
                               string.Format("TypeKey {0} not found.", TypeKey));
                }

                if (detectableType != null &&
                    !DetectableTypes.Contains(detectableType))
                {
                    DetectableTypes.Add(detectableType);
                }
            }
        }

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

        public static readonly CustomXmlSerializer.XmlProxyData _proxyData = new CustomXmlSerializer.XmlProxyData(typeof(DetectionFactor))
        {
            TypeMappings = BaseDataLoader.GetListOfTypeMappings()
        };

        #endregion
    }

    public interface IDetectableType: IGameData
    {

    }

    /// <summary>
    /// TODO: merge with ResourceTypeDetectionFactor. Make them share tags. There are too many designer mistakes where the wrong tag gets used.
    /// </summary>
   /* public class EntityTypeDetectionFactor : IXmlSerializable
    {
        public string EntityTypeKeyName;
        public string EntityTypeTag;

        /// <summary>
        /// NEW: if true, the entity can only be detected when the agent is actively searching (for it???)
        /// </summary>
        public bool OnlyActiveDetection = false;


        /// <summary>
        /// entities within this distance are immediately detected (set to 0 to disable)
        /// </summary>
        public float? DistanceToAlwaysDetect;

        public bool ShouldSerializeDistanceToAlwaysDetect()
        {
            return DistanceToAlwaysDetect != null;
        }

        public float DetectionFactor;


        /// <summary>
        /// the list of entity types that have specific detection factors defined
        /// </summary>
        [XmlIgnore]
        public List<EntityType> EntityTypes = new List<EntityType>();


        /// <summary>
        /// an optional skill to modify the standard detection ability
        /// </summary>
        public SkillType SkillToUse;


        public void PostLoadContentInitialize()
        {
            // requires all types to have been loaded.
            if (!string.IsNullOrEmpty(EntityTypeTag))
            {
                EntityTypes = GameData.Instance.EntityTypeDetectionByTag[EntityTypeTag];
            }

            if (!string.IsNullOrEmpty(EntityTypeKeyName))
            {
                EntityType t = GameData.Instance.AllEntityTypes[EntityTypeKeyName];
                if (!EntityTypes.Contains(t))
                {
                    EntityTypes.Add(t);
                }
            }
        }


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

        public static readonly CustomXmlSerializer.XmlProxyData _proxyData = new CustomXmlSerializer.XmlProxyData(typeof(EntityTypeDetectionFactor))
        {
            TypeMappings = BaseDataLoader.GetListOfTypeMappings()
        };

        #endregion
    }*/
}
