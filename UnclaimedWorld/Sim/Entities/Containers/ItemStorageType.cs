using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Items;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Jobs;
using Xclna.Xna.Animation;
using System.Xml.Serialization;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.Entities.Containers
{
    
    public class ItemStorageType 
    {
        /// <summary>
        /// the storage spaces must share the same total capacity in the compartment.
        /// 
        /// StorageCondition.Keyname is key
        /// </summary>
        public SerializableDictionary<string, StorageType> StorageSpaces = new SerializableDictionary<string, StorageType>();
      

        public float? FullStatePercentage;
        public float? HalfFullStatePercentage;

       
        // maybe include info on which items we can carry? Pile item types in a hopper for instance?
        public ItemStorageType()
        {           
        }

        public ItemStorageType(float isolatedCapacity)
        {
          //  StorageSpaces.Add(Storage.Conditions.Isolated, new StorageType() { Capacity = isolatedCapacity });
            StorageSpaces.Add("isolated", new StorageType() { Capacity = isolatedCapacity });
        }

      /*  public ItemStorageType(Storage.Conditions condition1, float capacity1, Storage.Conditions? condition2 = null, float? capacity2 = null,
            Storage.Conditions? condition3 = null, float? capacity3 = null)*/
        public ItemStorageType(string condition1, float capacity1, 
            string condition2 = null, float? capacity2 = null,
            string condition3 = null, float? capacity3 = null)
     
        {
           
            StorageSpaces.Add(condition1, new StorageType() { Capacity = capacity1 });

            if (condition2 != null)
            {
                StorageSpaces.Add(condition2, new StorageType() { Capacity = capacity2.Value });
            }

            if (condition3 != null)
            {
                StorageSpaces.Add(condition3, new StorageType() { Capacity = capacity3.Value });
            }
        }

        public void Initialize()
        {
            // add default storage (to keep items when the other stores are filled, but there is still Total Capacity left.)
            if (!StorageSpaces.ContainsKey("isolated"))
            {
                StorageSpaces.Add("isolated", new StorageType() { Capacity = 0f });
            }
           /* if (!StorageSpaces.ContainsKey(Storage.Conditions.Isolated))
            {
                StorageSpaces.Add(Storage.Conditions.Isolated, new StorageType() { Capacity = 0f });
            }*/
        }

       

        public float GetTotalCapacity()
        {
            float total = 0f;
            foreach (var kvp in StorageSpaces)
            {
                total += kvp.Value.Capacity;
            }

            return total;
        }


        /*
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

        public static readonly CustomXmlSerializer.XmlProxyData _proxyData = new CustomXmlSerializer.XmlProxyData(typeof(ItemStorageType))
        {
            TypeMappings = new List<CustomXmlSerializer.XmlTypeMappingBase>() 
                {
                    new CustomXmlSerializer.XmlTypeMapping<Dictionary<Storage.Conditions, StorageType>, KVP<Storage.Conditions, StorageType>[]>()
                    {
                        GetterMethod = t => {
                            if (t == null) return null;
                            var d = new KVP<Storage.Conditions, StorageType>[t.Count];
                            int index = 0;
                            foreach (KeyValuePair<Storage.Conditions, StorageType> kvp in t)
	                        {
                        	    d[index] = new KVP<Storage.Conditions, StorageType>(kvp.Key, kvp.Value);	
                                index++;
	                        }
                            return d;                            
                        },

                        SetterMethod = s => { 
                            if (s == null) return null; 
                            var d = new Dictionary<Storage.Conditions, StorageType>(); 
                            foreach (var el in s) 
                            { 
                                d[el.Key] = el.Value; 
                            } 
                            return d; 
                        }
                    }    
                }
        };

        #endregion
         */
    }

    /// <summary>
    /// storage types are used to divide compartment capacity into different sections that all share the same total capacity
    /// 
    /// why make this tiny class..?
    /// </summary>
    public class StorageType
    {
        /// <summary>
        /// must be read only!
        /// </summary>
        public float Capacity;

    }

    
}
