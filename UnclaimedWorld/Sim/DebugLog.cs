using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide
{
    public class DebugLog: ISnapshot
    {
        // LOG: FOR DEBUG ONLY
        public List<Tuple<double, string>> Entries = new List<Tuple<double, string>>();



        public void Add(string text)
        {

#if DEBUG || PROFILE

            Entries.Add(new Tuple<double, string>(The.Sim.TotalUnPausedGameTimeInSeconds, text));

            while (Entries.Count > 100)
            {
                Entries.RemoveAt(0);
            }
#endif
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
            this.Entries = sn.DoList(Entries);


            return this;
        }


        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);


        }


        #endregion

    }
}
