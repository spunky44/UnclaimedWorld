using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Xml.Serialization;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.Entities.Biological
{
   /// <summary>
   /// is parallel to caste types.
   /// model scale should probably be a factor along with other properties...
   /// </summary>
    public class RaceType : IXmlSerializable, IEdge
    {
        public string KeyName;
        
        /// <summary>
        /// displayed name
        /// </summary>
        public string Name;

        public string ModelName;

        public string ModelBasicTextureName;

        /// <summary>
        /// additional model textures to select from
        /// </summary>
        public string[] ModelBasicTextureNames;

        public float? ModelScale;

        public string Description;

        /// <summary>
        /// not used?
        /// </summary>
        public string PortraitSkinType;

        public Vector3? PrimaryColor;      
        public List<ColorProbability> SecondaryColorProbabilityEdges;
        public Vector3? TertiaryColor;
        public Vector3? QuaternaryColor;
        public float? Size;

       // public float? ResilienceMean;
    //    public float? ResilienceStandardDeviation;

       

        [XmlElement("ProbabilityEdge")]
        public float Edge { get; set; }

        public SerializableDictionary<string, BioProperty> BioProperties;

    //    [XmlIgnore]
      //  private Dictionary<BioPropertyType, BioProperty> bioProperties;
        //private SerializableDictionary<BioPropertyType, BioProperty> BioProperties;

        public void Initialize(int raceNo)
        {            

            if (string.IsNullOrEmpty(Name))
            {
                Name = "Race #" + raceNo;
            }

           
        }

        public bool GetBioPropertyValue(BioPropertyType propertyKey, out BioProperty property)
        {
            return BiologicalEntity.GetBioPropertyValue(propertyKey, BioProperties, out property);
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

        public static readonly CustomXmlSerializer.XmlProxyData _proxyData = new CustomXmlSerializer.XmlProxyData(typeof(RaceType))
        {
            TypeMappings = new List<CustomXmlSerializer.XmlTypeMappingBase>() 
                {
                    new CustomXmlSerializer.XmlTypeMapping<Vector3?, string> ()
                    {
                        GetterMethod = t => !t.HasValue ? null : PersonType.Vector3ToHexString(t.Value),
                        SetterMethod = s => s == null ? null : new Vector3?(PersonType.HexStringToVector3(s))
                    }/*,                     
                    new CustomXmlSerializer.XmlTypeMapping<float?, string> ()
                    {
                        GetterMethod = t => t == null ? null : t.Value.ToString(),
                        SetterMethod = s => string.IsNullOrEmpty(s) ? null : new float?(float.Parse(s))
                    }  */
               
                }
        };

        #endregion

    }
}
