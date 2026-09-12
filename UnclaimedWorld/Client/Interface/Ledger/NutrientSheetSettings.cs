using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Snapshots;

namespace UWGame.ClientSide.Interface.Ledger
{

    public class NutrientSheetSettings : SheetSettings<NutrientSheetSettings.SortColumns>
    {
        public enum SortColumns { Name, Produced, Consumed, Overconsumed, Stored, DaysLeft }

       // public SortingSettings<SortColumns> SortingSettings;



        public NutrientSheetSettings()
        {
            if (!Snapshotter.IsSnapshotting)
            {
                SortingSettings = new SortingSettings<SortColumns>(SortColumns.Name, SortingSettings<SortColumns>.DefaultSortOrder);
            }
        }        
    }
}
