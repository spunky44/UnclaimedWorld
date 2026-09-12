using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Items;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities
{
    public class Power: Component
    {
        public float Energy = 0.81f;


        public float PowerOutput; //??

        public string EnergyLevelToString()
        {
            return ((int)(Energy * 100)).ToString() + "/100"; //" %";
        }

        public override ISnapshot DoSnapshot(Snapshotter sn)
        {
            base.DoSnapshot(sn);
            sn.DoFloat(this.Energy);           
            sn.DoFloat(this.PowerOutput);
            return this;
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

        public Power(Entity parent): base(parent)
        {
           
        }

        public Power()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }
        
    }
}
