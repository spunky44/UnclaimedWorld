using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.ClientSide.GameEvents;
using UWGame.SimSide.InGameEvents.PropertyObjects;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Jobs;
using UWGame.Control.Commands;
using UWGame.SimSide.Commands;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities.Owners;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.InGameEvents.Expressions;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.AI;

namespace UWGame.SimSide.InGameEvents.Actions
{
   // public enum TargetEntityOfAction { SpecifiedEntity, TriggeringEntity, TargetEntity }
    public class CreateJobAction : EventActionType
    {
        public TargetObject TargetObject;                
        public string EntityName;

       // public string ProcessTypeKey;
        public EvalNode ProcessTypeKey;

        /// <summary>
        /// optional hardcoded job owner - default is the current owner of the job target (structure)
        /// </summary>    
        public AllegianceAndExpedition AllegianceAndExpedition;

        public CreateJobAction(string keyName): base(keyName)
        {

        }

        public CreateJobAction()           
        {

        }

        public override bool Execute(EventAction action, ref string failReason) //public bool Execute(EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget, ref string failReason)
        {
            IKnownEntityData entityData = null;

           // entity = GetEntity(EntityToDestroy, EntityName, triggeringEntity, targetEntity);

            if (EntityName != null)
            {
                entityData = TalkAction.GetEntityByName(EntityName);

                if (entityData == null)
                {
                    failReason = "No entity with name '" + EntityName + "' exists.";
                }
            }
            else
            {

                var result = TargetObject.GetResult(action);

                if (result.Count == 0)
                {
                    failReason = "Lookup did not give any results.";
                    return false;
                }

                entityData = (IKnownEntityData)result[0];
            }

            Allegiance allegiance;
            EntityGroup jobOwner;
            if (AllegianceAndExpedition != null) // ExpeditionKey != null)
            {
                allegiance = The.Sim.World.GetAllegianceFromKey(AllegianceAndExpedition.AllegianceKey);
                if (allegiance != null)
                {
                    Expedition expedition = allegiance.GetExpedition(AllegianceAndExpedition.ExpeditionKey);
                    if (expedition != null)
                    {
                        jobOwner = expedition.OwnedEntities;
                    }
                    else
                    {
                        failReason = "The expedition '" + AllegianceAndExpedition.ExpeditionKey + "' was not found.";
                        return false;
                    }
                }
                else
                {
                    failReason = "The allegiance '" + AllegianceAndExpedition.AllegianceKey + "' was not found.";
                    return false;
                }
              
            }
            else if (entityData.OwnedBy.HasValue)
            {
                IOwner owner = LookUpOwners.FindByID(entityData.OwnedBy);

                if (owner != null)
                {
                    jobOwner = owner.OwnedEntities;
                    allegiance = owner.Allegiance;
                }
                else
                {
                    failReason = "The entity owner was not found.";
                    return false;
                }
            }
            else
            {
                failReason = "Could not resolve a job owner.";
                return false;
            }

            ProcessType process = null;
            PropertyResult? processResult = ProcessTypeKey.Evaluate(action);

            if (processResult != null)
            {
                GameData.Instance.AllProcessTypes.TryGetValue(processResult.Value.StringResult, out process);                
            }

            if (process == null)
            {
                failReason = "Could not resolve process type.";
                return false;
            }
         
            if (entityData != null)
            {
                TryCreateActionJob(entityData, allegiance, jobOwner, process);                

                return true;
            }

            return false;
        }


        private void TryCreateActionJob(IKnownEntityData targetEntityData, Allegiance allegiance, EntityGroup owner,  ProcessType processType) //string process)
        {
          
            List<Job> jobs = owner.OtherJobs;

            if (!SpecialAction.ActionJobExists(targetEntityData, processType, jobs)) // see that this entity was not added already
            {
                // sowing is not a special action, it is a normal job...
                ProcessJob pJob = JobManager.CreateSpecialActionJob(owner, targetEntityData, processType);

                /* OLD:
                Command command = new SpecialAction(targetEntityData.EntityID, allegiance.ID, owner.ID, true, processType.KeyName); 

                command.Execute(false); // don't store/record!             
                */
            }
        }
              

        public override string ToString()
        {
            return "Create special action: " + this.ProcessTypeKey;
        }
    }
}
