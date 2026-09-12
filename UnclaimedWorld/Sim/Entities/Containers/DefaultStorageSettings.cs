using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.Entities.Containers
{

    /// <summary>
    /// This defines the items that may be stored in the structure by default. 
    /// If the value is true, it means storage is allowed -  this structure will then be the preferred storage for that item. 
    /// if false, storage is not permitted.
    /// The value can also be left undefined - then the default is true. 
    /// 
    /// item tag is a shorthand for naming all item types individually. all the different tags can be used: food/ammo/fuel...
    /// 
    /// in case of overlap, this priority is used: 
    /// entitytype overrides item tag.
    /// </summary>
    public class DefaultStorageSettings: IGameData
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

        public string Comments;

        public bool DeleteRecord
        {
            get;
            set;
        }

        /// <summary>
        ///  item tag is a shorthand for naming all item types individually. all the different tags can be used: food/ammo/fuel...
        ///  the item setting applied here can be overriden by MayStockpileEntityType.
        /// </summary>
        public SerializableDictionary<string, int> MayStockpileItemTag;

        /// <summary>
        /// item settings will override the catgeory settings.      
        /// </summary>
        public SerializableDictionary<string, bool> MayStockpileCategory;

        /// <summary>
        /// it may be safer in the long run to use the MayStockpileItemTag collection instead of this!
        /// i.e. use a tag 'knife' instead of defining item:advancedKnife, item:improvisedKnife etc.
        /// </summary>
        public SerializableDictionary<string, int> MayStockpileEntityType;



        [XmlIgnore]
        public Dictionary<EntityType, int> MayStockpileItemFinal
        {
            get;
            private set;
        }
       
        [XmlIgnore]
        public Dictionary<EntityCategory, bool> MayStockpileCategoryFinal 
        {
            get;
            private set;
        }


        public void PreInitValidate(ref List<string> errors)
        {
            
        }

        public void Initialize()
        {
            
        }

        public void PostInitValidate(ref List<string> errors)
        {
            
        }

        public void PostLoadContentInitialize()
        {
            if (MayStockpileItemTag != null)
            {               
                foreach (var tag in MayStockpileItemTag)
                {
                    // go through all tag collections until a match is found:
                    if (TryTagCollection(GameData.Instance.AmmoByTag, tag))
                    {
                        continue;
                    }

                    if (TryTagCollection(GameData.Instance.FoodByTag, tag))
                    {
                        continue;
                    }

                    if (TryTagCollection(GameData.Instance.ToolsByTag, tag))
                    {
                        continue;
                    }

                    if (TryTagCollection(GameData.Instance.FuelByTag, tag))
                    {
                        continue;
                    }
                }
            }

            if (MayStockpileCategory != null)
            {
                MayStockpileCategoryFinal = new Dictionary<EntityCategory,bool>();

                foreach (var item in MayStockpileCategory)
                {
                    MayStockpileCategoryFinal.Add(GameData.Instance.AllEntityCategories[item.Key], item.Value);
                }
            }

            if (MayStockpileEntityType != null)
            {
                foreach (var item in MayStockpileEntityType)
                {
                    AddItem(GameData.Instance.AllEntityTypes[item.Key], item.Value);                   
                }
            }
        }

        public void PostDataCompleteInitialize()
        {
        }

        public void PostDataCompleteValidate(ref List<string> listOfErrors)
        {
        }
        public void PreDataCompleteValidate(ref List<string> listOfErrors) { }
       
        private bool TryTagCollection(Dictionary<string, List<EntityType>> tagCollection, KeyValuePair<string, int> tag)
        {
            List<EntityType> list;
            if (tagCollection.TryGetValue(tag.Key, out list))
            {
                foreach (var item in list)
                {
                    AddItem(item, tag.Value);
                }

                return true;

            }

            return false;
        }

        private void AddItem(EntityType item, int value)
        {
            if (MayStockpileItemFinal == null)
                MayStockpileItemFinal = new Dictionary<EntityType, int>();


            if (MayStockpileItemFinal.ContainsKey(item))
            {
                MayStockpileItemFinal[item] = value;
            }
            else
            {
                MayStockpileItemFinal.Add(item, value);
            }
           
        }


    }
}
