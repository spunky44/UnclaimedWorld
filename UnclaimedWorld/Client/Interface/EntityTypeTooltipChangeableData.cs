using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Snapshots;

namespace UWGame.Client.Interface
{
    /// <summary>
    /// Lars: this class is so puny... I really don't like it
    /// </summary>
    public class EntityTypeTooltipInstanceData: ISnapshot
    {
        public string Description;
        public string RaceTypeDescription;


        public EntityTypeTooltipInstanceData() { }

        public EntityTypeTooltipInstanceData(EntityTypeTooltipInstanceData instanceToCopyFrom)
        {
            this.Description = instanceToCopyFrom.Description;
            this.RaceTypeDescription = instanceToCopyFrom.RaceTypeDescription;

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
            this.Description = sn.DoString(Description);
            this.RaceTypeDescription = sn.DoString(RaceTypeDescription);
          
            return this;
        }
        

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

        }

        #endregion

    }
}
