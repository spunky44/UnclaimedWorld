using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Snapshots;
using WindowSystem;

namespace UWGame.ClientSide.Interface
{
    public class SortingSettings<T> : ISnapshot where T : struct,  IComparable, IFormattable, IConvertible // the closest constraint to enum we can get..
    {
        public T SortedBy; 

        public Grid.Sorting SortOrder = DefaultSortOrder;

      //  private T defaultSortColumn;

      
        public const Grid.Sorting DefaultSortOrder = Grid.Sorting.Ascending;
       
        public SortingSettings()
        {

        }

       public SortingSettings(T defaultSortColumn, Grid.Sorting initialSortingOrder)
       {
           this.SortedBy = defaultSortColumn;
          // this.defaultSortColumn = defaultSortColumn;
           this.SortOrder = initialSortingOrder;

       }

       public void SetDefaultSortOrder()
       {
           SortOrder = DefaultSortOrder;
       }

      
      /*  public void SetSortedByToDefault()
        {
            SortedBy = defaultSortColumn;
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
            SortedBy = sn.DoEnum(SortedBy);
            SortOrder = sn.DoEnum(SortOrder);
         //   defaultSortColumn = sn.DoEnum(defaultSortColumn);

            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);


        }
        #endregion
    }
}
