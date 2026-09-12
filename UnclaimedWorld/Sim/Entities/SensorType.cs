using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Xml.Serialization;
using UWGame.SimSide.Maps;
using UWGame.SimSide.AllGameData;

namespace UWGame.SimSide.Entities
{
    public class SensorType: IXmlSerializable
    {
        // only vision...?

        /// <summary>
        /// default values are set...
        /// </summary>
        public float Range = 480; // 10;        
        public float RangeAtNight = 200;// = 4;

        public float PowerNeeds;


        public string DetectionTypeKey;

        [XmlIgnore]
        public DetectionType DetectionType
        {
            get;
            private set;
        }


       


        public void Initialize()
        {
            

          //  DetectionType.Initialize(); // was already init'ed
        }


        public void PostInitValidate(ref List<string> listOfErrors)
        {


            //DetectionType.PostInitValidate(listOfErrors);  // was already init'ed
        }

        public void PostLoadContentInitialize()
        {
            if (!string.IsNullOrEmpty(DetectionTypeKey))
            {
                DetectionType = GameData.Instance.AllDetectionTypes[DetectionTypeKey];
            }

            if (DetectionType == null)
            {
                DetectionType = new DetectionType(); // default
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

        public static readonly CustomXmlSerializer.XmlProxyData _proxyData = new CustomXmlSerializer.XmlProxyData(typeof(SensorType))
        {
            // use placeholders!
            TypeMappings = BaseDataLoader.GetListOfTypeMappings(true)
        };

        #endregion
    }
}
