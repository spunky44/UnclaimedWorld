using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UWGame.SimSide;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Items;

namespace UWGame.ClientSide.Interface.Inventory
{
    [XmlInclude(typeof(CategoryFilterSettingType))]
    [XmlInclude(typeof(StaticFilterSettingType))]
    [XmlInclude(typeof(NutrientFilterSettingType))]
    [XmlInclude(typeof(TierFilterSettingType))]
    [XmlInclude(typeof(PolicyAreaFilterSettingType))]        
    public abstract class FilterSettingType: IGameData
    {
      
        public string KeyName
        {
            get;
            set;
        }

        public string Name { get; set; }

        public bool DeleteRecord
        {
            get;
            set;
        }



        public string GetDisplayName()
        {
            return Name ?? GetDefaultDisplayName();
        }

        public abstract string GetDefaultDisplayName();
        

        public virtual void Initialize()
        {
        }

        public virtual void PreInitValidate(ref List<string> listOfErrors) { }
        public virtual void PostInitValidate(ref List<string> listOfErrors) { }
        public virtual void PostDataCompleteInitialize()
        {
        }
        public virtual void PreDataCompleteValidate(ref List<string> listOfErrors) { }
       
        public virtual void PostDataCompleteValidate(ref List<string> listOfErrors)
        {
        }

       // public abstract Dictionary<string, EntityType> GetData();
        public abstract HashSet<EntityType> GetData(Predicate<EntityType> filter);
       
    }
}
