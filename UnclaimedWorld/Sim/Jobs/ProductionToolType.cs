using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Items;
using System.Xml.Serialization;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.Jobs
{
    public class ProductionToolType: IXmlSerializable
    {
        public EntityType ToolItem;
        
        public float ProductivityBonus;


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

         public static readonly CustomXmlSerializer.XmlProxyData _proxyData = new CustomXmlSerializer.XmlProxyData(typeof(ProductionToolType))
         {
             TypeMappings = new List<CustomXmlSerializer.XmlTypeMappingBase>() 
                {                    
                    new CustomXmlSerializer.XmlTypeMapping<EntityType, string> ()
                    {
                        GetterMethod = t => t == null ? null : t.KeyName,
                        SetterMethod = s => s == null ? null : GameData.Instance.AllItemTypes[s]
                    }                   
                }
         };

         #endregion

    }
}
