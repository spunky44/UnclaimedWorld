using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Linq;
using System.Xml;

namespace UWGame.SimSide.XmlCollections
{
   

    [XmlRoot("hashset")]
    public class SerializableHashSet<T>
        : HashSet<T>, IXmlSerializable
    {
        // store list type
        private readonly Type m_type = typeof(T);


        public SerializableHashSet()
            : base()
        {

        }

       /* public SerializableHashSet(IDictionary<TKey> dictionary)
            : base(dictionary)
        {

        }*/

        public SerializableHashSet(IEqualityComparer<T> comparer)
            : base(comparer)
        {

        }

       /* public SerializableHashSet(int capacity)
            : base(capacity)
        {

        }*/

      /*  public SerializableHashSet(IDictionary<TKey> dictionary, IEqualityComparer<TKey> comparer)
            : base(dictionary, comparer)
        {

        }

        public SerializableHashSet(int capacity, IEqualityComparer<TKey> comparer)
            : base(capacity, comparer)
        {

        }*/

        protected SerializableHashSet(SerializationInfo info, StreamingContext context)
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
            // Create xml serializer for type
            XmlSerializer typeSerializer = new XmlSerializer(m_type);

            bool isEmptyElement = reader.IsEmptyElement;

            // Read start element and move to content
            reader.ReadStartElement();

            if (!isEmptyElement)
            {
                reader.MoveToContent();

                // Loop through elements
                while (reader.NodeType != XmlNodeType.EndElement)
                {
                    // Deserialize type
                    T item = (T)typeSerializer.Deserialize(reader);

                    Add(item);
                }

                // Read end element and move to content
                reader.ReadEndElement();
            }

            reader.MoveToContent();
        }



        public void WriteXml(System.Xml.XmlWriter writer)
        {
            // Create xml serializer for type
            XmlSerializer typeSerializer = new XmlSerializer(m_type);

            IEnumerator<T> enumerator = GetEnumerator();
            while (enumerator.MoveNext())
            {
                // Serialize type
                typeSerializer.Serialize(writer, enumerator.Current);
            }
        }
        #endregion

        
    }


}
