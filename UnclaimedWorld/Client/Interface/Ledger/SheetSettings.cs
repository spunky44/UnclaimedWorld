using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWGame.SimSide.Snapshots;

namespace UWGame.ClientSide.Interface.Ledger
{
    public class SheetSettings<T> : ISnapshot where T : struct,  IComparable, IFormattable, IConvertible // the closest constraint to enum we can get..
    {               

        public SortingSettings<T> SortingSettings;




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
            this.SortingSettings = (SortingSettings<T>)sn.DoISnapshot(SortingSettings);


            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            SortingSettings.LoadPostProcess(sn);


        }
        #endregion
    }
}
