using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities.RepairTypes
{
    public class RepairPackage: ISnapshot
    {
        /// <summary>
        /// all the actions needed to repair a root entity and its parts
        /// </summary>
        public List<RepairPackageAction> Actions;


        public RepairPackage()
        {

        }

        /// <summary>
        /// copy ctor
        /// </summary>
        /// <param name="original"></param>
        public RepairPackage(RepairPackage original)
        {            
            Actions = new List<RepairPackageAction>();
            foreach (var action in original.Actions)
            {
                Actions.Add(new RepairPackageAction(action));
            }           
        }

        /// <summary>
        /// return true if the part is already handled by the other actions
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        public bool IncludesPartRepair(Entity part)
        {
            return false; 
        }

        public bool MatchesJob(ProcessJob repairJob)
        {
            return Actions.Any(p => p.RepairAction == repairJob.RepairJob.RepairActionToUse
                && ((p.Part == null && repairJob.RepairJob.PartToFix == null)
                || (p.Part != null && repairJob.RepairJob.PartToFix.HasValue && p.Part.ID == repairJob.RepairJob.PartToFix.Value.Entity)));
            
        }


        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            Actions = sn.DoList(Actions);


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

            if (Actions != null)
            {
                foreach (var item in Actions)
                {
                    item.LoadPostProcess(sn);
                }
            }

        }

        public bool IsSnapshotted
        {
            get;
            set;
        }


        #endregion
    }

   
}
