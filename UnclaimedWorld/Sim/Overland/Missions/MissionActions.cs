using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Snapshots;
using Microsoft.Xna.Framework;

namespace UWGame.SimSide.Overland.Missions
{
  /*  public class MissionActions: ISnapshot
    {

        public Mission Mission;
        MissionID snapshotMission;

       // public List<MissionAction> Actions;
        public Queue<MissionAction> Actions;

       // public Dictionary<TravelLocation, List<MissionAction>> locationActions;

      //  MissionAction currentAction;

        public MissionActions(Mission parent)
        {
            this.Mission = parent;


        }

        public MissionActions()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");      
        }


    

        public bool Update(GameTime gameTime)
        {
            if (Actions.Count > 0)
            {
                bool isCompleted = Actions.Peek().Update(gameTime);
                if (isCompleted)
                {
                    Actions.Dequeue();

                    if (Actions.Count == 0)
                        return true; // finished
                }
            }

            return false;
        }


        


        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            Actions = sn.DoQueue(Actions);
            snapshotMission = (MissionID)sn.SnapshotID<Mission, MissionID>(Mission);
           

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

            Mission = LookUp<Mission, MissionID>.FindByID(snapshotMission);

            foreach (var item in Actions)
            {
                item.LoadPostProcess(sn);
            }

        }

        #endregion
    }*/
}
