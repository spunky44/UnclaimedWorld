using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Maps;

namespace UWGame.SimSide.Commands
{
    public class SetTaskPriority : Control.Commands.Command
    {
        public long jobID;
        // uint jobID;
        public Priority jobPriority;

        public SetTaskPriority()
        {
        }
        public SetTaskPriority(JobID jobID, Priority jobPriority)
        {
            this.jobID = (long)jobID;
            this.jobPriority = jobPriority;

        }



        public override void Execute(bool giveClientFeedback)
        {
            Job job = LookUp<Job, JobID>.FindByID((JobID)jobID);


            if (job != null)
            {
                job.Priority = jobPriority;
                ProcessJob pJob = job as ProcessJob;

                //Harvest jobs are grouped together so we need to update them as a group.
                if (pJob != null
                    && pJob.HarvestJob != null)
                {
                    Zone zone = pJob.HarvestJob.Zone;
                    if (zone != null && zone.HasHarvestJobs()) // needed?
                    {
                        // get jobs of the same type, (in the same zone only)?
                        List<ProcessJob> list;
                        if (pJob.HarvestJob.Zone.HarvestJobs.TryGetValue(pJob.HarvestJob.ResourceType, out list))
                        {
                            foreach (var otherProcessJob in list)
                            {
                                otherProcessJob.Priority = jobPriority;
                            }
                        }
                    }
                }
            }
        }

        /*
        public override void Execute(bool giveClientFeedback)
        {
            Job job = LookUp<Job, JobID>.FindByID((JobID)jobID);


            if (job != null)
            {
                job.Priority = jobPriority;
                ProcessJob pJob = job as ProcessJob;

                //Harvest jobs are grouped together so we need to update them as a group.
                if (pJob != null
                    && pJob.HarvestJob != null)
                {
                    Zone zone = pJob.HarvestJob.Zone;
                    if (zone != null && zone.HasHarvestJobs()) // needed?
                    {
                        foreach (var item in pJob.HarvestJob.Zone.HarvestJobs) // Lars: changed it to iterate Zone's list of jobs instead of the 'private' list...
                        {
                            foreach (var otherProcessJob in item.Value)
                            {
                                otherProcessJob.Priority = jobPriority;
                            }
                        }
                    }
                }
            }

        }*/
    }
}
