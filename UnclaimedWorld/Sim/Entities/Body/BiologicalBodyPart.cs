using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities.Body
{
    public class BiologicalBodyPart: BodyPart
    {
        public List<Wound> Wounds;

        public List<Organ> Organs;

        public BiologicalBodyPart()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
    
        }

        public BiologicalBodyPart(BodyPartType bodyPartType, Body body)
            : base(bodyPartType, body)
        {
        }

        public BiologicalBodyPart(BodyPart original)
            : base(original)
        {

        }

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public override Snapshotter.Version DoVersion(Snapshotter sn)
        {
            base.DoVersion(sn); // each class in the class hierarchy snapshots and maintains their own version.

            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }

        public override Snapshots.ISnapshot DoSnapshot(Snapshots.Snapshotter sn)
        {
            base.DoSnapshot(sn);

            sn.Postpone(Wounds);
            sn.Postpone(Organs);

            return this;
        }

        public override void LoadPostProcess(Snapshotter sn)
        {
            base.LoadPostProcess(sn);

            sn.RegisterLoadPostProcessCall(this);

        }


    }
}
