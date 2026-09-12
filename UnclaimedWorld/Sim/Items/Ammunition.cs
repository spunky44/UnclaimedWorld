using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Items
{
    /// <summary>
    /// TODO: replace with Substance
    /// </summary>
    public class Ammunition: ISnapshot
    {
        public int NoOfRounds;

       
        public bool HasEnoughAmmo(int neededRounds)
        {
            return neededRounds <= NoOfRounds;
        }


     /*   public void ConsumeRounds(int rounds, EntityGroup owner)
        {
            NoOfRounds -= rounds;

            owner.SetAmmoDirty()
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
            this.NoOfRounds = sn.DoInt32(NoOfRounds);

            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);


        }

        #endregion
    }
}
