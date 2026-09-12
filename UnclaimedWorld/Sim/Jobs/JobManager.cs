using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Items;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Trees;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Processes;
using GameStateManagement;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Goals;
using UWGame.ClientSide.Interface;
using UWGame.Control;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Systems.TimeSlicing;
using UWGame.SimSide.Snapshots;
using UWGame.ClientSide.Interface.Inventory;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Commands;

namespace UWGame.SimSide.Jobs
{

    /// <summary>
    /// 
    /// </summary>
    public class JobManager : ICyclable
    {
        /*
         tasks:      
         * Fulfill stock orders either by creating
         * production jobs, gather jobs, (or later import jobs). 
         *       
         * All orders exists in ProductionOrders.
         * 
         * Direct orders take priority. Then standing orders
         */

        private Regulator regulator;

        EntityGroup owner;
        EntityGroupID snapshotOwnerID;

        /*
        // only expeditions use the manager... or also households?
        // per owner instead?     
        Expedition expedition;
        ExpeditionID snapshotExpedition;
        */



        /// <summary>
        /// cache for each update
        /// </summary>
        private Dictionary<EntityType, InventoryPanel.Availability> allAvailableItems = new Dictionary<EntityType, InventoryPanel.Availability>();


        private enum Phase { ImportJobs, CleanupExistingProductionJobs, RebalanceStandingOrderJobs, UpdateDirectOrderJobs, RemoveExcessiveStandingOrderJobs, ScoreImportance }
        private Phase phase = Phase.ImportJobs;


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

        public JobManager(EntityGroup owner) // Expedition expedition)
        {
            this.owner = owner;

            //this.expedition = expedition;

            AddToLookup();

            CreateRegulators();
        }

        public JobManager()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }


        public void Destroy()
        {
            The.Sim.CycleManager.UnRegister(this);

            RemoveIDEntry();
        }

        void CreateRegulators()
        {
            regulator = new Regulator(The.Sim.GameplayRandomGenerator, 1d / UpdateInterval.Value, "JobManager");
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

                    phase = Phase.ImportJobs;
                }
            }

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
            id = CyclableID.Invalid;
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

         #region ICyclable Members

        public void PrintInfo(StringBuilder text)
        {
            text.Append(string.Format("JobManager {0}:", ID));

        }


        //private enum ProductionJobTypeToAdd { Harvest, }
        public bool CycleOnce()
        {
            switch (phase)
            {
                case Phase.ImportJobs:
                    /* 
                     * TODO!!!
                     * 
                    int amountToImport = 0;
                    HaulingJobAnyItemOfType haulingJob;

                  //  List<HaulingJobAnyItemOfType> existingImportJobs;
                    List<Job> existingImportJobs;
                    int existingNoOfImportJobs = 0;

                    foreach (KeyValuePair<EntityType, List<EntityID>> kvp in expedition.OwnerContent.Items)
                    {
                        if (expedition.ImportJobs.TryGetValue(kvp.Key, out existingImportJobs))
                        {
                            existingNoOfImportJobs = existingImportJobs.Count;
                        }
                        else
                        {
                            existingNoOfImportJobs = 0;
                        }

                        amountToImport = expedition.Stocks.Targets[kvp.Key].KeepInStore - kvp.Value.Count - existingNoOfImportJobs;
                        
                        if (amountToImport > 0)
                        {                            
                            for (int i = 0; i < amountToImport; i++)
                            {                              
                                haulingJob = new HaulingJobAnyItemOfType(expedition.Center, expedition.ImportJobs[kvp.Key], expedition.PlayerSetWorkPriority, kvp.Key, null, 
                                    expedition.OwnerContent.ID); 
                               
                            }
                        }
                        else if (amountToImport < 0 && existingNoOfImportJobs > 0)
                        {
                            // remove some Import jobs:
                            // TODO: use heuristics to determine which ones to abort...

                            // overland trades should not be cancelled by this!


                        }
                       // Entity entity = Entity.FindByID(kvp.Value[0]);
                    }
                    */
                    phase = Phase.CleanupExistingProductionJobs;

                    break;
             
                case Phase.CleanupExistingProductionJobs:
                    // remove any production jobs that may have problems, then we can recreate them after this step.

                    CleanupProductionJobs();

                    phase = Phase.UpdateDirectOrderJobs;

                    break;
                case Phase.UpdateDirectOrderJobs:

                    UpdateDirectOrderJobs();

                    phase = Phase.RemoveExcessiveStandingOrderJobs;

                    break;

                case Phase.RemoveExcessiveStandingOrderJobs:

                    RemoveUnneededStandingOrderJobs();

                    phase = Phase.RebalanceStandingOrderJobs;

                    break;

                case Phase.RebalanceStandingOrderJobs:
                   
                    RebalanceStandingOrderJobs();

                    phase = Phase.ScoreImportance;

                  
                    break;

                case Phase.ScoreImportance:

                    ScoreImportance();

                    allAvailableItems.Clear();

                    phase = Phase.ImportJobs;

                    return true; // done.         

            }

            return false;
        }

        /// <summary>
        /// periodic re-scoring of output types
        /// </summary>
        private void ScoreImportance()
        {

           // HashSet<EntityType> foodUnderProduction = null;

            foreach (var item in owner.ProductionJobs)
            {
               
                EntityType entityType = item.Key;

                bool isEatable = owner.GetAllegiance().FoodExtraction.IsEatable(entityType);

                if (!isEatable)
                {
                    if (item.Value.Count > 0)
                    {
                        owner.RecomputeImportance(entityType, allAvailableItems);
                    }
                    else
                    {
                        owner.SetNeutralImportance(entityType);                      
                    }

                }
              /*  else
                {
                    Common.AddToSet(ref foodUnderProduction, entityType);
                }    */           
            }

            foreach (FindPreyJob findPreyJob in owner.FindPreyJobs)
            {
                if (findPreyJob.Zone.ZoneHunt.CreaturesToHunt != null)
                {
                    foreach (var item in findPreyJob.Zone.ZoneHunt.CreaturesToHunt)
                    {
                        if (item.Value > 0)
                        {
                            owner.RecomputeImportance(item.Key.BiologicalType.CarcassType, allAvailableItems);                          
                        }
                    }
                }
            }

          /*  if (foodUnderProduction != null)
            {
                owner.RecomputeFoodProductionImportance(foodUnderProduction, allAvailableItems);
            }*/

            owner.RecomputeFoodProductionImportance(allAvailableItems);
        }


       

        private void CreateFixedNoOfProcessJobs(int jobsToCreate, EntityType entityType) //, List<ProcessJob> existingProductionJobs)
        {
            // we will add more jobs to fill the production quota
            List<ProcessType> processesThatCanProduce = GetJobManagerProductionProcessesThatCanProduce(entityType);
            if (processesThatCanProduce == null)
            {
                return; // no processes have input / tools etc.
            }

            int jobsCreated = 0;          
            do
            {
                // for gather processes, the job assigns a free resource item when created. So compute the process/zone/item combo for each job we want to create.
                // perhaps we could cache the unassigned resource items in each zone.

                // combos for one item/job:
                List<ProcessCombo> combos = CreateProductionCombos(processesThatCanProduce);

                ProcessCombo? bestCombo = SelectBestCombo(combos);

                if (bestCombo != null)
                {
                    if (bestCombo.Value.ResourceItem != null)
                    {
                        CreateHarvestJob(owner, entityType,
                            bestCombo.Value.ProcessType, bestCombo.Value.Zone, null, bestCombo.Value.ResourceItem);
                    }
                    else
                    {
                        CreateProcessJobAndHaulingJobs(entityType, bestCombo.Value.ProcessType);
                    }

                    jobsCreated++;

                }
                else
                {
                    return;
                }
            }
            while (jobsCreated < jobsToCreate);


            /*

            ProcessType processType = SelectJobManagerProductionProcess(entityType);


            for (int i = 0; i < jobsToCreate; i++)
            {
                if (entityType.ItemType.CarcassType == null)  //is it a hunted resource? <- should not be here
                {
                    CreateProcessJobAndHaulingJobs(entityType, processType, expedition);
                }
            }*/
        }

        /*
        public void RemoveJobsThatProduceEntityType(int jobsToRemove, EntityType entityType, List<ProcessJob> existingProductionJobs)
        {
            // remove unnecessary jobs to fit the order
            // pick jobs that have not been started first
            List<ProcessJob> productionJobsToCancel = new List<ProcessJob>();

            int noOfJobsToRemove = Math.Abs(jobsToRemove);

            for (int i = 0; i < existingProductionJobs.Count; i++)
            {              
                ProcessJob processJob = existingProductionJobs[i];

                bool isStarted;
                if (processJob.IsStarted(out isStarted))
                {
                    if (!isStarted) 
                    {
                        productionJobsToCancel.Add(processJob);

                        if (productionJobsToCancel.Count == noOfJobsToRemove)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    // the job has an invalid output... let's remove it?
                    productionJobsToCancel.Add(processJob);
                    if (productionJobsToCancel.Count == noOfJobsToRemove)
                    {
                        break;
                    }
                }
            }

            // nrOfJobsToRemove = Common.Min(nrOfJobsToRemove, productionJobsToCancel.Count);
            // remove and cancel jobs
            //This function (RemoveJobsFromList) is also called for removal of harvesting jobs in GatherResourcesWindow
            //It is places in there as it needs to call this update as soon as the "Ok" button is pressed 
            //as it would break if the currently selected zone isnt selected when it tries to get the resources to cancel
            if (productionJobsToCancel.Count > 0)
            {
                if (productionJobsToCancel[0].HarvestJob == null) //If we are not a harvesting job
                {                   
                    // cancel the production jobs:
                    //  RemoveJobsFromList(productionJobsToCancel, nrOfJobsToRemove);
                    productionJobsToCancel.ForEach(j => j.Destroy(true));
                }
            }
        }*/

        private void UpdateDirectOrderJobs()
        {
            if (owner.ProductionOrders != null)
            {
                foreach (var kvp in owner.ProductionOrders.Orders)
                {
                    EntityType entityType = kvp.Key;
                    ProductionOrder order = kvp.Value;

                    if (order.ProductionJobsToComplete.HasValue)
                    {
                        UpdateDirectOrderJobs(entityType);
                    }                   
                }
            }
        }

        /// <summary>
        /// do this first, then rebalance the total jobs after
        /// </summary>
        private void RemoveUnneededStandingOrderJobs()
        {
            if (owner.ProductionOrders != null)
            {
                foreach (var kvp in owner.ProductionOrders.Orders)
                {
                    EntityType entityType = kvp.Key;
                    ProductionOrder order = kvp.Value;

                    if (order.AmountToKeepInStore.HasValue
                        && order.AmountToKeepInStore.Value != Sim.HasNoLimitValue) // ??? Test this!
                    {                        
                        RemoveUnneededStandingOrderJobs(entityType, order);                       
                    }
                }

            }
        }

        /// <summary>
        /// #GATHERFIX
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        private bool ManagerCanRemoveProductionJob(ProcessJob job)
        {
            // don't remove direct gather jobs that the player may have given in some zones.
            // these jobs are still included in the ManagedJobs collection.
            if (job.HarvestJob != null
                && job.HarvestJob.Zone.AllowStandingOrderHarvest != null)
            {
                if (job.HarvestJob.Zone.AllowStandingOrderHarvest.Contains(job.HarvestJob.ResourceType))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// create and remove jobs to match the order  
        /// the goal is to avoid job spam and process starvation
        /// </summary>
        /// <param name="entityType"></param>
        public void RebalanceStandingOrderJobs()
        {
            if (owner.ProductionOrders != null)
            {
                // each order should get at least 1 job
                // if room for more, distribute the rest of the jobs
                // don't remove jobs incessantly - never remove started jobs for example.

                int jobCapacity = GetStandingOrderJobCapacity();

               // int noOfItemsWithStandingOrders = owner.ProductionOrders.Orders.Sum(o => (o.Value.AmountToKeepInStore > 0 ? 1 : 0));
                int noOfItemsWithStandingOrders = owner.ProductionOrders.Orders.Sum(o => (o.Value.AmountToKeepInStore.HasValue && o.Value.AmountToKeepInStore.Value != 0 ? 1 : 0));

                int jobsPerItem = 0;
                if (noOfItemsWithStandingOrders > jobCapacity)
                {
                    jobsPerItem = GameData.Instance.AIConstants.MinimumJobsPerStandingOrder; // 1;
                }
                else
                {
                    jobsPerItem = (int)Math.Floor((float)jobCapacity / (float)noOfItemsWithStandingOrders);
                }


                foreach (var kvp in owner.ProductionOrders.Orders)
                {
                    EntityType entityType = kvp.Key;
                    ProductionOrder order = kvp.Value;

                    if (order.AmountToKeepInStore.HasValue)
                    {
                        UpdateStandingOrder(entityType, order, jobsPerItem);
                    }
                }
            }
        }

        /// <summary>
        /// the max job limit can be taken up by direct order jobs.
        /// only create standing order jobs if there is left over capacity from direct orders.
        /// </summary>
        /// <returns></returns>
        private int GetStandingOrderJobCapacity()
        {
            int maxJobs = EntityGroup.GetMaximumJobsBeforeWarning(owner.Parent); // expedition);

           // int totalJobs = expedition.OwnedEntities.TotalProductionJobs;

            int totalDirectOrderJobs = owner.ProductionOrders.TotalDirectOrders; // CountDirectOrderJobs();

            return maxJobs - totalDirectOrderJobs; // (totalJobs - totalDirectOrderJobs);

            //int totalStandingOrderJobs = CountStandingOrderJobs();

        }

       /* int CountDirectOrderJobs()
        {
            expedition.OwnedEntities.ProductionJobs
        }*/

        private void RemoveUnneededStandingOrderJobs(EntityType entityType, ProductionOrder order)
        {
            // count complete items in stock, on site, not parts
            int currentJobs;
            int amountToProduce;
            GetAmountToProduce(owner, entityType, order, out currentJobs, out amountToProduce);

            if (amountToProduce < 0)
            {
                // do this first - then do balancing/new jobs after!! the goal is to avoid job spam and process starvation
                // see if we can remove any jobs - may not be feasible because of batch production
               // RemoveExcessiveStandingOrderJobs(entityType, Math.Abs(amountToProduce));

                int amountToReduce = Math.Abs(amountToProduce);

                List<ProcessJob> jobs;
                owner.ManagedProductionJobs.TryGetValue(entityType, out jobs); //get the current production jobs

                RemoveStandingOrderProcessJobsByOutputAmount(entityType, jobs, amountToReduce);                
            }
        }

        private void UpdateStandingOrder(EntityType entityType, ProductionOrder order, int maxJobs)
        {
            List<ProcessJob> jobs;
            owner.ManagedProductionJobs.TryGetValue(entityType, out jobs); //get the current production jobs


            // count complete items in stock, on site, not parts
            int currentJobs;
            int missingAmountToProduce;
            GetAmountToProduce(owner, entityType, order, out currentJobs, out missingAmountToProduce);

            if (currentJobs > maxJobs)
            {
                // remove any unstarted jobs:
                int jobsToRemove = currentJobs - maxJobs;

                DestroyJobsIntelligently(jobs, jobsToRemove, 
                    j => ManagerCanRemoveProductionJob(j)  // don't allow harvest jobs in non-standing order zones to be removed
                    );
               
            }
            else
            {
                // we can start more jobs:
                if (missingAmountToProduce > 0)
                {
                    // create the minimum amount of jobs:
                   // CreateProcessJobsToMatchOutput(entityType, missingAmountToProduce, currentJobs, maxJobs - currentJobs);
                    CreateProcessJobsToMatchOutput(entityType, missingAmountToProduce, currentJobs, maxJobs);
                }
                else if (missingAmountToProduce < 0)
                {
                    // see if it is possible to remove some jobs while staying over the required output amount:
                    RemoveStandingOrderProcessJobsByOutputAmount(entityType, jobs, Math.Abs(missingAmountToProduce));
                }
            }
        }

        private static void GetAmountToProduce(EntityGroup owner, EntityType entityType, ProductionOrder order, out int currentJobs, out int amountToProduce)
        {
            int itemsInStock = owner.CountAvailableItems(entityType);

            float averageSpeed;
            int totalOutstandingOutput = owner.CountOutstandingJobOutput(entityType, true, out currentJobs, out averageSpeed);

            amountToProduce = GetAmountToProduce(order, itemsInStock, totalOutstandingOutput);
        }

        public static int GetAmountToProduce(ProductionOrder order, int itemsInStock, int totalOutstandingOutput)
        {
            int amountToProduce;

            if (order.AmountToKeepInStore.Value == Sim.HasNoLimitValue)
            {
                amountToProduce = Common.Clamp(10 - totalOutstandingOutput, 0, 10);               
            }
            else
            {
                amountToProduce = order.AmountToKeepInStore.Value - itemsInStock - totalOutstandingOutput;
            }

            return amountToProduce;
        }


        private void CreateProcessJobsToMatchOutput(EntityType entityType, int amountToProduce, int currentJobs, int maxJobs)
        {
             List<ProcessType> processesThatCanProduce = GetJobManagerProductionProcessesThatCanProduce(entityType);

             if (processesThatCanProduce == null)
                 return;

            int jobsCreated = 0;
            int outputAdded = 0;
            do
            {
                 
                // for gather processes, the job assigns a free resource item when created. So compute the process/zone/item combo for each job we want to create.
          
                // combos for one item/job:
                List<ProcessCombo> combos = CreateProductionCombos(processesThatCanProduce);

                ProcessCombo? bestCombo = SelectBestCombo(combos);

                if (bestCombo != null)
                {
                    if (bestCombo.Value.ResourceItem != null)
                    {
                        CreateHarvestJob(owner, entityType, 
                            bestCombo.Value.ProcessType, bestCombo.Value.Zone, null, bestCombo.Value.ResourceItem);
                    }
                    else 
                    {
                        CreateProcessJobAndHaulingJobs(entityType, bestCombo.Value.ProcessType);
                    }

                    jobsCreated++;

                    int? output = bestCombo.Value.ProcessType.GetOutputAmount(entityType);
                    outputAdded += output ?? 0;
                  
                }
                else
                {
                    return;
                }
            }
            while(currentJobs + jobsCreated < maxJobs 
                && outputAdded < amountToProduce);

           // ProcessType process = SelectJobManagerProductionProcess(entityType);
            /*
            if (process != null)
            {
                int? output = process.GetOutputAmount(entityType);

                if (output.HasValue)
                {
                    int jobsToCreate = amountToProduce / output.Value;

                    jobsToCreate = Common.ClampTop(jobsToCreate, maxJobs);

                    for (int i = 0; i < jobsToCreate; i++)
                    {
                        CreateProcessJobAndHaulingJobs(entityType, process, expedition);
                    }
                }
            }*/

        }

        private ProcessCombo? SelectBestCombo(List<ProcessCombo> combos)
        {
            if (combos != null && combos.Count > 0)
            {
                return combos[0];
            }

            return null;
        }

        private List<ProcessCombo> CreateProductionCombos(List<ProcessType> processesThatCanProduce)
        {
            List<ProcessCombo> combos = new List<ProcessCombo>();

            foreach (var process in processesThatCanProduce)
            {
                ProcessCombo combo;
                if (process.IsGathering)
                {
                    CreateGatherCombos(process, combos);    
                }
                else
                {
                    combo = new ProcessCombo() { ProcessType = process };
                    combos.Add(combo);
                }
            }

            return combos;
        }

        private void CreateGatherCombos(ProcessType process, List<ProcessCombo> combos)
        {           
            ResourceType resourceType = process.ResourceTypeInput;
            List<IResourceItem> items = new List<IResourceItem>();

            foreach (var zone in owner.Zones)
            {
                if (zone.AllowStandingOrderHarvest.Contains(resourceType))
                {

                    FindResourceItemsToGather(owner.GetAllegiance().SharedKnowledge, zone.MapArea, resourceType, items, 1);

                    if (items.Count > 0)
                    {
                        ProcessCombo combo = new ProcessCombo() { ProcessType = process, Zone = zone, ResourceItem = items[0] };

                        combos.Add(combo);

                        items.Clear();
                    }
                }
            }
        }


     /*   private void CreateProcessJobsToMatchOutput(EntityType entityType, int amountToProduce, int currentJobs, int maxJobs)
        {
            // create a limited no of jobs at a time to avoid job spam
            // prefer process types that can produce now
           
           ProcessType process = SelectJobManagerProductionProcess(entityType);

            if (process != null)
            {
                int? output = process.GetOutputAmount(entityType);

                if (output.HasValue)
                {
                    int jobsToCreate = amountToProduce / output.Value;

                    jobsToCreate = Common.ClampTop(jobsToCreate, maxJobs);

                    for (int i = 0; i < jobsToCreate; i++)
                    {
                        CreateProcessJobAndHaulingJobs(entityType, process, expedition);
                    }
                }
            }
         
        }*/

        /// <summary>
        /// returns the set of processes that can produce now. 
        /// For gather processes, assignment of resource items is not considered though
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        private List<ProcessType> GetJobManagerProductionProcessesThatCanProduce(EntityType entityType)
        {
            List<ProcessType> processesCanProduceNow = null;

            List<ProcessType> listOfProcesses;
            GameData.Instance.ProcessYieldsThisOutput.TryGetValue(entityType, out listOfProcesses);

            if (listOfProcesses != null)
            {

              /*  bool hasInputs;
                bool hasTools;
                int maxAmountThatCanBeProduced;
                int? noOfMissingInputTypes;
                int? noOfAvailableInputTypes;
                int? outputBatchAmount;
                bool hasSkills;
                bool hasResources;
                bool hasSpecialSite;
                bool hasPolicy;
                EntityType immovableInput;*/
               
                foreach (var processType in listOfProcesses)
                {
                    if (IsManagedProcess(processType))
                    {
                        //outputBatchAmount = processType.GetOutputAmount(entityType);

                       /* bool canProduceNow = InventoryPanel.HasAllInputsAndToolsForProcess(processType, owner, out hasInputs, out hasTools, out maxAmountThatCanBeProduced,
                            out noOfMissingInputTypes, out noOfAvailableInputTypes, out hasSkills, out hasResources, out hasSpecialSite, out hasPolicy, out immovableInput, allAvailableItems);
                        */

                        bool canProduceNow = InventoryPanel.HasAllInputsAndToolsForProcess(processType, owner, allAvailableItems);
                      

                        if (canProduceNow)
                        {
                            Common.AddToList(ref processesCanProduceNow, processType);
                        }
                    }
                }
            }

            return processesCanProduceNow;
        }

       /* private ProcessType SelectJobManagerProductionProcess(EntityType entityType)
        {
            bool hasInputs;
            bool hasTools;
            int maxAmountThatCanBeProduced;
            int? noOfMissingInputTypes;
            int? noOfAvailableInputTypes;
            int? outputBatchAmount;
            bool hasSkills;
            bool hasResources;
            bool hasSpecialSite;          
            EntityType immovableInput;
            ProcessType process;
            
            // TODO: score process types...

            // hasResources: true, even if all resources have jobs attached.
            InventoryPanel.HasProcessInputsAndToolsForProduct(
                entityType,
                expedition.OwnedEntities,
                out hasInputs,
                out hasTools,
                out maxAmountThatCanBeProduced,
                out noOfMissingInputTypes,
                out noOfAvailableInputTypes,
                out hasSkills,
                out hasResources,
                out hasSpecialSite,
                out immovableInput, // needsImmovableInput,
                out outputBatchAmount,
                out process,
                false, null, null,
                p => IsManagedProcess(p));


            return process;
        }*/

        /// <summary>
        /// gathering is included, but not hunting.
        /// </summary>
        /// <param name="processType"></param>
        /// <returns></returns>
        public static bool IsManagedProcess(ProcessType processType)
        {
            return processType.IsSalvageProcess == false 
                && processType.IsKilling == false 
                && processType.IsPseudoProcess == false; 
        }

        public static bool IsManagedProductionJob(ProcessJob processJob)
        {
            return IsManagedProcess(processJob.ProcessType);
        }

        //static int outputAmountRemoved = 0;
        private void RemoveStandingOrderProcessJobsByOutputAmount(EntityType entityType, List<ProcessJob> jobs, int outputAmountToRemove)
        {
            if (jobs == null)
                return;

           // int outputAmountRemoved = 0;

            // find a job to destroy, one at a time, prefer unstarted ones:
            int currentJobs;
            do
            {
                currentJobs = jobs.Count;

                DestroyJobsIntelligently(jobs, 1,
                   j => j.ProcessType.GetOutputAmount(entityType) <= outputAmountToRemove // only allow jobs with the correct output to be removed
                       && ManagerCanRemoveProductionJob(j), // don't touch direct order gather jobs. // #GATHERFIX2
                   j => OnRemoveProcessJob(j, entityType, ref outputAmountToRemove)); // remove one job at a time, increase the counter

            }
            while (jobs.Count != currentJobs);

        }

        private bool OnRemoveProcessJob(ProcessJob j, EntityType entityType, ref int outputAmountToRemove) // outputAmountRemoved)
        {
             //outputAmountRemoved += j.ProcessType.GetOutputAmount(entityType) ?? 0;
             outputAmountToRemove -= j.ProcessType.GetOutputAmount(entityType) ?? 0;
             return true;
        }


      /*  private void RemoveExcessiveStandingOrderJobs(EntityType entityType, int amountToReduce)
        {
            List<ProcessJob> jobs;
            expedition.OwnedEntities.ProductionJobs.TryGetValue(entityType, out jobs); //get the current production jobs

           
            bool startedJobFound = false;

            // prefer cancelling unstarted jobs
            if (!RemoveJobsUpToLimitOutput(entityType, jobs, ref amountToReduce, ref startedJobFound, false))
            {
                if (startedJobFound)
                { 
                    // now cancel any ongoing jobs:
                    RemoveJobsUpToLimitOutput(entityType, jobs, ref amountToReduce, ref startedJobFound, true);
                }
            }
        }*/


      /*  private bool RemoveJobsUpToLimitOutput(EntityType entityType, List<ProcessJob> jobs, ref int amountStillToReduce, ref bool startedJobFound, bool allowStarted)
        {
            ProcessJob processJob;
           // int amountStillToReduce = amountToReduce;

            for (int i = jobs.Count - 1; i >= 0; i--)
            {
                processJob = jobs[i];

                int output = processJob.ProcessType.GetOutputAmount(entityType) ?? 0;

                // use a greedy algo... don't go under the limit.
                if (output <= amountStillToReduce) // amountToReduce - totalRemoved)
                {
                    bool isStarted;
                    if (allowStarted)
                    {
                        processJob.Destroy(true);
                        amountStillToReduce -= output;
                    }
                    else
                    {
                        if (processJob.IsStarted(out isStarted))
                        {
                            if (!isStarted)
                            {
                                processJob.Destroy(true);
                                //totalRemoved += output;
                                amountStillToReduce -= output;
                            }
                            else
                            {
                                startedJobFound = true;
                            }
                        }
                        else
                        {
                            processJob.Destroy(true); //GoalEvaluator.HandleInvalidJob(processJob); //If the job is invalid we will remove this from the job list
                            amountStillToReduce -= output;

                            //totalRemoved += output;
                        }
                    }

                    if (amountStillToReduce <= 0) // totalRemoved >= amountToReduce)
                    {
                        return true;
                    }
                }
            }

            return false;

        }*/

       


             

        /// <summary>
        /// only manages inventory jobs, not gather jobs that have been ordered for the same products, like firewood...
        /// Direct gather jobs are never removed.
        /// </summary>
        /// <param name="entityType"></param>
        public void UpdateDirectOrderJobs(EntityType entityType)
        {
            if (entityType.Name.Contains("glassyPorridge"))
            {

            }

            int jobsToCreate = 0;

            ResourceType resourceType;
            GameData.Instance.ItemHarvestSource.TryGetValue(entityType, out resourceType);


            ProductionOrder target = owner.ProductionOrders.Orders[entityType];

            if (target.ProductionJobsToComplete.HasValue)
            {
                int howManyJobsWeWantToCreate = target.ProductionJobsToComplete.Value;

                List<ProcessJob> jobs;
            
                owner.ManagedProductionJobs.TryGetValue(entityType, out jobs); //get the current production jobs

                // the reason we only count unstarted jobs is because outputs are created in Start, and ProductionJobsToComplete orders are modified at that point
                int notStartedAlreadyCreatedJobs = CountUnstartedJobs(entityType, jobs); 

                // amountToProduce is the amount of jobs that we need to create or remove, negative number will remove jobs positive will create.
                jobsToCreate = howManyJobsWeWantToCreate - notStartedAlreadyCreatedJobs;

                if (jobsToCreate > 0)
                {
                    //We want to create jobs 
                    CreateFixedNoOfProcessJobs(jobsToCreate, entityType);
                }
                else if (jobsToCreate < 0
                    && entityType.ItemType.CarcassType == null /*If not a hunting job*/)
                {
                    DestroyJobsIntelligently(jobs, Math.Abs(jobsToCreate),
                        j => ManagerCanRemoveProductionJob(j));  //NEW: don't allow harvest jobs in non-standing order zones to be removed 

                }
            }
        }

      



       

        private int CountUnstartedJobs(EntityType entityType, List<ProcessJob> existingProductionJobs) //, out int totalOutput)
        {
            if (existingProductionJobs == null || existingProductionJobs.Count == 0)
            {
                return 0;
            }

            int unstartedProductionJobs;
            unstartedProductionJobs = existingProductionJobs.Count; //We set the unstarted jobs as the whole list.
                       
            ProcessJob processJob;
            for (int i = existingProductionJobs.Count - 1; i >= 0; i--)
            {
                processJob = existingProductionJobs[i];
               
                bool isStarted;
                if (processJob.IsStarted(out isStarted)) // the reason we only count unstarted jobs is because outputs are created in Start
                {
                    if (isStarted)
                    {
                        unstartedProductionJobs--; //If the job has started we can remove this unstartedProductionJobs counter
                    }                   
                }
                else
                {
                    GoalEvaluator.HandleInvalidJob(processJob); //If the job is invalid we will remove this from the job list
                    unstartedProductionJobs--; //And then remove it from the unstartedJobCounter
                }
            }
            
            return unstartedProductionJobs;
        }

       /* private void CancelProcessJobs(List<ProcessJob> processJobs)
        {
            ProcessJob job;
            for (int i = processJobs.Count - 1; i >= 0; i--)
            {
                job = processJobs[i];
                job.CancelAllTakersAndRemoveJob();
            }
        }*/

        /// <summary>
        /// un-assigns the materials for these process jobs
        /// 
        /// This code is duplicated in ProcessJob.Cleanup()
        /// </summary>
        /// <param name="jobs"></param>
      /*  void UnAssignAssignedJobItems(List<ProcessJob> jobs)
        {
            for (int j = 0; j < jobs.Count; j++)
            {
                List<Tuple<EntityID, WorldLocation>> assignedInputs;

              
                ProcessJob processJob = jobs[j];

                foreach (var input in processJob.ProcessType.Inputs)
                {

                    if (processJob.InputsAssignedAndOnSite.TryGetValue(input.EntityType, out assignedInputs))
                    {
                        for (int index = 0; index < assignedInputs.Count; index++)
                        {
                            IKnownEntityData entityData;
                            EntityID inputEntityID = (assignedInputs[index].Item1);

                            if (GoalEvaluator.HandleOwnerDataResult(expedition.Allegiance.SharedKnowledge,//Get the item data
                                                                    inputEntityID,
                                                                    expedition.OwnedEntities, 
                                                                    out entityData))
                            {
                                if (entityData.AssignedToJob == processJob)
                                {
                                    entityData.AssignedToJob = null;
                                }
                            }
                        }
                    }
                }
            }
        }*/

        /// <summary>
        /// remove any hauling jobs that supply materials to these process jobs
        /// 
        /// construction jobs also need to do the same when they are cancelled.
        /// </summary>
        /// <param name="jobs"></param>
        void CancelHaulingJobsForProcessJobs(List<ProcessJob> jobs)
        {
            // assume that process job and hauling job has the same owner??
            List<Job> haulingJobs = owner.HaulingJobs;

            Job haulingJob;
            for (int i = haulingJobs.Count - 1; i >= 0; i--) // reverse loop to enable removal of currently treated item
            {
                haulingJob = haulingJobs[i];
                for (int j = jobs.Count - 1; j >= 0; j--)
                {
                    if ((haulingJob as HaulingJob).RequiredByProcessJob == jobs[j])//If the item is assigned to the job we are going to cancel
                    {
                        haulingJob.Destroy(true);
                    }
                }
            }

            /*
            for (int i = 0; i < haulingJobs.Count; i++)//loops trough all hauling jobs
            {
                for (int j = 0; j < jobs.Count; j++)
                {
                    if ((The.Sim.Site.AllOwners[0].InternalOwner.OwnerContent.HaulingJobs[i] as HaulingJob).RequiredByProcessJob == jobs[j])//If the item is assigned to the job we are going to cancel
                    {
                        The.Sim.Site.AllOwners[0].InternalOwner.OwnerContent.HaulingJobs[i].CancelAllTakersAndRemoveJob();//Remove the hauling job
                    }
                }
            }*/
        }


       
        private static bool TestIsUnstarted(ProcessJob processJob, ref int removedJobs)
        {         
            bool isStarted;
            if (processJob.IsStarted(out isStarted))
            {
                if (isStarted) 
                {
                    return false;
                }
            }  

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jobs"></param>
        /// <param name="noOfJobsToRemove"></param>
        /// <param name="allowRemovalOfStartedJobs"></param>
        static public void DestroyJobsIntelligently(List<ProcessJob> jobs, int noOfJobsToRemove, 
            Predicate<ProcessJob> allowJobToBeRemoved = null,
            Predicate<ProcessJob> breakOnDestroy = null) //, bool allowRemovalOfStartedJobs)       
        {
            if (jobs == null)
                return;

            int removedJobs = 0;

            // remove invalid jobs first:
             ProcessJob processJob;
             for (int i = jobs.Count - 1; i >= 0; i--)
             {
                 processJob = jobs[i];
                 bool isStarted;
                 if (!processJob.IsStarted(out isStarted))
                 {                   
                     //the job is invalid
                     processJob.Destroy(true); //GoalEvaluator.HandleInvalidJob(processJob);
                     jobs.Remove(processJob); // needed?
                     removedJobs++;

                     if (breakOnDestroy != null && breakOnDestroy(processJob))
                     {
                         return;
                     }

                     if (removedJobs == noOfJobsToRemove)
                     {
                         return;
                     }

                     continue;
                 }
             }

            
            // pick the jobs that are furthest from completion:
            // first look at jobs not taken:
           
            for (int i = jobs.Count - 1; i >= 0; i--)
            {
                processJob = jobs[i];

                if (!TestIsUnstarted(processJob, ref removedJobs))
                {
                    continue;
                }


                if (processJob.TakenBy.Count == 0)
                {
                    if (allowJobToBeRemoved == null || allowJobToBeRemoved(processJob))
                    {
                        processJob.Destroy(true);
                        jobs.Remove(processJob); // needed?
                        //  jobs.RemoveAt(i);

                        removedJobs++;

                        if (breakOnDestroy != null && breakOnDestroy(processJob))
                        {
                            return;
                        }
                    }

                }

                if (removedJobs == noOfJobsToRemove)
                {
                    return;
                }
            }

            // now see if there are jobs where the taker is some distance away:
            for (int i = jobs.Count - 1; i >= 0; i--)
            {
                processJob = jobs[i];

                if (!TestIsUnstarted(processJob, ref removedJobs))
                {
                    continue;
                }


                /*if (processJob.HarvestJob != null) // Why only harvest?? If the job is a HarvestJob we can check the location of the agent compared to the thing to harvest
                {*/
                if (allowJobToBeRemoved == null || allowJobToBeRemoved(processJob))
                {
                    if (AllTakersMinimumDistance(processJob) > 60f)
                    {                    
                        processJob.Destroy(true);
                        jobs.Remove(processJob);
                        removedJobs++;

                        if (breakOnDestroy != null && breakOnDestroy(processJob))
                        {
                            return;
                        }
                    }
                }

                if (removedJobs == noOfJobsToRemove)
                {
                    return;
                }
               
            }

            // now unstarted jobs:
            for (int i = jobs.Count - 1; i >= 0; i--)
            {
                processJob = jobs[i];

                if (!TestIsUnstarted(processJob, /*allowRemovalOfStartedJobs,*/ ref removedJobs))
                {
                    continue;
                }

                if (allowJobToBeRemoved == null || allowJobToBeRemoved(processJob))
                {
                    processJob.Destroy(true);
                    jobs.Remove(processJob);
                    removedJobs++;
                    
                    if (breakOnDestroy != null && breakOnDestroy(processJob))
                    {
                        return;
                    }

                }

                if (removedJobs == noOfJobsToRemove)
                {
                    return;
                }
            }


            // now cancel those left:
            for (int i = jobs.Count - 1; i >= 0; i--)
            {
                processJob = jobs[i];

                if (allowJobToBeRemoved == null || allowJobToBeRemoved(processJob))
                {
                    processJob.Destroy(true);
                    jobs.Remove(processJob);
                    removedJobs++;

                    if (breakOnDestroy != null && breakOnDestroy(processJob))
                    {
                        return;
                    }
                }

                if (removedJobs == noOfJobsToRemove)
                {
                    return;
                }
            }
        }


        public static float AllTakersMinimumDistance(Job job)
        {
            float minDistance = 1000000f;
            float distance;
            Entity taker;

            Vector3? jobLocation;
            ((ProcessJob)job).GetCurrentJobLocation(out jobLocation);

            //Vector3 jobLocation = ((ProcessJob)job).HarvestJob.Item.Container.Location;
            if (jobLocation.HasValue)
            {
                for (int i = 0; i < job.TakenBy.Count; i++)
                {
                    taker = job.TakenBy.Get(i);

                    distance = Common.DistanceOctile(jobLocation.Value, taker.PlaySiteLocation);

                    if (distance < minDistance)
                    {
                        minDistance = distance;
                    }
                }
            }

            return minDistance;

        }

        public static void AddHarvestJobs(int jobDifference, EntityGroup expeditionOwner, EntityType outputType, ProcessType processType, ResourceType resourceType, Zone zone, Priority? priority)
        {
            List<IResourceItem> items = new List<IResourceItem>();

           
            // find suitable resource items:
            int noOfItemsFound = FindResourceItemsToGather(expeditionOwner.GetAllegiance().SharedKnowledge, zone.MapArea, resourceType, items, jobDifference);

            for (int i = 0; i < noOfItemsFound; i++)
            {
                IResourceItem item = items[items.Count - 1];

                CreateHarvestJob(expeditionOwner, outputType, processType, zone, priority, item);

                items.RemoveAt(items.Count - 1);

            }

        }

        private static void CreateHarvestJob(EntityGroup expeditionOwner, EntityType outputType, ProcessType processType, Zone zone, Priority? priority, IResourceItem item)
        {
            ProcessJob pJob;
            pJob = new ProcessJob(processType, outputType, expeditionOwner, item); // items[items.Count - 1]); //, priority);

            if (priority.HasValue)
            {
                pJob.Priority = priority.Value;
            }

            zone.AddHarvestJob(pJob);

            // TODO: now create hauling jobs for the inputs - provbably none:
          /*  Vector3? location;
            if (pJob.GetCurrentJobLocation(out location))
            {
                pJob.CreateHaulingJobsForProcessInputs(expeditionOwner, location.Value); // jobLocation.Value);
            }*/
        }

        private static int FindResourceItemsToGather(SharedKnowledge sharedKnowledge, MapArea area, ResourceType resourceType, List<IResourceItem> items, int noOfRequiredItems)
        {
            items.Clear();

            UWGame.SimSide.Trees.Tree treeComponent;
            Crop crop;
            TileResourceContainer container;

            //OLD: We will add all the inaccessible objects that we try to add into a list. So in the case when we try to harvest more
            //Accessible objects than existing we will pick inaccessible objects here to add to the items list. But we will always
            //prioritize Accessible items first and thats why this is done at the end of this function.
           // List<IResourceItem> unAvailableItems = new List<IResourceItem>();

            area.IterateAreaBreakOnTrue(terrainTile =>
            {
                if (resourceType.CropType != null)
                {
                    if (terrainTile.TreesOnTile != null)
                    {

                        foreach (Entity tree in terrainTile.TreesOnTile)
                        {
                            tree.Find(out treeComponent);

                            if (treeComponent.Crops != null
                                && treeComponent.Crops.TryGetValue(resourceType, out crop))
                            {
                                if (AllowGatherJobForResource(sharedKnowledge, crop))
                                {                                    
                                    foreach (var item in crop.ResourceItems)
                                    {
                                        if (item.AssignedToJob == null)
                                        {
                                            items.Add(item);
                                        }

                                        if (items.Count == noOfRequiredItems)
                                        {
                                            return true; // noOfRequiredItems;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else if (resourceType.TileResourceType != null)
                {

                    if (terrainTile.TileResources != null
                        && terrainTile.TileResources.TryGetValue(resourceType, out container))
                    {
                        if (AllowGatherJobForResource(sharedKnowledge, container))
                        {                           
                            foreach (var item in container.ResourceItems)
                            {

                                if (item.AssignedToJob == null)
                                {
                                    items.Add(item);
                                }

                                if (items.Count == noOfRequiredItems)
                                {
                                    return true; // noOfRequiredItems;
                                }
                            }                            
                        }
                    }
                }

                if (items.Count == noOfRequiredItems)
                {
                    return true; // noOfRequiredItems;
                }

                return false;

            });

           /* if (items.Count != noOfRequiredItems)
            {
                foreach (var item in unAvailableItems)
                {
                    items.Add(item);
                    if (items.Count == noOfRequiredItems)
                    {
                        break;
                    }
                }
            }*/

            return Math.Min(items.Count, noOfRequiredItems);

        }

        public static bool AllowGatherJobForResource(SharedKnowledge sharedKnowledge, ResourceContainer container)
        {
            return /*The.Sim.PlaySite.PlayerAllegiance*/
                sharedKnowledge.AllDetectedEntities.Contains(container.DetectableID) 
                && !The.Map.SubtileIsCompletelyBlocked(The.Map.TerrainCosts[SurfaceType.TransportType.Foot], MapManager.WorldPosToSubtilePos(container.AccessPoint));
        }

        /*
        private static int FindResourceItemsToGather(MapArea area, ResourceType resourceType, List<IResourceItem> items, int noOfRequiredItems)
        {
            items.Clear();

            UWGame.SimSide.Trees.Tree treeComponent;
            Crop crop;
            TileResourceContainer container;

            //We will add all the inaccessible objects that we try to add into a list. So in the case when we try to harvest more
            //Accessible objects than existing we will pick inaccessible objects here to add to the items list. But we will always
            //prioritize Accessible items first and thats why this is done at the end of this function.
            List<IResourceItem> unAvailableItems = new List<IResourceItem>();
            area.IterateAreaBreakOnTrue(terrainTile =>
            {
                if (resourceType.CropType != null)
                {
                    if (terrainTile.TreesOnTile != null)
                    {

                        foreach (Entity tree in terrainTile.TreesOnTile)
                        {
                            tree.Find(out treeComponent);

                            if (treeComponent.Crops != null
                                && treeComponent.Crops.TryGetValue(resourceType, out crop))
                            {
                                if (The.Sim.PlaySite.PlayerAllegiance.SharedKnowledge.AllDetectedEntities.Contains(crop.DetectableID) == false)
                                {
                                    continue;
                                }
                                foreach (var item in crop.ResourceItems)
                                {
                                    if (item.AssignedToJob == null)
                                    {
                                        items.Add(item);
                                    }

                                    if (items.Count == noOfRequiredItems)
                                    {
                                        return true; // noOfRequiredItems;
                                    }
                                }
                            }
                        }
                    }
                }
                else if (resourceType.TileResourceType != null)
                {

                    if (terrainTile.TileResources != null
                        && terrainTile.TileResources.TryGetValue(resourceType.KeyName, out container))
                    {
                        if (The.Map.SubtileIsCompletelyBlocked(The.Map.TerrainCosts[SurfaceType.TransportType.Foot], MapManager.WorldPosToSubtilePos(container.AccessPoint)))
                        {
                            foreach (var item in container.ResourceItems)
                            {
                                if (item.AssignedToJob == null)
                                {
                                    unAvailableItems.Add(item);
                                }
                            }
                        }
                        else if (The.Sim.PlaySite.PlayerAllegiance.SharedKnowledge.AllDetectedEntities.Contains(container.DetectableID) == true)
                        {
                            foreach (var item in container.ResourceItems)
                            {

                                if (item.AssignedToJob == null)
                                {
                                    items.Add(item);
                                }

                                if (items.Count == noOfRequiredItems)
                                {
                                    return true; // noOfRequiredItems;
                                }
                            }
                        }
                        //jobsAdded = AddHarvestJobs(expedition, container.Value) | jobsAdded;
                    }
                }

                if (items.Count == noOfRequiredItems)
                {
                    return true; // noOfRequiredItems;
                }

                return false;

            });

            if (items.Count != noOfRequiredItems)
            {
                foreach (var item in unAvailableItems)
                {
                    items.Add(item);
                    if (items.Count == noOfRequiredItems)
                    {
                        break;
                    }
                }
            }

            return Math.Min(items.Count, noOfRequiredItems);

        }*/


        struct ProcessCombo
        {
            public ProcessType ProcessType;

            public float Score;

            /// <summary>
            /// for gather
            /// </summary>
            public Zone Zone;

            /// <summary>
            /// for gather
            /// </summary>
            public IResourceItem ResourceItem;            
        }

       /* ProcessCombo SelectGatherCombo(ResourceType resource)
        {
            List<ProcessCombo> combos;

            foreach (var zone in expedition.OwnedEntities.Zones)
            {
                if (zone.HasStandingOrders.Contains(resource))
                {
                    FindResourceItemsToGather(zone.MapArea)
                    return zone;
                }
            }

            return null;
        }*/

        Zone SelectGatherZone(ResourceType resource)
        {
            foreach (var zone in owner.Zones)
            {
                if (zone.AllowStandingOrderHarvest.Contains(resource))
                {
                    return zone;
                }
            }

            return null;
        }

        /// <summary>
        /// not harvest. 
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="processType"></param>
        /// <param name="owner"></param>
        /// <returns></returns>
        private bool CreateProcessJobAndHaulingJobs(EntityType entityType, ProcessType processType)
        {
            if (processType != null)
            {
                // NEW: is it a harvested resource?       
                /* if (processType.ResourceTypeInput != null) // resourceType != null)
                {
                    CreateHarvestJob(expedition.OwnedEntities, entityType, processType, zone, null, item);

                   
                    Zone zone = SelectGatherZone(processType.ResourceTypeInput);
                    if (zone != null)
                    {
                        AddHarvestJobs(1, expedition.OwnedEntities, entityType, processType, processType.ResourceTypeInput, zone, null);
                    }
                }
                else
                {*/

                    // next we need to assign a location for the input hauling jobs:
                    Vector3? productionSiteLocation = null;
                    // Entity productionSite;

                    bool locationCanBeFound = FindProductionLocation(processType, owner.Parent, out productionSiteLocation); //, out productionSite);

                    if (locationCanBeFound)
                    {                       
                        // create the job and do the rest:
                        ProcessJob processJobToAdd = CreateProcessJob(entityType, owner, processType,
                            null,
                            productionSiteLocation);

                      //  processJobToAdd.CanCancel = false;

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
                        // else assign a location in the evaluator.
                    }
                    //  }

                    return true;
                }
           // }

            return false;
        }

              


        /// <summary>
        /// called for processes "Acting on". Not sure how much different these cases are...
        /// </summary>
        /// <param name="entityGroup"></param>
        /// <param name="actingOnEntityData"></param>
        /// <param name="processType"></param>
        /// <returns></returns>
        public static ProcessJob CreateSpecialActionJob(EntityGroup entityGroup, IKnownEntityData actingOnEntityData, ProcessType processType)
        {
            Vector3 productionLocation = actingOnEntityData.AccessPoint.Value; // set a fixed location that will not be changed.
            

            ProcessJob pJob = JobManager.CreateProcessJob(null, entityGroup,
                processType, null, productionLocation, actingOnEntityData.GetAsEntityAndRoot()); // .EntityID);


            //first, assign the action target as input, if needed (reassigned input will not be allowed.)
            Input input;
            if (processType.InputsByType != null
                && processType.InputsByType.TryGetValue(actingOnEntityData.EntityType, out input))
            {
                if (input.InputIsImmovable())
                {
                    pJob.AssignImmovableInput(actingOnEntityData);
                }
            }

            // now create hauling jobs for the inputs:
            pJob.CreateHaulingJobsForProcessInputs(entityGroup, productionLocation);

            return pJob;
        }

        /*

        /// <summary>
        /// TODO: should maybe not depend on Expedition
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="?"></param>
        /// <param name="expedition"></param>
        /// <returns></returns>
        private static bool CreateProcessJobAndHaulingJobs(EntityType entityType, Expedition expedition)
        {           
            ProcessType processType = null;
          
            
            // pick the best process if more than one exists:
            processType = GetBestProcessType(GameData.Instance.ProcessYieldsThisOutput[entityType], expedition.OwnedEntities);  //crashes if the process has not been defined, check in processloader
            

            // process type is null if we don't have the needed tools:
            if (processType != null)
            {

                // next we need to assign a location for the input hauling jobs:
                Vector3? productionSiteLocation = null;
               // Entity productionSite;

                bool locationCanBeFound = FindProductionLocation(processType, expedition, out productionSiteLocation); //, out productionSite);
                

                if (locationCanBeFound)
                {
                    // create the job and do the rest:
                    ProcessJob processJobToAdd = CreateProcessJob(entityType, expedition.OwnedEntities, processType, 
                        null,
                        productionSiteLocation);

                    processJobToAdd.CanCancel = false;

                    Vector3? jobLocation;
                    if (!processJobToAdd.GetFixedJobLocation(out jobLocation))
                    {
                        return false;
                    }
                    else if (jobLocation.HasValue)
                    {
                        // now create hauling jobs for the inputs:
                        processJobToAdd.CreateHaulingJobsForProcessInputs(expedition.OwnedEntities, jobLocation.Value);
                    }
                    // else assign a location in the evaluator.
                }

                return true;
            }

            return false;
        }
        */

        /// <summary>
        /// examines assigned inputs for each process job, either recreates hauling jobs or destroys the job if no items are available.
        /// 
        /// Perhaps we should clean up structure/special action jobs too?
        /// </summary>
        private void CleanupProductionJobs()
        {
            foreach (var kvp in owner.ProductionJobs)
            {
                EntityType entityType = kvp.Key;
                List<ProcessJob> existingProductionJobs = kvp.Value;

                // Create hauling jobs for jobs that might have gotten its input destroyed
                if (existingProductionJobs == null || existingProductionJobs.Count == 0) // If there are no production jobs to clean
                {
                    continue;
                }

                for (int i = existingProductionJobs.Count - 1; i >= 0; i--)
                {
                    ProcessJob pJob = existingProductionJobs[i];
                    /* if (pJob.HarvestJob == null) 
                     {*/
                    bool isStarted;
                    if (!pJob.IsStarted(out isStarted) || isStarted == false) //only if not started (why?)                        
                    {
                        CleanupUnstartedJob(owner, entityType, pJob);
                    }                   
                }   
            }
        }

      

        private static void CleanupUnstartedJob(EntityGroup owner, EntityType entityType, ProcessJob pJob)
        {
           
            bool hasInputs = false;
            bool hasTools = false;
            int maxAmountThatCanBeProduced;
            int? noOfMissingInputTypes, noOfAvailableInputTypes;
            bool hasSkills;
            bool hasResources;
            bool hasSpecialSite;
            bool hasPolicy;
            EntityType immovableInput;

            // let's make sure all assigned inputs are really there:
            pJob.RepairAssignedInputs(owner.GetAllegiance().SharedKnowledge);

            if (pJob.ID == JobID.Invalid)
                return;

            bool resourceIsAvailable = true;
            if (pJob.HarvestJob != null)
            {
                // NEW: also check that the gather item is still accessible:
                if (!AllowGatherJobForResource(owner.GetAllegiance().SharedKnowledge, pJob.HarvestJob.Item.Container))
                {
                    // harvest jobs will not be recreated if directly ordered. The order isn't saved. But for standing orders, a new resource item will be found, if possible.
                    resourceIsAvailable = false;                   
                }

                // for standing orders we could also clean up harvest jobs that cannot trace to the expedition center on the terrain map. But then we need to do the same test when creating the job...

            }

            if (pJob.BuildingJob == null) // don't remove player ordered structures
            {
                // see if all inputs are there - disregarding assignment etc.
                // this only makes sense for unstarted jobs.
                // currently, missing tools will not cause the job to be cleaned.
                // why call this for every job? because it checks stock target, which can be modified in this loop.
                InventoryPanel.HasAllInputsAndToolsForProcess(pJob.ProcessType, owner,
                    out hasInputs, out hasTools, out maxAmountThatCanBeProduced, out noOfMissingInputTypes, out noOfAvailableInputTypes,
                    out hasSkills, out hasResources, out hasSpecialSite, out hasPolicy, out immovableInput);


                bool cleanupJob = hasInputs == false || resourceIsAvailable == false; // destroy the jobs only when no global inputs are available...


                if (cleanupJob)
                {
                    // pJob.CancelHaulingJobsForProcess();

                    ProductionOrder order;
                    if (owner.ProductionOrders.Orders.TryGetValue(entityType, out order)
                        && order.ProductionJobsToComplete > 0)
                    {
                        order.ProductionJobsToComplete--; // it would be much cleaner if this were done in Destroy..
                    }

                    pJob.Destroy(true);

                }
            }
        }

        
        /*
        public static void CreateHaulingJobsForReplenishJobs(EntityGroup entityGroup, ProcessJob processJobToAdd, Vector3 jobLocation)
        {
            if (processJobToAdd.ProcessType.InputsByType != null)
            {               
                foreach (KeyValuePair<EntityType, Input> kvp in processJobToAdd.ProcessType.InputsByType)
                {
                    Input input = kvp.Value;

                    if (!input.InputIsImmovable())
                    {
                        int needHauling = processJobToAdd.ProcessType.InputsByType[input.EntityType].Amount.NoOfItems.Value - processJobToAdd.InputsAssignedAndOnSite[input.EntityType].Count; // TODO: Bulk items!!!
                        //  int needHauling = processJobToAdd.NeededItems[input.EntityType].NoOfItems.Value - processJobToAdd.AssignedInputsToThisJob[input.EntityType].Count; // TODO: Bulk items!!!

                        for (int i = 0; i < needHauling; i++)
                        {
                            HaulingJobAnyItemOfType haulingJob =
                                new HaulingJobAnyItemOfType(jobLocation, entityGroup, //expedition.OwnedEntities, 
                                    processJobToAdd.Priority, // use the priority of the process job..
                                    input.EntityType,
                                    entityGroup.ID, // expedition.OwnedEntities.ID, 
                                    null);


                            haulingJob.RequiredByProcessJob = processJobToAdd;

                        }
                    }
                }
            }
        }
        */

        /// <summary>
        /// no inputs??!?!
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="entityGroup"></param>
        /// <param name="processType"></param>
        /// <param name="containerToPlaceOutputsIn"></param>
        /// <param name="productionSiteLocation"></param>
        /// <param name="processActingOn"></param>
        /// <param name="isSalvage"></param>
        /// <param name="upgradeCategory"></param>
        /// <returns></returns>
        public static ProcessJob CreateProcessJob(EntityType entityType, EntityGroup entityGroup, ProcessType processType, EntityID? containerToPlaceOutputsIn, 
            Vector3? productionSiteLocation, EntityAndRoot? /* EntityID?*/ processActingOn = null, 
            bool isSalvage = false, UpgradeCategory upgradeCategory = null)
        {
           // ProcessJob processJobToAdd = new ProcessJob(null, processType, entityType, entityGroup, processActingOn);
            ProcessJob processJobToAdd = new ProcessJob(containerToPlaceOutputsIn, processType, entityType, entityGroup, processActingOn, isSalvage, upgradeCategory);
           
            
            if (productionSiteLocation.HasValue)
            {
                SimProcess process = LookUp<SimProcess, SimProcessID>.FindByID(processJobToAdd.ProductionProcess);
                process.GroundLocation = productionSiteLocation;
            }


            return processJobToAdd;
        }

        /// <summary>
        /// repair
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="entityGroup"></param>
        /// <param name="processType"></param>
        /// <param name="containerToPlaceOutputsIn"></param>
        /// <param name="productionSiteLocation"></param>
        /// <param name="processActingOn"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static ProcessJob CreateProcessJob(EntityType entityType, EntityGroup entityGroup, ProcessType processType, EntityID? containerToPlaceOutputsIn,
            Vector3? productionSiteLocation, EntityAndRoot processActingOn,
            RepairAction action, EntityAndRoot? partToFix)
        {
            ProcessJob processJobToAdd = new ProcessJob(containerToPlaceOutputsIn, processType, entityType, entityGroup, action, processActingOn, partToFix);

            if (productionSiteLocation.HasValue)
            {
                SimProcess process = LookUp<SimProcess, SimProcessID>.FindByID(processJobToAdd.ProductionProcess);
                process.GroundLocation = productionSiteLocation;
            }


            return processJobToAdd;
        }


       

     

        public static IKnownEntityData FindBestTool(List<EntityType> toolTypes, Expedition expedition)
        {
            // the purpose of this method is to find a stationary tool location to assign to the job.

            foreach (var toolType in toolTypes) // just pick the first one that matches... the situation will probably never arise where we can choose between immobile tools.
            {
                // how do we divide the jobs between the available tools??? use random for now...
                List<EntityID> items;
                if (expedition.OwnedEntities.Structures.TryGetValue(toolType, out items))
                {

                    List<EntityID> itemsCopy = new List<EntityID>(items);

                    itemsCopy = Common.Randomize(itemsCopy, The.Sim.GameplayRandomGenerator);

                    IKnownEntityData toolData;
                    foreach (var toolID in itemsCopy)
                    {
                        if (GoalEvaluator.HandleOwnerDataResult(expedition.Allegiance.SharedKnowledge, toolID, expedition.OwnedEntities, out toolData))
                        {
                            return toolData;
                        }

                    }
                }
            }

            return null;
        }

        /// <summary>
        /// finds a suitable location for this job. Returns false if we cannot find a sensible location! (this may be because the required set of inputs and tools make the job impossible)
        /// </summary>
        /// <param name="processJob"></param>
        /// <param name="location"></param>
        /// <param name="productionSite"></param>
        public static bool FindProductionLocation(ProcessType processType, IHasEntityGroup hasEntityGroup, out Vector3? location) //, out Entity productionSite)
        {
            //productionSite = null;
            location = null;         
   
            /* added a validation to process types for input combos that do not make sense, such as more than one immobile input required.
             * do the same for tools? */

            // - logs, carcasses too big to move without vehicles:
            // if the process only has one input, we can just use that input's location... we won't be creating hauling jobs for other inputs then. HOWEVER - when vehicles (exoskeletons?) are in play, we may want to haul these big items
           
            // - if any tool is a structure/immobile (campfire, smithy), use that location (if we need more than one immobile tool (work benches or machines?), the job can probably only be performed inside a workshop)
            // - else select a spot in the expedition area where no one is standing or building anything. also take zones into account when they get added.
                                  

            if (processType.ProcessToolSet != null) // &&  .ImmobileToolChoices.Count > 0)
            {
                foreach (var item in processType.ProcessToolSet.ToolTypeCombinations)
                {
                    if (item.Tools.Exists(t => ToolType.IsImmovable(t.Item1)))
                    {
                        // immobile tool type needed - the location must be set by the agent after he chooses a tool / combo!
                        return true;                         
                    }
                }
               
            }

            // does the job require inputs that we can't move (yet)?
            // (this code must be reworked once we implement workshops and vehicles...)
            if (processType.Inputs != null)
            {
                if (processType.Inputs.Length == 1)
                {
                    Input input = processType.Inputs[0];
                    if (input.InputIsImmovable()) // (is it possible to set a maximum bulk for unique bulk items..?)
                    {
                        // immobile input type found - the location must be set by the agent after he chooses an input!
                        return true;
                    }
                }
                else
                {                    
                    int noOfImmobileInputs = processType.Inputs.Count(i => i.InputIsImmovable());

                    // if there is more than one input, and one of them is immobile, we cannot do this job because hauling jobs cannot be created for the other inputs since we don't know where they need to be hauled to:
                    if (noOfImmobileInputs > 0)
                    {
                        return false;
                    }
                }
            }

            location = FindProductionLocation(hasEntityGroup);

            return true;
        }

      
        public static Vector3 FindProductionLocation(IHasEntityGroup hasEntityGroup) // Expedition expedition)
        {
            // pick a suitable location:            
            Vector3 location = EntityGroup.GetFreeGroundLocation(hasEntityGroup); //expedition.GetFreeGroundLocation();
            return location;
        }


#endregion


      /*  public static List<Job> GetProductionJobs(EntityGroup owner, EntityType key)
        {
            List<Job> jobs;
            owner.ProductionJobs.TryGetValue(key, out jobs);
            
            if (jobs == null)
            {
                jobs = new List<Job>();
                owner.ProductionJobs.Add(key, jobs);
            }          

            return jobs;

            // harvest job
           // jobToAdd = new HarvestJob(cropType, existingProductionJobs, expedition.ExpeditionOwner);
        }*/

        public bool UnregisterBeforeSnapshot
        {
            get
            {
                return true;
            }
        }

        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            // currently, this class does not time slice... so
            // don't snapshot the current progrerss snapshot the current progress...
            // after timeslicing gets implemented, this will have to be re-thought

            this.id = SnapshotID(sn, id);
            this.phase = (Phase)sn.DoEnum(phase);          
            this.IsPaused = sn.DoBool(IsPaused);

          //  this.snapshotExpedition = (ExpeditionID)sn.SnapshotID<Expedition, ExpeditionID>(expedition);
            this.snapshotOwnerID = (EntityGroupID)sn.SnapshotID<EntityGroup, EntityGroupID>(owner);

            sn.Ignore(regulator);
            sn.Ignore(totalComputationAllInstancesInSeconds);
            sn.Ignore(ComputationTimeSpentInSeconds);
            sn.Ignore(StartedOnTimeInSeconds);
            sn.Ignore(allAvailableItems);

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

          //  expedition = LookUp<Expedition, ExpeditionID>.FindByID(snapshotExpedition);
            owner = LookUp<EntityGroup, EntityGroupID>.FindByID(snapshotOwnerID);

            CreateRegulators();

        }
        
        #endregion

      
    }
}
