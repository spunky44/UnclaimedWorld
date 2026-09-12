using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Goals
{
    public class ReplenishItemsForAction: ISnapshot
    {
        /// <summary>
        /// the type of replenish action
        /// </summary>
        public ProcessType Action;

        /// <summary>
        /// the list of items to replenish with
        /// </summary>
        public List<EntityID> Items;


        public ReplenishItemsForAction(ProcessType action, List<EntityID> items)
        {
            this.Action = action;
            this.Items = items;
        }

        public ReplenishItemsForAction()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");     
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
          //  this.Action = (GoalReplenish.ReplenishAction)sn.DoEnum(Action);
            this.Action = sn.DoGameData(Action);
            this.Items = sn.DoList(Items);

            return this;
        }


        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

        }

        #endregion
    }
}
