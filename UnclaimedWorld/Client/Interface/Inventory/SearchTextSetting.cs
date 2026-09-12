using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Items;
using UWGame.SimSide.Snapshots;

namespace UWGame.ClientSide.Interface.Inventory
{
    /// <summary>
    /// caches the results of text search in the entity type collection
    /// </summary>
    public class SearchTextFilterSetting : ISnapshot
    {

       // public FilterSettingType FilterSettingType;

        public string SearchText;


        // cache the results:
        HashSet<EntityType> Results;


        public SearchTextFilterSetting()
        {

        }


        public SearchTextFilterSetting(string text)
        {
            this.SearchText = text;
        }

       

        public HashSet<EntityType> GetData(Predicate<EntityType> filter)
        {
            if (this.Results == null)
            {
                Results = GetEntityTypesMatchingString(filter);
            }
            return Results;
        }


        private HashSet<EntityType> GetEntityTypesMatchingString(Predicate<EntityType> filter)
        {
            HashSet<EntityType> set = new HashSet<EntityType>();

            string searchTerm = SearchText.ToLower();
            foreach (var item in GameData.Instance.AllEntityTypes)
            {
                if ((filter == null || filter(item.Value))
                    && item.Value.Name.ToLower().Contains(searchTerm))
                {
                    set.Add(item.Value);
                }
            }

            return set;
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
            SearchText = sn.DoString(SearchText);


            sn.Ignore(Results);

            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);           

        }
        #endregion
    }
}
