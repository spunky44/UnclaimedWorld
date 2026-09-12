using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.Entities
{
    /// <summary>
    /// this is a client class.
    /// </summary>
    [DebuggerDisplay("{KeyName}")]
    public class EntityCategory: ICategoryType, IHasIcon, IGameData
    {
        public enum CategoryColors { None, Blue, Green, Red }

        public CategoryColors CategoryColor;

        public bool DisplayStructureIcon;

        /// <summary>
        /// controls logging of disappeared items
        /// </summary>
        public bool IsWaste;

        public string Name { get; set; }
        public bool DeleteRecord
        {
            get;
            set;
        }
        public string SpriteName;

        [XmlIgnore]
        public string IconSpriteName { get; set; }

        public int SortOrder = 1000;

        public void Initialize()
        {
            IconSpriteName = SpriteName + "_icon";
        }
        public void PreInitValidate(ref List<string> listOfErrors) { }
        public void PostInitValidate(ref List<string> listOfErrors) { }

        public string KeyName
        {
            get;
            set;
        }


        public void PostDataCompleteInitialize()
        {
        }
        public void PreDataCompleteValidate(ref List<string> listOfErrors) { }
       
        public void PostDataCompleteValidate(ref List<string> listOfErrors)
        {
        }
    }
}
