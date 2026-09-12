using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Items;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Commands
{
    /// <summary>
    /// updates/changes an existing attack zone
    /// </summary>
    public class AttackAreaUpdateJob : Control.Commands.Command
    {
        public long jobID;
        

        public bool GiveClientFeedback;
        
        /*

        public bool AttackVermin;
        public bool AttackThreats;*/

        public int NoOfAttackers;

        public AttackAreaUpdateJob()
        {
        }


        public AttackAreaUpdateJob(JobID jobID, bool giveClientFeedback, 
            //EntityGroupID entityGroupID, bool attackVermin, bool attackThreats, 
            int noOfPatrollers) 
        {
            this.jobID = (long)jobID;
            this.GiveClientFeedback = giveClientFeedback;
          /*  this.EntityGroupID = (long)entityGroupID;

            this.AttackVermin = attackVermin;
            this.AttackThreats = attackThreats;*/

            this.NoOfAttackers = noOfPatrollers;
        }

       
        public override void Execute(bool giveClientFeedback)
        {
            Zone zone;
            bool success = DoUpdateAttackArea(out zone);


            if (giveClientFeedback && GiveClientFeedback)
            {
                if (success)
                {
                    The.Client.OnPatrolOrAttackArea(zone);
                }
            }
        }


        private bool DoUpdateAttackArea(out Zone zone)
        {
            zone = null;

            Job job = LookUp<Job, JobID>.FindByID((JobID)jobID);

            if (job != null)
            {
                AttackAreaJob attackJob = job as AttackAreaJob;
                attackJob.SetJobPositions(NoOfAttackers);

                zone = attackJob.Zone;

                return true;
            }

            return false;
        }
    }
}
