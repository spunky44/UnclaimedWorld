using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml.Serialization;
using UWGame.SimSide.Entities.Skills;

namespace UWGame.SimSide.Entities
{
    [XmlRoot("Skill")]
    [DebuggerDisplay("{KeyName}")]
    public class SkillType : IXmlSerializable, IGameData, IHasCategory<SkillCategory>
    {
        public string Name { get; set; }
        public string KeyName { get; set; }
        public bool DeleteRecord
        {
            get;
            set;
        }

        public string Description;


        public string ProfessionKey;

        public SkillCategory Category { get; set; }

        [XmlIgnore]
        public ProfessionType ProfessionType;


        public bool SuppressDisplayForPersons;
        public bool SuppressDisplayForBiologicals;

        public int SortOrder;

        public bool GiveExpertSkillBonus;

      

        public SkillType() { }

        public SkillType(string key, string name) //, SkillCategory category)
        {
            this.KeyName = key;
            this.Name = name;
            //this.SkillCategory = category;
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
            if (ProfessionKey != null)
            {
                EntityType.ValidateGameDataTypeExists(ref listOfErrors, ProfessionKey, GameData.Instance.AllProfessionTypes, out ProfessionType);               

            }

        }
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

        public static readonly CustomXmlSerializer.XmlProxyData _proxyData = new CustomXmlSerializer.XmlProxyData(typeof(SkillType)) //typeof(ItemCategory))
        {
            TypeMappings = new List<CustomXmlSerializer.XmlTypeMappingBase>() 
                {
                    new CustomXmlSerializer.XmlTypeMapping<SkillCategory, string> ()
                    {
                        GetterMethod = t => t == null ? null : t.KeyName,
                        SetterMethod = s => s == null ? null : GameData.Instance.AllSkillCategories[s]
                    }               
                }
        };

        #endregion


        
    }
}
