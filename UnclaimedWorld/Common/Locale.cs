using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using UWGame.SimSide.AllGameData;

namespace UWGame  
{
    public class Locale
    {
        private static Dictionary<string, string> InvariantStrings;

        private static Dictionary<string, string> CurrentStrings;

        /// <summary>
        /// load invariant strings
        /// </summary>
        public static void Init()
        {           
            // we cannot serialize dictionaries directly. Use a list instead...
            // do this once to form the xml file:

            

        /*    List<String> invariantStrings = new List<String>();          

            invariantStrings.Add(new String(){ Key = "MyKey", Value = "MyValue"});
            
            //Create our own namespaces for the output
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            //Add an empty namespace and empty value
            ns.Add("", "");     

            using (TextWriter w = new StreamWriter(UWGame.SimSide.DataFolder + "Strings/en-US.xml"))
            {

                s.Serialize(w, invariantStrings, ns);
            }*/

            Load(ref InvariantStrings, "English (US)");

            CurrentStrings = InvariantStrings; // we use this for now.
        }



        public static string Get(string key)
        {
            string value;
            if (CurrentStrings.TryGetValue(key, out value))
            {
                return value;
            }
            else return InvariantStrings[key];
        }

        public static List<string> GetCultures()
        {
            List<string> cultures = new List<string>();

            string folderPath = Config.GetDataFolderPath(Config.DataType.BaseData, "Strings", "");
            string[] files = System.IO.Directory.GetFiles(folderPath /*GameDataLoader.DataFolder + "Strings"*/, "*.xml");

            foreach (string file in files)
            {
                cultures.Add(Path.GetFileNameWithoutExtension(file));                
            }

            return cultures;
        }

        public static void Load(ref Dictionary<string, string> dictionary, string fileName)
        {
            XmlSerializer s = new XmlSerializer(typeof(List<String>));
            List<String> invList;

            string path = Config.GetDataFolderPath(Config.DataType.BaseData, "Strings", fileName + ".xml");
            using (TextReader r = new StreamReader(path)) // GameDataLoader.DataFolder + string.Format("Strings/{0}.xml", fileName)))
            {
                invList = (List<String>)s.Deserialize(r);
            }

            if (dictionary != null)
            {
                dictionary.Clear();
            }
            else
            {
                dictionary = new Dictionary<string, string>();
            }

            // place in dictionary:
            foreach (String st in invList)
            {
                dictionary.Add(st.Key, st.Value);
            }
        }

    }


    public class String
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
