using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UWGame.SimSide.Entities
{
    /// <summary>
    /// skill categories seem to be broader than Professions, for instance Science...
    /// </summary>
    public class SkillCategory : IGameData /*IDataType*/, ICategoryType
    {
        public string Name { get; set; }
     
       
        [XmlIgnore]
        public string IconSpriteName { get; set; }

        
        public string KeyName
        {
            get;
            set;
        }
        public bool DeleteRecord
        {
            get;
            set;
        }
        public void Initialize() { }
        public void PreInitValidate(ref List<string> listOfErrors) { }
        public void PostInitValidate(ref List<string> listOfErrors) { }
        public void PostDataCompleteInitialize()
        {
        }
        public void PreDataCompleteValidate(ref List<string> listOfErrors) { }
       
        public void PostDataCompleteValidate(ref List<string> listOfErrors)
        {
        }
    }
}
