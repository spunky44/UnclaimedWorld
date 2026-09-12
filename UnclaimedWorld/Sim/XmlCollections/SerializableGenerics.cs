using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.SimSide.XmlCollections
{
    
    /*
     The following exceptions are thrown when you try to serialize a any of the types (except for List) in the System.Collections.Generic namespace:

A first chance exception of type 'System.NotSupportedException' occurred in System.Xml.dll
System.NotSupportedException: The type System.Collections.Generic.Dictionary`2[[System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]] is not supported because it implements IDictionary.
     
     */

    public static class SerializableGenerics
    {
        private const string OF = "Of";

        /// <summary>
        /// Gets the key-value pair name
        /// </summary>
        /// <param name="tKey"></param>
        /// <param name="tValue"></param>
        /// <returns></returns>
        public static string GetKeyValuePairName(Type tKey, Type tValue)
        {
            // return key-value pair name
            return GetTypeName(tKey) + GetTypeName(tValue);
        }

        /// <summary>
        /// Returns the name of the type
        /// </summary>
        /// <param name="type">Type to generate the name from</param>
        /// <returns>The name of the type</returns>
        public static string GetTypeName(Type type)
        {
            string typeName;

            // Is type generic
            if (type.IsGenericType)
            {
                // Get type name - Generics 
                typeName = type.Name.Substring(0, type.Name.Length - 2);
                typeName += OF;

                // Get type's arguments
                Type[] types = type.GetGenericArguments();
                foreach (Type t in types)
                {
                    // Append type's name
                    typeName += GetTypeName(t);
                }
            }
            else if (type.IsArray)
            {
                // Compose array name as "ArrayOf" and get array element's type name
                typeName = type.BaseType.Name;
                typeName += OF;
                typeName += GetTypeName(type.GetElementType());
            }
            else
            {
                // Append type's name
                typeName = type.Name;
            }

            // return type's name
            return typeName;
        }
    }

}
