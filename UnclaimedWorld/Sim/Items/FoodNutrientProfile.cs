using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UWGame.SimSide.Entities;
using UWGame.SimSide.AllGameData;

namespace UWGame.SimSide.Items
{
    public class FoodNutrientProfile : IGameData, IXmlSerializable
    {
        public string KeyName { get; set; }

        public string Name { get; set; }
        public bool DeleteRecord
        {
            get;
            set;
        }
        public FoodNutrientAmount[] FoodNutrientTypes;


        public void Initialize()
        {
        }

        public void PreInitValidate(ref List<string> listOfErrors) { }
        public void PostInitValidate(ref List<string> listOfErrors) { }
        public void PostDataCompleteInitialize()
        {
        }
        public void PreDataCompleteValidate(ref List<string> listOfErrors) { }
       
        public void PostDataCompleteValidate(ref List<string> listOfErrors)
        {
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

        public static readonly CustomXmlSerializer.XmlProxyData _proxyData = new CustomXmlSerializer.XmlProxyData(typeof(FoodNutrientProfile))
        {
            TypeMappings = BaseDataLoader.GetListOfTypeMappings()
        };

        #endregion
    }
}
