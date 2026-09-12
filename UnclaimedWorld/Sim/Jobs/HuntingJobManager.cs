using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems.TimeSlicing;

namespace UWGame.SimSide.Jobs
{
    public class HuntingJobManager : ICyclable
    {
        private enum Phase { CleanupJobs, RebalanceStandingOrderJobs, UpdateDirectOrderJobs, RemoveExcessiveStandingOrderJobs, UpdateStandingOrderJobs }
        private Phase phase = Phase.CleanupJobs;


        EntityGroup owner;
        EntityGroupID snapshotOwnerID;


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


        private Regulator regulator;


        public HuntingJobManager(EntityGroup owner)
        {
            
            this.owner = owner;

            AddToLookup();

            CreateRegulators();
        }

        public HuntingJobManager()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }

        void CreateRegulators()
        {
            regulator = new Regulator(The.Sim.GameplayRandomGenerator, 1d / UpdateInterval.Value, "HuntingJobManager");
        }

        public void Destroy()
        {
            The.Sim.CycleManager.UnRegister(this);

            RemoveIDEntry();
        }

        #region ICyclable Members

        public void PrintInfo(StringBuilder text)
        {
            text.Append(string.Format("HuntingJobManager {0}:", ID));

        }

        public bool CycleOnce()
        {
            switch (phase)
            {
               
                case Phase.CleanupJobs:
                   
                    CleanupJobs();
                    
                    phase = Phase.UpdateDirectOrderJobs;

                    break;
                case Phase.UpdateDirectOrderJobs:

                    UpdateDirectOrderJobs();

                    phase = Phase.UpdateStandingOrderJobs;

                    break;

                case Phase.UpdateStandingOrderJobs:
                    
                    UpdateStandingOrderJobs();

                    return true; // done.      

                    /*
                case Phase.RemoveExcessiveStandingOrderJobs:

                    RemoveUnneededStandingOrderJobs();

                    phase = Phase.RebalanceStandingOrderJobs;

                    break;

                case Phase.RebalanceStandingOrderJobs:

                 //   RebalanceStandingOrderJobs();

                    phase = Phase.CleanupJobs;

                 //   allAvailableItems.Clear();
                  
                    return true; // done.         
  */
            }

            return false;
        }

        public bool UnregisterBeforeSnapshot
        {
            get
            {
                return true;
            }
        }

        #endregion

        

        private void CleanupJobs()
        {
            List<Zone> zonesToClear = null;

            foreach (var zone in owner.Zones) // don't remove jobs inside the loop wihle iterating, the zones may be destroyed as well (crash)
            {
                List<EntityType> timepointsToReset = null;
                // reset cooldown timepoints if reached:
                foreach (var item in zone.ZoneHunt.TimePointForNextHunt)
                {
                   /* if (item.Value.HasValue
                     && The.Sim.TimepointReached(item.Value.Value))*/
                    if (The.Sim.TimepointReached(item.Value))
                    {
                        Common.AddToList(ref timepointsToReset, item.Key);
                    }
                }

                if (timepointsToReset != null)
                {
                    foreach (var item in timepointsToReset)
                    {
                        zone.ZoneHunt.ResetTimepoint(item);
                    }
                }


                bool hasJobs = zone.ZoneHunt.HasFindPreyJobs();
                if (!hasJobs)
                {
                    continue; // nothing to clean
                }


                //  * if there are no direct orders, and standing orders are not allowed, cancel any jobs in the zone.
                if (!zone.ZoneHunt.HasHuntOrders())
                {
                    Common.AddToList(ref zonesToClear, zone);
                    //zone.ZoneHunt.CancelJobs();
                    continue;                    
                }
                                

                // * Delete jobs in zones on cooldown, based on type
                // allow the jobs to stay if there are orders not on cooldown
                if (!zone.ZoneHunt.HasOrdersNotOnCooldown())
                {
                    Common.AddToList(ref zonesToClear, zone);
                    //zone.ZoneHunt.CancelJobs();
                    continue;
                }

                // delete standing order jobs which don't make sense.
                // zones which have standing order jobs only                
                if (zone.ZoneHunt.AllowsStandingOrders()
                    && !zone.ZoneHunt.HasDirectOrders()) // no direct orders
                {
                    bool hasStandingOrderWithPurpose = false;
                    foreach (var item in zone.ZoneHunt.AllowStandingOrderHunt)
                    {
                        EntityType carcassType = item.BiologicalType.CarcassType;
                        if (owner.StandingOrderJobIsNeeded(carcassType)) // OrdersExist(carcassType))
                        {
                            hasStandingOrderWithPurpose = true;
                            break;   
                        }
                    }

                    if (!hasStandingOrderWithPurpose)
                    {
                        Common.AddToList(ref zonesToClear, zone);
                        continue;

                        //zone.ZoneHunt.CancelJobs();
                    }
                }

            }   

            if (zonesToClear != null)
            {
                foreach (var item in zonesToClear)
                {
                    item.ZoneHunt.CancelJobs();
                }
            }

        }

        private void UpdateStandingOrderJobs()
        {
            if (owner.ProductionOrders == null) // critters
                return;

            /*
              loop over standing carcass orders and find the needed amount of jobs. 
             * 
             * To make it easier, each zone will have either 0 or max jobs.
             *           
             * distribute the needed jobs among all zones that allow standing orders and which have job slots
             * 
             * Never create jobs in zones that are on cooldown after unsuccessful hunt
             */
            EntityType allegianceEntityType = owner.GetAllegiance().RepresentativeEntityType;

            if (allegianceEntityType.IntelligenceType.PreyTypes == null)
                return;

            foreach (var item in allegianceEntityType.IntelligenceType.PreyTypes)
	        {	
	 
           /* foreach (var item in GameData.Instance.AllCarcassTypes)
            {*/
                EntityType carcassType = item.BiologicalType.CarcassType; //item.Value;
                EntityType creatureType = item;
                ProductionOrder order = owner.ProductionOrders.Orders[carcassType];

                int itemsInStock = owner.CountAvailableItems(carcassType);

                bool jobsAreNeeded = order.AmountToKeepInStore - itemsInStock > 0;

                if (jobsAreNeeded)
                {
                    // first check to see if hunting is going on in a zone that could provide this carcass. If so, skip to the next type.
                    // If not, find a zone that permits standing orders for that type - create maximum no. of jobs in it.
                    if (!HuntingJobsExistForCreature(creatureType))
                    {
                        Zone zoneForJobs = FindZoneToHuntCreature(creatureType);

                        if (zoneForJobs != null)
                        {
                            for (int i = 0; i < zoneForJobs.ZoneHunt.MaxHuntJobs; i++)
                            {
                                FindPreyJob job = new FindPreyJob(zoneForJobs, owner);
                            }
                        }
                    }

                }
            }
        }

        private Zone FindZoneToHuntCreature(EntityType creatureType)
        {
            foreach (var zone in owner.Zones)
            {
                if (zone.ZoneHunt.AllowStandingOrderHunt.Contains(creatureType)
                    && !zone.ZoneHunt.TimePointForNextHunt.ContainsKey(creatureType))
                {
                    return zone;
                }
            }

            return null;
        }

        private bool HuntingJobsExistForCreature(EntityType creature)
        {
            foreach (var zone in owner.Zones)
            {
                if (zone.ZoneHunt.HasAnyHuntOrders(creature) && zone.ZoneHunt.HasFindPreyJobs())
                {
                    return true;
                }
            }

            return false;
        }

       
        private void UpdateDirectOrderJobs()
        {
           // IterateCarcassOrders(UpdateDirectOrderJobs);

            /*          
             * Cleanup first: 
             * Delete surplus jobs:
             * if there are no direct orders, and standing orders are not allowed, cancel any jobs in the zone.
             * 
             * Delete jobs in zones on cooldown, based on type
             * 
             * Delete standing order jobs
             * 
             * 
             * Create jobs:
             * for each zone with direct orders, create a number of jobs equal to MaxHunters
               if there are any direct orders, always create max jobs (or max orders?).
             * 
             * next, loop over standing carcass orders and find the needed amount of jobs. 
             * one needed carcass, and 2 zones, means one zone will get a job, or both zones will get max jobs? more than one job only makes sense if there is not much else going on.
             *           
             * distribute the needed jobs among all zones that allow standing orders and which have job slots
             * if the needed amount of jobs is negative, 
             * 
             * 
             * Never create jobs in zones that are on cooldown after unsuccessful hunt
             */

            foreach (var zone in owner.Zones)
            {
                int maxJobsInZone = zone.ZoneHunt.MaxHuntJobs;

                int currentJobs = zone.ZoneHunt.FindPreyJobs.Count;

                int jobsToCreate = maxJobsInZone - currentJobs;

                if (jobsToCreate > 0)
                {
                    if (zone.ZoneHunt.HasOrdersNotOnCooldown())
                    {
                        if (zone.ZoneHunt.CreaturesToHunt != null)
                        {
                            bool canHuntCreature = false;
                            foreach (var item in zone.ZoneHunt.CreaturesToHunt)
                            {
                                if (item.Value > 0 && !zone.ZoneHunt.HuntIsOnCooldown(item.Key))
                                {
                                    canHuntCreature = true;
                                    break;
                                }
                            }

                            if (canHuntCreature)
                            {
                                for (int i = 0; i < jobsToCreate; i++)
                                {
                                    FindPreyJob job = new FindPreyJob(zone, owner);
                                }
                            }
                        }
                    }
                }
            }      

        }


        /// <summary>
        /// for each zone with direct orders, create a number of jobs equal to MaxHunters
        /// </summary>
      /*  private void UpdateDirectOrderJobs(EntityType entityType, ProductionOrder order)
        {
            if (!order.ProductionJobsToComplete.HasValue)
            {
                return;
            }
            
            int jobsToCreate = 0;

           // ProductionOrder target = owner.ProductionOrders.Orders[entityType];

            if (order.ProductionJobsToComplete.HasValue)
            {
                int howManyJobsWeWantToCreate = target.ProductionJobsToComplete.Value;

                List<Job> jobs = owner.FindPreyJobs;
             
                // amountToProduce is the amount of jobs that we need to create or remove, negative number will remove jobs positive will create.
                jobsToCreate = howManyJobsWeWantToCreate - notStartedAlreadyCreatedJobs;

                if (jobsToCreate > 0)
                {
                    //We want to create jobs 
                    CreateFixedNoOfProcessJobs(jobsToCreate, entityType);
                }
                else if (jobsToCreate < 0
                    && entityType.ItemType.CarcassType == null)
                {
                    DestroyJobsIntelligently(jobs, Math.Abs(jobsToCreate));
                }
            }
        }*/


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

      /*  private void GetAmountToProduce(EntityType entityType, ProductionOrder order, out int currentJobs, out int amountToProduce)
        {
            int itemsInStock = CountItemsInStock(entityType);

            int totalOutstandingOutput = CountOutstandingJobOutput(entityType, out currentJobs);

            amountToProduce = order.AmountToKeepInStore.Value - itemsInStock - totalOutstandingOutput;
        }*/


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

                    if (entityType.ItemType.CarcassType != null)
                    {
                        if (order.AmountToKeepInStore.HasValue)
                        {
                            RemoveUnneededStandingOrderJobs(entityType, order);
                            //UpdateStandingOrder(entityType, order);
                        }
                    }
                }

            }
        }


        private void RemoveUnneededStandingOrderJobs(EntityType entityType, ProductionOrder order)
        {
            // count complete items in stock, on site, not parts
          
            int amountToProduce;
            GetAmountToProduce(entityType, order, out amountToProduce);

            if (amountToProduce < 0)
            {
                // do this first - then do balancing/new jobs after!! the goal is to avoid job spam and process starvation
                // see if we can remove any jobs - may not be feasible because of batch production
                // RemoveExcessiveStandingOrderJobs(entityType, Math.Abs(amountToProduce));

                int amountToReduce = Math.Abs(amountToProduce);

                List<Job> jobs = owner.FindPreyJobs;
                //owner.ManagedProductionJobs.TryGetValue(entityType, out jobs); 

                DestroyJobsIntelligently(jobs, amountToReduce);
               
            }
        }

       

        private void GetAmountToProduce(EntityType entityType, ProductionOrder order, out int amountToProduce)
        {
            int itemsInStock = owner.CountAvailableItems(entityType);

            int totalOutstandingOutput = owner.FindPreyJobs.Count; // CountOutstandingJobOutput(owner, entityType, out currentJobs);

            amountToProduce = JobManager.GetAmountToProduce(order, itemsInStock, totalOutstandingOutput);
        }

        private void UpdateStandingOrder(EntityType entityType, ProductionOrder order, int maxJobs)
        {
           /* List<Job> jobs = owner.FindPreyJobs;
        

            // count complete items in stock, on site, not parts
            int currentJobs;
            int missingAmountToProduce;
            GetAmountToProduce(entityType, order, out missingAmountToProduce);

            if (currentJobs > maxJobs)
            {
                // remove any unstarted jobs:
                int jobsToRemove = currentJobs - maxJobs;

                DestroyJobsIntelligently(jobs, jobsToRemove);

            }
            else
            {
                // we can start more jobs:
                if (missingAmountToProduce > 0)
                {
                    // create the minimum amount of jobs:
                    CreateProcessJobsToMatchOutput(entityType, missingAmountToProduce, currentJobs, maxJobs - currentJobs);
                }
                else if (missingAmountToProduce < 0)
                {
                    // see if it is possible to remove some jobs while staying over the required output amount:
                    RemoveStandingOrderProcessJobsByOutputAmount(entityType, jobs, Math.Abs(missingAmountToProduce));
                }
            }*/
        }

       
        static void DestroyJobsIntelligently(List<Job> jobs, int noOfJobsToRemove)
        {
            if (jobs == null)
                return;

            int removedJobs = 0;

            // remove invalid jobs first:
           Job job;
        /*     for (int i = jobs.Count - 1; i >= 0; i--)
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
            }*/


            // pick the jobs that are furthest from completion:
            // first look at jobs not taken:
            for (int i = jobs.Count - 1; i >= 0; i--)
            {
                job = jobs[i];
                
                if (job.TakenBy.Count == 0)
                {                   
                    job.Destroy(true);
                    jobs.Remove(job); // needed?
                    //  jobs.RemoveAt(i);

                    removedJobs++;
                }

                if (removedJobs == noOfJobsToRemove)
                {
                    return;
                }
            }

            // now see if there are jobs where the taker is some distance away:
            for (int i = jobs.Count - 1; i >= 0; i--)
            {
                job = jobs[i];

               
                if (JobManager.AllTakersMinimumDistance(job) > 60f)
                {
                    job.Destroy(true);
                    jobs.Remove(job);
                    removedJobs++;
                }
                

                if (removedJobs == noOfJobsToRemove)
                {
                    return;
                }

            }

            // now unstarted jobs:
            for (int i = jobs.Count - 1; i >= 0; i--)
            {
                job = jobs[i];
                               
                job.Destroy(true);
                jobs.Remove(job);
                removedJobs++;
                
                if (removedJobs == noOfJobsToRemove)
                {
                    return;
                }
            }


            // now cancel those left:
            for (int i = jobs.Count - 1; i >= 0; i--)
            {
                job = jobs[i];
                                
                job.Destroy(true);
                jobs.Remove(job);
                removedJobs++;

                if (removedJobs == noOfJobsToRemove)
                {
                    return;
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



        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {            
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
