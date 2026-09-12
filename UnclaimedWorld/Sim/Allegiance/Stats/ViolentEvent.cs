using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Allegiances.Statistics
{
    public enum ViolentEventType { Death, Injury };

   
    public class ViolentEvent : ISnapshot
    {
        public ViolentEventType EventType;
        public EntityID Victim;
        public string Name;
        public string Description;

        public DateAndTime.TimeDateYear Time;



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
            this.EventType = sn.DoEnum(EventType);
            this.Victim = sn.DoEnum(Victim);
            this.Name = sn.DoString(Name);
            this.Description = sn.DoString(Description);
            this.Time = sn.DoTimeDateYear(Time);

            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);



        }

        #endregion
    }
}
