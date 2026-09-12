using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Jobs.JobTypes;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.Commands
{
    public class SetJobTypePriority : Control.Commands.Command
    {
        //public long ExpeditionID;
        public long EntityGroupID;

        public string JobTypeKey;

        public Priority jobPriority;

        public bool UpdateJobInstances;

        public SetJobTypePriority()
        {
        }

        public SetJobTypePriority(EntityGroup entityGroup,  //Expedition expedition, 
            string jobTypeKey, Priority jobPriority, bool updateInstances)
        {
          //  this.ExpeditionID = (long)expedition.ID;
            this.EntityGroupID = (long)entityGroup.ID;
            this.jobPriority = jobPriority;
            this.JobTypeKey = jobTypeKey;
            this.UpdateJobInstances = updateInstances;
        }

        public override void Execute(bool giveClientFeedback)
        {
           // Expedition expedition = LookUp<Expedition, ExpeditionID>.FindByID((ExpeditionID)ExpeditionID);
            EntityGroup entityGroup = LookUp<EntityGroup, EntityGroupID>.FindByID((EntityGroupID)EntityGroupID);

            if (entityGroup != null)
            {
                JobType jobType = GameData.Instance.AllJobTypes[JobTypeKey];
                entityGroup.Policy.JobTypePriorities[jobType] = jobPriority; // .HaulToStoragePriority = jobPriority;  
 
                // also update all job instances:
                jobType.IterateJobs(entityGroup, j => j.Priority = jobPriority);

               
              //  entityGroup.IterateJobs(jobType, j => j.Priority = jobPriority);

            }

        }
    }
}
