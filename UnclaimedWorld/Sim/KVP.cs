using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UWGame.SimSide
{
    /// <summary>
    /// To be used by XmlSerializer. The built in KeyValuePair cannot be serialized properly... no setters.
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <typeparam name="V"></typeparam>
    [Serializable]
    [XmlRoot(ElementName="KeyValuePair")]
    public class KVP<K, V>
    {
        
        public K Key { get; set; }
        public V Value { get; set; }

        public KVP() { }

        public KVP(K key, V value)
        {
            Key = key;
            Value = value;
        }
    }
}
