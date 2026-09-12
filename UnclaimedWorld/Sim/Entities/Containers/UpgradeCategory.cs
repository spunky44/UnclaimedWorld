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
    /// an upgrade 'slot'
    /// </summary>
    [DebuggerDisplay("{KeyName}")]
    public class UpgradeCategory: IGameData
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

        public string Description;


        public bool DeleteRecord
        {
            get;
            set;
        }

        public int SortOrder;

       // public string EntityTag;
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

       /* [XmlIgnore]
        public List<EntityType> EntityTypes;
        */

        public void PreInitValidate(ref List<string> errors)
        {

        }

        public void Initialize()
        {

        }

        public void PostInitValidate(ref List<string> listOfErrors)
        {    
        }

        public void PostDataCompleteInitialize()
        {
          //  EntityTypes = GameData.Instance.UpgradesByTag[EntityTag];

            /*
            UpgradeEntityOptions = new Dictionary<UpgradeCategory, List<EntityType>>();

            foreach (var item in UpgradeOptions)
            {
                UpgradeEntityOptions.Add(GameData.Instance.AllUpgradeCategories[item.Key], GameData.Instance.UpgradesByTag[item.Value]);

            }*/
        }

        public void PreDataCompleteValidate(ref List<string> listOfErrors) { }

        public void PostDataCompleteValidate(ref List<string> listOfErrors)
        {
        }
    }
}
