using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Scenarios;
using System.Reflection;
using GameStateManagement;

namespace UWGame.SimSide.Snapshots
{
    /// <summary>
    /// here we place all the save game data that needs to be accessible without doing a full load.
    /// 
    /// It should not be necessary to duplicate the data here in the save file body.
    /// </summary>
    public class SnapshotHeader: ISnapshot
    {
        /// <summary>
        /// The params used in creating the game originally.
        /// Options are included, since we may still need them for scoring etc.
        /// </summary> 
        public StartGameParams StartGameParams;

       
        public System.Version ProgramVersion;

        /// <summary>
        /// a number marking the game/campaign that the save belongs to. May be useful for sorting save games
        /// We need to save a counter in user options to handle this...
        /// </summary>
     //   public int? Campaign;

        /// <summary>
        /// the local time and date when this save game was created
        /// </summary>
        public DateTime Timestamp;



        public SnapshotHeader(StartGameParams startGameParams) //string scenario, Source? source, SerializableDictionary<string, Option> options /*, int? campaign*/)
        {
            this.StartGameParams = startGameParams;

                        
            ProgramVersion = UnclaimedWorld.GetVersion();            
            Timestamp = DateTime.Now;

        }

        public SnapshotHeader(){}


        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            this.StartGameParams = (StartGameParams)sn.DoISnapshot(StartGameParams);

            string versionAsString = null;
            if (sn.mode != Snapshotter.Mode.Load)
            {
                versionAsString = ProgramVersion.ToString();
            }

            this.ProgramVersion = new Version(sn.DoString(versionAsString)); // new System.Version(DoStringStatic(mode, writer, reader, null));
            this.Timestamp = sn.DoDateTime(Timestamp); // new DateTime(reader.ReadInt64());


            sn.Ignore(ProgramVersion); // serialized as a string

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

            StartGameParams.LoadPostProcess(sn);
        }


        #endregion

    }
}
