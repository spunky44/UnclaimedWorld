using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Overland.Missions.Templates
{
    /// <summary>
    /// store info about passengers and where they came from, so in case of a mission abort they can be brought back home...
    /// </summary>
    public class PassengerListTemplate: ISnapshot
    {
        /// <summary>
        /// their 'address'
        /// </summary>
        public TravelLocation StartingLocation;

        /// <summary>
        /// long for the xml serializer
        /// </summary>
        public List<long> Passengers;


        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            Passengers = sn.DoList(Passengers);

            StartingLocation = sn.DoTravelLocation(StartingLocation);
            //StartingLocation = (TravelLocation)sn.DoISnapshot(StartingLocation);

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
