using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Xml.Serialization;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.Entities.Biological
{
    public enum Reproduction { Male, Female, None, Self }
    public class CasteType : IXmlSerializable, IEdge
    {
        public string KeyName;

        public string Name;

        public List<AgeGroupType> AgeGroupTypes;

        /// <summary>
        /// let's compute the max age...
        /// </summary>
        [XmlIgnore]
        public float MaxAge;

        public string ModelName;
        public float? ModelScale;

        public string ModelBasicTextureName;

        public Vector3? PrimaryColor;
        public Vector3? SecondaryColor;
        public Vector3? TertiaryColor;
        public Vector3? QuaternaryColor;

       // [XmlElement(IsNullable = false)] 
        public float? Size;
        public Reproduction Reproduction;
                
        /// <summary>
        /// gives adult target weight
        /// in kilos
        /// </summary>
        public float WeightMean;
        public float WeightStandardDeviation;

        /// <summary>
        /// gives adult target height
        /// in meters
        /// </summary>
        public float HeightMean;
        public float HeightStandardDeviation;
                                        /*HeightMean = 1.8f, HeightStandardDeviation = 0.08f, 
                                        WeightMean = */

      //  public float? ResilienceMean;
      //  public float? ResilienceStandardDeviation;

        public SerializableDictionary<string, BioProperty> BioProperties;

        public void Initialize(int casteNo)
        {
            MaxAge = 0f;
            foreach (AgeGroupType ageGroupType in AgeGroupTypes)
            {
                ageGroupType.Initialize();

                // compute the max age, it can be useful:
                if (ageGroupType.Edge > MaxAge)
                {
                    MaxAge = ageGroupType.Edge;
                }
            }

            if (string.IsNullOrEmpty(Name))
            {
                Name = "Caste #" + casteNo;
            }

        }

        public void PostDataCompleteInitialize()
        {
            foreach (AgeGroupType ageGroupType in AgeGroupTypes)
            {
                ageGroupType.PostDataCompleteInitialize();
            }
        }

        public bool GetBioPropertyValue(BioPropertyType propertyKey, out BioProperty property)
        {
            return BiologicalEntity.GetBioPropertyValue(propertyKey, BioProperties, out property);
        }

        static int noOfAgeGroupTypes = Enum.GetValues(typeof(AIAgeGroup)).Length;

        public void Validate(ref List<string> errors)
        {
            EntityType.ValidateRequiredValue(ref errors, "Caste KeyName", KeyName != null);

            if (AgeGroupTypes.Count != noOfAgeGroupTypes)
            {
                EntityType.CreateValidationError(ref errors, KeyName + ": All age group types must be defined.");              
            }

            int lastAgeGroupAI = -1;
            foreach (AgeGroupType ageGroup in AgeGroupTypes)
            {
                if ((int)ageGroup.AIAgeGroup <= lastAgeGroupAI)
                {
                    EntityType.CreateValidationError(ref errors, KeyName + ": The age group AI types are not in the correct sequence.");
                }

                lastAgeGroupAI = (int)ageGroup.AIAgeGroup;

                ageGroup.Validate(ref errors);
            }

           
        }

        [XmlElement("ProbabilityEdge")]
        public float Edge { get; set; }

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

        public static readonly CustomXmlSerializer.XmlProxyData _proxyData = new CustomXmlSerializer.XmlProxyData(typeof(CasteType))
        {
            TypeMappings = new List<CustomXmlSerializer.XmlTypeMappingBase>() 
                {
                    new CustomXmlSerializer.XmlTypeMapping<Vector3?, string> ()
                    {
                        GetterMethod = t => t == null ? null : PersonType.Vector3ToHexString(t.Value),
                        SetterMethod = s => s == null ? null : new Vector3?(PersonType.HexStringToVector3(s))
                    }/*,  
                    new CustomXmlSerializer.XmlTypeMapping<float?, string> () // don't print nulls
                    {
                        GetterMethod = t => t == null ? null : t.Value.ToString(),
                        SetterMethod = s => string.IsNullOrEmpty(s) ? null : new float?(float.Parse(s))
                        //SetterMethod = s => s == null ? null : new float?(float.Parse(s))
                    } */         
                }
        };

        #endregion
    }

    

    public interface IEdge
    {
        float Edge { get; set; }
    }
    public class ColorProbability: IXmlSerializable, IEdge
    {
        public float Edge { get; set; }
        public Vector3 Color { get; set; }

       // public Pair() { }
        public ColorProbability(float key, Vector3 value)
        {
            Edge = key;
            Color = value;
        }
        public ColorProbability(){ }

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

        public static readonly CustomXmlSerializer.XmlProxyData _proxyData = new CustomXmlSerializer.XmlProxyData(typeof(ColorProbability))
        {
            TypeMappings = new List<CustomXmlSerializer.XmlTypeMappingBase>() 
                {
                    new CustomXmlSerializer.XmlTypeMapping<Vector3, string> ()
                    {
                        GetterMethod = t => t == null ? null : PersonType.Vector3ToHexString(t),
                        SetterMethod = s => PersonType.HexStringToVector3(s)
                    }       
                }
        };

        #endregion
    }

  /*  public class Pair<K, V> : IXmlSerializable
    {
        public K Key { get; set; }
        public V Value { get; set; }

        public Pair() { }

        public Pair(K key, V value)
        {
            Key = key;
            Value = value;
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

        public static readonly CustomXmlSerializer.XmlProxyData _proxyData = new CustomXmlSerializer.XmlProxyData(typeof(Pair<float, Vector3>))
        {
            TypeMappings = new List<CustomXmlSerializer.XmlTypeMappingBase>() 
                {
                    new CustomXmlSerializer.XmlTypeMapping<Vector3, string> ()
                    {
                        GetterMethod = t => t == null ? null : PersonType.Vector3ToHexString(t),
                        SetterMethod = s => PersonType.HexStringToVector3(s)
                    }       
                }
        };

        #endregion
    }
    */


    
}
