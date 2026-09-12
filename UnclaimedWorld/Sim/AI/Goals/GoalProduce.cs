using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Buildings;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Items;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Maps;
namespace UWGame.SimSide.AI.Goals
{
    /// <summary>
    /// 1. Move to site. 2. Start producing.
    /// </summary>
    class GoalProduce : CompositeGoal, ITopLevelGoal
    {
        #region properties for when job does not exist

        ProcessType processType;

        EntityAndRoot? actingOnEntity;
       /* EntityID? actingOnEntity;
        EntityID? actingOnEntityRoot;
        */


        /// <summary>
        /// Replenish.
        /// for excess ammo... this should be the same compartment as is used for the weapon. Equipment for optional weapons, Haul for tools...
        /// </summary>
        StorageCompartment? replenishCompartmentToUse;

        #endregion

        /// <summary>
        /// can be null!
        /// </summary>
        ProcessJob job;
        JobID? snapshotJob;

        public OwnerID? OwnerOfProduct;


        public List<EntityID> Tools = new List<EntityID>();
               
        /// <summary>
        /// has productivity info     
        /// </summary>
        public ToolTypeCombination ToolTypeCombination;
        ToolTypeCombinationID? snapshotToolCombo;

        public double TimeSpentInTopLevelGoal { get; set; }

        /// <summary>
        /// optional immovable input item (carcass)
        /// </summary>
        EntityID? inputItem;

        Dictionary<EntityAndRoot, List<ReplenishItemsForAction>> replenishItemsForTools;

        EntityAndRoot? ActingOnEntity
        {
            get
            {
                if (job != null)
                {
                    EntityAndRoot? actingOn;
                    if (job.GetActingOnEntity(out actingOn))
                    {
                        return actingOn;
                    }

                    return null;

                }              
                else //if (actingOnEntity.HasValue)
                {
                    return actingOnEntity;
                }

                return null;
            }
        }

        ProcessType ProcessType
        {
            get
            {
                if (job != null)
                {
                    return job.ProcessType;
                }
                else return processType;
            }
        }

        public GoalProduce(Entity owner, ProcessJob job, OwnerID? ownerOfProduct, List<EntityGroupID> ownersOfVehicles, 
            List<EntityID> tools,
            Dictionary<EntityAndRoot, List<ReplenishItemsForAction>> replenishItemsForTools,             
            ToolTypeCombination toolTypeCombination,
            EntityID? inputItem) 
            : base(owner)
        {
            this.job = job;
            this.OwnerOfProduct = ownerOfProduct;
            this.ownersOfVehicles = ownersOfVehicles;

            this.Tools = tools;
            this.ToolTypeCombination = toolTypeCombination;
            this.replenishItemsForTools = replenishItemsForTools;
            this.inputItem = inputItem;
        }

        /// <summary>
        /// to be used when there is no job...
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="job"></param>
        /// <param name="ownerOfProduct"></param>
        /// <param name="ownersOfVehicles"></param>
        /// <param name="tools"></param>
        /// <param name="replenishItemsForTools"></param>
        /// <param name="toolTypeCombination"></param>
        /// <param name="inputItem"></param>
        public GoalProduce(Entity owner, ProcessType processType, EntityAndRoot? actingOnEntity, OwnerID? ownerOfProduct, List<EntityGroupID> ownersOfVehicles,
           List<EntityID> tools,
           Dictionary<EntityAndRoot, List<ReplenishItemsForAction>> replenishItemsForTools,
           ToolTypeCombination toolTypeCombination,
           EntityID? inputItem)
            : base(owner)
        {
            this.processType = processType;
            this.actingOnEntity = actingOnEntity;
            this.OwnerOfProduct = ownerOfProduct;
            this.ownersOfVehicles = ownersOfVehicles;

            this.Tools = tools;
            this.ToolTypeCombination = toolTypeCombination;
            this.replenishItemsForTools = replenishItemsForTools;
            this.inputItem = inputItem;
        }

        public GoalProduce()
        {
        }

        protected override void Activate()
        {
           
            Status = Status.Active;

            //make sure the subgoal list is clear.
            RemoveAllSubgoals();

            // wait a bit, to see if we are asked to cancel by other agents.
            AddSubgoal(new GoalWait(entity, GameData.Instance.AIConstants.TimeToWaitBeforeStartingGoal));

            IKnownEntityData inputItemData = null;
            IKnownProcess processData = null;

            // LOCKS
            if (job != null)
            {
                job.TakeJob(entity);
                SetLocksOnReplenishItems(job, replenishItemsForTools);

                SetLockOnActingOnEntity(job); // NEW: Repair, Upgrade and so on should also set a lock

                EntityID? immovableInput;
               
                if (job.GetImmovableInput(out immovableInput, out processData))
                {
                    if (immovableInput.HasValue)
                    {
                        if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(immovableInput.Value, out inputItemData)))
                        {
                            // the process / job will be destroyed and removed the next time its location is evaluated...                         
                            Status = Goals.Status.Failed;
                            return;
                        }
                    }
                }
                else
                {
                    Status = Goals.Status.Failed;
                    return;
                }                
            }


            //Add Tasks to take, in sequence:
           

            // set our stance to 'Bold' when attacking the quadite's nest
            if (job != null && job.RequiresBoldStance)
            {
                entity.Intelligence.SetBoldStance();
            }


            Vector3? location;

            processData = null;
            if (!GetWorkLocation(out location, ref processData) //!job.GetCurrentJobLocation(out location, ref siteData, ref toolData)
                || !location.HasValue)
            {               
                Status = Goals.Status.Failed;

                return;
            }

            // if we can see the entity directly, we can start the job. Won't work if tools or inputs are needed...
            if (ProcessType.WorkNeeded != WorkerNeededOptions.StartRemotely
                || !CheckIfJobTargetCanBeSeenAndStarted()) 
            {

                List<ItemType.TaskType> gearTasks = null;
                AddNightActivityGear(ref gearTasks);

                FindOptionalEquipmentIfNeeded(location.Value, job, taskTypesForGear: gearTasks);

                List<IKnownEntityData> toolsData = null;
                if (!ResolveTools(Tools, ref toolsData))
                {
                    return;
                }

                // gather tools - add pickup goals
                // Sets locks on tools as well
                if (!GatherToolsOrWeapons(toolsData, ownersOfVehicles, job, false, StorageCompartment.Haul))
                {
                    return;
                }

                AddSubgoal(new GoalMoveToPosition(entity, location.Value, ownersOfVehicles) { IsFinalDestination = true });

                PlaceStationaryToolsAtWorkSite(toolsData, location);


                // NEW: if the input item (or immovable tool perhaps?) is inside a building, we should be able to do the production inside that building. Otherwise we should unload first.
                if (inputItemData != null)
                {
                    bool carriedBySelf = false;
                    bool insideContainerWeCannotUse = false;
                    IKnownEntityData buildingWeCanEnter = null;
                    IKnownEntityData containerWeCanUnloadFrom = null;

                    bool success = GetInputInsideContainer(inputItemData, out carriedBySelf, out insideContainerWeCannotUse, out buildingWeCanEnter, out containerWeCanUnloadFrom);

                    if (success)
                    {
                        if (carriedBySelf)
                        {
                            // ??? would probably never happen.. drop the item:
                            AddSubgoal(new GoalDropItem(entity, inputItemData.EntityID));
                        }
                        else if (insideContainerWeCannotUse)
                        {
                            Status = Goals.Status.Failed;
                            return;
                        }
                        else if (buildingWeCanEnter != null)
                        {

                            // enter & retrieve:
                            // job location should be the building's accesspoint:
                            AddSubgoal(new GoalEnter(entity, inputItemData.ContainedBy.Value));

                            if (inputItemData.EntityType.IsImmovable())
                            {

                            }
                            else
                            {

                                float totalDroppedItems; // needed..? we should already have placed the tools by now.
                                JobID? thisJobID = job != null ? job.ID : (JobID?)null;
                                DropUnneededItemsToMakeCapacity(inputItemData.Bulk, (e => e.AssignedToJob != thisJobID), out totalDroppedItems, StorageCompartment.Haul);

                                // pickup:
                                AddSubgoal(new GoalPickup(entity, inputItemData.EntityID, null));

                                /// exit:
                                AddSubgoal(new GoalExit(entity));

                                // drop:
                                AddSubgoal(new GoalDropItem(entity, inputItemData.EntityID, job));  // re-assign to job so the location for the final check gets updated
                            }
                        }
                        else if (containerWeCanUnloadFrom != null)
                        {
                            // unload:
                            AddSubgoal(new GoalUnload(entity, inputItemData.ContainedBy.Value, inputItemData.EntityID, job)); // re-assign to job so the location for the final check gets updated
                        }

                        // in the open - do nothing

                    }
                    else return;
                }
                

                ReplenishToolsOrWeapons(replenishItemsForTools, ownersOfVehicles, job);

                // light fire etc.
                PrepareTools(toolsData, ownersOfVehicles);


                if (entity.HasStance())
                {
                    ChangeStance(entity.Locomotor.Stance.PickRandomProcessStance(ProcessType.StanceTypes));
                }
            }


            AddDoProduceGoal(); 
           
        }

        private void AddDoProduceGoal()
        {
            // start 
            if (job != null)
            {
                AddSubgoal(new GoalDoProduce(entity, job, OwnerOfProduct, Tools, ToolTypeCombination));
            }
            else
            {
                // prepare tool
                AddSubgoal(new GoalDoProduce(entity, processType, null, null, false, OwnerOfProduct, Tools, ToolTypeCombination, null, null, actingOnEntity));
            }
        }


        private bool SetLockOnActingOnEntity(ProcessJob job)
        {
            IKnownEntityData actingOn;
            EntityID? actingOnID = EntityAndRoot.GetEntity(ActingOnEntity);

            if (actingOnID.HasValue)
            {
                if (EntityResultCausesFailedGoal(entity.Intelligence.Allegiance.SharedKnowledge.GetKnownData(actingOnID.Value, out actingOn)))
                {
                    return false;                
                }
                else 
                {
                    actingOn.AssignedToJob = job.ID;
                }
            }

            return true;
        }

        private bool CheckIfJobTargetCanBeSeenAndStarted()
        {
             // if we can see the entity directly, we can start the job
            if (ProcessType.WorkNeeded == WorkerNeededOptions.StartRemotely)
            {
                SharedKnowledge sharedKnowledge = entityIntelligence.Allegiance.SharedKnowledge;
                Vector3? location;

                IKnownProcess processData = null;
                if (!GetWorkLocation(out location, ref processData) //!job.GetCurrentJobLocation(out location, ref siteData, ref toolData)
                    || !location.HasValue)
                {
                    return false;
                }

                Point tilePos = MapManager.WorldPosToTile(location.Value);
                if (The.Map.GetTile(tilePos).AllegiancesThatSeeThisTile.Contains(entityIntelligence.Allegiance))
                {
                    // test that we can see all entities also:
                    if (processData != null)
                    {
                       if (processData.ActingOnEntity.HasValue)
                       {
                           if (sharedKnowledge.GetKnownDataAsEntity(processData.ActingOnEntity.Value.Entity) == null)
                           {
                               return false;
                           }
                       }

                        if (processData.ImmovableTool.HasValue)
                        {
                            if (sharedKnowledge.GetKnownDataAsEntity(processData.ImmovableTool.Value) == null)
                            {
                                return false;
                            }
                        }

                        if (processData.ImmovableInput.HasValue)
                        {
                            if (sharedKnowledge.GetKnownDataAsEntity(processData.ImmovableTool.Value) == null)
                            {
                                return false;
                            }
                        }
                    }

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// handles non-job processes also
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        private bool GetWorkLocation(out Vector3? location, ref IKnownProcess processData)
        {
            location = null;
            if (actingOnEntity.HasValue)
            {
                IKnownEntityData entityData;

                entityIntelligence.Allegiance.SharedKnowledge.GetKnownData(actingOnEntity.Value.Entity, out entityData);
                if (entityData != null)
                {
                    location = entityData.AccessPoint;
                    return true;
                }
                else return false;

            }
            else
            {
                //IKnownEntityData inputData = null, toolData = null;
                return job.GetCurrentJobLocation(out location, out processData); //, ref inputData, ref toolData);
            }

        }


        public override string GetStatus()
        {
            if (ProcessType.IsSalvageProcess)
            {
                return "Salvaging";
            }
            else
            {
                return ProcessType.Name; // "Producing";
            }
        }

        protected override bool ArePreconditionsOK()
        {
          // bool toolsAreOk = true;

            if (!AreToolsOK(Tools))
                return false;

            IKnownEntityData entityData;
            if (job != null)
            {
                EntityID? containerToPlaceOutputsIn;
                if (!job.GetContainerToPlaceOutputsIn(out containerToPlaceOutputsIn)) 
                {
                    return false;
                }

                if (containerToPlaceOutputsIn.HasValue
                    && EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(containerToPlaceOutputsIn.Value, out entityData)))
                {
                    return false;
                }
            }


            if (ActingOnEntity.HasValue)
            {
               // IKnownEntityData actingOnEntityData;
               /* if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(ActingOnEntity.Value, out entityData)))
                {
                    return false;
                }*/
                if (!ActingOnEntity.Value.IsValid(entityIntelligence.Allegiance.SharedKnowledge, out entityData))
                {
                    return false;
                }
                else
                {
                    // test that the 'special' action is still available:
                    if (!entityIntelligence.Allegiance.SharedKnowledge.SpecialActionIsAvailable(entityData, ProcessType))
                    {
                        return false;
                    }
                                                          
                    if (!ProcessType.AllowProcessOnBrokenTarget())
                    {
                        //NEW: when not repairing, require a functional target
                        if (!Entity.IsFunctional(entityData))
                        {
                            return false;
                        }
                    }

                    if (ProcessType.SetPreparedProperty == true
                        && entityData.IsPrepared == true)
                    {
                        return false;
                    }
                }
            }           


            if (!IsOutputOKAndNotCompleted(job, false))
            {
                return false;
            }
                      

            return true;
        }

        bool isInRangeOfRemotelyStartedJob = false;

        protected override void ProcessWhileActive(GameTime elapsed)
        {
            //if status is inactive, call Activate()

            // preconditions! did the factory blow up, is the job already done...        
            if (//entityIntelligence.Brain.IsExecutingOpportunityGoal ||
                !preconditionsRegulator.IsReady() ||
                ArePreconditionsOK())
            {
                //process the subgoals
                Status = ProcessSubgoals(elapsed);
         
                //Added a check to see if the output was completed as we had an case that we could not reproduce
                //That occured when an job still existed with completed outputs
                //Now if we see that the output is completed we will set the status to Completed instead of failed as before
                // Lars: not sure if we should keep this..?
                // LPE: I removed this hack. during testing, find out if it can still occur.
                /*
                if (job != null)
                {
                    IKnownEntityData outputData;
                    job.GetFirstOutputEntityData(out outputData); // replace with IsCompleted and OutputExists
                    if (outputData != null)
                    {
                        //If the outputdata is completed we want to remove the job.
                        if (outputData.IsCompleted())
                        {
                            Status = Status.Completed;
                        }
                    }
                }    */   
            }
            else
            {
                Status = Goals.Status.Failed;            
            }


            if (Status == Goals.Status.Active
                && !isInRangeOfRemotelyStartedJob)
            {
                if (CheckIfJobTargetCanBeSeenAndStarted())
                {
                    isInRangeOfRemotelyStartedJob = true;
                    RemoveAllSubgoals();

                    AddDoProduceGoal();
                }
            }
            

            if (Status == Status.Completed)
            {  
                if (job != null)
                {
                   // ResetThreatStance(job); // moved to Deactivate                  
                  //  RemoveHandToolLocksFromJob(job, Tools, replenishItemsForTools);

                }
            }
            else if (Status == Status.Failed)
            {   // we don't want it anymore...
                if (job != null)
                {
                    job.Abandon(entity);
                }
            }

        }


        public double ScoreGoal()
        {            
           
            return ScoreJobGoal(job, new ToolParams() { Tools = this.Tools,
                                                              ToolProductivity = GetToolCombinationProductivity(ToolTypeCombination),
                                                              ReplenishStatus = null,
                                                              JobDurationInDays = null
            });         
        }

        public static float GetToolCombinationProductivity(ToolTypeCombination combination)
        {           
            return combination != null? combination.Productivity : 1f;
        }

        public override bool IsSame(Jobs.Job job)
        {
            return job == this.job;
        }

        public override bool HandleMessage(Message message)
        {
            //first, pass the message down the goal hierarchy
          bool handled = ForwardMessageToFrontMostSubgoal(message);

          //if the msg was not handled, test to see if this goal can handle it
          if (handled == false)
          {
              switch (message.MessageType)
              {
                  // someone wants us to stop doing this job:
                  case Message.MessageTypes.CancelJobOrItemInUse:
                  case Message.MessageTypes.CancelJobForAIReset:
                                           
                      if (job != null)
                      {
                          job.Abandon(entity);

                          Status = Status.Failed;

                          return true; //msg handled
                      }
                      else
                      {
                          return false; // this wil be the case for GoalProduce when the process type is LightFire - this has no job for some reason. We need the parent goal to cancel the job.
                      }

                     // return true; //msg handled
                                       

                  default: return false;
              }
          }
          else
          {
              return true;
          }
        }

       
        public override bool RequiresBoldStance()
        {
            return job.RequiresBoldStance;

        }

        public override void Deactivate()
        {
            if (job != null)
            {
                RemoveProcessToolLocks(job, Tools, replenishItemsForTools);


                //RemoveLocksFromJob(job, Tools, replenishItemsForTools);
                //job.Abandon(entity);

                //ReleaseLocksOnTools(Tools, job);

                //// de-assign replenish items:
                //AssignReplenishItems(job, replenishItemsForTools, false);

                //UnAssignOptionalEquipment(job);

                ResetThreatStance(job);
            }

        }



        #region ISnapshot

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public override Snapshotter.Version DoVersion(Snapshotter sn)
        {
            base.DoVersion(sn); // each class in the class hierarchy snapshots and maintains their own version.

            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }



        public override ISnapshot DoSnapshot(Snapshotter sn)
        {
            base.DoSnapshot(sn);

            this.snapshotJob = sn.SnapshotID<Job, JobID>(job);
            this.OwnerOfProduct = sn.DoEnumNullable(this.OwnerOfProduct);
            this.Tools = sn.DoList(Tools);

            if (ToolTypeCombination != null)
            {
                snapshotToolCombo = ToolTypeCombination.ID;
            }
            this.snapshotToolCombo = sn.DoEnumNullable(snapshotToolCombo);
          
            this.replenishItemsForTools = sn.DoMultiMap(replenishItemsForTools);
            this.TimeSpentInTopLevelGoal = sn.DoDouble(TimeSpentInTopLevelGoal);

            this.processType = sn.DoGameData(processType);
            this.actingOnEntity = sn.DoEntityAndRootNullable(actingOnEntity);           
            this.replenishCompartmentToUse = sn.DoEnumNullable(replenishCompartmentToUse);
            this.isInRangeOfRemotelyStartedJob = sn.DoBool(isInRangeOfRemotelyStartedJob);

            sn.Ignore(job);
            sn.Ignore(ToolTypeCombination);

            return this;
        }


        public override void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            base.LoadPostProcess(sn);

            if (snapshotJob != null)
            {
                job = (ProcessJob)LookUp<Job, JobID>.FindByID(snapshotJob);
            }
            ToolTypeCombination = LookUp<ToolTypeCombination, ToolTypeCombinationID>.FindByID(snapshotToolCombo);
        }

        #endregion
    }
}
