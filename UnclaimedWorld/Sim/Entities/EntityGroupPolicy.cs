using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Jobs.JobTypes;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities
{
    /// <summary>
    /// contains job related policies
    /// 
    /// Household, Expedition, Person, (SharedKnowlege) all have an EntityGroup
    /// </summary>
    public class EntityGroupPolicy: ISnapshot
    {

        /// <summary>
        /// get applied to all new jobs
        /// </summary>
        public Dictionary<JobType, Priority> JobTypePriorities = new Dictionary<JobType, Priority>();



        public Priority GetPriority(JobType jobType)
        {
            Priority priority;
            if (!JobTypePriorities.TryGetValue(jobType, out priority))
            {
                priority = Priority.Normal;
            }

            return priority;
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
            // HaulToStoragePriority = sn.DoEnum(HaulToStoragePriority);

            JobTypePriorities = sn.DoDictionary(JobTypePriorities);

            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            /* if (JobTypePriorities != null)
             {
                 foreach (var item in JobTypePriorities)
                 {
                     item.Key.loadpo    
                 }
             }*/

        }

        #endregion
    }
}
