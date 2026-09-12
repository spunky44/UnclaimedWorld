using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Maps
{
    /// <summary>
    /// This class contains stockpile settings. It can belong to a zone or a structure
    /// </summary>
    public class Stockpile: ISnapshot
    {
        public enum TypesOfStockpiles { Normal, OfferedForTrade }

        private TypesOfStockpiles stockpileType;
      
        /// <summary>
        /// true means no limit, unless overridden by item setting
        /// </summary>
        private Dictionary<EntityCategory, bool> mayStockpileCategory = new Dictionary<EntityCategory, bool>();

        /// <summary>
        /// item setting will override category setting
        /// </summary>
        private Dictionary<EntityType, int> mayStockpileItem = new Dictionary<EntityType, int>();
     //   private Dictionary<EntityType, bool> mayStockpileItem = new Dictionary<EntityType,bool>();


        private bool limitsAreDirty = true;

        /// <summary>
        /// a cached subset of the above, used by haulingmanager for item limits below infinite
        /// </summary>
        private Dictionary<EntityType, int> itemsWithLimits = new Dictionary<EntityType, int>();
    
        /// <summary>
        /// computed from the other two dicts.
        /// </summary>
        private Dictionary<EntityCategory, List<EntityType>> categoryItemSettings = new Dictionary<EntityCategory, List<EntityType>>();


       

        /// <summary>
        /// never change these. But we need to merge with them when the user makes changes to the settings...
        /// </summary>
        DefaultStorageSettings defaultSettings;

        private bool settingsAreDirty = true; // false;

        public Stockpile()
        {
        }

        public Stockpile(Stockpile.TypesOfStockpiles typeOfStockpile, DefaultStorageSettings defaultSettings)
        {
            this.stockpileType = typeOfStockpile;

            Init(defaultSettings);
        }

        public Stockpile(Stockpile.TypesOfStockpiles typeOfStockpile,
           DefaultStorageSettings defaultSettings,
           Dictionary<EntityCategory, bool> mayStockpileCategory,
           Dictionary<EntityType, int> mayStockpileItem) //Dictionary<EntityType, bool> mayStockpileItem) 
        {
            this.stockpileType = typeOfStockpile;
            this.mayStockpileCategory = mayStockpileCategory;
            this.mayStockpileItem = mayStockpileItem;

            Init(defaultSettings);
           
        }

        private void Init(DefaultStorageSettings defaultSettings)
        {
            this.defaultSettings = defaultSettings;

            // create a copy of the settings:
         /*   if (defaultSettings != null)
            {
                if (defaultSettings.MayStockpileItemFinal != null)
                {
                    mayStockpileItem = new Dictionary<EntityType, bool>(defaultSettings.MayStockpileItemFinal);
                }

                if (defaultSettings.MayStockpileCategoryFinal != null)
                {
                    mayStockpileCategory = new Dictionary<EntityCategory, bool>(defaultSettings.MayStockpileCategoryFinal);
                }

                settingsAreDirty = true; // recompute when shown in GUI
            }*/

            // now merge with defaults to keep 'future categories/items': 
            MergeWithDefaultSettings();

            settingsAreDirty = true; // NEW: recompute when shown in GUI

            limitsAreDirty = true;
        }

      
        public Dictionary<EntityType, int> GetMaxLimits()
        {
            if (limitsAreDirty)
            {
                itemsWithLimits.Clear();
                foreach (var item in mayStockpileItem)
                {
                    if (!HasNoLimit(item.Value))
                    {
                        itemsWithLimits.Add(item.Key, item.Value);
                    }
                }

                limitsAreDirty = false;
            }

            return itemsWithLimits;
        }

        
        public static bool HasNoLimit(int limit)
        {
            return limit == Sim.HasNoLimitValue; // GameData.Instance.Constants.UnlimitedStockpileValue; // HasNoLimitValue;
        }
       

        /// <summary>
        /// returns true if there is an overriding item setting in this category
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
      /*  public bool CategoryHasItemSetting(EntityCategory category)
        {
            if (settingsAreDirty)
            {
                RecomputeCategoryHasItemSettings();
            }

            bool value;
            if (categoryItemSettings.TryGetValue(category, out value))
            {
                return true;
            }

            return false;
        }*/

        public bool CategoryHasDifferentItemSetting(bool categorySetting, EntityCategory category, List<EntityType> exclusionList)
        {
            
            if (settingsAreDirty)
            {
                RecomputeCategoryHasItemSettings();
            }

            List<EntityType> list;
            if (categoryItemSettings.TryGetValue(category, out list))
            {
                return list.Exists(e => (exclusionList == null || !exclusionList.Contains(e)) 
                                            && ItemSettingDiffers(mayStockpileItem, e, categorySetting));
            }

            return false;
        }

       // public static bool ItemSettingDiffers( Dictionary<EntityType, bool> mayStockpileItem, EntityType entityType, bool setting)
        public static bool ItemSettingDiffers(Dictionary<EntityType, int> mayStockpileItem, EntityType entityType, bool categorySetting)
        {
            int value;
            if (mayStockpileItem.TryGetValue(entityType, out value))
            {
                if (categorySetting == true)
                {
                    return value == 0;
                }
                else
                {
                    return value != 0;
                }
            }

            return false;
        }

        /// <summary>
        /// compute a mapping from category to item types that have a setting (mayStockpileItem)
        /// </summary>
        private void RecomputeCategoryHasItemSettings()
        {
            categoryItemSettings.Clear();

            foreach (var item in mayStockpileItem)
	        {
                Common.AddToMultiList(categoryItemSettings, item.Key.Category, item.Key);     
	        }

            settingsAreDirty = false;

        }

        /// <summary>
        /// default is data-driven...
        /// </summary>
        /// <param name="typeOfStockpile"></param>
        /// <returns></returns>
        public static bool GetAllowBaseSetting(TypesOfStockpiles typeOfStockpile, out int limit)
        {
            if (typeOfStockpile == TypesOfStockpiles.Normal)
            {
                limit = Sim.HasNoLimitValue; // GameData.Instance.Constants.UnlimitedStockpileValue; // HasNoLimitValue;
                return true; // if category has not been set, and item neither, then default is Allow:
            }
            else
            {
                limit = 0;
                return false; // default is don't trade
            }
        }

        public bool GetAllowBaseSetting(out int limit)
        {
            return GetAllowBaseSetting(stockpileType, out limit);
        }

        /// <summary>
        /// item setting will override category setting
        /// </summary>
        public bool MayStockpile(EntityType entityType, out int limit)
        {        
           
            int itemSetting;
            if (mayStockpileItem.TryGetValue(entityType, out itemSetting))
            {
                limit = itemSetting;
                return itemSetting != 0;
            }
            else
            {
                bool categorySetting;
                if (mayStockpileCategory.TryGetValue(entityType.Category, out categorySetting))
                {
                    limit = Sim.HasNoLimitValue; // GameData.Instance.Constants.UnlimitedStockpileValue;  
                    return categorySetting;
                }
                else
                {
                    //return GetAllowBaseSetting();   

                    return GetAllowBaseSetting(out limit);                   
                }
            }

        }

        /*
        public void AddCategory(EntityCategory category, bool setting)
        {
            mayStockpileCategory.Add(category, setting);

            settingsAreDirty = true;
        }

        public void AddItem(EntityType item, bool setting)
        {
            mayStockpileItem.Add(item, setting);

            settingsAreDirty = true;
        }
        */


        public bool TryGetCategory(EntityCategory entityCategory, out bool categorySetting)
        {
            return mayStockpileCategory.TryGetValue(entityCategory, out categorySetting);
        }

        public bool TryGetItem(EntityType entityType, out int setting)
        {
            return mayStockpileItem.TryGetValue(entityType, out setting);
        }

      
        /*
        public void Clear()
        {
            mayStockpileCategory.Clear();
            mayStockpileItem.Clear();
            categoryItemSettings.Clear();

            settingsAreDirty = true;
        }*/

        /// <summary>
        /// copy over any settings that are in the default collection, but not in the stockpile collection
        /// </summary>
        private void MergeWithDefaultSettings()
        {
            if (defaultSettings != null)
            {
                if (defaultSettings.MayStockpileCategoryFinal != null)
                {
                    foreach (var item in defaultSettings.MayStockpileCategoryFinal)
                    {
                        if (!mayStockpileCategory.ContainsKey(item.Key))
                        {
                            mayStockpileCategory.Add(item.Key, item.Value);
                        }
                    }
                }

                if (defaultSettings.MayStockpileItemFinal != null)
                {
                    foreach (var item in defaultSettings.MayStockpileItemFinal)
                    {
                        if (!mayStockpileItem.ContainsKey(item.Key))
                        {
                            mayStockpileItem.Add(item.Key, item.Value);
                        }
                    }
                }
            }
        }

        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            this.categoryItemSettings = sn.DoMultiMap(categoryItemSettings);
            this.mayStockpileCategory = sn.DoDictionary(mayStockpileCategory);
            this.mayStockpileItem = sn.DoDictionary(mayStockpileItem);
            this.settingsAreDirty = sn.DoBool(settingsAreDirty);
            this.defaultSettings = sn.DoGameData(defaultSettings);
            this.stockpileType = sn.DoEnum(stockpileType);

            sn.Ignore(limitsAreDirty);
            sn.Ignore(itemsWithLimits);

            return this;
        }

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public Snapshotter.Version DoVersion(Snapshotter sn)
        {
            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }

        public bool IsSnapshotted { get; set; }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

           
        }

        #endregion
    }
}
