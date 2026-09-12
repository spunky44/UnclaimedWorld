using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Entities;
using UWGame.ClientSide.Interface;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Entities.Owners;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Maps;

namespace UWGame.SimSide.Commands
{
    public class Salvage : Control.Commands.Command
    {
        public long EntityID;

        /// <summary>
        /// for SharedKnowledge lookup of entity
        /// </summary>
        public long AllegianceID;

        public bool GiveClientFeedback;

        public Priority? Priority;

        public Salvage()
        { 
        }

        public Salvage(EntityID entityID, AllegianceID allegianceID, bool giveClientFeedback) //, Priority? priority = Jobs.Priority.Normal)
        {
            this.EntityID = (long)entityID;
            this.AllegianceID = (long)allegianceID;
            this.GiveClientFeedback = giveClientFeedback;
          //  this.Priority = priority;
        }

        public override void Execute(bool giveClientFeedback)
        {
            bool salvageSuccesful = DoSalvage();

            if (giveClientFeedback && GiveClientFeedback)
            {
                if (salvageSuccesful)
                {
                    The.Client.OnSalvageEntity();
                }
            }   
        }


        private bool DoSalvage()
        {
            Allegiance allegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)AllegianceID);


            IKnownEntityData entity; 

            allegiance.SharedKnowledge.GetKnownData((EntityID)EntityID, out entity);

            /* IOwner ownerOfEntity;
             LookUpOwners.ResolveEntityOwner(entity, out ownerOfEntity);
             */
            /* if (ownerOfEntity != null
                 && ownerOfEntity.Allegiance.AllegianceType == SimSide.Allegiances.AllegianceType.Player) // must be player owned
             {*/

            /*  if (!SidePanelEntity.SalvageJobExists(entity)) // see that this entity was not added already
              {*/
            // Expedition expedition = ownerOfEntity as Expedition;


            EntityGroup resolvedOwner;
            if (entity.OwnedBy == null
                || !LookUpOwners.ResolveEntityOwner(entity, out resolvedOwner))
            {
                return false;
            }

            ProcessJob pJob = CreateSalvageJob(entity, resolvedOwner);

            if (Priority.HasValue)
            {
                pJob.Priority = Priority.Value;
            }

            return true;

        }

        public static ProcessJob CreateSalvageJob(IKnownEntityData entity, EntityGroup resolvedOwner)
        {
            ProcessJob pJob = JobManager.CreateProcessJob(null, resolvedOwner, 
                entity.EntityType.NonLivingType.SalvageProcessType, null, entity.AccessPoint, isSalvage: true);

            pJob.AssignImmovableInput(entity);

            return pJob;
        }



        public static bool SalvageJobExists(IKnownEntityData entity)
        {
            IOwner ownerOfEntity;
            LookUpOwners.ResolveEntityOwner(entity, out ownerOfEntity);
            if (ownerOfEntity != null)
            {
                List<Job> jobs = ownerOfEntity.OwnedEntities.OtherJobs; // JobManager.GetProductionJobs(expedition.ExpeditionOwner, intface.SelectedEntity.EntityType);
                if (jobs.Exists(j => SalvageJobExistsForEntity(j, entity))) // see that this entity was not added already
                {
                    return true;
                }
            }
            return false;
        }

        private static bool SalvageJobExistsForEntity(Job j, IKnownEntityData entity)
        {
            ProcessJob p = j as ProcessJob;
            if (p != null)
            {
                if (p.SalvageJob != null)
                {
                    List<Tuple<EntityID, WorldLocation>> inputsList;
                    Dictionary<EntityType, List<Tuple<EntityID, WorldLocation>>> inputs;
                    if (p.GetAssignedInputs(out inputs))
                    {
                        if (inputs.TryGetValue(entity.EntityType, out inputsList))
                        {
                            if (inputsList.Exists(i => i.Item1 == entity.EntityID))
                            {
                                return true;
                            }
                        }
                    }

                    /*
                    if (p.InputsAssignedAndOnSite.TryGetValue(entity.EntityType, out inputs))
                    {
                        if (inputs.Exists(i => i.Item1 == entity.EntityID))
                        {
                            return true;
                        }
                    }*/
                }
            }

            return false;
        }
     
    }
}
