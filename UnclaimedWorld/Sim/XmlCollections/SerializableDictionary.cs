using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Linq;

namespace UWGame.SimSide.XmlCollections
{
   

    [XmlRoot("dictionary")]
    public class SerializableDictionary<TKey, TValue>
        : Dictionary<TKey, TValue>, IXmlSerializable
    {
        public SerializableDictionary()
            : base()
        {

        }

        public SerializableDictionary(IDictionary<TKey, TValue> dictionary)
            : base(dictionary)
        {

        }

        public SerializableDictionary(IEqualityComparer<TKey> comparer)
            : base(comparer)
        {

        }

        public SerializableDictionary(int capacity)
            : base(capacity)
        {

        }

        public SerializableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
            : base(dictionary, comparer)
        {

        }

        public SerializableDictionary(int capacity, IEqualityComparer<TKey> comparer)
            : base(capacity, comparer)
        {

        }

        protected SerializableDictionary(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }


        #region IXmlSerializable Members

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;

        }



        public void ReadXml(System.Xml.XmlReader reader)
        {
            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

            bool wasEmpty = reader.IsEmptyElement;
            reader.Read();

            if (wasEmpty)
                return;
            // look up based on prefix
            while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
            {
                reader.ReadStartElement("item");
                reader.ReadStartElement("key");

                TKey key = (TKey)keySerializer.Deserialize(reader);

                reader.ReadEndElement();
                reader.ReadStartElement("value");
                TValue value = (TValue)valueSerializer.Deserialize(reader);

                reader.ReadEndElement();
                this.Add(key, value);

                reader.ReadEndElement();
                reader.MoveToContent();

            }
            reader.ReadEndElement();
        }



        public void WriteXml(System.Xml.XmlWriter writer)
        {
            XmlSerializer keySerializer;
            XmlSerializer valueSerializer;

            if (this.Count > 0)
            {
                KeyValuePair<TKey, TValue> kvp = this.First();
                if (kvp.Key is IGameData)
                {
                    keySerializer = new XmlSerializer(typeof(string));
                }
                else
                {
                    keySerializer = new XmlSerializer(typeof(TKey));
                }

                if (kvp.Value is IGameData)
                {
                    valueSerializer = new XmlSerializer(typeof(string));
                }
                else
                {
                    valueSerializer = new XmlSerializer(typeof(TValue));
                }



                foreach (TKey key in this.Keys)
                {
                    writer.WriteStartElement("item");
                    writer.WriteStartElement("key");

                    if (key is IGameData)
                    {   // how do we store type information? with prefix? Item:cement
                        keySerializer.Serialize(writer, ((IGameData)key).KeyName);
                    }
                    else
                    {
                        keySerializer.Serialize(writer, key);
                    }

                    writer.WriteEndElement();
                    writer.WriteStartElement("value");


                    TValue value = this[key];

                    if (value is IGameData)
                    {
                        valueSerializer.Serialize(writer, ((IGameData)value).KeyName);
                    }
                    else
                    {
                        valueSerializer.Serialize(writer, value);
                    }

                    writer.WriteEndElement();
                    writer.WriteEndElement();

                }
            }
        }
        #endregion

        
    }


}
