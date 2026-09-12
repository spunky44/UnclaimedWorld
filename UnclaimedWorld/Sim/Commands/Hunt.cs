using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.AI;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Commands
{
    public class Hunt : Control.Commands.Command
    {
        public long EntityID;

        /// <summary>
        /// for shared knowledge lookup
        /// </summary>
        public long AllegianceID;

        public bool GiveClientFeedback;

        /// <summary>
        /// the owner of the job
        /// </summary>
        public long EntityGroup;


        public Hunt()
        { 
        }
        public Hunt(EntityID entityID, AllegianceID allegianceID, EntityGroupID ownerOfJobID, bool giveClientFeedback)
        {
            this.EntityID = (long)entityID;
            this.EntityGroup = (long)ownerOfJobID;
            this.AllegianceID = (long)allegianceID;
            this.GiveClientFeedback = giveClientFeedback;
        }

        public override void Execute(bool giveClientFeedback)
        {
            bool huntSuccesful = HuntCreature();

            if (giveClientFeedback && GiveClientFeedback)
            {
                if (huntSuccesful)
                {
                    The.Client.OnHuntCreature();
                }
            }   
        }


        private bool HuntCreature()
        {
            Allegiance allegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)AllegianceID);


            IKnownEntityData creatureToHunt;
            if (!GoalEvaluator.EntityDataResultCausesSkip(allegiance.SharedKnowledge.GetKnownData((EntityID)EntityID, out creatureToHunt)))
            {

                EntityGroup entityGroup = LookUp<EntityGroup, EntityGroupID>.FindByID((EntityGroupID)EntityGroup);
                               
               // List<Job> jobs = JobManager.GetProductionJobs(entityGroup, creatureToHunt.EntityType);
               
                if (!entityGroup.OtherJobs.Exists(j => j is HuntingJob //  !jobs.Exists(j => j is HuntingJob
                    && ((HuntingJob)j).Target == creatureToHunt.EntityID)) // see that this entity was not added already
                {
                    HuntingJob job = new HuntingJob(creatureToHunt.EntityID, creatureToHunt.EntityType, entityGroup);
                    return true;

                }
                //}
            }

            return false;
        }
    }
}
