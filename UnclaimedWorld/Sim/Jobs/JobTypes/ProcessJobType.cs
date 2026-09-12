using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.Jobs.JobTypes
{
    public class ProcessJobType: JobType
    {
       // public string DisplayName;

       // public string ProcessTypeTag;

        /// <summary>
        /// uses linear search through all jobs... we probably won't update prio that often.
        /// </summary>
        /// <param name="entityGroup"></param>
        /// <param name="iterateFunction"></param>
        public override void IterateJobs(EntityGroup entityGroup, Action<Job> iterateFunction)
        {
            foreach (var item in entityGroup.ProductionJobs)
            {
                foreach (var job in item.Value)
                {
                    if (IsType(job))
                    {
                        iterateFunction(job);
                    }
                }
            }

            foreach (var item in entityGroup.OtherJobs)
            {
                if (IsType(item))
                {
                    iterateFunction(item);
                }
            }
            
        }

        public override bool IsType(Job job)
        {
            ProcessJob processJob = job as ProcessJob;
            if (processJob != null)
            {
                if (processJob.ProcessType.JobType == this) // designer setting overrides general types
                {
                    return true;
                }               
            }

            return false;
        }


        public override string GetDefaultDisplayName()
        {
            return Name; // DisplayName;
            
        }
    }
}
