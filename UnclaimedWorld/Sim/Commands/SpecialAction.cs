using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.AI;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Processes;

namespace UWGame.SimSide.Commands
{
    public class SpecialAction : Control.Commands.Command
    {
        /// <summary>
        /// the owner of the Job
        /// </summary>
        public long EntityGroup;

        /// <summary>
        /// the entity to perform the action on - can be unowned.
        /// </summary>
        public long EntityID;

        /// <summary>
        /// for SharedKnowledge lookup of entity
        /// </summary>
        public long AllegianceID;

        public string ProcessType;

        public bool GiveClientFeedback;

        public Priority? Priority;

        public SpecialAction()
        { 
        }

        public SpecialAction(EntityID entityID, AllegianceID allegianceID, EntityGroupID entityGroupID, bool giveClientFeedback, string processTypeKey) //, Priority priority = Priority.Normal)
        {
            this.EntityID = (long)entityID;
            this.EntityGroup = (long)entityGroupID;
            this.AllegianceID = (long)allegianceID;
            this.GiveClientFeedback = giveClientFeedback;
            this.ProcessType = processTypeKey;
           // this.Priority = priority;
        }

        public override void Execute(bool giveClientFeedback)
        {
            
            bool success = DoSpecialAction();
            

            if (giveClientFeedback && GiveClientFeedback)
            {
                if (success)
                {
                    The.Client.OnSpecialAction();
                }
            }   
        }

        private bool DoSpecialAction()
        {
            Allegiance allegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)AllegianceID);

            IKnownEntityData entityData;
            allegiance.SharedKnowledge.GetKnownData((EntityID)EntityID, out entityData);

            if (entityData != null)
            {
                Processes.ProcessType processType = GameData.Instance.AllProcessTypes[ProcessType];

                // why is this needed???
               // Entity.EnableSpecialAction(entityData, processType.OriginalProcess);// make sure it is enabled, use the shared process here

                EntityGroup entityGroup = LookUp<EntityGroup, EntityGroupID>.FindByID((EntityGroupID)EntityGroup);

                ProcessJob pJob = JobManager.CreateSpecialActionJob(entityGroup, entityData, processType);

                if (Priority.HasValue)
                {
                    pJob.Priority = Priority.Value;
                }

                // hide/disable alternative action:
              /*  if (processType.UsesAnchor())
                {
                    // this is a sim action...
                    // disable special action(s) on the anchor to prevent re-clicking:
                    // this should be repeated when the process finishes to account for event spawns
                    
                    // still necessary? the GUI will check for existing jobs and disable the button.
                   // Entity.DisableSpecialActionsUsingAnchor(entityData);
                }*/

                return true;
            }
            else return false;
        }

        


        public static bool ActionJobExists(IKnownEntityData entity, ProcessType processType, List<Job> jobs)
        {
            // what about agents??
            //  List<Job> jobs = entity.Owner.InternalOwner.OwnerContent.Jobs; // JobManager.GetProductionJobs(expedition.ExpeditionOwner, intface.SelectedEntity.EntityType);
            if (jobs.Exists(j => JobExistsForEntity(j, entity, processType))) // see that this entity was not added already
            {
                return true;
            }

            return false;
        }

        private static bool JobExistsForEntity(Job j, IKnownEntityData entity, ProcessType processType)
        {
            ProcessJob p = j as ProcessJob;
            if (p != null)
            {
                if (p.ProcessType == processType)
                {
                    EntityID? actingOn;
                    if (p.GetActingOnEntity(out actingOn))
                    {
                        return actingOn == entity.EntityID;
                    }
                    /*
                    if (p.ActingOnEntity == entity.EntityID)
                    {
                        return true;
                    }*/
                }
            }

            return false;
        }
    }
}
