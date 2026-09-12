using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.Entities.Containers
{
    /// <summary>
    /// can be held by HomeContainerType, WorkshopContainerType...
    ///    
    /// </summary>
    [DebuggerDisplay("{KeyName}")]
    public class UpgradeProfile: IGameData
    {
        public string KeyName
        {
            get;
            set;
        }
        public string Name
        {
            get;
            set;
        }
        public bool DeleteRecord
        {
            get;
            set;
        }


        public string[] UpgradeCategories;

        [XmlIgnore]
        public List<UpgradeCategory> UpgradeCategoriesFinal;

        /*
        /// <summary>
        /// UpgradeCategory key, entity tag
        /// 
        /// the entity type should have Effects, and a process type, as well as salvage process
        /// </summary>
        public SerializableDictionary<string, string> UpgradeOptions;


        [XmlIgnore]
        public Dictionary<UpgradeCategory, List<EntityType>> UpgradeEntityOptions;

        */


        public void PostDataCompleteInitialize()
        {
            UpgradeCategoriesFinal = new List<UpgradeCategory>();
            foreach (var item in UpgradeCategories)
            {
                UpgradeCategoriesFinal.Add(GameData.Instance.AllUpgradeCategories[item]);
            }

            /*
            UpgradeEntityOptions = new Dictionary<UpgradeCategory, List<EntityType>>();

            foreach (var item in UpgradeOptions)
            {
                UpgradeEntityOptions.Add(GameData.Instance.AllUpgradeCategories[item.Key], GameData.Instance.UpgradesByTag[item.Value]);
                
            }*/
        }


        public void PreInitValidate(ref List<string> errors)
        {

        }

        public void Initialize()
        {

        }

        public void PostInitValidate(ref List<string> listOfErrors)
        {
        }

      
        public void PreDataCompleteValidate(ref List<string> listOfErrors) { }

        public void PostDataCompleteValidate(ref List<string> listOfErrors)
        {
        }


    }
}
