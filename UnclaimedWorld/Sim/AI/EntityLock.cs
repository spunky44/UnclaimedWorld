using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI
{
    /// <summary>
    /// contains locks that an allegiance can assign to entity/memory facts
    /// </summary>
    public class EntityLock: ISnapshot
    {
        /// <summary>
        /// 
        /// the purpose of InUseBy is to enable agents to place locks on items not used in jobs. Such as GoalEat, or weapons/equipment being used in GoalEat and other non-job activities.
        /// 
        /// InuseBy will be set when the agent targets the item for pickup, and cleared when he is done with it (but he may still carry it)
        /// 
        /// 
        /// For simplicity reasons, the lock is also set together with AssignedToJob. 
        /// 
        /// For process tools in unattended processes, the lock is cleared when the agent leaves the process. Then AssignedToJob is the only lock left.
        /// 
        /// </summary>
        public EntityID? InUseBy;

     //   public JobID? AssignedToJob;


        /// <summary>
        /// to allow cleanup
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty()
        {
            return InUseBy == null;
        }


        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            InUseBy = sn.DoEnumNullable(InUseBy);


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
