using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.ClientSide.Interface.Inventory;
using UWGame.SimSide;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;
using WindowSystem;

namespace UWGame.ClientSide.Interface.BuyAndSell
{
    /// <summary>
    /// keep 2 instances, one for buy, one for sell...
    /// </summary>
    public class BuySellPanelSettings: ISnapshot
    {
        public FilterPropertySettings FilterPropertySettings;

        public enum SortColumns { Name, Amount, Price, Bulk, OfferDemand }
        public SortingSettings<SortColumns> SortingSettings;
       

        public const SortColumns DefaultSortColumn = SortColumns.Amount;
        public const Grid.Sorting DefaultSortOrder = Grid.Sorting.Descending;

        public ViewType viewType = ViewType.List;

        public bool IsExpanded = false; 

        /// <summary>
        /// all entity types with prices
        /// </summary>
        HashSet<EntityType> baseData;


        /// <summary>
        /// changes with filters
        /// </summary>
        HashSet<EntityType> staticData;

        bool settingsAreDirty = true;


        /// <summary>
        /// callback method
        /// </summary>
     //   public Func<HashSet<EntityType>> GetBaseData;


        public BuySellPanelSettings()
        {
            if (!Snapshotter.IsSnapshotting)
            {
                FilterPropertySettings = new FilterPropertySettings();
                SortingSettings = new SortingSettings<SortColumns>(DefaultSortColumn, Grid.Sorting.Descending);
            }


        }

        /// <summary>
        /// I want this to look similar to InventorySettings
        /// </summary>
        /// <returns></returns>
        public HashSet<EntityType> GetData() //Dictionary<EntityType, InventoryPanel.Availability> allAvailableItems)
        {
            // only recompute data when the filters change
            // except if there are active filters which depend on the curent state, like for instance the items the player has
            // the data sets that don't depend on the curent state such as current inventory can be cached.            
            HashSet<EntityType> listOfFiltered;

            if (FilterPropertySettings.SettingsAreDirty)
            {                              
                listOfFiltered = GetFilteredEntities();


                FilterPropertySettings.SetSettingsNotDirty();
                

                staticData = listOfFiltered;
            }         


            return staticData;

            //return listOfFiltered;
        }


        /// <summary>
        /// exclude structures etc.
        /// </summary>
        /// <returns></returns>
        public static bool FilterTradeItems(EntityType entityType)
        {
            if (entityType.KeyName.Contains("hauling"))
            {

            }

            if (entityType.TerrainType == null
                        && entityType.TreeType == null
                     //   && entityType.BiologicalType == null // dogs too?
                        && entityType.StructureType == null
                        && entityType.Category != null
                      /*  && entityType.ItemType != null*/)
            {
                return true;
            }

            return false;

        }

        public static HashSet<EntityType> GetBaseData(ref HashSet<EntityType> cachedBaseData)
        {
            if (cachedBaseData == null)
            {
                cachedBaseData = new HashSet<EntityType>(); // new Dictionary<string, EntityType>();
                foreach (var item in GameData.Instance.AllEntityTypes)
                {
                    // only show production related types?
                    if (FilterTradeItems(item.Value))
                    {
                        cachedBaseData.Add(item.Value);
                    }
                }

            }

            return cachedBaseData;
        }

        private HashSet<EntityType> GetFilteredEntities()
        {

            HashSet<EntityType> listOfFiltered = new HashSet<EntityType>();

            //get the entity-collection that fit for the filter(s)
            if (FilterPropertySettings.HasActiveFilters())
            {
                staticData = new HashSet<EntityType>(); 

                List<HashSet<EntityType>> results = FilterPropertySettings.GetFilteredEntities(FilterTradeItems);


                //create one dictionary from the result dictionary list 
                //if OR we union them if AND we intersect them
                if (results.Count > 0)
                {
                    /*if (AndOrSetting == AndOr.Or)
                    {*/
                        listOfFiltered = results.Aggregate((previousList, nextList) => previousList.Union(nextList).ToHashSet());
                   /* }
                    else
                    {
                        listOfFiltered = results.Aggregate((previousList, nextList) => previousList.Intersect(nextList).ToHashSet());
                    }*/
                }
            }
            else
            {
                listOfFiltered = GetBaseData(ref baseData);
            }

            return listOfFiltered;
        }


       /* private HashSet<EntityType> GetBaseData()
        {
            if (baseData == null)
            {
                baseData = new HashSet<EntityType>(); // new Dictionary<string, EntityType>();
                foreach (var item in GameData.Instance.AllEntityTypes)
                {
                    // only show production related types?
                    if (item.Value.TerrainType == null
                        && item.Value.TreeType == null
                        && item.Value.BiologicalType == null)
                    {
                        baseData.Add(item.Value);
                    }
                }

            }

            return baseData;
        }*/


        #region ISnapshot

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

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
          
            this.IsExpanded = sn.DoBool(IsExpanded);

            this.SortingSettings = (SortingSettings<SortColumns>)sn.DoISnapshot(SortingSettings);

            this.FilterPropertySettings = (FilterPropertySettings)sn.DoISnapshot(FilterPropertySettings);

            this.viewType = sn.DoEnum(viewType); 

            settingsAreDirty = sn.DoBool(settingsAreDirty);
            staticData = sn.DoHashSet(staticData);


            sn.Ignore(baseData);

            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            SortingSettings.LoadPostProcess(sn);

            FilterPropertySettings.LoadPostProcess(sn);

        }
        #endregion
    }
}
