using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UWGame.SimSide.Items;
using UWGame.SimSide.AllGameData;

namespace UWGame.SimSide.Entities.Body
{
   // [XmlInclude(typeof(BiologicalBodyPartType)), XmlInclude(typeof(BodyPartType))]
    public class MachineBodyPartType: BodyPartType, IXmlSerializable
    {
       // public BodyPartType Parent;


       // public Dictionary<EntityType, int> MadeOf; 

        /// <summary>
        /// if true, all BodyParts inside will get protection from the elements
        /// </summary>
        //public bool IsEnclosed;
       // public bool IsInternal;

        /// <summary>
        /// time needed to put this body part together with the others!
        /// </summary>
        public float ManSecondsOfWorkNeeded;

        
        // the entity functions affected by this body part.
       // public MachineBodyPartFunction[] MachineFunctions;

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

        // SERIALIZEDICTIONARY
        public static readonly CustomXmlSerializer.XmlProxyData _proxyData = new CustomXmlSerializer.XmlProxyData(typeof(MachineBodyPartType))
        {
            TypeMappings = BaseDataLoader.GetListOfTypeMappings()

            /*TypeMappings = new List<CustomXmlSerializer.XmlTypeMappingBase>() 
                {                   
                    new CustomXmlSerializer.XmlTypeMapping<Dictionary<EntityType, int>, KVP<string, int>[]>()
                    {
                        GetterMethod = GameDataLoader.SerializeEntityTypeIntDictionary,
                        SetterMethod = GameDataLoader.DeserializeEntityTypeIntDictionary                      
                    }  
                }*/
        };



        #endregion
    }
}
