using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Xml;

namespace UWGame.SimSide.XmlCollections
{
    /// <summary>
    /// Represents a first-in, first-out serializable collection of objects.
    /// </summary>
    /// <typeparam name="T">The type of the items on the queue.</typeparam>
    public class SerializableQueue<T> : Queue<T>, IXmlSerializable
    {
        // store list type
        private readonly Type m_type = typeof(T);

        /// <summary>
        /// Returns a string that represents the current SerializableDictionary.
        /// </summary>
        /// <returns>
        /// A string that represents the current SerializableDictionary.
        /// </returns>
        public override string ToString()
        {
            return SerializableGenerics.GetTypeName(GetType());
        }

        #region IXmlSerializable Members

        /// <summary>
        /// This property is reserved, apply the System.Xml.Serialization.XmlSchemaProviderAttribute
        /// to the class instead.
        /// </summary>
        /// <returns>
        /// An System.Xml.Schema.XmlSchema that describes the XML representation of the
        /// object that is produced by the System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter)
        /// method and consumed by the System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader)
        /// method.
        /// </returns>
        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        /// <summary>
        /// Generates an object from its XML representation.
        /// </summary>
        /// <param name="reader">The System.Xml.XmlReader stream from which the object is deserialized.</param>
        void IXmlSerializable.ReadXml(XmlReader reader)
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

                    // Enqueue item on queue
                    Enqueue(item);
                }

                // Read end element and move to content
                reader.ReadEndElement();
            }

            reader.MoveToContent();
        }

        /// <summary>
        /// Converts an object into its XML representation.
        /// </summary>
        /// <param name="writer">The System.Xml.XmlWriter stream to which the object is serialized.</param>
        void IXmlSerializable.WriteXml(XmlWriter writer)
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
