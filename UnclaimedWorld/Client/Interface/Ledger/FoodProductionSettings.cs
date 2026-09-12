using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Snapshots;

namespace UWGame.ClientSide.Interface.Ledger
{

    public class FoodProductionSettings : SheetSettings<FoodProductionSettings.SortColumns> // ISnapshot
    {
        public enum SortColumns { Name, Produced, Consumed, Wasted, EatenByCreatures, Disappeared }

       // public SortingSettings<SortColumns> SortingSettings;

      

        public FoodProductionSettings()
        {
            if (!Snapshotter.IsSnapshotting)
            {
                SortingSettings = new SortingSettings<SortColumns>(SortColumns.Name, SortingSettings<SortColumns>.DefaultSortOrder);
            }
        }

        /*
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
           // this.IsExpanded = sn.DoBool(IsExpanded);
            SortingSettings = (SortingSettings<SortColumns>)sn.DoISnapshot(SortingSettings);


            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            SortingSettings.LoadPostProcess(sn);


        }
        #endregion*/
    }
}
