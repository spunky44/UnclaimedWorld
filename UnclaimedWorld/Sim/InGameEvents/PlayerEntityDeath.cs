using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Entities.Skills;

namespace UWGame.SimSide.InGameEvents
{
    public class PlayerEntityDeath : ISnapshot
    {
        public string EntityName;

        public EntityID Corpse;

        public CauseOfDeath? CauseOfDeath;

        public ProfessionType Profession;

        public string DisplayImageName;

        /// <summary>
        /// hmmmm.... seems hacky to store this string
        /// </summary>
        public string HisHerIts;


        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            EntityName = sn.DoString(EntityName);
            Corpse = sn.DoEnum(Corpse);
            CauseOfDeath = sn.DoEnumNullable(CauseOfDeath);
            HisHerIts = sn.DoString(HisHerIts);
            Profession = sn.DoGameData(Profession);

            DisplayImageName = sn.DoString(DisplayImageName);

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
