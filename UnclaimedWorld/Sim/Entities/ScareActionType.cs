using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities.Body;
using System.Xml.Serialization;
using UWGame.SimSide.AllGameData;
using UWGame.SimSide.Combat;

namespace UWGame.SimSide.Entities
{
    public class ScareActionType : IXmlSerializable
    {
       // public IntelligenceType Parent;
              
        public BodyPartType[] DependsOn;

        float Period;

        float EnergyCost;

        public string AnimationKey;
     
        public override string ToString()
        {
            return AnimationKey;
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

       
        public static readonly CustomXmlSerializer.XmlProxyData _proxyData = new CustomXmlSerializer.XmlProxyData(typeof(AttackType))
        {
            TypeMappings = BaseDataLoader.GetListOfTypeMappings()
            /*
            TypeMappings = new List<CustomXmlSerializer.XmlTypeMappingBase>() 
                {                   
                    new CustomXmlSerializer.XmlTypeMapping<BodyPartType[], string[]>()
                    {
                       
                        GetterMethod = AttackType.SerializeBodyPartTypes,                           

                        SetterMethod = AttackType.DeserializeBodyPartTypes
                                           
                    }  
                }*/
        };

        
        #endregion
    }
}
