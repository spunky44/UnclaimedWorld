using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.InGameEvents.PropertyObjects;
using UWGame.SimSide.Processes;

namespace UWGame.SimSide.InGameEvents.Actions
{
    /// <summary>
    /// starts a new process in a scripted event, optionally completes it immediately
    /// </summary>
    public class ProcessAction : EventActionType
    {
        /// <summary>
        /// for build farm plot...
        /// </summary>
        public string ActingOnEntityName;
        public TargetObject ActingOnEntityObject;

        public string ProcessType;

        /// <summary>
        /// the farm plot finish event needs this to assign ownership
        /// </summary>
        public string WorkerName;
        public TargetObject WorkerObject;

        public bool FinishProcessImmediately;

        /// <summary>
        /// use this to supress talk by the 'worker'
        /// </summary>
        public bool SuppressSpawningEvents = true;


        public ProcessAction(string keyName): base(keyName)
        {

        }

        public ProcessAction()           
        {

        }

        public override bool Execute(EventAction action, ref string failReason) //public bool Execute(EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget, ref string failReason)
        {
                 
            Entity actingOnEntity = null;
            if (ActingOnEntityName != null || ActingOnEntityObject != null)
            {
                if (ActingOnEntityName == "Fish trap spot Saltwater 2")
                {

                }

                if (!EventActionType.GetEntity(ActingOnEntityName, ActingOnEntityObject, action, out actingOnEntity, ref failReason))
                {
                    return false;
                }
            }

            Entity worker = null;
            if (WorkerName != null || WorkerObject != null)
            {
                if (!EventActionType.GetEntity(WorkerName, WorkerObject, action, out worker, ref failReason))
                {
                    return false;
                }
            }

            EntityAndRoot? actingOn = null;
           /* EntityID? actingOnEntityID = null;
            EntityID? actingOnEntityRootID = null;*/
            if (actingOnEntity != null)
            {
                actingOn = actingOnEntity.GetAsEntityAndRoot();

               /* actingOnEntityID = actingOnEntity.ID;
                actingOnEntityRootID = actingOnEntity.GetRootEntity().ID;*/
            }

            ProcessType processType = null;
            if (ProcessType != null)
            {
                processType = GameData.Instance.AllProcessTypes[ProcessType];
            }


            SimProcess process = new SimProcess(processType, actingOn, /*actingOnEntityID, actingOnEntityRootID,*/ FinishProcessImmediately, SuppressSpawningEvents);

            if (process.Start(worker, null, null, null) == SimProcess.StatusOfProcess.Failed)
            {
                failReason = "Failed to start process";
                return false;
            }

            if (worker != null)
            {
                process.AssignWorker(worker, null);
            }

            if (FinishProcessImmediately)
            {
                process.ProduceTillCompletion(); // fires events that may kill worker or actingOnEntity... farm plots do this
            }

            DetectEntity(actingOnEntity, worker);


            return true;
        }

        public static void DetectEntity(Entity actingOnEntity, Entity worker)
        {
            // after firing all events, detect the entity that is being acted on, not needed but more logical for the player (fish trap):        
            if (worker != null && actingOnEntity != null
                && worker.ID != EntityID.Invalid && actingOnEntity.ID != EntityID.Invalid) // farm plots: when the structure is in place there is no need for the terrain anymore - it will be respawned after the farm plot is destroyed!",
            {
                Allegiance allegiance = worker.Intelligence.Allegiance;
                DetectEntity(actingOnEntity, allegiance);
            }
        }

        public static void DetectEntity(Entity actingOnEntity, Allegiance allegiance)
        {
            allegiance.SharedKnowledge.SeeDetectableIfRelevant(actingOnEntity, suppressClientFeedback: true, doAssert: false);

            Point tile = actingOnEntity.PlaySiteMapPosition;
            if (!The.Map.GetTile(tile).AllegiancesThatSeeThisTile.Contains(allegiance))
            {
                allegiance.SharedKnowledge.UnSeeEntity(actingOnEntity); // if in FOW
            }
        }

        public override string ToString()
        {
            return "Process: " + ProcessType + ", acting on: " + ActingOnEntityName;
        }

    }
}
