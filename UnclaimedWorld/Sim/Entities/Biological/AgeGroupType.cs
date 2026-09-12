using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Xml.Serialization;
using UWGame.SimSide.AI.Needs;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.Entities.Biological
{
    /// <summary>
    /// is below caste types. model scale and other properties should either override or factor in the final value. make this a designer option.
    /// </summary>
    public class AgeGroupType : IXmlSerializable, IEdge
    {
        public string Name;

        public AIAgeGroup AIAgeGroup;

        public NeedType[] NeedTypes;

        public string ModelName;
        //public float? ModelScale;
        public float? ModelScaleFraction;

        public string ModelBasicTextureName;

        public Vector3? PrimaryColor;
        public Vector3? SecondaryColor;
        public Vector3? TertiaryColor;
        public Vector3? QuaternaryColor;
        public float? Size;
        public bool CanReproduce;

        
        /// <summary>
        ///  multiply the target height and weight with these modifiers to get the target for the age group:
        /// are these needed..?
        /// </summary>
        public float HeightTargetModifier;
        public float WeightTargetModifier;

        public SerializableDictionary<string, BioProperty> BioProperties;

        //public float? Resilience;

        //public List<RaceType> RaceTypes;

        /// <summary>
        /// upper end of age range
        /// </summary>
        [XmlElement("AgeUpperEnd")]
        public float Edge { get; set; }

        public bool GetBioPropertyValue(BioPropertyType propertyKey, out BioProperty property)
        {
            return BiologicalEntity.GetBioPropertyValue(propertyKey, BioProperties, out property);           
        }

        public void Validate(ref List<string> errors)
        {
            if (NeedTypes != null)
            {
                foreach (var item in NeedTypes)
                {
                    item.Validate(ref errors);
                }
            }
            
        }

        public void Initialize()
        {
            if (NeedTypes != null)
            {
                foreach (var item in NeedTypes)
                {
                    item.Initialize();
                }
            }

            ComputeNeedsNormalizedWeight();
        }

        public void PostDataCompleteInitialize()
        {
            if (NeedTypes != null)
            {
                foreach (var item in NeedTypes)
                {
                    item.PostDataCompleteInitialize();
                }
            }
        }

        private void ComputeNeedsNormalizedWeight()
        {
            float totalWeight = 0f;
            if (NeedTypes != null)
            {
                // compute normalized weights:
                foreach (var need in NeedTypes)
                {
                    if (need.DecreasedEnergyWeight > 0f)
                    {
                        totalWeight += need.DecreasedEnergyWeight;
                    }
                }

                float factorToReduceWeightsWith = 1f / totalWeight;

                foreach (var need in NeedTypes)
                {
                    if (need.DecreasedEnergyWeight > 0f)
                    {
                        need.NormalizedDecreasedEnergyWeight = need.DecreasedEnergyWeight * factorToReduceWeightsWith;
                    }
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

        public static readonly CustomXmlSerializer.XmlProxyData _proxyData = new CustomXmlSerializer.XmlProxyData(typeof(AgeGroupType))
        {
            TypeMappings = new List<CustomXmlSerializer.XmlTypeMappingBase>() 
                {
                    new CustomXmlSerializer.XmlTypeMapping<Vector3?, string> ()
                    {
                        GetterMethod = t => t == null ? null : PersonType.Vector3ToHexString(t.Value),
                        SetterMethod = s => s == null ? null : new Vector3?(PersonType.HexStringToVector3(s))
                    }/*,  
                    new CustomXmlSerializer.XmlTypeMapping<float?, string> ()
                    {
                        GetterMethod = t => t == null ? null : t.Value.ToString(),
                        SetterMethod = s => string.IsNullOrEmpty(s) ? null : new float?(float.Parse(s))
                    }   */                            
                }
        };

        #endregion
    }
}
