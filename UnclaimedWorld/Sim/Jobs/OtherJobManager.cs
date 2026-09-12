using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Items;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Trees;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.AI;
using UWGame.SimSide.Systems.TimeSlicing;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Entities.RepairTypes;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Commands;
using UWGame.Control.Commands;
using UWGame.ClientSide.Interface;

namespace UWGame.SimSide.Jobs
{
    /// <summary>
    /// Lars: I commented out a lot of code for autonomously lighting and maintaining camp fires... perhaps we want to rework and add it later.
    /// </summary>
    public class OtherJobManager : ICyclable
    {
        /*
         tasks:
       
         * create repair jobs
         * create clearing jobs
         * create harvesting jobs
         * create mining jobs
         * 
         * create upgrade jobs
         * 
         * create salvage/rescue jobs?
         * fire fighting jobs?
         */

        private Regulator regulator;


        EntityGroup owner;
        EntityGroupID snapshotOwnerID;

        private enum Phase { CleanupJobs, StokeFireJobs, LightFireJobs, ReplenishJobs, RepairJobs, UpgradeJobs, CheckingJobs,
            RemoveExcessTakers
        }
        private Phase phase = Phase.CleanupJobs;


        public double? UpdateInterval
        {
            get
            {
                return 4d;
            }
        }


        public bool IsPaused { get; set; }
        public double StartedOnTimeInSeconds { get; set; }
        static double totalComputationAllInstancesInSeconds;
        public double TotalComputationAllInstancesInSeconds
        {
            get
            {
                return totalComputationAllInstancesInSeconds;
            }
            set
            {
                totalComputationAllInstancesInSeconds = value;
            }
        }
        public double ComputationTimeSpentInSeconds { get; set; }

        public OtherJobManager(EntityGroup owner)
        {
            
            this.owner = owner;

            AddToLookup();

            CreateRegulators();
        }

        public OtherJobManager()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }

        void CreateRegulators()
        {
            regulator = new Regulator(The.Sim.GameplayRandomGenerator, 1d / UpdateInterval.Value, "OtherJobManager");
        }

        public void Destroy()
        {
            The.Sim.CycleManager.UnRegister(this);

            RemoveIDEntry();
        }


        public void Update(GameTime gameTime)
        {
            if (!The.Sim.CycleManager.IsRegistered(this))
            {
                double milliSecondsSinceLastReady = 0;
                if (regulator.IsReady(ref milliSecondsSinceLastReady))
                {
                    /* deltaTime = milliSecondsSinceLastReady; // get the precise time since we were last here! use this value to calculate degradation.
                    
                     phase = Phase.Degrade;
                     itemCounter = 0;*/
                    The.Sim.CycleManager.Register(this, CycleManager.Priority.Medium);

                    phase = Phase.CleanupJobs;
                }
            }

        }

        private bool LightFireJobAlreadyExists(Entity fireplace)
        {
            foreach (Job job in owner.OtherJobs)
            {
                LightFireJob thisJob = job as LightFireJob;
                if (thisJob != null && thisJob.FireSite == fireplace)
                {
                    return true;
                }
            }

            return false;
        }

        private Job GetReloadJobIfExists(EntityID entity)
        {
            foreach (Job job in owner.OtherJobs)
            {
                ProcessJob thisJob = job as ProcessJob;
                if (thisJob != null && thisJob.ReplenishJob != null && thisJob.ReplenishJob.EntityToReplenish == entity
                    && thisJob.ReplenishJob.Action == GoalReplenish.ReplenishAction.Reload)
                {
                    return thisJob;
                }
            }

            return null;
        }

        private Job GetRepairJobIfExists(RepairPackageAction partRepairAction, IKnownEntityData entityData) //EntityID entity)
        {
            List<ProcessJob> jobs;
            if (owner.RepairJobs.TryGetValue(entityData.EntityID, out jobs)) 
            {
                foreach (var thisJob in jobs)
                {
                    if (thisJob.RepairJob.RepairActionToUse == partRepairAction.RepairAction)
                    {
                        return thisJob;
                    }
                }
            }

            return null;
        }

        private LightFireJob GetLightFireJobIfExists(Entity fireplace)
        {
            foreach (Job job in owner.OtherJobs)
            {
                LightFireJob thisJob = job as LightFireJob;
                if (thisJob != null && thisJob.FireSite == fireplace)
                {
                    return thisJob;
                }
            }

            return null;
        }

        private StokeFireJob GetStokeFireJobIfExists(IKnownEntityData fireplace)
        {
            foreach (Job job in owner.OtherJobs)
            {
                StokeFireJob thisJob = job as StokeFireJob;
                if (thisJob != null && thisJob.FireSite == fireplace)
                {
                    return thisJob;
                }
            }

            return null;
        }


        private bool JobTypeAlreadyExists(Type typeOfJob)
        {

            foreach (Job job in owner.OtherJobs)
            {
                if (typeOfJob.IsInstanceOfType(job))
                {
                    return true;
                }
            }

            return false;
        }


        #region ILookup

        private CyclableID id = CyclableID.Invalid;

        //=================== ILookup Methods =====================
        public CyclableID ID
        {
            get
            {
                return id;
            }

            private set
            {
                id = value;
            }
        }

        public CyclableID GetUniqueID()
        {
            return Cyclable.GetUniqueID();
        }

        public CyclableID SnapshotID(Snapshotter sn, CyclableID id)
        {
            return (CyclableID)sn.DoEnum(id);
        }


        public int LoadPostProcessOrder
        {
            get
            {
                return 0;
            }
        }


        public void AddToLookup()
        {
            ID = GetUniqueID();
            if (ID != CyclableID.Invalid)
                LookUp<ICyclable, CyclableID>.Add(ID, this);
        }

        public void RemoveIDEntry()
        {
            LookUp<ICyclable, CyclableID>.Remove(this);
        }

        public void SetInvalid()
        {
            ID = CyclableID.Invalid;
        }

        public void ResetIDCounter() // interface method - does nothing... Sim will call Cyclable.ResetIDCounter.
        {
        }

        void ILookUp<ICyclable, CyclableID>.CreateLookupCollection() // interface method - does nothing...
        {
        }

        public static void CreateLookupCollection()
        {
            LookUp<ICyclable, CyclableID>.Create();
        }

        #endregion


        private void CreateSalvageUpgradeJob(IKnownEntityData entityData, EntityGroup owner) //, ProcessType processType)
        {
            ProcessJob pJob = Salvage.CreateSalvageJob(entityData, owner);

        }

        private bool CreateUpgradeJob(IKnownEntityData entityData, ProcessType processType, UpgradeCategory upgradeCategory, EntityGroup owner)
        {
          
            if (processType != null)
            {
                ProcessJob processJob;
                if (CreateProcessJob(entityData, owner, processType, false, upgradeCategory, out processJob))
                {
                   // processJob.UpgradeJob = new UpgradeJob(upgradeCategory);                  
                    return true;
                }
            }

            return false;
        }

        /// <summary>    
        /// similar to JobManager's CreateProcessJob, this job is for replenishing entities like the sentry. Perhaps also for campfires?
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="?"></param>
        /// <param name="expedition"></param>
        /// <returns></returns>
        private bool CreateReplenishJob(IKnownEntityData entityData, EntityGroup owner) 
        {
            ProcessType processType = null;
            
            // pick the first process if more than one exists:
            processType = entityData.EntityType.ContainerType.GetReplenishProcesses().First().Value;

            // process type is null if we don't have the needed tools:
            if (processType != null)
            {
                ProcessJob processJob;
                if (CreateProcessJob(entityData, owner, processType, false, null, out processJob))
                {
                    processJob.ReplenishJob = new ReplenishJob(entityData.EntityID, GoalReplenish.ReplenishAction.Reload);
                    return true;
                }
            }

            return false;
        }

        private static bool CreateProcessJob(IKnownEntityData entityData, EntityGroup owner, ProcessType processType, bool canCancel, UpgradeCategory upgradeCategory, out ProcessJob processJobToAdd)
        {
            // next we need to assign a location for the input hauling jobs:
            Vector3? productionSiteLocation = null;
            EntityID? productionSite = null;

            if (entityData.ContainedBy.HasValue)
            {
                productionSite = entityData.ContainedBy;
            }
            else
            {
                productionSiteLocation = entityData.AccessPoint; // entityData.Location;
            }


            // create the job and do the rest:
            processJobToAdd = JobManager.CreateProcessJob(null, owner, processType,
                productionSite,
                productionSiteLocation,
                entityData.GetAsEntityAndRoot(), //.EntityID,
                upgradeCategory: upgradeCategory);

          //  processJobToAdd.CanCancel = canCancel; // false;
          

            Vector3? jobLocation;

            if (!processJobToAdd.GetFixedJobLocation(out jobLocation))
            {
                return false;
            }
            else if (jobLocation.HasValue)
            {
                // now create hauling jobs for the inputs:
                processJobToAdd.CreateHaulingJobsForProcessInputs(owner, jobLocation.Value);
            }

            return true;
        }


        private static bool CreateRepairJob(IKnownEntityData entityData, RepairPackageAction repairPart, /*RepairAction repairAction, EntityType part,*/ EntityGroup owner)
        {
            ProcessType processType = repairPart.RepairProcess; // null;

            if (processType != null)
            {
              
                // next we need to assign a location for the input hauling jobs:
                Vector3? productionSiteLocation = null;
                EntityID? productionSite = null;

                if (entityData.ContainedBy.HasValue)
                {
                    productionSite = entityData.ContainedBy;
                }
                else
                {
                    productionSiteLocation = entityData.AccessPoint; // entityData.Location;
                }


                // create the job and do the rest:
                ProcessJob processJobToAdd = JobManager.CreateProcessJob(null, owner, processType,
                    productionSite,
                    productionSiteLocation,
                    entityData.GetAsEntityAndRoot(), //EntityID, 
                    repairPart.RepairAction,
                    repairPart.Part != null ? repairPart.Part.GetAsEntityAndRoot() : (EntityAndRoot?)null);

               // processJobToAdd.CanCancel = false;
                 
                Vector3? jobLocation;

                if (!processJobToAdd.GetFixedJobLocation(out jobLocation))
                {
                    return false;
                }
                else if (jobLocation.HasValue)
                {
                    // now create hauling jobs for the inputs:
                    processJobToAdd.CreateHaulingJobsForProcessInputs(owner, jobLocation.Value);
                }

                return true;
            }

            return false;
        }

        #region ICyclable Members


        /// <summary>
        /// at night, fire is useful to keep animals away from an exposed dwelling like a camp...
        /// this is the strategic score (geography, threat level), not factoring in time of day.
        /// </summary>
        /// <returns></returns>
        private static double ScoreNeedForFireAsProtection(IKnownEntityData buildingEntity)
        {
            double result = 0;

            if (buildingEntity.EntityType.StructureType.IsCamp)
            {
                // TODO: add threat level...

                result = 1;
            }

            return result;
        }

        /*
        private static double ScoreNeedForFireForHeating(IKnownEntityData buildingEntity)
        {
            double result = 0;

            Residence residence;
            if (buildingEntity.Find(out residence))
            {
                Heating heating;
                if (!buildingEntity.Find(out heating) || heating.HeatIsAvailable == false)
                {
                    // there is no heating in this residence, we want a campfire...
                    result = 1;
                }
            }  

            return result;
        }

        private static double ScoreNeedForFireForCooking(IKnownEntityData buildingEntity)
        {
            double result = 0;

            Residence residence;
            if (buildingEntity.Find(out residence))
            {
                if (residence.Households.Count > 0)
                {
                    if (buildingEntity.EntityType.StructureType.IsCamp)
                    {                        
                        result = 1;
                    }
                }
            }

            return result;
        }

        private static bool HasResidents(IKnownEntityData buildingEntity)
        {
            Residence residence;
            if (buildingEntity.Find(out residence) && residence.Households.Count > 0)
            {
                return true;
            }

            return false;
        }
        */
        /// <summary>
        /// light the fire when it gets near dark...
        /// </summary>
        /// <returns></returns>
        /*  private static double ScoreNeedToLightFire(IKnownEntityData buildingEntity)
          {
              double result = 0;

              if (buildingEntity.IsCompleted())
              {                
                  if (HasResidents(buildingEntity))
                  {
                      double needForFireForCooking = ScoreNeedForFireForCooking(buildingEntity);
                      needForFireForCooking *= CookingJob.EstimateTimeOfDayForCooking();

                      double needForFireForProtection = ScoreNeedForFireAsProtection(buildingEntity);
                      needForFireForProtection *= Math.Pow(1f - DateAndTime.Instance.GetLightLevel(), 2); // scale by the light level, 0 = dark.

                      double needForFireForHeating = ScoreNeedForFireForHeating(buildingEntity);


                      result = 0.3 * needForFireForCooking + 0.3 * needForFireForProtection + 0.3 * needForFireForHeating;
                  }
              }

              return result;
          }
          */

        /// <summary>
        /// we want to build the fire and collect firewood while it's bright, but only light the fire when it gets near dark...
        /// </summary>
        /// <param name="buildingEntity"></param>
        /// <param name="campfireEntity"></param>
        /// <returns></returns>
        /*   public static double ScoreNeedToBuildCampfire(IKnownEntityData buildingEntity, IKnownEntityData campfireEntity)
           {
               double result = 1;

               if (buildingEntity.IsCompleted())
               {
                   if (HasResidents(buildingEntity))
                   {
                       ScoreNeedForFireForHeating(buildingEntity);

                   }
               }

               if (campfireEntity != null)
               {
                   // score need for the existing campfire

               }
               else
               {
                   // score the need to build a campfire here:


               }

               return result;
           }*/
               

        private static void RecordOutdatedJob(ref List<Job> outdatedJobs, Job job)
        {
            if (outdatedJobs != null)
            {
                outdatedJobs.Add(job);
            }
            else
            {
                outdatedJobs = new List<Job>();
                outdatedJobs.Add(job);
            }
        }

        public void PrintInfo(StringBuilder text)
        {
            text.Append(string.Format("OtherJobManager {0}:", ID));

        }

        public bool CycleOnce()
        {
            switch (phase)
            {
                case Phase.CleanupJobs:
                    {
                        // let's clean up any outdated jobs first:
                        CleanupJobs();

                        phase = Phase.StokeFireJobs;
                        break;
                    }
                // we take time of day into account when adding/removing these jobs - so agents don't need to.
                // This is factoring out calculations!
                case Phase.StokeFireJobs:
                    {
                        Allegiances.Allegiance allegiance = owner.GetAllegiance();
                        SharedKnowledge sharedKnowledge = allegiance.SharedKnowledge;


                        /*    foreach (KeyValuePair<EntityType, List<EntityID>> kvp in owner.InternalOwner.OwnerContent.Structures)
                            {
                                if (kvp.Key.StructureType.FireplaceType != null)
                                {
                                    EntityID id;
                                    IKnownEntityData structureData;
                                    for (int i = kvp.Value.Count - 1; i >= 0; i--)
                                    {
                                        id = kvp.Value[i];

                                        if (!GoalEvaluator.HandleOwnerDataResult(sharedKnowledge, id, owner, out structureData))
                                        {
                                            continue;
                                        }
                                    
                               
                                        double needForFire = ScoreNeedToBuildCampfire(fireplaceEntity.Structure.AddonTo, structureData);

                                 
                                        StokeFireJob stokeFireJob = GetStokeFireJobIfExists(structureData);

                                        if (needForFire > 0)
                                        {
                                            // only if not exist!!!
                                            if (stokeFireJob == null)
                                            {
                                                // if not already exists!!!
                                                stokeFireJob = new StokeFireJob(structureData, owner.InternalOwner.OwnerContent.Jobs);
                                            }
                                        }
                                        else
                                        {
                                            if (stokeFireJob != null)
                                            {
                                                // cancel / remove any stoke fire job - not needed:
                                                stokeFireJob.CancelAllTakersAndRemoveJob();
                                            }
                                        }
                                    }
                                }
                            }*/

                        phase = Phase.ReplenishJobs;
                        break;
                    }
                case Phase.ReplenishJobs:
                    {
                        CycleReplenish();

                        phase = Phase.CheckingJobs;
                        break;
                    }
                case Phase.CheckingJobs:
                    {
                        CycleChecking();

                        phase = Phase.RepairJobs;
                        break;
                    }
                case Phase.RepairJobs:
                    {
                        CycleRepair();

                        phase = Phase.UpgradeJobs;
                        break;
                    }
                case Phase.UpgradeJobs:
                    {
                        CycleUpgrade();

                        phase = Phase.RemoveExcessTakers;
                        break;
                    }
                case Phase.RemoveExcessTakers:
                    {
                        CycleRemoveExcessTakers();

                        phase = Phase.LightFireJobs;
                        break;
                    }
                case Phase.LightFireJobs:
                    {

                        /*   foreach (KeyValuePair<EntityType, List<EntityID>> kvp in owner.InternalOwner.OwnerContent.Structures)
                           {
                               if (kvp.Key.StructureType.FireplaceType != null)
                               {
                                   foreach (EntityID fireplaceEntity in kvp.Value)
                                   {                                    
                                       // only light a fire when we need it...

                                       double needForFire = 0;

                                       RequiresEnergy energy;
                                       fireplaceEntity.Find(out energy);

                                       if (!energy.RequiresFuel.IsBurning) // fireplaceEntity.Structure.Fireplace.IsLit)
                                       {
                                           needForFire = ScoreNeedToLightFire(fireplaceEntity.Structure.AddonTo);
                                        
                                       }

                                       LightFireJob lightFireJob = GetLightFireJobIfExists(fireplaceEntity);

                                       if (needForFire > 0)
                                       {
                                           // only if not exist!!!
                                           if (lightFireJob == null) 
                                           {
                                               // if not already exists!!!
                                               lightFireJob = new LightFireJob(fireplaceEntity, owner.InternalOwner.OwnerContent.Jobs);
                                           }
                                       }
                                       else
                                       {
                                           if (lightFireJob != null)
                                           {
                                               // cancel / remove any light fire job - not needed:
                                               lightFireJob.CancelAllTakersAndRemoveJob();
                                           }                                        
                                       }
                                   }
                               }
                           }

                           */
                        phase = Phase.CleanupJobs;
                        return true;

                    }

            }

            return false;
        }

        private void CycleReplenish()
        {
            Allegiances.Allegiance allegiance = owner.GetAllegiance();
            SharedKnowledge sharedKnowledge = allegiance.SharedKnowledge;

            EntityID entityID;
            IKnownEntityData itemToReload;
            IKnownEntityData entityData;
            foreach (KeyValuePair<EntityType, List<EntityID>> kvp in owner.AllEntities)
            {
                if (kvp.Key.RequiresOutsideReplenishment())
                {
                    for (int i = kvp.Value.Count - 1; i >= 0; i--)
                    {
                        entityID = kvp.Value[i];
                        if (!GoalEvaluator.HandleOwnerDataResult(sharedKnowledge, entityID, owner, out entityData))
                        {
                            continue;
                        }

                        if (entityData.IsCompleted()
                            && GoalEvaluator.IsOnPlaySite(entityData)
                            && Entity.IsFunctional(entityData)) // GoalEvaluator.ScoreIsEntityFunctional(entityData) > 0d)
                        {
                            // cleanup can be done by agents, but only if tools/inputs/skills are available

                            if (entityData.NeedsReload(sharedKnowledge, out itemToReload))
                            {

                                Job reloadJob = GetReloadJobIfExists(itemToReload.EntityID);

                                // only if not exist!!!
                                if (reloadJob == null)
                                {
                                    // if not already exists!!!
                                    CreateReplenishJob(itemToReload, owner);
                                }
                            }
                            else
                            {
                                /*  if (reloadJob != null)
                                  {
                                      // cancel / remove any job - not needed:
                                      reloadJob.Destroy(true);
                                  }*/
                            }
                        }
                    }
                }
            }
        }

        private static ProcessType GetUpgradeProcess(EntityType upgradeType)
        {
            List<ProcessType> processes;
            if (GameData.Instance.ProcessYieldsThisOutput.TryGetValue(upgradeType, out processes))
            {
                ProcessType processType = processes.FirstOrDefault(p => p.IsUpgrade);
                return processType;
            }

            return null;
        }

        /// <summary>
        /// examine upgrade settings, add/replace or remove upgrades as needed
        /// </summary>
        private void CycleUpgrade()
        {
            Allegiances.Allegiance allegiance = owner.GetAllegiance();
            SharedKnowledge sharedKnowledge = allegiance.SharedKnowledge;

            EntityID entityID;
            IKnownEntityData entityData;

            List<EntityID> invalidEntities = null;

            List<Job> jobs = owner.OtherJobs; // GetJobs(owner);
                        
            foreach (var item in owner.Upgrades)
            {
                entityID = item.Key;
                if (!GoalEvaluator.HandleOwnerDataResult(sharedKnowledge, entityID, owner, out entityData, ref invalidEntities))
                {
                    continue;
                }

                // add/remove upgrades:
                foreach (var upgrade in item.Value) // also empty slots
                {
                    UpgradeCategory upgradeCategory = upgrade.Key;
                    EntityType upgradeEntityType = upgrade.Value; // is null for empty slots

                    EntityID existingUpgrade;
                    if (entityData.ContainedUpgrades != null)
                    {
                        if (entityData.ContainedUpgrades.TryGetValue(upgradeCategory, out existingUpgrade))
                        {
                            IKnownEntityData existingUpgradeData;
                            if (!GoalEvaluator.HandleOwnerDataResult(sharedKnowledge, existingUpgrade, owner, out existingUpgradeData, ref invalidEntities))
                            {
                                continue; // handle this one next time.
                            }
                            else
                            {
                                // the slot already contains an upgrade. see if it is the correct type, and if it is functional?
                                // else add a salvage job
                                if (existingUpgradeData.EntityType != upgradeEntityType
                                    || !Entity.IsFunctional(existingUpgradeData))
                                {
                                    ProcessType processType = existingUpgradeData.EntityType.NonLivingType.SalvageProcessType;

                                    // only if the job does not not exist!
                                    if (!Salvage.SalvageJobExists(existingUpgradeData))
                                        //!SpecialAction.ActionJobExists(existingUpgradeData, processType, jobs)) // salvage acting on????
                                    {
                                        CreateSalvageUpgradeJob(existingUpgradeData, owner); //, existingUpgradeData);

                                        continue; // add any upgrade job after salvage is completed...
                                    }
                                }
                            }
                        }
                    }

                    // is the slot unfilled?
                    if (upgradeEntityType != null)
                    {
                        if (entityData.ContainedUpgrades == null || !entityData.ContainedUpgrades.ContainsKey(upgradeCategory))
                        {
                            // the salvage process may still be ongoing... perhaps avoid this?

                            ProcessType processType = GetUpgradeProcess(upgradeEntityType);
                            
                            // only if the job does not not exist!
                            if (!SpecialAction.ActionJobExists(entityData, processType, jobs))
                            {                                
                                if (CreateUpgradeJob(entityData, processType, upgradeCategory, owner))
                                {

                                }
                            }
                        }
                    }
                }                
            }

            // salvage upgrades:


            if (invalidEntities != null)
            {
                foreach (var item in invalidEntities)
                {
                    owner.DeleteEntity(item, null);
                }
            }

           
           // return true;
        }



        private void CycleChecking()
        {
            Allegiances.Allegiance allegiance = owner.GetAllegiance();
            SharedKnowledge sharedKnowledge = allegiance.SharedKnowledge;


            // look through the started, unattended process jobs in FOW. Get those that are scheduled to end now.
            // group them by area (quadtree of processes?)
            // create one job per area

            List<ProcessJob> jobsThatNeedChecking = null;

            // need to be sorted both when times change and when new jobs are added... just do it here each time
            owner.UnattendedProcessJobs.Sort(ProcessJob.CompareByEstimatedCompletion); // could also use isDirty pattern...

            foreach (ProcessJob item in owner.UnattendedProcessJobs)
            {
                double? completionTime = item.GetEstimatedCompletionTime();
                if (completionTime.HasValue && The.Sim.TimepointReached(completionTime.Value))
                {
                    if (!CheckingJobExists(item))
                    {
                        Common.AddToList(ref jobsThatNeedChecking, item);
                    }
                }
                else
                {
                    break;
                }
            }

            if (jobsThatNeedChecking != null)
            {
                // iterate the jobs. If there is a checking job in range, skip it. Otherwise create a new checking job.
                foreach (var processJob in jobsThatNeedChecking)
                {
                    Vector3? location;

                    if (processJob.GetCurrentJobLocation(out location) && location.HasValue)
                    {
                        List<Pair<JobID, Vector2>> checkingJobs = null;
                        owner.CheckProcessJobsQuadTree.GetEntitiesInRange(location.Value.ToVector2(), GameData.Instance.AIConstants.CheckingJobRange, null, ref checkingJobs);

                        CheckProcessJob checkProgressJob;

                        if (checkingJobs != null && checkingJobs.Count > 0)
                        {
                            // assign
                            checkProgressJob = (CheckProcessJob)LookUp<Job, JobID>.FindByID(checkingJobs[0].First);

                        }
                        else
                        {
                            checkProgressJob = new CheckProcessJob(location.Value, owner);
                        }

                        processJob.CheckingJobID = checkProgressJob.ID;
                        checkProgressJob.ProcessJobsToCheckOn.Add(processJob.ID);
                    }
                }
            }

                     
            // Cleanup. remove those checking jobs that have no process jobs in range that need checking.
            for (int i = owner.CheckProcessJobs.Count - 1; i >= 0; i--)
            {
                CheckProcessJob checkProgressJob = owner.CheckProcessJobs[i] as CheckProcessJob;

                for (int j = checkProgressJob.ProcessJobsToCheckOn.Count - 1; j >= 0; j--)
                {

                    Job job = LookUp<Job, JobID>.FindByID(checkProgressJob.ProcessJobsToCheckOn[j]);
                    if (job == null)
                    {
                        checkProgressJob.ProcessJobsToCheckOn.RemoveAt(j);
                    }                    
                }     
          
                if (checkProgressJob.ProcessJobsToCheckOn.Count == 0)
                {
                    checkProgressJob.Destroy(true);
                }                
            }

        }

        private bool CheckingJobExists(ProcessJob processJob)
        {
            if (processJob.CheckingJobID.HasValue)
            {
                Job job = LookUp<Job, JobID>.FindByID(processJob.CheckingJobID.Value);
                if (job != null)
                {
                    return true;
                }
                else
                {
                    processJob.CheckingJobID = null;
                }
            }

            return false;
        }


        private void CycleRepair()
        {
            Allegiances.Allegiance allegiance = owner.GetAllegiance();
            SharedKnowledge sharedKnowledge = allegiance.SharedKnowledge;

            EntityID entityID;
            foreach (KeyValuePair<EntityType, List<EntityID>> kvp in owner.AllEntities)
            {
                if (kvp.Key.IsRepairable())
                {
                    for (int i = kvp.Value.Count - 1; i >= 0; i--)
                    {
                        entityID = kvp.Value[i];
                        if (!GoalEvaluator.HandleOwnerDataResult(sharedKnowledge, entityID, owner, out IKnownEntityData entityData))
                        {
                            continue;
                        }

                        CreateRepairJobIfNeeded(owner, entityData);
                    }
                }
            }
        }

        public static void CreateRepairJobIfNeeded(EntityGroup owner, IKnownEntityData entityData)
        {
            if (entityData.IsCompleted()
                && GoalEvaluator.IsOnPlaySite(entityData)
                /*&& entityData.AssignedToJob == null*/) // don't repair things in use... // TODO: allow this, but let evaluator check if assigned
            {

                if (entityData.NeedsRepair()) //sharedKnowledge)) //, out action, out partToFix)) // memory fact won't change its result... so we only have to do this once
                {
                    // compile and score repair packages, select one package and make one or more jobs from it
                    // create multiple repair jobs for the same entity

                    RepairPackage repairPackage = entityData.ComputeBestRepairPackage();

                    CreateRepairJobs(owner, entityData, repairPackage);

                }
                else
                {
                    RemoveAllRepairJobs(owner, entityData);
                }
            }
        }

        /// <summary>
        /// create jobs that we need, remove any existing repair jobs that we don't need
        /// </summary>
        /// <param name="repairPackage"></param>
        private static void CreateRepairJobs(EntityGroup owner, IKnownEntityData entityData, RepairPackage repairPackage)
        {
            List<ProcessJob> jobs;
            owner.RepairJobs.TryGetValue(entityData.EntityID, out jobs);
            // remove repair jobs if not needed
            if (jobs != null)
            {
                for (int i = jobs.Count - 1; i >= 0; i--)
                {
                    ProcessJob job = jobs[i];
                    if (repairPackage == null || !repairPackage.MatchesJob(job))
                    {
                        job.Destroy(true);
                    }
                }
            }

            // add repair jobs if needed
            if (repairPackage != null)
            {
                foreach (var item in repairPackage.Actions)
                {
                    if (jobs == null || !jobs.Any(j => repairPackage.MatchesJob(j)))
                    {
                        CreateRepairJob(entityData, item, owner);
                    }

                    /*
                    Job repairJob = GetRepairJobIfExists(item, entityData); // entityData.EntityID);

                    // only if not already exists!!!
                    if (repairJob == null)
                    {
                        CreateRepairJob(entityData, action.Value, owner);
                    }*/
                }
            }
        }

        private static void RemoveAllRepairJobs(EntityGroup owner, IKnownEntityData entityData)
        {
            List<ProcessJob> jobs;
            owner.RepairJobs.TryGetValue(entityData.EntityID, out jobs);
           
            if (jobs != null)
            {
                for (int i = jobs.Count - 1; i >= 0; i--)
                {
                    ProcessJob job = jobs[i];
                    if (job.TakenBy.Count == 0)
                    {
                        // allow ongoing jobs to finish
                        job.Destroy(true);
                    }
                }
            }
        }

        /// <summary>
        /// The purpose of this is to remove jobs that refer to items which are no longer available for any reason. A sort of clean up.       
        /// 
        /// Cleanup is already done in EvaluateJob, so this is if no evaluators are running...       
        /// </summary>
        /// <returns></returns>
        private bool CleanupJobs()
        {
            Job job;
            ProcessJob processJob;
          
            Allegiances.Allegiance allegiance = owner.GetAllegiance();

            List<Job> outdatedJobs = null;

            OwnerID? ownerID = owner.GetOwnerID();           


            for (int j = 0; j < owner.OtherJobs.Count; j++)
            {
                job = owner.OtherJobs[j];

                processJob = job as ProcessJob;
                if (processJob != null)
                {
                   // EntityID? actingOnEntity;
                    EntityAndRoot? actingOnEntity;

                    if (processJob.GetActingOnEntity(out actingOnEntity) 
                        && actingOnEntity.HasValue)
                    {

                        IKnownEntityData actingOnEntityData;
                        if (!actingOnEntity.Value.IsValid(allegiance.SharedKnowledge, out actingOnEntityData))
                        {
                            Common.AddToList(ref outdatedJobs, job);
                            continue;
                        }

                        /*
                        
                        EntityResult result = allegiance.SharedKnowledge.GetKnownData(actingOnEntity.Value.Entity, out actingOnEntityData);

                        if (result == EntityResult.Destroyed || result == EntityResult.EntityStatusIsNowUnknown)
                        {
                            Common.AddToList(ref outdatedJobs, job);
                            continue;
                        }

                        if (actingOnEntityData.RootEntityID != actingOnEntity.Value.Root) // did the root change? the part was salvaged, that makes it invalid
                        {
                            Common.AddToList(ref outdatedJobs, job);
                            continue;
                        }*/
                        
                        
                        if (processJob.RequiresOwnedActingOnEntity() // farm plots are not owned...
                            && actingOnEntityData.OwnedBy != ownerID) 
                        {   // clean up discarded item jobs, even if taken:
                            Common.AddToList(ref outdatedJobs, job);
                            continue;
                        }

                        if (!allegiance.SharedKnowledge.SpecialActionIsAvailable(actingOnEntityData, processJob.ProcessType))
                        {
                            Common.AddToList(ref outdatedJobs, job);
                            continue;
                        }
                    }
                }
            }


            // clean up any outdated jobs that we found:          
            if (outdatedJobs != null)
            {
                foreach (var item in outdatedJobs)
                {
                    item.Destroy(true);
                }
            }

            return true;
        }

        /// <summary>
        /// NEW: remove takers exceeding the maximum (Patrol + Attack can now have their sliders manipulated after creating the job.)
        /// 
        /// This is also done for some jobs in the evaluators, so this for when they are not running or the calls are culled...
        /// </summary>
        private void CycleRemoveExcessTakers() //#UPDATEATTACKJOBTAKERS
        {
            for (int i = owner.VariableMaxTakerJobs.Count - 1; i >= 0; i--)
            {
                Job job = owner.VariableMaxTakerJobs[i];

                if (job.MaxJobPositions == 0)
                {
                    job.Destroy(true);
                    continue;
                }
                else if (job.TakenBy.Count > 0)
                {
                    // remove excess takers intelligently...
                    int takersToRemove = job.TakenBy.Count - job.MaxJobPositions;
                    if (takersToRemove > 0)
                    {
                        List<Entity> takersSortedByDistance = job.GetTakersSortedByDistance();
                        int messagesSent = 0;
                        foreach (var taker in takersSortedByDistance)
                        {
                            // Let's send a Cancel Job message to the agent rather than just dumping them:
                            if (taker.Intelligence.Brain.SendMessage(new Message(Message.MessageTypes.CancelJobOrItemInUse)))
                            {
                                messagesSent++;
                                if (messagesSent == takersToRemove)
                                {
                                    break;
                                }
                            }
                        }
                    }

                }
            }
         
        }

       


        public bool UnregisterBeforeSnapshot
        {
            get
            {
                return false; // TODO
            }
        }

        #endregion


        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            // TODO - this class is not currently used...

            this.id = SnapshotID(sn, id);
            this.phase = sn.DoEnum(phase);
            this.IsPaused = sn.DoBool(IsPaused);
            this.snapshotOwnerID = (EntityGroupID)sn.SnapshotID<EntityGroup, EntityGroupID>(owner);


            sn.Ignore(totalComputationAllInstancesInSeconds);
            sn.Ignore(ComputationTimeSpentInSeconds);
            sn.Ignore(StartedOnTimeInSeconds);

            sn.Ignore(regulator);

            return this;
        }

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public Snapshotter.Version DoVersion(Snapshotter sn)
        {
            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }

        public bool IsSnapshotted { get; set; }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            owner = LookUp<EntityGroup, EntityGroupID>.FindByID(snapshotOwnerID);

            CreateRegulators();
        }

        #endregion


    }
}
