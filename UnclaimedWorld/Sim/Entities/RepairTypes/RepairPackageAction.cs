using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities.RepairTypes
{
    public class RepairPackageAction : ISnapshot
    {
        public RepairAction RepairAction;

        public Entity Part;
        EntityID? snapshotPart;

        public ProcessType RepairProcess;


        public RepairPackageAction() { }

        /// <summary>
        /// copy ctor
        /// </summary>
        /// <param name="original"></param>
        public RepairPackageAction(RepairPackageAction original) 
        {
            RepairAction = original.RepairAction;
            Part = original.Part;
            RepairProcess = original.RepairProcess;
        }


        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            RepairAction = sn.DoEnum(RepairAction);
            RepairProcess = sn.DoGameData(RepairProcess);

            snapshotPart = sn.SnapshotID<Entity, EntityID>(Part);

            return this;
        }

        Snapshotter.Version version;
        public Snapshotter.Version DoVersion(Snapshotter sn)
        {
            version = sn.DoVersion(Snapshotter.Version.Original);
            return version;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

        }

        public bool IsSnapshotted
        {
            get;
            set;
        }


        #endregion
    }
}
