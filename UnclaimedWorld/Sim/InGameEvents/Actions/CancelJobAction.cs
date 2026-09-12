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
    public class CancelJobAction : EventActionType
    {
        public TargetObject TargetObject;
        public string EntityName;

        public EvalNode ProcessTypeKey;

        /// <summary>
        /// optional hardcoded job owner - default is the current owner of the job target (structure)
        /// </summary>    
        public AllegianceAndExpedition AllegianceAndExpedition;

        public CancelJobAction(string keyName): base(keyName)
        {

        }

        public CancelJobAction()           
        {

        }

        public override bool Execute(EventAction action, ref string failReason) //public bool Execute(EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget, ref string failReason)
        {
          //  Entity entity = null;
            IKnownEntityData entity = null;

            if (EntityName != null)
            {
                entity = TalkAction.GetEntityByName(EntityName);

                if (entity == null)
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

               // entity = (Entity)result[0];
                entity = (IKnownEntityData)result[0];
            }

            Allegiance allegiance;
            EntityGroup jobOwner;
            if (AllegianceAndExpedition != null)
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
            else if (entity.OwnedBy.HasValue)
            {
                IOwner owner = LookUpOwners.FindByID(entity.OwnedBy);

                if (owner != null)
                {
                    jobOwner = owner.OwnedEntities;
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

            if (entity != null)
            {
                TryCancelJob(jobOwner, process);

                return true;
            }

            return false;
        }


        private void TryCancelJob(EntityGroup owner, ProcessType processType) //string process)
        {
            List<Job> jobs = owner.OtherJobs;
            foreach (Job job in jobs)
            {
                if (job is ProcessJob)
                {
                    ProcessJob pJob = job as ProcessJob;
                    if (pJob.ProcessType.KeyName == processType.KeyName && job.ID != JobID.Invalid)
                    {
                        Command cancelJobCommand = new CancelJob(job.ID);

                        cancelJobCommand.Execute(false);

                        //The.Client.Controller.StoreAndExecuteCommand(cancelJobCommand);
                        break;
                    }
                }
            }
        }



        public override string ToString()
        {
            return "Cancelled special action: " + this.ProcessTypeKey;
        }
    }
}