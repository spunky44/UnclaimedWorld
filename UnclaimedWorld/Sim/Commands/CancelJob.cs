using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Commands
{
    public class CancelJob : Control.Commands.Command
    {
        public long jobID;
       // uint jobID;


        public CancelJob()
        { 
        }
        public CancelJob(JobID jobID)
        {
            this.jobID = (long)jobID;

        }

        public override void Execute(bool giveClientFeedback)
        {
            Job job = LookUp<Job, JobID>.FindByID((JobID)jobID);

            // cancel the job:
            job.Destroy(true);

            
        }
    }
}
