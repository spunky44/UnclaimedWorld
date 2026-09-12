using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities
{
    /// <summary>
    /// we don't believe non-agents should have an allegiance... but we still want abstract threats in some areas. 
    /// so entities without an allegiance can now belong to a ThreatGroup together with allegiance(s)
    /// </summary>
    public class Threat: Component
    {
        public ThreatGroup ThreatGroup;
        ThreatGroupID snapshotThreatGroup;
       

        public Threat()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }

        public Threat(Entity parent) : base(parent)
        {
          
        }
       
      
        public override ISnapshot DoSnapshot(Snapshotter sn)
        {
            base.DoSnapshot(sn);

            snapshotThreatGroup = (ThreatGroupID)sn.SnapshotID<ThreatGroup, ThreatGroupID>(this.ThreatGroup);

            return this;
        }

        public override void LoadPostProcess(Snapshotter sn)
        {
            base.LoadPostProcess(sn);

            ThreatGroup = LookUp<ThreatGroup, ThreatGroupID>.FindByID(snapshotThreatGroup);
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


    }
}
