using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;

namespace UWGame.ClientSide.Interface.Inventory
{
    public class FilterPropertySettings : ISnapshot
    {

        public Dictionary<string, FilterSetting> ActiveFilterSettings = new Dictionary<string, FilterSetting>();
        List<FilterSettingType> snapshotActiveFilterSettings;

        /// <summary>
        /// contains a cached result
        /// </summary>
        SearchTextFilterSetting SearchTextFilter;
       // public string SearchText;

        public bool SearchTextActive;

        bool settingsAreDirty = true;


        public bool SettingsAreDirty
        {
            get
            {
                return settingsAreDirty;
            }
        }

        public void SetSettingsNotDirty()
        {
            settingsAreDirty = false;
        }

        public bool AddFilterSetting(FilterSetting filter)
        {
            // FilterSetting filter;
            if (!ActiveFilterSettings.ContainsKey(filter.FilterSettingType.KeyName))
            {
                ActiveFilterSettings.Add(filter.FilterSettingType.KeyName, filter);

                settingsAreDirty = true;

                return true;

            }

            return false;
        }

        public void EnableSearchText(string text)
        {
            if (text == "")
            {
                SearchTextFilter = null;
                settingsAreDirty = true;
                //SearchText = null;
            }
            else
            {
                if (SearchTextFilter == null || SearchTextFilter.SearchText != text)
                {
                    SearchTextFilter = new SearchTextFilterSetting(text);
                    settingsAreDirty = true;
                }
                //SearchText = text;
            }

            if (SearchTextActive == false)
            {
                settingsAreDirty = true;
                SearchTextActive = true;
            }
        }

        public string GetSearchText()
        {
            if (SearchTextFilter != null)
            {
                return SearchTextFilter.SearchText;
            }

            return "";
        }

        public bool DisableSearchText()
        {
            if (SearchTextActive)
            {
                SearchTextActive = false;

                settingsAreDirty = true;
            }

            return true;
        }

        public bool RemoveFilterSetting(FilterSetting filter)
        {
            if (ActiveFilterSettings.Remove(filter.FilterSettingType.KeyName))
            {
                settingsAreDirty = true;

                return true;
            }

            return false;
        }

        public void RemoveAllFilterSettings()
        {
            if (ActiveFilterSettings.Count > 0)
            {
                ActiveFilterSettings.Clear();

                settingsAreDirty = true;
            }

            if (SearchTextFilter != null || SearchTextActive)
            {
                SearchTextFilter = null;
                SearchTextActive = false;

                settingsAreDirty = true;
            }

        }

        public bool HasActiveFilters()
        {
            if (ActiveFilterSettings.Count > 0 || SearchTextActive)
            {
                return true;
            }

            return false;
        }

        public List<HashSet<EntityType>> GetFilteredEntities(Predicate<EntityType> filter)
        {
            List<HashSet<EntityType>> results = new List<HashSet<EntityType>>();
            if (ActiveFilterSettings.Count > 0)
            {                
                // add results from other filters, make a method for each.
                foreach (var item in ActiveFilterSettings)
                {
                    results.Add(item.Value.GetData(filter));
                }

            }

            if (SearchTextActive && SearchTextFilter != null)
            {
                results.Add(SearchTextFilter.GetData(filter));
            }

            return results;

        }

       
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
           
            if (sn.mode != Snapshotter.Mode.Load)
            {
                snapshotActiveFilterSettings = ActiveFilterSettings.Select(k => k.Value.FilterSettingType).ToList();
            }

            snapshotActiveFilterSettings = sn.DoList(snapshotActiveFilterSettings);

            SearchTextFilter = (SearchTextFilterSetting)sn.DoISnapshot(SearchTextFilter);
            SearchTextActive = sn.DoBool(SearchTextActive);

            settingsAreDirty = sn.DoBool(settingsAreDirty);

            sn.Ignore(ActiveFilterSettings);

            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            foreach (var item in snapshotActiveFilterSettings)
            {
                ActiveFilterSettings.Add(item.KeyName, new FilterSetting(item));
            }

            if (SearchTextFilter != null)
            {
                SearchTextFilter.LoadPostProcess(sn);
            }

        }
        #endregion
    }
}
